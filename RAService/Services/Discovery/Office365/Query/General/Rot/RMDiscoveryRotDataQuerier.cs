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
using AvePoint.RA.Contract.Discovery.Model.Query;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot
{
    public abstract class RMDiscoveryRotDataQuerier<T> : RMDiscoveryDataQuerier<T>
    {
        protected RMDiscoveryRotDataQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        protected override string GetDataTable(bool queryNodeInfo = false)
        {
            if (queryNodeInfo)
            {
                return _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container ? "RMContainerRotData" : "RMSiteRotData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container &&
                (!string.IsNullOrWhiteSpace(_queryParameter.NodeQueryParameter.SearchKey) || _queryParameter.NodeQueryParameter.ContainerIds.Any()))
            {
                return "RMContainerRotData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer ||
                _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Site && (!string.IsNullOrWhiteSpace(_queryParameter.NodeQueryParameter.SearchKey) || _queryParameter.NodeQueryParameter.SiteIds.Any()))
            {
                return "RMSiteRotData";
            }

            return "RMBasicRotData";
        }

        protected override List<RMDiscoverySqlDefinition> GetAllConditionSqlDefinitions()
        {
            var res = base.GetAllConditionSqlDefinitions();

            if (_queryParameter.ROTRuleQueryParameter != null &&
                _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("dbo", DataTableAlias, out var sqlDefinition))
            {
                res.Add(sqlDefinition);
            }
            return res;
        }
    }
}
