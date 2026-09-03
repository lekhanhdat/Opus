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
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    interface IAveQuerySessionSchema
    {
        #region Replicator

        IAveQueryDataReader GetAllWebs();

        IAveQueryDataReader GetAllListsInWeb(Guid siteId, Guid webId, bool includeRecycleBin);

        void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder);

        void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs);

        IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix);

        IAveQueryDataReader GetAllEventReceivers(string assemblyFullName);

        #endregion

        #region Discover

        AveWebObject QueryRootWeb(Guid siteId);

        Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin);

        void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj);

        Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId);

        void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders);

        Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime);

        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId);

        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl);

        #endregion

        #region Backup/Restore

        AveSiteSettingInfo GetSiteSettingFromSites(IAveSite site);

        long GetSiteSizeFromSites(IAveSite site);

        Dictionary<Guid, long> GetAllWebSize(IAveSite site);

        AveSiteSettingInfo GetFullSiteSetting(IAveSite site);

        int[] GetCollectionIdAndProviderId(Guid siteId);

        string GetPageUrlById(Guid siteId, Guid pageId);

        string GetWebFullUrlById(Guid siteId, Guid webId);

        AveWebSettingInfo GetWebSettingFromWebs(IAveWeb web);

        void SetWebPartLists(AveWebPartBaseInfo webPartInfo, Guid siteId, Guid itemId, byte level);

        Dictionary<Guid, string> GetALLWebTemplates(IAveSite site, uint lcid);

        int GetSubWebCounts(Guid siteId, string serverRelativeUrl);

        List<Guid> GetAllWebsGuidByNative(Guid siteId);

        string GetWebPartsInGallery(Guid siteId);

        Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping);

        bool IsConflictWithRecycle(Guid siteId, string webUrl);

        Guid GetWebId(Guid siteId, string url);

        void GetSubWebsUrl(Guid siteId, Guid parentWebId, Dictionary<string, Dictionary<Guid, string>> infos);

        void GetListPagesUrl(Guid siteId, Guid listId, Dictionary<string, Dictionary<Guid, string>> infos);

        string GetContentTypeName(Guid siteId, byte[] contentTypeId);

        void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList);

        AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope);

        List<string> GetFields(Guid siteId, string scope);

        bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId);

        string GetWebCTNameById(Guid siteId, string contentTypeId);

        bool CheckContentTypeExist(Guid siteId, byte[] ctId);

        void UpdateWebsAuthorByNative(int userId, Guid siteId, Guid webId);

        #endregion

        #region GA+

        Dictionary<Guid, StorageUsageInfo> GetSitesStorageInfo();

        #endregion

    }
}
