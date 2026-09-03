using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    internal class APStorageCostEvaluationTaskExecutor : ITaskExecutor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(APStorageCostEvaluationTaskExecutor));

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();
        private readonly IScheduleService _scheduleService = PlatformWindsorManager.GetService<IScheduleService>();
        private readonly IKeyValueService _keyValueService = PlatformWindsorManager.GetService<IKeyValueService>();
        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        /// <summary>
        /// Executes the APStorageCostEvaluation task for all available tenants.
        /// </summary>
        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tenants = _tenantService.GetAllAvailableTenantInfo();
                foreach (var tenant in tenants)
                {
                    await TenantUtil.RunUnderTenantAsync(tenant.TenantId, tenant.RegisterEmail, CreateAPStorageCostEvaluationJobAsync);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while executing APStorageCostEvaluationTaskExecutor: {ex.Message}", ex);
            }
        }

        private async Task CreateAPStorageCostEvaluationJobAsync()
        {
            _logger.Info("Starting to create APStorageCostEvaluation job for tenant.");
            try
            {
                var keyValue = _keyValueService.Get(RMKeyValuesConstants.EnableDeleteRestoredDataFeature);
                if (keyValue is not null && bool.TryParse(keyValue.Value, out bool isEnabled) && isEnabled)
                {
                    if (!await VerifyIfTenantEnableAvePointStorageAsync())
                    {
                        _logger.Info("Tenant does not have AvePoint Storage enabled. Skipping APStorageCostEvaluation schedule creation.");
                        return;
                    }
                    var existingSchedule = await _scheduleService.GetScheduleByTypeServiceAsync(ScheduleType.APStorageCostEvaluationSchedule);
                    if (existingSchedule is not null && existingSchedule.Count > 0)
                    {
                        _logger.Info("APStorageCostEvaluation schedule already exists. Skipping creation.");
                        return;
                    }
                    var generalSetting = await _generalSettingService.GetGeneralSettingAsync();
                    var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSettingConfig.FindSystemTimeZoneById(generalSetting.TimeZoneId));
                    localNow = localNow.AddDays(1);
                    await _scheduleService.CreateScheduleServiceAsync(new ScheduleInfo
                    {
                        Id = Guid.NewGuid().ToString(),
                        StartTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0).ToString(),
                        EndTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0).ToString(),
                        EndType = EndType.NoEnd,
                        Interval = 1,
                        IntervalType = IntervalType.Monthly,
                        DayOfMonth = 1,
                        JobCategory = ScheduleType.APStorageCostEvaluationSchedule,
                        TimeZoneId = generalSetting.TimeZoneId,
                    });
                    _logger.Info("APStorageCostEvaluation schedule created successfully.");
                }
                else
                {
                    _logger.Info("Delete restored data feature is not enabled for this tenant. Skipping APStorageCostEvaluation schedule creation.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while creating APStorageCostEvaluation job: {ex}");
            }
        }

        private async Task<bool> VerifyIfTenantEnableAvePointStorageAsync()
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var info = await client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info is not null && info.Extension is CloudRecordsExtension cloudRecordsExtension)
            {
                return true;
            }
            info = await client.LicenseService.GetLicenseAsync(ProductInfo.PartnerStorageOptimization.Name);
            if (info is not null && info.Extension is PartnerStorageOptimizationExtension)
            {
                return true;
            }
            return false;
        }
    }
}
