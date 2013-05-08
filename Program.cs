using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace ldbuild
{
    class Program
    {
        static string m_version = "1.0.0.0";

        string m_server = "https://app.localizedirect.com";
        string m_docPath;
        string m_apiKey;
        string m_downloadPath;
        string m_buildName;


        static void Main(string[] args)
        {
            new Program().ldbuild(args);
        }

        public dynamic callApi(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "GET";
            var response = (HttpWebResponse) request.GetResponse();
            string responseText = ConvertResponseToString(response);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<Dictionary<string, dynamic>>(responseText);

            return result;
        }

        public void ldbuild(string[] args)
        {
            Console.WriteLine("Localize Direct Build Tool v" + m_version);
            if (parseArgs(args))
            {

                try
                {

                    // retrieve build id from list of document builds
                    var result = callApi(m_server + "/api/v1/" + m_docPath + "/find/build?key=" + m_apiKey);

                    if (result["success"])
                    {
                        Console.Write(".");
                        string strBuildId = null;
                        foreach (dynamic item in result["data"])
                        {
                            if (item["description"] == m_buildName)
                            {
                                strBuildId = item["buildId"];
                            }
                        }
                        if (strBuildId == null)
                        {
                            Console.WriteLine("ERROR: A build with the name '" + m_buildName + "' was not found for this document");
                            return;
                        }

                        // start the build
                        result = callApi(m_server + "/api/v1/" + m_docPath + "/start/build?key=" + m_apiKey + "&id=" + strBuildId);

                        if (result["success"])
                        {
                            Console.Write(".");
                            Boolean bDone = false;
                            string taskId = result["taskId"];
                            while (!bDone)
                            {
                                // check the build progress
                                result = callApi(m_server + "/api/v1/" + m_docPath + "/check-progress/api-task?key=" + m_apiKey + "&taskId=" + taskId);

                                if (result["success"])
                                {
                                    Console.Write(".");
                                    if (result["progress"] >= 100)
                                    {
                                        Console.WriteLine(".");
                                        bDone = true;

                                        // get the file locations
                                        result = callApi(m_server + "/api/v1/" + m_docPath + "/find/build?key=" + m_apiKey);
                                        foreach (dynamic buildDef in result["data"])
                                        {
                                            if (buildDef["description"] == m_buildName)
                                            {
                                                if (m_downloadPath == null)
                                                {
                                                    Console.WriteLine(buildDef["buildLocation"]);
                                                }
                                                else
                                                {
                                                    string strLocs = buildDef["buildLocation"];
                                                    string[] docLocs = strLocs.Split(';');
                                                    WebClient Client = new WebClient();
                                                    for (var i = 0; i < docLocs.Length; i++)
                                                    {
                                                        if (docLocs[i] != "")
                                                        {
                                                            Console.WriteLine("Downloading " + docLocs[i]);
                                                            string name = docLocs[i].Substring(docLocs[i].LastIndexOf('/') + 1);
                                                            Client.DownloadFile(docLocs[i], m_downloadPath + "\\" + name);
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: build task failed to complete.");
                                    return;
                                }

                                System.Threading.Thread.Sleep(500);
                            }
                        }
                        else
                        {
                            Console.WriteLine("ERROR: build could not start");
                        }

                    }
                    else
                    {
                        Console.WriteLine("ERROR: could read build list.");
                    }
                }
               catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }

            }

        }

        static void writeUsage()
        {
            Console.WriteLine("USAGE: ldbuild [-s <server_path>] -d <doc_path> -b <build_name> -k <api_key> [-p <download_path>]");
            Console.WriteLine("\nEXAMPLE: ldbuild -d /dom0/com128/prj337/doc361 -b \"My Build\" -k A40D86CC604766ED7200C7674AC892F6 -p \"c:\\dev\\strings\"");
        }

        public Boolean parseArgs(string[] args)
        {
            if (args.Length == 0 || args.Length % 2 == 1) {
                writeUsage();
                return false;
            }
            for (int i = 0; i < args.Length; i+=2)
            {
                if (args[i] == "-s")
                {
                    m_server = args[i + 1].Replace("\"", "");
                }
                else if (args[i] == "-d")
                {
                    m_docPath = args[i + 1].Replace("\"", "");
                }
                else if (args[i] == "-k")
                {
                    m_apiKey = args[i + 1].Replace("\"", "");
                }
                else if (args[i] == "-b")
                {
                    m_buildName = args[i + 1].Replace("\"", "");
                }
                else if (args[i] == "-p")
                {
                    m_downloadPath = args[i + 1].Replace("\"", "");
                }

            }
            if (m_server == null || m_apiKey == null || m_buildName == null)
            {
                writeUsage();
                return false;
            }

            return true;
        }

        static string ConvertResponseToString(HttpWebResponse response)
        {
            string result = ""; // "Status code: " + (int)response.StatusCode + " " + response.StatusCode + "\r\n";

            /*foreach (string key in response.Headers.Keys)
            {
                result += string.Format("{0}: {1} \r\n", key, response.Headers[key]);
            }*/

            result += "\r\n";
            result += new StreamReader(response.GetResponseStream()).ReadToEnd();

            return result;
        }

    }
}
