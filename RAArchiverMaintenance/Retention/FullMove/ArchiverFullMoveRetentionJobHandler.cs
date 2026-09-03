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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAArchiverCommon;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAArchiverCommon.Utility;
using System.Text;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace RAArchiverMaintenance
{
    public class ArchiverFullMoveRetentionJobHandler
    {
        private readonly IRALogger _logger;
        private readonly IRestoreSearchService _restoreSearchService;
        private readonly IRMReportManager _reportManager;
        private readonly IRMSubJobDao _subJobDao;
        private readonly IRMKeyValueDao _keyValueDao;

        private readonly string _jobId;

        private bool _hasCompleteNode;
        private bool _hasErrorNode;

        private long _totalSuccess = 0;
        private long _totalSkipped = 0;
        private long _totalFailed = 0;

        private readonly ChunkedCsvWriter<MigrationRecord>? _reportWriter;

        public ArchiverFullMoveRetentionJobHandler(string jobId, JobType jobType)
        {
            _jobId = jobId;

            _logger = RALogger.GetInstance(typeof(ArchiverFullMoveRetentionJobHandler));
            _restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

            _reportWriter = CreateCsvWriter();

            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                MainJobId = jobId.Split('_')[0],
                SubJobId = jobId,
                JobType = jobType,
            });
            _logger.Info($"ArchiverFullMoveRetentionJobHandler created for jobId: {jobId}, jobType: {jobType}");
        }

        public async Task RunAsync()
        {
            string comment = string.Empty;
            CompoundDisposalStatistics.Instance.StartStatistic();
            try
            {
                _reportManager.StartUpdateJobProgress();

                var subJob = _subJobDao.GetSubJob(_jobId, true);
                var archiverFullMoveJob = SerializerHelper.DeserializeByDataContractSerializer<ArchiverFullMoveRetentionJobInfo>(subJob.JobContext.Settings);
                DecryptSecretForGoogleStorage(archiverFullMoveJob);

                var retentionInfo = new ArchiverFullMoveRetentionInfo(archiverFullMoveJob, _jobId);

                await using var fullMoveService = PlatformWindsorManager.GetService<IArchiverFullMoveRetentionService>();
                fullMoveService.Open(retentionInfo, SendJobReport, SendMigrationReport);
                await fullMoveService.FullMoveDataAsync();

                _logger.Info($"Full move job completed for jobId: {_jobId}," +
                    $" total success: {_totalSuccess}," +
                    $" total skipped: {_totalSkipped}," +
                    $" total failed: {_totalFailed}.");
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                comment = ex.Message;
                _logger.Error($"Error occurred for jobId: {_jobId}, details: {ex}");
                _hasErrorNode = true;
            }
            finally
            {
                try
                {
                    if (_reportWriter is not null)
                    {
                        await _reportWriter.CompleteAsync();
                        await _reportWriter.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error while completing report writer for jobId: {_jobId}, details: {ex}");
                }

                CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                CompoundDisposalStatistics.Instance.WaitEndStatistic();
                if (_restoreSearchService.HasReachedIndexSizeLimitation())
                {
                    _restoreSearchService.SyncCategoryDataSize();
                }
                var jobStatus = GetJobStatus();
                _logger.Info($"JobId: {_jobId} is finishing with status: {jobStatus}, comment: \"{comment}\".");
                _reportManager.SetJobFinished(jobStatus, comment);
            }
        }

        private ChunkedCsvWriter<MigrationRecord>? CreateCsvWriter()
        {
            var reportStorageId = _keyValueDao.GetStorageIdForArchivedDataMigrationReport();
            if (!string.IsNullOrEmpty(reportStorageId) && Guid.TryParse(reportStorageId, out _))
            {
                var storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
                var reportStorage = storageDeviceService.GetStorageDeviceById(reportStorageId, needDecryptSecert: true);
                if (reportStorage is not null && reportStorage.Type == (int)StorageDeviceType.CloudAzure)
                {
                    var maxRecordsPerFile = _keyValueDao.GetMaxRecordsPerCSVFile();
                    var xriString = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(reportStorage).GetXRIS(PhysicalDeviceUsage.Data).FirstOrDefault();
                    if (!string.IsNullOrEmpty(xriString))
                    {
                        var writer = new ChunkedCsvWriter<MigrationRecord>(
                            xriString: xriString,
                            folderPath: SecurityUtils.SafeCombinePath("ArchivedDataMigrationReports", _jobId.Split('_').FirstOrDefault()),
                            groupKeySelector: record => record.SiteUrl,
                            jobIdSelector: record => _jobId,
                            maxRecordsPerFile: maxRecordsPerFile);
                        return writer;
                    }
                }
            }
            return null;
        }

        private JobStatus GetJobStatus()
        {
            if (_hasCompleteNode && _hasErrorNode)
            {
                return JobStatus.FinishWithException;
            }
            else if (!_hasCompleteNode && _hasErrorNode)
            {
                return JobStatus.Failed;
            }
            return JobStatus.Finished;
        }

        private void SendJobReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            AnalyzeStatus(rententionJobDetails.Status);
            _logger.Info($"File {rententionJobDetails.SrcStorageName} moved," +
                $" status {rententionJobDetails.Status}," +
                $" size {rententionJobDetails.Size}," +
                $" comment \"{rententionJobDetails.Comment}\".");
            _reportManager.Increase();
        }

        private void SendMigrationReport(JMArchiverRententionMigrationDetails rententionMigrationDetails)
        {
            _reportWriter?.EnqueueAsync(new MigrationRecord()
            {
                SiteUrl = rententionMigrationDetails.SiteUrl,
                SharePointUrl = rententionMigrationDetails.SharePointUrl,
                SourceStorageName = rententionMigrationDetails.SrcStorageName,
                TargetStorageName = rententionMigrationDetails.DesStorageName,
                BlobPath = rententionMigrationDetails.BlobPath,
                Status = rententionMigrationDetails.Status.ToString(),
                Size = long.TryParse(rententionMigrationDetails.Size, out var size) ? size : 0,
                Action = rententionMigrationDetails.Action,
                JobId = rententionMigrationDetails.JobId,
                Message = rententionMigrationDetails.Comment,
            }).ExecuteAsyncTask();
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful)
            {
                _hasCompleteNode = true;
                _totalSuccess++;
            }
            else if (status == JobDetailsStatus.Skipped)
            {
                _hasCompleteNode = true;
                _totalSkipped++;
            }
            else if (status == JobDetailsStatus.Exception)
            {
                _hasCompleteNode = true;
                _hasErrorNode = true;
                _totalFailed++;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                _hasErrorNode = true;
                _totalFailed++;
            }
        }

        private void DecryptSecretForGoogleStorage(ArchiverFullMoveRetentionJobInfo archiverFullMoveJob)
        {
            if (archiverFullMoveJob is not null)
            {
                if (archiverFullMoveJob.SourceDevice != null)
                    DecryptGoogleStorageSecret(archiverFullMoveJob.SourceDevice);
                if (archiverFullMoveJob.DestinationDevice != null)
                    DecryptGoogleStorageSecret(archiverFullMoveJob.DestinationDevice);
            }
        }

        private void DecryptGoogleStorageSecret(LogicalDeviceDto dto)
        {
            if (dto.PhysicalDrives != null && dto.PhysicalDrives.Count > 0)
            {
                foreach (var physicalDrive in dto.PhysicalDrives)
                {
                    string begin = "-----BEGIN PRIVATE KEY-----";
                    string end = "-----END PRIVATE KEY-----";
                    if (physicalDrive.Type == (int)StorageDeviceType.Google)
                    {
                        if (physicalDrive.Password != null && physicalDrive.Password.Count > 0)
                        {
                            string[] keyValue = physicalDrive.Password[0].Split(new char[] { '=' });
                            if (!keyValue[0].EndsWith("tokensecret") && !(keyValue[1].StartsWith(begin) && keyValue[1].Contains(end)))
                            {
                                keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(UnWrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1])));
                            }
                            physicalDrive.UpdatePassword(new List<string> { keyValue[0] + "=" + keyValue[1] });
                        }
                    }
                }
            }
        }

        private string UnWrapKey(string password)
        {
            var result = CspCrossPlatformExchangeWrapper.UnWrapKey(password);
            return Encoding.UTF8.GetString(result, 0, result.Length);
        }
    }
}
