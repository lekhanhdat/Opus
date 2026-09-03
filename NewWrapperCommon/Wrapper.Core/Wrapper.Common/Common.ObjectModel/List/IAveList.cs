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
        bool AllowEveryoneViewItems { get; set; }
        bool AllowRssFeeds { get; }
        bool AllowMultiResponses { get; set; }
        bool AllowContentTypes { get; }
        IAveUser Author { get; }//
        IAveAudit Audit { get; }
        AveBasePermissions AnonymousPermMask64 { get; set; }
        AveListTemplateType BaseTemplate { get; }
        AveBaseType BaseType { get; }
        AveBrowserFileHandling BrowserFileHandling { get; set; }
        AveCalculationOptions CalculationOptions { get; set; }
        bool CanReceiveEmail { get; }
        DateTime Created { get; }
        IAveContentTypeCollection ContentTypes { get; }
        bool ContentTypesEnabled { get; set; }
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
        AveDraftVisibilityType DraftVersionVisibility { get; set; }
        string Direction { get; set; }
        bool DisableGridEditing { get; set; }
        AveBasePermissions EffectiveBasePermissions { get; }
        AveBasePermissions EffectiveFolderPermissions { get; }
        string EmailAlias { get; set; }
        bool EnableAssignToEmail { get; set; }
        bool EnableAttachments { get; set; }
        bool EnforceDataValidation { get; set; }
        bool EnableDeployingList { get; set; }
        bool EnableDeployWithDependentList { get; set; }
        bool EnableFolderCreation { get; set; }
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
        AveComplianceTagInfo ComplianceTag { get; set; }
        string EventSinkAssembly { get; set; }
        string EventSinkClass { get; set; }
        string EventSinkData { get; set; }
        IAveFieldIndexCollection FieldIndexes { get; }
        IAveFieldCollection Fields { get; }
        IAveListItemCollection Folders { get; }
        bool ForceCheckout { get; set; }
        bool ForceDefaultContentType { get; set; }
        string GetPropertiesXmlForUncustomizedViews();
        bool HasExternalDataSource { get; }
        bool Hidden { get; set; }
		//On-premise 07和10 不支持set 方法
        string ImageUrl { get; set; }
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
        AveListExperience ListExperienceOptions { get; set; }
        int MajorWithMinorVersionsLimit { get; set; }
        int MajorVersionLimit { get; set; }
        string MobileDefaultDisplayFormUrl { get; }
        string MobileDefaultEditFormUrl { get; }
        string MobileDefaultNewFormUrl { get; }
        IAveView MobileDefaultView { get; }
        string MobileDefaultViewUrl { get; }
        bool MultipleDataList { get; set; }
        bool NavigateForFormsPages { get; set; }
        bool EnableManagedIndexes { get; set; }
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
        bool UseFormsForDisplay { get; set; }
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
        bool RestrictedTemplateList { get; }
        Dictionary<string, Dictionary<string, string>> ClientLocationBasedDefaults { get; }

        void Reload();
        void ReloadFields();
        IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType);
        IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName);
        IAveListItem AddItem(AveItemCreationInformation itemCreationInfo);
        void CleanListData();
        void Delete();
        void EnsureRssSettings();
        IAveListItem GetItemById(int id);
        IAveListItem GetItemById(string id);
        IAveListItemCollection GetItems(AveCamlQuery camlQuery);
        IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery);
        IAveListItemCollection GetItems(IAveQuery query);
        IAveView GetView(Guid viewGuid);
        void Update();
        Guid Recycle();
        //The items count is larger than 5000, it has problem for o365
        IAveListItem GetItemByUniqueId(Guid uniqueId);
        //Implement for Client, Server 13,10, not for 07
        IAveListItem GetItemByGuid(Guid tp_Guid);
        AveListInfo GetListInfo();
        string GetListViewSchema(Guid siteId, Guid listId);
        bool IsSchedulingEventOnList();
        bool IsRelationshipsList();
        IAveListItem AddItem();
        AveListSettingInfo GetListSettings();
        void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache);
        IAveFolder GetFolder(string serverRelativeUrl);

        AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData);
        IAveRelatedFieldCollection GetRelatedFields();
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
        bool CheckItemIsExist(string rowId, Guid itemId, string parentFolderServerRelativeUrl = null);

        [Obsolete("Use UpdateListModifyInfo instead")]
        void UpdateListCreated(DateTime created);

        void UpdateListModifyInfo(Dictionary<string, object> modifyInfoDictionary);

        bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency);

        bool? IsConnectorList { get; set; }

        bool IsOneDriveLibrary { get; }

        void RestoreListRatingSetting(AveListSettingInfo info);
        IAveUserCustomActionCollection UserCustomActions { get; }
        #region add for SP2013
        int SearchVersion { get; set; }
        IAveInformationRightsManagementSettings InformationRightsManagementSettings { get; }
        #endregion

        //To operate Change Log
        IAveChangeCollection GetChanges();
        IAveChangeCollection GetChanges(IAveChangeQuery query);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken);
        IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd);

        #region User Resource
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource TitleResource { get; }
        /// <summary>
        /// only support server 10,13 mode. server 07, client mode will return null.
        /// </summary>
        IAveUserResource DescriptionResource { get; }
        #endregion

        bool IsExceedListViewLookupThreshold { get; }
        bool CrawlNonDefaultViews { get; set; }

        #region Sharepoint InfoPath list
        void PublicSharepointInfoPathList(IAveFile templateFile, int lcid, string listId, string contentTypeId);
        #endregion
        void SaveNintexForm(string formXml, string contentTypeId);
        void PublishNintexForm(string contentTypeId);
        Stream ExportNintexForm(string contentTypeId);

        WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId);
        void RestoreWOrkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache);
    }
}
