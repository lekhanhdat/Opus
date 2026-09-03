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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Google;

public class RMRemoteGoogleNodeService : BaseContentRepositorySettingsService, IRMRemoteGoogleNodeService
{
    private RALogger Logger = RALogger.GetInstance(typeof(RMRemoteNodeService));

    private IRMGoogleRemoteNodeDao RemoteNodeDao =>
        PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();

    private IRMSecurityTrimmingHelper SecurityTrimmingHelper =>
        PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

    //private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();

    private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

    //private IRMGoogleSettingDao RmGoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

    private IUserService UserService =>
        PlatformWindsorManager.GetService<IUserService>();

    private IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao =>
        PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

    private IRMRuleDao RuleDao =>
        PlatformWindsorManager.GetService<IRMRuleDao>();

    public async Task<RMSampleGoogleTreeNode> GetContainersAsync(RMSampleGoogleTreeNode node, bool checkPermission)
    {
        if (!(checkPermission && !await IsGoogleAdminAsync() && !await IsOpusSOGoogleAdminAsync()))
            return await RemoteNodeDao.GetContainersWithoutCheckPermissionAsync(node);
        return await RemoteNodeDao.GetContainersWithCheckPermissionAsync(node);
    }

    public async Task<RMSampleGoogleTreeNode> GetContainersForRuleAsync(RMSampleGoogleTreeNode node, bool checkPermission, params NodeLevel[] nodeLevels)
    {
        if (!(checkPermission && !await IsGoogleAdminAsync() && !await IsOpusSOGoogleAdminAsync()))
            return await RemoteNodeDao.GetContainersWithoutCheckPermissionForRuleAsync(node, nodeLevels);
        return await RemoteNodeDao.GetContainersWithCheckPermissionForRuleAsync(node, nodeLevels);
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesAsync(RMSampleGoogleTreeNode node, bool checkPermission)
    {
        if (!(checkPermission && !await IsGoogleAdminAsync() && !await IsOpusSOGoogleAdminAsync()))
            return await RemoteNodeDao.GetDrivesWithoutCheckPermissionAsync(node);
        return await RemoteNodeDao.GetDrivesWithCheckPermissionAsync(node);
    }

    public async Task<RMSampleGoogleTreeNode> GetDrivesForRuleAsync(RMSampleGoogleTreeNode node, bool checkPermission)
    {
        if (!(checkPermission && !await IsGoogleAdminAsync() && !await IsOpusSOGoogleAdminAsync()))
            return await RemoteNodeDao.GetDrivesWithoutCheckPermissionForRuleAsync(node);
        return await RemoteNodeDao.GetDrivesWithCheckPermissionForRuleAsync(node);
    }

    public async Task<RMSampleGoogleTreeNode> GetContainersForSearchAsync(RMSampleGoogleTreeNode node,
        bool checkPermission)
    {
        var root = await RemoteNodeDao.GetContainersForSearchAsync(node,
            checkPermission && !await IsGoogleAdminAsync() && !await IsOpusSOGoogleAdminAsync());
        root.Expanded = true;
        if (root.Children.Count != 0)
        {
            List<RMSampleGoogleTreeNode> availableContainers = [];
            foreach (var container in root.Children)
            {
                container.SearchKey = root.SearchKey;
                container.PageIndex = 0;
                container.PageSize = 15;
                var googleDrives = await GetDrivesAsync(container, checkPermission);
                if (googleDrives.Children.Any())
                {
                    googleDrives.Expanded = true;
                    googleDrives.Children.ForEach(n =>
                    {
                        n.ParentId = googleDrives.Id;
                        n.Parent = googleDrives;
                    });
                    availableContainers.Add(googleDrives);
                }
            }

            root.Children = availableContainers;
        }

        return root;
    }

    public async Task<RMSampleGoogleTreeNode> GetRemoteNodeByDriveIdAsync(string id)
    {
        return await Task.FromResult(RemoteNodeDao.GetGoogleDriveById(id));
    }
    public async Task LoadGoogleSettingIconAsync(List<RMSampleGoogleTreeNode> nodes)
    {
        try
        {
            if (nodes.IsNotNullOrEmpty())
            {
                RMSampleGoogleTreeNode container = nodes[0];
                if (!IsGoogleContainer(container.Level))
                {
                    while (container != null && !IsGoogleContainer(container.Level))
                    {
                        container = container.Parent;
                    }

                    Guid containerId = Guid.Empty;
                    if (container != null)
                    {
                        containerId = new Guid(container.Id);
                    }

                    var containerSetting = RemoteNodeDao.LoadGoogleSetting(containerId, Guid.Empty);

                    var allSchedules =
                        await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.GoogleDisposalSchedule);
                    List<string> allSchedulesProfilesId = [];
                    if (allSchedules != null && allSchedules.Count != 0)
                    {
                        allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                    }

                    foreach (var node in nodes)
                    {
                        RMSampleGoogleTreeNode driveNode = node;
                        while (driveNode != null && !IsGoogleDrive(driveNode.Level))
                        {
                            driveNode = driveNode.Parent;
                        }

                        Guid driveId = Guid.Empty;
                        if (driveNode != null)
                        {
                            driveId = new Guid(driveNode.Id);
                        }

                        ArgumentCheck.NotNull(node, nameof(node));
                        var driveSetting = RemoteNodeDao.LoadGoogleSetting(containerId, driveId);
                        if (driveSetting != null)
                        {
                            node.IconStatus = IconStatus.Break;
                            continue;
                        }

                        var profileId = ScheduleService.GetProfileId(node);
                        if (allSchedulesProfilesId.Contains(profileId))
                        {
                            node.IconStatus = IconStatus.Break;
                            continue;
                        }

                        if (containerSetting != null)
                        {
                            node.IconStatus = IconStatus.Inhert;
                            continue;
                        }

                        node.IconStatus = IconStatus.NoSet;
                    }
                }
                else
                {
                    foreach (var selfContainerNode in nodes)
                    {
                        var selfContainerSetting = RemoteNodeDao.LoadGoogleSetting(new Guid(selfContainerNode.Id), Guid.Empty);
                        var profileId = ScheduleService.GetProfileId(selfContainerNode);
                        var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.GoogleDisposalSchedule);
                        selfContainerNode.IconStatus = selfContainerSetting == null && disposeSchedule == null ? IconStatus.NoSet : IconStatus.Break;
                        if (selfContainerNode.Children.IsNotNullOrEmpty())
                        {
                            await LoadGoogleSettingIconAsync(selfContainerNode.Children);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("An error occurred when load GoogleSetting Icon.Error:{0}", e.ToString());
            throw;
        }
    }

    private RMSampleGoogleTreeNode TryGetGoogleContainerNode(RMSampleGoogleTreeNode node)
    {
        while (node != null && !IsGoogleContainer(node.Level))
        {
            node = node.Parent;
        }
        return node;
    }

    private RMSampleGoogleTreeNode TryGetGoogleDriveNode(RMSampleGoogleTreeNode node)
    {
        while (node != null && !IsGoogleDrive(node.Level))
        {
            node = node.Parent;
        }
        return node;
    }

    private Task<bool> IsGoogleAdminAsync()
    {
        return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
    }

    private Task<bool> IsOpusSOGoogleAdminAsync()
    {
        return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
    }

    public List<RMSampleGoogleTreeNode> LoadGoogleDriveRoot()
    {
        List<RMSampleGoogleTreeNode> result = [];
        try
        {

            GoogleDriveTreeMessage msg = InitGoogleDriveRoot();
            if (msg != null && msg.NodeList != null)
            {
                foreach (var tree in msg.NodeList)
                {
                    result.Add(RMDtoConverter.ConvertTreeNodeDto2RMGoogleDriveTree(tree));
                }

            }
            else
            {
                Logger.Warn("google drive node is null.Please refresh page.");
            }
        }
        catch (Exception e)
        {
            Logger.Error("An error occurred when get google drive node. Error:{0}", e.ToString());
        }

        return result;
    }


    private GoogleDriveTreeMessage InitGoogleDriveRoot()
    {
        Logger.Info("Init google drive root level node list.");
        try
        {
            var treeMessage = new GoogleDriveTreeMessage()
            {
                TreeType = TreeType.GoogleDriveArchiverTree,
                Node = new() { Level = NodeLevel.Root }
            };
            return RABrowserClient.BrowseGoogle(treeMessage);

        }
        catch (AveException ae)
        {
            Logger.Error(ae.Message, ae);
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message, e);
            throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
        }
    }

    public async Task<List<RMGoogleTreeNode>> BrowserRMTreeAsync(RMGoogleTreeNode parent, bool needCheckPermission = false)
    {
        List<RMGoogleTreeNode> result = new List<RMGoogleTreeNode>();
        var children = GoogleBrowser.BrowserTreeNode(RMDtoConverter.ConvertGoogleRM2Dto(parent));
        if (children.IsNotNullOrEmpty())
        {
            List<string> containers = new List<string>();
            bool isAdminUser = false;
            if (needCheckPermission && parent.Level == (int)NodeLevel.Root)
            {
                if (await IsGoogleAdminAsync() && await IsOpusSOGoogleAdminAsync())
                {
                    Logger.Info("Current user is admin and skip check permission.UserId:{0}.", TenantLocalValue.LogonUserId);
                    isAdminUser = true;
                }
                else
                {
                    var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    containers = (await ScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => x.Key == (int)SourceFlag.Google).Select(x => x.Value.ToString()).ToList();
                }
            }
            foreach (GoogleDriveTreeNodeDto dto in children)
            {
                #region I18N for tree node
                if (parent.Level == (int)NodeLevel.Root)
                {
                    if (needCheckPermission)
                    {
                        if (isAdminUser)
                        {
                            Logger.Info("Current user is admin and skip check permission.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, dto.Name);
                        }
                        else
                        {
                            if (!containers.Contains(dto.ID))
                            {
                                Logger.Info("Skip this node due to current user does not have permission for this container.UserId:{0}.ContainerName:{1}.", TenantLocalValue.LogonUserId, dto.Name);
                                continue;
                            }
                        }
                    }
                    // to-do: add I18N for default node name
                }
                #endregion
                RMGoogleTreeNode child = new();
                // to-do: add new convert method later
                child.CopyProperties(RMDtoConverter.ConvertTreeNodeDto2RMGoogleDriveTree(dto));
                child.Parent = parent;
                result.Add(child);
            }
        }
        return result;
    }

    public List<GoogleDriveTreeNodeDto> BrowserTreeAsync(GoogleDriveTreeNodeDto parent)
    {
        List<GoogleDriveTreeNodeDto> result = new List<GoogleDriveTreeNodeDto>();
        var children = GoogleBrowser.BrowserTreeNode(parent);
        if (children.IsNotNullOrEmpty())
        {
            foreach (GoogleDriveTreeNodeDto dto in children)
            {
                dto.Parent = parent;
                dto.ParentId = parent.ID;
                result.Add(dto);
            }
        }
        return result;
    }

    //private GoogleDriveTreeMessage ConvertRM2Message(RMGoogleTreeNode node)
    //{
    //    return new GoogleDriveTreeMessage()
    //    {
    //        Node = RMDtoConverter.ConvertGoogleRM2Dto(node),
    //        TreeType = TreeType.GoogleDriveArchiverTree
    //    };
    //}

    //private GoogleDriveTreeMessage ConvertDto2Message(GoogleDriveTreeNodeDto dto)
    //{
    //    return new GoogleDriveTreeMessage()
    //    {
    //        Node = dto,
    //        TreeType = TreeType.GoogleDriveArchiverTree
    //    };
    //}

    private bool IsGoogleContainer(int level)
    {
        return level == (int)NodeLevel.GoogleMyDriveContainer || level == (int)NodeLevel.GoogleSharedDriveContainer;
    }

    private bool IsGoogleDrive(int level)
    {
        return level == (int)NodeLevel.GoogleMyDrive || level == (int)NodeLevel.GoogleSharedDrive;
    }

    #region Support GoogleOne

    #region Build SQL for Browse/Filter 
    private async Task<(string Sql, List<SqlParameter> Parameters)> BuildQueryForGoogleNodesAsync(RMSampleGoogleTreeNode node, bool checkPermission)
    {
        var pager = node.BrowsePager ?? GetDefaultBrowsePager();
        var parameters = new List<SqlParameter>();
        var whereClauses = new List<string>();
        var sqlBuilder = new StringBuilder();

        sqlBuilder.AppendLine(@"
        SELECT n.*, s.*
        FROM [@SCHEMA].[RMRemoteNodes] AS n
        LEFT JOIN [@SCHEMA].[RMGoogleSettings] AS s ON n.Id = s.ScopeId");

        var columnMapping = (NodeLevel)node.Level switch
        {
            NodeLevel.Root => s_containerColumnNameMapping,
            NodeLevel.GoogleSharedDriveContainer or NodeLevel.GoogleMyDriveContainer => s_driveColumnNameMapping,
            _ => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        bool needCheckPermission = checkPermission
            && !await IsGoogleAdminAsync()
            && !await IsOpusSOGoogleAdminAsync();

        await AddNodeRelationshipClause(node, whereClauses, parameters, needCheckPermission);

        if (!string.IsNullOrWhiteSpace(pager.SearchText))
            AddSearchClause(node, pager.SearchText, whereClauses, parameters);

        if (pager.Filters?.Any() == true)
            AddFilterClause(node, columnMapping, whereClauses, parameters);

        if (whereClauses.Any())
            sqlBuilder.AppendLine(" WHERE " + string.Join(" AND ", whereClauses));

        AddSortClause(pager.Order, columnMapping, sqlBuilder);

        AddPagingClause(pager, parameters, sqlBuilder);

        return (sqlBuilder.ToString(), parameters);
    }

    private string GenerateInParams<T>(IEnumerable<T> values, List<SqlParameter> parameters, string paramBaseName)
    {
        var paramNames = new List<string>();
        int index = 0;
        foreach (var val in values)
        {
            var paramName = $"@{paramBaseName}_{index++}";
            paramNames.Add(paramName);
            AddParam(paramName, (object)val ?? DBNull.Value, parameters);
        }
        return $"({string.Join(", ", paramNames)})";
    }

    private void AddParam(string name, object val, List<SqlParameter> parameters)
    {
        if (!parameters.Any(p => p.ParameterName.Equals(name)))
            parameters.Add(new SqlParameter(name, val ?? DBNull.Value));
    }

    private async Task AddNodeRelationshipClause(RMSampleGoogleTreeNode node, List<string> whereClauses, List<SqlParameter> parameters, bool needCheckPermission)
    {
        switch ((NodeLevel)node.Level)
        {
            case NodeLevel.Root:
                var containerLevels = new[]
                {
                    (int)NodeLevel.GoogleSharedDriveContainer,
                    (int)NodeLevel.GoogleMyDriveContainer
                };
                whereClauses.Add("n.ParentId IS NULL");
                whereClauses.Add($"n.NodeLevel IN {GenerateInParams(containerLevels, parameters, "NODELEVEL")}");

                if (needCheckPermission)
                {
                    var permissionIds = await RemoteNodeDao.GetPermissionContainerIdsAsync();
                    if (permissionIds.Any())
                        whereClauses.Add($"n.Id IN {GenerateInParams(permissionIds, parameters, "PERMID")}");
                }
                break;

            case NodeLevel.GoogleSharedDriveContainer:
            case NodeLevel.GoogleMyDriveContainer:
                var childLevels = new[]
                {
                    (int)NodeLevel.GoogleMyDrive,
                    (int)NodeLevel.GoogleSharedDrive
                };
                whereClauses.Add("n.ParentId = @PARENTID");
                AddParam("@PARENTID", node.Id, parameters);
                whereClauses.Add($"n.NodeLevel IN {GenerateInParams(childLevels, parameters, "NODELEVEL")}");
                break;
        }
    }

    private void AddSearchClause(RMSampleGoogleTreeNode node, string searchText, List<string> whereClauses, List<SqlParameter> parameters)
    {
        var searchConditions = new List<string>();
        if (node.Level == (int)NodeLevel.Root)
        {
            var matchedKeys = s_i18nDisplayNameMapping
                .Where(kvp =>
                    (kvp.Key == RMConstants.DEFAULT_GOOGLE_USER_GROUP ||
                     kvp.Key == RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP) &&
                    kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            if (matchedKeys.Any())
                searchConditions.Add($"n.Name IN {GenerateInParams(matchedKeys, parameters, "SEARCHTEXT")}");
        }
        var likeParam = "@SEARCHTEXT";
        AddParam(likeParam, $"%{searchText}%", parameters);
        searchConditions.Add($"n.Name LIKE {likeParam}");
        whereClauses.Add($"({string.Join(" OR ", searchConditions)})");
    }

    private void AddFilterClause(RMSampleGoogleTreeNode node, Dictionary<string, string> columnMapping, List<string> whereClauses, List<SqlParameter> parameters)
    {
        var validFilters = node.BrowsePager.Filters
            .Select(fv => BuildFilterKeyValues(fv, columnMapping))
            .Where(fv => fv.HasValue)
            .Select(fv => fv!.Value);

        bool isGoogleContainer = IsGoogleContainer(node.Level);
        bool isRoot = node.Level == (int)NodeLevel.Root;

        string SchemaSubQuery(string table, string whereClause, bool isNegative = false)
            => $"{(isNegative ? "NOT " : string.Empty)}EXISTS (SELECT 1 FROM [@SCHEMA].[{table}] ps WHERE {whereClause})";

        foreach (var (column, rawValue) in validFilters)
        {
            var paramName = $"@{column}";
            object value = rawValue;

            switch (column)
            {
                case nameof(RMGoogleSetting.EnableRecordManagement):
                    {
                        int intVal = Convert.ToInt32(value);

                        if (isRoot && intVal == (int)EnableRecordManagementSetting.Disable)
                        {
                            whereClauses.Add($"({column} IS NULL OR {column} = {paramName})");
                            AddParam(paramName, intVal, parameters);
                            continue;
                        }

                        if (isGoogleContainer)
                        {
                            AddParam("@EnableRecordManagement", intVal, parameters);
                            var subQuery = SchemaSubQuery(
                                "RMGoogleSettings",
                                "ps.ScopeId = @PARENTID AND ps.EnableRecordManagement = @EnableRecordManagement"
                            );
                            whereClauses.Add($"(({column} = @EnableRecordManagement) OR ({column} IS NULL AND {subQuery}))");
                            continue;
                        }

                        whereClauses.Add($"{column} = {paramName}");
                        AddParam(paramName, intVal, parameters);
                        continue;
                    }

                case nameof(RMGoogleSetting.IsNullClassificationSetting):
                    {
                        bool boolVal = !(bool)value;

                        if (isGoogleContainer)
                        {
                            var sub1 = SchemaSubQuery(
                                "RMGoogleSettings",
                                "ps.ScopeId = @PARENTID AND ps.IsNullClassificationSetting = @IsNullClassificationSetting AND ps.EnableRecordManagement = 1"
                            );
                            var sub2 = SchemaSubQuery("RMGoogleSettings", "ScopeId = n.Id", isNegative: true);

                            AddParam("@IsNullClassificationSetting", boolVal, parameters);
                            whereClauses.Add($"{sub1} AND {sub2}");
                            continue;
                        }
                        whereClauses.Add($"{column} = {paramName}  AND EnableRecordManagement = 1");
                        AddParam(paramName, boolVal, parameters);
                        continue;
                    }

                case nameof(RMSampleGoogleTreeNode.IconStatus) when isGoogleContainer:
                    {
                        bool isBreak = Convert.ToInt32(value) == (int)IconStatus.Break;
                        var subQuery = SchemaSubQuery(
                            "RMGoogleSettings",
                            "s.ScopeId = n.Id AND s.ContainerId = n.ParentId",
                            isNegative: !isBreak
                        );
                        whereClauses.Add(subQuery);
                        continue;
                    }

                default:
                    whereClauses.Add($"{column} = {paramName}");
                    AddParam(paramName, value, parameters);
                    break;
            }
        }
    }

    private void AddSortClause(BrowseOrder order, Dictionary<string, string> columnMapping, StringBuilder sql)
    {
        string mappedColumn = columnMapping.TryGetValue(order.OrderByColumn, out var col) ? col : null;
        mappedColumn = col switch
        {
            nameof(RMGoogleSetting.IsNullClassificationSetting) => nameof(RMGoogleSetting.IsNullClassificationSetting),
            _ => nameof(RMSampleGoogleTreeNode.Name)
        };
        sql.AppendLine($" ORDER BY {mappedColumn}{(order.OrderByDesc ? " DESC" : string.Empty)}");
    }

    private void AddPagingClause(BrowsePager pager, List<SqlParameter> parameters, StringBuilder sql)
    {
        sql.AppendLine(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY");
        AddParam("@OFFSET", pager.PageIndex * pager.PageSize, parameters);
        AddParam("@FETCH", pager.PageSize, parameters);
    }
    #endregion

    public async Task<RMSampleGoogleTreeNode> BrowseGoogleNodesByPagerAsync(RMSampleGoogleTreeNode node, bool checkPermission)
    {
        var nodeToReturn = node;
        var pager = nodeToReturn.BrowsePager = nodeToReturn.BrowsePager ?? GetDefaultBrowsePager();

        (string sql, List<SqlParameter> parameters) queryTuple = await BuildQueryForGoogleNodesAsync(nodeToReturn, checkPermission);
        List<RMRemoteNode> children = await RemoteNodeDao.QueryGoogleNodesForSearchAsync(nodeToReturn, queryTuple);
        if (!children.Any())
        {
            nodeToReturn.Children = new();
            nodeToReturn.ChildrenCount = 0;
            nodeToReturn.BrowsePager = pager;
            return nodeToReturn;
        }
        pager.HasNext = children.Count > pager.PageSize;
        var pagedChildren = pager.HasNext ? children.Take(pager.PageSize).ToList() : children;
        nodeToReturn.Children = pagedChildren.Select(c => Convert2SampleGoogleTreeNode(c, nodeToReturn)).ToList();
        AssemblePropertiesForNodes(nodeToReturn);
        await LoadGoogleSampleSettingsAsync(nodeToReturn.Children);
        nodeToReturn.Children.ForEach(c => c.Parent = null);
        nodeToReturn.BrowsePager = pager;
        return nodeToReturn;
    }

    public async Task LoadGoogleSampleSettingsAsync(List<RMSampleGoogleTreeNode> nodes)
    {
        try
        {
            if (nodes.IsNotNullOrEmpty())
            {
                var availableRules = RuleDao.GetAvailableRules();
                RMSampleGoogleTreeNode container = nodes[0];
                if (!IsGoogleContainer(container.Level))
                {
                    container = TryGetGoogleContainerNode(container);

                    Guid containerId = Guid.Empty;
                    if (container != null)
                    {
                        containerId = new Guid(container.Id);
                    }

                    var containerSetting = RemoteNodeDao.LoadGoogleSetting(containerId, Guid.Empty);

                    var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.GoogleDisposalSchedule);
                    List<string> allSchedulesProfilesId = new();
                    if (allSchedules != null && allSchedules.Count != 0)
                    {
                        allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                    }

                    foreach (var node in nodes)
                    {
                        RMSampleGoogleTreeNode driveNode = TryGetGoogleDriveNode(node);

                        Guid driveId = Guid.Empty;
                        if (driveNode != null)
                        {
                            driveId = new Guid(driveNode.Id);
                        }

                        ArgumentCheck.NotNull(node, nameof(node));
                        var driveSetting = RemoteNodeDao.LoadGoogleSetting(containerId, driveId);
                        if (driveSetting != null)
                        {
                            node.IconStatus = IconStatus.Break;
                            AssembleSampleSettingsForNode(node, driveSetting, availableRules);
                            continue;
                        }

                        var profileId = ScheduleService.GetProfileId(node);
                        if (allSchedulesProfilesId.Contains(profileId) && containerSetting != null)
                        {
                            node.IconStatus = IconStatus.Inhert;
                            AssembleSampleSettingsForNode(node, containerSetting, availableRules);
                            var schedule = allSchedules.FirstOrDefault(s => s.ProfileId == profileId);
                            node.ScheduleInfo = schedule;
                            continue;
                        }

                        if (containerSetting != null)
                        {
                            node.IconStatus = IconStatus.Inhert;
                            AssembleSampleSettingsForNode(node, containerSetting, availableRules);
                            continue;
                        }
                        node.IconStatus = IconStatus.NoSet;
                    }
                }
                else
                {
                    foreach (var selfContainerNode in nodes)
                    {
                        var selfContainerSetting = RemoteNodeDao.LoadGoogleSetting(new Guid(selfContainerNode.Id), Guid.Empty);
                        var profileId = ScheduleService.GetProfileId(selfContainerNode);
                        var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.GoogleDisposalSchedule);
                        selfContainerNode.IconStatus = selfContainerSetting == null && disposeSchedule == null ? IconStatus.NoSet : IconStatus.Break;
                        AssembleSampleSettingsForNode(selfContainerNode, selfContainerSetting, availableRules);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("An error occurred when load GoogleSetting Icon.Error:{0}", e.ToString());
            throw;
        }
    }

    private BrowsePager GetDefaultBrowsePager()
    {
        return new BrowsePager
        {
            PageIndex = 0,
            PageSize = 20,
            TotalCount = 0,
            HasNext = false,
            SearchText = string.Empty,
            Filters = new List<BrowseFilter>(),
            Order = new BrowseOrder
            {
                OrderByColumn = nameof(RMSampleGoogleTreeNode.DisplayName),
                OrderByDesc = false
            }
        };
    }

    private RMSampleGoogleTreeNode Convert2SampleGoogleTreeNode(RMRemoteNode childNode, RMSampleGoogleTreeNode? parentNode = null)
    {
        var sample = new RMSampleGoogleTreeNode
        {
            Id = childNode.Id,
            Name = RMDatabaseDefaultEncryptor.DecryptToString(childNode.UserName),
            DisplayName = childNode.Name,
            NodeType = childNode.NodeLevel,
            Level = childNode.NodeLevel,
            ParentId = childNode.ParentId,
            ObjectId = childNode.ObjectId,
            Parent = parentNode,
            FullPath = childNode.Url,
            GoogleTenantId = childNode.TenantId
        };

        switch (sample.NodeType)
        {
            case (int)NodeLevel.GoogleMyDrive:
            case (int)NodeLevel.GoogleSharedDrive:
                sample.NodeId = sample.Id;
                sample.ContainerId = sample.ParentId;
                break;

            case (int)NodeLevel.GoogleMyDriveContainer:
            case (int)NodeLevel.GoogleSharedDriveContainer:
                sample.ContainerId = sample.Id;
                break;
        }

        return sample;
    }

    private void AssembleSampleSettingsForNode(RMSampleGoogleTreeNode node, RMGoogleSetting setting, List<RMRule> availableRules)
    {
        if (setting == null)
        {
            node.IsEnableLifeCycleManagement = false;
            node.IsEnableClassification = false;
            node.Plan = string.Empty;
            node.ScheduleInfo = null;
            return;
        }
        var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(setting.NodeInfo);
        node.IsEnableLifeCycleManagement = setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
        node.IsEnableClassification = !setting.IsNullClassificationSetting;
        node.Plan = GeneratePlanContent(setting, nodeInfo, availableRules);
        node.ScheduleInfo = nodeInfo?.DisposeScheduleInfo;
    }

    private void AssemblePropertiesForNodes(RMSampleGoogleTreeNode parentNode)
    {
        if (parentNode.Children == null || parentNode.Children.Count == 0) return;

        var isRoot = parentNode.Level == (int)NodeLevel.Root;
        var parentId = isRoot ? null : parentNode.Id;

        Parallel.ForEach(parentNode.Children, child =>
        {
            child.BrowsePager = null;
            child.ParentId = parentId;
            if (isRoot && !string.IsNullOrWhiteSpace(child.DisplayName)
            && s_i18nDisplayNameMapping.TryGetValue(child.DisplayName, out var i18nKey))
            {
                child.DisplayName = i18nKey;
            }
        });
    }

    private string GeneratePlanContent(RMGoogleSetting setting, RMGoogleTreeNode nodeInfo, List<RMRule> availableRules)
    {
        var planObj = new ExpandoObject() as IDictionary<string, object>;

        planObj["withClassification"] = setting.DeployLabelMethod.ToString();
        planObj["withoutClassification"] = string.Empty;

        var rules = nodeInfo?.Rules;
        if (rules == null || rules.Count == 0)
            return JsonConvert.SerializeObject(planObj);

        var existingRules = rules
            .Where(r => availableRules.Any(ar => ar.RuleId == r.RuleId))
            .OrderBy(r => r.RuleOrder)
            .ToList();

        if (existingRules.Count > 0)
        {
            var rulesString = string.Join("; ", existingRules.Select((r, index) => $"{index + 1}. {r.RuleName}"));
            planObj["withoutClassification"] = rulesString;
        }

        return JsonConvert.SerializeObject(planObj);
    }

    private (string Column, object Value)? BuildFilterKeyValues(BrowseFilter filter, Dictionary<string, string> columnMapping)
    {
        if (filter == null || string.IsNullOrWhiteSpace(filter.ColumnName) || string.IsNullOrWhiteSpace(filter.ColumnValue))
            return null;

        if (!columnMapping.TryGetValue(filter.ColumnName, out string mappedColumn) || string.IsNullOrWhiteSpace(mappedColumn))
            return null;

        if (!s_filterParsers.TryGetValue(mappedColumn, out var parser))
            return null;

        var parsedValue = parser(filter.ColumnValue);
        return parsedValue != null ? (mappedColumn, parsedValue) : null;
    }

    private static readonly Dictionary<string, string> s_containerColumnNameMapping = new Dictionary<string, string>
    {
        { "EnableClassification" , nameof(RMGoogleSetting.IsNullClassificationSetting) },
        { "Status" , nameof(RMGoogleSetting.EnableRecordManagement) },
    };

    private static readonly Dictionary<string, string> s_driveColumnNameMapping = new Dictionary<string, string>
    {
        { "EnableClassification" , nameof(RMGoogleSetting.IsNullClassificationSetting) },
        { "Status" , nameof(RMGoogleSetting.EnableRecordManagement) },
        { "InheritParentSetting" , nameof(RMSampleGoogleTreeNode.IconStatus) },
    };

    private static readonly Dictionary<string, Func<string, object>> s_filterParsers = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(RMSampleGoogleTreeNode.IconStatus)] = raw => raw switch
        {
            "0" => IconStatus.NoSet,
            "1" => IconStatus.Inhert,
            "2" => IconStatus.Break,
            _ => null
        },

        [nameof(RMGoogleSetting.IsNullClassificationSetting)] = raw => raw switch
        {
            "0" => false,
            "1" => true,
            _ => null
        },

        [nameof(RMGoogleSetting.EnableRecordManagement)] = raw => raw switch
        {
            "1" => (int)EnableRecordManagementSetting.Enable,
            "2" => (int)EnableRecordManagementSetting.Disable,
            _ => null
        }
    };

    private static readonly Dictionary<string, string> s_i18nDisplayNameMapping = new()
    {
            {
                RMConstants.DEFAULT_GOOGLE_USER_GROUP,
                I18N.Core.I18NEntity.GetString("RM_GoogleUser_Default_Container")
            },
            {
                RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP,
                I18N.Core.I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container")
            }
    };
    #endregion
}