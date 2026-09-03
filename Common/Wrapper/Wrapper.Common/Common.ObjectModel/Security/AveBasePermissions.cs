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

namespace AvePoint.Wrapper.Common
{
    public enum AveBasePermissions : ulong
    {
        EmptyMask = 0,
        ViewListItems = 1,
        AddListItems = 2,
        EditListItems = 4,
        DeleteListItems = 8,
        ApproveItems = 16,
        OpenItems = 32,
        ViewVersions = 64,
        DeleteVersions = 128,
        CancelCheckout = 256,
        ManagePersonalViews = 512,
        ManageLists = 2048,
        ViewFormPages = 4096,
        AnonymousSearchAccessList = 8192,
        Review = 16384, // "can review" permissions
        Open = 65536,
        ViewPages = 131072,
        AddAndCustomizePages = 262144,
        ApplyThemeAndBorder = 524288,
        ApplyStyleSheets = 1048576,
        ViewUsageData = 2097152,
        CreateSSCSite = 4194304,
        ManageSubwebs = 8388608,
        CreateGroups = 16777216,
        ManagePermissions = 33554432,
        BrowseDirectories = 67108864,
        BrowseUserInfo = 134217728,
        AddDelPrivateWebParts = 268435456,
        UpdatePersonalWebParts = 536870912,
        ManageWeb = 1073741824,
        AnonymousSearchAccessWebLists = 2147483648,
        UseClientIntegration = 68719476736,
        UseRemoteAPIs = 137438953472,
        ManageAlerts = 274877906944,
        CreateAlerts = 549755813888,
        EditMyUserInfo = 1099511627776,
        EnumeratePermissions = 4611686018427387904,
        FullMask = 9223372036854775807,
    }
}
