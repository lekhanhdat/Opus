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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    /// <summary>
    /// add user
    /// </summary>
    [KnownType(typeof(CASecurityGrantTemporaryPermissionOperation))]
    [KnownType(typeof(CASecurityViewTemporaryPermissionOperation))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityUsersOperation : CAOperation
    {
        /// <summary>
        /// Only For Farm Level
        /// </summary>
        [DataMember]
        public string CAFarmSiteURL { get; set; }
        [DataMember]
        public List<UserDetail> CheckUserDetails { get; set; }

        [DataMember]
        public List<CAUserInfo> Users { get; set; }

        [DataMember]
        public List<CAGroupInfo> Groups { get; set; }

        [DataMember]
        public List<CAPermissionInfo> Permissions { get; set; }

        [DataMember]
        public CAEmailInfo EmailInfo { get; set; }

        [DataMember]
        public bool SendEmail { get; set; }

        [DataMember]
        public bool IsAddInGroup { get; set; }

        [DataMember]
        public bool ShowEveryoneClaim { get; set; } //SPO Tenant中控制显示Everyone group的开关

        [DataMember]
        public bool ShowEveryoneExceptExternalUsersClaim { get; set; } //SPO Tenant中控制显示Everyone Except External Users group的开关

        [DataMember]
        public bool ShowAllUsersClaim { get; set; } //SPO Tenant中控制显示AllUsers(membership)和AllUsers(windows) groups的开关

        [DataMember]
        public bool AddAllUsersMembership { get; set; } //界面中勾选了add AllUsers(membership)的checkbox

        [DataMember]
        public bool AddAllUsersWindows { get; set; } //界面中勾选了add AllUsers(windows)的checkbox

        [DataMember]
        public bool AddEveryone { get; set; } //界面中勾选了add Everyone的checkbox

        [DataMember]
        public bool AddEveryoneExceptExternalUsers { get; set; } //界面中勾选了add Everyone Except External Users的checkbox
    }
}
