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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveHttpWebRequestCommonEmpty : IAveHttpWebRequestCommon
    {
        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            return null;
        }

        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return null;
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return null;
        }

        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            return null;
        }

        public void AddSitePolicy(string policySchema, string siteUrl)
        {
        }

        public string AddSynonm(string term, string synTerm, string terms)
        {
            return null;
        }

        public void CustomizeReport(Dictionary<string, object> parameters)
        {
        }

        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
        }

        public Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            return null;
        }

        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return null;
        }

        public List<Dictionary<string, object>> GetInstalledLanguages(string webServerRelativeUrl)
        {
            return null;
        }

        public List<Dictionary<string, object>> GetKeyWords()
        {
            return null;
        }

        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj)
        {
            return null;
        }

        public List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId)
        {
            return new List<Dictionary<string, object>>();
        }

        public string GetListExperience(string webServerRelativeUrl, Guid listId)
        {
            return null;
        }

        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            return null;
        }

        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return false;
        }

        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return null;
        }

        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            return null;
        }

        public void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, long availableStorageQuota, double availableResourceQuota)
        {
        }

        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            return null;
        }

        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return null;
        }

        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return null;
        }

        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return null;
        }

        public AveRequestAudit GetRequestAudit()
        {
            return null;
        }

        public List<string> GetSiteEnabledHelpCollections()
        {
            return null;
        }

        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            return null;
        }

        public bool GetSiteRssSetting()
        {
            return false;
        }

        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl)
        {
            return null;
        }

        public Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            return null;
        }

        public void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, object obj)
        {
        }

        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            return null;
        }

        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
        }

        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
        }

        public void MoveTo(string webServerRelativeUrl, string oldUrl, string newUrl)
        {
        }

        public void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
        }

        public Guid PublishNintexWorkflow(Stream stream, string publishName, string tenant, string siteServerRelativeUrl, string listName, bool overWrite)
        {
            return Guid.Empty;
        }

        public List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList, object context, object web)
        {
            return null;
        }

        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
        }

        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, Hashtable webAllProperties)
        {
            return false;
        }

        public bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp)
        {
            return false;
        }

        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
        }

        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
        }

        public Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            return null;
        }

        public void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties)
        {
        }

        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return null;
        }

        public void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp)
        {
        }

        public void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp)
        {
        }

        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
        }

        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
        }

        public Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdmins, List<Dictionary<string, object>> newAdmins)
        {
            return null;
        }

        public Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> sitePortalProperties)
        {
            return null;
        }

        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
        }

        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
        }

        public void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
        }

        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
        }

        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
        }
    }
}
