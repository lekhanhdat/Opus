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
using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using RADownloadCenter;
using System.Text;

namespace RADownloadCentre.ReturnLoanHistoryExport
{
    public class ReturnLoanHistoryExportProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ReturnLoanHistoryExportProcessor));

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static IRecordReturnLoanDataHistoryTableDao RecordReturnLoanDataHistoryTableDao => PlatformWindsorManager.GetService<IRecordReturnLoanDataHistoryTableDao>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly BaseJobDto BaseJobDto;

        private readonly int PageSize = 1000;

        private readonly string FolderPath;

        private string FilePath;

        private readonly string JobId;

        private readonly int CountOfOneSheet = 200000;

        public string mTimeZone;

        public TimeSpan mTimeZoneOffset = new TimeSpan();

        public ReturnLoanHistoryExportProcessor(string subJobId, string jobId)
        {
            BaseJobDto = new BaseJobDto()
            {
                Id = jobId,
                JobType = (int)JobType.PhysicalReturnHistoryExport
            };
            JobId = jobId;
            GenerateAndUploadFileManager.Init(jobId, JobType.PhysicalReturnHistoryExport);
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto);
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("RLHE", I18NEntity.GetString("RM_JS_Phy_ReturnHistoryExport") + "_"), ".csv");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        protected override string BaseJobId => JobId;

        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        protected override async Task GenerateDataAsync()
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var pageIndex = 0;
            var currentCount = 0;
            var isCreateHeader = true;
            var sheetIndex = 0;
            var (returnHistoryDatas, totalCount) = await RecordReturnLoanDataHistoryTableDao.GetRecordReturnLoanDataHistoryPagination(TenantLocalValue.LogonGroupId, pageIndex, PageSize);
            do
            {
                try
                {
                    currentCount += returnHistoryDatas.Count();
                    var datas = new string[returnHistoryDatas.Count() + 1][];
                    pageIndex++;
                    if (isCreateHeader)
                    {
                        currentCount += 1;
                        datas = await GenerateReturnHistoryDataAsync(datas, returnHistoryDatas, true, gls);
                        ReportUtil.ExportDataToCsv(datas, FilePath);
                        isCreateHeader = false;
                        Logger.Info($"Create Excel with header success,current count is {currentCount}");
                        continue;
                    }

                    if (currentCount >= CountOfOneSheet)
                    {
                        sheetIndex++;
                        datas = await GenerateReturnHistoryDataAsync(datas, returnHistoryDatas, true, gls);
                        FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, $"_{sheetIndex}" + ".csv");
                        ReportUtil.ExportDataToCsv(datas, FilePath);
                        currentCount = returnHistoryDatas.Count();
                        Logger.Info($"Insert Excel with header success,current count is {currentCount},current sheet index is {sheetIndex}");
                        continue;
                    }

                    datas = await GenerateReturnHistoryDataAsync(datas, returnHistoryDatas, false, gls);
                    ReportUtil.ExportDataToCsv(datas, FilePath);
                    Logger.Info($"Insert data to sheet success,current count is {currentCount},current sheet index is {sheetIndex}");

                }
                catch (Exception e)
                {
                    Logger.Error($"Generate report detail to Excel error,current count is {currentCount},currrent sheet index is {sheetIndex},error : {e}");
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw;
                }

            } while ((returnHistoryDatas = (await RecordReturnLoanDataHistoryTableDao.GetRecordReturnLoanDataHistoryPagination(TenantLocalValue.LogonGroupId, pageIndex, PageSize)).Item1).Any());
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload return history details success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload return history details failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        public async Task<string[][]> GenerateReturnHistoryDataAsync(string[][] datas, IEnumerable<RecordReturnLoanDataHistory> returnHistoryDatas, bool isCreateHeader, GeneralSettingModel gls)
        {
            try
            {
                if (isCreateHeader)
                {
                    datas = AssembleReturnHistoryHeaderTittle(datas);
                }
                return await ConvertReturnHistoryToArrayAsync(returnHistoryDatas, datas, gls);
            }
            catch (Exception e)
            {
                Logger.Error($"Generate report for export job failed {e}");
                throw;
            }
        }

        private async Task<string[][]> ConvertReturnHistoryToArrayAsync(IEnumerable<RecordReturnLoanDataHistory> returnHistoryDatas, string[][] datas, GeneralSettingModel gls)
        {
            RecordReturnLoanDataHistory historyInfo = null;
            int rowCount = 1;
            if(datas.Length < 1)
            {
                Logger.Error("The datas array is empty, cannot convert return history to array.");
                return datas;
            }
            if (datas[0] == null) rowCount = 0;
            foreach (RecordReturnLoanDataHistory history in returnHistoryDatas)
            {
                try
                {
                    historyInfo = history;
                    datas[rowCount] = new string[5];
                    datas[rowCount][0] = historyInfo.ItemName;
                    datas[rowCount][1] = historyInfo.UniqueId;
                    datas[rowCount][2] = historyInfo.RequestBy;
                    datas[rowCount][3] = historyInfo.HomeLocation;
                    datas[rowCount][4] = GeneralSettingService.ConvertTiksToDateTime(gls, history.ReturnTime, true).SimplifyFormatTime;
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert return history to array failed {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        private string[][] AssembleReturnHistoryHeaderTittle(string[][] datas)
        {
            datas[0] = new string[5];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReturnHistoryColumn_ItemName");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReturnHistoryColumn_UniqueId");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReturnHistoryColumn_RequestedBy");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_ReturnHistoryColumn_HomeLocation");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ReturnHistoryColumn_Returned");
            return datas;
        }
    }
}
