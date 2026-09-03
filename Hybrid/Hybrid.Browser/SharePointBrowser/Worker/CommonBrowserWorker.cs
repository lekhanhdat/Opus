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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.Worker
{
    public class CommonBrowserWorker
    {

        protected readonly AvePoint.GCommon.AveLogger Logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Global Variables
        /// <summary>
        /// get from manager
        /// </summary>
        protected SharePointBrowserContract mContractFromGUI = null;

        /// <summary>
        /// SharePoint API mode provider
        /// </summary>
        protected AveObjectModelFactory mObjectModel = null;

        #endregion

        #region Constructor
        public CommonBrowserWorker(SharePointBrowserContract contractFromGUI, AveObjectModelFactory objectModel)
        {
            mObjectModel = objectModel;
            mContractFromGUI = contractFromGUI;
        }
        #endregion



        #region Interface Implements
        public virtual void BrowseChildren()
        {

            IAveSecurity security = mObjectModel.CreateSecurity();
            if (security != null)
            {
                security.RunWithElevatedPrivileges(delegate ()
                {
                    mContractFromGUI = DispatchBrowseRequest();
                }, true);
            }
            else
            {
                mContractFromGUI = DispatchBrowseRequest();
            }
            OutPutChildrenNodeLog("The Common browser browse children nodes are");
        }

        #endregion

        public void OutPutParentNodeLog(string message)
        {
            if (mContractFromGUI.ParentNodes != null && mContractFromGUI.ParentNodes.Count > 0)
            {
                StringBuilder parentInfo = new StringBuilder();
                parentInfo.AppendLine(message);
                mContractFromGUI.ParentNodes.ForEach(node =>
                {
                    parentInfo.Append(node.ToString());
                });
                Logger.Debug(parentInfo.ToString());
            }
        }

        public void OutPutChildrenNodeLog(string message)
        {
            if (mContractFromGUI.ChildenNodes != null && mContractFromGUI.ChildenNodes.Count > 0)
            {
                StringBuilder childInfo = new StringBuilder();
                childInfo.AppendLine(string.Format("The children count is {0}", mContractFromGUI.ChildrenCount));
                childInfo.AppendLine(message);
                mContractFromGUI.ChildenNodes.ForEach(node =>
                {
                    childInfo.Append(node.ToString());
                });
                Logger.Debug(childInfo.ToString());
            }
        }


        #region Virtual Functions for load tree
        public virtual void GetWebApplications()
        {
            int childrenCount = 0;
            var hasError = false;
            using (WebApplicationLevel webAppLevel = new WebApplicationLevel(mObjectModel))
            {
                mContractFromGUI.ChildenNodes = webAppLevel.GetWebApplications(false, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount, ref hasError, mContractFromGUI.UserNameList);
                mContractFromGUI.ChildrenCount = childrenCount;
                if (hasError)
                {
                    mContractFromGUI.Error = new SharePointBrowserError { Type = SharePointBrowserErrorType.UnKnown };
                }
            }
        }

        public virtual void GetSites(SPTreeNodeDto webAppNode, string siteUrl)
        {
            int childrenCount = 0;
            var hasError = false;
            List<SPTreeNodeDto> siteNodes = null;
            using (SiteLevel siteLevel = new SiteLevel(mObjectModel, siteUrl))
            {
                siteNodes = siteLevel.GetSites(webAppNode.FullPath, mContractFromGUI.UserNameList, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount, null, ref hasError);
                mContractFromGUI.ChildenNodes = siteNodes;
                mContractFromGUI.ChildrenCount = childrenCount;
                if (hasError)
                {
                    mContractFromGUI.Error = new SharePointBrowserError { Type = SharePointBrowserErrorType.UnKnown };
                }
            }
        }

        public virtual void GetContentDBs(SPTreeNodeDto webAppNode)
        {
            bool hasError = false;
            using (WebApplicationLevel webAppLevel = new WebApplicationLevel(mObjectModel))
            {
                mContractFromGUI.ChildenNodes = webAppLevel.GetContentDBs(webAppNode.FullPath, ref hasError);
                if (hasError)
                {
                    mContractFromGUI.Error = new SharePointBrowserError { Type = SharePointBrowserErrorType.UnKnown, Message = "Cannot get content database information, because it is null" };
                }
            }
        }

        public virtual void GetSite(SPTreeNodeDto SiteNode, ref Guid siteId, ref string siteUrl, ref string sqlConnString, ref uint siteLockStatus)
        {
            siteUrl = SiteNode.FullPath;
            using (SiteLevel siteLevel = new SiteLevel(mObjectModel, siteUrl))
            {
                siteLockStatus = SiteNode.SiteLockStatusValue;
                sqlConnString = siteLevel.GetQueryConnectionString(SiteNode.FullPath, ref siteId);
            }
        }

        public virtual void GetRootWeb(SPTreeNodeDto siteNode, Guid siteId, string siteUrl, string sqlConnString, uint siteLockStatus)
        {
            using (WebLevel webLevel = new WebLevel(mObjectModel, sqlConnString, siteUrl))
            {
                var rootWebNode = webLevel.GetBrowserRootWeb(siteId, siteLockStatus);
                if (rootWebNode == null)
                {
                    mContractFromGUI.Error = new SharePointBrowserError { Type = SharePointBrowserErrorType.UnKnown };
                }
                else
                {
                    mContractFromGUI.ChildenNodes.Add(rootWebNode);
                    mContractFromGUI.ChildrenCount = 1;
                }
            }
        }

        public virtual void GetWebs(Guid siteId, Guid parentWebId, string siteUrl, string sqlConnString, uint siteLockStatus)
        {
            int childrenCount = 0;
            using (WebLevel webLevel = new WebLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(webLevel.GetWebs(siteId, parentWebId, siteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount));
            }
            mContractFromGUI.ChildrenCount = childrenCount;
        }

        public virtual void GetWeb(SPTreeNodeDto webNode, ref string webServerRelativeUrl, ref Guid webId)
        {
            webServerRelativeUrl = AveUrlUtility.GetServerRelativeUrl(webNode.FullPath);
            webId = new Guid(webNode.SPObjectId);
        }
        public virtual void GetFolder(SPTreeNodeDto folderNode, ref Guid parentFolderUniqueId, ref string parentFolderServerRelatedUrl)
        {
            //folder's full path is equals to server related Url
            parentFolderServerRelatedUrl = folderNode.FullPath;
            if (folderNode.SPObjectId != null)
            {
                parentFolderUniqueId = new Guid(folderNode.SPObjectId);
            }
        }

        public virtual void GetLists(Guid siteId, Guid parentWebId, string siteUrl, string sqlConnString, uint siteLockStatus)
        {
            int childrenCount = 0;
            using (ListLevel listLevel = new ListLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(listLevel.Getlists(siteId, parentWebId, siteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount));
                mContractFromGUI.ChildrenCount = childrenCount;
            }
        }
        public virtual void GetAppsDefinitions(Guid siteId, Guid parentWebId, string siteUrl, string sqlConnString)
        {
            using (AppsLevel appsLevel = new AppsLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetAppDefinitions(siteId, parentWebId, siteUrl));
                mContractFromGUI.ChildrenCount = mContractFromGUI.ChildenNodes.Count;
            }
        }

        public virtual void GetAppsInstances(SPTreeNodeDto appDefinitionNode, Guid siteId, Guid parentWebId, string siteUrl, string sqlConnString)
        {
            using (AppsLevel appsLevel = new AppsLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(appsLevel.GetAppInstances(appDefinitionNode, siteId, parentWebId, siteUrl));
                mContractFromGUI.ChildrenCount = mContractFromGUI.ChildenNodes.Count;
            }
        }

        public virtual void GetRootFolder(SPTreeNodeDto listNode, Guid siteId, Guid parentWebId, string siteUrl, string sqlConnString, uint siteLockStatus)
        {
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.Add(folderLevel.GetRootFolder(siteId, parentWebId, new Guid(listNode.SPObjectId), siteLockStatus));
                mContractFromGUI.ChildrenCount = 1;
            }
        }

        public virtual void GetSubFolders(SPTreeNodeDto folderNode, Guid siteId, Guid parentWebId, Guid parentListId, Guid parentFolderUniqueId, string siteUrl, string parentFolderServerRelatedUrl, string sqlConnString, uint siteLockStatus)
        {
            int childrenCount = 0;
            string pageInfo = mContractFromGUI.PageInfo;
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, sqlConnString, siteUrl))
            {
                mContractFromGUI.ChildenNodes.AddRange(folderLevel.GetSubFolders(siteId, parentWebId, parentListId, parentFolderUniqueId, parentFolderServerRelatedUrl, siteLockStatus, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount, ref pageInfo, siteUrl));
                mContractFromGUI.ChildrenCount = childrenCount;
                mContractFromGUI.PageInfo = pageInfo;
            }
        }

        public virtual void GetItems(SPTreeNodeDto itemsNode, Guid siteId, Guid parentWebId, Guid parentFolderUniqueId, string siteUrl, string parentFolderServerRelatedUrl, string sqlConnString, uint siteLockStatus)
        {
            string pageInfo = mContractFromGUI.PageInfo;
            bool isForceNativeModel = AveBrowserHelper.IsForceNativeModel(pageInfo);
            using (FolderLevel folderLevel = new FolderLevel(mObjectModel, sqlConnString, siteUrl, isForceNativeModel))
            {
                var items = folderLevel.GetItems(siteId, parentWebId, parentFolderUniqueId, parentFolderServerRelatedUrl, ref pageInfo, mContractFromGUI.PerPage, siteLockStatus);
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

        public virtual void GetItemVersions(SPTreeNodeDto itemNode, Guid siteId, Guid parentFolderUniqueId, string webServerRelativeUrl, string listTitle, string siteUrl, string parentFolderServerRelativeUrl, string sqlConnString, uint siteLockStatus)
        {
            int childrenCount = 0;
            string pageInfo = mContractFromGUI.PageInfo;
            using (ItemLevel itemLevel = new ItemLevel(mObjectModel, sqlConnString, siteUrl))
            {
                List<SPTreeNodeDto> itemVersions = itemLevel.GetItemVersions(siteId, webServerRelativeUrl, listTitle, parentFolderServerRelativeUrl, parentFolderUniqueId, itemNode, mContractFromGUI.StartIndex, mContractFromGUI.PerPage, ref childrenCount, siteLockStatus);
                mContractFromGUI.ChildenNodes.AddRange(itemVersions);
                mContractFromGUI.ChildrenCount = childrenCount;
            }
        }

        public virtual void GetList(SPTreeNodeDto listNode, ref string listTitle, ref Guid listId)
        {
            listTitle = listNode.Name;
            if (!string.IsNullOrEmpty(listNode.SPObjectId))
            {
                listId = new Guid(listNode.SPObjectId);
            }

        }

        public virtual void DisposeSPObject()
        {

        }
        #endregion

        internal virtual SharePointBrowserContract DispatchBrowseRequest()
        {
            string sqlConnString = string.Empty;
            Guid siteId = Guid.Empty;
            string siteUrl = string.Empty;
            uint siteLockStatus = 0;
            //client 端GetItemVersions方法会用到
            string webServerRelativeUrl = string.Empty;

            Guid webId = Guid.Empty;
            //client 端GetItemVersions方法会用到
            string listTitle = string.Empty;

            Guid listId = Guid.Empty;
            Guid parentFolderUniqueId = Guid.Empty;
            string parentFolderServerRelatedUrl = string.Empty;
            SharePointBrowserError errorObj = null;
            try
            {
                if (mContractFromGUI.ParentNodes == null || mContractFromGUI.ParentNodes.Count == 0) // get web applications
                {
                    GetWebApplications();
                }
                else
                {
                    for (int i = 0; i < mContractFromGUI.ParentNodes.Count; i++)
                    {
                        SPTreeNodeDto node = mContractFromGUI.ParentNodes[i];
                        switch (node.Level)
                        {
                            case NodeLevel.WebApplication:
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetSites(node, siteUrl);
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
                                    GetSites(node.Parent, siteUrl);
                                }
                                break;
                            case NodeLevel.SiteCollection:// parentNodes do not include web application node
                                GetSite(node, ref siteId, ref siteUrl, ref sqlConnString, ref siteLockStatus);
                                if (mContractFromGUI.ParentNodes.Count == 1)
                                {
                                    GetRootWeb(node, siteId, siteUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.Lists: // only when this node is the last node, we need to get sub nodes, else ,skip
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetLists(siteId, webId, siteUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.Sites: // only when this node is the last node, we need to get sub nodes, else ,skip
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetWebs(siteId, webId, siteUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.Apps:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetAppsDefinitions(siteId, webId, siteUrl, sqlConnString);
                                }
                                break;
                            case NodeLevel.App:
                                GetAppsInstances(node, siteId, webId, siteUrl, sqlConnString);
                                break;
                            case NodeLevel.Site:
                                GetWeb(node, ref webServerRelativeUrl, ref webId);
                                break;
                            case NodeLevel.List:
                                GetList(node, ref listTitle, ref listId);
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetRootFolder(node, siteId, webId, siteUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.RootFolder:
                            case NodeLevel.Folder:
                                GetFolder(node, ref parentFolderUniqueId, ref parentFolderServerRelatedUrl);
                                break;
                            case NodeLevel.Folders:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetSubFolders(node, siteId, webId, listId, parentFolderUniqueId, siteUrl, parentFolderServerRelatedUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.Items:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetItems(node, siteId, webId, parentFolderUniqueId, siteUrl, parentFolderServerRelatedUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                            case NodeLevel.Item:
                                if (i + 1 == mContractFromGUI.ParentNodes.Count)
                                {
                                    GetItemVersions(node, siteId, parentFolderUniqueId, webServerRelativeUrl, listTitle, siteUrl, parentFolderServerRelatedUrl, sqlConnString, siteLockStatus);
                                }
                                break;
                        }
                    }
                }
            }
            catch (AveRPCException rpcEx)
            {
                errorObj = HandleSpecialExpcetion(rpcEx);
                if (errorObj == null)
                {
                    errorObj = new SharePointBrowserError()
                    {
                        Type = SharePointBrowserErrorType.UnKnown,
                        Message = rpcEx.Message,
                    };
                    Logger.Warn("Unknown error: {0}", rpcEx);
                }
            }
            catch (Exception ex)
            {
                errorObj = new SharePointBrowserError()
                {
                    Type = SharePointBrowserErrorType.UnKnown,
                    Message = ex.Message,
                };
                Logger.Warn("Unknown error: {0}", ex);
            }
            finally
            {
                if (errorObj != null)
                {
                    mContractFromGUI.Error = errorObj;
                }
                DisposeSPObject();
            }
            return mContractFromGUI;
        }

        private SharePointBrowserError HandleSpecialExpcetion(AveRPCException rpcEx)
        {
            #region AveQueryThrottledException
            var qtEx = rpcEx.InnerException as AveQueryThrottledException;
            if (qtEx != null)
            {
                Logger.Error("Query throttled for large list: {0}", qtEx);
                return new SharePointBrowserError()
                {
                    Type = SharePointBrowserErrorType.UnKnown,//change to SharePointBrowserErrorType.QueryThrottled for detail error msg
                    Message = qtEx.Message,
                };
            }
            #endregion

            #region Add more exception handling here
            #endregion
            return null;
        }

        public void GetPermissions(List<SPTreeNodeDto> nodes)
        {
            foreach (SPTreeNodeDto node in nodes)
            {
                if (node.Level > NodeLevel.SiteCollection || mContractFromGUI.UserNameList == null || mContractFromGUI.UserNameList.Count == 0)
                {
                    return;
                }
                if (node.NodeExtension.PermissionList != null && node.NodeExtension.PermissionList.Count > 0)
                {
                    if (mContractFromGUI.PermissionList == null)
                    {
                        mContractFromGUI.PermissionList = new List<SPTreePermissionMappingDto>();
                    }
                    mContractFromGUI.PermissionList.AddRange(node.NodeExtension.PermissionList);
                }
                GetPermissions(node.Children);
            }
        }

    }
}
