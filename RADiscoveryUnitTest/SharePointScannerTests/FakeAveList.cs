using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using AvePoint.Wrapper.Common;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// Minimal fake implementation of IAveList for testing scanner rule evaluation.
    /// Only properties accessed by IsSystemList and FilterAnalyser.GetListFilterInfo are implemented.
    /// All other members throw NotImplementedException.
    /// </summary>
    public class FakeAveList : IAveList
    {
        // Missing interface members
        public void DeleteItemsByRowIds(Dictionary<int, long> rowIdsWithModifiedTime, Dictionary<int, long> rowIdsWithTimeLastModified) { }
        public void DeleteItemsByRowIds(List<int> rowIds) { }
        public void SetComplianceTagOnBulkItems(List<int> itemIds, string complianceTagValue) { }
        public Dictionary<string, AveListItemConflictBaseInfo> FileCollection { get; } = new Dictionary<string, AveListItemConflictBaseInfo>();
        public Dictionary<string, AveListItemConflictBaseInfo> FoldersCollection { get; } = new Dictionary<string, AveListItemConflictBaseInfo>();
        public Dictionary<Guid, AveListItemConflictBaseInfo> UniqueIDMapping { get; } = new Dictionary<Guid, AveListItemConflictBaseInfo>();

        // Properties used by IsSystemList check
        public AveListTemplateType BaseTemplate { get; set; } = AveListTemplateType.DocumentLibrary;
        public bool Hidden { get; set; } = false;
        public string Title { get; set; } = "TestList";
        public bool AllowDeletion { get; set; } = true;

        // Properties used by FilterAnalyser.GetListFilterInfo
        public AveBaseType BaseType { get; set; } = AveBaseType.DocumentLibrary;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastItemModifiedDate { get; set; } = DateTime.UtcNow;
        public int ItemCount { get; set; } = 10;
        public Guid ID { get; set; } = Guid.NewGuid();

        // ParentWeb - needed by GetListFilterInfo for URL and timezone
        public IAveWeb ParentWeb { get; set; }

        // RootFolder - needed for URL
        public IAveFolder RootFolder { get; set; }

        // Author - needed for CreatedByRule
        public IAveUser Author { get; set; }

        // Fields - needed for ColumnsRule
        public IAveFieldCollection Fields { get; set; }

        // ContentTypes - needed for ContentTypeCollectionRule
        public IAveContentTypeCollection ContentTypes { get; set; }

        #region Not implemented members

        public int Version => 0;
        public IAveAlertTemplate AlertTemplate { get => null; set { } }
        public bool AllowRssFeeds => false;
        public bool AllowMultiResponses { get => false; set { } }
        public bool AllowContentTypes => false;
        public IAveAudit Audit => null;
        public AveBasePermissions AnonymousPermMask64 { get => default; set { } }
        public bool ContentTypesEnabled { get => false; set { } }
        public bool CrawlNonDefaultViews { get => false; set { } }
        public IAveListDataSource DataSource => null;
        public Guid DefaultContentApprovalWorkflowId { get => Guid.Empty; set { } }
        public string DefaultDisplayFormUrl { get => null; set { } }
        public string DefaultEditFormUrl { get => null; set { } }
        public AveDefaultItemOpen DefaultItemOpen { get => default; set { } }
        public bool DefaultItemOpenUseListSetting { get => false; set { } }
        public string DefaultNewFormUrl { get => null; set { } }
        public string DefaultViewUrl => null;
        public IAveView DefaultView => null;
        public string Description { get => null; set { } }
        public IAveUserResource DescriptionResource => null;
        public AveDraftVisibilityType DraftVersionVisibility { get => default; set { } }
        public string Direction { get => null; set { } }
        public bool DisableGridEditing { get => false; set { } }
        public string EmailAlias { get => null; set { } }
        public bool EnableAssignToEmail { get => false; set { } }
        public bool EnableAttachments { get => false; set { } }
        public bool EnforceDataValidation { get => false; set { } }
        public bool EnableDeployingList { get => false; set { } }
        public bool EnableDeployWithDependentList { get => false; set { } }
        public bool EnableFolderCreation { get => false; set { } }
        public bool EnableManagedIndexes { get => false; set { } }
        public AveListExperience ListExperience { get => default; set { } }
        public bool EnableMinorVersions { get => false; set { } }
        public bool EnableModeration { get => false; set { } }
        public bool EnablePeopleSelector { get => false; set { } }
        public bool EnableResourceSelector { get => false; set { } }
        public bool EnableSchemaCaching { get => false; set { } }
        public bool EnableSyndication { get => false; set { } }
        public bool EnableThrottling { get => false; set { } }
        public bool EnableVersioning { get => false; set { } }
        public bool ExcludeFromOfflineClient { get => false; set { } }
        public bool ExcludeFromTemplate => false;
        public IAveEventReceiverDefinitionCollection EventReceivers => null;
        public string EventSinkAssembly { get => null; set { } }
        public string EventSinkClass { get => null; set { } }
        public string EventSinkData { get => null; set { } }
        public Exception Exception => null;
        public IAveFieldIndexCollection FieldIndexes => null;
        public IAveListItemCollection Folders => null;
        public bool ForceCheckout { get => false; set { } }
        public bool HasExternalDataSource => false;
        public string ImageUrl => null;
        public bool IsApplicationList { get => false; set { } }
        public bool IsCatalog => false;
        public bool IsSiteAssetsLibrary { get => false; set { } }
        public bool IsThrottled => false;
        public bool IrmEnabled { get => false; set { } }
        public bool IrmExpire { get => false; set { } }
        public bool IrmReject { get => false; set { } }
        public IAveListItemCollection Items => null;
        public DateTime LastItemDeletedDate => DateTime.MinValue;
        public DateTime LastItemUserModifiedDate => DateTime.MinValue;
        public int MajorWithMinorVersionsLimit { get => 0; set { } }
        public int MajorVersionLimit { get => 0; set { } }
        public bool MultipleDataList { get => false; set { } }
        public bool NavigateForFormsPages { get => false; set { } }
        public bool NoCrawl { get => false; set { } }
        public bool OnQuickLaunch { get => false; set { } }
        public bool Ordered { get => false; set { } }
        public string ParentWebUrl => null;
        public int ReadSecurity { get => 0; set { } }
        public bool RootWebOnly { get => false; set { } }
        public string SchemaXml => null;
        public string SendToLocationName { get => null; set { } }
        public string SendToLocationUrl { get => null; set { } }
        public bool ServerTemplateCanCreateFolders => false;
        public bool ShowUser { get => false; set { } }
        public IAveAlertTemplate SmsAlertTemplate { get => null; set { } }
        public Guid TemplateFeatureId => Guid.Empty;
        public IAveUserResource TitleResource => null;
        public string ValidationFormula { get => null; set { } }
        public string ValidationMessage { get => null; set { } }
        public IAveViewCollection Views => null;
        public int WriteSecurity { get => 0; set { } }
        public IAveWorkflowAssociationCollection WorkflowAssociations => null;
        public IAveFormCollection Forms => null;
        public IAveListCollection Lists => null;
        public ulong Flags => 0;
        public bool RequestAccessEnabled { get => false; set { } }
        public Dictionary<string, int> ListItemGuidAndRowIdMappings => null;
        public Dictionary<string, int> ListAppendItemMappings => null;
        public IAveUserCustomActionCollection UserCustomActions => null;
        public bool? IsConnectorList { get => false; set { } }
        public IAveInformationRightsManagementSettings InformationRightsManagementSettings => null;

        // IAveSecurableObject
        public bool HasUniqueRoleAssignments => false;
        public IAveRoleAssignmentCollection RoleAssignments => null;
        public IAveSecurableObjectImpl SecurableObjectImpl => null;

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes) { }
        public void BreakRoleInheritance(bool copyRoleAssignments) { }
        public bool DoesUserHavePermissions(AveBasePermissions permissionMask) => true;
        public void ResetRoleInheritance() { }
        public IAvePermissionInfo GetUserEffectivePermissionInfo(string userName) => null;
        public AveBasePermissions GetUserEffectivePermissions(string userName) => default;

        // IAveList methods - not needed for rule checking
        public void ClearFieldsCache() { }
        public string GetPropertiesXmlForUncustomizedViews() => null;
        public void Reload() { }
        public void ReloadListWorkflowAssociations() { }
        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType) => throw new NotImplementedException();
        public IAveListItem AddItem(string fileServerRelativeUrl, Stream body, bool isOverwrite) => throw new NotImplementedException();
        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName) => throw new NotImplementedException();
        public IAveListItem AddItem(AveItemCreationInformation itemCreationInfo) => throw new NotImplementedException();
        public IAveListItem AddItemUsingPath(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName) => throw new NotImplementedException();
        public void CleanListData() { }
        public void Delete() { }
        public void EnsureRssSettings() { }
        public IAveListItem GetItemById(int id) => null;
        public IAveListItem GetItemById(string id) => null;
        public IAveListItemCollection GetItems(AveCamlQuery camlQuery) => null;
        public IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery, bool resetItemIdCache = true) => null;
        public IAveListItemCollection GetItems(IAveQuery query) => null;
        public IAveView GetView(Guid viewGuid) => null;
        public void Update() { }
        public Guid Recycle() => Guid.Empty;
        public IAveListItem GetItemByUniqueId(Guid uniqueId) => null;
        public IAveListItemCollection GetItemsByUniqueIds(Guid[] uniqueIds) => null;
        public IAveListItem GetFileByPath(string filePath) => null;
        public AveListInfo GetListInfo() => null;
        public string GetListViewSchema(Guid siteId, Guid listId) => null;
        public bool IsSchedulingEventOnList() => false;
        public IAveListItem AddItem() => throw new NotImplementedException();
        public AveListSettingInfo GetListSettings() => null;
        public void GetViews(ref Dictionary<string, List<AveViewInfo>> viewCache) { }
        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache) { }
        public IAveFolder GetFolder(string serverRelativeUrl) => null;
        public IAveListItemCollection GetPages() => null;
        public AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData) => default;
        public IAveRelatedFieldCollection GetRelatedFields() => null;
        public Dictionary<Guid, Guid> GetAlerts(string url, int itemId, AveSPAlertHostType hostType) => null;
        public bool IsACCSRVSystemList() => false;
        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association) => null;
        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation) { }
        public void SetWorkflowsAssociated(bool bWorkflowsAssociated) { }
        public IAveListItem GetItemByIdSelectedFields(int id, params string[] fields) => null;
        public void UpdateListRssSetting(Dictionary<string, object> updateProp) { }
        public Collection<IAveSPListItemInfo> GetItemsWithUniquePermissions() => null;
        public List<int> GetItemsByColumnValue(string columnDisplayName, string value) => null;
        public bool CheckItemIsExist(int rowId) => false;
        public bool CheckItemIsExist(string rowId, Guid itemId) => false;
        public void UpdateListCreated(DateTime created) { }
        public bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency) => false;
        public void RestoreSolutionStatus(IList<AveSolutionInfo> sandboxSolutions) { }
        public void SetAudienceTargetting(bool enableSettings) { }
        public void SetRatingSettings(bool enableSettings, AveRatingSettingType ratingExperience) { }
        public IAveChangeCollection GetChanges() => null;
        public IAveChangeCollection GetChanges(IAveChangeQuery query) => null;
        public IAveChangeCollection GetChanges(IAveChangeToken changeToken) => null;
        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd) => null;
        public IAveFolder GetRootFolder() => RootFolder;
        public void ReorderListFields(List<string> mappedSourceFields) { }
        public Dictionary<string, object> ConvertFieldValuesToStringForHS(Dictionary<string, object> fieldValues, Dictionary<string, object> multipleLookupFieldValues) => null;
        public Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadExistingItemIdUrlMapping() => null;
        public void SaveNintexForm(string formXml, string contentTypeId) { }
        public void PublishNintexForm(string contentTypeId) { }
        public Stream ExportNintexForm(string contentTypeId) => null;
        public List<int> GetItemsIdWithUniquePermissions() => null;
        public string GetViewSpotlightItemsMapping() => null;
        public AveComplianceTagInfo GetListComplianceTag() => null;
        public void SetListComplianceTag(AveComplianceTagInfo info) { }
        public IAveListItemCollection GetItemsLightly(params string[] loadFieldInternalNames) => null;
        public void DeclareItemsByRowIds(List<int> rowIds) { }
        public bool IsRelationshipsList() => false;
        public void RestoreListRatingSetting(AveListSettingInfo info) { }

        public bool TryGetCachedListItem(string fileRelativeUrl, out AveListItemConflictBaseInfo fileInfo) => throw new NotImplementedException();
        public void InitSqliteCacheInfo(string jobId, int aveListSqliteCacheTypes) { }
        #endregion
    }
}
