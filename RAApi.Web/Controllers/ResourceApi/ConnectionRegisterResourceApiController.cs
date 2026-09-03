using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model.QueryRequest;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/ConnectionRegisterApi/[action]")]
    public class ConnectionRegisterResourceApiController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ConnectionRegisterResourceApiController));
        private IRMFileSystemRegisterService _FSRegisterService;
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService(ref _FSRegisterService);

        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private IRMFileSystemBrowserService _RMFileSystemBrowserService;
        private IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService(ref _RMFileSystemBrowserService);
        private IFSConnectionDao _fsConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        public IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpPost]
        public async Task<string> SaveConnectionGroup([FromBody] ConnectionGroupDto connectionGroupDto)
        {
            ValidateReplicaConnectionGroupRequest(connectionGroupDto);

            RAReturnMessage result = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            await FSRegisterService.UpdateConnectionGroupAsync(connectionGroupDto);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveConnectionGroups([FromBody] List<ConnectionGroupDto> connectionGroupDtos)
        {
            List<RAReturnMessage> results = new List<RAReturnMessage>();

            foreach(var connectionGroup in connectionGroupDtos)
            {
                try
                {
                    ValidateReplicaConnectionGroupRequest(connectionGroup);

                    await FSRegisterService.UpdateConnectionGroupAsync(connectionGroup);
                    results.Add(new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful
                    });
                }
                catch(Exception ex)
                {
                    Logger.Error($"Failed to save connection group {connectionGroup.Id}: {ex.Message}", ex);
                    results.Add(new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Failed to save connection group {connectionGroup.Id}: {ex.Message}"
                    });
                }
            }
            return JsonConvert.SerializeObject(results);
        }

        [HttpPost]
        public async Task<string> DeleteGroup([FromBody] List<Guid> groupsIds)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.DeleteGroupConnectoinAsync(groupsIds));
        }


        [HttpPost]
        public async Task<string> SaveConnection([FromBody] ConnectionDto connectionDto)
        {
            ValidateReplicaConnectionRequest(connectionDto);

            RAReturnMessage result = new RAReturnMessage();
            var resultCode = 0;
            var enableJPMCFileSystemFeature = await RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false);
            if (connectionDto == null || string.IsNullOrEmpty(connectionDto.UNCPath) || string.IsNullOrEmpty(connectionDto.Name)) // || !mRMFileSystemBrowserService.ValidationTestConnection(connectionDto)
            {
                result.MessageType = RAMessageType.Failed;
            }
            else
            {
                result = FSRegisterService.ValidateConnection(connectionDto, isCreate: false);
                if (result != null)
                {
                    return JsonConvert.SerializeObject(result);
                }

                result = new RAReturnMessage();
                resultCode = await FSRegisterService.UpdateConnectoinAsync(connectionDto);
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
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public async Task<string> SaveConnections([FromBody] List<ConnectionDto> connectionDtos)
        {
            if (connectionDtos == null || connectionDtos.Count == 0)
            {
                return JsonConvert.SerializeObject(new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection payload for creation."
                });
            }

            var results = new List<RAReturnMessage>(connectionDtos.Count);
            var enableJPMCFileSystemFeature = await RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false);
            foreach(var connectionDto in connectionDtos)
            {
                ValidateReplicaConnectionRequest(connectionDto);

                RAReturnMessage result = new RAReturnMessage();
                var resultCode = 0;
                if (connectionDto == null || string.IsNullOrEmpty(connectionDto.UNCPath) || string.IsNullOrEmpty(connectionDto.Name)) // || !mRMFileSystemBrowserService.ValidationTestConnection(connectionDto)
                {
                    result.MessageType = RAMessageType.Failed;
                }
                else
                {
                    result = FSRegisterService.ValidateConnection(connectionDto, isCreate: false);
                    if (result != null)
                    {
                        results.Add(result);
                        continue;
                    }

                    result = new RAReturnMessage();
                    resultCode = await FSRegisterService.UpdateConnectoinAsync(connectionDto);
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
                    results.Add(result);
                }
            }
            return JsonConvert.SerializeObject(results);
        }

        [HttpPost]
        public async Task<string> DeleteConnection([FromBody] List<Guid> connectionIds)
        {
            return JsonConvert.SerializeObject(await FSRegisterService.DeleteConnectoinAsync(connectionIds));
        }

        [HttpPost]
        public async Task<List<Guid>> ValidateConnections([FromBody] ValidateConnectionParam param)
        {
            if (param == null)
            {
                return new List<Guid>();
            }

            param.IsPublicApiRole = true;
            param.TargetDCs = new List<string>();
            return await RMFileSystemBrowserService.ValidateTestConnectionsAsync(param);
        }
        [HttpPost]
        public async Task<bool> UpdateLastSyncTimeFSConnection([FromBody] UpdateLastSyncTimeRequest request)
        {
            var mainDC = _multiGeoDataCenterService.GetMainDC();
            Logger.Info($"Update last sync time connection in recource api, currentDCName: {RMSSOHelper.CurrentDCName}");
            if (string.Equals(mainDC, RMSSOHelper.CurrentDCName, StringComparison.OrdinalIgnoreCase))
            {
                return await RouteMultiGeoApiActionAsync(request,
                MultiGeoOperationType.UpdateLastSyncTimeFSConnection,
                request => _fsConnectionDao.UpdateLastSyncTimeAsync(request.ConnectionId, request.LastSyncTime),
                _ => false);
            }
            return await _fsConnectionDao.UpdateLastSyncTimeAsync(request.ConnectionId, request.LastSyncTime);
        }
        private static void ValidateReplicaConnectionGroupRequest(ConnectionGroupDto connectionGroupDto)
        {
            if (connectionGroupDto == null || connectionGroupDto.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Connection group id is required for replica requests.");
            }
        }

        private static void ValidateReplicaConnectionRequest(ConnectionDto connectionDto)
        {
            if (connectionDto == null || connectionDto.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Connection id is required for replica requests.");
            }
        }
    }
}
