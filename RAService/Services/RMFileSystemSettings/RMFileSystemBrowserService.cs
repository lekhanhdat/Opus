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
using AngleSharp.Common;
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AvePointService;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC;
using Microsoft.SharePoint.Client.RecordsRepository;
using Newtonsoft.Json;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    [Audit]
    public class RMFileSystemBrowserService : RMServiceBase, IRMFileSystemBrowserService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMFileSystemBrowserService));
        private static readonly TimeSpan MultiGeoAgentRedirectWaitTime = TimeSpan.FromSeconds(15);
        private IFSConnectionGroupDao FSGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMembershipDao => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();

        private IFileSystemTreeCacheDao FileSystemTreeCacheDao => PlatformWindsorManager.GetService<IFileSystemTreeCacheDao>();

        private IHybridBrowserService hybridBrowserService => PlatformWindsorManager.GetService<IHybridBrowserService>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        public IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
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

        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public List<RMFSTreeNode> LoadFSRoot()
        {
            var parentNode = new RMFSTreeNode();
            parentNode.Name = I18N.Core.I18NEntity.GetString("RM_JS_SPS_FS_RootNode");
            parentNode.Level = (int)NodeLevel.Farm;//RMNodeLevel.FSRoot
            parentNode.Id = RecordsConstants.FS_ROOT_GUID;
            return new List<RMFSTreeNode>() { parentNode };
        }

        public async Task<List<RMFSTreeNode>> FSBrowseAsync(RMFSTreeNode parent)
        {
            return parent.Level switch
            {
                (int)NodeLevel.Farm => await BrowseRootLevelAsync(parent),
                (int)NodeLevel.WebApplication => BrowseGroupLevel(parent),
                (int)NodeLevel.SiteCollection or (int)RMNodeLevel.FSFolder => await BrowseFolderLevelAsync(parent),
                _ => new List<RMFSTreeNode>()
            };
        }
        public async Task<List<Guid>> ValidateUNCPathsAsync(Dictionary<Guid, string> UNCPaths, AccessConnectionType AccessConnectionType, List<Guid> AgentIds)
        {
            try
            {
                var batchId = Guid.NewGuid();
                var enabledJPMCFileSystemFeature = IsEnabledJPMCFileSystemFeature();
                var args = new FileSystemUNCPathValidateArgs
                {
                    BatchId = batchId,
                    TenantId = TenantLocalValue.LogonGroupId,
                    UNCPaths = UNCPaths,
                    isEnabledJPMC= enabledJPMCFileSystemFeature
                };

                var agentResult = await hybridBrowserService.ValidateFileSystemUNCPathsAsync(args, AccessConnectionType, AgentIds);
                if (agentResult.Result == ValidateResultEnum.Failed)
                {
                    logger.Warn($"Validation test connection failed: {agentResult.Message}");
                }

                await TryUpdateValidateResultAsync(agentResult, UNCPaths?.Keys);

                var result = GetReturnInfoFromDB(batchId);
                return JsonConvert.DeserializeObject<List<Guid>>(result.TreeData);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while validate connections. Error: {e}");
                return new List<Guid>();
            }
        }
        public async Task<List<Guid>> ValidateTestConnectionsAsync(ValidateConnectionParam param)
        {
            try
            {
                var targetDCs = await ResolveValidationTargetDCsAsync(param);
                if (targetDCs.Count > 0)
                {
                    return await ValidateConnectionsInRealDCAsync(param, targetDCs);
                }

                if (!param.IsPublicApiRole && !await IsFSAdminAsync())
                {
                    logger.Info($"Current user doesn't FSAdmin when ValidationTestConnection. User:{TenantLocalValue.LogonUserEmail}.");
                    return new List<Guid>();
                }

                logger.Info($"Need validate connection ids: [{string.Join(", ", param.ConnectionIds)}]. Can use agent ids: [{string.Join(", ", param.AgentIds)}].");
                var batchId = Guid.NewGuid();
                var enabledJPMCFileSystemFeature = IsEnabledJPMCFileSystemFeature();
                var connections = FSConnectionDao.GetConnectionByIds(param.ConnectionIds);
                var uncPaths = connections.ToDictionary(item => item.Id, item => item.UNCPath);
                var args = new FileSystemUNCPathValidateArgs
                {
                    BatchId = batchId,
                    TenantId = TenantLocalValue.LogonGroupId,
                    UNCPaths = uncPaths,
                    isEnabledJPMC= enabledJPMCFileSystemFeature
                };

                var agentResult = await hybridBrowserService.ValidateFileSystemUNCPathsAsync(args, param.AccessConnectionType, param.AgentIds);
                if (agentResult.Result == ValidateResultEnum.Failed)
                {
                    logger.Warn($"Validation test connection failed: {agentResult.Message}");
                }

                await TryUpdateValidateResultAsync(agentResult, param.ConnectionIds);

                var result = GetReturnInfoFromDB(batchId);
                return JsonConvert.DeserializeObject<List<Guid>>(result.TreeData);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while validate connections. Error: {e}");
                return new List<Guid>();
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSConnectionValidationTest, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        public async Task<bool> ValidationTestConnectionAsync(ConnectionDto connectionDto)
        {
            try
            {
                var batchId = Guid.NewGuid();
                TreeBrowserArgs args = new TreeBrowserArgs()
                {
                    Type = (int)TreeBrowserType.Validation,
                    BatchId = batchId.ToString(),
                    TenantId = TenantLocalValue.LogonGroupId,
                    RootDir = connectionDto.UNCPath
                };
                
                //var account = AccountDao.GetActiveUserByName(TenantLocalValue.LogonUserEmail);
                if (!(await IsFSAdminAsync()))
                {
                    logger.Info($"Current user doesn't FSAdmin when ValidationTestConnection. User:{TenantLocalValue.LogonUserEmail}.");
                    return false;
                }
                logger.Info("Start hybridBrowserService BrowseTreeNode.");
                var agentResult = await hybridBrowserService.BrowseTreeNodeByGroupIdAsync(args, connectionDto.GroupId);

                if (agentResult != null && agentResult.Result == BrowserResultEnum.Succeed)
                {
                    //get browse return message from DB
                    FileSystemTreeCache info = GetReturnInfoFromDB(batchId);
                    if (info != null && !string.IsNullOrEmpty(info.TreeData))
                    {
                        var nodes = JsonConvert.DeserializeObject<List<HBTreeNode>>(info.TreeData);
                        if (nodes != null && nodes.Count > 0)
                        {
                            return !string.IsNullOrEmpty(nodes.FirstOrDefault()?.Url);
                        }
                    }
                }
                else
                {
                    ArgumentCheck.NotNull(agentResult, nameof(agentResult));
                    logger.Warn($"ValidationTestConnection error: {agentResult.Message}");
                }
            }
            catch (Exception e)
            {
                logger.Warn($"ValidationTestConnection Error:{e}");
            }
            return false;
        }



        public async Task<bool> CheckHasAvailableAgentAsync()
        {
            return await hybridBrowserService.CheckHasAvailableAgentAsync(Hybrid.Contract.Object.SourceType.FileSystem);
        }

        public async Task<bool> CheckHasAvailableAgentAsync(Guid groupId)
        {
            return await hybridBrowserService.CheckHasAvailableAgentAsync(Hybrid.Contract.Object.SourceType.FileSystem, groupId);
        }

        public Task<bool> CheckHasAvailableAgentAsync(List<Guid> agentIds)
        {
            return hybridBrowserService.CheckHasAvailableAgentAsync(Hybrid.Contract.Object.SourceType.FileSystem, agentIds);
        }

        public bool ValidAllConnectionExist(List<Guid> connectionIds)
        {
            try
            {
                return FSConnectionDao.CheckAllConnectionIdsExist(connectionIds);
            }
            catch(Exception e)
            {
                logger.Error($"Valid All connection exist have error:{e}");
                return false;
            }
        }

        public bool ValidFSConnectionNotHaveOutsideGroup(List<Guid> connectionIds, Guid id, bool isCreate)
        {
            try
            {
                return !FSConnectionDao.AnyConnectionExistsOutsideGroup(connectionIds, id, isCreate);
            }
            catch (Exception e)
            {
                logger.Error($"Valid All connection have group have error:{e}");
                return false;
            }
        }

        #region private method
        private async Task<bool> IsFSAdminAsync()
        {
            var isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
            if (isFSAdmin)
            {
                return true;
            }

            return await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryFileSystemPermissionMask.AccessAll);
        }

        private async Task<List<RMFSTreeNode>> BuildSearchResultTreeForMultiGeoAsync(RMFSTreeNode parent, string searchKey, bool isMainDC)
        {
            try
            {
                var fSConnectionGroupIdsBelongCurrentDC = isMainDC ? FSGroupDao.LoadAllConnectionGroupIdOfMainDC() : FSGroupDao.LoadAllConnectionGroupIdByDCInternalName(RMSSOHelper.CurrentDCName);
                var fSConnectionIdsBelongCurrentDC = await FSConnectionDao.GetAllConnectionIdsByGroupIdsAsync(fSConnectionGroupIdsBelongCurrentDC);
                var taskGroups = FSGroupDao.FsConnectionGroupWithSearchKeyAndId(searchKey, fSConnectionGroupIdsBelongCurrentDC);
                var taskConnections = FSConnectionDao.GetConnectionBySearchKeyAndGroupId(searchKey, fSConnectionGroupIdsBelongCurrentDC);
                var taskFolders = Task.Run(() => ExplorerDao.SearchFileSystemBySearchKeyAndConnectionIds(parent.SearchKey, fSConnectionIdsBelongCurrentDC, string.Empty, 1000));

                await Task.WhenAll(taskGroups, taskConnections, taskFolders);

                var matchedGroups = await taskGroups ?? new List<FSConnectionGroup>();
                var matchedConnections = await taskConnections ?? new List<FSConnection>();
                var foldersResult = await taskFolders;
                var matchedFolders = foldersResult.Item1.ToList()
                    .ConvertAll(ConvertUtil.ConvertRMBaseRecordToFSDto);

                var (folderExistOnLocal, allGroupIds, matchedGroupLookup, connectionsByGroupId, foldersByConnId) = await PrepareSearchFSDataStructuresAsync(matchedGroups, matchedConnections, matchedFolders);

                List<RMFSTreeNode> result = await BuildSearchResultNodesAsync(parent, folderExistOnLocal, allGroupIds, matchedGroupLookup, connectionsByGroupId, foldersByConnId);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while building search result tree. SearchKey: {searchKey}. Error: {ex}");
                return new List<RMFSTreeNode>();
            }
        }

        private async Task<(List<Guid> folderExistOnLocal, HashSet<Guid> allGroupIds, Dictionary<Guid, FSConnectionGroup> matchedGroupLookup, Dictionary<Guid, List<FSConnection>> connectionsByGroupId, Dictionary<string, List<FileSystemRecordDto>> foldersByConnId)> PrepareSearchFSDataStructuresAsync(List<FSConnectionGroup> matchedGroups, List<FSConnection> matchedConnections, List<FileSystemRecordDto> matchedFolders)
        {
            // Fetch all ancestor folders to build complete hierarchy
            var allFolders = await FetchAllAncestorFoldersAsync(matchedFolders);
            var folderExistOnLocal = new List<Guid>();

            // Edge handle: Need do later

            var connIdsFromFolders = allFolders
                .Select(f => new Guid(f.AveSiteId))
                .Where(id => id != Guid.Empty)
                .ToHashSet();
            var existingConnIds = matchedConnections.Select(c => c.Id).ToHashSet();

            var missingConnIds = connIdsFromFolders.Except(existingConnIds).ToList();
            if (missingConnIds.Any())
            {
                var additionalConnections = FSConnectionDao.GetConnectionByIds(missingConnIds) ?? new List<FSConnection>();
                matchedConnections.AddRange(additionalConnections);
            }

            var groupIdsFromConns = matchedConnections.Select(c => c.GroupId).Distinct().ToHashSet();
            var allGroupIds = matchedGroups.Select(g => g.Id).Union(groupIdsFromConns).ToHashSet();
            var matchedGroupLookup = matchedGroups.ToDictionary(g => g.Id);

            var missingGroupIds = allGroupIds.Except(matchedGroupLookup.Keys).ToList();
            if (missingGroupIds.Any())
            {
                var additionalGroups = FSGroupDao.GetGroupByIds(missingGroupIds) ?? new List<FSConnectionGroup>();
                foreach (var group in additionalGroups)
                {
                    matchedGroupLookup[group.Id] = group;
                }
            }

            var connectionsByGroupId = matchedConnections
                .GroupBy(c => c.GroupId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var foldersByConnId = allFolders
                .GroupBy(f => f.AveSiteId)
                .ToDictionary(g => g.Key, g => g.ToList());
            return (folderExistOnLocal, allGroupIds, matchedGroupLookup, connectionsByGroupId, foldersByConnId);
        }

        private async Task<List<RMFSTreeNode>> BuildSearchResultTreeAsync(RMFSTreeNode parent, string searchKey)
        {
            try
            {
                var taskGroups = FSGroupDao.FsConnectionGroupWithSearchKey(searchKey);
                var taskConnections = FSConnectionDao.GetConnectionBySearchKey(searchKey);
                var taskFolders = Task.Run(() => ExplorerDao.SearchFileSystemBySearchKey(parent.SearchKey, string.Empty, 1000));

                await Task.WhenAll(taskGroups, taskConnections, taskFolders);

                var matchedGroups = await taskGroups ?? new List<FSConnectionGroup>();
                var matchedConnections = await taskConnections ?? new List<FSConnection>();
                var foldersResult = await taskFolders;
                var matchedFolders = foldersResult.Item1.ToList()
                    .ConvertAll(r => ConvertUtil.ConvertRMBaseRecordToFSDto(r));

                var (folderExistOnLocal, allGroupIds, matchedGroupLookup, connectionsByGroupId, foldersByConnId) = await PrepareSearchFSDataStructuresAsync(matchedGroups, matchedConnections, matchedFolders);

                List<RMFSTreeNode> result = await BuildSearchResultNodesAsync(parent, folderExistOnLocal, allGroupIds, matchedGroupLookup, connectionsByGroupId, foldersByConnId);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while building search result tree. SearchKey: {searchKey}. Error: {ex}");
                return new List<RMFSTreeNode>();
            }
        }

        private async Task<List<RMFSTreeNode>> BuildSearchResultNodesAsync(RMFSTreeNode parent, List<Guid> folderExistOnLocal, HashSet<Guid> allGroupIds, Dictionary<Guid, FSConnectionGroup> matchedGroupLookup, Dictionary<Guid, List<FSConnection>> connectionsByGroupId, Dictionary<string, List<FileSystemRecordDto>> foldersByConnId)
        {
            var result = new List<RMFSTreeNode>();

            foreach (var groupId in allGroupIds)
            {
                if (!matchedGroupLookup.TryGetValue(groupId, out var group)) continue;

                var groupNode = new RMFSTreeNode
                {
                    Id = group.Id,
                    Name = group.Name,
                    Level = (int)NodeLevel.WebApplication,
                    ConnGroupId = group.Id,
                    FullPath = group.Name,
                    Parent = parent,
                    ParentId = parent?.Id.ToString()
                };

                if (connectionsByGroupId.TryGetValue(groupId, out var connections))
                {
                    groupNode.Children = new List<RMFSTreeNode>();
                    groupNode.Expanded = true;
                    foreach (var conn in connections)
                    {
                        var csSetting = FileSystemSettingDao.LoadFSSetting(conn.Id, groupNode.ConnGroupId);
                        var connNode = new RMFSTreeNode
                        {
                            Id = conn.Id,
                            Name = conn.Name,
                            Level = (int)NodeLevel.SiteCollection,
                            AgentId = conn.AgentId,
                            FullPath = conn.UNCPath,
                            ConnGroupId = groupNode.ConnGroupId,
                            Parent = groupNode,
                            ParentId = groupNode.Id.ToString()
                        };
                        // Build hierarchical folder tree under connection
                        if (foldersByConnId.TryGetValue(conn.Id.ToString(), out var folders))
                        {
                            connNode.Expanded = true;
                            connNode.Children = BuildFolderHierarchy(folders, connNode, groupNode.ConnGroupId, folderExistOnLocal);
                        }
                        await FileSystemSettingsService.LoadFSNodeSettingAsync(connNode);
                        groupNode.Children.Add(connNode);
                    }
                    FileSystemSettingsService.LoadFSSettingIcon(groupNode.Children);
                }
                result.Add(groupNode);
            }

            return result;
        }

        /// <summary>
        /// Fetches all ancestor folders for the given matched folders to build complete hierarchy.
        /// </summary>
        private async Task<List<FileSystemRecordDto>> FetchAllAncestorFoldersAsync(List<FileSystemRecordDto> matchedFolders)
        {
            if (matchedFolders == null || !matchedFolders.Any())
            {
                return new List<FileSystemRecordDto>();
            }

            var allFoldersById = matchedFolders
                .GroupBy(f => f.NodeId)
                .ToDictionary(g => g.Key, g => g.First());
            var parentIdsToFetch = new HashSet<Guid>();

            // Collect all parent IDs that are not yet in our collection
            foreach (var folder in matchedFolders)
            {
                CollectMissingParentIds(folder, allFoldersById, parentIdsToFetch);
            }

            // Batch fetch missing ancestors until no more parents are needed
            while (parentIdsToFetch.Any())
            {
                var ancestorFolders = await FetchFoldersByIdsAsync(parentIdsToFetch.ToList());

                if (ancestorFolders == null || !ancestorFolders.Any())
                {
                    break;
                }

                var newParentIds = new HashSet<Guid>();

                foreach (var ancestor in ancestorFolders)
                {
                    if (!allFoldersById.ContainsKey(ancestor.NodeId))
                    {
                        allFoldersById[ancestor.NodeId] = ancestor;
                        CollectMissingParentIds(ancestor, allFoldersById, newParentIds);
                    }
                }

                parentIdsToFetch = newParentIds;
            }

            return allFoldersById.Values.ToList();
        }

        /// <summary>
        /// Collects parent IDs that are not yet present in the folder dictionary.
        /// </summary>
        private void CollectMissingParentIds(FileSystemRecordDto folder, Dictionary<Guid, FileSystemRecordDto> existingFolders, HashSet<Guid> missingParentIds)
        {
            if (folder == null || string.IsNullOrEmpty(folder.ParentId.ToString()))
            {
                return;
            }

            var parentId = folder.ParentId;

            // Skip if parent is the connection (root level) or already exists
            if (parentId == Guid.Empty || existingFolders.ContainsKey(parentId))
            {
                return;
            }

            // Check if parent ID matches the connection ID (AveSiteId)
            var connectionId = new Guid(folder.AveSiteId);
            if (parentId == connectionId)
            {
                return;
            }

            missingParentIds.Add(parentId);
        }

        /// <summary>
        /// Fetches folders by their IDs from the database.
        /// </summary>
        private async Task<List<FileSystemRecordDto>> FetchFoldersByIdsAsync(List<Guid> folderIds)
        {
            if (folderIds == null || !folderIds.Any())
            {
                return new List<FileSystemRecordDto>();
            }

            try
            {
                var result = ExplorerDao.GetRecordByIds(folderIds);
                return result?.ToList()
                    .ConvertAll(r => ConvertUtil.ConvertRMBaseRecordToFSDto(r)) ?? new List<FileSystemRecordDto>();
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to fetch folders by IDs. Count: {folderIds.Count}. Error: {ex.Message}");
                return new List<FileSystemRecordDto>();
            }
        }

        /// <summary>
        /// Builds a hierarchical folder tree structure from a flat list of folders.
        /// </summary>
        private List<RMFSTreeNode> BuildFolderHierarchy(List<FileSystemRecordDto> folders, RMFSTreeNode connectionNode, Guid connGroupId, List<Guid> folderExistOnLocal)
        {
            if (folders == null || !folders.Any())
            {
                return new List<RMFSTreeNode>();
            }

            var folderNodesById = new Dictionary<string, RMFSTreeNode>();
            var connectionIdStr = connectionNode.Id.ToString();

            // Create all folder nodes first
            foreach (var folder in folders)
            {
                var folderNode = new RMFSTreeNode
                {
                    Id = folder.NodeId,
                    Name = folder.LeafName,
                    Level = (int)NodeLevel.FSFolder,
                    FullPath = folder.DirPath + "\\" + folder.LeafName,
                    ConnGroupId = connGroupId,
                    Children = new List<RMFSTreeNode>()
                };
                folderNodesById[folder.NodeId.ToString()] = folderNode;
            }

            // Build parent-child relationships
            var rootFolders = new List<RMFSTreeNode>();
            foreach (var folder in folders)
            {
                var folderNode = folderNodesById[folder.NodeId.ToString()];
                var parentId = folder.ParentId.ToString();
                
                // Check if parent is the connection (root level folder)
                
                FileSystemSettingsService.LoadFSNodeSettingAsync(folderNode);
                if (IsConnectionParent(folder, connectionNode))
                {
                    folderNode.Parent = connectionNode;
                    folderNode.ParentId = connectionIdStr;
                    rootFolders.Add(folderNode);
                }
                else if (folderNodesById.TryGetValue(parentId, out var parentNode))
                {
                    folderNode.Parent = parentNode;
                    folderNode.ParentId = parentId;
                    parentNode.Children.Add(folderNode);
                }
            }
            foreach (var node in folderNodesById.Values)
            {
                node.Expanded = node.Children != null && node.Children.Any();
            }
            FileSystemSettingsService.LoadFSSettingIcon(rootFolders);
            foreach (var node in folderNodesById.Values.Where(n => n.Children.Any()))
            {
                FileSystemSettingsService.LoadFSSettingIcon(node.Children);
            }
            //ApplyDeletedFromLocalFlags(folders, folderNodesById, folderExistOnLocal);
            return rootFolders;
        }

        /// <summary>
        /// Determines if the folder's parent is the connection itself.
        /// </summary>
        private bool IsConnectionParent(FileSystemRecordDto folder, RMFSTreeNode connectionNode)
        {
            if (folder == null || connectionNode == null)
            {
                return false;
            }

            // Check if folder path is directly under connection path
            if (!string.IsNullOrEmpty(folder.DirPath) && !string.IsNullOrEmpty(connectionNode.FullPath))
            {
                var connectionPath = connectionNode.FullPath.TrimEnd('\\');
                var folderDirPath = folder.DirPath.TrimEnd('\\');

                // If the folder's directory path equals the connection path, it's a root folder
                if (string.Equals(folderDirPath, connectionPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyDeletedFromLocalFlags(
         List<FileSystemRecordDto> folders,
         Dictionary<string, RMFSTreeNode> folderNodesById,
         List<Guid> folderExistOnLocal)
        {
            foreach (var folder in folders)
            {
                if (!folderExistOnLocal.Contains(folder.NodeId))
                {
                    if (folderNodesById.TryGetValue(folder.NodeId.ToString(), out var folderNode))
                    {
                        folderNode.IsDeletedFromLocal = true;
                        folderNode.IsCustomSetting = true;
                        folderNode.IsActive = false;
                    }
                }
            }
        }

        private async Task<List<RMFSTreeNode>> FSFolderBrowserAsync(RMFSTreeNode parent)
        {
            logger.Info($"start browser folder:{parent.Id}");
            var children = new List<RMFSTreeNode>();
            FileSystemTreeCache info = await AgentBrowserFSTreeAsync(parent);
            if (info != null && !string.IsNullOrEmpty(info.TreeData))
            {
                var nodes = JsonConvert.DeserializeObject<List<HBTreeNode>>(info.TreeData);
                foreach (var node in nodes)
                {
                    if (string.IsNullOrEmpty(node.Id))
                    {
                        continue;
                    }
                    if (node.Name == ".DFSFolderLink"|| node.Name == "DfsrPrivate")
                        continue;
                    //.DFSFolderLink and DfsrPrivate are virtual hidden folder 
                    // DFS referral resolution is unstable and may fallback to local I/O, causing PathType to be incorrectly identified as UNC (1) instead of DFS (2).
                    var child = new RMFSTreeNode();
                    child.Id = node.Url.ToLowerInvariant().ToMd5();
                    child.Name = node.Name;
                    child.Level = (int)NodeLevel.FSFolder;
                    child.FullPath = node.Url;
                    child.ConnGroupId = parent.ConnGroupId;

                    child.AgentId = parent.AgentId;
                    child.Parent = parent;
                    child.ParentId = parent.Id.ToString();
                    
                    children.Add(child);
                }
            }
            return children;
        }

        private async Task<FileSystemTreeCache> AgentBrowserFSTreeAsync(RMFSTreeNode parent)
        {
            var batchId = Guid.NewGuid();
            TreeBrowserArgs args = new TreeBrowserArgs()
            {
                Type = (int)TreeBrowserType.Browser,
                BatchId = batchId.ToString(),
                TenantId = TenantLocalValue.LogonGroupId,
                RootDir = parent.FullPath
            };
            logger.Info("Start hybridBrowserService BrowseTreeNode.");
            BrowserResult agentResult;
            try
            {
                //agentResult = hybridBrowserService.BrowseTreeNode(args);
                agentResult = await hybridBrowserService.BrowseTreeNodeByGroupIdAsync(args, parent.ConnGroupId);
            }
            catch (NotAvailableAgentException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AgentProcessException(e.Message, e);
            }
            if (agentResult != null)
            {
                if (agentResult.Result == BrowserResultEnum.Succeed)
                {
                    //get browse return message from DB
                    return GetReturnInfoFromDB(batchId);
                }
                else
                {
                    throw new AgentProcessException(agentResult.Message);
                }
            }
            else
            {
                throw new AgentProcessException("Agent Browser Timeout");
            }
        }

        private FileSystemTreeCache GetReturnInfoFromDB(Guid batchId)
        {
            logger.Info($"Start [GetReturnInfoFromDB] batchId: {batchId}");
            FileSystemTreeCache info = FileSystemTreeCacheDao.GetTreeNodeInfoByBatchId(batchId);
            if (info != null)
            {
                logger.Info($"successfully get tree info from db");
                FileSystemTreeCacheDao.Delete(info);
            }
            else
            {
                logger.Warn($"agent return success, but can not get return data from db.");
                throw new AgentNotifyWebApiException();
            }
            return info;
        }

        private async Task TryUpdateValidateResultAsync(ValidateResult agentResult, IEnumerable<Guid> connectionIds)
        {
            if (agentResult == null || connectionIds == null)
            {
                return;
            }

            var ids = connectionIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return;
            }

            try
            {
                var pathTypes = agentResult.PathType?.ToDictionary(item => item.Key, item => (int)item.Value);
                await FSConnectionDao.UpdateValidateResultAsync(ids, agentResult.UNCPaths, pathTypes);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to persist UNC path validation result. Error: {ex}");
            }
        }
        private bool IsEnabledJPMCFileSystemFeature()
        {
            return  RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
        }

        private async Task<List<string>> ResolveValidationTargetDCsAsync(ValidateConnectionParam param)
        {
            if (param == null || !MultiGeoDataCenterService.IsMainDC() || !await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return new List<string>();
            }

            if (param.ConnectionIds == null || param.ConnectionIds.Count == 0)
            {
                return new List<string>();
            }

            if (param.TargetDCs?.Count > 0)
            {
                return param.TargetDCs
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return new List<string>();
        }

        private async Task<List<Guid>> ValidateConnectionsInTargetDCAsync(ValidateConnectionParam param, List<string> targetDCs)
        {
            var result = await RAMultiGeoClient.RouteApiActionAsync<ValidateConnectionParam, List<Guid>>(
                MultiGeoOperationType.ValidateConnections,
                param,
                targetDCs);

            var invalidConnectionIds = new HashSet<Guid>();

            foreach (var item in result)
            {
                if (item.Value != null)
                {
                    foreach (var connectionId in item.Value)
                    {
                        invalidConnectionIds.Add(connectionId);
                    }
                }
            }

            return invalidConnectionIds.ToList();
        }
        private async Task<List<Guid>> ValidateConnectionsInRealDCAsync(ValidateConnectionParam param, List<string> targetDCs)
        {
            var (mainDCAgentIds, otherDCAgentIds) = await ResolveValidationAgentsAsync(param);
            var validConnectionIds = new List<Guid>();

            if (otherDCAgentIds.Count > 0)
            {
                var otherDCResult = await ValidateConnectionsInTargetDCAsync(
                    new ValidateConnectionParam
                    {
                        ConnectionIds = param.ConnectionIds,
                        AgentIds = otherDCAgentIds,
                        AccessConnectionType = param.AccessConnectionType,
                        IsPublicApiRole = param.IsPublicApiRole,
                        TargetDCs = targetDCs
                    },
                    targetDCs);

                validConnectionIds.AddRange(otherDCResult);
            }

            if (mainDCAgentIds.Count > 0)
            {
                var mainResult = await ValidateTestConnectionsAsync(
                    new ValidateConnectionParam
                    {
                        ConnectionIds = param.ConnectionIds,
                        AgentIds = mainDCAgentIds,
                        AccessConnectionType = param.AccessConnectionType,
                        IsPublicApiRole = param.IsPublicApiRole,
                        TargetDCs = new List<string>()
                    });

                validConnectionIds.AddRange(mainResult);
            }

            return validConnectionIds.Distinct().ToList();
        }
        #endregion

        #region Browser tree node
        private async Task<List<RMFSTreeNode>> BrowseRootLevelAsync(RMFSTreeNode parent)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                parent.Expanded = true;
                return await BrowseConnectionGroupForMultiGeoAsync(parent, parent.SearchKey);
            }
            if (!string.IsNullOrEmpty(parent.SearchKey))
            {
                var result = await BuildSearchResultTreeAsync(parent, parent.SearchKey);
                parent.Expanded = true;
                return result;
            }
            var allGroups = FSGroupDao.LoadAllGroups();
            return allGroups.Select(item => RMFileSystemTreeNodeFactory.Create(item, parent, NodeLevel.WebApplication)).ToList();
        }

        private async Task<List<RMFSTreeNode>> BrowseConnectionGroupForMultiGeoAsync(RMFSTreeNode parent, string searchKey)
        {
            List<RMFSTreeNode> result = new();
            bool isMainDC = MultiGeoDataCenterService.IsMainDC();
            if (!string.IsNullOrEmpty(searchKey)) // Suport searching with key.
            {
                result = await BuildSearchResultTreeForMultiGeoAsync(parent, searchKey, isMainDC);
                parent.Expanded = true;
            }
            else
            {
                var allG = isMainDC ? FSGroupDao.LoadAllGroupsOfMainDC() : FSGroupDao.LoadAllGroupsByDCInternalName(RMSSOHelper.CurrentDCName);
                foreach (var item in allG)
                {
                    var child = new RMFSTreeNode();
                    child.Id = item.Id;
                    child.Name = item.Name;
                    child.Level = (int)NodeLevel.WebApplication;//NodeLevel.FSGroup
                    child.ConnGroupId = item.Id;
                    child.FullPath = item.Name;
                    child.Parent = parent;
                    child.ParentId = parent.Id.ToString();
                    result.Add(child);
                }
            }
            return result;
        }

        private List<RMFSTreeNode> BrowseGroupLevel(RMFSTreeNode parent)
        {
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return BrowseGroupLevelPaged(parent);
            }
            var allConnections = FSConnectionDao.GetAllConnectionsByGroupId(parent.Id);
            return allConnections.Select(item => RMFileSystemTreeNodeFactory.Create(item, parent, NodeLevel.SiteCollection)).ToList();
        }

        private List<RMFSTreeNode> BrowseGroupLevelPaged(RMFSTreeNode parent)
        {
            var param = new GetConnectionListParam
            {
                PageIndex = parent.PageIndex + 1,
                PageSize = parent.PageSize == 0 ? int.MaxValue : parent.PageSize,
                Filters = new List<FSConnectionFilter>
                {
                    new FSConnectionFilter
                    {
                        ColumnName = nameof(FSConnection.GroupId),
                        ColumnValues = new List<string> { parent.ConnGroupId.ToString() }
                    }
                },
                Order = new FSConnectionOrder
                {
                    ColumnName = nameof(FSConnection.Name),
                    IsDesc = false
                }
            };

            var filter = new FSConnectionFilterBuilder(param.Filters).Build();
            var connections = FSConnectionDao.QueryConnectionsPager(filter, param, out var totalCount);
            var children = connections.Select(item => RMFileSystemTreeNodeFactory.Create(item, parent, NodeLevel.SiteCollection)).ToList();
            parent.ChildrenCount = totalCount;
            return children;
        }

        private async Task<List<RMFSTreeNode>> BrowseFolderLevelAsync(RMFSTreeNode parent)
        {
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return await BrowseFolderLevelPagedAsync(parent);
            }
            return await FSFolderBrowserAsync(parent);
        }

        private async Task<List<RMFSTreeNode>> BrowseFolderLevelPagedAsync(RMFSTreeNode parent)
        {
            logger.Info($"Start browsing folder. Path: {parent.FullPath}, Level: {parent.Level}");
            
            var children = new List<RMFSTreeNode>();
            FileSystemTreeCache info = await AgentBrowserFSTreeAsync(parent);
            if (info == null || string.IsNullOrEmpty(info.TreeData)) return children;

            var nodes = JsonConvert.DeserializeObject<List<HBTreeNode>>(info.TreeData);
            return nodes.Where(node => IsValidFolderNode(node, parent.PathType))
                        .Select(node => RMFileSystemTreeNodeFactory.Create(node, parent, NodeLevel.FSFolder))
                        .ToList();
        }

        private static bool IsValidFolderNode(HBTreeNode node, int pathType)
        {
            if (string.IsNullOrEmpty(node.Id)) return false;
            //.DFSFolderLink and DfsrPrivate are virtual hidden folder 
            // DFS referral resolution is unstable and may fallback to local I/O, causing PathType to be incorrectly identified as UNC (1) instead of DFS (2).
            if (node.Name == ".DFSFolderLink" || node.Name == "DfsrPrivate") return false;
            return true;
        }
        #endregion

        private async Task<(List<Guid> MainDCAgentIds, List<Guid> TargetDCAgentIds)> ResolveValidationAgentsAsync(ValidateConnectionParam param)
        {
            if (param?.AgentIds == null || param.AgentIds.Count == 0)
            {
                return (new List<Guid>(), new List<Guid>());
            }

            var agents = await AgentMgmtService.GetAgentsByIdsAsync(param.AgentIds);
            var mainDCName = RMKeyValueDao.GetValueByKey(KeyNameCollection.JPMCMultiGEOMainDC)?.Value ?? string.Empty;

            var mainDCAgentIds = agents
                .Where(a => string.IsNullOrEmpty(a.DCInternalName) ||
                            string.Equals(a.DCInternalName, mainDCName, StringComparison.OrdinalIgnoreCase))
                .Select(a => a.Id)
                .ToList();

            var targetDCAgentIds = agents.Select(a => a.Id).Except(mainDCAgentIds).ToList();

            return (mainDCAgentIds, targetDCAgentIds);
        }

    }
}
