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
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Core.Discovery;
using RADiscovery.Query.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.SourceQuerier
{
    public abstract class DiscoverySourceQuerier
    {
        protected const string S_BASIC_INACTIVE_TABLE = "RMBasicInactiveData";

        protected const string S_BASIC_ROT_TABLE = "RMBasicRotData";

        protected readonly RALogger _logger;

        protected readonly DiscoveryQueryParameter _queryParameter;

        protected readonly string _schemaName;

        public DiscoverySourceQuerier(DiscoveryQueryParameter queryParameter)
        {
            if(queryParameter.O365TenantId == Guid.Empty)
            {
                throw new ArgumentException("queryParameter.O365TenantId");
            }

            _logger = RALogger.GetInstance(GetType());
            _queryParameter = queryParameter;
            _schemaName = RMDiscoveryDBManager.GetSchemaName(_queryParameter.O365TenantId);
        }

        protected string GetInactiveDataTableName()
        {
            if (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.Container && 
                (!string.IsNullOrWhiteSpace(_queryParameter.NodeQueryParameter.SearchKey) || _queryParameter.NodeQueryParameter.ContainerIds.Any()))
            {
                return "RMContainerInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.SiteInContainer || 
                (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.Site && (!string.IsNullOrWhiteSpace(_queryParameter.NodeQueryParameter.SearchKey) || _queryParameter.NodeQueryParameter.SiteIds.Any())))
            {
                return "RMSiteInactiveData";
            }

            return "RMBasicInactiveData";
        }

        protected string GetRotDataTableName()
        {
            if (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.Container && _queryParameter.NodeQueryParameter.ContainerIds.Any())
            {
                return "RMContainerRotData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.SiteInContainer ||
                (_queryParameter.NodeQueryParameter.ViewMode == DiscoveryNodeViewMode.Site && _queryParameter.NodeQueryParameter.SiteIds.Any()))
            {
                return "RMSiteRotData";
            }

            return "RMBasicRotData";
        }
    }
}
