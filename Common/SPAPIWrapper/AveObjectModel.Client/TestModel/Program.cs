/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.Wrapper.Common;
using System.Xml;
using System.IO;
using System.Web.UI.WebControls.WebParts;
using System.Net;
using System.Security;
using System.Threading;
//using AvePoint.ObjectModel.WebService;
//using Microsoft.SharePoint.Client;
//using System.Web.Script.Serialization;
using AvePoint.ObjectModel.WebService.Lists;
using System.Web.Services.Protocols;
using AvePoint.GCommon.Utility;
using TestModel;
using System.Globalization;
using AvePoint.GCommon;
//using AvePoint.ObjectModel.Common.WebPart;


namespace TestModeWebService
{
    class AveHttpWebRequest : WebRequest
    {
        public void SetKeepAlive()
        {
            //KeepAlive = false;
        }
    }

    class Program
    {
        //[DllImport("kernel32.dll")]
        private static CookieContainer mCookieContainer;

        static void Main(string[] args)
        {
            new UserCustomActionTest();
            var factory=AveObjectModelFactory.CreateObjectModelFactory("https://m365x761482.sharepoint.com/sites/tony_highspeed001",
                new AveBPOSAccountInfo
                { UserName = "wbhu@m365x761482.onmicrosoft.com",
                    Password = "demo12!@",
                    AdminUrl = "https://m365x761482-admin.sharepoint.com",
                    ConnectionType = BposConnectionType.ServiceAccount
                });
           var site= factory.CreateSite();
            var web=site.OpenWeb();
            foreach (var single in web.Lists)
            {
                single.LoadExistingItemIdUrlMapping();
            }
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = web.UICulture;
            Console.WriteLine("***************List************");
            foreach (var l in web.Lists)
            {
                Console.WriteLine(l.Title);
            }
            var list = web.Lists["Documents"];
            list.Fields.GetFields(web.ID, list.ID);
            Console.WriteLine("***************Documents Fields************");
            foreach (var field in list.Fields)
            {
                Console.WriteLine(field.Title);
            }
         
            new CreateMetadata().WriteGroupData(250, 2000);
           // new CreateMetadata().WriteGroupData(300, 2000);
            new CreateMetadata().WriterAllMetadata(2000, 400, 10, 50, 100, 14);
            new CreateMetadata().WriteTermStore(10, 50, 140, 14);
            new CreateMetadata().WriteGroupData(400, 2000);
            new CreateMetadata().WriteUserData(2000);
            
            var rs=new UserResourceTest();
            //rs.RunUpdateTest();
            rs.RunTest();
            TestTenantAPI();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            TestAPI();
            #region stp
            //TestContentType();
            //Test();
            //TestWebPart();
            //TestFields();
            //TestDiscovery();
            //TestItemUniqueId();
            //TestGetFile();
            //TestGetItem();
            //TestGetFolder();
            //TestContentTypeXml();
            //TestContentTypeFieldSchemaBackup();
            //TestWebpart1();
            //TestUserInfoList();
            //TestGetDocInfo();
            //TestGetFolderUserData();
            //TestNumberField();
            //TestGetFolderInfo();
            //TestWave15();
            //TestOffice366Http();
            //TestOffice366Http10();
            //AddAttachmentTest();
            //WebRequestTest();
            //Daniel Test
            //ClientAPITest();
            //ExceptionHandlingScopeTest();
            //DeleteListTest();
            //AddRelatedItemTest();
            //CreateSubDiscussionTest();
            //WindowsLiveIdLoginTest();
            //TestLoadFileStream();
            //TestInitClientRequest();
            //Off365LoginTest();
            //TestBackupApps();            
            //TestRestoreApps();
            //TestAddSlideLibrary();
            //TestDocumentBackup();
            //TestBreakPermissionInheritance();
            //TestPermission();
            //TestListParentFolder();
            //TestDiscoverSystemDocument();
            //TestAppWeb();
            //TestApp();
            //TestDownloadFile();
            //TestAspxDocument();
            //TestWebpartBackup();
            //HtmlDocumentTest();
            //TestTypeInfo();
            //TestWebPartMapping();
            //TestSiteOwner();            
            //TestContentTypePublishing();
            //TestSiteAdministrators();
            //Test15Version();
            //Test10Site();
            //Test10TopSite();
            //Test10ModeSite();
            //TestGetItemValues();
            //TestHtmlDoc();
            //TestContentTypePolicy();
            //TestDestContentTypePolicy();
            //TestSourceBarcodes();
            //TestLargeList();
            //TestGetPages();
            //TestGetPages1();
            //TestSiteTempalteDocument();
            //TestDownloadSiteTemplate();
            //TestTwoTemplateFile();
            //TestPageTemplate();
            //TestAppStartPage();
            //TestNavPages();
            //TestUpdateWebPart();
            //TestRichTextField();
            //TestRichTextField1();
            //TestField2();
            //TestContentTypeBackup();
            //TestFileVersionComment();
            //TestAppInstanceTitle();
            //TestSiteAdmin();
            //TestSiteApp();
            //TestSiteUsage();
            //TestFileVersion();
            //TestRecyleBinItems();
            //TestListTemplate();
            //TestContentTypeFieldLinks();
            //DeleteWebWithApp();
            //TestSubFolders();
            //TestGetSiteUsersInSubSite();   
            //TestBreakPermission();
            //TestCASiteCollection();
            //TestItemRoleAssignments();
            //TestLookupField();
            //TestSubSite();
            //TestDiscovery();
            //TestRpcCreateList();
            //TestReplaceCab();

            //TestHtmlAPI();
            //TestAccounts();
            //TestSetSiteAdmin();
            //TestQASite1();
            //TestMissingManifest();
            //TestListItemVersions();
            //TestReliableHttpWebRequest1();
            //TestAsynWebService();
            //TestListItemVersions1();
            #endregion
            TestGetLeftMemory();
        }

        static void TestTenantAPI()
        {
            string userName = "admin@SPEMR146494.onmicrosoft.com";
            string pwd = "tony19890@892240";
            string siteUrl = "https://spemr146494-admin.sharepoint.com";
            string testUrl = "https://spemr146494.sharepoint.com/sites/s2";
            SecureString securePwd = new SecureString();
            pwd.ToList().ForEach(c => securePwd.AppendChar(c));

            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = userName, Password = pwd };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            var tenant = factory.CreateTenant(siteUrl);
            if (IsSiteExistInRecycleBin(tenant, testUrl))
            {
                tenant.RemoveDeletedSite(testUrl);
            }
        }

        private static bool IsSiteExistInRecycleBin(IAveTenant tenant, string siteUrl)
        {
            var exist = false;
            try
            {
                var properties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                exist = true;
            }
            catch (Exception e)
            {
               // mLog.Debug("Check site in recycle bin failed {0}.Error:{1}", siteUrl, e);
            }
            return exist;
        }

        static void TestGetLeftMemory()
        {
            Console.WriteLine(OSInformation.GetLeftMemory());
        }

        static void TestListItemVersions1()
        {
            string siteUrl = "https://frankkk.sharepoint.com/sites/web part site";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "lxy@frankkk.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveFile file = site.RootWeb.GetFile("/sites/web part site/_catalogs/masterpage/hi-in/Preview Images/DefaultMasterPage.png");
            IAveFileVersionCollection fileversions = file.Versions;
            foreach (IAveFileVersion fileVersion in fileversions)
            {
                Console.WriteLine(fileVersion.CreatedBy);
            }
        }

        static void TestAsynWebService()
        {
            Console.WriteLine("begin");
            Console.ReadLine();
            for (int i = 0; i < 100; i++)
            {
                CallGetVersionAsync();
                System.Threading.Thread.Sleep(30000);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }            
            Console.ReadLine();
        }

        static void CallGetVersionAsync()
        {
            for (int i = 0; i < 30; i++)
            {
                using (Lists lists = new Lists())
                {
                    lists.Url = "http://jeff2013:12000/sites/largelist1/_vti_bin/Lists.asmx";
                    (lists as WebClientProtocol).Credentials = new System.Net.NetworkCredential("sjcao", "1qaz2wsxE", "dev9");
                    lists.Timeout = int.MaxValue;
                    lists.GetVersionCollectionCompleted += GetVersionCollectionCompletedEventHandler;
                    lists.GetVersionCollectionAsync("A8168714-AA72-4178-A488-4595BC09AC06", "6003", "Author");
                }
            }
        }

        static void GetVersionCollectionCompletedEventHandler(object sender, GetVersionCollectionCompletedEventArgs e)
        {
            
            Console.WriteLine(e.Result.OuterXml);
        }

        static void TestReliableHttpWebRequest()
        {
            ReliableHttpWebRequest request = ReliableHttpWebRequest.CreateRequest("http://Jeff:8080/WCFTest") as ReliableHttpWebRequest;
            IAsyncResult result = request.BeginGetResponse(CallBack, request);
            Console.WriteLine("do something else");
            //request.EndGetResponse(result);
            Console.ReadLine();
        }

        static void TestReliableHttpWebRequest1()
        {
            for (int i = 0; i < 100; i++)
            {
                HttpWebRequest request1 = WebRequest.Create("http://www.baidu.com") as HttpWebRequest;
                //request1.Proxy = new System.Net.WebProxy("127.0.0.1", 8888);
                //request1.Method = "POST";
                IAsyncResult result1 = request1.BeginGetResponse(CallBack, request1);
                //request1.EndGetRequestStream(result1);
            }

            Console.ReadLine();
            //ReliableHttpWebRequest request = ReliableHttpWebRequest.CreateRequest("http://Jeff:8080/WCFTest") as ReliableHttpWebRequest;
            //IAsyncResult result = request.BeginGetRequestStream(CallBack1, request);
            //Console.WriteLine("do something else");
            ////request.EndGetResponse(result);
            //Console.ReadLine();
        }

        static void CallBack1(IAsyncResult ar)
        {
            ReliableHttpWebRequest request = ar.AsyncState as ReliableHttpWebRequest;
            try
            {
                Console.WriteLine("finished");
                Stream responseStream = request.EndGetRequestStream(ar) as Stream;
            }
            catch (Exception e)
            {
                request.EndGetResponse(ar);
            }
        }

        static void CallBack(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            try
            {
                Console.WriteLine("finished");
                HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;
            }
            catch (Exception e)
            {
                request.EndGetResponse(ar);
            }
        }

        static void TestListItemVersions()
        {
            string siteUrl = "https://test0113.sharepoint.com/sites/helen full02";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "helen01@test0113.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWebCollection webs = site.RootWeb.Webs;            
            IAveWeb web = site.OpenWeb("/sites/helen full02/doc level");
            IAveListCollection lists = web.Lists;
            IAveList list = lists["1w docs sub folder"];
            IAveListItem item = list.GetItemById(5324);
            foreach (IAveListItemVersion itemVersion in item.Versions)
            {
                Console.WriteLine(itemVersion.VersionLabel);
            }
        }

        static void TestMissingManifest()
        {
            string siteUrl = "https://asendia.sharepoint.com";
            AveBPOSAccountInfo user = null;
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWebCollection webs = site.RootWeb.Webs;
            Console.WriteLine(webs[0].Lists.Count);
            Console.WriteLine(webs[1].Lists.Count);
            IAveWeb web = site.OpenWeb("/Asendia");
            IAveListCollection lists = web.Lists;
            Console.WriteLine(lists.Count);           
        }

        static void PrintListCount(IAveWeb web)
        {
            Console.WriteLine(web.Webs[2].Lists.Count);

            Console.WriteLine(web.Lists.Count);
            foreach (IAveWeb subweb in web.Webs)
            {                
                PrintListCount(subweb);
            }
        }

        static void TestQASite1()
        {
            string siteUrl = "https://appcompat11.sharepoint.com/sites/full_helen";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "helen01@appcompat11.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.ID);
        }

        static void TestSetSiteAdmin()
        {
            string siteUrl = "https://jefftest1-admin.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "Jeff@JeffTest1.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.ID);
        }

        static void TestAccounts()
        {
            string siteUrl = "https://jefftest1.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "Jeff@JeffTest1.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            foreach (IAveUser user1 in site.RootWeb.SiteUsers)
            {
                Console.WriteLine(user1.LoginName);
            }

            foreach (IAveGroup group1 in site.RootWeb.SiteGroups)
            {
                Console.WriteLine(group1.LoginName);
            }

            //IAveTenant tenant = factory.CreateTenant(site);
            //tenant.GetPersonalSiteCollectionList("");
        }

        static void TestHtmlAPI()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionOutputOriginalCase = true;
            doc.LoadHtml("<A href='http://www.baidu.com'></A>");
            Console.WriteLine(doc.DocumentNode.OuterHtml);
        }

        static void TestAPI()
        {
            string siteUrl = "https://goodgoodstudy.sharepoint.com/sites/des";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "LeBron@goodgoodstudy.onmicrosoft.com", Password = "mz19930922." };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.RootWeb;
            var a = web.Fields.GetById(new Guid("{23f27201-bee3-471e-b2e7-b64fd8b7ca38}")) as IAveTaxonomyField;//{23f27201-bee3-471e-b2e7-b64fd8b7ca38}
            IAveTaxonomySession session = site.AveSPTaxonomySession;// new IAveTaxonomySession(site);//objectModelFactory.CreateTaxonomySession(mSPSite);
            IAveTermSetCollection terms = session.GetTermSets("222", 1033);//.GetTerm(new Guid("{ff674598-6b03-4ebf-b4f8-f9b0fd739e40}"));
            //IAveTermSet b = a.TermSetId;.
             IAveTermSetCollection terms1 = session.GetTermSets("111", 1033);
            IAveTerm useterm = null;
              foreach (IAveTermSet item in terms1)
            {
                IAveTermCollection tc = item.GetAllTerms();
                if(tc.Count>0)
                {
                    foreach (IAveTerm term in tc)
                    {
                        if(term.Name == "soso")
                          useterm  = term;
                    }
                }
            }
            foreach (IAveTermSet item in terms)
            {
                IAveTermCollection tc = item.GetAllTerms();
                if(tc.Count>0)
                {
                    foreach (IAveTerm term in tc)
                    {
                        if (term.Name == "2222")
                            term.PinTerm(useterm);
                    }
                }
            }
            Console.WriteLine();
            
        }

            //string siteUrl = "https://hlavepoint.sharepoint.com/sites/site01";
            //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "cbi@hlavepoint.onmicrosoft.com", Password = "demo12!@" };
            //AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            //IAveSite site = factory.CreateSite(siteUrl);
            //IAveWeb web = site.RootWeb;
        static void TestReplaceCab()
        {
            byte[] originalCabBinary = File.ReadAllBytes("c:\\Approval.xsn");
            Stream stream = new MemoryStream(originalCabBinary, false);
            Stream fileStream = null;
            using (CabinetExtractor extractor = new CabinetExtractor())
            {
                fileStream = extractor.Extract(stream, "manifest.xsf");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(fileStream);
            XmlNamespaceManager xnm = new XmlNamespaceManager(doc.NameTable);
            xnm.AddNamespace("xsf", "http://schemas.microsoft.com/office/infopath/2003/solutionDefinition");
            xnm.AddNamespace("xsf2", "http://schemas.microsoft.com/office/infopath/2006/solutionDefinition/extensions");
            xnm.AddNamespace("xsf3", "http://schemas.microsoft.com/office/infopath/2009/solutionDefinition/extensions");
            XmlNode node = doc.SelectSingleNode("xsf:xDocumentClass/xsf:extensions/xsf:extension/xsf3:solutionDefinition/xsf3:baseUrl", xnm);
            node.Attributes["relativeUrlBase"].Value = "http://39vm36:12002/sites/workflowtest2/Workflows/workflow test3/Approval.xsn";
            Stream newManifestStream = new MemoryStream();
            doc.Save(newManifestStream);
            newManifestStream.Position = 0;
            if (newManifestStream != null)
            {
                Stream input = GenenerateFixedXSNCab(stream, newManifestStream);
                input.Position = 0;
                newManifestStream.Close();
                input.Seek(0L, SeekOrigin.Begin);
                byte[] buffer = new byte[input.Length];
                new BinaryReader(input).Read(buffer, 0, Convert.ToInt32(input.Length));
                input.Close();
                System.IO.File.WriteAllBytes("c:\\Approval1.xsn", buffer);
            }
            if (newManifestStream != null)
            {
                newManifestStream.Close();
            }
        }

        private static Stream GenenerateFixedXSNCab(Stream originalCabStream, Stream newManifestStream)
        {
            MemoryStream newCabStream = new MemoryStream();
            using (CabinetExtractor extractor = new CabinetExtractor())
            {
                IList<CabinetFileInfo> fileInfo = extractor.GetFileInfo(originalCabStream);
                List<string> list2 = new List<string>(fileInfo.Count);
                Dictionary<string, CabinetFileInfo> cabFilesInfo = new Dictionary<string, CabinetFileInfo>(fileInfo.Count, StringComparer.OrdinalIgnoreCase);
                foreach (CabinetFileInfo info in fileInfo)
                {
                    cabFilesInfo.Add(info.Name, info);
                    list2.Add(info.Name);
                }
                using (CabinetCreator creator = new CabinetCreator())
                {
                    //creator.Create(new CabinetCreatorHelper(newCabStream, newManifestStream, originalCabStream, cabFilesInfo, extractor), list2.ToArray());
                }
            }
            return newCabStream;
        }

        static void TestRpcCreateList()
        {
            //string postUrl = "http://39vm36:12002/sites/workflow4/_vti_bin/owssvr.dll?Cmd=DisplayPost";
            //NetworkCredential mObj = new NetworkCredential("sjcao", "1qaz2wsxE", "dev9");
            //byte[] body = Encoding.UTF8.GetBytes(System.IO.File.ReadAllText("c:\\createlist.txt"));
            //Dictionary<string, object> headers = new Dictionary<string, object>();
            //headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            //headers["MIME-Version"] = "1.0";
            //headers["X-Vermeer-Content-Type"] = "application/xml";
            //AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/xml", body, headers);
        }

        static void TestCamlQuery()
        {

        }

        static void TestDiscovery()
        {
            WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = true;
            string siteUrl = "https://danieldemo1.sharepoint.com/sites/TeamSC3";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveList list = site.RootWeb.Lists["Tasks"];
            IAveDiscoveryQuery discoveryQuery = factory.CreateDiscoveryQuery(site, DiscoverModule.Item);
            AveSiteCache siteCache = new AveSiteCache(site, factory, DiscoverModule.Item);
            AveWebCache webCache = new AveWebCache(siteCache, site.RootWeb.ID, site.RootWeb);
            AveFolderCache folderCache = new AveFolderCache(webCache, list.ID);
            AveItemObject itemObject = new AveItemObject();
            itemObject.FullUrl = list.RootFolder.ServerRelativeUrl;
            discoveryQuery.QueryListItemForFB(folderCache, itemObject, true, false);
            Console.ReadLine();
        }

        static void TestSubSite()
        {
            string siteUrl = "http://jeff2013:12001/sites/createsitetest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb subweb = site.RootWeb.Webs.Add("sub03", "sub03", "", 1033, "sts#0", false, false);
        }

        static void TestLookupField()
        {
            string siteUrl = "https://avepointqa.sharepoint.com/sites/Jeff1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jyu@avepointqa.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            //IAveWeb subweb = site.RootWeb.Webs.Add("site03", "site03", "", 1033, "sts#0", false, false);
            IAveWeb web = site.OpenWeb("/sites/Jeff1/site03");
            //web.Delete();
            foreach (IAveField field in web.Fields)
            {
                if (field.Title.Equals("lookup2_helen"))
                {
                    IAveFieldLookup fieldLookup = field as IAveFieldLookup;
                    Console.WriteLine(fieldLookup.LookupList);
                    Console.WriteLine(fieldLookup.LookupField);
                }
            }
            IAveList list = web.Lists["dependent_lib"];
            Console.WriteLine(list.ID);
        }

        static void TestItemRoleAssignments()
        {
            string siteUrl = "https://docavedemo.sharepoint.com/sites/DAO_QA_Source";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/DAO_QA_Source/lxy(japanese)/Team Site");
            IAveList list = web.Lists["custom list"];
            IAveListItem item = list.GetItemById(3);
            Console.WriteLine(item.HasUniqueRoleAssignments);
            foreach (IAveListItemVersion version in item.Versions)
            {

            }
        }

        static void TestCASiteCollection()
        {
            string siteUrl = "https://us13tester-admin.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "olivia@us13tester.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
        }

        static void TestBreakPermission()
        {
            string siteUrl = "https://docavedemo.sharepoint.com/sites/PermissionTest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/PermissionTest1/Team Site");
            IAveList list = web.Lists["list"];
            IAveListItem item = list.GetItemById(1);
            item.BreakRoleInheritance(false, false);
        }

        static void TestGetSiteUsersInSubSite()
        {
            string siteUrl = "https://avepointqa.sharepoint.com/sites/第４回園芸技術講演会";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jyu@avepointqa.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/第４回園芸技術講演会/グループで共同作業する場所です");
            IAveUserCollection users = web.SiteUsers;
            Console.WriteLine(users.Count);
        }

        static void TestSubFolders()
        {
            string siteUrl = "http://jeff2013:12001/sites/apptest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "cbi", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1/subsite1");
            IAveList list = web.Lists["Documents"];
            IAveFolderCollection folders = list.RootFolder.SubFolders;
            foreach (IAveFolder folder in folders)
            {
                Console.WriteLine(folder.Name);
            }
        }

        static void DeleteWebWithApp()
        {
            string siteUrl = "http://jeff2013:12001/sites/apptest1/subsite1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "cbi", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1/subsite1");
            DeleteWeb(web, factory.CreateAppCatalog());
        }

        private static void DeleteWeb(IAveWeb web, IAveAppCatalog appCatalog)
        {
            try
            {
                for (int i = web.Webs.Count - 1; i >= 0; i--)
                {
                    IAveWeb subWeb = web.Webs[i];
                    try
                    {
                        DeleteWeb(subWeb, appCatalog);
                    }
                    finally
                    {
                        if (subWeb != null)
                        {
                            subWeb.Dispose();
                        }
                    }
                }

                //IList<IAveAppInstance> appinstances = appCatalog.GetAppInstances(web);
                //foreach (IAveAppInstance appInstance in appinstances)
                //{
                //    appInstance.Uninstall();
                //}

                if (web.Properties.ContainsKey("BackedUp"))
                {
                    web.Properties["BackedUp"] = "true";
                }
                else
                {
                    web.Properties.Add("BackedUp", "true");
                }
                web.Properties.Update();
                web.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void TestContentTypeFieldLinks()
        {
            string siteUrl = "https://lxy.sharepoint.com/sites/apptest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "lxy@lxy.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1/Google Office Locator");
            IAveList list = web.Lists["Master Page Gallery"];
            IAveContentType ct = list.ContentTypes["Master Page Preview"];
            Console.WriteLine(ct.SchemaXml);
            foreach (IAveFieldLink fieldLink in ct.FieldLinks)
            {
                Console.WriteLine(fieldLink.Name);
            }

            Console.WriteLine("------");
            foreach (IAveField field in ct.Fields)
            {
                Console.WriteLine(field.InternalName);
            }
        }

        static void TestListTemplate()
        {
            string siteUrl = "https://compatqq.sharepoint.com/sites/Blog";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jyu@compatqq.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/Blog");
            foreach (IAveList list in web.Lists)
            {
                Console.WriteLine("title: {0},  template: {1}", list.Title, list.BaseTemplate);
            }
        }

        static void TestRecyleBinItems()
        {
            string siteUrl = "http://jeff2013:12000/sites/apptest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1");
            foreach (IAveRecycleBinItem rbItem in site.RecycleBin)
            {
                Console.WriteLine(rbItem.ItemType == AveRecycleBinItemType.Web);
            }
        }

        static void TestFileVersion()
        {
            string siteUrl = "http://jeff2013:12000/sites/apptest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1");
            IAveFile file = web.GetFile("/sites/apptest1/Shared Documents/badnav.txt");
            foreach (IAveFileVersion fv in file.Versions)
            {
                Console.WriteLine(fv.Size);
            }
        }

        static void TestSiteUsage()
        {
            string siteUrl = "https://docaveonline2.sharepoint.com/sites/ct1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@docaveonline2.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            Console.WriteLine(site.Usage.Bandwidth);
            Console.WriteLine(site.Usage.DiscussionStorage);
            Console.WriteLine(site.Usage.Hits);
            Console.WriteLine(site.Usage.Storage);
            Console.WriteLine(site.Usage.StoragePercentageUsed);
            Console.WriteLine(site.Usage.Visits);
        }

        static void TestSiteApp()
        {
            string siteUrl = "https://docaveonline2.sharepoint.com/sites/ct1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@docaveonline2.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/ct1/subsite2");
            IAveAppCatalog appcatalog = factory.CreateAppCatalog();
            IList<IAveAppInstance> apps = appcatalog.GetAppInstances(web);
            foreach (IAveAppInstance app in apps)
            {
                Console.WriteLine(app.AppWebFullUrl);
            }
        }

        static void TestSiteAdmin()
        {
            //ff@offo.onmicrosoft.com 
            string siteUrl = "https://offo.sharepoint.com/sites/01f1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "ff@offo.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            foreach (IAveUser user1 in site.RootWeb.SiteUsers)
            {
                Console.WriteLine(user1.IsSiteAdmin);
            }
        }

        static void TestAppInstanceTitle()
        {
            string siteUrl = "http://jeff2013:12000/sites/apptest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/apptest1");
            IAveAppCatalog appCatalog = factory.CreateAppCatalog();
            IList<IAveAppInstance> apps = appCatalog.GetAppInstances(web);
            foreach (IAveAppInstance app in apps)
            {
                Console.WriteLine(app.Title);
            }
        }

        static void TestFileVersionComment()
        {
            //_CheckinComment
            string siteUrl = "https://us13tester.sharepoint.com/sites/Jeff";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "olivia@us13tester.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/Jeff");
            IAveList list = web.Lists["doc2"];
            foreach (IAveField field in list.Fields)
            {
                Console.WriteLine(field.InternalName);
            }
        }

        static void TestContentTypeBackup()
        {
            string siteUrl = "https://lxy.sharepoint.com/sites/jira review 16";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "lxy@lxy.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/jira review 16/site1/Bing Office Locator");
            IAveList list = web.Lists["Composed Looks"];
            IAveContentTypeCollection cts = list.ContentTypes;
            AveContentTypeCollectionInfo ctinfos = cts.GetContentTypeInfos(true);
        }

        static void TestField2()
        {
            string siteUrl = "http://jeff2013:12000/sites/field test1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/field test1");
            IAveField field = web.Fields["richtext1"];
            Console.WriteLine(field.SchemaXml);
            IAveFieldMultiLineText mField = field as IAveFieldMultiLineText;
            mField.RichText = true;
            mField.IsolateStyles = true;
            mField.RichTextMode = AveRichTextMode.FullHtml;
            mField.Update();
            Console.WriteLine(mField.RowOrdinal);
        }

        static void TestRichTextField1()
        {
            string siteUrl = "https://us13tester.sharepoint.com/sites/Jeff";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "olivia@us13tester.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/Jeff");
            IAveList list = web.Lists["PromotedLinks_S"];
            IAveField field = list.Fields["Description"];
            Console.WriteLine(field.SchemaXml);
            IAveFieldMultiLineText mField = field as IAveFieldMultiLineText;
            mField.RichTextMode = AveRichTextMode.FullHtml;
            mField.Update();
        }

        static void TestRichTextField()
        {
            string siteUrl = "https://us13tester.sharepoint.com/sites/ts_s";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "olivia@us13tester.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/ts_s");
            IAveList list = web.Lists["PromotedLinks_S"];
            IAveField field = list.Fields["Description"];
            Console.WriteLine(field.SchemaXml);
            IAveFieldMultiLineText mField = field as IAveFieldMultiLineText;
            Console.WriteLine(mField.IsolateStyles);
            IAveListItem item = list.GetItemById(1);
            object value = item["Description"];
        }

        static void TestUpdateWebPart()
        {
            string siteUrl = "http://jeff2013:12000/sites/webparttest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveWeb web = site.OpenWeb("/sites/webparttest1");
            IAveFile file = web.GetFile("/sites/webparttest1/SitePages/tag cloud webpart.aspx");
            IAveLimitedWebPartManager lwpm = factory.CreateLimitedWebPartManager(site, web, file.ServerRelativeUrl);

            List<AveWebPartBaseInfo> webparts = lwpm.GetWebParts(new AveBaseItemInfo());
            foreach (AveWebPartBaseInfo baseinfo in webparts)
            {
                IWebPartPropertyExtractor extractor = WebPartExtractorFactory.Create(baseinfo.DefinitionXml);
                Console.WriteLine(extractor.TypeFullName);
            }
        }


        static void TestNavPages()
        {
            Dictionary<string, object> formvalues = AveHttpWebRequestUtility.GetPostFormValues(System.IO.File.ReadAllText(@"c:\areanavigationsettingpage.txt"), false);
            Console.WriteLine(formvalues.Count);
        }

        static void TestAppStartPage()
        {
            string url = "https://daotest7.sharepoint.com/sites/site tempalte test1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = " docave@daotest7.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveAppCatalog appCatalog = factory.CreateAppCatalog();
            IList<IAveAppInstance> apps = appCatalog.GetAppInstances(site.RootWeb);
            Console.WriteLine(apps[0].LaunchUrl);
            IList<IAveAppInstance> apps1 = appCatalog.GetAppInstancesByProductId(site.RootWeb, apps[0].App.ProductId);
            Console.WriteLine(apps1[0].LaunchUrl);
        }

        static void TestPageTemplate()
        {
            string url = "http://jeff2013:12001/sites/navigation1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/navigation1");
            IAveList list = web.Lists["Pages"];
            Console.WriteLine(list.BaseTemplate);
        }

        static void TestTwoTemplateFile()
        {
            byte[] file1 = File.ReadAllBytes("c:\\template1.wsp");
            byte[] file2 = File.ReadAllBytes("c:\\template2.wsp");
            for (int i = 0; i < file1.Length; i++)
            {
                if (file1[i] != file2[i])
                {
                    Console.WriteLine(i);
                }
            }
        }

        static void TestDownloadSiteTemplate()
        {
            string url = "https://daotest7.sharepoint.com/sites/jericho4";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@daotest7.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/jericho4");
            IAveFile file = web.GetFile("/sites/jericho4/_catalogs/solutions/template.wsp");
            Stream filecontent = file.OpenBinaryStream();
            using (Stream fileStream = File.OpenWrite("c:\\template1.wsp"))
            {
                AveIOHelper.Copy(filecontent, fileStream);
            }
        }

        static void TestSiteTempalteDocument()
        {
            string url = "https://daotest7.sharepoint.com/sites/site tempalte test1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@daotest7.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/site tempalte test1");
            IAveList list = web.GetCatalog(AveListTemplateType.SolutionCatalog);
            using (Stream stream = File.OpenRead("c:\\template.wsp"))
            {
                MemoryStream ms = new MemoryStream();
                AveIOHelper.Copy(stream, ms);
                ms.Position = 0;
                IAveFile file = list.RootFolder.Files.Add(list.RootFolder.ServerRelativeUrl + "/template.wsp", ms.ToArray(), true);
            }
        }

        static void TestGetPages1()
        {
            string url = "http://jeff2013:12001/sites/ca search test1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/ca search test1");
            IAveList list = web.Lists["Documents"];
            IAveListItemCollection items = list.GetPages();
            foreach (IAveListItem item in items)
            {
                string itemName = item["FileLeafRef"].ToString();
                string parentFolderName = item["FileDirRef"].ToString();
                string fullPath = string.Format("{0}{1}/{2}", "http://jeff2013:12001", parentFolderName, itemName);
                Console.WriteLine(fullPath);
            }
        }

        static void TestGetPages()
        {
            string url = "https://compatqq.sharepoint.com/sites/Blog";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jyu@compatqq.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/Blog");
            IAveList list = web.Lists["IM Excel"];
            IAveListItemCollection items = list.GetPages();
        }

        static void TestLargeList()
        {
            string url = "https://compatqq.sharepoint.com/sites/Blog";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jyu@compatqq.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/Blog");
            IAveList list = web.Lists["IM Excel"];
            foreach (IAveField field in list.Fields)
            {
                Console.WriteLine(field.InternalName);
            }
            //AveCamlQuery query = new AveCamlQuery();
            //query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><Query><Where><And>"
            //                + "<Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt>"
            //                + "<Lt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Lt>"
            //                //+ "<Contains><FieldRef Name=\"FileLeafRef\"/><Value Type='Text'>.aspx</Value></Contains>"                            
            //                + "</And></Where></Query></View>", 0, int.MaxValue);
            //query.ListItemCollectionPosition = new AveItemCollectionPosition();
            //query.FolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;


            //foreach (IAveListItem item in list.GetItems(query))
            //{
            //}
        }

        static void TestSourceBarcodes()
        {
            string url = "https://lxy.sharepoint.com/sites/lxy(s)library001";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "lxy@lxy.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/lxy(s)library001/Asset Library");
            IAveList list = web.Lists["13-47"];
            IAveListItem item = list.GetItemById(4);
            foreach (IAveField field in list.Fields)
            {
                Console.WriteLine("name: {0}, value: {1}", field.Title, item[field.InternalName]);
            }
        }

        static void TestDestContentTypePolicy()
        {
            string url = "https://offo.sharepoint.com/sites/asset library test1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "ff@offo.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/asset library test1/Asset Library");
            IAveList list = web.Lists["13-47"];
            IAveListItem item = list.GetItemById(59);
            foreach (IAveField field in list.Fields)
            {
                Console.WriteLine("name: {0}, value: {1}", field.Title, item[field.InternalName]);
            }
        }

        static void TestContentTypePolicy()
        {
            string url = "https://lxy.sharepoint.com/sites/lxy(s)library001";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "lxy@lxy.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/lxy(s)library001/Asset Library");
            IAveList list = web.Lists["13-47"];
            IAveContentTypeCollection cts = list.ContentTypes;
            foreach (IAveContentType ct in cts)
            {
                Console.WriteLine(ct.Name);
                Console.WriteLine(ct.SchemaXml);
            }
        }

        static void TestHtmlDoc()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml("<ReplaceXmlLinks><div>a</div></ReplaceXmlLinks>");
            Console.WriteLine(doc.DocumentNode.FirstChild.OuterHtml);
        }

        static void TestGetItemValues()
        {
            string url = "http://jeff2013:12001/sites/fieldtest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveList list = site.RootWeb.Lists["13 fields"];
            IAveListItem item = list.Items[0];
            foreach (IAveField field in item.Fields)
            {
                Console.WriteLine(item[field.InternalName]);
            }
            IAveListItemVersion itemVersion = item.Versions.GetVersionFromID(512);
            foreach (IAveField field in itemVersion.Fields)
            {
                Console.WriteLine(itemVersion[field.InternalName]);
            }
        }

        static void TestAddFile()
        {
            string url = "http://39vm36:12000/sites/contenttype publishing";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();

        }

        static void Test10TopSite()
        {
            string url = "http://jeff:12000";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.ID);
        }

        static void Test10Site()
        {
            string url = "http://jeff:12001/sites/site1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.ID);
        }

        static void Test15Version()
        {
            string url = "https://avepointus-admin.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "JCPM@AvePointUS.onmicrosoft.com", Password = "Dao-365@#^%" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
        }

        static void TestSiteCollectionCreation()
        {
            string url = "https://offo-admin.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "ff@offo.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveUserCollection users = site.RootWeb.SiteAdministrators;
            Console.WriteLine(users.Count);
        }

        static void TestSiteAdministrators()
        {
            string url = "https://susanbeta.sharepoint.com";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "susan.li@susanbeta.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveUserCollection users = site.RootWeb.SiteAdministrators;
            Console.WriteLine(users.Count);
        }

        static void TestContentTypePublishing()
        {
            string url = "https://testinsg.sharepoint.com/sites/JeffDest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@testinsg.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            List<Dictionary<string, object>> cts = site.GetPublishedContentTypes();
        }

        static void Test10ModeSite()
        {
            string url = "http://39vm36:12000/sites/contenttype publishing";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.Owner.LoginName);
        }

        static void TestSiteOwner()
        {
            string url = "https://jerichoren.sharepoint.com/teams/source1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jerichoren@jerichoren.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.Owner);
        }

        static void TestWebPartMapping()
        {
            string webpartXml = new StreamReader(File.OpenRead("c:\\RssViewer.webpart")).ReadToEnd();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webpartXml);
            //AveWebPartPropertyUpdater updater = AveClientWebPartUrlHandlerFactory.GenerateWebPartUrlHanlder(webpartXml, null, doc.DocumentElement, null);
        }

        static void TestTypeInfo()
        {
            TypeInfo typeInfo = TypeInfo.Parse("Microsoft.SharePoint.WebPartPages.XsltListViewWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
        }

        static void HtmlDocumentTest()
        {
            using (Stream s = File.OpenRead("c:\\htmlfragment.txt"))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(s);
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a");
            }
        }

        static void TestWebpartBackup()
        {
            string url = "https://jerichoren.sharepoint.com/teams/source1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jerichoren@jerichoren.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveFile file = site.RootWeb.GetFile("/teams/source1/SitePages/custompage.aspx");
            IAveLimitedWebPartManager webpartManager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
            List<AveWebPartBaseInfo> webparts = webpartManager.GetWebParts(new AveBaseItemInfo());
            foreach (IAveWebPart wp in webpartManager.WebParts)
            {
                Console.WriteLine("zone: {0}, index: {1}, title: {2}", wp.ZoneID, wp.ZoneIndex, wp.Title);
            }

        }

        static void TestAspxDocument()
        {
            using (FileStream fs = File.OpenRead("c:\\aspxtest.txt"))
            {
                MixedCodeDocument mcd = new MixedCodeDocument();
                mcd.Load(fs);
                foreach (MixedCodeDocumentCodeFragment mcdf in mcd.CodeFragments)
                {
                    Console.WriteLine(mcdf.FragmentText);
                }
                Console.WriteLine("----------------------------------------------------");
                foreach (MixedCodeDocumentTextFragment mcdtf in mcd.TextFragments)
                {
                    Console.WriteLine(mcdtf.FragmentText);
                }
                Console.WriteLine("----------------------------------------------------");
                foreach (MixedCodeDocumentFragment m in mcd.Fragments)
                {
                    Console.WriteLine(m.FragmentText);
                }
            }
        }

        static void TestDownloadFile()
        {
            string url = "https://testinsg.sharepoint.com/sites/JeffDest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@testinsg.onmicrosoft.com", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveFile file = site.RootWeb.GetFile("/sites/JeffDest1/Lists/ContentTypeSyncLog/Forms/client_LocationBasedDefaults.html");
            Stream content = file.OpenBinaryStream(AveOpenBinaryOptions.None);
            string str = new StreamReader(content).ReadToEnd();
        }

        static void TestApp()
        {
            //"5a613e7e-8158-4dd8-b889-d72153f8ef5a"
            string url = "https://danieldemo.sharepoint.com/sites/apptest2";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "killer@danieldemo.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IList<IAveAppInstance> apps = site.RootWeb.GetAppInstancesByProductId(new Guid("5a613e7e-8158-4dd8-b889-d72153f8ef5a"));
            Console.WriteLine(apps[0].AppWebFullUrl);
        }

        static void TestAppWeb()
        {
            ///sites/apptest2/MyLocations/_cts/Master Page Preview
            string url = "https://danieldemo.sharepoint.com/sites/apptest2";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "killer@danieldemo.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveWeb web = site.OpenWeb("/sites/apptest2/MyLocations");
            IAveFolder folder = web.GetFolder("/sites/apptest2/MyLocations/_cts/Master Page Preview");
        }

        static void TestDiscoverSystemDocument()
        {
            WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = true;
            string siteUrl = "http://testp1insg.sharepoint.com/";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@testP1inSG.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite(siteUrl);
            IAveDiscoveryQuery discoveryQuery = factory.CreateDiscoveryQuery(site, DiscoverModule.Item);
            AveSiteCache siteCache = new AveSiteCache(site, factory, DiscoverModule.Item);
            AveWebCache webCache = new AveWebCache(siteCache, site.RootWeb.ID, site.RootWeb);
            AveListCache listCache = new AveListCache(webCache, Guid.Empty);
            AveFolderCache folderCache = new AveFolderCache(webCache, Guid.Empty);
            AveItemObject itemObject = new AveItemObject();
            itemObject.FullUrl = "/";
            discoveryQuery.QueryListItemForFB(folderCache, itemObject, true, true);
            Console.WriteLine(itemObject.VersionObjs.Count);
        }

        static void TestListParentFolder()
        {
            string url = "http://39vm36:12000/sites/webparttest1";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            IAveList list1 = site.RootWeb.GetCatalog(AveListTemplateType.MasterPageCatalog);
            IAveFolder folder = site.RootWeb.GetFolder("/sites/webparttest1/Shared Documents/folder1/folder2");
            IAveFolder parentFolder = folder.ParentFolder;
            while (parentFolder != null)
            {
                Console.WriteLine(parentFolder.Name);
                parentFolder = parentFolder.ParentFolder;
            }
        }

        static void TestPermission()
        {
            string url = "http://testp1insg.sharepoint.com/";
            AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@testP1inSG.onmicrosoft.com", Password = "demo12!@" };
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
            IAveSite site = factory.CreateSite();
            Console.WriteLine(site.ID);
        }

        //static void TestBreakPermissionInheritance()
        //{
        //    string url = "http://jeff:12001/sites/manysubsite";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "dev9", UserName = "sjcao", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite();
        //    IAveWeb web = site.OpenWeb("/sites/manysubsite/subsite_1_7/subsite_2_2/subsite_3_1");
        //    Console.WriteLine(web.Lists.Count);
        //    web.BreakRoleInheritance(true, false);
        //}

        //static void TestDocumentBackup()
        //{
        //    string url = "https://daotest11.sharepoint.com";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@daotest11.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(url, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite();
        //    IAveFile file = site.RootWeb.GetFile("/Document 1/QA_Test.docx");
        //    foreach (IAveFileVersion fileVersion in file.Versions)
        //    {
        //        //Stream stream = fileVersion.OpenBinaryStream();                
        //        Stream stream = file.OpenVersionBinaryStream(fileVersion.ID);
        //    }
        //}

        //static void TestAddSlideLibrary()
        //{
        //    string siteUrl = "https://daotest11.sharepoint.com/sites/JeffDest";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jericho.ren@daotest11.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    SPOnlineAuthentication oauth = new SPOnlineAuthentication(siteUrl);
        //    object cookie = oauth.Login(user.UserName, user.Password);
        //    AveWebServiceRequest request = new AveWebServiceRequest(siteUrl, user, cookie, "15.0.0.0");
        //    AveWebServiceRequest.AddSlideFolder("https://daotest11.sharepoint.com", "/sites/JeffDest", "Slide Library", "/sites/JeffDest/Slide Library", "bb", cookie);
        //}

        //static void TestRestoreApps()
        //{//{548a4826-d6ef-4c56-b736-0edf7d0b7abb}  world clock           
        //    string addanappUrl = "http://jeff2013:12000/sites/apptest3/_layouts/15/addanapp.aspx?task=GetMyApps&sort=1&query=&myappscatalog=1&ci=1&vd=1";
        //    NetworkCredential nc = new NetworkCredential("cbi", "1qaz2wsxE", "dev9");

        //    string addanapp = "http://jeff2013:12000/sites/apptest3/_layouts/15/addanapp.aspx";
        //    string yourapps = AveHttpWebRequestUtility.HttpGet(addanapp, nc);
        //    Dictionary<string, object> addappFormValues = AveHttpWebRequestUtility.GetPostFormValues(yourapps);

        //    string result = AveHttpWebRequestUtility.HttpGet(addanappUrl, nc);
        //    JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
        //    IList<Dictionary<string, object>> appsMetadata = jsSerializer.Deserialize<List<Dictionary<string, object>>>(result);
        //    Guid productId = new Guid("{548a4826-d6ef-4c56-b736-0edf7d0b7abb}");
        //    Dictionary<string, object> appMetadata = GetAppPropertiesById(appsMetadata, productId);

        //    string appinvUrl = "http://jeff2013:12000/sites/apptest3/_layouts/15/appinv.aspx?catalog=0&appcatalogid=WA103062091&bm=CN&cm=en%2DUS&IsDlg=1";
        //    StringBuilder appInv = new StringBuilder("http://jeff2013:12000/sites/apptest3/_layouts/15/appinv.aspx");
        //    appInv.Append("?")
        //          .Append("catalog=").Append(HttpUtility.UrlEncode(appMetadata["Catalog"] as string))
        //          .Append("&appcatalogid=").Append(HttpUtility.UrlEncode(appMetadata["AssetId"] as string))
        //          .Append("&bm=").Append(HttpUtility.UrlEncode((appMetadata["License"] as Dictionary<string, object>)["CountryRegion"] as string))
        //          .Append("&cm=").Append(HttpUtility.UrlEncode((appMetadata["License"] as Dictionary<string, object>)["Culture"] as string))
        //          .Append("&IsDlg=1");
        //    string appinvResult = AveHttpWebRequestUtility.HttpGet(appinvUrl, nc);

        //    Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(appinvResult);
        //    formValues["__EVENTTARGET"] = "ctl00$PlaceHolderMain$BtnAllow";
        //    byte[] body = AveHttpWebRequestUtility.GetByte(formValues, null);
        //    string closeDialogContent = AveHttpWebRequestUtility.HttpReturn(appInv.ToString(), nc, "application/x-www-form-urlencoded", body, null);

        //    //<script type="text/javascript">window.frameElement.commonModalDialogClose(1, 'i:0i.t|ms.sp.int|f32519e3-4e7f-41ff-8eff-e3e1efbb3e28@3325bbbf-2b71-4815-a615-68cfb39ad96e');</script>
        //    string closeDialog = "window.frameElement.commonModalDialogClose(";
        //    int leftBracePos = closeDialogContent.IndexOf(closeDialog);
        //    if (leftBracePos != -1)
        //    {
        //        string principalId = closeDialogContent.Substring(leftBracePos + closeDialog.Length, closeDialogContent.IndexOf(")", leftBracePos) - leftBracePos - closeDialog.Length);
        //        principalId = principalId.Split(',')[1].Trim().Trim('\'');

        //        addappFormValues["task"] = "AppDownload";
        //        addappFormValues["appid"] = productId.ToString();
        //        addappFormValues["oID"] = principalId;
        //        addappFormValues["catalog"] = appMetadata["Catalog"];
        //        byte[] body1 = AveHttpWebRequestUtility.GetByte(addappFormValues, null);
        //        string r = AveHttpWebRequestUtility.HttpReturn(addanapp, nc, "application/x-www-form-urlencoded", body1, null);
        //    }
        //}

        //public static Dictionary<string, object> GetAppPropertiesById(IList<Dictionary<string, object>> appsMetadata, Guid appId)
        //{
        //    return appsMetadata.SingleOrDefault<Dictionary<string, object>>(
        //         (appMetadata) => appMetadata.ContainsKey("ProductId")
        //            && new Guid(appMetadata["ProductId"] as string) == appId);
        //}

        //static void TestBackupApps()
        //{
        //    string siteUrl = "https://offr.sharepoint.com/sites/appdest";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "ff@offr.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveAppCatalog appCatalog = factory.CreateAppCatalog();            
        //    IList<IAveAppInstance> appInstances = appCatalog.GetAppInstances(site.RootWeb);
        //    foreach (IAveAppInstance appInstance in appInstances)
        //    {
        //        appCatalog.GetAppInstancesByProductId(site.RootWeb, appInstance.App.ProductId);
        //        Console.WriteLine(appInstance.Id);
        //        Console.WriteLine(appInstance.App.ProductId);                
        //    }
        //}

        //static void TestInitClientRequest()
        //{
        //    string siteUrl = "http://docavedemo-web.sharepoint.com";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //}

        //static void TestLoadFileStream()
        //{
        //    ///sites/a8
        //    ///sites/a8/New Doc/te st.png
        //    //_vti_history/512/New Doc/te st.png
        //    //512

        //    //Console.WriteLine(HttpUtility.UrlPathEncode("serverRelativeUrl=/list/1，。  a.txt", false));

        //    string url = "https://dao7.sharepoint.com/sites/a8";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "docave@dao7.onmicrosoft.com", Password = "demo12!@" };
        //    SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //    object obj = au.Login(user.UserName, user.Password);
        //    using (AveWebServiceRequest webServiceRequest = new AveWebServiceRequest(url, user, obj, ""))
        //    {
        //        using (Stream s = webServiceRequest.GetFileStream("/sites/a8", "New Doc/Te，。 st.txt", null))
        //        {
        //            byte[] buffer = new byte[65536];
        //            int len = s.Read(buffer, 0, 65536);
        //        }
        //    }
        //}

        //static void LoginTestThread(string title, string url, string username, string password, bool isErrorCheck)
        //{
        //    string passCode = isErrorCheck ? "Failed" : "Passed";
        //    string failCode = isErrorCheck ? "Passed" : "Failed";
        //    try
        //    {
        //        SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //        Console.WriteLine(title + " " + (au.Login(username, password) != null ? passCode : failCode));
        //    }
        //    catch (Exception e)
        //    {
        //        if (isErrorCheck)
        //        {
        //            Console.WriteLine(title + " Passed");
        //        }
        //        else
        //        {
        //            Console.WriteLine(title + " Failed, detail : {0}", e.ToString());
        //        }
        //    }
        //    lock (LoginTestLocker)
        //    {
        //        TotalLoginTestCount--;
        //        if (TotalLoginTestCount == 0)
        //        {
        //            LoginTestWaiter.Set();
        //        }
        //    }
        //}

        //static void StartLoginTest(string title, string url, string username, string password, bool isErrorCheck = false)
        //{
        //    Thread t = new Thread(new ThreadStart(() => { LoginTestThread(title, url, username, password, isErrorCheck); }));
        //    t.IsBackground = true;
        //    t.Start();
        //}

        //static int TotalLoginTestCount = 0;
        //static object LoginTestLocker = new object();
        //static EventWaitHandle LoginTestWaiter = new EventWaitHandle(false, EventResetMode.AutoReset);

        //static void Off365LoginTest()
        //{
        //    Dictionary<string, bool> testcases = new Dictionary<string,bool>();
        //    testcases.Add("SpecialCharUrl", false);
        //    testcases.Add("AddAsAdministrator", false);
        //    testcases.Add("PasswordIncorrect", false);
        //    testcases.Add("UsernameIncorrect", false);
        //    testcases.Add("HttpSite2010", false);
        //    testcases.Add("HttpSite2013", false);
        //    testcases.Add("SharePointID2010", false);
        //    testcases.Add("SharePointID2013", false);
        //    testcases.Add("LiveID2010", false);
        //    testcases.Add("LiveID2013", false);
        //    testcases.Add("LocalADFS2010", false);
        //    testcases.Add("LocalADFS2013", false);
        //    testcases.Add("LocalADFS2010Scan", false);
        //    testcases.Add("LocalADFS2013Scan", false);
        //    bool isFullTest = true;
        //    foreach (KeyValuePair<string, bool> singleCase in testcases)
        //    {
        //        TotalLoginTestCount = singleCase.Value || isFullTest ? TotalLoginTestCount + 1 : TotalLoginTestCount;
        //    }

        //    #region Url contains special chars
        //    if (testcases["SpecialCharUrl"] || isFullTest)
        //    {
        //        StartLoginTest("Url contains special chars Test",
        //                       "https://avebeta.sharepoint.com/sites/Sdfunweiolfkds!!@$^()_-=[];’,.1265787fdgrxtye545269Sdfunweiolfkds!!@$^()_-=[];’,.1265787fdgrtye5452",
        //                       "susan.li@avebeta.onmicrosoft.com",
        //                       "1qaz2wsxE");
        //    }
        //    #endregion

        //    #region Add as administrator
        //    if (testcases["AddAsAdministrator"] || isFullTest)
        //    {
        //        StartLoginTest("Add as administrator Test",
        //                       "https://docavedemo-admin.sharepoint.com/_layouts/online/TA_SiteCollectionOwnersDialog.aspx?site=https://docavedemo.sharepoint.com/sites/BiCheng&IsDlg=1",
        //                       "daniel.han@adfstest.docaveonline.com",
        //                       "demo12!@");
        //    }
        //    #endregion

        //    #region Incorrect Password
        //    if (testcases["PasswordIncorrect"] || isFullTest)
        //    {
        //        StartLoginTest("Incorrect Password Test", "https://docavedemo.sharepoint.com/sites/BiCheng", "daniel.han@adfstest.docaveonline.com", "1qaz2wsxE", true);
        //    }
        //    #endregion

        //    #region Incorrect Username
        //    if (testcases["UsernameIncorrect"] || isFullTest)
        //    {
        //        StartLoginTest("Incorrect Username Test", "https://avepointbeta.sharepoint.com/sites/Daniel", "daniel.han@avepoint.com", "demo12!@", true);
        //    }
        //    #endregion

        //    #region 2010 http Site
        //    if (testcases["HttpSite2010"] || isFullTest)
        //    {
        //        StartLoginTest("2010 http Site Test", "http://docavedemo-web.sharepoint.com", "admin@docavedemo.onmicrosoft.com", "1qaz2wsxE");
        //    }
        //    #endregion

        //    #region 2013 http Site
        //    if (testcases["HttpSite2013"] || isFullTest)
        //    {
        //        StartLoginTest("2013 http Site Test", "http://docaveonline2-public.sharepoint.com", "docave@docaveonline2.onmicrosoft.com", "1qaz2wsxE");
        //    }
        //    #endregion

        //    #region 2010 SharePoint ID
        //    if (testcases["SharePointID2010"] || isFullTest)
        //    {
        //        StartLoginTest("2010 SharePoint ID Test", "https://docavedemo.sharepoint.com/sites/BiCheng", "admin@docavedemo.onmicrosoft.com", "1qaz2wsxE");
        //    }
        //    #endregion

        //    #region 2013 SharePoint ID
        //    if (testcases["SharePointID2013"] || isFullTest)
        //    {
        //        StartLoginTest("2013 SharePoint ID Test", "https://avepointbeta.sharepoint.com/sites/Daniel", "pm@avepointbeta.onmicrosoft.com", "1qaz2wsxE");
        //    }
        //    #endregion

        //    #region 2010 Live ID
        //    if (testcases["LiveID2010"] || isFullTest)
        //    {
        //        StartLoginTest("2010 Live ID Test", "https://vvdemo.sharepoint.com/sites/us lw02", "daniel.han@avepoint.com", "demo12!@");
        //    }
        //    #endregion

        //    #region 2013 Live ID
        //    if (testcases["LiveID2013"] || isFullTest)
        //    {
        //        StartLoginTest("2013 Live ID Test", "https://avepointbeta.sharepoint.com/sites/Daniel", "killerhx@126.com", "demo12!@");
        //    }
        //    #endregion

        //    #region 2010 Local ADFS
        //    if (testcases["LocalADFS2010"] || isFullTest)
        //    {
        //        StartLoginTest("2010 Local ADFS Test", "https://docavedemo.sharepoint.com/sites/BiCheng", "daniel.han@adfstest.docaveonline.com", "demo12!@");
        //    }
        //    #endregion

        //    #region 2013 Local ADFS
        //    if (testcases["LocalADFS2013"] || isFullTest)
        //    {
        //        StartLoginTest("2013 Local ADFS Test", "https://avepointbeta.sharepoint.com/sites/Daniel", "daniel.han@adfstest13.docaveonline.com", "demo12!@");
        //    }
        //    #endregion

        //    #region 2010 Local ADFS Scan
        //    if (testcases["LocalADFS2010Scan"] || isFullTest)
        //    {
        //        StartLoginTest("2010 Local ADFS Scan Test", "https://docavedemo-admin.sharepoint.com/_layouts/online/SiteCollections.aspx", "daniel.han@adfstest.docaveonline.com", "demo12!@");
        //    }
        //    #endregion

        //    #region 2013 Local ADFS Scan
        //    if (testcases["LocalADFS2013Scan"] || isFullTest)
        //    {
        //        StartLoginTest("2013 Local ADFS Scan Test", "https://avepointbeta-admin.sharepoint.com/_layouts/online/SiteCollections.aspx", "daniel.han@adfstest13.docaveonline.com", "demo12!@");
        //    }
        //    #endregion
        //    LoginTestWaiter.WaitOne();
        //    Console.WriteLine("Test finished");
        //    Console.ReadLine();
        //}

        //static void WindowsLiveIdLoginTest()
        //{
        //    //string url = "https://vvdemo.sharepoint.com/sites/us lw02";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "daniel.han@avepoint.com", Password = "demo12!@" };
        //    //string url = "https://wne3tw.sharepoint.com/sites/test01";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "daniel.han@avepoint.com", Password = "demo12!@" };
        //    //string url = "https://avepointbeta.sharepoint.com/sites/Daniel";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "killerhx@126.com", Password = "demo12!@" };
        //    //string url = "https://docavedemo.sharepoint.com/sites/BiCheng";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    string url = "https://avepointbeta.sharepoint.com/sites/Daniel";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "pm@avepointbeta.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    //using (ClientContext context = new ClientContext(url))
        //    //{
        //    //    SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //    //    object obj = au.Login(user.UserName, user.Password);
        //    //    mCookieContainer = obj as CookieContainer;
        //    //    context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);
        //    //    Web w = context.Web;
        //    //    context.Load(w);
        //    //    context.Load(w, web => web.Lists);
        //    //    context.ExecuteQuery();
        //    //}
        //    //string url = "https://docavedemo.sharepoint.com/sites/BiCheng";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    //SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //    //object obj = au.Login(user.UserName, user.Password);
        //}

        //static void CreateSubDiscussionTest()
        //{
        //    string url = "https://avepointbeta.sharepoint.com/sites/Daniel/DestSub1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "pm@avepointbeta.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    try
        //    {
        //        using (ClientContext context = new ClientContext(url))
        //        {
        //            SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //            object obj = au.Login(user.UserName, user.Password);
        //            mCookieContainer = obj as CookieContainer;
        //            context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //            Web w = context.Web;
        //            context.Load(w);
        //            context.Load(w, web => web.Lists);
        //            context.ExecuteQuery();
        //            List l = w.Lists.GetByTitle("task1");

        //            context.Load(l);
        //            context.Load(l, list => list.RootFolder);
        //            context.ExecuteQuery();

        //            //ListItem discussion = Utility.CreateNewDiscussion(context, l, "test");
        //            //discussion.Update();
        //            //context.ExecuteQuery();
        //            //ListItem item = new ListItem(context, new ObjectPathStaticMethod(context, "{16f43e7e-bf35-475d-b677-9dc61e549339}", "CreateNewDiscussion", new object[] { l, "taskCreatedCtor" }));
        //            //item.Update();
        //            //context.ExecuteQuery();
        //            CamlQuery query = new CamlQuery();
        //            query.FolderServerRelativeUrl = l.RootFolder.ServerRelativeUrl;
        //            ListItemCollection items = l.GetItems(query);
        //            context.Load(items);
        //            context.ExecuteQuery();
        //            ListItem item = items[0];
        //            context.Load(item);
        //            //context.Load(item, i => i.Folder);
        //            context.ExecuteQuery();
        //            //ListItem subItem = new ListItem(context, new ObjectPathStaticMethod(context, "{16f43e7e-bf35-475d-b677-9dc61e549339}", "CreateNewDiscussion", new object[] { l, "test\\taskCreatedCtor" }));
        //            //subItem.Update();
        //            //context.ExecuteQuery();
        //            //query.FolderServerRelativeUrl = item.Folder.ServerRelativeUrl;
        //            ////items = l.GetItems(query);
        //            ////context.Load(items);
        //            ////context.ExecuteQuery();
        //            //Folder folder = item.Folder.Folders.Add("test111");
        //            //folder.Update();
        //            //context.ExecuteQuery();
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //static void AddRelatedItemTest()
        //{
        //    string url = "https://avepointbeta.sharepoint.com/sites/Daniel/DestSub1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "pm@avepointbeta.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    using (ClientContext context = new ClientContext(url))
        //    {
        //        SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //        object obj = au.Login(user.UserName, user.Password);
        //        mCookieContainer = obj as CookieContainer;
        //        context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //        Web w = context.Web;
        //        context.Load(w);
        //        context.Load(w, web => web.Lists);
        //        context.ExecuteQuery();
        //        List l = w.Lists.GetByTitle("task1");
        //        context.Load(l);
        //        context.Load(l, list => list.RootFolder);
        //        context.ExecuteQuery();
        //        CamlQuery query = new CamlQuery();
        //        query.FolderServerRelativeUrl = l.RootFolder.ServerRelativeUrl;
        //        ListItemCollection items = l.GetItems(query);
        //        context.Load(items);
        //        context.ExecuteQuery();
        //        ListItem item = items[0];
        //        context.Load(item);
        //        context.ExecuteQuery();
        //    }
        //}

        //static void DeleteListTest()
        //{
        //    string url = "https://docavedemo.sharepoint.com/sites/BiCheng";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    using (ClientContext context = new ClientContext(url))
        //    {
        //        SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //        object obj = au.Login(user.UserName, user.Password);
        //        mCookieContainer = obj as CookieContainer;
        //        context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //        Web w = context.Web;
        //        context.Load(w);
        //        context.Load(w, web => web.Lists);
        //        context.ExecuteQuery();
        //        List l = w.Lists.GetByTitle("Shared Documents");
        //        context.Load(l);
        //        context.ExecuteQuery();
        //    }
        //}

        //static void ExceptionHandlingScopeTest()
        //{
        //    List l = null, l1 = null, l2;
        //    try
        //    {
        //        string url = "https://docavedemo.sharepoint.com/sites/BiCheng";
        //        AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //        using (ClientContext context = new ClientContext(url))
        //        {
        //            SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //            object obj = au.Login(user.UserName, user.Password);
        //            mCookieContainer = obj as CookieContainer;
        //            context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //            Web w = context.Web;
        //            context.Load(w);
        //            context.Load(w, web => web.Lists);
        //            context.ExecuteQuery();
        //            l = w.Lists.GetByTitle("Shared Documents");
        //            context.Load(l);
        //            ExceptionHandlingScope excepScope = new ExceptionHandlingScope(context);
        //            using (excepScope.StartScope())
        //            {
        //                using (excepScope.StartTry())
        //                {
        //                    l1 = w.Lists.GetByTitle("Shared Documents2");
        //                    context.Load(l1);
        //                }
        //                using (excepScope.StartCatch())
        //                {
        //                    l2 = w.Lists.GetByTitle("Shared Documents");
        //                    context.Load(l2);
        //                    Console.WriteLine("111");
        //                }
        //            }
        //            context.ExecuteQuery();
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //static void AddAttachmentTest()
        //{
        //    string url = "https://docavedemo.sharepoint.com/sites/BiCheng";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    using (ClientContext context = new ClientContext(url))
        //    {
        //        SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //        object obj = au.Login(user.UserName, user.Password);
        //        mCookieContainer = obj as CookieContainer;
        //        context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //        Web web = context.Web;
        //        context.Load(web);
        //        context.Load(web, w => w.Lists);
        //        context.ExecuteQuery();
        //        List list = web.Lists.GetByTitle("AttachmentTestList");
        //        context.Load(list);

        //        //AveWebServiceRequest webServiceRequest = new AveWebServiceRequest(url, user, obj, "15.0.0.0");

        //        //webServiceRequest.AddAttachmentNow(web.ServerRelativeUrl, "AttachmentTestList", 1, "test.txt", Encoding.UTF8.GetBytes("12345"));
        //    }
        //}

        //static void WebRequestTest()
        //{
        //    AveHttpWebRequest webRequest = new AveHttpWebRequest();
        //    webRequest.GetRequestStream();
        //}

        //static void ClientAPITest()
        //{
        //    try
        //    {
        //        string url = "https://docavedemo.sharepoint.com/sites/BiCheng";

        //        AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "admin@docavedemo.onmicrosoft.com", Password = "1qaz2wsxE" };
        //        using (ClientContext context = new ClientContext(url))
        //        {
        //            SPOnlineAuthentication au = new SPOnlineAuthentication(url);
        //            object obj = au.Login(user.UserName, user.Password);
        //            mCookieContainer = obj as CookieContainer;
        //            context.ExecutingWebRequest += new EventHandler<WebRequestEventArgs>(cc_ExecutingWebRequest);

        //            Site site = context.Site;
        //            //int i = 0;
        //            //while (i < 500)
        //            //{
        //            //    int tt = i;
        //            //    Thread t = new Thread(new ThreadStart(() => { LoadSiteThread(context, site, tt); }));
        //            //    t.Start();
        //            //    Thread.Sleep(10);
        //            //}
        //        }
        //        Thread.Sleep(30000000);
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}

        //static void LoadSiteThread(ClientContext context, Site site, int count)
        //{
        //    try
        //    {
        //        Console.WriteLine("第 " + count + " 次请求");
        //        context.Load(site);
        //        context.ExecuteQuery();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("exception");
        //    }
        //}

        //static void TestOffice366Http10()
        //{
        //    string siteUrl = "http://ouou-web.sharepoint.com";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "aa.susan@ouou.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    Console.WriteLine(site.ID);

        //    siteUrl = "https://office365test10.sharepoint.com/sites/browsetest";
        //    user = new AveBPOSAccountInfo() { Domain = "", UserName = "jeff@office365test10.onmicrosoft.com", Password = "1qaz2wsxE" };
        //    factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    site = factory.CreateSite(siteUrl);
        //    Console.WriteLine(site.ID);
        //}

        //static void TestOffice366Http()
        //{
        //    string siteUrl = "http://avebeta-public.sharepoint.com";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "susan.li@avebeta.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    Console.WriteLine(site.ID);
        //}

        //static void TestWave15()
        //{
        //    //AvePoint.ObjectModel.ClientOM.AveClientOM15Request request = new AvePoint.ObjectModel.ClientOM.AveClientOM15Request(null, null, 0, null, null);

        //    string siteUrl = "https://Avepointbeta.sharepoint.com";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "pm@avepointbeta.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/des2");
        //}

        //static void TestNumberField()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/des2";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/des2");
        //    IAveList list = web.Lists["Long Running Operation Status"];
        //    IAveField filed = list.Fields[new Guid("{25acb982-7009-4866-8c2c-721b024bcb4b}")];
        //}

        //static void TestGetFolderUserData()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/des2";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/des2");
        //    IAveFolder folder = web.GetFolder("/sites/des2/Style Library/zh-cn/Themable");
        //    Console.WriteLine(folder.Item["Modified"]);
        //}

        //static void TestGetDocInfo()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    IAveFolder file = web.GetFolder("/sites/saasca1/_catalogs/masterpage/ar-sa/Preivew Images");
        //    AveBaseItemInfo itemInfo = new AveBaseItemInfo();
        //    IAveList list = web.GetCatalog(AveListTemplateType.MasterPageCatalog);
        //    itemInfo.RowId = file.Item.ID;
        //    itemInfo.Version = 512;
        //    IAveItem item = factory.CreateAveItem(itemInfo, file.ParentFolder, web, list);
        //    Console.WriteLine(item.GetDocInfo(itemInfo, null));
        //}

        //static void TestGetFolderInfo()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    AveBaseItemInfo itemInfo = new AveBaseItemInfo();
        //    IAveList list = web.GetCatalog(AveListTemplateType.MasterPageCatalog);
        //    IAveFolder folder = list.RootFolder.SubFolders[0];
        //    itemInfo.RowId = folder.Item.ID;
        //    itemInfo.Version = 512;
        //    IAveItem item = factory.CreateAveItem(itemInfo, list.RootFolder, web, list);
        //    Dictionary<string, object> docInfo = item.GetDocInfo(itemInfo, null);
        //}


        //static void TestUserInfoList()
        //{
        //    WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = true;
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveList list = site.RootWeb.SiteUserInfoList;
        //    IAveDiscoveryQuery discoveryQuery = factory.CreateDiscoveryQuery(site, DiscoverModule.Item);
        //    AveSiteCache siteCache = new AveSiteCache(site, factory, DiscoverModule.Item);
        //    AveWebCache webCache = new AveWebCache(siteCache, site.RootWeb.ID, site.RootWeb);
        //    AveFolderCache folderCache = new AveFolderCache(webCache, list.ID);
        //    AveItemObject itemObject = new AveItemObject();
        //    itemObject.FullUrl = list.RootFolder.ServerRelativeUrl;
        //    discoveryQuery.QueryListItemForFB(folderCache, itemObject, true, false);
        //    Console.ReadLine();
        //}

        //static void TestWebpart1()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    IAveFile page = web.GetFile("/sites/saasca1/_catalogs/masterpage/PeopleSearchResults.aspx");
        //    IAveLimitedWebPartManager wpManager = page.GetLimitedWebPartManager(PersonalizationScope.Shared);
        //    IAveLimitedWebPartCollection webparts = wpManager.WebParts;
        //}

        //static void TestContentTypeFieldSchemaBackup()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    for (int i = 0; i < web.ContentTypes.Count; i++)
        //    {
        //        string fieldSchema = web.ContentTypes[i].Fields.SchemaXml;
        //        string replacedSchema = web.ContentTypes[i].Fields.TransListIdToTitle(web, null, fieldSchema);
        //    }
        //}

        //static void TestContentTypeXml()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    IAveContentType ct = web.AvailableContentTypes["Reusable HTML"];
        //    Console.WriteLine(ct.SchemaXml);
        //}

        //static void TestGetFolder()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1/testNav");
        //    IAveFolder folder = web.GetFolder("/sites/saasca1/testNav/_catalogs/masterpage");
        //    Console.WriteLine(folder.Properties);
        //    if (folder.Item != null)
        //    {
        //        string str = folder.Item["MetaInfo"] as string;
        //        Dictionary<string, string> dicMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(str);
        //        if (dicMetaInfo.ContainsKey("vti_setuppath"))
        //        {
        //        }
        //    }
        //}

        //static void TestGetItem()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1");
        //    IAveList list = web.Lists["Tasks"];
        //    IAveListItem item = list.GetItemById(1);
        //    Console.WriteLine(item["Modified"]);
        //}

        //static void TestGetFile()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.OpenWeb("/sites/saasca1/testNav");
        //    IAveFile file = web.GetFile("/sites/saasca1/testNav/_catalogs/masterpage/default.master");
        //    Console.WriteLine(file.Properties);
        //}

        //static void TestItemUniqueId()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveList list = site.RootWeb.Lists["Cache Profiles"];
        //    IAveListItem item = list.GetItemById(1);
        //    Console.WriteLine(item.UniqueId);
        //    string str = item["MetaInfo"] as string;
        //    Dictionary<string, string> dicMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(str);
        //    if (dicMetaInfo.ContainsKey("vti_setuppath"))
        //    {
        //    }
        //}

        //static void TestDiscovery()
        //{
        //    WrapperConfiguration.BPOS_S.IncludeVersionForPerformance = true;
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveList list = site.RootWeb.Lists["Tasks"];
        //    IAveDiscoveryQuery discoveryQuery = factory.CreateDiscoveryQuery(site, DiscoverModule.Item);
        //    AveSiteCache siteCache = new AveSiteCache(site, factory, DiscoverModule.Item);
        //    AveWebCache webCache = new AveWebCache(siteCache, site.RootWeb.ID, site.RootWeb);
        //    AveFolderCache folderCache = new AveFolderCache(webCache, list.ID);
        //    AveItemObject itemObject = new AveItemObject();
        //    itemObject.FullUrl = list.RootFolder.ServerRelativeUrl;
        //    discoveryQuery.QueryListItemForFB(folderCache, itemObject, true, false);
        //    Console.ReadLine();
        //}

        //static void TestFields()
        //{
        //    string siteUrl = "https://avepointqa3.sharepoint.com/sites/saasca1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "AvePointQA3@AvePointQA3.onmicrosoft.com", Password = "demo12!@" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.RootWeb;
        //    IAveFieldCollection fields = web.Fields;
        //    AveFieldCollectionInfo fieldsInfo = fields.GetFieldInfoObj();
        //    Console.WriteLine(fieldsInfo.AveSchemaXml);
        //}

        //static void TestSite()
        //{
        //    string siteUrl = "http://client3wdz:9200/sites/bposdst";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "jiang\\wdz", Password = "1qaz2wsxE" };
        //    //string siteUrl = "https://Jeff:12024/sites/destsite4";
        //    //AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = ".", UserName = "user10", Password = "!Passw0rd" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.RootWeb;
        //    IAveList list = web.GetList(AveUrlUtility.GetServerRelativeUrl(siteUrl));
        //    IAveFolder folder = list.RootFolder;
        //    IAveFolderCollection focol = folder.SubFolders;
        //    Console.WriteLine(focol.Count);
        //}

        //static void TestWeb()
        //{
        //    string siteUrl = "http://avepoint-lxzou:9000/sites/site1";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "", UserName = "dlbranch\\lxzou", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.RootWeb;
        //    foreach (IAveWeb subWeb in web.Webs)
        //    {
        //        Console.WriteLine(subWeb.ID);
        //    }
        //}

        //static void TestContentType()
        //{
        //    string siteUrl = "http://sjcao:12029";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "sp10", UserName = "administrator", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.RootWeb;
        //    IAveContentType contentType = factory.CreateContentType(factory.CreateContentTypeId(AveBuiltInContentTypeId.Document), web.ContentTypes, "custom0");
        //    Console.WriteLine(contentType.ID);
        //}

        //static void Test()
        //{
        //    Stopwatch myWatch = new Stopwatch();
        //    myWatch.Start();

        //    string siteUrl = "http://www.zlxtest.com:9000";//http://10.2.5.127:9000/sites/cbi";//"https://www.avepoint.net/";////";////
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = ".", UserName = "dlbranch\\lxzou", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    AveAPIType type = site.GetAPIType();
        //    //IAveWeb subweb = site.OpenWeb("http://10.2.5.127:9000/sites/cbi/test1");
        //    IAveWeb web = site.OpenWeb("testSite1");
        //    //IAveFile text_file = web.GetFile("/sites/cbi/DocLibrary/item11.txt");
        //    IAveList list1 = web.Lists["Tasks"];
        //    IAveListItem item1 = list1.Items[0];
        //    IAveAttachmentCollection coll = item1.Attachments;
        //    foreach (IAveList list in web.Lists)
        //    {
        //        Console.WriteLine(list.Title + "\t list");
        //        Console.WriteLine("list fields:");
        //        foreach (IAveField field in list.Fields)
        //        {
        //            Console.WriteLine("\t" + field.ID + "\t" + field.StaticName);
        //        }
        //        foreach (IAveListItem item in list.Items)
        //        {
        //            Console.WriteLine("\t" + "item fields:");
        //            foreach (IAveField f in item.ContentType.Fields)
        //            {
        //                Console.WriteLine("\t\t" + f.ID + "\t" + f.StaticName);
        //            }
        //            if (item.FileSystemObjectType == AveFileSystemObjectType.File)
        //            {
        //                IAveFile file = item.File;
        //                Console.WriteLine("\t\t" + file.Title + "\t" + file.Name + "\t file");
        //            }
        //            if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
        //            {
        //                IAveFolder folder = item.Folder;
        //                Console.WriteLine("\t\t" + folder.Name + "\t folder");
        //                foreach (IAveFile i in folder.Files)
        //                {
        //                    Console.WriteLine("\t\t\t" + i.Title + "\t" + i.Name + "\t file");
        //                }
        //                foreach (IAveFolder f in folder.SubFolders)
        //                {
        //                    Console.WriteLine("\t\t\t" + f.Name + "\t folder");
        //                }
        //            }
        //        }
        //    }
        //    myWatch.Stop();
        //    Console.WriteLine(myWatch.ElapsedMilliseconds + "\t毫秒");
        //}

        //static void TestWebPart()
        //{
        //    string siteUrl = "http://sjcao:12029";
        //    AveBPOSAccountInfo user = new AveBPOSAccountInfo() { Domain = "sp10", UserName = "administrator", Password = "1qaz2wsxE" };
        //    AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    IAveSite site = factory.CreateSite(siteUrl);
        //    IAveWeb web = site.RootWeb;
        //    IAveFile file = web.GetFile("/doc/Forms/AllItems.aspx");
        //    IAveLimitedWebPartManager webpartManager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);

        //    //List<AveWebPartBaseInfo> webpartBaseInfoList = webpartManager.GetWebParts();
        //    //webpartManager.RestoreWebParts(webpartBaseInfoList);

        //}
    }
}
