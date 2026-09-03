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
using AvePoint.Wrapper.Core.Internal.Restore;
using AvePoint.Wrapper.Core.SPRestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.Common;
using System.IO;
using System.Threading;
using AvePoint.GCommon;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Restore.Core;

namespace AvePoint.Wrapper.Restore.Core
{
    class SPWebImport : RestoreController<SPWebRestoreOption, ISPWebImportProfiler, SPFileRestoreReport>, ISPWebImport
    {
        private bool exists;
        private bool isNewCreated;
        private readonly SPSiteImport restoreSite;
        private readonly string siteRelativeURL;
        private readonly IWebImport webImport;
        private WebSourceInfo sourceWebInfo = new WebSourceInfo();

        /// <summary>
        /// 必须要保证是SPWebImport，不能和以前的code混用
        /// </summary>
        /// <param name="restoreSite"></param>
        /// <param name="siteRelativeURL">如“http://w03aio-02x64:1000/sites/sub1/sub2”，root web url是'',sub1 是“sub1”，sub2 是“sub1/sub2”,与Title没有关系</param>
        /// <param name="cloneDependence"></param>
        public SPWebImport(SPSiteImport restoreSite, string siteRelativeURL)
        {
            if (restoreSite == null)
            {
                throw new ArgumentNullException("SPWebImport_restoreSite");
            }
            if (siteRelativeURL == null)
            {
                throw new ArgumentNullException("SPWebImport_siteRelativeURL");
            }

            this.restoreSite = restoreSite;
            this.siteRelativeURL = siteRelativeURL;
            this.webImport = this.restoreSite.DeploymentAPI.CreateWebImport(this.restoreSite.SiteImport, siteRelativeURL);
            this.exists = this.webImport.LoadWeb();
        }

        protected override Action<Common.IAveRestoreStream, Common.AveMetadata, SPWebRestoreOption, ISPWebImportProfiler> GetMetadataRestoreAction(AveMetadataType metadataType)
        {
            switch (metadataType)
            {
                case Common.AveMetadataType.WebBasicInfo:
                    return RestoreWebBasicInfo;
                case Common.AveMetadataType.WebProperty:
                    return RestoreWebProperties;
                case Common.AveMetadataType.WebContentType:
                    return RestoreWebContentTypes;
                case Common.AveMetadataType.WebField:
                    return RestoreWebFields;
                case Common.AveMetadataType.WebFeature:
                    return RestoreWebFeatures;
                case Common.AveMetadataType.Navigation:
                    return RestoreWebNavigations;
                case Common.AveMetadataType.LanguageFile:
                    return RestoreWebLanguageFile;
                case Common.AveMetadataType.WebEventReceiver:
                    return RestoreWebEventReceiver;
                case Common.AveMetadataType.WebProjectPolicy:
                    break;
                case Common.AveMetadataType.WebWorkflowAssociation:
                    return RestoreWebWorkflowAssociation;
                case Common.AveMetadataType.WebCTWorkflowAssociation:
                    return RestoreWebCTWorkflowAssociation;
                case Common.AveMetadataType.WebWorkflowInstance:
                    return RestoreWebWorkflowInstance;
                case Common.AveMetadataType.WebWorkflowSchedule:
                    return RestoreWebWorkflowSchedule;
                case Common.AveMetadataType.WebWorkflowTemplate:
                    return RestoreWebWorkflowTemplate;
                case Common.AveMetadataType.Users:
                    return RestoreWebUsers;
                case Common.AveMetadataType.Groups:
                    return RestoreWebGroups;
                case Common.AveMetadataType.Roles:
                    return RestoreWebRoles;
                case Common.AveMetadataType.RoleAssignment:
                    return RestoreWebRoleAssignments;
            }

            return null;
        }

        private void RestoreWebFields(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                var fieldSchemaXml = string.Empty;
                if (restoreOption.ConfigurationRestoreOption.FieldRestoreAction != SPObjectRestoreAction.Skip)
                {
                    fieldSchemaXml = metadata.GetMetadata<string>();
                    if (restoreOption.ConfigurationRestoreOption.ProcessFieldAction != null)
                    {
                        fieldSchemaXml = restoreOption.ConfigurationRestoreOption.ProcessFieldAction(fieldSchemaXml);
                    }
                }
                webImport.RestoreWebFields(fieldSchemaXml, restoreOption.ConfigurationRestoreOption, profiler);

                
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Field, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebContentTypes(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            //throw new NotImplementedException();
        }

        private void RestoreWebRoleAssignments(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.SecurityRestoreOption.RestoreSecurity)
            {
                try
                {
                    var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                    if (roleAssignments != null && roleAssignments.Count > 0)
                    {
                        if (restoreOption.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
                        {
                            roleAssignments=restoreOption.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
                        }
                        //对备份数据和option进行处理
                        webImport.RestoreRoleAssignments(new AveRoleAssignmentInfoList() { SourceHasUniqueRoleAssignments = sourceWebInfo.HasUniqueRoleAssignments, RoleAssignmentInfos = roleAssignments }, restoreOption.SecurityRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.RoleAssignment }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.RoleAssignment, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
            
        }


        private void RestoreWebRoles(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.SecurityRestoreOption.RestoreSecurity)
            {
                try
                {
                    var roles = metadata.GetMetadata<List<AveRoleInfo>>();
                    if (roles != null && roles.Count > 0)
                    {
                        webImport.RestoreRoles(new AveRoleInfoList() { RoleInfos = roles, SourceHasUniqueRoleDefinitions = sourceWebInfo.HasUniqueRoleDefinitions }, restoreOption.SecurityRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.PermissionLevel }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.PermissionLevel, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebUsers(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.SecurityRestoreOption.RestoreSecurity)
            {
                try
                {
                    var userInfos = metadata.GetMetadata<List<AveUserInfo>>();
                    if (userInfos != null && userInfos.Count > 0)
                    {
                        if (restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
                        {
                            userInfos = restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(userInfos);
                        }
                        webImport.RestoreUsers(userInfos, restoreOption.SecurityRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Group }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Group, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebGroups(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.SecurityRestoreOption.RestoreSecurity)
            {
                try
                {
                    var groupInfos = metadata.GetMetadata<List<AveGroupInfo>>();
                    if (groupInfos != null && groupInfos.Count > 0)
                    {
                        if (restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore != null)
                        {
                            foreach (var group in groupInfos)
                            {
                                group.Members = restoreOption.SecurityRestoreOption.UserGroupRestoreOption.ProcessUserInfoBeforeRestore(group.Members);
                                group.Memberships = new List<int>();
                                group.Members.ForEach(userInfo=> group.Memberships.Add(userInfo.ID));
                                    }
                                }
                        webImport.RestoreGroups(groupInfos, restoreOption.SecurityRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.User }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.User, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebLanguageFile(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var languageInfo = metadata.GetMetadata<AveLanguageInfo>();

                    if (languageInfo != null)
                    {
                        this.restoreSite.ObjectManager.LanguageMappingController.RestoreLanguageFile(languageInfo);
                        ////no need to restore language file, will load mapping from xml file
                        //todo yzshao:Language mapping
                        
                        profiler.OnStatusChangedSafe(new SPImportEventArgs() { Message = WrapperResourceKey.Wrapper_RestoreLanguageFile, Url = siteRelativeURL, Status = WrapperRestoreStatus.Successful, Type = SPObjectType.EventReceiver }); 
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.LanguageFile }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.LanguageFile, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebEventReceiver(IAveRestoreStream restoreStrem, AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var eventReceivers = metadata.GetMetadata<List<AveEventReceiverInfo>>();

                    if (eventReceivers != null && eventReceivers.Count > 0)
                    {
                        webImport.RestoreEventReceiver(eventReceivers, restoreOption.ConfigurationRestoreOption, profiler);
                        if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResourceKey.Wrapper_RestoreEventReceiver, Url = siteRelativeURL, Status = WrapperRestoreStatus.Successful, Type = SPObjectType.EventReceiver }); }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.EventReceiver }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.EventReceiver, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebBasicInfo(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            try
            {
                var webInfo = metadata.GetMetadata<AveWebInfo>();
                sourceWebInfo.Title = webInfo.Title;
                sourceWebInfo.HasUniqueRoleDefinitions = webInfo.HasUniqueRoleDefinitions;

                //todo 支持还原app web
                //目的端不存在，而且源端是app web暂不支持
                if (!exists && webInfo.IsAppWeb)
                {
                    throw new AveWrapperAppDataException(WrapperResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl);
                }

                if (exists)
                {
                    //目的端和源端只有一端是app web时，也不支持
                    if (webImport.IsAppWeb ^ webInfo.IsAppWeb)
                    {
                        throw new AveWrapperAppDataException(WrapperResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl);
                    }
                    //目的端和源端都是APP web，需要check目的端app instance是安装
                    if (webImport.IsAppWeb && webInfo.IsAppWeb && !webImport.IsAppInstanceInstalled)
                    {
                        throw new AveWrapperAppDataException(WrapperResourceKey.Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl);
                    }

                    if (restoreOption.RestoreAction == SPContainerRestoreAction.Replace)
                    {
                        if (webImport.DeleteWeb())
                        {
                            if (restoreOption.WebDeleted != null)
                            {
                                restoreOption.WebDeleted();
                            }
                            exists = false;
                        }
                    }
                }

                //判断在目的端是否找到了这个web,如果存在，则直接返回
                if (exists)
                {
                }
                //如果外围不想创建web，则通过这个参数来控制
                else if (restoreOption.RestoreAction != SPContainerRestoreAction.None)
                {
                    //回收站冲突处理
                    if (restoreOption.ConflictCheckOption == SPWebConflictCheckOption.CheckRecycleBin && restoreOption.RestoreAction == SPContainerRestoreAction.Skip
                        && webImport.IsConflictWithRecycle())
                    {
                        throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_SkipRecycleBinConflict);
                    }

                    var webCreationParameters = ConvertWebInfoToCreationParameters(webInfo, siteRelativeURL);
                    if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_CreateWebInfo, webCreationParameters.Title, webCreationParameters.Description, webCreationParameters.WebUrl, webCreationParameters.WebTemplate, webCreationParameters.LCID, webCreationParameters.UseUniquePermissions, webCreationParameters.ConvertIfThere), Url = webInfo.Url, Status = WrapperRestoreStatus.None, Type = SPObjectType.Self }); }
                    webImport.CreateWeb(webCreationParameters);
                    if (profiler != null) { profiler.OnProgressUpdated(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_CreateWebInfo, webCreationParameters.Title, webCreationParameters.Description, webCreationParameters.WebUrl, webCreationParameters.WebTemplate, webCreationParameters.LCID, webCreationParameters.UseUniquePermissions, webCreationParameters.ConvertIfThere), Url = webInfo.Url, Status = WrapperRestoreStatus.Successful, Type = SPObjectType.Self }); }
                    isNewCreated = true;
                    //to do add mapping                  
                }
                else
                {
                    throw new FileNotFoundException(
                        WrapperResource.GetString(WrapperResourceKey.Wrapper_WebNotFound, this.siteRelativeURL));
                }
                //多语言环境，需要切换语言，因语言信息不同导致获取信息错误。
                ChangeCurrentThreadCulture();
                webImport.InitCurrentLanguageMapping(webInfo.LCID);
            }
            catch (Exception ex)
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Self }); }
                throw;
            }
        }

        private void RestoreWebFeatures(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var featureInfo = metadata.GetMetadata<AveFeatureInfoBox>();

                    if (featureInfo.FeatureList != null && featureInfo.FeatureList.Count > 0)
                    {
                        webImport.RestoreFeatures(featureInfo, restoreOption.ConfigurationRestoreOption, profiler);
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Feature }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Feature, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebNavigations(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);

            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                var navigationInfoList = metadata.GetMetadata<AveNavigationInfoList>();

                if (navigationInfoList != null)
                {
                    webImport.AddToNavNodesCache(navigationInfoList);
                }

            }
        }

        private void EnsureConfigurationOption(SPWebRestoreOption option)
        {
            if (option.ConfigurationRestoreOption == null)
            {
                throw new ArgumentNullException("option.ConfigurationRestoreOption");
            }
        }

        private void RestoreWebWorkflowAssociation(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);
            if (restoreOption.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
            {
                try
                {
                    var wfInfos = metadata.GetMetadata<List<AveWorkflowInfo>>();
                    SPWorkflowProcessorRuntime.ProcessAssociation = true;
                    SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)restoreOption.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;
                    if (wfInfos != null)
                    {
                        foreach (AveWorkflowInfo wfInfo in wfInfos)
                        {
                            if (restoreOption.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
                            {
                                webImport.RestoreWorkflowAssociation(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                            }
                            else
                            {
                                webImport.CacheNotRestoredWorkflowAssociation(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.WorkflowAssociation }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Feature, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebCTWorkflowAssociation(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);
            if (restoreOption.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
            {
                try
                {
                    var wfInfos = metadata.GetMetadata<List<AveWorkflowInfo>>();
                    SPWorkflowProcessorRuntime.ProcessAssociation = true;
                    SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)restoreOption.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;
                    webImport.IsWebContentTypeAssociation = true;
                    //webImport.AssociationOption = (SPWFAssociationConflictResolutionOption)restoreOption.WorkflowRestoreOption.AssociationRestoreOption.ConflictResolutionOption;
                    if (wfInfos != null)
                    {
                        foreach (AveWorkflowInfo wfInfo in wfInfos)
                        {
                            if (restoreOption.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
                            {
                                webImport.RestoreWebCTWorkflowAssociation(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                            }
                            else
                            {
                                webImport.CacheNotRestoredWorkflowAssociation(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.ContentTypeWorkflowAssociation }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Feature, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        private void RestoreWebWorkflowInstance(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);
            if (restoreOption.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
            {
                try
                {
                    var wfInfos = metadata.GetMetadata<List<AveWorkflowInfo>>();
                    restoreOption.WorkflowRestoreOption.InstanceRestoreOption.ToWFInstanceSetting();
                    if (wfInfos != null)
                    {
                        foreach (AveWorkflowInfo wfInfo in wfInfos)
                        {
                            webImport.RestoreWorkflowInstance(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.WorkflowInstance }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.WorkflowInstance, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }

        }

        private void RestoreWebWorkflowSchedule(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);
            if (restoreOption.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
            {
                try
                {
                    var wfInfos = metadata.GetMetadata<List<AveWorkflowInfo>>();
                    SPWorkflowProcessorRuntime.ProcessAssociation = true;
                    if (wfInfos != null)
                    {
                        foreach (AveWorkflowInfo wfInfo in wfInfos)
                        {
                            webImport.RestoreWorkflowSchedule(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.WorkflowInstance }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.WorkflowSchedual, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
                
            }
        }

        private void RestoreWebWorkflowTemplate(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            EnsureConfigurationOption(restoreOption);
            if (restoreOption.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
            {
                try
                {
                    var wfInfos = metadata.GetMetadata<List<AveWorkflowInfo>>();
                    SPWorkflowProcessorRuntime.ProcessAssociation = true;
                    if (wfInfos != null)
                    {
                        foreach (AveWorkflowInfo wfInfo in wfInfos)
                        {
                            webImport.RestoreWorkflowTemplate(wfInfo, restoreOption.WorkflowRestoreOption, profiler);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.WorkflowInstance }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.WorkflowTemplate, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }

            }
        }

        private void ChangeCurrentThreadCulture()
        {
            if (Thread.CurrentThread.CurrentUICulture != webImport.UICulture)
            {
                Thread.CurrentThread.CurrentUICulture = webImport.UICulture;
            }
            if (Thread.CurrentThread.CurrentCulture != webImport.UICulture)
            {
                Thread.CurrentThread.CurrentCulture = webImport.UICulture;
            }
        }

        /// <summary>
        /// Template 和 Language需要走mapping处理
        /// </summary>
        /// <param name="webInfo"></param>
        /// <returns></returns>
        private WebCreationParameters ConvertWebInfoToCreationParameters(AveWebInfo webInfo, string strURL)
        {
            var webCreationParameters = new WebCreationParameters();
            webCreationParameters.Title = webInfo.Title;
            webCreationParameters.WebUrl = strURL;
            webCreationParameters.Description = webInfo.Description;
            webCreationParameters.UseUniquePermissions = false;
            webCreationParameters.ConvertIfThere = false;

            webCreationParameters.WebTemplate = restoreSite.TemplateMapping.GetSiteTemplateMappingName(ChangeSpecialWebTemplate(webInfo.WebTemplate));
            webCreationParameters.LCID = restoreSite.ObjectManager.LanguageMappingController.GetMappingLCID(webInfo.LCID);

            //获取parent Web的create 参数
            if (strURL.Contains('/'))
            {
                var subUrl = strURL.Substring(0, strURL.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                if (webInfo.parentWebInfo != null)
                {//源端备份了parent信息
                    webCreationParameters.parentWeb = ConvertWebInfoToCreationParameters(webInfo.parentWebInfo, subUrl);
                }
                else
                {//目的端多出来的parent
                    webCreationParameters.parentWeb = ConvertWebInfoToCreationParameters(webInfo, subUrl);
                }
            }

            return webCreationParameters;
        }


        /// <summary>
        /// 空Web Template的template是"STS#-1"；如果直接用此template创建web，会出现异常，需要替换成String.Empty去创建空模板Web。
        /// </summary>
        /// <param name="webTemplate"></param>
        /// <returns></returns>
        private string ChangeSpecialWebTemplate(string webTemplate)
        {
            //确认Template格式是否正确
            var index = webTemplate.IndexOf("#", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                int templateNumber = 0;
                if (int.TryParse(webTemplate.Substring(index + 1), out templateNumber))
                {
                    if (templateNumber < 0)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning,WrapperResourceKey.Wrapper_InvalidTemplateNumber, webTemplate);
                        return string.Empty;
                    }
                }
            }
            return webTemplate;
        }

        private void RestoreWebProperties(Common.IAveRestoreStream restoreStream, Common.AveMetadata metadata, SPWebRestoreOption restoreOption, ISPWebImportProfiler profiler)
        {
            if (restoreOption.ConfigurationRestoreOption.RestoreConfiguration)
            {
                try
                {
                    var settingInfo = metadata.GetMetadata<AveWebSettingInfo>();

                    webImport.RestoreSettings(settingInfo, restoreOption.ConfigurationRestoreOption, profiler, restoreOption.SecurityRestoreOption);
                }
                catch (Exception ex)
                {
                    if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = ex.Message, Url = siteRelativeURL, Status = WrapperRestoreStatus.Failed, Type = SPObjectType.Setting }); }
                }
            }
            else
            {
                if (profiler != null) { profiler.OnStatusChanged(new SPImportEventArgs() { Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_OptionIsNotEnabled), Type = SPObjectType.Setting, Status = WrapperRestoreStatus.Skipped, Url = siteRelativeURL }); }
            }
        }

        protected override ISPWebImportProfiler CreateDefaultProfiler()
        {
            return new DefaultRestoreWebProfiler();
        }

        protected override SPFileRestoreReport GenerateReport(ISPWebImportProfiler profiler)
        {
            return profiler.GenerateReport();
        }

        protected override void BeginRestore(SPWebRestoreOption restoreOption)
        {
        }

        protected override void EndRestore(SPWebRestoreOption restoreOption)
        {
        }

        public void Dispose()
        {
            this.restoreSite.ObjectManager.PostActionManager.ExecutePostActions(webImport);
            if (this.webImport != null)
            {
                this.webImport.Dispose();
            }
        }

        public IAveWeb SPWeb
        {
            get { throw new NotImplementedException(); }
        }

        public Wrapper.Core.SPRestore.Mapping.IFieldMapping FieldMapping { set; get; }


        public Wrapper.Core.SPRestore.Mapping.IContentTypeMapping ContentTypeMapping { get; set; }
    }

    class WebSourceInfo
    {
        public string Title { get; set; }
        public bool HasUniqueRoleDefinitions { get; set; }
        public bool HasUniqueRoleAssignments { get; set; }
    }

}
