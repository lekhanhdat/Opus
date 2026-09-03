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

using AvePoint.RA.SharePoint.ArchiverCommon;
using RAGoogle.Common;

namespace RAGoogle.Archive.ApprovalService
{
    internal interface IApprovalReportOpers : IDisposable
    {
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

        long GetDataCount();

        List<Guid> ExistInScanJob(List<Guid> nodeIds);
    }

    public class ApprovalReportService : IScheduleContainer<ArchiveApproveReport>
    {
        private bool mShouldDisposeReporter = false;
        private IApprovalReportOpers mMainOpers;
        private GoogleConfiguration mConfig;

        /// <summary>
        /// For Backup
        /// </summary>
        /// <param name="config"></param>
        public ApprovalReportService(GoogleConfiguration config)
        {
            mConfig = config;
            mMainOpers = new AveSqliteOperation(config);
        }

        public void ResetRuleId(string ruleId)
        {
            mMainOpers.Reset(ruleId);
        }

        public void Store(ArchiveApproveReport node, bool hasReported)
        {
            mMainOpers.AddToDB(node, hasReported);
        }

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
            ArchiveApproveReport tmp2 = mMainOpers.ReadFromDB();
            if (tmp2 != null)
            {
                BackwardDependenceNode<ArchiveApproveReport> result = new BackwardDependenceNode<ArchiveApproveReport>();
                result.Level = tmp2.CacheNodeType;
                result.SFThreshold = true;
                result.Value = tmp2;
                return result;
            }
            return null;
        }

        public List<string> GetDataRuleCollection()
        {
            return mMainOpers.GetDataRuleCollection();
        }

        public long GteDataCount()
        {
            return mMainOpers.GetDataCount();
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return mMainOpers.ExistInScanJob(nodeIds);
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