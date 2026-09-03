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
using AngleSharp.Browser.Dom;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.PickStatus
{
    public class ExportLoanPickListProcessor : BaseExportPickListProcessor
    {
        public ExportLoanPickListProcessor(JobType jobType, string jobId) : base(jobType, jobId)
        {
            FileNamePrefix = I18NEntity.GetString("RM_JS_Phy_ReturnHistoryExport");
            SheetName = I18NEntity.GetString("RM_MT_PickList_LoanRequests");
        }

        protected override string GetTempFolder()
        {
            return JobReportUtility.GetDownloadLoanPickListReportTempleFolder("Temple");
        }

        protected override async Task ProcessRecordsAsync(BaseRecordDto rec)
        {
            CurrentIndex++;
            CreateEmptyFile = false;
            sheetList.Add(rec);
            if (CurrentIndex > CountOfOneSheet)
            {
                ExportToExcel();
                sheetList = new List<BaseRecordDto>();
                CurrentIndex = 0;
                SheetIndex++;
            }
        }

        protected override ExplorerQueryV3Dto GetQueryDto(CompleteActionParam jobParam, string pageIndex)
        {
            return PickListService.GetLoanQueryDto(pageIndex, PageSize, jobParam.SearchText, jobParam.FilterOptions);
        }

        protected override async Task AfterProcessAsync()
        {
            if (sheetList.Count > 0 || CreateEmptyFile)
            {
                ExportToExcel();
            }
            try
            {
                UploadBlobAsync(FolderPath, JobId.Split('_')[0]).GetAwaiter().GetResult();
                var fileInfo = new FileInfo(FolderPath + ".zip");
                DownCenterInfo.FileSize = fileInfo.Length;
                DownCenterInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                DownCenterInfo.JobStatus = (int)DownloadContentJobStatus.Finished;
                var success = await DownloadDataInfoDao.UpdateAsync(DownCenterInfo);
                if (success)
                {
                    logger.Info("Update download file finished.");
                }
                else
                {
                    logger.Info("Update download file failed, retry update.");
                    success = DownloadDataInfoDao.ApplyCurrentValues(DownCenterInfo);
                    var status = success ? "finished" : "failed";
                    logger.Info($"Update retry download file {status}.");
                }
            }
            catch (Exception e)
            {
                DownCenterInfo.JobStatus = (int)DownloadContentJobStatus.Failed;
                await DownloadDataInfoDao.UpdateAsync(DownCenterInfo);
                logger.Error($"Run export pick list data job failed, error : {e}");
            }
        }
        
        private void ExportToExcel()
        {
            var data = new string[sheetList.Count + 1][];
            if (!CreateEmptyFile)
            {
                AssembleHeaderTittle(data);
                ConvertDataToArray(sheetList, data);
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
                ReportUtil.CreateExcel(FullPath, SheetName, data);
                logger.Info($"Create Excel success, sheet list index is:{SheetIndex}");
            }
            else
            {
                ReportUtil.InsertWorksheet(FullPath, SheetName + SheetIndex, data);
            }
        }

        private static string[][] AssembleHeaderTittle(string[][] data)
        {
            var rowIndex = 0;
            var colIndex = 0;
            data[rowIndex] = new string[5];
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_MyRequest_ItemName");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_RequestManagement_UniqueId");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_RequestLoanBy");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_Template_Column_Name_HomeLocation");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_Status");
            return data;
        }

        private string[][] ConvertDataToArray(List<BaseRecordDto> records, string[][] data)
        {
            var rowIndex = 1;
            //var gls = GeneralSettingService.GetGeneralSettingAsync();
            foreach (var record in records)
            {
                try
                {
                    var colIndex = 0;
                    data[rowIndex] = new string[5];
                    data[rowIndex][colIndex++] = record.LeafName;
                    data[rowIndex][colIndex++] = record.RecordsId;
                    data[rowIndex][colIndex++] = record.PersonHoldBy;
                    data[rowIndex][colIndex++] = GetPhysicalObjectFullPath(record);
                    data[rowIndex][colIndex++] = record.LoanPickStatus == (int)PickStatusType.Pendding ? I18NEntity.GetString("RM_MT_PickList_Status_PendingLoan") : I18NEntity.GetString("RM_MT_PickList_Status_Loaned");
                    rowIndex++;
                }
                catch (Exception ex)
                {
                    logger.Error($"Convert data to cell failed, item id {record.Id}, error: {ex}");
                }
            }
            return data;
        }
    }
}
