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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Rot
{
    public abstract class RMDiscoveryFSRotDataQuerier<T>
    {
        protected readonly RALogger _logger;

        protected readonly IRMDiscoveryFSDataQueryDao _queryDao = new RMDiscoveryFSDataQueryDao();

        protected readonly IRMDiscoveryFSRuleInfoDao _ruleInfoDao = new RMDiscoveryFSRuleInfoDao();

        protected readonly RMDiscoveryFSQueryParameter _queryParameter;

        protected readonly string _schemaName;

        public RMDiscoveryFSRotDataQuerier(RMDiscoveryFSQueryParameter queryParameter)
        {
            _logger = RALogger.GetInstance(GetType());
            _schemaName = RMDiscoveryDBManager.GetFileSystemSchemaName();
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            _queryParameter = queryParameter;
        }

        public abstract Task<T> QueryAsync();
    }
}
