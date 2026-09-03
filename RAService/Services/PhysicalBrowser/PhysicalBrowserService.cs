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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.PhysicalBrowserService;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZXing;

namespace AvePoint.RA.Service.Services.PhysicalBrowser
{
    //与PM 确认，暂时不需要Browser 的audit 的记录
    public class PhysicalBrowserService : RMServiceBase, IPhysicalBrowserService
    {
        private RALogger logger = RALogger.GetInstance(typeof(PhysicalBrowserService));

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();

        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IRMSuiteDao SuiteDao => PlatformWindsorManager.GetService<IRMSuiteDao>();
        private IRMTemplateRelationshipDao TemplateRelationshipDao => PlatformWindsorManager.GetService<IRMTemplateRelationshipDao>();

        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        public async Task<List<RMPhysicalExplorerNode>> InitTreeAsync(int pageCount = 15)
        {
            logger.Info($"Begin init tree.");
            List<int> userAndGroupIds = new List<int>();
            var isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
            var isHoldManager = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManageHold);
            if (!isPhysicalAdmin && !isHoldManager)
            {
                userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
            }
            List<RMPhysicalExplorerNode> physicalTreeNodes = new List<RMPhysicalExplorerNode>();
            var rootLocations = LocationDao.LoadRootNode(0, pageCount);
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            List<Guid> locationPermissionScopeIds = userPermission.ScopePermissionInfo.FirstOrDefault(p => p.DataSourceType == SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
            foreach (var rootlocation in rootLocations)
            {
                var rootTreeNode = ConvertToRMPhysicalExplorerNode(rootlocation, string.Empty);
                var subLocations = new List<RMLocation>();
                if (isPhysicalAdmin && !isHoldManager)
                {
                    if(userPermission.IsAdmin)
                    {
                        subLocations = LocationDao.GetSubLocationByParentId(rootlocation.Id, 0, pageCount);
                    }
                    else
                    {
                        subLocations = LocationDao.GetTopLocationByParentIdAndId(rootlocation.Id, 0, pageCount, locationPermissionScopeIds);
                    }
                }
                else
                {
                     subLocations = LocationDao.GetSubLocationByParentId(rootlocation.Id, 0, pageCount, userAndGroupIds);
                }
                var subLocationsTree = ConvertToRMPhysicalExplorerNode(subLocations, rootlocation.Id.ToString());
                var scopeBreakInherDic = PermissionManagementService.GetScopeBreakInherMapping(subLocationsTree.Select(o => o.Id).ToList());
                subLocationsTree.ForEach(s =>
                {
                    s.BreakInheritance = scopeBreakInherDic[s.Id];
                    if (s.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                    {
                        //s.HasChildren = ExplorerDao.QueryAll(GetBrowserLambda(s)).Count() > 0;
                        s.HasChildren = ExplorerDao.Exist(GetBrowserLambda(s));
                    }
                    else
                    {
                        s.HasChildren = LocationDao.CountSubLocation(int.Parse(s.Id)) > 0;
                    }
                });
                rootTreeNode.Children = subLocationsTree;
                if (isPhysicalAdmin)
                {
                    if (userPermission.IsAdmin)
                    {
                        rootTreeNode.ChildrenCount = LocationDao.CountSubLocation(rootlocation.Id);
                    }
                    else
                    {
                        rootTreeNode.ChildrenCount = LocationDao.CountSubLocationByLocationIds(rootlocation.Id, locationPermissionScopeIds);
                    }
                }
                else if (isHoldManager)
                {
                    rootTreeNode.ChildrenCount = LocationDao.CountSubLocation(rootlocation.Id);
                }
                else
                {
                    rootTreeNode.ChildrenCount = LocationDao.CountSubLocation(rootlocation.Id, userAndGroupIds);
                }
                rootTreeNode.HasChildren = rootTreeNode.ChildrenCount > 0;
                physicalTreeNodes.Add(rootTreeNode);
            }
            logger.Info($"Finish init tree.");
            return physicalTreeNodes;
        }

        public async Task<List<RMPhysicalExplorerNode>> InitTreeAsync(Guid recordId, int pageCount = 15)
        {
            logger.Info($"Begin init tree by box: {recordId}.");

            var boxDto = await ExplorerService.GetPhysicalObjectByIdAsync(recordId);
            if (boxDto == null)
            {
                logger.Warn($"Could not find physical object with Id: {recordId}. Falling back to full tree load.");
                return await InitTreeAsync(pageCount);
            }

            if (boxDto.NodeType == RMNodeType.PhyRecord)
            {
                return await LoadTreeByNearParentLocationAsync(boxDto.ParentId, pageCount);
            }

            logger.Info($"Box Id: {recordId} is not a record. Loading all locations.");
            return await InitTreeAsync(pageCount);
        }

        private async Task<List<RMPhysicalExplorerNode>> LoadTreeByNearParentLocationAsync(Guid parentId, int pageCount)
        {
            logger.Info($"Loading near parent location for parent Id: {parentId}.");

            var parentDto = await ExplorerService.GetPhysicalObjectByIdAsync(parentId);
            if (parentDto == null)
            {
                logger.Warn($"Could not find parent physical object with Id: {parentId}. Falling back to full tree load.");
                return await InitTreeAsync(pageCount);
            }

            var location = LocationDao.GetLocationByUniqueId(parentDto.LocationId);
            if (location == null)
            {
                logger.Warn($"Could not find location with UniqueId: {parentDto.LocationId}. Falling back to full tree load.");
                return await InitTreeAsync(pageCount);
            }

            var locationNode = ConvertToRMPhysicalExplorerNode(location, string.Empty);
            locationNode.PagerIndex = 0;
            locationNode.PagerSize = pageCount;

            var browserResult = await GetSubNodesInfoAsync(locationNode);
            locationNode.Children = browserResult.Item1;
            locationNode.HasChildren = browserResult.Item1.Count > 0;

            logger.Info($"Finish loading near parent location for parent Id: {parentId}.");
            return new List<RMPhysicalExplorerNode> { locationNode };
        }

        public async Task<Tuple<bool, PhysicalResultInfo, List<RMPhysicalExplorerNode>, PhysicalObjectDto, bool>> SearchTreeAsync(string uniqueId)
        {
            bool success = false;
            var isPhyRecord = false;
            RMPhysicalExplorerNode rootNode = null;
            RMLocation roomLocation = null;
            PhysicalResultInfo resultTable = new PhysicalResultInfo() { PagingInfo = new PhysicalExplorerPagingInfo() };
            PhysicalObjectDto boxDto = await ExplorerService.FindPhysicalObjectByRecordsIdAsync(uniqueId);
            if (boxDto == null)
            {
                logger.Warn($"Couldn't found record which UniqueId: {uniqueId}.");
            } 
            else
            {
                if (boxDto.NodeType == RMNodeType.PhyRecord)
                {
                    boxDto = await ExplorerService.GetPhysicalObjectByIdAsync(boxDto.ParentId);
                    isPhyRecord = true;
                }
                var queryDto = new PhysicalExplorerQueryDto()
                {
                    CurrentNodeType = RMNodeLevel.PhysicalBox, NodeId = boxDto.Id.ToString(), FilterOption = new PhysicalExplorerFilterOption(),
                    PagingInfo = new PhysicalExplorerPagingInfo() { PageIndex = 0, PageSize = 10 }
                };
                if (isPhyRecord)
                {
                    queryDto.FilterOption.SearchKey = uniqueId;
                }
                resultTable = await ExplorerService.QueryPhysicalNodesAsync(queryDto);

                roomLocation = LocationDao.GetLocationByUniqueId(boxDto.LocationId);
                if (roomLocation == null)
                {
                    logger.Warn($"Physical Record's parent location (id: {boxDto.LocationId}) has been deleted. Record UniqueId: {uniqueId}.");
                }
                else
                {
                    var parentIDs = roomLocation.DirPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
                    parentIDs.Add(roomLocation.Id);
                    var rootLocation = LocationDao.GetLocationById(parentIDs[0]);

                    if (rootLocation == null)
                    {
                        ArgumentCheck.NotNull(rootLocation, nameof(rootLocation));
                        logger.Warn($"Physical Record's root location (id: {rootLocation.Id}) not exist. Record UniqueId: {uniqueId}.");
                    }
                    else
                    {
                        rootLocation.Name = rootLocation.Name == "RM_SPS_Location_RootNode" ? I18N.Core.I18NEntity.GetString(rootLocation.Name) : rootLocation.Name;
                        int pagerSize = 10, pagerIndex = 0;
                        var parentId = rootLocation.Id;
                        var parentNode = ConvertToRMPhysicalExplorerNode(rootLocation, null);
                        List<int> userAndGroupIds = new List<int>();
                        var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
                        if (!isAdmin)
                        {
                            userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                        }
                        rootNode = parentNode;
                        for (int i = 1; i < parentIDs.Count; i++)
                        {
                            ArgumentCheck.NotNull(parentNode, nameof(parentNode));
                            RMPhysicalExplorerNode currentChildNode = null;
                            var currentChildId = parentIDs[i];
                            var ids = LocationDao.GetChildIDsOrderByName(parentId, userAndGroupIds);
                            var index = ids.IndexOf(currentChildId);
                            parentNode.ChildrenCount = ids.Count;
                            parentNode.HasChildren = ids.Count > 0;
                            parentNode.Children = new List<RMPhysicalExplorerNode>();
                            if (index >= 0)
                            {
                                pagerIndex = index / 10;
                                parentNode.PagerIndex = pagerIndex;
                                parentNode.HasNextPage = (pagerIndex + 1) * pagerSize < ids.Count;
                                var currentPagerChildIDs = ids.Skip(pagerIndex * pagerSize).Take(pagerSize);
                                var childLocations = LocationDao.GetLocationByIDs(currentPagerChildIDs);
                                foreach (var childId in currentPagerChildIDs)
                                {
                                    var childNode = ConvertToRMPhysicalExplorerNode(childLocations[childId], parentNode.Id);
                                    parentNode.Children.Add(childNode);
                                    if(childId == currentChildId)
                                    {
                                        currentChildNode = childNode;
                                        childNode.HasChildren = true;


                                        //childNode.Children = new List<RMPhysicalExplorerNode>();
                                        //var recordObject = ExplorerDao.GetPhysicalRecordById(boxDto.Id);
                                        //var physicalNode = ConvertToRMPhysicalExplorerNode(recordObject, childNode.Id);
                                        //physicalNode.BreakInheritance = PermissionManagementService.GetBreakOrInheritPermission(boxDto.Id.ToString(), true).BreakInheritStatus;
                                        //physicalNode.Checked = true;
                                        //physicalNode.HasChildren = resultTable.PagingInfo.Total > 0;
                                        //childNode.Children.Add(physicalNode);

                                        currentChildNode = childNode;
                                        childNode.HasChildren = true;

                                        if (childLocations[childId].UniqueId.ToString() == childNode.LocationId)
                                        {
                                            var ancestors = new List<Guid>(boxDto.Ancestors);
                                            ancestors.RemoveAt(0);
                                            if (ancestors.Count > 0)
                                            {
                                                RMPhysicalExplorerNode lastCustomObject = childNode;//buttom location
                                                foreach (var ancestorId in ancestors)
                                                {
                                                    var customRecordObject = ExplorerDao.GetPhysicalRecordById(ancestorId);
                                                    var customPhysicalNode = ConvertToRMPhysicalExplorerNode(customRecordObject, lastCustomObject.Id);
                                                    customPhysicalNode.BreakInheritance = (await PermissionManagementService.GetBreakOrInheritPermissionAsync(customPhysicalNode.Id.ToString(), true)).BreakInheritStatus;
                                                    lastCustomObject.Children = new List<RMPhysicalExplorerNode>();
                                                    lastCustomObject.Children.Add(customPhysicalNode);
                                                    await AssembleTemplateIdPathAsync(lastCustomObject, lastCustomObject.Children);
                                                    lastCustomObject = customPhysicalNode;
                                                }
                                                childNode = lastCustomObject;//last custom object
                                            }
                                            var recordObject = ExplorerDao.GetPhysicalRecordById(boxDto.Id);
                                            var physicalNode = ConvertToRMPhysicalExplorerNode(recordObject, childNode.Id);
                                            physicalNode.BreakInheritance = (await PermissionManagementService.GetBreakOrInheritPermissionAsync(boxDto.Id.ToString(), true)).BreakInheritStatus;
                                            physicalNode.Checked = true;
                                            physicalNode.HasChildren = resultTable.PagingInfo.Total > 0;
                                            childNode.Children = new List<RMPhysicalExplorerNode>();
                                            childNode.Children.Add(physicalNode);
                                            await AssembleTemplateIdPathAsync(childNode, childNode.Children);
                                        }
                                    }
                                    else
                                    {
                                        if (childNode.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                                        {
                                            childNode.HasChildren = ExplorerDao.Exist(GetBrowserLambda(childNode));
                                        }
                                        else
                                        {
                                            childNode.HasChildren = LocationDao.CountSubLocation(childId) > 0;
                                        }
                                    }
                                }
                                parentId = currentChildId;
                                parentNode = currentChildNode;
                            }
                            else
                            {
                                logger.Warn($"Physical Record's parent location (id: {currentChildId}) has been deleted. Record UniqueId: {uniqueId}.");
                                break;
                            }
                        }
                        ArgumentCheck.NotNull(parentNode, nameof(parentNode));
                        await AssembleTemplateIdPathAsync(parentNode, parentNode.Children);
                        success = true;
                    }
                }
            }

            List<RMPhysicalExplorerNode> treeData = null;
            if (success)
            {
                treeData = new List<RMPhysicalExplorerNode> { rootNode };
                boxDto.Template = await TemplateManagementService.LoadTemplateDtoAsync(boxDto.TemplateId, boxDto);
                boxDto.ChildTemplates = await TemplateManagementService.GetTemplatesByPhysicalObject4ExplorerAsync(boxDto);
                var barcodeUtil = new BarcodeUtil();
                var barcode = boxDto.BarcodeId ?? boxDto.UniqueId;
                var isValid = barcodeUtil.PreCheckBarcodeInfo(barcode);
                if (isValid)
                {
                    boxDto.BarcodeBase64Str = barcodeUtil.GetBarcodeImgBase64Str(barcode);
                }
                else
                {
                    boxDto.BarcodeBase64Str = string.Empty;
                }
            }
            else
            {
                treeData = (await InitTreeAsync());
                treeData[0].Checked = true;
            }
            return new Tuple<bool, PhysicalResultInfo, List<RMPhysicalExplorerNode>, PhysicalObjectDto, bool> (
                success,
                resultTable,
                treeData,
                boxDto,
                isPhyRecord
            );
        }

        public async Task<RMPhysicalExplorerNode> BrowserAsync(RMPhysicalExplorerNode currentRecord)
        {
            logger.Debug($"Begin browser tree, nodeId : [{currentRecord.Id}], Name:[{currentRecord.Name}], pageIndex : [{currentRecord.PagerIndex}], pageCount : [{currentRecord.PagerSize}], pagePosition : [{currentRecord.PagePosition}], LeafNodeType : [{currentRecord.LeafNodeType}].");
            
            //currentRecord.ParentId = currentRecord.LocationId;
            var browserResult = await GetSubNodesInfoAsync(currentRecord);
            currentRecord.Children = browserResult.Item1;
            currentRecord.ChildrenCount = browserResult.Item2;
            currentRecord.HasNextPage = browserResult.Item3;
            currentRecord.PagePosition = browserResult.Item4;
            logger.Info($"Finish browser tree, nodeid : [{currentRecord.Id}], children count : {currentRecord.Children.Count}.");
            return currentRecord;
        }


        public async Task<RMPhysicalExplorerNode> BrowserSearchTreeAsync(RMPhysicalExplorerNode currentRecord)
        {
            try
            {
                logger.Debug($"Begin browser search tree, nodeId : [{currentRecord.Id}], Name:[{currentRecord.Name}], pageIndex : [{currentRecord.PagerIndex}], pageCount : [{currentRecord.PagerSize}], pagePosition : [{currentRecord.PagePosition}], LeafNodeType : [{currentRecord.LeafNodeType}], [IsSearch] : [{currentRecord.IsSearch}], [SearchValue] : [{currentRecord.SearchKey}].");
                if (currentRecord.IsSearch)
                {
                    var childNode = SearchNodesInfos(currentRecord);
                    currentRecord.Children = childNode.Item1;
                    currentRecord.ChildrenCount = childNode.Item2;
                    currentRecord.HasNextPage = childNode.Item3;
                    currentRecord.PagePosition = childNode.Item4;
                    currentRecord.IsSearch = true;
                    currentRecord.CanSearch = childNode.Item5;
                    logger.Info($"Finish search browser tree");
                    return currentRecord;
                }

                var browserResult = await GetSubNodesInfoAsync(currentRecord);
                currentRecord.Children = browserResult.Item1;
                currentRecord.ChildrenCount = browserResult.Item2;
                currentRecord.HasNextPage = browserResult.Item3;
                currentRecord.PagePosition = browserResult.Item4;
                logger.Info($"Finish browser tree, nodeid : [{currentRecord.Id}], children count : {currentRecord.Children.Count}.");
                return currentRecord;
            }
            catch (Exception ex)
            {
                logger.Error($"Browser search physical tree have errors:{ex}");
                return currentRecord;
            }
        }

        private Tuple<List<RMPhysicalExplorerNode>, int, bool, string, bool> SearchNodesInfos(RMPhysicalExplorerNode currentRecord)
        {
            var records = ExplorerDao.SearchPhysicalBoxOrFolderByName(currentRecord.SearchKey, string.Empty, 1000, currentRecord.IsGlobalSearch, currentRecord.IsSearchFolder, currentRecord.LocationId);
            RMPhysicalExplorerNode ConvertRecordToPhysicalExplorerNode(Record record)
            {
                var node = new RMPhysicalExplorerNode();
                node.Id = record.Id.ToString();
                node.Name = record.LeafName;
                node.NodeType = record.NodeType;
                node.ParentId = record.ParentId.ToString();
                node.HasChildren = true;//Get from cosmos db later
                node.LocationId = record.LocationId.ToString();
                node.LocationName = LocationDao.GetLocationByUniqueId(record.LocationId)?.Name;
                node.BoxId = record.BoxId.ToString();
                node.FileId = record.FileId.ToString();
                node.RecordStatus = record.RecordStatus;
                node.IsHoldStatus = record.HoldStatus;
                node.OnLoan = this.GetLoanStatus(record);
                node.TemplateId = record.TemplateId;
                node.Expanded = true;
                return node;
            }
            var physicalBoxNodes = records.Item1?.Select(_ => ConvertRecordToPhysicalExplorerNode(_)) ?? [];
            var listLocationId = records.Item1?.Select(_ => _.LocationId).Distinct();
            var locationBottoms = LocationDao.GetLocationBottomByLocationIds(listLocationId);
            var parentIds = locationBottoms.Select(_ => _.DirPath).AsEnumerable().SelectMany(_ => _.Split('/', StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();

            var locationsNormal = LocationDao.GetLocationNormalByIds(parentIds);
            var root = LocationDao.GetRootLocation();
            List<RMPhysicalExplorerNode> locationNodes = new List<RMPhysicalExplorerNode>();
            HashSet<int> skipProcessLocationBottom = new HashSet<int>();
            var rootChilds = locationsNormal.Where(_ => _.ParentId == root.Id).Concat(locationBottoms.Where(_ => _.ParentId == root.Id) ?? []);
            foreach(var rootChild in rootChilds)
            {
                var rootChildNodes = ConvertToRMPhysicalExplorerNode(rootChild, root.Id.ToString());
                rootChildNodes.Children = rootChild.NodeType == (int)RMNodeType.PhysicalBottomLocation ? physicalBoxNodes.Where(_ => _.LocationId.Equals(rootChildNodes.LocationId)).ToList()
                    : BuildPhysicalChildNode(rootChildNodes);
                rootChildNodes.Expanded = true;
                rootChildNodes.ChildrenCount = rootChildNodes.Children?.Count ?? 0;
                locationNodes.Add(rootChildNodes);
            }
            List<RMPhysicalExplorerNode> BuildPhysicalChildNode(RMPhysicalExplorerNode parentNode)
            {
                var childNodes = new List<RMPhysicalExplorerNode>();
                var bottomChildNodes = locationBottoms.Where(_ => _.ParentId.ToString() == parentNode.Id).ConvertAll(_ => ConvertToRMPhysicalExplorerNode(_,parentNode.Id)).ToList();
                foreach(var bottomNode in bottomChildNodes)
                {
                    bottomNode.Children = physicalBoxNodes.Where(_ => _.LocationId.Equals(bottomNode.LocationId)).ToList();
                    bottomNode.Expanded = true;
                    bottomNode.ChildrenCount = bottomNode.Children?.Count ?? 0;
                }
                childNodes.AddRange(bottomChildNodes);
                var normalChildNode = locationsNormal.Where(_ => _.ParentId.ToString() == parentNode.Id).ConvertAll(_ => ConvertToRMPhysicalExplorerNode(_, parentNode.Id)).ToList();
                if(normalChildNode != null && normalChildNode.Count() >= 1)
                {
                    foreach (var normalNode in normalChildNode)
                    {
                        normalNode.Children = BuildPhysicalChildNode(normalNode);
                        normalNode.Expanded = true;
                        normalNode.ChildrenCount = normalNode.Children?.Count ?? 0;
                    }
                    childNodes.AddRange(normalChildNode);
                }
                return childNodes;
            }
            return new Tuple<List<RMPhysicalExplorerNode>, int, bool, string, bool>(locationNodes, locationNodes.Count , false, string.Empty, string.IsNullOrEmpty(records.Item2));
        }

        public List<RMPhysicalExplorerNode> Search(RMPhysicalExplorerNode node, string key)
        {
            throw new NotImplementedException();
        }

        private async Task<Tuple<List<RMPhysicalExplorerNode>, int, bool, string>> GetSubNodesInfoAsync(RMPhysicalExplorerNode currentRecord)
        {
            List<RMPhysicalExplorerNode> subNodes = new List<RMPhysicalExplorerNode>();
            int subNodesTotalCount = 0;
            string pagePosition = string.Empty;
            bool hasNext = false;
            if (currentRecord.NodeType == (int)RMNodeLevel.PhysicalRootLocation)
            {
                List<int> userAndGroupIds = new List<int>();
                var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
              
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
                if (!isAdmin)
                {
                    userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                }
                if (userPermission.IsAdmin)
                {
                    subNodes = GetChildrenNodesFromLocationDB(currentRecord);
                    subNodesTotalCount = LocationDao.CountSubLocation(int.Parse(currentRecord.Id));
                }
                else if (isAdmin)
                {
                    List<Guid> locationPermissionScopeIds = userPermission.ScopePermissionInfo.FirstOrDefault(p => p.DataSourceType == SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
                    subNodes = GetChildrenNodesFromLocationDB(currentRecord, locationPermissionScopeIds);
                    subNodesTotalCount = LocationDao.CountSubLocationByLocationIds(int.Parse(currentRecord.Id), locationPermissionScopeIds);
                }
                else
                {
                    subNodes = GetChildrenNodesFromLocationDB(currentRecord, userAndGroupIds);
                    subNodesTotalCount = LocationDao.CountSubLocation(int.Parse(currentRecord.Id), userAndGroupIds);
                }
                var scopeBreakInherDic = PermissionManagementService.GetScopeBreakInherMapping(subNodes.Select(o => o.Id).ToList());
                subNodes.ForEach(n =>
                {
                    n.BreakInheritance = scopeBreakInherDic[n.Id];
                });
            }
            else if (currentRecord.NodeType == (int)RMNodeLevel.PhysicalNormalLocation)//判断是Location 根节点还是其他节点，来决定是从Location表查询数据还是Cosmos DB查询
            {
                List<int> userAndGroupIds = new List<int>();
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
                if (!isAdmin)
                {
                    userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                }
                if (isAdmin)
                {
                    subNodes = GetChildrenNodesFromLocationDB(currentRecord);
                    subNodesTotalCount = LocationDao.CountSubLocation(int.Parse(currentRecord.Id));
                }
                else
                {
                    subNodes = GetChildrenNodesFromLocationDB(currentRecord, userAndGroupIds);
                    subNodesTotalCount = LocationDao.CountSubLocation(int.Parse(currentRecord.Id), userAndGroupIds);
                }
                var scopeBreakInherDic = PermissionManagementService.GetScopeBreakInherMapping(subNodes.Select(o => o.Id).ToList());
                subNodes.ForEach(n =>
                {
                    n.BreakInheritance = scopeBreakInherDic[n.Id];
                });
            }
            else
            {
                //如果cosmos db 无法在分页查询的时候查到总数，就直接查出来
                var result = await GetChildrenNodesFromCosmosDBAsync(currentRecord);
                subNodes = result.Item1
                            .Select(r => ConvertToRMPhysicalExplorerNode(r, currentRecord.Id))
                            .ToList();
                var scopeBreakInherDic = PermissionManagementService.GetScopeBreakInherMapping(subNodes.Select(o => o.Id).ToList());
                subNodes.ForEach(n =>
                {
                    n.LeafNodeType = currentRecord.LeafNodeType;
                    n.HasChildren = ExplorerDao.Exist(GetBrowserLambda(n));
                    n.BreakInheritance = scopeBreakInherDic[n.Id];
                });

                hasNext = !string.IsNullOrEmpty(result.Item2);
                pagePosition = result.Item2;
                await AssembleTemplateIdPathAsync(currentRecord, subNodes);
            }
            return new Tuple<List<RMPhysicalExplorerNode>, int, bool, string>(subNodes, subNodesTotalCount, hasNext, pagePosition);
        }

        private async System.Threading.Tasks.Task AssembleTemplateIdPathAsync(RMPhysicalExplorerNode currentRecord, List<RMPhysicalExplorerNode> subNodes)
        {
            if (currentRecord.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
            {
                //如果browse的节点是bottom location，那么其子节点所用到的template一定是某一个suite的start template，
                var suiteIds = SuiteDao.GetSuiteIdsByLocationID(new Guid(currentRecord.LocationId));
                var rootTemplates = (await TemplateRelationshipDao.FindListAsync(o => suiteIds.Contains(o.Ancestor) && o.Distance == 1))
                    .Select(o => new { SuiteId = o.Ancestor, TemplateUniqueId = o.Descendant }).ToList(); //get root template
                foreach(var subNode in subNodes)
                {
                    try
                    {
                        var template = TemplateDao.Find(o => o.Id == subNode.TemplateId);
                        var rootTemplate = rootTemplates.FirstOrDefault(o => o.TemplateUniqueId == template.UniqueId);
                        if (rootTemplate == null)
                        {
                            logger.Warn($"Can't get the template in root templates with Unique Id : {template.UniqueId}, Id :{template.Id}, Name : {template.Name}, location id : {currentRecord.LocationId}");
                        }
                        else
                        {
                            subNode.TemplateIdPath = rootTemplate?.SuiteId.ToString() + TemplateUtil.IdPathSeprator + subNode.TemplateId + TemplateUtil.IdPathSeprator;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while get TemplateIdPath for {subNode.Name}, {e.ToString()}");
                    }
                }
            }
            else
            {
                foreach (var subNode in subNodes)
                {
                    subNode.TemplateIdPath = currentRecord.TemplateIdPath + subNode.TemplateId.ToString() + TemplateUtil.IdPathSeprator;
                }
            }
        }

        private List<RMPhysicalExplorerNode> GetChildrenNodesFromLocationDB(RMPhysicalExplorerNode currentRecord, List<int> userAndGroupIds = null)
        {
            List<RMPhysicalExplorerNode> physicalTreeNodes = new List<RMPhysicalExplorerNode>();
            var nodeId = int.Parse(currentRecord.Id);
            var subLocations = new List<RMLocation>();
            if (userAndGroupIds!=null)
            {
                subLocations = LocationDao.GetSubLocationByParentId(nodeId, currentRecord.PagerIndex, currentRecord.PagerSize, userAndGroupIds);
            }
            else
            {
                subLocations = LocationDao.GetSubLocationByParentId(nodeId, currentRecord.PagerIndex, currentRecord.PagerSize);
                
            }
            physicalTreeNodes = ConvertToRMPhysicalExplorerNode(subLocations, currentRecord.Id);
            physicalTreeNodes.ForEach(t =>
            {
                t.LeafNodeType = currentRecord.LeafNodeType;
                if (t.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    //t.HasChildren = ExplorerDao.QueryAll(GetBrowserLambda(t)).Count() > 0;
                    t.HasChildren = ExplorerDao.Exist(GetBrowserLambda(t));
                }
                else
                {
                    t.HasChildren = LocationDao.CountSubLocation(int.Parse(t.Id)) > 0;
                }
            });
            return physicalTreeNodes;
        }

        private List<RMPhysicalExplorerNode> GetChildrenNodesFromLocationDB(RMPhysicalExplorerNode currentRecord, List<Guid> topLocationIds)
        {
            List<RMPhysicalExplorerNode> physicalTreeNodes = new List<RMPhysicalExplorerNode>();
            var nodeId = int.Parse(currentRecord.Id);
            var subLocations = new List<RMLocation>();
            subLocations = LocationDao.GetTopLocationByParentIdAndId(nodeId, currentRecord.PagerIndex, currentRecord.PagerSize, topLocationIds);

            physicalTreeNodes = ConvertToRMPhysicalExplorerNode(subLocations, currentRecord.Id);
            physicalTreeNodes.ForEach(t =>
            {
                t.LeafNodeType = currentRecord.LeafNodeType;
                if (t.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    //t.HasChildren = ExplorerDao.QueryAll(GetBrowserLambda(t)).Count() > 0;
                    t.HasChildren = ExplorerDao.Exist(GetBrowserLambda(t));
                }
                else
                {
                    t.HasChildren = LocationDao.CountSubLocation(int.Parse(t.Id)) > 0;
                }
            });
            return physicalTreeNodes;
        }

        private async Task<Tuple<IEnumerable<Record>, string>> GetChildrenNodesFromCosmosDBAsync(RMPhysicalExplorerNode currentRecord)
        {
            var termPermDto = await ExplorerService.GetSecurityTermDtoAsync();
            var (permissionIds, hasScopePermission) = await GetPermissonConditionAsync(currentRecord);
            return ExplorerDao.QueryPageBySqlForBrowse(currentRecord, permissionIds, hasScopePermission, currentRecord.PagerSize, currentRecord.PagePosition, termPermDto);
        }

        private List<RMPhysicalExplorerNode> ConvertToRMPhysicalExplorerNode(List<RMLocation> mLocations, string parentNodeId)
        {
            List<RMPhysicalExplorerNode> physicalTreeNodes = new List<RMPhysicalExplorerNode>();
            if (mLocations != null && mLocations.Count > 0)
            {
                physicalTreeNodes = mLocations.Select(l => ConvertToRMPhysicalExplorerNode(l, parentNodeId)).ToList();
            }
            return physicalTreeNodes;
        }

        private RMPhysicalExplorerNode ConvertToRMPhysicalExplorerNode(RMLocation location, string parentNodeId)
        {
            var physicalTreeNodes = new RMPhysicalExplorerNode();
            physicalTreeNodes.Id = location.Id.ToString();
            physicalTreeNodes.Name = location.Name;
            physicalTreeNodes.NodeType = location.NodeType;
            physicalTreeNodes.ParentId = parentNodeId;
            physicalTreeNodes.LocationId = location.UniqueId.ToString();
            physicalTreeNodes.LocationName = location.Name;
            return physicalTreeNodes;
        }

        private RMPhysicalExplorerNode ConvertToRMPhysicalExplorerNode(Record record, string parentNodeId)
        {
            var node = new RMPhysicalExplorerNode();
            node.Id = record.Id.ToString();
            node.Name = record.LeafName;
            node.NodeType = record.NodeType;
            node.ParentId = parentNodeId;
            node.HasChildren = true;//Get from cosmos db later
            node.LocationId = record.LocationId.ToString();
            node.LocationName = LocationDao.GetLocationByUniqueId(record.LocationId)?.Name;
            node.BoxId = record.BoxId.ToString();
            node.FileId = record.FileId.ToString();
            node.RecordStatus = record.RecordStatus;
            node.IsHoldStatus = record.HoldStatus;
            node.OnLoan = this.GetLoanStatus(record);
            node.TemplateId = record.TemplateId;
            return node;
        }

        private bool GetLoanStatus(Record record)
        {
            try
            {
                Dictionary<string, string> metaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
                if (metaInfo.TryGetValue(DefaultColumnIDs.LoanedBy, out string loanColumn))
                {
                    List<AOSUserDto> userList = JsonConvert.DeserializeObject<List<AOSUserDto>>(loanColumn);
                    ArgumentCheck.NotNull(userList, nameof(userList));
                    return userList?.Count > 0;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"ConvertToRMPhysicalExplorerNode get loan status error: {e}");
            }
            return false;
        }

        private Expression<Func<Record, bool>> GetBrowserLambda(RMPhysicalExplorerNode node)
        {
            Expression queryExpress = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            List<Expression> nodeTypeExpressionList = new List<Expression>();
            switch (node.NodeType)
            {
                case (int)RMNodeLevel.PhysicalBottomLocation:
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalBox));
                    if (node.LeafNodeType == 0 || node.LeafNodeType > (int)RMNodeLevel.PhysicalFile)
                    {
                        nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
                    }
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", new Guid(node.LocationId)));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId",  Guid.Empty));
                    break;
                case (int)RMNodeLevel.PhysicalBox:
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", new Guid(node.Id)));
                    break;
                case (int)RMNodeLevel.PhysicalFile:
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalRecord));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "FileId", new Guid(node.Id)));
                    break;
                default:
                    break;
            }
            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ParentId", node.NodeType == (int)RMNodeLevel.PhysicalBottomLocation? new Guid(node.LocationId): new Guid(node.Id)));

            allExpressionList.Add(nodeTypeExpressionList.Aggregate(Expression.OrElse));
            allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.RMDeleted));
            allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", SourceFlag.Physical));

            ///此处明确知道要查询的数据在同一个partition中，所以要加上ScopeId的查询，提高查询效率
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", Guid.Empty));
            queryExpress = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<Record, bool>>(queryExpress, param);
        }


        private async Task<(List<int>, bool)> GetPermissonConditionAsync(RMPhysicalExplorerNode currentRecord)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.PhysicalAdmin);
            var permissionConditions = new List<Expression>();
            bool hasScopePermission = true;
            if (isAdmin)
            {
                logger.Info($"GetPermissonCondition current loginUser :{TenantLocalValue.LogonUserId} isAdmin that skip GetPermissonCondition.");
                return (null, hasScopePermission);//管理员不做限制
            }
            else
            {
                logger.Debug($"GetPermissonCondition current loginUser :{TenantLocalValue.LogonUserId} is not admin:{TenantLocalValue.AccountType.ToString()} AccountId:{TenantLocalValue.LogonUserId} and need GetPermissonCondition.");
                var permissionIds = new List<int>();
                try
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var scopeId = currentRecord.Id;
                    var idFullPath = PermissionManagementService.GetScopeIdFullPath(scopeId);
                    hasScopePermission = PermissionManagementService.HasCurrentScopePermission(idFullPath, userAndGroupIds);
                    if (!hasScopePermission)
                    {
                        permissionIds = PermissionManagementService.GetIncludeScopePermissionIds(scopeId, userAndGroupIds);
                    }
                    else
                    {
                        permissionIds = PermissionManagementService.GetExcludeScopePermissionIds(scopeId, userAndGroupIds);
                    }
                    logger.Info($"GetPermissonCondition current loginUser :{TenantLocalValue.LogonUserId} permissionIds count:{permissionIds.Count}.");
                    return (permissionIds, hasScopePermission);
                }
                catch (Exception ex)
                {
                    hasScopePermission = false;
                    logger.Warn($"An error occured when GetPermissonCondition, message:{ex.ToString()}");
                    return (permissionIds, hasScopePermission);
                }
            }
        }

        public Expression<Func<Record, bool>> GetSubNodesLambda(ScopeInfoDto node)
        {
            Expression queryExpress = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "c");
            List<Expression> nodeTypeExpressionList = new List<Expression>();
            switch (node.NodeType)
            {
                case (int)RMNodeLevel.PhysicalBottomLocation:
                    var locationNode = LocationDao.GetLocationById(int.Parse(node.ScopeId));
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalBox));
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", locationNode.UniqueId));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", Guid.Empty));
                    break;
                case (int)RMNodeLevel.PhysicalBox:
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", new Guid(node.ScopeId)));
                    break;
                case (int)RMNodeLevel.PhysicalFile:
                    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalRecord));
                    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "FileId", new Guid(node.ScopeId)));
                    break;
                default:
                    break;
            }
            allExpressionList.Add(nodeTypeExpressionList.Aggregate(Expression.OrElse));
            allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.RMDeleted));
            allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", SourceFlag.Physical));

            ///此处明确知道要查询的数据在同一个partition中，所以要加上ScopeId的查询，提高查询效率
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", Guid.Empty));
            queryExpress = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<Record, bool>>(queryExpress, param);
        }

        public async Task<string> GetTermTreeViewDataAsync(TermTreeView tree)
        {
            var needGetTermGuidNodeType = new string[] { RecordsConstants.TYPE_STRING_BOXES, RecordsConstants.TYPE_STRING_FILES };
            var nodeType = tree.NodeType;
            Guid termGuid = Guid.Empty;
            try
            {
                var termIntId = Convert.ToInt32(tree.TermId);
                string strResult = string.Empty;
                if (needGetTermGuidNodeType.Contains(nodeType))
                {
                    var rmTerm = TermDao.GetRMTermByTermId(termIntId);
                    termGuid = rmTerm.UniqueId;
                }
                switch (nodeType)
                {
                    case RecordsConstants.TYPE_STRING_TERM:
                        strResult = JsonConvert.SerializeObject(new List<TermTreeViewDto>() {
                            new TermTreeViewDto(){ Id = Guid.NewGuid().ToString(), Name = I18N.Core.I18NEntity.GetString("RM_PRM_PRE_VirtualNodeType_Boxes"), Type = RecordsConstants.TYPE_STRING_BOXES, TermId = termIntId.ToString()},
                            new TermTreeViewDto(){ Id = Guid.NewGuid().ToString(), Name = I18N.Core.I18NEntity.GetString("RM_PRM_PRE_VirtualNodeType_Files"), Type = RecordsConstants.TYPE_STRING_FILES, TermId = termIntId.ToString()},
                            new TermTreeViewDto(){ Id = Guid.NewGuid().ToString(), Name = I18N.Core.I18NEntity.GetString("RM_PRM_PRE_VirtualNodeType_SubTerms"), Type = RecordsConstants.TYPE_STRING_SUB_TERM, TermId = termIntId.ToString()},
                        });
                        break;
                    case RecordsConstants.TYPE_STRING_SUB_TERM:
                        strResult = JsonConvert.SerializeObject(TermDao.GetTermFromParentTerm(termIntId, tree.PageIndex.Value, tree.PageSize));
                        break;
                    case RecordsConstants.TYPE_STRING_BOXES:
                        strResult = JsonConvert.SerializeObject(ConvertToDto(await GetNodesByTermFromCosmosDBAsync(RMNodeType.PhyBox, termGuid, tree.PageSize, tree.PagePosition), termGuid));
                        break;
                    case RecordsConstants.TYPE_STRING_FILES:
                        strResult = JsonConvert.SerializeObject(ConvertToDto(await GetNodesByTermFromCosmosDBAsync(RMNodeType.PhyFile, termGuid, tree.PageSize, tree.PagePosition), termGuid));
                        break;
                }
                return strResult;
            }
            catch (Exception e)
            {
                logger.Error($"GetTaxonomyTreeData Exception: {e}");
                return string.Empty;
            }
        }


        private async Task<Tuple<IEnumerable<Record>, string>> GetNodesByTermFromCosmosDBAsync(RMNodeType nodeType, Guid termId, int pagerSize, string pagePosition)
        {
            List<int> permissionIds = new List<int>();
            permissionIds = await ExplorerService.GetPermissionConditionAsync();
            bool isEnduser = await ExplorerService.IsPhysicalEndUserAsync();
            if (isEnduser)
            {
                permissionIds = await ExplorerService.GetPermissionConditionAsync();
            }
            bool hasScopePermission = false;
            var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
            var isManagerHoldBBuidIn = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);
            if (!userPermission.IsAdmin && !isEnduser && !isManagerHoldBBuidIn)
            {
                var phyPermissionIds = userPermission.ScopePermissionInfo.FirstOrDefault(_ => _.DataSourceType == SourceFlag.Physical)?.ScopeIds ?? new List<Guid>();
                var bottomLocationIds = LocationDao.LoadAllLocationBottomIdUnderTopLocation(phyPermissionIds);
                return ExplorerDao.QueryPageBySqlForTermBrowse(nodeType, termId, permissionIds, hasScopePermission, bottomLocationIds, pagerSize, pagePosition);
            }
            return ExplorerDao.QueryPageBySqlForTermBrowse(nodeType, termId, permissionIds, hasScopePermission, pagerSize, pagePosition);
        }

        public RMPhysicalExplorerTermViewNode ConvertToDto(Tuple<IEnumerable<Record>, string> result, Guid termId)
        {
            bool hasNext = false;
            string pagePosition = string.Empty;
            var subNodes = result.Item1.Select(r => ConvertToRMPhysicalExplorerTermViewNodeAsync(r, termId).Result).ToList();
            var scopeBreakInherDic = PermissionManagementService.GetScopeBreakInherMapping(subNodes.Select(o => o.Id).ToList());
            subNodes.ForEach(n =>
            {
                //n.LeafNodeType = currentRecord.LeafNodeType;
                //n.HasChildren = ExplorerDao.Exist(GetBrowserLambda(n));
                n.BreakInheritance = scopeBreakInherDic[n.Id];
            });

            hasNext = !string.IsNullOrEmpty(result.Item2);
            pagePosition = result.Item2;

            RMPhysicalExplorerTermViewNode node = new RMPhysicalExplorerTermViewNode();
            node.Children = subNodes;
            node.HasNextPage = hasNext;
            node.PagePosition = pagePosition;
            return node;
        }

        private async Task<RMPhysicalExplorerTermViewNode> ConvertToRMPhysicalExplorerTermViewNodeAsync(Record record, Guid termId)
        {
            var node = new RMPhysicalExplorerTermViewNode();
            node.Id = record.Id.ToString();
            node.Name = record.LeafName;
            node.NodeType = record.NodeType;
            node.Type = ((RMNodeType)record.NodeType).ToString();
            node.HasChildren = false;
            node.LocationId = record.LocationId.ToString();
            node.LocationName = LocationDao.GetLocationByUniqueId(record.LocationId)?.Name;
            node.BoxId = record.BoxId.ToString();
            node.FileId = record.FileId.ToString();
            node.RecordStatus = record.RecordStatus;
            node.IsHoldStatus = record.HoldStatus;
            node.TermId = termId;
            node.OnLoan = GetLoanStatus(record);
            node.Ancestors = record.GetPhysicalAncestorsIndludeSelf();
            node.TemplateIdPath = await GetTemplateIdPathAsync(record);
            return node;
        }

        /// <summary>
        /// get the templateIdPath
        /// to be modified...需要优化
        /// </summary>
        /// <param name="currentRecord"></param>
        /// <returns></returns>
        private Task<string> GetTemplateIdPathAsync(Record record)
        {
            return TemplateManagementService.GetTemplateIdPathAsync(new PhysicalObjectDto { 
                LocationId = record.LocationId,
                BoxId = record.BoxId,
                FileId = record.FileId,
                Ancestors = record.Ancestors,
                TemplateId = record.TemplateId
            });
        }

        public int GetTreeViewMode()
        {
            try
            {
                var key = $"{TenantLocalValue.LogonUserId}{RMNameValueDto.Seprator}{RMNameValueType.TreeViewMode}";
                var value = KeyValueService.Get(key)?.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    if (int.TryParse(value, out int mode))
                    {
                        return mode;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                logger.Error($"GetTreeViewMode Error: {e}");
                return 0;
            }
        }

        public async System.Threading.Tasks.Task SetTreeViewModeAsync(int mode)
        {
            try
            {
                var dto = new RMNameValueDto
                {
                    Name = TenantLocalValue.LogonUserId,
                    Type = RMNameValueType.TreeViewMode,
                    Value = mode.ToString()
                };
                await KeyValueService.SaveAsync(dto);
            }
            catch (Exception e)
            {
                logger.Error($"SetTreeViewMode Error: {e}");
            }
        }
    }
}
