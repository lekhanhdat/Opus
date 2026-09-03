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

namespace RADownloadCentre.SiteWhitelist
{
    public class ExportFullTextIndexSiteCollectionlistProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ExportFullTextIndexSiteCollectionlistProcessor));
        private static readonly int _maxRowNumberInOneSheet = 500000;
        private static readonly int _maxSheetNumberInOneBook = 4;

        private readonly string _jobId;
        private readonly string _folderPath;
        private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();
        private readonly IRMRestoreSiteMappingDao _rmRestoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        private int _fileIndex = 0;
        private int _workBookSheetIndex = 0;
        private long _sheetRowIndex;
        private string _fileName;
        private JobType _jobType;
        private string[][]? _datas;

        protected override string BaseJobId => _jobId;
        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        public ExportFullTextIndexSiteCollectionlistProcessor(string jobId, JobType jobType)
        {
            _jobId = jobId;
            _jobType = jobType;
            GenerateAndUploadFileManager.Init(jobId, _jobType);

            var reportName = _jobType == JobType.ExportSCWhitelist
                ? I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionWhitelist")
                : I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionBlacklist");

            var folder = _jobType == JobType.ExportSCWhitelist
                ? JobReportUtility.GetDownloadsSiteCollectionWhitelistTempleFolder("Temple")
                : JobReportUtility.GetDownloadsSiteCollectionBlacklistTempleFolder("Temple");

            _folderPath = SecurityUtils.SafeCombinePath(
                folder,
                $"{reportName}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}"
            );

            GenerateFolder();
            _fileIndex++;
            _fileName = $"{reportName}.xlsx";
        }

        private void GenerateFolder()
        {
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        protected override async Task GenerateDataAsync()
        {
            WriteHeadToReportFile();

            List<RMRestoreSiteMapping> mappings = GetMappingsByJobType();
            foreach (var mapping in mappings)
            {
                WriteToReportFile(mapping);
                _logger.Info($"Mapping ID: {mapping?.Id}, Site Collection URL: {mapping?.SourceSiteUrl}, Int ID: {mapping?.intId}");
            }

            FlushDataToReportFile();
        }

        protected override async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(_folderPath, $"{_folderPath}.zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, $"{_jobId}.zip");
            try
            {
                await _retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, _folderPath + ".zip");
                    _logger.Info("Upload site collection whitelist export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Upload site collection whitelist export failed. Error: {ex}");
                throw;
            }

            _logger.Info($"Finished uploading blob: {blobName}");
            fileInfo = new FileInfo($"{_folderPath}.zip");
        }

        private void WriteToReportFile(RMRestoreSiteMapping info)
        {
            WriteHeadToReportFile();
            if (_datas == null) return;

            _datas[_sheetRowIndex++] = ConvertFileInfoToExcelRow(info);

            if (_sheetRowIndex >= _maxRowNumberInOneSheet)
            {
                FlushDataToReportFile();
            }
        }

        private void WriteHeadToReportFile()
        {
            if (_datas == null || _sheetRowIndex == 0)
            {
                _sheetRowIndex = 0;
                _datas = new string[_maxRowNumberInOneSheet][];
                _datas[_sheetRowIndex++] = CreateExcelTitle();
            }
        }

        private void FlushDataToReportFile()
        {
            if (_sheetRowIndex <= 0)
            {
                return;
            }

            _sheetRowIndex = 0;

            if (++_workBookSheetIndex == 1)
            {
                ReportUtil.CreateExcel(
                    Path.Combine(_folderPath, _fileName),
                    "Sheet",
                    _datas?.Where(row => row != null).ToArray()
                );
            }
            else
            {
                ReportUtil.InsertWorksheet(
                    Path.Combine(_folderPath, _fileName),
                    $"Sheet{_workBookSheetIndex}",
                    _datas?.Where(row => row != null).ToArray()
                );
            }

            if (_workBookSheetIndex >= _maxSheetNumberInOneBook)
            {
                _fileIndex++;
                _fileName = _fileIndex > 1
                    ? $"{I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionWhitelist")}({_fileIndex}).xlsx"
                    : $"{I18NEntity.GetString("RM_AR_Report_ExportSiteCollectionWhitelist")}.xlsx";

                _workBookSheetIndex = 0;
            }

            _datas = null;
        }

        private string[] CreateExcelTitle()
        {
            return new[] { I18NEntity.GetString("RM_TM_SiteCollection") };
        }

        private string[] ConvertFileInfoToExcelRow(RMRestoreSiteMapping info)
        {
            return new[] { info.SourceSiteUrl };
        }

        private List<RMRestoreSiteMapping> GetMappingsByJobType()
        {
            return _jobType switch
            {
                JobType.ExportSCWhitelist => _rmRestoreSiteMappingDao.GetAllWhitelist(),
                JobType.ExportSCBlacklist => _rmRestoreSiteMappingDao.GetAllBlacklist(),
                _ => throw new InvalidOperationException($"Unsupported job type for export: {_jobType}")
            };
        }

    }
}
