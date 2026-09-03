using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMPublicAPI.OpusReport.SharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.SharePoint.Report.Audit;
using AvePoint.RA.SharePoint.RMExplorer;
using FluentFTP.Helpers;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SharePoint.Report
{
    [AsyncAudit]
    public class RMSharePointSiteMetricsReportService : IRMSharePointSiteMetricsReportService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMSharePointSiteMetricsReportService));
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        public async Task<string> SubmitSPReportExportJobAsync(SPReportExportRequest request)
        {
            request.SiteCollectionUrls = SanitizeUrls(request.SiteCollectionUrls);
            request.DestinationLibraryUrl = SanitizeUrl(request.DestinationLibraryUrl);

            var normalizedUrls = NormalizeUrls(request.SiteCollectionUrls);
            if (normalizedUrls.Count == 0)
            {
                _logger.Warn("No valid site collection URL is provided.");
                return null;
            }

            var invalidUrls = new List<string>();

            foreach (var siteUrl in normalizedUrls)
            {
                var siteCollection = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
                if (siteCollection == null)
                {
                    invalidUrls.Add(siteUrl);
                    _logger.Warn("Site collection not found for URL: {0}", siteUrl);
                    return invalidUrls.Join(", \n");
                }
            }

            var jobQueueDto = new JobQueueDto
            {
                JobType = JobType.SharePointSiteMetricsReport,
                Parameters = SerializerHelper.SerializeByJsonSerializer(request),
                JobRunType = JobRunBy.Control,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonGroupEmail ?? "RM_TS_RunSchedule",
                CreatedTime = DateTime.UtcNow.Ticks,
            };

            var jobQueueId = JobQueueService.AddToDBJobQueue(jobQueueDto);
            _logger.Info("SP report export job submitted. MessageId={0}, SiteCount={1}", jobQueueId, normalizedUrls.Count);

            return await Task.FromResult("");
        }

        [AsyncAudit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SharePointSiteMetricsReport, IAsyncAfterHandler = typeof(RMSharePointSiteMetricsReportAfterAuditHandler))]
        public async Task<string> RealRunSPReportExportJobAsync(JobQueueDto jobQueueDto)
        {
            var explorerUtil = new RMExplorerUtility();
            var jobId = string.Empty;
            try
            {
                JobType jobType = jobQueueDto.JobType;
                string jobRunByUser = jobQueueDto.JobRunByUser;
                var payload = DeserializePayload(jobQueueDto.Parameters);
                var hasJobRunning = JobMonitorService.GetRunningJobs(new List<JobType> { jobQueueDto.JobType });
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);

                if (payload.SiteCollectionUrls.Count == 0 || string.IsNullOrWhiteSpace(payload.DestinationLibraryUrl))
                {
                    _logger.Warn("Invalid job payload. JobId={0}, SiteCount={1}, DestinationLibraryUrl={2}", jobId, payload.SiteCollectionUrls.Count, payload.DestinationLibraryUrl);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_CreateJobError"));
                    return jobId;
                }

                foreach (var siteUrl in payload.SiteCollectionUrls)
                {
                    var siteCollection = RemoteNodeDao.GetRemoteSiteCollectionNodeByUrl(siteUrl);
                    if (siteCollection == null)
                    {
                        _logger.Warn("Invalid site collection. JobId={0}, SiteUrl={1}", jobId, siteUrl);
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_InvalidSiteCollectionUrl"));
                        return jobId;
                    }
                }

                var validatedResult = await explorerUtil.ValidationDestUrlForRestore(payload.DestinationLibraryUrl);
                if (validatedResult == null)
                {
                    _logger.Warn("Failed to validate destination library URL. JobId={0}, DestinationLibraryUrl={1}", jobId, payload.DestinationLibraryUrl);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_InvalidDestinationLibraryUrl"));
                    return jobId;
                }

                if (hasJobRunning.Count == 0)
                {
                    var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                    DownloadDataInfoDao.Create(new RMDownloadDataInfo
                    {
                        Name = jobId + ".zip",
                        JobId = jobId,
                        RecordsId = Guid.NewGuid(),
                        UserId = account?.UserId ?? string.Empty,
                        JobStatus = (int)DownloadContentJobStatus.Wait,
                        DownloadType = DownloadContentType.SharePointSiteMetricsReport,
                        FileDownloadTime = DateTime.UtcNow.Ticks
                    });

                    var encodedScUrls = EncodeUrlsForCommandLine(payload.SiteCollectionUrls);
                    JobQueueService.HandleMessage(new JobQueueMessage
                    {
                        JobId = jobId,
                        JobType = jobType,
                        CommandLine = $"{jobType} {jobId} {payload.DestinationLibraryUrl}",
                        Extension = encodedScUrls,
                    });

                    _logger.Info("SP report export job started. JobId={0}, SiteCount={1}", jobId, payload.SiteCollectionUrls.Count);
                }
                else
                {
                    _logger.Warn("There is already a running job for type {0}. Cannot start another job.", jobType);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, I18NEntity.GetString("RM_Job_ScheduledJobConflict"));
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to run SP report export job. JobId: {jobId}, Error: {e}");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_CreateJobError"));
            }
            return jobId;
        }

        private static string EncodeUrlsForCommandLine(List<string> urls)
        {
            var json = SerializerHelper.SerializeByJsonSerializer(urls);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        private static SPReportExportRequest DeserializePayload(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.Warn("The raw payload is empty so skip deserialize and return default.");
                return new SPReportExportRequest();
            }
            try
            {
                var payload = SerializerHelper.DeserializeByJsonSerializer<SPReportExportRequest>(raw);
                if (payload != null && payload.SiteCollectionUrls != null)
                {
                    payload.SiteCollectionUrls = NormalizeUrls(payload.SiteCollectionUrls);
                    return payload;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to deserialize job payload. Error: {0}", e);
            }
            return new SPReportExportRequest();
        }

        private static List<string> NormalizeUrls(List<string> urls)
        {
            return urls?.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        }

        private static List<string> SanitizeUrls(List<string> urls)
        {
            if (urls == null) return [];
            return [.. urls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(SanitizeUrl)];
        }

        private static string SanitizeUrl(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) throw new Exception("URL is null or empty.");
                if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)) return url.Trim();
                return new UriBuilder(uri)
                {
                    Path = string.Join("/", uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(
                        s => Uri.EscapeDataString(Uri.UnescapeDataString(s))))
                }.Uri.AbsoluteUri.TrimEnd('/');
            } catch (Exception ex)
            {
                _logger.Error("Failed to sanitize URL. URL={0}, Error={1}", url ?? "", ex);
                return string.Empty;
            }
        }
    }
}