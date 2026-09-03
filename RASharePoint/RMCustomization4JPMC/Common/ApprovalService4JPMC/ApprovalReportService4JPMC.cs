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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Common.ApprovalService4JPMC
{
    internal interface IApprovalReportOpers4JPMC : IDisposable
    {
        /// <summary>
        /// 向数据库中加入一条记录。需要注意更新相关的servcie信息表。
        /// </summary>
        /// <param name="detail"></param>
        void AddToDB(ArchiveApproveReport4JPMC nodeEntity, bool hasReported);
        /// <summary>
        /// AddScanReport 为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="detail"></param>
        void AddScanReport(ArchiveApproveReport4JPMC nodeEntity);
        /// <summary>
        /// 从数据库中读出一条由Reset函数指定的相关的service的记录。
        /// </summary>
        /// <returns>如果读取完毕， 返回空</returns>
        ArchiveApproveReport4JPMC ReadFromDB();
        /// <summary>
        /// 初始化指定一次迭代读取数据库中对应的svcId对应的记录
        /// </summary>
        /// <param name="ruleId"></param>
        void Reset(string ruleId);

        List<Guid> ExistInScanJob(List<Guid> nodeIds);
        List<ArchiveApproveReport4JPMGroupBy> ReadFromApproveDBGroupByColumns(string ruleId, string listId);

        List<ArchiveApproveReport4JPMTotalSize> ReadFromApproveDBTotalSize(string ruleId, string listId);
        void DeleteByWebId(Guid webId);
        void DeleteByList(Guid webId, Guid listId);
        void DeleteByNodeIds(List<Guid> nodeIds);
        void DeleteByParentIds(List<Guid> parentIds);
    }

    public class ApprovalReportService4JPMC : IScheduleContainer<ArchiveApproveReport4JPMC>
    {
        private bool mShouldDisposeReporter = false;
        private IApprovalReportOpers4JPMC mMainOpers;
        private ScheduleConfiguration mConfig;

        /// <summary>
        /// For Backup
        /// </summary>
        /// <param name="config"></param>
        public ApprovalReportService4JPMC(ScheduleConfiguration config)
        {
            mConfig = config;
#if SELF_DEBUG
            mMainOpers = new SelfApprovalDBOpers(confg);
#elif INTER_DEBUG

            mMainOpers = new SqliteOperation4JPMC(config);
#endif

#if SELF_DEBUG
            //If EXO use the new report service class
            mMainOpers = new ApprovalReportServiceV2(config);
#endif
        }

        public void ResetRuleId(string ruleId)
        {
            mMainOpers.Reset(ruleId);
        }
        /// <summary>
        /// 存储   hasReported参数为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        public void Store(ArchiveApproveReport4JPMC node, bool hasReported)
        {
            mMainOpers.AddToDB(node, hasReported);
        }
        /// <summary>
        /// AddReport  为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        public void AddReport(ArchiveApproveReport4JPMC node)
        {
            mMainOpers.AddScanReport(node);
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns>如果是空则结束</returns>
        public BackwardDependenceNode<ArchiveApproveReport4JPMC> FetchNext()
        {
            ArchiveApproveReport4JPMC tmp2 = mMainOpers.ReadFromDB();
            if (tmp2 != null)
            {
                BackwardDependenceNode<ArchiveApproveReport4JPMC> result = new BackwardDependenceNode<ArchiveApproveReport4JPMC>();
                result.Level = tmp2.CacheNodeType;
                result.SFThreshold = true;
                result.Value = tmp2;
                return result;
            }
            return null;
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return mMainOpers.ExistInScanJob(nodeIds);
        }

        public List<ArchiveApproveReport4JPMGroupBy> ReadFromApproveDBGroupByColumns(string ruleId, string listId = "")
        {
            return mMainOpers.ReadFromApproveDBGroupByColumns(ruleId, listId);
        }
        
        public List<ArchiveApproveReport4JPMTotalSize> ReadFromApproveDBTotalSize(string ruleId, string listId = "")
        {
            return mMainOpers.ReadFromApproveDBTotalSize(ruleId, listId);
        }

        public void DeleteWebData(Guid webId)
        {
            if (webId == Guid.Empty)
            {
                return;
            }

            mMainOpers.DeleteByWebId(webId);
        }

        public void DeleteListData(Guid webId, Guid listId)
        {
            if (listId == Guid.Empty)
            {
                return;
            }

            mMainOpers.DeleteByList(webId, listId);
        }

        public void DeleteItemData(List<Guid> nodeIds)
        {
            if (nodeIds == null || nodeIds.Count == 0)
            {
                return;
            }

            mMainOpers.DeleteByNodeIds(nodeIds);
        }

        public void DeleteFolderData(List<Guid> folderNodeIds)
        {
            if (folderNodeIds == null || folderNodeIds.Count == 0)
            {
                return;
            }

            mMainOpers.DeleteByParentIds(folderNodeIds);
        }

        public void Flush()
        {
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