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
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.PermissionManagement;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Performance;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Util;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Exchange.WebServices.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser, preferred: false)]
    public class ManualApprovalController : BaseApiController
    {
        private IRMManualApprovalService _ManualApprovalService;
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);

        private IRMTenantUpgradeInfoDao _TenantUpgradeInfoDao;
        private IRMTenantUpgradeInfoDao TenantUpgradeInfoDao => PlatformWindsorManager.GetService(ref _TenantUpgradeInfoDao);

        private IRMSharePointSettingsService _RMSPSettingsService;
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);

        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "UnderReviewQuery")]
        public Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody]ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.UnderReviewQueryAsync(queryDefinition);
        }

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "RelatedRecordsQuery")]
        public Task<ManualApprovalPaginateResult> RelatedRecordsQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.RelatedRecordsQueryAsync(queryDefinition);
        }

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "ExtendQuery")]
        public Task<ManualApprovalPaginateResult> ExtendQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.ExtendQueryAsync(queryDefinition);
        }

        [ValidManualApprovalParameterFilter(ManualApprovalActionType.ChangeTerm)]
        public async Task<string> ChangeTerm([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeTermAsync(termDto));
        }

        [HttpPost]
        public Task<string> GetChildrenTreeNodes([FromBody] TreePage tree)
        {
            int pIndex = tree.PageIndex ?? 0;
            int pSize = tree.PageSize ?? 0;

            if (pIndex > 0)
            {
                pIndex -= 1;
            }

            string nodeId = tree.NodeId ?? string.Empty;
            string nodeType = tree.NodeType ?? string.Empty;
            int SettingType = tree.SettingType != null ? Convert.ToInt32(tree.SettingType) : 0;
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = false,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = tree.ForPhysicalView
            };

            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, SettingType, filterOption);
        }

        [HttpGet]
        public Task<string> GetChildrenByDBForView([FromQuery] TreePage tree)
        {
            int pIndex = 0;
            if (tree.PageIndex != null)
            {
                int.TryParse(tree.PageIndex.ToString(), out pIndex);
            }
            int pSize = 0;
            if (tree.PageSize != null)
            {
                int.TryParse(tree.PageSize.ToString(), out pSize);
            }
            pIndex = pIndex == 0 ? pIndex : pIndex - 1;

            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = !tree.ShowAllTerms,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = true
            };

            if (tree.SourceFlag == SourceFlag.Google)
            {
                return TaxonomyService.GetTaxonomyAllGoogleTermTreeDataAsync(filterOption, pIndex, pSize);
            }
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, 0, filterOption);
        }

        [HttpPost]
        public async Task<RAReturnMessage> DoAction([FromBody] GlobalSearchActionDto actionDto)
        {
            RAReturnMessage message = await ExplorerService.ValidateParameterAsync(actionDto, ChangeTermPage.MyHub);
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
            return message;
        }

        [ValidManualApprovalParameterFilter(ManualApprovalActionType.ChangeTerm)]
        public async Task<string> ChangeLabel([FromBody] ChangeTermDto termDto)
        {
            return JsonConvert.SerializeObject(await ExplorerService.ChangeGoogleTermAsync(termDto));
        }

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "WaitDisposalQuery")]
        public Task<ManualApprovalPaginateResult> WaitDisposalQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.WaitDiposalQueryAsync(queryDefinition);
        }

        [HttpPost]
        [PerformanceTestMonitor(FunctionName = "HistoryAzureTableQuery")]
        public Task<List<ManualApprovalItem>> HistoryAzureTableQuery()
        {
            return ManualApprovalService.HistoryAzureTableQueryAsync();
        }

        [HttpPost]
        public Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptions()
        {
            return ManualApprovalService.GetFilterDefaultOptionsAsync();
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
        [PerformanceTestMonitor(FunctionName = "Approve")]
        public Task<ManualApprovalActionResult> Approve([FromBody]ManualApprovalActionParams approveParameters)
        {
            return ManualApprovalService.ApproveAsync(approveParameters);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
        [PerformanceTestMonitor(FunctionName = "Reject")]
        public Task<ManualApprovalActionResult> Reject([FromBody]ManualApprovalActionParams rejectParameters)
        {
            return ManualApprovalService.RejectAsync(rejectParameters);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Escalate)]
        public Task<ManualApprovalActionResult> Escalate([FromBody] ManualAprovalEscalateDefinition definition)
        {
            return ManualApprovalService.EscalateAsync(definition);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reassign)]
        public Task<ManualApprovalActionResult> Reassign([FromBody] ManualAprovalEscalateDefinition definition)
        {
            return ManualApprovalService.ReassignAsync(definition);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Extend)]
        public Task<ManualApprovalActionResult> Extend([FromBody] ManualApprovalExtendDefinition definition)
        {
            return ManualApprovalService.Extend(definition);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.RestoreExtend)]
        public Task<ManualApprovalActionResult> RestoreExtended([FromBody]List<Guid> itemIds)
        {
            return ManualApprovalService.RestoreExtended(itemIds);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.ChangeDisposalAction)]
        public Task<ManualApprovalActionResult> ChangeDiposalAction([FromBody] ManualApprovalRelatedRecordsDisposalDefinition definition)
        {
            return ManualApprovalService.ChangeDiposalAction(definition);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.ResetManualWorkflow)]
        public Task<ManualApprovalActionResult> ResetManualReviewForWorkflow([FromBody]List<Guid> itemIds)
        {
            return ManualApprovalService.ResetManualReviewForWorkflow(itemIds);
        }

        [HttpPost]
        public Task<ManualApprovalSettings> GetSettingInfo()
        {
            return ManualApprovalService.GetManualApprovalSettingsAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.UpdateSetting)]
        public Task<bool> UpdateSettingInfo([FromBody] ManualApprovalSettings setting)
        {
            return ManualApprovalService.UpdateManualApprovalSetting(setting);
        }

        [HttpPost]
        public Task<bool> DisabledEscalate()
        {
            return ManualApprovalService.DisabledEscalateAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public bool RunManualApprovalSettingSchedule()
        {
            return ManualApprovalService.SchduleRunEmailScheduleJob(Contract.RMWeb.JobRunBy.Control);
        }

        [HttpPost]
        public MAReturnMessage RunBulkActionJob([FromBody] ManualApprovalJobParam param)
        {
            return ManualApprovalService.RunBulkActionJob(param);
        }

        [HttpPost]
        public RMTenantUpgradeInfo GetTenantUpgradeInfo()
        {
            return TenantUpgradeInfoDao.Get(TenantLocalValue.LogonGroupId);
        }

        [HttpPost]
        public Task<ManualApprovalWorkspacePaginateResult> QueryWorkspaces([FromBody] ManualApprovalWorkspaceQueryDefinition queryDefinition)
        {
            return ManualApprovalService.QueryWorkspacesAsync(queryDefinition);
        }


        [HttpPost]
        public Task<ManualApprovalFilterFolderPathResult> QueryFolderPath([FromBody] ManualApprovalFolderPathQueryDefinition queryDefinition)
        {
            return ManualApprovalService.QueryFolderPathAsync(queryDefinition);
        }

        [HttpPost]
        public Task<bool> EnableFolderPath()
        {
            return ManualApprovalService.EnableFolderPathForDeloitte();
        }

        [HttpPost]
        public async Task<bool> EnableFolderPathOnlyOneLocation()
        {
            var result = await ManualApprovalService.EnableFolderPathForDeloitteOnlyOneLocation();
            return result.isOnlyOneLocation;
        }


        [HttpPost]
        public RAReturnMessage RunExportHistoryDataJob([FromBody] ManualApprovalHistoryOption historyOption)
        {
            var originalHost = Request.Headers.Host;
            if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
            {
                string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                originalHost = Request.Headers.GetHeaderValue(originalHostKey);
            }
            var serverUrl = !string.IsNullOrEmpty(originalHost) ? $"https://{originalHost}" : "";
            historyOption.ServiceUrl = serverUrl;
            return ManualApprovalService.RunExportHistoryDatasJob(serverUrl, historyOption);
        }

        [HttpPost]
        public Task<RAReturnMessage> RunExportRecordsForReviewDataJob([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return ManualApprovalService.RunExportRecordsForReviewDatasJobAsync(queryDefinition);
        }

        [HttpPost]
        public RAReturnMessage ImportManualUnderReviewDatas()
        {
            var file = Request.Form.Files["fileUp"];
            Logger.Info("Manual under review import file,file name :{0}", file.FileName);
            var binaryReader = new System.IO.BinaryReader(file.OpenReadStream());
            var fileLength = binaryReader.BaseStream.Length;
            var fileSize = fileLength / (1024 * 1024);
            if(fileSize > 80)
            {
                return new RAReturnMessage() { ErrorMessage = "The size of the uploaded file cannot exceed 25MB", MessageType = RAMessageType.Failed };
            }
            return ManualApprovalService.RunImportUnderReviewDatasJob(file.FileName, file.OpenReadStream());
        }

        [HttpPost]
        public ManualApprovalCountResult GetImportFileInfo()
        {
            var file = Request.Form.Files["fileUp"];
            Logger.Info("Manual under review import file,file name :{0}", file.FileName);
            var binaryReader = new System.IO.BinaryReader(file.OpenReadStream());
            var fileLength = binaryReader.BaseStream.Length;
            var fileSize = fileLength / (1024 * 1024);
            if (fileSize > 20)
            {
                return new ManualApprovalCountResult();
            }
            return ManualApprovalService.ReadUploadFile(file.FileName, file.OpenReadStream());
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.SaveConfigOption)]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewAdmin, RMPermissionExtensionMasks.ManualApprovalSettingEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public Task<bool> SaveApprovalCommentOption([FromBody]ManualApprovalCommentInfos option)
        {
            return ManualApprovalService.SaveApprovalCommentOptionAsync(option);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.SaveApprovalSetting)]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewAdmin, RMPermissionExtensionMasks.ManualApprovalSettingEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public async Task<bool> SaveApprovalSettingInfo([FromBody] ManualApprovalSettingInfo manualApprovalSetting)
        {
            return await RouteMultiGeoApiActionAsync(manualApprovalSetting, MultiGeoOperationType.SaveApprovalSettingInfo,
                async (request) =>
                {
                    return await ManualApprovalService.SaveApprovalSettingAsync(request);
                },
                req => false);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser)]
        public Task<ManualApprovalCommentInfos> GetApprovalCommentOption()
        {
            return ManualApprovalService.GetApprovalCommentOptionAsync();
        }


        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser)]
        public Task<List<CustomMetadataColumnInfo>> GetInUsedCustomMetadataColumns()
        {
            return RMSPSettingsService.GetInUsedCustomMetadataColumnInfoAsync();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser)]
        public Task<bool> IsHideReclassifyBtnInManualApproval()
        {
            return ManualApprovalService.IsHideReclassifyBtnInManualApproval();
        }

    }
}