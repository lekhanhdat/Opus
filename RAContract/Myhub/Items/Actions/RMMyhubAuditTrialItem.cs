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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Items.Actions
{
    public class RMMyhubAuditTrialFilterItem
    {
        public Dictionary<int, string> ActionItems { get; set; }
        public Dictionary<int, string> ModuleItems { get; set; }
        public Dictionary<int, string> StatusItems { get; set; }
        public List<string> UserItems { get; set; }
    }

    public class RMMyhubReportAuditItem
    {
        public Guid ConnGroupId { get; set; }
        public Guid ConnectionId { get; set; }
        public Guid ItemId { get; set; }
        public string FullPath { get; set; }
        public string ReportName { get; set; }
        public int Level { get; set; }
    }
}
