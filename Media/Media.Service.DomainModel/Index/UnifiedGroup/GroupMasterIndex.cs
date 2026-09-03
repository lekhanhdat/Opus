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

    [Table(IndexConstants.TableNameExchangeSiteMaster)]
    public class GroupMasterIndex :
        IIndexable
    {
        [Column("COL_LOGICAL_DEVICE_ID")]
        public string LogicalDrive { get; set; }

        [Column("COL_BACKUP_PLAN_TYPE")]
        public int BackupPlanType { get; set; }

        [Column("COL_BACKUP_TIME")]
        public long BackupTime { get; set; }

        [Column("COL_USER_ADDRESS")]
        public string Useraddress { get; set; }

        [Column("COL_MAX_DATA_BLOCK_SIZE")]
        public int MaxDataBlockSize { get; set; }

        [Column("COL_ENCRYPTION_INFO")]
        public string EncryptionInfo { get; set; }

        [Column("COL_GROUP_NAME")]
        public string GroupName { get; set; }

        [Column("COL_ID")]
        public string Id { get; set; }

        [Column("COL_CYCLE_ID")]
        public string CycleId { get; set; }

        [Column("COL_JOB_ID")]
        public string JobId { get; set; }

        [Column("COL_PLAN_ID")]
        public string PlanId { get; set; }

        [Column("COL_MODIFY_DATA")]
        public long ModifyData { get; set; }

        [Column("COL_CURRENT_JOB_ID")]
        public string CurrentJobId { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<string, object>();
            result.Add("@COL_LOGICAL_DEVICE_ID", LogicalDrive);
            result.Add("@COL_BACKUP_PLAN_TYPE", BackupPlanType);
            result.Add("@COL_BACKUP_TIME", BackupTime);
            result.Add("@COL_USER_ADDRESS", Useraddress);
            result.Add("@COL_MAX_DATA_BLOCK_SIZE", MaxDataBlockSize);
            result.Add("@COL_ENCRYPTION_INFO", EncryptionInfo);
            result.Add("@COL_GROUP_NAME", GroupName);
            result.Add("@COL_ID", Id);
            result.Add("@COL_CYCLE_ID", CycleId);
            result.Add("@COL_JOB_ID", JobId);
            result.Add("@COL_PLAN_ID", PlanId);
            result.Add("@COL_MODIFY_DATA", ModifyData);
            result.Add("COL_CURRENT_JOB_ID", CurrentJobId);
            return result;
        }

    }
}