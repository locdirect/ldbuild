ldbuild
=======

Windows command line command to trigger LocDirect build and automatically download the build data files. For more information
on how to setup a build in LocDirect see [docs](http://docs.localizedirect.com/display/docs/Getting+build+data+into+the+game)


When you execute the ldbuild.exe you need to provide the following information:

-d <doc_path>
You can find the doc path by viewing the document's API tab. It's the datasource ID path.

-b <build_name>
This is the name you gave the build when you defined it. You can find it in the "Name" column in the document's "Build" tab.

-k <api_key>
Create an API key with minimum access right of "Read, Edit, Add" in the document's API tab.

-p <download_path>
Folder path on your local machine.
