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
using AvePoint.RA.DB.Model.Discovery.Office365;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.ROT;

public class RMDiscoveryGoogleRotDataAnalyzer
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleRotDataAnalyzer));
    
    private readonly string _googleOrganizationId;
    
    private readonly RMDiscoveryGoogleProfileInfo _profileInfo;
    
    private readonly List<RMDiscoveryGoogleDriveInfo> _needAnalysisDriveInfoList;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;
    
    private readonly List<int> _sizeRangeIds;
    
    private readonly List<int> _dateRangeIds;
    
    private readonly List<RMDiscoveryGoogleFileExtension> _fileExtensions;
    

    public RMDiscoveryGoogleRotDataAnalyzer(
        string googleOrganizationId,
        RMDiscoveryGoogleProfileInfo profileInfo,
        List<RMDiscoveryGoogleDriveInfo> needAnalysisDriveInfoList,
        List<RMDiscoveryGoogleRuleInfo> rules,
        List<int> sizeRangeIds,
        List<int> dateRangeIds,
        List<RMDiscoveryGoogleFileExtension> fileExtensions)
    {
        _googleOrganizationId = googleOrganizationId;
        _profileInfo = profileInfo;
        _needAnalysisDriveInfoList = needAnalysisDriveInfoList;
        _rules = rules;
        _sizeRangeIds = sizeRangeIds;
        _dateRangeIds = dateRangeIds;
        _fileExtensions = fileExtensions;
    }
    public async Task<List<RMDiscoveryGoogleDriveInfo>> AnalysisAsync()
    {
        try
        {
            _logger.Info($"Start analysis profile [{_profileInfo.Id}] rot data.");

            List<RMDiscoveryGoogleDriveInfo> failedDriveInfoList = [];

            var containerAndDriveListDictionary = _needAnalysisDriveInfoList.GroupBy(item => item.ContainerId)
                .ToDictionary(
                    item => item.Key,
                    item => item.ToList());

            var fileExtensionIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.FileExtensionIdsJson);
            var fileExtensionsProfile = _fileExtensions.Where(item => fileExtensionIds.Count == 0 || fileExtensionIds.Contains(item.Id)).Select(item => item.Name == "RM_FA_FileType_Empty" ? "" : item.Name).ToList();
            
            foreach (var (containerId, driveInfoList) in containerAndDriveListDictionary)
            {
                foreach (var driveInfo in driveInfoList)
                {
                    var driveDataAnalyzer = new RMDiscoveryGoogleDriveRotDataAnalyzer(
                        _googleOrganizationId,
                        driveInfo,
                        _profileInfo,
                        _rules,
                        _sizeRangeIds,
                        _dateRangeIds,
                        fileExtensionsProfile);
                    var driveAnalysisSucceed = await driveDataAnalyzer.AnalysisAsync();
                    if (!driveAnalysisSucceed)
                    {
                        failedDriveInfoList.Add(driveInfo);
                    }
                }

                var containerDataAnalyzer = new RMDiscoveryGoogleContainerRotDataAnalyzer(_googleOrganizationId, _profileInfo, containerId, _rules);
                var containerAnalysisSucceed = await containerDataAnalyzer.AnalysisAsync();
                if (!containerAnalysisSucceed)
                {
                    failedDriveInfoList.AddRange(driveInfoList);
                }
            }
            
            var basicDataAnalyzer = new RMDiscoveryGoogleBasicRotDataAnalyzer(_googleOrganizationId, _profileInfo, _rules);
            var basicAnalysisSucceed = await basicDataAnalyzer.AnalysisAsync();

            if (!basicAnalysisSucceed)
            {
                failedDriveInfoList.AddRange(containerAndDriveListDictionary.Values.SelectMany(item => item));
            }

            _logger.Info($"End analysis profile [{_profileInfo.Id}] rot data. Failed count [{failedDriveInfoList.Count}].");

            return failedDriveInfoList;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while analysis profile [{_profileInfo.Id}] rot data. Error: {e}");
            return _needAnalysisDriveInfoList;
        }
    }
}