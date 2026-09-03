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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPBackupDto
{
    public class SPItemMetadataDto
    {
        /// <summary>
        /// Old Doc Info, 重构之后将会去掉，请使用DocInfo
        /// </summary>
        public Dictionary<string, object> DocInfo_Old;

        //private AveDocInfo docInfo = null;
        ///// <summary>
        ///// 重构之后新的DocInfo，外围处理请使用这个, 还原使用
        ///// </summary>
        //public AveDocInfo DocInfo
        //{
        //    get
        //    {
        //        if (docInfo == null)
        //        {
        //            if (DocInfo_Old == null)
        //            {
        //                docInfo = new AveDocInfo();
        //            }
        //            else
        //            {
        //                docInfo = AveDataUpgradeUtil.UpgradeDocInfo(DocInfo_Old);
        //            }
        //        }
        //        return docInfo;
        //    }
        //    set
        //    {
        //        docInfo = value;
        //    }
        //}

        /// <summary>
        /// User Cache for metadata
        /// </summary>
        public AveUserList UserCache;

        /// <summary>
        /// Group Cache for metadata
        /// </summary>
        public AveGroupList GroupCache;   

        /// <summary>
        /// Column Info
        /// </summary>
        public Dictionary<string, object> UserDataInfo;

        /// <summary>
        /// Related Metadata info
        /// </summary>
        public List<AveTermStoreInfo> MetadataInfo;

        /// <summary>
        /// Data Junction
        /// </summary>
        public List<Dictionary<string, object>> DocDataJunction;

        /// <summary>
        /// ITEM tp_guid of lookup value
        /// </summary>
        public Dictionary<string, string> ItemTPGUIDofLookupValue;

        /// <summary>
        /// All Versions No. of Item
        /// </summary>
        public List<int> ItemUIVersionNums;
    }

    public class SPFolderMetadataDto : SPItemMetadataDto
    {
 
    }

    public class SPDocumentMetadataDto : SPItemMetadataDto
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
        /// 标识要还原的doc是否是一个View Page
        /// </summary>
        public bool IsView { get; set; }

    }

    public class SPListItemMetadataDto : SPItemMetadataDto
    {
        private List<SPDocumentMetadataDto> AttachmentsInfo { get; set; }
    }

    public class SPAttachmentMetadataDto
    {
        /// <summary>
        /// Real Name of Attachment
        /// </summary>
        public string Name { get; set; }
    }

}
