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

    [Table(IndexConstants.TableNameGeneralSiteMaster)]
    public class GeneralSiteMasterIndex
        : IndexBase
        , IIndexable
    {
        [Column("COL_ID")]
        public String ID { get; set; }

        [Column("COL_PLAN_ID")]
        public String PlanId { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }

        [Column("COL_LOGICAL_DRIVE")]
        public String LogicalDrive { get; set; }

        [Column("COL_BACKUP_TIME")]
        public Int64 BackupTime { get; set; }

        [Column("COL_SP_VERSION")]
        public Int64 SpVersion { get; set; }

        [Column("COL_MAX_DATA_BLOCK_SIZE")]
        public Int64 MaxDataBlockSize { get; set; }

        [Column("COL_REMARK1")]
        public String Remark1 { get; set; }

        [Column("COL_REMARK2")]
        public String ClipId { get; set; }

        [Column("COL_REMARK3")]
        public String Remark3 { get; set; }

        [Column("COL_REMARK4")]
        public String Remark4 { get; set; }

        [Column("COL_REMARK5")]
        public Int64 ModifyData { get; set; }

        [Column("COL_REMARK6")]
        public Int64 Remark6 { get; set; }

        [Column("COL_REMARK7")]
        public String Remark7 { get; set; }

        [Column("COL_REMARK8")]
        public String Remark8 { get; set; }

        [Column("COL_REMARK9")]
        public String Remark9 { get; set; }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<String, Object>();
            result.Add("@COL_ID", ID);
            result.Add("@COL_JOB_ID", JobId);
            result.Add("@COL_PLAN_ID", PlanId);
            result.Add("@COL_LOGICAL_DRIVE", LogicalDrive);
            result.Add("@COL_BACKUP_TIME", BackupTime);
            result.Add("@COL_MAX_DATA_BLOCK_SIZE", MaxDataBlockSize);
            result.Add("@COL_SP_VERSION", SpVersion);
            result.Add("@COL_REMARK1", Remark1);
            result.Add("@COL_REMARK2", ClipId);
            result.Add("@COL_REMARK3", Remark3);
            result.Add("@COL_REMARK4", Remark4);
            result.Add("@COL_REMARK5", ModifyData);
            result.Add("@COL_REMARK6", Remark6);
            result.Add("@COL_REMARK7", Remark7);
            result.Add("@COL_REMARK8", Remark8);
            result.Add("@COL_REMARK9", Remark9);
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GeneralSiteMasterIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" PlanId: ");
            sb.Append(this.PlanId);
            sb.Append(" BackupTime: ");
            sb.Append(this.BackupTime);
            return sb.ToString();
        }
    }
}