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
using Aspose.Words.Lists;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.PersonalSetting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client.RecordsRepository;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/recordexplorer")]
    public class GoogleOneRecordExplorerApiController : GoogleOneApiBaseController
    {
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IPersonalSettingService PersonalSettingService => PlatformWindsorManager.GetService<IPersonalSettingService>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();

        [HttpPost("changeterm")]
        public async Task<string> ChangeTermAsync(ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeGoogleTermAsync(termDto));
        }

        [HttpPost("loaddetails")]
        public async Task<string> LoadDetailsAsync(DetailQueryDto dto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.LoadDetailByKeyAsync(dto.status, dto.Id, dto.tab, true));
        }

        [HttpPost("checkItemsInTheSameSecurityGroup")]
        public async Task<bool> CheckItemsInTheSameSecurityGroup(List<Guid> recordIds)
        {
            return ExplorerService.CheckItemsInTheSameSecurityGroup(recordIds);
        }
        [HttpGet("getrealtimejobstatusinfo")]
        public async Task<string> GetRealTimeJobStatusInfo(string jobId)
        {
            return JsonConvert.SerializeObject(ExplorerService.GetRealTimeJobStatusInfo(jobId));
        }

        [HttpPost("queryofflinejob")]
        public async Task<String> QueryOfflineSearchData([FromBody] ExplorerOfflineResultQueryDto dto)
        {
            var result = new ExplorerResultInfoV3
            {
                CanConvert2BasicSearch = false,
                CanDoGlobalAction = false,
                Datas = null,
                PagingInfo = null
            };
            
            RMPersonalSettingDto profile = PersonalSettingService.GetById(dto.ProfileId, true);
            ExplorerQueryV3Dto queryV3Dto = AssembleQueryDto(profile);
            if (queryV3Dto != null)
            {
                var canConvert2BasicSearch = queryV3Dto.QueryOption.CanConvertBasicSearchCriteria();

                var allAvaliableSourceFlags = await SecurityTrimmingHelper.GetAllAvailableSourceFlagsFromDbAsync();
                var canDoAction = allAvaliableSourceFlags.Count == 1 ? true : queryV3Dto.QueryOption.CanDoGlobalAction();
                var queryResult = await ExplorerQueryService.QueryOfflineSearchDataAsync(dto);
               
                queryResult.PagingInfo.HasNextPage = queryResult.PagingInfo.Total > (int.Parse(queryResult.PagingInfo.PageIndex) + 1) * queryResult.PagingInfo.PageSize;
                result = new ExplorerResultInfoV3
                {
                    CanConvert2BasicSearch = canConvert2BasicSearch,
                    CanDoGlobalAction = canDoAction,
                    Datas = queryResult.Datas,
                    PagingInfo = queryResult.PagingInfo
                };
            }
            return JsonConvert.SerializeObject(result);
        }

        private ExplorerQueryV3Dto AssembleQueryDto(RMPersonalSettingDto profile)
        {
            if (profile != null)
            {
                ExplorerQueryV3Dto queryV3Dto = new ExplorerQueryV3Dto();

                RMExplorerSearchCriteriaSetting setting = SerializerHelper.DeserializeByJsonConvert<RMExplorerSearchCriteriaSetting>(profile.ContentStr);
                if (setting != null && setting.AdvancedSearchs != null)
                {
                    ExplorerQueryOptionV3 optionV3 = new ExplorerQueryOptionV3() { Values = new List<ExplorerSearchOptionV3>() };
                    if (!string.IsNullOrEmpty(setting.ColumnSortSetting))
                    {
                        ExplorerQueryOrderColumn orderColumn = SerializerHelper.DeserializeByJsonConvert<ExplorerQueryOrderColumn>(setting.ColumnSortSetting);
                        optionV3.OrderColumn = orderColumn;
                    }
                    foreach (var option in setting.AdvancedSearchs)
                    {
                        if (!string.IsNullOrEmpty(option.ContentStr))
                        {
                            ExplorerSearchOptionV3 searchOption = SerializerHelper.DeserializeByJsonConvert<ExplorerSearchOptionV3>(option.ContentStr);
                            optionV3.Values.Add(searchOption);
                        }
                    }
                    queryV3Dto.QueryOption = optionV3;
                    return queryV3Dto;
                }
            }
            return null;
        }
    }
}
