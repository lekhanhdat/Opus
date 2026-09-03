using Aspose.Pdf.Operators;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Multi_Geo.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Multi_Geo
{
    [Audit]
    public class MultiGeoDataCenterService : IMultiGeoDataCenterService
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MultiGeoDataCenterService));
        private readonly IRMFunctionSettingDao RMFunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IJobQueueService JobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        private readonly IJobMonitorService JobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        public async Task<MultiGeoDCInfo> GetMultiGeoDCInformation()
        {
            try
            {
                if (!(await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao)))
                {
                    Logger.Info($"Current account have not yet enable the Multi Geo");
                    return new();
                }
                var mainDC = GetMainDC();
                var supportedDCs = await GetAllDCs();
                var configurationSections = RMGlobalConfiguration.AppConfig.GetMultiGeoDomainUrl();
                var currentDC = RMSSOHelper.CurrentDCName;
                foreach (var dc in supportedDCs)
                {
                    Logger.Info($"Supported DC: {dc.DCInternalName}, display name: {dc.DCDisplayName}");
                    if (!configurationSections.TryGetValue(dc.DCInternalName, out var domainUrl))
                    {
                        Logger.Warn($"The DC: {dc.DCInternalName} don't have corresponding domain url in config");
                        continue;                                                                                               
                    }

                    var ssoUrl = $"{domainUrl}/sso";
                    dc.SSOUrl = ssoUrl;
                }
                return new MultiGeoDCInfo
                {
                    MainDC = mainDC,
                    CurrentDC = currentDC,
                    DCsSupported = supportedDCs,
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Get all DCs supported have error {e}");
                return new();
            }
        }

        public async Task<List<DataCenterInfo>> GetDCsSupported()
        {
            try
            {
                var value = await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.JPMCMultiGEODC);
                return ConvertJsonToDictionary(value).Select(dc => new DataCenterInfo
                {
                    DCDisplayName = dc.Value,
                    DCInternalName = dc.Key
                }).ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Get DCs supported have errors: {e}");
                throw;
            }
        }

        private Dictionary<string, string> ConvertJsonToDictionary(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            var list = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (list is null)
                return new Dictionary<string, string>();

            return list;
        }

        public string GetMainDC()
        {
            return RMKeyValueDao.GetValueByKey(KeyNameCollection.JPMCMultiGEOMainDC)?.Value ?? string.Empty;
        }

        public async Task<bool> IsLimitMultiGeoManageContainer()
        {
            return await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao) && !(RMSSOHelper.CurrentDCName?.Equals(GetMainDC(), StringComparison.OrdinalIgnoreCase) ?? true);
        }

        private async Task<List<DataCenterInfo>> GetAllDCs(bool isNeedGetMainDC = true, string mainDC = "")
        {
            mainDC = isNeedGetMainDC ? GetMainDC() : mainDC;
            if (string.IsNullOrEmpty(mainDC))
            {
                Logger.Warn("Current account don't have main DC");
            }
            var DCsSupported = await GetDCsSupported();
            if (!DCsSupported.Any())
            {
                Logger.Warn("Current account don't have any supported DC");
            }
            if (!string.IsNullOrEmpty(mainDC) && !DCsSupported.Any(dc => string.Equals(dc.DCInternalName, mainDC, StringComparison.OrdinalIgnoreCase))) DCsSupported.Add(new DataCenterInfo
            {
                DCInternalName = mainDC,
                DCDisplayName = I18NEntity.GetString("RM_GEO_DefaultDC_DisplayName")
            });
            return DCsSupported;
        }

        public bool IsMainDC()
        {
            return RMSSOHelper.CurrentDCName?.Equals(GetMainDC(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task<string> RunMainDCSyncCommonDataJob(JobRunBy jobRunBy)
        {
            if (!(await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao)) || !IsMainDC())
            {
                Logger.Info("Current account don't enable MultiGeo feature or current DC is not main dc");
                return string.Empty;
            }
            var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            return RunJob(loginName, JobType.MultiGeoMainDCSyncCommonData, jobRunBy, string.Empty);
        }

        public string RunOtherDCSyncCommonDataJob(SyncCommonDataInforDto syncCommonDataInfor)
        {
            var loginName = !string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail) ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            string syncImages = null;
            if (syncCommonDataInfor != null)
            {
                syncImages = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(syncCommonDataInfor.SyncCommonImages)));
            }
            return RunJob(loginName, JobType.MultiGeoOtherDCSyncCommonData, JobRunBy.Schedule, $"{syncCommonDataInfor.SQLiteDownloadUrl} {syncCommonDataInfor.NeedUpdateTable} {syncImages}");
        }

        private string RunJob(string loginName, JobType jobType, JobRunBy jobRunBy, string parameter)
        {
            string id = string.Empty;
            try
            {
                id = JobQueueService.AddToDBJobQueue(new Contract.CloudService.JobQueueDto
                {
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    Parameters = parameter
                });
            }
            catch (Exception e)
            {
                Logger.Error($"error occurred while {jobType.ToString()}, ERROR:{e.ToString()}");
            }
            return id;
        }

        [Audit(Module = AuditModule.MultiGeo, Category = AuditCategory.MultiGeo, Action = AuditAction.RunMainDCSyncCommonDataJob ,BeforeHandler = typeof(MultiGeoServiceBeforeAuditHandler) ,AfterHandler = typeof(MultiGeoServiceAfterAuditHandler))]
        public async Task<string> RealRunMainDCSyncCommonDataJob(JobRunBy jobRunBy)
        {
            Logger.Info("Start run Main DC sync common data job");
            string jobId = string.Empty;
            try
            {
                var jobType = JobType.MultiGeoMainDCSyncCommonData;
                var username = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var runningSameTypeJobs = JobMonitorService.GetRunningJobs(jobType);
                if (runningSameTypeJobs.Any())
                {
                    Logger.Warn($"Skip Main DC Sync common data job because other instance is running");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "Another MultiGeoMainDCSyncCommonData job is already running.");
                    return jobId;
                }
                jobId = JobMonitorService.CreateJob(jobType, username);

                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    RunBy = JobRunBy.Schedule,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, jobId),
                });
            }
            catch(Exception e)
            {
                Logger.Error($"Real Run Main DC Sync Common Data Job has error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJob(jobId, 0, (int)JobStatus.Failed, DateTime.UtcNow.Ticks, e.Message);
                }
            }
            return jobId;
        }

        [Audit(Module = AuditModule.MultiGeo, Category = AuditCategory.MultiGeo, Action = AuditAction.RunOtherDCSyncCommonDataJob ,BeforeHandler = typeof(MultiGeoServiceBeforeAuditHandler) ,AfterHandler = typeof(MultiGeoServiceAfterAuditHandler))]
        public async Task<string> RealRunOtherDCSyncCommonDataJob(string param)
        {
            Logger.Info("Start run OtherDC DC sync common data job");
            string jobId = string.Empty;
            try
            {
                var jobType = JobType.MultiGeoOtherDCSyncCommonData;
                var runningSameTypeJobs = JobMonitorService.GetRunningJobs(jobType);
                if (runningSameTypeJobs.Any())
                {
                    Logger.Warn($"Skip Other DC Sync common data job because other instance is running");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "Another MultiGeoOtherDCSyncCommonData job is already running.");
                    return jobId;
                }
                jobId = JobMonitorService.CreateJob(jobType, "RM_TS_RunSchedule");
                string[] paras = param.Split(' ');
                string sqLiteDownloadUrl = paras[0];
                long.TryParse(paras[1],out var needUpdateTable);
                string syncImages = paras.Length > 2 ? paras[2] : string.Empty;
                Logger.Info($"Need update table: {needUpdateTable}");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    RunBy = JobRunBy.Schedule,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1} {2} {3} {4}", jobType, jobId, sqLiteDownloadUrl, needUpdateTable, syncImages),
                });
            }
            catch(Exception e)
            {
                Logger.Error($"Real Run Other DC Sync Common Data Job has error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJob(jobId, 0, (int)JobStatus.Failed, DateTime.UtcNow.Ticks, e.Message);
                }
            }
            return jobId;
        }


        public async Task<List<string>> GetOtherDataCentersAsync()
        {
            var current = RMSSOHelper.CurrentDCName;
            var allDCs = await GetDCsSupported();
            return allDCs
                .Where(dc => !string.IsNullOrWhiteSpace(dc?.DCInternalName))
                .Select(dc => dc.DCInternalName)
                .Where(dc => !string.Equals(dc, current, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
    }
