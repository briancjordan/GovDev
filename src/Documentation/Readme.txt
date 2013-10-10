GovDev for TFS 1.0 READ ME
==========================================

Prequisite: TFS 2010, SP 1

Installation instructions for the GovDev for TFS 1.0 process template
-------------------------------------------------------------------------------------

Last updated: 2012-06-08

The installation of the GovDev for TFS 2010 v1.0 process template consists of one manadatory part and two optional but recommended parts 
depending on your installation.  Please read all of the instruction below prior to performing the installation.

1.  (Mandatory) Upload of the process template files located in the Process Template diectory of the installation.  
    Installation instructions are below.

2.  (Optional) Installation of the SharePoint sollution package containing customizations for the SharePoint team project portal.  This
    is only necessary if you have one installed and configured SharePoint Services or SharePoint Servers to work with TFS.
	Installation instruction are located in the ReadMe.txt file located in the Project Portal directory of the installation.

3.  (Optional) Installation of the process guidance files located in the Process Guidance directory of the installation.  NOTE: If you
    do wish to make use of the process guidance, you will need to update some html files as described below in step 0 for "Uploading
	the Process Template."

A.  UPLOADING THE PROCESS TEMPLATE

0.  If you want to make use of the Process Guidance and have it properly linked with SharePoint and the Team Web Access, you will need to 
    update the meta refresh URLs located in the HTM files of the Windows SharePoint Services -> Process Guidance -> Supporintg Files directory
	of the Process Template installation directory.  Each file contains a meta refresh tag of the form:

		<meta http-equiv="refresh" content="0;URL=http://<ServerPath>/html/<WorkItems File>.html" >

	You will need to replace the "<ServerPath>" token with the server name and full path of the virtual directory that is hosting the content.  See
	INSTALL THE PROCESS GUIDANCE below.  Save each file after updating the <ServerPath> token.

1.  Open Visual Studio. 

2.  On the Team menu, point to Team Project Collection Settings, and then click Process Template Manager. The Process Template Manager 
    lists each process template that has been uploaded to the team project collection. 

3.  Click Upload.

4.  In the Upload Process Template dialog box, click the folder that contains the root file, ProcessTemplate.xml. This is in the Process Template folder.

5.  Click Upload. ProcessTemplate.xml and all its surrounding folders and files are uploaded. The uploaded process template appears in the list of available process templates.

6.  Click Close to close the Process Template Manager.

See http://msdn.microsoft.com/en-us/library/ms181512(v=VS.100).aspx for more details.


B.  UPLOADING THE SHAREPOINT SOLUTION

1.  Change directory to Project Portal directory of the installation.

2.  Follow the instructions located in the ReadMe.txt file.


C.  INSTALL THE PROCESS GUIDANCE

1.  The Process Guidance consists of a set of static html pages and their associated image, script and css files aranged 
    in a directory structure for ensuring that the html pages are rendered correctly when deployed to a web server.

2.  To install the process documentation, simply create a virtual directory in IIS that points to the location on disk where the 
     content is stored.  See http://technet.microsoft.com/en-us/library/cc771804(v=WS.10).aspx for more details.

3.  In the root of the content directory for the process guidance are located two index files--index.htm and index.html.  Each file contains a meta refresh tag of the form:

		<meta http-equiv="refresh" content="0;URL=http://<ServerPath>/html/GovDevv1.0.html" >

	You will need to update the <ServerPath> token and replace it with the server name and full path of the virtual directory.

4.  To ensure that the SharePoint Process Guidance Support files and the Team Web Access Work Item help files are rendered correctly,
    see STEP O under UPLOADING THE PROCESS TEMPLATE above.

