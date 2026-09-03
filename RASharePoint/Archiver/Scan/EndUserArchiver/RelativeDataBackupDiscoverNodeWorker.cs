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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.StorageOptimization.Schedule.Archiver
{
    class RelativeDataBackupDiscoverNodeWork : IDiscoverNodeWorker
    {
        #region Private Fields
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IBackwardDependencyNodeCache<ArchiveApproveReport> backupNodeCache;
        private IBackwardDependencyNodeCache<object> mDependencyObjs;
        private ArchiveApproveReport mReport;
        private bool mDoDelete = true;
        private List<int> mSystemListTable = new List<int>();
        private ScheduleConfiguration mConfig;
        #endregion

        #region Public properties
        public int DiscoverCacheNodeType { get; set; }
        #endregion

        #region Constructor
        public RelativeDataBackupDiscoverNodeWork(IBackwardDependencyNodeCache<ArchiveApproveReport> mBackupNodeCache, IBackwardDependencyNodeCache<object> dependencyObjs, ScheduleConfiguration config)
        {
            backupNodeCache = mBackupNodeCache;
            mDependencyObjs = dependencyObjs;
            mConfig = config;
            mSystemListTable = config.BackgroundSettings.ListTemplateTable;
        }

        #endregion

        #region Public Functions

        public bool IsRuleBreakInheritNode(string md5URL)
        {
            return false;
        }

        public void Init(object obj)
        {
        }
        public bool ProcessContainerLevelNodeWithRule(ArchiverNodeItem item)
        {
            if (item.SPNodeLevel == NodeLevel.WebApplication)
            {
                return false;
            }

            ///*system list not to check rule*/
            if (!item.IsSystemObject)
            {
                switch ((CacheNodeType)item.Cache_NodeType)
                {
                    case CacheNodeType.Folder:
                        {
                            break;
                        }
                    case CacheNodeType.List:
                        {
                            IAveList tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                            if ((tmpList.Hidden || tmpList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase)) || (!tmpList.AllowDeletion && !mSystemListTable.Contains((int)tmpList.BaseTemplate)))
                            {
                                mDoDelete = false;
                            }
                            else
                            {
                                mDoDelete = true;
                            }
                            break;
                        }
                    case CacheNodeType.SiteCollection:
                        {
                            mDoDelete = true;
                            break;
                        }
                    default:
                        {
                            if (item.Cache_NodeType > (int)CacheNodeType.Web && item.Cache_NodeType < (int)CacheNodeType.List)
                            {
                                mDoDelete = true;
                            }
                            break;
                        }
                }
            }
            //all in this case need to add in pccontainer(diffrent from Archive Backup)
            mReport = item.ConvertToArchiveApproveReport();
            if ((item.Cache_NodeType >= DiscoverCacheNodeType) && mDoDelete)
            {
                mReport.DoDelete = true;
            }
            else
            {
                mReport.DoDelete = false;
            }
            mReport.ArchiveLevel = item.ReportLevel;
            backupNodeCache.PutIn(mReport, mReport.CacheNodeType, true);
            return false;
        }

        public async Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            ProcessResult result = ProcessResult.Default;
            //mConfig.currentRule.DeleteRecords only records job has DeleteRecords = true case.
            if (mConfig.currentRule.DeleteRecords || mConfig.BackgroundSettings.IsDeleteRecord || RuleHelper.CheckArchiveOnlyRule(mConfig.currentRule) || !CheckItemRecord(item))
            {
                if (!CheckItemHoldOnly(item))
                {
                    mReport = item.ConvertToArchiveApproveReport();
                    mReport.DoDelete = mDoDelete;
                    mReport.RuleId = mConfig.currentRule.Id;
                    mReport.RuleName = mConfig.currentRule.Name;
                    mReport.ArchiveLevel = GetArchiveLevel(mReport);
                    mReport.IsRelativeDataJob = true;
                    backupNodeCache.PutIn(mReport, mReport.CacheNodeType, true);
                    JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, mReport, mConfig.currentRule);
                    //在此处将Item 插入DB，这样后续的更新db 逻辑就都可以正常work
                    //mConfig.soArchiverQueryWorker.InsertEndUserNodeToApproveDB(mReport, mConfig);

                }
                else
                {
                    result = ProcessResult.SkipCurrentNode;
                }
            }
            else
            {
                result = ProcessResult.SkipCurrentNode;
            }
            return result;
        }

        public int GetArchiveLevel(ArchiveApproveReport reportNode)
        {
            int ArchiveLevel = -1;
            Rule rule = mConfig.currentRule;
            switch (rule.PolicyLevel)
            {
                case GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection:
                    ArchiveLevel = (int)SPNodeLevel.SiteCollection;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Site:
                    ArchiveLevel = (int)SPNodeLevel.Web;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Library:
                case GCommon.Contract.CommonFilter.PolicyLevel.List:
                    ArchiveLevel = (int)SPNodeLevel.List;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Folder:
                    ArchiveLevel = (int)SPNodeLevel.Folder;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Item:
                    //如果节点级别是ItemVersion 6 或者Attachment 6 ，并且符合了Item rule 表示符合parent rule
                    ArchiveLevel = (reportNode.NodeType == 5 || reportNode.NodeType == 6) ? (int)SPNodeLevel.FitParentRule : (int)SPNodeLevel.Item;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Newsfeed:
                    ArchiveLevel = (int)SPNodeLevel.Item;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.ItemVersion:
                    ArchiveLevel = (int)SPNodeLevel.ItemVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Document:
                    //如果节点级别是Document version 2 ，并且符合了Document rule ，表示符合parent rule
                    ArchiveLevel = reportNode.NodeType == 2 ? (int)SPNodeLevel.FitParentRule : (int)SPNodeLevel.Document;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion:
                    ArchiveLevel = (int)SPNodeLevel.DocumentVersion;
                    break;
                case GCommon.Contract.CommonFilter.PolicyLevel.Attachment:
                    ArchiveLevel = (int)SPNodeLevel.Attachment;
                    break;
                default:
                    break;
            }

            return ArchiveLevel;
        }


        public void ProcessContainerLevelNodeResultRule(ArchiverNodeItem item)
        {
            if (item.SPNodeLevel == NodeLevel.WebApplication)
            {
                return;
            }
            mReport = item.ConvertToArchiveApproveReport();
            mReport.DoDelete = false;
            backupNodeCache.PutIn(mReport, mReport.CacheNodeType, true);
        }

        public void Flush()
        {
            backupNodeCache.Flush();
        }

        public void Dispose()
        {
            backupNodeCache?.Dispose();
        }
        #endregion



        public bool ProcessLowerLevel(ArchiverNodeItem item)
        {
            return true;
        }

        //"CheckRule" Only for MetlifeVault ,Other Type do not need
        public async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)
        {
            if (type == ProcessType.NoNeedProcess)
            {
                ProcessContainerLevelNodeResultRule(item);
            }
            else
            {
                ProcessContainerLevelNodeWithRule(item);
            }
            return ProcessResult.Continue;
        }

        private bool CheckItemRecord(ArchiverNodeItem item)
        {
            bool isRecord = false;
            try
            {
                if (!item.IsSystemObject)
                {
                    IAveList tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                    if (item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT || item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER || item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.ITEM_VERSION)
                    {
                        isRecord = item.Parent.CheckisRecord(tmpList) ? (item.Parent.CheckIsBlockDeleteOnlyRecord(tmpList) && mConfig.IsILMode ? false : true) : false;
                    }
                    else
                    {
                        if (item.Parent.IsRecord == true)
                        {
                            isRecord = item.CheckisRecord(tmpList) ? (item.CheckIsBlockDeleteOnlyRecord(tmpList) && mConfig.IsILMode ? false : true) : false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverCheckRecordError, ex.ToString());
            }
            return isRecord;
        }

        private bool CheckItemHoldOnly(ArchiverNodeItem item)
        {
            bool isHoldOnly = false;
            try
            {
                if (!item.IsSystemObject)
                {
                    IAveList tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                    if (item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT || item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER || item.ItemType == RA.SharePoint.ArchiverCommon.ItemType.ITEM_VERSION)
                    {
                        isHoldOnly = item.Parent.CheckIsHoldOnly(tmpList);
                    }
                    else
                    {
                        if (item.Parent.IsRecord == true)
                        {
                            isHoldOnly = item.CheckIsHoldOnly(tmpList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiverCheckRecordError, ex.ToString());
            }
            return isHoldOnly;
        }

        public void SendScanDetail(string errorMessage, string srcURL, string subJobId, int cacheNodeType, JobDetailsStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
