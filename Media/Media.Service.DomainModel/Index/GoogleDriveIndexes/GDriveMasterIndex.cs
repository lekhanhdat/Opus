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

    #endregion

    [Table(IndexConstants.TableNameGDriveMaster)]
    public class GDriveMasterIndex
          : IndexBase
    {
        [Column("COL_ID")]
        public String ID { get; set; }

        [Column("COL_BACKUP_TIME")]
        public long BackupTime { get; set; }

        [Column("COL_TENANT_ID")]
        public String TenantId { get; set; }

        [Column("COL_DRIVE_NAME")]
        public String DriveName { get; set; }

        [Column("COL_DRIVE_ID")]
        public String DriveId { get; set; }

        [Column("COL_DRIVE_TYPE")]
        public int DriveType { get; set; }

        [Column("COL_JOB_ID")]
        public String JobId { get; set; }

        [Column("COL_PLAN_ID")]
        public String PlanId { get; set; }

        [Column("COL_LOGICAL_DRIVE")]
        public String LogicalDrive { get; set; }

        [Column("COL_MAX_DATA_BLOCK_SIZE")]
        public int MaxDataBlockSize { get; set; }
        

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GDriveMasterIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" PlanId: ");
            sb.Append(this.PlanId);
            sb.Append(" DriveName: ");
            sb.Append(this.DriveName);
            return sb.ToString();
        }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();
            dic.Add("@COL_ID", this.ID);
            dic.Add("@COL_BACKUP_TIME", this.BackupTime);
            dic.Add("@COL_TENANT_ID", this.TenantId);
            dic.Add("@COL_DRIVE_NAME", this.DriveName);
            dic.Add("@COL_DRIVE_ID", this.DriveId);
            dic.Add("@COL_JOB_ID", this.JobId);
            dic.Add("@COL_PLAN_ID", this.PlanId);
            dic.Add("@COL_LOGICAL_DRIVE", this.LogicalDrive);
            dic.Add("@COL_MAX_DATA_BLOCK_SIZE", this.MaxDataBlockSize);
            dic.Add("@COL_DRIVE_TYPE", this.DriveType);
            return dic;
        }
    }
}