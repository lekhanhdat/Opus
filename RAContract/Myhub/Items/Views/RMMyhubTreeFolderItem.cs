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

namespace AvePoint.RA.Contract.MyHub.Items.Views
{
    // 此处代表返回值为每个文件夹的基本信息
    public class RMMyhubTreeFolderItem : RMMyhubFolderAndFileItem
    {
        public Guid ParentId { get; set; }
        public bool HasChildren { get; set; }
    }

    // 此处代表返回值为根节点和展开文件夹结果
    public class RMMyhubTreeFolderItemResult
    {
        public List<RMMyhubTreeFolderItem> Items { get; set; } = [];
        public bool HasMore { get; set; }
        public string ContinuationToken { get; set; }
    }
}
