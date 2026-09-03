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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Newtonsoft.Json;
using RAManualApproval.ExportAction;
using System.IO;
using System.Text;
using System.ComponentModel;
using AvePoint.RA.RACommonUtility.Common;

namespace RAArtificialIntelligence.MachineLearningReport
{
    public class MLReportExportProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(MLReportExportProcessor));

        #region interface
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IExplorerDao explorerDao = new ExplorerDao();
        #endregion

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        private const int PageSize = 100;
        private static readonly int CountOfOneSheet = 63375;

        private string JobId;
        private IRMReportManager ReportManager;
        private MLTrainingReportExportParam ExportParam;
        private string JobComment = string.Empty;
        private bool HasFailedDetail;

        private int CurrentIndex = 0;
        private int SheetIndex = 0;

        private string FolderPath = string.Empty;
        private string FullPath = string.Empty;
        private bool CreateEmptyFile = true;
        public MLReportExportProcessor(string subJobId, string jobId)
        {
            var jobType = JobType.MachineLearningExportReportJob;
            ManualApprovalExportJobManager.Init(jobId, jobType);
            JobId = jobId;

            RMSubJob subJob = SubJobDao.GetSubJob(subJobId, true);
            ExportParam = SerializerHelper.DeserializeByJsonSerializer<MLTrainingReportExportParam>(subJob.JobContext.Content);
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            ReportManager.StartUpdateJobProgress(60);
        }

        public async Task RunAsync()
        {
            var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).First();
            try
            {
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
                var fileName = I18NEntity.GetString("RM_MachineLearning_ReprotExportFileName") + "_" + nowDateTimeStr;
                var tempFolder = JobReportUtility.GetMLTempleFolder("Temple");
                FolderPath = tempFolder + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
                FullPath = SecurityUtils.SafeCombinePath(FolderPath, fileName + ".xlsx");

                List<BaseRecordDto> sheetList = new List<BaseRecordDto>();
                Tuple<List<BaseRecordDto>, ExplorerPagingInfo> result = new Tuple<List<BaseRecordDto>, ExplorerPagingInfo>([], new ExplorerPagingInfo() { });
                do
                {
                    result = await GetDataByPagerAsync(result.Item2.PageIndex);
                    foreach (var rec in result.Item1)
                    {
                        if (rec != null)
                        {
                            CurrentIndex++;
                            CreateEmptyFile = false;
                            sheetList.Add(rec);
                            if (CurrentIndex > CountOfOneSheet)
                            {
                                await ExportToExcelAsync(sheetList);
                                sheetList = [];
                                CurrentIndex = 0;
                                SheetIndex++;
                            }
                        }
                    }
                } while (result.Item2.HasNextPage);

                if (sheetList.Count > 0 || CreateEmptyFile)
                {
                    await ExportToExcelAsync(sheetList);
                }
                await UploadBlobAsync(FolderPath, JobId);
                var fileInfo = new FileInfo(FolderPath + ".zip");
                downloadDataInfo.FileSize = fileInfo.Length;
                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);
            }
            catch (Exception e)
            {
                logger.Error($"Run export job error:{e}");
                HasFailedDetail = true;
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {
                var jobFinishStatus = HasFailedDetail ? JobStatus.Failed : JobStatus.Finished;
                ReportManager.SetJobFinished(jobFinishStatus, JobComment);
            }
        }


        private async Task ExportToExcelAsync(List<BaseRecordDto> sheetList)
        {
            var sheetName = I18NEntity.GetString("RM_MachineLearning_ReprotExportSheetName");
            var data = new string[sheetList.Count + 1][];
            if (!CreateEmptyFile)
            {
                AssembleHeaderTittle(data);
                await ConvertDataToArrayAsync(sheetList, data);
            }
            else
            {
                data = new string[1][];
                data[0] = new string[] { I18NEntity.GetString("RM_Common_NoReport") };
            }
            if (SheetIndex == 0)
            {
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
                ReportUtil.CreateExcel(FullPath, sheetName, data);
                logger.Info($"Create Excel success, sheet list index is:{SheetIndex}");
            }
            else
            {
                ReportUtil.InsertWorksheet(FullPath, sheetName + SheetIndex, data);
            }
        }

        private static string[][] AssembleHeaderTittle(string[][] data)
        {
            var rowIndex = 0;
            var colIndex = 0;
            data[rowIndex] = new string[7];
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_ML_TS_Column_DocumentName");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MachineLearning_ReprotIntelligentClassification");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MachineLearning_ReprotCurrentClassification");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_UniqueID");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_JMD_Grid_ApprovalStatus");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MachineLearning_ReprotPredictTime");
            return data;
        }

        private async Task<string[][]> ConvertDataToArrayAsync(List<BaseRecordDto> records, string[][] data)
        {
            var rowIndex = 1;
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var record in records)
            {
                try
                {
                    var colIndex = 0;
                    data[rowIndex] = new string[7];
                    data[rowIndex][colIndex++] = record.LeafName;
                    data[rowIndex][colIndex++] = record.PredictTermName;
                    data[rowIndex][colIndex++] = record.TermName;
                    data[rowIndex][colIndex++] = record.RecordsId;
                    data[rowIndex][colIndex++] = (RMMLApprovalStatus)record.MLApprovalStatus switch
                    {
                        RMMLApprovalStatus.None => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_AutoApply"),
                        RMMLApprovalStatus.WaitingApprove => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Waiting"),
                        RMMLApprovalStatus.Approved => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Approved"),
                        RMMLApprovalStatus.Rejected => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Reclassify"),
                        _ => throw new NotImplementedException(),
                    }; ;
                    data[rowIndex][colIndex++] = record.ExtensionForFile;
                    data[rowIndex][colIndex++] = record.PredictTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, record.PredictTime, true).SimplifyFormatTime;
                    rowIndex++;
                }
                catch (Exception ex)
                {
                    logger.Error($"Convert data to cell failed, item id {record.Id}, error: {ex}");
                }
            }
            return data;
        }

        private async Task<Tuple<List<BaseRecordDto>, ExplorerPagingInfo>> GetDataByPagerAsync(string pageIndex)
        {
            var dto = new ExplorerQueryV3Dto()
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>()
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = pageIndex,
                    PageSize = PageSize,
                }
            };

            dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
            {
                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.PredictTermId },
                Value = JsonConvert.SerializeObject(new List<Guid>()),
            });

            if (ExportParam.TimeRange != TimeRange.All)
            {
                var currentDateTime = DateTime.UtcNow;
                DateTime startTime = DateTime.UtcNow;
                DateTime endTime = DateTime.UtcNow;

                switch (ExportParam.TimeRange)
                {
                    case TimeRange.After3Month:
                        startTime = currentDateTime.AddMonths(-2);
                        break;
                    case TimeRange.After6Month:
                        startTime = currentDateTime.AddMonths(-5);
                        break;
                    case TimeRange.After1Year:
                        startTime = currentDateTime.AddMonths(-11);
                        break;
                    case TimeRange.Custom:
                        var tempStartTime = DateTime.Parse(ExportParam.StartTime);
                        var tempEndTime = DateTime.Parse(ExportParam.EndTime);

                        var gls = await GeneralSettingService.GetGeneralSettingAsync();
                        startTime = DateTimeUtil.ConvertTimeToUtcDate(tempStartTime, gls);
                        endTime = DateTimeUtil.ConvertTimeToUtcDate(tempEndTime, gls);
                        logger.Info($"Export by custom time range:{startTime} - {endTime}");
                        break;
                }

                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3()
                {
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.FromTo,
                        Value1 = startTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        Value2 = endTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        TimeZoneId = "UTC"
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.PredictTime
                    }
                });
            }
            var resultInfo = await ExplorerQueryService.QueryDataListWithoutTotalCustomAsync(dto);
            return new Tuple<List<BaseRecordDto>, ExplorerPagingInfo>(resultInfo.Datas, resultInfo.PagingInfo);
        }

        private async Task UploadBlobAsync(string folderPath, string jobId)
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = Path.Combine(customId, jobId + ".zip");
            var retryFailed = false;
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, folderPath + ".zip");
                    logger.Info($"Upload machine learning export report success");
                    return Task.CompletedTask;
                });
            }
            catch
            {
                retryFailed = true;
                logger.Error($"Upload machine learning export report failed");
            }
            if (retryFailed)
            {
                return;
            }

            logger.Info($"finish to upload blob name:{blobName}");
        }


        private void UpdateDownloadDataInfo(RMDownloadDataInfo DownCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}")) ;
            {
                DownCenterInfo.JobStatus = (int)downloadStatus;
                var success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                if (success)
                {
                    logger.Info($"Update download file status to {downloadStatus} finished.");
                }
                else
                {
                    logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                    success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                    var status = success ? "finished" : "failed";
                    logger.Info($"Update retry download file {status}.");
                }
            }
        }
    }
}
