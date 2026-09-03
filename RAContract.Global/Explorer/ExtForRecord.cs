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

namespace AvePoint.RA.Contract.Explorer
{
    public class ExtForRecord
    {
        public Guid ScopeId { get; set; }
        public string DirPath { get; set; }
        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public Guid FolderId { get; set; }

        public int ItemRowId { get; set; }

        public string FullPath { get; set; }

        public string MetaInfo { get; set; }
    }
    /// <summary>
    /// 在DAO中也使用了这个类，如果要添加或者修改属性，请查看DAO中是否也需要修改
    /// </summary>
    public class RecordMetaInfo
    {
        public long FileSize { get; set; }

        public List<string> AttachmentNames { get; set; }

        public Dictionary<string, object> Fields { get; set; }

        public long LastAccessTime { get; set; }

        public string Owner { get; set; }

        public int DataStatus { get; set; }

        public string BackUpJobId { get; set; }

        public string PathMD5 { get; set; }

        public string ArchiverIndex { get; set; }

        public bool IsSyncFailed { get; set; }

        public string LocalFullPath { get; set; }
        public long LastModifiedTime { get; set; }
        public long CreatedTime { get; set; }
        public string FileTypeName { get; set; }
    }

    public enum DataStatus
    {
        None = 0,
        Moved = 1
    }
}

