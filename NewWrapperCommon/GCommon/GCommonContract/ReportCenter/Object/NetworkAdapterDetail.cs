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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.Adonis.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NetworkAdapterDetail
    {
        [DataMember]
        public string HostName { set; get; }
        [DataMember]
        public string AdapterType { set; get; }
        [DataMember]
        public string AdapterTypeId { set; get; }
        [DataMember]
        public string Caption { set; get; }
        [DataMember]
        public string Description { set; get; }
        [DataMember]
        public String MacAddress { set; get; }
        [DataMember]
        public String Manufacturer { set; get; }
        [DataMember]
        public String Name { set; get; }
        [DataMember]
        public String NetConnectionId { set; get; }
        [DataMember]
        public String ProductName { set; get; }
        [DataMember]
        public String ServiceName { set; get; }
        [DataMember]
        public String Status { set; get; }
        [DataMember]
        public String Speed { set; get; }
        [DataMember]
        public String Width { set; get; }
        [DataMember]
        public string LinkSpeed { set; get; }
        [DataMember]
        public String ReceivedPerSec { set; get; }
        [DataMember]
        public String SentPerSec { set; get; }
        [DataMember]
        public string NetworkUtilization { set; get; }
        [DataMember]
        public DateTime CurrentTime { set; get; }
        [DataMember]
        public string NetworkUsageInPercentage { set; get; }
    }
}
