
using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

public class RAMultiGeoClient
{
    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RAMultiGeoClient));
    private static string CurrentDCName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];
    
    private static readonly IReadOnlyDictionary<string, MultiGeoOperationDescriptor> s_operationRegistry =
        MultiGeoOperationRegistry.Create();
    private static IFSConnectionGroupDao _fsConnectionGroupDao;
    private static IFSConnectionGroupDao FSConnectionGroupDao
    {
        get
        {
            if (_fsConnectionGroupDao == null)
            {
                _fsConnectionGroupDao = new FSConnectionGroupDao();
            }
            return _fsConnectionGroupDao;
        }
    }
    private static IMultiGeoSettingService _multiGeoSettingService;
    private static IMultiGeoSettingService MultiGeoSettingService
    {
        get
        {
            if (_multiGeoSettingService == null)
            {
                _multiGeoSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
            }
            return _multiGeoSettingService;
        }
    }

    private static IMultiGeoDataCenterService _multiGeoDataCenterService;
    private static IMultiGeoDataCenterService MultiGeoDataCenterService
    {
        get
        {
            if (_multiGeoDataCenterService == null)
            {
                _multiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
            }
            return _multiGeoDataCenterService;
        }
    }
    public static Task<Dictionary<string, TResponse>> RouteApiActionAsync<TResponse>(MultiGeoOperationType operationType, IEnumerable<string> dCs, bool isNeedValidIP)
    {
        return ProcessMultiGeoApiActionAsync<TResponse>(operationType.ToString(), dCs, isNeedValidIP);
    }

    public static Task<Dictionary<string, TResponse>> RouteApiActionAsync<TRequest, TResponse>(MultiGeoOperationType operationType,TRequest request, IEnumerable<string> dCs)
    {
        return ProcessMultiGeoApiActionAsync<TRequest, TResponse>(operationType.ToString(), request, dCs);
    }

    public static Task<Dictionary<string, TResponse>> RouteMainDCApiActionAsync<TRequest, TResponse>(MultiGeoOperationType operationType, TRequest request)
    {
        return ProcessMultiGeoMainDCApiActionAsync<TRequest, TResponse>(operationType.ToString(), request);
    }

    public static async Task<TResponse> SeperateRouteToDataCenterByPartitionKeysAsync<TRequest, TResponse>(string[] partitionKeys, MultiGeoOperationType operationType, TRequest request, Func<TRequest, Dictionary<string, IEnumerable<string>>, Dictionary<string, TRequest>> funcSeparateRequest, Func<TRequest, Task<TResponse>> mainFunc, Func<IEnumerable<TResponse>, TResponse> summaryResponeFunc)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (routeInfo.IsEnableMultiGeoFeature)
        {
            var groupDataCenters = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionIdsAsync(partitionKeys);
            var separatedRequest = funcSeparateRequest(request, groupDataCenters);
            var operationDescriptor = ResolveOperationDescriptor(operationType.ToString());
            List<TResponse> responses = new List<TResponse>();
            foreach (var sendRequest in separatedRequest)
            {
                try
                {
                    if (!string.IsNullOrEmpty(sendRequest.Key))
                    {
                        if(!(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, sendRequest.Key)))
                        {
                            s_logger.Warn($"The login IP is not allowed to access data center [{sendRequest.Key}]. Reject the request.");
                            responses.Add(GetDefaultObject<TResponse>());
                            continue;
                        }
                        s_logger.Info($"Start route to other DC: {sendRequest.Key}");
                        var tagertDC = routeInfo.RouteApis?.Where(api => string.Equals(api.DataCenter, sendRequest.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        responses.Add(await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, sendRequest.Value, operationDescriptor.ReplicaApiPath, tagertDC.ApiUrl));
                    }
                    else
                    {
                        s_logger.Info($"The connection group belongs to Main DC: {sendRequest.Key}");
                        responses.Add(await mainFunc(sendRequest.Value));
                    }
                }
                catch (Exception e)
                {
                    s_logger.Warn($"Failed to route multi-geo request to data center [{sendRequest.Key}]. Error: {e}");
                    responses.Add(default);
                    continue;
                }
            }
            return summaryResponeFunc(responses);
        }
        else
        {
            return await mainFunc(request);
        }
    }

    private static TResponse GetDefaultObject<TResponse>()
    {
        var type = typeof(TResponse);

        if (type == typeof(string))
        {
            return (TResponse)(object)string.Empty;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var emptyArray = Array.CreateInstance(elementType, 0);
            return (TResponse)(object)emptyArray;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments();

            var listType = typeof(List<>).MakeGenericType(genericArguments);

            var emptyList = Activator.CreateInstance(listType);
            return (TResponse)emptyList;
        }

        return default(TResponse);
    }

    public static async Task<TResponse> RouteToDataCenterByConnectionIdAsync<TRequest, TResponse>(string partitionKeyId, TRequest request, Func<TRequest, Task<TResponse>> mainFunc, MultiGeoOperationType operationType)
    {
        return await ProcessMultiGeoApiActionWithConnectionIdAsync(partitionKeyId, operationType.ToString(), request, mainFunc);
    }

    public static async Task<TResponse> RouteToDataCenterByConnectionIdAsync<TRequest, TResponse>(string partitionKeyId,
         MultiGeoOperationType operationType,
         TRequest request,
         Func<TRequest, Task<TResponse>> mainFunc,
         Func<MultiGeoErrorType, TResponse> createRejectedResponse)
    {
        return await ProcessMultiGeoApiActionWithConnectionIdAsync(partitionKeyId, operationType.ToString(), request, mainFunc, createRejectedResponse);
    }

    public static async Task<TResponse> RouteToDataCenterByConnectionIdAsync<TResponse>(string partitionKeyId, Func<Task<TResponse>> mainFunc, MultiGeoOperationType operationType)
    {
        return await ProcessMultiGeoApiActionWithConnectionIdAsync(partitionKeyId, operationType.ToString(), mainFunc);
    }

    public static async Task<TResponse> RouteToTagertDC<TRequest, TResponse>(string tagertDC, TRequest request, string operationType)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (routeInfo.IsEnableMultiGeoFeature)
        {
            if (!(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, tagertDC)))
            {
                s_logger.Warn($"The login IP is not allowed to access data center [{tagertDC}]. Reject the request.");
                return GetDefaultObject<TResponse>();
            }
            if (string.IsNullOrWhiteSpace(operationType))
            {
                throw new ArgumentException("Operation type is required.", nameof(operationType));
            }
            var operationDescriptor = ResolveOperationDescriptor(operationType);
            var target = routeInfo.RouteApis?.Where(api => string.Equals(api.DataCenter, tagertDC, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            try
            {
                return await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
            }
            catch (Exception e)
            {
                s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
                return default;
            }
        }
        else
        {
            throw new Exception("Multi-geo feature is not enabled.");
        }
    }

    public static async Task<TResponse> ProcessMultiGeoApiActionWithConnectionIdAsync<TRequest, TResponse>(
         string partitionKeyId,
         string operationType,
         TRequest request,
         Func<TRequest, Task<TResponse>> mainFunc)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (routeInfo.IsEnableMultiGeoFeature)
        {
            if (!Guid.TryParse(partitionKeyId, out var connectionId))
            {
                s_logger.Info($"Invalid partitionKeyId {partitionKeyId} for RouteToDataCenterAsync");
                return await mainFunc(request);
            }
            var tagertDC = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
            return await RouteToTargetDataCenterAsync(operationType, request, mainFunc, routeInfo, connectionId, tagertDC);
        }
        else
        {
            return await mainFunc(request);
        }
    }

    public static async Task<TResponse> ProcessMultiGeoApiActionWithConnectionIdAsync<TRequest, TResponse>(
         string partitionKeyId,
         string operationType,
         TRequest request,
         Func<TRequest, Task<TResponse>> mainFunc,
         Func<MultiGeoErrorType, TResponse> createRejectedResponse)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (routeInfo.IsEnableMultiGeoFeature)
        {
            if (!Guid.TryParse(partitionKeyId, out var connectionId))
            {
                s_logger.Info($"Invalid partitionKeyId {partitionKeyId} for RouteToDataCenterAsync");
                return await mainFunc(request);
            }
            var tagertDC = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
            return await RouteToTargetDataCenterAsync(operationType, request, mainFunc, routeInfo, connectionId, tagertDC, createRejectedResponse);
        }
        else
        {
            return await mainFunc(request);
        }
    }

    private static async Task<TResponse> RouteToTargetDataCenterAsync<TRequest, TResponse>(
        string operationType, TRequest request, Func<TRequest, Task<TResponse>> mainFunc,
        MultiGeoRouteInfo routeInfo, Guid connectionId, string tagertDC,
        Func<MultiGeoErrorType, TResponse> createRejectedResponse)
    {

        if (string.IsNullOrEmpty(tagertDC))
        {
            s_logger.Info($"The connection belongs to Main DC: {connectionId}");
            return await mainFunc(request);
        }
        if (!(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, tagertDC)))
        {
            s_logger.Warn($"The login IP is not allowed to access data center [{tagertDC}]. Reject the request.");
            return CreateRejectedResponse<TResponse>(
                operationType, routeInfo.MainDataCenter, routeInfo.MainApiUrl,
                MultiGeoErrorType.InValidIPRequestError, createRejectedResponse);
        }
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }
        var operationDescriptor = ResolveOperationDescriptor(operationType);
        var target = routeInfo.RouteApis?.Where(api => string.Equals(api.DataCenter, tagertDC, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        try
        {
            return await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
        }
        catch (Exception e)
        {
            s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            return default;
        }
    }

    private static async Task<TResponse> RouteToTargetDataCenterAsync<TRequest, TResponse>(string operationType, TRequest request, Func<TRequest, Task<TResponse>> mainFunc, MultiGeoRouteInfo routeInfo, Guid connectionId, string tagertDC)
    {

        if (string.IsNullOrEmpty(tagertDC))
        {
            s_logger.Info($"The connection belongs to Main DC: {connectionId}");
            return await mainFunc(request);
        }
        if (!(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, tagertDC)))
        {
            s_logger.Warn($"The login IP is not allowed to access data center [{tagertDC}]. Reject the request.");
            return GetDefaultObject<TResponse>();
        }
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }
        var operationDescriptor = ResolveOperationDescriptor(operationType);
        var target = routeInfo.RouteApis?.Where(api => string.Equals(api.DataCenter, tagertDC, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        try
        {
            return await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
        }
        catch (Exception e)
        {
            s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            return default;
        }
    }

    private static async Task<TResponse> RouteToTargetDataCenterAsync<TResponse>(string operationType, Func<Task<TResponse>> mainFunc, MultiGeoRouteInfo routeInfo, Guid connectionId, string tagertDC)
    {
        if (string.IsNullOrEmpty(tagertDC))
        {
            s_logger.Info($"The connection belongs to Main DC: {connectionId}");
            return await mainFunc();
        }
        if (!(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, tagertDC)))
        {
            s_logger.Warn($"The login IP is not allowed to access data center [{tagertDC}]. Reject the request.");
            return GetDefaultObject<TResponse>();
        }
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }
        var operationDescriptor = ResolveOperationDescriptor(operationType);
        var target = routeInfo.RouteApis?.Where(api => string.Equals(api.DataCenter, tagertDC, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        try
        {
            return await PostToApiRoleAsync<TResponse>(operationDescriptor.ReplicaApiPath, operationDescriptor.ReplicaApiPath, target.ApiUrl);
        }
        catch (Exception e)
        {
            s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            return default;
        }
    }

    public static async Task<TResponse> ProcessMultiGeoApiActionWithConnectionIdAsync<TResponse>(
        string partitionKeyId,
        string operationType,
        Func<Task<TResponse>> mainFunc)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (routeInfo.IsEnableMultiGeoFeature)
        {
            if (!Guid.TryParse(partitionKeyId, out var connectionId))
            {
                s_logger.Info($"Invalid partitionKeyId {partitionKeyId} for RouteToDataCenterAsync");
                return await mainFunc();
            }
            var tagertDC = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
            return await RouteToTargetDataCenterAsync(operationType, mainFunc, routeInfo, connectionId, tagertDC);
        }
        else
        {
            return await mainFunc();
        }
    }

    public static Task<Dictionary<string, TResponse>> RouteApiActionWithRetryAsync<TRequest, TResponse>(MultiGeoOperationType operationType, TRequest request, IEnumerable<string> dCs)
    {
        return ProcessMultiGeoApiActionWithRetryAsync<TRequest, TResponse>(operationType.ToString(), request, dCs);
    }

    public static async Task<Dictionary<string, TResponse>> ProcessMultiGeoApiActionAsync<TResponse>(
     string operationType,
     IEnumerable<string> dCs,
     bool isNeedValidIP)
    {
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }

        var operationDescriptor = ResolveOperationDescriptor(operationType);

        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        var currentSendRequestDCRouteInfo = routeInfo.RouteApis?.Where(api => dCs.Contains(api.DataCenter, StringComparer.OrdinalIgnoreCase)).ToList();
        Dictionary<string, TResponse> result = new Dictionary<string, TResponse>();
        TResponse response;
        foreach(var target in currentSendRequestDCRouteInfo)
        {
            try
            {
                if(isNeedValidIP && !(await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, target.DataCenter)))
                {
                    s_logger.Warn($"The login IP is not allowed to access data center [{target.DataCenter}]. Reject the request.");
                    result[target.DataCenter] = GetDefaultObject<TResponse>();
                    continue;
                }
                response = await PostToApiRoleAsync<TResponse>(operationDescriptor.ReplicaApiPath, operationDescriptor.ReplicaApiPath, target.ApiUrl);
                result[target.DataCenter] = response;
            }
            catch (Exception e)
            {
                s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
                result[target.DataCenter] = default;
            }
        }
        return result;
    }

    public static async Task<Dictionary<string, TResponse>> ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
     string operationType,
     TRequest request,
     IEnumerable<string> dCs)
    {
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }

        var operationDescriptor = ResolveOperationDescriptor(operationType);

        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        var currentSendRequestDCRouteInfo = routeInfo.RouteApis?.Where(api => dCs.Contains(api.DataCenter, StringComparer.OrdinalIgnoreCase)).ToList();
        Dictionary<string, TResponse> result = new Dictionary<string, TResponse>();
        TResponse response;
        foreach (var target in currentSendRequestDCRouteInfo)
        {
            try
            {
                response = await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
                result[target.DataCenter] = response;
            }
            catch (Exception e)
            {
                result[target.DataCenter] = default;
                s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            }
        }
        return result;
    }
    public static async Task<Dictionary<string, TResponse>> ProcessMultiGeoMainDCApiActionAsync<TRequest, TResponse>(
     string operationType,
     TRequest request)
    {
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }
        var mainDC = await MultiGeoRouteInfoProvider.GetMainDataCenterAsync();
        var operationDescriptor = ResolveOperationDescriptor(operationType);

        var routeInfo = await MultiGeoRouteInfoProvider.CreateMainDCAsync();
        var currentSendRequestDCRouteInfo = routeInfo.RouteApis?.Where(api => string.Equals(mainDC,api.DataCenter, StringComparison.OrdinalIgnoreCase));
        Dictionary<string, TResponse> result = new Dictionary<string, TResponse>();
        TResponse response;
        foreach (var target in currentSendRequestDCRouteInfo)
        {
            try
            {
                response = await PostToApiRoleAsync<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
                result[target.DataCenter] = response;
            }
            catch (Exception e)
            {
                result[target.DataCenter] = default;
                s_logger.Warn($"Failed to route multi-geo request [{operationDescriptor.ReplicaApiPath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            }
        }
        return result;
    }

    public static async Task<Dictionary<string, TResponse>> ProcessMultiGeoApiActionWithRetryAsync<TRequest, TResponse>(
        string operationType,
        TRequest request,
        IEnumerable<string> dCs)
    {
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }

        var operationDescriptor = ResolveOperationDescriptor(operationType);

        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        var currentSendRequestDCRouteInfo = routeInfo.RouteApis?.Where(api => dCs.Contains(api.DataCenter, StringComparer.OrdinalIgnoreCase)).ToList();
        Dictionary<string, TResponse> result = new Dictionary<string, TResponse>();
        TResponse response;
        foreach (var target in currentSendRequestDCRouteInfo)
        {
            response = await PostToApiRoleAsyncWithRetry<TRequest, TResponse>(operationDescriptor.ReplicaApiPath, request, operationDescriptor.ReplicaApiPath, target.ApiUrl);
            result[target.DataCenter] = response;
        }
        return result;
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> localAction)
    {
        return RouteApiActionAsync(requestBody, operationType.ToString(), localAction);
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<string, TResponse> createRejectedResponse)
    {
        return RouteApiActionAsync(requestBody, operationType.ToString(), localAction, createRejectedResponse);
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<TRequest, TResponse, Task> prepareReplicaRequest)
    {
        return RouteApiActionAsync(requestBody, operationType.ToString(), localAction, prepareReplicaRequest);
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationType operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<TRequest, TResponse, Task> prepareReplicaRequest,
        Func<string, TResponse> createRejectedResponse)
    {
        return RouteApiActionAsync(requestBody, operationType.ToString(), localAction, prepareReplicaRequest, createRejectedResponse);
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        string operationType,
        Func<TRequest, Task<TResponse>> localAction)
    {
        ArgumentNullException.ThrowIfNull(localAction);

        return ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
            requestBody,
            operationType,
            () => localAction(requestBody));
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        string operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<string, TResponse> createRejectedResponse)
    {
        ArgumentNullException.ThrowIfNull(localAction);

        return ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
            requestBody,
            operationType,
            () => localAction(requestBody),
            createRejectedResponse: createRejectedResponse);
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        string operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<TRequest, TResponse, Task> prepareReplicaRequest)
    {
        ArgumentNullException.ThrowIfNull(localAction);
        ArgumentNullException.ThrowIfNull(prepareReplicaRequest);

        return ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
            requestBody,
            operationType,
            () => localAction(requestBody),
            response => prepareReplicaRequest(requestBody, response));
    }

    public static Task<TResponse> RouteApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        string operationType,
        Func<TRequest, Task<TResponse>> localAction,
        Func<TRequest, TResponse, Task> prepareReplicaRequest,
        Func<string, TResponse> createRejectedResponse)
    {
        ArgumentNullException.ThrowIfNull(localAction);
        ArgumentNullException.ThrowIfNull(prepareReplicaRequest);

        return ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
            requestBody,
            operationType,
            () => localAction(requestBody),
            response => prepareReplicaRequest(requestBody, response),
            createRejectedResponse);
    }

    public static async Task<TResponse> PostToMainDcAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationType operationType)
    {
        var operationDescriptor = ResolveOperationDescriptor(operationType.ToString());
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();

        if (!routeInfo.IsRoute)
        {
            return default(TResponse);
        }

        if (!ShouldRouteToMainDataCenter(routeInfo.MainDataCenter))
        {
            return default(TResponse);
        }

        return await PostToApiRoleAsync<TRequest, TResponse>(
            operationDescriptor.ReplicaApiPath,
            requestBody,
            operationDescriptor.OperationType.ToString(),
            routeInfo.MainApiUrl);
    }

    public static async Task ReplicateToOtherDataCentersAsync<TRequest>(
        TRequest requestBody,
        MultiGeoOperationType operationType)
    {
        var operationDescriptor = ResolveOperationDescriptor(operationType.ToString());
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();

        if (!routeInfo.IsRoute || routeInfo.RouteApis?.Count == 0)
        {
            return;
        }

        await ReplicateToRouteApisAsync<TRequest, string>(
            operationDescriptor.ReplicaApiPath,
            operationDescriptor.OperationType.ToString(),
            requestBody,
            routeInfo.MainDataCenter,
            routeInfo.RouteApis);
    }

    public static async Task<bool> ShouldPostToMainDcAsync()
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        return routeInfo.IsRoute && ShouldRouteToMainDataCenter(routeInfo.MainDataCenter);
    }

    public static async Task<string> ValidateJobActionExecutionAsync(MultiGeoOperationType operationType)
    {
        var operationDescriptor = ResolveOperationDescriptor(operationType.ToString());
        if (!operationDescriptor.IsJobAction)
        {
            return null;
        }

        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (!routeInfo.IsRoute || !ShouldRouteToMainDataCenter(routeInfo.MainDataCenter))
        {
            return null;
        }

        return I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage");
    }

    public static async Task<TResponse> ProcessMultiGeoApiActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        string operationType,
        Func<Task<TResponse>> localAction,
        Func<TResponse, Task> prepareReplicaRequest = null,
        Func<string, TResponse> createRejectedResponse = null)
    {
        ArgumentNullException.ThrowIfNull(localAction);
        if (string.IsNullOrWhiteSpace(operationType))
        {
            throw new ArgumentException("Operation type is required.", nameof(operationType));
        }

        var operationDescriptor = ResolveOperationDescriptor(operationType);

        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();
        if (!routeInfo.IsRoute)
        {
            return await localAction();
        }

        if (operationDescriptor.IsJobAction)
        {
            return await ProcessJobActionAsync<TRequest, TResponse>(
                requestBody,
                operationDescriptor,
                routeInfo,
                createRejectedResponse,
                localAction);
        }

        TResponse response;

        if (ShouldRouteToMainDataCenter(routeInfo.MainDataCenter))
        {
            return CreateRejectedResponse<TResponse>(
                operationDescriptor.ReplicaApiPath,
                routeInfo.MainDataCenter,
                routeInfo.MainApiUrl,
                createRejectedResponse);
        }

        response = await localAction();
        if (!ShouldReplicateResponse(response, out var needRecordChangeLog, out var errorMessage, out var realErrorMessage))
        {
            await LogFailureAndTriggerSyncJobAsync(requestBody, operationType, routeInfo, operationDescriptor, needRecordChangeLog, errorMessage, realErrorMessage);
            return response;
        }

        if (prepareReplicaRequest != null && routeInfo.RouteApis?.Count > 0)
        {
            await prepareReplicaRequest(response);
        }

        await ReplicateToRouteApisAsync<TRequest, TResponse>(
            operationDescriptor.ReplicaApiPath,
            operationDescriptor.OperationType.ToString(),
            requestBody,
            routeInfo.MainDataCenter,
            routeInfo.RouteApis);

        return response;
    }

    private static bool ShouldReplicateResponse<TResponse>(TResponse response, out bool needRecordChangeLog, out string errorMessage, out string realErrorMessage)
    {
        needRecordChangeLog = false;
        errorMessage = string.Empty;
        realErrorMessage = string.Empty;
        if (response is RAReturnMessage returnMessage)
        {
            return returnMessage.MessageType == RAMessageType.Successful;
        }

        if (response is IEnumerable<RAReturnMessage> returnMessages)
        {
            needRecordChangeLog = returnMessages.Any(item => item != null && item.MessageType == RAMessageType.Successful);
            if (needRecordChangeLog)
            {
                errorMessage = "Have any data handle error in Main DC";
                realErrorMessage = string.Join("|", returnMessages.Where(item => item != null && item.MessageType != RAMessageType.Successful).Select(item => item.ErrorMessage));
            }
            return returnMessages.All(item => item != null && item.MessageType == RAMessageType.Successful);
        }

        if (response is string responseText && TryParseReturnMessage(responseText, out var parsedReturnMessage))
        {
            return parsedReturnMessage.MessageType == RAMessageType.Successful;
        }

        return true;
    }

    private static bool TryParseReturnMessage(string responseText, out RAReturnMessage returnMessage)
    {
        returnMessage = null;
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return false;
        }

        var trimmedResponse = responseText.Trim();
        if (!trimmedResponse.StartsWith("{", StringComparison.Ordinal)
            || trimmedResponse.IndexOf("\"MessageType\"", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return false;
        }

        try
        {
            returnMessage = JsonConvert.DeserializeObject<RAReturnMessage>(trimmedResponse);
            return returnMessage != null;
        }
        catch
        {
            return false;
        }
    }

    private static void LogRejectedRequest(string apiPath, string mainDC, string mainApi, string reason)
    {
        s_logger.Warn(
            $"Multi-geo request [{apiPath}] is rejected on non-main DC. Current DC: [{CurrentDCName}], Main DC: [{mainDC}], Main Api: [{mainApi}], Reason: [{reason}].");
    }

    private static async Task ReplicateToRouteApisAsync<TRequest, TResponse>(
        string relativePath,
        string operationType,
        TRequest requestBody,
        string mainDC,
        IReadOnlyCollection<MultiGeoApiTarget> routeApis)
    {
        if (routeApis == null || routeApis.Count == 0)
        {
            return;
        }

        var replicationTasks = routeApis.Select(target =>
            ReplicateToRouteApiAsync<TRequest, TResponse>(
                relativePath,
                operationType,
                requestBody,
                mainDC,
                target));

        await Task.WhenAll(replicationTasks);
    }

    private static async Task ReplicateToRouteApiAsync<TRequest, TResponse>(
        string relativePath,
        string operationType,
        TRequest requestBody,
        string mainDC,
        MultiGeoApiTarget target)
    {
        string responseBody = null;

        try
        {
            TenantLocalValue.MultiGeoIP = ClientRequestLocalValue.ClientIP;
            responseBody = await PostToApiRoleAsync<TRequest, string>(relativePath, requestBody, relativePath, target.ApiUrl);
            return;
        }
        catch (Exception e)
        {
            s_logger.Warn($"Failed to route multi-geo request [{relativePath}] to data center [{target.DataCenter}] with api [{target.ApiUrl}]. Error: {e}");
            MultiGeoReplicaFailureLogWriter.Write(
                tenantGroupId: TenantLocalValue.LogonGroupId,
                operationType: operationType,
                apiPath: relativePath,
                requestBody: requestBody,
                currentDataCenter: CurrentDCName,
                mainDataCenter: mainDC,
                targetDataCenter: target.DataCenter,
                targetApiUrl: target.ApiUrl,
                errorResponse: responseBody,
                failureReason: e.ToString());
        }
    }

    private static async Task<TResponse> PostToApiRoleAsync<TRequest, TResponse>(
        string relativePath,
        TRequest requestBody,
        string operationName,
        string apiUrl)
    {
        var client = MultiGeoHybridAgentClientFactory.Create(apiUrl);
        return await client.PostAsync<TRequest, TResponse>(relativePath, requestBody, operationName);
    }

    private static async Task<TResponse> PostToApiRoleAsync<TResponse>(
        string relativePath,
        string operationName,
        string apiUrl)
    {
        var client = MultiGeoHybridAgentClientFactory.Create(apiUrl);
        return await client.PostAsync<TResponse>(relativePath, operationName);
    }

    private static async Task<TResponse> PostToApiRoleAsyncWithRetry<TRequest, TResponse>(
        string relativePath,
        TRequest requestBody,
        string operationName,
        string apiUrl)
    {
        const int maxRetryCount = 3;

        for (var attempt = 1; attempt <= maxRetryCount; attempt++)
        {
            try
            {
                return await PostToApiRoleAsync<TRequest, TResponse>(relativePath, requestBody, operationName, apiUrl);
            }
            catch (TimeoutException ex)
            {
                if (attempt == maxRetryCount) throw;
                s_logger.Warn($"Timeout error, retry {attempt}/{maxRetryCount}. Error: {ex}");
                continue;
            }
            catch (HttpRequestException ex)
            {
                if (attempt == maxRetryCount) throw;
                s_logger.Warn($"Network error, retry {attempt}/{maxRetryCount}. Error: {ex}");
                continue;
            }
        }

        throw new TimeoutException($"Timeout calling multi-geo api [{relativePath}] on [{apiUrl}].");
    }

    private static bool ShouldRouteToMainDataCenter(string mainDC)
    {
        if (string.IsNullOrWhiteSpace(mainDC) || string.IsNullOrWhiteSpace(CurrentDCName))
        {
            return false;
        }

        return !CurrentDCName.Equals(mainDC, StringComparison.OrdinalIgnoreCase);
    }

    private static TResponse CreateRejectedResponse<TResponse>(
        string apiPath,
        string mainDC,
        string mainApi,
        MultiGeoErrorType errorType,
        Func<MultiGeoErrorType, TResponse> createRejectedResponse)
    {
        string errorMessage = GetErrorMessage(errorType);
        LogRejectedRequest(apiPath, mainDC, mainApi, errorMessage);
        return createRejectedResponse != null
            ? createRejectedResponse(errorType)
            : MultiGeoResponseHelper.CreateUnsupportedCommonDataResponse<TResponse>(errorMessage);
    }

    private static string GetErrorMessage(MultiGeoErrorType errorType) => errorType switch
    {
        MultiGeoErrorType.AccessCommonDataError or _ => I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
    };

private static TResponse CreateRejectedResponse<TResponse>(
        string apiPath,
        string mainDC,
        string mainApi,
        Func<string, TResponse> createRejectedResponse)
    {
        var errorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage");
        LogRejectedRequest(apiPath, mainDC, mainApi, errorMessage);
        return createRejectedResponse != null
            ? createRejectedResponse(errorMessage)
            : MultiGeoResponseHelper.CreateUnsupportedCommonDataResponse<TResponse>(errorMessage);
    }

    private static async Task<TResponse> ProcessJobActionAsync<TRequest, TResponse>(
        TRequest requestBody,
        MultiGeoOperationDescriptor operationDescriptor,
        MultiGeoRouteInfo routeInfo,
        Func<string, TResponse> createRejectedResponse,
        Func<Task<TResponse>> localAction)
    {
        if (ShouldRouteToMainDataCenter(routeInfo.MainDataCenter))
        {
            return CreateRejectedResponse<TResponse>(
                operationDescriptor.ReplicaApiPath,
                routeInfo.MainDataCenter,
                routeInfo.MainApiUrl,
                createRejectedResponse);
        }

        return await localAction();
    }


    private static MultiGeoOperationDescriptor ResolveOperationDescriptor(string operationType)
    {
        var normalizedOperationType = operationType.Trim();
        if (s_operationRegistry.TryGetValue(normalizedOperationType, out var descriptor))
        {
            if (string.IsNullOrWhiteSpace(descriptor.ReplicaApiPath))
            {
                throw new InvalidOperationException(
                    $"No multi-geo replica api path is registered for operation type [{descriptor.OperationType}].");
            }

            return descriptor;
        }

        throw new InvalidOperationException(
            $"No multi-geo replica api path is registered for operation type [{normalizedOperationType}].");
    }

    public static async Task<TResponse> PostCommonDataToMainDcAsync<TRequest, TResponse>(TRequest requestBody, MultiGeoOperationType mainDCOperationType, MultiGeoOperationType otherDCOperationType, Func<TRequest, Task<TResponse>> localAction)
    {
        var routeInfo = await MultiGeoRouteInfoProvider.CreateAsync();

        if (!routeInfo.IsRoute)
        {
            return default(TResponse);
        }

        TResponse response;

        if (!routeInfo.IsEnableMultiGeoFeature)
        {
            s_logger.Info("Multi-geo feature is not enabled. Executing local action.");
            response = await localAction(requestBody);
            return response;
        }

        if (!ShouldRouteToMainDataCenter(routeInfo.MainDataCenter))
        {
            var mainDCOperationDescriptor = ResolveOperationDescriptor(otherDCOperationType.ToString());
            s_logger.Info("The current data center is the main data center.");
            response = await localAction(requestBody);

            if (!ShouldReplicateResponse(response, out var needRecordChangeLog, out var errorMessage, out var realErrorMessage))
            {
                await LogFailureAndTriggerSyncJobAsync(requestBody, mainDCOperationType.ToString(), routeInfo, mainDCOperationDescriptor, needRecordChangeLog, errorMessage, realErrorMessage);
                return response;
            }

            s_logger.Info("Replicating common data to other data centers.");
            await ReplicateToRouteApisAsync<TRequest, TResponse>(
            mainDCOperationDescriptor.ReplicaApiPath,
            mainDCOperationDescriptor.OperationType.ToString(),
            requestBody,
            routeInfo.MainDataCenter,
            routeInfo.RouteApis);

            return response;
        }

        s_logger.Info("Route to Main DC to update common data.");

        var otherDCOperationDescriptor = ResolveOperationDescriptor(mainDCOperationType.ToString());

        return await PostToApiRoleAsync<TRequest, TResponse>(
            otherDCOperationDescriptor.ReplicaApiPath,
            requestBody,
            otherDCOperationDescriptor.OperationType.ToString(),
            routeInfo.MainApiUrl);
    }

    private static async Task LogFailureAndTriggerSyncJobAsync<TRequest>(TRequest requestBody, string operationType, MultiGeoRouteInfo routeInfo, MultiGeoOperationDescriptor operationDescriptor, bool needRecordChangeLog, string errorMessage, string realErrorMessage)
    {
        s_logger.Warn($"Some data cannot handle in main DC so we will not route to other DC");
        if (needRecordChangeLog)
        {
            s_logger.Warn("Recording multi-geo replica failure log and triggering sync job.");
            MultiGeoReplicaFailureLogWriter.Write(
            tenantGroupId: TenantLocalValue.LogonGroupId,
            operationType: operationType,
            apiPath: operationDescriptor.ReplicaApiPath,
            requestBody: requestBody,
            currentDataCenter: CurrentDCName,
            mainDataCenter: routeInfo.MainDataCenter,
            targetDataCenter: string.Empty,
            targetApiUrl: string.Empty,
            errorResponse: errorMessage,
            failureReason: realErrorMessage);
            await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(Contract.RMWeb.JobRunBy.Control);
        }
    }
}
