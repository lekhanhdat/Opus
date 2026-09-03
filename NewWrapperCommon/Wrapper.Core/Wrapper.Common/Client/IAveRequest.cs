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
namespace AvePoint.Wrapper.Common
{

    using System;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;
    using System.Collections.ObjectModel;
    using Restore;

    public interface IAveRequest
    {
        /// <summary>
        /// 返回当前request的类型，所有类型记录在 AveClientRequestType 枚举中
        /// </summary>
        AveClientRequestType Type { get; }

        object Credentials { get; set; }
        string Url { get; }
        AveRequestKind Kind { get; }
        void Dispose(bool KeepRequest);

        #region used for OpenWebMethod

        void SetCurrentWebUrl(string currentWebUrl);
        Dictionary<string, object> OpenCurrentWeb();

        #endregion

        #region Get
        AveRequestAudit GetAuditValues();
        Dictionary<string, object> GetAdminCenterSite();
        Dictionary<string, object> GetSite();
        Dictionary<string, object> GetBrowserSiteInfo();

        Dictionary<string, object> GetWeb(Guid webId);
        Dictionary<string, object> GetWeb(string webServerRelativeUrl);      
        Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetSubWebs(string webServerRelativeUrl);
        Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource);
        Dictionary<string, object> GetLists(string webServerRelativeUrl);
        Dictionary<string, object> GetLists(Guid webId);
        Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode);
        Dictionary<string, object> GetItemsForRecords(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode);
        Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId);
        Dictionary<string, object> GetItemByGuid(Guid webId, Guid listId, Guid tp_Guid);
        Dictionary<string, object> GetItemByUniqueId(Guid webId, Guid listId, Guid itemId);
        Dictionary<string, object> GetItemByUrl(Guid webId, string itemUrl,out Guid listId);
        Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl);
        Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl);
        Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl);
        Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName);
        Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId);
        Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId);
        Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId);
        Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp);
        Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp);
        //Dictionary<string, object> GetTemplateByBaseID(Guid baseTemplateId);
        Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl);
        Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields);
        Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource);
        Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null);
        Dictionary<string, object> GetAllWebs();
        Dictionary<string, object> GetNavigation(string webServerRelativeUrl);
        Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource);
        Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp);
        Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource);
        Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource);
        /// <summary>
        /// 注意:此接口调用SP API获取User属性,由于API limitation,获取不到SID,IsDomainGroup和Notes这三个属性,调用时请注意。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Dictionary<string, object> GetUser(int id);
        Dictionary<string, object> GetUser(string userEmail);
        Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName);
        Dictionary<string, object> GetListTemplates(string webServerRelativeUrl);
        Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName);
        Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog);
        Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage);
        Dictionary<string, object> GetUserSolutions();
        Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl);
        Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource);
        Dictionary<string, object> GetAlerts(string webServerRelativeUrl);
        Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource);
        void ApplySiteDesign(string webUrl, Guid siteDesignId);
        Dictionary<string, object> GetSiteEventReceiverDefinitions(string siteServerRelativeUrl, string eventReceiverDefSource);
        Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null);
        Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties);
        IList<Dictionary<string, object>> GetManagedThemes();
        Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source);
        Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId);
        byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options);
        Dictionary<string, object> GetUserProfileByName(string accountName, bool isOnlineSite);
        Dictionary<string, object> GetUserProfileManager();
        Dictionary<string, object> GetAudienceManager();
        Guid GetListId(Guid webId, string listTitle);
        string GetApplicationPath(string serverRelativeUrl);
        Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId);
        Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff);
        Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount);
        Dictionary<string, object> GetTaxonomySession();
        Dictionary<string, object> GetTermStores();
        Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime);
        Dictionary<string, object> GetChanges(Guid termStoreId, TimeSpan sinceTimeAgo);
        Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType);
        Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType);
        Dictionary<string, object> GetTaxonomyGroups(Guid guid);
        Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId);
        bool IsTermSetExist(Guid termStoreId, Guid termSetId);
        Dictionary<string, object> GetTermSets(Guid termStoreId, Guid groupId);
        Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID);
        Dictionary<string, object> GetTerms(Guid termStoreId, Guid groupId, Guid termSetId, Guid parentTermId);
        Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable);
        Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId);
        Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId);
        Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId);
        bool IsTermExist(Guid termStoreId, Guid termId);
        Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId);
        Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl, bool createIfMissing);
        string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID);
        string GetDescription(Guid termStoreId, Guid termSetId, Guid parentTermId, int lcid);
        Dictionary<int, string> GetAllDescriptions(Guid termStoreId, Guid termSetId, Guid parentTermId, Collection<int> lcids);
        //Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle);
        Dictionary<string, object> GetSitePortal(string siteUrl);
        List<string> GetSiteEnabledHelpCollections();
        bool GetListRated(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle);
        List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId);
        Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId);
        List<Dictionary<string, object>> GetPublishedContentTypes();
        Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListEditViewSettingProperties(String webServerRelativeUrl, String listTitle, Guid listId, Guid viewId);
        Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId);
        List<Dictionary<string, object>> GetDisplayGroupsForSite();
        List<Dictionary<string, object>> GetKeyWords();
        Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl);
        Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl);
        Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource);
        bool DoesUserHavePermissions(string webServesrRelativeUrl, ulong permissionMask);
        Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl);
        Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid);
        Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel);
        Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl);
        Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl);
        Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl);
        Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId);
        bool GetSiteRssSetting();
        string GetWebTemplateConfiguration(string webRelativeUrl);
        Dictionary<string, object> GetListByTitle(Guid webId, string listTitle);
        Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl);//zyq
        Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string fieldSource, string listTitle, Guid listId);
        Dictionary<string, object> GetFeedFor(string postId, Dictionary<string, object> options);
        Dictionary<string, object> GetFullThread(string threadId);
        int GetListItemRatings(string listItemUrl);
        Dictionary<string, object> GetManagedSitecollectionData();
        Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, Dictionary<string, object> queryProps);
        Dictionary<string, object> GetSiteChangesByQuery(Dictionary<string, object> queryProps);
        Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, Dictionary<string, object> queryProps);
        #endregion

        #region Add
        Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment);
        Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource);
        Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource);
        Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere);
        Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties);
        Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, Guid featureId, int webTemplateType);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, string listName, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields, bool? listEnableMinorVersion);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType);
        Dictionary<string, object> AddFolder(string webServerRelativeUrl, Guid listId, string folderServerRelativeUrl, string strUrl);
        Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView);
        Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties);
        Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, int receiverType, string assembly, string className, string name);
        Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource);
        Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp);
        void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field);
        Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp);
        Dictionary<string, object> AddUserProfile(string accountName);
        void AddPersonalSite(string accountName, int lcid);
        void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId);

        Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType);
        string AddSynonm(string term, string synTerm, string terms);
        Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action);

        void AddTag(string url, Guid termId, string title, bool? isPrivate);
        void AddComment(string url, string comment, bool? isHighPriority, string title);

        bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "");
        string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota);
        #endregion

        #region workflow
        string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion);
        void BrowserEnableUserFormTemplate(string formTemplateUrl);
        Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostListId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid webId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostListId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        #endregion

        #region Delete
        void DeleteSite(string CAUrl, string url);
        void DeleteSiteToRecylebin(string CAUrl, string url); 
        void DeleteWeb(string webServerRelativeUrl);
        void DeleteList(string webServerRelativeUrl, string listName, Guid listId);
        void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl);
        void DeleteItem(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId);
        void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, int versionId);
        void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId);
        void DeleteFeature(string webServerRelativeUrl, Guid featureId, bool force, string featureSource);
        void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null);
        void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl);
        void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id);
        void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel);
        void DeleteGroup(string webServerRelativeUrl, int id);
        void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source);
        void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName);
        void DeleteAttachment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid webId, Guid listId, int rowId, string attachmentName);
        // use for 14.0.0.0, higher version use DeleteAttachment(Guid webId, Guid listId, int rowId, string attachmentName)
        //void DeleteAttachmentNow(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, int itemId, string leafName);
        void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName);
        void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId);
        void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl);
        void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId);
        void DeleteNavigationNode(string webServerRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> deleteNodeProperties);
        void DeleteField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp);
        //void DeleteUserSolution(Guid solutionId);
        void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName);
        void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles);
        void DeleteTag(string url, Guid termId);
        void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source);
        void DeleteAllWorkflowAasociations(string webUrl, Guid listId, string contentTypeId, string source);
        #endregion

        #region Update
        Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties);
        Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties);
        //Dictionary<string, object> UpdatePublishingWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties);
        Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties);
        Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, Dictionary<string, object> folderProperties);
        Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, Dictionary<string, object> viewProperties);
        Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties);
        Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties);
        Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties);
        Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource);
        Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties);
        Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties);
        Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties);
        Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties);
        Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop);
        Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties);
        Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties);
        Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp);
        Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties);
        Dictionary<string, object> UpdateTermStore(Guid guid, int termStoreDefaultLanguage, Dictionary<string, object> needUpdateProperties);
        void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listId, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties);
        void UpdateUserProfileDetails(string accountName, string xml);
        void UpdateUserProfileMemberships(string accountName, string xml);
        void UpdateUserProfileColleages(string accountName, string xml);
        void UpdateUserProfileTags(string accountName, string xml);
        void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);

        Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource);
        //Dictionary<string, object> BreakRoleDefinitionInheritance(string webServerRelativeUrl, bool copyRoleDefinitions, bool keepRoleAssignments);
        Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource);

        void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field, int index);
        void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);

        void Deny(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);

        Dictionary<string, object> CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType);
        Dictionary<string, object> CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl);
        void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite);
        void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags);
        void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file);
        void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file);
        void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl);
        void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);
        void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);
        void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName);
        void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp);
        void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp);
        void UpdateSpecialProperty(Dictionary<string, object> specialProp);
        void RevertAllDocumentContentStreams(string webServerRelativeUrl);
        void RevertContentStream(string webServerRelativeUrl, string fileUrl);
        void UpdateSiteRssSetting(bool syndicationEnabled);
        Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp);
        void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared);
        #endregion

        #region Restore
        void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null);
        void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl);
        void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, System.Collections.IList webpartBaseInfoList, AveWebPartCache mapping, bool clearAll, IAveWeb web, IReport report);

        Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Action<Guid, Guid, int, IDictionary<string, object>> AddItemMapping);
        Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData);
        Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream,IReport report);
        Dictionary<string, object> RestoreAttachment(Dictionary<string, object> data, Dictionary<string, object> userData, Stream fileStream);
        List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList);
        bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties);
        void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl);
        void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl);
        #endregion

        #region Recycle
        Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTitle, Guid listId, int itemId);
        Guid RecycleList(string webRelativeUrl, string listTitle, Guid listId);
        #endregion

        #region Discovery Query
        int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache);
        Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo);
        Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime);
        Dictionary<string, object> QueryRootWeb(Guid siteId);
        Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId);
        Dictionary<Guid, Dictionary<string, object>> GetSubWebsBasicInfo(string siteUrl, Guid parentWebId);
        Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache);
        Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);
        Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID);
        Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId);
        Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID);
        Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID);
        Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId);
        Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl);
        //Dictionary<int, object> GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);
        Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId);
        Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);
        bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName);
        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);
        Dictionary<string, object> DiscoverAllListContent(Guid siteId,Guid webId,Guid listId, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder);
        Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder);
        Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache);
        Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId);
        Dictionary<string, object> QueryWebRootFolder(Guid webId);

        Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        #endregion

        #region set

        void SetSiteEnabledHelpCollections(string[] enabledHelpCollections);
        bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating);
        void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties);
        void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp);
        Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI);
        Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter);
        Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type);
        Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value);
        Dictionary<string, object> CreatePost(string targetId, Dictionary<string, object> creationData);
        Dictionary<string, object> LikePost(string postId);
        //void SetListItemRatings(string listItemUrl, string itemTitle, int ratings, Guid siteId, Guid webId);
        //Dictionary<string, object> SetWebNavigationSettings(string webServerRelativeUrl, int globalSource, int currentSource, Dictionary<string,Guid> globalTaxonomy, Dictionary<string, Guid> currentTaxonomy);
        #endregion

        #region Common Browser
        //[Obsolete]
        //List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId);
        //[Obsolete]
        //AveWebBrowserInfo GetBrowserRootWeb();
        //[Obsolete]
        //List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);
        //[Obsolete]
        //AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId);
        //[Obsolete]
        //List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentFolderUniqueId, Guid parentListId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount);
        //[Obsolete]
        //List<AveItemBrowserInfo> GetBrowserItems(Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage);
        //[Obsolete]
        //List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);
        AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option);

        List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option);

        AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option);

        List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option);

        List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option);

        List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option);

        List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option);

        string GetServerVersion();

        #endregion

        #region WebPart
        Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);

        bool HaveAddAndCustomizePagesPermission { get; }
        #endregion

        Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite);

        Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip);

        string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle);

        Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data);

        Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int parentId, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties, bool isDiscussion);

        void CustomizeReport(Dictionary<string, object> parameters, Guid reportId);

        Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id);

        Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl);

        void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl);

        void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId);

        void ApplyWebTemplate(string webUrl, string webTemplate);

        string GetWebTemplateTitle(string siteUrl, uint language, string templateName);
        

        #region infoPath
        void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId);
        #endregion

        #region Only Online support

        bool DeleteMigrationJob(Guid id);

        AveMigrationJobState GetMigrationJobStatus(Guid id);

        Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri);

        Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options);

        void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl);

        Dictionary<string, object> GetFileById(string webServerRelativeUrl, Guid fileId);

        Dictionary<string, object> GetFolderById(string webServerRelativeUrl, Guid folderId);
        bool GetSiteExists(string url);

        AveWebMasterPageInfo GetRootWebMasterPageInfo();
        void SetRootWebAndMySiteWebMasterPageInfo(string mySiteWebServerRelativeUrl, AveWebMasterPageInfo pageInfo);
        #endregion

        #region workflow
        WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId);
        void RestoreWorkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache);
        void PostRestoreModernWebpart(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo sourceSiteInfo, Func<string, string> GetUserFromMapping);
        #endregion

        string GetListExperience(string webServerRelativeUrl, Guid guid);
        bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, string experience);
        void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
        void AddSitePolicy(string policySchema, string siteUrl);
        Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);
        Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinish);
        Dictionary<string, object> GetApps(string webServerRelativeUrl);
        Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId);
        Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo);
        Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl);
        Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId);
        Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId);
        Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition);
        void PublishDefinition(string webServerRelativeUrl, Guid definitionId);
        Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId);
        Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId);
        Dictionary<string, object> GetSiteStorageInfo();
        DateTime GetUTCToLocalTime(string webServerRelativeUrl, DateTime time);
        DateTime GetLocalToUTCTime(string webServerRelativeUrl, DateTime time);
        Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId);
        void AddDocumentsetVersion(string webRelativeUrl, string listTitle, int itemId, bool isMajor, string comment);
        /// <summary>
        /// Get all site collection under this Tenant.
        /// </summary>
        /// <param name="tenantAdminSiteUrl"></param>
        /// <param name="inlcudeOneDriveSite"></param>
        /// <param name="excludeTempaltes">Filter属性不支持根据template过滤，所以添加此参数控制</param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetAllSiteCollectionsList(string tenantAdminSiteUrl, bool inlcudeOneDriveSite, List<string> excludeTempaltes);
        List<Dictionary<string, object>> GetGroupSiteCollectionsList(string tenantAdminSiteUrl);
        List<Dictionary<string, object>> GetOneDriveSiteCollectionsList(string tenantAdminSiteUrl);
        List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl);
        Dictionary<string, object> GetWebAppById(string webServerRelativeUrl, Guid appId);
        Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly);
        Dictionary<string, object> GetFieldValueAsTaxonomyFieldValue(string webRelativeUrl, Guid listId, Guid fieldId, string text);
        int GetSiteOwnerId();
        Dictionary<string, object> GetSiteBasicProperties();
        List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames);
        SiteStatus GetSiteStatus(string siteUrl, Func<AveBPOSAccountInfo, string, string> GetAdminUrl);
        Dictionary<string, Dictionary<string, int>> GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, Guid listId, List<string> fieldNameList);
        void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId);

        Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listName, Guid parentListId);
        Guid PublishNintexWorkflow(string webUrl, Guid workflowDefinitionId);

        Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl);
        void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp);
        int GetSiteCollectionsCount(string tenantAdminSiteUrl);
        int GetOneDriveCount(List<string> usernames);
        void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota);

        string ImportNintexWorkflow(System.IO.Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId, bool migrate);

        AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers();

        AveProvisionedMigrationQueueInfo ProvisionMigrationQueue();

        void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId);

        void PublishNintexForm(string webUrl, Guid listId, string contentTypeId);
        Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId);

        string ConvertNintexFormJsonObjectToXml(string webUrl, string formJsonData, string fileName);

        Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIDs, string loginName);
        Dictionary<string, string> GetWebUserResource(string webServerRelativeUrl, string resourceName, List<string> cultureNames);
        Dictionary<string, string> GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, List<string> cultureNames);
        Dictionary<string, string> GetFieldUserResource(string webServerRelativeUrl, Guid listId, string resouceName, string fieldResourceName, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProp, List<string> cultureNames);
        Dictionary<string, string> GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string resouceName, string contentTypeResourceName, string contentTypeId, List<string> cultureNames);
        List<AveComplianceTagInfo> GetAvailableTagsForSite(string siteUrl);
        bool GetDenyAddAndCustomizePagesStatus();
        void SetDenyAddAndCustomizePagesStatus(bool status);
        AveComplianceTagInfo GetListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl);
        AveComplianceTagInfo UpdateListComplianceTagProperties(string webServerRelativeUrl, string listServerRelativeUrl, AveComplianceTagInfo properties);
        Dictionary<string, object> GetListItemComplianceTag(Guid webID, Guid listID, int rowID);
        Dictionary<string, object> SetComplianceTag(Guid webID, Guid listID, int rowID, AveItemComplianceTagInfo complianceSettingInfo);
        void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        #region pwa psi

        string ReadServerTimeLine();
        void UpdateTimeLineByPSI(string tlViewData);

        List<AveProjectDetailPageInfo> GetDetailPages(Guid eptId);
        void UpdateEnterpriseTypeByPSI(Guid projId, AveProjectEnterpriseProjectTypeInfo eptInfo);

        #endregion
        #region pwa backup

        bool TestProjectLicense();

        List<Dictionary<string, object>> QueryProjects(bool includeDetails);
        List<Dictionary<string, object>> QueryProjectCalendars();
        List<Dictionary<string, object>> QueryProjectCustomFields();
        List<Dictionary<string, object>> QueryProjectLookupTables();
        List<Dictionary<string, object>> QueryProjectEnterpriseProjectTypes();
        List<Dictionary<string, object>> QueryProjectEnterpriseResources();
        List<Dictionary<string, object>> QueryProjectPhases();
        List<Dictionary<string, object>> QueryProjectStages();
        List<Dictionary<string, object>> QueryProjectTasks(Guid projectId, bool isPublished);

        List<Dictionary<string, object>> QueryProjectDetailPages();
        Dictionary<string, object> QueryDraftProject(Guid projectId);

        Dictionary<string, object> GetProjectById(Guid id);
        //List<Dictionary<string, object>> QueryProjectTimeSheet();

        #endregion
        #region pwa restore
        void RestoreCalendar(List<AveProjectCalendarInfo> calendarInfos);

        void RestoreCustomFields(List<AveProjectCustomFieldInfo> customFieldInfos);

        void RestoreEnterpriseResource(List<AveProjectEnterpriseResourceInfo> resourceInfos);

        void RestoreLookupTable(List<AveProjectLookupTableInfo> lookupTableInfos);

        void RestorePhase(List<AveProjectPhaseInfo> phaseInfos);

        void RestoreStage(List<AveProjectStageInfo> stageInfos);

        //void RestoreTimeSheet(List<AveProjectTimeSheetInfo> timeSheetInfos);

        void RestoreEnterpriseProjectTypes(List<AveProjectEnterpriseProjectTypeInfo> eptInfos);

        Dictionary<string, object> RestoreProject(AveProjectInfo info, AveProjectReader projectDetails, AveProjectConfig projectConfig, AveRestoreMode option);

        #endregion
        #region add

        Dictionary<string, object> AddLookupTable(AveProjectLookupTableInfo lookupTableInfo);
        Dictionary<string, object> AddCustomField(AveProjectCustomFieldInfo customFieldInfo);
        Dictionary<string, object> AddEnterpriseType(AveProjectEnterpriseProjectTypeInfo eptInfo);
        Dictionary<string, object> AddEnterpriseResource(AveProjectEnterpriseResourceInfo resourceInfo);
        Dictionary<string, object> AddStage(AveProjectStageInfo stageInfo);
        Dictionary<string, object> AddPhase(AveProjectPhaseInfo phaseInfo);

        #endregion

        #region pwa delete

        void DeleteProject(Guid id, string siteUrl);

        #endregion

        #region pwa udpate

        Dictionary<string, object> UpdateLookupTable(Guid id, Dictionary<string, object> updateProp);
        Dictionary<string, object> UpdateCustomField(Guid id, Dictionary<string, object> updateProp);
        Dictionary<string, object> UpdateEnterpriseProjectType(Guid id, Dictionary<string, object> updateProp);
        Dictionary<string, object> UpdateEnterpriseResource(Guid id, Dictionary<string, object> updateProp);
        Dictionary<string, object> UpdateStage(Guid id, Dictionary<string, object> updateProp);
        Dictionary<string, object> UpdatePhase(Guid id, Dictionary<string, object> updateProp);

        #endregion

        #region records folder default value
        string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName);
        bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName);
        bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName, string value);
        
        #endregion

    }

}
