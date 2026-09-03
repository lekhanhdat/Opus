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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AutoScanProfileContent : IProfileContent
    {
        [DataMember]
        public List<AutoScanProfileItem> ProfileItems { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AutoScanProfileItem
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public string GroupId { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public bool IncludeArchiveMailbox { get; set; }
        [DataMember]
        public bool IncludeResourceMailbox { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AutoRegistrationProfileContent : IProfileContent
    {
        [DataMember]
        public AutoRegistrationConflict GroupConflict { get; set; }
        [DataMember]
        public AutoRegistrationConflict ContainerConflict { get; set; }
        [DataMember]
        public AutoRegistrationConflict ContentConflict { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AutoRegistrationConflict : int
    {
        [EnumMember]
        Skip = 0,
        [EnumMember]
        Merge = 1,
        [EnumMember]
        Remove = 2,
        [EnumMember]
        Replace = 3,
    }
}
