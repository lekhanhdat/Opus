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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using HSMCommon;
//using SP2013ComplianceVaultCommonUtility;
using LS.SPWorkflowProcessor;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using PnP.Framework.Diagnostics;
using Polly;
using RAArchiverCommon;
using RAExportCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Util;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2012/8/7",
      "ruiheng.liu@AvePoint.com",
      "yanlong.gu@AvePoint.com",
      new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
      "ADO-44684",
      true
      )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/11/2",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
            {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
            },
    "ADO-53910",
    false
    )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2013/2/28",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
            {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
            },
    "ADO-63322",
    false
    )]
    internal class SiteCollectionBackup : SPObjectBackup
    {
        public SiteCollectionBackup(AveLogger log)
        {
            mLog = log;
        }



        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                AveSPSite aveSite;
                aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, AveSender.BackupStream);
                current.WrapperObject = aveSite;
                aveSite.SetLanguageMappingProcesser(AveLanguageProcesser.GetLanguageInstance(AveEnv.AgentRootFolder, ""));
                current.FileHeader = AveSender.GeneSiteHeader(aveSite, entity, AveSender.BackupStream.StreamTransfered, ruleName, subJobId, mediaName, aveSite.SPSite.Url);
                if (!BriefScanDBOperation.GetInstance(Configuration).NodeIsFailProcessed(entity))
                {
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                else
                {
                    current.BackupStatus = FileHeaderStatus.Failed;
                }
                return (int)JobDetailsStatus.Successful;
            }
            catch (Exception e)
            {
                mLog.Error("Build cache of Site Collection Error: {0}", e.ToString());
                throw;
            }
        }


        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsSiteCollectionBackupbackup);
            string errorMessage = string.Empty;
            bool hasBackupHeader = false;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.SiteCollectionBackup"))
                {
                    AveSPSite aveSite;
                    if (entity.IsArchiveBy365)
                    {
                        aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, AveSender?.BackupStream);
                        current.WrapperObject = aveSite;
                        return 0;
                    }
                    aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, AveSender.BackupStream);
                    //add for RevIM export
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        SiteCollectionVault siteCollectionVault = (SiteCollectionVault)VaultExport;
                        siteCollectionVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        siteCollectionVault.VaultExport(aveSite, entity, subJobId, ruleName, mediaName);
                    }
                    current.WrapperObject = aveSite;
                    aveSite.SetLanguageMappingProcesser(AveLanguageProcesser.GetLanguageInstance(AveEnv.AgentRootFolder, ""));

                    current.FileHeader = AveSender.BackupSiteHeader(aveSite, entity, AveSender.BackupStream.StreamTransfered, ruleName, subJobId, mediaName, aveSite.SPSite.Url);
                    hasBackupHeader = true;
                    var stream = AveSender.BackupStream;
                    stream.BeginWriteMetadata();
                    try
                    {
                        AveSPSiteInfo aveSPSiteInfo = new AveSPSiteInfo(aveSite);
                        mLog.Info("Start to export SP Site info.");
                        aveSPSiteInfo.Export(stream);
                        mLog.Info("End to export SP Site info.");
                        AveSPSiteFeature featureManager = new AveSPSiteFeature(aveSite);
                        mLog.Info("Start to export feature manager.");
                        featureManager.Export(stream);
                        mLog.Info("End to export feature manager.");
                        AveSPSiteSettingInfo aveSPSiteSettingInfo = new AveSPSiteSettingInfo(aveSite);
                        mLog.Info("Start to export SP Site setting.");
                        aveSPSiteSettingInfo.Export(stream);
                        mLog.Info("End to export SP Site setting.");
                        AveLanguage aveSPSiteLanguageInfo = AveLanguage.CreateInstance(aveSite);
                        mLog.Info("Start to export SP Site language.");
                        aveSPSiteLanguageInfo.Export(stream);
                        mLog.Info("End to export SP Site language.");
                        AveUser users = AveUser.CreateInstance(aveSite);
                        mLog.Info("Start to export users.");
                        users.Export(stream, true);
                        mLog.Info("End to export users.");
                        AveGroup groups = AveGroup.CreateInstatnce(aveSite);
                        mLog.Info("Start to export groups.");
                        groups.Export(stream, true);
                        mLog.Info("End to export groups.");

                        try
                        {
                            if (Configuration.IncludeMetadataService)
                            {
                                AveMetadataService metadataService = new AveMetadataService(aveSite.SPSite);
                                metadataService.IsTeamsLevelJob = !string.IsNullOrEmpty(Configuration.ForceFitTeamsRuleID);
                                mLog.Info("Start to export metadata service.");
                                metadataService.Export(stream);
                                mLog.Info("End to export metadata service.");
                            }
                            else
                            {
                                mLog.Warn("Skip Backup Metadata Services ");
                            }
                        }
                        catch (Exception mx)
                        {
                            mLog.Error("Backup Metadata Error :{0}", mx.ToString());
                        }
                        Dictionary<string, object> fullText = new Dictionary<string, object>();
                        foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                        {
                            try
                            {
                                fullText[tag.Key] = tag.Value;
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                            }
                        }
                        mLog.Info("Start to export full text index.");
                        aveSite.ExportFullTextIndex(stream, fullText);
                        mLog.Info("End to export full text index.");
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup Site Collection Metadata Error: {0}", e.ToString());
                        status = JobDetailsStatus.Failed;
                        current.DoDelete = false;
                        throw;
                    }
                    finally
                    {
                        AveSender.BackupStream.EndWriteMetadata();
                        AveSender.BackupStream.FlushMetadata(0);
                        if (hasBackupHeader)
                        {
                            AveSender.BackupTail(status == JobDetailsStatus.Successful);
                        }
                        else
                        {
                            mLog.Warn($"Backup Site Collection:{entity.FullPath} does not backup header so skip BackupTail.");
                        }
                        current.BackupStatus = FileHeaderStatus.Complete;
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Site Collection Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.DoDelete = false;
                current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            finally
            {
                if (!entity.IsArchiveBy365)
                {
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsSiteCollectionBackupArchiveLevel, entity.ArchiveLevel.ToString());
                    if (current.BackupStatus == FileHeaderStatus.Failed && Configuration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == true)
                    {
                        BriefScanDBOperation.GetInstance(Configuration).InsertFailProcessedNodeToDB(entity);
                    }
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(entity.FullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                }
                else
                {
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                }
            }
            return 0;
        }
    }

    internal class WebBackup : SPObjectBackup
    {
        public WebBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                string realFullPath = entity.FullPath;
                var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                AveSPWeb aveWeb = new AveSPWeb(aveSite, new Guid(entity.NodeId), entity.LeafName);
                realFullPath = aveWeb.SPWeb.Url;
                Configuration.TimeZone = AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                current.WrapperObject = aveWeb;
                AveSender.BackupWebHeader(aveWeb, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, aveWeb.SPWeb.Url);
                current.FileHeader = AveSender.GenerateHeader(realFullPath);
                AveSender.SetHeaderAsArchiveSuccessForEnableDelete(current.FileHeader);
                if (!BriefScanDBOperation.GetInstance(Configuration).NodeIsFailProcessed(entity))
                {
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                else
                {
                    current.BackupStatus = FileHeaderStatus.Failed;
                }
                return (int)JobDetailsStatus.Successful;
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for web,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsWebBackupbackup);
            string errorMessage = string.Empty;
            string realFullPath = entity.FullPath;
            bool hasBackupHeader = false;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            var tail = new StringBuilder();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.WebBackup"))
                {
                    AveSPWeb aveWeb;
                    if (entity.IsArchiveBy365)
                    {
                        var tempAveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                        aveWeb = new AveSPWeb(tempAveSite, new Guid(entity.NodeId), entity.LeafName);
                        current.WrapperObject = aveWeb;
                        return 0;
                    }
                    var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                    aveWeb = new AveSPWeb(aveSite, new Guid(entity.NodeId), entity.LeafName);
                    realFullPath = aveWeb.SPWeb.Url;
                    //add for RevIM export
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        WebVault webVault = (WebVault)VaultExport;
                        webVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        webVault.VaultExport(aveWeb, entity, subJobId, ruleName, mediaName);
                    }
                    Configuration.TimeZone = AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                    current.WrapperObject = aveWeb;
                    AveSender.BackupWebHeader(aveWeb, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, aveWeb.SPWeb.Url);
                    hasBackupHeader = true;
                    current.FileHeader = AveSender.BackupHeader(realFullPath);
                    var stream = AveSender.BackupStream;
                    AveSender.BackupStream.BeginWriteMetadata();
                    try
                    {
                        AveSPWebInfo webInfo = new AveSPWebInfo(aveWeb);
                        mLog.Info("Start to export web info.");
                        webInfo.Export(stream);
                        mLog.Info("End to export web info.");
                        AveSPWebFeature featureManager = new AveSPWebFeature(aveWeb);
                        mLog.Info("Start to export feature manager.");
                        featureManager.Export(stream);
                        mLog.Info("End to export feature manager.");
                        AveSPWebSettingInfo webSettingInfo = new AveSPWebSettingInfo(aveWeb);
                        mLog.Info("Start to export web setting info.");
                        webSettingInfo.Export(stream);
                        mLog.Info("End to export web setting info.");
                        AveLanguage webLanguageInfo = AveLanguage.CreateInstance(aveWeb);
                        mLog.Info("Start to export web language info.");
                        webLanguageInfo.Export(stream);
                        mLog.Info("End to export web language info.");
                        mLog.Info("Start to export fields");
                        aveWeb.ExportFields(stream);
                        mLog.Info("End to export fields");
                        mLog.Info("Start to export content types.");
                        aveWeb.ExportContentTypes(stream);
                        mLog.Info("End to export content types.");

                        AveSPEventReceiver eventReceiver = AveSPEventReceiver.CreateInstance(aveWeb);
                        mLog.Info("Start to export event receiver.");
                        eventReceiver.Export(stream);
                        mLog.Info("End to export event receiver.");
                        AveSPNavigation navigation = new AveSPNavigation(aveWeb);
                        mLog.Info("Start to export navigation.");
                        navigation.Export(stream);
                        mLog.Info("End to export navigation.");
                        AveUser user = AveUser.CreateInstance(aveWeb);
                        mLog.Info("Start to export user.");
                        user.Export(stream, true);
                        mLog.Info("End to export user.");
                        AveGroup group = AveGroup.CreateInstatnce(aveWeb);
                        mLog.Info("Start to export group.");
                        group.Export(stream);
                        mLog.Info("End to export group.");
                        if (aveWeb.SPWeb.HasUniqueRoleDefinitions || aveWeb.SPWeb.HasUniqueRoleAssignments)
                        {
                            AveRoles roles = new AveRoles(aveWeb);
                            mLog.Info("Start to export roles.");
                            roles.Export(stream);
                            mLog.Info("End to export roles.");
                        }
                        if (aveWeb.SPWeb.HasUniqueRoleAssignments)
                        {
                            AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveWeb);
                            mLog.Info("Start to export role assignments.");
                            roleAssignments.Export(stream);
                            mLog.Info("End to export role assignments.");
                        }
                        if (SPWorkflowProcessorRuntime.ProcessAssociation)
                        {
                            mLog.Info("Start to export workflow.");
                            aveWeb.ExportWorkflows(stream);
                            mLog.Info("End to export workflow.");
                        }
                        Dictionary<string, object> fullText = new Dictionary<string, object>();
                        foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                        {
                            try
                            {
                                fullText[tag.Key] = tag.Value;
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                            }
                        }
                        mLog.Info("Start to export full text index.");
                        aveWeb.ExportFullTextIndex(AveSender.BackupStream, fullText);
                        mLog.Info("End to export full text index.");
                        current.BackupStatus = FileHeaderStatus.Complete;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup Web Metadata Error: {0}", e.ToString());
                        current.DoDelete = false;
                        throw;
                    }
                    finally
                    {
                        AveSender.BackupStream.EndWriteMetadata();
                        AveSender.BackupStream.FlushMetadata(0);
                        try
                        {
                            XmlElement xe = new XmlDocument().CreateElement("Attribute");
                            if (aveWeb.SPWeb != null)
                            {
                                if (aveWeb.SPWeb.Title != null)
                                {
                                    XmlElement titleInfo = new XmlDocument().CreateElement("Title");
                                    titleInfo.InnerText = aveWeb.SPWeb.Title;
                                    tail.Append(titleInfo.OuterXml);
                                    xe.InnerText = "Title:" + aveWeb.SPWeb.Title;
                                }
                                else
                                {
                                    xe.InnerText = "Title:" + string.Empty;
                                }
                                tail.Append(xe.OuterXml);
                                xe.InnerXml = "WebType:" + XmlConvert.EncodeName(aveWeb.SPWeb.WebTemplate.ToString());
                                tail.Append(xe.OuterXml);
                                xe.InnerXml = "Description:" + EncodingStringUsingBase64(aveWeb.SPWeb.Description);
                                tail.Append(xe.OuterXml);
                                xe.InnerXml = "LastModifiedDate:" + XmlConvert.EncodeName(aveWeb.SPWeb.LastItemModifiedDate.ToString());
                                tail.Append(xe.OuterXml);
                            }
                            xe.InnerXml = "TimeZoneID:" + AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                            tail.Append(xe.OuterXml);
                        }
                        catch (Exception e)
                        {
                            mLog.Error("Backup Web Attribute Error: {0}", e.ToString());
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverStructError);
                        }
                        if (hasBackupHeader)
                        {
                            AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                        }
                        else
                        {
                            mLog.Warn($"Backup Web:{entity.FullPath} does not backup header so skip BackupTail.");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Web Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                throw;
            }
            finally
            {
                if (!entity.IsArchiveBy365)
                {
                    if (current.BackupStatus == FileHeaderStatus.Failed && Configuration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == true)
                    {
                        BriefScanDBOperation.GetInstance(Configuration).InsertFailProcessedNodeToDB(entity);
                    }
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(realFullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                }
                else
                {
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                }
            }
            return 0;
        }

        public string EncodingStringUsingBase64(string content)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(buffer);
        }
    }

    internal class ListBackup : SPObjectBackup
    {
        private HSMConnector HSMConnector
        {
            get
            {
                if (HSMConnectorInstance == null)
                {
                    HSMConnectorInstance = HSMConnector.GetInstance(Configuration);
                }
                return HSMConnectorInstance;
            }
        }
        public ListBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                string listType = string.Empty;
                HeaderUrl headUrl = new HeaderUrl();
                var aveWeb = parent.WrapperObject as AveSPWeb;
                var aveList = new AveSPList(aveWeb, new Guid(entity.NodeId), entity.LeafName, true);
                if (aveList.SPList != null && aveList.SPList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("ExternalList do not suspend");
                }
                if (aveList.SPList != null && aveList.SPList.BaseType == AveBaseType.DocumentLibrary)
                {
                    listType = "2";
                }
                else
                {
                    listType = "0";
                }
                string nameForSpecialChar = AveConverter.EncodeSpecialChar(entity.LeafName);

                AveSender.BackupListHeader(aveList, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, nameForSpecialChar, listType, headUrl.GetUrlBySR(aveWeb.SPWeb.Site.Url, aveWeb.SPWeb.Site.ServerRelativeUrl, aveList.ServerRelativeUrl));
                current.FileHeader = AveSender.GenerateHeader(Configuration.GetNodeFullPath(entity.FullPath));
                AveSender.SetHeaderAsArchiveSuccessForEnableDelete(current.FileHeader);
                aveList.EncodePathForSpecialChar();
                current.WrapperObject = aveList;
                if (!BriefScanDBOperation.GetInstance(Configuration).NodeIsFailProcessed(entity))
                {
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                else
                {
                    current.BackupStatus = FileHeaderStatus.Failed;
                }
                return (int)JobDetailsStatus.Successful;
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache for list,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            return BackupList(parent, current, entity, ruleName, subJobId, ruleLevel, mediaName, AveSender);
        }
        public int BackupList(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupList, entity.FullPath);
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            string listType = string.Empty;
            bool hasBackupHeader = false;
            var tail = new StringBuilder();
            HeaderUrl headUrl = new HeaderUrl();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.ListBackup"))
                {
                    var aveWeb = parent.WrapperObject as AveSPWeb;
                    var aveList = new AveSPList(aveWeb, new Guid(entity.NodeId), entity.LeafName, true);
                    if (entity.IsArchiveBy365)
                    {
                        current.WrapperObject = aveList;
                        return 0;
                    }
                    if (aveList.SPList != null && aveList.SPList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase))
                    {
                        status = JobDetailsStatus.Failed;
                        throw new Exception("ExternalList do not suspend");
                    }
                    //add for RevIM
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        ListVault listVault = (ListVault)VaultExport;
                        listVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        listVault.VaultExport(aveList, entity, subJobId, ruleName, mediaName);
                    }
                    if (aveList.SPList != null && aveList.SPList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        listType = "2";
                    }
                    else
                    {
                        listType = "0";
                    }
                    string nameForSpecialChar = AveConverter.EncodeSpecialChar(entity.LeafName);
                    AveSender.BackupListHeader(aveList, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, nameForSpecialChar, listType, headUrl.GetUrlBySR(aveWeb.SPWeb.Site.Url, aveWeb.SPWeb.Site.ServerRelativeUrl, aveList.ServerRelativeUrl));
                    hasBackupHeader = true;
                    current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                    aveList.EncodePathForSpecialChar();
                    current.WrapperObject = aveList;
                    var stream = AveSender.BackupStream;
                    stream.BeginWriteMetadata();

                    try
                    {
                        if (!aveList.IsSystemList)
                        {
                            var listInfo = new AveSPListInfo(aveList);
                            listInfo.Export(stream);
                            var listSettingInfo = new AveSPListSettingInfo(aveList);
                            listSettingInfo.Export(stream);
                            aveList.ExportFields(stream, false);
                            aveList.ExportContentTypes(stream);
                            AveSPEventReceiver eventReceiver = AveSPEventReceiver.CreateInstance(aveList);
                            eventReceiver.Export(stream);
                            if (aveList.SPList.HasUniqueRoleAssignments)
                            {
                                mLog.Info($"Current List:{entity.FullPath} HasUniqueRoleAssignments.");
                                AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveList);
                                roleAssignments.Export(stream);
                            }
                            AveSPAlert listAlert = AveSPAlert.CreateInstance(aveList);
                            listAlert.Export(stream);
                            if (SPWorkflowProcessorRuntime.ProcessAssociation)
                            {
                                aveList.ExportWorkflows(stream);
                            }
                        }
                        Dictionary<string, object> fullText = new Dictionary<string, object>();
                        foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                        {
                            try
                            {
                                fullText[tag.Key] = tag.Value;
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                            }
                        }
                        aveList.ExportFullTextIndex(stream, fullText);
                        current.BackupStatus = FileHeaderStatus.Complete;
                        if (aveList.SPList != null && aveList.SPList.Fields != null && aveList.SPList.Fields.ContainsField(LinkFileCommon.LinkFileFieldName))
                        {
                            Configuration.LibraryHasStubHiddenColumn = true;
                            mLog.Info($"Current list:{entity.FullPath} have OPUS stub hidden column.");
                        }
                        else
                        {
                            Configuration.LibraryHasStubHiddenColumn = false;
                            mLog.Info($"Current list:{entity.FullPath} does not have OPUS stub hidden column.");
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup List Metadata Error: {0}", e.ToString());
                        current.DoDelete = false;
                        throw;
                    }
                    finally
                    {
                        stream.EndWriteMetadata();
                        stream.FlushMetadata(0);
                    }
                    XmlElement xe = new XmlDocument().CreateElement("Attribute");
                    try
                    {
                        if (aveList.SPList != null)
                        {
                            XmlElement titleInfo = new XmlDocument().CreateElement("Title");
                            titleInfo.InnerText = XmlConvert.EncodeName(aveList.SPList.Title);
                            tail.Append(titleInfo.OuterXml);
                            xe.InnerXml = "Title:" + XmlConvert.EncodeName(aveList.SPList.Title);
                            tail.Append(xe.OuterXml);
                            xe.InnerXml = "ListType:" + ((int)aveList.SPList.BaseType).ToString();
                            tail.Append(xe.OuterXml);
                            xe.InnerXml = "ListBaseTemplate:" + ((int)aveList.SPList.BaseTemplate).ToString();
                            tail.Append(xe.OuterXml);
                            xe.InnerXml = "Description:" + EncodingStringUsingBase64(aveList.SPList.Description ?? "");
                            tail.Append(xe.OuterXml);
                            xe.InnerXml = "LastModifiedDate:" + XmlConvert.EncodeName(aveList.SPList.LastItemModifiedDate.ToString());
                            tail.Append(xe.OuterXml);
                        }
                        xe.InnerXml = "TimeZoneID:" + AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                        tail.Append(xe.OuterXml);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup List Attribute Error: {0}", e.ToString());
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverStructError);
                    }
                    if (HSMConnector.IsStart)
                    {
                        using (AvePerformanceScope avePerformanceScope = new AvePerformanceScope("ArchiverBackup.AddList2Queue"))
                        {
                            if (Configuration.StubUserInfos == null || Configuration.StubUserInfos.Count == 0)
                            {
                                Configuration.StubUserInfos = aveWeb.ParentSite.GetUsers();
                            }

                            if (Configuration.StubGroupInfos == null || Configuration.StubGroupInfos.Count == 0)
                            {
                                Configuration.StubGroupInfos = aveWeb.ParentSite.GetAllGroups();
                                try
                                {
                                    if (Configuration.StubGroupInfos.Count > 0)
                                    {
                                        var groupLogBuilder = new StringBuilder();
                                        foreach (var groupInfo in Configuration.StubGroupInfos)
                                        {
                                            if (groupLogBuilder.Length > 0)
                                            {
                                                groupLogBuilder.Append("; ");
                                            }

                                            groupLogBuilder.Append($"ID:{groupInfo.ID}, Title:{groupInfo.Title}");
                                        }
                                        mLog.Info($"Configuration.StubGroupInfos loaded groups: {groupLogBuilder}.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLog.Error($"Configuration.StubGroupInfos failed to get groups. List:{entity.FullPath}. Error:{ex}");
                                }
                            }

                            HSMConnector.Add2Queue(new HSMListInfo() { ListObject = aveList.SPList });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Backup List:{entity.FullPath} Error: {e}");
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                throw;
            }
            finally
            {
                if (!entity.IsArchiveBy365)
                {
                    if (current.BackupStatus == FileHeaderStatus.Failed && Configuration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == true)
                    {
                        BriefScanDBOperation.GetInstance(Configuration).InsertFailProcessedNodeToDB(entity);
                    }
                    if (hasBackupHeader)
                    {
                        AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                    }
                    else
                    {
                        mLog.Warn($"Backup List:{entity.FullPath} does not backup header so skip BackupTail.");
                    }
                    Configuration.ProgressDto.UpdateProgress(true);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(entity.FullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                }
                else
                {
                    Configuration.ProgressDto.UpdateProgress(true);
                }
            }
            return 0;
        }

        public string EncodingStringUsingBase64(string content)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(buffer);
        }

    }

    internal class FolderBackup : SPObjectBackup
    {
        public FolderBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            bool mIsRootFolder = false;
            AveSPFolder aveFolder = null;
            HeaderUrl headUrl = new HeaderUrl();
            try
            {
                var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                AveSPWeb aveWeb = null;
                if (CacheSPObjs.ValueInCacheOfLevel(500) != default(object))
                {
                    aveWeb = (CacheSPObjs.ValueInCacheOfLevel(500) as CacheNode).WrapperObject as AveSPWeb;
                }
                else if (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) != default(object))
                {
                    aveWeb = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as CacheNode).WrapperObject as AveSPWeb;
                }
                if (((CacheNode)parent).WrapperObject is AveSPList)
                {
                    mIsRootFolder = true;
                }
                if (entity.NodeType == (int)ArchiverCommon.ItemType.FOLDER_VERSION)
                {
                    AveSender.BackupFolderVerHeader(aveFolder, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headUrl.GetFolderVersionAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, entity.LibRowId, entity.UIVersion));
                    current.FileHeader = AveSender.GenerateHeader(Configuration.GetNodeFullPath(entity.FullPath));
                }
                else
                {
                    if (mIsRootFolder)
                    {
                        aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPList);
                        current.IsRootFolder = true;
                    }
                    else
                    {
                        aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                    }
                    current.WrapperObject = aveFolder;
                    AveSender.BackupFolderHeader(aveFolder, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headUrl.GetUrlBySR(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl));
                    current.FileHeader = AveSender.GenerateHeader(Configuration.GetNodeFullPath(entity.FullPath));
                }
                AveSender.SetHeaderAsArchiveSuccessForEnableDelete(current.FileHeader);

                if (!BriefScanDBOperation.GetInstance(Configuration).NodeIsFailProcessed(entity))
                {
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                else
                {
                    current.BackupStatus = FileHeaderStatus.Failed;
                }
                return (int)JobDetailsStatus.Successful;
            }
            catch (Exception e)
            {
                mLog.Error($"Fail build cache node for folder, ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {

            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            bool mIsRootFolder = false;
            bool hasBackupHeader = false;

            if (entity.LeafName.IndexOf(':') >= 0)
            {
                entity.FullPath = entity.FullPath.Substring(0, entity.FullPath.Length - entity.LeafName.IndexOf(':')) + entity.LeafName;
            }
            mLog.Info($"Begin backup folder.Name:{entity.FullPath}.LibRowId: {entity.LibRowId}.NodeId:{entity.NodeId}.");
            var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
            AveSPWeb aveWeb = null;
            if (CacheSPObjs.ValueInCacheOfLevel(500) != default(object))
            {
                aveWeb = (CacheSPObjs.ValueInCacheOfLevel(500) as CacheNode).WrapperObject as AveSPWeb;
            }
            else if (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) != default(object))
            {
                aveWeb = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as CacheNode).WrapperObject as AveSPWeb;
            }
            if (((CacheNode)parent).WrapperObject is AveSPList)
            {
                mIsRootFolder = true;
            }
            AveSPFolder aveFolder = null;
            HeaderUrl headUrl = new HeaderUrl();
            var tail = new StringBuilder();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.FolderBackup"))
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.FOLDER_VERSION)
                    {
                        FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
                        aveFolder = new AveSPFolder(parent.WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                        try
                        {
                            //add for RevIM export 
                            if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                            {
                                FolderVault folderVault = (FolderVault)VaultExport;
                                folderVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                                folderVault.VaultExport(aveFolder, (int)entity.CacheNodeType, entity.FullPath, subJobId, false, ruleName, mediaName);
                            }
                            AveSender.BackupFolderVerHeader(aveFolder, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headUrl.GetFolderVersionAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, entity.LibRowId, entity.UIVersion));
                            current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                            hasBackupHeader = true;
                            var stream = AveSender.BackupStream;
                            stream.BeginWriteMetadata();
                            try
                            {
                                aveFolder.ExportDocInfo(stream);
                                aveFolder.ExportUserDataInfo(stream);
                                aveFolder.ExportDataJunctionInfo(stream);

                                //Cache the group info of sharing links
                                aveFolder.CachePrincipalFromPermission(4);
                                aveFolder.ExportGroupCache(stream);
                                Dictionary<string, object> fullText = new Dictionary<string, object>();
                                foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                                {
                                    try
                                    {
                                        fullText[tag.Key] = tag.Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                                    }
                                }
                                aveFolder.ExportFullTextIndex(stream, fullText);
                                XmlElement xe = new XmlDocument().CreateElement("Attribute");
                                string delimiter = ((Char)0x12).ToString();
                                xe.InnerText = "TimeZoneID" + delimiter + AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                                tail.Append(xe.OuterXml);
                                xe.InnerText = "Archived By" + delimiter + Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveBy").Value.ToString();
                                tail.Append(xe.OuterXml);
                                xe.InnerText = "Archived" + delimiter + Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveTime").Value.ToString();
                                tail.Append(xe.OuterXml);
                            }
                            catch (Exception e)
                            {
                                mLog.Error($"Backup Folder Version:{entity.FullPath}.Error: {e}.");
                                current.DoDelete = false;
                                throw;
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Backup Folder Version:{entity.FullPath}.Error: {e}.");
                            status = JobDetailsStatus.Failed;
                            current.DoDelete = false;
                            throw;
                        }
                        finally
                        {
                            AveSender.BackupStream.EndWriteMetadata();
                            AveSender.BackupStream.FlushMetadata(0);
                            SetFileExtraInfo(entity.LibRowId, entity.LeafName, entity.LeafName, fileatrrinfo);
                            if (hasBackupHeader)
                            {
                                AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                            }
                            else
                            {
                                mLog.Warn($"Backup FOLDER_VERSION:{entity.FullPath} does not backup header so skip BackupTail.");
                            }

                        }
                    }
                    else
                    {
                        if (mIsRootFolder)
                        {
                            aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPList);
                            current.IsRootFolder = true;
                            current.WrapperObject = aveFolder;
                            return 0;
                        }
                        else
                        {
                            aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                        }
                        current.WrapperObject = aveFolder;
                        if (entity.IsArchiveBy365)
                        {
                            return 0;
                        }
                        try
                        {
                            //add for RevIM 
                            if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                            {
                                FolderVault folderVault = (FolderVault)VaultExport;
                                folderVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                                folderVault.VaultExport(aveFolder, (int)entity.CacheNodeType, entity.FullPath, subJobId, mIsRootFolder, ruleName, mediaName);
                            }

                            AveSender.BackupFolderHeader(aveFolder, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headUrl.GetUrlBySR(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl));
                            current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                            hasBackupHeader = true;
                            var stream = AveSender.BackupStream;
                            stream.BeginWriteMetadata();
                            try
                            {
                                aveFolder.ExportDocInfo(stream);
                                aveFolder.ExportUserDataInfo(stream);
                                aveFolder.ExportDataJunctionInfo(stream);
                                aveFolder.CachePrincipalFromPermission(4);
                                aveFolder.ExportGroupCache(stream);
                                if (aveFolder.AveItem.HasUniqueRoleAssignments)
                                {
                                    mLog.Info($"Current Folder:{entity.FullPath} HasUniqueRoleAssignments.");
                                    AveRoleAssignments roleAssignmetns = AveRoleAssignments.CreateInstance(aveFolder.AveItem);
                                    roleAssignmetns.Export(stream);
                                }
                                Dictionary<string, object> fullText = new Dictionary<string, object>();
                                foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                                {
                                    try
                                    {
                                        fullText[tag.Key] = tag.Value;
                                    }
                                    catch (Exception ex)
                                    {
                                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                                    }
                                }
                                aveFolder.ExportFullTextIndex(stream, fullText);
                                if (aveFolder.AveItem != null)
                                {
                                    XmlElement titleInfo = new XmlDocument().CreateElement("Title");
                                    titleInfo.InnerText = aveFolder.AveItem.Title;
                                    tail.Append(titleInfo.OuterXml);
                                }
                                XmlElement xe = new XmlDocument().CreateElement("Attribute");
                                string delimiter = ((Char)0x12).ToString();
                                xe.InnerText = "TimeZoneID" + delimiter + AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                                tail.Append(xe.OuterXml);
                                xe.InnerText = "Archived By" + delimiter + Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveBy").Value.ToString();
                                tail.Append(xe.OuterXml);
                                xe.InnerText = "Archived" + delimiter + Configuration.tagInfoCollection.FirstOrDefault(tag => tag.Key == "ArchiveTime").Value.ToString();
                                tail.Append(xe.OuterXml);
                                current.BackupStatus = FileHeaderStatus.Complete;
                            }
                            catch (Exception e)
                            {
                                mLog.Error($"Backup Folder:{entity.FullPath} Metadata Error: {e}.");
                                current.DoDelete = false;
                                throw;
                            }
                            finally
                            {
                                stream.EndWriteMetadata();
                                stream.FlushMetadata(0);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Backup Folder:{entity.FullPath} Error: {e}");
                            errorMessage = e.Message.ToString();
                            status = JobDetailsStatus.Failed;
                            current.BackupStatus = FileHeaderStatus.Failed;
                            current.DoDelete = false;
                            throw;
                        }
                        finally
                        {
                            if (hasBackupHeader)
                            {
                                AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                            }
                            else
                            {
                                mLog.Warn($"Backup Folder:{entity.FullPath} does not backup header so skip BackupTail.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Backup Folder or Folder Version:{entity.FullPath}.Error: {e}.");
                errorMessage = e.Message;
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                if (Configuration?.ProgressDto != null)
                {
                    mLog.Warn("[BackupAsync][Exception]Set the HasErrorNode true.");
                    Configuration.ProgressDto.HasErrorNode = true;
                }
                throw;
            }
            finally
            {
                if (!entity.IsArchiveBy365)
                {
                    if (current.BackupStatus == FileHeaderStatus.Failed && Configuration?.ArchiveJobSplitedDBInfo?.IsUseSplitedDB == true)
                    {
                        BriefScanDBOperation.GetInstance(Configuration).InsertFailProcessedNodeToDB(entity);
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(entity.FullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                }
                else
                {
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                }
            }
            return 0;
        }

        private void SetFileExtraInfo(int rowId, string fullName, string displayName, FileAtrributeInfo info)
        {
            try
            {
                int indexColon = fullName.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (indexColon == 0) // Folder version
                {
                    info.ExtraTitle = fullName.Substring(1);
                }
                else
                {
                    info.ExtraTitle = displayName;
                }
            }
            catch (Exception e)
            {
                mLog.Error("Set File Extra Info Error: {0}", e.ToString());
                info.ExtraTitle = fullName; // Set fullname as Title while get an error
            }
            info.ExtraId = rowId.ToString();
        }
    }

    internal class ItemBackup : SPObjectBackup, IMultiBackup
    {
        CallProcess callProcess = new Archiver.CallProcess();
        //JobDetailsStatus status = JobDetailsStatus.Successful;
        private HSMConnector HSMConnector
        {
            get
            {
                if (HSMConnectorInstance == null)
                {
                    HSMConnectorInstance = HSMConnector.GetInstance(Configuration);
                }
                return HSMConnectorInstance;
            }
        }

        //private CGDBReader CGDBReader
        //{
        //    get 
        //    {
        //        if (Configuration.IsCGDBDiscover)
        //        {
        //            CGDBReaderInstance = CGDBReader.GetInstance(Configuration.ArchiverExtendSetting, Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId.ToString(), Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteUrl);
        //        }
        //        return CGDBReaderInstance;
        //    }
        //}

        public ItemBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"ItemBackup.ProcessBackedNode should not reach, item id:{entity?.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            string errorMessage = string.Empty;
            string itemTitle = string.Empty;
            string fullPath = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.ItemBackup"))
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        mLog.Info($"Start item backup {entity.LibRowId}.");
                    }
                    else if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
                    {
                        mLog.Info($"Start item version backup {entity.LibRowId}.");
                    }
                    if (Configuration.IsILMode && IsRecordHold(new Guid(entity.NodeId)))
                    {
                        mLog.Info($"Current file:{entity.NodeId} is on hold in records and current rule is remove rule. Will not process it.");
                        //Configuration.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(entity.NodeId), entity.ArchiveLevel, Configuration.JobId);
                        //Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(entity.FullPath), AveSender.BackupStream.StreamTransfered, JobDetailsStatus.Skipped, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, "StorageOptimization_EXOExploreHoldFile");
                        fullPath = entity.FullPath;
                        errorMessage = "StorageOptimization_EXOExploreHoldFile";
                        status = JobDetailsStatus.Skipped;
                        return 0;
                    }
                    bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(Configuration.currentRule);
                    if (isLinkToDucument)
                    {
                        entity.StubInfo = Configuration.currentRule.LeaveStubType.ToString();
                    }
                    else
                    {
                        entity.StubInfo = "null";
                    }
                    switch (entity.NodeType)
                    {
                        case (int)ArchiverCommon.ItemType.DOCUMENT:
                        case (int)ArchiverCommon.ItemType.DOCUMENT_VER:
                            {
                                BackupDocumentOrDocumentVersion(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender, ref errorMessage, ref status);
                                break;
                            }
                        case (int)ArchiverCommon.ItemType.ITEM_TYPE:
                        case (int)ArchiverCommon.ItemType.ITEM_VERSION:
                            {
                                BackupItemOrItemVersion(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender, ref errorMessage, ref status);
                                break;
                            }
                    }
                    current.BackupStatus = FileHeaderStatus.Complete;
                    try
                    {
                        fullPath = entity.FullPath;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Item Full Path Assignment Error: {0}", e.ToString());
                        fullPath = entity.FullPath;
                    }
                }
            }
            catch (AveWrapperI18NException ex1)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error($"[BackupAsync][AveWrapperI18NException]Backup Item:{fullPath} .Error: {ex1}");
                Configuration.ProgressDto.HasErrorNode = true;
                errorMessage = AveWrapperHandleErrorMessage.GetFormateErrorMessage(ex1.Key, ex1.Message, ex1.Args.ToArray());
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                string defaultValue = string.Format(ex1.Message, ex1.Args);
                Configuration.JobReportDto.AddI18NReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, ex1.Key, defaultValue, ex1.Args.ToArray());
                throw;
            }
            catch (FileContentLengthNullException fe)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error("Backup Item FileContentLengthNullException: {0}", fe.ToString());
                status = JobDetailsStatus.Skipped;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                errorMessage = "RM_JM_Detail_SkipBackup0KBFile";
                Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), 0, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                UpdateCGArchiverStatus(entity, BackupRestoreStatus.Skipped);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error($"[BackupAsync][Exception]Backup Item:{fullPath} .Error: {ex}");
                Configuration.ProgressDto.HasErrorNode = true;
                errorMessage = ex.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                UpdateCGArchiverStatus(entity, BackupRestoreStatus.Failed);
                throw;
            }
            finally
            {
                Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                }
                if (status != JobDetailsStatus.Failed && entity.DoDelete && errorMessage != "RM_JM_Detail_SkipBackup0KBFile")
                {
                    if (!current.IsSkipVersion)
                    {
                        JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, entity.DocumentSize, Configuration.currentRule, status);
                        Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), entity.DocumentSize, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                        //UpdateCGArchiverStatus(entity, BackupRestoreStatus.Succeed);//RECO-24458
                    }
                    else
                    {
                        bool isDecreaseOtherActions = entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT
                            || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE
                            || (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers;
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles(true, true, isDecreaseOtherActions);
                    }
                }
                else if (entity.DoDelete)
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                    else if (JobExecutionProcessStatisticExecutor.Instance.IsRuleSupportVersionAction((CacheNodeType)entity.CacheNodeType, Configuration.currentRule))
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                    if (errorMessage.Contains("Item does not exist. It may have been deleted by another user."))
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles(true, false, false);
                    }
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseArchivedFiles(0);
                }
            }
            return 0;
        }

        private void UpdateCGArchiverStatus(ArchiveApproveReport entity, BackupRestoreStatus status)
        {
            if (Configuration.ArchiverExtendSetting != null && Configuration.ArchiverExtendSetting.IsCGDiscovery)
            {
                try
                {
                    CGDBReader dbReader = CGDBReader.GetInstance(Configuration.ArchiverExtendSetting, Configuration.SiteCollectionID.ToString(), Configuration.SiteCollectionUrl);
                    dbReader?.UpdateStatus(Configuration.SiteCollectionID.ToString(), new Guid(entity.NodeId), status);
                }
                catch (Exception e)
                {
                    mLog.Warn($"update CG archiver status failed,maybe not CG job or something error, error: {e}");
                }
            }
        }
        private void BackupListItem(AveSPListItem aveListItem, BackupInfoSender AveSender)
        {
            var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
            var stream = AveSender.BackupStream;
            stream.BeginWriteMetadata();
            try
            {
                aveListItem.ExportDocInfo(stream);
                aveListItem.ExportUserDataInfo(stream);
                aveListItem.ExportDataJunctionInfo(stream);
                aveListItem.ExportDocVersions(stream);
                //Cache the group info of sharing links
                aveListItem.CachePrincipalFromPermission(4);
                aveListItem.ExportGroupCache(stream);

                Dictionary<string, object> fullText = new Dictionary<string, object>();
                if (!aveListItem.AveSPItem.IsVersion)
                {
                    AveSPAlert alerts = AveSPAlert.CreateInstance(aveListItem);
                    alerts.Export(stream);
                    if (aveListItem.AveSPItem.HasUniqueRoleAssignments)
                    {
                        AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveListItem.AveSPItem);
                        roleAssignments.Export(stream);
                    }
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("Backup.AveSPListItem.ExportWorkflowInstance"))
                    {
                        AveWorkflow workflow = new AveWorkflow();
                        workflow.ExportWorkflowInstance(stream, aveListItem.AveSPItem);
                    }
                }

                foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                {
                    try
                    {
                        fullText[tag.Key] = tag.Value;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                    }
                }
                aveListItem.ExportFullTextIndex(stream, fullText, FullTextIndexLevel.IncludeAllVisiableColumns);
            }
            catch (Exception e)
            {
                mLog.Error("Backup ListItem Error: {0}", e.ToString());
                throw;
            }
            finally
            {
                stream.EndWriteMetadata();
                stream.FlushMetadata(0);
            }
        }

       /* [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        private void AddFullTextColumnForFeedItem(ref Dictionary<string, object> fullText, object o)
        {
            string post = string.Empty;
            string likes = string.Empty;
            string mentions = string.Empty;
            string tags = string.Empty;
            string replyNames = string.Empty;
            fullText["postbydisplayname"] = post;
            fullText["replybydisplayname"] = replyNames;
            fullText["likebydisplayname"] = likes;
            fullText["tags"] = tags;
            fullText["mentionbydisplayname"] = mentions;
            fullText["participantdisplayname"] = post + "#" + replyNames + "#" + likes + "#" + mentions;
            fullText["listbasetemplate"] = "newsfeed";
        }*/

        private void BackupDocument(AveSPDoc aveDoc, BackupInfoSender AveSender, FileAtrributeInfo fileatrrinfo)
        {
            var stream = AveSender.BackupStream;
            stream.BeginWriteMetadata();
            var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
            try
            {
                aveDoc.ExportDocInfo(stream);
                aveDoc.ExportUserDataInfo(stream);
                aveDoc.ExportDataJunctionInfo(stream);
                aveDoc.ExportStorgeInfo(stream);
                aveDoc.ExportDocVersions(stream);
                aveDoc.ExportWebParts(stream);
                //Cache the group info of sharing links
                aveDoc.CachePrincipalFromPermission(4);
                aveDoc.ExportGroupCache(stream);
                if (!aveDoc.AveSPItem.IsVersion)
                {
                    AveSPAlert alerts = AveSPAlert.CreateInstance(aveDoc);
                    alerts.Export(stream);
                    if (aveDoc.AveSPItem.HasUniqueRoleAssignments)
                    {
                        AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveDoc.AveSPItem);
                        roleAssignments.Export(stream);
                    }
                    AveWorkflow workflow = new AveWorkflow();
                    workflow.ExportWorkflowInstance(stream, aveDoc.AveSPItem);
                }
                Dictionary<string, object> fullText = new Dictionary<string, object>();
                foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                {
                    try
                    {
                        fullText[tag.Key] = tag.Value;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                    }
                }
                aveDoc.ExportFullTextIndex(stream, fullText, FullTextIndexLevel.IncludeAllVisiableColumns);

            }
            catch (Exception e)
            {
                mLog.Error("Backup Document Error: {0}", e.ToString());
                throw;
            }
            finally
            {
                stream.EndWriteMetadata();
            }
            try
            {
                if (Configuration.IsCalculateCRC)
                {
                    string fileCRC64 = aveDoc.ExportContentAndCalculateCRC(stream);
                    fileatrrinfo.Crc64 = fileCRC64;
                    //mLog.Info($"Backup document content success.FileId: {fileId}.fileCRC64:{fileCRC64}.");
                }
                else
                {
                    aveDoc.ExportContent(stream);
                }
            }
            catch (Exception e)
            {
                mLog.Info(e.ToString());
                throw;
            }
        }

        private void BackupDocumentOrDocumentVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender AveSender, ref string errorMessage, ref JobDetailsStatus status)
        {
            using (AvePerformanceScope performanceScope = new AvePerformanceScope("ArchiverBackup.BackupDocumentOrDocumentVersion"))
            {
                status = JobDetailsStatus.Successful;
                if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER && entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                {
                    throw new Exception(LOGRESOURCE.StorageOptimization13_SOARBackupImpsItemBackupException);
                }
                string realName = entity.LeafName;
                ThrowUtil.ThrowIfNull(realName, "LeafName is null");
                int index = realName.IndexOf(':');
                if (index >= 0)
                {
                    realName = realName.Substring(0, index);
                }
                AveSPFolder parentFolder = null;
                if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    ThrowUtil.ThrowIfNull(parent, "parent cache node is null");
                    parentFolder = parent.WrapperObject as AveSPFolder;
                }
                else
                {
                    parentFolder = CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder;
                }
                if (entity.ListID != Guid.Empty && parentFolder != null && parentFolder.AveList != null && parentFolder.AveList.Id != entity.ListID)
                {
                    mLog.Info($"Current file:{entity.FullPath} parent list:{entity.ListID} does not same as current backup list:{parentFolder.AveList.Id}.");
                    throw new Exception("The list where the current file is located is inconsistent with the backup list.");
                }
                var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection)?.WrapperObject as AveSPSite;
                ThrowUtil.ThrowIfNull(parentFolder, "parent folder node is null");
                mLog.Info("BackupDocumentOrDocumentVersion.ParentFolderID:{0},ParentFolderUrl:{1}.NodeId:{2}.LibRowId:{3}.UIVersion:{4}.", parentFolder?.Id, parentFolder?.ServerRelativeUrl, entity.NodeId, entity.LibRowId, entity.UIVersion);
                AveSPDoc aveDoc = GetAveSPDoc(entity, parentFolder, realName);
                if (!Configuration.LibraryHasStubHiddenColumn && entity.DocumentSize < 1024 * 10 && IsStubFileType(realName))
                {
                    mLog.Info($"Current file:{entity.FullPath} may OPUS generate stub and should double check stub content.");
                    using (Stream fileStream = aveDoc.GetContent())
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            string fileContent = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(Configuration.ReCenterURL) && fileContent.Contains(Configuration.ReCenterURL))
                            {
                                mLog.Info($"Current file:{entity.FullPath} is OPUS generate stub and skip backup.");
                               status = JobDetailsStatus.Failed;
                                current.DoDelete = false;
                                current.BackupStatus = FileHeaderStatus.Failed;
                                //errorMessage = "StorageOptimization_Skip_OPUSStubFile";
                                return;
                            }
                        }
                    }
                }
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(aveDoc);
                if (aveDoc?.AveSPItem?.ScopeUrl != null && $"{aveDoc.AveSPItem.ScopeUrl.TrimEnd('/')}\\{entity?.LeafName}" != entity?.FullPath?.TrimStart('/'))
                {
                    mLog.Warn($"this file has move to another folder,source:{entity?.FullPath},target:{aveDoc.AveSPItem.ScopeUrl}");
                }
                if (Configuration.currentRule != null && Configuration.currentRule.PolicyLevel == PolicyLevel.Document)
                {
                    if (entity.NodeType != (int)ArchiverCommon.ItemType.DOCUMENT)
                    {
                        if(((Configuration.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveLatestVersion) == (int)KeepDataOption.ArchiveLatestVersion
                            && CheckVersionShouldSkipArchive(aveDoc, entity.UIVersion, Configuration.currentRule.ArchivedLatestVersion)))
                        {
                            if (!SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyVersion)
                            {
                                SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyVersion = true;
                            }
                            mLog.Info($"Skip current version due to ArchivedLatestVersion:{Configuration.currentRule.ArchivedLatestVersion},ParentFolderUrl:{parentFolder.ServerRelativeUrl}.NodeId:{entity.NodeId}.LibRowId:{entity.LibRowId}.UIVersion:{entity.UIVersion},size:{entity.DocumentSize}.");
                            current.IsSkipVersion = true;
                            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSizeForVersion(entity.DocumentSize, Configuration.GetNodeFullPath(entity.FullPath));
                            return;
                        }
                        else if((Configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                            && !CheckShouldArchiveForKeepNumbersOfPreviousVersion(aveDoc, entity.UIVersion))
                        {
                            mLog.Info($"Skip current version due to KeepCurrentVersionAndNumberOfPreviousVersionAndArchiveOthers:{Configuration.currentRule.KeepLatestMajorAndMinorVersionAndArchiveOthers},ParentFolderUrl:{parentFolder.ServerRelativeUrl}.NodeId:{entity.NodeId}.LibRowId:{entity.LibRowId}.UIVersion:{entity.UIVersion}.");
                            current.IsSkipVersion = true;
                            return;
                        }
                        else if((Configuration.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveOnlyLastestVersion) == (int)KeepDataOption.ArchiveOnlyLastestVersion
                            && CheckVersionShouldSkipArchive(aveDoc, entity.UIVersion, Configuration.currentRule.ArchiverOnlyLastestVersion))
                        {
                            mLog.Info($"Skip current version due to ArchiverOnlyLastestVersion:{Configuration.currentRule.ArchiverOnlyLastestVersion},ParentFolderUrl:{parentFolder.ServerRelativeUrl}.NodeId:{entity.NodeId}.LibRowId:{entity.LibRowId}.UIVersion:{entity.UIVersion},size:{entity.DocumentSize}.");
                            current.IsSkipVersion = true;
                            return;
                        }
                    }
                    else 
                    {
                        if((Configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers)
                        {
                            mLog.Info($"Skip latest version due to KeepCurrentVersionAndNumberOfPreviousVersionAndArchiveOthers:{Configuration.currentRule.ArchivedLatestVersion},ParentFolderUrl:{parentFolder.ServerRelativeUrl}.NodeId:{entity.NodeId}.LibRowId:{entity.LibRowId}.UIVersion:{entity.UIVersion}.");
                            current.IsSkipVersion = true;
                            return;
                        }
                    }
                }
                if (Configuration?.currentRule?.KeepDataOption != null &&
                    (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly)
                {
                    current.IsSkipDeleteVersion = true;
                }
                GetComplianceTagIfEnableRemove(aveDoc?.AveSPItem?.SPListItem, out ListItemComplianceInfo complianceInfo);
                if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(aveSite.SPSite, complianceInfo.ComplianceTag))
                {
                    mLog.Info("skip archvie current unlock status item. Item Name: {0}.", aveDoc?.AveSPItem.SPListItem.Name);
                    status = JobDetailsStatus.Skipped;
                    errorMessage = "StorageOptimization_Skip_Unlock_Status_Item";
                    current.IsCurrentVersion = true;
                    return ;
                }
                current.WrapperObject = aveDoc;
                aveDoc.AveSPItem.IsBackupLinkForArchivedData = false;
                HeaderUrl headerUrl = new HeaderUrl();
                if (!entity.DoDelete)//version rule 不需要备份current version，DoDelete为是否符合rule的条件
                {
                    if (ruleLevel == (int)PolicyLevel.DocumentVersion)
                    {
                        current.IsCurrentVersion = true;
                        return;
                    }
                    else
                    {
                        mLog.Warn($"this item DoDelete is false and ruleLevel is {ruleLevel}");
                        return;
                    }
                }
                try
                {
                    //add for RevIM export
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        aveDoc.AveSPItem.UserDataCache = aveDoc.AveSPItem.GetUserData();
                        ItemVault itemVault = (ItemVault)VaultExport;
                        itemVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        itemVault.ExportVaultDocument(aveDoc, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName);
                    }
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                    {
                        AveSender.BackupDocumentHeader(aveDoc, parent, AveSender.BackupStream.StreamTransfered, entity, parentFolder, ruleName, subJobId, mediaName, headerUrl.GetDocumentAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, parentFolder.SPFolder.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType(), Configuration.currentRule.NeedRecordStubId);
                    }
                    else
                    {
                        AveSender.BackupDocumentVersionHeader(aveDoc, parent, AveSender.BackupStream.StreamTransfered, entity, parentFolder, ruleName, subJobId, mediaName, headerUrl.GetDocumentVersionAP(parentFolder.SPFolder.ParentWeb.Site.Url, parentFolder.SPFolder.ParentWeb.Site.ServerRelativeUrl, entity.UIVersion, parentFolder.SPFolder.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType());
                    }
                    current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                    BackupDocument(aveDoc, AveSender, fileatrrinfo);
                    if (!aveDoc.AveSPItem.IsSystemFileOrFolder)
                    {
                        SetItemAttributes(aveDoc.AveSPItem, aveDoc.AveSPItem.GetAllColumnValues(ColumnsLevel.DisplayColumns), aveDoc.AveSPItem.Item.File.Title, entity, fileatrrinfo, Configuration.tagInfoCollection);
                    }
                    else if (index < 0)
                    {
                        fileatrrinfo.IsSystemFile = true;
                    }

                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT && aveDoc != null && aveDoc.AveSPItem != null && aveDoc.AveSPItem.DocumentSize == 0)
                    {
                        mLog.Warn($"Current File Content Length is 0. path {entity.FullPath}");
                        if (Configuration.Skip0KBFile)
                        {
                            throw new FileContentLengthNullException("File Content Length is 0");
                        }
                    }

                    if (Configuration.IsILMode && entity.CacheNodeType == (int)CacheNodeType.Item)
                    {
                        if (entity.DeleteRelatedRecords == 1 && !Configuration.CheckItemIsRecordsHold(new Guid(entity.NodeId)))
                        {
                            if (current.FileHeader != null)
                            {
                                //Set secend header's job id = string.Empty first
                                //先把当前Header 的EndUser Job ID 置空，防止上一个文件的Job ID 延续到本次Job，导致这个document 备份失败，Header 中仍然存在上次Job 的EndUser Job ID
                                current.FileHeader.SetAttribute(KeyWord.RelativeDataJobId, string.Empty);
                            }
                            string recordRelatedValue = string.Empty;
                            //system aspx file aveDoc.AveSPItem.SPListItem is null.
                            if (aveDoc.AveSPItem != null && aveDoc.AveSPItem.SPListItem != null && aveDoc.AveSPItem.SPListItem.Fields.ContainsFieldWithStaticName("RecordsRelated"))
                            {
                                var metadata = aveDoc.AveSPItem.SPListItem["RecordsRelated"];
                                if (metadata != null && !string.IsNullOrEmpty(metadata.ToString()))
                                {
                                    recordRelatedValue = metadata.ToString();
                                    mLog.Info("start archive the related items.Path:{0}.RelatedInfo:{1}.", entity.NodeId, recordRelatedValue);
                                    string jobIds = DisposalRelatedItemUtility.DisposeRelatedItemsForArchiveAndRemove(callProcess, Configuration, Configuration.currentRule, recordRelatedValue, SendJobDetail);
                                    //Add Job id to header
                                    if (current.FileHeader != null && !string.IsNullOrEmpty(jobIds))
                                    {
                                        current.FileHeader.SetAttribute(KeyWord.RelativeDataJobId, jobIds);
                                    }
                                }
                                else
                                {
                                    mLog.Info("Related column value is null.");
                                }
                            }
                            else
                            {
                                mLog.Info("Current document does not have related column.");
                            }
                        }
                        else
                        {
                            mLog.Info("Skip delete related records.DeleteRelatedRecords:{0}.", entity.DeleteRelatedRecords);
                        }
                    }

                    if (HSMConnector.IsStart && entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                    {
                        var list = aveDoc.ParentFolder.AveList.SPList;
                        if (list.BaseTemplate == AveListTemplateType.DesignCatalog
                            || list.BaseTemplate == AveListTemplateType.MasterPageCatalog
                            || list.BaseTemplate == AveListTemplateType.WebPageLibrary
                            || list.BaseTemplate == AveListTemplateType.ThemeCatalog
                            || list.Hidden || list.IsCatalog)
                        {
                            mLog.Info($"Skip system file for HSM Stub {entity.NodeId}");
                            return;
                        }
                        using (AvePerformanceScope avePerformanceScope = new AvePerformanceScope("ArchiverBackup.AddDocument2Queue"))
                        {
                            HSMFileInfo stubFileInfo = new HSMFileInfo();
                            stubFileInfo.FileObject = aveDoc.AveSPItem.Item.File;

                            var metadata = new AveSPDocumentMetadataDto();

                            if (aveDoc.AveSPItem.RowId > 0)
                            {
                                metadata.UserDataInfo = aveDoc.AveSPItem.GetUserData();
                                metadata.ItemTPGUIDofLookupValue = aveDoc.AveSPItem.GetLookupFieldGuidValue();
                            }
                            aveDoc.AveSPItem.CachePrincipalFromDatajunction();
                            metadata.DocDataJunction = aveDoc.AveSPItem.GetUserDataJunction();
                            metadata.DocInfo_Old = aveDoc.AveSPItem.GetDocInfo();
                            stubFileInfo.MetadataDto = metadata;
                            if (aveDoc.AveSPItem.HasUniqueRoleAssignments)
                            {
                                stubFileInfo.RoleAssignment = AveRoleAssignments.CreateInstance(aveDoc.AveSPItem).GetRoleAssignments();
                            }
                            stubFileInfo.PathMD5 = LinkFileCommon.GetDocumnetPathMD5(aveSite.SPSite.Url, parentFolder.Path, realName);
                            stubFileInfo.StubId = entity.StubId;
                            stubFileInfo.FileServerRelatedUrl = entity.FullPath.Replace("\\","/");
                            HSMConnector.Add2Queue(stubFileInfo);
                        }
                    }
                }
                catch (FileContentLengthNullException)
                {
                    status = JobDetailsStatus.Skipped;
                    throw;
                }
                catch (Exception e)
                {
                    status = JobDetailsStatus.Failed;
                    mLog.Error($"[BackupDocumentOrDocumentVersion][Exception]Backup Document or Document Version,Path{entity.FullPath} Error: {e.ToString()}");
                    Configuration.ProgressDto.HasErrorNode = true;
                    current.DoDelete = false;
                    if (entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
                    {
                        Configuration.FailedVersionFileIds.Add(entity.ParentId);
                    }
                    //if (entity.NodeType == (int)ItemType.DOCUMENT && Configuration.IsCGDBDiscover && CGDBReader != null)
                    //{
                    //    CGDBReader.UpdateStatus(Configuration.archiverMessage.ScheduledConfigs.FirstOrDefault().SiteId, new Guid(entity.NodeId), JobDetailsStatus.Failed);
                    //}
                    throw;
                }
                finally
                {
                    string tail = fileatrrinfo.ToString();
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupDocumentOrDocumentVersionInfo, entity.NodeId);
                    AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
                }
            }
        }

        private bool IsStubFileType(string fileName)
        {
            List<string> stubTypes = new List<string>() { ".aspx",".txt",".html",".url"};
            foreach (string stubType in stubTypes)
            {
                if (fileName.EndsWith(stubType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckShouldArchiveForKeepNumbersOfPreviousVersion(AveSPDoc aveDoc, int currentUIVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckCurrentVersionShouldDelete"))
            {
                IAveFile file = aveDoc.AveSPItem.Item.File;
                int versionSequenceNo = 0;
                int majorVersionSequenceNo = 0;
                int minorOfMajorSequenceNo = 0;
                bool isMajorVersion = false;
                bool isLastMajorVersion = false;
                //Current Version不处理.
                //Current Version/ Publish Major Version，IsCurrentVersion都是true.
                bool isCurrentVersion = false;
                if ((currentUIVersion / 512) == (file.Versions[0].ID / 512))
                {
                    isLastMajorVersion = true;
                }
                IAveFileVersion version = null;
                foreach (IAveFileVersion tmpVersion in file.Versions.OrderByDescending(v => v.ID))
                {
                    isMajorVersion = tmpVersion.ID % 512 == 0;
                    if (tmpVersion.ID == currentUIVersion)
                    {
                        version = tmpVersion;
                        isCurrentVersion = tmpVersion.IsCurrentVersion;
                        break;
                    }
                    if (tmpVersion.IsCurrentVersion)
                    {
                        continue;
                    }
                    if ((tmpVersion.ID / 512 == currentUIVersion / 512) && !isMajorVersion)
                    {
                        ++minorOfMajorSequenceNo;
                    }
                    ++versionSequenceNo;
                    majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                }
                if (version == null)
                {
                    mLog.Info($"Version is null when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                    return false;
                }
                else if (isCurrentVersion)
                {
                    mLog.Info($"Version isCurrentVersion when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                    return false;
                }
                //case PolicyCondition.MajorAndMintorVersions:
                int leaveLastVersionCount = Configuration.currentRule.KeepLatestMajorAndMinorVersionAndArchiveOthers;
                return versionSequenceNo >= leaveLastVersionCount;
            }
        }

        private bool CheckVersionShouldSkipArchive(AveSPDoc aveDoc, int currentUIVersion, int shouldKeepVersionCount)
        {
            return CheckVersionShouldSkipArchive(aveDoc.AveSPItem.Item.File.Versions.Select(ver => (ver.ID, ver.IsCurrentVersion)),currentUIVersion, shouldKeepVersionCount);
        }

        private bool CheckVersionShouldSkipArchive(AveSPListItem aveItem, int currentUIVersion, int shouldKeepVersionCount)
        {
           
            return CheckVersionShouldSkipArchive(aveItem.AveSPItem.Item.File.Versions.Select(ver => (ver.ID, ver.IsCurrentVersion)), currentUIVersion, shouldKeepVersionCount);
        }

        private bool CheckVersionShouldSkipArchive(IEnumerable<(int versionId, bool isCurrentVersion)> versions, int currentUIVersion, int shouldKeepVersionCount)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.CheckVersionShouldSkipArchive"))
            {
                int versionSequenceNo = 0;
                int majorVersionSequenceNo = 0;
                int minorOfMajorSequenceNo = 0;
                bool isMajorVersion = false;
                //Current Version不处理.
                //Current Version/ Publish Major Version，IsCurrentVersion都是true.
                foreach ((int versionId, bool isCurrentVersion) tmpVersion in versions.OrderByDescending(v => v.versionId))
                {
                    isMajorVersion = tmpVersion.versionId % 512 == 0;
                    if (tmpVersion.versionId == currentUIVersion)
                    {
                        if (tmpVersion.isCurrentVersion)
                        {
                            mLog.Info($"Version isCurrentVersion when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                            return false;
                        }
                        return versionSequenceNo >= shouldKeepVersionCount;
                    }
                    if (tmpVersion.isCurrentVersion)
                    {
                        continue;
                    }
                    if ((tmpVersion.versionId / 512 == currentUIVersion / 512) && !isMajorVersion)
                    {
                        ++minorOfMajorSequenceNo;
                    }
                    ++versionSequenceNo;
                    majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                }
                mLog.Info($"Version is null when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                return false;
            }
        }
        private AveSPDoc GetAveSPDoc(ArchiveApproveReport entity, AveSPFolder parentFolder, string realName) 
        {
            using (AvePerformanceScope avePerformanceScope = new AvePerformanceScope("ArchiverBackup.GetAveSPDoc"))
            {
                AveSPDoc aveDoc = null;
                try
                {
                    //Document / Document Version Level Rule，Discover组成的ArchiveApproveReport对象不包含IAveListItem，会走到此判断.
                    //CG Discover，Current 需要check rule，导致实例化了IAveListItem对象，但是Version并没有实例化，会导致Version和Current同时多线程处理时出现异常.因此CG Discover全部走#1.
                    //if (entity.ItemObject == null || Configuration.IsCGDBDiscover)
                    if (entity.ItemObject == null)
                    {
                        if (Configuration.CheckBackupItemCacheExist(entity.NodeId))
                        {
                            //other thread already init IAveListItem.
                            if (Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem != null)
                            {
                                mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} use other thread init IAveListItem.");
                                aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem);
                            }
                            else
                            {
                                //wait other thread init IAveListItem.
                                if (CheckOtherThreadHasInitIAveListItem(entity.NodeId, entity.UIVersion))
                                {
                                    //other thread init success.
                                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem);
                                }
                                else
                                {
                                    //other thread init failed and force backup.
                                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                                }
                            }
                        }
                        else
                        {
                            mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} is responsible for init IAveListItem.");
                            aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                            Configuration.UpdateBackupItemCache(entity.NodeId, aveDoc.AveSPItem.SPListItem);
                        }
                    }
                    //Container级别Rule，Discover组成的ArchiveApproveReport对象包含IAveListItem，默认会走到此判断.
                    else
                    {
                        mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} use ArchiveApproveReport IAveListItem.");
                        aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, entity.ItemObject);
                    }
                }
                catch (Exception ex)
                {
                    //添加兜底逻辑
                    mLog.Info($"GetAveSPDoc failed.NodeId:{entity.NodeId} version:{entity.UIVersion} use ArchiveApproveReport IAveListItem.");
                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                }
                return aveDoc;
            }
        }

        private bool CheckOtherThreadHasInitIAveListItem(string nodeId, int version)
        {
            bool hasInitIAveListItem = false;
            int checkTimes = 0;
            do
            {
                checkTimes++;
                if (Configuration.CheckOtherThreadHasInitIAveListItem(nodeId))
                {
                    mLog.Info($"Current IAveListItem:{nodeId} version:{version} has init by other thread.");
                    hasInitIAveListItem = true;
                    break;
                }
                else
                {
                    mLog.Warn($"Current IAveListItem:{nodeId} version:{version} begin sleep 2s and wait document init.");
                    Thread.Sleep(2 * 1000);
                    if (checkTimes >= 10)
                    {
                        mLog.Warn($"Current IAveListItem:{nodeId} version:{version} sleep over 5 times and begin force backup.");
                        break;
                    }
                }
            } while (true);
            return hasInitIAveListItem;
        }

        private bool IsRecordHold(Guid docId)
        {
            bool isHold = false;
            try
            {
                if (IsRemoveRule(Configuration.currentRule) && Configuration.CheckItemIsRecordsHold(docId))
                {
                    isHold = true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Error occurred while checking item is on hold in records. Id:{docId} Error:{e.ToString()}");
            }
            return isHold;
        }

        public int GetArchiveLevel(ArchiveApproveReport reportNode)
        {
            int ArchiveLevel = -1;
            Rule rule = Configuration.currentRule;
            switch (rule.PolicyLevel)
            {
                case GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection:
                    ArchiveLevel = (int)SPNodeLevel.SiteCollection;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Site:
                    ArchiveLevel = (int)SPNodeLevel.Web;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Library:
                case GCommon.Contract.CommonFilter.PolicyLevel.List:
                    ArchiveLevel = (int)SPNodeLevel.List;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Folder:
                    ArchiveLevel = (int)SPNodeLevel.Folder;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Item:
                    //如果节点级别是ItemVersion 6 或者Attachment 6 ，并且符合了Item rule 表示符合parent rule
                    ArchiveLevel = (reportNode.NodeType == 5 || reportNode.NodeType == 6) ? (int)SPNodeLevel.FitParentRule : (int)SPNodeLevel.Item;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Newsfeed:
                    ArchiveLevel = (int)SPNodeLevel.Item;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.ItemVersion:
                    ArchiveLevel = (int)SPNodeLevel.ItemVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Document:
                    //如果节点级别是Document version 2 ，并且符合了Document rule ，表示符合parent rule
                    ArchiveLevel = reportNode.NodeType == 2 ? (int)SPNodeLevel.FitParentRule : (int)SPNodeLevel.Document;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion:
                    ArchiveLevel = (int)SPNodeLevel.DocumentVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Attachment:
                    ArchiveLevel = (int)SPNodeLevel.Attachment;
                    break;
                default:
                    break;
            }

            return ArchiveLevel;
        }

        private bool IsRemoveRule(Rule result)
        {
            bool isRemoveRule = false;
            //mConfiguration.currentRule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
            if (result.ExportInfo != null && result.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                isRemoveRule = false;
            }
            else if (result.MoveToRecordCenterAndDelareSetting != null && result.MoveToRecordCenterAndDelareSetting.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare)
            {
                isRemoveRule = false;
            }
            //mConfiguration.BackupRequest.Rules.ContainsKey(mConfiguration.currentRule.Id) && mConfiguration.BackupRequest.Rules[mConfiguration.currentRule.Id].ExportType != ExportTypeValue.Autonomy
            else if (
                (result.KeepDataOption == (int)KeepDataOption.Delete
                || (result.KeepDataOption & (int)KeepDataOption.LinkDocument)== (int)KeepDataOption.LinkDocument
                || (result.KeepDataOption & (int)KeepDataOption.Remove)== (int)KeepDataOption.Remove)
                )
            {
                isRemoveRule = true;
            }
            //Backup only doesn't support Export.
            else if (result.KeepDataOption == (int)KeepDataOption.Keep)
            {
                isRemoveRule = false;
            }
            //Only DeleteOnly and ExportBeforeDeleteOnly.
            else if ((result.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
            {
                isRemoveRule = true;
            }
            //当前逻辑的前提是Archiver不支持ExportBeforeArchiverAndKeepData，如果支持，需要修改此处判断.
            else if ((result.KeepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep)
            {
                //Records Keep Data Default use KeepDataOnly.RECO-5524
                if (Configuration.IsILMode)
                {
                    isRemoveRule = false;
                }
                else
                {
                    isRemoveRule = true;
                }
            }
            else if ((result.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                || (result.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub)
            {
                isRemoveRule = true;
            }
            else if ((result.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                || (result.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub)
            {
                isRemoveRule = true;
            }

            return isRemoveRule;
        }
        private void BackupItemOrItemVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender AveSender, ref string errorMessage, ref JobDetailsStatus status)
        {
            status = JobDetailsStatus.Successful;
            AveSPFolder parentFolder = null;
            if (entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
            {
                parentFolder = parent.WrapperObject as AveSPFolder;
            }
            else
            {
                parentFolder = CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder;
            }
            var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
            HeaderUrl headerUrl = new HeaderUrl();
            mLog.Info("BackupItemOrItemVersion.ParentFolderID:{0},ParentFolderUrl:{1}.NodeId:{2}.LibRowId:{3}.UIVersion:{4}.ItemUrl:{5}.", parentFolder.Id, parentFolder.ServerRelativeUrl, entity.NodeId, entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + entity.LeafName);
            var aveListItem = new AveSPListItem(parentFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
            current.WrapperObject = aveListItem;
            if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.ItemVersion)//version rule 不需要备份current version
            {
                current.IsCurrentVersion = true;
                return;
            }
            if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.Attachment)
            { //判断是什么rule,attachement rule 不备份item
                current.IsCurrentVersion = true;
                return;
            }
            if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.Item)
            {
                current.IsCurrentVersion = true;
                return;
            }
            //User Information List中SPListItem为空，User所对应的Item不存在，不需要备份
            if (aveListItem.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.UserInformation && aveListItem.AveSPItem.SPListItem == null)
            {
                mLog.Warn("This user does not exist, User : {0}", aveListItem.AveSPItem.Title);
                return;
            }
            GetComplianceTagIfEnableRemove(aveListItem.AveSPItem.SPListItem, out ListItemComplianceInfo complianceInfo);
            if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(aveSite.SPSite, complianceInfo.ComplianceTag))
            {
                mLog.Info("skip archive current unlock status item. Item Name: {0}.", aveListItem.AveSPItem.SPListItem.Name);
                status = JobDetailsStatus.Skipped;
                errorMessage = "StorageOptimization_Skip_Unlock_Status_Item";
                current.IsCurrentVersion = true;
                return;
            }
            if (Configuration?.currentRule?.KeepDataOption != null &&
                (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly)
            {
                current.IsSkipDeleteVersion = true;
            }
            try
            {
                if (entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                {
                    if (!string.IsNullOrEmpty(aveListItem.AveSPItem.Title))
                    {
                        entity.FullPath = entity.FullPath.Substring(0, entity.FullPath.IndexOf('\\') + 1) + aveListItem.AveSPItem.Title;
                    }
                    AveSender.BackupItemHeader(aveListItem, parent, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headerUrl.GetListItemAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, parentFolder.SPFolder.ServerRelativeUrl, parentFolder.AveList.Id, entity.LibRowId));
                }
                else
                {
                    AveSPListItem currentItem = (AveSPListItem)current.WrapperObject;
                    string itemTitle = currentItem.AveSPItem.Title;
                    AveSender.BackupItemVersionHeader(aveListItem, parent, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, headerUrl.GetListItemVersionAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, parentFolder.SPFolder.ServerRelativeUrl, entity.LibRowId, entity.UIVersion));
                }
                //add for RevIM export
                if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                {
                    ItemVault itemVault = (ItemVault)VaultExport;
                    itemVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                    itemVault.VaultExportItem(aveListItem, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName);
                }

                current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                BackupListItem(aveListItem, AveSender);
                if (!aveListItem.AveSPItem.IsSystemFileOrFolder)
                {
                    SetItemAttributes(aveListItem.AveSPItem, aveListItem.AveSPItem.GetAllColumnValues(ColumnsLevel.DisplayColumns), aveListItem.AveSPItem.SPListItem.Title, entity, fileatrrinfo, Configuration.tagInfoCollection);   //SAAS-10847 使用正确的ListItem的Tile （原来为aveListItem.AveSPItem.Title）
                }
                if (Configuration.IsILMode && entity.CacheNodeType == (int)CacheNodeType.Item)
                {
                    if (entity.DeleteRelatedRecords == 1 && !Configuration.CheckItemIsRecordsHold(new Guid(entity.NodeId)))
                    {
                        if (current.FileHeader != null)
                        {
                            //Set secend header's job id = string.Empty first
                            //先把当前Header 的EndUser Job ID 置空，防止上一个文件的Job ID 延续到本次Job，导致这个document 备份失败，Header 中仍然存在上次Job 的EndUser Job ID
                            current.FileHeader.SetAttribute(KeyWord.RelativeDataJobId, string.Empty);
                        }

                        string recordRelatedValue = string.Empty;
                        if (aveListItem.AveSPItem.SPListItem.Fields.ContainsFieldWithStaticName("RecordsRelated"))
                        {
                            var metadata = aveListItem.AveSPItem.SPListItem["RecordsRelated"];
                            if (metadata != null && !string.IsNullOrEmpty(metadata.ToString()))
                            {
                                recordRelatedValue = metadata.ToString();
                                mLog.Info("start archive the related items.Path:{0}.RelatedInfo:{1}.", entity.FullPath, recordRelatedValue);
                                string jobIds = DisposalRelatedItemUtility.DisposeRelatedItemsForArchiveAndRemove(callProcess, Configuration, Configuration.currentRule, recordRelatedValue, SendJobDetail);
                                //Add Job id to header
                                if (current.FileHeader != null && !string.IsNullOrEmpty(jobIds))
                                {
                                    current.FileHeader.SetAttribute(KeyWord.RelativeDataJobId, jobIds);
                                }
                            }
                            else
                            {
                                mLog.Info("Related column value is null.");
                            }
                        }
                        else
                        {
                            mLog.Info("Item does not have related column.");
                        }
                    }
                    else
                    {
                        mLog.Info("Skip delete related records.DeleteRelatedRecords:{0}.", entity.DeleteRelatedRecords);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("[BackupItemOrItemVersion][Exception]Backup Item or Item Version Error: {0}", e.ToString());
                status = JobDetailsStatus.Failed;
                Configuration.ProgressDto.HasErrorNode = true;
                current.DoDelete = false;
                if (entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
                {
                    Configuration.FailedVersionFileIds.Add(entity.ParentId);
                }
                throw;
            }
            finally
            {
                string tail = fileatrrinfo.ToString();
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverBeforeTail, tail);
                AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
            }
        }

        public void SendJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            Configuration.ProgressDto.HasCompleteNode = status == JobDetailsStatus.Successful;
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(status, "", "PhysicalDelete", ConvertToSPCacheNodeType(int.Parse(cacheNodeType)));
            Configuration.JobReportDto.AddDeletionReport(originPath, 0, status, ConvertToSPCacheNodeType(int.Parse(cacheNodeType)), "", "", "", "PhysicalDelete", comment);
        }

        public int ConvertToSPCacheNodeType(int cacheNodeType)
        {
            int spCacheNodeType = (int)CacheNodeType.Item;
            switch (cacheNodeType)
            {
                case (int)RMNodeLevel.PhysicalFile:
                    spCacheNodeType = (int)CacheNodeType.Folder;
                    break;
                case (int)RMNodeLevel.PhysicalRecord:
                    spCacheNodeType = (int)CacheNodeType.Item;
                    break;
                default:
                    break;
            }
            return spCacheNodeType;
        }
    }
    internal class ArchiveBy365ItemBackup : SPObjectBackup, IMultiBackup
    {
        CallProcess callProcess = new Archiver.CallProcess();
        private AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private RMGraphTenantManager mGraphManager;
        public RMGraphTenantManager GraphManager
        {
            get
            {
                if(mGraphManager == null)
                {
                    mGraphManager = new RMGraphTenantManager(Configuration.O365TenantId);
                }
                return mGraphManager;
            }
        }

        public ArchiveBy365ItemBackup(AveLogger log,string O365TenantId)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"ArchiveBy365ItemBackup.ProcessBackedNode should not reach, item id:{entity?.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            string errorMessage = string.Empty;
            string itemTitle = string.Empty;
            string fullPath = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.ItemBackup"))
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        mLog.Info($"Start archiver by 365 item backup {entity.LibRowId}.");
                    }
                    else
                    {
                        mLog.Warn($"Start archiver by 365 item backup {entity.LibRowId}.but the nodeType is wrong,type:{entity.NodeType}");
                    }
                    switch (entity.NodeType)
                    {
                        case (int)ArchiverCommon.ItemType.DOCUMENT:
                        case (int)ArchiverCommon.ItemType.DOCUMENT_VER:
                            {
                                var result = await ArchiveBy365Async(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender);
                                if (result != null)
                                {
                                    errorMessage = result.Item1;
                                    status = result.Item2;
                                }
                                else
                                {
                                    status = JobDetailsStatus.Failed;
                                }
                                break;
                            }
                    }
                    current.BackupStatus = FileHeaderStatus.Complete;
                    try
                    {
                        fullPath = entity.FullPath;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("archive by 365 Item Full Path Assignment Error: {0}", e.ToString());
                        fullPath = entity.FullPath;
                    }
                }
            }
            catch (AveWrapperI18NException ex1)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error($"[BackupAsync][AveWrapperI18NException]archiver by 365 Backup Item:{fullPath} .Error: {ex1}");
                Configuration.ProgressDto.HasErrorNode = true;
                errorMessage = AveWrapperHandleErrorMessage.GetFormateErrorMessage(ex1.Key, ex1.Message, ex1.Args.ToArray());
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                string defaultValue = string.Format(ex1.Message, ex1.Args);
                Configuration.JobReportDto.AddI18NReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, ex1.Key, defaultValue, ex1.Args.ToArray());
                throw;
            }
            catch (FileContentLengthNullException fe)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error("archive by 365 Backup Item FileContentLengthNullException: {0}", fe.ToString());
                status = JobDetailsStatus.Skipped;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                errorMessage = "RM_JM_Detail_SkipBackup0KBFile";
                Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), 0, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                UpdateCGArchiverStatus(entity, BackupRestoreStatus.Skipped);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                mLog.Error($"[BackupAsync][Exception]archive by 365 Backup Item:{fullPath} .Error: {ex}");
                Configuration.ProgressDto.HasErrorNode = true;
                errorMessage = ex.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                Configuration.JobReportDto.M365ArchiveAddReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                UpdateCGArchiverStatus(entity, BackupRestoreStatus.Failed);
                throw;
            }
            finally
            {
                Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                }
                if (status != JobDetailsStatus.Failed && entity.DoDelete && errorMessage != "RM_JM_Detail_SkipBackup0KBFile")
                {
                    if (!current.IsSkipVersion)
                    {
                        JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, entity.DocumentSize, Configuration.currentRule, status);
                        Configuration.JobReportDto.M365ArchiveAddReport(Configuration.GetNodeFullPath(fullPath), entity.DocumentSize, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSum += entity.DocumentSize;
                        //UpdateCGArchiverStatus(entity, BackupRestoreStatus.Succeed);//RECO-24458
                    }
                    else
                    {
                        bool isDecreaseOtherActions = entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT
                            || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE
                            || (Configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers;
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles(true, true, isDecreaseOtherActions);
                    }
                }
                else if (entity.DoDelete)
                {
                    // Archive M365 uses other actions
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
                }
            }
            return 0;
        }
        private void UpdateCGArchiverStatus(ArchiveApproveReport entity, BackupRestoreStatus status)
        {
            if (Configuration.ArchiverExtendSetting != null && Configuration.ArchiverExtendSetting.IsCGDiscovery)
            {
                try
                {
                    CGDBReader dbReader = CGDBReader.GetInstance(Configuration.ArchiverExtendSetting, Configuration.SiteCollectionID.ToString(), Configuration.SiteCollectionUrl);
                    dbReader?.UpdateStatus(Configuration.SiteCollectionID.ToString(), new Guid(entity.NodeId), status);
                }
                catch (Exception e)
                {
                    mLog.Warn($"update CG archiver status failed,maybe not CG job or something error, error: {e}");
                }
            }
        }


        private async Task<Tuple<string, JobDetailsStatus>> BackupDocumentBy365Async(AveSPDoc aveDoc, ArchiveApproveReport entity)
        {
            try
            {
                var siteId = GetGraphSiteId(aveDoc);
                var listId = aveDoc.AveSPItem.ListId.ToString();
                var rowId = aveDoc.AveSPItem.RowId;
                var isArchived = await GetFileArchiveStatusAsync(siteId, listId, rowId);
                if (isArchived)
                {
                    RemoveArchivedItemFromExplorer(aveDoc);
                    mLog.Info($"this item has archive by 365 Backup Document: {rowId}");
                    return new Tuple<string, JobDetailsStatus>("RM_ArchiveBy365_Detail_Skip", JobDetailsStatus.Skipped);
                }
                else
                {
                    await SetToArchiveStatusAsync(siteId, listId, rowId);
                    UpdateCGArchiverStatus(entity, BackupRestoreStatus.Succeed);
                    RemoveArchivedItemFromExplorer(aveDoc);
                    return new Tuple<string, JobDetailsStatus>("", JobDetailsStatus.Successful);
                }
            }
            catch (Exception e)
            {
                mLog.Error("archive by 365 Backup Document Error: {0}", e.ToString());
                throw;
            }
        }
        private string GetGraphSiteId(AveSPDoc aveDoc)
        {
            var spWeb = aveDoc.AveSPItem.SPListItem?.ParentList?.ParentWeb;
            if (spWeb == null)
            {
                mLog.Warn("archive by 365 GetGraphSiteId: spWeb is null, fallback to SiteId");
                return aveDoc.AveSPItem.SiteId.ToString();
            }

            var hostname = new Uri(spWeb.Url).Host;
            var siteCollectionId = aveDoc.AveSPItem.SiteId.ToString();
            var webId = spWeb.ID.ToString();
            var graphSiteId = $"{hostname},{siteCollectionId},{webId}";

            mLog.Info($"archive by 365 GetGraphSiteId: {graphSiteId}");
            return graphSiteId;
        }
        public async Task<bool> GetFileArchiveStatusAsync(string siteId, string listId, int rowId)
        {
            var statusResult = await GraphManager.GetItemArchiveStatusAsync(siteId, listId, rowId);
            if (statusResult != null && !string.IsNullOrEmpty(statusResult.Fields.FileArchiveStatus) && !string.IsNullOrEmpty(statusResult.Fields.FileArchiveStatus))
            {
                return true;
            }
            return false;
        }
        public async Task SetToArchiveStatusAsync(string siteId, string listId, int rowId)
        {
            await Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryTimes => TimeSpan.FromSeconds(retryTimes * 5))
                .ExecuteAsync(async () => await GraphManager.SetItemToArchiveStatusAsync(siteId, listId, rowId));
        }

        private void RemoveArchivedItemFromExplorer(AveSPDoc aveDoc)
        {
            try
            {
                if (Configuration.IsILMode)
                {
                    var siteId = aveDoc.AveSPItem.SiteId;
                    var objectId = aveDoc.AveSPItem.Id;
                    var recordId = IDGenerator.GetRecordId(siteId, objectId);
                    var recordInDb = explorerDao.ReadById(siteId, recordId);
                    if (recordInDb == null)
                    {
                        mLog.Warn($"Cannot find explorer record for archived M365 item. SiteId:{siteId}, ObjectId:{objectId}, RowId:{aveDoc.AveSPItem.RowId}");
                        return;
                    }

                    if (recordInDb.RecordStatus != (int)AvePoint.RA.Contract.Explorer.RMRecordStatus.Active)
                    {
                        mLog.Info($"Skip explorer removal for archived M365 item because record status is {recordInDb.RecordStatus}. SiteId:{siteId}, ObjectId:{objectId}, RowId:{aveDoc.AveSPItem.RowId}");
                        return;
                    }

                    explorerDao.UpdateRecordState(recordInDb, (int)AvePoint.RA.Contract.Explorer.RMRecordStatus.RMDeleted);
                    mLog.Info($"Marked archived M365 item as logically deleted in explorer. SiteId:{siteId}, ObjectId:{objectId}, RowId:{aveDoc.AveSPItem.RowId}");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to remove archived M365 item from explorer. Error:{ex}");
            }
        }

        private async Task<Tuple<string, JobDetailsStatus>> ArchiveBy365Async(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            using (AvePerformanceScope performanceScope = new AvePerformanceScope("ArchiverBackup.BackupDocumentOrDocumentVersion"))
            {
                var status = JobDetailsStatus.Successful;
                var errorMessage = "";
                string realName = entity.LeafName;
                int index = realName.IndexOf(':');
                if (index >= 0)
                {
                    realName = realName.Substring(0, index);
                }
                AveSPFolder parentFolder = null;
                if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    parentFolder = parent.WrapperObject as AveSPFolder;
                }
                else
                {
                    parentFolder = CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder;
                }
                ThrowUtil.ThrowIfNull(parentFolder, "parentFolder is null");
                if (entity.ListID != Guid.Empty && parentFolder != null && parentFolder.AveList != null && parentFolder.AveList.Id != entity.ListID)
                {
                    mLog.Info($"archive by 365 Current file:{entity.FullPath} parent list:{entity.ListID} does not same as current backup list:{parentFolder.AveList.Id}.");
                    throw new Exception("archive by 365 The list where the current file is located is inconsistent with the backup list.");
                }
                var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
                mLog.Info("archive by 365.ParentFolderID:{0},ParentFolderUrl:{1}.NodeId:{2}.LibRowId:{3}.UIVersion:{4}.", parentFolder?.Id, parentFolder.ServerRelativeUrl, entity.NodeId, entity.LibRowId, entity.UIVersion);
                AveSPDoc aveDoc = GetAveSPDoc(entity, parentFolder, realName);
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(aveDoc);
                GetComplianceTagIfEnableRemove(aveDoc?.AveSPItem?.SPListItem, out ListItemComplianceInfo complianceInfo);
                if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(aveSite.SPSite, complianceInfo.ComplianceTag))
                {
                    mLog.Info("archive by 365 skip archvie current unlock status item. Item Name: {0}.", aveDoc?.AveSPItem.SPListItem.Name);
                    status = JobDetailsStatus.Skipped;
                    errorMessage = "StorageOptimization_Skip_Unlock_Status_Item";
                    current.IsCurrentVersion = true;
                    return new Tuple<string, JobDetailsStatus>(errorMessage, status); ;
                }
                current.WrapperObject = aveDoc;
                aveDoc.AveSPItem.IsBackupLinkForArchivedData = false;
                HeaderUrl headerUrl = new HeaderUrl();
                try
                {
                    var backupResult = await BackupDocumentBy365Async(aveDoc, entity);
                    status = backupResult.Item2;
                    errorMessage = backupResult.Item1;
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT && aveDoc != null && aveDoc.AveSPItem != null && aveDoc.AveSPItem.DocumentSize == 0)
                    {
                        mLog.Warn($"archive by 365 Current File Content Length is 0. path {entity.FullPath}");
                        if (Configuration.Skip0KBFile)
                        {
                            throw new FileContentLengthNullException("File Content Length is 0");
                        }
                    }

                }
                catch (FileContentLengthNullException)
                {
                    status = JobDetailsStatus.Skipped;
                    throw;
                }
                catch (Exception e)
                {
                    status = JobDetailsStatus.Failed;
                    mLog.Error($"[ArchiveBy365Async][Exception]archiver by 365 Backup Document or Document Version,Path{entity.FullPath} Error: {e.ToString()}");
                    Configuration.ProgressDto.HasErrorNode = true;
                    current.DoDelete = false;
                    if (entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
                    {
                        Configuration.FailedVersionFileIds.Add(entity.ParentId);
                    }
                    throw;
                }
                finally
                {
                    mLog.Info($"archive by 365 finish backup by the 365 archive:{entity.NodeId}");
                }
                return new Tuple<string, JobDetailsStatus>(errorMessage, status);
            }
        }


        private AveSPDoc GetAveSPDoc(ArchiveApproveReport entity, AveSPFolder parentFolder, string realName)
        {
            using (AvePerformanceScope avePerformanceScope = new AvePerformanceScope("ArchiverBackup.GetAveSPDoc"))
            {
                AveSPDoc aveDoc = null;
                try
                {
                    //Document / Document Version Level Rule，Discover组成的ArchiveApproveReport对象不包含IAveListItem，会走到此判断.
                    //CG Discover，Current 需要check rule，导致实例化了IAveListItem对象，但是Version并没有实例化，会导致Version和Current同时多线程处理时出现异常.因此CG Discover全部走#1.
                    //if (entity.ItemObject == null || Configuration.IsCGDBDiscover)
                    if (entity.ItemObject == null)
                    {
                        if (Configuration.CheckBackupItemCacheExist(entity.NodeId))
                        {
                            //other thread already init IAveListItem.
                            if (Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem != null)
                            {
                                mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} use other thread init IAveListItem.");
                                aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem);
                            }
                            else
                            {
                                //wait other thread init IAveListItem.
                                if (CheckOtherThreadHasInitIAveListItem(entity.NodeId, entity.UIVersion))
                                {
                                    //other thread init success.
                                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, Configuration.ArchiverBackupCacheItems[entity.NodeId].CacheItem);
                                }
                                else
                                {
                                    //other thread init failed and force backup.
                                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                                }
                            }
                        }
                        else
                        {
                            mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} is responsible for init IAveListItem.");
                            aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                            Configuration.UpdateBackupItemCache(entity.NodeId, aveDoc.AveSPItem.SPListItem);
                        }
                    }
                    //Container级别Rule，Discover组成的ArchiveApproveReport对象包含IAveListItem，默认会走到此判断.
                    else
                    {
                        mLog.Info($"Current document:{entity.NodeId} version:{entity.UIVersion} use ArchiveApproveReport IAveListItem.");
                        aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName, entity.ItemObject);
                    }
                }
                catch (Exception ex)
                {
                    //添加兜底逻辑
                    mLog.Info($"GetAveSPDoc failed.NodeId:{entity.NodeId} version:{entity.UIVersion} use ArchiveApproveReport IAveListItem.");
                    aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                }
                return aveDoc;
            }
        }

        private bool CheckOtherThreadHasInitIAveListItem(string nodeId, int version)
        {
            bool hasInitIAveListItem = false;
            int checkTimes = 0;
            do
            {
                checkTimes++;
                if (Configuration.CheckOtherThreadHasInitIAveListItem(nodeId))
                {
                    mLog.Info($"Current IAveListItem:{nodeId} version:{version} has init by other thread.");
                    hasInitIAveListItem = true;
                    break;
                }
                else
                {
                    mLog.Warn($"Current IAveListItem:{nodeId} version:{version} begin sleep 2s and wait document init.");
                    Thread.Sleep(2 * 1000);
                    if (checkTimes >= 10)
                    {
                        mLog.Warn($"Current IAveListItem:{nodeId} version:{version} sleep over 5 times and begin force backup.");
                        break;
                    }
                }
            } while (true);
            return hasInitIAveListItem;
        }

        public void SendJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            Configuration.ProgressDto.HasCompleteNode = status == JobDetailsStatus.Successful;
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(status, "", "PhysicalDelete", ConvertToSPCacheNodeType(int.Parse(cacheNodeType)));
            Configuration.JobReportDto.AddDeletionReport(originPath, 0, status, ConvertToSPCacheNodeType(int.Parse(cacheNodeType)), "", "", "", "PhysicalDelete", comment);
        }

        public int ConvertToSPCacheNodeType(int cacheNodeType)
        {
            int spCacheNodeType = (int)CacheNodeType.Item;
            switch (cacheNodeType)
            {
                case (int)RMNodeLevel.PhysicalFile:
                    spCacheNodeType = (int)CacheNodeType.Folder;
                    break;
                case (int)RMNodeLevel.PhysicalRecord:
                    spCacheNodeType = (int)CacheNodeType.Item;
                    break;
                default:
                    break;
            }
            return spCacheNodeType;
        }
    }
    internal class HSMItemBackup : SPObjectBackup, IMultiBackup
    {
        private readonly CallProcess callProcess = new Archiver.CallProcess();
        private static readonly HttpClient ManifestHttpClient = new HttpClient();
        private HSMConnector HSMConnector
        {
            get
            {
                if (HSMConnectorInstance == null)
                {
                    HSMConnectorInstance = HSMConnector.GetInstance(Configuration);
                }
                return HSMConnectorInstance;
            }
        }
        public HSMItemBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"HSMItemBackup.ProcessBackedNode should not reach, item id:{entity?.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            string errorMessage = string.Empty;
            string fullPath = entity.FullPath ?? string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
            try
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("ArchiverBackup.HSMItemBackup"))
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        mLog.Info($"Start HSM item backup {entity.LibRowId}.");
                    }
                    else if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
                    {
                        mLog.Info($"Start HSM item version backup {entity.LibRowId}.");
                    }

                    bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(Configuration.currentRule);
                    if (isLinkToDucument)
                    {
                        entity.StubInfo = Configuration.currentRule.LeaveStubType.ToString();
                    }
                    else
                    {
                        entity.StubInfo = "null";
                    }
                    switch (entity.NodeType)
                    {
                        case (int)ArchiverCommon.ItemType.DOCUMENT:
                        case (int)ArchiverCommon.ItemType.DOCUMENT_VER:
                            BackupDocumentOrDocumentVersionFromManifest(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender, ref errorMessage, ref status);
                            break;
                        default:
                            status = JobDetailsStatus.Skipped;
                            errorMessage = "Unsupported node type for HSM backup.";
                            mLog.Warn($"Skip HSM backup for unsupported node type:{entity.NodeType}");
                            break;
                    }

                    current.BackupStatus = status == JobDetailsStatus.Failed ? FileHeaderStatus.Failed : FileHeaderStatus.Complete;
                }
            }
            catch (FileContentLengthNullException)
            {
                status = JobDetailsStatus.Skipped;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                errorMessage = "RM_JM_Detail_SkipBackup0KBFile";
                Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), 0, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
            }
            catch (Exception ex)
            {
                mLog.Error($"[BackupAsync][Exception]Backup HSM Item:{fullPath} .Error: {ex}");
                Configuration.ProgressDto.HasErrorNode = true;
                errorMessage = ex.Message;
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                throw;
            }
            finally
            {
                Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                }

                if (status != JobDetailsStatus.Failed && entity.DoDelete && errorMessage != "RM_JM_Detail_SkipBackup0KBFile")
                {
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, entity.DocumentSize, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), entity.DocumentSize, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                }
                else if (entity.DoDelete)
                {
                    if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT || entity.NodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                    else if (JobExecutionProcessStatisticExecutor.Instance.IsRuleSupportVersionAction((CacheNodeType)entity.CacheNodeType, Configuration.currentRule))
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                    if (errorMessage.Contains("Item does not exist. It may have been deleted by another user."))
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles(true, false, false);
                    }
                    JobExecutionProgressStatisticExecutor.Instance.IncreaseArchivedFiles(0);
                }
            }

            return 0;
        }

        private void BackupDocumentOrDocumentVersionFromManifest(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender aveSender, ref string errorMessage, ref JobDetailsStatus status)
        {
            ManifestDocumentSnapshot snapshot = entity.ManifestDocumentSnapshot;
            if (snapshot == null)
            {
                throw new InvalidOperationException("Manifest snapshot is required for manifest-only backups.");
            }

            if (!entity.DoDelete)
            {
                if (ruleLevel == (int)PolicyLevel.DocumentVersion)
                {
                    current.IsCurrentVersion = true;
                }
                else
                {
                    mLog.Warn($"this item DoDelete is false and ruleLevel is {ruleLevel}");
                }
                status = JobDetailsStatus.Successful;
                return;
            }

            status = JobDetailsStatus.Successful;
            current.WrapperObject = snapshot;
            string headerPath = snapshot.DocumentAccessUrl;
            if (string.IsNullOrWhiteSpace(headerPath))
            {
                headerPath = !string.IsNullOrWhiteSpace(entity.FullPath) ? entity.FullPath : snapshot.FileServerRelativeUrl ?? string.Empty;
            }
            bool needRecordStubId = Configuration?.currentRule?.NeedRecordStubId ?? false;
            long resolvedDocumentSize = snapshot.DocumentSize;
            if (resolvedDocumentSize <= 0)
            {
                resolvedDocumentSize = GetManifestContentLength(snapshot);
            }
            if (resolvedDocumentSize > 0)
            {
                entity.DocumentSize = resolvedDocumentSize;
            }

            try
            {
                int backupFileType = Configuration.GetBackupFileType();
                if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    aveSender.BackupManifestDocumentHeader(snapshot, entity, ruleName, subJobId, mediaName, headerPath, backupFileType, needRecordStubId);
                }
                else
                {
                    aveSender.BackupManifestDocumentVersionHeader(snapshot, entity, ruleName, subJobId, mediaName, headerPath, backupFileType);
                }

                current.FileHeader = aveSender.BackupHeader(headerPath);

                if (Configuration.Skip0KBFile && resolvedDocumentSize == 0 && entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    mLog.Warn($"Manifest file content length is 0. path {entity.FullPath}");
                    throw new FileContentLengthNullException("File Content Length is 0");
                }

                BackupManifestDocument(snapshot, aveSender, fileatrrinfo);
                PopulateManifestAttributes(snapshot, entity, fileatrrinfo);

                current.BackupStatus = FileHeaderStatus.Complete;
                if (HSMConnector.IsStart && entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT && snapshot.EnableHsm)
                {
                    TryQueueHsmStubFromManifest(parent, entity, snapshot);
                }
            }
            catch (FileContentLengthNullException)
            {
                status = JobDetailsStatus.Skipped;
                throw;
            }
            catch (Exception ex)
            {
                status = JobDetailsStatus.Failed;
                Configuration.ProgressDto.HasErrorNode = true;
                current.DoDelete = false;
                current.BackupStatus = FileHeaderStatus.Failed;
                errorMessage = ex.Message;
                mLog.Error($"[BackupDocumentOrDocumentVersionFromManifest][Exception]Backup Manifest Document Error. Path:{entity.FullPath}. Error:{ex}");
                throw;
            }
            finally
            {
                string tail = fileatrrinfo.ToString();
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupDocumentOrDocumentVersionInfo, entity.NodeId);
                aveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
            }
        }

        private void BackupManifestDocument(ManifestDocumentSnapshot snapshot, BackupInfoSender aveSender, FileAtrributeInfo fileatrrinfo)
        {
            var stream = aveSender.BackupStream;
            stream.BeginWriteMetadata();
            try
            {
                WriteManifestMetadataEntries(snapshot, stream);
            }
            finally
            {
                stream.EndWriteMetadata();
            }

            WriteManifestContent(snapshot, stream, fileatrrinfo);
        }

        private void TryQueueHsmStubFromManifest(CacheNode parent, ArchiveApproveReport entity, ManifestDocumentSnapshot snapshot)
        {
            try
            {
                var siteUrl = snapshot.Site?.Url ?? string.Empty;
                var fileUrl = NormalizeServerRelativeUrl(snapshot.FileServerRelativeUrl)
                    ?? NormalizeServerRelativeUrl(snapshot.DocumentServerRelativeUrl)
                    ?? NormalizeServerRelativeUrl(entity.FullPath?.Replace("\\", "/"))
                    ?? string.Empty;

                var fileName = entity.LeafName ?? Path.GetFileName(fileUrl);
                string pathMd5 = string.Empty;
                var folderPath = NormalizeServerRelativeUrl(snapshot.Folder?.Path ?? snapshot.Folder?.ServerRelativeUrl) ?? string.Empty;
                if (parent.WrapperObject is AveSPFolder)
                {
                    string SPfolderPath = (parent.WrapperObject as AveSPFolder).Path;
                    pathMd5 = LinkFileCommon.GetDocumnetPathMD5(siteUrl, SPfolderPath, fileName);
                    mLog.Info($"parent.WrapperObject is AveSPFolder,the foldre path is :{SPfolderPath},md5:{pathMd5}");
                }
                else
                {
                    pathMd5 = LinkFileCommon.GetDocumnetPathMD5(siteUrl, folderPath, fileName);
                    mLog.Info($"parent.WrapperObject not AveSPFolder,the foldre path is :{folderPath},md5:{pathMd5}");
                }
                var listInfo = snapshot.List;
                if (listInfo != null &&
                    (listInfo.BaseTemplate == (int)AveListTemplateType.DesignCatalog
                    || listInfo.BaseTemplate == (int)AveListTemplateType.MasterPageCatalog
                    || listInfo.BaseTemplate == (int)AveListTemplateType.WebPageLibrary
                    || listInfo.BaseTemplate == (int)AveListTemplateType.ThemeCatalog
                    || listInfo.Hidden || listInfo.IsCatalog))
                {
                    mLog.Info($"Skip system file for HSM Stub {entity.NodeId}");
                    return;
                }

                var resolvedStubId = !string.IsNullOrWhiteSpace(snapshot.StubId)
                    ? snapshot.StubId
                    : (!string.IsNullOrWhiteSpace(entity.StubId)
                        ? entity.StubId
                        : string.Concat(Guid.NewGuid().ToString("N"), DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)));
                entity.StubId = resolvedStubId;

                var webId = entity.WebID;

                var manifestStub = new HSMManifestFileInfo
                {
                    FileServerRelatedUrl = fileUrl,
                    PathMD5 = pathMd5,
                    StubId = resolvedStubId,
                    SpId = entity.NodeId,
                    WebId = webId,
                    DocumentSize = entity.DocumentSize > 0 ? entity.DocumentSize : snapshot.DocumentSize,
                    FileName = fileName,
                    DocumentAccessUrl = snapshot.DocumentAccessUrl,
                    SiteUrl = siteUrl,
                    FolderPath = folderPath,
                    ListId = snapshot.List?.Id ?? Guid.Empty,
                    BaseTemplate = snapshot.List?.BaseTemplate ?? 0,
                    Hidden = snapshot.List?.Hidden ?? false,
                    IsCatalog = snapshot.List?.IsCatalog ?? false,
                    RowId = entity.LibRowId,
                    RuleName = Configuration?.currentRule?.Name ?? string.Empty,
                    StubTemplateId = Configuration?.currentRule?.StubTemplateId,
                    CreatedTime = snapshot.CreatedTime,
                    ModifiedTime = snapshot.ModifiedTime,
                    Author = snapshot.Author,
                    Editor = snapshot.Editor,
                    AuthorId = snapshot.AuthorId,
                    ModifiedId = snapshot.EditorId,
                    ColumnValues = RemoveNoNeedFieldForStub(snapshot.ColumnValues),
                    RoleAssignments = ResolveRoleAssignments(snapshot),
                    VersionString = snapshot.Version,
                    TotalSize = snapshot.TotalSize,
                    ParentFolder = parent.WrapperObject as AveSPFolder
                };

                HSMConnector.Add2Queue(manifestStub);
            }
            catch (Exception ex)
            {
                mLog.Warn($"Skip HSM stub creation for manifest-only backup. Node:{entity.NodeId}. Error:{ex}");
            }
        }
        private Dictionary<string, object> RemoveNoNeedFieldForStub(Dictionary<string, object> columns)
        {
            if (columns.ContainsKey("MediaServiceMetadata"))
            {
                columns["MediaServiceMetadata"] = string.Empty;
            }
            if (columns.ContainsKey("MediaServiceFastMetadata"))
            {
                columns["MediaServiceFastMetadata"] = string.Empty;
            }
            return columns;
        }
        private static string NormalizeServerRelativeUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var trimmed = path.Trim();
            return trimmed.StartsWith("/", StringComparison.Ordinal)
                ? trimmed
                : "/" + trimmed.TrimStart('/');
        }

        private void WriteManifestMetadataEntries(ManifestDocumentSnapshot snapshot, IAveBackupStream stream)
        {
            if (snapshot.MetadataEntries == null || snapshot.MetadataEntries.Count == 0)
            {
                mLog.Warn($"Manifest snapshot metadata is empty for node {snapshot?.DocumentAccessUrl}.");
                return;
            }

            foreach (var entry in snapshot.MetadataEntries)
            {
                if (entry == null || entry.Data == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(entry.Type) && Enum.TryParse(entry.Type, true, out AveMetadataType metadataType))
                {
                    var normalizedData = NormalizeManifestMetadataData(metadataType, entry.Data);

                    if (normalizedData is IDictionary dictionary)
                    {
                        stream.WriteMetadata(metadataType, dictionary);
                    }
                    else
                    {
                        stream.WriteMetadata(metadataType, normalizedData);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(entry?.Type))
                {
                    stream.WriteMetadata(entry.Type, entry.Data);
                }
            }
        }

        private static object NormalizeManifestMetadataData(AveMetadataType metadataType, object data)
        {
            if (data is JProperty jProperty)
            {
                data = jProperty.Value;
            }

            if (data is JToken token)
            {
                switch (metadataType)
                {
                    case AveMetadataType.DocProperty:
                    case AveMetadataType.DocData:
                        return token.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    case AveMetadataType.RoleAssignment:
                        return token.ToObject<List<AveRoleAssignmentInfo>>() ?? new List<AveRoleAssignmentInfo>();
                    default:
                        return token.ToObject<object>();
                }
            }

            return data;
        }

        private static List<AveRoleAssignmentInfo> ResolveRoleAssignments(ManifestDocumentSnapshot snapshot)
        {
            if (snapshot?.MetadataEntries == null || snapshot.MetadataEntries.Count == 0)
            {
                return new List<AveRoleAssignmentInfo>();
            }

            var roleEntry = snapshot.MetadataEntries
                .FirstOrDefault(e => e != null && string.Equals(e.Type, AveMetadataType.RoleAssignment.ToString(), StringComparison.OrdinalIgnoreCase));

            if (roleEntry?.Data == null)
            {
                return new List<AveRoleAssignmentInfo>();
            }

            var normalized = NormalizeManifestMetadataData(AveMetadataType.RoleAssignment, roleEntry.Data);

            if (normalized is List<AveRoleAssignmentInfo> typedList)
            {
                return typedList;
            }

            if (normalized is IEnumerable<AveRoleAssignmentInfo> enumerable)
            {
                return enumerable.ToList();
            }

            return new List<AveRoleAssignmentInfo>();
        }

        private void WriteManifestContent(ManifestDocumentSnapshot snapshot, IAveBackupStream stream, FileAtrributeInfo fileatrrinfo)
        {
            string contentPath = snapshot.ContentFilePath;
            bool deleteAfterUse = ShouldDeleteTempContent(contentPath);

            using (Stream contentStream = OpenManifestContentStream(snapshot))
            {
                if (contentStream == null)
                {
                    throw new InvalidOperationException("Manifest snapshot does not contain content information.");
                }

                if (contentStream.CanSeek)
                {
                    stream.FlushMetadata(contentStream.Length);
                    contentStream.Position = 0;
                }
                else
                {
                    stream.FlushMetadata(-1);
                }

                byte[] buffer = new byte[64 * 1024];
                Crc64 hashAlgorithm = Configuration.IsCalculateCRC ? new Crc64() : null;
                int read;
                while ((read = contentStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (hashAlgorithm != null)
                    {
                        hashAlgorithm.Append(new ReadOnlySpan<byte>(buffer, 0, read));
                    }
                    stream.WriteContent(buffer, 0, read);
                }

                if (hashAlgorithm != null)
                {
                    fileatrrinfo.Crc64 = Convert.ToBase64String(hashAlgorithm.GetCurrentHash());
                }
            }

            if (deleteAfterUse && !string.IsNullOrWhiteSpace(contentPath) && System.IO.File.Exists(contentPath))
            {
                try
                {
                    System.IO.File.Delete(contentPath);
                }
                catch (Exception ex)
                {
                    mLog.Warn($"Failed to delete temp content file. Directory:{DescribePathForLog(contentPath)}.", ex);
                }
            }
        }

        private Stream OpenManifestContentStream(ManifestDocumentSnapshot snapshot)
        {
            if (snapshot == null)
            {
                mLog.Warn("OpenManifestContentStream received a null snapshot.");
                return null;
            }

            mLog.Info("OpenManifestContentStream started. HasContentPath:{0}, HasContentBytes:{1}, HasStorageConnection:{2}, HasContentBlobPrefix:{3}.",
                !string.IsNullOrWhiteSpace(snapshot.ContentFilePath),
                snapshot.ContentBytes != null && snapshot.ContentBytes.Length > 0,
                !string.IsNullOrWhiteSpace(snapshot.StorageConnectionString),
                !string.IsNullOrWhiteSpace(snapshot.ContentBlobPrefix));

            if (!string.IsNullOrWhiteSpace(snapshot.ContentFilePath) && System.IO.File.Exists(snapshot.ContentFilePath))
            {
                mLog.Info("OpenManifestContentStream selected local file content. Directory:{0}.", DescribePathForLog(snapshot.ContentFilePath));
                return new FileStream(snapshot.ContentFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.ContentFilePath))
            {
                mLog.Info("OpenManifestContentStream local file path is configured but file does not exist. Directory:{0}.", DescribePathForLog(snapshot.ContentFilePath));
            }

            if (snapshot.ContentBytes != null && snapshot.ContentBytes.Length > 0)
            {
                mLog.Info("OpenManifestContentStream selected in-memory content. ByteLength:{0}.", snapshot.ContentBytes.Length);
                return new MemoryStream(snapshot.ContentBytes, false);
            }

            mLog.Info("OpenManifestContentStream did not find usable in-memory content.");

            if (TryOpenBlobContentStream(snapshot, out Stream blobStream))
            {
                mLog.Info("OpenManifestContentStream selected direct blob stream content.");
                return blobStream;
            }

            mLog.Info("OpenManifestContentStream direct blob stream is unavailable. HasStorageConnection:{0}, HasContentBlobPrefix:{1}, HasContentPath:{2}.",
                !string.IsNullOrWhiteSpace(snapshot.StorageConnectionString),
                !string.IsNullOrWhiteSpace(snapshot.ContentBlobPrefix),
                !string.IsNullOrWhiteSpace(snapshot.ContentFilePath));

            // Lazy download from Azure blob if storage info is provided.
            if (!string.IsNullOrWhiteSpace(snapshot.StorageConnectionString)
                && !string.IsNullOrWhiteSpace(snapshot.ContentBlobPrefix)
                && !string.IsNullOrWhiteSpace(snapshot.ContentFilePath))
            {
                mLog.Info("OpenManifestContentStream is attempting lazy blob download. ContentDirectory:{0}, BlobPrefix:{1}.", DescribePathForLog(snapshot.ContentFilePath), DescribePrefixForLog(snapshot.ContentBlobPrefix));
                var downloadedPath = DownloadContentFromBlob(snapshot);
                if (!string.IsNullOrWhiteSpace(downloadedPath) && System.IO.File.Exists(downloadedPath))
                {
                    snapshot.ContentFilePath = downloadedPath; // allow downstream delete-after-use
                    mLog.Info("OpenManifestContentStream selected lazy-downloaded content. Directory:{0}.", DescribePathForLog(downloadedPath));
                    return new FileStream(downloadedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

                mLog.Warn("OpenManifestContentStream lazy blob download did not produce a readable local file.");
            }
            else
            {
                mLog.Info("OpenManifestContentStream skipped lazy blob download because required storage information is incomplete.");
            }

            if (!string.IsNullOrWhiteSpace(snapshot.ContentFilePath) && TryOpenRemoteManifest(snapshot.ContentFilePath, out Stream remoteContentStream))
            {
                mLog.Info("OpenManifestContentStream selected remote content stream.");
                return remoteContentStream;
            }

            if (!string.IsNullOrWhiteSpace(snapshot.ContentFilePath))
            {
                mLog.Warn("OpenManifestContentStream remote content is unavailable. Location:{0}.", DescribePathForLog(snapshot.ContentFilePath));
            }
            else
            {
                mLog.Warn("OpenManifestContentStream cannot attempt remote content because ContentFilePath is empty.");
            }

            mLog.Warn("OpenManifestContentStream returned null because no content source was available.");

            return null;
        }

        private bool TryOpenBlobContentStream(ManifestDocumentSnapshot snapshot, out Stream blobStream)
        {
            blobStream = null;

            if (string.IsNullOrWhiteSpace(snapshot.StorageConnectionString)
                || string.IsNullOrWhiteSpace(snapshot.ContentBlobPrefix)
                || string.IsNullOrWhiteSpace(snapshot.ContentFilePath))
            {
                mLog.Info("TryOpenBlobContentStream skipped. HasStorageConnection:{0}, HasContentBlobPrefix:{1}, HasContentPath:{2}.",
                    !string.IsNullOrWhiteSpace(snapshot.StorageConnectionString),
                    !string.IsNullOrWhiteSpace(snapshot.ContentBlobPrefix),
                    !string.IsNullOrWhiteSpace(snapshot.ContentFilePath));
                return false;
            }

            try
            {
                BlobContainerClient containerClient = null;
                string blobName = string.Empty;
                if (!string.IsNullOrEmpty(snapshot.ContentStorageConnectionString))
                {
                    blobName = snapshot.ContentFilePath;
                    containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(snapshot.ContentStorageConnectionString);
                }
                else
                {
                    var blobPrefix = snapshot.ContentBlobPrefix.TrimStart('/');
                    if (!blobPrefix.EndsWith("/", StringComparison.Ordinal))
                    {
                        blobPrefix += "/";
                    }

                    blobName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobPrefix, snapshot.ContentFilePath.Replace("\\", "/", StringComparison.Ordinal)).Replace("//", "/", StringComparison.Ordinal);

                    containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(snapshot.StorageConnectionString);

                }
                var blobClient = containerClient.GetBlobClient(blobName);
                var exists = blobClient.Exists();
                if (!exists.Value)
                {
                    mLog.Warn("TryOpenBlobContentStream could not find blob content. BlobPrefix:{0}, ContentDirectory:{1}.", DescribePrefixForLog(snapshot.ContentBlobPrefix), DescribePathForLog(snapshot.ContentFilePath));
                    return false;
                }

                blobStream = blobClient.OpenRead();
                mLog.Info("TryOpenBlobContentStream opened blob stream successfully. BlobPrefix:{0}.", DescribePrefixForLog(snapshot.ContentBlobPrefix));
                return blobStream != null;
            }
            catch (Exception ex)
            {
                mLog.Warn($"TryOpenBlobContentStream failed. BlobPrefix:{DescribePrefixForLog(snapshot.ContentBlobPrefix)}, ContentDirectory:{DescribePathForLog(snapshot.ContentFilePath)}.", ex);
                blobStream = null;
                return false;
            }
        }

        private string DownloadContentFromBlob(ManifestDocumentSnapshot snapshot)
        {
            try
            {
                var blobPrefix = snapshot.ContentBlobPrefix.TrimStart('/');
                if (!blobPrefix.EndsWith("/", StringComparison.Ordinal))
                {
                    blobPrefix += "/";
                }

                var blobName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobPrefix, snapshot.ContentFilePath.Replace("\\", "/", StringComparison.Ordinal)).Replace("//", "/", StringComparison.Ordinal);

                var containerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(snapshot.StorageConnectionString);
                var blobClient = containerClient.GetBlobClient(blobName);
                var exists = blobClient.Exists();
                if (!exists.Value)
                {
                    mLog.Warn("DownloadContentFromBlob could not find blob content. BlobPrefix:{0}, ContentDirectory:{1}.", DescribePrefixForLog(snapshot.ContentBlobPrefix), DescribePathForLog(snapshot.ContentFilePath));
                    return null;
                }

                var tempRoot = Path.Combine(Path.GetTempPath(), "HSMManifest", "Content");
                var targetPath = Path.Combine(tempRoot, snapshot.ContentFilePath.Replace('/', Path.DirectorySeparatorChar));
                var targetDir = Path.GetDirectoryName(targetPath) ?? tempRoot;
                Directory.CreateDirectory(targetDir);
                mLog.Info("DownloadContentFromBlob is downloading blob content to temp directory. TempDirectory:{0}, BlobPrefix:{1}.", DescribePathForLog(targetPath), DescribePrefixForLog(snapshot.ContentBlobPrefix));

                using (var fs = System.IO.File.Create(targetPath))
                {
                    blobClient.DownloadTo(fs);
                }

                mLog.Info("DownloadContentFromBlob completed successfully. TempDirectory:{0}.", DescribePathForLog(targetPath));
                return targetPath;
            }
            catch (Exception ex)
            {
                mLog.Warn($"DownloadContentFromBlob failed. BlobPrefix:{DescribePrefixForLog(snapshot.ContentBlobPrefix)}, ContentDirectory:{DescribePathForLog(snapshot.ContentFilePath)}.", ex);
                return null;
            }
        }

        private static bool ShouldDeleteTempContent(string contentPath)
        {
            if (string.IsNullOrWhiteSpace(contentPath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.GetFullPath(contentPath);
                var tempRoot = Path.GetFullPath(Path.GetTempPath());
                return fullPath.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private bool TryOpenRemoteManifest(string manifestLocation, out Stream remoteStream)
        {
            remoteStream = null;

            if (!Uri.TryCreate(manifestLocation, UriKind.Absolute, out Uri manifestUri))
            {
                mLog.Info("TryOpenRemoteManifest skipped because location is not an absolute URI. Location:{0}.", DescribePathForLog(manifestLocation));
                return false;
            }

            if (!IsHttpOrHttps(manifestUri.Scheme))
            {
                mLog.Info("TryOpenRemoteManifest skipped because URI scheme is unsupported. Scheme:{0}.", manifestUri.Scheme);
                return false;
            }

            try
            {
                remoteStream = ManifestHttpClient.GetStreamAsync(manifestUri).ConfigureAwait(false).GetAwaiter().GetResult();
                mLog.Info("TryOpenRemoteManifest opened remote stream successfully. Scheme:{0}, Host:{1}, Path:{2}.", manifestUri.Scheme, manifestUri.Host, DescribePathForLog(manifestUri.AbsolutePath));
                return remoteStream != null;
            }
            catch (Exception ex)
            {
                mLog.Warn($"TryOpenRemoteManifest failed. Scheme:{manifestUri.Scheme}, Host:{manifestUri.Host}, Path:{DescribePathForLog(manifestUri.AbsolutePath)}.", ex);
                remoteStream = null;
                return false;
            }
        }

        private static string DescribePathForLog(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "<empty>";
            }

            try
            {
                var normalized = path.Replace('\\', '/').TrimEnd('/');
                var lastSlashIndex = normalized.LastIndexOf('/');
                if (lastSlashIndex <= 0)
                {
                    return normalized;
                }

                return normalized.Substring(0, lastSlashIndex);
            }
            catch
            {
                return "<unavailable>";
            }
        }

        private static string DescribePrefixForLog(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return "<empty>";
            }

            try
            {
                return prefix.Replace('\\', '/').TrimEnd('/');
            }
            catch
            {
                return "<unavailable>";
            }
        }

        private static bool IsHttpOrHttps(string scheme)
        {
            return !string.IsNullOrWhiteSpace(scheme) &&
                (scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                 scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase));
        }

        private void PopulateManifestAttributes(ManifestDocumentSnapshot snapshot, ArchiveApproveReport entity, FileAtrributeInfo fileatrrinfo)
        {
            if (snapshot.IsSystemFile)
            {
                fileatrrinfo.IsSystemFile = true;
                return;
            }

            Dictionary<string, object> columnValues = FilterWritableColumns(snapshot.ColumnValues);
            List<TagInfoCollection> mergedTags = new List<TagInfoCollection>();
            if (Configuration.tagInfoCollection != null && Configuration.tagInfoCollection.Count > 0)
            {
                mergedTags.AddRange(Configuration.tagInfoCollection);
            }
            if (snapshot.TagInfoOverrides != null && snapshot.TagInfoOverrides.Count > 0)
            {
                mergedTags.AddRange(snapshot.TagInfoOverrides.Where(tag => tag != null));
            }

            string displayName = !string.IsNullOrWhiteSpace(snapshot.FileTitle) ? snapshot.FileTitle : entity.LeafName;
            SetItemAttributes(null, columnValues, displayName, entity, fileatrrinfo, mergedTags.Count == 0 ? null : mergedTags);
        }

        private static Dictionary<string, object> FilterWritableColumns(Dictionary<string, object> source)
        {
            if (source == null || source.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            // 黑名单：过滤已知只读/系统/计算字段，允许其余自定义列自动透传。
            var readOnlyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ID","GUID","UniqueId","FileRef","FileDirRef","FileLeafRef","FSObjType","ContentTypeId","ContentType",
                "_UIVersion","_UIVersionString","_Level","_ModerationStatus","_CheckinComment","WorkflowVersion","WorkflowInstanceID",
                "_HasCopyDestinations","_CopySource","AppAuthor","AppEditor","PermMask",
                "DocIcon","FileSizeDisplay","SortBehavior","MetaInfo","File_x0020_Size","EncodedAbsUrl","ServerUrl","BaseName",
                "_IsCurrentVersion","_IsRecord","InstanceID","_ModerationComments","Modified_x0020_By","Created_x0020_By","File_x0020_Type",
                "HTML_x0020_File_x0020_Type","_SourceUrl","_SharedFileIndex","_ColorHex","_ColorTag","_Emoji","MediaGeneratedMetadata",
                "MediaUserMetadata","ExtractedMetadataComputed","ComplianceAssetId","TemplateUrl","xd_ProgID","xd_Signature","_EffectiveIpLabelDisplayName",
                "_ShortcutUrl","_ShortcutSiteId","_ShortcutWebId","_ShortcutUniqueId","_ExtendedDescription","TriggerFlowInfo","PrincipalCount",
                "LinkCheckedOutTitle","_EditMenuTableStart","_EditMenuTableStart2","_EditMenuTableEnd","A2ODMountCount","MainLinkSettings",
                "SelectTitle","SelectFilename","Combine","RepairDocument","PolicyDisabledUICapabilities"
            };

            var filtered = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in source)
            {
                if (kv.Key != null && !readOnlyColumns.Contains(kv.Key))
                {
                    filtered[kv.Key] = kv.Value;
                }
            }
            return filtered;
        }


        private long GetManifestContentLength(ManifestDocumentSnapshot snapshot)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.ContentFilePath) && System.IO.File.Exists(snapshot.ContentFilePath))
            {
                return new FileInfo(snapshot.ContentFilePath).Length;
            }

            if (snapshot.ContentBytes != null)
            {
                return snapshot.ContentBytes.LongLength;
            }

            return 0;
        }

        public void SendJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            Configuration.ProgressDto.HasCompleteNode = status == JobDetailsStatus.Successful;
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(status, string.Empty, "PhysicalDelete", ConvertToSPCacheNodeType(int.Parse(cacheNodeType)));
            Configuration.JobReportDto.AddDeletionReport(originPath, 0, status, ConvertToSPCacheNodeType(int.Parse(cacheNodeType)), string.Empty, string.Empty, string.Empty, "PhysicalDelete", comment);
        }

        public int ConvertToSPCacheNodeType(int cacheNodeType)
        {
            int spCacheNodeType = (int)CacheNodeType.Item;
            switch (cacheNodeType)
            {
                case (int)RMNodeLevel.PhysicalFile:
                    spCacheNodeType = (int)CacheNodeType.Folder;
                    break;
                case (int)RMNodeLevel.PhysicalRecord:
                    spCacheNodeType = (int)CacheNodeType.Item;
                    break;
                default:
                    break;
            }
            return spCacheNodeType;
        }
    }

    internal class AttachmentBackup : SPObjectBackup
    {
        public AttachmentBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"AttachmentBackup.ProcessBackedNode should not reach, item id:{entity?.NodeId}");
            return (int)JobDetailsStatus.Successful;
        }


        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            string errorMessage = string.Empty;
            string itemTitle = string.Empty;
            string fullPath = entity.FullPath;
            bool hasBackupHeader = false;
            AveSPItem parentNode = null;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
            HeaderUrl headerUrl = new HeaderUrl();
            var aveList = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.List).WrapperObject as AveSPList;
            bool isFolder = false;
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.AttachmentBackup"))
            {
                try
                {

                    if (parent.WrapperObject is AveSPListItem)
                    {
                        var parentItem = parent.WrapperObject as AveSPListItem;
                        parentNode = parentItem.AveSPItem;
                        itemTitle = parentItem.AveSPItem.Title;
                    }
                    else if (parent.WrapperObject is AveSPFolder)
                    {
                        var parentItem = parent.WrapperObject as AveSPFolder;
                        //如果Attachment 节点的ParentID 不是Folder 的ID ，说明Folder 不是Attachment 的Parent，需要重新获取下Parent
                        //由于DB 结构控制，只有Manual Job 会出现这样的现象 ADO-137300
                        if (parentItem.AveItem != null && !parentItem.AveItem.Id.ToString().Equals(entity.ParentId, StringComparison.OrdinalIgnoreCase))
                        {
                            int attIndex = entity.LeafName.IndexOf(':');
                            int DoclibRowId = 0;
                            if (attIndex >= 0)
                            {
                                DoclibRowId = Convert.ToInt32(entity.LeafName.Substring(0, entity.LeafName.IndexOfAny(new char[] { '_', '.' })));
                            }
                            int uiVersion = 0;
                            IAveListItem listItem = aveList.SPList.GetItemByUniqueId(new Guid(entity.ParentId));
                            try
                            {
                                uiVersion = Convert.ToInt32(listItem["_UIVersion"]);
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Getting item UIVersion with exception: {0}.", ex.ToString());
                            }
                            var aveListItem = new AveSPListItem(parentItem, entity.LeafName.Substring(0, attIndex), new Guid(entity.ParentId), DoclibRowId, uiVersion);
                            parentNode = aveListItem.AveSPItem;
                            itemTitle = aveListItem.AveSPItem.Title;
                        }
                        else
                        {
                            isFolder = true;
                            parentNode = parentItem.AveItem;
                            itemTitle = parentItem.AveItem.Title;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Attachment Full Path Assignment Error: {0}", e.ToString());
                    fullPath = entity.FullPath;
                }
                #region GetAttachmentServerRelativeUrl
                int index = entity.LeafName.IndexOf(':');
                int id = 0;
                string realName = string.Empty;
                if (index >= 0)
                {
                    id = Convert.ToInt32(entity.LeafName.Substring(0, entity.LeafName.IndexOfAny(new char[] { '_', '.' })));
                    realName = entity.LeafName.Substring(index + 1);
                }

                string serverUrl = headerUrl.GetAttachmentAP(aveList.ServerRelativeUrl, id, realName);
                fullPath = serverUrl;
                #endregion

                var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
                var aveAttachemnt = new AveSPAttachment(CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder, new Guid(entity.NodeId), entity.LeafName, serverUrl);
                aveAttachemnt.AveSPItem.IsBackupLinkForArchivedData = false;
                try
                {
                    if (RuleHelper.CheckArchiveOnlyRule(Configuration.currentRule.KeepDataOption))
                    {
                        current.IsSkipDeleteVersion = true;
                    }

                    GetComplianceTagIfEnableRemove(parentNode?.SPListItem, out ListItemComplianceInfo complianceInfo);
                    if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(aveSite.SPSite, complianceInfo.ComplianceTag))
                    {
                        mLog.Info("skip archive current unlock status item. Item Name: {0}.", parentNode?.Name);
                        status = JobDetailsStatus.Skipped;
                        errorMessage = "StorageOptimization_Skip_Unlock_Status_Item";
                        current.IsCurrentVersion = true;
                        return 0;
                    }
                    //add for RevIM export
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        AttachmentVault attachmentVault = (AttachmentVault)VaultExport;
                        attachmentVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        attachmentVault.VaultExport((int)entity.CacheNodeType, aveAttachemnt, fullPath, subJobId, ruleName, mediaName);
                    }
                    AveSender.BackupAttaHeader(parentNode, entity, AveSender.BackupStream.StreamTransfered, ruleName, subJobId, mediaName, headerUrl.GetAttachmentAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveAttachemnt.AveSPItem.AveSPList.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType());
                    current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(fullPath));
                    hasBackupHeader = true;
                    AveSender.BackupStream.BeginWriteMetadata();
                    aveAttachemnt.ExportDocInfo(AveSender.BackupStream, parentNode);
                    Dictionary<string, object> fullText = new Dictionary<string, object>();
                    foreach (TagInfoCollection tag in Configuration.tagInfoCollection)
                    {
                        try
                        {
                            fullText[tag.Key] = tag.Value;
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverAddInformationError, ex.ToString());
                        }
                    }
                    aveAttachemnt.ExportFullTextIndex(AveSender.BackupStream, fullText);
                    try
                    {
                        aveAttachemnt.ExportStorgeInfo(AveSender.BackupStream);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup Attachment Storage Info Error: {0}", e.ToString());
                        throw;
                    }
                    finally
                    {
                        AveSender.BackupStream.EndWriteMetadata();
                    }
                    
                    if (Configuration.IsCalculateCRC)
                    {
                        string fileCRC64 = aveAttachemnt.ExportContentAndCalculateCRC(AveSender.BackupStream);
                        fileatrrinfo.Crc64 = fileCRC64;
                    }
                    else
                    {
                        aveAttachemnt.ExportContent(AveSender.BackupStream);
                    }

                    if (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Item) == null)
                    {
                        //这个判断正常只会有Manual Job 单独的Attachment，没有Item在DB 中的情况才会走进来,
                        if (!isFolder && CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Folder) == null)
                        {
                            //普通Item 也会穿个空的displayName  ==  ""
                            SetItemAttributes(null, aveAttachemnt.GetAttachmentInfo(), "", entity, fileatrrinfo, Configuration.tagInfoCollection);
                        }
                        else
                        {
                            SetItemAttributes(null, aveAttachemnt.GetAttachmentInfo(), CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Folder).Name, entity, fileatrrinfo, Configuration.tagInfoCollection);
                        }
                    }
                    else
                    {
                        SetItemAttributes(null, aveAttachemnt.GetAttachmentInfo(), CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.Item).Name, entity, fileatrrinfo, Configuration.tagInfoCollection);
                        //在这里为Attachment 添加PostID 属性，如果Attachment Post 的Attachment，赋值Item ID ，如果是Reply 的Attachment ，赋值 RootPostID
                        if (aveAttachemnt.AveSPItem.AveSPList.SPList != null &&
                            aveAttachemnt.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.MicroFeed)
                        {
                            #region 获取NewsFeed  Attachment  的必要属性，为了Media使用
                            Dictionary<string, object> userData = parentNode.GetAllColumnValues(ColumnsLevel.DisplayColumns);
                            if (userData != null)
                            {
                                string microBlogType = userData.ElementAt(1).Key.ToString();
                                string rootPostID = userData.ElementAt(4).Key.ToString();
                                string createdTime = userData.ElementAt(34).Key.ToString();
                                string itemID = userData.ElementAt(32).Key.ToString();
                                if ((MicroBlogType)userData[microBlogType] == MicroBlogType.Post)
                                {
                                    fileatrrinfo.PostId = userData[itemID].ToString();
                                }
                                else
                                {
                                    if (userData.ContainsKey(rootPostID))
                                    {
                                        fileatrrinfo.PostId = userData[rootPostID].ToString();
                                    }
                                }
                                if (userData.ContainsKey(createdTime))
                                {
                                    fileatrrinfo.NewsFeedCreatedTime = ((DateTime)userData[createdTime]).Ticks;
                                }
                            }
                            #endregion
                        }
                    }
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                catch (Exception e)
                {
                    Configuration.FailedObjectIds.Add(id.ToString());
                    mLog.Error($"Backup Attachment:{entity.FullPath} Error: {e}");
                    errorMessage = e.Message.ToString();
                    status = JobDetailsStatus.Failed;
                    current.BackupStatus = FileHeaderStatus.Failed;
                    current.DoDelete = false;
                    throw;
                }
                finally
                {
                    string tail = fileatrrinfo.ToString();
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupInfo, tail);
                    if (hasBackupHeader)
                    {
                        AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
                    }
                    else
                    {
                        mLog.Warn($"Backup Attachment:{entity.FullPath} does not backup header so skip BackupTail.");
                    }
                    Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(Configuration.GetNodeFullPath(fullPath), AveSender.BackupStream.StreamTransfered, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                    if (status == JobDetailsStatus.Failed && !current.DoDelete)
                    {
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                }
                return 0;
            }
        }
    }

    internal class AppDefinitionBackup : SPObjectBackup
    {
        public AppDefinitionBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            try
            {
                var aveWeb = parent.WrapperObject as AveSPWeb;
                current.WrapperObject = aveWeb;
                AveSender.BackupAppDefinitionHeader(aveWeb, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, entity.FullPath);
                current.FileHeader = AveSender.GenerateHeader(entity.FullPath);
                current.BackupStatus = FileHeaderStatus.Complete;

                return (int)JobDetailsStatus.Successful;
            }
            catch(Exception e)
            {
                mLog.Error($"Fail build cache for app,ex:{e}");
                throw;
            }
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info("Start AppDefinition backup:{0}.", entity.FullPath);
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            string listType = string.Empty;
            var tail = new StringBuilder();
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver_AppDefinitionBackup"))
            {
                var aveWeb = parent.WrapperObject as AveSPWeb;
                current.WrapperObject = aveWeb;
                AveSender.BackupAppDefinitionHeader(aveWeb, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, entity.FullPath);
                current.FileHeader = AveSender.BackupHeader(entity.FullPath);
                AveSender.BackupStream.BeginWriteMetadata();
                try
                {
                    var aveAppManager = new AveSPAppManager(aveWeb, new Guid(entity.NodeId));
                    try
                    {
                        aveAppManager.ExportAppBaseInfo(AveSender.BackupStream);
                        //Backup Othe Metadata, for example security..
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup App Package Metadata Error : {0}", e.ToString());
                    }
                    finally
                    {
                        AveSender.BackupStream.EndWriteMetadata();
                        AveSender.BackupStream.FlushMetadata(0);
                        try
                        {
                            XmlElement xe = new XmlDocument().CreateElement("Attribute");
                            if (aveAppManager.AveAppInstance != null)
                            {
                                if (aveAppManager.AveAppInstance.Title != null)
                                {
                                    XmlElement titleInfo = new XmlDocument().CreateElement("Title");
                                    titleInfo.InnerText = aveAppManager.AveAppInstance.Title;
                                    tail.Append(titleInfo.OuterXml);
                                    xe.InnerText = "Title:" + aveAppManager.AveAppInstance.Title;
                                }
                                else
                                {
                                    xe.InnerText = "Title:" + string.Empty;
                                }
                                tail.Append(xe.OuterXml);
                                xe.InnerXml = "WebType:" + XmlConvert.EncodeName("APP");
                                tail.Append(xe.OuterXml);
                            }
                            xe.InnerXml = "TimeZoneID:" + AveTimeZoneUtility.ToTimeZoneInfoId(aveWeb.SPWeb.RegionalSettings.TimeZone.ID);
                            tail.Append(xe.OuterXml);
                        }
                        catch (Exception e)
                        {
                            mLog.Error("Backup APP Definaition Attribute Error: {0}", e.ToString());
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverStructError);
                        }
                        AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("Backup App Error: {0}", e.ToString());
                    errorMessage = e.Message.ToString();
                    status = JobDetailsStatus.Failed;
                    current.BackupStatus = FileHeaderStatus.Failed;
                    throw;
                }
                finally
                {
                    AveSender.BackupTail(true);
                    Configuration.ProgressDto.UpdateProgress(true);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, 0, Configuration.currentRule, status);
                    Configuration.JobReportDto.AddReport(entity.FullPath, 0, status, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, errorMessage);
                }

            }
            return 0;
        }
    }

    public class HeaderUrl
    {
        public HeaderUrl()
        {

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Test")]
        public string GetFolderVersionAP(string siteUrl, string siteSRUrl, string folerSRUrl, int id, int version)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, folerSRUrl) + "DispForm.aspx?ID=" + id.ToString() + "&VersionNo=" + version.ToString();
        }

        public string GetDocumentAP(string siteUrl, string siteSRUrl, string folderSRUrl, string docName)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, folderSRUrl).TrimEnd('/') + '/' + docName;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "url")]
        public string GetDocumentVersionAP(string siteUrl, string siteSRUrl, int version, string folderUrl, string docName)
        {
            int index = docName.IndexOf(':');
            if (index >= 0)
            {
                docName = docName.Substring(0, index);
            }
            return siteUrl + "/_vti_history/" + version.ToString() + "/" + folderUrl.Trim('/') + "/" + docName;
        }

        public string GetListItemAP(string siteUrl, string siteSRUrl, string listViewSRUrl, Guid listId, int id)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, listViewSRUrl) + "?ListId={" + listId.ToString() + "}&ID=" + id.ToString();
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Test")]
        public string GetListItemVersionAP(string siteUrl, string siteSRUrl, string listViewSRUrl, int id, int version)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, listViewSRUrl).TrimEnd('/') + "/DispForm.aspx?ID=" + id.ToString() + "&VersionNo=" + version.ToString();
        }

        public string GetAttachmentAP(string siteUrl, string siteSRUrl, string listSRUrl, string attName)
        {
            int index = attName.IndexOf(':');
            int id = 0;
            string realName = string.Empty;
            if (index >= 0)
            {
                id = Convert.ToInt32(attName.Substring(0, attName.IndexOfAny(new char[] { '_', '.' })));
                realName = attName.Substring(index + 1);
            }
            return GetAttachmentAP(GetUrlBySR(siteUrl, siteSRUrl, listSRUrl), id, realName);
        }

        public string GetAttachmentAP(string listUrl, int id, string attName)
        {
            return listUrl + "/Attachments/" + id + "/" + attName;
        }

        public string GetUrlBySR(string siteUrl, string siteSRUrl, string objSRUrl)
        {
            if (!string.IsNullOrEmpty(siteSRUrl) && !siteSRUrl.Equals("/", StringComparison.OrdinalIgnoreCase) && siteUrl.Contains(siteSRUrl))
            {
                return siteUrl.Remove(siteUrl.LastIndexOf(siteSRUrl, StringComparison.OrdinalIgnoreCase)).TrimEnd('/') + '/' + objSRUrl.TrimStart('/');
            }
            return siteUrl + '/' + objSRUrl.TrimStart('/');
        }
    }
}