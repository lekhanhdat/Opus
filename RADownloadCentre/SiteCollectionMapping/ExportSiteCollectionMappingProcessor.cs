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
using System.Text;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using RADownloadCenter;

namespace RADownloadCentre.SiteCollectionMapping
{
    public class ExportSiteCollectionMappingProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ExportSiteCollectionMappingProcessor));

        private string jobId;
        private string folderPath;
        private int fileIndex = 0;
        private string fileName;
        private long sheetRowIndex;
        private string[][] datas;
        private static int MAX_ROW_NUMBER_IN_ONE_SHEET = 500000;
        private int workBookSheetIndex = 0;
        private static int MAX_SHEET_NUMBER_IN_ONE_BOOK = 4;
        protected override string BaseJobId => jobId;
        private RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        public ExportSiteCollectionMappingProcessor(string jobId)
        {
            Logger.Info($"Current version 2025-1-14, last commit sha1: d1be2df");
            this.jobId = jobId;
            GenerateAndUploadFileManager.Init(jobId, JobType.ArchiverDeduplicationReport);
            folderPath = SecurityUtils.SafeCombinePath(
                JobReportUtility.GetDownloadsSiteCollectionMappingTempleFolder("Temple"),
                I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionMapping") + "_" + DateTime.UtcNow.Ticks + Guid.NewGuid());
            GenerateFolder(folderPath);
            fileIndex++;
            fileName = I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionMapping") + ".xlsx";
        }

        private void GenerateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        protected override async Task GenerateDataAsync()
        {
            WriteHeadToReportFile();
            List<RMRestoreSiteMapping> mappings = RMRestoreSiteMappingDao.GetAllMappings();
            foreach (RMRestoreSiteMapping mapping in mappings)
            {
                WriteToReportFile(mapping);
                Logger.Info($@"mapping id: {mapping?.Id} , source: {mapping?.SourceSiteUrl}, target: {mapping?.TargetSiteUrl},int id: {mapping?.intId}");
            }
            FlushDataToReportFile();
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, jobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, folderPath + ".zip");
                    Logger.Info($"Upload site mapping export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload site mapping export failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(folderPath + ".zip");
        }


        private void WriteToReportFile(RMRestoreSiteMapping info)
        {
            WriteHeadToReportFile();
            datas[sheetRowIndex++] = ConvertFileInfoToExcelRow(info);
            if (this.sheetRowIndex >= MAX_ROW_NUMBER_IN_ONE_SHEET)
            {
                FlushDataToReportFile();
            }
        }

        private void WriteHeadToReportFile()
        {
            if (datas == null || sheetRowIndex == 0)
            {
                sheetRowIndex = 0;
                datas = new string[MAX_ROW_NUMBER_IN_ONE_SHEET][];
                datas[sheetRowIndex++] = CreateExcelTitle();
            }
        }

        private void FlushDataToReportFile()
        {
            if (sheetRowIndex <= 0)
            {
                return;
            }
            this.sheetRowIndex = 0;
            if (++this.workBookSheetIndex == 1)
            {
                ReportUtil.CreateExcel(folderPath + "/" + fileName, "Sheet", datas.Where(row => row != null).ToArray());
            }
            else
            {
                ReportUtil.InsertWorksheet(folderPath + "/" + fileName, "Sheet" + workBookSheetIndex, datas.Where(row => row != null).ToArray());
            }
            if (this.workBookSheetIndex >= MAX_SHEET_NUMBER_IN_ONE_BOOK)
            {
                ++fileIndex;
                if (fileIndex > 1)
                {
                    fileName = I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionMapping") +  "("+ fileIndex + ")" + ".xlsx";
                }
                else
                {
                    fileName = I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionMapping") + ".xlsx";
                }
                this.workBookSheetIndex = 0;
            }
            datas = null;
        }




        private string[] CreateExcelTitle()
        {
            string[] title = new string[2];
            title[0] = I18NEntity.GetString("RM_AR_RC_TableCol_SourceSite");
            title[1] = I18NEntity.GetString("RM_AR_RC_TableCol_DestinationSite");
            return title;
        }

        public string[] ConvertFileInfoToExcelRow(RMRestoreSiteMapping info)
        {
            string[] data = new string[2];
            data[0] = info.SourceSiteUrl;
            data[1] = info.TargetSiteUrl;
            return data;
        }
    }
}
