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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General.ROT
{
    public class RMDiscoveryAOSPSiteRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPSiteRotDataAnalyzer));

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly int _containerId;

        private readonly int _siteId;

        private readonly Guid _siteUniqueId;

        private readonly HashSet<Guid> _ruleIds;

        private readonly Dictionary<Guid, RMDiscoveryAOSPRuleInfo> _ruleInfoDict;

        private readonly List<RMDiscoveryAOSPRuleInfo> _rules;

        private readonly RMDiscoveryAOSPFileExtensionAnalysisManager _fileExtensionManager;

        private readonly List<RMDiscoveryAOSPSiteRuleLevelRotData> _ruleLevelRotDataList = [];

        private readonly List<RMDiscoveryAOSPSiteCategoryLevelRotData> _categoryLevelRotDataList = [];

        private readonly List<RMDiscoveryAOSPSiteRootLevelRotData> _rootLevelRotDataList = [];

        public RMDiscoveryAOSPSiteRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource,
            int containerId,
            int siteId,
            Guid siteUniqueId,
            List<RMDiscoveryAOSPRuleInfo> rules,
            RMDiscoveryAOSPFileExtensionAnalysisManager fileExtensionManager
        )
        {
            _dataDao = new RMDiscoveryAOSPDataDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _containerId = containerId;
            _siteId = siteId;
            _siteUniqueId = siteUniqueId;
            _rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            _ruleIds = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).Select(item => item.UniqueId).ToHashSet();
            _ruleInfoDict = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToDictionary(item => item.UniqueId, item => item);
            _fileExtensionManager = fileExtensionManager;
        }

        public void Increse(RMDiscoveryAOSPAnalyzedDataInfo dataInfo)
        {
            RuleLevelIncrese(dataInfo);
            CategoryLevelIncrese(dataInfo);
            RootLevelIncrese(dataInfo);
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryAOSPSiteRuleLevelRotData> dataList, string errorMessage)> AnalysisRuleLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    await _dataDao.DeleteSiteRuleLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] rule Level rot data.");
                }

                await _dataDao.AddSiteRuleLevelRotDataListAsync(_o365TenantId, _ruleLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data, count [{_ruleLevelRotDataList.Count}].");

                return (true, _ruleLevelRotDataList, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data. Error: {e}");
                return (false, [], e.Message);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryAOSPSiteCategoryLevelRotData> dataList, string errorMessage)> AnalysisCategoryLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    await _dataDao.DeleteSiteCategoryLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] category Level rot data.");
                }

                await _dataDao.AddSiteCategoryLevelRotDataListAsync(_o365TenantId, _categoryLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data, count [{_categoryLevelRotDataList.Count}].");

                return (true, _categoryLevelRotDataList, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data. Error: {e}");
                return (false, [], e.Message);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryAOSPSiteRootLevelRotData> dataList, string errorMessage)> AnalysisRootLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    await _dataDao.DeleteSiteRootLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] root Level rot data.");
                }

                await _dataDao.AddSiteRootLevelRotDataListAsync(_o365TenantId, _rootLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data, count [{_rootLevelRotDataList.Count}].");

                return (true, _rootLevelRotDataList, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data. Error: {e}");
                return (false, [], e.Message);
            }
        }

        private void RuleLevelIncrese(RMDiscoveryAOSPAnalyzedDataInfo dataInfo)
        {
            foreach (var ruleInfo in _rules)
            {
                if (dataInfo.RuleData.TryGetValue(ruleInfo.UniqueId, out var ruleData))
                {
                    var rotData = new RMDiscoveryAOSPSiteRuleLevelRotData
                    {
                        ContainerId = _containerId,
                        SiteId = _siteId,
                        Rule = ruleInfo.Id,
                        WithoutInDate = dataInfo.DateRangeId,
                        SizeRange = dataInfo.SizeRangeId,
                        FileExtension = _fileExtensionManager.GetIdAndAddOrUpdate(dataInfo.FileExtension),
                        FileTotalSize = ruleData.FileTotalSize,
                        FileSumCount = ruleData.FileSumCount,
                    };
                    _ruleLevelRotDataList.Add(rotData);
                }
            }
        }

        private void CategoryLevelIncrese(RMDiscoveryAOSPAnalyzedDataInfo dataInfo)
        {
            foreach (var category in RMDiscoveryAOSPAnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING.Keys)
            {
                if (dataInfo.RuleData.TryGetValue(RMDiscoveryAOSPAnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING[category], out var ruleData))
                {
                    var rotData = new RMDiscoveryAOSPSiteCategoryLevelRotData
                    {
                        ContainerId = _containerId,
                        SiteId = _siteId,
                        Category = category,
                        WithoutInDate = dataInfo.DateRangeId,
                        SizeRange = dataInfo.SizeRangeId,
                        FileExtension = _fileExtensionManager.GetIdAndAddOrUpdate(dataInfo.FileExtension),
                        FileTotalSize = ruleData.FileTotalSize,
                        FileSumCount = ruleData.FileSumCount,
                    };
                    _categoryLevelRotDataList.Add(rotData);
                }
            }
        }

        private void RootLevelIncrese(RMDiscoveryAOSPAnalyzedDataInfo dataInfo)
        {
            if (dataInfo.RuleData.TryGetValue(RMDiscoveryBuildInRule.ROT_RULE_UNIQUE_ID, out var ruleData))
            {
                var rotData = new RMDiscoveryAOSPSiteRootLevelRotData
                {
                    ContainerId = _containerId,
                    SiteId = _siteId,
                    WithoutInDate = dataInfo.DateRangeId,
                    SizeRange = dataInfo.SizeRangeId,
                    FileExtension = _fileExtensionManager.GetIdAndAddOrUpdate(dataInfo.FileExtension),
                    FileTotalSize = ruleData.FileTotalSize,
                    FileSumCount = ruleData.FileSumCount,
                };
                _rootLevelRotDataList.Add(rotData);
            }
        }
    }
}
