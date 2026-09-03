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
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityCloneUserPermissionsOperation:CAOperation
    {
        [DataMember]
        public UserDetail SourceUser { get; set; }
        [DataMember]
        public string SourceSPGroup { get; set; }
        /// <summary>
        /// dest user is a list, a single source user can transfer permissions to mutiple users
        /// </summary>
        [DataMember]
        public List<UserDetail> DestUsers { get; set; }
        [DataMember]
        public string DestSPGroups { get; set; }

        [DataMember]
        public TransferOption TransferOption { get; set; }

        [DataMember]
        public bool IncludeAlerts { get; set; }

        [DataMember]
        public List<string> Matadates { get; set; }

        [DataMember]
        public bool IncludeInheritedFromGroup { get; set; }

        [DataMember]
        public bool AddToSameGroupsInDestination { get; set; }

        [DataMember]
        public bool RemoveSourceUserPermissions { get; set; }

        [DataMember]
        public bool DeleteSourceUserFromSPGroup { get; set; }

        [DataMember]
        public bool DeleteSourceUserFromSiteCollection { get; set; }

        [DataMember]
        public bool IncludeDetailsReport { get; set; }

        //filter
        [DataMember]
        public FilterPolicyInfo FilterPolicyInfo { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TransferOption
    {
        [EnumMember]
        Replace,

        [EnumMember]
        Append,

        [EnumMember]
        Skip
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CloneDetailResult : ResultBase
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Level { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public ClonePermissionUserInfo SourceUserInfo { get; set; }

        [DataMember]
        public Dictionary<string, ClonePermissionUserInfo> DestUserInfos { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ClonePermissionUserInfo
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string UserOriginalPerms { get; set; }

        [DataMember]
        public string UserCurrentPerms { get; set; }

        [DataMember]
        public ResultStatus Status { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }

    /// <summary>
    /// 提供给Server使用的类
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ClonePermissionJobReportItem
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Level { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string SourceUserName { get; set; }

        [DataMember]
        public string SourceUserPerms { get; set; }

        [DataMember]
        public string DestUserName { get; set; }

        [DataMember]
        public string DestUserOriginalPerms { get; set; }

        [DataMember]
        public string DestUserCurrentPerms { get; set; }

        [DataMember]
        public ResultStatus Status { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }
}
