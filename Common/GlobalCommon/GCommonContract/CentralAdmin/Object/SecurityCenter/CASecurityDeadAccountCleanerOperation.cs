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




namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityDeadAccountCleanerOperation : CAOperation
    {
        /// <summary>
        /// 内部Dictionary 的key是DeadAccount loginName，value是Transfer目的端
        /// </summary>
        [DataMember]
        public Dictionary<string, Dictionary<DeadAccount, string>> Accounts { get; set; }

        [DataMember]
        public string GetJobId { get; set; }

        [DataMember]
        public TransferOption TransferOption { get; set; }

        [DataMember]
        public List<string> MetaData { get; set; }

        [DataMember]
        public bool IncludeAlerts { get; set; }

        [DataMember]
        public bool IncludeInheritedFromGroup { get; set; }

        [DataMember]
        public bool AddToSameGroupsInDestination { get; set; }

        [DataMember]
        public bool RemoveProfilesFromSSP { get; set; }

        [DataMember]
        public MySiteOption MySiteOption { get; set; }

        [DataMember]
        public bool VerifyBeforeDelete { get; set; }

        [DataMember]
        public string DocAveAccount { get; set; }

        [DataMember]
        public string DeviceID { get; set; }

        [DataMember]
        public bool isWebLevel { get; set; }

        /// <summary>
        /// 用于标识是否是Security Search的后续操作
        /// </summary>
        [DataMember]
        public bool IsSecuritySearchResultAction { get; set; }

        /// <summary>
        /// DeadAccount Cleaner Filter
        /// </summary>
        [DataMember]
        public FilterPolicyInfo FilterPolicyInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MySiteOption
    {
        [EnumMember]
        Archive,

        [EnumMember]
        Delete,

        [EnumMember]
        Keep
    }
    /// <summary>
    /// 用于返回搜索结果
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeadAccountResult : ResultBase
    {
        [DataMember]
        public List<DeadAccount> DeadAccounts { get; set; }

        [DataMember]
        public Int32 TotalCount { get; set; }

        [DataMember]

        public String JobID { get; set; }
    }

    /// <summary>
    /// 用于返回GET结果中的deadaccount
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeadAccount
    {
        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public string Permissions { get; set; }

        /// <summary>
        /// 用于记录TransferPermission的Users
        /// </summary>
        [DataMember]
        public string CloneUserPermissions { get; set; }

        [DataMember]
        public AccountStatus Status { get; set; } // true 表示 deleted

        [DataMember]
        public bool IsUtilityCheck { get; set; }

        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// 0, failed, 1, success, 2, not process
        /// </summary>
        [DataMember]
        public int IsSuccess { get; set; }

        [DataMember]
        public string FailedReason { get; set; }
    }

}