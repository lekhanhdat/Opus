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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Discovery
{
    public interface IAveDiscoverList : IAveDiscoverFilterBase, IDisposable
    {
        ChangeType ChangeType { get; set; }
        ChangeType AlertChangeType { get; }
        ChangeType RoleAssignmentsChangeType { get; }
        List<AveSecurityObject> DeleteRoleAssignments { get; set; }
        object Flag { get; set; }
        Dictionary<Guid, AveAlertObject> GetChangeAlerts();
        Dictionary<byte[], AveContentTypeObject> GetChangeListContentTypes();
        IAveDiscoverFolder GetChangeRootFolder(List<AveDiscoverExtraItemBaseInfo> extraItems = null);
        List<AveSecurityObject> GetChangeSecuritys();
        Dictionary<Guid, AveViewObject> GetChangeViews();
        long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl);
        IAveList GetListObject();
        long GetListSize(Guid siteId, Guid webId, Guid listId);
        long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderUrl, DateTime beginTime);
        IAveDiscoverFolder GetRootFolder();
        /// <summary>
        /// 调用此方法后，会query List下所有的object，包括Version和Attachment。
        /// 1.只有真实365有实现。2.传进来的Folder必须是root folder。
        /// </summary>
        /// <param name="listRootFolder"></param>
        /// <param name="includeRecycleBin"></param>
        /// <param name="discoverStubOption"></param>
        /// <param name="maxItemCount">当List中的ItemCount超出此限制，则不query所有content，防止内存问题。</param>
        /// <param name="includeSystemFolder"></param>
        void DiscoverAllListContent(IAveDiscoverFolder listRootFolder, bool includeRecycleBin, DiscoverStubOption discoverStubOption, int maxItemCount = 50000, bool includeSystemFolder = false);
        IAveDiscoverFolder GetVirtualSystemFolder(string leafName, string fullUrl);
        Dictionary<Guid, AveViewObject> GetViews();
        bool? Hidden { get; set; }
        Guid ListId { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedTime { get; set; }
        string Name { get; set; }
        Guid RootFolderId { get; set; }
        string RootFolderUrl { get; set; }
        int? ServerTemplate { get; set; }
        string Title { get; set; }
        int Type { get; set; }
        byte[] DeleteTransactionId { get; set; }
    }
}
