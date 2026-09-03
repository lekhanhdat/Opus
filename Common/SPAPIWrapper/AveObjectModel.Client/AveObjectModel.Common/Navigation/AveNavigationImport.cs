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




using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using System.Collections;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Resource.Client;
    #endregion

    internal class AveNavigationImport
    {
        private AveWeb m_Web;
        private AveWeb m_TempWeb;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNavigationImport));

        public NavigationRestoreSetting NavigationRestoreSetting { set; get; }

        public AveNavigationImport(AveWeb web)
        {
            m_Web = web;
        }

        public void Run(KeyValuePair<Guid, AveNavigationInfoList> data)
        {
            m_TempWeb = null;
            try
            {
                //m_TempWeb = m_Web.Site.OpenWeb(data.Key) as AveWeb;
                m_TempWeb = (m_Web.Site as AveSite).ReloadWeb(data.Key) as AveWeb;
                try
                {
                    if (!m_TempWeb.AllowUnsafeUpdates)
                    {
                        m_TempWeb.AllowUnsafeUpdates = true;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Set web:{0} allowUnsafeUpdates failed.Error Message:{1}", m_TempWeb.ServerRelativeUrl, ex.ToString());
                }
                if (m_TempWeb != null)
                {
                    bool compoundSupport = false;
                    Guid publishingFeatureId = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");
                    AveNavigationInfoList value = data.Value;

                    RestorePublishAppearance(value.PublishFeatureAppearance);
                    m_TempWeb.RemoveNavigation();
                    //SAAS-30941/30052, Need to remove all the items in Recent quick launch (SharePoint would add new created list into the Recent quick launch, while it only keep 5 items showing in the quick lauch, if these 5 items got removed, sharepoint will display the older items in the quick launch, thus, before resotring the quick launch, need to remove all the links in Recent
                    while (!RecentQuickLaunchIsEmpty(m_TempWeb))
                    {
                        mLogger.Info("Deleting quick launch links in Recent.");
                    }
                    //如果site的Publishing Feature开启的话, 则执行WebServiceReqeust的还原逻辑;
                    if (m_Web.Site.Features[publishingFeatureId] != null)
                    {
                        ReplaceUrlAndTitle(value.NavNodes);
                        ReplaceTermId(value);
                        compoundSupport = m_TempWeb.Navigation.RestoreNavigation(value, this.NavigationRestoreSetting);
                        mLogger.Info("Restore Navigation with web service.compoundSupport:{0}",compoundSupport);
                    }
                    //如果Publishing Freaure没有开启, 或者OMReeqest执行失败的, 则使用正常逻辑Restore;
                    if (!compoundSupport)
                    {
                        AveNavigationNodeCollection topBarNavCollection = m_TempWeb.Navigation.TopNavigationBar as AveNavigationNodeCollection;
                        AveNavigationNodeCollection quickLaunchNavCollection = m_TempWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
                        AveNavigationNodeCollection searchNavCollection = m_TempWeb.Navigation.SearchNav as AveNavigationNodeCollection;

                        ClearAllNodes(topBarNavCollection);
                        ClearAllNodes(quickLaunchNavCollection);
                        ClearAllNodes(searchNavCollection);
                       
                        value.NavNodes = SortNodeChildren(value.NavNodes);
                        foreach (AveNavigationInfo navInfo in value.NavNodes.Where(navInfo => NeedRestore(navInfo.Scope, value)))
                        {
                            try
                            {
                                mLogger.Info("Node name:{0},type:{1},isMoss:{2},url:{3}", navInfo.Title, navInfo.NodeType, WrapperRuntime.CurrentContext.IsMoss,navInfo.Url);
                                CreateNavNodeOption nodeCreateOption = (WrapperRuntime.CurrentContext.IsMoss && navInfo.NodeType != -1 ) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;

                                if (navInfo.Scope.Equals(AveNavigationScope.TopNavigationBar))
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, ref topBarNavCollection);
                                }
                                else if (navInfo.Scope.Equals(AveNavigationScope.QuickLaunch))
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, ref quickLaunchNavCollection);
                                }
                                else if (navInfo.Scope.Equals(AveNavigationScope.SearchNav))
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, ref searchNavCollection);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Debug(AveObjectModel_CommonResource.RestoreNavigationNodeError, navInfo.Title, this.m_Web.Url, ex.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.AveNavigationImportRunError, this.m_Web.Url, e.ToString());
            }
            finally
            {
                if (m_TempWeb != null)
                {
                    m_TempWeb.Dispose();
                }
            }
        }

        /// <summary>
        /// original backup,check IsExternal by url,and only check full url,
        /// so relative url isExternal is incorrect for some external nodes
        /// add this logic to check such urls,fix isExternal incorrect issue
        /// </summary>
        /// <param name="url"></param>
        /// <param name="siteUrl"></param>
        /// <param name="siteRelativeUrl"></param>
        /// <returns>full url return null,empty url return true,other according to check logic below</returns>
        public bool? EnsureRelativeUrlExternalStatus(string url, string siteUrl, string siteRelativeUrl)
        {
            bool isExternal = false;

            if (string.IsNullOrEmpty(url))
            {
                return true;
            }
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                       url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                       url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            string trimedUrl = "/" + url.Trim('/') + "/";
            //unreachable logic ,the relative url can go here

            List<string> Prefixes = new List<string>() { "/sites/", "/team/", "/personal/", "/portals/" };
            //source is root site collection
            if (siteRelativeUrl.Trim('/').Equals(string.Empty))
            {

                foreach (var managePath in Prefixes)
                {
                    //internal url
                    if (trimedUrl.StartsWith(managePath, StringComparison.OrdinalIgnoreCase))
                    {
                        isExternal = true;
                        break;
                    }
                }
            }
            else//source is not root site collection.check it with normal logic
            {
                isExternal = !trimedUrl.StartsWith("/" + siteRelativeUrl.Trim('/') + "/", StringComparison.OrdinalIgnoreCase);
            }
            return isExternal;
        }

        private void RestorePublishAppearance(bool isPublishing)
        {
            try
            {
                if (isPublishing && m_TempWeb.Site.Features[AveConstants.PUBLISHINGRESOURCES] == null)
                {
                    m_TempWeb.Site.Features.Add(AveConstants.OFFICEPUBLISHINGSITE, true);
                }
            }
            catch (Exception e)
            {
                mLogger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(AveConstants.OFFICEPUBLISHINGSITE, e));
            }
        }

        public bool NeedRestore(AveNavigationScope aveNavigationScope, AveNavigationInfoList value)
        {
            if (aveNavigationScope == AveNavigationScope.TopNavigationBar)
            {
                if (m_TempWeb.IsRootWeb && value.SharedTopLink)
                {
                    return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveTopLink || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                }
                return !m_TempWeb.Navigation.UseShared;
            }
            if (aveNavigationScope == AveNavigationScope.QuickLaunch)
            {
                if (m_TempWeb.IsRootWeb && value.ShareQuickLunch)
                {
                    return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveQuickLunch || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                }
                bool quickLunchIsInherited = m_TempWeb.AllProperties != null &&
                       m_TempWeb.AllProperties["__InheritCurrentNavigation"] != null &&
                       string.Equals(m_TempWeb.AllProperties["__InheritCurrentNavigation"].ToString(), "True", StringComparison.OrdinalIgnoreCase);

                return !quickLunchIsInherited;
            }
            if (aveNavigationScope == AveNavigationScope.SearchNav)
            {
                return !m_TempWeb.Navigation.UseShared;
            }
            return true;
        }

        private void ReplaceUrlAndTitle(List<AveNavigationInfo> navigationNodes)
        {
            AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
            foreach (AveNavigationInfo navInfo in navigationNodes)
            {
                ReplaceOption replaceOption = new ReplaceOption(true, true);
                navInfo.Url = AveReplaceProcessor.UrlReplace(navInfo.Url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                navInfo.Url = AveUrlUtility.ReplaceGroupId(siteMappingManager.SourceSiteInfo, navInfo.Url, GetGroupId());
                ReplaceUrlAndTitle(navInfo.Children);
            }
        }

        private string GetGroupId()
        {
            object idObject = null;
            if (m_Web.Site.RootWeb.WebTemplateName.Equals("GROUP#0", StringComparison.OrdinalIgnoreCase) &&
                m_Web.Site.RootWeb.AllProperties.ContainsKey("GroupId"))
            {
                idObject = m_Web.Site.RootWeb.AllProperties["GroupId"];
            }
            return idObject == null ? string.Empty : idObject.ToString();
        }

        private void ReplaceTermId(AveNavigationInfoList navigationInfo)
        {
            try
            {
                Ave2013NavigationInfo navigation = navigationInfo as Ave2013NavigationInfo;
                if (navigation != null)
                {
                    AveTermMappingManager termMappingManager = WrapperRuntime.CurrentContext.MappingManager.TermMappingManager;
                    if (navigation.CurrentNavigation.Source == AveStandardNavigationSource.TaxonomyProvider)
                    {
                        navigation.CurrentNavigation.TermStoreId = termMappingManager.TermStoreIdMapping[navigation.CurrentNavigation.TermStoreId];
                        navigation.CurrentNavigation.TermSetId = termMappingManager.TermSetIdMapping[navigation.CurrentNavigation.TermSetId];
                        navigation.CurrentNavigation.TermGroupId = termMappingManager.TermGroupIdMapping[navigation.CurrentNavigation.TermGroupId];
                        IAveTaxonomySession session = this.m_Web.Site.AveSPTaxonomySession;//mAveSite.ObjectModelFactory.CreateTaxonomySession(mAveSite.SPSite);
                        IAveTermStore destTermStore = session.TermStores[0];
                        IAveTermSet destTermSet = destTermStore.GetTermSet(navigation.CurrentNavigation.TermSetId);                        

                        foreach (IAveTerm term in destTermSet.Terms)
                        {
                            ProcessTermLocalCustomProperties(term);
                        }
                        destTermSet.TermStore.CommitAll();
                    }

                    if (navigation.GlobalNavigation.Source == AveStandardNavigationSource.TaxonomyProvider)
                    {
                        navigation.GlobalNavigation.TermStoreId = termMappingManager.TermStoreIdMapping[navigation.GlobalNavigation.TermStoreId];
                        navigation.GlobalNavigation.TermSetId = termMappingManager.TermSetIdMapping[navigation.GlobalNavigation.TermSetId];
                        navigation.GlobalNavigation.TermGroupId = termMappingManager.TermGroupIdMapping[navigation.GlobalNavigation.TermGroupId];
                        IAveTaxonomySession session = this.m_Web.Site.AveSPTaxonomySession;//mAveSite.ObjectModelFactory.CreateTaxonomySession(mAveSite.SPSite);
                        IAveTermStore destTermStore = session.TermStores[0];
                        IAveTermSet destTermSet = destTermStore.GetTermSet(navigation.GlobalNavigation.TermSetId);

                        foreach (IAveTerm term in destTermSet.Terms)
                        {
                            ProcessTermLocalCustomProperties(term);
                        }
                        destTermSet.TermStore.CommitAll();
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Failed to replace term ids, error detail : {0}", e.ToString());
            }
        }

        private void ProcessTermLocalCustomProperties(IAveTerm term)
        {
            List<string> systemNavgationPropertiesList = new List<string> { "_Sys_Nav_SimpleLinkUrl", "_Sys_Nav_CatalogTargetUrl", "_Sys_Nav_CatalogTargetUrlForChildTerms", "_Sys_Nav_TargetUrl", "_Sys_Nav_TargetUrlForChildTerms", "_Sys_Nav_AssociatedFolderUrl", "_Sys_Nav_CategoryImageUrl" };

            foreach (var prop in systemNavgationPropertiesList)
            {
                if (term.LocalCustomProperties.ContainsKey(prop))
                {
                    string url = term.LocalCustomProperties[prop];
                    bool urlReplaced = false;
                    if (!string.IsNullOrEmpty(url) && url.StartsWith("~sitecollection", StringComparison.OrdinalIgnoreCase))
                    {
                        url = string.Concat(WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.SourceSiteInfo.ServerRelativeUrl, url.Substring("~sitecollection".Length));
                        urlReplaced = true;
                    }
                    AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                    var resultUrl = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, this.m_Web.Site.Url);
                    if (urlReplaced)
                    {
                        if (!string.IsNullOrEmpty(url) && resultUrl.StartsWith(this.m_Web.Site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            resultUrl = string.Concat("~sitecollection", resultUrl.Substring(this.m_Web.Site.ServerRelativeUrl.Length));
                        }
                    }
                    term.SetLocalCustomProperty(prop, resultUrl);
                }
            }

            foreach (IAveTerm item in term.Terms)
            {
                ProcessTermLocalCustomProperties(item);
            }
        }

        private List<AveNavigationInfo> SortNodeChildren(List<AveNavigationInfo> children)
        {
            if (children == null)
            {
                return null;
            }
            List<AveNavigationInfo> list = new List<AveNavigationInfo>();
            foreach (AveNavigationInfo info in children)
            {
                int rank = SearchChildNodePosition(list, info);
                if (info.Scope == AveNavigationScope.QuickLaunch)
                {
                    list.Insert(rank, info);
                }
                else
                {
                    list.Insert(list.Count, info);
                }
            }
            return list;
        }

        private int SearchChildNodePosition(List<AveNavigationInfo> children, AveNavigationInfo navNodeInfo)
        {
            if (children.Count == 0)
            {
                return 0;
            }
            int rank = navNodeInfo.RankChild;
            for (int i = 0; i < children.Count; i++)
            {
                if (rank <= children[i].RankChild)
                {
                    return i;
                }
            }
            return children.Count;
        }

        private AveNavigationNode RestoreOneNode(AveNavigationInfo navNodeInfo, CreateNavNodeOption option, ref AveNavigationNodeCollection parentCollection)
        {
            AveNavigationNode navNode = RestoreNavNodeInternal(navNodeInfo, ref  parentCollection, option);

            if (navNodeInfo.Scope == AveNavigationScope.QuickLaunch)
            {
                MoveToPos(navNode, navNodeInfo.RankChild, parentCollection);
            }

            if (navNode != null)
            {
                navNodeInfo.Children = SortNodeChildren(navNodeInfo.Children);
                foreach (AveNavigationInfo subNavNodeInfo in navNodeInfo.Children)
                {
                    mLogger.Info("Node name:{0},type:{1},isMoss:{2},url:{3}", subNavNodeInfo.Title, subNavNodeInfo.NodeType, WrapperRuntime.CurrentContext.IsMoss,subNavNodeInfo.Url);
                    option = (WrapperRuntime.CurrentContext.IsMoss && subNavNodeInfo.NodeType != -1) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;
                    AveNavigationNodeCollection children = navNode.Children as AveNavigationNodeCollection;
                    RestoreOneNode(subNavNodeInfo, option, ref children);
                }
            }

            return navNode;
        }

        private void MoveToPos(AveNavigationNode navNode, int rankChild, AveNavigationNodeCollection navNodeCollection)
        {
            try
            {
                if (navNode != null&& navNodeCollection!=null)
                {
                    if (rankChild >= navNodeCollection.Count)
                    {
                        navNode.MoveToLast(navNodeCollection);
                    }
                    else if (rankChild <= 0)
                    {
                        navNode.MoveToFirst(navNodeCollection);
                    }
                    else
                    {
                        navNode.Move(navNodeCollection, rankChild);
                    }
                }
            }
            catch(Exception ex)
            {
                mLogger.Warn("Move to pos error.Error:{0}", ex);
            }
        }

        private AveNavigationNode RestoreNavNodeInternal(AveNavigationInfo navNodeInfo, ref  AveNavigationNodeCollection parentCollection, CreateNavNodeOption option)
        {
            if (navNodeInfo.Url == null)
            {
                navNodeInfo.Url = "";
            }
            AveNavigationNode navNode = null;
            string url = navNodeInfo.Url;
            try
            {
                Hashtable propertyTable = new Hashtable();
                if (navNodeInfo.HasMetaInfo)
                {
                    propertyTable = GetProperties(navNodeInfo.MetaInfo);
                }
                string nodeType = string.Empty;
                if (propertyTable.ContainsKey("NodeType"))
                {
                    nodeType = propertyTable["NodeType"].ToString();
                }
                if (propertyTable.Contains("UrlQueryString") && propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                {
                    url = propertyTable["UrlQueryString"].ToString();
                }
                ReplaceOption replaceOption = new ReplaceOption(true, true); // opetion set to replace AbsoluteUrl and RelativeUrl
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                //兼容老数据备份IsExternal属性不正确的问题
                if (!navNodeInfo.IsExternal)
                {
                    bool? result = EnsureRelativeUrlExternalStatus(url, siteMappingManager.SourceSiteInfo.Url, siteMappingManager.SourceSiteInfo.ServerRelativeUrl);
                    if (result.HasValue)
                    {
                        navNodeInfo.IsExternal = result.Value;
                    }
                    mLogger.Info("EnsureExternalStatus.SourceSiteUrl:{0},SourceSiteRelativeUrl:{1},NodeUrl:{2},IsExternalInBackup:{3},EnsureResult.IsExternal:{4}",
                        siteMappingManager.SourceSiteInfo.Url, siteMappingManager.SourceSiteInfo.ServerRelativeUrl, url, false, navNodeInfo.IsExternal);
                }

                url = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                url = AveUrlUtility.ReplaceGroupId(siteMappingManager.SourceSiteInfo, url, GetGroupId());

                navNode = GetExistingNavNode(navNodeInfo.Eid, navNodeInfo.Title, url, nodeType, navNodeInfo.IsExternal, m_TempWeb, parentCollection);

                if (navNode == null)
                {
                    if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navNodeInfo.Eid))
                    {
                        navNode = CreateDefaultQuickLaunchHeading(m_TempWeb, (AveQuickLaunchHeading)navNodeInfo.Eid, navNodeInfo, propertyTable);

                        //m_Web.Site.ReloadSite();
                        Guid tempGuid = m_TempWeb.ID;
                        //if (m_TempWeb != null)
                        //{
                        //    m_TempWeb.Dispose();
                        //}
                        //m_TempWeb = m_Web.Site.OpenWeb(tempGuid) as AveWeb;
                        m_TempWeb = (m_Web.Site as AveSite).ReloadWeb(tempGuid) as AveWeb;
                        try
                        {
                            if (!m_TempWeb.AllowUnsafeUpdates)
                            {
                                m_TempWeb.AllowUnsafeUpdates = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Debug("Set web:{0} allowUnsafeUpdates failed.Error Message:{1}", m_TempWeb.ServerRelativeUrl, ex.ToString());
                        }
                        parentCollection = m_TempWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
                    }
                    else
                    {
                        navNode = CreateNavNode(url, navNodeInfo, propertyTable, parentCollection, option);
                    }
                }
                else if (!navNodeInfo.Title.Equals(navNode.Title))
                {
                    navNode.Title = navNodeInfo.Title;
                    navNode.Update();
                }
                else
                {
                    UpdateExistingNavNode(navNode, navNodeInfo, propertyTable, url);
                }
            }
            catch (Exception e)
            {
                //make sure logging not throw any exception, especially null reference
                mLogger.Debug(AveObjectModel_CommonResource.UpdateNavigationNodeErrorNewCreate, navNodeInfo.Title, this.m_Web.Url, e.ToString());
                //mLogger.Debug(AveObjectModel_CommonResource.UpdateNavigationNodeErrorNewCreate, navNode.Title, this.m_Web.Url, e.ToString());
                //mLog.Log(AveLogLevel.WARN, "WP10RTSPNavag284", navNodeInfo.Url, navNodeInfo.Title, url, e);
            }
            return navNode;
        }

        private static Hashtable GetProperties(string metainfo)
        {
            Hashtable prp = new Hashtable();
            string[] mSplitedString = metainfo.Replace("\r\n", "*").Split(new char[] { '*' });
            foreach (string mStr in mSplitedString)
            {
                int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                if (index1 < 0 && index2 < 0)
                {
                    continue;
                }
                string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                string value = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                prp.Add(key, value);
            }
            return prp;
        }

        private void UpdateExistingNavNode(IAveNavigationNode navNode, AveNavigationInfo navNodeInfo, Hashtable propertyTable, string url)
        {
            try
            {
                navNode.Title = navNodeInfo.Title;
                if (navNode.Properties != null)
                {
                    navNode.Properties["Target"] = navNodeInfo.Target;
                    if (propertyTable != null && propertyTable.ContainsKey("Description"))
                    {
                        navNode.Properties["Description"] = propertyTable["Description"].ToString();
                    }
                    if (propertyTable != null && propertyTable.ContainsKey("Audience"))
                    {
                        string audience = propertyTable["Audience"].ToString();
                        //if (mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                        //{
                        //    navNode.Properties["Audience"] = AveAudienceManager.ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                        //}
                    }
                    if (propertyTable != null && propertyTable.ContainsKey("UrlQueryString") && !propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                    {
                        navNode.Properties["UrlQueryString"] = propertyTable["UrlQueryString"].ToString();
                    }
                }
                navNode.Url = url;
                navNode.Update();
            }
            catch (Exception ex)
            {
                mLogger.Debug(AveObjectModel_CommonResource.UpdateNavigationNodeErrorExisting, navNode.Title, this.m_Web.Url, ex.ToString());
                //mLog.Warn("An error occurred while updating navigation node.ErrorMessage:{0}", ex.ToString());
            }
        }

        private AveNavigationNode CreateNavNode(string url, AveNavigationInfo navNodeInfo, Hashtable propertyTable, AveNavigationNodeCollection parentCollection, CreateNavNodeOption option)
        {
            AveNavigationNode navNode = null;
            if (option.Equals(CreateNavNodeOption.WithNodeType))
            {
                mLogger.Debug("Create node with Type:Title:{0},Url:{1},IsExternal:{2},NodeType:{3}", navNodeInfo.Title, url, navNodeInfo.IsExternal, navNodeInfo.NodeType.ToString());
                navNode = AveMOSSNavigation.CreateNavNode(navNodeInfo.Title, url,navNodeInfo.IsExternal, navNodeInfo.NodeType.ToString(), parentCollection);
            }
            else if (option.Equals(CreateNavNodeOption.WithoutNodeType))
            {
                mLogger.Debug("Create node without Type.Title:{0},Url:{1},IsExternal:{2}",navNodeInfo.Title, url, navNodeInfo.IsExternal);
                navNode = new AveNavigationNode(navNodeInfo.Title, url, navNodeInfo.IsExternal);
                navNode = parentCollection.AddAsLast(navNode) as AveNavigationNode;
            }
            if (navNode?.Properties != null)
            {
                navNode.Properties["Target"] = navNodeInfo.Target;
                if (propertyTable.ContainsKey("Description"))
                {
                    navNode.Properties["Description"] = propertyTable["Description"].ToString();
                }
                if (propertyTable.ContainsKey("Audience"))
                {
                    string audience = propertyTable["Audience"].ToString();
                    //if (mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                    //{
                    //    navNode.Properties["Audience"] = AveAudienceManager.ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                    //}
                }
                if (propertyTable.ContainsKey("UrlQueryString") && !propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                {
                    //RootFolder=%2Fsites%2Fsource%2FShared%20Documents%2Ffolder1&FolderCTID=0x0120007A870534FF42704A8C299F7E4F3B65DF&View={9908FBF0-E1A6-4B77-A384-7E30833B75E0}
                    navNode.Properties["UrlQueryString"] = propertyTable["UrlQueryString"].ToString();
                }
            }
            navNode?.Update();
            return navNode;
        }

        private AveNavigationNode GetExistingNavNode(int id, string title, string url, string nodeType, bool isExternal, AveWeb web, AveNavigationNodeCollection parentCollection)
        {
            AveNavigationNode navNode = null;
            if (Enum.IsDefined(typeof(AveQuickLaunchHeading), id))
            {
                navNode = web.Navigation.GetNodeById(id) as AveNavigationNode;
            }
            else
            {
                navNode = GetExistingSubNavNode(parentCollection, title, url, isExternal, nodeType);
            }
            return navNode;
        }

        private AveNavigationNode CreateDefaultQuickLaunchHeading(AveWeb web, AveQuickLaunchHeading quickLaunchHeading, AveNavigationInfo navNodeInfo, Hashtable propertyTable)
        {
            AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
            string destinationUrl = AveReplaceProcessor.UrlReplace(navNodeInfo.Url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            if (!destinationUrl.TrimStart('/').StartsWith(m_Web.Site.ServerRelativeUrl.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(destinationUrl))//recent节点还原 SAAS-3834
                {
                    destinationUrl = m_Web.Site.ServerRelativeUrl;
                }
                else
                {
                    destinationUrl = AveUrlUtility.CombineUrl(m_Web.Site.ServerRelativeUrl.TrimStart('/') + "/", destinationUrl.TrimStart('/'));
                }                
            }
            AveNavigationNode headingNode = null;
            AveNavigationNode tmpNode = new AveNavigationNode(navNodeInfo.Title, destinationUrl, navNodeInfo.IsExternal);
            headingNode = web.Navigation.AddToQuickLaunch(tmpNode, quickLaunchHeading) as AveNavigationNode;
            return headingNode;
        }

        private AveNavigationNode GetExistingSubNavNode(AveNavigationNodeCollection navNodeCollection, string title, string url, bool isExternal, string nodeType)
        {
            AveNavigationNode navNode = null;

            foreach (AveNavigationNode node in navNodeCollection)
            {
                if ((node.IsExternal == isExternal) && string.Compare(node.Url, url, StringComparison.OrdinalIgnoreCase) == 0
                    && string.Compare(node.Title, title, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(nodeType) || (
                        node.Properties.ContainsKey("NodeType") && node.Properties["NodeType"].ToString().Equals(nodeType, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        navNode = node;
                        break;
                    }
                }
            }
            return navNode;
        }

        private void ClearAllNodes(IAveNavigationNodeCollection navNodeCollection)
        {
            try
            {
                for (int i = navNodeCollection.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navNodeCollection[i].ID))
                        {
                            IAveNavigationNodeCollection co = navNodeCollection[i].Children;
                            for (int index = co.Count - 1; index >= 0; index--)
                            {
                                co[index].Delete();
                            }
                            continue;
                        }
                        navNodeCollection.Delete(navNodeCollection[i]);
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveObjectModel_CommonResource.DeleteNavNodeError, navNodeCollection[i].Title, this.m_Web.Url, e.ToString());
                        //mLog.Log(AveLogLevel.WARN, "An error occurred occurred while delete navigation node. error:{0}", e.ToString());
                        //mLog.Warn("An error occurred occurred while delete navigation node. error:{0}", e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Debug(AveObjectModel_CommonResource.ClearAllNavNodesError, this.m_Web.Url, e.ToString());
                //mLog.Log(AveLogLevel.WARN, "An error occurred while clear navigation nodes. error:{0}", e.ToString());
                //mLog.Warn("An error occurred while clear navigation nodes. error:{0}", e.ToString());
            }
        }

        private bool RecentQuickLaunchIsEmpty(AveWeb m_TempWeb)
        {
            try
            {
                m_TempWeb.RemoveNavigation();
                AveNavigationNodeCollection quickLaunchNavCollection = m_TempWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
                IAveNavigationNode recentquickLuanch = quickLaunchNavCollection.Where(nav => nav.ID.Equals((int)AveQuickLaunchHeading.Recent)).FirstOrDefault();
                if (recentquickLuanch != null)
                {
                    int childrenCount = recentquickLuanch.Children.Count;
                    ClearAllNodes(quickLaunchNavCollection);
                    if (childrenCount == 5)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Error occurred while deleting quick launch links under Recent. Ex:{0}", ex.ToString());
                return true;
            }
        }
    }

    public enum CreateNavNodeOption
    {
        WithNodeType,
        WithoutNodeType
    }

    class AveMOSSNavigation
    {
        public static AveNavigationNode CreateNavNode(string title, string url,bool isExternal, string type, IAveNavigationNodeCollection navNodeCollection)
        {
            AveNavigationNode node = null;

            AveNodeTypes nodeType = (AveNodeTypes)(Enum.Parse(typeof(AveNodeTypes), type));

            AveNavigationSiteMapNode creator = new AveNavigationSiteMapNode();

            node = creator.CreateSPNavigationNode(title, url, isExternal, nodeType, navNodeCollection) as AveNavigationNode;

            return node;
        }
    }
}
