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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPPermission : IProfileContent
    {
        [DataMember]
        [XmlAttribute("detail")]
        public SPPermissions SPPermissions { get; set; }

        #region For Client
        [DataMember]
        [XmlAttribute("id")]
        public string PermissionId { get; set; }

        [DataMember]
        [XmlAttribute("roleName")]
        public string PermissionName { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public string Description { get; set; }
        #endregion

    }

    /// <summary>
    /// 临时保存默认SP permission的设置
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DefaultPermisson
    {
        public static List<string> DefaultIds = new List<string>() { 
            "-1", "1073741924", "1073741826", "1073741827", "1073741828", "1073741829",
        };
        public static List<SPPermission> DefaultPerms = new List<SPPermission>() {
            new SPPermission
            {
                PermissionId = "-1",
                PermissionName = "None"
            },
            new SPPermission
            {
                PermissionId = "1073741924",
                PermissionName = "View Only"
            },
            new SPPermission
            {
                PermissionId = "1073741826",
                PermissionName = "Read"
            },
            new SPPermission
            {
                PermissionId = "1073741827",
                PermissionName = "Contribute"
            },
            new SPPermission
            {
                PermissionId = "1073741828",
                PermissionName = "Design"
            },
            new SPPermission
            {
                PermissionId = "1073741829",
                PermissionName = "Full Control"
            },
        };
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SPPermissions : ulong
    {
        [EnumMember]
        AddAndCustomizePages = 0x40000L,
        [EnumMember]
        AddDelPrivateWebParts = 0x10000000L,
        [EnumMember]
        AddListItems = 2L,
        [EnumMember]
        ApplyStyleSheets = 0x100000L,
        [EnumMember]
        ApplyThemeAndBorder = 0x80000L,
        [EnumMember]
        ApproveItems = 0x10L,
        [EnumMember]
        BrowseDirectories = 0x4000000L,
        [EnumMember]
        BrowseUserInfo = 0x8000000L,
        [EnumMember]
        CancelCheckout = 0x100L,
        [EnumMember]
        CreateAlerts = 0x8000000000L,
        [EnumMember]
        CreateGroups = 0x1000000L,
        [EnumMember]
        CreateSSCSite = 0x400000L,
        [EnumMember]
        DeleteListItems = 8L,
        [EnumMember]
        DeleteVersions = 0x80L,
        [EnumMember]
        EditListItems = 4L,
        [EnumMember]
        EditMyUserInfo = 0x10000000000L,
        [EnumMember]
        EmptyMask = 0L,
        [EnumMember]
        EnumeratePermissions = 0x4000000000000000L,
        [EnumMember]
        FullMask = 0x7fffffffffffffffL,
        [EnumMember]
        ManageAlerts = 0x4000000000L,
        [EnumMember]
        ManageLists = 0x800L,
        [EnumMember]
        ManagePermissions = 0x2000000L,
        [EnumMember]
        ManagePersonalViews = 0x200L,
        [EnumMember]
        ManageSubwebs = 0x800000L,
        [EnumMember]
        ManageWeb = 0x40000000L,
        [EnumMember]
        Open = 0x10000L,
        [EnumMember]
        OpenItems = 0x20L,
        [EnumMember]
        UpdatePersonalWebParts = 0x20000000L,
        [EnumMember]
        UseClientIntegration = 0x1000000000L,
        [EnumMember]
        UseRemoteAPIs = 0x2000000000L,
        [EnumMember]
        ViewFormPages = 0x1000L,
        [EnumMember]
        ViewListItems = 1L,
        [EnumMember]
        ViewPages = 0x20000L,
        [EnumMember]
        ViewUsageData = 0x200000L,
        [EnumMember]
        ViewVersions = 0x40L
    }

}
