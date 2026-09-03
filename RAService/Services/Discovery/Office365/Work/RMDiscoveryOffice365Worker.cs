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
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public abstract class RMDiscoveryOffice365Worker
    {
        protected readonly RALogger _logger;

        protected readonly IEApiClient _ieApiClient;

        protected readonly IRMDiscoveryOffice365JobDao _jobDao;

        protected readonly IRMDiscoveryOffice365TenantDao _o365TenantDao;

        protected readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        protected readonly IRMDiscoveryConfigurationDao _configurationDao;

        protected readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        protected readonly IRMDiscoveryExecutionInfoDao _executionInfoDao;

        public RMDiscoveryOffice365Worker()
        {
            _logger = RALogger.GetInstance(GetType());
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _o365TenantDao = new RMDiscoveryOffice365TenantDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _configurationDao = new RMDiscoveryConfigurationDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _executionInfoDao = new RMDiscoveryExecutionInfoDao();
        }
    }
}
