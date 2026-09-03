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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.PhysicalCore.SQL
{

    public class RecordDBConstString
    {
        public const int DEFAULT_TIMEOUT = 180;
        public const int DEFAULT_COMMAND_TIMEOUT = 300;
    }
    public class RecordQueryString
    {

        public const string GetItemHoldStatus = "Select HoldStatus FROM {0} WITH(NOLOCK) where ScopeId=@ScopeId And NodeId=@NodeId";

        public const string GetRecordsByScope = "Select * from {0} WITH(NOLOCK) where ScopeId=@ScopeId And {1}=@NodeId";
        public const string GetFSRecordsByPathMD5 = "Select * from {0} WITH(NOLOCK) where NodeId=@NodeId";
        public const string GetRecord = "Select NodeType from {0} WITH(NOLOCK) where ScopeId=@ScopeId And NodeId=@NodeId";
        public const string ExistInArchivedTable = "Select COUNT(Id) from {0} WITH(NOLOCK) where ScopeId=@ScopeId And NodeId=@NodeId";
        public const string DeleteRecords = "Delete from {0} where ScopeId=@ScopeId And {1}=@NodeId";
        public const string DeleteFSRecords = "Delete from {0} where NodeId=@NodeId";
        public const string CreateArchivedRecord = @"INSERT INTO {0} ( LifecycleStatus,
                                                                        DestroyedTime1,
                                                                        ManualAdd,
                                                                        SourceFlag,
                                                                        ScopeId,
                                                                        NodeId,
                                                                        DirPath,
                                                                        RecordsId,
                                                                        NodeType,
                                                                        LeafName,
                                                                        ExtensionForFile,
                                                                        TermId,
                                                                        TermName,
                                                                        RuleId,
                                                                        RuleLevel,
                                                                        HoldStatus,
                                                                        RecordOwner,
                                                                        RelatedRecords,
                                                                        RelatedRecordsCount,
                                                                        CreatedBy,
                                                                        DisposalDueDate,
                                                                        DeclareAsRecord,
                                                                        TimeCreated1,
                                                                        TimeLastModified,
                                                                        CollectionTime,
                                                                        RecordHistory,
                                                                        AveSiteId,
                                                                        WebId,
                                                                        ListId,
                                                                        FolderId,
                                                                        ItemId,
                                                                        ItemRowId,
                                                                        FullPath,
                                                                        MetaInfo,
                                                                        ParentId
                                                                        )
                                                        VALUES(       @LifecycleStatus,
                                                                        @DestroyedTime1,
                                                                        @ManualAdd,
                                                                        @SourceFlag,
                                                                        @ScopeId,
                                                                        @NodeId,
                                                                        @DirPath,
                                                                        @RecordsId,
                                                                        @NodeType,
                                                                        @LeafName,
                                                                        @ExtensionForFile,
                                                                        @TermId,
                                                                        @TermName,
                                                                        @RuleId,
                                                                        @RuleLevel,
                                                                        @HoldStatus,
                                                                        @RecordOwner,
                                                                        @RelatedRecords,
                                                                        @RelatedRecordsCount,
                                                                        @CreatedBy,
                                                                        @DisposalDueDate,
                                                                        @DeclareAsRecord,
                                                                        @TimeCreated1,
                                                                        @TimeLastModified,
                                                                        @CollectionTime,
                                                                        @RecordHistory,
                                                                        @AveSiteId,
                                                                        @WebId,
                                                                        @ListId,
                                                                        @FolderId,
                                                                        @ItemId,
                                                                        @ItemRowId,
                                                                        @FullPath,
                                                                        @MetaInfo,
                                                                        @ParentId
                                                                        )";

        public const string CreateManagedRecord = @"INSERT INTO {0} (
                                                                        SourceFlag,
                                                                        ScopeId,
                                                                        NodeId,
                                                                        DirPath,
                                                                        RecordsId,
                                                                        NodeType,
                                                                        LeafName,
                                                                        ExtensionForFile,
                                                                        TermId,
                                                                        TermName,
                                                                        RuleId,
                                                                        RuleLevel,
                                                                        HoldStatus,
                                                                        RecordOwner,
                                                                        RelatedRecords,
                                                                        RelatedRecordsCount,
                                                                        CreatedBy,
                                                                        DisposalDueDate,
                                                                        DeclareAsRecord,
                                                                        TimeCreated1,
                                                                        TimeLastModified,
                                                                        CollectionTime,
                                                                        RecordHistory,
                                                                        AveSiteId,
                                                                        WebId,
                                                                        ListId,
                                                                        FolderId,
                                                                        ItemId,
                                                                        ItemRowId,
                                                                        FullPath,
                                                                        MetaInfo,
                                                                        ParentId)
                                                        VALUES(       
                                                                        @SourceFlag,
                                                                        @ScopeId,
                                                                        @NodeId,
                                                                        @DirPath,
                                                                        @RecordsId,
                                                                        @NodeType,
                                                                        @LeafName,
                                                                        @ExtensionForFile,
                                                                        @TermId,
                                                                        @TermName,
                                                                        @RuleId,
                                                                        @RuleLevel,
                                                                        @HoldStatus,
                                                                        @RecordOwner,
                                                                        @RelatedRecords,
                                                                        @RelatedRecordsCount,
                                                                        @CreatedBy,
                                                                        @DisposalDueDate,
                                                                        @DeclareAsRecord,
                                                                        @TimeCreated1,
                                                                        @TimeLastModified,
                                                                        @CollectionTime,
                                                                        @RecordHistory,
                                                                        @AveSiteId,
                                                                        @WebId,
                                                                        @ListId,
                                                                        @FolderId,
                                                                        @ItemId,
                                                                        @ItemRowId,
                                                                        @FullPath,
                                                                        @MetaInfo,
                                                                        ParentId
                                                                        )";
        public const string UpdateManagedRecord = "Update {0} Set RuleId=@RuleId, RuleLevel=@RuleLevel, DisposalDueDate=@DisposalDueDate where ScopeId=@ScopeId and ItemId=@ItemId";
        public const string GetMetaInfo = "Select MetaInfo from {0} WITH(NOLOCK) where ScopeId=@ScopeId And NodeId=@NodeId";
        public const string GetFakeSiteID = "Select NodeId from {0} WITH(NOLOCK) where ScopeId=@ScopeId";
        public const string UpdateManagedRecordForMoveTo = "Update {0} Set ScopeId=@ScopeId, NodeId=@NodeId, DirPath=@DirPath,AveSiteId=@AveSiteId, WebId=@WebId, ListId=@ListId, FolderId=@FolderId, ItemId=@ItemId, ItemRowId=@ItemRowId, FullPath=@FullPath, MetaInfo=@MetaInfo, DeclareAsRecord=@DeclareAsRecord where ScopeId=@SourceScopeId and ItemId=@SourceItemId";
        public const string GetTermInfosMappingByPathMD5 = "Select TermName,TermId,HoldStatus from {0} WITH(NOLOCK) where NodeId=@NodeId";//need add cache logic later

        #region setting lock records 
        //public static readonly string LockTableName = @"RMDeclaredSettingLocks";
        public static readonly string CheckTableExist = "SELECT COUNT(0) FROM sys.sysobjects WHERE id = OBJECT_ID(@TableName) AND OBJECTPROPERTY(id, N'IsUserTable')=1";
        public static readonly string GetTimeStamp = @"Select RowVersion, UpdateTime, Status, ID From {0} where ObjectName = @ObjectName";

        public static readonly string InsertLockerRecord = @"Insert into {0} 
(ObjectName, ID, Status, UpdateTime, ProcessName, ProcessId, ThreadId, ComputerName)
 Values (@ObjectName, @ID, @Status, @UpdateTime, @ProcessName, @ProcessId, @ThreadId, @ComputerName)";

        public static readonly string UpdateLockerRecord = @"Update {0} Set ID = @ID, Status = @Status, ProcessName = @ProcessName, ProcessId = @ProcessId, 
ThreadId = @ThreadId, ComputerName = @ComputerName, UpdateTime = @UpdateTime Where ObjectName = @ObjectName and RowVersion = @RowVersion";

        public static readonly string ReleaseLockerRecord = @"Update {0} Set Status = @Status, ProcessName = @ProcessName, ProcessId = @ProcessId, 
ThreadId = @ThreadId, ComputerName = @ComputerName, UpdateTime = @UpdateTime Where ObjectName = @ObjectName and ID = @ID";
        public static readonly string GetTenantUserAndDBSchema = @"Select RegisterEmail,DBSchema,DBName from RMTenantInfoes where Id = @Id ";

        public static readonly string UpdateRMRecordAlliancesTableRecordsId = @"Update {0} Set RecordsId = @desRecordsId where RecordsId = @sourceRecordsId";

        public static readonly string ApplySourceHoldInfoForDestination = @"if exists(select * from {0} where RecordsId=@sourceRecordsId)
                                                                               begin
                                                                                   Insert {0}
                                                                                   select  RecordsId= @desRecordsId,HoldId,HoldReleaseTime,HoldBy,AllianceType,BoxId,LocationId,Level  
                                                                                   from {0} 
                                                                                   where RecordsId=@sourceRecordsId
                                                                               end";

        public static readonly string GetRMSharePointSettingsInfoByScopeId = @"Select ColumnName, ExistColumnName, IsUsingExistColumnName From {0} where ScopeId = @ScopeId";

        public static readonly string GetRemoteSiteCollectionBySiteUrl = "select Id,DomainName,UserName,Password,Url,ParentId,State,AgentGroupId,AgentGroupName,Description,ModifiedDate,BposMode,CreateTime," +
                            "TemplateName,SPVersion,NodeLevel,Name,DisplayName,AvailableAgentIds,TemplateTitle,IsPublicWebSite,SiteCollectionType,AdminUrl,ServiceAccountId," +
                            "TenantId,AuthType,AppType,ScanSource,teamid,SecondParentId from {0} where Url = @siteUrl";
        public static readonly string DeleteSiteByUrl = "delete from {0} where Url = @siteUrl";

        public static readonly string GetRMSharePointSettingsInfoBySiteUrl = @"Select ColumnName, ExistColumnName, IsUsingExistColumnName From {0} s inner join {1} r on s.ScopeId  = r.ParentId where r.Url = @SiteUrl";

        public static readonly string GetPhysicalScheduleByLocationId = @"Select count(*) From {0} where JobCategory = 19 and IsRemoved = @IsRemoved and ProfileId = @ProfileId";

        public static readonly string IsRMRecordAllianceHold = @"Select count(*) From {0} where HoldReleaseTime > @HoldReleaseTime and RecordsId = @RecordsId";

        public static readonly string IsRMRecordLoanAllianceHold = @"Select count(*) From {0} where RecordsId = @RecordsId";

        public static readonly string CountSubLocation = @"Select count(*) From {0} where ParentId = @ParentId and IsRemoved = @IsRemoved";

        public static readonly string GetAllSubLocationByParentId = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0} where ParentId = @ParentId and IsRemoved = @IsRemoved Order By Name";

        public static readonly string GetAllLocations = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0}";

        public static readonly string GetAllRMScopePermissions = @"Select Id,Scope,ParentScope,ScopePath From {0}";

        public static readonly string GetAllRMTemplateRelationships = @"Select IdPath,Distance,Ancestor,Descendant,TemplateType From {0}";

        public static readonly string GetLocationById = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0} where Id = @Id and IsRemoved = @IsRemoved";

        public static readonly string GetLocationByName = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0} where Name = @Name and IsRemoved = @IsRemoved";

        public static readonly string GetLocationIdNameMapping = @"Select Id,Name From {0}";

        public static readonly string GetLocationByUniqueId = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0} where UniqueId = @UniqueId and IsRemoved = @IsRemoved";

        public static readonly string GetChildsLocationById = @"Select Id,UniqueId,ParentId,Name,Description,NodeType,IsRemoved,AvailableSpace,DirPath,MetaInfo,CreatedUserId,CreatedTime,ModifiedUserId,ModifiedTime From {0} where ParentId = @ParentId and IsRemoved = @IsRemoved";

        public static readonly string GetRMRuleById = @"Select Id,RuleId,RuleName,RuleLevel,DisposalAction,DeleteRecords,IsRemoved,Description,ModifyTime,ExchangeDisposalAction,PhysicalDisposalAction From {0} where RuleId = @RuleId";

        public static readonly string LoadCategories = @"Select Id,UniqueId,Name,TemplateId,TemplateUniqueId,LastModifiedOn,IsDefault From {0} where TemplateUniqueId = @TemplateUniqueId";

        public static readonly string GetTemplateByIdToDto = @"Select Id,UniqueId,Name,Type,Prefix,NumberOfDigits,ParentId,ParentUniqueId,Size,Creater,CreatedOn,Modifier,LastModifiedOn,ColumnSchema From {0} where Id = @Id";  

        public static readonly string GetTemplateByTemplateType = @"Select Id,UniqueId,Name,Type,Prefix,NumberOfDigits,ParentId,ParentUniqueId,Size,Creater,CreatedOn,Modifier,LastModifiedOn,ColumnSchema From {0} where Type = @Type";

        public static readonly string DeleteRecordAllianceById = @"Delete from {0} where RecordsId = @RecordsId";

        public static readonly string GetAllTermsForce = @"Select Id,TermSetId,UniqueId,Name,Description,IsDeprecated,IsRemoved,BreakInheritFromParent,TimeZoneId,RuleInfo,TermExpirationFrom,TermExpirationTo,IsRootTerm,IsDayLight,AvailableSpace,IsDefaultTerm,EnforceRetention,EXORetentionLabel,SPRetentionLabel,IsPermanent From {0}";

        public static readonly string GetAllRMTermSetMembership = @"Select TermId,TermSetId,ParentTermId,TermName,Path,IsSource,IsRemoved from {0} where IsRemoved = @IsRemoved";

        public static readonly string GetTermWithRule = @"Select Id,TermId,TermName,RuleId,RuleName,RuleLevel,RuleOrder from {0}";

        public static readonly string GetPhyRecordAlliance = @"Select Id,RecordsId,HoldReleaseTime,HoldBy,ParentId from {0}";

        public static readonly string GetPhyRecordAllianceById = @"Select Id,RecordsId,HoldReleaseTime,HoldBy,ParentId from {0} where RecordsId = @RecordsId";

        public static readonly string GetAllRMRecordAlliance = @"Select RecordsId,HoldId,HoldReleaseTime,HoldBy,AllianceType,BoxId,LocationId,Level from {0}";

        public static readonly string DeleteRMRecordAllianceByRecordsId = @"Delete from {0} where RecordsId=@RecordsId";

        public static readonly string GetPushColumns = @"Select Id,ColumnUniqueId,PhysicalObjectId,TemplateId,ColumnValue from {0} where ColumnUniqueId = @ColumnUniqueId";

        public static readonly string GetAllRMSuiteMemberships = @"Select Id,SuiteUniqueId,RootTemplateUniqueId,BoxTemplateUniqueId,FolderTemplateUniqueId,RecordTemplateUniqueId from {0}";

        public static readonly string GetAllRMSuites = @"Select Id,UniqueId,Name,Description,StartFromType,Creater,CreatedOn,Modifier,LastModifiedOn,RootTemplateCreateType from {0}";

        public static readonly string GetALLRMLocationSuiteAssociations = @"Select Id,LocationUniqueId,SuiteUniqueId from {0}";

        public static readonly string GetSuiteIdsByLocationID = @"Select SuiteUniqueId from {0} where LocationUniqueId=@LocationUniqueId";

        public static readonly string AddOneTemplateRelatonship = @"if not exists (select * from {0} where IdPath=@IdPath and Distance=@Distance)
                                                                               begin
                                                                                   Insert into {0}
                                                                                   (IdPath, Distance, Ancestor, Descendant, TemplateType) 
                                                                                   VALUES
                                                                                   (@IdPath, @Distance, @Ancestor, @Descendant, @TemplateType)
                                                                               end";
        public static readonly string GetStartTemplateUniqueId = @"select Descendant from {0} where Ancestor=@Ancestor and Distance=1";

        public static readonly string GetSuiteUniqueIdByRootTemplateId = @"select Ancestor from {0} where Descendant in (select Ancestor from {0} where Descendant=@Descendant and Distance=1) and Distance=0 and TemplateType=6";

        public static readonly string TemplateRelationExistByIdPath = @"select top 1 IdPath from {0} where IdPath in {1}";

        public static readonly string GetAllRMTemplates = @"Select Id,UniqueId,Name,Type,Prefix,NumberOfDigits,ParentId,ParentUniqueId,Size,Creater,CreatedOn,Modifier,LastModifiedOn,ColumnSchema From {0}";

        public static readonly string GetALLRMEXOLabels = @"Select Id,LabelName,Status,Type,LabelId,recordId,SavedTime From {0}";

        public static readonly string CreateLocation = @"INSERT INTO {0} (
                                                                        UniqueId,
                                                                        ParentId,
                                                                        Name,
                                                                        Description,
                                                                        NodeType,
                                                                        IsRemoved,
                                                                        AvailableSpace,
                                                                        DirPath,
                                                                        MetaInfo,
                                                                        CreatedUserId,
                                                                        CreatedTime,
                                                                        ModifiedUserId,
                                                                        ModifiedTime)
                                                        VALUES(       
                                                                        @UniqueId,
                                                                        @ParentId,
                                                                        @Name,
                                                                        @Description,
                                                                        @NodeType,
                                                                        @IsRemoved,
                                                                        @AvailableSpace,
                                                                        @DirPath,
                                                                        @MetaInfo,
                                                                        @CreatedUserId,
                                                                        @CreatedTime,
                                                                        @ModifiedUserId,
                                                                        @ModifiedTime
                                                                        )";
        public static readonly string UpdateLocation = @"Update {0} set UniqueId = @UniqueId,
                                                                        ParentId = @ParentId,
                                                                        Name = @Name,
                                                                        Description = @Description,
                                                                        NodeType = @NodeType,
                                                                        IsRemoved = @IsRemoved,
                                                                        AvailableSpace = @AvailableSpace,
                                                                        DirPath = @DirPath,
                                                                        MetaInfo = @MetaInfo,
                                                                        CreatedUserId = @CreatedUserId,
                                                                        CreatedTime = @CreatedTime,
                                                                        ModifiedUserId = @ModifiedUserId,
                                                                        ModifiedTime = @ModifiedTime 
                                                                        where UniqueId = @UniqueId";

        public static readonly string InsertRMRecordAlliance = @"INSERT INTO {0} (
                                                                        RecordsId,
                                                                        HoldId,
                                                                        HoldReleaseTime,
                                                                        HoldBy,
                                                                        AllianceType,
                                                                        BoxId,
                                                                        LocationId,
                                                                        Level)
                                                        VALUES(       
                                                                        @RecordsId,
                                                                        @HoldId,
                                                                        @HoldReleaseTime,
                                                                        @HoldBy,
                                                                        @AllianceType,
                                                                        @BoxId,
                                                                        @LocationId,
                                                                        @Level
                                                                        )";

        public static readonly string InsertRMLocationSuiteAssociation = @"INSERT INTO {0} (
                                                                        LocationUniqueId,
                                                                        SuiteUniqueId
                                                                        )
                                                        VALUES(       
                                                                        @LocationUniqueId,
                                                                        @SuiteUniqueId
                                                                        )";

        public static readonly string InsertRMSuiteMembership = @"INSERT INTO {0} (
                                                                        SuiteUniqueId,
                                                                        RootTemplateUniqueId,
                                                                        BoxTemplateUniqueId,
                                                                        FolderTemplateUniqueId,
                                                                        RecordTemplateUniqueId
                                                                        )
                                                        VALUES(       
                                                                        @SuiteUniqueId,
                                                                        @RootTemplateUniqueId,
                                                                        @BoxTemplateUniqueId,
                                                                        @FolderTemplateUniqueId,
                                                                        @RecordTemplateUniqueId
                                                                        )";

        public static readonly string InsertRMSuite = @"INSERT INTO {0} (
                                                                        UniqueId,
                                                                        Name,
                                                                        Description,
                                                                        StartFromType,
                                                                        Creater,
                                                                        CreatedOn,
                                                                        Modifier,
                                                                        LastModifiedOn,
                                                                        RootTemplateCreateType
                                                                        )
                                                        VALUES(       
                                                                        @UniqueId,
                                                                        @Name,
                                                                        @Description,
                                                                        @StartFromType,
                                                                        @Creater,
                                                                        @CreatedOn,
                                                                        @Modifier,
                                                                        @LastModifiedOn,
                                                                        @RootTemplateCreateType
                                                                        )";

        public static readonly string InsertRMTemplate = @"INSERT INTO {0} (
                                                                        UniqueId,
                                                                        Name,
                                                                        Type,
                                                                        Prefix,
                                                                        NumberOfDigits,
                                                                        ParentId,
                                                                        ParentUniqueId,
                                                                        Size,
                                                                        Creater,
                                                                        CreatedOn,
                                                                        Modifier,
                                                                        LastModifiedOn,
                                                                        ColumnSchema,
                                                                        Description
                                                                        )
                                                        VALUES(       
                                                                        @UniqueId,
                                                                        @Name,
                                                                        @Type,
                                                                        @Prefix,
                                                                        @NumberOfDigits,
                                                                        @ParentId,
                                                                        @ParentUniqueId,
                                                                        @Size,
                                                                        @Creater,
                                                                        @CreatedOn,
                                                                        @Modifier,
                                                                        @LastModifiedOn,
                                                                        @ColumnSchema,
                                                                        @Description
                                                                        )";

        public static readonly string UpdateRMRecordAllianceByRecordsId = @"Update {0} set RecordsId = @RecordsId,
                                                                        HoldId =@HoldId,
                                                                        HoldReleaseTime = @HoldReleaseTime,
                                                                        HoldBy = @HoldBy,
                                                                        AllianceType = @AllianceType,
                                                                        BoxId = @BoxId,
                                                                        LocationId = @LocationId,
                                                                        Level = @Level
                                                                        where RecordsId = @RecordsId";

        public static readonly string UpdateTemplateColumnsSchema = @"Update {0} set ColumnSchema = @ColumnSchema where UniqueId = @UniqueId";

        public static readonly string DeleteDifferentScopeDataFromManualApproveTable = @"Delete from {0} where NodeId=@NodeId And PartKey=@PartKey And Status = 1";

        public static readonly string UpdateRMScopePermission = @"Update {0} set Scope = @Scope, ParentScope = @ParentScope, ScopePath = @ScopePath where Id = @Id";
        #endregion
        #region fs related records info
        //public static readonly string GetRelatedRecordsInfo = @"Select RelatedRecordId from RMManagedRecordRelateds where CurrentRecordId = @CurrentRecordId";
        public static readonly string GetRelatedFileInfos = @"Select AveSiteId,DirPath,LeafName,NodeId,HoldStatus from RMManagedRecords where Id in (Select RelatedRecordId from RMManagedRecordRelateds where CurrentRecordId in (select id from RMManagedRecords where NodeId = @NodeId))";
        #endregion
    }
}