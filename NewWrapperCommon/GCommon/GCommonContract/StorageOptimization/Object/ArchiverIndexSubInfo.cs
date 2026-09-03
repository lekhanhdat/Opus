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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
//using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// Archiver Site Master Index表的子表contract, 存rule中的Storage信息用于Restore和Retention
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverIndexSubInfoContract
    {
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 和主表中的JobId关联, 用于反查ArchiverSiteMasterIndex记录. 以及Retention JobMode
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        [DataMember]
        public string LogicalDeviceId { get; set; }

        [DataMember]
        public string PhysicalDeviceId { get; set; }
        /// <summary>
        /// 用于Retention取Media和Restore和备用Media
        /// </summary>
        [DataMember]
        public string MediaServiceId { get; set; }

        /// <summary>
        /// 第一次存Archiver Time, 执行Retention后更新为Retention Time
        /// </summary>
        [DataMember]
        public long RetentionTime { get; set; }

        /// <summary>
        /// 保存保留时间，以秒为单位
        /// </summary>
        [DataMember]
        public long RetentionTimeSpanSeconds { get; set; }

        /// <summary>
        /// 存放 backup job时应用的Security profile信息,在restore job时使用.
        /// </summary>
        [DataMember]
        public DataEncryptionInfo DataEncryptionInfo { get; set; }
        /// <summary>
        /// 目前Docave5的数据为null
        /// </summary>
        [DataMember]
        public ArchiverSubInfoExtension ArchiverSubInfoExtension { get; set; }

        [DataMember]
        public string StorageInfo { get; set; }

        [DataMember]
        public long MediaDataSize { get; set; }

        [DataMember]
        public long AgentDataSize { get; set; }
        [DataMember]
        public string CurrentStorageId { get; set; }
        [DataMember]
        public int RetentionCount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSubInfoExtension
    {
        /// <summary>
        /// 此属性不再使用, 保留只为某些POC客户Patch升级用; 确认几个POC分支的客户都升级到了6.1之后, 可以删除此属性.
        /// </summary>
        [DataMember]
        public StoragePolicyDto RetentionPolicy { get; set; }

        [DataMember]
        public DataEncryptionInfo DataEncryptionInfo { get; set; }
        /// <summary>
        /// 保存做数据时的retention 设置
        /// </summary>
        [DataMember]
        public RetentionRuleOption RetentionOption { get; set; }
        /// <summary>
        /// 用于比较是否是Delete数据.
        /// </summary>
        [DataMember]
        public string PrimaryLogicalId { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetainedInfo
    {
        [DataMember]
        public string SubSubJobId { get; set; }
        [DataMember]
        public long RetainSize { get; set; }
        [DataMember]
        public bool IsSimulateJob { get; set; }
        [DataMember]
        public long RetainFileNumber { get; set; }
    }
}
