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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Model;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Rot
{
    public class RMDiscoveryOffice365SiteRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365SiteRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly int _containerId;

        private readonly int _siteId;

        private readonly Guid _siteUniqueId;

        private readonly HashSet<Guid> _ruleIds;

        private readonly Dictionary<Guid, RMDiscoveryOffice365RuleInfo> _ruleInfoDict;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly RMDiscoveryOffice365FileExtensionAnalysisManager _fileExtensionManager;

        private readonly List<RMDiscoveryOffice365SiteRuleLevelRotData> _ruleLevelRotDataList = [];

        private readonly List<RMDiscoveryOffice365SiteCategoryLevelRotData> _categoryLevelRotDataList = [];

        private readonly List<RMDiscoveryOffice365SiteRootLevelRotData> _rootLevelRotDataList = [];

        public RMDiscoveryOffice365SiteRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource,
            int containerId,
            int siteId,
            Guid siteUniqueId,
            List<RMDiscoveryOffice365RuleInfo> rules,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionManager
        )
        {
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
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

        public void Increse(RMDiscoveryOffice365AnalyzedDataInfo dataInfo)
        {
            RuleLevelIncrese(dataInfo);
            CategoryLevelIncrese(dataInfo);
            RootLevelIncrese(dataInfo);
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteRuleLevelRotData> dataList)> AnalysisRuleLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteRuleLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] rule Level rot data.");
                }

                await _dataDao.AddSiteRuleLevelRotDataListAsync(_o365TenantId, _ruleLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data, count [{_ruleLevelRotDataList.Count}].");

                return (true, _ruleLevelRotDataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] rule level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteCategoryLevelRotData> dataList)> AnalysisCategoryLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteCategoryLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] category Level rot data.");
                }

                await _dataDao.AddSiteCategoryLevelRotDataListAsync(_o365TenantId, _categoryLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data, count [{_categoryLevelRotDataList.Count}].");

                return (true, _categoryLevelRotDataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] category level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryOffice365SiteRootLevelRotData> dataList)> AnalysisRootLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteSiteRootLevelRotDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] root Level rot data.");
                }

                await _dataDao.AddSiteRootLevelRotDataListAsync(_o365TenantId, _rootLevelRotDataList);

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data, count [{_rootLevelRotDataList.Count}].");

                return (true, _rootLevelRotDataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] root level rot data. Error: {e}");
                return (false, []);
            }
        }

        public List<RMDiscoveryOffice365SiteRuleLevelRotData> GetRuleLevelDataList()
        {
            return _ruleLevelRotDataList;
        }

        public List<RMDiscoveryOffice365SiteCategoryLevelRotData> GetCategoryLevelDataList()
        {
            return _categoryLevelRotDataList;
        }

        public List<RMDiscoveryOffice365SiteRootLevelRotData> GetRootLevelDataList()
        {
            return _rootLevelRotDataList;
        }

        private void RuleLevelIncrese(RMDiscoveryOffice365AnalyzedDataInfo dataInfo)
        {
            foreach(var ruleInfo in _rules)
            {
                if(dataInfo.RuleData.TryGetValue(ruleInfo.UniqueId, out var ruleData))
                {
                    var rotData = new RMDiscoveryOffice365SiteRuleLevelRotData
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

        private void CategoryLevelIncrese(RMDiscoveryOffice365AnalyzedDataInfo dataInfo)
        {
            foreach(var category in RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING.Keys)
            {
                if (dataInfo.RuleData.TryGetValue(RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING[category], out var ruleData))
                {
                    var rotData = new RMDiscoveryOffice365SiteCategoryLevelRotData
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

        private void RootLevelIncrese(RMDiscoveryOffice365AnalyzedDataInfo dataInfo)
        {
            if(dataInfo.RuleData.TryGetValue(RMDiscoveryBuildInRule.ROT_RULE_UNIQUE_ID, out var ruleData))
            {
                var rotData = new RMDiscoveryOffice365SiteRootLevelRotData
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
