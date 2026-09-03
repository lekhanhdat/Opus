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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.V1.General.Inactive
{
    public class RMDiscoveryFSConnectionInactiveDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSConnectionInactiveDataAnalyzer));

        private readonly RMDiscoveryJobType _jobType;

        private readonly int _containerId;

        private readonly int _connectionId;

        private readonly Guid _connectionUniqueId;

        private readonly List<RMDiscoveryFSRuleInfo> _rules;

        private readonly RMDiscoveryFSFileExtensionAnalysisManager _fileExtensionManager;

        private readonly List<RMDiscoveryFSConnectionInactiveData> _inactiveDataList = [];

        public RMDiscoveryFSConnectionInactiveDataAnalyzer(
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
            _rules = rules;
            _fileExtensionManager = fileExtensionManager;
        }

        public void Increse(RMDiscoveryFSAnalyzedDataInfo dataInfo)
        {
            var inactiveData = new RMDiscoveryFSConnectionInactiveData
            {
                ContainerId = _containerId,
                ConnectionId = _connectionId,
                WithoutInDate = dataInfo.DateRangeId,
                SizeRange = dataInfo.SizeRangeId,
                FileExtension = _fileExtensionManager.GetIdAndAddOrUpdate(dataInfo.FileExtension),
                FileTotalSize = dataInfo.AggregationInfo.FileTotalSize,
                FileSumCount = dataInfo.AggregationInfo.FileSumCount,
            };
            foreach (var ruleInfo in _rules)
            {
                if (!dataInfo.RuleData.TryGetValue(ruleInfo.UniqueId, out var ruleData))
                {
                    ruleData = new();
                }

                inactiveData.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                    ruleInfo.ToCustomColumn().Name,
                    ruleData.FileTotalSize,
                    typeof(long)
                ));
            }

            _inactiveDataList.Add(inactiveData);
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryFSConnectionInactiveData> dataList)> AnalysisAsync()
        {
            _logger.Info($"Successful analysis connection [{_connectionId}] inactive data, count [{_inactiveDataList.Count}].");
            return (true, _inactiveDataList);
        }
    }
}
