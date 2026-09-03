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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidPhysicalExplorerActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidPhysicalExplorerActionFilter));
        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        public IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        public IPhysicalRequestDao PhysicalRequestDao => PlatformWindsorManager.GetService<IPhysicalRequestDao>();

        public IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private string action;
        public ValidPhysicalExplorerActionFilter()
        {

        }
        public ValidPhysicalExplorerActionFilter(string type)
        {
            action = type;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            //var isPhysicalAdmin = SecurityTrimmingHelper.DoesUserHasThisPermission(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
            //var isPhyEndUser = !isPhysicalAdmin && SecurityTrimmingHelper.DoesUserHasThisPermission(Contract.RoleAssignments.RMPermissionMasks.PhysicalEndUser);
            var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj != null)
            {
                var validateResult = await ValidateActionParameterAsync(parmObj, userAndGroupIds);
                if (validateResult.MessageType == RAMessageType.Failed)
                {
                    actionContext.Result = new ObjectResult(validateResult.ErrorMessage) { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }

            if (action.Equals("ValidateNewRequest"))
            {
                if (parmObj is PhysicalRequestDto newRequestDto)
                {
                    var phy = newRequestDto.PhysicalFileInfo;
                    if (phy.NodeType == RMNodeType.PhyBox || phy.NodeType == RMNodeType.PhyFile)
                    {
                        var validateTermResult = await ValidateTermPermissionAsync(new List<Guid>() { phy.TermId });
                        if (validateTermResult.MessageType == RAMessageType.Failed)
                        {
                            actionContext.Result = new ObjectResult("RM_NotPermission_ForUsedTerm") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                    }
                }
            }
        }

        private async Task<RAReturnMessage> ValidateActionParameterAsync(object parmObj, List<int> userAndGroupIds)
        {
            var returnMessage = new RAReturnMessage();
            var isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            var isPhyEndUser = !isPhysicalAdmin && (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalEndUser));
            var locationId = ""; //需要验证的location节点Id
            var recordsId = new List<Guid>(); //需要验证的Box/Folder节点Ids
            if (action.Equals("BrowseTree"))
            {
                if (parmObj is RMPhysicalExplorerNode phyExplorerNode && phyExplorerNode != null)
                {
                    if (phyExplorerNode.NodeType != (int)RMNodeLevel.PhysicalRootLocation)
                    {
                        var nodeId = phyExplorerNode.Id;
                        if (phyExplorerNode.NodeType == (int)RMNodeLevel.PhysicalBottomLocation
                        || phyExplorerNode.NodeType == (int)RMNodeLevel.PhysicalNormalLocation)
                        {
                            if (isPhyEndUser)
                            {
                                locationId = nodeId;
                            }
                        }
                        else
                        {
                            if (Guid.TryParse(nodeId, out Guid nodeUniqueId))
                            {
                                recordsId = new List<Guid> { nodeUniqueId };
                            }
                        }
                    }
                }
            }
            if (action.Equals("SavePhysicalPermission"))
            {
                if (parmObj is ScopePermissionSimpleDto scopePermissionDto && scopePermissionDto != null)
                {
                    var nodeIds = scopePermissionDto.ScopeIds;
                    var nodeUniqueIds = nodeIds.ConvertAll(o => new Guid(o));
                    var LocationDao = new RMLocationDao();
                    var isLocationNode = false;
                    if (nodeUniqueIds.Count == 1)
                    {
                        var nodeUniqueId = nodeUniqueIds.FirstOrDefault();
                        var locaiton = LocationDao.GetLocationByUniqueId(nodeUniqueId);
                        if (locaiton != null)
                        {
                            locationId = nodeUniqueId.ToString();
                            isLocationNode = true;
                        }
                    }
                    if (!isLocationNode)
                    {
                        recordsId = nodeUniqueIds;
                    }
                }
            }
            if (action.Equals("GetBreakOrInheritPermission"))
            {
                if (parmObj is String nodeId && !String.IsNullOrEmpty(nodeId))
                {
                    if (Guid.TryParse(nodeId, out Guid nodeUniqueId))
                    {
                        var isLocationNode = false;
                        var LocationDao = new RMLocationDao();
                        var locaiton = LocationDao.GetLocationByUniqueId(nodeUniqueId);
                        if (locaiton != null)
                        {
                            isLocationNode = true;
                            locationId = nodeId;
                        }
                        if (!isLocationNode)
                        {
                            recordsId = new List<Guid>() { nodeUniqueId };
                        }
                    }
                }
            }
            if (action.Equals("MultiplePhysicalObjects"))
            {
                if (parmObj is List<PhysicalObjectDto> dtos && dtos.Count > 0)
                {
                    recordsId = dtos.Select(o => o.Id).Distinct().ToList();
                }
            }
            if (action.Equals("RemovePersonalHold"))
            {
                if (parmObj is List<Guid> nodeIds)
                {
                    recordsId = nodeIds;
                }
            }
            if (action.Equals("GetPhysicalObjectById"))
            {
                if (parmObj is String nodeId && !string.IsNullOrEmpty(nodeId))
                {
                    if (int.TryParse(nodeId, out int locationNodeId))
                    {
                        var LocationDao = new RMLocationDao();
                        var locaiton = LocationDao.GetLocationById(locationNodeId);
                        locationId = locaiton.UniqueId.ToString();
                    }
                    else
                    {
                        recordsId = new List<Guid> { new Guid(nodeId) };
                    }
                }
            }
            if (action.Equals("GetPhysicalObjectList"))
            {
                if (parmObj is PhysicalExplorerQueryDto dto && dto != null)
                {
                    if (dto.CurrentNodeType != RMNodeLevel.PhysicalRootLocation)
                    {
                        var nodeId = dto.NodeId;
                        if (dto.CurrentNodeType == RMNodeLevel.PhysicalBottomLocation || dto.CurrentNodeType == RMNodeLevel.PhysicalNormalLocation)
                        {
                            if (int.TryParse(nodeId, out int locationNodeId1))
                            {
                                var LocationDao = new RMLocationDao();
                                var locaiton = LocationDao.GetLocationById(locationNodeId1);
                                locationId = locaiton.UniqueId.ToString();
                            }
                        }
                        else
                        {
                            recordsId = new List<Guid> { new Guid(nodeId) };
                        }
                    }
                }
            }
            if (action.Equals("ValidateExportBarcode"))
            {
                if (parmObj is ExportBarcodeDto dto && dto != null)
                {
                    if (dto.NodeType != RMNodeType.PhysicalRootLocation)
                    {
                        var nodeId = dto.NodeId;
                        if (dto.NodeType == RMNodeType.PhysicalNormalLocation || dto.NodeType == RMNodeType.PhysicalBottomLocation)
                        {
                            locationId = nodeId.ToString();
                        }
                        else
                        {
                            recordsId = new List<Guid> { nodeId };
                        }
                    }
                }
            }
            if (action.Equals("ValidateLocationByUniqueId"))
            {
                if (parmObj is Guid nodeId)
                {
                    locationId = nodeId.ToString();
                }
            }
            if (action.Equals("ValidateNewRequest"))
            {
                //if (parmObj is PhysicalRequestDto newRequestDto)
                //{
                //    var phyDto = newRequestDto.PhysicalFileInfo;
                //    switch (phyDto.NodeType)
                //    {
                //        case RMNodeType.PhyBox:
                //            locationId = phyDto.LocationId.ToString();
                //            break;
                //        case RMNodeType.PhyFile:
                //            var boxId = phyDto.BoxId;
                //            if (boxId != Guid.Empty)
                //            {
                //                recordsId = new List<Guid> { boxId };
                //            } else { 
                //                locationId = phyDto.LocationId.ToString();
                //            }
                //            break;
                //        case RMNodeType.PhyRecord:
                //            recordsId = new List<Guid> { phyDto.FileId };
                //            break;
                //        default:
                //            break;
                //    }
                //}
            }
            if (action.Equals("ValidateLoanRequest"))
            {
                if (parmObj is LoanRequestDto loanRequestDto && loanRequestDto.Items.Count > 0)
                {
                    recordsId = loanRequestDto.Items.Select(o => new Guid(o.Id)).ToList();
                }
            }
            if (action.Equals("GetRequest"))
            {
                if (isPhyEndUser && int.TryParse(parmObj?.ToString(), out int requestId))
                {
                    var validRequestResult = ValidateGetRequest(requestId);
                    if (validRequestResult.MessageType == RAMessageType.Failed)
                    {
                        return validRequestResult;
                    }
                }
            }
            using (new PerformanceScope("ValidPhysicalExplorerAction"))
            {
                //验证location节点
                if (isPhyEndUser && !string.IsNullOrEmpty(locationId))
                {
                    var validLocationResult = ValidateLocationNodePermission(locationId, userAndGroupIds);
                    if (validLocationResult.MessageType == RAMessageType.Failed)
                    {
                        return validLocationResult;
                    }
                }
                //验证非location节点
                if (recordsId.Count > 0)
                {
                    if (isPhyEndUser)
                    {
                        var validateNodeResult = ValidatePhysicalNodePermission(recordsId, userAndGroupIds);
                        if (validateNodeResult.MessageType == RAMessageType.Failed)
                        {
                            return validateNodeResult;
                        }
                    }
                }
            }
            return returnMessage;
        }

        /// <summary>
        /// 验证location节点
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="userAndGroupIds"></param>
        /// <returns></returns>
        public RAReturnMessage ValidateLocationNodePermission(string nodeId, List<int> userAndGroupIds)
        {
            var returnMessage = new RAReturnMessage();
            var noPhyDataPermissionMsg = "no location node permission";
            var idPath = PermissionManagementService.GetScopeIdFullPath(nodeId);
            var hasScopePermission = PermissionManagementService.HasCurrentScopePermission(idPath, userAndGroupIds);
            if (!hasScopePermission)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = noPhyDataPermissionMsg;
            }
            return returnMessage;
        }

        /// <summary>
        /// 验证非location节点(box/folder)
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="userAndGroupIds"></param>
        /// <returns></returns>
        public RAReturnMessage ValidatePhysicalNodePermission(List<Guid> nodeIds, List<int> userAndGroupIds)
        {
            var returnMessage = new RAReturnMessage();
            var errorMsg = "no phy node permission";
            var nodePermissionIds = ExplorerService.GetPhysicalObjectPermissionIds(nodeIds);
            if (nodePermissionIds.Count > 0)
            {
                var scopePermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                if (nodePermissionIds.Any(o => !scopePermissionIds.Contains(o) && o != 0))
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = errorMsg;
                }
            }
            return returnMessage;
        }

        public async Task<RAReturnMessage> ValidateTermPermissionAsync(List<Guid> usedTermIds)
        {
            var returnMessage = new RAReturnMessage();
            var errorMsg = "no term permission";
            ExplorerDao ExplorerDao = new ExplorerDao();
            if (!(await HaveTermPermissionAsync(usedTermIds)))
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = errorMsg;
            }
            return returnMessage;
        }

        public RAReturnMessage ValidateGetRequest(int requestId)
        {
            var returnMessage = new RAReturnMessage();
            var errorMsg = "no request permission";
            var request = PhysicalRequestDao.GetRequest(requestId);
            if (request != null && request.Count > 0)
            {
                if (!request[0].CreatedUserId.Equals(TenantLocalValue.LogonUserId, StringComparison.OrdinalIgnoreCase))
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = errorMsg;
                }
            }
            return returnMessage;
        }

        public async Task<bool> HaveTermPermissionAsync(List<Guid> usedTermIds)
        {
            bool hasPermission = true;
            try
            {
                var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                //目前只有ValidateNewRequest时, 会去按照Content Sources 中的Settings去验证
                QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                {
                    Level = SecurityTermLevel.Term,
                    UserAndGroupIds = userAndGroupIds,
                    FilterByContentSource = true,
                    ExcludeBuiltIn = true,
                    SourceFlag = SourceFlag.Physical,
                    ForPhysicalView = true
                };
                hasPermission = SecurityGroupDao.DoesUserHasPermisionToTerm(usedTermIds, dto);

                if (!hasPermission)
                {
                    logger.Info("No term permission.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred in checking term permission. Error:{e}");
            }
            return hasPermission;
        }
    }
}