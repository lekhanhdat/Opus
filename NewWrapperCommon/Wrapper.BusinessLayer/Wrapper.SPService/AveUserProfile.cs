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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Collections;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.SPService;

namespace AvePoint.Wrapper.SPService
{
    public class AveUserProfile : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //private IAveOUserProfile mUserProfile = null;
        private AveServiceContext mServiceContext = null;
        private string mSourceSiteUrl;

        private static Dictionary<string, ArrayList> mColleagues = new Dictionary<string, ArrayList>();
        private IAveWebApplication mWebApp = null;
        private Dictionary<string, string> mAbsoluteUrlMapping = new Dictionary<string, string>();
        private bool mEnableTag = false;
        private AveSocialComment mSocialComment;
        private AveSocialTag mSocialTag;
        private AveColleague mColleague;
        private AveMembership mMembership;
        private AveQuickLink mQuickLink;
        private AveSocialFeed mSocialFeed;
        private AveSocialFollowing mSocialFollowing;
        private AveSocialRating mSocialRating;
        private AveSiteInfo mSourceSiteInfo;
        private string mDestSiteUrl;
        private bool mUserProfileIsNewCreated;

        private AveSocialComment SocialComment
        {
            get
            {
                if (mSocialComment == null)
                {
                    //mSocialComment = new AveSocialComment(this.ServiceContext);
                    mSocialComment = new AveSocialComment(this);
                }
                return mSocialComment;
            }
        }
        public AveSocialTag SocialTag
        {
            get
            {
                if (mSocialTag == null)
                {
                    //mSocialTag = new AveSocialTag(this.ServiceContext);
                    mSocialTag = new AveSocialTag(this);
                }
                return mSocialTag;
            }
        }
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
        private AveSocialFeed SocialFeed
        {
            get
            {
                if (mSocialFeed == null)
                {
                    mSocialFeed = new AveSocialFeed(this,ServiceContext.OMFactory);
                }
                return mSocialFeed;
            }
        }

        private AveSocialFollowing SocialFollowing
        {
            get
            {
                if (mSocialFollowing == null)
                {
                    mSocialFollowing = new AveSocialFollowing(this,ServiceContext.OMFactory);
                }
                return mSocialFollowing;
            }
        }

        public AveSocialRating SocialRating
        {
            get
            {
                if (mSocialRating == null)
                {
                    mSocialRating = new AveSocialRating(this);
                }
                return mSocialRating;
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
                if (!AveSPUtility.IfServiceAvailable(mWebApp, AveServiceApplicationType.UserProfileService))
                {
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
                mUserProfileIsNewCreated = false;
            }
            else
            {
                userProfile = mServiceContext.UserProfileManager.CreateUserProfile(loginName);
                mUserProfileIsNewCreated = true;
            }
            mServiceContext.AddUserProfileCache(userProfile);
            return userProfile;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "the wrong word is project name")]
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
                if (!String.IsNullOrEmpty(userProfile.SubTypeName))
                {
                    RestoreSubTypeToUserProfile(userProfile.SubTypeName);
                }
                RestoreDetails(userProfile.Properties,userProfile.NeedOverWriteUserProfileDetails);
                this.Colleague.CreateColleagues(userProfile.Colleagues);
                this.SocialComment.Restore(userProfile.Comments);
                this.SocialTag.Restore(userProfile.Tags);
                //do not need to backup and restore memberships,remove for Userprofile performance 
                //this.Membership.CreateMemberships(userProfile.Memberships);
                this.QuickLink.CreateQuickLinks(userProfile.Links);
                if (this.ServiceContext.OMFactory.ContextKind.IsServerMode13Upper()|| this.ServiceContext.OMFactory.ContextKind== AveContextKind.ClientObjectModel)
                {
                    this.SocialFeed.Restore(userProfile.Feeds);
                }
                //Let's keep this code for a while in case we need to backup/restore the followed information separately.
                //this.SocialFollowing.Restore(userProfile.Followed);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "User profile restore in spservice , error: {0}", ex.ToString());
            }
        }

        private void RestoreSubTypeToUserProfile(string subTypeName)
        {
            var subType = GetOrCreateSubType(new AveUserProfileSubTypeInfo()
            {
                Name = subTypeName,
                DisplayName = subTypeName
            });
            //暂时没有找更好办法进行更新
            var profileDisplayName = this.UserProfile.DisplayName;
            this.UserProfile.DisplayName = "in progress";
            this.UserProfile.Commit();
            this.UserProfile.DisplayName = profileDisplayName;
            this.UserProfile.ProfileSubType = subType;
            this.UserProfile.Commit();
        }

        public void RestoreForArchiver(AveUserProfileInfo userProfile)
        {
            try
            {
                string login = this.ServiceContext.GetMappingUser(userProfile.LoginName);
                this.UserProfile = FindOrCreateUserProfile(login);
                this.ServiceContext.LoginName = login;
                this.SocialFeed.RestoreForArchiver(userProfile.Feeds);
                //Let's keep this code for a while in case we need to backup/restore the followed information separately.
                //this.SocialFollowing.Restore(userProfile.Followed);
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

        public void RestoreSocialFeed(List<AveSocialFeedInfo> fInfo)
        {
            this.SocialFeed.Restore(fInfo);
        }

        public void RestoreMembership(AveMembershipInfo mInfo)
        {
            this.Membership.CreateMembership(mInfo);
        }

        public void RestoreTag(AveSocialTagInfo tagInfo)
        {
            this.SocialTag.Restore(tagInfo);
        }

        public void RestoreComment(AveSocialCommentInfo commentInfo)
        {
            this.SocialComment.Restore(commentInfo);
        }

        public void RestoreColleague(AveColleagueInfo colleagueInfo)
        {
            this.Colleague.CreateColleague(colleagueInfo);
        }


        [SuppressMessage("FxCopCustomRules", "100007:SpellCheckStringValues", Justification = "spservice is project name")]
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
                    log.Log(AveLogLevel.WARN, "User profile restore in SPService , error: {0}, login name: {1}, ", ex, profile.LoginName);
                }
            }
        }

        public void RestoreDetails(List<AveUserProfileValueInfo> values,bool needOverWriteDetail)
        {
            if (values == null)
            {
                return;
            }
            foreach (AveUserProfileValueInfo value in values)
            {
                RestoreDetail(value, needOverWriteDetail);
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
                if (propertyName.ToLowerInvariant() == "sps-proxyaddresses" || propertyName.ToLowerInvariant() == "sps-masteraccountname"
                    || propertyName.ToLowerInvariant() == "adguid" || propertyName.ToLowerInvariant() == "quicklinks"
                    || propertyName.ToLowerInvariant() == "sps-peers" || propertyName.ToLowerInvariant() == "sps-resourceaccountname"
                    || propertyName.ToLowerInvariant() == "sps-resourcesid" || propertyName == "UserProfile_GUID"
                    || propertyName == "SID" || propertyName == "AccountName"
                    || propertyName == "UserName" || propertyName == "PersonalSpace"
                    //We need to skip the "SPS-FeedIdentifier" property. Otherwise, the post can't be replied in the destination because of the incorrect identifier id.
                    || propertyName.ToLowerInvariant() == "sps-feedidentifier")
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

                //this.UserProfile[propertyName].Privacy = (AvePrivacy)profileValue.Privacy;
                try
                {
                    this.UserProfile[propertyName].Privacy = (AvePrivacy)profileValue.Privacy;
                }
                catch (Exception ex)
                {
                    //log.Debug(string.Format("Cannot set privacy for property:{0}", propertyName));
                    log.Log(AveLogLevel.DEBUG, string.Format("Cannot set privacy for property:{0}. Exception:{1}", propertyName, ex.ToString()));
                }


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
                //}

                //We can not modify UserProfileValueCollection when Property.IsAdminEditable is not true.
                if (this.UserProfile[propertyName].Property.IsAdminEditable)
                {
                    if (propertyName.Equals("PictureUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        if (this.UserProfile[propertyName].Count == 0)
                        {
                            if (profileValue.Values.Count == 1)
                            {
                                var url = profileValue.Values[0];
                                ReplaceOption replaceOption = new ReplaceOption(true, true); // opetion set to replace AbsoluteUrl and RelativeUrl
                                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                                url = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                this.UserProfile[propertyName].Add(url);
                                this.UserProfile.Commit();
                            }
                        }
                        return;
                    }
                    if (ProfileValueEquals(profileValue.Values, UserProfile[propertyName]))
                    {
                        return;
                    }
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
                            newValue = mServiceContext.GetMappingUser(value);
                        }
                        if (this.UserProfile[propertyName].Property.Type.Equals("Url", StringComparison.OrdinalIgnoreCase))
                        {
                            //http default port
                            string tempUrl = string.Format("{0}:{1}/", SourceSiteInfo.WebAppUrl.TrimEnd('/'), "80");
                            if (newValue.StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                newValue = newValue.Replace(tempUrl, SourceSiteInfo.WebAppUrl);
                            }
                            //https default port
                            tempUrl = string.Format("{0}:{1}/", SourceSiteInfo.WebAppUrl.TrimEnd('/'), "443");
                            if (newValue.StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                newValue = newValue.Replace(tempUrl, SourceSiteInfo.WebAppUrl);
                            }
                            newValue = AveReplaceProcessor.UrlReplace(newValue, AbsoluteUrlMapping, new ReplaceOption(true, true), SourceSiteInfo, DestSiteUrl);
                        }
                        this.UserProfile[propertyName].Add(newValue);
                    }
                    this.UserProfile.Commit();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperSPServiceResource.RestoreUserProfileDetailsError, e);
            }
        }

        private bool ProfileValueEquals(List<string> list, IAveOUserProfileValueCollection values)
        {
            if (list.Count != values.Count)
            {
                return false;
            }
            for (int i = 0; i < list.Count; ++i)
            {
                if (!string.Equals(list[i], ConvertValueAsString(values[i])))
                {
                    return false;
                }
            }
            return true;
        }

        private string ConvertValueAsString(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            IAveTimeZone timeZone = obj as IAveTimeZone;
            if (timeZone != null)
            {
                return timeZone.ID.ToString();
            }
            return obj.ToString();
        }

        public void RestoreUserProfileProperty(AvePropertyInfo info)
        {
            //RestoreUserProfileProperty(info, true);
            RestoreUserProfileProperty(info, true);
        }

        /// <summary>
        /// 创建并还原Core Property的方法，调用这个方法后无需再次Commit。
        /// </summary>
        /// <param name="info">备份的Property Info</param>
        /// <param name="coreProperty">已经完成初始化的IAveOUserProfileCoreProperty对象。</param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Property of UserProfile are as attributes in xml.")]
        private void RestoreUserProfileCoreProperty(AvePropertyInfo info, IAveOUserProfileCoreProperty coreProperty)
        {
            try
            {
                //必须备份的属性：

                coreProperty.Name = info.Name;
                coreProperty.Description = info.Description;
                coreProperty.DisplayName = info.DisplayName;

                if (!info.IsSection)
                {
                    coreProperty.Type = info.Type;
                    //非必要、有默认值的属性：
                    coreProperty.IsAlias = info.IsAlias;
                    coreProperty.IsMultivalued = info.IsMultivalued;
                    coreProperty.IsSearchable = info.IsSearchable;
                    coreProperty.IsUpgrade = info.IsUpgrade;
                    coreProperty.IsUpgradePrivate = info.IsUpgradePrivate;
                    try
                    {
                        coreProperty.MaxLength = info.Length;

                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Length", info.Length, e);
                    }

                    coreProperty.Separator = (AveMultiValueSeparator)info.Separator;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 修改并还原CoreProperty的方法，调用后无需再次commit。
        /// </summary>
        /// <param name="info">备份的Property info</param>
        /// <param name="coreProperty">完成初始化后的IAveOUserProfileCoreProperty对象</param>
        private void RestoreChangedUserProfileCoreProperty(AvePropertyInfo info, IAveOUserProfileCoreProperty coreProperty)
        {
            try
            {
                bool isChanged = false;
                if (coreProperty.Description != info.Description)
                {
                    if (!(String.IsNullOrEmpty(coreProperty.Description) && String.IsNullOrEmpty(info.Description)))
                    {
                        coreProperty.Description = info.Description;
                        isChanged = true;
                    }
                }
                if (coreProperty.DisplayName != info.DisplayName)
                {
                    if (!(String.IsNullOrEmpty(coreProperty.DisplayName) && String.IsNullOrEmpty(info.DisplayName)))
                    {
                        coreProperty.DisplayName = info.DisplayName;
                        isChanged = true;
                    }
                }
                if (!info.IsSection)
                {
                    if (coreProperty.Type != info.Type)
                    {
                        if (!(String.IsNullOrEmpty(coreProperty.Type) && String.IsNullOrEmpty(info.Type)))
                        {
                            coreProperty.Type = info.Type;
                            isChanged = true;
                        }
                    }
                    if (coreProperty.IsAlias != info.IsAlias)
                    {
                        coreProperty.IsAlias = info.IsAlias;
                        isChanged = true;
                    }
                    if (coreProperty.IsSearchable != info.IsSearchable)
                    {
                        coreProperty.IsSearchable = info.IsSearchable;
                        isChanged = true;
                    }
                    if (coreProperty.IsMultivalued != info.IsMultivalued)
                    {
                        coreProperty.IsMultivalued = info.IsMultivalued;
                        isChanged = true;
                    }
                    if (coreProperty.IsUpgrade != info.IsUpgrade)
                    {
                        coreProperty.IsUpgrade = info.IsUpgrade;
                        isChanged = true;
                    }
                    if (coreProperty.IsUpgradePrivate != info.IsUpgradePrivate)
                    {
                        coreProperty.IsUpgradePrivate = info.IsUpgradePrivate;
                        isChanged = true;
                    }
                    if (coreProperty.MaxLength != info.Length)
                    {
                        if (coreProperty.MaxLength != 0 && info.Length != 0)
                        {
                            try
                            {
                                coreProperty.MaxLength = info.Length;
                                isChanged = true;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Length", info.Length, e);
                            }
                        }
                    }
                    if (coreProperty.Separator != (AveMultiValueSeparator)info.Separator)
                    {
                        coreProperty.Separator = (AveMultiValueSeparator)info.Separator;
                        isChanged = false;
                    }
                }
                //发生修改后才会commit。
                if (isChanged)
                {
                    coreProperty.Commit();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 还原新创建的TypeProperty对象，该对象的CoreProperty已在初始化中进行设置。调用后无需再次commit。
        /// </summary>
        /// <param name="info">备份的Property Info数据</param>
        /// <param name="typeProperty">经过初始化后的IAveOUserProfileTypeProperty对象。</param>
        private void RestoreUserProfileTypeProperty(AvePropertyInfo info, IAveOUserProfileTypeProperty typeProperty)
        {
            if (info.IsSection)
            {
                return;
            }
            try
            {
                typeProperty.IsReplicable = info.IsReplicable;
                typeProperty.IsUpgrade = info.IsUpgrade;
                typeProperty.IsUpgradePrivate = info.IsUpgradePrivate;
                typeProperty.IsVisibleOnEditor = info.IsVisibleOnEditor;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 还原已有的TypeProperty对象，该对象的CoreProperty已在初始化中进行设置。调用后无需再次commit。
        /// </summary>
        /// <param name="info">备份的Property Info数据</param>
        /// <param name="typeProperty">经过初始化后的IAveOUserProfileTypeProperty对象。</param>
        private void RestoreChangedUserProfileTypeProperty(AvePropertyInfo info, IAveOUserProfileTypeProperty typeProperty)
        {
            if (info.IsSection)
            {
                return;
            }
            try
            {
                bool isChanged = false;
                if (typeProperty.IsReplicable != info.IsReplicable)
                {
                    typeProperty.IsReplicable = info.IsReplicable;
                    isChanged = true;
                }
                if (typeProperty.IsUpgrade != info.IsUpgrade)
                {
                    typeProperty.IsUpgrade = info.IsUpgrade;
                    isChanged = true;
                }
                if (typeProperty.IsUpgradePrivate = info.IsUpgradePrivate)
                {
                    typeProperty.IsUpgradePrivate = info.IsUpgradePrivate;
                    isChanged = true;
                }
                if (isChanged)
                {
                    typeProperty.Commit();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 还原新创建的SubtypeProperty，其中TypeProperty属性已在初始化中设置。调用后无需再次Commit。
        /// </summary>
        /// <param name="info">备份的Property Info数据</param>
        /// <param name="subtypeProperty">经过初始化后的IAveOUserProfileSubtypeProperty对象</param>
        private void RestoreUserProfileSubtypeProperty(AvePropertyInfo info, IAveOUserProfileSubtypeProperty subtypeProperty)
        {
            if (info.IsSection)
            {
                return;
            }
            try
            {
                if (info.IsSection)
                { return; }
                //subtypeProperty.AllowPolicyOverride = info.AllowPolicyOverride;
                subtypeProperty.DefaultPrivacy = (AvePrivacy)info.DefaultPrivacy;
                subtypeProperty.IsUpgrade = info.IsUpgrade;
                subtypeProperty.isUpgradePribate = info.IsUpgradePrivate;
                subtypeProperty.IsUserEditable = info.IsUserEditable;
                subtypeProperty.PrivacyPolicy = (AvePrivacyPolicy)info.PrivacyPolicy;
                subtypeProperty.UserOverridePrivacy = info.UserOverridePrivacy;

            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 还原已有的SubtypeProperty，其中TypeProperty属性已在初始化中设置。调用后无需再次Commit。
        /// </summary>
        /// <param name="info">备份的Property Info数据</param>
        /// <param name="subtypeProperty">经过初始化后的IAveOUserProfileSubtypeProperty对象</param>
        private void RestoreChangedUserProfileSubtypeProperty(AvePropertyInfo info, IAveOUserProfileSubtypeProperty subtypeProperty)
        {
            if (info.IsSection)
            {
                return;
            }
            try
            {

                bool isChanged = false;
                if (subtypeProperty.DefaultPrivacy != (AvePrivacy)info.DefaultPrivacy)
                {
                    subtypeProperty.DefaultPrivacy = (AvePrivacy)info.DefaultPrivacy;
                    isChanged = true;
                }
                if (subtypeProperty.IsUpgrade != info.IsUpgrade)
                {
                    subtypeProperty.IsUpgrade = info.IsUpgrade;
                    isChanged = true;
                }
                if (subtypeProperty.isUpgradePribate != info.IsUpgradePrivate)
                {
                    subtypeProperty.isUpgradePribate = info.IsUpgradePrivate;
                    isChanged = true;
                }
                if (subtypeProperty.IsUserEditable != info.IsUserEditable)
                {
                    subtypeProperty.IsUserEditable = info.IsUserEditable;
                    isChanged = true;
                }
                if (subtypeProperty.PrivacyPolicy != (AvePrivacyPolicy)info.PrivacyPolicy)
                {
                    subtypeProperty.PrivacyPolicy = (AvePrivacyPolicy)info.PrivacyPolicy;
                    isChanged = true;
                }
                if (subtypeProperty.UserOverridePrivacy != info.UserOverridePrivacy)
                {
                    subtypeProperty.UserOverridePrivacy = info.UserOverridePrivacy;
                    isChanged = true;
                }
                if (isChanged)
                {
                    subtypeProperty.Commit();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
            }
        }
        /// <summary>
        /// 利用ProfileProertyManager，使用CoreProperty、TypeProperty和SubtypeProperty来进行User Profile Property的还原。
        /// </summary>
        /// <param name="info">备份的UserProfile Property数据</param>
        /// <param name="isOverwrite">当属性已存在时，是否覆盖</param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        public void RestoreUserProfileProperty(AvePropertyInfo info, bool isOverwrite)
        {
            IAveOUserProfilePropertyManager propertyManager = null;
            IAveOUserProfileCorePropertyManager corePropertyManager = null;
            IAveOUserProfileTypePropertyManager typePropertyManager = null;
            IAveOUserProfileSubtypePropertyManager subtypePropertyManager = null;
            try
            {
                propertyManager = ServiceContext.PropertyManager;//core、type、subtype的Manager都可以再PropertyManager中实现初始化，故无需调用工厂类。
                corePropertyManager = propertyManager.GetCoreProperties();
                typePropertyManager = propertyManager.GetProfileTypeProperties();
                if (String.IsNullOrEmpty(info.SubtypeName))
                {
                    info.SubtypeName = ServiceContext.SubTypeManager.GetDefaultProfileName(AveProfileType.User);
                }
                subtypePropertyManager = propertyManager.GetProfileSubtypeProperties(info.SubtypeName);
                String propertyName = info.Name;
                if (propertyName.ToLowerInvariant() == "sps-proxyaddresses" || propertyName.ToLowerInvariant() == "sps-masteraccountname"
                   || propertyName.ToLowerInvariant() == "adguid" || propertyName.ToLowerInvariant() == "quicklinks"
                   || propertyName.ToLowerInvariant() == "sps-peers" || propertyName.ToLowerInvariant() == "sps-resourceaccountname"
                   || propertyName.ToLowerInvariant() == "sps-resourcesid" || propertyName == "UserProfile_GUID"
                   || propertyName == "SID" || propertyName == "AccountName"
                   || propertyName == "UserName" || propertyName == "PersonalSpace")
                {
                    return;
                }

                //在WebApp中查找是否存在这个Property


                var coreProperty = info.IsSection ? corePropertyManager.GetSectionPropertyByName(info.Name)
                                                  : corePropertyManager.GetCorePropertyByName(info.Name);
                if (coreProperty == null)
                {
                    coreProperty = corePropertyManager.Create(info.IsSection);
                    this.RestoreUserProfileCoreProperty(info, coreProperty);
                    corePropertyManager.Add(coreProperty);
                    coreProperty.Commit();
                }
                else if (isOverwrite)
                {
                    this.RestoreChangedUserProfileCoreProperty(info, coreProperty);
                }
                

                var typeProperty = info.IsSection ? typePropertyManager.GetSectionPropertyByName(info.Name)
                                                  : typePropertyManager.GetTypePropertyByName(info.Name);
                if (typeProperty == null)
                {
                    typeProperty = typePropertyManager.Create(coreProperty);
                    this.RestoreUserProfileTypeProperty(info, typeProperty);
                    typePropertyManager.Add(typeProperty);
                    typeProperty.Commit();
                }
                else if (isOverwrite)
                {
                    this.RestoreUserProfileTypeProperty(info, typeProperty);
                }
                

                var subtypeProperty = info.IsSection ? subtypePropertyManager.GetSectionPropertyByName(info.Name)
                                                     : subtypePropertyManager.GetSubtypePropertyByName(info.Name);
                if (subtypeProperty == null)
                {
                    subtypeProperty = subtypePropertyManager.Create(typeProperty);
                    this.RestoreUserProfileSubtypeProperty(info, subtypeProperty);
                    subtypePropertyManager.Add(subtypeProperty);
                    subtypeProperty.Commit();
                }
                else if (isOverwrite)
                {
                    this.RestoreChangedUserProfileSubtypeProperty(info, subtypeProperty);                    
                }
                if(isOverwrite)
                {
                    subtypePropertyManager.SetDisplayOrderByName(info.Name, info.IsSection, info.DisplayOrder);
                    subtypePropertyManager.CommitDisplayOrder();
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while restore user profile property:{0}, exception:{1}.", info.Name, e.ToString());
            }
        }

        //[SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        //public void RestoreUserProfileProperty(AvePropertyInfo info, bool isOverwrite)
        //{
        //    IAveOProperty prop = null;
        //    try
        //    {
        //        string propertyName = info.Name;
        //        if (propertyName.ToLowerInvariant() == "sps-proxyaddresses" || propertyName.ToLowerInvariant() == "sps-masteraccountname"
        //           || propertyName.ToLowerInvariant() == "adguid" || propertyName.ToLowerInvariant() == "quicklinks"
        //           || propertyName.ToLowerInvariant() == "sps-peers" || propertyName.ToLowerInvariant() == "sps-resourceaccountname"
        //           || propertyName.ToLowerInvariant() == "sps-resourcesid" || propertyName == "UserProfile_GUID"
        //           || propertyName == "SID" || propertyName == "AccountName"
        //           || propertyName == "UserName" || propertyName == "PersonalSpace")
        //        {
        //            return;
        //        }
        //        if (info.IsSection)
        //        {
        //            prop = this.ServiceContext.UserProfileManager.Properties.GetSectionByName(info.Name);
        //        }
        //        else
        //        {
        //            prop = this.ServiceContext.UserProfileManager.Properties.GetPropertyByName(info.Name);
        //        }
        //        if (prop == null)
        //        {
        //            IAveOPropertyCollection pc = this.ServiceContext.UserProfileManager.Properties;
        //            IAveOPropertyCollection ps = this.ServiceContext.UserProfileManager.PropertiesWithSection;
        //            if (info.IsSection)
        //            {
        //                prop = ps.Create(true);
        //            }
        //            else
        //            {
        //                prop = pc.Create(false);
        //            }
        //            prop.Name = info.Name;
        //            prop.DisplayName = info.DisplayName;
        //            prop.Description = info.Description;
        //            if (!info.IsSection)
        //            {
        //                prop.DefaultPrivacy = (AvePrivacy)info.DefaultPrivacy;
        //                prop.IsAlias = info.IsAlias;
        //                prop.IsColleagueEventLog = info.IsColleagueEventLog;
        //                prop.IsMultivalued = info.IsMultivalued;
        //                prop.IsReplicable = info.IsReplicable;
        //                prop.IsSearchable = info.IsSearchable;
        //                prop.IsUpgrade = info.IsUpgrade;
        //                prop.IsUpgradePrivate = info.IsUpgradePrivate;
        //                prop.IsUserEditable = info.IsUserEditable;
        //                prop.IsVisibleOnEditor = info.IsVisibleOnEditor;
        //                prop.IsVisibleOnViewer = info.IsVisibleOnViewer;
        //                prop.PrivacyPolicy = (AvePrivacyPolicy)info.PrivacyPolicy;
        //                try
        //                {
        //                    prop.Length = info.Length;
        //                }
        //                catch (Exception e)
        //                {
        //                    log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Length", info.Length, e);
        //                }
        //                prop.MaximumShown = info.MaximumShown;
        //                prop.Separator = (AveMultiValueSeparator)info.Separator;
        //                prop.UserOverridePrivacy = info.UserOverridePrivacy;
        //            }
        //            try
        //            {
        //                prop.Type = info.Type;
        //            }
        //            catch (Exception e)
        //            {
        //                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
        //            }
        //            try
        //            {
        //                if (info.IsSection)
        //                {
        //                    ps.Add(prop);
        //                }
        //                else
        //                {
        //                    pc.Add(prop);
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                log.Warn("Cannot add userProfile property:{0}. exception:{1}", info.Name, e.ToString());
        //            }
        //        }
        //        else if (!isOverwrite)
        //        {
        //            return;
        //        }
        //        else
        //        {
        //            bool changed = false;
        //            if (info.Description != prop.Description)
        //            {
        //                if (!(string.IsNullOrEmpty(info.Description) && string.IsNullOrEmpty(prop.Description)))
        //                {
        //                    prop.Description = info.Description;
        //                    changed = true;
        //                }
        //            }
        //            if (info.DisplayName != prop.DisplayName)
        //            {
        //                if (!(string.IsNullOrEmpty(info.DisplayName) && string.IsNullOrEmpty(prop.DisplayName)))
        //                {
        //                    prop.DisplayName = info.DisplayName;
        //                    changed = true;
        //                }
        //            }
        //            if (!info.IsSection)
        //            {
        //                if (info.DefaultPrivacy != (int)prop.DefaultPrivacy)
        //                {
        //                    prop.DefaultPrivacy = (AvePrivacy)info.DefaultPrivacy;
        //                    changed = true;
        //                }
        //                if (info.IsAlias != prop.IsAlias)
        //                {
        //                    prop.IsAlias = info.IsAlias;
        //                    changed = true;
        //                }
        //                if (info.IsColleagueEventLog != prop.IsColleagueEventLog)
        //                {
        //                    prop.IsColleagueEventLog = info.IsColleagueEventLog;
        //                    changed = true;
        //                }
        //                if (info.IsReplicable != prop.IsReplicable)
        //                {
        //                    prop.IsReplicable = info.IsReplicable;
        //                    changed = true;
        //                }
        //                if (info.IsSearchable != prop.IsSearchable)
        //                {
        //                    prop.IsSearchable = info.IsSearchable;
        //                    changed = true;
        //                }
        //                if (info.IsUpgrade != prop.IsUpgrade)
        //                {
        //                    prop.IsUpgrade = info.IsUpgrade;
        //                    changed = true;
        //                }
        //                if (info.IsUpgradePrivate != prop.IsUpgradePrivate)
        //                {
        //                    prop.IsUpgradePrivate = info.IsUpgradePrivate;
        //                    changed = true;
        //                }
        //                if (info.IsUserEditable != prop.IsUserEditable)
        //                {
        //                    prop.IsUserEditable = info.IsUserEditable;
        //                    changed = true;
        //                }
        //                if (info.IsVisibleOnEditor != prop.IsVisibleOnEditor)
        //                {
        //                    prop.IsVisibleOnEditor = info.IsVisibleOnEditor;
        //                    changed = true;
        //                }
        //                if (info.IsVisibleOnViewer != prop.IsVisibleOnViewer)
        //                {
        //                    prop.IsVisibleOnViewer = info.IsVisibleOnViewer;
        //                    changed = true;
        //                }
        //                if (info.MaximumShown != prop.MaximumShown)
        //                {
        //                    prop.MaximumShown = info.MaximumShown;
        //                    changed = true;
        //                }
        //                if (info.PrivacyPolicy != (int)prop.PrivacyPolicy)
        //                {
        //                    prop.PrivacyPolicy = (AvePrivacyPolicy)info.PrivacyPolicy;
        //                    changed = true;
        //                }
        //                if (info.Separator != (int)prop.Separator)
        //                {
        //                    prop.Separator = (AveMultiValueSeparator)info.Separator;
        //                    changed = true;
        //                }
        //                if (info.UserOverridePrivacy != prop.UserOverridePrivacy)
        //                {
        //                    prop.UserOverridePrivacy = info.UserOverridePrivacy;
        //                    changed = true;
        //                }
        //            }
        //            try
        //            {

        //                if (prop.Type != info.Type)
        //                {
        //                    prop.Type = info.Type;
        //                    changed = true;
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.SetUserProfilePropertyError, "Type", info.Type, e);
        //            }
        //            if (changed)
        //            {
        //                try
        //                {
        //                    prop.Commit();
        //                }
        //                catch (Exception e)
        //                {
        //                    log.Warn("An error occurred while commit  user of profile property:{0}, exception:{1}.", info.Name, e.ToString());
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Warn("An error occurred while restore user profile property:{0}, exception:{1}.", info.Name, e.ToString());
        //    }
        //}
        public void Dispose()
        {
            if (mSocialFeed != null)
            {
                mSocialFeed.Dispose();
                mSocialFeed = null;
            }

            if (mSocialTag != null)
            {
                mSocialTag.Dispose();
                mSocialTag = null;
            }

            if (mSocialComment != null)
            {
                mSocialComment.Dispose();
                mSocialComment = null;
            }

            if (mSocialFollowing != null)
            {
                mSocialFollowing.Dispose();
                mSocialFollowing = null;
            }
        }

        //目前无需覆盖sub type DisplayName
        public void RestoreUserProfileSubTypes(List<AveUserProfileSubTypeInfo> subTypes)
        {
            foreach (var subType in subTypes)
            {
                GetOrCreateSubType(subType);
            }
        }

        private IAveOProfileSubtype GetOrCreateSubType(AveUserProfileSubTypeInfo subTypeInfo)
        {
            IAveOProfileSubtype subType = null;
            try
            {
                if ((subType = this.ServiceContext.SubTypeManager.GetProfileSubtype(subTypeInfo.Name)) == null)
                {
                    subType = this.ServiceContext.SubTypeManager.CreateSubtype(subTypeInfo.Name, subTypeInfo.DisplayName, AveProfileType.User);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while restoring subType. Name:{0},DisplayName{1},Error{2}", subTypeInfo.Name, subTypeInfo.DisplayName, e.ToString());
            }
            return subType;
        }
    }
}
