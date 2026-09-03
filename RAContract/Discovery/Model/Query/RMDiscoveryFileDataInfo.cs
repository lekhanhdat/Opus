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

namespace AvePoint.RA.Contract.Discovery.Model.Query
{
    public class RMDiscoveryFileDataInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string SiteUrl { get; set; }

        public string FullUrl { get; set; }

        public string FolderRelativeUrl { get; set; }

        public string SiteId { get; set; }

        public string WebId { get; set; }

        public string ListId { get; set; }

        public string FolderId { get; set; }

        public int ItemId { get; set; }

        public string ItemUniqueId { get; set; }

        public string FileExtension { get; set; }

        public long FileSize { get; set; }

        public string CurrentVersion { get; set; }

        public long HistoryVersionsCount { get; set; }

        public long HistoryVersionsSize { get; set; }

        public long AuthorId { get; set; }

        public long EditorId { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public List<RMDiscoveryFileVersionDataInfo> Versions { get; set; }

        public Dictionary<string, object> Tags { get; set; }
    }

    public class RMDiscoveryFileVersionDataInfo
    {
        public string Version { get; set; }

        public long VersionSize { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        public string FileValue { get; set; }
    }
}
