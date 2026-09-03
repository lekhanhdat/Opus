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
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using NVelocity.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Profile.Inactive
{
    public class RMDiscoveryOffice365ContainerInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ContainerInactiveDataAnalyzer));

        private readonly IRMDiscoveryOffice365ProfileDataDao _profileDataDao;

        private readonly Guid _o365TenantId;

        private readonly int _containerId;

        private readonly SourceFlag _contentSource;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        public RMDiscoveryOffice365ContainerInactiveDataAnalyzer(
            Guid o365TenantId,
            SourceFlag  contentSource,
            RMDiscoveryOffice365ProfileInfo profileInfo,
            int containerId,
            List<RMDiscoveryOffice365RuleInfo> rules
            )
        {
            _profileDataDao = new RMDiscoveryOffice365ProfileDataDao();
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _profileInfo = profileInfo;
            _containerId = containerId;
            _rules = rules;
        }

        public async Task<bool> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

                var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());

                await _profileDataDao.DeleteContainerInactiveDataByContainerIdAsync(_o365TenantId, _profileInfo.Id, _containerId);

                var siteDataEnumerable = _profileDataDao.GetSiteInactiveDataByContainerIdAsync(_o365TenantId, _profileInfo.Id, _containerId, customColumns);

                var containerDataInfo = new RMDiscoveryProfileContainerInactiveData
                {
                    ContainerId = _containerId,
                    ContentSource = _contentSource
                };

                foreach (var customColumn in customColumns)
                {
                    containerDataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
                }

                await foreach(var siteData in siteDataEnumerable)
                {
                    containerDataInfo.InactiveFileTotalSize += siteData.InactiveFileTotalSize;
                    containerDataInfo.InactiveFileSumCount += siteData.InactiveFileSumCount;
                    containerDataInfo.FileTotalSize += siteData.FileTotalSize;
                    containerDataInfo.FileSumCount += siteData.FileSumCount;
                    foreach (var customColumn in customColumns)
                    {
                        var targetDataCustomColumn = containerDataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                        var sourceDataCustomColumn = siteData.CustomColumns.First(item => item.Name == customColumn.Name);
                        targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) + Convert.ToInt64(sourceDataCustomColumn.Value);
                    }
                }

                await _profileDataDao.AddContainerInactiveDataListAsync(_o365TenantId, _profileInfo.Id, containerDataInfo);

                _logger.Info($"End analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] inactive data. Error: {e}");
                return false;
            }
        }
    }
}
