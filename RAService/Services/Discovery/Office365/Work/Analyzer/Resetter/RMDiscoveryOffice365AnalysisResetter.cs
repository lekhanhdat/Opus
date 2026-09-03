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
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.Resetter
{
    public abstract class RMDiscoveryOffice365AnalysisResetter
    {
        protected readonly RALogger _logger;

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        protected readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        protected readonly RMDiscoveryOffice365AnalysisJob _analysisJob;

        public RMDiscoveryOffice365AnalysisResetter(RMDiscoveryOffice365AnalysisJob analysisJob)
        { 
            _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365AnalysisResetter));
            _dataDao = new RMDiscoveryOffice365DataDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _analysisJob = analysisJob;
        }

        protected abstract Task ResetAsync(RMDiscoveryOffice365SiteInfo siteInfo);

        public async Task<bool> ResetAsync()
        {
            try
            {
                var siteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(_analysisJob.O365TenantId, _analysisJob.SiteId);
                if(siteInfo == null)
                {
                    _logger.Info($"No site found in discovery db. Reset skipped.");
                    return true;
                }

                await ResetAsync(siteInfo);

                await _nodeDao.DeleteDiscoverySiteAsync(_analysisJob.O365TenantId, siteInfo);
                _logger.Info($"The site data has been reset.");
                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while reset site. Error: {e}");
                return false;
            }
        }

        protected async Task<List<RMDiscoveryOffice365SiteRotData>> ResetSiteRotDataAsync(RMDiscoveryOffice365SiteInfo siteInfo)
        {
            try
            {
                var dataList = await _dataDao.GetSiteRotDataListAsync(_analysisJob.O365TenantId, siteInfo.Id);

                await _dataDao.DeleteSiteRotDataListAsync(_analysisJob.O365TenantId, siteInfo.Id);
                _logger.Info($"The site rot data has been  reset.");

                return dataList;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while reset site rot data. Error: {e}");
                throw;
            }
        }

        protected async Task<List<RMDiscoveryOffice365SiteInactiveData>> ResetSiteInactiveDataAsync(RMDiscoveryOffice365SiteInfo siteInfo, List<RMDiscoveryCustomColumn> customColumns)
        {
            try
            {
                var dataList = await _dataDao.GetSiteInactiveDataListAsync(_analysisJob.O365TenantId, siteInfo.Id, customColumns);

                await _dataDao.DeleteSiteInactiveDataListAsync(_analysisJob.O365TenantId, siteInfo.Id);
                _logger.Info($"The site inactive data has been  reset.");

                return dataList;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while reset site inactive data. Error: {e}");
                throw;
            }
        }
    }
}
