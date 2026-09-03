using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Enums;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.FullTetIndexSiteCollectionlist;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace AvePoint.RA.SharePoint.Discovery.Import
{
    public class ImportDiscoverySpecifySitesProccessor
    {
        private static readonly IRALogger _logger = RALogger.GetInstance(typeof(ImportDiscoverySpecifySitesProccessor));

        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }

        private readonly string _jobId;
        private readonly string _path;

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly IRMDiscoverySpecificSiteService _discoverySpecificSiteService = PlatformWindsorManager.GetService<IRMDiscoverySpecificSiteService>();
        private readonly IRMDiscoverySpecificSiteDao _rMDiscoverySpecificSiteDao = PlatformWindsorManager.GetService<IRMDiscoverySpecificSiteDao>();
        private long _failedCount;
        private long _succeedCount;
        private string _errorMessage;


        private JobType _jobType;

        private List<DiscoverySpecificSiteDto> _backupList = null;

        public ImportDiscoverySpecifySitesProccessor(string jobId, string path, JobType jobType)
        {
            _jobId = jobId;
            _path = path;
            _jobType = jobType;
            _jobMonitorService.UpdateJobProgress(_jobId, 10);
            ReportManager.IncreaseBase(10);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task RunJob()
        {
            string filePath = null;
            try
            {
                using (CheckJobStopScope jsScope = new CheckJobStopScope())
                {
                    _logger.Info($"Start to import discovery specific sites for job {_jobId}");
                    _backupList = GetCurrentScList();

                    filePath = DownloadFile();

                    List<DiscoverySpecificSiteDto> csvData = await ParseCSVAsync(filePath);

                    if (csvData.Count == 0)
                    {
                        _errorMessage = "RM_RS_SiteWhitelist_EmptyExcel";
                        return;
                    }
                    _logger.Info("Start to validate sc whitelist from csv file");
                    _discoverySpecificSiteService.ValidM365ListSites(csvData, out List<DiscoverySpecificSiteDto> notExistSites, out List<string> dupSites, out List<DiscoverySpecificSiteDto> validSites);

                    _failedCount += notExistSites.Count;

                    var willAddSiteUrls = validSites.Select(s => s.SiteCollectionUrl).ToList();
                    willAddSiteUrls.AddRange(dupSites);
                    _logger.Info($"Start to clear site collection {_jobType}");
                    ClearListData();

                    _jobMonitorService.UpdateJobProgress(_jobId, 30);
                    _logger.Info("Start to save data into db");
                    Dictionary<JMImportFullTextIndexSClistJobDetail, RMDiscoverySpecificSite> sites = new();
                    foreach (var info in csvData)
                    {
                        JMImportFullTextIndexSClistJobDetail detail = new() { Url = info.SiteCollectionUrl };
                        RMDiscoverySpecificSite site = ConvertDtoToModel(info);
                        try
                        {
                            _logger.Info($"Validating site {info.SiteCollectionUrl}");
                            if (notExistSites.Any(s => s.SiteCollectionUrl.Equals(info.SiteCollectionUrl, StringComparison.OrdinalIgnoreCase)))
                            {
                                var errorMess = "RM_DA_DiscoveryExcludeList_ErrorMessage";
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = errorMess;
                                continue;
                            }
                            if(dupSites.Contains(info.SiteCollectionUrl) && !willAddSiteUrls.Contains(info.SiteCollectionUrl))
                            {
                                detail.Status = JobDetailsStatus.Skipped;
                                detail.Comment = "RM_DA_SiteExcludelist_HaveDupSiteUrl";
                                continue;
                            }
                            willAddSiteUrls.Remove(info.SiteCollectionUrl);
                            _logger.Info($"Add valid site {info.SiteCollectionUrl} into DB");
                            detail.Status = JobDetailsStatus.Successful;
                        }
                        catch(Exception e)
                        {
                            _failedCount++;
                            detail.Status = JobDetailsStatus.Failed;
                            detail.Comment = e.Message;
                            _logger.Error($"Import Site into DB Error:{e}");
                        }
                        finally
                        {
                            sites.Add(detail, site);
                            if (sites.Count >= 500)
                            {
                                await RealAddListDataToDB(sites);
                                sites.Clear();
                            }
                        }
                    }
                    RealAddListDataToDB(sites).GetAwaiter().GetResult();
                    _jobMonitorService.UpdateJobProgress(_jobId, 90);
                    _logger.Info("Finish to import site collection whitelist");
                }
            }
            catch (JobStopException)
            {
                _logger.Info($"Job {_jobId} is stopping, exit directly.");
            }
            catch (Exception ex)
            {
                if(string.IsNullOrEmpty(_errorMessage))
                {
                    _errorMessage = "RM_DA_DiscoveryExcludeList_ErrorMessage";
                }
                _logger.Error("Some error occurred. Error: {0}", ex.ToString());
            }
            finally
            {
                _logger.Info("SucceedCount:[{0}] FailedCount:[{1}]", _succeedCount, _failedCount);
                await UpdateJobStatus();
                if(File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to delete file {filePath}. Error: {ex}");
                    }
                }
            }
        }

        private async Task UpdateJobStatus()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                ReportManager.SetJobFinished(JobStatus.Stopped);
            }
            else if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                await RestoreBackupList(_backupList);
                ReportManager.SetJobFinished(JobStatus.Failed, _errorMessage);
                _logger.Info("Import sc whitelist job failed.");
            }
            else if (_failedCount == 0)
            {
                ReportManager.SetJobFinished(JobStatus.Finished);
                _logger.Info("Import sc whitelist job finished.");
            }
            else if (_succeedCount == 0)
            {
                await RestoreBackupList(_backupList);
                ReportManager.SetJobFinished(JobStatus.Failed, "RM_DA_DiscoveryExcludeList_ErrorMessage");
                _logger.Info("Import sc whitelist job failed.");
            }
            else if (_succeedCount > 0 && _failedCount > 0)
            {
                ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_DA_DiscoveryExcludeList_PartDataSaveFail");
                _logger.Info("Import sc whitelist job finished with exception.");
            }
        }

        private async Task RestoreBackupList(List<DiscoverySpecificSiteDto> backupList)
        {
            if (backupList == null || backupList.Count == 0)
            {
                return;
            }
            _logger.Info($"Start to restore site collection.");
            try
            {
                ClearListByJobType();
                const int batchSize = 500;
                for (int i = 0; i < backupList.Count; i += batchSize)
                {
                    var batch = backupList.Skip(i).Take(batchSize).ToList();
                    if (batch.Any())
                    {
                        await _rMDiscoverySpecificSiteDao.BatchCreateAsync(backupList.ConvertAll(ConvertDtoToModel));
                    }
                }
                _logger.Info($"Finish to restore site collection.");
            }
            catch (Exception e)
            {
                _logger.Error($"Fail to restore site collection, ex:{e}");
            }
        }

        private RMDiscoverySpecificSite ConvertDtoToModel(DiscoverySpecificSiteDto info) => _jobType switch
        {
            JobType.DiscoveryImportExcludeSCList => new RMDiscoverySpecificSite
            {
                Url = info.SiteCollectionUrl,
                Type = SpecifySiteFlag.Exclude,
                SourceFlag = SourceFlag.SharePoint
            },
            _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
        };

        private async Task RealAddListDataToDB(Dictionary<JMImportFullTextIndexSClistJobDetail, RMDiscoverySpecificSite> sites)
        {
            if (sites == null || sites.Count == 0)
            {
                return;
            }
            try
            {
                if (sites.Any(group => group.Key.Status == JobDetailsStatus.Successful))
                {
                    _succeedCount += _rMDiscoverySpecificSiteDao.AddSpecifySites(sites.Where(group => group.Key.Status == JobDetailsStatus.Successful).Select(group => group.Value));
                }
            }
            catch (Exception e)
            {
                _errorMessage = "RM_DA_DiscoveryExcludeList_ErrorMessage";
                _logger.Error($"Fail to add site collection into db, ex:{e}");
                foreach (var site in sites)
                {
                    if (site.Key.Status == JobDetailsStatus.Successful)
                    {
                        site.Key.Status = JobDetailsStatus.Failed;
                        site.Key.Comment = e.Message;
                        _failedCount += 1;
                    }
                }
                throw;
            }
            finally
            {
                ReportManager.BatchSendJobDetail(sites.Keys);
            }
        }

        private void ClearListData()
        {
            CheckJobIsStopping();
            try
            {
                ClearListByJobType();
            }
            catch (Exception e)
            {
                _logger.Error($"Fail clear site collection {_jobType}, ex:{e}");
                _failedCount += 1;
                throw;
            }
            CheckJobIsStopping();
            _jobMonitorService.UpdateJobProgress(_jobId, 50);
        }

        private void ClearListByJobType()
        {
            switch (_jobType)
            {
                case JobType.DiscoveryImportExcludeSCList:
                    _discoverySpecificSiteService.DeleteM365ExcludeList();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported job type: {_jobType}");
            }
        }

        private async Task<List<DiscoverySpecificSiteDto>> ParseCSVAsync(string filePath)
        {
            CheckJobIsStopping();
            List<DiscoverySpecificSiteDto> datas = new List<DiscoverySpecificSiteDto>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[2];
                    int bytesRead = fs.Read(header, 0, header.Length);
                    fs.Seek(0, SeekOrigin.Begin);
                    if (bytesRead == header.Length && header[0] == 0x50 && header[1] == 0x4B)
                    {
                        _errorMessage = "RM_JS_JM_ImportFileFormatError";
                        throw new Exception("RM_JS_JM_ImportFileFormatError");
                    }
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        var line = string.Empty;
                        var special = false;
                        var rowStr = string.Empty;
                        bool isHearder = true;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }
                            if (isHearder)
                            {
                                isHearder = false;
                                continue;
                            }
                            rowStr += line;
                            int remainder = (line.Split(new char[] { '"' }, StringSplitOptions.None).Length - 1) % 2;
                            if (remainder != 0)
                            {
                                if (special)
                                {
                                    special = false;
                                }
                                else
                                {
                                    rowStr += System.Environment.NewLine;
                                    special = true;
                                    continue;
                                }
                            }
                            else
                            {
                                if (special)
                                {
                                    rowStr += System.Environment.NewLine;
                                    continue;
                                }
                            }
                            var itemFields = CSVHelper.AnalyseCSVRow2ArrayForManualImport(rowStr);
                            rowStr = string.Empty;
                            if (itemFields != null && itemFields.Length > 0)
                            {
                                datas.Add(new DiscoverySpecificSiteDto
                                {
                                    SiteCollectionUrl = itemFields[0]
                                                        ?.Replace(System.Environment.NewLine, string.Empty)
                                                        .Replace("\n", string.Empty)
                                                        .Replace("\r", string.Empty)
                                                        .Trim(' ', '/', '\\')
                                });
                            }

                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.Error("Failed to parse CSV file.", ex);
                throw;
            }
            return datas;
        }

        private string DownloadFile()
        {
            _logger.Info($"Path InvalidFileNameChars: {string.Join(",", Path.GetInvalidFileNameChars())}");
            _logger.Info($"Path InvalidPathChars: {string.Join(",", Path.GetInvalidPathChars())}");
            if (!_path.EndsWith(".csv"))
            {
                _errorMessage = "RM_JS_JM_ImportFileFormatError";
                throw new Exception();
            }

            CheckJobIsStopping();

            try
            {
                return JobReportUtility.GetImportJobCSVFile(_path);
            }
            catch (Exception e)
            {
                _logger.Error("Cannot download file: {0}, error: {1}", _path, e.ToString());
                throw;
            }
        }

        private void CheckJobIsStopping()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                throw new JobStopException();
            }
        }

        private List<DiscoverySpecificSiteDto> GetCurrentScList()
        {
            _logger.Info("Start to get current specific sites list for backup.");
            try
            {
                return _jobType switch
                {
                    JobType.DiscoveryImportExcludeSCList => _discoverySpecificSiteService.GetAllM365ExclusionListSites().Result.ToList(),
                    _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get current specific sites list for backup.", ex);
                throw;
            }
        }
    } 
}
