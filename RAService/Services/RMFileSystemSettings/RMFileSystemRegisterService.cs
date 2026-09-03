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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using CommonModel.DataModel;
using Microsoft.Graph.Beta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    [Audit]
    public class RMFileSystemRegisterService : RMServiceBase, IRMFileSystemRegisterService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMFileSystemRegisterService));
        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));

        private static readonly Regex UncPathRegex = new Regex(
            @"^\\\\[^\\/:*?""<>|]+\\[^\\/:*?""<>|]+(\\[^\\/:*?""<>|]+)*$",
            RegexOptions.Compiled);
        private IFSConnectionGroupDao FSGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();

        private IFileSystemSettingDao FSSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();

        private IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMemebershipDao => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();

        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IRMFSConnectionAndOwnerRelationshipDao FSConnectionOwnerDao => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();

        private IUserService UserServices => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private IFSConnectionRelatedJobInfoDao FSConnectionRelatedJobInfoDao => PlatformWindsorManager.GetService<IFSConnectionRelatedJobInfoDao>();

        private IRMFunctionSettingDao RMFunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        public void DeleteConnectoin(Guid connectionId)
        {
            FSConnectionDao.DeleteConnectoin(connectionId);
        }
        
        public void DeleteGroupConnectoin(Guid groupId)
        {
            FSGroupDao.DeleteGroupConnectoin(groupId);
        }

        public Task<List<ConnectionDto>> GetAllConnectionsByGroupIdAsync(Guid groupId)
        {
            try
            {
                return FSConnectionDao.GetAllConnectionsByGroupId(groupId).ConvertAllAsync(conn => ConvertConnectionDtoAsync(conn));
            }
            catch (Exception e)
            {
                logger.Error($"Get all connections by groupid error {e}");
                return default;
            }
        }

        public Task<ConnectionDto> GetConnectionByIdAsync(Guid connectionId)
        {
            var connection = FSConnectionDao.GetConnectionById(connectionId);
            return connection == null ? Task.FromResult<ConnectionDto>(null) : ConvertConnectionDtoAsync(connection);
        }
        public string GetConnectionNameByIdAsync(Guid connectionId)
        {
            string name = string.Empty;
            var temp = FSConnectionDao.GetConnectionById(connectionId);
            name = temp?.Name;
            return name;
        }
        public async Task<List<ConnectionDto>> GetConnectionByIdsAsync(List<Guid> connectionIds)
        {
            List<ConnectionDto> resultDto = new List<ConnectionDto>();
            var tempResult = FSConnectionDao.GetConnectionByIds(connectionIds);
            foreach (var item in tempResult)
            {
                resultDto.Add(await ConvertConnectionDtoAsync(item));
            }
            return resultDto;
        }
        public Task<ConnectionGroupDto> GetGroupByIdAsync(Guid groupId)
        {
            return ConvertConnectionGroupDtoAsync(FSGroupDao.GetGroupById(groupId));
        }

        public Task<ConnectionGroupDto> GetGroupAsync(Guid groupId)
        {
            return ConvertConnectionGroupDtoAsync(FSGroupDao.GetGroup(groupId));
        }

        public Task<ConnectionGroupDto> GetGroupOrNullAsync(Guid groupId)
        {
            var group = FSGroupDao.GetGroupOrNull(groupId);
            return group == null ? Task.FromResult<ConnectionGroupDto>(null) : ConvertConnectionGroupDtoAsync(group);
        }

        public Task<List<ConnectionGroupDto>> LoadAllGroupsAsync()
        {
            try
            {
                return FSGroupDao.LoadAllGroups().ConvertAllAsync(group => ConvertConnectionGroupDtoAsync(group));
            }
            catch (Exception e)
            {
                logger.Error($"LoadAllGroups -- {e}");
                return default;
            }
        }

        public async Task<List<ConnectionGroupDto>> GetAllGroupsAsync()
        {
            try
            {
                if (await MultiGeoDataCenterService.IsLimitMultiGeoManageContainer())
                {
                    return await FSGroupDao.LoadAllGroupsByDCInternalName(RMSSOHelper.CurrentDCName).ConvertAllAsync(ConvertConnectionGroupDtoAsync);
                }
                return await FSGroupDao.LoadAllGroups().ConvertAllAsync(ConvertConnectionGroupDtoAsync);
            }
            catch (Exception e)
            {
                logger.Error($"Get All FS connection group have errors -- {e}");
                return default;
            }
        }
        

        public Task<List<ConnectionDto>> LoadAllConnectionAsync(bool onlyNoGroup)
        {
            return FSConnectionDao.GetAllConnections(onlyNoGroup).ConvertAllAsync(group => ConvertConnectionDtoAsync(group));
        }

        public async Task<ConnectionResultData> LoadAllNoGroupConnectionAsync(GetConnectionListParam param)
        {
            if(await MultiGeoDataCenterService.IsLimitMultiGeoManageContainer())
            {
                return new ConnectionResultData();
            }
            var dbConnections = FSConnectionDao.GetAllNoGroupConnections(param, out int totalCount);
            var connectionList = await dbConnections.ConvertAllAsync(conn => ConvertConnectionDtoAsync(conn));
            return new ConnectionResultData
            {
                ConnectionList = connectionList,
                TotalCount = totalCount
            };
        }

        public async Task<List<string>> LoadAllConnectionGroupNamesAsync()
        {
            if(await MultiGeoDataCenterService.IsLimitMultiGeoManageContainer())
            {
                return FSGroupDao.LoadAllConnectionGroupNamesByDCInternalName(RMSSOHelper.CurrentDCName);
            }
            return FSGroupDao.LoadAllConnectionGroupNames();
        }

        public async Task<ConnectionResultData> QueryConnectionByPagerAsync(GetConnectionListParam param)
        {
            ConnectionResultData result = new ConnectionResultData();
            try
            {
                logger.Info($"Start querying connection. PageIndex={param.PageIndex}, PageSize={param.PageSize}");

                param.Filters ??= new List<FSConnectionFilter>();
                param.Order ??= new FSConnectionOrder { ColumnName = nameof(FSConnection.LastModifiedTime), IsDesc = true };

                if (!string.IsNullOrWhiteSpace(param.SearchKey))
                {
                    param.Filters.Insert(0, new FSConnectionFilter
                    {
                        ColumnName = nameof(FSConnection.Name),
                        ColumnValues = new() { param.SearchKey }
                    });
                }
                var filterExpression = new FSConnectionFilterBuilder(param.Filters).Build();
                if (!IsValidOrderColumn<FSConnection>(param.Order?.ColumnName))
                {
                    param.Order = new()
                    {
                        ColumnName = nameof(FSConnection.LastModifiedTime),
                        IsDesc = true
                    };
                }
                int totalCount;
                var list = await MultiGeoDataCenterService.IsLimitMultiGeoManageContainer() ? 
                    await FSConnectionDao.QueryConnectionsPagerForOtherDCs(filterExpression, param, out totalCount, RMSSOHelper.CurrentDCName).ConvertAllAsync(ConvertConnectionDtoAsync)
                    : await FSConnectionDao.QueryConnectionsPager(filterExpression, param, out totalCount).ConvertAllAsync(ConvertConnectionDtoAsync);
                result.ConnectionList = list;
                result.TotalCount = totalCount;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error while quering connection by pager. Ex: {ex}.");
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.CreateFSGroup, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.CreateFSGroup, AuditLevel = FSAuditLevel.ConnectionGroup, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<Guid> CreateConnectionGroupAsync(ConnectionGroupDto connectionGroup)
        {
            if (connectionGroup.Id == Guid.Empty)
            {
                connectionGroup.Id = Guid.NewGuid();
            }
           await FSGroupDao.SaveConnectionGroupAsync(await ConvertConnectionGroupEntity(connectionGroup));
            var needUpdateGroupIdConnections = connectionGroup.FSConnections.Select(item => item.Id).ToList();
            FSConnectionDao.UpdateConnectionsGroupId(connectionGroup.Id, needUpdateGroupIdConnections);
            await FSConnectionGroupWithAgentMemebershipDao.RemoveAllAsync(connectionGroup.Id);
            var needRelateAgentIds = connectionGroup.Agents.Select(item => item.Id).ToList();
            FSConnectionGroupWithAgentMemebershipDao.AddMemberships(connectionGroup.Id, needRelateAgentIds);
            return connectionGroup.Id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.CreateFSConnection, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.CreateFSConnection, AuditLevel = FSAuditLevel.Connection, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<int> CreateConnectoinAsync(ConnectionDto connection)
        {
            var enableJPMCFileSystemFeature = RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
            if (connection.Id == Guid.Empty)
            {
                connection.Id = Guid.NewGuid();
            }
            if (FSConnectionDao.CheckConnectoinUNCPathExist(connection.Id, connection.UNCPath))
            {
                return -2;//TODO xwwang Error Code.
            }
            else if (enableJPMCFileSystemFeature && (FSConnectionDao.CheckConnectionIdExist(connection.JPMCConnectionId) && !string.IsNullOrEmpty(connection.JPMCConnectionId)))
            {
                return -3;
            }
            else if (enableJPMCFileSystemFeature && string.IsNullOrEmpty(connection.JPMCConnectionId))
            {
                return -6;
            }
            else if (enableJPMCFileSystemFeature && connection.JPMCConnectionId.Length > 255)
            {
                return -5;
            }
            if (FSConnectionDao.GetConnectionByName(connection.Name) != null)
            {
                return -4;
            }
            if (enableJPMCFileSystemFeature)
            {
                var allUsers = new List<ToUserInfo>();
                if (connection.RecordOwners != null && connection.RecordOwners.Count > 0)
                {
                    allUsers.AddRange(connection.RecordOwners);
                }
                if (connection.InformationOwners != null && connection.InformationOwners.Count > 0)
                {
                    allUsers.AddRange(connection.InformationOwners);
                }

                if (allUsers.Count > 0)
                {
                    var syncResult = await SyncOwnerUsersAsync(allUsers);
                    if (syncResult == 0) return -7;
                }
                await FSConnectionDao.SaveConnectoinAsync(ConvertConnectionEntity(connection));
                await AddConnectinOwners(connection.Id, connection.InformationOwners, connection.RecordOwners);
                //await SaveConnectionOwnersAsync(connection.Id, connection.RecordOwners, connection.InformationOwners);
            }
            else
            {
                await FSConnectionDao.SaveConnectoinAsync(ConvertConnectionEntity(connection));
            }
            return 1;
        }

        public async Task<RAReturnMessage> UpdateRecordManagementStatus(string connectionId, RMFSTreeNode.EnableRecordManagementSetting status)
        {
            try
            {
                await FSSettingDao.UpdateRecordManagementStatus(new Guid(connectionId), (int)status);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while updating record management status. ConnectionId: {connectionId}, Status: {status}, Error: {ex}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while updating record management status."
                };

            }
        }

        public RAReturnMessage ValidateConnection(ConnectionDto connectionDto, bool isCreate)
        {
            if (connectionDto == null)
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation."
                    : "Invalid connection payload for update.");
            }

            if (isCreate ? connectionDto.Id != Guid.Empty : connectionDto.Id == Guid.Empty)
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation."
                    : "Invalid connection payload for update.");
            }

            if (string.IsNullOrWhiteSpace(connectionDto.Name))
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation."
                    : "Invalid connection payload for update.");
            }

            if (!IsValidUncPath(connectionDto.UNCPath))
            {
                return CreateFailedResult(I18NEntity.GetString("RM_FS_Register_UNCPathInputValidateMessage"));
            }

            var enableJPMCFileSystemFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            if (enableJPMCFileSystemFeature)
            {
                if (connectionDto.RecordOwners != null && connectionDto.RecordOwners.Count > 0 && !HasValidOwners(connectionDto.RecordOwners))
                {
                    return CreateFailedResult("RecordOwners is invalid, or it is required because JPMC file system feature is enabled.");
                }

                if (!HasValidOwners(connectionDto.InformationOwners))
                {
                    return CreateFailedResult("InformationOwners is invalid, or it is required because JPMC file system feature is enabled.");
                }
            }

            return null;
        }

        private RAReturnMessage CreateFailedResult(string errorMessage)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = errorMessage
            };
        }

        private bool HasValidOwners(List<ToUserInfo> owners)
        {
            if (owners == null || owners.Count == 0)
            {
                return false;
            }

            foreach (var owner in owners)
            {
                if (string.IsNullOrWhiteSpace(owner?.Id))
                {
                    return false;
                }

                var aadAccount = ResolveAADAccount(owner.Id);
                if (aadAccount == null)
                {
                    logger.Warn($"Failed to resolve AAD account for owner Id {owner.Id}. The owner will be considered as invalid.");
                }
                else
                {
                    logger.Info($"Successfully resolved AAD account for owner Id {owner.Id}");
                    owner.UserPrincipalName = aadAccount.UserPrincipalName ?? aadAccount.Mail ?? aadAccount.DisplayName;
                    owner.DisplayName = aadAccount.DisplayName;
                    owner.Email = aadAccount.Mail;
                    owner.InviteType = aadAccount.InviteType;
                }
            }

            return true;
        }

        private bool IsValidUncPath(string uncPath)
        {
            return !string.IsNullOrWhiteSpace(uncPath)
                && UncPathRegex.IsMatch(uncPath.Trim());
        }

        private AADAccount ResolveAADAccount(string aadId)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            var group = TryGetGroup(tenantId, aadId);
            if (group != null)
            {
                group.InviteType = AccountType.Group;
                return group;
            }

            return TryGetUser(tenantId, aadId);
        }

        private AADAccount TryGetUser(string tenantId, string aadId)
        {
            try
            {
                return AccountWrapperService.GetAccount(tenantId, aadId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get user from AAD with tenantId {tenantId} and aadId {aadId}. Exception: {ex} Messsage: {ex.Message}");
                return null;
            }
        }

        private AADAccount TryGetGroup(string tenantId, string aadId)
        {
            try
            {
                return AccountWrapperService.GetGroupsByAadId(tenantId, aadId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get group from AAD with tenantId {tenantId} and aadId {aadId}. Exception: {ex} Messsage: {ex.Message}");
                return null;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditFSGroup, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.EditFSGroup, AuditLevel = FSAuditLevel.ConnectionGroup, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async System.Threading.Tasks.Task UpdateConnectionGroupAsync(ConnectionGroupDto connectionGroup)
        {
            await FSGroupDao.SaveConnectionGroupAsync(await ConvertConnectionGroupEntity(connectionGroup));
            var needUpdateGroupIdConnections = connectionGroup.FSConnections.Select(item => item.Id).ToList();
            FSConnectionDao.UpdateConnectionsGroupId(connectionGroup.Id, needUpdateGroupIdConnections);
            var needRemoveGroupIdConnections = connectionGroup.RemoveFSConnections.Select(item => item.Id).ToList();
            if(needRemoveGroupIdConnections.Count > 0)
            {
                await FSSettingDao.DeleteFSWithSubFolderSettingAsync(connectionGroup.RemoveFSConnections.Select(item => item.Id).ToList());
            }
            await FSConnectionGroupWithAgentMemebershipDao.RemoveAllAsync(connectionGroup.Id);
            var needRelateAgentIds = connectionGroup.Agents.Select(item => item.Id).ToList();
            FSConnectionGroupWithAgentMemebershipDao.AddMemberships(connectionGroup.Id, needRelateAgentIds);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditFSConnection, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.EditFSConnection, AuditLevel = FSAuditLevel.Connection, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<int> UpdateConnectoinAsync(ConnectionDto connection)
        {
            var enableJPMCFileSystemFeature = RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
            if (FSConnectionDao.CheckConnectoinUNCPathExist(connection.Id, connection.UNCPath))
            {
                return -2;//TODO xwwang Error Code.
            }
            if (enableJPMCFileSystemFeature && (FSConnectionDao.CheckUpdateConnectionIdExist(connection.JPMCConnectionId, connection.Id) && !string.IsNullOrEmpty(connection.JPMCConnectionId)))
            {
                return -3;
            }
            else if (enableJPMCFileSystemFeature && connection.JPMCConnectionId.Length > 255)
            {
                return -5;
            }
            var existing = FSConnectionDao.GetConnectionByName(connection.Name);
            if (existing != null && existing.Id != connection.Id)
            {
                return -4;
            }
            if (enableJPMCFileSystemFeature)
            {
                var allUsers = new List<ToUserInfo>();
                if (connection.RecordOwners != null && connection.RecordOwners.Count > 0)
                {
                    allUsers.AddRange(connection.RecordOwners);
                }
                if (connection.InformationOwners != null && connection.InformationOwners.Count > 0)
                {
                    allUsers.AddRange(connection.InformationOwners);
                }

                if (allUsers.Count > 0)
                {
                    var syncResult = await SyncOwnerUsersAsync(allUsers);
                    if (syncResult == 0) return -7;
                }
                await FSConnectionDao.SaveConnectoinAsync(ConvertConnectionEntity(connection));
                await FSConnectionOwnerDao.RemoveAllByConnectionIdAsync(connection.Id);
                await AddConnectinOwners(connection.Id, connection.InformationOwners, connection.RecordOwners);
            }
            else
            {
                await FSConnectionDao.SaveConnectoinAsync(ConvertConnectionEntity(connection));
            }
            return 1;
        }

        public List<AgentInformationDto> GetAllAgent()
        {
            List<AgentInformationDto> resultsList = new List<AgentInformationDto>();
            var allAgentsKeyValues = retryPolicy.ExecuteAction(()=>RASignalRAgentProxy.GetProxy().GetAllAgentsForce());
            logger.Info("Agent count : " + allAgentsKeyValues.Count);

            foreach (var agentList in allAgentsKeyValues.Values)
            {
                resultsList.AddRange(agentList.ConvertAll((a) =>
                {
                    return new AgentInformationDto()
                    {
                        AgentId = a.AgentId,
                        AgentName = string.Empty,//TODO xwwang
                        TenantId = a.TenantId
                    };
                }));
            }
            return resultsList;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSConnectionCorrelateGroup, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.EditFSGroup, AuditLevel = FSAuditLevel.ConnectionGroup, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<bool> CorrelateConnectionGroupAsync(CorrelateConnectionDto dto)
        {
            var existConnIds = FSConnectionDao.GetAllConnectionsByGroupId(dto.GroupId).Select(c => c.Id);
            foreach (var connId in dto.ConnectionIdList)
            {
                if (!existConnIds.Contains(connId))
                {
                    //Add
                    await FSConnectionDao.UpdateConnectoinGroupIdAsync(connId, dto.GroupId);
                }
            }
            foreach (var connId in existConnIds)
            {
                if (!dto.ConnectionIdList.Contains(connId))
                {
                    //Remove
                    await FSConnectionDao.UpdateConnectoinGroupIdAsync(connId, Guid.Empty);
                }
            }
            return true;//TODO xwwang
        }



        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.DeleteFSGroup, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.DeleteFSGroup, AuditLevel = FSAuditLevel.ConnectionGroup, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<int> DeleteGroupConnectoinAsync(List<Guid> groupIds)
        {
            int deleteCount = 0;
            try
            {
                var groupEntities = await FSGroupDao.FindListAsync(f => groupIds.Contains(f.Id));
                deleteCount = FSGroupDao.BatchDelete(groupEntities);
                var connectionEntities = await FSConnectionDao.FindListAsync(c => groupIds.Contains(c.GroupId));
                connectionEntities.ForEach(c => c.GroupId = Guid.Empty);
                FSConnectionDao.BatchUpdate(connectionEntities);
                await FSConnectionGroupWithAgentMemebershipDao.RemoveAllAsync(groupIds);
            }
            catch (Exception e)
            {
                logger.Error($"DeleteGroupConnectoin--- error: {e}");
                throw;
            }
            return deleteCount;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.DeleteFSConnection, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.DeleteFSConnection, AuditLevel = FSAuditLevel.Connection, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<int> DeleteConnectoinAsync(List<Guid> connectionIds)
        {
            try
            {
                var entities = await FSConnectionDao.FindListAsync(f => connectionIds.Contains(f.Id));
                await FSConnectionOwnerDao.RemoveAllByConnectionIdsAsync(connectionIds);
                await FSSettingDao.DeleteFSWithSubFolderSettingAsync(entities.Select(f => f.Id).ToList());
                return FSConnectionDao.BatchDelete(entities);
            }
            catch (Exception e)
            {
                logger.Error($"DeleteConnectoin--- error: {e}");
                return default;
            }
        }

        #region private method

        private async Task<ConnectionGroupDto> ConvertConnectionGroupDtoAsync(FSConnectionGroup fsConnectionGroup)
        {
            var dto = new ConnectionGroupDto();
            dto.Id = fsConnectionGroup.Id;
            dto.Name = fsConnectionGroup.Name;
            dto.Description = fsConnectionGroup.Description;
            dto.AccessConnectionType = fsConnectionGroup.AccessConnectionType;
            dto.LastModifiedTime = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(fsConnectionGroup.LastModifiedTime, true)).SimplifyFormatTime;
            dto.FSConnections = new List<ConnectionDto>();
            dto.Agents = fsConnectionGroup.Agents.ConvertAll(item => new AgentDto
            {
                Id = item.Id,
                Name = item.Name,
                SourceType = item.SourceType,
                Status = item.Status
            });
            await ConvertMultiGeoDCInfo(fsConnectionGroup, dto);
            foreach (var conn in fsConnectionGroup.FSConnections)
            {
                dto.FSConnections.Add(await ConvertConnectionDtoAsync(conn));
            }
            return dto;
        }

        private async Task ConvertMultiGeoDCInfo(FSConnectionGroup fsConnectionGroup, ConnectionGroupDto dto)
        {
            if (await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao))
            {
                dto.DataCenterType = Contract.Multi_Geo.Enum.DataCenterType.None;
            }

            var multiGeoDCInfoes = await MultiGeoDataCenterService.GetMultiGeoDCInformation();
            if (string.IsNullOrEmpty(fsConnectionGroup.DCInternalName))
            {
                dto.DataCenterType = Contract.Multi_Geo.Enum.DataCenterType.DefaultDC;
                dto.DCDisplayName = multiGeoDCInfoes.DCsSupported?.FirstOrDefault(dc => dc.DCInternalName.Equals(multiGeoDCInfoes.MainDC, StringComparison.OrdinalIgnoreCase))?.DCDisplayName ?? string.Empty;
            }
            else
            {
                dto.DataCenterType = Contract.Multi_Geo.Enum.DataCenterType.SpecificDC;
                dto.DCInternalName = fsConnectionGroup.DCInternalName;
                dto.DCDisplayName = multiGeoDCInfoes.DCsSupported?.FirstOrDefault(dc => dc.DCInternalName.Equals(dto.DCInternalName, StringComparison.OrdinalIgnoreCase))?.DCDisplayName ?? string.Empty;
            }
        }

        private async Task<ConnectionDto> ConvertConnectionDtoAsync(FSConnection fsConnection)
        {
            var dto = new ConnectionDto();
            dto.Id = fsConnection.Id;
            dto.GroupId = fsConnection.GroupId;
            dto.Name = fsConnection.Name;
            dto.Description = fsConnection.Description;
            dto.UNCPath = fsConnection.UNCPath;
            dto.LastModifiedTime = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(fsConnection.LastModifiedTime, true)).SimplifyFormatTime;
            dto.AgentId = fsConnection.AgentId;
            dto.GroupName = fsConnection.GroupName;
            dto.JPMCConnectionId = fsConnection.JPMCConnectionId;
            dto.LastSyncTime = fsConnection.LastSyncTime != 0 ? (await mGeneralSettingService.ConvertTiksToDateTimeAsync(fsConnection.LastSyncTime, true)).SimplifyFormatTime : null;
            dto.Monitor = fsConnection.FailureJobCount;
            dto.JPMCConnectionId = fsConnection.JPMCConnectionId;

            var owners = FSConnectionOwnerDao.GetOwnersByConnectionId(fsConnection.Id);
            if (owners != null && owners.Count > 0)
            {
                var userIds = owners.Select(o => o.UserIntId).Distinct().ToList();
                var users = await AccountDao.GetUserByIdsAsync(userIds);
                var ownerLookup = users.ToDictionary(item => item.Id, item => item);

                dto.RecordOwners = owners
                    .Where(o => o.Type == FSConnectionOwnerType.RecordOwner)
                    .Select(o => ConvertToAccountInfoDto(o.UserIntId, ownerLookup)).ToList();
                dto.InformationOwners = owners
                    .Where(o => o.Type == FSConnectionOwnerType.InformationOwner)
                    .Select(o => ConvertToAccountInfoDto(o.UserIntId, ownerLookup)).ToList();
            }
            return dto;
        }

        private async Task<FSConnectionGroup> ConvertConnectionGroupEntity(ConnectionGroupDto fsConnectionGroup)
        {
            var entity = new FSConnectionGroup();
            entity.Id = fsConnectionGroup.Id;
            entity.Name = fsConnectionGroup.Name;
            entity.Description = fsConnectionGroup.Description;
            entity.LastModifiedTime = DateTime.UtcNow.Ticks;
            entity.AccessConnectionType = fsConnectionGroup.AccessConnectionType;
            await ConvertMultiGeoDCIntoEntity(fsConnectionGroup, entity);
            return entity;
        }

        private async Task ConvertMultiGeoDCIntoEntity(ConnectionGroupDto fsConnectionGroup, FSConnectionGroup entity)
        {
            if (await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao))
            {
                switch (fsConnectionGroup.DataCenterType)
                {
                    case Contract.Multi_Geo.Enum.DataCenterType.SpecificDC:
                        entity.DCInternalName = fsConnectionGroup.DCInternalName;
                        break;
                    case Contract.Multi_Geo.Enum.DataCenterType.DefaultDC:
                    default:
                        entity.DCInternalName = string.Empty;
                        break;
                }
            }
        }

        private FSConnection ConvertConnectionEntity(ConnectionDto fsConnection)
        {
            var entity = new FSConnection();
            entity.Id = fsConnection.Id;
            entity.GroupId = fsConnection.GroupId;
            entity.Name = fsConnection.Name;
            entity.Description = fsConnection.Description;
            entity.LastModifiedTime = DateTime.UtcNow.Ticks;
            entity.UNCPath = fsConnection.UNCPath.TrimEnd('\\');
            entity.AgentId = fsConnection.AgentId;
            entity.JPMCConnectionId = fsConnection.JPMCConnectionId;
            entity.JPMCConnectionId = fsConnection.JPMCConnectionId;
            return entity;
        }
        private async Task SaveConnectionOwnersAsync(Guid connectionId, List<ToUserInfo> recordOwners, List<ToUserInfo> informationOwners)
        {
            var allUsers = new List<ToUserInfo>();
            if (recordOwners != null && recordOwners.Count > 0)
            {
                allUsers.AddRange(recordOwners);
            }
            if (informationOwners != null && informationOwners.Count > 0)
            {
                allUsers.AddRange(informationOwners);
            }

            if (allUsers.Count > 0)
            {
                await SyncOwnerUsersAsync(allUsers);
            }

            await AddConnectinOwners(connectionId, informationOwners, recordOwners);
        }

        private async Task AddConnectinOwners(Guid connectionId, List<ToUserInfo> informationOwners, List<ToUserInfo> recordOwners)
        {
            if(informationOwners == null || informationOwners.Count == 0)
            {
                return;
            }
            recordOwners ??= new List<ToUserInfo>();
            informationOwners.Concat(recordOwners).ForEach(item =>
            {
                if(string.IsNullOrWhiteSpace(item.UserId))
                {
                    var existsAccount = AccountDao.Find(existsItem => ((existsItem.UserPrincipalName == item.UserPrincipalName) ||
                                                                    (existsItem.UserPrincipalName == item.Email)) && existsItem.IsRemoved == 0);
                    var existAccountHasId = AccountDao.Find(existsItem => ((existsItem.AADId == item.Id) || (existsItem.UserId == item.UserId)) && existsItem.IsRemoved == 0);
                    if (existAccountHasId != null && existsAccount == null)
                    {
                        existAccountHasId.UserPrincipalName = item.UserPrincipalName;
                        existAccountHasId.DisplayName = item.DisplayName;
                        existAccountHasId.FirstName = item.GivenName;
                        existAccountHasId.LastName = item.SurName;
                        item.UserId = existAccountHasId.UserId;
                        AccountDao.UpdateAsync(existAccountHasId);
                    }
                    else if (existAccountHasId == null && existsAccount == null)
                    {
                        throw new Exception($"Can't find user in opus [{item.Id}]");
                    }
                    if (existsAccount != null)
                    {
                        item.UserId = existsAccount.UserId;
                    }
                }
            });
            var informationOwnerUserIds = informationOwners.Select(item => item.UserId).ToHashSet();
            var recordOwnerUserIds = recordOwners.Select(item => item.UserId).ToHashSet();
            var existsInformationOwnerIntIds = (await AccountDao.FindListAsync(item => informationOwnerUserIds.Contains(item.UserId) && item.IsRemoved == 0)).Select(item => item.Id).ToList();
            var existsRecordOwnerIntIds = (await AccountDao.FindListAsync(item => recordOwnerUserIds.Contains(item.UserId) && item.IsRemoved == 0)).Select(item => item.Id).ToList();
            FSConnectionOwnerDao.AddOwners(connectionId, existsInformationOwnerIntIds, existsRecordOwnerIntIds);
        }

        private async Task<int> SyncOwnerUsersAsync(List<ToUserInfo> owners)
        {
            try
            {
                if (owners == null || owners.Count == 0) return 1;
                var uniqueOwners = owners.GroupBy(u => !string.IsNullOrEmpty(u.Id) ? u.Id.ToLower() : u.UserPrincipalName?.ToLower())
                                        .Where(g => g.Key != null)
                                        .Select(g => g.First())
                                        .ToList();
                await UserServices.SyncUsersAsync(TenantLocalValue.LogonGroupId, uniqueOwners);
                return 1;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while syncing owner users from AOS.");
                return 0;
            }
        }
        private ToUserInfo ConvertToAccountInfoDto(int userId, Dictionary<int, RMAccount> ownerLookup)
        {
            var dto = new ToUserInfo();
            if (ownerLookup.TryGetValue(userId, out var account))
            {
                dto.UserId = account.UserId;
                dto.DisplayName = account.DisplayName;
                dto.UserPrincipalName = account.UserPrincipalName;
                dto.Id = account.AADId;
            }
            return dto;
        }
        #endregion

        #region JPMC
        public async Task<List<string>> QueryAllConnGroupNameRelatedJobAsync(Guid connectionId)
        {
            try
            {
                return await FSConnectionRelatedJobInfoDao.GetAllConnGroupNameByConnectionIDAsync(connectionId);
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while querying all connection group by related job. ConnectionId: {connectionId}, Error: {ex}");
                throw;
            }
        }

        public async Task<List<string>> QueryAllConnPathRelatedJobAsync(Guid connectionId)
        {
            try
            {
                return await FSConnectionRelatedJobInfoDao.GetAllConnPathByConnectionIdAsync(connectionId);
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while querying all connection path by related job. Error: {ex}");
                throw;
            }
        }

        public async Task<FSConnectionMonitorResultData> QueryConnectionMonitorByPagerAsync(FSConnectionMonitorQueryPager pager)
        {
            try
            {
                logger.Info($"Start querying connection monitor. PageIndex={pager.PageIndex}, PageSize={pager.PageSize}");

                pager.Filters ??= new List<FSConnectionMonitorFilter>();

                if (pager.ConnectionId != Guid.Empty)
                {
                    pager.Filters.Insert(0, new FSConnectionMonitorFilter
                    {
                        ColumnName = nameof(FSConnectionRelatedJobInfo.ConnectionId),
                        ColumnValues = new() { pager.ConnectionId.ToString() }
                    });
                }

                if (!string.IsNullOrWhiteSpace(pager.SearchKey))
                {
                    pager.Filters.Insert(0, new FSConnectionMonitorFilter
                    {
                        ColumnName = nameof(FSConnectionRelatedJobInfo.JobId),
                        ColumnValues = new() { pager.SearchKey }
                    });
                }

                var filterExpression = new FSConnectionRelatedJobFilterBuilder(pager.Filters).Build();

                if (!IsValidOrderColumn(pager.Order?.ColumnName))
                {
                    pager.Order = new()
                    {
                        ColumnName = nameof(FSConnectionRelatedJobInfo.StartTime),
                        IsDesc = true
                    };
                }

                var (totalCount, dataList) = await FSConnectionRelatedJobInfoDao.QueryConnectionMonitorPagerAsync(filterExpression, pager);

                var dtoList = await Task.WhenAll(dataList.Select(ConvertFromMonitorRecordToDto));

                return new FSConnectionMonitorResultData
                {
                    ConnectionMonitorList = dtoList.ToList(),
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while querying connection monitor by pager. Error: {ex}");
                throw;
            }
        }

        private bool IsValidOrderColumn(string columnName)
        {
            return typeof(FSConnectionRelatedJobInfo).GetProperties().Any(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsValidOrderColumn<T>(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            return typeof(T)
                .GetProperties()
                .Any(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        private string NormalizeOrderColumn<T>(string columnName)
        {
            return typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                ?.Name;
        }

        private async Task<FSConnectionRelatedJobInfoDto> ConvertFromMonitorRecordToDto(FSConnectionRelatedJobInfo record)
        {
            return new FSConnectionRelatedJobInfoDto
            {
                JobId = record.JobId,
                JobType = record.JobType,
                JobRunBy = I18NEntity.GetString(record.JobRunBy),
                Status = record.Status,
                Comment = record.Comment,
                ConnectionGroupName = record.ConnectionGroupName,
                Path = !string.IsNullOrWhiteSpace(record.ConnectionPath) && string.IsNullOrEmpty(record.FolderPath) ? record.ConnectionPath : record.FolderPath,
                StartTime = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(record.StartTime, true)).SimplifyFormatTime,
                EndTime = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(record.EndTime, true)).SimplifyFormatTime,
            };
        }
        #endregion
    }
}
