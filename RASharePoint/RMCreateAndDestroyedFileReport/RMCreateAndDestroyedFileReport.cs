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
//using AvePoint.RA.CommonUtil;
//using AvePoint.GCommon.Contract.Tree.Object;
//using AvePoint.RA.Common;
//using AvePoint.RA.Common.Util;
//using AvePoint.RA.Contract.DocAve;
//using AvePoint.RA.Contract.JobMonitor;
//using AvePoint.RA.Contract.Object;
//using AvePoint.RA.Contract.RMWeb;
//using AvePoint.RA.Contract.RMWeb.JobMonitor;
//using AvePoint.RA.Contract.RMWeb.ReportCenter;
//using AvePoint.RA.DB.Model;
//using AvePoint.RA.I18N.Core;
//using AvePoint.RA.SharePoint.Common;
//using AvePoint.RA.SharePoint.Discover.Base;
//using AvePoint.RA.SharePoint.Object;
//using AvePoint.Wrapper.Common;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Xml;
//using AvePoint.RA.DB.Dao;
//using AvePoint.GCommon.Contract.StorageOptimization.Object;
//using AvePoint.RA.Contract.Tenant;
//using Newtonsoft.Json;
//using AvePoint.RA.Contract.Exceptions;
//using AvePoint.RA.RADataBroker;
//using AvePoint.RA.Common.Report;
//using AvePoint.RA.Contract.Object.JobMessage;

//namespace AvePoint.RA.SharePoint.RMCreateAndDestroyedFileReport
//{
//    public class RMCreateAndDestroyedFileReport : IDisposable
//    {
//        #region Const string
//        private const string SITES = "Sites";
//        private const string LISTS = "Lists";
//        private const string DESTROYED = "Destroyed";


//        private const string PHYSICAL_LIBRARY_NAME_KEY = "RevIMHoldPhysicalLibraryName";

//        private const string CONTENT_TYPE_DOCUMENT_NAME = "Document";
//        private const string CONTENT_TYPE_PHYSICAL_RECORD_NAME = "Physical Record";
//        private const string CONTENT_TYPE_PHYSICAL_FILE_NAME = "Physical File";
//        private const string CONTENT_TYPE_OfficeDataConnectionFile_NAME = "Office Data Connection File";

//        private const string SP_FIELD_NAME_NAME = "FileLeafRef";
//        private const string SP_DESTROYED_TIME_NAME = "Destroyed Time";
//        private const string SP_FIELD_MODIFIED_NAME = "Modified";
//        private const string SP_FIELD_CREATED_NAME = "Created";
//        private const string SP_FIELD_MODIFIED_BY_NAME = "Editor";
//        private const string SP_FIELD_CREATED_BY_NAME = "Author";
//        private const string SP_FIELD_HOME_LOCATION_NAME_KEY = "RevIMHomeLocationName";
//        private const string SP_FIELD_BOX_NAME_KEY = "RevIMBoxName";
//        private const string SP_FIELD_LIFECYCLE_STATUS_KEY = "RevIMLifecycleStatusName";
//        private const string SP_FIELD_AVAILABILITY_KEY = "RevIMAvailabilityName";
//        private const string SP_FIELD_CURRENTLY_HELD_BY_KEY = "RevIMCurrentlyHeldByName";

//        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";
//        private const string ARCHIVER_XML_NODE_NAME = "Name";
//        private const string ARCHIVER_XML_NODE_VALUE = "Value";
//        private const string ARCHIVER_XML_NODE_CONTENT_TYPE = "content type";
//        private const string ARCHIVER_XML_NODE_MODIFIED = "modified";
//        private const string ARCHIVER_XML_NODE_MODIFIED_BY = "modified by";
//        private const string ARCHIVER_XML_NODE_LIFECYCLE_STATUS = "lifecycle status";
//        private const string ARCHIVER_XML_NODE_BOX = "box";
//        private const string ARCHIVER_XML_NODE_AVAILABILITY = "availability";
//        private const string ARCHIVER_XML_NODE_CURRENTLY_HELD_BY = "currently held by";
//        private const string ARCHIVER_XML_NODE_EXTEND_VALUE = "ExtendValue";
//        private const string STATIC_BCS_COLUMN_NAME = "RevIMBCS";
//        private const string TIME_FORMAT = "yyyy-MM-dd HH:mm";
//        #endregion

//        #region Field
//        private static RALogger logger = RALogger.GetInstance(typeof(RMCreateAndDestroyedFileReport));

//        private Dictionary<Guid, Guid> idMappings = new Dictionary<Guid, Guid>();
//        private List<string> reportedListIds = null;
//        private List<Guid> physicalLibraryIds = null;
//        private ISPSettingTreeService spSettingTreeService = null;
//        private IArchiverTableDao mArchiverTableDao = null;
//        private DAOAPIClientV1 mDocAveClient = null;
//        private AzureTableConnectContract mAzureTableConnectInfo = null;
//        private string mTenantGroupId = TenantLocalValue.LogonGroupId;
//        private bool onlyGetListCount = false;
//        private int listCount = 0;
//        private JobResult result = null;
//        private RMCreationJobMessage msg = null;
//        private DateTime startUtcTime;
//        private DateTime endUtcTime;
//        private string physicalLibraryName = string.Empty;
//        private string homeLocationColumnName = string.Empty;
//        private string boxColumnName = string.Empty;
//        private string bcColumnName = string.Empty;
//        private string currentlyHeldByColumnName = string.Empty;
//        private string availabilityColumnName = string.Empty;
//        private string lifecycleStatusColumnName = string.Empty;
//        private string commomErrorMessage = string.Empty;
//        private DBUtility mdbUtility = null;
//        private List<RMSharePointSetting> physicalSettings = null;
//        private RMSPTreeNode farmTreeNode = null;
//        private IAveSite mBufferSite = null;
//        private IAveWeb mBufferWeb = null;
//        private Guid mCurrentSiteNodeId;    //not sp object id

//        protected IRMReportManager mReportManager;
//        protected IRMReportManager ReportManager
//        {
//            get
//            {
//                if (mReportManager == null)
//                {
//                    mReportManager = ReportMangerFactory.Instance.ReportManager;
//                }
//                return mReportManager;
//            }
//        }
//        private IAveSite bufferSite
//        {
//            get
//            {
//                return mBufferSite;
//            }
//            set
//            {
//                if (mBufferSite != null)
//                {
//                    mBufferSite.Dispose();
//                }
//                mBufferSite = value;
//            }
//        }
//        private IAveWeb bufferWeb
//        {
//            get
//            {
//                return mBufferWeb;
//            }
//            set
//            {
//                if (mBufferWeb != null)
//                {
//                    mBufferWeb.Dispose();
//                }
//                mBufferWeb = value;
//            }
//        }

//        #endregion

//        public RMCreateAndDestroyedFileReport(RMCreationJobMessage msg)
//        {
//            logger.Info(msg.ToString());
//            commomErrorMessage = I18NEntity.GetString("RM_TS_SS_Summary");
//            this.msg = msg;
//            this.msg.EndTime = this.msg.EndTime.AddDays(1);//包含当天
//            var globalTimeZone = TimeZoneInfo.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
//            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
//            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
//            homeLocationColumnName = Util.GetAppSettingValue(SP_FIELD_HOME_LOCATION_NAME_KEY);
//            physicalLibraryName = Util.GetAppSettingValue(PHYSICAL_LIBRARY_NAME_KEY);
//            boxColumnName = Util.GetAppSettingValue(SP_FIELD_BOX_NAME_KEY);
//            lifecycleStatusColumnName = Util.GetAppSettingValue(SP_FIELD_LIFECYCLE_STATUS_KEY);
//            availabilityColumnName = Util.GetAppSettingValue(SP_FIELD_AVAILABILITY_KEY);
//            currentlyHeldByColumnName = Util.GetAppSettingValue(SP_FIELD_CURRENTLY_HELD_BY_KEY);

//            spSettingTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
//            mArchiverTableDao = (IArchiverTableDao)PlatformWindsorManager.GetService(typeof(IArchiverTableDao));

//            result = new JobResult();
//            reportedListIds = new List<string>();
//            physicalLibraryIds = new List<Guid>();
//            ReportMangerFactory.Instance.Init(msg.JobID, msg.JobType, true);
//            //reportManager = new RAReportManager(msg.JobID, msg.JobType, true);
//            //reportManager.BaseJobDto = new BaseJobDto() { Id = msg.JobID, JobType = (int)msg.JobType };
//            //reportManager.DetailBufferCount = 5;
//            //reportManager.ReportBufferCount = 100;

//            mDocAveClient = new DAOAPIClientV1();
//            mAzureTableConnectInfo = mDocAveClient.GetArchiverDataBaseConfig();
//        }

//        public void Run()
//        {
//            JobStatus status = JobStatus.None;
//            try
//            {
//                Initialization();

//                #region run job
//                SharePointSettingUtility spUtility = new SharePointSettingUtility();
//                physicalSettings = physicalSettings ?? spUtility.GetAllPhysicalSiteSettings();
//                onlyGetListCount = false;

//                #region SelectPhysical
//                try
//                {
//                    foreach (RMSharePointSetting spSetting in physicalSettings)
//                    {
//                        using (CheckJobStopScope jScope = new CheckJobStopScope())
//                        {
//                            try
//                            {
//                                mCurrentSiteNodeId = spSetting.SiteId;
//                                var node = spUtility.GetRemoteSiteCollection(mCurrentSiteNodeId.ToString());
//                                if (node == null)
//                                {
//                                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
//                                    detail.Status = JobDetailsStatus.Skipped;
//                                    detail.URL = spSetting.FullPath;
//                                    detail.Comment = I18NEntity.GetString("RM_SS_SiteRemovedFromDAO");
//                                    ReportManager.SendJobDetail(detail);
//                                    logger.Warn("An error occurred when get sitecollection by id from docave failed,siteId:{0}, path:{1}", mCurrentSiteNodeId, spSetting.FullPath);
//                                    //throw new Exception("Get tree node failed.");
//                                    continue;
//                                }

//                                if (bufferSite == null || !Guid.Equals(new Guid(node.id), bufferSite.ID))
//                                {
//                                    var mfactory = AveObjectModelFactory.CreateObjectModelFactory(node.url, PoolUserUtil.GetBPOSInfo(node), AveContextKind.ClientObjectModel);
//                                    bufferSite = mfactory.CreateSite(node.url);
//                                }
//                                if (!idMappings.Keys.Contains(bufferSite.ID))
//                                {
//                                    idMappings.Add(bufferSite.ID, new Guid(node.id));
//                                    //idMappings.Add(bufferSite.ID, new Guid(node.parentId));
//                                }

//                                if (spSetting.WebId == null || spSetting.WebId.Equals(Guid.Empty))
//                                {
//                                    bufferWeb = bufferSite.RootWeb;
//                                }
//                                else
//                                {
//                                    bufferWeb = bufferSite.OpenWeb(spSetting.WebId);
//                                }

//                                IAveList list = (spSetting.ListId == null || spSetting.ListId.Equals(Guid.Empty))
//                                      ? bufferWeb.GetListByName(physicalLibraryName, false)
//                                      : bufferWeb.GetList(spSetting.ListId);

//                                if (!physicalLibraryIds.Contains(list.ID))
//                                {
//                                    physicalLibraryIds.Add(list.ID);
//                                }

//                                if (msg.SelectPhysical)
//                                {
//                                    ReportList(list);
//                                }
//                            }
//                            catch (JobStopException ex)
//                            {
//                                throw new JobStopException("This Job is stopped.");
//                            }
//                            catch (Exception e)
//                            {
//                                logger.Warn("Get physical list failed.Setting Id:[{0}],Error:{1}", spSetting.Id, e);
//                            }
//                        }

//                    }
//                }
//                catch (JobStopException ex)
//                {
//                    throw new JobStopException("This Job is stopped.");
//                }
//                catch (Exception e)
//                {
//                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
//                    detail.Status = JobDetailsStatus.Failed;
//                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
//                    ReportManager.SendJobDetail(detail);

//                    result.HasFailed = true;
//                    logger.Error("Initialization SelectPhysical Error:{0}", e);
//                }
//                #endregion

//                if (msg.SelectElectronic)
//                {
//                    #region SelectElectronic
//                    try
//                    {
//                        if (farmTreeNode == null)
//                        {
//                            throw new Exception("Can not get sharepoint farm tree node.");
//                        }

//                        NodeItem farmNode = new NodeItem(farmTreeNode);
//                        Process(farmNode);
//                    }
//                    catch (JobStopException ex)
//                    {
//                        throw new JobStopException("This Job is stopped.");
//                    }
//                    catch (Exception e)
//                    {
//                        JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
//                        detail.Status = JobDetailsStatus.Failed;
//                        detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
//                        ReportManager.SendJobDetail(detail);

//                        result.HasFailed = true;
//                        logger.Error("Initialization SelectElectronic Error:{0}", e);
//                    }
//                    #endregion
//                }
//                #endregion

//                status = result.HasFailed
//                    ? result.HasSuccessful
//                        ? JobStatus.FinishWithException
//                        : JobStatus.Failed
//                    : JobStatus.Finished;
//            }
//            catch (JobStopException ex)
//            {
//                status = JobStatus.Stopped;
//            }
//            catch (Exception)
//            {
//                status = JobStatus.Failed;
//                throw;
//            }
//            finally
//            {
//                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
//                    ? commomErrorMessage
//                    : string.Empty;
//                ReportManager.SetJobFinished(status, jobComment);
//            }
//        }

//        public void Dispose()
//        {
//            if (mdbUtility != null)
//            {
//                mdbUtility.Dispose();
//            }

//            if (bufferWeb != null)
//            {
//                bufferWeb.Dispose();
//            }

//            if (bufferSite != null)
//            {
//                bufferSite.Dispose();
//            }
//        }

//        private void Initialization()
//        {
//            //ReportManager.Increase(1);
//            ReportManager.StartUpdateJobProgress();
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                #region get total count
//                onlyGetListCount = true;
//                if (msg.SelectPhysical)
//                {
//                    SharePointSettingUtility spUtility = new SharePointSettingUtility();
//                    physicalSettings = spUtility.GetAllPhysicalSiteSettings();
//                    ReportManager.IncreaseBase(physicalSettings.Count);
//                }

//                if (msg.SelectElectronic)
//                {
//                    ReportServiceUtility rsUtility = new ReportServiceUtility();
//                    farmTreeNode = rsUtility.GetSPFarmTreeNode(msg.ProfileId);
//                    if (farmTreeNode == null)
//                    {
//                        throw new Exception("Can not get sharepoint farm tree node.");
//                    }
//                    NodeItem farmNode = new NodeItem(farmTreeNode);
//                    Process(farmNode);
//                    ReportManager.IncreaseBase(listCount);
//                }
//                #endregion
//                //ReportManager.Increase(2);
//            }
            
//        }

//        private void ReportList(IAveList list)
//        {
//            TermUtility homeLocationTermUtility = null;
//            TermUtility bcTermUtility = null;
//            bool isPhysicalLibrary = physicalLibraryIds.Contains(list.ID);
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                if (!msg.SelectPhysical && isPhysicalLibrary)
//                {
//                    return;
//                }
//                try
//                {
//                    if (reportedListIds.Contains(list.ParentWeb.Site.ID.ToString() + list.ID.ToString())
//                        || list.Hidden
//                        || (list.BaseType != AveBaseType.DocumentLibrary && list.BaseTemplate != AveListTemplateType.PictureLibrary)
//                        || CommonUtility.CheckIsDesignList(list.RootFolder.Name + (int)list.BaseTemplate)
//                        )
//                    {
//                        return;
//                    }

//                    try
//                    {
//                        SharePointSettingUtility spsUtility = new SharePointSettingUtility();

//                        Guid nodeWebId = Guid.Empty;
//                        Guid nodeSiteId = Guid.Empty;
//                        idMappings.TryGetValue(list.ParentWeb.ID, out nodeWebId);
//                        idMappings.TryGetValue(list.ParentWeb.Site.ID, out nodeSiteId);

//                        List<Guid> ids = new List<Guid>();
//                        ids.Add(nodeSiteId);
//                        logger.Info("nodeSiteId:[{0}]", nodeSiteId);
//                        logger.Info(idMappings.Keys.Count + "");


//                        bcColumnName = spsUtility.GetMedataColumn(ids);
//                        logger.Info("bcColumnName:[{0}]", bcColumnName);
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Warn("Get medata column name failed.Error:{0}", e);
//                    }

//                    try
//                    {
//                        if (!string.IsNullOrEmpty(homeLocationColumnName))
//                        {
//                            homeLocationTermUtility = new TermUtility(list, homeLocationColumnName);
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Warn(e.ToString());
//                    }

//                    try
//                    {
//                        if (!string.IsNullOrEmpty(bcColumnName))
//                        {
//                            bcTermUtility = new TermUtility(list, bcColumnName);
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Warn(e.ToString());
//                    }

//                    if (msg.SelectCreated)
//                    {
//                        BuildCreatedReport(list, homeLocationTermUtility, bcTermUtility);
//                    }
//                    if (msg.SelectDestroyed)
//                    {
//                        BuildDestroyedReport(list, homeLocationTermUtility, bcTermUtility, isPhysicalLibrary);
//                    }
//                }
//                catch (Exception e)
//                {
//                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
//                    detail.Title = string.Format("Web Url:[{0}],List title:[{1}]", list.ParentWeb.Url, list.Title);
//                    detail.Status = JobDetailsStatus.Failed;
//                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
//                    ReportManager.SendJobDetail(detail);

//                    result.HasFailed = true;
//                    logger.Error("Report list failed,web url:[{0}]list title:[{1}],error:{2}", list.ParentWeb.Url, list.Title, e);
//                }
//                finally
//                {
//                    string value = list.ParentWeb.Site.ID.ToString() + list.ID.ToString();
//                    if (!reportedListIds.Contains(value))
//                    {
//                        reportedListIds.Add(value);
//                    }
//                    ReportManager.Increase();
//                }
//            }
//        }

//        /// <summary>
//        /// 获取每次最多可以操作多少条记录。
//        /// *以后需要让Wrapper在AgentCommonObjectModelCommon.dll的IAveSite中提供一个获取MaxItemsPerThrottledOperation的方式*
//        /// </summary>
//        /// <param name="discoverSite">IAveSite</param>
//        /// <returns>MaxItemsPerThrottledOperation: 每次最多可以操作多少条记录</returns>
//        private int GetMaxItemsPerThrottledOperation(IAveSite discoverSite)
//        {
//            int maxItemsPer = 5000;
//            try
//            {
//                var dataCacheType = discoverSite.GetType().GetProperty("DataCache");
//                var dataCacheObj = dataCacheType.GetValue(discoverSite);
//                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache");
//                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
//                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
//                object maxItemsPerObj;
//                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
//                {
//                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
//            }
//            return maxItemsPer;
//        }

//        private void BuildCreatedReport(IAveList list, TermUtility homeLocationTermUtility, TermUtility bcTermUtility)
//        {
//            int rowLimit = GetMaxItemsPerThrottledOperation(list.ParentWeb.Site);

//            AveCamlQuery query = new AveCamlQuery();
//            query.DatesInUtc = true;
//            query.ViewXml = string.Format(
//"<View Scope=\"Recursive\">" +
//"<Query>" +
//"<Where>" +
//"<And>" +
//"<Gt>" +
//"<FieldRef Name=\"Created\"/><Value Type=\"DateTime\" IncludeTimeValue=\"TRUE\" StorageTZ=\"TRUE\">{0}</Value>" +
//"</Gt>" +
//"<Lt>" +
//"<FieldRef Name=\"Created\"/><Value Type=\"DateTime\" IncludeTimeValue=\"TRUE\" StorageTZ=\"TRUE\">{1}</Value>" +
//"</Lt>" +
//"</And>" +
//"</Where>" +
//"</Query>" +
//"<RowLimit>{2}</RowLimit>" +
//"</View>"
//, CreateISO8601DateTimeFromSystemDateTime(startUtcTime)
//, CreateISO8601DateTimeFromSystemDateTime(endUtcTime)
//, rowLimit
//);

//            logger.Info("Query XML: {0}", query.QueryXml);

//            IAveListItemCollection items = null;
//            AveItemCollectionPosition position = null;
//            bool firstTime = true;
//            bool needManualCheckRule = false;

//            do
//            {
//                query.ListItemCollectionPosition = position;
//                try
//                {
//                    items = list.GetItems(query);
//                }
//                catch (Exception ex)
//                {
//                    string exceptionType = string.Empty;
//                    if (firstTime && ex.InnerException != null)
//                    {
//                        var exType = ex.InnerException.GetType();
//                        if (exType.FullName == "Microsoft.SharePoint.Client.ServerException")
//                        {
//                            var obj = exType.InvokeMember("ServerErrorTypeName", System.Reflection.BindingFlags.GetProperty, null, ex.InnerException, new object[] { });
//                            exceptionType = obj.ToString();
//                        }
//                    }

//                    if (exceptionType.Equals("Microsoft.SharePoint.SPQueryThrottledException", StringComparison.OrdinalIgnoreCase))
//                    {
//                        logger.Warn("The number of items in this list exceeds the list view threshold, which is {0} items. So query all listitems and manual check rule. Error: {1}.",
//                            rowLimit.ToString(), ex.ToString());

//                        query.ViewXml = string.Format("<View Scope=\"Recursive\"><RowLimit>{0}</RowLimit></View>", rowLimit);
//                        needManualCheckRule = true;
//                        items = list.GetItems(query);
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                firstTime = false;
//                //position = new AveItemCollectionPosition() { PagingInfo = items.ListItemCollectionPosition?.PagingInfo };
//                position = items.ListItemCollectionPosition == null ? null : new AveItemCollectionPosition() { PagingInfo = items.ListItemCollectionPosition.PagingInfo };
//                foreach (IAveListItem item in items)
//                {
//                    if (!needManualCheckRule || CheckItemForThrottledList(item))
//                    {
//                        SendJobDetail(item, homeLocationTermUtility, bcTermUtility, OperationType.Created);
//                    }
//                }
//            }
//            while (position != null);
//        }

//        private bool CheckItemForThrottledList(IAveListItem item)
//        {
//            var created = GetDateTimeFieldValue(item, SP_FIELD_CREATED_NAME);
//            var createdTicks = created.Ticks;
//            return startUtcTime.Ticks < createdTicks && createdTicks < endUtcTime.Ticks;
//        }

//        private void BuildDestroyedReport(IAveList list, TermUtility homeLocationTermUtility, TermUtility bcTermUtility, bool isPhysicalLibrary)
//        {
//            //bool hasDestroyedData = false;

//            //for REC-2037
//            if (isPhysicalLibrary)
//            {
//                #region physical library
//                int rowLimit = GetMaxItemsPerThrottledOperation(list.ParentWeb.Site);
//                AveCamlQuery query = new AveCamlQuery();
//                query.DatesInUtc = true;
//                query.ViewXml = string.Format("<View Scope=\"RecursiveAll\"><RowLimit>{0}</RowLimit></View>", rowLimit);
//                IAveListItemCollection items = null;
//                AveItemCollectionPosition position = null;
//                do
//                {
//                    query.ListItemCollectionPosition = position;
//                    items = list.GetItems(query);
//                    //position = new AveItemCollectionPosition() { PagingInfo = items.ListItemCollectionPosition?.PagingInfo };
//                    position = items.ListItemCollectionPosition == null ? null : new AveItemCollectionPosition() { PagingInfo = items.ListItemCollectionPosition.PagingInfo };
//                    foreach (IAveListItem item in items)
//                    {
//                        try
//                        {
//                            object lifecycleStatus = item[lifecycleStatusColumnName];
//                            if (lifecycleStatus != null && string.Equals(DESTROYED, lifecycleStatus.ToString(), StringComparison.OrdinalIgnoreCase))
//                            {
//                                object destroyedTime = item[SP_DESTROYED_TIME_NAME];
//                                if (destroyedTime == null)
//                                {
//                                    throw new Exception("Destroyed time column is null");
//                                }
//                                else
//                                {
//                                    DateTime dtDestroyedTime = (DateTime)destroyedTime;
//                                    if (dtDestroyedTime > startUtcTime && dtDestroyedTime < endUtcTime)
//                                    {
//                                        //hasDestroyedData = true;
//                                        SendJobDetail(item, homeLocationTermUtility, bcTermUtility, OperationType.Destroyed);
//                                    }
//                                    else
//                                    {
//                                        logger.Debug("Destroyed time not match,destroyed time:[{0}]", dtDestroyedTime.ToString());
//                                    }
//                                }
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            logger.Warn("Report destroyed item failed,error:{0}", e.ToString());
//                        }
//                    }
//                }
//                while (position != null);
//                #endregion
//            }

//            #region other
//            List<ArchiverTableEntity> infos = null;
//            try
//            {
//                var listUrl = list.ParentWeb.Url + "/" + list.RootFolder.Url;
//                var listPath = listUrl.Replace("/", "_");
//                infos = mArchiverTableDao.GetDestroyedItemsByListId(mAzureTableConnectInfo, mTenantGroupId, list.ParentWeb.Site.ID.ToString(), list.ID, startUtcTime, endUtcTime, isPhysicalLibrary);
//            }
//            catch (AzureTableNotExistException ex)
//            {
//                logger.Error("Failed to retrieve the archived data that meets the rule settings. table not exist, List Title:[{0}],error:{1}", list.Title, ex.ToString());
//                commomErrorMessage = I18NEntity.GetString("RM_DAM_NoTable");
//                throw;
//            }
//            catch (SqlException se)
//            {
//                //REC-2281
//                logger.Warn("Get destroyed items failed,error:{0}", se.ToString());
//            }
//            List<Guid> destroyedNodeIds = new List<Guid>();
//            foreach (ArchiverTableEntity info in infos)
//            {
//                //去除重复项
//                if (destroyedNodeIds.Contains(info.NodeID))
//                {
//                    continue;
//                }

//                JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
//                try
//                {
//                    var asd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(info.JsonMeta);
//                    detail.URL = list.ParentWeb.Site.Url.Replace(list.ParentWeb.Site.ServerRelativeUrl, "").TrimEnd('/') + "/" + asd.Path.Replace('\\', '/').TrimStart('/');
//                    //detail.URL = list.ParentWeb.Site.Url.TrimEnd('/') + "/" + info.Path.Replace('\\', '/').TrimStart('/');
//                    detail.Title = asd.LeafName;
//                    detail.Operation = (int)OperationType.Destroyed;
//                    detail.OperationTime = asd.ArchivedTime.Ticks.ToString();
//                    detail.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
//                    if (isPhysicalLibrary && string.IsNullOrEmpty(asd.Metadata))
//                    {
//                        //config "DisplayColumns" node in "AgentCommonStorageEnv.cfg". Fields info will be not null after archived.
//                        logger.Warn("Fields info is null,Url:[{0}]", detail.URL);
//                        continue;
//                    }
//                    else if (!string.IsNullOrEmpty(asd.Metadata))
//                    {
//                        #region Get metadata
//                        ObjectLevel oLevel = ObjectLevel.None;
//                        logger.Info("LeafName:[{0}],FieldsInfo:[{1}]", asd.LeafName, asd.Metadata);
//                        XmlDocument doc = new XmlDocument();
//                        doc.LoadXml(asd.Metadata);
//                        XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
//                        foreach (XmlNode node in root.ChildNodes)
//                        {
//                            string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
//                            string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
//                            string termId = string.Empty;
//                            switch (fieldName)
//                            {
//                                case ARCHIVER_XML_NODE_CONTENT_TYPE:
//                                    //for PhysicalLibrary archived data: only PhysicalFile need report
//                                    oLevel = GetObjectLevelByContentType(fieldValue);
//                                    if (isPhysicalLibrary && oLevel != ObjectLevel.PhysicalFile)
//                                    {
//                                        continue;
//                                    }
//                                    break;
//                                //case ARCHIVER_XML_NODE_MODIFIED://OperationTime
//                                //    detail.OperationTime = TimeZone.CurrentTimeZone.ToLocalTime(Convert.ToDateTime(fieldValue)).ToString(TIME_FORMAT) + TimeZoneInfo.Local.ToString();
//                                //    break;
//                                //case ARCHIVER_XML_NODE_MODIFIED_BY://OperationBy
//                                //    //detail.OperationBy = fieldValue;
//                                //    break;
//                                case ARCHIVER_XML_NODE_LIFECYCLE_STATUS:
//                                    detail.LifecycleStatus = fieldValue;
//                                    break;
//                                case ARCHIVER_XML_NODE_BOX:
//                                    detail.Box = fieldValue;
//                                    break;
//                                case ARCHIVER_XML_NODE_AVAILABILITY:
//                                    detail.Availablity = fieldValue;
//                                    break;
//                                case ARCHIVER_XML_NODE_CURRENTLY_HELD_BY:
//                                    detail.CurrentHeldBy = fieldValue;
//                                    break;
//                                default:
//                                    if (string.Equals(fieldName, bcColumnName, StringComparison.OrdinalIgnoreCase) || string.Equals(fieldName, STATIC_BCS_COLUMN_NAME, StringComparison.OrdinalIgnoreCase))
//                                    {
//                                        if (bcTermUtility != null)
//                                        {
//                                            if (node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null)
//                                            {
//                                                string bcStr = node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value;
//                                                if (!string.IsNullOrEmpty(bcStr))
//                                                {
//                                                    Guid bcTermId = new Guid(bcStr.Split('|')[1]);
//                                                    detail.TermName = bcTermUtility.GetTermPath(bcTermId);
//                                                }
//                                            }
//                                        }
//                                    }
//                                    else if (string.Equals(fieldName, homeLocationColumnName, StringComparison.OrdinalIgnoreCase))
//                                    {
//                                        if (homeLocationTermUtility != null)
//                                        {
//                                            if (node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null)
//                                            {
//                                                string homeLocationStr = node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value;
//                                                if (!string.IsNullOrEmpty(homeLocationStr))
//                                                {
//                                                    Guid homeLocationTermId = new Guid(homeLocationStr.Split('|')[1]);
//                                                    detail.HomeLocation = homeLocationTermUtility.GetTermPath(homeLocationTermId);
//                                                }
//                                            }
//                                        }
//                                    }
//                                    break;
//                            }
//                        }

//                        if (!isPhysicalLibrary)
//                        {
//                            oLevel = ObjectLevel.Document;
//                        }
//                        else if (oLevel != ObjectLevel.PhysicalFile)
//                        {
//                            continue;
//                        }
//                        detail.ObjectLevel = oLevel.ToString();
//                        #endregion
//                    }
//                    else
//                    {
//                        detail.ObjectLevel = ObjectLevel.Document.ToString();
//                    }

//                    detail.Status = JobDetailsStatus.Successful;
//                    result.HasSuccessful = true;
//                    destroyedNodeIds.Add(info.NodeID);
//                    ReportManager.SendJobDetail(detail);
//                    ReportManager.SendJobReport(ConvertToReport(detail));
//                }
//                catch (Exception e)
//                {
//                    detail.Status = JobDetailsStatus.Failed;
//                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
//                    ReportManager.SendJobDetail(detail);

//                    result.HasFailed = true;
//                    logger.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
//                }
//            }
//            #endregion
//        }

//        private void SendJobDetail(IAveListItem item, TermUtility homeLocationTermUtility, TermUtility bcTermUtility, OperationType operationType)
//        {
//            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();

//            try
//            {
//                var oLevel = GetObjectLevelByContentType(item.ContentType.Name);
//                if (oLevel == ObjectLevel.PhysicalRecord || oLevel == ObjectLevel.None)
//                {
//                    return;
//                }
//                detail.ObjectLevel = oLevel.ToString();
//                detail.Title = GetFieldValue(item, SP_FIELD_NAME_NAME);
//                DateTime operationTimeDt = DateTime.MinValue;
//                string operationByStr = string.Empty;
//                switch (operationType)
//                {
//                    case OperationType.Created:
//                        operationTimeDt = GetDateTimeFieldValue(item, SP_FIELD_CREATED_NAME);
//                        operationByStr = GetFieldValue(item, SP_FIELD_CREATED_BY_NAME);
//                        break;
//                    case OperationType.Destroyed:
//                        operationTimeDt = GetDateTimeFieldValue(item, SP_DESTROYED_TIME_NAME);
//                        operationByStr = GetFieldValue(item, SP_FIELD_MODIFIED_BY_NAME);
//                        break;
//                }
//                detail.OperationTime = operationTimeDt.Equals(DateTime.MinValue) ? string.Empty : operationTimeDt.Ticks.ToString();
//                if (string.IsNullOrEmpty(operationByStr))
//                {
//                    detail.OperationBy = operationByStr;
//                }
//                else
//                {
//                    string[] sArray = operationByStr.Split('#');
//                    if (sArray.Length > 1)
//                    {
//                        detail.OperationBy = sArray[1];
//                    }
//                    else
//                    {
//                        detail.OperationBy = sArray[0];
//                    }
//                }


//                detail.URL = item.ParentList.ParentWeb.Url.TrimEnd('/') + "/" + item.Url.TrimStart('/');
//                detail.LifecycleStatus = GetFieldValue(item, lifecycleStatusColumnName);
//                detail.Box = GetFieldValue(item, boxColumnName);
//                detail.Availablity = GetFieldValue(item, availabilityColumnName);
//                string currentHeldByStr = GetFieldValue(item, currentlyHeldByColumnName);
//                detail.CurrentHeldBy = string.IsNullOrEmpty(currentHeldByStr) ? currentHeldByStr : currentHeldByStr.Split('#')[1];
//                detail.Operation = (int)operationType;

//                if (homeLocationTermUtility != null)
//                {
//                    string homeLocationStr = GetFieldValue(item, homeLocationColumnName);
//                    if (!string.IsNullOrEmpty(homeLocationStr))
//                    {
//                        Guid homeLocationTermId = new Guid(homeLocationStr.Split('|')[1]);
//                        detail.HomeLocation = homeLocationTermUtility.GetTermPath(homeLocationTermId);
//                    }
//                }

//                if (bcTermUtility != null)
//                {
//                    string bcStr = GetFieldValue(item, bcColumnName);
//                    if (!string.IsNullOrEmpty(bcStr))
//                    {
//                        Guid bcTermId = new Guid(bcStr.Split('|')[1]);
//                        try
//                        {
//                            detail.TermName = bcTermUtility.GetTermPath(bcTermId);
//                        }
//                        catch (Exception ex)
//                        {
//                            detail.TermName = "";
//                            logger.Warn("Get term from term store failed. item url: {0}, message:{1}", detail.URL, ex.Message);
//                        }
//                    }
//                }

//                detail.Status = JobDetailsStatus.Successful;
//                result.HasSuccessful = true;
//                if (!(operationType == OperationType.Created && string.Equals(DESTROYED, detail.LifecycleStatus, StringComparison.OrdinalIgnoreCase))
//                    && !string.Equals(detail.ObjectLevel, ObjectLevel.PhysicalRecord.ToString(), StringComparison.OrdinalIgnoreCase)
//                    && !string.IsNullOrEmpty(detail.ObjectLevel))
//                //if (!(operationType == OperationType.Created && string.Equals(DESTROYED, detail.LifecycleStatus, StringComparison.OrdinalIgnoreCase)))
//                {
//                    ReportManager.SendJobDetail(detail);
//                    ReportManager.SendJobReport(ConvertToReport(detail));
//                }
//            }
//            catch (Exception e)
//            {
//                detail.Status = JobDetailsStatus.Failed;
//                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
//                ReportManager.SendJobDetail(detail);

//                result.HasFailed = true;
//                logger.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
//            }
//        }

//        private ObjectLevel GetObjectLevelByContentType(string contentTypeName)
//        {
//            ObjectLevel level = ObjectLevel.Document;
//            switch (contentTypeName)
//            {
//                case CONTENT_TYPE_DOCUMENT_NAME:
//                case CONTENT_TYPE_OfficeDataConnectionFile_NAME:
//                    level = ObjectLevel.Document;
//                    break;
//                case CONTENT_TYPE_PHYSICAL_RECORD_NAME:
//                    level = ObjectLevel.PhysicalRecord;
//                    break;
//                case CONTENT_TYPE_PHYSICAL_FILE_NAME:
//                    level = ObjectLevel.PhysicalFile;
//                    break;
//            }
//            return level;
//        }

//        private string GetFieldValue(IAveListItem item, string fieldName)
//        {
//            try
//            {
//                if (item[fieldName] == null)
//                {
//                    return string.Empty;
//                }
//                else
//                {
//                    return item[fieldName].ToString();
//                }

//            }
//            catch (Exception)
//            {
//                logger.Warn("Get field value failed.Field name:[{0}]", fieldName);
//                return string.Empty;
//            }
//        }

//        private DateTime GetDateTimeFieldValue(IAveListItem item, string fieldName)
//        {
//            try
//            {
//                if (item[fieldName] == null)
//                {
//                    return DateTime.MinValue;
//                }
//                else
//                {
//                    return Convert.ToDateTime(item[fieldName]);
//                }

//            }
//            catch (Exception)
//            {
//                logger.Warn("Get field value failed.Field name:[{0}]", fieldName);
//                return DateTime.MinValue;
//            }
//        }

//        private string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append(dtValue.Year.ToString("0000"));
//            stringBuilder.Append("-");
//            stringBuilder.Append(dtValue.Month.ToString("00"));
//            stringBuilder.Append("-");
//            stringBuilder.Append(dtValue.Day.ToString("00"));
//            stringBuilder.Append("T");
//            stringBuilder.Append(dtValue.Hour.ToString("00"));
//            stringBuilder.Append(":");
//            stringBuilder.Append(dtValue.Minute.ToString("00"));
//            stringBuilder.Append(":");
//            stringBuilder.Append(dtValue.Second.ToString("00"));
//            stringBuilder.Append("Z");
//            return stringBuilder.ToString();
//        }

//        private CreateAndDestroyedFileReport ConvertToReport(JMCreateAndDestroyedFileReportJobDetail detail)
//        {
//            CreateAndDestroyedFileReport report = new CreateAndDestroyedFileReport();
//            //PhysicalFile;Document
//            if (string.Equals(detail.ObjectLevel, "Document", StringComparison.OrdinalIgnoreCase))
//            {
//                report.LevelStr = (int)RMReportObjectLevel.Document;
//            }
//            else if (string.Equals(detail.ObjectLevel, "PhysicalFile", StringComparison.OrdinalIgnoreCase))
//            {
//                report.LevelStr = (int)RMReportObjectLevel.PhysicalFile;
//            }
//            //report.LevelStr = detail.ObjectLevel;
//            report.Title = detail.Title;
//            report.OperationTime = detail.OperationTime;
//            report.OperationBy = detail.OperationBy;
//            report.TermName = detail.TermName;

//            report.Url = detail.URL;
//            report.LifecycleStatus = detail.LifecycleStatus;
//            report.HomeLocation = detail.HomeLocation;
//            report.Box = detail.Box;
//            report.Availablity = detail.Availablity;

//            report.CurrentHeldBy = detail.CurrentHeldBy;
//            report.Operation = detail.Operation;
//            return report;
//        }

//        #region Processor
//        private void Process(NodeItem farmNode)
//        {
//            CheckNodeLevel(farmNode, NodeLevel.Farm);

//            List<RMSPTreeNode> webApps = spSettingTreeService.Browse(farmNode.TreeNode);
//            foreach (var webapp in webApps)
//            {
//                using (CheckJobStopScope jScope = new CheckJobStopScope())
//                {
//                    try
//                    {
//                        NodeItem tempApp;
//                        if (farmNode.Children.TryGetValue(new Guid(webapp.SPObjectId), out tempApp))
//                        {
//                            //保存的webapplication节点有子节点是勾选的 或者 webapplication节点被勾选且没有展开
//                            if (AreThereProcessedChildren(tempApp))
//                            {
//                                ProcessWebApp(tempApp);
//                            }
//                        }
//                        else if (farmNode.IncludeNew)
//                        {
//                            webapp.CheckNumber = 1;
//                            webapp.IncludeNew = 1;
//                            ProcessWebApp(new NodeItem(webapp, farmNode));
//                        }
//                    }
//                    catch (JobStopException ex)
//                    {
//                        throw new JobStopException("This Job is stopped.");
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Error("An error occurred while farm process. fullPath: [{0}], error message : {1}.", farmNode.FullPath, e);
//                    }
//                }
//            }
//        }

//        private void ProcessWebApp(NodeItem webappNode)
//        {
//            try
//            {
//                CheckNodeLevel(webappNode, NodeLevel.WebApplication);
//                logger.Info("Start web app process. fullPath: [{0}], isIncludeNew : [{1}].", webappNode.FullPath, webappNode.IncludeNew);

//                List<RMSPTreeNode> sites = spSettingTreeService.Browse(webappNode.TreeNode);
//                foreach (var site in sites)
//                {
//                    using (CheckJobStopScope jScope = new CheckJobStopScope())
//                    {
//                        //reportedListIds.Clear();
//                        NodeItem tempSite;
//                        if (webappNode.Children.TryGetValue(new Guid(site.Id), out tempSite))
//                        {
//                            if (AreThereProcessedChildren(tempSite))
//                            {
//                                ProcessSite(tempSite);
//                                if (!idMappings.Keys.Contains(bufferSite.ID))
//                                {
//                                    idMappings.Add(bufferSite.ID, webappNode.Id);
//                                }
//                            }
//                            else if (tempSite.IsChecked)
//                            {
//                                //遍历整个Site Collection
//                                if (bufferSite == null || !Guid.Equals(new Guid(site.Id), bufferSite.ID))
//                                {
//                                    var mfactory = AveObjectModelFactory.CreateObjectModelFactory(site.FullPath, PoolUserUtil.GetAveBPOSAccountInfo(site.BposInfo, site.FullPath), AveContextKind.ClientObjectModel);
//                                    bufferSite = mfactory.CreateSite(site.FullPath);
//                                }
//                                if (!idMappings.Keys.Contains(bufferSite.ID))
//                                {
//                                    idMappings.Add(bufferSite.ID, webappNode.Id);
//                                }

//                                GetSPListFromSPWeb(bufferSite.RootWeb);
//                            }
//                        }
//                        else if (webappNode.IncludeNew)
//                        {
//                            site.CheckNumber = 1;
//                            site.IncludeNew = 1;
//                            ProcessSite(new NodeItem(site, webappNode));
//                            if (!idMappings.Keys.Contains(bufferSite.ID))
//                            {
//                                idMappings.Add(bufferSite.ID, webappNode.Id);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (JobStopException ex)
//            {
//                throw new JobStopException("This Job is stopped.");
//            }
//            catch (Exception e)
//            {
//                logger.Error("An error occurred while prosess webapplication, fullPath is :{0}, error message: {1}.", webappNode.FullPath, e.ToString());
//            }
//        }

//        private void ProcessSite(NodeItem siteNode)
//        {
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                try
//                {
//                    CheckNodeLevel(siteNode, NodeLevel.SiteCollection);
//                    logger.Info("Start Site process. fullPath: [{0}], isIncludeNew : [{1}].", siteNode.FullPath, siteNode.IncludeNew);
//                    mCurrentSiteNodeId = siteNode.Id;
//                    if (bufferSite == null || !Guid.Equals(siteNode.Id, bufferSite.ID))
//                    {
//                        var mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, siteNode.FullPath), AveContextKind.ClientObjectModel);
//                        bufferSite = mfactory.CreateSite(siteNode.FullPath);
//                    }

//                    IAveWeb discoverWeb = bufferSite.RootWeb;
//                    siteNode.DiscoverObj = bufferSite;
//                    siteNode.NameOrTitle = discoverWeb.Title;
//                    NodeItem rootWebNode;

//                    //Sitecollection 节点有子节点是勾选的
//                    if (siteNode.HasCheckedChildren)
//                    {
//                        rootWebNode = siteNode.Children.Values[0];
//                        rootWebNode.DiscoverObj = discoverWeb;
//                        rootWebNode.NameOrTitle = discoverWeb.Title;
//                    }
//                    else  //if (site.IsChecked && site.Children.Count == 0)   Sitecollection 节点被勾选了，但是没有展开
//                    {
//                        rootWebNode = new NodeItem()
//                        {
//                            Id = discoverWeb.ID,
//                            NameOrTitle = discoverWeb.Title,
//                            DiscoverObj = discoverWeb,
//                            FullPath = siteNode.FullPath,
//                            NodeLevel = NodeLevel.Site,
//                            Parent = siteNode,
//                            IncludeNew = true,
//                            IsChecked = true
//                        };
//                    }

//                    ProcessWeb(rootWebNode);
//                }
//                catch (JobStopException ex)
//                {
//                    throw new JobStopException("This Job is stopped.");
//                }
//                catch (Exception e)
//                {
//                    logger.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", siteNode.FullPath, e.ToString());
//                }
//            }
//        }

//        private void ProcessWeb(NodeItem webNode)
//        {
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                try
//                {
//                    CheckNodeLevel(webNode, NodeLevel.Site);
//                    logger.Info("Start web process. fullPath: [{0}], isIncludeNew : [{1}].", webNode.FullPath, webNode.IncludeNew);
//                    if (webNode.Children.Count == 0)
//                    {
//                        //Lists节点
//                        var treeNodeLists = new NodeItem()
//                        {
//                            NodeLevel = NodeLevel.Lists,
//                            IncludeNew = true,
//                            IsChecked = true,
//                            FullPath = LISTS,
//                            NameOrTitle = LISTS,
//                            Parent = webNode,
//                            DiscoverObj = webNode.DiscoverObj
//                        };
//                        ProcessLists(treeNodeLists);

//                        //Sites节点
//                        var treeNodeSites = new NodeItem()
//                        {
//                            NodeLevel = NodeLevel.Sites,
//                            IncludeNew = true,
//                            IsChecked = true,
//                            FullPath = SITES,
//                            NameOrTitle = SITES,
//                            Parent = webNode,
//                            DiscoverObj = webNode.DiscoverObj
//                        };
//                        ProcessWebs(treeNodeSites);
//                    }
//                    else
//                    {
//                        foreach (var childNode in webNode.Children.Values.OrderBy(n => n.NodeLevel))
//                        {
//                            if (AreThereProcessedChildren(childNode))
//                            {
//                                childNode.DiscoverObj = webNode.DiscoverObj;
//                                switch (childNode.NodeLevel)
//                                {
//                                    case NodeLevel.Lists:
//                                        ProcessLists(childNode);
//                                        break;

//                                    case NodeLevel.Sites:
//                                        ProcessWebs(childNode);
//                                        break;
//                                }
//                            }
//                        }
//                    }
//                }
//                catch (JobStopException ex)
//                {
//                    throw new JobStopException("This Job is stopped.");
//                }
//                catch (Exception e)
//                {
//                    logger.Error("An error occurred while processing web: {0}, error message: {1}.", webNode.FullPath, e.ToString());
//                }
//            }
//        }

//        private void ProcessWebs(NodeItem sitesNode)
//        {
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                try
//                {
//                    CheckNodeLevel(sitesNode, NodeLevel.Sites);
//                    IAveWeb parentWeb = sitesNode.DiscoverObj as IAveWeb;
//                    NodeItem tempWebNode;
//                    foreach (var subWeb in parentWeb.Webs)
//                    {
//                        if (sitesNode.Children.TryGetValue(subWeb.ID, out tempWebNode))
//                        {
//                            tempWebNode.DiscoverObj = subWeb;
//                            if (AreThereProcessedChildren(tempWebNode))
//                            {
//                                ProcessWeb(tempWebNode);
//                            }
//                            else if (tempWebNode.IsChecked)
//                            {
//                                GetSPListFromSPWeb(subWeb);
//                            }
//                        }
//                        else if (sitesNode.IncludeNew)
//                        {
//                            tempWebNode = new NodeItem()
//                            {
//                                Id = subWeb.ID,
//                                NameOrTitle = subWeb.Name,
//                                DiscoverObj = subWeb,
//                                FullPath = subWeb.Url,
//                                NodeLevel = NodeLevel.Site,
//                                Parent = sitesNode,
//                                IncludeNew = true,
//                                IsChecked = true
//                            };
//                            ProcessWeb(tempWebNode);
//                        }
//                    }
//                }
//                catch (JobStopException ex)
//                {
//                    throw new JobStopException("This Job is stopped.");
//                }
//                catch (Exception e)
//                {
//                    logger.Error("An error occurred while processing sites level node, error message: {0}.", e.ToString());
//                }
//            }
//        }

//        private void ProcessLists(NodeItem listsNode)
//        {
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                try
//                {
//                    CheckNodeLevel(listsNode, NodeLevel.Lists);

//                    IAveWeb parentWeb = listsNode.DiscoverObj as IAveWeb;
//                    NodeItem tempListNode;

//                    StringBuilder sb = new StringBuilder();
//                    sb.Append("=================================================" + Environment.NewLine);
//                    sb.Append(parentWeb.Url + Environment.NewLine);
//                    foreach (IAveList discoverList in parentWeb.Lists)
//                    {
//                        sb.Append(discoverList.ID + Environment.NewLine);
//                    }
//                    sb.Append("=================================================" + Environment.NewLine);
//                    logger.Info(sb.ToString());

//                    foreach (IAveList discoverList in parentWeb.Lists)
//                    {
//                        logger.Info("list rootfolder url {0}", discoverList.RootFolder.Name);

//                        if (listsNode.Children.TryGetValue(discoverList.ID, out tempListNode) && tempListNode.IsChecked)
//                        {
//                            tempListNode.DiscoverObj = discoverList;
//                            if (tempListNode != null)
//                            {
//                                ProcessList(tempListNode);
//                            }
//                            else
//                            {
//                                logger.Warn("Temp list node is null.");
//                            }
//                        }
//                        else if (listsNode.IncludeNew)
//                        {
//                            if (!listsNode.Children.TryGetValue(discoverList.ID, out tempListNode))
//                            {
//                                tempListNode = new NodeItem()
//                                {
//                                    NodeLevel = NodeLevel.List,
//                                    Id = discoverList.ID,
//                                    NameOrTitle = discoverList.Title,
//                                    NodeType = discoverList.BaseType == AveBaseType.DocumentLibrary ? NodeType.DocumentLibrary : NodeType.GenericList,
//                                    DiscoverObj = discoverList,
//                                    Parent = listsNode,
//                                    IncludeNew = true,
//                                    IsChecked = true
//                                };
//                                if (tempListNode.NameOrTitle.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
//                                {
//                                    tempListNode.NodeType = NodeType.DocumentLibrary;
//                                }
//                            }
//                            if (tempListNode != null)
//                            {
//                                ProcessList(tempListNode);
//                            }
//                            else
//                            {
//                                logger.Warn("Temp list node is null.");
//                            }
//                        }
//                    }
//                }
//                catch (JobStopException ex)
//                {
//                    throw new JobStopException("This Job is stopped.");
//                }
//                catch (Exception e)
//                {
//                    logger.Error("An error occurred while processing lists level node, error message: {0}.", e.ToString());
//                }
//            }
//        }

//        private void ProcessList(NodeItem listNode)
//        {
//            using (CheckJobStopScope jScope = new CheckJobStopScope())
//            {
//                try
//                {
//                    CheckNodeLevel(listNode, NodeLevel.List);
//                    logger.Info("Start web process. fullPath: [{0}], isIncludeNew : [{1}].", listNode.FullPath, listNode.IncludeNew);
//                    if (onlyGetListCount)
//                    {
//                        listCount++;
//                    }
//                    else
//                    {
//                        RealReport(listNode);
//                        //GetSPListByNode(listNode);
//                    }
//                }
//                catch (Exception e)
//                {
//                    logger.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", listNode.FullPath, e.ToString());
//                }
//            }
//        }

//        private void RealReport(NodeItem listNode)
//        {
//            NodeItem siteNode = GetParentNode(listNode, NodeLevel.SiteCollection);
//            NodeItem webNode = GetParentNode(listNode, NodeLevel.Site);

//            if (bufferSite == null || !Guid.Equals(siteNode.Id, bufferSite.ID))
//            {
//                SharePointSettingUtility spUtility = new SharePointSettingUtility();
//                var mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, siteNode.FullPath), AveContextKind.ClientObjectModel);
//                bufferSite = mfactory.CreateSite(siteNode.FullPath);
//            }

//            if (webNode.Id == null || webNode.Id.Equals(Guid.Empty))
//            {
//                bufferWeb = bufferSite.RootWeb;
//            }
//            else
//            {
//                bufferWeb = bufferSite.OpenWeb(webNode.Id);
//            }

//            IAveList list = bufferWeb.GetList(listNode.Id);

//            ReportList(list);
//        }

//        private void GetSPListByNode(NodeItem listNode)
//        {
//            NodeItem siteNode = GetParentNode(listNode, NodeLevel.SiteCollection);
//            NodeItem webNode = GetParentNode(listNode, NodeLevel.Site);

//            if (bufferSite == null || !Guid.Equals(siteNode.Id, bufferSite.ID))
//            {
//                var mfactory = AveObjectModelFactory.CreateObjectModelFactory(siteNode.FullPath, PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, siteNode.FullPath), AveContextKind.ClientObjectModel);
//                bufferSite = mfactory.CreateSite(siteNode.FullPath);
//            }

//            if (webNode.Id == null || webNode.Id.Equals(Guid.Empty))
//            {
//                bufferWeb = bufferSite.RootWeb;
//            }
//            else
//            {
//                bufferWeb = bufferSite.OpenWeb(webNode.Id);
//            }

//            IAveList list = bufferWeb.GetList(listNode.Id);

//            ReportList(list);
//        }

//        private void GetSPListFromSPWeb(IAveWeb web)
//        {
//            //遍历整个Web
//            foreach (IAveList list in web.Lists)
//            {
//                ReportList(list);
//            }

//            foreach (IAveWeb subWeb in web.Webs)
//            {
//                GetSPListFromSPWeb(subWeb);
//            }
//        }

//        private NodeItem GetParentNode(NodeItem node, NodeLevel level)
//        {
//            if (node.NodeLevel == level)
//            {
//                return node;
//            }
//            else
//            {
//                return GetParentNode(node.Parent, level);
//            }
//        }

//        private void CheckNodeLevel(NodeItem node, NodeLevel expected)
//        {
//            if (!node.NodeLevel.Equals(expected))
//            {
//                throw new Exception(string.Format("Node expected level is {0}, but current node type is {1}. Node full path: {2}.", expected.ToString(), node.NodeLevel.ToString(), node.FullPath));
//            }
//        }

//        private bool AreThereProcessedChildren(NodeItem node)
//        {
//            return node.HasCheckedChildren || node.IncludeNew || (node.IsChecked && node.Children.Count == 0);
//        }
//        #endregion


//        public static bool TryGetOperationTimeUtcTicks(CreateAndDestroyedFileReport report, out long utcTimeTicks)
//        {
//            utcTimeTicks = 0;
//            int dtLength = 16;
//            //TIME_FORMAT.Length: 16
//            if (!string.IsNullOrEmpty(report.OperationTime) && report.OperationTime.Length > dtLength)
//            {
//                try
//                {
//                    DateTime dt = DateTimeUtil.ConvertStringToDateTime(report.OperationTime.Substring(0, dtLength), TIME_FORMAT);
//                    var zone = GetTimeZoneInfoByDisplayName(report.OperationTime.Substring(dtLength));
//                    if (zone != null)
//                    {
//                        dt = TimeZoneInfo.ConvertTimeToUtc(dt, zone);
//                        utcTimeTicks = dt.Ticks;
//                        return true;
//                    }
//                }
//                catch
//                {
//                }
//            }

//            return false;
//        }

//        private static TimeZoneInfo GetTimeZoneInfoByDisplayName(string displayName)
//        {
//            string sourceZoneStr = displayName?.Split(' ')[0];
//            if (TimeZoneInfo.Local.DisplayName.StartsWith(sourceZoneStr, StringComparison.OrdinalIgnoreCase))
//            {
//                return TimeZoneInfo.Local;
//            }
//            return null;
//        }
//    }

//    public enum ObjectLevel
//    {
//        None,
//        Document,
//        PhysicalRecord,
//        PhysicalFile
//    }

//    public enum OperationType
//    {
//        Created = 0,
//        Destroyed = 1
//    }
//}
