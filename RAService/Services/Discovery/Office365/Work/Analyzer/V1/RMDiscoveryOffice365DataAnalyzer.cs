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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1
{
    public abstract class RMDiscoveryOffice365DataAnalyzer
    {
        protected readonly RALogger _logger;

        protected readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        protected readonly IRMDiscoveryConfigurationDao _configurationDao;

        protected readonly IRMDiscoveryOffice365DataDao _dataDao;

        protected readonly IEApiClient _ieApiClient;

        protected readonly SourceFlag _contentSource;

        protected readonly RMDiscoveryOffice365AnalysisJob _jobInfo;

        protected readonly RMDiscoveryOffice365FileExtensionAnalysisManager _fileExtensionAnalysisManager;

        protected readonly RMDiscoveryOffice365ContainerInfo _containerInfo;

        protected readonly RMDiscoveryOffice365SiteInfo _siteInfo;

        protected readonly List<int> _sizeRangeIds;

        protected readonly List<int> _dateRangeIds;

        protected readonly bool _enableExpandQueryTest;

        protected static readonly Dictionary<string, int> s_fileTypes = new();

        public RMDiscoveryOffice365DataAnalyzer(
            SourceFlag contentSource,
            RMDiscoveryOffice365AnalysisJob jobInfo,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalysisManager,
            RMDiscoveryOffice365ContainerInfo containerInfo,
            RMDiscoveryOffice365SiteInfo siteInfo,
            List<int> sizeRangeIds,
            List<int> dateRangeIds,
            bool enableExpandQueryTest)
        {
            _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DataAnalyzer));
            _contentSource = contentSource;
            _jobInfo = jobInfo;
            _fileExtensionAnalysisManager = fileExtensionAnalysisManager;
            _containerInfo = containerInfo;
            _siteInfo = siteInfo;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _enableExpandQueryTest = enableExpandQueryTest;
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _configurationDao = new RMDiscoveryConfigurationDao();
            _dataDao = new RMDiscoveryOffice365DataDao();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
        }

        public abstract Task<bool> AnalysisAsync();
    }
}
