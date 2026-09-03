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
using AvePoint.Hybrid.ClientCore;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.Global.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface ISharePointOnPremJobService
    {
        [Api(Url = "api/SharePointOnPremJob/BrowseSPTreeNode", HttpMethod = "POST")]
        Task<List<RMSPTreeNode>> BrowseSPTreeNode(RMSPTreeNode node);

        [Api(Url = "api/SharePointOnPremJob/SetSettingJobTime", HttpMethod = "POST")]
        Task<bool> SetSettingJobTime(SPSettingJobInfo info);

        [Api(Url = "api/jobwebapi/getspjobmessage", HttpMethod = "POST")]
        Task<string> GetSPJobMessage(JobInfo jobInfo);

        [Api(Url = "api/SharePointOnPremJob/GetAutoJobCollectionTime", HttpMethod = "GET")]
        Task<long> GetAutoJobCollectionTime(int type, Guid folderId, Guid listId, Guid nodeId, Guid groupId);

        [Api(Url = "api/SharePointOnPremJob/UpdateAutoJobCollectionTime", HttpMethod = "POST")]
        Task<bool> UpdateAutoJobCollectionTime(List<NodeFlag> nodeFlags);

        [Api(Url = "api/OnPremiseQuerySPData/RetrieveItemOwnerMapping", HttpMethod = "POST")]
        Task<Dictionary<Guid, string>> GetItemOwnerMapping(ItemOwnerMappingDto mappingDto);

        [Api(Url = "api/OnPremiseQuerySPData/RetrieveIncrementalItemOwnerMapping", HttpMethod = "POST")]
        Task<Dictionary<Guid, string>> GetIncrementalItemOwnerMapping(IncrementalItemOwnerMappingDto mappingDto);        

        [Api(Url = "api/OnPremiseQuerySPData/GetManualNodeAndApproverMapping", HttpMethod = "GET")]
        Task<Dictionary<Guid, List<string>>> GetManualNodeAndApproverMapping(Guid siteId, List<Guid> nodeId);

        [Api(Url = "api/OnPremiseQuerySPData/GetUserByUserIds", HttpMethod = "GET")]
        Task<List<RMAccount>> GetUserByUserIds(List<string> userIds);
        [Api(Url = "api/OnPremiseQuerySPData/AddSPDataToExplorer", HttpMethod = "POST")]
        Task<RA.Contract.FileSystem.AgentSyncDataResultDto> AddSPDataToExplorer(List<RecordDto> records);

        [Api(Url = "api/OnPremiseQuerySPData/RetrieveRecordsByTerms", HttpMethod = "POST")]
        Task<List<RecordDto>> GetRecordsByTerms(QueryChangedTermItemsDto queryDto);

        [Api(Url = "api/OnPremiseQuerySPData/RemoveSPObjInExplorer", HttpMethod = "POST")]
        Task<bool> RemoveSPObjInExplorer(RemoveSPObjDto removeDto);

        [Api(Url = "api/OnPremiseQuerySPData/AddSiteFlagInfos", HttpMethod = "POST")]
        Task<bool> AddSiteFlagInfos(List<NodeFlag> nodeFlags);

        [Api(Url = "api/OnPremiseQuerySPData/AddSiteScope", HttpMethod = "POST")]
        Task<bool> AddSiteScope(RMScope site);

        [Api(Url = "api/OnPremiseQuerySPData/UpdateRecordsInExplorer", HttpMethod = "POST")]
        Task<List<Guid>> UpdateRecordsInExplorer(List<RecordDto> records);

        [Api(Url = "api/OnPremiseQuerySPData/UpdateDeletedItemsInExplorer", HttpMethod = "POST")]
        Task<bool> UpdateDeletedItemsInExplorer(List<DeleteItemDto> dtos);

        [Api(Url = "api/OnPremiseQuerySPData/FindOnPremiseSiteInfos", HttpMethod = "POST")]
        Task<Dictionary<string, SiteInfo>> GetOnPremiseSiteInfos(List<string> siteIds);

        [Api(Url = "api/OnPremiseQuerySPData/AddRecordHistory", HttpMethod = "POST")]
        Task<bool> AddRecordHistory(AvePoint.RA.Contract.Global.Explorer.RecordHistoryDto recordHistoryDto );

        [Api(Url = "api/OnPremiseQuerySPData/UpdateTermChangeItems", HttpMethod = "POST")]
        Task<bool> UpdateTermChangeItems(AvePoint.RA.Contract.Global.Explorer.TermChangeItemDto termChangeItemDto);

        [Api(Url = "api/OnPremiseQuerySPData/UpdateDeclaredItems", HttpMethod = "POST")]
        Task<bool> UpdateDeclaredItems(AvePoint.RA.Contract.Global.Explorer.DeclareItemDto declareItemDto);

        [Api(Url = "api/OnPremiseQuerySPData/AddClassificationHistory", HttpMethod = "POST")]
        Task<bool> AddClassificationHistory(List<AvePoint.RA.Contract.Global.Object.RMClassificationHistory> classificationHistories);

        [Api(Url = "api/OnPremiseQuerySPData/FindRecordsByIds", HttpMethod = "POST")]
        Task<List<RecordDto>> GetRecordsByIds(List<Guid> ids);

        [Api(Url = "api/OnPremiseQuerySPData/UpdateRealtimeJobState", HttpMethod = "POST")]
        Task<bool> UpdateRealtimeJobState(RealtimeJobState realtimeJobState);

        [Api(Url = "api/OnPremiseQuerySPData/QueryDataForGlobalSearch", HttpMethod = "POST")]
        Task<AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryResult> QueryDataForGlobalSearch(AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryDto dto);
    }
}
