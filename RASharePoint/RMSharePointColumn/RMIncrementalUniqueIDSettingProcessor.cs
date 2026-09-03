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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
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
    public class RMIncrementalUniqueIDSettingProcessor
    {
        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(RMIncrementalUniqueIDSettingProcessor));
        private JobType currentJobType;
        private string currentJobId;
        private BaseJobDto baseJobDto;

        private IJobDetailService mJobDetailService;
        private IJobMonitorService mJobService;
        private ISPSettingTreeService mSPTreeService;
        private IGeneralSettingService mGeneralSettingService;
        private IUniqueIdSettingService mUniqueIdSettingService;

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

        protected IGeneralSettingService GeneralSettingService
        {
            get
            {
                if (mGeneralSettingService == null)
                {
                    mGeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
                }
                return mGeneralSettingService;
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

        private ISharePointSettingDao mSharePointSettingDao;
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

        private DAOAPIClient mClient;
        protected DAOAPIClient Client
        {
            get
            {
                if (mClient == null)
                {
                    mClient = new DAOAPIClient();
                }
                return mClient;
            }
            
        }

        private List<JMUniqueIDSettingJobDetails> finalDetails;
        private List<string> mDesignLists = new List<string>();
        private List<Guid> mIsDesignListIDs = new List<Guid>();
        private List<Guid> mIsNeedUesUniqueIDListIDs = new List<Guid>();
        private List<Guid> mEnsureFieldListIDs = new List<Guid>();

        private List<Guid> mRemoveWebsID = new List<Guid>();
        private List<Guid> mRemoveListsID = new List<Guid>();
        private Dictionary<Guid, Web> websCache = new Dictionary<Guid, Web>();
        private Dictionary<Guid, Dictionary<Guid, List>> listsCache = new Dictionary<Guid, Dictionary<Guid, List>>();
        private bool hasErrorNode = false;
        private bool hasSuccessNode = false;
        private string errorMessage = string.Empty;
        private int tempcounter = 0;
        ClientContext mClientContext;
        public RMIncrementalUniqueIDSettingProcessor(string jobId, JobType jobType)
        {
            currentJobId = jobId;
            currentJobType = jobType;
            InitCurrentJobInfo();
        }

        private string columnDisplayName;
        private string RevIMUniqueIDInternalName = "RevIMUniqueID";
        //private string uniqueIdPrefix = "RevIMFlag";
        private RMUniqueIDSettingProcessor fullProcess = null;
        private DateTime mUTCJobStartTime;
        private Guid RevIMUniqueIDColumnID
        {
            get
            {
                return new Guid("40f84bba906045b4af568ee102a52dcb");
            }
        }
        private void InitCurrentJobInfo()
        {
            baseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)currentJobType };
            RMJobService.UpdateJobProgress(currentJobId, 1);//
        }
        public void ApplyUniqueIDSetting()
        {
            var isEmptyDetails = true;
            mUTCJobStartTime = DateTime.UtcNow;
            try
            {
                var uqSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                if (uqSetting == null || !uqSetting.IsActived)
                {
                    RMJobService.UpdateJobStatus(currentJobId, JobStatus.Failed);
                    return;
                }
                 
                this.columnDisplayName = uqSetting.Name;
                finalDetails = new List<JMUniqueIDSettingJobDetails>();
                Dictionary<string, RMSharePointSetting> GlobalSettingMaping = new Dictionary<string, RMSharePointSetting>();
                var processeFinishedSite = 0;
                
                mDesignLists = GetDesignLists();
                int totalCount = 0;
                var sites = Client.GetAuthorisedRemoteSiteCollectionsByUser();
                totalCount = sites.Count;
                //RMSPTreeNode farmNode = RMSPTreeService.LoadFarm()[0];//browse出当前选择的group node下所有的site collection
                //Dictionary<string, List<RMSPTreeNode>> processNodesMap = GetTotalRMSPTreeNode(RMSPTreeService.Browse(farmNode), ref totalCount);
                if (totalCount == 0)
                {
                    RMJobService.UpdateJobStatus(currentJobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                    return;
                }
                //RAPortalUtil.Init();
                var sitePercent = 100 / totalCount;
                foreach (var site in sites)
                {
                    var gorupId = site.parentId;
                    RMSharePointSetting groupSetting = null;
                    if (GlobalSettingMaping.ContainsKey(gorupId))
                    {
                        groupSetting = GlobalSettingMaping[gorupId];
                    }
                    else
                    {
                        groupSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(gorupId), Guid.Empty);
                        GlobalSettingMaping[gorupId] = groupSetting;

                    }
                    if (groupSetting == null)
                    {
                        logger.Info("This group has not been set global setting {0}", site.parentId);
                        continue;
                    }
                    else
                    {
                        logger.Info("Begin init IsEnableUniqueIDSetting {0}:{1}", site.url, this.columnDisplayName);//to do next
                    }
                    var fullProcessItems = 0;

                    try
                    {
                        
                        mIsDesignListIDs.Clear();
                        mIsNeedUesUniqueIDListIDs.Clear();
                        mRemoveWebsID.Clear();
                        mRemoveListsID.Clear();
                        mEnsureFieldListIDs.Clear();
                        websCache = new Dictionary<Guid, Web>();
                        listsCache = new Dictionary<Guid, Dictionary<Guid, List>>();
                        //TODO will cache web & list object;
                        
                        DateTime startTime = GetStartDate();
                        var siteCreateTime = new DateTime(site.CreateTime, DateTimeKind.Utc);
                        if (siteCreateTime >= startTime && siteCreateTime <= mUTCJobStartTime)
                        {
                            //new registe site run full
                            if (fullProcess == null)
                            {
                                fullProcess = new RMUniqueIDSettingProcessor(currentJobId, currentJobType);
                            }
                            fullProcessItems = ProcessFull(site);
                        }
                        else
                        {
                            ProcessIncremental(site, startTime, sitePercent, processeFinishedSite);
                        }

                    }
                    catch (Exception exc)
                    {
                        hasErrorNode = true;
                        errorMessage = "RM_SYNC_InitException";
                        finalDetails.Add(new JMUniqueIDSettingJobDetails() { ObjectName = site.Name, SourceURL = site.url, ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage });
                        logger.Error("Apply Unique ID Settings Error Path {0} : {1}", site.url, exc.ToString());

                    }
                    finally
                    {
                        if (finalDetails.Count > 0 || fullProcessItems > 0)
                        {
                            isEmptyDetails = false;
                        }
                        RunUpdateJobDetails(finalDetails);
                        processeFinishedSite++;
                        //RMJobService.UpdateJobProgress(currentJobId, CalculateProgress(progress, totalCount, true));
                    }

                }

                #region old logic
                //var groupNodes = RMSPTreeService.Browse(farmNode);

                //foreach (var groupNode in groupNodes)
                //{
                //    List<RMSPTreeNode> currentGroupNodes = processNodesMap[groupNode.SPObjectId];
                //    #region init group node 
                //    if (currentGroupNodes == null || currentGroupNodes.Count == 0)
                //    {
                //        continue;
                //    }
                //    var groupSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
                //    if (groupSetting == null)
                //    {
                //        logger.Info("This group has not been set global setting {0}", groupNode.Name);
                //        continue;
                //    }
                //    else
                //    {
                //        logger.Info("Begin init IsEnableUniqueIDSetting {0}:{1}", groupNode.FullPath, this.columnDisplayName);//to do next
                //    }
                //    #endregion
                //    foreach (var siteNode in currentGroupNodes)
                //    {

                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                hasErrorNode = true;
                errorMessage = "RM_SYNC_InitException";
                finalDetails.Add(new JMUniqueIDSettingJobDetails() { ObjectName = @"N/A", SourceURL = @"N/A", ColumnName = "RM_JS_Common_Pending", Action = @"N/A", Status = JobDetailsStatus.Failed, Comment = errorMessage });
                RunUpdateJobDetails(finalDetails);
                logger.Error("Apply Unique ID Settings Error {1}", ex.ToString());
            }
            finally {
                JobDetailService.UploadJobDetailsAndReport(baseJobDto);
                UpdateSettingJobStatus(isEmptyDetails);
            }
        }

        private int ProcessFull(GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection site)
        {
            RMSPTreeNode siteNode = null;
            RMUniqueIDCoulumn RevIMConfig = null;
            int processCnt = 0;
            try
            {
                logger.Info("begin to process full,siteUrl:{0}", site.url);
                siteNode = RMDtoConverter.ConvertRemoteSite2RMTree(site);
                RevIMConfig = new RMUniqueIDCoulumn(siteNode);
                RevIMConfig.jobId = currentJobId;
                RevIMConfig.columnDisplayName = this.columnDisplayName;
                RevIMConfig.IsEnableUniqueIDSetting = true;
                logger.Info("Set UniqueID column SiteCollection [{0}]", site.url);
                RevIMConfig.ConfigSiteCollectionSetting(siteNode);
                var rootNode = RMSPTreeService.Browse(siteNode)[0];
                RevIMConfig.ConfigSubNodeSettings(rootNode);
                hasSuccessNode = true;
            }
            catch (Exception ex)
            {
                hasErrorNode = true;
                errorMessage = "RM_SYNC_InitException";
                logger.Error("Add Global Settings Error Path {0} : {1}", siteNode.FullPath, ex.ToString());
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
                    processCnt = RevIMConfig.UniqueIDSettingJobDetails.Count;
                    if (!hasErrorNode)
                    {
                        hasErrorNode = !this.IsJobFinishWithoutException(RevIMConfig.UniqueIDSettingJobDetails);
                    }
                    RunUpdateJobDetails(RevIMConfig.UniqueIDSettingJobDetails);
                    RevIMConfig.Dispose();
                }
                
            }
            return processCnt;


        }

        private bool IsJobFinishWithoutException(List<JMUniqueIDSettingJobDetails> details)
        {
            if (details.AsQueryable().Where(d => d.Status == JobDetailsStatus.Failed).FirstOrDefault() != null)
            {
                return false;
            }
            return true;
        }

        private void ProcessIncremental(GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection site, DateTime startTime, int sitePercent, int processeFinishedSite)
        {
            logger.Info("begin to process Incremental,siteUrl:{0}", site.url);
            if (mClientContext == null || mClientContext.Site.Url != site.url)
            {
                CommonClientContext commonClientContext = new CommonClientContext();
                mClientContext = commonClientContext.InitClientContext(site);
                mClientContext.Load(mClientContext.Web);
                mClientContext.ExecuteQuery();
                //var site = mClientContext.Site;
                //mClientContext.Load(site);
            }
            ChangeQuery query = BuildChangeQuery(mClientContext.Site.Id.ToString(), startTime);
            int processCount = 0;
            while (true)
            {
                ChangeCollection changedCollection = mClientContext.Site.GetChanges(query);
                mClientContext.Load(changedCollection);
                mClientContext.ExecuteQuery();
                foreach (Change changeObject in changedCollection)
                {
                    if (changeObject.ChangeType != ChangeType.Add)
                    {
                        continue;
                    }
                    tempcounter++;
                    if (tempcounter >= 50)
                    {
                        RMJobService.UpdateJobWithoutProgressChange(currentJobId);
                        tempcounter = 0;
                    }
                    Dictionary<string, object> objectProperties = new Dictionary<string, object>();
                    Dictionary<string, object> tempProperties = new Dictionary<string, object>();
                    CopyProperty(objectProperties, changeObject);
                    switch (changeObject.GetType().ToString())
                    {
                        case "Microsoft.SharePoint.Client.ChangeWeb":

                            ProcessWeb(objectProperties);

                            break;
                        case "Microsoft.SharePoint.Client.ChangeList":

                            ProcessList(objectProperties);

                            break;
                        case "Microsoft.SharePoint.Client.ChangeItem":

                            ProcessItem(objectProperties);

                            break;
                        case "Microsoft.SharePoint.Client.ChangeFile":
                            break;
                    }
                    processCount++;
                    int progress = sitePercent * processeFinishedSite + sitePercent * processCount / changedCollection.Count;
                    //logger.Info("progress:" + progress);
                    RMJobService.UpdateJobProgress(currentJobId, progress);
                }
                if (changedCollection.Count < query.FetchLimit)
                {
                    break;
                }
                query.ChangeTokenStart = changedCollection[(int)query.FetchLimit - 1].ChangeToken;
            }
        }

        private void ProcessWeb(Dictionary<string, object> objectProperties)
        {
            Guid webId = new Guid(objectProperties["WebId"].ToString());
            if (mRemoveWebsID.Contains(webId))
            {
                return;
            }
            try
            {
                Web web = null;
                if (websCache.ContainsKey(webId))
                {
                    web = websCache[webId];
                    logger.Info("use web Cache:{0}", web.Url);
                }
                else
                {
                    web = mClientContext.Site.OpenWebById(webId);
                    try
                    {
                        mClientContext.Load(web);
                        mClientContext.ExecuteQuery();
                    }
                    catch (Exception)
                    {
                        logger.Info("web has deleted,webId:{0}", webId);
                        mRemoveWebsID.Add(webId);
                        return;
                    }
                    websCache.Add(webId, web);
                    logger.Info("add web Cache:{0}", web.Url);
                }
                EnsureWebField(web);
            }
            catch (Exception e)
            {
                logger.Warn("error occurred while process web,webId:{0},error:{1}", webId, e.ToString());
            }
        }
        private void ProcessList(Dictionary<string, object> objectProperties)
        {
            Guid listWebId = new Guid(objectProperties["WebId"].ToString());
            if (mRemoveWebsID.Contains(listWebId))
            {
                return;
            }
            Guid listId = new Guid(objectProperties["ListId"].ToString());
            if (mRemoveListsID.Contains(listId))
            {
                return;
            }
            try
            {
                if (CheckListIsSkipByListId(listId))
                {
                    return;
                }
                Web listWeb = null;
                if (websCache.ContainsKey(listWebId))
                {
                    listWeb = websCache[listWebId];
                    logger.Info("use web Cache:{0}", listWeb.Url);
                }
                else
                {
                    listWeb = mClientContext.Site.OpenWebById(listWebId);
                    try
                    {
                        mClientContext.Load(listWeb);
                        mClientContext.ExecuteQuery();
                    }
                    catch (Exception)
                    {
                        logger.Info("web has deleted,webId:{0}", listWebId);
                        mRemoveWebsID.Add(listWebId);
                        return;
                    }
                    websCache.Add(listWebId, listWeb);
                    logger.Info("add web Cache:{0}", listWeb.Url);
                }
                List list = null;
                if (listsCache.ContainsKey(listWebId) && listsCache[listWebId].ContainsKey(listId))
                {
                    list = listsCache[listWebId][listId];
                    if (CheckListIsSkipByBaseTemplate(list))
                    {
                        return;
                    }
                    logger.Info("use list Cache:{0}", list.RootFolder.ServerRelativeUrl);
                }
                else
                {
                    list = listWeb.Lists.GetById(listId);
                    try
                    {
                        mClientContext.Load(list);
                        mClientContext.ExecuteQuery();
                    }
                    catch (Exception)
                    {
                        logger.Info("list's web has deleted,webId:{0},listId:{1}", listWebId, listId);
                        mRemoveListsID.Add(listId);
                        return;
                    }
                    if (listsCache.ContainsKey(listWebId))
                    {
                        listsCache[listWebId].Add(listId, list);
                    }
                    else
                    {
                        var listCache = new Dictionary<Guid, List>();
                        listCache.Add(listId, list);
                        listsCache.Add(listWebId, listCache);
                    }
                    if (CheckListIsSkipByBaseTemplate(list))
                    {
                        return;
                    }
                    logger.Info("add list Cache:{0}", list.RootFolder.ServerRelativeUrl);


                    //var listCache = new Dictionary<Guid, List>();
                    //listCache.Add(listId, list);
                    //listsCache.Add(listWebId, listCache);
                }
                
                EnsureListField(list, listWeb);
            }
            catch (Exception e)
            {
                logger.Warn("error occurred while process list,webId:{0},listId:{1},error:{2}", listWebId, listId, e.ToString());
            }
        }
        private void ProcessItem(Dictionary<string, object> objectProperties)
        {
            /* check web is remove*/
            Guid itemWebId = new Guid(objectProperties["WebId"].ToString());
            if (mRemoveWebsID.Contains(itemWebId))
            {
                return;
            }
            /* check list is remove*/
            Guid itemListId = new Guid(objectProperties["ListId"].ToString());
            if (mRemoveListsID.Contains(itemListId))
            {
                return;
            }

            int itemId = int.Parse(objectProperties["ItemId"].ToString());
            using (PerformanceScope scProcessItem = new PerformanceScope(string.Format("ProcessItem webId:{0},listId:{1},itemId:{2}", itemWebId, itemListId, itemId)))
            {
                try
                {
                    if (CheckListIsSkipByListId(itemListId))
                    {
                        return;
                    }

                    /* get web form cache or sp*/
                    Web itemWeb = null;
                    if (websCache.ContainsKey(itemWebId))
                    {
                        itemWeb = websCache[itemWebId];
                        logger.Info("use web Cache:{0}", itemWeb.Url);
                    }
                    else
                    {
                        itemWeb = mClientContext.Site.OpenWebById(itemWebId);
                        try
                        {
                            mClientContext.Load(itemWeb);
                            mClientContext.ExecuteQuery();
                        }
                        catch (Exception)
                        {
                            logger.Info("item's web has deleted,webId:{0},listId:{1},item:{2}", itemWebId, itemListId, itemId);
                            mRemoveWebsID.Add(itemWebId);
                            return;
                        }
                        websCache.Add(itemWebId, itemWeb);
                        logger.Info("add web Cache:{0}", itemWeb.Url);
                    }

                    /* get list form cache or sp*/
                    List itemList = null;
                    if (listsCache.ContainsKey(itemWebId) && listsCache[itemWebId].ContainsKey(itemListId))
                    {
                        itemList = listsCache[itemWebId][itemListId];
                        if (CheckListIsSkipByBaseTemplate(itemList))
                        {
                            return;
                        }
                        logger.Info("use list Cache:{0}", itemList.RootFolder.ServerRelativeUrl);
                    }
                    else
                    {
                        itemList = itemWeb.Lists.GetById(itemListId);
                        try
                        {
                            mClientContext.Load(itemList);
                            mClientContext.ExecuteQuery();
                        }
                        catch (Exception)
                        {
                            logger.Info("item's list has deleted,webId:{0},listId:{1},item:{2}", itemWebId, itemListId, itemId);
                            mRemoveListsID.Add(itemListId);
                            return;
                        }
                        if (listsCache.ContainsKey(itemWebId))
                        {
                            listsCache[itemWebId].Add(itemListId, itemList);
                        }
                        else
                        {
                            var listCache = new Dictionary<Guid, List>();
                            listCache.Add(itemListId, itemList);
                            listsCache.Add(itemWebId, listCache);
                        }
                        if (CheckListIsSkipByBaseTemplate(itemList))
                        {
                            return;
                        }
                        logger.Info("add list Cache:{0}", itemList.RootFolder.ServerRelativeUrl);
                    }

                    using (PerformanceScope sc = new PerformanceScope("Ensure List Field"))
                    {
                        EnsureListField(itemList, itemWeb);
                    }
                    var listItem = itemList.GetItemById(itemId);
                    try
                    {
                        using (PerformanceScope sc = new PerformanceScope("load item"))
                        {
                            mClientContext.Load(listItem, item => item[RevIMUniqueIDInternalName], item => item["FileLeafRef"], item => item["FSObjType"], item => item["FileRef"], item => item["Title"], item => item.ContentType, item => item.Properties);
                            mClientContext.ExecuteQuery();
                            var itemContentType = listItem.ContentType.Name;
                            var filterContentTypes = new List<string>() { "Physical File", "Physical Box" };
                            var itemType = listItem["FSObjType"].ToString();

                            if (itemType == "1" && !filterContentTypes.Contains(itemContentType))
                            {
                                logger.Info("skip set value : Item name:{2} ContentType:{0},Type:{1}", itemContentType, itemType, listItem["FileLeafRef"].ToString());
                                return;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        logger.Info("item has deleted,webId:{0},listId:{1},itemId:{2}", itemWebId, itemListId, itemId);
                        return;
                    }
                    //try
                    //{
                    //    if (listItem.Folder.Exists)
                    //    {
                    //        logger.Info("skip folder {0}", listItem["Title"]);
                    //        return;
                    //    }
                    //}
                    //catch (Exception)
                    //{

                    //}
                    if (listItem[RevIMUniqueIDInternalName] == null || string.IsNullOrEmpty(listItem[RevIMUniqueIDInternalName].ToString()))
                    {
                        string uniqueId = string.Empty;
                        string objName = string.Empty;
                        string sourceUrl = string.Empty;
                        try
                        {
                            if (itemList.BaseType == BaseType.DocumentLibrary)
                            {
                                objName = listItem["FileLeafRef"].ToString();
                            }
                            else
                            {
                                objName = listItem["Title"].ToString();
                            }
                            sourceUrl = MakeFullUrl(mClientContext.Site.Url, listItem["FileRef"].ToString());
                            try
                            {
                                if (!string.IsNullOrEmpty(listItem.Properties["ecm_ItemLockHolders"].ToString()))
                                {
                                    //item is declared
                                    finalDetails.Add(new JMUniqueIDSettingJobDetails()
                                    {
                                        ObjectName = objName,
                                        SourceURL = sourceUrl,
                                        ColumnName = this.columnDisplayName,
                                        Action = I18NEntity.GetString("RM_UI_Detail_Add"),
                                        Status = JobDetailsStatus.Skipped,
                                        Comment = I18NEntity.GetString("RM_UI_Detail_IsDeclared"),
                                        UniqueID = ""
                                    });
                                    return;
                                }
                            }
                            catch
                            {

                            }
                            uniqueId = UniqueIdSettingService.LoadingCurrentId();
                            using (PerformanceScope sc = new PerformanceScope("update item"))
                            {
                                listItem[RevIMUniqueIDInternalName] = uniqueId;
                                listItem.SystemUpdate();
                                mClientContext.ExecuteQuery();
                            }
                            finalDetails.Add(new JMUniqueIDSettingJobDetails()
                            {
                                ObjectName = objName,
                                SourceURL = sourceUrl,
                                ColumnName = this.columnDisplayName,
                                Action = I18NEntity.GetString("RM_UI_Detail_Add"),
                                Status = JobDetailsStatus.Successful,
                                Comment = "",
                                UniqueID = uniqueId
                            });
                            hasSuccessNode = true;
                            logger.Info("Process item:{0}", sourceUrl);
                        }
                        catch (ServerUnauthorizedAccessException se)
                        {
                            logger.Warn("set value error,webId:{0},listId:{1},itemId:{2},url:{3},error{4}", itemWebId, itemListId, itemId, sourceUrl, se.ToString());
                            //finalDetails.Add(new JMUniqueIDSettingJobDetails()
                            //{
                            //    ObjectName = objName,
                            //    SourceURL = sourceUrl,
                            //    ColumnName = this.columnDisplayName,
                            //    Action =  I18NEntity.GetString("RM_UI_Detail_Add"),
                            //    Status = JobDetailsStatus.Failed,
                            //    Comment = "RM_SS_DocLibraryAccessDeny",
                            //    UniqueID = uniqueId
                            //});
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("error occurred while process item,webId:{0},listId:{1},itemId:{2},error:{3}", itemWebId, itemListId, itemId, e.ToString());
                } 
            }
        }

        private void EnsureListField(List list, Web web)
        {
            if (mEnsureFieldListIDs.Contains(list.Id))
            {
                return;
            }
            Field field = null;
            bool isExist = false;
            try
            {
                field = list.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(field);
                mClientContext.ExecuteQuery();
                isExist = true;
            }
            catch (Exception)
            {
                logger.Info("need create new field,url:{0}", MakeFullUrl(mClientContext.Site.Url, list.RootFolder.ServerRelativeUrl));
            }
            if (!isExist)
            {
                var webField = EnsureWebField(web);
                field = list.Fields.AddFieldAsXml(webField.SchemaXml, false, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddToAllContentTypes);
                mClientContext.Load(field);
                //mClientContext.Load(list, l => l.Title, l => l.RootFolder.ServerRelativeUrl);
                mClientContext.ExecuteQuery();
                finalDetails.Add(new JMUniqueIDSettingJobDetails()
                {
                    ObjectName = list.Title,
                    SourceURL = MakeFullUrl(mClientContext.Site.Url, list.RootFolder.ServerRelativeUrl),
                    ColumnName = this.columnDisplayName,
                    Action = I18NEntity.GetString("RM_UI_Detail_Add"),
                    Status = JobDetailsStatus.Successful,
                    Comment = "",
                    UniqueID = ""
                });
                hasSuccessNode = true;
            }
            else
            {
                mClientContext.Load(list, l => l.Fields);
                Field listField = list.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(listField, l => l.Title);
                mClientContext.ExecuteQuery();
                var oldDisplayName = listField.Title;
                if (this.columnDisplayName != oldDisplayName)
                {
                    UpdateUniqueIDColumn(listField);
                    finalDetails.Add(new JMUniqueIDSettingJobDetails()
                    {
                        ObjectName = list.Title,
                        SourceURL = MakeFullUrl(mClientContext.Site.Url, list.RootFolder.ServerRelativeUrl),
                        ColumnName = this.columnDisplayName,
                        Action = I18NEntity.GetString("RM_UI_Detail_Update"),
                        Status = JobDetailsStatus.Successful,
                        Comment = "",
                        UniqueID = ""
                    });
                }
            }
            mEnsureFieldListIDs.Add(list.Id);
            //return field;
        }
        private Field EnsureWebField(Web web)
        {
            Field field = null;
            bool isExist = false;
            try
            {
                field = mClientContext.Web.Fields.GetById(RevIMUniqueIDColumnID);
                mClientContext.Load(field, f => f.Title, f => f.SchemaXml);
                mClientContext.ExecuteQuery();
                isExist = true;
            }
            catch (Exception)
            {

            }
            if (!isExist)
            {
                field = mClientContext.Web.Fields.AddFieldAsXml("<Field Type='" + "Text" + "' Name='" + this.RevIMUniqueIDInternalName + "' ID='" + RevIMUniqueIDColumnID + "' DisplayName='" + this.columnDisplayName + "' ReadOnly = 'TRUE'  StaticName='" + this.RevIMUniqueIDInternalName + "' />",
                    false, AddFieldOptions.AddFieldInternalNameHint | AddFieldOptions.AddToAllContentTypes);
                mClientContext.Load(field);
                mClientContext.ExecuteQuery();
                finalDetails.Add(new JMUniqueIDSettingJobDetails()
                {
                    ObjectName = web.Title,
                    SourceURL = MakeFullUrl(mClientContext.Site.Url, web.Url),
                    ColumnName = this.columnDisplayName,
                    Action = I18NEntity.GetString("RM_UI_Detail_Add"),
                    Status = JobDetailsStatus.Successful,
                    Comment = "",
                    UniqueID = ""
                });
                hasSuccessNode = true;
            }
            else
            {
                if (this.columnDisplayName != field.Title)
                {
                    UpdateUniqueIDColumn(field);
                    finalDetails.Add(new JMUniqueIDSettingJobDetails()
                    {
                        ObjectName = web.Title,
                        SourceURL = MakeFullUrl(mClientContext.Site.Url, web.Url),
                        ColumnName = this.columnDisplayName,
                        Action = I18NEntity.GetString("RM_UI_Detail_Update"),
                        Status = JobDetailsStatus.Successful,
                        Comment = "",
                        UniqueID = ""
                    });
                }
            }
            return field;
        }
        private void UpdateUniqueIDColumn(Field uniqueIdField)
        {
            uniqueIdField.Title = this.columnDisplayName;
            uniqueIdField.Update();
            mClientContext.Web.Update();
            mClientContext.ExecuteQuery();
            logger.Info("update  UniqueID Column Success");
        }

        #region Check designLists
        private bool CheckListIsSkipByListId(Guid listId)
        {
            return mIsDesignListIDs.Contains(listId);
        }
        private bool CheckListIsSkipByBaseTemplate(List list)
        {
            if (mIsNeedUesUniqueIDListIDs.Contains(list.Id))
            {
                return false;
            }
            mClientContext.Load(list, l => l.Fields, l => l.RootFolder, l => l.BaseTemplate, l => l.Title, l => l.Id);
            mClientContext.ExecuteQuery();

            if (list.BaseTemplate == 600)
            {
                mIsDesignListIDs.Add(list.Id);
                logger.Info("Skip the design list & system list{0}", list.RootFolder.Name);
                return true;
            }
            if (CheckIsDesignList(list.RootFolder.Name + list.BaseTemplate.ToString()))
            {
                mIsDesignListIDs.Add(list.Id);
                logger.Info("Skip the design list {0}", list.RootFolder.Name);
                return true;
            }
            mIsNeedUesUniqueIDListIDs.Add(list.Id);
            return false;
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
        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.mDesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch
            { }
            return isDesignList;
        }
        #endregion

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
        private ChangeQuery BuildChangeQuery(string spSiteId, DateTime startTime)
        {
            
            DateTime endTime = mUTCJobStartTime;
            logger.Info("BuildChangeQuery:{0} - {1}", startTime.ToString("yyyy-MM-dd HH:mm:ss"), endTime.ToString("yyyy-MM-dd HH:mm:ss"));
            ChangeQuery query = new ChangeQuery(false, false);
            query.Web = true;
            query.List = true;
            query.Item = true;
            //query.File = true;

            query.Add = true;
            ChangeToken startToken = new ChangeToken();
            ChangeToken endToken = new ChangeToken();
            startToken.StringValue = "1;1;" + spSiteId + ";" + startTime.Ticks.ToString() + ";-1";
            endToken.StringValue = "1;1;" + spSiteId + ";" + endTime.Ticks.ToString() + ";-1";
            query.ChangeTokenStart = startToken;
            query.ChangeTokenEnd = endToken;
            return query;
        }

        private DateTime GetStartDate()
        {
            DateTime utcNow = DateTime.UtcNow;
            var globalTimeZoneId = GeneralSettingService.GetGeneralSetting().TimeZoneId;
            TimeZoneInfo localZone = TimeZoneInfo.FindSystemTimeZoneById(globalTimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
            var localYesterday = localNow.AddDays(-1);

            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(localYesterday.Year, localYesterday.Month, localYesterday.Day, 0, 0, 0), localZone);
            return startTime;
        }


        #region Property Method
        public object CastEnumValue(object value)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());
            return Convert.ChangeType(value, underlyingType);
        }
        public void CopyProperty(Dictionary<string, object> proDic, ClientObject Obj)
        {
            ClientObjectData objData = Obj.GetObjectData();
            Dictionary<string, object> clientObjData = objData.Properties;
            foreach (KeyValuePair<string, object> propertyInfo in clientObjData)
            {
                object obj = propertyInfo.Value;
                if (obj == null)
                {
                    proDic[propertyInfo.Key] = null;
                }
                else
                {
                    Type proType = obj.GetType();
                    if (proType.IsEnum)
                    {
                        proDic[propertyInfo.Key] = CastEnumValue((obj));
                    }
                    else
                    {
                        proDic[propertyInfo.Key] = obj;
                    }
                }
            }
        }
        #endregion

        #region Deteils
        private void RunUpdateJobDetails(List<JMUniqueIDSettingJobDetails> details)
        {
            List<JMUniqueIDSettingJobDetails> needUpdateDetails = this.CloneJobDetailsAddSCUrl(details);
            if (details.Count == 0)
            {
                return;
            }
            JobDetailService.UpdateJobDetails(details, baseJobDto);
            details.Clear();
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
        #endregion

        #region Public tool method
        public static string MakeFullUrl(string siteUrl, string strUrl)
        {
            if (siteUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (siteUrl == strUrl)
            {
                return siteUrl;
            }

            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(siteUrl);
                stringBuilder.Append("https:");
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(siteUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }
        #endregion

        private void UpdateSettingJobStatus(bool isEmptyDetails)
        {
            JobDetailService.UploadJobDetailsAndReport(baseJobDto);
            //更新Job进度
            RMJobService.UpdateJobProgress(this.currentJobId, 100);
            if (isEmptyDetails)
            {
                RMJobService.UpdateJobStatus(this.currentJobId, JobStatus.Finished, isEmptyDetails ? I18NEntity.GetString("RM_UI_JobNoData") : "");
            }
            else if (hasSuccessNode && hasErrorNode)
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
                RMJobService.UpdateJobStatus(currentJobId, JobStatus.Skipped, "RM_SS_JobSkip");
            }
            
        }
    }
}
