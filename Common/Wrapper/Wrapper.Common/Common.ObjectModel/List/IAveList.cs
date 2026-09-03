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
using System.Collections.ObjectModel;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public interface IAveList : IAveSecurableObject
    {
        int Version { get; }
        IAveAlertTemplate AlertTemplate { get; set; }
        bool AllowDeletion { get; set; }//
        bool AllowRssFeeds { get; }
        bool AllowMultiResponses { get; set; }
        bool AllowContentTypes { get; }
        IAveUser Author { get; }//
        IAveAudit Audit { get; }
        AveBasePermissions AnonymousPermMask64 { get; set; }
        AveListTemplateType BaseTemplate { get; }
        AveBaseType BaseType { get; }
        DateTime Created { get; }
        IAveContentTypeCollection ContentTypes { get; }
        bool ContentTypesEnabled { get; set; }
        bool CrawlNonDefaultViews { get; set; }
        IAveListDataSource DataSource { get; }
        Guid DefaultContentApprovalWorkflowId { get; set; }
        string DefaultDisplayFormUrl { get; set; }
        string DefaultEditFormUrl { get; set; }
        AveDefaultItemOpen DefaultItemOpen { get; set; }
        bool DefaultItemOpenUseListSetting { get; set; }
        string DefaultNewFormUrl { get; set; }
        string DefaultViewUrl { get; }
        IAveView DefaultView { get; }
        string Description { get; set; }
        IAveUserResource DescriptionResource { get; }
        AveDraftVisibilityType DraftVersionVisibility { get; set; }
        string Direction { get; set; }
        bool DisableGridEditing { get; set; }
        string EmailAlias { get; set; }
        bool EnableAssignToEmail { get; set; }
        bool EnableAttachments { get; set; }
        bool EnforceDataValidation { get; set; }
        bool EnableDeployingList { get; set; }
        bool EnableDeployWithDependentList { get; set; }
        bool EnableFolderCreation { get; set; }
        bool EnableManagedIndexes { get; set; }
        AveListExperience ListExperience { get; set; }
        bool EnableMinorVersions { get; set; }
        bool EnableModeration { get; set; }
        bool EnablePeopleSelector { get; set; }
        bool EnableResourceSelector { get; set; }
        bool EnableSchemaCaching { get; set; }
        bool EnableSyndication { get; set; }
        bool EnableThrottling { get; set; }
        bool EnableVersioning { get; set; }
        bool ExcludeFromOfflineClient { get; set; }
        bool ExcludeFromTemplate { get; }
        IAveEventReceiverDefinitionCollection EventReceivers { get; }
        string EventSinkAssembly { get; set; }
        string EventSinkClass { get; set; }
        string EventSinkData { get; set; }
        Exception Exception { get; }
        IAveFieldIndexCollection FieldIndexes { get; }
        IAveFieldCollection Fields { get; }
        IAveListItemCollection Folders { get; }
        void ClearFieldsCache();
        bool ForceCheckout { get; set; }
        string GetPropertiesXmlForUncustomizedViews();
        bool HasExternalDataSource { get; }
        bool Hidden { get; set; }
        string ImageUrl { get; }
        bool IsApplicationList { get; set; }
        bool IsCatalog { get; }
        bool IsSiteAssetsLibrary { get; set; }
        bool IsThrottled { get; }
        bool IrmEnabled { get; set; }
        bool IrmExpire { get; set; }
        bool IrmReject { get; set; }
        IAveListItemCollection Items { get; }
        int ItemCount { get; }
        DateTime LastItemDeletedDate { get; }
        DateTime LastItemModifiedDate { get; }
        DateTime LastItemUserModifiedDate { get; }
        int MajorWithMinorVersionsLimit { get; set; }
        int MajorVersionLimit { get; set; }
        bool MultipleDataList { get; set; }
        bool NavigateForFormsPages { get; set; }
        bool NoCrawl { get; set; }
        bool OnQuickLaunch { get; set; }
        bool Ordered { get; set; }
        IAveWeb ParentWeb { get; }
        string ParentWebUrl { get; }
        int ReadSecurity { get; set; }
        IAveFolder RootFolder { get; }
        bool RootWebOnly { get; set; }
        string SchemaXml { get; }
        string SendToLocationName { get; set; }
        string SendToLocationUrl { get; set; }
        bool ServerTemplateCanCreateFolders { get; }
        bool ShowUser { get; set; }
        IAveAlertTemplate SmsAlertTemplate { get; set; }
        Guid TemplateFeatureId { get; }
        string Title { get; set; }
        IAveUserResource TitleResource { get; }
        string ValidationFormula { get; set; }
        string ValidationMessage { get; set; }
        IAveViewCollection Views { get; }
        int WriteSecurity { get; set; }
        IAveWorkflowAssociationCollection WorkflowAssociations { get; }
        IAveFormCollection Forms { get; }
        IAveListCollection Lists { get; }
        Guid ID { get; }
        ulong Flags { get; }
        bool RequestAccessEnabled { get; set; }
        Dictionary<string, int> ListItemGuidAndRowIdMappings { get; }  //SAAS-11351
        Dictionary<string, int> ListAppendItemMappings { get; } // RECO-34442

        IAveUserCustomActionCollection UserCustomActions { get; }


        void Reload();
        void ReloadListWorkflowAssociations();
        IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType);
        IAveListItem AddItem(string fileServerRelativeUrl, Stream body, bool isOverwrite);
        IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName);
        IAveListItem AddItem(AveItemCreationInformation itemCreationInfo);
        IAveListItem AddItemUsingPath(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName);
        void CleanListData();
        void Delete();
        void EnsureRssSettings();
        IAveListItem GetItemById(int id);
        IAveListItem GetItemById(string id);
        IAveListItemCollection GetItems(AveCamlQuery camlQuery);
        IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery, bool resetItemIdCache = true);
        IAveListItemCollection GetItems(IAveQuery query);
        IAveView GetView(Guid viewGuid);
        void Update();
        Guid Recycle();
        IAveListItem GetItemByUniqueId(Guid uniqueId);
        IAveListItemCollection GetItemsByUniqueIds(Guid[] uniqueIds);
        IAveListItem GetFileByPath(string filePath);
        AveListInfo GetListInfo();
        string GetListViewSchema(Guid siteId, Guid listId);
        bool IsSchedulingEventOnList();
        IAveListItem AddItem();
        AveListSettingInfo GetListSettings();
        void GetViews(ref Dictionary<string, List<AveViewInfo>> viewCache);
        void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache);
        IAveFolder GetFolder(string serverRelativeUrl);
        IAveListItemCollection GetPages();

        AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData);
        IAveRelatedFieldCollection GetRelatedFields();
        Dictionary<Guid, Guid> GetAlerts(string url, int itemId, AveSPAlertHostType hostType);
        /// <summary>
        /// For Web Database System List
        /// </summary>
        /// <returns></returns>
        bool IsACCSRVSystemList();
        IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association);
        void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation);
        void SetWorkflowsAssociated(bool bWorkflowsAssociated);
        IAveListItem GetItemByIdSelectedFields(int id, params string[] fields);
        void UpdateListRssSetting(Dictionary<string, object> updateProp);
        Collection<IAveSPListItemInfo> GetItemsWithUniquePermissions();
        List<int> GetItemsByColumnValue(string columnDisplayName, string value);
        bool CheckItemIsExist(int rowId);
        bool CheckItemIsExist(string rowId, Guid itemId);
        void UpdateListCreated(DateTime created);
        bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency);
        void RestoreSolutionStatus(IList<AveSolutionInfo> sandboxSolutions); 
        /// <summary>
        /// 设置List Audience Targetting,由于该setting关闭后可能会影响list下数据，关闭需慎重
        /// </summary>
        /// <param name="enableSettings">true:开启 false：关闭</param>
        void SetAudienceTargetting(bool enableSettings);
        /// <summary>
        ///  设置List rattign setting,由于该setting关闭后可能会影响list下数据，关闭需慎重
        /// </summary>
        /// <param name="enableSettings">true:开启  false:关闭</param>
        /// <param name="ratingExperience">"Likes" ro "Ratings"</param>
        void SetRatingSettings(bool enableSettings, AveRatingSettingType ratingExperience);
        bool? IsConnectorList { get; set; }
        IAveInformationRightsManagementSettings InformationRightsManagementSettings { get; }
        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);

        /// <summary>
        /// Get root folder directly without cache the root folder object.
        /// </summary>
        /// <returns></returns>
        IAveFolder GetRootFolder();
        void ReorderListFields(List<string> mappedSourceFields);

        Dictionary<string, object> ConvertFieldValuesToStringForHS(Dictionary<string, object> fieldValues, Dictionary<string, object> multipleLookupFieldValues);
        Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadExistingItemIdUrlMapping();
        void SaveNintexForm(string formXml, string contentTypeId);
        void PublishNintexForm(string contentTypeId);
        Stream ExportNintexForm(string contentTypeId);
        /// <summary>
        /// Microsoft limitation, just return the maximum 100 items
        /// </summary>
        /// <returns></returns>
		List<int> GetItemsIdWithUniquePermissions();
        //Dictionary<int, List<int>> GetUniquePermissionItemsIDInEachFolder();

        String GetViewSpotlightItemsMapping();

        AveComplianceTagInfo GetListComplianceTag();
        void SetListComplianceTag(AveComplianceTagInfo info);
        //Dictionary<int, KeyValuePair<int, List<int>>> GetFoldersIncludeUniquePermissionSubItemsOrFolders();
        IAveListItemCollection GetItemsLightly(params string[] loadFieldInternalNames);
        void DeclareItemsByRowIds(List<int> rowIds);
        void DeleteItemsByRowIds(Dictionary<int,long> rowIdsWithModifiedTime, Dictionary<int, long> rowIdsWithTimeLastModified);

        /// <summary>
        /// use to delete stub after restore, no need check modified time
        /// </summary>
        /// <param name="rowIds"></param>
        void DeleteItemsByRowIds(List<int> rowIds);

        void SetComplianceTagOnBulkItems(List<int> itemIds, string complianceTagValue);

        Dictionary<string, AveListItemConflictBaseInfo> FileCollection { get; }
        Dictionary<string, AveListItemConflictBaseInfo> FoldersCollection { get; }
        Dictionary<Guid, AveListItemConflictBaseInfo> UniqueIDMapping { get; }

        //void SetAppendItemTitleUsed(string tpGuid);
        bool TryGetCachedListItem(string fileRelativeUrl, out AveListItemConflictBaseInfo fileInfo);

        void InitSqliteCacheInfo(string jobId, int aveListSqliteCacheTypes);
    }
}
