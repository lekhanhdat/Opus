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
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;
using AvePoint.GCommon.Contract.CodeReview;
using System.Xml;
using Microsoft365.Authentication;

namespace AvePoint.ObjectModel.WebService
{
    public class AveHttpWebRequestCommon2010 : IAveHttpWebRequestCommon, IDisposable
    {
        private string mLayout = "/_layouts";
        private string mSiteUrl;
        private string mWebAppName;
        private ITokenProvider tokenProvider;
        private AveWebServiceRequest mWebServiceRequest;
        private AveHttpWebRequestCommon mRequestCommon;
        private string mInternalServerVersion;

        public AveHttpWebRequestCommon2010(string siteUrl, ITokenProvider tokenProvider, string serverVersion, string internalServerVersion)
        {
            mSiteUrl = siteUrl;
            this.tokenProvider = tokenProvider;
            mInternalServerVersion = internalServerVersion;
            mWebServiceRequest = new AveWebServiceRequest(mSiteUrl, null, tokenProvider, serverVersion, internalServerVersion);
            mRequestCommon = new AveHttpWebRequestCommon(mSiteUrl, tokenProvider, "14", internalServerVersion);
        }

        internal string WebAppName
        {
            get
            {
                if (mWebAppName == null)
                {
                    int indexOfSlash = mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mSiteUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mSiteUrl.Substring(0, mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }

        #region Get

        public void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, ITokenProvider tokenProvider)
        {
            AveWebServiceRequest.GetWebSearchAndOfflineAvailability(this.WebAppName, webServerRelativeUrl, webProp, tokenProvider);
        }
        public List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId,int localedId,bool isTime24)
        {
            return mWebServiceRequest.GetListCheckOutFiles(webServerRelativeUrl, "", listId);
        }

        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            return mWebServiceRequest.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
        }
        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return mWebServiceRequest.GetMetadataListFieldSettings(webServerRelativeUrl, listTitle, listId);
        }
        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListRated(webServerRelativeUrl, listId);
        }
        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListRssProperties(webServerRelativeUrl, listId);
        }

        public Dictionary<string, object> GetAllFeatureDefinitions(string url, int lcid, string featuresSource)
        {
            return mWebServiceRequest.GetAllFeatureDefinitions(url, lcid, featuresSource);
        }

        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return mWebServiceRequest.GetMetadataNavigationSettings(webServerRelativeUrl, listId, listTitle);
        }

        public Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetPerLocationViewSettings(webServerRelativeUrl, listId);
        }

        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return mWebServiceRequest.GetDefaultRegionalSetting(webServerRelativeUrl, lcid);
        }

        public List<Dictionary<string, object>> GetKeyWords()
        {
            return mWebServiceRequest.GetKeyWords();
        }

        public List<string> GetSiteEnabledHelpCollections()
        {
            return mWebServiceRequest.GetSiteEnabledHelpCollections();
        }

        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mWebServiceRequest.GetPublishedContentTypes();
        }

        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            return mWebServiceRequest.GetSitePortal(siteUrl);
        }

        public bool GetSiteRssSetting()
        {
            return mWebServiceRequest.GetSiteRssSetting();
        }

        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj)
        {
            return mWebServiceRequest.GetListAdvancedSettingProperties(webServerRelativeUrl, listId);
        }

        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListGeneralProperties(webServerRelativeUrl, listId);
        }

        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListVersionLimited(webServerRelativeUrl, listId);
        }

        public string GetListExperience(string webServerRelativeUrl, Guid listId)
        {
            return string.Empty;
        }
        public void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            mWebServiceRequest.SetMetadataNavigationSettings(webServerRelativeUrl, listTitle, listId, updateProperties);
        }

        public int GetAuditFlags()
        {
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/AuditSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input ";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            string edit = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit";
            string checkInOut = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut";
            string moveCopy = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy";
            string deleteRestore = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore";
            string columnsContentType = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType";
            string search = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch";
            string perms = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms";
            int flag = 0;
            string tmp = string.Empty;
            int index = html.IndexOf(edit) + edit.Length + 1;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 16;
            }
            index = html.IndexOf(checkInOut) + checkInOut.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 3;
            }
            index = html.IndexOf(moveCopy) + moveCopy.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 6144;
            }
            index = html.IndexOf(deleteRestore) + deleteRestore.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 520;
            }
            index = html.IndexOf(columnsContentType) + columnsContentType.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 160;
            }
            index = html.IndexOf(search) + search.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 8192;
            }
            index = html.IndexOf(perms) + perms.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 256;
            }
            return flag;
        }

        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl)
        {
            return mWebServiceRequest.GetThemeUrlForWeb(webServerRelativeUrl);
        }

        #endregion

        #region Add

        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return mWebServiceRequest.AddBestBet(term, bestBetUrlList, bestBetProp, action);
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return mWebServiceRequest.AddKeyWord(term, startDate, localId, calendarType);
        }

        public string AddSynonm(string term, string synTerm, string terms)
        {
            return mWebServiceRequest.AddSynonm(term, synTerm, terms);
        }

        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            mWebServiceRequest.RestoreMasterPage(webServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
        }
        public void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, long availableStorageQuota, double availableResourceQuota)
        {
            throw new NotImplementedException();
        }
        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Update

        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties, AveNavigationInfoList navigationList)
        {
            return mWebServiceRequest.RestoreNavigation(webServerRelativeUrl, nodes, webAllProperties, navigationList);
        }

        public bool RestoreSearchNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            return mWebServiceRequest.RestoreSearchNavigation(webServerRelativeUrl, nodes, webAllProperties);
        }

        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            mWebServiceRequest.UpdateMetadataListFieldSettings(webServerRelativeUrl,"", listId, updateProperties);
        }

        public bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp)
        {
            return mWebServiceRequest.SetListRating(webServerRelativeUrl, listUrl, listId, enableRating);
        }

        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            mWebServiceRequest.UpdateListRssSetting(webServerRelativeUrl, listId, updateProp);
        }

        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            mWebServiceRequest.UpdateWebLogo(webServerRelativeUrl, webProperties);
        }

        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            mWebServiceRequest.UpdateWebSearchAndOfflineAvailability(webServerRelativeUrl, webProperties);
        }

        public void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
            mWebServiceRequest.UpdateWebRegionalSetting(webServerRelativeUrl, regionalProp);
        }

        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return mWebServiceRequest.UpdateKeyWord(term, localId, calendarType, keyWordProp);
        }

        public Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdmins, List<IDictionary<string, object>> newAdmins)
        {
            return mWebServiceRequest.UpdateSiteAdministrators(webServerRelativeUrl, oldAdmins, newAdmins);
        }

        public Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> sitePortalProperties)
        {
            return mWebServiceRequest.UpdateSitePortal(sitePortalProperties);
        }

        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            mWebServiceRequest.UpdateSiteRssSetting(syndicationEnabled);
        }

        public void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp)
        {
            mWebServiceRequest.UpdateListAdvancedSetting(webServerRelativeUrl, listId, advancedSettingProp);
        }

        public void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp)
        {
            mWebServiceRequest.UpdateListGeneralSetting(webServerRelativeUrl, listId, generalSettingProp);
        }

        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
            mRequestCommon.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
        }

        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
            mWebServiceRequest.MoveNavigationNodeToCollection(webServerRelativeUrl, navigationNodeProperties);
        }

        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            mRequestCommon.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
        }

        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            mWebServiceRequest.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }

        public List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList, object context, object web)
        {
            return mWebServiceRequest.RestoreFeatures(webServerRelativeUrl, force, scope, featuresSource, featureInfoList);
        }
        public void OperateOnVersion(string webServerRelativeUrl, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            AveHttpWebRequestCommon.OperateOnVersion(webServerRelativeUrl, webAppName, tokenProvider, listUrl, itemId, versionId, listId, fileName, op);
        }
        public Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            if (needUpdateProperties.ContainsKey("AuditFlags"))
            {
                int auditFlags = (int)needUpdateProperties["AuditFlags"];
                string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/AuditSettings.aspx";
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);

                Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                string searchContent = "<input ";
                AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

                if (bodyDic.ContainsKey("__EVENTVALIDATION"))
                {
                    bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
                }
                if (bodyDic.ContainsKey("__VIEWSTATE"))
                {
                    bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
                }
                string isEnable = (string)formValues["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"];
                if (isEnable.Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase))
                {
                    //"RadTrimAuditLogYes"
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                else
                {
                    //RadTrimAuditLogNo
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                #region convert flags
                if ((auditFlags & (int)AveAuditMaskType.Update) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit");
                }
                if ((auditFlags & (int)AveAuditMaskType.CheckIn) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut");
                }
                if ((auditFlags & (int)AveAuditMaskType.Copy) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy");
                }
                if ((auditFlags & (int)AveAuditMaskType.Delete) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore");
                }
                if ((auditFlags & (int)AveAuditMaskType.SchemaChange) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType");
                }
                if ((auditFlags & (int)AveAuditMaskType.Search) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch");
                }
                if ((auditFlags & (int)AveAuditMaskType.SecurityChange) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms");
                }
                bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                bodyDic.Remove("");

                #endregion
                bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$BtnUpdateAuditSettings"] = "OK";

                byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
            return needUpdateProperties;
        }
        #endregion

        #region Delete


        #endregion

        public void Dispose()
        {
            this.mWebServiceRequest.Dispose();
        }

        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            return mWebServiceRequest.AddAlert(webServerRelativeUrl, listUrl, listTitle, listId, itemId, data);
        }

        public void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties)
        {
            mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, properties);
        }

        //public void RefreshCredentials(object credentials)
        //{
        //    tokenProvider = credentials;
        //}

        public void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/spcontnt.aspx?url=" + HttpUtility.UrlEncode(fileServerRelativeUrl);
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            string webpartIdStr = webpartId.ToString("d").Replace('-', '_');
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, true);
            bodyDic["hdnToolbarAction"] = 2; //2 means resetdefaults
            bodyDic["ctl00$PlaceHolderMain$hdnCurrentView"] = "Shared";
            bodyDic["ctl00$PlaceHolderMain$" + webpartIdStr] = "on";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);            
        }

        public void CustomizeReport(Dictionary<string, object> parameters)
        {
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/CustomizeReport.aspx?ReportId=f43c916f-4450-4737-b889-8078c9826841&Category=Auditing";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();

            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }

            bodyDic["__SCROLLPOSITIONX"] = "0";
            bodyDic["__SCROLLPOSITIONY"] = "0";

            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$TxtReportStorageLocation"] = (string)parameters["LibraryLocation"];// "/docave library";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsLocation$ctl00$serializedId"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDate"] = (string)parameters["StartDateDate"];// "4/2/2012";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDateHours"] = (string)parameters["StartDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDateMinutes"] = (string)parameters["StartDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDate"] = (string)parameters["EndDateDate"];// "4/2/2013";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDateHours"] = (string)parameters["EndDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDateMinutes"] = (string)parameters["EndDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$hiddenSpanData"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$OriginalEntities"] = "<Entities />";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$HiddenEntityKey"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$HiddenEntityDisplayText"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$downlevelTextBox"] = "&#160;";

            if (parameters.ContainsKey("View") && ((string)parameters["View"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditView"] = "on";
            }
            if (parameters.ContainsKey("Update") && ((string)parameters["Update"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditUpdate"] = "on";
            }
            if (parameters.ContainsKey("CheckInOut") && ((string)parameters["CheckInOut"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditCheckInOut"] = "on";
            }
            if (parameters.ContainsKey("MoveCopy") && ((string)parameters["MoveCopy"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditMoveCopy"] = "on";
            }
            if (parameters.ContainsKey("DeleteRestore") && ((string)parameters["DeleteRestore"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditDeleteRestore"] = "on";
            }
            if (parameters.ContainsKey("ColumnContentType") && ((string)parameters["ColumnContentType"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditColumnContentType"] = "on";
            }
            if (parameters.ContainsKey("Search") && ((string)parameters["Search"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditSearch"] = "on";
            }
            if (parameters.ContainsKey("Perms") && ((string)parameters["Perms"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditPerms"] = "on";
            }
            if (parameters.ContainsKey("Change") && ((string)parameters["Change"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditChange"] = "on";
            }
            if (parameters.ContainsKey("Workflow") && ((string)parameters["Workflow"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditWorkflow"] = "on";
            }
            if (parameters.ContainsKey("Custom") && ((string)parameters["Custom"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditCustom"] = "on";
            }
            
            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$btnOK"] = "Ok";

            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);

            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);

        }

        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> masterPropDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_Layouts/ChangeSiteMasterPage.aspx";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj);
            
            html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);
            
            int index = html.IndexOf("ctl00$PlaceHolderMain$ctl00$ctl00$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList");
            string searchKey = "selected=\"selected\" value=\"";
            int startIndex = html.IndexOf(searchKey, index);
            if (startIndex > 0)
            {
                startIndex = startIndex + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex);
                string siteMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                masterPropDic["CustomMasterUrl"] = siteMasterUrl;
            }
            index = html.IndexOf("ctl00$PlaceHolderMain$ctl01$ctl00$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList");
            startIndex = html.IndexOf(searchKey, index);
            if (startIndex > 0)
            {
                startIndex = startIndex + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex);
                string sysMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                masterPropDic["MasterUrl"] = sysMasterUrl;
            }
            string searchContent = "<input name=\"ctl00$PlaceHolderMain$ctl02$ctl00$alternateCssSelector$AssetUrlInput\"";
            string infomation = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(infomation);
            masterPropDic["AlternateCssUrl"] = doc.FirstChild.Attributes["value"] != null ? doc.FirstChild.Attributes["value"].Value : default(string);
            return masterPropDic;
        }

        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            throw new NotImplementedException();
        }


        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            throw new NotImplementedException();
        }


        public void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields)
        {
            mRequestCommon.ReorderListFields(webServerRelativeUrl, listId, mappedSourceFields);
        }

        public List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl)
        {
            return null;
        }

        public List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib)
        {
            return null;
        }

        public bool GetRequestAccessEnable(string webUrl)
        {
            throw new NotImplementedException();
        }

        public bool SetRequestAccessEnable(string webUrl, bool value)
        {
            throw new NotImplementedException();
        }

        public bool GetAccessRequestApprover(string webUrl)
        {
            throw new NotImplementedException();
        }

        public void SetAccessRequestApprover(string webUrl, bool value, string email)
        {
            throw new NotImplementedException();
        }
    }
}
