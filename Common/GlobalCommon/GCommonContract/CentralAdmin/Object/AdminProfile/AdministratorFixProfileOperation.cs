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
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdministratorFixProfileOperation : AdministratorProfileReportOperation
    {

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FixProfileResult: ResultBase
    {
        [DataMember]
        public ProfileStatus Status { get; set; }
        [DataMember]
        public string Scope { get; set; }
        [DataMember]
        public string Name { get; set; }     //SharePoint Object Name
        [DataMember]
        public NodeLevel Level { get; set; }    //SharePoint Object Level
        [DataMember]
        public CAStringFormatMessage Comment { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public CustomAction CustomAction { get; set; }
    }

    public class FixProfileInfo
    {
        [DataMember]
        public ProfileStatus Status { get; set; }
        [DataMember]
        public string Scope { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public CustomAction CustomAction { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CustomAction
    {
        None,
        On,
        Off
    }
}
