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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;
using System.IO;
using System.Globalization;
using AvePoint.Wrapper.Common.Office;
using Microsoft365.Authentication;
using System.Threading.Tasks;
using Util.MIP;
using Microsoft.SharePoint.Client;
using PnP.Framework.ALM;
using PnP.Framework.Enums;
using AvePoint.Wrapper.Common.Common.ObjectModel.Apps;

namespace AvePoint.Wrapper.Common
{
    public enum AveFieldSource
    {
        WebFields,
        WebAvaliableFields,
        ListFields,
        WebContentTypeFields,
        ListContentTypeFields
    }

    public enum AveContentTypeSource
    {
        WebContentTypes,
        WebAvaliableContentTypes,
        ListContentTypes
    }

    public interface IAveRequest
    {
        ITokenProvider TokenProvider { get; set; }
        AveBPOSAccountInfo BposInfo { get; }
        //object Credentials { get; set; }
        string Url { get; }
        string OriginalUrl { get; set; }
        AveRequestKind Kind { get; }
        void Dispose(bool KeepRequest);

        #region Get
        string GetAuthor(string webServerRelativeUrl);
        int GetAuditFlags();
        Dictionary<string, object> GetWebApplication();
        Dictionary<string, object> GetSite();
        bool GetSiteHasHolds();
        int GetSiteCompatibility();
        Dictionary<string, object> GetWeb(Guid webId);
        Dictionary<string, object> GetWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetSubWebs(string webServerRelativeUrl);
        Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource);
        Dictionary<string, object> GetLists(string webServerRelativeUrl, List<string> supportedResourceCultureNames);
        Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId);
        string GetListTitle(Guid siteId, Guid webId, Guid listId);
        Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode);
        Dictionary<string, object> GetItemsForRecords(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode, bool resetItemsIdCache = true);
        Dictionary<string, object> GetItemsLightly(string webServerRelativeUrl, string listName, Guid listId, string[] loadFieldInternalNames);
        Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId);
        Dictionary<string, object> GetItemsByUniqueIds(string webServerRelativeUrl, string listName, Guid listId, Guid[] uniqueIds);
        Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, string folderServerRelativeUrl);

        Dictionary<string, object> GetFolderFromCache(string webServerRelativeUrl, string listName, string folderServerRelativeUrl, Guid listId, int folderId);
        Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl);
        Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl);
        Dictionary<string, object> GetFileByPath(string webServerRelativeUrl, string filePath);
        Dictionary<string, object> GetFile(string webServerRelativeUrl, Guid id);
        Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName);
        Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, Guid listId, int itemId);
        Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId);
        Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId);
        Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp);
        Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp);
        //Dictionary<string, object> GetTemplateByBaseID(Guid baseTemplateId);
        Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl);
        Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo culture, Dictionary<string, string> needLoadFields, bool force);
        Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource);
        Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null);
        Dictionary<string, object> GetAllWebs();
        Dictionary<string, object> GetNavigation(string webServerRelativeUrl, bool isPublishFeatureEnable);
        Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, List<string> supportedResourceCultureNames);
        List<string> GetContentTypeResourceFiles(string webServerRelativeUrl, string serverRelativeUrl, Dictionary<string, List<string>> resourceFilesIndex);
        Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp, List<string> supportedResourceCultureNames);
        Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource);
        Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource);
        Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName);
        Dictionary<string, object> GetSiteGroupsWithUsers(string webRelativeUrl);
        Dictionary<string, object> GetListTemplates(string webServerRelativeUrl);
        Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName);
        Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog);
        Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage);
        Dictionary<string, object> GetUserSolutions();
        Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl);
        Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource);
        Dictionary<string, object> GetAlerts(string webServerRelativeUrl);
        Dictionary<string, object> GetAlertsV2(string webServerRelativeUrl);
        void DisableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds);
        void EnableAlert(string webServerRelativeUrl, List<Guid> disableAlertIds);
        Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, Guid listId, string listTitle, string eventReceiverDefSource);
        Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope);
        Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties);
        IList<Dictionary<string, object>> GetManagedThemes();
        Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source, Guid uniqueId, bool isSpecialList = false);
        Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId, Guid uniqueId);
        byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options, Guid uniqueId);
        //Dictionary<string, object> GetUserProfileByName(string accountName);
        Dictionary<string, object> GetUserProfileManager();
        Dictionary<string, object> GetAudienceManager();
        Guid GetListId(Guid webId, string listTitle);
        string GetApplicationPath(string serverRelativeUrl);
        Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId);
        Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff = true);
        Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount);
        Dictionary<string, object> GetTaxonomySession();
        Dictionary<string, object> GetTermStores();
        Dictionary<string, object> GetTaxonomyGroups(Guid guid);
        Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId);
        Dictionary<string, object> GetTermSets(Guid termStoreId, string groupName);
        Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID);
        Dictionary<string, object> GetTerms(Guid termStoreId, string groupName, string termSetName, Guid parentTermId);
        Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable);
        Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId);
        Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId);
        Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl);
        string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID);
        Dictionary<string, object> GetSitePortal(string siteUrl);
        List<string> GetSiteEnabledHelpCollections();
        Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle);
        List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, string listTitle, Guid listId);
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
        Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl);
        Dictionary<string, object> GetAllFeatureDefinitions(string url, int lcid, string featuresSource);
        bool DoesUserHavePermissions(string webServerRelativeUrl, int permissionMask);
        Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl);
        Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid);
        Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl);
        Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl);
        Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl);
        Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId);
        bool GetSiteRssSetting();
        string GetWebTemplateConfiguration(string webRelativeUrl);
        Dictionary<string, object> GetListByTitle(Guid webId, string rootFolderUrl);
        Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl);//zyq
        Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string source, string listTitle, Guid listId);
        Dictionary<string, object> GetPages(string webServerRelativeUrl, string listTitle, Guid listId);
        Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, IDictionary<string, object> queryProps);
        Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, string listTitle, IDictionary<string, object> queryProps);
        object GetClientContext();
        #endregion

        #region Add
        Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, int itemId, string leafName, byte[] attachment);
        Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource);
        Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource);
        Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties);
        Dictionary<string, object> AddItemUsingPath(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties);
        Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere);
        Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties);
        Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource);
        Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int itemId, int eventType, int frequency, bool isSendEmail);
        Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int eventType, int frequency, bool isSendEmail);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, int webTemplateType, string featureId = null);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields);
        Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType);
        Dictionary<string, object> AddFolder(string webServerRelativeUrl, string folderServerRelativeUrl, string strUrl);
        Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView);
        Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties);
        Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, Guid listId, string listTitle, string eventReceiverDefSource, int receiverType, string assembly, string className, string name);
        Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource);
        Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp);
        void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field);
        Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp);
        bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "");

        Dictionary<string, object> AddUserProfile(string accountName);
        void AddPersonalSite(string accountName, int lcid);
        void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId);

        Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType);
        string AddSynonm(string term, string synTerm, string terms);
        Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action);
        void AddSitePolicy(string policySchema, string siteUrl);

        Dictionary<string, object> GetManagedSitecollectionData();

        void UnlockSensitivityLabelEncryptedFile(string fileUrl, string justificationText);

        Dictionary<string, object> AddSite(int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota);

        #endregion

        #region
        string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion);
        void BrowserEnableUserFormTemplate(string formTemplateUrl);
        Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostListId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid webId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostListId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso);
        #endregion

        #region Delete
        void DeleteSite();
        void DeleteWeb(string webServerRelativeUrl);
        bool DeleteList(string webServerRelativeUrl, string listName, Guid listId, int baseTemplate, string entityTypeName, string templateFeatureId, bool recycle = false);
        void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl);
        void DeleteItem(string webServerRelativeUrl, string listUrl, string listTile, Guid listId, int itemId);
        void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTile, Guid listId, int itemId, int versionId);
        void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId);
        void DeleteFeature(string webServerRelativeUrl, Guid featureId, bool force, string featureSource);
        void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null);
        void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl);
        void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id);
        void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel);
        List<int> DeleteFileVersionSpecificNumber(string webServerRelativeUrl, string fileServerRelativeUrl, List<int> id);
        void RecycleFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id);
        void RecycleFileVersionByIdList(string webServerRelativeUrl, string fileServerRelativeUrl, List<int> ids);
        void DeleteGroup(string webServerRelativeUrl, int id);
        void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source);
        void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName);
        void DeleteAttachmentNow(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listid, int itemId, string leafName);
        void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName);
        void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listid, Guid viewId);
        void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl);
        void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listid, string eventReceiverDefSource, Guid eventReceiverDefId);
        void DeleteNavigationNode(string webServerRelativeUrl, IDictionary<string, object> parentNodeProperties, IDictionary<string, object> deleteNodeProperties);
        void DeleteField(string webServerRelativeUrl, string listName, Guid listid, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp);
        void DeleteUserSolution(Guid solutionId);
        void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName);
        void DeleteUsers(string webServerRelativeUrl, string source, string groupName, List<string> loginNames);
        void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles);
        bool DeleteContextType(string contentTypeId, string webServerRelativeUrl, Guid listId);

        #endregion

        #region Update
        Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties);
        Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties, bool isCustomScriptDisabled = false);
        Dictionary<string, object> UpdatePublishingWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties);
        Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties);
        Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, string folderServerRelativeUrl, Dictionary<string, object> folderProperties);
        Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listid, Guid viewId, Dictionary<string, object> viewProperties);
        Dictionary<string, object> UpdateAudit(int compatibilityLevel, Dictionary<string, object> needUpdateProperties);
        void SystemUpdateItemForRecords(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties, bool isFolder = false);
        void SystemUpdateForProps(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties);
        Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listid, int itemId, Dictionary<string, object> itemProperties);
        Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties);
        Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listid, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource);
        Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties);
        Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties);
        Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames);
        Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listid, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties);
        Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop);
        Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties, string fieldSchema);
        Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listid, string internalName, string fieldSource, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProperties);
        Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp);
        Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties);
        Dictionary<string, object> UpdatePropertyBag(string webServerRelativeUrl, string propertyBagSource, Guid alertId, Dictionary<string, object> needUpdateProperties);
        Dictionary<string, object> UpdateTermStore(Guid guid, Dictionary<string, object> dictionary);
        void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listid, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties);
        void UpdateUserProfileDetails(string accountName, string xml);
        void UpdateUserProfileMemberships(string accountName, string xml);
        void UpdateUserProfileColleages(string accountName, string xml);
        void UpdateUserProfileTags(string accountName, string xml);
        void UpdateMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties);

        Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listid, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource);
        Dictionary<string, object> BreakRoleDefinitionInheritance(string webServerRelativeUrl, bool copyRoleDefinitions, bool keepRoleAssignments);
        Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listid, int itemId, string roleAssignmentsSource);

        void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listid, Guid viewId, string field, int index);
        void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);
        void CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType);
        void CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl);
        void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite);
        void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags);
        void MoveToKeepEditor(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, string editor, DateTime modified, int flags);
        void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file);
        void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file);
        void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl);
        void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);
        void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment);
        void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName);
        void SetThemeUrlForWeb(string webServerRelativeUrl, string themeUrl);
        void ApplyTo(string webServerRelativeUrl, bool shareGenerated, string name);
        void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp);
        void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp);
        void UpdateSpecialProperty(Dictionary<string, object> specialProp);
        void RevertAllDocumentContentStreams(string webServerRelativeUrl);
        void RevertContentStream(string webServerRelativeUrl, string fileUrl);
        void UpdateSiteRssSetting(bool syndicationEnabled);
        Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp);
        void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared);
        void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);
        void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock, bool unlockedAsDefault);
        void SetComplianceTag(Guid webID, Guid listID, int rowID, string complianceTag, bool blockDel, bool blockEdit, DateTime complianceWrittenDate, string userEmail, bool isTagSuperLock);

        void LockRecordItem(string parentWebServerRelativeUrl, string listUrl, string itemId);
        void UnlockRecordItem(string parentWebServerRelativeUrl, string listUrl, string itemId);
        Task<List<AveAppMetadata>> GetAvailableAppsAsync(string webServerRelativeUrl, AppCatalogScope scope);
        Task<List<AveAppMetadata>> GetAvailableAppsByTitleAsync(string webServerRelativeUrl, AppCatalogScope scope, string title);
        Task<AveAppMetadata> GetAvailableAppByIdAsync(string webServerRelativeUrl, AppCatalogScope scope, Guid id);
        void UninstallApp(string webServerRelativeUrl, Guid productId);
        AveAppStatus GetAppStatus(string webServerRelativeUrl, Guid productId, out ClientObjectList<AppInstance> apps);
        Task<bool> InstallAppAsync(string webServerRelativeUrl, Guid id, AppCatalogScope scope = AppCatalogScope.Tenant);
        Task<bool> DeployAppAsync(string webServerRelativeUrl, Guid id, bool skipFeatureDeployment = true, AppCatalogScope scope = AppCatalogScope.Tenant);
        ListItemComplianceInfo GetListItemComplianceInfo(Guid webID, Guid listID, int rowID);

        ListItemComplianceInfo GetListItemComplianceInfo(ClientContext context, ListItem item);
        List<AveComplianceTagInfo> GetAvailableTagsForSite(string siteUrl);
        #endregion

        #region Restore
        void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null);
        void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl);
        void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, List<AveWebPartBaseInfo> webpartBaseInfoList, AveWebPartCache mapping, bool post);

        Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData);
        Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData, Dictionary<string, object> uniqueValues);
        Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData);
        Dictionary<string, object> RestoreDocument(AveDocumentInfo docInfo, Stream fileStream, DocumentRestoreInfo parentInfo);
        Dictionary<string, object> RestoreAttachment(string parentWebFullUrl, Dictionary<string, object> data, Stream fileStream);
        List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList);
        bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties, AveNavigationInfoList navigationList);
        bool RestoreSearchNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties);
        void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl);
        void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl);
        void RestoreSolutionStatus(string webServerRelativeUrl, IList<AveSolutionInfo> sandboxSolutions);
        #endregion

        #region Recycle
        Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTile, Guid listid, int itemId);
        Guid RecycleList(string webRelativeUrl, string listTitle, Guid listid);
        #endregion



        #region set

        void SetSiteEnabledHelpCollections(string[] enabledHelpCollections);
        //bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating);
        void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties);
        void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp);

        Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI);
        Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter);
        #endregion



        #region webpart

        void CloseWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId);
        void DeleteWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId);
        Dictionary<string, object> ImportAndAddWebPart(string webServerRelativeUrl, string fileServerRelativeUrl, string webPartXml, string zoneId, int zoneIndex);//GAO-3649

        #endregion

        #region records folder default value
        string GetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName);
        bool RemoveFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName);
        bool SetFieldDefault(string webServerRelativeUrl, string listName, Guid listid, string folderPath, string fieldName, string value);

        bool SetTaxonomyFieldValue(string webServerRelativeUrl, Guid listId, int itemId, string fieldName, string termId, string termName);
        #endregion

        Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite);

        Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo);

        string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle);

        void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId);

        void FolderMoveTo(string webServerRelativeUrl, string folderServerRelativeUrl, string desServerRelativeUrl);

        Dictionary<string, object> LoadSolution(int id);

        #region add from Ave2013Request
        List<IAveTenantMultiGeoLocationInfo> GetTenantGeoLocationinfo(string adminUrl = null);
        //string GetGroupPsResourceUrlByEmail(string email);
        int GetWebWorkingLanguage(string url);
        bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp);
        bool GetListRated(string webServerRelativeUrl, Guid listId);
        string GetListExperience(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetApps(string webServerRelativeUrl);
        Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId);
        Dictionary<string, object> GetAppInstanceById(string webServerRelativeUrl, Guid appInstanceId);
        Dictionary<string, object> LoadAndInstallApp(string webServerRelativeUrl, Stream stream);
        //string CustomizeReport(int compatibilityLevel, Dictionary<string, object> parameters);
        void SetAuditLogTrimming(int compatibilityLevel, Dictionary<string, object> parameters);
        Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo, List<AveAppMetadata> avaliableTenantApp, List<AveAppMetadata> avaliableSiteApp);
        List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl);
        int GetSiteCollectionsCount(string tenantAdminSiteUrl);
        bool GetDenyAddAndCustomizePagesStatus();
        bool AllowWebPropertyBagUpdateWhenDenyAddAndCustomizePagesIsEnabled();
        void SetDenyAddAndCustomizePagesStatus(bool enable);
        Dictionary<string, object> GetQuota();
        ItemIdMapping GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, string listTitle, Guid listId);
        Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl);
        Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId);
        Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId);
        Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly);
        Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition);
        void PublishDefinition(string webServerRelativeUrl, Guid definitionId);
        void PublishNintexWorkflow(string webUrl, string workflowId, string workflowRestrictToScope);
        void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId);
        void PublishNintexForm(string webUrl, Guid listId, string contentTypeId);
        Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId);
        Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId);
        Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId);
        Dictionary<string, object> GetSiteBasicProperties();
        List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames);
        int GetOneDriveCount(List<string> usernames);
        Dictionary<string, object> LoadMySiteInfo();
        Dictionary<string, Dictionary<string, object>> ResolvePrincipals(string webServerRelativeUrl, List<string> searchNames, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff = true);
        Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type);
        Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value);
        Ave2013NavigationInfo Get2013Navigation(string webServerRelativeUrl, bool isPublishFeatureEnable);
        IDictionary<string, object> TakeOverCheckOut(string webServerRelativeUrl, Guid listId, string fileServerRelativeUrl);
        void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId);
        IList<IDictionary<string, object>> GetListCheckOutFilesWithCSOM(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetItemsByIdSelectedFields(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode);
        Dictionary<string, object> GetItemById(Guid webId, Guid listId, int itemId);
        Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);
        void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
        /// <summary>
        /// apply web template
        /// first get the template, then apply it.
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="webTemplate"></param>
        /// <param name="lcid"></param>
        void ApplyWebTemplate(string webServerRelativeUrl, string webTemplate, uint lcid);
        /// <summary>
        /// apply web template directly with get the template information
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="webTemplateName"></param>
        void ApplyWebTemplate(string webServerRelativeUrl, string webTemplateName);
        Guid RecycleFolder(string webServerRelativeUrl, string folderServerRelativeUrl);
        Guid RecycleList(string webServerRelativeUrl, string listName, Guid listId, int baseTemplate, string entityTypeName, string templateFeatureId);
        void SetFormForList(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId);
        Dictionary<string, object> GetAdminCenterSite();
        IList<AveEventReceiver> RemoveAllEventReceivers(string webServerRelativeUrl, Guid listId);
        void AddEventReceivers(string webServerRelativeUrl, Guid listId, IList<AveEventReceiver> eventReceivers);
        void AddSupportedUILanguage(string webServerRelativeUrl, List<int> supportedUILanguageIds);
        Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId);
        void RestoreDocumentsetVersions(string webRelativeUrl, Guid listId, int itemId, System.Linq.IOrderedEnumerable<XmlElement> versions);
        void AddDocumentsetVersion(string webRelativeUrl, Guid listId, string listTitle, int itemId, bool isMajor, string comment);
        void DeleteSite(string siteUrl);
        void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source);
        void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota);
        AveStorageMetrics GetFolderStorageMetrics(string webServerRelativeUrl, string folderServerRelativeUrl);

        int GetSiteOwnerId();
        AveBasePermissions GetUserEffectivePermissions(string level, string Url, Guid id, string userName, int itemId = 0);
        Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId);
        Dictionary<string, object> GetAllTerms(Guid termStoreId, Guid termSetId);
        Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, bool isReadOnly, List<string> supportedResourceCultureNames);
       // Dictionary<string, object> OperateOnSolution(string operation, int id);
        void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId);
        Dictionary<string, object> GetTenant(bool includeProperties);
        Dictionary<string, object> GetTenantInstalledLanguages(long availableStorageQuota, double availableResourceQuota);
        Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl);
        string GetTenantAppCatalogSite(string webRelativeUrl);
        Dictionary<string, object> GetDeletedSitePropertiesByUrl(string siteUrl);
        SiteExistence SiteExistsAnywhere(string siteUrl);
        Dictionary<string, object> GetObjectSharingInformationByUrl(string objectUrl, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
        Dictionary<string, object> GetWebSharingInformation(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests);
        Dictionary<string, object> GetWebSharingInformation2(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
        Dictionary<string, object> GetWebSharingInformation3(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
        Dictionary<string, object> GetListSharingInformation(Guid listID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
        Dictionary<string, object> GetListSharingInformation2(Guid listID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
        Dictionary<string, object> GetListItemSharingInformation(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests);
        Dictionary<string, object> GetListItemSharingInformation2(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
        Dictionary<string, object> GetListItemSharingInformation3(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
        int CanCurrentUserShare(string docId);
        int CanCurrentUserShareRemote(string docId);
        string GetWebUserResource(string webServerRelativeUrl, string cultureName, string resourceName);
        void SetWebUserResource(string webServerRelativeUrl, string resourceName, Dictionary<string, string> changedTitle);
        Dictionary<Guid, string> GetListTitleResource(string webServerRelativeUrl, string cultureName);
        string GetListTitleResource(string webServerRelativeUrl, Guid id, string cultureName);
        void SetListTitleResource(string webServerRelativeUrl, Guid id, Dictionary<string, string> changedTitle);
        string GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, string cultureName);
        string GetFieldUserResource(string webServerRelativeUrl, Guid listId, string listName, string fieldSource, string resourceName, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProp, string cultureName);
        string GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string listName, string contentTypeSource, string resourceName, string contentTypeId, string cultureName);
        Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIds, string loginName);

        #region field

        void SetShowInDisplayForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);
        void SetShowInEditForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);
        void SetShowInNewForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);

        #endregion

        DateTime QueryLastAccessTime(Guid itemId, string folderServerRelativeUrl, DateTime modifiedtime, bool isCompatibleByModifiedTime = false);
        DateTime QueryLastAccessTime(string sitecollectionURL, DateTime? modifiedTime = null, bool isCompatibleByModifiedTime = false);


        void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp);

        /// <summary>
        /// 将对象share给指定用户
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="url"></param>
        /// <param name="peoplePickerInput"></param>
        /// <param name="roleValue"></param>
        /// <param name="groupId"></param>
        /// <param name="propagateAcl"></param>
        /// <param name="sendEmail"></param>
        /// <param name="includeAnonymousLinkInEmail"></param>
        /// <param name="emailSubject"></param>
        /// <param name="emailBody"></param>
        void ShareObject(string webUrl, string url, string peoplePickerInput, string roleValue, int groupId, bool propagateAcl, bool sendEmail, bool includeAnonymousLinkInEmail, string emailSubject, string emailBody, bool useSimplifiedRoles);

        /// <summary>
        /// Creates and returns an anonymous link that can be used to access a document without needing to authenticate.
        /// </summary>
        /// <param name="weUrl">Can not be null or empty</param>
        /// <param name="fileFullPath">The URL of the site, with the path of the object in SharePoint that is represented as query string parameters, forSharing set to 1 if sharing, and mbypass set to 1 to bypass any mobile logic.</param>
        /// <param name="isEditLink"> If true, the link will allow the guest user edit privileges on the item.</param>
        /// <param name="expirationString">
        /// A UTC date/time ticks represents the time and date of expiry for the anonymous link.
        /// Long default value indicates no expiry.
        /// </param>
        /// <returns></returns>
        string CreateAnonymousLinkWithExpiration(string webUrl, string fileFullPath, bool isEditLink, long expirationTicks);

        /// <summary>
        /// Creates and returns an organization-internal link that can be used to access a document and gain permissions to it.
        /// </summary>
        /// <param name="weUrl">Can not be null or empty</param>
        /// <param name="fileFullPath">The URL of the site, with the path of the object in SharePoint that is represented as query string parameters, forSharing set to 1 if sharing, and mbypass set to 1 to bypass any mobile logic.</param>
        /// <param name="isEditLink">If true, the link will allow the logged in user to edit privileges on the item.</param>
        /// <returns></returns>
        string CreateOrganizationSharingLink(string webUrl, string fileFullPath, bool isEditLink);

        void DeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId);// Archiver Record Manager
        void UndeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId);// Archiver Record Manager

        #region user customer action new
        Dictionary<string, object> UserCustomActionCollection_Add(AveUserCustomActionScope scope, string webUrl, Guid listId, string location);
        Dictionary<string, object> UserCustomAction_Update(AveUserCustomActionScope scope, string webUrl, Guid listId, Guid actionId, Dictionary<string, object> changeProperties);
        void UserCustomAction_Delete(AveUserCustomActionScope scope, string webUrl, Guid listId, Guid actionId);
        void UserCustomActionCollection_Clear(AveUserCustomActionScope scope, string webUrl, Guid listId);
        Dictionary<string, object> UserCustomActionCollection_Load(AveUserCustomActionScope scope, string webUrl, Guid listId);
        #endregion user customer action new 

        #region user custom action original
        [Obsolete]
        void DeleteUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId);
        [Obsolete]
        Dictionary<string, object> UpdateUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId, Dictionary<string, object> usercustomActionProperties);
        [Obsolete]
        Dictionary<string, object> AddUserCustomAction(string webServerRelativeUrl, string location);
        [Obsolete]
        Dictionary<string, object> GetUserCustomActions(string webServerRelativeUrl);
        [Obsolete]
        void UserCustomActionsClear(string webServerRelativeUrl);
        #endregion user custom action original
        //List<AvePropertyInfo> GetUserProfileSchema();

        void ShareLinkByRestApi(int linkKind, string loginName, bool isDomainGroup, string parentWebUrl, Guid listId, int itemId, string roleValue);


        void AddFileByRestApi(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite);
        void AddFileByRestApiWithContext(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite);
        void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields);

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

        #region tenant admin

        void RemoveSiteCollection(string siteUrl);

        void DeleteSiteCollectionImmediately(string siteUrl);

        void RemoveDeletedSiteCollection(string siteUrl);

        #endregion


        #region highspeed copy data

        bool DeleteMigrationJob(Guid id);

        AveMigrationJobState GetMigrationJobStatus(Guid id);

        MigrationJobProgress GetMigrationJobProgress(Guid id, string nextToken = "0");

        Dictionary<Guid, AveMigrationJobState> GetMigrationStatus();

        Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri);

        Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options);

        AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers();

        AveProvisionedMigrationQueueInfo ProvisionMigrationQueue();
        #endregion

        #region high speed cache
        Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadListItemIDUrlCache(string webServerRelativeUrl, Guid listId);
        #endregion

        List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl);
        List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib);

        #region publishing 
        void InstallDesignPackage(AveDesignPackageInfo info, string path);
        void UnInstallDesignPackage(AveDesignPackageInfo info);
        void ApplyDesignPackage(AveDesignPackageInfo info);
        AveDesignPackageInfo ExportEnterpriseDesignPackage(bool includeSearchConfiguration);
        AveDesignPackageInfo ExportSmallBusinessDesignPackage(string packageName, bool includeSearchConfiguration);
        #endregion

        //string GetTenantId();getu
        void UpdateTenantProperties(Dictionary<string, object> props);

        bool GetRequestAccessEnable(string webUrl);

        bool SetRequestAccessEnable(string webUrl, bool value);

        bool GetAccessRequestApprover(string webUrl);

        void SetAccessRequestApprover(string webUrl, bool value, string email);
        //Dictionary<int, List<int>> GetUniquePermissionItemsIDInEachFolder(string webUrl, Guid listId);

        #region compliance
        AveComplianceTagInfo GetListComplianceTag(string webUrl, string listUrl);
        void SetListComplianceTag(string webUrl, string listUrl, AveComplianceTagInfo info);

        void SetComplianceTagOnBulkItems(string webUrl,Guid webID, Guid listID, List<int> itemIds, string complianceTagValue);

        void SetComplianceTagOnBulkItems(ClientContext context, string listUrl, List<int> itemIds, string complianceTagValue);

        #endregion
        //Dictionary<int, KeyValuePair<int, List<int>>> GetFoldersIncludeUniquePermissionSubItemsOrFolders(string webUrl, Guid listId);
        Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinsh);//add for records
        #endregion

        #region add from browser
        #region Common Browser

        AveWebBrowserInfo GetBrowserRootWeb();

        List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);

        List<AveAppBrowserInfo> GetBrowserApps(Guid parentWebId);

        List<AveProjectBrowserInfo> GetBrowserProjects(int startIndex, uint perPage, ref int childrenCount);

        List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId);

        List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);
        /// <summary>
        /// Only get document library list for OneDrive
        /// </summary>
        /// <param name="parentWebId"></param>
        /// <param name="startIndex"></param>
        /// <param name="perPage"></param>
        /// <param name="childrenCount"></param>
        /// <returns></returns>
        List<AveListBrowserInfo> GetBrowserOneDriveLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount);


        AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId);

        List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentListId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, bool needLoadDesignFolders);

        List<AveItemBrowserInfo> GetBrowserItems(Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage);

        Dictionary<string, object> GetListsLightly(Guid webId);
        #endregion

        #region DPM browser

        List<AveSolutionBrowserInfo> GetBrowserSolutionInfos();

        List<AveAppBrowserInfo> GetBrowserAppsByProductId(Guid parentWebId, Guid productId);

        AveFolderBrowserInfo GetBrowserWebRootFolder(Guid parentWebId);

        List<AveHiddenFileInfo> GetBrowserFolderHiddenFiles(Guid parentWebId, Guid parentListId, string folderServerRelativeUrl);

        List<AveFieldBrowserInfo> GetBrowserFields(Guid webId, Guid listId, string fieldSource, out CultureInfo cultureInfo);

        List<AveContentTypeInfo> GetBrowserContentTypes(string webServerRelativeUrl, string listTitle, ContentTypeScope scope);

        List<AveWorkflowAssociationBrowserInfo> GetBrowserWorkflowAssociations(Guid webId, Guid listId, string contentTypeId, string workflowSource, out List<Guid> workflowTemplateIds);

        #endregion
        #endregion

        #region add from discover
        #region Discovery Query

        int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache);
        int GetListChangedForRecords(Guid webId, Guid listId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache);
        int GetListChangedCount(Guid webId, Guid listId, DateTime startTime, DateTime endTime);
        Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<string, object> GetListDeletedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);
        
        Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime);

        Dictionary<string, object> GetFolderAndSubFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime);

        Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo);

        Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime);

        Dictionary<string, object> QueryRootWeb(Guid siteId);

        Dictionary<string, object> QueryWeb(Guid webId);

        Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId);

        Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache);

        Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<string, object> changedCache, DateTime startTime, DateTime endTime);

        Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);

        Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID);

        Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId);

        Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID);

        Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID);

        Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId, bool throwException = false);

        Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl);

        Dictionary<int, object> GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);

        Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId);

        Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);

        bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName);

        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);

        Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool loadSubFolders, bool loadSubItems, bool includeSystemFolder);

        Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changCache);

        Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId);

        Dictionary<byte[], object> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId);

        Dictionary<string, object> QueryWebRootFolder(Guid webId);

        /// <summary>
        /// 根据AveQueryOption来check site从startTime到现在是否有变化
        /// </summary>
        /// <param name="siteUrl">需要check的site collection url</param>
        /// <param name="startTime">check的其实时间, 接受时间为当前时间, 使用的是Site.CurrentChangeToken属性</param>
        /// <param name="option">需要check哪些操作或者对象, 同ChangeQuery</param>
        /// <returns></returns>
        bool CheckSiteChanged(string siteUrl, long startTime, AveQueryOption option);

        void RemoveFolderCache(string folderServerRelativeUrl);

        void RemoveItemCache(int itemId);

        void ClearItemCache();

        void RemoveFolderCache(List<int> folderIds);

        Dictionary<string, object> QueryFolderForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool includeSystemFolder = false);

        Dictionary<string, object> QueryItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, ref string pageInfo, bool includeSystemFolder = false);

        Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);

        Dictionary<string, object> GetItemsByCamlQueryWithAttachments(string webServerRelativeUrl, Guid listId, string[] camlQueryNode);

        #endregion
        #endregion

        IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false);

        IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false);

        IEnumerable<Dictionary<string, object>> QueryItemWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion);

        IEnumerable<Dictionary<string, object>> QueryItemWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion);

        Dictionary<string, object> QueryListRootFolderWithStructureCache(Guid siteId, Guid webId, Guid mlistId);

        Dictionary<string, object> QueryListRootFolderForFullDiscover(Guid siteId, Guid webId, Guid mlistId);

        void DeclareItemsByRowIds(string webUrl, Guid listId, List<int> rowIds);
        void DeleteItemsByRowIds(string webUrl, Guid listId, Dictionary<int,long> rowIdsWithModifiedTime, Dictionary<int, long> rowIdsWithTimeLastModified);
        void DeleteItemsByRowIds(string webUrl, Guid listId, List<int> rowIds);
        void RestoreSharingLink(AveSharingLinkInfo shareLinkInfo, IEnumerable<IAvePrincipal> avePrincipals, string parentWebServerRelativeUrl, Guid listId, int itemId);
        AveDictionary<Guid, AveSharingLinkInfo> GetListItemSharingLinks(string parentWebUrl, Guid listId, int itemId);
        Task<LabelOperationResponse> RemoveSensitiveLabelAsync(FileInfo srcFile, FileInfo dstFile);
        void InitMIPService(string office365TenantId, string workingUser, Util.MIP.Cloud cloudLocation);
        bool CheckSiteIsLocked();
        void RemoveSiteLockedState();

        void DeleteSCTermGroup();
        bool ExistSCTermGroup();

        void UpdateSCTermGroupName(string name);

        Dictionary<string, AveListItemConflictBaseInfo> GetItemsForConflict(string webServerRelativeUrl, Guid siteId, Guid webId, string listName, Guid listId, string[] camlQueryNode);

        IEnumerable<List<AveListItemConflictBaseInfo>> GetItemsForConflictByBatch(string webServerRelativeUrl, Guid siteId, Guid webId, string listName, Guid listId, string[] camlQueryNode, int batchSize);

        Dictionary<string, (Guid UniqueId, Guid ListId)> GetStubNodesByBatchPath(List<string> serverRelativeUrls);
    }

    //public interface IAveRequestV1 : IAveRequest, IAveBrowserRequest, IAveDiscoverRequest
    //{
    //    string GetGroupPsResourceUrlByEmail(string email);
    //    int GetWebWorkingLanguage(string url);
    //    bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp);
    //    bool GetListRated(string webServerRelativeUrl, Guid listId);
    //    string GetListExperience(string webServerRelativeUrl, Guid listId);
    //    Dictionary<string, object> GetApps(string webServerRelativeUrl);
    //    Dictionary<string, object> GetAppsByProductId(string webServerRelativeUrl, Guid productId);
    //    Dictionary<string, object> GetAppInstanceById(string webServerRelativeUrl, Guid appInstanceId);
    //    Dictionary<string, object> LoadAndInstallApp(string webServerRelativeUrl, Stream stream);
    //    //string CustomizeReport(int compatibilityLevel, Dictionary<string, object> parameters);
    //    void SetAuditLogTrimming(int compatibilityLevel, Dictionary<string, object> parameters);
    //    Dictionary<string, object> RestoreApp(string webServerRelativeUrl, AveAppPackageInfo appInfo, Dictionary<string, object> restoreInfo);
    //    List<Dictionary<string, object>> GetManagedSiteCollectionsList(string tenantAdminSiteUrl);
    //    int GetSiteCollectionsCount(string tenantAdminSiteUrl);
    //    bool GetDenyAddAndCustomizePagesStatus();
    //    void SetDenyAddAndCustomizePagesStatus(bool enable);
    //    Dictionary<string, object> GetQuota();
    //    ItemIdMapping GetListItemGuidAndRowIdMappingsInLargeList(string webServerRelativeUrl, string rootFolderServerRelativeUrl, string listTitle, Guid listId);
    //    Dictionary<string, object> GetWorkflowServicesManager(string webServerRelativeUrl);
    //    Dictionary<string, object> EnumerateSubscriptionsByList(string webServerRelativeUrl, Guid listId);
    //    Dictionary<string, object> EnumerateSubscriptionsByEventSource(string webServerRelativeUrl, Guid webId);
    //    Dictionary<string, object> GetWorkflowDefinitionById(string webServerRelativeUrl, Guid definitionId);
    //    Dictionary<string, object> EnumWorkflowDefinition(string webServerRelativeUrl, bool publishedOnly);
    //    Guid SaveDefinition(string webServerRelativeUrl, IAveWorkflowDefinition definition);
    //    void PublishDefinition(string webServerRelativeUrl, Guid definitionId);
    //    void PublishNintexWorkflow(string webUrl, string workflowId, string workflowRestrictToScope);
    //    void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId);
    //    void PublishNintexForm(string webUrl, Guid listId, string contentTypeId);
    //    Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId);
    //    Guid PublishSubscription(string webServerRelativeUrl, IAveWorkflowSubscription subscription, Guid listId);
    //    Dictionary<string, object> GetSubscription(string webServerRelativeUrl, Guid subscriptionId);
    //    Dictionary<string, object> GetSiteBasicProperties();
    //    List<Dictionary<string, object>> LoadPersonalSiteInfosForUsers(List<string> usernames);
    //    int GetOneDriveCount(List<string> usernames);
    //    Dictionary<string, object> LoadMySiteInfo();
    //    Dictionary<string, Dictionary<string, object>> ResolvePrincipals(string webServerRelativeUrl, List<string> searchNames, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff = true);
    //    Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type);
    //    Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value);
    //    Ave2013NavigationInfo Get2013Navigation(string webServerRelativeUrl, bool isPublishFeatureEnable);
    //    IDictionary<string, object> TakeOverCheckOut(string webServerRelativeUrl, Guid listId, string fileServerRelativeUrl);
    //    void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId);
    //    IList<IDictionary<string, object>> GetListCheckOutFilesWithCSOM(string webServerRelativeUrl, Guid listId);
    //    Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
    //    Dictionary<string, object> GetItemsByIdSelectedFields(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode);
    //    Dictionary<string, object> GetItemById(Guid webId, Guid listId, int itemId);
    //    Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
    //    Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);
    //    void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
    //    /// <summary>
    //    /// apply web template
    //    /// first get the template, then apply it.
    //    /// </summary>
    //    /// <param name="webServerRelativeUrl"></param>
    //    /// <param name="webTemplate"></param>
    //    /// <param name="lcid"></param>
    //    void ApplyWebTemplate(string webServerRelativeUrl, string webTemplate, uint lcid);
    //    /// <summary>
    //    /// apply web template directly with get the template information
    //    /// </summary>
    //    /// <param name="webServerRelativeUrl"></param>
    //    /// <param name="webTemplateName"></param>
    //    void ApplyWebTemplate(string webServerRelativeUrl, string webTemplateName);
    //    Guid RecycleFolder(string webServerRelativeUrl, string folderServerRelativeUrl);
    //    Guid RecycleList(string webServerRelativeUrl, string listName, Guid listId, int baseTemplate, string entityTypeName, string templateFeatureId);
    //    void SetFormForList(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId);
    //    Dictionary<string, object> GetAdminCenterSite();
    //    IList<AveEventReceiver> RemoveAllEventReceivers(string webServerRelativeUrl, Guid listId);
    //    void AddEventReceivers(string webServerRelativeUrl, Guid listId, IList<AveEventReceiver> eventReceivers);
    //    void AddSupportedUILanguage(string webServerRelativeUrl, List<int> supportedUILanguageIds);
    //    Dictionary<string, object> AddDocumentSet(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, string name, IAveContentTypeId contentTypeId);
    //    void RestoreDocumentsetVersions(string webRelativeUrl, Guid listId, int itemId, System.Linq.IOrderedEnumerable<XmlElement> versions);
    //    void AddDocumentsetVersion(string webRelativeUrl, Guid listId, string listTitle, int itemId, bool isMajor, string comment);
    //    void DeleteSite(string siteUrl);
    //    void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source);
    //    void UpdateSiteUsage(string siteUrl, long storageQuota, double serverResourceQuota);
    //    AveStorageMetrics GetFolderStorageMetrics(string webServerRelativeUrl, string folderServerRelativeUrl);

    //    int GetSiteOwnerId();
    //    AveBasePermissions GetUserEffectivePermissions(string level, string Url, Guid id, string userName, int itemId = 0);
    //    Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId);
    //    Dictionary<string, object> GetAllTerms(Guid termStoreId, Guid termSetId);
    //    Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, bool isReadOnly, List<string> supportedResourceCultureNames);
    //    Dictionary<string, object> OperateOnSolution(string operation, int id);
    //    void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId);
    //    Dictionary<string, object> GetTenant(bool includeProperties);
    //    Dictionary<string, object> GetTenantInstalledLanguages(long availableStorageQuota, double availableResourceQuota);
    //    Dictionary<string, object> GetSitePropertiesByUrl(string siteUrl);
    //    Dictionary<string, object> GetDeletedSitePropertiesByUrl(string siteUrl);
    //    Dictionary<string, object> GetObjectSharingInformationByUrl(string objectUrl, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
    //    Dictionary<string, object> GetWebSharingInformation(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests);
    //    Dictionary<string, object> GetWebSharingInformation2(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
    //    Dictionary<string, object> GetWebSharingInformation3(bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
    //    Dictionary<string, object> GetListSharingInformation(Guid listID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
    //    Dictionary<string, object> GetListSharingInformation2(Guid listID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
    //    Dictionary<string, object> GetListItemSharingInformation(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests);
    //    Dictionary<string, object> GetListItemSharingInformation2(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels);
    //    Dictionary<string, object> GetListItemSharingInformation3(Guid listID, int itemID, bool excludeCurrentUser, bool excludeSiteAdmin, bool excludeSecurityGroups, bool retrieveAnonymousLinks, bool retrieveUserInfoDetails, bool checkForAccessRequests, bool retrievePermissionLevels, bool forceRetrievePermissionLevels);
    //    int CanCurrentUserShare(string docId);
    //    int CanCurrentUserShareRemote(string docId);
    //    string GetWebUserResource(string webServerRelativeUrl, string cultureName, string resourceName);
    //    void SetWebUserResource(string webServerRelativeUrl, string resourceName, Dictionary<string, string> changedTitle);
    //    Dictionary<Guid, string> GetListTitleResource(string webServerRelativeUrl, string cultureName);
    //    string GetListTitleResource(string webServerRelativeUrl, Guid id, string cultureName);
    //    void SetListTitleResource(string webServerRelativeUrl, Guid id, Dictionary<string, string> changedTitle);
    //    string GetListUserResource(string webServerRelativeUrl, Guid id, string resourceName, string cultureName);
    //    string GetFieldUserResource(string webServerRelativeUrl, Guid listId, string listName, string fieldSource, string resourceName, IDictionary<string, object> contentTypeProp, IDictionary<string, object> fieldProp, string cultureName);
    //    string GetContentTypeUserResource(string webServerRelativeUrl, Guid listId, string listName, string contentTypeSource, string resourceName, string contentTypeId, string cultureName);
    //    Dictionary<string, object> CreatePersonalSiteEnqueueBulk(string[] emailIds, string loginName);

    //    #region field

    //    void SetShowInDisplayForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);
    //    void SetShowInEditForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);
    //    void SetShowInNewForm(string webServerRelativeUrl, AveFieldSource source, Guid listId, IDictionary<string, object> contentTypeProps, Guid fieldId, bool value);

    //    #endregion

    //    DateTime QueryLastAccessTime(Guid itemId, string folderServerRelativeUrl, DateTime modifiedtime);
    //    DateTime QueryLastAccessTime(string sitecollectionURL);


    //    void UpdateSiteBasicPropertiesByUrl(string siteUrl, Dictionary<string, object> siteProp);

    //    /// <summary>
    //    /// 将对象share给指定用户
    //    /// </summary>
    //    /// <param name="webUrl"></param>
    //    /// <param name="url"></param>
    //    /// <param name="peoplePickerInput"></param>
    //    /// <param name="roleValue"></param>
    //    /// <param name="groupId"></param>
    //    /// <param name="propagateAcl"></param>
    //    /// <param name="sendEmail"></param>
    //    /// <param name="includeAnonymousLinkInEmail"></param>
    //    /// <param name="emailSubject"></param>
    //    /// <param name="emailBody"></param>
    //    void ShareObject(string webUrl, string url, string peoplePickerInput, string roleValue, int groupId, bool propagateAcl, bool sendEmail, bool includeAnonymousLinkInEmail, string emailSubject, string emailBody, bool useSimplifiedRoles);

    //    /// <summary>
    //    /// Creates and returns an anonymous link that can be used to access a document without needing to authenticate.
    //    /// </summary>
    //    /// <param name="weUrl">Can not be null or empty</param>
    //    /// <param name="fileFullPath">The URL of the site, with the path of the object in SharePoint that is represented as query string parameters, forSharing set to 1 if sharing, and mbypass set to 1 to bypass any mobile logic.</param>
    //    /// <param name="isEditLink"> If true, the link will allow the guest user edit privileges on the item.</param>
    //    /// <param name="expirationString">
    //    /// A UTC date/time ticks represents the time and date of expiry for the anonymous link.
    //    /// Long default value indicates no expiry.
    //    /// </param>
    //    /// <returns></returns>
    //    string CreateAnonymousLinkWithExpiration(string webUrl, string fileFullPath, bool isEditLink, long expirationTicks);

    //    /// <summary>
    //    /// Creates and returns an organization-internal link that can be used to access a document and gain permissions to it.
    //    /// </summary>
    //    /// <param name="weUrl">Can not be null or empty</param>
    //    /// <param name="fileFullPath">The URL of the site, with the path of the object in SharePoint that is represented as query string parameters, forSharing set to 1 if sharing, and mbypass set to 1 to bypass any mobile logic.</param>
    //    /// <param name="isEditLink">If true, the link will allow the logged in user to edit privileges on the item.</param>
    //    /// <returns></returns>
    //    string CreateOrganizationSharingLink(string webUrl, string fileFullPath, bool isEditLink);

    //    void DeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId);// Archiver Record Manager
    //    void UndeclareItemAsRecord(string webServerRelativeUrl, Guid listId, int itemId);// Archiver Record Manager

    //    #region user customer action new
    //    Dictionary<string, object> UserCustomActionCollection_Add(AveUserCustomActionScope scope, string webUrl, Guid listId, string location);
    //    Dictionary<string, object> UserCustomAction_Update(AveUserCustomActionScope scope, string webUrl, Guid listId, Guid actionId, Dictionary<string, object> changeProperties);
    //    void UserCustomAction_Delete(AveUserCustomActionScope scope, string webUrl, Guid listId, Guid actionId);
    //    void UserCustomActionCollection_Clear(AveUserCustomActionScope scope, string webUrl, Guid listId);
    //    Dictionary<string, object> UserCustomActionCollection_Load(AveUserCustomActionScope scope, string webUrl, Guid listId);
    //    #endregion user customer action new 

    //    #region user custom action original
    //    [Obsolete]
    //    void DeleteUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId);
    //    [Obsolete]
    //    Dictionary<string, object> UpdateUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId, Dictionary<string, object> usercustomActionProperties);
    //    [Obsolete]
    //    Dictionary<string, object> AddUserCustomAction(string webServerRelativeUrl, string location);
    //    [Obsolete]
    //    Dictionary<string, object> GetUserCustomActions(string webServerRelativeUrl);
    //    [Obsolete]
    //    void UserCustomActionsClear(string webServerRelativeUrl);
    //    #endregion user custom action original
    //    List<AvePropertyInfo> GetUserProfileSchema();

    //    void ShareLinkByRestApi(int linkKind, string loginName, bool isDomainGroup, string parentWebUrl, Guid listId, int itemId, string roleValue);


    //    void AddFileByRestApi(string parentWebUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite);
    //    void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields);

    //    #region pwa psi

    //    string ReadServerTimeLine();
    //    void UpdateTimeLineByPSI(string tlViewData);

    //    List<AveProjectDetailPageInfo> GetDetailPages(Guid eptId);
    //    void UpdateEnterpriseTypeByPSI(Guid projId, AveProjectEnterpriseProjectTypeInfo eptInfo);

    //    #endregion

    //    #region pwa backup

    //    bool TestProjectLicense();

    //    List<Dictionary<string, object>> QueryProjects(bool includeDetails);
    //    List<Dictionary<string, object>> QueryProjectCalendars();
    //    List<Dictionary<string, object>> QueryProjectCustomFields();
    //    List<Dictionary<string, object>> QueryProjectLookupTables();
    //    List<Dictionary<string, object>> QueryProjectEnterpriseProjectTypes();
    //    List<Dictionary<string, object>> QueryProjectEnterpriseResources();
    //    List<Dictionary<string, object>> QueryProjectPhases();
    //    List<Dictionary<string, object>> QueryProjectStages();
    //    List<Dictionary<string, object>> QueryProjectTasks(Guid projectId, bool isPublished);

    //    List<Dictionary<string, object>> QueryProjectDetailPages();
    //    Dictionary<string, object> QueryDraftProject(Guid projectId);

    //    Dictionary<string, object> GetProjectById(Guid id);
    //    //List<Dictionary<string, object>> QueryProjectTimeSheet();

    //    #endregion

    //    #region pwa restore
    //    void RestoreCalendar(List<AveProjectCalendarInfo> calendarInfos);

    //    void RestoreCustomFields(List<AveProjectCustomFieldInfo> customFieldInfos);

    //    void RestoreEnterpriseResource(List<AveProjectEnterpriseResourceInfo> resourceInfos);

    //    void RestoreLookupTable(List<AveProjectLookupTableInfo> lookupTableInfos);

    //    void RestorePhase(List<AveProjectPhaseInfo> phaseInfos);

    //    void RestoreStage(List<AveProjectStageInfo> stageInfos);

    //    //void RestoreTimeSheet(List<AveProjectTimeSheetInfo> timeSheetInfos);

    //    void RestoreEnterpriseProjectTypes(List<AveProjectEnterpriseProjectTypeInfo> eptInfos);

    //    Dictionary<string, object> RestoreProject(AveProjectInfo info, AveProjectReader projectDetails, AveProjectConfig projectConfig, AveRestoreMode option);

    //    #endregion

    //    #region add

    //    Dictionary<string, object> AddLookupTable(AveProjectLookupTableInfo lookupTableInfo);
    //    Dictionary<string, object> AddCustomField(AveProjectCustomFieldInfo customFieldInfo);
    //    Dictionary<string, object> AddEnterpriseType(AveProjectEnterpriseProjectTypeInfo eptInfo);
    //    Dictionary<string, object> AddEnterpriseResource(AveProjectEnterpriseResourceInfo resourceInfo);
    //    Dictionary<string, object> AddStage(AveProjectStageInfo stageInfo);
    //    Dictionary<string, object> AddPhase(AveProjectPhaseInfo phaseInfo);

    //    #endregion

    //    #region pwa delete

    //    void DeleteProject(Guid id, string siteUrl);

    //    #endregion

    //    #region pwa udpate

    //    Dictionary<string, object> UpdateLookupTable(Guid id, Dictionary<string, object> updateProp);
    //    Dictionary<string, object> UpdateCustomField(Guid id, Dictionary<string, object> updateProp);
    //    Dictionary<string, object> UpdateEnterpriseProjectType(Guid id, Dictionary<string, object> updateProp);
    //    Dictionary<string, object> UpdateEnterpriseResource(Guid id, Dictionary<string, object> updateProp);
    //    Dictionary<string, object> UpdateStage(Guid id, Dictionary<string, object> updateProp);
    //    Dictionary<string, object> UpdatePhase(Guid id, Dictionary<string, object> updateProp);

    //    #endregion

    //    #region tenant admin

    //    void RemoveSiteCollection(string siteUrl);

    //    void DeleteSiteCollectionImmediately(string siteUrl);

    //    void RemoveDeletedSiteCollection(string siteUrl);

    //    #endregion


    //    #region highspeed copy data

    //    bool DeleteMigrationJob(Guid id);

    //    AveMigrationJobState GetMigrationJobStatus(Guid id);

    //    Dictionary<Guid, AveMigrationJobState> GetMigrationStatus();

    //    Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri);

    //    Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options);

    //    AveProvisionedMigrationContainersInfo ProvisionMigraitonContainers();

    //    AveProvisionedMigrationQueueInfo ProvisionMigrationQueue();
    //    #endregion

    //    #region high speed cache
    //    Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadListItemIDUrlCache(string webServerRelativeUrl, Guid listId);
    //    #endregion

    //    List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl);
    //    List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib);

    //    #region publishing 
    //    void InstallDesignPackage(AveDesignPackageInfo info, string path);
    //    void UnInstallDesignPackage(AveDesignPackageInfo info);
    //    void ApplyDesignPackage(AveDesignPackageInfo info);
    //    AveDesignPackageInfo ExportEnterpriseDesignPackage(bool includeSearchConfiguration);
    //    AveDesignPackageInfo ExportSmallBusinessDesignPackage(string packageName, bool includeSearchConfiguration);
    //    #endregion

    //    string GetTenantId();
    //    void UpdateTenantProperties(Dictionary<string, object> props);

    //    bool GetRequestAccessEnable(string webUrl);

    //    bool SetRequestAccessEnable(string webUrl, bool value);

    //    bool GetAccessRequestApprover(string webUrl);

    //    void SetAccessRequestApprover(string webUrl, bool value, string email);
    //    Dictionary<int, List<int>> GetUniquePermissionItemsIDInEachFolder(string webUrl, Guid listId);

    //    #region compliance
    //    AveComplianceTagInfo GetListComplianceTag(string webUrl, string listUrl);
    //    void SetListComplianceTag(string webUrl, string listUrl, AveComplianceTagInfo info);

    //    #endregion
    //    Dictionary<int, KeyValuePair<int, List<int>>> GetFoldersIncludeUniquePermissionSubItemsOrFolders(string webUrl, Guid listId);
    //    Guid UninstallAppByInstanceId(Guid webId, Guid instanceId, Guid productId, bool waitUninstallFinsh);//add for records
    //}

    

    public class ItemIdMapping
    {
        public bool HasAttachment { get; set; }

        public Dictionary<string, int> IdMapping { get; set; }

        public Dictionary<string, int> AppendItemMapping { get; set; } // key: ApppendGUID, value: highest append item Id
    }

    public class AveEventReceiver
    {
        public AveEventReceiverType EventType
        {
            get;
            set;
        }

        public string ReceiverAssembly
        {
            get;
            set;
        }

        public string ReceiverClass
        {
            get;
            set;
        }

        public Guid ReceiverId
        {
            get;
            set;
        }

        public string ReceiverName
        {
            get;
            set;
        }

        public string ReceiverUrl
        {
            get;
            set;
        }

        public int SequenceNumber
        {
            get;
            set;
        }

        public AveEventReceiverSynchronization Synchronization
        {
            get;
            set;
        }

    }

    public enum AveEventReceiverSynchronization
    {
        DefaultSynchronization,
        Synchronous,
        Asynchronous
    }
}
