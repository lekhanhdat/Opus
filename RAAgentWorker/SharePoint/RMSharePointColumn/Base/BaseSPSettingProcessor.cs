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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;

namespace AvePoint.RA.SharePoint.RMSharePointColumn.Base
{
    /// <summary>
    /// TO DO if support incremental job , 
    /// </summary>
    public abstract class BaseSPSettingProcessor
    {
        //private static readonly AveLogger logger = AveLogger.GetInstance(typeof(BaseSPSettingProcessor));
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(BaseSPSettingProcessor));
        // protected ConfigSiteSetting mConfigSiteSetting = null;
        protected readonly Guid RelatedAppProductId = new Guid("e1fa5ab5-0db3-4a7b-91b6-322b28de4116");
        protected readonly string relatedId = "RelatedId";
        protected List<string> DesignLists = new List<string>();
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        private static int rowLimit = -1;
        private GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection currentRemoteSite;
        protected SPTreeNodeDto curNodeInfo;
        protected IAveSite curSite;
        protected AveObjectModelFactory mfactory;
        protected SPTreeNodeDto siteNode;
        protected SPTreeNodeDto groupNode;
        protected RMSharePointOnPremiseSetting curSetting;
        protected IAveORecords curRecords;
        protected bool isFailedAddBCS = false;
        protected bool isFailedAddContainer = false;
        protected bool isFailedEnablePhysical = false;
        protected bool isFailedEnableApp = false;
        protected bool applyTermHasError = false;
        protected bool autoApplyTermHasError = false;
        private bool mSiteLevelAddBCSFailed = false;
        protected bool hasSuccessfulNode = false;
        protected bool hasFailedNode = false;
        public IProgressService ProgressService { get; set; }
        public IReportService<JMJobDetails> JobDetailService { get; set; }

        private List<AvePoint.RA.Contract.Global.Object.NodeFlag> mNodeFlags = new List<NodeFlag>();
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

        public bool HasSuccessfulNode
        {
            get
            {
                return hasSuccessfulNode;
            }
        }

        public bool HasFailedNode
        {
            get
            {
                return hasFailedNode;
            }
        }

        protected bool UseServerApi = false;
        //private List<JMGlobalSettingJobDetails> jobDetails = new List<JMGlobalSettingJobDetails>();
        //private BaseJobDto mBaseJobDto;

        public BaseSPSettingProcessor()
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
        }
        public BaseSPSettingProcessor(SPTreeNodeDto nodeInfo)
        {
            //init job info
            siteNode = GetSiteCollectionNode(nodeInfo);
            groupNode = GetGroupNode(nodeInfo);
            this.DesignLists = WebUtil.GetDesignLists();//TO DO debug design list config file path later
                                                        //ProgressService.Increase();
                                                        //mBaseJobDto = jobDto;
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
        }
        public virtual void ProcessSiteCollection()
        {
            //ProgressService.Increase();
        }
        public virtual void ProcessWeb()
        {
            //ProgressService.Increase();
        }
        public virtual void ProcessList()
        {
            //ProgressService.Increase();
        }
        public virtual void Run()
        {
            if (mNodeFlags.Count > 0)
            {
                UpdateNodeFlags();
            }
            //Update setting status.......
            StringBuilder errorMessage = new StringBuilder();
            JobState status = JobState.Finished;
            if (isFailedAddBCS || isFailedAddContainer || isFailedEnablePhysical || isFailedEnableApp)
            {
                status = JobState.FinishedException;
                errorMessage.Append("RM_TS_SS_Summary");
            }
            //if (isFailedAddBCS)
            //{
            //    status = JobState.FinishedException;
            //    errorMessage.Append("I18N Faild add bcs ");
            //}
            //if (isFailedAddContainer)
            //{
            //    status = JobState.FinishedException;
            //    errorMessage.Append("I18N Failed add container property");
            //}
            //if (isFailedEnablePhysical)
            //{
            //    status = JobState.FinishedException;
            //    errorMessage.Append("I18N Failed Enable physical");
            //}
            //if (isFailedEnableApp)
            //{
            //    status = JobState.FinishedException;
            //    errorMessage.Append("I18N Failed Eable app");
            //}
            //logger.Info("Sub job end {0}", JobContext.Current.JobMessage.SubJobId);
            try
            {
                //SharePointSettingDao.SetSettingJobTime(curSetting.ScopeId, curSetting.SiteId, isFailedAddBCS, isFailedAddContainer);
                using (var performance = new AgentPerformanceScope("BaseSPSettingProcessor.SetSettingJobTime", addToStatistics: true))
                {
                    HybridApiClient.Instance.SetSettingJobTime(curSetting.ScopeId, curSetting.SiteId, isFailedAddBCS, isFailedAddContainer);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Update status error {0}", e.ToString());
            }
            //JobContext.Current.Cleanup();
            //JobContext.Current.JobSummaryService.NotifyManager(status, errorMessage.ToString());
        }

        private void UpdateNodeFlags()
        {
            for (int i = 0; i < mNodeFlags.Count; i += 100)
            {
                var nodeFlags = mNodeFlags.Skip(i).Take(100).ToList();
                using (var performance = new AgentPerformanceScope("BaseSPSettingProcessor.UpdateAutoJobCollectionTime", $"BaseSPSettingProcessor.UpdateAutoJobCollectionTime.Count:{nodeFlags.Count}", true))
                {
                    HybridApiClient.Instance.UpdateAutoJobCollectionTime(nodeFlags);
                }
            }
        }
        public virtual void SetModuleFactoryForAuto(AveObjectModelFactory factory)
        {
            SPSettingsUtility.factoryForAuto = factory;
            //SPSettingsUtility.discoverFactoryForAuto = discoverFactory;
            //discoverFactory.CreateDiscoverList()
        }
        public virtual void DoSettingAction(IAveSite aveSite, IAveSiteProperties siteProperties)
        {
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.ProcessSitecollection", $"BaseSPSettingProcessor.ProcessList.{aveSite.Url}", true))
            {
                #region config bcs column
                string columnName = curSetting.IsUsingExistColumnName ? curSetting.ExistColumnName : curSetting.ColumnName;
                try
                {
                    SettingResult result = SPSettingsUtility.ConfigBCSColumn(aveSite, curSetting);
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
                    logger.Error("Add sitecollection level bcs column failed. Path:[{0}] Error:{1}", siteNode.FullPath.LogBase64(), ce.ToString());
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
                        //if (curSetting.TermIdOfContainer != null && curSetting.TermIdOfContainer != Guid.Empty)
                        //{
                        //    if (SPSettingsUtility.NeedUpdateContainer(aveSite.RootWeb, curSetting.TermIdOfContainer))
                        //    {
                        //        SPSettingsUtility.ConfigBCSProperty(siteProperties, aveSite.Url, aveSite.RootWeb, curSetting.TermIdOfContainer);//TO DO Rebuild Logic of job details.
                        //                                                                                                                        //TO DO Detail.
                        //        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                        //            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Success, string.Empty);
                        //    }
                        //    else
                        //    {
                        //        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                        //            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Skipped, string.Empty);
                        //    }
                        //}
                        //else
                        //{
                        //    SPSettingsUtility.ConfigBCSProperty(siteProperties, aveSite.Url, aveSite.RootWeb, Guid.Empty);
                        //}
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
                    logger.Warn("Config container property failed {0}:{1}", aveSite.Url.LogBase64(), de.ToString());
                    isFailedAddContainer = true;
                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                }
                catch (Exception e)
                {
                    logger.Warn("Config container property failed {0}:{1}", aveSite.Url.LogBase64(), e.ToString());
                    isFailedAddContainer = true;
                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateSiteCollectionClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
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
                            logger.Info("add site related column failed, site url: {0}, error message:{1}", aveSite.Url.LogBase64(), ex.ToString());
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
                #region enable physical
                try
                {
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && curSetting.IsEnableHoldPhyical)//TO DO current logic only support sitecollection level enable physical.
                    {
                        SPSettingsUtility.ConfigPhysicalSetting(aveSite);
                        AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                            string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Success, string.Empty);
                    }
                }
                catch (Exception he)
                {
                    isFailedEnablePhysical = true;
                    logger.Warn("enable physical setting failed {0}:{1}", aveSite.Url.LogBase64(), he.ToString());
                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                            string.Empty, "RM_SS_ConfigPhysicalAction", JobReportDetailStatus.Failed, he.Message);
                }
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
                    logger.Warn("remove Related app failed {0}:{1}", aveSite.Url.LogBase64(), e.ToString());
                    AddDetail(aveSite.RootWeb.Title, aveSite.RootWeb.Url, string.Empty,
                                                      string.Empty, "RM_JS_JMD_Status_RemoveSiteCollectionUniqueId", JobReportDetailStatus.Failed, string.Empty);
                }
                #endregion
            }
        }
        public virtual void DoSettingAction(IAveWeb aveWeb, IAveSiteProperties siteProperties)
        {
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.Web", $"BaseSPSettingProcessor.Web.{aveWeb.Url}", true))
            {
                #region config container
                try
                {
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        //if (curSetting.TermIdOfContainer != null && curSetting.TermIdOfContainer != Guid.Empty)
                        //{
                        //    if (SPSettingsUtility.NeedUpdateContainer(aveWeb, curSetting.TermIdOfContainer))
                        //    {
                        //        SPSettingsUtility.ConfigBCSProperty(siteProperties, aveWeb.Site.Url, aveWeb, curSetting.TermIdOfContainer);
                        //        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                        //           curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Success, string.Empty);
                        //    }
                        //    else
                        //    {
                        //        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                        //            curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Skipped, string.Empty);
                        //    }
                        //}
                        //else
                        //{
                        //    SPSettingsUtility.ConfigBCSProperty(siteProperties, aveWeb.Site.Url, aveWeb, Guid.Empty);
                        //}
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
                    logger.Warn("Add web propery error {0}:{1}", aveWeb.Url.LogBase64(), de.ToString());
                    isFailedAddContainer = true;
                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Failed, "RM_JMD_SPS_SetTenantIdError");
                }
                catch (Exception e)
                {
                    logger.Warn("Add web propery error {0}:{1}", aveWeb.Url.LogBase64(), e.ToString());
                    isFailedAddContainer = true;
                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateWebClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
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
                            //SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, aveWeb.Site.Url);
                            //AddTenantIdInWeb(aveWeb);
                            ////relatedUtility.AddAnApp(RelatedAppProductId);
                            //var apps = aveWeb.GetAppInstancesByProductId(RelatedAppProductId);
                            //if (apps != null && apps.Count > 0)
                            //{
                            //    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                            //       string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Skipped, string.Empty);
                            //}
                            //else
                            //{
                            //    if (mfactory.AccountInfo.ConnectionType == BposConnectionType.ServiceAccount)
                            //    {
                            //        aveWeb.DeployApp(RelatedAppProductId, AveRestoreMode.Default);//TO DO option
                            //    }
                            //    else
                            //    {
                            //        this.TryToEnableOrDisableAppUsingSA(aveWeb, true);
                            //    }
                            //    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                            //       string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Success, string.Empty);
                            //}
                        }
                        else
                        {
                            //RemoveTenantIdInWeb(aveWeb);
                            try
                            {
                                var apps = aveWeb.GetAppInstancesByProductId(RelatedAppProductId);
                                if (apps.Count > 0)
                                {
                                    apps.FirstOrDefault().Uninstall();
                                    logger.Info("remove app {0}", aveWeb.Url.LogBase64());
                                    AddDetail(
                                        aveWeb.Title, aveWeb.Url, string.Empty, string.Empty,
                                        I18NEntity.GetString("RM_JMD_RemoveApp"), JobReportDetailStatus.Success, string.Empty);
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
                        logger.Warn("Enable app failed {0}:{1}", aveWeb.Url.LogBase64(), ae.ToString());
                        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                string.Empty, "RM_JMD_AddApp", JobReportDetailStatus.Failed, "RM_SS_AddAppError");
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
                        logger.Warn("remove app failed {0}:{1}", aveWeb.Url.LogBase64(), e.ToString());
                        AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
                                string.Empty, "RM_JMD_RemoveApp", JobReportDetailStatus.Failed, string.Empty);
                    }
                }
                #endregion
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

        private Wrapper.Common.AveBPOSAccountInfo GetMFAAccountInfo()
        {
            GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSite = GetCurrentRemoteSite();
            if (remoteSite != null && remoteSite.username != null)
            {
                logger.Info("Successfullly get MFA account {0}, in site {1}", remoteSite.username.LogBase64(), remoteSite.id);

                //string password = RA.Common.Aos.RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, remoteSite.username);
                var accountInfo = new Wrapper.Common.AveBPOSAccountInfo();
                //{
                //    Domain = remoteSite.domain,
                //    UserName = remoteSite.username,
                //    Password = password,
                //    AdminUrl = remoteSite.AdminUrl,
                //    ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                //    TenantGroupId = TenantLocalValue.LogonGroupId,
                //    TenantId = remoteSite.TenantId
                //};
                logger.Info(accountInfo.ToString().LogBase64());
                return accountInfo;
            }
            else
            {
                logger.Warn("No MFA account found in site, or remote site is null");
                return null;
            }
        }

        private void TryToEnableOrDisableAppUsingSA(IAveWeb aveWeb, bool isEnable)
        {
            //var remoteSite = GetCurrentRemoteSite();
            //AveBPOSAccountInfo accountInfo = null;
            //if (remoteSite.AuthorizeType ==  GCommon.Contract.SharePointBrowser.AuthorizeType.AppTokenInfo)
            //{
            //    logger.Info("Current connection type is App Token, try to use MFA account to enable of disable app. is enable ? {0}", isEnable);
            //    accountInfo = this.GetMFAAccountInfo();
            //}
            //else
            //{
            //    logger.Info("Current connection type is Service Account, try to use service account to enable of disable app. is enable ? {0}", isEnable);
            //    accountInfo = PoolUserUtil.GetConnectionSAInfo(remoteSite);
            //}

            //if (accountInfo == null)
            //{
            //    throw new AveErrorException("Can't get an account to deploy and enable related APP");
            //}
            //IAveSite aveSite;
            //AveObjectModelFactory newfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, accountInfo, AveContextKind.ClientObjectModel);//TO DO Confirm Object Model.
            //try
            //{
            //    using (aveSite = newfactory.CreateSite(siteNode.FullPath))
            //    {
            //        using (IAveWeb cuWeb = aveSite.OpenWeb(aveWeb.ID))
            //        {
            //            if (isEnable)
            //            {
            //                cuWeb.DeployApp(RelatedAppProductId, AveRestoreMode.Default);//TO DO option
            //            }
            //            else
            //            {
            //                var apps = cuWeb.GetAppInstancesByProductId(RelatedAppProductId);
            //                if (apps.Count > 0)
            //                {
            //                    apps.FirstOrDefault().Uninstall();
            //                    logger.Info("remove app {0}", aveWeb.Url);
            //                    AddDetail(aveWeb.Title, aveWeb.Url, string.Empty,
            //                        string.Empty, I18NEntity.GetString("RM_JMD_RemoveApp"), JobReportDetailStatus.Success, string.Empty);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.Error(e.Message, e);
            //    throw e;
            //}
        }

        //private bool ListHasRequiredFields(IAveList list)
        //{
        //    if (list.Hidden)
        //    {
        //        return false;
        //    }
        //    bool hasComments = list.Fields.ContainsField(CSDFieldName.Comments);
        //    bool hasEventDate = list.Fields.ContainsField(CSDFieldName.EventDate);
        //    bool hasDeletionDate = list.Fields.ContainsField(CSDFieldName.DeletionDate);
        //    bool hasDocOwner = list.Fields.ContainsField(CSDFieldName.DocOwner);
        //    bool hasExtends = list.Fields.ContainsField(CSDFieldName.Extends);
        //    var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
        //    var matchs = reg.Match(list.ParentWeb.Site.Url);
        //    //if ((int)list.BaseTemplate == GCommon.Utility.GConstants.SPNodeTemplate.MySiteDocumentLibrary)
        //    //if (list.ParentWeb.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
        //    if (matchs.Success)
        //    {
        //        //VWREC-393  VWREC-404
        //        //One drive does not need to generate event date, event comment column
        //        //One drive needs to remove the judgment about event two column.
        //        if (!hasDeletionDate || !hasDocOwner || !hasExtends)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        if (!hasComments || !hasEventDate || !hasDeletionDate || !hasDocOwner || !hasExtends)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return true;
        //        }

        //    }
        //}
        public virtual void DoSettingAction(IAveList aveList)
        {
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.ProcessList", $"BaseSPSettingProcessor.ProcessList.{aveList.RootFolder.ServerRelativeUrl}", true))
            {
                bool listLevelFailedAddBCS = false;
                #region add bcs
                IAveTaxonomyField taxField = null;
                string columnName = curSetting.IsUsingExistColumnName ? curSetting.ExistColumnName : curSetting.ColumnName;
                var site = aveList.ParentWeb.Site;

                if (!mSiteLevelAddBCSFailed)
                {
                    try
                    {
                        var result = SPSettingsUtility.ConfigBCSColumn(site, aveList, curSetting, ref taxField);
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
                        else
                        {
                            AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                               string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(ex));
                        }

                    }

                }
                if (!mSiteLevelAddBCSFailed && !listLevelFailedAddBCS)
                {
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                        (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn)))
                    {
                        try
                        {
                            if (curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                            {
                                //SPDicoverCache.Instance.ListCache.Init(aveList);
                                bool hasFailedItem = SPSettingsUtility.ApplyExistItems(new Guid(siteNode.ID), aveList, aveList.RootFolder, taxField, curSetting, curRecords);
                                if (!applyTermHasError && hasFailedItem)
                                {
                                    applyTermHasError = true;
                                }
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                        string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Success, string.Empty);//need to do i18n
                            }
                        }
                        catch (Exception ae)
                        {
                            applyTermHasError = true;
                            logger.Warn("Failed to apply exist items [{0}]:{1}", aveList.RootFolder.ServerRelativeUrl.LogBase64(), ae.ToString());
                            AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                        string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(ae));
                        }
                    }
                    #region Auto-Classification
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable &&
                        (!curSetting.IsUsingExistColumnName || (curSetting.IsUsingExistColumnName && curSetting.SetDocLevelTermForExistColumn)))
                    {
                        try
                        {
                            if ((DeployTermMethod)curSetting.DeployTermMethod == DeployTermMethod.UseAutoClassification && aveList.BaseType == AveBaseType.DocumentLibrary)
                            {
                                DateTime startTime = DateTime.MinValue;

                                using (var performance = new AgentPerformanceScope("BaseSPSettingProcessor.GetAutoJobCollectionTime", addToStatistics: true))
                                {
                                    startTime = new DateTime(HybridApiClient.Instance.GetAutoJobCollectionTime(2, Guid.Empty, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                }
                                //new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.AutoClassification, Guid.Empty, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                                DateTime actionTime = DateTime.UtcNow;
                                bool jobHasError = false;

                                SPSettingsUtility.Autoclassification(new Guid(siteNode.ID), aveList, aveList.RootFolder, taxField, curSetting, startTime, actionTime, curRecords, ref jobHasError);
                                AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                        string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Success, string.Empty);

                                if (!jobHasError)
                                {
                                    mNodeFlags.Add(new NodeFlag()
                                    {
                                        NodeId = aveList.ParentWeb.Site.ID,
                                        ListId = aveList.ID,
                                        FolderId = Guid.Empty,
                                        Title = aveList.Title,
                                        FullPath = aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url,
                                        CollectionTime = actionTime.Ticks,
                                        GroupId = new Guid(groupNode.ID),
                                        IsRemoved = false,
                                        NodeFlagType = 2
                                    });
                                    //RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                                    //{
                                    //    NodeId = aveList.ParentWeb.Site.ID,
                                    //    ListId = aveList.ID,
                                    //    FolderId = Guid.Empty,
                                    //    Title = aveList.Title,
                                    //    FullPath = aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url,
                                    //    CollectionTime = actionTime.Ticks,
                                    //    GroupId = new Guid(groupNode.ID),
                                    //    IsRemoved = false,
                                    //    NodeFlagType = (int)NodeFlagType.AutoClassification
                                    //});
                                }
                                else
                                {
                                    autoApplyTermHasError = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            autoApplyTermHasError = true;
                            logger.Warn("Failed to apply auto classification rules {0}:{1}", aveList.RootFolder.Url.LogBase64(), e.ToString());
                            AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(e));
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
                                    logger.Info("add site related column on list failed, url: {0}, error message:{1}", site.Url.LogBase64(), ex.ToString());
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
                                        string.Empty, "RM_JMD_Related_RemoveColumn", JobReportDetailStatus.Success, string.Empty);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("add related column failed {0}:{1}", aveList.RootFolder.Url.LogBase64(), e.ToString());
                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                string.Empty, "RM_JS_JMD_Status_AddListRelatedColumn", JobReportDetailStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), AgentUtil.GetExceptionMessage(e)));
                }
                #endregion
                #region add bcs property
                try
                {
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        if (curSetting.TermIdOfContainer != null && curSetting.TermIdOfContainer != Guid.Empty)
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
                    logger.Warn("Add web propery error {0}:{1}", aveList.RootFolder.Url.LogBase64(), e.ToString());
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
                    logger.Warn("remove unique column failed {0}:{1}", aveList.RootFolder.Url.LogBase64(), e.ToString());
                    AddDetail(aveList.Title, aveList.ParentWeb.Url + "/" + aveList.RootFolder.Url, string.Empty,
                                                         curSetting.TermNameOfContainer, "RM_JS_JMD_Status_RemoveListUniqueId", JobReportDetailStatus.Failed, string.Empty);
                }
                #endregion
            }
        }

        /// <summary>
        /// Library or other high level node "apply default value setting" will be false, only folder level setting should be ture.
        /// </summary>
        /// <param name="aveFolder"></param>
        /// <param name="applyDefaultValueSetting"></param>
        public virtual void DoSettingAction(IAveFolder aveFolder, bool applyDefaultValueSetting = true)
        {
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.ProcessFolder", $"BaseSPSettingProcessor.ProcessFolder.{aveFolder.ServerRelativeUrl}", true))
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
                        var result = SPSettingsUtility.ConfigBCSColumn(site, aveList, aveFolder, curSetting, ref taxField);
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
                        else
                        {
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                               string.Empty, "RM_JS_JMD_Action_CheckColumnSetting", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(ex));
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
                        if (curSetting.NeedCheckDefaultValue && curSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                        {
                            //SPDicoverCache.Instance.ListCache.Init(aveList);
                            bool hasFailedItem = SPSettingsUtility.ApplyExistItems(new Guid(siteNode.ID), aveList, aveFolder, taxField, curSetting, curRecords);
                            if (!applyTermHasError && hasFailedItem)
                            {
                                applyTermHasError = true;
                            }
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                    string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Success, string.Empty);
                        }
                    }
                    catch (Exception ae)
                    {
                        applyTermHasError = true;
                        logger.Warn("Failed to apply exist items [{0}]:{1}", aveFolder.ServerRelativeUrl.LogBase64(), ae.ToString());
                        AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                    string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(ae));
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
                            DateTime startTime = DateTime.MinValue;
                            using (var performance = new AgentPerformanceScope("BaseSPSettingProcessor.GetAutoJobCollectionTime", addToStatistics: true))
                            {
                                startTime = new DateTime(HybridApiClient.Instance.GetAutoJobCollectionTime(2, aveFolder.UniqueId, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                            }
                            //new DateTime(RMNodeFlagDao.GetAutoJobCollectionTime((int)NodeFlagType.AutoClassification, aveFolder.UniqueId, aveList.ID, aveList.ParentWeb.Site.ID, new Guid(groupNode.ID)));
                            DateTime actionTime = DateTime.UtcNow;
                            bool jobHasError = false;

                            SPSettingsUtility.Autoclassification(new Guid(siteNode.ID), aveFolder.ParentList, aveFolder, taxField, curSetting, startTime, actionTime, curRecords, ref jobHasError);
                            AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                    string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Success, string.Empty);

                            if (!jobHasError)
                            {
                                mNodeFlags.Add(new NodeFlag()
                                {
                                    NodeId = aveList.ParentWeb.Site.ID,
                                    ListId = aveList.ID,
                                    FolderId = aveFolder.UniqueId,
                                    Title = aveFolder.Name,
                                    FullPath = aveList.ParentWeb.Url + "/" + aveFolder.Url,
                                    CollectionTime = actionTime.Ticks,
                                    GroupId = new Guid(groupNode.ID),
                                    IsRemoved = false,
                                    NodeFlagType = 2
                                });
                            }
                            else
                            {
                                autoApplyTermHasError = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        autoApplyTermHasError = true;
                        logger.Warn("Failed to apply auto classification rules {0}:{1}", aveFolder.Url.LogBase64(), e.ToString());
                        AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, columnName,
                                                string.Empty, "RM_JS_JMD_Action_SetAutoClassification", JobReportDetailStatus.Failed, AgentUtil.GetExceptionMessage(e));
                    }
                }
                #endregion

                #endregion
                #region add bcs property
                try
                {
                    if (curSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        if (curSetting.TermIdOfContainer != null && curSetting.TermIdOfContainer != Guid.Empty)
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
                    logger.Warn("Add web propery error {0}:{1}", aveFolder.Url.LogBase64(), e.ToString());
                    AddDetail(aveFolder.Name, aveList.ParentWeb.Url + "/" + aveFolder.Url, string.Empty,
                        curSetting.TermNameOfContainer, "RM_JS_JMD_Status_UpdateListClassification", JobReportDetailStatus.Failed, "RM_SS_ConfigureClassificationFailed");
                }
                #endregion
            }
        }

        public void AddTenantIdInWeb(IAveWeb web)
        {
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.AddTenantIdInWeb", addToStatistics: true))
            {
                var tenantLogonGrouId = "";
                //TenantLocalValue.LogonGroupId;
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
            using (var scope = new AgentPerformanceScope("BaseSPSettingProcessor.RemoveTenantIdInWeb", addToStatistics: true))
            {
                //web.AllowUnsafeUpdates = true;
                if (web.AllProperties.ContainsKey(relatedId))
                {
                    //web.AllProperties.Remove(relatedId);
                    web.AllProperties[relatedId] = null;
                }
                web.Update();
                //web.AllowUnsafeUpdates = false;
                //web.ReloadWeb();
                //var webPropTenantId = web.AllProperties[relatedId];
                //if (webPropTenantId != null)
                //{
                //    throw new Exception("remove property error. please check site DenyAddAndCustomizePages is disabled.");
                //}
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
        protected void AddDetail(string objectName, string sourceURL, string columnName, string container, string action, JobReportDetailStatus status, string message)
        {
            var detailStatus = (JobDetailsStatus)status;
            if(detailStatus == JobDetailsStatus.Successful || detailStatus == JobDetailsStatus.Skipped)
            {
                hasSuccessfulNode = true;
            }
            else
            {
                hasFailedNode = true;
            }

            JMGlobalSettingJobDetails detail = new JMGlobalSettingJobDetails();
            detail.ObjectName = objectName;
            detail.SourceURL = sourceURL;
            detail.ColumnName = columnName;
            detail.Action = action;
            detail.Status = detailStatus;
            detail.Comment = message;
            detail.Classification = container;
            detail.AgentName = OSInformation.HostName;
            JobDetailService.Commit(detail);
            //ReportManager.SendJobDetail(detail);
        }


        //private List<JMGlobalSettingJobDetails> CloneJobDetailsAddSCUrl(List<JMGlobalSettingJobDetails> details)
        //{
        //    List<JMGlobalSettingJobDetails> cloneDetails = new List<JMGlobalSettingJobDetails>();
        //    foreach (JMGlobalSettingJobDetails detail in details)
        //    {
        //        cloneDetails.Add(detail);
        //    }
        //    return cloneDetails;
        //}
        //public void RunUpdateJobDetails()
        //{
        //    List<JMGlobalSettingJobDetails> needUpdateDetails = this.CloneJobDetailsAddSCUrl(jobDetails);
        //    if (needUpdateDetails.Count == 0)
        //    {
        //        return;
        //    }
        //    JobDetailService.UpdateJobDetails(needUpdateDetails, mBaseJobDto);
        //}

    }
}
