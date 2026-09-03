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



namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOSiteMasterIndexDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        [DataMember]
        public string CycleId { get; set; }

        /// <summary>
        /// 添加该属性主要因为FarmName用户可能修改，此属性主要用来查找Farm info。
        /// </summary>
        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        /// Logical Device Id
        /// </summary>
        [DataMember]
        public string LogicalDeviceId { get; set; }

        /// <summary>
        /// Physical Device Id存储这个字段是为了方便以后做report，例如获取某个physical device上做过哪些job
        /// </summary>
        [DataMember]
        public string PhysicalDeviceId { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// Plan的type用来区分是哪些类型的plan，如item level site level等
        /// </summary>
        [DataMember]
        public EOBackupPlanType PlanType { get; set; }

        /// <summary>
        /// 存放run backup job时,用户应用的Security encryption info信息,在restore load job时使用.
        /// </summary>
        [DataMember]
        public DataEncryptionInfo SecurityInfo { get; set; }

        /// <summary>
        /// 记录pruning的状态:success or failed,
        /// -1代表job记录被删除
        /// 0或者default(int)代表"成功",1或者不等于default(int)代表失败.(注释：6.0GA media将'3'作为default(int),6.1 version后default(int)改回0状态.)
        /// </summary>
        [DataMember]
        public int PruneState { get; set; }

        /// <summary>
        /// 记录job的状态:成功，失败.....
        /// </summary>
        [DataMember]
        public int JobState { get; set; }

        /// <summary>
        /// 记录本次job是否真正备份了数据，主用用于Only show incremental data功能
        /// </summary>
        [DataMember]
        public int ModifyData { get; set; }

        /// <summary>
        /// sharepoint 版本
        /// </summary>
        [DataMember]
        public int SPVersion { get; set; }

        [DataMember]
        public string CreatedUserId { get; set; }

        /// <summary>
        /// 存放Storage policy Id便于查找media service
        /// </summary>
        [DataMember]
        public string StoragePolicyId { get; set; }

        /// <summary> 存放Index Job扩展信息</summary>
        [DataMember]
        public EOSiteMasterIndexExtension IndexExtension { get; set; }

        [DataMember]
        public EODataVersionContentDto VersionDetails { get; set; }

        /// <summary>
        /// 存放主记录对应的所有子记录，默认情况下为null，需要手动赋值
        /// </summary>
        [DataMember]
        public List<EOSiteMasterIndexSubDto> subInfos { get; set; }

        /// <summary>
        /// 当前cycle 的的状态， 成功 失败。。
        /// </summary>
        [DataMember]
        public int CycleJobState { get; set; }

        /// <summary>
        /// 当前cycle 的备份时间，以最后的job 时间为准
        /// </summary>
        [DataMember]
        public long CycleBackupTime { get; set; }

        [DataMember]
        public long MediaDataSize { get; set; }

        [DataMember]
        public long AgentDataSize { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EOSiteMasterIndexSubDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        ///  备份数据的web application name
        /// </summary>
        [DataMember]
        public string GroupName { get; set; }

        /// <summary>
        /// 备份数据的site collection name
        /// </summary>
        [DataMember]
        public string Address { get; set; }

        /// <summary>
        /// 记录当前site collection是否真正备份了数据，主要用于Only show incremental data功能。注意与主表中ModifyData属性的区别。
        /// </summary>
        [DataMember]
        public int ModifyData { get; set; }

        /// <summary>
        /// 存储特殊介质的Index Database的标示符
        /// </summary>
        [DataMember]
        public string StorageInfo { get; set; }

        /// <summary>
        /// 可以通过这个ID找到Run job的Media
        /// </summary>
        [DataMember]
        public string MediaServiceId { get; set; }

        /// <summary>
        /// 标记subinfo的类型，如 0: mailbox， 1: office365 group
        /// </summary>
        [DataMember]
        public int DataType { get; set; }
    }
}
