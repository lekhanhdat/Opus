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
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;

namespace AvePoint.Item.Restore
{
    public static class AppendItemMapping
    {
        #region Append New Name Mapping

        private static readonly Dictionary<string, string> mMappingAppendName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);  // It is for append_1 Name conflict solution

        public static void AddToMappingAppendName(string key, string value)
        {
            if (mMappingAppendName.ContainsKey(key))
            {
                mMappingAppendName[key] = value;
            }
            else
            {
                mMappingAppendName.Add(key, value);
            }
        }

        public static string GetValueAppendName(string key)
        {
            return mMappingAppendName[key];
        }

        public static bool ContainsKeyAppendName(string key)
        {
            return mMappingAppendName.ContainsKey(key);
        }

        #endregion

        public static void RemoveAll()
        {
            mMappingAppendVersion.Clear();
            mMappingAppendName.Clear();
        }

        #region Append New Version Mapping

        private static readonly Dictionary<string, bool> mMappingAppendVersion =
          new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // It is for append new version confliction solution. bool = true means the item is restring by docave.

        public static void AddToMappingAppendVersion(string key, bool value)
        {
            if (mMappingAppendVersion.ContainsKey(key))
            {
                mMappingAppendVersion[key] = value;
            }
            else
            {
                mMappingAppendVersion.Add(key, value);
            }
        }

        public static bool GetValueAppendVersion(string key)
        {
            return mMappingAppendVersion[key];
        }

        public static bool ContainsKeyAppendVersion(string key)
        {
            return mMappingAppendVersion.ContainsKey(key);
        }

        #endregion
    }

    public static class ReplaceWorker
    {
        private static readonly AveLogger Log = AveLogger.GetInstance(typeof(ReplaceWorker));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="url"></param>
        /// <returns>Is site exist</returns>
        public static bool DeleteSite(AveObjectModelFactory factory, AveSPSite aveSPSite, bool includeProjectData)
        {
            bool isExist = false;
            try
            {   //当前产品中Contextkind为ClientObjectModel，所以注释掉一下代码
                aveSPSite.SPSite = aveSPSite.ObjectModelFactory.CreateSite(aveSPSite.SiteUrl);
                //if (factory.ContextKind != AveContextKind.ClientObjectModel)
                //{
                //    using (IAveWeb web = site.RootWeb)
                //    {
                //        if (web.Properties.ContainsKey("BackedUp"))
                //        {
                //            web.Properties["BackedUp"] = "true";
                //        }
                //        else
                //        {
                //            web.Properties.Add("BackedUp", "true");
                //        }
                //        web.Properties.Update();
                //    }
                //    site.Delete();
                //}
                //replace的时候，将rootweb以下的要清除。
                using (IAveWeb web = aveSPSite.SPSite.RootWeb)
                {
                    DeleteWeb(web, includeProjectData);
                }
                isExist = true;
            }
            catch (Exception ex)
            {
                Log.Warn("Deleted the site failed,Site url:{0}.Error Message: {1}.", aveSPSite.SiteUrl, ex.ToString());
            }
            finally
            {
                if (aveSPSite != null)
                {
                    aveSPSite.Dispose();
                }
            }
            return isExist;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="url"></param>
        /// <returns>Is web exist</returns>
        public static bool DeleteWeb(AveSPSite aveSPSite, string webName, bool includeProjectData)
        {
            bool isWebExist = false;
            if (aveSPSite.IsNewCreated ||
                string.Equals(webName, AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase))
            {
                //return true;
                webName = string.Equals(webName, AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase) ? string.Empty : webName;
            }
            IAveWeb web = null;
            try
            {
                web = aveSPSite.SPSite.OpenWeb(webName);
                if (web.AppInstanceId == Guid.Empty)
                {
                    DeleteWeb(web, includeProjectData);
                }
                isWebExist = true;
            }
            finally
            {
                if (web != null)
                {
                    web.Dispose();
                }
            }
            return isWebExist;
        }

        private static void DeleteWeb(IAveWeb web, bool includeProjectData)
        {
            try
            {
                for (int i = web.Webs.Count - 1; i >= 0; i--)
                {
                    IAveWeb subWeb = web.Webs[i];
                    try
                    {
                        DeleteWeb(subWeb, false);
                    }
                    finally
                    {
                        if (subWeb != null)
                        {
                            subWeb.Dispose();
                        }
                    }
                }
                if (web.IsRootWeb)
                {
                    if (includeProjectData && string.Equals(web.WebTemplateName, "PWA#0", StringComparison.OrdinalIgnoreCase))
                    {
                        IAveProjectCollection projects = web.Site.Projects;
                        for (int index = projects.Count - 1; index >= 0; index--)
                        {
                            var project = projects[index];
                            try
                            {
                                Log.Info("delete project:{0}", project.Name);
                                project.Delete();
                            }
                            catch (Exception ex)
                            {
                                Log.Warn("delete project failed. name:{0}, error:{1}", project.Name, ex);
                            }
                        }
                    }
                    //清空RootWeb下的List
                    IAveListCollection lists = web.Lists;
                    for (int index = lists.Count - 1; index >= 0; index--)
                    {
                        var list = lists[index];
                        try
                        {

                            if (!list.IsCatalog && list.AllowDeletion && !string.Equals(list.Title, "wfpub", StringComparison.OrdinalIgnoreCase))
                            {
                                list.Delete();
                                Log.Info("Delete list:{0}", list.Title);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Log(AveLogLevel.ERROR, "An error occurred while deleting list. Title:{0}, message:{1}.", list.Title, ex);
                        }
                    }
                }
                else
                {
                    web.Delete();
                }
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.ERROR, "An error occurred while deleting web. Url:{0}, message:{1}.",
                    (web == null || web.Exists == false) ? string.Empty : web.Url, ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="url"></param>
        /// <returns>Is list exist</returns>
        public static bool DeleteList(AveSPWeb aveSPWeb, string listTitle)
        {
            bool isExist = false;
            IAveList list;
            try
            {
                list = aveSPWeb.SPWeb.Lists[listTitle];
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.DEBUG, "An error occurred while deleting list. Title:{0}, message:{1}.", listTitle, ex);
                return isExist;
            }
            if (list != null)
            {
                isExist = true;
                if (!list.IsCatalog && list.AllowDeletion)
                {
                    list.Delete();
                }
            }
            return isExist;
        }

        public static bool DeleteProjet(AveSPWeb aveSPWeb, string projectName)
        {
            bool isExist = false;
            IAveProject proj;
            try
            {
                proj = aveSPWeb.ParentSite.SPSite.Projects.GetByName(projectName);
                if (proj != null)
                {
                    isExist = true;
                    proj.Delete();
                }
            }
            catch (ArgumentException)
            {
                return isExist;
            }
            catch (Exception ex)
            {
                Log.Warn("delete project failed. name:{0}, error:{1}", projectName, ex);
                return isExist;
            }
            return isExist;
        }

        public static bool ExistFolder(AveSPList aveSPList, AveSPFolder aveFolder)
        {
            try
            {
                if (aveFolder?.ParentFolder?.SPFolder?.ParentWeb == null)
                {
                    Log.Log(AveLogLevel.DEBUG, "Unable to resolve parent web while checking folder existence. Url:{0}.",
                        aveFolder == null ? string.Empty : aveFolder.Url);
                    return false;
                }

                string targetServerRelativeUrl = aveFolder.ServerRelativeUrl;
                if (string.IsNullOrEmpty(targetServerRelativeUrl) && aveFolder.ParentFolder.SPFolder != null)
                {
                    targetServerRelativeUrl = string.Concat(aveFolder.ParentFolder.SPFolder.ServerRelativeUrl.TrimEnd('/'), "/", aveFolder.Name);
                }

                if (string.IsNullOrEmpty(targetServerRelativeUrl))
                {
                    Log.Log(AveLogLevel.DEBUG, "Folder server relative url is empty while checking folder existence. Url:{0}.",
                        aveFolder.Url ?? string.Empty);
                    return false;
                }
                aveFolder.SPFolder = aveSPList.SPList?.GetFolder(aveFolder.ServerRelativeUrl);
                return aveFolder.SPFolder != null && aveFolder.SPFolder.Exists;
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.DEBUG, "An error occurred while check exist folder. Url:{0}, message:{1}.",
                    aveFolder == null ? string.Empty : aveFolder.Url, ex);
            }
            return false;
        }

        public static bool DeleteFolder(AveSPList aveSPList, AveSPFolder aveFolder)
        {
            bool isExist = false;
            try
            {
                if (aveSPList.IsSystemList ||
                    aveFolder.Name.Equals("Forms"))
                {
                    return true;
                }
                aveFolder.ParentFolder.SPFolder.SubFolders[aveFolder.Name].Delete();
                isExist = true;
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.DEBUG, "An error occurred while deleting folder. Url:{0}, message:{1}.",
                    aveFolder == null ? string.Empty : aveFolder.Url, ex);
            }
            return isExist;
        }
    }

    [AveCodeReview("2012/03/02", "qlluo@avepoint.com", "cheng.cui@avepoint.com", new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CO_10 }, "ADO-25546", false)]
    public static class GlobalRestoreOptionWorker
    {
        public static GlobalRestoreOption GlobalRestoreOption { get; set; }

        private static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void CheckSiteGlobalSetting(AveObjectModelFactory factory, string url, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            if (GlobalRestoreOption.ContainerSetting == ContainerSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "site collection restore option: {0} ,security {1} , property {2}, Url:{3} ", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), url);
            if (GlobalRestoreOption.ContainerSetting.CheckRestoreContainer())
            {//勾选RestoreContainer,需要重置restoreOption                
                ContainerRestoreReset(contentDto);
                return;
            }
            try
            {
                factory.CreateSite(url);
            }
            catch (AveSecurityTrimingException e)
            {
                log.Warn(string.Format("Can not connect to the destination site collection. Details:{0}", e.ToString()));
                throw new Exception("Can not connect to the destination site collection.");
            }
            catch (Exception e)
            {
                throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "Site Collection: " + url), e);
            }
            ContainerRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
        }

        public static void CheckWebGlobalSetting(AveSPSite aveSPSite, string webName, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            if (GlobalRestoreOption.ContainerSetting == ContainerSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "Web restore option: {0} ,security {1} , property {2}, webName: {3}", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), webName);
            if (GlobalRestoreOption.ContainerSetting.CheckRestoreContainer())
            {//勾选RestoreContainer,需要重置restoreOption                
                ContainerRestoreReset(contentDto);
                return;
            }

            using (var web = string.Equals(webName, ".", StringComparison.OrdinalIgnoreCase) ? aveSPSite.SPSite.OpenWeb() : aveSPSite.SPSite.OpenWeb(webName))
            {
                if (!web.Exists)
                {
                    throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "Web: " + webName));
                }
                ContainerRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
            }
        }

        public static void CheckListGlobalSetting(AveSPWeb aveSPWeb, string listTitle, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            if (GlobalRestoreOption.ContainerSetting == ContainerSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "List restore option: {0} ,security {1} , property {2}, list title: {3}", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), listTitle);
            if (GlobalRestoreOption.ContainerSetting != ContainerSetting.None && String.Equals(listTitle, AveConstants.SYSTEM_FOLDER))
            {
                throw new SkipException();
            }
            if (GlobalRestoreOption.ContainerSetting.CheckRestoreContainer())
            {//勾选RestoreContainer,需要重置restoreOption                
                ContainerRestoreReset(contentDto);
                return;
            }
            try
            {
                aveSPWeb.SPWeb.GetListByName(listTitle, true);
            }
            catch (Exception e)
            {
                throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "List: " + listTitle), e);
            }
            ContainerRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
        }

        public static void CheckFolderGlobalSetting(AveSPFolder aveSPFolder, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            if (GlobalRestoreOption.ContainerSetting == ContainerSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "Folder restore option: {0} ,security {1} , property {2}, folderName: {3}", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), aveSPFolder.Name);
            if (GlobalRestoreOption.ContainerSetting.CheckRestoreContainer())
            {//勾选RestoreContainer,需要重置restoreOption                
                contentDto.RestoreOption.ResetSecurity(contentDto.IsChecked && GlobalRestoreOption.ContainerSetting.CheckRestoreContainerSecurity());
                return;
            }
            try
            {
                aveSPFolder.ParentFolder.SPFolder.SubFolders.GetByName(aveSPFolder.Name);
            }
            catch (Exception e)
            {
                throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "Folder: " + aveSPFolder.Name), e);
            }
            FolderRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
        }


        public static void CheckDocumentGlobalSetting(AveSPFolder aveSPFolder, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            if (GlobalRestoreOption.ContentSetting == ContentSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "Folder restore option: {0} ,security {1} , property {2}, document name: {3}", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), contentDto.Name);
            if (GlobalRestoreOption.ContentSetting.CheckRestoreContent())
            {//勾选RestoreContent,需要重置restoreOption                      
                ContentRestoreReset(contentDto);
                return;
            }
            if (contentDto.Name.Contains(":"))
            {
                throw new SkipException(string.Format("{0} is history version. Security only restore will skip this version..", contentDto.Name));
            }
            try
            {
                var file = aveSPFolder.SPFolder.Files[contentDto.Name];
            }
            catch (Exception e)
            {
                throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "Documenet: " + contentDto.Name), e);
            }
            ContentRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
        }

        private static void ContentRestoreReset(RestoreContentDto contentDto)
        {
            contentDto.RestoreOption.ResetProperty(contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.OverWrite) && contentDto.IsChecked);
            contentDto.RestoreOption.ResetSecurity(GlobalRestoreOption.ContentSetting.CheckRestoreContentSecurity() && contentDto.IsChecked);
        }

        public static void CheckListItemGlobalSetting(AveSPFolder aveSPFolder, RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption, Dictionary<string, object> userData)
        {
            Guid itemGuid = userData.ContainsKey("#tp_GUID") ? new Guid(userData["#tp_GUID"].ToString()) : Guid.Empty;
            if (GlobalRestoreOption.ContentSetting == ContentSetting.None)
            {
                return;
            }
            log.Log(AveLogLevel.DEBUG, "Folder restore option: {0} ,security {1} , property {2}, itemName: {3}", contentDto.RestoreOption.mAveRestoreMode, contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreSecurity), contentDto.RestoreOption.CheckRestoreOption(AveRestoreMode.RestoreProperty), contentDto.Name);
            if (GlobalRestoreOption.ContentSetting.CheckRestoreContent())
            {//勾选RestoreContent,需要重置restoreOption                      
                ContentRestoreReset(contentDto);
                return;
            }
            if (contentDto.Name.Contains(":"))
            {
                throw new SkipException(string.Format("{0} is history version. Security only restore will skip this version..", contentDto.Name));
            }
            try
            {
                var splitop = contentDto.Name.IndexOf("_", StringComparison.OrdinalIgnoreCase);
                if (splitop > 0)
                {
                    aveSPFolder.ParentList.SPList.CheckItemIsExist(contentDto.Name.Substring(0, splitop), itemGuid);
                }
            }
            catch (Exception e)
            {
                throw new SkipException(string.Format("Cannot find the object &quot;{0}&quot;.  Restoration of the securities and properties was skipped..", "Listitem: " + contentDto.Name), e);
            }
            ContentRestoreSecurityOnlyReset(contentDto, securityRestoreOption);
        }

        #region Reset Method for Container
        private static void ContainerRestoreReset(RestoreContentDto contentDto)
        {
            contentDto.RestoreOption.ResetSecurity(contentDto.IsChecked && GlobalRestoreOption.ContainerSetting.CheckRestoreContainerSecurity());
            contentDto.RestoreOption.ResetProperty(contentDto.IsChecked && GlobalRestoreOption.ContainerSetting.CheckRestoreContainerProperty());
        }

        private static void ContainerRestoreSecurityOnlyReset(RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            var checkedSetting = GlobalRestoreOption.ContainerSetting;
            if (checkedSetting.CheckRestoreSecurityOnly())
            {
                //勾选SecurityOnly       
                contentDto.RestoreOption.ResetRequestOption(false, contentDto.IsChecked, contentDto.IsChecked ? (int)AveRestoreMode.OverWrite : (int)AveRestoreMode.Default);
                securityRestoreOption.ConflictResolutionForSecurityObject = GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyMerge ? ConflictResolutionForSecurityObject.Merge : ConflictResolutionForSecurityObject.OverWrite;
            }
        }
        #endregion

        private static void ContentRestoreSecurityOnlyReset(RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            var checkedSetting = GlobalRestoreOption.ContentSetting;
            if (checkedSetting.CheckRestoreSecurityOnly())
            {
                //勾选SecurityOnly   
                contentDto.RestoreOption.ResetRequestOption(false, contentDto.IsChecked, (int)AveRestoreMode.Default);
                securityRestoreOption.ConflictResolutionForSecurityObject = GlobalRestoreOption.ContentSetting == ContentSetting.SecurityOnlyMerge ? ConflictResolutionForSecurityObject.Merge : ConflictResolutionForSecurityObject.OverWrite;
            }
        }

        private static void FolderRestoreSecurityOnlyReset(RestoreContentDto contentDto, SecurityRestoreOption securityRestoreOption)
        {
            var checkedSetting = GlobalRestoreOption.ContainerSetting;
            if (checkedSetting.CheckRestoreSecurityOnly())
            {
                //勾选SecurityOnly       
                contentDto.RestoreOption.ResetRequestOption(false, contentDto.IsChecked, (int)AveRestoreMode.Default);
                securityRestoreOption.ConflictResolutionForSecurityObject = GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyMerge ? ConflictResolutionForSecurityObject.Merge : ConflictResolutionForSecurityObject.OverWrite;
            }
        }
    }

    [AveCodeReview("2012/03/02", "qlluo@avepoint.com", "cheng.cui@avepoint.com", new string[] { CodeReviewConstants.CHECK_LIST_ID_BL_1, CodeReviewConstants.CHECK_LIST_ID_CO_10 }, "ADO-25546", false)]
    public static class GlobalRestoreOptionExtension
    {
        private const int RestoreContainerOrContent = 1;
        private const int RestoreContainerOrContentSecurity = 2;
        private const int RestoreContainerProperty = 4;
        private const int RestoreSecurityOnly = 16;
        public static bool CheckRestoreSecurityOnly(this ContainerSetting setting)
        {
            int containerSetting = (int)setting;
            return (containerSetting & RestoreSecurityOnly) == RestoreSecurityOnly;
        }
        public static bool CheckRestoreSecurityOnly(this ContentSetting setting)
        {
            int contentSetting = (int)setting;
            return (contentSetting & RestoreSecurityOnly) == RestoreSecurityOnly;
        }
        public static bool CheckRestoreContainer(this ContainerSetting setting)
        {
            int containerSetting = (int)setting;
            return (containerSetting & RestoreContainerOrContent) == RestoreContainerOrContent;
        }

        public static bool CheckRestoreContent(this ContentSetting setting)
        {
            int contentrSetting = (int)setting;
            return (contentrSetting & RestoreContainerOrContent) == RestoreContainerOrContent;
        }

        public static bool CheckRestoreContainerSecurity(this ContainerSetting setting)
        {
            int containerSetting = (int)setting;
            return (containerSetting & RestoreContainerOrContentSecurity) == RestoreContainerOrContentSecurity;
        }

        public static bool CheckRestoreContentSecurity(this ContentSetting setting)
        {
            int contentSetting = (int)setting;
            return (contentSetting & RestoreContainerOrContentSecurity) == RestoreContainerOrContentSecurity;
        }

        public static bool CheckRestoreContainerProperty(this ContainerSetting setting)
        {
            int containerSetting = (int)setting;
            return (containerSetting & RestoreContainerProperty) == RestoreContainerProperty;
        }

        public static string GetSettingInfo(this GlobalRestoreOption option)
        {
            if (option == null)
            {
                return "GlobalRestoreOption: null";
            }
            return string.Format("GlobalRestoreOption:ContainerSetting is {0},ContentSetting is {1}", option.ContainerSetting, option.ContentSetting);
        }
    }
}
