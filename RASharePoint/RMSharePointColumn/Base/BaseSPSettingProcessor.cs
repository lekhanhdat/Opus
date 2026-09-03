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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.SharePoint.RelatedRecords;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Common.Configurations;
using System.Text.RegularExpressions;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.Wrapper.Discovery;
using Microsoft.SharePoint.Client;

namespace AvePoint.RA.SharePoint.RMSharePointColumn.Base
{
    /// <summary>
    /// TO DO if support incremental job , 
    /// </summary>
    public abstract class BaseSPSettingProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(BaseSPSettingProcessor));
        protected ConfigSiteSetting mConfigSiteSetting = null;
        protected readonly Guid RelatedAppProductId = new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116");
        protected readonly Guid SpfxNewRelatedAppProductId = new Guid("f856b7db-f9d2-4555-90e0-6c94d589d3fd");
        protected readonly string relatedId = "RelatedId";
        protected List<string> DesignLists = new List<string>();
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        private GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection currentRemoteSite;
        protected SPTreeNodeDto curNodeInfo;
        protected IAveSite curSite;
        protected AveObjectModelFactory mfactory;
        protected bool? hasInstalledNewRelatedApp = null;
        protected bool? hasInstalledOldRelatedApp = null;
        private ISharePointSettingDao mSharePointSettingDao;
        protected ISharePointSettingDao SharePointSettingDao
        {
            get { return mSharePointSettingDao ??= new SharePointSettingDao(); }
        }
        private IRMNodeFlagDao mRMNodeFlagDao;
        protected IRMNodeFlagDao RMNodeFlagDao
        { get { return mRMNodeFlagDao ??= new RMNodeFlagDao(); } }
        private IRMScopeDao mRMScopeDao;
        protected IRMScopeDao RMScopeDao { get { return mRMScopeDao ??= new RMScopeDao(); } }
        protected IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        protected ITeamsSettingDao mTeamsSettingDao => new TeamsSettingDao();
        protected SPTreeNodeDto siteNode;
        protected SPTreeNodeDto groupNode;
        protected RMSharePointSetting curSetting;
        protected IAveORecords curRecords;
        protected bool isFailedApps = false;
        protected bool isFailedAddBCS = false;
        protected bool isFailedAddContainer = false;
        protected bool isFailedEnablePhysical = false;
        protected bool isFailedEnableApp = false;
        protected bool applyTermHasError = false;
        protected bool autoApplyTermHasError = false;
        protected bool aiApplyTermHasError = false;
        protected bool hasErrorNode = false;
        protected bool mJobHasStopped = false;
        protected bool hasSuccessNode = false;
        private bool mSiteLevelAddBCSFailed = false;
        protected SPOLabelUtility mLabelUtility = null;
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao; // lazy explorer dao for record lookup
        protected RA.DB.Explorer.Dao.IExplorerDao ExplorerDao => _explorerDao ??= new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
    
        public bool AddBCSFailed
        {
            get
            {
                return isFailedAddBCS;
            }
        }
        public bool AddContainerFailed
        {
            get
            {
                return isFailedAddContainer;
            }
        }
        public bool EnablePhysicalFailed
        {
            get
            {
                return isFailedEnablePhysical;
            }
        }
        public bool EnableAppFailed
        {
            get
            {
                return isFailedEnableApp;
            }
        }
        public bool ApplyTermHasError
        {
            get
            {
                return applyTermHasError;
            }
        }
        public bool AutoApplyTermHasError
        {
            get
            {
                return autoApplyTermHasError;
            }
        }
        public bool SmartApplyTermHasError
        {
            get
            {
                return aiApplyTermHasError;
            }
        }
        public bool GetAppsFailed
        {
            get { return isFailedApps; }
        }
        public bool GetNodeError
        {
            get { return hasErrorNode; }
        }
        public bool GetNodeSuccess
        {
            get { return hasSuccessNode; }
        }
        protected bool UseServerApi = false;
        private BaseJobDto mBaseJobDto;

        public BaseSPSettingProcessor()
        { }
        public BaseSPSettingProcessor(SPTreeNodeDto nodeInfo, BaseJobDto jobDto, SPOLabelUtility labelUtility)
        {
            //init job info
            siteNode = GetSiteCollectionNode(nodeInfo);
            groupNode = GetGroupNode(nodeInfo);
            this.DesignLists = WebUtil.GetDesignLists(JobContext.IsCSDTenant);//TO DO debug design list config file path later
            //ProgressService.Increase();
            mBaseJobDto = jobDto;
            mLabelUtility = labelUtility;
        }
        public virtual System.Threading.Tasks.Task ProcessSiteCollectionAsync() => System.Threading.Tasks.Task.CompletedTask;
        public virtual System.Threading.Tasks.Task ProcessWebAsync() => System.Threading.Tasks.Task.CompletedTask;
        public virtual System.Threading.Tasks.Task ProcessListAsync() => System.Threading.Tasks.Task.CompletedTask;
        public virtual async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                StringBuilder errorMessage = new StringBuilder();
                if (isFailedAddBCS || isFailedAddContainer || isFailedEnablePhysical || isFailedEnableApp)
                {
                    errorMessage.Append("RM_TS_SS_Summary");
                }

                try
                {
                    await SharePointSettingDao.SetSettingJobTimeAsync(curSetting.ScopeId, curSetting.SiteId, isFailedAddBCS, isFailedAddContainer);
                }
                catch (Exception e)
                {
                    logger.Warn("Update status error {0}", e.ToString());
                }
                try
                {
                    if (mLabelUtility != null && mLabelUtility.LabelApplied)
                    {
                        await mLabelUtility.AddLabelHistoryAsync();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Error occurred while updating label history {0}", e.ToString());
                }

                try
                {
                    RMMachineLearningDataSyncManager.Commit();
                }
                catch (Exception e)
                {
                    logger.Warn($"An error while commit ai sync data, message: {e}");
                }
            }
            catch
            {
                logger.Warn("Error occurred while finalizing setting job.");
            }
        }

        public virtual void SetModuleFactoryForAuto(AveObjectModelFactory factory)
        {
            SPSettingsUtility.factoryForAuto = factory;
        }

        private bool IsSpecialCSDClass()
        {
            if (curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
            {
                if (curSetting.DefaultTermId == mConfigSiteSetting.ExcludedFileTypeDefaultTerm.ID)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void DoSettingAction(IAveSite aveSite, IAveSiteProperties siteProperties)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.ProcessSitecollection", $"BaseSPSettingProcessor.ProcessSitecollection.{aveSite.Url}", true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    #region config bcs column
                    IAveTaxonomyField taxField = null;
                    string columnName = curSetting.IsUsingExistColumnName ? curSetting.ExistColumnName : curSetting.ColumnName;
                    try
                    {
                        if (JobContext.IsCSDTenant && IsSpecialCSDClass())
                        {
                            isFailedAddBCS = true;
                            logger.Info($"Skip Site Collection for default Term: [{curSetting.DefaultTermId}] is white class or modfied based class. Site Collection URL:[{aveSite.RootWeb.Url}]");
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, curSetting.ColumnName,
                                    string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_JS_JMD_SpecialCSDClassCannotBeDefaultTerm");
                            return;
                        }
                        SettingResult result = JobContext.IsCSDTenant ?
                            SPSettingsUtility.ConfigBCSColumn(aveSite, curSetting, ref taxField, mConfigSiteSetting)
                            : SPSettingsUtility.ConfigBCSColumn(aveSite, curSetting, ref taxField);
                        if (curSetting.IsUsingExistColumnName)
                        {
                            columnName = AppendDisplayName4Details(taxField, columnName);
                        }
                        if (result == SettingResult.Add)
                        {
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                    string.Empty, "RM_JS_JMD_Status_AddSiteCollectionColumn", JobReportDetailStatus.Success, string.Empty);
                        }
                        else if (result == SettingResult.Update)
                        {
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Status_UpdateSiteCollectionColumn", JobReportDetailStatus.Success, string.Empty);
                        }
                        else if (result == SettingResult.SKip)
                        {
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Status_SkipSiteCollectionColumn", JobReportDetailStatus.Skipped, string.Empty);
                        }
                        else if (result == SettingResult.UseExistSkip)
                        {
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                   string.Empty, "RM_RC_Audit_Action_SaveGlobalSettintExistColumn", JobReportDetailStatus.Success, string.Empty);
                        }
                        else if (result == SettingResult.Delete || result == SettingResult.SkipDelete)
                        {
                            var status = result == SettingResult.Delete ? JobReportDetailStatus.Success : JobReportDetailStatus.Skipped;
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Status_RemoveBCSColumn", status, string.Empty);
                        }
                    }
                    catch (Exception ce)
                    {
                        mSiteLevelAddBCSFailed = true;
                        isFailedAddBCS = true;
                        logger.Error("Add sitecollection level bcs column failed. Path:[{0}] Error:{1}", siteNode.FullPath, ce.ToString());
                        if (ce.Message.Contains("Term Is Unavailable"))
                        {
                            AddDetail(aveSite.RootWeb.Title, siteNode.FullPath, string.Empty,
                               string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SS_ConfigureColumnFailed");
                        }
                        else if (ce.Message.Contains(I18NEntity.GetString("RM_SPS_CanNotFindExistingColumn")))
                        {
                            AddDetail(aveSite.RootWeb.Title, siteNode.FullPath, columnName,
                               string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SPS_CanNotFindExistingColumn");
                        }
                        else if (ce.Message.Contains("This site has exceeded its maximum file storage limit."))
                        {
                            AddDetail(aveSite.RootWeb.Title, siteNode.FullPath, columnName,
                               string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SS_SiteStorageLimitExceeded");
                        }
                        else
                        {
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, columnName,
                                    string.Empty, "RM_JS_JMD_Status_AddSiteCollectionColumn", JobReportDetailStatus.Failed, "RM_SS_SCAddiOrNameRepeat");//TO DO ylgu update action or not
                        }
                    }
                    #endregion
                    #region config container property
                    try
                    {
                        if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            if (curSetting.TermIdOfContainer != Guid.Empty)
                            {
                                if (SPSettingsUtility.NeedUpdateContainer(aveSite.RootWeb, curSetting.TermIdOfContainer))
                                {
                                    SPSettingsUtility.ConfigBCSProperty(siteProperties, aveSite.Url, aveSite.RootWeb, curSetting.TermIdOfContainer);//TO DO Rebuild Logic of job details.                                                                                                                                             
                                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Success, string.Empty);
                                }
                                else
                                {
                                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Skipped, string.Empty);
                                }
                            }
                            else
                            {
                                SPSettingsUtility.ConfigBCSProperty(siteProperties, aveSite.Url, aveSite.RootWeb, Guid.Empty);
                            }
                        }
                        else
                        {
                            if (SPSettingsUtility.RemoveBCSProperty(aveSite.RootWeb))
                            {
                                AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveSiteCollectionClassification", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (DenyAddAndCustomizePagesEnableExcetion de)
                    {
                        logger.Warn("Config container property failed {0}:{1}", aveSite.Url, de.ToString());
                        isFailedAddContainer = true;
                        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                    }
                    catch (Exception e)
                    {
                        isFailedAddContainer = true;
                        if (e.InnerException?.GetType() == typeof(ServerUnauthorizedAccessException))
                        {
                            logger.Warn("Config container property failed {0}:{1}", aveSite.Url, e.ToString());
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetContainerLevelTermError");
                        }
                        else
                        {
                            logger.Warn("Config container property failed {0}:{1}", aveSite.Url, e.ToString());
                            AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                        }
                    }
                    #endregion
                    #region enable realted app
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        if (curSetting.EnableRelatedRecords)
                        {
                            //relatedUtility.AddAnApp(RelatedAppProductId);
                            try
                            {
                                bool result = SPSettingsUtility.AddSiteCollectionRelatedColumn(aveSite);
                                JobReportDetailStatus detailStatus = result ? JobReportDetailStatus.Success : JobReportDetailStatus.Skipped;
                                AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                   string.Empty, "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", detailStatus, string.Empty);
                            }
                            catch (Exception ex)
                            {
                                logger.Info("add site related column failed, site url: {0}, error message:{1}", aveSite.Url, ex.ToString());
                                AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                   string.Empty, "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobReportDetailStatus.Failed, string.Empty);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            SPSettingsUtility.DeleteSiteCollectionRelatedColumn(aveSite);
                            //SPSettingsUtility.UninstallApp(aveSite.RootWeb, RelatedAppProductId);
                            //AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                            //    string.Empty, I18NEntity.GetString("RM_JMD_RemoveApp"), JobReportDetailStatus.Success, string.Empty);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("remove site related column failed {0}:{1}", aveSite.Url, e.ToString());
                            //AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                            //       string.Empty, I18NEntity.GetString("RM_JS_JMD_Status_AddSiteCollectionRelatedColumn"), JobReportDetailStatus.Failed, string.Empty);
                        }
                    }
                    #endregion
                    #region enable physical current not enable physical in SiteCollection
                    //try
                    //{
                    //    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && curSetting.IsEnableHoldPhyical)//TO DO current logic only support sitecollection level enable physical.
                    //    {
                    //        SPSettingsUtility.ConfigPhysicalSetting(aveSite);
                    //        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                    //            string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Success, string.Empty);
                    //    }
                    //}
                    //catch (Exception he)
                    //{
                    //    isFailedEnablePhysical = true;
                    //    logger.Warn("enable physical setting failed {0}:{1}", aveSite.Url, he.ToString());
                    //    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                    //            string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Failed, he.Message);
                    //}
                    #endregion
                    #region remove unique id
                    try
                    {
                        if (curSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            if (SPSettingsUtility.DeleteUniqueIdColumn(aveSite))
                            {
                                AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                                   string.Empty, "RM_JS_JMD_Status_RemoveSiteCollectionUniqueId", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("remove Related app failed {0}:{1}", aveSite.Url, e.ToString());
                        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                                          string.Empty, "RM_JS_JMD_Status_RemoveSiteCollectionUniqueId", JobReportDetailStatus.Failed, string.Empty);
                    }
                    #endregion
                }
            }
        }
        public virtual void DoSettingAction(IAveWeb aveWeb, IAveSiteProperties siteProperties)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.Web", $"BaseSPSettingProcessor.Web.{aveWeb.Url}", true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    #region config container
                    try
                    {
                        if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            if (curSetting.TermIdOfContainer != Guid.Empty)
                            {
                                if (SPSettingsUtility.NeedUpdateContainer(aveWeb, curSetting.TermIdOfContainer))
                                {
                                    SPSettingsUtility.ConfigBCSProperty(siteProperties, aveWeb.Site.Url, aveWeb, curSetting.TermIdOfContainer);
                                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                       curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Success, string.Empty);
                                }
                                else
                                {
                                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Skipped, string.Empty);
                                }
                            }
                            else
                            {
                                SPSettingsUtility.ConfigBCSProperty(siteProperties, aveWeb.Site.Url, aveWeb, Guid.Empty);
                            }
                        }
                        else
                        {
                            if (SPSettingsUtility.RemoveBCSProperty(aveWeb))
                            {
                                AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveWebClassification", JobReportDetailStatus.Success, string.Empty);
                            }
                        }

                    }
                    catch (DenyAddAndCustomizePagesEnableExcetion de)
                    {
                        logger.Warn("Add web propery error {0}:{1}", aveWeb.Url, de.ToString());
                        isFailedAddContainer = true;
                        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                    }
                    catch (Exception e)
                    {
                        isFailedAddContainer = true;
                        logger.Warn("Add web propery error {0}:{1}", aveWeb.Url, e.ToString());
                        if (e.InnerException?.GetType() == typeof(ServerUnauthorizedAccessException))
                        {
                            AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetContainerLevelTermError");
                        }
                        else
                        {
                            AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                        }
                    }
                    #endregion
                    #region enable related app
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        try
                        {
                            //RelatedRecordsAppUtility relatedUtility = new RelatedRecordsAppUtility(aveWeb.Site, aveWeb, aveWeb.Url);
                            if (curSetting.EnableRelatedRecords)
                            {
                                if (!hasInstalledNewRelatedApp.HasValue || !hasInstalledOldRelatedApp.HasValue)
                                {
                                    bool foundOldApp = false;
                                    bool foundNewApp = false;
                                    var appCatalogURL = string.Empty;
                                    try
                                    {
                                        appCatalogURL = aveWeb.GetTenantAppCatalogSite();
                                        var appSite = mfactory.CreateSite(appCatalogURL);
                                        var appList = appSite.RootWeb.Lists.GetListByName("Apps for SharePoint", false);
                                        if (appList != null)
                                        {
                                            foreach (var item in appList.Items)
                                            {
                                                string appProdId = string.Empty;
                                                if (item.FieldValues.TryGetValue("AppProductID", out object appProdIdObj))
                                                {
                                                    appProdId = appProdIdObj.ToString();
                                                }
                                                
                                                if (Guid.TryParse(appProdId, out Guid tempNewAppId) && SpfxNewRelatedAppProductId == tempNewAppId)
                                                {
                                                    foundNewApp = true;
                                                }

                                                if (Guid.TryParse(appProdId, out Guid tempOldAppId) && RelatedAppProductId == tempOldAppId)
                                                {
                                                    foundOldApp = true;
                                                }
                                            }
                                        }
                                        if (foundNewApp)
                                        {
                                            logger.Info("New Spfx Related App installed.");
                                        }
                                        else
                                        {
                                            logger.Info("New Spfx Related App NOT installed.");
                                        }
                                        if (foundOldApp)
                                        {
                                            logger.Info("Old Related App installed.");
                                        }
                                        else
                                        {
                                            logger.Info("Old Related App NOT installed.");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"Get new App in app catalog error: {e}");
                                    }
                                    hasInstalledNewRelatedApp = foundNewApp;
                                    hasInstalledOldRelatedApp = foundOldApp;
                                }

                                //bool needSkipEnableOldApp = hasInstalledNewRelatedApp.GetValueOrDefault() && !hasInstalledOldRelatedApp.GetValueOrDefault();

                                bool needSkipEnableOldApp = false;
                                if (hasInstalledNewRelatedApp.GetValueOrDefault())
                                {
                                    needSkipEnableOldApp = true;
                                    logger.Info("Found new app, so need skip enable old app logic");
                                }
                                if (!hasInstalledOldRelatedApp.GetValueOrDefault())
                                {
                                    needSkipEnableOldApp = true;
                                    logger.Info("Not found old app, so need skip enable old app logic");
                                }

                                if (!needSkipEnableOldApp)
                                {
                                    SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, aveWeb.Site.Url);
                                    AddTenantIdInWeb(aveWeb);
                                    var apps = aveWeb.GetAppInstancesByProductId(RelatedAppProductId);
                                    if (apps != null && apps.Count > 0)
                                    {
                                        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                           string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Skipped, string.Empty);
                                    }
                                    else
                                    {
                                        TryAddRelateAppByServiceAccount(aveWeb);

                                        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                           string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Success, string.Empty);
                                    }
                                }
                            }
                            else
                            {
                                //RemoveTenantIdInWeb(aveWeb);
                                try
                                {
                                    var apps = aveWeb.GetAppInstancesByProductId(RelatedAppProductId);
                                    if (apps.Count > 0)
                                    {
                                        apps.FirstOrDefault()?.Uninstall();
                                        logger.Info("remove app {0}", aveWeb.Url);
                                        AddDetail(
                                            aveWeb.Title, aveWeb.Url, string.Empty, string.Empty,
                                            "RM_JMD_RemoveApp", JobReportDetailStatus.Success, string.Empty);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("Uninstall app failed {0}", ex.ToString());
                                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                    string.Empty, "RM_JMD_RemoveApp", JobReportDetailStatus.Failed, string.Empty);
                                }
                                //relatedUtility.UninstallApp(RelatedAppProductId);
                            }
                        }
                        catch (DenyAddAndCustomizePagesEnableExcetion e)
                        {
                            isFailedEnableApp = true;
                            logger.Error("set tenant id to web error:{0}", e.ToString());
                            AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                              string.Empty, I18NEntity.GetString("RM_JMD_AddApp"), JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                        }
                        catch (Exception ae)
                        {
                            isFailedEnableApp = true;
                            if (ae.InnerException?.GetType() == typeof(ServerUnauthorizedAccessException))
                            {
                                logger.Error("Try set tenant id to web error:{0}", ae.ToString());
                                AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                  string.Empty, I18NEntity.GetString("RM_JMD_AddApp"), JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                            }
                            else
                            {
                                logger.Warn("Enable app failed {0}:{1}", aveWeb.Url, ae.ToString());
                                AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                        string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Failed, "RM_SS_AddAppError");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (SPSettingsUtility.UninstallApp(aveWeb, RelatedAppProductId))
                            {
                                AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                       string.Empty, "RM_JMD_RemoveApp", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                        catch (Exception e)
                        {
                            isFailedEnableApp = true;
                            logger.Warn("remove app failed {0}:{1}", aveWeb.Url, e.ToString());
                            AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                    string.Empty, "RM_JMD_RemoveApp", JobReportDetailStatus.Failed, string.Empty);
                        }
                    }
                    #endregion
                }
            }
        }

        private GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection GetCurrentRemoteSite()
        {
            SPTreeNodeDto site = this.GetSiteCollectionNode(curNodeInfo);
            if (currentRemoteSite == null || currentRemoteSite.url != site.Url)
            {
                currentRemoteSite = new SharePointSettingUtility().GetRemoteSiteCollection(site.SPObjectId.ToString());
            }
            return currentRemoteSite;
        }

        private AveBPOSAccountInfo GetServiceAccountInfo()
        {
            GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSite = GetCurrentRemoteSite();
            if (remoteSite != null)
            {
                var userName = string.Empty;
                if (string.IsNullOrEmpty(remoteSite.username))
                {
                    userName = RA.Common.Aos.RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, remoteSite.TenantId).FirstOrDefault()?.UserName;
                }
                else
                {
                    userName = remoteSite.username;
                }

                var accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                {
                    Domain = remoteSite.domain,
                    UserName = userName,
                    Password = RA.Common.Aos.RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, userName).ToSecureStringWithEmptyCheck(),
                    AdminUrl = remoteSite.AdminUrl,
                    ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    TenantId = remoteSite.TenantId
                };
                logger.Info($"Successfullly init MFA service account, in site {remoteSite.url} : {accountInfo.AdminUrl}: {accountInfo.ConnectionType}");
                return accountInfo;
            }
            else
            {
                logger.Warn("No service account found in site, or remote site is null");
                return null;
            }
        }

        private void TryAddRelateAppByServiceAccount(IAveWeb aveWeb)
        {
            var previousStatus = WrapperConfiguration.AddAPPByServiceAccount;
            var previousAccount = WrapperConfiguration.accountInfo;
            try
            {
                if (!WrapperConfiguration.AddAPPByServiceAccount)
                {
                    WrapperConfiguration.AddAPPByServiceAccount = true;
                    WrapperConfiguration.accountInfo = new List<AveBPOSAccountInfo>() { this.GetServiceAccountInfo() };
                }

                aveWeb.DeployApp(RelatedAppProductId, AveRestoreMode.Default);

            }
            catch (Exception e)
            {
                logger.Error($"Add related app failed, error :{e}");
                throw;
            }
            finally
            {
                WrapperConfiguration.AddAPPByServiceAccount = previousStatus;
                WrapperConfiguration.accountInfo = previousAccount;
            }
        }

        private bool ListHasRequiredFields(IAveList list)
        {
            if (list.Hidden)
            {
                return false;
            }
            bool hasComments = list.Fields.ContainsField(CSDFieldName.Comments);
            bool hasEventDate = list.Fields.ContainsField(CSDFieldName.EventDate);
            bool hasDeletionDate = list.Fields.ContainsField(CSDFieldName.DeletionDate);
            //bool hasDocOwner = list.Fields.ContainsField(CSDFieldName.DocOwner);
            bool hasExtends = list.Fields.ContainsField(CSDFieldName.Extends);
            return list.BaseTemplate == AveListTemplateType.WebPageLibrary ?
                hasDeletionDate && hasExtends
                : hasComments && hasEventDate && hasDeletionDate && hasExtends;
        }
        public virtual async System.Threading.Tasks.Task DoSettingActionAsync(IAveList aveList)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.ProcessList", $"BaseSPSettingProcessor.ProcessList.{aveList.RootFolder.ServerRelativeUrl}", true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    bool listLevelFailedAddBCS = false;
                    string listUrl = aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url;
                    #region Check CSD Setting
                    if (JobContext.IsCSDTenant)
                    {
                        if (mConfigSiteSetting.ExcludeList.Contains(aveList.Title.ToLowerInvariant()))
                        {
                            logger.Info($"Skip library for library is in ExcludeList. Library Path:[{listUrl}]");
                            AddDetail(aveList.Title, listUrl, string.Empty,
                                string.Empty, "", JobReportDetailStatus.Skipped, I18NEntity.GetString("RM_SS_SkipExcludeList"));
                            return;
                        }
                        if (!ListHasRequiredFields(aveList))
                        {
                            logger.Info($"Current library does not have required fields. Library Path:[{listUrl}]");
                            AddDetail(aveList.Title, listUrl, string.Empty,
                                string.Empty, "", JobReportDetailStatus.Skipped, I18NEntity.GetString("RM_SS_ListNeedRequiredFields"));
                            return;
                        }
                        if (IsSpecialCSDClass())
                        {
                            isFailedAddBCS = true;
                            logger.Info($"Skip list for default Term: [{curSetting.DefaultTermId}] is white class or modfied based class. Library Path:[{listUrl}]");
                            AddDetail(aveList.Title, listUrl, curSetting.ColumnName,
                                    string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_JS_JMD_SpecialCSDClassCannotBeDefaultTerm");
                            return;
                        }
                    }
                    #endregion
                    #region add bcs
                    IAveTaxonomyField taxField = null;
                    string columnName = curSetting.IsUsingExistColumnName ? curSetting.ExistColumnName : curSetting.ColumnName;
                    var site = aveList.ParentWeb.Site;

                    if (!mSiteLevelAddBCSFailed)
                    {
                        try
                        {
                            var result = JobContext.IsCSDTenant ?
                                SPSettingsUtility.ConfigBCSColumn(site, aveList, curSetting, ref taxField, mConfigSiteSetting)
                                : SPSettingsUtility.ConfigBCSColumn(site, aveList, curSetting, ref taxField);
                            if (curSetting.IsUsingExistColumnName)
                            {
                                columnName = AppendDisplayName4Details(taxField, columnName);
                            }
                            if (result == SettingResult.Add)
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_AddListColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.SKip)
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_SkipListColumn", JobReportDetailStatus.Skipped, string.Empty);
                            }
                            else if (result == SettingResult.Update)
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_UpdateListColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.UseExistSkip)
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                       string.Empty, "RM_RC_Audit_Action_SaveGlobalSettintExistColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.Delete || result == SettingResult.SkipDelete)
                            {
                                var status = result == SettingResult.Delete ? JobReportDetailStatus.Success : JobReportDetailStatus.Skipped;
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                       string.Empty, "RM_JS_JMD_Status_RemoveBCSColumn", status, string.Empty);
                            }
                        }
                        catch (Exception ex)
                        {
                            listLevelFailedAddBCS = true;
                            isFailedAddBCS = true;
                            logger.Error("Failed to add list bcs column {0}", ex.ToString());
                            string errorMsg = ex.Message;
                            if (errorMsg.Contains("Term Is Unavailable"))
                            {
                                errorMsg = "RM_SS_ConfigureColumnFailed";
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                    string.Empty, "RM_JS_JMD_Status_AddListColumn", JobReportDetailStatus.Failed, errorMsg);
                            }
                            else if (ex.Message.Contains(I18NEntity.GetString("RM_SPS_CanNotFindExistingColumn")))
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SPS_CanNotFindExistingColumn");
                            }
                            else if (ex.Message.Contains("Parameter 'siteField'"))
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SPS_ExistingColumnTypeError");
                            }
                            else if(ex.Message.Contains("This site has exceeded its maximum file storage limit."))
                            {
                                AddDetail(aveList.Title, siteNode.FullPath, columnName,
                                string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SS_SiteStorageLimitExceeded");
                            }
                            else
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(ex));
                            }

                        }




                    }
                    if (!mSiteLevelAddBCSFailed && !listLevelFailedAddBCS)
                    {
                        var needCheckDocumentLevel = curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn));

                        if (needCheckDocumentLevel)
                        {
                            try
                            {
                                if (NeedApply(curSetting) && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                                {
                                    //SPDicoverCache.Instance.ListCache.Init(aveList);
                                    bool hasFailedItem = SPSettingsUtility.ApplyExistItems(new Guid(siteNode.ID), aveList, aveList.RootFolder, taxField, curSetting, curRecords, mLabelUtility, mConfigSiteSetting);
                                    if (!applyTermHasError && hasFailedItem)
                                    {
                                        applyTermHasError = true;
                                    }

                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                            string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Success, string.Empty);//need to do i18n

                                }
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception ae)
                            {
                                if (!JobContext.IsCSDTenant && curSetting.IsKeepSharePointDefaultValue)
                                {
                                    logger.Warn("Skipped to apply exist items [{0}]:{1}", aveList.RootFolder.ServerRelativeUrl, ae.ToString());
                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                            string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Skipped, String.Empty);
                                }
                                else
                                {
                                    applyTermHasError = true;
                                    logger.Warn("Failed to apply exist items [{0}]:{1}", aveList.RootFolder.ServerRelativeUrl, ae.ToString());
                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(ae));
                                }
                            }
                        }
                        #region Auto-Classification
                        if (needCheckDocumentLevel)
                        {
                            try
                            {
                                if ((DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary)
                                {
                                    DateTime startTime = new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.AutoClassification, Guid.Empty, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                    DateTime actionTime = DateTime.UtcNow;
                                    bool jobHasError = false;

                                    SPSettingsUtility.Autoclassification(new Guid(siteNode.ID), aveList, aveList.RootFolder, taxField, curSetting, startTime, actionTime, curRecords, ref jobHasError, mLabelUtility, mConfigSiteSetting);

                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Success, string.Empty);


                                    if (!jobHasError)
                                    {
                                        RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                                        {
                                            NodeId = aveList.ParentWeb.Site.ID,
                                            ListId = aveList.ID,
                                            FolderId = Guid.Empty,
                                            Title = aveList.Title,
                                            FullPath = aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url,
                                            CollectionTime = actionTime.Ticks,
                                            GroupId = new Guid(groupNode.ID),
                                            IsRemoved = false,
                                            NodeFlagType = (int)NodeFlagType.AutoClassification
                                        });
                                    }
                                    else
                                    {
                                        autoApplyTermHasError = true;
                                    }
                                }
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception e)
                            {
                                autoApplyTermHasError = true;
                                logger.Warn("Failed to apply auto classification rules {0}:{1}", aveList.RootFolder.Url, e.ToString());
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(e));
                            }
                        }
                        #endregion

                        #region AI
                        if (needCheckDocumentLevel)
                        {
                            try
                            {
                                if ((DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification && curSetting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                                {
                                    DateTime startTime = new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.IntelligenceClassification, Guid.Empty, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                    DateTime actionTime = DateTime.UtcNow;

                                    bool hasFailedItem = await SPSettingsUtility.SetIntelligenceClassificationAsync(new Guid(siteNode.ID), aveList, aveList.RootFolder, taxField, curSetting, startTime, actionTime, curRecords, mLabelUtility, mConfigSiteSetting);

                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                            string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Success, string.Empty);//need to do i18n

                                    if (!hasFailedItem)
                                    {
                                        RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                                        {
                                            NodeId = aveList.ParentWeb.Site.ID,
                                            ListId = aveList.ID,
                                            FolderId = Guid.Empty,
                                            Title = aveList.Title,
                                            FullPath = aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url,
                                            CollectionTime = actionTime.Ticks,
                                            GroupId = new Guid(groupNode.ID),
                                            IsRemoved = false,
                                            NodeFlagType = (int)NodeFlagType.IntelligenceClassification
                                        });
                                    }
                                    else
                                    {
                                        aiApplyTermHasError = true;
                                    }
                                }
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (CallPredictServiceException e)
                            {
                                aiApplyTermHasError = true;
                                logger.Warn("Failed to call predict term service, {0}:{1}", aveList.RootFolder.Url, e.ToString());
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Failed, "RM_ML_CallPredictService_Failed_Message");
                            }
                            catch (Exception e)
                            {
                                aiApplyTermHasError = true;
                                logger.Warn("Failed to apply by ai {0}:{1}", aveList.RootFolder.Url, e.ToString());
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(e));
                            }
                        }
                        #endregion
                    }

                    #endregion
                    #region enable related
                    try
                    {
                        if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            if (curSetting.EnableRelatedRecords)
                            {
                                if (!SPSettingsUtility.VerifyExistsSiteRelatedColumn(site))
                                {
                                    try
                                    {
                                        JobReportDetailStatus detailStatus = SPSettingsUtility.AddSiteCollectionRelatedColumn(site) ? JobReportDetailStatus.Success : JobReportDetailStatus.Skipped;
                                        AddDetail(site.RootWeb.Title, site.RootWeb.Url, string.Empty,
                                           string.Empty, "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", detailStatus, string.Empty);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info("add site related column on list failed, url: {0}, error message:{1}", site.Url, ex.ToString());
                                        AddDetail(site.RootWeb.Title, site.RootWeb.Url, string.Empty,
                                           string.Empty, "RM_JS_JMD_Status_AddSiteCollectionRelatedColumn", JobReportDetailStatus.Failed, string.Empty);
                                    }
                                }
                                bool result = SPSettingsUtility.AddListRelatedColumn(site, aveList);
                                if (result)
                                {
                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                            string.Empty, "RM_JS_JMD_Status_AddListRelatedColumn", JobReportDetailStatus.Success, string.Empty);
                                }
                                else
                                {
                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                        string.Empty, "RM_JS_JMD_Status_AddListRelatedColumn", JobReportDetailStatus.Skipped, string.Empty);
                                }
                            }
                        }
                        else
                        {
                            if (SPSettingsUtility.DeleteListRelatedColumn(site, aveList))
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                            string.Empty, "RM_JMD_RemoveApp", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("add related column failed {0}:{1}", aveList.RootFolder.Url, e.ToString());
                        AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                    string.Empty, "RM_JS_JMD_Status_AddListRelatedColumn", JobReportDetailStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), Common.Util.GetExceptionMessage(e)));
                    }
                    #endregion
                    #region add bcs property
                    try
                    {
                        if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            if (curSetting.TermIdOfContainer != Guid.Empty)
                            {
                                if (SPSettingsUtility.NeedUpdateContainer(aveList, curSetting.TermIdOfContainer))
                                {
                                    SPSettingsUtility.ConfigBCSProperty(aveList, curSetting.TermIdOfContainer);

                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Success, string.Empty);
                                }
                                else
                                {
                                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                      curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Skipped, string.Empty);
                                }
                            }
                            else
                            {
                                SPSettingsUtility.ConfigBCSProperty(aveList, Guid.Empty);
                            }
                        }
                        else
                        {
                            if (SPSettingsUtility.RemoveBCSProperty(aveList))
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                          curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveListClassification", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Add web propery error {0}:{1}", aveList.RootFolder.Url, e.ToString());
                        isFailedAddContainer = true;
                        AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                    }
                    #endregion
                    #region remove unique id
                    try
                    {
                        if (curSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            if (SPSettingsUtility.DeleteUniqueIdColumn(aveList))
                            {
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                                     curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveListUniqueId", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("remove unique column failed {0}:{1}", aveList.RootFolder.Url, e.ToString());
                        AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                                             curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveListUniqueId", JobReportDetailStatus.Failed, string.Empty);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Library or other high level node "apply default value setting" will be false, only folder level setting should be ture.
        /// </summary>
        /// <param name="aveFolder"></param>
        /// <param name="applyDefaultValueSetting"></param>
        public virtual async System.Threading.Tasks.Task DoSettingActionAsync(IAveFolder aveFolder, bool applyDefaultValueSetting = true)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.ProcessFolder", $"BaseSPSettingProcessor.ProcessFolder.{aveFolder.ServerRelativeUrl}", true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    #region add bcs
                    IAveTaxonomyField taxField = null;
                    var aveList = aveFolder.ParentList;
                    var site = aveList.ParentWeb.Site;

                    string columnName = curSetting.IsUsingExistColumnName ? curSetting.ExistColumnName : curSetting.ColumnName;
                    if (applyDefaultValueSetting)
                    {
                        #region folder default value setting
                        try
                        {
                            var folderPath = aveList.ParentWeb.Url + "/" + aveFolder.Url;
                            if (JobContext.IsCSDTenant && IsSpecialCSDClass())
                            {
                                isFailedAddBCS = true;
                                logger.Info($"Skip folder for default Term: [{curSetting.DefaultTermId}] is white class or modfied based class. folder Path:[{folderPath}]");
                                AddDetail(aveList.Title, folderPath, curSetting.ColumnName,
                                        string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_JS_JMD_SpecialCSDClassCannotBeDefaultTerm");
                                return;
                            }
                            var result = JobContext.IsCSDTenant ?
                                SPSettingsUtility.ConfigBCSColumn(site, aveList, aveFolder, curSetting, ref taxField, mConfigSiteSetting)
                                : SPSettingsUtility.ConfigBCSColumn(site, aveList, aveFolder, curSetting, ref taxField);

                            if (curSetting.IsUsingExistColumnName)
                            {
                                columnName = AppendDisplayName4Details(taxField, columnName);
                            }
                            if (result == SettingResult.Add)
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_AddFolderColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.SKip)
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_SkipFolderColumn", JobReportDetailStatus.Skipped, string.Empty);
                            }
                            else if (result == SettingResult.Update)
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                        string.Empty, "RM_JS_JMD_Status_UpdateFolderColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.UseExistSkip)
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                       string.Empty, "RM_RC_Audit_Action_SaveGlobalSettintExistColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                            else if (result == SettingResult.Delete || result == SettingResult.SkipDelete)
                            {
                                //folder level don't need delete column
                                //AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, curSetting.ColumnName,
                                //       string.Empty, "RM_JS_JMD_Status_RemoveBCSColumn", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Failed to add folder bcs column {0}", ex.ToString());
                            isFailedAddBCS = true;
                            string errorMsg = ex.Message;
                            if (errorMsg.Contains("Term Is Unavailable"))
                            {
                                errorMsg = "RM_SS_ConfigureColumnFailed";
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                    string.Empty, "RM_JS_JMD_Status_AddFolderColumn", JobReportDetailStatus.Failed, errorMsg);
                            }
                            else if (ex.Message.Contains(I18NEntity.GetString("RM_SPS_CanNotFindExistingColumn")))
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SPS_CanNotFindExistingColumn");
                            }
                            else if(ex.Message.Contains("This site has exceeded its maximum file storage limit."))
                            {
                                AddDetail(aveFolder.Name, siteNode.FullPath, columnName,
                                string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, "RM_SS_SiteStorageLimitExceeded");
                            }
                            else
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                   string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(ex));
                            }

                        }
                        #endregion
                    }
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn)))
                    {
                        //apply exists for folder
                        try
                        {
                            if (taxField == null)
                            {
                                taxField = SPSettingsUtility.GetTaxonomyField(aveFolder.ParentList, curSetting);
                            }
                            if (NeedApply(curSetting) && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                            {
                                //SPDicoverCache.Instance.ListCache.Init(aveList);
                                bool setTermForFolderSelf = JobContext.IsCSDTenant;
                                bool hasFailedItem = SPSettingsUtility.ApplyExistItems(new Guid(siteNode.ID), aveList, aveFolder, taxField, curSetting, curRecords, mLabelUtility, mConfigSiteSetting, setTermForFolderSelf: true);
                                if (!applyTermHasError && hasFailedItem)
                                {
                                    applyTermHasError = true;
                                }
                                if (!setTermForFolderSelf)
                                {
                                    AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                        string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Success, string.Empty);
                                }
                            }
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ae)
                        {
                            applyTermHasError = true;
                            logger.Warn("Failed to apply exist items [{0}]:{1}", aveFolder.ServerRelativeUrl, ae.ToString());
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                        string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(ae));
                        }
                    }
                    #region Auto-Classification
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn)))
                    {
                        try
                        {
                            if (taxField == null)
                            {
                                taxField = SPSettingsUtility.GetTaxonomyField(aveFolder.ParentList, curSetting);
                            }
                            if ((DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary)
                            {
                                DateTime startTime = new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.AutoClassification, aveFolder.UniqueId, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                DateTime actionTime = DateTime.UtcNow;
                                bool jobHasError = false;

                                SPSettingsUtility.Autoclassification(new Guid(siteNode.ID), aveFolder.ParentList, aveFolder, taxField, curSetting, startTime, actionTime, curRecords, ref jobHasError, mLabelUtility, mConfigSiteSetting, setTermForFolderSelf: true);
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Success, string.Empty);


                                if (!jobHasError)
                                {
                                    RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                                    {
                                        NodeId = aveList.ParentWeb.Site.ID,
                                        ListId = aveList.ID,
                                        FolderId = aveFolder.UniqueId,
                                        Title = aveFolder.Name,
                                        FullPath = aveList.ParentWeb.Url + "/" + aveFolder.Url,
                                        CollectionTime = actionTime.Ticks,
                                        GroupId = new Guid(groupNode.ID),
                                        IsRemoved = false,
                                        NodeFlagType = (int)NodeFlagType.AutoClassification
                                    });
                                }
                                else
                                {
                                    autoApplyTermHasError = true;
                                }
                            }
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            autoApplyTermHasError = true;
                            logger.Warn("Failed to apply auto classification rules {0}:{1}", aveFolder.Url, e.ToString());
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(e));
                        }
                    }
                    #endregion

                    #region AI
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                            (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn)))
                    {
                        try
                        {
                            if ((DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseIntelligenceClassification && curSetting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                            {
                                DateTime startTime = new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.IntelligenceClassification, aveFolder.UniqueId, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                DateTime actionTime = DateTime.UtcNow;
                                bool hasFailedItem = await SPSettingsUtility.SetIntelligenceClassificationAsync(new Guid(siteNode.ID), aveList, aveFolder, taxField, curSetting, startTime, actionTime, curRecords, mLabelUtility, mConfigSiteSetting);

                                if (!hasFailedItem)
                                {
                                    RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                                    {
                                        NodeId = aveList.ParentWeb.Site.ID,
                                        ListId = aveList.ID,
                                        FolderId = aveFolder.UniqueId,
                                        Title = aveFolder.Name,
                                        FullPath = aveList.ParentWeb.Url + "/" + aveFolder.Url,
                                        CollectionTime = actionTime.Ticks,
                                        GroupId = new Guid(groupNode.ID),
                                        IsRemoved = false,
                                        NodeFlagType = (int)NodeFlagType.IntelligenceClassification
                                    });
                                }
                                else
                                {
                                    aiApplyTermHasError = true;
                                }

                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (CallPredictServiceException e)
                        {
                            aiApplyTermHasError = true;
                            logger.Warn("Failed to call predict term service {0}:{1}", aveList.RootFolder.Url, e.ToString());
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Failed, "RM_ML_CallPredictService_Failed_Message");
                        }
                        catch (Exception e)
                        {
                            aiApplyTermHasError = true;
                            logger.Warn("Failed to apply by ai {0}:{1}", aveList.RootFolder.Url, e.ToString());
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAIClassification", JobReportDetailStatus.Failed, Common.Util.GetExceptionMessage(e));
                        }
                    }
                    #endregion

                    #endregion
                    #region add bcs property
                    try
                    {
                        if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                        {
                            if (curSetting.TermIdOfContainer != Guid.Empty)
                            {
                                if (SPSettingsUtility.NeedUpdateContainer(aveFolder, curSetting.TermIdOfContainer))
                                {
                                    SPSettingsUtility.ConfigBCSProperty(aveFolder, curSetting.TermIdOfContainer);
                                    AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, string.Empty,
                                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Success, string.Empty);
                                }
                                else
                                {
                                    AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, string.Empty,
                                      curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Skipped, string.Empty);
                                }
                            }
                            else
                            {
                                SPSettingsUtility.ConfigBCSProperty(aveFolder, Guid.Empty);
                            }
                        }
                        else
                        {
                            if (SPSettingsUtility.RemoveBCSProperty(aveFolder))
                            {
                                AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, string.Empty,
                                          curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveListClassification", JobReportDetailStatus.Success, string.Empty);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Add web propery error {0}:{1}", aveFolder.Url, e.ToString());
                        AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, string.Empty,
                            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                    }
                    #endregion
                }
            }
        }

        private static string AppendDisplayName4Details(IAveTaxonomyField taxField, string columnName)
        {
            string title = taxField?.Title;
            if (!string.IsNullOrEmpty(title) && columnName != title)
            {
                columnName = $"{columnName} ({title})";
            }

            return columnName;
        }

        public void AddTenantIdInWeb(IAveWeb web)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.AddTenantIdInWeb"))
            {
                var tenantLogonGrouId = TenantLocalValue.LogonGroupId;
                if (web.AllProperties.ContainsKey(relatedId))
                {
                    web.AllProperties[relatedId] = tenantLogonGrouId;
                }
                else
                {
                    web.AllProperties.Add(relatedId, tenantLogonGrouId);
                }
                web.Update();

                web.ReloadWeb();
                if (!web.AllProperties.ContainsKey(relatedId))
                {
                    throw new Exception("Set property error. please check site DenyAddAndCustomizePages is disabled.");
                }
                var webPropTenantId = web.AllProperties[relatedId];
                if (webPropTenantId == null)
                {
                    throw new Exception("Set property error. please check site DenyAddAndCustomizePages is disabled.");
                }
            }
        }

        public void RemoveTenantIdInWeb(IAveWeb web)
        {
            using (var scope = new PerformanceScope("BaseSPSettingProcessor.RemoveTenantIdInWeb"))
            {
                //web.AllowUnsafeUpdates = true;
                if (web.AllProperties.ContainsKey(relatedId))
                {
                    //web.AllProperties.Remove(relatedId);
                    web.AllProperties[relatedId] = null;
                }
                web.Update();
            }
        }
        protected SPTreeNodeDto GetSiteCollectionNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        protected SPTreeNodeDto GetGroupNode(SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }
        protected bool CheckIsDesignList(IAveList list)
        {
            var listInfo = list.RootFolder.Name + ((int)list.BaseTemplate).ToString();//TO DO Debug
            return InnerCheckIsDesignList(list.Title, listInfo);
        }

        private bool InnerCheckIsDesignList(string listTitle, string listInfo)
        {
            logger.Info($"CheckIsDesignList, list name: {listTitle}, list info: {listInfo}");
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Check is DesignList error {0}", ex.ToString());
            }
            return isDesignList;
        }

        protected bool CheckIsDesignListAgain(AveDiscoverList list)
        {
            try
            {
                var listInfo = list.RootFolderUrl[(list.RootFolderUrl.LastIndexOf('/') + 1)..] + list.ServerTemplate;
                return InnerCheckIsDesignList(list.Name, listInfo);
            }
            catch (Exception ex)
            {
                logger.Warn("get list system setting error {0}", ex.ToString());
                return false;
            }
        }


        protected void AddDetail(string objectName, string sourceURL, string columnName, string container, string action, JobReportDetailStatus status, string message)
        {
            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = objectName;
            detail.SourceURL = sourceURL;
            detail.ColumnName = columnName;
            detail.Action = action;
            detail.Status = (JobDetailsStatus)status;
            detail.Comment = message;
            detail.Classification = container;
            ReportManager.SendJobDetail(detail);
            StatisticNodeStatus(detail.Status);
        }

        private void StatisticNodeStatus(JobDetailsStatus status)
        {
            
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    hasSuccessNode = true;
                    break;
                case JobDetailsStatus.Failed:
                case JobDetailsStatus.Exception:
                    hasErrorNode = true;
                    break;
                default:
                    break;
            }
        }

        private static bool NeedApply(RMSharePointSetting setting)
        {
            var applyFolders = setting.IsApplyTermIncludeFolder();
            var applyDocuments = setting.NeedCheckDefaultValue;
            return applyFolders || applyDocuments;
        }
    }
}
