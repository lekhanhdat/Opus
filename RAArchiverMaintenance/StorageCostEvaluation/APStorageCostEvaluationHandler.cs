using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using System.Text;

namespace RAArchiverMaintenance
{
    public class APStorageCostEvaluationHandler
    {
        private readonly IRALogger _logger;
        private readonly IRMReportManager _reportManager;
        private readonly IRMSubJobDao _subJobDao;

        private readonly string _jobId;

        private bool _hasCompleteNode;
        private bool _hasErrorNode;

        public APStorageCostEvaluationHandler(string jobId)
        {
            _jobId = jobId;

            _logger = RALogger.GetInstance(typeof(APStorageCostEvaluationHandler));
            _reportManager = ReportMangerFactory.Instance.ReportManager;
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

            _logger.Info($"APStorageCostEvaluationHandler initialized for jobId: {jobId}");
        }

        public async Task RunAsync()
        {
            string comment = string.Empty;
            try
            {
                ReportMangerFactory.Instance.Init(_jobId, JobType.APStorageCostEvaluation);
                _reportManager.StartUpdateJobProgress();
                var subJob = _subJobDao.GetSubJob(_jobId, true);
                var jobSettings = SerializerHelper.DeserializeByDataContractSerializer<APStorageCostEvaluationJobInfo>(subJob.JobContext.Settings);
                DecryptSecretForGoogleStorage(jobSettings);

                await using var evaluationService = PlatformWindsorManager.GetService<IAPStorageCostEvaluationService>();
                evaluationService.Open(jobSettings, SendJobReport);
                await evaluationService.EvaluateAsync(subJob.ParentId, (JobType)subJob.JobType);

                _logger.Info($"APStorageCostEvaluationHandler completed successfully for jobId: {_jobId}");
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
                var jobStatus = GetJobStatus();
                _logger.Info($"JobId: {_jobId} is finishing with status: {jobStatus}, comment: \"{comment}\".");
                _reportManager.SetJobFinished(jobStatus, comment);
            }
        }

        private void SendJobReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            AnalyzeStatus(rententionJobDetails.Status);
            if (rententionJobDetails.Status != JobDetailsStatus.Failed)
            {
                _logger.Info($"File {rententionJobDetails.SrcStorageName} calculated," +
                    $" status {rententionJobDetails.Status}," +
                    $" size {rententionJobDetails.Size}.");
            }
            else
            {
                _logger.Error($"Calculation failed, status {rententionJobDetails.Status}, comment \"{rententionJobDetails.Comment}\".");
            }
            _reportManager.Increase();
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful)
            {
                _hasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Skipped)
            {
                _hasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Exception)
            {
                _hasCompleteNode = true;
                _hasErrorNode = true;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                _hasErrorNode = true;
            }
        }

        private void DecryptSecretForGoogleStorage(APStorageCostEvaluationJobInfo jobInfo)
        {
            if (jobInfo is not null)
            {
                if (jobInfo.SourceDevice is not null)
                    DecryptGoogleStorageSecret(jobInfo.SourceDevice);
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
    }
}
