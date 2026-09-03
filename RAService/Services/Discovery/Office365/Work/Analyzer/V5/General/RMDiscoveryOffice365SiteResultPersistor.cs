using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Model;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Rot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General
{
    internal sealed class RMDiscoveryOffice365SiteResultPersistor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteResultPersistor));
        private readonly IRMDiscoveryOffice365DataV3Dao _dataV3Dao = new RMDiscoveryOffice365DataV3Dao();
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();
        private readonly Guid _tenantId;
        private readonly SourceFlag _contentSource;
        private readonly RMDiscoveryJobType _jobType;
        private readonly int _containerId;
        private readonly List<RMDiscoveryOffice365RuleInfo> _inactiveRules;
        private readonly RMDiscoveryOffice365AggregateTotalDataAnalyzer _aggregateTotalDataAnalyzer;

        public RMDiscoveryOffice365SiteResultPersistor(
            Guid tenantId,
            SourceFlag contentSource,
            RMDiscoveryJobType jobType,
            int containerId,
            List<RMDiscoveryOffice365RuleInfo> inactiveRules,
            RMDiscoveryOffice365AggregateTotalDataAnalyzer aggregateTotalDataAnalyzer)
        {
            _tenantId = tenantId;
            _contentSource = contentSource;
            _jobType = jobType;
            _containerId = containerId;
            _inactiveRules = inactiveRules;
            _aggregateTotalDataAnalyzer = aggregateTotalDataAnalyzer;
        }

        public async Task<bool> PersistAsync(RMDiscoveryOffice365SiteAnalysisResult result)
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RebuildAndPersistAsync();
                }

                await SaveContainerAsync(result.SiteInfo);
                await SaveInactiveAsync(result.InactiveDataList);
                await SaveRotAsync(result.RuleLevelRotDataList, result.CategoryLevelRotDataList, result.RootLevelRotDataList);
                return await _aggregateTotalDataAnalyzer.SaveDeltaAsync(result.AggregateInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while persisting tenant [{_tenantId}] container [{_containerId}] site result. Error: {e}");
                return false;
            }
        }

        private async Task SaveContainerAsync(RMDiscoveryOffice365SiteInfo siteInfo)
        {
            var containerInfo = (await _nodeDao.GetDiscoveryContainerInfoesAsync(_tenantId, [_containerId])).FirstOrDefault();
            if (containerInfo == null)
            {
                return;
            }

            containerInfo.FileTotalSize += siteInfo.FileTotalSize;
            containerInfo.FileSumCount += siteInfo.FileSumCount;
            containerInfo.VersionTotalSize += siteInfo.VersionTotalSize;
            containerInfo.PHLTotalSize += siteInfo.PHLTotalSize;
            containerInfo.MaxFileAge = Math.Max(containerInfo.MaxFileAge, siteInfo.MaxFileAge);
            containerInfo.SiteCount++;
            containerInfo.ModifiedTime = DateTime.UtcNow.Ticks;
            await _nodeDao.AddOrUpdateDiscoveryContainerAsync(_tenantId, containerInfo);
        }

        private async Task SaveInactiveAsync(List<RMDiscoveryOffice365SiteInactiveData> siteInactiveDataList)
        {
            foreach (var siteData in siteInactiveDataList)
            {
                var containerData = new RMDiscoveryOffice365ContainerInactiveData
                {
                    ContainerId = siteData.ContainerId,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                    CustomColumns = CloneCustomColumns(siteData.CustomColumns),
                };
                await _dataV3Dao.UpsertContainerInactiveDataAsync(_tenantId, containerData);

                var basicData = new RMDiscoveryOffice365BasicInactiveData
                {
                    ContentSource = _contentSource,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                    CustomColumns = CloneCustomColumns(siteData.CustomColumns),
                };
                await _dataV3Dao.UpsertBasicInactiveDataAsync(_tenantId, basicData);
            }
        }

        private async Task SaveRotAsync(
            List<RMDiscoveryOffice365SiteRuleLevelRotData> ruleLevelDataList,
            List<RMDiscoveryOffice365SiteCategoryLevelRotData> categoryLevelDataList,
            List<RMDiscoveryOffice365SiteRootLevelRotData> rootLevelDataList)
        {
            foreach (var siteData in ruleLevelDataList)
            {
                await _dataV3Dao.UpsertContainerRuleLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365ContainerRuleLevelRotData
                {
                    ContainerId = siteData.ContainerId,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    Rule = siteData.Rule,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });

                await _dataV3Dao.UpsertBasicRuleLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365BasicRuleLevelRotData
                {
                    ContentSource = _contentSource,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    Rule = siteData.Rule,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });
            }

            foreach (var siteData in categoryLevelDataList)
            {
                await _dataV3Dao.UpsertContainerCategoryLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365ContainerCategoryLevelRotData
                {
                    ContainerId = siteData.ContainerId,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    Category = siteData.Category,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });

                await _dataV3Dao.UpsertBasicCategoryLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365BasicCategoryLevelRotData
                {
                    ContentSource = _contentSource,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    Category = siteData.Category,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });
            }

            foreach (var siteData in rootLevelDataList)
            {
                await _dataV3Dao.UpsertContainerRootLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365ContainerRootLevelRotData
                {
                    ContainerId = siteData.ContainerId,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });

                await _dataV3Dao.UpsertBasicRootLevelRotDataAsync(_tenantId, new RMDiscoveryOffice365BasicRootLevelRotData
                {
                    ContentSource = _contentSource,
                    WithoutInDate = siteData.WithoutInDate,
                    FileExtension = siteData.FileExtension,
                    SizeRange = siteData.SizeRange,
                    FileTotalSize = siteData.FileTotalSize,
                    FileSumCount = siteData.FileSumCount,
                });
            }
        }

        private async Task<bool> RebuildAndPersistAsync()
        {
            var containerInfo = (await _nodeDao.GetDiscoveryContainerInfoesAsync(_tenantId, [_containerId])).FirstOrDefault();
            if (containerInfo == null)
            {
                return false;
            }

            var inactiveContainerAnalyzer = new RMDiscoveryOffice365ContainerInactiveDataAnalyzer(_jobType, _tenantId, _containerId, _inactiveRules);
            var inactiveBasicAnalyzer = new RMDiscoveryOffice365BasicInactiveDataAnalyzer(_jobType, _tenantId, _contentSource, _inactiveRules);
            var rotContainerAnalyzer = new RMDiscoveryOffice365ContainerRotDataAnalyzer(_jobType, _tenantId, _containerId);
            var rotBasicAnalyzer = new RMDiscoveryOffice365BasicRotDataAnalyzer(_jobType, _tenantId, _contentSource);

            return await inactiveContainerAnalyzer.RefreshAndSaveAsync()
                && await inactiveBasicAnalyzer.RefreshAndSaveAsync()
                && await rotContainerAnalyzer.RefreshAndSaveAsync()
                && await rotBasicAnalyzer.RefreshAndSaveAsync()
                && await _aggregateTotalDataAnalyzer.RefreshAndSaveAsync();
        }

        private static List<RMDiscoveryCustomColumnWithValue> CloneCustomColumns(List<RMDiscoveryCustomColumnWithValue> customColumns)
        {
            return customColumns.ConvertAll(item => new RMDiscoveryCustomColumnWithValue(item.Name, item.Value, item.ValueType));
        }

    }
}
