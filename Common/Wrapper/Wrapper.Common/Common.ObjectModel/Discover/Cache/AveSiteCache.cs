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
        private string mSiteUrl;//Just for create site

        public string SiteUrl
        {
            get { return mSiteUrl; }
            set 
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("site url");
                }
                mSiteUrl = value;
            }
        }

		public IAveSite AveSite
		{
			get
			{
				if (mAveSite == null)
				{
                    if (!string.IsNullOrEmpty(mSiteUrl))
                    {
                        mAveSite = mObjectModelFactory.CreateSite(mSiteUrl);
                    }
                    else if (!Guid.Empty.Equals(this.SiteId))
                    {
                        mAveSite = mObjectModelFactory.CreateSite(this.SiteId);
                    }
				}
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

		public Guid SiteId { get; set; }

		private bool mHasChangeCache = false;
		public ChangeType ChangeType { get; set; }

        private AveSiteCache(IAveSite site, AveObjectModelFactory objectModelFactory)
        {
            mObjectModelFactory = objectModelFactory;
            mHasChangeCache = false;
            InitSiteCacheInfo(site);
        }

        private void InitSiteCacheInfo(IAveSite site)
        {
            SiteId = site.ID;
            mSiteUrl = site.Url;
            this.mAveSite = site;
        }

        public AveSiteCache(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind)
            : this(site, AveObjectModelFactory.CreateObjectModelFactory(site.Url, account, kind))
		{
		}		

		public AveSiteCache(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module)
            : this(site, account, kind)
		{
            Query = mObjectModelFactory.CreateDiscoveryQuery(site, module);
		}

		public AveSiteCache(IAveSite site, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
            : this(site, account, kind)
		{
            Query = mObjectModelFactory.CreateDiscoveryQuery(site, startTime, endTime, module);		        
		}

        public AveSiteCache(IAveSite site, AveObjectModelFactory objectModelFactory, DiscoverModule module, DateTime startTime, DateTime endTime)
            : this(site, objectModelFactory)
        {
            Query = mObjectModelFactory.CreateDiscoveryQuery(site, startTime, endTime, module);	            
        }

        public AveSiteCache(IAveSite site, AveObjectModelFactory objectModelFactory, DiscoverModule module)
            : this(site, objectModelFactory)
        {
            Query = mObjectModelFactory.CreateDiscoveryQuery(site, module);
        }
        /// <summary>
        /// For Unit Test to create SiteCache module
        /// </summary>
        public AveSiteCache()
		{
		}

		#region FB

        public Dictionary<Guid, AveWebObject> GetWebs(bool includeAppWeb = false)
		{
			return Query.QuerySiteWebForFB(SiteId, includeAppWeb);
		}

        public AveWebObject GetWeb(Guid webId)
        {
            return Query.QueryWeb(webId);
        }

		public AveWebObject GetRootWeb()
		{
			return Query.QueryRootWeb(SiteId);
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
			return  Query.QuerySiteSecurityForIB(SiteId);
		}

		public Dictionary<Guid, AveWebObject> GetChangeWebs()
		{
			return Query.QueryWebForIB(SiteId);
		}

		#endregion

		#region Support Replicator

		public AveItemObject GetItemExist(Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
		{
			return Query.GetItemExist(SiteId, webId, listId, id, dirName, leafName, isListItem);
		}

        public AveItemObject GetItemExistForListener(string webServerRelativeUrl, System.Globalization.CultureInfo culture, Guid listId, Guid tpGuid, string dirName, string leafName, bool isListItem)
        {
            return Query.GetItemExistForListener(webServerRelativeUrl, culture, listId, tpGuid, dirName, leafName, isListItem);
        }

		public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
		{
			return Query.GetItemLastModifiedTime(SiteId, webId, listId, id, hasDocLibRowId);
		}

		public DateTime GetItemLastModifiedTime(Guid listId, int rowId)
		{
			return Query.GetItemLastModifiedTime(listId, rowId);
		}

		public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
		{
			return Query.GetItemLastModifiedTime(siteId, itemId);
		}

		public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
		{
			return Query.GetItemLastModifiedTime(SiteId, webId, listId, dirName, leafName, ref docId);
		}

		public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
		{
			return Query.GetItemLastModifiedTime(SiteId, webId, listId, tp_Guid, ref docId);
		}

		public AveItemObject GetItemVersions(Guid webId, Guid listId, int docLibRowId)
		{
			return Query.GetItemVersions(SiteId, webId, listId, docLibRowId);
		}

		public void GetSiteChanged()
		{
			if (!this.mHasChangeCache)
			{                
				this.ChangeType = (ChangeType)Query.GetSiteChangedForIB(SiteId);
				this.mHasChangeCache = true;
			}
		}

		public Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
		{
			return Query.GetListItemGuid(webId, listId, tp_Guid, rowId);
		}

		public Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid)
		{
			return Query.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
		}

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid,int rowId)
        {
            return Query.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
        }

        public bool GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl, ref Dictionary<Guid, Guid> itemsMapping, ref Dictionary<Guid, Guid> foldersMapping)
		{
			return Query.GetTPGUIDAndDocIdMapping(siteId,webId,listId,parentId,folderUrl, ref itemsMapping, ref foldersMapping);
		}

		public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
		{
			return Query.IsHaveSameName(SiteId, webId, listId, dirName, leafName);
		}

		public bool IsListItemHaveSameName(Guid webId, Guid tpGuid, Guid listId, int rowId)
		{
			return Query.IsListItemHaveSameName(SiteId, webId, tpGuid, listId, rowId);
		}

		public string GetListContentTypes(Guid webId, Guid listId)
		{
			return Query.GetListContentTypes(webId, listId);
		}

		public List<AveWebPartObject> GetItemWebParts(Guid webId,Guid listId, Guid itemDocId)
		{
		   return Query.GetItemWebParts(SiteId, webId, listId, itemDocId);
		}

		public int GetItemSize(Guid webId, Guid listId, Guid docId, ref string createdBy, ref string modifiedBy)
		{
			return Query.GetItemSize(SiteId, webId, listId, docId, ref createdBy, ref modifiedBy);
		}

		#endregion

		public void Dispose()
		{
			if (Query != null)
			{
				Query.Dispose();
				Query = null;
			}
			if (mAveSite != null)
			{
				mAveSite.Dispose();
				mAveSite = null;
			}
		}
	}
}
