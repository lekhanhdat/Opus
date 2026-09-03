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
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityOnlineDeadAccountCleanerOperation : CAOperation
    {
        [DataMember]
        public List<OnlineDeadAccount> OnlineDeadAccounts { get; set; }

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
        /// <summary>
        /// 判断时候给administrator发邮件
        /// </summary>
        [DataMember]
        public bool IsSendEmailToAdmin { get; set; }

        [DataMember]
        public List<string> AllSiteAdminEmails { get; set; }

        [DataMember]
        public bool IsDeleteDeletedAccount { get; set; }

        [DataMember]
        public bool IsDeleteDeactivatedAccount { get; set; }
    }

    /// <summary>
    /// 用于返回搜索结果
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnlineDeadAccountResult : ResultBase
    {
        [DataMember]
        public List<OnlineDeadAccount> OnlineDeadAccounts { get; set; }

        [DataMember]
        public Int32 TotalCount { get; set; }

        [DataMember]
        public String JobID { get; set; }

        [DataMember]
        public int SendResultAction { get; set; }//0代表Insert，1代表Update
    }

    /// <summary>
    /// 用于返回GET结果中的deadaccount
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnlineDeadAccount
    {
        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public int IsDeletedSuccess { get; set; }//0, failed, 1, success, 2, not process

        [DataMember]
        public string FailedReason { get; set; }

        [DataMember]
        public string CloneLoginName { get; set; }// 将deaduser的permission clone到指定的user name

        [DataMember]
        public string Permissions { get; set; }

        [DataMember]
        public string CloneUserPermissions { get; set; }// 用于记录TransferPermission的Users

        [DataMember]
        public AccountStatus Status { get; set; } // true 表示 deleted
    }

}
