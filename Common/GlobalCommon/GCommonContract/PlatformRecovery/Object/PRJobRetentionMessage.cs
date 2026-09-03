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




using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Retention;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRJobRetentionMessage : PRMultipleControlMessage
    {
        /// <summary>
        /// 所发往agent信息
        /// </summary>
        [DataMember]
        public ServiceDto ControlService { get; set; }

        /// <summary>
        /// 当retention中选择move时为false，其余为true
        /// </summary>
        [DataMember]
        public bool DeleteIfExists { get; set; }

        /// <summary>
        /// 发送需要删除的job信息，另作为返回值，其中JobRetentionState标识job是否删除成功
        /// </summary>
        [DataMember]
        public Dictionary<string, PRBackupJobDto> JobList
        {
            get;
            set;
        }

        /// <summary>
        /// 发送需要删除的job的Catalog消息
        /// </summary>
        [DataMember]
        public Dictionary<string, PRBackupCatalogDto> CatalogList
        {
            get;
            set;
        }

        /// <summary>
        /// key:jobID,value:snapshotSetIdList
        /// </summary>
        [DataMember]
        public Dictionary<string, List<Guid>> SnapShotRecordToDelete
        {
            get;
            set;
        }

        #region == for smsp ==

        /// <summary>
        /// key:jobID,value:indexdbList
        /// </summary>
        [DataMember]
        public Dictionary<string, List<PRJobRetentionInfo>> JobIdJobRetentionInfoList
        {
            get;
            set;
        }
        /// <summary>
        /// key:backupJobId, value: PRJobRetentionStatus
        /// </summary>
        [DataMember]
        public Dictionary<string, PRJobRetentionStatus> JobIdPRJobRetentionStatus
        {
            get;
            set;
        }

        [DataMember]
        public RetentionJobDto RetentionJobDto
        {
            get;
            set;
        }
        #endregion
    }
    /// <summary>
    /// 0:默认值
    /// 1：符合retention条件，应该delete index snapshot
    /// 2：不符合retention条件，但是需要check
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRJobRetentionStatus
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        IsRetention = 1,

        [EnumMember]
        IsCheckedOnly = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRJobRetentionInfo
    {
        [DataMember]
        public string SnapShotName
        {
            get;
            set;
        }

        [DataMember]
        public string FullPath
        {
            get;
            set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public RetentionInfoType Type
        {
            get;
            set;
        }

        [DataMember]
        public string Instance
        {
            get;
            set;
        }

        [DataMember]
        public string Agent
        {
            get;
            set;
        }

        [DataMember]
        public RetentionStatusType Status
        {
            get;
            set;
        }

        [DataMember]
        public List<PRJobRetentionInfo> DBLists
        {
            get;
            set;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum RetentionInfoType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        DB = 1,

        [EnumMember]
        Index = 2,

        [EnumMember]
        Extender=4,

        [EnumMember]
        Connector = 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RetentionStatusType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Succeed = 1,

        [EnumMember]
        Failed = 2,
    }
}
