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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveNavigationImport
    {
        readonly static AveLogger logger = AveLogger.GetInstance(typeof(AveNavigationImport));

        public NavigationRestoreSetting NavigationRestoreSetting { set; get; }

        private readonly AveSite mAveSite;

        private AveWeb mCurrentWeb;

        internal AveWeb CurrentWeb
        {
            set { mCurrentWeb = value; }
            get
            {
                if (!mCurrentWeb.AllowUnsafeUpdates)
                {
                    mCurrentWeb.AllowUnsafeUpdates = true;
                }
                return mCurrentWeb;
            }
        }
        private List<int> newCreatedNodes = new List<int>();
        private bool alertMappingInitialized;
        internal IReport mReportor;

        public AveNavigationImport(AveWeb web)
        {
            mAveSite = web.Site as AveSite;
        }

        public AveNavigationImport(AveWeb web, NavigationRestoreSetting importSetting)
        {
            mAveSite = web.Site as AveSite;
        }

        public void Run(KeyValuePair<Guid, AveNavigationInfoList> data)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.Run"))
            {

                alertMappingInitialized = false;
                CurrentWeb = null;
                try
                {
                    CurrentWeb = mAveSite.OpenWeb(data.Key) as AveWeb;
                    if (CurrentWeb == null)
                    {
                        return;
                    }

                    newCreatedNodes.Clear();
                    var value = data.Value;
                    bool needRestoreTopLinkBar = NeedRestore(AveNavigationScope.TopNavigationBar, value);
                    bool needRestoreQuickLaunch = NeedRestore(AveNavigationScope.QuickLaunch, value);
                    bool needRestoreSearchNavigation = NeedRestore(AveNavigationScope.SearchNavigation, value);

                    ClearAllNodes(needRestoreTopLinkBar, needRestoreQuickLaunch, needRestoreSearchNavigation);

                    RestorePublishAppearance(value.PublishFeatureAppearance);
                    value.NavNodes = SortNodeChildren(value.NavNodes);
                    foreach (AveNavigationInfo navInfo in value.NavNodes.Where(navInfo => NeedRestore(navInfo.Scope, value)))
                    {
                        try
                        {
                            var nodeCreateOption = (WrapperRuntime.CurrentContext.IsMoss && navInfo.NodeType != -1 && AvePublishing.IsPublishingSite(mAveSite)) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;

                            if (navInfo.Scope.Equals(AveNavigationScope.TopNavigationBar))
                            {
                                if (this.CurrentWeb.Navigation.TopNavigationBar != null)
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, this.CurrentWeb.Navigation.TopNavigationBar as AveNavigationNodeCollection);
                                }
                                else
                                {
                                    logger.Warn("The destination site's TopNavigationBar is null, site url: {0}", this.CurrentWeb.Url);
                                }
                            }
                            else if (navInfo.Scope.Equals(AveNavigationScope.QuickLaunch))
                            {
                                if (this.CurrentWeb.Navigation.QuickLaunch != null)
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, this.CurrentWeb.Navigation.QuickLaunch as AveNavigationNodeCollection);
                                }
                                else
                                {
                                    logger.Warn("The destination site's QuickLaunch is null, site url: {0}", this.CurrentWeb.Url);
                                }
                            }
                            else if (navInfo.Scope.Equals(AveNavigationScope.SearchNavigation))
                            {
                                if (this.CurrentWeb.Navigation.SearchNav != null)
                                {
                                    RestoreOneNode(navInfo, nodeCreateOption, this.CurrentWeb.Navigation.SearchNav as AveNavigationNodeCollection);
                                }
                                else
                                {
                                    logger.Warn("The destination site's Configure Search Navigation is null, site url: {0}", this.CurrentWeb.Url);
                                }
                            }
                            mReportor.AddDetail(new AveWrapperReportDto(navInfo.Title, CurrentWeb.Title, AveReportObjectType.WebNavigation, AveStatus.Successful, string.Empty));
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.WARN, "An error occurred while restoring navigation node. name:{0} , error:{1}", navInfo.Title, e);
                            mReportor.AddDetail(new AveWrapperReportDto(navInfo.Title, CurrentWeb.Title, AveReportObjectType.WebNavigation, AveStatus.Failed, e.Message));
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.NaviNodeRestoreError, data.Key, e);
                }
                finally
                {
                    if (CurrentWeb != null) CurrentWeb.Dispose();
                }

            }

        }

        private void RestorePublishAppearance(bool isPublishing)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.RestorePublishAppearance"))
            {

                try
                {
                    if (isPublishing && CurrentWeb.Site.Features[AveConstants.PUBLISHINGRESOURCES] == null)
                    {
                        CurrentWeb.Site.Features.Add(AveConstants.OFFICEPUBLISHINGSITE, true);
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.ActivateFeatureFailedEventMessage(AveConstants.OFFICEPUBLISHINGSITE, e));
                }

            }

        }

        private bool NeedRestore(AveNavigationScope aveNavigationScope, AveNavigationInfoList value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.InheritNavNodesFromParent"))
            {

                if (aveNavigationScope == AveNavigationScope.TopNavigationBar)
                {
                    if (CurrentWeb.IsRootWeb && value.SharedTopLink)
                    {
                        return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveTopLink || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                    }
                    return !CurrentWeb.Navigation.UseShared;
                }
                if (aveNavigationScope == AveNavigationScope.QuickLaunch)
                {
                    if (CurrentWeb.IsRootWeb && value.ShareQuickLaunch)
                    {
                        return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveQuickLunch || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                    }
                    if (CurrentWeb.Template.Equals("BDR#0", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    bool quickLunchIsInherited = CurrentWeb.AllProperties != null &&
                           CurrentWeb.AllProperties["__InheritCurrentNavigation"] != null &&
                           string.Equals(CurrentWeb.AllProperties["__InheritCurrentNavigation"].ToString(), "True", StringComparison.OrdinalIgnoreCase);
                    return !quickLunchIsInherited || !value.BackupFromInheritedWeb;
                }
                if (aveNavigationScope == AveNavigationScope.SearchNavigation)
                {
                    return !CurrentWeb.Navigation.UseShared;
                }
                return true;

            }

        }

        private List<AveNavigationInfo> SortNodeChildren(IEnumerable<AveNavigationInfo> children)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.SortNodeChildren"))
            {

                if (children == null)
                {
                    return null;
                }
                var list = new List<AveNavigationInfo>();
                foreach (var info in children)
                {
                    int rank = SearchChildNodePosition(list, info);
                    list.Insert(rank, info);
                }
                return list;

            }

        }

        private int SearchChildNodePosition(List<AveNavigationInfo> children, AveNavigationInfo navNodeInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.SortNodeChildren"))
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

        }

        private static Hashtable GetProperties(string metainfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.GetProperties"))
            {

                Hashtable prp = new Hashtable();
                string[] mSplitedString = metainfo.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string mStr in mSplitedString)
                {
                    int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index1 < 0 && index2 < 0)
                    {
                        continue;
                    }
                    string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                    object value = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                    if (index2 - index1 - 1 > 0)
                    {
                        string valueType = mStr.Substring(index1 + 1, index2 - index1 - 1);
                        switch (valueType)
                        {
                            case "TW":
                                value = Convert.ToDateTime(value).ToUniversalTime();
                                break;
                            case "SW":
                            default:
                                break;
                        }
                    }
                    prp.Add(key, value);
                }
                return prp;

            }

        }

        private void MoveToPos(AveNavigationNode navNode, int rankChild, AveNavigationNodeCollection navNodeCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.MoveToPos"))
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
                        navNode.Move(navNodeCollection, navNodeCollection[rankChild]);
                    }
                }

            }

        }

        private void RestoreOneNode(AveNavigationInfo navNodeInfo, CreateNavNodeOption option, AveNavigationNodeCollection parentCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.RestoreOneNode"))
            {

                AveNavigationNode navNode = RestoreNavNodeInternal(navNodeInfo, ref parentCollection, option);

                MoveToPos(navNode, navNodeInfo.RankChild, parentCollection);

                if (navNode != null)
                {
                    navNodeInfo.Children = SortNodeChildren(navNodeInfo.Children);
                    foreach (AveNavigationInfo subNavNodeInfo in navNodeInfo.Children)
                    {
                        option = (WrapperRuntime.CurrentContext.IsMoss && subNavNodeInfo.NodeType != -1 && AvePublishing.IsPublishingSite(mAveSite)) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;
                        RestoreOneNode(subNavNodeInfo, option, navNode.Children as AveNavigationNodeCollection);
                    }
                }

            }

        }

        private AveNavigationNode RestoreNavNodeInternal(AveNavigationInfo navNodeInfo, ref AveNavigationNodeCollection parentCollection, CreateNavNodeOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.RestoreNavNodeInternal"))
            {

                if (navNodeInfo.Url == null)
                {
                    navNodeInfo.Url = "";
                }
                AveNavigationNode navNode = null;
                string url = navNodeInfo.Url;
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

                if (propertyTable.ContainsKey("UrlQueryString") && !string.IsNullOrEmpty(propertyTable["UrlQueryString"].ToString()))
                {
                    //ADO-200849 经测试发现SP13 SP16的url 属性包含query string部分，SP10的url属性 不包含query string 部分
                    if (!url.Contains(string.Concat("?", propertyTable["UrlQueryString"].ToString())))
                    {
                        url = string.Concat(url, "?", propertyTable["UrlQueryString"]);
                    }
                    propertyTable.Remove("UrlQueryString");
                }

                if (!alertMappingInitialized && url.Contains("Alert={"))
                {//需要替换Alert的时候再去查询
                    WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AddAlertIdMapping(this.mCurrentWeb.GetWebAlerts()
                        .SelectMany(node => node.Value)
                        .ToDictionary(node => node.Key, node => node.Value));
                    alertMappingInitialized = true;
                }

                ReplaceOption replaceOption = new ReplaceOption(true, true, NavigationRestoreSetting.KeepExternalRelativeUrl); // opetion set to replace AbsoluteUrl and RelativeUrl
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                url = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);

                navNode = GetExistingNavNode(navNodeInfo.Eid, navNodeInfo.Title, url, nodeType, navNodeInfo.IsExternal, parentCollection);

                if (navNode == null)
                {
                    if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navNodeInfo.Eid))
                    {
                        navNode = CreateDefaultQuickLaunchHeading((AveQuickLaunchHeading)navNodeInfo.Eid, navNodeInfo, propertyTable, url);
                        navNode = CurrentWeb.Navigation.GetNodeById(navNodeInfo.Eid) as AveNavigationNode;
                        if (navNode == null)
                        {
                            mAveSite.ReloadSite();
                            Guid tempGuid = CurrentWeb.ID;
                            if (CurrentWeb != null)
                            {
                                CurrentWeb.Dispose();
                            }
                            CurrentWeb = mAveSite.OpenWeb(tempGuid) as AveWeb;
                            navNode = CurrentWeb.Navigation.GetNodeById(navNodeInfo.Eid) as AveNavigationNode;
                        }
                        parentCollection = CurrentWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
                        //Heading Node Do not need to restore properties again,return here.
                        return navNode;
                    }
                    else
                    {
                        navNode = CreateNavNode(url, navNodeInfo, propertyTable, parentCollection, option);
                    }

                    RecordNewCreatedNode(navNode);
                }
                else
                {
                    UpdateExistingNavNode(navNode, navNodeInfo, propertyTable, url);
                }
                RestoreTitleResource(navNode, navNodeInfo);
                RestoreNavNodesProperties(navNodeInfo, propertyTable, navNode);
                if (navNode != null)
                {
                    navNode.Update();
                }
                return navNode;

            }

        }

        private void RestoreTitleResource(AveNavigationNode navNode, AveNavigationInfo navNodeInfo)
        {
            if (navNode != null)
            {
                navNode.TitleResource.SetUserResource(CurrentWeb, navNodeInfo.TitleResource);
            }
        }

        private void RecordNewCreatedNode(AveNavigationNode navNode)
        {
            if (navNode != null && !newCreatedNodes.Contains(navNode.ID))
            {
                newCreatedNodes.Add(navNode.ID);
            }
        }

        private void UpdateExistingNavNode(IAveNavigationNode navNode, AveNavigationInfo navNodeInfo, Hashtable propertyTable, string url)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.UpdateExistingNavNode"))
            {

                navNode.Title = navNodeInfo.Title;
                navNode.Url = String.IsNullOrEmpty(url) ? null : url;  //url如果赋值为empty, sharepoint会自动更新为site的相对url

            }

        }

        private AveNavigationNode CreateNavNode(string url, AveNavigationInfo navNodeInfo, Hashtable propertyTable, AveNavigationNodeCollection parentCollection, CreateNavNodeOption option)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.CreateNavNode"))
            {

                AveNavigationNode navNode = null;
                if (option.Equals(CreateNavNodeOption.WithNodeType))
                {
                    navNode = AveMOSSNavigation.CreateNavNode(navNodeInfo.Title, url, navNodeInfo.NodeType.ToString(), parentCollection);


                    if (navNode.IsExternal && !string.IsNullOrEmpty(url))
                    {
                        IAveNavigationNode tempNode = parentCollection.Navigation.GetNodeById(navNode.ID);

                        var inertalTypes = new string[] { "Area" };//Some NodeTypes IsExternal is always false, delete it when it is true

                        if (null != tempNode && inertalTypes.Contains(tempNode.Properties["NodeType"]))
                        {
                            parentCollection.Delete(tempNode);
                            return null;
                        }
                    }
                }
                else if (option.Equals(CreateNavNodeOption.WithoutNodeType))
                {
                    navNode = new AveNavigationNode(navNodeInfo.Title, url, navNodeInfo.IsExternal);
                    try
                    {
                        navNode = parentCollection.AddAsLast(navNode) as AveNavigationNode;
                    }
                    catch (SPException)
                    {
                        if (!navNodeInfo.IsExternal && navNodeInfo.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && NavigationRestoreSetting.ForceKeepNode)
                        {
                            navNode = new AveNavigationNode(navNodeInfo.Title, url, true);
                            navNode = parentCollection.AddAsLast(navNode) as AveNavigationNode;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                RestoreNavNodesProperties(navNodeInfo, propertyTable, navNode);
                if(navNode != null)
                {
                    navNode.Update();
                }
                return navNode;

            }

        }

        private void RestoreNavNodesProperties(AveNavigationInfo navNodeInfo, Hashtable propertyTable, AveNavigationNode navNode)
        {
            if (propertyTable == null)
            {
                return;
            }
            if (navNode == null || navNode.Properties == null)
            {
                //Not sure what we need to do now
                return;
            }
            //在propertyTable中有几个元素在navNode.Property中没有，如以后遇到navigation node属性还原问题，可以考虑为navNode.Property添加元素（从propertyTable中取）。
            navNode.Properties["Target"] = navNodeInfo.Target;
            //restore created date.
            if (propertyTable.ContainsKey("CreatedDate"))
            {
                navNode.Properties["CreatedDate"] = propertyTable["CreatedDate"];
            }
            if (propertyTable.ContainsKey("LastModifiedDate"))
            {
                navNode.Properties["LastModifiedDate"] = propertyTable["LastModifiedDate"];
            }

            if (propertyTable.ContainsKey("Description"))
            {
                navNode.Properties["Description"] = propertyTable["Description"].ToString().Replace("\\r\\n", "\r\n");
            }

            if (propertyTable.ContainsKey("Audience"))
            {
                string audience = propertyTable["Audience"].ToString();
                audience = audience.Replace("\\n", "\n");
                navNode.Properties["Audience"] = ReplaceAudienceId(WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager, audience);
            }

            //if (propertyTable.ContainsKey("UrlQueryString") && !propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
            //{
            //    navNode.Properties["UrlQueryString"] = propertyTable["UrlQueryString"].ToString();
            //}

            if (propertyTable.ContainsKey("UrlFragment"))
            {
                navNode.Properties["UrlFragment"] = propertyTable["UrlFragment"].ToString();
            }
        }

        private AveNavigationNode GetExistingNavNode(int id, string title, string url, string nodeType, bool isExternal, AveNavigationNodeCollection parentCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.GetExistingNavNode"))
            {

                AveNavigationNode navNode = null; ;
                if (Enum.IsDefined(typeof(AveQuickLaunchHeading), id))
                {
                    navNode = CurrentWeb.Navigation.GetNodeById(id) as AveNavigationNode;
                }
                if (!Enum.IsDefined(typeof(AveQuickLaunchHeading), id) || (navNode != null && string.Compare(navNode.Title, title, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    try
                    {
                        navNode = GetExistingSubNavNode(parentCollection, title, url, isExternal, nodeType);
                    }
                    catch (Exception e)
                    {
                        navNode = null;
                        logger.Warn("GetExistsSubNavNode error", e);
                    }
                }
                if (navNode != null && newCreatedNodes.Contains(navNode.ID))
                {
                    return null;
                }
                return navNode;

            }

        }

        private AveNavigationNode GetExistingSubNavNode(AveNavigationNodeCollection navNodeCollection, string title, string url, bool isExternal, string nodeType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.GetExistingSubNavNode"))
            {

                AveNavigationNode navNode = null;

                foreach (AveNavigationNode node in navNodeCollection)
                {
                    if ((node.IsExternal == isExternal) && string.Compare(node.Url, url, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(node.Title, title, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (string.IsNullOrEmpty(nodeType) || (node.Properties.ContainsKey("NodeType") && node.Properties["NodeType"].ToString().Equals(nodeType, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            navNode = node;
                            break;
                        }
                    }
                }
                return navNode;

            }

        }

        private AveNavigationNode CreateDefaultQuickLaunchHeading(AveQuickLaunchHeading quickLaunchHeading, AveNavigationInfo navNodeInfo, Hashtable propertyTable, string url)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.CreateDefaultQuickLaunchHeading"))
            {

                AveNavigationNode tmpNode = new AveNavigationNode("", "", true);
                tmpNode = CurrentWeb.Navigation.AddToQuickLaunch(tmpNode, quickLaunchHeading) as AveNavigationNode;
                tmpNode.Delete();
                AveNavigationNode headingNode = CurrentWeb.Navigation.GetNodeById((int)quickLaunchHeading) as AveNavigationNode;
                headingNode.Title = navNodeInfo.Title;
                if (string.IsNullOrEmpty(url))
                {
                    //不赋null 会变成当前site 的url
                    headingNode.Url = null;
                }
                else
                {
                    headingNode.Url = url;
                }
                RestoreNavNodesProperties(navNodeInfo, propertyTable, headingNode);
                headingNode.Update();
                return headingNode;

            }

        }

        private string ReplaceAudienceId(AveSiteMappingManager siteMappingMnager, string oldValue)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.ReplaceAudienceId"))
            {

                if (string.IsNullOrEmpty(oldValue))
                {
                    return oldValue;
                }
                if (oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return oldValue;
                }
                string tempValue = oldValue.Substring(0, oldValue.IndexOf(";;", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(tempValue))
                {
                    return oldValue;
                }
                string newValue = oldValue;
                string[] tValues = tempValue.Split(',');
                foreach (string tValue in tValues)
                {
                    string value;
                    if (siteMappingMnager.GetValueFromAudienceIDMapping(tValue, out value))
                    {
                        newValue = newValue.Replace(tValue, value);
                    }
                }
                return newValue;

            }

        }

        private void ClearAllNodes(bool needRestoreTopLinkBar, bool needRestoreQuickLaunch, bool needRestoreSearchNavigation)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationImport.ClearAllNodes"))
            {

                if (needRestoreTopLinkBar)
                {
                    RealClear(CurrentWeb.Navigation.TopNavigationBar);
                }
                if (needRestoreQuickLaunch)
                {
                    RealClear(CurrentWeb.Navigation.QuickLaunch);
                }
                if (needRestoreSearchNavigation)
                {
                    RealClear(CurrentWeb.Navigation.SearchNav);
                }

            }

        }

        private static void RealClear(IAveNavigationNodeCollection navNodeCollection)
        {
            if (navNodeCollection != null)
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
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.NaviNodeClearError, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.NaviNodeClearError, ex);
                }
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