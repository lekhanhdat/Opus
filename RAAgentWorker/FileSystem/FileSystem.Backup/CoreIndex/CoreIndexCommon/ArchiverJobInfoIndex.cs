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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    #endregion

    [Table(IndexConstants.TableNameArchiveJobInfo)]
    public class ArchiverJobInfoIndex
          : IndexBase
    {
        [Column("COL_GUID")]
        public String Guid { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }

        [Column("COL_KEY")]
        public String Key { get; set; }

        [Column("COL_VALUE")]
        public String Value { get; set; }

        [Column("COL_EXTENSION_3")]
        public int Extension3 { get; set; }

        [Column("COL_EXTENSION_4")]
        public long Extension4 { get; set; }

        [Column("COL_EXTENSION_5")]
        public long Extension5 { get; set; }

        [Column("COL_EXTENSION_6")]
        public String Extension6 { get; set; }

        [Column("COL_EXTENSION_7")]
        public String Extension7 { get; set; }

        [Column("COL_EXTENSION_8")]
        public String Extension8 { get; set; }

        [Column("COL_EXTENSION_9")]
        public String Extension9 { get; set; }

        [Column("COL_EXTENSION_10")]
        public String Extension10 { get; set; }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverJobInfoIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" Key: ");
            sb.Append(this.Key);
            sb.Append(" Value: ");
            sb.Append(this.Value);
            return sb.ToString();
        }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();
            dic.Add("@COL_GUID", this.Guid);
            dic.Add("@COL_JOB_ID", this.JobId);
            dic.Add("@COL_KEY", this.Key);
            dic.Add("@COL_VALUE", this.Value);
            dic.Add("@COL_EXTENSION_3", this.Extension3);
            dic.Add("@COL_EXTENSION_4", this.Extension4);
            dic.Add("@COL_EXTENSION_5", this.Extension5);
            dic.Add("@COL_EXTENSION_6", this.Extension6);
            dic.Add("@COL_EXTENSION_7", this.Extension7);
            dic.Add("@COL_EXTENSION_8", this.Extension8);
            dic.Add("@COL_EXTENSION_9", this.Extension9);
            dic.Add("@COL_EXTENSION_10", this.Extension10);
            return dic;
        }
    }
}