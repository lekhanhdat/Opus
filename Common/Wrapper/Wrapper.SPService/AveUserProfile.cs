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
using System.Reflection;
using System.IO;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Collections;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.SPService
{
    public class AveUserProfile
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //private IAveOUserProfile mUserProfile = null;
        private AveServiceContext mServiceContext = null;
        private string mSourceSiteUrl;


        private IAveWebApplication mWebApp = null;
        private Dictionary<string, string> mAbsoluteUrlMapping = new Dictionary<string, string>();
        private bool mEnableTag = false;
        private AveColleague mColleague;
        private AveMembership mMembership;
        private AveQuickLink mQuickLink;
        private AveSiteInfo mSourceSiteInfo;
        private string mDestSiteUrl;
        private bool mUserProfileIsNewCreated;

        private AveColleague Colleague
        {
            get
            {
                if (mColleague == null)
                {
                    mColleague = new AveColleague(this);
                }
                return mColleague;
            }
        }
        private AveMembership Membership
        {
            get
            {
                if (mMembership == null)
                {
                    mMembership = new AveMembership(this);
                }
                return mMembership;
            }
        }
        private AveQuickLink QuickLink
        {
            get
            {
                if (mQuickLink == null)
                {
                    mQuickLink = new AveQuickLink(this);
                }
                return mQuickLink;
            }
        }

        /// <summary>
        /// 设为True，就会在Restore User Profile的时候还原其中的Tag, Note，否则不还原。
        /// CM, DM, Replicator使用
        /// </summary>
        public bool EnableTag
        {
            set { mEnableTag = value; }
        }

        public bool IsUserProfileNewCreated
        {
            get { return mUserProfileIsNewCreated; }
        }

        public Dictionary<string, string> AbsoluteUrlMapping
        {
            get { return mAbsoluteUrlMapping; }
            set { mAbsoluteUrlMapping = value; }
        }

        public bool ExistSkip { get; set; }

        public bool Overwrite
        {
            get;
            set;
        }

        public IAveOUserProfile UserProfile
        {
            get { return mServiceContext.UserProfile; }
            set { mServiceContext.UserProfile = value; }
        }
        public AveServiceContext ServiceContext
        {
            get { return mServiceContext; }
        }

        public AveUserMap UserMap
        {
            get;
            set;
        }

        public AveSiteInfo SourceSiteInfo
        {
            get
            {
                return mSourceSiteInfo;
            }
        }

        public string DestSiteUrl
        {
            get
            {
                return mDestSiteUrl;
            }
        }

        //public AveUserProfile(IAveSite site, AveObjectModelFactory fac, string sourceUrl, bool needInit)
        //    : this(site, fac, site.Owner.LoginName, sourceUrl, needInit)
        //{ }

        //public AveUserProfile(IAveSite site, AveObjectModelFactory fac, string loginName, string sourceUrl, bool needInit)
        //{
        //    mServiceContext = new AveServiceContext(site, fac);
        //    mSourceSiteUrl = sourceUrl;
        //    mWebApp = site.WebApplication;
        //    if (needInit)
        //    {
        //        this.UserProfile = mServiceContext.UserProfileManager.GetUserProfile(loginName);
        //        this.ServiceContext.LoginName = loginName;
        //    }
        //}

        public AveUserProfile(AveServiceContext context, bool needInit, AveSiteInfo sourceSiteInfo, string destSiteUrl)
            : this(context, context.Site.Owner.LoginName, needInit, sourceSiteInfo, destSiteUrl)
        { }

        public AveUserProfile(AveServiceContext context, string loginName, bool needInit, AveSiteInfo sourceSiteInfo, string destSiteUrl)
        {
            CheckUserPermission(context);
            mSourceSiteUrl = sourceSiteInfo.ServerRelativeUrl;
            mWebApp = context.Site.WebApplication;
            mServiceContext = context;
            mSourceSiteInfo = sourceSiteInfo;
            mDestSiteUrl = destSiteUrl;
            if (needInit)
            {
                string mappedLoginName = ServiceContext.GetMappingUser(loginName);
                if (context.UserProfileManager.UserExists(mappedLoginName))
                {
                    this.UserProfile = context.UserProfileManager.GetUserProfile(mappedLoginName);
                }
                else
                {
                    this.UserProfile = context.UserProfileManager.CreateUserProfile(mappedLoginName);
                }
                this.ServiceContext.LoginName = mappedLoginName;
            }
        }

        /// <summary>
        /// The mothod is only to volidate the user permission and service available.
        /// If the docave user has no permission ,here will throw excetion from sharepoint.
        /// </summary>
        /// <param name="context"></param>
        private void CheckUserPermission(AveServiceContext context)
        {
            var manager = context.UserProfileManager;
        }

        internal bool CheckServiceAvailable()
        {
            bool ifAvailable = true;
            try
            {
                if (!AveSPUtility.IfServiceAvailable(mWebApp, ServiceApplicationType.UserProfileService))
                {
                    //mLog.Log(AveLogLevel.ERROR, "WP10RTMySite0086", mWebApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri);
                    log.Error("There is no User Profile Service associate with the web application: {0}", mWebApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString());
                    ifAvailable = false;
                }
            }
            catch (Exception ex)
            {
                ifAvailable = false;
                log.Log(AveLogLevel.ERROR, "WP10RTUserPro120 {0}", ex.ToString());
            }
            return ifAvailable;
        }

        internal IAveOUserProfile FindOrCreateUserProfile(string loginName)
        {
            IAveOUserProfile userProfile = null;
            if (mServiceContext.UserProfileManager.UserExists(loginName))
            {
                userProfile = mServiceContext.UserProfileManager.GetUserProfile(loginName);
            }
            else
            {
                userProfile = mServiceContext.UserProfileManager.CreateUserProfile(loginName);
                mUserProfileIsNewCreated = true;
            }
            mServiceContext.AddUserProfileCache(userProfile);
            return userProfile;
        }

        public void Restore(AveUserProfileInfo userProfile)
        {
            try
            {
                string login = this.ServiceContext.GetMappingUser(userProfile.LoginName);

                this.UserProfile = FindOrCreateUserProfile(login);
                this.ServiceContext.LoginName = login;
                if (!mUserProfileIsNewCreated && this.ExistSkip)
                {
                    return;
                }
                RestoreDetails(userProfile.Properties);
                this.Colleague.CreateColleagues(userProfile.Colleagues);
                this.Membership.CreateMemberships(userProfile.Memberships);
                this.QuickLink.CreateQuickLinks(userProfile.Links);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "WP10RTUserPro100 {0}", ex.ToString());
            }
        }

        public void RestoreQuickLink(AveQuickLinkInfo lInfo)
        {
            this.QuickLink.CreateQuickLink(lInfo);
        }

        public void RestoreMembership(AveMembershipInfo mInfo)
        {
            this.Membership.CreateMembership(mInfo);
        }

        public void RestoreColleague(AveColleagueInfo colleagueInfo)
        {
            this.Colleague.CreateColleague(colleagueInfo);
        }

        public void Restore(List<AveUserProfileInfo> userProfiles)
        {
            if (!CheckServiceAvailable())
            {
                return;
            }

            #region --Output Controller--
            log.Info("The status of restoring social tags is :" + mEnableTag);
            #endregion

            foreach (AveUserProfileInfo profile in userProfiles)
            {
                try
                {
                    this.Restore(profile);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "WP10RTUserPro110 {0}  {1}", ex, profile.LoginName);
                }
            }
        }

        public void RestoreDetails(List<AveUserProfileValueInfo> values)
        {
            if (values == null)
            {
                return;
            }
            foreach (AveUserProfileValueInfo value in values)
            {
                RestoreDetail(value);
            }
        }

        public void RestoreDetail(AveUserProfileValueInfo profileValue)
        {
            RestoreDetail(profileValue, true);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Property of UserProfile are as attributes in xml.")]
        public void RestoreDetail(AveUserProfileValueInfo profileValue, bool isOverwrite)
        {
            if (UserProfile == null)
            {
                return;
            }
            try
            {
                string propertyName = profileValue.Name;// xe.Attributes["NameValue"].Value;

                //If we know the properties list below won't be restored, why we backup them?
                if (propertyName.ToLower() == "sps-proxyaddresses" || propertyName.ToLower() == "sps-masteraccountname"
                    || propertyName.ToLower() == "adguid" || propertyName.ToLower() == "quicklinks"
                    || propertyName.ToLower() == "sps-peers" || propertyName.ToLower() == "sps-resourceaccountname"
                    || propertyName.ToLower() == "sps-resourcesid" || propertyName == "UserProfile_GUID"
                    || propertyName == "SID" || propertyName == "AccountName"
                    || propertyName == "UserName" || propertyName == "PersonalSpace")
                {
                    return;
                }
                if (propertyName.Equals("WorkEmail", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("FirstName", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("PreferredName", StringComparison.OrdinalIgnoreCase))
                { }
                if (this.ServiceContext.UserProfileManager.Properties.GetPropertyByName(propertyName) == null)
                {
                    //IAveOPropertyCollection pc = this.ServiceContext.UserProfileManager.Properties;
                    //IAveOProperty p = pc.Create(false);
                    //p.Name = propertyName;
                    //p.DisplayName = profileValue.Property.DisplayName;// subxe.Attributes["DisplayName"].Value;
                    //p.Type = profileValue.Property.DisplayName;// subxe.Attributes["Type"].Value;
                    //p.Length = profileValue.Property.Length;//Convert.ToInt32(subxe.Attributes["Length"].Value);
                    //p.PrivacyPolicy = (AvePrivacyPolicy)profileValue.Property.PrivacyPolicy;//(AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                    //p.DefaultPrivacy = (AvePrivacy)profileValue.Property.DefaultPrivacy;//(AvePrivacy)Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                    //p.IsMultivalued = profileValue.Property.IsMultivalued;//Convert.ToBoolean(subxe.Attributes["IsMultivalued"].Value);
                    //p.IsUserEditable = profileValue.Property.IsUserEditable;//Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                    //p.IsVisibleOnEditor = profileValue.Property.IsVisibleOnEditor;//Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                    //p.IsVisibleOnViewer = profileValue.Property.IsVisibleOnViewer;//Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);

                    //try
                    //{
                    //    pc.Add(p);
                    //}
                    //catch (Exception e)
                    //{
                    //    //mLog.Log(AveLogLevel.ERROR, "WP10RTMySite0155", p.Name, e);
                    //    mLog.Log(AveLogLevel.ERROR, "Cannot add property: {0}, error:{1}", p.Name, e.ToString());
                    //    return;
                    //}
                    log.Warn("Cannot get user profile property:{0}.", propertyName);
                    return;
                }
                else if (!isOverwrite)
                {
                    return;
                }
                int capacityValue = profileValue.Capacity;
                //We can not modify the value of Capacity to 0 when Capacity is not 0.
                if (capacityValue > 0 || this.UserProfile[propertyName].Capacity == 0)
                {
                    this.UserProfile[propertyName].Capacity = capacityValue;
                }

                this.UserProfile[propertyName].Privacy = (AvePrivacy)profileValue.Privacy;
                #region We have not backuped ChoiceList
                //XmlElement subsubxe;

                //subsubxe = (XmlElement)xn2.ChildNodes[0];
                //if (subsubxe.HasAttributes && (Convert.ToInt32(subsubxe.Attributes["Count"].Value) != 0))
                //{
                //    for (int i = 0; i < Convert.ToInt32(subsubxe.Attributes["Count"].Value); i++)
                //    {
                //        mUserProfile[property].Property.ChoiceList.Add(subsubxe.Attributes["ChoiceValue" + i.ToString()].Value);
                //    }
                //}
                //mUserProfile[property].Property.ChoiceType = (ChoiceTypes)Convert.ToInt32(subxe.Attributes["ChoiceType"].Value);
                #endregion

                //IAveOProperty property = this.UserProfile[propertyName].Property;
                //AvePropertyInfo propertyInfo = profileValue.Property;
                //property.Description = propertyInfo.Description;// subxe.Attributes["Description"].Value;
                //property.DisplayName = propertyInfo.DisplayName;//subxe.Attributes["DisplayName"].Value;
                //property.IsAlias = propertyInfo.IsAlias;//Convert.ToBoolean(subxe.Attributes["IsAlias"].Value);
                //property.IsColleagueEventLog = propertyInfo.IsColleagueEventLog;//Convert.ToBoolean(subxe.Attributes["IsColleagueEventLog"].Value);
                //property.IsReplicable = propertyInfo.IsReplicable;//Convert.ToBoolean(subxe.Attributes["IsReplicable"].Value);
                //property.IsSearchable = propertyInfo.IsSearchable;//Convert.ToBoolean(subxe.Attributes["IsSearchable"].Value);
                //property.IsUpgrade = propertyInfo.IsUpgrade;//Convert.ToBoolean(subxe.Attributes["IsUpgrade"].Value);
                //property.IsUpgradePrivate = propertyInfo.IsUpgradePrivate;//Convert.ToBoolean(subxe.Attributes["IsUpgradePrivate"].Value);
                //property.IsUserEditable = propertyInfo.IsUserEditable;//Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                //property.IsVisibleOnEditor = propertyInfo.IsVisibleOnEditor;//Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                //property.IsVisibleOnViewer = propertyInfo.IsVisibleOnViewer;//Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);
                //property.MaximumShown = propertyInfo.MaximumShown;//Convert.ToInt32(subxe.Attributes["MaximumShown"].Value);

                ////通过Reflector查看源代码，赋值时需要判断这些属性，否则就异常
                //if (property.IsSection || property.AllowPolicyOverride)
                //{
                //    this.UserProfile[propertyName].Property.PrivacyPolicy = (AvePrivacyPolicy)propertyInfo.PrivacyPolicy;//(AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                //}
                //this.UserProfile[propertyName].Property.Separator = (AveMultiValueSeparator)propertyInfo.Separator;//Convert.ToInt32(subxe.Attributes["Separator"].Value);
                //if (property.AllowPolicyOverride && !(property.IsReplicable && propertyInfo.UserOverridePrivacy))
                //{
                //    this.UserProfile[propertyName].Property.UserOverridePrivacy = propertyInfo.UserOverridePrivacy;//Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                //}
                //AvePrivacy privacy = (AvePrivacy)propertyInfo.DefaultPrivacy;//Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                //if (!(property.IsReplicable && (privacy != AvePrivacy.Public)) && !property.AllowPolicyOverride)
                //{
                //    property.DefaultPrivacy = privacy;
                //}
                //try
                //{
                //    property.Commit();
                //}
                //catch (Exception e)
                //{
                //    //mLog.Log(AveLogLevel.WARN, "WP10RTMySite0212", propertyName, e);
                //    mLog.Log(AveLogLevel.WARN, "update property: {0} error:{1}", propertyName, e.ToString());
                //}

                //We can not modify UserProfileValueCollection when Property.IsAdminEditable is not true.
                if (this.UserProfile[propertyName].Property.IsAdminEditable)
                {
                    this.UserProfile[propertyName].Clear();
                    foreach (string value in profileValue.Values)
                    {
                        //10中的属性，如果有多值，是通过‘；’去分隔的，但是在07里面却是用‘，’分隔
                        string newValue = value;
                        //if (this.UserProfile[propertyName].Property.IsMultivalued)
                        //{
                        //    newValue = value.Replace(";", "");
                        //}

                        #region 发现10中多值可以通过‘；’也可以通过‘，’，更改为如下处理
                 
                        if (this.UserProfile[propertyName].Property.IsMultivalued)
                        {
                            if (this.UserProfile[propertyName].Property.Separator.Equals(AveMultiValueSeparator.Semicolon))
                            {
                                newValue = value.Replace(";", "");
                            }
                            else if (this.UserProfile[propertyName].Property.Separator.Equals(AveMultiValueSeparator.Comma))
                            {
                                newValue = value.Replace(",", "");
                            }
                        }
                        #endregion

                        if (this.UserProfile[propertyName].Property.Type.Equals("Person", StringComparison.OrdinalIgnoreCase))
                        {
                            //if (mSite != null && mSite.SPMembers != null)
                            //{
                            //    newValue = mSite.SPMembers.GetMappingUserLogin(value, false, true);
                            //}
                            //TODO: map user
                        }
                        this.UserProfile[propertyName].Add(newValue);
                        this.UserProfile.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperSPServiceResource.RestoreUserProfileDetailsError, e);
            }
        }

        public void RestoreUserProfileProperty(AvePropertyInfo info)
        {
            RestoreUserProfileProperty(info, true);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Property of UserProfile are as attributes in xml.")]
        public void RestoreUserProfileProperty(AvePropertyInfo info, bool isOverwrite)
        {
            IAveOProperty prop = null;
            try
            {
                string propertyName = info.Name;
                if (propertyName.ToLower() == "sps-proxyaddresses" || propertyName.ToLower() == "sps-masteraccountname"
                   || propertyName.ToLower() == "adguid" || propertyName.ToLower() == "quicklinks"
                   || propertyName.ToLower() == "sps-peers" || propertyName.ToLower() == "sps-resourceaccountname"
                   || propertyName.ToLower() == "sps-resourcesid" || propertyName == "UserProfile_GUID"
                   || propertyName == "SID" || propertyName == "AccountName"
                   || propertyName == "UserName" || propertyName == "PersonalSpace")
                {
                    return;
                }
                if (info.IsSection)
                {
                    prop = this.ServiceContext.UserProfileManager.Properties.GetSectionByName(info.Name);
                }
                else
                {
                    prop = this.ServiceContext.UserProfileManager.Properties.GetPropertyByName(info.Name);
                }
                if (prop == null)
                {
                    IAveOPropertyCollection pc = this.ServiceContext.UserProfileManager.Properties;
                    prop = pc.Create(info.IsSection);
                    prop.Name = info.Name;
                    prop.DisplayName = info.DisplayName;
                    prop.Description = info.Description;
                    if (!info.IsSection)
                    {
                        prop.DefaultPrivacy = (AvePrivacy)info.DefaultPrivacy;
                        prop.IsAlias = info.IsAlias;
                        prop.IsColleagueEventLog = info.IsColleagueEventLog;
                        prop.IsMultivalued = info.IsMultivalued;
                        prop.IsReplicable = info.IsReplicable;
                        prop.IsSearchable = info.IsSearchable;
                        prop.IsUpgrade = info.IsUpgrade;
                        prop.IsUpgradePrivate = info.IsUpgradePrivate;
                        prop.IsUserEditable = info.IsUserEditable;
                        prop.IsVisibleOnEditor = info.IsVisibleOnEditor;
                        prop.IsVisibleOnViewer = info.IsVisibleOnViewer;
                        try
                        {
                            prop.Length = info.Length;
                        }
                        catch(Exception e)                      
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Length", info.Length, e);
                        }
                        prop.MaximumShown = info.MaximumShown;
                        prop.Separator = (AveMultiValueSeparator)info.Separator;
                        prop.UserOverridePrivacy = info.UserOverridePrivacy;
                    }
                    try
                    {
                        prop.Type = info.Type;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
                    }
                    try
                    {
                        pc.Add(prop);
                    }
                    catch (Exception e)
                    {
                        log.Warn("Cannot add userProfile property:{0}. exception:{1}", info.Name, e.ToString());
                    }
                }
                else if (!isOverwrite)
                {
                    return;
                }
                else
                {
                    bool changed = false;
                    if (info.Description != prop.Description)
                    {
                        if (!(string.IsNullOrEmpty(info.Description) && string.IsNullOrEmpty(prop.Description)))
                        {
                            prop.Description = info.Description;
                            changed = true;
                        }
                    }
                    if (info.DisplayName != prop.DisplayName)
                    {
                        if (!(string.IsNullOrEmpty(info.DisplayName) && string.IsNullOrEmpty(prop.DisplayName)))
                        {
                            prop.DisplayName = info.DisplayName;
                            changed = true;
                        }
                    }
                    if (!info.IsSection)
                    {
                        if (info.DefaultPrivacy != prop.DefaultPrivacy)
                        {
                            prop.DefaultPrivacy = info.DefaultPrivacy;
                            changed = true;
                        }
                        if (info.IsAlias != prop.IsAlias)
                        {
                            prop.IsAlias = info.IsAlias;
                            changed = true;
                        }
                        if (info.IsColleagueEventLog != prop.IsColleagueEventLog)
                        {
                            prop.IsColleagueEventLog = info.IsColleagueEventLog;
                            changed = true;
                        }
                        if (info.IsReplicable != prop.IsReplicable)
                        {
                            prop.IsReplicable = info.IsReplicable;
                            changed = true;
                        }
                        if (info.IsSearchable != prop.IsSearchable)
                        {
                            prop.IsSearchable = info.IsSearchable;
                            changed = true;
                        }
                        if (info.IsUpgrade != prop.IsUpgrade)
                        {
                            prop.IsUpgrade = info.IsUpgrade;
                            changed = true;
                        }
                        if (info.IsUpgradePrivate != prop.IsUpgradePrivate)
                        {
                            prop.IsUpgradePrivate = info.IsUpgradePrivate;
                            changed = true;
                        }
                        if (info.IsUserEditable != prop.IsUserEditable)
                        {
                            prop.IsUserEditable = info.IsUserEditable;
                            changed = true;
                        }
                        if (info.IsVisibleOnEditor != prop.IsVisibleOnEditor)
                        {
                            prop.IsVisibleOnEditor = info.IsVisibleOnEditor;
                            changed = true;
                        }
                        if (info.IsVisibleOnViewer != prop.IsVisibleOnViewer)
                        {
                            prop.IsVisibleOnViewer = info.IsVisibleOnViewer;
                            changed = true;
                        }
                        if (info.MaximumShown != prop.MaximumShown)
                        {
                            prop.MaximumShown = info.MaximumShown;
                            changed = true;
                        }
                        if (info.PrivacyPolicy != (int)prop.PrivacyPolicy)
                        {
                            prop.PrivacyPolicy = (AvePrivacyPolicy)info.PrivacyPolicy;
                            changed = true;
                        }
                        if (info.Separator != (int)prop.Separator)
                        {
                            prop.Separator = (AveMultiValueSeparator)info.Separator;
                            changed = true;
                        }
                        if (info.UserOverridePrivacy != prop.UserOverridePrivacy)
                        {
                            prop.UserOverridePrivacy = info.UserOverridePrivacy;
                            changed = true;
                        }
                    }
                    try
                    {

                        if (prop.Type != info.Type)
                        {
                            prop.Type = info.Type;
                            changed = true;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
                    }
                    if (changed)
                    {
                        try
                        {
                            prop.Commit();
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while commit  user of profile property:{0}, exception:{1}.", info.Name, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while restore user profile property:{0}, exception:{1}.", info.Name, e.ToString());
            }
        }
    }
}
