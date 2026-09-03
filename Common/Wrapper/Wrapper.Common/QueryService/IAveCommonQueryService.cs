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
using System.Collections;

namespace AvePoint.Wrapper.Common
{
    public interface IAveCommonQueryService : IAveQueryService
    {
        #region MetadataServiceApplication

        int GetLanguage(ref AveTermStoreInfo termStoreInfo, Guid defaultPartitionId);

        List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId);

        AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId);

        List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId);

        AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId, int defaultLanguage);

        List<Guid> GetTermIds(Guid termSetId, Guid defaultPartitionId);

        List<Guid> GetTermIds(Guid termSetId, Guid termId, Guid defaultPartitionId);

        bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId);

        List<Guid> GetSiteCollectionId(Guid groupId, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChanges(Nullable<int> groupId, Nullable<int> termSetId, DateTime sinceTime, Nullable<int> changedItemType, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage);

        int GetTermId(Guid termId, Guid defaultPartitionId);

        Dictionary<Guid, string> GetGroupIds(bool isGlobal);

        int GetGroupId(Guid groupId);

        bool IsPublished(string contentTypeId, Guid defaultPartitionId);

        bool IsUnPublished(string contentTypeId, Guid defaultPartitionId);

        string GetTermStore(Guid defaultpartitionId);

        List<AveTermInfo> GetTerms(Guid termSetId, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId);

        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermChangeItem> GetChangesInGroup(Guid groupId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, Nullable<DateTime> sinceTime, Nullable<DateTime> toTime, Guid defaultPartitionId, int defaultLanguage);

        List<AveTermSetInfo> GetTermSetIds(Guid groupId, int defaultLangugage);

        List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId, int defaultLanguage);

        AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId, int defaultLanguage);

        AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId, int defaultLanguage);

        string GetTermDefaultLabel(int termId, Guid defaultPartitionId, int defaultLanguage);

        #endregion

        #region others

        #region Replicator

        IAveQueryDataReader GetAllWebs();

        IAveQueryDataReader GetAllEventReceivers(string assemblyFullName);

        void Commit(List<string> scripts);

        void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs);

        void WebDelAndMoveEventHandler(Guid webId, Guid siteId, string assemblyFullName, string eventHandlerClassNames);

        void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder);

        IAveListItem GetListItem(IAveSite site, IAveList list, Guid tp_GUID);

        [Obsolete("Please use GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID) instead")]
        int GetCheckOutUserID(Guid siteID, Guid itemID);

        int GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID);
        #endregion

        #region ContentDatabase

        List<AveUserDetail> GetUserDetailByNative(string userSearchInfo, AveAccountSearchFlag flag, string siteId, bool isExact);

        #endregion

        #region Central Admin

        void GetDBSize(out double usedSize, out double freeSize, out double diskFreesize);

        string GetDBServerName(IAveContentDatabase db);

        IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix);

        string GetSiteIds(Guid webAppId);

        IAveQueryDataReader GetSiteNoPermissionAccounts(Guid siteId, Guid scopeId, List<string> searchUsers);

        string GetDocNameFromDB(Guid siteId, Guid webId);

        void DeleteOrphanSiteInDB(IAveContentDatabase dataBase, string itemId);

        IAveQueryDataReader WebAddWebPartMessageHandler(Guid siteId, string webPartKey, string webpartNameTemp);

        IAveQueryDataReader SearchDuplicateFiles(List<string> siteIds, List<string> webIds, List<string> excludeFileNames, string fileNamePattern, List<string> includeFileExtensions, bool searchFile, bool searchAttachment);

        #endregion

        string GetNavigationNodeMetainfo(IAveWeb web, int Eid);

        AveFeatureInfoBox GetFeatures(Guid siteId, Guid webId, AveFeatureScope scope);

        ulong GetConnectorDataSize();

        #region Migration
        bool CheckDatabaseServerRole(string userName, ServerRole sRole, byte[] sid);
        bool CheckDatabaseRole(string userName, DatabaseRole dbRole, byte[] sid);
        bool CheckViewServerState(string userName, byte[] sid);
        #endregion

        #endregion
    }
}
