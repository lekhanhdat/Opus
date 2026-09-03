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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationUserPolicyOperation : CAOperation
    {
        [DataMember]
        public List<WebAppPolicyUser> Users { get; set; }

        [DataMember]
        public List<PolicyRole> AllPermissions { get; set; }

        [DataMember]
        public List<SharePointUrlZone> Zones { get; set; }

        [DataMember]
        public string Url { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebAppPolicyUser
    {
        [DataMember]
        public UserDetail User { get; set; }

        [DataMember]
        public bool IsSystemUser { get; set; }

        [DataMember]
        public List<PolicyRole> Permissions { get; set; }

        [DataMember]
        public bool AllUrlZones { get; set; }

        [DataMember]
        public SharePointUrlZone UrlZone { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PolicyRole
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Description { get; set; }

        public bool IsChecked { get; set; } //Only used in GUI page.

        public bool IsEnabled { get; set; } //Only used in GUI page.
    }
}
