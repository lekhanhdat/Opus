using AvePoint.GCommon.Utility.I18N;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Public.Common.Requests;
using AvePoint.RA.Api.Web.Public.Common.Response;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("connections")]
    public class ConnectionsController : RAWebApiBase
    {
        private static readonly Regex UncPathRegex = new Regex(
            @"^\\\\[^\\/:*?""<>|]+\\[^\\/:*?""<>|]+(\\[^\\/:*?""<>|]+)*$",
            RegexOptions.Compiled);

        private static readonly Regex InternalIdRegex = new Regex(
            @"^[A-Za-z0-9_. -]{1,256}$",
            RegexOptions.Compiled);

        private readonly RALogger logger = RALogger.GetInstance(typeof(ConnectionsController));

        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetConnections([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? groupId = null)
        {
            if (groupId.HasValue)
            {
                var group = await FSRegisterService.GetGroupAsync(groupId.Value);
                if (group == null)
                {
                    return this.NotFoundApi("Connection group not found.");
                }

                if (!await CanAccessConnectionGroupAsync(group))
                {
                    return this.ForbiddenApi("The connection group is not available in the current data center.");
                }

                return this.OkApi(await FSRegisterService.GetAllConnectionsByGroupIdAsync(groupId.Value));
            }

            return this.OkApi(await FSRegisterService.QueryConnectionByPagerAsync(new GetConnectionListParam
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            }));
        }

        [HttpGet("ungrouped")]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetUngroupedConnections()
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature()
                && !string.Equals(RMSSOHelper.CurrentDCName, MultiGeoDataCenterService.GetMainDC(), StringComparison.OrdinalIgnoreCase))
            {
                return this.BadRequestApi("GetUngroupedConnections is only allowed on Main DC when Multi-Geo is enabled.");
            }

            return this.OkApi(await FSRegisterService.LoadAllConnectionAsync(true));
        }

        [HttpGet("{id:guid}")]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetConnectionById(Guid id)
        {
            var connection = await FSRegisterService.GetConnectionByIdAsync(id);
            if (connection == null)
            {
                return this.NotFoundApi("Connection not found.");
            }

            if (!await CanAccessConnectionAsync(connection))
            {
                return this.ForbiddenApi("The connection is not available in the current data center.");
            }

            return this.OkApi(connection);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConnection([FromBody] ConnectionRequest connectionDto)
        {
            var requestParam = connectionDto?.ToContract();
            var result = await RouteMultiGeoApiActionAsync(requestParam, MultiGeoOperationType.SaveConnection,
                CreateConnectionInternalAsync,
                _ => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
            return this.FromReturnMessage(result);
        }

        [HttpPost("batch")]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> CreateConnections([FromBody] List<ConnectionRequest> connectionDtos)
        {
            if (connectionDtos == null || connectionDtos.Count == 0)
            {
                return this.BadRequestApi("Invalid connection payload for creation.");
            }

            var results = new List<RAReturnMessage>(connectionDtos.Count);
            foreach (var connectionDto in connectionDtos)
            {
                results.Add(await CreateConnectionInternalAsync(connectionDto?.ToContract()));
            }

            await TrySyncCommonDataAfterBatchCreateAsync(results, MultiGeoOperationType.SaveConnections);
            return this.OkApi(null);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConnection([FromBody] ConnectionRequest connectionDto)
        {
            var requestParam = connectionDto?.ToContract();
            var validationResult = ValidateConnection(requestParam, isCreate: false);
            if (validationResult != null)
            {
                return this.FromReturnMessage(validationResult);
            }

            var result = await RouteMultiGeoApiActionAsync(requestParam, MultiGeoOperationType.SaveConnection,
                request => SaveConnectionAsync(request, isCreate: false),
                _ => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
            return this.FromReturnMessage(result);
        }

        [HttpPost("batch-delete")]
        public async Task<IActionResult> DeleteConnections([FromBody] List<Guid> connectionIds)
        {
            var result = await RouteMultiGeoApiActionAsync(connectionIds, MultiGeoOperationType.DeleteConnection,
                request => FSRegisterService.DeleteConnectoinAsync(request),
                _ => -1);

            return this.OkApi(null);
        }

        private async Task<RAReturnMessage> CreateConnectionInternalAsync(ConnectionDto connectionDto)
        {
            var validationResult = ValidateConnection(connectionDto, isCreate: true);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await SaveConnectionAsync(connectionDto, isCreate: true);
        }

        private async Task<bool> CanAccessConnectionGroupAsync(ConnectionGroupDto connectionGroup)
        {
            if (connectionGroup == null)
            {
                return false;
            }
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return true;
            }

            var currentDC = RMSSOHelper.CurrentDCName;
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            if (string.Equals(currentDC, mainDC, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(connectionGroup.DCInternalName)
                && string.Equals(connectionGroup.DCInternalName, currentDC, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> CanAccessConnectionAsync(ConnectionDto connection)
        {
            if (connection == null)
            {
                return false;
            }
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return true;
            }

            var currentDC = RMSSOHelper.CurrentDCName;
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            if (string.Equals(currentDC, mainDC, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (connection.GroupId == Guid.Empty)
            {
                return false;
            }

            var group = await FSRegisterService.GetGroupAsync(connection.GroupId);
            return await CanAccessConnectionGroupAsync(group);
        }

        private async Task<RAReturnMessage> SaveConnectionAsync(ConnectionDto connectionDto, bool isCreate)
        {
            var result = new RAReturnMessage();
            try
            {
                var resultCode = isCreate
                    ? await FSRegisterService.CreateConnectoinAsync(connectionDto)
                    : await FSRegisterService.UpdateConnectoinAsync(connectionDto);

                if (resultCode == 1)
                {
                    result.MessageType = RAMessageType.Successful;
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    if (resultCode == -2)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_UNCPath_Exist");
                    }
                    if (resultCode == -3)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_JPMCConnectionId_Exist");
                    }
                    if (resultCode == -4)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_SameConnectionNameErrorMessage");
                    }
                    if (resultCode == -5)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_JS_Common_Msg_CannotExceed255");
                    }
                    if (resultCode == -6)
                    {
                        result.ErrorMessage = "JPMC Id should not be null";
                    }
                    if (resultCode == -7)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                    }
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString(ex.Message);
            }

            return result;
        }

        private RAReturnMessage ValidateConnection(ConnectionDto connectionDto, bool isCreate)
        {
            if (connectionDto == null)
            {
                return CreateFailedResult(isCreate ? "Invalid connection payload for creation." : "Invalid connection payload for update, the Id must be filled in the params");
            }
            if (isCreate ? connectionDto.Id != Guid.Empty : connectionDto.Id == Guid.Empty)
            {
                return CreateFailedResult(isCreate ? "Invalid connection payload for creation." : "Invalid connection payload for update, the Id must be filled in the params");
            }
            if (isCreate && string.IsNullOrEmpty(connectionDto.JPMCConnectionId))
            {
                return CreateFailedResult("Invalid connection payload for creatio, the CustomId must be filled in the params");
            }
            if (!string.IsNullOrWhiteSpace(connectionDto.JPMCConnectionId) && !IsValidInternalId(connectionDto.JPMCConnectionId))
            {
                return CreateFailedResult("CustomId can contain only letters, numbers, spaces, periods, hyphens, and underscores, and must be 256 characters or fewer.");
            }
            if (string.IsNullOrWhiteSpace(connectionDto.Name))
            {
                return CreateFailedResult(isCreate ? "Invalid connection payload for creation, the Name must be filled in the params" : "Invalid connection payload for update, the Name must be filled in the params");
            }

            var enableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(AvePoint.RA.Contract.Common.KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if (!IsValidUncPath(connectionDto.UNCPath))
            {
                return CreateFailedResult(enableJPMCFileSystemFeature
                    ? I18NEntity.GetString("RM_FS_Register_PathInputValidateMessage")
                    : I18NEntity.GetString("RM_FS_Register_UNCPathInputValidateMessage"));
            }
            if (enableJPMCFileSystemFeature)
            {
                if (connectionDto.RecordOwners != null && connectionDto.RecordOwners.Count > 0 && !HasValidOwners(connectionDto.RecordOwners))
                {
                    return CreateFailedResult("RecordOwners is invalid, or it is required because JPMC file system feature is enabled.");
                }
                if (!HasValidOwners(connectionDto.InformationOwners))
                {
                    return CreateFailedResult("Information is invalid, or it is required because JPMC file system feature is enabled");
                }
            }

            return null;
        }

        private static bool IsValidUncPath(string uncPath)
        {
            var clearPath = new string(uncPath.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Format).ToArray());
            return !string.IsNullOrWhiteSpace(uncPath) && UncPathRegex.IsMatch(clearPath.Trim());
        }

        private bool HasValidOwners(List<AvePoint.RA.Contract.RMWeb.ReportCenter.ToUserInfo> owners)
        {
            if (owners == null || owners.Count == 0)
            {
                return false;
            }

            foreach (var owner in owners)
            {
                if (string.IsNullOrWhiteSpace(owner?.Id) && string.IsNullOrWhiteSpace(owner?.UserPrincipalName))
                {
                    return false;
                }

                var aadAccount = ResolveAADAccount(owner);
                if (aadAccount != null)
                {
                    if (string.IsNullOrEmpty(owner.Id))
                    {
                        owner.Id = aadAccount.Id;
                    }
                    owner.UserPrincipalName = aadAccount.UserPrincipalName ?? aadAccount.Mail ?? aadAccount.DisplayName;
                    owner.DisplayName = aadAccount.DisplayName;
                    owner.Email = aadAccount.Mail;
                    owner.InviteType = aadAccount.InviteType;
                }
            }

            return true;
        }

        private AADAccount ResolveAADAccount(ToUserInfo owner)
        {
            var tenantId = global::AvePoint.RA.Contract.Tenant.TenantLocalValue.LogonGroupId;
            var group = TryGetGroup(tenantId, owner);
            if (group != null)
            {
                group.InviteType = AccountType.Group;
                return group;
            }

            return TryGetUser(tenantId, owner);
        }

        private AADAccount TryGetUser(string tenantId, ToUserInfo owner)
        {
            try
            {
                return AccountWrapperService.GetAccountByIdOrUPN(tenantId, owner.Id, owner.UserPrincipalName);
            }
            catch
            {
                return null;
            }
        }

        private AADAccount TryGetGroup(string tenantId, ToUserInfo owner)
        {
            try
            {
                if(string.IsNullOrEmpty(owner.Id) && string.IsNullOrEmpty(owner.UserPrincipalName))
                {
                    logger.Warn($"Owner Id and UserPrincipalName are both null or empty");
                    return null;
                }
                AADAccount group = AccountWrapperService.GetGroupsByIdOrGroupEmail(tenantId, owner.Id, owner.UserPrincipalName);
                return group;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsValidInternalId(string internalId)
        {
            return !string.IsNullOrWhiteSpace(internalId) && InternalIdRegex.IsMatch(internalId.Trim());
        }

        private async Task TrySyncCommonDataAfterBatchCreateAsync(List<RAReturnMessage> results, MultiGeoOperationType operationType)
        {
            if (results == null || !results.Any(item => item?.MessageType == RAMessageType.Successful))
            {
                return;
            }
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return;
            }
            if (!string.Equals(RMSSOHelper.CurrentDCName, MultiGeoDataCenterService.GetMainDC(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            MultiGeoReplicaFailureLogWriter.WriteForJob(global::AvePoint.RA.Contract.Tenant.TenantLocalValue.LogonGroupId, operationType.ToString());
            await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(JobRunBy.Control);
        }

        private static RAReturnMessage CreateFailedResult(string errorMessage)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = errorMessage
            };
        }
    }
}

