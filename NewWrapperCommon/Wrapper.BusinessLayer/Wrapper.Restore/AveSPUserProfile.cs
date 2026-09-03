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
using AvePoint.Wrapper.SPService;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPUserProfile : RestoreableObject, AvePoint.Wrapper.Restore.IAveSPUserProfile, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveOUserProfile mUserProfile = null;
        private IAveServiceContext mContext = null;
        private static AveVolatileCache<string, ArrayList> mColleagues = new AveVolatileCache<string, ArrayList>();
        private string mWorkingListName;
        private IAveOUserProfileManager mUserProfileManager = null;
        private IAveWebApplication mWebApp = null;
        private uint mDestLCID;
        private Hashtable mAbsoluteUrlMapping = new Hashtable();
        private AveSPSite mSite;
        private bool mEnableTag = false;
        private AveObjectModelFactory mOMFactory;
        private AveContextKind mContextKind;
        private bool mExistSkip = false;
        private IAveUserProfileSerializer userProfileSerializer;

        private bool OverWrite
        {
            get;
            set;
        }

        public bool ExistSkip
        {
            set { mExistSkip = value; }
        }

        private bool NeedSkip
        {
            get
            {
                if (mExistSkip && mAveProfile != null && !mAveProfile.IsUserProfileNewCreated)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 设为True，就会在Restore User Profile的时候还原其中的Tag，否则不还原。
        /// CM, DM, Replicator使用
        /// </summary>
        public bool EnableTag
        {
            set { mEnableTag = value; }
        }

        public AveSPSite Site
        {
            get { return mSite; }
            set { mSite = value; }
        }

        public Hashtable AbsoluteUrlMapping
        {
            get { return mAbsoluteUrlMapping; }
            set { mAbsoluteUrlMapping = value; }
        }

        public AveSPUserProfile(AveSPSite _aveSite)
            : this(_aveSite, _aveSite.SPSite.Owner.LoginName, true)
        { }

        public AveSPUserProfile(AveSPSite _aveSite, string loginName)
            : this(_aveSite, loginName, true)
        { }

        public AveSPUserProfile(AveSPSite _aveSite, uint destLCID)
            : this(_aveSite, string.Empty, false)
        {
            mDestLCID = destLCID;
        }

        public AveSPUserProfile(IAveWebApplication webApp, uint destLCID, AveContextKind contextKind)
        {
            mContextKind = contextKind;
            mOMFactory = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), mContextKind);
            IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = mOMFactory.CreateSiteSubscriptionIdentifier();
            IAveServiceContext context = mOMFactory.CreateServiceContext();
            mContext = context.GetContext(webApp.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
            IAveOUserProfileManager userProfileManager = mOMFactory.CreateUserProfileManager(mContext);
            mWebApp = webApp;
            mDestLCID = destLCID;
        }

        internal AveSPUserProfile(AveSPSite _aveSite, string loginName, bool needInit)
        {
            mSite = _aveSite;
            mWebApp = mSite.SPSite.WebApplication;
            IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = mSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier();
            IAveServiceContext context = mSite.ObjectModelFactory.CreateServiceContext();
            mContext = context.GetContext(mWebApp.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
            mUserProfileManager = mSite.ObjectModelFactory.CreateUserProfileManager(mContext);
            if (needInit)
            {
                if (mUserProfileManager.UserExists(loginName))
                {
                    mUserProfile = mUserProfileManager.GetUserProfile(loginName);
                }
                else
                {
                    mUserProfile = mUserProfileManager.CreateUserProfile(loginName);
                }
            }
            mAveProfile = new AveUserProfile(_aveSite.ServiceContext, loginName, needInit, _aveSite.SourceSiteInfo, _aveSite.ServerRelativeUrl);
            userProfileSerializer = mSite.ObjectModelFactory.CreateUserProfileSerializer(mSite.SPSite, loginName, needInit, mSite.SourceSiteInfo, _aveSite.SPMembers.GetMappingUserLogin);
        }


        public bool CheckServiceAvailable()
        {
            bool ifAvailable = true;
            try
            {
                if (!AveSPUtility.IfServiceAvailable(mWebApp, AveServiceApplicationType.UserProfileService))
                {
                    //mLog.Log(AveLogLevel.ERROR, string.Format("There is no User Profile Service associate with the web application: {0}", mWebApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri));
                    log.Error("There is no User Profile Service associate with the web application: {0}", mWebApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString());
                    ifAvailable = false;
                }
            }
            catch (Exception ex)
            {
                ifAvailable = false;
                log.Log(AveLogLevel.ERROR, "An error occurred while CheckServiceAvailable {0}", ex.ToString());
            }
            return ifAvailable;
        }

        public IAveOUserProfile FindOrCreatePersonalSite(string loginName)
        {
            IAveOUserProfile userProfile = null;
            if (mUserProfileManager.UserExists(loginName))
            {
                userProfile = mUserProfileManager.GetUserProfile(loginName);
            }
            else
            {
                userProfile = mUserProfileManager.CreateUserProfile(loginName);
            }
            return userProfile;
        }

        //public void Restore(string xml)
        //{
        //    XmlDocument xDoc = new XmlDocument();
        //    xDoc.LoadXml(xml);
        //    string mWorkingListName = null;
        //    foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
        //    {
        //        mWorkingListName = node.Name;
        //        switch (mWorkingListName)
        //        {
        //            case "<Colleague>":
        //                UpdateColleages(xml);
        //                break;
        //            case "<Detail>":
        //                UpdateDetails(xml);
        //                break;
        //            case "<Memberships>":
        //                UpdateMemberships(xml);
        //                break;
        //            case "<Notes>":
        //                UpdateNotes(xml);
        //                break;
        //            case "<Tags>":
        //                UpdateTags(xml);
        //                break;
        //            default:
        //                throw new Exception("Invalid profile list name: " + mWorkingListName);
        //        }
        //    }
        //}

        public void Restore(Dictionary<string, string> userProfileLists)
        {
            foreach (KeyValuePair<string, string> pair in userProfileLists)
            {
                try
                {
                    switch (pair.Key)
                    {
                        case AveConstants.MY_COLLEAGUES:
                            UpdateUserProfileColleages(pair.Value);
                            break;
                        case AveConstants.MY_DETAILS:
                            UpdateUserProfileDetails(pair.Value);
                            break;
                        case AveConstants.MY_MEMBERSHIPS:
                            UpdateUserProfileMemberships(pair.Value);
                            break;
                        case AveConstants.MY_NOTES:
                            UpdateUserProfileNotes(pair.Value);
                            break;
                        case AveConstants.MY_TAGS:
                            UpdateUserProfileTags(pair.Value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while restore user profile , error: {0}", ex.ToString());
                }
            }
        }

        public void RestoreUserProfile(List<Dictionary<string, Dictionary<string, string>>> userProfiles)
        {
            if (!CheckServiceAvailable())
            {
                return;
            }

            #region --Output Controller--
            log.Info("The status of restoring social tags is :" + mEnableTag);
            #endregion

            Dictionary<string, Dictionary<string, string>> collection = new Dictionary<string, Dictionary<string, string>>();
            if (userProfiles.Count > 0)
            {
                collection = userProfiles[0];
            }
            foreach (KeyValuePair<string, Dictionary<string, string>> pair in collection)
            {
                try
                {
                    mUserProfile = FindOrCreatePersonalSite(pair.Key);
                    this.Restore(pair.Value);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred while restore user profile, error: {0} , key:{1}", ex, pair.Key);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Property of UserProfile are as attributes in xml.")]
        public void UpdateDetails(string xml)
        {

            using (AvePerformanceScope pcs = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateDetails"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlElement xe = (XmlElement)xDoc.FirstChild;
                IAveOUserProfileManager upm = mUserProfile.ProfileManager;
                using (IAveSite site = mUserProfile.PersonalSite)
                {
                    try
                    {
                        string propertyName = xe.Attributes["NameValue"].Value;

                        //If we know the properties list below won't be restored, why we backup them?
                        if (propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-proxyaddresses"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-masteraccountname"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "adguid"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "quicklinks"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-peers"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-resourceaccountname"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-resourcesid"
                            || propertyName == "UserProfile_GUID"
                            || propertyName == "SID"
                            || propertyName == "AccountName"
                            || propertyName == "FirstName"
                            || propertyName == "PreferredName"
                            || propertyName == "WorkEmail"
                            || propertyName == "UserName"
                            || propertyName == "PersonalSpace")
                            return;

                        if (upm.Properties.GetPropertyByName(propertyName) == null)
                        {
                            IAveOPropertyCollection pc = upm.Properties;
                            IAveOProperty p = pc.Create(false);
                            XmlElement subxe = (XmlElement)xe.ChildNodes[0];
                            p.Name = propertyName;
                            p.DisplayName = subxe.Attributes["DisplayName"].Value;
                            p.Type = subxe.Attributes["Type"].Value;
                            p.Length = Convert.ToInt32(subxe.Attributes["Length"].Value);
                            p.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            p.DefaultPrivacy = (AvePrivacy)Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                            p.IsMultivalued = Convert.ToBoolean(subxe.Attributes["IsMultivalued"].Value);
                            p.IsUserEditable = Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                            p.IsVisibleOnEditor = Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                            p.IsVisibleOnViewer = Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);

                            try
                            {
                                pc.Add(p);
                            }
                            catch (Exception e)
                            {
                                //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot add property. propertyName:{0}\n error message:{1}", p.Name, e));
                                log.Log(AveLogLevel.ERROR, "Cannot add property: {0}, error:{1}", p.Name, e.ToString());
                                return;
                            }
                        }
                        else if (RestoreOption.mAveRestoreMode != AveRestoreMode.OverWrite)
                        {
                            return;
                        }
                        int capacityValue = Convert.ToInt32(xe.Attributes["Capacity"].Value);
                        //We can not modify the value of Capacity to 0 when Capacity is not 0.
                        if (capacityValue > 0 || mUserProfile[propertyName].Capacity == 0)
                        {
                            mUserProfile[propertyName].Capacity = Convert.ToInt32(xe.Attributes["Capacity"].Value);
                        }
                        if (mUserProfile[propertyName].ProfileSubtypeProperty != null && mUserProfile[propertyName].Property.UserOverridePrivacy)
                        {
                            mUserProfile[propertyName].Privacy = (AvePrivacy)Convert.ToInt32(xe.Attributes["Privacy"].Value);
                        }

                        XmlNodeList xnf1 = xe.ChildNodes;
                        foreach (XmlNode xn2 in xnf1)
                        {
                            XmlElement subxe = (XmlElement)xn2;
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

                            IAveOProperty property = mUserProfile[propertyName].Property;
                            property.Description = subxe.Attributes["Description"].Value;
                            property.DisplayName = subxe.Attributes["DisplayName"].Value;
                            property.IsAlias = Convert.ToBoolean(subxe.Attributes["IsAlias"].Value);
                            property.IsColleagueEventLog = Convert.ToBoolean(subxe.Attributes["IsColleagueEventLog"].Value);
                            property.IsReplicable = Convert.ToBoolean(subxe.Attributes["IsReplicable"].Value);
                            property.IsSearchable = Convert.ToBoolean(subxe.Attributes["IsSearchable"].Value);
                            property.IsUpgrade = Convert.ToBoolean(subxe.Attributes["IsUpgrade"].Value);
                            property.IsUpgradePrivate = Convert.ToBoolean(subxe.Attributes["IsUpgradePrivate"].Value);
                            property.IsUserEditable = Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                            property.IsVisibleOnEditor = Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                            property.IsVisibleOnViewer = Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);
                            property.MaximumShown = Convert.ToInt32(subxe.Attributes["MaximumShown"].Value);

                            //通过Reflector查看源代码，赋值时需要判断这些属性，否则就异常
                            if (property.IsSection || property.AllowPolicyOverride)
                            {
                                mUserProfile[propertyName].Property.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            }
                            mUserProfile[propertyName].Property.Separator = (AveMultiValueSeparator)Convert.ToInt32(subxe.Attributes["Separator"].Value);
                            if (property.AllowPolicyOverride && !(property.IsReplicable && Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value)))
                            {
                                mUserProfile[propertyName].Property.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                            }
                            AvePrivacy privacy = (AvePrivacy)Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                            if (!(property.IsReplicable && (privacy != AvePrivacy.Public)) && !property.AllowPolicyOverride)
                            {
                                property.DefaultPrivacy = privacy;
                            }
                            try
                            {
                                property.Commit();
                            }
                            catch (Exception e)
                            {
                                //mLog.Log(AveLogLevel.WARN, string.Format("Update propery error. propertyName:{0}\n error message:{1}", propertyName, e));
                                log.Log(AveLogLevel.WARN, "update property: {0} error:{1}", propertyName, e.ToString());
                            }
                        }
                        //We can not modify UserProfileValueCollection when Property.IsAdminEditable is not true.
                        if (mUserProfile[propertyName].Property.IsAdminEditable)
                        {
                            mUserProfile[propertyName].Clear();
                            for (int i = 0; i < Convert.ToInt32(xe.Attributes["Count"].Value); i++)
                            {
                                //mUserProfile[propertyName].Add(xe.Attributes["Value" + i.ToString()].Value);

                                //10中的属性，如果有多值，是通过‘；’去分隔的，但是在07里面却是用‘，’分隔
                                string value = xe.Attributes["Value" + i.ToString()].Value;
                                if (mUserProfile[propertyName].Property.IsMultivalued)
                                {
                                    value = value.Replace(";", "");
                                }
                                if (mUserProfile[propertyName].Property.Type.Equals("Person", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (mSite != null && mSite.SPMembers != null)
                                    {
                                        value = mSite.SPMembers.GetMappingUserLogin(value, false, true);
                                    }
                                }
                                if (!string.IsNullOrEmpty(value))
                                {
                                    mUserProfile[propertyName].Add(value);
                                }
                            }
                        }

                        if (propertyName == "PictureURL" && mUserProfile[propertyName].Value != null)
                        {
                            string value = mUserProfile[propertyName].Value.ToString();

                            ReplaceOption replaceOption = new ReplaceOption(true, true); // opetion set to replace AbsoluteUrl and RelativeUrl
                            AveSiteMappingManager siteMappingManager = mSite.MappingManager.SiteMappingManager;
                            value = AveReplaceProcessor.UrlReplace(value, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);

                            int index = value.IndexOf(@"/Shared%20Pictures/Profile%20Pictures", StringComparison.OrdinalIgnoreCase);
                            if (index < 0)
                            {
                                index = value.IndexOf(@"/Shared Pictures/Profile Pictures", StringComparison.OrdinalIgnoreCase);
                            }
                            if (index > 0)
                            {
                                string tempStr = value.Substring(index);
                                value = site.Url + tempStr;
                                value = value.Replace("%20", " ");
                                mUserProfile[propertyName].Value = (object)value;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update detail. \n error message:{0}", e));
                        log.Log(AveLogLevel.WARN, "Error: {0}", e.ToString());
                    }
                }
                try
                {
                    mUserProfile.Commit();
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Update user profile property error. \n error message:{0}", e));
                    log.Log(AveLogLevel.WARN, "Update User profile property error: {0}", e.ToString());
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Property of UserProfile are as attributes in xml.")]
        public void UpdateUserProfileDetails(string xml)
        {

            using (AvePerformanceScope pcs = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateUserProfileDetails"))
            {


                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                IAveOUserProfileManager upm = mUserProfile.ProfileManager;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    XmlElement xe = (XmlElement)node;
                    try
                    {
                        string propertyName = xe.Attributes["NameValue"].Value;

                        //If we know the properties list below won't be restored, why we backup them?
                        if (propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-proxyaddresses"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-masteraccountname"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "adguid"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "quicklinks"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-peers"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-resourceaccountname"
                            || propertyName.ToLower(CultureInfo.InvariantCulture) == "sps-resourcesid"
                            || propertyName == "UserProfile_GUID"
                            || propertyName == "SID"
                            || propertyName == "AccountName"
                            || propertyName == "UserName"
                            || propertyName == "PersonalSpace")
                        {
                            continue;
                        }
                        if (propertyName.Equals("WorkEmail", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("FirstName", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("PreferredName", StringComparison.OrdinalIgnoreCase))
                        { }
                        if (upm.Properties.GetPropertyByName(propertyName) == null)
                        {
                            IAveOPropertyCollection pc = upm.Properties;
                            IAveOProperty p = pc.Create(false);
                            XmlElement subxe = (XmlElement)xe.ChildNodes[0];
                            p.Name = propertyName;
                            p.DisplayName = subxe.Attributes["DisplayName"].Value;
                            p.Type = subxe.Attributes["Type"].Value;
                            p.Length = Convert.ToInt32(subxe.Attributes["Length"].Value);
                            p.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            p.DefaultPrivacy = (AvePrivacy)Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                            p.IsMultivalued = Convert.ToBoolean(subxe.Attributes["IsMultivalued"].Value);
                            p.IsUserEditable = Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                            p.IsVisibleOnEditor = Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                            p.IsVisibleOnViewer = Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);

                            try
                            {
                                pc.Add(p);
                            }
                            catch (Exception e)
                            {
                                //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot add property. propertyName:{0}\n error message:{1}", p.Name, e));
                                log.Log(AveLogLevel.ERROR, "Cannot add property: {0}, error:{1}", p.Name, e.ToString());
                                continue;
                            }
                        }
                        else if (RestoreOption.mAveRestoreMode != AveRestoreMode.OverWrite)
                        {
                            continue;
                        }
                        int capacityValue = Convert.ToInt32(xe.Attributes["Capacity"].Value);
                        //We can not modify the value of Capacity to 0 when Capacity is not 0.
                        if (capacityValue > 0 || mUserProfile[propertyName].Capacity == 0)
                        {
                            mUserProfile[propertyName].Capacity = Convert.ToInt32(xe.Attributes["Capacity"].Value);
                        }

                        mUserProfile[propertyName].Privacy = (AvePrivacy)Convert.ToInt32(xe.Attributes["Privacy"].Value);

                        XmlNodeList xnf1 = xe.ChildNodes;
                        foreach (XmlNode xn2 in xnf1)
                        {
                            XmlElement subxe = (XmlElement)xn2;
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
                            IAveOProperty property = mUserProfile[propertyName].Property;
                            property.Description = subxe.Attributes["Description"].Value;
                            property.DisplayName = subxe.Attributes["DisplayName"].Value;
                            property.IsAlias = Convert.ToBoolean(subxe.Attributes["IsAlias"].Value);
                            property.IsColleagueEventLog = Convert.ToBoolean(subxe.Attributes["IsColleagueEventLog"].Value);
                            property.IsReplicable = Convert.ToBoolean(subxe.Attributes["IsReplicable"].Value);
                            property.IsSearchable = Convert.ToBoolean(subxe.Attributes["IsSearchable"].Value);
                            property.IsUpgrade = Convert.ToBoolean(subxe.Attributes["IsUpgrade"].Value);
                            property.IsUpgradePrivate = Convert.ToBoolean(subxe.Attributes["IsUpgradePrivate"].Value);
                            property.IsUserEditable = Convert.ToBoolean(subxe.Attributes["IsUserEditable"].Value);
                            property.IsVisibleOnEditor = Convert.ToBoolean(subxe.Attributes["IsVisibleOnEditor"].Value);
                            property.IsVisibleOnViewer = Convert.ToBoolean(subxe.Attributes["IsVisibleOnViewer"].Value);
                            property.MaximumShown = Convert.ToInt32(subxe.Attributes["MaximumShown"].Value);

                            //通过Reflector查看源代码，赋值时需要判断这些属性，否则就异常
                            if (property.IsSection || property.AllowPolicyOverride)
                            {
                                mUserProfile[propertyName].Property.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            }
                            mUserProfile[propertyName].Property.Separator = (AveMultiValueSeparator)Convert.ToInt32(subxe.Attributes["Separator"].Value);
                            if (property.AllowPolicyOverride && !(property.IsReplicable && Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value)))
                            {
                                mUserProfile[propertyName].Property.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                            }
                            AvePrivacy privacy = (AvePrivacy)Convert.ToInt32(subxe.Attributes["DefaultPrivacy"].Value);
                            if (!(property.IsReplicable && (privacy != AvePrivacy.Public)) && !property.AllowPolicyOverride)
                            {
                                property.DefaultPrivacy = privacy;
                            }
                            try
                            {
                                property.Commit();
                            }
                            catch (Exception e)
                            {
                                //mLog.Log(AveLogLevel.WARN, string.Format("Update propery error. propertyName:{0}\n error message:{1}", propertyName, e));
                                log.Log(AveLogLevel.WARN, "update property: {0} error:{1}", propertyName, e.ToString());
                            }
                        }
                        //We can not modify UserProfileValueCollection when Property.IsAdminEditable is not true.
                        if (mUserProfile[propertyName].Property.IsAdminEditable)
                        {
                            mUserProfile[propertyName].Clear();
                            for (int i = 0; i < Convert.ToInt32(xe.Attributes["Count"].Value); i++)
                            {
                                mUserProfile[propertyName].Add(xe.Attributes["Value" + i.ToString()].Value);
                            }
                        }
                        //Just Profile ,no my site.So can not change the picture url

                        //if (propertyName == "PictureURL" && mUserProfile[propertyName].Value != null)
                        //{
                        //    string value = mUserProfile[propertyName].Value.ToString();
                        //    int index = value.IndexOf(@"/Shared%20Pictures/Profile%20Pictures");
                        //    if (index < 0)
                        //    {
                        //        index = value.IndexOf(@"/Shared Pictures/Profile Pictures");
                        //    }
                        //    if (index > 0)
                        //    {
                        //        string tempStr = value.Substring(index);
                        //        value = mUserProfile.PersonalSite.Url + tempStr;
                        //        value = value.Replace("%20", " ");
                        //        mUserProfile[propertyName].Value = (object)value;
                        //    }
                        //}
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while update detail. \n error message:{0}", e));
                        log.Log(AveLogLevel.WARN, "Error: {0}", e.ToString());
                    }
                }
                try
                {
                    mUserProfile.Commit();
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Update user profile property error. \n error message:{0}", e));
                    log.Log(AveLogLevel.WARN, "Update User profile property error: {0}", e.ToString());
                }


            }

        }

        public void UpdateMemberships(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateMemberships"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlElement xe = (XmlElement)xDoc.FirstChild;
                IAveOMembership membership = null;
                IAveOUserProfileManager profileManager = mUserProfile.ProfileManager;

                try
                {
                    XmlElement subxe = (XmlElement)xe.ChildNodes[0];
                    XmlElement subxe1 = (XmlElement)xe.ChildNodes[1];
                    Guid source = new Guid(subxe1.Attributes["SourceInternal"].Value);
                    string displayName = subxe1.Attributes["DisplayName"].Value;
                    string mailNickName = subxe1.Attributes["MailNickName"].Value;
                    string description = subxe1.Attributes["Description"].Value;
                    string url = subxe1.Attributes["Url"].Value;
                    string sourceReference = subxe1.Attributes["SourceReference"].Value;
                    IAveOMemberGroup memberGroup = null;
                    try
                    {
                        memberGroup = profileManager.GetMemberGroups().GetMemberGroupBySourceAndSourceReference(source, sourceReference);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.INFO, "Can not find specific member group in the destination, error: {0}", e.ToString());
                        memberGroup = profileManager.GetMemberGroups().CreateMemberGroup(source, displayName, mailNickName, description, url, sourceReference);
                    }
                    try
                    {
                        membership = mUserProfile.Memberships[memberGroup];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetMemberShipFailed, e);
                        membership = null;
                    }

                    if (membership == null)
                    {
                        AveMembershipGroupType groupType = (AveMembershipGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                        string groupName = xe.Attributes["Group"].Value;
                        AvePrivacy privacyLevel = (AvePrivacy)Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                        membership = mUserProfile.Memberships.Create(memberGroup, groupType, groupName, privacyLevel);
                        if (membership.IsTitleEditable)
                            membership.Title = xe.Attributes["Title"].Value;
                        if (membership.IsUrlEditable)
                            membership.Url = xe.Attributes["Url"].Value;
                        membership.Policy.DisplayName = subxe.Attributes["DisplayName"].Value;
                        membership.Policy.Group = subxe.Attributes["Group"].Value;
                        membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                        membership.Policy.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                    }
                    else
                    {
                        membership.Group = xe.Attributes["Group"].Value;
                        membership.GroupType = (AveMembershipGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                        if (membership.IsTitleEditable)
                            membership.Title = xe.Attributes["Title"].Value;
                        if (membership.IsUrlEditable)
                            membership.Url = xe.Attributes["Url"].Value;
                        membership.Policy.DisplayName = subxe.Attributes["DisplayName"].Value;
                        membership.Policy.Group = subxe.Attributes["Group"].Value;
                        membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                        membership.Policy.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                    }
                    try
                    {
                        if (membership != null)
                            membership.Commit();
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot update membership. membership title:{0}\n error message:{1}", membership.Title, e));
                        log.Log(AveLogLevel.ERROR, "Cannot update membership: {0} error: {1}", membership.Title, e.ToString());
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, "An error occurred while UpdateMemberships, error: {0}", e.ToString());
                }

            }

        }

        public void UpdateUserProfileMemberships(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateUserProfileMemberships"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                IAveOMembership membership = null;
                IAveOUserProfileManager profileManager = mUserProfile.ProfileManager;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    XmlElement xe = (XmlElement)node;
                    try
                    {
                        XmlElement subxe = (XmlElement)xe.ChildNodes[0];
                        XmlElement subxe1 = (XmlElement)xe.ChildNodes[1];
                        Guid source = new Guid(subxe1.Attributes["SourceInternal"].Value);
                        string displayName = subxe1.Attributes["DisplayName"].Value;
                        string mailNickName = subxe1.Attributes["MailNickName"].Value;
                        string description = subxe1.Attributes["Description"].Value;
                        string url = subxe1.Attributes["Url"].Value;
                        string sourceReference = subxe1.Attributes["SourceReference"].Value;

                        IAveOMemberGroup memberGroup = null;
                        try
                        {
                            memberGroup = profileManager.GetMemberGroups().GetMemberGroupBySourceAndSourceReference(source, sourceReference);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.INFO, "Can not find specific member group in the destination, error: {0}", e.ToString());
                            memberGroup = profileManager.GetMemberGroups().CreateMemberGroup(source, displayName, mailNickName, description, url, sourceReference);
                        }
                        try
                        {
                            membership = mUserProfile.Memberships[memberGroup];
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetMemberShipFailed, e);
                            membership = null;
                        }

                        if (membership == null)
                        {
                            AveMembershipGroupType groupType = (AveMembershipGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                            string groupName = xe.Attributes["Group"].Value;
                            AvePrivacy privacyLevel = (AvePrivacy)Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                            membership = mUserProfile.Memberships.Create(memberGroup, groupType, groupName, privacyLevel);
                            if (membership.IsTitleEditable)
                                membership.Title = xe.Attributes["Title"].Value;
                            if (membership.IsUrlEditable)
                                membership.Url = xe.Attributes["Url"].Value;
                            membership.Policy.DisplayName = subxe.Attributes["DisplayName"].Value;
                            membership.Policy.Group = subxe.Attributes["Group"].Value;
                            membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            membership.Policy.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                        }
                        else
                        {
                            membership.Group = xe.Attributes["Group"].Value;
                            membership.GroupType = (AveMembershipGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                            if (membership.IsTitleEditable)
                                membership.Title = xe.Attributes["Title"].Value;
                            if (membership.IsUrlEditable)
                                membership.Url = xe.Attributes["Url"].Value;
                            membership.Policy.DisplayName = subxe.Attributes["DisplayName"].Value;
                            membership.Policy.Group = subxe.Attributes["Group"].Value;
                            membership.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(subxe.Attributes["PrivacyPolicy"].Value);
                            membership.Policy.UserOverridePrivacy = Convert.ToBoolean(subxe.Attributes["UserOverridePrivacy"].Value);
                        }
                        try
                        {
                            if (membership != null)
                                membership.Commit();
                        }
                        catch (Exception e)
                        {
                            //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot update membership. membership title:{0}\n error message:{1}", membership.Title, e));
                            log.Log(AveLogLevel.ERROR, "Cannot update membership: {0} error: {1}", membership.Title, e.ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.ERROR, WrapperRestoreResource.CannotUpdateMemberShip, e);
                    }
                }

            }

        }

        public void UpdateColleages(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateColleagues"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                XmlElement xe = (XmlElement)xDoc.FirstChild;
                IAveOColleague colleague = null;
                IAveOUserProfileManager profileManager = mUserProfile.ProfileManager;

                try
                {
                    string profileName = xe.Attributes["AccountName"].Value;

                    foreach (IAveOColleague c in mUserProfile.Colleagues.GetItems())
                    {
                        try
                        {
                            string title = string.Empty;
                            if (c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                title = c.Title.Substring(c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                            }
                            else
                                title = c.Title;
                            if (profileName == title)
                            {
                                if (RestoreOption.mAveRestoreMode == AveRestoreMode.OverWrite)
                                {
                                    c.Delete();
                                    break;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateCollegueFailed, e);
                        }
                    }

                    XmlElement policyXml = (XmlElement)xe.ChildNodes[0];
                    if (!profileManager.UserExists(profileName))
                    {
                        StringBuilder temp = new StringBuilder("<Colleague>");
                        temp.Append(xe.OuterXml);
                        temp.Append("</Colleague>");
                        if (mColleagues.ContainsKey(profileName))
                        {
                            if (mColleagues[profileName] != null)
                                mColleagues[profileName].Add(new KeyValuePair<IAveOUserProfile, string>(mUserProfile, temp.ToString()));
                        }
                        else
                        {
                            ArrayList temparraylist = new ArrayList();
                            temparraylist.Add(new KeyValuePair<IAveOUserProfile, string>(mUserProfile, temp.ToString()));
                            mColleagues[profileName] = temparraylist;
                        }
                    }
                    else
                    {
                        if (mUserProfile.Colleagues.IsColleague(profileManager.GetUserProfile(profileName).ID))
                        {
                            colleague = mUserProfile.Colleagues[profileManager.GetUserProfile(profileName)];
                            colleague.Group = xe.Attributes["Group"].Value;
                            colleague.GroupType = (AveColleagueGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                            colleague.Policy.DisplayName = policyXml.Attributes["DisplayName"].Value;
                            colleague.Policy.Group = policyXml.Attributes["Group"].Value;
                            colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                            colleague.Policy.UserOverridePrivacy = Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                            if (colleague.IsTitleEditable)
                                colleague.Title = xe.Attributes["Title"].Value;
                            if (colleague.IsUrlEditable)
                                colleague.Url = xe.Attributes["Url"].Value;
                        }
                        else
                        {
                            string strGroup = xe.Attributes["Group"].Value;
                            AveColleagueGroupType colleagueGroupType = (AveColleagueGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                            bool isInWorkGroup = Convert.ToBoolean(xe.Attributes["IsInWorkGroup"].Value);
                            AvePrivacy privacyLevel = (AvePrivacy)Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                            try
                            {
                                colleague = mUserProfile.Colleagues.Create(profileManager.GetUserProfile(profileName), colleagueGroupType, strGroup, isInWorkGroup, privacyLevel);
                                colleague.Policy.DisplayName = policyXml.Attributes["DisplayName"].Value;
                                colleague.Policy.Group = policyXml.Attributes["Group"].Value;
                                colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                                colleague.Policy.UserOverridePrivacy = Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                                if (colleague.IsTitleEditable)
                                    colleague.Title = xe.Attributes["Title"].Value;
                                if (colleague.IsUrlEditable)
                                    colleague.Url = xe.Attributes["Url"].Value;
                            }
                            catch (Exception e)
                            {
                                //mLog.Log(AveLogLevel.WARN, string.Format("Set colleague policy error. colleague title:{0}\n error message:{1}", colleague.Title, e));
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.SetColleaguePolicyError, colleague.Title, e);
                            }
                        }
                        try
                        {
                            if (colleague != null)
                                colleague.Commit();
                        }
                        catch (Exception e)
                        {
                            //mLog.Log(AveLogLevel.WARN, string.Format("Cannot update colleague. colleague title:{0}\n error message:{1}", colleague.Title, e));
                            log.Log(AveLogLevel.WARN, "Cannot update colleague: {0} error: {1}", colleague.Title, e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, string.Format("Update colleague errror. \n error message:{0}", e));
                    log.Log(AveLogLevel.WARN, "UpdateColleagues error: {0}", e.ToString());
                }

            }

        }

        public void UpdateUserProfileColleages(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateUserProfileColleagues"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                IAveOColleague colleague = null;
                IAveOUserProfileManager profileManager = mUserProfile.ProfileManager;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    XmlElement xe = (XmlElement)node;
                    bool overwrite = true;
                    try
                    {
                        string profileName = xe.Attributes["AccountName"].Value;

                        foreach (IAveOColleague c in mUserProfile.Colleagues.GetItems())
                        {
                            try
                            {
                                string title = string.Empty;
                                if (c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) != -1)
                                {
                                    title = c.Title.Substring(c.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                                }
                                else
                                    title = c.Title;
                                if (profileName == title)
                                {
                                    if (RestoreOption.mAveRestoreMode == AveRestoreMode.OverWrite)
                                    {
                                        c.Delete();
                                        break;
                                    }
                                    else
                                    {
                                        overwrite = false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateCollegueFailed, e);
                            }
                        }
                        if (overwrite)
                        {
                            XmlElement policyXml = (XmlElement)xe.ChildNodes[0];
                            if (!profileManager.UserExists(profileName))
                            {
                                StringBuilder temp = new StringBuilder("<Colleague>");
                                temp.Append(xe.OuterXml);
                                temp.Append("</Colleague>");
                                if (mColleagues.ContainsKey(profileName))
                                {
                                    if (mColleagues[profileName] != null)
                                        mColleagues[profileName].Add(new KeyValuePair<IAveOUserProfile, string>(mUserProfile, temp.ToString()));
                                }
                                else
                                {
                                    ArrayList temparraylist = new ArrayList();
                                    temparraylist.Add(new KeyValuePair<IAveOUserProfile, string>(mUserProfile, temp.ToString()));
                                    mColleagues[profileName] = temparraylist;
                                }
                            }
                            else
                            {
                                if (mUserProfile.Colleagues.IsColleague(profileManager.GetUserProfile(profileName).ID))
                                {
                                    colleague = mUserProfile.Colleagues[profileManager.GetUserProfile(profileName)];
                                    colleague.Group = xe.Attributes["Group"].Value;
                                    colleague.GroupType = (AveColleagueGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                                    colleague.Policy.DisplayName = policyXml.Attributes["DisplayName"].Value;
                                    colleague.Policy.Group = policyXml.Attributes["Group"].Value;
                                    colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                                    colleague.Policy.UserOverridePrivacy = Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                                    if (colleague.IsTitleEditable)
                                        colleague.Title = xe.Attributes["Title"].Value;
                                    if (colleague.IsUrlEditable)
                                        colleague.Url = xe.Attributes["Url"].Value;
                                }
                                else
                                {
                                    string strGroup = xe.Attributes["Group"].Value;
                                    AveColleagueGroupType colleagueGroupType = (AveColleagueGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                                    bool isInWorkGroup = Convert.ToBoolean(xe.Attributes["IsInWorkGroup"].Value);
                                    AvePrivacy privacyLevel = (AvePrivacy)Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                                    try
                                    {
                                        colleague = mUserProfile.Colleagues.Create(profileManager.GetUserProfile(profileName), colleagueGroupType, strGroup, isInWorkGroup, privacyLevel);
                                        colleague.Policy.DisplayName = policyXml.Attributes["DisplayName"].Value;
                                        colleague.Policy.Group = policyXml.Attributes["Group"].Value;
                                        colleague.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                                        colleague.Policy.UserOverridePrivacy = Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                                        if (colleague.IsTitleEditable)
                                            colleague.Title = xe.Attributes["Title"].Value;
                                        if (colleague.IsUrlEditable)
                                            colleague.Url = xe.Attributes["Url"].Value;
                                    }
                                    catch (Exception e)
                                    {
                                        //mLog.Log(AveLogLevel.WARN, string.Format("Set colleague policy error. colleague title:{0}\n error message:{1}", colleague.Title, e));
                                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.SetColleaguePolicyError, colleague.Title, e);
                                    }
                                }
                                try
                                {
                                    if (colleague != null)
                                        colleague.Commit();
                                }
                                catch (Exception e)
                                {
                                    //mLog.Log(AveLogLevel.WARN, string.Format("Cannot update colleague. colleague title:{0}\n error message:{1}", colleague.Title, e));
                                    log.Log(AveLogLevel.WARN, "Cannot update colleague: {0} error: {1}", colleague.Title, e.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.ERROR, string.Format("Update colleague errror. \n error message:{0}", e));
                        log.Log(AveLogLevel.ERROR, "UpdateColleagues error: {0}", e.ToString());
                    }

                }

            }

        }

        public void UpdateTags(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateTags"))
            {

                IAveSite tempSite = null;
                bool needDispose = true;
                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(xml);
                    XmlElement xe = (XmlElement)xDoc.FirstChild;

                    string tagTitle = xe.Attributes["NameValue"].Value;
                    string url = xe.Attributes["Url"].Value;
                    bool isPrivate = Boolean.Parse(xe.Attributes["IsPrivate"].Value);
                    string termName = xe.Attributes["termName"].Value;


                    string ownerLogin = mUserProfile.DisplayName;
                    if (mUserProfile.MultiloginAccounts.Length > 0)
                    {
                        ownerLogin = mUserProfile.MultiloginAccounts[0];
                    }
                    tempSite = mUserProfile.PersonalSite;
                    if (tempSite == null)
                    {
                        needDispose = false;
                        tempSite = this.mSite.SPSite;
                    }
                    IAveTaxonomySession session = mSite.ObjectModelFactory.CreateTaxonomySession(tempSite);
                    IAveTermStore keywords = session.DefaultKeywordsTermStore;
                    IAveTerm term = null;
                    int lcid = keywords.DefaultLanguage;
                    try
                    {
                        term = keywords.KeywordsTermSet.Terms[termName];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        term = keywords.KeywordsTermSet.CreateTerm(termName, lcid);
                        term.Owner = ownerLogin;
                        keywords.CommitAll();
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetTermFailed, e);
                    }

                    IAveOSocialTagManager tagManager = mSite.ObjectModelFactory.CreateSocialTagManager(mContext);
                    try
                    {
                        #region reset m_UserProfile
                        IAveOProfileLoader objProfileLoder = tagManager.ProfileLoader;// typeof(SocialDataManager).InvokeMember("ProfileLoader", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, tagManager, new object[] { });
                        objProfileLoder.UserProfile = mUserProfile;
                        //typeof(ProfileLoader).InvokeMember("m_UserProfile", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.SetField, null, objProfileLoder, new object[] { mUserProfile });
                        #endregion
                        tagManager.AddTag(new Uri(url), term, tagTitle, isPrivate);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("Cannot add tag. tagTitle:{0}\n error message:{1}", tagTitle, e));
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("UpdateTags error. \n error message:{0}", e));
                }
                finally
                {
                    if (needDispose && tempSite != null)
                    {
                        tempSite.Dispose();
                    }
                }


            }

        }

        public void UpdateUserProfileTags(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateUserProfileTags"))
            {

                if (!mEnableTag)
                {
                    return;
                }
                IAveSite tempSite = null;
                bool needDispose = true;
                try
                {
                    string ownerLogin = mUserProfile.DisplayName;
                    if (mUserProfile.MultiloginAccounts.Length > 0)
                    {
                        ownerLogin = mUserProfile.MultiloginAccounts[0];
                    }
                    tempSite = mUserProfile.PersonalSite;
                    if (tempSite == null)
                    {
                        needDispose = false;
                        tempSite = this.mSite.SPSite;
                    }

                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(xml);
                    foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                    {
                        try
                        {
                            XmlElement xe = (XmlElement)node;
                            string tagTitle = xe.Attributes["NameValue"].Value;
                            string url = xe.Attributes["Url"].Value;
                            bool isPrivate = Boolean.Parse(xe.Attributes["IsPrivate"].Value);
                            string termName = xe.Attributes["termName"].Value;

                            IAveTaxonomySession session = mSite.ObjectModelFactory.CreateTaxonomySession(tempSite);
                            IAveTermStore keywords = session.DefaultKeywordsTermStore;
                            IAveTerm term = null;
                            try
                            {
                                term = keywords.KeywordsTermSet.Terms[termName];
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                term = keywords.KeywordsTermSet.CreateTerm(termName, 1033);
                                keywords.CommitAll();
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetTermFailed, e);
                            }

                            IAveOSocialTagManager tagManager = mSite.ObjectModelFactory.CreateSocialTagManager(mContext);
                            try
                            {
                                tagManager.AddTag(new Uri(url), term, tagTitle, isPrivate);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.ERROR, string.Format("Cannot add tag. tagTitle:{0}\n error message:{1}", tagTitle, e));
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.ERROR, string.Format("UpdateTags error. \n error message:{0}", e));
                        }
                    }
                }
                finally
                {
                    if (needDispose && tempSite != null)
                    {
                        tempSite.Dispose();
                    }
                }

            }

        }

        public void UpdateNotes(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateNotes"))
            {

                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(xml);
                    XmlElement xe = (XmlElement)xDoc.FirstChild;

                    string comment = xe.Attributes["NameValue"].Value;
                    string url = xe.Attributes["Url"].Value;
                    bool isHighPriority = Boolean.Parse(xe.Attributes["IsHighPriority"].Value);

                    IAveOSocialCommentManager commentManager = mSite.ObjectModelFactory.CreateSocialCommentManager(mContext);

                    try
                    {
                        commentManager.AddComment(new Uri(url), comment, isHighPriority);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("Cannot add note. comment:{0}\n error message:{1}", comment, e));
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("UpdateNotes error. \n error message:{0}", e));
                }

            }

        }

        public void UpdateUserProfileNotes(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateUserProfileNotes"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);

                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    try
                    {
                        XmlElement xe = (XmlElement)node;
                        string comment = xe.Attributes["NameValue"].Value;
                        string url = xe.Attributes["Url"].Value;
                        bool isHighPriority = Boolean.Parse(xe.Attributes["IsHighPriority"].Value);

                        IAveOSocialCommentManager commentManager = mSite.ObjectModelFactory.CreateSocialCommentManager(mContext);

                        try
                        {
                            commentManager.AddComment(new Uri(url), comment, isHighPriority);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.ERROR, string.Format("Cannot add note. comment:{0}\n error message:{1}", comment, e));
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("UpdateNotes error. \n error message:{0}", e));
                    }
                }


            }

        }

        public void UpdateLinks(string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.UpdateLinks"))
            {

                try
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(xml);
                    XmlElement xe = (XmlElement)xDoc.FirstChild;
                    IAveOQuickLink quickLink = null;
                    string profileManagerUrl = xe.Attributes["profileManagerUrl"].Value;
                    foreach (IAveOQuickLink link in mUserProfile.QuickLinks.GetItems())
                    {
                        if (link.Title == xe.Attributes["NameValue"].Value)
                        {
                            if (RestoreOption.mAveRestoreMode == AveRestoreMode.OverWrite)
                            {
                                link.Delete();
                                break;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    XmlElement policyXml = (XmlElement)xe.ChildNodes[0];
                    string strTitle = xe.Attributes["Title"].Value;
                    string strUrl = xe.Attributes["Url"].Value;

                    foreach (string originalUrl in mAbsoluteUrlMapping.Keys)
                    {
                        if (strUrl.Contains(originalUrl))
                        {
                            strUrl = strUrl.Replace(originalUrl, mAbsoluteUrlMapping[originalUrl].ToString());
                            break;
                        }
                    }
                    if (strUrl.Contains(profileManagerUrl))//
                    {
                        strUrl = strUrl.Replace(profileManagerUrl, mUserProfileManager.MySiteHostUrl.ToString());
                    }
                    string strGroup = xe.Attributes["Group"].Value;
                    AveQuickLinkGroupType groupType = (AveQuickLinkGroupType)Convert.ToInt32(xe.Attributes["GroupType"].Value);
                    AvePrivacy privacyLevel = (AvePrivacy)Convert.ToInt32(xe.Attributes["PrivacyLevel"].Value);
                    try
                    {
                        quickLink = mUserProfile.QuickLinks.Create(strTitle, strUrl, groupType, strGroup, privacyLevel);
                        quickLink.Policy.DefaultPrivacy = (AvePrivacy)Convert.ToInt32(policyXml.Attributes["DefaultPrivacy"].Value);
                        quickLink.Policy.DisplayName = policyXml.Attributes["DisplayName"].Value;
                        quickLink.Policy.Group = policyXml.Attributes["Group"].Value;
                        quickLink.Policy.PrivacyPolicy = (AvePrivacyPolicy)Convert.ToInt32(policyXml.Attributes["PrivacyPolicy"].Value);
                        quickLink.Policy.UserOverridePrivacy = Convert.ToBoolean(policyXml.Attributes["UserOverridePrivacy"].Value);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.ERROR, string.Format("Update Link Policy error. {0}", e));
                    }
                    quickLink.Commit();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("UpdateLinks error. \n error message:{0}", e));
                }

            }

        }

        public void SetWorkingProfileList(string listName)
        {
            mWorkingListName = listName;
        }

        private AveUserProfile mAveProfile;

        public AveSPUserProfile(AveSPSite _aveSite, bool needInit)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.Constructor"))
            {

                string login = "";
                if (needInit)
                {
                    try
                    {
                        if (_aveSite.SPSite.Owner != null)
                        {
                            login = _aveSite.SPSite.Owner.LoginName;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetLoginUserError, e.ToString());
                        needInit = false;
                    }
                }
                if (String.IsNullOrEmpty(login))
                {
                    needInit = false;
                }
                mAveProfile = new AveUserProfile(_aveSite.ServiceContext, login, needInit, _aveSite.SourceSiteInfo, _aveSite.ServerRelativeUrl) { UserMap = _aveSite.SPMembers.GetMappingUserLogin };
                mSite = _aveSite;

                userProfileSerializer = mSite.ObjectModelFactory.CreateUserProfileSerializer(mSite.SPSite, login, needInit, mSite.SourceSiteInfo, _aveSite.SPMembers.GetMappingUserLogin);

            }

        }

        public void Restore(AveUserProfileInfo profileInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.Restore"))
            {
                profileInfo.LoginName = mSite.SPMembers.GetMappingUserLogin(profileInfo.LoginName, true);
                profileInfo.UserMapping = ((AveCustomUserAndDomainMapping)((AveUserAndDomainMapping)mSite.SPMembers.UserAndDomainMapping).customUserAndDomainMapping).customUserMappings;
                this.userProfileSerializer.ExistSkip = this.mExistSkip;
                this.userProfileSerializer.SetObjectData(profileInfo);
                mSite.MappingManager.SiteMappingManager.AddRatingCache(profileInfo.LoginName, profileInfo.Ratings);
            }

        }

        public void RestoreRating(List<AveSOcialRatingInfo> ratings)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreRaring"))
            {
                this.userProfileSerializer.SetObjectData(ratings);
            }
        }

        public void RestoreForArchiver(AveUserProfileInfo profileInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.Restore"))
            {

                profileInfo.LoginName = mSite.SPMembers.GetMappingUserLogin(profileInfo.LoginName, true);
                this.userProfileSerializer.ExistSkip = this.mExistSkip;
                this.userProfileSerializer.SetObjectDataForArchiver(profileInfo);

            }

        }

        public void RestoreQuickLink(AveQuickLinkInfo lInfo)
        {
            this.mAveProfile.RestoreQuickLink(lInfo);
        }

        public void RestoreMembership(AveMembershipInfo mInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreMemberships"))
            {

                if (!NeedSkip)
                {
                    this.mAveProfile.RestoreMembership(mInfo);
                }

            }

        }

        public void RestoreTag(AveSocialTagInfo tagInfo)
        {
            if (!mEnableTag || NeedSkip)
            {
                return;
            }

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreTag"))
            {

                this.mAveProfile.RestoreTag(tagInfo);

            }

        }

        public void RestoreComment(AveSocialCommentInfo commentInfo)
        {
            if (!mEnableTag || NeedSkip)
            {
                return;
            }

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreComment"))
            {

                this.mAveProfile.RestoreComment(commentInfo);

            }

        }

        public void RestoreColleague(AveColleagueInfo colleagueInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreColleague"))
            {

                if (!NeedSkip)
                {
                    this.mAveProfile.RestoreColleague(colleagueInfo);
                }

            }

        }

        public void RestoreDetails(List<AveUserProfileValueInfo> valueInfos)
        {
            if (!NeedSkip)
            {
                RestoreDetails(valueInfos, true);
            }
        }

        public void RestoreDetails(List<AveUserProfileValueInfo> valueInfos, bool isOverwrite)
        {

            if (NeedSkip)
            {
                return;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreDetails"))
            {

                foreach (AveUserProfileValueInfo valueInfo in valueInfos)
                {
                    mAveProfile.RestoreDetail(valueInfo, isOverwrite);
                }

            }

        }

        public void RestoreDetail(AveUserProfileValueInfo valueInfo)
        {
            if (NeedSkip)
            {
                return;
            }
            RestoreDetail(valueInfo, true);
        }

        public void RestoreDetail(AveUserProfileValueInfo valueInfo, bool isOverwrite)
        {
            if (NeedSkip)
            {
                return;
            }
            mAveProfile.RestoreDetail(valueInfo, isOverwrite);
        }

        public void RestoreUserProfileProperties(List<AvePropertyInfo> properties)
        {
            RestoreUserProfileProperties(properties, true);
        }

        public void RestoreUserProfileProperties(List<AvePropertyInfo> properties, bool isOverwrite)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPUserProfile.RestoreUserProfileProperties"))
            {

                this.userProfileSerializer.SetObjectData(properties, isOverwrite);

            }

        }

        public void RestoreUserProfileSubTypes(List<AveUserProfileSubTypeInfo> subTypes)
        {
            this.userProfileSerializer.SetObjectData(subTypes);
        }
        #region IAveSPUserProfile Members


        IAveSPSite IAveSPUserProfile.Site
        {
            get
            {
                return mSite;
            }
            set
            {
                mSite = value as AveSPSite;
            }
        }
        public void Dispose()
        {
            mAveProfile.Dispose();
        }

        #endregion
    }
}
