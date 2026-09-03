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
    /// <summary>
    /// 下面是所有Discover用到的SqlString（不包括IB Item-QueryListItemForIB 查询的语句）
    /// </summary>
    public class DiscoverConditionString
    {
        public const string ListItems = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId";
        public const string WebItems = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type<>2 ";
        public const string ListItemsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 ";
        public const string WebItemsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type<>2 ";

        public const string ListItems07 = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.DirName=@DirName";
        public const string WebItems07 = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND doc.DeleteTransactionId=0x AND doc.DirName=@DirName AND doc.Level<=255 AND doc.Type<>2 ";
        public const string ListItemsWithRecycleBin07 = "WHERE doc.SiteId=@SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.DirName=@DirName AND doc.Level<=255 ";
        public const string WebItemsWithRecycleBin07 = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.DirName=@DirName AND doc.Level<=255 AND doc.Type<>2 ";

        public const string ListItemExits = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Id=@Id AND doc.Level<=255 ";
        public const string DocumentExits = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.DirName=@DirName AND doc.LeafName=@LeafName AND doc.Level<=255 ";

        public const string ItemStubsByIds = "WHERE doc.SiteId=@SiteId AND doc.Id in ({0}) AND doc.DeleteTransactionId=0x AND doc.Level<=255 AND doc.IsCurrentVersion=1";
        public const string ItemStubsByIdsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND doc.Id in ({0}) AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.IsCurrentVersion=1 AND doc.Level<=255";

        public const string WebStubItems = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type <= 0 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";
        public const string WebStubItemsForAllDocVersions = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type <= 0 AND (docver.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string WebStubItemsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type <= 0 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";
        public const string WebStubItemsForAllDocVersionsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND doc.ListId IS NULL AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type <= 0 AND (docver.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string ListStubItems = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";
        public const string ListStubItemsForAllDocVersions = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND (docver.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string ListStubItemsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";
        public const string ListStubItemsForAllDocVersionsWithRecycleBin = "WHERE doc.SiteId=@SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x) AND doc.ParentId=@ParentId AND doc.Level<=255 AND (docver.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string ListSubFolders = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.Type=1";
        public const string WebSubFolders = "WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=@ParentId AND doc.Level<=255 AND doc.ListId IS NULL AND doc.Type=1";

        //添加了WebId作为筛选条件，可能会对某些模块造成影响。目前尚不确定。
        public const string ListItemChanged =
@"WHERE ec.EventTime <= @endTime AND ec.EventTime >= @startTime AND ec.SiteId=@SiteId AND ec.WebId=@webId
AND (ec.ListId=@ListId OR (ec.ListId IS NULL AND ec.DocId IS NOT NULL)) AND (ec.ItemId IS NOT NULL OR ec.ObjectType IN (16,32,64,4096)) ORDER BY ec.EventTime ";
        public const string WebItemChanged =
@"WHERE ec.EventTime <= @endTime AND ec.EventTime >= @startTime AND ec.SiteId=@siteId AND ec.WebId=@webId AND 
ec.ListId IS NULL AND ec.ObjectType IN (16,32) ORDER BY ec.EventTime ";

        //添加了WebId作为筛选条件，可能对某些模块造成影响。目前尚不确定。
        public const string ListItemChangedIgnoreModifiedBySystem =
@"WHERE ec.EventTime <= @endTime AND ec.EventTime >= @startTime AND ec.SiteId=@SiteId AND ec.WebId=@webId
AND (ec.ListId=@ListId OR (ec.ListId IS NULL AND ec.DocId IS NOT NULL)) AND (ec.ItemId IS NOT NULL OR ec.ObjectType IN (16,32,64,4096)) and ec.EventType <> 1048576 ORDER BY ec.EventTime ";

        public const string ListItemUserdata = "WHERE data.tp_SiteId = @SiteId AND data.tp_DeleteTransactionId = 0x AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1) AND tp_ParentId = @ParentId";

        public const string ListItemUserdataWithDocIds = "WHERE data.tp_SiteId = @SiteId AND data.tp_DeleteTransactionId = 0x AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1)  AND data.tp_DocId in ({0})";
        public const string ListItemUserdataWithDocId = "WHERE data.tp_SiteId = @SiteId AND data.tp_DeleteTransactionId = 0x AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1) AND tp_ParentId = @ParentId AND data.tp_DocId =@Id";

        public const string ListItemVersionsWithDocIdsForExtender = "WHERE doc.Id in ( {0} )";
        public const string ListItemVersionsWithDocIdForExtender = "WHERE doc.SiteId = @SiteId AND doc.Id = @Id";

        public const string VersionConditionWithDocIds = "WHERE data.tp_DocId in({0}) AND tp_Level<=255";

        public const string ListItemUserdataWithRecycleBin = "WHERE data.tp_SiteId = @SiteId AND (data.tp_DeleteTransactionId = 0x OR data.tp_DeleteTransactionId <> 0x) AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1) AND tp_ParentId = @ParentId";

        public const string ListItemUserdataFor07 = "WHERE data.tp_SiteId = @SiteId AND data.tp_DeleteTransactionId = 0x AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1) AND tp_DirName = @DirName";

        public const string ListItemUserdataWithRecycleBinFor07 = "WHERE data.tp_SiteId = @SiteId AND (data.tp_DeleteTransactionId = 0x OR data.tp_DeleteTransactionId <> 0x) AND (data.tp_IsCurrentVersion = 0 OR data.tp_IsCurrentVersion = 1)  AND tp_DirName = @DirName";

        public const string DocIds = "WHERE doc.id in ({0}) AND doc.IsCurrentVersion = 1";

        public const string DocIdsFor13 = "WHERE doc.SiteId = @siteId AND doc.id in ({0})  AND doc.IsCurrentVersion = 1 ";

        public const string SystemDocIds = "WHERE doc.id in ({0}) AND doc.ListId IS NULL AND doc.IsCurrentVersion = 1 ";

        public const string SystemDocIdsFor13 = "WHERE doc.SiteId = @siteId AND doc.id in ({0}) AND doc.ListId IS NULL AND doc.IsCurrentVersion = 1 ";

        public const string ConditionCurrentVersion = " AND tp_IsCurrentVersion = 1";

        public const string FolderURLs = @"WHERE doc.SiteId=@siteId AND doc.DeleteTransactionId=0x AND ( @Urls ) AND doc.Level<=255";

        public const string StubInfoByIdForAllDocs = "WHERE doc.SiteId=@SiteId AND doc.Id=@Id AND doc.DeleteTransactionId=0x AND doc.Level<=255";

        public const string StubInfoByIdForAllDocVersions = "WHERE docver.SiteId=@SiteId AND docver.Id=@Id AND docver.DeleteTransactionId=0x AND docver.Level<=255";

        public const string ItemDocLibRowIds = @"WHERE doc.SiteId = @SiteId AND doc.DeleteTransactionId=0x AND doc.ListId=@listId And doc.DirName like '{0}'  AND doc.DoclibRowId in ({1}) AND doc.Level<=255";
    }

    public class CommonColumn
    {

        public const string AllDocsLogicalColumns =
@"              
 doc.Id,
doc.LeafName,
doc.DoclibRowId,
doc.Type ";

        public const string AllDocsCommonColumns =
@"
 doc.Id,
doc.LeafName,
doc.DoclibRowId,
doc.Type,
doc.DirName,
doc.TimeLastModified,
doc.UIVersion,
doc.Size";

        public const string AllDocsCommonColumns_BigIntSize =
@"
 doc.Id,
doc.LeafName,
doc.DoclibRowId,
doc.Type,
doc.DirName,
doc.TimeLastModified,
doc.UIVersion,
CAST (doc.Size AS bigint) as Size";


        public const string EventCache =
@"
 ec.EventType,
ec.EventTime,
ec.DocId,
ec.ObjectType,
ec.ItemId,
ec.ItemFullUrl,
ec.ItemName,
ec.ModifiedBy,
ec.Guid0,
ec.TimeLastModified,
Int0,
Int1";
    }
    public class ViewColumn
    {
        public const int Id = 0;
        public const int Flags = 1;
        public const int BaseViewID = 2;
        public const int DisplayName = 3;
        public const int PageUrlID = 4;
        public const int UserID = 5;
    }
    public class AveDiscoverQueryString
    {

        #region Common


        public const string AllItemAndVersionForCommon07 =
    @"
SELECT doc.Id,doc.LeafName,doc.DoclibRowId,doc.IsCurrentVersion,doc.Type,doc.TimeLastModified,doc.UIVersion as UIVersion,doc.Level 
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON 
data.tp_ListId=doc.ListId and data.tp_SiteId=doc.SiteId and data.tp_DirName=doc.DirName and data.tp_LeafName=doc.LeafName and data.tp_ID=doc.DoclibRowId AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1)
AND doc.UIVersion = data.tp_UIVersion and doc.level=data.tp_level AND data.tp_CalculatedVersion=0
@WHERE
UNION ALL
SELECT doc.Id,doc.LeafName,doc.DoclibRowId,CONVERT(bit,0),doc.Type,doc.TimeLastModified,docver.Version as UIVersion,docver.Level 
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON  docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x and data.tp_DirName=doc.DirName and data.tp_LeafName=doc.LeafName and data.tp_ID=doc.DoclibRowId
AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1)
AND data.tp_CalculatedVersion=1 AND docver.version=data.tp_UIVersion 
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";



        public const string AllItemsAndVersionsForItem07 =
   @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,doc.Level, data.tp_GUID 
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON 
data.tp_ListId=doc.ListId and data.tp_SiteId=doc.SiteId and data.tp_DirName=doc.DirName and data.tp_LeafName=doc.LeafName and data.tp_ID=doc.DoclibRowId AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1)
AND doc.UIVersion = data.tp_UIVersion and doc.level=data.tp_level AND data.tp_CalculatedVersion=0
@WHERE
UNION ALL
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.Version as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,docver.Level, data.tp_GUID
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) on  docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON 
data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x and data.tp_DirName=doc.DirName and data.tp_LeafName=doc.LeafName and data.tp_ID=doc.DoclibRowId
AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1)
AND data.tp_CalculatedVersion=1 AND docver.version=data.tp_UIVersion 
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string ItemChanged = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.CheckoutUserId,doc.DocFlags,doc.IsCurrentVersion," + CommonColumn.EventCache + @"
FROM EventCache ec WITH(NOLOCK) 
LEFT JOIN AllDocs doc WITH(NOLOCK) ON doc.Id=ec.DocId AND doc.Level<=255 AND doc.IsCurrentVersion=1
@WHERE";

        public const string AllDocValueForEventCache = "SELECT " + CommonColumn.AllDocsCommonColumns_BigIntSize + ",doc.Level,doc.CheckoutUserId,doc.DocFlags,doc.IsCurrentVersion,doc.ParentId,doc.ListId" + @"
FROM ALLDocs doc WITH(NOLOCK)
@WHERE
";

        public const string ItemChangedInCache = @"SELECT Top {0} " + CommonColumn.EventCache + @"
FROM EventCache ec WITH(NOLOCK) 
@WHERE";

        public const string ItemChangedByCustomItems = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.HasStream,doc.DocFlags,doc.DeleteTransactionId" + @"
FROM AllDocs doc WITH(NOLOCK) 
WHERE doc.Level<=255 AND doc.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 AND doc.Id IN(";

        public const string ItemChangedByCustomItems13 = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.HasStream,doc.DocFlags,doc.DeleteTransactionId" + @"
FROM AllDocs doc WITH(NOLOCK) 
WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.Level<=255 AND doc.IsCurrentVersion=1 AND doc.Id IN(";

        public const string ItemDirNameAndLibRowId =
@"SELECT doc.DirName, doc.DoclibRowId
FROM AllDocs doc WITH(NOLOCK)
WHERE doc.Level<=255 AND doc.IsCurrentVersion=1 AND doc.Id=@ItemId";
        public const string ItemDirNameAndLibRowId13 =
@"SELECT doc.DirName, doc.DoclibRowId
FROM AllDocs doc WITH(NOLOCK)
WHERE doc.SiteId=@SiteId AND doc.Level<=255 AND doc.IsCurrentVersion=1 AND doc.Id=@ItemId";

        public const string AttachmentsByCustomItem =
@"SELECT doc.Id
FROM AllDocs doc WITH(NOLOCK)
WHERE doc.Level<=255 AND doc.IsCurrentVersion=1 AND doc.DirName=@AttachmentDirName";
        public const string AttachmentsByCustomItem13 =
@"SELECT doc.Id
FROM AllDocs doc WITH(NOLOCK)
WHERE doc.SiteId=@SiteId AND doc.Level<=255 AND doc.IsCurrentVersion=1 AND doc.DirName=@AttachmentDirName";

        /// <summary>
        /// Only for FB,query all attachments.  SP 10,13  CM,GR,RP,PR item,SPM
        /// </summary>
        public const string AllAttachmentsForCommon =
@"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.TimeLastModified,doc.UIVersion,doc.Size
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl
";

        /// <summary>
        /// Only For SPM 07
        /// </summary>
        public const string AllAttachmentsFor07 =
@"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.TimeLastModified,doc.UIVersion,doc.Size
FROM AllDocs doc WITH(NOLOCK)
WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId =0x AND doc.DirName in ( @DirName )
";

        public const string SingleAttachmentsForCommon =
@"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.TimeLastModified,doc.UIVersion,doc.Size
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl AND (CASE WHEN ISNUMERIC(o.LeafName)=1 THEN CAST(o.LeafName as INT) ELSE Null END)=@ItemId";

        public const string AllItemAndVersionForCommon =
    @"
SELECT doc.Id,doc.LeafName,doc.DoclibRowId,doc.IsCurrentVersion,doc.Type,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified
,doc.UIVersion as UIVersion,doc.Level
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId =0x AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 and data.tp_Level=doc.Level 
@WHERE
UNION ALL
SELECT doc.Id,doc.LeafName,doc.DoclibRowId,CONVERT(bit,0),doc.Type,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,docver.UIVersion as UIVersion,docver.Level
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=docver.UIVersion AND data.tp_Level=docver.Level 
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";

        public const string AllItemsInDocs =
@"
SELECT doc.Id,doc.LeafName,doc.DoclibRowId,doc.IsCurrentVersion,doc.Type,doc.TimeLastModified as TimeLastModified
,doc.UIVersion as UIVersion,doc.Level,doc.HasStream,2 AS QueryType,NULL AS tp_Guid,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK)
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";

        public const string AllItemsInVersion =
@"
SELECT docver.UIVersion as UIVersion,docver.Level,docver.Size,docver.InternalVersion,NULL as TimeLastModified,CAST( 0 AS BIT) as IsCurrentVersion
FROM AllDocVersions AS docver WITH(NOLOCK) 
@WHERE
";
        public const string AllItemsInUserData =
@"
SELECT data.tp_Modified ,data.tp_GUID
FROM AllUserData AS data WITH(NOLOCK)
@WHERE
";

        public const string WebFullUrlById = @"SELECT FullUrl FROM Webs WITH(NOLOCK) WHERE Id=@webId";

        public const string WebFullUrlByIdForSP1 = @"SELECT FullUrl FROM AllWebs WITH(NOLOCK) WHERE SiteId=@siteId AND Id=@webId AND DeleteTransactionId=0x";

        public const string WebRootFolder =
@"
SELECT @Column,doc.DirName,doc.Level
FROM AllDocs As doc WITH(NOLOCK)
WHERE SiteId=@SiteId AND 
DeleteTransactionId=0x AND
DirName=@DirName AND
LeafName=@LeafName";

        public const string ListRootFolder =
@"
SELECT @Column,doc.DirName,doc.Level,al.tp_MaxMajorwithMinorVersionCount FROM AllLists al WITH(NOLOCK)
INNER JOIN AllDocs doc WITH(NOLOCK) ON doc.Id=al.tp_RootFolder AND doc.Level<=255
WHERE al.tp_WebId=@WebId AND al.tp_Id=@ListId";

        public const string ListViewChanged =
  @"
SELECT wp.tp_ID,ISNULL(wp.tp_Flags,0), wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_PageUrlID,wp.tp_UserID,
ec.EventType,ec.Guid0,ec.EventTime
FROM EventCache ec WITH(NOLOCK) 
LEFT JOIN AllWebParts wp WITH(NOLOCK) ON wp.tp_SiteId=@siteId AND wp.tp_ListId=@ListId AND ec.Guid0 = wp.tp_ID AND (wp.tp_IsCurrentVersion=0 OR wp.tp_IsCurrentVersion=1)
WHERE ec.EventTime <= @endTime AND ec.EventTime >= @startTime AND ec.SiteId=@siteId AND ec.WebId=@webId AND 
ec.ListId=@ListId AND ec.ObjectType = 4096 ORDER BY EventTime";

        public const string ListViewChanged07 =
  @"
SELECT wp.tp_ID,ISNULL(wp.tp_Flags,0), wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_PageUrlID,wp.tp_UserID,
ec.EventType,ec.Guid0,ec.EventTime
FROM EventCache ec WITH(NOLOCK) 
LEFT JOIN WebParts wp WITH(NOLOCK) ON wp.tp_SiteId=@siteId AND wp.tp_ListId=@ListId AND ec.Guid0 = wp.tp_ID
WHERE ec.EventTime <= @endTime AND ec.EventTime >= @startTime AND ec.SiteId=@siteId AND ec.WebId=@webId AND 
ec.ListId=@ListId AND ec.ObjectType = 4096 ORDER BY EventTime";

        public const string ListViews =
            @"
SELECT wp.tp_ID,ISNULL(wp.tp_Flags,0), wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_PageUrlID,wp.tp_UserID
FROM AllWebParts wp WITH(NOLOCK) 
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_IsCurrentVersion=1 AND tp_level=1 AND (tp_Type=0 OR tp_Type=1) ";

        public const string ListViews07 =
            @"
SELECT wp.tp_ID,ISNULL(wp.tp_Flags,0), wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_PageUrlID,wp.tp_UserID
FROM WebParts wp WITH(NOLOCK) 
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_level=1 AND (tp_Type=0 OR tp_Type=1) ";

        public const string SiteChanged =
    @"
SELECT EventTime,EventType,ObjectType FROM EventCache WITH(NOLOCK)
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND ObjectType=8 ORDER BY EventTime";

        public const string SiteCollectionChangedWithUserAndGroup =
@"
SELECT EventType,ObjectType FROM EventCache WITH(NOLOCK)
WHERE EventCache.SiteId=@siteId AND EventTime <= @endTime AND EventTime >= @startTime  AND  ObjectType in (8, 256, 128) ORDER BY EventTime
";


        public const string RootWeb = "SELECT Id, FullUrl, Title FROM Webs WITH(NOLOCK) WHERE SiteId = @SiteId AND ParentWebId IS NULL";

        public const string RootWebForSP1 = "SELECT Id, FullUrl, Title, DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND ParentWebId IS NULL AND DeleteTransactionId = 0x";

        public const string SubWebs = "SELECT Id, FullUrl, Title,ParentWebId FROM Webs WITH(NOLOCK) WHERE SiteId = @SiteId AND (ParentWebId=@ParentId OR  ParentWebId IS NULL) ORDER BY FullUrl";

        public const string SubWebsForSP1 = "SELECT Id, FullUrl, Title,ParentWebId,DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND (ParentWebId=@ParentId OR  ParentWebId IS NULL) AND DeleteTransactionId = 0x ORDER BY FullUrl";

        public const string SubWebsForSP1WithRecycleBin = "SELECT Id, FullUrl, Title,ParentWebId,DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND (ParentWebId=@ParentId OR  ParentWebId IS NULL) ORDER BY FullUrl";

        public const string Web = "SELECT Id, FullUrl, Title,ParentWebId FROM Webs WITH(NOLOCK) WHERE SiteId = @SiteId AND (FullUrl=@FullUrl OR ParentWebId IS NULL) ORDER BY FullUrl";

        public const string WebForSP1 = "SELECT Id, FullUrl, Title,ParentWebId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND (FullUrl=@FullUrl OR ParentWebId IS NULL) AND DeleteTransactionId = 0x ORDER BY FullUrl";

        public const string Webs = @"SELECT Id, FullUrl, Title,ParentWebId FROM Webs WITH(NOLOCK) WHERE SiteId = @SiteId ORDER BY FullUrl";

        public const string WebsForSP1 = @"SELECT Id, FullUrl, Title,ParentWebId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND DeleteTransactionId = 0x ORDER BY FullUrl";

        /// <summary>
        /// 添加Root Folder的检查，还有List Is Not Null的过滤。
        /// 
        /// EventTime, EventType, ObjectType, WebId, FullUrl, Title, ParentWebId, Webs.Id, ItemFullURL, int0, int1, ItemName, WebRootFolder
        /// </summary>
        public const string WebChanged =
            @"
SELECT distinct '2010/01/01 1:01:01' as EventTime,2 as EventType,16 as ObjectType,WebId,FullUrl,Title,ParentWebId,Webs.Id,NULL as ItemFullUrl,NULL as int0,NULL as int1,NULL as ItemName
,(0) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN Webs WITH(NOLOCK) ON Webs.Id=EventCache.WebId
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND EventCache.WebId IS NOT NULL AND EventCache.ListId IS NOT NULL 

union

SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,Webs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName
,( case when ObjectType=32 and FullUrl=ItemFullUrl then 1 else 0 end) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN Webs WITH(NOLOCK) ON Webs.Id=EventCache.WebId
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND EventCache.WebId IS NOT NULL AND EventCache.ListId IS NULL";
        //             @"
        //SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,Webs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName
        //FROM EventCache WITH(NOLOCK) LEFT JOIN Webs WITH(NOLOCK) ON Webs.Id=EventCache.WebId
        //WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL ORDER BY EventTime";
        /// <summary>
        /// 添加Root Folder的检查，过滤没必要的记录
        /// 
        /// EventTime, EventType, ObjectType, WebId, FullUrl, Title, ParentWebId, Webs.Id, ItemFullURL, int0, int1, ItemName, WebRootFolder
        /// </summary>
        public const string WebChangedForSP1 =
            @"
SELECT distinct '2010/01/01 1:01:01' as EventTime,2 as EventType,16 as ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,NULL as ItemFullUrl,NULL as int0,NULL as int1,NULL as ItemName
,(0) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL AND EventCache.ListId IS NOT NULL

union 

SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName
,( case when ObjectType=32 and FullUrl=ItemFullUrl then 1 else 0 end) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL AND EventCache.ListId IS NULL
";
        //             @"
        //SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName
        //FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
        //WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL ORDER BY EventTime";

        public const string SiteSecurityChanged =
    @"
SELECT EventTime,ItemId,int0,ItemName,EventType,ObjectType FROM EventCache WITH(NOLOCK)
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@siteId AND objectType IN (256,128) ORDER BY EventTime";

        public const string SiteDeleted = @"SELECT EventTime,SiteId FROM EventCache WITH(NOLOCK)
WHERE EventTime <= @endTime AND EventTime >= @startTime AND objectType = 8 AND EventType=16384";

        public const string WebSecurityChanged =
            @"
SELECT EventType,ObjectType,int0,int1,Guid0,EventTime FROM EventCache WITH(NOLOCK)
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@siteId AND WebId=@webId AND ObjectType=4 AND EventType in(524288,33554432,786432,41943040,262144,8388608,16777216) ORDER BY EventTime";

        public const string ListIdByItem =
            @"SELECT doc.ListId FROM AllDocs AS doc WITH(NOLOCK) 
WHERE doc.SiteId=@SiteId AND doc.DeleteTransactionId=0x AND doc.DirName=@DirName AND doc.LeafName=@LeafName";

        public const string ListById =
           @"
SELECT tp_Title,tp_RootFolder,tp_BaseType,tp_Flags,tp_ServerTemplate,tp_WebId,tp_MaxMajorwithMinorVersionCount
FROM AllLists WITH(NOLOCK) WHERE tp_ID=@ListId";
        public const string ListByIdSP2013 =
           @"
SELECT tp_Title,tp_RootFolder,tp_BaseType,tp_Flags,tp_ServerTemplate,tp_WebId,tp_MaxMajorwithMinorVersionCount
FROM AllLists WITH(NOLOCK) WHERE tp_ID=@ListId AND tp_SiteId=@SiteId";

        public const string ListWithRootFolderById =
           @"
SELECT tp_Title,tp_RootFolder,tp_BaseType,tp_Flags,tp_ServerTemplate,tp_MaxMajorwithMinorVersionCount,ad.DirName, ad.LeafName
FROM AllLists as al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) 
ON ad.DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND Level=1
and al.tp_webId = @WebId and al.tp_Id = @ListId";


        public const string Lists =
            @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate,al.tp_DeleteTransactionId,al.tp_Fields 
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_WebId=@WebId AND al.tp_DeleteTransactionId=0x ORDER BY al.tp_Title";

        public const string ListsWithRecycleBin =
            @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate,al.tp_DeleteTransactionId,al.tp_Fields
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_WebId=@WebId  ORDER BY al.tp_Title";

        public const string ListChanged =
             @"
SELECT EventType,ObjectType,EventCache.ListId as ecListId,ModifiedBy,EventTime,ItemFullUrl,
al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,al.tp_ServerTemplate,ad.DirName+'/'+ad.LeafName as RootFolderUrl,EventCache.Int0,EventCache.Int1
FROM EventCache WITH(NOLOCK)
LEFT JOIN AllLists AS al WITH(NOLOCK) ON al.tp_WebId=@WebId AND al.tp_ID=EventCache.ListId 
LEFT JOIN AllDocs AS ad WITH(NOLOCK) ON ad.Id=al.tp_RootFolder AND ad.Level<=255
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND EventCache.WebId=@webId AND (EventCache.ListId IS NOT NULL OR ObjectType IN (16,32)) ORDER BY EventTime";

        public const string ListChangedEvent =
            @"
SELECT EventType,ObjectType,ListId,ModifiedBy,EventTime,ItemFullUrl,EventCache.Int0,EventCache.Int1,DocId, ItemId
FROM EventCache WITH(NOLOCK)
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@siteId AND WebId=@webId ORDER BY EventTime";

        /// <summary>
        /// 如果只修改View上的webpart，包括添加web part，则不会有list id，所以需要反向查询下对应的list，来捕获该事件
        /// </summary>
        public const string ListViewWebPartChangedEvent =
            @"
select distinct listid from alldocs WITH(NOLOCK) 
where id in 
( 
select distinct DocId from EventCache WITH(NOLOCK) 
where EventTime <= @endTime AND EventTime >= @startTime 
AND SiteId=@siteId AND WebId=@webId 
and ListId is NULL 
and ItemId is null and DocId is not null and ObjectType=16
) 
and Level=1
";

        public const string WebContentTypesUseWebFullUrl =
            @"
SELECT Webs.FullUrl
FROM Webs with(nolock)
WHERE Webs.Id = @WebId
";

        public const string WebContentTypesUseWebFullUrlSP1 =
            @"
SELECT AllWebs.FullUrl
FROM ALLWebs with(nolock)
WHERE AllWebs.SiteId = @SiteId AND AllWebs.Id = @WebId AND AllWebs.DeleteTransactionId=0x
";

        public const string WebContentTypes =
    @"
SELECT ContentTypes.ContentTypeId,ContentTypes.Definition,ContentTypes.ResourceDir,ContentTypes.Scope 
FROM ContentTypes with(nolock) 
WHERE ContentTypes.SiteId=@SiteId AND ContentTypes.Class=1 AND ContentTypes.Scope = @WebFullUrl";

        public const string WebContentTypesForSP1 =
    @"
SELECT ContentTypes.ContentTypeId,ContentTypes.Definition,ContentTypes.ResourceDir,ContentTypes.Scope 
FROM ContentTypes with(nolock)
WHERE ContentTypes.SiteId=@SiteId AND ContentTypes.Class=1 AND ContentTypes.Scope = @WebFullUrl AND ContentTypes.DeleteTransactionId = 0x";

        public const string WebContentTypesFast =
        @"
SELECT ContentTypes.ContentTypeId,ContentTypes.Definition,ContentTypes.ResourceDir,ContentTypes.Scope 
FROM ContentTypes with(nolock) 
WHERE ContentTypes.SiteId=@SiteId AND ContentTypes.Class=1 AND ContentTypes.Scope=@FullUrl";

        public const string WebContentTypesFastForSP1 =
        @"
SELECT ContentTypes.ContentTypeId,ContentTypes.Definition,ContentTypes.ResourceDir,ContentTypes.Scope 
FROM ContentTypes with(nolock) 
WHERE ContentTypes.SiteId=@SiteId AND ContentTypes.Class=1 AND ContentTypes.Scope=@FullUrl AND ContentTypes.DeleteTransactionId=0x";

        #region ContentType IB
        public const string WebContentTypeChangesInEventCache =
     @"
SELECT ec.EventType,ec.ObjectType,ec.ModifiedBy,ec.ContentTypeId,ec.EventTime
FROM EventCache ec WITH(NOLOCK)
Where
ec.EventTime <= @endTime  AND ec.EventTime >= @startTime AND ec.SiteId=@siteId AND ec.WebId=@webId AND ec.ListId IS NULL AND ec.ObjectType IN (1024,512) 
ORDER BY EventTime";

        public const string WebContentTypesForIBForSP1 =
            @"
SELECT ct.ContentTypeId, ct.Scope,ct.Version,ct.Definition,ct.ResourceDir,ct.SolutionId,ct.IsFromFeature,ct.ContentTypeId
FROM
ContentTypes ct WITH(NOLOCK) 
WHERE
ct.SiteId=@siteId AND ct.Class=1 AND  ct.ContentTypeId in ({0}) AND ct.DeleteTransactionId=0x ";

        public const string WebContentTypesForIBForRTM =
    @"
SELECT ct.ContentTypeId, ct.Scope,ct.Version,ct.Definition,ct.ResourceDir,ct.SolutionId,ct.IsFromFeature,ct.ContentTypeId
FROM
ContentTypes ct WITH(NOLOCK) 
WHERE
ct.SiteId=@siteId AND ct.Class=1 AND  ct.ContentTypeId in ({0})";
        #endregion

        public const string ListAlertChanged =
     @"
SELECT EventCache.EventType,ModifiedBy,Guid0,EventTime,ISNULL(imm.Properties,''),ISNULL(sche.Properties,''),imm.Id,sche.Id
FROM EventCache WITH(NOLOCK) 
LEFT JOIN ImmedSubscriptions imm WITH(NOLOCK)  ON imm.SiteId=@siteId AND imm.ListId=@listId AND EventCache.Guid0=imm.Id 
LEFT JOIN SchedSubscriptions sche WITH(NOLOCK)  ON sche.SiteId=@siteId AND sche.ListId=@listId AND EventCache.Guid0=sche.Id
WHERE EventTime <= @endTime and EventTime >= @startTime AND EventCache.SiteId=@siteId AND EventCache.WebId=@webId AND EventCache.ListId=@listId 
AND EventCache.ItemId IS NULL AND ObjectType=64 ORDER BY EventTime";

        public const string ListSecurityChanged =
     @"
SELECT EventType,ObjectType,int0,int1,Guid0,EventTime
FROM EventCache WITH(NOLOCK)  
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@siteId AND WebId=@webId 
AND ListId=@ListId AND ObjectType=2 AND EventType IN (524288,33554432,786432,41943040) ORDER BY EventTime";

        public const string ListContentTypeChanged =
             @"
SELECT EventType,ObjectType,ModifiedBy,ContentTypeId,EventTime
FROM EventCache WITH(NOLOCK) 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@siteId AND WebId=@webId AND ListId=@ListId 
AND (EventType IN (268435456,536870912) OR ObjectType=512) ORDER BY EventTime";

        /// <summary>
        /// Add SiteId to improve performance.
        /// </summary>
        public const string ItemSecurityChanged =
    @"
SELECT EventType,ObjectType,int0,int1,Guid0,EventTime
FROM EventCache WITH(NOLOCK) 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND WebId=@webId AND ListId=@listId 
AND ItemId=@itemId AND EventType IN (524288,33554432,786432,41943040) ORDER BY EventTime";


        public const string FolderAlerts =
            @"
select Id,Properties from ImmedSubscriptions with(nolock)
where SiteId=@siteId and ListId=@listId and Id in(@WHERE)
UNION All
select Id,Properties from SchedSubscriptions with(nolock)
where SiteId=@siteId and ListId=@listId and Id in(@WHERE)";

        public const string GetAllPagesUnderFolder_Select_AllDocs =
            @"SELECT Id,DirName,LeafName,Type from AllDocs with(nolock) 
              WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND DirName LIKE @parentUrl 
                    AND leafname LIKE '%.aspx' AND IsCurrentVersion=1 AND Type=0";

        #endregion

        #region Item

        public const string AllItemsAndVersionsForItem =
    @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,doc.Level,data.tp_GUID
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId =0x AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 AND data.tp_Level=doc.Level AND data.tp_RowOrdinal=0
@WHERE
UNION ALL
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,docver.Level,data.tp_GUID
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId = 0x AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND docver.UIVersion=data.tp_CalculatedVersion AND data.tp_Level=docver.Level AND data.tp_RowOrdinal=0 
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string AllVersionsForCommon =
@"
SELECT data.tp_Id as DoclibRowId,data.tp_DocId,data.tp_UIVersion as UIVersion, data.tp_Modified as TimeLastModified ,data.tp_Level as Level,data.tp_Guid,data.tp_IsCurrent as IsCurrentVersion, data.tp_DeleteTransactionId as DeleteTransactionId,data.tp_SiteId,data.tp_CalculatedVersion,data.tp_IsCurrentVersion,data.tp_GUID
FROM AllUserData AS data  WITH(INDEX (ALLUSERDATA_PARENTID) NOLOCK)
@WHERE
ORDER BY data.tp_DocId, data.tp_UIVersion DESC";//需要注意data.tp_IsCurrent，data.tp_CalculatedVersion均为bit类型

        public const string AllVersionsForReplicator =
@"
SELECT data.tp_Id as DoclibRowId,data.tp_DocId,data.tp_UIVersion as UIVersion, data.tp_Modified as TimeLastModified ,data.tp_Level as Level,data.tp_Guid,data.tp_IsCurrent as IsCurrentVersion, data.tp_DeleteTransactionId as DeleteTransactionId,data.tp_SiteId,data.tp_CalculatedVersion,data.tp_IsCurrentVersion,data.tp_GUID,tp_Size as Size
FROM AllUserData AS data  WITH(INDEX (ALLUSERDATA_PARENTID) NOLOCK)
@WHERE
ORDER BY data.tp_DocId, data.tp_UIVersion DESC";//需要注意data.tp_IsCurrent，data.tp_CalculatedVersion均为bit类型

        public const string AllVersionsFor07 =
@"
SELECT data.tp_Id,data.tp_UIVersion as UIVersion, data.tp_Modified as TimeLastModified ,data.tp_Level as Level,data.tp_Guid,data.tp_IsCurrentVersion as IsCurrentVersion,data.tp_DeleteTransactionId as DeleteTransactionId
FROM AllUserData AS data  WITH(NOLOCK)
@WHERE
ORDER BY data.tp_Id, data.tp_UIVersion DESC";

        public const string AllItemsAndVersionsForItemIB =
    @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,doc.Level,data.tp_GUID
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 AND data.tp_Level=doc.Level AND data.tp_RowOrdinal=0
@WHERE
UNION ALL
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,docver.Level,data.tp_GUID
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId =doc.DeleteTransactionId AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND docver.UIVersion=data.tp_CalculatedVersion AND data.tp_Level=docver.Level AND data.tp_RowOrdinal=0 
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        #endregion

        #region PlatformRecovery

        #endregion

        #region Archive

        public const string AllAttachmentsForArchive =
   @"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.Level,doc.UIVersion,doc.ParentId
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl";

        public const string SingleAttachmentsForArcjover =
    @"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.Level,doc.UIVersion,doc.ParentId
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl AND (CASE WHEN ISNUMERIC(o.LeafName)=1 THEN CAST(o.LeafName as INT) ELSE Null END)=@ItemId
";

        #endregion

        #region Externder

        public const string AllAttachmentsForExternder =
            @"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Size,DATALENGTH(stream.Content)AS ContentLength,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content,
stream.RbsId 
FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
LEFT OUTER JOIN AllDocStreams AS stream ON doc.SiteId = stream.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl";

        public const string AllAttachmentsForExternderWithRecycleBin =
            @"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Size,DATALENGTH(stream.Content)AS ContentLength,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content,
stream.RbsId,doc.DeleteTransactionId 
FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x)  AND doc.ParentId=o.Id
LEFT OUTER JOIN AllDocStreams AS stream ON doc.SiteId = stream.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
WHERE o.SiteId=@SiteId AND (o.DeleteTransactionId=0x OR o.DeleteTransactionId<>0x) AND o.DirName=@AttachmentUrl ";

        public const string SingleAttachmentsForExternder =
    @"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Size,DATALENGTH(stream.Content)AS ContentLength,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content,
stream.RbsId 
FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
LEFT OUTER JOIN AllDocStreams AS stream ON doc.SiteId = stream.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl AND (CASE WHEN ISNUMERIC(o.LeafName)=1 THEN CAST(o.LeafName as INT) ELSE Null END)=@ItemId
";

        public const string AttachmentStubExtenderIB =
            @"SELECT NULL AS Content,stream.RbsId
FROM AllDocStreams AS stream
@WHERE";

        public const string AllAttachmentsStubInfoForExtender =
            @"
SELECT doc.Id,doc.DocFlags,stream.RbsId,stream.Content
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
WHERE doc.SiteId=@SiteId AND doc.DirName=@DirName AND doc.DeleteTransactionId=0x AND doc.Level=1 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string AllItemAndVersionsStubInfo =
@"
SELECT  doc.UIVersion, doc.DocFlags, stream.RbsId,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHEREAllDocs
UNION ALL
SELECT  docver.UIVersion, docver.DocFlags, stream.RbsId,
CASE WHEN docver.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocVersions AS docver with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND docver.Id = stream.Id AND docver.InternalVersion = stream.InternalVersion
@WHEREAllDocVersions
";

        public const string AllItemsAndVersionsForExtender =
@"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion ,
doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,doc.Size,NULL AS Content,stream.RbsId,doc.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=doc.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHERE
UNION ALL
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
docver.DocFlags,CAST(COALESCE(stream.InternalVersion, 0) AS BIT) HasStream,docver.Level,3 AS QueryType,docver.Size,NULL AS Content,stream.RbsId,docver.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=docver.SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string AllItemsAndVersionsForExtenderWithRecycleBin =
            @"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion ,
doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,doc.Size,NULL AS Content,stream.RbsId,doc.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=doc.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHERE
UNION ALL
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
docver.DocFlags,CAST(COALESCE(stream.InternalVersion, 0) AS BIT) HasStream,docver.Level,3 AS QueryType,docver.Size,NULL AS Content,stream.RbsId,docver.DeleteTransactionId,doc.id as tp_docid
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND (docver.DeleteTransactionId=0x OR docver.DeleteTransactionId<>0x) AND doc.IsCurrentVersion=1
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=docver.SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";


        public const string AllItemsInDocsForExtender =
            @"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion ,
doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,doc.Size,NULL AS Content,stream.RbsId,doc.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=doc.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHERE
ORDER BY DocLibRowId,LeafName DESC
";

        public const string AllDocValueForEventCache_Extender = "SELECT " + CommonColumn.AllDocsCommonColumns_BigIntSize + ",doc.Level,doc.CheckoutUserId,doc.DocFlags,doc.IsCurrentVersion,doc.ParentId,doc.ListId,doc.HasStream,2 AS QueryType,NULL AS Content,stream.RbsId" + @"
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=doc.SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHERE
";

        public const string AllVersionsForExtender =
            @"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
docver.DocFlags,CONVERT(int,CAST(COALESCE(stream.InternalVersion, 0) AS BIT)) HasStream,docver.Level,3 AS QueryType,docver.Size,NULL AS Content,stream.RbsId,docver.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=docver.SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";

        public const string AllVersionsForExtenderWithRecycleBin =
            @"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
docver.DocFlags,CONVERT(int,CAST(COALESCE(stream.InternalVersion, 0) AS BIT)) HasStream,docver.Level,3 AS QueryType,docver.Size,NULL AS Content,stream.RbsId,docver.DeleteTransactionId,doc.id as tp_DocId
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND (docver.DeleteTransactionId=0x OR docver.DeleteTransactionId<>0x) AND doc.IsCurrentVersion=1
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=docver.SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";


        public const string ItemStubsByIdsCammandLine =
@"
SELECT  doc.Id, doc.DocFlags, stream.RbsId,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHEREAllDocs
";


        public const string StubAllItemAndVersions =
@"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion ,
doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,doc.Size,stream.RbsId,doc.TimeLastModified as TimeLastModified,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHEREAllDocs
UNION ALL
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
docver.DocFlags,doc.HasStream,docver.Level,3 AS QueryType,doc.Size,stream.RbsId,doc.TimeLastModified as TimeLastModified,
CASE WHEN docver.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHEREAllDocVersions
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string StubAllItemAndVersionsWithRecycleBin =
@"
SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion, doc.DeleteTransactionId ,
doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,doc.Size,stream.RbsId,doc.TimeLastModified as TimeLastModified,
CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocs AS doc with(nolock)
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND doc.Id = stream.Id AND doc.InternalVersion = stream.InternalVersion
@WHEREAllDocs
UNION ALL
SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion, docver.DeleteTransactionId ,
docver.DocFlags,doc.HasStream,docver.Level,3 AS QueryType,doc.Size,stream.RbsId,doc.TimeLastModified as TimeLastModified,
CASE WHEN docver.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
FROM AllDocVersions AS docver with(nolock) 
INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND (docver.DeleteTransactionId=0x OR docver.DeleteTransactionId<>0x) AND doc.IsCurrentVersion=1
LEFT JOIN AllDocStreams AS stream with(nolock) ON stream.SiteId=@SiteId AND stream.Id=docver.Id AND stream.InternalVersion=docver.InternalVersion
@WHEREAllDocVersions
ORDER BY DocLibRowId,LeafName,UIVersion DESC";
        public const string StubFilesInFolderCount = @" 
WITH DocsBlob(Id)
AS
(
--Get All Stubs in AllDocs table
SELECT docs.Id FROM AllDocs docs WITH (NOLOCK)
INNER JOIN AllDocStreams docStream WITH (NOLOCK) ON docs.SiteId = docStream.SiteId AND docs.Id = docStream.Id AND docs.InternalVersion = docStream.InternalVersion
AND docs.Type <= 0 AND docs.ParentId = @ParentId AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
AND (docs.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
AND docs.IsCurrentVersion <= 1
WHERE docStream.SiteId = @SiteId
UNION ALL
--Get All Stubs in AllDocVersions table
SELECT Versions.Id FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId 
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
INNER JOIN AllDocStreams docStream WITH (NOLOCK) on docStream.SiteId = versions.SiteId AND docStream.Id = versions.Id AND docStream.InternalVersion = versions.InternalVersion
AND (versions.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
WHERE versions.SiteId = @SiteId
)
SELECT COUNT(Id) FROM DocsBlob";

        public const string StubFilesInFolderCountWithRecycleBin = @" 
WITH DocsBlob(Id)
AS
(
--Get All Stubs in AllDocs table
SELECT docs.Id FROM AllDocs docs WITH (NOLOCK)
INNER JOIN AllDocStreams docStream WITH (NOLOCK) ON docs.SiteId = docStream.SiteId AND docs.Id = docStream.Id AND docs.InternalVersion = docStream.InternalVersion
AND docs.Type <= 0 AND docs.ParentId = @ParentId AND docs.Level <= 255
AND (docs.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
AND docs.IsCurrentVersion <= 1
WHERE docStream.SiteId = @SiteId
UNION ALL
--Get All Stubs in AllDocVersions table
SELECT Versions.Id FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId 
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion AND docs.Type <= 0
INNER JOIN AllDocStreams docStream WITH (NOLOCK) on docStream.SiteId = versions.SiteId AND docStream.Id = versions.Id AND docStream.InternalVersion = versions.InternalVersion
AND (versions.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
WHERE versions.SiteId = @SiteId
)
SELECT COUNT(Id) FROM DocsBlob";

        public const string ItemStubAttachmentsInFolder = @"
SELECT COUNT(ds.Id) FROM AllDocStreams ds WITH(NOLOCK)
INNER JOIN AllDocs att WITH(NOLOCK) ON ds.Id = att.Id AND ds.SiteId = att.SiteId AND ds.InternalVersion = att.InternalVersion
AND (att.DocFlags & 65536 = 65536 OR (ds.Content IS NULL AND ds.RbsId IS NOT NULL)) AND att.WebId = @WebId AND att.ListId = @ListId
AND att.Level <= 1 AND att.Type <= 0 AND att.DeleteTransactionId = 0x AND att.SiteId = @SiteId AND att.DoclibRowId IS NULL
AND att.IsCurrentVersion <= 1  
INNER JOIN AllDocs item WITH(NOLOCK) ON att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId
AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar) AND att.Level <= item.Level AND item.Type <= 1
AND item.IsCurrentVersion <= 1 AND item.DoclibRowId IS NOT NULL --AND item.Size <= 0
WHERE item.SiteId = @SiteId AND (item.ParentId = @ParentId OR item.Id=@ParentId) AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
AND (att.DocFlags&65536 = 65536 OR (ds.Content IS NULL AND ds.RbsId IS NOT NULL))";

        #endregion

        #region Replicator

        public const string AllItemsAndVersionsForReplicator =
            @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.DocFlags,doc.Size,
ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,data.tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 and data.tp_Level=doc.Level
@WHERE
UNION ALL
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.Level,doc.DocFlags,docver.Size,
ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,data.tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK) 
INNER JOIN AllDocVersions AS docver WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND docver.UIVersion=data.tp_CalculatedVersion AND docver.Level=data.tp_Level
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string AllItemsInDocsForReplicator =
            @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.DocFlags,doc.Size,
doc.TimeLastModified as TimeLastModified,NULL as tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK)
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";

        public const string AllItemsAndVersionsForReplicatorIB =
    @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.DocFlags,doc.Size,
ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,data.tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=doc.DeleteTransactionId AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 and data.tp_Level=doc.Level
@WHERE
UNION ALL
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.Level,doc.DocFlags,docver.Size,
ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,data.tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK) 
INNER JOIN AllDocVersions AS docver WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=doc.DeleteTransactionId AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND docver.UIVersion=data.tp_CalculatedVersion AND docver.Level=data.tp_Level
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        /// <summary>
        /// 只需要取得当前Version，不需要获取所有Version的Webpart，原因是history的web part对于我们来说没有太大的用途。
        /// </summary>
        public const string ItemWebParts =
             @"SELECT tp_ID,tp_Flags,tp_DisplayName,tp_PartOrder,tp_ZoneID,tp_IsIncluded,tp_View,tp_AllUsersProperties,tp_PerUserProperties FROM AllWebParts with(nolock) 
WHERE tp_SiteId=@SiteId AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@DocId";

        /// <summary>
        /// Add SiteId to improve the performance.
        /// </summary>
        public const string ListContentTypes = "SELECT tp_ContentTypes FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@webId AND tp_ID=@listId";

        public const string ItemLastModifiedTimeWithDoclibRowId = @"select tp_Modified from AllUserData With(nolock) where tp_SiteId=@SiteId and tp_DocId=@Id and tp_DeleteTransactionId=0x and tp_IsCurrent=1";

        public const string ItemLastModifiedTimeWithoutDoclibRowId = @"select TimeLastModified from AllDocs With(nolock) where Id=@Id AND Level in (1, 2, 255) AND IsCurrentVersion=1 And DeleteTransactionId = 0x";

        public const string ItemLastModifiedTimeWithoutDoclibRowId13 = @"select TimeLastModified from AllDocs With(nolock) where SiteId=@SiteId AND Id=@Id AND Level in (1, 2, 255) AND IsCurrentVersion=1 And DeleteTransactionId = 0x";

        public const string ItemLastModifiedTimeWithDirName = @"select Id,TimeLastModified,DocLibRowId from AllDocs With(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and DirName=@DirName and LeafName=@LeafName and level in (1, 2, 255) AND IsCurrentVersion=1 AND Type = 0";

        public const string ItemLastModifiedTimeByListIdAndDoclibRowId = @"select tp_Modified from AllUserData With(nolock) where tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_Id=@Id AND tp_CalculatedVersion=0 AND tp_RowOrdinal=0 And tp_IsCurrent=1";

        public const string ItemLastModifiedTimeByListIdAndDoclibRowId13 = @"select tp_Modified from AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_Id=@Id AND tp_CalculatedVersion=0 AND tp_RowOrdinal=0 And tp_IsCurrent=1";

        public const string ItemLastModifiedTimeWithtpGuid = @"select tp_DocId,tp_Modified from AllUserData With(nolock) where tp_SiteId=@SiteId and tp_Guid=@tp_Guid and tp_DeleteTransactionId=0x and tp_IsCurrent=1";

        public const string IsHaveSameNameByTpGuid = "Select count (*) from  AllUserData with(nolock) Where tp_ListId=@ListId and tp_Guid=@tp_Guid and tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0";

        public const string IsHaveSameNameByLeafName = "SELECT Count(*) FROM AllDocs with(nolock) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND DirName=@dirName AND LeafName=@LeafName";

        public const string ListItemGuid = @"SELECT tp_DocId FROM AllUserData With(NOLOCK) WHERE tp_ListId=@listId AND tp_GUID=@tp_Guid AND tp_DeleteTransactionId = 0x";

        public const string ItemVersions =
            @"SELECT DISTINCT tp_UIVersion, tp_Modified, tp_IsCurrent, tp_GUID, tp_ID ,tp_UIVersionString,tp_Level,tp_Size,tp_IsCurrentVersion
FROM AllUserData With(NOLOCK) WHERE [tp_DeleteTransactionId]=0x AND [tp_ListId]=@ListId AND ([tp_IsCurrentVersion]=0 OR [tp_IsCurrentVersion]=1) AND [tp_id]=@docLibId And [tp_RowOrdinal]=0 ORDER BY tp_UIVersion DESC";

        public const string ItemSizeAndParnetId = @"SELECT Size,ParentId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId And DeleteTransactionId = 0x And Id=@DocId AND Level=@Level AND IsCurrentVersion=1";

        public const string AuthorAndEditor =
            @"SELECT tp_Author,tp_Editor FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x 
AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId
AND tp_DocId=@DocId  AND tp_IsCurrent=1";

        public const string UserTitle =
            @"SELECT tp_Title FROM UserInfo WITH(NOLOCK) WHERE tp_SiteID=@SiteId AND tp_ID=@UserId";

        public const string ItemIdAndTPGUID =
            @"SELECT tp_DocId, tp_GUID FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId
And tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 And tp_ParentId=@ParentId
And tp_CalculatedVersion=0 And tp_RowOrdinal=0";

        public const string ItemIdAndType =
            @"SELECT Id, type FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId
And DeleteTransactionId=0x And ParentId=@ParentId";

        #endregion

        #region ContentManager

        public const string AllItemsAndVersionsForContentManager =
     @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.Type,doc.CheckoutUserId,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,doc.Size
FROM AllDocs AS doc WITH(NOLOCK)
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId =0x AND data.tp_IsCurrentVersion=1 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=0 and data.tp_Level=doc.Level AND data.tp_RowOrdinal=0
@WHERE
UNION All
SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.Level,doc.Type,doc.CheckoutUserId,ISNULL(data.tp_Modified,doc.TimeLastModified) as TimeLastModified,docver.Size
FROM AllDocs AS doc WITH(NOLOCK) 
INNER JOIN AllDocVersions AS docver WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId = 0x AND data.tp_IsCurrentVersion=0 AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=doc.Id AND data.tp_CalculatedVersion=docver.UIVersion AND data.tp_Level=docver.Level AND data.tp_RowOrdinal=0
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC";

        public const string AllItemsInDocsForContentManager =
            @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.Type,doc.CheckoutUserId,doc.TimeLastModified as TimeLastModified,doc.Size
FROM AllDocs AS doc WITH(NOLOCK)
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";
        #endregion


        public const string AllDocVersionsForSpecialLibrary =
            @"
SELECT avd.Id, avd.UIVersion
FROM AllDocVersions As avd WITH(NOLOCK)
@WHERE
ORDER BY avd.Id, avd.UIVersion DESC";

        public const string SpecifyViewDocIds =
            @"
SELECT tp_PageUrlID , tp_ID
FROM AllWebParts WITH(NOLOCK) 
WHERE AllWebParts.tp_SiteId=@SiteId AND (AllWebParts.tp_IsCurrentVersion=0x OR AllWebParts.tp_IsCurrentVersion<>0x) 
AND AllWebParts.tp_ID IN ( {0} )
";
        public const string SpecifyViewDocIdsFor07 =
            @"
SELECT tp_PageUrlID 
FROM WebParts WITH(NOLOCK) 
WHERE tp_SiteId=@SiteId AND tp_ID IN ( {0} )
";

    }
}
