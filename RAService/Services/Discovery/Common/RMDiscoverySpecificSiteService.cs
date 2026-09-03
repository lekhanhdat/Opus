using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Common
{
    public class RMDiscoverySpecificSiteService : IRMDiscoverySpecificSiteService
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMDiscoverySpecificSiteService));
        private readonly IRMDiscoverySpecificSiteDao RMDiscoverySpecificSiteDao = PlatformWindsorManager.GetService<IRMDiscoverySpecificSiteDao>();
        private readonly IRMRemoteNodeDao RMRemoteNode = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private readonly IJobQueueService JobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        private readonly IJobMonitorService JobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();
        private readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private readonly IAuditCommonService AuditCommonService = PlatformWindsorManager.GetService<IAuditCommonService>();

        public RAReturnMessage AddM365ExcludeSites(IEnumerable<DiscoverySpecificSiteDto> sites)
        {
            try
            {
                foreach (DiscoverySpecificSiteDto site in sites)
                {
                    site.SiteCollectionUrl = site.SiteCollectionUrl.Trim(' ', '/', '\\');
                }

                if (RMDiscoverySpecificSiteDao.ExistM365ExcludeListInSiteUrls(sites.Select(s => s.SiteCollectionUrl)))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_DA_DiscoveryExcludeList_PartSiteAlreadyExist")
                    };
                }

                if (!ValidM365ListSites(sites, out List<DiscoverySpecificSiteDto> notExistSites, out List<string> dupSites, out List<DiscoverySpecificSiteDto> validSites))
                {
                    var errorMess = dupSites.Count > 0 ? "RM_DA_SiteExcludelist_HaveDupSiteUrl" : "RM_DA_DiscoveryExcludeList_ErrorMessage";
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString(errorMess) };
                }

                RMDiscoverySpecificSiteDao.AddSpecifySites(sites.ToM365ExcludeSiteModel());
                AddDiscoveryExclusionListAudit(
                    AuditAction.AddSCToDiscoveryM365ExcludeSCList,
                    AuditStatus.Successful,
                    MethodBase.GetCurrentMethod(),
                    null,
                    sites.Select(s => s.SiteCollectionUrl));
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to add specific sites to exclusion list. {e}");
                AddDiscoveryExclusionListAudit(
                    AuditAction.AddSCToDiscoveryM365ExcludeSCList,
                    AuditStatus.Failed,
                    MethodBase.GetCurrentMethod());
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_DA_DiscoveryExcludeList_ErrorMessage") };
            }
        }

        public bool ValidM365ListSites(IEnumerable<DiscoverySpecificSiteDto> sites,
            out List<DiscoverySpecificSiteDto> notExistSites,
            out List<string> dupSites,
            out List<DiscoverySpecificSiteDto> validSites)
        {
            notExistSites = new List<DiscoverySpecificSiteDto>();
            dupSites = new List<string>();
            validSites = new List<DiscoverySpecificSiteDto>();

            HashSet<string> needCheckedUrls = sites.Select(s => s.SiteCollectionUrl).ToHashSet(StringComparer.OrdinalIgnoreCase);
            dupSites = sites.GroupBy(x => x.SiteCollectionUrl).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

            var exiistSites = new HashSet<string>(RMRemoteNode.GetRemoteSiteCollectionBySiteUrls(needCheckedUrls).Select(node => node.url), StringComparer.OrdinalIgnoreCase);
            foreach (var site in sites)
            {
                if (!exiistSites.Contains(site.SiteCollectionUrl))
                {
                    notExistSites.Add(site);
                    continue;
                }
                if (!dupSites.Contains(site.SiteCollectionUrl))
                {
                    validSites.Add(site);
                    continue;
                }
            }
            return dupSites.Count == 0 && notExistSites.Count == 0;
        }

        public void DeleteM365ExcludeList()
        {
            try
            {
                RMDiscoverySpecificSiteDao.DeleteM365ExcludeList();
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to delete all specific sites in exclusion list. {e}");
                throw;
            }
        }

        public bool IsSiteIncludeInExclusionList(string siteUrl)
        {
            try
            {
                return RMDiscoverySpecificSiteDao.IsSiteIncludeInExclusionList(siteUrl);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to check if site is included in exclusion list. {e}");
                return false;
            }
        }

        public async Task<DiscoverySpecificSiteInfo> LoadM365ExclusionListSitesByPaginationAsync(int pageIndex, int pageSize)
        {
            try
            {
                var excludeSites = await RMDiscoverySpecificSiteDao.LoadM365ExcludeListSitesByPaginationAsync(pageIndex, pageSize);
                return new DiscoverySpecificSiteInfo
                {
                    TotalCount = excludeSites.Item2,
                    SiteCollections = excludeSites.Item1.ToDiscoverySpecificSiteDto()
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load specific sites in exclusion list. {e}");
                return new DiscoverySpecificSiteInfo
                {
                    ErrorMessage = e.Message,
                };
            }
        }

        public async Task<List<DiscoverySpecificSiteDto>> GetAllM365ExclusionListSites()
        {
            try
            {
                var excludeSites = await RMDiscoverySpecificSiteDao.GetAllM365ExclusionListSitesAsync();
                return excludeSites.ToDiscoverySpecificSiteDto().ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to get all specific sites in exclusion list. {e}");
                throw;
            }
        }

        public RAReturnMessage RemoveM365ExclusionListSitesByIds(IEnumerable<int> ids)
        {
            try
            {
                var idSet = ids.ToHashSet();
                var removedUrls = GetAllM365ExclusionListSites().GetAwaiter().GetResult()
                    .Where(s => idSet.Contains((int)s.Id))
                    .Select(s => s.SiteCollectionUrl);
                RMDiscoverySpecificSiteDao.BatchRemoveM365ExclusionListSitesByIds(ids);
                AddDiscoveryExclusionListAudit(
                    AuditAction.RemoveSCFromDiscoveryM365ExcludeSCList,
                    AuditStatus.Successful,
                    MethodBase.GetCurrentMethod(),
                    removedUrls);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to remove specific sites from exclusion list. {e}");
                AddDiscoveryExclusionListAudit(
                    AuditAction.RemoveSCFromDiscoveryM365ExcludeSCList,
                    AuditStatus.Failed,
                    MethodBase.GetCurrentMethod());
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = e.Message };
            }
        }

        public RAReturnMessage ImportExcludeSCList(Stream csvExcludeFileStream)
        {
            return ImportSiteCollectionList(csvExcludeFileStream, JobType.DiscoveryImportExcludeSCList);
        }

        private RAReturnMessage ImportSiteCollectionList(Stream csvFileStream, JobType jobType)
        {
            try
            {
                var filePrefix = jobType switch
                {
                    JobType.DiscoveryImportExcludeSCList => JobReportUtility.ImportDiscoveryExcludeListFile,
                    _ => throw new ArgumentException($"Unsupported job type {jobType} for importing site collection list.")
                };
                var folder = jobType switch
                {
                    JobType.DiscoveryImportExcludeSCList => JobReportUtility.ImportDiscoveryExcludeListFolder,
                    _ => throw new ArgumentException($"Unsupported job type {jobType} for importing site collection list.")
                };
                string fileName = filePrefix + DateTime.Now.Ticks.ToString() + ".csv";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), folder, fileName);
                RAStorageUtil.UploadReportBlob(blobName, csvFileStream);
                var jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = blobName,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                AddDiscoveryExclusionListAudit(
                    AuditAction.ImportDiscoveryM365ExcludeSCList,
                    AuditStatus.Successful,
                    MethodBase.GetCurrentMethod());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to run {jobType} import site collection list. {e}");
                AddDiscoveryExclusionListAudit(
                    AuditAction.ImportDiscoveryM365ExcludeSCList,
                    AuditStatus.Failed,
                    MethodBase.GetCurrentMethod());
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        public RAReturnMessage ExportSCExcludelist()
        {
            return ExportSiteCollectionList(JobType.DiscoveryExportExcludeSCList);
        }

        private RAReturnMessage ExportSiteCollectionList(JobType jobType)
        {
            try
            {
                var jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                AddDiscoveryExclusionListAudit(
                        AuditAction.ExportDiscoveryM365ExcludeSCList,
                        AuditStatus.Successful,
                        MethodBase.GetCurrentMethod());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to run {jobType} export site collection list. {e}");
                AddDiscoveryExclusionListAudit(
                    AuditAction.ExportDiscoveryM365ExcludeSCList,
                    AuditStatus.Failed,
                    MethodBase.GetCurrentMethod());
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        public string RealRunExportSCExcludeList(string jobRunByUser)
        {
            return RealRunExportSiteCollectionJob(JobType.DiscoveryExportExcludeSCList, jobRunByUser);
        }

        public string RealRunImportSCExcludeList(string jobRunByUser, string filePath)
        {
            return RealRunImportSiteCollectionJob(JobType.DiscoveryImportExcludeSCList, jobRunByUser, filePath);
        }

        private string RealRunExportSiteCollectionJob(JobType jobType, string jobRunByUser)
        {
            string jobId = string.Empty;
            try
            {
                Logger.Info($"Start real run export sc {jobType} job");
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
                var downloadType = jobType switch
                {
                    JobType.DiscoveryExportExcludeSCList => DownloadContentType.DiscoveryExportExcludeList,
                    _ => throw new ArgumentException($"Unsupported job type {jobType} for exporting site collection list.")
                };
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = downloadType,
                });

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, jobId)
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to run {jobType} export site collection list. {e}");
                if (string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJob(jobId, 0, (int)JobStatus.Failed, DateTime.UtcNow.Ticks, e.Message);
                }
            }

            return jobId;
        }

        private string RealRunImportSiteCollectionJob(JobType jobType, string jobRunByUser, string filePath)
        {
            string jobId = string.Empty;
            try
            {
                Logger.Info($"Start real run import sc {jobType} job");
                var importJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.DiscoveryImportExcludeSCList });
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                if (importJobs.Count > 0)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_ImportWhitelist_JobSkip");
                }
                else
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1} {2}", jobType, jobId, filePath),
                    });
                }

                return jobId;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to run {jobType} import site collection job. {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJob(jobId, 0, (int)JobStatus.Failed, DateTime.UtcNow.Ticks, e.Message);
                }
            }
            return jobId;
        }

        private void AddDiscoveryExclusionListAudit(
            AuditAction action,
            AuditStatus status,
            MethodBase callerMethod,
            IEnumerable<string> originalValues = null,
            IEnumerable<string> newValues = null)
        {
            try
            {
                var originalValue = originalValues != null ? string.Join(", ", originalValues) : "";
                var newValue = newValues != null ? string.Join(", ", newValues) : "";

                AuditCommonService.AddAudits(new List<RMAuditInfo>
                {
                    new RMAuditInfo
                    {
                        Module = AuditModule.Discovery,
                        Category = AuditCategory.DiscoveryConfiguration,
                        Action = action,
                        Status = (int)status,
                        Object = "RM_DA_DiscoveryExcludeList_Title",
                        Method = callerMethod.DeclaringType + "." + callerMethod.Name,
                        UserName = TenantLocalValue.PartnerUser ?? TenantLocalValue.LogonUserEmail,
                        Role = "Administrator",
                        ClientIP = ClientRequestLocalValue.ClientIP,
                        ExecuteOn = DateTime.UtcNow,
                        ModifyContent = (!string.IsNullOrEmpty(originalValue) || !string.IsNullOrEmpty(newValue))
                            ? new List<AuditItem> { new AuditItem { TargetSetting = "RM_JS_JMD_Grid_SiteCollectionURL", OldValue = originalValue, NewValue = newValue } }
                            : null
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to add discovery exclusion list audit. {e}");
            }
        }

        public (IEnumerable<string> runnerSite, IEnumerable<string> skipExcludeSite) GetRunnableAndExcludedM365Sites(IEnumerable<string> siteUrls)
        {
            if(siteUrls == null || !siteUrls.Any())
            {
                Logger.Info("No site urls provided, returning empty lists.");
                return (Enumerable.Empty<string>(), Enumerable.Empty<string>());
            }

            if(!RMDiscoverySpecificSiteDao.IsExistM365ExcludeSite())
            {
                Logger.Info("No M365 Exclude Sites exist, all sites can be run.");
                return (siteUrls, Enumerable.Empty<string>());
            }

            var (runningSites, skipExcludeSites) = RMDiscoverySpecificSiteDao.GetSiteNotInM365ExcludeSite(siteUrls);
            return (runningSites, skipExcludeSites);
        }
    }
}