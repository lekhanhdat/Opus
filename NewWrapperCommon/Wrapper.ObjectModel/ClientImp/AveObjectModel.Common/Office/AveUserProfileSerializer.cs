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
using AvePoint.Wrapper.Common.Office;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveUserProfileSerializer : IAveUserProfileSerializer
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveUserProfileSerializer));

        private AveSite site;
        private AveSiteInfo sourceSiteInfo;
        private IAveRequest request;
        public bool ExistSkip { get; set; }
        public AveUserProfileSerializer(AveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo)
        {
            new AveUserProfileSerializer(site, login, needInit, sourceSiteInfo, null);
        }
        public AveUserProfileSerializer(AveSite site, string login, bool needInit, AveSiteInfo sourceSiteInfo, Func<String, String> userMapping)
        {
            this.site = site;
            this.request = site.Request;
            this.sourceSiteInfo = sourceSiteInfo;
        }

        /// <summary>
        ///  还原user profile PropertiesWithSecton属性；
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="isOverWrite"></param>
        public void SetObjectData(List<AvePropertyInfo> properties, bool isOverWrite)
        {
            Dictionary<string, object> userProfilePropertiesInfo = AssembleUserProfilePropertiesInfo(properties);
            Dictionary<string, object> restoreResult = request.RestoreUserProfileProperties(userProfilePropertiesInfo, isOverWrite);
            if (restoreResult.ContainsKey("Exception"))
            {
                throw new Exception(restoreResult["Exception"].ToString());
            }
        }

        /// <summary>
        /// 还原user profile，包括Colleagues，Properties，Memberships，Comments，Tags，Links；
        /// </summary>
        /// <param name="profileInfo"></param>
        public void SetObjectData(AveUserProfileInfo profileInfo)
        {
            Dictionary<string, object> userProfileInfo = AssembleUserProfileInfo(profileInfo);
            if (userProfileInfo != null)
            {
                Dictionary<string, object> restoreResult = request.RestoreUserProfileInfo(userProfileInfo, site.IsOnlineSite, ExistSkip);
                if (restoreResult.ContainsKey("Exception"))
                {
                    throw new Exception(restoreResult["Exception"].ToString());
                }
            }
        }

        public object SetObjectData(object userProfile)
        {
            throw new NotImplementedException();
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }
        #region AssembleUserProfile
        private Dictionary<string, object> AssembleUserProfileInfo(AveUserProfileInfo userProfileInfo)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string loginName = GetMappingUserName(userProfileInfo.LoginName, userProfileInfo.UserMapping, true);
            EnsureUserLogin(ref loginName);
            if (loginName != null)
            {
                data["LoginName"] = loginName;
                data["Colleagues"] = AssembleUserProfileColleagueInfo(userProfileInfo.Colleagues, userProfileInfo.UserMapping);
                data["Links"] = AssembleUserProfileQuickLinkInfo(userProfileInfo.Links);
                data["Properties"] = AssembleUserProfileValueInfo(userProfileInfo.Properties, loginName, userProfileInfo.UserMapping);
                data["Comments"] = AssembleSocialCommentInfo(userProfileInfo.Comments);
                data["Tags"] = AssembleSocialTagInfo(userProfileInfo.Tags);
                data["Memberships"] = AssembleMembershipInfo(userProfileInfo.Memberships);
                data["SourceSiteInfo"] = AssembleSourSiteInfo(sourceSiteInfo);
                data["DestinationSiteUrl"] = site.Url;
                return data;
            }
            return null;
        }

        private Dictionary<string, object> AssembleSourSiteInfo(AveSiteInfo sourceSiteInfo)
        {
            Dictionary<string, object> siteInfos = new Dictionary<string, object>();
            siteInfos["ServerRelativeUrl"] = sourceSiteInfo.ServerRelativeUrl;
            siteInfos["IsHostheader"] = sourceSiteInfo.IsHostheader;
            //siteInfos["WebAppUrl"] = sourceSiteInfo.WebAppUrl;
            siteInfos["Url"] = sourceSiteInfo.Url;
            //siteInfos["Title"] = sourceSiteInfo.Title;
            //siteInfos["Description"] = sourceSiteInfo.Description;
            //siteInfos["LCID"] = sourceSiteInfo.LCID;
            //siteInfos["WebTemplate"] = sourceSiteInfo.WebTemplate;
            //siteInfos["OwnerLogin"] = sourceSiteInfo.OwnerLogin;
            //siteInfos["OwnerName"] = sourceSiteInfo.OwnerName;
            //siteInfos["OwnerEmail"] = sourceSiteInfo.OwnerEmail;
            //siteInfos["SecondaryContactLogin"] = sourceSiteInfo.SecondaryContactLogin;
            //siteInfos["SecondaryContactName"] = sourceSiteInfo.SecondaryContactName;
            //siteInfos["SecondaryContactEmail"] = sourceSiteInfo.SecondaryContactEmail;
            //siteInfos["AllWebTemplates"] = sourceSiteInfo.AllWebTemplates;
            //siteInfos["Prefixes"] = sourceSiteInfo.Prefixes;
            return siteInfos;
        }

        private List<Dictionary<string, object>> AssembleMembershipInfo(List<AveMembershipInfo> Memberships)
        {
            List<Dictionary<string, object>> membershipsList = new List<Dictionary<string, object>>();
            foreach (AveMembershipInfo membershipInfo in Memberships)
            {
                Dictionary<string, object> membershipData = new Dictionary<string, object>();
                membershipData["Title"] = membershipInfo.Title;
                membershipData["Group"] = membershipInfo.Group;
                membershipData["GroupType"] = membershipInfo.GroupType;
                membershipData["IsEditable"] = membershipInfo.IsEditable;
                membershipData["IsPrivacyLevelEditable"] = membershipInfo.IsPrivacyLevelEditable;
                membershipData["IsTitleEditable"] = membershipInfo.IsTitleEditable;
                membershipData["IsUrlEditable"] = membershipInfo.IsUrlEditable;
                membershipData["PrivacyLevel"] = membershipInfo.PrivacyLevel;
                membershipData["Url"] = membershipInfo.Url;

                membershipData["Policy"] = new Dictionary<string, object>();
                InitPolicyPropertity(membershipData["Policy"] as Dictionary<string, object>, membershipInfo.Policy);

                membershipData["MembershipGroup"] = new Dictionary<string, object>();
                InitMembershipGroup(membershipData["MembershipGroup"] as Dictionary<string, object>, membershipInfo.MembershipGroup);
                membershipsList.Add(membershipData);
            }
            return membershipsList;
        }

        private void InitMembershipGroup(Dictionary<string, object> membershipGroupData, AveMembershipGroup aveMembershipGroup)
        {
            membershipGroupData["Count"] = aveMembershipGroup.Count;
            membershipGroupData["Description"] = aveMembershipGroup.Description;
            membershipGroupData["DisplayName"] = aveMembershipGroup.DisplayName;
            membershipGroupData["Count"] = aveMembershipGroup.Id;
            membershipGroupData["LastUpdate"] = aveMembershipGroup.LastUpdate;
            membershipGroupData["MailNickName"] = aveMembershipGroup.MailNickName;
            membershipGroupData["Source"] = aveMembershipGroup.Source;
            membershipGroupData["SourceInternal"] = aveMembershipGroup.SourceInternal;
            membershipGroupData["SourceReference"] = aveMembershipGroup.SourceReference;
            membershipGroupData["Url"] = aveMembershipGroup.Url;
        }

        private List<Dictionary<string, object>> AssembleSocialTagInfo(List<AveSocialTagInfo> Tags)
        {
            List<Dictionary<string, object>> tagsList = new List<Dictionary<string, object>>();
            foreach (AveSocialTagInfo socialTagInfo in Tags)
            {
                Dictionary<string, object> socialTagData = new Dictionary<string, object>();
                socialTagData["ProfileManagerUrl"] = socialTagInfo.ProfileManagerUrl;
                socialTagData["Url"] = socialTagInfo.Url;
                socialTagData["Title"] = socialTagInfo.Title;
                socialTagData["Owner"] = socialTagInfo.Owner;
                socialTagData["IsPrivate"] = socialTagInfo.IsPrivate;
                socialTagData["LastModifiedTime"] = socialTagInfo.LastModifiedTime;

                //for term
                socialTagData["Term"] = new Dictionary<string, object>();
                InitTermInfo(socialTagData["Term"] as Dictionary<string, object>, socialTagInfo.Term);
                tagsList.Add(socialTagData);
            }
            return tagsList;
        }

        private void InitTermInfo(Dictionary<string, object> termData, AveTermInfo termInfo)
        {
            termData["TermName"] = termInfo.TermName;
            termData["IsKeyword"] = termInfo.IsKeyword;
            termData["IsRoot"] = termInfo.IsRoot;
            termData["SourceTermName"] = termInfo.SourceTermName;
            termData["SourceTermId"] = termInfo.SourceTermId;
            termData["IsAvailableForTagging"] = termInfo.IsAvailableForTagging;
            termData["Id"] = termInfo.Id;
            termData["Owner"] = termInfo.Owner;
        }

        private List<Dictionary<string, object>> AssembleSocialCommentInfo(List<AveSocialCommentInfo> Comments)
        {
            List<Dictionary<string, object>> commentsList = new List<Dictionary<string, object>>();
            foreach (AveSocialCommentInfo socialCommentInfo in Comments)
            {
                Dictionary<string, object> socialCommentData = new Dictionary<string, object>();
                socialCommentData["ProfileManagerUrl"] = socialCommentInfo.ProfileManagerUrl;
                socialCommentData["Url"] = socialCommentInfo.Url;
                socialCommentData["Comment"] = socialCommentInfo.Comment;
                socialCommentData["Owner"] = socialCommentInfo.Owner;
                socialCommentData["IsHighPriority"] = socialCommentInfo.IsHighPriority;
                socialCommentData["Title"] = socialCommentInfo.Title;
                socialCommentData["LastModifiedTime"] = socialCommentInfo.LastModifiedTime;
                commentsList.Add(socialCommentData);
            }
            return commentsList;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SPS-DontSuggestList")]
        private List<Dictionary<string, object>> AssembleUserProfileValueInfo(List<AveUserProfileValueInfo> Properties, string loginName, Dictionary<string, string> userMappingInfo)
        {
            List<Dictionary<string, object>> propertiesList = new List<Dictionary<string, object>>();
            List<string> personTypePro = new List<string>();
            personTypePro.Add("Manager");
            personTypePro.Add("SPS-Dotted-line");
            personTypePro.Add("SPS-DontSuggestList");
            personTypePro.Add("Assistant");
            foreach (AveUserProfileValueInfo userProfileValueInfo in Properties)
            {
                List<string> needUpdateUser = ProcessSpecialPropertyValues(userProfileValueInfo, loginName, personTypePro, userMappingInfo);
                Dictionary<string, object> propertiesData = new Dictionary<string, object>();
                propertiesData["Name"] = userProfileValueInfo.Name;
                propertiesData["Capacity"] = userProfileValueInfo.Capacity;
                propertiesData["Count"] = userProfileValueInfo.Count;
                propertiesData["Privacy"] = userProfileValueInfo.Privacy;
                propertiesData["Values"] = needUpdateUser.Count > 0 ? needUpdateUser : userProfileValueInfo.Values;
                propertiesList.Add(propertiesData);
            }
            return propertiesList;
        }

        private List<string> ProcessSpecialPropertyValues(AveUserProfileValueInfo userProfileValueInfo, string loginName, List<string> personTypePro, Dictionary<string, string> userMappingInfo)
        {
            List<string> needUpdateUser = new List<string>();
            if (userProfileValueInfo.Name.Equals("PictureURL") && userProfileValueInfo.Values.Count == 1)// Process PictureURL 
            {
                string sourcePictureURL = userProfileValueInfo.Values[0];
                var indexPos = sourcePictureURL.IndexOf("/User%20Photos/Profile%20Pictures", StringComparison.OrdinalIgnoreCase);
                if (indexPos > 0)
                {
                    string myHostUrl = GetMySiteHostUrl(site.Url);
                    if (!string.IsNullOrEmpty(myHostUrl))
                    {
                        needUpdateUser.Add(myHostUrl.TrimEnd('/') + sourcePictureURL.Substring(indexPos));
                    }
                    else
                    {
                        needUpdateUser.Add(sourcePictureURL);
                    }
                }
                return needUpdateUser;
            }
            if (personTypePro.Contains(userProfileValueInfo.Name))//Process Person Type Property
            {
                string prefixType = loginName.Contains("|") ? loginName.Substring(0, loginName.LastIndexOf('|') + 1) : null;
                List<string> userList = userProfileValueInfo.Values;
                foreach (string userinfo in userList)
                {
                    string mappingName = GetMappingUserName(userinfo, userMappingInfo, true);
                    //string name = prefixType != null && !mappingName.StartsWith(prefixType,StringComparison.OrdinalIgnoreCase) ? prefixType + mappingName : mappingName;//有些name backup下来是全称
                    EnsureUserLogin(ref mappingName);
                    if (!string.IsNullOrEmpty(mappingName))
                    {
                        needUpdateUser.Add(mappingName);
                    }
                    else
                    {
                        continue;
                    }
                    //try
                    //{
                    //    request.GetEnsureUser(site.ServerRelativeUrl, name);
                    //    needUpdateUser.Add(userinfo);
                    //}
                    //catch (Exception e)
                    //{
                    //    mLogger.Debug("get user find error: {0}", e.Message);
                    //    continue;
                    //}
                }
            }
            return needUpdateUser;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "onmicrosoft")]
        public string GetMySiteHostUrl(string mWebUrl)
        {
            //if (site.UserAccountInfo.UserName.Contains(".onmicrosoft.com") && site.Url.Contains(".sharepoint.com"))//it is means that the destination site is real office365
            if (site.IsOnlineSite)
            {
                string domain = new Uri(mWebUrl).Host;
                return string.Format("https://{0}-my.sharepoint.com", domain.Substring(0, domain.IndexOf('.')));
            }
            return null;
        }

        private List<Dictionary<string, object>> AssembleUserProfileQuickLinkInfo(List<AveQuickLinkInfo> Links)
        {
            List<Dictionary<string, object>> quickLinksList = new List<Dictionary<string, object>>();
            foreach (AveQuickLinkInfo quickLink in Links)
            {
                string url = quickLink.Url;
                string profileManagerUrl = quickLink.ProfileManagerUrl;

                Dictionary<string, object> quickLinkData = new Dictionary<string, object>();
                quickLinkData["ProfileManagerUrl"] = profileManagerUrl;
                quickLinkData["Title"] = quickLink.Title;
                quickLinkData["Group"] = quickLink.Group;
                quickLinkData["GroupType"] = quickLink.GroupType;
                quickLinkData["PrivacyLevel"] = quickLink.PrivacyLevel;
                //replace quick link url
                Dictionary<string, string> absoluteUrlMapping = new Dictionary<string, string>();
                absoluteUrlMapping.Add(sourceSiteInfo.Url, site.Url);
                url = AveReplaceProcessor.UrlReplace(url, absoluteUrlMapping, new ReplaceOption(true, true), sourceSiteInfo, site.Url);
                //if (profileManagerUrl != null && url.Contains(profileManagerUrl))
                //{
                //    url = url.Replace(profileManagerUrl, profileManagerUrl);
                //}
                quickLinkData["Url"] = url;

                quickLinkData["Policy"] = new Dictionary<string, object>();
                InitPolicyPropertity(quickLinkData["Policy"] as Dictionary<string, object>, quickLink.Policy);
                quickLinksList.Add(quickLinkData);
            }
            return quickLinksList;
        }

        private List<Dictionary<string, object>> AssembleUserProfileColleagueInfo(List<AveColleagueInfo> Colleagues, Dictionary<string, string> userMappingInfo)
        {
            List<Dictionary<string, object>> colleaguesList = new List<Dictionary<string, object>>();
            foreach (AveColleagueInfo colleague in Colleagues)
            {
                Dictionary<string, object> colleagueData = new Dictionary<string, object>();
                string accountName = GetMappingUserName(colleague.AccountName, userMappingInfo, true);
                EnsureUserLogin(ref accountName);
                if (string.IsNullOrEmpty(accountName))
                {
                    continue;
                }
                colleagueData["AccountName"] = accountName;
                colleagueData["Group"] = colleague.Group;
                colleagueData["GroupType"] = colleague.GroupType;
                colleagueData["IsInWorkGroup"] = colleague.IsInWorkGroup;
                colleagueData["PrivacyLevel"] = colleague.PrivacyLevel;
                colleagueData["IsAssistant"] = colleague.IsAssistant;
                colleagueData["IsEditable"] = colleague.IsEditable;
                colleagueData["IsPrivacyLevelEditable"] = colleague.IsPrivacyLevelEditable;
                colleagueData["IsTitleEditable"] = colleague.IsTitleEditable;
                colleagueData["IsUrlEditable"] = colleague.IsUrlEditable;
                colleagueData["Url"] = colleague.Url;
                colleagueData["Title"] = colleague.Title;
                Dictionary<string, object> policyData = new Dictionary<string, object>();
                InitPolicyPropertity(policyData, colleague.Policy);
                colleagueData["Policy"] = policyData;
                colleaguesList.Add(colleagueData);
            }
            return colleaguesList;
        }

        private void InitPolicyPropertity(Dictionary<string, object> policyData, AvePolicyInfo policy)
        {
            policyData["AllowPolicyOverride"] = policy.AllowPolicyOverride;
            policyData["DefaultPrivacy"] = policy.DefaultPrivacy;
            policyData["DisplayName"] = policy.DisplayName;
            policyData["FilterPrivacyItems"] = policy.FilterPrivacyItems;
            policyData["Group"] = policy.Group;
            policyData["PrivacyPolicy"] = policy.PrivacyPolicy;
            policyData["UserOverridePrivacy"] = policy.UserOverridePrivacy;
        }

        public Dictionary<string, object> AssembleUserProfilePropertiesInfo(List<AvePropertyInfo> propertiesList)
        {
            Dictionary<string, object> userProfilePropertiesInfo = new Dictionary<string, object>();
            List<Dictionary<string, object>> avePropertyInfoList = new List<Dictionary<string, object>>();
            foreach (AvePropertyInfo propertyInfo in propertiesList)
            {
                Dictionary<string, object> info = new Dictionary<string, object>();
                info["AllowPolicyOverride"] = propertyInfo.AllowPolicyOverride;
                info["DefaultPrivacy"] = propertyInfo.DefaultPrivacy;
                info["Description"] = propertyInfo.Description;
                info["DescriptionLocalized"] = propertyInfo.DescriptionLocalized;
                info["DisplayName"] = propertyInfo.DisplayName;
                info["DisplayNameLocalized"] = propertyInfo.DisplayNameLocalized;
                info["DisplayOrder"] = propertyInfo.DisplayOrder;
                info["IsAdminEditable"] = propertyInfo.IsAdminEditable;
                info["IsAlias"] = propertyInfo.IsAlias;
                info["IsColleagueEventLog"] = propertyInfo.IsColleagueEventLog;
                info["IsImported"] = propertyInfo.IsImported;
                info["IsMultivalued"] = propertyInfo.IsMultivalued;
                info["IsReplicable"] = propertyInfo.IsReplicable;
                info["IsRequired"] = propertyInfo.IsRequired;
                info["IsSearchable"] = propertyInfo.IsSearchable;
                info["IsSection"] = propertyInfo.IsSection;
                info["IsSystem"] = propertyInfo.IsSystem;
                info["IsUpgrade"] = propertyInfo.IsUpgrade;
                info["IsUpgradePrivate"] = propertyInfo.IsUpgradePrivate;
                info["IsUserEditable"] = propertyInfo.IsUserEditable;
                info["IsVisibleOnEditor"] = propertyInfo.IsVisibleOnEditor;
                info["IsVisibleOnViewer"] = propertyInfo.IsVisibleOnViewer;
                info["Length"] = propertyInfo.Length;
                info["ManagedPropertyName"] = propertyInfo.ManagedPropertyName;
                info["MaximumShown"] = propertyInfo.MaximumShown;
                info["Name"] = propertyInfo.Name;
                info["PrivacyPolicy"] = propertyInfo.PrivacyPolicy;
                info["Separator"] = propertyInfo.Separator;
                info["Type"] = propertyInfo.Type;
                if (propertyInfo.URI != null)
                {
                    info["URI"] = propertyInfo.URI;
                }
                info["UserOverridePrivacy"] = propertyInfo.UserOverridePrivacy;
                avePropertyInfoList.Add(info);
            }
            userProfilePropertiesInfo.Add(AveObjectModelConstant.ChildrenProperties, avePropertyInfoList);
            return userProfilePropertiesInfo;
        }

        public string GetMappingUserName(string name, Dictionary<string, string> userMappingInfo, bool needMapping)
        {
            if (name.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase)
                || name.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase)
                || name.Equals("NT AUTHORITY\\local service", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
            if (!needMapping && !IsSP10FBAUser(name))
            {
                return name;
            }
            string logonName = userMappingInfo.ContainsKey(name) ? userMappingInfo[name] : name;
            if (!string.IsNullOrEmpty(logonName) && logonName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
            {
                if (site != null && site.IsClassicWindowsModeAuthentication)
                {
                    logonName = logonName.Substring("i:0#.w|".Length);
                }
            }
            return logonName;
        }
        private bool IsSP10FBAUser(string login)
        {
            return (login.IndexOf(".f|", StringComparison.OrdinalIgnoreCase) > 0
             || login.IndexOf(".m|", StringComparison.OrdinalIgnoreCase) > 0
             || login.IndexOf(".r|", StringComparison.OrdinalIgnoreCase) > 0);
        }

        internal void EnsureUserLogin(ref string loginName)
        {
            try
            {
                IAveUtility utility = new AveUtility();
                var info = utility.ResolvePrincipal(site.RootWeb, loginName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                loginName = info.LoginName;
            }
            catch (Exception e)
            {
                mLogger.Warn("get user find error: {0}", e.Message);
                loginName = null;
            }
        }

        #region olde code
        //public string GetEnsureUser(string loginName)
        //{
        //    try
        //    {
        //        IAveUser user = null;
        //        user = site.RootWeb.SiteUsers.GetByLoginName(loginName);
        //        if (user == null)
        //        {
        //            if (loginName.Contains("membership:"))
        //            {
        //                loginName = (loginName.Split(':') as string[])[1];
        //            }
        //            user = site.RootWeb.EnsureUser(loginName);
        //            if (user != null)
        //            {
        //                return user.LoginName;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        mLogger.Debug("get user find error: {0}", e.Message);
        //        return null;
        //    }
        //    return loginName;
        //}
        #endregion
        #endregion


        public void SetObjectDataForArchiver(AveUserProfileInfo profileInfo)
        {
            throw new NotImplementedException();
        }


        public void SetObjectData(List<AveUserProfileSubTypeInfo> subTypes)
        {
            throw new NotImplementedException();
        }

        public void SetObjectData(List<AveSOcialRatingInfo> ratingInfo)
        {
            //throw new NotImplementedException();
            mLogger.Debug("This constructor is not supported in BPOS mode. Method: {0}", "SetObjectData(List<AveSocialRatingInfo> ratingInfo)");
        }
    }
}