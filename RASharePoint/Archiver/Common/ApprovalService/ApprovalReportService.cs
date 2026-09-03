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





//#define SELF_DEBUG //没有manager
#define INTER_DEBUG //有manager

using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal interface IApprovalReportOpers : IDisposable
    {
        List<ArchiveApproveReport> SelectItemsByParentWithJsonMeta(string ruleId, string parentNodeId);
        List<ArchiveApproveReport> SelectItemVersionsWithJsonMeta(string ruleId, Guid nodeId);
        /// <summary>
        /// 向数据库中加入一条记录。需要注意更新相关的servcie信息表。
        /// </summary>
        /// <param name="detail"></param>
        void AddToDB(ArchiveApproveReport nodeEntity, bool hasReported);
        /// <summary>
        /// AddScanReport 为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="detail"></param>
        void AddScanReport(ArchiveApproveReport nodeEntity);
        /// <summary>
        /// 从数据库中读出一条由Reset函数指定的相关的service的记录。
        /// </summary>
        /// <returns>如果读取完毕， 返回空</returns>
        ArchiveApproveReport ReadFromDB();
        /// <summary>
        /// 初始化指定一次迭代读取数据库中对应的svcId对应的记录
        /// </summary>
        /// <param name="ruleId"></param>
        void Reset(string ruleId);

        /// <summary>
        /// 从数据库读取所有的文件符合rule的id, 用于之后遍历rule
        /// </summary>
        /// <returns></returns>
        List<string> GetDataRuleCollection();

        long GetDataCount(int minCacheNodeType = 0);

        Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "");

        List<Guid> ExistInScanJob(List<Guid> nodeIds);

        bool CheckListOrFolderHasFitRuleFile(Guid listId, string containerId, string ruleId);
        void Flush();
    }

    


    public class ApprovalReportService : IScheduleContainer<ArchiveApproveReport> 
    {
        private bool mShouldDisposeReporter = false;
        private IApprovalReportOpers mMainOpers;
        private ScheduleConfiguration mConfig;

        private Stack<ArchiveApproveReport> mTreeCache = new Stack<ArchiveApproveReport>();
        private LinkedList<ArchiveApproveReport> mProcessingNodes = new LinkedList<ArchiveApproveReport>();

        /// <summary>
        /// For Backup
        /// </summary>
        /// <param name="config"></param>
        public ApprovalReportService(ScheduleConfiguration config)
        {
            mConfig = config;
#if SELF_DEBUG
            mMainOpers = new SelfApprovalDBOpers(confg);
#elif INTER_DEBUG

            mMainOpers = ScanDBOperationFactory.GetScanDBOperation(config);
#endif

#if SELF_DEBUG
            //If EXO use the new report service class
            mMainOpers = new ApprovalReportServiceV2(config);
#endif
        }

        public void ResetRuleId(string ruleId)
        {
            mTreeCache.Clear();
            mProcessingNodes.Clear();
            mMainOpers.Reset(ruleId);
        }
        /// <summary>
        /// 存储   hasReported参数为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        public void Store(ArchiveApproveReport node, bool hasReported)
        {
            mMainOpers.AddToDB(node, hasReported);
            if (!mConfig.IsRelativeDataJob && node.ShouldAddDetails)
            {
                long nodeSize = node.DoDelete ? node.DocumentSize : 0;
                string fullPath = mConfig.GetNodeFullPath(node.FullPath);
                if (mConfig.jobtype == JobType.SOPreScan 
                    || mConfig.jobtype == JobType.DiscoveryPreScan 
                    || mConfig.jobtype == JobType.TeamsPreScan)
                {
                    mConfig.JobReportDto.AddScanReportForSimulation(fullPath, nodeSize, node.CacheNodeType, node.RuleName, node.RuleArchiverAction, node.Created, node.Author, node.Modified, node.Editor);
                    return;
                }
                if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule && node.CacheNodeType < (int)CacheNodeType.Item)
                {
                    return;
                }
                mConfig.JobReportDto.AddScanReport(fullPath, nodeSize, node.CacheNodeType, node.RuleName);
            }
        }
        /// <summary>
        /// AddReport  为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        public void AddReport(ArchiveApproveReport node)
        {
            mMainOpers.AddScanReport(node);
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns>如果是空则结束</returns>
        public BackwardDependenceNode<ArchiveApproveReport> FetchNext()
        {
            if (mProcessingNodes.Any())
            {
                return FetchNextNode();
            }

            while (true)
            {
                var tmp = mMainOpers.ReadFromDB();
                if (tmp == null)
                {
                    break;
                }

                while (mProcessingNodes.Any() && !SplitScanDBWriterOperation.ValidIsParentNode(mProcessingNodes.Last(), tmp))
                {
                    mProcessingNodes.RemoveLast();
                }
                while (!mProcessingNodes.Any() && mTreeCache.Any() && !SplitScanDBWriterOperation.ValidIsParentNode(mTreeCache.Peek(), tmp))
                {
                    mTreeCache.Pop();
                }
                mProcessingNodes.AddLast(tmp);
                if (tmp.RuleId != Guid.Empty.ToString())
                {
                    return FetchNextNode();
                }
            }

            mProcessingNodes.Clear();
            return null;
            

            BackwardDependenceNode<ArchiveApproveReport> FetchNextNode()
            {
                var res = mProcessingNodes.First();
                mProcessingNodes.RemoveFirst();
                mTreeCache.Push(res);
                return new BackwardDependenceNode<ArchiveApproveReport>()
                {
                    Level = (int)res.CacheNodeType,
                    SFThreshold = true,
                    Value = res
                };
            }
        }

        public List<string> GetDataRuleCollection()
        {
            return mMainOpers.GetDataRuleCollection();
        }

        public long GteDataCount(int minCacheNodeType = 0)
        {
            return mMainOpers.GetDataCount(minCacheNodeType);
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            return mMainOpers.GetDataCounts(minCacheNodeType, ruleId);
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return mMainOpers.ExistInScanJob(nodeIds);
        }

        public void Flush()
        {
            mMainOpers.Flush();
            //mReporter.Finish();
        }

        public void Dispose()
        {
            if (mShouldDisposeReporter)
            {
                //using (mReporter) { }
            }
            if (mMainOpers is IDisposable)
            {
                (mMainOpers as IDisposable).Dispose();
            }

        }
    }


}