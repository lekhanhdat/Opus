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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.Wrapper.Common;
namespace AvePoint.Wrapper.Discovery
{
    public interface IAveDiscoverItem : IAveDiscoverFilterBase, IAveDiscoverObjectInfo, IDisposable
    {
        IAveListItem CurrentItem { get; }
        bool RealFilterVersion { get; }
        string LeafName { get; set; }
        DateTime TimeLastModified { get; set; }

        [Obsolete("It will delete soon")]
        List<AveItemObject> GetAttachments();
        [Obsolete("It will delete soon")]
        List<AveItemObject> GetAttachments(DiscoverStubOption discoverStubOption);

        List<AveItemObject> GetAttachmentsForIB();
        List<AveItemObject> GetAttachmentsForIB(DiscoverStubOption discoverStubOption);
        List<AveItemObject> GetAttachmentsForFB();
        List<AveItemObject> GetAttachmentsForFB(DiscoverStubOption discoverStubOption);

        List<AveItemObject> GetAttachmentsForRP(Guid siteId, string listRootUrl);
        List<AveAlertObject> GetChangeAlerts();
        List<AveSecurityObject> GetChangeSecuritys();
        int GetCurrentUIVersion();
        ObjectInfoBase GetFilterAttachmentInfo(List<FilterPolicy> policies, string attachementName);
        ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies);
        List<AveItemObject> GetStubAttachments();
        List<AveVersionObject> GetStubVersions();
        ObjectInfoBase GetVersionObjectInfo(List<FilterPolicy> policies, int UIVersion);
        List<AveVersionObject> GetVersions();
        List<AveVersionObject> GetVersions(DiscoverStubOption discoverStubOption);
        bool IsAttachmentQualified(string attachmentName);
        bool IsVersionQualified(int uiVersion, bool needFilterDeletedVersions = true);
    }
}
