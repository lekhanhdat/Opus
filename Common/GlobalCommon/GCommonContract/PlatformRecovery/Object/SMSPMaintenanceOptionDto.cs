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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNMaintenanceOptionDto
    {
        //for index
        [DataMember]
        public int IndexJobQuantity { get; set; }

        [DataMember]
        public PRStagingPolicyDto IndexServer { get; set; }

        [DataMember]
        public string MountPointForIndex { get; set; }

        //for verify
        [DataMember]
        public int VerifyJobQuanlity { get; set; }

        [DataMember]
        public PRStagingPolicyDto VerifyServer { get; set; }

        [DataMember]
        public string MountPointForVerify { get; set; }

        //for snap mirror
        [DataMember]
        public bool IsUpdateMirror { get; set; }

        [DataMember]
        public bool IsVerifyDestMirror { get; set; }

        //[DataMember]
        //public bool IsUpdateDeviceMirror { get; set; }

        //for snap vault
        [DataMember]
        public bool IsArchiveBackup { get; set; }

        [DataMember]
        public bool IsVerifyArchive { get; set; }

        [DataMember]
        public ManagementGroup Group { get; set; }

        //for script
        [DataMember]
        public bool IsRunScript { get; set; }

        [DataMember]
        public PRSNCommandOperationDto CommandOperationDto { get; set; }

        //for storage policy settings
        [DataMember]
        public bool IsUpdateDeviceMirror { get; set; }
        [DataMember]
        public bool IsUpdateDeviceVault { get; set; }

        #region Blob Setting
        [DataMember]
        public bool IsUpdateSnapMirror { get; set; }
        [DataMember]
        public bool IsUpdateSnapVault { get; set; }
        #endregion

        // CloneOnMirrorDestination
        [DataMember]
        public bool IsCloneOnMirrorDestination { get; set; }
        // CloneOnVaultDestination
        [DataMember]
        public bool IsCloneOnVaultDestination { get; set; }
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class SQLInstanceConfig 
    //{
    //    [DataMember]
    //    public string Id { get; set; }
    //    [DataMember]
    //    public string ProfileName { get; set; }
    //    [DataMember]
    //    public string ServerName { get; set; }
    //    [DataMember]
    //    public AuthenticationType Authentication { get; set; }
    //    [DataMember]
    //    public string Username { get; set; }
    //    [DataMember]
    //    public string Description { get; set; }
    //    [DataMember]
    //    public bool IsAllFarmsUsedServer { get; set; }
    //    [DataMember]
    //    public List<string> FarmNames { get; set; }
    //    //[DataMember]
    //    //public PRSNErrorCode StagingErrorCode { get; set; }
    //    [DataMember]
    //    public ErrorCode StagingNamePassedCode { get; set; }
    //}

    //[Flags, DataContract(Namespace = ContractConstants.Namespace)]
    //public enum PRSNErrorCode
    //{
    //    [EnumMember]
    //    NoError = 0,
    //    [EnumMember]
    //    StagingPolicyNameError = 1,
    //    [EnumMember]
    //    AccountInfoError = 2,
    //    [EnumMember]
    //    TemporaryDBError = 3,
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ManagementGroup
    {
        // client标识没有选择ManagementGroup
        [EnumMember]
        All = 0,

        [EnumMember]
        Daily = 1,

        [EnumMember]
        Hourly = 2,

        [EnumMember]
        Monthly = 3,

        [EnumMember]
        Unlimited = 4,

        [EnumMember]
        Weekly = 5,

        // 不使用
        [EnumMember]
        Standard = 6
    }
}