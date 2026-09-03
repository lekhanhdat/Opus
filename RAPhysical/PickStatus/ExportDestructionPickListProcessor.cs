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
using AvePoint.RA.Contract.Object;
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
    public class ExportDestructionPickListProcessor : BaseExportPickListProcessor
    {

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private List<RMAccount> Accounts;

        public ExportDestructionPickListProcessor(JobType jobType, string jobId) : base(jobType, jobId)
        {
            FileNamePrefix = I18NEntity.GetString("RM_JS_Phy_DestructionPickExport");
            SheetName = I18NEntity.GetString("RM_MT_PickList_Destruction");
        }

        protected override string GetTempFolder()
        {
            return JobReportUtility.GetDownloadDestructionPickListReportTempleFolder("Temple");
        }

        protected override async Task ProcessRecordsAsync(BaseRecordDto rec)
        {
            CurrentIndex++;
            CreateEmptyFile = false;
            sheetList.Add(rec);
            if (CurrentIndex > CountOfOneSheet)
            {
                await ExportToExcelAsync();
                sheetList = new List<BaseRecordDto>();
                CurrentIndex = 0;
                SheetIndex++;
            }
        }

        protected override ExplorerQueryV3Dto GetQueryDto(CompleteActionParam jobParam, string pageIndex)
        {
            return PickListService.GetDestructionQueryDto(pageIndex, PageSize, jobParam.SearchText, jobParam.FilterOptions);
        }

        protected override async Task AfterProcessAsync()
        {
            if (sheetList.Count > 0 || CreateEmptyFile)
            {
                await ExportToExcelAsync();
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

        private Task<List<RMAccount>> GetAccountAsync(List<int> userIds)
        {
            return AccountDao.GetUserByIdsAsync(userIds);
        }

        private async Task ExportToExcelAsync()
        {
            List<List<int>> listGroup = new List<List<int>>();
            int j = QueryDBGroupCount;
            for (int i = 0; i < sheetList.Count; i += QueryDBGroupCount)
            {
                List<int> cList = new List<int>();
                cList = sheetList.Take(j).Skip(i).Select(r => r.ManualApprovedBy).ToList();
                j += QueryDBGroupCount;
                listGroup.Add(cList);
            }
            Accounts = new List<RMAccount>();
            foreach (var cList in listGroup)
            {
                List<int> userIdList = cList;
                if (userIdList.Count > 0)
                {
                    Accounts = await this.GetAccountAsync(userIdList);
                }
            }
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
            data[rowIndex] = new string[7];
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_MyRequest_ItemName");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_RequestManagement_UniqueId");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_PRM_PRE_Column_DisposalClass");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_DateDestroyed");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_Template_Column_Name_HomeLocation");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_ApproveBy");
            data[rowIndex][colIndex++] = I18NEntity.GetString("RM_MT_PickList_Column_Status");
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
                    var manualApprovedBy = Accounts.FirstOrDefault(a => a.Id == record.ManualApprovedBy);
                    var colIndex = 0;
                    data[rowIndex] = new string[7];
                    data[rowIndex][colIndex++] = record.LeafName;
                    data[rowIndex][colIndex++] = record.RecordsId;
                    data[rowIndex][colIndex++] = record.TermName;
                    data[rowIndex][colIndex++] = record.DestryoedTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, record.DestryoedTime, true).SimplifyFormatTime;
                    data[rowIndex][colIndex++] = GetPhysicalObjectFullPath(record);
                    data[rowIndex][colIndex++] = manualApprovedBy?.DisplayName;
                    data[rowIndex][colIndex++] = record.DestructionPickStatus == (int)PickStatusType.Pendding ? I18NEntity.GetString("RM_MT_PickList_Status_PendingDestroy") : I18NEntity.GetString("RM_MT_PickList_Status_Destroyed");
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
