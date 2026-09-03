/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobMonitorJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.SharePoint.FullTetIndexSiteCollectionlist
{
    public class ImportFullTextIndexSiteCollectionlistProcessor
    {
        private static readonly IRALogger _logger = RALogger.GetInstance(typeof(ImportFullTextIndexSiteCollectionlistProcessor));

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
        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private readonly IRestoreSearchService _restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();
        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private long _failedCount;
        private long _succeedCount;
        private string _errorMessage;
        private JobMonitorJobType _jobType;
        List<RMRestoreSiteMapping> backupList = null;

        public ImportFullTextIndexSiteCollectionlistProcessor(string jobId, string path, JobMonitorJobType jobType)
        {
            _jobId = jobId;
            _path = path;
            _jobType = jobType;
            _jobMonitorService.UpdateJobProgress(_jobId, 10);
            ReportManager.IncreaseBase(10);
            ReportManager.StartUpdateJobProgress();
        }

        public void RunJob()
        {
            string filePath = null;
            

            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    _logger.Info("Start to import site collection whitelist");

                    backupList = GetCurrentScList();

                    filePath = DownloadFile();

                    List<WhitelistInfo> excelData = ParseExcelAsync(filePath);

                    if (excelData.Count == 0)
                    {
                        _errorMessage = "RM_RS_SiteWhitelist_EmptyExcel";
                        return;
                    }

                    _logger.Info("Start to validate sc whitelist from excel");
                    _restoreSearchService.CheckSiteCollectionList(excelData, false, out List<WhitelistInfo> notExistSites, out List<WhitelistInfo> validSites, out List<(WhitelistInfo, Exception)> unKnowExceptionSites, out List<string> dupSites);

                    _failedCount += notExistSites.Count + unKnowExceptionSites.Count;

                    var willAddSiteUrl = validSites.Select(vs => vs.SiteCollectionUrl).ToList();
                    willAddSiteUrl.AddRange(dupSites);

                    _logger.Info($"Start to clear site collection {GetListTypeName()}.");
                    ClearListData();

                    _jobMonitorService.UpdateJobProgress(_jobId, 30);
                    _logger.Info("Start to save data into db");

                    int maxIntId = 1;

                    Dictionary<JMImportFullTextIndexSClistJobDetail, RMRestoreSiteMapping> sites = new ();
                    foreach (var info in excelData)
                    {
                        JMImportFullTextIndexSClistJobDetail detail = new() { Url = info.SiteCollectionUrl };
                        RMRestoreSiteMapping site = null;
                        try
                        {
                            if (notExistSites.Contains(info))
                            {
                                _logger.Warn($"Site: {info.SiteCollectionUrl} did not register or did not sync to Opus");
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = _jobType == JobType.ImportSCWhitelist ? "RM_RS_Whitelist_ErrorMessage" : "RM_RS_Blacklist_ErrorMessage";
                                notExistSites.Remove(info);
                                continue;
                            }
                            var exceptionEntry = unKnowExceptionSites.FirstOrDefault(entry => entry.Item1.Equals(info));
                            if (exceptionEntry != default)
                            {
                                _logger.Warn($"Site: {info.SiteCollectionUrl} has unknow exception");
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = exceptionEntry.Item2.Message;
                                unKnowExceptionSites.Remove(exceptionEntry);
                                continue;
                            }
                            if (dupSites.Contains(info.SiteCollectionUrl) && !willAddSiteUrl.Contains(info.SiteCollectionUrl))
                            {
                                _logger.Warn($"Site: {info.SiteCollectionUrl} duplicate");
                                detail.Status = JobDetailsStatus.Skipped;
                                detail.Comment = "RM_RS_SiteWhitelist_HaveDupSiteUrl";
                                continue;
                            }

                            willAddSiteUrl.Remove(info.SiteCollectionUrl);
                            _logger.Info($"Add valid site {info.SiteCollectionUrl} into whitelist");
                            site = GeneRMRestoreSiteMapping(info, maxIntId++);
                            detail.Status = JobDetailsStatus.Successful;
                        }
                        catch (Exception e)
                        {
                            _failedCount++;
                            detail.Status = JobDetailsStatus.Failed;
                            detail.Comment = e.Message;
                            _logger.Error($"Import Site into Whitelist Error:{e}");
                        }
                        finally
                        {
                            sites.Add(detail, site);
                            if (sites.Count >= 500)
                            {
                                RealAddListDataToDB(sites).GetAwaiter().GetResult();
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
                _logger.Warn("Import job is stopped.");
            }
            catch (Exception e)
            {
                _errorMessage = _jobType == JobType.ImportSCWhitelist ? "RM_RS_Whitelist_ErrorMessage" : "RM_RS_Blacklist_ErrorMessage";
                _logger.Error("Some error occurred. Error: {0}", e.ToString());
            }
            finally
            {
                _logger.Info("SucceedCount:[{0}] FailedCount:[{1}]", _succeedCount, _failedCount);
                UpdateJobStatus();
                File.Delete(filePath);
            }
        }

        public async Task RealAddListDataToDB(Dictionary<JMImportFullTextIndexSClistJobDetail, RMRestoreSiteMapping> sites)
        {
            if(sites == null || sites.Count == 0)
            {
                return;
            }
            try
            {
                if(sites.Any(group => group.Key.Status == JobDetailsStatus.Successful))
                {
                    _succeedCount += await _restoreSiteMappingDao.BatchCreateAsync(sites.Where(group => group.Key.Status == JobDetailsStatus.Successful).Select(group => group.Value));
                }
            }
            catch (Exception e)
            {
                _errorMessage = _jobType == JobType.ImportSCWhitelist ? "RM_RS_Whitelist_ErrorMessage" : "RM_RS_Blacklist_ErrorMessage";
                _logger.Error($"Fail to add site collection {GetListTypeName()} into db, ex:{e}");
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


        public string DownloadFile()
        {
            _logger.Info($"Path InvalidFileNameChars: {string.Join(",", Path.GetInvalidFileNameChars())}");
            _logger.Info($"Path InvalidPathChars: {string.Join(",", Path.GetInvalidPathChars())}");
            if (!_path.EndsWith(".xlsx"))
            {
                _errorMessage = "RM_JS_JM_ImportFileFormatError";
                throw new Exception();
            }

            CheckJobIsStoping();

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

        public List<WhitelistInfo> ParseExcelAsync(string filePath)
        {
            CheckJobIsStoping();
            Dictionary<string, List<string[]>> datas = new Dictionary<string, List<string[]>>();
            try
            {
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    datas = ExcelUtil.ReadExcel(fs, null);
                }
            }
            catch (OpenXmlPackageException e)
            {
                if (e.ToString().Contains("Invalid Hyperlink") || e.ToString().Contains("Invalid URI"))
                {
                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        UriFixer.FixInvalidUri(fs, UriFixer.FixUri);
                    }
                    using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        datas = ExcelUtil.ReadExcel(fs);
                    }
                }
            }

            List<WhitelistInfo> res = new List<WhitelistInfo>();
            foreach (List<string[]> shell in datas.Values)
            {
                CheckJobIsStoping();
                if (shell == null)
                {
                    continue;
                }
                foreach (var row in shell)
                {
                    if (row.Length >= 1 && !string.IsNullOrWhiteSpace(row[0]))
                    {
                        res.Add(new WhitelistInfo
                        {
                            SiteCollectionUrl = row[0]?.Trim(' ', '/', '\\'),
                        });
                    }
                }
            }

            CheckJobIsStoping();
            _jobMonitorService.UpdateJobProgress(_jobId, 20);
            return res;
        }

        private List<RMRestoreSiteMapping> GetCurrentScList()
        {
            _logger.Info($"Start to backup site collection {GetListTypeName()}.");
            try
            {
                return _jobType switch
                {
                    JobMonitorJobType.ImportSCWhitelist => _restoreSiteMappingDao.GetAllWhitelist(),
                    JobMonitorJobType.ImportSCBlacklist => _restoreSiteMappingDao.GetAllBlacklist(),
                    _ => new List<RMRestoreSiteMapping>()
                };
            }
            catch (Exception e)
            {
                _logger.Error($"Fail to backup site collection {GetListTypeName()}, ex:{e}");
                throw;
            }
        }

        private void RestoreBackupList(List<RMRestoreSiteMapping> backupList)
        {
            if (backupList == null || backupList.Count == 0)
            {
                return;
            }
            _logger.Info($"Start to restore site collection {GetListTypeName()}.");
            try
            {
                ClearListByJobType();
                const int batchSize = 500;
                for (int i = 0; i < backupList.Count; i += batchSize)
                {
                    var batch = backupList.Skip(i).Take(batchSize).ToList();
                    if (batch.Any())
                    {
                        _restoreSiteMappingDao.BatchCreateAsync(batch).GetAwaiter().GetResult();
                    }
                }
                _logger.Info($"Finish to restore site collection {GetListTypeName()}.");
            }
            catch (Exception e)
            {
                _logger.Error($"Fail to restore site collection {GetListTypeName()}, ex:{e}");
            }
        }

        private int GetLastListIntId()
        {
            return _jobType switch
            {
                JobMonitorJobType.ImportSCWhitelist => _restoreSiteMappingDao.GetLastWhitelistIntId(),
                JobMonitorJobType.ImportSCBlacklist => _restoreSiteMappingDao.GetLastBlacklistIntId(),
                _ => throw new InvalidOperationException($"Unsupported job type: {_jobType}")
            };
        }

        private void ClearListByJobType()
        {
            switch (_jobType)
            {
                case JobMonitorJobType.ImportSCWhitelist:
                    _restoreSiteMappingDao.DeleteWhitelist();
                    break;
                case JobMonitorJobType.ImportSCBlacklist:
                    _restoreSiteMappingDao.DeleteBlacklist();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported job type: {_jobType}");
            }
        }

        public void ClearListData()
        {
            CheckJobIsStoping();
            try
            {
                ClearListByJobType();
            }
            catch (Exception e)
            {
                _logger.Error($"Fail clear site collection {GetListTypeName()}, ex:{e}");
                _failedCount += 1;
                throw;
            }
            CheckJobIsStoping();
            _jobMonitorService.UpdateJobProgress(_jobId, 50);
        }

        private string GetListTypeName() => _jobType switch
        {
            JobMonitorJobType.ImportSCWhitelist => "whitelist",
            JobMonitorJobType.ImportSCBlacklist => "blacklist",
            _ => "list"
        };

        public RMRestoreSiteMapping GeneRMRestoreSiteMapping(WhitelistInfo info, int maxIntId)
        {
            CheckJobIsStoping();
            try
            {
                var settingFlag = _jobType switch
                {
                    JobMonitorJobType.ImportSCBlacklist => RestoreSettingFlag.SearchBlacklist,
                    JobMonitorJobType.ImportSCWhitelist => RestoreSettingFlag.SearchWhitelist,
                    _ => throw new InvalidOperationException($"Unsupported job type: {_jobType}")
                };

                return new RMRestoreSiteMapping
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceSiteUrl = info.SiteCollectionUrl,
                    intId = maxIntId,
                    SettingFlag = settingFlag,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckJobIsStoping()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                throw new JobStopException();
            }
        }

        private void UpdateJobStatus()
        {
            if (CheckJobStatusUtility.isStopping)
            {
                ReportManager.SetJobFinished(JobStatus.Stopped);
            }
            else if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                RestoreBackupList(backupList);
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
                RestoreBackupList(backupList);
                ReportManager.SetJobFinished(JobStatus.Failed, _jobType == JobType.ImportSCWhitelist ? "RM_RS_Whitelist_ErrorMessage" : "RM_RS_Blacklist_ErrorMessage");
                _logger.Info("Import sc whitelist job failed.");
            }
            else if (_succeedCount > 0 && _failedCount > 0)
            {
                ReportManager.SetJobFinished(JobStatus.FinishWithException, _jobType == JobType.ImportSCWhitelist ? "RM_RS_Whitelist_PartDataSaveFail" : "RM_RS_Blacklist_PartDataSaveFail");
                _logger.Info("Import sc whitelist job finished with exception.");
            }
        }
    }
}
