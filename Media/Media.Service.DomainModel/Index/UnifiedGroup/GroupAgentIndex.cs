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


namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System.Collections.Generic;

    #endregion

    [Table(IndexConstants.TableNameExchangeAgent)]
    public class GroupAgentIndex : IIndexable
    {
        [Column("COL_LAST_BACKUP_CONTENT")]
        public string LastBackUpContent { get; set; }

        [Column("COL_TIME_STAMP")]
        public string TimeStamp { get; set; }

        [Column("COL_JOB_ID")]
        public string JobId { get; set; }

        [Column("COL_INDEX_TYPE")]
        public string IndexType { get; set; }

        [Column("COL_STORAGEINFO")]
        public string StorageInfo { get; set; }

        [Column("COL_EXTENSION_1")]
        public long Extension1 { get; set; }

        [Column("COL_EXTENSION_2")]
        public string Extension2 { get; set; }

        [Column("COL_EXTENSION_3")]
        public string Extension3 { get; set; }

        [Column("COL_CURRENT_JOB_ID")]
        public string CurrentJobId { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<string, object>();
            result.Add("@COL_LAST_BACKUP_CONTENT", LastBackUpContent);
            result.Add("@COL_JOB_ID", JobId);
            result.Add("@COL_INDEX_TYPE", IndexType);
            result.Add("@COL_TIME_STAMP", TimeStamp);
            result.Add("@COL_STORAGEINFO", StorageInfo);
            result.Add("@COL_EXTENSION_1", Extension1);
            result.Add("@COL_EXTENSION_2", Extension2);
            result.Add("@COL_EXTENSION_3", Extension3);
            result.Add("COL_CURRENT_JOB_ID", CurrentJobId);
            return result;
        }
    }
}