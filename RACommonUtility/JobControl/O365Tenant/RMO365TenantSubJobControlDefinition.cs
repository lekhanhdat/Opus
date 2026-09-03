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

namespace AvePoint.RA.RACommonUtility.JobControl.O365Tenant
{
    public class RMO365TenantSubJobControlDefinition
    {
        public string TenantId { get; set; }

        // GB
        public int AverageUserDataSize { get; set; } = 5;

        // MB
        public int AverageFileSize { get; set; } = 1;

        // Files/Hour
        public int ScanSpeed { get; set; } = 30000;

        public List<RMO365TenantJobControlSLA> SLACollection { get; set; } = new();

        public double Rate { get; set; } = 1;

        public int MinLimit { get; set; } = 5;

        public int MaxLimit { get; set; } = 100;
    }

    public class RMO365TenantJobControlSLA
    {
        public int UserSeats { get; set; }

        public int Days { get; set; }

        public RMO365TenantJobControlSLA() { }

        public RMO365TenantJobControlSLA(int userSeats, int days)
        {
            UserSeats = userSeats;
            Days = days;
        }
    }
}
