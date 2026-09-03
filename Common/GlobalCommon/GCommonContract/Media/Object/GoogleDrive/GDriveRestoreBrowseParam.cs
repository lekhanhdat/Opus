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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives

    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using Server.ControlPanel.Cryptography.Wrapper;

    #endregion using directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GDriveRestoreBrowseParam
    {
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string UserAddress { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public long EndTime { get; set; }

        [DataMember]
        public int OffSet { get; set; }

        [DataMember]
        public int Length { get; set; }

        [DataMember]
        public bool OnlyOneJob { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public string BackupPlanId { get; set; }

        [DataMember]
        public string BackupCycleId { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }
        //[DataMember]
        //public OversizeIndexDBCacheInfo OversizeIndexDBCacheInfo { get; set; }
        //[DataMember]
        //public HostIndexDBCacheInfo HostIndexDBCacheInfo { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public MailboxType MailboxType { get; set; }

        /// <summary> 存储storage的一些信息，如：EMC存储介质的clip id,Dell存储介质的Object id。</summary>
        [DataMember]
        public string StorageInfo { get; set; }

        [DataMember]
        public DataEncryptionInfoWrapper IndexEncryptionInfoWrapper { get; set; }

        [DataMember]
        public bool SupportObjectId { get; set; }

        [DataMember]
        public long RetentionEarliestTime { get; set; }

        [DataMember]
        public bool BackendPagination { get; set; }

        [DataMember]
        public string ObjectId { get; set; }
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        /// <summary> Restore load tree 时，选择 all、all with deleted contents、from this backup only </summary>
        //[DataMember]
        //public BrowseShowDataType ShowDataType { get; set; }

        [DataMember]
        public string OrderRule { get; set; }
        [DataMember]
        public string OrderField { get; set; }

        public override string ToString()
        {
            return string.Format("GDriveBrowseParam : Path: {0}, Level: {1}, Backup Job ID: {2}", Path, Level, BackupJobId);
        }
    }
}