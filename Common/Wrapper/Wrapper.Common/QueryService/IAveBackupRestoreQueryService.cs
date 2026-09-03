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
using System.Text;
using System.IO;
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveBackupRestoreQueryService : IAveQueryService
    {
        #region Backup

        List<AveUserInfo> GetSiteUsers(IAveSite site, bool allAvailableUser);

        //
        // Summary:
        //     Gets the collection of AveUserInfo objects that all the users are explicitly assigned permissions
        //     in the Web site.
        //
        // Returns:
        //     An List<AveUserInfo> object that represents the users.
        List<AveUserInfo> GetWebUsers(IAveWeb web, bool allAvailableUser);

        List<AveGroupInfo> GetGroups(IAveWeb web, bool allGroups);

        AveSiteSettingInfo GetSiteSettingFromSites(IAveSite site);

        long GetSiteSizeFromSites(IAveSite site);

        AveSiteSettingInfo GetFullSiteSetting(IAveSite site);

        string GetPathByNative(Dictionary<string, object> parameters, string type);

        void GetSubWebsAndPageInfo(Dictionary<string, object> parameters, Dictionary<string, Dictionary<Guid, string>> websAndPages, string type);

        AveWebSettingInfo GetWebSettingFromWebs(IAveWeb web);

        long GetWebSize(IAveWeb web);

        AveListInfo GetListInfo(IAveList list);

        List<AveRoleAssignmentInfo> GetListRoleAssignments(string SiteId, string ScopeId);

        List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info);

        int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex);

        int GetFileModifiedByIdByNative(Guid siteId, Guid parentId, Guid docId);
        int GetFileAuthorIdByNative(Guid siteId, Guid parentId, Guid docId);

        List<AveRoleAssignmentInfo> GetWebRoleAssignments(Guid SiteId, Guid ScopeId);

        bool GetFieldCollectionRelationship(string siteId, string listId, string fieldId);

        string GetListViewSchema(Guid siteId, Guid listId);

        List<AveRoleInfo> GetWebRoles(Guid siteId, Guid FirstUniqueRoleDefinitionWebId);

        List<AveRoleAssignmentInfo> GetItemRoleAssignments(Guid siteId, Guid itemScopeId);

        List<AveContentTypeFileInfo> GetContentTypeCollectionResources(Guid siteId, string folderUrl);

        string GetContentTypeName(Guid siteId, byte[] contentTypeId);

        List<AveWebPartBaseInfo> GetWebParts(Guid siteId, Guid itemId, byte itemlevel, bool itemIsVersion, int version);

        string GetWebPartsInGallery(Guid siteId);

        string GetListTitle(Guid listId);

        void SetWebPartPersonalization(AveWebPartBaseInfo webPartInfo);

        void SetWebPartLists(AveWebPartBaseInfo webPartInfo);

        void GetVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache);

        bool GetDocHasStream(AveBaseItemInfo itemInfo, int internalVersion);

        Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo);

        void GetDocInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache);

        /// <summary>
        /// Query specific count row of DocData(AllDocs) And UserData(AllUserData)
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDocAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count);
        /// <summary>
        /// Query specific count row of DocData(AllDocVersions) And UserData(AllUserData)
        /// </summary>
        /// <param name="siteId">Query index, for performance</param>
        /// <param name="parentId">Parent folder id</param>
        /// <param name="currentDocLibRowId">Query entities which DocLibRowId is larger or equal than currentDocLibRowId</param>
        /// <param name="count">count of row to query</param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetVersionAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count);

        int GetInternalVersion(AveBaseItemInfo itemInfo);

        int GetDocFlag(AveBaseItemInfo info);

        byte[] GetRbsIdByNative(AveBaseItemInfo info);

        string GetStubInfoByNative(Guid siteId, Guid id, int internalVersion);

        string GetFields(Guid webId, Guid listId);

        string GetViewFields(Guid siteId, Guid listId);

        int GetThreadIndexParentId(Guid listId, byte[] threadIndex);

        List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo infoItem);

        Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> GetFolderItemsUserDataJunctions(Guid siteId, Guid parentId);

        AveRBSStubInfo AveRBSBackup_BackupRBSStub(int collectionId, long blob_num, short blobStoreId);

        long AveRBSBackup_GenerateBlobNumber(byte[] rbs_id);

        long AveRBSBackup_WriteBlobInformationToDB(AveRBSStubInfo stubinfo, int collectionId, short blobStoreId);

        byte[] AveRBSExtenderRestore_GenerateRbsId(int collectionId, long blob_num);

        void AveRBSExtenderRestore_CreatePool(byte[] poolId, bool canStoreNewBlobs, int collectionId, short blobStoreId);

        int[] AveRBSCommon_GetCollectionIdAndProviderId();

        List<Guid> AveRBSCommon_GetPoolsOfDB();

        long AveRBSConnectorRestore_RegisterBlob(int collectionId, int blobStoreId, byte[] storePoolId, byte[] storeBlobId, DateTime createTime, long blobSize);

        byte[] AveRBSConnectorRestore_GetRbsId(int collectionId, long blobNumber);

        int AveRBSConnectorRestore_AddPool(int blobSotreId, byte[] storePoolId, int collectionId, int clientVersion);

        int AveRBSConnectorRestore_ClosePool(int blobStoreId, byte[] storePoolId, int poolId, bool canStoreNewBlobs);

        bool AveRBSConnectorRestore_CheckBlobExist(byte[] storePoolId, byte[] storeBlobId, int blobStoreId, int collectionId, ref long blobNumber);

        Dictionary<Guid, string> GetALLWebTemplates(IAveSite site, uint lcid);

        int GetCheckOutUserId(AveBaseItemInfo info);

        List<int> GetDocVersions(AveBaseItemInfo info);

        void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList);

        string GetContentTypeContent(Guid listId, Guid webId);

        AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope);

        void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid listId, Guid defaultViewId);

        void GetViews(Dictionary<string, List<AveViewInfo>> viewCache, Guid listId, Guid defaultViewId);

        List<string> GetFields(Guid siteId, string scope);

        bool CheckContentTypeExist(Guid siteId, byte[] ctId);

        bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, byte[] ctId);

        void DeleteWebPartByNative(Guid siteId, Guid docId, string webPartId);

        /// <summary>
        /// 删除当前page上所有非personal view webpart的personal webpart，只删除当前page version上的(tp_PageVersion=0 and tp_level=@level)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="level"></param>
        void DeleteAllPersonalWebParts(Guid siteId, Guid docId, int level, List<Guid> viewIds);

        string GetWebCTNameById(Guid siteId, string contentTypeId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parentWeb"></param>
        /// <param name="listSettingInfo"></param>
        /// <returns>flag</returns>
        ulong GetListSettingInfoByNative(IAveList list, IAveWeb parentWeb, AveListSettingInfo listSettingInfo);

        List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType);

        List<Dictionary<string, object>> GetSchedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType);

        int SetAttachmentSize(AveBaseItemInfo info);

        Guid GetFirstUniqueRoleDefinitionWebGuid(Guid siteId, Guid scopeId);

        int GetRoleAssignmentCount(Guid siteId, Guid scopeId, int roleId, int principalId);

        void UpdateUserInfo(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField);

        AveGroupInfo GetGroupInfo(Guid siteId, int principalId);

        AveUserInfo GetUserInfo(Guid siteId, int principalId);
        AveUserInfo GetUserInfo(Guid siteId, int principalId, bool checkDeleted);

        bool CheckUserIfAvailable(Guid siteId, int userId);

        Guid GetListId(Guid webId, string listTitle);

        void UpdateSpecialPropertyByNative(string editor, string author, DateTime modified, DateTime created, AveBaseItemInfo info);

        void UpdateModifiedBy(string modifiedBy, string createdBy, string colNameModified, string colNameCreated, AveBaseItemInfo info);

        Guid GetFolderIdByName(AveBaseItemInfo info);

        List<AveHiddenFileInfo> GetHiddenFiles(Guid siteId, Guid webId, Guid listId, Guid folderId);

        Guid GetListItemGuid(Guid listId, int rowId);

        byte[] GetDocStream(AveDocumentInfo info, Guid guid);

        bool IsAttachmentExsits(Guid siteId, Guid parentId, string leafName);

        void GetCurrentVersionDocInfo(Guid siteId, Guid parentId, Guid itemId, Dictionary<string, object> dataCache);

        void AveSOUpdateRbsID(Guid siteID, Guid itemID, int uiVersion, int Size, byte[] data, bool isRbsID);

        Dictionary<int, Guid> GetListAttchmentFolderIds(Guid siteId, Guid attachmentRootFolderId);

        List<string> GetAttachments(Guid siteId, Guid attachmentFolderId);

        Guid GetLookupGUIDById(Guid lookupListId, int rowId);

        IAveQueryDataReader ExportContentByNative(AveBaseItemInfo info, int internalVersion);

        bool CheckContentIfAveStub(Guid siteId, Guid Id, int internalVersion);

        int GetSubWebCounts(Guid siteId, string serverRelativeUrl);

        string GetScopeUrl(Guid siteId, Guid scopeId);

        DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId);

        #region RBSUtility

        AveRBSStubInfo BackupRBSStub(byte[] rbs_id, short blobStoreId, int collectionId);

        #endregion

        #region Workflow

        void GetWorkflowId(List<Guid> tempIds, Guid siteId, Guid webId, int itemId, Guid listId);
        AveWorkflowStatus GetWorkflowStatus(string workflowId);
        AveWorkflowStatus GetWorkflowStatus(Guid workflowId);

        string GetFieldsSchemaXML(Guid webId, Guid listId);

        IAveQueryDataReader BackupTaskItemContext(object siteId, object webId, object hostId, byte[] contextCollectionId);

        IAveQueryDataReader BackupParentItemContext(object siteId, object webId, object hostId, byte[] contextCollectionId);

        IAveQueryDataReader BackupInstance(Guid id);

        void BackupInstanceSelf(Guid webId, Guid id, Hashtable properties, string customFieldProfix);

        Dictionary<string, object> GetDictionary(Guid instanceId);

        IAveQueryDataReader BackupTasks(Guid siteId, Guid webId, Guid listId, Guid workflowInstanceId);

        IAveQueryDataReader BackupHistory(Guid siteId, Guid webId, Guid historyListId, Guid workflowInstanceId, string instanceIdColName);

        IAveQueryDataReader BackupWorkflowEvents(object siteId, object webId, byte[] contextCollectionId);

        #endregion

        #endregion

        #region Restore

        int GetTpIdByTpGuid(Guid tp_guid, Guid listId);

        void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version);

        void UpdateWebsAuthorByNative(int userId, Guid webId);

        void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue);

        /// <summary>
        /// remove item by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="mQueryWorker"></param>
        /// <param name="spSite"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        void RemoveListItemInRecycleBin(IAveSite site, Guid parentId, Guid tp_Guid);

        void RemoveItemInRecycleBin(IAveSite site, Guid parentId, string name);

        //修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        void UpdateAllDocsPropertyByNative(AveBaseItemInfo info, DateTime timeCreated, DateTime timeLastModified, int version);

        bool CreateVersionByNative(AveBaseItemInfo info, int version, RestoringDto restoringDto);

        void ChangeNextItemId(int toId, Guid listId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="rootFolderId"></param>
        /// <param name="itemType">
        /// itemType=1, list item
        /// itemType=2, document
        /// itemType=3, folder
        /// </param>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="mQueryWorker"></param>
        /// <returns></returns>
        int ChangeItemId(
           Guid siteId,
           Guid id,
           Guid rootFolderId,
           int itemType,
           int fromId,
           int toId);

        int ChangeItemId(Guid siteId, Guid listId, Guid parentId, Guid id, int fromId, int toId);
        //检查list下itemId是否被占用,没被占用返回true。
        bool CheckItemIdAvailable(Guid siteId, Guid listId, int itemId);

        DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid listId, int itemId);
        
        void UpdateUserInfoByNative(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField);

        void UpdateVersionByNative(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, int version);

        void ChangeLevelByNative(AveBaseItemInfo info, IAveListItem item, int version, int originaleLevel, int draftOwnerId);
        //TODO:Combine it with the function that with the same name but belong to aveDoc
        void ChangeCheckoutUserID(AveBaseItemInfo info, Guid uniqueID, int newUserID);

        void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID);

        // TODO:Add User Mapping
        // TODO:make this an the same function in AveSPItem one function
        void ChangeCheckoutUserIDForAllVersion(AveBaseItemInfo info, Guid uniqueID, int newUserID);

        void ChangeModerationStatusByNative(AveBaseItemInfo info, IAveFile file, int originalModerationStatus);

        void ChangeModerationStatusByNative(AveBaseItemInfo info, IAveListItem item, int uiVersion, int originalModerationStatus);
        /// <summary>
        /// keep tp_guid
        /// update tp_guid by SQL after created a new item(listitem, document, folder)
        /// </summary>
        /// <param name="mmQueryWorker"></param>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <param name="tp_Guid"></param>
        void ChangeItemTPGuidByNative(AveBaseItemInfo info, Guid siteId, Guid parentId, Guid id, Guid tp_Guid);

        int GetCurrentUIVersion(Guid siteId, Guid id);

        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id);

        DateTime GetLastModifiedByNative(Guid listId, int docLibRowId);

        /// <summary>
        /// just for doc
        /// </summary>
        /// <param name="parentList"></param>
        /// <param name="parentFolder"></param>
        /// <param name="listItem"></param>
        /// <param name="mmQueryWorker"></param>
        bool MoveDocToConflictFolderByNative(Guid listId, string parentFolderServerRelativeUrl, string listItemName, int docLibRowId, Guid listItemUniqueId, Guid conflictFolderUniqueId, string conflictFolderName, DateTime lastModified, bool isSourceWin, Guid siteId);

        bool MoveListItemToConflictFolderByNative(string titleColName, string parentFolderServerRelativeUrl, string conflictFolderName, Guid conflictFolderlistId, Guid conflictFolderUniqueId, int docLibRowId, Guid listItemUniqueId, DateTime lastModified, Guid siteId);

        #region modify for pic slide image thumbnail
        bool QueryBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, ref int author, ref int editor, ref DateTime tp_create, ref DateTime tp_modify, ref DateTime create, ref DateTime modify);

        void UpdateBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, int author, int editor, DateTime tp_create, DateTime tp_modify, DateTime create, DateTime modify);
        #endregion

        int? GetInternalVersion(AveBaseItemInfo info, bool isVersion, Guid id, int UIVersion);

        bool IsCheckOutFile(AveBaseItemInfo info, Guid siteId, Guid parentId, string name);

        bool IsCheckOutFile(Guid siteId, Guid fileId, ref int checkId);

        bool IsCheckOutFile(Guid siteId, string url, ref int checkId);

        bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id);

        //for form library item, to change content.
        void ChangeContentByNative(AveBaseItemInfo info, byte[] content);

        byte GetLevel(AveBaseItemInfo info, int version);

        int SetInternalVersion(AveBaseItemInfo info, RestoringDto restoringDto, int version);

        void SetDocFlagAsContent(AveBaseItemInfo info, int version);

        /// <summary>
        /// checkin comment可以通过API改的，但是这个主要是为了改Approve Comment，因为Approve comment和Checkin Comment是同一个。
        /// </summary>
        /// <param name="checkinComment"></param>
        void UpdateCheckinCommentByNative(AveBaseItemInfo info, Guid fileGuid, string checkinComment);

        void ChangeModerationStatusAndDraftOwnerIdByNative(AveBaseItemInfo info, IAveFile file, int originalModerationStatus);

        void UpdateViewLastModifiedTimeByNative(AveBaseItemInfo info, IAveFile spFile, DateTime timeLastModified);

        void ResetContentByNative(AveSPItemNativeInfo info);

        string GetMaxListItemLeafName(Guid siteId, Guid listId);

        string GetMaxSubLeafName(Guid siteId, Guid parentId);

        void CheckConflictInfoForListItem(Guid siteId, string listRootDir, RestoringDto restoringDto);

        void CheckConflictInfoForListItem(Guid siteId, Guid listId, RestoringDto restoringDto);

        void CheckConflictInfo(Guid siteId, Guid parentId, RestoringDto restoringDto);

        void CheckConflictInfo(Guid siteId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto);

        void CheckConflictInfoForReply(Guid siteId, Guid parentId, string messageId, string fieldColumn, RestoringDto restoringDto);

        void UpdateFileContentByNative(AveSPItemNativeInfo info, Stream fs);

        void DeleteVersionByNative(AveBaseItemInfo info, Guid uniqueId, int uiVersion);

        DateTime GetLastModified(Guid siteId, Guid parentId, int rowId);

        void ChangeInstanceIdByNative(AveBaseItemInfo info, int level, Guid uniqueId, int newInstanceId);

        int GetItemEditorByNative(AveBaseItemInfo info, IAveListItem item);

        void SetItemEditorByNative(AveBaseItemInfo info, IAveListItem item, int modified);

        void RenameAttachment(AveBaseItemInfo info, string oldName, string newName);

        Guid GetAttachmentUniqueId(AveBaseItemInfo info, string realName);

        int GetAttachmentVersion(AveBaseItemInfo info, string realName);

        Guid GetAttachmentsParentID(AveBaseItemInfo info, IAveListItem item);

        /// <summary>
        /// change webpart id
        /// </summary>
        void UpdateWebPartInfo(Guid oldId, Guid siteId, Guid fileId, Guid newId);

        void UpdatePropertiesByNative(string webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties);

        void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion);

        void UpdateView(string webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId);

        void UpdateUserID(string webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal);

        void UpdateItemGuid(Guid tp_Guid, Guid itemUniqueId, Guid parentUniqueId, Guid siteId, bool isCurrentVersion, byte level, int calculatedVersion);

        void UpdatePersonalPropertiesByNative(string webPartId, Guid siteId, int currentUserId, byte[] perUserBytes);

        Dictionary<Guid, Guid> GetAlerts(Guid siteId, Guid listId, int itemId, AveSPAlertHostType hostType);

        int GetLookupIdByGUID(Guid lookupListId, Guid GUID);

        List<Guid> GetAllWebsGuidByNative(Guid siteId);

        void ChangeWSPNameByNative(string originalName, Guid uniqueId, Guid siteId);

        void RemoveDatajunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version);

        Nullable<bool> CurrentIsEBS(IAveFile file, Guid siteId);

        Nullable<bool> CurrentIsRBS(IAveFile file, Guid siteId);

        //void SetRbsIdNull(AveBaseItemInfo baseItemInfo);

        bool ConflictWithFolderInRecycleBin(string name, Guid siteId, Guid uniqueId);

        void RemoveFolderInRecycleBin(string name, IAveRecycleBinItemCollection recycleBin, Guid siteId, Guid uniqueId, string folderName);

        Guid GetFolderIdByName(string name, Guid siteId, Guid parentId);

        void LoadHiddenPages(Dictionary<Guid, string> hiddenPages, Dictionary<Guid, Guid> pageItemSDGuidMapping, Dictionary<string, string> listUrlMapping, Guid siteId, IAveWeb web);

        Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping);

        void UpdateListCreatedByNative(Guid webId, Guid listId, DateTime created);

        bool IsConfictWithRecycle(string name, Guid webId);

        bool IsConflictWithRecycle(Guid siteId, string webUrl);

        void DeleteListAlertsEventsFromEventCache(Guid siteId, Guid webId, Guid listId);

        void UpdateAllDocsPropertyByNative(DateTime timeCreated, DateTime timeLastModified, Guid parentId, Guid siteId, string leafName);

        Guid GetAttachmentUniqueIdByNative(Guid parentId, Guid siteId, string leafName);

        int GetAttachmentInternalVersionByNative(Guid parentId, Guid siteId, string leafName);

        int GetNextAvailableId(Guid listId);

        Guid GetWebId(Guid siteId, string url);

        IAveQueryDataReader GetWebAlerts(Guid siteId, Guid webId);

        bool ListHasLerts(Guid siteId, Guid listId);

        DateTime GetVersionModified(Guid siteId, Guid parentId, int rowId, int uiVersion);

        bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId);

        bool IsItemDeleted(Guid listId, int id, Guid siteId);

        #region RBS Utility

        int[] GetCollectionIdAndProviderId(Guid siteId);

        byte[] RestoreRBSStub(AveRBSStubInfo stubinfo, List<Guid> poolsOfDB, short blobStoreId, int collectionId);

        List<Guid> GetPoolsOfDB();

        #endregion

        #region Workflow

        void HandleWorkflowInstanceConflict(Guid siteId, Guid webId, Guid listId, int itemId, Guid assoid);

        void DeleteSpecificEventFromEventReceiver(Guid siteId, Guid webId, Guid hostId, byte[] contextCollectionId, object sequenceNumber);

        void InsertTableRow(Hashtable data, string tableName);

        void UpdateStatusFieldValue(Guid siteId, Guid listId, Guid tpGuid, byte[] StatusFieldValue, string statusField);

        void UpdateStatusFieldValue(Guid siteId, Guid listId, Guid tpGuid, int tpId, byte[] StatusFieldValue, short rowOrdinal, string statusField);

        int CheckWorkflowInstanceConflict(Guid siteId, Guid webId, Guid listId, Guid assoId, int itemId, int conflictCondition);

        int UpdateTableRow(Hashtable metadata, List<string> excludeField, Hashtable conditionParam, string tableName, string condition);

        void UpdateStatusFieldName(Guid workflowAssociationId, string internalNameStatusField);

        void UpdateConfiguration(Guid workflowAssociationId, int configuration);

        void UpdateAssociationName(Guid idworkflowAssociationId, string name);

        void UpdateCreatedTime(Guid workflowAssociationId, DateTime created);

        void UpdateModifiedTime(Guid workflowAssociationId, DateTime modified);
        void UpdateListModifiedTime(Guid listId, DateTime lastModified);

        void RecalculateRunningInstanceCount(Guid siteId, Guid webId, Guid listId, Guid workflowAssociationId);

        void RecalculateRunningInstanceCount(Guid siteId, Guid webId, Guid listId, int itemId, Guid workflowAssociationId);

        #endregion
        List<int> GetItemsByColumnValue(Guid siteId, Guid listId, string ColName, string ColValue);
        List<int> GetItemsByColumnValue(Guid siteId, Guid listId, Guid parentId, string ColName, string ColValue);

        void UpdateListAuthorByNative(Guid webId, Guid listId, int author);

        /// <summary>
        /// Update current uiversion of item to info.OriginalVersion by native
        /// </summary>
        /// <param name="info"></param>
        /// <param name="itemId">uniqueId of item</param>
        void UpdateUIVersionByNative(AveBaseItemInfo info, Guid itemId);


        void LoadAttachmentProperties(Guid UniqueId, byte Level, Guid ParentListID, int ID, Dictionary<string, object> propertyList);

        void SaveAttachmentProperties(Guid UniqueId, byte Level, Guid ParentListID, int ID, Dictionary<string, object> propertyList);

        List<string> GetAllListContentTypes(Guid webId);
        #endregion


        string GetUserLoginBySystemId(Guid siteId, byte[] systemId);

        bool IsItemHasAlerts(Guid siteId, Guid listId, int itemId);

        bool CheckIfHasAlertsOfSpecificConditions(Guid siteId, Guid listId, int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency);
    }
}
