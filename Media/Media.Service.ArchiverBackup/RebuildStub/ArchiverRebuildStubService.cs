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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Common;
using Merged18NResources.MediaServiceArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Core.Index;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Common;
using AvePoint.RA.Contract.Archiver;
using Storage;
using AvePoint.RA.Common.Util;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Media.Service.DomainModel;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Diagnostics;
using AvePoint.GCommon.Utility;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public class ArchiverRebuildStubService
        : RebuildStubServiceBase<ArchiverRebuildStubInfo>
        , IRebuildStubService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ArchiverRebuildStubInfo archiverRebuildStubInfo = new ArchiverRebuildStubInfo();
        JobStatusInfo jobStatusInfo = new JobStatusInfo();
        IXSystem indexLogicalDevice;
        String indexVolume;

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IArchiverRetentionIndexService RetentionIndexService { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }

        public IMArchiverJobManagementService ArchiverJobManagementService { get; set; }

        public IStorageDeviceManager DeviceManager { get; set; }
        private IAveORecords Record
        {
            get
            {
                IAveORecords records = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.Auto).CreateRecords();
                return records;
            }
        }
        public override void Open(ArchiverRebuildStubInfo rebuildStubInfo)
        {
            this.archiverRebuildStubInfo = rebuildStubInfo;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceOpenStart, this.archiverRebuildStubInfo.JobId);
            this.jobStatusInfo.State = 2;
            this.indexVolume = rebuildStubInfo.IndexVolume;
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            this.indexLogicalDevice = this.DeviceManager.Open(this.archiverRebuildStubInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(rebuildStubInfo.CacheSetting, false, true);
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);
            this.OpenMainIndex(this.archiverRebuildStubInfo, this.indexVolume);

        }
        

        public override void Dispose()
        {
            if (this.IndexService != null)
            {
                this.IndexService.Close();
            }
            if (this.DeviceManager != null)
            {
                this.DeviceManager.Close(this.indexLogicalDevice);
            }
        }

        private static string GetWebServerRelativeUrl(string webUrl, IAveSite site)
        {
            if (webUrl.TrimEnd('/').Length == site.Url.TrimEnd('/').Length)
            {
                return string.Empty;
            }
            else
            {
                int hostLength = site.Url.Length - site.ServerRelativeUrl.Length;
                var result = webUrl.Substring(hostLength, webUrl.Length - hostLength);
                return result.Substring(result.IndexOf('/'));
            }
        }
        private string EnsureStubType(string stubType)
        {
            switch (stubType)
            {
                case "Aspx":
                    logger.Info("stub type is aspx.");
                    return ".aspx";
                case "Html":
                    logger.Info("stub type is html.");
                    return ".html";
                case "Txt":
                    logger.Info("stub type is txt.");
                    return ".txt";
                case "Link":
                    logger.Info("stub type is link.");
                    return ".url";
                default:
                    logger.Warn("stub type is empty or the type not exist.");
                    return string.Empty;
            }
        }
        private async Task RebuildStubFromSharePoint(Dictionary<string, List<ArchiverBasicIndex>> docs, string tenantGroupId, string siteUrl, string jobId, string stubType)
        {
            try
            {
                if (docs != null && docs.Count > 0)
                {
                    List<string> defaultStubType = null;
                    AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    if (remoteSiteCollection == null)
                    {
                        logger.Warn($"Cannot find {siteUrl} in the RemoteSiteCollection table. so skip remove stub.");
                        return;
                    }
                    AveBPOSAccountInfo bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                    var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
                    if (string.IsNullOrEmpty(stubType))
                    {
                        defaultStubType = new List<string> { ".aspx", ".html", ".txt", ".url" };
                    }
                    else
                    {
                        if (stubType == "null")
                        {
                            logger.Info("this job is not create stub job");
                            return;
                        }
                        defaultStubType = new List<string> { EnsureStubType(stubType) };
                    }
                    using (IAveSite mSite = aveObjectModelFactory.CreateSite(siteUrl))
                    {
                        foreach (var webUrl in docs.Keys)
                        {
                            if (docs[webUrl].Count <= 0)
                            {
                                logger.Warn($"Current web:{webUrl} does not have rebuild stub files.");
                                continue;
                            }
                            var webServerRelatedUrl = GetWebServerRelativeUrl(webUrl, mSite);
                            using (IAveWeb web = mSite.OpenWeb(webServerRelatedUrl))
                            {
                                foreach (var doc in docs[webUrl])
                                {
                                    try
                                    {
                                        foreach (var stub in defaultStubType)
                                        {
                                            var stubRelativeUrl = GetWebServerRelativeUrl(string.Format("{0}{1}", doc.Url, stub), mSite);
                                            var stubFile = web.GetFile(stubRelativeUrl);
                                            bool isStubMatch = false;
                                            if (stubFile.Exists)
                                            {
                                                try
                                                {
                                                    if (stubFile.Item != null)
                                                    {
                                                        var archiverLinkFileType = stubFile.Item.FieldValues["ArchiverLinkFileType"];
                                                        isStubMatch = archiverLinkFileType.ToString().StartsWith(jobId.Substring(0, jobId.LastIndexOf('_')));
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Warn("file not a stub,because it's fieldValues does not contain ArchiverLinkFileType,error:{0}", e.ToString());
                                                    continue;
                                                }
                                                if (isStubMatch)
                                                {
                                                    try
                                                    {
                                                        Stopwatch stopwatch = Stopwatch.StartNew();
                                                        DateTime stubFileModified = DateTime.Now;
                                                        string stubFileEditor = string.Empty;
                                                        try
                                                        {
                                                            stubFileModified = (DateTime)stubFile.Item["Modified"];
                                                            stubFileEditor = stubFile.Item["Editor"].ToString();
                                                        }
                                                        catch (Exception es)
                                                        {
                                                            logger.Info($"Error get stub file modifed property when rebuild stub.MD5:{doc.PathMD5}.Message:{es}.");
                                                        }
                                                        
                                                        logger.Info($"Begin rebuild stub.MD5:{doc.PathMD5}.JobID:{archiverRebuildStubInfo.JobId}.");
                                                        //declare file need undeclare.
                                                        if (CheckisRecord(stubFile.Item))
                                                        {
                                                            logger.Info($"RebuildStub current file is declare file.MD5:{doc.PathMD5}.");
                                                            Record.UndeclareItemAsRecord(stubFile.Item);
                                                            stubFile = web.GetFile(stubRelativeUrl);
                                                            logger.Info($"RebuildStub UndeclareItemAsRecord success.MD5:{doc.PathMD5}.");
                                                        }
                                                        var psc = await RebuildStubFileCommon.SetStubContentValueAsync(
                                                            stubFile,
                                                            doc.Name,
                                                            doc.Url,
                                                            archiverRebuildStubInfo.StubSettingDto, 
                                                            doc.PathMD5,
                                                            remoteSiteCollection.TenantId,
                                                            TenantLocalValue.LogonGroupId,
                                                            remoteSiteCollection.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro,
                                                            (LeaveStubType)archiverRebuildStubInfo.StubSettingDto.StubType,
                                                            archiverRebuildStubInfo.RebuildJobId,
                                                            bposInfo,
                                                            doc.ArchiveTime
                                                            );
                                                        byte[] mStubBytes = RebuildStubFileCommon.GetFileContent((LeaveStubType)archiverRebuildStubInfo.StubSettingDto.StubType, stubFile.ParentFolder.ParentList.ParentWeb.LanguageCulture, psc);
                                                        try
                                                        {
                                                            stubFile.SaveBinary(mStubBytes);
                                                        }
                                                        catch (Exception sex)
                                                        {
                                                            //Versuni has auto check out setting library.
                                                            if (sex != null && sex.Message.Contains("You must first check out this document before making changes"))
                                                            {
                                                                logger.Info($"RebuildStub SaveBinary need check out file.MD5:{doc.PathMD5}.Message:{sex}.");
                                                                stubFile.CheckOut();
                                                                stubFile.SaveBinary(mStubBytes);
                                                            }
                                                            else
                                                            {
                                                                throw;
                                                            }
                                                        }
                                                        if (archiverRebuildStubInfo.KeepStubModifiedAndModifiedBy)
                                                        {
                                                            try
                                                            {
                                                                logger.Info($"RebuildStub begin KeepStubModifiedAndModifiedBy.MD5:{doc.PathMD5}.");
                                                                stubFile.Item["Modified"] = stubFileModified;
                                                                stubFile.Item["Editor"] = stubFileEditor;
                                                                stubFile.Item.SystemUpdate();
                                                                logger.Info($"RebuildStub finish KeepStubModifiedAndModifiedBy.MD5:{doc.PathMD5}.");
                                                            }
                                                            catch (Exception ksm)
                                                            {
                                                                logger.Warn($"RebuildStub KeepStubModifiedAndModifiedBy error.MD5:{doc.PathMD5}.Message:{ksm}.");
                                                            }
                                                        }
                                                        if (archiverRebuildStubInfo.StubSettingDto.IsDeclareStubAsRecords)
                                                        {
                                                            try
                                                            {
                                                                //currently one drive site does not allow declare record, so catch this exception.
                                                                logger.Info($"RebuildStub IsDeclareStubAsRecords.MD5:{doc.PathMD5}.");
                                                                Record.DeclareItemAsRecord(stubFile.Item);
                                                                logger.Info($"RebuildStub DeclareItemAsRecord success.MD5:{doc.PathMD5}.");
                                                            }
                                                            catch (Exception dex)
                                                            {
                                                                logger.Warn($"RebuildStub declare file failed.MD5:{doc.PathMD5}.Message:{dex}.");
                                                            }
                                                        }
                                                        stopwatch.Stop();
                                                        logger.Info($"Success rebuild stub.MD5:{doc.PathMD5}.JobID:{archiverRebuildStubInfo.JobId}.RebuildStubTime:{stopwatch.Elapsed}.");
                                                        AddToReport(archiverRebuildStubInfo.SiteUrl, doc.Url + defaultStubType[0], JobDetailsStatus.Successful, archiverRebuildStubInfo.JobId);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Info($"Rebuild Stub file exception: {e.Message}. retry action.");
                                                        AddToReport(archiverRebuildStubInfo.SiteUrl, doc.Url + defaultStubType[0], JobDetailsStatus.Failed, archiverRebuildStubInfo.JobId, e.Message);
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    logger.Info(string.Format("stub type: {0} does not exist in library.", System.IO.Path.GetExtension(stub)));
                                                    AddToReport(archiverRebuildStubInfo.SiteUrl, doc.Url + defaultStubType[0], JobDetailsStatus.Skipped, archiverRebuildStubInfo.JobId, "Stub is not AvePoint stub.");
                                                }
                                            }
                                            else
                                            {
                                                logger.Info("current stub type:{0} not exsit.", stub);
                                                AddToReport(archiverRebuildStubInfo.SiteUrl, doc.Url + defaultStubType[0], JobDetailsStatus.Skipped, archiverRebuildStubInfo.JobId, "Stub does not exist.");
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"delete stubfile failed reson:{e.ToString()}");
                                        AddToReport(archiverRebuildStubInfo.SiteUrl, doc.Url + defaultStubType[0], JobDetailsStatus.Failed, archiverRebuildStubInfo.JobId, "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.Info($"the job:{jobId} has no stub need to rebuild.");
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Error in rebuild stub.reason : {0}.", e.ToString()));
            }
        }

        public static bool CheckisRecord(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckisRecord"))
            {
                bool isRecord = false;
                int result = 0;
                try
                {
                    object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                    if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                }
                catch (Exception ex)
                {
                    //mLog.Info(ex.ToString());
                    result = 0;
                }
                if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
                {
                    isRecord = true;
                }
                return isRecord;
            }
        }

        public override void RealRebuildStub(ArchiverRebuildStubInfo rebuildStubInfo)
        {
            this.logger.Info("Begin Real Rebuild Stub.");
            String stubType = string.Empty;
            //string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(string.Empty, this.archiverRebuildStubInfo.JobId);
            var stubUrlList = this.RetentionIndexService.FilterDocumentsByJobId(this.archiverRebuildStubInfo.JobId, ref stubType);
            this.logger.Info($"Get rebuild stub web count:{stubUrlList.Count}.");
            RebuildStubFromSharePoint(stubUrlList, rebuildStubInfo.TenantGroupId, rebuildStubInfo.SiteUrl, this.archiverRebuildStubInfo.JobId, stubType);
            this.logger.Info("Finish Real Rebuild Stub.");
        }
        
        private void OpenMainIndex(ArchiverRebuildStubInfo archiverRebuildStubInfo, String indexVolume)
        {
            this.logger.Info("Begin opening main index.");
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                BackupJobId = archiverRebuildStubInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRebuildStubInfo.CacheSetting,
                StorageInfo = archiverRebuildStubInfo.MainIndexStorageInfo
            };
            IndexSynchronizer.Initialize(indexServiceOpenParameter);
            this.InitIndexProcessor(indexServiceOpenParameter);
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.IndexMainProcessor.Open(param);
            }
            this.logger.Info("Open Main Index Finished.");
        }
        
    }
}
