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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class RelativeDataArchiveObject
    {
        //private  public get
        public Guid mWebappId = Guid.Empty;
        public string mWebappUrl = string.Empty;
        public Guid mSiteId = Guid.Empty;
        public string mSiteUrl = string.Empty;
        public Guid mWebId = Guid.Empty;
        public Guid mListId = Guid.Empty;
        public Guid mFolderId = Guid.Empty;
        public Guid mItemId = Guid.Empty;
        public bool mIsRootFolder = false;
        private string mTreeNodeString = string.Empty;
        public string mCurrentlevel = string.Empty;
        private ArchiveApproveReport mApprove;
        public SORelativeDataArchiveBackupRequest mSORelativeDataRequest;



        public RelativeDataArchiveObject(string treeNode)
        {
            mTreeNodeString = treeNode;
            mSORelativeDataRequest = ArchiverCommonStaticMethod.DeSerializer(mTreeNodeString, typeof(SORelativeDataArchiveBackupRequest)) as SORelativeDataArchiveBackupRequest;
            mCurrentlevel = mSORelativeDataRequest.CurrentLevel;
        }

        public List<TagInfoCollection> TagValue()
        {
            List<TagInfoCollection> tagInfo = new List<TagInfoCollection>();
            tagInfo = mSORelativeDataRequest.TagInfo;
            return tagInfo;
        }

        public void InitDiscoverObject()
        {
            int libRowId;
            string leafName = string.Empty;
            string level = string.Empty;
            string path = mSORelativeDataRequest.Path;
            //begin init Object that Discover Need
            mWebappUrl = mSORelativeDataRequest.WebAppUrl;
            mWebappId = string.IsNullOrEmpty(mSORelativeDataRequest.WebAppId) ? Guid.Empty : new Guid(mSORelativeDataRequest.WebAppId);
            mSiteUrl = mSORelativeDataRequest.SiteCollectionUrl;
            mSiteId = new Guid(mSORelativeDataRequest.SiteCollectionId);
            level = mSORelativeDataRequest.CurrentLevel;
            libRowId = mSORelativeDataRequest.DocLibRowId;
            leafName = mSORelativeDataRequest.LeafName;
            mIsRootFolder = mSORelativeDataRequest.ParentFolderIsRootFolder;
            if (level.Equals(SORelativeDataArchiverNodeLevel.Site.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.SiteCollection, CacheNodeType = 1, FullPath = path, NodeId = mSiteId.ToString(), SPNodeLevel = (int)NodeLevel.SiteCollection, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
            mWebId = new Guid(mSORelativeDataRequest.WebId);
            if (level.Equals(SORelativeDataArchiverNodeLevel.Web.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.Web, CacheNodeType = 500, FullPath = path, NodeId = mWebId.ToString(), SPNodeLevel = (int)NodeLevel.Site, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
            mListId = new Guid(mSORelativeDataRequest.ListId);
            if (level.Equals(SORelativeDataArchiverNodeLevel.List.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.List, CacheNodeType = 1000, FullPath = path, NodeId = mListId.ToString(), SPNodeLevel = (int)NodeLevel.List, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
            mFolderId = new Guid(mSORelativeDataRequest.FolderId);
            if (level.Equals(SORelativeDataArchiverNodeLevel.Folder.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.Folder, CacheNodeType = 5000, FullPath = path, NodeId = mFolderId.ToString(), SPNodeLevel = (int)NodeLevel.Folder, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
            if (level.Equals(SORelativeDataArchiverNodeLevel.Multifiles.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (mIsRootFolder)
                {
                    mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.Folder, CacheNodeType = 5000, FullPath = path, NodeId = mFolderId.ToString(), SPNodeLevel = (int)NodeLevel.RootFolder, DoDelete = true, LibRowId = libRowId, LeafName = leafName, ItemIDs = mSORelativeDataRequest.IncludeIds };
                }
                else
                {
                    mApprove = new ArchiveApproveReport() { ArchiveLevel = (int)SPNodeLevel.Folder, CacheNodeType = 5000, FullPath = path, NodeId = mFolderId.ToString(), SPNodeLevel = (int)NodeLevel.Folder, DoDelete = true, LibRowId = libRowId, LeafName = leafName, ItemIDs = mSORelativeDataRequest.IncludeIds };
                }
                return;
            }
            mItemId = new Guid(mSORelativeDataRequest.ItemId);
            if (level.Equals(SORelativeDataArchiverNodeLevel.Item.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { NodeType = (int)ItemType.ITEM_TYPE, ArchiveLevel = (int)SPNodeLevel.Item, CacheNodeType = 10000, FullPath = path, NodeId = mItemId.ToString(), SPNodeLevel = (int)NodeLevel.Item, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
            if (level.Equals(SORelativeDataArchiverNodeLevel.Document.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                mApprove = new ArchiveApproveReport() { NodeType = (int)ItemType.DOCUMENT, ArchiveLevel = (int)SPNodeLevel.Document, CacheNodeType = 10000, FullPath = path, NodeId = mItemId.ToString(), SPNodeLevel = (int)NodeLevel.Item, DoDelete = true, LibRowId = libRowId, LeafName = leafName };
                return;
            }
        }

        private string GetArchiveInfo()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(mTreeNodeString);
            XmlElement xmlElement = (XmlElement)xmlDocument.GetElementsByTagName("ArchiveBy")[0];
            return xmlElement.GetAttribute("ArchiveBy");
        }

        public ArchiveApproveReport Approve
        {
            get { return mApprove; }
        }

        public string ArchiveInfo
        {
            get { return GetArchiveInfo(); }
        }
    }
}
