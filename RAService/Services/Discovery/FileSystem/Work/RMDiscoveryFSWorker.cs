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
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work
{
    public abstract class RMDiscoveryFSWorker
    {
        protected readonly RALogger _logger;

        protected readonly IRMDiscoveryFSJobDao _jobDao;

        protected readonly IRMDiscoveryFSRuleInfoDao _ruleInfoDao;

        protected readonly IRMDiscoveryConfigurationDao _configurationDao;

        protected readonly IRMDiscoveryFSNodeDao _nodeDao;

        protected readonly IRMDiscoveryFSAgentInfoDao _agentInfoDao;

        protected readonly IRMDiscoveryFSExecutionInfoDao _executionInfoDao;

        public RMDiscoveryFSWorker()
        {
            _logger = RALogger.GetInstance(GetType());
            _jobDao = new RMDiscoveryFSJobDao();
            _ruleInfoDao = new RMDiscoveryFSRuleInfoDao();
            _configurationDao = new RMDiscoveryConfigurationDao();
            _nodeDao = new RMDiscoveryFSNodeDao();
            _agentInfoDao = new RMDiscoveryFSAgentInfoDao();
            _executionInfoDao = new RMDiscoveryFSExecutionInfoDao();
        }
    }
}
