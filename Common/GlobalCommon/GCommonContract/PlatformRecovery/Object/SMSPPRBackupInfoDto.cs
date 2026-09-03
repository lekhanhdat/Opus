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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNBackupInfoDto
    {
        #region Verify Backup
        [DataMember]
        public bool IsVerifyBackup { get; set; }

        [DataMember]
        public SQLInstanceConfig VerifyServer { get; set; }

        [DataMember]
        public string MountPointForVerify { get; set; }
        #endregion

        #region Check Old Backups to Be Deleted
        [DataMember]
        public bool IsCheckOldBackups { get; set; }
        #endregion

        #region Transaction Log Backup
        [DataMember]
        public bool IsTransactionLogBackup { get; set; }
        #endregion

        #region Snap Mirror
        [DataMember]
        public bool IsUpdateMirror { get; set; }

        [DataMember]
        public bool IsVerifyDestMirror { get; set; }

        [DataMember]
        public bool IsUpdateDeviceMirror { get; set; }
        #endregion

        #region Snap Vault
        [DataMember]
        public bool IsArchiveBackup { get; set; }

        [DataMember]
        public bool IsVerifyArchive { get; set; }

        [DataMember]
        public ManagementGroup Group { get; set; }
        #endregion

        #region Scripts
        [DataMember]
        public bool IsRunScript { get; set; }

        [DataMember]
        public string ScriptLocation { get; set; }
        #endregion

        #region Restore Granularity Level
        [DataMember]
        public PRBackupLevel PRBackupLevel { get; set; }

        [DataMember]
        public bool IsDeferIndexing { get; set; }

        [DataMember]
        public SQLInstanceConfig IndexServer { get; set; }

        [DataMember]
        public string MountPointForIndex { get; set; }
        #endregion

        #region MaintenanceOption
        [DataMember]
        public PRSNMaintenanceOptionDto MaintenanceOption { get; set; }
        #endregion
    }
}
