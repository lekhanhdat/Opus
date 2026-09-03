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
using System.Globalization;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService : IAveSOIntegrationQueryService
    {
        #region private Methods
        /// <summary>
        /// 获取一个item的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="web"></param>
        /// <param name="docLibRowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        private List<string> GetStubAttachmentsByDocLibRowId(Guid siteId, int docLibRowId)
        {
            List<string> stubAttachmentList = new List<string>();

            mQueryWorker.AddParameter("@DocLibRowId", docLibRowId.ToString());
            mQueryWorker.AddParameter("@SiteId", siteId);
            object attachmentDir = null;
            try
            {
                attachmentDir = mQueryWorker.ExecuteScalar(
    @"SELECT DirName + '/' + LeafName + '/Attachments/' FROM AllLists With(nolock) INNER JOIN AllDocs With(nolock) ON Id = tp_RootFolder AND Level = 1
WHERE tp_SiteId=@SiteId AND tp_WebId = @WebId AND tp_ID = @ListId");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetStubAttachmentsError, e);
                attachmentDir = null;
            }
            if (attachmentDir == null || !(attachmentDir is string))
            {
                return stubAttachmentList;
            }

            mQueryWorker.AddParameter("@AttachmentDir", ((string)attachmentDir).Trim('/'));

            object attachmentParentId = null;
            try
            {
                attachmentParentId = mQueryWorker.ExecuteScalar(
    @"SELECT Id FROM AllDocs With(nolock) WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x ) AND LeafName=@DocLibRowId AND DirName=@AttachmentDir");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetStubAttachmentsError, e);
                attachmentParentId = null;
            }
            if (attachmentParentId == null || !(attachmentParentId is Guid))
            {
                return stubAttachmentList;
            }

            mQueryWorker.AddParameter("@ParentId", (Guid)attachmentParentId);
            string stubAttachmentRelativeUrl = @"
SELECT DISTINCT DirName+'/'+LeafName FROM AllDocs doc With(nolock) INNER JOIN DocStreams AS stream With(nolock)
on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0";
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(stubAttachmentRelativeUrl))
                {
                    while (sr.Read())
                    {
                        stubAttachmentList.Add(sr.GetString(0).Trim('/'));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetStubAttachmentsError, e);
            }
            return stubAttachmentList;
        }

        #endregion

        #region SOIntegrationUtility
        /// <summary>
        /// 获取一个item的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        [QueryReview("2012/05/15", "Kexin Guo")]
        public List<string> GetItemStubAttachments(Guid siteId, Guid webId, Guid listId, int itemId)
        {
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            return GetStubAttachmentsByDocLibRowId(siteId, itemId);
        }
        /// <summary>
        /// 获取一个item的指定范围的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="startNum"></param>
        /// <param name="endNum"></param>
        /// <param name="totalNum"></param>
        /// <returns></returns>
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        public List<StubDocumentInfo> GetItemStubAttachmentsByDB(IAveListItem listItem, int startNum, int endNum, ref int totalNum)
        {
            List<StubDocumentInfo> result = new List<StubDocumentInfo>();
            mQueryWorker.AddParameter("@SiteId", listItem.ParentList.ParentWeb.Site.ID);
            mQueryWorker.AddParameter("@WebId", listItem.ParentList.ParentWeb.ID);
            mQueryWorker.AddParameter("@ListId", listItem.ParentList.ID);
            mQueryWorker.AddParameter("@StartNum", startNum);
            mQueryWorker.AddParameter("@endNum", endNum);

            object attachmentDir = null;
            try
            {
                attachmentDir = mQueryWorker.ExecuteScalar(
    @"SELECT DirName + '/' + LeafName + '/Attachments/' FROM AllLists With(nolock) INNER JOIN AllDocs With(nolock) ON Id = tp_RootFolder AND Level = 1
WHERE tp_SiteId=@SiteId AND tp_WebId = @WebId AND tp_ID = @ListId");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetItemStubAttachmentsError, e);
                attachmentDir = null;
            }
            if (attachmentDir == null || !(attachmentDir is string))
            {
                return result;
            }

            mQueryWorker.AddParameter("@DocLibRowId", listItem.ID.ToString());
            mQueryWorker.AddParameter("@AttachmentDir", ((string)attachmentDir).Trim('/'));
            object attachmentParentId = null;
            try
            {
                attachmentParentId = mQueryWorker.ExecuteScalar(
    @"SELECT Id FROM AllDocs With(nolock) WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x ) AND LeafName=@DocLibRowId AND DirName=@AttachmentDir");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetItemStubAttachmentsError, e);
                attachmentParentId = null;
            }
            if (attachmentParentId == null || !(attachmentParentId is Guid))
            {
                return result;
            }

            mQueryWorker.AddParameter("@ParentId", (Guid)attachmentParentId);

            string totalCout = @"
SELECT Count(doc.Id) FROM AllDocs doc With(nolock) INNER JOIN DocStreams AS stream With(nolock)
on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0";
            try
            {
                totalNum = (int)mQueryWorker.ExecuteScalar(totalCout);
            }
            catch (Exception)
            {
                throw;
            }

            string stubAttachmentRelativeUrl = @"
BEGIN
WITH StubAttachment(Id, DirName, LeafName, Size,UIVersion, RbsId,Content)
AS
(
SELECT doc.Id, doc.DirName, doc.LeafName, doc.Size, doc.UIVersion,stream.RbsId,stream.Content
FROM AllDocs doc With(nolock) INNER JOIN DocStreams AS stream With(nolock)
on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0
),
StubAttachmentWithRowNum(Id, DirName, LeafName, Size, UIVersion,RbsId,Content,RowNum)
AS
(
select *,ROW_NUMBER() Over (order by StubAttachment.Id desc) RowNum from StubAttachment With(nolock)
)
SELECT Id,DirName,LeafName,Size,UIVersion,RbsId,Content,RowNum FROM StubAttachmentWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";

            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(stubAttachmentRelativeUrl))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            Guid id = sr.GetGuid(0);
                            StubDocumentInfo stubAttachment = new StubDocumentInfo();
                            stubAttachment.IsAttachment = true;
                            stubAttachment.DocId = id;
                            stubAttachment.DirName = sr.GetString(1);
                            stubAttachment.LeafName = sr.GetString(2);
                            stubAttachment.Size = sr.IsDBNull(3) ? 0 : sr.GetInt32(3);
                            stubAttachment.UIVersion = sr.GetInt32(4);
                            stubAttachment.RbsId = sr.IsDBNull(5) ? null : sr.GetValue(5) as byte[];
                            stubAttachment.Content = sr.IsDBNull(6) ? null : sr.GetValue(6);
                            stubAttachment.ItemLeafName = listItem.Title;
                            result.Add(stubAttachment);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetColumnValueError, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.StubAttachmentRelativeUrlError, ex);
            }
            return result;
        }
        /// <summary>
        /// 获取一个folder下的所有item的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/03", "Kexin Guo")]
        public List<string> GetItemStubAttachmentsInFolder(Guid siteId, Guid webId, Guid listId, Guid parentId)
        {
            List<string> stubAttachemntList = new List<string>();

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ParentId", parentId);

            string docLibRowIdStr = @"SELECT DISTINCT DocLibRowId FROM AllDocs With(nolock) WHERE SiteId=@SiteId AND ParentId=@ParentId AND DocLibRowId IS NOT NULL AND DeleteTransactionId=0x ";

            List<int> docLibRowIdList = new List<int>();
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(docLibRowIdStr))
                {
                    while (sr.Read())
                    {
                        docLibRowIdList.Add(sr.GetInt32(0));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            foreach (int docLibRowId in docLibRowIdList)
            {
                stubAttachemntList.AddRange(GetStubAttachmentsByDocLibRowId(siteId, docLibRowId));
            }
            return stubAttachemntList;
        }
        /// <summary>
        /// 获取一个folder下指定范围的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="startNum"></param>
        /// <param name="endNum"></param>
        /// <param name="totalNum"></param>
        /// <returns></returns>
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        public List<StubDocumentInfo> GetItemStubAttachmentsInFolderByDB(Guid siteId, Guid webId, Guid listId, Guid parentId, int startNum, int endNum, ref int totalNum)
        {
            List<StubDocumentInfo> result = new List<StubDocumentInfo>();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@StartNum", startNum);
            mQueryWorker.AddParameter("@endNum", endNum);

            object attachmentDir = null;
            try
            {
                attachmentDir = mQueryWorker.ExecuteScalar(
    @"SELECT DirName + '/' + LeafName + '/Attachments/' FROM AllLists With(nolock) INNER JOIN AllDocs With(nolock) ON Id = tp_RootFolder AND Level = 1
WHERE tp_SiteId=@SiteId AND tp_WebId = @WebId AND tp_ID = @ListId");
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetItemStubAttachmentsError, e);
                attachmentDir = null;
            }
            if (attachmentDir == null || !(attachmentDir is string))
            {
                return result;
            }
            mQueryWorker.AddParameter("@AttachmentDir", ((string)attachmentDir).Trim('/') + "/");

            #region OldTotalCount
            //            string totalCount = @" 
            //SELECT COUNT(stream.Id) FROM AllDocs item WITH(NOLOCK)
            //INNER JOIN AllDocs att WITH(NOLOCK) ON 
            //att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
            //AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
            //AND att.Level <= item.Level 
            //AND att.WebId = @WebId AND att.ListId = @ListId
            //AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
            //AND att.IsCurrentVersion <= 1  
            //INNER JOIN DocStreams stream WITH(NOLOCK) ON 
            //stream.Id = att.Id AND stream.SiteId = att.SiteId AND stream.InternalVersion = att.InternalVersion
            //AND (att.DocFlags&65536 = 65536 OR (stream.Content IS NULL AND stream.RbsId IS NOT NULL))
            //WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
            //AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1";
            #endregion

            string totalCount = @" 
SELECT COUNT(distinct(stream.DocId)) FROM AllDocs item WITH(NOLOCK)
INNER JOIN AllDocs att WITH(NOLOCK) ON 
att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
AND att.Level <= item.Level 
AND att.WebId = @WebId AND att.ListId = @ListId
AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
AND att.IsCurrentVersion <= 1  
INNER JOIN DocsToStreams DTS WITH(NOLOCK) ON DTS.DocId=att.Id and DTS.SiteId=att.SiteId and DTS.Level=att.Level
INNER JOIN DocStreams stream WITH(NOLOCK) ON 
stream.DocId = att.Id AND stream.SiteId = att.SiteId AND DTS.Partition=stream.Partition and DTS.BSN=stream.BSN
AND  (stream.Content IS NULL AND stream.RbsId IS NOT NULL)
WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1";
            try
            {
                totalNum = (int)mQueryWorker.ExecuteScalar(totalCount);
            }
            catch (Exception)
            {
                throw;
            }

            #region OldItemStubAttachmentsInFolder
            //            string itemStubAttachmentsInFolder =
            // @"
            //BEGIN
            //WITH StubAttachment(Id, DirName, LeafName, Size,UIVersion,RbsId,ItemName,Content)
            //AS
            //(SELECT att.Id, att.DirName, att.LeafName, att.Size, att.UIVersion,stream.RbsId,item.LeafName,stream.Content
            //FROM AllDocs item WITH(NOLOCK)
            //INNER JOIN AllDocs att WITH(NOLOCK) ON 
            //att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
            //AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
            //AND att.Level <= item.Level 
            //AND att.WebId = @WebId AND att.ListId = @ListId
            //AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
            //AND att.IsCurrentVersion <= 1  
            //INNER JOIN DocStreams stream WITH(NOLOCK) ON 
            //stream.DocId = att.Id AND stream.SiteId = att.SiteId AND stream.InternalVersion = att.InternalVersion
            //AND (att.DocFlags&65536 = 65536 OR (stream.Content IS NULL AND stream.RbsId IS NOT NULL))
            //WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
            //AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1
            //),
            //StubAttachmentWithRowNum(Id, DirName, LeafName, Size, UIVersion,RbsId,ItemName,Content,RowNum)
            //AS
            //(
            //select *,ROW_NUMBER() Over (order by StubAttachment.ItemName asc,StubAttachment.LeafName asc) RowNum from StubAttachment With(nolock)
            //)
            //SELECT Id, DirName, LeafName, Size, UIVersion,RbsId,ItemName,Content,RowNum FROM StubAttachmentWithRowNum With(nolock)
            //WHERE RowNum between @StartNum AND @endNum
            //END";
            #endregion

            string itemStubAttachmentsInFolder = @"
BEGIN
WITH StubAttachment(Id, DirName, LeafName, Size,UIVersion,ItemName)
AS
(SELECT distinct(att.Id), att.DirName, att.LeafName, att.Size, att.UIVersion,item.LeafName
FROM AllDocs item WITH(NOLOCK)
INNER JOIN AllDocs att WITH(NOLOCK) ON 
att.SiteId = item.SiteId AND att.DeleteTransactionId = item.DeleteTransactionId 
AND att.DirName = @AttachmentDir + CAST(item.DocLibRowId AS nvarchar)
AND att.Level <= item.Level 
AND att.WebId = @WebId AND att.ListId = @ListId
AND att.Level <= 1 AND att.Type <= 0 AND att.DoclibRowId IS NULL
AND att.IsCurrentVersion <= 1  
INNER JOIN DocsToStreams DTS WITH(NOLOCK) ON DTS.DocId=att.id and DTS.SiteId=att.SiteId and DTS.Level=att.Level
INNER JOIN DocStreams stream WITH(NOLOCK) ON stream.DocId = att.Id AND stream.SiteId = att.SiteId 
AND stream.Partition=DTS.Partition and stream.BSN=DTS.BSN
AND  (stream.Content IS NULL AND stream.RbsId IS NOT NULL)
WHERE item.SiteId = @SiteId AND item.ParentId = @ParentId  AND item.Level <= 2 AND item.DeleteTransactionId = 0x 
AND item.IsCurrentVersion = 1 AND item.DoclibRowId IS NOT NULL AND item.Type <= 1
),
StubAttachmentWithRowNum(Id, DirName, LeafName, Size, UIVersion,ItemName,RowNum)
AS
(
select *,ROW_NUMBER() Over (order by StubAttachment.ItemName asc,StubAttachment.LeafName asc) RowNum from StubAttachment With(nolock)
)
SELECT Id, DirName, LeafName, Size, UIVersion,ItemName,RowNum FROM StubAttachmentWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(itemStubAttachmentsInFolder))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            Guid id = sr.GetGuid(0);
                            StubDocumentInfo stubAttachment = new StubDocumentInfo();
                            stubAttachment.IsAttachment = true;
                            stubAttachment.DocId = id;
                            stubAttachment.DirName = sr.GetString(1);
                            stubAttachment.LeafName = sr.GetString(2);
                            stubAttachment.Size = sr.IsDBNull(3) ? 0 : sr.GetInt32(3);
                            stubAttachment.UIVersion = sr.GetInt32(4);
                            //stubAttachment.RbsId = sr.IsDBNull(5) ? null : sr.GetValue(5) as byte[];
                            stubAttachment.ItemLeafName = sr.GetString(5);
                            //stubAttachment.Content = sr.IsDBNull(7) ? null : sr.GetValue(7);
                            result.Add(stubAttachment);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetColumnValueError, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ItemStubAttachmentsInFolderError, ex);
            }
            return result;
        }
        /// <summary>
        /// 获取一个folder下stub类型的files.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        public List<string> GetStubFilesUrlInFolder(Guid siteId, Guid parentId)
        {
            List<string> stubFileUrlList = new List<string>();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);

            string stubFileRelativeUrl =
@"SELECT DISTINCT DirName+'/'+LeafName FROM AllDocs doc With(nolock) INNER JOIN DocStreams AS stream With(nolock)
on stream.Id = doc.Id AND stream.SiteId = doc.SiteId AND stream.InternalVersion = doc.InternalVersion 
AND (doc.DocFlags&65536<>0 OR stream.Content is NULL AND stream.RbsId is not NULL)
WHERE doc.SiteId=@SiteId AND doc.ParentId=@ParentId AND doc.DeleteTransactionId=0x AND doc.Type=0";
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(stubFileRelativeUrl))
                {
                    while (sr.Read())
                    {
                        string stubFileServerRelativeUrl = sr.GetString(0).Trim('/');
                        stubFileUrlList.Add(stubFileServerRelativeUrl);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return stubFileUrlList;
        }
        /// <summary>
        /// 获取一个folder下指定范围的stub类型的files.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="startNum"></param>
        /// <param name="endNum"></param>
        /// <param name="totalNum"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of sql statement. ")]
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        public List<StubDocumentInfo> GetStubFilesInFolderByDB(IAveFolder folder, int startNum, int endNum, ref int totalNum)
        {
            mQueryWorker.AddParameter("@SiteId", folder.ParentWeb.Site.ID);
            mQueryWorker.AddParameter("@ParentId", folder.UniqueId);

            mQueryWorker.AddParameter("@StartNum", startNum);
            mQueryWorker.AddParameter("@endNum", endNum);

            List<StubDocumentInfo> result = new List<StubDocumentInfo>();

            #region oldCount
            //            string totalCount = @" 
            //WITH DocsBlob(Id)
            //AS
            //(
            //--Get All Stubs in AllDocs table
            //SELECT docs.Id FROM AllDocs docs WITH (NOLOCK)
            //INNER JOIN DocStreams docStream WITH (NOLOCK) ON docStream.SiteId = docs.SiteId AND docStream.Id = docs.Id AND docStream.InternalVersion = docs.InternalVersion
            //AND (docs.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
            //WHERE docs.SiteId = @SiteId AND docs.DeleteTransactionId = 0x AND docs.ParentId = @ParentId AND docs.Type <= 0  AND docs.Level <= 255 AND docs.IsCurrentVersion <= 1
            //UNION ALL
            //--Get All Stubs in AllDocVersions table
            //SELECT Versions.Id FROM AllDocs docs  WITH (NOLOCK)
            //INNER JOIN AllDocVersions versions WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id 
            //AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion AND docs.DeleteTransactionId = versions.DeleteTransactionId 
            //INNER JOIN DocStreams docStream WITH (NOLOCK) on docStream.SiteId = versions.SiteId AND docStream.Id = versions.Id AND docStream.InternalVersion = versions.InternalVersion
            //AND (versions.DocFlags & 65536 = 65536 OR (docStream.Content IS NULL AND docStream.RbsId IS NOT NULL))
            //WHERE docs.SiteId = @SiteId AND docs.DeleteTransactionId = 0x AND docs.ParentId = @ParentId AND docs.IsCurrentVersion = 1 AND docs.Level <= 255  AND docs.Type <= 0
            //)
            //SELECT COUNT(Id) FROM DocsBlob With(nolock)";
            #endregion

            string totalCount = @"
BEGIN
WITH DocsBlob(Id,InternalVersion)
AS
(
SELECT distinct(docs.Id),docs.InternalVersion 
FROM AllDocs docs WITH (NOLOCK)
inner join DocsToStreams Dts on docs.SiteId=Dts.SiteId and docs.Id=Dts.DocId and docs.Level=Dts.Level
inner join DocStreams docStream on Dts.SiteId=docStream.SiteId and Dts.DocId=docStream.DocId and Dts.Partition=docStream.Partition and Dts.BSN=docStream.BSN
AND docs.Type <= 0 AND docs.ParentId = @ParentId  AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
AND docStream.Content IS NULL AND docStream.RbsId IS NOT NULL and Dts.HistVersion=0
AND docs.IsCurrentVersion <= 1 WHERE docStream.SiteId = @SiteId
UNION ALL
SELECT distinct(Versions.Id),versions.InternalVersion
FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId 
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
inner join DocsToStreams Dts on Dts.SiteId=versions.SiteId and Dts.DocId=versions.Id and Dts.HistVersion=versions.UIVersion
inner join DocStreams docStream on Dts.SiteId=docStream.SiteId and Dts.DocId=docStream.DocId and Dts.Partition=docStream.Partition and Dts.BSN=docStream.BSN
AND docStream.Content IS NULL AND docStream.RbsId IS NOT NULL and  Dts.HistVersion<>0
WHERE versions.SiteId = @SiteId
)
SELECT Count(Id) FROM DocsBlob With(nolock)
End";
            try
            {
                totalNum = (int)mQueryWorker.ExecuteScalar(totalCount);
            }
            catch (Exception)
            {
                throw;
            }

            #region oldStubFilesAndVersions
            //            string stubFilesAndVersions =
            //                @"
            //BEGIN
            //with StubFiles(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RbsId,Content)
            //as 
            //(
            //SELECT docs.Id, docs.DirName AS DirName, docs.LeafName, docs.UIVersion, docs.IsCurrentVersion, docs.Size, stream.RbsId,stream.Content
            //FROM AllDocs docs WITH (NOLOCK)
            //INNER JOIN DocStreams stream WITH (NOLOCK) on stream.SiteId = docs.SiteId AND stream.Id = docs.Id AND stream.InternalVersion = docs.InternalVersion
            //AND (docs.DocFlags & 65536 = 65536 OR (stream.Content IS NULL AND stream.RbsId IS NOT NULL))
            //WHERE docs.SiteId = @SiteId AND docs.DeleteTransactionId = 0x AND docs.ParentId = @ParentId AND docs.Type <= 0  AND docs.Level <= 255 AND docs.IsCurrentVersion <= 1 
            //UNION ALL
            //SELECT Versions.Id, docs.DirName AS DirName, docs.LeafName, versions.UIVersion, 0, versions.Size, stream.RbsId,stream.Content
            //FROM AllDocs docs WITH (NOLOCK)
            //INNER JOIN AllDocVersions versions WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
            //AND versions.DeleteTransactionId = docs.DeleteTransactionId
            //INNER JOIN DocStreams stream WITH (NOLOCK) on stream.SiteId = versions.SiteId AND stream.Id = versions.Id AND stream.InternalVersion = versions.InternalVersion
            //AND (versions.DocFlags & 65536 = 65536 OR (stream.Content IS NULL AND stream.RbsId IS NOT NULL))
            //WHERE docs.SiteId = @SiteId AND docs.DeleteTransactionId = 0x AND docs.ParentId = @ParentId AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND docs.Type <= 0
            //),
            //StubFilesWithRowNum(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RbsId,Content,RowNum)
            //as
            //(
            //select *,ROW_NUMBER() Over (order by StubFiles.LeafName asc,StubFiles.UIVersion asc) RowNum from StubFiles With(nolock)
            //)
            //select Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RbsId,Content,RowNum FROM StubFilesWithRowNum With(nolock)
            //WHERE RowNum between @StartNum AND @endNum
            //END";
            #endregion

            string stubFilesAndVersions = @"
BEGIN
with StubFiles(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size)
as 
(
SELECT distinct(docs.Id), docs.DirName AS DirName, docs.LeafName, docs.UIVersion, docs.IsCurrentVersion, docs.Size
FROM AllDocs docs WITH (NOLOCK)
inner join DocsToStreams Dts on docs.SiteId=Dts.SiteId and docs.Id=Dts.DocId and docs.Level=Dts.Level
inner join DocStreams ds on Dts.SiteId=ds.SiteId and Dts.DocId=ds.DocId and Dts.Partition=ds.Partition and Dts.BSN=ds.BSN
AND docs.Type <= 0 AND docs.ParentId = @ParentId AND docs.DeleteTransactionId = 0x AND docs.Level <= 255
AND ds.Content IS NULL AND ds.RbsId IS NOT NULL and Dts.HistVersion=0
AND docs.IsCurrentVersion <= 1 WHERE ds.SiteId = @SiteId 
UNION ALL
SELECT distinct(Versions.Id), docs.DirName AS DirName, docs.LeafName, versions.UIVersion, 0, versions.Size
FROM AllDocVersions versions WITH (NOLOCK)
INNER JOIN AllDocs docs WITH (NOLOCK) on versions.SiteId = docs.SiteId AND versions.Id = docs.Id AND docs.ParentId = @ParentId
AND docs.IsCurrentVersion = 1 AND docs.Level <= 255 AND versions.Level <= 2 AND versions.UIVersion < docs.UIVersion
AND docs.DeleteTransactionId = versions.DeleteTransactionId AND docs.DeleteTransactionId = 0x AND docs.Type <= 0
inner join DocsToStreams Dts on Dts.SiteId=versions.SiteId and Dts.DocId=versions.Id and Dts.HistVersion=versions.UIVersion
inner join DocStreams ds on Dts.SiteId=ds.SiteId and Dts.DocId=ds.DocId and Dts.Partition=ds.Partition and Dts.BSN=ds.BSN
AND ds.Content IS NULL AND ds.RbsId IS NOT NULL and Dts.HistVersion<>0
 WHERE versions.SiteId = @SiteId
),
StubFilesWithRowNum(Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RowNum)
as
(
select *,ROW_NUMBER() Over (order by StubFiles.Id desc) RowNum from StubFiles With(nolock)
)
select Id,DirName,LeafName,UIVersion,IsCurrentVersion,Size,RowNum FROM StubFilesWithRowNum With(nolock)
WHERE RowNum between @StartNum AND @endNum
END";
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(stubFilesAndVersions))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            Guid id = sr.GetGuid(0);
                            StubDocumentInfo stubFile = new StubDocumentInfo();
                            stubFile.IsAttachment = false;
                            stubFile.DocId = id;
                            stubFile.DirName = sr.GetString(1);
                            stubFile.ItemLeafName = stubFile.LeafName = sr.GetString(2);
                            stubFile.UIVersion = sr.GetInt32(3);
                            stubFile.IsCurrentVersion = sr.GetInt32(4) == 1 ? true : false;
                            stubFile.Size = sr.IsDBNull(5) ? 0 : sr.GetInt32(5);
                            //stubFile.RbsId = sr.IsDBNull(6) ? null : sr.GetValue(6) as byte[];
                            //stubFile.Content = sr.IsDBNull(7) ? null : sr.GetValue(7);
                            result.Add(stubFile);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetColumnValueError, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.StubFileAndVersionError, ex);
            }
            return result;
        }
        /// <summary>
        /// 通过RbsId获得blobId 并返回是否是Docave6 的stub。
        /// </summary>
        /// <param name="rbsId"></param>
        /// <param name="isD6Stub"></param>
        /// <returns></returns>
        [QueryReview("2012/05/03", "Kexin Guo")]
        public Guid GetStubIdByRbsId(object rbsId, ref bool isD6Stub)
        {
            string getStubIdByRbsId = @"
SELECT store_blob_id
FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH (INDEX(rbs_internal_blobs_pk),NOLOCK) 
WHERE collection_id =CONVERT(int,substring(@RBSId,9,4)) AND blob_number=CONVERT(bigint,SUBSTRING(@RBSId,1,8))";
            if (null == mQueryWorker)
            {
                return Guid.Empty;
            }
            mQueryWorker.AddParameter("@RBSId", rbsId);

            Guid ret = Guid.Empty;
            isD6Stub = false;
            using (SqlDataReader sr = mQueryWorker.ExecuteReader(getStubIdByRbsId))
            {
                if (sr.Read())
                {
                    byte[] blobId = new byte[20];
                    int len = (int)sr.GetBytes(0, 0, blobId, 0, 20);

                    if (blobId[0] == 'D' && blobId[1] == 'O' && blobId[2] == 'C')
                    {
                        isD6Stub = true;
                    }
                    else
                    {
                        return ret;//for non-D6 stub, return Guid.Empty
                    }

                    byte[] result = new byte[16];
                    Array.Copy(blobId, 4, result, 0, 16);
                    ret = new Guid(result);
                }
            }

            return ret;
        }

        /// <summary>
        /// 更新stub类型的file数据库中Content，Size等字段
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <param name="version"></param>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        public void UpdateStubFileStream(Guid siteId, Guid parentId, Guid uniqueId, byte[] bytes, long length, int version)
        {
            //if (needConvertStub)
            //{
            //    bytes = ConvertStub(bytes, file);
            //}
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", uniqueId);
            mQueryWorker.AddParameter("@UIVersion", version);
            mQueryWorker.AddParameter("@Content", bytes);
            mQueryWorker.AddParameter("@Size", length);
            string cmdText = @"Update DocStreams Set Content=@Content where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs With(nolock) WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));";
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            cmdText = @"UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);
                        UPDATE AllDocVersions SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)";
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        /// <summary>
        /// 更新file的数据库中的Content，Size，RbsId等字段，对于Rbs的stub数据，需要将Content设置为null并更新RbsId，无法通过API来实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="uiVersion"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="storageInfo"></param>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        public void UpdateRbsID(Guid siteId, Guid parentId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            string cmdText = string.Empty;
            mQueryWorker.AddParameter("@Id", uniqueId);
            mQueryWorker.AddParameter("@UIVersion", uiVersion);
            if ((DataType)type == DataType.Stub)
            {
                mQueryWorker.AddParameter("@RbsId", data);
                mQueryWorker.AddParameter("@Size", storageInfo.Size);
                cmdText = @"Update DocStreams Set Content=null, RbsId=@RbsId where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs With(nolock) WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);";
            }
            else
            {
                mQueryWorker.AddParameter("@Content", data);
                mQueryWorker.AddParameter("@Size", data.Length);
                cmdText = @"Update DocStreams Set Content=@Content, RbsId=null where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM  AllDocs With(nolock)WHERE (SiteId = @SiteId) AND (Id = @Id) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions With(nolock) WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND (ParentID=@ParentID) AND (Id = @Id) AND (UIVersion = @UIVersion);";
            }


            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        /// <summary>
        /// 更新file的数据库中的Size字段
        /// </summary>
        /// <param name="docInfo"></param>
        [QueryReview("2012/05/16", "Kexin Guo", true, "add DeleteTransactionId and ParentId for AllDocs")]
        public void UpdateDocumentSize(AveSPItemNativeInfo docInfo)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", docInfo.SiteId);
            mQueryWorker.AddParameter("@ParentId", docInfo.Folder.UniqueId);
            mQueryWorker.AddParameter("@Id", docInfo.ItemId);
            mQueryWorker.AddParameter("@Size", docInfo.Size);
            mQueryWorker.AddParameter("@InternalVersion", docInfo.InternalVersion);
            string updateText = @"Update AllDocs Set Size = @Size where SiteId=@SiteId AND ( DeleteTransactionId = 0x or DeleteTransactionId <> 0x ) AND ParentId=@ParentId AND Id=@Id AND InternalVersion=@InternalVersion ;
                            UPDATE AllDocVersions SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (InternalVersion = @InternalVersion);";
            mQueryWorker.ExecuteNonQuery(updateText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        public void UpdateStubDocumentSize(int level, Guid parentId, Guid docId, Guid siteId, int size, long nextBSN)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", docId);
            mQueryWorker.AddParameter("@Size", size);
            mQueryWorker.AddParameter("@Level", level);
            mQueryWorker.AddParameter("@NextBSN", nextBSN);
            string updateText = @"Update AllDocs Set Size = @Size,NextBSN= @nextBSN where SiteId=@SiteId AND ( DeleteTransactionId = 0x or DeleteTransactionId <> 0x ) AND ParentId=@ParentId AND Id=@Id AND Level=@Level ;
                           ";
            mQueryWorker.ExecuteNonQuery(updateText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        public long GetMaxRbs(Guid siteId,Guid docId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", docId);
                string text = @"SELECT TOP 1 BSN FROM DocsToStreams WITH(NOLOCK) WHERE DocId = @DocId AND SiteId = @SiteId ORDER BY BSN DESC";
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(text))
                {
                    if (reader.Read())
                    {
                        return (long)reader[0];
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        /// <summary>
        /// 读取Content中指定Size的数据到传进的流中
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="internalVersion"></param>
        /// <param name="size"></param>
        /// <param name="dataStream"></param>
        [QueryReview("2012/05/03", "Kexin Guo")]
        public void BeginReadBufferEx(Guid siteId, Guid itemId, int internalVersion, long size, Stream dataStream)
        {
            int count = 100 * 1024;
            byte[] temp = new byte[count];
            long mPosition = 0;

            string commandGetContent = @"
                SELECT
                    ADS.Content 
                FROM
                    DocStreams AS ADS WITH (NOLOCK,INDEX=AllDocStreams_CI)
                WHERE
                    SiteId = @SiteId AND Id = @Id AND InternalVersion = @InternalVersion";
            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = commandGetContent;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    cmd.Parameters.AddWithValue("@InternalVersion", internalVersion);
                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            while (mPosition < size)
                            {
                                int rs = (int)dr.GetBytes(0, mPosition, temp, 0, count);
                                mPosition += rs;

                                dataStream.Write(temp, 0, rs);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
        #endregion

        #region RBS related methods (Not review yet)

        /// <summary>
        /// 无API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="blob_num"></param>
        /// <param name="blobStoreId"></param>
        /// <returns></returns>
        public AveRBSStubInfo AveRBSBackup_BackupRBSStub(int collectionId, long blob_num, short blobStoreId)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandText = AveRBSCommon.CMD_FETCH_RBS_BLOBID_AND_POOLID;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_number", blob_num);
                    cmd.Parameters.AddWithValue("@client_version", 0);
                    cmd.Parameters.Add(new SqlParameter("@blob_store_id", SqlDbType.SmallInt));
                    cmd.Parameters.Add(new SqlParameter("@store_pool_id", SqlDbType.VarBinary, 255));
                    cmd.Parameters.Add(new SqlParameter("@store_blob_id", SqlDbType.VarBinary, 255));
                    cmd.Parameters.Add(new SqlParameter("@create_time", SqlDbType.SmallDateTime));
                    cmd.Parameters.Add(new SqlParameter("@length", SqlDbType.BigInt));
                    cmd.Parameters["@blob_store_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@store_pool_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@store_blob_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@create_time"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@length"].Direction = ParameterDirection.Output;
                    //cmd.Parameters.Add(new SqlParameter("@returnValue", SqlDbType.Int)).Direction = ParameterDirection.ReturnValue;

                    int i = cmd.ExecuteNonQuery();

                    short temProviderId = (short)(cmd.Parameters["@blob_store_id"].Value);
                    if (temProviderId != blobStoreId)
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_QueryService_NotGenerateRBSStub);
                    byte[] tem_blobId = cmd.Parameters["@store_blob_id"].Value as byte[];
                    byte[] tem_poolId = cmd.Parameters["@store_pool_id"].Value as byte[];
                    long dataLen = (long)(cmd.Parameters["@length"].Value);

                    AveRBSStubInfo stubInfo = new AveRBSStubInfo(tem_blobId, tem_poolId, AveRBSCommon.RBS_PROVIDER_NAME_SP2013, dataLen);
                    return stubInfo;
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 无API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="rbs_id"></param>
        /// <returns></returns>
        public long AveRBSBackup_GenerateBlobNumber(byte[] rbs_id)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_number]";
                    cmd.Parameters.AddWithValue("@blob_id", rbs_id);
                    cmd.Parameters.Add(new SqlParameter("@blob_num", SqlDbType.BigInt));
                    cmd.Parameters["@blob_num"].Direction = ParameterDirection.ReturnValue;

                    object x = cmd.ExecuteScalar();
                    return (long)(cmd.Parameters["@blob_num"].Value);
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        /// <summary>
        /// 无API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="stubinfo"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        /// <returns></returns>
        public long AveRBSBackup_WriteBlobInformationToDB(AveRBSStubInfo stubinfo, int collectionId, short blobStoreId)
        {
            long blobNum = -1;
            long blobSize = stubinfo.DataLength;
            if (blobSize == 0)
                return -1;
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_sp_register_blob]";
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", stubinfo.StorePoolId);
                    cmd.Parameters.AddWithValue("@store_blob_id", stubinfo.StoreBlobId);
                    cmd.Parameters.AddWithValue("@create_time", DateTime.Now.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@length", blobSize);
                    cmd.Parameters.AddWithValue("@client_version", 0);

                    cmd.Parameters.Add("@blob_number", SqlDbType.BigInt);
                    cmd.Parameters["@blob_number"].Direction = ParameterDirection.Output;

                    object x = cmd.ExecuteScalar();
                    blobNum = (long)cmd.Parameters["@blob_number"].Value;
                }
            }
            catch (SqlException queryException)
            {
                //由于可能在插入STUB的过程中破坏mssqlrbs_resources.rbs_internal_blobs的unique index 'rbs_internal_blobs_ix_orphan_scan'，因此，如果出现这样的错误
                //我们应该获取已存在的这条STUB的Blob_Number并利用它生成一个RbsId返回给调用者，这样，将会出现有两个或者多个DocStreams中的记录拥有同一个
                //RBS Stub的情况，也就是有多个DocStreams中的记录有着相同的RbsId。
                if (queryException.ToString().Contains(@"Cannot insert duplicate key row in object 'mssqlrbs_resources.rbs_internal_blobs' with unique index 'rbs_internal_blobs_ix_orphan_scan'.") || queryException.Number == 2601)
                {
                    return AveRBSExtenderRestore_GetBlobNumber(stubinfo, blobStoreId);
                }
                else
                    throw new AveQueryException(queryException);
            }
            catch (Exception ex)
            {
                throw new AveQueryException(ex.Message, ex);
            }
            return blobNum;
        }

        /// <summary>
        /// 无API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="poolId"></param>
        /// <param name="canStoreNewBlobs"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        public void AveRBSExtenderRestore_CreatePool(byte[] poolId, bool canStoreNewBlobs, int collectionId, short blobStoreId)
        {

            using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
            {
                try
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_sp_add_pool]";
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", poolId);
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@client_version", 0);
                    cmd.Parameters.Add("@pool_id", SqlDbType.Int);
                    cmd.Parameters["@pool_id"].Direction = ParameterDirection.Output;
                    object x = cmd.ExecuteScalar();
                    int poolIndex = (int)cmd.Parameters["@pool_id"].Value;

                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@BlobStoreId", blobStoreId);
                    mQueryWorker.AddParameter("@StorePoolId", poolId);
                    mQueryWorker.AddParameter("@PoolId", poolIndex);
                    mQueryWorker.AddParameter("@CanStoreNewBlobs", canStoreNewBlobs);
                    mQueryWorker.AddParameter("@CloseTime", DateTime.Now);
                    string commandText = @"UPDATE [mssqlrbs_resources].[rbs_internal_pools] 
        SET [can_store_new_blobs]=@CanStoreNewBlobs,[close_time]=@CloseTime 
        WHERE [blob_store_id]=@BlobStoreId AND [store_pool_id]=@StorePoolId AND [pool_id]=@PoolId";
                    mQueryWorker.ExecuteNonQuery(commandText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveWrapperBaseException(e, AveInternalResourceKey.Wrapper_Exception_QueryService_CreateArchivePoolFailed, collectionId, blobStoreId, e.Message);
                }
            }
        }

        /// <summary>
        /// 无API实现
        /// </summary>
        /// <returns></returns>
        [QueryReview("2012/05/10", "Fengfu Zhang")]
        public int[] AveRBSCommon_GetCollectionIdAndProviderId()
        {
            int[] temId = new int[2];
            string commandText = @"SELECT collection_id FROM [mssqlrbs_resources].[rbs_internal_collections] WITH(NOLOCK) WHERE owning_application=@CollectionName";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@CollectionName", AveRBSCommon.COLLECTION_OWNING_APPLICATION);
            temId[0] = (int)mQueryWorker.ExecuteScalar(commandText);

            commandText = @"SELECT blob_store_id FROM [mssqlrbs_resources].[rbs_internal_blob_stores] WITH(NOLOCK) WHERE blob_store_name=@ProviderName";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ProviderName", AveRBSCommon.RBS_PROVIDER_NAME_SP2013);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sdr.Read())
                    {
                        temId[1] = sdr.GetInt16(0);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return temId;
        }

        /// <summary>
        /// 无API实现 
        /// </summary>
        /// <returns></returns>
        [QueryReview("2012/05/10", "Fengfu Zhang")]
        public List<Guid> AveRBSCommon_GetPoolsOfDB()
        {
            List<Guid> temList = new List<Guid>();
            try
            {
                byte[] poolIdBinary = null;
                string commandText = @"SELECT store_pool_id FROM [mssqlrbs_resources].[rbs_internal_pools] WITH(NOLOCK)";
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText))
                {
                    while (reader.Read() && !reader.IsDBNull(0))
                    {
                        poolIdBinary = (byte[])reader.GetValue(0);
                        Guid poolGuid = AveRBSCommon.GetPoolGuid(poolIdBinary);
                        if (!temList.Contains(poolGuid))
                        {
                            temList.Add(poolGuid);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetPoolsOfDBError, ex);
                temList = null;
            }
            return temList;
        }

        /// <summary>
        /// 无API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="storePoolId"></param>
        /// <param name="storeBlobId"></param>
        /// <param name="createTime"></param>
        /// <param name="blobSize"></param>
        /// <returns></returns>
        public long AveRBSConnectorRestore_RegisterBlob(int collectionId, int blobStoreId, byte[] storePoolId, byte[] storeBlobId, DateTime createTime, long blobSize)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Command)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "mssqlrbs.rbs_sp_register_blob";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@returnValue", SqlDbType.Int)).Direction = ParameterDirection.Output;

                    cmd.Parameters.AddWithValue("@collection_id", collectionId).SqlDbType = SqlDbType.Int;
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId).SqlDbType = SqlDbType.SmallInt;
                    cmd.Parameters.AddWithValue("@store_pool_id", storePoolId).SqlDbType = SqlDbType.VarBinary;
                    cmd.Parameters.AddWithValue("@store_blob_id", storeBlobId).SqlDbType = SqlDbType.VarBinary;
                    cmd.Parameters.AddWithValue("@create_time", createTime).SqlDbType = SqlDbType.SmallDateTime;
                    cmd.Parameters.AddWithValue("@length", blobSize).SqlDbType = SqlDbType.BigInt;

                    cmd.Parameters.Add(new SqlParameter("@blob_number", SqlDbType.BigInt)).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    int returnValue = Convert.ToInt32(cmd.Parameters["@returnValue"].Value, CultureInfo.InvariantCulture);
                    switch (returnValue)
                    {
                        case 0:
                            return Convert.ToInt64(cmd.Parameters["@blob_number"].Value);
                        default:
                            throw new Exception("Unexpected return value.");
                    }
                    throw new Exception("Unexpected stored procedure return code.");
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }


        /// <summary>
        /// 无API实现
        /// </summary>
        /// <param name="storePoolId"></param>
        /// <param name="storeBlobId"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <param name="blobNumber"></param>
        /// <returns></returns>
        [QueryReview("2012/05/10", "Fengfu Zhang", true, "use rbs_internal_blobs_ix_orphan_scan index for rbs_internal_blobs table")]
        public bool AveRBSConnectorRestore_CheckBlobExist(byte[] storePoolId, byte[] storeBlobId, int blobStoreId, int collectionId, ref long blobNumber)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@store_pool_id", storePoolId).SqlDbType = SqlDbType.VarBinary;
            mQueryWorker.AddParameter("@store_blob_id", storeBlobId).SqlDbType = SqlDbType.VarBinary;
            mQueryWorker.AddParameter("@blob_store_id", blobStoreId);
            mQueryWorker.AddParameter("@collection_id", collectionId);

            StringBuilder builder = new StringBuilder();
            builder.Append("Select [blob_number] From ");
            builder.Append("[mssqlrbs_resources].[rbs_internal_blobs] WITH(NOLOCK)");
            builder.Append(" Where ");
            builder.Append("[blob_store_id]");
            builder.Append("=");
            builder.Append("@blob_store_id");
            builder.Append(" AND ");
            builder.Append("[store_pool_id]");
            builder.Append("=");
            builder.Append("@store_pool_id");
            builder.Append(" AND ");
            builder.Append("[store_blob_id]");
            builder.Append("=");
            builder.Append("@store_blob_id");
            builder.Append(" AND ");
            builder.Append("collection_id");
            builder.Append("=");
            builder.Append("@collection_id");
            try
            {
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(builder.ToString()))
                {
                    if (reader.Read())
                    {
                        blobNumber = reader.GetInt64(0);
                        return true;
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            return false;
        }
        #endregion

        /// <summary>
        /// 获取Document的DocFlags
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetDocFlag(AveBaseItemInfo info)
        {
            string cmdText = @"SELECT DISTINCT DocFlags
                                    FROM AllDocs WITH(NOLOCK)
                                    WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
            if (info.ParentId != Guid.Empty)
            {
                cmdText += " ParentID=@ParentID AND ";
            }
            else
            {
                if (info.ItemType != AveItemType.Attachement)
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
            }
            cmdText += " Id=@Id AND UIVersion=@UIVersion ";
            cmdText += @"   UNION
                                    SELECT     DocFlags
                                    FROM         AllDocVersions WITH(NOLOCK)
                                    WHERE     (SiteId = @SiteId) AND (Id = @ID) AND (UIVersion = @UIVersion)";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ID", info.GUID);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@UIVersion", info.Version);
            mQueryWorker.AddParameter("@ParentID", info.ParentId);
            object result = mQueryWorker.ExecuteScalar(cmdText);
            if (result is int)
            {
                return (int)result;
            }
            return 0;
        }

        public List<AveShredInfo> GetShredInfo(AveBaseItemInfo info)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetShredInfo"))
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@HISTVersion", info.IsVersion ? info.Version : 0);

                const string cmd = @"select DT.Partition,DT.BSN from DocsToStreams as DT
                                 where DT.SiteId=@SiteId
                                 and DT.DocId=@Id
                                 and DT.HistVersion =@HISTVersion
                                 and DT.Level=@Level";

                SqlDataReader reader = null;
                List<AveShredInfo> shredInfos = new List<AveShredInfo>();
                try
                {
                    reader = mQueryWorker.ExecuteReader(cmd);
                    while (reader.Read())
                    {
                        shredInfos.Add(new AveShredInfo() { Partition = reader.GetByte(0), BSN = reader.GetInt64(1) });
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                return shredInfos;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HistVersion is the parameter of sql statement. ")]
        [QueryReview("SOInte-001")]
        public void ClearDocsToStreamsAndDocStreams(Guid siteId, Guid DocId, int HistVersion, byte level, bool clearDocStreams)
        {
            string cmd = "Delete from DocsToStreams where siteId = @SiteId and DocId = @DocId and HistVersion = @HistVersion And Level = @Level";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DocId", DocId);
            //always 0 是否有bug？
            mQueryWorker.AddParameter("@HistVersion", 0);
            mQueryWorker.AddParameter("@Level", level);

            mQueryWorker.ExecuteNonQuery(cmd);

            if (clearDocStreams)
            {
                cmd = "Delete from DocStreams where siteId= @SiteId and DocId = @DocId";

                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        public IAveQueryDataReader GetRBSIdOrContentOfOneShred(AveBaseItemInfo info, AveShredInfo shredInfo)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetRBSIdOrContentOfOneShred"))
            {
                string cmdText = @"SELECT RBSId,Size,Content FROM DocStreams ds WITH(NOLOCK) WHERE ds.SiteId=@SiteId AND ds.DocId=@Id AND ds.Partition = @Partition AND ds.BSN=@BSN";
                mQueryWorker.ClearParameters();

                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@BSN", shredInfo.BSN);
                mQueryWorker.AddParameter("@Partition", shredInfo.Partition);

                var reader = mQueryWorker.ExecuteReader(cmdText, CommandBehavior.SequentialAccess);

                if (reader.Read())
                {
                    shredInfo.RBSId = reader.IsDBNull(0) ? null : reader.GetSqlBinary(0).Value;
                    return new AveQueryDataReader(reader);
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
        }

        #region RBS Utility (Since we don't support SO integration for now, not review yet.)

        /// <summary>
        /// 获取Site下的RbsCollectionId和Blob_Store_Id
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int[] GetCollectionIdAndProviderId(Guid siteId)
        {
            return mQuerySessionSchema.GetCollectionIdAndProviderId(siteId);
        }

        /// <summary>
        /// 还原RBS stub
        /// 无API实现
        /// </summary>
        /// <param name="stubinfo"></param>
        /// <param name="poolsOfDB"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public byte[] RestoreRBSStub(AveRBSStubInfo stubinfo, List<Guid> poolsOfDB, short blobStoreId, int collectionId)
        {
            byte[] rbs_id = null;

            //如果要还原的STUB的PoolId在当前的DB中不存在，则添加一个PoolId到这个DB
            if (!poolsOfDB.Contains(AveRBSCommon.GetPoolGuid(stubinfo.StorePoolId)))
            {
                CreatePool(stubinfo.StorePoolId, false, blobStoreId, collectionId);
            }
            //还原RBS的STUB，如果成功，则返回一个大于0的整数；
            long blobNumber = WriteBlobInformationToDB(stubinfo, blobStoreId, collectionId);
            if (blobNumber == -1)
            {
                throw new Exception("Failed to generate blob record.");
            }

            rbs_id = GenerateRbsId(blobNumber, collectionId);
            return rbs_id;
        }




        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HistVersion is the parameter of the sql statement. ")]
        public void InsertDocsToStreams(Guid siteId, Guid docId, AveRBSStubInfo13 aveRBSStubInfo13, bool isCheckOut, byte level)
        {
            string cmd = @"INSERT INTO [dbo].[DocsToStreams] ([SiteId],[DocId],[HistVersion],[Level],[Partition],[BSN],[StreamId]) VALUES
           (@SiteId,@DocId,@HistVersion,@Level,@Partition,@BSN,@StreamId)";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DocId", docId);
            //always 0?
            mQueryWorker.AddParameter("@HistVersion", 0);
            //mQueryWorker.AddParameter("@Level", isCheckOut ? 255 : aveRBSStubInfo13.level);
            mQueryWorker.AddParameter("@Level", level);
            mQueryWorker.AddParameter("@Partition", aveRBSStubInfo13.partition);
            mQueryWorker.AddParameter("@BSN", aveRBSStubInfo13.BSN);
            mQueryWorker.AddParameter("@StreamId", aveRBSStubInfo13.streamId);

            mQueryWorker.ExecuteNonQuery(cmd);
        }

        public void InsertDocStreams(Guid siteId, Guid docId, AveRBSStubInfo13 aveRBSStubInfo13)
        {
            string cmd = @"if not exists (select * from DocStreams where SiteId= @SiteId and DocId= @DocId and Partition = @Partition and BSN = @BSN)
                            begin
           INSERT INTO [dbo].[DocStreams]
           ([DocId]
           ,[SiteId]
           ,[Partition]
           ,[BSN]
           ,[Size]
           ,[Content]
           ,[RbsId]
           ,[Type]
           ,[ExpirationUTC])
     VALUES
           (@DocId
           ,@SiteId
           ,@Partition
           ,@BSN
           ,@Size
           ,null
           ,@RbsId
           ,@Type
           ,null)
            End";


            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DocId", docId);
            mQueryWorker.AddParameter("@Size", aveRBSStubInfo13.size);
            mQueryWorker.AddParameter("@Type", aveRBSStubInfo13.type);
            mQueryWorker.AddParameter("@Partition", aveRBSStubInfo13.partition);
            mQueryWorker.AddParameter("@BSN", aveRBSStubInfo13.BSN);

            mQueryWorker.AddParameterWithType("@RbsId", SqlDbType.VarBinary);
            if (aveRBSStubInfo13.newRBSId != null)
            {
                mQueryWorker.SetParameterValue("@RbsId", aveRBSStubInfo13.newRBSId);
            }
            else
            {
                mQueryWorker.SetParameterValue("@RbsId", DBNull.Value);
            }

            mQueryWorker.ExecuteNonQuery(cmd);
        }

        public void UpdateContentNative13(List<AveShredStubInfo> shredInfoList, Guid siteId, Guid DocId, Stream stream)
        {
            string cmd = @"UPDATE ADS 
SET Content.write(@streamBuffer,NULL,NULL)
FROM DocStreams AS ADS WITH (INDEX(DocStreams_CI)) 
WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition and BSN=@BSN";

            string cmdText = @"UPDATE ADS 
SET Content=0x,RbsId=NULL 
FROM DocStreams AS ADS WITH (INDEX(DocStreams_CI)) 
WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition and BSN=@BSN";

            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@DocId", DocId);
            mQueryWorker.AddParameterWithType("@Partition", SqlDbType.TinyInt);
            mQueryWorker.AddParameterWithType("@BSN", SqlDbType.BigInt);
            mQueryWorker.AddParameterWithType("@streamBuffer", SqlDbType.VarBinary);

            foreach (var shredInfo in shredInfoList)
            {
                //It's Content
                if (shredInfo.RBSInfo.RBSId == null)
                {
                    mQueryWorker.SetParameterValue("@Partition", shredInfo.RBSInfo.partition);
                    mQueryWorker.SetParameterValue("@BSN", shredInfo.RBSInfo.BSN);
                    mQueryWorker.SetParameterValue("@streamBuffer", DBNull.Value);

                    mQueryWorker.ExecuteNonQuery(cmdText);

                    int sizeToRead = shredInfo.RBSInfo.size;

                    byte[] streamBuffer = new byte[8080];
                    int hasRead;
                    while ((hasRead = stream.Read(streamBuffer, 0, Math.Min(8080, sizeToRead))) > 0)
                    {
                        if (hasRead == streamBuffer.Length)
                        {
                            mQueryWorker.SetParameterValue("@streamBuffer", streamBuffer);
                        }
                        else
                        {
                            var tempBuff = new byte[sizeToRead];
                            Array.Copy(streamBuffer, 0, tempBuff, 0, sizeToRead);
                            mQueryWorker.SetParameterValue("@streamBuffer", tempBuff);
                        }

                        mQueryWorker.ExecuteNonQuery(cmd);

                        sizeToRead -= hasRead;
                        if (sizeToRead == 0) { break; }
                    }
                }
            }
        }


        #endregion

        public void UpdateEBSStubByNative(Guid siteId, Guid parentId, Guid docId, int uiVersion, AveStorageInfo storageInfo, byte[] content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function will be replaced by the other one called GetRbsIdListByNative for SP2013 in the future.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/05/02", "Kexin Guo")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public byte[] GetRbsIdByNative(AveBaseItemInfo info)
        {
            string cmdText = @"select RbsId from DocStreams ds with(nolock)
                                inner join DocsToStreams dts with(nolock) on ds.SiteId = dts.SiteId and ds.DocId = dts.DocId and ds.Partition = dts.Partition and ds.BSN = dts.BSN
                               where ds.SiteId = @SiteId and ds.DocId = @ID and dts.HistVersion = @HistVersion and dts.Level = @Level and RbsId is not null";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ID", info.GUID);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            //Can not use these condition because we haven't fetched them yet!
            mQueryWorker.AddParameter("@HistVersion", info.IsVersion ? info.Version : 0);
            mQueryWorker.AddParameter("@Level", info.Level);

            //Just return the first one to identify if this is a stub.
            return mQueryWorker.ExecuteScalar(cmdText) as byte[];
        }

        /// <summary>
        /// To return multiple rbsid for SP2013s
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HistVersion is the parameter of sql statement. ")]
        [QueryReview("2012/05/02", "Kexin Guo")]
        public List<AveRBSStubInfo13> GetRbsIdListByNative(AveBaseItemInfo info)
        {
            string cmdText = @"select RbsId,ds.BSN,ds.Partition,ds.Size,ds.Type,ds.ExpirationUTC,dts.StreamId from DocStreams ds with(nolock)
                                inner join DocsToStreams dts with(nolock) on ds.SiteId = dts.SiteId and ds.DocId = dts.DocId and ds.Partition = dts.Partition and ds.BSN = dts.BSN
                               where ds.SiteId = @SiteId and ds.DocId = @ID and dts.HistVersion = @HistVersion and dts.Level = @Level";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ID", info.GUID);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@HistVersion", info.IsVersion ? info.Version : 0);
            mQueryWorker.AddParameter("@Level", info.Level);
            List<AveRBSStubInfo13> stubInfoList = new List<AveRBSStubInfo13>();
            using (var reader = mQueryWorker.ExecuteReader(cmdText))
            {
                while (reader.Read())
                {
                    AveRBSStubInfo13 stubInfo = new AveRBSStubInfo13();
                    stubInfo.histVersion = info.IsVersion ? info.Version : 0;
                    stubInfo.level = (byte)info.Level;
                    stubInfo.BSN = (long)reader.GetValue(1);
                    stubInfo.partition = (byte)reader.GetValue(2);
                    stubInfo.RBSId = reader.IsDBNull(0) ? null : reader.GetValue(0) as byte[];
                    stubInfo.size = reader.GetInt32(3);
                    stubInfo.type = reader.GetByte(4);
                    //stubInfo.ExpirationUTC = reader.GetSqlDateTime(5).Value;
                    stubInfo.streamId = reader.GetInt64(6);
                    stubInfoList.Add(stubInfo);
                }
            }
            return stubInfoList;
        }

        /// <summary>
        /// Extender 获取Blob number
        /// 无API实现
        /// </summary>
        /// <param name="stubInfo"></param>
        /// <param name="blobStoreId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/11", "Sujie Cao")]
        private long AveRBSExtenderRestore_GetBlobNumber(AveRBSStubInfo stubInfo, short blobStoreId)
        {
            long blobNum = -1;
            string cmdStr = @"SELECT blob_number FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH(NOLOCK)
WHERE blob_store_id=@blob_store_id AND store_pool_id=@store_pool_id AND store_blob_id=@store_blob_id";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@blob_store_id", blobStoreId);
                mQueryWorker.AddParameter("@store_pool_id", stubInfo.StorePoolId);
                mQueryWorker.AddParameter("@store_blob_id", stubInfo.StoreBlobId);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdStr))
                {
                    if (dr.Read())
                        blobNum = dr.GetInt64(0);
                }
            }
            catch (SqlException queryException)
            {
                Console.WriteLine(new AveQueryException(string.Format("Exception Error Code----{0}", queryException.Number), queryException).ToString());
            }
            catch (Exception ex)
            {//log here
                Console.WriteLine(ex.ToString());
            }
            return blobNum;
        }


        /// <summary>
        /// 获取特定Stub信息
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "This should be changed for SP2013 in the future.")]
        public string GetStubInfoByNative(Guid siteId, Guid id, int internalVersion)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@InternalVersion", internalVersion);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", id);
            string cmdText = "SELECT DATALENGTH(Content),Content FROM DocStreams WITH(NOLOCK) WHERE SiteId=@SiteId AND DocId=@Id AND InternalVersion=@InternalVersion";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int length = (int)dr.GetInt64(0);
                        byte[] buffer = new byte[length];
                        dr.GetBytes(1, 0, buffer, 0, length);
                        return Encoding.UTF8.GetString(buffer);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

    }
}
