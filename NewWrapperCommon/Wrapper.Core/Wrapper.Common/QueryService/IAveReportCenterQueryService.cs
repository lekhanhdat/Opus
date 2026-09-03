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



namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAveReportCenterQueryService : IAveQueryService
    {
        #region Blob Calc
        [Obsolete("Please use GetBlobRawDataSP2010SubFoldersAndItems instead")]
        IAveQueryDataReader GetBlobRawDataSP2010UnderFolder(Guid siteId, string folderId);

        [Obsolete("Please use GetBlobRawDataSP2010Versions instead")]
        IAveQueryDataReader GetBlobRawDataSP2010Info(Guid siteId, Guid listId, string docId, int uiVersion, int isCurrentVersion);

        [Obsolete("Please use GetBlobRawDataSP2010UserInfo instead")]
        IAveQueryDataReader GetBlobRawDataSP2010Version(Guid siteId, string id);

        IAveQueryDataReader GetBlobRawDataSP2010SubFoldersAndItems(Guid siteId, Guid parentId);

        IAveQueryDataReader GetBlobRawDataSP2010Versions(Guid siteId, Guid parentId, Guid id);

        IAveQueryDataReader GetBlobRawDataSP2010UserInfo(Guid siteId);

        IAveQueryDataReader GetBlobRawDataSP2010Attachments(Guid siteId, Guid attachmentFolderId);

        IAveQueryDataReader BlobSP2010GetItemSize(Guid siteId, Guid listId);

        #region Blob Raw Data SP2013
        IAveQueryDataReader BlobRawDataSP2013Documents(Guid siteId, Guid parentId, Guid docId);
        IAveQueryDataReader BlobRawDataAttachmentInfos(Guid siteId, Guid attachmentsFolderId);
        IAveQueryDataReader BlobRawDataSP2013Attachments(Guid siteId, Guid docId);
        #endregion

        #region Blob calculator SP2010
        IAveQueryDataReader BlobSP2010Documents(string aveSiteId, string parentId);

        IAveQueryDataReader BlobSP2010DocumentVersions(string aveSiteId, string parentId);

        IAveQueryDataReader BlobSP2010ListItems(Guid listId);

        IAveQueryDataReader BlobSP2010ListItemVersions(Guid listId);

        IAveQueryDataReader BlobSP2010GetListInfo(Guid siteId);

        IAveQueryDataReader BlobSP2010Documents(Guid siteId, int commandTimeout);

        IAveQueryDataReader BlobSP2010DocumentVersions(Guid siteId, int commandTimeout);

        #endregion Blob calculator SP2010

        #region Blob inventory SP2010

        IAveQueryDataReader BlobInventorySP2010GetSOInfo(Guid aveSiteId, Guid parentId);
        IAveQueryDataReader BlobInventorySP2010GetSOInfoInList(Guid aveSiteId, Guid attachmentFolderId);
        Guid BlobInventorySP2010GetAttachmentsFolder(Guid aveSiteId, Guid rootFolderId);
        List<byte[]> BlobInventorySP2010GetDeleteTransactionId(Guid siteId, Guid webId);

        IAveQueryDataReader BlobInventorySP2010GetSOInfo(Guid siteId, int commandTimeout);
        IAveQueryDataReader BlobSP2010GetItemSize(Guid siteId);
        #endregion Blob inventory SP2010

        #region Blob Calculator SP2013
        IAveQueryDataReader BlobSP2013DocAllVersionInfo(Guid siteId, Guid parentId, Guid docId);
        IAveQueryDataReader BlobSP2013DocVersionInfo(Guid siteId, Guid parentId, Guid docId);
        IAveQueryDataReader BlobSP2013ItemsInList(Guid siteId, Guid listId);
        IAveQueryDataReader BlobSP2013ListItemVersions(Guid siteId, Guid listId);
        [Obsolete("This function has been replaced by the one which needs three parameters.")]
        IAveQueryDataReader BlobSP2013DocumentCurrentVersion(Guid siteId, Guid parentId);
        IAveQueryDataReader BlobSP2013DocumentCurrentVersion(Guid siteId, Guid parentId, Guid docId);

        IAveQueryDataReader BlobSP2013AllDocBSN(Guid siteId, int commandTimeout);
        #endregion

        #endregion

        #region Admin Report Storage Report and Blob

        IAveQueryDataReader BlobSP2013DocIdsByParentId(Guid siteId, Guid parentId);
        IAveQueryDataReader BlobSP2013AttachmentIdsByParentId(Guid siteId, Guid parentId);
        IAveQueryDataReader BlobSP2013GetInfo(Guid siteId, Guid docId);
        IAveQueryDataReader BlobSP2013GetItemSize(Guid siteId, Guid listId);
        #endregion

        #region Storage Trends

        #region New Logic

        /// <summary>
        /// 该方法返回Folder.SubFolder
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        List<Guid> GetSubFolders(Guid siteId, Guid parentId);

        /// <summary>
        /// 该方法返回Folder.SubFolders,包括被删除的SubFolder
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        List<Guid> GetSubFoldersIncludeDeleted(Guid siteId, Guid parentId);

        Dictionary<Guid, bool> GetRootFoldersAndBaseTypeIncludeDeleted(Guid webId);

        /// <summary>
        /// 该方法返回web下所有list的root folder，包括被删除的List
        /// </summary>
        /// <param name="webId">Web ID</param>
        /// <returns>Dictionary<ListId,RootFolderId></returns>
        List<Guid> GetRootFoldersIncludeDeleted(Guid webId);

        /// <summary>
        /// 该方法返回Document Library下单个文件夹的统计信息
        /// </summary>
        /// <example>
        /// Extension   ItemCount   VersionCount    MaxVersionCount     Size
        /// cs	        14	        20	            5	                170765
        /// csproj	    1	        1	            1	                17611
        /// </example>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        IAveQueryDataReader GetStaticInfoForLibraryFolder(Guid siteId, Guid parentId);

        /// <summary>
        /// 该方法返回Web或者List下单个文件夹的统计信息，不包括ListItem（ListItem没有Extension，另外提供方法单独支持）
        /// 这些文件都不会产生Version，所以VersionCount等于ItemCount,MaxVersionCount为1
        /// </summary>
        /// <example>
        /// Extension   ItemCount   Size
        /// aspx	    14	        170765  //注：View，System Page
        /// txt 	    1	        17611   //注：Attachment
        /// </example>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        IAveQueryDataReader GetStaticInfoForWebOrListFolder(Guid siteId, Guid parentId);

        /// <summary>
        /// 该方法返回List下单个文件夹的ListItem统计信息，只有一行
        /// </summary>
        /// <example>
        /// ItemCount   VersionCount    MaxVersionCount     Size
        /// 14	        20	            5	                170765
        /// </example>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        IAveQueryDataReader GetStaticInfoForListItems(Guid siteId, Guid parentId);

        #endregion

        /// <summary>
        /// SP10 SP07
        /// </summary>
        /// <returns></returns>
        long GetContentDBStubSize();

        #region only used for sp07
        /// <summary>
        /// SP07
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        //IAveQueryDataReader GetDataFromDocByName(string dirName, Guid siteId);
        /// <summary>
        /// SP07
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        //IAveQueryDataReader GetDataFromUserDataByName(string dirName, string leafName, Guid siteId);
        /// <summary>
        /// SP07
        /// </summary>
        /// <param name="id"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        //IAveQueryDataReader GetDataFromDocVersionById(string id, Guid siteId);
        #endregion

        #endregion

        #region Site Referrers
        IAveQueryDataReader GetSiteReferrerData(bool isSelectedAll, DateTime beginTime, DateTime endTime, string aggregationId);
        #endregion

        #region Last Access Time
        IAveQueryDataReader GetAuditData(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime);
        IAveQueryDataReader GetAuditData(string viewName, DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid siteId);
        IAveQueryDataReader GetListIds(Guid siteId, Guid webId);
        IAveQueryDataReader GetDocIds(Guid siteId, Guid webId);
        IAveQueryDataReader GetSiteUsers(Guid siteId, SPUserFilter userFilter);

        IAveQueryDataReader GetLastAccessTimeOfSite(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter);
        IAveQueryDataReader GetLastAccessTimeOfWeb(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter);
        IAveQueryDataReader GetLastAccessTimeOfList(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter);
        IAveQueryDataReader GetLastAccessTimeOfItem(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter);

        IAveQueryDataReader GetLastAccessTimeOfSite(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime);
        IAveQueryDataReader GetLastAccessTimeOfWeb(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime);
        IAveQueryDataReader GetLastAccessTimeOfList(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime);

        #endregion

        #region Search Usage
        IAveQueryDataReader GetSearchUsage(bool isGetAllSearchUsage, DateTime beginTime, DateTime endTime, List<string> aggregationIdList);
        string GetSearchUsageDayColumn(DateTime time);
        IAveQueryDataReader GetSearchUsageDataByDay(byte[] partitionId, int reportType, DateTime reportDate, string day, int index, int pageSize);
        #endregion

        #region Admin Report

        IAveQueryDataReader AdminReportSP10GetListBlobData(Guid listId);

        IAveQueryDataReader AdminReportSP10GetWebBlobData(Guid webId);

        IAveQueryDataReader AdminReportSP10GetWebRecycleBinSize(Guid webId);

        IAveQueryDataReader AdminReportSP13GetWebRecycleBinSize(Guid siteId, Guid webId);

        int AdminReportSP10GetListCountUnderWeb(Guid webId);

        int AdminReportSP13GetListCountUnderWeb(Guid siteId, Guid webId);

        long AdminReportSP10GetSCRecycleBinSize(Guid siteId);

        long AdminReportSP10GetAuditInfo(Guid webId);

        IAveQueryDataReader GetLibLastAndVersionSizeByParentId(Guid siteId, Guid parentId);
        IAveQueryDataReader GetListLastAndVersionSizeByParentId(Guid siteId, Guid parentId);
        IAveQueryDataReader GetLastAndVersionSizeForWebOrListFolder(Guid siteId, Guid parentId);
        long GetDocumentVersionSize(Guid siteId, Guid parentId);
        int GetNumberOfFileTypes(Guid siteId, Guid parentId);

        #region new storage size logic method

        IAveQueryDataReader GetSiteIdInContentDatabase(int commandTimeout);

        int GetWebCountInSite(Guid siteId, int commandTimeout);

        int GetListCountInSite(Guid siteId, int commandTimeout);

        #region site level size
        IAveQueryDataReader AdminReportGetWholeSiteVersionSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWholeSiteDocSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWholeSiteItemSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWholeSiteRecycleSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWholeSiteSOSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetOnlySiteRBSSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetOnlySiteRecycleSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetOnlySiteItemSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetOnlySiteDocSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetOnlySiteVersionSize(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportSP13GetListInfoUnderSite(Guid siteId, int commandTimeout);
        #endregion

        #region web level size
        IAveQueryDataReader AdminReportGetWebSOSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWebRecycleSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWebItemSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWebDocSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWebVersioniSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportSP13GetListInfoUnderWeb(Guid siteId, Guid webId, int commandTimeout);
        #endregion

        #region list level size
        IAveQueryDataReader AdminReportGetListSOSize(Guid siteId, Guid webId, Guid listId, int commandTimeout);

        IAveQueryDataReader AdminReportGetListRecycleSize(Guid siteId, Guid webId, Guid listId, int commandTimeout);

        IAveQueryDataReader AdminReportGetListItemSize(Guid siteId, Guid webId, Guid listId, int commandTimeout);

        IAveQueryDataReader AdminReportGetListDocSize(Guid siteId, Guid webId, Guid listId, int commandTimeout);

        IAveQueryDataReader AdminReportGetListVersionSize(Guid siteId, Guid webId, Guid listId, int commandTimeout);
        #endregion

        #endregion

        #region SC/Web Usage Report
        IAveQueryDataReader AdminReportGetTotalHits(Guid aggregationId, int startDate, int endDate, int commandTimeout);

        IAveQueryDataReader AdminReportGetUsageTotalHits(Guid aggregationId, int startDate, int endDate, int timeNow, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenHitAllTime(Guid aggregationId, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenHitLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenVisitorAllTime(Guid aggregationId, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenVisitorLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenLeastHitAllTime(Guid aggregationId, int commandTimeout);

        IAveQueryDataReader AdminReportGetTopTenLeastHitLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout);

        #endregion

        IAveQueryDataReader AdminReportGetContentDBSize(int commandTimeout);

        string AdminReportGetFarmPersonalSiteLocation(Guid partitionID, int commandTimeout);

        long AdminReportGetSiteDiskUsed(Guid siteId, int commandTimeout);

        #region web general info

        bool AdminReportIsOrphanSite(Guid webId, int commandTimeout);

        object AdminReportGetWebLastAccessedTime(Guid siteId, Guid webId, int commandTimeout);

        string AdminReportGetLastModifier(Guid webId, int commandTimeout);

        [Obsolete("function invalid.")]
        IAveQueryDataReader AdminReportGetWebFullControlUsers(string siteId, string webId, int commandTimeout);

        #endregion

        #region number info of List and Library
        IAveQueryDataReader AdminReportGetNumberOfDocumentLibraries(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfLists(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfLitItems(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfDocuments(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfAttachments(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetDocumentsTotalSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetBigFile(Guid siteId, Guid webId, long fileSizeBytes, int commandTimeout);

        IAveQueryDataReader AdminReportGetListTotalSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfListPersonalView(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfListPublicView(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfDiscussionBoard(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfDiscussionItem(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetDiscussionBoardTotalSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportSP13GetDiscussionBoardTotalSize(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfSurvey(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetNumberOfSurveyResponse(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetSurveyTotalSize(Guid siteId, Guid webId, int commandTimeout);

        #endregion

        int AdminReportGetPageNumInWeb(Guid siteId, Guid webId, int commandTimeout);

        int AdminReportGetCustomPageNumInWeb(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetWebContentAnalysis(Guid siteId, Guid webId, int commandTimeout);

        IAveQueryDataReader AdminReportGetSCContentAnalysis(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetSCLastAccessTime(Guid siteId, int commandTimeout);

        IAveQueryDataReader AdminReportGetQueryCrawlStatus(string appId, int commandTimeout);

        #endregion

        IAveQueryDataReader AdminReportGetItemSizes(List<string> itemIds, int commandTimeout);

        IAveQueryDataReader PageTrafficGetItemSizes(List<string> itemIds, int commandTimeout, Guid siteId);

        #region Best Practice Reports

        long BPRSP2010GetUserProfileCount();
        long BPRSP2010GetSocialContentCount();
        IAveQueryDataReader GetTermSetByLevelLimit(int level);
        IAveQueryDataReader GetTermSetByTermLimit(long limit);
        long GetTermSetNumber();
        long GetItemNumber();

        /// <summary>
        /// discuss with PM,this count include item or document version
        /// </summary>
        /// <returns></returns>
        long BPRSP2010GetItemCount();
        IAveQueryDataReader BPRSP2010GetMajorVersionCount(Guid listId, long maxValue, int isList);
        IAveQueryDataReader BPRSP2010GetGroupCountAUserBelongTo(long maxGroupCount, string siteId);
        IAveQueryDataReader GetAlertItemByAlertId(string alertId, bool isImme);
        long BPRSP2010DocumentCountInList(string siteId, string listId);
        long BPRSP2010GetUserCountInSC(string siteId);
        long BPRSP2010GetGroupCountInSC(string siteId);
        IAveQueryDataReader BPRSP2010GetSecurityScope(string siteId);
        long BPRSP2010GetPrincipalCount(string siteId, string scopeId);
        long BPRSP2010SecurityScoptCount(string siteId, string listId);
        IAveQueryDataReader BPRSP2010GetAllDocInfoInSC(Guid siteId);
        IAveQueryDataReader GetAllUserData(Guid siteId);
        int BPRSP2010GetSubWebCount(Guid siteId, Guid webId);
        #endregion

        #region User Storage Size
        IAveQueryDataReader GetUserStorageDocInfo(Guid siteId, Guid parentId);
        IAveQueryDataReader GetUserStorageItemInfo(Guid siteId, Guid parentId);
        IAveQueryDataReader GetUserStorageAttachment(Guid siteId, Guid parentId);
        IAveQueryDataReader GetUserStorageDocInfoWithTimeScope(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end);
        IAveQueryDataReader GetUserStorageAttachmentWithTimeScope(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end);
        IAveQueryDataReader GetUserStorageItemInfo(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end);
        #endregion

        #region Content Type Usage
        long GetContentTypeUsageCountInList(Guid listId, byte[] ctbytes);
        long GetContentTypeUsageCountInList(Guid siteId, Guid listId, byte[] ctbytes);
        #endregion

        #region RC Common Query
        IAveQueryDataReader GetSiteUserInfo(Guid siteId);
        IAveQueryDataReader GetDocumentInfo(Guid siteId, Guid docId);
        IAveQueryDataReader GetDocumentInfo(Guid siteId, List<Guid> docIds);
        #endregion

        IAveQueryDataReader GetSocialBlogPost(Guid siteId, Guid parentId);

        IAveQueryDataReader GetSocialBlogCommentsWithPost(Guid siteId, Guid parentId);

        #region SP Usage Data
        IAveQueryDataReader GetUsageData(string viewName, DateTime startTimeToDisplay, DateTime endTimeToDisplay);

        IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay);

        IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid webId);

        IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid webId, long index, int pageSize);
        #endregion SP Usage Data

        IAveQueryDataReader GetWebPartsByPage(Guid siteId, Guid pageId);

        #region Storage Metrics
        long GetSizeFromStorageMetrics(Guid siteId, Guid folderId);
        long GetSizeFromStorageMetrics(Guid siteId, List<Guid> folderIds);
        #endregion Storage Metrics

        #region Usage Pattern Alerting
        IAveQueryDataReader GetItemLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime);
        IAveQueryDataReader GetItemLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime, List<int> eventTypes, List<int> itemTypes);
        IAveQueryDataReader GetListLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime);
        IAveQueryDataReader GetWebLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime);
        IAveQueryDataReader GetSiteLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime);
        IAveQueryDataReader GetItemAddedEvents(Guid siteId, long startEventId, long endEventId);
        long GetNextEventId(DateTime endTime);
        long GetPreviousEventId(DateTime startTime);
        IAveQueryDataReader GetItemInfos(Guid siteId, List<Guid> itemIds);
        IAveQueryDataReader GetAuditData(Guid siteId, DateTime startTime, DateTime endTime, List<int> eventTypes, List<int> itemTypes);
        #endregion 

        IAveQueryDataReader GetCheckedOutFiles(Guid guid, string libraryUrl);
        IAveQueryDataReader GetUserAndPersonalSite();

        IAveQueryDataReader GetMysiteFollowedItems();

        IAveQueryDataReader GetWebInfos();

        IAveQueryDataReader GetDocInfoWithWebId(string siteId);
    }

    public class SPUserFilter
    {
        public UserFilterType userFilterType;
        public List<string> IncludeUsers = new List<string>();
        public List<string> ExcludeUsers = new List<string>();
        public List<RCUserDetail> Users = new List<RCUserDetail>();
    }

    public class RCUserDetail
    {
        public string SPLoginName { get; set; }
        public string LoginName { get; set; }
        public string Prefix { get; set; }
    }
    public enum UserFilterType
    {
        IncludeAll,
        Include,
        Exclude
    }
}