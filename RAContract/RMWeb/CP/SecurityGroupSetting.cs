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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    [DataContract]
    public class SecurityGroupDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<AOSUserDto> Users { get; set; }
        [DataMember]
        public List<string> UserIds { get; set; }
        [DataMember]
        public long PermissionMasks { get; set; }
        [DataMember]
        public long SubPermission1Masks { get; set; }
        [DataMember]
        public long PermissionExtensionMasks { get; set; }
        [DataMember]
        public long SOPermissionMasks { get; set; }
        [DataMember]
        public List<SecurityDataSourceScopeDto> DataSourceScopeInfo { get; set; }
        [DataMember]
        public GroupsAndContainers AvailableDataSourceScopeInfo { get; set; }
        [DataMember]
        public Dictionary<SecurityTermLevel, List<Guid>> SelectedTermObjIds { get; set; }
        [DataMember]
        public SecurityTermInfo TermTreeNodeInfo { get; set; }
        [DataMember]
        public List<SecurityTermInfo> SelectedTermObjs { get; set; }
        [DataMember]
        public TermPermissionMethod SetTermPermissionMethod { get; set; }
        [DataMember]
        public SecurityRuleInfo RuleTreeNodeInfo { get; set; }
        [DataMember]
        public List<SecurityRuleInfo> SelectedRuleObjs { get; set; }
        [DataMember]
        public RulePermissionMethod SetRulePermissionMethod { get; set; }
        [DataMember]
        public bool IsEnableTrim { get; set; }
        [DataMember]
        public bool IsEnableManageHold { get; set; }
        [DataMember]
        public bool IsEnableApprovalSetting { get; set; }
        [DataMember]
        public bool IsBuiltInGroup { get; set; }
        [DataMember]
        public bool HasOpusILLicense { get; set; }

        [DataMember]
        public SecurityGroupControlType SecurityGroupControlType { get; set; }

        [DataMember]
        public FunctionSubPermission FunctionSubPermission { get; set; }

        [DataMember]
        public long ReportingPermission { get; set; }

        [DataMember]
        public bool IsUseReportingPermissionControl { get; set; }

        [DataMember]
        public bool IsNewGroup { get; set; } //for so only license upgrade

    }

    public class SimpleSecurityGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long PermissionMasks { get; set; }
        public long SOPermissionMasks { get; set; }
        public long ReportingPermission { get; set; }
        public List<SourceFlag> ContainsSourceType { get; set; }
        public string TermScope { get; set; }
        public string RuleScope { get; set; }
        public bool IsEnableTrim { get; set; }
        public bool IsEnableApprovalSetting { get; set; }
        public bool IsEnableManageHold { get; set; }
        public SubPermissionType PhysicalRole{ get; set; }
        public long PermissionExtensionMasks { get; set; }
        public bool IsBuiltInGroup { get; set; }
        public bool IsNewCreatedGroup { get; set; }  //for so only license upgrade
    }

    public class SecurityContainerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class GroupsAndContainers
    {
        public List<SimpleSecurityGroupDto> GroupItems { get; set; }
        public List<SecurityContainerDto> SPContainerItems { get; set; }
        public List<SecurityContainerDto> EXOContainerItems { get; set; }
        public List<SecurityContainerDto> OneDriveContainerItems { get; set; }
        public List<SecurityContainerDto> TeamsContainerItems { get; set; }
        public List<SecurityContainerDto> PhysicalLocationItems { get; set; }
    }
    [DataContract]
    public class SecurityDataSourceScopeDto
    {
        [DataMember]
        public List<Guid> ScopeIds { get; set; }
        [DataMember]
        public SourceFlag DataSourceType { get; set; }
        [DataMember]
        public SubPermissionType SubPermission { get; set; }
        [DataMember]
        public bool IsScopeAdmin { get; set; }
        [DataMember]
        public List<string> ScopePaths { get; set; }
        [DataMember]
        public List<SubPermission> SubPermissions { get; set; }
        [DataMember]
        public bool Hidden { get; set; }
    }

    public class ScopeContainerInfo
    {
        public bool IsChecked { get; set; }
        public string ContainerNames { get; set; }
    }

    public class SecurityUserDto
    {
        public string UserId { get; set; }
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public List<string> SecurityGroupNames { get; set; }
    }

    public class SecurityUserPermissionsDto
    {
        public bool IsAdmin { get; set; }
        public List<long> SecurityGroupPermissionMasks { get; set; }
        public List<SecurityDataSourceScopeDto> ScopePermissionInfo { get; set; }
        public UserSecurityTermPermissionDto TermPermissionInfo { get; set; }
        public UserSecurityRulePermissionDto RulePermissionInfo { get; set; }
        public List<long> SecurityGroupSubPermissionMasks { get; set; }
        public List<long> SecurityGroupPermissionExtensionMasks { get; set; }
        public List<long> SOPermissionMasks { get; set; }
        public List<long> ReportPermissionMasks { get; set; }
        public FunctionSubPermission FunctionMoudleRestoreCenter { get; set; }
        public bool IsEnableApprovalSetting { get; set; }
        public bool IsEnableManageHold { get; set; }
        public bool IsUseReportingPermissionControl { get; set; }
        public int ReportingPermission { get; set; }
        public List<bool> IsNewCreateGroupList { get; set; } //for so only license upgrade
        public bool HasHoldManagerPermission { get; set; }

    }
    [DataContract]
    public class QueryTermObjDto
    {
        [DataMember]
        public RMTermType ParentType { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public PagerInfo PageInfo { get; set; }
        [DataMember]
        public int GroupId { get; set; }
    }

    public class QueryTermObjResultDto
    {
        public List<SecurityTermInfo> TermObjItems { get; set; }
        public int TotalCount { get; set; }
    }
    [DataContract]
    public class QueryRuleObjDto
    {
        [DataMember]
        public RMRuleType ParentType { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public PagerInfo PageInfo { get; set; }
        [DataMember]
        public int GroupId { get; set; }
    }

    public enum BuiltInGroupId
    { 
        Admin = 1,
        EndUser = 2
    }
    [DataContract]
    public class PagerInfo
    {
        [DataMember]
        public int PagerIndex { get; set; }
        [DataMember]
        public int PagerSize { get; set; }
    }

    public class SecurityTermInfo
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public RMTermType Type { get; set; }
        public bool IsChecked { get; set; }
        public bool IsExpand { get; set; }
        public bool IsLoaded { get; set; }
        public int SubPerIndex { get; set; }
        public int SubPerSize { get; set; }
        public int SubTermCount { get; set; }
        public List<SecurityTermInfo> SubTerms { get; set; }
    }

    public class UserSecurityTermPermissionDto
    {
        public TermPermissionMethod TermPermissionType { get; set; }
        public List<SimpleSecurityTermInfo> TermGroups { get; set; }
    }
    
    public class UserSecurityRulePermissionDto
    {
        public RulePermissionMethod RulePermissionType { get; set; }
        public List<SecurityRuleInfo> RuleContainers{ get; set; }
    }
    public class SimpleSecurityTermInfo
    {
        public Guid UniqueId { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public RMTermType Type { get; set; }
        public bool IsExpand { get; set; }
        public int SubPerIndex { get; set; }
        public int SubPerSize { get; set; }
        public int SubTermCount { get; set; }
        public List<SimpleSecurityTermInfo> SubTerms { get; set; }
    }

    public class SecurityTermPermissionDto
    {
        public List<Guid> TermObjIds { get; set; }
        public TermPermissionMethod TermPermissionType { get; set; }
    }
    [DataContract]
    public class ValidateSecurityGroupDto
    {
        [DataMember]
        public SecurityGroupDto ValidateGroup { get; set; }
        [DataMember]
        public SecurityGroupValidateType ValidateType { get; set; }
    }
    public class SecurityTermRuleConflictDto
    {
        public string ObjectName { get; set; }
        public string ObjectId { get; set; }
        public List<TermRuleConflictItemDto> ConflictItems { get; set; }
    }

    public class TermRuleConflictItemDto
    {
        public string ItemName { get; set; }
        public string ItemFullPath { get; set; }
        public string ItemId { get; set; }
        public int ItemLevel { get; set; }
    }

    public class QuerySecurityTermObjDto
    {
        public string UserId { get; set; }
        public List<string> UserAndGroupIds { get; set; }
        public SecurityTermLevel Level { get; set; }
        public Guid ParentId { get; set; }

        public bool FilterByContentSource { get; set; }
        public bool ExcludeBuiltIn { get; set; }
        public string ContainerId { get; set; }
        public SourceFlag SourceFlag { get; set; }
        public bool ForPhysicalView { get; set; }
    }

    public class FilterTermObjOption
    {
        public bool NeedCheckPermission { get; set; }
        public List<string> userAndGroupUserIds { get; set; }

        public bool FilterByContentSource { get; set; }
        public bool ExcludeBuiltIn { get; set; }
        public string ContainerId { get; set; }
        public SourceFlag SourceFlag { get; set; }
        public bool ForPhysicalView { get; set; }
    }

    public enum TermPermissionMethod
    { 
        None = 0,
        All = 1,
        SpecifyScope = 2
    }

    public enum RulePermissionMethod
    {
        None = 0,
        All = 1,
        SpecifyScope = 2
    }
    [DataContract]
    public enum SubPermissionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Admin = 1,
        [EnumMember]
        EndUser = 2
    }
    [DataContract]
    public enum SecurityTermLevel
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        All = 0,
        [EnumMember]
        TermGroup = 1,
        [EnumMember]
        TermSet = 2,
        [EnumMember]
        Term = 3,
        [EnumMember]
        TermGroupForPhysicalView = 4,
        [EnumMember]
        TermSetForPhysicalView = 5,
        [EnumMember]
        TermForPhysicalView = 6
    }
    [DataContract]
    public enum SubPermission
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SetAccessControl = 1,
        [EnumMember]
        BoxCreationRequest = 2,
        [EnumMember]
        FolderCreationRequest = 3,
        [EnumMember]
        FolderLoanRequest = 4,
        [EnumMember]
        FolderLoanReturn = 5,
        [EnumMember]
        MoveRequest = 6
    }

    [DataContract]
    public enum SecurityGroupControlType
    {
        [EnumMember]
        DataScope = 0,
        [EnumMember]
        FunctionModule = 1
    }

    [DataContract]
    public enum FunctionSubPermission
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        RestoreCenterFullControl = 1,
        [EnumMember]
        RestoreCenterExport = 2,
        [EnumMember]
        RestoreCenterSearch = 3,
    }
    [DataContract]
    public enum SearchMode
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NormalSearch = 1,
        [EnumMember]
        FullTextAdvanceSearch = 2,
        [EnumMember]
        FullTextSimpleSearch = 3,
    }
    [DataContract]
    public enum ReportingPermission
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ContentDueForAction = 1,
        [EnumMember]
        TermUsage = 2,
        [EnumMember]
        RuleUsage = 4,
        [EnumMember]
        CreationAndDestruction = 8,
        [EnumMember]
        ActionAudit = 16,
        [EnumMember]
        RestoredData = 32,
        [EnumMember]
        AvailableSpace = 64,
    }

    #region Rule
    public class SecurityRuleInfo
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public RMRuleType Type { get; set; }
        public bool IsChecked { get; set; }
        public bool IsExpand { get; set; }
        public bool IsLoaded { get; set; }
        public int SubPerIndex { get; set; }
        public int SubPerSize { get; set; }
        public int SubItemCount { get; set; }
        public List<SecurityRuleInfo> SubItems { get; set; }

    }
    public class QueryRuleObjResultDto
    {
        public List<SecurityRuleInfo> TermObjItems { get; set; }
        public int TotalCount { get; set; }
    }

    public enum SecurityRuleLevel
    {
        None = -1,
        All = 0,
        RuleContainer = 1,
        Rule = 2
    }

    public enum SecurityGroupValidateType
    {
        ValidateAll = 0,
        ValidateSourceContainerConflict = 1,
        ValidateTermConflict = 2,
        ValidateRuleConflict = 3,
        ValidateTermAssociationRuleMissing = 4,
        ValidateRuleAssociationTermMissing = 5,
        ValidateRuleAssociationNodeMissing = 6,
    }

    public enum DefaultAddedSecurityGroupType
    { 
        BuiltInEndUserGroup = 1,
        BuiltInReviewUserGroup = 2
    }
    #endregion
}
