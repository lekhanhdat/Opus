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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.PhysicalBrowserService;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Controllers.API;
using AvePoint.RA.Web.Models.Common;
using AvePoint.RA.Web.Models.PRM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, preferred: false)]
    public class PhysicalRecordApiController : BaseApiController
    {
        private ITemplateManagementService _TemplateManagementService;
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService(ref _TemplateManagementService);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IExplorerQueryService _ExplorerQueryService;
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService(ref _ExplorerQueryService);
        private IPhysicalBrowserService _PhysicalBrowserService;
        private IPhysicalBrowserService PhysicalBrowserService => PlatformWindsorManager.GetService(ref _PhysicalBrowserService);
        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);
        private IRMPhysicalRecordSettingsService _PhysicalRecordSettingService;
        private IRMPhysicalRecordSettingsService PhysicalRecordSettingService => PlatformWindsorManager.GetService(ref _PhysicalRecordSettingService);
        private IPermissionManagementService _PermissionManagementService;
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService(ref _PermissionManagementService);
        private IUserService _UserService;
        private IUserService UserService => PlatformWindsorManager.GetService(ref _UserService);
        private IReportCollectionService _ReportCollectionService;
        private IReportCollectionService ReportCollectionService => PlatformWindsorManager.GetService(ref _ReportCollectionService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private ISecurityGroupManagementService _SecurityGroupManagementService;
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService(ref _SecurityGroupManagementService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<string> LoadTemplateDatas(int id)
        {
            return TemplateManagementService.LoadTemplateDatasAsync(id);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<string> LoadTemplateDatasForBulkUpdate(int id)
        {
            return TemplateManagementService.LoadTemplateDatasAsync(id, true);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public int ValidHasUniqueIdSettings(Guid templateId)
        {
            return TemplateManagementService.ValidHasUniqueIdSettings(templateId);
        }

        [HttpPost]
        public async Task<Dictionary<string, object>> GetTemplateDataById([FromBody] QueryTemplateDto dto)
        {
            var templateData = new Dictionary<string, object>()
            {
                { "Template", await TemplateManagementService.LoadTemplateDtoAsync(dto.TemplateId, dto.PhyNodeInfo) },
                { "Settings", await PhysicalRecordSettingService.LoadPhysicalRecordSettingAsync(dto.LocationUid) }
            };
            return templateData;
        }

        [HttpGet]
        [ValidPhysicalExplorerActionFilter("ValidateLocationByUniqueId")]
        public Task<RMPRTreeNode> GetTermSettingForLocation(Guid locationId)
        {
            return PhysicalRecordSettingService.LoadPhysicalRecordSettingAsync(locationId);
        }

        [HttpGet]
        [ValidPhysicalExplorerActionFilter("ValidateLocationByUniqueId")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<bool> HasTermSettingsForLocation(Guid locationId)
        {
            var settings = await PhysicalRecordSettingService.LoadPhysicalRecordSettingAsync(locationId);
            return settings != null && settings.TermSetId != Guid.Empty;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidTermTreeParameterFilter("AddOrUpdatePhysicalObject")]
        public async Task<ResponseDto> AddOrUpdatePhysicalObject([FromBody] PhysicalObjectDto obj)
        {
            ResponseDto result = new ResponseDto
            {
                success = true
            };

            if (obj.Template == null && obj.TemplateId != 0)
            {
                obj.Template = await TemplateManagementService.LoadTemplateDtoAsync(obj.TemplateId, obj);
            }
            obj.ParentId = obj.Ancestors.Last();
            try
            {
                if (obj.ScopePerDto != null)
                {
                    if (obj.ScopePerDto.IsInheritSave)
                    {
                        try
                        {
                            var scopeFullPath = PermissionManagementService.GetScopeIdFullPath(obj);
                            var permissionId = PermissionManagementService.GetScopePermissionId(scopeFullPath, false);
                            obj.ScopePermissionId = permissionId;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn($"An error when get inher permission for record, id:[{obj.Id}],name:[{obj.Name}], message:{ex.ToString()}");
                        }
                    }
                    else
                    {
                        //同步没有注册的user
                        var syncUserResult = await PermissionManagementService.SyncADUsersAsync(obj.ScopePerDto.Accounts);
                        if (syncUserResult.MessageType != RAMessageType.Successful)
                        {
                            result.success = false;
                            result.message = syncUserResult.ErrorMessage;
                            return result;
                        }

                        //添加权限记录并给PhysicalObjectDto赋值ScopePermissionId
                        var permissionDto = PermissionManagementService.ConvertToScopePermissionDto(obj);
                        var addPermissionResult = await PermissionManagementService.SavePermissionForNewPhysicalAsync(permissionDto, obj);
                        if (addPermissionResult.MessageType != RAMessageType.Successful)
                        {
                            result.success = false;
                            result.message = addPermissionResult.ErrorMessage;
                            return result;
                        }
                    }
                }

                //添加physical数据
                var addPhyObjectResult = await ExplorerService.AddOrUpdatePhysicalObjectAsync(obj);
                if (addPhyObjectResult.MessageType != RAMessageType.Successful)
                {
                    result.success = false;
                    result.message = addPhyObjectResult.ErrorMessage;
                    PermissionManagementService.DeletePermissionInfo(obj.Id.ToString());
                }
                else
                {
                    result.data = addPhyObjectResult.Extsion1;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in AddOrUpdatePhysicalObject, [{ex.ToString()}]");
                result.success = false;
                result.message = ex.Message;
            }
            return result;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidTermTreeParameterFilter("AddOrUpdatePhysicalObject")]
        public async Task<ResponseDto> EditPhysicalObject([FromBody] PhysicalObjectDto obj)
        {
            ResponseDto result = new ResponseDto();
            result.success = true;

            if (obj.Template == null && obj.TemplateId != 0)
            {
                obj.Template = await TemplateManagementService.LoadTemplateDtoAsync(obj.TemplateId, obj);
            }

            try
            {
                var tempResult = await ExplorerService.EditPhysicalObjectAsync(obj);
                if (tempResult.MessageType != RAMessageType.Successful)
                {
                    result.success = false;
                    result.message = tempResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in EditPhysicalObject, [{ex}]");
                result.success = false;
                result.message = ex.Message;
            }
            return result;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidPhysicalBulkUpdateParameterFilter]
        public async Task<ResponseDto> BulkEditPhysicalObject([FromBody] BuldUpdatePhysicalDto dto)
        {
            ResponseDto result = new ResponseDto
            {
                success = true
            };
            try
            {
                var tempResult = await ExplorerService.BulkEditPhysicalObjectAsync(dto.RecordIds, dto.MetaInfo, dto.TemplateId);
                if (tempResult.MessageType != RAMessageType.Successful)
                {
                    result.success = false;
                    result.message = tempResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in BulkEditPhysicalObject, [{ex}]");
                result.success = false;
                result.message = ex.Message;
            }
            return result;
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("GetPhysicalObjectById")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<string> GetPhysicalObjectById([FromBody] QueryPhyObjectDto dto)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var nodeInfo = dto.PhyNodeInfo;
                var templateIdPath = dto.TemplateIdPath;
                var nodeType = dto.NodeType;
                if (nodeType <= (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    var nodeId = 0;
                    if (int.TryParse(dto.Id, out nodeId))
                    {
                        if (nodeId != 0)
                        {
                            result = await LocationManagementService.GetPhysicalObjectByIdAsync(nodeId);
                            result.ChildTemplates = await TemplateManagementService.GetAllTemplatesByLocationId4ExplorerAsync(result.Id);
                        }
                    }
                    else
                    {
                        Logger.Error($"Load location info, current id seems is not in correct format, id value: [{dto.Id}].");
                    }
                }
                else
                {
                    var nodeId = Guid.Empty;
                    if (Guid.TryParse(dto.Id, out nodeId))
                    {
                        if (nodeId != Guid.Empty)
                        {
                            result = await ExplorerService.GetPhysicalObjectByIdAsync(nodeId, true);
                            result.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(nodeId);
                            result.TermFullPath = TaxonomyService.GetTermPathByTermId(result.TermId);
                            if (result.MetaInfo == null)
                            {
                                result.MetaInfo = new Dictionary<string, string>();
                            }
                            if (result.Id != Guid.Empty)
                            {
                                if (nodeInfo != null)
                                {
                                    if (result.BoxId == Guid.Empty)
                                    {
                                        nodeInfo.BoxId = Guid.Empty;
                                    }
                                    result.Template = await TemplateManagementService.LoadTemplateDtoAsync(result.TemplateId, nodeInfo);
                                }
                                else
                                {
                                    result.Template = await TemplateManagementService.LoadTemplateDtoAsync(result.TemplateId, result);
                                }
                                await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(result);

                                if (result.NodeType == RMNodeType.PhyBox || result.NodeType == RMNodeType.PhyFile)
                                {
                                    await ExplorerService.GetPhysicalBarcodeInfoAsync(result);
                                }
                                //result.ChildTemplates = TemplateManagementService.GetTemplatesByPhysicalObject4Explorer(result);
                                result.ChildTemplates = await TemplateManagementService.GetTemplatesByIdPathAsync(result.Template.uniqueId, templateIdPath, Convert2TemplateType(nodeType));
                            }
                        }
                    }
                    else
                    {
                        Logger.Error($"Load physical object info, current id seems is not in correct format, id value: [{dto.Id}].");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error, [{ex.ToString()}]");
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<List<PhysicalAudit>> GetPhysicalActionAuditsById(Guid id)
        {
            return await RecordsHistoryService.GetPhysicalRecordActionAuditsAsync(id);
        }

        private List<TemplateType> Convert2TemplateType(int nodeType)
        {
            switch(nodeType)
            {
                case (int)RMNodeLevel.PhysicalCustom:
                    return new List<TemplateType> { TemplateType.Box, TemplateType.Folder, TemplateType.Custom};
                case (int)RMNodeLevel.PhysicalBox:
                    return new List<TemplateType> { TemplateType.Folder };
                case (int)RMNodeLevel.PhysicalFile:
                    return new List<TemplateType> { TemplateType.Records};
                default:
                    throw new ArgumentException("nodeType is invalid");
            }
        }

        [HttpGet]
        [ValidPhysicalExplorerActionFilter("GetPhysicalObjectById")]
        public string GetPhysicalObjectFullPathById(string id)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var nodeId = Guid.Empty;
                if (Guid.TryParse(id, out nodeId))
                {
                    return ExplorerService.GetPhysicalObjectFullPath(nodeId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error, [{ex.ToString()}]");
            }
            return string.Empty;
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("GetPhysicalObjectList")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<string> GetPhysicalObjectList([FromBody] PhysicalExplorerQueryDto dto)
        {
            var result = new PhysicalResultInfo();
            try
            {
                if (dto.CurrentNodeType <RMNodeLevel.PhysicalBottomLocation)
                {
                    result = await LocationManagementService.QueryPhysicalNodesAsync(dto);
                }
                else
                {
                    result = await ExplorerService.QueryPhysicalNodesAsync(dto);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error, [{ex.ToString()}]");
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }

        //目前parentPhyBoxId，parentPhyFolderId 没有被使用到，可以去掉
        [HttpPost]
        public Dictionary<Guid, string> GetPhysicalPushedColumnValues([FromQuery]Guid parentPhyBoxId, [FromQuery] Guid parentPhyFolderId, [FromBody]List<PushColumnDto> pushedColumnIDs)
        {
            return ExplorerService.GetPushedColumnValues(parentPhyBoxId, pushedColumnIDs);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidPhysicalExplorerActionFilter("MultiplePhysicalObjects")]
        public DeleteResultInfo DeletePhysicalObject([FromBody] List<PhysicalObjectDto> physicalObjectDtos)
        {
            return ExplorerService.DeletePhysicalObject(physicalObjectDtos); ;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        [ValidPhysicalExplorerActionFilter("MultiplePhysicalObjects")]
        public string PreDeletePhysicalObject([FromBody] List<PhysicalObjectDto> physicalObjectDtos)
        {
            var result = ExplorerService.PreDeletePhysicalObjects(physicalObjectDtos);
            return SerializerHelper.SerializeByJsonSerializer(result);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalFolderLoanReturn, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("RemovePersonalHold")]
        public async Task<Dictionary<string, object>> RemovePersonalHold([FromBody]List<Guid> nodeIDs)
        {
            Dictionary<string, object> result = new Dictionary<string, object>()
            {
                { "success", true }
            };
            RAReturnMessage returnMessage = await ExplorerService.RemovePersonalHoldAsync(nodeIDs);
            if (returnMessage.MessageType == RAMessageType.Failed)
            {
                Logger.Error("an error occurred while records cancel person hold setting, record ids: {0}", string.Join(",", nodeIDs));
                result["success"] = false;
                result["message"] = returnMessage.ErrorMessage;
            }
            else
            {
                bool startJob = false;
                if (bool.TryParse(returnMessage.Extension, out startJob))
                {
                    result["isStartJob"] = true;
                }
                Logger.Info("Canceled person hold, record ids: {0}", string.Join(",", nodeIDs));
            }
            return result;
        }

        [HttpGet]
        public async Task<string> Test()
        {
            var result = string.Empty;
            try
            {
                result = await ExplorerService.GetPhysicalBoxPathByIdAsync(new Guid("736ce4b6-224e-45aa-8f4c-40a5d3e641b3"));
            }
            catch (Exception ex)
            {
                Logger.Error($"Error, [{ex.ToString()}]");
            }
            return result;
        }

        #region Explorer Tree



        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public Task<List<RMPhysicalExplorerNode>> InitTree([FromBody] string recordId = null)
        {
            //TemplateManagementService.InitDefaultData();
            return Guid.TryParse(recordId, out Guid recordIdValue)
                ? PhysicalBrowserService.InitTreeAsync(recordIdValue)
                : PhysicalBrowserService.InitTreeAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<Dictionary<string, object>> SearchTree(string uniqueId)
        {
            //TemplateManagementService.InitDefaultData();
            var result = await PhysicalBrowserService.SearchTreeAsync(uniqueId);
            return new Dictionary<string, object> {
                { "success", result.Item1 },
                { "tableData", result.Item2},
                { "treeData", result.Item3 },
                { "selectPhyObj", result.Item4 },
                { "showTableSearchKey", result.Item5 }
            };
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("BrowseTree")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public Task<RMPhysicalExplorerNode> BrowseTree([FromBody]RMPhysicalExplorerNode node)
        {
            if (node.PagerSize <= 0)
            {
                node.PagerSize = 10;
            }
            return PhysicalBrowserService.BrowserAsync(node);
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("BrowseTree")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<RMPhysicalExplorerNode> BrowseSearchTree([FromBody] RMPhysicalExplorerNode node)
        {
            if (node.PagerSize <= 0)
            {
                node.PagerSize = 10;
            }
            return await PhysicalBrowserService.BrowserSearchTreeAsync(node);
        }

        #endregion

        [HttpPost]
        public PhysicalHoldValidateBoxResult ValidateBoxHold([FromBody] PhysicalHoldValidateBoxParam param)
        {
            PhysicalHoldValidateBoxResult result = new PhysicalHoldValidateBoxResult();
            List<string> names = ExplorerService.GetHoldChildrenByBox(param.NodeId);
            if (names != null && names.Count > 0)
            {
                result.HasChildrenHold = true;
                result.FolderNames = names;
            }
            return result;
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public PhysicalHoldValidateBoxResult IsBoxHasHoldChildren([FromBody] PhysicalHoldValidateBoxParam param)
        {
            PhysicalHoldValidateBoxResult result = new PhysicalHoldValidateBoxResult();
            result.HasChildrenHold = ExplorerService.IsBoxHasHoldChildren(param.NodeId);
            return result;
        }
        [HttpGet]
        public AOSUserDto CurrentUser()
        {
            return new AOSUserDto()
            {
                UserId = TenantLocalValue.LogonUserId,
                DisplayName = TenantLocalValue.DisplayName,
                UserPrincipalName = TenantLocalValue.LogonUserEmail
            };
        }

        //[HttpPost]
        //[RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        //[ValidTermTreeParameterFilter("QueryDataList")]
        //public string QueryDataList(ExplorerQueryDto dto)
        //{
        //    return JsonConvert.SerializeObject(ExplorerService.QueryDataListWithoutTotal(dto, true));
        //}

        //[HttpPost]
        //[RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        //[ValidTermTreeParameterFilter("QueryDataListV2")]
        //public string QueryDataListV2(ExplorerQueryV2Dto dto)
        //{
        //    return JsonConvert.SerializeObject(ExplorerQueryService.QueryDataListWithTotal(dto));
        //}

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        //[ValidTermTreeParameterFilter("QueryDataListV2")]
        public async Task<string> QueryDataListV3([FromBody] ExplorerQueryV3Dto dto)
        {
            var canConvert2BasicSearch = dto.QueryOption.CanConvertBasicSearchCriteria();

            //var allAvaliableSourceFlags = SourceFlagHelper.GetAllSourceFlags();
            //var userPermission = SecurityTrimmingHelper.GetUserPermission<RMPermissionMasks>(false);
            //allAvaliableSourceFlags = userPermission.RemoveNoPermissionFourceFlags(allAvaliableSourceFlags);
            //var azureFile = SecurityTrimmingHelper.DoesUserHasThisPermission(RMPermissionExtensionMasks.AzureFSEndUser);
            //if (!azureFile)
            //{
            //    allAvaliableSourceFlags.RemoveAll(s => s == SourceFlag.AzureFileShare);
            //}
            var allAvaliableSourceFlags = await SecurityTrimmingHelper.GetAllAvailableSourceFlagsFromDbAsync();
            var canDoAction = allAvaliableSourceFlags.Count == 1 ? true : dto.QueryOption.CanDoGlobalAction();
            var canDoPhysicalBulkUpdate = dto.QueryOption.CanDoPhysicalBulkUpdate();

            var queryResut = await ExplorerQueryService.QueryDataListWithTotalAsync(dto);
            var result = new ExplorerResultInfoV3
            {
                CanConvert2BasicSearch = canConvert2BasicSearch,
                CanDoGlobalAction = canDoAction,
                CanDoPhysicalBulkUpdate = canDoPhysicalBulkUpdate,
                Datas = queryResut.Datas,
                PagingInfo = queryResut.PagingInfo
            };
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("ValidateExportBarcode")]
        public int GetSelectNodeAllChildCount([FromBody] ExportBarcodeDto exportBarcodeDto)
        {
            return ExplorerService.GetSelectNodeAllChildCount(exportBarcodeDto);
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("ValidateExportBarcode")]
        public async Task<ActionResult> ExportBarcode()
        {
            var exportTypeStr = Request.Form["ExportType"];
            var nodeIdStr = Request.Form["NodeId"];
            var nodeTypeStr = Request.Form["NodeType"];
            var fullPathStr = Request.Form["FullPath"];
            var suiteIdStr = Request.Form["SuiteId"];
            if (Enum.TryParse(exportTypeStr, out ExportType exportType) && Enum.TryParse(nodeTypeStr, out RMNodeType nodeType))
            {
                var exportBarcodeDto = new ExportBarcodeDto
                {
                    ExportType = exportType,
                    NodeId = new Guid(nodeIdStr),
                    NodeType = nodeType,
                    FullPath = fullPathStr,
                    SuiteId = new Guid(suiteIdStr),
                };
                if(await ValidPhyPermissionByNodeId(exportBarcodeDto) == false)
                {
                    return new StatusCodeResult((int)HttpStatusCode.Forbidden);
                }
                var result = await ExplorerService.ExportBarcodeAsync(exportBarcodeDto);
                if(result == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
                var fileName = result.FileName;
                if (GCommon.Utility.SecurityUtils.IsValidFileName(fileName))
                {
                    return File(result.FileContent, "application/octet-stream", fileName);
                }
            }
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }

        private async Task<bool> ValidPhyPermissionByNodeId(ExportBarcodeDto exportBarcodeDto)
        {
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            if (userPermission.IsAdmin) return true;
            var phyPermission = userPermission.ScopePermissionInfo.Where(s => s.DataSourceType == SourceFlag.Physical).FirstOrDefault();
            var topLocationPermission = phyPermission?.ScopeIds ?? new List<Guid>();
            Guid topLocationId = Guid.Empty;
            if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                var phyBox = ExplorerService.GetPhysicalObjectById(exportBarcodeDto.NodeId);
                topLocationId = RMLocationDao.LoadTopLocationIdBySubLocation(phyBox.LocationId);
            }
            else
            {
                topLocationId = RMLocationDao.LoadTopLocationIdBySubLocation(exportBarcodeDto.NodeId);
            }
            if (topLocationPermission.Contains(topLocationId))
            {
                return true;
            }
            return false;
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("ValidateExportBarcode")]
        public async Task<RAReturnMessage> ExportBarcodeToLD([FromBody] ExportBarcodeDto exportBarcodeDto)
        {
            if(!(await ValidPhyPermissionByNodeId(exportBarcodeDto)))
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Phy_Import_NoPermissionForLocation")
                };
            }
            return await ExplorerService.ExportBarcodeToLocationAsync(exportBarcodeDto);
        }

        #region Location Permission Management
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalAccessControl, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("SavePhysicalPermission")]
        public async Task<RAReturnMessage> SavePhysicalPermission([FromBody] ScopePermissionSimpleDto permissionDto)
        {
            var result = new RAReturnMessage();
            var syncUserResult = await PermissionManagementService.SyncADUsersAsync(permissionDto.Accounts);
            if (syncUserResult.MessageType != RAMessageType.Successful)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            else
            {
                var dto = PermissionManagementService.ConvertToScopePermissionDto(permissionDto);
                result = await PermissionManagementService.SaveLocationPermissionAsync(dto);
            }
            return result;
        }

        [HttpGet]
        [ValidPhysicalExplorerActionFilter("GetBreakOrInheritPermission")]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<string> GetBreakOrInheritPermission(string scopeId, bool includeSelf)
        {
            return JsonConvert.SerializeObject(await PermissionManagementService.GetBreakOrInheritPermissionAsync(scopeId, includeSelf));
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public async Task<bool> CheckPerForScope(string scopeId)
        {
            var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
            var idFullPath = PermissionManagementService.GetScopeIdFullPath(scopeId);
            return PermissionManagementService.HasCurrentScopePermission(idFullPath, userAndGroupIds);
        }


        #endregion

        #region global search set permission
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalAccessControl, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [HttpPost]
        public async Task<RAReturnMessage> RunJobForGlobalSearch([FromBody] GSPermissionSimpleDto dto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if ((dto.NodeIds == null || dto.NodeIds.Count == 0) && dto.QueryDto == null && dto.QueryV3Dto == null)
                {
                    ThrowUtil.ThrowIfNull(null, "NodeIds and QueryDto all null values");
                }
                else
                {
                    var syncUserResult = await PermissionManagementService.SyncADUsersAsync(dto.Accounts);
                    if (syncUserResult.MessageType != RAMessageType.Successful)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = syncUserResult.ErrorMessage;
                        return returnMessage;
                    }
                }

                var jd = await GetJobContextDtoAsync(dto);
                var runJobResult = PermissionManagementService.RunSetPermissionJob(jd);
                if (runJobResult.MessageType == RAMessageType.Successful)
                {
                    returnMessage.Extension = runJobResult.Extension;
                }
                else
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error when RunJobForGlobalSearch, message:{ex.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_SavePermission_ErrorMessage");
            }
            return returnMessage;
        }

        private async Task<ScopePermissionJobContextDto> GetJobContextDtoAsync(GSPermissionSimpleDto dto)
        {
            var jd = new ScopePermissionJobContextDto
            {
                GSJobContextDto = new GSPermissionJobContextDto
                {
                    //页面操作的User
                    UserId = TenantLocalValue.LogonUserId,
                    //目前只支持打破继承
                    IsInheritSave = false,
                    //权限类型暂时是All权限
                    PermissionType = RMScopePermissionEnum.All,
                    //Search Result方式设置权限，Query参数
                    QueryDto = dto.QueryDto,
                    //Search Result方式设置权限，Query参数 新Search
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

        #endregion

        #region Term Tree View
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        [ValidTermTreeParameterFilter("TreeViewChildrenNodes")]
        public Task<string> GetTermTreeViewChildrenTreeNodes([FromBody] TermTreeView tree)
        {
            int pIndex = tree.PageIndex?? 0;
            if (pIndex > 0)
            {
                tree.PageIndex = pIndex - 1;
            }
            tree.NodeType = tree.NodeType ?? string.Empty;
            return PhysicalBrowserService.GetTermTreeViewDataAsync(tree);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public int GetViewMode()
        {

            return PhysicalBrowserService.GetTreeViewMode();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]
        public Task SetViewMode([FromBody]int mode)
        {
            return PhysicalBrowserService.SetTreeViewModeAsync(mode);
        }

        [HttpPost]
        public string GetTermUsageInfo([FromBody] LineChartPageMode mode)
        {
            int sourceFlag = (int)mode.SourceFlag;
            List<BarChartDto> data = ReportCollectionService.GetTop10TermUsageData(sourceFlag);
            return JsonConvert.SerializeObject(data);
        }
        #endregion

        [HttpPost]
        public async Task<string> GetRootNodeOfDefaultTermTree([FromBody] TreePage tree)
        {
            if (string.IsNullOrEmpty(tree.NodeId))
            {
                Logger.Warn("NodeId is null or empty");
                throw new ArgumentNullException("NodeId");
            }
            if (tree.NodeType.Equals("TermSet", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(tree.NodeId, out Guid termSetId) && !(await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, Contract.RMWeb.CP.SecurityTermLevel.TermSet, new List<Guid> { termSetId })))
                {
                    return "";
                }
                return TaxonomyService.GetTermSetByTermSetId(tree.NodeId);
            }
            else
            {
                if (Guid.TryParse(tree.NodeId, out Guid termId) && !(await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, Contract.RMWeb.CP.SecurityTermLevel.TermSet, new List<Guid> { termId })))
                {
                    return "";
                }
                return TaxonomyService.GetTermByTermId(tree.NodeId);
            }
        }

        [HttpPost]
        public Task<string> GetChildrenTreeNodes([FromBody] TreePage tree)
        {
            int pIndex = tree.PageIndex ?? 0;
            int pSize = tree.PageSize ?? 0;

            //调整一下index，和前台匹配
            if (pIndex > 0)
            {
                pIndex -= 1;
            }

            string nodeId = tree.NodeId ?? string.Empty;
            string nodeType = tree.NodeType ?? string.Empty;
            int SettingType = tree.SettingType != null ? Convert.ToInt32(tree.SettingType) : 0;
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId
            };
            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, SettingType, filterOption);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public Task<bool> SaveBarcodeStandard([FromBody]int barcodeType)
        {
            return ExplorerService.SaveBarcodeStandardAsync(barcodeType);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, PermissionJoinType.Any)]

        public Task<int> GetBarcodeStandard()
        {
            return ExplorerService.GetBarcodeStandardAsync();
        }
        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ManageHold)]
        public async Task<List<LocationPermissionDto>> GetEffectiveLocationPermissions()
        {
            return await ExplorerService.GetEffectiveLocationPermissionsAsync();
        }
    }
}
