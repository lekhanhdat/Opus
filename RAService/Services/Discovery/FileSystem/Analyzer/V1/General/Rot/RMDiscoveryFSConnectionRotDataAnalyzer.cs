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
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.Model;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.V1.General.Rot
{
    public class RMDiscoveryFSConnectionRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSConnectionRotDataAnalyzer));

        private readonly RMDiscoveryJobType _jobType;

        private readonly int _containerId;

        private readonly int _connectionId;

        private readonly Guid _connectionUniqueId;

        private readonly HashSet<Guid> _ruleIds;

        private readonly Dictionary<Guid, RMDiscoveryFSRuleInfo> _ruleInfoDict;

        private readonly List<RMDiscoveryFSRuleInfo> _rules;

        private readonly RMDiscoveryFSFileExtensionAnalysisManager _fileExtensionManager;

        private readonly List<RMDiscoveryFSConnectionRuleLevelRotData> _ruleLevelRotDataList = [];

        private readonly List<RMDiscoveryFSConnectionCategoryLevelRotData> _categoryLevelRotDataList = [];

        private readonly List<RMDiscoveryFSConnectionRootLevelRotData> _rootLevelRotDataList = [];

        public RMDiscoveryFSConnectionRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            int containerId,
            int connectionId,
            Guid connectionUniqueId,
            List<RMDiscoveryFSRuleInfo> rules,
            RMDiscoveryFSFileExtensionAnalysisManager fileExtensionManager
        )
        {
            _jobType = jobType;
            _containerId = containerId;
            _connectionId = connectionId;
            _connectionUniqueId = connectionUniqueId;
            _rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            _ruleIds = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).Select(item => item.UniqueId).ToHashSet();
            _ruleInfoDict = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToDictionary(item => item.UniqueId, item => item);
            _fileExtensionManager = fileExtensionManager;
        }

        public void Increse(RMDiscoveryFSAnalyzedDataInfo dataInfo)
        {
            RuleLevelIncrese(dataInfo);
            CategoryLevelIncrese(dataInfo);
            RootLevelIncrese(dataInfo);
        }

        private void RuleLevelIncrese(RMDiscoveryFSAnalyzedDataInfo dataInfo)
        {
            foreach (var ruleInfo in _rules)
            {
                if (dataInfo.RuleData.TryGetValue(ruleInfo.UniqueId, out var ruleData))
                {
                    var rotData = new RMDiscoveryFSConnectionRuleLevelRotData
                    {
                        ContainerId = _containerId,
                        ConnectionId = _connectionId,
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

        private void CategoryLevelIncrese(RMDiscoveryFSAnalyzedDataInfo dataInfo)
        {
            foreach (var category in RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING.Keys)
            {
                if (dataInfo.RuleData.TryGetValue(RMDiscoveryOffice365AnalysisConfiguration.ROT_CATEGORY_UNIQUE_ID_MAPPING[category], out var ruleData))
                {
                    var rotData = new RMDiscoveryFSConnectionCategoryLevelRotData
                    {
                        ContainerId = _containerId,
                        ConnectionId = _connectionId,
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

        private void RootLevelIncrese(RMDiscoveryFSAnalyzedDataInfo dataInfo)
        {
            if (dataInfo.RuleData.TryGetValue(RMDiscoveryBuildInRule.ROT_RULE_UNIQUE_ID, out var ruleData))
            {
                var rotData = new RMDiscoveryFSConnectionRootLevelRotData
                {
                    ContainerId = _containerId,
                    ConnectionId = _connectionId,
                    WithoutInDate = dataInfo.DateRangeId,
                    SizeRange = dataInfo.SizeRangeId,
                    FileExtension = _fileExtensionManager.GetIdAndAddOrUpdate(dataInfo.FileExtension),
                    FileTotalSize = ruleData.FileTotalSize,
                    FileSumCount = ruleData.FileSumCount,
                };
                _rootLevelRotDataList.Add(rotData);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryFSConnectionRuleLevelRotData> dataList)> AnalysisRuleLevelAsync()
        {
            _logger.Info($"Successful analysis connection [{_connectionId}] rule level rot data, count [{_ruleLevelRotDataList.Count}].");
            return (true, _ruleLevelRotDataList);
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryFSConnectionCategoryLevelRotData> dataList)> AnalysisCategoryLevelAsync()
        {
            _logger.Info($"Successful analysis connection [{_connectionId}] category level rot data, count [{_categoryLevelRotDataList.Count}].");
            return (true, _categoryLevelRotDataList);
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryFSConnectionRootLevelRotData> dataList)> AnalysisRootLevelAsync()
        {
            _logger.Info($"Successful analysis connection [{_connectionId}] root level rot data, count [{_rootLevelRotDataList.Count}].");
            return (true, _rootLevelRotDataList);
        }
    }
}
