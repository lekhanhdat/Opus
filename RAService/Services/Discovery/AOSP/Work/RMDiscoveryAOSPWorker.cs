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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery;
using Cloud.Sdk.IE;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work
{
    public abstract class RMDiscoveryAOSPWorker
    {
        protected RALogger _logger;

        protected readonly IEApiClient _ieApiClient;

        protected readonly IRMDiscoveryAOSPJobDao _jobDao;

        protected readonly IRMDiscoveryAOSPTenantDao _aospTenantDao;

        protected readonly IRMDiscoveryAOSPRuleInfoDao _ruleInfoDao;

        protected readonly IRMDiscoveryAOSPConfigurationDao _configurationDao;

        protected readonly IRMDiscoveryAOSPNodeDao _nodeDao;

        //protected readonly IRMDiscoveryExecutionInfoDao _executionInfoDao;

        public RMDiscoveryAOSPWorker()
        {
            _logger = RALogger.GetInstance(GetType());
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryAOSPJobDao();
            _aospTenantDao = new RMDiscoveryAOSPTenantDao();
            _ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();
            _configurationDao = new RMDiscoveryAOSPConfigurationDao();
            _nodeDao = new RMDiscoveryAOSPNodeDao();
            //_executionInfoDao = new RMDiscoveryExecutionInfoDao();
        }
    }
}
