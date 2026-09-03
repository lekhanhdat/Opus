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
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Utils
{
    public sealed class RMDiscoveryOffice365ReportHeaderDefinition
    {
        public IReadOnlyList<string> OrderedColumns { get; }

        private RMDiscoveryOffice365ReportHeaderDefinition(List<string> orderedColumns)
        {
            OrderedColumns = orderedColumns;
        }

        public static RMDiscoveryOffice365ReportHeaderDefinition FromList(IEnumerable<string> columns)
        {
            return new RMDiscoveryOffice365ReportHeaderDefinition(columns.ToList());
        }

        public static RMDiscoveryOffice365ReportHeaderDefinition FromOrderColumn(Dictionary<int, string> orderColumn)
        {
            return new RMDiscoveryOffice365ReportHeaderDefinition(orderColumn.OrderBy(x => x.Key).Select(x => x.Value).ToList());
        }
    }
}
