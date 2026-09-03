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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchCondition : CAOperation
    {
        [DataMember]
        public CACommonFilterPolicy FilterPolicy { get; set; }
        [DataMember]
        public PermissionCondition SearchForPermissionCondition { get; set; } //for security search

        [DataMember]
        public List<PolicyLevel> SearchLevels { get; set; }

        [DataMember]
        public SecuritySearchResultInfo ResultSummaryInfo { get; set; }

        [DataMember]
        public string ExportReportId { get; set; }

        //20161019 shayne SAAS-23728 添加该planID属性
        [DataMember]
        public string downLoadPlanId { get; set; }
        //SAAS-22651
        //[DataMember]
        //public bool excludeHiddenList { get; set; }

        //[DataMember]
        //public List<PolicyLevel> SearchForLevels { get; set; }
        [DataMember]
        public List<TreeNodeCollection> SearchNodeInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminSearchCondition : SearchCondition
    {
        //[DataMember]
        //public CACommonFilterPolicy FilterPolicy { get; set; }
        //[DataMember]
        //public PermissionCondition SearchForPermissionCondition { get; set; } //for security search

        //[DataMember]
        //public SecuritySearchResultInfo ResultSummaryInfo { get; set; }
    }

    #region CASearchFilter
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchFilter
    {
        [DataMember]
        public SPObjectLevel ResultLevel { get; set; }
    }
    #endregion

    #region Security Search Result Contract
    //此类为了统计Level,Inherited,AccountType的filter信息
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecuritySearchResultInfo
    {
        [DataMember]
        public List<bool> AllInherited { get; set; }

        [DataMember]
        public List<string> AllLevels { get; set; }

        [DataMember]
        public List<MemberType> AllAccountTypes { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SeacuritySearchResult : AdminSearchResult
    {
        /// <summary>
        /// 一个TreeNode附带的User和Group结果
        /// </summary>
        [DataMember]
        public SecurityResults UsersAndGroups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityResults
    {
        [DataMember]
        public List<SecurityMember> Members { get; set; }

        [DataMember]
        public string TreeNodeId { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MemberType
    {
        [EnumMember]
        SharePointGroup,
        [EnumMember]
        User,
        [EnumMember]
        DomainGroup,
        [EnumMember]
        Guest,
        [EnumMember]
        None
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Permission
    {
        [DataMember]
        public string PermissionName { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public bool Hidden { get; set; }
        [DataMember]
        public ulong PermissionMask { get; set; }

        [DataMember]
        public AveRoleTypeForPermission Type { get; set; }

        #region Override Equals()
        public override bool Equals(object obj)
        {
            if (!(obj is Permission)) return false;
            Permission p = obj as Permission;
            return PermissionName == p.PermissionName &&
                Id == p.Id && Hidden == p.Hidden;
        }

        public override int GetHashCode()
        {
            return PermissionName.GetHashCode() + Id.GetHashCode() + PermissionMask.GetHashCode() + Type.GetHashCode();
        }
        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BinaryPermission :Permission {
        private String _binaryPermissionMask;
        [DataMember]
        public String BinaryPermissionMask
        {
            set {  this._binaryPermissionMask = value; }            
            get { return Convert.ToString((long)this.PermissionMask, 2); }
        }
    }

    /// <summary>
    /// 将SecurityMember设计成一个Tree的结构
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityMember
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Permission> Permissions { get; set; }

        [DataMember]
        public MemberType MemberType { get; set; }

        /// <summary>
        /// AD User和AD Group用到此属性
        /// </summary>
        [DataMember]
        public MemberStateType MemberState { get; set; }

        /// <summary>
        /// AD的user或者AD Group才需要用到此属性
        /// </summary>
        [DataMember]
        public List<string> ParentGroupList { get; set; }

        [DataMember]
        public List<SecurityMember> Children { get; set; }

        /// <summary>
        /// 只有AD Group才用到此属性
        /// </summary>
        [DataMember]
        public int UserNumber { get; set; }

        /// <summary>
        /// user/group的ID 付给前台tree上的节点 Add User\Delete User  都会用到
        /// </summary>
        [DataMember]
        public int MemberID { get; set; }

        [DataMember]
        public bool AllowMembersEditMembership { get; set; }

        [DataMember]
        public bool AllowRequestToJoinLeave { get; set; }

        [DataMember]
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        [DataMember]
        public bool OnlyAllowMembersViewMembership { get; set; }

        [DataMember]
        public string RequestToJoinLeaveEmailSetting { get; set; }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public string Discription { get; set; }

        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public AccountZone Zone { get; set; }
    }

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MemberStateType : int
    {
        [EnumMember]
        Active,
        [EnumMember]
        Disabled,
        [EnumMember]
        Deleted
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionCondition
    {
        [DataMember]
        public List<UserDetail> ADUserGroups { get; set; }
        [DataMember]
        public List<string> FBAUserGroups { get; set; }
        [DataMember]
        public SearchForPermissionOption Permission { get; set; }
        [DataMember]
        public bool ExtractPermission { get; set; }
        [DataMember]
        public AdvancedSecurityPermissionCondition AdvancedPermission { get; set; } //only for search search
        [DataMember]
        public bool OnlySearchInheritNodes { get; set; }
        [DataMember]
        public List<UserType> UserTypeItemsSource { get; set; }
        [DataMember]
        public bool? SearchIncludeInheritPermissions { get; set; }
        //SAAS-22651
        [DataMember]
        public bool? ExcludeHiddenListAndAppData { get; set; }
        [DataMember]
        public bool? ExcludeSystemList { get; set; }
        [DataMember]
        public string CustomizedPermissionLevels { get; set; }
        [DataMember]
        public string SharePointGroups { get; set; }
    }

    public enum UserType 
    {
        [EnumMember]
        SharePointUserAndGroup,
        [EnumMember]
        GuestLink,
        [EnumMember]
        ExternalUser,
        [EnumMember]
        ADGroup
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdvancedSecurityPermissionCondition
    {
        [DataMember]
        public bool SearchForDeactivatedUsersOnly { get; set; }
        [DataMember]
        public bool IncludeUserWithNoPermissions { get; set; }
        [DataMember]
        public bool AlsoSearchParentADGroups { get; set; }
        [DataMember]
        public bool IncludeADGroupMembers { get; set; }
        [DataMember]
        public bool SearchAllMembers { get; set; }
        [DataMember]
        public List<string> NonotExpandGroups { get; set; }
        [DataMember]
        public bool ExcludeGroups { get; set; }
        [DataMember]
        public int Depth { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchForPermissionOption
    {
        [EnumMember]
        SearchForAnyPermission = 0,
        [EnumMember]
        FullControl = 1,
        [EnumMember]
        Design = 2,
        [EnumMember]
        Contribute = 3,
        [EnumMember]
        Read = 4,
        [EnumMember]
        ViewOnly = 5,
        [EnumMember]
        LimitedAccess = 6,
        [EnumMember]
        InputedPermission = 7,
        [EnumMember]
        Edit = 8,
        [EnumMember]
        Administrator = 9,
        [EnumMember]
        CustomizedPermissionLevels = 10,
    }

    /// <summary>
    /// 暴露给GUI的数据契约 针对ListView显示,一个对象对应ListView中的一条记录
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchResultNodeInfo : BackstageResultBase
    {
        //包含这个user的站点信息
        [DataMember]
        public string NodeId { set; get; }
        /// <summary>
        /// 表示tree结点的title
        /// </summary>
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public NodeLevel PathType { set; get; }
        [DataMember]
        public int Inherit { set; get; }
        [DataMember]
        public string Path { set; get; }
        [DataMember]
        public string FullPath { set; get; }
        [DataMember]
        public string Agent { set; get; }
        [DataMember]
        public string DisplayName { set; get; }
        [DataMember]
        public int LockStatus { set; get; }

        //id信息
        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public string SPObjectId { set; get; }

        [DataMember]
        public string ParentId { set; get; }
        [DataMember]
        public string FarmId { set; get; }

        //member信息，是指user和group

        [DataMember]
        public int MemberID { get; set; }
        [DataMember]
        public string MemberName { get; set; }
        [DataMember]
        public MemberType MemberType { set; get; }
        [DataMember]
        public int MemberState { set; get; }
        [DataMember]
        public string ParentGroupList { set; get; }

        [DataMember]
        public List<Permission> Permissions { set; get; }

        [DataMember]
        public List<BinaryPermission> BinaryPermissions { set; get; }

        // GroupInfo
        [DataMember]
        public bool AllowMembersEditMembership { get; set; }

        [DataMember]
        public bool AllowRequestToJoinLeave { get; set; }

        [DataMember]
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        [DataMember]
        public bool OnlyAllowMembersViewMembership { get; set; }

        [DataMember]
        public string RequestToJoinLeaveEmailSetting { get; set; }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public AccountZone Zone { get; set; }

        [DataMember]
        public int ParentMemberId { get; set; }

        [DataMember]
        public string ParentMemberName { get; set; }

        [DataMember]
        public int ParentMemberType { get; set; }

        /// <summary>
        /// 用于区分List/Library以及Item/Document
        /// </summary>
        [DataMember]
        public NodeType NodeType { get; set; }

        [DataMember]
        public Int32 SPVersion { get; set; }

        /// <summary>
        /// 用于区分Moss和BPOS
        /// </summary>
        [DataMember]
        public SPType SPType { get; set; }

        /// <summary>
        /// Use by item level, import config file
        /// </summary>
        public string WebUrl { get; set; }
        /// <summary>
        /// Use by item level, import config file
        /// </summary>
        public string ListTitle { get; set; }
        /// <summary>
        /// Use by item level, import config file
        /// </summary>
        public int ItemRowId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchResultNodeInfosWrapper
    {
        [DataMember]
        public string JobID { set; get; }

        [DataMember]
        public List<CASearchResultNodeInfo> CASearchResultNodeInfos { set; get; }

        [DataMember]
        public SecuritySearchResultInfo ResultSummaryInfo { get; set; }

        [DataMember]
        public int TotalRowCounts { set; get; }

        [DataMember]
        public bool IsLarge { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAdminSearchResultInfosWrapper
    {
        [DataMember]
        public List<SPTreeNodeDto> TreeNodes { get; set; }

        [DataMember]
        public bool IsLarge { get; set; }

    }
    public enum AveRoleTypeForPermission
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Guest = 1,
        [EnumMember]
        Reader = 2,
        [EnumMember]
        Contributor = 3,
        [EnumMember]
        WebDesigner = 4,
        [EnumMember]
        Administrator = 5,
        [EnumMember]
        Editor = 6,
        [EnumMember]
        Reviewer = 7,
        [EnumMember]
        RestrictedReader = 8,
        [EnumMember]
        RestrictedGuest = 9,
        [EnumMember]
        System = 255
    }
}
