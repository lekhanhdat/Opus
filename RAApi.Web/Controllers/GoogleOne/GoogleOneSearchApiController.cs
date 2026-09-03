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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.PermissionManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/search")]
    public class GoogleOneSearchApiController : GoogleOneApiBaseController
    {
        private IRALogger _logger = RALogger.GetInstance(typeof(GoogleOneSearchApiController));
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

        [HttpPost("querypager")]
        public async Task<string> QueryPager([FromBody] ExplorerQueryV3Dto dto)
        {
            try
            {
                var canConvert2BasicSearch = dto.QueryOption.CanConvertBasicSearchCriteria();
                var allAvaliableSourceFlags = await SecurityTrimmingHelper.GetAllAvailableSourceFlagsFromDbAsync();
                var canDoAction = allAvaliableSourceFlags.Count == 1 ? true : dto.QueryOption.CanDoGlobalAction();
                var queryResut = await ExplorerQueryService.QueryDataListWithTotalAsync(dto);
                var result = new ExplorerResultInfoV3
                {
                    CanConvert2BasicSearch = canConvert2BasicSearch,
                    CanDoGlobalAction = canDoAction,
                    Datas = queryResut.Datas,
                    PagingInfo = queryResut.PagingInfo
                };
                 await ConvertCreateTime(result.Datas, TenantLocalValue.TimezoneId);
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occured while querying GoogleOne global search data: {ex.Message}.");
                return JsonConvert.SerializeObject(new ExplorerResultInfoV3());
            }
        }

        public async Task ConvertCreateTime(List<BaseRecordDto> datas, string timeZoneId)
        {
            if (datas == null || datas.Count == 0) return;

            var targetTz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var gls = await mGeneralSettingService.GetGeneralSettingAsync();
            var sourceTzId = gls.TimeZoneId;

            foreach (var item in datas)
            {
                if (!string.IsNullOrEmpty(item.TimeCreatedStr))
                {
                    
                    item.TimeCreatedStr = DateTimeUtil.GetFormattedTimeBetweenTimezones(DateTimeUtil.RemoveTimeUtcSuffix(item.TimeCreatedStr, AveDateTimeUtility.DATETYPE011), sourceTzId, targetTz.Id, AveDateTimeUtility.DATETYPE011, true);
                }

                if (!string.IsNullOrEmpty(item.TimeLastModifiedStr))
                {
                    item.TimeLastModifiedStr = DateTimeUtil.GetFormattedTimeBetweenTimezones(DateTimeUtil.RemoveTimeUtcSuffix(item.TimeLastModifiedStr, AveDateTimeUtility.DATETYPE011), sourceTzId, targetTz.Id, AveDateTimeUtility.DATETYPE011, true);
                }
            }
        }

        [HttpPost("doaction")]
        public async Task<string> DoAction([FromBody] GlobalSearchActionDto actionDto)
        {
            RAReturnMessage message = await VialidateParameterAsync(actionDto);
            if (message.MessageType == RAMessageType.Successful)
            {
                if (actionDto.IsRealTimeAction)
                {
                    message = ExplorerService.DoGlobalSearchRealTimeAction(actionDto);
                }
                else
                {
                    message = ExplorerService.StartGlobalSearchActionJob(actionDto);
                }
            }
            return JsonConvert.SerializeObject(message);
        }
        private async Task<RAReturnMessage> VialidateParameterAsync(GlobalSearchActionDto actionDto)
        {
            var returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                switch (actionDto.Action)
                {
                    case GlobalSearchAction.AccessControl:
                        GSPermissionSimpleDto simpleDto = JsonConvert.DeserializeObject<GSPermissionSimpleDto>(actionDto.ActionExtension.ToString());
                        var syncUserResult = await PermissionManagementService.SyncADUsersAsync(simpleDto.Accounts);
                        if (syncUserResult.MessageType != RAMessageType.Successful)
                        {
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = syncUserResult.ErrorMessage;
                            return returnMessage;
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(await GetJobContextDtoAsync(simpleDto));
                        break;
                    case GlobalSearchAction.DeclareRecords:
                    case GlobalSearchAction.UnDeclareRecords:
                        actionDto.ActionExtension = WebUtil.LogOnUserName;
                        break;
                    case GlobalSearchAction.MoveTo:
                        break;
                    case GlobalSearchAction.Reclassify:
                        ChangeTermDto changeTermDto = JsonConvert.DeserializeObject<ChangeTermDto>(actionDto.ActionExtension.ToString());
                        RMTerm selectedTerm = new();
                        selectedTerm = TermDao.GetRMTermByUniqueId(changeTermDto.TermInfo.UniqueId, false);
                        if (selectedTerm.IsDeprecated || selectedTerm.IsExpired || changeTermDto.TermInfo == null)
                        {
                            string message = I18NEntity.GetString("RM_JS_JMD_Comment_Auto_TermNotAvailable");
                            returnMessage.ErrorMessage = message;
                            returnMessage.MessageType = RAMessageType.Failed;
                            return returnMessage;
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(GetChangeTermOption(changeTermDto));
                        break;
                    case GlobalSearchAction.PhysicalBulkUpdate:
                        Dictionary<string, string> physicalUpdateDto = JsonConvert.DeserializeObject<Dictionary<string, string>>(actionDto.ActionExtension.ToString());
                        if (physicalUpdateDto.Keys != null)
                        {
                            if (DefaultColumnIDs.HideForBulkUpdateIDs.Any(c => physicalUpdateDto.Keys.Contains(c)))
                            {
                                string message = "Edit Column Invalid Physical Objects";//Not displayed in gui
                                returnMessage.ErrorMessage = message;
                                returnMessage.MessageType = RAMessageType.Failed;
                                return returnMessage;
                            }
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(physicalUpdateDto);
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while validating parameter. Error{e.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage= e.Message;
            }
            return returnMessage;
        }
        private async Task<ScopePermissionJobContextDto> GetJobContextDtoAsync(GSPermissionSimpleDto dto)
        {
            var jd = new ScopePermissionJobContextDto
            {
                GSJobContextDto = new GSPermissionJobContextDto
                {
                    UserId = TenantLocalValue.LogonUserId,
                    //目前只支持打破继承
                    IsInheritSave = false,
                    //权限类型暂时是All权限
                    PermissionType = RMScopePermissionEnum.All,
                    //Search Result方式设置权限，Query参数
                    QueryDto = dto.QueryDto,

                    QueryV3Dto = dto.QueryV3Dto,
                    //UI选中的Physical数据
                    NodeIds = dto.NodeIds,
                    //对于已经打破继承的数据，批量设置权限User时，是Append还是Overwrite
                    UserConflictOption = dto.UserConflictOption
                }
            };

            var accountIds = new List<int>();
            var uiAccounts = dto.Accounts;
            if (uiAccounts != null && uiAccounts.Count > 0)
            {
                accountIds = uiAccounts.Select(o => o.RMUserId).Distinct().ToList();
            }
            //UI设置的Permission Users
            jd.GSJobContextDto.AccountIds = accountIds;

            if (jd.GSJobContextDto.QueryDto != null)
            {
                //获取EndUser权限Id集合赋值到QueryDto中，确保Job中查询的数据都是EndUser有权限的
                jd.GSJobContextDto.QueryDto.PermissionIds = await ExplorerService.GetPermissionConditionAsync();
                jd.GSJobContextDto.QueryDto.IsForGlobalSearchJob = true;
            }
            return jd;
        }
        private ChangeTermOption GetChangeTermOption(ChangeTermDto changeTermInfo)
        {
            ChangeTermOption ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds,
                SourceFSRecordIds = changeTermInfo.FSRecordIds,
                SourceEXORecordIds = changeTermInfo.EXORecordIds,
                SourcePhyRecordIds = changeTermInfo.PhyRecordIds,
                SourceSPOnPremRecordIds = changeTermInfo.SPOnPremRecordIds,
                SourceOneDriveRecordIds = changeTermInfo.OneDriveRecordIds,
                GoogleDriveRecordIds = changeTermInfo.GoogleDriveRecordIds,
                SourceTeamsRecordIds = changeTermInfo.TeamsRecordIds,
                TargetTermId = changeTermInfo.TermInfo.Id,
                TargetTermName = changeTermInfo.TermInfo.Name,
                TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                ReclassifySubFiles = changeTermInfo.ReclassifySubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeTermInfo.Comment
            };
            return ChangeTermOption;
        }

        [HttpPost("loadallcolumns")]
        public async Task<string> LoadAllColumns([FromBody] LoadTemplateColumn4DisplayParam param)
        {
            var result = await TemplateManagementService.GetAllColumnsAsync();
            if (param.LoadAll)
                return JsonConvert.SerializeObject(result.Where(o => o.UniqueId != new Guid(DefaultColumnIDs.Barcode)).ToList());
            return JsonConvert.SerializeObject(result.Where(o => param.ColumnTypes.Contains(o.ColumnType) && o.UniqueId != new Guid(DefaultColumnIDs.Barcode)).ToList());
        }
    }
}
