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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive
{
    public abstract class RMDiscoveryInactiveDataQuerier<T> : RMDiscoveryDataQuerier<T>
    {
        public RMDiscoveryInactiveDataQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        protected override string GetDataTable(bool queryNodeInfo = false)
        {
            if (queryNodeInfo)
            {
                return _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container ? "RMContainerInactiveData" : "RMSiteInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container &&
                _queryParameter.NodeQueryParameter.ContainerIds.Any())
            {
                return "RMContainerInactiveData";
            }

            if (_queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.SiteInContainer ||
                _queryParameter.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Site && _queryParameter.NodeQueryParameter.SiteIds.Any())
            {
                return "RMSiteInactiveData";
            }

            return "RMBasicInactiveData";
        }
    }
}
