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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Controllers.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.FileSystem
{
    [RMApiAuthorize(RMPermissionMasks.FSAdmin, RMDiscoveryFileSystemPermissionMask.AccessAll, PermissionJoinType.Any, PermissionJoinType.Any, preferred: false)]
    public class ConnectionRegisterApiController : BaseApiController
    {
        private static readonly Regex UncPathRegex = new Regex(
            @"^\\\\[^\\/:*?""<>|]+\\[^\\/:*?""<>|]+(\\[^\\/:*?""<>|]+)*$",
            RegexOptions.Compiled);
        private IRMFileSystemRegisterService _FSRegisterService;
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService(ref _FSRegisterService);
        private IRMFileSystemSettingsService _RMFileSystemSettingsService;
        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService(ref _RMFileSystemSettingsService);
        private IRMFileSystemBrowserService _RMFileSystemBrowserService;
        private IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService(ref _RMFileSystemBrowserService);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private IAccountWrapperService _AccountWrapperService;
        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService(ref _AccountWrapperService);

        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IFSConnectionGroupDao FSGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();

        private RALogger logger = RALogger.GetInstance(typeof(ConnectionRegisterApiController));

        [HttpGet]
        public async Task<string> GetAllGroups()
        {
            try
            {
                var result = await FSRegisterService.GetAllGroupsAsync();
                return JsonConvert.SerializeObject(result);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get all groups. Error: {e}");
                throw;
            }
        }

        [HttpPost]
        public async Task<string> QueryConnectionsPager([FromBody] GetConnectionListParam param)
        {
            try
            {
                return JsonConvert.SerializeObject(await FSRegisterService.QueryConnectionByPagerAsync(param));
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while query connections pager. Error: {e}");
                throw;
            }
        }

        [HttpGet]
        public async Task<string> GetAllNoGroupConnections()
        {
            return JsonConvert.SerializeObject(await FSRegisterService.LoadAllConnectionAsync(true));
        }

        [HttpPost]
        public async Task<string> GetAllNoGroupConnections4JPMC([FromBody] GetConnectionListParam param)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.LoadAllNoGroupConnectionAsync(param));
        }

        [HttpGet]
        public async Task<string> GetGroupById(Guid id)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.GetGroupByIdAsync(id));
        }

        [HttpGet]
        public async Task<string> GetGroup(Guid id)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.GetGroupAsync(id));
        }

        [HttpGet]
        public async Task<string> GetConnectionById(Guid id)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.GetConnectionByIdAsync(id));
        }
        [HttpGet]
        public async Task<string> GetConnectionByGroupId(Guid id)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.GetAllConnectionsByGroupIdAsync(id));
        }

        [HttpPost]
        public async Task<string> SaveConnectionGroup([FromBody] ConnectionGroupDto connectionGroupDto)
        {
            var validationResult = ValidateSaveConnectionGroupRequest(connectionGroupDto);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await RouteMultiGeoApiActionAsync(
                connectionGroupDto,
                MultiGeoOperationType.SaveConnectionGroup,
                async request =>
                {
                    RAReturnMessage result = new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful
                    };
                    try
                    {
                        if (request.Id == Guid.Empty)
                        {
                            await FSRegisterService.CreateConnectionGroupAsync(connectionGroupDto);
                            connectionGroupDto.MultiGeoOperation = MultiGeoOperation.MultiGeoCreateFSGroup;
                        }
                        else
                        {
                            await FSRegisterService.UpdateConnectionGroupAsync(request);
                            connectionGroupDto.MultiGeoOperation = MultiGeoOperation.MultiGeoEditFSGroup;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while saving connection group. Group Name: {request?.Name}. Error: {ex}");

                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = ex.Message;
                    }

                    return JsonConvert.SerializeObject(result);
                },
                _ => JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                }));
        }

        private string ValidateSaveConnectionGroupRequest(ConnectionGroupDto connectionGroupDto)
        {
            if (connectionGroupDto == null || string.IsNullOrEmpty(connectionGroupDto.Name))
            {
                return JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed
                });
            }

            return null;
        }

        [HttpPost]
        public async Task<string> SaveConnection([FromBody] ConnectionDto connectionDto)
        {
            var validationResult = ValidateSaveConnectionRequest(connectionDto);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await RouteMultiGeoApiActionAsync(
                connectionDto,
                MultiGeoOperationType.SaveConnection,
                async request =>
                {
                    RAReturnMessage result = new RAReturnMessage();
                    var resultCode = 0;
                    var enableJPMCFileSystemFeature = RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
                    if (request.Id == Guid.Empty)
                    {
                        resultCode = await FSRegisterService.CreateConnectoinAsync(request);
                        request.MultiGeoOperation = MultiGeoOperation.MultiGeoCreateFSConnection;
                    }
                    else
                    {
                        resultCode = await FSRegisterService.UpdateConnectoinAsync(request);
                        request.MultiGeoOperation = MultiGeoOperation.MultiGeoEditFSConnection;
                    }

                    if (resultCode == 1)
                    {
                        result.MessageType = RAMessageType.Successful;
                    }
                    else if (resultCode == -2)
                    {
                        result.MessageType = RAMessageType.Failed;
                        if (enableJPMCFileSystemFeature)
                        {
                            result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_Path_Exist");
                        }
                        else
                        {
                            result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_UNCPath_Exist");
                        }
                    }
                    else if (resultCode == -3)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_JPMCConnectionId_Exist");
                    }
                    else if (resultCode == -4) // Duplicate connection name
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_SameConnectionNameErrorMessage");
                    }
                    else if (resultCode == -5) // Exceed 255 length
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_JS_Common_Msg_CannotExceed255");
                    }
                    else if (resultCode == -6)
                    {
                        result.ErrorMessage = "JPMC Id should not be null";
                    }
                    else if (resultCode == -7)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                    }

                    return JsonConvert.SerializeObject(result);
                },
                _ => JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                }));

        }

        private string ValidateSaveConnectionRequest(ConnectionDto connectionDto)
        {
            if (connectionDto == null || string.IsNullOrEmpty(connectionDto.UNCPath) || string.IsNullOrEmpty(connectionDto.Name))
            {
                return JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed
                });
            }

            var validationResult = FSRegisterService.ValidateConnection(connectionDto, isCreate: connectionDto.Id == Guid.Empty);
            return validationResult == null ? null : JsonConvert.SerializeObject(validationResult);
        }

        [HttpGet]
        public string GetAllAgents()
        {
            return JsonConvert.SerializeObject(FSRegisterService.GetAllAgent());
        }

        [HttpPost]
        public async Task<string> ValidationConnection([FromBody] ConnectionDto connectionDto)
        {
            var result = new RAReturnMessage();
            if (!await RMFileSystemBrowserService.CheckHasAvailableAgentAsync(connectionDto.GroupId))
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_SS_FSNoAvailableAgent");

            }
            result.MessageType = await RMFileSystemBrowserService.ValidationTestConnectionAsync(connectionDto) ? RAMessageType.Successful : RAMessageType.Failed;
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<List<Guid>> ValidateConnections([FromBody] ValidateConnectionParam param)
        {
            if (param.AccessConnectionType == AccessConnectionType.All)
            {
                if (!await RMFileSystemBrowserService.CheckHasAvailableAgentAsync())
                {
                    return new List<Guid>();
                }
            }
            else if (param.AccessConnectionType == AccessConnectionType.Specify)
            {
                if (!await RMFileSystemBrowserService.CheckHasAvailableAgentAsync(param.AgentIds))
                {
                    return new List<Guid>();
                }
            }

            return await RMFileSystemBrowserService.ValidateTestConnectionsAsync(param);
        }

        [HttpPost]
        public async Task<string> CorrelateConnection([FromBody] CorrelateConnectionDto dto)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.CorrelateConnectionGroupAsync(dto));
        }

        [HttpPost]
        public async Task<string> DeleteConnection([FromBody] List<Guid> connectionIds)
        {
            return await RouteMultiGeoApiActionAsync(
                connectionIds,
                MultiGeoOperationType.DeleteConnection,
                async request => JsonConvert.SerializeObject(await FSRegisterService.DeleteConnectoinAsync(request)),
                _ => JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                }));
        }

        [HttpPost]
        public async Task<string> DeleteGroup([FromBody] List<Guid> groupsIds)
        {
            var normalizedGroupIds = groupsIds ?? new List<Guid>();

            return await RouteMultiGeoApiActionAsync(
                normalizedGroupIds,
                MultiGeoOperationType.DeleteGroup,
                async request =>
                {
                    try
                    {
                        return JsonConvert.SerializeObject(await FSRegisterService.DeleteGroupConnectoinAsync(request));
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Delete groups failed. GroupIds: [{string.Join(", ", request ?? new List<Guid>())}]. Error: {ex}");

                        return JsonConvert.SerializeObject(new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                        });
                    }
                },
                _ => JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                }));
        }

        [HttpPost]
        public string CheckConnectionSettings([FromBody] List<Guid> connectionIds)
        {
            return JsonConvert.SerializeObject(RMFileSystemSettingsService.CheckFSNodeSettingExist(connectionIds));
        }



        [HttpGet]
        public Task<string> GetJobMessage(string subJobId)
        {
            return RMFileSystemSettingsService.GetJobMessageAsync(subJobId);
        }

        [HttpPost]
        public string GetRecords([FromBody] List<Guid> ids)
        {
            return JsonConvert.SerializeObject(ExplorerService.GetFileSystemObjectByGuids(ids));
        }

        [HttpGet]
        public async Task<List<string>> GetAllConnectionGroupNames()
        {
            return await FSRegisterService.LoadAllConnectionGroupNamesAsync();
        }
    }
}