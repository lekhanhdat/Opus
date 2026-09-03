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


namespace AvePoint.Wrapper.QueryService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using System.Diagnostics.CodeAnalysis;

    [QueryCommandString(SPDatabaseVersion.SharePoint2016TAP1, QueryCommandType.Select)]
    internal static class SP2016SelectQueryString
    {
        #region backup restore

        #region 存储过程
        public const string GetBlobNumber_SELECT_mssqlrbs_rbs_fn_get_blob_number = @"[mssqlrbs].[rbs_fn_get_blob_number]";
        public const string GetBlobDetails_SELECT_mssqlrbs_rbs_sp_get_blob_details = AveRBSCommon.CMD_FETCH_RBS_BLOBID_AND_POOLID;
        public const string GetBlobId_SELECT_mssqlrbs_rbs_fn_get_blob_id = @"[mssqlrbs].[rbs_fn_get_blob_id]";
        #endregion

        #region SQL function
        public const string IsDBOwner_SELECT_IS_ROLEMEMBER = @"select IS_ROLEMEMBER('db_owner')";

        public const string TVF_ECMLanguage_PartitionId = "Select PartitionId, WorkingLanguageId, IsDefaultLanguage From TVF_ECMLanguage_PartitionId(@PartitionId)";

        public const string TVF_ECMPermission_PartitionIdGroupId = "SELECT PrincipalName,Rights FROM TVF_ECMPermission_PartitionIdGroupId(@PartitionId, 0)";

        #endregion

        #region RBS Table
        public const string GetBlobNumber_SELECT_mssqlrbs_resources_rbs_internal_blobs = @"SELECT blob_number FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH(NOLOCK)
                WHERE blob_store_id=@blob_store_id AND store_pool_id=@store_pool_id AND store_blob_id=@store_blob_id";

        public const string GetProviderId_SELECT_mssqlrbs_resources_rbs_internal_blob_stores = @"SELECT blob_store_id FROM [mssqlrbs_resources].[rbs_internal_blob_stores] WITH(NOLOCK) WHERE blob_store_name=@ProviderName";
        public const string GetCollectionId_SELECT_mssqlrbs_resources_rbs_internal_collections = @"SELECT collection_id FROM [mssqlrbs_resources].[rbs_internal_collections] WITH(NOLOCK) WHERE owning_application=@CollectionName";
        public const string GetPoolId_SELECT_mssqlrbs_resources_rbs_internal_pools = @"SELECT store_pool_id FROM [mssqlrbs_resources].[rbs_internal_pools] WITH(NOLOCK)";

        public const string IsBlobExist_SELECT_mssqlrbs_resources_rbs_internal_blobs = @"Select [blob_number] From [mssqlrbs_resources].[rbs_internal_blobs] WITH(NOLOCK) 
                Where [blob_store_id]=@blob_store_id AND [store_pool_id]=@store_pool_id AND [store_blob_id]=@store_blob_id AND collection_id=@collection_id";
        #endregion

        public const string GetSiteActivedUsers_SELECT_UserInfo = @" 
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
        Order by tp_ID";


        public const string GetSiteAllUsers_SELECT_UserInfo = @"
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
        FROM UserInfo WITH(NOLOCK)       
        WHERE tp_SiteID=@SiteId
        Order by tp_ID";
        public const string GetPrincipalIds_SELECT_RoleAssignment = @"SELECT distinct(PrincipalId) FROM  RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId And RoleId != 1073741825";

        public const string GetWebMemberUsers_SELECT_RoleAssignment_GroupMembership = @"
        SELECT distinct(principalId) as tp_ID FROM RoleAssignment WITH(NOLOCK)
        WHERE SiteId=@SiteId And Scopeid=@ScopeId
        UNION
        SELECT Distinct(MemberId) as tp_ID FROM GroupMembership WITH(NOLOCK)
        WHERE SiteId=@SiteId AND GroupId in(
                        SELECT PrincipalId FROM Roleassignment WITH(NOLOCK) 
                        WHERE SiteId=@SiteId And Scopeid=@ScopeId) order by PrincipalId";

        public const string GetWebUsers_SELECT_RoleAssignment = "SELECT distinct(principalId) as tp_ID FROM RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId And Scopeid=@ScopeId";

        public const string GetUsers_SELECT_GroupMembership = "SELECT Distinct(MemberId),GroupId FROM GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId";

        public const string GetSiteGroups_SELECT_Groups_RoleAssignment = @"
        SELECT distinct(ID),Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags,Convert(bit,isnull(r.RoleId,0)) as HasPermission
        FROM Groups g WITH(NOLOCK)
		LEFT JOIN RoleAssignment r WITH(NOLOCK) ON g.SiteId = r.SiteId AND g.ID=r.PrincipalId AND r.ScopeId=@ScopeId
		WHERE g.SiteId=@SiteId";

        public const string GetWebGroups_SELECT_Groups_RoleAssignment = @"
        SELECT distinct(ID),Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags
        From Groups g WITH(NOLOCK) 
        INNER JOIN RoleAssignment r WITH(NOLOCK) ON g.SiteId=r.SiteId AND g.ID=r.PrincipalId
		WHERE g.SiteId=@SiteId AND r.ScopeId=@ScopeId";

        public const string GetWebAllUsers_SELECT_UserInfo = @"
        SELECT tp_ID
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId
        Order by tp_ID";

        public const string GetGroupMembership_SELECT_GroupMembership = @"SELECT GroupId,MemberId From GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId ORDER BY GroupId,MemberId";

        public const string GetSiteSetting_SELECT_AllSites = @"
        SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,TimeCreated,UsersCount,
               BWUsed,DiskUsed,SecondStageDiskUsed,QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
               CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
               SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,LastContentChange,
               LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,UserIsActiveFieldRowOrdinal,
               UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,DomainGroupMapVersion,
               DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
        FROM AllSites With(nolock) WHERE Id=@SiteId AND Deleted = CONVERT(bit, 0)";

        public const string GetSiteDiskUsed_SELECT_AllSites = @"SELECT DiskUsed FROM AllSites WITH(NOLOCK) WHERE Id=@SiteId AND Deleted = CONVERT(bit, 0)";

        public const string GetWebMetaInfo_SELECT_AllWebs = @"SELECT MetaInfo FROM AllWebs With(nolock) WHERE SiteId=@SiteId AND Id = @Id AND DeleteTransactionId=0x";

        public const string GetWebFullUrlById_SELECT_AllWebs = @"Select FullUrl From AllWebs With(noLock) Where SiteId=@SiteId AND Id=@Id And DeleteTransactionId=0x";

        public const string GetPageUrlById_SELECT_AllDocs = @"Select DirName+'/'+LeafName From AllDocs With(noLock) Where SiteId =@SiteId and DeleteTransactionId=0x and Id=@Id";

        public const string GetWebSetting_SELECT_AllWebs = @"
        SELECT Author, Title, TimeCreated, Description, SecurityProvider, MetaInfo, MetaInfoVersion, LastMetadataChange, NavStructNextEid, 
               NextWebGroupId, DefTheme, AlternateCSSUrl, CustomizedCss, CustomJSUrl, AlternateHeaderUrl, DailyUsageData, DailyUsageDataVersion, 
               MonthlyUsageData, MonthlyUsageDataVersion, DayLastAccessed, Language, Locale, TimeZone, Time24, CalendarType, AdjustHijriDays, 
               ProvisionConfig, Flags,MasterUrl,CustomMasterUrl, Collation, RequestAccessEmail, SiteLogoUrl, SiteLogoDescription, AuditFlags, 
               InheritAuditFlags, Ancestry, AltCalendarType, CalendarViewOptions, WorkDayStartHour, WorkDayEndHour,WorkDays 
        FROM AllWebs With(nolock) WHERE SiteId=@SiteId AND Id=@WebId AND DeleteTransactionId=0x";

        public const string GetListInfo_SELECT_AllLists = @"
        SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
               tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
               tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, 
               tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
               tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
               tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
               tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
               tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID
       FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_WebId=@WebId AND tp_Id=@ListId";

        public const string GetSubWebFullUrlId_SELECT_AllWebs = "select FullUrl,Id from AllWebs With(noLock) where SiteId=@SiteId And ParentWebId=@ParentWebId And DeleteTransactionId=0x";

        public const string GetSubPageUrlId_SELECT_AllDocs = "select DirName+'/'+LeafName as FullUrl,Id from AllDocs With(noLock) where SiteId=@SiteId and DeleteTransactionId=0x and ListId=@ListId and LeafName like '%.aspx' and DoclibRowId is not null";
        
        public const string GetRoleAssignment_SELECT_RoleAssignment = @"
                   SELECT RoleId,PrincipalId FROM RoleAssignment r WITH(NOLOCK) 
                   Inner Join UserInfo u WITH(NOLOCK)
                   on u.tp_SiteID=r.SiteId AND u.tp_ID = r.PrincipalId 
                   WHERE  r.SiteId = @SiteId AND r.ScopeId=@ScopeId  AND u.tp_Deleted = 0
                   UNION ALL
                   SELECT RoleId, PrincipalId  from RoleAssignment r WITH(NOLOCK)
                   Inner Join Groups g WITH(NOLOCK) 
                   on g.SiteId = r.SiteId AND g.ID = r.PrincipalId 
                   WHERE r.SiteId = @SiteId  AND r.ScopeId = @ScopeId
                   order by PrincipalId ASC";

        public const string GetUserDataFormat_SELECT_AllUserData = @"
                   SELECT tp_ID,tp_RowOrdinal,tp_Version,tp_Author,tp_Editor,tp_Modified,tp_Created,tp_Ordering,tp_ThreadIndex,
                   tp_HasAttachment,tp_ModerationStatus,tp_IsCurrent,tp_ItemOrder,tp_InstanceID,tp_GUID,tp_CopySource,
                   tp_HasCopyDestinations,tp_AuditFlags,tp_InheritAuditFlags,tp_Size,tp_WorkflowVersion,tp_WorkflowInstanceID,
                   tp_ContentTypeId,tp_Level,tp_IsCurrentVersion,tp_UIVersion,tp_UIVersionString,tp_CalculatedVersion,tp_DraftOwnerId,tp_CheckoutUserId,tp_AppAuthor,tp_AppEditor{0}
                   FROM  AllUserData WITH(NOLOCK)
                   WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND
                   tp_ParentId = @ParentId AND tp_DocId = @DocId AND tp_UIVersion = @Version";

        [QueryCommandArgument(Arguments = new object[] { ",$additionalColName1,$additionalColName2" })]
        public static string GetUserData_SELECT_AllUserData(string additionalColNames)
        {
            return string.Format(GetUserDataFormat_SELECT_AllUserData, additionalColNames);
        }

        public const string GetItemRowIdByThreadIndex_SELECT_AllUserData = @"select tp_ID from AllUserData WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1)  AND tp_ListId=@ListId AND tp_ThreadIndex =@ThreadIndex";

        public const string GetItemEditor_SELECT_AllUserData = @"SELECT tp_Editor FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_parentid=@ParentId AND tp_DocId=@DocId";
        public const string GetItemAuthor_SELECT_AllUserData = @"SELECT tp_Author FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_parentid=@ParentId AND tp_DocId=@DocId";
        public const string GetLookupRelationships_SELECT_AllLookupRelationships = @"SELECT FieldId FROM AllLookupRelationships WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND FieldId = @FieldId";

        public const string GetViewSchemas_SELECT_AllWebParts = @"select tp_View from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteId and tp_ListId=@ListId and tp_Type=0";
        public const string GetRoles_SELECT_Roles = @"SELECT RoleId,Title,Description,PermMask,PermMaskDeny,Hidden,Type,WebGroupId,RoleOrder FROM Roles WITH(NOLOCK) WHERE SiteId=@SiteId and WebId=@WebId";

        public const string GetCTResourceFiles_SELECT_AllDocs_DocStreams = @"
             SELECT Content, LeafName from AllDocs WITH(NOLOCK)
             INNER JOIN DocStreams WITH(NOLOCK) on AllDocs.SiteId = DocStreams.SiteId AND AllDocs.Id = DocStreams.DocId 
             WHERE AllDocs.SiteId=@SiteId AND DeleteTransactionId=0x AND DirName=@DirName";

        public const string GetWebParts_SELECT_AllWebParts = @"
            SELECT wp.tp_ID,wp.tp_ListId,wp.tp_Type,wp.tp_Flags,wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_Version,wp.tp_PartOrder,wp.tp_ZoneID,
                   wp.tp_IsIncluded,wp.tp_FrameState,wp.tp_View,wp.tp_WebPartTypeId,wp.tp_AllUsersProperties,wp.tp_PerUserProperties,
                   wp.tp_Cache,wp.tp_UserID,wp.tp_Source,wp.tp_CreationTime,wp.tp_Size,wp.tp_Level,wp.tp_Deleted,wp.tp_HasFGP,
                   wp.tp_ContentTypeId,wp.tp_PageVersion,wp.tp_SolutionId,wp.tp_IsCurrentVersion,wp.tp_Assembly,wp.tp_Class,wp.tp_WebPartIdProperty
            FROM AllWebParts wp WITH(NOLOCK)
            WHERE wp.tp_SiteId=@SiteId AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND wp.tp_PageUrlId=@Id AND wp.tp_Level=@Level AND wp.tp_PageVersion=@PageVersion"; // order by wp.tp_PartOrder ASC

        //连web表只是为了去掉被删除到回收站中的
        public const string GetWebpartGallery_SELECT_AllLists_AllWebs = @"SELECT L.tp_ID AS GalleryListId FROM AllLists L With(nolock) JOIN AllWebs W With(nolock) ON L.tp_SiteId = W.SiteId AND L.tp_WebId = W.Id AND W.DeleteTransactionId=0x WHERE W.SiteId = @SiteID AND L.tp_ServerTemplate=113";

        public const string GetWebPartsTemplate_SELECT_AllUserData = @"SELECT nvarchar9 as WebPartName, nvarchar8 as Assembly, nvarchar7 as Title, nvarchar10 as Image, ntext2 as Description, nvarchar3 as FileType, nvarchar11 as Category FROM AllUserData With(nolock) WHERE tp_ListId = @ListID";

        public const string GetListTitle_SELECT_AllLists = @"SELECT tp_Title FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_Id=@ListId";

        public const string GetWebpartLists_SELECT_WebPartLists_AllWebs = @"SELECT wp.tp_WebId,wp.tp_UserID,wp.tp_Level, w.FullUrl AS tp_FullUrl
                        FROM WebPartLists wp With(nolock) LEFT JOIN AllWebs w With(nolock) ON wp.tp_WebId=w.Id AND w.DeleteTransactionId=0x WHERE tp_SiteId=@SiteId and tp_PageUrlID=@Id AND tp_Level=@Level AND tp_WebPartID=@WebPartID";

        public const string GetWebpartPersonalization_SELECT_Personalization = @"SELECT tp_UserID,tp_PartOrder,tp_ZoneID,tp_IsIncluded,tp_FrameState,tp_PerUserProperties,tp_Cache,tp_Size,tp_Deleted 
                        FROM Personalization WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID";

        public const string GetVersion_SELECT_AllDocVersions = @"SELECT UIVersion,InternalVersion,TimeCreated,DocFlags,MetaInfoSize,Size,MetaInfo,CheckinComment,
                                 Level,DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo
                        FROM  AllDocVersions WITH(NOLOCK)
                        WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x AND UIVersion=@Version";

        public const string GetVersionUserData_SELECT_AllUserData = @"
               SELECT tp_Modified as TimeLastModified,tp_Created as TimeCreated,tp_ParentId as ParentId
                     ,tp_DocId as Id,tp_DeleteTransactionId as DeleteTransactionId,tp_Level as Level,tp_IsCurrentVersion as IsCurrentVersion
                     ,tp_UIVersion as UIVersion,tp_DraftOwnerId as DraftOwnerId,tp_CheckoutUserId as CheckoutUserId
               FROM  AllUserData WITH(NOLOCK)
               WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ID=@Id AND tp_UIVersion=@Version";


        public const string GetHasStream_SELECT_AllDocs = @"SELECT HasStream FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0X AND ParentId=@ParentId AND Id=@Id AND Level=@Level";
        public const string GetHasStream_SELECT_AllDocVersions = @"SELECT HasStream FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id";
        public const string GetAttachmentInfoWithParentId_SELECT_AllDocs = @"SELECT LeafName as Title, TimeCreated as Created, TimeLastModified as Modified, MetaInfo FROM AllDocs WITH(NOLOCK)
            WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentID=@ParentID AND Id=@Id AND UIVersion=@Version";
        public const string GetAttachmentInfoWithoutParentId_SELECT_AllDocs = @"SELECT LeafName as Title, TimeCreated as Created, TimeLastModified as Modified, MetaInfo FROM AllDocs WITH(NOLOCK)
            WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND Id=@Id AND UIVersion=@Version";


        private const string GetDocAndUserInfoFormat_SELECT_AllDocs_AllUserData = @"SELECT top {0}
doc.Id as DOC#Id,doc.DirName as DOC#DirName,doc.LeafName as DOC#LeafName,doc.DoclibRowId as DOC#DoclibRowId,doc.Type as DOC#Type,doc.SortBehavior as DOC#SortBehavior,doc.Size as DOC#Size,doc.UIVersion as DOC#UIVersion,doc.Dirty as DOC#Dirty,doc.ListDataDirty as DOC#ListDataDirty,doc.DocFlags as DOC#DocFlags,doc.ThicketFlag as DOC#ThicketFlag,doc.CharSet as DOC#CharSet,doc.ProgId as DOC#ProgId,doc.TimeCreated as DOC#TimeCreated,doc.TimeLastModified as DOC#TimeLastModified,doc.NextToLastTimeModified as DOC#NextToLastTimeModified,doc.MetaInfoTimeLastModified as DOC#MetaInfoTimeLastModified,doc.TimeLastWritten as DOC#TimeLastWritten,doc.SetupPathVersion as DOC#SetupPathVersion,doc.SetupPath as DOC#SetupPath,doc.SetupPathUser as DOC#SetupPathUser,doc.CheckoutUserId as DOC#CheckoutUserId,doc.CheckoutDate as DOC#CheckoutDate,doc.CheckoutExpires as DOC#CheckoutExpires,doc.VersionCreatedSinceSTCheckout as DOC#VersionCreatedSinceSTCheckout,doc.LTCheckoutUserId as DOC#LTCheckoutUserId,doc.VirusVendorID as DOC#VirusVendorID,doc.VirusStatus as DOC#VirusStatus,doc.VirusInfo as DOC#VirusInfo,doc.MetaInfo as DOC#MetaInfo,doc.MetaInfoSize as DOC#MetaInfoSize,doc.MetaInfoVersion as DOC#MetaInfoVersion,doc.UnVersionedMetaInfo as DOC#UnVersionedMetaInfo,doc.UnVersionedMetaInfoSize as DOC#UnVersionedMetaInfoSize,doc.UnVersionedMetaInfoVersion as DOC#UnVersionedMetaInfoVersion,doc.WelcomePageUrl as DOC#WelcomePageUrl,doc.WelcomePageParameters as DOC#WelcomePageParameters,doc.IsCurrentVersion as DOC#IsCurrentVersion,doc.Level as DOC#Level,doc.CheckinComment as DOC#CheckinComment,doc.AuditFlags as DOC#AuditFlags,doc.InheritAuditFlags as DOC#InheritAuditFlags,doc.DraftOwnerId as DOC#DraftOwnerId,doc.UIVersionString as DOC#UIVersionString,doc.ParentId as DOC#ParentId,doc.HasStream as DOC#HasStream,doc.ScopeId as DOC#ScopeId,doc.BuildDependencySet as DOC#BuildDependencySet,doc.ParentVersion as DOC#ParentVersion,doc.ParentVersionString as DOC#ParentVersionString,doc.TransformerId as DOC#TransformerId,doc.ParentLeafName as DOC#ParentLeafName,doc.IsCheckoutToLocal as DOC#IsCheckoutToLocal,doc.CtoOffset as DOC#CtoOffset,doc.Extension as DOC#Extension,doc.ExtensionForFile as DOC#ExtensionForFile,doc.ItemChildCount as DOC#ItemChildCount,doc.FolderChildCount as DOC#FolderChildCount,doc.FileFormatMetaInfo as DOC#FileFormatMetaInfo,doc.FileFormatMetaInfoSize as DOC#FileFormatMetaInfoSize,doc.ListSchemaVersion as DOC#ListSchemaVersion,doc.ClientId as DOC#ClientId,doc.InternalVersion as DOC#InternalVersion,doc.BumpVersion as DOC#BumpVersion,
data.tp_ID as UD#tp_ID,data.tp_RowOrdinal as UD#tp_RowOrdinal,data.tp_Version as UD#tp_Version,data.tp_UIVersionString as UD#tp_UIVersionString,data.tp_Author as UD#tp_Author,data.tp_Editor as UD#tp_Editor,data.tp_Modified as UD#tp_Modified,data.tp_Created as UD#tp_Created,data.tp_Ordering as UD#tp_Ordering,data.tp_ThreadIndex as UD#tp_ThreadIndex,data.tp_HasAttachment as UD#tp_HasAttachment,data.tp_ModerationStatus as UD#tp_ModerationStatus,data.tp_IsCurrent as UD#tp_IsCurrent,data.tp_ItemOrder as UD#tp_ItemOrder,data.tp_InstanceID as UD#tp_InstanceID,data.tp_GUID as UD#tp_GUID,data.tp_CopySource as UD#tp_CopySource,data.tp_HasCopyDestinations as UD#tp_HasCopyDestinations,data.tp_AuditFlags as UD#tp_AuditFlags,data.tp_InheritAuditFlags as UD#tp_InheritAuditFlags,data.tp_Size as UD#tp_Size,data.tp_WorkflowVersion as UD#tp_WorkflowVersion,data.tp_WorkflowInstanceID as UD#tp_WorkflowInstanceID,{1}data.tp_ContentTypeId as UD#tp_ContentTypeId
FROM AllDocs as doc WITH(NOLOCK) LEFT JOIN AllUserData as data WITH(NOLOCK) 
ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1) AND data.tp_ParentId=doc.ParentId AND data.tp_DocId = doc.Id AND data.tp_UIVersion = doc.UIVersion
WHERE doc.SiteId = @SiteId AND doc.ParentId = @ParentId AND doc.DeleteTransactionId = 0x AND
doc.Type = 0 AND doc.DoclibRowId >= @CurrentDoclibRowId
ORDER BY doc.DoclibRowId";

        public const string GetDeleteTransactionId_SELECT_AllDocVersions = @"SELECT DeleteTransactionId FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";

        public const string GetDeleteTransactionId_SELECT_AllDocs = @"SELECT DeleteTransactionId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND (DeleteTransactionId<>0x or DeleteTransactionId = 0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";

        public const string GetDeleteTransactionId_SELECT_AllUserData = @"SELECT tp_DeleteTransactionId FROM AllUserData  WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";

        public const string GetDocInfoForInsertToAllDocVersion_SELECT_AllDocs = @"
SELECT SiteId,Id,UIVersion,TimeCreated,DocFlags,MetaInfoSize,Size,Level,
       DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo,SetupPathVersion
FROM AllDocs WITH(NOLOCK)
WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion";



        public const string GetDocInfoForInsertToAllDocs_SELECT_AllDocs = @"SELECT [Id] ,[SiteId],[DirName]
      ,[LeafName],[WebId],[ListId] ,[DoclibRowId],[Type] ,[SortBehavior] ,[Size],[ETagVersion]
      ,[EffectiveVersion],[BumpVersion],[UIVersion]
      ,[ListDataDirty],[DocFlags],[ThicketFlag]
      ,[CharSet],[ProgId],[TimeCreated],[TimeLastModified] ,[NextToLastTimeModified]
      ,[MetaInfoTimeLastModified] ,[TimeLastWritten],[DeleteTransactionId]
      ,[SetupPathVersion],[SetupPath],[SetupPathUser] ,[CheckoutUserId]
      ,[CheckoutDate],[CheckoutExpires] ,[VersionCreatedSinceSTCheckout],[LTCheckoutUserId]
      ,[VirusVendorID],[VirusStatus],[VirusInfo],[MetaInfo],[MetaInfoSize]
      ,[MetaInfoVersion],[UnVersionedMetaInfo] ,[UnVersionedMetaInfoSize]
      ,[UnVersionedMetaInfoVersion],[WelcomePageUrl] ,[WelcomePageParameters]
      ,[IsCurrentVersion],[Level] ,[AuditFlags] ,[InheritAuditFlags]
      ,[DraftOwnerId],[UIVersionString] ,[ParentId] ,[HasStream]
      ,[ScopeId],[BuildDependencySet] ,[ParentVersion]
      ,[ParentVersionString],[TransformerId],[ParentLeafName]
      ,[IsCheckoutToLocal],[CtoOffset],[Extension] ,[ExtensionForFile] ,[ItemChildCount]
      ,[FolderChildCount],[FileFormatMetaInfo] ,[FileFormatMetaInfoSize]
      ,[FFMConsistent] ,[ContentVersion],[ListSchemaVersion] ,[ClientId]
FROM AllDocs WITH(NOLOCK)
WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion";


        public const string GetDocInfoForInsertToAllUserData_SELECT_AllUserData_1 = @"select tp_RowOrdinal FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_Id=@RowId AND tp_UIVersion=@UIVersion order by tp_RowOrdinal ASC";



        public const string GetDocInfoForInsertToAllUserData_SELECT_AllUserData_2 = @"SELECT [tp_ID],[tp_ListId],[tp_SiteId],[tp_RowOrdinal]
      ,[tp_Version],[tp_Author],[tp_Editor] ,[tp_Modified] ,[tp_Created]
      ,[tp_Ordering],[tp_ThreadIndex],[tp_HasAttachment],[tp_ModerationStatus]
      ,[tp_IsCurrent],[tp_ItemOrder],[tp_InstanceID],[tp_GUID] ,[tp_CopySource]
      ,[tp_HasCopyDestinations] ,[tp_AuditFlags],[tp_InheritAuditFlags]
      ,[tp_Size],[tp_WorkflowVersion],[tp_WorkflowInstanceID],[tp_ParentId]
      ,[tp_DocId],[tp_DeleteTransactionId],[tp_ContentTypeId]
      ,[tp_Level] ,[tp_IsCurrentVersion],[tp_UIVersion] ,[tp_CalculatedVersion]
      ,[tp_UIVersionString],[tp_DraftOwnerId] ,[tp_CheckoutUserId]
FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_Id=@RowId AND tp_UIVersion=@UIVersion AND tp_RowOrdinal=@rowOrdinal";

        [QueryCommandArgument(Arguments = new object[] { 1000, "$additionalColName1,$additionalColName2," })]
        public static string GetDocAndUserInfoBulk_SELECT_AllDocs_AllUserData(int maxRow, string additionalColNames)
        {
            return string.Format(GetDocAndUserInfoFormat_SELECT_AllDocs_AllUserData, maxRow, additionalColNames);
        }

        private const string GetVersionAndUserInfoFormat_SELECT_AllDocVersions_AllDocs_AllUserData = @"SELECT TOP {0}
doc.DoclibRowId AS DOC#DoclibRowId,
docver.UIVersion as VER#UIVersion,docver.InternalVersion as VER#InternalVersion,docver.TimeCreated as VER#TimeCreated,docver.DocFlags as VER#DocFlags,docver.MetaInfoSize as VER#MetaInfoSize,docver.Size as VER#Size,docver.MetaInfo as VER#MetaInfo,docver.CheckinComment as VER#CheckinComment,docver.Level as VER#Level,docver.DraftOwnerId as VER#DraftOwnerId,docver.DeleteTransactionId as VER#DeleteTransactionId,docver.VirusVendorID as VER#VirusVendorID,docver.VirusStatus as VER#VirusStatus,docver.VirusInfo as VER#VirusInfo,
data.tp_ID as UD#tp_ID,data.tp_RowOrdinal as UD#tp_RowOrdinal,data.tp_Version as UD#tp_Version,data.tp_Author as UD#tp_Author,data.tp_Editor as UD#tp_Editor,data.tp_Modified as UD#tp_Modified,data.tp_Created as UD#tp_Created,data.tp_Ordering as UD#tp_Ordering,data.tp_ThreadIndex as UD#tp_ThreadIndex,data.tp_HasAttachment as UD#tp_HasAttachment,data.tp_ModerationStatus as UD#tp_ModerationStatus,data.tp_IsCurrent as UD#tp_IsCurrent,data.tp_ItemOrder as UD#tp_ItemOrder,data.tp_InstanceID as UD#tp_InstanceID,data.tp_GUID as UD#tp_GUID,data.tp_CopySource as UD#tp_CopySource,data.tp_HasCopyDestinations as UD#tp_HasCopyDestinations,data.tp_AuditFlags as UD#tp_AuditFlags,data.tp_InheritAuditFlags as UD#tp_InheritAuditFlags,data.tp_Size as UD#tp_Size,data.tp_WorkflowVersion as UD#tp_WorkflowVersion,data.tp_WorkflowInstanceID as UD#tp_WorkflowInstanceID,data.tp_ContentTypeId as UD#tp_ContentTypeId,{1}data.tp_Level as UD#tp_Level,data.tp_IsCurrentVersion as UD#tp_IsCurrentVersion,data.tp_UIVersion as UD#tp_UIVersion,data.tp_CalculatedVersion as UD#tp_CalculatedVersion,data.tp_DraftOwnerId as UD#tp_DraftOwnerId,data.tp_CheckoutUserId as UD#tp_CheckoutUserId
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND data.tp_IsCurrentVersion=0 
AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND data.tp_CalculatedVersion=docver.UIVersion and data.tp_Level=docver.Level
WHERE doc.SiteId = @SiteId AND doc.ParentId = @ParentId AND doc.DeleteTransactionId = 0x AND
doc.Type = 0 AND doc.DoclibRowId >= @CurrentDoclibRowId
ORDER BY doc.DoclibRowId";

        [QueryCommandArgument(Arguments = new object[] { 1000, "$additionalColName1,$additionalColName2," })]
        public static string GetVersionAndUserInfoBulk_SELECT_AllDocVersions_AllDocs_AllUserData(int maxRow, string additionalColNames)
        {
            return string.Format(GetVersionAndUserInfoFormat_SELECT_AllDocVersions_AllDocs_AllUserData, maxRow, additionalColNames);
        }

        public const string GetRbsId_SELECT_DocStreams_DocsToStreams = @"select RbsId from DocStreams ds with(nolock)
                inner join DocsToStreams dts with(nolock) on ds.SiteId = dts.SiteId and ds.DocId = dts.DocId and ds.Partition = dts.Partition and ds.BSN = dts.BSN
                where ds.SiteId = @SiteId and ds.DocId = @ID and dts.HistVersion = @HistVersion and dts.Level = @Level and RbsId is not null";

        public const string GetRbsIdList_SELECT_DocStreams_DocsToStreams = @"select RbsId,ds.BSN,ds.Partition,ds.Size,ds.Type,ds.ExpirationUTC,dts.StreamId from DocStreams ds with(nolock)
                inner join DocsToStreams dts with(nolock) on ds.SiteId = dts.SiteId and ds.DocId = dts.DocId and ds.Partition = dts.Partition and ds.BSN = dts.BSN
                where ds.SiteId = @SiteId and ds.DocId = @ID and dts.HistVersion = @HistVersion and dts.Level = @Level";

        public const string GetFields_SELECT_AllLists = @"select tp_Fields from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_WebId=@WebId and tp_ID=@ListId";
        public const string GetViewFields_SELECT_AllWebParts = @"select tp_View from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteId and (tp_IsCurrentVersion = 1 or tp_IsCurrentVersion =0) and tp_ListId=@ListId and tp_Type=0"; //0 means the webpart is in default view
        public const string GetStubInfo_SELECT_DocStreams = @"SELECT DATALENGTH(Content),Content FROM DocStreams WITH(NOLOCK) WHERE SiteId=@SiteId AND DocId=@Id AND InternalVersion=@InternalVersion";

        public const string GetDataJunctions_SELECT_AllUserDataJunctions = @"SELECT tp_FieldId,tp_Id,tp_UIVersion,tp_Ordinal,tp_SourceListId
                FROM AllUserDataJunctions WITH(NOLOCK)
                WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@DocId AND tp_UIVersion=@Version
                ORDER BY tp_FieldId,tp_Ordinal";

        private const string GetUserDataJunctionsFormat_SELECT_AllUserDataJunctions = @"SELECT TOP({0}) tp_FieldId,tp_Id,tp_UIVersion,tp_Ordinal,tp_SourceListId,tp_DocId
                FROM AllUserDataJunctions WITH(NOLOCK)
                WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion>=0 AND tp_ParentId=@ParentId
                ORDER BY tp_DocId,tp_UIVersion,tp_FieldId,tp_Ordinal";

        [QueryCommandArgument(Arguments = new object[] { 1000 })]
        public static string GetUserDataJunctionsBulk_SELECT_AllUserDataJunctions(int maxRow)
        {
            return string.Format(GetUserDataJunctionsFormat_SELECT_AllUserDataJunctions, maxRow);
        }

        public const string GetCTName_SELECT_ContentTypes = @"select ResourceDir,Definition 
                from ContentTypes With(nolock) 
                where SiteId=@SiteId and Class=1 and ContentTypeId=@ContentTypeId and DeleteTransactionId = 0x";

        public const string GetWebTemplate_SELECT_AllWebs = @"SELECT Id,WebTemplate,ProvisionConfig FROM AllWebs With(nolock) WHERE SiteId=@SiteId AND DeleteTransactionId=0x";

        public const string GetContentTypeInfoById_SELECT_ContentTypes = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes With(nolock)
                                       where SiteId=@SiteId and Class=1 and ContentTypeId=@ContentTypeId and DeleteTransactionId = 0x";

        public const string GetContentTypeInfoByScope_SELECT_ContentTypes = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes With(nolock)
                                       where SiteId=@SiteId and Class=1 and Scope=@Scope and DeleteTransactionId = 0x";

        public const string GetListCTSchemaByListID_SELECT_AllLists = @"select tp_ContentTypes from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_WebId=@WebId and tp_ID=@ListId";

        public const string GetAllListCTSchema_SELECT_AllLists = @"select tp_ContentTypes from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_WebId=@WebId and tp_ID=@ListId";


        public const string GetListViews_SELECT_AllWebParts_AllDocs = @"select tp_ID,tp_DisplayName,tp_Type,tp_PageUrlID,tp_Flags,tp_BaseViewID,tp_UserID,tp_View
                from AllWebParts webpart WITH(NOLOCK)
                inner join AllDocs  docs WITH(NOLOCK) on docs.SiteId =  webpart.tp_SiteId and docs.Id = webpart.tp_PageUrlID and docs.DoclibRowId is  null
                where tp_SiteId=@SiteId AND tp_ListId =@listid AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND (tp_Type=1  or tp_Type=0) AND tp_DisplayName<> '' ";

        public const string GetFieldsSchema_SELECT_ContentTypes = @"select Definition from ContentTypes With(nolock) 
                where SiteId=@SiteId and Class=0 and Scope=@Scope and Definition is not null and DeleteTransactionId = 0x";

        public const string GetCTCountById_SELECT_ContentTypes = @"SELECT COUNT(ContentTypeId) FROM ContentTypes With(nolock) WHERE SiteId=@SiteId AND Class=1 AND ContentTypeId=@ContentTypeId AND DeleteTransactionId=0x";

        public const string GetFieldCountById_SELECT_ContentTypes = @"SELECT count(ContentTypeId) FROM contenttypes With(nolock)
                                WHERE siteid=@SiteId And Class=0 AND cast(ContentTypeId as uniqueidentifier)=@FieldId 
                                        AND  Scope like @Scope AND DeleteTransactionId=0x";

        public const string GetCTCountByScope_SELECT_ContentTypes = @"SELECT COUNT(ContentTypeId) FROM TVF_ContentTypes_SiteClassCTId(@SiteId, 1, @ContentTypeId) WHERE SCOPE LIKE @Scope";

        public const string GetWebPartId_SELECT_AllWebParts = @"select tp_ID from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_Level=@Level AND tp_UserID > 0";

        public const string GetCTNameById_SELECT_ContentTypes = @"SELECT ResourceDir FROM ContentTypes With(nolock) WHERE SiteId=@SiteId AND Class=1 AND DeleteTransactionId=0x AND ContentTypeId=@ContentTypeId";

        public const string GetListSettings_SELECT_AllLists = @"
        SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
               tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
               tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, tp_Flags2,
               tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
               tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
               tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
               tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
               tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID,tp_RootFolder
        FROM AllLists WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId and tp_WebId=@WebId and tp_Id=@ListId";


        public const string GetItemBasicInfo_SELECT_AllDocs = @"SELECT CharSet,TimeCreated,TimeLastModified,MetaInfo,Dirty,DocFlags,WelcomePageUrl 
                    FROM AllDocs WITH (NOLOCK, INDEX=Docs_IdLevelUnique) WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId = 0x";

        private const string GetImmedSubscriptionsFormat_SELECT_ImmedSubscriptions = @"SELECT Id,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,WebLanguage,WebLocale,WebTimeZone,
                 WebTime24,WebCalendarType,WebAdjustHijriDays,ListUrl,ListTitle,ListBaseType,
                 ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,Filter,BinaryFilter,
                 Properties,Status,ItemDocId,DeliveryChannel,EventType
                FROM ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId{0} AND Deleted=0";//{0}:ItemId


        private const string GetSchedSubscriptionsFormat_SELECT_SchedSubscriptions = @"SELECT Id,NotifyFreq,NotifyTime,NotifyTimeUTC,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,
                 WebLanguage,WebLocale,WebTimeZone,WebTime24,WebCalendarType,WebAdjustHijriDays,
                 ListUrl,ListTitle,ListBaseType,ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,
                 Filter,BinaryFilter,Properties,Status,ItemDocId,DeliveryChannel,EventType
                FROM  SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId{0} AND NotifyFreq<>0 AND Deleted=0";

        private const string ItemIdCondition_ImmedSubscriptions = @" AND ItemId=@ItemId";
        private const string ItemIdNullCondition_ImmedSubscriptions = @" AND ItemId is NULL";

        [QueryCommandArgument(Arguments = new object[] { AveSPAlertHostType.Item })]
        public static string GetSchedSubscriptions_SELECT_SchedSubscriptions(AveSPAlertHostType hostType)
        {
            return string.Format(GetSchedSubscriptionsFormat_SELECT_SchedSubscriptions,
                GetItemIdCondition(hostType));//{0}:ItemId
        }

        [QueryCommandArgument(Arguments = new object[] { AveSPAlertHostType.List })]
        public static string GetImmedSubscriptions_SELECT_ImmedSubscriptions(AveSPAlertHostType hostType)
        {
            return string.Format(GetImmedSubscriptionsFormat_SELECT_ImmedSubscriptions,
                GetItemIdCondition(hostType));//{0}:ItemId
        }

        private static string GetItemIdCondition(AveSPAlertHostType hostType)
        {
            switch (hostType)
            {
                case AveSPAlertHostType.List:
                case AveSPAlertHostType.Folder:
                    return ItemIdNullCondition_ImmedSubscriptions;
                case AveSPAlertHostType.Item:
                case AveSPAlertHostType.Doc:
                    return ItemIdCondition_ImmedSubscriptions;
                default:
                    throw new ArgumentException("Host type");
            }
        }

        public const string GetAttachmentSize_SELECT_AllDocs = @"Select Size From AllDocs With(noLock) Where SiteId =@SiteId And DeleteTransactionId=0x And Id=@Id And UIVersion=@Version";

        public const string GetRoleDefWebId_SELECT_Perms = @"select RoleDefWebId from Perms WITH(NOLOCK) where SiteId=@SiteId AND ScopeId=@ScopeId AND DelTransId=0x";

        public const string GetRoleAssignmentCount_SELECT_RoleAssignment = @"SELECT COUNT(PrincipalId) from RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId and ScopeId=@ScopeId and RoleId=@RoleId and PrincipalId=@PrincipalId";

        public const string GetGroupInfo_SELECT_Groups = @"
                SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
                From Groups WITH(NOLOCK) WHERE SiteId=@SiteId AND ID=@Id";
        public const string GetGroupMembershipById_SELECT_GroupMembership = @"SELECT MemberId From GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId AND GroupId=@Id";

        public const string GetUserInfoById_SELECT_UserInfo = @"
                SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
                       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
                       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
                FROM UserInfo WITH(NOLOCK)
                WHERE tp_SiteID=@SiteId AND tp_ID=@Id";

        public const string GetUserInfoByLoginName_SELECT_UserInfo = @"
                SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
                       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
                       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
                FROM UserInfo WITH(NOLOCK)
                WHERE tp_SiteID=@SiteId AND tp_Login=@LoginName";

        public const string GetUndeletedUserCount_SELECT_UserInfo = @"SELECT COUNT(tp_ID) FROM UserInfo WITH(NOLOCK) WHERE tp_SiteID=@SiteId AND tp_ID=@UserId AND tp_Deleted<>0";
        public const string GetListId_SELECT_AllLists = @"SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_WebId=@WebId AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
        public const string GetListId2_SELECT_AllLists = @"SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebID AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
        //Actived并且有权限的用户
        public const string GetAvailableUserCount_SELECT_AllLists = @"
                SELECT COUNT(tp_ID)
                FROM UserInfo WITH(NOLOCK)
                WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                        AND tp_ID in (
                        SELECT DISTINCT(PrincipalId) FROM RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId And PrincipalId=@UserId
                        UNION
                        SELECT DISTINCT(MemberId) FROM GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId AND MemberId=@UserId
        )";

        public const string GetFolderId_SELECT_AllDocs = @"SELECT ID FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x AND type=1";

        public const string GetHiddenFilesInList_SELECT_AllDocs = @"SELECT Id, LeafName, UIVersion, DocFlags, Level , TimeLastModified, doc.HasStream, COALESCE(doc.Size ,doc.SizeWrite) as Size FROM AllDocs as doc WITH(NOLOCK) 
                WHERE SiteId = @SiteId AND DeleteTransactionId=0x AND ParentId=@FolderId AND WebId = @WebId AND ListId = @ListId 
                AND Type = 0 AND DocLibRowId IS NULL AND IsCurrentVersion = 1 ORDER BY LeafName, UIVersion";

        public const string GetHiddenFilesNotInList_SELECT_AllDocs = @"SELECT Id, LeafName, UIVersion, DocFlags, Level , TimeLastModified, doc.HasStream, COALESCE(doc.Size ,doc.SizeWrite) as Size FROM AllDocs as doc WITH(NOLOCK) 
                WHERE SiteId = @SiteId AND DeleteTransactionId=0x AND ParentId=@FolderId AND WebId = @WebId  
                AND Type = 0 AND DocLibRowId IS NULL AND IsCurrentVersion = 1 ORDER BY LeafName, UIVersion";

        public const string GetItemTPGUID_SELECT_AllUserData = @"SELECT tp_GUID from AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 
                                                AND tp_ID=@RowId AND tp_RowOrdinal=0";
        public const string GetItemRowId_SELECT_AllUserData = @"SELECT tp_ID from AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0  
                                                AND tp_GUID=@GUID AND (tp_Level=1 OR tp_Level=2 OR tp_Level=255) AND tp_RowOrdinal=0";

        public const string GetItemCount_SELECT_AllDocs = @"SELECT count(Id) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";

        public const string GetItemInfoByParentId_SELECT_AllDocs = @"select LeafName, Id from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId";

        public const string GetShredInfo_SELECT_DocsToStreams = @"select DT.Partition,DT.BSN from DocsToStreams as DT
                                 where DT.SiteId=@SiteId
                                 and DT.DocId=@Id
                                 and DT.HistVersion =@HISTVersion
                                 and DT.Level=@Level";

        public const string GetContentAndRBSId_SELECT_DocStreams = @"SELECT RBSId,Size,Content FROM DocStreams ds WITH(NOLOCK) WHERE ds.SiteId=@SiteId AND ds.DocId=@Id AND ds.Partition = @Partition AND ds.BSN=@BSN";
        public const string GetWebUrlId_SELECT_AllWebs = @"select FullUrl,Id from AllWebs WITH(NOLOCK) where SiteId=@SiteId AND DeleteTransactionId=0x";
        public const string GetWebLastAccessed_SELECT_AllWebs = @"
            SELECT TimeCreated, FullUrl, LastAccess = case 
                WHEN DayLastAccessed=0 then TimeCreated
                    else DATEADD(d, DayLastAccessed + 65536, '01/01/1899')
                end
            FROM AllWebs WITH(NOLOCK)
            WHERE Id=@WebId and SiteId=@SiteId";

        public const string GetItemCTId_SELECT_AllUserData = @"
            SELECT tp_contenttypeId FROM AllUserData WITH (NOLOCK)
            WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=1 or tp_IsCurrentVersion=0) AND tp_ParentId=@ParentId AND tp_DocId=@DocId AND tp_UIVersion=@Version";

        public const string GetNavMetainfo_SELECT_NavNodes = @"SELECT NodeMetainfo FROM NavNodes WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND Eid=@Eid";

        public const string GetActivatedFeatureId_SELECT_Features = @"SELECT FeatureId FROM Features WITH(NOLOCK) WHERE SiteId=@SiteId and WebId=@WebId ORDER BY TimeActivated";

        public const string GetAppPrincipal_SELECT_AppPrincipals = @"SELECT TOP 1 Id FROM AppPrincipals WITH(NOLOCK) WHERE SiteId =@SiteId AND Name=@AppPrincipalId";

        public const string GetWorkflowIds_SELECT_Workflow = @"SELECT Id FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebID AND ListId=@ListId AND ItemId=@ItemId ORDER BY Created";
        public const string GetWorkflowAssociationBaseIds_SELECT_WorkflowAssociation = @"SELECT Distinct([BaseId]) FROM [WorkflowAssociation] WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebID AND ListId=@ListId";

        public const string GetScopeUrl_SELECT_Perms = @"SELECT ScopeUrl FROM Perms WITH(NOLOCK) WHERE SiteId =@SiteId AND ScopeId =@ScopeId";

        public const string GetWorkflowInstance_SELECT_Workflow = @"SELECT [Id],[TemplateId],[ListId],[SiteId],[WebId],[ItemId],
                [ItemGUID],[TaskListId],[AdminTaskListId],[Author],[Modified],[Created],[InternalState],[LockMachineId],[LockMachinePID],
                [InstanceDataVersion],[InstanceDataSize],[InstanceData],[Modifications],[HistorySize],[History],[StatusVersion],
                [Status1],[Status2],[Status3],[Status4],[Status5],[Status6],[Status7],[Status8],[Status9],[Status10],
                [TextStatus1],[TextStatus2],[TextStatus3],[TextStatus4],[TextStatus5],[ActivityDetails],[CorrelationId] 
                FROM Workflow WITH(NOLOCK) WHERE Id=@Id ";

        public const string GetScheduledWorkItems_SELECT_ScheduledWorkItems = @"SELECT [SiteId],[Id],[DeliveryDate],[Type],[ParentId],[ItemId],[ItemGuid],[BatchId],[WebId],[UserId],
                        [Created],[BinaryPayload],[TextPayload],[ProcessingId],[ProcessMachineId],[ProcessMachinePID],[InternalState] 
                FROM ScheduledWorkItems WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@WorkflowInstanceId";

        #region 外围调用无论column是否在fieldmap中存在，都会把查出来的column存起来，暂时无法去掉Select *，需要进一步确认。
        public const string GetWFTaskItem_SELECT_AllLists_AllUserData = @"DECLARE @parentId uniqueidentifier
                       SELECT @parentId=tp_RootFolder FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId AND tp_ID=@ListId
                       SELECT * FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) 
                               AND tp_ParentId=@parentId AND (tp_WorkflowInstanceId=@WorkflowInstanceId or nvarchar6=@nvarchar6) ORDER BY tp_Id,tp_Version";
        public const string GetEventReceivers_SELECT_EventReceivers = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND ContextCollectionId=@ContextCollectionId";

        public const string GetEventReceivers2_SELECT_EventReceivers = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND 
                    Type=32767 AND ContextCollectionId=@ContextCollectionId AND ContextObjectId IS NULL AND ContextId IS NULL AND 
                    ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";

        public const string GetEventReceivers3_SELECT_EventReceivers = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostId=@HostId AND HostType=2 AND
                    Type=32767 AND ContextCollectionId IS NULL AND ContextObjectId IS NULL AND ContextId IS NULL AND
                    ContextType IS NULL AND ContextEventType IS NULL AND SequenceNumber=10000 AND Assembly='' AND Class=''";

        public const string GetEventReceivers4_SELECT_EventReceivers = @"SELECT * FROM EventReceivers WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND HostType=5 AND ContextCollectionId=@ContextCollectionId ORDER BY ItemId";
        #endregion

        public const string GetWFTaskAndHistoryList_SELECT_WorkflowAssociation = @"SELECT TaskListTitle,HistoryListTitle,cast(cast(TaskListId as VARBINARY) as UNIQUEIDENTIFIER),cast(cast(HistoryListId as VARBINARY) as UNIQUEIDENTIFIER),Name,BaseId FROM WorkflowAssociation WITH(NOLOCK) WHERE Id=@Id ";

        private const string GetWFHistoryItemFormat_SELECT_AllLists_AllUserData = @"DECLARE @parentId uniqueidentifier
                       SELECT @parentId=tp_RootFolder FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId AND tp_ID=@ListId
                       SELECT * FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) 
                               AND tp_ParentId=@parentId AND {0}=@WorkflowInstanceId ORDER BY tp_Id,tp_Version";

        [QueryCommandArgument(Arguments = new object[] { "$instanceColName" })]
        public static string GetWFHistoryItem_SELECT_AllLists_AllUserData(string instanceColName)
        {
            return string.Format(GetWFHistoryItemFormat_SELECT_AllLists_AllUserData, instanceColName);
        }

        public const string GetDeleteTransactionIdByTPGUID_SELECT_AllUserData = @"SELECT Distinct tp_DeleteTransactionId FROM ALLUserData WITH(NOLOCK) 
                                                 WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId<>0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_GUID=@TP_GUID";
        public const string GetDeleteTransactionIdByLeafName_SELECT_AllDocs = @"SELECT Distinct DeleteTransactionId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName";

        public const string GetItemModified_SELECT_AllUserData = @"SELECT tp_Modified FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId = 0x Or tp_DeleteTransactionId <> 0x) AND tp_IsCurrentVersion=1 AND tp_ID=@ID and tp_IsCurrent=1";


        public const string GetTPModified2_SELECT_AllUserData = @"SELECT tp_Modified FROM AllUserData with(nolock)
                                        WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND (tp_Level=1 or tp_Level=2 or tp_Level=255)";

        public const string GetTPModified1_SELECT_AllUserData = @"SELECT tp_Modified FROM AllUserData with(nolock)
                                        WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND (tp_Level=1 or tp_Level=2 or tp_Level=255) AND tp_IsCurrent=1";

        public const string GetMaxRowId_SELECT_AllDocs = @"SELECT MAX(DoclibRowId) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId and ListId=@ListId AND (DoclibRowId IS NOT NULL)";

        public const string GetMaxLeafName_SELECT_AllDocs = @"SELECT MAX(LeafName) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND (DeleteTransactionId=0x OR DeleteTransactionId<>0x) AND ParentId=@ParentId";

        public const string GetItemLevel_SELECT_AllDocs_AllDocVersions = @"SELECT Level FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x
                        union all SELECT Level FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x";

        public const string GetItemInRecyclebinById_SELECT_AllUserData = @"select tp_ID FROM AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and tp_ID=@TP_ID and tp_CalculatedVersion=0 and tp_Level>0 and tp_RowOrdinal=0";
        public const string GetItemById_SELECT_AllUserData = @"select tp_DeleteTransactionId,tp_ID,tp_Level ,tp_UIVersion FROM AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId =@ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion =1 and tp_ID=@TP_ID and tp_CalculatedVersion=0 and tp_Level>0 and tp_RowOrdinal=0";
        public const string GetItemInRecyclebinByName_SELECT_AllDocs = @"SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
        public const string GetItemByName_SELECT_AllDocs = @"SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
        public const string GetItemInRecyclebinByTPGUID_SELECT_AllUserData = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData  WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_RowOrdinal=0 and tp_ListId=@tp_ListId and tp_DeleteTransactionId<>0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid";
        public const string GetItemByTPGUID_SELECT_AllUserData = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData With(nolock)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ListId=@tp_ListId and tp_DeleteTransactionId=0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid";
        [QueryCommandArgument(Arguments = new object[] { "$colName" })]
        public static string GetItemByColumn_SELECT_AllUserData(string colName)
        {
            return string.Format(@"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion=1 and {0}=@ColumnValue", colName);
        }
        [QueryCommandArgument(Arguments = new object[] { "$colName" })]
        public static string GetItemInRecyclebinByColumn_SELECT_AllUserData(string colName)
        {
            return string.Format(@"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion=1 and {0}=@ColumnValue", colName);
        }
        public const string GetItemInRecyclebinByTitle_SELECT_AllUserData = @"select tp_ID FROM AllUserData With(nolock) where tp_SiteId=@SiteId and tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and nvarchar1=@title and tp_CalculatedVersion=0 and tp_RowOrdinal=0";

        public const string GetItemByTitle_SELECT_AllUserData = @"select tp_DeleteTransactionId,tp_ID,tp_Level ,tp_UIVersion FROM AllUserData With(nolock) where tp_SiteId=@SiteId and tp_ListId =@ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion =1 and nvarchar1=@title and tp_CalculatedVersion=0 and tp_RowOrdinal=0";

        public const string GetItemAECM_SELECT_AllUserData = @"Select tp_Modified,tp_Created,tp_Author,tp_Editor From AllUserData With(noLock) Where tp_SiteId =@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) And tp_ParentId=@ParentId And tp_DocId=@Id And tp_UIVersion=@UIVersion And tp_Level=@Level";
        public const string GetItemCM_SELECT_AllDocs = @"Select TimeCreated,TimeLastModified From AllDocs With(noLock) Where SiteId =@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And Level=@Level And UIVersion=@UIVersion";
        public const string GetItemUniqueId_SELECT_AllDocs = @"SELECT Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName";

        public const string GetNextAvailableId_SELECT_AllListAux = @"SELECT NextAvailableId FROM AllListsAux WITH(NOLOCK) WHERE SiteId=@SiteId AND ListID=@ListId";

        public const string GetWebId_SELECT_AllWebs = @"SELECT Id FROM AllWebs With(nolock) WHERE FullUrl=@Url AND DeleteTransactionId=0x and SiteId=@SiteId";

        public const string GetDocInfo_SELECT_AllDocs = @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
                 DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
                 NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
                 SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
                 LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
                 UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
                 WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
                 DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
                 ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
                 ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
                 ListSchemaVersion,ClientId,InternalVersion,BumpVersion,StreamSchema
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentID=@ParentID AND Id=@Id AND UIVersion=@Version";

        public const string GetDocFlag_SELECT_AllDocs_AllDocVersions = @"SELECT DISTINCT DocFlags
                                    FROM AllDocs WITH(NOLOCK)
                                    WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentID=@ParentID AND Id=@Id AND UIVersion=@UIVersion
                                    UNION
                               SELECT DocFlags
                                    FROM  AllDocVersions WITH(NOLOCK)
                                    WHERE (SiteId = @SiteId) AND (Id = @ID) AND (UIVersion = @UIVersion)";

        public const string GetCheckoutUserId_SELECT_AllDocs = @"SELECT CheckoutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentID=@ParentID AND Id=@Id AND UIVersion=@Version";

        public const string GetUIVersion_SELECT_AllDocs_AllDocVersions = @"Select UIVersion from Alldocs WITH(NOLOCK) where SiteId=@SiteId And DeleteTransactionId=0x And ParentID=@ParentID AND Id=@Id 
                                                                           Union
                                                                           Select UIVersion from AllDocVersions WITH(NOLOCK) where SiteId=@SiteId And Id=@Id And DeleteTransactionId=0x";

        public const string GetDocInfoCurrentVersion_SELECT_AllDocs = @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
                 DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
                 NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
                 SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
                 LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
                 UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
                 WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
                 DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
                 ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
                 ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
                 ListSchemaVersion,ClientId,InternalVersion,BumpVersion
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentID=@ParentID AND Id=@Id AND IsCurrentVersion=1 AND (Level=1 OR Level=2 OR Level=255)";

        public const string GetDocInfoCurrentVersionNoParentId_SELECT_AllDocs = @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
                 DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
                 NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
                 SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
                 LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
                 UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
                 WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
                 DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
                 ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
                 ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
                 ListSchemaVersion,ClientId,InternalVersion,BumpVersion
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND Id=@Id AND IsCurrentVersion=1 AND (Level=1 OR Level=2 OR Level=255)";

        #region GetWebSize_SELECT_AllDocs_AllFileFragments_AllWebs_AllLists_AllUserData_AllWebParts_Personalization_ContentTypes_RecycleBin_AllDocVersions
        public const string GetWebSize_SELECT_AllDocs_AllFileFragments_AllWebs_AllLists_AllUserData_AllWebParts_Personalization_ContentTypes_RecycleBin_AllDocVersions =
        @"SELECT SUM(PartSize), SiteSize.WebId
        FROM
        (
        	SELECT PartSize, WebId FROM
        	(
        		SELECT ISNULL
        		(
        			SUM
        			(
        				CAST
        					(ISNULL(Size, 0) AS BIGINT) +
        				CAST
        					(ISNULL(MetaInfoSize, 0) AS BIGINT) +
        				CAST
        					(FileFormatMetaInfoSize AS BIGINT) +
        				CAST
        					(ISNULL(UnVersionedMetaInfoSize,0) AS BIGINT) +
        				CAST
        					(152 AS BIGINT)
        			), 0
        		) AS PartSize, WebId
        		FROM
        		(                
        			SELECT
        				WebId, Size, MetaInfoSize, FileFormatMetaInfoSize, UnVersionedMetaInfoSize
        			FROM
        				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
        			WHERE
        				SiteId = @SiteId AND
        				DeleteTransactionId = 0x
        		) AS AD GROUP BY WebId
        	) Docs_NoLock_Site
        	UNION ALL
        	SELECT PartSize, WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(AFF.BlobSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
        			WHERE
        				SiteId = @SiteId AND
        				DeleteTransactionId = 0x
        		) AS AD 
        		CROSS APPLY
        		(
        			SELECT
        				*
        			FROM
        				AllFileFragments WITH (NOLOCK, INDEX=AllFileFragments_PartId_UCI)
        			WHERE
        					DocId = AD.Id
        		) AS AFF GROUP BY WebId
        	)AllFileFragments_NoLock_DocId
        	UNION ALL
        	SELECT WL.PartSize, WL.WebId FROM 
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(DATALENGTH(L.tp_ContentTypes), 0) + ISNULL(DATALENGTH(L.tp_Fields), 0)) AS BIGINT))),0)) AS PartSize, L.tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				AllWebs WITH (NOLOCK, INDEX=Webs_SiteIdParent)
        			WHERE
        				SiteId = @SiteId AND DeleteTransactionId=0x
        		) AS W
        		CROSS APPLY
        		(
        			SELECT
        				*
        			FROM
        				AllLists WITH (NOLOCK, INDEX=AllLists_PK)
        			WHERE
        				tp_WebId = W.Id AND
        				tp_DeleteTransactionId = 0x
        		) AS L GROUP BY tp_WebId
        	)WL
        	UNION ALL
        	SELECT UD_AL.PartSize, UD_AL.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				AUD.tp_Size, AL.tp_WebId
        			FROM
        				AllUserData AUD WITH (NOLOCK, INDEX=AllUserData_ParentId), AllLists AL WITH(NOLOCK)
        			WHERE
        				AUD.tp_SiteId = @SiteId AND
        				AUD.tp_DeleteTransactionId = 0x AND
        				(AUD.tp_IsCurrentVersion = CONVERT(BIT, 0) OR AUD.tp_IsCurrentVersion = CONVERT(BIT, 1)) AND
        				AUD.tp_ListId = AL.tp_ID
        		) AS UD GROUP BY tp_WebId
        	)UD_AL
        	UNION ALL
            SELECT AWP_AL.PartSize, AWP_AL.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(AWP.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, AWP.WebId
        		FROM
        		(
        			SELECT
        				AW_T.tp_Deleted, AW_T.WebId, AW_T.tp_Size, AW_T.tp_ListId, AW_T.tp_SiteId
        			FROM
        			(
        				SELECT AD.WebId, tp_Deleted, tp_Size, tp_ListId, tp_SiteId 
        				FROM
        					AllWebParts AW WITH (INDEX=PageUrlID_FK), AllDocs AD WITH(NOLOCK)
        				WHERE
        					AW.tp_PageUrlID = AD.Id AND
        					AW.tp_SiteId = @SiteId AND 
        					AW.tp_Deleted = CONVERT(BIT, 0)
        			) AW_T
        		) AS AWP
        		WHERE AWP.tp_Deleted = CONVERT(BIT, 0) GROUP BY AWP.WebId		
        	)AWP_AL
        	UNION ALL
        	SELECT P_AL.PartSize, P_AL.WebId FROM 
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(P.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				PL.tp_Size, PL.tp_Deleted, AW.tp_ListId, AL.tp_WebId
        			FROM
        				Personalization AS PL WITH (NOLOCK, INDEX=Personalization_PK), AllWebParts AS AW WITH (INDEX=PageUrlID_FK), AllLists AL WITH(NOLOCK)
        			WHERE
        				PL.tp_SiteId = @SiteId AND
        				PL.tp_WebPartID = Aw.tp_ID AND
        				AW.tp_ListId = AL.tp_ID
        		) AS P
        		WHERE P.tp_Deleted = CONVERT(BIT,0) GROUP BY tp_WebId
        	)P_AL
        	UNION ALL
        	SELECT CT_Data.PartSize, CT_Data.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(CT_T.Size, 0)) AS BIGINT))),0)) AS PartSize, CT_T.Id AS WebId
        		FROM
        		(
        			SELECT
        				CT.Size, Web.Id
        			FROM
        				ContentTypes CT WITH (NOLOCK, INDEX=ContentTypes_SiteClassCTId), AllWebs Web WITH(NOLOCK) 
        			WHERE
        				CT.SiteId = @SiteId and (Web.FullUrl=CT.Scope or CT.Scope='') and Web.SiteId = @SiteId and Web.DeleteTransactionId=0x
        		) AS CT_T GROUP BY CT_T.Id		
        	) CT_Data
        	UNION ALL
        	SELECT R_Data.PartSize, R_Data.WebId FROM
        	(
        		SELECT ISNULL(SUM(R.Size),0) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				RecycleBin WITH (NOLOCK, INDEX=RecycleBin_SiteBinWebUser)
        			WHERE
        				SiteId = @SiteId AND
        				BinId = 1
        		) AS R GROUP BY WebId
        	) R_Data
        	UNION ALL
        	SELECT PartSize, WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(Size, 0) + ISNULL(MetaInfoSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				ADV.Size, ADV.MetaInfoSize, AD.WebId
        			FROM
        				AllDocVersions ADV WITH (NOLOCK, INDEX=AllDocVersions_PK), AllDocs AD
        			WHERE
        				ADV.SiteId = @SiteId AND
        				ADV.DeleteTransactionId = 0x AND
        				ADV.Id = AD.Id
        		) AS ADV_AD
        		GROUP BY WebId
        	)ADV_AD_Data
        ) SiteSize
        GROUP BY SiteSize.WebId";
        #endregion

        public const string GetItemUIVersion_SELECT_AllDocs = @"SELECT UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x0 AND ParentId=@ParentId AND Id=@Id and IsCurrentVersion=1";
        public const string GetItemUIVersionNoParentId_SELECT_AllDocs = @"SELECT UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x0 AND Id=@Id and IsCurrentVersion=1";

        [QueryCommandArgument(Arguments = new object[] { "$colName" })]
        public static string GetItemIdColName_SELECT_AllUserData(string colName)
        {
            return string.Format(
@"SELECT tp_ID,{0} FROM AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1",
                        colName);
        }

        public const string GetAppInstallStatus_SELECT_TVF_AppSourceInfo_SourceInfoId_TVF_AppPackages_PackageFingerprint_TVF_AppSourceInfo_PackageFingerprint_TVF_AppInstallations_WebIdSourceInfoId =
                  @"SELECT TOP 1 Installations.Status FROM
                    TVF_AppSourceInfo_SourceInfoId(@SiteId, @SourceInfoId) as InstallingSourceInfo --the item being installed
                    CROSS APPLY TVF_AppPackages_PackageFingerprint(@SiteId, InstallingSourceInfo.PackageFingerprint) as InstallingPackage --the package being installed
                    CROSS APPLY TVF_AppPackages_ProductId(@SiteId, InstallingPackage.ProductId) as ProductPackages --all packages with same ProductId
                    CROSS APPLY TVF_AppSourceInfo_PackageFingerprint(@SiteId, ProductPackages.PackageFingerprint) as ProductSourceInfo --all source info with same product id
                    CROSS APPLY TVF_AppInstallations_WebIdSourceInfoId(@SiteId, @WebId, ProductSourceInfo.SourceInfoId) AS Installations --all installations with same product id";

        public const string GetAppManifest_SELECT_AppPackages = @"SELECT TOP 1 Manifest FROM AppPackages WITH(NOLOCK) WHERE SiteId=@SiteId AND PackageFingerprint=@PackageFingerprint";

        public const string GetLoginName_SELECT_UserInfo = @"select tp_Login from UserInfo WITH(NOLOCK) where tp_SiteId=@SiteId and tp_SystemId=@SystemId";

        public const string HasAlertByItemId_SELECT_ImmedSubscriptions_SchedSubscriptions =
                        @"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND ItemId=@ItemId And Deleted=0
                        Union All
                        SELECT top 1 0 FROM  SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND ItemId=@ItemId And Deleted=0";

        public const string HasAlertByListId_SELECT_ImmedSubscriptions_SchedSubscriptions =
                        @"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId
                        Union All
                        SELECT top 1 0 FROM  SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId";

        public const string GetAlertsByWebId_SELECT_ImmedSubscriptions_SchedSubscriptions =
                        @"SELECT Id,Properties,ListId  FROM ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0 
                        Union All 
                        SELECT Id,Properties,ListId  FROM SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0";
        [QueryCommandArgument(Arguments = new object[] { true })]
        public static string HasImmedSubscription_SELECT_ImmedSubscription(bool listAlert)
        {
            return string.Format(@"SELECT top 1 0 FROM  ImmedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND EventType=@EventType And UserId =@UserId And Deleted=0 {0}",
                listAlert ? " AND ItemId is NULL AND Filter=''" : " AND ItemId=@ItemId");
        }

        [QueryCommandArgument(Arguments = new object[] { false, 2 })]
        public static string HasSchedSubscriptions_SELECT_SchedSubscriptions(bool listAlert, int frequency)
        {
            return string.Format(@"SELECT top 1 0 FROM  SchedSubscriptions WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND EventType=@EventType And UserId =@UserId And Deleted=0 {0} AND NotifyFreq = {1}",
                listAlert ? " AND ItemId is NULL AND Filter=''" : " AND ItemId=@ItemId",
                frequency);
        }

        public const string GetContentSize_SELECT_DocStreams = @"select sum(cast(size as bigint)) from DocStreams WITH(NOLOCK) where SiteId=@SiteId and DocId = @DocId and RbsId is null";

        public const string GetWFStatus_SELECT_Workflow = @"SELECT Status1 FROM Workflow WITH(NOLOCK) WHERE Id=@Id";

        public const string GetWebpartView_SELECT_AllWebParts = @"select top(1) tp_View from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteID and tp_PageUrlID=@PageID and tp_ID=@WebPartId";

        public const string GetWebpartCount_SELECT_WebPartLists = @"SELECT COUNT(tp_Level) FROM WebPartLists WITH(NOLOCK) where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@SourceLevel";

        public const string GetWebpartById_SELECT_Personalization = @"SELECT tp_WebPartID FROM Personalization WITH(NOLOCK) WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@UserID";

        [QueryCommandArgument(Arguments = new object[] { true })]
        public static string GetItemIdByName_SELECT_AllDocs(bool includeDirName)
        {
            return string.Format(@"Select Id from AllDocs With(noLock)where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and LeafName=@LeafName{0}",
                  includeDirName ? " AND DirName=@DirName" : string.Empty);
        }

        public const string GetVersionModified_SELECT_AllUserData = @"SELECT tp_Modified FROM AllUserData WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) 
                        AND tp_ParentId=@ParentId AND tp_ID=@RowId AND tp_UIVersion=@UIVersion";
        public const string GetWFNameById_SELECT_WorkflowAssociation = @"SELECT Name FROM WorkflowAssociation WITH(NOLOCK) WHERE Id=@Id";

        public const string GetWFInstanceInfo_SELECT_Workflow = @"SELECT Id,InternalState,Modified,Created,Status1 FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND ItemId=@ItemId AND TemplateId=@TemplateId";

        public const string GetWFInstanceCount_SELECT_Workflow = @"SELECT COUNT(Id) FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND TemplateId=@TemplateId AND ((InternalState & 2)<>0)";

        public const string GetWFInternalState_SELECT_Workflow = @"SELECT InternalState FROM Workflow WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND ListId=@ListId AND ItemId=@ItemId AND TemplateId=@TemplateId";

        public const string GetWebpartProperties_SELECT_AllWebParts = @"select tp_AllUsersProperties,tp_PerUserProperties from AllWebParts with(nolock)
                                    where tp_SiteId =@SiteID and tp_PageUrlID=@PageID and tp_ID=@ID and tp_PageVersion=0 and tp_IsCurrentVersion=1";

        [QueryCommandArgument(Arguments = new object[] { "$colName" })]
        public static string GetRowIdByColName1_SELECT_AllUserData(string colName)
        {
            return $@"select tp_Id FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND {colName} =@Value order by tp_RowOrdinal ASC";
        }

        [QueryCommandArgument(Arguments = new object[] { "$colName" })]
        public static string GetItemIdByColName2_SELECT_AllUserData(string colName)
        {
            return $@"select tp_Id FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 
AND tp_ParentId=@parentId And {colName} =@Value collate Chinese_PRC_CS_AS_WS order by tp_ID ASC";
        }

        public const string GetRbsCollectionId_SELECT_AllSites = @"SELECT RbsCollectionId FROM AllSites With(nolock) WHERE Id=@SiteId AND Deleted = CONVERT(bit, 0)";

        public const string GetWebIdInRecyclebin_SELECT_AllWebs = @"SELECT Id FROM AllWebs With(nolock) WHERE SiteId =@SiteId AND FullUrl=@FullUrl AND DeleteTransactionId <> 0x";

        public const string GetItemCheckoutUserIdByIDUIVersion_SELECT_AllDocs = @"SELECT Id, Level, CheckOutUserId FROM AllDocs With(NoLock) WHERE SiteId=@SiteId AND ID=@ID AND Level=255 AND UIVersion=@UIVersion";

        public const string GetItemCheckoutUserIdBName_SELECT_AllDocs = @"SELECT Id, Level, CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND  DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName AND Level=255";

        public const string GetItemCheckoutUserIdById_SELECT_AllDocs = @"SELECT Id, Level,CheckOutUserId FROM AllDocs With(NoLock) WHERE SiteId=@SiteId AND ID=@ID AND Level=255";

        public const string GetItemCheckoutUserIdByRowId_SELECT_AllDocs = @"SELECT ID, CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND DoclibRowId=@ID AND DeleteTransactionId=0x AND Level=255";

        public const string GetItemCheckoutUserIdByName2_SELECT_AllDocs = @"SELECT Id, Level,CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DirName=@DirName AND LeafName=@LeafName AND DeleteTransactionId=0x AND Level=255";

        public const string GetListIdInRecyclebin_SELECT_AllLists = @"SELECT tp_Id from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId and tp_WebId = @WebId and tp_Title=@Title and tp_DeleteTransactionId <> 0x";

        public const string GetSolutionId_SELECT_Solutions = @"SELECT SolutionId FROM Solutions With(nolock) WHERE SiteId = @SiteId";
        public const string GetContentTypeResource_SELECT_Resources = @"Select ResourceName, LCID, NvarcharVal, NtextVal From Resources WITH(NOLOCK) Where SiteId = @SiteId And WebId = @WebId And ListId = @ListId And(ResourceName Like '_CTDesc%' Or ResourceName Like '_CTName%')";

        public const string UpdateCustomActionId = @"UPDATE CustomActions set Id = @NewId WHERE SiteId=@SiteId AND WebId=@WebId AND ScopeID=@ScopeId AND ID=@OldId";
        #endregion backup restore

        #region common,Discover,and SO
        #region proc

        public const string GetLanguage_Select_proc_ECM_GetSessionData = "proc_ECM_GetSessionData";

        public const string GetGlobalGroups_Select_proc_ECM_GetGroups = "proc_ECM_GetGroups";

        public const string GetMetadataServiceChanges_Select_proc_ECM_GetChanges = "proc_ECM_GetChanges";

        #endregion proc

        #region system table or proc

        public const string GetDBServerName_Select_SERVERPROPERTY = "SELECT SERVERPROPERTY('servername')";

        public const string GetDBUsedDisk_Select_sp_helpfile = "exec sp_helpfile";

        public const string GetDiskFreeSpace_Select_xp_FixedDrives = "exec master..xp_FixedDrives";

        public const string GetCurrentDBSpaceInfo_Select_sp_SpaceUsed = "exec sp_SpaceUsed";

        public const string CheckUserInServerRole_Select_server_role_members_server_principals = @"
        SELECT * FROM sys.server_role_members rm WITH(NOLOCK)
        JOIN sys.server_principals Roles WITH(NOLOCK) ON rm.role_principal_id = Roles.principal_id
        JOIN sys.server_principals Logins WITH(NOLOCK) ON rm.member_principal_id = Logins.principal_id
        WHERE Roles.type='R' AND (Logins.type='U' OR Logins.type = 'G') 
        AND Roles.name = @RoleName AND (Logins.name IN (@LoginNames) OR Logins.sid = @Sid)";

        public const string IsMemberOfDBRole_Select_IS_ROLEMEMBER = @"SELECT IS_ROLEMEMBER(@RoleName,@UserName)";

        public const string CheckUserInDBRole_Select_IS_ROLEMEMBER = @"
            SELECT IS_ROLEMEMBER( @RoleName, 
                                    (SELECT sys.sysusers.name FROM sys.sysusers WITH(NOLOCK)
                                     INNER JOIN master.sys.syslogins WITH(NOLOCK) ON sys.sysusers.sid = syslogins.sid 
                                     WHERE loginname=@UserName))";

        #endregion

        public const string GetTermStoreInfo_Select_ECMPermission = @"SELECT PrincipalName,Rights FROM ECMPermission WITH(NOLOCK) WHERE PartitionId = @PartitionId and GroupId = 0";

        public const string GetGlobalGroups_Select_ECMGroup = @"SELECT [Id] FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and ( [Type]=0 or [Type]=1)";

        public const string GetGroupByGuid_Select_ECMGroup = @"SELECT [Id] FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and UniqueId=@UniqueId";

        public const string GetGroupByName_Select_ECMGroup = @"SELECT [Id] FROM [ECMGroup] WITH(NOLOCK) WHERE PartitionId=@PartitionId and Name=@Name";

        public const string GetLocalGroups_Select_ECMGroup = @"SELECT [Id],[PartitionId],[UniqueId],[Name],[Description],[LastModifiedTime],[CreatedTime] ,[Type] FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=2";

        public const string GetTermById_Select_ECMTerm_ECMTermSetMembership = @"SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
        etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
        From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId AND etsm.TermSetId = ets.Id
        left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id
        WHERE ets.Id = @TermSetId AND et.Id = @Id AND etsm.PartitionId = @PartitionId";

        public const string GetTermLabels_Select_ECMTermLabel = @"
        Select etl.TermId, etl.LCID, etl.Label, etl.IsDefault  
        from ECMTermLabel etl WITH(NOLOCK) 
        where etl.PartitionId =@PartitionId and etl.TermId=@Id";

        public const string GetTermDescription_Select_ECMTermDescription = @"
        Select etd.TermId, etd.LCID, etd.Description 
        from ECMTermDescription etd WITH(NOLOCK) 
        where etd.PartitionId=@PartitionId and etd.TermId=@Id";

        public const string GetTermProperty_Select_ECMTerm_ECMTermProperty = @"
        select Property.PropertyName,Property.PropertyValue 
        from dbo.ECMTerm as Term With(NOLOCK) 
        inner join dbo.ECMTermProperty as Property With(NOLOCK) 
        on Term.PartitionId = Property.PartitionId and Property.TermId = Term.Id and Property.TermSetId = @TermSetId
        where Term.Id = @ID";

        public const string GetTermSetProperty_Select_ECMTermSet_ECMTermProperty = @"
        select Property.PropertyName,Property.PropertyValue 
        from dbo.ECMTermSet as TermSet With(NOLOCK) 
        inner join dbo.ECMTermProperty as Property With(NOLOCK) 
        on TermSet.PartitionId = Property.PartitionId and Property.TermId = 0 and TermSet.Id = Property.TermSetId 
        where TermSet.Id = @ID";

        public const string GetTermSetIdByGuid_Select_ECMTermSet = @"SELECT TOP 1 es.Id FROM ECMTermSet es WITH(NOLOCK) WHERE es.PartitionId=@PartitionId and es.UniqueId=@UniqueId";

        public const string GetTermsIdInTerm_Select_ECMTermSetMembership_ECMTerm = @"
        SELECT etsm.TermId 
        From ECMTermSetMembership etsm WITH(NOLOCK), ECMTerm et WITH(NOLOCK) 
        WHERE et.UniqueId=@TermId AND et.PartitionId=@PartitionId AND etsm.PartitionId=@PartitionId AND etsm.ParentTermId=et.Id AND etsm.TermSetId = @TermSetId";

        public const string GetTermsIdInTermSet_Select_ECMTermSetMembership_ECMTermSet = @"
        SELECT etsm.TermId 
        From ECMTermSetMembership etsm WITH(NOLOCK), ECMTermSet ets WITH(NOLOCK) 
        WHERE ets.PartitionId = @PartitionId AND ets.UniqueId=@SetId AND etsm.PartitionId=@PartitionId AND etsm.TermSetId=ets.Id AND etsm.ParentTermId=0 ";

        public const string GetTermsUniqueIdInTermSet_Select_ECMTermSetMembership_ECMTerm = @"
        SELECT DISTINCT et.UniqueId 
        FROM ECMTermSetMembership etsm WITH(NOLOCK) 
        INNER JOIN ECMTerm et WITH(NOLOCK) ON et.PartitionId=etsm.PartitionId and et.Id=etsm.TermId 
        WHERE etsm.PartitionId=@PartitionId and etsm.TermSetId=@ID";

        public const string GetTermIdByGuid_Select_ECMTerm = @"SELECT TOP 1 et.Id FROM ECMTerm et WITH(NOLOCK) WHERE et.PartitionId =@PartitionId AND et.UniqueId=@TermId";

        public const string GetTermsUniqueIdInTerm_Select_ECMTermSetMembership_ECMTerm = @"
        SELECT et.UniqueId 
        FROM ECMTermSetMembership etsm WITH(NOLOCK), ECMTerm et WITH(NOLOCK) 
        WHERE etsm.PartitionId=@PartitionId AND etsm.TermSetId=@TermSetId AND etsm.ParentTermId=@TermId AND et.Id=etsm.TermId ";

        public const string GetTermGroupTypeByUniqueId_Select_ECMGroup = "SELECT Type FROM ECMGroup WITH(NOLOCK) WHERE PartitionId = @PartitionId AND UniqueId=@UniqueId";

        public const string GetTermGroupPrincipalName_Select_ECMPermission_ECMGroup = @"
        SELECT ep.PrincipalName 
        FROM ECMPermission ep WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) 
        WHERE eg.PartitionId=@PartitionId AND eg.UniqueId=@UniqueId AND ep.PartitionId = @PartitionIdAND ep.GroupId=eg.Id ";

        public const string GetChangeTermsInTerm_Select_ECMChangeLog_ECMTermSetMembership = @"
        declare @SetId int
        set @SetId = ( SELECT ID FROM ECMTermSet WITH(NOLOCK) WHERE PartitionId=@PartitionId And UniqueId=@UniqueId)
        SELECT  GroupUniqueId,
        TermSetUniqueId,
        ObjectUniqueId,
        ObjectId,
        ObjectType,
        ChangeType,
        ChangeTime,
        ChangeData,
        ModifiedBy,
        ecmTermSet.Path 
        FROM ECMChangeLog ecmLog WITH(NOLOCK)
        INNER JOIN ECMTermSetMembership ecmTermSet WITH(NOLOCK) ON ecmTermSet.PartitionId=ecmlog.PartitionId AND ecmTermSet.TermId=ecmLog.ObjectId AND ecmTermSet.TermSetId=@SetId 
        WHERE ecmlog.PartitionId=@PartitionId
        AND ecmLog.ChangeTime>@SinceTime 
        AND ecmLog.TermSetUniqueId=@UniqueId 
        AND ecmLog.ObjectType=1 
        AND ecmTermSet.PartitionId = @PartitionId
        AND ecmTermSet.ParentTermId=@TermId";

        public const string GetGlobalTermGroupIds_Select_ECMGroup = @"SELECT [UniqueId],[Name] FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=0 or [Type]=1";

        public const string GetLocalTermGroupIds_Select_ECMGroup = @"SELECT [UniqueId],[Name] FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=2";

        public const string GetGlobalTermGroupIdsWithPartitionId_Select_ECMGroup = @"SELECT [UniqueId],[Name] FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=0 or [Type]=1";

        public const string GetLocalTermGroupIdsWithPartitionId_Select_ECMGroup = @"SELECT [UniqueId],[Name] FROM [ECMGroup] WITH(NOLOCK) WHERE [Type]=2";

        public const string GetTermGroupIdByUniqueId_Select_ECMGroup = "SELECT TOP 1 Id FROM ECMGroup WITH(NOLOCK) WHERE UniqueId=@UniqueId";

        public const string GetTermGroupIdInStoreByUniqueId_Select_ECMGroup = "SELECT TOP 1 Id FROM ECMGroup WITH(NOLOCK) WHERE PartitionId = @PartitionId and UniqueId=@UniqueId";

        public const string GetPublishingContentTypeCountById_Select_ECMPackage = "SELECT COUNT(Id) FROM [ECMPackage] WITH(NOLOCK) WHERE [PartitionId]=@PartitionId AND Id=@Id AND [Type]=@Type AND IsPublished=@IsPublished";

        public const string GetTermPathById_Select_ECMTermSetMembership = @"SELECT TOP 1 Path FROM ECMTermSetMembership WITH(NOLOCK) WHERE PartitionId=@PartitionId And TermId=@TermIntId And IsSource=1";

        public const string GetTermSetsInGroup_Select_ECMTermSet_ECMGroup = @"
        SELECT ets.UniqueId,ets.Name,ets.Type 
        FROM ECMTermSet ets WITH(NOLOCK) 
        INNER JOIN ECMGroup eg WITH(NOLOCK) ON  ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId 
        WHERE eg.UniqueId=@GroupId and eg.PartitionId=@PartitionId";

        public const string GetTermSetIdsInGroup_Select_ECMTermSet_ECMGroup = @"
        SELECT DISTINCT es.Id 
        FROM ECMTermSet es WITH(NOLOCK), ECMGroup eg WITH(NOLOCK) 
        WHERE eg.PartitionId=@PartitionId AND eg.UniqueId=@GroupId AND es.PartitionId=@PartitionId AND es.GroupId = eg.Id";

        public const string GetTermDefaultLableByTermId_Select_ECMTermLabel = @"SELECT [ECMTermLabel].Label 
                    FROM [ECMTermLabel] WITH(NOLOCK)
                    WHERE 
                    [ECMTermLabel].PartitionId = @DefaultPartitionId
                    AND
                    [ECMTermLabel].TermId = @TermId 
                    AND 
                    [ECMTermLabel].LCID = @DefaultLanguage 
                    AND 
                    [ECMTermLabel].IsDefault = 1 
                    ; 
                    ";

        public const string GetTermSet_Select_ECMTermSet = @"Select et.Id, et.PartitionId, et.CreatedTime, et.LastModifiedTime, et.Owner, et.CustomSortOrder, et.UniqueId, et.Name, et.Description,
                               et.Type, et.IsOpen, et.AvailableForTagging, et.Stakeholders, et.Contact, et.GroupId from ECMTermSet et WITH(NOLOCK) where 
                               et.PartitionId=@PartitionId and et.UniqueId=@UniqueId";

        public const string GetMetadataServiceSettings_Select_ECMServiceSettings = @"SELECT [ECMServiceSettings].PartitionId,[ECMServiceSettings].Settings 
                    FROM [ECMServiceSettings] WITH(NOLOCK)
                    where [ECMServiceSettings].PartitionId <> '00000000-0000-0000-0000-000000000000';
                    ";

        public const string GetParentTermUniqueId_Select_ECMTermSetMembership = @"
                SELECT et.UniqueId From ECMTermSetMembership etsm WITH(NOLOCK) left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id
                WHERE etsm.TermId = @Id AND etsm.PartitionId = @PartitionId";

        public const string GetTermPinSourceTermSetUniqueId_Select_ECMTermSet = @"
                select top 1 ts.UniqueId from ECMTermSet ts WITH(NOLOCK) left join ECMTermSetMembership tsms WITH(NOLOCK) on ts.PartitionId=tsms.PartitionId
                where tsms.TermSetId = ts.Id and tsms.TermSetId = @TermSetId and tsms.PartitionId = @PartitionId";

        public const string GetTermIsReusedProperty_Select_ECMTerm = @"
                Select COUNT(*) from ECMTerm et WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on et.PartitionId=etsm.PartitionId
                where et.Id = @Id and etsm.TermId = @Id and etsm.PartitionId = @PartitionId";

        public const string GetTenantAdminSiteIdByPartitionId_Select_SiteMap = @"
                SELECT Id,ApplicationId,DatabaseId,[Path],[Version] FROM
                dbo.SiteMap WITH (NOLOCK) where SubscriptionId = @SubscriptionId and DeleteTransactionId = 0x";

        public const string GetTermSetChildren_Select_ECMTermSet = @"
                SELECT et.Id,et.UniqueId,etsm.Path,etsm.ParentTermId,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
                From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId AND etsm.TermSetId = ets.Id
                left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId AND etsm.TermId = et.Id 
                WHERE ets.UniqueId = @UniqueId AND etsm.PartitionId = @PartitionId AND etsm.IsSource = 1";

        public const string GetTermGroupInfo_Select_ECMTermSet = @"
                SELECT eg.Id,eg.UniqueId,eg.Name,eg.Description,eg.Type FROM ECMTermSet ets WITH(NOLOCK) INNER JOIN ECMGroup eg WITH(NOLOCK) ON ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId 
                where ets.UniqueId=@UniqueId and ets.PartitionId=@PartitionId";

        public const string GetParentTermInfo_Select_ECMTermSet = @"
                SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
                etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
                From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
                left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId 
                WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.IsSource = 1 AND
                et.UniqueId = @ParentTermUniqueId AND etsm.PartitionId = @PartitionId AND
                ets.UniqueId = @TermSetId";

        public const string GetTermSetInfo_Select_ECMTermSet = @"
                SELECT eg.UniqueId,ets.Name,ets.Type FROM ECMTermSet ets WITH(NOLOCK) INNER JOIN ECMGroup eg WITH(NOLOCK) ON ets.GroupId=eg.Id and ets.PartitionId=eg.PartitionId WHERE
                ets.UniqueId = @UniqueId AND ets.PartitionId = @PartitionId";

        public const string GetSourceTermInfo_Select_ECMTermSet = @"
                SELECT et.Id,et.CreatedTime,et.LastModifiedTime,et.UniqueId,et.Owner,et.IsDeprecated,et.IsDeleted,et.MergedIdList,
                etsm.Path,etsm.ParentTermId,etsm.AvailableForTagging,etsm.CustomSortOrder,etsm.IsSource,etsm.PinSourceTermSetId,ets.UniqueId
                From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
                left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId 
                WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.IsSource = 1 AND
                et.Id = @Id and etsm.TermId = @Id and etsm.PartitionId = @PartitionId";

        public const string GetParentTermUniqueIdForTermSet_Select_ECMTermSet = @"
                SELECT et.UniqueId From ECMTermSet ets WITH(NOLOCK) left join ECMTermSetMembership etsm WITH(NOLOCK) on ets.PartitionId = etsm.PartitionId
                left join ECMTerm et WITH(NOLOCK) on etsm.PartitionId = et.PartitionId WHERE etsm.TermSetId = ets.Id AND etsm.TermId = et.Id AND etsm.TermId = @Id AND 
                etsm.PartitionId = @PartitionId AND ets.UniqueId = @TermSetId";

        public const string GetAllWebs_Select_AllSites_AllWebs = @"
        Select AllSites.Id as SiteID,
        AllWebs.Id as WebID,
        (case when AllWebs.Id=AllSites.RootWebId Then CAST(1 AS bit) Else CAST(0 AS bit) End) as IsRootWeb,
        AllWebs.FullUrl,
        Hostheader
        From AllSites With(NoLock) 
        Inner Join AllWebs With(NoLock) On AllWebs.SiteId=AllSites.Id AND AllSites.Deleted = CONVERT(bit, 0) AND AllWebs.DeleteTransactionId = 0x 
        Order By SiteID, IsRootWeb Desc;";

        public const string GetAllListsInWebWithRecycleBin_Select_AllLists = @"
        Select tp_ID, tp_Title From AllLists With(NoLock) Where tp_SiteId = @SiteId and tp_WebId = @WebId";

        public const string GetAllListsInWebWithoutRecycleBin_Select_AllLists = @"
        Select tp_ID, tp_Title From AllLists With(NoLock) Where tp_SiteId = @SiteId and tp_WebId = @WebId and tp_DeleteTransactionId = 0x";

        public const string GetNewCreatedWebsInDB_Select_EventCache_AllWebs = @"
        Select e.WebId, e.SiteId, w.FullUrl 
        From EventCache as e with(nolock), AllWebs as w with(nolock)
        Where e.ObjectType=4 And e.EventType=4096 And e.WebId=w.Id And w.DeleteTransactionId = 0x And e.EventTime Between @StartTime And @EndTime;";

        public const string GetNewCreatedWebsInDB_Select_EventCache_AllWebs_2 = @"
        Select e.WebId, e.SiteId
        From EventCache as e with(nolock), AllWebs as w with(nolock)
        Where e.ObjectType=4 And e.EventType=4096 And e.WebId=w.Id And w.DeleteTransactionId = 0x And e.EventTime Between @StartTime And @EndTime;";

        public const string GetEventReceiversByAssembly_Select_EventReceivers_AllSites = @"
        Select SiteId, WebId, Type, HostId, HostType 
        From EventReceivers With(NoLock) 
        Where Assembly=@AssemblyFullName And SiteId In(Select Id From AllSites With(NoLock) where Deleted = CONVERT(bit, 0));";

        public const string GetAllWebsInDB_Select_AllWebs = @"Select ID, SiteId, ParentWebId From AllWebs with (nolock) where DeleteTransactionId = 0x;";

        public const string GetListItemRowIdByGuid_Select_AllUserData = @"SELECT tp_ID FROM AllUserData With(noLock) WHERE tp_ListId = @tp_ListId AND tp_Guid = @tp_Guid ;";

        public const string GetSiteIdCollectionByWebAppId_Select_SiteMap = @"select id from sitemap with (nolock) where ApplicationId= @ApplicationId";

        public const string GetWelcomePageDocId_Select_NavNodes = "select docid from NavNodes with (nolock) where SiteId=@siteID and webID=@webID and EidParent < 0 and url is null";

        public const string GetLeafNameByDocId_Select_AllDocs = "Select leafname from alldocs with (nolock) where id=@ID and SiteId=@siteID";

        public const string GetSiteDeletionIdBySiteId_Select_SiteDeletion = @"select Id from SiteDeletion with (nolock) where SiteId=@SiteId";

        public const string GetAllPagesUnderFolder_Select_AllDocs = @"
        select Id,DirName,LeafName,Type 
        from AllDocs with(nolock) 
        where SiteId=@SiteId and DeleteTransactionId=0x and DirName like @parentUrl and leafname like '%.aspx' and IsCurrentVersion=1 and Type=0";

        public const string GetAllPagesUnderWeb_Select_AllDocs = @"
        select Id,DirName,LeafName,Type 
        from AllDocs with(nolock) 
        where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and leafname like '%.aspx' and IsCurrentVersion=1 and Type=0";

        public const string GetAllPagesUnderWeb_Select_AllDocs_WithoutVersion = @"
       select Id,DirName,LeafName,UIVersion,UIVersionString,IsCurrentVersion
        from AllDocs with(nolock) 
        where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and leafname like '%.aspx' and Type=0";

        public const string GetSubFolderAndPagesInFolder_Select_AllDocs = @"
        select Id,DirName,LeafName,Type 
        from AllDocs with(nolock) 
        where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and IsCurrentVersion=1 and (ExtensionForFile ='aspx' or Type=1)";

        public const string GetSiteStorageInfoInDB_Select_AllSites = "Select Id,DiskUsed,DiskQuota from AllSites with (nolock) where Deleted = CONVERT(bit, 0)";

        public const string GetDocIdByParentIdAndGuid_Select_AllUserData = "SELECT tp_DocId from AllUserData WITH(NOLOCK) where tp_siteid=@siteId and tp_DeleteTransactionId=0x and (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) and tp_parentid=@parentId and tp_guid=@tp_Guid";

        #region SOIntegration

        public const string GetContentByDocId_Select_DocStreams = @"
                SELECT
                    ADS.Content 
                FROM
                    DocStreams AS ADS WITH (NOLOCK,INDEX=AllDocStreams_CI)
                WHERE
                    SiteId = @SiteId AND Id = @Id AND InternalVersion = @InternalVersion";

        public const string GetAttachmentsFolderUrlByListId_Select_AllDocs_AllLists = @"
        SELECT DirName + '/' + LeafName + '/Attachments/' 
        FROM AllLists With(nolock) 
        INNER JOIN AllDocs With(nolock) ON Id = tp_RootFolder AND Level = 1 AND SiteId=tp_SiteId
        WHERE tp_SiteId=@SiteId AND tp_WebId = @WebId AND tp_ID = @ListId";

        public const string GetItemAttachmentFolderUrlByDirNameLeafName_Select_AllDocs = @"
        SELECT Id FROM AllDocs With(nolock) 
        WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x ) 
              AND LeafName=@DocLibRowId AND DirName=@AttachmentDir";

        public const string GetStubItemUrlByParentId_Select_AllDocs_DocStreams = @"
        SELECT DISTINCT DirName+'/'+LeafName 
        FROM AllDocs doc With(nolock) 
        INNER JOIN DocStreams AS stream With(nolock)
        on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND stream.InternalVersion = doc.InternalVersion
            AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
        WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0";

        public const string GetStubDocumentCountByParentId_Select_AllDocs_DocStreams = @"
        SELECT Count(doc.Id) 
        FROM AllDocs doc With(nolock) 
        INNER JOIN DocStreams AS stream With(nolock)
        on stream.Id = doc.Id AND stream.SiteId = doc.SiteId 
            AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
        WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0";

        public const string GetStubAttachmentRelativeUrl_Select_AllDocs_DocStreams = @"
BEGIN
WITH StubAttachment(Id, DirName, LeafName, Size,UIVersion, RbsId,Content)
AS
(
SELECT doc.Id, doc.DirName, doc.LeafName, doc.Size, doc.UIVersion,stream.RbsId,stream.Content
FROM AllDocs doc With(nolock) INNER JOIN DocStreams AS stream With(nolock)
on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0
),
StubAttachmentWithRowNum(Id, DirName, LeafName, Size, UIVersion,RbsId,Content,RowNum)
AS
(
select *,ROW_NUMBER() Over (order by StubAttachment.Id desc) RowNum from StubAttachment With(nolock)
)
SELECT Id,DirName,LeafName,Size,UIVersion,RbsId,Content,RowNum FROM StubAttachmentWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";

        public const string GetItemRowIdsByParentId_Select_AllDocs = @"SELECT DISTINCT DocLibRowId FROM AllDocs With(nolock) WHERE SiteId=@SiteId AND ParentId=@ParentId AND DocLibRowId IS NOT NULL AND DeleteTransactionId=0x ";

        public const string GetStubAttachmentsTotalCount_Select_AllDocs_DocsToStreams_DocStreams = @" 
SELECT COUNT(distinct(stream.DocId)) FROM AllDocs item WITH(NOLOCK)
INNER JOIN AllDocs att WITH(NOLOCK) ON 
att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
AND att.Level <= item.Level 
AND att.WebId = @WebId AND att.ListId = @ListId
AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
AND att.IsCurrentVersion <= 1  
INNER JOIN DocsToStreams DTS WITH(NOLOCK) ON DTS.DocId=att.Id and DTS.SiteId=att.SiteId and DTS.Level=att.Level
INNER JOIN DocStreams stream WITH(NOLOCK) ON 
stream.DocId = att.Id AND stream.SiteId = att.SiteId AND DTS.Partition=stream.Partition and DTS.BSN=stream.BSN
AND  (stream.Content IS NULL AND stream.RbsId IS NOT NULL)
WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1";

        public const string GetStubAttachmentsInFolder_Select_AllDocs_DocsToStreams_DocStreams = @"
BEGIN
WITH StubAttachment(Id, DirName, LeafName, Size,UIVersion,ItemName)
AS
(SELECT distinct(att.Id), att.DirName, att.LeafName, att.Size, att.UIVersion,item.LeafName
FROM AllDocs item WITH(NOLOCK)
INNER JOIN AllDocs att WITH(NOLOCK) ON 
att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
AND att.Level <= item.Level 
AND att.WebId = @WebId AND att.ListId = @ListId
AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
AND att.IsCurrentVersion <= 1  
INNER JOIN DocsToStreams DTS WITH(NOLOCK) ON DTS.DocId=att.id and DTS.SiteId=att.SiteId and DTS.Level=att.Level
INNER JOIN DocStreams stream WITH(NOLOCK) ON stream.DocId = att.Id AND stream.SiteId = att.SiteId 
AND stream.Partition=DTS.Partition and stream.BSN=DTS.BSN
AND  (stream.Content IS NULL AND stream.RbsId IS NOT NULL)
WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1
),
StubAttachmentWithRowNum(Id, DirName, LeafName, Size, UIVersion,ItemName,RowNum)
AS
(
select *,ROW_NUMBER() Over (order by StubAttachment.ItemName asc,StubAttachment.LeafName asc) RowNum from StubAttachment With(nolock)
)
SELECT Id, DirName, LeafName, Size, UIVersion,ItemName,RowNum FROM StubAttachmentWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";

        public const string GetMaxRbsBsnByDocId_Select_DocsToStreams = @"SELECT TOP 1 BSN FROM DocsToStreams WITH(NOLOCK) WHERE DocId = @DocId AND SiteId = @SiteId ORDER BY BSN DESC";

        public const string SelectStubItemAndVersionCount_Select_AllDocs_DocsToStreams_DocStreams_AllDocVersions = @"
BEGIN
WITH DocsBlob(Id,InternalVersion)
AS
(
SELECT distinct(docs.Id),docs.InternalVersion 
FROM AllDocs docs WITH (NOLOCK)
inner join DocsToStreams Dts WITH(NOLOCK) on docs.SiteId=Dts.SiteId and docs.Id=Dts.DocId and docs.Level=Dts.Level
inner join DocStreams docStream WITH(NOLOCK) on Dts.SiteId=docStream.SiteId and Dts.DocId=docStream.DocId and Dts.Partition=docStream.Partition and Dts.BSN=docStream.BSN
AND docs.Type <= 0 AND docs.ParentId = @ParentId  AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
AND docStream.Content IS NULL AND docStream.RbsId IS NOT NULL and Dts.HistVersion=0
AND docs.IsCurrentVersion <= 1 WHERE docStream.SiteId = @SiteId
UNION ALL
SELECT distinct(Versions.Id),versions.InternalVersion
FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId 
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
inner join DocsToStreams Dts WITH(NOLOCK) on Dts.SiteId=versions.SiteId and Dts.DocId=versions.Id and Dts.HistVersion=versions.UIVersion
inner join DocStreams docStream WITH(NOLOCK) on Dts.SiteId=docStream.SiteId and Dts.DocId=docStream.DocId and Dts.Partition=docStream.Partition and Dts.BSN=docStream.BSN
AND docStream.Content IS NULL AND docStream.RbsId IS NOT NULL and  Dts.HistVersion<>0
WHERE versions.SiteId = @SiteId
)
SELECT Count(Id) FROM DocsBlob With(nolock)
End";

        public const string GetStubFileAndVersions_Select_AllDocs_DocsToStreams_DocStreams_AllDocVersions = @"
BEGIN
with StubFiles(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size)
as 
(
SELECT distinct(docs.Id), docs.DirName AS DirName, docs.LeafName, docs.UIVersion, docs.IsCurrentVersion, docs.Size
FROM AllDocs docs WITH (NOLOCK)
inner join DocsToStreams Dts WITH(NOLOCK) on docs.SiteId=Dts.SiteId and docs.Id=Dts.DocId and docs.Level=Dts.Level
inner join DocStreams ds WITH(NOLOCK) on Dts.SiteId=ds.SiteId and Dts.DocId=ds.DocId and Dts.Partition=ds.Partition and Dts.BSN=ds.BSN
AND docs.Type <= 0 AND docs.ParentId = @ParentId AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
AND ds.Content IS NULL AND ds.RbsId IS NOT NULL and Dts.HistVersion=0
AND docs.IsCurrentVersion <= 1 WHERE ds.SiteId = @SiteId 
UNION ALL
SELECT distinct(Versions.Id), docs.DirName AS DirName, docs.LeafName, versions.UIVersion, 0, versions.Size
FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
inner join DocsToStreams Dts WITH(NOLOCK) on Dts.SiteId=versions.SiteId and Dts.DocId=versions.Id and Dts.HistVersion=versions.UIVersion
inner join DocStreams ds WITH(NOLOCK) on Dts.SiteId=ds.SiteId and Dts.DocId=ds.DocId and Dts.Partition=ds.Partition and Dts.BSN=ds.BSN
AND ds.Content IS NULL AND ds.RbsId IS NOT NULL and Dts.HistVersion<>0
 WHERE versions.SiteId = @SiteId
),
StubFilesWithRowNum(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RowNum)
as
(
select *,ROW_NUMBER() Over (order by StubFiles.Id desc) RowNum from StubFiles With(nolock)
)
select Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RowNum FROM StubFilesWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";

        public const string GetStubBlobIdByRBSId_Select_rbs_internal_blobs = @"
        SELECT store_blob_id
        FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH (INDEX(rbs_internal_blobs_pk),NOLOCK) 
        WHERE collection_id =CONVERT(int,substring(@RBSId,9,4)) AND blob_number=CONVERT(bigint,SUBSTRING(@RBSId,1,8))";

        #endregion SOIntegration

        #region Connector

        public const string GetDocInfoByDocIdForConnector_Select_AllDocs = @"SELECT WebId,ListId,ParentId,UIVersion,Level,InternalVersion,CheckoutUserId,StreamSchema FROM AllDocs with(nolock) WHERE SiteId=@SiteId AND Id=@Id";

        public const string GetParticalContentFromDbForConnector_Select_DocStreams = @"SELECT cast(Content as varbinary(210)) 
FROM DocStreams WITH (INDEX(AllDocStreams_CI),NOLOCK) 
where SiteId=@SiteId 
AND Id=@Id 
AND InternalVersion=@InternalVersion";

        public const string GetContentAndRbsIdFromDB_Select_TVF_DocsToStreams_SiteDocHistVerLvlPart_TVF_DocStreams_CI = @"
DECLARE @rbsId varbinary(64), @content varbinary(32), @histVersion int, @rowc int

IF @IsCurrentVersion=1
    SET @histVersion=0
ELSE
    SET @histVersion=@UIVersion
BEGIN
    SELECT @rbsId=DS.RbsId, @content=CAST(DS.Content AS varbinary(32)) FROM 
        TVF_DocsToStreams_SiteDocHistVerLvlPart(@SiteId,@DocId,@HistVersion,@Level,@Partition) as DTS
    CROSS APPLY 
        TVF_DocStreams_CI(DTS.SiteId,DTS.DocId,DTS.Partition,DTS.BSN) as DS
    ORDER BY DS.BSN DESC

    SET @rowc=@@ROWCOUNT
END
IF (@rowc>0)
    SELECT @content Content, @rbsId RbsId,@rowc [Rows]
";

        public const string GetStoreBlobIdByRBSId_Select_mssqlrbs_resources_rbs_internal_blobs = @"SELECT store_blob_id 
FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH (INDEX(rbs_internal_blobs_pk),NOLOCK) 
WHERE 
collection_id =CONVERT(int,substring(@RBSId,9,4)) AND 
blob_number=CONVERT(bigint,SUBSTRING(@RBSId,1,8))";

        public const string GetItemsInRecycleBin_Select_RecycleBin_AllDocs = @"SELECT RecycleBin.ItemType, RecycleBin.DocId, RecycleBin.DirName, RecycleBin.LeafName, RecycleBin.SiteId, RecycleBin.WebId, AllDocs.Level,  AllDocs.UIVersion, AllDocs.InternalVersion 
                                    FROM RecycleBin(nolock) INNER JOIN AllDocs(nolock) on RecycleBin.DocId = AllDocs.Id  WHERE RecycleBin.SiteId=@SiteId AND RecycleBin.ListId=@ListID AND (RecycleBin.ItemType=1 OR RecycleBin.ItemType=5)";

        public const string GetVersionsInRecycleBin_Select_RecycleBin_AllDocVersions = @"SELECT Id, UIVersion, InternalVersion, Level, version.Size FROM RecycleBin(nolock)
                    INNER JOIN AllDocVersions version(nolock) ON  version.SiteId=@SiteId AND version.Id=RecycleBin.DocId AND version.UIVersion=RecycleBin.DocVersionId
                    WHERE ListId=@ListId AND ItemType = 2";

        public const string GetItemVersionsById_Select_AllDocVersions = @"SELECT UIVersion, InternalVersion, Level, Size FROM AllDocVersions(nolock) WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId<>0x";

        public const string GetItemCountById_Select_AllDocs = "SELECT COUNT(*) FROM AllDocs(nolock) WHERE SiteID=@SiteId and Id=@Id";

        public const string GetListCountById_Select_AllLists = "SELECT COUNT(*) FROM AllLists(nolock) WHERE tp_SiteId=@SiteID AND tp_WebId=@WebID AND tp_ID=@ListID";

        public const string GetWebCountById_Select_AllWebs = "SELECT COUNT(*) FROM AllWebs(nolock) WHERE SiteId=@SiteId and Id=@WebId";

        public const string GetSiteCountById_Select_AllSites = "SELECT COUNT(*) FROM AllSites(nolock) WHERE Id=@SiteId";

        public const string GetItemDeleteTransactionIdById_Select_AllDocs = "SELECT top 1 DeleteTransactionId FROM AllDocs(nolock) WHERE SiteID = @SiteId AND Id=@Id ORDER BY UIVersion DESC";

        public const string GetSiteIdCollectionByFeatureId_Select_Features = @"SELECT SiteId FROM Features (NOLOCK)  where FeatureId = @FeatureId";

        public static string GetRecycleItemProperties_Select_AllDocs(Guid parentId, int level)
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.Append(@"SELECT top 1 MetaInfo FROM AllDocs with(nolock) WHERE SiteId=@SiteId AND Id=@Id AND (DeleteTransactionId >0x OR DeleteTransactionId<0x) ");
            if (parentId != Guid.Empty)
            {
                commandBuilder.Append("AND ParentId = @ParentId ");
            }
            if (level > 0)
            {
                commandBuilder.Append("AND Level = @Level ");
            }
            commandBuilder.Append("Order BY UIVersion DESC");
            return commandBuilder.ToString();
        }

        public static string GetDocFlagById_Select_AllDocs(Guid parentId, string parentFolderRelativeUrl, string leafName, bool isEffectRecycle)
        {
            const string commandText = @"SELECT DocFlags 
FROM AllDocs with(nolock) 
WHERE SiteId=@SiteId 
AND Id=@Id 
AND Level=@Level 
AND UIVersion=@UIVersion ";
            var commandBuilder = new StringBuilder();
            commandBuilder.Append(commandText);
            if (parentId != Guid.Empty)
            {
                commandBuilder.Append(" AND ParentID=@ParentID ");
            }
            if (!string.IsNullOrEmpty(parentFolderRelativeUrl))
            {
                commandBuilder.Append(" AND DirName=@DirName ");
            }
            if (!string.IsNullOrEmpty(leafName))
            {
                commandBuilder.Append(" AND LeafName=@LeafName ");
            }
            commandBuilder.Append(!isEffectRecycle
                ? " AND DeleteTransactionId=0x"
                : " AND (DeleteTransactionId=0x or DeleteTransactionId<>0x )");
            return commandBuilder.ToString();
        }

        #endregion Connector

        #region discover field

        public const string GetListWithRootFolderById_Select_AllLists_AllDocs = @"
                SELECT tp_Title,tp_RootFolder,tp_BaseType,tp_Flags,tp_ServerTemplate,tp_MaxMajorwithMinorVersionCount,ad.DirName, ad.LeafName
                FROM AllLists as al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) 
                ON ad.SiteId= @SiteId AND ad.DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND Level=1
                and al.tp_SiteId = @SiteId AND al.tp_WebId = @WebId AND al.tp_Id = @ListId";

        public const string GetListById_Select_AllLists = @"
        SELECT tp_Title,tp_RootFolder,tp_BaseType,tp_Flags,tp_ServerTemplate,tp_WebId,tp_MaxMajorwithMinorVersionCount
        FROM AllLists WITH(NOLOCK) 
        WHERE tp_ID=@ListId and tp_WebId=@WebId and tp_SiteId=@SiteId";

        public const string GetItemVersionsByRowId_Select_AllUserData = @"
        SELECT tp_UIVersion, tp_Modified, tp_IsCurrent, tp_GUID, tp_ID ,tp_UIVersionString,tp_Level,tp_Size,tp_IsCurrentVersion
        FROM AllUserData With(NOLOCK) 
        WHERE [tp_DeleteTransactionId]=0x
        AND [tp_SiteId]=@SiteId 
        AND [tp_ListId]=@ListId 
        AND ([tp_IsCurrentVersion]=0 OR [tp_IsCurrentVersion]=1) 
        AND [tp_id]=@docLibId 
        And [tp_RowOrdinal]=0 
        ORDER BY tp_UIVersion DESC";

        public const string GetWebUrlById_Select_AllWebs = @"
        SELECT AllWebs.FullUrl FROM ALLWebs with(nolock) 
        WHERE AllWebs.Id = @WebId AND AllWebs.DeleteTransactionId=0x AND AllWebs.SiteId=@SiteId";

        public const string GetDeletedByUserTitleOfList_Select_UserInfo_AllLists_RecycleBin = @"select u.tp_Title as username from recyclebin r with(nolock)
                                                left join userinfo u with(nolock) on  u.tp_siteid=r.siteid and u.tp_id=r.deleteUserID
                                                inner join alllists a with(nolock) on r.siteid=a.tp_SiteId and r.listid=a.tp_id and r.webid=a.tp_WebId
                                                where r.itemtype=4 and r.siteid=@siteId and a.tp_WebId=@webId and a.tp_id=@ListId;";

        public const string GetCheckoutItemsInList_Select_AllDocs = @"SELECT doc.CheckoutUserId, doc.DoclibRowId, Id FROM AllDocs as doc WITH(NOLOCK)
                                              WHERE doc.level = 255 AND doc.SiteId = @SiteId AND doc.WebId = @WebId AND doc.ListId = @ListId  
                                              AND doc.CheckoutUserId IS NOT NULL AND doc.DoclibRowId IS NOT NULL AND DeleteTransactionId=0x";

        public const string GetListUrlById_Select_AllDocs = "Select top 1 DirName,LeafName from AllDocs where ListId=@ListId and SiteId=@SiteId order by Dirname, LeafName";

        #endregion Discover field

        #region discover property

        public static string GetListIdByDirNameLeafName_Select_AllDocs => AveDiscoverQueryString.ListIdByItem;

        public static string GetWebByFullUrlAndSiteId_Select_AllWebs => AveDiscoverQueryString.WebForSP1;

        public static string GetItemIdModifiedInfoByName_Select_AllDocs => AveDiscoverQueryString.ItemLastModifiedTimeWithDirName;

        public static string GetItemLastModifiedTimeByRowId_Select_AllUserData => AveDiscoverQueryString.ItemLastModifiedTimeByListIdAndDoclibRowId13;

        public static string GetItemLastModifiedTimeByDocId_Select_AllDocs => AveDiscoverQueryString.ItemLastModifiedTimeWithoutDoclibRowId13;

        public static string GetItemTpGuidAndIdMapping_Select_AllUserData => AveDiscoverQueryString.ItemIdAndTPGUID;

        public static string GetItemOrVersionCountByLeafname_Select_AllDocs => AveDiscoverQueryString.IsHaveSameNameByLeafName;

        public static string GetItemOrVersionCountByTpGuid_Select_AllUserData => AveDiscoverQueryString.IsHaveSameNameByTpGuid;

        public static string GetItemCurrentVersionWebParts_Select_AllWebParts => AveDiscoverQueryString.ItemWebParts;

        public static string GetItemSizeAndParentIdByDocId_Select_AllDocs => AveDiscoverQueryString16.ItemSizeAndParnetId;

        public static string GetAuthorAndEditorByDocIdParentId_Select_AllUserData => AveDiscoverQueryString.AuthorAndEditor;

        public static string GetUserTitleById_Select_UserInfo => AveDiscoverQueryString.UserTitle;

        public static string GetAllWebsBySiteId_Select_AllWebs => AveDiscoverQueryString16.DiscoverAllWebs;

        public static string GetRootWebBySiteId_Select_AllWebs => AveDiscoverQueryString.RootWebForSP1;

        public static string GetViewsByListId_Select_AllWebParts => AveDiscoverQueryString.ListViews;

        public static string GetStubFilesCountInFolder_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams => AveQueryString16.Sp13StubFilesInFolderCount;

        public static string GetStubFilesCountInFolderWithRecycleBin_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams => AveQueryString16.Sp13StubFilesInFolderCountWithRecycleBin;

        public static string GetItemStubAttachmentsCountInFolder_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams => AveQueryString16.Sp13ItemStubAttachmentsInFolder;

        public static string GetItemStubAttachmentsCountInFolderWithRecycleBin_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams => AveQueryString16.Sp13ItemStubAttachmentsInFolderWithRecycleBin;

        public static string GetWebContentTypesByWebUrl_Select_ContentTypes => AveDiscoverQueryString.WebContentTypesForSP1;

        public static string GetChangedItemSecurity_Select_EventCache => AveDiscoverQueryString.ItemSecurityChanged;

        public static string GetChangedListAlerts_Select_EventCache_ImmedSubscriptions_SchedSubscriptions => AveDiscoverQueryString.ListAlertChanged;

        public static string GetChangedListViews_Select_EventCache_AllWebParts => AveDiscoverQueryString.ListViewChanged;

        public static string GetChangedListSecurity_Select_EventCache => AveDiscoverQueryString.ListSecurityChanged;

        public static string GetChangedListContentTypes_Select_EventCache => AveDiscoverQueryString.ListContentTypeChanged;

        public static string GetChangedWebSecurity_Select_EventCache => AveDiscoverQueryString.WebSecurityChanged;

        public static string GetItemDirNameAndRowIdByDocId_Select_AllDocs => AveDiscoverQueryString.ItemDirNameAndLibRowId13;

        public static string GetAttachmentsByParentFolderUrl_Select_AllDocs => AveDiscoverQueryString.AttachmentsByCustomItem13;

        public static string GetChangeEventsInWeb_Select_EventCache => AveDiscoverQueryString.ListChangedEvent;

        public static string GetViewWebPartChangedListIdsForLists_Select_AllDocs_EventCache => AveDiscoverQueryString16.ListViewWebPartChangedEvent;

        public static string GetChangedWebContentTypes_Select_EventCache => AveDiscoverQueryString.WebContentTypeChangesInEventCache;

        public static string GetChangedSite_Select_EventCache => AveDiscoverQueryString.SiteChanged;

        public static string GetChangedWebsInSite_Select_EventCache_AllWebs => AveDiscoverQueryString16.DiscoverChangedWebs;

        public static string GetChangedSiteSecurity_Select_EventCache => AveDiscoverQueryString.SiteSecurityChanged;

        public static string GetDeletedSiteChangeEvent_Select_EventCache => AveDiscoverQueryString.SiteDeleted;

        #endregion
        #endregion
        #region select methods
        #region system table or proc

        public static string GetGroupNameByLogins_Select_sysusers_syslogins(string logins) =>
            $"SELECT sys.sysusers.name FROM sys.sysusers WITH(NOLOCK) INNER JOIN master.sys.syslogins WITH(NOLOCK) ON sys.sysusers.sid = syslogins.sid WHERE loginname  IN ('{logins}')";

        #endregion

        /// <summary>
        /// 查询MetadataService change log中特定scope下的change 数据，固定动态拼接语句，condition与参数值有关，所以都放到里面一起处理了
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="scopeId"></param>
        /// <param name="scopeType"></param>
        /// <param name="partitionId"></param>
        /// <param name="dynamicParameters">动态拼接的条件的参数，以集合的形式返回</param>
        /// <returns></returns>
        public static string GetChangeMetadataWithCondition_Select_ECMChangeLog(DateTime? startTime, DateTime? endTime, Guid scopeId, AveTermChangeItem.ChangedItemType scopeType, Guid partitionId, out Dictionary<string, object> dynamicParameters)
        {
            dynamicParameters = new Dictionary<string, object>();
            var query = @"SELECT * FROM ECMChangeLog WITH(NOLOCK) where PartitionId = @PartitionId";
            dynamicParameters.Add("@PartitionId", partitionId);
            switch (scopeType)
            {
                case AveTermChangeItem.ChangedItemType.TermSet:
                    {
                        query += " AND TermSetUniqueId = @TermSetUniqueId";
                        dynamicParameters.Add("@TermSetUniqueId", scopeId);
                        break;
                    }
                case AveTermChangeItem.ChangedItemType.Group:
                    {
                        query += " AND GroupUniqueId = @GroupUniqueId";
                        dynamicParameters.Add("@GroupUniqueId", scopeId);
                        break;
                    }
                default:
                    {
                        //其他类型目前都没有实际应用，以后用到需要在此处加处理
                        return string.Empty;
                    }
            }
            if (startTime.HasValue)
            {
                query += " AND ChangeTime >= @ChangeTimeFrom";
                dynamicParameters.Add("@ChangeTimeFrom", startTime.Value);
            }
            if (endTime.HasValue)
            {
                query += " AND ChangeTime <= @ChangeTimeTo";
                dynamicParameters.Add("@ChangeTimeTo", endTime.Value);
            }

            return query;
        }

        /// <summary>
        /// 根据索引动态拼接id条件，用来查询指定的一些TermSet的信息
        /// </summary>
        /// <param name="idCollection"></param>
        /// <param name="index"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static string GetTermSetsByIdCollection_Select_ECMTermSet(List<int> idCollection, int index, int batchSize)
        {
            const string cmdText = @"Select et.Id, et.PartitionId, et.CreatedTime, et.LastModifiedTime, et.Owner, et.CustomSortOrder, et.UniqueId, et.Name, et.Description,
                               et.Type, et.IsOpen, et.AvailableForTagging, et.Stakeholders, et.Contact, et.GroupId from ECMTermSet et WITH(NOLOCK) where 
                               et.Id in ";
            var builder = new StringBuilder();
            builder.Append("( ");
            for (var offset = 0; offset < batchSize; offset++)
            {
                builder.Append(idCollection[offset + index]);
                builder.Append(offset == batchSize - 1 ? " " : ", ");
            }
            builder.Append(" )");

            return cmdText + builder;
        }

        public static string GetCheckoutUserIdByItemGuid_Select_AllDocs(Guid parentId, int level, int version)
        {
            var cmdText = @"
            SELECT CheckoutUserId 
            FROM AllDocs with(nolock) 
            WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x";
            if (parentId != Guid.Empty)
            {
                cmdText += " AND ParentId=@ParentId ";
            }
            if (level == 255)
            {
                cmdText += " AND Level = @Level";
            }
            if (version > 0)
            {
                cmdText += " AND UIVersion = @Version";
            }
            return cmdText;
        }

        public static string GetUserOrGroupByName_Select_Alldocs(AveAccountSearchFlag mFlag)
        {
            const string searchGroup = @"
            select distinct top 201 title as loginName, title as displayName, '' as eMail, '2' as type 
            from groups with (nolock) 
            where (@siteId is null or SiteId = @siteId) and (Title like '%'+@displayName+'%')";
            const string searchUser = @"
            select distinct top 201 tp_login as loginName, tp_title as displayName, tp_Email as eMail, (case when tp_DomainGroup=1 then '1' else '0' end) as type 
            from UserInfo with (nolock) 
            where (@siteId is null or tp_SiteID = @siteId) and tp_Deleted=0 and (tp_IsActive=1 or tp_Login='SHAREPOINT\system') 
            and ((tp_Title like '%'+@displayName+'%') or (tp_Login like '%'+@loginName+'%') or (tp_Email like '%'+@emailAddress+'%'))";
            const string orderBy = " order by loginName";
            const string unionAll = " union all ";
            var sqlText = new StringBuilder();
            if ((mFlag & AveAccountSearchFlag.IncludeSharePointGroup) != AveAccountSearchFlag.None)
            {
                sqlText.Append(searchGroup);
            }
            if ((mFlag & AveAccountSearchFlag.IncludeSharePointUser) != AveAccountSearchFlag.None)
            {
                if (sqlText.Length != 0)
                {
                    sqlText.Append(unionAll);
                }
                sqlText.Append(searchUser);
            }
            sqlText.Append(orderBy);
            return sqlText.ToString();
        }

        public static string GetSiteInfoBySiteIdFilterCondition_Select_AllSites_AllWebs(string siteIdFilterCondition)
        {
            const string cmd = @"select s.id,
                                  (case when s.hostheader is null then @appUrl + w.fullurl
                                   else @appSuffix + s.hostheader + '/' + w.FullUrl end) as url, w.title 
                                   from AllSites s with (nolock) 
                                   inner join AllWebs w with (nolock) 
                                   on s.id=w.siteid and w.parentWebId is null and w.DeleteTransactionId = 0x and s.Deleted = CONVERT(bit, 0) ";
            return cmd + siteIdFilterCondition;
        }

        /// <summary>
        /// 根据user login search没有权限的user信息的query语句
        /// </summary>
        /// <param name="searchUsers"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public static string GetSiteNoPermisssionAccountByCondition_Select_UserInfo_RoleAssignment_GroupMembership_Groups(List<string> searchUsers)
        {
            const string getAllNoPermissionAccountsCmd = @"
            WITH  noPermissionUserAndGroup(SiteId, GroupName, LoginName) AS 
            (
 
                SELECT  tp_SiteID, null , tp_Login 
                From UserInfo with (nolock) 
                Where tp_isActive = 1 AND tp_siteAdmin = 0 and tp_SiteID = @siteId
                and tp_id not in
                (   
			        SELECT DISTINCT UserInfo.tp_ID 
			        FROM UserInfo with(nolock)
			        inner join
			        RoleAssignment with(nolock)
			        on UserInfo.tp_SiteID=RoleAssignment.SiteId 
			        and UserInfo.tp_ID = RoleAssignment.PrincipalId
			        Where RoleAssignment.SiteId=@siteId and RoleAssignment.ScopeId=@scopeId and UserInfo.tp_SiteID=@siteId

			        union
			        select distinct GroupMembership.MemberId from GroupMembership with(nolock)
			        inner join RoleAssignment with(nolock)
			        on GroupMembership.SiteId = RoleAssignment.SiteId and GroupMembership.GroupId=RoleAssignment.PrincipalId
			        where GroupMembership.SiteId=@siteId and RoleAssignment.SiteId=@siteId and RoleAssignment.ScopeId=@scopeId
                )
                union
                 SELECT  SiteId, Title, Title
                 FROM    Groups with (nolock)  where groups.SiteId = @siteId and
                 Groups.id not in
                 (
	                 SELECT DISTINCT Groups.ID FROM  Groups with (nolock)  inner JOIN
	                 RoleAssignment with (nolock) ON groups.SiteId = RoleAssignment.SiteId AND groups.id = principalId
                     where  Groups.SiteId = @siteId and RoleAssignment.ScopeId=@scopeId
                 )
            ) 
            select * from noPermissionUserAndGroup WITH(NOLOCK)";
            var conditionStringBuilder = new StringBuilder();
            if (searchUsers != null && searchUsers.Count > 0)
            {
                conditionStringBuilder.Append(" where LoginName in (");
                searchUsers.ForEach(login =>
                {
                    conditionStringBuilder.AppendFormat("N'{0}',N'i:0#.w|{0}',N'c:0+.w|{0}',", login.Replace("'", "''"));
                });
                conditionStringBuilder.Length--;
                conditionStringBuilder.Append(")");
            }
            return getAllNoPermissionAccountsCmd + conditionStringBuilder;
        }

        #region GetDuplicateFileQuery

        //select two views (docs, lists)
        /// <summary>
        /// TODO: improve it in different way.
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="webIds"></param>
        /// <param name="searchFile"></param>
        /// <param name="searchAttachment"></param>
        /// <param name="includeFileExtensions"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="fileNamePattern"></param>
        /// <returns></returns>
        public static string GetDuplicateFileQuery_Select_Docs_Lists_Sites(List<string> siteIds, List<string> webIds, bool searchFile, bool searchAttachment, List<string> includeFileExtensions, List<string> excludeFileNames, string fileNamePattern)
        {
            var text = @"SELECT D.LeafName as leafName, D.DirName as dirName, D.SiteId as siteId, D.WebId as webId, 
	        D.ListId as listId, D.Id as docId, L.tp_BaseType as listType, D.UIVersionString as versionStr, IsNull(D.Size, 0) as fileSize, D.[TimeLastModified] as modifiedTime,
            S.HostHeader as hostHeader 
            FROM Docs D WITH(NOLOCK)
            inner join Lists L with (nolock) on D.listid = L.tp_id  and D.webId = L.tp_webId 
            inner join Sites S with (nolock) on D.SiteId = S.Id 
            where D.ListId is not null AND D.Size is not null AND D.IsCurrentVersion = 1 AND D.Type = 0 AND (L.tp_Flags & 256 = 0)";
            var siteFilter = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(siteIds);
            if (!string.IsNullOrEmpty(siteFilter))
            {
                text = text + " and D.SiteId in (" + siteFilter + ")";
            }
            var webFilter = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(webIds);
            if (!string.IsNullOrEmpty(webFilter))
            {
                text = text + " and D.WebId in (" + webFilter + ")";
            }
            if (searchFile && !searchAttachment)
            {
                text += " and (L.tp_BaseType = 1)";
            }
            else if (!searchFile && searchAttachment)
            {
                text += " and not (L.tp_BaseType in (1,4))";
            }
            var condByCommaSeparatedList = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(includeFileExtensions);
            if (!string.IsNullOrEmpty(condByCommaSeparatedList))
            {
                text = text + " and (IsNull(d.Extension,'') in (" + condByCommaSeparatedList.ToLower(CultureInfo.InvariantCulture) + "))";
            }

            GetExcludeFileNamesCond(excludeFileNames, ref text);
            if (!string.IsNullOrEmpty(fileNamePattern) && fileNamePattern.Trim() != string.Empty)
            {
                text = $"{text} and (d.LeafName like N'%{fileNamePattern.Trim()}%')";
            }
            return text;
        }

        private static void GetExcludeFileNamesCond(IEnumerable<string> excludeFileNames, ref string sSQL)
        {
            var clearList = new List<string>();
            var fuzzyList = new List<string>();
            foreach (var excludeName in excludeFileNames)
            {
                if (excludeName.IndexOf("*", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fuzzyList.TryAddDistinctValue(excludeName, true);
                }
                else
                {
                    clearList.TryAddDistinctValue(excludeName, true);
                }
            }
            if (fuzzyList.Count > 0)
            {
                sSQL = (
                    from current in fuzzyList
                    where (current.StartsWith("*", StringComparison.OrdinalIgnoreCase) ||
                            current.EndsWith("*", StringComparison.OrdinalIgnoreCase)) &&
                            current.IndexOf("**", StringComparison.OrdinalIgnoreCase) < 0
                    select current.Replace("*", "%")).Aggregate(sSQL,
                            (current1, currentStr) => $"{current1} and (not (lower(d.LeafName) like '{currentStr.ToLower(CultureInfo.InvariantCulture)}'))");
            }
            if (clearList.Count > 0)
            {
                var text = clearList.Aggregate(string.Empty, (current, current2) => current + $"N'{current2.Trim().ToLower(CultureInfo.InvariantCulture)}',");
                text = text.Trim(',');
                if (!string.IsNullOrEmpty(text))
                {
                    sSQL = sSQL + " and (not (lower(d.LeafName) in (" + text + ")))";
                }
            }
        }

        #endregion

        #region discover

        public static string GetSubWebsByParentWebId_Select_AllWebs(bool includeRecycleBin)
        {
            return includeRecycleBin
                ? AveDiscoverQueryString16.SubWebsWithRecycleBin
                : AveDiscoverQueryString16.SubWebs;
        }

        public static string GetAllListsInWeb_Select_AllLists(bool includeRecycleBin)
        {
            return includeRecycleBin ? AveDiscoverQueryString16.ListsWithRecycleBin : AveDiscoverQueryString16.Lists;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public static string GetStubAllItemAndVersions_Select_AllDocs_DocsToStreams_DocStreams(bool includeRecycleBin, bool isInSystemFolder)
        {
            string result;
            if (isInSystemFolder)
            {
                if (includeRecycleBin)
                {
                    result = AveQueryString16.Sp16StubAllItemAndVersionsWithRecycleBin
                        .Replace("@WHEREAllDocs", DiscoverConditionString.WebStubItemsWithRecycleBin)
                        .Replace("@WHEREAllDocVersions", DiscoverConditionString.WebStubItemsForAllDocVersionsWithRecycleBin);
                }
                else
                {
                    result = AveQueryString16.Sp16StubAllItemAndVersions
                        .Replace("@WHEREAllDocs", DiscoverConditionString.WebStubItems)
                        .Replace("@WHEREAllDocVersions", DiscoverConditionString.WebStubItemsForAllDocVersions);

                }
            }
            else
            {
                if (includeRecycleBin)
                {
                    result = AveQueryString16.Sp16StubAllItemAndVersionsWithRecycleBin
                        .Replace("@WHEREAllDocs", DiscoverConditionString.ListStubItemsWithRecycleBin)
                        .Replace("@WHEREAllDocVersions", DiscoverConditionString.ListStubItemsForAllDocVersionsWithRecycleBin);
                }
                else
                {
                    result = AveQueryString16.Sp16StubAllItemAndVersions
                        .Replace("@WHEREAllDocs", DiscoverConditionString.ListStubItems)
                        .Replace("@WHEREAllDocVersions", DiscoverConditionString.ListStubItemsForAllDocVersions);

                }
            }
            return result;
        }

        public static string GetDocInfoByIdsBatch_Select_AllDocs(List<Guid> docIdCollection, AveDiscoverReader discoverReader,bool isSystemFolder)
        {
            //@SiteId
            var idCollectionStringBuilder = new StringBuilder();
            docIdCollection.ForEach(id => idCollectionStringBuilder.AppendFormat("'{0}',", id));
            idCollectionStringBuilder.Length--;
            var condition = string.Format(isSystemFolder ? DiscoverConditionString.SystemDocIdsFor13 : DiscoverConditionString.DocIdsFor13, idCollectionStringBuilder);
            return discoverReader.GetDocInfoForIBQueryString().Replace("@WHERE", condition);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docver")]
        public static string GetListItemsForIB_Select_EventCache(bool isSystemFolder)
        {
            if (isSystemFolder)
            {//@SiteId  @WebId @startTime @endTime
                return string.Format(AveDiscoverQueryString.ItemChangedInCache.Replace("@WHERE", DiscoverConditionString.WebItemChanged), AveWrapperConstants.MaxRows);
            }
            //@SiteId  @WebId @ListId @startTime @endTime
            var whereString = WrapperConfiguration.IgnoreDiscoverModifiedBySystem
                ? DiscoverConditionString.ListItemChangedIgnoreModifiedBySystem
                : DiscoverConditionString.ListItemChanged;
            return string.Format(AveDiscoverQueryString.ItemChangedInCache.Replace("@WHERE", whereString), AveWrapperConstants.MaxRows);

        }

        public static string GetViewInfosByIds_Select_AllWebParts(List<Guid> docIds)
        {
            //@SiteId
            var condition = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(docIds);
            return string.Format(AveDiscoverQueryString.SpecifyViewDocIds, condition);
        }

        public static string GetFolderAlertsByIds_Select_ImmedSubscriptions_SchedSubscriptions(List<Guid> folderIds)
        {
            var folderIdString = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(folderIds);
            return AveDiscoverQueryString.FolderAlerts.Replace("@WHERE", folderIdString);
        }

        public static string GetListByIds_Select_AllLists_AllDocs(List<Guid> listIds)
        {
            const string getListByIds = @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate,al.tp_DeleteTransactionId
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON ad.SiteId=al.tp_SiteId AND ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_SiteId=@siteId AND al.tp_WebId=@WebId AND tp_ID in ({0})";
            string idCollectionString = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(listIds);
            return string.Format(getListByIds, idCollectionString);
        }

        public static string GetContentTypeByIds_Select_ContentTypes(List<string> ids)
        {
            var condition = AveQueryStringCommonUtility.GetCondByCommaSeparatedWithoutQuoteList(ids);
            return string.Format(AveDiscoverQueryString.WebContentTypesForIBForSP1, condition);
        }

        public static string GetUserInfoByIds_Select_UserInfo(List<int> ids)
        {
            const string formatString = "tp_id='{0}' or ";
            const string getUsersByIds = @"select tp_id,tp_DomainGroup,tp_title,tp_login from UserInfo WITH(NOLOCK) where tp_SiteID=@siteId and ({0})";
            var condition = AveQueryStringCommonUtility.GetCondByCommaSeparatedWithoutQuoteList(ids, formatString, 4);
            return string.Format(getUsersByIds, condition);
        }

        public static string GetGroupInfoByIds_Select_Groups(List<int> ids)
        {
            const string formatString = "ID='{0}' or ";
            const string getUsersByIds = @"select ID,Title from Groups WITH(NOLOCK) where SiteId=@siteId and ({0})";
            var condition = AveQueryStringCommonUtility.GetCondByCommaSeparatedWithoutQuoteList(ids, formatString, 4);
            return string.Format(getUsersByIds, condition);
        }

        public static string GetItemStubInfo_Select_AllDocs_DocsToStreams_DocStreams(List<Guid> ids, bool includeRecycleBin)
        {
            var idCondition = AveQueryStringCommonUtility.GetCondByCommaSeparatedList(ids);
            var condition = string.Format(includeRecycleBin ? AveQueryString16.Sp16ItemStubsByIdsWithRecycleBin : AveQueryString16.Sp16ItemStubsByIds, idCondition);
            return AveQueryString16.Sp16ItemStubsByIdsCammandLine.Replace("@WHEREAllDocs", condition);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentIdCollection"></param>
        /// <param name="startIndex"> min 0  , max parentIdCollection</param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static string GetUserDataSizeByParentId_Select_AllUserData(List<string> parentIdCollection, int startIndex, int batchSize)
        {
            if (startIndex >= parentIdCollection.Count)
            {
                return string.Empty;
            }
            const string commandBaseStr = "SELECT SUM(cast(tp_Size as bigint)) FROM AllUserData with(nolock) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_ParentId in ({0}) AND tp_DeleteTransactionId=0x";
            StringBuilder commandArgsBuilder = new StringBuilder();
            int endIndex = parentIdCollection.Count > startIndex + batchSize ? startIndex + batchSize : parentIdCollection.Count;
            for (var k = startIndex; k < endIndex; k++)
            {
                commandArgsBuilder.AppendFormat("'{0}',", parentIdCollection[k]);
            }
            return string.Format(commandBaseStr, commandArgsBuilder.ToString().TrimEnd(','));
        }
        #endregion discover
        #endregion

    }
}

