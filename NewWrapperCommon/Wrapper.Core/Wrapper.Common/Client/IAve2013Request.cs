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
    using System.Collections.Generic;
    using System.IO;

    public interface IAve2013Request : IAveRequest
    {
        string GetListExperience(string webServerRelativeUrl, Guid guid);
        bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, string experience);
        void ApplyTheme(string webServerRelativeUrl, string colorPaletteUrl, string fontSchemeUrl, string backgroundImageUrl, bool shareGenerated);
        void AddSitePolicy(string policySchema, string siteUrl);
        Dictionary<string, object> GetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> ResetListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> UpdateListInformationRightsManagementSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);
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

        bool GetDenyAddAndCustomizePagesStatus();
        AveComplianceTagInfo GetListComplianceTagProperties(string listServerRelativeUrl);
        AveComplianceTagInfo UpdateListComplianceTagProperties(string listServerRelativeUrl, AveComplianceTagInfo properties);
        Dictionary<string, object> GetListItemComplianceTag(Guid webID, Guid listID, int rowID);
        Dictionary<string, object> SetComplianceTag(Guid webID, Guid listID, int rowID, AveItemComplianceTagInfo complianceSettingInfo);
    }
}
