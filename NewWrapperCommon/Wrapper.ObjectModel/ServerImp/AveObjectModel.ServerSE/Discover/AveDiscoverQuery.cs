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
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.QueryService;

namespace AvePoint.ObjectModel.ServerSE
{
    public class AveDiscoverQuery : IAveDiscoveryQuery
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveDiscoverQuery));

        private AveDiscoverReader mDiscoverReader;
        private const int DocList = 1;
        internal IAveDiscoverQueryService mQueryService;
        private readonly DateTime mStartTime;
        private readonly DateTime mEndTime;
        private readonly Dictionary<string, AveItemObject> mNoPropertyFolders = new Dictionary<string, AveItemObject>();

        [Obsolete("Use it with date time")]
        public AveDiscoverQuery(IAveSite site, DiscoverModule module)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.AveDiscoverQuery"))
            {

                mQueryService = AveQueryServiceProvider.Instance<IAveDiscoverQueryService>(site);
                SupportIB = false;
                mDiscoverReader = AveDiscoverReaderFactory.GetAveDiscoverReader(module);

            }

        }

        public AveDiscoverQuery(IAveSite site, DateTime startTime, DateTime endTime, DiscoverModule module)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.AveDiscoverQuery_1"))
            {

                mQueryService = AveQueryServiceProvider.Instance<IAveDiscoverQueryService>(site);
                SupportIB = false;
                mStartTime = startTime;
                mEndTime = endTime;
                mDiscoverReader = AveDiscoverReaderFactory.GetAveDiscoverReader(module);

            }

        }

        public AveDiscoverQuery(string siteUrl, object conn, AveBPOSAccountInfo account, DateTime startTime, DateTime endTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.AveDiscoverQuery_2"))
            {

                mQueryService = AveQueryServiceProvider.Instance<IAveDiscoverQueryService>(conn);
                mStartTime = startTime;
                mEndTime = endTime;
                SupportIB = true;

            }

        }

        public AveDiscoverQuery(string siteUrl, object conn, AveBPOSAccountInfo account)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.AveDiscoverQuery_3"))
            {

                mQueryService = AveQueryServiceProvider.Instance<IAveDiscoverQueryService>(conn);
                SupportIB = false;

            }

        }

        public bool SupportIB { get; set; }

        #region Site Level

        public int GetSiteChangedForIB(Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetSiteChangedForIB"))
            {

                return mQueryService.GetSiteChangedForIB(siteId, mStartTime, mEndTime);

            }

        }

        public bool GetSiteChangedForIB(Guid siteId, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetSiteChangedForIB"))
            {

                return mQueryService.GetSiteChangedForIB(siteId, mStartTime, mEndTime, ref siteCollectionChangeType, ref userChangeType, ref groupChangeType);

            }

        }

        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebForIB"))
            {

                return mQueryService.QueryWebForIB(siteId, mStartTime, mEndTime);

            }

        }

        public Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QuerySiteSecurityForIB"))
            {

                return mQueryService.QuerySiteSecurityForIB(siteId, mStartTime, mEndTime);

            }

        }

        #endregion

        #region Web Level

        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebRootFolder"))
            {

                mQueryService.QueryWebRootFolder(listCache, rootFolderObject, mDiscoverReader, mNoPropertyFolders);

            }

        }

        public Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebSecurityForIB"))
            {

                return mQueryService.QueryWebSecurityForIB(siteId, webId, mStartTime, mEndTime);

            }

        }

        public Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListForIB"))
            {

                return mQueryService.QueryListForIB(siteId, webId, mStartTime, mEndTime);

            }

        }

        #endregion

        #region List Level
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "tp_MaxMajorwithMinorVersionCount is a part of Keys")]
        public void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListRootFolder"))
            {

                mQueryService.QueryListRootFolder(listCache, mDiscoverReader, listObject, rootFolderObject);

            }

        }

        public Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListAlertForIB"))
            {

                return mQueryService.QueryListAlertForIB(siteId, webId, listId, mStartTime, mEndTime);

            }

        }

        public Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListViewForIB"))
            {

                return mQueryService.QueryListViewForIB(siteId, webId, listId, mStartTime, mEndTime);

            }

        }

        public Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListSecurityForIB"))
            {

                return mQueryService.QueryListSecurityForIB(siteId, webId, listId, mStartTime, mEndTime);

            }

        }

        public Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListContentTypeForIB"))
            {

                return mQueryService.QueryListContentTypeForIB(siteId, webId, listId, mStartTime, mEndTime);

            }

        }

        private bool InvalidDirName(string dirName, SqlDataReader sr)
        {
            if (sr["Id"] is DBNull)
            {
                return false;
            }
            else
            {
                return !dirName.Equals(((string)sr["DirName"]).Trim('/'), StringComparison.OrdinalIgnoreCase);
            }
        }

        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QuerySystemListItemForIB"))
            {
                mQueryService.QuerySystemListItemForIB(folderCache, folderObject, mStartTime, mEndTime, null, mDiscoverReader, mNoPropertyFolders, extraItems);
            }
        }
        [Obsolete("no use now, will remove later")]
        public void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DiscoverModeForSOIB discoverMode,
            List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            throw new NotImplementedException();
        }

        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListItemForIB"))
            {
                mQueryService.QueryListItemForIB(folderCache, folderObject, mStartTime, mEndTime, listObject, mDiscoverReader, mNoPropertyFolders, extraItems);
            }
        }
        [Obsolete("no use now, will remove later")]
        public void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject,
            DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            throw new NotImplementedException();
        }

        public void QueryChangedListItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryChangedListItemFromExtraItemList"))
            {
                mQueryService.QueryItemFromExtraItemList(folderCache, folderObject, listObject, mDiscoverReader, mNoPropertyFolders, extraItems);
            }
        }

        #endregion

        #region Item Level

        public Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryItemSecurityForIB"))
            {

                return mQueryService.QueryItemSecurityForIB(siteId, webId, listId, itemId, mStartTime, mEndTime);

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "TransListIdToTitle is function name")]
        public void QueryAttachmentByItemObj(Guid siteId, string listRootFolderUrl, AveItemObject itemObj, IAveWeb web, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryAttachmentByItemObj"))
            {

                mQueryService.QueryItemAttachment(siteId, listRootFolderUrl, itemObj, mDiscoverReader);

            }

        }

        #endregion

        #region FB

        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QuerySiteWebForFB"))
            {

                return mQueryService.QuerySiteWebForFB(siteId);

            }

        }

        public AveWebObject QueryRootWeb(Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryRootWeb"))
            {

                return mQueryService.QueryRootWeb(siteId);

            }

        }

        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetSubWebs"))
            {

                return mQueryService.GetSubWebs(siteId, parentWebId, includeRecycleBin);

            }

        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebListForFB"))
            {

                return mQueryService.QueryWebListForFB(siteId, webId, false);

            }

        }

        public Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebListForFB"))
            {

                return mQueryService.QueryWebListForFB(siteId, webId, includeRecycleBin);

            }

        }

        public Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListViewForFB"))
            {

                return mQueryService.QueryListViewForFB(siteId, webId, listId);

            }

        }

        public void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin, bool includeSystemFolder, bool includeVersion)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryListItemForFB"))
            {

                mQueryService.QueryListItemForFB(folderCache, folderObject, parentListObject, mDiscoverReader, includeRecycleBin, includeVersion);

            }

        }

        public void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryStubItemForFB"))
            {

                mQueryService.QueryStubItemForFB(folderCache, folderObject, parentListObject, mDiscoverReader, includeRecycleBin);

            }

        }

        public int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject ParentListObject, bool includeRecycleBin = false)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetAllStubCount"))
            {

                return mQueryService.GetAllStubCount(folderCache, folderObject, ParentListObject, includeRecycleBin);

            }

        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebContentTypeForFB"))
            {

                return mQueryService.QueryWebContentTypeForFB(siteId, webId);

            }

        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.QueryWebContentTypeForFB"))
            {

                return mQueryService.QueryWebContentTypeForFB(siteId, serverRelativeUrl);

            }

        }

        #endregion

        #region For Replicator

        public AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, int? maxMajorwithMinorVersionCount)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemExist"))
            {

                return mQueryService.GetItemExist(SiteId, webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, mDiscoverReader, maxMajorwithMinorVersionCount);

            }

        }

        private DateTime GetTimeAndDocId(SqlDataReader sr, ref Guid docId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetTimeAndDocId"))
            {

                DateTime result = DateTime.MinValue;
                if (sr.Read())
                {
                    docId = sr.GetGuid(0);
                    result = sr.GetDateTime(1);
                    return result;
                }
                return DateTime.MinValue;

            }

        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemLastModifiedTime"))
            {

                return mQueryService.GetItemLastModifiedTime(siteId, webId, listId, dirName, leafName, ref docId);

            }

        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemLastModifiedTime"))
            {

                return mQueryService.GetItemLastModifiedTime(siteId, listId, rowId);

            }

        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemLastModifiedTime_1"))
            {

                return mQueryService.GetItemLastModifiedTime(siteId, itemId);

            }

        }

        public AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemVersions"))
            {

                return mQueryService.GetItemVersions(siteId, listId, docLibRowId);

            }

        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return mQueryService.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
        }

        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetTPGUIDAndDocIdMapping"))
            {

                return mQueryService.GetTPGUIDAndDocIdMapping(siteId, parentId);

            }

        }

        public bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.IsHaveSameName"))
            {

                return mQueryService.IsHaveSameName(siteId, webId, listId, dirName, leafName);

            }

        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.IsListItemHaveSameName"))
            {

                return mQueryService.IsListItemHaveSameName(siteId, webId, tpGuid, listId, rowId);

            }

        }

        public List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemWebParts"))
            {

                return mQueryService.GetItemWebParts(siteId, webId, listId, itemDocId);

            }

        }

        public long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetItemSizeAndUserInfo"))
            {

                return mQueryService.GetItemSizeAndUserInfo(siteId, webId, listId, docId, level, ref createdBy, ref modifiedBy);

            }

        }

        #endregion


        #region Support Extender
        public int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.GetCurrentUIVersion"))
            {

                return mQueryService.GetCurrentUIVersion(siteId, parentId, docId);

            }

        }
        #endregion

        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.InitDiscoverWeb"))
            {

                mQueryService.InitDiscoverWeb(webCache, webObj);

            }

        }

        public void InitDiscoverList(AveListCache listCache, AveListObject listObj)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.InitDiscoverList"))
            {

                mQueryService.InitDiscoverList(listCache, listObj);

            }

        }

        public void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, ref AveListObject parentListObject)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDiscoverQuery.InitDiscoverFolder"))
            {

                mQueryService.InitDiscoverFolder(folderCache, folderObj, mNoPropertyFolders, ref parentListObject, mDiscoverReader);

            }

        }

        public void Dispose()
        {
            if (mQueryService != null)
            {
                mQueryService.Dispose();
                mQueryService = null;
            }
        }

        public IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest)
        {
            return this;
        }

        #region IAveDiscoveryQuery Members


        public long GetWebSize(Guid siteId, Guid webId)
        {
            return mQueryService.GetWebSize(siteId, webId);
        }

        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime)
        {
            return mQueryService.GetObjectChangedSize(siteId, webId, listId, folderPath, beginTime);
        }

        public long GetListSize(Guid siteId, Guid webId, Guid listid)
        {
            return mQueryService.GetListSize(siteId, webId, listid);
        }

        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            return mQueryService.GetFolderSize(siteId, webId, listId, folderUrl);
        }

        #endregion


        public void QueryAttachmentByItemObj(IAveWeb web, Guid listId, AveItemObject itemObject)
        {
        }

        public void QueryVersionsByItemObj(AveItemCache itemCache, AveItemObject itemObject)
        {
        }

        public void DiscoverAllListContent(AveListCache listCache, AveItemObject rootFolderObj, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }
    }
}
