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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Profile.Rot
{
    public class RMDiscoveryOffice365ContainerRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ContainerRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365ProfileDataDao _profileDataDao;

        private readonly Guid _o365TenantId;

        private readonly int _containerId;

        private readonly SourceFlag _contentSource;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        public RMDiscoveryOffice365ContainerRotDataAnalyzer(
            Guid o365TenantId,
            SourceFlag contentSource,
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
                _logger.Info($"Start analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                var customColumns = new List<RMDiscoveryCustomColumn>();
                
                if(!_profileInfo.IsBuildIn)
                {
                    var ruleIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.RuleIdsJson);
                    var ruleInfoes = _rules.Where(item => ruleIds.Contains(item.Id)).ToList();
                    customColumns = ruleInfoes.ConvertAll(item => item.ToCustomColumn());
                }

                await _profileDataDao.DeleteContainerRotDataByContainerIdAsync(_o365TenantId, _profileInfo.Id, _containerId);

                var siteDataEnumerable = _profileDataDao.GetSiteRotDataByContainerIdAsync(_o365TenantId, _profileInfo.Id, _containerId, customColumns);

                var containerDataInfo = new RMDiscoveryProfileContainerRotData
                {
                    ContainerId = _containerId,
                    ContentSource = _contentSource
                };

                foreach (var customColumn in customColumns)
                {
                    containerDataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
                }

                await foreach (var siteData in siteDataEnumerable)
                {
                    containerDataInfo.FileTotalSize += siteData.FileTotalSize;
                    containerDataInfo.RotFileTotalSize += siteData.RotFileTotalSize;
                    containerDataInfo.RCategoryFileTotalSize += siteData.RCategoryFileTotalSize;
                    containerDataInfo.OCategoryFileTotalSize += siteData.OCategoryFileTotalSize;
                    containerDataInfo.TCategoryFileTotalSize += siteData.TCategoryFileTotalSize;
                    foreach (var customColumn in customColumns)
                    {
                        var targetDataCustomColumn = containerDataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                        var sourceDataCustomColumn = siteData.CustomColumns.First(item => item.Name == customColumn.Name);
                        targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) + Convert.ToInt64(sourceDataCustomColumn.Value);
                    }
                }

                await _profileDataDao.AddContainerRotDataListAsync(_o365TenantId, _profileInfo.Id, containerDataInfo);

                _logger.Info($"End analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{_o365TenantId}] container [{_containerId}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data. Error: {e}");
                return false;
            }
        }
    }
}
