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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Restore.Core
{
    internal class SPItemMetadataImportDto
    {
        /// <summary>
        /// Doc Info
        /// </summary>
        public AveDocInfo DocInfo { get; set; }

        /// <summary>
        /// TODO: AllUserData + AllUserDataJuncInfo + Lookup
        /// Column Info
        /// </summary>
        public Dictionary<string, AveFieldValueInfo> ColumnValueInfos { get; set; }
    }

    internal class SPDocumentMetadataImportDto : SPItemMetadataImportDto
    {
        /// <summary>
        /// Storage Info for SP2010, SP2007
        /// </summary>
        public AveStorageInfo StorageInfo { get; set; }

        /// <summary>
        /// Storage Info for SP2013  看看把10和13继承一个类，然后使用基类来存储。
        /// </summary>
        public AveStorageInfo13 StorageInfo13 { get; set; }

        /// <summary>
        /// WebParts
        /// </summary>
        public List<AveWebPartBaseInfo> WebParts { get; set; }

        /// <summary>
        /// View 信息
        /// </summary>
        public List<AveViewInfo> ViewInfos { get; set; }

        /// <summary>
        /// 是否是View
        /// </summary>
        public bool IsView { get; set; }

        /// <summary>
        /// stream of file content
        /// </summary>
        public IAveRestoreStream ContentStream { get; set; }
    }

    internal class SPListItemMetadataImportDto : SPItemMetadataImportDto
    {
        private List<SPDocumentMetadataImportDto> AttachmentsInfo { get; set; }
    }
}
