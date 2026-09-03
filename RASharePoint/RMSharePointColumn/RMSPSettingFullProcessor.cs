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
using Aspose.Email.Storage.Pst;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.RelatedRecords;
using AvePoint.RA.SharePoint.RMSharePointColumn.Base;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Discovery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class SPSettingFullProcessor : BaseSPSettingProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SPSettingFullProcessor));
        //private RMSharePointSetting curSetting; 
        protected IAveSiteProperties curSiteProperties;
        protected BaseJobDto mBaseJobDto;
        //private SPOLabelUtility mLabelUtility = null;
        private RMScope scopeInfo;
        protected bool mSupportLockedSite;
        private readonly bool mEnableLifecycleManagementForSharePointLists;
        protected DeferredDisposalScope mDeferredDisposalScope = new ();

        public SPSettingFullProcessor(RMSharePointSetting setting, SPTreeNodeDto nodeInfo, long settingTime, BaseJobDto jobDto, SPOLabelUtility labelUtility, bool supportLockedSite, bool enableLifecycleManagementForSharePointLists) : base(nodeInfo, jobDto, labelUtility)
        {
            curSetting = setting;
            curNodeInfo = nodeInfo;
            mBaseJobDto = jobDto;
            mLabelUtility = labelUtility;
            mSupportLockedSite = supportLockedSite; //CheckSupportLockedSite(setting.NodeInfo);
            mEnableLifecycleManagementForSharePointLists = enableLifecycleManagementForSharePointLists;
            var groupId = Guid.Empty;
            //if (Guid.TryParse(groupNode.ID, out groupId))
            //{
            //    RMSharePointSetting gSetting = SharePointSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
            //    if (gSetting != null)
            //    {
            //        curSetting.IncludeDeclaredRecords = gSetting.IncludeDeclaredRecords;
            //        logger.Info("set include declared records by group:{0}", gSetting.FullPath);
            //    }
            //}
            logger.Info($"SPSettingFullProcessor start to process [{curNodeInfo.FullPath}]");
        }

        private bool CheckSupportLockedSite(string nodeInfo)
        {
            try
            {
                RMSPTreeNode rMSPTree = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeInfo);
                return rMSPTree.SupportLockedSite;
            }
            catch (Exception ex)
            {
                logger.Warn($"Process locked site collection error: {ex}");
            }
            return false;
        }

        protected SiteStateTransitionScope TryUnlockSiteCollection(AveObjectModelFactory aveObjectModelFactory)
        {
            logger.Info($"Try to unlock site collection {siteNode.FullPath}, mSupportLockedSite: {mSupportLockedSite}.");
            SiteStateTransitionScope scope = new SiteStateTransitionScope(siteNode.FullPath, aveObjectModelFactory, SiteState.Unlock);
            if (mSupportLockedSite)
            {
                scope.TryConvertToTargetStatus();
            }
            else if (scope.TryGetSiteProperties(out IAveSiteProperties siteProps) && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) < SiteState.Unlock)
            {
                curNodeInfo.Title = siteProps.Title;
                curSetting.FullPath = siteProps.Url;
                throw new AveSkipLockSiteException("RM_AR_Restore_SiteLocked_ErrorMessage");
            }
            return scope;
        }

        public override async System.Threading.Tasks.Task ProcessSiteCollectionAsync()
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessSiteCollection", $"RMSPSettingFullProcessor.ProcessSiteCollection{siteNode.FullPath}", true))
            {
                try
                {
                    using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(curNodeInfo.NodeExtension.BposInfo, curNodeInfo.FullPath);
                        var bposInfo = await GetBposInfoBySiteAsync(curNodeInfo.FullPath);
                        IAveSite aveSite;
                        using (var siteScope = new PerformanceScope("RMSPSettingFullProcessor.CreateObjectModelFactorySite", $"CreateObjectModelFactorySite{siteNode.FullPath}", true))
                        {
                            mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                            mDeferredDisposalScope.Add(TryUnlockSiteCollection(mfactory));
                            try
                            {
                                aveSite = mfactory.CreateSite(siteNode.FullPath);
                                mLabelUtility.CacheSPLabel(aveSite);
                                SetLanguage(aveSite);
                            }
                            catch (Exception e)
                            {
                                var we = e.InnerException as WebException;
                                if (we != null)
                                {
                                    if (we.Status == WebExceptionStatus.ProtocolError)
                                    {
                                        var httpResp = (we.Response as HttpWebResponse);
                                        if (httpResp != null)
                                        {
                                            if (httpResp.StatusCode == HttpStatusCode.NotFound)
                                            {
                                                logger.Error("[DirtyData] SiteCollection {0} is deleted, error: {1}", siteNode.FullPath, e.ToString());
                                                //base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty, string.Empty, string.Empty, JobReportDetailStatus.Failed, "RM_SS_SiteRemovedFromDAO");
                                                return;
                                            }
                                        }
                                    }
                                }
                                throw;
                            }
                            curRecords = mfactory.CreateRecords();
                            base.SetModuleFactoryForAuto(mfactory);
                        }
                        var mTotalWebs = aveSite.AllWebs.Count;
                        ReportManager.IncreaseBase(mTotalWebs);

                        if (JobContext.IsCSDTenant)
                        {
                            mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                        }
                        using (aveSite)
                        {
                            curSite = aveSite;
                            CacheSiteScope(curSite);
                            AveDiscoverSite tmpDiscoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                            /* Add logic for RECO-3843 start*/
                            try
                            {
                                IAveTenant tenant = mfactory.CreateTenantCompatibleGeo(bposInfo, siteNode.FullPath);
                                curSiteProperties = tenant.GetSitePropertiesByUrl(siteNode.FullPath);
                            }
                            catch (Exception e)
                            {
                                logger.Info($"Init current site properties failed {siteNode.FullPath}:{e}");
                            }
                            /* Add logic for RECO-3843 end*/

                            base.DoSettingAction(curSite, curSiteProperties);
                            var allDiscoverWebs = tmpDiscoverSite.GetWebs().Values?.ToList() ?? new List<AveDiscoverWeb>();
                            var webHierarchyRoot = RestoreWebHierarchyByParentWebId(allDiscoverWebs);
                            //****ProgressService.IncreaseBase(allDiscoverWebs.Count);
                            await ProcessWebHierarchyAsync(webHierarchyRoot);
                            try
                            {
                                allDiscoverWebs.FirstOrDefault(w => w?.AveWeb?.IsRootWeb == true)?.Dispose();
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Dispose root web error:{e}");
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    isFailedAddBCS = true;
                    isFailedAddContainer = true;
                    logger.Error("Process sitecollection error {0}", e.ToString());
                    if (e.Message.Equals("RM_APP_AppProfileNotAvailable"))
                    {
                        isFailedApps = true;
                    }
                    //TO DO Add Detail
                    base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                        string.Empty, string.Empty, JobReportDetailStatus.Failed, e.Message);//TO DO I18N
                }
                finally
                {
                    await base.ProcessSiteCollectionAsync();
                    RMMachineLearningDataSyncManager.Commit();
                }
            }
        }

        public void SetLanguage(IAveSite site)
        {
            try
            {
                var ci = site.RootWeb.LanguageCulture;
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch (Exception e)
            {
                logger.Info($"Set language failed {e.ToString()}");
            }
        }
        public override async System.Threading.Tasks.Task ProcessWebAsync()
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessWeb1", $"RMSPSettingFullProcessor.ProcessWeb1{curSetting.FullPath}", true))
            {
                try
                {
                    using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //*****ProgressService.IncreaseBase(1);
                        //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                        var bposInfo = await GetBposInfoBySiteAsync(siteNode.FullPath);
                        IAveSite aveSite;
                        //try   //debug server object model
                        //{
                        //    mfactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Auto);
                        //    aveSite = mfactory.CreateSite(siteNode.FullPath);
                        //    UseServerApi = true;
                        //}
                        //catch (Exception e)
                        //{
                        //logger.Info("use server object model failed {0}", e.ToString());
                        mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                        mDeferredDisposalScope.Add(TryUnlockSiteCollection(mfactory));
                        aveSite = mfactory.CreateSite(siteNode.FullPath);
                        mLabelUtility.CacheSPLabel(aveSite);
                        curRecords = mfactory.CreateRecords();
                        try
                        {
                            IAveTenant tenant = mfactory.CreateTenantCompatibleGeo(bposInfo, siteNode.FullPath);
                            curSiteProperties = tenant.GetSitePropertiesByUrl(siteNode.FullPath);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Init site properties failed {siteNode.FullPath}:{e}");
                        }
                        base.SetModuleFactoryForAuto(mfactory);
                        //}
                        if (JobContext.IsCSDTenant)
                        {
                            mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                        }
                        using (aveSite)
                        {
                            curSite = aveSite;
                            CacheSiteScope(curSite);
                            IAveWeb curWeb = null;
                            try
                            {
                                curWeb = curSite.OpenWeb(curSetting.WebId);
                            }
                            catch (Exception e)
                            {
                                var exception = e.InnerException as ServerException;
                                if (exception != null && exception.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                                {
                                    logger.Error("[DirtyData] Web {0} is deleted, error: {1}", curSetting.FullPath, e.ToString());
                                    return;
                                }
                                throw;
                            }

                            //*****var discoverWeb = discoverFactory.CreateDiscoverWeb(aveSite, curWeb.ServerRelativeUrl, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory);
                            var discoverWeb = new AveDiscoverWeb(curSite, curWeb.ServerRelativeUrl, DiscoverModule.Archive, mfactory);
                            base.DoSettingAction(discoverWeb.AveWeb, curSiteProperties);
                            var allSubWebs = discoverWeb.GetSubWebs(true).Values;
                            //****ProgressService.IncreaseBase(allSubWebs.Count);
                            foreach (var subWeb in allSubWebs)
                            {
                                await ProcessWebAsync(subWeb);
                            }
                            try
                            {
                                allSubWebs.FirstOrDefault(w => w.AveWeb.IsRootWeb)?.Dispose();
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Dispose root web error:{e}");
                            }
                            var allLists = discoverWeb.GetLists().Values;
                            //****ProgressService.IncreaseBase(allLists.Count);
                            foreach (var list in allLists)
                            {
                                await ProcessListAsync(list);
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    isFailedAddBCS = true;
                    isFailedAddContainer = true;
                    logger.Warn("Set web setting failed {0}", e.ToString());

                    string comment = GetExceptionMessage(e);
                    base.AddDetail(curNodeInfo.Title, curSetting.FullPath, string.Empty,
                       string.Empty, "", JobReportDetailStatus.Failed, comment);//TO DO I18N
                }
                finally
                {
                    await base.ProcessWebAsync();
                }
            }
        }
        public override async System.Threading.Tasks.Task ProcessListAsync()
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessList1", $"RMSPSettingFullProcessor.ProcessList1{curSetting.FullPath}", true))
            {
                IAveList aveList = null;
                try
                {
                    using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //****ProgressService.IncreaseBase(1);
                        var siteNode = GetSiteCollectionNode(curNodeInfo);//TO DO Ylgu Get site bpos info from DA. or 
                                                                          //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                        var bposInfo = await GetBposInfoBySiteAsync(siteNode.FullPath);
                        IAveSite aveSite;
                        //try
                        //{
                        //    mfactory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Auto);
                        //    aveSite = mfactory.CreateSite(siteNode.FullPath);
                        //    UseServerApi = true;
                        //}
                        //catch (Exception e)
                        //{
                        //    logger.Info("use server object model failed {0}", e.ToString());
                        mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                        mDeferredDisposalScope.Add(TryUnlockSiteCollection(mfactory));
                        curRecords = mfactory.CreateRecords();
                        aveSite = mfactory.CreateSite(siteNode.FullPath);
                        mLabelUtility.CacheSPLabel(aveSite);
                        base.SetModuleFactoryForAuto(mfactory);
                        //}
                        if (JobContext.IsCSDTenant)
                        {
                            mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                        }
                        using (aveSite)
                        {
                            curSite = aveSite;
                            CacheSiteScope(curSite);
                            IAveWeb aveWeb = null;
                            try
                            {
                                aveWeb = aveSite.OpenWeb(curSetting.WebId);
                            }
                            catch (Exception e)
                            {
                                var exception = e.InnerException as ServerException;
                                if (exception != null && exception.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                                {
                                    logger.Error("[DirtyData] Web {0} is deleted, error: {1}", curSetting.FullPath, e.ToString());
                                    return;
                                }
                                throw;
                            }

                            try
                            {
                                aveList = aveWeb.GetList(curSetting.ListId);
                            }
                            catch (Exception e)
                            {
                                var exception = e.InnerException as ServerException;
                                if (exception != null && exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    logger.Error("[DirtyData] List {0} is deleted, error: {1}", curSetting.FullPath, e.ToString());
                                    return;
                                }
                                throw;
                            }

                            if (ShouldSkipSharePointList(aveList))
                            {
                                logger.Info("Skip list {0} in scope {1} because lifecycle management for SharePoint Lists is disabled", curSetting.FullPath, curNodeInfo.FullPath);
                                return;
                            }

                            await base.DoSettingActionAsync(aveList);
                            if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                            {
                                if (SkipRemoveFolderDefault(aveList))
                                {
                                    logger.Info("IsUsingExistColumnName and field not exist in the list, skip remove folder default value");
                                }
                                else
                                {
                                    if (!IsKeepSPDefaultValue(curSetting))
                                    {
                                        List<string> folders = GetNeedRemoveValueFolders(aveList);
                                        folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
                                        logger.Info($"Remove the folders default value.");
                                    }
                                }
                            }
                            #region 不再按照Folder结构处理Apply Existing和Auto Job
                            //if (aveList.BaseType == AveBaseType.DocumentLibrary)
                            //{
                            //    bool isEnableDocumentLevelSetting = curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            //                                    (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn));
                            //    bool isApplyExistingJob = curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm;
                            //    bool isAutoJob = (DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary;

                            //    if (isEnableDocumentLevelSetting)
                            //    {
                            //        if (isApplyExistingJob || isAutoJob)
                            //        {
                            //            AveDiscoverSite tmpDiscoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
                            //            var discoverList = tmpDiscoverSite.GetDiscoverList(aveSite, aveWeb, curSetting.FullPath);

                            //            var rootFolder = discoverList.GetRootFolder();
                            //            var subFolders = rootFolder.GetSubFolders();

                            //            foreach (var discoverFolder in subFolders)
                            //            {
                            //                if (!discoverFolder.IsSystemObject)
                            //                {
                            //                    ProcessFolder(discoverFolder);
                            //                }
                            //            }
                            //        }
                            //        else
                            //        {
                            //            logger.Info($"Current setting does not contain ApplyExisting or AutoJob, so skip folders under list.");
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    isFailedAddBCS = true;
                    isFailedAddContainer = true;
                    logger.Warn("Set list setting error {0}", e.ToString());
                    string comment = GetExceptionMessage(e);
                    base.AddDetail(curNodeInfo.Title, curSetting.FullPath, string.Empty,
                       string.Empty, "", JobReportDetailStatus.Failed, comment);//TO DO I18N
                }
                finally
                {
                    //SPDicoverCache.Instance.ListCache.RemoveCahce(aveList);
                    await base.ProcessListAsync();
                }
            }
        }

        private bool SkipRemoveFolderDefault(IAveList list)
        {
            bool needSkip = false;
            try
            {
                var columnName = GetColumnInternalName(list);
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    needSkip = true;
                }
            }
            catch (Exception e)
            {
                logger.Info("An error occurred while getting column name. Error:{0}", e.ToString());
                needSkip = true;
            }
            return needSkip;
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }
        public virtual async System.Threading.Tasks.Task<bool> ProcessWebAsync(AveDiscoverWeb discoverWeb, bool browserSub = true)
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessWeb2", $"RMSPSettingFullProcessor.ProcessWeb2.{discoverWeb.FullUrl}", true))
            {
                //using (discoverWeb)
                //{
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var webSetting = SharePointSettingDao.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), discoverWeb.WebID);
                        if (webSetting != null)
                        {
                            logger.Info("Web {0} is a break node which has custom setting", discoverWeb.FullUrl);
                            return true;
                        }
                        base.DoSettingAction(discoverWeb.AveWeb, curSiteProperties);
                        var allDiscoverLists = discoverWeb.GetLists().Values;
                        if (allDiscoverLists != null && allDiscoverLists.Count > 0)
                        {
                            ReportManager.IncreaseBase(allDiscoverLists.Count);
                        }
                        //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                        ArgumentNullException.ThrowIfNull(allDiscoverLists);
                        foreach (var list in allDiscoverLists)
                        {
                            ReportManager.Increase();
                            await ProcessListAsync(list);
                        }
                        if (browserSub)
                        {
                            var allSubWebs = discoverWeb.GetSubWebs(true).Values;
                            //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                            foreach (var subWeb in allSubWebs)
                            {
                                await ProcessWebAsync(subWeb);
                            }
                            try
                            {
                                allSubWebs.FirstOrDefault(w => w.AveWeb.IsRootWeb)?.Dispose();
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Dispose root web error:{e}");
                            }
                        }
                        await base.ProcessWebAsync();
                        return false;
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("Process web Error {0}:{1}", discoverWeb.FullUrl, e.ToString());
                    return false;
                }
                finally
                {
                    if (!discoverWeb.AveWeb.IsRootWeb)
                    {
                        discoverWeb.Dispose();
                        logger.Info($"Dispose web {discoverWeb.FullUrl}");
                    }
                    else
                    {
                        logger.Info($"This web is root web, we dispose the web when process all webs.");
                    }
                }
                //}

            }
        }

        private async Task<AveBPOSAccountInfo> GetBposInfoBySiteAsync(string siteUrl)
        {
            lock (locker)
            {
                if (_bposCache.ContainsKey(siteUrl))
                {
                    return _bposCache[siteUrl];
                }
                else
                {
                    //RADataBroker.DAOAPIClientV1 DAOAPIClientV1 = new RADataBroker.DAOAPIClientV1();
                    //GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = DAOAPIClientV1.GetRemoteSiteCollectionByUrl(siteUrl);
                    GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    AveBPOSAccountInfo aveBPOSAccountInfo = PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection).Result;
                    _bposCache.Add(siteUrl, aveBPOSAccountInfo);
                    return aveBPOSAccountInfo;
                }
            }
        }
        private readonly object locker = new object();
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        protected virtual List<string> GetNeedRemoveValueFolders(IAveList list, string parentFolderPath = "")
        {
            logger.Info($"Get need remove default value folders for list:{list.RootFolder.ServerRelativeUrl}");
            var needRemoveDefaultValueFolders = new List<string>();
            var folderSettingsUnderList = SharePointSettingDao.GetFolderSettingUnderList(list.ID, new Guid(siteNode.ID)).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            var foldersWithDefault = SPSettingsUtility.GetFoldersWithDefaultValue(list, GetColumnInternalName(list), parentFolderPath);

            // Root folder server relative url
            var rootFolderUrl = list.RootFolder.ServerRelativeUrl;

            foreach (var fWithDefault in foldersWithDefault)
            {
                if (fWithDefault == rootFolderUrl)
                {
                    logger.Info($"Skip remove root folder default value:{fWithDefault}");
                    continue;
                }

                if (!folderSettingsUnderList.Contains(fWithDefault))
                {
                    logger.Info($"Need remove default value folder:{fWithDefault}");
                    needRemoveDefaultValueFolders.Add(fWithDefault);
                }
            }
            return needRemoveDefaultValueFolders;
        }

        protected string GetColumnInternalName(IAveList list)
        {
            if (curSetting.IsUsingExistColumnName)
            {
                string columnName = curSetting.ExistColumnName;
                var listField = list.Fields.GetRecordTaxonomyField(columnName);
                string internalName;
                if (listField != null)
                {
                    internalName = listField.InternalName;
                }
                else
                {
                    throw new Exception($"Get column internal name faild. Can not find list column for:{list.RootFolder.ServerRelativeUrl}");
                }
                return internalName;
            }
            else
            {
                return RcordsBuiltInColumn.ITEM_BCS_NAME;
            }
        }

        protected static bool IsKeepSPDefaultValue(RMSharePointSetting setting)
        {
            return JobContext.IsCSDTenant ? false : setting.IsKeepSharePointDefaultValue;
        }
        public virtual async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverList list)
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessList2", $"RMSPSettingFullProcessor.ProcessList2.{list.Title}", true))
            {
                try
                {
                    #region debug code 
                    //DateTime startTime = DateTime.UtcNow.AddHours(-1);
                    //DateTime endTime = DateTime.UtcNow;
                    //var debugSite = discoverFactory.CreateDiscoverSite(curSite, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory, startTime, endTime);
                    //var debugList = discoverFactory.CreateDiscoverList(curSite, curSite.RootWeb.ID, list.RootFolderUrl, startTime, endTime, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory);
                    //var debugFolder = debugList.GetChangeRootFolder();
                    //foreach (var discoverItem in debugFolder.GetChangeItems())
                    //{
                    //    logger.Info("Item name {0}", discoverItem.FullUrl);
                    //}
                    #endregion
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (list.Title == "{System Folder}")
                        {
                            return;
                        }
                        IAveList aveList = list.GetListObject();
                        if (CheckIsDesignList(aveList))
                        {
                            logger.Info("Skip the system list {0}", list.RootFolderUrl);
                            //****ProgressService.Increase();
                            return;
                        }
                        else if (CheckIsDesignListAgain(list))
                        {
                            logger.Info("Skip the system list 2 {0}", list.RootFolderUrl);
                        }
                        if (aveList.Hidden)
                        {
                            logger.Info("Skip the hidden list {0}", list.RootFolderUrl);
                            return;
                        }
                        logger.Info($"Process list {list.Title}, list root folder name:{aveList.RootFolder.Name}, list base template:{(int)aveList.BaseTemplate}, " +
                            $"discover list root folder url:{list.RootFolderUrl}, server template:{list.ServerTemplate}");
                        if (ShouldSkipSharePointList(aveList))
                        {
                            logger.Info("Skip list {0} in scope {1} because lifecycle management for SharePoint Lists is disabled", list.RootFolderUrl, curNodeInfo.FullPath);
                            return;
                        }
                        var listSetting = SharePointSettingDao.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), list.ListId);
                        if (listSetting != null)
                        {
                            logger.Info("List {0} is a break node which has custom setting", list.RootFolderUrl);
                            //*****ProgressService.Increase();
                            //TO DO Detail
                            return;
                        }
                        await base.DoSettingActionAsync(aveList);
                        if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                        {
                            if (!IsKeepSPDefaultValue(curSetting))
                            { 
                                List<string> folders = GetNeedRemoveValueFolders(aveList);
                                folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
                                logger.Info($"Remove the folders default value.");
                            }
                        }
                        #region 不再按照Folder结构处理Apply Existing和Auto Job
                        //if (aveList.BaseType == AveBaseType.DocumentLibrary)
                        //{
                        //    bool isEnableDocumentLevelSetting = curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                        //                                    (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn));
                        //    bool isApplyExistingJob = curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm;
                        //    bool isAutoJob = (DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary;
                        //    if (isEnableDocumentLevelSetting)
                        //    {
                        //        if (isApplyExistingJob || isAutoJob)
                        //        {
                        //            var rootFolder = list.GetRootFolder();
                        //            var subFolders = rootFolder.GetSubFolders();

                        //            if (subFolders != null && subFolders.Count > 0)
                        //            {
                        //                ReportManager.IncreaseBase(subFolders.Count);
                        //            }

                        //            foreach (var discoverFolder in subFolders)
                        //            {
                        //                ReportManager.Increase();
                        //                if (!discoverFolder.IsSystemObject)
                        //                {
                        //                    ProcessFolder(discoverFolder);
                        //                }
                        //            }
                        //        }
                        //        else
                        //        {
                        //            logger.Info($"Current setting does not contain ApplyExisting or AutoJob, so skip folders under list.");
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("Process List Error {0}:{1}", list.Name, e.ToString());
                }
                finally
                {
                    await base.ProcessListAsync();
                }
            }
        }

        #region Process Folder
        public async System.Threading.Tasks.Task ProcessFolderAsync()
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessFolder1", $"RMSPSettingFullProcessor.ProcessFolder1{curSetting.FullPath}", true))
            {
                try
                {
                    using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var siteNode = GetSiteCollectionNode(curNodeInfo);//TO DO Ylgu Get site bpos info from DA. or 
                                                                          //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                        var bposInfo = await GetBposInfoBySiteAsync(siteNode.FullPath);
                        IAveSite aveSite;
                        mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                        mDeferredDisposalScope.Add(TryUnlockSiteCollection(mfactory));
                        aveSite = mfactory.CreateSite(siteNode.FullPath);
                        mLabelUtility.CacheSPLabel(aveSite);
                        curRecords = mfactory.CreateRecords();
                        base.SetModuleFactoryForAuto(mfactory);

                        if (JobContext.IsCSDTenant)
                        {
                            mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                        }
                        using (aveSite)
                        {
                            CacheSiteScope(aveSite);
                            IAveWeb aveWeb = null;
                            IAveList aveList = null;
                            try
                            {
                                aveWeb = aveSite.OpenWeb(curSetting.WebId);
                            }
                            catch (Exception e)
                            {
                                var exception = e.InnerException as ServerException;
                                if (exception != null && exception.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND)
                                {
                                    logger.Error("[DirtyData] Web {0} is deleted, error: {1}", curSetting.FullPath, e.ToString());
                                    return;
                                }
                                throw;
                            }

                            try
                            {
                                aveList = aveWeb.GetList(curSetting.ListId);
                            }
                            catch (Exception e)
                            {
                                var exception = e.InnerException as ServerException;
                                if (exception != null && exception.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED)
                                {
                                    logger.Error("[DirtyData] List {0} is deleted, error: {1}", curSetting.FullPath, e.ToString());
                                    return;
                                }
                                throw;
                            }

                            if (ShouldSkipSharePointList(aveList))
                            {
                                logger.Info("Skip folder {0} under list {1} in scope {2} because lifecycle management for SharePoint Lists is disabled", curSetting.FullPath, aveList.ID, curNodeInfo.FullPath);
                                return;
                            }

                            IAveFolder aveFolder = null;
                            aveFolder = aveList.GetFolder(curSetting.FullPath);
                            if (!aveFolder.Exists)
                            {
                                logger.Error("[DirtyData] Folder {0} is deleted, aveFolder.Exists is {1}", curSetting.FullPath, aveFolder.Exists);
                                return;
                            }
                            await base.DoSettingActionAsync(aveFolder);
                            if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                            {
                                if (!IsKeepSPDefaultValue(curSetting))
                                { 
                                    List<string> folders = GetNeedRemoveValueFolders(aveList, aveFolder.ServerRelativeUrl);
                                    folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
                                    logger.Info($"Remove the folders default value.");
                                }
                            }


                            #region 不再按照Folder结构处理Apply Existing和Auto Job
                            //bool isEnableDocumentLevelSetting = curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            //                                    (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn));
                            //bool isApplyExistingJob = curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm;
                            //bool isAutoJob = (DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary;
                            //if (isEnableDocumentLevelSetting)
                            //{
                            //    if (isApplyExistingJob || isAutoJob)
                            //    {
                            //        var discoverFolder = new AveDiscoverFolder(aveSite, curSetting.WebId, WebUtil.MakeServerRelativeUrl(curSetting.FullPath), DiscoverModule.Archive, mfactory);
                            //        var subFolders = discoverFolder.GetSubFolders(false, false);
                            //        foreach (var subFolder in subFolders)
                            //        {
                            //            if (!subFolder.IsSystemObject)
                            //            {
                            //                ProcessFolder(subFolder);
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        logger.Info($"Current setting does not contain ApplyExisting or AutoJob, so skip folders under folder.");
                            //    }
                            //}
                            #endregion
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    isFailedAddBCS = true;
                    isFailedAddContainer = true;
                    logger.Warn("Set list setting error {0}", e.ToString());

                    string comment = GetExceptionMessage(e);
                    base.AddDetail(curNodeInfo.Title, curSetting.FullPath, string.Empty,
                       string.Empty, "", JobReportDetailStatus.Failed, comment);//TO DO I18N
                }
                finally
                {
                }
            }
        }

        protected bool ShouldSkipSharePointList(IAveList list)
        {
            if (list == null)
            {
                return false;
            }

            bool isDocLib = list.BaseType == AveBaseType.DocumentLibrary
                || list.BaseTemplate == AveListTemplateType.DocumentLibrary;
            return !mEnableLifecycleManagementForSharePointLists && !isDocLib;
        }

        [Obsolete("unused")]
        public async System.Threading.Tasks.Task ProcessFolderAsync(AveDiscoverFolder folder, bool browserSub = true)
        {
            using (var scope = new PerformanceScope("RMSPSettingFullProcessor.ProcessFolder2", $"RMSPSettingFullProcessor.ProcessFolder2{folder.FullUrl}", true))
            {
                try
                {
                    logger.Info("Process folder {0}", folder.FullUrl);
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var folderSetting = SharePointSettingDao.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), folder.AveFolder.UniqueId);
                        if (folderSetting != null)
                        {
                            logger.Info("Folder {0} is a break node which has custom setting", folder.FullUrl);
                            //*****ProgressService.Increase();
                            //TO DO Detail
                            return;
                        }
                        await base.DoSettingActionAsync(folder.AveFolder, false);
                        //SPSettingsUtility.RemoveFolderDefalutValue(folder.AveFolder, folder.AveFolder.ParentList, curSetting);

                        if (browserSub)
                        {
                            var allSubFolders = folder.GetSubFolders(false, false);
                            if (allSubFolders != null && allSubFolders.Count > 0)
                            {
                                ReportManager.IncreaseBase(allSubFolders.Count);
                            }
                            //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                            ArgumentNullException.ThrowIfNull(allSubFolders);
                            foreach (var subFolder in allSubFolders)
                            {
                                ReportManager.Increase();
                                if (!subFolder.IsSystemObject)
                                {
                                    await ProcessFolderAsync(subFolder);
                                }
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("Process Folder Error {0}:{1}", folder?.ID, e.ToString());
                }
                finally
                {

                }
            }
        }
        #endregion
        public override async System.Threading.Tasks.Task RunAsync()
        {
            switch (curNodeInfo.Level)
            {
                case NodeLevel.SiteCollection:
                    await ProcessSiteCollectionAsync();
                    break;
                case NodeLevel.Site:
                    await ProcessWebAsync();
                    break;
                case NodeLevel.List:
                    await ProcessListAsync();
                    break;
                case NodeLevel.Folder:
                    await ProcessFolderAsync();
                    break;
            }
            AddSiteScope();
            await base.RunAsync();
            logger.Info($"SPSettingFullProcessor finish processing [{curNodeInfo.FullPath}]");
        }


        private void CacheSiteScope(IAveSite item)
        {
            try
            {
                if (scopeInfo == null && item != null)
                {
                    (var siteFullPath, var siteName, var siteId) = (item.RootWeb?.Url, item.RootWeb?.Title, item.ID);
                    scopeInfo = new RMScope()
                    {
                        FullPath = siteFullPath,
                        ScopeId = siteId,
                        IsRemoved = false,
                        ScopeName = siteName,
                    };
                    logger.Info($"success to cache site scope info. url:{siteFullPath}, id:[{siteId}], name:[{siteName}]");
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error while cache site scope, message:{e}");
            }
        }

        protected void AddSiteScope()
        {
            try
            {
                if (scopeInfo != null)
                {
                    RMScopeDao.AddOrUpateSiteScope(scopeInfo);
                    logger.Info($"success to save site scope info.");
                }
                else
                {
                    logger.Info($"no need to save site scope info.");
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error while add site scope, message:{e}");
            }
        }

        private sealed class DiscoverWebHierarchyNode
        {
            public DiscoverWebHierarchyNode(AveDiscoverWeb currentNode)
            {
                CurrentNode = currentNode ?? throw new ArgumentNullException(nameof(currentNode));
            }

            public AveDiscoverWeb CurrentNode { get; }

            public List<DiscoverWebHierarchyNode> ChildrenNodes { get; } = new List<DiscoverWebHierarchyNode>();
        }

        private DiscoverWebHierarchyNode RestoreWebHierarchyByParentWebId(IEnumerable<AveDiscoverWeb> allDiscoverWebs)
        {
            var webList = allDiscoverWebs?.Where(w => w != null).ToList() ?? new List<AveDiscoverWeb>();
            if (webList.Count == 0)
            {
                return null;
            }

            var nodeByWebId = new Dictionary<Guid, DiscoverWebHierarchyNode>();
            var nodes = new List<DiscoverWebHierarchyNode>(webList.Count);

            foreach (var web in webList)
            {
                var node = new DiscoverWebHierarchyNode(web);
                nodes.Add(node);

                var webId = web.WebID;
                if (webId != Guid.Empty && !nodeByWebId.ContainsKey(webId))
                {
                    nodeByWebId.Add(webId, node);
                }
            }

            var roots = new List<DiscoverWebHierarchyNode>();
            foreach (var node in nodes)
            {
                var currentWeb = node.CurrentNode;
                if (currentWeb?.AveWeb?.IsRootWeb == true)
                {
                    roots.Add(node);
                    continue;
                }

                var webId = currentWeb.WebID;
                var parentWebId = currentWeb.AveWeb?.ParentWebId ?? Guid.Empty;
                if (parentWebId == Guid.Empty || parentWebId == webId)
                {
                    roots.Add(node);
                    continue;
                }

                if (nodeByWebId.TryGetValue(parentWebId, out var parentNode))
                {
                    parentNode.ChildrenNodes.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            var rootNode = roots.FirstOrDefault(r => r?.CurrentNode?.AveWeb?.IsRootWeb == true) ?? roots.FirstOrDefault();
            if (rootNode == null)
            {
                return null;
            }

            if (roots.Count > 1)
            {
                logger.Warn($"Multiple root webs found ({roots.Count}). Attaching extra roots under root web: {rootNode.CurrentNode?.FullUrl}");
                foreach (var extraRoot in roots)
                {
                    if (extraRoot == null || ReferenceEquals(extraRoot, rootNode))
                    {
                        continue;
                    }
                    rootNode.ChildrenNodes.Add(extraRoot);
                }
            }

            SortWebHierarchy(rootNode.ChildrenNodes);
            return rootNode;
        }

        private async Task ProcessWebHierarchyAsync(DiscoverWebHierarchyNode root)
        {
            if (root == null)
            {
                return;
            }

            var stack = new Stack<DiscoverWebHierarchyNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node == null)
                {
                    continue;
                }

                ReportManager.Increase();
                var isSkipped = await ProcessWebAsync(node.CurrentNode, false);
                if (isSkipped)
                {
                    continue;
                }

                var children = node.ChildrenNodes;
                if (children == null || children.Count == 0)
                {
                    continue;
                }

                for (var childIndex = children.Count - 1; childIndex >= 0; childIndex--)
                {
                    stack.Push(children[childIndex]);
                }
            }
        }

        private static void SortWebHierarchy(List<DiscoverWebHierarchyNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            nodes.Sort((left, right) =>
            {
                var leftKey = GetSortKey(left);
                var rightKey = GetSortKey(right);
                return StringComparer.OrdinalIgnoreCase.Compare(leftKey, rightKey);
            });

            foreach (var node in nodes)
            {
                SortWebHierarchy(node.ChildrenNodes);
            }
        }

        private static string GetSortKey(DiscoverWebHierarchyNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var serverRelativeUrl = node.CurrentNode?.AveWeb?.ServerRelativeUrl;
            if (!string.IsNullOrWhiteSpace(serverRelativeUrl))
            {
                return serverRelativeUrl;
            }

            return node.CurrentNode?.FullUrl ?? string.Empty;
        }
    }
}
