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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.Model;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General.Inactive
{
    public class RMDiscoveryAOSPSiteInactiveDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPSiteInactiveDataAnalyzer));

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly int _containerId;

        private readonly int _siteId;

        private readonly Guid _siteUniqueId;

        private readonly List<RMDiscoveryAOSPRuleInfo> _rules;

        private readonly RMDiscoveryAOSPFileExtensionAnalysisManager _fileExtensionManager;

        private readonly List<RMDiscoveryAOSPSiteInactiveData> _inactiveDataList = [];

        public RMDiscoveryAOSPSiteInactiveDataAnalyzer(
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
            _rules = rules;
            _fileExtensionManager = fileExtensionManager;
        }

        public void Increse(RMDiscoveryAOSPAnalyzedDataInfo dataInfo)
        {
            var inactiveData = new RMDiscoveryAOSPSiteInactiveData
            {
                ContainerId = _containerId,
                SiteId = _siteId,
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

        public async Task<(bool analysisSucceed, List<RMDiscoveryAOSPSiteInactiveData> dataList, string errorMessage)> AnalysisAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    await _dataDao.DeleteSiteInactiveDataListAsync(_o365TenantId, _siteId);
                    _logger.Info($"Successful delete tenant [{_o365TenantId}] site [{_siteId}] inactive data.");
                }

                await _dataDao.AddSiteInactiveDataListAsync(_o365TenantId, _inactiveDataList.ToArray());

                _logger.Info($"Successful analysis tenant [{_o365TenantId}] site [{_siteId}] inactive data, count [{_inactiveDataList.Count}].");

                return (true, _inactiveDataList, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis tenant [{_o365TenantId}] site [{_siteId}] inactive data. Error: {e}");
                return (false, [], e.Message);
            }
        }
    }
}
