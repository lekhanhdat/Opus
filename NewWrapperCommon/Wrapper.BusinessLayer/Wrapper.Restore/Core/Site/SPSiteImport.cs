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


using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.Internal;
using AvePoint.Wrapper.Core.Internal.Restore;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Restore.Core
{
    class SPSiteImport : RestoreController<SPSiteRestoreOption, ISPSiteImportProfiler, SPFileRestoreReport>, ISPSiteImport
    {
        private readonly bool loaded;
        private readonly string webApplicationUrl;
        private readonly string url;
        private readonly Wrapper.Core.Common.WrapperSPMode spMode;
        private readonly Wrapper.Core.Common.O365AccountInfo accountInfo;
        private readonly IWrapperDeploymentAPI deploymentAPI;
        private readonly ISiteImport siteImport;
        private readonly IImportObjectManager objectManager;
        private bool eventReceiverFiringDisabled = false;

        private bool IsNewCreated
        {
            get;
            set;
        }
        private bool IsMySite
        {
            get;
            set;
        }
        public IWrapperDeploymentAPI DeploymentAPI
        {
            get { return deploymentAPI; }
        }

        public ISiteImport SiteImport
        {
            get { return siteImport; }
        }

        public IImportObjectManager ObjectManager
        {
            get { return objectManager; }
        }

        public IUserMapping UserMapping { get { return objectManager.UserMapping; } set { objectManager.UserMapping = value; } }

        public ILanguageMappingController LanguageMappingController { get { return objectManager.LanguageMappingController; } private set { objectManager.LanguageMappingController = value; } }

        public ITemplateMapping TemplateMapping { get; set; }

        public bool EventReceiverFiringDisabled
        {
            get { return deploymentAPI.SPEventReceiverFiringDisabled; }
            set
            {
                deploymentAPI.SPEventReceiverFiringDisabled = value;
                eventReceiverFiringDisabled = value;
            }
        }


        public IAveSite SPSite
        {
            get { return this.siteImport.Site; }
        }

        /// <summary>
        /// O365 Side
        /// </summary>
        /// <param name="url"></param>
        /// <param name="spMode"></param>
        /// <param name="accountInfo"></param>
        public SPSiteImport(string url, Wrapper.Core.Common.O365AccountInfo accountInfo)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            if (accountInfo == null)
            {
                throw new ArgumentNullException("accountInfo");
            }

            this.url = url;
            this.spMode = WrapperSPMode.O365;
            this.accountInfo = accountInfo;
            this.deploymentAPI = WrapperFactory.GetWrapperDeploymentAPI(spMode);
            this.objectManager = WrapperUtil.CreateDefaultObjectManager();
            this.siteImport = this.deploymentAPI.CreateSiteImport(url, accountInfo, objectManager);
            this.LanguageMappingController = WrapperUtil.CreateDefaultLanguageMappingController(spMode);
            /*
             * 此处调用的目的是为了能够load site collection
             */
            this.loaded = this.siteImport.LoadSiteCollection();
        }

        /// <summary>
        /// Server Side
        /// </summary>
        /// <param name="webApplicationUrl"></param>
        /// <param name="url"></param>
        public SPSiteImport(string webApplicationUrl, string url)
        {
            if (string.IsNullOrEmpty(webApplicationUrl))
            {
                throw new ArgumentNullException("webApplicationUrl");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            this.webApplicationUrl = webApplicationUrl;
            this.url = url;
            this.spMode = WrapperSPMode.Server;
            this.deploymentAPI = WrapperFactory.GetWrapperDeploymentAPI(spMode);
            this.objectManager = WrapperUtil.CreateDefaultObjectManager();
            this.siteImport = this.deploymentAPI.CreateSiteImport(webApplicationUrl, url, objectManager);
            this.LanguageMappingController = WrapperUtil.CreateDefaultLanguageMappingController(spMode);
            /*
             * 此处调用的目的是为了能够load site collection
             */
            this.loaded = this.siteImport.LoadSiteCollection();
        }

        /// <summary>
        /// for clone another site import object
        /// </summary>
        private SPSiteImport(bool loaded, string webApplicationUrl, string url,
            Wrapper.Core.Common.WrapperSPMode spMode, Wrapper.Core.Common.O365AccountInfo accountInfo, IWrapperDeploymentAPI deploymentAPI,
            ISiteImport siteImport, IImportObjectManager objectManager)
        {
            this.loaded = loaded;
            this.webApplicationUrl = webApplicationUrl;
            this.url = url;
            this.spMode = spMode;
            this.accountInfo = accountInfo;
            this.deploymentAPI = deploymentAPI;
            this.siteImport = siteImport;
            this.objectManager = objectManager;
        }

        internal void Close()
        {
            this.LanguageMappingController.CleanLanguageFile();

            if (this.siteImport != null)
            {
                this.siteImport.Dispose();
            }
        }

        public void Dispose()
        {
            this.objectManager.PostActionManager.ExecutePostActions(siteImport);
            Close();
        }

        protected override Action<IAveRestoreStream, AveMetadata, SPSiteRestoreOption, ISPSiteImportProfiler> GetMetadataRestoreAction(AveMetadataType metadataType)
        {
            Action<IAveRestoreStream, AveMetadata, SPSiteRestoreOption, ISPSiteImportProfiler> action = null;

            switch (metadataType)
            {
                case AveMetadataType.SiteBasicInfo:
                    action = RestoreSiteBasicInfo;
                    break;
                case AveMetadataType.SiteProperty:
                    action = RestoreSiteProperty;
                    break;
                case AveMetadataType.SiteFeature:
                    action = RestoreSiteFeature;
                    break;
                case AveMetadataType.Users:
                    action = RestoreSiteUsers;
                    break;
                case AveMetadataType.Groups:
                    action = RestoreSiteGroups;
                    break;
                case AveMetadataType.AudienceCache:
                    action = RestoreAudience;
                    break;
                case AveMetadataType.LanguageFile:
                    action = RestoreLanguageFile;
                    break;
                case AveMetadataType.SiteSearchInfo:
                    action = RestoreSiteSearchInfo;
                    break;
                case AveMetadataType.MetadataService:
                    action = RestoreMetadataService;
                    break;
                #region ###########UserProfile###########
                case AveMetadataType.UserProfileSubTypes:
                    action = RestoreUserProfileSubTypes;
                    break;
                case AveMetadataType.UserProfileProperties:
                    action = RestoreUserProfileProperties;
                    break;
                case AveMetadataType.UserProfile:
                    action = RestoreUserProfile;
                    break;
                case AveMetadataType.UserProfileDetail:
                    action = RestoreUserProfileDetails;
                    break;
                case AveMetadataType.UserProfileColleague:
                    action = RestoreUserProfileColleague;
                    break;
                case AveMetadataType.UserProfileMembership:
                    action = RestoreUserProfileMembership;
                    break;
                case AveMetadataType.UserProfileComment:
                    action = RestoreUserProfileComment;
                    break;
                case AveMetadataType.UserProfileTag:
                    action = RestoreUserProfileTag;
                    break;
                    break;
                #endregion
                default:
                    break;
            }

            return action;
        }

        private void RestoreUserProfileSubTypes(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var subTypes = metadata.GetMetadata<List<AveUserProfileSubTypeInfo>>();

                if (subTypes != null)
                {
                    siteImport.RestoreUserProfileSubTypes(subTypes, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreMetadataService(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.ManagedMetadataOption.RestoreType != SPManagedMetadataRestoreType.None)
            {
                var termStoreInfos = metadata.GetMetadata<List<AveTermStoreInfo>>();

                if (termStoreInfos != null)
                {
                    siteImport.RestoreMetadataService(termStoreInfos, restoreOption.ManagedMetadataOption, profiler);
                }
            }
        }

        private void RestoreUserProfileMembership(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            //Do nothings
        }

        private void RestoreUserProfileTag(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var socialTagInfo = metadata.GetMetadata<AveSocialTagInfo>();

                if (socialTagInfo != null)
                {
                    siteImport.RestoreSocialTag(socialTagInfo, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreUserProfileColleague(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var colleagues = metadata.GetMetadata<List<Wrapper.Common.Office.AveColleagueInfo>>();

                if (colleagues != null)
                {
                    siteImport.RestoreUserProfileColleagues(colleagues, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreUserProfileDetails(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var userProfileDetails = metadata.GetMetadata<List<Wrapper.Common.Office.AveUserProfileValueInfo>>();

                if (userProfileDetails != null)
                {
                    siteImport.RestoreUserProfileDetails(userProfileDetails, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreUserProfileComment(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var socialCommentInfo = metadata.GetMetadata<AveSocialCommentInfo>();

                if (socialCommentInfo != null)
                {
                    siteImport.RestoreSocialComment(socialCommentInfo, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreUserProfile(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var userProfileInfo = metadata.GetMetadata<AveUserProfileInfo>();

                if (userProfileInfo != null)
                {
                    siteImport.RestoreUserProfile(userProfileInfo, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreUserProfileProperties(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (IsMySite || restoreOption.UserProfileOption.RestoreUserProfile)
            {
                var userProfileProperties = metadata.GetMetadata<List<AvePropertyInfo>>();

                if (userProfileProperties != null)
                {
                    siteImport.RestoreUserProfileProperties(userProfileProperties, restoreOption.UserProfileOption, profiler);
                }
            }
        }

        private void RestoreSiteSearchInfo(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (NeedRestore(restoreOption.RestoreAction) && restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                var searchInfo = metadata.GetMetadata<AveSearchInfo>();

                if (searchInfo != null)
                {
                    siteImport.RestoreSearchInfo(searchInfo, restoreOption.ConfigurationRestoreOption, profiler);
                }
            }
        }

        private void RestoreAudience(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            var audiences = metadata.GetMetadata<Dictionary<string, string>>();

            if (audiences != null && audiences.Count > 0)
            {
                siteImport.RestoreAudience(audiences, restoreOption.UserProfileOption, profiler);
            }
        }

        private void RestoreLanguageFile(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            //no need to restore language file, will load mapping from xml file
        }

        /// <summary>
        /// Case:
        /// 1. Group owner如果是其他group，则还原不了的问题。
        /// 2. Group setting, membership
        /// 3. Group Title & Language mapping
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="metadata"></param>
        /// <param name="restoreOption"></param>
        /// <param name="profiler"></param>
        internal void RestoreSiteGroups(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPImportProfiler profiler)
        {
            var groups = metadata.GetMetadata<List<AveGroupInfo>>();

            if (groups != null && groups.Count > 0)
            {
                if (NeedRestore(restoreOption.RestoreAction) && restoreOption.SecurityRestoreOption.RestoreSecurity)
                {
                    if (restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessGroupInfoBrforeRestore != null)
                    {
                        groups = restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessGroupInfoBrforeRestore(groups);
                    }
                    if (restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
                    {
                        foreach (var group in groups)
                        {
                            group.Members = restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(group.Members);
                            group.Memberships.Clear();
                            group.Members.ForEach(userInfo => group.Memberships.Add(userInfo.ID));
                        }
                    }
                    siteImport.RestoreGroups(groups, restoreOption.SecurityRestoreOption, profiler);
                }
                else
                {
                    objectManager.GroupManager.CacheGroups(groups);
                }
            }
        }

        /// <summary>
        /// Case:
        /// 1. 不同类型的claims到非claims的转移。
        /// 2. FBA用户。
        /// 3. Regional setting的还原。
        /// 4. deleted的用户
        /// 5. place holder
        /// 6. user mapping
        /// 
        /// 
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="metadata"></param>
        /// <param name="restoreOption"></param>
        /// <param name="profiler"></param>
        internal void RestoreSiteUsers(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPImportProfiler profiler)
        {
            var users = metadata.GetMetadata<List<AveUserInfo>>();

            if (users != null && users.Count > 0)
            {
                if (NeedRestore(restoreOption.RestoreAction) && restoreOption.SecurityRestoreOption.RestoreSecurity)
                {
                    if (restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
                    {
                        users = restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(users);
                    }
                    siteImport.RestoreUsers(users, restoreOption.SecurityRestoreOption, profiler);
                }
                else
                {
                    objectManager.UserManager.CacheUsers(users);
                }
            }
        }

        private void RestoreSiteFeature(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (NeedRestore(restoreOption.RestoreAction) && restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var featureInfo = metadata.GetMetadata<AveFeatureInfoBox>();

                    if (featureInfo.FeatureList != null && featureInfo.FeatureList.Count > 0)
                    {
                        siteImport.RestoreFeatures(featureInfo, restoreOption.ConfigurationRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = url, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Feature }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Feature, Status = WrapperRestoreStatus.Skipped, Url = url }); }
            }
        }

        /// <summary>
        /// TODO:
        /// 
        /// Case:
        /// 1. 各种setting的还原
        /// 
        /// Limitations
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="metadata"></param>
        /// <param name="restoreOption"></param>
        /// <param name="restoreProfiler"></param>
        private void RestoreSiteProperty(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            if (NeedRestore(restoreOption.RestoreAction) && restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var settingInfo = metadata.GetMetadata<AveSiteSettingInfo>();

                    siteImport.RestoreSettings(settingInfo, restoreOption.ConfigurationRestoreOption, profiler);
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = url, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Setting }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Setting, Status = WrapperRestoreStatus.Skipped, Url = url }); }
            }
        }

        /// <summary>
        /// TODO: 
        /// 1. 以前的case中，如果创建出来之后，会去remove user name以及role assignments
        /// 
        /// Case:
        /// 1. 如果目的端是Host Header
        /// 2. 如果目的端之前存在，但是后来被删除了
        /// 3. 如果目的端一直存在。
        /// 4. 如果目的端 site collection对应的host header被使用过。
        /// 5. 如果template找不到，retry到default template？
        /// 6. 如果owner name为空，使用当前用户。
        /// 7. 如果是my site，需要通过第三方程序来创建，因为权限问题。
        /// 8. 如果对应managed path不存在，则提出对应的错误。
        /// 9. 如果存在/目录的Site Collection，那么数据可能会指向/的site collection，而不是想还原的site collection。
        /// 
        /// 
        /// </summary>
        /// <param name="restoreStream"></param>
        /// <param name="metadata"></param>
        /// <param name="restoreOption"></param>
        /// <param name="restoreProfiler"></param>
        private void RestoreSiteBasicInfo(IAveRestoreStream restoreStream, AveMetadata metadata, SPSiteRestoreOption restoreOption, ISPSiteImportProfiler profiler)
        {
            try
            {
                var siteInfo = metadata.GetMetadata<AveSiteInfo>();

                if (!this.loaded)
                {
                    //如果外围不想创建site collection，则通过这个参数来控制
                    if (restoreOption.RestoreAction != SPContainerRestoreAction.None)
                    {
                        siteInfo.WebTemplate = TemplateMapping.GetMappingSiteTemplateNameEx(siteInfo.WebTemplate);

                        if (siteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                        {
                            var siteCreationParameters = ConvertSiteInfoToPersonalSiteCreationParameters(siteInfo, restoreOption);

                            if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_CreateMySiteInfo, siteCreationParameters.OwnerLogin, siteCreationParameters.LCID), Url = url, Status = WrapperRestoreStatus.None, Type = SPObjectType.Self }); }

                            siteImport.CreatePersonalSite(siteCreationParameters);
                            IsMySite = true;
                        }
                        else
                        {
                            var siteCreationParameters = ConvertSiteInfoToCreationParameters(siteInfo, restoreOption);

                            if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_CreateSiteInfo, siteCreationParameters.Title, siteCreationParameters.Description, siteCreationParameters.Url, siteCreationParameters.IsHostHeader, siteCreationParameters.Template, siteCreationParameters.OwnerLogin, siteCreationParameters.SecondaryContactLogin, siteCreationParameters.CompatibilityLevel, siteCreationParameters.LCID, siteCreationParameters.WebApplicationUrl), Url = url, Status = WrapperRestoreStatus.None, Type = SPObjectType.Self }); }

                            siteImport.CreateSiteCollection(siteCreationParameters);

                            if (restoreOption.CleanDefaultSPObjects)
                            {
                                siteImport.CleanDefaultSPObjects();
                            }
                        }
                        IsNewCreated = true;
                    }
                    else
                    {
                        throw new FileNotFoundException(
                            WrapperResource.GetString(WrapperResourceKey.Wrapper_SiteNotFound, this.url));
                    }
                }
            }
            catch (Exception ex)
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = url, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Self }); }
                throw;
            }
        }

        /// <summary>
        /// 创建Personal Site Creation Parameters
        /// </summary>
        /// <param name="siteInfo"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        private PersonalSiteCreationParameters ConvertSiteInfoToPersonalSiteCreationParameters(AveSiteInfo siteInfo, SPSiteRestoreOption restoreOption)
        {
            var siteCreationParameters = new PersonalSiteCreationParameters();

            if (string.IsNullOrEmpty(restoreOption.SpecialSiteCreationAccount))
            {
                siteCreationParameters.OwnerLogin = UserMapping.GetMappingLoginNameEx(siteInfo.OwnerLogin);
            }
            else
            {
                siteCreationParameters.OwnerLogin = restoreOption.SpecialSiteCreationAccount;
            }
            siteCreationParameters.LCID = LanguageMappingController.GetMappingLCID(siteInfo.LCID);

            return siteCreationParameters;
        }

        /// <summary>
        /// TODO:
        /// 1. all web templates没有使用，需要了解下。
        /// 2. 创建site到指定的content db中，这个没有加，需要问问还需要不。
        /// 3. 没有Load FBA配置文件，需要测试下不load是否存在问题？
        /// 
        /// Case:
        /// 1. 判断HostHeader逻辑。
        /// 2. 获取template 的mapping
        /// 3. 获取own login和secondary login的mapping
        /// 4. 获取LCID的mapping
        /// </summary>
        /// <param name="siteInfo"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        private SiteCreationParameters ConvertSiteInfoToCreationParameters(AveSiteInfo siteInfo,
                                                                           SPSiteRestoreOption restoreOption)
        {
            var siteCreationParameters = new SiteCreationParameters();
            siteCreationParameters.Title = siteInfo.Title;
            siteCreationParameters.Description = siteInfo.Description;
            siteCreationParameters.CompatibilityLevel = siteInfo.CompatibilityLevel;
            siteCreationParameters.Url = this.url;
            siteCreationParameters.WebApplicationUrl = this.webApplicationUrl;
            siteCreationParameters.IsHostHeader = IsHostHeader();
            siteCreationParameters.Template = siteInfo.WebTemplate;
            //由于上层处理过了，所以不需要再次处理了。效率考虑
            //restoreOption.TemplateMapping.GetMappingTemplateNameEx(siteInfo.WebTemplate);

            var index = siteCreationParameters.Template.IndexOf("#", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                int templateNumber = 0;
                if (int.TryParse(siteCreationParameters.Template.Substring(index + 1), out templateNumber))
                {
                    if (templateNumber < 0)
                    {
                        logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_InvalidTemplateNumber, siteCreationParameters.Template));
                        siteCreationParameters.Template = string.Empty;
                    }
                }
            }

            siteCreationParameters.LCID = LanguageMappingController.GetMappingLCID(siteInfo.LCID);

            siteCreationParameters.ContentDBId = restoreOption.ContentDBId;

            if (!string.IsNullOrEmpty(restoreOption.SpecialSiteCreationAccount))
            {
                siteCreationParameters.OwnerName = restoreOption.SpecialSiteCreationAccount;
                siteCreationParameters.OwnerLogin = restoreOption.SpecialSiteCreationAccount;
                siteCreationParameters.OwnerEmail = null;
                siteCreationParameters.SecondaryContactLogin = null;
                siteCreationParameters.SecondaryContactName = null;
                siteCreationParameters.SecondaryContactEmail = null;
            }
            else
            {
                if (string.IsNullOrEmpty(siteInfo.OwnerLogin))
                {
                    siteCreationParameters.OwnerLogin = string.Format("{0}\\{1}", Environment.UserDomainName,
                                                                      Environment.UserName);
                    siteCreationParameters.OwnerName = Environment.UserName;
                }
                else
                {
                    var userInfo = GetUserLoginNameAndEmail(siteInfo.OwnerLogin, siteInfo.OwnerName, siteInfo.OwnerEmail, restoreOption);

                    siteCreationParameters.OwnerLogin = userInfo.ItemA;
                    siteCreationParameters.OwnerName = userInfo.ItemB;
                    siteCreationParameters.OwnerEmail = userInfo.ItemC;
                }

                if (!string.IsNullOrEmpty(siteInfo.SecondaryContactLogin))
                {
                    var userInfo = GetUserLoginNameAndEmail(siteInfo.SecondaryContactLogin, siteInfo.SecondaryContactName, siteInfo.SecondaryContactEmail, restoreOption);

                    siteCreationParameters.SecondaryContactLogin = userInfo.ItemA;
                    siteCreationParameters.SecondaryContactName = userInfo.ItemB;
                    siteCreationParameters.SecondaryContactEmail = userInfo.ItemC;
                }
            }

            return siteCreationParameters;
        }

        /// <summary>
        /// 如果LoginName不存在，则默认转换成当前进程用户
        /// </summary>
        /// <param name="login"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="restoreOption"></param>
        /// <returns></returns>
        private Tuple<string, string, string> GetUserLoginNameAndEmail(string login, string name, string email,
                                                                       SPSiteRestoreOption restoreOption)
        {
            string newLogin = login;
            string newName = name;
            string newEmail = email;
            try
            {
                if (!string.IsNullOrEmpty(login))
                {
                    newLogin = UserMapping.GetMappingLoginNameEx(login);

                    Tuple<string, string> nameAndEmail = siteImport.GetUserNameAndEmail(newLogin);
                    if (string.IsNullOrEmpty(nameAndEmail.ItemA))
                    {
                        newLogin = string.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
                        newName = Environment.UserName;
                        newEmail = string.Empty;
                    }
                    else
                    {
                        newName = nameAndEmail.ItemA;
                        newEmail = nameAndEmail.ItemB;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(WrapperResource.GetString(WrapperResourceKey.Wrapper_GetUserNameAndEmailFailed, login, ex));
            }

            return new Tuple<string, string, string>(newLogin, newName, newEmail);
        }

        /// <summary>
        /// 判断是否是HostHeader
        /// </summary>
        /// <returns></returns>
        private bool IsHostHeader()
        {
            if (!string.IsNullOrEmpty(webApplicationUrl))
            {
                string fullUrl = this.url;
                if (this.url[this.url.Length - 1] != '/')
                {
                    fullUrl = string.Format("{0}/", this.url);
                }

                return !fullUrl.StartsWith(this.webApplicationUrl, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool NeedRestore(SPContainerRestoreAction restoreAction)
        {
            return IsNewCreated || restoreAction == SPContainerRestoreAction.Overwrite;
        }

        protected override ISPSiteImportProfiler CreateDefaultProfiler()
        {
            return new DefaultRestoreSiteProfiler();
        }

        protected override SPFileRestoreReport GenerateReport(ISPSiteImportProfiler profiler)
        {
            return profiler.GenerateReport();
        }

        protected override void BeginRestore(SPSiteRestoreOption restoreOption)
        {
        }

        protected override void EndRestore(SPSiteRestoreOption restoreOption)
        {
        }

        /// <summary>
        /// clone the current object
        /// </summary>
        /// <returns></returns>
        internal SPSiteImport Clone()
        {
            var import = new SPSiteImport(loaded, webApplicationUrl, url, spMode, accountInfo, deploymentAPI, siteImport.Clone(), objectManager);

            import.EventReceiverFiringDisabled = eventReceiverFiringDisabled;

            return import;
        }
    }
}
