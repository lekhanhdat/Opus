using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DalServices;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.RA.Service.Services.Tenant;
using Cloud.Sdk.Data.Cop.Insights;
using Cloud.Sdk.Data.Dal;
using Cloud.Sdk.LAL.PlatformSS;
using DocumentFormat.OpenXml.Spreadsheet;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITenantService = Cloud.Sdk.LAL.PlatformSS.ITenantService;

namespace AvePoint.RA.Service.Services.DalServices
{
    public class DalService : IDalService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(DalService));

        private readonly ICloudSdkLALPlatformSSClientFactory lalPlatformSSClientFactory = AosApiUtility.LALPlatformSSClientFactory;

        private const Int32 InitPollIntervalMs = 2_000;
        private const Int32 InitMaxAttempts =480; // ~8 minutes total

        private LALPlatformSSDGApiClient TenantClient(String tenantId)
            => lalPlatformSSClientFactory.CreateLALPlatformSSDGClient(GCommonRoleConfiguration.DAL_GATEWAY_API_URL, tenantId);
        private LALPlatformSSDGApiClient TenantInteractiveClient(String tenantId) => lalPlatformSSClientFactory.CreateLALPlatformSSDGClient(GCommonRoleConfiguration.DAL_GATEWAY_API_URL, tenantId, Guid.Parse("0b755c6a-f0fd-4047-ab6f-c633b8f59472"));
        public async Task InitializeTenantAsync(string tenantId, string cloudTenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tenantId);
                ArgumentNullException.ThrowIfNull(cloudTenantId);

                logger.Info($"[{nameof(InitializeTenantAsync)}] Initializing DAL tenant. TenantId:{tenantId}.");

                ITenantService tenantService = TenantClient(tenantId).TenantService;
                IJobService jobService = TenantClient(tenantId).JobService;

                SystemTaskModel taskModel = await tenantService
                    .InitializeAsync(tenantId, new InitializationModel())
                    .ConfigureAwait(false);

                logger.Info($"[{nameof(InitializeTenantAsync)}] DAL initialization task created. TaskId:{taskModel.Id}. Polling for completion.");

                for (Int32 attempt = 0; attempt < InitMaxAttempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    SystemTaskModel taskInfo = await jobService
                        .GetSystemTaskAsync(taskModel.Id)
                        .ConfigureAwait(false);

                    logger.Info($"[{nameof(InitializeTenantAsync)}] Attempt {attempt + 1}/{InitMaxAttempts}: status={taskInfo.Status}.");

                    if (taskInfo.Status == Cloud.Sdk.Data.Dal.JobStatus.Completed)
                    {
                        logger.Info($"[{nameof(InitializeTenantAsync)}] Tenant initialized successfully. TenantId:{tenantId}.");
                        return;
                    }

                    if (taskInfo.Status is Cloud.Sdk.Data.Dal.JobStatus.Failed
                        or Cloud.Sdk.Data.Dal.JobStatus.FinishedWithExceptions
                        or Cloud.Sdk.Data.Dal.JobStatus.Timeout)
                    {
                        logger.Error($"[{nameof(InitializeTenantAsync)}] Initialization failed with status:{taskInfo.Status}. TenantId:{tenantId}.");
                        throw new InvalidOperationException($"DAL tenant initialization failed with status: {taskInfo.Status}.");
                    }

                    await Task.Delay(InitPollIntervalMs, cancellationToken).ConfigureAwait(false);
                }

                logger.Error($"[{nameof(InitializeTenantAsync)}] Initialization timed out after {InitMaxAttempts} attempts. TenantId:{tenantId}.");
                throw new TimeoutException($"DAL tenant initialization did not complete within the expected time.");
            }
            catch(Exception ex)
            {
                logger.Error($"[{nameof(InitializeTenantAsync)}] Exception occurred while initializing tenant. TenantId:{tenantId}. Exception: {ex}");
                throw;
            }
            
        }

        public async Task RegisterConnectorAsync(String tenantId, String cloudTenantId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tenantId);
            ArgumentNullException.ThrowIfNull(cloudTenantId);

            logger.Info($"[{nameof(RegisterConnectorAsync)}] Registering DAL connector. TenantId:{tenantId}");

            ITenantService tenantService = TenantClient(tenantId).TenantService;
            await tenantService.RegisterAsync(tenantId, new ConnectorRegistrationModel
            {
                CloudTenantId = cloudTenantId,
                ConnectorIds = new List<Guid> {
                    Guid.Parse(Constants.ConnectorIds.MicrosoftSiteBasicBatchConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftSiteIncrementalConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftSiteListItemsBatchConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftSiteCsomItemBatchConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftOneDriveSiteBatchConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftTeamChannelSiteBatchConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftSensitivityLabelConnectorId),
                    Guid.Parse(Constants.ConnectorIds.MicrosoftDriveBasicBatchConnectorId),}
            }).ConfigureAwait(false);

            logger.Info($"[{nameof(RegisterConnectorAsync)}] Connector registered. TenantId:{tenantId}");
        }
        public async Task RegisterConnectorDefinitionAsync()
        {
            try
            {
                String connectorId = Cloud.Sdk.Data.Dal.Constants.ConnectorIds.InteractiveConnectorId;
                logger.Info($"[{nameof(RegisterConnectorDefinitionAsync)}] Registering DAL connector definition. ConnectorId:{connectorId}.");
                IConnectorService connectorService = TenantClient(TenantLocalValue.LogonGroupId).ConnectorService;
                var schema = new JsonSchema();
                schema.Title = "MicrosoftSite";
                schema.Type = JsonObjectType.Object;
                schema.Properties.Add("Opus_ContainerId", new JsonSchemaProperty()
                {
                    Type = JsonObjectType.String
                });
                await connectorService.RegisterConnectorDefinition(connectorId, new ConnectorDefinitionModel
                {
                    IngestionDataType = IngestionDataType.MicrosoftSite,
                    SchemaJson = schema.ToJson(),
                }).ConfigureAwait(false);

                logger.Info($"[{nameof(RegisterConnectorDefinitionAsync)}] Connector definition registered. ConnectorId:{connectorId}");
            }
            catch (Exception ex)
            {
                logger.Error($"[{nameof(RegisterConnectorDefinitionAsync)}] Exception occurred while registering connector definition. Exception: {ex}");
                throw;
            }
        }
        public async Task<Guid> TriggerJobAsync(string cloudTenantId, List<ConnectorType> connectorTypes, List<string> objectIds)
        {
            try
            {
                String customerId = TenantLocalValue.LogonGroupId;
                JobInitiationModel jobInitiationModel = new()
                {
                    ConnectorTypes = connectorTypes,
                    CustomerId = customerId,
                    CloudTenantId = cloudTenantId,
                    ObjectIds = objectIds, 
                };
                IJobService jobService = TenantClient(customerId).JobService;
                JobInfoModel jobInfo = await jobService.TriggerJobAsync(jobInitiationModel).ConfigureAwait(false);
                return jobInfo.Id;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger dal job, Exception: {ex}");
                throw;
            }
        }

        public async Task IngestContainerIdAsync( String cloundTenantId, String containerId, List<String> sites)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(cloundTenantId);
                ArgumentNullException.ThrowIfNull(containerId);
                var capturedTime = DateTime.UtcNow;
                var data = sites.Select(siteId => new
                {
                    CloudTenantId = cloundTenantId,
                    SiteId = siteId,
                    Opus_ContainerId = containerId,
                    CapturedTime = capturedTime
                }).ToList();

                OperationResultModel result =
                    await TenantInteractiveClient(TenantLocalValue.LogonGroupId).IngestionService.IngestAsync(new IngestionModel
                        {
                            IngestionDataType = IngestionDataType.MicrosoftSite,
                            Data = data.Cast<dynamic>().ToList()
                    })
                        .ConfigureAwait(false);

               logger.Info($"Ingested containerId: {containerId} in cloudTenantId: {cloundTenantId}. Result: {result}");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to ingest container, Exception: {ex}");
                throw;
            }
        }
        public async Task<JobHistoryModel> GetJobStatusAsync(Guid id)
        {
            IJobService jobService = TenantClient(TenantLocalValue.LogonGroupId).JobService;
            JobHistoryModel jobHistory = await jobService.GetJobHistoryAsync(id).ConfigureAwait(false);
            return jobHistory;
        }
    }
}
