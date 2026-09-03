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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMUniqueIDSettingProcessor
    {
        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(RMUniqueIDSettingProcessor));
        public RMUniqueIDSettingProcessor(string jobId, JobType jobType)
        {
            currentJobId = jobId;
            currentJobType = jobType;
            InitCurrentJobInfo();
            #region fips logic for now not used
            //FipsModeUtil.InitControlCryptoMode();
            //if (CspCommunicationWrapper.CommunicationEncryptionKey == null)
            //{
            //    RMCPDocAveConnection docave = DocAveConnectionDaoService.Find(a => a.Id > 0);
            //    DocAveServiceHelper.LoginToDocAve(docave.DocAveUsername, AvePoint.RA.Common.Util.EncodeUtil.Decode(docave.DocAvePassword));
            //}
            #endregion
        }

        #region for UniqueID Setting Job
        private string currentJobId;
        private int totalCount;
        private int progress = 0;
        private bool hasErrorNode = false;
        private bool hasSuccessNode = false;
        private string errorMessage = string.Empty;
        private JobType currentJobType;

        private void InitCurrentJobInfo()
        {
            baseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)currentJobType };
            if (currentJobType == JobType.UniqueIDSettingFullSchedule)
            {
                RMJobService.UpdateJobProgress(currentJobId, 1);//
            }
        }
        #endregion

        #region Interface
        private ISPSettingTreeService mSPTreeService;
        private IJobMonitorService mJobService;
        private ISharePointSettingDao mSharePointSettingDao;
        private BaseJobDto baseJobDto;
        private IJobDetailService mJobDetailService;
        private IRMSettingJobDao mSettingJobDao;
        private IUniqueIdSettingService mUniqueIdSettingService;
        protected IRMSettingJobDao RMSettingsJob
        {
            get
            {
                if (mSettingJobDao == null)
                {
                    mSettingJobDao = (IRMSettingJobDao)PlatformWindsorManager.GetService(typeof(IRMSettingJobDao));
                }
                return mSettingJobDao;
            }
        }
        protected IJobDetailService JobDetailService
        {
            get
            {
                if (mJobDetailService == null)
                {
                    mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
                }
                return mJobDetailService;
            }
        }
        protected ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }
        protected IJobMonitorService RMJobService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }
        protected ISPSettingTreeService RMSPTreeService
        {
            get
            {
                if (mSPTreeService == null)
                {
                    mSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPTreeService;
            }
        }
        protected IUniqueIdSettingService UniqueIdSettingService
        {
            get
            {
                if (mUniqueIdSettingService == null)
                {
                    mUniqueIdSettingService = (IUniqueIdSettingService)PlatformWindsorManager.GetService(typeof(IUniqueIdSettingService));
                }
                return mUniqueIdSettingService;
            }
        }
        #endregion

        public RMUniqueIDCoulumn RevIMConfig = new RMUniqueIDCoulumn();
        public void ApplyUniqueIDSetting()
        {
            UniqueIdSetting setting = UniqueIdSettingService.LoadingUniqueIdSetting();
            if (setting == null || !setting.IsActived)
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Failed, "not set uniqueid active");
                return;
            }
            RMSPTreeNode farmNode = RMSPTreeService.LoadFarm()[0];            //browse出当前选择的group node下所有的site collection
            Dictionary<string, List<RMSPTreeNode>> processNodesMap = GetTotalRMSPTreeNode(RMSPTreeService.Browse(farmNode), ref totalCount);
            if (totalCount == 0)
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                return;
            }
            
            foreach (var groupNode in RMSPTreeService.Browse(farmNode))
            {
                List<RMSPTreeNode> currentGroupNodes = processNodesMap[groupNode.SPObjectId];
                #region init group node 
                if (currentGroupNodes == null || currentGroupNodes.Count == 0)
                {
                    //List<JMUniqueIDSettingJobDetails> finalDetails = new List<JMUniqueIDSettingJobDetails>();
                    //finalDetails.Add(new JMUniqueIDSettingJobDetails() { ObjectName = groupNode.Name, SourceURL = groupNode.FullPath, ColumnName = groupNode.ColumnName, Action = "Skipped", Status = JobDetailsStatus.Skipped, Comment = "RM_SS_NoSCUnderGroup"});
                    //RunUpdateJobDetails(finalDetails);
                    continue;
                }

                var groupSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
                if (groupSetting == null)
                {
                    logger.Info("This group has not been set global setting {0}", groupNode.Name);
                    continue;
                }
                #endregion

                foreach (var siteNode in currentGroupNodes)
                {
                    try
                    {
                        RevIMConfig = new RMUniqueIDCoulumn(siteNode);
                        RevIMConfig.jobId = currentJobId;
                        RevIMConfig.columnDisplayName = setting.Name;
                        RevIMConfig.IsEnableUniqueIDSetting = setting.IsActived;
                        logger.Info("Set UniqueID column SiteCollection [{0}]", siteNode.FullPath);
                        RevIMConfig.ConfigSiteCollectionSetting(siteNode);
                        var rootNode = RMSPTreeService.Browse(siteNode)[0];
                        RevIMConfig.ConfigSubNodeSettings(rootNode);
                        hasSuccessNode = true;
                        progress++;
                    }
                    catch (Exception exc)
                    {
                        hasErrorNode = true;
                        errorMessage = "RM_SYNC_InitException";
                        logger.Error("Add Global Settings Error Path {0} : {1}", groupNode.FullPath, exc.ToString());
                        if (RevIMConfig != null)
                        {
                            RevIMConfig.UniqueIDSettingJobDetails.Add(new JMUniqueIDSettingJobDetails() { ObjectName = siteNode.Name, SourceURL = siteNode.FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage });
                        }
                        else
                        {
                            //在初始化site collection之前，就出现了异常
                            List<JMUniqueIDSettingJobDetails> finalDetails = new List<JMUniqueIDSettingJobDetails>();
                            finalDetails.Add(new JMUniqueIDSettingJobDetails() { ObjectName = siteNode.Name, SourceURL = siteNode.FullPath, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage });
                            RunUpdateJobDetails(finalDetails);
                        }
                    }
                    finally
                    {
                        if (RevIMConfig != null)
                        {
                            if (!hasErrorNode)
                            {
                                hasErrorNode = !this.IsJobFinishWithoutException(RevIMConfig.UniqueIDSettingJobDetails);
                            }
                            RunUpdateJobDetails(RevIMConfig.UniqueIDSettingJobDetails);
                            RevIMConfig.Dispose();
                        }
                        RMJobService.UpdateJobProgress(currentJobId, CalculateProgress(progress, totalCount, true));
                    }
                }
            }
            UpdateSettingJobStatus();
        }
       
        #region SharePoint Setting Job method group

        /// <summary>
        /// browse选中group node下的所有site collection，并记录site collection的总数
        /// </summary>
        /// <param name="rootNodes"></param>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        private Dictionary<string, List<RMSPTreeNode>> GetTotalRMSPTreeNode(List<RMSPTreeNode> rootNodes, ref int nodeCount)
        {
            Dictionary<string, List<RMSPTreeNode>> returnMap = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (RMSPTreeNode rootNode in rootNodes)
            {
                List<RMSPTreeNode> childNodes = RMSPTreeService.Browse(rootNode);
                if (childNodes != null && childNodes.Count > 0)
                {
                    returnMap.Add(rootNode.SPObjectId, childNodes);
                    nodeCount = nodeCount + childNodes.Count;
                }
                else
                {
                    returnMap.Add(rootNode.SPObjectId, new List<RMSPTreeNode>());
                    nodeCount = nodeCount + 0;
                }
            }
            return returnMap;
        }
        private List<JMUniqueIDSettingJobDetails> CloneJobDetailsAddSCUrl(List<JMUniqueIDSettingJobDetails> details)
        {
            List<JMUniqueIDSettingJobDetails> cloneDetails = new List<JMUniqueIDSettingJobDetails>();
            foreach (JMUniqueIDSettingJobDetails detail in details)
            {
                cloneDetails.Add(detail);
            }
            return cloneDetails;
        }
        private void RunUpdateJobDetails(List<JMUniqueIDSettingJobDetails> details)
        {
            List<JMUniqueIDSettingJobDetails> needUpdateDetails = this.CloneJobDetailsAddSCUrl(details);
            if (needUpdateDetails.Count == 0)
            {
                return;
            }
            JobDetailService.UpdateJobDetails(needUpdateDetails, baseJobDto);
        }
        private void UpdateJobDetails(object details)
        {
            List<JMUniqueIDSettingJobDetails> syncJobDetails = (List<JMUniqueIDSettingJobDetails>)details;
            JobDetailService.UpdateJobDetails(syncJobDetails, baseJobDto);
        }
        private bool IsJobFailed(List<JMUniqueIDSettingJobDetails> details)
        {
            if (details.AsQueryable().Where(d => d.Status == JobDetailsStatus.None ||
             d.Status == JobDetailsStatus.Pending ||
             d.Status == JobDetailsStatus.Skipped ||
             d.Status == JobDetailsStatus.Successful).FirstOrDefault() == null)
            {
                return true;
            }
            return false;
        }
        private bool IsJobFinishWithoutException(List<JMUniqueIDSettingJobDetails> details)
        {
            if (details.AsQueryable().Where(d => d.Status == JobDetailsStatus.Failed).FirstOrDefault() != null)
            {
                return false;
            }
            return true;
        }
        private bool IsJobFailedByType(List<JMUniqueIDSettingJobDetails> details, FailedType ftype)
        {
            bool isFailed = false;
            List<string> classificationActions = new List<string>() {
                I18NEntity.GetString("RM_JS_JMD_Status_AddSiteCollectionClassification"),
                I18NEntity.GetString("RM_JS_JMD_Status_UpdateSiteCollectionClassification"),
                I18NEntity.GetString("RM_JS_JMD_Status_AddWebClassification"),
                I18NEntity.GetString("RM_JS_JMD_Status_UpdateWebClassification"),
                I18NEntity.GetString("RM_JS_JMD_Status_UpdateListClassification"),
                I18NEntity.GetString("RM_JS_JMD_Status_AddListClassification")
            };
            List<string> listComment = new List<string>() { "RM_SS_ConfigureClassificationFailed" };
            List<string> listCommonError = new List<string>() { "RM_SYNC_InitException" };

            List<string> physicalActions = new List<string>() {
            I18NEntity.GetString("RM_SS_ConfigPhysicalAction"),
            };

            foreach (JMUniqueIDSettingJobDetails detail in details)
            {
                switch (ftype)
                {
                    case FailedType.ConfigColumn:
                        if (JobDetailsStatus.Failed == detail.Status && (!classificationActions.Contains(detail.Action) && !physicalActions.Contains(detail.Action) && !listComment.Contains(detail.Comment)))
                        {
                            isFailed = true;
                            break;
                        }
                        break;
                    case FailedType.ConfigClassification:
                        if (JobDetailsStatus.Failed == detail.Status && (classificationActions.Contains(detail.Action) || listComment.Contains(detail.Comment) || listCommonError.Contains(detail.Comment)))
                        {
                            isFailed = true;
                            break;
                        }
                        break;
                    case FailedType.ConfigPhysical:
                        if (JobDetailsStatus.Failed == detail.Status && physicalActions.Contains(detail.Action))
                        {
                            isFailed = true;
                            break;
                        }
                        break;
                    default:
                        break;
                }
            }
            return isFailed;
        }
        private int CalculateProgress(int numerator, int denominator, bool isGlobalSetting = false)
        {
            double progressCount = 0;
            if (numerator == denominator)
            {
                progressCount = 99;
            }
            else
            {
                if (isGlobalSetting)
                {
                    progressCount = (double)numerator / (double)denominator * 95 + 5;
                }
                else
                {
                    progressCount = (double)numerator / (double)denominator * 99 + 1;
                }

            }
            return (int)progressCount;
        }
        private void UpdateSettingJobStatus()
        {
            JobDetailService.UploadJobDetailsAndReport(baseJobDto);
            //更新Job进度
            RMJobService.UpdateJobProgress(this.currentJobId, 100);
            if (hasSuccessNode && hasErrorNode)
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
            }
            else if (!hasErrorNode)
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Finished, "");
            }
            else if (!hasSuccessNode)
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Failed, "RM_TS_SS_Summary");
            }
            //else if (hasErrorNode)
            //{
            //    RMJobService.UpdateJobStatus(currentJobId, JobStatus.FinishWithException, "RM_TS_SS_Summary");
            //}
            else
            {
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Skipped, "RM_UID_JobSkip");
            }
            //RMJobService.UpdateJobStatus(this.currentJobId, JobStatus.Finished);
        }
        #endregion
    }


    public class RMUniqueIDCoulumn : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMUniqueIDCoulumn));
        #region construct method
        public RMUniqueIDCoulumn(RMSPTreeNode sitenode)
        {
            RMSPTreeService = PlatformWindsorManager.GetService(typeof(ISPSettingTreeService)) as ISPSettingTreeService;
            SharePointSettingDao = PlatformWindsorManager.GetService(typeof(ISharePointSettingDao)) as ISharePointSettingDao;
            UniqueIdSettingService = PlatformWindsorManager.GetService(typeof(IUniqueIdSettingService)) as IUniqueIdSettingService;
            JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            TermSetDao = PlatformWindsorManager.GetService(typeof(ITermSetDao)) as ITermSetDao;
            TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
            this.mSPObjectId = sitenode.SPObjectId;
            InitContext(sitenode);
        }

        public RMUniqueIDCoulumn()
        {
            RMSPTreeService = PlatformWindsorManager.GetService(typeof(ISPSettingTreeService)) as ISPSettingTreeService;
            SharePointSettingDao = PlatformWindsorManager.GetService(typeof(ISharePointSettingDao)) as ISharePointSettingDao;
            UniqueIdSettingService = PlatformWindsorManager.GetService(typeof(IUniqueIdSettingService)) as IUniqueIdSettingService;
            JobService = PlatformWindsorManager.GetService(typeof(IJobMonitorService)) as IJobMonitorService;
            TermSetDao = PlatformWindsorManager.GetService(typeof(ITermSetDao)) as ITermSetDao;
            TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;

        }

        //private RMSPTreeNode siteCollectionTreeNode { get; set; }//current sitecollection node
        private string mSPObjectId;
        public bool IsEnableUniqueIDSetting = false;
        public string columnDisplayName;
        private string RevIMUniqueIDInternalName = "RevIMUniqueID";
        private uint limitedCount;
        public Guid GetSiteId()
        {
            try
            {
                return new Guid(mSPObjectId);
            }
            catch
            {
                return Guid.Empty;
            }
        }
        #endregion

        #region SharePoint Client Object
        private List<string> DesignLists = new List<string>();
        private Site mSite { get; set; }
        private Web mWeb { get; set; }// column web root web. 
        private ClientContext mClientContext { get; set; }
        #endregion

        #region const string value
        private Guid RevIMUniqueIDColumnID
        {
            get
            {
                return new Guid("40f84bba906045b4af568ee102a52dcb");
            }
        }
        private const string RM_JS_Common_Pending = "RM_JS_Common_Pending";
        private const string RM_SS_ConfigureColumnFailed = "RM_SS_ConfigureColumnFailed";
        #endregion

        #region Dao Interface
        private IContainerDao mContainerDao { get; set; }
        public IContainerDao ContainerDao
        {
            get
            {
                if (mContainerDao == null)
                {
                    mContainerDao = PlatformWindsorManager.GetService(typeof(IContainerDao)) as IContainerDao;
                    return mContainerDao;
                }
                else
                {
                    return mContainerDao;
                }
            }
        }
        public ISPSettingTreeService RMSPTreeService { get; set; }
        public ISharePointSettingDao SharePointSettingDao { get; set; }
        public IUniqueIdSettingService UniqueIdSettingService { get; set; }
        public IJobMonitorService JobService { get; set; }
        public ITermSetDao TermSetDao { get; set; }

        public ITermDao TermDao { get; set; }
        #endregion

        #region property for job 
        public JobType jobType { get; set; }
        public Dictionary<Guid, int> settingResults = new Dictionary<Guid, int>();
        public string jobId;
        public List<JMUniqueIDSettingJobDetails> UniqueIDSettingJobDetails = new List<JMUniqueIDSettingJobDetails>();
        #endregion

        #region Add UniqueID Column
        public void ConfigSiteCollectionSetting(RMSPTreeNode node)
        {
            //if (!CheckClassificationSetting(node))
            //{
            //    return;
            //}
            if (IsEnableUniqueIDSetting)
            {
                AddUniqueIDColumnToSiteCollection(node);
            }
        }


        public void ConfigSubNodeSettings(RMSPTreeNode node, bool isApplySettingJob = false)//node level is Site or List.
        {
            if ((node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library) && !node.Hidden)
            {
                JobService.UpdateJobWithoutProgressChange(jobId);
                try
                {
                     AddBCSColumnToList(node);
                }
                catch (Exception ex)
                {
                    logger.Info("Set list Setting error {0}", ex.ToString());
                }
            }
            else if (node.Level == (int)NodeLevel.Site)
            {
                try
                {
                    foreach (var subNode in RMSPTreeService.Browse(node))
                    {
                        ConfigSubNodeSettings(subNode);
                    }
                }
                catch (Exception ex1)
                {
                    logger.Info("Set web setting error {0}", ex1.ToString());
                }
            }
            //for vir node.
            else if (node.Level == (int)NodeLevel.Sites || node.Level == (int)NodeLevel.Lists)
            {
                foreach (var subNode in RMSPTreeService.Browse(node))
                {
                    ConfigSubNodeSettings(subNode);
                }
            }
        }

        private bool CheckUniqueIDSetting(RMSPTreeNode node)
        {
            if (!IsEnableUniqueIDSetting)
            {
                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_JS_JMD_Action_CheckColumnSetting"), JobDetailsStatus.Skipped, "Not Enable UniqueID");
                return false;
            }
            return true;
        }
        private string GetFullUrl(RMSPTreeNode node)
        {
            return node.FullPath;
        }

        private void AddUniqueIDColumnToSiteCollection(RMSPTreeNode node)
        {
            try
            {
                Guid siteCollColumnID = GetSiteCollectionRevIMUniqueColumnID();
                if (siteCollColumnID != Guid.Empty)
                {
                    try
                    {
                        mClientContext.Load(mWeb, w => w.Fields, w => w.Title);
                        Field uniqueIDColumn = mWeb.Fields.GetById(siteCollColumnID);
                        mClientContext.Load(uniqueIDColumn);
                        mClientContext.ExecuteQuery();
                        var oldDisplayName = uniqueIDColumn.Title;
                        if (this.columnDisplayName != oldDisplayName)
                        {
                            UpdateUniqueIDColumn(uniqueIDColumn);
                            this.AddDetailToList(mWeb.Title, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Update"), JobDetailsStatus.Successful, null);
                        }
                        else
                        {
                            this.AddDetailToList(mWeb.Title, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Skipped, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("update sitecollection uniqueid column ,error info {0}", ex.Message);
                        this.AddDetailToList(mWeb.Title, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Update"), JobDetailsStatus.Failed, null);
                        #endregion
                    }
                }
                else
                {
                    logger.Info("Need create new unique id column for site Path {0}", node.FullPath);
                    #region create new site column
                    InitContext(node);
                    mClientContext.Load(mWeb);
                    mClientContext.ExecuteQuery();
                    Field uniqueIDField = CreateNewSiteCollectionUniqueIDColumn(node.FullPath);
                    this.AddDetailToList(mWeb.Title, GetFullUrl(node),I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Successful, null);
                    #endregion
                }
            }
            catch (Exception e)
            {
                logger.Error(" Create new site uniqueid column error Path {0}:{1}", node.FullPath, e.ToString());
                this.AddDetailToList(mWeb.Title, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Failed, e.Message);
            }
        }

        private void AddBCSColumnToList(RMSPTreeNode node)
        {
            try
            {
                Web web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                mClientContext.Load(web);
                List list = web.Lists.GetById(new Guid(node.SPObjectId));
                mClientContext.Load(mWeb);
                this.mClientContext.ExecuteQuery();

                mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                mClientContext.ExecuteQuery();

                if (list.BaseTemplate == 600)
                {
                    logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                    return;
                }
                if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
                {
                    logger.Info("Skip the design list {0}", list.RootFolder.Name);
                    return;
                }
               
                Guid fieldId = GetSiteCollectionRevIMUniqueColumnID();
                if (fieldId == Guid.Empty)
                {
                    logger.Info("Site collection not config unique column {0}", web.Url);
                    var siteCollectionNode = GetSiteCollectionNode(node);
                    var groupNode = GetWebAppNode(node);
                    AddUniqueIDColumnToSiteCollection(siteCollectionNode);
                    fieldId = GetSiteCollectionRevIMUniqueColumnID();
                    //reload context 
                    InitContext(node);
                    web = this.mSite.OpenWebById(new Guid(node.Parent.Parent.SPObjectId));
                    mClientContext.Load(web);
                    list = web.Lists.GetById(new Guid(node.SPObjectId));
                    mClientContext.Load(mWeb);
                    mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
                    mClientContext.ExecuteQuery();
                }
                Field webField = this.mWeb.Fields.GetById(fieldId);
                mClientContext.Load(webField);
                mClientContext.ExecuteQuery();
                Guid listColumnId = CheckListUniqueColumnID(list);
                if (listColumnId != Guid.Empty)
                {
                    mClientContext.Load(list, l => l.Fields);
                    Field listField = list.Fields.GetById(RevIMUniqueIDColumnID);
                    mClientContext.Load(listField, l => l.Title);
                    mClientContext.ExecuteQuery();
                    var oldDisplayName = listField.Title;
                    if (this.columnDisplayName != oldDisplayName)
                    {
                        UpdateUniqueIDColumn(listField);
                        this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Update"), JobDetailsStatus.Successful, null);
                    }
                    else
                    {
                        this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Skipped, null);
                    }
                }
                else
                {
                    AddBCSColumnToList(list, webField, node);
                }
                SetDefaultValue(node, web, list);
            }
            catch (ServerUnauthorizedAccessException se)
            {
                logger.Warn("Add site column on list error Path :{0}, {1}", node.FullPath, se.ToString());
                this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Failed, "RM_SS_DocLibraryAccessDeny");

            }
            catch (ServerException ex)
            {
                if (ex.Message.Contains("List does not exist"))
                {
                    logger.Warn("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Failed, "RM_SS_ListIsNotExist");//to do next
                }
                else
                {
                    logger.Warn("Add site column on list error Path :{0}, error detail {1}", node.FullPath, ex.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message));
                }
            }
            catch (Exception e)
            {
                if (!e.Message.Equals("Have same name column in the list", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn("Add site column on list error Path :{0}, error detail {1}", node.FullPath, e.ToString());
                    this.AddDetailToList(node.Name, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Failed, string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message));
                }
            }
        }
        private Field CreateNewSiteCollectionUniqueIDColumn(string siteUrl)
        {
            Field uniqueIdField = null;
            bool isFieldExist = false;
            try
            {
                Field dispalyNameField = this.mWeb.Fields.GetByTitle(this.columnDisplayName);
                mClientContext.Load(dispalyNameField);
                mClientContext.ExecuteQuery();
                string getTitle = dispalyNameField.Title;
                isFieldExist = true;
            }
            catch (Exception e)
            {
                logger.Info("Not get same column from site {0} : {1}", siteUrl, e.ToString());
            }
            if (isFieldExist)
            {
                logger.Warn("Have same name column in the SiteCollection");
                throw new Exception("Have same name column in the SiteCollection");
            }
            try
            {
                uniqueIdField = this.mWeb.Fields.AddFieldAsXml("<Field Type='Text' Name='" + this.RevIMUniqueIDInternalName + "' ID='" + RevIMUniqueIDColumnID + "' DisplayName='" + this.columnDisplayName + "' ReadOnly = 'TRUE'  StaticName='"+this.RevIMUniqueIDInternalName + "' />", false, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddToAllContentTypes);
                this.mClientContext.Load(uniqueIdField);
                this.mClientContext.ExecuteQuery();
            }
            catch
            {
                uniqueIdField = this.mWeb.Fields.GetById(RevIMUniqueIDColumnID);
                this.mClientContext.Load(uniqueIdField);
                this.mClientContext.ExecuteQuery();
            }
            logger.Info("Create SiteCollection UniqueID Column Success, The internal name of this column is {0}", this.RevIMUniqueIDInternalName);
            return uniqueIdField;
        }
        private void UpdateUniqueIDColumn(Field uniqueIdField)
        {
            uniqueIdField.Title = this.columnDisplayName;
            uniqueIdField.Update();
            this.mWeb.Update();
            mClientContext.ExecuteQuery();
            logger.Info("update  UniqueID Column Success");
        }
        private void AddBCSColumnToList(List list, Field field, RMSPTreeNode node)
        {
            mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title);
            mClientContext.ExecuteQuery();
            bool isFieldExist = false;
            try
            {
                Field dispalyNameField = list.Fields.GetByTitle(this.columnDisplayName);
                mClientContext.Load(dispalyNameField);
                mClientContext.ExecuteQuery();
                string getTitle = dispalyNameField.Title;
                isFieldExist = true;
            }
            catch
            {

            }
            if (isFieldExist)
            {
                logger.Warn("Have same name column in the list");
                throw new Exception("Have same name column in the list");
            }
            if (list.BaseTemplate == 600)
            {
                logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                return;
            }
            if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
            {
                logger.Info("Skip the design list {0}", list.RootFolder.Name);
                return;
            }
            Field newListField = list.Fields.AddFieldAsXml(field.SchemaXml, false, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddToAllContentTypes);
            this.mClientContext.Load(newListField);
            this.mClientContext.ExecuteQuery();
            this.AddDetailToList(list.Title, GetFullUrl(node), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Successful, null);
        }
        public Guid GetSiteCollectionRevIMUniqueColumnID()
        {
            try
            {
                mClientContext.Load(mWeb, w => w.Fields);
                Field uniqueIdField = mWeb.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(uniqueIdField);
                mClientContext.ExecuteQuery();
                logger.Info("check site unique column id");
                return RevIMUniqueIDColumnID;
            }
            catch (Exception ex)
            {
                logger.Info("Site not config unique id column {0}", ex.ToString());
            }
            return Guid.Empty;
        }
        public Guid CheckListUniqueColumnID(List list)
        {
            try
            {
                logger.Info("Check list unique column id");
                mClientContext.Load(list, l => l.Fields);
                Field uniqueIdField = list.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(uniqueIdField);
                mClientContext.ExecuteQuery();
                return RevIMUniqueIDColumnID;
            }
            catch (Exception ex)
            {
                logger.Info("List not config unique id column {0}", ex.ToString());
            }
            return Guid.Empty;

        }
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetWebAppNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }
        private void SetDefaultValue(RMSPTreeNode node, Web web, List list)
        {
            try
            {
                var items = QueryItems(list);
                SetValue(node, list, items);
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while set item uniqueColumn value,siteUrl:{0}, listTitle:{1},ERROR:{2}", list != null ? web.Url : string.Empty, list != null ? list.Title : string.Empty, ex.ToString());
                throw ex;
            }
        }
        private ListItemCollection QueryItems(List list)
        {
            ListItemCollection items = null;
            try
            {
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                query.ViewXml = @"
                    <View Scope='RecursiveAll'>
                        <Query>
                            <Where>
                                <IsNull>
                                    <FieldRef Name='" + RevIMUniqueIDInternalName + @"'/>
                                </IsNull>
                            </Where>
                        </Query>
                        <RowLimit>" + limitedCount + @"</RowLimit>
                    </View>";
                items = list.GetItems(query);

            }
            catch (Exception ex)
            {
                logger.Error("an error occurred while Query And Set UniqueID,ERROR:{0}", ex.ToString());
            }
            mClientContext.Load(items);
            mClientContext.ExecuteQuery();
            return items;
        }
        private void SetValue(RMSPTreeNode node, List list, ListItemCollection items)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            if (items != null)
            {
                var itemCount = 0;
                int tempcounter = 0;

                mClientContext.Load(list, l => l.Fields, l => l.BaseType);
                mClientContext.ExecuteQuery();

                var uniqueIDField = list.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(uniqueIDField);
                mClientContext.ExecuteQuery();
                //mClientContext.Load(list);
                //mClientContext.ExecuteQuery();

                foreach (var item in items)
                {
                    try
                    {
                        mClientContext.Load(item, l => l.ContentType, l => l.Properties);
                        mClientContext.ExecuteQuery();
                        var itemContentType = item.ContentType.Name;
                        var filterContentTypes = new List<string>() { "Physical File", "Physical Box" };
                        var itemType = item["FSObjType"].ToString();
                        string itemName = item["FileLeafRef"].ToString();
                        string objectName = list.BaseType == BaseType.DocumentLibrary ? itemName : item["Title"].ToString();

                        if (itemType == "1" && !filterContentTypes.Contains(itemContentType))
                        {
                            logger.Info("skip set value : Item name:{2} ContentType:{0},Type:{1}", itemContentType, itemType, itemName);
                            continue;
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(item.Properties["ecm_ItemLockHolders"].ToString()))
                            {
                                //item is declared
                                this.AddDetailToList(objectName, string.Format("{0}/{1}", GetFullUrl(node), itemName), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Skipped, I18NEntity.GetString("RM_UI_Detail_IsDeclared"));
                                continue;
                            }
                        }
                        catch 
                        {
                            
                        }
                        string currentId = UniqueIdSettingService.LoadingCurrentId();
                        logger.Info("current id :[{0}]", currentId);
                        item[uniqueIDField.InternalName] = currentId;
                        item.SystemUpdate();
                        mClientContext.ExecuteQuery();
                      
                        
                        this.AddDetailToList(objectName, string.Format("{0}/{1}", GetFullUrl(node), itemName), I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Successful, null, currentId);
                    }
                    catch (ServerUnauthorizedAccessException ae)
                    {
                        logger.Error(" error:{0}", ae.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error("an error occurred while set uniqueid value,listUrl:{0},itemId:{1},ERROR:{2}", GetFullUrl(node), item.Id, ex.ToString());
                    }
                    tempcounter++;
                    if (tempcounter >= 100)
                    {
                        JobService.UpdateJobWithoutProgressChange(jobId);
                        tempcounter = 0;
                    }
                }

                if (itemCount > 0 && this.UniqueIDSettingJobDetails.Count > 0)
                {
                    if (this.UniqueIDSettingJobDetails.Last().Status != JobDetailsStatus.Failed)
                    {
                        this.AddDetailToList(this.UniqueIDSettingJobDetails.Last().ObjectName,
                            this.UniqueIDSettingJobDetails.Last().SourceURL, I18NEntity.GetString("RM_UI_Detail_Add"), JobDetailsStatus.Successful, "");
                    }
                }

            }
            stopWatch.Stop();
            TimeSpan timer = stopWatch.Elapsed;
            logger.Info("Stop Watch SetUniqueID 5000 Items ,time : {0} s ", timer.TotalSeconds);
        }
     
        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch
            { }
            return isDesignList;
        }
        private List<string> GetDesignLists()
        {
            List<string> results = new List<string>();
            try
            {
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Config\\DesignLists\\DesignLists.config";
                XmlDocument doc = new XmlDocument();
                doc.Load(configFilePath);
                foreach (var node in doc.GetElementsByTagName("List"))
                {
                    XmlElement xe = (XmlElement)node;
                    results.Add(xe.GetAttribute("url") + xe.GetAttribute("serverTemplate"));
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get Design Lists config file error {0}", ex.ToString());
            }
            return results;
        }
        public void InitContext(RMSPTreeNode node)
        {
            CommonClientContext commonClientContext = new CommonClientContext();
            mClientContext = commonClientContext.InitClientContext(node);
            this.mSite = mClientContext.Site;
            this.mWeb = mClientContext.Site.RootWeb;
            this.DesignLists = GetDesignLists();
            mClientContext.Load(mSite);
            mClientContext.Load(mWeb);
        }
  
        private void AddDetailToList(string objectName, string sourceURL, string Action, JobDetailsStatus status, string message,string currentId = "")
        {
            JMUniqueIDSettingJobDetails detail = new JMUniqueIDSettingJobDetails();
            if (objectName == sourceURL)
            {
                detail.ObjectName = mWeb.Title;
            }
            else
            {
                detail.ObjectName = objectName;
            }
            detail.SourceURL = sourceURL;
            detail.ColumnName = this.columnDisplayName;
            detail.Action = Action;
            detail.Status = status;
            detail.Comment = message;
            detail.UniqueID = currentId;
            this.UniqueIDSettingJobDetails.Add(detail);
        }
        public void Dispose()
        {
            try
            {
                if (mClientContext != null)
                {
                    mClientContext.Dispose();
                }
                mSite = null;
                mWeb = null;
            }
            catch (Exception ex)
            {
                logger.Warn("dispose the site context error:{0}", ex.ToString());
            }
            try
            {
                this.UniqueIDSettingJobDetails.Clear();
            }
            catch (Exception ex1)
            {
                logger.Warn("dispose unique ID settings details error:{0}", ex1.ToString());
            }
        }
    }
}
