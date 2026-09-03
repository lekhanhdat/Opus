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
using Aspose.Pdf.Operators;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using HSMAzureCommon;
using HSMCommon;
using HSMCommon.DeploymentXML;
using Newtonsoft.Json;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class HSMConnector : IDisposable
    {
        #region private fields
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string HSMFOLDER = "HSM";
        private bool start = false;
        private static readonly object padlock = new object();
        private static readonly object fieldslock = new object();
        private ScheduleConfiguration mConfig;
        //private CGDBReader dbReader;
        private string fileNameSUFFIX = string.Empty;
        private bool hasProcessedUserXML = false;
        private Dictionary<string, MetadataCacheInfo> mMetadataInfoList = new Dictionary<string, MetadataCacheInfo>();
        private static Dictionary<ScheduleConfiguration, HSMConnector> instanceMapping = new();


        string tempJobPath = string.Empty;
        private SPGenericObjectCollection mSPObjectCollection { get; set; }
        private SPGenericObject mParentSPFolderObject { get; set; }
        private SPGenericObject mRoleDefinitionObject { get; set; }
        private SPGenericObject mRoleAssignmentsObject { get; set; }

        DeploymentUserGroupMap mUserGroupMap = new DeploymentUserGroupMap();
        private List<int> mUserGroupMappingForCurrentPackage = new List<int>();

        protected List<SPGenericObject> mCacheSPFolderObjects = new List<SPGenericObject>();
        List<string> mBuildinRoleDefinations = new List<string>() { "1073741825", "1073741826", "1073741827", "1073741828", "1073741829", "1073741830", "1073741924" };
        private int mCurrentPackageCountCapacity = 0;
        private HSMLocalInfo currentHSMLocalInfo = null;

        //it best store two mapping to sqlite.
        private List<HSMFileMapping> mHSMFileMappings = null;
        private Dictionary<string, HSMLocalInfo> mHSMLocalInfos = null;
        private List<StubBasicInfo> mStubBasicInfoList = new List<StubBasicInfo>();

        private static int DefaultLCID = -1;
        private bool mReadProcessHasStart = false;

        private IAveList mCurrentAveList { get; set; }
        private Dictionary<Guid, SPLookupList> mSPLookupListCollection = new Dictionary<Guid, SPLookupList>();
        private SPLookupLists mSPLookupLists
        {
            get;
            set;
        }

        private bool Finished { get; set; }
        private bool ProcessedFinished { get; set; }

        private Queue<HSMStubInfo> mHSMFileQueue = new Queue<HSMStubInfo>();
        private DB4HSMStub mDB4HSM = null;
        private HSMJobRunner HSMJobsRunner = null;

        private SPSchemaVersion _ConfiguredSPSchemaVersion = null;

        #endregion

        #region public fields
        public Exception Error { get; private set; }
        #endregion

        #region ctor
        public static HSMConnector GetInstance(ScheduleConfiguration config)
        {
            lock (padlock)
            {
                if (!instanceMapping.ContainsKey(config))
                {
                    instanceMapping.Add(config, new HSMConnector(config));
                }
                return instanceMapping[config];
            }
        }

        public HSMConnector(ScheduleConfiguration config)
        {
            mConfig = config;

            Init();
        }

        public static void ResetInstance()
        {
            if (instanceMapping == null)
            {
                return;
            }
            lock (padlock)
            {
                HashSet<HSMConnector> oldCache = instanceMapping.Values.ToHashSet();
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
                mLog.Error($"Fail disposal hsm connector, e:{e}");
            }
        }

        private void Init()
        {
            if (mConfig.currentRule != null)
            {
                mLog.Info($"HSM connector init.");
                mSPObjectCollection = new SPGenericObjectCollection();
                mSPLookupLists = new SPLookupLists();
                tempJobPath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, mConfig.TenantGroupId, mConfig.JobId, HSMFOLDER);
                if (!Directory.Exists(tempJobPath))
                {
                    Directory.CreateDirectory(tempJobPath);
                }
                fileNameSUFFIX = LinkFileCommon.GetStubFileNameSuffix(mConfig);
                mCurrentAveList = null;
                bool deleteWithNoBackup = mConfig.actionType == ActionType.DeleteOnly || mConfig.actionType == ActionType.ExportBeforeDelete || mConfig.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfig.currentRule);
                if (isLinkToDucument && !deleteWithNoBackup)
                {
                    mLog.Info($"HSM connector will start.");
                    start = true;
                    mDB4HSM = new DB4HSMStub(mConfig.TenantGroupId, mConfig.JobId);
                    Run();
                }
                else
                {
                    mLog.Info($"HSM connector will not start. DeleteWithNoBackup {deleteWithNoBackup} isLinkToDucument {isLinkToDucument}");
                }
                HSMJobsRunner = new HSMJobRunner(mConfig);
            }
        }
        #endregion

        #region public

        public Dictionary<string, HSMLocalInfo> HSMLocalInfos
        {
            get
            {
                if (mHSMLocalInfos == null)
                {
                    lock (padlock)
                    {
                        if (mHSMLocalInfos == null)
                        {
                            mHSMLocalInfos = new Dictionary<string, HSMLocalInfo>();
                        }
                        return mHSMLocalInfos;
                    }
                }
                else
                {
                    return mHSMLocalInfos;
                }
            }
            private set { }
        }

        public List<HSMFileMapping> HSMFileMappings
        {
            get
            {
                if (mHSMFileMappings == null)
                {
                    lock (padlock)
                    {
                        if (mHSMFileMappings == null)
                        {
                            mHSMFileMappings = new List<HSMFileMapping>();
                        }
                        return mHSMFileMappings;
                    }
                }
                else
                {
                    return mHSMFileMappings;
                }
            }
            private set { }
        }

        public DB4HSMStub DBForHSMStub
        {
            get
            {
                return mDB4HSM;
            }
            private set { }
        }

        public bool IsStart
        {
            get { return start; }
            private set { }
        }
        public void Add2Queue(HSMStubInfo auditFileInfo)
        {
            if (start)
            {
                lock (mHSMFileQueue)
                {
                    while (mHSMFileQueue.Count > 250)
                    {
                        mLog.Info($"HSMFileQueue count {mHSMFileQueue.Count}, will wait.");
                        Monitor.Wait(mHSMFileQueue);
                        mLog.Info($"HSMFileQueue Continue Enqueue.");
                    }

                    mHSMFileQueue.Enqueue(auditFileInfo);
                    Monitor.Pulse(mHSMFileQueue);
                }
            }
        }

        public void WaitingQueueFinshed()
        {
            if (start)
            {
                mLog.Info("WaitingQueueFinshed");
                long currentCount = mHSMFileQueue.Count;
                int waitCount = 0;
                while (!ProcessedFinished)
                {
                    mLog.Info($"Will sleep 5s and wait HSMConnector queue free. current left count : {mHSMFileQueue.Count}");
                    Thread.Sleep(5 * 1000);
                    waitCount++;
                    if (currentCount != mHSMFileQueue.Count)
                    {
                        currentCount = mHSMFileQueue.Count;
                        waitCount = 0;
                    }
                    else
                    {
                        if (waitCount > 6 * 60)
                        {
                            mLog.Info("May be hang, will break wait.");
                            Error = new Exception("UnExpectedException");
                            break;
                        }
                    }
                }
                SplitPackage(true);
            }
        }

        public void Reset()
        {
            mLog.Info("HSMConnector Reset");
            start = false;
            Finished = false;
            ProcessedFinished = false;
            mHSMLocalInfos = null;
            mHSMFileMappings = null;
            mSPObjectCollection = new SPGenericObjectCollection();
            mSPLookupLists = new SPLookupLists();
            tempJobPath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, mConfig.TenantGroupId, mConfig.JobId, HSMFOLDER);
            if (!Directory.Exists(tempJobPath))
            {
                Directory.CreateDirectory(tempJobPath);
            }
            if (mConfig.currentRule != null)
            {
                bool deleteWithNoBackup = mConfig.actionType == ActionType.DeleteOnly || mConfig.actionType == ActionType.ExportBeforeDelete || mConfig.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfig.currentRule);
                mCurrentAveList = null;
                if (isLinkToDucument && !deleteWithNoBackup)
                {
                    mLog.Info($"HSM connector will start.");
                    fileNameSUFFIX = LinkFileCommon.GetStubFileNameSuffix(mConfig);
                    start = true;
                    mDB4HSM = new DB4HSMStub(mConfig.TenantGroupId, mConfig.JobId);
                    Run();
                }
                else
                {
                    mLog.Info($"HSM connector will not start. DeleteWithNoBackup {deleteWithNoBackup} isLinkToDucument {isLinkToDucument}");
                }
                HSMJobsRunner = new HSMJobRunner(mConfig);
            }
        }

        public void Finish()
        {
            mLog.Info("HSMConnector Set Finish.");
            if (start)
            {
                lock (mHSMFileQueue)
                {
                    Finished = true;
                    Monitor.Pulse(mHSMFileQueue);
                }
            }
        }

        public void RebuildJobManifestXML(string containerid, List<HSMFileMapping> removeItems)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("HSMStub.RebuildJobManifestXML"))
            {
                if (removeItems.Count > 0)
                {
                    List<string> itemids = (from item in removeItems select item.FileNewID.ToString()).ToList();
                    if (HSMLocalInfos.ContainsKey(containerid))
                    {
                        var container = HSMLocalInfos[containerid];
                        var file = Path.Combine(container.MetadataContainerPath, MIImportConstant.MANIFEST_XML_NAME);
                        if (File.Exists(file))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(file);
                            var nodes = doc.DocumentElement.ChildNodes;
                            List<XmlNode> needRemoveNodes = new List<XmlNode>();
                            foreach (XmlNode node in nodes)
                            {
                                var ele = (XmlElement)node;
                                if (ele.HasAttribute("Id") && itemids.Contains(ele.GetAttribute("Id")))
                                {
                                    needRemoveNodes.Add(node);
                                }
                            }
                            foreach (var node in needRemoveNodes)
                            {
                                doc.DocumentElement.RemoveChild(node);
                            }
                            //File.Delete(file);
                            doc.Save(file);
                        }
                        else
                        {
                            mLog.Warn($"RebuildJobManifestXML:Cannot find file {file}");
                        }
                    }
                    else
                    {
                        mLog.Warn($"RebuildJobManifestXML:Cannot find {containerid}");
                    }
                }
                else
                {
                    mLog.Info($"RebuildJobManifestXML:removeItems is empty, containerid {containerid}");
                }
            }
        }

        public void AddImportJobTask(IAveSite site, Guid webId, Guid listId, string containerId, bool isEncryption, string listUrl, List<ARRestoreFileInfo> currentPackageItemsList)
        {
            if (IsStart && HSMLocalInfos.ContainsKey(containerId))
            {
                mLog.Info($"Start add import job task, containerId: {containerId}");
                var localInfo = HSMLocalInfos[containerId];

                DecompressPackage(localInfo);

                foreach (var item in currentPackageItemsList)
                {
                    string url = WebUtil.MakeFullUrl(site.Url, item.serverRelativeUrl);
                    HSMJobsRunner.AddReportItemInfo(listId, url, item.id, item.MD5);
                }
                mLog.Info($"AddImportJobTask.listId:{listId}.DataContainerPath:{localInfo.DataContainerPath}.MetadataContainerPath:{localInfo.MetadataContainerPath}.currentPackageItemsList:{currentPackageItemsList.Count}.");
                HSMJobsRunner.AddImportJobTask(site, webId, listId, localInfo.DataContainerPath, localInfo.MetadataContainerPath, isEncryption, listUrl, currentPackageItemsList);
            }
            else
            {
                mLog.Error($"AddImportJobTask IsStart:{IsStart}.HSMLocalInfos.ContainsKey(containerId):{HSMLocalInfos.ContainsKey(containerId)}.");
            }
        }

        public void WatingCompleted()
        {
            if (IsStart)
            {
                HSMJobsRunner.WatingCompleted();
                if (mConfig.mOffice365AlertUtil != null)
                {
                    mConfig.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                }
            }
        }

        public void UploadDataToReportLocation()
        {
            try
            {
                bool needUpload = false;
                string path1 = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, mConfig.TenantGroupId, mConfig.JobId);
                string targetpath = string.Format("{0}\\{1}{2}", BackgroundSettings.GetInstance().ArchiveTemp, "ARReport_", mConfig.JobId);
                DirectoryInfo dir1 = new DirectoryInfo(path1);
                if (dir1.Exists && (dir1.GetDirectories().Length > 0 || dir1.GetFiles().Length > 0))
                {
                    CopyImportReport(path1, targetpath, true);
                    needUpload = true;
                }

                if (needUpload)
                {
                    string lowName = Path.GetFileName(targetpath) + ".zip";
                    string zipFilePath = targetpath + ".zip";
                    try
                    {
                        GCommon.ZipUtil.ZipFolder(targetpath, zipFilePath);
                        mLog.Info($"Delete Directory [{targetpath}].Location:AveReportUploader.GetZipFile");
                        Directory.Delete(targetpath, true);
                        if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]))
                        {
                            mLog.Info($"start to upload file");
                            var tenantFolderName = Path.Combine(mConfig.TenantGroupId, "RMArchiverHSM", mConfig.JobId, lowName);

                            RAStorageUtil.UploadReportBlob(tenantFolderName, zipFilePath);
                            mLog.Info($"finish to upload blob name:{lowName}");
                            DeleteFile(zipFilePath);
                            mLog.Info($"finish to delete file.");
                            //DeleteFile(zipFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("upload hsm report folder failed.{0}", ex);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("UploadDataToReportLocation failed.{0}", e);
            }
        }

        #endregion


        #region private
        private void Run()
        {
            //UserMappings = cloudInsightsConnector.GetUserMappings();
            var mProcessor = new Thread(Process) { IsBackground = true };
            mProcessor.Start();
        }
        private void Process()
        {
            if (!mReadProcessHasStart)
            {
                try
                {
                    mReadProcessHasStart = true;
                    while (!ProcessedFinished)
                    {
                        HSMStubInfo mCurrent = null;
                        lock (mHSMFileQueue)
                        {
                            while (mHSMFileQueue.Count == 0)
                            {
                                if (Error != null)
                                {
                                    throw Error;
                                }
                                if (Finished)
                                {
                                    mLog.Info("HSMConnector Processed finished");
                                    ProcessedFinished = true;
                                    break;
                                }
                                Monitor.Wait(mHSMFileQueue);
                            }
                            if (mHSMFileQueue.Count > 0)
                            {
                                mCurrent = mHSMFileQueue.Dequeue();
                                Monitor.Pulse(mHSMFileQueue);
                            }
                        }
                        if (mCurrent != null)
                        {
                            AssembleItem2HSMPackageAsync(mCurrent).Wait();
                        }
                    }
                }
                catch (AveSkipLockSiteException e)
                {
                    mLog.Error("HSMConnector Process Skip Locked site exception : {0}", e);
                    ProcessedFinished = true;
                    Error = e;
                }
                catch (Exception e)
                {
                    if (e is AggregateException && e?.InnerException is AveSkipLockSiteException)
                    {
                        mLog.Error("HSMConnector Process Skip Locked site exception : {0}", e);
                        ProcessedFinished = true;
                    }

                    mLog.Error("HSMConnector Process exception : {0}", e.ToString());
                    Error = e;
                }
                finally
                {
                    mReadProcessHasStart = false;
                    Finish();
                }
            }
        }

        private void SplitPackage(bool isLastPackage = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("HSMStub.SplitPackage"))
            {
                lock (mSPObjectCollection)
                {
                    if (isLastPackage)
                    {
                        lock (padlock)
                        {
                            if (HSMFileMappings.Count > 0)
                            {
                                mLog.Info($"Will insert to db {mDB4HSM.dbFilePath}");
                                mDB4HSM.InsertValueToDB(HSMFileMappings);
                                HSMFileMappings.Clear();
                            }
                        }
                        if (mSPObjectCollection.SPObject.Count > 0)
                        {
                            if (mCurrentAveList != null)
                            {
                                SplitPackageFiles(isLastPackage);
                            }
                            mCurrentPackageCountCapacity = 0;
                            mSPObjectCollection.SPObject.Clear();
                            //GenerateCurrentFolderObjects();
                            //mCurrentPackageNumber = 0;
                        }
                    }
                    else
                    {
                        if (mCurrentPackageCountCapacity == MIImportConstant.PackageCountCapacity)
                        {
                            if (mCurrentAveList != null)
                            {
                                SplitPackageFiles(isLastPackage);
                            }

                            mCurrentPackageCountCapacity = 0;
                            mSPObjectCollection.SPObject.Clear();
                            GenerateCurrentFolderObjects();
                            UpdateTempPathInfo();
                        }
                    }
                }
            }
        }

        private void SplitPackageFiles(bool isLastPackage)
        {
            mLog.Info($"Start SplitPackage. isLastPackage: {isLastPackage}");
            StorageStubBasicInfoList();
            CopyMultiFileToFolder(tempJobPath, currentHSMLocalInfo.MetadataContainerPath, true);
            StorageManifest(currentHSMLocalInfo.MetadataContainerPath);
            StorageLookupListMapXml(currentHSMLocalInfo.MetadataContainerPath);
            StorageUserGroupXMLXml(currentHSMLocalInfo.MetadataContainerPath);

            string metaPackageZipPath = $"{currentHSMLocalInfo.MetadataContainerPath}.zip";
            ZipUtil.ZipFolder(currentHSMLocalInfo.MetadataContainerPath, metaPackageZipPath);

            try
            {
                Directory.Delete(currentHSMLocalInfo.MetadataContainerPath, true);
            }
            catch (Exception ex)
            {
                mLog.Error($"Delete folders of the package failed: {ex}");
            }

            try
            {
                if(MIImportConstant.FileValue > MIImportConstant.THRESHOLD_CACHE_STUB_CACHE_IN_STORAGE)
                {
                    AzureUtil.UploadStorageBlob(
                        RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING],
                        GetTemporarySaveStorageContainerName(),
                        $"{mConfig.TenantGroupId}/{mConfig.JobId}/{currentHSMLocalInfo.MetadataContainerName}.zip",
                        metaPackageZipPath,
                        Azure.Storage.Blobs.Models.AccessTier.Cool);

                    File.Delete(metaPackageZipPath);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Upload stub package files to storage failed: {ex}");
            }
            
            mLog.Info("End SplitPackage.tempManifestPath: {0}.", currentHSMLocalInfo.MetadataContainerName);
        }

        private string GetTemporarySaveStorageContainerName()
        {
            return "stubcache";
        }

        private void DecompressPackage(HSMLocalInfo hsmLocalInfo)
        {
            var metaPackageZipPath = $"{hsmLocalInfo.MetadataContainerPath}.zip";
            try
            {
                if (!File.Exists(metaPackageZipPath))
                {
                    var storageConnStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                    var storageContainerName = GetTemporarySaveStorageContainerName();
                    var blobName = $"{mConfig.TenantGroupId}/{mConfig.JobId}/{hsmLocalInfo.MetadataContainerName}.zip";
                    AzureUtil.DownloadBlobToAsync(
                        storageConnStr,
                        storageContainerName,
                        blobName,
                        metaPackageZipPath).GetAwaiter().GetResult();

                    AzureUtil.DeleteBlob(
                        storageConnStr,
                        storageContainerName,
                        blobName);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Download stub package files to storage failed: {ex}");
            }

            ZipUtil.UnZipFile(metaPackageZipPath, hsmLocalInfo.MetadataContainerPath);

            try
            {
                File.Delete(metaPackageZipPath);
            }
            catch (Exception ex)
            {
                mLog.Error($"Delete temporary package zip file failed: {ex}");
            }

            var stubInfoList = LoadStubInfoList(hsmLocalInfo);
            foreach (var stubBasicInfo in stubInfoList)
            {
                var psc = stubBasicInfo.ToProcessStubFileContentAsync().GetAwaiter().GetResult();
                byte[] mStubBytes = LinkFileCommon.GetFileContent(new CultureInfo(stubBasicInfo.LCID), psc);
                SaveStubStream(mStubBytes, stubBasicInfo.StubNumber, stubBasicInfo.ContainerId);
            }
            try
            {
                File.Delete(GetStubInfoListCacheFilePath(hsmLocalInfo));
            }
            catch (Exception ex)
            {
                mLog.Error($"Delete stub infoes cache file failed: {ex}");
            }
            
        }

        private string GetStubInfoListCacheFilePath(HSMLocalInfo hsmLocalInfo)
        {
            return Path.Combine(hsmLocalInfo.MetadataContainerPath, "StubBasicInfoList.json");
        }
        private void StorageStubBasicInfoList()
        {
            if (mStubBasicInfoList.Count > 0)
            {
                string filePath = GetStubInfoListCacheFilePath(currentHSMLocalInfo);
                try
                {
                    var jsonContent = SerializerHelper.SerializeByJsonConvert(mStubBasicInfoList);
                    File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(jsonContent));
                    mStubBasicInfoList.Clear();
                }
                catch (Exception ex)
                {
                    mLog.Error($"StorageStubBasicInfoList failed: {ex}");
                }
            }
        }
        private List<StubBasicInfo> LoadStubInfoList(HSMLocalInfo hsmLocalInfo)
        {
            string filePath = GetStubInfoListCacheFilePath(hsmLocalInfo);
            var jsonContent = File.ReadAllText(filePath);
            return SerializerHelper.DeserializeByJsonConvert<List<StubBasicInfo>>(jsonContent);
        }

        private async System.Threading.Tasks.Task AssembleItem2HSMPackageAsync(HSMStubInfo mHSMStubInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("HSMStub.AssembleItem2HSMPackage"))
            {
                try
                {
                    if (mHSMStubInfo is HSMListInfo)
                    {
                        ResetList(mHSMStubInfo as HSMListInfo);
                    }
                    else if (mHSMStubInfo is HSMFileInfo)
                    {
                        var fileinfo = mHSMStubInfo as HSMFileInfo;
                        //delete existing stub file with the same name
                        if (!mConfig.IsConvertStubJob && IsEnablePreDeleteDuplicateStub())
                        {
                            try
                            {
                                string stubSuffixWithDot = "." + fileNameSUFFIX;
                                string oldStubUrl;
                                oldStubUrl = fileinfo.FileObject.ServerRelativeUrl + stubSuffixWithDot;
                                var parentWeb = fileinfo.FileObject.ParentFolder?.ParentWeb;
                                if (parentWeb != null)
                                {
                                    IAveFile oldStub = null;
                                    try
                                    {
                                        oldStub = parentWeb.GetFile(oldStubUrl);
                                    }
                                    catch (Exception innerEx)
                                    {
                                        mLog.Warn("[DeleteDuplicateStub] PreCheck get stub file failed. Url:{0}. Error:{1}", oldStubUrl, innerEx.ToString());
                                    }
                                    if (oldStub != null && oldStub.Exists)
                                    {
                                        try
                                        {
                                            mLog.Info("[DeleteDuplicateStub] Found existing stub. Deleting. Url:{0}", oldStubUrl);
                                            oldStub.Delete();
                                            mLog.Info("[DeleteDuplicateStub] Deleted existing stub. Url:{0}", oldStubUrl);
                                        }
                                        catch (Exception ex)
                                        {
                                            mLog.Warn("[DeleteDuplicateStub] Delete existing stub failed. Url:{0}. Error:{1}", oldStubUrl, ex.ToString());
                                            mLog.Info("[DeleteDuplicateStub] Attempt undeclare stub. Url:{0}", oldStubUrl);
                                            mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(oldStub.Item);
                                            mLog.Info("[DeleteDuplicateStub] Undeclare finish. Url:{0}", oldStubUrl);
                                            oldStub.Delete();
                                            mLog.Info("[DeleteDuplicateStub] Deleted existing stub after undeclare. Url:{0}", oldStubUrl);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("[DeleteDuplicateStub] PreCheck failed:{0}", ex.ToString());
                            }
                        }
                        ProcessStubfileContent psc = null;
                        int stubFileNum = 0;
                        if (mConfig.IsConvertStubJob)
                        {
                            psc = await LinkFileCommon.SetStubContentValueAsync(fileinfo.ArchiverFileIndex, fileinfo.FileObject, mConfig, fileinfo.StubId);
                            stubFileNum = ProcessDocument(fileinfo.ArchiverFileIndex, fileinfo.FileObject, fileinfo.MetadataDto, fileinfo.RoleAssignment, psc.StubId);
                        }
                        else
                        {
                            if (mConfig.BackgroundSettings.SkipExtentionName.Exists(f => fileinfo.FileObject.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                            {
                                mLog.Info("Skip create stub for KeepDocument");
                                return;
                            }
                            psc = await LinkFileCommon.SetStubContentValueAsync(fileinfo.FileObject, mConfig, fileinfo.PathMD5, fileinfo.StubId, fileinfo.FileServerRelatedUrl);
                            stubFileNum = ProcessDocument(fileinfo.FileObject, fileinfo.MetadataDto, fileinfo.RoleAssignment, fileinfo.FileServerRelatedUrl, fileinfo.PathMD5, psc.StubId);
                        }

                        var stubBasicInfo = psc.GetStubBasicInfo(
                            mConfig.currentRule.StubTemplateId,
                            fileinfo.FileObject.ParentFolder.ParentList.ParentWeb.LanguageCulture.LCID,
                            stubFileNum,
                            currentHSMLocalInfo.ContainerId);
                        mStubBasicInfoList.Add(stubBasicInfo);

                        SplitPackage(false);
                    }
                    else if (mHSMStubInfo is HSMManifestFileInfo manifestFileInfo)
                    {

                        try
                        {
                            string stubSuffixWithDot = "." + fileNameSUFFIX;
                            string oldStubUrl;
                            oldStubUrl = manifestFileInfo.FileServerRelatedUrl + stubSuffixWithDot;
                            var parentFolder = manifestFileInfo.ParentFolder;
                            if (parentFolder != null)
                            {
                                IAveFile oldStub = null;
                                try
                                {
                                    oldStub = parentFolder.SPFolder.ParentWeb.GetFile(oldStubUrl);
                                }
                                catch (Exception innerEx)
                                {
                                    mLog.Warn("[DeleteDuplicateStub] PreCheck get stub file failed. Url:{0}. Error:{1}", oldStubUrl, innerEx.ToString());
                                }
                                if (oldStub != null && oldStub.Exists)
                                {
                                    mLog.Info("this stub has exist,for hsm we should not remove it");
                                    mConfig.JobReportDto.AddDeletionReport(manifestFileInfo.DocumentAccessUrl,
                                             0,
                                             JobDetailsStatus.Skipped,
                                             (int)CacheNodeType.Item,
                                             mConfig.JobId,
                                             mConfig.currentRule.Name,
                                             "",
                                             "SO_Action_LevelStub",
                                             "RM_PU_SkipItemMessage",
                                             "");
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("[DeleteDuplicateStub] PreCheck failed:{0}", ex.ToString());
                        }
                        await ProcessManifestStubAsync(manifestFileInfo);
                    }
                }
                catch (AveSkipLockSiteException e)
                {
                    mLog.Error("AssembleItem2HSMPackage Skip Locked site exception : {0}", e);
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("AssembleItem2HSMPackage error, {0}", e.ToString());
                }
            }
        }

        private bool IsEnablePreDeleteDuplicateStub()
        {
            try
            {
                var KeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                var enablePreDeleteDuplicateStub = KeyValueDao?.GetValueByKey("EnablePreDeleteDuplicateStub");
                if (enablePreDeleteDuplicateStub != null && bool.TryParse(enablePreDeleteDuplicateStub.Value, out bool enabled))
                {
                    return enabled;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("IsEnablePreDeleteDuplicateStub failed: {0}", ex.ToString());
            }
            return false;
        }

        private async System.Threading.Tasks.Task ProcessManifestStubAsync(HSMManifestFileInfo manifestFileInfo)
        {
            if (manifestFileInfo == null || manifestFileInfo.RowId == 0)
            {
                return;
            }

            EnsureCurrentLocalInfo();

            if (IsSystemList(manifestFileInfo))
            {
                mLog.Info($"Skip system file for HSM Stub {manifestFileInfo.FileServerRelatedUrl}");
                return;
            }

            var stubTemplateId = !string.IsNullOrWhiteSpace(manifestFileInfo.StubTemplateId)
                ? manifestFileInfo.StubTemplateId
                : mConfig?.currentRule?.StubTemplateId;

            var stubSetting = await LinkFileCommon.GetStubTemplatesByIdAsync(stubTemplateId);
            if (stubSetting == null)
            {
                mLog.Warn($"Skip HSM manifest stub because stub template {stubTemplateId} is missing. Url:{manifestFileInfo.FileServerRelatedUrl}");
                return;
            }

            var psc = new ProcessStubfileContent(stubSetting.StubContent, (LeaveStubType)stubSetting.StubType);
            var resolvedStubId = !string.IsNullOrWhiteSpace(manifestFileInfo.StubId)
                ? manifestFileInfo.StubId
                : string.Concat(Guid.NewGuid().ToString("N"), DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
            psc.StubId = resolvedStubId;

            if (psc.FileNameSelected)
            {
                psc.SetValue(StubDynamicValueType.FileName, ResolveManifestFileName(manifestFileInfo));
            }

            if (psc.FullPathToFileSelected)
            {
                psc.SetValue(StubDynamicValueType.Url, ResolveManifestFullPath(manifestFileInfo));
            }

            if (psc.RuleNameSelected)
            {
                psc.SetValue(StubDynamicValueType.RuleName, manifestFileInfo.RuleName ?? mConfig?.currentRule?.Name ?? string.Empty);
            }

            if (psc.DateOfArchivalSelected)
            {
                psc.SetValue(StubDynamicValueType.ArchivalDate, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }

            if (psc.ReCenterLinkSelected && !string.IsNullOrWhiteSpace(manifestFileInfo.PathMD5))
            {
                psc.SetValue(StubDynamicValueType.ReCenterLink, BuildReCenterRestoreLink(manifestFileInfo, resolvedStubId));
            }

            var stubFileNum = Interlocked.Increment(ref MIImportConstant.FileValue);

            var stubBasicInfo = psc.GetStubBasicInfo(
                stubTemplateId,
                CultureInfo.InvariantCulture.LCID,
                stubFileNum,
                currentHSMLocalInfo.ContainerId);

            mStubBasicInfoList.Add(stubBasicInfo);

            var mapping = BuildManifestFileMapping(manifestFileInfo);
            Guid newFileGuid = Guid.NewGuid();
            AddManifestFileObject(manifestFileInfo, mapping, stubFileNum, newFileGuid);
            AddManifestListItemObject(manifestFileInfo, mapping, newFileGuid);
            HSMFileMappings.Add(mapping);

            mCurrentPackageCountCapacity++;
            if (mCurrentPackageCountCapacity >= MIImportConstant.PackageCountCapacity)
            {
                SplitPackage(false);
            }
        }

        private void AddManifestFileObject(HSMManifestFileInfo manifestFileInfo, HSMFileMapping mapping, int stubFileNum,Guid newFileId)
        {
            if (manifestFileInfo == null || mapping == null)
            {
                return;
            }

            var fileId = mapping.ID;
            var rawUrl = manifestFileInfo.FileServerRelatedUrl + "." + fileNameSUFFIX ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawUrl) && !string.IsNullOrWhiteSpace(manifestFileInfo.DocumentAccessUrl))
            {
                rawUrl = manifestFileInfo.DocumentAccessUrl;
            }

            var parentWebServerRelativeUrl = GetServerRelativeUrl(manifestFileInfo.SiteUrl);
            var serverRelativeUrl = NormalizeServerRelativeUrl(rawUrl);
            var webRelativeUrl = GetWebRelativeUrl(parentWebServerRelativeUrl, serverRelativeUrl);
            var fileName = ResolveManifestFileName(manifestFileInfo) + "." + fileNameSUFFIX;
            var resolvedAuthorId = ResolveManifestIntColumn(manifestFileInfo.ColumnValues, "Author") ?? manifestFileInfo.AuthorId;
            var resolvedEditorId = ResolveManifestIntColumn(manifestFileInfo.ColumnValues, "Editor") ?? manifestFileInfo.ModifiedId;
            var resolvedAuthorRaw = ResolveManifestUserColumnString(manifestFileInfo.ColumnValues, "Author") ?? manifestFileInfo.Author;
            var resolvedEditorRaw = ResolveManifestUserColumnString(manifestFileInfo.ColumnValues, "Editor") ?? manifestFileInfo.Editor;
            var createdTime = NormalizeManifestDate(ResolveManifestDateColumn(manifestFileInfo.ColumnValues, "Created") ?? manifestFileInfo.CreatedTime);
            var modifiedTime = NormalizeManifestDate(ResolveManifestDateColumn(manifestFileInfo.ColumnValues, "Modified") ?? manifestFileInfo.ModifiedTime);

            TrackUserGroupMapping(resolvedAuthorId);
            TrackUserGroupMapping(resolvedEditorId);

            var resolvedWebId = manifestFileInfo.WebId != Guid.Empty
                ? manifestFileInfo.WebId
                : (manifestFileInfo.ListId != Guid.Empty ? manifestFileInfo.ListId : Guid.Empty);

            var spFile = new SPFile
            {
                Id = newFileId.ToString(),
                Name = fileName,
                Url = webRelativeUrl,
                ListItemIntId = manifestFileInfo.RowId,
                InDocumentLibrary = true,
                ParentId = manifestFileInfo.ListId != Guid.Empty ? manifestFileInfo.ListId.ToString() : string.Empty,
                ParentWebId = resolvedWebId.ToString(),
                ParentWebUrl = parentWebServerRelativeUrl,
                ListId = manifestFileInfo.ListId != Guid.Empty ? manifestFileInfo.ListId.ToString() : string.Empty,
                FileValue = string.Format(CultureInfo.InvariantCulture, "{0}.dat", stubFileNum),
                Author = resolvedAuthorId > 0 ? resolvedAuthorId.ToString(CultureInfo.InvariantCulture) : resolvedAuthorRaw ?? string.Empty,
                ModifiedBy = resolvedEditorId > 0 ? resolvedEditorId.ToString(CultureInfo.InvariantCulture) : resolvedEditorRaw ?? string.Empty,
                TimeCreated = createdTime,
                TimeLastModified = modifiedTime,
                Version = string.IsNullOrEmpty(manifestFileInfo.VersionString)? "1.0":manifestFileInfo.VersionString,
            };

            var spFileObject = new SPGenericObject
            {
                Id = spFile.Id,
                ObjectType = SPObjectType.SPFile,
                ParentId = spFile.ParentId,
                ParentWebId = spFile.ParentWebId,
                ParentWebUrl = spFile.ParentWebUrl,
                Url = serverRelativeUrl,
                Item = spFile,
            };

            lock (mSPObjectCollection)
            {
                if (mSPObjectCollection == null)
                {
                    mSPObjectCollection = new SPGenericObjectCollection();
                }
                mSPObjectCollection.SPObject.Add(spFileObject);
            }
        }

        private void AddManifestListItemObject(HSMManifestFileInfo manifestFileInfo, HSMFileMapping mapping, Guid newFileId)
        {
            if (manifestFileInfo == null || mapping == null)
            {
                return;
            }

            var fileName = ResolveManifestFileName(manifestFileInfo) + "." + fileNameSUFFIX;
            var parentWebServerRelativeUrl = GetServerRelativeUrl(manifestFileInfo.SiteUrl);
            var serverRelativeUrl = NormalizeServerRelativeUrl((manifestFileInfo.FileServerRelatedUrl ?? string.Empty) + "." + fileNameSUFFIX);
            var webRelativeUrl = GetWebRelativeUrl(parentWebServerRelativeUrl, serverRelativeUrl);
            var parentListId = manifestFileInfo.ListId != Guid.Empty ? manifestFileInfo.ListId.ToString() : string.Empty;
            var dirName = ResolveManifestDirectory(manifestFileInfo);
            var resolvedWebId = manifestFileInfo.WebId != Guid.Empty
                ? manifestFileInfo.WebId
                : (manifestFileInfo.ListId != Guid.Empty ? manifestFileInfo.ListId : Guid.Empty);
            var resolvedAuthorId = ResolveManifestIntColumn(manifestFileInfo.ColumnValues, "Author") ?? manifestFileInfo.AuthorId;
            var resolvedEditorId = ResolveManifestIntColumn(manifestFileInfo.ColumnValues, "Editor") ?? manifestFileInfo.ModifiedId;
            var resolvedAuthorRaw = ResolveManifestUserColumnString(manifestFileInfo.ColumnValues, "Author") ?? manifestFileInfo.Author;
            var resolvedEditorRaw = ResolveManifestUserColumnString(manifestFileInfo.ColumnValues, "Editor") ?? manifestFileInfo.Editor;
            var createdTime = NormalizeManifestDate(ResolveManifestDateColumn(manifestFileInfo.ColumnValues, "Created") ?? manifestFileInfo.CreatedTime);
            var modifiedTime = NormalizeManifestDate(ResolveManifestDateColumn(manifestFileInfo.ColumnValues, "Modified") ?? manifestFileInfo.ModifiedTime);

            TrackUserGroupMapping(resolvedAuthorId);
            TrackUserGroupMapping(resolvedEditorId);

            var listItem = new SPListItem
            {
                ParentWebId = resolvedWebId.ToString(),
                ParentFolderId = Guid.Empty.ToString(),
                Name = fileName,
                DirName = dirName,
                Id = newFileId.ToString(),
                DocId = newFileId.ToString(),
                Version = string.IsNullOrEmpty(manifestFileInfo.VersionString) ? "1.0" : manifestFileInfo.VersionString,
                DocType = ListItemDocType.File,
                IntId = manifestFileInfo.RowId,
                ParentListId = parentListId,
                FileUrl = webRelativeUrl,
                TimeCreated = createdTime,
                TimeLastModified = modifiedTime,
            };

            listItem.Author = resolvedAuthorId > 0 ? resolvedAuthorId.ToString(CultureInfo.InvariantCulture) : resolvedAuthorRaw ?? string.Empty;
            listItem.ModifiedBy = resolvedEditorId > 0 ? resolvedEditorId.ToString(CultureInfo.InvariantCulture) : resolvedEditorRaw ?? string.Empty;
            var fieldCol = BuildManifestFieldCollection(manifestFileInfo.ColumnValues);
            SPField linkfield = new SPField();
            linkfield.Name = LinkFileCommon.LinkFileFieldName;
            linkfield.Value = LinkFileCommon.GenerateLinkFieldValue(mConfig.JobId);
            linkfield.ID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
            fieldCol.Field.Add(linkfield);
            listItem.Items.Add(fieldCol);

            if (manifestFileInfo.RoleAssignments != null && manifestFileInfo.RoleAssignments.Count > 0 && mCurrentAveList != null)
            {
                ProcessRoleAssigementsXML(manifestFileInfo.RoleAssignments, listItem.Id, listItem.FileUrl);
            }

            var listItemObject = new SPGenericObject
            {
                Id = listItem.Id,
                Name = listItem.Name,
                ObjectType = SPObjectType.SPListItem,
                ParentId = parentListId,
                ParentWebId = resolvedWebId.ToString(),
                ParentWebUrl = parentWebServerRelativeUrl,
                Url = serverRelativeUrl,
                Item = listItem,
            };

            lock (mSPObjectCollection)
            {
                if (mSPObjectCollection == null)
                {
                    mSPObjectCollection = new SPGenericObjectCollection();
                }
                mSPObjectCollection.SPObject.Add(listItemObject);
            }
        }

        private void EnsureCurrentLocalInfo()
        {
            if (currentHSMLocalInfo == null)
            {
                UpdateTempPathInfo();
            }
        }

        private string ResolveManifestFileName(HSMManifestFileInfo manifestFileInfo)
        {
            if (!string.IsNullOrWhiteSpace(manifestFileInfo.FileName))
            {
                return manifestFileInfo.FileName;
            }

            if (!string.IsNullOrWhiteSpace(manifestFileInfo.FileServerRelatedUrl))
            {
                return Path.GetFileName(manifestFileInfo.FileServerRelatedUrl);
            }

            return string.Empty;
        }

        private string ResolveManifestFullPath(HSMManifestFileInfo manifestFileInfo)
        {
            if (!string.IsNullOrWhiteSpace(manifestFileInfo.DocumentAccessUrl))
            {
                return manifestFileInfo.DocumentAccessUrl;
            }

            var siteUrl = manifestFileInfo.SiteUrl ?? string.Empty;
            var serverRelativeUrl = manifestFileInfo.FileServerRelatedUrl ?? string.Empty;

            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return serverRelativeUrl;
            }

            return ArchiverCommonStaticMethod.MakeFullUrl(siteUrl, serverRelativeUrl);
        }

        private bool IsSystemList(HSMManifestFileInfo manifestFileInfo)
        {
            if (manifestFileInfo == null)
            {
                return false;
            }

            if (manifestFileInfo.Hidden || manifestFileInfo.IsCatalog)
            {
                return true;
            }

            return manifestFileInfo.BaseTemplate == (int)AveListTemplateType.DesignCatalog
                || manifestFileInfo.BaseTemplate == (int)AveListTemplateType.MasterPageCatalog
                || manifestFileInfo.BaseTemplate == (int)AveListTemplateType.WebPageLibrary
                || manifestFileInfo.BaseTemplate == (int)AveListTemplateType.ThemeCatalog;
        }

        private string BuildReCenterRestoreLink(HSMManifestFileInfo manifestFileInfo, string stubId)
        {
            try
            {
                var reCenterUrl = ArchiverCommonStaticMethod.GetReCenterHost(mConfig.TenantGroupId);
                if (string.IsNullOrWhiteSpace(reCenterUrl))
                {
                    return string.Empty;
                }

                var serverRelativeUrl = manifestFileInfo.FileServerRelatedUrl ?? string.Empty;
                if (!serverRelativeUrl.StartsWith("/", StringComparison.Ordinal))
                {
                    serverRelativeUrl = "/" + serverRelativeUrl.TrimStart('/');
                }

                var stubLinkDetails = new StubLinkDetails(
                    mConfig.aveObjectModelFactory.AccountInfo.TenantId,
                    manifestFileInfo.SiteUrl ?? string.Empty,
                    serverRelativeUrl,
                    manifestFileInfo.PathMD5 ?? string.Empty,
                    mConfig.CurrentIndexJobID,
                    string.Empty,
                    mConfig.currentRule.LeaveStubType)
                {
                    StubId = stubId,
                    StubProductSource = StubProductSource.Opus,
                    FileSize = manifestFileInfo.DocumentSize.ToString(CultureInfo.InvariantCulture)
                };

                return string.Format(CultureInfo.InvariantCulture, "{0}/?Id=({1})&archiver={2}", reCenterUrl.TrimEnd('/'), stubLinkDetails.StubId, new StubLinkProcessor(mConfig).ConvertToString(stubLinkDetails));
            }
            catch (Exception ex)
            {
                mLog.Warn($"Build reCenter link for manifest stub failed. Url:{manifestFileInfo.FileServerRelatedUrl}. Error:{ex}");
                return string.Empty;
            }
        }

        private HSMFileMapping BuildManifestFileMapping(HSMManifestFileInfo manifestFileInfo)
        {
            Guid fileId = Guid.NewGuid();
            if (!string.IsNullOrWhiteSpace(manifestFileInfo.SpId) && Guid.TryParse(manifestFileInfo.SpId, out Guid parsedFileId))
            {
                fileId = parsedFileId;
            }

            return new HSMFileMapping
            {
                ID = fileId,
                FileNewID = fileId,
                RowID = manifestFileInfo.RowId,
                ListID = manifestFileInfo.ListId,
                FileUrl = manifestFileInfo.FileServerRelatedUrl ?? string.Empty,
                Size = manifestFileInfo.DocumentSize,
                TotalSize = manifestFileInfo.TotalSize,
                MD5 = manifestFileInfo.PathMD5,
                RuleID = mConfig.currentRule.Id,
                ContainerId = currentHSMLocalInfo?.ContainerId,
                Status = StubExportStauts.Successful,
                AuthorID = manifestFileInfo.AuthorId,
                ModifiedID = manifestFileInfo.ModifiedId,
                AuthorEmail = manifestFileInfo.Author ?? string.Empty,
                ModifiedEmail = manifestFileInfo.Editor ?? string.Empty,
                CreateTime = manifestFileInfo.CreatedTime?.ToString("yyyyMM", CultureInfo.InvariantCulture) ?? string.Empty,
                ModifiedTime = manifestFileInfo.ModifiedTime?.ToString("yyyyMM", CultureInfo.InvariantCulture) ?? string.Empty,
                VersionCount = 1,
                ModifiedTimeTicks = manifestFileInfo.ModifiedTime?.Ticks ?? 0,
                TimeLastModifiedTicks = manifestFileInfo.ModifiedTime?.Ticks ?? 0,
                IsManifestStub = true,
                StubId = manifestFileInfo.StubId,
            };
        }

        private static DateTime NormalizeManifestDate(DateTime? source)
        {
            var nowUtc = DateTime.UtcNow;
            if (!source.HasValue)
            {
                return nowUtc;
            }

            var resolved = source.Value;
            if (resolved.Year <= 1900 || resolved.Year > 9999)
            {
                return nowUtc;
            }

            if (resolved.Kind == DateTimeKind.Unspecified)
            {
                resolved = DateTime.SpecifyKind(resolved, DateTimeKind.Utc);
            }

            return resolved.Kind == DateTimeKind.Utc ? resolved : resolved.ToUniversalTime();
        }

        private static string ResolveManifestDirectory(HSMManifestFileInfo manifestFileInfo)
        {
            if (!string.IsNullOrWhiteSpace(manifestFileInfo.FolderPath))
            {
                return NormalizeServerRelativeUrl(manifestFileInfo.FolderPath);
            }

            var rawUrl = manifestFileInfo.FileServerRelatedUrl ?? string.Empty;
            var lastSlash = rawUrl.LastIndexOf('/');
            if (lastSlash > 0)
            {
                return NormalizeServerRelativeUrl(rawUrl.Substring(0, lastSlash));
            }

            return string.Empty;
        }

        private static int? ResolveManifestIntColumn(Dictionary<string, object>? columnValues, string key)
        {
            if (columnValues == null)
            {
                return null;
            }

            var candidates = new List<string> { key };
            if (string.Equals(key, "Author", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("Created_x0020_By");
                candidates.Add("AuthorId");
            }
            else if (string.Equals(key, "Editor", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("Modified_x0020_By");
                candidates.Add("EditorId");
            }

            foreach (var candidate in candidates)
            {
                if (!columnValues.TryGetValue(candidate, out var raw) || raw == null)
                {
                    continue;
                }

                if (raw is int direct)
                {
                    return direct;
                }

                if (int.TryParse(raw.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }

        private static DateTime? ResolveManifestDateColumn(Dictionary<string, object>? columnValues, string key)
        {
            if (columnValues == null || !columnValues.TryGetValue(key, out var raw) || raw == null)
            {
                return null;
            }

            if (raw is DateTime dt)
            {
                return dt;
            }

            if (DateTime.TryParse(raw.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static string? ResolveManifestUserColumnString(Dictionary<string, object>? columnValues, string key)
        {
            if (columnValues == null)
            {
                return null;
            }

            var candidates = new List<string> { key };
            if (string.Equals(key, "Author", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("Created_x0020_By");
                candidates.Add("AuthorId");
            }
            else if (string.Equals(key, "Editor", StringComparison.OrdinalIgnoreCase))
            {
                candidates.Add("Modified_x0020_By");
                candidates.Add("EditorId");
            }

            foreach (var candidate in candidates)
            {
                if (!columnValues.TryGetValue(candidate, out var raw) || raw == null)
                {
                    continue;
                }

                var str = Convert.ToString(raw, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }
            }

            return null;
        }

        private void TrackUserGroupMapping(int? userId)
        {
            if (!userId.HasValue || userId.Value <= 0)
            {
                return;
            }

            try
            {
                if (!mUserGroupMappingForCurrentPackage.Contains(userId.Value))
                {
                    mUserGroupMappingForCurrentPackage.Add(userId.Value);
                }
            }
            catch (Exception ex)
            {
                mLog.Info($"TrackUserGroupMapping add userId failed. Id:{userId}. Error:{ex}");
            }
        }

        private SPFieldCollection BuildManifestFieldCollection(Dictionary<string, object>? columnValues)
        {
            var collection = new SPFieldCollection();
            if (collection.Field != null && !collection.Field.Select(a => a.Name).Contains("File_x0020_Type"))
            {
                collection.Field.Add(new SPField
                {
                    ID = AveBuiltInFieldId.File_x0020_Type.ToString(),
                    Name = "File_x0020_Type",
                    Value = fileNameSUFFIX,
                });
            }
            if (columnValues == null)
            {
                return collection;
            }

            foreach (var kv in columnValues)
            {
                if (kv.Key.EndsWith("#2"))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(kv.Key))
                {
                    continue;
                }

                if (IsManifestSpecialField(kv.Key))
                {
                    continue;
                }

                try
                {
                    var field = new SPField
                    {
                        Name = kv.Key,
                        Value = ConvertManifestFieldValue(kv.Key, kv.Value),
                    };
                    if (columnValues.ContainsKey(kv.Key + "#2"))
                    {
                        field.Value2 = columnValues[kv.Key + "#2"].ToString();
                    }
                    collection.Field.Add(field);
                }
                catch (Exception ex)
                {
                    mLog.Warn($"BuildManifestFieldCollection convert field failed. Name:{kv.Key}, Value:{kv.Value}. Error:{ex}");
                }
            }
            
            return collection;
        }

        private static bool IsManifestSpecialField(string key)
        {
            return key.Equals("Author", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Editor", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Created", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Modified", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Created_x0020_By", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Modified_x0020_By", StringComparison.OrdinalIgnoreCase)
                || key.Equals("HasStream", StringComparison.OrdinalIgnoreCase);
        }

        private string ConvertManifestFieldValue(string fieldName, object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            switch (value)
            {
                case DateTime dt:
                    var normalized = dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
                    return normalized.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                case bool b:
                    return b ? "true" : "false";
                case Guid g:
                    return g.ToString();
                case string s:
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                    {
                        return parsed.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    }
                    mLog.Warn($"ConvertManifestFieldValue date parse failed. Field:{fieldName}, Raw:{s}");
                    return s;
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
        }

        private static string GetWebRelativeUrl(string webServerRelativeUrl, string serverRelativeUrl)
        {
            if (string.IsNullOrWhiteSpace(serverRelativeUrl))
            {
                return string.Empty;
            }

            var normalizedServerRelativeUrl = NormalizeServerRelativeUrl(serverRelativeUrl);
            if (string.IsNullOrWhiteSpace(webServerRelativeUrl))
            {
                return normalizedServerRelativeUrl.TrimStart('/');
            }

            var normalizedWebServerRelativeUrl = NormalizeServerRelativeUrl(webServerRelativeUrl);
            if (string.IsNullOrEmpty(normalizedWebServerRelativeUrl) || normalizedWebServerRelativeUrl == "/")
            {
                return normalizedServerRelativeUrl.TrimStart('/');
            }

            if (normalizedServerRelativeUrl.Equals(normalizedWebServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (normalizedServerRelativeUrl.StartsWith(normalizedWebServerRelativeUrl + "/", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedServerRelativeUrl.Substring(normalizedWebServerRelativeUrl.Length + 1);
            }

            return normalizedServerRelativeUrl.TrimStart('/');
        }

        private static string GetServerRelativeUrl(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return string.Empty;
            }

            return NormalizeServerRelativeUrl(siteUrl);
        }

        private static string NormalizeServerRelativeUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var trimmed = path.Trim();

            var isAbsoluteHttpUrl = trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            if (isAbsoluteHttpUrl && Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
            {
                trimmed = Uri.UnescapeDataString(absoluteUri.AbsolutePath);
            }

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            trimmed = trimmed.Replace('\\', '/');
            trimmed = "/" + trimmed.TrimStart('/');

            return trimmed == "/" ? string.Empty : trimmed;
        }


        private void ResetList(HSMListInfo mHSMListInfo)
        {
            mLog.Info($"HSMConnector ResetList {mHSMListInfo.ListObject.ID}");
            lock (padlock)
            {
                if (mCurrentAveList == null || mCurrentAveList.ID != mHSMListInfo.ListObject.ID)
                {
                    SplitPackage(true);
                    mCurrentAveList = mHSMListInfo.ListObject;
                    try
                    {
                        using (var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig))
                        {
                            LinkFileCommon.AddLinkField2List(mCurrentAveList);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is AveSkipLockSiteException)
                        {
                            throw;
                        }

                        mLog.Error($"HSMConnector ResetList TryUnlockSiteCollection failed,error:{e}");
                        LinkFileCommon.AddLinkField2List(mCurrentAveList);
                    }
                    ImportJobResources importJobResources = new ImportJobResources();
                    //if (!mAllJobStatus.ContainsKey(mCurrentAveList.ID))
                    //{
                    //    mAllJobStatus.Add(mCurrentAveList.ID, importJobResources);
                    //}

                    UpdateTempPathInfo();

                    using (new AvePerformanceScope("ProcessRoleDefinitionsXML"))
                    {
                        ProcessRoleDefinitionsXML(mCurrentAveList.ParentWeb);
                    }
                    using (new AvePerformanceScope("ProcessListRootFolderXML"))
                    {
                        ProcessListRootFolderXML();
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
                    //using (new AvePerformanceScope("ProcessUserGroupXML"))
                    //{
                    //    ProcessUserGroupXML();
                    //}
                    // need some set null logic.
                }
            }
        }

        private int ProcessDocument(IAveFile aveFile, AveSPDocumentMetadataDto MetadataDto, List<AveRoleAssignmentInfo> RoleAssignment, string serverRelatedUrl, string md5 = null, string stubId = null)
        {
            string currentContainerId = string.Empty;
            string newLeafName = GetStubFileName(aveFile);
            int filenum = Interlocked.Increment(ref MIImportConstant.FileValue);
            SPFile file = null;
            Guid itemid = Guid.NewGuid();

            HSMFileMapping mHSMFileMapping = new HSMFileMapping();
            List<string> nullTargets = new List<string>();
            try
            {
                if (aveFile == null) nullTargets.Add("aveFile");
                if (aveFile?.Item?.FieldValues?["Modified"] is DateTime dtMod)
                {
                    mHSMFileMapping.ModifiedTime = dtMod.Year.ToString() + dtMod.Month.ToString("00");
                    mHSMFileMapping.ModifiedTimeTicks = dtMod.Ticks;
                }
                else
                {
                    if (aveFile?.Item == null) nullTargets.Add("Item");
                    else if (aveFile.Item.FieldValues == null) nullTargets.Add("FieldValues");
                    else nullTargets.Add("FieldValues[Modified]");
                }
                mHSMFileMapping.TimeLastModifiedTicks = aveFile?.TimeLastModified.Ticks ?? 0;
                mHSMFileMapping.ID = aveFile?.UniqueId ?? Guid.Empty;
                mHSMFileMapping.ContainerId = currentHSMLocalInfo?.ContainerId;
                if (currentHSMLocalInfo == null) nullTargets.Add("currentHSMLocalInfo");
                mHSMFileMapping.RowID = aveFile?.Item?.ID ?? -1;
                mHSMFileMapping.ListID = aveFile?.ParentFolder?.ParentList?.ID ?? Guid.Empty;
                if (aveFile?.ParentFolder == null) nullTargets.Add("ParentFolder");
                else if (aveFile.ParentFolder.ParentList == null) nullTargets.Add("ParentList");
                mHSMFileMapping.FileNewID = itemid;
                mHSMFileMapping.Size = aveFile?.Length ?? 0;
                mHSMFileMapping.TotalSize = aveFile != null ? GetFileTotalSize(aveFile) : 0;
                mHSMFileMapping.MD5 = md5;
                mHSMFileMapping.FileUrl = string.IsNullOrEmpty(serverRelatedUrl) ? aveFile?.ServerRelativeUrl : serverRelatedUrl;
                mHSMFileMapping.RuleID = mConfig.currentRule.Id;
                mHSMFileMapping.Status = StubExportStauts.Successful;
                mHSMFileMapping.AuthorID = aveFile?.Author?.ID ?? 0;
                if (aveFile?.Author == null) nullTargets.Add("Author");
                mHSMFileMapping.AuthorEmail = aveFile?.Author?.Email ?? string.Empty;
                mHSMFileMapping.ModifiedID = aveFile?.ModifiedBy?.ID ?? 0;
                if (aveFile?.ModifiedBy == null) nullTargets.Add("ModifiedBy");
                mHSMFileMapping.ModifiedEmail = aveFile?.ModifiedBy?.Email ?? string.Empty;
                if (aveFile?.TimeCreated == null) nullTargets.Add("TimeCreated");
                mHSMFileMapping.CreateTime = aveFile?.TimeCreated.Year.ToString() + aveFile?.TimeCreated.Month.ToString("00");
                mHSMFileMapping.StubId = stubId;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(mHSMFileMapping.AuthorEmail)) mHSMFileMapping.AuthorEmail = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.ModifiedEmail)) mHSMFileMapping.ModifiedEmail = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.CreateTime)) mHSMFileMapping.CreateTime = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.ModifiedTime)) mHSMFileMapping.ModifiedTime = string.Empty;
                mLog.Warn($"Some thing went wrong when generate HSMFileMapping,error:{ex.ToString()}");
            }
            if (nullTargets.Count > 0)
            {
                mLog.Warn($"ProcessDocument.Mapping null/issue targets:[{string.Join(',', nullTargets.Distinct())}] File:{aveFile?.ServerRelativeUrl}");
            }
            try
            {
                var userData = UpdateMetadata(MetadataDto.UserDataInfo);
                using (new AvePerformanceScope("GenerateFileNode"))
                {
                    string sourcekey = aveFile.UniqueId.ToString();
                    file = GenrateFileNode(aveFile, userData, MetadataDto.DocDataJunction, itemid.ToString(), filenum);
                }
                //if ((file.ParentWebUrl.TrimStart('/') + "/" + file.Url).Length > 400 || newLeafName.Length > 256)
                //{
                //    mLog.Info("The specified file or folder name is too long.");
                //    throw new Exception("The specified file or folder name is too long.");
                //}

                SPListItem item = null;
                using (new AvePerformanceScope("GenerateItemNode"))
                {
                    item = GenrateItemNode(aveFile.Item, aveFile, userData, MetadataDto.DocDataJunction, file.Id);
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
                    mLog.Error($"ProcessDocument.file:{aveFile.ServerRelativeUrl}.TimeLastModified.Year <= 1900 and reset file.TimeLastModified:{file.TimeLastModified}.item.TimeLastModified:{item.TimeLastModified}.aveFile.TimeLastModified:{aveFile.TimeLastModified}.");
                    //file.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                    //item.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                    file.TimeLastModified = DateTime.UtcNow;
                    item.TimeLastModified = DateTime.UtcNow;
                    mLog.Error($"ProcessDocument.After reset file:{aveFile.ServerRelativeUrl}.TimeLastModified:{file.TimeLastModified}.item.TimeLastModified:{item.TimeLastModified}.");
                }
                if (item.TimeCreated != file.TimeCreated)
                {
                    file.TimeCreated = item.TimeCreated;
                }
                if (file.TimeCreated.Year <= 1900)
                {
                    mLog.Error($"ProcessDocument.file:{aveFile.ServerRelativeUrl}.TimeCreated.Year <= 1900 and reset file.TimeCreated:{file.TimeCreated}.item.TimeCreated:{item.TimeCreated}.aveFile.TimeCreated:{aveFile.TimeCreated}.");
                    //file.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                    //item.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                    file.TimeCreated = DateTime.UtcNow;
                    item.TimeCreated = DateTime.UtcNow;
                    mLog.Error($"ProcessDocument.After reset file:{aveFile.ServerRelativeUrl}.TimeCreated:{file.TimeCreated}.item.TimeLastModified:{item.TimeCreated}.");
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
                    ProcessFileObjectNode(aveFile, userData, MetadataDto.DocDataJunction, file);
                }
                using (new AvePerformanceScope("ProcessListItemNode"))
                {
                    ProcessListItemNode(aveFile, userData, MetadataDto.DocDataJunction, item, RoleAssignment, out currentContainerId);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred when exporting stub data to xml. error is {e.ToString()}");
                mHSMFileMapping.Status = StubExportStauts.Failed;
                throw;
            }

            HSMFileMappings.Add(mHSMFileMapping);

            lock (padlock)
            {
                if (HSMFileMappings.Count > 50)
                {
                    mLog.Info($"Will insert to db {mDB4HSM.dbFilePath}");
                    mDB4HSM.InsertValueToDB(HSMFileMappings.GetRange(0, 50));

                    HSMFileMappings.RemoveRange(0, 50);
                }
                mCurrentPackageCountCapacity++;
            }
            return filenum;
        }

        private int ProcessDocument(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, IAveFile aveFile, AveSPDocumentMetadataDto MetadataDto, List<AveRoleAssignmentInfo> RoleAssignment, string stubId = null)
        {
            string currentContainerId = string.Empty;
            string newLeafName = archiverFileIndex.Name + "." + fileNameSUFFIX;
            int filenum = Interlocked.Increment(ref MIImportConstant.FileValue);
            
            SPFile file = null;
            Guid itemid = Guid.NewGuid();
            HSMFileMapping mHSMFileMapping = new HSMFileMapping();
            List<string> nullTargets2 = new List<string>();
            try
            {
                if (aveFile == null) nullTargets2.Add("aveFile");
                if (aveFile?.Item?.FieldValues?["Modified"] is DateTime dtMod)
                {
                    mHSMFileMapping.ModifiedTime = dtMod.Year.ToString() + dtMod.Month.ToString("00");
                    mHSMFileMapping.ModifiedTimeTicks = dtMod.Ticks;
                }
                else
                {
                    if (aveFile?.Item == null) nullTargets2.Add("Item");
                    else if (aveFile.Item.FieldValues == null) nullTargets2.Add("FieldValues");
                    else nullTargets2.Add("FieldValues[Modified]");
                }
                mHSMFileMapping.TimeLastModifiedTicks = aveFile?.TimeLastModified.Ticks ?? 0;
                mHSMFileMapping.ID = aveFile?.UniqueId ?? Guid.Empty;
                mHSMFileMapping.RowID = aveFile?.Item?.ID ?? -1;
                mHSMFileMapping.ListID = aveFile?.ParentFolder?.ParentList?.ID ?? Guid.Empty;
                if (aveFile?.ParentFolder == null) nullTargets2.Add("ParentFolder");
                else if (aveFile.ParentFolder.ParentList == null) nullTargets2.Add("ParentList");
                mHSMFileMapping.ContainerId = currentHSMLocalInfo?.ContainerId;
                if (currentHSMLocalInfo == null) nullTargets2.Add("currentHSMLocalInfo");
                mHSMFileMapping.FileNewID = itemid;
                mHSMFileMapping.Size = archiverFileIndex.ContentLength;
                mHSMFileMapping.TotalSize = archiverFileIndex.DataFileLength;
                mHSMFileMapping.MD5 = archiverFileIndex.PathMD5;
                mHSMFileMapping.FileUrl = aveFile != null ? $"{aveFile.ParentFolder?.ServerRelativeUrl}/{archiverFileIndex.Name}" : string.Empty;
                mHSMFileMapping.RuleID = mConfig.currentRule.Id;
                mHSMFileMapping.Status = StubExportStauts.Successful;
                mHSMFileMapping.AuthorID = aveFile?.Author?.ID ?? 0;
                if (aveFile?.Author == null) nullTargets2.Add("Author");
                mHSMFileMapping.AuthorEmail = aveFile?.Author?.Name ?? string.Empty;
                mHSMFileMapping.ModifiedID = aveFile?.ModifiedBy?.ID ?? 0;
                if (aveFile?.ModifiedBy == null) nullTargets2.Add("ModifiedBy");
                mHSMFileMapping.ModifiedEmail = aveFile?.ModifiedBy?.Name ?? string.Empty;
                if (aveFile?.TimeCreated == null) nullTargets2.Add("TimeCreated"); 
                mHSMFileMapping.CreateTime = aveFile?.TimeCreated.Year.ToString() + aveFile?.TimeCreated.Month.ToString("00");
                mHSMFileMapping.StubId = stubId;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(mHSMFileMapping.AuthorEmail)) mHSMFileMapping.AuthorEmail = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.ModifiedEmail)) mHSMFileMapping.ModifiedEmail = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.CreateTime)) mHSMFileMapping.CreateTime = string.Empty;
                if (string.IsNullOrEmpty(mHSMFileMapping.ModifiedTime)) mHSMFileMapping.ModifiedTime = string.Empty;
                mLog.Warn($"V2: Some thing went wrong when generate HSMFileMapping,error:{ex.ToString()}");
            }
            if (nullTargets2.Count > 0)
            {
                mLog.Warn($"ProcessDocument.Mapping null/issue targets2:[{string.Join(',', nullTargets2.Distinct())}] File:{aveFile?.ServerRelativeUrl}");
            }
            try
            {
                var userData = UpdateMetadata(MetadataDto.UserDataInfo);
                using (new AvePerformanceScope("GenerateFileNode"))
                {
                    //string sourcekey = aveFile.UniqueId.ToString();
                    file = GenrateFileNode(archiverFileIndex, aveFile, userData, MetadataDto.DocDataJunction, itemid.ToString(), filenum);
                }

                SPListItem item = null;
                using (new AvePerformanceScope("GenerateItemNode"))
                {
                    item = GenrateItemNode(archiverFileIndex, aveFile.Item, aveFile, userData, MetadataDto.DocDataJunction, file.Id);
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
                    mLog.Error($"ProcessDocument.file:{aveFile.ServerRelativeUrl}.TimeLastModified.Year <= 1900 and reset file.TimeLastModified:{file.TimeLastModified}.item.TimeLastModified:{item.TimeLastModified}.aveFile.TimeLastModified:{aveFile.TimeLastModified}.");
                    //file.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                    //item.TimeLastModified = aveFile.TimeLastModified.ToUniversalTime();
                    file.TimeLastModified = DateTime.UtcNow;
                    item.TimeLastModified = DateTime.UtcNow;
                    mLog.Error($"ProcessDocument.After reset file:{aveFile.ServerRelativeUrl}.TimeLastModified:{file.TimeLastModified}.item.TimeLastModified:{item.TimeLastModified}.");
                }
                if (item.TimeCreated != file.TimeCreated)
                {
                    file.TimeCreated = item.TimeCreated;
                }
                if (file.TimeCreated.Year <= 1900)
                {
                    mLog.Error($"ProcessDocument.file:{aveFile.ServerRelativeUrl}.TimeCreated.Year <= 1900 and reset file.TimeCreated:{file.TimeCreated}.item.TimeCreated:{item.TimeCreated}.aveFile.TimeCreated:{aveFile.TimeCreated}.");
                    //file.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                    //item.TimeCreated = aveFile.TimeCreated.ToUniversalTime();
                    file.TimeCreated = DateTime.UtcNow;
                    item.TimeCreated = DateTime.UtcNow;
                    mLog.Error($"ProcessDocument.After reset file:{aveFile.ServerRelativeUrl}.TimeCreated:{file.TimeCreated}.item.TimeLastModified:{item.TimeCreated}.");
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
                    ProcessFileObjectNode(archiverFileIndex, aveFile, userData, MetadataDto.DocDataJunction, file);
                }
                using (new AvePerformanceScope("ProcessListItemNode"))
                {
                    ProcessListItemNode(archiverFileIndex, aveFile, item, RoleAssignment);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred when exporting stub data to xml. error is {e.ToString()}");
                mHSMFileMapping.Status = StubExportStauts.Failed;
                throw;
            }

            HSMFileMappings.Add(mHSMFileMapping);

            lock (padlock)
            {
                if (HSMFileMappings.Count > 50)
                {
                    mLog.Info($"Will insert to db {mDB4HSM.dbFilePath}");
                    mDB4HSM.InsertValueToDB(HSMFileMappings.GetRange(0, 50));

                    HSMFileMappings.RemoveRange(0, 50);
                }
                mCurrentPackageCountCapacity++;
            }
            return filenum;
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

        private string GetStubFileName(IAveFile file)
        {
            string stubFileName = string.Empty;

            stubFileName = file.Name + "." + fileNameSUFFIX;

            return stubFileName;
        }

        private bool SaveStubStream(byte[] stubStreamBytes, int filenum, string containerId)
        {
            using (new AvePerformanceScope("Performance.SaveStream"))
            {
                var localInfo = HSMLocalInfos[containerId];
                string azureFileValue = string.Format("{0}.dat", filenum);
                string tempFilePath = Path.Combine(localInfo.DataContainerPath, azureFileValue);
                mLog.Info("tempFilePath:{0}", tempFilePath);
                try
                {
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

        private SPListItem GenrateItemNode(IAveListItem aveItem, IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, string itemid)
        {
            int version = (int)userData["#tp_UIVersion"];
            int docRowId = (int)userData["#tp_ID"];
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

        private SPListItem GenrateItemNode(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, IAveListItem aveItem, IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, string itemid)
        {
            int version = (int)userData["#tp_UIVersion"];
            int docRowId = (int)userData["#tp_ID"];
            SPListItem item = new SPListItem();

            item.ParentWebId = aveFile.ParentFolder.ParentWeb.ID.ToString();
            item.ParentFolderId = aveFile.ParentFolder.UniqueId.ToString();
            item.Name = archiverFileIndex.Name + "." + fileNameSUFFIX;
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
                item.FileUrl = $"{aveFile.ParentFolder.ServerRelativeUrl.Substring(1)}/{archiverFileIndex.Name}.{fileNameSUFFIX}";
            }
            else
            {
                item.FileUrl = $"{aveFile.ParentFolder.ServerRelativeUrl.Substring(aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Length + 1)}/{archiverFileIndex.Name}.{fileNameSUFFIX}";
            }

            using (new AvePerformanceScope("ProcessFieldCollection"))
            {
                SPFieldCollection fieldCollection = ProcessFieldCollection(archiverFileIndex, item, aveItem, docRowId, version, userData, dataJunction);
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
                var webUrl = aveFile.ParentFolder.ParentWeb.ServerRelativeUrl;
                if (aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Trim('/').Length == 0)
                {
                    file.Url = TruncateUrl(aveFile.ServerRelativeUrl.Substring(1) + "." + fileNameSUFFIX, webUrl);
                }
                else
                {
                    file.Url = TruncateUrl(aveFile.ServerRelativeUrl.Substring(webUrl.Length + 1) + "." + fileNameSUFFIX, webUrl);
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
            if (userData.ContainsKey("Author"))
            {
                file.Author = userData["Author"].ToString();
            }
            else
            {
                mLog.Info($"GenrateFileNode userData does not contains Author and use File.Author.ID instead.");
                file.Author = aveFile.Author.ID.ToString();
            }

            if (userData.ContainsKey("Editor"))
            {
                file.ModifiedBy = userData["Editor"].ToString();
            }
            else
            {
                mLog.Info($"GenrateFileNode userData does not contains Editor and use File.ModifiedBy.ID instead.");
                file.ModifiedBy = aveFile.ModifiedBy.ID.ToString();
            }
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

        private string TruncateUrl(string relativePath, string serverRelativeWebUrl)
        {
            const int maxLength = 400;
            if (string.IsNullOrWhiteSpace(relativePath))
                return relativePath;
            relativePath = relativePath.Replace("\\", "/");
            int webPrefixLength = serverRelativeWebUrl.Trim('/').Length == 0 ? 0 : serverRelativeWebUrl.TrimStart('/').Length + 1;
            if (relativePath.Length + webPrefixLength <= maxLength)
                return relativePath;
            int lastSlash = relativePath.LastIndexOf('/');
            var folder = lastSlash >= 0 ? relativePath[..lastSlash] : "";
            var fileName = relativePath[(lastSlash + 1)..];
            int firstDot = fileName.IndexOf('.');
            string baseName;
            string fullExtension;
            if (firstDot >= 0)
            {
                baseName = fileName[..firstDot];
                fullExtension = fileName[firstDot..];
            }
            else
            {
                baseName = fileName;
                fullExtension = "";
            }
            int allowedBaseLength = maxLength - webPrefixLength - folder.Length - (folder.Length > 0 ? 1 : 0) - fullExtension.Length;
            if (allowedBaseLength <= 0)
                return relativePath;
            if (baseName.Length > allowedBaseLength)
            {
                int remaining = allowedBaseLength - 3;
                if (remaining <= 0)
                {
                    baseName = baseName[..allowedBaseLength];
                }
                else
                {
                    int head = remaining / 2;
                    int tail = remaining - head;
                    baseName = $"{baseName[..head]}...{baseName[^tail..]}";
                }
            }
            return string.IsNullOrEmpty(folder)
                ? $"{baseName}{fullExtension}"
                : $"{folder}/{baseName}{fullExtension}";
        }

        private SPFile GenrateFileNode(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, string ItemID, int fileNum)
        {
            int docRowId = (int)userData["#tp_ID"];
            SPFile file = new SPFile();
            if (aveFile != null)
            {
                file.Name = archiverFileIndex.Name + "." + fileNameSUFFIX;
                file.ParentId = aveFile.ParentFolder.UniqueId.ToString();
                if (aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Trim('/').Length == 0)
                {
                    file.Url = $"{aveFile.ParentFolder.ServerRelativeUrl.Substring(1)}/{archiverFileIndex.Name}.{fileNameSUFFIX}";
                }
                else
                {
                    file.Url = $"{aveFile.ParentFolder.ServerRelativeUrl.Substring(aveFile.ParentFolder.ParentWeb.ServerRelativeUrl.Length + 1)}/{archiverFileIndex.Name}.{fileNameSUFFIX}";
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
            if (userData.ContainsKey("Author"))
            {
                file.Author = userData["Author"].ToString();
            }
            else
            {
                mLog.Info($"GenrateFileNode userData does not contains Author and use File.Author.ID instead.");
                file.Author = aveFile.Author.ID.ToString();
            }

            if (userData.ContainsKey("Editor"))
            {
                file.ModifiedBy = userData["Editor"].ToString();
            }
            else
            {
                mLog.Info($"GenrateFileNode userData does not contains Editor and use File.ModifiedBy.ID instead.");
                file.ModifiedBy = aveFile.ModifiedBy.ID.ToString();
            }
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

        private void UpdateTempPathInfo()
        {
            try
            {
                currentHSMLocalInfo = HSMLocalInfo.CreateNew(mConfig.TenantGroupId, mConfig.JobId);

                CreateDirectory(currentHSMLocalInfo.MetadataContainerPath);
                CreateDirectory(currentHSMLocalInfo.DataContainerPath);
                HSMLocalInfos[currentHSMLocalInfo.ContainerId] = currentHSMLocalInfo;
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

        protected void ProcessUserGroupXML()
        {
            try
            {
                mUserGroupMap.Users.Clear();
                mUserGroupMap.Groups.Clear();
                if (!hasProcessedUserXML)
                {
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
                    hasProcessedUserXML = true;
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while processing usergroup xml. Exception: {0}.", e.ToString());
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
                List<string> roleNameList = new List<string>();
                DeploymentRoles roles = new DeploymentRoles();
                roles.Role = new List<DeploymentRole>();
                foreach (var role in web.RoleDefinitions)
                {
                    if (roleNameList.Contains(role.Name))
                    {
                        mLog.Info($"this role name has add to role name list ,will continue.role name:{role.Name}");
                        continue;
                    }
                    else
                    {
                        roleNameList.Add(role.Name);
                        mLog.Info($"this role name not add to role name list ,will add it.role name:{role.Name}");
                    }
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
                        //roleInfo.PermMask = "206292717568";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "160";
                        roleInfo.Type = "1";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0046u";
                        roleInfo.Hidden = true;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741826"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Fu";
                        roleInfo.RoleId = role.ID.ToString();
                        //roleInfo.PermMask = "756052856929";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "128";
                        roleInfo.Type = "2";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0045u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741827"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Du";
                        roleInfo.RoleId = role.ID.ToString();
                        //roleInfo.PermMask = "1856436900591";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "64";
                        roleInfo.Type = "3";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0043u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741828"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Cu";
                        roleInfo.RoleId = role.ID.ToString();
                        //roleInfo.PermMask = "1856438737919";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "32";
                        roleInfo.Type = "4";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0042u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741829"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Bu";
                        roleInfo.RoleId = role.ID.ToString();
                        //roleInfo.PermMask = "9223372036854775807";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "1";
                        roleInfo.Type = "5";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0041u";
                        roleInfo.Hidden = false;
                        roles.Role.Add(roleInfo);
                    }
                    else if (role.ID.ToString().Equals("1073741830"))
                    {
                        DeploymentRole roleInfo = new DeploymentRole();
                        roleInfo.Title = "$Resources:fpext,0x001C003Eu";
                        roleInfo.RoleId = role.ID.ToString();
                        //roleInfo.PermMask = "1856436902639";
                        roleInfo.PermMask = ((long)role.BasePermissions).ToString();
                        roleInfo.RoleOrder = "48";
                        roleInfo.Type = "6";
                        roleInfo.Description = role.Description;//"$Resources:fpext,0x001C0044u";
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

        private void ProcessListRootFolderXML()
        {
            mParentSPFolderObject = new SPGenericObject()
            {
                Id = mCurrentAveList.ID.ToString(),
                ObjectType = SPObjectType.SPFolder,
                ParentId = mCurrentAveList.RootFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = mCurrentAveList.ParentWeb.ID.ToString(),
                ParentWebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl,
                Url = mCurrentAveList.RootFolder.ServerRelativeUrl,
            };
            var spFolder = new SPFolder()
            {
                Id = mCurrentAveList.RootFolder.UniqueId.ToString(),
                Url = mCurrentAveList.RootFolder.Url,
                Name = mCurrentAveList.RootFolder.Name,
                ParentFolderId = mCurrentAveList.RootFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = mCurrentAveList.ParentWeb.ID.ToString(),
                ParentWebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl,
                ContainingDocumentLibrary = mCurrentAveList.ID.ToString(),
                TimeCreated = mCurrentAveList.Created,
                TimeLastModified = mCurrentAveList.LastItemModifiedDate,
            };
            mLog.Info($"ProcessListRootFolderXML.FolderId:{spFolder.ParentFolderId}.FolderURL:{spFolder.Url}.FolderName:{spFolder.Name}.FolderTimeCreated:{spFolder.TimeCreated}.FolderTimeLastModified:{spFolder.TimeLastModified}.");
            if (spFolder.TimeCreated.Year <= 1900)
            {
                spFolder.TimeCreated = DateTime.UtcNow;
                mLog.Info($"ProcessListRootFolderXML.Reset Folder.TimeCreated:{spFolder.TimeCreated}.");
            }
            if (spFolder.TimeLastModified.Year <= 1900)
            {
                spFolder.TimeLastModified = DateTime.UtcNow;
                mLog.Info($"ProcessListRootFolderXML.Reset Folder.TimeLastModified:{spFolder.TimeLastModified}.");
            }
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
                    SiteUrl = mCurrentAveList.ParentWeb.Site.Url,
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
                    Id = mCurrentAveList.ID.ToString(),
                    Type = SPDeploymentObjectType.List,
                    ParentId = mCurrentAveList.ParentWeb.ID.ToString(),
                    Url = mCurrentAveList.RootFolder.ServerRelativeUrl,
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
                    Data = mCurrentAveList.DefaultViewUrl,
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
                rootObject.Url = mCurrentAveList.RootFolder.ServerRelativeUrl;
                rootObject.WebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl;
                rootObject.ParentId = mCurrentAveList.ParentWeb.ID.ToString();
                rootObject.Type = SPDeploymentObjectType.List;
                rootObject.Id = mCurrentAveList.ID.ToString();
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

        private SPSchemaVersion GetSchemaVersion()
        {
            if(_ConfiguredSPSchemaVersion == null)
            {
                try
                {
                    var KeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var jsonContent = KeyValueDao.GetValueByKey("ConfiguredSPSchemaVersion")?.Value;
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        _ConfiguredSPSchemaVersion = JsonConvert.DeserializeObject<SPSchemaVersion>(jsonContent);
                        mLog.Info($"use configured SPSchemaVersion: {_ConfiguredSPSchemaVersion.Version}_{_ConfiguredSPSchemaVersion.SiteVersion}_{_ConfiguredSPSchemaVersion.Build}_{_ConfiguredSPSchemaVersion.DatabaseVersion}");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error($"An error occurred while retrieving the configured SPSchemaVersion. Exception: {ex}.");
                }

                if (_ConfiguredSPSchemaVersion == null)
                {
                    _ConfiguredSPSchemaVersion = new SPSchemaVersion()
                    {
                        Version = "15.0.0.0",
                        SiteVersion = "15",
                        DatabaseVersion = "4406368",
                        Build = "15.0.4420.1017",
                    };
                    mLog.Info($"use default SPSchemaVersion: {_ConfiguredSPSchemaVersion.Version}_{_ConfiguredSPSchemaVersion.SiteVersion}_{_ConfiguredSPSchemaVersion.Build}_{_ConfiguredSPSchemaVersion.DatabaseVersion}");
                }
            }

            return new SPSchemaVersion()
            {
                Version = _ConfiguredSPSchemaVersion.Version,
                SiteVersion = _ConfiguredSPSchemaVersion.SiteVersion,
                DatabaseVersion = _ConfiguredSPSchemaVersion.DatabaseVersion,
                Build = _ConfiguredSPSchemaVersion.Build,
            };
        }

        protected void ProcessSystemDataXML()
        {
            try
            {
                SPSystemData systemData = new SPSystemData();
                SPSchemaVersion schemaVersion = GetSchemaVersion();
                schemaVersion.ObjectsProcessed = mSPObjectCollection.SPObject.Count;

                systemData.SchemaVersion = schemaVersion;

                SPManifestFile manifestFile = new SPManifestFile();
                manifestFile.Name = "Manifest.xml";

                systemData.ManifestFiles.Add(manifestFile);

                //SPSystemObject systemObject1 = new SPSystemObject();
                //systemObject1.Id = mCurrentAveList.ParentWeb.AveWeb.RootFolder.UniqueId.ToString();
                //systemObject1.Url = mCurrentAveList.ParentWeb.AveWeb.RootFolder.ServerRelativeUrl;
                //systemObject1.Type = SPDeploymentObjectType.Folder;

                SPSystemObject systemObject2 = new SPSystemObject();
                systemObject2.Id = mCurrentAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.ID.ToString();
                systemObject2.Url = mCurrentAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.RootFolder.ServerRelativeUrl;
                systemObject2.Type = SPDeploymentObjectType.List;

                SPSystemObject systemObject3 = new SPSystemObject();
                systemObject3.Id = mCurrentAveList.ParentWeb.Site.RootWeb.ID.ToString();
                systemObject3.Url = mCurrentAveList.ParentWeb.Site.RootWeb.ServerRelativeUrl;
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
                            if (mSPObjectCollection.SPObject[i].ObjectType == SPObjectType.SPList)
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
                systemObject2.Id = mCurrentAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.ID.ToString();
                systemObject2.Url = mCurrentAveList.ParentWeb.Site.RootWeb.SiteUserInfoList.RootFolder.ServerRelativeUrl;
                systemObject2.Type = SPDeploymentObjectType.List;

                SPSystemObject systemObject3 = new SPSystemObject();
                systemObject3.Id = mCurrentAveList.ParentWeb.Site.RootWeb.ID.ToString();
                systemObject3.Url = mCurrentAveList.ParentWeb.Site.RootWeb.ServerRelativeUrl;
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
                    var mappingIdsLogBuilder = new StringBuilder();
                    if (mUserGroupMappingForCurrentPackage != null && mUserGroupMappingForCurrentPackage.Count > 0)
                    {
                        foreach (var mappingId in mUserGroupMappingForCurrentPackage)
                        {
                            if (mappingIdsLogBuilder.Length > 0)
                            {
                                mappingIdsLogBuilder.Append(",");
                            }

                            mappingIdsLogBuilder.Append(mappingId);
                        }
                    }

                    mLog.Info($"StorageUserGroupXMLXml.Groups Count:{mUserGroupMap.Groups.Count}." +
                    $"Users Count:{mUserGroupMap.Users.Count}." +
                    $"mUserGroupMappingForCurrentPackage Count:{mUserGroupMappingForCurrentPackage?.Count ?? 0}." +
                    $"mUserGroupMappingForCurrentPackage Ids:[{mappingIdsLogBuilder}]." +
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

                    mLog.Info($"Copy {file.FullName} to {destFilePath}");
                    System.IO.File.Copy(file.FullName, destFilePath, overWrite);
                }

                return true;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while copying multi-files to folder. Exception: {0}.", e.ToString());
                return false;
            }
        }

        private void GenerateCurrentFolderObjects()
        {
            mCacheSPFolderObjects.ForEach(folder => mSPObjectCollection.SPObject.Add(folder));
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


        #endregion

        #region sharepoint

        protected SPFieldCollection ProcessFieldCollection(SPListItem listItem, IAveListItem item, int docRowId, int version, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction)
        {
            SPFieldCollection fieldCollection = new SPFieldCollection();
            try
            {
                string taxonomyListId = Guid.Empty.ToString();
                //Wait Wrapper Team provide method
                ItemMetadataForHSMConnector itemData = new ItemMetadataForHSMConnector(mConfig.aveObjectModelFactory, item, version, docRowId, userData, dataJunction);
                Dictionary<string, AveFieldValueInfo> fieldValues = itemData.ProcessItemMetadata();
                if (fieldValues == null)
                {
                    mLog.Warn($"ProcessFieldCollection.The fieldValues is null.");
                    return fieldCollection;
                }
                List<string> NeedSetNullFields = SetNeedSetNullFieldsEx(fieldValues?.Keys.ToList());
                var termIdCache = new List<string>();
                foreach (var fieldValue in fieldValues)
                {
                    string columnName = string.Empty;
                    try
                    {
                        columnName = fieldValue.Key;
                        AveFieldValueInfo valueInfo = fieldValue.Value;
                        if (valueInfo == null)
                        {
                            mLog.Warn($"The valueInfo is null,need skip.Column:{columnName}.");
                            continue;
                        }
                        if (valueInfo.ColValue == null)
                        {
                            mLog.Warn($"The column value is null,need skip.Column:{fieldValue.Key}.");
                            continue;
                        }

                        SPField field = new SPField();

                        switch (columnName)
                        {
                            case "Author":
                                listItem.Author = valueInfo.ColValue.ToString();
                                try
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
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
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
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
                                                if ((!userIDs[i].Contains("\\")) && !mUserGroupMappingForCurrentPackage.Contains(userPricinpleId))
                                                {
                                                    mUserGroupMappingForCurrentPackage.Add(userPricinpleId);
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
                            IAveField formField = mCurrentAveList.Fields.GetFieldByInternalName("NFFormData");
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
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.column process failed.ColumnName:{columnName}.Message:{ex}.");
                    }
                }
                #region add ArchiverLinkFileType column
                SPField linkfield = new SPField();
                linkfield.Name = LinkFileCommon.LinkFileFieldName;
                linkfield.Value = LinkFileCommon.GenerateLinkFieldValue(mConfig.JobId);
                linkfield.ID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
                fieldCollection.Field.Add(linkfield);
                foreach (var name in NeedSetNullFields)
                {
                    try
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
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.SetNullFields failed.ColumnName:{name}.Message:{ex}.");
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while processing fields, Exception:{0}", e.ToString());
                throw;
            }
            return fieldCollection;
        }

        protected SPFieldCollection ProcessFieldCollection(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, SPListItem listItem, IAveListItem item, int docRowId, int version, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction)
        {
            SPFieldCollection fieldCollection = new SPFieldCollection();
            try
            {
                string taxonomyListId = Guid.Empty.ToString();
                //Wait Wrapper Team provide method
                ItemMetadataForHSMConnector itemData = new ItemMetadataForHSMConnector(mConfig.aveObjectModelFactory, item, version, docRowId, userData, dataJunction);
                Dictionary<string, AveFieldValueInfo> fieldValues = itemData.ProcessItemMetadata();
                if (fieldValues == null)
                {
                    mLog.Warn($"ProcessFieldCollection.The fieldValues is null.");
                    return fieldCollection;
                }
                List<string> NeedSetNullFields = SetNeedSetNullFieldsEx(fieldValues?.Keys.ToList());
                var termIdCache = new List<string>();
                foreach (var fieldValue in fieldValues)
                {
                    string columnName = string.Empty;
                    try
                    {
                        columnName = fieldValue.Key;
                        AveFieldValueInfo valueInfo = fieldValue.Value;
                        if (valueInfo == null)
                        {
                            mLog.Warn($"The valueInfo is null,need skip.Column:{columnName}.");
                            continue;
                        }
                        if (valueInfo.ColValue == null)
                        {
                            mLog.Warn($"The column value is null,need skip.Column:{fieldValue.Key}.");
                            continue;
                        }

                        SPField field = new SPField();

                        switch (columnName)
                        {
                            case "Author":
                                listItem.Author = valueInfo.ColValue.ToString();
                                try
                                {
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
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
                                    if (!mUserGroupMappingForCurrentPackage.Contains(Convert.ToInt32(valueInfo.ColValue)))
                                    {
                                        mUserGroupMappingForCurrentPackage.Add(Convert.ToInt32(valueInfo.ColValue));
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
                                                if ((!userIDs[i].Contains("\\")) && !mUserGroupMappingForCurrentPackage.Contains(userPricinpleId))
                                                {
                                                    mUserGroupMappingForCurrentPackage.Add(userPricinpleId);
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
                            IAveField formField = mCurrentAveList.Fields.GetFieldByInternalName("NFFormData");
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
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.column process failed.ColumnName:{columnName}.Message:{ex}.");
                    }
                }
                #region add ArchiverLinkFileType column
                SPField linkfield = new SPField();
                linkfield.Name = LinkFileCommon.LinkFileFieldName;
                linkfield.Value = LinkFileCommon.GenerateLinkFieldValue(archiverFileIndex.JobId.Substring(0, archiverFileIndex.JobId.LastIndexOf('_')));
                linkfield.ID = "b4b338db-fc52-4bf4-a363-0ae0b59ec1cd";
                fieldCollection.Field.Add(linkfield);
                foreach (var name in NeedSetNullFields)
                {
                    try
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
                    catch (Exception ex)
                    {
                        mLog.Warn($"ProcessFieldCollection.SetNullFields failed.ColumnName:{name}.Message:{ex}.");
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while processing fields, Exception:{0}", e.ToString());
                throw;
            }
            return fieldCollection;
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
                mSPFileObject.Url = mCurrentAveList.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + file.Url;
                SPFile fileObject = (SPFile)mSPFileObject.Item;
                mSPFileObject.Item = file;
            }
        }

        private void ProcessFileObjectNode(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, SPFile file)
        {
            string id = archiverFileIndex.Id;
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

        private MetadataCacheInfo GetTermInfo(string columnName, AveFieldValueInfo valueInfo, int LCID = -1, bool forceAddTerm = true)
        {
            IAveTaxonomyField tField = null;
            Dictionary<Guid, IAveTerm> termCache = new Dictionary<Guid, IAveTerm>();
            Dictionary<Guid, Guid> termIdMapping = new Dictionary<Guid, Guid>();
            Dictionary<Guid, List<Guid>> mergedTermIdMapping = new Dictionary<Guid, List<Guid>>();
            tField = GetTaxonomyField(columnName);
            var textField = mCurrentAveList.Fields.GetFieldById(tField.TextField, false);

            if (!mMetadataInfoList.ContainsKey(columnName))
            {
                mMetadataInfoList[columnName] = new MetadataCacheInfo { TextFieldName = textField.InternalName, TextFieldId = textField.ID };
            }

            IAveTaxonomySession session = mCurrentAveList.ParentWeb.Site.AveSPTaxonomySession;
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
                    else
                    {
                        mLog.Warn("Can't find term info, term label {0}", termName);
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
            var aveField = mCurrentAveList.Fields.GetFieldByInternalName(columnName, false);
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
                IAveFieldLookup lookupField = mCurrentAveList.Fields.GetById(valueInfo.Id) as IAveFieldLookup;
                Guid lookupListId = new Guid(lookupField.LookupList);
                IAveList lookupList = mCurrentAveList.ParentWeb.Site.OpenWeb(lookupField.LookupWebId).GetList(lookupListId);
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

        private List<string> SetNeedSetNullFieldsEx(List<string> fieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AveSPList.SetNeedSetNullFields"))
            {

                List<string> needSetNullFields = new List<string>();
                string[] AllCols = new string[] {"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
                "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

                //ExternalList 没有ColName，会抛异常
                if (mCurrentAveList != null && mCurrentAveList.BaseTemplate != AveListTemplateType.ExternalList && (int)mCurrentAveList.BaseTemplate != 160)
                {
                    IAveFieldCollection fieldCollection = mCurrentAveList.Fields;
                    bool isCollecterList = mCurrentAveList.IsConnectorList.HasValue ? mCurrentAveList.IsConnectorList.Value : false;
                    foreach (IAveField field in fieldCollection)
                    {
                        try
                        {
                            object obj = field.ColName;
                            if (obj != null
                                //ADO-129426 item的SetNeedSetNullFields逻辑中，过滤BaseType是Facilities类型的column，在还column的过程中，
                                //如果将这个column设为null，在update的时候会报System.Exception: Field or property "Facilities" does not exist.的错。
                                && !string.Equals(field.TypeAsString, "Facilities", StringComparison.OrdinalIgnoreCase)
                                //ADO-89825 App Store Site中，特殊field AppMetadataLocale不能设置为null。
                                && !field.ID.Equals(new Guid("{14c6cd06-7417-42c1-a051-89e455fd1090}")))
                            {
                                string colName = obj.ToString();
                                if (IsColColumn(colName) && IsSupportToSetNull(field.InternalName))
                                {
                                    if (field.Type == AveFieldType.WorkflowStatus || fieldValues.Exists(name => name.Equals(field.InternalName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        continue;
                                    }
                                    needSetNullFields.Add(field.InternalName);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                        }
                    }
                }
                return needSetNullFields;

            }

        }

        private bool IsColColumn(string colName)
        {
            //添加对column 类型的判断，SP对类型的数量是有限制的，可以通过SP的数据库查询，当前没发现超过数据的情况，因此没有添加对于超过限制的判断，如果有问题，需要添加检查类型数量的逻辑
            List<string> allcols = new List<string> { "nvarchar", "ntext", "sql_variant", "int", "float", "datetime", "bit", "uniqueidentifier" };
            Regex reg = new Regex("^(nvarchar|ntext|sql_variant|int|float|datetime|bit|uniqueidentifier)[0-9]*$");
            return reg.IsMatch(colName);
        }

        private bool IsSupportToSetNull(string internalName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsSupportToSetNull"))
            {
#endif
            bool isSupportToSetNull = true;
            try
            {
                if ((string.Equals(internalName, "_dlc_Reporting_TemplateId", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_QueryAssembly", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_InjectionAssembly", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_InjectionClass", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_IconUrl", StringComparison.Ordinal)
                    || string.Equals(internalName, "_dlc_Reporting_HttpContentType", StringComparison.Ordinal)
                    && IsReportingMetadataList()))
                {
                    isSupportToSetNull = false;
                }
            }
            catch (Exception e)
            {
                mLog.Warn("charge whether the list field is '_dlc_Reporting_TemplateId', Exception:{0}", e.ToString());
            }
            return isSupportToSetNull;
#if PerformanceLog
            }
#endif
        }

        private bool IsReportingMetadataList()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsReportingMetadataList"))
            {
#endif
            bool isReportMetadataList = false;
            try
            {
                IAveList list = mCurrentAveList;
                IAveWeb web = mCurrentAveList.ParentWeb;
                if (web.Properties != null)
                {
                    if (web.Properties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        if (string.Equals(web.Properties["_reportinggallerymetadataid"].ToString(), list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
                else
                {
                    if (web.AllProperties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        if (string.Equals(web.AllProperties["_reportinggallerymetadataid"].ToString(), list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("charge whether the list is reporting metadata error,Exception:{0}", e.ToString());
            }
            return isReportMetadataList;
#if PerformanceLog
            }
#endif
        }

        private void ProcessListItemNode(IAveFile aveFile, Dictionary<string, object> userData, List<Dictionary<string, object>> dataJunction, SPListItem item, List<AveRoleAssignmentInfo> roleAssignments, out string ContainerId)
        {
            string id = aveFile.UniqueId.ToString();
            lock (mSPObjectCollection)
            {
                ContainerId = currentHSMLocalInfo.ContainerId;
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
                mSPObject.ParentWebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl;
                mSPObject.ParentWebId = mCurrentAveList.ParentWeb.ID.ToString();
                mSPObject.ParentId = mCurrentAveList.ID.ToString();
                mSPObject.Url = mCurrentAveList.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.FileUrl;

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

        private void ProcessListItemNode(Media.Service.DomainModel.ArchiverBasicIndex archiverFileIndex, IAveFile aveFile, SPListItem item, List<AveRoleAssignmentInfo> roleAssignments)
        {
            string id = archiverFileIndex.Id;
            lock (mSPObjectCollection)
            {
                if (mSPObjectCollection == null)
                {
                    mSPObjectCollection = new SPGenericObjectCollection();
                }
                var mSPObject = new SPGenericObject();
                mSPObject.Id = id;
                mSPObject.Item = new SPListItem();
                mSPObjectCollection.SPObject.Add(mSPObject);

                var list = aveFile.Item.ParentList;

                mSPObject.Id = item.Id;
                mSPObject.Name = item.Name;
                mSPObject.ObjectType = SPObjectType.SPListItem;
                mSPObject.ParentId = item.ParentWebId;
                mSPObject.ParentWebUrl = list.ParentWeb.ServerRelativeUrl; 
                mSPObject.ParentWebId = list.ParentWeb.ID.ToString();
                mSPObject.ParentId = list.ID.ToString();
                mSPObject.Url = list.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.FileUrl;

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
                mRoleAssignmentsObject.ParentId = mCurrentAveList.ParentWeb.ID.ToString();
                mRoleAssignmentsObject.ParentWebId = mCurrentAveList.ParentWeb.ID.ToString();
                mRoleAssignmentsObject.ParentWebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl;
                mRoleAssignmentsObject.ObjectType = SPObjectType.DeploymentRoleAssignments;

                DeploymentRoleAssignments roleAssignmentsObj = new DeploymentRoleAssignments();
                mRoleAssignmentsObject.Item = roleAssignmentsObj;
            }

            DeploymentRoleAssignment roleAssignment = new DeploymentRoleAssignment();
            roleAssignment.ScopeId = Guid.NewGuid().ToString();
            roleAssignment.ObjectId = objectId;
            roleAssignment.ObjectType = "2";
            roleAssignment.Assignment = new List<DeploymentAssignment>();
            roleAssignment.RoleDefWebId = mCurrentAveList.ParentWeb.ID.ToString();
            roleAssignment.RoleDefWebUrl = mCurrentAveList.ParentWeb.ServerRelativeUrl;
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
                        if (!mUserGroupMappingForCurrentPackage.Contains(roleAssignmentInfo.PrincipalId))
                        {
                            mUserGroupMappingForCurrentPackage.Add(roleAssignmentInfo.PrincipalId);
                        }
                        assignment.RoleId = roleAssignmentInfo.RoleId.ToString();
                        roleAssignment.Assignment.Add(assignment);
                    }
                }
                ((DeploymentRoleAssignments)mRoleAssignmentsObject.Item).RoleAssignment.Add(roleAssignment);
                var principalIdsLogBuilder = new StringBuilder();
                if (roleAssignmentsInfo != null && roleAssignmentsInfo.Count > 0)
                {
                    foreach (var roleAssignmentInfo in roleAssignmentsInfo)
                    {
                        if (principalIdsLogBuilder.Length > 0)
                        {
                            principalIdsLogBuilder.Append(",");
                        }

                        principalIdsLogBuilder.Append(roleAssignmentInfo?.PrincipalId ?? 0);
                    }
                }

                mLog.Info("HSMStubPermissionPackaged|Url={0}|roleAssignmentsCount={1}|principalIds=[{2}]",
                    objectUrl,
                    roleAssignmentsInfo?.Count ?? 0,
                    principalIdsLogBuilder.ToString());
            }
            catch (Exception e)
            {
                mLog.Warn(string.Format("ImportRestoreItemSecurityWarn", e.Message));
            }
        }

        private void CopyImportReport(string sourceDirectory, string targetDirectory, bool onlyXML)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyRecursive(diSource, diTarget, onlyXML);
        }

        private void CopyRecursive(DirectoryInfo source, DirectoryInfo target, bool onlyLog)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                if (onlyLog && fi.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                else
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyRecursive(diSourceSubDir, nextTargetSubDir, onlyLog);
            }
        }
        private void DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                mLog.Error("delete file faile." + ex);
            }
        }

        #endregion
    }
}
