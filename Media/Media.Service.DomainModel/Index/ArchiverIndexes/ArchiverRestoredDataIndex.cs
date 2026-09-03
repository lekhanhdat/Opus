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
using AvePoint.Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Service.DomainModel.Index.ArchiverIndexes
{
    [Serializable]
    public class ArchiverRestoredDataIndex : IIndexable
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("SiteId")]
        public string SiteId { get; set; }

        [Column("JobId")]
        public string JobId { get; set; }

        [Column("StoragePath")]
        public string StoragePath { get; set; }

        [Column("COL_ID")]
        public string BasicIndexId { get; set; }

        [Column("ItemPathMd5")]
        public string ItemPathMd5 { get; set; }

        [Column("RestoreSetting")]
        public string RestorSetting { get; set; }

        [Column("CleanRestoredOption")]
        public string CleanRestoredOption { get; set; }

        [Column("RestoredSiteUrl")]
        public string RestoredSiteUrl { get; set; }

        [Column("RestoredUrl")]
        public string RestoredUrl { get; set; }

        [Column("RestoredTimeTicks")]
        public long RestoredTimeTicks { get; set; }

        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            return new()
            {
                {"id", Id },
                {"SiteId", SiteId },
                {"JobId", JobId },
                {"StoragePath", StoragePath},
                {"COL_ID", BasicIndexId},
                {"ItemPathMd5", ItemPathMd5 },
                {"RestoreSetting", RestorSetting},
                {"CleanRestoredOption", CleanRestoredOption},
                {"RestoredUrl", RestoredUrl },
                {"RestoredTimeTicks", RestoredTimeTicks }
            };
        }
    }
}
