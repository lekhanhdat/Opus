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
using Aspose.Email.Clients.Exchange.WebService.Schema_2016;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Discovery;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.Common;
using System.IO;
using Newtonsoft.Json;
using Microsoft.SharePoint.Client;
using PnP.Core.Model.SharePoint;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using RazorEngine.Compilation.ImpromptuInterface;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using RAManualApprovalCommon.Model;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.JPMC;
using static Org.BouncyCastle.Math.EC.ECCurve;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.Contract.Tenant;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;


namespace AvePoint.RA.SharePoint.Archiver.Common.Manual
{
    internal class ManualUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly IRMCustomIndexMetadataDao s_customIndexMetadataDao = PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();
        private static readonly IRMCustomMetadataColumnDao s_customMetadataColumnDao = PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();
        private static readonly IRMRemoteNodeDao s_remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private static readonly ITenantService s_tenantService = PlatformWindsorManager.GetService<ITenantService>();
        private IExplorerDao explorerDao = new ExplorerDao(true);
        private ScheduleConfiguration configuration;
        private List<RMCustomIndexMetadata> CustomIndexMetadatas = [];
        private List<RMCustomMetadataColumn> CustomMetadataColumns = [];
        private readonly JPMCTenantConfig JPMCConfig;
        public ManualUtil(ScheduleConfiguration scheduleConfiguration)
        {
            this.configuration = scheduleConfiguration;
            _ = s_keyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnableCustomColumn);
            if (isEnableCustomColumn)
            {
                CustomIndexMetadatas = s_customIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.SharePoint).GetAwaiter().GetResult().ToList();
                CustomMetadataColumns = s_customMetadataColumnDao.GetAllCustomMetadataColumnsAsync().GetAwaiter().GetResult().ToList();
            }
            JPMCConfig = GetJPMCConfig();
        }
        public Record ApprovaledRecord(ArchiverNodeItem item)
        {
            if (item.ItemType == ArchiverCommon.ItemType.DOCUMENT || item.ItemType == ArchiverCommon.ItemType.ITEM_TYPE)
            {
                Record recordInDB = GetDBRecord(item);
                if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                {
                    logger.Info($"Item:{item.ID} approve status is approved.");
                    return recordInDB;
                }
                else
                {
                    logger.Info($"Item:{item.ID} approve status is {recordInDB.ManualApprovedStatus}.");
                    return null;
                }
            }
            else
            {
                logger.Info($"Item:{item.ID} not doc or item,type:{item.ItemType}.");
                return null;
            }
        }
        public async Task<bool> IsNeedProcessDataActionForManualAsync(ScheduleConfiguration config, Rule rule, ArchiverNodeItem item, RMTermInfo termInfo, string actionDueDate = "")
        {
            Guid parentId = item.Parent.ID;
            var showDetails = true;
            Record recordInDB = GetDBRecord(item);
            if (recordInDB != null)
            {
                logger.Info($"record in db not null,recordInDB.Id:{recordInDB.Id}");
                if (string.IsNullOrEmpty(recordInDB.ContainerId))
                {
                    recordInDB.ContainerId = config.ContainerId.ToString();
                    logger.Warn($"Item {recordInDB.Id} ContainerId is null or empty, set it {config.ContainerId}");
                }

                if (string.IsNullOrEmpty(recordInDB.AveSiteId))
                {
                    recordInDB.AveSiteId = config.AveSiteId;
                    logger.Warn($"Item {recordInDB.Id} AveSiteID is null or empty, set it {config.AveSiteId}");
                }       
            }   
            ProcessManualResult processManualResult = new();
            if (rule != null)
            {
                var isProcessByOwners = string.IsNullOrEmpty(rule.WorkflowId);
                if (!rule.IsManualApproval)
                {
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info($"Item:{item.ItemId} not match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
                        return false;
                    }
                    if (recordInDB != null && recordInDB.IsManualSynced && recordInDB.ManualArchiveStatus != (int)Contract.Schedule.ActionStatus.Archiverd)
                    {
                        logger.Info($"Item:{item.ItemId} not match manual rule, New rule id:{rule.Id}");
                        recordInDB.RemoveManualFields();
                        CosmosDBManualDataUpdater.Add(recordInDB);
                    }
                    return true;
                }
                if (recordInDB != null)
                {                 
                    if (recordInDB.ManualExtendTime > configuration.ArchiverUNCTime.Ticks && recordInDB.RuleId.ToString() == rule.Id)
                    {
                        logger.Info($"Item:{item.ID} match manual rule, but is extend time data.");
                        showDetails = false;
                    }
                    recordInDB.ManualFullPath = GetManualFullPath(item);
                    recordInDB.ManualFolderPath = GetManualFolderPath(item, recordInDB.ManualFullPath);
                    recordInDB.ManualSiteUrl = item.SiteUrl;
                    recordInDB.ExtensionForFile = GetFileExtension(recordInDB);
                    if (recordInDB.RuleId.ToString() == rule.Id)
                    {
                        //update
                        if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                        {
                            logger.Info($"Item:{item.ID} match manual rule, and approve status is approved.");
                            return true;
                        }
                        else if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            logger.Info($"Item:{item.ItemId} match manual rule, New rule id:{rule.Id},and it is process ApprovalDatasOnly");
                            return false;
                        }
                        else if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                        {
                            if (recordInDB.ManualExtendTime >= DateTime.UtcNow.Ticks)
                            {
                                logger.Warn($"Item {recordInDB.LeafName} current item in disposal extensions Cant Scan,and rule is same ");
                                return false;
                            }
                            logger.Info($"Item:{item.ID} match manual rule, and approve status is rejected.");
                            //add manual history
                            AddManualHistory(recordInDB);
                            //change status to waiting                           
                            processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners, item, termInfo, actionDueDate);
                        }
                        else if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                        {
                            logger.Info($"Item:{item.ID} match manual rule, and approve status is none.");
                            processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners, item, termInfo, actionDueDate);
                        }
                        else
                        {
                            if (IsItemLevelDataChanged(item, recordInDB))
                            {
                                processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners, item, termInfo, actionDueDate);
                                //update to waiting
                                showDetails = true;
                            }
                            logger.Info($"Item:{item.ID} match manual rule, and approve status is {recordInDB.ManualApprovedStatus}.");
                        }
                        InnerProcessSetRecordIsAutoApproval(recordInDB, processManualResult);
                        if (recordInDB.IsAutoApproval)
                        {
                            processManualResult.IsAutoManualApproval = true;
                        }
                    }
                    else
                    {     
                        logger.Info($"Item:{item.ID} match rule id changed. Old rule id:{recordInDB.RuleId} New rule id:{rule.Id}");
                        recordInDB.RuleId = new Guid(rule.Id);
                        recordInDB.ManualExtendCount = 0; // 相同的rule老数据不变，新数据cosmosdb int默认为0，不同的rule清零
                        processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners, item, termInfo, actionDueDate);
                    }
                }
                else
                {
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info($"Item:{item.ItemId} match manual rule, New rule id:{rule.Id},but it is daily job so will not insert to manual review");
                        showDetails = false;
                    }
                    else
                    {
                        //insert
                        var newRecord = GenerateManualRecord(item, rule, parentId);
                        processManualResult = await InnerProcessWaitingForApprovalRecordAsync(newRecord, isProcessByOwners, item, termInfo, actionDueDate);
                    }
                }
            }
            else
            {
                logger.Info($"current item not fit rule,need to hide it,Item:{item.ItemId}");
                if (WrapperConfiguration.IsProcessApprovalDatasOnly && recordInDB != null && recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                {
                    logger.Info($"Item:{item.ItemId} not match rule, and it is process ApprovalDatasOnly and it is approved,will set status.");
                    recordInDB.RemoveManualFields();
                    CosmosDBManualDataUpdater.Add(recordInDB);
                }
                else if (recordInDB != null && recordInDB.IsManualSynced && recordInDB.ManualArchiveStatus != (int)Contract.Schedule.ActionStatus.Archiverd && !WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    logger.Info($"Item:{item.ItemId} not match rule, and it is not process ApprovalDatasOnly,will set status.");
                    recordInDB.RemoveManualFields();
                    CosmosDBManualDataUpdater.Add(recordInDB);
                }
                else
                {
                    logger.Info($"Item:{item.ItemId} not match rule, and it is not process ApprovalDatasOnly,will not insert to manual review.recordInDB.IsManualSynced:{recordInDB?.IsManualSynced},recordInDB.ManualArchiveStatus:{recordInDB?.ManualArchiveStatus}");
                }
            }
            if (rule != null && showDetails)
            {
                if (processManualResult.IsAutoManualApproval)
                {
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info($"this node has set auto approval,item cache node:{item.Cache_NodeType},and it is process ApprovalDatasOnly,will skip this file");
                        return false;

                    }
                    else
                    {
                        logger.Info($"this node has set auto approval,node level:{item.Cache_NodeType}");
                        return true;
                    }
                }
                if (processManualResult.IsSuccess)
                {
                    if (processManualResult?.ErrorType == ProcessManualErrorType.NotExistCustomColumn)
                    {
                        config.JobReportDto.HasErrorNode = true;
                        config.JobReportDto.HasCompleteNode = true;
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Exception, "RM_SPS_Manual_CustomColumnNotExist");
                    }
                    else
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "RM_JM_FSFileWaitingForApproval");
                    }
                }
                else
                {
                    if (processManualResult?.ErrorType == ProcessManualErrorType.NoOwnerError)
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_NoRecordOwner");
                    }
                    else if (processManualResult?.ErrorType == ProcessManualErrorType.WorkflowNoSiteOwner)
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_NotFound_SiteOwner");
                    }
                    else if (processManualResult?.ErrorType == ProcessManualErrorType.WorkflowNoGroup)
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_NotFound_SpecifiedGroup");
                    }
                    else if (processManualResult?.ErrorType == ProcessManualErrorType.NoCustomAppForSendEmail)
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_NotFound_CustomApp");
                    }
                    else if (processManualResult?.ErrorType == ProcessManualErrorType.NoUsersUnderSharePointGroup)
                    {
                        config.JobReportDto.AddScanReport(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, rule.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_NotFoundUserUnderGroup_SpecifiedGroup");
                    }
                    else
                    {
                        logger.Warn("Unsupported exception type");
                    }
                }
            }
            return false;
        }

        private void InnerProcessSetRecordIsAutoApproval(Record recordInDB, ProcessManualResult processManualResult)
        {
            try
            {
                configuration.SetRecordIsAutoApproval(recordInDB);
            }
            catch(Exception e)
            {
                if ((e.InnerException ?? e).Message == "RM_MA_NotFound_CustomApp")
                {
                    logger.Warn($"SetRecordIsAutoApproval ERROR, RM_MA_NotFound_CustomApp");
                    processManualResult.IsSuccess = false;
                    processManualResult.ErrorType = ProcessManualErrorType.NoCustomAppForSendEmail;
                }
            }
        }

        private async Task<ProcessManualResult> InnerProcessWaitingForApprovalRecordAsync(Record recordInDB, bool isProcessByOwners, ArchiverNodeItem item, RMTermInfo termInfo, string actionDueDate)
        {
            ProcessManualResult result = new();
            if (recordInDB.NodeType == (int)NodeLevel.Item)
            {
                recordInDB.LeafName = GetAveListItem(item.DiscoverSPObject)?.GetObjectName();
                recordInDB.LeafName_Array = recordInDB.LeafName.ExplorerAnalyzeBuiltInColumn();
            }
            if(recordInDB.NodeType == (int)NodeLevel.Folder)
            {
                recordInDB.LeafName = item.Name ?? recordInDB.LeafName;
                recordInDB.LeafName_Array = recordInDB.LeafName.ExplorerAnalyzeBuiltInColumn();
            }
            if (recordInDB.ParentId == Guid.Empty)
            {
                if (recordInDB.NodeType == (int)NodeLevel.Document || recordInDB.NodeType == (int)NodeLevel.Item)
                {
                    var aveItem = GetAveListItem(item.DiscoverSPObject);
                    recordInDB.ParentId = item.Parent.SPNodeLevel != NodeLevel.RootFolder ? item.Parent.ID : aveItem.ParentList.ID;
                }
                else if(recordInDB.NodeType == (int)NodeLevel.Folder)
                {
                    var discoverFolder = (AveDiscoverFolder)item.DiscoverSPObject;
                    recordInDB.ParentId = discoverFolder.ParentID != Guid.Empty ? discoverFolder.ParentID : item.Parent.ID;
                    if (item.Parent.SPNodeLevel == NodeLevel.RootFolder)
                    {
                        recordInDB.ParentId = recordInDB.ListId;
                    }
                }
            }

            if (termInfo != null)
            {
                recordInDB.TermId = termInfo.UniqueId;
                recordInDB.TermName = termInfo.Name;
            }

            if (recordInDB.SourceFlag == (int)SourceFlag.SharePoint || recordInDB.SourceFlag == (int)SourceFlag.Teams)
            {
                var aveItem = GetAveListItem(item.DiscoverSPObject);
                recordInDB.ManualDisposalDueDate = !string.IsNullOrEmpty(actionDueDate) ? long.Parse(actionDueDate) : 0;
                recordInDB.CustomColumnDic = GetCustomMetadata(aveItem, recordInDB);
            }

            recordInDB.ManualModifiedTime = item.Modified;
            try
            {
                var newRec = await configuration.ProcessWaitingForApprovalRecordAsync(recordInDB);
                if(newRec == null)
                {
                    logger.Error("can not get newRec form configuation of process waiting for disposal record");
                    throw new ArgumentNullException(nameof(newRec));
                }
                if (newRec.IsAutoApproval)
                {
                    result.IsAutoManualApproval = newRec.IsAutoApproval;
                    logger.Info("this node approval type is auto");
                }
                else
                {
                    if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
                    {
                        result.IsSuccess = false;
                        result.ErrorType = ProcessManualErrorType.NoOwnerError;

                    }
                    else
                    {
                        CosmosDBManualDataUpdater.Add(newRec);
                        if (newRec.SourceFlag == (int)SourceFlag.OneDrive)
                        {
                            GenerateParentRecord(item);
                        }
                    }
                }

                if (newRec.CustomColumnNotExist)
                {
                    result.ErrorType = ProcessManualErrorType.NotExistCustomColumn;
                }
            }
            catch (Exception e)
            {
                logger.Error($"ProcessWaitingForApprovalRecordAsync ERROR:{e}");
                if ((e.InnerException ?? e).Message == "RM_MA_NotFound_SiteOwner")
                {
                    logger.Warn($"ProcessWaitingForApprovalRecordAsync ERROR, RM_MA_NotFound_SiteOwner");
                    result.IsSuccess = false;
                    result.ErrorType = ProcessManualErrorType.WorkflowNoSiteOwner;
                    return result;
                }
                else if ((e.InnerException ?? e).Message == "RM_MA_NotFound_CustomApp")
                {
                    logger.Warn($"ProcessWaitingForApprovalRecordAsync ERROR, RM_MA_NotFound_CustomApp");
                    result.IsSuccess = false;
                    result.ErrorType = ProcessManualErrorType.NoCustomAppForSendEmail;
                    return result;
                }
                else if((e.InnerException ?? e).Message == "RM_MA_NotFound_SpecifiedGroup")
                {
                    logger.Warn($"ProcessWaitingForApprovalRecordAsync ERROR, RM_MA_NotFound_SpecifiedGroup");
                    result.IsSuccess = false;
                    result.ErrorType = ProcessManualErrorType.WorkflowNoGroup;
                    return result;
                }
                else if ((e.InnerException ?? e).Message == "RM_MA_NotFoundUserUnderGroup_SpecifiedGroup")
                {
                    logger.Warn($"ProcessWaitingForApprovalRecordAsync ERROR, RM_MA_NotFoundUserUnderGroup_SpecifiedGroup");
                    result.IsSuccess = false;
                    result.ErrorType = ProcessManualErrorType.NoUsersUnderSharePointGroup;
                    return result;
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        private void GenerateParentRecord(ArchiverNodeItem item)
        {
            if(item.Parent.SPNodeLevel == NodeLevel.RootFolder)
            {
                var parentItem = item.Parent.Parent;
                var recordInDB = GetDBRecord(parentItem);
                if (recordInDB == null)
                {
                    var listRecord = GenerateManualRecord(parentItem, null, Guid.Empty); 
                    listRecord.IsManualSynced = true;
                    listRecord.CreatedBy = "";
                    listRecord.ModifiedBy = "";
                    CosmosDBManualDataUpdater.Add(listRecord);
                }
                else
                {
                    if (!recordInDB.IsManualSynced)
                    {
                        var discoverList = (AveDiscoverList)parentItem.DiscoverSPObject;
                        var aveList = discoverList.GetListObject();
                        recordInDB.IsManualSynced = true;
                        recordInDB.ManualSiteUrl = parentItem.SiteUrl;
                        recordInDB.ManualFullPath = WebUtil.MakeFullUrl(aveList.ParentWeb.Site.Url, discoverList.RootFolderUrl);
                        recordInDB.CreatedBy = "";
                        recordInDB.ModifiedBy = "";
                        recordInDB.ExtensionForFile = GetFileExtension(recordInDB);
                        CosmosDBManualDataUpdater.Add(recordInDB);
                    }
                }
            }
            else if(item.Parent.SPNodeLevel == NodeLevel.Folder)
            {
                var parentItem = item.Parent;
                var recordInDB = GetDBRecord(parentItem);
                if (recordInDB == null)
                {
                    var folderRecord = GenerateManualRecord(parentItem, null, parentItem.ID);
                    if (parentItem.Parent.SPNodeLevel == NodeLevel.RootFolder)
                    {
                        var discoverFolder = (AveDiscoverFolder)parentItem.DiscoverSPObject;
                        var aveFolder = discoverFolder.AveFolder;
                        folderRecord.ParentId = aveFolder.ParentListId;
                    }
                    folderRecord.IsManualSynced = true;
                    CosmosDBManualDataUpdater.Add(folderRecord);
                }
                else
                {
                    var isNeedSync = false;
                    if (recordInDB.ParentId == Guid.Empty)
                    {
                        isNeedSync = true;
                        var discoverFolder = (AveDiscoverFolder)parentItem.DiscoverSPObject;
                        recordInDB.ParentId = discoverFolder.ParentID != Guid.Empty ? discoverFolder.ParentID : parentItem.Parent.ID;
                        if (parentItem.Parent.SPNodeLevel == NodeLevel.RootFolder)
                        {
                            recordInDB.ParentId = recordInDB.ListId;
                        }   
                    }
                    if (!recordInDB.IsManualSynced)
                    {
                        isNeedSync = true;
                        var discoverFolder = (AveDiscoverFolder)parentItem.DiscoverSPObject;
                        var aveFolder = discoverFolder.AveFolder;
                        recordInDB.IsManualSynced = true;
                        recordInDB.ManualSiteUrl = parentItem.SiteUrl;
                        recordInDB.ManualFullPath = WebUtil.MakeFullUrl(aveFolder.ParentWeb.Site.Url, discoverFolder.FullUrl);
                        recordInDB.ExtensionForFile = GetFileExtension(recordInDB);
                    }
                    if (isNeedSync)
                    {
                        CosmosDBManualDataUpdater.Add(recordInDB);
                    }
                }
                GenerateParentRecord(parentItem);
            }
        }
        
        public void AddManualHistory(Record record)
        {
            configuration.ProcessApprovedOrRejectedRecord(record);
        }

        private bool IsItemLevelDataChanged(ArchiverNodeItem item, Record record)
        {
            bool changed = false;
            if (item.SPNodeLevel == NodeLevel.Folder || item.SPNodeLevel == NodeLevel.Item)
            {
                if (record.ManualModifiedTime == 0 && record.TimeModified == 0)
                {
                    logger.Info($"Item:{item.ID}, because it is manual approval data before migration, it cannot check the changes, so the reset logic is skipped.");
                    return false;
                }
                var dbModifiedTime = record.ManualModifiedTime;
                if (record.ManualModifiedTime == 0)
                {
                    dbModifiedTime = record.TimeModified;
                }
                if (item.Modified != dbModifiedTime)
                {
                    changed = true;
                    if (record.ManualModifiedTime == 0)
                    {
                        logger.Info($"Item:{item.ID}, The item modified time use sync time.");
                    }
                    else
                    {
                        logger.Info($"Item:{item.ID}, The item modified time use manual time.");
                    }
                    logger.Info($"Item:{item.ID} changed, so change status to waiting. discover item time is:{new DateTime(item.Modified)}, db item is:{new DateTime(dbModifiedTime)}");
                }
            }
            return changed;
        }

        private string GetManualFullPath(ArchiverNodeItem item)
        {
            string fullPath = string.Empty;
            switch (item.SPNodeLevel)
            {
                case NodeLevel.SiteCollection:
                    var discoverSite = (AveDiscoverSite)item.DiscoverSPObject;
                    var rootWeb = discoverSite.GetRootWeb().AveWeb;
                    fullPath = rootWeb.Url;
                    break;
                case NodeLevel.Site:
                    var discoverObj = (AveDiscoverWeb)item.DiscoverSPObject;
                    var aveWeb = discoverObj.AveWeb;
                    fullPath = aveWeb.Url;
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    var discoverList = (AveDiscoverList)item.DiscoverSPObject;
                    var aveList = discoverList.GetListObject();
                    fullPath = WebUtil.MakeFullUrl(aveList.ParentWeb.Site.Url, discoverList.RootFolderUrl);
                    break;
                case NodeLevel.Folder:
                    var discoverFolder = (AveDiscoverFolder)item.DiscoverSPObject;
                    var aveFolder = discoverFolder.AveFolder;
                    fullPath = WebUtil.MakeFullUrl(aveFolder.ParentWeb.Site.Url, discoverFolder.FullUrl);;
                    break;
                case NodeLevel.Item:
                case NodeLevel.Document:
                    var aveItem = GetAveListItem(item.DiscoverSPObject);
                    var itemUrl = aveItem.FullPath();
                    fullPath = itemUrl;
                    break;
                default:
                    break;
            }
            return fullPath;
        }

        private string GetManualFolderPath(ArchiverNodeItem item, string manualFullPath)
        {
            string folderPath = string.Empty;
            try
            {
                if (item.SPNodeLevel == NodeLevel.Folder || item.SPNodeLevel == NodeLevel.Item || item.SPNodeLevel == NodeLevel.Document) 
                {
                    folderPath = manualFullPath.Replace("\\", "/").Replace(item.SiteUrl, "").Replace(item.Name, "");
                    folderPath = folderPath.Substring(0, folderPath.Length - 1);
                }
            }
            catch (Exception ex) 
            {
                logger.Error($"ManulUtil-GetManualFolderPath Error Exception:{ex}");
                throw;
            }
            return folderPath;
        }
       

        private Record GenerateManualRecord(ArchiverNodeItem item, Rule rule, Guid parentId)
        {
            Record record = new Record();
            //record.Id = IDGenerator.GetRecordId(ScanDataCache.Instance.SiteLevelCache.SPSiteId, item.ID);
            record.ScopeId = configuration.SiteCollectionID;
            record.AveSiteId = configuration.AveSiteId;
            record.CollectTime = DateTime.UtcNow.Ticks;
            record.SourceFlag = configuration.IsOneDriverSite ? (int)SourceFlag.OneDrive : (int)SourceFlag.SharePoint;
            if (configuration.IsTeams)
            {
                record.SourceFlag = (int)SourceFlag.Teams;
                try
                {
                    record.TeamsId = new Guid(configuration.TeamsId);
                }
                catch (Exception e)
                {
                    logger.Error($"ManulUtil-GenerateManualRecord Get teamsId error:{e}");
                }
            }
            record.RuleId = rule != null ? new Guid(rule.Id) : Guid.Empty;
            record.RuleLevel = rule != null ? (int)rule.PolicyLevel : 0;
            record.RecordStatus = (int)RMRecordStatus.ManualPreSync;
            record.ContainerId = configuration.WebAppId;
            record.ManualSiteUrl = item.SiteUrl;
            switch (item.SPNodeLevel)
            {
                case NodeLevel.SiteCollection:
                    var discoverSite = (AveDiscoverSite)item.DiscoverSPObject;
                    var rootWeb = discoverSite.GetRootWeb().AveWeb;
                    record.Id = IDGenerator.GetRecordId(configuration.SiteCollectionID, discoverSite.SiteID);
                    record.LeafName = rootWeb.Title;
                    record.FullPath = rootWeb.Url;
                    record.DirPath = rootWeb.Url;
                    record.NodeId = discoverSite.SiteID;
                    record.TimeCreated = rootWeb.Created.Ticks;
                    record.CreateDate = int.Parse(rootWeb.Created.ToString("yyyyMMdd"));
                    record.NodeType = (int)NodeLevel.SiteCollection;
                    record.CreatedBy = GetUserName(discoverSite.Site.Owner.LoginName);
                    record.ModifiedBy = GetUserName(rootWeb.CurrentUser.Name);
                    record.ExtensionForFile = GetFileExtension(record);
                    break;
                case NodeLevel.Site:
                    var discoverObj = (AveDiscoverWeb)item.DiscoverSPObject;
                    var aveWeb = discoverObj.AveWeb;
                    record.Id = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveWeb.ID);
                    record.LeafName = discoverObj.Title;
                    record.FullPath = aveWeb.Url;
                    record.DirPath = aveWeb.Url;
                    record.NodeId = aveWeb.ID;
                    record.WebId = aveWeb.ID;
                    record.TimeCreated = aveWeb.Created.ToUniversalTime().Ticks;
                    record.CreateDate = int.Parse(aveWeb.Created.ToString("yyyyMMdd"));
                    record.NodeType = (int)NodeLevel.Site;
                    record.CreatedBy = aveWeb.IsRootWeb ? GetUserName(aveWeb.Site.Owner.LoginName) : GetUserName(aveWeb.Author.LoginName);
                    record.ModifiedBy = GetUserName(aveWeb.CurrentUser.Name);
                    record.ExtensionForFile = GetFileExtension(record);
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    var discoverList = (AveDiscoverList)item.DiscoverSPObject;
                    var aveList = discoverList.GetListObject();
                    record.Id = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveList.ID);
                    record.LeafName = discoverList.Title;
                    record.FullPath = WebUtil.MakeFullUrl(aveList.ParentWeb.Site.Url, discoverList.RootFolderUrl);
                    record.DirPath = discoverList.RootFolderUrl;
                    record.NodeId = aveList.ID;
                    record.WebId = aveList.ParentWeb.ID;
                    record.ListId = aveList.ID;
                    record.TimeCreated = aveList.Created.ToUniversalTime().Ticks;
                    record.NodeType = (int)NodeLevel.List;
                    record.CreatedBy = aveList.Author != null ? GetUserName(aveList.Author.LoginName) : "";
                    record.ExtensionForFile = GetFileExtension(record);
                    break;
                case NodeLevel.Folder:
                    var discoverFolder = (AveDiscoverFolder)item.DiscoverSPObject;
                    var aveFolder = discoverFolder.AveFolder;
                    record.Id = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveFolder.UniqueId);
                    record.LeafName = discoverFolder.LeafName;
                    record.FullPath = WebUtil.MakeFullUrl(aveFolder.ParentWeb.Site.Url, discoverFolder.FullUrl);
                    record.ManualFolderPath = GetManualFolderPath(item, WebUtil.MakeFullUrl(aveFolder.ParentWeb.Site.Url, discoverFolder.FullUrl));
                    record.DirPath = discoverFolder.FullUrl;
                    record.NodeId = discoverFolder.DocID;
                    record.WebId = aveFolder.ParentWeb.ID;
                    record.ListId = aveFolder.ParentListId;
                    record.FolderId = discoverFolder.ParentID != Guid.Empty ? discoverFolder.ParentID : parentId;
                    record.ParentId = discoverFolder.ParentID != Guid.Empty ? discoverFolder.ParentID : parentId;
                    record.TimeCreated = ((DateTime)aveFolder.Properties["vti_timecreated"]).ToUniversalTime().Ticks;
                    record.NodeType = (int)NodeLevel.Folder;
                    record.CreatedBy = aveFolder.Item != null ? aveFolder.Item.GetSingleUserFieldValue(SPColumnConstants.Author) : string.Empty;
                    record.ModifiedBy = aveFolder.Item != null ? aveFolder.Item.GetSingleUserFieldValue(SPColumnConstants.Editor) : string.Empty;
                    record.TimeModified = aveFolder.Item != null && aveFolder.Item.FieldValues.ContainsKey(SPColumnConstants.Modified) ? aveFolder.Item.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0;
                    record.ExtensionForFile = GetFileExtension(record);
                    break;
                case NodeLevel.Item:
                case NodeLevel.Document:
                    var aveItem = GetAveListItem(item.DiscoverSPObject);
                    if (aveItem == null)
                    {
                        logger.Error("aveItem is null while due with Document");
                        throw new ArgumentNullException(nameof(aveItem));
                    }
                    var itemUrl = aveItem.FullPath();
                    var itemName = aveItem.GetObjectName();
                    var extension = GetItemExtension(itemName, aveItem);
                    record.Id = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveItem.UniqueId);
                    record.NodeId = aveItem.UniqueId;
                    record.DirPath = aveItem.DirPath();
                    record.FullPath = itemUrl;
                    record.ManualFolderPath = GetManualFolderPath(item, itemUrl);
                    record.LeafName = itemName;
                    record.ExtensionForFile = extension;
                    record.WebId = aveItem.ParentList.ParentWeb.ID;
                    record.ListId = aveItem.ParentList.ID;
                    record.ItemId = aveItem.UniqueId;
                    record.TimeCreated = aveItem.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0;
                    record.NodeType = (int)NodeLevel.Item;
                    record.FolderId = parentId;
                    record.ParentId = item.Parent.SPNodeLevel != NodeLevel.RootFolder ? parentId : aveItem.ParentList.ID;
                    record.MetaInfo = GetMetaInfo(aveItem);
                    record.CreatedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Author);
                    record.ModifiedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Editor);
                    record.DeclareAsRecord = aveItem.IsBlockEditAndDeleteRecord();
                    record.TimeModified = aveItem.FieldValues.ContainsKey(SPColumnConstants.Modified) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0;
                    record.ManualModifiedTime = record.TimeModified;
                    record.ItemRowId = aveItem.ID;
                    try
                    {
                        List<AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo> relatedItemInfos = new RelatedRecordsUtility().GetRelatedProperties(aveItem);
                        if (relatedItemInfos != null)
                        {
                            var reportRelatedRecords = new List<ReportRelatedRecords>();
                            relatedItemInfos.ForEach(item =>
                            {
                                if (item.SourceFlag == (int)SourceFlag.SharePoint || item.SourceFlag == (int)SourceFlag.All)
                                {
                                    var relatedItemUrl = WebUtil.MakeFullUrl(item.SiteUrl, item.url);
                                    reportRelatedRecords.Add(
                                        new ReportRelatedRecords
                                        {
                                            Name = item.name,
                                            Url = relatedItemUrl
                                        }
                                    );
                                }
                                else if (item.SourceFlag == (int)SourceFlag.Physical)
                                {
                                    var url = $"/Root/PRM/RecordsExplorer/?uniqueId={item.recId}";
                                    reportRelatedRecords.Add(new ReportRelatedRecords() { Name = item.recId, Url = url });
                                }
                            });
                            record.ManualRelatedRecords = AvePoint.GCommon.Utility.SerializerHelper.SerializeToXmlString(reportRelatedRecords);

                            record.ManualIsRelatedRecords = reportRelatedRecords.Count > 0;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Get related properties error: {e}");
                    }

                    break;
                default:
                    break;
            }
            record.ManualFullPath = record.FullPath;
            return record;
        }

        private string GetUserName(string name)
        {
            string value = string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
            {
                value = name.Split('|').Last();
            }
            return value;
        }

        private string GetFileExtension(Record record)
        {
            if (!string.IsNullOrEmpty(record.ExtensionForFile))
            {
                return record.ExtensionForFile;
            }

            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.ExchangeOnlineItem:
                    return "msg";
                //case RMNodeLevel.Item:
                //    if (record.ArchiveLevel == (int)CacheNodeType.Item)
                //    {
                //        return "RM_RDM_RecordDetails_DataType_SPItem";
                //    }
                //    var ext = Path.GetExtension(record.LeafName);
                //    return ext.Contains('.', StringComparison.CurrentCulture) ? ext[1..] : "RM_RDM_RecordDetails_DataType_FileNull";
                case RMNodeLevel.SiteCollection:
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case RMNodeLevel.Site:
                    return "RM_JS_Rule_ObjectLevel_Site";
                case RMNodeLevel.List:
                    return "RM_Common_ObjectLevel_List";
                case RMNodeLevel.Folder:
                    return "RM_Common_ObjectLevel_Folder";
                case RMNodeLevel.FSFolder:
                    return "RM_RDM_RecordDetails_DataType_FSFolder";
                case RMNodeLevel.FSFile:
                    var fsExt = Path.GetExtension(record.LeafName);
                    if (fsExt.Contains('.', StringComparison.CurrentCulture))
                    {
                        return fsExt[1..];
                    }
                    return "";
                case RMNodeLevel.PhysicalBox:
                    return "RM_PRM_PRE_Filter_PhysicalBox";
                case RMNodeLevel.PhysicalFile:
                    return "RM_PRM_PRE_Filter_PhysicalFile";
                case RMNodeLevel.PhysicalRecord:
                    return "RM_PRM_PRE_Filter_PhysicalRecord";
                case RMNodeLevel.PhysicalCustom:
                    return "RM_PRM_PRE_TableItemType_Container";
                case RMNodeLevel.CustomizeConnectorItem:
                    return "RM_Connector_ItemLevel_Item";
            }


            return "";
        }

        private IAveListItem GetAveListItem(object info)
        {
            if (info is IAveListItem)
            {
                return (IAveListItem)info;
            }
            else if (info is AveDiscoverItem)
            {
                return ((AveDiscoverItem)info).CurrentItem;
            }
            return null;
        }

        private string GetItemExtension(string objectName, IAveListItem aveItem)
        {
            var result = string.Empty;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                var ext = Path.GetExtension(objectName);
                result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
            }
            else
            {
                result = "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return result;
        }
        private string GetMetaInfo(IAveListItem aveItem)
        {
            RecordMetaInfo metaInfo = new RecordMetaInfo
            {
                FileSize = aveItem.FieldValues.ContainsKey(SPColumnConstants.File_Size) ? Convert.ToInt64(aveItem.FieldValues[SPColumnConstants.File_Size]) : 0
            };
            return JsonConvert.SerializeObject(metaInfo);
        }

        private Record GetDBRecord(ArchiverNodeItem item)
        {
            Record dbRecord = null;
            Guid recId = Guid.Empty;
            switch (item.SPNodeLevel)
            {
                case NodeLevel.SiteCollection:
                    var discoverSite = (AveDiscoverSite)item.DiscoverSPObject;                 
                    recId = IDGenerator.GetRecordId(configuration.SiteCollectionID, discoverSite.SiteID);
                    break;
                case NodeLevel.Site:
                    var discoverObj = (AveDiscoverWeb)item.DiscoverSPObject;
                    var aveWeb = discoverObj.AveWeb;
                    recId = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveWeb.ID);
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    var discoverList = (AveDiscoverList)item.DiscoverSPObject;
                    var aveList = discoverList.GetListObject();
                    recId = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveList.ID);
                    break;
                case NodeLevel.Folder:
                    var discoverFolder = (AveDiscoverFolder)item.DiscoverSPObject;
                    var aveFolder = discoverFolder.AveFolder;
                    recId = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveFolder.UniqueId);
                    break;
                case NodeLevel.Item:
                case NodeLevel.Document:
                    var aveItem = GetAveListItem(item.DiscoverSPObject);
                    recId = IDGenerator.GetRecordId(configuration.SiteCollectionID, aveItem.UniqueId);
                    break;
                default:
                    break;
            }
            if (recId != Guid.Empty)
            {
                dbRecord = explorerDao.QueryAll(r => r.Id == recId).FirstOrDefault();
            }
            return dbRecord;
        }

        protected Dictionary<string, CustomColumn> GetCustomMetadata(IAveListItem aveItem, Record record)
        {
            var dic = new Dictionary<string, CustomColumn>();
            foreach (var customIndexMetadata in CustomIndexMetadatas)
            {
                try
                {
                    var columnInfo = CustomMetadataColumns.Where(c => c.UniqueId == customIndexMetadata.TargetColumnId).FirstOrDefault();
                    var sourceColumnName = customIndexMetadata.SourceColumnName;
                    if(JPMCConfig != null)
                    {
                        logger.Info($"Current site [{configuration.SiteCollectionUrl}] has JPMC config.");
                        if (sourceColumnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && sourceColumnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                        {
                            sourceColumnName = customIndexMetadata.SourceColumnName.Trim(['[', ']']);
                        }

                        if(sourceColumnName.Equals(JPMCConfig.CustomColumns.StartDate, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info($"Current site [{configuration.SiteCollectionUrl}] has JPMC config, and current column is [{JPMCConfig.CustomColumns.StartDate}].");
                            var startDate = ProcessJPMCStartDateColumn(sourceColumnName, columnInfo, aveItem, record);
                            dic[columnInfo.UniqueId.ToString()] = startDate;
                            continue;
                        }
                    }
                    if (sourceColumnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && sourceColumnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceColumnName = customIndexMetadata.SourceColumnName.Trim(['[', ']']);
                        if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object internalValue))
                        {
                            var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, internalValue);
                            dic[columnInfo.UniqueId.ToString()] = metadataValue;
                        }
                        else
                        {
                            logger.Warn($"Can not get value by column [{sourceColumnName}]");
                            record.CustomColumnNotExist = true;
                        }
                        continue;
                    }

                    if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object value))
                    {
                        var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, value);
                        dic[columnInfo.UniqueId.ToString()] = metadataValue;
                        continue;
                    }

                    try
                    {
                        var aveItemColumnValue = aveItem[sourceColumnName];
                        var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, aveItemColumnValue);
                        dic[columnInfo.UniqueId.ToString()] = metadataValue;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Can not get value by column [{sourceColumnName}]");
                        record.CustomColumnNotExist = true;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Get custom column [{customIndexMetadata.SourceColumnName}] failed, error: {e}");
                    record.CustomColumnNotExist = true;
                }
            }

            return dic;
        }
        
        private object GetItemValue(string sourceColumnName, RMCustomMetadataColumn column, IAveListItem aveItem, Record record)
        {
            if (sourceColumnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && sourceColumnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
            {
                sourceColumnName = sourceColumnName.Trim(['[', ']']);
                if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object internalValue))
                {
                    return internalValue;
                }
                else
                {
                    logger.Warn($"Can not get value by column [{sourceColumnName}]");
                    record.CustomColumnNotExist = true;
                    return null;
                }
            }

            if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object value))
            {
                return value;
            }

            try
            {
                return aveItem[sourceColumnName];
            }
            catch (Exception ex)
            {
                logger.Warn($"Can not get value by column [{sourceColumnName}]");
                record.CustomColumnNotExist = true;
                return null;
            }
        }

        private CustomColumn ProcessJPMCStartDateColumn(string sourceColumnName, RMCustomMetadataColumn column, IAveListItem aveItem, Record record)
        {
            var startDateItemValue = GetItemValue(sourceColumnName, column, aveItem, record);
            logger.Info($"Process JPMC StartDate column [{sourceColumnName}] with value [{startDateItemValue}]");
            var resultColumn = GetValueByType(column, aveItem, sourceColumnName, startDateItemValue);

            if (column.ColumnType != CustomColumnType.DateTime)
            {
                logger.Warn($"Column [{sourceColumnName}] is not DateTime type, but got {column.ColumnType}");
                return resultColumn;
            }

            var retention = CustomIndexMetadatas.FirstOrDefault(c => c.SourceColumnName.Equals(JPMCConfig.CustomColumns.RetentionType, StringComparison.OrdinalIgnoreCase));
            if (retention == null)
            {
                logger.Warn($"Can not find retention column by name [{JPMCConfig.CustomColumns.RetentionType}]");
                return resultColumn;
            }
            var retentionColumn = CustomMetadataColumns.Where(c => c.UniqueId == retention.TargetColumnId).FirstOrDefault();
            if (retentionColumn == null || retentionColumn.ColumnType != CustomColumnType.SingleText)
            {
                logger.Warn($"Retention column [{retention.SourceColumnName}] is not SingleText type, but got {retentionColumn?.ColumnType}");
                return resultColumn;
            }

            var retentionValue = GetItemValue(retention.SourceColumnName, retentionColumn, aveItem, record);
            if (retentionValue == null || string.IsNullOrEmpty(retentionValue.ToString()))
            {
                logger.Warn($"Can not get retention value by column [{retention.SourceColumnName}]");
                return resultColumn;
            }

            if (retentionValue.ToString().Equals("Flat", StringComparison.OrdinalIgnoreCase))
            {
                logger.Info("Retention value is Flat, get startDate by modified time");
                if (aveItem.FieldValues.ContainsKey(SPColumnConstants.Modified))
                {
                    var dateTime = aveItem.GetCustomUTCDateWithTimeZone(SPColumnConstants.Modified);
                    if (dateTime == DateTime.MinValue)
                    {
                        throw new Exception("Can not get the DateTime value");
                    }
                    var timeColumn = new DateTimeColumnValue() { Date = dateTime, TimeZoneId = "UTC" };
                    resultColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    resultColumn.Date = dateTime;
                    resultColumn.TimeZoneId = "UTC";
                }
            }

            return resultColumn;
        }

        private CustomColumn GetValueByType(RMCustomMetadataColumn column, IAveListItem aveItem, string sourceColumnName, object value)
        {
            var customColumn = new CustomColumn();
            if (value == null)
            {
                return customColumn;
            }

            // Special handling for _ComplianceTagWrittenTime
            if (string.Equals(sourceColumnName, "_ComplianceTagWrittenTime", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var dateTime = DateTime.Parse(value.ToString()).ToUniversalTime();
                    var timeColumn = new DateTimeColumnValue() { Date = dateTime, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTime;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error parsing _ComplianceTagWrittenTime: {ex}");
                    return customColumn;
                }
            }

            // Special handling for _dlc_DocId
            if (string.Equals(sourceColumnName, "_dlc_DocId", StringComparison.OrdinalIgnoreCase))
            {
                customColumn.Value = aveItem.FieldValues[sourceColumnName]?.ToString() ?? string.Empty;
                customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                return customColumn;
            }

            switch (column.ColumnType)
            {
                case CustomColumnType.SingleText:
                    return GetMetadataCustomColumn(value);
                case CustomColumnType.Number:
                    if (!double.TryParse(value.ToString(), out var numberValue))
                    {
                        throw new Exception("Can not get the Number value");
                    }

                    customColumn.Value = numberValue.ToString();
                    customColumn.Value_Array = value?.ToString().ExplorerAnalyzeBuiltInColumn() ?? [];
                    customColumn.Number = GetNumber(value.ToString());
                    return customColumn;
                case CustomColumnType.YesOrNo:
                    if (!bool.TryParse(value?.ToString(), out var result))
                    {
                        throw new Exception("Can not get the YesOrNo value");
                    }

                    customColumn.Value = value?.ToString() ?? string.Empty;
                    customColumn.YesOrNo = result ? "Yes" : "No";
                    return customColumn;
                case CustomColumnType.DateTime:
                    var dateTime = aveItem.GetCustomUTCDateWithTimeZone(sourceColumnName);
                    if (dateTime == DateTime.MinValue)
                    {
                        throw new Exception("Can not get the DateTime value");
                    }
                    var timeColumn = new DateTimeColumnValue() { Date = dateTime, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTime;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;
                default:
                    return customColumn;
            }
        }

        private static double GetNumber(string content)
        {
            double result = default(double);
            if (content != null && content.Length < 255)
            {
                if (double.TryParse(content, out result))
                {
                    return result;
                }
            }
            return result;
        }

        private static CustomColumn GetMetadataCustomColumn(object value)
        {
            var valueString = value?.ToString() ?? string.Empty;
            var customColumn = new CustomColumn();
            if (string.IsNullOrEmpty(valueString))
            {
                customColumn.Value = string.Empty;
                customColumn.Value_Array = [];
                return customColumn;
            }

            if (valueString.IndexOf('|') > -1)
            {
                var metadataInfo = valueString.Split('|');
                if (metadataInfo.Length == 2)
                {
                    valueString = metadataInfo[0];
                }
            }
            else if (valueString.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
            {
                valueString = valueString.Substring("i:0#.w|".Length);
            }
            else if (valueString.IndexOf(";#") > -1)
            {
                var userValues = valueString.Split(new string[] { ";#" }, StringSplitOptions.None);
                valueString = userValues[1];
            }

            customColumn.Value = valueString;
            customColumn.Value_Array = valueString.ExplorerAnalyzeBuiltInColumn();
            return customColumn;
        }

        private RMTermInfo GetTermInfo(IAveListItem item)
        {
            RMTermInfo termInfo = null;
            if (string.IsNullOrEmpty(this.configuration.BCSColumnName))
            {
                return termInfo;
            }
            try
            {
                var termObj = item[this.configuration.BCSColumnName];
                if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                {
                    var valueString = termObj.ToString().Split('|');
                    if (valueString.Length > 1)
                    {
                        termInfo.UniqueId = new Guid(valueString[1]);
                        termInfo.Name = valueString[0] ?? string.Empty;
                    }
                    else
                    {
                        logger.Info($"{item.UniqueId} invalid term format:{valueString}");
                    }
                }
                else
                {
                    logger.Warn($"Item FieldValues do not contain BCS column or term is null. Item ID: [{item.ID}] Column Internal Name: [{this.configuration.BCSColumnName}]");
                }
            }
            catch
            {
                logger.Warn($"Item Fields do not contain BCS column. Item ID: [{item.ID}] Column Internal Name: [{this.configuration.BCSColumnName}]");
            }
            
            return termInfo;
        }

        private JPMCTenantConfig GetJPMCConfig()
        {
            var jsonConfig = s_keyValueDao.GetValueByKey("JPMC_Customization");
            if (jsonConfig != null && !string.IsNullOrEmpty(jsonConfig.Value))
            {
                try
                {
                    var configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                    if(configs.Count > 0)
                    {
                        var configSiteUrls = configs.Select(c => c.ConfigSiteUrl).ToList();
                        var remoteNodes = s_remoteNodeDao.GetRemoteSiteCollectionBySiteUrls(configSiteUrls);
                        configs.ForEach(c =>
                        {
                            var remoteSite = remoteNodes.FirstOrDefault(s => s.url == c.ConfigSiteUrl);
                            if (remoteSite != null)
                            {
                                c.ConfigSite = remoteSite;
                                c.M365TenantId = remoteSite.TenantId;
                            }
                            else
                            {
                                logger.Warn($"Can not get this site:{c.ConfigSiteUrl}");
                            }
                        });
                        var currentRemoteNode = s_remoteNodeDao.GetRemoteSiteCollectionByUrl(configuration.SiteCollectionUrl);

                        var currentSiteConfig = configs.FirstOrDefault(c => c.M365TenantId.Equals(currentRemoteNode.TenantId, StringComparison.OrdinalIgnoreCase));
                        return currentSiteConfig;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error deserializing JPMC_Customization: {e.Message}");
                    return null;
                }
            }

            return null;
        }
    }
}
