using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.Telemetry;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Telemetry;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using System.Runtime.CompilerServices;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public class APStorageCostEvaluationService : IAPStorageCostEvaluationService
    {
        private readonly IRALogger _logger;
        private readonly IStorageDeviceManager _deviceManager;

        private readonly IStorageDeviceService _storageDeviceService;
        private readonly IRATelemetryService _telemetryService;
        private readonly IJobMonitorService _jobMonitorService;

        private readonly IRMStorageCostEvaluationDao _storageCostEvaluationDao;

        private IXSystem _sourceDevice { get; set; } = default!;
        private string _sourceDeviceId { get; set; } = string.Empty;

        private Action<JMArchiverRententionJobDetails>? _reportAction;

        private static readonly IEnumerable<ProductModule> SUPPORTED_PRODUCT_MODULES = new List<ProductModule>
        {
            ProductModule.ArchiverBackup,
            ProductModule.ExchangeBackup,
            ProductModule.TeamsArchiverBackup,
            //ProductModule.GDriveArchiverBackup,
        };

        private static readonly IEnumerable<string> EXCLUDED_SUBJOB_PREFIXES = new List<string>
        {
            "GEA", // GoogleRecordsDisposal
        };

        public APStorageCostEvaluationService()
        {
            _logger = RALogger.GetInstance(typeof(APStorageCostEvaluationService));

            _deviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
            _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
            _telemetryService = PlatformWindsorManager.GetService<IRATelemetryService>();
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

            _storageCostEvaluationDao = PlatformWindsorManager.GetService<IRMStorageCostEvaluationDao>();

            _logger.Info("APStorageCostEvaluationService initialized.");
        }

        public void Open(APStorageCostEvaluationJobInfo jobInfo, Action<JMArchiverRententionJobDetails> reportAction)
        {
            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            _sourceDeviceId = jobInfo.SourceDevice.Id;
            _sourceDevice = XFactory.InstanceSystem(jobInfo.SourceDevice.GetXRIS(PhysicalDeviceUsage.Data).First());
            _sourceDevice.Open();
            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);

            _reportAction = reportAction;
        }

        public async Task EvaluateAsync(string jobId, JobType jobType)
        {
            _logger.Info($"Starting storage cost evaluation for device: {_sourceDeviceId}");
            using var tokenSource = new CancellationTokenSource();

            try
            {
                _logger.Info($"Supported product modules for storage size calculation: {string.Join(", ", SUPPORTED_PRODUCT_MODULES)}");
                _logger.Info($"Excluded subjob prefixes for storage size calculation: {string.Join(", ", EXCLUDED_SUBJOB_PREFIXES)}");
                double totalArchivedSizeInGB, totalBlobSizeInGB;
                using (PerformanceScope _ = new("GetAllArchiverStorageSize"))
                {
                    totalArchivedSizeInGB = await _storageDeviceService.GetAllArchiverStorageGBSizeAsync(_sourceDeviceId, EXCLUDED_SUBJOB_PREFIXES, tokenSource.Token);
                }

                var runningArchiverJobIds = _jobMonitorService.GetRunningArchiverJobs();
                using (PerformanceScope _ = new("CalculateTotalBlobSize"))
                {
                    totalBlobSizeInGB = await CalculateTotalBlobSizeAsync(runningArchiverJobIds, tokenSource.Token);
                }

                double totalUnrecordedSizeInGB = totalBlobSizeInGB - totalArchivedSizeInGB;

                _logger.Info($"Storage cost evaluation completed for device: {_sourceDeviceId}." +
                    $" Total Archived Size: {totalArchivedSizeInGB} GB," +
                    $" Total Blob Size: {totalBlobSizeInGB} GB," +
                    $" Total Unrecorded Size: {totalUnrecordedSizeInGB} GB");

                var jobTelemetry = new APStorageCostEvaluationJobTelemetry
                {
                    TenantId = TenantLocalValue.LogonGroupId,
                    JobId = jobId,
                    JobType = jobType.ToString(),
                    StorageId = _sourceDeviceId,
                    CalculatedDate = DateTime.UtcNow,
                    TotalArchivedSizeInGB = totalArchivedSizeInGB,
                    TotalBlobSizeInGB = totalBlobSizeInGB,
                    TotalUnrecordedSizeInGB = totalUnrecordedSizeInGB,
                };

                using (PerformanceScope _ = new("SendTelemetryAndSaveEvaluationData"))
                {
                    await SendTelemetryAsync(jobTelemetry);
                    await SaveEvaluationDataAsync(jobTelemetry, jobType);
                }

                _logger.Info($"Telemetry sent for storage cost evaluation of device: {_sourceDeviceId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred during storage cost evaluation for device: {_sourceDeviceId}, details: {ex}");
                AddToReport(new JMArchiverRententionJobDetails
                {
                    Status = JobDetailsStatus.Failed,
                    Comment = ex.Message,
                });
                await tokenSource.CancelAsync();
                throw;
            }

            _logger.Info($"Ending storage cost evaluation for device: {_sourceDeviceId}");
        }

        private void AddToReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            if (_reportAction != null && rententionJobDetails != null)
            {
                _reportAction(rententionJobDetails);
            }
        }

        private async Task SendTelemetryAsync(APStorageCostEvaluationJobTelemetry telemetry)
        {
            try
            {
                await _telemetryService.AddTelemetryForStorageCostEvaluationJobAsync(telemetry);
            }
            catch (Exception e)
            {
                _logger.Error(@$"Fail end and send telemetry, ex:{e}");
            }
        }

        private async Task SaveEvaluationDataAsync(APStorageCostEvaluationJobTelemetry telemetry, JobType jobType)
        {
            _logger.Info($"Saving storage cost evaluation results to control DB for tenant {telemetry.TenantId}, device: {_sourceDeviceId}");
            var result = await _storageCostEvaluationDao.SaveCostEvaluationAsync(new RA.DB.Model.RMStorageCostEvaluation
            {
                TenantId = telemetry.TenantId,
                StorageId = telemetry.StorageId,
                CalculatedDate = telemetry.CalculatedDate,
                TotalArchivedSizeInGB = telemetry.TotalArchivedSizeInGB,
                TotalBlobSizeInGB = telemetry.TotalBlobSizeInGB,
                TotalUnrecordedSizeInGB = telemetry.TotalUnrecordedSizeInGB,
            });
            if (!result)
            {
                _logger.Error($"Failed to save storage cost evaluation results to control DB for tenant {telemetry.TenantId}, device: {_sourceDeviceId}");
            }
            else
            {
                _logger.Info($"Successfully saved storage cost evaluation results to control DB for tenant {telemetry.TenantId}, device: {_sourceDeviceId}");
            }
        }

        private async Task<double> CalculateTotalBlobSizeAsync(HashSet<string> runningArchiverJobIds, CancellationToken cancellationToken = default)
        {
            double totalSizeInMB = 0;
            await foreach (var file in ListAllFilesAsync(cancellationToken))
            {
                var report = new JMArchiverRententionJobDetails()
                {
                    Size = "0",
                    Status = JobDetailsStatus.Successful,
                    Comment = string.Empty,
                    SrcStorageName = file.HighPlusLowName.Replace("\\", "/"),
                };
                if (!runningArchiverJobIds.Contains(file.LowName.Split("_").First()))
                {
                    totalSizeInMB += (double)file.FileSize / (1024 * 1024);
                    report.Size = $"{file.FileSize} Bytes";
                }
                else
                {
                    report.Status = JobDetailsStatus.Skipped;
                }
                AddToReport(report);
            }
            return totalSizeInMB / 1024;
        }

        private async IAsyncEnumerable<XFileInfo> ListAllFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var productModule in SUPPORTED_PRODUCT_MODULES)
            {
                var dataVolume = GenerateDataVolumePath(productModule);
                var excludedPath = SecurityUtils.SafeCombinePath(dataVolume, "Temp").Replace("\\", "/");
                if (_sourceDevice is IXCloudSystem cloudSystem)
                {
                    await foreach (var file in cloudSystem.ListAllFilesAsync(XConvert.FromNames(dataVolume, string.Empty), cancellationToken))
                    {
                        if (file is not null && !file.HighPlusLowName.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                            yield return file;
                    }
                }
            }
        }

        private string GenerateDataVolumePath(ProductModule productModule)
        {
            string containerPath = productModule switch
            {
                ProductModule.ArchiverBackup => ServiceConstants.ArchiverPath,
                ProductModule.ExchangeBackup => ServiceConstants.EXOArchiverPath,
                ProductModule.TeamsArchiverBackup => ServiceConstants.TeamsArchiverPath,
                ProductModule.GDriveArchiverBackup => ServiceConstants.GoogleArchiverPath,
                _ => throw new NotSupportedException($"Product module {productModule} is not supported for data volume path generation."),
            };
            return SecurityUtils.SafeCombinePath(containerPath, ServiceConstants.DefaultDataVolume);
        }

        public async ValueTask DisposeAsync()
        {
            if (_deviceManager is not null)
            {
                _deviceManager.Close(_sourceDevice);
            }
        }
    }
}
