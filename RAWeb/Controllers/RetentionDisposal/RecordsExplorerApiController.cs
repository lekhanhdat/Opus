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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Utility;
using AvePoint.Nintex.O365API.Json;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.RMTasks;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.RA.Service.Services.MyHub;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Web.Controllers.RetentionDisposal
{
    [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, preferred: false)]
    public class RecordsExplorerApiController : BaseApiController
    {
        private const int ExportHoldLimitCount = 10;

        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);
        private IExplorerQueryService _ExplorerQueryService;
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService(ref _ExplorerQueryService);
        private IPersonalSettingService _PersonalSettingService;
        private IPersonalSettingService PersonalSettingService => PlatformWindsorManager.GetService(ref _PersonalSettingService);
        private IArchivedContentDownloadService _ArchivedContentDownloadService;
        private IArchivedContentDownloadService ArchivedContentDownloadService => PlatformWindsorManager.GetService(ref _ArchivedContentDownloadService);
        private IDownloadDataInfoDao _DownloadDataInfoDao;
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService(ref _DownloadDataInfoDao);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService<IRMMyhubServices>();
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();
        private IWorkspaceHoldService _WorkspaceHoldService;
        private IWorkspaceHoldService WorkspaceHoldService => PlatformWindsorManager.GetService(ref _WorkspaceHoldService);
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser)]
        [ValidTermTreeParameterFilter("QueryDataListV2")]
        public async Task<string> QueryDataListWithTotalV2([FromBody] ExplorerQueryV2Dto dto)
        {
            //System.Threading.Thread.Sleep(10000); //just for big data test
            return JsonConvert.SerializeObject(await ExplorerQueryService.QueryDataListWithTotalAsync(dto,false));
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<ExplorerResultInfoV3> QueryOfflineSearchData([FromBody] ExplorerOfflineResultQueryDto dto)
        {

            RMPersonalSettingDto profile = PersonalSettingService.GetById(dto.ProfileId, true);
            ExplorerQueryV3Dto queryV3Dto = AssembleQueryDto(profile);
            if(queryV3Dto != null)
            { 
                var canConvert2BasicSearch = queryV3Dto.QueryOption.CanConvertBasicSearchCriteria();

				//var allAvaliableSourceFlags = SourceFlagHelper.GetAllSourceFlags();
				//var userPermission = SecurityTrimmingHelper.GetUserPermission<RMPermissionMasks>(false);
				//allAvaliableSourceFlags = userPermission.RemoveNoPermissionFourceFlags(allAvaliableSourceFlags);
				var allAvaliableSourceFlags = await SecurityTrimmingHelper.GetAllAvailableSourceFlagsFromDbAsync();
				var canDoAction = allAvaliableSourceFlags.Count == 1 ? true : queryV3Dto.QueryOption.CanDoGlobalAction();
                var canDoPhysicalBulkUpdate = queryV3Dto.QueryOption.CanDoPhysicalBulkUpdate();
                //System.Threading.Thread.Sleep(10000); //just for big data test
                var queryResut = await ExplorerQueryService.QueryOfflineSearchDataAsync(dto);
                var result = new ExplorerResultInfoV3
                {
                    CanConvert2BasicSearch = canConvert2BasicSearch,
                    CanDoGlobalAction = canDoAction,
                    CanDoPhysicalBulkUpdate = canDoPhysicalBulkUpdate,
                    Datas = queryResut.Datas,
                    PagingInfo = queryResut.PagingInfo
                };
                return result;
            }
            return new ExplorerResultInfoV3
            {
                CanConvert2BasicSearch = false,
                CanDoGlobalAction = false,
                Datas = null,
                PagingInfo = null
            }; 
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

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<List<CustomizeConnectorNameValue<int>>> GetAvaliableSourceFlagsFromDb()
        {
            var dataSources = await SecurityTrimmingHelper.GetAllAvailableDataSourceFromDbAsync();
            return dataSources.ConvertAll(item => new CustomizeConnectorNameValue<int>
            {
                Name = item.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn ? I18NEntity.GetString(item.Name) : item.Name,
                Value = item.Flag
            }).OrderBy(item =>
            {
                if (DashboardConfig.SourceFlagOrder.TryGetValue((SourceFlag)item.Value, out var result))
                {
                    return result;
                }
                return item.Value;
            }).ToList();
        }

        /// <summary>
        /// check license
        /// </summary>
        /// <param name="sourceFlags"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<List<SourceFlag>> GetAvaliableSourceFlags([FromBody] List<SourceFlag> sourceFlags)
        {
            return  (await SecurityTrimmingHelper.GetAvailableDataSourceAsync()).ToList();
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public string CancelHoldByRecords([FromBody] ChangeHoldDto dto)
        {
            RAReturnMessage returnMessage = ExplorerService.CancelHoldByRecords(dto.recordsId, dto.isPhysical);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while records cancel hold setting,record ids is{0}", string.Join(",", dto.recordsId));
                return returnMessage.ErrorMessage;
            }
            Logger.Info("create records cancel hold setting,name:{0}", string.Join(",", dto.recordsId));
            return string.Empty;
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public string CancelSelectedHoldByRecords([FromBody] ChangeHoldDto dto)
        {
            RAReturnMessage returnMessage = ExplorerService.CancelHoldByRecords(dto.recordsId, dto.isPhysical, dto.removeHoldIds);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while records cancel hold setting,record ids is{0}", string.Join(",", dto.recordsId));
                return returnMessage.ErrorMessage;
            }
            Logger.Info("create records cancel hold setting,name:{0}", string.Join(",", dto.recordsId));
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<RAReturnMessage> DownLoadReportJob([FromBody] List<string> holdIds)
        {
            if (holdIds != null && holdIds.Count > ExportHoldLimitCount)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = string.Format(I18NEntity.GetString("RM_HS_Export_DataLimit"), ExportHoldLimitCount)
                };
            }

            return await ExplorerService.RunExportHoldRecordsJobAsync(JobRunBy.Control, holdIds);
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public string SusPendRecords([FromBody] UpdateHoldDto dto)
        {
            //if (dto.HoldCategory == RecordsConstants.RecordHold_Default)
            //{
            //    dto.HoldCategory = RecordsConstants.RecordHold_Electronic;
            //}
            RAReturnMessage returnMessage = ExplorerService.SusPendRecords(dto, dto.AllFolder);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while suspend  records ,record ids is{0}", string.Join(",", dto.ReletedIds));
                return returnMessage.ErrorMessage;
            }
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public string SusPendHolds([FromBody] UpdateHoldDto dto)
        {
            //if (dto.HoldCategory == RecordsConstants.RecordHold_Default)
            //{
            //    dto.HoldCategory = RecordsConstants.RecordHold_Electronic;
            //}
            RAReturnMessage returnMessage = ExplorerService.SusPendHolds(dto);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while suspend  records ,record ids is{0}", string.Join(",", dto.ReletedIds));
                return returnMessage.ErrorMessage;
            }
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<string> CreateHold([FromBody] UpdateHoldDto dto)
        {
            if (dto.HoldCategory != RecordsConstants.RecordHold_Default)
            {
                Logger.Warn("Hold category is not default, set to default");
                dto.HoldCategory = RecordsConstants.RecordHold_Default;
            }
            if (dto.HoldSetting != null && dto.HoldSetting.ProfileType != HoldProfileType.All)
            {
                Logger.Warn("Hold ProfileType is not all, set to all");
                dto.HoldSetting.ProfileType = HoldProfileType.All;
            }
            dto.HoldSetting.Id = Guid.NewGuid().ToString();
            var result = await ExplorerService.CreateHoldAsync(dto);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                await ExplorerService.BuildHoldNotificationScheduleJob(dto);
                return string.Empty;
            }
            return result.ErrorMessage;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any ,PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<string> EditHold([FromBody] UpdateHoldDto dto)
        {
            var result = await ExplorerService.EditHoldAsync(dto);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                await ExplorerService.BuildHoldNotificationScheduleJob(dto);
                return string.Empty;
            }
            return result.ErrorMessage;
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<string> ChangeHoldCreate([FromBody] UpdateHoldDto dto)
        {
            if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
            {
                var cannotHold = ExplorerService.IsFolderHasParentHold(dto.ReletedIds, out List<string> holdingBoxes);
                if (cannotHold)
                {
                    return I18N.Core.I18NEntity.GetString("RM_JS_RDM_Hold_PreventHoldByBox", string.Join(",", holdingBoxes));
                }
            }
            if (dto.HoldSetting.Id == null)
            {
                dto.HoldSetting.Id = Guid.NewGuid().ToString();
            }
            if (dto.HoldCategory != RecordsConstants.RecordHold_Default)
            {
                Logger.Warn("Hold category is not default, set to default");
                dto.HoldCategory = RecordsConstants.RecordHold_Default;
            }
            if (dto.HoldSetting != null && dto.HoldSetting.ProfileType != HoldProfileType.All)
            {
                Logger.Warn("Hold ProfileType is not all, set to all");
                dto.HoldSetting.ProfileType = HoldProfileType.All;
            }
            RAReturnMessage returnMessage = await ExplorerService.ChangeHoldCreateAsync(dto);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while create hold with record,name:{0},record id :{1},ERROR:{2}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            await ExplorerService.BuildHoldNotificationScheduleJob(dto);
            Logger.Info("create hold success,name:{0},record id :{1}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds));
            return string.Empty;
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public string ChangeHoldReuse([FromBody] UpdateHoldDto dto)
        {
            //if (dto.HoldCategory == RecordsConstants.RecordHold_PhyProfile)
            if(ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
            {
                var cannotHold = ExplorerService.IsFolderHasParentHold(dto.ReletedIds, out List<string> holdingBoxes);
                if (cannotHold)
                {
                    return I18N.Core.I18NEntity.GetString("RM_JS_RDM_Hold_PreventHoldByBox", string.Join(",", holdingBoxes));
                }
            }           
            RAReturnMessage returnMessage = ExplorerService.ChangeHoldReuse(dto);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while reuse hold with record,name:{0},record id :{1},ERROR:{2}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("reuse hold success,name:{0},record id :{1}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds));
            return string.Empty;
        }


        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public async Task<string> CreateHoldTypeWithRecord([FromBody] UpdateHoldDto dto)
        {
            if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
            {
                var cannotHold = ExplorerService.IsFolderHasParentHold(dto.ReletedIds, out List<string> holdingBoxes);
                if (cannotHold)
                {
                    return I18N.Core.I18NEntity.GetString("RM_JS_RDM_Hold_PreventHoldByBox", string.Join(",", holdingBoxes));
                }
            }
            if (dto.HoldCategory != RecordsConstants.RecordHold_Default)
            {
                Logger.Warn("Hold category is not default, set to default");
                dto.HoldCategory = RecordsConstants.RecordHold_Default;
            }
            if (dto.HoldSetting != null && dto.HoldSetting.ProfileType != HoldProfileType.All)
            {
                Logger.Warn("Hold ProfileType is not all, set to all");
                dto.HoldSetting.ProfileType = HoldProfileType.All;
            }
            if (dto.HoldSetting.Id == null)
            {
                dto.HoldSetting.Id = Guid.NewGuid().ToString();
            }
            RAReturnMessage returnMessage = await ExplorerService.CreateHoldTypeWithRecordAsync(dto, dto.AllFolder);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while create hold with record,name:{0},record id :{1},ERROR:{2}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            await ExplorerService.BuildHoldNotificationScheduleJob(dto);
            Logger.Info("create hold success,name:{0},record id :{1}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds));
            return string.Empty;
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [ValidateHoldActionFilter]
        public async Task<string> ReuseHoldTypeWithRecord([FromBody] UpdateHoldDto dto)
        {
            if (ExplorerService.IsPhysicalRecord(dto.ReletedIds.FirstOrDefault()))
            {
                var cannotHold = ExplorerService.IsFolderHasParentHold(dto.ReletedIds, out List<string> holdingBoxes);
                if (cannotHold)
                {
                    return I18N.Core.I18NEntity.GetString("RM_JS_RDM_Hold_PreventHoldByBox", string.Join(",", holdingBoxes));
                }
            }
            //if (dto.HoldCategory == RecordsConstants.RecordHold_Default)
            //{
            //    dto.HoldCategory = RecordsConstants.RecordHold_Electronic;
            //}
            RAReturnMessage returnMessage = await ExplorerService.ReuseHoldTypeWithRecord(dto, dto.AllFolder);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while reuse hold with record,name:{0},record id :{1},ERROR:{2}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("reuse hold success,name:{0},record id :{1}", dto.HoldSetting.Name, string.Join(",", dto.ReletedIds));
            return string.Empty;
        }

        [HttpPost]
        public  RAReturnMessage CheckItemOnLoaned([FromBody] List<Guid> ids)
        {
            RAReturnMessage returnMessage =  ExplorerService.CheckItemOnLoaned(ids);
            if (returnMessage.MessageType == RAMessageType.Confirmation)
            {
                Logger.Info("confirmation before placing on hold for item on loaned already, record id: {0}", string.Join(",", ids), returnMessage.ErrorMessage);
                return returnMessage;
            }
            Logger.Info("checking item on loaned success, record id :{0}", string.Join(",", ids));
            return returnMessage;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin , RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public async Task<List<HoldSetting>> GetAllHolds()
        {
            List<HoldSetting> settings = await ExplorerService.GetHoldAsync((int)HoldProfileType.All);
            settings.Reverse();  //RECO-4994
            return settings;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ManageHold)]
        public async Task<List<HoldSetting>> GetAssignedHolds()
        {
            List<HoldSetting> settings = await ExplorerService.GetAssignedHoldsAsync();
            settings.Reverse();  
            return settings;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<List<HoldSetting>> GetSampleAllHolds()
        {
            List<HoldSetting> settings = await ExplorerService.GetSampleHoldAsync((int)HoldProfileType.All);
            settings.Reverse();
            return settings;
        }

        /// <summary>
        /// 不可用于Physical Hold
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.ManageHold | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<HoldSetting> LoadHoldSetting(Guid recordId)
        {
            HoldSetting holdsetting = await ExplorerService.GetHoldByRecoedIdAsync(recordId);
            return holdsetting;
        }
        /// <summary>
        /// 用于Physical Hold
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public List<string> LoadPhyHoldSetting(Guid recordId)
        {
            return ExplorerService.GetHoldsByRecoedId(recordId);
        }

        public Task<List<RemoveHoldSetting>> LoadHoldSettings(Guid recordId)
        {
            return ExplorerService.GetHoldListByRecoedIdAsync(recordId);
        }

        /// <summary>
        /// 用于Elec Hold
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public List<string> LoadElecHoldSetting(Guid recordId)
        {
            return ExplorerService.GetHoldsByRecoedId(recordId);
        }

        /// <summary>
        /// 以Hold为条件 cancel hold
        /// </summary>
        /// <param name="holdIds"></param>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public string CancelHolds([FromBody] List<string> holdIds)
        {
            RAReturnMessage returnMessage = ExplorerService.CancelHoldSetting(holdIds);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while cancel hold,hold id:{0},ERROR:{1}", string.Join(",", holdIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("Cancel hold success,hold id:{0}", string.Join(",", holdIds));
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<string> DeleteHoldAndSetting([FromBody] List<string> holdIds)
        {
            RAReturnMessage returnMessage = await ExplorerService.DeleteHoldAndSettingAsync(holdIds);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while delete hold,hold id:{0},ERROR:{1}", string.Join(",", holdIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("Cancel hold success,hold id:{0}", string.Join(",", holdIds));
            return string.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public async Task<string> GetRecordbyHoldId([FromBody] ExplorerSetHoldDto dto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.GetRecordbyHoldIdAsync(dto));
        }

        [HttpGet]
        [ValidateExplorerActionFilter("RelatedRecords")]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> GetRelatedRecords(Guid id)
        {
            return JsonConvert.SerializeObject(await ExplorerService.GetRelatedRecoredsInfoAsync(id));
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> SearchRecords([FromBody] AddPageSearchRecordsDto dto)
        {

            //System.Threading.Thread.Sleep(10000); //just for big data test
            return JsonConvert.SerializeObject(await ExplorerService.SearchRecordsAsync(dto.PageIndex, dto.PageSize, dto.Value, dto.CurrentId, dto.RelatedsCache));
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public string UpdateRelatedRecords([FromBody] UpdateRecordsDto dto)
        {
            List<Guid> addrelatedIdsForHistory = null;
            var idNameDict = new Dictionary<Guid, string>();
            var requestResult = ExplorerService.UpdateRelatedRecords(dto.Id, dto.ReletedIds, dto.DeleteReletedIds, idNameDict, out addrelatedIdsForHistory);
            return JsonConvert.SerializeObject(requestResult);
        }

        [ValidateExplorerActionFilter]
        [ValidTermTreeParameterFilter("ChangeTerm")]
        [ValidateHoldActionFilter]
        [ValidReclassifyParameterFilter]
        public async Task<string> ChangeTermAsync([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeTermAsync(termDto));
        }

        [ValidateExplorerActionFilter]
        [ValidTermTreeParameterFilter("ChangeLabel")]
        [ValidateHoldActionFilter]
        [ValidReclassifyParameterFilter]
        public async Task<string> ChangeLabelAsync([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeGoogleTermAsync(termDto));
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManualReviewEnduser, PermissionJoinType.Any)]
        public bool CheckItemsInTheSameSecurityGroup([FromBody] List<Guid> recordIds)
        {
            return ExplorerService.CheckItemsInTheSameSecurityGroup(recordIds);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManualReviewEnduser, PermissionJoinType.Any)]
        public string GetRealTimeJobStatusInfo(string jobId)
        {
            return JsonConvert.SerializeObject(ExplorerService.GetRealTimeJobStatusInfo(jobId));
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> DeclareRecords([FromBody] List<Guid> ids)
		{
            var result = await ExplorerService.DeclareAsRecordAsync(ids);
			return JsonConvert.SerializeObject(result);
		}
		[HttpPost]
		[ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> UndeclareRecords([FromBody] List<Guid> ids)
        {
			var result = await ExplorerService.UndeclareAsRecordAsync(ids);
			return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidTermTreeParameterFilter("PhysicalMove")]
        public string PhysicalMove([FromBody] PhysicalMoveDto moveDto)
        {
            return JsonConvert.SerializeObject(ExplorerService.PhysicalMove(moveDto));
        }
        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> LoadDetails([FromBody] DetailQueryDto dto)
        {
            RecordDetailDto result = await ExplorerService.LoadDetailByKeyAsync(dto.status, dto.Id, dto.tab);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public string StartRestoreArchivedContent([FromBody] List<Guid> ids)
        {
            return JsonConvert.SerializeObject(ExplorerService.StartRestoreArchivedContent(ids));
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public async Task<IActionResult> DownloadArchivedContent()
        {
            try
            {
                Logger.Debug("DownloadArchivedContent controller");
                string recordIdsString = HttpUtility.UrlDecode(Request.Form["fileIdString"]);
                List<Guid> ids = recordIdsString.Split(',').Select(r => new Guid(r)).ToList();
                string fileName = string.Empty;
                if (ids.Count == 1)
                {
                    string userId = TenantLocalValue.LogonUserId;
                    Guid recordId = ids[0];
                    var contentInfo = DownloadDataInfoDao.GetDownloadDataInfos(ids, new List<int>() { (int)DB.Model.DownloadContentJobStatus.Finished }).FirstOrDefault();
                    if (contentInfo == null || string.IsNullOrWhiteSpace(contentInfo.JobId))
                    {
                        Logger.Error($"Cannot find download info with id:{ids[0]}");
                        return new StatusCodeResult((int)HttpStatusCode.NoContent);
                    }
                    fileName = HttpUtility.UrlDecode(contentInfo.Name);
                }
                else
                {
                    string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(DateTime.UtcNow.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                    fileName = I18NEntity.GetString("RM_DC_DownloadMultipleArchivedContent") + "_" + nowTimeStr + ".zip";
                }
                FileTransferStream stream = ArchivedContentDownloadService.DownloadArchivedContent(ids);
                if (stream == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                return GetValidatedFile(stream, GetContentType(fileName), Path.GetFileName(fileName));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        [ValidateExplorerActionFilter]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public string GetDownloadSasById([FromBody] Guid id)
        {
            var uri = DownloadDataInfoDao.GetBlobSasUriByRecordId(id);
            return uri;
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, PermissionJoinType.Any)]
        public async Task<string> LoadArchivedContent([FromBody] ArchivedContentSearchInfo searchInfo)
        {
            return JsonConvert.SerializeObject(await ExplorerService.LoadDownloadArchivedContentAsync(searchInfo));
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, PermissionJoinType.Any)]
        public string DeleteArchivedContent([FromBody] List<Guid> jobIds) 
        {
            return JsonConvert.SerializeObject(ExplorerService.DeleteArchivedContent(jobIds));
        }

        #region Move

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any)]
        public async Task<string> CheckSPLocation([FromBody] MoveToDto dto)
        {
            var rst = await ExplorerService.CheckSPUrlAsync(dto.LocationPath, dto.SPAccount);
            if (rst == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(rst);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public async Task<string> CheckSPLocation4Rule([FromBody] MoveToDto dto)
        {
            var rst = await ExplorerService.CheckSPUrl4RuleAsync(dto.LocationPath, dto.SPAccount);
            if (rst == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(rst);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public string CheckSPLocation4Job([FromBody] MoveToDto dto)
        {
            var rst = ExplorerService.CheckSPUrl4Job(dto.LocationPath, dto.SPAccount, true);
            if (rst == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(rst);
        }

        //[HttpPost]
        //[ValidateExplorerActionFilter]
        //public string MoveTo(MoveToDto dto)
        //{
        //    return ExplorerService.AddMoveJobTODBJobQueue(dto);
        //}
        #endregion

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public async Task<bool> RunMHoldNotificationSchedule()
        {
            await new ProcessHoldNotificationExecutor().ExecutorAsync();
            return true;
        }
        [HttpPost]
        public IActionResult DownloadTemplate()
        {
            try
            {
                string filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Hold import template.csv");
                var name = System.IO.Path.GetFileName(filepath);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                var ContentType = GetContentType(filepath);
                return File(memoryStream, ContentType, name);
            }
            catch (Exception e)
            {
                Logger.Error($"Fail download import hold records template,ex:{e}");
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<string> ImportData()
        {
            try
            {
                var file = Request.Form.Files["holdImportFile"];
                if (file == null || file.Length == 0)
                {
                    Logger.Warn("ImportHolds: No file uploaded.");
                    return "No file uploaded.";
                }
                Logger.Info("Import hold records file,file name :{0}", file.FileName);
                CheckFile(file);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "HoldRecords_" + dt.Ticks.ToString() + "." + "csv";
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                var result = await ExplorerService.RunImportHoldRecordsJobAsync(JobRunBy.Control, blobName);
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                Logger.Error($"ImportHolds: Unexpected error, ex: {ex}");
                return ex.Message;
            }
        }

        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV, FileExtension.XLSX };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            //WebUtil.CheckFileHeadCode(file.InputStream, allowFileExts);
        }

        [HttpPost]
        public async Task<List<RecordPermissionDto>> GetRecordsPermission([FromBody] List<ExplorerRecordPermission> records)
        {
           return await ExplorerService.GetRecordsPermission(records);
        }

        [HttpPost]
        public List<WorkplaceDto> GetWorkspadeByNodeLevel([FromBody] GetWorkspaceRequestDto dto)
        {
            return WorkspaceHoldService.GetWorkspadeByNodeLevel(dto);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<RAReturnMessage> CreateWorkspaceHold([FromBody] WorkspaceRequestDto dto)
        {
            var result = await WorkspaceHoldService.CreateWorkspaceHold(dto);
            return result;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<RAReturnMessage> UpdateWorkspaceHold([FromBody] WorkspaceHoldUpdateDto dto)
        {
            var result = await WorkspaceHoldService.UpdateWorkspaceHoldAsync(dto);
            return result;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        [ValidateHoldActionFilter]
        public async Task<string> DeleteWorkspaceHolds([FromBody] List<string> workspaceHoldIds)
        {
            var returnMessage = await WorkspaceHoldService.DeleteWorkspaceHoldsAsync(workspaceHoldIds);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while delete workspace hold, id:{0},ERROR:{1}", string.Join(",", workspaceHoldIds), returnMessage.ErrorMessage);
                return returnMessage.ErrorMessage;
            }
            Logger.Info("Cancel workspace hold success, id:{0}", string.Join(",", workspaceHoldIds));
            return string.Empty;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public async Task<List<WorkspaceHoldItemDto>> GetWorkspaceHoldsByPageSize()
        {
            return await WorkspaceHoldService.GetWorkspaceHoldsByPageSizeAsync();
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public IActionResult DownloadWorkspaceHoldTemplate()
        {
            try
            {
                string filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Workplace Hold import template.csv");
                var name = System.IO.Path.GetFileName(filepath);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                var ContentType = GetContentType(filepath);
                return File(memoryStream, ContentType, name);
            }
            catch (Exception e)
            {
                Logger.Error($"Fail download import workplace hold template,ex:{e}");
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public async Task<string> ImportWorkspaceHoldData()
        {
            try
            {
                var file = Request.Form.Files["workspaceHoldImportFile"];
                if (file == null || file.Length == 0)
                {
                    Logger.Warn("ImportWorkspaceHoldData: No file uploaded.");
                    return "No file uploaded.";
                }
                Logger.Info("Import workspace hold file,file name :{0}", file.FileName);
                CheckFile(file);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "WorkspaceHold_" + dt.Ticks.ToString() + "." + "csv";
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                var result = await WorkspaceHoldService.RunImportWorkspaceHoldJobAsync(JobRunBy.Control, blobName);
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                Logger.Error($"ImportWorkspaceHoldData: Unexpected error, ex: {ex}");
                return ex.Message;
            }
        }

    }


}
