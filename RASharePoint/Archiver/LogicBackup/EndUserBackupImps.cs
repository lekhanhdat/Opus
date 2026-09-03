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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using LS.SPWorkflowProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.Contract.FileSystem;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class RelativeDataSiteCollectionBackup : SPObjectBackup
    {
        public RelativeDataSiteCollectionBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataSiteCollectionBackup.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsSiteCollectionBackupbackup);
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.SiteCollectionBackup"))
                {
                    AveSPSite aveSite;
                    if (Configuration.sharePointType == GCommon.Contract.Tree.Object.SPType.BPOS)
                    {
                        aveSite = new AveSPSite(entity.LeafName, AveContextKind.ClientObjectModel, Configuration.user, AveSender.BackupStream);
                    }
                    else
                    {
                        //Online走不到当前逻辑
                        aveSite = new AveSPSite(entity.LeafName, AveContextKind.Auto, new AveBPOSAccountInfo(), AveSender.BackupStream);
                    }
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
                    var stream = AveSender.BackupStream;
                    stream.BeginWriteMetadata();
                    try
                    {
                        AveSPSiteInfo aveSPSiteInfo = new AveSPSiteInfo(aveSite);
                        aveSPSiteInfo.Export(stream);
                        //aveSite.ExportBaseInfo(stream);

                        #region 精简备份，不备份其他信息
                        //AveSPSiteFeature featureManager = new AveSPSiteFeature(aveSite);
                        //featureManager.Export(stream);
                        ////aveSite.ExportFeatures(stream);

                        //AveSPSiteSettingInfo aveSPSiteSettingInfo = new AveSPSiteSettingInfo(aveSite);
                        //aveSPSiteSettingInfo.Export(stream);
                        ////aveSite.ExportSettings(stream);

                        ////Search service is not supported
                        ////aveSite.ExportSearchInfo(stream);

                        //AveLanguage aveSPSiteLanguageInfo = AveLanguage.CreateInstance(aveSite);
                        //aveSPSiteLanguageInfo.Export(stream);
                        ////aveSite.ExportLanguageInfo(stream);

                        AveUser users = AveUser.CreateInstance(aveSite);
                        users.Export(stream, true);
                        //aveSite.ExportUsers(stream);

                        //AveGroup groups = AveGroup.CreateInstatnce(aveSite);
                        //groups.Export(stream, true);
                        ////aveSite.ExportGroups(stream);

                        //try
                        //{
                        //    if (aveSite.SPContextKind == AveContextKind.Server13ObjectModel && AveEnv.IsMoss)
                        //    {
                        //        if (aveSite.IsMySite)
                        //        {
                        //            AveSPUserProfile userProfile = new AveSPUserProfile(aveSite, aveSite.SPSite.Owner.LoginName);
                        //            userProfile.Export(stream);
                        //        }
                        //        else
                        //        {
                        //            AveSPUserProfile userProfile = new AveSPUserProfile(aveSite, users.GetUsers());
                        //            userProfile.Export(stream);
                        //        }
                        //    }
                        //    //aveSite.ExportUserProfiles(stream, true);
                        //}
                        //catch (Exception ex)
                        //{
                        //    mLog.Error("Backup UserProfile Error :{0}", ex.ToString());
                        //}
                        //try
                        //{
                        //    if (Configuration.BackupRequest.IncludeTerm)
                        //    {
                        //        AveMetadataService metadataService = new AveMetadataService(aveSite.SPSite);
                        //        metadataService.Export(stream);
                        //    }
                        //    else
                        //    {
                        //        mLog.Warn("Skip Backup Metadata Services ");
                        //    }
                        //}
                        //catch (Exception mx)
                        //{
                        //    mLog.Error("Backup Metadata Error :{0}", mx.ToString());
                        //}
                        #endregion
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
                        aveSite.ExportFullTextIndex(stream, fullText);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup Site Collection Metadata Error: {0}", e.ToString());
                        status = JobDetailsStatus.Failed;
                        throw;
                    }
                    finally
                    {
                        AveSender.BackupStream.EndWriteMetadata();
                        AveSender.BackupStream.FlushMetadata(0);
                        AveSender.BackupTail(status == JobDetailsStatus.Successful);
                        current.BackupStatus = FileHeaderStatus.Complete;
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Site Collection Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            finally
            {
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsSiteCollectionBackupArchiveLevel, entity.ArchiveLevel.ToString());
                //Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
            }
            return 0;
        }
    }

    internal class RelativeDataWebBackup : SPObjectBackup
    {
        public RelativeDataWebBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataWebBackup.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsWebBackupbackup);
            string errorMessage = string.Empty;
            string realFullPath = entity.FullPath;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            var tail = new StringBuilder();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.WebBackup"))
                {
                    var aveSite = (CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection) as CacheNode).WrapperObject as AveSPSite;
                    AveSPWeb aveWeb = new AveSPWeb(aveSite, new Guid(entity.NodeId), entity.LeafName);
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
                    current.FileHeader = AveSender.BackupHeader(realFullPath);
                    var stream = AveSender.BackupStream;
                    AveSender.BackupStream.BeginWriteMetadata();
                    try
                    {
                        AveSPWebInfo webInfo = new AveSPWebInfo(aveWeb);
                        webInfo.Export(stream);
                        //aveWeb.ExportBaseInfo(stream);

                        #region 精简备份，不备份其他信息
                        //AveSPWebFeature featureManager = new AveSPWebFeature(aveWeb);
                        //featureManager.Export(stream);
                        ////aveWeb.ExportFeatures(stream);

                        //AveSPWebSettingInfo webSettingInfo = new AveSPWebSettingInfo(aveWeb);
                        //webSettingInfo.Export(stream);
                        ////aveWeb.ExportSettings(stream);

                        //AveLanguage webLanguageInfo = AveLanguage.CreateInstance(aveWeb);
                        //webLanguageInfo.Export(stream);
                        ////aveWeb.ExportLanguageInfo(stream);
                        //mLog.Info("Start to export fields");
                        //aveWeb.ExportFields(stream);
                        //mLog.Info("End to export fields");
                        //aveWeb.ExportContentTypes(stream);

                        //AveSPEventReceiver eventReceiver = AveSPEventReceiver.CreateInstance(aveWeb);
                        //eventReceiver.Export(stream);
                        ////aveWeb.ExportEventReceivers(stream);

                        ////Search service is not supported
                        ////aveWeb.ExportSearchInfo(stream);

                        //AveSPSocialTag socialTags = new AveSPSocialTag(aveWeb.SPWeb.Url + "/", aveSite);
                        //socialTags.Export(stream);
                        ////aveWeb.ExportSocialTags(stream);

                        //AveSPSocialComment socialComments = new AveSPSocialComment(aveWeb.SPWeb.Url + "/", aveSite);
                        //socialComments.Export(stream);
                        ////aveWeb.ExportSocialComments(stream);
                        //#region MicroFeed List
                        ////aveWeb.ExportSocialFeeds(stream);
                        ////try
                        ////{
                        ////    mLog.Debug("Get Feed Cache");
                        ////    //dd hang at here because of a bug
                        ////    this.MicroFeedCache = aveWeb.GetMicroFeedCache();
                        ////    mLog.Debug("Get Feed Cache End");
                        ////}
                        ////catch (Exception ex)
                        ////{
                        ////    mLog.Warn("Some exception occurred during backing up the social feeds:" + ex.ToString());
                        ////}
                        //#endregion

                        //AveSPNavigation navigation = new AveSPNavigation(aveWeb);
                        //navigation.Export(stream);
                        ////aveWeb.ExportNavigation(stream);

                        //AveUser user = AveUser.CreateInstance(aveWeb);
                        //user.Export(stream, true);

                        //AveGroup group = AveGroup.CreateInstatnce(aveWeb);
                        //group.Export(stream);
                        ////aveWeb.ExportUsers(stream);
                        ////aveWeb.ExportGroups(stream);
                        //if (aveWeb.SPWeb.HasUniqueRoleDefinitions || aveWeb.SPWeb.HasUniqueRoleAssignments)
                        //{
                        //    AveRoles roles = new AveRoles(aveWeb);
                        //    roles.Export(stream);
                        //    //aveWeb.ExportRoles(stream);
                        //}
                        //if (aveWeb.SPWeb.HasUniqueRoleAssignments)
                        //{
                        //    AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveWeb);
                        //    roleAssignments.Export(stream);
                        //    //aveWeb.ExportRoleAssignments(stream);
                        //}
                        ////TODO:Not supported in DocAve Online
                        ////try
                        ////{
                        ////    aveWeb.ExportPolicy(stream);
                        ////}
                        ////catch (Exception ex)
                        ////{
                        ////    mLog.Warn("Some exception occurred during backing up the policy:" + ex.ToString());
                        ////}
                        //if (SPWorkflowProcessorRuntime.ProcessAssociation)
                        //{
                        //    aveWeb.ExportWorkflows(stream);
                        //}
                        #endregion
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
                        aveWeb.ExportFullTextIndex(AveSender.BackupStream, fullText);
                        current.BackupStatus = FileHeaderStatus.Complete;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup Web Metadata Error: {0}", e.ToString());
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
                        AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                    }

                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Web Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                throw;

            }
            finally
            {
                //Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
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

    internal class RelativeDataListBackup : SPObjectBackup
    {
        public RelativeDataListBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataListBackup.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
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
            var tail = new StringBuilder();
            HeaderUrl headUrl = new HeaderUrl();
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.ListBackup"))
                {
                    var aveWeb = parent.WrapperObject as AveSPWeb;
                    var aveList = new AveSPList(aveWeb, new Guid(entity.NodeId), entity.LeafName, true);
                    // string nameForSpecialChar = AveConverter.EncodeSpecialChar(entity.LeafName);
                    //System Folder  SPList ==null
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
                    AveSender.BackupListHeader(aveList, AveSender.BackupStream.StreamTransfered, entity, ruleName, subJobId, mediaName, entity.LeafName, listType, headUrl.GetUrlBySR(aveWeb.SPWeb.Site.Url, aveWeb.SPWeb.Site.ServerRelativeUrl, aveList.ServerRelativeUrl));
                    current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));

                    current.WrapperObject = aveList;
                    var stream = AveSender.BackupStream;
                    stream.BeginWriteMetadata();

                    try
                    {
                        if (!aveList.IsSystemList)
                        {
                            var listInfo = new AveSPListInfo(aveList);
                            listInfo.Export(stream);
                            //aveList.ExportBaseInfo(stream);
                            #region 精简备份，不备份其他信息
                            //var listSettingInfo = new AveSPListSettingInfo(aveList);
                            //listSettingInfo.Export(stream);
                            ////aveList.ExportSettings(stream, false);

                            //aveList.ExportFields(stream, false);
                            //aveList.ExportContentTypes(stream);

                            //AveSPEventReceiver eventReceiver = AveSPEventReceiver.CreateInstance(aveList);
                            //eventReceiver.Export(stream);
                            ////aveList.ExportEventReceivers(stream);

                            //if (aveList.SPList.HasUniqueRoleAssignments)
                            //{
                            //    AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveList);
                            //    roleAssignments.Export(stream);
                            //    //aveList.ExportRoleAssignments(stream, false);
                            //}

                            //AveSPAlert listAlert = AveSPAlert.CreateInstance(aveList);
                            //listAlert.Export(stream);
                            ////TODO:List policy is not supported
                            ////aveList.ExportAlerts(stream);

                            ////try
                            ////{
                            ////    aveList.ExportPolicy(stream);
                            ////}
                            ////catch (Exception ex)
                            ////{
                            ////    mLog.Warn("Failed to backup list policy. reason: {0}", ex.Message);
                            ////}
                            //if (SPWorkflowProcessorRuntime.ProcessAssociation)
                            //{
                            //    aveList.ExportWorkflows(stream);
                            //}
                            #endregion
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
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Backup List Metadata Error: {0}", e.ToString());
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
                            xe.InnerXml = "Description:" + EncodingStringUsingBase64(aveList.SPList.Description);
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
                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup List Error: {0}", e.ToString());
                errorMessage = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            finally
            {
                AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                //Configuration.ProgressDto.UpdateProgress(true);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
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

    internal class RelativeDataFolderBackup : SPObjectBackup
    {
        public RelativeDataFolderBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataFolderBackup.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {

            //mLog.Debug(LOGRESOURCE.StorageOptimization13_SOARBackupImpsFolderBackupbackup);
            string errorMessage = string.Empty;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            bool mIsRootFolder = false;

            if (entity.LeafName.IndexOf(':') >= 0)
            {
                entity.FullPath = entity.FullPath.Substring(0, entity.FullPath.Length - entity.LeafName.IndexOf(':')) + entity.LeafName;
            }

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
                using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.FolderBackup"))
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
                            var stream = AveSender.BackupStream;
                            stream.BeginWriteMetadata();
                            try
                            {
                                aveFolder.ExportDocInfo(stream);
                                aveFolder.ExportUserDataInfo(stream);
                                aveFolder.ExportDataJunctionInfo(stream);
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
                                mLog.Error("Backup Folder Version Error: {0}", e.ToString());
                                throw;
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error("Backup Folder Version Error: {0}", e.ToString());
                            //if (e is ExportServiceException)
                            //{
                            //    status = JobDetailsStatus.Skipped;
                            //}
                            //else
                            //{
                            status = JobDetailsStatus.Failed;
                            //}
                            throw;
                        }
                        finally
                        {
                            AveSender.BackupStream.EndWriteMetadata();
                            AveSender.BackupStream.FlushMetadata(0);
                            SetFileExtraInfo(entity.LibRowId, entity.LeafName, entity.LeafName, fileatrrinfo);
                            AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);

                        }
                    }
                    else
                    {
                        if (mIsRootFolder)
                        {
                            aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPList);
                            current.IsRootFolder = true;
                            current.WrapperObject = aveFolder;
                            //Do not backup list root folder ,we well backup it in list properties
                            return 0;
                        }
                        else
                        {
                            aveFolder = new AveSPFolder(((CacheNode)parent).WrapperObject as AveSPFolder, entity.LeafName, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion);
                        }
                        current.WrapperObject = aveFolder;
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
                            var stream = AveSender.BackupStream;
                            stream.BeginWriteMetadata();
                            try
                            {
                                aveFolder.ExportDocInfo(stream);
                                aveFolder.ExportUserDataInfo(stream);
                                aveFolder.ExportDataJunctionInfo(stream);

                                if (aveFolder.AveItem.HasUniqueRoleAssignments)
                                {
                                    AveRoleAssignments roleAssignmetns = AveRoleAssignments.CreateInstance(aveFolder.AveItem);
                                    roleAssignmetns.Export(stream);
                                    //aveFolder.AveItem.ExportRoleAssignments(stream, false);
                                }
                                //aveFolder.AveItem.ExportWorkflowInstance(stream);
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
                                mLog.Error("Backup Folder Metadata Error: {0}", e.ToString());
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
                            mLog.Error("Backup Folder Error: {0}", e.ToString());
                            errorMessage = e.Message.ToString();
                            //if (e is ExportServiceException)
                            //{
                            //    status = JobDetailsStatus.Skipped;
                            //}
                            //else
                            //{
                            status = JobDetailsStatus.Failed;
                            //}
                            status = JobDetailsStatus.Failed;
                            current.BackupStatus = FileHeaderStatus.Failed;
                            throw;
                        }
                        finally
                        {
                            AveSender.BackupTail(tail.ToString(), status == JobDetailsStatus.Successful);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Folder or Folder Version Error: {0}", e.ToString());
                errorMessage = e.Message;
                status = JobDetailsStatus.Failed;
                current.BackupStatus = FileHeaderStatus.Failed;
                current.DoDelete = false;
                if (Configuration?.ProgressDto != null)
                {
                    mLog.Error("[BackupAsync][Exception]Backup Folder or Folder Version Failed.");
                    Configuration.ProgressDto.HasErrorNode = true;
                }
                throw;
            }
            finally
            {
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

    internal class RelativeDataItemBackup : SPObjectBackup, IMultiBackup
    {
        //JobDetailsStatus status = JobDetailsStatus.Successful;
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        public RelativeDataItemBackup(AveLogger log)
        {
            mLog = log;
        }

        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataItemBackup.ProcessBackedNode should not reach");
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
                using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.ItemBackup"))
                {
                    //mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsItemBackupbackup, entity.FullPath);
                    switch (entity.NodeType)
                    {
                        case (int)ArchiverCommon.ItemType.DOCUMENT:
                        case (int)ArchiverCommon.ItemType.DOCUMENT_VER:
                            {
                                BackupDocumentOrDocumentVersion(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender);
                                break;
                            }
                        case (int)ArchiverCommon.ItemType.ITEM_TYPE:
                        case (int)ArchiverCommon.ItemType.ITEM_VERSION:
                            {
                                BackupItemOrItemVersion(parent, current, entity, ruleName, subJobId, fileatrrinfo, ruleLevel, mediaName, AveSender);
                                break;
                            }
                    }
                    current.BackupStatus = FileHeaderStatus.Complete;
                    try
                    {
                        fullPath = entity.FullPath;
                        itemTitle = entity.LeafName;
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
                if (string.IsNullOrEmpty(itemTitle))
                {
                    itemTitle = entity.LeafName;
                }
                mLog.Error("Backup Item Error: {0}", ex1.ToString());
                errorMessage = AveWrapperHandleErrorMessage.GetFormateErrorMessage(ex1.Key, ex1.Message, ex1.Args.ToArray());
                //if (ex is ExportServiceException)
                //{
                //    status = JobDetailsStatus.Skipped;
                //}
                //else
                //{
                status = JobDetailsStatus.Failed;
                //}
                current.BackupStatus = FileHeaderStatus.Failed;
                string defaultValue = string.Format(ex1.Message, ex1.Args);
            }
            catch (FileContentLengthNullException fe)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                if (string.IsNullOrEmpty(itemTitle))
                {
                    itemTitle = entity.LeafName;
                }
                mLog.Error("Backup Item FileContentLengthNullException: {0}", fe.ToString());
                status = JobDetailsStatus.Skipped;
                current.BackupStatus = FileHeaderStatus.Failed;
                errorMessage = "RM_JM_Detail_SkipBackup0KBFile";
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = entity.FullPath;
                }
                if (string.IsNullOrEmpty(itemTitle))
                {
                    itemTitle = entity.LeafName;
                }
                mLog.Error("Backup Item Error: {0}", ex.ToString());
                errorMessage = ex.Message.ToString();
                //if (ex is ExportServiceException)
                //{
                //    status = JobDetailsStatus.Skipped;
                //}
                //else
                //{
                status = JobDetailsStatus.Failed;
                //}
                current.BackupStatus = FileHeaderStatus.Failed;
                throw;
            }
            finally
            {
                //Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                if (current.FileHeader != null)
                {
                    current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                    current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                }
                JobDetail detail = new JobDetail()
                {
                    SubJobId = Configuration.JobId,
                    Type = entity.CacheNodeType.ToString(),
                    SrcURL = Configuration.GetNodeFullPath(fullPath),
                    Size = AveSender.BackupStream.StreamTransfered,
                    Status = current.FileHeader == null ? (int)JobDetailsStatus.Skipped : (int)status,
                    Remark12 = "Backup",
                    Message = errorMessage
                };
                //JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                AddRelativeDataDetail(AveSender.BackupStream.StreamTransfered, itemTitle, Configuration.GetNodeFullPath(fullPath), entity.CacheNodeType, status, errorMessage);
                //Configuration.relativeDataJobReportOperation.AddDetail(detail);
            }
            return 0;
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

                    using (AvePerformanceScope pc1 = new AvePerformanceScope("RelativeData.Backup.AveSPListItem.ExportWorkflowInstance"))
                    {
                        AveWorkflow workflow = new AveWorkflow();
                        workflow.ExportWorkflowInstance(stream, aveListItem.AveSPItem);
                    }
                    //add for micro feed archive,针对Micro Feed Item,在类型为Post对应的Item中备份Feed对象,包含整个Post及其Reply信息.
                    if (aveListItem.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.MicroFeed)
                    {
                        mLog.Info("Microfeed item is skipped");
                        //int id = Convert.ToInt32(aveListItem.AveSPItem.SPListItem["ID"]);
                        //if (MicroFeedCache.ContainsKey(id))
                        //{
                        //    if ((MicroBlogType)aveListItem.AveSPItem.SPListItem["MicroBlogType"] == MicroBlogType.Post)
                        //    {

                        //        try
                        //        {
                        //            aveListItem.ExportSingleSocialFeedForArchiver(stream, MicroFeedCache[id]);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            mLog.Error("This Item does not Exist in Micro Feed Cache" + ex.ToString());
                        //            throw;
                        //        }
                        //    }
                        //    AddFullTextColumnForFeedItem(ref fullText, MicroFeedCache[id]);
                        //}
                        //else
                        //{
                        //    mLog.Info("MicroFeedCache does not contain the SPListItem ID" + id);
                        //}
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

        /*[SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        private void AddFullTextColumnForFeedItem(ref Dictionary<string, object> fullText, object o)
        {
            string post = string.Empty;
            string likes = string.Empty;
            string mentions = string.Empty;
            string tags = string.Empty;
            string replyNames = string.Empty;
            //if (o is AveSocialFeedInfo)
            //{
            //    AveSocialFeedInfo feedPost = (AveSocialFeedInfo)o;
            //    post = feedPost.PostName;
            //    likes = feedPost.Likers;
            //    mentions = feedPost.Mentions;
            //    tags = feedPost.Tags;
            //    replyNames = feedPost.ReplyNames;
            //}
            //else if (o is AveSocialFeedReplyInfo)
            //{
            //    //Reply has no reply
            //    AveSocialFeedReplyInfo feedReply = (AveSocialFeedReplyInfo)o;
            //    post = feedReply.PostName;
            //    likes = feedReply.Likers;
            //    mentions = feedReply.Mentions;
            //    tags = feedReply.Tags;
            //}
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

                if (!aveDoc.AveSPItem.IsVersion)
                {
                    AveSPAlert alerts = AveSPAlert.CreateInstance(aveDoc);
                    alerts.Export(stream);
                    //aveDoc.ExportAlerts(stream, true, true);

                    if (aveDoc.AveSPItem.HasUniqueRoleAssignments)
                    {
                        AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(aveDoc.AveSPItem);
                        roleAssignments.Export(stream);
                        //aveDoc.AveSPItem.ExportRoleAssignments(stream, false);
                    }

                    AveWorkflow workflow = new AveWorkflow();
                    workflow.ExportWorkflowInstance(stream, aveDoc.AveSPItem);
                    //aveDoc.AveSPItem.ExportWorkflowInstance(stream);
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

        private void BackupDocumentOrDocumentVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            JobDetailsStatus status = JobDetailsStatus.Successful;
            if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER && entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
            {
                throw new Exception(LOGRESOURCE.StorageOptimization13_SOARBackupImpsItemBackupException);
            }
            string realName = entity.LeafName;
            int index = realName.IndexOf(':');
            if (index >= 0)
            {
                //entity.FullPath = entity.FullPath.Substring(0, entity.FullPath.Length - index) + realName;
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
            if (parentFolder != null)
            {
                IAveFile file = parentFolder.AveList.ParentWeb.SPWeb.GetFile(new Guid(entity.NodeId), parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
                if (!file.Exists)
                {
                    mLog.Info("Current document doesn't exist in SharePoint when BackupDocumentOrDocumentVersion.File:{0}.", entity.LibRowId);
                    return;
                }
            }
            var aveSite = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.SiteCollection).WrapperObject as AveSPSite;
            var aveDoc = new AveSPDoc(parentFolder, new Guid(entity.NodeId), entity.LibRowId, entity.UIVersion, parentFolder?.ServerRelativeUrl.TrimEnd('/') + "/" + realName);
            current.WrapperObject = aveDoc;
            aveDoc.AveSPItem.IsBackupLinkForArchivedData = false;
            HeaderUrl headerUrl = new HeaderUrl();
            if (!entity.DoDelete && ruleLevel == (int)PolicyLevel.DocumentVersion)//version rule 不需要备份current version，DoDelete为是否符合rule的条件
            {
                current.IsCurrentVersion = true;
                return;
            }
            try
            {
                //AvePerformanceTimerPool.Start("Archiver_DocumentBackup");
                //add for RevIM export
                if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                {
                    aveDoc.AveSPItem.UserDataCache = aveDoc.AveSPItem.GetUserData();
                    ItemVault itemVault = (ItemVault)VaultExport;
                    itemVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                    itemVault.ExportVaultDocument(aveDoc, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName, true);
                }
                if (entity.NodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    AveSender.BackupDocumentHeader(aveDoc, parent, AveSender.BackupStream.StreamTransfered, entity, parentFolder, ruleName, subJobId, mediaName, headerUrl.GetDocumentAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, parentFolder?.SPFolder.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType());
                }
                else
                {
                    AveSender.BackupDocumentVersionHeader(aveDoc, parent, AveSender.BackupStream.StreamTransfered, entity, parentFolder, ruleName, subJobId, mediaName, headerUrl.GetDocumentVersionAP(parentFolder?.SPFolder.ParentWeb.Site.Url, parentFolder?.SPFolder.ParentWeb.Site.ServerRelativeUrl, entity.UIVersion, parentFolder?.SPFolder.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType());
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
            }
            catch (FileContentLengthNullException)
            {
                status = JobDetailsStatus.Skipped;
                throw;
            }
            catch (Exception e)
            {
                status = JobDetailsStatus.Failed;
                mLog.Error("Backup Document or Document Version Error: {0}", e.ToString());
                //Configuration.ProgressDto.HasErrorNode = true;
                throw;
            }
            finally
            {
                string tail = fileatrrinfo.ToString();
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupDocumentOrDocumentVersionInfo, entity.NodeId);
                AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
                //AvePerformanceTimerPool.Stop("Archiver_DocumentBackup");
            }
        }

        private void BackupItemOrItemVersion(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, FileAtrributeInfo fileatrrinfo, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            JobDetailsStatus status = JobDetailsStatus.Successful;
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
            if (aveListItem.AveSPItem.SPListItem == null)
            {
                mLog.Info("This SPListItem does not exist in BackupItemOrItemVersion, ItemTitle : {0}", aveListItem.AveSPItem.Title);
                return;
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
                    itemVault.VaultExportItem(aveListItem, (int)entity.CacheNodeType, entity.FullPath, subJobId, ruleName, mediaName, true);
                }

                current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(entity.FullPath));
                BackupListItem(aveListItem, AveSender);
                if (!aveListItem.AveSPItem.IsSystemFileOrFolder)
                {
                    SetItemAttributes(aveListItem.AveSPItem, aveListItem.AveSPItem.GetAllColumnValues(ColumnsLevel.DisplayColumns), aveListItem.AveSPItem.SPListItem.Title, entity, fileatrrinfo, Configuration.tagInfoCollection);   //SAAS-10847 使用正确的ListItem的Tile （原来为aveListItem.AveSPItem.Title）
                }
            }
            catch (Exception e)
            {
                mLog.Error("Backup Item or Item Version Error: {0}", e.ToString());
                status = JobDetailsStatus.Failed;
                //Configuration.ProgressDto.HasErrorNode = true;
                throw;
            }
            finally
            {
                string tail = fileatrrinfo.ToString();
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARSOArchiverBeforeTail, tail);
                AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
            }
        }

        private void AddRelativeDataDetail(long size, string name, string fullPath, int type, JobDetailsStatus status, string comment)
        {
            if (Configuration.RelativeDataJobSourceFlag == (int)SourceFlag.Physical)
            {
                //RM_JS_JM_Related_DeleteRelatedFailed                
                //SendPhysicalJobDetail(name, fullPath, PhysicalDisposalActionType.Disposal, String.Empty, ArchiverTypeConvert.ConvertNodeLevelToI18n(type), status, comment);
            }
            else
            {
                SendSPJobDetail(size, fullPath, type, status, comment);
            }
        }

        public void SendPhysicalJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }
        public void SendSPJobDetail(long nodeSize, string originPath, int cacheNodeType, JobDetailsStatus status, string comment = "")
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = originPath;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = Configuration.currentRule.Name;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Backup;
            //mArchiverActionJobDetails.Action = "Delete";
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = comment;
            JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(Configuration.currentRule, nodeSize, cacheNodeType, status);
            ReportManager.SendJobDetail(mArchiverActionJobDetails);
        }
        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }

    }

    internal class RelativeDataAttachmentBackup : SPObjectBackup
    {
        public RelativeDataAttachmentBackup(AveLogger log)
        {
            mLog = log;
        }
        public override async Task<int> RepeatProcessContainerNode(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            mLog.Warn($"RelativeDataAttachmentBackup.ProcessBackedNode should not reach");
            return (int)JobDetailsStatus.Successful;
        }

        public override async Task<int> BackupAsync(CacheNode parent, CacheNode current, ArchiveApproveReport entity, string ruleName, string subJobId, int ruleLevel, string mediaName, BackupInfoSender AveSender)
        {
            string errorMessage = string.Empty;
            string itemTitle = string.Empty;
            string fullPath = entity.FullPath;
            AveSPItem parentNode = null;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            FileAtrributeInfo fileatrrinfo = new FileAtrributeInfo();
            HeaderUrl headerUrl = new HeaderUrl();
            var aveList = CacheSPObjs.ValueInCacheOfLevel((int)CacheNodeType.List).WrapperObject as AveSPList;
            bool isFolder = false;
            using (AvePerformanceScope pc = new AvePerformanceScope("RelativeData.ArchiverBackup.AttachmentBackup"))
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
                    //if (!string.IsNullOrEmpty(itemTitle) && !isFolder)
                    //{
                    //    //fullPath = entity.FullPath.Substring(0, entity.FullPath.IndexOf('\\') + 1) + itemTitle + "\\" + entity.LeafName.Substring(entity.LeafName.LastIndexOf(':') + 1);
                    //}
                    //else if (parentNode.AveSPList.SPList.BaseTemplate == AveListTemplateType.DiscussionBoard && !isFolder)
                    //{
                    //    //fullPath = entity.FullPath.Substring(0, entity.FullPath.IndexOf('\\') + 1) + entity.LeafName.Replace(':', '\\');
                    //}
                    //else
                    //{
                    //    //fullPath = entity.FullPath;
                    //}
                }
                catch (Exception e)
                {
                    mLog.Error("Attachment Full Path Assignment Error: {0}", e.ToString());
                    fullPath = entity.FullPath;
                }

                //Office 365 Must give ServerRelativeUrl to CreeateAveSPAttachment
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
                //var aveAttachemnt = mFactory.CreateAveSPAttachment(CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder, new Guid(entity.NodeId), entity.LeafName, serverUrl, parentNode);
                var aveAttachemnt = new AveSPAttachment(CacheSPObjs.ParentValueInCacheOfLevel((int)CacheNodeType.Item).WrapperObject as AveSPFolder, new Guid(entity.NodeId), entity.LeafName, serverUrl);
                aveAttachemnt.AveSPItem.IsBackupLinkForArchivedData = false;
                try
                {
                    //add for RevIM export
                    if (VaultBeforeArcInfo != null && VaultBeforeArcInfo.VaultExport != null && VaultBeforeArcInfo.VaultExportPathGenerator != null)
                    {
                        AttachmentVault attachmentVault = (AttachmentVault)VaultExport;
                        attachmentVault.VaultBeforeArcInfo = VaultBeforeArcInfo;
                        attachmentVault.VaultExport(entity.CacheNodeType, aveAttachemnt, fullPath, subJobId, ruleName, mediaName, true);
                    }
                    AveSender.BackupAttaHeader(parentNode, entity, AveSender.BackupStream.StreamTransfered, ruleName, subJobId, mediaName, headerUrl.GetAttachmentAP(aveSite.SPSite.Url, aveSite.SPSite.ServerRelativeUrl, aveAttachemnt.AveSPItem.AveSPList.ServerRelativeUrl, entity.LeafName), Configuration.GetBackupFileType());
                    current.FileHeader = AveSender.BackupHeader(Configuration.GetNodeFullPath(fullPath));
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
                            #endregion
                        }
                    }
                    current.BackupStatus = FileHeaderStatus.Complete;
                }
                catch (Exception e)
                {
                    mLog.Error("Backup Attachment Error: {0}", e.ToString());
                    errorMessage = e.Message.ToString();
                    //if (e is ExportServiceException)
                    //{
                    //    status = JobDetailsStatus.Skipped;
                    //    //vaultState = new ExportStatus() { ErrorMessage = e.Message.ToString(), State = ExportState.Failed };
                    //}
                    //else
                    //{
                    status = JobDetailsStatus.Failed;
                    //}
                    current.BackupStatus = FileHeaderStatus.Failed;
                    throw;
                }
                finally
                {
                    string tail = fileatrrinfo.ToString();
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARBackupImpsBackupInfo, tail);
                    JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(entity, AveSender.BackupStream.StreamTransfered, Configuration.currentRule, status);
                    AveSender.BackupTail(tail, status == JobDetailsStatus.Successful);
                    //reportDto.Type = AveConstants.TYPE_LISTITEM;
                    //Configuration.ProgressDto.UpdateProgress(entity.ArchiveLevel != -1);
                    if (current.FileHeader != null)
                    {
                        current.FileHeader.SetAttribute(KeyWord.URL, fullPath);
                        current.FileHeader.SetAttribute(KeyWord.SIZE, AveSender.BackupStream.StreamTransfered.ToString());
                    }
                    JobDetail detail = new JobDetail()
                    {
                        SubJobId = Configuration.JobId,
                        Type = entity.CacheNodeType.ToString(),
                        SrcURL = Configuration.GetNodeFullPath(fullPath),
                        Size = AveSender.BackupStream.StreamTransfered,
                        Status = (int)status,
                        Remark12 = "Backup",
                        Message = errorMessage
                    };
                    //Configuration.RelativeDataJobReportOperation.AddDetail(detail);
                }
                return 0;
            }
        }
    }
}
