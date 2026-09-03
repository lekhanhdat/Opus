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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPZone
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String Name { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPWebApp
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String Url { get; set; }
        [DataMember]
        public List<SPZone> ZoneList { get; set; }
        [DataMember]
        public String CurrentZoneName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CrossFirewallAccessZoneOperation : CAOperation
    {
        [DataMember]
        //The key stands for the URL of the WebApp
        public List<SPWebApp> WebApps { get; set; }
        [DataMember]
        public string SelectedWebAppURL { get; set; }
        [DataMember]
        public SPZone SelectedZone { get; set; }

    }
}
