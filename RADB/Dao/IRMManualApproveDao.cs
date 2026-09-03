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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMManualApproveDao : IBaseDao<RMManualApprove>
    {
        void SaveManualApprove(RMManualApprove datas);

        void SaveArchivedManualApprove(RMManualApprove manualApprove);

        List<string> GetManualApproveOwnerNames(RMManualApprove manualApprove);

        IEnumerable<List<RMManualApprove>> GetManualDatas(int limit = 1000);

        bool HasData();

        bool HasWorkflowData();

        List<int> GetManualApproveOwnerIds(RMManualApprove manualApprove);

        Task UpdateManualApproveActionStatusAsync(string partionKey, string rowKey, int actionStatus);

        List<RMManualApprove> GetAllApproveOrRejectedData(SourceFlag dataType);

        List<T> GetFilterList<T>(Expression<Func<RMManualApprove, T>> selectLambda, Expression<Func<RMManualApprove, bool>> whereLambda);
        List<RMManualApprove> GetAllData(ViewTab viewTab, int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, DateTime? startTime, DateTime? endTime, Expression<Func<RMManualApprove, bool>> whereLambda = null);
        QueryResult GetAllDataInJob(ViewTab viewTab,out int totalRecord, int pageIndex,int pageSize, DateTime? startTime, DateTime? endTime, Expression<Func<RMManualApprove, bool>> whereLambda = null);
        List<RMManualApprove> GetWaitingApprovalData();
        List<RMManualApprove> GetAllDatas(SourceFlag flag = SourceFlag.None);
        List<RMManualApprove> GetDatasByPager(int pageIndex, int pageSize, ref int totalCount, Expression<Func<RMManualApprove, bool>> whereLambda = null);
        List<long> GetAllCollectionTime(int pageIndex, int pageSize, ref int totalCount, SourceFlag flag);
        int GetWaitingCount(SourceFlag flag);

        List<RMManualApprove> GetManualApproveByNodes(Guid siteId, List<Guid> nodeId);
        Dictionary<Guid, List<string>> GetManualNodeAndApproverMapping(Guid siteId, List<Guid> nodeId);

        //List<Guid> GetAllStepIDList();
        List<string> GetOwnerExceptWorkflow(int pageIndex, int pageSize, ref int totalCount);
        Dictionary<string, int> GetUserAndWaitingReviewCountMapping();

        void UpdateManualApproveActionStatus(List<int> ids, int status, string approvedBy);
        void UpdateManualApproveDisposalAction(List<int> ids, RelatedRecordOption relatedRecordAction);
        List<RMManualApprove> GetExportData(bool isAdmin, string accountId = "");
        object GetTabInfo(Expression<Func<RMManualApprove, bool>> filterUserLambda);
        ManualReviewInfo GetAuditInfos(Guid SiteId, Guid NodeId,bool isFSSource=false);
        string GetLastReviewedUserIds(Guid SiteId, Guid NodeId);
        Dictionary<Guid, string> GetLastReviewedUserIdsByScope(Guid SiteId);
        void SaveManualApproveForPhysical(RMManualApprove manualApprove);
        void SaveManualApproveItem(RMManualApprove manualApprove);
        void SaveManualApproveForFS(RMManualApprove manualApprove);
        List<Guid> GetAllInstanceIds();

        //Dictionary<string ,string> GetOwnersFilterList<T>(Expression<Func<RMManualApprove, T>> selectLambda, Expression<Func<RMManualApprove, bool>> whereLambda);
    }
}
