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
using Aspose.Pdf.Operators;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object.Compare;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.JobMonitor.AuditHandler;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    [Audit]
    public class JobMonitorDetailDownloadWorker : IJobMonitorDetailDownloadWorker
    {
        private readonly CommonUtil.RALogger logger = CommonUtil.RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<int, AbstractJobDetailWorker> jobTypeAndJobDetailWorkerDictionary { set; get; }
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private bool _isDownloadJobReports { get; set; }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.DownloadJobDetails, AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task<FileTransferStream> GenerateDetailReportAsync(List<BaseJobDto> jobInfos, bool isDownloadJobReports = false)
        {
            _isDownloadJobReports = isDownloadJobReports;
            try
            {
                logger.Debug("Start GenerateDetailReport...");
                FileTransferStream resultStream = null;
                jobInfos = jobInfos.OrderBy(item => item.JobType).ToList<BaseJobDto>();
                if (jobInfos.IsNullOrEmpty()) { return resultStream; }
                string baseFolder = JobReportUtility.GetDownloadJobMonitorDetailTempleFolder(Guid.NewGuid().ToString());
                foreach (BaseJobDto baseJobDto in jobInfos)
                {
                    logger.Debug("GenerateDetailReport job id: {0}", baseJobDto.Id);
                    await GenerateSingleAsync(baseFolder, baseJobDto, isDownloadJobReports);
                }
                logger.Debug("export excel finished:{0}", baseFolder);
                ZipUtil.ZipFolder(baseFolder, baseFolder + JobMonitorConstants.ZIP, Encoding.UTF8);
                logger.Debug("zip file finished:{0}", baseFolder);
                resultStream = new FileTransferStream(baseFolder + ".zip", baseFolder, FileMode.Open);
                return resultStream;
            }
            catch (Exception e)
            {
                logger.Warn("export error: {0}", e.ToString());
                return null;
            }
        }

        public async System.Threading.Tasks.Task GenerateSingleAsync(string baseFolder, BaseJobDto baseJobDto, bool isDownloadJobReports = false)
        {
            logger.Info($"Start GenerateSingleAsync, jobId: {baseJobDto.Id}, jobType: {baseJobDto.JobType}, isDownloadJobReports: {isDownloadJobReports}");
            if (baseJobDto.JobType == (int)JobType.DisposalActivityManagement || baseJobDto.JobType == (int)JobType.MigrationDisposalActivityManagement)
            {
                if (TenantService.IsNewOpusTenant())
                {
                    await ExportMigrationDisposalJobAsync(baseFolder, baseJobDto.Id);
                }
                else
                {
                    await ExportDisposalJobAsync(baseFolder, baseJobDto.Id);
                }
            }
            else if (baseJobDto.Id.Contains("_"))
            {
                logger.Info($"this job is export need to delete orphan job,id:{baseJobDto.Id}");
                AbstractJobDetailWorker worker = null;
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                }
                if (worker == null)
                {
                    logger.Warn("worker not found, worker type:{0}", baseJobDto.JobType);
                    return;
                }
                GenerateExcelForSoJobThatHasOrphanDatasDetail(worker, baseJobDto, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadDetail"), baseJobDto.Id), false, isDownloadJobReports);
            }
            else
            {
                await GenerateSummaryExcelAsync(baseJobDto, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadSummary"), baseJobDto.Id));
                AbstractJobDetailWorker worker = null;
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                }
                if (baseJobDto.JobType == (int)JobType.BCSTermUsageReport || baseJobDto.JobType == (int)JobType.EXOTermUsageReport || baseJobDto.JobType == (int)JobType.PhysicalTermUsageReport || baseJobDto.JobType == (int)JobType.FSBCSTermUsageReport || baseJobDto.JobType == (int)JobType.OneDriveTermUsageReport || baseJobDto.JobType == (int)JobType.BoxBCSTermUsageReport || baseJobDto.JobType == (int)JobType.TeamsBCSTermUsageReport)
                {
                    GenerateExcelForDifferentJobDetail(worker, baseJobDto, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadTermSelection"), baseJobDto.Id), true, isDownloadJobReports);
                }
                if (worker == null)
                {
                    logger.Warn("worker not found, worker type:{0}", baseJobDto.JobType);
                    return;
                }
                GenerateExcelForDifferentJobDetail(worker, baseJobDto, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadDetail"), baseJobDto.Id), false, isDownloadJobReports);
            }
        }

        public async Task<string> ExportDisposalJobAsync(string baseFolder, string baseJobId)
        {
            //JobMonitorService.UpdateJobProgress(jobId, 15);
            //string baseFolder = JobReportUtility.GetDownloadJobMonitorDetailTempleFolder(Guid.NewGuid().ToString());
            await GenerateSummaryExcelAsync(new BaseJobDto() { JobType = (int)JobType.DisposalActivityManagement, Id = baseJobId }, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadSummary"), baseJobId));
            string reportFileFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(baseFolder, baseJobId);
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                reportFileFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadDetail"), baseJobId) + ".xlsx");
            if (!Directory.Exists(reportFileFolder)) { Directory.CreateDirectory(reportFileFolder); }

            #region Queue
            logger.Debug("GenerateDetailReport --Queue Start");
            var client = new DAOAPIClientV1();
            var dbjobs = JobMonitorService.GetJobByRECOID(baseJobId);
            var scanJob = dbjobs.Where(j => j.Type == (int)JobTypes.ArchiverScan || j.Type == (int)JobTypes.ExchangeArchiverScan).FirstOrDefault();
            var backupJob = dbjobs.Where(j => j.Type == (int)JobTypes.ArchiverBackup || j.Type == (int)JobTypes.ExchangeArchiverBackup).FirstOrDefault();
            var physicalJob = dbjobs.Where(j => j.Type == (int)JobTypes.PhysicalRecords).FirstOrDefault();

            int order = 1;
            string[][] queueDatas = new string[dbjobs.Count + 1][];
            queueDatas[0] = new string[7];
            queueDatas[0][0] = I18NEntity.GetString("RM_JS_JM_JobOrder");
            queueDatas[0][1] = I18NEntity.GetString("RM_JS_JM_JobID");
            queueDatas[0][2] = I18NEntity.GetString("RM_JS_JM_Module");
            queueDatas[0][3] = I18NEntity.GetString("RM_JS_JM_Progress");
            queueDatas[0][4] = I18NEntity.GetString("RM_JS_JM_Status");
            queueDatas[0][5] = I18NEntity.GetString("RM_JS_JM_StartTime");
            queueDatas[0][6] = I18NEntity.GetString("RM_JS_JM_EndTime");
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in dbjobs)
            {
                queueDatas[order] = new string[7];
                queueDatas[order][0] = order.ToString();
                queueDatas[order][1] = job.Id;
                queueDatas[order][2] = I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)job.Type).ToString());
                queueDatas[order][3] = ((int)job.Progress).ToString();
                queueDatas[order][4] = ConvertJobStatusToString(AvePoint.RA.Service.JobMonitor.JobMonitorService.ConvertToRAStatus(job.State));
                queueDatas[order][5] = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime;
                queueDatas[order][6] = job.FinishTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.FinishTime, true).SimplifyFormatTime;
                order++;
            }
            ReportUtil.CreateExcel(reportFilePath, I18NEntity.GetString("RM_JM_Export_JobInfo"), queueDatas);
            #endregion

            if (scanJob != null)
            {
                #region Scan Summary
                logger.Debug("GenerateDetailReport --Scan Summary Start");
                var scanSummary = client.JobSummary(scanJob);
                var scanSummaryTotalLength = 0;
                foreach (var item in scanSummary.SummaryItem)
                {
                    scanSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] scanSummaryDatas = new string[scanSummaryTotalLength][];
                var scanSummaryIndex = 0;
                foreach (var item in scanSummary.SummaryItem)
                {
                    scanSummaryDatas[scanSummaryIndex] = new string[2];
                    scanSummaryDatas[scanSummaryIndex][0] = item.Title + ":";
                    scanSummaryDatas[scanSummaryIndex][1] = string.Empty;
                    scanSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "Finish Time")
                        {
                            rowValue = GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(row.Value), true).SimplifyFormatTime;
                        }
                        if (row.Key == "Scope" || row.Key == "範囲")
                        {
                            rowValue = DefaultSecurityContainerNameHelper.GetI18NName(row.Value);
                        }
                        scanSummaryDatas[scanSummaryIndex] = new string[2];
                        scanSummaryDatas[scanSummaryIndex][0] = row.Key;
                        scanSummaryDatas[scanSummaryIndex][1] = rowValue;
                        scanSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobSummary"), scanSummaryDatas);
                #endregion

                #region Scan Job Details
                logger.Debug("GenerateDetailReport --Scan Details Start");
                int getScanSize = 1000;
                int getScanCount = 0;
                int scanStartIndex = 0;
                int scanTotalCount = 0;
                List<JobDetailDto> sanAllResult = new List<JobDetailDto>();
                do
                {
                    JobDetailInfos scanDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", scanStartIndex)))
                    {
                        scanDetails = client.JobDetails(new ArchiverJobDto() { Id = scanJob.Id, JobType = scanJob.Type, JobCategory = scanJob.Category, PlanId = scanJob.PlanId },
                        new List<string>() { scanJob.Id }, string.Empty, scanStartIndex, getScanSize, new int[] { 0, 1, 2 }, new int[] { });
                    }
                    if (scanDetails != null)
                    {
                        scanTotalCount = scanDetails.TotalLength;
                        sanAllResult.AddRange(scanDetails.Values);
                    }
                    scanStartIndex = getScanSize * ++getScanCount;
                } while (scanStartIndex < scanTotalCount);
                IOrderedEnumerable<JobDetailDto> orderScanAllResult;
                using (PerformanceScope sc = new PerformanceScope("Sort all result"))
                {
                    orderScanAllResult = sanAllResult.OrderBy(j => (j as SOJobDetailDto).EntityType);
                }
                logger.Debug("Ordered scan detail count:{0}", orderScanAllResult.Count());
                var scanSheetSize = 5000;
                var sacnSheetCount = scanTotalCount % scanSheetSize == 0 ? scanTotalCount / scanSheetSize : (scanTotalCount / scanSheetSize) + 1;
                for (int i = 1; i < sacnSheetCount + 1; i++)
                {
                    var sheetDatas = orderScanAllResult.Skip((i - 1) * scanSheetSize).Take(scanSheetSize);
                    var scanDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertScanDetailsToArrayAsync(scanDetailsDatas, sheetDatas);
                    if (sacnSheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobDetails"), scanDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobDetails") + i.ToString(), scanDetailsDatas);
                    }
                    logger.Debug("Insert scan detail count:{0}", sheetDatas.Count());
                }
                #endregion
            }

            if (backupJob != null)
            {
                #region Backup Summary 
                logger.Debug("GenerateDetailReport --Backup Summary Start");
                var backupSummary = client.JobSummary(backupJob);
                var backupSummaryTotalLength = 0;
                foreach (var item in backupSummary.SummaryItem)
                {
                    backupSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] backupSummaryDatas = new string[backupSummaryTotalLength][];
                var backupSummaryIndex = 0;
                foreach (var item in backupSummary.SummaryItem)
                {
                    backupSummaryDatas[backupSummaryIndex] = new string[2];
                    backupSummaryDatas[backupSummaryIndex][0] = item.Title + ":";
                    backupSummaryDatas[backupSummaryIndex][1] = string.Empty;
                    backupSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "Finish Time")
                        {
                            rowValue = GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(row.Value), true).SimplifyFormatTime;
                        }
                        if (row.Key == "Scope" || row.Key == "範囲")
                        {
                            rowValue = DefaultSecurityContainerNameHelper.GetI18NName(row.Value);
                        }
                        backupSummaryDatas[backupSummaryIndex] = new string[2];
                        backupSummaryDatas[backupSummaryIndex][0] = row.Key;
                        backupSummaryDatas[backupSummaryIndex][1] = rowValue;
                        backupSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobSummary"), backupSummaryDatas);


                #endregion

                #region Archiver Job Details
                logger.Debug("GenerateDetailReport --Backup Details Start");
                int getSize = 1000;
                int getCount = 0;
                int startIndex = 0;
                int totalCount = 0;
                List<JobDetailDto> allResult = new List<JobDetailDto>();
                do
                {
                    JobDetailInfos backupDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", startIndex)))
                    {
                        backupDetails = client.JobDetails(new ArchiverJobDto() { Id = backupJob.Id, JobType = backupJob.Type, JobCategory = backupJob.Category, PlanId = backupJob.PlanId },
                        new List<string>() { backupJob.Id }, string.Empty, startIndex, getSize, new int[] { 0, 1, 2 }, new int[] { });
                    }
                    if (backupDetails != null)
                    {
                        totalCount = backupDetails.TotalLength;
                        allResult.AddRange(backupDetails.Values);
                    }
                    startIndex = getSize * ++getCount;
                } while (startIndex < totalCount);
                IOrderedEnumerable<JobDetailDto> orderAllResult;
                using (PerformanceScope sc = new PerformanceScope("Sort all result"))
                {
                    orderAllResult = allResult.OrderBy(j => (j as SOJobDetailDto).EntityType);
                }
                logger.Debug("Ordered backup detail count:{0}", orderAllResult.Count());
                var sheetSize = 5000;
                var sheetCount = totalCount % sheetSize == 0 ? totalCount / sheetSize : (totalCount / sheetSize) + 1;
                for (int i = 1; i < sheetCount + 1; i++)
                {
                    var sheetDatas = orderAllResult.Skip((i - 1) * sheetSize).Take(sheetSize);
                    var archiverDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertBackupDetailsToArrayAsync(archiverDetailsDatas, sheetDatas);
                    if (sheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobDetails"), archiverDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobDetails") + i.ToString(), archiverDetailsDatas);
                    }
                    logger.Debug("Insert backup detail count:{0}", sheetDatas.Count());
                }
                #endregion
            }

            if (physicalJob != null)
            {
                #region physicalJob Summary 
                logger.Debug("GenerateDetailReport --physicalJob Summary Start");
                var physicalSummary = client.JobSummary(physicalJob);
                var physicalSummaryTotalLength = 0;
                foreach (var item in physicalSummary.SummaryItem)
                {
                    physicalSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] physicalSummaryDatas = new string[physicalSummaryTotalLength][];
                var physicalSummaryIndex = 0;
                foreach (var item in physicalSummary.SummaryItem)
                {
                    physicalSummaryDatas[physicalSummaryIndex] = new string[2];
                    physicalSummaryDatas[physicalSummaryIndex][0] = GetRecordsPhysicalSummaryTitle(item.Title) + ":";
                    physicalSummaryDatas[physicalSummaryIndex][1] = string.Empty;
                    physicalSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "Finish Time")
                        {
                            rowValue = GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(row.Value), true).SimplifyFormatTime;
                        }
                        physicalSummaryDatas[physicalSummaryIndex] = new string[2];
                        physicalSummaryDatas[physicalSummaryIndex][0] = row.Key;
                        physicalSummaryDatas[physicalSummaryIndex][1] = rowValue;
                        physicalSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobSummary"), physicalSummaryDatas);
                logger.Debug("GenerateDetailReport --physicalJob Summary End");
                #endregion

                #region Physical Job Details
                logger.Debug("GenerateDetailReport --Physical Details Start");
                int getSize = 1000;
                int getCount = 0;
                int startIndex = 0;
                int totalCount = 0;
                List<JobDetailDto> allResult = new List<JobDetailDto>();
                do
                {
                    JobDetailInfos physicalDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", startIndex)))
                    {
                        physicalDetails = client.JobDetails(new ArchiverJobDto() { Id = physicalJob.Id, JobType = physicalJob.Type, JobCategory = physicalJob.Category, PlanId = physicalJob.PlanId },
                        new List<string>() { physicalJob.Id }, string.Empty, startIndex, getSize, new int[] { 0, 1, 2 }, new int[] { });
                    }
                    if (physicalDetails != null)
                    {
                        totalCount = physicalDetails.TotalLength;
                        allResult.AddRange(physicalDetails.Values);
                    }
                    startIndex = getSize * ++getCount;
                } while (startIndex < totalCount);
                IOrderedEnumerable<JobDetailDto> orderAllResult;
                using (PerformanceScope sc = new PerformanceScope("Sort all result"))
                {
                    orderAllResult = allResult.OrderBy(j => (j as SOJobDetailDto).EntityType);
                }

                var sheetSize = 5000;
                var sheetCount = totalCount % sheetSize == 0 ? totalCount / sheetSize : (totalCount / sheetSize) + 1;
                for (int i = 1; i < sheetCount + 1; i++)
                {
                    var sheetDatas = orderAllResult.Skip((i - 1) * sheetSize).Take(sheetSize);
                    var physicalDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertPhysicalDetailsToArrayAsync(physicalDetailsDatas, sheetDatas);
                    if (sheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobDetails"), physicalDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobDetails") + i.ToString(), physicalDetailsDatas);
                    }
                }
                logger.Debug("GenerateDetailReport --Physical Details End");
                #endregion
            }
            return string.Empty;
        }

        public async Task<string> ExportMigrationDisposalJobAsync(string baseFolder, string baseJobId)
        {
            await GenerateSummaryExcelAsync(new BaseJobDto() { JobType = (int)JobType.DisposalActivityManagement, Id = baseJobId }, baseFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadSummary"), baseJobId));
            string reportFileFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(baseFolder, baseJobId);
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                reportFileFolder, string.Format(I18NEntity.GetString("RM_JM_DownLoadDetail"), baseJobId) + ".xlsx");
            if (!Directory.Exists(reportFileFolder)) { Directory.CreateDirectory(reportFileFolder); }

            #region Queue
            logger.Debug("GenerateDetailReport --Queue Start");
            var dbjobs = JobMonitorService.GetJobByRECOID(baseJobId);
            var scanJob = dbjobs.Where(j => j.Type == (int)JobType.ArchiverScan || j.Type == (int)JobType.MigrationArchiverScan || j.Type == (int)JobType.ExchangeArchiverScan).FirstOrDefault();
            var backupJob = dbjobs.Where(j => j.Type == (int)JobType.ArchiverBackup || j.Type == (int)JobType.MigrationArchiverBackup || j.Type == (int)JobType.ExchangeArchiverBackup).FirstOrDefault();
            var physicalJob = dbjobs.Where(j => j.Type == (int)JobType.PhysicalDisposal).FirstOrDefault();

            int order = 1;
            string[][] queueDatas = new string[dbjobs.Count + 1][];
            queueDatas[0] = new string[7];
            queueDatas[0][0] = I18NEntity.GetString("RM_JS_JM_JobOrder");
            queueDatas[0][1] = I18NEntity.GetString("RM_JS_JM_JobID");
            queueDatas[0][2] = I18NEntity.GetString("RM_JS_JM_Module");
            queueDatas[0][3] = I18NEntity.GetString("RM_JS_JM_Progress");
            queueDatas[0][4] = I18NEntity.GetString("RM_JS_JM_Status");
            queueDatas[0][5] = I18NEntity.GetString("RM_JS_JM_StartTime");
            queueDatas[0][6] = I18NEntity.GetString("RM_JS_JM_EndTime");
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in dbjobs)
            {
                queueDatas[order] = new string[7];
                queueDatas[order][0] = order.ToString();
                queueDatas[order][1] = job.Id;
                queueDatas[order][2] = I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)job.Type).ToString());
                queueDatas[order][3] = ((int)job.Progress).ToString();
                queueDatas[order][4] = ConvertJobStatusToString(AvePoint.RA.Service.JobMonitor.JobMonitorService.ConvertToRAStatus(job.State));
                queueDatas[order][5] = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime;
                queueDatas[order][6] = job.FinishTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.FinishTime, true).SimplifyFormatTime;
                order++;
            }
            ReportUtil.CreateExcel(reportFilePath, I18NEntity.GetString("RM_JM_Export_JobInfo"), queueDatas);
            #endregion
            var registerEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
            if (scanJob != null)
            {
                #region Scan Summary
                logger.Debug("GenerateDetailReport --Scan Summary Start");
                var scanSummary = (await JobMonitorService.GetDAOJobSummaryDetailsAsync(scanJob.Id, scanJob.Type)).DisposalSummary;
                var scanSummaryTotalLength = 0;
                foreach (var item in scanSummary.SummaryItem)
                {
                    scanSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] scanSummaryDatas = new string[scanSummaryTotalLength][];
                var scanSummaryIndex = 0;
                foreach (var item in scanSummary.SummaryItem)
                {
                    scanSummaryDatas[scanSummaryIndex] = new string[2];
                    scanSummaryDatas[scanSummaryIndex][0] = item.Title + ":";
                    scanSummaryDatas[scanSummaryIndex][1] = string.Empty;
                    scanSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        scanSummaryDatas[scanSummaryIndex] = new string[2];
                        scanSummaryDatas[scanSummaryIndex][0] = row.Key;
                        scanSummaryDatas[scanSummaryIndex][1] = row.Value;
                        scanSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobSummary"), scanSummaryDatas);
                #endregion


                #region Scan Job Details
                logger.Debug("GenerateDetailReport --Scan Details Start");
                int getScanSize = 1000;
                int getScanCount = 0;
                int scanStartIndex = 0;
                int scanTotalCount = 0;
                List<JMJobDetails> sanAllResult = new List<JMJobDetails>();
                do
                {
                    IEnumerable<JMJobDetails> scanDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", scanStartIndex)))
                    {
                        BaseJobDto baseJobDto = new BaseJobDto();
                        baseJobDto.Id = scanJob.Id;
                        baseJobDto.JobType = scanJob.Type;
                        baseJobDto.Category = scanJob.Category;
                        baseJobDto.PlanId = scanJob.PlanId;
                        baseJobDto.TenantGroupEmail = registerEmail;

                        AbstractJobDetailWorker worker = null;
                        if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                        {
                            worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                        }
                        scanDetails = worker.GetData(JobMonitorConstants.MAX_COUNT_ONE_SHEET, scanStartIndex, ref scanTotalCount, null, baseJobDto);
                        if (scanDetails != null)
                        {
                            sanAllResult.AddRange(scanDetails);
                        }
                    }
                    scanStartIndex = getScanSize * ++getScanCount;
                } while (scanStartIndex < scanTotalCount);

                logger.Debug("Scan detail count:{0}", sanAllResult.Count);
                var scanSheetSize = 5000;
                var sacnSheetCount = scanTotalCount % scanSheetSize == 0 ? scanTotalCount / scanSheetSize : (scanTotalCount / scanSheetSize) + 1;
                for (int i = 1; i < sacnSheetCount + 1; i++)
                {
                    var sheetDatas = sanAllResult.Skip((i - 1) * scanSheetSize).Take(scanSheetSize);
                    var scanDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertScanDetailsToArrayAsync(scanDetailsDatas, ConvertToSOJobDetails(sheetDatas));
                    if (sacnSheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobDetails"), scanDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ScanJobDetails") + i.ToString(), scanDetailsDatas);
                    }
                    logger.Debug("Insert scan detail count:{0}", sheetDatas.Count());
                }
                #endregion
            }

            if (backupJob != null)
            {
                #region Backup Summary 
                logger.Debug("GenerateDetailReport --Backup Summary Start");
                var backupSummary = (await JobMonitorService.GetDAOJobSummaryDetailsAsync(backupJob.Id, backupJob.Type)).DisposalSummary;
                var backupSummaryTotalLength = 0;
                foreach (var item in backupSummary.SummaryItem)
                {
                    backupSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] backupSummaryDatas = new string[backupSummaryTotalLength][];
                var backupSummaryIndex = 0;
                foreach (var item in backupSummary.SummaryItem)
                {
                    backupSummaryDatas[backupSummaryIndex] = new string[2];
                    backupSummaryDatas[backupSummaryIndex][0] = item.Title + ":";
                    backupSummaryDatas[backupSummaryIndex][1] = string.Empty;
                    backupSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        backupSummaryDatas[backupSummaryIndex] = new string[2];
                        backupSummaryDatas[backupSummaryIndex][0] = row.Key;
                        backupSummaryDatas[backupSummaryIndex][1] = row.Value;
                        backupSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobSummary"), backupSummaryDatas);

                #endregion

                #region Archiver Job Details
                logger.Debug("GenerateDetailReport --Backup Details Start");
                int getSize = 1000;
                int getCount = 0;
                int startIndex = 0;
                int totalCount = 0;
                List<JMJobDetails> allResult = new List<JMJobDetails>();
                do
                {
                    IEnumerable<JMJobDetails> backupDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", startIndex)))
                    {
                        BaseJobDto baseJobDto = new BaseJobDto();
                        baseJobDto.Id = backupJob.Id;
                        baseJobDto.JobType = backupJob.Type;
                        baseJobDto.Category = backupJob.Category;
                        baseJobDto.PlanId = backupJob.PlanId;
                        baseJobDto.TenantGroupEmail = registerEmail;

                        AbstractJobDetailWorker worker = null;
                        if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                        {
                            worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                        }
                        backupDetails = worker.GetData(JobMonitorConstants.MAX_COUNT_ONE_SHEET, startIndex, ref totalCount, null, baseJobDto);
                        if (backupDetails != null)
                        {
                            allResult.AddRange(backupDetails);
                        }
                    }
                    startIndex = getSize * ++getCount;
                } while (startIndex < totalCount);
                logger.Debug("Backup detail count:{0}", allResult.Count);
                var sheetSize = 5000;
                var sheetCount = totalCount % sheetSize == 0 ? totalCount / sheetSize : (totalCount / sheetSize) + 1;
                for (int i = 1; i < sheetCount + 1; i++)
                {
                    var sheetDatas = allResult.Skip((i - 1) * sheetSize).Take(sheetSize);
                    var archiverDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertBackupDetailsToArrayAsync(archiverDetailsDatas, ConvertToSOJobDetails(sheetDatas));
                    if (sheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobDetails"), archiverDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_ArchiverJobDetails") + i.ToString(), archiverDetailsDatas);
                    }
                    logger.Debug("Insert backup detail count:{0}", sheetDatas.Count());
                }
                #endregion
            }

            if (physicalJob != null)
            {
                #region physicalJob Summary 
                logger.Debug("GenerateDetailReport --physicalJob Summary Start");
                var physicalSummary = (await JobMonitorService.GetDAOJobSummaryDetailsAsync(physicalJob.Id, physicalJob.Type)).DisposalSummary;
                var physicalSummaryTotalLength = 0;
                foreach (var item in physicalSummary.SummaryItem)
                {
                    physicalSummaryTotalLength += item.SummaryRow.Count + 1;
                }
                string[][] physicalSummaryDatas = new string[physicalSummaryTotalLength][];
                var physicalSummaryIndex = 0;
                foreach (var item in physicalSummary.SummaryItem)
                {
                    physicalSummaryDatas[physicalSummaryIndex] = new string[2];
                    physicalSummaryDatas[physicalSummaryIndex][0] = GetRecordsPhysicalSummaryTitle(item.Title) + ":";
                    physicalSummaryDatas[physicalSummaryIndex][1] = string.Empty;
                    physicalSummaryIndex++;
                    foreach (var row in item.SummaryRow)
                    {
                        physicalSummaryDatas[physicalSummaryIndex] = new string[2];
                        physicalSummaryDatas[physicalSummaryIndex][0] = row.Key;
                        physicalSummaryDatas[physicalSummaryIndex][1] = row.Value;
                        physicalSummaryIndex++;
                    }
                }
                ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobSummary"), physicalSummaryDatas);
                logger.Debug("GenerateDetailReport --physicalJob Summary End");
                #endregion

                #region Physical Job Details
                logger.Debug("GenerateDetailReport --Physical Details Start");
                int getSize = 1000;
                int getCount = 0;
                int startIndex = 0;
                int totalCount = 0;
                List<JMJobDetails> allResult = new List<JMJobDetails>();
                do
                {
                    IEnumerable<JMJobDetails> physicalDetails = null;
                    using (PerformanceScope sc = new PerformanceScope(string.Format("Get Details startIndex:{0}", startIndex)))
                    {
                        BaseJobDto baseJobDto = new BaseJobDto();
                        baseJobDto.Id = physicalJob.Id;
                        baseJobDto.JobType = physicalJob.Type;
                        baseJobDto.Category = physicalJob.Category;
                        baseJobDto.PlanId = physicalJob.PlanId;
                        baseJobDto.TenantGroupEmail = registerEmail;

                        AbstractJobDetailWorker worker = null;
                        if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                        {
                            worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                        }
                        physicalDetails = worker.GetData(JobMonitorConstants.MAX_COUNT_ONE_SHEET, startIndex, ref totalCount, null, baseJobDto);
                    }
                    if (physicalDetails != null)
                    {
                        allResult.AddRange(physicalDetails);
                    }
                    startIndex = getSize * ++getCount;
                } while (startIndex < totalCount);

                var sheetSize = 5000;
                var sheetCount = totalCount % sheetSize == 0 ? totalCount / sheetSize : (totalCount / sheetSize) + 1;
                for (int i = 1; i < sheetCount + 1; i++)
                {
                    var sheetDatas = allResult.Skip((i - 1) * sheetSize).Take(sheetSize);
                    var physicalDetailsDatas = new string[sheetDatas.Count() + 1][];
                    await ConvertPhysicalDetailsToArrayAsync(physicalDetailsDatas, ConvertToJobDetails4Phy(sheetDatas));
                    if (sheetCount == 1)
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobDetails"), physicalDetailsDatas);
                    }
                    else
                    {
                        ReportUtil.InsertWorksheet(reportFilePath, I18NEntity.GetString("RM_JM_Export_PhysicalJobDetails") + i.ToString(), physicalDetailsDatas);
                    }
                }
                logger.Debug("GenerateDetailReport --Physical Details End");
                #endregion
            }
            return string.Empty;
        }

        private static List<SOJobDetailDto> ConvertToSOJobDetails(IEnumerable<JMJobDetails> sheetDatas)
        {
            List<SOJobDetailDto> sheetDtoData = new();
            foreach (var soDetail in sheetDatas.Cast<JMDisposalJobDetails>())
            {
                sheetDtoData.Add(new SOJobDetailDto()
                {
                    Action = soDetail.Action,
                    Comment = soDetail.Comment,
                    DataOperation = soDetail.Action,
                    EntityType = soDetail.EntityType,
                    Date = soDetail.Date,
                    RuleName = soDetail.RuleName,
                    Size = soDetail.Size,
                    SrcURL = soDetail.SourceURL,
                    Status = soDetail.StatusStr,
                    Type = soDetail.Type,
                });
            }

            return sheetDtoData;
        }

        private static List<SOJobDetailDto> ConvertToJobDetails4Phy(IEnumerable<JMJobDetails> sheetDatas)
        {
            List<SOJobDetailDto> sheetDtoData = new();
            foreach (var soDetail in sheetDatas.Cast<JMPhysicalDisposalJobDetails>())
            {
                sheetDtoData.Add(new SOJobDetailDto()
                {
                    EntityType = (int)JobReportDetailEntityType.ArchiveDeletion,
                    MediaHost = soDetail.ObjectName,
                    SrcURL = soDetail.FullPath,
                    Action = soDetail.ActionType,
                    Type = soDetail.ItemType,
                    RuleName = soDetail.RuleName,
                    Comment = soDetail.Comment,
                    Status = soDetail.StatusStr,
                });
            }

            return sheetDtoData;
        }

        private string GetRecordsPhysicalSummaryTitle(string DAOTitle)
        {
            string summaryTitle = DAOTitle;
            switch (DAOTitle)
            {
                case "Deletion Statistics":
                    summaryTitle = I18NEntity.GetString("RM_JM_Summary_Title_DataDisposal");
                    break;
                case "Record Declaration Statistics":
                    summaryTitle = I18NEntity.GetString("RM_JM_Summary_Title_DataMove"); ;
                    break;
                default:
                    break;
            }
            return summaryTitle;
        }

        private async System.Threading.Tasks.Task ConvertScanDetailsToArrayAsync(string[][] scanDetailsDatas, IEnumerable<JobDetailDto> jobs)
        {
            var scanDetailsIndex = 1;
            scanDetailsDatas[0] = new string[9];
            scanDetailsDatas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            scanDetailsDatas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_BackupSourceURL");
            scanDetailsDatas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            scanDetailsDatas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            scanDetailsDatas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            scanDetailsDatas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in jobs)
            {
                var soJob = job as SOJobDetailDto;
                var entityType = (JobReportDetailEntityType)soJob.EntityType;

                scanDetailsDatas[scanDetailsIndex] = new string[6];
                scanDetailsDatas[scanDetailsIndex][0] = soJob.Type;
                scanDetailsDatas[scanDetailsIndex][1] = soJob.SrcURL;
                scanDetailsDatas[scanDetailsIndex][2] = soJob.Status;
                scanDetailsDatas[scanDetailsIndex][3] = GeneralSettingService.ConvertTiksToDateTime(gls, soJob.Date, true).SimplifyFormatTime;
                scanDetailsDatas[scanDetailsIndex][4] = (job as SOJobDetailDto)?.RuleName ?? string.Empty;
                scanDetailsDatas[scanDetailsIndex][5] = I18NEntity.GetString(soJob.Comment);
                scanDetailsIndex++;
            }
        }

        private async System.Threading.Tasks.Task ConvertBackupDetailsToArrayAsync(string[][] archiverDetailsDatas, IEnumerable<JobDetailDto> jobs)
        {
            var archiverDetailsIndex = 1;
            archiverDetailsDatas[0] = new string[9];
            archiverDetailsDatas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailsTab");
            archiverDetailsDatas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            archiverDetailsDatas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_BackupSourceURL");
            archiverDetailsDatas[0][3] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            archiverDetailsDatas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            archiverDetailsDatas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            archiverDetailsDatas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            archiverDetailsDatas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            archiverDetailsDatas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in jobs)
            {
                var soJob = job as SOJobDetailDto;
                var entityType = (JobReportDetailEntityType)soJob.EntityType;
                switch (entityType)
                {
                    case JobReportDetailEntityType.Export://Export
                        archiverDetailsDatas[archiverDetailsIndex] = new string[9];
                        archiverDetailsDatas[archiverDetailsIndex][0] = I18NEntity.GetString("RM_JS_JM_EntityType_Export");
                        archiverDetailsDatas[archiverDetailsIndex][1] = soJob.Type;
                        archiverDetailsDatas[archiverDetailsIndex][2] = soJob.SrcURL;
                        archiverDetailsDatas[archiverDetailsIndex][3] = ConvertUnitUtil.ConvertToKB(soJob.Size);
                        archiverDetailsDatas[archiverDetailsIndex][4] = soJob.Status;
                        archiverDetailsDatas[archiverDetailsIndex][5] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][6] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][7] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][8] = I18NEntity.GetString(soJob.Comment);
                        break;
                    case JobReportDetailEntityType.NormalInfo://Backup
                        archiverDetailsDatas[archiverDetailsIndex] = new string[9];
                        archiverDetailsDatas[archiverDetailsIndex][0] = I18NEntity.GetString("RM_JS_JM_EntityType_Backup");
                        archiverDetailsDatas[archiverDetailsIndex][1] = soJob.Type;
                        archiverDetailsDatas[archiverDetailsIndex][2] = soJob.SrcURL;
                        archiverDetailsDatas[archiverDetailsIndex][3] = ConvertUnitUtil.ConvertToKB(soJob.Size);
                        archiverDetailsDatas[archiverDetailsIndex][4] = soJob.Status;
                        archiverDetailsDatas[archiverDetailsIndex][5] = GeneralSettingService.ConvertTiksToDateTime(gls, soJob.Date, true).SimplifyFormatTime;
                        archiverDetailsDatas[archiverDetailsIndex][6] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][7] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][8] = I18NEntity.GetString(soJob.Comment);
                        break;
                    case JobReportDetailEntityType.ArchiveDeletion://Deletion 
                        archiverDetailsDatas[archiverDetailsIndex] = new string[9];
                        archiverDetailsDatas[archiverDetailsIndex][0] = I18NEntity.GetString("RM_JS_JM_EntityType_Deletion");
                        archiverDetailsDatas[archiverDetailsIndex][1] = soJob.Type;
                        archiverDetailsDatas[archiverDetailsIndex][2] = soJob.SrcURL;
                        archiverDetailsDatas[archiverDetailsIndex][3] = ConvertUnitUtil.ConvertToKB(soJob.Size);
                        archiverDetailsDatas[archiverDetailsIndex][4] = soJob.Status;
                        archiverDetailsDatas[archiverDetailsIndex][5] = GeneralSettingService.ConvertTiksToDateTime(gls, soJob.Date, true).SimplifyFormatTime;
                        archiverDetailsDatas[archiverDetailsIndex][6] = I18NEntity.GetString(soJob.DataOperation);
                        archiverDetailsDatas[archiverDetailsIndex][7] = string.Empty;
                        archiverDetailsDatas[archiverDetailsIndex][8] = I18NEntity.GetString(soJob.Comment);
                        break;
                    case JobReportDetailEntityType.RecordManager://Record Declaration
                        archiverDetailsDatas[archiverDetailsIndex] = new string[9];
                        archiverDetailsDatas[archiverDetailsIndex][0] = I18NEntity.GetString("RM_JS_JM_EntityType_RecordDeclaration");
                        archiverDetailsDatas[archiverDetailsIndex][1] = soJob.Type;
                        archiverDetailsDatas[archiverDetailsIndex][2] = soJob.SrcURL;
                        archiverDetailsDatas[archiverDetailsIndex][3] = ConvertUnitUtil.ConvertToKB(soJob.Size);
                        archiverDetailsDatas[archiverDetailsIndex][4] = soJob.Status;
                        archiverDetailsDatas[archiverDetailsIndex][5] = GeneralSettingService.ConvertTiksToDateTime(gls, soJob.Date, true).SimplifyFormatTime;
                        archiverDetailsDatas[archiverDetailsIndex][6] = I18NEntity.GetString(soJob.DataOperation);
                        archiverDetailsDatas[archiverDetailsIndex][7] = soJob.DestURL;
                        archiverDetailsDatas[archiverDetailsIndex][8] = I18NEntity.GetString(soJob.Comment);
                        break;
                    default:
                        break;
                }
                archiverDetailsIndex++;
            }
        }

        private async System.Threading.Tasks.Task ConvertPhysicalDetailsToArrayAsync(string[][] physicalDetailsDatas, IEnumerable<JobDetailDto> jobs)
        {
            var archiverDetailsIndex = 1;
            physicalDetailsDatas[0] = new string[7];
            physicalDetailsDatas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            physicalDetailsDatas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            physicalDetailsDatas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_LocationPath");
            physicalDetailsDatas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ActionType");
            physicalDetailsDatas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationPath");
            physicalDetailsDatas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            physicalDetailsDatas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in jobs)
            {
                var soJob = job as SOJobDetailDto;
                var entityType = (JobReportDetailEntityType)soJob.EntityType;
                switch (entityType)
                {
                    case JobReportDetailEntityType.ArchiveDeletion://Deletion 
                        physicalDetailsDatas[archiverDetailsIndex] = new string[7];
                        physicalDetailsDatas[archiverDetailsIndex][0] = soJob.MediaHost;
                        physicalDetailsDatas[archiverDetailsIndex][1] = I18NEntity.GetString(soJob.Type);
                        physicalDetailsDatas[archiverDetailsIndex][2] = ReplaceUrlWith18NValue(soJob.SrcURL);
                        physicalDetailsDatas[archiverDetailsIndex][3] = I18NEntity.GetString("RM_JS_JM_EntityType_Deletion");
                        physicalDetailsDatas[archiverDetailsIndex][4] = ReplaceUrlWith18NValue(soJob.DestURL);
                        physicalDetailsDatas[archiverDetailsIndex][5] = soJob.Status;
                        physicalDetailsDatas[archiverDetailsIndex][6] = I18NEntity.GetString(soJob.Comment);
                        break;
                    case JobReportDetailEntityType.RecordManager://Record Declaration
                        physicalDetailsDatas[archiverDetailsIndex] = new string[7];
                        physicalDetailsDatas[archiverDetailsIndex][0] = soJob.MediaHost;
                        physicalDetailsDatas[archiverDetailsIndex][1] = I18NEntity.GetString(soJob.Type);
                        physicalDetailsDatas[archiverDetailsIndex][2] = ReplaceUrlWith18NValue(soJob.SrcURL);
                        physicalDetailsDatas[archiverDetailsIndex][3] = I18NEntity.GetString("RM_JS_JM_JobType_RecordsExplorerMove");
                        physicalDetailsDatas[archiverDetailsIndex][4] = ReplaceUrlWith18NValue(soJob.DestURL);
                        physicalDetailsDatas[archiverDetailsIndex][5] = soJob.Status;
                        physicalDetailsDatas[archiverDetailsIndex][6] = I18NEntity.GetString(soJob.Comment);
                        break;
                    default:
                        break;
                }
                archiverDetailsIndex++;
            }
        }

        private string ReplaceUrlWith18NValue(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("RM_SPS_Location_RootNode"))
            {
                return url.Replace("RM_SPS_Location_RootNode", I18NEntity.GetString("RM_SPS_Location_RootNode"));
            }
            return url;
        }
        public void GenerateExcelForSoJobThatHasOrphanDatasDetail(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string BaseFolder, string excelFileName, bool isTermSelection, bool isDownloadJobReports = false)
        {
            int jobDetailTotalCount = 0;
            int allSheetCount = 0;
            string[][] datas = null;
            string reportFileFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(BaseFolder, baseJobDto.Id);
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                reportFileFolder, excelFileName + ".xlsx");
            if (!Directory.Exists(reportFileFolder)) { Directory.CreateDirectory(reportFileFolder); }
            try
            {
                if (isDownloadJobReports)
                {
                    GenerateExcelForSoJobThatHasOrphanDatasDetailByCursor(worker, baseJobDto, reportFilePath, isTermSelection);
                }
                else
                {
                    IEnumerable<JMJobDetails> reportDetailList = this.GetSoFailedJobDetailsInfo(worker, 1, baseJobDto, ref jobDetailTotalCount);
                    if (jobDetailTotalCount > 0)
                    {
                        allSheetCount = jobDetailTotalCount % JobMonitorConstants.MAX_COUNT_ONE_SHEET == 0 ? jobDetailTotalCount / JobMonitorConstants.MAX_COUNT_ONE_SHEET : jobDetailTotalCount / JobMonitorConstants.MAX_COUNT_ONE_SHEET + 1;
                        for (int index = 1; index < allSheetCount + 1; index++)
                        {
                            reportDetailList = this.GetSoFailedJobDetailsInfo(worker, index, baseJobDto, ref jobDetailTotalCount);
                            datas = new string[reportDetailList.Count() + 1][];
                            datas = ConvertJobDetailInfoToArray(baseJobDto, reportDetailList, datas, isTermSelection);

                            try
                            {
                                for (int i = 0; i < datas.Length; i++)
                                {
                                    var row = datas[i];
                                    for (int j = 0; j < row.Length; j++)
                                    {
                                        if (row[j] != null && (row[j].Contains("0x80070005") || row[j].Contains("E_ACCESSDENIED")))
                                        {
                                            datas[i][j] = I18NEntity.GetString("RM_JM_Details_Failed_AccessDenied");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Replace job comment error, {e}");
                            }

                            if (index == 1)
                            {
                                ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                            }
                            else
                            {
                                ReportUtil.InsertWorksheet(reportFilePath, "Sheet" + index, datas);
                            }
                        }
                    }
                    else
                    {
                        datas = new string[1][];
                        datas[0] = new string[] { I18NEntity.GetString("RM_JM_DownLoadNoInformationInDB") };
                        ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug("generate Detail Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
        }

        private void GenerateExcelForSoJobThatHasOrphanDatasDetailByCursor(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string reportFilePath, bool isTermSelection)
        {
            long lastRowId = 0;
            long totalCount = 0;
            int sheetIndex = 1;
            long totalRowsInCurrentFile = 0;
            int fileIndex = 0;
            string currentFilePath = reportFilePath;

            IEnumerable<JMJobDetails> reportDetailList = this.GetSoFailedJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount);

            if (totalCount > 0)
            {
                var buffer = new List<JMJobDetails>();

                while (reportDetailList != null && reportDetailList.Any())
                {
                    buffer.AddRange(reportDetailList);
                    FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, false);
                    reportDetailList = this.GetSoFailedJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount);
                }

                FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, true);
            }
            else
            {
                string[][] datas = new string[1][];
                datas[0] = new string[] { I18NEntity.GetString("RM_JM_DownLoadNoInformationInDB") };
                ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
            }
        }
        
        public void GenerateExcelForDifferentJobDetail(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string BaseFolder, string excelFileName, bool isTermSelection, bool isDownloadJobReports = false)
        {
            int jobDetailTotalCount = 0;
            int allSheetCount = 0;
            string[][] datas = null;
            IEnumerable<JMJobDetails> reportDetailList = null;
            string reportFileFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(BaseFolder, baseJobDto.Id);
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                reportFileFolder, excelFileName + ".xlsx");
            if (!Directory.Exists(reportFileFolder)) { Directory.CreateDirectory(reportFileFolder); }
            try
            {
                if (isDownloadJobReports)
                {
                    if (baseJobDto.JobVersion == JobVersion.UnMerged)
                    {
                        GenerateExcelForDifferentJobDetailByCursorV2(worker, baseJobDto, reportFilePath, isTermSelection);
                    }
                    else
                    {
                        GenerateExcelForDifferentJobDetailByCursor(worker, baseJobDto, reportFilePath, isTermSelection);
                    }
                }
                else
                {
                    reportDetailList = this.GetJobDetailsInfo(worker, 1, baseJobDto, ref jobDetailTotalCount, isTermSelection);
                    if (jobDetailTotalCount > 0)
                    {
                        allSheetCount = jobDetailTotalCount % JobMonitorConstants.MAX_COUNT_ONE_SHEET == 0 ? jobDetailTotalCount / JobMonitorConstants.MAX_COUNT_ONE_SHEET : jobDetailTotalCount / JobMonitorConstants.MAX_COUNT_ONE_SHEET + 1;
                        for (int index = 1; index < allSheetCount + 1; index++)
                        {
                            reportDetailList = this.GetJobDetailsInfo(worker, index, baseJobDto, ref jobDetailTotalCount, isTermSelection);
                            datas = new string[reportDetailList.Count() + 1][];
                            datas = ConvertJobDetailInfoToArray(baseJobDto, reportDetailList, datas, isTermSelection);

                            try
                            {
                                for (int i = 0; i < datas.Length; i++)
                                {
                                    var row = datas[i];
                                    for (int j = 0; j < row.Length; j++)
                                    {
                                        if (row[j] != null && (row[j].Contains("0x80070005") || row[j].Contains("E_ACCESSDENIED")))
                                        {
                                            datas[i][j] = I18NEntity.GetString("RM_JM_Details_Failed_AccessDenied");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Replace job comment error, {e}");
                            }

                            if (index == 1)
                            {
                                ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                            }
                            else
                            {
                                ReportUtil.InsertWorksheet(reportFilePath, "Sheet" + index, datas);
                            }
                        }
                    }
                    else
                    {
                        datas = new string[1][];
                        datas[0] = new string[] { I18NEntity.GetString("RM_JM_DownLoadNoInformationInDB") };
                        ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug("generate Detail Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
        }

        private void GenerateExcelForDifferentJobDetailByCursor(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string reportFilePath, bool isTermSelection)
        {
            long lastRowId = 0;
            long totalCount = 0;
            int sheetIndex = 1;
            long totalRowsInCurrentFile = 0;
            int fileIndex = 0;
            string currentFilePath = reportFilePath;

            IEnumerable<JMJobDetails> reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);

            if (totalCount > 0)
            {
                var buffer = new List<JMJobDetails>();

                while (reportDetailList != null && reportDetailList.Any())
                {
                    buffer.AddRange(reportDetailList);
                    FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, false);
                    reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);
                }

                FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, true);
            }
            else
            {
                logger.Info($"No data is returned for job {baseJobDto.Id}.");
                string[][] datas = new string[1][];
                datas[0] = new string[] { I18NEntity.GetString("RM_JM_DownLoadNoInformationInDB") };
                ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
            }
        }

        private void GenerateExcelForDifferentJobDetailByCursorV2(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string reportFilePath, bool isTermSelection)
        {
            int sheetIndex = 1;
            long totalRowsInCurrentFile = 0;
            int fileIndex = 0;
            string currentFilePath = reportFilePath;

            string originalJobId = baseJobDto.Id;
            var buffer = new List<JMJobDetails>();
            bool hasAnyData = false;

            logger.Info($"Start to generate job details for job {originalJobId} with sub job ids, total sub job count: {baseJobDto.SubJobCount}.");
            for (int i = 0; i < baseJobDto.SubJobCount; i++)
            {
                baseJobDto.Id = $"{originalJobId}_{i:D3}";
                long lastRowId = 0;
                long totalCount = 0;

                IEnumerable<JMJobDetails> reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);
                if (totalCount > 0)
                {
                    hasAnyData = true;
                    while (reportDetailList != null && reportDetailList.Any())
                    {
                        buffer.AddRange(reportDetailList);
                        FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, false);
                        reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);
                    }
                }
                try
                {
                    GenerateExcelForDifferentJobDetailByCursorV2ForSubSubJob(worker, baseJobDto, reportFilePath, isTermSelection, buffer, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, ref hasAnyData);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred when generating sub-sub job details for job {baseJobDto.Id}, error: {ex}");
                }
            }

            if (hasAnyData)
            {
                // Flush remaining rows after all subjobs are processed
                FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, true);
            }
            else
            {
                // Fallback to use original job id to query data if no data is returned with sub job ids, which might be caused by old data.
                logger.Info($"No data is returned with sub job ids for job {originalJobId}, fallback to query with original job id.");
                baseJobDto.Id = originalJobId;
                GenerateExcelForDifferentJobDetailByCursor(worker, baseJobDto, reportFilePath, isTermSelection);
            }
        }

        private void GenerateExcelForDifferentJobDetailByCursorV2ForSubSubJob(
            AbstractJobDetailWorker worker, BaseJobDto baseJobDto, string reportFilePath, bool isTermSelection, List<JMJobDetails> buffer,
            ref int sheetIndex, ref long totalRowsInCurrentFile, ref int fileIndex, ref bool hasAnyData)
        {
            string currentFilePath = reportFilePath;
            string originalJobId = baseJobDto.Id;

            // Get all sub-sub job reports' uri
            string baseBlobUri = JobReportUtility.GetJobReportUri(baseJobDto.Id, baseJobDto.JobType, string.Empty).Replace("\\", "/").TrimEnd('/');
            var blobUriList = RAStorageUtil.GetAllReportBlobNames(baseBlobUri);

            logger.Info($"Sub job {baseJobDto.Id} has {blobUriList.Count - 1} sub-sub jobs. Start to generate details for sub-sub jobs.");
            foreach (var blobUri in blobUriList)
            {
                baseJobDto.Id = System.IO.Path.GetFileNameWithoutExtension(blobUri);
                if (baseJobDto.Id == originalJobId)
                {
                    continue;
                }
                long lastRowId = 0;
                long totalCount = 0;
                IEnumerable<JMJobDetails> reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);
                while (reportDetailList != null && reportDetailList.Any())
                {
                    hasAnyData = true;
                    buffer.AddRange(reportDetailList);
                    FlushBuffer(buffer, baseJobDto, isTermSelection, ref currentFilePath, reportFilePath, ref sheetIndex, ref totalRowsInCurrentFile, ref fileIndex, false);
                    reportDetailList = this.GetJobDetailsInfoByCursor(worker, baseJobDto, ref lastRowId, ref totalCount, isTermSelection);
                }
            }
            baseJobDto.Id = originalJobId;
        }

        private string GetSplitFilePath(string originalFilePath, int fileIndex)
        {
            string directory = System.IO.Path.GetDirectoryName(originalFilePath);
            string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = System.IO.Path.GetExtension(originalFilePath);
            return AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(directory, $"{fileNameWithoutExt}_{fileIndex:D3}{extension}");
        }

        private void FlushBuffer(List<JMJobDetails> buffer, BaseJobDto baseJobDto, bool isTermSelection, ref string currentFilePath, string originalFilePath, ref int sheetIndex, ref long totalRowsInCurrentFile, ref int fileIndex, bool forceFlush)
        {
            int flushThreshold = forceFlush ? 1 : JobMonitorConstants.MAX_COUNT_ONE_SHEET;

            while (buffer.Count >= flushThreshold && buffer.Count > 0)
            {
                long remaining = JobMonitorConstants.MAX_ROWS_PER_FILE - totalRowsInCurrentFile;
                if (remaining <= 0)
                {
                    fileIndex++;
                    sheetIndex = 1;
                    totalRowsInCurrentFile = 0;
                    currentFilePath = GetSplitFilePath(originalFilePath, fileIndex);
                    remaining = JobMonitorConstants.MAX_ROWS_PER_FILE;
                }

                int takeCount = (int)Math.Min(Math.Min(remaining, buffer.Count), JobMonitorConstants.MAX_COUNT_ONE_SHEET);
                var chunk = buffer.GetRange(0, takeCount);
                buffer.RemoveRange(0, takeCount);

                string[][] datas = new string[takeCount + 1][];
                datas = ConvertJobDetailInfoToArray(baseJobDto, chunk, datas, isTermSelection);
                ReplaceAccessDeniedMessages(datas);

                FlushSheetToExcel(currentFilePath, sheetIndex, datas);
                totalRowsInCurrentFile += takeCount;
                sheetIndex++;
            }
        }

        private void FlushSheetToExcel(string filePath, int sheetIndex, string[][] datas)
        {
            if (sheetIndex == 1)
            {
                ReportUtil.CreateExcel(filePath, "Sheet", datas);
            }
            else
            {
                ReportUtil.InsertWorksheet(filePath, "Sheet" + sheetIndex, datas);
            }
        }

        private void ReplaceAccessDeniedMessages(string[][] datas)
        {
            try
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    var row = datas[i];
                    if (row == null) continue;
                    for (int j = 0; j < row.Length; j++)
                    {
                        if (row[j] != null && (row[j].Contains("0x80070005") || row[j].Contains("E_ACCESSDENIED")))
                        {
                            datas[i][j] = I18NEntity.GetString("RM_JM_Details_Failed_AccessDenied");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Replace job comment error, {e}");
            }
        }

        private IEnumerable<JMJobDetails> GetJobDetailsInfo(AbstractJobDetailWorker worker, int StartPage, BaseJobDto baseJobDto, ref int jobDetailTotalCount, bool isTermSelection)
        {
            IEnumerable<JMJobDetails> reportDetailList = null;
            if (isTermSelection)
            {
                BCSTermUsageReportJobDetailWorker jobDetailWorker = worker as BCSTermUsageReportJobDetailWorker;
                reportDetailList = jobDetailWorker.GetDataForTermSelection(JobMonitorConstants.MAX_COUNT_ONE_SHEET, StartPage, ref jobDetailTotalCount, null, baseJobDto);
            }
            else
            {
                reportDetailList = worker.GetData(JobMonitorConstants.MAX_COUNT_ONE_SHEET, StartPage, ref jobDetailTotalCount, null, baseJobDto);
            }
            return reportDetailList;
        }

        private IEnumerable<JMJobDetails> GetJobDetailsInfoByCursor(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, ref long lastRowId, ref long totalCount, bool isTermSelection)
        {
            long pageSize = JobMonitorConstants.MAX_COUNT_ONE_SHEET;
            IEnumerable<JMJobDetails> reportDetailList = null;
            if (isTermSelection)
            {
                int intTotalCount = (int)totalCount;
                BCSTermUsageReportJobDetailWorker jobDetailWorker = worker as BCSTermUsageReportJobDetailWorker;
                reportDetailList = jobDetailWorker.GetDataForTermSelection((int)pageSize, (int)lastRowId, ref intTotalCount, null, baseJobDto);
                totalCount = intTotalCount;
            }
            else
            {
                reportDetailList = worker.GetData(pageSize, ref lastRowId, ref totalCount, null, baseJobDto);
            }
            return reportDetailList;
        }
        private IEnumerable<JMJobDetails> GetSoFailedJobDetailsInfo(AbstractJobDetailWorker worker, int StartPage, BaseJobDto baseJobDto, ref int jobDetailTotalCount)
        {
            IEnumerable<JMJobDetails> reportDetailList = null;
            BaseJobDto tempDto = new BaseJobDto() { 
                Id = baseJobDto.Id.Substring(0, baseJobDto.Id.LastIndexOf("_")),
                JobType = baseJobDto.JobType,
                Category = baseJobDto.Category,
                PlanId = baseJobDto.PlanId,
                TenantGroupEmail = baseJobDto.TenantGroupEmail
            };
            reportDetailList = worker.GetData(JobMonitorConstants.MAX_COUNT_ONE_SHEET, StartPage, ref jobDetailTotalCount, null, tempDto);
            reportDetailList = reportDetailList.Where(a=>(a.Status == JobDetailsStatus.Exception || a.Status == JobDetailsStatus.Failed));
            return reportDetailList;
        }

        private IEnumerable<JMJobDetails> GetSoFailedJobDetailsInfoByCursor(AbstractJobDetailWorker worker, BaseJobDto baseJobDto, ref long lastRowId, ref long totalCount)
        {
            long pageSize = JobMonitorConstants.MAX_COUNT_ONE_SHEET;
            IEnumerable<JMJobDetails> reportDetailList = null;
            BaseJobDto tempDto = new BaseJobDto()
            {
                Id = baseJobDto.Id.Substring(0, baseJobDto.Id.LastIndexOf("_")),
                JobType = baseJobDto.JobType,
                Category = baseJobDto.Category,
                PlanId = baseJobDto.PlanId,
                TenantGroupEmail = baseJobDto.TenantGroupEmail
            };
            reportDetailList = worker.GetData(pageSize, ref lastRowId, ref totalCount, null, tempDto);
            reportDetailList = reportDetailList.Where(a => (a.Status == JobDetailsStatus.Exception || a.Status == JobDetailsStatus.Failed));
            return reportDetailList;
        }

        public async System.Threading.Tasks.Task GenerateSummaryExcelAsync(BaseJobDto baseJobDto, string BaseFolder, string excelFileName)
        {
            string[][] datas = null;
            string reportFileFolder = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                BaseFolder, baseJobDto.Id);
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                reportFileFolder, excelFileName + ".xlsx");
            if (!Directory.Exists(reportFileFolder)) { Directory.CreateDirectory(reportFileFolder); }
            try
            {
                JMJobSummary jobSummary = null;
                if (baseJobDto.JobType == (int)JobType.MigrationArchiverRetention || baseJobDto.JobType == (int)JobType.MigrationArchiverRestore)
                {
                    jobSummary = await JobMonitorService.GetDAOJobSummaryDetailsAsync(baseJobDto.Id, baseJobDto.JobType);
                }
                else
                {
                    jobSummary = await JobMonitorService.GetJobSummaryAsync(baseJobDto.Id);
                }
                if (jobSummary != null)
                {
                    datas = AssembleJobSummaryContent(baseJobDto, jobSummary, datas);
                    ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_JM_DownLoadNoSummaryInformationInDB") };
                    ReportUtil.CreateExcel(reportFilePath, "summary", datas);
                }
            }
            catch (Exception e)
            {
                logger.Debug("generate Detail Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
        }

        private string[][] AssembleJobSummaryContent(BaseJobDto baseJobDto, JMJobSummary jobSummary, string[][] datas)
        {
            int rowNumber = 0;
            if (baseJobDto.JobType == (int)JobType.BCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.ItemsFilesDueDisposal
                || baseJobDto.JobType == (int)JobType.CreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.AvailableSpaceReport
                || baseJobDto.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                || baseJobDto.JobType == (int)JobType.OneDriveTermUsageReport
                || baseJobDto.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.SPOnPremItemsFilesDueDisposal
                || baseJobDto.JobType == (int)JobType.SPOnPremBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.SPOnPremCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.TeamsBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport)
            {
                rowNumber = 9;
            }
            else if (baseJobDto.JobType == (int)JobType.RMArchiverBackup 
                || baseJobDto.JobType == (int)JobType.RMEndUserArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifySitesArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifyTeamsArchiverBackup
                || baseJobDto.JobType == (int)JobType.DiscoverOptimization
				|| baseJobDto.JobType == (int)JobType.DiscoveryAOSPOptimization
                || baseJobDto.JobType == (int)JobType.TeamsArchiverBackup
                || baseJobDto.JobType == (int)JobType.ArchiverByHSMXml
                || baseJobDto.JobType == (int)JobType.EXORecordsDisposal
                || baseJobDto.JobType == (int)JobType.CleanUpDuplicateDatas)
            {
                rowNumber = 11 + 29;
            }
            else if (baseJobDto.JobType == (int)JobType.SOPreScan
                || baseJobDto.JobType == (int)JobType.DiscoveryPreScan
                || baseJobDto.JobType == (int)JobType.RecordsDisposal
                || baseJobDto.JobType == (int)JobType.ApprovalProcessArchive
                || baseJobDto.JobType == (int)JobType.OneDriveRecordsDisposal
                || baseJobDto.JobType == (int)JobType.TeamsRecordsDisposal
                || baseJobDto.JobType == (int)JobType.TeamsPreScan
                || baseJobDto.JobType == (int)JobType.GoogleRecordsDisposal)
            {
                rowNumber = 9 + 29;
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverRestore)
            {
                rowNumber = 14;
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverRetention)
            {
                rowNumber = 9;
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverDeduplicationReport)
            {
                rowNumber = 9 + 5;
            }
            else
            {
                rowNumber = 8;
            }
            datas = new string[rowNumber][];
            for (int i = 0; i < rowNumber; i++)
            {
                datas[i] = new string[2];
            }
            datas[0][0] = I18NEntity.GetString("RM_JM_DownLoad_JobInformation");
            datas[1][0] = I18NEntity.GetString("RM_JS_JM_Module");
            datas[1][1] = ConvertJobTypeToString(jobSummary.JobType);
            datas[2][0] = I18NEntity.GetString("RM_JS_JM_JobID");
            datas[2][1] = jobSummary.JobId;
            if (baseJobDto.JobType == (int)JobType.BCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.ItemsFilesDueDisposal
                || baseJobDto.JobType == (int)JobType.CreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.AvailableSpaceReport
                || baseJobDto.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                || baseJobDto.JobType == (int)JobType.OneDriveTermUsageReport
                || baseJobDto.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.SPOnPremItemsFilesDueDisposal
                || baseJobDto.JobType == (int)JobType.SPOnPremBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.SPOnPremCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.TeamsBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport)
            {

                datas[3][0] = I18NEntity.GetString("RM_JM_ProfileName");
                datas[4][0] = I18NEntity.GetString("RM_JS_JM_StartTime");
                datas[5][0] = I18NEntity.GetString("RM_JM_EndTime");
                datas[6][0] = I18NEntity.GetString("RM_JM_JobRunBy");
                datas[7][0] = I18NEntity.GetString("RM_JS_JM_Status");
                datas[8][0] = I18NEntity.GetString("RM_JM_Comment");

                datas[3][1] = jobSummary.ProfileName;
                datas[4][1] = jobSummary.StartTime;
                datas[5][1] = jobSummary.EndTime;
                datas[6][1] = jobSummary.JobRunBy;
                datas[7][1] = ConvertJobStatusToString(jobSummary.Status);
                datas[8][1] = I18NEntity.GetString(jobSummary.Comment);
            }
            else if (baseJobDto.JobType == (int)JobType.SOPreScan
                || baseJobDto.JobType == (int)JobType.DiscoveryPreScan
                || baseJobDto.JobType == (int)JobType.RMArchiverBackup
                || baseJobDto.JobType == (int)JobType.RMEndUserArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifySitesArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifyTeamsArchiverBackup
                || baseJobDto.JobType == (int)JobType.RecordsDisposal
                || baseJobDto.JobType == (int)JobType.OneDriveRecordsDisposal
                || baseJobDto.JobType == (int)JobType.DiscoverOptimization
                || baseJobDto.JobType == (int)JobType.ApprovalProcessArchive
                || baseJobDto.JobType == (int)JobType.TeamsRecordsDisposal
                || baseJobDto.JobType == (int)JobType.TeamsArchiverBackup
				|| baseJobDto.JobType == (int)JobType.DiscoveryAOSPOptimization
                || baseJobDto.JobType == (int)JobType.GoogleRecordsDisposal
                || baseJobDto.JobType == (int)JobType.TeamsPreScan
                || baseJobDto.JobType == (int)JobType.ArchiverByHSMXml
                || baseJobDto.JobType == (int)JobType.EXORecordsDisposal
                || baseJobDto.JobType == (int)JobType.CleanUpDuplicateDatas
                )
            {
                int pos = 3;

                datas[pos][0] = I18NEntity.GetString("RM_JS_JM_StartTime");
                datas[pos++][1] = jobSummary.StartTime;

                datas[pos][0] = I18NEntity.GetString("RM_JM_EndTime");
                datas[pos++][1] = jobSummary.EndTime;

                datas[pos][0] = I18NEntity.GetString("RM_JM_JobRunBy");
                datas[pos++][1] = jobSummary.JobRunBy;

                datas[pos][0] = I18NEntity.GetString("RM_JS_JM_Status");
                datas[pos++][1] = ConvertJobStatusToString(jobSummary.Status);

                if (baseJobDto.JobType != (int)JobType.DiscoverOptimization 
                    && baseJobDto.JobType != (int)JobType.DiscoveryAOSPOptimization
                    && baseJobDto.JobType != (int)JobType.DiscoveryPreScan
                    && baseJobDto.JobType != (int)JobType.GoogleRecordsDisposal
                    && baseJobDto.JobType != (int)JobType.ArchiverByHSMXml
                    && baseJobDto.JobType != (int)JobType.CleanUpDuplicateDatas
                    )
                {
                    datas[pos][0] = I18NEntity.GetString("RM_JS_JMD_Summary_Scope");
                    datas[pos++][1] = jobSummary.Scope;
                }

                datas[pos][0] = I18NEntity.GetString("RM_JM_Comment");
                datas[pos++][1] = I18NEntity.GetString(jobSummary.Comment);


                if (baseJobDto.JobType == (int)JobType.RMArchiverBackup
                || baseJobDto.JobType == (int)JobType.RMEndUserArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifySitesArchiverBackup
                || baseJobDto.JobType == (int)JobType.SpecifyTeamsArchiverBackup
                || baseJobDto.JobType == (int)JobType.DiscoverOptimization 
				|| baseJobDto.JobType == (int)JobType.DiscoveryAOSPOptimization
                || baseJobDto.JobType == (int)JobType.TeamsArchiverBackup
                || baseJobDto.JobType == (int)JobType.ArchiverByHSMXml
                || baseJobDto.JobType == (int)JobType.CleanUpDuplicateDatas
                )
                {
                    datas[pos][0] = I18NEntity.GetString("RM_JS_JMD_Summary_Process_Site");
                    datas[pos++][1] = jobSummary.ProgressSCStr;

                    datas[pos][0] = I18NEntity.GetString("RM_JS_JMD_Summary_Process_File");
                    datas[pos++][1] = jobSummary.ProgressFileCountStr;
                }
                else if(baseJobDto.JobType == (int)JobType.EXORecordsDisposal)
                {
                    datas[pos][0] = I18NEntity.GetString("RM_JS_JMD_Summary_Process_Mailbox");
                    datas[pos++][1] = jobSummary.ProgressSCStr;

                    datas[pos][0] = I18NEntity.GetString("RM_JS_JMD_Summary_Process_Item");
                    datas[pos++][1] = jobSummary.ProgressFileCountStr;
                }

                var summaryDetails = JobMonitorService.GetSOJobSummaryDetailsAsync(jobSummary.JobId).GetAwaiter().GetResult();
                if (summaryDetails != null && summaryDetails is JMSOSummaryDetails)
                {
                    var soSummaryDetails = summaryDetails as JMSOSummaryDetails;
                    if (soSummaryDetails.ActionStatistics != null && soSummaryDetails.ActionStatistics.Count > 0)
                    {
                        foreach (var sta in soSummaryDetails.ActionStatistics)
                        {
                            pos++;

                            if (sta.ActionTab == (int)ActionTab.DOJobSettings)
                            {
                                try
                                {
                                    var settings = sta as DOJobSettingsStatistics;
                                    if (settings != null)
                                    {
                                        datas[pos++][0] = I18NEntity.GetString("RM_JM_DOSummary_SettingTitle");

                                        datas[pos][0] = I18NEntity.GetString("RM_FA_Inactive_DSOJobSummaryMS365DataFilterTypeTitle");
                                        datas[pos++][1] = settings.ScopeSettings.MS365DataTypeStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_FA_Inactive_ModifiedTitle");
                                        datas[pos++][1] = settings.ScopeSettings.ModifiedTimeRangeStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeTitle");
                                        datas[pos++][1] = settings.ScopeSettings.SizeRangeStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileCategoryTitle");
                                        datas[pos++][1] = settings.ScopeSettings.FileCatagorysStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_JM_DOSummary_Column_Rules");
                                        datas[pos++][1] = settings.DefinitionAndActionSettings.DefinitionsStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_JM_DOSummary_Column_DocumentAction");
                                        datas[pos++][1] = settings.DefinitionAndActionSettings.DocumentActionStr;
                                        datas[pos][0] = I18NEntity.GetString("RM_JM_DOSummary_Column_DocumentVersionAction");
                                        datas[pos++][1] = settings.DefinitionAndActionSettings.DocumentVersionActionStr;
                                                
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Error($"An error occurred while adding settings to statistics. error {e.ToString()}");
                                }
                            }
                            else
                            {
                                if (sta.ActionTab == (int)ActionTab.Scan)
                                {
                                    datas[pos++][0] = I18NEntity.GetString("RM_JM_SOSummary_ScanTitle");
                                }
                                else if (sta.ActionTab == (int)ActionTab.Export)
                                {
                                    datas[pos++][0] = I18NEntity.GetString("RM_JM_SOSummary_ExportTitle");
                                }
                                else if (sta.ActionTab == (int)ActionTab.Backup)
                                {
                                    datas[pos++][0] = I18NEntity.GetString("RM_JM_SOSummary_ArchivingTitle");
                                }
                                else if (sta.ActionTab == (int)ActionTab.Action)
                                {
                                    datas[pos++][0] = I18NEntity.GetString("RM_JM_SOSummary_OthersTitle");
                                }
                                else if(sta.ActionTab == (int)ActionTab.Delete)
                                {
                                    datas[pos++][0] = I18NEntity.GetString("RM_JM_SOSummary_DeleteTitle");
                                }

                                datas[pos][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_SuccessfulNumber");
                                datas[pos++][1] = GetDatasSumaryNumberContent(baseJobDto.JobType, sta.SuccessfulObj);
                                datas[pos][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_SkipNumber");
                                datas[pos++][1] = GetDatasSumaryNumberContent(baseJobDto.JobType, sta.SkippedObj);
                                datas[pos][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_FailedNumber");
                                datas[pos++][1] = GetDatasSumaryNumberContent(baseJobDto.JobType, sta.FailedObj);

                                if (baseJobDto.JobType != (int)JobType.EXORecordsDisposal)
                                {
                                    if (sta.ActionTab != (int)ActionTab.Action)
                                    {
                                        datas[pos][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_TotalSize");
                                        datas[pos++][1] = sta.SizeStr;
                                    }
                                    else
                                    {
                                        datas[pos][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_Total_Deletion_Size");
                                        datas[pos++][1] = sta.DeleteSizeStr;
                                    }
                                }
                                
                                datas[pos][0] = I18NEntity.GetString("RM_JS_JM_Status");
                                datas[pos++][1] = ConvertJobStatusToString(sta.Status);
                            }
                        }
                    }
                }
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverRestore || baseJobDto.JobType == (int)JobType.MigrationArchiverRetention)
            {
                var index = 0;
                foreach (var summary in jobSummary.DisposalSummary.SummaryItem)
                {
                    if (index != 0)
                    {
                        index++;
                    }
                    datas[index++][0] = summary.Title;
                    foreach (var row in summary.SummaryRow)
                    {
                        datas[index][0] = row.Key;
                        datas[index][1] = row.Value;
                        index++;
                    }
                }
            }
            else if(baseJobDto.JobType == (int)JobType.ArchiverDeduplicationReport)
            {
                datas[3][0] = I18NEntity.GetString("RM_JS_JM_StartTime");
                datas[4][0] = I18NEntity.GetString("RM_JM_EndTime");
                datas[5][0] = I18NEntity.GetString("RM_JM_JobRunBy");
                datas[6][0] = I18NEntity.GetString("RM_JS_JM_Status");
                datas[7][0] = I18NEntity.GetString("RM_JM_Comment");

                datas[3][1] = jobSummary.StartTime;
                datas[4][1] = jobSummary.EndTime;
                datas[5][1] = jobSummary.JobRunBy;
                datas[6][1] = ConvertJobStatusToString(jobSummary.Status);
                datas[7][1] = I18NEntity.GetString(jobSummary.Comment);

                datas[9][0] = I18NEntity.GetString("RM_JM_Summary_DedupTitle");
                datas[10][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_SuccessSitefulNumber");
                datas[11][0] = I18NEntity.GetString("RM_JM_SOSummary_Column_FailedSiteNumber");
                datas[12][0] = I18NEntity.GetString("RM_JM_Summary_Column_Total_Deduped_Cout");
                datas[13][0] = I18NEntity.GetString("RM_JM_Summary_Column_Total_Deduped_Size");
                var summaryDetails = JobMonitorService.GetSOJobSummaryDetailsAsync(jobSummary.JobId).GetAwaiter().GetResult();
                if (summaryDetails != null && summaryDetails is JMArchiverDedupReportSummaryDetails)
                {
                    var archiverDedupReportDetails = summaryDetails as JMArchiverDedupReportSummaryDetails;

                    datas[10][1] = archiverDedupReportDetails.SiteCollectionCount.ToString();
                    datas[11][1] = archiverDedupReportDetails.FailedSiteCollectionCount.ToString();
                    datas[12][1] = archiverDedupReportDetails.TotalDedupFilesCount.ToString();
                    datas[13][1] = archiverDedupReportDetails.TotalDedupFilesSizeStr;
                }
            }
            else
            {
                datas[3][0] = I18NEntity.GetString("RM_JS_JM_StartTime");
                datas[4][0] = I18NEntity.GetString("RM_JM_EndTime");
                datas[5][0] = I18NEntity.GetString("RM_JM_JobRunBy");
                datas[6][0] = I18NEntity.GetString("RM_JS_JM_Status");
                datas[7][0] = I18NEntity.GetString("RM_JM_Comment");

                datas[3][1] = jobSummary.StartTime;
                datas[4][1] = jobSummary.EndTime;
                datas[5][1] = jobSummary.JobRunBy;
                datas[6][1] = ConvertJobStatusToString(jobSummary.Status);
                datas[7][1] = I18NEntity.GetString(jobSummary.Comment);
            }
            return datas;
        }

        private string GetDatasSumaryNumberContent(int jobType, ObjectStatistic staObj)
        {
            if (jobType == (int)JobType.BoxRecordsDisposal)
            {
                var succCount = staObj.ConnectionCount + staObj.UserCount + staObj.FolderCount + staObj.FileCount;
                return succCount + string.Format(I18NEntity.GetString("RM_JM_BoxSummary_NumberContent"), staObj.ConnectionCount, staObj.UserCount, staObj.FolderCount, staObj.FileCount);
            }
            else if (jobType == (int)JobType.TeamsArchiverBackup 
                || jobType == (int)JobType.SpecifyTeamsArchiverBackup
                || jobType == (int)JobType.TeamsRecordsDisposal 
                || jobType == (int)JobType.TeamsPreScan)
            {
                var succCount = staObj.TeamsTotalCount;
                return succCount + string.Format(I18NEntity.GetString("RM_JM_Teams_SOSummary_NumberContent"), 
                    staObj.TeamsGroupCount,
                    staObj.ChannelCount,
                    staObj.PlanCount,
                    staObj.TaskCount,
                    staObj.ChannelConversationCount,
                    staObj.GroupMailboxCount,
                    staObj.GroupMailboxItemCount,
                    staObj.SiteCollectionCount,
                    staObj.SiteCount,
                    staObj.ListCount,
                    staObj.FolderCount,
                    staObj.ItemCount
                    );
            }
            else if(jobType == (int)JobType.GoogleRecordsDisposal)
            {
                var succCount = staObj.DriveTotalCount;
                return succCount + string.Format(I18NEntity.GetString("RM_JM_Google_SOSummary_NumberContent"), staObj.DriveCount, staObj.FolderCount, staObj.ItemCount);
            }
            else if(jobType == (int)JobType.EXORecordsDisposal)
            {
                return string.Format(I18NEntity.GetString("RM_JM_ExchangeOnline_SOSummary_NumberContent"),staObj.TeamsTotalCount, staObj.GroupMailboxCount, staObj.GroupMailboxFolderCount, staObj.GroupMailboxItemCount);
            }
            else
            {
                var succCount = staObj.SiteCollectionCount + staObj.SiteCount + staObj.ListCount + staObj.FolderCount + staObj.ItemCount;
                return succCount + string.Format(I18NEntity.GetString("RM_JM_SOSummary_NumberContent"), staObj.SiteCollectionCount, staObj.SiteCount, staObj.ListCount, staObj.FolderCount, staObj.ItemCount);
            }
        }

        private string[][] ConvertJobDetailInfoToArray(BaseJobDto baseJobDto, IEnumerable<JMJobDetails> jobDetails, string[][] datas, bool isTermSelection)
        {
            if ((baseJobDto.JobType == (int)JobType.BCSTermUsageReport || baseJobDto.JobType == (int)JobType.EXOTermUsageReport || baseJobDto.JobType == (int)JobType.PhysicalTermUsageReport || baseJobDto.JobType == (int)JobType.FSBCSTermUsageReport) || baseJobDto.JobType == (int)JobType.OneDriveTermUsageReport || baseJobDto.JobType == (int)JobType.TeamsBCSTermUsageReport && isTermSelection)
            {
                AssembleJMTermSelectionHeaderTittle(baseJobDto, datas);
                ConvertJMTermSelectionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.BCSTermUsageReport || baseJobDto.JobType == (int)JobType.ItemsFilesDueDisposal
                || baseJobDto.JobType == (int)JobType.EXOItemsFilesDueDisposalReport || baseJobDto.JobType == (int)JobType.EXOTermUsageReport
                || baseJobDto.JobType == (int)JobType.PhysicalTermUsageReport || baseJobDto.JobType == (int)JobType.PhysicalItemsFilesDueDisposalReport
                || baseJobDto.JobType == (int)JobType.FSItemsFilesDueDisposal || baseJobDto.JobType == (int)JobType.FSBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.OneDriveItemsFilesDueDisposalReport || baseJobDto.JobType == (int)JobType.OneDriveTermUsageReport
                || baseJobDto.JobType == (int)JobType.SPOnPremItemsFilesDueDisposal || baseJobDto.JobType == (int)JobType.SPOnPremBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.BoxItemsFilesDueDisposalReport || baseJobDto.JobType == (int)JobType.BoxBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.GoogleItemsFilesDueDisposalReport || baseJobDto.JobType == (int)JobType.GoogleBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.TeamsBCSTermUsageReport
                || baseJobDto.JobType == (int)JobType.TeamsItemsFilesDueDisposalReport)
            {
                AssembleJMReportJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertJMReportJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverDeduplicationReport)
            {
                AssembleArchvierDedupReportJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertArchvierDedupReportJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.TermSynchronization || baseJobDto.JobType == (int)JobType.PhysicalTermSynchronization)
            {
                AssembleJMTermSyncJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertJMTermSyncJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremTermSynchronization || baseJobDto.JobType == (int)JobType.SPOnPremTermSynchronizationSchedule)
            {
                AssembleOnPremiseJMTermSyncJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertOnPremiseJMTermSyncJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SharePointGlobalSetting || baseJobDto.JobType == (int)JobType.SharePointScheduleSetting || baseJobDto.JobType == (int)JobType.ApplySharePointSettings
                || baseJobDto.JobType == (int)JobType.TeamsScheduleSetting || baseJobDto.JobType == (int)JobType.ApplyTeamsSettings)
            {
                AssembleJMGlobalSettingHeaderTittle(baseJobDto, datas);
                ConvertJMGlobalSettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremApplySetting || baseJobDto.JobType == (int)JobType.SPOnPremApplySettingSchedule)
            {
                AssembleOnPremiseJMGlobalSettingHeaderTittle(baseJobDto, datas);
                ConvertOnPremiseJMGlobalSettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalFolderSynchronization)
            {
                AssembleJMPhysicalSyncJobHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalSyncJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.UpdateLocation)
            {
                AssembleJMUpdateLocationJobHeaderTittle(baseJobDto, datas);
                ConvertJMUpdateLocationJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportPhysicalRecords)
            {
                AssembleJMImportPhysicalRecordsJobHeaderTittle(baseJobDto, datas);
                ConvertImportPhysicalRecordsJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.TrimRecordsDeletion)
            {
                AssembleJMImportedRecordsDeletionJobHeaderTittle(baseJobDto, datas);
                ConvertImportedRecordsDeletionJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportRecordsRelated)
            {
                AssembleJMImportRecordsRelatedJobHeaderTittle(baseJobDto, datas);
                ConvertImportRecordsRelatedJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.AvailableSpaceReport)
            {
                AssembleJMAvailableSpaceReportJobHeaderTittle(baseJobDto, datas);
                ConvertJMAvailableSpaceReportJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.CreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.EXOCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.PhysicalCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.FSCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.SPOnPremCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.BoxCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                || baseJobDto.JobType == (int)JobType.TeamsCreateAndDestroyedFileReport)
            {
                AssembleJMTimeFrameSpaceReportJobHeaderTittle(baseJobDto, datas);
                ConvertJMTimeFrameReportJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportTermStructure || baseJobDto.JobType == (int)JobType.ExportTermStructure || baseJobDto.JobType == (int)JobType.ImportGoogleTermStructure)
            {
                AssembleJMTermImportJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertJMTermImportJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryExportO365Profile)
            {
                AssembleJMDiscoveryExportProfileJobDetailsHeaderTittle(baseJobDto, datas);
                ConvertJMDiscoveryExportProfileJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ManualApproval)
            {
                AssembleJMManualApprovalHeaderTittle(baseJobDto, datas);
                ConvertJMManualApprovalDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverFullTextIndex)
            {
                AssembleJMArchiverFullTextIndexHeaderTitle(baseJobDto, datas);
                ConvertJMArchiverFullTextIndexDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DeleteRestoredData)
            {
                AssembleJMArchiverDeleteRestoredDataHeaderTitle(baseJobDto, datas);
                ConvertJMArchiverDeleteRestoredDataDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryJobV2 || baseJobDto.JobType == (int)JobType.DiscoveryJobV3 || baseJobDto.JobType == (int)JobType.DiscoveryJobV4 || baseJobDto.JobType == (int)JobType.DiscoveryJobV5 || baseJobDto.JobType == (int)JobType.DiscoveryAOSPJob)
            {
                AssembleJMDiscoveryJobV2HeaderTitle(baseJobDto, datas);
                ConvertJMDiscoveryJobV2DetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryGoogleJobV1)
            {
                AssembleJMDiscoveryGoogleJobHeaderTitle(baseJobDto, datas);
                ConvertJMDiscoveryGoogleJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryAnalysisFileSystemV1)
            {
                AssembleJMDiscoveryFileSystemJobHeaderTitle(baseJobDto, datas);
                ConvertJMDiscoveryFileSystemJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryProfileJob)
            {
                AssembleJMDiscoveryProfileJobHeaderTitle(baseJobDto, datas);
                ConvertJMDiscoveryProfileJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryGoogleProfileJob)
            {
                AssembleJMDiscoveryGoogleProfileJobHeaderTitle(baseJobDto, datas);
                ConvertJMDiscoveryGoogleProfileJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ManualApprovalTimer)
            {
                AssembleJMManualApprovalTimerHeaderTittle(baseJobDto, datas);
                ConvertJMManualApprovalTimerDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SharePointOnlineDeletionSyncUpgrade ||
                baseJobDto.JobType == (int)JobType.CosmosDBDirtyDataDeleteUpgrade ||
                baseJobDto.JobType == (int)JobType.ManualFileSystemUpgrade ||
                baseJobDto.JobType == (int)JobType.SendEmailJob ||
                baseJobDto.JobType == (int)JobType.DiscoveryJob ||
                baseJobDto.JobType == (int)JobType.DiscoveryOptimizationCalculate ||
                baseJobDto.JobType == (int)JobType.DiscoveryAOSPOptimizationCalculate ||
                baseJobDto.JobType == (int)JobType.DiscoveryReCalculate)
            {

            }
            else if (baseJobDto.JobType == (int)JobType.ManualApprovalOrRejectJob
               || baseJobDto.JobType == (int)JobType.ManualExportHistoryDatasJob
               || baseJobDto.JobType == (int)JobType.ManualExportRecordsForReviewDatasJob
               || baseJobDto.JobType == (int)JobType.ManualImportUnderReviewDatasJob
               || baseJobDto.JobType == (int)JobType.ManualFolderViewActions)
            {
                AssembleJMManualApprovalTimerHeaderTittle(baseJobDto, datas);
                ConvertJMManualApprovalTimerDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.UniqueIDSettingFullSchedule || baseJobDto.JobType == (int)JobType.UniqueIDSettingIncrementalSchedule
                || baseJobDto.JobType == (int)JobType.TeamsUniqueIDSettingFullSchedule || baseJobDto.JobType == (int)JobType.TeamsUniqueIDSettingIncrementalSchedule)
            {
                AssembleJMUniqueIDSettingHeaderTittle(baseJobDto, datas);
                ConvertJMUniqueIDSettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule || baseJobDto.JobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule)
            {
                AssembleOnPremiseJMUniqueIDSettingHeaderTittle(baseJobDto, datas);
                ConvertOnPremiseJMUniqueIDSettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DataSynchronisation || baseJobDto.JobType == (int)JobType.SPDataSynchronisationSchedule
                || baseJobDto.JobType == (int)JobType.OneDriveDataSynchronisation || baseJobDto.JobType == (int)JobType.OneDriveDataSynchronisationSchedule
                || baseJobDto.JobType == (int)JobType.TeamsDataSynchronisation || baseJobDto.JobType == (int)JobType.TeamsDataSynchronisationSchedule)
            {
                AssembleJMDataCollectionHeaderTittle(baseJobDto, datas);
                ConvertJMDataCollectionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.AzureFileShareDataSynchronisation || baseJobDto.JobType == (int)JobType.AzureFileShareDataSynchronisationSchedule)
            {
                AssembleJMAzureFileShareDataSyncTitle(baseJobDto, datas);
                ConvertJMAzureFileShareDataSyncToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremDataSync || baseJobDto.JobType == (int)JobType.SPOnPremDataSyncSchedule)
            {
                AssembleOnPremiseJMDataCollectionHeaderTittle(baseJobDto, datas);
                ConvertOnPremiseJMDataCollectionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SyncSecurityContainer)
            {
                AssembleJMSyncSecurityContainerHeaderTittle(baseJobDto, datas);
                ConvertJMSecurityContianerToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.EnforceRetention || baseJobDto.JobType == (int)JobType.OldEnforceRetention)
            {
                AssembleJMEnforceRetentionHeaderTittle(baseJobDto, datas);
                ConvertJMEnforceRetentionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.RecordsExplorerMove)
            {
                AssembleJMRecordsExplorerMoveHeaderTittle(baseJobDto, datas);
                ConvertJMRecordsExplorerMoveToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.EXOApplySetting)
            {
                AssembleJMEXOApplySettingHeaderTittle(baseJobDto, datas);
                ConvertJMEXOApplySettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.EXODataSynchronisation || baseJobDto.JobType == (int)JobType.EXODataSynchronisationSchedule)
            {
                AssembleJMEXODataSyncHeaderTittle(baseJobDto, datas);
                ConvertJMEXODataSynchronisationToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.EXORecordsDisposal)
            {
                AssembleJMEXOEnforceRuleActionHeaderTittle(baseJobDto, datas);
                ConvertJMEXOEnforceRuleActionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalExplorerTimer)
            {
                AssembleJMPhysicalExplorerTimerHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalExplorerTimerToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ConnectorTimer)
            {
                AssembleJMConnectorExplorerTimerHeaderTittle(baseJobDto, datas);
                ConvertJMConnectorExplorerTimerToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalDisposal || baseJobDto.JobType == (int)JobType.PhysicalRecordsDisposal)
            {
                AssembleJMPhysicalDisposalHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalDisposalToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportSPSetting || baseJobDto.JobType == (int)JobType.ArchiverExport || baseJobDto.JobType == (int)JobType.ExportSPSetting
                || baseJobDto.JobType == (int)JobType.ExportSPSOSetting || baseJobDto.JobType == (int)JobType.ExportTeamsSOSetting)
            {
                AssembleImportSPSettingHeaderTittle(baseJobDto, datas);
                ConvertImportSPSettingJobDetailToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalExportBarcode)
            {
                AssembleJMPhysicalExportBarcodeHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalExportBarcodeToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportSCWhitelist || baseJobDto.JobType == (int)JobType.ImportSCBlacklist
                || baseJobDto.JobType == (int)JobType.DiscoveryImportExcludeSCList)
            {
                AssembleFullTextIndexListHeaderTittle(baseJobDto, datas);
                ConvertFullTextIndexListDetailToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ActionOnly)
            {
                AssembleActionOnlyHeaderTittle(baseJobDto, datas);
                ConvertActionOnlyDetailToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalSetPermission)
            {
                AssembleJMPhysicalSetPermissionHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalSetPermissionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.Dashboard)
            {
                AssembleJMDashBoardHeaderTitle(baseJobDto, datas);
                ConvertJMDashBoardToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ManualApprovalEmailSchedule)
            {
                AssembleJMManualApprovalEmailScheduleHeaderTitle(baseJobDto, datas);
                ConvertJMManualApprovalEmailScheduleToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSDashBoard)
            {
                AssembleJMFSDashBoardHeaderTittle(baseJobDto, datas);
                ConvertJMFSDashBoardToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremDashBoard)
            {
                AssembleJMSPOnPremDashBoardHeaderTitle(baseJobDto, datas);
                ConvertJMSPOnPremDashBoardToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSDataSynchronization || baseJobDto.JobType == (int)JobType.FSDataSynchronizationSchedule)
            {
                AssembleJMFSDataSyncHeaderTittle(baseJobDto, datas);
                ConvertJMFSDataSyncToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ImportFSSetting || baseJobDto.JobType == (int)JobType.ExportFSSetting || baseJobDto.JobType == (int)JobType.DownloadRCCReport)
            {
                AssembleJMFSImportSettingHeaderTittle(baseJobDto, datas);
                ConvertJMFSImportSettingToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSDisposal || baseJobDto.JobType == (int)JobType.FSDisposalSchedule || baseJobDto.JobType == (int)JobType.FSDisposalByClassCode)
            {
                AssembleJMFSDisposalHeaderTittle(baseJobDto, datas);
                ConvertJMFSDisposalToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSFolderChangeTerm)
            {
                AssembleJMFSFolderReclassifyHeaderTittle(baseJobDto, datas);
                ConvertJMFSFolderReclassifyToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSFolderManageHold)
            {
                AssembleJMFSFolderHoldHeaderTittle(baseJobDto, datas);
                ConvertJMFSFolderHoldToArray(jobDetails, datas);
            }
            else if ((JobType)baseJobDto.JobType is JobType.GlobalSearchAction
                or JobType.MachineLearningExportReportJob)
            {
                AssembleJMGlobalSearchActionHeaderTittle(baseJobDto, datas);
                ConvertJMGlobalSearchActionToArray(jobDetails, datas);
            }
            else if ((JobType)baseJobDto.JobType is JobType.MachineLearningReviewApprove
                or JobType.MachineLearningReviewReclassify)
            {
                AssembleJMSmartTermActionHeaderTittle(baseJobDto, datas);
                ConvertJMSmartTermActionToArray(jobDetails, datas);
            }
            else if ((JobType)baseJobDto.JobType is JobType.ExportSiteMetrics)
            {
                AssembleJMExportSiteMetricsHeaderTittle(baseJobDto, datas);
                ConvertJMExportSiteMetricsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SyncNodesFromAOS)
            {
                AssembleJMSyncRemoteNodesHeaderTittle(baseJobDto, datas);
                ConvertJMSyncRemoteNodesToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremScanLocalNodes)
            {
                AssembleJMScanLocalNodesHeaderTittle(baseJobDto, datas);
                ConvertJMScanLocalNodesToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOnPremEnforceRuleAction || baseJobDto.JobType == (int)JobType.SPOnPremEnforceRuleActionSchedule)
            {
                AssembleJMOnremiseEnforceRuleActionHeaderTittle(baseJobDto, datas);
                ConvertJMOnremiseEnforceRuleActionToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ExportSearchResult)
            {
                AssembleJMExportSearchResultHeaderTittle(baseJobDto, datas);
                ConvertJMExportSearchResultToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalLoanBox || baseJobDto.JobType == (int)JobType.PhysicalReturnBox)
            {
                AssembleJMPhyLoanBoxHeaderTittle(baseJobDto, datas);
                ConvertJMPhyLoanBoxToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalMoveDataJob)
            {
                AssembleJMPhysicalMoveHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalMoveJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.SPOActionAuditReport
                || baseJobDto.JobType == (int)JobType.OneDriveActionAuditReport
                || baseJobDto.JobType == (int)JobType.TeamsActionAuditReport)
            {
                AssembleJMActionAuditReportHeaderTittle(baseJobDto, datas);
                ConvertJMActionAuditReportToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalLoanPick)
            {
                AssembleJMPickCompleteHeaderTittle(baseJobDto, datas);
                ConvertJMPickCompleteToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.MachineLearningTraining)
            {
                AssembleJMTrainingJobHeaderTittle(baseJobDto, datas);
                ConvertJMTrainingJobToArray(jobDetails, datas);
            }
            else if(baseJobDto.JobType == (int)JobType.ApplyClassCode)
            {
                AssembleJMApplyClassCodeJobHeaderTittle(baseJobDto, datas);
                ConvertJMApplyClassCodeJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.MachineLearningAnalyse)
            {
                AssembleJMTrainingAnalyseJobHeaderTittle(baseJobDto, datas);
                ConvertJMTrainingAnalyseJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.RMArchiverBackup || baseJobDto.JobType == (int)JobType.RMEndUserArchiverBackup || baseJobDto.JobType == (int)JobType.SpecifySitesArchiverBackup || baseJobDto.JobType == (int)JobType.RecordsDisposal || baseJobDto.JobType == (int)JobType.OneDriveRecordsDisposal || baseJobDto.JobType == (int)JobType.DiscoverOptimization || baseJobDto.JobType == (int)JobType.DiscoveryAOSPOptimization || baseJobDto.JobType == (int)JobType.BoxRecordsDisposal || baseJobDto.JobType == (int)JobType.ApprovalProcessArchive || baseJobDto.JobType == (int)JobType.TeamsArchiverBackup || baseJobDto.JobType == (int)JobType.TeamsRecordsDisposal || baseJobDto.JobType == (int)JobType.SpecifyTeamsArchiverBackup || baseJobDto.JobType == (int)JobType.CleanUpDuplicateDatas)
            {
                AssembleJMArchiverActionJobHeaderTittle(baseJobDto, datas);
                ConvertJMArchiverActionJobToArray(jobDetails, datas, baseJobDto.SiteCollectionUrl);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverByHSMXml)
            {
                AssembleJMHSMArchiverActionJobHeaderTittle(baseJobDto, datas);
                ConvertJMHSMArchiverActionJobToArray(jobDetails, datas, baseJobDto.SiteCollectionUrl);
            }
            else if (baseJobDto.JobType == (int)JobType.SOPreScan || baseJobDto.JobType == (int)JobType.TeamsPreScan)
            {
                AssembleJMPreScanJobHeaderTittle(baseJobDto, datas);
                ConvertJMPreScanJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DiscoveryPreScan)
            {
                AssembleJMDiscoveryPreScanJobHeaderTittle(baseJobDto, datas);
                ConvertJMDiscoveryPreScanJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverMoveIndex)
            {
                AssembleJMMoveIndexJobHeaderTittle(baseJobDto, datas);
                ConvertJMMoveIndexJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverRetention 
                || baseJobDto.JobType == (int)JobType.FSRetain 
                || baseJobDto.JobType == (int)JobType.TeamsArchiverRetention 
                || baseJobDto.JobType == (int)JobType.EXOArchiverRetention
                || baseJobDto.JobType == (int)JobType.GoogleArchiverRetention)
            {
                AssembleJMArchiveRetentionJobHeaderTittle(baseJobDto, datas);
                ConvertJMArchiveRetentionJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSRetainSimulate
               || baseJobDto.JobType == (int)JobType.ArchiverRetentionSimulate)
            {
                AssembleJMArchiveRetentionJobSimulateHeaderTittle(baseJobDto, datas);
                ConvertJMArchiveRetentionSimulateJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DeleteOrphanDatas)
            {
                AssembleJMDeleteOrphanDatasJobHeaderTittle(baseJobDto, datas);
                ConvertJMDeleteOrphanDatasJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverDeduplication)
            {
                AssembleJMArchiverDedupJobHeaderTittle(baseJobDto, datas);
                ConvertJMArchiverDedupJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.VeoMerge)
            {
                AssembleJMArchiverVEOJobHeaderTittle(baseJobDto, datas);
                ConvertJMArchiverVEOJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ArchiverRestore || baseJobDto.JobType == (int)JobType.ArchiverOutPlaceRestore || baseJobDto.JobType == (int)JobType.StubOopRestore || baseJobDto.JobType == (int)JobType.AOSPRestore || baseJobDto.JobType == (int)JobType.TeamsArchiverRestore || baseJobDto.JobType == (int)JobType.TeamsOutPlaceRestore || baseJobDto.JobType == (int)JobType.MailBoxArchiverRestore || baseJobDto.JobType == (int)JobType.ArchiverToSpoRestore || baseJobDto.JobType == (int)JobType.StubArchiverRestore || baseJobDto.JobType == (int)JobType.M365InPlaceArchiverRestore)
            {
                AssembleJMArchiverRestoreJobHeaderTittle(baseJobDto, datas);
                ConvertJMArchiverRestoreJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.FSArchiverRestore)
            {
                AssembleJMFSRestoreJobHeaderTitle(baseJobDto, datas);
                ConvertJMFSRestoreJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.CloudArchiverMigration)
            {
                AssembleJMArchiverMigrationHeaderTittle(baseJobDto, datas);
                ConvertJMArchiverMigrationDataToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverRestore)
            {
                AssembleJMMigrationRestoreHeaderTittle(baseJobDto, datas);
                ConvertJMMigrationRestoreDataToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverRetention)
            {
                AssembleJMMigrationRetentionHeaderTittle(baseJobDto, datas);
                ConvertJMMigrationRetentionDataToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.MigrationArchiverFileLevelRetention)
            {
                AssembleJMMigrationFileLevelRetentionHeaderTittle(baseJobDto, datas);
                ConvertJMMigrationFileLevelRetentionDataToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalBulkInsertExport || baseJobDto.JobType == (int)JobType.PhysicalBulkEditExport)
            {
                AssembleJMExportBulkUpdatePhysicalRecordsJobHeaderTittle(baseJobDto, datas);
                ConvertExportBulkUpdatePhysicalRecordsJobToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.RestoreReport || baseJobDto.JobType == (int)JobType.OneDriverRestoreReport || baseJobDto.JobType == (int)JobType.TeamsRestoreReport || baseJobDto.JobType == (int)JobType.GoogleRestoreReport)
            {
                AssembleRestoreReportTittle(baseJobDto, datas);
                ConvertRestoreReportToArray(jobDetails, datas);
            } else if (baseJobDto.JobType == (int)JobType.BoxDataSynchronisation || baseJobDto.JobType == (int)JobType.BoxDataSynchronisationSchedule)
            {
                AssembleBoxDataSyncTittle(datas);
                ConvertBoxDataSyncToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.PhysicalTemplateImport)
            {
                AssembleJMPhysicalTemplateImportHeaderTittle(baseJobDto, datas);
                ConvertJMPhysicalTemplateImportJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.ConvertStub)
            {
                AssembleConvertStubHeaderTittle(baseJobDto, datas);
                ConvertConvertStubJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DeclaredRecordsMigration)
            {
                AssembleDeclaredRecordsMigrationHeaderTittle(baseJobDto, datas);
                ConvertDeclaredRecordsMigrationJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.StubDisposal)
            {
                AssembleStubDisposalHeaderTittle(baseJobDto, datas);
                ConvertStubDisposalJobDetailsToArray(jobDetails, datas);
            }
            else if (baseJobDto.JobType == (int)JobType.DeleteArchivedSiteCollection)
            {
                AssembleDeleteArchivedSCHeaderTittle(baseJobDto, datas);
                DeleteArchivedSCJobDetailsToArray(jobDetails, datas);
            }
            else if(baseJobDto.JobType == (int)JobType.TeamsNodeSettingUpgrade)
            {
                AssembleConvertStubHeaderTittle(baseJobDto, datas);
                ConvertConvertStubJobDetailsToArray(jobDetails, datas);
            }
            else 
            {
                switch ((JobType) baseJobDto.JobType)
                {
                    case JobType.GoogleApplySettings:
                        AssembleGoogleApplySettingTittle(datas);
                        ConvertGoogleApplySettingToArray(jobDetails, datas);
                        break;
                    case JobType.GoogleDataSynchronization:
                        AssembleGoogleDataSyncTittle(datas);
                        ConvertGoogleDataSyncToArray(jobDetails, datas);
                        break;
                    case JobType.GoogleRecordsDisposal:
                        AssembleGoogleDisposalTittle(datas);
                        ConvertGoogleDisposalToArray(jobDetails, datas);
                        break;
                    case JobType.SFDiscoveryJob:
                        AssembleSalesforceTittle(datas);
                        ConvertSalesforceToArray(jobDetails, datas);
                        break;
                }
                
            }
            return datas;
        }

        private string[][] AssembleActionOnlyHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = "Rule Name";//I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");//TO DO I18N
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertActionOnlyDetailToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMActionOnlyJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMActionOnlyJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.Url;
                datas[rowCount][2] = jobDetailInfo.RuleName;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleImportSPSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_Common_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertImportSPSettingJobDetailToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportSPSettingDetail jobDetailInfo = null;
            foreach (JMImportSPSettingDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportSPSettingDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.Url;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMReportJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMReportJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMReportJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMReportJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.TitleOrName;
                datas[rowCount][1] = jobDetailInfo.Type;
                datas[rowCount][2] = jobDetailInfo.Url;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleArchvierDedupReportJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_SiteCollectionURL");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_DedupFilesCount");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_DedupFilesSize");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertArchvierDedupReportJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverDedupReportDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMArchiverDedupReportDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.SrcURL;
                datas[rowCount][1] = jobDetailInfo.Remark1.ToString();
                datas[rowCount][2] = jobDetailInfo.SizeStr;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMTermSyncJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Term");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            //datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SiteCollectionURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_TenantAdminURL");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMTermSyncJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMTermSyncJobDetails jobDetailInfo = null;
            foreach (JMTermSyncJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMTermSyncJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.Term;
                datas[rowCount][1] = jobDetailInfo.Action;
                //datas[rowCount][2] = jobDetailInfo.SiteCollectionURL;
                datas[rowCount][2] = jobDetailInfo.MMSApplication;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleOnPremiseJMTermSyncJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Term");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            //datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SiteCollectionURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_MMSApplication");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertOnPremiseJMTermSyncJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMTermSyncJobDetails jobDetailInfo = null;
            foreach (JMTermSyncJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMTermSyncJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.Term;
                datas[rowCount][1] = jobDetailInfo.Action;
                //datas[rowCount][2] = jobDetailInfo.SiteCollectionURL;
                datas[rowCount][2] = jobDetailInfo.MMSApplication;
                datas[rowCount][3] = jobDetailInfo.AgentName;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }


        private string[][] AssembleJMTermSelectionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[2];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Term");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Term_TermFullPath");
            return datas;
        }

        private string[][] ConvertJMTermSelectionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMTermSelection jobDetailInfo = null;
            foreach (JMTermSelection jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMTermSelection;
                datas[rowCount] = new string[2];
                datas[rowCount][0] = jobDetailInfo.Term;
                datas[rowCount][1] = jobDetailInfo.TermFullPath;
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMGlobalSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_SPS_FieldContainerClassification");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ColumnName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleOnPremiseJMGlobalSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_SPS_FieldContainerClassification");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ColumnName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] AssembleJMPhysicalSyncJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_TermName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_LocationPath");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SiteCollectionURL");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMGlobalSettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGlobalSettingJobDetails jobDetailInfo = null;
            foreach (JMGlobalSettingJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGlobalSettingJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.SourceURL;
                datas[rowCount][2] = jobDetailInfo.Classification;
                datas[rowCount][3] = jobDetailInfo.ColumnName;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertOnPremiseJMGlobalSettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGlobalSettingJobDetails jobDetailInfo = null;
            foreach (JMGlobalSettingJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGlobalSettingJobDetails;
                datas[rowCount] = new string[8];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.SourceURL;
                datas[rowCount][2] = jobDetailInfo.Classification;
                datas[rowCount][3] = jobDetailInfo.ColumnName;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = jobDetailInfo.AgentName;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] ConvertJMPhysicalSyncJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPhysicalSyncJobDetails jobDetailInfo = null;
            foreach (JMPhysicalSyncJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMPhysicalSyncJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.TermName;
                datas[rowCount][1] = jobDetailInfo.LocationPath;
                datas[rowCount][2] = jobDetailInfo.SiteCollectionURL;
                datas[rowCount][3] = jobDetailInfo.Action;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                //datas[rowCount][6] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMUpdateLocationJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_SiteCollectionURL");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMUpdateLocationJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMUpdateLocationJobDetail jobDetailInfo = null;
            foreach (JMUpdateLocationJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMUpdateLocationJobDetail;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.SiteCollectionURL;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.SourceUrl;
                datas[rowCount][3] = jobDetailInfo.DestinationUrl;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        //SrcRecordType ,DestRecordType ,TemplateName ,UniqueId ,Title ,Container,SrcLocation,LocationFullPath,Status,Comment
        private string[][] AssembleJMImportPhysicalRecordsJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[11];
            datas[0][0] = "Source Type";//I18NEntity.GetString("RM_JS_JMD_Grid_SrcRecordType");
            datas[0][1] = "Record Type"; I18NEntity.GetString("RM_JS_JMD_Grid_DestRecordType");
            datas[0][2] = "Template Name"; // I18NEntity.GetString("RM_JS_JMD_Grid_TemplateName");
            datas[0][3] = "Unique ID"; // I18NEntity.GetString("RM_JS_JMD_Grid_UniqueId");
            datas[0][4] = "Barcode"; // I18NEntity.GetString("RM_JS_JMD_Grid_UniqueId");
            datas[0][5] = "Title"; // I18NEntity.GetString("RM_JS_JMD_Grid_Title");
            datas[0][6] = "Container"; // I18NEntity.GetString("RM_JS_JMD_Grid_Container");
            datas[0][7] = "Source Location"; // I18NEntity.GetString("RM_JS_JMD_Grid_SrcLocation");
            datas[0][8] = "Location Path"; // I18NEntity.GetString("RM_JS_JMD_Grid_LocationFullPath");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][10] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] ConvertImportPhysicalRecordsJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportPhysicalRecordsJobDetail jobDetailInfo = null;
            foreach (JMImportPhysicalRecordsJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportPhysicalRecordsJobDetail;
                datas[rowCount] = new string[11];
                datas[rowCount][0] = jobDetailInfo.SrcRecordType;
                datas[rowCount][1] = jobDetailInfo.DestRecordType;
                datas[rowCount][2] = jobDetailInfo.TemplateName;
                datas[rowCount][3] = jobDetailInfo.UniqueId;
                datas[rowCount][4] = jobDetailInfo.Barcode;
                datas[rowCount][5] = jobDetailInfo.Title;
                datas[rowCount][6] = jobDetailInfo.Container;
                datas[rowCount][7] = jobDetailInfo.SrcLocation;
                datas[rowCount][8] = jobDetailInfo.LocationFullPath;
                datas[rowCount][9] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][10] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        //SrcRecordType ,DestRecordType ,TemplateName ,UniqueId ,Title ,Container,SrcLocation,LocationFullPath,Status,Comment
        private string[][] AssembleJMImportedRecordsDeletionJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = "Object Name";//I18NEntity.GetString("RM_JS_JMD_Grid_SrcRecordType");
            datas[0][1] = "Unique Id"; I18NEntity.GetString("RM_JS_JMD_Grid_DestRecordType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] ConvertImportedRecordsDeletionJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportedPhysicalRecordsDeletionDetail jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportedPhysicalRecordsDeletionDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.UniqueId;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMImportRecordsRelatedJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[15];
            datas[0][0] = "Unique ID";
            datas[0][1] = "Record Name";
            datas[0][2] = "Record Type";
            datas[0][3] = "Item ID";
            datas[0][4] = "Item Url";
            datas[0][5] = "Site ID";
            datas[0][6] = "Location/Site Url";

            datas[0][7] = "Related Name/Title";
            datas[0][8] = "Related Type";
            datas[0][9] = "Related Item Id";
            datas[0][10] = "Related Item Url";
            datas[0][11] = "Related Site Id";
            datas[0][12] = "Related Site Url";
            datas[0][13] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][14] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertImportRecordsRelatedJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportRecordsRelatedJobDetail jobDetailInfo = null;
            foreach (JMImportRecordsRelatedJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportRecordsRelatedJobDetail;
                datas[rowCount] = new string[15];
                datas[rowCount][0] = jobDetailInfo.SrcId;
                datas[rowCount][1] = jobDetailInfo.SrcName;
                datas[rowCount][2] = jobDetailInfo.SrcType;
                datas[rowCount][3] = jobDetailInfo.SrcItemId;
                datas[rowCount][4] = jobDetailInfo.SrcItemUrl;
                datas[rowCount][5] = jobDetailInfo.SrcSiteId;
                datas[rowCount][6] = jobDetailInfo.SrcLocation;

                datas[rowCount][7] = jobDetailInfo.DestName;
                datas[rowCount][8] = jobDetailInfo.DestType;
                datas[rowCount][9] = jobDetailInfo.DestItemId;
                datas[rowCount][10] = jobDetailInfo.DestItemUrl;
                datas[rowCount][11] = jobDetailInfo.DestSiteId;
                datas[rowCount][12] = jobDetailInfo.DestSiteUrl;

                datas[rowCount][13] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][14] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMAvailableSpaceReportJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LocationPath");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment"); return datas;
        }

        private string[][] AssembleJMTimeFrameSpaceReportJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMAvailableSpaceReportJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMAvailableSpaceReportJobDetail jobDetailInfo = null;
            foreach (JMAvailableSpaceReportJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMAvailableSpaceReportJobDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Location;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMTimeFrameReportJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMCreateAndDestroyedFileReportJobDetail jobDetailInfo = null;
            foreach (JMCreateAndDestroyedFileReportJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMCreateAndDestroyedFileReportJobDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectLevel;
                datas[rowCount][1] = jobDetailInfo.Title;
                datas[rowCount][2] = jobDetailInfo.URL;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMTermImportJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Term");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryExportProfileJobDetailsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_DA_Profile_ProfileName");
            datas[0][1] = I18NEntity.GetString("RM_DA_JMD_Grid_ProfileCriteria");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMArchiverFullTextIndexHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        public string[][] AssembleJMArchiverDeleteRestoredDataHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_RestoredUrl");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_CleanOption");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_CleanDelayDays");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_IsRelatedDelete");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryJobV2HeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryGoogleJobHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_GoogleDrive_Name");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryFileSystemJobHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Connection_Name");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryProfileJobHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DiscoveryProfileName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryGoogleProfileJobHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DiscoveryProfileName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_GoogleDrive_Name");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMManualApprovalHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[10];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ApprovalStatus");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_RecordOwner");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleCriteria");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMTermImportJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportTermDetail jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportTermDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Term;
                datas[rowCount][1] = jobDetailInfo.Action;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryExportProfileJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryExportProfileJobDetails jobDetailInfo = null;
            foreach (JMDiscoveryExportProfileJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryExportProfileJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ProfileName;
                datas[rowCount][1] = jobDetailInfo.ProfileCriteria;     
                datas[rowCount][2] = jobDetailInfo.Action;     
                datas[rowCount][3] = jobDetailInfo.Status.ToString();
                datas[rowCount][4] = jobDetailInfo.FinishTime;
                datas[rowCount][5] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMUniqueIDSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ColumnName");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_UniqueID");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMUniqueIDSettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMUniqueIDSettingJobDetails jobDetailInfo = null;
            foreach (JMUniqueIDSettingJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMUniqueIDSettingJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.SourceURL;
                datas[rowCount][2] = jobDetailInfo.ColumnName;
                datas[rowCount][3] = jobDetailInfo.UniqueID;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleOnPremiseJMUniqueIDSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ColumnName");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_UniqueID");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertOnPremiseJMUniqueIDSettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMUniqueIDSettingJobDetails jobDetailInfo = null;
            foreach (JMUniqueIDSettingJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMUniqueIDSettingJobDetails;
                datas[rowCount] = new string[8];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.SourceURL;
                datas[rowCount][2] = jobDetailInfo.ColumnName;
                datas[rowCount][3] = jobDetailInfo.UniqueID;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = jobDetailInfo.AgentName;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMAzureFileShareDataSyncTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDataCollectionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleOnPremiseJMDataCollectionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMDataCollectionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMCollectionDataJobDetails jobDetailInfo = null;
            foreach (JMCollectionDataJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMCollectionDataJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMAzureFileShareDataSyncToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            var rowCount = 1;
            foreach (var jobDetail in jobDetails)
            {
                var jobDetailInfo = jobDetail as JMAzureFileShareDataSyncDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = jobDetailInfo.ItemType;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                try
                {
                    datas[rowCount][4] = I18NEntity.GetStringWithSeparator(jobDetailInfo.Comment);
                }
                catch (Exception e)
                {
                    datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                }
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertOnPremiseJMDataCollectionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMCollectionDataJobDetails jobDetailInfo = null;
            foreach (JMCollectionDataJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMCollectionDataJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = jobDetailInfo.AgentName;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMSyncSecurityContainerHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            var index = 0;
            datas[0][index++] = I18NEntity.GetString("RM_JS_JMD_Grid_Container");
            datas[0][index++] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][index++] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][index++] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][index++] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMSecurityContianerToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMSyncSecurityContainerJobDetails jobDetailInfo = null;
            foreach (JMSyncSecurityContainerJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMSyncSecurityContainerJobDetails;
                datas[rowCount] = new string[5];
                var index = 0;
                datas[rowCount][index++] = jobDetailInfo.Container;
                datas[rowCount][index++] = jobDetailInfo.ObjectName;
                datas[rowCount][index++] = jobDetailInfo.FullPath;
                datas[rowCount][index++] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][index++] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMEnforceRetentionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMEnforceRetentionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int jobDetailsIndex = 1;
            JMEnforceRetentionJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMEnforceRetentionJobDetail;
                datas[jobDetailsIndex] = new string[5];
                datas[jobDetailsIndex][0] = jobDetailInfo.ObjectName;
                datas[jobDetailsIndex][1] = jobDetailInfo.SourceURL;
                datas[jobDetailsIndex][2] = jobDetailInfo.Action;
                datas[jobDetailsIndex][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[jobDetailsIndex][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                jobDetailsIndex++;
            }
            return datas;
        }

        private string[][] AssembleJMManualApprovalTimerHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ApprovalStatus");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_RecordOwner");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleCriteria");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMPhysicalTemplateImportHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_TemplateSuiteName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_TemplateSuiteStartFrom");
            datas[0][2] = I18NEntity.GetString("RM_PRM_TM_TemplateName_Title");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_TemplateType");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_TemplatePrefix");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_TemplateDigits");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMManualApprovalTimerDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMManualApprovalJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMManualApprovalJobDetails;
                datas[rowCount] = new string[9];
                datas[rowCount][0] = jobDetailInfo.ObjectLevel;
                datas[rowCount][1] = jobDetailInfo.TitleOrName;
                datas[rowCount][2] = jobDetailInfo.Url;
                datas[rowCount][3] = jobDetailInfo.ApprovalStatus;
                datas[rowCount][4] = jobDetailInfo.RecordOwner;
                datas[rowCount][5] = jobDetailInfo.RuleCriteria;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMPhysicalTemplateImportJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPhysicalTemplateImportJobDetail jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMPhysicalTemplateImportJobDetail;
                datas[rowCount] = new string[9];
                datas[rowCount][0] = jobDetailInfo.TemplateSuiteName;
                datas[rowCount][1] = jobDetailInfo.TemplateSuiteStartFrom;
                datas[rowCount][2] = jobDetailInfo.TemplateName;
                datas[rowCount][3] = jobDetailInfo.TemplateType;
                datas[rowCount][4] = jobDetailInfo.TemplatePrefix;
                datas[rowCount][5] = jobDetailInfo.TemplateDigits;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMRecordsExplorerMoveHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMRecordsExplorerMoveToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMExplorerMoveJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMExplorerMoveJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.DestinationFullPath;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMEXOApplySettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMEXODataSyncHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMEXOEnforceRuleActionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMEXOApplySettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMEXOApplySettingJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMEXOApplySettingJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.Action;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                datas[rowCount][6] = jobDetailInfo.Classification;
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMEXODataSynchronisationToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMEXODataSyncJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMEXODataSyncJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMEXOEnforceRuleActionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMEXOEnforceRuleActionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMEXOEnforceRuleActionJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.Action;
                datas[rowCount][1] = jobDetailInfo.ObjectName;
                datas[rowCount][2] = jobDetailInfo.ItemType;
                datas[rowCount][3] = jobDetailInfo.FullPath;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMPhysicalExplorerTimerHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMConnectorExplorerTimerHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ConnectorName");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_BCSTermName");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPhysicalExplorerTimerToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPhysicalExplorerTimerJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMPhysicalExplorerTimerJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.RuleName;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMConnectorExplorerTimerToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMConnectorTimerJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMConnectorTimerJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ConnectorName;
                datas[rowCount][2] = jobDetailInfo.TermName;
                datas[rowCount][3] = jobDetailInfo.RuleName;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }


        private string[][] AssembleJMPhysicalExportBarcodeHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleFullTextIndexListHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertFullTextIndexListDetailToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportFullTextIndexSClistJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMImportFullTextIndexSClistJobDetail;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMPhysicalExportBarcodeToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMExportBarcodeJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMExportBarcodeJobDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMExportSearchResultHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ExportLocation");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ReportName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMExportSearchResultToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMExportSearchResultJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMExportSearchResultJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ExportLocation;
                datas[rowCount][1] = jobDetailInfo.ReportName;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMPhysicalDisposalHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_FullPathForPhysical");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_ActionType");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationPath");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPhysicalDisposalToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 8;
            JMPhysicalDisposalJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMPhysicalDisposalJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.RuleName;
                datas[rowCount][4] = jobDetailInfo.ActionType;
                datas[rowCount][5] = jobDetailInfo.DestinationPath;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMPhysicalSetPermissionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPhysicalSetPermissionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMExportBarcodeJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMExportBarcodeJobDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ItemType;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMSPOnPremDashBoardHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMSPOnPremDashBoardToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMSPOnPremDashBoardJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMSPOnPremDashBoardJobDetail;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.Action;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMDashBoardHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][1] = I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMDashBoardToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDashboardJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMDashboardJobDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Action;
                datas[rowCount][1] = jobDetailInfo.SourceFlag;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMManualApprovalEmailScheduleHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMManualApprovalEmailScheduleToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMManualApprovalSettingScheduleDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMManualApprovalSettingScheduleDetail;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.TitleOrName;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMFSDashBoardHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMFSDashBoardToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSDashBoardJobDetail jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMFSDashBoardJobDetail;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.Action;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMFSDataSyncHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_LocationPath");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMFSDataSyncToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            FSDataSyncJobReportDetail jobDetailInfo = null;
            foreach (FSDataSyncJobReportDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as FSDataSyncJobReportDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = jobDetailInfo.AgentName;
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMFSImportSettingHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMFSImportSettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            FSDataSyncJobReportDetail jobDetailInfo = null;
            foreach (FSDataSyncJobReportDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as FSDataSyncJobReportDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] ConvertJMFSDisposalToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSDisposalJobDetails jobDetailInfo = null;
            foreach (JMFSDisposalJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMFSDisposalJobDetails;
                datas[rowCount] = new string[11];
                //datas[rowCount][0] = jobDetailInfo.DetailTab;
                datas[rowCount][0] = jobDetailInfo.Type;
                datas[rowCount][1] = jobDetailInfo.ObjectName;
                datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.Size);
                datas[rowCount][3] = jobDetailInfo.SourceLocation;
                datas[rowCount][4] = jobDetailInfo.DestinationLocation;
                datas[rowCount][5] = jobDetailInfo.FinishTime;
                datas[rowCount][6] = jobDetailInfo.RuleName;
                datas[rowCount][7] = jobDetailInfo.Action;
                datas[rowCount][8] = jobDetailInfo.AgentName;
                datas[rowCount][9] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][10] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMFSDisposalHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[11];
            //datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailTab");
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_BackupSourceURL");         //RM_JS_JMD_Grid_SourceURL
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][10] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMFSFolderReclassifyHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMFSFolderHoldHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMGlobalSearchActionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMSmartTermActionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMSyncRemoteNodesHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Container");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMScanLocalNodesHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMOnremiseEnforceRuleActionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[11];
            //datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailTab");
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL");
            //datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_AgentName");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMArchiverMigrationHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_ItemType"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertJMOnremiseEnforceRuleActionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMOnPremiseSPEnforceRuleActionJobDetails jobDetailInfo = null;
            foreach (JMOnPremiseSPEnforceRuleActionJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMOnPremiseSPEnforceRuleActionJobDetails;
                datas[rowCount] = new string[11];
                //datas[rowCount][0] = jobDetailInfo.DetailTab;
                datas[rowCount][0] = jobDetailInfo.Type;
                datas[rowCount][1] = jobDetailInfo.ObjectName;
                datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.Size);
                datas[rowCount][3] = jobDetailInfo.SourceLocation;
                //datas[rowCount][3] = jobDetailInfo.DestinationLocation;
                datas[rowCount][4] = jobDetailInfo.FinishTime;
                datas[rowCount][5] = jobDetailInfo.RuleName;
                datas[rowCount][6] = jobDetailInfo.Action;
                datas[rowCount][7] = jobDetailInfo.AgentName;
                datas[rowCount][8] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][9] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMFSFolderReclassifyToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSReclassifierJobDetails jobDetailInfo = null;
            foreach (JMFSReclassifierJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMFSReclassifierJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMFSFolderHoldToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSHoldJobDetails jobDetailInfo = null;
            foreach (JMFSHoldJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMFSHoldJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = jobDetailInfo.Action;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMGlobalSearchActionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGlobalSearchActionJobDetails jobDetailInfo = null;
            foreach (JMGlobalSearchActionJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGlobalSearchActionJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.Type;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.Action;
                datas[rowCount][4] = jobDetailInfo.DestinationLocation;
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMSmartTermActionToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGlobalSearchActionJobDetails jobDetailInfo = null;
            foreach (JMGlobalSearchActionJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGlobalSearchActionJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.Type;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = jobDetailInfo.Action;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMSyncRemoteNodesToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMSyncRemoteNodesJobDetails jobDetailInfo = null;
            foreach (var jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMSyncRemoteNodesJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.Container;
                datas[rowCount][1] = jobDetailInfo.ObjectName;
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.ItemType);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Action);
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMScanLocalNodesToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMScanLocalNodesJobDetails jobDetailInfo = null;
            foreach (var jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMScanLocalNodesJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.ItemType);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Action);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.AgentName);
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string ConvertJobDetailsStatusToString(JobDetailsStatus status)
        {
            string result = null;
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Successful");
                    break;
                case JobDetailsStatus.Failed:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Failed");
                    break;
                case JobDetailsStatus.Skipped:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Skipped");
                    break;
                case JobDetailsStatus.Pending:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Pending");
                    break;
                case JobDetailsStatus.Exception:
                    result = I18NEntity.GetString("RM_JS_JMD_Status_Exception");
                    break;
            }
            return result;
        }

        private string[][] ConvertJMManualApprovalDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMManualApprovalJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMManualApprovalJobDetails;
                datas[rowCount] = new string[9];
                datas[rowCount][0] = jobDetailInfo.ObjectLevel;
                datas[rowCount][1] = jobDetailInfo.TitleOrName;
                datas[rowCount][2] = jobDetailInfo.Url;
                datas[rowCount][3] = jobDetailInfo.ApprovalStatus;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = jobDetailInfo.RecordOwner;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = jobDetailInfo.RuleCriteria;
                datas[rowCount][8] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMArchiverFullTextIndexDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverFullTextIndexJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMArchiverFullTextIndexJobDetails;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);
                
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMArchiverDeleteRestoredDataDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverDeleteRestoredDataJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMArchiverDeleteRestoredDataJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = jobDetailInfo.RestoredUrl;
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.CleanOption);
                datas[rowCount][3] = jobDetailInfo.CleanDelayDays.ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.IsRelatedDelete);
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryJobV2DetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryJobV2Details jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryJobV2Details;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryGoogleJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryGoogleJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryGoogleJobDetails;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.DriveName;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryFileSystemJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryFileSystemJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryFileSystemJobDetails;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = jobDetailInfo.ConnectionName;
                datas[rowCount][1] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][2] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryProfileJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryProfileJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryProfileJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ProfileName;
                datas[rowCount][1] = jobDetailInfo.Url;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryGoogleProfileJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDiscoveryGoogleProfileJobDetails jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDiscoveryGoogleProfileJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ProfileName;
                datas[rowCount][1] = jobDetailInfo.DriveName;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleBoxDataSyncTittle(string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_ItemType"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] AssembleGoogleDisposalTittle(string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_DetailsTab"),
                I18NEntity.GetString("RM_Google_Level"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_Export_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_RuleName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                I18NEntity.GetString("RM_JS_JM_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }
        
        private string[][] AssembleSalesforceTittle(string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectType"),
                I18NEntity.GetString("RM_JS_JMD_Grid_TotalItemCount"),
                I18NEntity.GetString("RM_JS_JMD_Grid_TotalSize") + " (kB)",
                I18NEntity.GetString("RM_JS_JM_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] AssembleGoogleDataSyncTittle(string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_ItemType"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }
        
        private string[][] AssembleGoogleApplySettingTittle(string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_ItemType"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Classification"),                
                I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertBoxDataSyncToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMBoxDataSyncDetail jobDetailInfo = null;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                int colCount = 0;
                jobDetailInfo = jobDetail as JMBoxDataSyncDetail;
                datas[rowCount] = new string[5];
                datas[rowCount][colCount++] = jobDetailInfo.ObjectName;
                datas[rowCount][colCount++] = jobDetailInfo.FullPath;
                datas[rowCount][colCount++] = jobDetailInfo.ItemType;
                datas[rowCount][colCount++] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][colCount++] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }
        
        private string[][] ConvertGoogleApplySettingToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGoogleJobDetails jobDetailInfo;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGoogleJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = jobDetailInfo.ItemType;
                datas[rowCount][3] = jobDetailInfo.Classification;
                datas[rowCount][4] = jobDetailInfo.Action;
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertGoogleDisposalToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            Dictionary<int, string> actions = new Dictionary<int, string> { { 0, "RM_JM_Tab_DetailFilter_Scan" }, { 1, "RM_JM_Tab_DetailFilter_Export" }, { 2, "RM_JM_Tab_DetailFilter_Backup" }, { 3, "RM_JM_Tab_DetailFilter_Action" } };
            JMArchiverActionJobDetails jobDetailInfo;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverActionJobDetails;
                datas[rowCount] = new string[9];
                datas[rowCount][0] = I18NEntity.GetString(actions[jobDetailInfo.ActionTab]);
                datas[rowCount][1] = jobDetailInfo.Level;
                datas[rowCount][2] = jobDetailInfo.SourceLocation;
                datas[rowCount][3] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = jobDetailInfo.RuleName;
                datas[rowCount][5] = jobDetailInfo.Action;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][7] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][8] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        
        private string[][] ConvertSalesforceToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            foreach (var job in jobDetails)
            {
                var jobDetailInfo = job as JMSalesforceDiscoveryJob;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.ObjectType;
                datas[rowCount][2] = jobDetailInfo.TotalItemCount.ToString();
                datas[rowCount][3] = jobDetailInfo.TotalSize.ToString();
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertGoogleDataSyncToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGoogleDataSyncJobDetails jobDetailInfo;
            foreach (JMJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGoogleDataSyncJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = jobDetailInfo.ItemType;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string ConvertJobTypeToString(JobType jobType)
        {
            string result = null;
            switch (jobType)
            {
                case JobType.TermSynchronization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TermSynchronization"); ;
                    break;
                case JobType.DeleteRestoredData:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DeleteRestoredData"); ;
                    break;
                case JobType.DiscoveryJobV2:
                case JobType.DiscoveryJobV3:
                case JobType.DiscoveryJobV4:
                case JobType.DiscoveryJobV5:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryJobV2");
                    break;
                case JobType.DiscoveryAOSPJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryAOSPJob");
                    break;
                case JobType.DiscoveryGoogleJobV1:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryGoogleJobV1");
                    break;
                case JobType.DiscoveryProfileJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryProfileJob");
                    break;
                case JobType.DiscoveryGoogleProfileJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryGoogleProfileJob");
                    break;
                case JobType.DiscoveryAnalysisFileSystemV1:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_AnalysisFileSystemJob");
                    break;
                case JobType.ItemsFilesDueDisposal:
                case JobType.EXOItemsFilesDueDisposalReport:
                case JobType.PhysicalItemsFilesDueDisposalReport:
                case JobType.FSItemsFilesDueDisposal:
                case JobType.SPOnPremItemsFilesDueDisposal:
                case JobType.BoxItemsFilesDueDisposalReport:
                case JobType.GoogleItemsFilesDueDisposalReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ItemsFilesDueDisposal"); ;
                    break;
                case JobType.BCSTermUsageReport:
                case JobType.EXOTermUsageReport:
                case JobType.PhysicalTermUsageReport:
                case JobType.FSBCSTermUsageReport:
                case JobType.SPOnPremBCSTermUsageReport:
                case JobType.BoxBCSTermUsageReport:
                case JobType.GoogleBCSTermUsageReport:
                case JobType.TeamsBCSTermUsageReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_BCSTermUsageReport"); ;
                    break;
                case JobType.SharePointGlobalSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SharePointGlobalSetting");
                    break;
                case JobType.SharePointCustomSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SharePointCustomSetting"); ;
                    break;
                case JobType.SharePointInheritSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SharePointInheritSetting");
                    break;
                case JobType.UpdateLocation:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_UpdateLocation");
                    break;
                case JobType.SharePointScheduleSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SharePointScheduleSetting");
                    break;
                case JobType.ArchiverFullTextIndex:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverFullTextIndex");
                    break;
                case JobType.ApplySharePointSettings:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ApplySharePointSettings");
                    break;
                case JobType.AvailableSpaceReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_AvailableSpaceReport");
                    break;
                case JobType.CreateAndDestroyedFileReport:
                case JobType.EXOCreateAndDestroyedFileReport:
                case JobType.PhysicalCreateAndDestroyedFileReport:
                case JobType.FSCreateAndDestroyedFileReport:
                case JobType.OneDriveCreateAndDestroyedFileReport:
                case JobType.SPOnPremCreateAndDestroyedFileReport:
                case JobType.BoxCreateAndDestroyedFileReport:
                case JobType.GoogleCreateAndDestroyedFileReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_CreateAndDestroyedFileReport");
                    break;
                case JobType.PhysicalFolderSynchronization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalFolderSynchronization");
                    break;
                case JobType.PhysicalTermSynchronization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalTermSynchronization");
                    break;
                case JobType.ImportPhysicalRecords:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ImportPhysicalRecords");
                    break;
                case JobType.TrimRecordsDeletion:
                    result = "TRIM Import Records Deletion";
                    break;
                case JobType.ImportRecordsRelated:
                    result = "Import Records Related";
                    break;
                case JobType.ImportTermStructure:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ImportTermStructure");
                    break;
                case JobType.ImportGoogleTermStructure:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ImportGoogleTermStructure");
                    break;
                case JobType.ExportTermStructure:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ExportTermStructure");
                    break;
                case JobType.ManualApproval:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualApproval");
                    break;
                case JobType.ManualApprovalOrRejectJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualApprovalOrRejectJob");
                    break;
                case JobType.ManualFolderViewActions:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualFolderViewActions");
                    break;
                case JobType.UniqueIDSettingFullSchedule:
                case JobType.SPOnPremUniqueIDSettingFullSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_UniqueIDSettingFullSchedule");
                    break;
                case JobType.UniqueIDSettingIncrementalSchedule:
                case JobType.TeamsUniqueIDSettingIncrementalSchedule:
                case JobType.SPOnPremUniqueIDSettingIncrementalSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_UniqueIDSettingIncrementalSchedule");
                    break;
                case JobType.CollectionDataFull:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_CollectionDataFull");
                    break;
                case JobType.CollectionDataIncremental:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_CollectionDataIncremental");
                    break;
                case JobType.ManualApprovalTimer:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualApprovalTimer");
                    break;
                case JobType.DiscoveryJob:
                    result = "Discovery";
                    break;
                case JobType.DiscoveryReCalculate:
                    result = "Discovery Re Calculate";
                    break;
                case JobType.DiscoveryOptimizationCalculate:
                    result = "Discovery Optimization Calculate";
                    break;
                case JobType.DiscoveryAOSPOptimizationCalculate:
                    result = "Discovery AOSP Optimization Calculate";
                    break;
                case JobType.ManualFileSystemUpgrade:
                    result = "Manual Approval File System Upgrade";
                    break;
                case JobType.SharePointOnlineDeletionSyncUpgrade:
                    result = "SharePoint Online Deletion Sync Upgrade";
                    break;
                case JobType.SendEmailJob:
                    result = "Send Email Job.";
                    break;
                case JobType.CosmosDBDirtyDataDeleteUpgrade:
                    result = "Cosmos DB Dirty Data Delete Upgrade";
                    break;
                case JobType.EnforceRetention:
                case JobType.OldEnforceRetention:
                case JobType.EXOEnforceRetention:
                case JobType.OneDriveEnforceRetention:
                case JobType.TeamsEnforceRetention:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_EnforceRetention");
                    break;
                case JobType.DisposalActivityManagement:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DisposalActivityManagement");
                    break;
                case JobType.DataSynchronisation:
                case JobType.SPDataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DataSynchronisation");
                    break;
                case JobType.RecordsExplorerMove:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_RecordsExplorerMove");
                    break;
                case JobType.EXOApplySetting:
                case JobType.EXOApplySettingSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_EXOApplySetting");
                    break;
                case JobType.EXODataSynchronisation:
                case JobType.EXODataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_EXODataSynchronisation");
                    break;
                case JobType.PhysicalExplorerTimer:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalExplorerTimer");
                    break;
                case JobType.ConnectorTimer:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ConnectorTimer");
                    break;
                case JobType.PhysicalDisposal:
                case JobType.PhysicalRecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalDisposal");
                    break;
                case JobType.ImportSPSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ImportSPSetting");
                    break;
                case JobType.PhysicalExportBarcode:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalExportBarcode");
                    break;
                case JobType.PhysicalSetPermission:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalSetPermission");
                    break;
                case JobType.FSDataSynchronization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSDataSynchronization");
                    break;
                case JobType.ImportFSSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ImportSPSetting");
                    break;
                case JobType.FSDataSynchronizationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSDataSynchronizationSchedule");
                    break;
                case JobType.FSDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSDisposal");
                    break;
                case JobType.FSDisposalSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSDisposalSchedule");
                    break;
                case JobType.FSDisposalByClassCode:
                    result = I18NEntity.GetString("RM_JS_FS_DisposalOnSpecificClassCode");
                    break;
                case JobType.FSDashBoard:
                case JobType.SPOnPremDashBoard:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSDashBoard");
                    break;
                case JobType.Dashboard:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_Dashboard");
                    break;
                case JobType.TenantUpgrade:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TenantUpgrade");
                    break;
                case JobType.ManualApprovalEmailSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualApprovalEmailSchedule");
                    break;
                case JobType.FSFolderChangeTerm:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSFolderChangeTerm");
                    break;
                case JobType.FSFolderManageHold:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSFolderManageHold");
                    break;
                case JobType.SyncNodesFromAOS:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SyncNodesFromAOS");
                    break;
                case JobType.SPOnPremScanLocalNodes:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremScanLocalNodes");
                    break;
                case JobType.GlobalSearchAction:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_GlobalSearchAction");
                    break;
                case JobType.SPOnPremApplySetting:
                case JobType.SPOnPremApplySettingSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremApplySetting");
                    break;
                case JobType.SPOnPremTermSynchronization:
                case JobType.SPOnPremTermSynchronizationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremTermSynchronization");
                    break;
                case JobType.SPOnPremEnforceRuleAction:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremEnforceRuleAction");
                    break;
                case JobType.SPOnPremEnforceRuleActionSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremEnforceRuleActionSchedule");
                    break;
                case JobType.SPOnPremDataSync:
                case JobType.SPOnPremDataSyncSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SPOnPremDataSync");
                    break;
                case JobType.OneDriveDataSynchronisation:
                case JobType.OneDriveDataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_OneDriveDataSynchronisation");
                    break;
                case JobType.AzureFileShareDataSynchronisation:
                case JobType.AzureFileShareDataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_AzureFileShareDataSynchronisation");
                    break;
                case JobType.OneDriveItemsFilesDueDisposalReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_OneDriveItemsFilesDueDisposalReport");
                    break;
                case JobType.OneDriveTermUsageReport:
                case JobType.OneDriveOrphanedTermUsageReport:
                case JobType.OneDriveRetiredTermUsageReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_OneDriveTermUsageReport");
                    break;
                case JobType.ExportSearchResult:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ExportSearchResult");
                    break;
                case JobType.PhysicalLoanBox:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalLoanBox");
                    break;
                case JobType.PhysicalReturnBox:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalReturnBox");
                    break;
                case JobType.PhysicalMovePickExportJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalMovePickExportJob");
                    break;
                case JobType.PhysicalMoveDataJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalMoveDataJob");
                    break;
                case JobType.EXORecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_EXORecordsDisposal");
                    break;
                case JobType.ManualExportHistoryDatasJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportHistoryDatasJob");
                    break;
                case JobType.ExportReportDetails:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ExportReportDetails");
                    break;
                case JobType.ManualExportRecordsForReviewDatasJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportRecordsForReviewDatasJob");
                    break;
                case JobType.DeleteInvalidRecords:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DeleteInvalidRecords");
                    break;
                case JobType.ExportFSSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ExportFSSetting");
                    break;
                case JobType.DownloadRCCReport:
                    result = I18NEntity.GetString("RM_FS_DownloadRCCReport");
                    break;
                case JobType.SharePointSiteMetricsReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SharePointReportExport");
                    break;
                case JobType.ExportSPSetting:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ExportSPSetting");
                    break;
                case JobType.ManualImportUnderReviewDatasJob:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ManualImportUnderReviewDatasJob");
                    break;
                case JobType.RecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_RecordsDisposal");
                    break;
                case JobType.OneDriveRecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_OneDriveRecordsDisposal");
                    break;
                case JobType.ArchiverByHSMXml:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverByHSMXml");
                    break;
                case JobType.CleanUpDuplicateDatas:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_CleanUpDuplicateDatas");
                    break;
                case JobType.RMArchiverBackup:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_RMArchiverBackup");
                    break;
                case JobType.RMEndUserArchiverBackup:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_RMEndUserArchiverBackup");
                    break;
                case JobType.SOPreScan:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_SOPreScan");
                    break;
                case JobType.ArchiverRestore:
                case JobType.AOSPRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverRestore");
                    break;
                case JobType.ArchiverToSpoRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverToSpoRestore");
                    break;
                case JobType.StubArchiverRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_StubArchiverRestore");
                    break;
                case JobType.M365InPlaceArchiverRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_M365InPlaceArchiverRestore");
                    break;
                case JobType.StubOopRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_StubOopRestore");
                    break;
                case JobType.ArchiverOutPlaceRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverOutPlaceRestore");
                    break;
                case JobType.ArchiverMoveIndex:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverMoveIndex");
                    break;
                case JobType.ArchiverRetention:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverRetention");
                    break;
                case JobType.VeoMerge:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_VeoMerge");
                    break;
                case JobType.ArchiverExport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverExport");
                    break;
                case JobType.DiscoverOptimization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoverOptimization");
                    break;
                case JobType.DiscoveryAOSPOptimization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoverAOSPOptimization");
                    break;
                case JobType.DiscoveryPreScan:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryPreScan");
                    break;
                case JobType.BoxDataSynchronisation:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_BoxDataSynchronisation");
                    break;
                case JobType.BoxDataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_BoxDataSynchronisationSchedule");
                    break;
                case JobType.BoxRecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_BoxRecordsDisposal");
                    break;
                case JobType.GoogleApplySettings:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_GoogleApplySettings");
                    break;
                case JobType.GoogleDataSynchronization:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_GoogleDataSynchronization");
                    break;
                case JobType.GoogleRecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_GoogleRecordsDisposal");
                    break;
                case JobType.DeleteOrphanDatas:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_DeleteOrphanDatas");
                    break;
                case JobType.PhysicalTemplateImport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_PhysicalTemplateImport");
                    break;
                case JobType.TeamsDataSynchronisation:
                case JobType.TeamsDataSynchronisationSchedule:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsDataSynchronisation");
                    break;
                case JobType.TeamsRecordsDisposal:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsRecordsDisposal");
                    break;
                case JobType.TeamsArchiverRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsArchiverRestore");
                    break;
                case JobType.TeamsOutPlaceRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsOutPlaceRestore");
                    break;
                case JobType.MailBoxArchiverRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_MailboxArchiverRestore");
                    break;
                case JobType.TeamsArchiverRetention:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsArchiverRetention");
                    break;
                case JobType.TeamsCreateAndDestroyedFileReport:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_CreateAndDestroyedFileReport");
                    break;
                case JobType.EXOArchiverRetention:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_EXOArchiverRetention");
                    break;
                case JobType.TeamsPreScan:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsPreScan");
                    break;
                case JobType.ArchiverRetentionSimulate:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverRetentionSimulate");
                    break;
                case JobType.FSRetainSimulate:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_FSRetainSimulate");
                    break;
                case JobType.GoogleArchiverRestore:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsArchiverRestore");
                    break;
                case JobType.GoogleArchiverRetention:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_TeamsArchiverRetention");
                    break;
                default:
                    result = I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)jobType).ToString());
                    break;
            }
            return result;
        }

        private string ConvertJobStatusToString(JobStatus jobStatus)
        {
            string result = null;
            switch (jobStatus)
            {
                case JobStatus.Wait:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Wait");
                    break;
                case JobStatus.InProgress:
                    result = I18NEntity.GetString("RM_JS_JM_Status_InProgerss");
                    break;
                case JobStatus.Finished:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Finished");
                    break;
                case JobStatus.Failed:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Failed");
                    break;
                case JobStatus.FinishWithException:
                    result = I18NEntity.GetString("RM_JS_JM_Status_FinishWithException");
                    break;
                case JobStatus.Stopped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopped");
                    break;
                case JobStatus.Skipped:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Skipped");
                    break;
                case JobStatus.Stopping:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Stopping");
                    break;
                case JobStatus.Pending:
                    result = I18NEntity.GetString("RM_JS_JM_Status_Pending");
                    break;
            }
            return result;
        }

        private string[][] AssembleJMPhyLoanBoxHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPhyLoanBoxToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPhyBoxLoanJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMPhyBoxLoanJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Name;
                datas[rowCount][1] = jobDetailInfo.Level.ToString();
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMPhysicalMoveHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_UniqueID");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_ItemType");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPhysicalMoveJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPhysicalMoveJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMPhysicalMoveJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.UniqueId;
                datas[rowCount][1] = jobDetailInfo.ObjectName;
                datas[rowCount][2] = jobDetailInfo.ItemType;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMActionAuditReportHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Count");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMActionAuditReportToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMClientAuditReportJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMClientAuditReportJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.Type;
                datas[rowCount][1] = jobDetailInfo.ObjectPath;
                datas[rowCount][2] = jobDetailInfo.Count;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMPickCompleteHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPickCompleteToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMPickCompleteJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMPickCompleteJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Name;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMTrainingJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Classification");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMTrainingJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMTrainingJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMTrainingJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.TermName;
                datas[rowCount][1] = jobDetailInfo.FileName;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMApplyClassCodeJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] ConvertJMApplyClassCodeJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSReclassifierJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMFSReclassifierJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMTrainingAnalyseJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[4];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Classification");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMTrainingAnalyseJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMTrainingJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMTrainingJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.TermName;
                datas[rowCount][1] = jobDetailInfo.FullPath;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMArchiverActionJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[10];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailsTab");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Level");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationUrl");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] AssembleJMHSMArchiverActionJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailsTab");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Level");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMArchiverActionJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas,string siteCollectionUrl)
        {
            int rowCount = 1;
            Dictionary<int, string> actions = new Dictionary<int, string> { { 0, "RM_JM_Tab_DetailFilter_Scan" }, { 1, "RM_JM_Tab_DetailFilter_Export" }, { 2, "RM_JM_Tab_DetailFilter_Backup" }, { 3, "RM_JM_Tab_DetailFilter_Action" } };

            JMArchiverActionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverActionJobDetails;
                if (!string.IsNullOrEmpty(siteCollectionUrl))
                {
                    if (!jobDetailInfo.SourceLocation.StartsWith(siteCollectionUrl))
                    {
                        logger.Info($"this detail is not start with site url,do not export.{jobDetailInfo.SourceLocation}");
                        continue;
                    }
                }
                datas[rowCount] = new string[10];
                datas[rowCount][0] = I18NEntity.GetString(actions[jobDetailInfo.ActionTab]);
                datas[rowCount][1] = jobDetailInfo.Level;
                datas[rowCount][2] = jobDetailInfo.SourceLocation;
                datas[rowCount][3] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][6] = jobDetailInfo.RuleName;
                datas[rowCount][7] = jobDetailInfo.Action;
                datas[rowCount][8] = jobDetailInfo.DestinationLocation;
                datas[rowCount][9] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }
        private string[][] ConvertJMHSMArchiverActionJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas, string siteCollectionUrl)
        {
            int rowCount = 1;
            Dictionary<int, string> actions = new Dictionary<int, string> { { 0, "RM_JM_Tab_DetailFilter_Scan" }, { 1, "RM_JM_Tab_DetailFilter_Export" }, { 2, "RM_JM_Tab_DetailFilter_Backup" }, { 3, "RM_JM_Tab_DetailFilter_Action" } };

            JMArchiverActionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverActionJobDetails;
                if (!string.IsNullOrEmpty(siteCollectionUrl))
                {
                    if (!jobDetailInfo.SourceLocation.StartsWith(siteCollectionUrl))
                    {
                        logger.Info($"this detail is not start with site url,do not export.{jobDetailInfo.SourceLocation}");
                        continue;
                    }
                }
                datas[rowCount] = new string[10];
                datas[rowCount][0] = I18NEntity.GetString(actions[jobDetailInfo.ActionTab]);
                datas[rowCount][1] = jobDetailInfo.Level;
                datas[rowCount][2] = jobDetailInfo.SourceLocation;
                datas[rowCount][3] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][6] = jobDetailInfo.Action;
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMPreScanJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[12];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Level");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_CreatedTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_CreatedBy");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_ModifiedTime");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_ModifiedBy");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][10] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][11] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMDiscoveryPreScanJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[13];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_DetailsTab");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_RuleName");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_CreatedTime");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_CreatedBy");
            datas[0][8] = I18NEntity.GetString("RM_JS_JMD_Grid_ModifiedTime");
            datas[0][9] = I18NEntity.GetString("RM_JS_JMD_Grid_ModifiedBy");
            datas[0][10] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][11] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][12] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMPreScanJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;

            JMArchiverActionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverActionJobDetails;
                datas[rowCount] = new string[12];
                datas[rowCount][0] = jobDetailInfo.Level;
                datas[rowCount][1] = jobDetailInfo.SourceLocation;
                datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][3] = jobDetailInfo.RuleName;
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Action);
                datas[rowCount][5] = jobDetailInfo.CreatedTime;
                datas[rowCount][6] = jobDetailInfo.CreatedBy;
                datas[rowCount][7] = jobDetailInfo.ModifiedTime;
                datas[rowCount][8] = jobDetailInfo.ModifiedBy;
                datas[rowCount][9] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][10] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][11] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMDiscoveryPreScanJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            Dictionary<int, string> actions = new Dictionary<int, string> { { 0, "RM_JM_Tab_DetailFilter_Scan" }, { 1, "RM_JM_Tab_DetailFilter_Export" }, { 2, "RM_JM_Tab_DetailFilter_Backup" }, { 3, "RM_JM_Tab_DetailFilter_Action" } };

            JMArchiverActionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverActionJobDetails;
                datas[rowCount] = new string[13];
                datas[rowCount][0] = I18NEntity.GetString(actions[jobDetailInfo.ActionTab]);
                datas[rowCount][1] = jobDetailInfo.Level;
                datas[rowCount][2] = jobDetailInfo.SourceLocation;
                datas[rowCount][3] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = jobDetailInfo.RuleMatchFile;
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Action);
                datas[rowCount][6] = jobDetailInfo.CreatedTime;
                datas[rowCount][7] = jobDetailInfo.CreatedBy;
                datas[rowCount][8] = jobDetailInfo.ModifiedTime;
                datas[rowCount][9] = jobDetailInfo.ModifiedBy;
                datas[rowCount][10] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][11] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][12] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMMoveIndexJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_SrcStorageName");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_DesStorageName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMMoveIndexJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverMoveIndexJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverMoveIndexJobDetails;
                datas[rowCount] = new string[10];
                datas[rowCount][0] = jobDetailInfo.SiteUrl;
                datas[rowCount][1] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][2] = jobDetailInfo.SrcStorageName;
                datas[rowCount][3] = jobDetailInfo.DesStorageName;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMArchiveRetentionJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_JM_JobID");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_SrcStorageName");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_DesStorageName");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Action");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][7] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMArchiveRetentionJobSimulateHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_DSB_Retention_Column_FileName");
            datas[0][1] = I18NEntity.GetString("RM_DSB_Retention_Column_Url");
            datas[0][2] = I18NEntity.GetString("RM_DSB_Retention_Column_ContentSource");
            datas[0][3] = I18NEntity.GetString("RM_DSB_Retention_Column_Size");
            datas[0][4] = I18NEntity.GetString("RM_DSB_Retention_Column_Setting");
            datas[0][5] = I18NEntity.GetString("RM_DSB_Retention_Column_Storage");
            return datas;
        }
        private string[][] AssembleJMDeleteOrphanDatasJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][1] = I18NEntity.GetString("RM_JS_JM_JobID");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMArchiveRetentionSimulateJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverRententionDashboardDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverRententionDashboardDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.FileName;
                datas[rowCount][1] = jobDetailInfo.SiteUrl;
                datas[rowCount][2] = TelemetryUtility.ConvertSourceFlag(jobDetailInfo.SourceFlag);
                datas[rowCount][3] = string.IsNullOrEmpty(jobDetailInfo.SizeStr) ? jobDetailInfo.SizeStr : ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = string.Format(I18NEntity.GetString("RM_DSB_Retention_Column_SettingValue"), jobDetailInfo.RetentionSource, jobDetailInfo.RetentionKeepDate, GenerateRetentionKeepDateUnitStr(jobDetailInfo.RetentionKeepDateUnit));
                datas[rowCount][5] = jobDetailInfo.SrcStorageName;
                rowCount++;
            }
            return datas;

            string GenerateRetentionKeepDateUnitStr(int retentionKeepDateUnit)
            {
                switch (retentionKeepDateUnit)
                {
                    case 0:
                        return I18NEntity.GetString("RM_DSB_Retention_DayUnit");
                    case 1:
                        return I18NEntity.GetString("RM_DSB_Retention_WeekUnit");
                    case 2:
                        return I18NEntity.GetString("RM_DSB_Retention_Column_Storage");
                    case 3:
                        return I18NEntity.GetString("RM_DSB_Retention_YearUnit");
                }
                return "";
            }
        }

        private string[][] ConvertJMArchiveRetentionJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverRententionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverRententionJobDetails;
                datas[rowCount] = new string[8];
                datas[rowCount][0] = jobDetailInfo.SiteUrl;
                datas[rowCount][1] = jobDetailInfo.JobId;
                datas[rowCount][2] = string.IsNullOrEmpty(jobDetailInfo.SizeStr) ?jobDetailInfo.SizeStr:ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][3] = jobDetailInfo.SrcStorageName;
                datas[rowCount][4] = jobDetailInfo.DesStorageName;
                datas[rowCount][5] = jobDetailInfo.Action;
                datas[rowCount][6] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][7] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] ConvertJMDeleteOrphanDatasJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDeleteOrphanDatasJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMDeleteOrphanDatasJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.SiteUrl;
                datas[rowCount][1] = jobDetailInfo.JobId;
                datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMArchiverDedupJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_DC_FileName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_DedupTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment")
            };
            return datas;
        }
        private string[][] ConvertJMArchiverDedupJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverDedupJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverDedupJobDetails;
                datas[rowCount] = new string[]
                {
                    jobDetailInfo.Name,
                    jobDetailInfo.SrcURL,
                    ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr),
                    jobDetailInfo.DedupTimeStr,
                    ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString(),
                    I18NEntity.GetString(jobDetailInfo.Comment)
                };

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMArchiverRestoreJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] AssembleJMFSRestoreJobHeaderTitle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_LocationPath");
            datas[0][1] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMArchiverRestoreJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMRestoreActionJobDetailes jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMRestoreActionJobDetailes;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.Level;
                datas[rowCount][1] = jobDetailInfo.SourceLocation;
                datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][4] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMFSRestoreJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMFSRestoreJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMFSRestoreJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.SourceLocation;
                datas[rowCount][1] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][3] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }


        private string[][] AssembleJMArchiverVEOJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_DC_FileName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_LocationPath");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_DestinationPath");
            datas[0][3] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMArchiverVEOJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMVEOMergeJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMVEOMergeJobDetails;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.FileName;
                datas[rowCount][1] = jobDetailInfo.SourceLocation;
                datas[rowCount][2] = jobDetailInfo.DestinationLocation;
                datas[rowCount][3] = ConvertUnitUtil.ConvertToKB(jobDetailInfo.SizeStr);
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString();
                datas[rowCount][5] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);

                rowCount++;
            }
            return datas;
        }

        private string[][] ConvertJMArchiverMigrationDataToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverMigrationJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverMigrationJobDetails;
                datas[rowCount] = new string[]
                {
                    jobDetailInfo.ObjectName,
                    jobDetailInfo.ObjectType,
                    ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString(),
                    I18NEntity.GetString(jobDetailInfo.Comment)
                };

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMMigrationRetentionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_LogicalDevice"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                I18NEntity.GetString("RM_JS_JMD_Grid_MoveDataTo"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertJMMigrationRetentionDataToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverMigrationRententionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverMigrationRententionJobDetails;
                datas[rowCount] = new string[]
                {
                    jobDetailInfo.LogicalDevice,
                    jobDetailInfo.Action,
                    jobDetailInfo.MoveDataTo,
                    jobDetailInfo.SizeStr,
                    jobDetailInfo.Date,
                    ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString(),
                    I18NEntity.GetString(jobDetailInfo.Comment)
                };

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMMigrationFileLevelRetentionHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_SourceURL"),
                I18NEntity.GetString("RM_JS_JM_JobID"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_LastModified"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }
        private string[][] ConvertJMMigrationFileLevelRetentionDataToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMArchiverMigrationFileLevelRetentionJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMArchiverMigrationFileLevelRetentionJobDetails;
                datas[rowCount] = new string[]
                {
                    jobDetailInfo.FileName,
                    jobDetailInfo.FilePath,
                    jobDetailInfo.JobId,
                    jobDetailInfo.SizeStr,
                    jobDetailInfo.LastModifiedStr,
                    jobDetailInfo.FinishTimeStr,
                    jobDetailInfo.Action,
                    ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString(),
                    I18NEntity.GetString(jobDetailInfo.Comment)
                };

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMMigrationRestoreHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_Type"),
                I18NEntity.GetString("RM_JS_JMD_Grid_BackupSourceURL"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertJMMigrationRestoreDataToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDisposalJobDetails jobDetailInfo = null;
            foreach (var job in jobDetails)
            {
                jobDetailInfo = job as JMDisposalJobDetails;
                datas[rowCount] = new string[]
                {
                    jobDetailInfo.Type,
                    jobDetailInfo.SourceURL,
                    jobDetailInfo.Size,
                    jobDetailInfo.FinishTime,
                    ConvertJobDetailsStatusToString(jobDetailInfo.Status).ToString(),
                    I18NEntity.GetString(jobDetailInfo.Comment)
                };

                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleJMExportBulkUpdatePhysicalRecordsJobHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[7];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][1] = I18NEntity.GetString("RM_PRM_TM_TemplateName_Title");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_UniqueID");
            datas[0][3] = I18NEntity.GetString("RM_PRM_PRE_Column_Barcode");
            datas[0][4] = I18NEntity.GetString("RM_PRM_PRE_Column_RecordTitle");
            datas[0][5] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][6] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }
        private string[][] AssembleRestoreReportTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Comment");
            return datas;
        }
        private string[][] ConvertRestoreReportToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMRestoreReportJobDetailes jobDetailInfo = null;
            foreach (JMRestoreReportJobDetailes jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMRestoreReportJobDetailes;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.Level;
                datas[rowCount][1] = jobDetailInfo.Title;
                datas[rowCount][2] = jobDetailInfo.Url;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = jobDetailInfo.Comment;
                rowCount++;
            }
            return datas;
        }
        private string[][] ConvertExportBulkUpdatePhysicalRecordsJobToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMImportPhysicalRecordsJobDetail jobDetailInfo = null;
            foreach (JMImportPhysicalRecordsJobDetail jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMImportPhysicalRecordsJobDetail;
                datas[rowCount] = new string[7];
                datas[rowCount][0] = jobDetailInfo.DestRecordType;
                datas[rowCount][1] = jobDetailInfo.TemplateName;
                datas[rowCount][2] = jobDetailInfo.UniqueId;
                datas[rowCount][3] = jobDetailInfo.Barcode;
                datas[rowCount][4] = jobDetailInfo.Title;
                datas[rowCount][5] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][6] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
        private string[][] AssembleJMExportSiteMetricsHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_JMD_Grid_ObjectName");
            datas[0][1] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[0][2] = I18NEntity.GetString("RM_JS_JMD_Grid_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_JMD_Grid_Status");
            datas[0][4] = I18NEntity.GetString("RM_JS_JMD_Grid_Comment");
            return datas;
        }

        private string[][] ConvertJMExportSiteMetricsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMGlobalSearchActionJobDetails jobDetailInfo = null;
            foreach (JMGlobalSearchActionJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMGlobalSearchActionJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.ObjectName;
                datas[rowCount][1] = jobDetailInfo.Type;
                datas[rowCount][2] = jobDetailInfo.FullPath;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleConvertStubHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }
        
        private string[][] ConvertConvertStubJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMConvertStubJobDetails jobDetailInfo = null;
            foreach (JMConvertStubJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMConvertStubJobDetails;
                datas[rowCount] = new string[5];
                datas[rowCount][0] = jobDetailInfo.FullPath;
                datas[rowCount][1] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][2] = jobDetailInfo.ActionStr;
                datas[rowCount][3] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][4] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleDeclaredRecordsMigrationHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertDeclaredRecordsMigrationJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDeclaredRecordsMigrationJobDetails jobDetailInfo = null;
            foreach (JMDeclaredRecordsMigrationJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDeclaredRecordsMigrationJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleStubDisposalHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = new string[] {
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JMD_Grid_FinishTime"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            };
            return datas;
        }

        private string[][] ConvertStubDisposalJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMStubDisposalJobDetails jobDetailInfo = null;
            foreach (JMStubDisposalJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMStubDisposalJobDetails;
                datas[rowCount] = new string[4];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = jobDetailInfo.FinishTimeStr;
                datas[rowCount][2] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][3] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }

        private string[][] AssembleDeleteArchivedSCHeaderTittle(BaseJobDto baseJobDto, string[][] datas)
        {
            datas[0] = [
                I18NEntity.GetString("RM_JS_JMD_Grid_Url"),
                I18NEntity.GetString("RM_JS_JM_JobID"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Size"),
                I18NEntity.GetString("RM_JS_JMD_Grid_SrcStorageName"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Status"),
                I18NEntity.GetString("RM_JS_JMD_Grid_Comment"),
            ];
            return datas;
        }

        private string[][] DeleteArchivedSCJobDetailsToArray(IEnumerable<JMJobDetails> jobDetails, string[][] datas)
        {
            int rowCount = 1;
            JMDeleteArchivedSCJobDetails jobDetailInfo = null;
            foreach (JMDeleteArchivedSCJobDetails jobDetail in jobDetails)
            {
                jobDetailInfo = jobDetail as JMDeleteArchivedSCJobDetails;
                datas[rowCount] = new string[6];
                datas[rowCount][0] = jobDetailInfo.Url;
                datas[rowCount][1] = jobDetailInfo.JobId;
                datas[rowCount][2] = jobDetailInfo.SizeStr;
                datas[rowCount][3] = jobDetailInfo.SourceStorageName;
                datas[rowCount][4] = ConvertJobDetailsStatusToString(jobDetailInfo.Status);
                datas[rowCount][5] = I18NEntity.GetString(jobDetailInfo.Comment);
                rowCount++;
            }
            return datas;
        }
    }
}
