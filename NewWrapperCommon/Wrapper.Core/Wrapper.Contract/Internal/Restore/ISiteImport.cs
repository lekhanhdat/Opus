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

using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Internal.Restore
{
    public interface ISiteImport : IDisposable
    {
        /// <summary>
        /// Create Site Collection
        /// </summary>
        /// <param name="siteCreationParameters"></param>
        void CreateSiteCollection(SiteCreationParameters siteCreationParameters);

        /// <summary>
        /// Create Personal Site
        /// </summary>
        /// <param name="siteCreationParameters"></param>
        void CreatePersonalSite(PersonalSiteCreationParameters siteCreationParameters);

        /// <summary>
        /// Load Site Collection用来判断site collection是否存在，如果不存在，则需要创建
        /// </summary>
        /// <returns></returns>
        bool LoadSiteCollection();

        /// <summary>
        /// Get User Name and Email
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        Tuple<string, string> GetUserNameAndEmail(string login);

        /// <summary>
        /// Restore Setting
        /// </summary>
        /// <param name="settingInfo"></param>
        /// <param name="spSiteSettingRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreSettings(Wrapper.Common.AveSiteSettingInfo settingInfo, SPRestore.SPSiteConfigurationRestoreOption spSiteSettingRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// 删除site collection中默认的对象，比如groups
        /// 
        /// 请慎重使用，保证是在创建site collection之后调用
        /// </summary>
        void CleanDefaultSPObjects();

        /// <summary>
        /// Restore Portal Url
        /// </summary>
        /// <param name="url"></param>
        void RestorePortalUrl(string url);

        /// <summary>
        /// Restore Features
        /// </summary>
        /// <param name="featureInfo"></param>
        /// <param name="spSiteConfigurationRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreFeatures(Wrapper.Common.AveFeatureInfoBox featureInfo, SPRestore.SPSiteConfigurationRestoreOption spSiteConfigurationRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Restore users
        /// </summary>
        /// <param name="users"></param>
        /// <param name="spSecurityRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreUsers(List<Wrapper.Common.AveUserInfo> users, SPRestore.SPSecurityRestoreOption spSecurityRestoreOption, SPRestore.ISPImportProfiler profiler);

        /// <summary>
        /// Restore Groups
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="spSecurityRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreGroups(List<Wrapper.Common.AveGroupInfo> groups, SPRestore.SPSecurityRestoreOption spSecurityRestoreOption, SPRestore.ISPImportProfiler profiler);

        /// <summary>
        /// Restore Group Owner
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="groupOwnerSourceId"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        bool RestoreGroupOwner(int groupId, int groupOwnerSourceId, string groupName);

        /// <summary>
        /// Restore Audience
        /// </summary>
        /// <param name="audiences"></param>
        /// <param name="spUserProfileRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreAudience(Dictionary<string, string> audiences, SPRestore.SPUserProfileRestoreOption spUserProfileRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Restore Search Info
        /// </summary>
        /// <param name="searchInfo"></param>
        /// <param name="configurationRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreSearchInfo(Wrapper.Common.AveSearchInfo searchInfo, SPRestore.SPSiteConfigurationRestoreOption configurationRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Restore UserProfile SubTypes
        /// </summary>
        /// <param name="subTypes"></param>
        /// <param name="sPUserProfileRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreUserProfileSubTypes(List<Wrapper.Common.Office.AveUserProfileSubTypeInfo> subTypes, SPRestore.SPUserProfileRestoreOption spUserProfileRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Restore userprofile properties
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="configurationRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreUserProfileProperties(List<AvePoint.Wrapper.Common.Office.AvePropertyInfo> properties, SPRestore.SPUserProfileRestoreOption configurationRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// clone the current object with different context
        /// </summary>
        /// <returns></returns>
        ISiteImport Clone();

        /// <summary>
        /// Restore user profile.
        /// 除6.0版本,其他版本都会在此方法中还原detail,tag,membership 等相关信息
        /// </summary>
        /// <param name="userProfileInfo"></param>
        /// <param name="sPUserProfileRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreUserProfile(AvePoint.Wrapper.Common.Office.AveUserProfileInfo userProfileInfo, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// 6.0版本单独还原此属性
        /// </summary>
        /// <param name="userProfileDetails"></param>
        /// <param name="userProfileOption"></param>
        /// <param name="profiler"></param>
        void RestoreUserProfileDetails(List<Wrapper.Common.Office.AveUserProfileValueInfo> userProfileDetails, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// 6.0版本单独还原此属性
        /// </summary>
        /// <param name="userProfileDetails"></param>
        /// <param name="userProfileOption"></param>
        /// <param name="profiler"></param>
        void RestoreUserProfileColleagues(List<Wrapper.Common.Office.AveColleagueInfo> colleagues, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// 6.0版本单独还原此属性
        /// </summary>
        /// <param name="userProfileInfo"></param>
        /// <param name="userProfileOption"></param>
        /// <param name="profiler"></param>
        void RestoreSocialComment(AvePoint.Wrapper.Common.AveSocialCommentInfo socialCommentInfo, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// 6.0版本单独还原此属性
        /// </summary>
        /// <param name="socialTagInfo"></param>
        /// <param name="userProfileOption"></param>
        /// <param name="profiler"></param>
        void RestoreSocialTag(AvePoint.Wrapper.Common.AveSocialTagInfo socialTagInfo, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Restore Metadata Service For Site Level.
        /// </summary>
        /// <param name="termStoreInfos"></param>
        /// <param name="sPManagedMetadataRestoreOption"></param>
        /// <param name="profiler"></param>
        void RestoreMetadataService(List<Wrapper.Common.AveTermStoreInfo> termStoreInfos, SPRestore.SPManagedMetadataRestoreOption sPManagedMetadataRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        /// <summary>
        /// Site 这个对象如果使用的话，记得cache下，因为底层实现的逻辑是每次都封装下API。
        /// </summary>
        Wrapper.Common.IAveSite Site { get; }
    }

    /// <summary>
    /// 建立这个主要是为了以后添加接口的时候不需要改动07代码了。
    /// </summary>
    abstract class BaseSiteImport : ISiteImport
    {
        public abstract void CreateSiteCollection(SiteCreationParameters siteCreationParameters);

        public abstract void CreatePersonalSite(PersonalSiteCreationParameters siteCreationParameters);

        public abstract bool LoadSiteCollection();

        public abstract Tuple<string, string> GetUserNameAndEmail(string login);

        public abstract void RestoreSettings(Wrapper.Common.AveSiteSettingInfo settingInfo, SPRestore.SPSiteConfigurationRestoreOption spSiteSettingRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void CleanDefaultSPObjects();

        public abstract void RestorePortalUrl(string url);

        public abstract void RestoreFeatures(Wrapper.Common.AveFeatureInfoBox featureInfo, SPRestore.SPSiteConfigurationRestoreOption spSiteConfigurationRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreUsers(List<Wrapper.Common.AveUserInfo> users, SPRestore.SPSecurityRestoreOption spSecurityRestoreOption, SPRestore.ISPImportProfiler profiler);

        public abstract void RestoreGroups(List<Wrapper.Common.AveGroupInfo> groups, SPRestore.SPSecurityRestoreOption spSecurityRestoreOption, SPRestore.ISPImportProfiler profiler);

        public abstract bool RestoreGroupOwner(int groupId, int groupOwnerSourceId, string groupName);

        public abstract void RestoreAudience(Dictionary<string, string> audiences, SPRestore.SPUserProfileRestoreOption spUserProfileRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreSearchInfo(Wrapper.Common.AveSearchInfo searchInfo, SPRestore.SPSiteConfigurationRestoreOption configurationRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreUserProfileSubTypes(List<Wrapper.Common.Office.AveUserProfileSubTypeInfo> subTypes, SPRestore.SPUserProfileRestoreOption spUserProfileRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreUserProfileProperties(List<AvePoint.Wrapper.Common.Office.AvePropertyInfo> properties, SPRestore.SPUserProfileRestoreOption userprofileOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract ISiteImport Clone();

        public abstract void RestoreUserProfile(AvePoint.Wrapper.Common.Office.AveUserProfileInfo userProfileInfo, SPRestore.SPUserProfileRestoreOption userProfileRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreUserProfileDetails(List<Wrapper.Common.Office.AveUserProfileValueInfo> userProfileDetails, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreUserProfileColleagues(List<Wrapper.Common.Office.AveColleagueInfo> colleagues, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreSocialComment(Wrapper.Common.AveSocialCommentInfo socialCommentInfo, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreSocialTag(AvePoint.Wrapper.Common.AveSocialTagInfo socialTagInfo, SPRestore.SPUserProfileRestoreOption userProfileOption, SPRestore.ISPSiteImportProfiler profiler);

        public abstract void RestoreMetadataService(List<Wrapper.Common.AveTermStoreInfo> termStoreInfos, SPRestore.SPManagedMetadataRestoreOption sPManagedMetadataRestoreOption, SPRestore.ISPSiteImportProfiler profiler);

        public void Dispose()
        {
            this.Close();
        }

        protected abstract void Close();

        public abstract Wrapper.Common.IAveSite Site { get; }

        protected T CreateInstance<T, TParam>(ref T instance, Func<TParam, T> createMethod, TParam param)
        {
            if (instance == null)
            {
                lock (this)
                {
                    if (instance == null)
                    {
                        instance = createMethod(param);
                    }
                }
            }

            return instance;
        }

        protected T CreateInstance<T>(ref T instance, Func<T> createMethod)
        {
            if (instance == null)
            {
                lock(this)
                {
                    if (instance == null)
                    {
                        instance = createMethod();
                    }
                }
            }

            return instance;
        }
        
    }
}
