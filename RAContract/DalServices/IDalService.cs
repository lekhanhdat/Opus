using Cloud.Sdk.Data.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;

namespace AvePoint.RA.Contract.DalServices
{
    public interface IDalService
    {
        /// <summary>
        /// Initializes the DAL tenant workspace. Safe to call multiple times (idempotent).
        /// Polls the background task until it reaches a terminal state.
        /// </summary>
        System.Threading.Tasks.Task InitializeTenantAsync(String tenantId, String cloudTenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers one or more DAL connector IDs for the specified tenant.
        /// </summary>
        System.Threading.Tasks.Task RegisterConnectorAsync(String tenantId, String cloudTenantId, CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task<Guid> TriggerJobAsync(String cloudTenantId, List<ConnectorType> connectorTypes, List<string> objectIds);
        Task<JobHistoryModel> GetJobStatusAsync(Guid id);
        System.Threading.Tasks.Task RegisterConnectorDefinitionAsync();
        System.Threading.Tasks.Task IngestContainerIdAsync(String cloundTenantId, String containerId, List<String> siteIds);

    }
}
