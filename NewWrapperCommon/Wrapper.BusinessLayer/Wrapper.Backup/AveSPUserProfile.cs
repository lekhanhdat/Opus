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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System.Linq;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPUserProfile
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveOUserProfile mUserProfile = null;
        private IAveServiceContext mContext = null;
        private IAveOUserProfileManager mUserProfileManager = null;
        private string[] mUnPermitEndString = new string[] { "{My Colleagues}", "{My Details}", "{My Memberships}", "{My Notes}", "{My Tags}" };
        private List<AveUserInfo> mAveUserInfoList = new List<AveUserInfo>();
        private AveObjectModelFactory mOMFactory;
        private AveContextKind mContextKind;
        private AveSPSite mParentSite = null;

        public AveSPUserProfile(IAveWebApplication webApp, string loginName, AveContextKind contextKind)
        {
            mContextKind = contextKind;
            mOMFactory = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), mContextKind);
            AveServiceContextInfo info = new AveServiceContextInfo
            {
                WebApplication = webApp,
                SiteSubscriptionIdentifier = mOMFactory.CreateSiteSubscriptionIdentifier().Default
            };
            mContext = mOMFactory.CreateServerContext(info);
            mUserProfileManager = mOMFactory.CreateUserProfileManager(mContext);
            mUserProfile = mUserProfileManager.GetUserProfile(loginName);
            mAveUserInfoList.Add(new AveUserInfo() { Login = loginName });
        }

        //public AveSPUserProfile(IAveWebApplication webApp, List<AveUserInfo> aveUserInfoList, AveContextKind contextKind)
        //{
        //    mContextKind = contextKind;
        //    mOMFactory = AveObjectModelFactory.CreateObjectModelFactory("", new AveBPOSAccountInfo(), mContextKind);
        //    IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = mOMFactory.CreateSiteSubscriptionIdentifier();
        //    IAveServiceContext context = mOMFactory.CreateServiceContext();
        //    mContext = context.GetContext(webApp.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
        //    IAveOUserProfileManager userProfileManager = mOMFactory.CreateUserProfileManager(mContext);
        //    mAveUserInfoList = aveUserInfoList;
        //    //mUserProfile = userProfileManager.GetUserProfile(loginName);
        //}

        public AveSPUserProfile(AveSPSite site, string loginName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.UserProfile.Constructor"))
            {
                mParentSite = site;
                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mParentSite.SPSite,
                    WebApplication = mParentSite.SPSite.WebApplication,
                    SiteSubscriptionIdentifier = mParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                };
                mContextKind = mParentSite.ObjectModelFactory.ContextKind;
                mContext = mParentSite.ObjectModelFactory.CreateServerContext(info);
                mUserProfileManager = mParentSite.ObjectModelFactory.CreateUserProfileManager(mContext);
                mUserProfile = mUserProfileManager.GetUserProfile(loginName);
                mAveUserInfoList.Add(new AveUserInfo() { Login = loginName });
            }
        }

        public AveSPUserProfile(AveSPSite site, List<AveUserInfo> aveUserInfoList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.UserProfile.Constructor"))
            {
                mParentSite = site;

                AveServiceContextInfo info = new AveServiceContextInfo
                {
                    Site = mParentSite.SPSite,
                    WebApplication = mParentSite.SPSite.WebApplication,
                    SiteSubscriptionIdentifier = mParentSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier().Default
                };
                mContextKind = mParentSite.ObjectModelFactory.ContextKind;
                mContext = mParentSite.ObjectModelFactory.CreateServerContext(info);
                mUserProfileManager = mParentSite.ObjectModelFactory.CreateUserProfileManager(mContext);
                mAveUserInfoList = aveUserInfoList;
            }
        }

        public string GetDetailXml()
        {
            try
            {
                IEnumerator enumprop = null;
                enumprop = mUserProfile.ProfileManager.DefaultProfileSubtypeProperties.GetEnumerator();
                StringBuilder temp = new StringBuilder("<Detail>");
                XmlDocument xdoc = new XmlDocument();

                while (enumprop.MoveNext())
                {
                    IAveOProfileSubtypeProperty prop = (IAveOProfileSubtypeProperty)enumprop.Current;

                    XmlElement xe = xdoc.CreateElement("MyDetail");
                    xe.SetAttribute("NameValue", prop.Name);
                    xe.SetAttribute("Capacity", mUserProfile[prop.Name].Capacity.ToString());
                    xe.SetAttribute("Count", mUserProfile[prop.Name].Count.ToString());
                    xe.SetAttribute("Privacy", ((int)mUserProfile[prop.Name].Privacy).ToString());
                    XmlElement subxe = xdoc.CreateElement("Property");

                    subxe.SetAttribute("AllowPolicyOverride", mUserProfile[prop.Name].Property.AllowPolicyOverride.ToString());
                    subxe.SetAttribute("DefaultPrivacy", ((int)mUserProfile[prop.Name].Property.DefaultPrivacy).ToString());
                    subxe.SetAttribute("Description", mUserProfile[prop.Name].Property.Description);
                    subxe.SetAttribute("DescriptionLocalized", mUserProfile[prop.Name].Property.DescriptionLocalized.DefaultLanguage.ToString());
                    subxe.SetAttribute("DisplayName", mUserProfile[prop.Name].Property.DisplayName);
                    subxe.SetAttribute("DisplayNameLocalized", mUserProfile[prop.Name].Property.DisplayNameLocalized.DefaultLanguage.ToString());
                    subxe.SetAttribute("DisplayOrder", mUserProfile[prop.Name].Property.DisplayOrder.ToString());
                    subxe.SetAttribute("IsAdminEditable", mUserProfile[prop.Name].Property.IsAdminEditable.ToString());
                    subxe.SetAttribute("IsAlias", mUserProfile[prop.Name].Property.IsAlias.ToString());
                    subxe.SetAttribute("IsColleagueEventLog", mUserProfile[prop.Name].Property.IsColleagueEventLog.ToString());
                    subxe.SetAttribute("IsImported", mUserProfile[prop.Name].Property.IsImported.ToString());
                    subxe.SetAttribute("IsMultivalued", mUserProfile[prop.Name].Property.IsMultivalued.ToString());
                    subxe.SetAttribute("IsReplicable", mUserProfile[prop.Name].Property.IsReplicable.ToString());
                    subxe.SetAttribute("IsRequired", mUserProfile[prop.Name].Property.IsRequired.ToString());
                    subxe.SetAttribute("IsSearchable", mUserProfile[prop.Name].Property.IsSearchable.ToString());
                    subxe.SetAttribute("IsSection", mUserProfile[prop.Name].Property.IsSection.ToString());
                    subxe.SetAttribute("IsSystem", mUserProfile[prop.Name].Property.IsSystem.ToString());
                    subxe.SetAttribute("IsUpgrade", mUserProfile[prop.Name].Property.IsUpgrade.ToString());
                    subxe.SetAttribute("IsUpgradePrivate", mUserProfile[prop.Name].Property.IsUpgradePrivate.ToString());
                    subxe.SetAttribute("IsUserEditable", mUserProfile[prop.Name].Property.IsUserEditable.ToString());
                    subxe.SetAttribute("IsVisibleOnEditor", mUserProfile[prop.Name].Property.IsVisibleOnEditor.ToString());
                    subxe.SetAttribute("IsVisibleOnViewer", mUserProfile[prop.Name].Property.IsVisibleOnViewer.ToString());
                    subxe.SetAttribute("Length", mUserProfile[prop.Name].Property.Length.ToString());
                    subxe.SetAttribute("ManagedPropertyName", mUserProfile[prop.Name].Property.ManagedPropertyName);
                    subxe.SetAttribute("MaximumShown", mUserProfile[prop.Name].Property.MaximumShown.ToString());
                    subxe.SetAttribute("Name", mUserProfile[prop.Name].Property.Name);
                    subxe.SetAttribute("PrivacyPolicy", ((int)mUserProfile[prop.Name].Property.PrivacyPolicy).ToString());
                    subxe.SetAttribute("Separator", ((int)mUserProfile[prop.Name].Property.Separator).ToString());
                    subxe.SetAttribute("Type", mUserProfile[prop.Name].Property.Type);
                    subxe.SetAttribute("URI", mUserProfile[prop.Name].Property.URI);
                    subxe.SetAttribute("UserOverridePrivacy", mUserProfile[prop.Name].Property.UserOverridePrivacy.ToString());
                    xe.AppendChild(subxe);
                    string value = "Value";
                    for (int m = 0; m < mUserProfile[prop.Name].Count; m++)
                    {
                        string valueStr = ConvertValueAsString(mUserProfile[prop.Name][m]);
                        xe.SetAttribute(value + m.ToString(), valueStr);
                    }
                    temp.Append(xe.OuterXml);
                    xe.RemoveAll();
                }
                temp.Append("</Detail>");
                xdoc.RemoveAll();
                return temp.ToString();
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting detail information.", e);
                //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mysite's detail.\n error message:{0}", e));
                return string.Empty;
            }
        }

        public void ExportDetails(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportDetails"))
            {
                try
                {
                    List<AveUserProfileValueInfo> list = new List<AveUserProfileValueInfo>();

                    IEnumerator enumprop = mUserProfile.ProfileManager.DefaultProfileSubtypeProperties.GetEnumerator();
                    while (enumprop.MoveNext())
                    {
                        IAveOProfileSubtypeProperty prop = (IAveOProfileSubtypeProperty)enumprop.Current;
                        AveUserProfileValueInfo valueInfo = GetUserProfileValueInfo(prop);
                        list.Add(valueInfo);
                    }
                    if (list.Count > 0)
                    {
                        stream.WriteMetadata(AveMetadataType.UserProfileDetail, list);
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting detail information.", e);
                    //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mysite's detail.\n error message:{0}", e));
                }
            }
        }

        public List<AveUserProfileValueInfo> GetDetails()
        {
            List<AveUserProfileValueInfo> list = new List<AveUserProfileValueInfo>();
            try
            {
                IEnumerator enumprop = mUserProfile.ProfileManager.DefaultProfileSubtypeProperties.GetEnumerator();
                while (enumprop.MoveNext())
                {
                    IAveOProfileSubtypeProperty prop = (IAveOProfileSubtypeProperty)enumprop.Current;
                    AveUserProfileValueInfo valueInfo = GetUserProfileValueInfo(prop);
                    list.Add(valueInfo);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting detail information. Error information:{0}", e);
                //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mysite's detail.\n error message:{0}", e));
            }
            return list;
        }

        public AveUserProfileValueInfo GetUserProfileValueInfo(IAveOProfileSubtypeProperty prop)
        {
            AveUserProfileValueInfo valueInfo = new AveUserProfileValueInfo();
            valueInfo.Name = prop.Name;
            valueInfo.Capacity = mUserProfile[prop.Name].Capacity;
            valueInfo.Count = mUserProfile[prop.Name].Count;
            valueInfo.Privacy = (int)mUserProfile[prop.Name].Privacy;

            #region PropertyInfo

            //AvePropertyInfo propertyInfo = new AvePropertyInfo();
            //valueInfo.Property = propertyInfo;

            //propertyInfo.AllowPolicyOverride = mUserProfile[prop.Name].Property.AllowPolicyOverride;
            //propertyInfo.DefaultPrivacy = (int)mUserProfile[prop.Name].Property.DefaultPrivacy;
            //propertyInfo.Description = mUserProfile[prop.Name].Property.Description;
            //propertyInfo.DescriptionLocalized = mUserProfile[prop.Name].Property.DescriptionLocalized.DefaultLanguage;
            //propertyInfo.DisplayName = mUserProfile[prop.Name].Property.DisplayName;
            //propertyInfo.DisplayNameLocalized = mUserProfile[prop.Name].Property.DisplayNameLocalized.DefaultLanguage;

            //propertyInfo.DisplayOrder = mUserProfile[prop.Name].Property.DisplayOrder;
            //propertyInfo.IsAdminEditable = mUserProfile[prop.Name].Property.IsAdminEditable;
            //propertyInfo.IsAlias = mUserProfile[prop.Name].Property.IsAlias;
            //propertyInfo.IsColleagueEventLog = mUserProfile[prop.Name].Property.IsColleagueEventLog;
            //propertyInfo.IsImported = mUserProfile[prop.Name].Property.IsImported;
            //propertyInfo.IsMultivalued = mUserProfile[prop.Name].Property.IsMultivalued;
            //propertyInfo.IsReplicable = mUserProfile[prop.Name].Property.IsReplicable;
            //propertyInfo.IsRequired = mUserProfile[prop.Name].Property.IsRequired;
            //propertyInfo.IsSearchable = mUserProfile[prop.Name].Property.IsSearchable;
            //propertyInfo.IsSection = mUserProfile[prop.Name].Property.IsSection;
            //propertyInfo.IsSystem = mUserProfile[prop.Name].Property.IsSystem;
            //propertyInfo.IsUpgrade = mUserProfile[prop.Name].Property.IsUpgrade;
            //propertyInfo.IsUpgradePrivate = mUserProfile[prop.Name].Property.IsUpgradePrivate;
            //propertyInfo.I;sUserEditable = mUserProfile[prop.Name].Property.IsUserEditable;
            //propertyInfo.IsVisibleOnEditor = mUserProfile[prop.Name].Property.IsVisibleOnEditor;
            //propertyInfo.IsVisibleOnViewer = mUserProfile[prop.Name].Property.IsVisibleOnViewer;

            //propertyInfo.Length = mUserProfile[prop.Name].Property.Length;
            //propertyInfo.ManagedPropertyName = mUserProfile[prop.Name].Property.ManagedPropertyName;
            //propertyInfo.MaximumShown = mUserProfile[prop.Name].Property.MaximumShown;
            //propertyInfo.Name = mUserProfile[prop.Name].Property.Name;
            //propertyInfo.PrivacyPolicy = (int)mUserProfile[prop.Name].Property.PrivacyPolicy;
            //propertyInfo.Separator = (int)mUserProfile[prop.Name].Property.Separator;
            //propertyInfo.Type = mUserProfile[prop.Name].Property.Type;
            //propertyInfo.URI = mUserProfile[prop.Name].Property.URI;
            //propertyInfo.UserOverridePrivacy = mUserProfile[prop.Name].Property.UserOverridePrivacy;

            #endregion PropertyInfo

            for (int m = 0; m < mUserProfile[prop.Name].Count; m++)
            {
                try
                {
                    valueInfo.Values.Add(ConvertValueAsString(mUserProfile[prop.Name][m]));
                }
                catch (Exception ex)
                {
                    log.Error("An error occurred while convert the profileDetail to string, property:{0}, error:{1}", prop.Name, ex.ToString());
                }
            }
            return valueInfo;
        }

        private string ConvertValueAsString(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
            IAveTimeZone timeZone = obj as IAveTimeZone;
            if (timeZone != null)
            {
                return timeZone.ID.ToString();
            }
            return obj.ToString();
        }

        public string GetMembershipXml()
        {
            try
            {
                StringBuilder temp = new StringBuilder("<Memberships>");
                XmlDocument xdoc = new XmlDocument();
                foreach (IAveOMembership membership in mUserProfile.Memberships.GetItems())
                {
                    XmlElement xe = xdoc.CreateElement("MyMembership");
                    xe.SetAttribute("NameValue", membership.Title);
                    xe.SetAttribute("Group", membership.Group);
                    xe.SetAttribute("GroupType", ((int)membership.GroupType).ToString());
                    xe.SetAttribute("IsEditable", membership.IsEditable.ToString());
                    xe.SetAttribute("IsPrivacyLevelEditable", membership.IsPrivacyLevelEditable.ToString());
                    xe.SetAttribute("IsTitleEditable", membership.IsTitleEditable.ToString());
                    xe.SetAttribute("IsUrlEditable", membership.IsUrlEditable.ToString());
                    xe.SetAttribute("PrivacyLevel", ((int)membership.PrivacyLevel).ToString());
                    xe.SetAttribute("Url", membership.Url);
                    XmlElement subxe = xdoc.CreateElement("Policy");
                    subxe.SetAttribute("AllowPolicyOverride", membership.Policy.AllowPolicyOverride.ToString());
                    subxe.SetAttribute("DefaultPrivacy", ((int)membership.Policy.DefaultPrivacy).ToString());
                    subxe.SetAttribute("DisplayName", membership.Policy.DisplayName);
                    subxe.SetAttribute("FilterPrivacyItems", membership.Policy.FilterPrivacyItems.ToString());
                    subxe.SetAttribute("Group", membership.Policy.Group);
                    subxe.SetAttribute("PrivacyPolicy", ((int)membership.Policy.PrivacyPolicy).ToString());
                    subxe.SetAttribute("UserOverridePrivacy", membership.Policy.UserOverridePrivacy.ToString());
                    xe.AppendChild(subxe);
                    XmlElement subxe1 = xdoc.CreateElement("MemberGroup");
                    subxe1.SetAttribute("Count", membership.MembershipGroup.Count.ToString());
                    subxe1.SetAttribute("Description", membership.MembershipGroup.Description);
                    subxe1.SetAttribute("DisplayName", membership.MembershipGroup.DisplayName);
                    subxe1.SetAttribute("Id", membership.MembershipGroup.Id.ToString());
                    subxe1.SetAttribute("LastUpdate", membership.MembershipGroup.LastUpdate.ToBinary().ToString());
                    subxe1.SetAttribute("MailNickName", membership.MembershipGroup.MailNickName);
                    subxe1.SetAttribute("Source", ((int)membership.MembershipGroup.Source).ToString());
                    subxe1.SetAttribute("SourceInternal", membership.MembershipGroup.SourceInternal.ToString());
                    subxe1.SetAttribute("SourceReference", membership.MembershipGroup.SourceReference);
                    subxe1.SetAttribute("Url", membership.MembershipGroup.Url);
                    xe.AppendChild(subxe1);
                    temp.Append(xe.OuterXml);
                    xe.RemoveAll();
                }
                temp.Append("</Memberships>");
                xdoc.RemoveAll();
                return temp.ToString();
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mySite's detail. \n error message:{0}", e));
                log.Error("An error occurred while getting Membership.}", e);
                return string.Empty;
            }
        }

        public void ExportMemberships(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportMemberships"))
            {
                try
                {
                    foreach (IAveOMembership membership in mUserProfile.Memberships.GetItems())
                    {
                        AveMembershipInfo info = GetMembershipInfo(membership);
                        stream.WriteMetadata(AveMetadataType.UserProfileMembership, info);
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mySite's detail. \n error message:{0}", e));
                    log.Error("An error occurred while getting Membership.}", e);
                }
            }
        }

        public List<AveMembershipInfo> GetMemberships()
        {
            List<AveMembershipInfo> memberships = new List<AveMembershipInfo>();
            try
            {
                foreach (IAveOMembership membership in mUserProfile.Memberships.GetItems())
                {
                    AveMembershipInfo info = GetMembershipInfo(membership);
                    memberships.Add(info);
                }
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.ERROR, string.Format("Can't get mySite's detail. \n error message:{0}", e));
                log.Error("An error occurred while getting Membership.}", e);
            }
            return memberships;
        }

        public AveMembershipInfo GetMembershipInfo(IAveOMembership membership)
        {
            AveMembershipInfo info = new AveMembershipInfo();
            info.Title = membership.Title;
            info.Group = membership.Group;
            info.GroupType = (int)membership.GroupType;
            info.IsEditable = membership.IsEditable;
            info.IsPrivacyLevelEditable = membership.IsPrivacyLevelEditable;
            info.IsTitleEditable = membership.IsTitleEditable;
            info.IsUrlEditable = membership.IsUrlEditable;
            info.PrivacyLevel = (int)membership.PrivacyLevel;
            info.Url = membership.Url;

            info.Policy = GetPolicyInfo(membership.Policy);

            info.MembershipGroup = new AveMembershipGroup();
            info.MembershipGroup.Count = membership.MembershipGroup.Count;
            info.MembershipGroup.Description = membership.MembershipGroup.Description;
            info.MembershipGroup.DisplayName = membership.MembershipGroup.DisplayName;
            info.MembershipGroup.Id = membership.MembershipGroup.Id;
            info.MembershipGroup.LastUpdate = membership.MembershipGroup.LastUpdate.ToBinary();
            info.MembershipGroup.MailNickName = membership.MembershipGroup.MailNickName;
            info.MembershipGroup.Source = (int)membership.MembershipGroup.Source;
            info.MembershipGroup.SourceInternal = membership.MembershipGroup.SourceInternal;
            info.MembershipGroup.SourceReference = membership.MembershipGroup.SourceReference;
            info.MembershipGroup.Url = membership.MembershipGroup.Url;
            return info;
        }

        public string GetColleaguesXml()
        {
            try
            {
                StringBuilder temp = new StringBuilder("<Colleague>");
                XmlDocument xdoc = new XmlDocument();
                foreach (IAveOColleague colleague in mUserProfile.Colleagues.GetItems())
                {
                    string title = "";
                    if (colleague.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        title = colleague.Title.Substring(colleague.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                    }
                    else
                        title = colleague.Title;

                    XmlElement xe = xdoc.CreateElement("MyColleague");
                    xe.SetAttribute("NameValue", title);
                    xe.SetAttribute("AccountName", colleague.Profile.MultiloginAccounts[0]);
                    xe.SetAttribute("Group", colleague.Group);
                    xe.SetAttribute("GroupType", ((int)colleague.GroupType).ToString());
                    xe.SetAttribute("IsInWorkGroup", colleague.IsInWorkGroup.ToString());
                    xe.SetAttribute("PrivacyLevel", ((int)colleague.PrivacyLevel).ToString());
                    xe.SetAttribute("IsAssistant", colleague.IsAssistant.ToString());
                    xe.SetAttribute("IsEditable", colleague.IsEditable.ToString());
                    xe.SetAttribute("IsPrivacyLevelEditable", colleague.IsPrivacyLevelEditable.ToString());
                    xe.SetAttribute("IsTitleEditable", colleague.IsTitleEditable.ToString());
                    xe.SetAttribute("IsUrlEditable", colleague.IsUrlEditable.ToString());
                    xe.SetAttribute("Url", colleague.Url.ToString());
                    xe.SetAttribute("Title", colleague.Title);

                    XmlElement subxe = xdoc.CreateElement("Policy");
                    subxe.SetAttribute("AllowPolicyOverride", colleague.Policy.AllowPolicyOverride.ToString());
                    subxe.SetAttribute("DefaultPrivacy", ((int)colleague.Policy.DefaultPrivacy).ToString());
                    subxe.SetAttribute("DisplayName", colleague.Policy.DisplayName);
                    subxe.SetAttribute("FilterPrivacyItems", colleague.Policy.FilterPrivacyItems.ToString());
                    subxe.SetAttribute("Group", colleague.Policy.Group);
                    subxe.SetAttribute("PrivacyPolicy", ((int)colleague.Policy.PrivacyPolicy).ToString());
                    subxe.SetAttribute("UserOverridePrivacy", colleague.Policy.UserOverridePrivacy.ToString());
                    xe.AppendChild(subxe);
                    temp.Append(xe.OuterXml);
                }
                temp.Append("</Colleague>");
                xdoc.RemoveAll();
                return temp.ToString();
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting colleagues.", e);
                //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot get My Colleagues. \n error message:{0}", e));
                return string.Empty;
            }
        }

        public void ExportColleagues(IAveBackupStream stream)
        {
            try
            {
                foreach (IAveOColleague colleague in mUserProfile.Colleagues.GetItems())
                {
                    AveColleagueInfo colleageInfo = GetColleagueInfo(colleague);
                    stream.WriteMetadata(AveMetadataType.UserProfileColleague, colleageInfo);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting colleagues.", e);
            }
        }

        public List<AveColleagueInfo> GetColleagues()
        {
            List<AveColleagueInfo> Colleagues = new List<AveColleagueInfo>();
            try
            {
                foreach (IAveOColleague colleague in mUserProfile.Colleagues.GetItems())
                {
                    AveColleagueInfo colleageInfo = GetColleagueInfo(colleague);
                    Colleagues.Add(colleageInfo);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting colleagues.", e);
            }
            return Colleagues;
        }

        public AveColleagueInfo GetColleagueInfo(IAveOColleague colleague)
        {
            string title = "";
            if (colleague.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) != -1)
            {
                title = colleague.Title.Substring(colleague.Title.IndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            }
            else
                title = colleague.Title;
            AveColleagueInfo colleageInfo = new AveColleagueInfo();
            colleageInfo.NameValue = title;
            colleageInfo.AccountName = colleague.Profile.MultiloginAccounts[0];
            colleageInfo.Group = colleague.Group;
            colleageInfo.GroupType = (int)colleague.GroupType;
            colleageInfo.IsInWorkGroup = colleague.IsInWorkGroup;
            colleageInfo.PrivacyLevel = (int)colleague.PrivacyLevel;
            colleageInfo.IsAssistant = colleague.IsAssistant;
            colleageInfo.IsEditable = colleague.IsEditable;
            colleageInfo.IsPrivacyLevelEditable = colleague.IsPrivacyLevelEditable;
            colleageInfo.IsTitleEditable = colleague.IsTitleEditable;
            colleageInfo.IsUrlEditable = colleague.IsUrlEditable;
            colleageInfo.Url = colleague.Url;
            colleageInfo.Title = colleague.Title;

            colleageInfo.Policy = GetPolicyInfo(colleague.Policy);
            return colleageInfo;
        }

        private AvePolicyInfo GetPolicyInfo(IAveOPrivacyPolicyItem policy)
        {
            AvePolicyInfo info = new AvePolicyInfo();
            info.AllowPolicyOverride = policy.AllowPolicyOverride;
            info.DefaultPrivacy = (int)policy.DefaultPrivacy;
            info.DisplayName = policy.DisplayName;
            info.FilterPrivacyItems = policy.FilterPrivacyItems;
            info.Group = policy.Group;
            info.PrivacyPolicy = (int)policy.PrivacyPolicy;
            info.UserOverridePrivacy = policy.UserOverridePrivacy;
            return info;
        }

        public string GetTagsXml()
        {
            try
            {
                StringBuilder temp = new StringBuilder("<Tags>");
                XmlDocument xDoc = new XmlDocument();
                IAveOSocialTagManager tagManager = mParentSite.ObjectModelFactory.CreateSocialTagManager(mContext);
                foreach (IAveOSocialTag tag in tagManager.GetTags(mUserProfile))
                {
                    XmlElement e = xDoc.CreateElement("Tag");
                    e.SetAttribute("NameValue", tag.Title);
                    e.SetAttribute("IsPrivate", tag.IsPrivate.ToString());
                    e.SetAttribute("Url", tag.Url.ToString());
                    e.SetAttribute("termName", tag.Term.Name);
                    temp.Append(e.OuterXml);
                    e.RemoveAll();
                }
                temp.Append("</Tags>");
                xDoc.RemoveAll();
                return temp.ToString();
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, string.Format("Cannot get tags.\n error message:{0}", e));
                return string.Empty;
            }
        }

        public void ExportTags(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportTags"))
            {
                try
                {
                    IAveOSocialTagManager tagManager = mParentSite.ObjectModelFactory.CreateSocialTagManager(mContext);
                    foreach (IAveOSocialTag tag in tagManager.GetTags(mUserProfile))
                    {
                        AveSocialTagInfo dtInfo = GetSocialTagInfo(tag);
                        stream.WriteMetadata(AveMetadataType.UserProfileTag, dtInfo);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, string.Format("Cannot get tags.\n error message:{0}", e));
                }
            }
        }

        public List<AveSocialTagInfo> GetTags()
        {
            List<AveSocialTagInfo> tags = new List<AveSocialTagInfo>();
            try
            {
                IAveOSocialTagManager tagManager = mParentSite.ObjectModelFactory.CreateSocialTagManager(mContext);
                foreach (IAveOSocialTag tag in tagManager.GetTags(mUserProfile))
                {
                    AveSocialTagInfo dtInfo = GetSocialTagInfo(tag);
                    tags.Add(dtInfo);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.ERROR, string.Format("Cannot get tags.\n error message:{0}", e));
            }
            return tags;
        }

        public AveSocialTagInfo GetSocialTagInfo(IAveOSocialTag tag)
        {
            AveSocialTagInfo dtInfo = new AveSocialTagInfo();
            dtInfo.ProfileManagerUrl = mUserProfileManager.MySiteHostUrl;//sitecollection 级别userprofile service还原需要替换url；ADO-33630；
            dtInfo.Url = tag.Url.ToString();
            dtInfo.Title = tag.Title;
            dtInfo.Owner = tag.OwnerName;
            dtInfo.LastModifiedTime = tag.LastModifiedTime;
            dtInfo.Term = new AveTermInfo();
            dtInfo.Term.Owner = tag.Term.Owner;
            dtInfo.IsPrivate = tag.IsPrivate;

            dtInfo.Term.Id = tag.Term.ID;
            dtInfo.Term.TermName = tag.Term.Name;
            dtInfo.Term.IsRoot = tag.Term.IsRoot;
            dtInfo.Term.IsKeyword = tag.Term.IsKeyword;
            dtInfo.Term.SourceTermId = tag.Term.SourceTerm.ID;
            dtInfo.Term.SourceTermName = tag.Term.SourceTerm.Name;
            dtInfo.Term.IsAvailableForTagging = tag.Term.IsAvailableForTagging;
            dtInfo.Term.MergedTermIds = tag.Term.MergedTermIds;

            return dtInfo;
        }

        public string GetNotesXml()
        {
            try
            {
                StringBuilder temp = new StringBuilder("<Notes>");
                XmlDocument xDoc = new XmlDocument();
                IAveOSocialCommentManager manager = mParentSite.ObjectModelFactory.CreateSocialCommentManager(mContext);
                foreach (IAveOSocialComment comment in manager.GetComments(mUserProfile))
                {
                    XmlElement e = xDoc.CreateElement("Note");
                    e.SetAttribute("NameValue", comment.Comment);
                    e.SetAttribute("IsHighPriority", comment.IsHighPriority.ToString());
                    e.SetAttribute("Url", comment.Url.ToString());
                    temp.Append(e.OuterXml);
                    e.RemoveAll();
                }
                temp.Append("</Notes>");
                xDoc.RemoveAll();
                return temp.ToString();
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting notes.", e);
                //mLog.Log(AveLogLevel.ERROR, string.Format("Cannot get Notes. \n error message:{0}", e));
                return string.Empty;
            }
        }

        public void ExportNotes(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportNotes"))
            {
                try
                {
                    IAveOSocialCommentManager manager = mParentSite.ObjectModelFactory.CreateSocialCommentManager(mContext);
                    foreach (IAveOSocialComment comment in manager.GetComments(mUserProfile))
                    {
                        AveSocialCommentInfo info = GetSocialCommentInfo(comment);
                        stream.WriteMetadata(AveMetadataType.UserProfileComment, info);
                    }
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while getting notes.", e);
                }
            }
        }

        public List<AveSocialCommentInfo> GetNotes()
        {
            List<AveSocialCommentInfo> comments = new List<AveSocialCommentInfo>();
            try
            {
                IAveOSocialCommentManager manager = mParentSite.ObjectModelFactory.CreateSocialCommentManager(mContext);
                foreach (IAveOSocialComment comment in manager.GetComments(mUserProfile))
                {
                    AveSocialCommentInfo info = GetSocialCommentInfo(comment);
                    comments.Add(info);
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting notes.", e);
            }
            return comments;
        }

        public AveSocialCommentInfo GetSocialCommentInfo(IAveOSocialComment comment)
        {
            AveSocialCommentInfo info = new AveSocialCommentInfo();
            info.ProfileManagerUrl = mUserProfileManager.MySiteHostUrl;//sitecollection 级别userprofile service还原需要替换url；ADO-33630；
            info.Comment = comment.Comment;
            info.IsHighPriority = comment.IsHighPriority;
            info.Owner = comment.Owner.MultiloginAccounts[0];
            info.Url = comment.Url.ToString();
            info.LastModifiedTime = comment.LastModifiedTime;
            info.Title = comment.Title;
            return info;
        }

        public Dictionary<string, string> Export()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.Export"))
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                try
                {
                    foreach (string myProfileListName in mUnPermitEndString)
                    {
                        switch (myProfileListName)
                        {
                            case AveConstants.MY_COLLEAGUES:
                                temp.Add(AveConstants.MY_COLLEAGUES, GetColleaguesXml());
                                break;
                            case AveConstants.MY_DETAILS:
                                temp.Add(AveConstants.MY_DETAILS, GetDetailXml());
                                break;
                            //do not need to backup and restore memberships,remove for Userprofile performance 
                            //case AveConstants.MY_MEMBERSHIPS:
                            //    temp.Add(AveConstants.MY_MEMBERSHIPS, GetMembershipXml());
                            //    break;
                            case AveConstants.MY_NOTES:
                                temp.Add(AveConstants.MY_NOTES, GetNotesXml());
                                break;
                            case AveConstants.MY_TAGS:
                                temp.Add(AveConstants.MY_TAGS, GetTagsXml());
                                break;
                        }
                    }
                    return temp;
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while exporting user profile.", e);
                    //mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while backup user profile list{0}", e));
                    return temp;
                }
            }
        }

        public void ExportAllUsersProfile(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportAllUsersProfile"))
            {
                List<Dictionary<string, Dictionary<string, string>>> collections = new List<Dictionary<string, Dictionary<string, string>>>();
                Dictionary<string, Dictionary<string, string>> collection = new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, string> userProfile = new Dictionary<string, string>();
                foreach (AveUserInfo userInfo in mAveUserInfoList)
                {
                    try
                    {
                        if (mUserProfileManager.UserExists(userInfo.Login))
                        {
                            mUserProfile = mUserProfileManager.GetUserProfile(userInfo.Login);
                            userProfile = this.Export();
                            if (userProfile.Count > 0)
                            {
                                collection.Add(userInfo.Login, userProfile);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("An error occurred while exporting all user profiles. " + userInfo.Login, e);
                        //mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while backup user profile. The user login name :{0}", e, userInfo.Login));
                    }
                }
                collections.Add(collection);
                output.WriteMetadata(AveMetadataType.UserProfile.ToString(), collections);
            }
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.UserProfile.Export"))
            {
                ExportUserProfileSubTypes(output);

                ExportUserProfileProperties(output);

                foreach (AveUserInfo userInfo in mAveUserInfoList)
                {
                    try
                    {
                        if (mUserProfileManager.UserExists(userInfo.Login))
                        {
                            mUserProfile = mUserProfileManager.GetUserProfile(userInfo.Login);
                            AveUserProfileInfo profile = new AveUserProfileInfo();
                            profile.LoginName = userInfo.Login;
                            if (mUserProfile.ProfileSubType != null)
                            {
                                profile.SubTypeName = mUserProfile.ProfileSubType.Name;
                            }                            
                            profile.Colleagues = GetColleagues();
                            profile.Properties = GetDetails();
                            //do not need to backup and restore memberships,remove for Userprofile performance 
                            //profile.Memberships = GetMemberships();
                            profile.Comments = GetNotes();
                            profile.Tags = GetTags();
                            profile.Links = GetLinks();
                            //backup feed at web level
                            //profile.Feeds = GetFeeds(userInfo.Login);
                            //It seems we don't need to backup the followed information separately.
                            //profile.Followed = GetFollowed(); 

                            profile.Ratings = GetRatings();

                            output.WriteMetadata(AveMetadataType.UserProfile, profile);
                            //ExportColleagues(output);
                            //ExportDetails(output);
                            //ExportMemberships(output);
                            //ExportNotes(output);
                            //ExportTags(output);
                            //ExportLinks(output);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("An error occurred while exporting all user profiles. " + userInfo.Login, e);
                        //mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while backup user profile. The user login name :{0}", e, userInfo.Login));
                    }
                }
            }
        }// add by adrian for 07 item backup

        private void ExportUserProfileSubTypes(IAveBackupStream output)
        {
            if (mParentSite != null)
            {
                var subTypeManager = mParentSite.ObjectModelFactory.CreateProfileSubTypeManager(mContext);
                if (subTypeManager == null)
                {
                    log.Warn("O365 do not support backup user profile sub type");
                    return;
                }
                var subtypes = subTypeManager.GetSubtypesForProfileType(AveProfileType.User).
                    Cast<IAveOProfileSubtype>().Select<IAveOProfileSubtype, AveUserProfileSubTypeInfo>
                    (subtype =>
                        new AveUserProfileSubTypeInfo()
                        {
                            DisplayName = subtype.DisplayName,
                            Name = subtype.Name
                        }
                    ).ToList();
                output.WriteMetadata(AveMetadataType.UserProfileSubTypes, subtypes);
            }
        }

        private List<AveSocialFeedInfo> GetFeeds(string siteOwner)
        {
            try
            {
                List<AveSocialFeedInfo> feedsInfo = new List<AveSocialFeedInfo>();
                if (mParentSite.SPContextKind.IsServerMode13Upper())
                {
                    AveSPSocialFeed feed = new AveSPSocialFeed(siteOwner, mParentSite);
                    feedsInfo = feed.GetSocialFeeds();
                }
                return feedsInfo;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting the new feeds. " + siteOwner, e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Just keep this function in case we need to backup followed information separately in the future.
        /// </summary>
        /// <returns></returns>
        private List<AveSocialActorInfo> GetFollowed()
        {
            try
            {
                AveSPFollowing following = new AveSPFollowing(mUserProfile, mParentSite);
                return following.GetFollowed();
            }
            catch (Exception ex)
            {
                log.Warn(string.Format("An error occurred while getting the new feeds of {0} : {1}", mUserProfile.AccountName, ex.ToString()));
                return null;
            }
        }

        private void ExportLinks(IAveBackupStream output)
        {
            try
            {
                List<AveQuickLinkInfo> quicklinks = new List<AveQuickLinkInfo>();
                foreach (IAveOQuickLink link in mUserProfile.QuickLinks.GetItems())
                {
                    try
                    {
                        AveQuickLinkInfo quicklink = new AveQuickLinkInfo();
                        quicklink.ProfileManagerUrl = mUserProfileManager.MySiteHostUrl;
                        quicklink.PrivacyLevel = (int)link.Policy.PrivacyPolicy;
                        quicklink.Title = link.Title;

                        quicklink.Policy = new AvePolicyInfo();
                        quicklink.Policy.DefaultPrivacy = (int)link.Policy.DefaultPrivacy;
                        quicklink.Policy.AllowPolicyOverride = link.Policy.AllowPolicyOverride;
                        quicklink.Policy.DisplayName = link.Policy.DisplayName;
                        quicklink.Policy.FilterPrivacyItems = link.Policy.FilterPrivacyItems;
                        quicklink.Policy.Group = link.Policy.Group;
                        quicklink.Policy.PrivacyPolicy = (int)link.Policy.PrivacyPolicy;
                        quicklink.Policy.UserOverridePrivacy = link.Policy.UserOverridePrivacy;

                        quicklinks.Add(quicklink);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while get link property:{0}, Exception:{1}", link.Title, e.ToString());
                    }
                }
                if (quicklinks.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.UserProfileLink, quicklinks);
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while export links properties. error:{0}", e.ToString());
            }
        }

        private List<AveQuickLinkInfo> GetLinks()
        {
            List<AveQuickLinkInfo> quicklinks = new List<AveQuickLinkInfo>();
            try
            {
                foreach (IAveOQuickLink link in mUserProfile.QuickLinks.GetItems())
                {
                    try
                    {
                        AveQuickLinkInfo quicklink = new AveQuickLinkInfo();
                        quicklink.ProfileManagerUrl = mUserProfileManager.MySiteHostUrl;
                        quicklink.PrivacyLevel = (int)link.PrivacyLevel;
                        quicklink.Title = link.Title;
                        quicklink.Url = link.Url;
                        quicklink.Group = link.Group;
                        quicklink.GroupType = link.GroupType;

                        quicklink.Policy = new AvePolicyInfo();
                        quicklink.Policy.DefaultPrivacy = (int)link.Policy.DefaultPrivacy;
                        quicklink.Policy.AllowPolicyOverride = link.Policy.AllowPolicyOverride;
                        quicklink.Policy.DisplayName = link.Policy.DisplayName;
                        quicklink.Policy.FilterPrivacyItems = link.Policy.FilterPrivacyItems;
                        quicklink.Policy.Group = link.Policy.Group;
                        quicklink.Policy.PrivacyPolicy = (int)link.Policy.PrivacyPolicy;
                        quicklink.Policy.UserOverridePrivacy = link.Policy.UserOverridePrivacy;

                        quicklinks.Add(quicklink);
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while get link property:{0}, Exception:{1}", link.Title, e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while export links properties. error:{0}", e.ToString());
            }
            return quicklinks;
        }

        public List<AveSOcialRatingInfo> GetRatings()
        {
            List<AveSOcialRatingInfo> rates = new List<AveSOcialRatingInfo>();
            try
            {
                IAveOSocialRatingManager manager = mParentSite.ObjectModelFactory.CreateSocialRatingManager(mContext);
                if (manager != null)
                {
                    string siteUrl = mParentSite.SPSite.Url;
                    var ratings = manager.GetRatings(mUserProfile);
                    if (ratings != null)
                    {
                        rates = ratings.Where(rating => rating.Url.ToString().StartsWith(siteUrl, StringComparison.OrdinalIgnoreCase))
                            .Select(rating => GetSocialRatingInfo(rating)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting ratings. Error: {0}", e);
            }
            return rates;
        }

        private AveSOcialRatingInfo GetSocialRatingInfo(IAveOSocialRating rating)
        {
            AveSOcialRatingInfo info = new AveSOcialRatingInfo();
            info.LastModifiedTime = rating.LastModifiedTime;
            info.Owner = rating.Owner.MultiloginAccounts[0];
            info.Rating = rating.Rating;
            info.Title = rating.Title;
            info.Url = rating.Url.ToString();
            return info;
        }

        private void ExportUserProfileProperties(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPUserProfile.ExportUserProfileProperties"))
            {
                try
                {
                    List<AvePropertyInfo> userProfileProperties = new List<AvePropertyInfo>();
                    IAveOPropertyCollection propertiesWithSection = mUserProfileManager.PropertiesWithSection;
                    if (propertiesWithSection == null)
                    {
                        return;
                    }
                    foreach (IAveOProperty prop in mUserProfileManager.PropertiesWithSection)
                    {
                        AvePropertyInfo property = new AvePropertyInfo();
                        try
                        {
                            property.AllowPolicyOverride = prop.AllowPolicyOverride;
                            property.DefaultPrivacy = (int)prop.DefaultPrivacy;
                            property.Description = prop.Description;
                            property.DescriptionLocalized = prop.DescriptionLocalized.DefaultLanguage;
                            property.DisplayName = prop.DisplayName;
                            property.DisplayNameLocalized = prop.DisplayNameLocalized.DefaultLanguage;
                            property.DisplayOrder = prop.DisplayOrder;
                            property.IsAdminEditable = prop.IsAdminEditable;
                            property.IsAlias = prop.IsAlias;
                            property.IsColleagueEventLog = prop.IsColleagueEventLog;
                            property.IsImported = prop.IsImported;
                            property.IsMultivalued = prop.IsMultivalued;
                            property.IsReplicable = prop.IsReplicable;
                            property.IsRequired = prop.IsRequired;
                            property.IsSearchable = prop.IsSearchable;
                            property.IsSection = prop.IsSection;
                            property.IsSystem = prop.IsSystem;
                            property.IsUpgrade = prop.IsUpgrade;
                            property.IsUpgradePrivate = prop.IsUpgradePrivate;
                            property.IsUserEditable = prop.IsUserEditable;
                            property.IsVisibleOnEditor = prop.IsVisibleOnEditor;
                            property.IsVisibleOnViewer = prop.IsVisibleOnViewer;
                            property.Length = prop.Length;
                            property.ManagedPropertyName = prop.ManagedPropertyName;
                            property.MaximumShown = prop.MaximumShown;
                            property.Name = prop.Name;
                            property.PrivacyPolicy = (int)prop.PrivacyPolicy;
                            property.Separator = (int)prop.Separator;
                            property.Type = prop.Type;
                            property.SubtypeName = prop.SubtypeName;                        
                            if (prop.URI != null)
                            {
                                property.URI = prop.URI.ToString();
                            }
                            property.UserOverridePrivacy = prop.UserOverridePrivacy;
                            userProfileProperties.Add(property);
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while get userProfile property:{0}, Exception:{1}", prop.Name, e.ToString());
                        }
                    }
                    if (userProfileProperties.Count > 0)
                    {
                        output.WriteMetadata(AveMetadataType.UserProfileProperties, userProfileProperties);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while export userProfile properties. error:{0}", e.ToString());
                }
            }
        }

        private static bool HasFlag(long val, long flag)
        {
            return (val & flag) == flag;
        }
    }
}