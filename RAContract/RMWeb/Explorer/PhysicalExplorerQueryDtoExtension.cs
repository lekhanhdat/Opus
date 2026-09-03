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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public static class PhysicalExplorerQueryDtoExtension
    {
        public static ExplorerQueryOptionV2 GetDefaultQueryOptionV2()
        {
            var filterOptionV2 = new ExplorerFilterOptionV2
            {
                SourceFlags = new List<SourceFlag> { SourceFlag.Physical },
            };
            //default node types
            filterOptionV2.NodeTypes = filterOptionV2.GetDefaultPhysicalNodeTypes().Select(o => (RMNodeLevel)(o)).ToList();
            //default status
            filterOptionV2.Status = RecordStatusHelper.GetDefaultPhysicalStatus().ToList();
            var queryOptionV2 = new ExplorerQueryOptionV2
            {
                FilterOption = filterOptionV2
            };

            return queryOptionV2;
        }
        public static ExplorerQueryV2Dto Convert2ExplorerQueryV2Dto(this PhysicalExplorerQueryDto queryDto, SecurityTermPermissionDto securityTermPermission, bool withoutPhysicalRecord = false)
        {
            var queryOptionV2 = GetDefaultQueryOptionV2();
            var filterOptionV2 = queryOptionV2.FilterOption;
            var result = new ExplorerQueryV2Dto
            {
                QueryOption = queryOptionV2,
                PagingInfo = queryDto.PagingInfo.Convert2ExplorerPagingInfo()
            };
            bool hasFilterOrSearchCase = false;
            //search key
            if (!string.IsNullOrEmpty(queryDto.FilterOption.SearchKey))
            {
                hasFilterOrSearchCase = true;
                queryOptionV2.SearchOption = new ExplorerSearchOptionV2
                {
                    Key = queryDto.FilterOption.SearchKey.Trim().ToLower(),
                    Columns = new List<ExplorerQueryColumn>
                    {
                        new ExplorerQueryColumn {
                            Id = DefaultColumnIDs.NameOrTitle
                        },
                        new ExplorerQueryColumn {
                            Id = DefaultColumnIDs.UniqueId
                        }
                    }
                };
            }
            //global term id
            //remove term permission filter
            //ProcessSecurityTermQuery(filterOptionV2, securityTermPermission);
            if (queryDto != null)
            {
                if (queryDto.NodeId != null)
                {
                    if (queryDto.FilterOption != null)
                    {
                        //term id
                        if (queryDto.FilterOption.TermTreeFilter != null && queryDto.FilterOption.TermTreeFilter != Guid.Empty)
                        {
                            if (queryDto.CurrentNodeType != RMNodeLevel.PhysicalFile)
                            {
                                filterOptionV2.TermIds = new List<Guid> { queryDto.FilterOption.TermTreeFilter };
                            }
                        }
                        //node type
                        if (queryDto.FilterOption.NodeType != RMNodeLevel.Undefined)
                        {
                            hasFilterOrSearchCase = true;
                            // -4 为前后台约定好的值，在传递 -4 的时候，表示搜索所有类型。
                            if (queryDto.FilterOption.NodeType != RMNodeLevel.RMSelectAll)
                            {
                                filterOptionV2.NodeTypes = new List<RMNodeLevel> { queryDto.FilterOption.NodeType };
                            }
                        }

                        //RecordStatus                        
                        if (queryDto.FilterOption.Status != 0)
                        {
                            hasFilterOrSearchCase = true;
                            // -1 为前后台约定好的值，在传递 -1 的时候，表示搜索所有类型。
                            if (queryDto.FilterOption.Status != -1)
                            {
                                filterOptionV2.Status = new List<RMRecordStatus> { (RMRecordStatus)queryDto.FilterOption.Status };
                            }
                        }
                        //RecordOwner
                        if (queryDto.FilterOption.RecordsOwner != null && queryDto.FilterOption.RecordsOwner.Count > 0)
                        {
                            hasFilterOrSearchCase = true;
                            filterOptionV2.Owners = queryDto.FilterOption.RecordsOwner.Select(o => new AOSUserDto { RMUserId = int.Parse(o) }).ToList();
                        }
                        //CreatedBy
                        if (queryDto.FilterOption.CreatedBy != null && queryDto.FilterOption.CreatedBy.Count > 0)
                        {
                            hasFilterOrSearchCase = true;
                            filterOptionV2.CreatedBy = queryDto.FilterOption.CreatedBy.Select(o => new AOSUserDto { DisplayName = o }).ToList();
                        }
                        //ModifiedBy
                        if (queryDto.FilterOption.ModifiedBy != null && queryDto.FilterOption.ModifiedBy.Count > 0)
                        {
                            hasFilterOrSearchCase = true;
                            filterOptionV2.ModifiedBy = queryDto.FilterOption.ModifiedBy.Select(o => new AOSUserDto { DisplayName = o }).ToList();
                        }

                        if (hasFilterOrSearchCase)
                        {
                            GenerateDeepQueryExpression(queryDto.CurrentNodeType, new Guid(queryDto.NodeId), filterOptionV2);
                        }
                        else
                        {
                            GenerateShallowQueryExpression(queryDto.CurrentNodeType, new Guid(queryDto.NodeId), filterOptionV2);
                        }
                    }
                    else
                    {
                        GenerateShallowQueryExpression(queryDto.CurrentNodeType, new Guid(queryDto.NodeId), filterOptionV2);
                    }
                }
            }

            if (withoutPhysicalRecord && filterOptionV2.NodeTypes != null)
            {
                //remove record node type
                filterOptionV2.NodeTypes.RemoveAll(o => o == RMNodeLevel.PhysicalRecord);
            }
            //permission id
            GeneratePermissionQueryExpression(filterOptionV2, queryDto.PermissionIds, queryDto.HaveCurrentNodePermission);
            return result;
        }

        public static void ProcessSecurityTermQuery(ExplorerFilterOptionV2 filterOption, SecurityTermPermissionDto permissionDto)
        {
            switch (permissionDto.TermPermissionType)
            {
                case TermPermissionMethod.All:
                    break;
                case TermPermissionMethod.SpecifyScope:
                    var termIds = permissionDto.TermObjIds;
                    filterOption.TermIds = termIds;
                    filterOption.TermIds.Add(Guid.Empty);
                    break;
                case TermPermissionMethod.None:
                    filterOption.TermIds = new List<Guid> { Guid.Empty };
                    break;
                default:
                    break;
            }

        }

        public static void GenerateShallowQueryExpression(RMNodeLevel currentNodeLevel, Guid nodeId, ExplorerFilterOptionV2 filterOptionV2)
        {
            filterOptionV2.ParentIds = new List<Guid> { nodeId };
            filterOptionV2.PhycialModel = PhysicalSearchModel.Shallow;
            switch (currentNodeLevel)
            {
                case RMNodeLevel.PhysicalBottomLocation:
                    filterOptionV2.NodeTypes = GetDefaultPhysicalContainerNodeTypes();
                    filterOptionV2.PhysicalLocationIds = new List<Guid> { nodeId };
                    filterOptionV2.PhysicalBoxIds = new List<Guid> { Guid.Empty };
                    break;
                case RMNodeLevel.PhysicalCustom:
                    filterOptionV2.PhysicalSearchNodeLevel = RMNodeLevel.PhysicalCustom;
                    filterOptionV2.NodeTypes = GetDefaultPhysicalContainerNodeTypes();
                    break;
                case RMNodeLevel.PhysicalBox:
                    filterOptionV2.NodeTypes = new List<RMNodeLevel> { RMNodeLevel.PhysicalFile };
                    filterOptionV2.PhysicalBoxIds = new List<Guid> { nodeId };
                    break;
                case RMNodeLevel.PhysicalFile:
                    filterOptionV2.NodeTypes = new List<RMNodeLevel> { RMNodeLevel.PhysicalRecord };
                    filterOptionV2.PhysicalFileIds = new List<Guid> { nodeId };
                    break;
                case RMNodeLevel.PhysicalRecord:
                case RMNodeLevel.Undefined:
                default:
                    break;
            }
        }

        public static void GenerateDeepQueryExpression(RMNodeLevel currentNodeLevel, Guid nodeId, ExplorerFilterOptionV2 filterOptionV2)
        {
            filterOptionV2.Ancestor = nodeId;
            filterOptionV2.PhycialModel = PhysicalSearchModel.Deep;
            //无需处理RMNodeLevel.PhysicalCustom，因为在新的逻辑中，已经通过Ancestor来处理了
            switch (currentNodeLevel)
            {
                case RMNodeLevel.PhysicalBottomLocation:
                    filterOptionV2.PhysicalLocationIds = new List<Guid> { nodeId };
                    break;
                case RMNodeLevel.PhysicalBox:
                    filterOptionV2.PhysicalBoxIds = new List<Guid> { nodeId };
                    break;
                case RMNodeLevel.PhysicalFile:
                    filterOptionV2.PhysicalFileIds = new List<Guid> { nodeId };
                    break;
                case RMNodeLevel.PhysicalRecord:
                case RMNodeLevel.Undefined:
                default:
                    break;
            }
        }

        private static List<RMNodeLevel> GetDefaultPhysicalContainerNodeTypes()
        {
            return new List<RMNodeLevel> { RMNodeLevel.PhysicalCustom, RMNodeLevel.PhysicalBox, RMNodeLevel.PhysicalFile };
        }

        /// <summary>
        /// deal with permission ids
        /// </summary>
        /// <param name="filterOptionV2"></param>
        /// <param name="permissionIds"></param>
        /// <param name="hasCurrentNodePermission"></param>
        public static void GeneratePermissionQueryExpression(ExplorerFilterOptionV2 filterOptionV2, List<int> permissionIds, bool hasCurrentNodePermission)
        {
            if (permissionIds == null) return;
            var includePermissionIds = new List<int>();
            var excludePermissionIds = new List<int>();

            var isQueryAllPermissionData = true;
            if (!hasCurrentNodePermission)
            {
                //user对当前节点没有权限
                if (permissionIds.Count == 0)
                {
                    //子节点没有有权限的数据时，-1代表不返回数据
                    permissionIds.Add(-1);
                    isQueryAllPermissionData = false;
                }
                includePermissionIds.AddRange(permissionIds);
            }
            else
            {
                //user对当前节点有权限
                excludePermissionIds.AddRange(permissionIds);
            }

            if (includePermissionIds.Count > 0 || excludePermissionIds.Count > 0)
            {
                if (isQueryAllPermissionData)
                {
                    ////需要load没有scopePermissionId属性的老数据
                    //permissionConditions.Add($"NOT IS_DEFINED(c.scopePermissionId)");
                    ////没有设置全新的数据，默认值是0
                    //permissionConditions.Add($"c.scopePermissionId = 0");
                    includePermissionIds.Add(0);
                }
            }

            if (includePermissionIds.Count > 0) filterOptionV2.PersmissionScopes = includePermissionIds;
            if (excludePermissionIds.Count > 0) filterOptionV2.ExcludePersmissionScopes = excludePermissionIds;


        }
    }

    public static class PhysicalExplorerPagingInfoExtension
    {
        public static ExplorerPagingInfo Convert2ExplorerPagingInfo(this PhysicalExplorerPagingInfo pagingInfo)
        {
            return new ExplorerPagingInfo
            {
                PageIndex = pagingInfo.currentBrowserState,
                PageSize = pagingInfo.PageSize,
                HasNextPage = pagingInfo.HasNextPage,
                Total = pagingInfo.Total
            };
        }
    }
}
