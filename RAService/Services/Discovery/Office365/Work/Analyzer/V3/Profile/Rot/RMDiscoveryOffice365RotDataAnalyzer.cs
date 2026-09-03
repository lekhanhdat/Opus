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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile.Rot
{
    public class RMDiscoveryOffice365RotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365RotDataAnalyzer));

        private readonly Guid _o365TenantId;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365SiteInfo> _needAnalysisSiteInfoes;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<RMDiscoveryOffice365FileExtension> _fileExtensions;

        public RMDiscoveryOffice365RotDataAnalyzer(
                Guid o365TenantId,
                RMDiscoveryOffice365ProfileInfo profileInfo,
                List<RMDiscoveryOffice365SiteInfo> needAnalysisSiteInfoes,
                List<RMDiscoveryOffice365RuleInfo> rules,
                List<int> sizeRangeIds,
                List<int> dateRangeIds,
                List<RMDiscoveryOffice365FileExtension> fileExtensions
            )
        {
            _o365TenantId = o365TenantId;
            _profileInfo = profileInfo;
            _needAnalysisSiteInfoes = needAnalysisSiteInfoes;
            _rules = rules;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _fileExtensions = fileExtensions;
        }

        public async Task<List<RMDiscoveryOffice365SiteInfo>> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis profile [{_profileInfo.Id}] rot data.");

                var failedSiteInfoes = new List<RMDiscoveryOffice365SiteInfo>();

                var contentSourceDic = _needAnalysisSiteInfoes.GroupBy(item => item.ContentSource)
                    .ToDictionary(
                        item => item.Key,
                        item => item.GroupBy(i => i.ContainerId).ToDictionary(i => i.Key, i => i.ToList()));

                foreach (var contentSourceEntry in contentSourceDic)
                {
                    var contentSource = contentSourceEntry.Key;

                    foreach (var containerEntry in contentSourceEntry.Value)
                    {
                        var containerId = containerEntry.Key;
                        foreach (var siteInfo in containerEntry.Value)
                        {
                            var siteDataAnalyzer = new RMDiscoveryOffice365SiteRotDataAnalyzer(
                                _o365TenantId, 
                                siteInfo, 
                                _profileInfo, 
                                _rules,
                                _sizeRangeIds,
                                _dateRangeIds,
                                _fileExtensions);
                            var siteAnalysisSucceed = await siteDataAnalyzer.AnalysisAsync();
                            if (!siteAnalysisSucceed)
                            {
                                failedSiteInfoes.Add(siteInfo);
                            }
                        }

                        var containerDataAnalyzer = new RMDiscoveryOffice365ContainerRotDataAnalyzer(_o365TenantId, contentSource, _profileInfo, containerId, _rules);
                        var containerAnalysisSucceed = await containerDataAnalyzer.AnalysisAsync();
                        if (!containerAnalysisSucceed)
                        {
                            failedSiteInfoes.AddRange(containerEntry.Value);
                        }
                    }

                    var basicDataAnalyzer = new RMDiscoveryOffice365BasicRotDataAnalyzer(_o365TenantId, contentSource, _profileInfo, _rules);
                    var basicAnalysisSucceed = await basicDataAnalyzer.AnalysisAsync();

                    if (!basicAnalysisSucceed)
                    {
                        failedSiteInfoes.AddRange(contentSourceEntry.Value.SelectMany(item => item.Value));
                    }
                }

                _logger.Info($"End analysis profile [{_profileInfo.Id}] rot data. Failed count [{failedSiteInfoes.Count}].");

                return failedSiteInfoes;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis profile [{_profileInfo.Id}] rot data. Error: {e}");
                return _needAnalysisSiteInfoes;
            }
        }
    }
}
