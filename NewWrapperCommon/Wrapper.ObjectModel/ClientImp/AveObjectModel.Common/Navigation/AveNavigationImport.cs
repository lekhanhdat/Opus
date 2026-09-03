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
        internal IReport mReportor;

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
                    mLogger.Debug("Set web:{0} allowUnsafeUpdates failed.Error Message:{1}",m_TempWeb.ServerRelativeUrl,ex.ToString());
                }
                if (m_TempWeb != null)
                {
                    bool? compoundSupport = null;
                    AveNavigationInfoList value = data.Value;

                    RestorePublishAppearance(value.PublishFeatureAppearance);
                    AveNavigationNodeCollection topBarNavCollection = m_TempWeb.Navigation.TopNavigationBar as AveNavigationNodeCollection;
                    AveNavigationNodeCollection quickLaunchNavCollection = m_TempWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
                    //与local保持一致
                    if (NeedRestore(AveNavigationScope.TopNavigationBar, value))
                    {
                        ClearAllNodes(topBarNavCollection);
                    }
                    if (NeedRestore(AveNavigationScope.QuickLaunch, value))
                    {
                        ClearAllNodes(quickLaunchNavCollection);
                    }
                    //如果site的Publishing Feature开启的话, 则执行WebServiceReqeust的还原逻辑;
                    if (m_Web.Site.Features[ AveSP2010FeatureDefinitions.PublishingSite] != null)
                    {
                        ReplaceUrlAndTitle(value.NavNodes);
                        compoundSupport = m_TempWeb.Navigation.RestoreNavigation(value, this.NavigationRestoreSetting);
                    }
                    //如果Publishing Freaure没有开启, 或者OMReeqest执行失败的, 则使用正常逻辑Restore;
                    if (!compoundSupport.HasValue || !compoundSupport.Value)
                    {
                        value.NavNodes = SortNodeChildren(value.NavNodes);
                        var noNeedReplaceUrl = compoundSupport.HasValue && !compoundSupport.Value; // 是否进入过 m_Web.Site.Features[ AveSP2010FeatureDefinitions.PublishingSite] 逻辑，已经替换过url，不需要再次替换，否则会跟option keepExternalUrl 冲突，把目的端url 加上源端前缀
                        foreach (AveNavigationInfo navInfo in value.NavNodes.Where(navInfo => NeedRestore(navInfo.Scope, value)))
                        {
                            try
                            {
                                CreateNavNodeOption nodeCreateOption = (WrapperRuntime.CurrentContext.IsMoss && navInfo.NodeType != -1 && m_Web.Site.IsPublish) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;

                                if (navInfo.Scope.Equals(AveNavigationScope.TopNavigationBar))
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, noNeedReplaceUrl, ref topBarNavCollection);
                                }
                                else if (navInfo.Scope.Equals(AveNavigationScope.QuickLaunch))
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, noNeedReplaceUrl, ref quickLaunchNavCollection);
                                }
                                mReportor.AddDetail(new AveWrapperReportDto(navInfo.Title, m_TempWeb.Title, AveReportObjectType.WebNavigation, AveStatus.Successful, string.Empty));
                            }
                            catch (Exception e)
                            {
                                mLogger.Debug(AveObjectModel_CommonResource.RestoreNavigationNodeError, navInfo.Title, this.m_Web.Url, e);
                                mReportor.AddDetail(new AveWrapperReportDto(navInfo.Title, m_TempWeb.Title, AveReportObjectType.WebNavigation, AveStatus.Failed, e.Message));
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
                if (m_TempWeb.IsRootWeb && value.ShareQuickLaunch)
                {
                    return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveQuickLunch || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                }
                bool quickLunchIsInherited = m_TempWeb.AllProperties != null &&
                       m_TempWeb.AllProperties["__InheritCurrentNavigation"] != null &&
                       string.Equals(m_TempWeb.AllProperties["__InheritCurrentNavigation"].ToString(), "True", StringComparison.OrdinalIgnoreCase);

                return !quickLunchIsInherited || !value.BackupFromInheritedWeb;
            }
            return true;
        }

        private void ReplaceUrlAndTitle(List<AveNavigationInfo> navigationNodes)
        {
            AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
            foreach (AveNavigationInfo navInfo in navigationNodes)
            {
                Guid sourceItemId;
                if (AveUrlUtility.IsDurableLink(navInfo.Url, out sourceItemId))
                {
                    string mappingUrl;
                    if (siteMappingManager.TryGetDurableLinkUrl(sourceItemId, out mappingUrl))
                    {
                        navInfo.Url = mappingUrl;
                    }
                }
                ReplaceOption replaceOption = new ReplaceOption(true, true, NavigationRestoreSetting.KeepExternalRelativeUrl);
                navInfo.Url = AveReplaceProcessor.UrlReplace(navInfo.Url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                ReplaceUrlAndTitle(navInfo.Children);
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
                list.Insert(rank, info);
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

        private AveNavigationNode RestoreOneNode(AveNavigationInfo navNodeInfo, CreateNavNodeOption option, bool noNeedReplaceUrl, ref AveNavigationNodeCollection parentCollection)
        {
            AveNavigationNode navNode = RestoreNavNodeInternal(navNodeInfo, noNeedReplaceUrl, ref  parentCollection, option);

            MoveToPos(navNode, navNodeInfo.RankChild, parentCollection);

            if (navNode != null)
            {
                navNodeInfo.Children = SortNodeChildren(navNodeInfo.Children);
                foreach (AveNavigationInfo subNavNodeInfo in navNodeInfo.Children)
                {
                    option = (WrapperRuntime.CurrentContext.IsMoss && subNavNodeInfo.NodeType != -1 && m_Web.Site.IsPublish) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;
                    AveNavigationNodeCollection children = navNode.Children as AveNavigationNodeCollection;
                    RestoreOneNode(subNavNodeInfo, option, noNeedReplaceUrl,ref children);
                }
            }

            return navNode;
        }

        private void MoveToPos(AveNavigationNode navNode, int rankChild, AveNavigationNodeCollection navNodeCollection)
        {
            if (navNode != null)
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

        private AveNavigationNode RestoreNavNodeInternal(AveNavigationInfo navNodeInfo, bool noNeedReplaceUrl, ref  AveNavigationNodeCollection parentCollection, CreateNavNodeOption option)
        {
            if (navNodeInfo.Url == null)
            {
                navNodeInfo.Url = "";
            }
            AveNavigationNode navNode = null;
            string url = navNodeInfo.Url;
            try
            {
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                Guid sourceItemId;
                if (AveUrlUtility.IsDurableLink(url, out sourceItemId))
                {
                    string mappingUrl;
                    if (siteMappingManager.TryGetDurableLinkUrl(sourceItemId, out mappingUrl))
                    {
                        url = mappingUrl;
                    }
                }
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

                if (!noNeedReplaceUrl)
                {
                    ReplaceOption replaceOption = new ReplaceOption(true, true, NavigationRestoreSetting.KeepExternalRelativeUrl); // opetion set to replace AbsoluteUrl and RelativeUrl
                    url = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                }

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
                            mLogger.Debug("Set web:{0} allowUnsafeUpdates failed.Error Message:{1}",m_TempWeb.ServerRelativeUrl,ex.ToString());
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
                navNode.Url = String.IsNullOrEmpty(url) ? null : url;  //url如果赋值为empty, sharepoint会自动更新为site的相对url
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
                navNode = AveMOSSNavigation.CreateNavNode(navNodeInfo.Title, url, navNodeInfo.NodeType.ToString(), parentCollection);
            }
            else if (option.Equals(CreateNavNodeOption.WithoutNodeType))
            {
                navNode = new AveNavigationNode(navNodeInfo.Title, url, navNodeInfo.IsExternal);
                navNode = parentCollection.AddAsLast(navNode) as AveNavigationNode;
            }
            if (navNode.Properties != null)
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
            navNode.Update();
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
            string destinationUrl = AveReplaceProcessor.UrlReplace(navNodeInfo.Url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true, NavigationRestoreSetting.KeepExternalRelativeUrl), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                destinationUrl = null;//不设为空则为siteurl
            }
            else if (!destinationUrl.TrimStart('/').StartsWith(m_Web.Site.ServerRelativeUrl.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
            {
                destinationUrl = AveUrlUtility.CombineUrl(m_Web.Site.ServerRelativeUrl.TrimStart('/') + "/", destinationUrl.TrimStart('/'));
            }
            AveNavigationNode headingNode = null;
            AveNavigationNode tmpNode = new AveNavigationNode(navNodeInfo.Title, destinationUrl, navNodeInfo.IsExternal);
            headingNode = web.Navigation.AddToQuickLaunch(tmpNode, quickLaunchHeading) as AveNavigationNode;
            return headingNode;
            /*
            //tmpNode.Delete();
            //headingNode = web.Navigation.GetNodeById((int)quickLaunchHeading) as AveNavigationNode;
            headingNode.Title = navNodeInfo.Title;
            headingNode.Properties["Target"] = navNodeInfo.Target;
            if (propertyTable.ContainsKey("Description"))
            {
                headingNode.Properties["Description"] = propertyTable["Description"].ToString();
            }
            if (propertyTable.ContainsKey("Audience"))
            {
                string audience = propertyTable["Audience"].ToString();
                //do it later
                //if (mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                //{
                //    headingNode.Properties["Audience"] = AveAudienceManager.ReplaceAudienceId(mAveParentSite.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                //}
            }
            if (propertyTable.ContainsKey("UrlQueryString") && !propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
            {
                //RootFolder=%2Fsites%2Fsource%2FShared%20Documents%2Ffolder1&FolderCTID=0x0120007A870534FF42704A8C299F7E4F3B65DF&View={9908FBF0-E1A6-4B77-A384-7E30833B75E0}
                headingNode.Properties["UrlQueryString"] = propertyTable["UrlQueryString"].ToString();
            }
            if (!headingNode.Url.TrimStart('/').StartsWith(m_Web.Site.ServerRelativeUrl.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
            {
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                string destinationUrl = AveReplaceProcessor.UrlReplace(navNodeInfo.Url, siteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                if (destinationUrl.TrimStart('/').StartsWith(m_Web.Site.ServerRelativeUrl.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
                {
                    headingNode.Url = destinationUrl;
                }
                else
                {
                    headingNode.Url = AveUrlUtility.CombineUrl(m_Web.Site.ServerRelativeUrl.TrimStart('/') + "/", destinationUrl.TrimStart('/'));
                }
            }
            headingNode.Update();
            return headingNode;
            */
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
                            //continue;
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
    }

    public enum CreateNavNodeOption
    {
        WithNodeType,
        WithoutNodeType
    }

    class AveMOSSNavigation
    {
        public static AveNavigationNode CreateNavNode(string title, string url, string type, IAveNavigationNodeCollection navNodeCollection)
        {
            AveNavigationNode node = null;

            AveNodeTypes nodeType = (AveNodeTypes)(Enum.Parse(typeof(AveNodeTypes), type));

            AveNavigationSiteMapNode creator = new AveNavigationSiteMapNode();

            node = creator.CreateSPNavigationNode(title, url, nodeType, navNodeCollection) as AveNavigationNode;

            return node;
        }
    }
}
