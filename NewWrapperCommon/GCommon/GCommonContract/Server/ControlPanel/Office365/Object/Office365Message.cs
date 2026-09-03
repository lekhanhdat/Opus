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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365Message : AveMessage
    {
        [DataMember]
        public String WebAppDomain { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String DomainName { get; set; }
        [DataMember]
        public String SiteCollectionUrl { get; set; }
        //[DataMember]
        //public String WebAppUrl { get; set; }
        //[DataMember]
        //public SiteCollectionScanType ScanType { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Result
    {
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; } 
        [DataMember]
        public Boolean Status { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorInfo
    {
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        NoError = 0,
        [EnumMember]
        BadUrl = 1,
        [EnumMember]
        UnAuthorized = 2,
        [EnumMember]
        TimeOut = 3,
        [EnumMember]
        NotFound = 4,
    }
}
