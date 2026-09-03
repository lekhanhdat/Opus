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
//using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IExplorerQueryService
    {


        //void AddScopeData(ScopeDto dto);
        //long GetCollectionTime(Guid scopeId);

        /// <summary>
        /// get data by query dto for UI pager, will not return the total number
        /// </summary>
        /// <param name="dto">query dto</param>
        /// <returns></returns>
        Task<ExplorerResultInfo> QueryDataListWithoutTotalAsync(ExplorerQueryV2Dto dto);

        /// <summary>
        /// will return the query data and total number.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="convertMetaInfo"></param>
        /// <returns></returns>
        Task<ExplorerResultInfo> QueryDataListWithTotalAsync(ExplorerQueryV2Dto dto, bool convertMetaInfo = false);

        /// <summary>
        /// for offline search
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ExplorerFilterOptionV2> PrepareFilterV2Async(ExplorerQueryV3Dto dto);
        /// <summary>
        /// will get data without checking permission
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="convertMetaInfo"></param>
        /// <returns></returns>
        Task<ExplorerResultInfo> QueryDataListWithoutTotalDirectlyAsync(ExplorerQueryV2Dto dto, bool convertMetaInfo = false);

        Task<ExplorerResultInfo> QueryOfflineSearchDataAsync(ExplorerOfflineResultQueryDto dto);

        /// <summary>
        /// Advanced search, will return the query data and total number.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ExplorerResultInfo> QueryDataListWithTotalAsync(ExplorerQueryV3Dto dto);
        Task<ExplorerResultInfo> QueryAdvancedDataListWithTotalAsync(ExplorerQueryV3Dto dto, bool suggestionSearch = false);

        /// <summary>
        /// Advanced search, will return the query data without total count.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ExplorerResultInfo> QueryDataListWithoutTotalAsync(ExplorerQueryV3Dto dto);


        List<Guid> GetSecurityTerms(SecurityTermPermissionDto termPremDto);
        Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync();
        Task<ExplorerResultInfo> QueryDataListWithoutTotalCustomAsync(ExplorerQueryV3Dto dto, ExplorerFilterOptionV2 builtinFilterOption = null, bool returnTotalCount = false, bool convertMetaInfo = false);
        //void ProcessWithoutNodeTypeParam(ExplorerFilterOptionV2 filterOption);
    }
}
