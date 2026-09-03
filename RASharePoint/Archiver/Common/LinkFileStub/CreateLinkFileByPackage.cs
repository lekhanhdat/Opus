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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.PhysicalCore.SQL;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.DisposalStub;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.SharePoint.Archiver.Move;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using HSMAzureCommon;
using HSMCommon;
using HSMCommon.DeploymentXML;
using Microsoft.SharePoint.Client;
using Microsoft365.SharePoint;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;
using SPObjectType = HSMCommon.DeploymentXML.SPObjectType;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class CreateLinkFileByPackage : IDisposable
    {
        #region private fields

        private static Dictionary<ScheduleConfiguration, CreateLinkFileByPackage> instanceMapping = new ();
        private static readonly object padlock = new object();
        private static readonly object fieldslock = new object();

        WinAzure mAzureInfo = new WinAzure();
        List<string> mBuildinRoleDefinations = new List<string>() { "1073741825", "1073741826", "1073741827", "1073741828", "1073741829", "1073741830", "1073741924" };
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        DeploymentUserGroupMap mUserGroupMap = new DeploymentUserGroupMap();
        private List<int> mUserGroupMappingForCurrentPackage = new List<int>();

        string tempContentPath = string.Empty;
        string tempManifestPath = string.Empty;
        string tempBaseJobPath = string.Empty;
        string tempJobPath = string.Empty;

        private int mCurrentPackageCountCapacity = 0;
        private int mCurrentPackageSizeCapacity = 0;
        private List<ARRestoreFileInfo> mCurrentPackageIdList = new List<ARRestoreFileInfo>();
        private static int DefaultLCID = -1;
        private Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");

        private ScheduleConfiguration mConfig;
        public Dictionary<Guid, ImportJobResources> mAllJobStatus = new Dictionary<Guid, ImportJobResources>();
        private AveMultiReceiver multiReceiver = null;
        string fileNameSUFFIX = string.Empty;

        private SPGenericObjectCollection mSPObjectCollection { get; set; }
        //private SPGenericObject mSPObject { get; set; }
        //private SPGenericObject mSPFileObject { get; set; }
        private SPGenericObject mParentSPFolderObject { get; set; }
        private SPGenericObject mRoleDefinitionObject { get; set; }
        private SPGenericObject mRoleAssignmentsObject { get; set; }

        protected List<SPGenericObject> mCacheSPFolderObjects = new List<SPGenericObject>();

        #region sharepoint object

        private IAveList mAveList { get; set; }
        #endregion
        protected Dictionary<Guid, SPLookupList> mSPLookupListCollection = new Dictionary<Guid, SPLookupList>();
        protected SPLookupLists mSPLookupLists
        {
            get;
            set;
        }

        protected SPLookupList mSPLookupList
        {
            get;
            set;
        }

        private Dictionary<string, MetadataCacheInfo> mMetadataInfoList = new Dictionary<string, MetadataCacheInfo>();
        private CGDBReader dbReader;
        #endregion

        #region public fields
        public Guid AveSPSiteId { get; private set; }
        public Guid AveSPWebId { get; private set; }
        public Guid AveSPListId { get; private set; }
        public Guid AveSPFolderId { get; private set; }
        public string CurrentFolderUrl { get; private set; }

        public bool NeedReInitSPObject { get; private set; }
        #endregion

        #region ctor
        private CreateLinkFileByPackage(ScheduleConfiguration config)
        {
            mConfig = config;
            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery)
            {
                dbReader = CGDBReader.GetInstance(config.ArchiverExtendSetting, config.SiteCollectionID.ToString(), config.SiteCollectionUrl);
            }
        }

        public void Init()
        {
            mSPObjectCollection = new SPGenericObjectCollection();
            mSPLookupLists = new SPLookupLists();
            tempBaseJobPath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, mConfig.JobId);
            if (!Directory.Exists(tempBaseJobPath))
            {
                Directory.CreateDirectory(tempBaseJobPath);
            }
            tempJobPath = Path.Combine(tempBaseJobPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(tempJobPath))
            {
                Directory.CreateDirectory(tempJobPath);
            }
            fileNameSUFFIX = LinkFileCommon.GetStubFileNameSuffix(mConfig);
            InitTaskManager();
            mAveList = null;
        }

        public static CreateLinkFileByPackage GetInstance(ScheduleConfiguration config)
        {
            lock (padlock)
            {
                if (!instanceMapping.ContainsKey(config))
                {
                    instanceMapping.Add(config, new CreateLinkFileByPackage(config));
                }
                return instanceMapping[config];
            }
        }
        #endregion

        #region public methods

        public void ResetList(IAveList list)
        {
            lock (padlock)
            {
                if (mAveList == null || mAveList.ID != list.ID)
                {
                    SplitPackage(true);
                    mAveList = list;
                    ImportJobResources importJobResources = new ImportJobResources();
                    if (!mAllJobStatus.ContainsKey(mAveList.ID))
                    {
                        mAllJobStatus.Add(mAveList.ID, importJobResources);
                    }
                    try
                    {
                        if (list.ParentWeb.Site.Features[mRecordFeatureId] == null)
                        {
                            list.ParentWeb.Site.Features.Add(mRecordFeatureId, true);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                    }
                    using (var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig))
                    {
                        LinkFileCommon.AddLinkField2List(list);
                    }
                    using (new AvePerformanceScope("ProcessRoleDefinitionsXML"))
                    {
                        ProcessRoleDefinitionsXML(list.ParentWeb);
                    }
                    using (new AvePerformanceScope("ProcessListRootFolderXML"))
                    {
                        ProcessListRootFolderXML();
                    }
                    using (new AvePerformanceScope("InitContainerInfo"))
                    {
                        InitContainerInfo(mAzureInfo);
                    }
                    //Process List XML;
                    using (new AvePerformanceScope("ProcessExportSettingsXml"))
                    {
                        ProcessExportSettingsXml();
                    }
                    using (new AvePerformanceScope("ProcessRequirementsXml"))
                    {
                        ProcessRequirementsXml();
                    }
                    using (new AvePerformanceScope("ProcessRootObjectMapxml"))
                    {
                        ProcessRootObjectMapxml();
                    }
                    using (new AvePerformanceScope("ProcessViewFormsListXML"))
                    {
                        ProcessViewFormsListXML();
                    }
                    using (new AvePerformanceScope("ProcessSystemDataXML"))
                    {
                        ProcessSystemDataXML();
                    }
                }
            }
        }
        public void ProcessDocument(IAveFile aveFile, string desUrl, string filePath, long reportSize, ProcessStubfileContent psc, string md5 = null)
        {
            string newLeafName = GetStubFileName(aveFile);
            int filenum = Interlocked.Increment(ref MIImportConstant.FileValue);
            byte[] mStubBytes = LinkFileCommon.GetFileContent(aveFile.ParentFolder.ParentList.ParentWeb.LanguageCulture, psc);
            using (AvePerformanceScope performanceRestore = new AvePerformanceScope("ArchiverDeletion.LeaveDocumentLinkFile.Restore"))
            {
                using (RecordManagerFileReceiver fileReceiver = new RecordManagerFileReceiver(filePath))
                {
                    using (IAveRestoreStream importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                    {
                        var mMetadataGenerator = ReadMetadataCacheFromStream(importStream);
                        var userData = UpdateMetadata(mMetadataGenerator.GetData<Dictionary<string, object>>(AveMetadataType.DocData.ToString()));
                        var dataJunction = mMetadataGenerator.GetData<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction.ToString());
                        var roleAssignments = mMetadataGenerator.GetData<List<AveRoleAssignmentInfo>>(AveMetadataType.RoleAssignment.ToString());

                        string key = aveFile.UniqueId.ToString();
                        AveSPDoc aveDoc = new AveSPDoc(mConfig.StubRestoreAveSPRootFolder, newLeafName);
                        importStream.Reset();
                        SPFile file = null;
                        using (new AvePerformanceScope("GenerateFileNode"))
                        {
                            string itemid = Guid.NewGuid().ToString();
                            string sourcekey = aveFile.UniqueId.ToString();
                            file = GenrateFileNode(aveFile, userData, dataJunction, itemid.ToString(), filenum);
                        }
                        //if ((file.ParentWebUrl.TrimStart('/') + "/" + file.Url).Length > 400 || newLeafName.Length > 256)
                        //{
                        //    mLog.Info("The specified file or folder name is too long.");
                        //    throw new Exception("The specified file or folder name is too long.");
                        //}

                        SPListItem item = null;
                        using (new AvePerformanceScope("GenerateItemNode"))
                        {
                            item = GenrateItemNode(aveDoc.AveSPItem, aveFile, userData, dataJunction, file.Id);
                            item.Id = file.Id;
                            item.DocId = file.Id;
                        }
                        if (item.Author != file.Author)
                        {
                            file.Author = item.Author;
                        }
                        if (item.TimeLastModified != file.TimeLastModified)
                        {
                            file.TimeLastModified = item.TimeLastModified;
                        }
                        if (file.TimeLastModified.Year <= 1900)
                        {
                            file.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                            item.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                        }
                        if (item.TimeCreated != file.TimeCreated)
                        {
                            file.TimeCreated = item.TimeCreated;
                        }
                        if (file.TimeCreated.Year <= 1900)
                        {
                            file.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                            item.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                        }
                        if (item.ModifiedBy != file.ModifiedBy)
                        {
                            file.ModifiedBy = item.ModifiedBy;
                        }
                        //ADO-185528 need to support check in comment.
                        if (!string.IsNullOrEmpty(aveFile.CheckInComment))
                        {
                            file.CheckinComment = aveFile.CheckInComment;
                        }
                        using (new AvePerformanceScope("ProcessFileObjectNode"))
                        {
                            ProcessFileObjectNode(aveFile, userData, dataJunction, file);
                        }
                        using (new AvePerformanceScope("ProcessListItemNode"))
                        {
                            string dataPath = string.Empty;
                            ProcessListItemNode(aveFile, userData, dataJunction, item, roleAssignments, out dataPath);
                            SaveStubStream(mStubBytes, filenum, dataPath);
                        }

                        CreateLinkFileReportDto report = new CreateLinkFileReportDto();
                        report.FileUrl = WebUtil.MakeFullUrl(aveFile.ParentFolder.ParentWeb.Site.Url, aveFile.ServerRelativeUrl);
                        report.Md5 = md5;
                        mAllJobStatus[mAveList.ID].AddReports(aveFile.UniqueId.ToString(), report);
                        ARRestoreFileInfo fileinfo = new ARRestoreFileInfo();
                        try
                        {
                            fileinfo.id = key;
                            fileinfo.rowid = aveFile.Item.ID;
                            fileinfo.name = aveFile.Name;
                            fileinfo.serverRelativeUrl = aveFile.ServerRelativeUrl;
                            fileinfo.size = reportSize;
                            fileinfo.TotalSize = GetFileTotalSize(aveFile);
                            fileinfo.AuthorID = aveFile.Author.ID;
                            fileinfo.AuthorEmail = aveFile.Author.Email;
                            fileinfo.ModifiedID = aveFile.ModifiedBy.ID;
                            fileinfo.ModifiedEmail = aveFile.ModifiedBy.Email;
                            fileinfo.CreateTime = aveFile.TimeCreated.Year.ToString() + aveFile.TimeCreated.Month.ToString("00");
                            fileinfo.ModifiedTime = aveFile.TimeLastModified.Year.ToString() + aveFile.TimeLastModified.Month.ToString("00");
                            fileinfo.VersionCount = aveFile.Versions.Count;
                            fileinfo.StubId = psc.StubId;
                        }
                        catch (Exception e)
                        {
                            fileinfo.AuthorEmail = string.Empty;
                            fileinfo.ModifiedEmail = string.Empty;
                            fileinfo.CreateTime = string.Empty;
                            fileinfo.ModifiedTime = string.Empty;
                            mLog.Error($"generate fileinfo failed,because {e.ToString()}");
                        }
                        lock (padlock)
                        {
                            mCurrentPackageIdList.Add(fileinfo);
                            mCurrentPackageCountCapacity++;
                        }

                    }
                }
            }
        }
        private long GetFileTotalSize(IAveFile aveFile)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetFileTotalSize"))
            {
                long fileTotalSize = 0;
                if (aveFile != null && aveFile.Item != null && aveFile.Item.Fields != null && aveFile.Item.FieldValues.ContainsKey("SMTotalSize"))
                {
                    try
                    {
                        string totalSize = aveFile.Item.FieldValues["SMTotalSize"].ToString();
                        if (totalSize.IndexOf(";#") != -1)
                        {
                            totalSize = totalSize.Substring(0, totalSize.IndexOf(";#"));
                        }
                        fileTotalSize = Convert.ToInt64(totalSize);
                    }
                    catch (Exception ex)
                    {
                        mLog.Info($"Can't get file total size.FileId:{aveFile.UniqueId}.Message:{ex}.");
                        fileTotalSize = aveFile.Length;
                    }
                }
                return fileTotalSize;
            }
        }
        public void SplitPackage(bool isLastPackage = false)
        {
            lock (mSPObjectCollection)
            {
                if (isLastPackage && mSPObjectCollection.SPObject.Count > 0)
                {
                    if (mAveList != null)
                    {
                        mLog.Info("Start SplitPackage. isLastPackage: {0}", isLastPackage.ToString());
                        //using (new AvePerformanceScope("ProcessUserGroupXML"))
                        //{
                        //    ProcessUserGroupXML();
                        //}
                        CopyMultiFileToFolder(tempJobPath, tempManifestPath, true);
                        StorageManifest(tempManifestPath);
                        StorageLookupListMapXml(tempManifestPath);
                        StorageUserGroupXMLXml(tempManifestPath);
                        mLog.Info("Add Import Job Task. tempManifestPath: {0}.", tempManifestPath);
                        AddImportJobTask(mAveList.ParentWeb.Site, mAveList.ParentWeb.ID, mAveList.ParentWeb.Name, mAveList.ID, mAveList.Title, true, tempContentPath, tempManifestPath, mAzureInfo, false, mAveList.RootFolder.Url, mAveList.DefaultDisplayFormUrl, mAveList.BaseTemplate);
                        mLog.Info("End SplitPackage.");
                    }
                    mCurrentPackageCountCapacity = 0;
                    mCurrentPackageSizeCapacity = 0;
                    mSPObjectCollection.SPObject.Clear();
                    //GenerateCurrentFolderObjects();
                    //mCurrentPackageNumber = 0;
                }
                else
                {
                    if (mCurrentPackageCountCapacity == MIImportConstant.PackageCountCapacity || mCurrentPackageSizeCapacity >= MIImportConstant.PackageSizeCapacity)
                    {
                        if (mAveList != null)
                        {
                            mLog.Info("Start SplitPackage2. isLastPackage: {0}", isLastPackage.ToString());
                            //using (new AvePerformanceScope("ProcessUserGroupXML"))
                            //{
                            //    ProcessUserGroupXML();
                            //}
                            CopyMultiFileToFolder(tempJobPath, tempManifestPath, true);
                            StorageManifest(tempManifestPath);
                            StorageLookupListMapXml(tempManifestPath);
                            StorageUserGroupXMLXml(tempManifestPath);
                            mLog.Info("Add Import Job Task2.");
                            AddImportJobTask(mAveList.ParentWeb.Site, mAveList.ParentWeb.ID, mAveList.ParentWeb.Name, mAveList.ID, mAveList.Title, true, tempContentPath, tempManifestPath, mAzureInfo, false, mAveList.RootFolder.Url, mAveList.DefaultDisplayFormUrl, mAveList.BaseTemplate);
                            mLog.Info("End SplitPackage2.");
                        }

                        mCurrentPackageCountCapacity = 0;
                        mCurrentPackageSizeCapacity = 0;
                        mSPObjectCollection.SPObject.Clear();
                        GenerateCurrentFolderObjects();
                        UpdateContainerInfo();
                    }
                }
            }
        }

        public void WatingCompleted()
        {
            if (multiReceiver != null && !multiReceiver.scheduler.IsEmpty)
            {
                mLog.Info("Start Site Import.");
                multiReceiver.scheduler.Finish();
                multiReceiver.Wait();
                mLog.Info("End Site Import. ");
            }
        }

        #endregion

        #region private methods
        private string GetStubFileName(IAveFile file)
        {
            string stubFileName = string.Empty;

            stubFileName = file.Name + "." + fileNameSUFFIX;

            return stubFileName;
        }

        private Dictionary<string, object> UpdateMetadata(Dictionary<string, object> data)
        {
            try
            {
                if (data.ContainsKey("File_x0020_Type"))
                {
                    data["File_x0020_Type"] = fileNameSUFFIX;
                }
                //for ghost page.ADO-206213
                if (data.ContainsKey("SetupPath"))
                {
                    data.Remove("SetupPath");
                }
                if (data.ContainsKey("HasStream"))
                {
                    data["HasStream"] = 1;
                }
                //for picture library preview ADO-206271
                if (data.ContainsKey("PreviewExists"))
                {
                    data["PreviewExists"] = false;
                }
                if (data.ContainsKey("MediaServiceMetadata"))
                {
                    mLog.Info("Current file is leave link file and userData contains MediaServiceMetadata.MediaServiceMetadata:{0}.", data["MediaServiceMetadata"].ToString());
                    data.Remove("MediaServiceMetadata");
                }
                if (data.ContainsKey("MediaServiceFastMetadata"))
                {
                    mLog.Info("Current file is leave link file and userData contains MediaServiceFastMetadata.MediaServiceFastMetadata:{0}.", data["MediaServiceFastMetadata"].ToString());
                    data.Remove("MediaServiceFastMetadata");
                }
                //userData["URL"] = System.Web.HttpUtility.UrlEncode(linkAspxUrl).Replace("+", " ");
                // userData["#tp_ContentTypeId"] = linkContentType.ID.ToByteArray();

                return data;
            }
            catch (Exception ex)
            {
                mLog.Warn("RA not keep property error {0}", ex.ToString());
                return data;
            }
        }

        private bool SaveStubStream(byte[] stubStreamBytes, int filenum, string tempPath)
        {
            using (new AvePerformanceScope("Performance.SaveStream"))
            {
                string azureFileValue = string.Format("{0}.dat", filenum);
                string tempFilePath = Path.Combine(tempPath, azureFileValue);
                mLog.Info("tempFilePath:{0}", tempFilePath);
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(tempFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
                    }
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        fileStream.Write(stubStreamBytes, 0, stubStreamBytes.Length);
                        fileStream.Flush();
                    }
                    return true;
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while saving stream. Exception: {0}.", e.ToString());
                    return false;
                }
            }
        }
        private void AddImportJobTask(IAveSite site, Guid webId, string webName, Guid listId, string listTitle, bool isLast, string dataContainerDir, string manifestContainerDir, WinAzure azureInfo, bool isEncryption, string listUrl, string listDefaultDisplayFormUrl, AveListTemplateType listTemplateType, bool ismultiply = true)
        {
            lock (mAllJobStatus)
            {
                if (mAllJobStatus.ContainsKey(listId))
                {
                    mAllJobStatus[listId].JobCount++;
                    mAllJobStatus[listId].AddJobsFinished = isLast;
                    mAllJobStatus[listId].ListTitle = listTitle;
                    mAllJobStatus[listId].WebName = webName;
                }
                else
                {
                    ImportJobResources jobStatus = new ImportJobResources();
                    jobStatus.JobCount++;
                    jobStatus.AddJobsFinished = isLast;
                    mAllJobStatus[listId] = jobStatus;
                    mAllJobStatus[listId].ListTitle = listTitle;
                    mAllJobStatus[listId].WebName = webName;
                }
            }
            WinAzure temAzure = new WinAzure();
            temAzure.AccessPoint = azureInfo.AccessPoint;
            temAzure.AccountKey = azureInfo.AccountKey;
            temAzure.AccountName = azureInfo.AccountName;
            temAzure.AzureContainerManifestUri = azureInfo.AzureContainerManifestUri;
            temAzure.AzureContainerSourceUri = azureInfo.AzureContainerSourceUri;
            temAzure.AzureIused = azureInfo.AzureIused;
            temAzure.AzureManifestContainerName = azureInfo.AzureManifestContainerName;
            temAzure.AzureQueueReportContainerName = azureInfo.AzureQueueReportContainerName;
            temAzure.AzureQueueReportUri = azureInfo.AzureQueueReportUri;
            temAzure.AzureSourceContainerName = azureInfo.AzureSourceContainerName;
            temAzure.EndPointSuffixm = azureInfo.EndPointSuffixm;

            MutliImportParameter importParameter = new MutliImportParameter() { AzureInfo = temAzure, Site = site, WebId = webId, ListId = listId, ManifestContainerDir = manifestContainerDir, DataContainerDir = dataContainerDir, MigrationModuleType = MigrationModuleType.SPMigration, IsEncryption = isEncryption, IsNeedCheckSourceFilesUploaded = false, RetryMigrationJobTime = 60, CurrentRestoreFileIdsList = new List<ARRestoreFileInfo>(mCurrentPackageIdList) };

            //importParameter.Report.ListUrl = string.IsNullOrEmpty(listUrl) ? listTitle : listUrl;
            //importParameter.Report.ThreadIdentity = string.IsNullOrEmpty(listUrl) ? listTitle : listUrl.Split('/').Last();
            //importParameter.Report.PackageName = manifestContainerDir;
            //importParameter.Report.Location = Path.Combine(mConfig.scheduleDir, "PrimeReport.ave");
            //if (mConfig.IsFreeContainer)

            FreeContainerManager fcManager = new FreeContainerManager();
            importParameter.FCParameters = fcManager.CreateFreeContainers(site);
            importParameter.IsFreeContainer = true;


            AzureMultipleImport import = new AzureMultipleImport(importParameter, 1);

            PostActionDelegate postAction = new PostActionDelegate(ImportPostAction);
            import.updateErrorReportsEvent += new UpdateErrorReportsDelegate(this.UpdateErrorReports);
            import.sendJobReportEvent += new SendJobReportDelegate(this.SendJobReports);
            import.sendErrorJobReportEvent += new SendErrorJobReportDelegate(this.SendErrorJobReport);
            import.PostActionEvent = postAction;
            mLog.Info("The package ListUrl is {0}. Name is {1}.", listUrl, importParameter.AzureInfo.AzureManifestContainerName);
            multiReceiver.scheduler.AddTask(import);
            mCurrentPackageIdList.Clear();
        }

        private void ImportPostAction(MutliImportParameter multiImportParameter)
        {

        }

        private void UpdateErrorReports(Dictionary<string, AzureQueueMessage> ErrorItems, MutliImportParameter multiImportParameter)
        {
            using (new AvePerformanceScope("Event:UpdateErrorReports"))
            {
                if (ErrorItems.Count > 0)
                {
                    mLog.Error("ErrorItems Count:{0}", ErrorItems.Count);
                }
                using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                {
                    IAveList mList = mWeb.GetList(multiImportParameter.ListId);
                    foreach (var m in ErrorItems)
                    {
                        mLog.Error("Error File Url:{0}", m.Value.Url);
                        mLog.Error("Error Code:{0}", m.Value.ErrorCode);
                        mLog.Error("Message: {0}", m.Value.Message);
                        var reportMess = ArchiverCommonStaticMethod.GetMessageFromCallStack(m.Value.Message);
                        if (m.Value.ErrorCode == SPErrorCode.TP_E_DOCALREADYEXISTS.ToString()) // cannot get modified and editor from SPO error queueMessage
                        {
                            reportMess = "RM_PU_SkipItemMessage";
                        }
                        else if (m.Value.ErrorCode == SPErrorCode.TP_E_INVALIDFILENAME.ToString()) // 429 too many request also can meet this error
                        {
                            reportMess = string.Format(I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubPathInvalidOrBusy"), m.Value.Url);
                        }
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(m.Key))
                        {
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessage(m.Key, JobDetailsStatus.Failed, reportMess);
                        }
                        else
                        {
                            //mAllJobStatus[multiImportParameter.ListId].AddErrorFileUrl(m.Value.Url);
                            string url = m.Value.Url;
                            string fileSuffix = LinkFileCommon.GetStubFileNameSuffix(mConfig);
                            if (url.EndsWith(fileSuffix))
                            {
                                url = url.Substring(0, url.Length - fileSuffix.Length - 1);
                            }
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessageByUrl(url, JobDetailsStatus.Failed, reportMess);
                            IAveFile stubfile = mWeb.GetFile(m.Value.Url);
                            //1.出异常的Stub未生成，则不需要删除源文件。
                            if (!stubfile.Exists)
                            {
                                mLog.Warn("Stub does not exist when UpdateErrorReports.StubUrl: {0}.", m.Value.Url);
                            }
                            //2.出异常的Stub文件生成了，需要先删除异常Stub再删除源文件。
                            else
                            {
                                bool isStubMatch = false;
                                try
                                {
                                    if (stubfile.Item != null)
                                    {
                                        var archiverLinkFileType = stubfile.Item.FieldValues["ArchiverLinkFileType"];
                                        if (archiverLinkFileType != null)
                                        {
                                            isStubMatch = !string.IsNullOrEmpty(archiverLinkFileType.ToString());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("file not a stub,because it's fieldValues does not contain ArchiverLinkFileType,error:{0}", e.ToString());
                                }
                                if (isStubMatch)
                                {
                                    using (AvePerformanceScope pc2 = new AvePerformanceScope("FileLevelRetention.RemoveStubFromSharePoint.DeleteFile.StubMatch"))
                                    {
                                        try
                                        {
                                            DeleteStubFile(stubfile);
                                            mLog.Info("Delete exception stub file successfully. {0}", m.Value.Url);
                                        }
                                        catch (Exception exc)
                                        {
                                            mLog.Warn("Delete exception stub file has some error. Detail : {0}.", exc.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    mLog.Info($"The file is not a stub file. so don't delete it.");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendJobReports(MutliImportParameter multiImportParameter, bool isImportJobCanceled)
        {            //isImportJobCanceled = true;
            if (isImportJobCanceled)
            {
                mLog.Warn("Import Job Canceled");
                using var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig);
                SendJobReportsForCanceledJob(multiImportParameter);
            }
            else
            {
                List<ARRestoreFileInfo> mRRestoreFileInfos = new List<ARRestoreFileInfo>();
                using (new AvePerformanceScope("Event:SendJobReports"))
                {
                    using (var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig))
                    {
                        using IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId);
                        IAveList mList = mWeb.GetList(multiImportParameter.ListId);
                        foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                        {
                            if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                            {
                                mLog.Warn("[SendJobReports]This file status is Failed.");
                                mConfig.JobReportDto.HasErrorNode = true;
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(info.id), 50000, multiImportParameter.JobId);
                                continue;
                            }
                            mRRestoreFileInfos.Add(info);
                            if (mRRestoreFileInfos.Count >= 20)
                            {
                                BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter);
                                mRRestoreFileInfos.Clear();
                            }
                        }
                        if (mRRestoreFileInfos.Count != 0)
                        {
                            BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter);
                            mRRestoreFileInfos.Clear();
                        }
                    }

                    #region send report
                    foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                    {
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                        {
                            try
                            {
                                var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(info.size, mConfig.GetNodeFullPath(report.FileUrl));
                                JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(report.Status, mConfig?.currentRule?.Id, "SO_Action_LevelStub", (int)CacheNodeType.Item);
                                mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(report.FileUrl),
                                             info.size,
                                             report.Status,
                                             (int)CacheNodeType.Item,
                                             mConfig.JobId,
                                             mConfig.currentRule.Name,
                                             "",
                                             "SO_Action_LevelStub",
                                             report.Message,
                                             "");
                                var archiverExtendSetting = mConfig.ArchiverExtendSetting;
                                if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery && dbReader != null)
                                {
                                    dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), new Guid(info.id), ConvertToBackupRestoreStatus(report.Status), info.size, mConfig.ArchiverUNCTime);
                                }
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                            }
                            mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                            mConfig.ProgressDto.UpdateProgress();
                        }
                    }
                    #endregion
                }
            }
        }
        private BackupRestoreStatus ConvertToBackupRestoreStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    return BackupRestoreStatus.Succeed;
                case JobDetailsStatus.Failed:
                    return BackupRestoreStatus.Failed;
                case JobDetailsStatus.Skipped:
                    return BackupRestoreStatus.Skipped;
                default:
                    return BackupRestoreStatus.UnKnown;
            }
        }

        private void SendJobReportsForCanceledJob(MutliImportParameter multiImportParameter)
        {
            using (new AvePerformanceScope("Event:SendJobReportsForCanceledJob"))
            {
                using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                {
                    foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                    {
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                        {
                            mLog.Warn("[SendJobReportsForCanceledJob]This file status is Failed.");
                            mConfig.ProgressDto.HasErrorNode = true;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(info.id), 50000, multiImportParameter.JobId);
                            continue;
                        }
                        try
                        {
                            IAveFile file = GetFile(mWeb, info.serverRelativeUrl, new Guid(info.id));

                            IAveFile stubfile = mWeb.GetFile(file.ServerRelativeUrl + "." + fileNameSUFFIX);
                            if (!stubfile.Exists)
                            {
                                mLog.Info($"The Stub file not exists. will set failed,Id {info.id}, Url {info.serverRelativeUrl}");
                                mConfig.ProgressDto.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(info.id, JobDetailsStatus.Failed);
                                }
                                continue;
                            }
                            else if (stubfile.CheckedOutByUser != null)
                            {
                                stubfile.CheckIn("");
                            }

                            // stub created successfully, add tracking record
                            AddStubFileRecordMapping(multiImportParameter.WebId, multiImportParameter.ListId, info);

                            //Stub exist and source file exist->Delete Source File.
                            if (file.Exists)
                            {
                                RemoveRelatedRelationship(multiImportParameter.Site, file.Item, file.ServerRelativeUrl);
                                DeleteFile(file);
                            }
                            IAveListItem newItem = stubfile.Item;
                            //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                            if (newItem == null)
                            {
                                mLog.Debug("Current IAveListItem is null and will ReGet IAveListItem by List GetItemByUniqueId.");
                                newItem = file.ParentFolder.ParentList.GetItemByUniqueId(stubfile.UniqueId);
                                mLog.Info("ReGet IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                            }
                            ArgumentNullException.ThrowIfNull(newItem);
                            if (!newItem.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                                || newItem.FieldValues[LinkFileCommon.LinkFileFieldName] == null
                                || string.IsNullOrEmpty(newItem.FieldValues[LinkFileCommon.LinkFileFieldName].ToString()))
                            {
                                mLog.Info("Need set link field value again.");
                                LinkFileCommon.SetLinkFieldValue(stubfile.Item, mConfig);
                            }
                            if (DeclareLinkFile())
                            {
                                if (mConfig.IsOneDriverSite)
                                {
                                    mAllJobStatus?[multiImportParameter.ListId]?.SetReportMessage(info.id, "RM_SO_OneDriveDeclareItem_ErrorMessage");
                                }
                                else
                                {
                                    DeclareItem(multiImportParameter.Site, newItem);
                                }  
                            }
                            DeleteEmptyFolder(file);
                            if (mConfig.IsILMode)
                            {
                                string md5 = mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) ? mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Md5 : string.Empty;
                                UpdateExploreDB(multiImportParameter.Site, new Guid(info.id), mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                            mConfig.ProgressDto.HasErrorNode = true;
                        }
                    }
                }

                #region send report
                foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                {
                    if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                    {
                        try
                        {
                            var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);
                            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(report.Status, "", "SO_Action_LevelStub", (int)CacheNodeType.Item);
                            mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(report.FileUrl),
                                         info.size,
                                         report.Status,
                                         (int)CacheNodeType.Item,
                                         mConfig.JobId,
                                         mConfig.currentRule?.Name ?? "",
                                         "",
                                         "SO_Action_LevelStub",
                                         report.Message,
                                         "");
                            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
                            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery && dbReader != null)
                            {
                                dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), new Guid(info.id), ConvertToBackupRestoreStatus(report.Status), info.size, mConfig.ArchiverUNCTime);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                        }
                        mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// DeclareLinkFile is RA job option.
        /// DeclareStubOption is DAO job option.
        /// DeclareStubType.None for old rule,
        /// DeclareStubType.Declare for new rule.
        /// </summary>
        /// <returns></returns>
        private bool DeclareLinkFile()
        {
            //mConfig.currentRule.DeclareLinkFile = false;
            if (mConfig.IsILMode)
            {
                if (mConfig.currentRule != null && mConfig.currentRule.DeclareLinkFile)
                {
                    return true;
                }
                return false;
            }
            else if (mConfig.currentRule != null && (mConfig.currentRule.DeclareLinkFile || mConfig.currentRule.DeclareStubOption == DeclareStubType.Declare || mConfig.currentRule.DeclareStubOption == DeclareStubType.None))
            {
                return true;
            }
            return false;
        }

        private void RemoveRelatedRelationship(IAveSite site, IAveListItem aveListItem, string itemUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.RemoveRelatedRelationship"))
            {
                if (mConfig.IsILMode)
                {
                    try
                    {
                        //移除related 关系
                        var utility = new RelatedRecordsUtility();
                        var relatedInfos = utility.GetRelatedProperties(aveListItem);
                        foreach (var relatedInfo in relatedInfos)
                        {
                            utility.RemoveRelateColumnValue(relatedInfo, site, itemUrl, aveListItem.UniqueId, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("An error occur when remove related relationship.ItemUrl:{0}.Message:{1}.", itemUrl, ex.ToString());
                    }
                }
            }
        }

        private void DeleteEmptyFolder(IAveFile file)
        {
            //mConfig.currentRule.IsDeleteParentFolder = true;
            if (mConfig.currentRule.IsDeleteParentFolder)
            {
                IAveFolder aveFolder = null;
                //From CI,some file get parent folder failed and add retry.
                new AveTaskRetryHelper(5, true).ExecuteWithRetryMechanism(() =>
                {
                    aveFolder = file.ParentFolder;
                });
                if (aveFolder != null)
                {
                    if (aveFolder.UniqueId == aveFolder.ParentList.RootFolder.UniqueId)
                    {
                        mLog.Info("Current folder is root folder and skip delete empty folder.");
                    }
                    else
                    {
                        mConfig.AddDeleteFolderCache(aveFolder.UniqueId);
                    }
                }
            }
        }

        private void BulkDeclareAndDelete(IAveWeb mWeb, IAveList mList, List<ARRestoreFileInfo> infos, MutliImportParameter multiImportParameter)
        {
            try
            {
                bool declareStubFile = LinkFileCommon.IsDeclareLinkFile(mConfig);
                //1.先remove掉没有成功生成的Stub数据，这种数据不需要删除源文件.同时也不需要对Declare文件执行Declare操作.
                List<BulkDeclareAndDeleteFileInfo> mBulkDeclareAndDeleteFileInfos = new List<BulkDeclareAndDeleteFileInfo>();
                foreach (ARRestoreFileInfo mRRestoreFileInfo in infos)
                {

                    AddStubFileRecordMapping(multiImportParameter.WebId, multiImportParameter.ListId, mRRestoreFileInfo);

                    BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo = new BulkDeclareAndDeleteFileInfo();
                    bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo = mRRestoreFileInfo;
                    if (declareStubFile)
                    {
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.GetStubItem"))
                        {
                            IAveFile stubfile = mWeb.GetFile(mRRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                            if (!stubfile.Exists)
                            {
                                mLog.Warn("Stub does not exist when BulkDeclareAndDelete.StubUrl: {0}.", mRRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                mConfig.JobReportDto.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(mRRestoreFileInfo.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(mRRestoreFileInfo.id, JobDetailsStatus.Failed);
                                }
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(mRRestoreFileInfo.id), 50000, mRRestoreFileInfo.subjobid);
                                continue;
                            }
                            else if (stubfile.CheckedOutByUser != null)
                            {
                                stubfile.CheckIn("");
                            }
                            if (!stubfile.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                            || stubfile.Item.FieldValues[LinkFileCommon.LinkFileFieldName] == null
                            || string.IsNullOrEmpty(stubfile.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString()))
                            {
                                mLog.Info("Need set link field value again.");
                                LinkFileCommon.SetLinkFieldValue(stubfile.Item, mConfig);
                            }
                            bulkDeclareAndDeleteFileInfo.stubListItem = stubfile.Item;
                            bulkDeclareAndDeleteFileInfo.stubItemRowId = stubfile.Item.ID;
                        }
                    }
                    mBulkDeclareAndDeleteFileInfos.Add(bulkDeclareAndDeleteFileInfo);
                }
                //2.如果勾选Declare，则对Stub文件执行Declare操作(先批量，出异常再one by one).
                //测试结果：批量Declare，Declare数据和非Declare数据同时存在，也可以批量Declare成功。
                if (declareStubFile)
                {
                    if (mConfig.currentRule.DeclareStubOption == DeclareStubType.AddRecordLabel)
                    {
                        mLog.Info("Start Add record label for stub file");
                        if (mConfig.SharePointRetentionLabel == null)
                        {
                            mConfig.InitRetentionLabelCollections(mWeb.Site);
                        }
                        string retentionLabel = mConfig.GeneralRetentionLabel;
                        if (string.IsNullOrEmpty(retentionLabel))
                        {
                            mLog.Warn($"Record label in general setitng is empty");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "StorageOptimization_SOARRecordLabelDoesNotSetValue");
                            }
                        }
                        else if (mConfig.SharePointRetentionLabel.TryGetValue(retentionLabel, out var info))
                        {
                            if (info.BlockDelete && info.BlockEdit)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.SetComplianceTagOnBulkItems"))
                                {
                                    var record = mConfig.aveObjectModelFactory.CreateRecords();
                                    foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                                    {
                                        try
                                        {
                                            var listItem = bulkDeclareAndDeleteFileInfo.stubListItem;
                                            var isNeedDeclareRecord = false;
                                            if (listItem.IsRecord())
                                            {
                                                mLog.Info($"Start undeclare item {bulkDeclareAndDeleteFileInfo.stubItemRowId}");
                                                record.UndeclareItemAsRecord(listItem);
                                                isNeedDeclareRecord = true;
                                            }
                                            listItem.SetComplianceTag(info.TagName, true, true, false, false);
                                            if (isNeedDeclareRecord)
                                            {
                                                mLog.Info($"Declare item {bulkDeclareAndDeleteFileInfo.stubItemRowId}");
                                                record.DeclareItemAsRecord(listItem);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("Failed record label and Add record label item one by one.Message:{0}", e.ToString());
                                            mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, e.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                mLog.Error($"Current label : {retentionLabel} is not record label");
                                foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                                }
                            }
                        }
                        else
                        {
                            mLog.Error($"Cannot get label : {retentionLabel} in current site collection.");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "StorageOptimization_SOARTagCannotGetLabelByName");
                            }
                        }
                    }
                    else if (mConfig.IsOneDriverSite)
                    {
                        try
                        {
                            mLog.Warn("OneDrive site unable declare item as record, will update report message and status");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "RM_SO_OneDriveDeclareItem_ErrorMessage");
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Fail check or update onedrive stub file skip decalre, ex:{e}");
                        }
                    }
                    else
                    {
                        try
                        {
                            using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeclareItem"))
                            {
                                mLog.Info("Begin BulkDeclareItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                                mList.DeclareItemsByRowIds(mBulkDeclareAndDeleteFileInfos.Select(x => x.stubItemRowId).ToList<int>());
                                mLog.Info("End BulkDeclareItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Failed DeclareItemsByRowIds and declare item one by one.Message:{0}", ex.ToString());
                            //one by one declare
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeclareItemOneByOne"))
                                {
                                    try
                                    {
                                        if (!ScheduleConfiguration.CheckisRecord(bulkDeclareAndDeleteFileInfo.stubListItem))
                                        {
                                            mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(bulkDeclareAndDeleteFileInfo.stubListItem);
                                            mLog.Info("Success declare file one by one.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        mLog.Warn("Declare Item has some error when one by one declare. Detail : {0}.", exc.ToString());
                                        mConfig.JobReportDto.HasErrorNode = true;
                                        bulkDeclareAndDeleteFileInfo.hasErrorNode = true;
                                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id))
                                        {
                                            mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed);
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                //3.Stub操作完成后，删除源文件(先批量，出异常再one by one).
                //测试结果：批量删除会删除正常数据，特殊的数据删除不掉，比如五个数据，两个Declare，三个正常，那么正常的三个数据会删除掉，两个Declare文件删除不掉。
                try
                {
                    using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeleteItem"))
                    {
                        mLog.Info("Begin BulkDeleteItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                        try
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var f in mBulkDeclareAndDeleteFileInfos)
                            {
                                builder.AppendFormat("{0},", f.mARRestoreFileInfo.rowid);
                            }
                            mLog.Info($"Begin BulkDeleteItem.Ids {builder.ToString()}");
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"error occured when BulkDeclareAndDelete1,error:{e}");
                        }
                        if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                            mConfig.currentRule.IncludeDeleteRecordLabel || 
                            (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
                        {
                            mList.SetComplianceTagOnBulkItems(mBulkDeclareAndDeleteFileInfos.Select(x => x.mARRestoreFileInfo.rowid).ToList<int>(), "");
                        }
                        mList.DeleteItemsByRowIds(mBulkDeclareAndDeleteFileInfos.ToDictionary(x => x.mARRestoreFileInfo.rowid, y => y.mARRestoreFileInfo.ModifiedTimeTicks)
                            , mBulkDeclareAndDeleteFileInfos.ToDictionary(x => x.mARRestoreFileInfo.rowid, y => y.mARRestoreFileInfo.TimeLastModifiedTicks));

                        foreach (var bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                        {
                            SendDestructionReport(multiImportParameter.Site, bulkDeclareAndDeleteFileInfo);
                        }
                        mLog.Info("End BulkDeleteItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Failed DeleteItemsByRowIds and delete item one by one.Message:{0}", ex.ToString());
                    //one by one delete
                    foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                    {
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeleteItemOneByOne"))
                        {
                            try
                            {
                                IAveFile file = mWeb.GetFile(new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                if (file != null && !file.Exists)
                                {
                                    mLog.Info("Current file already deleted in batch.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                    continue;
                                }
                                else
                                {
                                    DeleteFile(file);
                                    SendDestructionReport(multiImportParameter.Site, bulkDeclareAndDeleteFileInfo);
                                }
                            }
                            catch (Exception exc)
                            {
                                IAveFile stubfile = mWeb.GetFile(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                if (stubfile.Exists)
                                {
                                    DeleteStubFile(stubfile);
                                    mLog.Warn($"Delete archived file has error, need to delete stub file. File: {bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl.LogBase64()}");
                                    mConfig.DeleteStubFileRecordEntitiesInBatch(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                                }
                                mLog.Warn("Delete Item has some error when one by one delete. Detail : {0}.", exc.ToString());
                                mConfig.JobReportDto.HasErrorNode = true;
                                bulkDeclareAndDeleteFileInfo.hasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed);
                                }
                                continue;
                            }
                        }
                    }
                }
                foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                {
                    if (mConfig.IsILMode)
                    {
                        string md5 = mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id) ? mAllJobStatus[multiImportParameter.ListId].GetReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id).Md5 : string.Empty;
                        UpdateExploreDB(multiImportParameter.Site, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                    }
                    if (bulkDeclareAndDeleteFileInfo.hasErrorNode)
                    {
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), 50000, infos.FirstOrDefault().subjobid);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                mConfig.JobReportDto.HasErrorNode = true;
            }
        }

        private void AddStubFileRecordMapping(Guid webId, Guid listId, ARRestoreFileInfo fileInfo)
        {
            try
            {
                var stubFileRecord = new StubFileRecordDto()
                {
                    //SiteCollectionID = mConfig.SiteCollectionID,
                    ArchivedItemId = new Guid(fileInfo.id),
                    RefDateTime = DateTime.UtcNow,
                    //StubTemplateId = mConfig.currentRule.StubTemplateId,
                    StubId = fileInfo.StubId,
                    //StubType = mConfig.currentRule.LeaveStubType,
                    ArchivedFileFullPath = fileInfo.serverRelativeUrl,
                    ListId = listId,
                    WebId = webId,
                };
                mConfig.AddStubFileRecord(stubFileRecord);
            }
            catch (Exception e)
            {
                mLog.Error("AddStubFileRecordMapping error {0}", e);
            }
        }

        private void SendDestructionReport(IAveSite site, BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo)
        {
            try
            {
                mLog.Info($"Send destruction report, doc id:{bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id}");
                var docId = new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                var record = GetRecordInExplorerDao(site, docId);
                var versionInfos = ScanDBOperationFactory.GetScanDBOperation(mConfig).SelectItemVersionsWithJsonMeta(mConfig.currentRule.Id, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id));
                foreach (var info in versionInfos)
                {
                    DestructionReport destructionReport = new()
                    {
                        NodeId = info.NodeId,
                        ListId = info.ListID,
                        FullPath = info.FullPath,
                        RuleID = new Guid(mConfig.currentRule.Id),
                        ArchivedTime = DateTime.UtcNow.Ticks,
                        SortTicks = Snowflake.Instance().GetTicks().ToString(),
                        JsonMeta = info.JsonMeta,
                        ActionType = (int)mConfig.actionType
                    };
                    if (mConfig.IsOneDriverSite && record != null)
                    {
                        var jsonMeta = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destructionReport.JsonMeta);
                        jsonMeta.OnedriveTermName = record.TermName;
                        destructionReport.JsonMeta = JsonConvert.SerializeObject(jsonMeta);
                    }
                    AddToDestructionCache(destructionReport);
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"SendDestructionReport error {e}");
            }
        }

        private void AddToDestructionCache(DestructionReport destructionReport)
        {
            if (destructionReport == null)
            {
                return;
            }
            DestructionFactory.GetInstance(mConfig.SiteCollectionID.ToString(), mConfig.JobId).InsertValueToDB(new List<DestructionReport>() { destructionReport });
        }

        private void UpdateExploreDB(IAveSite mSite, Guid nodeID, int updateStatus, Record addRecord = null, string pathMd5 = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;

                        record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);

                        if (record != null)
                        {
                            if (mConfig.currentRule.IsManualApproval)
                            {
                                AddManualHistory(record);
                                if (updateStatus == (int)RMRecordStatus.Archived && record.RecordStatus == (int)RMRecordStatus.ManualPreSync)
                                {
                                    //unsync data,keep status when archiver
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, (int)RMRecordStatus.ManualPreSync);
                                }
                                else
                                {
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                                }
                                //mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                            }
                            else
                            {
                                mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime(mSite.ID, recordID, updateStatus);
                            }
                            //mConfig.ExplorerDao.UpdateAll(r => r.ScopeId == record.ScopeId && r.Id == record.Id, r => { r.DestroyedTime = DateTime.UtcNow.Ticks; r.RecordStatus = (int)RMRecordStatus.Destroyed; });
                            mLog.Info("Update Record Status successful");
                            if (updateStatus == 8)
                            {
                                ArchiverIndexDto indexDto = null; ;
                                try
                                {
                                    indexDto = Convert2ArchiverIndexDto(mConfig.GetArchiverIndex(pathMd5));
                                }
                                catch (Exception e)
                                {
                                    mLog.Error($"Error occurred while getting archiver index. Node id:{nodeID} Error:{e.ToString()}");
                                }
                                //ecord.AppendMetaInfoForArchiverIndex(SerializerHelper.SerializeByDataContractJsonSerializer(indexDto));
                                mConfig.ExplorerDao.AddArchivedRelatedColumn(mSite.ID, recordID, pathMd5, mConfig.CurrentIndexJobID, indexDto != null ? SerializerHelper.SerializeByDataContractJsonSerializer(indexDto) : null);
                                mLog.Info("Update archived custom column successful");
                            }
                        }
                        else
                        {
                            mLog.Info("Current object:{0} doesn't exist in explore.", recordID);
                            if (addRecord != null)
                            {
                                //RECO-9707 if site collection doesn't exist in explorer, add it
                                addRecord.DestroyedTime = DateTime.UtcNow.Ticks;
                                mConfig.ExplorerDao.Add(addRecord);
                                mLog.Info("Add object:{0} in explorer.", addRecord.NodeId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
        }

        private Record GetRecordInExplorerDao(IAveSite mSite, Guid nodeID)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;
                        if (mConfig.IsRelativeDataJob)
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        else
                        {
                            record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                            if (record == null && mConfig.exploreDBSPRecords.Count >= 10000)
                            {
                                record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                            }
                        }
                        return record;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                        return null;
                    }
                }
                else
                { return null; }
            }
        }
        private void AddManualHistory(Record record)
        {
            ManualUtil manualUtil = new ManualUtil(mConfig);
            manualUtil.AddManualHistory(record);
        }

        public ArchiverIndexDto Convert2ArchiverIndexDto(ArchiverBasicIndex index)
        {
            ArchiverIndexDto dto = new ArchiverIndexDto()
            {
                ContentDataFileNumber = index.ContentDataFileNumber,
                ContentDataHeaderOffset = index.ContentDataHeaderOffset,
                ContentDataOffset = index.ContentDataOffset,
                ContentLength = index.ContentLength,
                ContentOffset = index.ContentOffset,
                ContentPageSize = index.ContentPageSize,
                DataFileLength = index.DataFileLength,
                DataFileNumber = index.DataFileNumber,
                DataFileOffset = index.DataFileOffset,
                Flag = index.Flag,
                JobId = index.JobId,
                MetaDataHeaderOffset = index.MetaDataHeaderOffset,
                Version = index.Version
            };
            return dto;
        }

        private IAveFile GetFile(IAveWeb web, string serverRelativeUrl, Guid id)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.GetCreateLinkFile"))
            {
                IAveFile linkFile = web.GetFile(serverRelativeUrl);
                if (!linkFile.Exists)
                {
                    mLog.Info("Load file by id.");
                    try
                    {
                        //Office 365 Root Site Collection need send serverRelativeUrl.RECO-1278
                        linkFile = web.GetFile(id);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Can't reGet file UniqueId.Message:{0}.", ex.ToString());
                    }
                }
                if (!linkFile.Exists)
                {
                    mLog.Warn($"Load file failed,ServerRelativeUrl: {serverRelativeUrl} id:{id}");
                }
                return linkFile;
            }
        }

        private void DeclareItem(IAveSite site, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.KeepData.DeclareItem"))
            {
                try
                {
                    if (!ScheduleConfiguration.CheckisRecord(item))
                    {
                        mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(item);
                    }
                }
                catch (Exception exc)
                {
                    mLog.Warn("Declare Item has some error, detail : {0}", exc.ToString());
                    throw;
                }
            }
        }

        private void DeleteFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteFile"))
            {
                IAveListItem listItem = null;
                ListItemComplianceInfo complianceInfo = null;
                bool needRestoreComplianceTag = false;
                try
                {
                    listItem = file.Item;
                    GetComplianceTagIfEnableRemove(listItem, out complianceInfo);
                    if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag))
                    {
                        throw new Exception("StorageOptimization_Skip_Unlock_Status_Item");
                    }
                    #region delete label file
                    if (mConfig.IsILMode
                        && file.Item.Fields.ContainsField("Retention label")
                        && RecordsDBOperation.RMEXOLabels.Where(
                            x => x.LabelName == file.Item["Retention label"].ToString()
                            && x.Status == 1 && x.Type == 1).FirstOrDefault() != null)
                    {
                        mLog.Info("Current file is label file and Records remove label and delete.FileName:{0}.", file.UniqueId);
                        //file.Item.SetComplianceTag(string.Empty, false, false, isTagSuperLock: false);
                        file.Item.SetComplianceTagOnBulkItems(string.Empty);
                    }
                    #endregion

                    #region delete declare document for office 365
                    if ((mConfig.BackgroundSettings.IsDeleteRecord || mConfig.currentRule.DeleteRecords || (mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem))) 
                        && ArchiverCommonStaticMethod.CheckisRecord(listItem))
                    {
                        mLog.Info("Office365 Begin CheckFileIsRecord");
                        if (ArchiverCommonStaticMethod.CheckIsRecordOnly(listItem))
                        {
                            mLog.Info("This File is Declare File.FileName:{0}", file.UniqueId);
                            mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(listItem);
                        }
                        else
                        {
                            mLog.Warn("This File is Declare And Hold File.FileName:{0}", file.UniqueId);
                            mConfig.JobReportDto.HasErrorNode = true;
                            throw new Exception("This File is Declare And Hold File.");
                        }
                    }
                    #endregion

                    DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out needRestoreComplianceTag);
                    try
                    {
                        file.Delete();
                        needRestoreComplianceTag = false;
                        mLog.Info("Delete the file successfully. {0}", file.ServerRelativeUrl);
                    }
                    catch (Exception e)
                    {
                        string errorMessage = e.ToString();
                        #region Error Code: 6404
                        if (errorMessage.Contains("Code d'erreur : 6404")//法语
                            || errorMessage.Contains(": 6404")
                            || errorMessage.Contains("Error Code: 6404")
                            || errorMessage.Contains("Код ошибки: 6404")//俄语
                            || errorMessage.Contains("Impossible de supprimer le fichier")
                            || errorMessage.Contains("Cannot remove file"))

                        {
                            mLog.Info("Delete file has 6404 error and need reget file.File url:{0}.", file.ServerRelativeUrl);
                            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(3, true);
                            retryHelper.ExecuteWithRetryMechanism(() =>
                            {
                                file = mConfig.aveObjectModelFactory.CreateSite(file.ParentFolder.ParentList.ParentWeb.Site.Url).OpenWeb(file.ParentFolder.ParentList.ParentWeb.ID).GetFile(file.UniqueId, file.ServerRelativeUrl);
                                if (file.Exists)
                                {
                                    file.Delete();
                                    needRestoreComplianceTag = false;
                                }
                                else
                                {
                                    mLog.Info("Current file:{0} doesn't exist when Error Code: 6404.", file.ServerRelativeUrl);
                                }
                            });
                            mLog.Info("Delete 6404 error file success.File name:{0}", file.Name);
                        }
                        #endregion
                        #region check out/lock/label file.
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("is checked out for editing by")
                            || e.InnerException.Message.Contains("est extrait pour modification par")//法语
                            || e.InnerException.Message.Contains("został wyewidencjonowany do edycji przez użytkownika")//波兰语
                            || e.InnerException.Message.Contains("извлечен для редактирования пользователем")//俄语
                            || e.InnerException.Message.Contains("Foi feito o check-out para edição do arquivo")
                            || e.InnerException.Message.Contains("Foi dada saída ao ficheiro")//葡萄牙语
                            || e.InnerException.Message.Contains("zur Bearbeitung ausgecheckt")//德语
                            ))
                        {
                            mLog.Info("Current file is check out file and Records check in and delete.FileName:{0}.", file.Name);
                            file.CheckIn("");
                            file.Delete();
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete check out document success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        #region Item cannot be deleted while on hold
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("Item cannot be deleted while on hold.")))
                        {
                            mLog.Info("Current file is is hold file.FileName:{0}. will retry.", file.Name);
                            Thread.Sleep(500);
                            file.Delete();
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete hold file success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        else
                        {
                            file = file.ParentFolder.ParentList.ParentWeb.GetFile(file.UniqueId, file.ServerRelativeUrl);
                            file.Delete();
                            needRestoreComplianceTag = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("File Delete Error: {0} error message: {1}", file.Name, ex.ToString());
                    if (needRestoreComplianceTag)
                    {
                        SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                    }
                    throw;
                }
            }
        }

        private void DeleteComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo, out bool deletedComplianceTag)
        {
            deletedComplianceTag = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
                {
                    if (mConfig.currentRule.IncludeDeleteRecordLabel)
                    {
                        var isRecordTypeLabel = IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag);
                        if (isRecordTypeLabel)
                        {
                            mLog.Info("Current label is record label and the rule is include the record label");
                            if (complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && isRecordTypeLabel)
                            {
                                mLog.Info("Current status of label is unlock status. So start lock label before remove the current label");
                                listItem?.LockRecordItem();
                            }
                            listItem?.SetComplianceTagOnBulkItems("");
                            deletedComplianceTag = true;
                        }
                    }
                    if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                        ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                    {
                        if(!IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag))
                        {
                            mLog.Info("Current label is not record label and the rule is enable remove retention label");
                            listItem?.SetComplianceTagOnBulkItems("");
                            deletedComplianceTag = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Fail delete retention label,error message:{e.Message},error:{e}");
            }
        }

        private bool SetComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag) &&
                (WrapperConfiguration.EnableRemoveRetentionLabel ||
                (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
            {
                try
                {
                    listItem.SetComplianceTagOnBulkItems(complianceInfo.ComplianceTag);
                    if (mConfig.SharePointRetentionLabel == null)
                    {
                        mConfig.InitRetentionLabelCollections(listItem.Web.Site);
                    }
                    if (mConfig.SharePointRetentionLabel.TryGetValue(complianceInfo.ComplianceTag, out AveComplianceTagInfo aveComplianceTagInfo))
                    {
                        if (aveComplianceTagInfo.UnlockedAsDefault && complianceInfo.TagPolicyHold && complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(listItem.Web.Site, complianceInfo.ComplianceTag))
                        {
                            listItem.LockRecordItem();
                        }
                    }
                    else
                    {
                        mLog.Warn($"can not get compliance init lock status, compliane name :{complianceInfo.ComplianceTag}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    mLog.Error($"Fail set retention label,label:{complianceInfo.ComplianceTag},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
            return false;
        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName)
        {
            try
            {
                if (mConfig.SharePointRetentionLabel == null)
                {
                    mConfig.InitRetentionLabelCollections(site);
                }
                if (mConfig.SharePointRetentionLabel.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                throw;
            }
        }

        private bool GetComplianceTagIfEnableRemove(IAveListItem listItem, out ListItemComplianceInfo complianceInfo)
        {
            try
            {
                complianceInfo = null;
                string retentionLabel = listItem.GetComplianceTagName();
                if (!string.IsNullOrWhiteSpace(retentionLabel) &&
                    (WrapperConfiguration.EnableRemoveRetentionLabel ||
                    mConfig.currentRule.IncludeDeleteRecordLabel || 
                    (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                {
                    var nowComplianceInfo = listItem.GetComplianceInfo(false);
                    if (string.IsNullOrWhiteSpace(nowComplianceInfo?.ComplianceTag))
                    {
                        return false;
                    }
                    complianceInfo = new ListItemComplianceInfo()
                    {
                        ComplianceTag = nowComplianceInfo.ComplianceTag,
                        TagPolicyHold = nowComplianceInfo.TagPolicyHold,
                        TagPolicyEventBased = nowComplianceInfo.TagPolicyEventBased,
                        TagPolicyRecord = nowComplianceInfo.TagPolicyRecord
                    };
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Error($"fail get complianceTag, item:{listItem.Url},Exception:{e}");
                throw;
            }
        }

        private void DeleteStubFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteStubFile"))
            {
                try
                {
                    IAveListItem listItem = file.Item;
                    if (ScheduleConfiguration.CheckisRecord(listItem))
                    {
                        mLog.Info("This Stub File is Declare File.FileName:{0}", file.UniqueId);
                        mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(listItem);
                    }
                    try
                    {
                        file.Delete();
                        //mLog.Info("Delete the file successfully. {0}", file.ServerRelativeUrl);
                    }
                    catch (Exception e)
                    {
                        string errorMessage = e.ToString();
                        #region Error Code: 6404
                        if (errorMessage.Contains("Code d'erreur : 6404")//法语
                            || errorMessage.Contains(": 6404")
                            || errorMessage.Contains("Error Code: 6404")
                            || errorMessage.Contains("Код ошибки: 6404")//俄语
                            || errorMessage.Contains("Impossible de supprimer le fichier")
                            || errorMessage.Contains("Cannot remove file"))

                        {
                            mLog.Info("Delete file has 6404 error and need reget file.File url:{0}.", file.ServerRelativeUrl);
                            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(3, true);
                            retryHelper.ExecuteWithRetryMechanism(() =>
                            {
                                file = mConfig.aveObjectModelFactory.CreateSite(file.ParentFolder.ParentList.ParentWeb.Site.Url).OpenWeb(file.ParentFolder.ParentList.ParentWeb.ID).GetFile(file.UniqueId, file.ServerRelativeUrl);
                                if (file.Exists)
                                {
                                    file.Delete();
                                }
                                else
                                {
                                    mLog.Info("Current file:{0} doesn't exist when Error Code: 6404.", file.ServerRelativeUrl);
                                }
                            });
                            mLog.Info("Delete 6404 error file success.File name:{0}", file.Name);
                        }
                        #endregion
                        #region check out/lock/label file.
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("is checked out for editing by")
                            || e.InnerException.Message.Contains("est extrait pour modification par")//法语
                            || e.InnerException.Message.Contains("został wyewidencjonowany do edycji przez użytkownika")//波兰语
                            || e.InnerException.Message.Contains("извлечен для редактирования пользователем")//俄语
                            || e.InnerException.Message.Contains("Foi feito o check-out para edição do arquivo")
                            || e.InnerException.Message.Contains("Foi dada saída ao ficheiro")//葡萄牙语
                            || e.InnerException.Message.Contains("zur Bearbeitung ausgecheckt")//德语
                            ))
                        {
                            mLog.Info("Current file is check out file and Records check in and delete.FileName:{0}.", file.Name);
                            file.CheckIn("");
                            file.Delete();
                            mLog.Info("Delete check out document success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        #region Item cannot be deleted while on hold
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("Item cannot be deleted while on hold.")))
                        {
                            mLog.Info("Current file is is hold file.FileName:{0}. will retry.", file.Name);
                            Thread.Sleep(500);
                            file.Delete();
                            mLog.Info("Delete hold file success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        else
                        {
                            file = file.ParentFolder.ParentList.ParentWeb.GetFile(file.UniqueId, file.ServerRelativeUrl);
                            file.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("Stub File Delete Error: {0} error message: {1}", file.Name, ex.ToString());
                }
            }
        }

        private void SendErrorJobReport(string errorMessage, MutliImportParameter multiImportParameter)
        {
            //RemoveReports

        }

        private void InitTaskManager()
        {
            multiReceiver = new AveMultiReceiver(10);
            multiReceiver.scheduler.AddTask(new AveMutiEmpty(0, true));
            mRoleAssignmentsObject = null;
        }

        private void UpdateContainerInfo()
        {
            mAzureInfo.AzureManifestContainerName = UpdateContainerName(mAzureInfo.AzureManifestContainerName);
            mAzureInfo.AzureSourceContainerName = UpdateContainerName(mAzureInfo.AzureSourceContainerName);
            mAzureInfo.AzureQueueReportContainerName = UpdateContainerName(mAzureInfo.AzureQueueReportContainerName);

            string guid = Guid.NewGuid().ToString();
            tempContentPath = Path.Combine(tempBaseJobPath, mAzureInfo.AzureSourceContainerName, guid);
            tempManifestPath = Path.Combine(tempBaseJobPath + "Manifest", mAzureInfo.AzureManifestContainerName, guid);
            CreateDirectory(tempContentPath);
            CreateDirectory(tempManifestPath);
            //SaveStubStream(mStubBytes);
        }

        private string UpdateContainerName(string containerName)
        {
            string containerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            int index = containerName.LastIndexOf('-');
            return containerName.Substring(0, index) + "-" + containerId;
        }

        private void GenerateCurrentFolderObjects()
        {
            mCacheSPFolderObjects.ForEach(folder => mSPObjectCollection.SPObject.Add(folder));
        }
        private void ProcessFileObjectNode(IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, SPFile file)
        {
            string id = aveFile.UniqueId.ToString();
            lock (mSPObjectCollection)
            {
                if (mSPObjectCollection == null)
                {
                    mSPObjectCollection = new SPGenericObjectCollection();
                }
                var mSPFileObject = new SPGenericObject();
                mSPFileObject.Id = id;
                mSPFileObject.Item = new SPFile();
                mSPObjectCollection.SPObject.Add(mSPFileObject);

                mSPFileObject.Id = file.Id;
                mSPFileObject.ObjectType = SPObjectType.SPFile;
                mSPFileObject.ParentId = file.ParentId;
                mSPFileObject.ParentWebId = file.ParentWebId;
                mSPFileObject.ParentWebUrl = file.ParentWebUrl;
                mSPFileObject.Url = aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + file.Url;
                SPFile fileObject = (SPFile)mSPFileObject.Item;
                mSPFileObject.Item = file;
            }
        }

        private void ProcessListItemNode(IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, SPListItem item, List<AveRoleAssignmentInfo> roleAssignments, out string dataPath)
        {
            string id = aveFile.UniqueId.ToString();
            lock (mSPObjectCollection)
            {
                dataPath = tempContentPath;
                if (mSPObjectCollection == null)
                {
                    mSPObjectCollection = new SPGenericObjectCollection();
                }
                var mSPObject = new SPGenericObject();
                mSPObject.Id = id;
                mSPObject.Item = new SPListItem();
                mSPObjectCollection.SPObject.Add(mSPObject);

                mSPObject.Id = item.Id;
                mSPObject.Name = item.Name;
                mSPObject.ObjectType = SPObjectType.SPListItem;
                mSPObject.ParentId = item.ParentWebId;
                mSPObject.ParentWebUrl = mAveList.ParentWeb.ServerRelativeUrl;
                mSPObject.ParentWebId = mAveList.ParentWeb.ID.ToString();
                mSPObject.ParentId = mAveList.ID.ToString();
                mSPObject.Url = mAveList.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.FileUrl;

                SPListItem itemObject = (SPListItem)mSPObject.Item;

                mSPObject.Item = item;
            }
            using (new AvePerformanceScope("SP2013ArchiveBackUp.ProcessRoleAssignmentsXML"))
            {
                if (roleAssignments != null)
                {
                    ProcessRoleAssigementsXML(roleAssignments, item.Id, item.FileUrl);
                }
            }
        }

        private void ProcessRoleAssigementsXML(List<AveRoleAssignmentInfo> roleAssignmentsInfo, string objectId, string objectUrl)
        {
            if (mRoleAssignmentsObject == null)
            {
                mRoleAssignmentsObject = new SPGenericObject();
                mRoleAssignmentsObject.Id = Guid.NewGuid().ToString();
                mRoleAssignmentsObject.ParentId = mAveList.ParentWeb.ID.ToString();
                mRoleAssignmentsObject.ParentWebId = mAveList.ParentWeb.ID.ToString();
                mRoleAssignmentsObject.ParentWebUrl = mAveList.ParentWeb.ServerRelativeUrl;
                mRoleAssignmentsObject.ObjectType = SPObjectType.DeploymentRoleAssignments;

                DeploymentRoleAssignments roleAssignmentsObj = new DeploymentRoleAssignments();
                mRoleAssignmentsObject.Item = roleAssignmentsObj;
            }

            DeploymentRoleAssignment roleAssignment = new DeploymentRoleAssignment();
            roleAssignment.ScopeId = Guid.NewGuid().ToString();
            roleAssignment.ObjectId = objectId;
            roleAssignment.ObjectType = "2";
            roleAssignment.Assignment = new List<DeploymentAssignment>();
            roleAssignment.RoleDefWebId = mAveList.ParentWeb.ID.ToString();
            roleAssignment.RoleDefWebUrl = mAveList.ParentWeb.ServerRelativeUrl;
            roleAssignment.ObjectUrl = objectUrl;
            roleAssignment.AnonymousPermMask = "0";
            try
            {
                if (roleAssignmentsInfo != null && roleAssignmentsInfo.Count > 0)
                {
                    foreach (AveRoleAssignmentInfo roleAssignmentInfo in roleAssignmentsInfo)
                    {
                        DeploymentAssignment assignment = new DeploymentAssignment();
                        assignment.PrincipalId = roleAssignmentInfo.PrincipalId.ToString();
                        lock (mSPObjectCollection)
                        {
                            if (!mUserGroupMappingForCurrentPackage.Contains(roleAssignmentInfo.PrincipalId))
                            {
                                mUserGroupMappingForCurrentPackage.Add(roleAssignmentInfo.PrincipalId);
                            }
                        }
                        assignment.RoleId = roleAssignmentInfo.RoleId.ToString();
                        roleAssignment.Assignment.Add(assignment);
                    }
                }
                ((DeploymentRoleAssignments)mRoleAssignmentsObject.Item).RoleAssignment.Add(roleAssignment);
            }
            catch (Exception e)
            {
                mLog.Warn(string.Format("ImportRestoreItemSecurityWarn", e.Message));
            }
        }

        private SPMMetadataGenerator ReadMetadataCacheFromStream(IAveRestoreStream restoreStream)
        {
            Dictionary<string, object> mCache = new Dictionary<string, object>();
            GetBackupData(restoreStream, ref mCache);
            return new SPMMetadataGenerator(mCache);
        }

        private void GetBackupData(IAveRestoreStream mSourceStream, ref Dictionary<string, object> data)
        {
            AveMetadata metadata;
            while ((metadata = mSourceStream.ReadMetadata()) != null)
            {
                switch (metadata.MetadataType)
                {
                    case AveMetadataType.UserProfileMembership:
                    case AveMetadataType.UserProfileColleague:
                    case AveMetadataType.UserCache:
                    case AveMetadataType.GroupCache:
                        if (!data.ContainsKey(metadata.MetadataType.ToString()))
                        {
                            List<AveMetadata> aveMetadatas = new List<AveMetadata>();
                            aveMetadatas.Add(metadata);
                            data.Add(metadata.MetadataType.ToString(), aveMetadatas);
                        }
                        else
                        {
                            (data[metadata.MetadataType.ToString()] as List<AveMetadata>).Add(metadata);
                        }
                        break;
                    case AveMetadataType.Unknown:
                        //var message = metadata.GetMetadata<SPMInternatMessage>();
                        //throw new SPMFailedUnknownException(message);
                        //throw new SPMFailedUnknownException(new SPMInternatMessage()
                        //{
                        //    Key = "Migration_SharePoint_ImportUnknownTypeByBackup",
                        //    Format = SPMReportResource.Migration_SharePoint_ImportUnknownTypeByBackup
                        //});
                        throw new Exception("UnknownTypeByBackup");
                    default:
                        data.Add(metadata.MetadataType.ToString(), metadata);
                        break;
                }
            }
        }

        private void InitContainerInfo(WinAzure mAzureInfo)
        {
            try
            {
                string containerId = Guid.NewGuid().ToString().ToLower().Replace("-", "");
                string jobid = mConfig.JobId.Replace("_", "-").ToLower();
                mAzureInfo.AzureManifestContainerName = "m-" + jobid + "-" + containerId;
                mAzureInfo.AzureSourceContainerName = "s-" + jobid + "-" + containerId;
                mAzureInfo.AzureQueueReportContainerName = "q-" + jobid + "-" + containerId;


                tempContentPath = Path.Combine(tempBaseJobPath, mAzureInfo.AzureSourceContainerName);
                tempManifestPath = Path.Combine(tempBaseJobPath + "Manifest", mAzureInfo.AzureManifestContainerName);

                CreateDirectory(tempContentPath);
                CreateDirectory(tempManifestPath);

                //File.Copy(tempJobPath + "\\" + USETGROUP_XML_NAME, tempManifestPath + "\\" + USETGROUP_XML_NAME);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while initiating container information. Exception: {0}.", e.ToString());
            }
        }

        private void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        //private bool SaveContentData(string key, IAveRestoreStream stream)
        //{
        //    bool saveDataSuccessful = false;
        //    string azureFileValue = string.Format("{0}.dat", ++MIImportConstant.FileValue);
        //    string tempFilePath = Path.Combine(tempContentPath, azureFileValue);
        //    try
        //    {

        //        if (SaveStream(tempFilePath, stream, ref md5Value))
        //        {
        //            if (!mFileValueDic.ContainsKey(key))
        //            {
        //                mFileValueDic.Add(key, azureFileValue);
        //                saveDataSuccessful = true;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        mLog.Error("An error occurred while saving content data. Exception: {0}.", e.ToString());
        //    }
        //    return saveDataSuccessful;
        //}

        //private bool SaveStream(string filePath, IAveRestoreStream mReceiver, ref string md5Value)
        //{
        //    using (new AvePerformanceScope("Performance.SaveStream"))
        //    {
        //        MD5 md5 = new MD5CryptoServiceProvider();
        //        bool isDamagedFile = false;
        //        try
        //        {
        //            if (System.IO.File.Exists(filePath))
        //            {
        //                System.IO.File.Delete(filePath);
        //            }
        //            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                using (AveSPFileStream fs = new AveSPFileStream(mReceiver))
        //                {
        //                    long length = mReceiver.ContentLength;
        //                    while (length > 0)
        //                    {
        //                        byte[] buffer = new byte[Int16.MaxValue * 2];
        //                        int readCount = fs.Read(buffer, 0, buffer.Length);
        //                        if (readCount <= 0)
        //                        {
        //                            isDamagedFile = true;
        //                            break;
        //                        }
        //                        fileStream.Write(buffer, 0, readCount);
        //                        length -= readCount;
        //                    }
        //                    fileStream.Flush();
        //                }
        //                if (isDamagedFile)
        //                {
        //                    mLog.Debug("This is damaged file. Don't restore this file.");
        //                    if (File.Exists(filePath))
        //                    {
        //                        File.Delete(filePath);
        //                    }
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            mLog.Error("An error occurred while saving stream. Exception: {0}.", e.ToString());
        //            return false;
        //        }
        //    }
        //}

        private SPListItem GenrateItemNode(AveSPItem aveItem, IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, string itemid)
        {
            int version = (int)userData["#tp_UIVersion"];
            int docRowId = (int)userData["#tp_ID"];
            //string id = docData["Id"].ToString();
            // string id = itemid;// Guid.NewGuid().ToString();
            SPListItem item = new SPListItem();

            item.ParentWebId = aveFile.ParentFolder.ParentWeb.ID.ToString();
            item.ParentFolderId = aveFile.ParentFolder.UniqueId.ToString();
            item.Name = aveFile.Name.ToString() + "." + fileNameSUFFIX;
            item.DirName = aveFile.ParentFolder.ServerRelativeUrl;
            item.Id = itemid;
            item.DocId = itemid;
            item.Version = userData["#tp_UIVersionString"].ToString();
            item.DocType = ListItemDocType.File;
            item.IntId = docRowId;
            item.ParentListId = aveFile.ParentFolder.ParentList.ID.ToString();
            if (userData.ContainsKey("#tp_ModerationStatus"))
            {
                item.ModerationStatus = (SPModerationStatusType)userData["#tp_ModerationStatus"];
            }
            if (userData.ContainsKey("_ModerationComments"))
            {
                item.ModerationComment = userData["_ModerationComments"].ToString();
            }
            if (aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Trim('/').Length == 0)
            {
                item.FileUrl = aveFile.ServerRelativeUrl.Substring(1) + "." + fileNameSUFFIX;
            }
            else
            {
                item.FileUrl = aveFile.ServerRelativeUrl.Substring(aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Length + 1) + "." + fileNameSUFFIX;
            }

            using (new AvePerformanceScope("ProcessFieldCollection"))
            {
                SPFieldCollection fieldCollection = ProcessFieldCollection(item, aveItem, docRowId, version, userData, dataJunction);
                //mAveSPList.AveFields.ResetNotUpdateLookupFieldValue(Convert.ToInt32(userData["#tp_ID"]));
                item.Items.Add(fieldCollection);
            }
            return item;
        }

        private SPFile GenrateFileNode(IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, string ItemID, int fileNum)
        {
            int docRowId = (int)userData["#tp_ID"];
            SPFile file = new SPFile();
            string fileValueKey = string.Empty;
            if (aveFile != null)
            {
                fileValueKey = aveFile.UniqueId.ToString();
                file.Name = aveFile.Name + "." + fileNameSUFFIX;
                file.ParentId = aveFile.ParentFolder.UniqueId.ToString();
                if (aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Trim('/').Length == 0)
                {
                    file.Url = aveFile.ServerRelativeUrl.Substring(1) + "." + fileNameSUFFIX;
                }
                else
                {
                    file.Url = aveFile.ServerRelativeUrl.Substring(aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Length + 1) + "." + fileNameSUFFIX;
                }
                file.ParentWebId = aveFile.ParentFolder.ParentWeb.ID.ToString();
                file.ParentWebUrl = aveFile.ParentFolder.ParentWeb.ServerRelativeUrl;
                file.ListId = aveFile.ParentFolder.ParentListId.ToString();
            }
            Dictionary<Dictionary<string, string>, string> value = new Dictionary<Dictionary<string, string>, string>();
            file.Id = ItemID;
            file.TimeCreated = Convert.ToDateTime(userData["Created"]);
            file.TimeLastModified = Convert.ToDateTime(userData["Modified"]);
            file.Version = userData["#tp_UIVersionString"].ToString();
            //file.FileValue = mFileValueDic[fileValueKey];
            file.FileValue = string.Format("{0}.dat", fileNum);
            file.Author = userData["Author"].ToString();
            file.ModifiedBy = userData["Editor"].ToString();
            file.InDocumentLibrary = true;
            //fileVersion.Links = "";
            file.ListItemIntId = docRowId;
            if (userData.ContainsKey("SetupPath"))
            {
                file.SetupPath = userData["SetupPath"].ToString();
                file.IsGhosted = true;
            }
            return file;
        }

        private void ProcessListRootFolderXML()
        {
            mParentSPFolderObject = new SPGenericObject()
            {
                Id = mAveList.ID.ToString(),
                ObjectType = SPObjectType.SPFolder,
                ParentId = mAveList.RootFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = mAveList.ParentWeb.ID.ToString(),
                ParentWebUrl = mAveList.ParentWeb.ServerRelativeUrl,
                Url = mAveList.RootFolder.ServerRelativeUrl,
            };
            var spFolder = new SPFolder()
            {
                Id = mAveList.RootFolder.UniqueId.ToString(),
                Url = mAveList.RootFolder.Url,
                Name = mAveList.RootFolder.Name,
                ParentFolderId = mAveList.RootFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = mAveList.ParentWeb.ID.ToString(),
                ParentWebUrl = mAveList.ParentWeb.ServerRelativeUrl,
                ContainingDocumentLibrary = mAveList.ID.ToString(),
                TimeCreated = mAveList.Created,
                TimeLastModified = mAveList.LastItemModifiedDate,
            };
            mParentSPFolderObject.Item = spFolder;
            mSPObjectCollection.SPObject.Add(mParentSPFolderObject);
            mCacheSPFolderObjects.Add(mParentSPFolderObject);
        }

        private void ProcessExportSettingsXml()
        {
            try
            {
                var spExportSettings = new SPExportSettings()
                {
                    SiteUrl = mAveList.ParentWeb.Site.Url,
                    FileLocation = string.Empty,//location
                    BaseFileName = "Doclib11.cmp",//need to change
                    IncludeSecurity = SPIncludeSecurity.All,
                    ExportPublicSchema = true,
                    ExportFrontEndFileStreams = true,
                    ExportMethod = SPExportMethodType.ExportAll,
                    ExcludeDependencies = false,
                    SourceType = SPSourceType.Other,
                    DetailedSource = "SharePointOnline",
                };
                spExportSettings.ExportObjects.Add(new SPExportObject()
                {
                    Id = mAveList.ID.ToString(),
                    Type = SPDeploymentObjectType.List,
                    ParentId = mAveList.ParentWeb.ID.ToString(),
                    Url = mAveList.RootFolder.ServerRelativeUrl,
                    ExcludeChildren = false,
                    IncludeDescendants = SPIncludeDescendants.All,
                });
                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.EXPORTSETTINGS_XML_NAME), spExportSettings);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing ExportSettingsXml. Exception: {0}.", e.ToString());
            }
        }

        private void ProcessRequirementsXml()
        {
            try
            {
                var spImportRequirements = new SPImportRequirements();
                spImportRequirements.Requirement.Add(new SPRequirement()
                {
                    Type = SPRequirementObjectType.WebPart,
                    Data = mAveList.DefaultViewUrl,
                    //Need to replace
                    Id = "Microsoft.SharePoint.dll v4.0.30319",
                    Name = "a6524906-3fd2-ee4e-23ee-252d3c6e0dc9",
                });
                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.REQUIREMENTS_XML_NAME), spImportRequirements);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing RequirementsXml. Exception: {0}.", e.ToString());
            }
        }

        protected void ProcessRootObjectMapxml()
        {
            try
            {
                SPRootObjects rootObjects = new SPRootObjects();
                SPRootObject rootObject = new SPRootObject();
                rootObject.IsDependency = false;
                rootObject.Url = mAveList.RootFolder.ServerRelativeUrl;
                rootObject.WebUrl = mAveList.ParentWeb.ServerRelativeUrl;
                rootObject.ParentId = mAveList.ParentWeb.ID.ToString();
                rootObject.Type = SPDeploymentObjectType.List;
                rootObject.Id = mAveList.ID.ToString();
                rootObjects.RootObject.Add(rootObject);
                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.ROOTOBJECTMAP_XML_NAME), rootObjects);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing RootObjectMapxml. Exception: {0}.", e.ToString());
            }
        }

        protected void ProcessViewFormsListXML()
        {
            try
            {
                SPViewFormsList viewFormsList = new SPViewFormsList();
                SPViewForm viewForm = new SPViewForm();
                viewForm.Id = "";
                viewForm.Type = "";
                viewFormsList.ViewForm.Add(viewForm);
                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.VIEWFORMSLIST_XML_NAME), viewFormsList);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing ViewFormsListXML. Exception: {0}.", e.ToString());
            }
        }

        protected void ProcessSystemDataXML()
        {
            try
            {
                SPSystemData systemData = new SPSystemData();
                SPSchemaVersion schemaVersion = new SPSchemaVersion();
                schemaVersion.Version = "15.0.0.0";
                schemaVersion.SiteVersion = "15";
                schemaVersion.DatabaseVersion = "4406368";
                schemaVersion.Build = "15.0.4420.1017";
                schemaVersion.ObjectsProcessed = mSPObjectCollection.SPObject.Count;

                systemData.SchemaVersion = schemaVersion;

                SPManifestFile manifestFile = new SPManifestFile();
                manifestFile.Name = "Manifest.xml";

                systemData.ManifestFiles.Add(manifestFile);

                //SPSystemObject systemObject1 = new SPSystemObject();
                //systemObject1.Id = mAveList.ParentWeb.AveWeb.RootFolder.UniqueId.ToString();
                //systemObject1.Url = mAveList.ParentWeb.AveWeb.RootFolder.ServerRelativeUrl;
                //systemObject1.Type = SPDeploymentObjectType.Folder;

                SPSystemObject systemObject2 = new SPSystemObject();
                systemObject2.Id = mAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.ID.ToString();
                systemObject2.Url = mAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.RootFolder.ServerRelativeUrl;
                systemObject2.Type = SPDeploymentObjectType.List;

                SPSystemObject systemObject3 = new SPSystemObject();
                systemObject3.Id = mAveList.ParentWeb.Site.RootWeb.ID.ToString();
                systemObject3.Url = mAveList.ParentWeb.Site.RootWeb.ServerRelativeUrl;
                systemObject3.Type = SPDeploymentObjectType.Web;

                //systemData.SystemObjects.Add(systemObject1);
                systemData.SystemObjects.Add(systemObject2);
                systemData.SystemObjects.Add(systemObject3);

                //SPList list = new SPList();

                //systemData.RootWebOnlyLists.Add(list);
                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.SYSTEMDATA_XML_NAME), systemData);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing SystemDataXML. Exception: {0}.", e.ToString());
            }

        }

        protected void ProcessRoleDefinitionsXML(IAveWeb web)
        {
            if (mRoleDefinitionObject == null || !mRoleDefinitionObject.ParentWebId.Equals(web.ID.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mRoleDefinitionObject = new SPGenericObject();
                mRoleDefinitionObject.Id = Guid.NewGuid().ToString();
                mRoleDefinitionObject.ParentId = web.ID.ToString();
                mRoleDefinitionObject.ParentWebId = web.ID.ToString();
                mRoleDefinitionObject.ParentWebUrl = web.ServerRelativeUrl.ToString();
                mRoleDefinitionObject.ObjectType = SPObjectType.DeploymentRoles;

                DeploymentRoles roles = new DeploymentRoles();
                roles.Role = new List<DeploymentRole>();
                foreach (var role in web.RoleDefinitions)
                {
                    if (!mBuildinRoleDefinations.Contains(role.ID.ToString()))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = role.Name;
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = role.Order.ToString();
                        roleInfo.Type = ((byte)role.Type).ToString();
                        roleInfo.Description = role.Description == null ? string.Empty : role.Description;
                        roleInfo.Hidden = role.Hidden;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741825"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C0040u";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "206292717568";
                        roleInfo.RoleOrder = "160";
                        roleInfo.Type = "1";
                        roleInfo.Description = "$Resources:fpext,0x001C0046u";
                        roleInfo.Hidden = true;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741826"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Fu";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "756052856929";
                        roleInfo.RoleOrder = "128";
                        roleInfo.Type = "2";
                        roleInfo.Description = "$Resources:fpext,0x001C0045u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741827"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Du";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "1856436900591";
                        roleInfo.RoleOrder = "64";
                        roleInfo.Type = "3";
                        roleInfo.Description = "$Resources:fpext,0x001C0043u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741828"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Cu";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "1856438737919";
                        roleInfo.RoleOrder = "32";
                        roleInfo.Type = "4";
                        roleInfo.Description = "$Resources:fpext,0x001C0042u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741829"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Bu";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "9223372036854775807";
                        roleInfo.RoleOrder = "1";
                        roleInfo.Type = "5";
                        roleInfo.Description = "$Resources:fpext,0x001C0041u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741830"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Eu";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = "1856436902639";
                        roleInfo.RoleOrder = "48";
                        roleInfo.Type = "6";
                        roleInfo.Description = "$Resources:fpext,0x001C0044u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741924"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:xlsrv,RoleNameViewer;";
                        roleInfo.RoleId = role.ID.ToString();
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = role.Order.ToString();
                        roleInfo.Type = ((byte)role.Type).ToString();
                        roleInfo.Description = role.Description;
                        roleInfo.Hidden = role.Hidden;
                        roles.Role.Add(roleInfo);
                    }
                }
                mRoleDefinitionObject.Item = roles;
            }
        }

        private void XmlSerializer(string xmlPath, Type type, object obj)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            using (XmlWriter sw = XmlWriter.Create(xmlPath, new XmlWriterSettings { Encoding = Encoding.UTF8, CheckCharacters = false }))
            {
                serializer.Serialize(sw, obj);
            }
        }

        private void XmlSerializer(string xmlPath, object obj)
        {
            XmlSerializer(xmlPath, obj.GetType(), obj);
        }

        private void StorageManifest(string directoryPath)
        {
            lock (mSPObjectCollection)
            {
                if (mSPObjectCollection.SPObject.Count > 0)
                {
                    if (mRoleDefinitionObject != null && mRoleDefinitionObject.Item != null)
                    {
                        SPGenericObject usedRoleDefinitionObject = (SPGenericObject)mRoleDefinitionObject.Clone();
                        DeploymentRoles originalRoles = mRoleDefinitionObject.Item as DeploymentRoles;
                        DeploymentRoles usedRoles = new DeploymentRoles();
                        foreach (DeploymentRole role in originalRoles.Role)
                        {
                            usedRoles.Role.Add(role);
                        }
                        usedRoleDefinitionObject.Item = usedRoles;
                        mSPObjectCollection.SPObject.Add(usedRoleDefinitionObject);
                    }
                    if (mRoleAssignmentsObject != null && mRoleAssignmentsObject.Item != null)
                    {
                        mSPObjectCollection.SPObject.Add(mRoleAssignmentsObject);
                    }
                    try
                    {
                        for (int i = 0; i < mSPObjectCollection.SPObject.Count; i++)
                        {
                            if (mSPObjectCollection.SPObject[i].ObjectType == HSMCommon.DeploymentXML.SPObjectType.SPList)
                            {
                                mSPObjectCollection.SPObject.Remove(mSPObjectCollection.SPObject[i]);
                                i--;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occured while handle MANIFEST XML {0}", e.ToString());
                    }
                    XmlSerializer(Path.Combine(directoryPath, MIImportConstant.MANIFEST_XML_NAME), mSPObjectCollection);
                    StorageSystemDataXML(directoryPath);
                    mSPObjectCollection.SPObject.Clear();
                    mRoleAssignmentsObject = null;
                }
            }
        }

        private void StorageSystemDataXML(string directoryPath)
        {
            try
            {
                SPSystemData systemData = new SPSystemData();
                SPSchemaVersion schemaVersion = new SPSchemaVersion();
                schemaVersion.Version = "15.0.0.0";
                schemaVersion.SiteVersion = "15";
                schemaVersion.DatabaseVersion = "11552";
                schemaVersion.Build = "16.0.3111.1200";
                schemaVersion.ObjectsProcessed = mSPObjectCollection.SPObject.Count;

                systemData.SchemaVersion = schemaVersion;

                SPManifestFile manifestFile = new SPManifestFile();
                manifestFile.Name = "Manifest.xml";

                systemData.ManifestFiles.Add(manifestFile);

                SPSystemObject systemObject2 = new SPSystemObject();
                systemObject2.Id = mAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.ID.ToString();
                systemObject2.Url = mAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.RootFolder.ServerRelativeUrl;
                systemObject2.Type = SPDeploymentObjectType.List;

                SPSystemObject systemObject3 = new SPSystemObject();
                systemObject3.Id = mAveList.ParentWeb.Site.RootWeb.ID.ToString();
                systemObject3.Url = mAveList.ParentWeb.Site.RootWeb.ServerRelativeUrl;
                systemObject3.Type = SPDeploymentObjectType.Web;

                systemData.SystemObjects.Add(systemObject2);
                systemData.SystemObjects.Add(systemObject3);

                XmlSerializer(Path.Combine(directoryPath, MIImportConstant.SYSTEMDATA_XML_NAME), systemData);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing SystemDataXML. Exception: {0}.", e.ToString());
            }
        }

        private void StorageLookupListMapXml(string directoryPath)
        {
            try
            {
                if (mSPLookupLists.LookupList.Count > 0)
                {
                    XmlSerializer(Path.Combine(directoryPath, MIImportConstant.LOOKUPLISTSMAP_XML_NAME), mSPLookupLists);
                    mSPLookupLists.LookupList.Clear();
                    if (mSPLookupListCollection.Count > 0)
                    {
                        mSPLookupListCollection.Clear();
                        mLog.Info("Clear lookup list collection.");
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while storing LookupListMapXml.xml. Exception: {0}.", e.ToString());
            }
        }

        private void StorageUserGroupXMLXml(string directoryPath)
        {
            try
            {
                mUserGroupMap.Users.Clear();
                mUserGroupMap.Groups.Clear();
                if (mConfig.StubUserInfos != null)
                {
                    List<string> cache = new List<string>();
                    foreach (AveUserInfo userInfo in mConfig.StubUserInfos)
                    {
                        if (mUserGroupMappingForCurrentPackage.Contains(userInfo.ID))
                        {
                            if (!cache.Contains(userInfo.Login))
                            {
                                DeploymentUser dUser = new DeploymentUser();
                                dUser.Id = userInfo.ID.ToString();
                                dUser.Login = userInfo.Login;
                                dUser.Name = userInfo.Title;
                                dUser.Email = userInfo.Email;
                                dUser.IsDomainGroup = userInfo.DomainGroup;
                                dUser.IsSiteAdmin = userInfo.SiteAdmin;
                                dUser.SystemId = Convert.ToBase64String(userInfo.SystemID ?? Guid.NewGuid().ToByteArray());
                                if (userInfo.Deleted == 0)
                                {
                                    dUser.IsDeleted = false;
                                }
                                if (userInfo.Deleted == 1)
                                {
                                    dUser.IsDeleted = true;
                                }
                                if (!string.IsNullOrEmpty(dUser.Login) && !string.IsNullOrEmpty(dUser.Name) && dUser.Login.Equals(dUser.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    dUser.Login = dUser.Name + "_PlaceHolder";
                                }

                                mUserGroupMap.Users.Add(dUser);
                                cache.Add(userInfo.Login);
                            }
                            else
                            {
                                mLog.Info("DeploymentUsers already contains {0}, id:{1},Title{2}", userInfo.Login, userInfo.ID, userInfo.Title);
                            }
                        }
                    }
                }
                if (mConfig.StubGroupInfos != null)
                {
                    foreach (AveGroupInfo group in mConfig.StubGroupInfos)
                    {
                        if (mUserGroupMappingForCurrentPackage.Contains(group.ID))
                        {
                            DeploymentGroup dGroup = new DeploymentGroup();
                            dGroup.Id = group.ID.ToString();
                            dGroup.Name = group.Title;
                            dGroup.Description = group.Description;
                            dGroup.Owner = group.Owner.ToString();
                            dGroup.OwnerIsUser = group.OwnerIsUser;
                            dGroup.RequestToJoinLeaveEmailSetting = "";
                            dGroup.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
                            mUserGroupMap.Groups.Add(dGroup);
                        }
                    }
                }
                XmlSerializer(Path.Combine(directoryPath, MIImportConstant.USETGROUP_XML_NAME), mUserGroupMap);
                try
                {
                    mLog.Info($"StorageUserGroupXMLXml.Groups Count:{mUserGroupMap.Groups.Count}." +
                    $"Users Count:{mUserGroupMap.Users.Count}." +
                    $"mUserGroupMappingForCurrentPackage Count:{mUserGroupMappingForCurrentPackage.Count}." +
                    $"mConfig.StubGroupInfos Count:{mConfig.StubGroupInfos.Count}." +
                    $"mConfig.StubUserInfos Count:{mConfig.StubUserInfos.Count}.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while StorageUserGroupXMLXml. Exception: {0}.", e.ToString());
                }
                mUserGroupMappingForCurrentPackage.Clear();
            }
            catch (Exception e)
            {
                mUserGroupMappingForCurrentPackage.Clear();
                mLog.Error("An error occurred while StorageUserGroupXMLXml. Exception: {0}.", e.ToString());
            }
        }

        private bool CopyMultiFileToFolder(string srcFolderPath, string destFolderPath, bool overWrite)
        {
            try
            {
                DirectoryInfo srcFolder = new DirectoryInfo(srcFolderPath);
                if (!srcFolder.Exists)
                {
                    mLog.Warn("Source folder is not exist.");
                    return false;
                }
                DirectoryInfo destFolder = new DirectoryInfo(destFolderPath);
                if (!destFolder.Exists)
                {
                    mLog.Warn("Destination folder is not exist.");
                    return false;
                }

                foreach (FileInfo file in srcFolder.GetFiles())
                {
                    string destFilePath = Path.Combine(destFolder.FullName, file.Name);
                    System.IO.File.Copy(file.FullName, destFilePath, overWrite);
                }

                //foreach (var user in mUserGroupMap.Users)
                //{
                //    try
                //    {
                //        var mapping = mAveList.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(Convert.ToInt32(user.Id));
                //        if (mapping != null && mapping is AveSPMemberInfo)
                //        {
                //            var memberInfo = mapping as AveSPMemberInfo;
                //            user.Id = memberInfo.NewId.ToString();
                //            //use source login name when user restored failed
                //            if (!string.IsNullOrEmpty(memberInfo.AccountName))
                //            {
                //                user.Login = memberInfo.AccountName;
                //            }
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        mLog.Warn("An error occurred while processing usergroup with mapping. Exception: {0}.", e.ToString());
                //    }
                //}

                return true;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while copying multi-files to folder. Exception: {0}.", e.ToString());
                return false;
            }
        }

        protected void ProcessUserGroupXML()
        {
            try
            {
                mUserGroupMap.Users.Clear();
                mUserGroupMap.Groups.Clear();

                if (mConfig.StubUserInfos != null)
                {
                    List<string> cache = new List<string>();
                    foreach (AveUserInfo userInfo in mConfig.StubUserInfos)
                    {
                        if (!cache.Contains(userInfo.Login))
                        {
                            DeploymentUser dUser = new DeploymentUser();
                            dUser.Id = userInfo.ID.ToString();
                            dUser.Login = userInfo.Login;
                            dUser.Name = userInfo.Title;
                            dUser.Email = userInfo.Email;
                            dUser.IsDomainGroup = userInfo.DomainGroup;
                            dUser.IsSiteAdmin = userInfo.SiteAdmin;
                            dUser.SystemId = Convert.ToBase64String(userInfo.SystemID ?? Guid.NewGuid().ToByteArray());
                            if (userInfo.Deleted == 0)
                            {
                                dUser.IsDeleted = false;
                            }
                            if (userInfo.Deleted == 1)
                            {
                                dUser.IsDeleted = true;
                            }
                            if (!string.IsNullOrEmpty(dUser.Login) && !string.IsNullOrEmpty(dUser.Name) && dUser.Login.Equals(dUser.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                dUser.Login = dUser.Name + "_PlaceHolder";
                            }

                            mUserGroupMap.Users.Add(dUser);
                            cache.Add(userInfo.Login);
                        }
                        else
                        {
                            mLog.Info("DeploymentUsers already contains {0}, id:{1},Title{2}", userInfo.Login, userInfo.ID, userInfo.Title);
                        }
                    }
                }


                if (mConfig.StubGroupInfos != null)
                {
                    foreach (AveGroupInfo group in mConfig.StubGroupInfos)
                    {
                        DeploymentGroup dGroup = new DeploymentGroup();
                        dGroup.Id = group.ID.ToString();
                        dGroup.Name = group.Title;
                        dGroup.Description = group.Description;
                        dGroup.Owner = group.Owner.ToString();
                        dGroup.OwnerIsUser = group.OwnerIsUser;
                        dGroup.RequestToJoinLeaveEmailSetting = "";
                        dGroup.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
                        mUserGroupMap.Groups.Add(dGroup);
                    }
                }

                XmlSerializer(Path.Combine(tempJobPath, MIImportConstant.USETGROUP_XML_NAME), mUserGroupMap);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing usergroup xml. Exception: {0}.", e.ToString());
            }
        }
        #endregion

        #region sharepoint

        protected SPFieldCollection ProcessFieldCollection(SPListItem listItem, AveSPItem item, int docRowId, int version, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction)
        {
            SPFieldCollection fieldCollection = new SPFieldCollection();
            try
            {
                string taxonomyListId = Guid.Empty.ToString();
                AveSPList mAveSPList = mConfig.StubRestoreAveSPRootFolder.ParentList;
                //Wait Wrapper Team provide method
                ItemMetadata itemData = new ItemMetadata(mConfig.aveObjectModelFactory, item, version, docRowId, userData, dataJunction);
                Dictionary<string, AveFieldValueInfo> fieldValues = itemData.ProcessItemMetadata();
                List<string> NeedSetNullFields = mAveSPList.SetNeedSetNullFieldsEx(fieldValues.Keys.ToList());
                var termIdCache = new List<string>();
                foreach (var fieldValue in fieldValues)
                {
                    string columnName = fieldValue.Key;
                    AveFieldValueInfo valueInfo = fieldValue.Value;
                    if (valueInfo.ColValue == null)
                    {
                        mLog.Log(AveLogLevel.DEBUG, "The column value is null,need skip.Column:{0}.", fieldValue.Key);
                        continue;
                    }

                    SPField field = new SPField();

                    switch (columnName)
                    {
                        case "Author":
                            listItem.Author = valueInfo.ColValue.ToString();
                            try
                            {
                                lock (mSPObjectCollection)
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Info($"An error occur when ProcessFieldCollection Author.Message:{ex}");
                            }
                            continue;
                        case "Editor":
                            listItem.ModifiedBy = valueInfo.ColValue.ToString();
                            try
                            {
                                lock (mSPObjectCollection)
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Info($"An error occur when ProcessFieldCollection Editor.Message:{ex}");
                            }
                            continue;
                        case "Modified":
                            listItem.TimeLastModified = (DateTime)valueInfo.ColValue;
                            continue;
                        case "Created":
                            listItem.TimeCreated = (DateTime)valueInfo.ColValue;
                            continue;
                        case "ContentType":
                            listItem.ContentTypeId = valueInfo.ColValue.ToString();
                            continue;
                        case "Order":
                            listItem.Order = Convert.ToSingle(valueInfo.ColValue);
                            continue;
                        case "Modified_x0020_By":
                        case "Created_x0020_By":
                        case LinkFileCommon.LinkFileFieldName:
                            continue;
                    }
                    #region process field
                    lock (fieldslock)
                    {
                        switch (valueInfo.FieldType)
                        {
                            case AveFieldType.Lookup:
                                field = ProcessLookupColumnValue(field, valueInfo);
                                break;
                            case AveFieldType.URL:
                                field.Value = valueInfo.ColValue.ToString();
                                if (!columnName.EndsWith("#2"))
                                {
                                    if (fieldValues.ContainsKey(columnName + "#2"))
                                    {
                                        field.Value2 = fieldValues[columnName + "#2"].ColValue.ToString();
                                    }
                                    else
                                    {
                                        mLog.Debug("No description of hyperlink column, column name:{0}", columnName);
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                break;
                            case AveFieldType.User:
                                field.Value = valueInfo.ColValue.ToString() + ";UserInfo";
                                try
                                {
                                    string[] userIDs = valueInfo.ColValue.ToString().Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int i = 0; i < userIDs.Length; i++)
                                    {
                                        try
                                        {
                                            int userPricinpleId = Convert.ToInt32(userIDs[i]);
                                            lock (mSPObjectCollection)
                                            {
                                                if ((!userIDs[i].Contains("\\")) && !mUserGroupMappingForCurrentPackage.Contains(userPricinpleId))
                                                {
                                                    mUserGroupMappingForCurrentPackage.Add(userPricinpleId);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            mLog.Info($"An error occurred while add user to user cache {ex}.UserID:{userIDs[i]}.");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("An error occurred while add user to user cache {0}", e.ToString());
                                }
                                break;
                            case AveFieldType.DateTime:
                                try
                                {
                                    DateTime currentDate = Convert.ToDateTime(valueInfo.ColValue);
                                    //field.Value
                                    field.Value = currentDate.ToString("MM/dd/yyyy hh:mm:ss tt", DateTimeFormatInfo.InvariantInfo);
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("An error occurred while convert datetime {0}", e.ToString());
                                    field.Value = valueInfo.ColValue.ToString();
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (columnName.Equals("Facilities", StringComparison.OrdinalIgnoreCase))
                                {
                                    field = ProcessLookupColumnValue(field, valueInfo);
                                }
                                else
                                {
                                    try
                                    {
                                        var termInfo = GetTermInfo(columnName, valueInfo, DefaultLCID, false);

                                        SPField taxonomyTextField = new SPField()
                                        {
                                            ID = termInfo.TextFieldId.ToString(),
                                            Name = termInfo.TextFieldName,
                                            Value = valueInfo.ColValue.ToString(),
                                        };
                                        fieldCollection.Field.Add(taxonomyTextField);

                                        if (valueInfo.ColValue.ToString().Contains(";"))
                                        {
                                            field.Value = valueInfo.ColValue.ToString().Replace(";", ";#-1;#");
                                            field.Value = "-1;#" + field.Value;
                                        }
                                        else
                                        {
                                            field.Value = "-1;#" + valueInfo.ColValue.ToString();
                                        }
                                        if (NeedSetNullFields.Contains(termInfo.TextFieldName))
                                        {
                                            NeedSetNullFields.Remove(termInfo.TextFieldName);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Warn("An error occurred while getting terms info, exception:{0}", e.ToString());
                                        //BUG ADO-180692 Need to Clear the column value while the value could not be found in the destination.
                                        //resolve list issue
                                        //field.Value = valueInfo.ColValue.ToString();
                                        if (columnName.Equals("LikesCount"))
                                        {
                                            field.Value = valueInfo.ColValue.ToString();
                                        }
                                        if (columnName.Equals("AverageRating"))
                                        {
                                            field.Value = valueInfo.ColValue.ToString();
                                        }
                                    }
                                }
                                break;
                            case AveFieldType.Currency:
                            case AveFieldType.Number:
                                field.Value = valueInfo.ColValue.ToString();
                                if (Thread.CurrentThread.CurrentUICulture.LCID == 1031)
                                {
                                    field.Value = field.Value.Replace(',', '.');
                                }
                                mLog.Info("Handle currency and number field type, field value:{0}", field.Value);
                                break;
                            default:
                                field.Value = valueInfo.ColValue.ToString();
                                break;
                        }
                    }
                    #endregion

                    field.Name = columnName;
                    if (field.Name.Equals("FormData", StringComparison.OrdinalIgnoreCase))
                    {
                        IAveField formField = mAveSPList.AveFields.GetFieldByInternalName("NFFormData");
                        if (formField != null)
                        {
                            field.Name = "NFFormData";
                            field.ID = formField.ID.ToString();
                            NeedSetNullFields.Remove("NFFormData");
                        }
                    }
                    field.ID = valueInfo.Id.ToString();
                    fieldCollection.Field.Add(field);
                }
                #region add ArchiverLinkFileType column
                SPField linkfield = new SPField();
                linkfield.Name = LinkFileCommon.LinkFileFieldName;
                linkfield.Value = LinkFileCommon.GenerateLinkFieldValue(mConfig.JobId);
                linkfield.ID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
                fieldCollection.Field.Add(linkfield);
                foreach (var name in NeedSetNullFields)
                {
                    if (name.Equals(LinkFileCommon.LinkFileFieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    SPField field = new SPField();
                    field.Name = name;
                    field.Value = null;
                    fieldCollection.Field.Add(field);
                }
                #endregion
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while processing fields, Exception:{0}", e.ToString());
            }
            return fieldCollection;
        }

        private MetadataCacheInfo GetTermInfo(string columnName, AveFieldValueInfo valueInfo, int LCID = -1, bool forceAddTerm = true)
        {
            IAveTaxonomyField tField = null;
            Dictionary<Guid, IAveTerm> termCache = new Dictionary<Guid, IAveTerm>();
            Dictionary<Guid, Guid> termIdMapping = new Dictionary<Guid, Guid>();
            Dictionary<Guid, List<Guid>> mergedTermIdMapping = new Dictionary<Guid, List<Guid>>();
            tField = GetTaxonomyField(columnName);
            var textField = mConfig.DeletionIAveList.Fields.GetFieldById(tField.TextField, false);

            if (!mMetadataInfoList.ContainsKey(columnName))
            {
                mMetadataInfoList[columnName] = new MetadataCacheInfo { TextFieldName = textField.InternalName, TextFieldId = textField.ID };
            }

            IAveTaxonomySession session = mConfig.DeletionIAveSite.AveSPTaxonomySession;
            IAveTermStore termStore = GetTermStore(tField, session, ref LCID);
            if (termStore == null)
            {
                return null;
            }

            IAveTermSet termSet = null;
            if (tField.TermSetId != Guid.Empty && termStore != null)
            {
                termSet = termStore.GetTermSet(tField.TermSetId);
            }

            IAveTerm endTerm = null;
            if (tField.AnchorId != Guid.Empty && termSet != null)
            {
                endTerm = termSet.GetTerm(tField.AnchorId);
            }

            var columnValue = valueInfo.ColValue.ToString();
            var newColumnValue = string.Empty;

            bool submit = false;
            HashSet<String> termNames = new HashSet<string>(columnValue.Split(';'), StringComparer.OrdinalIgnoreCase);

            foreach (string termName in termNames)
            {
                if (!mMetadataInfoList[columnName].TermValueMapping.ContainsKey(termName))
                {
                    string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                    if (string.IsNullOrEmpty(tName) || string.IsNullOrEmpty(tName.Trim()))
                    {
                        continue;
                    }
                    var term = AveTaxonomyFieldUtility.FindTerm(tName, LCID, forceAddTerm, endTerm, termSet, tField, session, termCache, termIdMapping, mergedTermIdMapping, termStore, ref submit);
                    if (term != null)
                    { 
                        var mappedValue = term.Name + "|" + term.ID;
                        if (!mMetadataInfoList[columnName].TermValueMapping.ContainsKey(termName))
                        {
                            mMetadataInfoList[columnName].TermValueMapping[termName] = mappedValue;
                        }
                        newColumnValue += mappedValue + ";";


                        //如果field不允许多值，没有必要找多个term了。
                        if (!tField.AllowMultipleValues)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    newColumnValue += mMetadataInfoList[columnName].TermValueMapping[termName] + ";";
                }
            }

            valueInfo.ColValue = newColumnValue.TrimEnd(';');

            return mMetadataInfoList[columnName];
        }

        private IAveTaxonomyField GetTaxonomyField(string columnName)
        {
            IAveTaxonomyField taxonomyField;
            var aveField = mConfig.DeletionIAveList.Fields.GetFieldByInternalName(columnName, false);
            if (aveField != null && aveField is IAveTaxonomyField)
            {
                taxonomyField = aveField as IAveTaxonomyField;
                //taxonomySchemaField.Name = textField.InternalName;
            }
            else
            {
                throw new ArgumentException("Can't find Taxonomy Field or field type is not taxonomy, column name:{0}", columnName);
                //taxonomySchemaField.Name = mTaxonomyDic[columnName];
            }
            return taxonomyField;
        }

        private IAveTermStore GetTermStore(IAveField field, IAveTaxonomySession session, ref int LCID)
        {
            IAveTermStore termStore = null;
            IAveTaxonomyField tField = field as IAveTaxonomyField;
            Guid sspId = Guid.Empty;
            if (tField.SspId == Guid.Empty && !tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                object customProperty = field.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    sspId = new Guid(customProperty.ToString());
                }
            }
            else
            {
                sspId = tField.SspId;
            }
            if (sspId != Guid.Empty)
            {
                try
                {
                    termStore = session.TermStores[sspId];
                }
                catch (Exception ex)
                {
                    //如果原端的field使用的service不在被原端引用，也就是说mms没有被还原，该field的原端属性无法替换，这个sspid也是原端的Id，这时在目的端无法找到
                    //为了保障其他的mms field属性的正确还原，添加try catch，跳过该field的还原
                    mLog.Log(AveLogLevel.WARN, "Can not Get TermStore by sspId:{0},Skip to restore this field value.Exception:{1}.", sspId, ex.ToString());
                    return null;
                }
            }
            else
            {
                termStore = session.DefaultKeywordsTermStore;
                if (termStore == null)
                {
                    termStore = session.DefaultSiteCollectionTermStore;
                }
                if (termStore == null)
                {
                    termStore = session.TermStores[0];
                }
            }
            if (LCID < 0)
            {
                LCID = termStore.WorkingLanguage;
            }
            if (termStore != null && !termStore.Languages.Contains(DefaultLCID))
            {
                DefaultLCID = termStore.WorkingLanguage;
                LCID = DefaultLCID;
            }
            return termStore;
        }

        private SPField ProcessLookupColumnValue(SPField field, AveFieldValueInfo valueInfo)
        {
            //ADO-190596
            SPLookupItem mSPLookupItem = null;
            try
            {
                SPLookupList spLookupListInfo = null;
                IAveFieldLookup lookupField = mConfig.DeletionIAveList.Fields.GetById(valueInfo.Id) as IAveFieldLookup;
                Guid lookupListId = new Guid(lookupField.LookupList);
                IAveList lookupList = mConfig.DeletionIAveList.ParentWeb.Site.OpenWeb(lookupField.LookupWebId).GetList(lookupListId);
                //lookup list already exists in the destination
                if (lookupList != null)
                {
                    if (!mSPLookupListCollection.ContainsKey(lookupListId))
                    {
                        spLookupListInfo = new SPLookupList()
                        {
                            Included = false,
                            Url = lookupList.RootFolder.ServerRelativeUrl,
                            Id = lookupList.ID.ToString(),
                        };
                        mSPLookupLists.LookupList.Add(spLookupListInfo);
                        mSPLookupListCollection.Add(lookupList.ID, spLookupListInfo);
                    }
                    else
                    {
                        spLookupListInfo = mSPLookupListCollection[lookupListId];
                    }
                    //lookup item already exists in the destination
                    if (lookupField.AllowMultipleValues)
                    {
                        //mulit lookup values
                        if (valueInfo.ColValue != null && valueInfo.ColValue is List<LookupItemValue>)
                        {
                            List<LookupItemValue> lookupItemValues = valueInfo.ColValue as List<LookupItemValue>;
                            string lookupItemIdStrs = null;
                            foreach (LookupItemValue value in lookupItemValues)
                            {
                                try
                                {
                                    IAveListItem lookupItem = lookupList.GetItemById(value.ItemRowId);
                                    mSPLookupItem = new SPLookupItem()
                                    {
                                        Included = false,
                                        Url = lookupList.ParentWeb.ServerRelativeUrl + "/" + lookupItem.Url,
                                        Id = lookupItem.ID.ToString(),
                                        DocId = lookupItem.UniqueId.ToString(),
                                    };
                                    //防止LookupListMap.xml加入重复的lookup item信息
                                    if (mSPLookupListCollection.ContainsKey(lookupListId) && mSPLookupListCollection[lookupListId].LookupItems.Where(j => j.DocId == mSPLookupItem.DocId).ToList().Count == 0)
                                    {
                                        mSPLookupListCollection[lookupListId].LookupItems.Add(mSPLookupItem);
                                    }

                                    lookupItemIdStrs += lookupItem.ID.ToString() + ";# ;#";
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("An error occurred while getting multi lookup value, Message:{0}", e.ToString());
                                }
                            }
                            //for manifest file xml --> value = itemID;# ;#itemID;# ;itemIDLookupListID
                            if (!string.IsNullOrEmpty(lookupItemIdStrs))
                            {
                                field.Value = lookupItemIdStrs.TrimEnd('#') + lookupListId.ToString();
                            }
                            else
                            {
                                field.Value = lookupListId.ToString();
                            }
                        }
                    }
                    else
                    {
                        //single value
                        if (valueInfo.ColValue != null && valueInfo.ColValue is LookupItemValue)
                        {
                            LookupItemValue lookupItemValue = valueInfo.ColValue as LookupItemValue;
                            IAveListItem lookupItem = lookupList.GetItemById(lookupItemValue.ItemRowId);
                            mSPLookupItem = new SPLookupItem()
                            {
                                Included = false,
                                Url = lookupList.ParentWeb.ServerRelativeUrl + "/" + lookupItem.Url,
                                Id = lookupItem.ID.ToString(),
                                DocId = lookupItem.UniqueId.ToString(),
                            };
                            //防止LookupListMap.xml 加入重复的lookup item 信息
                            if (mSPLookupListCollection.ContainsKey(lookupListId) &&
                                mSPLookupListCollection[lookupListId].LookupItems.Where(j => j.DocId == mSPLookupItem.DocId).ToList().Count == 0)
                            {
                                mSPLookupListCollection[lookupListId].LookupItems.Add(mSPLookupItem);
                            }
                            //for manifest file xml --> value = itemID;LookupListID
                            field.Value = lookupItem.ID.ToString() + ";" + lookupListId.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Error occurred while getting the lookup value for HSM, Info:{0}", e.ToString());
            }
            return field;
        }

        #endregion

        #region Dispose methods
        public static void ResetInstance()
        {
            lock (padlock)
            {
                if (instanceMapping == null)
                {
                    return;
                }
                HashSet<CreateLinkFileByPackage> oldCache = instanceMapping.Values.ToHashSet();
                foreach (var pair in oldCache)
                {
                    pair?.Dispose();
                }
            }

        }

        public void Dispose()
        {
            try
            {
                lock (padlock)
                {
                    instanceMapping.Remove(this.mConfig);
                }
                //now no resource need release in dispose method
            }
            catch (Exception e)
            {
                mLog.Error($"Fail disposal hsm CreateLinkFileByPackage, e:{e}");
            }
        }
        #endregion

    }

    #region inner class
    class MetadataCacheInfo
    {
        public string TextFieldName { get; set; }
        public Guid TextFieldId { get; set; }
        public Dictionary<string, string> TermValueMapping { get; set; }

        public MetadataCacheInfo()
        {
            TermValueMapping = new Dictionary<string, string>();
        }
    }
    #endregion

    public class ImportJobResources
    {
        public string ListTitle;
        public int JobCount = 0;
        public bool AddJobsFinished = false;
        public string WebName;
        private Dictionary<string, CreateLinkFileReportDto> reportDto = new Dictionary<string, CreateLinkFileReportDto>();

        private readonly object locker = new object();

        public void AddReports(string key, CreateLinkFileReportDto reports)
        {
            lock (locker)
            {
                reportDto.TryAdd(key, reports);
            }
        }

        public bool ContainsReport(string key)
        {
            lock (locker)
                return reportDto.ContainsKey(key);
        }

        public CreateLinkFileReportDto GetReport(string key)
        {
            lock (locker)
                return reportDto[key];
        }

        public void SetReportStatus(string key, JobDetailsStatus status,string message = "")
        {
            lock (locker)
            {
                if (reportDto.ContainsKey(key))
                {
                    reportDto[key].Status = status;
                    reportDto[key].Message = reportDto[key].Message + I18NEntity.MultiI18nSeparator + message;
                }
            }
        }

        public void SetReportMessage(string key, string message = "")
        {
            lock (locker)
            {
                if (reportDto.ContainsKey(key))
                {
                    reportDto[key].Message = reportDto[key].Message + I18NEntity.MultiI18nSeparator + message;
                }
            }
        }

        public void SetReportStatusAndMessage(string key, JobDetailsStatus status, string message)
        {
            lock (locker)
            {
                if (reportDto.ContainsKey(key))
                {
                    var rep = reportDto[key];
                    rep.Status = status;
                    rep.Message = message;
                }
            }
        }

        public void RemoveReports(string key)
        {
            lock (locker)
            {
                reportDto.Remove(key);
            }
        }

        public void SetReportStatusByUrl(string url, JobDetailsStatus status)
        {
            lock (locker)
            {
                var temp = reportDto.Where(v => v.Value.FileUrl.EndsWith(url));
                if (temp.Count() > 0)
                {
                    reportDto[temp.FirstOrDefault().Key].Status = status;
                }
            }
        }

        public void SetReportStatusAndMessageByUrl(string url, JobDetailsStatus status, string message)
        {
            lock (locker)
            {
                var temp = reportDto.Where(v => v.Value.FileUrl.EndsWith(url));
                if (temp.Count() > 0)
                {
                    var rep = reportDto[temp.FirstOrDefault().Key];
                    rep.Status = status;
                    rep.Message = message;
                }
            }
        }

        public void AddOrUpdateVersionReport(string key, List<MigrationRestoreVersionDto> versionDtos)
        {
            lock (locker)
            {
                if (reportDto.TryGetValue(key, out var rep) && rep is MigrationRestoreFileDto mRep)
                {
                    (mRep.VersionsReportDtos ??= []).AddRange(versionDtos);
                }
            }
        }
    }

}
