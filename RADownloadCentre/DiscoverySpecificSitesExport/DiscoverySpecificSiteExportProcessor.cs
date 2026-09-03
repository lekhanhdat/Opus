using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Discovery.Common;
using OpenNLP.Tools.Util;
using RADownloadCenter;
using RADownloadCentre.SiteWhitelist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADownloadCentre.DiscoverySpecificSitesExport
{
    public class DiscoverySpecificSiteExportProcessor : GenerateAndUploadFileExecutor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(DiscoverySpecificSiteExportProcessor));
        private readonly int _maxRowNumberInOneFile = 200000;
        private readonly int _pageSize = 500;

        private readonly string _jobId;
        private readonly string _folderPath;
        private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();
        protected override string BaseJobId => _jobId;
        private string _fileName;
        private JobType _jobType;
        private string[][]? _datas;
        private int _rowIndex;
        private int _fileIndex = 0;

        private readonly IRMDiscoverySpecificSiteService _discoverySpecificSiteService = new RMDiscoverySpecificSiteService();


        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        public DiscoverySpecificSiteExportProcessor(string jobId, JobType jobType)
        {
            _jobId = jobId;
            _jobType = jobType;
            GenerateAndUploadFileManager.Init(jobId, _jobType);

            var reportName = _jobType switch
            {
                JobType.DiscoveryExportExcludeSCList => I18NEntity.GetString("RM_DA_Report_ExportDiscoverySCExcludeList"),
                _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
            };

            var folder = _jobType switch
            {
                JobType.DiscoveryExportExcludeSCList => JobReportUtility.GetDownloadsDiscoveryExcludeSCTempleFolder("Temple"),
                _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
            };

            _folderPath = SecurityUtils.SafeCombinePath(
                folder,
                $"{reportName}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}"
            );

            GenerateFolder();
            _fileIndex++;
            _fileName = $"{reportName}.csv";
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
            DiscoverySpecificSiteInfo sites;
            int pageIndex = 0;
            do
            {
                sites = await GetDatasByJobType(pageIndex++);
                if(sites.SiteCollections == null || sites.SiteCollections.Count() < 1)
                {
                    _logger.Info("No more data to export, finish exporting.");
                    break;
                }
                foreach (var site in sites.SiteCollections)
                {
                    WriteToReportFile(site);
                    _logger.Info($"Site ID: {site?.Id}, Site Collection URL: {site?.SiteCollectionUrl}");
                }
            } while(true);

            FlushDataToReportFile();
        }

        private void WriteToReportFile(DiscoverySpecificSiteDto site)
        {
            WriteHeadToReportFile();
            if (_datas == null) return;

            _datas[_rowIndex++] = ConvertFileInfoToCSVRow(site);

            if (_rowIndex >= _maxRowNumberInOneFile)
            {
                FlushDataToReportFile();
            }
        }

        private string[] ConvertFileInfoToCSVRow(DiscoverySpecificSiteDto site)
        {
            return new[] { site.SiteCollectionUrl };
        }

        private async Task<DiscoverySpecificSiteInfo> GetDatasByJobType(int pageIndex)
        {
            return _jobType switch
            {
                JobType.DiscoveryExportExcludeSCList => await _discoverySpecificSiteService.LoadM365ExclusionListSitesByPaginationAsync(pageIndex, _pageSize),
                _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
            };
        }

        private void FlushDataToReportFile()
        {
            if (_rowIndex <= 0) return;

            _rowIndex = 0;

            ExportToCsv(_datas?.Where(row => row != null).ToArray() ?? [], SecurityUtils.SafeCombinePath(_folderPath, _fileName));
        }

        private void ExportToCsv(string[][] datas, string csvFilePath)
        {
            if(datas == null || datas.Length == 0)
            {
                _logger.Info("No data to export.");
                return;
            }
            var csvContent = new StringBuilder();

            foreach (var row in datas)
            {
                if (row != null)
                {
                    var rowContent = StringUtils.ToCSVString(row);
                    csvContent.AppendLine(rowContent);
                }
            }
            File.WriteAllText(csvFilePath, csvContent.ToString(), Encoding.UTF8);

            _fileName = $"{_jobType switch
            {
                JobType.DiscoveryExportExcludeSCList => I18NEntity.GetString("RM_DA_Report_ExportDiscoverySCExcludeList"),
                _ => throw new NotSupportedException($"Job type {_jobType} is not supported.")
            }}({_fileIndex++}).csv";
        }

        private void WriteHeadToReportFile()
        {
            if(_datas == null || _rowIndex == 0)
            {
                _datas = new string[_maxRowNumberInOneFile][];
                _datas[_rowIndex++] = CreateHeaderTitle();
            }
        }

        private string[] CreateHeaderTitle()
        {
            return new[] { I18NEntity.GetString("RM_TM_SiteCollection") };
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
                    _logger.Info($"Upload site collection {_jobType} export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Upload site collection {_jobType} export failed. Error: {ex}");
                throw;
            }

            _logger.Info($"Finished uploading blob: {blobName}");
            fileInfo = new FileInfo($"{_folderPath}.zip");
        }
    }
}
