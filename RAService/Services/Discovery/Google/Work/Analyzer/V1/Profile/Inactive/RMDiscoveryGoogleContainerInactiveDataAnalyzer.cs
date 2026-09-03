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
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.Profile.Inactive;

public class RMDiscoveryGoogleContainerInactiveDataAnalyzer
{
    
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleContainerInactiveDataAnalyzer));

    private readonly IRMDiscoveryGoogleProfileDataDao _profileDataDao = new RMDiscoveryGoogleProfileDataDao();
    private readonly string _googleOrganizationId;
    
    private readonly RMDiscoveryGoogleProfileInfo _profileInfo;
    
    private readonly int _containerId;
    
    private readonly List<RMDiscoveryGoogleRuleInfo> _rules;
    
    public RMDiscoveryGoogleContainerInactiveDataAnalyzer(
        string googleOrganizationId,
        RMDiscoveryGoogleProfileInfo profileInfo,
        int containerId,
        List<RMDiscoveryGoogleRuleInfo> rules)
    {
        _googleOrganizationId = googleOrganizationId;
        _profileInfo = profileInfo;
        _containerId = containerId;
        _rules = rules;
    }
    
    public async Task<bool> AnalysisAsync()
    {
        try
        { 
            _logger.Info($"Start analysis google organization [{_googleOrganizationId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

            var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());

            await _profileDataDao.DeleteContainerInactiveDataByContainerIdAsync(_googleOrganizationId, _profileInfo.Id, _containerId);

            var driveDataEnumerable = _profileDataDao.GetDriveInactiveDataByContainerIdAsync(_googleOrganizationId, _profileInfo.Id, _containerId, customColumns);

            var containerDataInfo = new RMDiscoveryGoogleProfileContainerInactiveData
            {
                ContainerId = _containerId,
            };

            foreach (var customColumn in customColumns)
            {
                containerDataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
            }

            await foreach (var siteData in driveDataEnumerable)
            {
                containerDataInfo.InactiveFileTotalSize += siteData.InactiveFileTotalSize;
                containerDataInfo.InactiveFileSumCount += siteData.InactiveFileSumCount;
                containerDataInfo.FileTotalSize += siteData.FileTotalSize;
                containerDataInfo.FileSumCount += siteData.FileSumCount;
                foreach (var customColumn in customColumns)
                {
                    var targetDataCustomColumn =
                        containerDataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                    var sourceDataCustomColumn = siteData.CustomColumns.First(item => item.Name == customColumn.Name);
                    targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) +
                                                   Convert.ToInt64(sourceDataCustomColumn.Value);
                }
            }

            await _profileDataDao.AddContainerInactiveDataListAsync(_googleOrganizationId, _profileInfo.Id, containerDataInfo);

            _logger.Info(
                $"End analysis google organization [{_googleOrganizationId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

            return true;
        }
        catch (Exception e)
        {
            _logger.Error(
                $"An error occurred while analysis google organization [{_googleOrganizationId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data. Error: {e}");
            return false;
        }
    }
}