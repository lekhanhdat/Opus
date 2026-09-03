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
using System.Text;

namespace AvePoint.GCommon.Utility.PerformanceMonitor
{
    public class NetworkAdapter
    {
        public String AdapterType { get; set; }
        public String AdapterTypeId { get; set; }
        public String Caption { get; set; }
        public String Description { get; set; }
        public String MacAddress { get; set; }
        public String Manufacturer { get; set; }
        public String Name { get; set; }
        public String NetConnectionId { get; set; }
        public String ProductName { get; set; }
        public String ServiceName { get; set; }
        public String Status { get; set; }
        public ulong Speed { get; set; }
        public Double LinkSpeed { get; set; }
        public String SentPerSec { get; set; }
        public String ReceivedPerSec { get; set; }
    }
}
