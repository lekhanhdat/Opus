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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveNavigationImport
    {
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

        public AveNavigationImport(AveWeb web)
        {
            mAveSite = web.Site as AveSite;
        }               

        public void Run(KeyValuePair<Guid, AveNavigationInfoList> data)
        {
            Guid key = data.Key;
            CurrentWeb = null;
            try
            {
                CurrentWeb = mAveSite.OpenWeb(key) as AveWeb;

                if (CurrentWeb == null)
                {
                    return;
                }

                AveNavigationInfoList value = data.Value;

                ClearAllNodes();

                value.NavNodes = SortNodeChildren(value.NavNodes);
                foreach (AveNavigationInfo navInfo in value.NavNodes)
                {
                    if (navInfo.Scope == AveNavigationScope.TopNavigationBar && CurrentWeb.Navigation.UseShared)
                    {
                        //mLog.Info("web:{0} inherit global navigation from parent site.", tempWeb.Url);
                        continue;
                    }
                    if (navInfo.Scope == AveNavigationScope.QuickLaunch && CurrentWeb.Properties != null && CurrentWeb.Properties["__InheritCurrentNavigation"] == "True")
                    {
                        //mLog.Info("web:{0} inherit current navigation from parent site.", tempWeb.Url);
                        continue;
                    }
                    try
                    {                     
                        CreateNavNodeOption nodeCreateOption = (WrapperRuntime.CurrentContext.IsMoss && navInfo.NodeType != -1 && AvePublishing.IsPublishingSite(mAveSite)) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;

                        if (navInfo.Scope.Equals(AveNavigationScope.TopNavigationBar))
                        {
                            RestoreOneNode(navInfo, nodeCreateOption, this.CurrentWeb.Navigation.TopNavigationBar as AveNavigationNodeCollection);
                        }
                        else if (navInfo.Scope.Equals(AveNavigationScope.QuickLaunch))
                        {
                            RestoreOneNode(navInfo, nodeCreateOption, this.CurrentWeb.Navigation.QuickLaunch as AveNavigationNodeCollection);
                        }
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.WARN, "WP10RTSPNavag135", key, navInfo.Title, navInfo.Url, navInfo.NodeType, navInfo.Scope, e);
                    }
                }

            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTSPNavag214", key, e);
            }
            finally
            {
                if (CurrentWeb != null) CurrentWeb.Dispose();
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

        private static Hashtable GetProperties(string metainfo)
        {
            Hashtable prp = new Hashtable();
            string[] mSplitedString = metainfo.Split(new [] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
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
                    navNode.Move(navNodeCollection, navNodeCollection[rankChild]);
                }
            }
        }

        private void RestoreOneNode(AveNavigationInfo navNodeInfo, CreateNavNodeOption option, AveNavigationNodeCollection parentCollection)
        {
            AveNavigationNode navNode = RestoreNavNodeInternal(navNodeInfo, ref  parentCollection, option);

            MoveToPos(navNode, navNodeInfo.RankChild, parentCollection);

            if (navNode != null)
            {
                navNodeInfo.Children = SortNodeChildren(navNodeInfo.Children);
                foreach (AveNavigationInfo subNavNodeInfo in navNodeInfo.Children)
                {
                    option = (WrapperRuntime.CurrentContext.IsMoss && subNavNodeInfo.NodeType != -1 && AvePublishing.IsPublishingSite(mAveSite)) ? CreateNavNodeOption.WithNodeType : CreateNavNodeOption.WithoutNodeType;       
                    RestoreOneNode(subNavNodeInfo, option,navNode.Children as AveNavigationNodeCollection);
                }
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
                #region folder url can not replace as this way
                //if (propertyTable.Contains("UrlQueryString") && propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                //{
                //    url = propertyTable["UrlQueryString"].ToString();
                //}
                #endregion
                ReplaceOption replaceOption = new ReplaceOption(true, true); // opetion set to replace AbsoluteUrl and RelativeUrl
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                url = AveReplaceProcessor.UrlReplace(url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);

                navNode = GetExistingNavNode(navNodeInfo.Eid, navNodeInfo.Title, url, nodeType, navNodeInfo.IsExternal, parentCollection);

                if (navNode == null)
                {
                    if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navNodeInfo.Eid))
                    {
                        navNode = CreateDefaultQuickLaunchHeading((AveQuickLaunchHeading)navNodeInfo.Eid, navNodeInfo, propertyTable, url);

                        mAveSite.ReloadSite();
                        Guid tempGuid = CurrentWeb.ID;
                        if (CurrentWeb != null)
                        {
                            CurrentWeb.Dispose();
                        }
                        CurrentWeb = mAveSite.OpenWeb(tempGuid) as AveWeb;
                        navNode = CurrentWeb.Navigation.GetNodeById(navNodeInfo.Eid) as AveNavigationNode;
                        parentCollection = CurrentWeb.Navigation.QuickLaunch as AveNavigationNodeCollection;
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
                //mLog.Log(AveLogLevel.WARN, "WP10RTSPNavag284", navNodeInfo.Url, navNodeInfo.Title, url, e);
            }
            return navNode;
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
                        if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                        {
                            navNode.Properties["Audience"] = ReplaceAudienceId(WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                        }
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
                //mLog.Warn("An error occured while updating navigation node.ErrorMessage:{0}", ex.ToString());
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
                    if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                    {
                        navNode.Properties["Audience"] = ReplaceAudienceId(WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                    }
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

        private AveNavigationNode GetExistingNavNode(int id, string title, string url, string nodeType, bool isExternal, AveNavigationNodeCollection parentCollection)
        {
            AveNavigationNode navNode;
            if (Enum.IsDefined(typeof(AveQuickLaunchHeading), id))
            {
                navNode = CurrentWeb.Navigation.GetNodeById(id) as AveNavigationNode;
            }
            else
            {
                navNode = GetExistingSubNavNode(parentCollection, title, url, isExternal, nodeType);
            }
            return navNode;
        }

        private AveNavigationNode GetExistingSubNavNode(AveNavigationNodeCollection navNodeCollection, string title, string url, bool isExternal, string nodeType)
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

        private AveNavigationNode CreateDefaultQuickLaunchHeading(AveQuickLaunchHeading quickLaunchHeading, AveNavigationInfo navNodeInfo, Hashtable propertyTable, string url)
        {
            AveNavigationNode tmpNode = new AveNavigationNode("", "", true);
            tmpNode = CurrentWeb.Navigation.AddToQuickLaunch(tmpNode, quickLaunchHeading) as AveNavigationNode;
            tmpNode.Delete();
            AveNavigationNode headingNode = CurrentWeb.Navigation.GetNodeById((int)quickLaunchHeading) as AveNavigationNode;
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
                if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping != null)
                {
                    headingNode.Properties["Audience"] = ReplaceAudienceId(WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AudienceIDMapping, audience);
                }
            }
            if (propertyTable.ContainsKey("UrlQueryString") && !propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
            {
                //RootFolder=%2Fsites%2Fsource%2FShared%20Documents%2Ffolder1&FolderCTID=0x0120007A870534FF42704A8C299F7E4F3B65DF&View={9908FBF0-E1A6-4B77-A384-7E30833B75E0}
                headingNode.Properties["UrlQueryString"] = propertyTable["UrlQueryString"].ToString();
            }
            if (!headingNode.Url.TrimStart('/').StartsWith(mAveSite.ServerRelativeUrl.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
            {
                headingNode.Url = mAveSite.ServerRelativeUrl + "/" + navNodeInfo.Url.TrimStart('/');
            }
            headingNode.Url = url;
            headingNode.Update();
            return headingNode;
        }

        private string ReplaceAudienceId(Dictionary<string, string> audienceIdMapping, string oldValue)
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
                if (audienceIdMapping.ContainsKey(tValue))
                {
                    newValue = newValue.Replace(tValue, audienceIdMapping[tValue]);
                }
            }
            return newValue;
        }

        private void ClearAllNodes()
        {
            RealClear(CurrentWeb.Navigation.TopNavigationBar);
            RealClear(CurrentWeb.Navigation.QuickLaunch);
        }

        private static void RealClear(IAveNavigationNodeCollection navNodeCollection)
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
                        //mLog.Log(AveLogLevel.WARN, "An error occured occured while delete navigation node. error:{0}", e.ToString());
                        //mLog.Warn("An error occured occured while delete navigation node. error:{0}", e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "An error occured while clear navigation nodes. error:{0}", e.ToString());
                //mLog.Warn("An error occured while clear navigation nodes. error:{0}", e.ToString());
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