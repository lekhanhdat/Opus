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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Myhub.Model;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.MyHub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub
{
    public interface IRMMyhubServices
    {
        Task<RMMyhubDriveItemResult> GetMyhubDriveItemsAsync(RMMyhubDriveQueryInfo queryInfo);
        Task<RMMyhubDriveDirectionItem> GetNodeInfoByPartitionKeyAsync(RMMyhubDriveDirectionQueryInfo queryInfo);
        Task<RMMyhubTreeFolderItemResult> GetMyhubTreeFoldersAsync(RMMyhubTreeChildFolderQueryInfo queryInfo);
        Task<RMMyhubFolderAndFileItemResult> GetMyhubFolderAndItemsAsync(RMMyhubFolderItemQueryInfo queryInfo);
        Task<List<RMMyhubFolderStatisticsInfo>> GetFolderStatisticsAsync(RMMyhubFolderStatisticsQueryInfo queryInfo);


        #region ClassCode
        List<string> ReadAllClassCodeName();

        Task<List<RMMyhubClassCodeItem>> ReadClassCodeNameByPartitionKeyIds(ReadAllClassCodeNameReq req);
        Task<List<RMMyhubClassCodeCascadeDataDto>> ReadClassifyDataByPartitionKeyIds(ReadAllClassCodeNameReq req);
        List<string> ReadAllCountryCodeName();
        #endregion
        Task<RMMyhubDriveVolumeItem> GetDrivesVolumeAsync();
        Task<RMMyhubFolderDetailTableItem> GetMyhubFolderDetailAsync(RMMyhubFolderDetailTableQueryInfo queryInfo);

        #region Classify
        List<string> ReadCountryCodeByClassCode(string ClassCode);
        List<string> ReadRetentionType();
        Task<List<MyhubClassifyReturnMessage>> UpdateMyhubClassifyAsync(RMMyhubClassifyQueryInfo queryInfo);
        Dictionary<string, RMMyhubClassifyQueryInfo> MultiGeoSeperateRequestRMMyhubClassifyQueryInfo(RMMyhubClassifyQueryInfo queryInfo, Dictionary<string, IEnumerable<string>> connectionIdsByDataCenter);
        Task<RMMyhubClassifyDto> UpdateMyhubClassifyReturnValueAsync(RMMyhubClassifyReturnInfo queryInfo);
        #endregion
        Task<FSAuditQueryResult> QueryAuditTrailAsync(RMMyhubAuditTrialQueryInfo queryInfo);
        RMMyhubAuditTrialFilterItem QueryAuditTrialFilter();

        #region Permission
        Task<RMConnectionPermissions> GetConnectionPermissionAsync(Guid connectionId);

        RMConnectionAddUserPageInfo SearchAvaliableOwners(string tenantId, string key);

        Task<bool> UpdateConnectionRecordOwners(RMConnectionRecordOwnerUpdateModel updateModels);
        Task<bool> UpdateConnectionRecordOwnersForOtherDC(RMConnectionRecordOwnerUpdateModel updateModels);
        #endregion
        bool RunFSMyHubDashboardJob(JobRunBy runBy, FileSystemMyhubSelectedNodeDto selectedNode);
        Task<FSDashboardInformation> GetMyHubDashboardDataAsync(RMMyHubFolderDashboard queryInfo);
        Task<int> GetPendingDisposalVolumeAsync(RMMyhubPendingDisposalQueryInfo queryInfo);
        Task<List<FSConnectionPermission>> GetConnectionPermission();

        Task<Dictionary<Guid, int>> GetChildFolderPendingDisposalVolumeByNodeIdAsync(RMMyhubPendingDisposalQueryInfo queryInfo);
        Task<RMMyhubPendingDisposalFolderFilterResult> GetPendingDisposalFolderFilterAsync(RMMyhubPendingDisposalFolderFilterQueryInfo queryInfo);
        Task<string> GetPendingDisposalFolderFilterPathAsync(string partitionKeyId, string nodeId, bool isFullPath = false);
        Task<RMMyhubParameterBeforePendingDisposalQuery> GetParameterBeforeUnderReviewQueryAsync(RMMyhubPendingDisposalQueryInfo queryInfo);

        Task<RAReturnMessage> UpdateConnectoinIsPauseAsync(PauseOrResumeReq req);
        Dictionary<string, RMMyhubDriveSettings> GetMyhubDriveSettings(List<RMMyhubDriveQuerySettings> queryInfos);
        Task<RAReturnMessage> DeleteReportContentAsync(List<Guid> jobIds, int reportType);
        Task<RMMyhubReportDownloadResponse> DownloadReportContentMyhub(RMMyhubReportQueryInfo queryInfo);
        List<RMMyhubReportAuditItem> GetMyhubReports(List<Guid> jobIds, int reportType, bool isMyhub = true);
        bool CurrentNodeIsDisableDownloadRCC(string connectionGroupId, string folderPath);
        Task<RAReturnMessage> CheckOwnerPermissionAsync(Guid connectionId, int reportType);
    }
}
