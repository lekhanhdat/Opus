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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityEditGroupsOperation : CAOperation
    {
        /// <summary>
        /// For Server
        /// </summary>
        [DataMember]
        public SecurityConfigurationFileInfo ConfigurationFileInfo { get; set; }

        /// <summary>
        /// key: site url; value: spgroup or spuser
        /// </summary>
        [DataMember]
        public Dictionary<string, List<CAEditGroupsResult>> Groups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAEditGroupsResult : ResultBase
    {
        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string SiteTitle { get; set; }

        [DataMember]
        public CAPrincipalType PrincipalType { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        /// <summary>
        /// 目前的逻辑如果是sharepoint group，
        /// 那么前台传递username是空，如果不是空的话，我们会把当做group owner处理
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public ChangeMode Action { get; set; }

        [DataMember]
        public CheckPrincipalStatus Status { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string GroupOwner { get; set; }

        [DataMember]
        public string ViewMembership { get; set; }

        [DataMember]
        public string EditMembership { get; set; }

        [DataMember]
        public string AllowRequests { get; set; }

        [DataMember]
        public string AutoacceptRequests { get; set; }

        [DataMember]
        public string MembershipRequestEmailAddress { get; set; }

        public CAEditGroupsResult(string siteUrl, string siteTitle, string groupName, string userName, ChangeMode action)
        {
            //status, comment由后续操作决定是否覆盖
            this.SiteUrl = siteUrl;
            this.SiteTitle = siteTitle;
            this.PrincipalType = CAPrincipalType.SharePointUser;
            this.UserName = userName;
            this.GroupName = groupName;
            this.Action = action;
            this.Status = CheckPrincipalStatus.Succeed;
            this.Comment = string.Empty;
        }

        public void Update(CheckPrincipalStatus status)
        {
            Update(status, string.Empty);
        }

        public void Update(CheckPrincipalStatus status, string comment)
        {
            this.Status = status;
            this.Comment = comment;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EditAction
    {
        [EnumMember]
        None,
        [EnumMember]
        ExportGroupInfo,
        [EnumMember]
        ImportGroupInfo,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EditGroupResultsOverview
    {
        [DataMember]
        public int TotalCount { get; set; }
        [DataMember]
        public int SucceedAddUsers { get; set; }
        [DataMember]
        public int SucceedRemoveUsers { get; set; }
        [DataMember]
        public int SucceedAddGroups { get; set; }
        [DataMember]
        public int SucceedRemoveGroups { get; set; }
        [DataMember]
        public int FailedAddUsers { get; set; }
        [DataMember]
        public int FailedRemoveUsers { get; set; }
        [DataMember]
        public int FailedAddGroups { get; set; }
        [DataMember]
        public int FailedRemoveGroups { get; set; }
    }
}
