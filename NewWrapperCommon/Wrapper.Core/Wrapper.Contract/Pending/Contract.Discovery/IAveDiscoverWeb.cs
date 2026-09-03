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
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.Wrapper.Discovery
{
    public interface IAveDiscoverWeb: IAveDiscoverFilterBase,IDisposable
    {
        IAveWeb AveWeb { get; }
        ChangeType ChangeType { get; set; }
        ChangeType ColumnChangeType { get; }
        ChangeType ContentTypeChangeType { get; }
        ChangeType NavigationChangeType { get; }
        ChangeType PermissionLevelChangeType { get; }
        ChangeType RoleAssignmentsChangeType { get; }
        List<AveSecurityObject> DeleteSecurities { get; set; }
        DateTime EventTime { get; set; }
        string FullUrl { get; set; }
        Guid AppInstanceId { get; set; }
        Dictionary<Guid, IAveDiscoverList> GetChangeLists();
        List<AveSecurityObject> GetChangeSecurityChanges();
        Dictionary<byte[], AveContentTypeObject> GetContentTypes();
        Dictionary<Guid, IAveDiscoverList> GetLists();
        Dictionary<Guid, IAveDiscoverList> GetLists(bool includeRecycleBin);
        Dictionary<Guid, IAveDiscoverList> GetLists(bool includeRecycleBin, bool needSortByDependency);
        IAveDiscoverFolder GetRootFolder();
        Dictionary<Guid, IAveDiscoverWeb> GetSubWebs();
        Dictionary<Guid, IAveDiscoverWeb> GetSubWebs(bool includeRecycleBin);
        Dictionary<Guid, IAveDiscoverAppDefinition> GetAppDefinitions();
        Dictionary<Guid, IAveDiscoverWeb> GetAppWebs();
        Guid GetAppInstanceIDByProductID(Guid productId);
        long GetWebSize(Guid siteId, Guid webId);
        string Name { get; set; }
        bool NavigationChanged { get; set; }
        string Title { get; set; }
        Guid WebID { get; set; }
        byte[] DeleteTransactionId { get; set; }
    }

    public interface IAveDiscoverAppDefinition
    {
        Guid ProductId { get; set; }
        string Name { get; set; }
        string AppFullUrl { get; set; }
        Guid InstanceId { get; set; }
        bool IsUpdateAvailable { get; set; }
        string VersionString { get; set; }
    }
}
