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
using Microsoft365.Authentication;

namespace AvePoint.Wrapper.Common
{
    public interface IAveHttpWebRequestCommon
    {
        #region Get

        Dictionary<string, object> GetAllFeatureDefinitions(string url, int lcid, string featuresSource);
        void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, ITokenProvider tokenProvider);
        Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle);
        Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid);
        List<Dictionary<string, object>> GetKeyWords();
        List<string> GetSiteEnabledHelpCollections();
        List<Dictionary<string, object>> GetPublishedContentTypes();
        Dictionary<string, object> GetSitePortal(string siteUrl);
        bool GetSiteRssSetting();
        Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj);
        Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId);
        List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId,int localedId,bool isTime24);
        Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId);
        bool GetListRated(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId);
        string GetListExperience(string webServerRelativeUrl, Guid listId);
        Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp);
        void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, long availableStorageQuota, double availableResourceQuota);
        int GetAuditFlags();

        Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl);
        Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl);

        List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl);

        List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib);

        bool GetRequestAccessEnable(string webUrl);

        bool GetAccessRequestApprover(string webUrl);
        #endregion

        #region Update
        void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties);
        void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties);
        void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp);
        Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp);
        Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdmins, List<IDictionary<string, object>> newAdmins);
        Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> sitePortalProperties);
        void UpdateSiteRssSetting(bool syndicationEnabled);
        void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp);
        void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp);
        void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties);
        void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields);
        void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties);
        void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName);
        bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties, AveNavigationInfoList navigationList);
        bool RestoreSearchNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties);
        void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties);
        bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp);
        void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp);
        void SetSiteEnabledHelpCollections(string[] enabledHelpCollections);
        List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList, object context, object web);
        void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl);
        void OperateOnVersion(string webServerRelativeUrl, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, int versionId, string listId, string fileName, string op);
        Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties);
        void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties);
        //void RefreshCredentials(object credentials);
        void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId);
        void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId);
        bool SetRequestAccessEnable(string webUrl, bool value);
        void SetAccessRequestApprover(string webUrl, bool value, string email);
        #endregion
        #region Add

        Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action);
        Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType);
        string AddSynonm(string term, string synTerm, string terms);
        void AddSitePolicy(string policySchema, string siteUrl);
        Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data);
        void CustomizeReport(Dictionary<string, object> parameters);
        void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties);
        Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate);
        #endregion

        #region Archiver Record Manager
        void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl);
        #endregion
    }
}
