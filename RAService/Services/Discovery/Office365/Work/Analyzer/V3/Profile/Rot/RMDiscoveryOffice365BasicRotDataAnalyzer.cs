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

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V3.Profile.Rot
{
    public class RMDiscoveryOffice365BasicRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365BasicRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365ProfileDataDao _profileDataDao;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly RMDiscoveryOffice365ProfileInfo _profileInfo;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        public RMDiscoveryOffice365BasicRotDataAnalyzer(
            Guid o365TenantId,
            SourceFlag contentSource,
            RMDiscoveryOffice365ProfileInfo profileInfo,
            List<RMDiscoveryOffice365RuleInfo> rules
            )
        {
            _profileDataDao = new RMDiscoveryOffice365ProfileDataDao();
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _profileInfo = profileInfo;
            _rules = rules;
        }

        public async Task<bool> AnalysisAsync()
        {
            try
            {
                _logger.Info($"Start analysis o365 tenant [{_o365TenantId}] cotnent source [{_contentSource}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                var customColumns = new List<RMDiscoveryCustomColumn>();

                if (!_profileInfo.IsBuildIn)
                {
                    var ruleIds = JsonConvert.DeserializeObject<List<int>>(_profileInfo.RuleIdsJson);
                    var ruleInfoes = _rules.Where(item => ruleIds.Contains(item.Id)).ToList();
                    customColumns = ruleInfoes.ConvertAll(item => item.ToCustomColumn());
                }

                await _profileDataDao.DeleteBasicRotDataByContentSourceAsync(_o365TenantId, _profileInfo.Id, _contentSource);

                var containerDataEnumerable = _profileDataDao.GetContainerRotDataByContentSourceAsync(_o365TenantId, _profileInfo.Id, _contentSource, customColumns);

                var basicDataInfo = new RMDiscoveryProfileBasicRotData
                {
                    ContentSource = _contentSource
                };

                foreach (var customColumn in customColumns)
                {
                    basicDataInfo.CustomColumns.Add(new(customColumn.Name, 0L, typeof(long)));
                }

                await foreach (var containerData in containerDataEnumerable)
                {
                    basicDataInfo.FileTotalSize += containerData.FileTotalSize;
                    basicDataInfo.RotFileTotalSize += containerData.RotFileTotalSize;
                    basicDataInfo.RCategoryFileTotalSize += containerData.RCategoryFileTotalSize;
                    basicDataInfo.OCategoryFileTotalSize += containerData.OCategoryFileTotalSize;
                    basicDataInfo.TCategoryFileTotalSize += containerData.TCategoryFileTotalSize;
                    foreach (var customColumn in customColumns)
                    {
                        var targetDataCustomColumn = basicDataInfo.CustomColumns.First(item => item.Name == customColumn.Name);
                        var sourceDataCustomColumn = containerData.CustomColumns.First(item => item.Name == customColumn.Name);
                        targetDataCustomColumn.Value = Convert.ToInt64(targetDataCustomColumn.Value) + Convert.ToInt64(sourceDataCustomColumn.Value);
                    }
                }

                await _profileDataDao.AddBasicRotDataListAsync(_o365TenantId, _profileInfo.Id, basicDataInfo);

                _logger.Info($"End analysis o365 tenant [{_o365TenantId}] cotnent source [{_contentSource}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis o365 tenant [{_o365TenantId}] cotnent source [{_contentSource}] profile [{_profileInfo.Id} {_profileInfo.Name}] rot data. Error: {e}");
                return false;
            }
        }
    }
}
