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
    using System.Text;

    #endregion using directives

    [Table(IndexConstants.TableNameGeneralJobInfo)]
    public class GeneralJobInfoIndex
        : IndexBase
        , IIndexable
    {
        [Column("COL_GUID")]
        public String Guid { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }

        [Column("COL_KEY")]
        public String Key { get; set; }

        [Column("COL_VALUE")]
        public String Value { get; set; }

        [Column("COL_EXTENSION_1")]
        public Int32 Extension1 { get; set; }

        [Column("COL_EXTENSION_2")]
        public Int32 Extension2 { get; set; }

        [Column("COL_EXTENSION_3")]
        public Int32 Extension3 { get; set; }

        [Column("COL_EXTENSION_4")]
        public Int64 Extension4 { get; set; }

        [Column("COL_EXTENSION_5")]
        public Int64 Extension5 { get; set; }

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

        public override Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<String, Object>();
            result.Add("@COL_GUID", Guid);
            result.Add("@COL_JOB_ID", JobId);
            result.Add("@COL_KEY", Key);
            result.Add("@COL_VALUE", Value);
            result.Add("@COL_EXTENSION_1", Extension1);
            result.Add("@COL_EXTENSION_2", Extension2);
            result.Add("@COL_EXTENSION_3", Extension3);
            result.Add("@COL_EXTENSION_4", Extension4);
            result.Add("@COL_EXTENSION_5", Extension5);
            result.Add("@COL_EXTENSION_6", Extension6);
            result.Add("@COL_EXTENSION_7", Extension7);
            result.Add("@COL_EXTENSION_8", Extension8);
            result.Add("@COL_EXTENSION_9", Extension9);
            result.Add("@COL_EXTENSION_10", Extension10);
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GeneralJobInfoIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" Key: ");
            sb.Append(this.Key);
            sb.Append(" Value: ");
            sb.Append(this.Value);
            return sb.ToString();
        }
    }
}