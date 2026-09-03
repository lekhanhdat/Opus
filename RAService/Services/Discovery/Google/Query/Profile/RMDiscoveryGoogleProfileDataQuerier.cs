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
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Google;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.Profile
{
    public abstract class RMDiscoveryGoogleProfileDataQuerier<T>
    {
        protected readonly RALogger _logger;

        protected readonly IRMDiscoveryGoogleDataQueryDao _queryDao = new RMDiscoveryGoogleDataQueryDao();

        protected readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        protected readonly IRMDiscoveryGoogleProfileDao _profileDao = new RMDiscoveryGoogleProfileDao();

        protected readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

        protected readonly RMDiscoveryGoogleProfileQueryParameter _queryParameter;

        protected readonly string _googleOrganizationSchemaName;

        protected readonly string _profileSchemaName;

        public RMDiscoveryGoogleProfileDataQuerier(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            _logger = RALogger.GetInstance(GetType());
            _queryParameter = queryParameter;
            _googleOrganizationSchemaName = RMDiscoveryDBManager.GetGoogleSchemaName(queryParameter.OrganizationId);
            _profileSchemaName = RMDiscoveryDBManager.GetGoogleSchemaName(queryParameter.OrganizationId, queryParameter.ProfileId);
            SecurityUtils.SanitizeSQLSchemaName(_profileSchemaName);
            SecurityUtils.SanitizeSQLSchemaName(_googleOrganizationSchemaName);
        }

        public abstract Task<T> QueryAsync();
    }
}
