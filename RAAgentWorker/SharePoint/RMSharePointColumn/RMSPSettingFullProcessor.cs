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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.RA.SharePoint.RMSharePointColumn.Base;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Common.Global;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.RA.CommonUtil;
using Microsoft.SharePoint.Client;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class SPSettingFullProcessor : BaseSPSettingProcessor
    {
        // private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(SPSettingFullProcessor));
        //private RMSharePointSetting curSetting; 
        private IAveSiteProperties curSiteProperties;
        //protected BaseJobDto mBaseJobDto;
        public SPSettingFullProcessor(RMSharePointOnPremiseSetting setting, SPTreeNodeDto nodeInfo, long settingTime) : base(nodeInfo)
        {
            curSetting = setting;
            curNodeInfo = nodeInfo;
            //mBaseJobDto = jobDto;

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
            logger.Info($"SPSettingFullProcessor start to process [{curNodeInfo.SPObjectId}]");
        }
        public override void ProcessSiteCollection()
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessSiteCollection", addToStatistics: true))
            {
                try
                {
                    //using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(curNodeInfo.NodeExtension.BposInfo, curNodeInfo.FullPath);
                        var bposInfo = GetBposInfoBySite(curNodeInfo.FullPath);
                        IAveSite aveSite;
                        mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                        try
                        {
                            aveSite = mfactory.CreateSite(siteNode.FullPath);
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
                                            logger.Error("[DirtyData] SiteCollection {0} is deleted, error: {1}", siteNode.FullPath.LogBase64(), e.ToString());
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

                        var mTotalWebs = aveSite.AllWebs.Count;
                        base.ProgressService.IncreaseBase(mTotalWebs);

                        //if (JobContext.IsCSDTenant)
                        //{
                        //    mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                        //}
                        using (aveSite)
                        {
                            curSite = aveSite;
                            AveDiscoverSite tmpDiscoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                            /* Add logic for RECO-3843 start*/
                            //var adminUrl  = AveUrlUtility.GetTenantAdminSiteUrl(mfactory, bposInfo);
                            //IAveTenant tenant  = mfactory.CreateTenant(adminUrl, true);
                            //mfactory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, siteNode.FullPath));
                            curSiteProperties = null;
                            //tenant.GetSitePropertiesByUrl(siteNode.FullPath);
                            /* Add logic for RECO-3843 end*/

                            base.DoSettingAction(curSite, curSiteProperties);
                            var allDiscoverWebs = tmpDiscoverSite.GetWebs().Values;
                            //****ProgressService.IncreaseBase(allDiscoverWebs.Count);
                            foreach (var web in allDiscoverWebs)
                            {
                                base.ProgressService.Increase();
                                ProcessWeb(web, false);
                            }
                        }
                    }
                }
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
                catch (Exception e)
                {
                    isFailedAddBCS = true;
                    isFailedAddContainer = true;
                    logger.Error("Process sitecollection error {0}", e.ToString());
                    //TO DO Add Detail
                    base.AddDetail(curNodeInfo.Title, curNodeInfo.Url, string.Empty,
                        string.Empty, string.Empty, JobReportDetailStatus.Failed, "RM_SS_SiteRemovedFromDAO");//TO DO I18N
                }
                finally
                {
                    base.ProcessSiteCollection();
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
        public override void ProcessWeb()
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessWeb1", addToStatistics: true))
            {
                try
                {
                    //*****ProgressService.IncreaseBase(1);
                    //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                    var bposInfo = GetBposInfoBySite(siteNode.FullPath);
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
                    mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                    aveSite = mfactory.CreateSite(siteNode.FullPath);
                    curRecords = mfactory.CreateRecords();
                    // IAveTenant tenant = mfactory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, siteNode.FullPath));
                    //curSiteProperties = tenant.GetSitePropertiesByUrl(siteNode.FullPath);
                    base.SetModuleFactoryForAuto(mfactory);
                    //}
                    //if (JobContext.IsCSDTenant)
                    //{
                    //    mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                    //}
                    using (aveSite)
                    {
                        curSite = aveSite;
                        IAveWeb curWeb = null;
                        try
                        {
                            curWeb = curSite.OpenWeb(curSetting.WebId);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException != null && IsServerException(e.InnerException) && e.InnerException.Message.Equals("File Not Found."))
                            {
                                logger.Error("[DirtyData] Web {0} is deleted, error: {1}", curSetting.FullPath.LogBase64(), e.ToString());
                                return;
                            }
                            throw e;
                        }

                        //*****var discoverWeb = discoverFactory.CreateDiscoverWeb(aveSite, curWeb.ServerRelativeUrl, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory);
                        var discoverWeb = new AveDiscoverWeb(curSite, curWeb.ServerRelativeUrl, DiscoverModule.Archive, AveDiscoveryKind.API, mfactory);
                        base.DoSettingAction(discoverWeb.AveWeb, curSiteProperties);
                        var allSubWebs = discoverWeb.GetSubWebs(true).Values;
                        //****ProgressService.IncreaseBase(allSubWebs.Count);
                        foreach (var subWeb in allSubWebs)
                        {
                            ProcessWeb(subWeb);
                        }
                        var allLists = discoverWeb.GetLists().Values;
                        //****ProgressService.IncreaseBase(allLists.Count);
                        foreach (var list in allLists)
                        {
                            ProcessList(list);
                        }
                    }
                }
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
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
                    base.ProcessWeb();
                }
            }
        }
        public override void ProcessList()
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessList1", addToStatistics: true))
            {
                IAveList aveList = null;
                try
                {
                    //****ProgressService.IncreaseBase(1);
                    var siteNode = GetSiteCollectionNode(curNodeInfo);//TO DO Ylgu Get site bpos info from DA. or 
                    //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                    var bposInfo = GetBposInfoBySite(siteNode.FullPath);
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
                    mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                    curRecords = mfactory.CreateRecords();
                    aveSite = mfactory.CreateSite(siteNode.FullPath);
                    base.SetModuleFactoryForAuto(mfactory);
                    //}
                    //if (JobContext.IsCSDTenant)
                    //{
                    //    mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                    //}
                    using (aveSite)
                    {
                        curSite = aveSite;
                        var aveWeb = aveSite.OpenWeb(curSetting.WebId);

                        try
                        {
                            if (!aveWeb.Exists)
                            {
                                logger.Warn("SharePoint web of the fullpath {0}, List Id {1}, Site Id {2}, Web Id {3} does not exists", curSetting.FullPath.LogBase64(), curSetting.ListId, curSetting.SiteId, curSetting.WebId);
                                base.AddDetail(curNodeInfo.Title, curSetting.FullPath, string.Empty, string.Empty, "", JobReportDetailStatus.Skipped, "File not found");//TO DO I18N
                                return;
                            }
                            aveList = aveWeb.GetList(curSetting.ListId);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException != null && IsServerException(e.InnerException) && e.InnerException.Message.Equals("List does not exist.\n\nThe page you selected contains a list that does not exist.  It may have been deleted by another user."))
                            {
                                logger.Error("[DirtyData] List {0} is deleted, error: {1}", curSetting.FullPath.LogBase64(), e.ToString());
                                return;
                            }
                            throw e;
                        }

                        base.DoSettingAction(aveList);
                        if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                        {
                            if (SkipRemoveFolderDefault(aveList))
                            {
                                logger.Info("IsUsingExistColumnName and field not exist in the list, skip remove folder default value");
                            }
                            else
                            {
                                List<string> folders = GetNeedRemoveValueFolders(aveList);
                                folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
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
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
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
                    base.ProcessList();
                }
            }
        }

        private bool IsServerException(Exception e)
        {
            return e != null
                && e.GetType().FullName.Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase);
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
        public void ProcessWeb(AveDiscoverWeb discoverWeb, bool browserSub = true)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessWeb2", $"RMSPSettingFullProcessor.ProcessWeb2.{discoverWeb.FullUrl}", true))
            {
                try
                {
                    //using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var webSetting = RMSPSettingUtil.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), discoverWeb.WebID);
                        if (webSetting != null)
                        {
                            logger.Info("Web {0} is a break node which has custom setting", discoverWeb.FullUrl.LogBase64());
                            return;
                        }
                        base.DoSettingAction(discoverWeb.AveWeb, curSiteProperties);
                        var allDiscoverLists = discoverWeb.GetLists().Values;
                        if (allDiscoverLists != null && allDiscoverLists.Count > 0)
                        {
                            logger.Info("Get list finished. Lists:{0}", string.Join(",", allDiscoverLists.Select(l => l.Title)).LogBase64());
                            // ReportManager.IncreaseBase(allDiscoverLists.Count);
                            base.ProgressService.IncreaseBase(allDiscoverLists.Count);
                        }
                        //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                        foreach (var list in allDiscoverLists)
                        {
                            // ReportManager.Increase();
                            base.ProgressService.Increase();
                            ProcessList(list);
                        }
                        if (browserSub)
                        {
                            var allSubWebs = discoverWeb.GetSubWebs(true).Values;
                            //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                            foreach (var subWeb in allSubWebs)
                            {
                                ProcessWeb(subWeb);
                            }
                        }
                        base.ProcessWeb();
                    }
                }
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
                catch (Exception e)
                {
                    logger.Error("Process web Error {0}:{1}", discoverWeb.FullUrl.LogBase64(), e.ToString());
                }
            }
        }

        private AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
        {
            lock (_bposCache)
            {
                if (_bposCache.ContainsKey(siteUrl))
                {
                    return _bposCache[siteUrl];
                }
                else
                {
                    var account = AgentAccountUtil.Get();
                    AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
                    {
                        Domain = account.Domain,
                        UserName = account.UserName,
                        Password = account.Password
                    };
                    _bposCache.Add(siteUrl, aveBPOSAccountInfo);
                    return aveBPOSAccountInfo;
                }
            }
        }
        private Dictionary<string, AveBPOSAccountInfo> _bposCache = new Dictionary<string, AveBPOSAccountInfo>();
        private List<string> GetNeedRemoveValueFolders(IAveList list, string parentFolderPath = "")
        {
            logger.Info($"Get need remove default value folders for list:{list.Title.LogBase64()}");
            var needRemoveDefaultValueFolders = new List<string>();
            var folderSettingsUnderList = RMSPSettingUtil.GetFolderSettingUnderList(list.ID, new Guid(siteNode.ID)).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
            var foldersWithDefault = SPSettingsUtility.GetFoldersWithDefaultValue(list, GetColumnInternalName(list), parentFolderPath);
            foreach (var fWithDefault in foldersWithDefault)
            {
                if (!folderSettingsUnderList.Contains(fWithDefault))
                {
                    logger.Info($"Need remove default value folder:{fWithDefault.LogBase64()}");
                    needRemoveDefaultValueFolders.Add(fWithDefault);
                }
            }
            return needRemoveDefaultValueFolders;
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

        private string GetColumnInternalName(IAveList list)
        {
            if (curSetting.IsUsingExistColumnName)
            {
                string internalName = string.Empty;
                string columnName = curSetting.ExistColumnName;
                var listField = list.Fields.Where(f => f.Title == columnName).FirstOrDefault();
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


        public void ProcessList(AveDiscoverList list)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessList2", $"RMSPSettingFullProcessor.ProcessList2.{list.Title}", true))
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
                    //using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (list.Title == "{System Folder}")
                        {
                            return;
                        }
                        IAveList aveList = list.GetListObject();
                        if (CheckIsDesignList(aveList))
                        {
                            logger.Info("Skip the system list {0}", list.Title.LogBase64());
                            //****ProgressService.Increase();
                            return;
                        }
                        if (aveList.Hidden)
                        {
                            logger.Info("Skip the hidden list {0}", list.Title.LogBase64());
                            return;
                        }
                        logger.Info("Process list {0}", list.Title.LogBase64());
                        var listSetting = RMSPSettingUtil.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), list.ListId);
                        if (listSetting != null)
                        {
                            logger.Info("List {0} is a break node which has custom setting", list.RootFolderUrl.LogBase64());
                            //*****ProgressService.Increase();
                            //TO DO Detail
                            return;
                        }
                        base.DoSettingAction(aveList);
                        if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                        {
                            if (SkipRemoveFolderDefault(aveList))
                            {
                                logger.Info("IsUsingExistColumnName and field not exist in the list, skip remove folder default value");
                            }
                            else
                            {
                                List<string> folders = GetNeedRemoveValueFolders(aveList);
                                folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
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
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
                catch (Exception e)
                {
                    logger.Error("Process List Error {0}:{1}", list.Name.LogBase64(), e.ToString());
                }
                finally
                {
                    base.ProcessList();
                }
            }
        }

        #region Process Folder
        public void ProcessFolder()
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessFolder1", addToStatistics: true))
            {
                try
                {
                    var siteNode = GetSiteCollectionNode(curNodeInfo);//TO DO Ylgu Get site bpos info from DA. or 
                    //var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
                    var bposInfo = GetBposInfoBySite(siteNode.FullPath);
                    IAveSite aveSite;
                    mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
                    aveSite = mfactory.CreateSite(siteNode.FullPath);
                    curRecords = mfactory.CreateRecords();
                    base.SetModuleFactoryForAuto(mfactory);

                    //if (JobContext.IsCSDTenant)
                    //{
                    //    mConfigSiteSetting = (new ConfigSiteUtil(bposInfo, siteNode.FullPath)).GetConfigData();
                    //}
                    using (aveSite)
                    {
                        var aveWeb = aveSite.OpenWeb(curSetting.WebId);
                        var aveList = aveWeb.GetList(curSetting.ListId);
                        IAveFolder aveFolder = null;
                        aveFolder = aveList.GetFolder(AveUrlUtility.GetServerRelativeUrl(curSetting.FullPath));
                        if (!aveFolder.Exists)
                        {
                            logger.Error("[DirtyData] Folder {0} is deleted, aveFolder.Exists is {1}", curSetting.FullPath.LogBase64(), aveFolder.Exists);
                            return;
                        }
                        base.DoSettingAction(aveFolder);
                        if (aveList.BaseTemplate == AveListTemplateType.DocumentLibrary)
                        {
                            if (SkipRemoveFolderDefault(aveList))
                            {
                                logger.Info("IsUsingExistColumnName and field not exist in the list, skip remove folder default value");
                            }
                            else
                            {
                                List<string> folders = GetNeedRemoveValueFolders(aveList, aveFolder.ServerRelativeUrl);
                                folders.ForEach(f => SPSettingsUtility.RemoveFolderDefaultValue(aveList, f, GetColumnInternalName(aveList)));
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
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
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

        public void ProcessFolder(AveDiscoverFolder folder, bool browserSub = true)
        {
            using (var scope = new AgentPerformanceScope("RMSPSettingFullProcessor.ProcessFolder2", addToStatistics: true))
            {
                try
                {
                    logger.Info("Process folder {0}", folder.LeafName.LogBase64());
                    //using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var folderSetting = RMSPSettingUtil.GetSettingInfoByScope(new Guid(groupNode.SPObjectId), new Guid(siteNode.ID), folder.AveFolder.UniqueId);
                        if (folderSetting != null)
                        {
                            logger.Info("Folder {0} is a break node which has custom setting", folder.FullUrl.LogBase64());
                            //*****ProgressService.Increase();
                            //TO DO Detail
                            return;
                        }
                        base.DoSettingAction(folder.AveFolder, false);
                        //SPSettingsUtility.RemoveFolderDefalutValue(folder.AveFolder, folder.AveFolder.ParentList, curSetting);

                        if (browserSub)
                        {
                            var allSubFolders = folder.GetSubFolders(false, false);
                            if (allSubFolders != null && allSubFolders.Count > 0)
                            {
                                //ReportManager.IncreaseBase(allSubFolders.Count);
                                base.ProgressService.IncreaseBase(allSubFolders.Count);
                            }
                            //****ProgressService.IncreaseBase(allDiscoverLists.Count);
                            foreach (var subFolder in allSubFolders)
                            {
                                //ReportManager.Increase();
                                base.ProgressService.Increase();
                                if (!subFolder.IsSystemObject)
                                {
                                    ProcessFolder(subFolder);
                                }
                            }
                        }
                    }
                }
                //catch (JobStopException ex)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
                catch (Exception e)
                {
                    logger.Error("Process Folder Error {0}:{1}", folder.LeafName.LogBase64(), e.ToString());
                }
                finally
                {

                }
            }
        }
        #endregion
        public override void Run()
        {
            //SPColumnCacheSetting.Instance.Init();
            switch (curNodeInfo.Level)
            {
                case NodeLevel.SiteCollection:
                    ProcessSiteCollection();
                    break;
                case NodeLevel.Site:
                    ProcessWeb();
                    break;
                case NodeLevel.List:
                    ProcessList();
                    break;
                case NodeLevel.Folder:
                    ProcessFolder();
                    break;
            }
            base.Run();
            logger.Info($"SPSettingFullProcessor finish processing [{curNodeInfo.SPObjectId}]");
        }
    }
}
