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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.FileSystem.Backup
{
    public class FSArchiverBackupRequest
    {
        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public DataEncryptionInfo EncryptionInfo { get; set; }

        /// <summary>
        /// Used to encrypt\decrypt index file. It can also be used to encrypt\decrypt file whoes lifecycle is the whole backup cycle
        /// </summary>
        [DataMember]
        public DataEncryptionInfoWrapper IndexEncryptionInfoWrapper { get; set; }
        /// <summary>
        /// 改用新的加密方式后，media不再使用这个属性
        /// </summary>
        [DataMember]
        public EncryptionMethods EncryptionMethods { get; set; }

        /// <summary>
        /// 暂时media和agent没有使用
        /// </summary>
        [DataMember]
        public String ParentJobId { get; set; }

        /// <summary>
        /// 暂时media和agent没有使用
        /// </summary>
        [DataMember]
        public String IndexLogicDriver { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public Int64 AchiverTime { get; set; }

        [DataMember]
        public Int64 RetentionTimeSpanSeconds { get; set; }

        [DataMember]
        public Int32 SpVersion { get; set; }

        [DataMember]
        public String StoragePolicyId { get; set; }

        [DataMember]
        public CompressionType CompressionType { get; set; }

        [DataMember]
        public DataSecurity DataSecurity { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexLogicalDevice { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public Dictionary<String, Rule> Rules { get; set; }

        [DataMember]
        public string RuleId { get; set; }

        [DataMember]
        public int SourceFlag { get; set; }

        [DataMember]
        public ArchiverSiteInfoDto ArchiverSiteInfoDto { get; set; }

        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        [DataMember]
        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }

        [DataMember]
        public Boolean UseSnapLock { get; set; }

        [DataMember]
        public Boolean UseArchiverTier { get; set; }

        [DataMember]
        public List<PermissionLevel> PermissionLevel { get; set; }

        [DataMember]
        public Boolean IsCurrentSiteCollectionBackuped { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }  //SAAS-12519 增加contract支持List View

        [DataMember]
        public bool DisableIRMSetting { get; set; }

        [DataMember]
        public bool IncludeTerm { get; set; }

        [DataMember]
        public bool ManualArchive { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }
        public override String ToString()
        {
            return String.Format("Media TCP Request: Job Id: {0}, Encryption Info: {1}",
                this.JobId,
                this.EncryptionInfo);
        }
    }
}
