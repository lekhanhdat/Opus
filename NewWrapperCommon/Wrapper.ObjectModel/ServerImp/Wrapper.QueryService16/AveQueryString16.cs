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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.QueryService
{
    public class AveQueryString16
    {
        public const string SingleAttachmentsForCommon =
@"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.TimeLastModified,doc.UIVersion,COALESCE(doc.Size ,doc.SizeWrite) as Size
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl AND (CASE WHEN ISNUMERIC(o.LeafName)=1 THEN CAST(o.LeafName as INT) ELSE Null END)=@ItemId";

        public const string Sp16SingleAttachmentForExternder =
            @"SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Level,COALESCE(doc.Size ,doc.SizeWrite) as Size
            FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
            ON doc.SiteId=o.SiteId
            AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
            WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl AND (CASE WHEN ISNUMERIC(o.LeafName)=1 THEN CAST(o.LeafName as INT) ELSE Null END)=@ItemId
            ";


        /// <summary>
        /// Only for FB,query all attachments.  SP 10,13  CM,GR,RP,PR item,SPM
        /// </summary>
        public const string AllAttachmentsForCommon =
@"
SELECT doc.Id,doc.DirName,doc.LeafName,doc.TimeLastModified,doc.UIVersion,COALESCE(doc.Size ,doc.SizeWrite) as Size
FROM AllDocs o WITH(NOLOCK) INNER JOIN AllDocs doc WITH(NOLOCK)
ON doc.SiteId=o.SiteId AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
WHERE o.SiteId=@SiteId AND o.DeleteTransactionId=0x AND o.DirName=@AttachmentUrl
";

        public const string Sp16AllAttachmentsForExternder =
            @"SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Level,COALESCE(doc.Size ,doc.SizeWrite) as Size
            FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
            ON doc.SiteId=o.SiteId
            AND doc.DeleteTransactionId=0x AND doc.ParentId=o.Id
            WHERE o.SiteId=@SiteId 
            AND o.DeleteTransactionId=0x
            AND o.DirName=@AttachmentUrl 
            ";

        public const string Sp16AllAttachmentsForExternderWithRecycleBin =
            @"SELECT doc.Id,doc.DirName,doc.LeafName,doc.ParentId,doc.DocFlags,doc.Level,doc.DeleteTransactionId,COALESCE(doc.Size ,doc.SizeWrite) as Size
            FROM AllDocs doc WITH(NOLOCK) INNER JOIN AllDocs o WITH(NOLOCK)
            ON doc.SiteId=o.SiteId AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x)  AND doc.ParentId=o.Id
            WHERE o.SiteId=@SiteId AND (o.DeleteTransactionId=0x OR o.DeleteTransactionId<>0x)  AND o.DirName=@AttachmentUrl ";

        
        public const string AllItemsInDocsForReplicator =
            @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.DocFlags,COALESCE(doc.Size ,doc.SizeWrite) as Size,
doc.TimeLastModified as TimeLastModified,NULL as tp_GUID,doc.CheckoutUserId
FROM AllDocs AS doc WITH(NOLOCK)
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";
        public const string AllItemsInDocsForContentManager =
            @"
SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.Level,doc.Type,doc.CheckoutUserId,doc.TimeLastModified as TimeLastModified,COALESCE(doc.Size ,doc.SizeWrite) as Size
FROM AllDocs AS doc WITH(NOLOCK)
@WHERE
ORDER BY DocLibRowId,LeafName,UIVersion DESC
";

        public const string Sp16AllItemsInDocsForExtender =
           @"SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,NULL AS Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,doc.DeleteTransactionId,doc.id as tp_docid,null
            FROM AllDocs AS doc WITH(NOLOCK)
            @WHERE
            ORDER BY LeafName,UIVersion DESC
";

        public const string Sp16AllDocValueForEventCache_Extender = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.CheckoutUserId,doc.DocFlags,doc.IsCurrentVersion,doc.ParentId,doc.ListId,doc.HasStream,2 AS QueryType,NULL AS Content,stream.RbsId" + @"
             FROM AllDocs AS doc WITH(NOLOCK)
            left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=doc.Level
            LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
            @WHERE
";

        public const string Sp16AllDocValueForEventCache_ReplicatorAndContentManager = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.CheckoutUserId,doc.DocFlags,doc.IsCurrentVersion,doc.ParentId,doc.ListId" + @"
FROM ALLDocs doc WITH(NOLOCK)
@WHERE
";

        public const string AllVersionsForExtenderWithRecycleBin =
          @"SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.DocFlags,CONVERT(int,docver.HasStream) AS HasStream,docver.Level,3 AS QueryType,NULL AS Content,COALESCE(docver.Size ,docver.SizeWrite) as Size,docver.DeleteTransactionId,doc.id as tp_docid
            FROM AllDocVersions AS docver WITH(NOLOCK) 
            INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND doc.IsCurrentVersion=1 
            left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0
            LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
            @WHERE
            ORDER BY LeafName,UIVersion DESC
";

        public const string AllVersionsForExtender =
          @"SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.DocFlags,CONVERT(int,docver.HasStream) AS HasStream,docver.Level,3 AS QueryType,NULL AS Content,COALESCE(docver.Size ,docver.SizeWrite) as Size,docver.DeleteTransactionId,doc.id as tp_docid,stream.RbsId
            FROM AllDocVersions AS docver WITH(NOLOCK) 
            INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
            left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0
            LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
            @WHERE
            ORDER BY LeafName,UIVersion DESC
";

        public const string Sp16AllItemsAndVersionsForExtender =
           @"SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,NULL AS Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,doc.DeleteTransactionId,doc.id as tp_docid,null
            FROM AllDocs AS doc WITH(NOLOCK)
            @WHERE
            UNION
            SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.DocFlags,docver.HasStream,docver.Level,3 AS QueryType,NULL AS Content,COALESCE(docver.Size ,docver.SizeWrite) as Size,docver.DeleteTransactionId,doc.id as tp_docid,null
            FROM AllDocVersions AS docver WITH(NOLOCK) 
            INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1
            @WHERE
            ORDER BY LeafName,UIVersion DESC";

        public const string Sp16AllItemsAndVersionsForExtenderWithRecycleBin =
           @"SELECT " + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,NULL AS Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,doc.DeleteTransactionId,doc.id as tp_docid,null
            FROM AllDocs AS doc WITH(NOLOCK)
            @WHERE
            UNION
            SELECT " + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,docver.DocFlags,docver.HasStream,docver.Level,3 AS QueryType,NULL AS Content,COALESCE(docver.Size ,docver.SizeWrite) as Size,docver.DeleteTransactionId,doc.id as tp_docid,null
            FROM AllDocVersions AS docver WITH(NOLOCK) 
            INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND doc.IsCurrentVersion=1
            @WHERE
            ORDER BY LeafName,UIVersion DESC";

        public const string Sp16AllItemAndVersionsStubInfo =
                @"SELECT doc.UIVersion,doc.DocFlags,stream.RbsId,
                CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
                FROM AllDocs AS doc with(nolock)
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=doc.Level
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocs
                UNION ALL
                SELECT docver.UIVersion,docver.DocFlags,stream.RbsId,
                CASE WHEN docver.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
                FROM AllDocVersions AS docver with(nolock)
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on docver.Id=DTStream.DocId and docver.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=docver.Level
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocVersions";

        public const string Sp16AllAttachmentsStubInfoForExtender =
            @"SELECT doc.Id,doc.DocFlags,stream.RbsId,stream.Content
            FROM AllDocs AS doc with(nolock)
            LEFT OUTER JOIN DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=1
            LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
            WHERE doc.SiteId=@SiteId AND doc.DirName=@DirName AND doc.DeleteTransactionId=0x AND doc.Level=1 AND (doc.DocFlags&65536<>0 OR (stream.Content is NULL and stream.RbsId is not NULL))";

        public const string Sp16ContentOrStub =
            @"select top 1 DocId from DocStreams where DocStreams.SiteId = @SiteId  and DocStreams.DocId =@DocId  
            and DocStreams.Content is null and DocStreams.RbsId is not null";

        public const string Sp13StubFilesInFolderCount = @" 
                WITH DocsBlob(Id,InternalVersion)
                AS
                (
                --Get All Stubs in AllDocs table
                SELECT DISTINCT(docs.Id),docs.InternalVersion FROM AllDocs docs WITH (NOLOCK)
                INNER join DocsToStreams As DTStream on docs.Id=DTStream.DocId and docs.SiteId=DTStream.SiteId and DTStream.Level=docs.Level
                INNER JOIN DocStreams AS stream ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                AND docs.Type <= 0 AND docs.ParentId = @ParentId 
                AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
                AND stream.Content IS NULL AND stream.RbsId IS NOT NULL
                AND docs.IsCurrentVersion <= 1
                AND DTStream.HistVersion=0
                WHERE stream.SiteId = @SiteId
                UNION ALL
                            --Get All Stubs in AllDocVersions table
                SELECT DISTINCT(Versions.Id),Versions.InternalVersion FROM AllDocVersions versions WITH (NOLOCK)
                INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId  
                AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
                AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
                INNER join DocsToStreams As DTStream on versions.ID=DTStream.DocId and versions.SiteId=DTStream.SiteId and  DTStream.Level=docs.Level
                INNER JOIN DocStreams stream  on stream.SiteId = DTStream.SiteId AND stream.DocId = DTStream.DocId  and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                AND stream.Content IS NULL AND stream.RbsId IS NOT NULL
                WHERE versions.SiteId = @SiteId
                AND DTStream.HistVersion<>0
                )
                SELECT COUNT(Id)FROM DocsBlob";

        public const string Sp13StubFilesInFolderCountWithRecycleBin = @" 
                WITH DocsBlob(Id,InternalVersion)
                AS
                (
                --Get All Stubs in AllDocs table
                SELECT DISTINCT(docs.Id),docs.InternalVersion FROM AllDocs docs WITH (NOLOCK)
                INNER join DocsToStreams As DTStream on docs.Id=DTStream.DocId and docs.SiteId=DTStream.SiteId and DTStream.Level=docs.Level
                INNER JOIN DocStreams AS stream ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                AND docs.Type <= 0 AND docs.ParentId = @ParentId 
                AND docs.Level <= 255
                AND stream.Content IS NULL AND stream.RbsId IS NOT NULL
                AND docs.IsCurrentVersion <= 1
                AND DTStream.HistVersion=0
                WHERE stream.SiteId = @SiteId
                UNION ALL
                            --Get All Stubs in AllDocVersions table
                SELECT DISTINCT(Versions.Id),Versions.InternalVersion FROM AllDocVersions versions WITH (NOLOCK)
                INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId  
                AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
                AND docs.Type <= 0
                INNER join DocsToStreams As DTStream on versions.ID=DTStream.DocId and versions.SiteId=DTStream.SiteId and  DTStream.Level=docs.Level
                INNER JOIN DocStreams stream  on stream.SiteId = DTStream.SiteId AND stream.DocId = DTStream.DocId  and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                AND stream.Content IS NULL AND stream.RbsId IS NOT NULL
                WHERE versions.SiteId = @SiteId
                AND DTStream.HistVersion<>0
                )
                SELECT COUNT(Id)FROM DocsBlob";

        public const string Sp13ItemStubAttachmentsInFolder = @"
            SELECT COUNT(distinct(ds.DocId)) FROM DocStreams ds
            inner join DocsToStreams Dts on ds.DocId=Dts.DocId and ds.SiteId=Dts.SiteId and ds.BSN=Dts.BSN and ds.Partition=Dts.Partition
            INNER JOIN AllDocs att WITH(NOLOCK) ON Dts.DocId = att.Id AND Dts.SiteId = att.SiteId AND Dts.HistVersion=0
            AND (ds.Content IS NULL AND ds.RbsId IS NOT NULL) AND att.WebId = @WebId AND att.ListId = @ListId
            AND att.Level <= 1 AND att.Type <= 0 AND att.DeleteTransactionId = 0x AND att.SiteId = @SiteId AND att.DoclibRowId IS NULL
            AND att.IsCurrentVersion <= 1  
            INNER JOIN AllDocs item WITH(NOLOCK) ON att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId
            AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar) AND att.Level <= item.Level AND item.Type <= 1
            AND item.IsCurrentVersion <= 1 AND item.DoclibRowId IS NOT NULL --AND item.Size <= 0
            WHERE item.SiteId = @SiteId AND (item.ParentId = @ParentId OR item.Id=@ParentId) AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
            AND ds.Content IS NULL AND ds.RbsId IS NOT NULL";

        public const string Sp13ItemStubAttachmentsInFolderWithRecycleBin = @"
            SELECT COUNT(distinct(ds.DocId)) FROM DocStreams ds
            inner join DocsToStreams Dts on ds.DocId=Dts.DocId and ds.SiteId=Dts.SiteId and ds.BSN=Dts.BSN and ds.Partition=Dts.Partition
            INNER JOIN AllDocs att WITH(NOLOCK) ON Dts.DocId = att.Id AND Dts.SiteId = att.SiteId AND Dts.HistVersion=0
            AND (ds.Content IS NULL AND ds.RbsId IS NOT NULL) AND att.WebId = @WebId AND att.ListId = @ListId
            AND att.Level <= 1 AND att.Type <= 0 AND att.SiteId = @SiteId AND att.DoclibRowId IS NULL
            AND att.IsCurrentVersion <= 1  
            INNER JOIN AllDocs item WITH(NOLOCK) ON att.SiteId = item.SiteId
            AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar) AND att.Level <= item.Level AND item.Type <= 1
            AND item.IsCurrentVersion <= 1 AND item.DoclibRowId IS NOT NULL --AND item.Size <= 0
            WHERE item.SiteId = @SiteId AND (item.ParentId = @ParentId OR item.Id=@ParentId) AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
            AND ds.Content IS NULL AND ds.RbsId IS NOT NULL";

        public const string Sp16StubAllItemAndVersionsWithRecycleBin =
                @"SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion,doc.DeleteTransactionId ,
                doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,stream.Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,stream.RbsId,doc.TimeLastModified as TimeLastModified
                FROM AllDocs AS doc with(nolock)
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=doc.Level
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocs
                UNION
                SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion, docver.DeleteTransactionId,
                docver.DocFlags,docver.HasStream,docver.Level,3 AS QueryType,stream.Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,stream.RbsId,doc.TimeLastModified as TimeLastModified
                FROM AllDocVersions AS docver with(nolock) 
                INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND (docver.DeleteTransactionId=0x OR docver.DeleteTransactionId<>0x) AND doc.IsCurrentVersion=1
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=docver.UIVersion
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocVersions
                ORDER BY LeafName,UIVersion DESC";

        public const string Sp16StubAllItemAndVersions =
                @"SELECT" + CommonColumn.AllDocsLogicalColumns + @",doc.IsCurrentVersion,doc.UIVersion as UIVersion ,
                doc.DocFlags,doc.HasStream,doc.Level,2 AS QueryType,stream.Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,stream.RbsId,doc.TimeLastModified as TimeLastModified
                FROM AllDocs AS doc with(nolock)
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=0 and DTStream.Level=doc.Level
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocs
                UNION
                SELECT" + CommonColumn.AllDocsLogicalColumns + @",CONVERT(bit,0),docver.UIVersion as UIVersion,
                docver.DocFlags,docver.HasStream,docver.Level,3 AS QueryType,stream.Content,COALESCE(doc.Size ,doc.SizeWrite) as Size,stream.RbsId,doc.TimeLastModified as TimeLastModified
                FROM AllDocVersions AS docver with(nolock) 
                INNER JOIN AllDocs AS doc with(nolock) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and DTStream.HistVersion=docver.UIVersion
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocVersions
                ORDER BY LeafName,UIVersion DESC";

        public const string Sp16ItemStubsByIdsWithRecycleBin = @"WHERE doc.SiteId=@SiteId AND doc.Id in ({0}) AND (doc.DeleteTransactionId=0x OR doc.DeleteTransactionId<>0x)  AND doc.Level<=255 AND doc.IsCurrentVersion=1";

        public const string Sp16ItemStubsByIds = @"WHERE doc.SiteId=@SiteId AND doc.Id in ({0}) AND doc.DeleteTransactionId=0x  AND doc.Level<=255 AND doc.IsCurrentVersion=1";

        public const string Sp16ItemStubsByIdsCammandLine =
                @"SELECT doc.Id,doc.DocFlags,stream.RbsId,
                CASE WHEN doc.DocFlags&65536=0 AND (stream.Content IS NOT NULL OR stream.RbsId IS NULL) THEN null ELSE stream.Content END as Content
                FROM AllDocs AS doc with(nolock)
                left outer join DocsToStreams As DTStream WITH(NOLOCK) on doc.Id=DTStream.DocId and doc.SiteId=DTStream.SiteId and doc.IsCurrentVersion=1 and DTStream.HistVersion=0 and DTStream.Level=doc.Level
                LEFT OUTER JOIN DocStreams AS stream WITH(NOLOCK) ON DTStream.SiteId = stream.SiteId AND DTStream.DocId = stream.DocId and DTStream.BSN=stream.BSN and DTStream.Partition=stream.Partition
                @WHEREAllDocs";
    }

    public class AveQueryStringReportCenter16
    {
        public const string GetLastAccessTimeOfSite_Select_UserInfo_AuditData = @"SELECT UserInfo.tp_Login as UserName, 
                   UserInfo.tp_Title as DisplayName, 
                   AuditData.Occurred as Occurred,
                   AuditData.UserId as UserId 
                   FROM AuditData (nolock)         
                   left join UserInfo (nolock) on tp_SiteId = AuditData.SiteId  
                   and tp_Id = UserId
                   WHERE SiteId= @SiteId 
                   and Occurred > @StartTime 
                   and Occurred < @EndTime  ORDER BY Occurred DESC ";

        public const string GetLastAccessTimeOfWeb_Select_UserInfo_AuditData = @"SELECT UserInfo.tp_Login as UserName, 
                   UserInfo.tp_Title as DisplayName, 
                   AuditData.Occurred as Occurred,
                   AuditData.UserId as UserId 
                   FROM dbo.AuditData (nolock)         
                   left join UserInfo (nolock) on tp_SiteId = AuditData.SiteId  
                   and tp_Id = UserId
                   WHERE SiteId= @SiteId 
                   and Occurred > @StartTime 
                   and Occurred < @EndTime 
                   and ItemId = @WebId 
                   and ItemType in(6, 7)  ORDER BY Occurred DESC ";

        public const string GetLastAccessTimeOfList_Select_UserInfo_AuditData = @" SELECT TOP 1 UserInfo.tp_Login as UserName, 
                       UserInfo.tp_Title as DisplayName, 
                       AuditData.Occurred as Occurred,
                       AuditData.UserId as UserId 
                       FROM AuditData (nolock)         
                       left join UserInfo (nolock) on tp_SiteId = AuditData.SiteId
                       and UserInfo.tp_Id = UserId
                       inner join AllLists (nolock) on ItemId = AllLists.tp_Id 
                       WHERE SiteId = @SiteId
                       and Occurred > @StartTime 
                       and Occurred < @EndTime 
                       and AllLists.tp_WebId = @WebId
                       and AllLists.tp_SiteId = @SiteId
                       and ItemType = 4 ORDER BY Occurred DESC";
    }

    public class AveDiscoverQueryString16
    {
        /// <summary>
        /// all webs under the site
        /// </summary>
        public const string DiscoverAllWebs = @"SELECT Id,FullUrl,Title,ParentWebId,AppInstanceId,DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND DeleteTransactionId = 0x ORDER BY FullUrl";

        /// <summary>
        /// changed webs from start time to end time
        /// Added SiteId to improve performance.
        /// 
        /// EventTime, EventType, ObjectType, WebId, FullUrl, Title, ParentWebId, Webs.Id, ItemFullURL, int0, int1, ItemName, AppInstanceId, WebRootFolder
        /// </summary>
        public const string DiscoverChangedWebs =
            @"
SELECT distinct '2010/01/01 1:01:01' as EventTime,2 as EventType,16 as ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,NULL as ItemFullUrl,NULL as int0,NULL as int1,NULL as ItemName,AppInstanceId
,(0) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.SiteId=EventCache.SiteId AND AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL AND EventCache.ListId IS NOT NULL

union 

SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName,AppInstanceId
,( case when ObjectType=32 and FullUrl=ItemFullUrl then 1 else 0 end) as WebRootFolder
FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.SiteId=EventCache.SiteId AND AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL AND EventCache.ListId IS NULL ORDER BY EventTime
";
        //             @"
        //SELECT EventTime,EventType,ObjectType,WebId,FullUrl,Title,ParentWebId,AllWebs.Id,ItemFullUrl,EventCache.int0,EventCache.int1,EventCache.ItemName,AppInstanceId
        //FROM EventCache WITH(NOLOCK) LEFT JOIN AllWebs WITH(NOLOCK) ON AllWebs.SiteId=EventCache.SiteId AND AllWebs.Id=EventCache.WebId AND AllWebs.DeleteTransactionId = 0x 
        //WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND WebId IS NOT NULL ORDER BY EventTime";

        /// <summary>
        /// sub webs under special web
        /// </summary>
        public const string SubWebs = "SELECT Id, FullUrl, Title,ParentWebId,AppInstanceId,DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND (ParentWebId=@ParentId OR  ParentWebId IS NULL) AND DeleteTransactionId = 0x ORDER BY FullUrl";

        public const string SubWebsWithRecycleBin = "SELECT Id, FullUrl, Title,ParentWebId,AppInstanceId,DeleteTransactionId FROM AllWebs WITH(NOLOCK) WHERE SiteId = @SiteId AND (ParentWebId=@ParentId OR  ParentWebId IS NULL) ORDER BY FullUrl";

        public const string ItemChanged = "SELECT " + CommonColumn.AllDocsCommonColumns + ",doc.Level,doc.CheckoutUserId,doc.IsCurrentVersion," + CommonColumn.EventCache + @"
FROM EventCache ec WITH(NOLOCK) 
LEFT JOIN AllDocs doc WITH(NOLOCK) ON doc.SiteId=ec.SiteId AND doc.Id=ec.DocId AND doc.Level<=255 AND doc.IsCurrentVersion=1
@WHERE";

        public const string ItemVersions =
           @"SELECT DISTINCT tp_UIVersion, tp_Modified, tp_IsCurrent, tp_GUID, tp_ID ,tp_UIVersionString,tp_Level,tp_Size,tp_IsCurrentVersion
FROM AllUserData With(NOLOCK) WHERE [tp_SiteId]=@SiteId AND [tp_DeleteTransactionId]=0x AND [tp_ListId]=@ListId AND ([tp_IsCurrentVersion]=0 OR [tp_IsCurrentVersion]=1) AND [tp_id]=@docLibId And [tp_RowOrdinal]=0 ORDER BY tp_UIVersion DESC";

        public const string ListRootFolder =
@"
SELECT @Column,doc.DirName,doc.Level,al.tp_MaxMajorwithMinorVersionCount FROM AllLists al WITH(NOLOCK)
INNER JOIN AllDocs doc WITH(NOLOCK) ON al.tp_SiteId=doc.SiteId AND doc.Id=al.tp_RootFolder AND doc.Level<=255
WHERE al.tp_SiteId=@SiteId AND al.tp_WebId=@WebId AND al.tp_Id=@ListId";

        public const string Lists =
            @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate,al.tp_DeleteTransactionId,al.tp_Fields
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON al.tp_SiteId=ad.SiteId AND DeleteTransactionId=0x AND ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_SiteId=@SiteId AND al.tp_WebId=@WebId AND al.tp_DeleteTransactionId=0x ORDER BY al.tp_Title";

        public const string ListsWithRecycleBin =
           @"
SELECT al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,ad.DirName+'/'+ad.LeafName as RootFolderUrl,al.tp_ServerTemplate,al.tp_DeleteTransactionId,al.tp_Fields
FROM AllLists al WITH(NOLOCK) INNER JOIN AllDocs AS ad WITH (NOLOCK, INDEX=Docs_IdLevelUnique) ON al.tp_SiteId=ad.SiteId AND ad.Id=al.tp_RootFolder AND Level=1
WHERE al.tp_SiteId=@SiteId AND al.tp_WebId=@WebId ORDER BY al.tp_Title";

        public const string ListChanged =
             @"
SELECT EventType,ObjectType,EventCache.ListId as ecListId,ModifiedBy,EventTime,ItemFullUrl,
al.tp_ID,al.tp_Title,al.tp_RootFolder,al.tp_BaseType,al.tp_Flags,al.tp_ServerTemplate,ad.DirName+'/'+ad.LeafName as RootFolderUrl,EventCache.Int0,EventCache.Int1
FROM EventCache WITH(NOLOCK)
LEFT JOIN AllLists AS al WITH(NOLOCK) ON al.tp_WebId=@WebId AND al.tp_ID=EventCache.ListId 
LEFT JOIN AllDocs AS ad WITH(NOLOCK) ON ad.SiteId=al.tp_SiteId AND ad.Id=al.tp_RootFolder AND ad.Level<=255
WHERE EventTime <= @endTime AND EventTime >= @startTime AND EventCache.SiteId=@siteId AND EventCache.WebId=@webId AND (EventCache.ListId IS NOT NULL OR ObjectType IN (16,32)) ORDER BY EventTime";

        public const string ItemSecurityChanged =
    @"
SELECT EventType,ObjectType,int0,int1,Guid0,EventTime
FROM EventCache WITH(NOLOCK) 
WHERE EventTime <= @endTime AND EventTime >= @startTime AND SiteId=@SiteId AND WebId=@webId AND ListId=@listId 
AND ItemId=@itemId AND EventType IN (524288,33554432,786432,41943040) ORDER BY EventTime";

        public const string ListContentTypes = @"SELECT tp_ContentTypes FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_WebId=@webId AND tp_ID=@listId";
        /// <summary>
        /// 如果只修改View上的webpart，包括添加web part，则不会有list id，所以需要反向查询下对应的list，来捕获该事件
        /// </summary>
        public const string ListViewWebPartChangedEvent =
            @"
select distinct listid from alldocs WITH(NOLOCK) 
where  SiteId=@siteId AND id in 
( 
select distinct DocId from EventCache WITH(NOLOCK) 
where EventTime <= @endTime AND EventTime >= @startTime 
AND SiteId=@siteId AND WebId=@webId 
and ListId is NULL 
and ItemId is null and DocId is not null and ObjectType=16
) 
and Level=1
";

        public const string ItemSizeAndParnetId = @"SELECT COALESCE(doc.Size ,doc.SizeWrite) as Size,ParentId FROM AllDocs as doc WITH(NOLOCK) WHERE SiteId=@SiteId And DeleteTransactionId = 0x And Id=@DocId AND Level=@Level AND IsCurrentVersion=1";

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
COALESCE(doc.Size ,doc.SizeWrite) as Size,
doc.UIVersion ";

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
ec.TimeLastModified as EventCacheTimeLastModified,
Int0,
Int1";
    }
}
