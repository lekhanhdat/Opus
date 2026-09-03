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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Common.Report;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System.Reflection;
using Microsoft.SharePoint.Client.Search.Portability;
using Microsoft.SharePoint.Client.Search.Administration;
using System.Xml;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.Hybrid.Utility.Configuration;
using System.Net;
using AgentUtil = AvePoint.RA.SharePoint.Common.Util;

namespace AvePoint.RA.SharePoint.UniqueIdSetting.Base
{
    public abstract class BaseUniqueIdSettingProcessor
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string RevIMUniqueIDInternalName = "RevIMUniqueID";
        private Guid RevIMUniqueIDColumnID
        {
            get
            {
                return new Guid("40f84bba906045b4af568ee102a52dcb");
            }
        }
        public Guid DocumentIDColumnID
        {
            get
            {
                return new Guid("3b63724f-3418-461f-868b-7706f69b029c");
            }
        }

        private Guid documentIDServiceFeatureId = new Guid("b50e3104-6812-424f-a011-cc90e6327318");
        protected IAveSite curSite;
        protected List<string> DesignLists = new List<string>();
        protected GRMUniqueIdSetting curSetting;
        protected SPTreeNodeDto curNode;
        protected ClientContext currentClientContext;  //based on per sitecollection
        protected SPTreeNodeDto groupNode;
        public bool haveErrorNode = false;
        public IProgressService ProgressService { get; set; }
        public IReportService<JMJobDetails> JobDetailService { get; set; }
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();

        private string idFormat = "{0}-{1}";
        protected string searchSiteColumnFileName;
        public int MaxItemsPerThrottledOperation;
        public List<WebEnableSetting> WebEnableSettings { get; set; }
        public List<SiteEnableSetting> SiteEnableSettings { get; set; }
        public Dictionary<string, SiteInfo> SiteInformationDic { get; set; }
        public long LastScanTime { get; set; }
        public long MainJobStartTime { get; set; }
        public BaseUniqueIdSettingProcessor()
        {
            //WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;    //avoid too many versions
            this.DesignLists = WebUtil.GetDesignLists();
        }
        public BaseUniqueIdSettingProcessor(SPTreeNodeDto siteNode, UniqueIdSettingJobMessage jobMessage)
        {
            //WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false; //avoid too many versions
            this.DesignLists = WebUtil.GetDesignLists();
            curNode = siteNode;
            groupNode = GetGroupNode(siteNode);
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            InitJobMessage(jobMessage);
            ProgressService.Increase();
        }

        public void InitJobMessage(UniqueIdSettingJobMessage mJobMessage)
        {
            curSetting = mJobMessage.CurUniqueIdSetting;
            WebEnableSettings = mJobMessage.WebEnableSettings;
            SiteEnableSettings = mJobMessage.SiteEnableSettings;
            SiteInformationDic = mJobMessage.SiteInformationDic;
            LastScanTime = SiteInformationDic[curNode.FullPath].LastScanTime;
            MainJobStartTime = mJobMessage.MainJobStartTime;
            logger.Info($"Current Node Path:{curNode.FullPath.LogBase64()} LastScanTime:{LastScanTime} MainJobStartTime:{MainJobStartTime}");
        }

        public int GetMaxItemsPerThrottledOperation(IAveSite aveSite)
        {
            int maxItemsPer = 2000; //5000;  //SPO默认值为5000 并且不能修改， 某些Library 5000分页查询依然会超出Throttle， 限制到2000   from CI
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as AveDictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                    logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");
                    if (maxItemsPer > 2000)
                    {
                        logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                        maxItemsPer = 2000;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }

        public virtual void ProcessSiteCollection(AveDiscoverSite discoverSite)
        {
            EnableFeatureAndUpdateBeginID();
            ConfigUniqueIdColumn(curSite);
            ProgressService.Increase();
            this.MaxItemsPerThrottledOperation = GetMaxItemsPerThrottledOperation(discoverSite.Site);
        }
        public virtual void ProcessWeb(AveDiscoverWeb discoverWeb)
        {
            if (!discoverWeb.AveWeb.IsRootWeb)
            { 
                UpdateWebProperyForDocumentIDSettings(discoverWeb.AveWeb);
            }
            ProgressService.Increase();
        }
        public virtual void ProcessList(AveDiscoverList discoverList)
        {
            if (discoverList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var list = discoverList.GetListObject();
            if (list.Hidden)
            {
                logger.Info("Skip the hidden list {0}", discoverList.RootFolderUrl.LogBase64());
                return;
            }
            if (CheckIsDesignList(list))
            {
                logger.Info("Skip the system list {0}", discoverList.RootFolderUrl.LogBase64());
                ProgressService.Increase();
                return;
            }
            try
            {
                ConfigUniqueIdColumn(discoverList.GetListObject());
                ProgressService.Increase();
            }
            catch (Exception e)
            {
                logger.Warn("config column failed {0}", e.ToString());
                haveErrorNode = true;
            }
        }
        public virtual void ProcessFolder(AveDiscoverFolder discoverFolder)
        {
            try
            {
                var list = discoverFolder.AveFolder.ParentList;
                var tempFolder = list.GetItemByUniqueId(discoverFolder.AveFolder.UniqueId);
                if (tempFolder != null)
                {
                    SetUniqueId(tempFolder);
                    ProgressService.Increase();
                }
            }
            catch (Exception e)
            {
                logger.Warn("Physical File Set Unique Id failed, message: {0}", e.ToString());
            }
        }
        public virtual void Run()
        {
           
        }

        public void EnableFeatureAndUpdateBeginID()
        {
            //try
            //{
            //    logger.Info("Try to disable DenyAddAndCustomizePages");
            //    SPTreeNodeDto siteNode = GetSiteCollectionNode(curNode);
            //    var bposInfo = GetBPOSInfo();
            //    var factory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.Url, bposInfo, AveContextKind.ClientObjectModel);
            //    IAveTenant tenant = factory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, siteNode.Url));
            //    ////var siteProperties = tenant.GetSitePropertiesByUrl(siteNode.Url);
            //    ////SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, siteNode.Url);
            //}
            //catch (Exception e)
            //{
            //    logger.Error(e.Message, e);
            //}
            EnableFeature();
            UpdateWebProperyForDocumentIDSettings(curSite.RootWeb);
        }

        public AveDiscoverSite GetDiscoverSite()
        {
            var bposInfo = GetBPOSInfo();
            var mfactory = AveObjectModelFactory.CreateObjectModelFactory(curNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            curSite = mfactory.CreateSite(curNode.FullPath);
            AveDiscoverSite tmpDiscoverSite = null;
            if (LastScanTime == DateTime.MinValue.Ticks)
            {
                logger.Info("need start full unique id setting job :{0}", curNode.FullPath.LogBase64());
                tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
            }
            else
            {
                logger.Info("need start incremental unique id setting job :{0}", curNode.FullPath.LogBase64());
                tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(LastScanTime, DateTimeKind.Utc), new DateTime(MainJobStartTime, DateTimeKind.Utc));
            }
            return tmpDiscoverSite;
        }

        private DateTime ModifyTime(DateTime time)
        {
            if (time == DateTime.MinValue) return time;
            //int offsetInMinuete = 120; // default value is 2 hours
            //int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.UniqueIdJobRunTimeOffsetInMinute], out offsetInMinuete); //TODO
            int offsetInMinuete = 1;
            var runTime = time.AddMinutes(-offsetInMinuete);
            logger.Info($"Modified job run time : {runTime}");
            return runTime;
        }

        public AveBPOSAccountInfo GetBPOSInfo()
        {
            
            var account = AgentAccountUtil.Get();
            AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
            {
                Domain = account.Domain,
                UserName = account.UserName,
                Password = account.Password
            };
            return aveBPOSAccountInfo;
        }

        public void EnableFeature()
        {
            try
            {
                if (curSite.Features[documentIDServiceFeatureId] == null)
                {
                    curSite.Features.Add(documentIDServiceFeatureId, true);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn("Document ID Service Feature is not installed in this farm, cannot set Document Unique ID:{0}", ex.ToString());
            }
            catch (Exception ex)
            {
                logger.Warn("Activate Document ID Service feature error:{0}", ex.ToString());
            }
        }

        public void UpdateDocumentIDSettings()
        {
            try
            {
                string prefix = null;
                string docidEnabled = null;
                if (curSite.RootWeb.AllProperties.ContainsKey("docid_enabled"))
                {
                    docidEnabled = curSite.RootWeb.AllProperties["docid_enabled"]?.ToString();
                    logger.Info("current docid enabled? {0}", docidEnabled);
                }
                if (curSite.RootWeb.AllProperties.ContainsKey("docid_msft_hier_siteprefix"))
                {
                    prefix = curSite.RootWeb.AllProperties["docid_msft_hier_siteprefix"].ToString();
                    logger.Info("current docid prefix: {0}, prefix in Records :{1}", prefix, curSetting.Prefix);
                }
                if(docidEnabled == null || docidEnabled != "1")
                {
                    logger.Info("Enable doc id and set prefix");
                    curSite.RootWeb.AllProperties["docid_enabled"] = "1";
                    curSite.RootWeb.AllProperties["docid_msft_hier_siteprefix"] = curSetting != null ? curSetting.Prefix : "";
                    curSite.RootWeb.Update();
                    //currentClientContext.Web.AllProperties["docid_enabled"] = "1";
                    //currentClientContext.Web.AllProperties["docid_msft_hier_siteprefix"] = curSetting != null ? curSetting.Prefix : ""; 
                }
                else if (docidEnabled == "1")
                {
                    //currentClientContext.Web.AllProperties["docid_enabled"] = "1";
                    if(curSetting != null && curSetting.OverrideSPPrefix && prefix != curSetting.Prefix)
                    {
                        logger.Info("Update doc id prefix to {0}", curSetting.Prefix);
                        //currentClientContext.Web.AllProperties["docid_msft_hier_siteprefix"] = curSetting.Prefix;
                        curSite.RootWeb.AllProperties["docid_msft_hier_siteprefix"] = curSetting.Prefix;
                        curSite.RootWeb.Update();
                    }
                    else
                    {
                        logger.Info("No need to update doc id prefix");
                    }
                }
                //currentClientContext.Web.Update();
                //currentClientContext.ExecuteQuery();
            }
            catch (Exception e)
            {
                logger.Warn("Set Document Id setting error {0}", e.ToString());
                JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = "Document ID", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                haveErrorNode = true;
            }
        }

        public void UpdateWebProperyForDocumentIDSettings(IAveWeb curWeb)
        {
            string webName = "";
            string webFullUrl = "";
            try
            {
                var isRootWeb = curWeb.IsRootWeb;
                webName = isRootWeb ? curNode.Name : curWeb?.Name;
                webFullUrl = isRootWeb ? curNode.FullPath : curWeb?.Url;
                string prefix = null;
                string docidEnabled = null;
                logger.Info($"Process web:{curWeb?.Url.LogBase64()}, it is root web:{isRootWeb}");
                if (curWeb.AllProperties.ContainsKey("docid_enabled"))
                {
                    docidEnabled = curWeb.AllProperties["docid_enabled"]?.ToString();
                    logger.Info("current docid enabled? {0}", docidEnabled);
                }
                if (curWeb.AllProperties.ContainsKey("docid_msft_hier_siteprefix"))
                {
                    prefix = curWeb.AllProperties["docid_msft_hier_siteprefix"].ToString();
                    logger.Info("current docid prefix: {0}, prefix in Records :{1}", prefix, curSetting.Prefix);
                }
                if (docidEnabled == null || docidEnabled != "1")
                {
                    logger.Info("Enable doc id and set prefix");
                    curWeb.AllProperties["docid_enabled"] = "1";
                    curWeb.AllProperties["docid_msft_hier_siteprefix"] = curSetting != null ? curSetting.Prefix : "";
                    curWeb.Update();
                }
                else if (docidEnabled == "1")
                {
                    if (curSetting != null && curSetting.OverrideSPPrefix && prefix != curSetting.Prefix)
                    {
                        logger.Info("Update doc id prefix to {0}", curSetting.Prefix);
                        curWeb.AllProperties["docid_msft_hier_siteprefix"] = curSetting.Prefix;
                        curWeb.Update();
                    }
                    else
                    {
                        logger.Info("No need to update doc id prefix");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Set Document Id setting error {0}", e.ToString());
                JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = webName, SourceURL = webFullUrl, ColumnName = "Document ID", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                haveErrorNode = true;
            }
        }
        protected void ConfigUniqueIdColumn(IAveSite siteCollection)
        {
            try
            {
                IAveField field = null;
                field = siteCollection.RootWeb.Fields.GetFieldById(RevIMUniqueIDColumnID, false);
                if (field == null)
                {
                    field = siteCollection.RootWeb.Fields.AddFieldAsXml("<Field Type='Text' Name='" + this.RevIMUniqueIDInternalName + "' ID='" + RevIMUniqueIDColumnID + "' DisplayName='" + curSetting.Name + "' ReadOnly = 'TRUE'  StaticName='" + this.RevIMUniqueIDInternalName + "' />", false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes);
                    JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = siteCollection.RootWeb.Title, SourceURL = siteCollection.Url, ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Successful, Comment = "" });

                }
                else if (field != null)
                {
                    if (field.Title != curSetting.Name)
                    {
                        try
                        {
                            field.Title = curSetting.Name;
                            field.Update();
                            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = siteCollection.RootWeb.Title, SourceURL = siteCollection.Url, ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Successful, Comment = "" });
                            
                        }
                        catch (Exception ee)
                        {
                            logger.Error("update column faild,ERROR:{0}", ee.ToString());
                            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = siteCollection.RootWeb.Title, SourceURL = siteCollection.Url, ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(ee) });
                        }
                    }
                    else
                    {
                        JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = siteCollection.RootWeb.Title, SourceURL = siteCollection.Url, ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Skipped, Comment = "RM_UID_ColumnExist" });
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Config site collection unique id column failed {0}", e.ToString());
                JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = siteCollection.RootWeb.Title, SourceURL = siteCollection.Url, ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                throw e;
            }
        }
        protected void ConfigUniqueIdColumn(IAveList list)
        {
            try
            {
                IAveField field = list.Fields.GetFieldById(RevIMUniqueIDColumnID, false);
                if (field == null)
                {
                    var siteField = list.ParentWeb.Site.RootWeb.Fields.GetFieldById(RevIMUniqueIDColumnID, false);
                    field = list.Fields.AddFieldAsXml(siteField.SchemaXml, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes);
                    JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Add", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Successful, Comment = string.Empty });

                }
                else if (field != null)
                {
                    if (field.Title != curSetting.Name)
                    {
                        try
                        {
                            field.Title = curSetting.Name;
                            field.Update();
                            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Successful, Comment = string.Empty });
                        }
                        catch (Exception ee)
                        {
                            logger.Error("update column1 faild,ERROR:{0}", ee.ToString());
                            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(ee) });
                        }
                    }
                    else
                    {
                        JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Skipped, Comment = "RM_UID_ColumnExist" });
                    }
                    
                }
            }
            catch (Exception e)
            {
                logger.Warn("Config list unique id column failed {0}", e.ToString());
                JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = "Document ID", Action = "RM_UI_Detail_Update", AgentName = AvePoint.GCommon.Utility.OSInformation.HostName, Status = JobDetailsStatus.Failed, Comment = AgentUtil.GetExceptionMessage(e) });
                throw e;
            }
        }
        
        protected void InitSearchFieldColumnName()
        {
            //var tmpSiteColumnName = string.Empty;
            //var tmpListColumnName = string.Empty;

            //var jobConfig = RMGlobalConfiguration.AppConfig.SubJobCountForCustomer;
            //if (jobConfig != null && jobConfig.Tenants != null)
            //{
            //    var tenantConfig = jobConfig.Tenants.First(t => t.TenantId.ToString() == TenantLocalValue.LogonGroupId);
            //    tmpSiteColumnName = tenantConfig?.UniqueIdJobSearchSiteColumnFieldName;
            //    tmpListColumnName = tenantConfig?.UniqueIdJobSearchListColumnFieldName;
            //}

            //searchSiteColumnFileName = !string.IsNullOrEmpty(tmpSiteColumnName) ? tmpSiteColumnName.Trim() : RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.UniqueIdJobSearchSiteColumnFieldName];
            //logger.Info($"UniqueIdJobSearchSiteColumnFieldName : {searchSiteColumnFileName}");
        }

        /// <summary>
        /// Check duplicated records based on created time and recordsId.
        /// If there exist an item which created time is less then current item's created time, it means current item is duplicated and need to be changed.
        /// </summary>
        /// <param name="recordsId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool HasDuplicateRecordsId(string recordsId, IAveListItem item)
        {
            if (string.IsNullOrEmpty(searchSiteColumnFileName)) return false;

            try
            {
                var keywordQuery = new KeywordQuery(currentClientContext);
                keywordQuery.TrimDuplicates = false;
                keywordQuery.RowLimit = 1;
                keywordQuery.StartRow = 0;

                var createdPropName = "Created";

                //if (!item.FieldValues.ContainsKey(createdPropName))
                //{
                //    logger.Warn("Property 'Created' does not exist");
                //    return false;
                //}
                var createdTime = DateTime.Parse(item.FieldValues[createdPropName].ToString());
               
                //keywordQuery.QueryText = $"({searchSiteColumnFileName}:\"{recordsId}\" OR {searchListColumnFileName}:\"{recordsId}\") AND {createdPropName} < \"{item.FieldValues[createdPropName]}\"";
                //keywordQuery.QueryText = $"{searchSiteColumnFileName}:\"{recordsId}\" OR {searchListColumnFileName}:\"{recordsId}\"";
                keywordQuery.QueryText = $"{searchSiteColumnFileName}:\"{recordsId}\"";
                //logger.Info($"QueryText : {keywordQuery.QueryText}");
                keywordQuery.SelectProperties.Add(createdPropName);
                keywordQuery.SelectProperties.Add("UniqueId");
                keywordQuery.EnableSorting = true;
                keywordQuery.SortList.Add(createdPropName, SortDirection.Ascending);

                var searchExecutor = new SearchExecutor(currentClientContext);
                var results = searchExecutor.ExecuteQuery(keywordQuery);

                currentClientContext.ExecuteQuery();

                //var count = results.Value[0].ResultRows.Count();
                //if (count > 0)
                //{
                //    logger.Warn($"Has duplicated records Id. recordsId : {recordsId}, itemUniqueId : {item.UniqueId}");
                //}
                //return count > 0;
                //query的记录已经按照create time升序排列，如果第一个记录不是当前item，那么说明当前item需要修改
                if (results.Value[0].ResultRows.Count() > 0)
                {
                    var dic = results.Value[0].ResultRows.First();
                    var uniqueIdPropName = "UniqueID";
                    if (dic.ContainsKey(uniqueIdPropName))
                    {
                        var v = dic[uniqueIdPropName].ToString();
                        if (!(new Guid(v)).Equals(item.UniqueId))
                        {
                            logger.Warn($"Has duplicated records Id. recordsId : {recordsId}, itemUniqueId : {item.UniqueId}");
                            return true;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Warn($"An error occurred while checking duplicated records. \r\n error: {e.ToString()}");
            }

            return false;
        }
        
        private bool CheckDuplicate(IDictionary<string, object> dic, string key, Guid uniqueId)
        {
            if (dic.ContainsKey(key))
            {
                var v = dic[key].ToString();
                logger.Warn($"key : {key.LogBase64()},  retrieved UniqueId : {new Guid(v)}, compared UniqueId : {uniqueId}");
                if (!(new Guid(v)).Equals(uniqueId))
                {
                    return true;
                }
            }

            return false;
        }
        protected void SetUniqueId(IAveListItem item)
        {
            //try
            //{
            //    #region add skip logic
            //    var itemContentType = item.ContentType.Name;
            //    var filterContentTypes = new List<string>() { "Physical File", "Physical Box" };
            //    string itemName = string.Empty;
            //    string objectName = string.Empty;
            //    try
            //    {
            //        var itemType = item["FSObjType"].ToString();
            //        itemName = item["FileLeafRef"].ToString();
            //        var list = item.ParentList;//TO DO ylgu merge
            //        if (list.BaseType == AveBaseType.DocumentLibrary || itemContentType == "Message")
            //        {
            //            objectName = itemName;
            //        }
            //        //REC-4226...
            //        else if (list.BaseTemplate == AveListTemplateType.Links)
            //        {
            //            objectName = item.DisplayName;
            //        }
            //        else
            //        {
            //            objectName = item["Title"].ToString();
            //        }
            //        if (itemType == "1" && !filterContentTypes.Contains(itemContentType))
            //        {
            //            logger.Info("skip set value : Item name:{2} ContentType:{0},Type:{1}", itemContentType, itemType, item["FileLeafRef"].ToString());
            //            return;
            //        }
            //        if (item.IsBlockEditAndDeleteRecord())
            //        {
            //            logger.Info("Skip set unique id for declard records item {0}", item.Url);
            //            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = GetObjectName(item), SourceURL = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url), ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Skipped, Comment = "RM_SS_ItemBlockEditAndDelete" });
            //            return;
            //        }
            //        if (item.CheckHasHold())
            //        {
            //            logger.Info("Skip set unique id for hold item {0}", item.Url);
            //            JobDetailService.Commit(new JMUniqueIDSettingJobDetails() { ObjectName = GetObjectName(item), SourceURL = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url), ColumnName = curSetting.Name, Action = "RM_UI_Detail_Update", Status = JobDetailsStatus.Skipped, Comment = "RM_SS_ItemHoldBlockEditAndDelete" });
            //            return;
            //        }

            //    }
            //    catch (Exception e)
            //    {
            //        logger.Info("Handle file or item name error {0}:{1}", itemName, e.ToString());
            //    }

            //    #endregion
            //    var RecordsIdInSP = item.FieldValues.ContainsKey(RevIMUniqueIDInternalName) ? item.FieldValues[RevIMUniqueIDInternalName]?.ToString() : string.Empty;
            //    logger.Info("Records ID in sp {0}:{1}", item.Url, RecordsIdInSP);

            //    var hasDuplicateRecordsIdInSP = false;
            //    if (!string.IsNullOrEmpty(RecordsIdInSP))
            //    {
            //        //search to check if there exist items with duplicated records id
            //        hasDuplicateRecordsIdInSP = HasDuplicateRecordsId(RecordsIdInSP, item);

            //    }
            //    //RMBaseRecord record = ExplorerDao.GetRecordByUniqueId(curSite.ID, item.UniqueId);(agent commenty)
            //    string uniqueId = string.Empty;
            //    string recordsIDInDB = string.Empty;
            //    //recordsIDInDB = ExplorerDao.QueryAll(r => r.ScopeId == curSite.ID && r.ItemId == item.UniqueId).FirstOrDefault()?.RecordsId;
            //    try
            //    {
            //        var id = IDGenerator.GetRecordId(curSite.ID, item.UniqueId);
            //        recordsIDInDB = ExplorerDao.ReadById(curSite.ID, id)?.RecordsId;
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Warn("get record id from explrer error: {0}", e.ToString());
            //    }

            //    if (!string.IsNullOrEmpty(recordsIDInDB))
            //    {
            //        if (!hasDuplicateRecordsIdInSP)
            //        {
            //            var prefix = this.GetPrefixFromExistingID(recordsIDInDB);
            //            if (curSetting.Prefix == null || prefix.Equals(curSetting.Prefix))
            //            {
            //                uniqueId = recordsIDInDB;
            //            }
            //            else
            //            {
            //                if (string.IsNullOrEmpty(prefix))
            //                {
            //                    uniqueId = string.Format(idFormat, curSetting.Prefix, recordsIDInDB);
            //                }
            //                else if (string.IsNullOrEmpty(curSetting.Prefix))
            //                {
            //                    uniqueId = recordsIDInDB.Split('-').Last();
            //                }
            //                else
            //                {
            //                    uniqueId = recordsIDInDB.Replace(prefix, curSetting.Prefix);
            //                }
            //            }
            //        }
            //        if (uniqueId != recordsIDInDB)
            //        {
            //            try
            //            {
            //                ExplorerDao.UpdateAll(r => r.ScopeId == curSite.ID && r.NodeId == item.UniqueId, rec => rec.RecordsId = uniqueId);
            //                //CollectionDataDao.UpdateRecordUniqueId(curSite.ID, item.UniqueId, uniqueId);
            //                logger.Debug("update record uniqueId, old:{0}, new:{1}", recordsIDInDB, uniqueId);
            //            }
            //            catch (Exception e)
            //            {
            //                logger.Warn("update record id to explrer error: {0}", e.ToString());
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (!string.IsNullOrEmpty(RecordsIdInSP) && !hasDuplicateRecordsIdInSP)//Prefix check for new added documents.
            //        {
            //            var prefix = RecordsIdInSP.Contains("-") ? RecordsIdInSP.Split('-').First() : string.Empty;
            //            if (!string.IsNullOrEmpty(prefix))
            //            {
            //                logger.Info("No need update item unique id {0}", item.Url);
            //                //
            //                return;
            //            }
            //        }
            //        uniqueId = FormateCurrentId(curSetting);
            //    }

            //    if (string.IsNullOrEmpty(RecordsIdInSP) || ((!string.IsNullOrEmpty(RecordsIdInSP) && !RecordsIdInSP.Equals(uniqueId, StringComparison.OrdinalIgnoreCase))))
            //    {
            //        item[RevIMUniqueIDInternalName] = uniqueId;
            //        item.SystemUpdateForRecords();
            //        ProgressService.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = GetObjectName(item), SourceURL = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url), ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Successful, UniqueID = uniqueId, Comment = "" });
            //    }

            //}
            //catch (Exception e)
            //{
            //    logger.Warn("Set Unique ID value failed {0}:{1}", item.Url, e.ToString()); ProgressService.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = GetObjectName(item), SourceURL = WebUtil.MakeFullUrl(item.ParentList.ParentWeb.Url, item.Url), ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Failed, Comment = Util.GetExceptionMessage(e) });
            //    haveErrorNode = true;
            //}
            ////ProgressService.Increase();
            ////ProgressService.Increase();
            ////TO DO Detail Or not... too many documents.....
        }
        /// <summary>
        /// 从现有的UniqueId中截取前缀， 要考虑前缀中可能包含 '-'
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        private string GetPrefixFromExistingID(string uniqueId)
        {
            if (uniqueId.Contains('-'))
            {
                int index = uniqueId.LastIndexOf('-');
                return uniqueId.Substring(0, index);
            }
            return string.Empty;
        }
        /// <summary>
        /// TO DO Common Method......
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private string FormateCurrentId(GRMUniqueIdSetting setting)
        {
            var result = string.Empty;
            //try
            //{
            //    string currentFormat = "{0}-{1}";
            //    string lockerKey = TenantLocalValue.LogonGroupId;
            //    if (setting != null)
            //    {
            //        result = string.IsNullOrEmpty(setting.Prefix) ? FormatNumber(RMGlobalLocker.GetId(lockerKey)) : string.Format(currentFormat, setting.Prefix, FormatNumber(RMGlobalLocker.GetId(lockerKey)));
            //    }
            //    else
            //    {
            //        result = string.Format(currentFormat, UniqueIdConfig.DefaultPrefix, FormatNumber(RMGlobalLocker.GetId(lockerKey)));
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.Info("Failed to formate currentId : " + e.ToString());
            //    throw;
            //}
            return result;
        }
        private string FormatNumber(long number)
        {
            var result = string.Empty;
            try
            {
                int digit = 10;
                if (number < (Math.Pow(10, digit - 1)))
                {
                    result = number.ToString().PadLeft(10, '0');
                }
                else
                {
                    result = number.ToString();
                }
            }
            catch (Exception e)
            {
                logger.Info(string.Format("Failed to formate number {0} : {1}", number, e.ToString()));
                throw;
            }
            return result;
        }
        private string GetObjectName(IAveListItem aveItem)
        {
            var listType = aveItem.ParentList.BaseType;
            string objName = string.Empty;
            var isLibrary = listType == AveBaseType.DocumentLibrary;
            if (isLibrary)
            {
                objName = aveItem.Name;
            }
            else
            {
                if (aveItem.FieldValues.ContainsKey("Title") && aveItem.FieldValues["Title"] != null)
                {
                    objName = aveItem.FieldValues["Title"].ToString();
                }
            }
            if (string.IsNullOrEmpty(objName))
            {
                if (aveItem.FieldValues.ContainsKey("Name") && aveItem.FieldValues["Name"] != null)
                {
                    objName = aveItem.FieldValues["Name"].ToString();
                }
                else if (aveItem.FieldValues.ContainsKey("URL") && aveItem.FieldValues["URL"] != null)
                {
                    var url = aveItem.FieldValues["URL"].ToString();
                    var urlArr = url.Split(',');
                    if (urlArr.Length == 2)
                    {
                        objName = urlArr[1];
                    }
                }
                else if (aveItem.FieldValues.ContainsKey("FileLeafRef") && aveItem.FieldValues["FileLeafRef"] != null)
                {
                    objName = aveItem.FieldValues["FileLeafRef"].ToString();
                }
            }
            return objName;
        }
        protected DateTime GetStartDate(string gorupId, string siteId)
        {
            return DateTime.UtcNow;
            //var dateTime = RMNodeFlagDao.GetSPValidChangeTime((int)NodeFlagType.UniqueId, new Guid(gorupId), new Guid(siteId));
            //return new DateTime(dateTime);
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

        protected void InitSearchContext(AveBPOSAccountInfo bposInfo, string siteUrl)
        {
            //var clientContext = new AvePoint.RA.RACommonUtility.CommonClientContext();
            //currentClientContext =  clientContext.InitClientContext(bposInfo, siteUrl);
            //var isSearchConfiged = CheckSearchConfig(currentClientContext);
            //if (!isSearchConfiged)
            //{
            //    logger.Warn("Search mapping in O365 is not configured, will not check the duplicated records id.");
            //}
            //else
            //{
            //    InitSearchFieldColumnName();
            //}
        }

        private bool CheckSearchConfig(ClientContext context)
        {
            try
            {
                var sconfig = new SearchConfigurationPortability(context);
                var owner = new SearchObjectOwner(context, SearchObjectLevel.SPSiteSubscription);
                ClientResult<string> configResults = sconfig.ExportSearchConfiguration(owner);
                context.ExecuteQuery();
                if (configResults != null)
                {
                    return Check(configResults.Value);
                }
            }
            catch(Exception e)
            {
                logger.Warn($"An error occurred while check search config. will disable the search flag. error : {e.ToString()}");
            }

            return false;
        }

        private bool Check(string configXml)
        {
            if (string.IsNullOrEmpty(configXml)) return false;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configXml);
            var docElement = xmlDoc.DocumentElement;
            var childNodes = docElement.ChildNodes;
            for (var i = 0; i < childNodes.Count; i++)
            {
                var item = childNodes.Item(i);
                if (item.Name == "SearchSchemaConfigurationSettings")
                {
                    for (var j = 0; j < item.ChildNodes.Count; j++)
                    {
                        var itemb = item.ChildNodes.Item(j);
                        if (itemb.Name == "Mappings")
                        {
                            if (itemb.InnerText.Contains("ows_q_TEXT_RevIMUniqueID"))
                            {
                                return true;
                            }
                        }
                    }

                    break;
                }
            }

            return false;
        }
        public bool CheckWebNeedSkip(AveDiscoverWeb discoverWeb)
        {
            if (WebEnableSettings != null)
            {
                var webSetting = WebEnableSettings.Where(o => o.GroupId == new Guid(groupNode.SPObjectId) && o.SiteId == new Guid(curNode.ID) && o.WebId == discoverWeb.WebID).FirstOrDefault();
                if (webSetting != null && !webSetting.EnableRecordsManagement)
                {
                    logger.Info("Skip web SharePoint setting is disable {0}", discoverWeb.FullUrl.LogBase64());
                    JobDetailService.Commit(new JMUniqueIDSettingJobDetails()
                    {
                        ObjectName = discoverWeb.Name,
                        SourceURL = discoverWeb.FullUrl,
                        ColumnName = "Document ID",
                        Action = "RM_UI_Detail_Add",
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        Status = JobDetailsStatus.Skipped,
                        Comment = "RM_JS_JMD_DisableRecordManagement"
                    });
                    return true;
                }
            }
            return false;
        }
    }
}
