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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile
{
    public abstract class RMDiscoveryProfileDataQuerier<T>
    {
        protected readonly RALogger _logger;

        protected readonly IRMDiscoveryOffice365DataQueryDao _queryDao = new RMDiscoveryOffice365DataQueryDao();

        protected readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        protected readonly IRMDiscoveryOffice365ProfileDao _profileDao = new RMDiscoveryOffice365ProfileDao();

        protected readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        protected readonly RMDiscoveryOffice365ProfileQueryParameter _queryParameter;

        protected readonly string _o365TenantSchemaName;

        protected readonly string _profileSchemaName;

        public RMDiscoveryProfileDataQuerier(RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            _logger = RALogger.GetInstance(GetType());
            _queryParameter = queryParameter;
            _o365TenantSchemaName = RMDiscoveryDBManager.GetOffice365SchemaName(queryParameter.O365TenantId);
            _profileSchemaName = RMDiscoveryDBManager.GetOffice365SchemaName(queryParameter.O365TenantId, queryParameter.ProfileId);
            SecurityUtils.SanitizeSQLSchemaName(_profileSchemaName);
            SecurityUtils.SanitizeSQLSchemaName(_o365TenantSchemaName);
        }

        public abstract Task<T> QueryAsync();
    }
}
