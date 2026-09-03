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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ArchivedFullTextIndex
{
    public class RMArchivedFullTextIndexDataInfo
    {
        public int IntId { get; set; }

        public string IndexDBUniqueId { get; set; }

        public string SiteId { get; set; }

        public string Name { get; set; }

        public long FileSize { get; set; }

        public string FileType { get; set; }
        
        public string SiteUrl { get; set; }

        public string FullPath { get; set; }

        public string FriendlyFullPath { get; set; }

        public string PathMd5 { get; set; }

        public string ParentPathMd5 { get; set; }

        public string Author { get; set; }

        public string Editor { get; set; }

        public int NodeLevel { get; set; }

        public string Content { get; set; }

        public bool IsCurrentVersion { get; set; }

        public long UIVersion { get; set; }

        public long ArchiverTime { get; set; }

        public long CreateTime { get; set; }

        public long ModifiedTime { get; set; }

        public string TreeNode { get; set; }

        public string ArchiverJobId { get; set; }

        public int AccessTierType { get; set; }

        public string TypeInIndex { get; set; }

        public string MetadataInfo { get; set; }
    }
}
