using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

public static class MultiGeoReplicaFailureLogWriter
{
    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(MultiGeoReplicaFailureLogWriter));
    private static readonly ISet<string> SkipChangeLogOperationWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        MultiGeoOperationType.UpdateAgentRuntimeStatus.ToString(),
    };
    private static IRMMultiGeoApiChangeLogDao MultiGeoApiChangeLogDao => PlatformWindsorManager.GetService<IRMMultiGeoApiChangeLogDao>();

    public static void Write<TRequest>(
        string tenantGroupId,
        string operationType,
        string apiPath,
        TRequest requestBody,
        string currentDataCenter,
        string mainDataCenter,
        string targetDataCenter,
        string targetApiUrl,
        string errorResponse,
        string failureReason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantGroupId) || ShouldSkipChangeLog(operationType))
            {
                return;
            }

            var entity = RMMultiGeoApiChangeLogEntity.Create(operationType?.Trim());
            entity.TenantGroupId = tenantGroupId;
            entity.CurrentDataCenter = currentDataCenter;
            entity.MainDataCenter = mainDataCenter;
            entity.TargetDataCenter = targetDataCenter;
            entity.TargetApiUrl = targetApiUrl;
            entity.ApiPath = apiPath;
            entity.TriggeredBy = TenantLocalValue.LogonUserEmail;
            entity.FailureReason = failureReason;
            entity.RequestBody = SerializeForTable(requestBody);
            entity.ErrorResponse = errorResponse;

            MultiGeoApiChangeLogDao.Add(tenantGroupId, entity);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to write multi-geo api change log. OperationType: [{operationType}], ApiPath: [{apiPath}]. Exception: {ex}");
        }
    }

    public static void WriteForJob(
        string tenantGroupId,
        string operationType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantGroupId) || ShouldSkipChangeLog(operationType))
            {
                return;
            }

            var entity = RMMultiGeoApiChangeLogEntity.Create(operationType?.Trim());
            entity.TenantGroupId = tenantGroupId;
            entity.TriggeredBy = TenantLocalValue.LogonUserEmail;

            MultiGeoApiChangeLogDao.Add(tenantGroupId, entity);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to write multi-geo api change log. OperationType: [{operationType}]. Exception: {ex}");
        }
    }

    public static void WriteForSync(string operationType, string tenantGroupId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantGroupId) || ShouldSkipChangeLog(operationType))
            {
                return;
            }
            var entity = RMMultiGeoApiChangeLogEntity.Create(operationType?.Trim());
            entity.TenantGroupId = tenantGroupId;
            MultiGeoApiChangeLogDao.Add(tenantGroupId, entity);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to write multi-geo api change log. OperationType: [{operationType}]. Exception: {ex}");
        }
    }

    private static string SerializeForTable(object value)
    {
        if (value == null)
        {
            return null;
        }

        return JsonConvert.SerializeObject(value);
    }

    private static bool ShouldSkipChangeLog(string operationType)
    {
        return !string.IsNullOrEmpty(operationType)
            && SkipChangeLogOperationWhitelist.Contains(operationType.Trim());
    }
}