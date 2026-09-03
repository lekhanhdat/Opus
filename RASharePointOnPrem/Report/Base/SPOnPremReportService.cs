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
using System;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System.Linq;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Schedule;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Model;
using System.IO;
using AvePoint.RA.Common.FileSystem;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Common.Report;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.Common.FilterEngine;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RASharePointOnPrem.Report.Base;
using AvePoint.Common;
using AvePoint.RA.Common.SystemSetting;

namespace AvePoint.RA.RASharePointOnPrem.Report.Base
{
    public abstract class SPOnPremReportService
    {

        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SPOnPremReportService));

        #region Interface
        private RA.DB.Explorer.Dao.IExplorerDao mExplorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        private IExplorerService mExplorerService;
        protected IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }

        private IRecordAllianceDao mRecordAllianceDao;
        public IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mRecordAllianceDao == null)
                {
                    mRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mRecordAllianceDao;
            }
        }

        private IRMReportService mReportService;
        public IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        private ISharePointOnPremiseSettingDao _SharePointOnPremiseSettingDao = null;
        public ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao
        {
            get
            {
                if (_SharePointOnPremiseSettingDao == null)
                {
                    _SharePointOnPremiseSettingDao = (ISharePointOnPremiseSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointOnPremiseSettingDao));
                }
                return _SharePointOnPremiseSettingDao;
            }
        }
        #endregion

        protected bool _jobHasException = false;
        protected bool _jobHasStopped = false;
        protected string mSiteUrl;
        protected string mSiteTitle;
        int[] mProcessRecordStatus;

        protected List<RMSPTreeNode> mSPTreeNodes;

        public SPOnPremReportService(string jobId, string profileId)
        {
            try
            {
                mProcessRecordStatus = GetProcessRecordStatus();
                RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
                mSPTreeNodes = this.GetSelectedTreeNode(profile.Extension2);
            }
            catch (Exception e)
            {
                mLog.Error($"Report ctor error: {e}");
            }
        }

        private List<RMSPTreeNode> GetSelectedTreeNode(string ext2)
        {
            //SerializerHelper.DeserializeByJsonSerializer<List<RMSPTreeNode>>(ext2, true);
            var farm = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(ext2);
            var treeNodes = new List<RMSPTreeNode>();
            GetNodesList(farm, treeNodes);
            //string Extension2 = RuleSPTreeUtil.BuildFSTreeXMLStr(ext2);
            //RMSPTreeNode rmTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(Extension2);
            var reportNodeLevels = new List<int>() { (int)NodeLevel.SiteCollection, (int)NodeLevel.Site, (int)NodeLevel.List };
            return treeNodes.Where(o => o.CheckNumber == 1 && reportNodeLevels.Contains(o.Level)).ToList();
        }


        private void GetNodesList(RMSPTreeNode node, List<RMSPTreeNode> nodesList)
        {
            nodesList.Add(node);
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GetNodesList(child, nodesList);
                }
            }
        }


        protected virtual void Process()
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    if (mSPTreeNodes == null)
                    {
                        mLog.Warn("No tree nodes found.");
                        return;
                    }
                    foreach (var mSPTreeNode in mSPTreeNodes)
                    {
                        ProcessSelectedNode(mSPTreeNode);
                    }
                }
            }
            catch (JobStopException)
            {
                mLog.Warn("This Job is stopped.");
                _jobHasStopped = true;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while runnning. ", e.ToString());
                _jobHasException = true;
                throw;
            }
            finally
            {
                var finalStatus = _jobHasStopped ? JobStatus.Stopped : _jobHasException ? JobStatus.FinishWithException : JobStatus.Finished;
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        protected void ProcessSelectedNode(RMSPTreeNode treeNode)
        {
            var siteNode = treeNode.GetSiteCollectionNode();
            mSiteUrl = siteNode?.FullPath;
            ArgumentCheck.NotNull(siteNode, nameof(siteNode));
            mSiteTitle = siteNode.Title;
            var groupNode = treeNode.GetGroupNode();
            switch ((NodeLevel)treeNode.Level)
            {
                case NodeLevel.SiteCollection:
                    var records = ExplorerDao.GetFirstOrDefault(o => o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                        && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                        && o.DirPath == treeNode.FullPath
                        && o.NodeType == (int)RMNodeLevel.SiteCollection);
                    ProcessSite(records);
                    break;
                case NodeLevel.Site:
                    var recordsSite = ExplorerDao.GetFirstOrDefault(o => o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                        && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                        && o.WebId == new Guid(treeNode.Id)
                        && o.NodeType == (int)RMNodeLevel.Site);
                    ProcessWeb(recordsSite);
                    break;
                case NodeLevel.List:
                    var recordsList = ExplorerDao.GetFirstOrDefault(o => o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                        && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                        && o.ListId == new Guid(treeNode.Id)
                        && o.NodeType == (int)RMNodeLevel.List);
                    ProcessList(recordsList);
                    break;
            }
        }

        protected virtual void ProcessSite(Record site)
        {
            if (site == null)
            {
                return;
            }
            SendJobReportDetails(site, JobDetailsStatus.Successful);
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                && o.AveSiteId == site.AveSiteId
                && o.NodeType == (int)RMNodeLevel.Site, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                foreach (var web in datas)
                {
                    ProcessWeb(web);
                }
            }
        }

        protected virtual void ProcessWeb(Record web)
        {
            if (web == null)
            {
                return;
            }
            SendJobReportDetails(web, JobDetailsStatus.Successful);
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                && o.AveSiteId == web.AveSiteId
                && o.WebId == web.WebId
                && o.NodeType == (int)RMNodeLevel.List, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                foreach (var list in datas)
                {
                    ProcessList(list);
                }
            }
        }

        protected virtual void ProcessList(Record list)
        {
            if (list == null)
            {
                return;
            }
            SendJobReportDetails(list, JobDetailsStatus.Successful);
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.SharePointOnPrem
                && Enumerable.Contains(mProcessRecordStatus, o.RecordStatus)
                && o.AveSiteId == list.AveSiteId
                && o.ListId == list.ListId
                && o.NodeType == (int)RMNodeLevel.Item, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                ReportManager.IncreaseBase(datas.Count);
                foreach (var record in datas)
                {
                    mLog.Info($"process item, record id: {record.NodeId}");
                    ProcessItem(record);
                }
            }
        }
        protected abstract int ProcessItem(Record record);

        protected virtual int[] GetProcessRecordStatus() {
            return new int[] { (int)RMRecordStatus.Active };
        }

        protected virtual void SendJobReportDetails(Record item, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = JobReportUtility.ConvertItemTypeForDetails((NodeLevel)item.NodeType);
            detail.TitleOrName = item.LeafName;
            detail.Url = WebUtil.MakeFullUrl(mSiteUrl, item.DirPath);
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        protected string GetListItemRealPath(Guid itemListId, string itemUrl)
        {
            var itemList = ExplorerDao.GetFirstOrDefault(o => o.NodeId == itemListId);
            if (itemList == null)
            {
                return WebUtil.MakeFullUrl(mSiteUrl, itemUrl);
            }
            else
            {
                return WebUtil.GetListItemRealPath(mSiteUrl, itemList.DirPath, itemUrl);
            }
        }

        protected long ConvertTimeFromUtc(long ticks, string currentTimeZoneInfoId)
        {
            var currentTimeZoneInfo = GeneralSettingConfig.FindSystemTimeZoneById(currentTimeZoneInfoId);
            return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(ticks, DateTimeKind.Utc), currentTimeZoneInfo).Ticks;
        }
    }
}
