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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Browser.IndividualLevel;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Worker.Impl
{
    public class CommonBrowserWorker : ISPBrowserWorker
    {

        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected SharePointBrowserContract mContractFromGUI = null;
        protected BrowserType mBrowserType = BrowserType.SharePointOnline;

        protected AveObjectModelFactory mObjectModel = null;

        protected IAveSite mSite = null;

        protected Guid mParentWebId = Guid.Empty;

        protected string parentFolderServerRelatedUrl = string.Empty;

        protected string mSiteUrl = string.Empty;

        Guid mSiteId = Guid.Empty;

        Guid mParentFolderUniqueId = Guid.Empty;

        Guid mCurrentListId = Guid.Empty;

        protected string mSqlConnString;

        protected int mSiteLockStatus;

        public CommonBrowserWorker(SharePointBrowserContract contractFromGUI, AveObjectModelFactory objectModel, BrowserType browserType)
        {
            mObjectModel = objectModel;
            mContractFromGUI = contractFromGUI;
            mBrowserType = browserType;
        }

        public void BrowseChildren()
        {
            var security = mObjectModel.CreateSecurity();
            if (security != null)
            {
                security.RunWithElevatedPrivileges(delegate ()
                {
                    mContractFromGUI = DispatchBrowseRequest();
                });
            }
            else
            {
                mContractFromGUI = DispatchBrowseRequest();
            }
            //mContractFromGUI = DispatchBrowseRequest();
        }

        public void DisposeSPObject()
        {
            if(mSite != null)
            {
                mSite.Dispose();
            }
        }

        internal virtual SharePointBrowserContract DispatchBrowseRequest()
        {
            try
            {
                if (mContractFromGUI.ParentNodes == null || mContractFromGUI.ParentNodes.Count == 0)
                {
                    GetWebApplications();
                }
                else
                {
                    for (var i = 0; i < mContractFromGUI.ParentNodes.Count; i++)
                    {
                        var node = mContractFromGUI.ParentNodes[i];
                        switch (node.Level)
                        {
                            case NodeLevel.WebApplication:
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetSites(node);
                                }
                                break;
                            case NodeLevel.ContentDBs:
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetContentDBs(node.Parent);
                                }
                                break;
                            case NodeLevel.SiteCollections:
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetSites(node.Parent);
                                }
                                break;
                            case NodeLevel.SiteCollection:// parentNodes do not include web application node
                                GetSite(node);
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetRootWeb(node);
                                }
                                break;
                            case NodeLevel.Lists: // only when this node is the last node, we need to get sub nodes, else ,skip
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetLists();
                                }
                                break;
                            case NodeLevel.Sites: // only when this node is the last node, we need to get sub nodes, else ,skip
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetWebs();
                                }
                                break;
                            case NodeLevel.Site:
                                GetWeb(node);
                                break;
                            case NodeLevel.Apps:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetAppsDefinitions();
                                }
                                break;
                            case NodeLevel.App:
                                GetAppsInstances(node);
                                break;

                            case NodeLevel.ProjectOnlines:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetProjects();
                                }
                                break;

                            //case NodeLevel.ProjectOnline:
                            //    break;

                            case NodeLevel.List:
                                GetList(node);
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetRootFolder(node);
                                }
                                break;
                            case NodeLevel.RootFolder:
                            case NodeLevel.Folder:
                                GetFolder(node);
                                break;
                            case NodeLevel.Folders:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetSubFolders(node);
                                }
                                break;
                            case NodeLevel.Items:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetItems(node);
                                }
                                break;
                            case NodeLevel.Item:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetItemVersions(node);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while dispatch browse request. Error: {e}");
            }
            finally
            {
                DisposeSPObject();
            }
            return mContractFromGUI;
        }

        public virtual void GetWebApplications()
        {
            int childrenCount = 0;
            WebApplicationLevel webAppLevel = new WebApplicationLevel(mObjectModel);
            mContractFromGUI.ChildenNodes = webAppLevel.GetWebApplications(false, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount);
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetSites(SPTreeNodeDto webAppNode)
        {
            int childrenCount = 0;
            SiteLevel siteLevel = new SiteLevel(mObjectModel, mSiteUrl);
            mContractFromGUI.ChildenNodes = siteLevel.GetSites(webAppNode.FullPath, webAppNode.LoginName, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount);
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetContentDBs(SPTreeNodeDto webAppNode)
        {
            WebApplicationLevel webAppLevel = new WebApplicationLevel(mObjectModel);
            mContractFromGUI.ChildenNodes = webAppLevel.GetContentDBs(webAppNode.FullPath);
        }

        public virtual void GetSite(SPTreeNodeDto SiteNode)
        {
            mSiteUrl = SiteNode.FullPath;
            SiteLevel siteLevel = new SiteLevel(mObjectModel, mSiteUrl);
            mSiteLockStatus = SiteNode.SiteLockStatus;
            mSqlConnString = siteLevel.GetQueryConnectionString(SiteNode.FullPath, ref mSiteId);
        }

        public virtual void GetRootWeb(SPTreeNodeDto siteNode)
        {
            using (WebLevel webLevel = new WebLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                mContractFromGUI.ChildenNodes.Add(webLevel.GetBrowserRootWeb(mSiteId, mSiteLockStatus));
                mContractFromGUI.ChildrenCount = 1;
            }
        }

        public virtual void GetWebs()
        {
            int childrenCount = 0;
            using (WebLevel webLevel = new WebLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(webLevel.GetWebs(mParentWebId, mSiteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount));
            }
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetWeb(SPTreeNodeDto webNode)
        {
            mParentWebId = new Guid(webNode.SPObjectId);
        }
        public virtual void GetFolder(SPTreeNodeDto folderNode)
        {
            parentFolderServerRelatedUrl = folderNode.FullPath;
            if (folderNode.SPObjectId != null)
            {
                mParentFolderUniqueId = new Guid(folderNode.SPObjectId);
            }
        }

        public virtual void GetLists()
        {
            int childrenCount = 0;
            using (var listLevel = mBrowserType == BrowserType.OneDrive ? new OneDriveListLevel(mObjectModel, mSqlConnString, mSiteUrl) : new ListLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(listLevel.Getlists(mParentWebId, mSiteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount));

            }
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetProjects()
        {
            int childrenCount = 0;
            using (var projectLevel = new ProjectLevel(mObjectModel, mSiteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(projectLevel.GetProjects(mSiteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount));
            }
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetList(SPTreeNodeDto listNode)
        {
            if (!string.IsNullOrEmpty(listNode.SPObjectId))
            {
                mCurrentListId = new Guid(listNode.SPObjectId);
            }
        }

        public virtual void GetRootFolder(SPTreeNodeDto listNode)
        {
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(folderLevel.GetRootFolder(mParentWebId, mCurrentListId, mSiteLockStatus));
                mContractFromGUI.ChildrenCount = 1;
            }
        }

        public virtual void GetSubFolders(SPTreeNodeDto foldesrNode)
        {
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                var folders = folderLevel.GetSubFolders(mParentWebId, mCurrentListId, mParentFolderUniqueId, parentFolderServerRelatedUrl, mSiteLockStatus);
                mContractFromGUI.ChildenNodes.AddRange(folders);
                mContractFromGUI.ChildrenCount = folders.Count;
            }
        }

        public virtual void GetItems(SPTreeNodeDto itemsNode)
        {
            string pageInfo = mContractFromGUI.PageInfo;
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                var items = folderLevel.GetItems(mParentWebId, mParentFolderUniqueId, parentFolderServerRelatedUrl, ref pageInfo, mContractFromGUI.PerPage, mSiteLockStatus);
                mContractFromGUI.ChildenNodes.AddRange(items);
                mContractFromGUI.ChildrenCount = items.Count;
            }
            mContractFromGUI.PageInfo = pageInfo;
            if (string.IsNullOrEmpty(pageInfo))
            {
                mContractFromGUI.HasNextPage = false;
            }
            else
            {
                mContractFromGUI.HasNextPage = true;
            }
        }

        public virtual void GetItemVersions(SPTreeNodeDto itemNode)
        {
            ItemLevel itemLevel = new ItemLevel(mObjectModel, mSiteUrl);
            mSite = itemLevel.GetSite(mSiteUrl);
            using (IAveWeb parentWeb = mSite.OpenWeb(mParentWebId))
            {
                IAveFolder folder = parentWeb.GetFolder(parentFolderServerRelatedUrl);
                IAveListItem item = folder.ParentList.GetItemByUniqueId(new Guid(itemNode.SPObjectId));
                string pageInfo = mContractFromGUI.PageInfo;
                mContractFromGUI.ChildenNodes.AddRange(itemLevel.GetItemVersions(item, ref pageInfo, mContractFromGUI.PerPage, mSiteLockStatus));
                mContractFromGUI.PageInfo = pageInfo;
                if (string.IsNullOrEmpty(pageInfo))
                {
                    mContractFromGUI.HasNextPage = false;
                }
                else
                {
                    mContractFromGUI.HasNextPage = true;
                }
            }
        }

        public virtual void GetAppsDefinitions()
        {
            using (AppsLevel appsLevel = new AppsLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                //mSite = appsLevel.GetSite(mSiteUrl);
                //IAveWeb parentWeb = mSite.OpenWeb(mParentWebId);
                //mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetAppDefinitions(parentWeb));
                mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetBrowserAppDefinitions(mParentWebId));
                mContractFromGUI.ChildrenCount = mContractFromGUI.ChildenNodes.Count;
            }
        }

        public virtual void GetAppsInstances(SPTreeNodeDto appDefinitionNode)
        {
            using (AppsLevel appsLevel = new AppsLevel(mObjectModel, mSqlConnString, mSiteUrl))
            {
                //mSite = appsLevel.GetSite(mSiteUrl);
                //IAveWeb parentWeb = mSite.OpenWeb(mParentWebId);
                //mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetAppInstances(parentWeb, appDefinitionNode));

                mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetBrowserAppInstances(mParentWebId, appDefinitionNode));
                mContractFromGUI.ChildrenCount = mContractFromGUI.ChildenNodes.Count;
            }
        }
    }
}
