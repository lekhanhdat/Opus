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
namespace AvePoint.GCommon.Contract.MediaManagement.Object
{
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataTransferMessageBase : AveMessage
    {
        #region Copy from AvePoint.GCommon.Contract.Media.Object.RestoreParamDto
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String Path { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public Int64 EndTime { get; set; }

        [DataMember]
        public Int32 OffSet { get; set; }

        [DataMember]
        public Int32 Length { get; set; }

        [DataMember]
        public Boolean OnlyOneJob { get; set; }

        [DataMember]
        public String BackupJobId { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String BackupPlanId { get; set; }

        [DataMember]
        public String BackupCycleID { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public PlatformType PlatformType { get; set; }

        [DataMember]
        public ProductVersion ProductVersion { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        /// <summary> 存储storage的一些信息，如：EMC存储介质的clip id,Dell存储介质的Object id。</summary>
        [DataMember]
        public string StorageInfo { get; set; }

        [DataMember]
        public Boolean IsMigrationBrowse { get; set; }

        [DataMember]
        public List<string> BackupJobIds { get; set; }

        public override String ToString()
        {
            return String.Format("Path: {0}, Level: {1}, Backup Job ID: {2}", this.Path, this.Level, this.BackupJobId);
        } 
        #endregion

        [DataMember]
        public Boolean NeedDeleteSourceData { get; set; }

        [DataMember]
        public List<LogicalDeviceDto> SourceLogicalDevices { get; set; }

        [DataMember]
        public LogicalDeviceDto DestinationLogicalDevice { get; set; }

        [DataMember]
        public String TransferJobId { get; set; }

        [DataMember]
        public Boolean NeedTransferSourceData { get; set; }

        [DataMember]
        public String SubJobId { get; set; }
    }
}
