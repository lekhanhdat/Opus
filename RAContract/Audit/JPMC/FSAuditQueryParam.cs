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

namespace AvePoint.RA.Contract.Audit.JPMC
{
    public class FSAuditQueryParam
    {
        public string SearchKey { get; set; }

        public List<FSAuditQueryFilter> Filters { get; set; }

        public FSAuditQueryOrder Order { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public bool IsDesc { get; set; } = true;

        public string PartitionKeyId { get; set; }
    }

    public class FSAuditQueryResult
    {
        public List<FSAuditRecord> Items { get; set; }
        public int TotalCount { get; set; }
        //This boolean value is only for audit trial in myhub
        public bool HasMore { get; set; }
    }

    public class FSAuditQueryFilter
    {
        public string ColumnName { get; set; }
        public List<string> ColumnValues { get; set; }
        public string MyhubTimeZoneId { get; set; } = null;
    }

    public class FSAuditQueryOrder
    {
        public string ColumnName { get; set; }
        public bool IsDesc { get; set; }
    }
}
