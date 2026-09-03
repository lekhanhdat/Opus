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



using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using Azure.Storage.Blobs;
using RAFileSystemCore.ReportSerializer;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public class JobContext : SingletonBase<JobContext>
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public JobContext()
        {
        }
        static object locker = new object();
        public static JobContext Current
        {
            get
            {
                return GetInstance();
            }
        }
        private STaskManager staskManager;
        public ProgressManager mProgressManager { get; set; }
        public ReportManager<JMJobDetails> JobDetailManager { get; set; }
        public ReportManager<BaseReport> ReportManager { get; set; }
        public JobSummaryService JobSummaryService { get; set; }
        public string JobMessage { get; set; }        
        private BaseJobDto jobDto = new BaseJobDto();
        public bool HasErrorNode { get; set; }
        public bool AllErrorNode { get; set; }
        public int Count { get; set; }
        public DateTime JobStartTime { get; set; }
        public bool BulkImportEnabled { get; set; }
        public int BulkSize { get; set; }
        public HybridApiClient ApiClient { get { return HybridApiClient.Instance; } }
        public string FSStubNameFormat { get; set; }
        public string JobId { get; set; }
        public bool DisposalScanFinish { get; set; }
        public bool DisposalArchiveFinish { get; set; }
        public bool SendDataToAzureTableFinish { get; set; }
        public bool SendDataToCosmosFinish { get; set; }
        public bool GetCosmosDBDataFinish { get; set; }
        public bool EnableFSHighPerformanceMode { get; set; }
        public void Init(string jobId, int type)
        {
            InitJobInfo(type, jobId);
            JobId = jobId;
            staskManager = new STaskManager();
            mProgressManager = new ProgressManager(jobId);

            var filePath = ReportUtil.GetJobReportPath(jobDto);

            var jobType = (JobType)Enum.Parse(typeof(JobType), type.ToString());
            ReportSerializer.Instance.Register(jobType, filePath, jobId);
            staskManager.Insert("JobProgess", mProgressManager.NotifyManager);

            ReportManager = new ReportManager<BaseReport>(ReportSerializer.Instance.SyncReport);
            staskManager.Insert("JobReport", ReportManager.NotifyManager);

            JobDetailManager = new ReportManager<JMJobDetails>(ReportSerializer.Instance.SyncDetail);
            staskManager.Insert("JobDetail", JobDetailManager.NotifyManager);

            staskManager.StartSchedule();
            JobSummaryService = new JobSummaryService();
            try
            {
                JobSummaryService.NotifyManager((int)JobStatus.InProgress, jobId);
                JobContext.Current.ApiClient.UpdateJobProgress(new HBJobStatusInfo() { JobId = jobId, Progress = 2 });
            }
            catch
            { }
            //JobMessage = msg;
            JobStartTime = DateTime.UtcNow;
            FSStubNameFormat = GetFSAStubNameFormat();
        }

        private void InitJobInfo(int type, string jobId)
        {
            jobDto.JobType = type;
            //jobDto.Category = msg.Job.Category;
           // jobDto.PlanId = msg.Job.PlanId;
            jobDto.Id = jobId;
        }

        private string GetFSAStubNameFormat()
        {
            string fSAStubNameFormat = "stub.html";
            try
            {
                string value = CommonConfiguration.getConfig(HybridAppSettingKey.FSAStubNameFormat).ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    fSAStubNameFormat = value;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Can not Get FSAStubNameFormat, use the default stub.html. Message: {0}." + ex.ToString());
            }
            logger.Info("fSAStubNameFormat value is :{0}", fSAStubNameFormat.ToString());
            return fSAStubNameFormat;
        }

        /// <summary>
        /// send all cached items to manager
        /// </summary>
        public void Cleanup()
        {
            logger.Info("Clean up all job progress thread");
            mProgressManager.FinalNotifyManager();
            ReportManager.FinalNotifyManager();
            JobDetailManager.FinalNotifyManager();

            staskManager.StopSchedule();

            UploadReport();

            UploadJobDetail().GetAwaiter().GetResult();
        }


        private void UploadReport()
        {
            using (new AgentPerformanceScope("ReportManager.UploadReport"))
            {
                try
                {
                    HBReportInfo reportInfo = new HBReportInfo() { JobId = jobDto.Id, JobType = jobDto.JobType };
                    var filePath = ReportUtil.GetJobReportPath(jobDto);
                    if (File.Exists(filePath))
                    {
                        reportInfo.FileName = Path.GetFileName(filePath);
                        var blockBytes = 1024 * 1024; //1M
                        using (var file = File.OpenRead(filePath))
                        {
                            int bytesRead;
                            var buffer = new byte[blockBytes];
                            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                var newArry = new Span<byte>(buffer, 0, bytesRead).ToArray();
                                reportInfo.File = newArry;
                                ApiClient.SendReport(reportInfo);
                                logger.Info($"uplaod report append, {bytesRead}");
                            }
                        }
                    }
                    else
                    {
                        logger.Warn("Cannot find report file.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"error occurred while upload report: {ex.ToString()}");
                    throw;
                }
              
            }
        }

        private async Task UploadJobDetail()
        {
            try
            {
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
                var agentInfor = ApiClient.GetAgentInformation(new AgentInfo
                {
                    AgentId = new Guid(agentId),
                    TenantId = tenantId,
                });
                if (agentInfor != null && agentInfor.CollectLog == false)
                {
                    logger.Info($"Agent Id {agentId} is disable collect log");
                    return;
                }
                var agentLogSaSResponse = ApiClient.GetAgentLogUploadSas(new Hybrid.Contract.DTOs.AgentLogSaSRequest
                {
                    AgentId = agentId,
                    TenantId = tenantId,
                    AgentLogCategory = Hybrid.Contract.DTOs.AgentLogCategory.AgentJob
                });

                
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                basePath = basePath.Substring(0, basePath.TrimEnd(new char[] { '\\' }).LastIndexOf("\\"));
                basePath = $"{basePath}\\Logs\\Jobs";
                var logFiles = Directory.GetFiles(basePath, $"*{jobDto.Id}*.log");
                var containerClient = new BlobContainerClient(new Uri(agentLogSaSResponse.SasUrl));

                var tasks = logFiles.Select(async filePath =>
                {
                    var fileName = Path.GetFileName(filePath);
                    var blobClient = containerClient.GetBlobClient($"{agentLogSaSResponse.PathPrefix}/{fileName}");

                    logger.Info($"Uploading log '{filePath}' to blob '{blobClient.Name}'.");

                    var uploadOptions = new Azure.Storage.Blobs.Models.BlobUploadOptions
                    {
                        AccessTier = Azure.Storage.Blobs.Models.AccessTier.Cool
                    };
                    using (var stream = File.OpenRead(filePath))
                    {
                        await blobClient.UploadAsync(stream, options: uploadOptions).ConfigureAwait(false);
                    }
                });
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error($"Uploaded faild: {ex.Message}");
            }
            
        }



    }
}
