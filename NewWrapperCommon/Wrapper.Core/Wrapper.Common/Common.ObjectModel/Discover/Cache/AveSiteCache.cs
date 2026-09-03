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
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveSiteCache : AveDiscoverCache, IDisposable
    {
        private AveObjectModelFactory mObjectModelFactory;

        private IAveSite mAveSite;//Just For Filter Policy

        private ChangeType changeType;
        private ChangeType userChangeType;
        private ChangeType groupChangeType;
        
        public IAveSite AveSite
        {
            get
            {
                return mAveSite;
            }
        }

        /// <summary>
        /// 内部使用
        /// </summary>
        public AveObjectModelFactory ObjectModelFactory
        {
            get { return mObjectModelFactory; }
        }
        
        private bool mHasChangeCache = false;
        public ChangeType ChangeType { get { return changeType; } set { changeType = value; } }
        /// <summary>
        /// 表示Site Collection User是否有改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType UserChangeType { get { return userChangeType; } set { userChangeType = value; } }
        /// <summary>
        /// 表示Site Collection Group是否有改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType GroupChangeType { get { return groupChangeType; } set { groupChangeType = value; } }

        public AveSiteCache(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module)
        {
            var unUsedDateTime = new DateTime();
            var objectModelFactory = GetObjectFactory(site, account, kind);
            InitParameters(site, kind, objectModelFactory, module, unUsedDateTime, unUsedDateTime);
        }

        public AveSiteCache(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
            var objectModelFactory = GetObjectFactory(site, account, kind);
            InitParameters(site, kind, objectModelFactory, module, startTime, endTime);
        }


        public AveSiteCache(IAveSite site, AveObjectModelFactory objectModelFactory, AveDiscoveryKind kind, DiscoverModule module)
        {
            var unUsedDateTime = new DateTime();
            InitParameters(site, kind, objectModelFactory, module, unUsedDateTime, unUsedDateTime);
        }

        public AveSiteCache(IAveSite site, AveObjectModelFactory objectModelFactory, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
            InitParameters(site, kind, objectModelFactory, module, startTime, endTime);
        }
        
        private AveObjectModelFactory GetObjectFactory(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind)
        {
            var contextKind = AveContextKind.Auto;
            if (kind == AveDiscoveryKind.API)
            {
                contextKind = AveContextKind.ClientObjectModel;
            }
            return AveObjectModelFactory.CreateObjectModelFactory(site.Url, account, contextKind);
        }

        private void InitParameters(IAveSite site, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
            CheckParametersNull(site, "site");
            CheckParametersNull(objectModelFactory, "objectModelFactory");
            this.mObjectModelFactory = objectModelFactory;
            this.mAveSite = site;
            mHasChangeCache = false;
            switch (kind)
            {
                case AveDiscoveryKind.ServerAPI:
                    Query = new AveDiscoverQueryForAPI(site, mObjectModelFactory, startTime, endTime, module);
                    break;
                case AveDiscoveryKind.API:
                case AveDiscoveryKind.Database:
                    Query = mObjectModelFactory.CreateDiscoveryQuery(site, startTime, endTime, module);
                    break;
                default:
                    break;
            }
        }

        private void CheckParametersNull(Object obj, string argumentName)
        {
            if (null == obj)
            {
                throw new AveArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// For Unit Test to create SiteCache module
        /// </summary>
        public AveSiteCache()
        {
        }

        #region FB

        public Dictionary<Guid, AveWebObject> GetWebs()
        {
            return Query.QuerySiteWebForFB(mAveSite.ID);
        }

        public AveWebObject GetRootWeb()
        {
            return Query.QueryRootWeb(mAveSite.ID);
        }
        public long GetSiteSize()
        {
            int mItemCount = 0;
            long mRecycleSize = 0L;
            mAveSite.GetRecycleBinStatistics(out mItemCount, out mRecycleSize);
            long size = mAveSite.Size - mRecycleSize;
            return size;
        }
        #endregion

        #region IB

        public Dictionary<int, AveSiteMemberObject> GetChangeMembers()
        {
            return Query.QuerySiteSecurityForIB(mAveSite.ID);
        }

        public Dictionary<Guid, AveWebObject> GetChangeWebs()
        {
            return Query.QueryWebForIB(mAveSite.ID);
        }

        #endregion

        #region Support Replicator

        public AveItemObject GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, int? maxMajorwithMinorVersionCount)
        {
            return Query.GetItemExist(mAveSite.ID, webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, maxMajorwithMinorVersionCount);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {
            return Query.GetItemLastModifiedTime(siteId, listId, rowId);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            return Query.GetItemLastModifiedTime(siteId, itemId);
        }

        public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            return Query.GetItemLastModifiedTime(mAveSite.ID, webId, listId, dirName, leafName, ref docId);
        }

        public AveItemObject GetItemVersions(Guid webId, Guid listId, int docLibRowId)
        {
            return Query.GetItemVersions(mAveSite.ID, webId, listId, docLibRowId);
        }

        public void GetSiteChanged()
        {
            if (!this.mHasChangeCache)
            {
                Query.GetSiteChangedForIB(mAveSite.ID, ref changeType, ref userChangeType, ref groupChangeType);
                //this.ChangeType = (ChangeType)Query.GetSiteChangedForIB(SiteId);
                this.mHasChangeCache = true;
            }
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return Query.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
        }

        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl)
        {
            return Query.GetTPGUIDAndDocIdMapping(siteId, webId, listId, parentId, folderUrl);
        }

        public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            return Query.IsHaveSameName(mAveSite.ID, webId, listId, dirName, leafName);
        }

        public bool IsListItemHaveSameName(Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            return Query.IsListItemHaveSameName(mAveSite.ID, webId, tpGuid, listId, rowId);
        }

        public List<AveWebPartObject> GetItemWebParts(Guid webId, Guid listId, Guid itemDocId)
        {
            return Query.GetItemWebParts(mAveSite.ID, webId, listId, itemDocId);
        }

        public long GetItemSizeAndUserInfo(Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            return Query.GetItemSizeAndUserInfo(mAveSite.ID, webId, listId, docId, level, ref createdBy, ref modifiedBy);
        }

        #endregion

        public void Dispose()
        {
            if (Query != null)
            {
                Query.Dispose();
                Query = null;
            }
        }
    }
}
