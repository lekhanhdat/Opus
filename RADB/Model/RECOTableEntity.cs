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
using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Model
{
    public class RECOTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public RECOTableEntity()
        {
        }
        public RECOTableEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }


        public string FullPath { set; get; }
        public int NodeType { set; get; }
        public Guid SiteID { set; get; }
        public Guid WebID { set; get; }
        public Guid ListID { set; get; }
        public Guid FolderID { set; get; }
        //public int folderRowId { set; get; }//parent folder id
        public int ItemRowId { set; get; }//current item id
        public Guid ItemID { set; get; }
        public Guid TermSetID { set; get; }
        public Guid TermID { set; get; }
        public string TermName { set; get; }
        public long CreatedTime { get; set; }
        public long CollectionTime { set; get; } 
        public int IsInActive { get; set; } 
        //public string JsonMeta { get; set; }
        public int LifecycleStatus { get; set; }
        public long DestroyedTime { get; set; }
    }

    public class RECOTableJsonMetaDto
    {
        public string SiteTitle { get; set; }
    }
}
