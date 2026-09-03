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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Inactive;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.Inactive;

public class RMDiscoveryGoogleInactiveDataAnalyzer
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleInactiveDataAnalyzer));
    
    private readonly string _googleOrganizationId;
    
    private readonly RMDiscoveryGoogleProfileInfo _profileInfo;
    
    private readonly List<RMDiscoveryGoogleDriveInfo> _needAnalysisDriveInfoList;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

    public RMDiscoveryGoogleInactiveDataAnalyzer(
        string googleOrganizationId,
        RMDiscoveryGoogleProfileInfo profileInfo,
        List<RMDiscoveryGoogleDriveInfo> needAnalysisDriveInfoList,
        List<RMDiscoveryGoogleRuleInfo> rules)
    {
        _googleOrganizationId = googleOrganizationId;
        _profileInfo = profileInfo;
        _needAnalysisDriveInfoList = needAnalysisDriveInfoList;
        _rules = rules;
    }

    public async Task<List<RMDiscoveryGoogleDriveInfo>> AnalysisAsync()
    {
        try
        {
            _logger.Info($"Start analysis profile [{_profileInfo.Id}] inactive data.");

            var failedDriveInfoList = new List<RMDiscoveryGoogleDriveInfo>();

            var containerAndDriveInfoListDictionary = _needAnalysisDriveInfoList.GroupBy(item => item.ContainerId)
                .ToDictionary(
                    item => item.Key,
                    item => item.ToList());

            foreach (var (containerId, driveInfoList) in containerAndDriveInfoListDictionary)
            {
                foreach (var driveInfo in driveInfoList)
                {
                    var driveDataAnalyzer =
                        new RMDiscoveryGoogleDriveInactiveDataAnalyzer(_googleOrganizationId, driveInfo, _profileInfo,
                            _rules);
                    var driveAnalysisSucceed = await driveDataAnalyzer.AnalysisAsync();
                    if (!driveAnalysisSucceed)
                    {
                        failedDriveInfoList.Add(driveInfo);
                    }
                }

                var containerDataAnalyzer =
                    new RMDiscoveryGoogleContainerInactiveDataAnalyzer(_googleOrganizationId, _profileInfo, containerId,
                        _rules);
                var containerAnalysisSucceed = await containerDataAnalyzer.AnalysisAsync();
                if (!containerAnalysisSucceed)
                {
                    failedDriveInfoList.AddRange(driveInfoList);
                }
            }

            var basicDataAnalyzer =
                new RMDiscoveryGoogleBasicInactiveDataAnalyzer(_googleOrganizationId, _profileInfo, _rules);
            var basicAnalysisSucceed = await basicDataAnalyzer.AnalysisAsync();

            if (!basicAnalysisSucceed)
            {
                failedDriveInfoList.AddRange(containerAndDriveInfoListDictionary.Values.SelectMany(item => item));
            }

            _logger.Info(
                $"End analysis profile [{_profileInfo.Id}] inactive data. Failed count [{failedDriveInfoList.Count}].");

            return failedDriveInfoList;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while analysis profile [{_profileInfo.Id}] inactive data. Error: {e}");
            return _needAnalysisDriveInfoList;
        }
    }
    
}