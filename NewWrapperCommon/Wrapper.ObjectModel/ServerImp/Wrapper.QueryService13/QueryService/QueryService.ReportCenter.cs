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



namespace AvePoint.Wrapper.QueryService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using System.Data.SqlClient;
    using System.Data;
    using AvePoint.GCommon.Utility;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal partial class AveQueryService : IAveReportCenterQueryService
    {
        private static readonly DateTimeOffset UsageDataStartTime = new DateTimeOffset(2007, 11, 1, 0, 0, 0, new TimeSpan(0L));
        private static readonly int MaxInClauseLength = 5000;

        private static string GetInClause(List<Guid> values)
        {
            if (values == null)
            {
                return null;
            }
            if (values.Count == 0)
            {
                return "";
            }
            var inParams = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                inParams[i] = string.Format("'{0}'", values[i]);
            }
            return string.Format("({0})", string.Join(",", inParams));
        }

        private static string GetInClause(List<int> values)
        {
            if (values == null)
            {
                return null;
            }
            if (values.Count == 0)
            {
                return "";
            }
            var inParams = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                inParams[i] = string.Format("{0}", values[i]);
            }
            return string.Format("({0})", string.Join(",", inParams));
        }

        #region Blob Raw Data
        [OnlyForSP2010]
        [Obsolete("Please use GetBlobRawDataSP2010SubFoldersAndItems instead")]
        public IAveQueryDataReader GetBlobRawDataSP2010UnderFolder(Guid siteId, string folderId)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"with RBS_doc(id)
as(
select distinct Id from DocStreams(nolock) where Content is null and RbsId is not null
)
select AllDocs.Id,(case when (DocFlags&65536<>65536 and RBS_doc.id is null) then 0 else 1 end) as flag,DirName,LeafName,Size,
ExtensionForFile,UIVersion ,Level,IsCurrentVersion,Type,HasStream
 from  AllDocs(nolock)
left join RBS_doc on AllDocs.Id=RBS_doc.id
where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId";
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@ParentId", folderId);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        [OnlyForSP2010]
        [Obsolete("Please use GetBlobRawDataSP2010Versions instead")]
        public IAveQueryDataReader GetBlobRawDataSP2010Version(Guid siteId, string id)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"select Id,UIVersion,Level,Size from AllDocVersions (nolock) where SiteId=@SiteId and Id=@Id  and DeleteTransactionId=0x";
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@Id", id);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        [OnlyForSP2010]
        [Obsolete("Please use GetBlobRawDataSP2010UserInfo instead")]
        public IAveQueryDataReader GetBlobRawDataSP2010Info(Guid siteId, Guid listId, string docId, int uiVersion, int isCurrentVersion)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = @"select userInfo1.tp_Title as Creator,userInfo1.tp_Login as CreatorLoginName,userInfo2.tp_Title as Modifier,
userInfo2.tp_Login as ModifierLoginName,AllUserData.tp_Created,AllUserData.tp_Modified,AllUserData.tp_Size from AllUserData (nolock)
left join UserInfo (nolock) as userInfo1 on AllUserData.tp_Author=userInfo1.tp_ID and userInfo1.tp_SiteId=@SiteId 
left join UserInfo (nolock) as userInfo2 on userInfo2.tp_ID=AllUserData.tp_Editor and userInfo2.tp_SiteId=@SiteId 
where tp_ListId=@ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion=@IsCurrentVersion
and tp_DocId=@DocId and tp_UIVersion=@uiVersion";
                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@ListId", listId);
                cmd.Parameters.AddWithValue("@DocId", docId);
                cmd.Parameters.AddWithValue("@IsCurrentVersion", isCurrentVersion);
                cmd.Parameters.AddWithValue("@uiVersion", uiVersion);

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        //取某个folder下对应的AllDocs表的数据。可能包括document和它的小version
        [OnlyForSP2010]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        public IAveQueryDataReader GetBlobRawDataSP2010SubFoldersAndItems(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetBlobRawDataSP2010SubFoldersAndItems"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.Id,UIVersion,Level,(case when (DocFlags&65536<>65536 and RbsId is null) then 0 else 1 end) as Flag,Size,data.tp_Author,data.tp_Editor,data.tp_Created,data.tp_Modified
,DirName,LeafName,ExtensionForFile,IsCurrentVersion
from  AllDocs doc(nolock)
inner join AllUserData data(nolock) on data.tp_SiteId = doc.SiteId and data.tp_DeleteTransactionId=0x and data.tp_IsCurrentVersion=1 and data.tp_ParentId=doc.ParentId and data.tp_DocId=doc.Id and data.tp_CalculatedVersion=0 and data.tp_Level=doc.Level
inner join DocStreams stream(nolock) on doc.SiteId=stream.SiteId and doc.Id=stream.id and doc.InternalVersion=stream.InternalVersion
where doc.SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and Type=0 and HasStream=1;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        //取某个文件对应的version信息
        [OnlyForSP2010]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        public IAveQueryDataReader GetBlobRawDataSP2010Versions(Guid siteId, Guid parentId, Guid id)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetBlobRawDataSP2010Versions"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select version.Id,UIVersion,Level,(case when (DocFlags&65536<>65536 and stream.RbsId is null) then 0 else 1 end) as Flag,Size,data.tp_Author,data.tp_Editor,data.tp_Created,data.tp_Modified
from AllDocVersions version(nolock) 
inner join AllUserData data(nolock) on data.tp_SiteId = version.SiteId and data.tp_DeleteTransactionId=0x and data.tp_IsCurrentVersion=0 and data.tp_ParentId=@ParentId and data.tp_DocId=version.Id and data.tp_CalculatedVersion=version.UIVersion and data.tp_Level=version.Level
inner join DocStreams stream(nolock) on version.SiteId=stream.SiteId and version.Id=stream.id and version.InternalVersion=stream.InternalVersion
where version.SiteId=@SiteId and version.Id=@DocId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@DocId", id);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [BothSP10AndSP13]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        [QueryReview("2012/12/13", "hyyin", true, "Add with nolock")]
        public IAveQueryDataReader GetBlobRawDataSP2010UserInfo(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetBlobRawDataSP2010UserInfo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select tp_ID,tp_Login,tp_Title from UserInfo with(nolock) where tp_SiteId=@SiteId;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [OnlyForSP2010]
        public IAveQueryDataReader GetBlobRawDataSP2010Attachments(Guid siteId, Guid attachmentFolderId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetBlobRawDataSP2010Attachments"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.Id,doc.UIVersion,doc.Level,(case when (doc.DocFlags&65536<>65536 and RbsId is null) then 0 else 1 end) as Flag,doc.Size,doc.MetaInfo
,doc.DirName,doc.LeafName,doc.ExtensionForFile,doc.TimeCreated,doc.TimeLastModified
from AllDocs parent(nolock)
inner join AllDocs doc(nolock) on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=parent.Id and doc.type=0 and doc.HasStream=1
inner join DocStreams stream(nolock) on doc.SiteId=stream.SiteId and doc.Id=stream.id and doc.InternalVersion=stream.InternalVersion
where parent.SiteId=@SiteId and parent.DeleteTransactionId=0x and parent.ParentId=@AttachmentFolderId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@AttachmentFolderId", attachmentFolderId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion

        #region Blob Raw Data SP2013

        //获取alldocs中的document所有version所对应的块的信息
        [QueryReview("2012/12/13", "hyyin")]
        public IAveQueryDataReader BlobRawDataSP2013Documents(Guid siteId, Guid parentId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobRawDataSP2013Documents"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select allData.tp_DocId,docs2Streams.Partition,docs2Streams.BSN,docs.ExtensionForFile,docs.DirName,docs.LeafName,allData.tp_UIVersion,docs.Level,allData.tp_Author,allData.tp_Editor,allData.tp_Created,allData.tp_Modified,allData.tp_IsCurrent, allData.tp_UIVersionString from AllUserData(nolock) allData
inner join AllDocs(nolock) docs
on docs.SiteId=@SiteId and docs.DeleteTransactionId=0x and docs.ParentId=@ParentId and docs.Id=@DocId and docs.Level=allData.tp_Level
inner join DocsToStreams(nolock) docs2Streams
on docs2Streams.SiteId=@SiteId and docs2Streams.DocId=@DocId and docs2Streams.HistVersion=allData.tp_CalculatedVersion and docs2Streams.Level=allData.tp_Level
where allData.tp_SiteId=@SiteId and allData.tp_DeleteTransactionId=0x and allData.tp_IsCurrentVersion in (0,1) and allData.tp_ParentId=@ParentId and allData.tp_DocId=@DocId
";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        /// <summary>
        /// For SP2013,get attachment infos from Alldocs table.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="attachmentsFolderId"></param>
        /// <returns></returns>
        public IAveQueryDataReader BlobRawDataAttachmentInfos(Guid siteId, Guid attachmentsFolderId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobRawDataAttachmentInfos"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", attachmentsFolderId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select doc.Id,doc.UIVersion,doc.UIVersionString,doc.Level,doc.MetaInfo,doc.ExtensionForFile,doc.DirName,doc.LeafName,doc.TimeCreated,doc.TimeLastModified from AllDocs(nolock) parent
inner join AllDocs(nolock) doc
on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.IsCurrentVersion=1 and doc.ParentId=parent.Id
where parent.SiteId=@SiteId and parent.DeleteTransactionId=0x and parent.IsCurrentVersion=1 and parent.ParentId=@ParentId;
";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        //取Attachment的所有块的信息
        [QueryReview("2012/12/13", "hyyin")]
        public IAveQueryDataReader BlobRawDataSP2013Attachments(Guid siteId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobRawDataSP2013Attachments"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select streams.Size,streams.RbsId,streams.Partition,streams.BSN from DocsToStreams(nolock) docs2Streams
inner join DocStreams(nolock) streams
on streams.SiteId=@SiteId and streams.DocId=@DocId and streams.Partition=docs2Streams.Partition and streams.BSN=docs2Streams.BSN
where docs2Streams.SiteId=@SiteId and docs2Streams.DocId=@DocId and docs2Streams.HistVersion=0 and docs2Streams.Level=1
";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion

        #region Blob Calculator SP2010
        /// <summary>
        /// 返回Docs表中IsCurrentVersion=1的记录，每个文件只统计一次
        /// </summary>
        [OnlyForSP2010]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        public IAveQueryDataReader BlobSP2010Documents(string aveSiteId, string parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010Documents"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", aveSiteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select Size,TimeCreated,TimeLastModified 
from AllDocs doc(nolock)
inner join DocStreams stream(nolock) on stream.SiteId=doc.SiteId and stream.Id=doc.Id and stream.InternalVersion=doc.InternalVersion
where doc.SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and Type=0 and IsCurrentVersion=1 and HasStream=1 and (DocFlags&65536<>65536 and RbsId is null)";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        /// <summary>
        /// 返回Version表中的记录，会统计Docs表中IsCurrentVersion=0的记录
        /// </summary>
        [OnlyForSP2010]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        public IAveQueryDataReader BlobSP2010DocumentVersions(string aveSiteId, string parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010DocumentVersions"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", aveSiteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select Size, data.tp_Created as TimeCreated, data.tp_Modified as TimeLastModified
from AllDocVersions version(nolock)
inner join AllUserData data(nolock) on data.tp_SiteId = version.SiteId and data.tp_DeleteTransactionId=0x and data.tp_IsCurrentVersion=0 and data.tp_ParentId=@ParentId and data.tp_DocId=version.Id and data.tp_CalculatedVersion=version.UIVersion and data.tp_Level=version.Level
inner join DocStreams stream(nolock) on stream.SiteId=version.SiteId and stream.Id=version.Id and stream.InternalVersion=version.InternalVersion
where version.SiteId=@SiteId and DeleteTransactionId=0x and (DocFlags&65536<>65536 and RbsId is null)
union all
select Size,TimeCreated,TimeLastModified
from AllDocs doc(nolock)
inner join DocStreams stream(nolock) on stream.SiteId=doc.SiteId and stream.Id=doc.Id and stream.InternalVersion=doc.InternalVersion
where doc.SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and Type=0 and IsCurrentVersion=0 and HasStream=1 and (DocFlags&65536<>65536 and RbsId is null)
;";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        /// <summary>
        /// Attachment与Item要分开取，不取view
        /// </summary>
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        [OnlyForSP2010]
        public IAveQueryDataReader BlobSP2010ListItems(Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010ListItems"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select tp_Size as Size,tp_Created as TimeCreated,tp_Modified as TimeLastModified from AllUserData (nolock)
where tp_ListId=@ListId and
tp_DeleteTransactionId=0x and
tp_IsCurrentVersion=1";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        /// <summary>
        /// Parent的Parent是Attachments这个folder的记录对应的就是attachments
        /// </summary>
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        [OnlyForSP2010]
        public IAveQueryDataReader BlobSP2010ListItemVersions(Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010ListItemVersions"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select tp_Size as Size,tp_Created as TimeCreated,tp_Modified as TimeLastModified from AllUserData (nolock)
where tp_ListId=@ListId and
tp_DeleteTransactionId=0x and
tp_IsCurrentVersion=0";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader BlobSP2010GetItemSize(Guid siteId, Guid listId)
        {
            throw new NotImplementedException();
        }

        #region Blob Inventory SP2010
        [BothSP10AndSP13]
        [DoNotNeedReview("Need further confirm:RC won't use this in further")]
        [QueryReview("2012/05/09", "Sid You", true, "Rewrite")]
        public IAveQueryDataReader BlobInventorySP2010GetSOInfo(Guid aveSiteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobInventorySP2010"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", aveSiteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select doc.Id,UIVersion,Size,Level,DeleteTransactionId from AllDocs doc(nolock)
inner join DocStreams stream(nolock) on doc.SiteId=stream.SiteId and doc.Id=stream.id and doc.InternalVersion=stream.InternalVersion
where doc.SiteId=@SiteId and doc.ParentId=@ParentId and type=0 and HasStream=1 and (DocFlags&65536=65536 or RbsId is not null)
union all
select version.Id, version.UIVersion,version.Size,version.Level,version.DeleteTransactionId from AllDocs doc(nolock)
inner join AllDocVersions version(nolock) on version.SiteId=doc.SiteId and version.Id=doc.Id
inner join DocStreams stream(nolock) on version.SiteId=stream.SiteId and version.Id=stream.id and version.InternalVersion=stream.InternalVersion
where doc.SiteId=@SiteId and doc.ParentId=@ParentId and doc.IsCurrentVersion=1 and (version.DocFlags&65536=65536 or RbsId is not null)
;";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public List<byte[]> BlobInventorySP2010GetDeleteTransactionId(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobInventorySP2010GetDeleteTransactionId"))
            {

                List<byte[]> result = new List<byte[]>();
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select EffectiveDeleteTransactionId from RecycleBin (nolock) where SiteId=@SiteId and BinId=1 and WebId=@WebId;";
                    using (IAveQueryDataReader reader = new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd)))
                    {
                        while (reader.Read())
                        {
                            byte[] deleteTransactionId = (byte[])reader["EffectiveDeleteTransactionId"];
                            result.Add(deleteTransactionId);
                        }
                        return result;
                    }
                }

            }

        }

        #endregion Blob Inventory SP2010
        [BothSP10AndSP13]
        [DoNotNeedReview("Need further confirm:RC won't use this in further")]
        public IAveQueryDataReader BlobInventorySP2010GetSOInfoInList(Guid aveSiteId, Guid attachmentFolderId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobInventorySP2010GetSOInfoInList"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", aveSiteId);
                    cmd.Parameters.AddWithValue("@ParentId", attachmentFolderId);
                    cmd.CommandText = @"
select doc.Id,doc.UIVersion,doc.Level,doc.Size,doc.DeleteTransactionId
from AllDocs parent(nolock)
inner join AllDocs doc(nolock) on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=parent.Id and doc.type=0 and doc.HasStream=1
inner join AllDocStreams stream(nolock) on doc.SiteId=stream.SiteId and doc.Id=stream.id and doc.InternalVersion=stream.InternalVersion
where parent.SiteId=@SiteId and parent.DeleteTransactionId=0x and parent.ParentId=@ParentId and (doc.DocFlags&65536=65536 or stream.RbsId is not null);";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/13", "hyyin")]
        public Guid BlobInventorySP2010GetAttachmentsFolder(Guid aveSiteId, Guid rootFolderId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobInventorySP2010GetAttachmentsFolder"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    Guid attachmentsFolderID = Guid.Empty;
                    cmd.CommandText = @"
                    SELECT Id FROM AllDocs(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND
                    LeafName='Attachments' AND Level=1;";

                    cmd.Parameters.AddWithValue("@SiteId", aveSiteId);
                    cmd.Parameters.AddWithValue("@ParentId", rootFolderId);
                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        attachmentsFolderID = (Guid)obj;
                    }
                    return attachmentsFolderID;
                }

            }

        }
        [OnlyForSP2010]
        public Dictionary<Guid, bool> GetRootFoldersAndBaseTypeIncludeDeleted(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetRootFoldersIncludeDeleted"))
            {

                Dictionary<Guid, bool> result = new Dictionary<Guid, bool>();
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select tp_RootFolder,tp_BaseType from AllLists(nolock) where tp_WebId= @WebId;";
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid rootFolderId = new Guid(reader["tp_RootFolder"].ToString());
                            bool isLibrary = Convert.ToInt32(reader["tp_BaseType"]) == 1;
                            result.Add(rootFolderId, isLibrary);
                        }
                    }
                }
                return result;

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin")]
        public long GetDocumentVersionSize(Guid siteId, Guid parentId)
        {
            long docVersionSize = 0;
            long docSize = 0;

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocumentVersionSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
                        select sum(convert(bigint,isnull(AllDocVersions.size,0))) from AllDocVersions with (nolock) inner join 
                        AllDocs with (nolock) on AllDocVersions.id = AllDocs.id 
                        where AllDocVersions.siteid =@SiteId and AllDocs.SiteId=@SiteId
                        and AllDocs.IsCurrentVersion=1 and AllDocs.ParentId=@parentId and AllDocs.DeleteTransactionId=0x";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);

                    object docVersionSizeResult = mQueryWorker.ExecuteScalar(cmd);
                    if (docVersionSizeResult != DBNull.Value)
                    {
                        docVersionSize = Convert.ToInt64(docVersionSizeResult);
                    }

                    cmd.CommandText = @"
                        select sum(convert(bigint,isnull(size,0))) from AllDocs with(nolock)
                        where SiteId=@SiteId and DeleteTransactionId=0x 
                        and IsCurrentVersion=0 and AllDocs.ParentId=@parentId";

                    object docSizeResult = mQueryWorker.ExecuteScalar(cmd);
                    if (docSizeResult != DBNull.Value)
                    {
                        docSize = Convert.ToInt64(docSizeResult);
                    }

                    return docVersionSize + docSize;


                }


            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/13", "hyyin")]
        public int GetNumberOfFileTypes(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetNumberOfFileTypes"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {

                    cmd.CommandText = @"
                        select COUNT(distinct Extension) from AllDocs with(nolock) 
                        where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId
                        and Extension <>''";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    return Convert.ToInt32(mQueryWorker.ExecuteScalar(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader GetAlertItemByAlertId(string alertId, bool isImme)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetAlertItemByAlertId"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@AlertId", alertId);
                    cmd.CommandType = CommandType.Text;
                    if (isImme)
                    {
                        cmd.CommandText = @"select DirName as Url, LeafName as Title from AllDocs(nolock)inner join ImmedSubscriptions(nolock) 
on AllDocs.ID= ImmedSubscriptions.itemdocid and AllDocs.Level>0 and ImmedSubscriptions.Id=@AlertId";
                    }
                    else
                    {
                        cmd.CommandText = @"select DirName as Url, LeafName as Title from AllDocs(nolock)inner join SchedSubscriptions(nolock) 
on AllDocs.ID= SchedSubscriptions.itemdocid and AllDocs.Level>0 and SchedSubscriptions.Id=@AlertId";
                    }
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }
        [OnlyForSP2010]
        public long GetContentTypeUsageCountInList(Guid listId, byte[] ctbytes)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetContentTypeUsageCountInList"))
            {
                long count = 0;
                try
                {
                    using (SqlCommand cmd = mQueryWorker.CreateCommand())
                    {
                        cmd.CommandText = @"select COUNT(1) from AllUserData(nolock)
                            where tp_ListId=@ListId and tp_DeleteTransactionId=0x
                            and tp_IsCurrentVersion=1 and tp_IsCurrent=1 and tp_ContentTypeId=@ContentTypeId";
                        cmd.Parameters.AddWithValue("@ListId", listId);
                        cmd.Parameters.AddWithValue("@ContentTypeId", ctbytes);
                        count = Convert.ToInt32(cmd.ExecuteScalar());
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
                return count;
            }
        }

        public long GetContentTypeUsageCountInList(Guid siteId, Guid listId, byte[] ctbytes)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetContentTypeUsageCountInList"))
            {
                long count = 0;
                try
                {
                    using (SqlCommand cmd = mQueryWorker.CreateCommand())
                    {
                        cmd.CommandText = @"select COUNT(1) from AllUserData(nolock)
                            where tp_ListId=@ListId and tp_DeleteTransactionId=0x
                            and tp_IsCurrentVersion=1 and tp_IsCurrent=1 
                            and tp_SiteId = @SiteId
                            and tp_ContentTypeId=@ContentTypeId";
                        cmd.Parameters.AddWithValue("@ListId", listId);
                        cmd.Parameters.AddWithValue("@ContentTypeId", ctbytes);
                        cmd.Parameters.AddWithValue("@SiteId", siteId);
                        count = Convert.ToInt32(cmd.ExecuteScalar());
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
                return count;
            }
        }

        public IAveQueryDataReader BlobSP2010GetListInfo(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010GetListInfo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT tp_RootFolder,list.tp_ID,tp_BaseType FROM AllLists(NOLOCK) list 
                            INNER JOIN AllWebs (nolock) web 
                                ON list.tp_WebId = web.Id AND list.tp_DeleteTransactionId = 0x 
                            WHERE web.SiteId = @SiteId AND web.DeleteTransactionId = 0x";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader BlobSP2010Documents(Guid siteId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader BlobSP2010DocumentVersions(Guid siteId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader BlobInventorySP2010GetSOInfo(Guid siteId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader BlobSP2010GetItemSize(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010GetItemSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT SUM(CAST(ISNULL(tp_Size, 0) AS bigint)) AS Size FROM AllUserData (NOLOCK) " +
                                        "WHERE tp_SiteId = @SiteId AND tp_DeleteTransactionId = 0x";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion

        #region Blob Calculator SP2013
        [DoNotNeedReview]
        [Obsolete("This function has been replaced by the one which needs three parameters.")]
        public IAveQueryDataReader BlobSP2013DocumentCurrentVersion(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013DocumentCurrentVersion"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
  select Id,DocStreams.Partition,DocStreams.BSN,AllDocs.Size as FileRealSize,DocStreams.Size as BSNSize,DocStreams.RbsId,
  TimeCreated,TimeLastModified,AllDocs.UIVersion from AllDocs (nolock)
  inner join DocsToStreams (nolock) on AllDocs.SiteId= DocsToStreams.SiteId and 
  AllDocs.Id=DocsToStreams.DocId and DocsToStreams.HistVersion=0
  and AllDocs.Level=DocsToStreams.Level
  inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId
  and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
  where AllDocs.SiteId=@SiteId and AllDocs.DeleteTransactionId=0x
  and ParentId=@ParentId and AllDocs.IsCurrentVersion=1
  and AllDocs.Type=0 and RbsId is null";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [NotUsedAttribute]
        public IAveQueryDataReader BlobSP2013DocumentCurrentVersion(Guid siteId, Guid parentId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013DocumentCurrentVersion"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@Id", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
  select Id,DocStreams.Partition,DocStreams.BSN,AllDocs.Size as FileRealSize,DocStreams.Size as BSNSize,DocStreams.RbsId,
  TimeCreated,TimeLastModified,AllDocs.UIVersion from AllDocs (nolock)
  inner join DocsToStreams (nolock) on AllDocs.SiteId= DocsToStreams.SiteId and 
  AllDocs.Id=DocsToStreams.DocId and DocsToStreams.HistVersion=0
  and AllDocs.Level=DocsToStreams.Level
  inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId
  and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
  where AllDocs.SiteId=@SiteId and AllDocs.DeleteTransactionId=0x
  and ParentId=@ParentId and AllDocs.Id=@Id and AllDocs.IsCurrentVersion=1
  and AllDocs.Type=0 and RbsId is null";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/13", "hyyin")]
        public IAveQueryDataReader BlobSP2013DocVersionInfo(Guid siteId, Guid parentId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013DocVersionInfo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select Id,UIVersion,AllDocs.Level,DocStreams.BSN,DocStreams.Partition,AllDocs.Size as FileRealSize,DocStreams.Size as BSNSize,
AllDocs.TimeCreated,AllDocs.TimeLastModified 
from AllDocs (nolock) 
inner join DocsToStreams (nolock) on  AllDocs.SiteId=DocsToStreams.SiteId and AllDocs.Id=DocsToStreams.DocId
and HistVersion=0 and AllDocs.Level=DocsToStreams.Level and AllDocs.IsCurrentVersion=0
inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId 
and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
where AllDocs.SiteId=@SiteId  and AllDocs.DeleteTransactionId=0x
and ParentId=@ParentId and Id=@DocId and AllDocs.IsCurrentVersion=0
and AllDocs.Type=0 and RbsId is null
union all
select Id,AllUserData.tp_UIVersion,AllUserData.tp_Level,DocStreams.BSN, DocStreams.Partition,AllDocVersions.Size as FileRealSize,DocStreams.Size as BSNSize,
AllUserData.tp_Created as TimeCreated,AllUserData.tp_Modified as TimeLastModified from AllDocVersions  (nolock)
inner join AllUserData (nolock) on AllDocVersions.SiteId=AllUserData.tp_SiteId and AllUserData.tp_DeleteTransactionId=0x
and (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) and tp_ParentId=@ParentId and tp_DocId=AllDocVersions.Id
and AllDocVersions.UIVersion=AllUserData.tp_UIVersion
inner join DocsToStreams (nolock) on AllUserData.tp_SiteId= DocsToStreams.SiteId and 
AllUserData.tp_DocId=DocsToStreams.DocId and DocsToStreams.HistVersion=AllUserData.tp_CalculatedVersion
and AllUserData.tp_Level=DocsToStreams.Level
inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId
and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
where AllDocVersions.SiteId=@SiteId and AllDocVersions.Id=@DocId
and RbsId is null";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013DocAllVersionInfo(Guid siteId, Guid parentId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013DocAllVersionInfo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select Id,UIVersion,AllDocs.Level,DocStreams.BSN,DocStreams.Partition,AllDocs.Size as FileRealSize,DocStreams.Size as BSNSize,
AllDocs.TimeCreated,AllDocs.TimeLastModified 
from AllDocs (nolock) 
inner join DocsToStreams (nolock) on  AllDocs.SiteId=DocsToStreams.SiteId and AllDocs.Id=DocsToStreams.DocId
and HistVersion=0 and AllDocs.Level=DocsToStreams.Level 
inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId 
and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
where AllDocs.SiteId=@SiteId  and AllDocs.DeleteTransactionId=0x
and ParentId=@ParentId and Id=@DocId 
and AllDocs.Type=0 and RbsId is null
union all
select Id,AllUserData.tp_UIVersion,AllUserData.tp_Level,DocStreams.BSN, DocStreams.Partition,AllDocVersions.Size as FileRealSize,DocStreams.Size as BSNSize,
AllUserData.tp_Created as TimeCreated,AllUserData.tp_Modified as TimeLastModified from AllUserData  (nolock)
inner join AllDocVersions  (nolock) on AllUserData.tp_SiteId=AllDocVersions.SiteId 
and AllUserData.tp_DocId=AllDocVersions.Id and AllUserData.tp_UIVersion=AllDocVersions.UIVersion
inner join DocsToStreams (nolock) on AllUserData.tp_SiteId= DocsToStreams.SiteId and 
AllUserData.tp_DocId=DocsToStreams.DocId and DocsToStreams.HistVersion=AllDocVersions.UIVersion
and AllUserData.tp_Level=DocsToStreams.Level
inner join DocStreams (nolock) on DocsToStreams.SiteId=DocStreams.SiteId and DocsToStreams.DocId=DocStreams.DocId
and DocsToStreams.Partition=DocStreams.Partition and DocsToStreams.BSN=DocStreams.BSN
where AllUserData.tp_SiteId=@SiteId and AllUserData.tp_DeleteTransactionId=0x and 
(AllUserData.tp_IsCurrentVersion=0 or AllUserData.tp_IsCurrentVersion=1) and AllUserData.tp_ParentId=@ParentId 
and AllUserData.tp_DocId=@DocId and AllUserData.tp_IsCurrent=0
and RbsId is null";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013ItemsInList(Guid siteId, Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013ItemsInList"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select tp_Size as Size,tp_Created as TimeCreated,tp_Modified as TimeLastModified from AllUserData (nolock)
where tp_SiteId=@SiteId and tp_ListId=@ListId and
tp_DeleteTransactionId=0x and
tp_IsCurrentVersion=1 and tp_IsCurrent=1";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013ListItemVersions(Guid siteId, Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2010ListItemVersions"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select tp_Size as Size,tp_Created as TimeCreated,tp_Modified as TimeLastModified from AllUserData (nolock)
where tp_SiteId=@SiteId and tp_ListId=@ListId and
tp_DeleteTransactionId=0x and
tp_IsCurrent=0";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        public IAveQueryDataReader BlobSP2013AllDocBSN(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013AllDocBSN"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT stream.Size,DocId,Partition,BSN,RbsId FROM AllDocs(NOLOCK) doc
                        INNER JOIN DocStreams stream(NOLOCK) ON 
                            doc.SiteId = stream.SiteId AND doc.Id = stream.DocId AND doc.IsCurrentVersion = 1 AND doc.ListId is not null
                        WHERE doc.SiteId = @SiteId AND doc.DeleteTransactionId = 0x AND doc.IsCurrentVersion = 1
                            AND doc.ListId is not null";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion

        #region Storage Trends

        #region New Logic

        /// <summary>
        /// 该方法返回Folder.SubFolder
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="parentId">Parent ID</param>
        /// <returns></returns>
        [BothSP10AndSP13]
        [QueryReview("2012/05/11", "Sid You", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public List<Guid> GetSubFolders(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSubFolders"))
            {

                List<Guid> ids = new List<Guid>();
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select Id from AllDocs with(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and Type=1 and IsCurrentVersion=1;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader.GetGuid(0));
                        }
                    }
                }
                return ids;

            }

        }
        [OnlyForSP2010]
        public List<Guid> GetSubFoldersIncludeDeleted(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSubFoldersIncludeDeleted"))
            {

                List<Guid> ids = new List<Guid>();
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select Id from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId>=0x and ParentId=@ParentId and Type=1 and IsCurrentVersion=1;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader.GetGuid(0));
                        }
                    }
                }
                return ids;

            }

        }

        /// <summary>
        /// 该方法返回web下所有list的root folder，包括被删除的List
        /// </summary>
        /// <param name="webId">Web ID</param>
        /// <returns>Dictionary<ListId,RootFolderId></returns>
        [OnlyForSP2010]
        public List<Guid> GetRootFoldersIncludeDeleted(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetRootFoldersIncludeDeleted"))
            {

                List<Guid> result = new List<Guid>();
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select tp_RootFolder from AllLists(nolock) where tp_WebId=@WebId;";
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid rootFolderId = new Guid(reader["tp_RootFolder"].ToString());
                            result.Add(rootFolderId);
                        }
                    }
                }
                return result;

            }

        }

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
        [QueryReview("2012/05/11", "Sid You", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        [BothSP10AndSP13]
        public IAveQueryDataReader GetStaticInfoForLibraryFolder(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetStaticInfoForLibraryFolder"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
with CurrentVersionDoc as
(
  select doc.Id,doc.Type,doc.Extension from AllDocs doc(nolock)
  where doc.SiteId=@SiteId and doc.ParentId=@ParentId and doc.DeleteTransactionId=0x and doc.IsCurrentVersion=1 and DoclibRowId is not Null
)

select Extension, COUNT(*) ItemCount, SUM(VersionCount) VersionCount, MAX(VersionCount) MaxVersionCount,  SUM(Cast(Size AS BigInt)) Size
from CurrentVersionDoc with(nolock)
inner join
(
  select Id, COUNT(*) VersionCount,  SUM(Cast(Size AS BigInt)) Size from
     (
        select doc.Id, Size from AllDocs doc(nolock)
        where doc.SiteId=@SiteId and doc.ParentId=@ParentId and DeleteTransactionId=0x and Type=0 and DoclibRowId is not Null
        union all
        select doc.Id, version.Size from CurrentVersionDoc doc
        inner join AllDocVersions version(nolock) on version.SiteId=@SiteId and version.Id=doc.Id and version.DeleteTransactionId=0x
     ) as DocAndVersion
  group by Id
) as DocAndVersion on CurrentVersionDoc.Id=DocAndVersion.Id
group by Extension";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

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
        [QueryReview("2012/05/11", "Sid You", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        [BothSP10AndSP13]
        public IAveQueryDataReader GetStaticInfoForWebOrListFolder(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetStaticInfoForWebOrListFolder"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select Extension, COUNT(*) ItemCount, SUM(CAST(ISNULL(Size,0)as bigint)) Size from AllDocs (nolock)
where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and DoclibRowId is null and Type=0 
group by Extension;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

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
        [QueryReview("2012/05/11", "Sid You", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        [BothSP10AndSP13]
        public IAveQueryDataReader GetStaticInfoForListItems(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetStaticInfoForListItems"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
with ListItemInfo(VersionCount,Size) as(
select COUNT(*)  VersionCount, SUM(Cast(ISNULL(tp_Size,0)as bigint)) Size from AllUserData data (nolock)
where tp_SiteId=@SiteId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion in (0,1) and tp_ParentId=@ParentId
group by tp_DocId
)
select COUNT(*) ItemCount, SUM(VersionCount) VersionCount, MAX(VersionCount) MaxVersionCount, SUM(Size) Size
 from ListItemInfo with(nolock);";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion

        /// <summary>
        /// 获取Stub数据的大小
        /// 无API实现
        /// </summary>
        /// <returns></returns>
        [QueryReview("2012/05/14", "Sid You", true, "Rewrite")]
        [OnlyForSP2010]
        public long GetContentDBStubSize()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetContentDBStubSize"))
            {

                long stubSize = 0;
                try
                {
                    using (SqlCommand cmd = mQueryWorker.CreateCommand())
                    {
                        cmd.CommandText = @"
SELECT sum(cast(coalesce(Size,0) as bigint)) Size
from
(
Select sum(cast(Size as bigint)) Size FROM AllDocs doc(nolock)
where (DocFlags&65536)=65536
Union All
Select sum(cast(Size as bigint)) Size FROM AllDocVersions doc(nolock)
where (DocFlags&65536)=65536
Union All
SELECT sum(cast(Size as bigint)) Size FROM dbo.AllDocs doc(nolock)
inner join DocStreams stream(nolock) on doc.Id=stream.Id and doc.InternalVersion= stream.InternalVersion and stream.RbsId is not null
WHERE (DocFlags&65536)= 0
Union All
SELECT sum(cast(Size as bigint)) Size FROM dbo.AllDocVersions version(nolock)
inner join DocStreams stream(nolock) on version.SiteId=stream.SiteId and version.Id=stream.Id and version.InternalVersion= stream.InternalVersion and stream.RbsId is not null
WHERE (DocFlags&65536)= 0
) Tmp;";
                        stubSize = Convert.ToInt64(cmd.ExecuteScalar());
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
                return stubSize;

            }

        }

        #endregion

        #region last access time

        public IAveQueryDataReader GetSiteUsers(Guid siteId, SPUserFilter userFilter)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserById"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText =
                    @"select tp_ID,tp_Login,tp_Title from userinfo(nolock) where tp_siteid=@SiteId " + BuildUserFilterSqlString(userFilter);
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetDocIds(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocIdsOfWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText =
                    @"select Id from AllDocs(nolock) where SiteId=@SiteId and WebId=@WebId and IsCurrentVersion=1";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetListIds(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetListIds"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText =
                    @"select tp_ID from AllLists(nolock) where tp_SiteId=@SiteId and tp_WebId=@WebId";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetAuditData(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText =
                    @"select ItemId,ItemType,UserId,Occurred from auditdata(nolock) where siteid=@SiteId
and Occurred between @StartTime and @EndTime order by Occurred desc";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                    cmd.Parameters.Add(startTimeParameter);
                    SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                    cmd.Parameters.Add(endTimeParameter);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        public IAveQueryDataReader GetAuditData(string viewName, DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid siteId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUsageData"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select LogTime,SiteUrl,WebUrl,DocumentPath,ReferrerUrl,UserAgent, UserLogin,SiteId, WebId,HttpStatus,UserAddress from " + viewName + " WITH (nolock) where LogTime >= @LogTimeStart and LogTime <= @LogTimeEnd and WebId <> '00000000-0000-0000-0000-000000000000' and SiteId = @SiteId";
                    cmd.Parameters.AddWithValue("@LogTimeStart", startTimeToDisplay);
                    cmd.Parameters.AddWithValue("@LogTimeEnd", endTimeToDisplay);
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        [OnlyForSP2010]
        public IAveQueryDataReader GetLastAccessTimeOfWeb(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfWeb1"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {

                    cmd.CommandText =
                    @"SELECT TOP 1 UserInfo.tp_Login as UserName, 
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
                   and ItemType in(6, 7) "
                     + BuildUserFilterSqlString(userFilter)
                     + " ORDER BY Occurred DESC ";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                    cmd.Parameters.Add(startTimeParameter);
                    SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                    cmd.Parameters.Add(endTimeParameter);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader GetLastAccessTimeOfList(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfList1"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @" SELECT TOP 1 UserInfo.tp_Login as UserName, 
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
                       and ItemType = 4 "
                       + BuildUserFilterSqlString(userFilter)
                       + " ORDER BY Occurred DESC";


                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                    cmd.Parameters.Add(startTimeParameter);
                    SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                    cmd.Parameters.Add(endTimeParameter);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader GetLastAccessTimeOfItem(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfItem"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                   cmd.CommandText = @" SELECT TOP 1 UserInfo.tp_Login as UserName, 
                   UserInfo.tp_Title as DisplayName, 
                   AuditData.Occurred as Occurred
                   FROM AuditData (nolock)         
                   left join UserInfo (nolock) on tp_SiteId = AuditData.SiteId  
                   and UserInfo.tp_Id = UserId
		           inner join AllDocs (nolock) on AllDocs.Id = ItemId 
                   and level > 0
		           and AllDocs.WebId = @WebId
                   WHERE AuditData.SiteId=@SiteId
                   and Occurred > @startTime 
                   and Occurred < @endTime 
		           and ItemType in (1,3,5) "
                    + BuildUserFilterSqlString(userFilter)
                    + " ORDER BY Occurred DESC";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    SqlParameter startTimeParameter = new SqlParameter("@startTime", SqlDbType.DateTime);
                    startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                    cmd.Parameters.Add(startTimeParameter);
                    SqlParameter endTimeParameter = new SqlParameter("@endTime", SqlDbType.DateTime);
                    endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                    cmd.Parameters.Add(endTimeParameter);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetLastAccessTimeOfSite(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime, SPUserFilter userFilter)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfSite1"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {

                    cmd.CommandText =
                    @"SELECT TOP 1 UserInfo.tp_Login as UserName, 
                   UserInfo.tp_Title as DisplayName, 
                   AuditData.Occurred as Occurred,
                   AuditData.UserId as UserId 
                   FROM AuditData (nolock)         
                   left join UserInfo (nolock) on tp_SiteId = AuditData.SiteId  
                   and tp_Id = UserId
                   WHERE SiteId= @SiteId 
                   and Occurred > @StartTime 
                   and Occurred < @EndTime "
                     + BuildUserFilterSqlString(userFilter)
                     + " ORDER BY Occurred DESC ";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                    cmd.Parameters.Add(startTimeParameter);
                    SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                    cmd.Parameters.Add(endTimeParameter);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetLastAccessTimeOfSite(Guid siteId, DateTimeOffset startTime, DateTimeOffset endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfSite2"))
            {

            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {

                cmd.CommandText = AveQueryStringReportCenter13.GetLastAccessTimeOfSite_Select_UserInfo_AuditData;

                cmd.Parameters.AddWithValue("@SiteId", siteId);
                SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                cmd.Parameters.Add(startTimeParameter);
                SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                cmd.Parameters.Add(endTimeParameter);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }

            }

        }

        public IAveQueryDataReader GetLastAccessTimeOfWeb(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfWeb2"))
            {

            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {

                cmd.CommandText = AveQueryStringReportCenter13.GetLastAccessTimeOfWeb_Select_UserInfo_AuditData;

                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@WebId", webId);
                SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                cmd.Parameters.Add(startTimeParameter);
                SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                cmd.Parameters.Add(endTimeParameter);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }

            }

        }
        public IAveQueryDataReader GetLastAccessTimeOfList(Guid siteId, Guid webId, DateTimeOffset startTime, DateTimeOffset endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLastAccessTimeOfList2"))
            {

            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandText = AveQueryStringReportCenter13.GetLastAccessTimeOfList_Select_UserInfo_AuditData;


                cmd.Parameters.AddWithValue("@SiteId", siteId);
                cmd.Parameters.AddWithValue("@WebId", webId);
                SqlParameter startTimeParameter = new SqlParameter("@StartTime", SqlDbType.DateTime);
                startTimeParameter.Value = AveDateTimeUtility.ConvertToType007(startTime.UtcDateTime);
                cmd.Parameters.Add(startTimeParameter);
                SqlParameter endTimeParameter = new SqlParameter("@EndTime", SqlDbType.DateTime);
                endTimeParameter.Value = AveDateTimeUtility.ConvertToType007(endTime.UtcDateTime);
                cmd.Parameters.Add(endTimeParameter);
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }

            }

        }

        [OnlyForSP2010]
        public string BuildUserFilterSqlString(SPUserFilter userFilter)
        {
            if (userFilter == null)
            {
                return string.Empty;
            }

            StringBuilder inClause = new StringBuilder();

            switch (userFilter.userFilterType)
            {
                case UserFilterType.Include:
                    if (userFilter.Users != null && userFilter.Users.Count > 0)
                    {
                        inClause.Append("AND tp_Login IN (");
                        foreach (RCUserDetail user in userFilter.Users)
                        {
                            string loginName = user.LoginName.Replace("'", "''");
                            string spLoginName = user.SPLoginName.Replace("'", "''");
                            inClause.Append(string.Format("N'{0}', N'{1}',", loginName, spLoginName));
                        }
                        inClause.Remove(inClause.Length - 1, 1);
                        inClause.Append(")");
                    }
                    break;
                case UserFilterType.Exclude:
                    if (userFilter.Users != null && userFilter.Users.Count > 0)
                    {
                        inClause.Append("AND (tp_Login NOT IN (");
                        foreach (RCUserDetail user in userFilter.Users)
                        {
                            string loginName = user.LoginName.Replace("'", "''");
                            string spLoginName = user.SPLoginName.Replace("'", "''");
                            inClause.Append(string.Format("N'{0}', N'{1}',", loginName, spLoginName));
                        }
                        inClause.Remove(inClause.Length - 1, 1);
                        inClause.Append(") or tp_Login is null)");
                    }
                    break;
                default:
                    return string.Empty;
            }

            return inClause.ToString();

        }

        #endregion

        #region Admin Report

        [OnlyForSP2010]
        public IAveQueryDataReader AdminReportSP10GetListBlobData(Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetListBlobData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"with RBS_doc(id)
as(
select distinct Id from DocStreams(nolock) where Content is null and RbsId is not null
),
DocTmp(id,listid,webId)as
(
select distinct Id,ListId,WebId from AllDocs(nolock) where ListId=@ListId
)
select AllDocs.Id,UIVersion,Level,Size,ListId,WebId from AllDocs (nolock)
left join RBS_doc on AllDocs.Id=RBS_doc.id
where ListId =@ListId and DeleteTransactionId=0x and type=0 and (DocFlags&65536=65536 or RBS_doc.id is not null)
union all
select AllDocVersions.Id,UIVersion,Level,Size ,listId,webId from AllDocVersions (nolock)
left join RBS_doc on AllDocVersions.Id=RBS_doc.id
inner join DocTmp on AllDocVersions.Id=DocTmp.id
where DeleteTransactionId=0x and (DocFlags&65536=65536 or RBS_doc.id is not null)";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader AdminReportSP10GetWebBlobData(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetWebBlobData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"with RBS_doc(id)
as(
select distinct Id from DocStreams(nolock) where Content is null and RbsId is not null
),
DocTmp(id,listId,webId)as
(
select distinct Id,ListId,WebId from AllDocs(nolock) where WebId =@WebId 
)
select AllDocs.Id,UIVersion,Level,Size,ListId,WebId,SiteId from AllDocs (nolock)
left join RBS_doc on AllDocs.Id=RBS_doc.id
where WebId =@WebId and DeleteTransactionId=0x and type=0 and (DocFlags&65536=65536 or RBS_doc.id is not null)
union all
select AllDocVersions.Id,UIVersion,Level,ISNULL(Size,0),listId,webId,SiteId from AllDocVersions (nolock)
left join RBS_doc on AllDocVersions.Id=RBS_doc.id
inner join DocTmp on AllDocVersions.Id=DocTmp.id
where DeleteTransactionId=0x and (DocFlags&65536=65536 or RBS_doc.id is not null)";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [BothSP10AndSP13]
        [DoNotNeedReview("Index issue:will add new method for 13")]
        public IAveQueryDataReader AdminReportSP10GetWebRecycleBinSize(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetWebRecycleBinSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(Convert(bigint,ISNULL(Size,0))) AS Size from RecycleBin(nolock) where BinId=1 and WebId =@WebId";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [BothSP10AndSP13]
        public IAveQueryDataReader AdminReportSP13GetWebRecycleBinSize(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetWebRecycleBinSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(Convert(bigint,ISNULL(Size,0))) AS Size from RecycleBin(nolock) where SiteId=@SiteId and BinId=1 and WebId =@WebId";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public int AdminReportSP13GetListCountUnderWeb(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP13GetListCountUnderWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllLists(nolock) where tp_SiteId=@SiteId and tp_WebId=@WebId  and tp_DeleteTransactionId=0x";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }

            }

        }

        [BothSP10AndSP13]
        [DoNotNeedReview("Index issue:will add new method for 13")]
        public int AdminReportSP10GetListCountUnderWeb(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetListCountUnderWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllLists(nolock) where tp_WebId=@WebId  and tp_DeleteTransactionId=0x";
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public long AdminReportSP10GetSCRecycleBinSize(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetSCRecycleBinSize"))
            {

                long scRecycleBinSize = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(CAST(ISNULL(size,0)as bigint)) from RecycleBin (nolock) where SiteId=@SiteId and BinId=2";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    object sizeObj = cmd.ExecuteScalar();
                    if (sizeObj != DBNull.Value)
                    {
                        scRecycleBinSize += Convert.ToInt64(sizeObj);
                    }
                }
                return scRecycleBinSize;

            }

        }
        [OnlyForSP2010]
        public long AdminReportSP10GetAuditInfo(Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP10GetAuditInfo"))
            {

                long dataCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count(*) from AuditData(nolock) where ItemId in (select id from Docs(nolock) where Docs.webId = @WebId) or 
itemid in (select tp_id from Lists(nolock) where tp_webId=@WebId) or itemid = @WebId";
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    dataCount += Convert.ToInt64(cmd.ExecuteScalar());
                }
                return dataCount;

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public IAveQueryDataReader GetLibLastAndVersionSizeByParentId(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetLibLastAndVersionSizeByParentId"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select SUM(CAST(ISNULL(DocVersion.Size,0)as bigint))as Size,0 as IsCurrentVersion from AllDocs doc (nolock)
inner join AllDocVersions DocVersion (nolock) on doc.SiteId=DocVersion.SiteId and doc.Id=DocVersion.Id and doc.IsCurrentVersion=1
and DocVersion.DeleteTransactionId=0x
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId and Type=0
union all
select doc.Size,IsCurrentVersion from AllDocs doc(nolock)
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId and Type=0";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", false, "performance is not good enough")]
        public IAveQueryDataReader GetListLastAndVersionSizeByParentId(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetListLastAndVersionSizeByParentId"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(CAST(ISNULL(tp_Size,0)as bigint))as size,tp_IsCurrent from alldocs doc(nolock)
inner join AllUserData data(nolock) on tp_SiteId=@SiteId and tp_DeleteTransactionId=0x and 
(tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) and tp_ParentId=@ParentId and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1 and doc.Type=0 AND data.tp_level=doc.Level
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId and Type=0
group by tp_IsCurrent";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public IAveQueryDataReader GetLastAndVersionSizeForWebOrListFolder(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetListLastAndVersionSizeByParentId"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(Size) Size from AllDocs (nolock)
where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and DoclibRowId is null and Type=0 ";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #region new storage size logic method

        public IAveQueryDataReader GetSiteIdInContentDatabase(int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSiteIdInContentDatabase"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select Id from Sites(nolock)";
                    cmd.CommandTimeout = commandTimeout;
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public int GetWebCountInSite(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetWebCountInSite"))
            {

                int webCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from Webs(nolock) where SiteId=@SiteId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        webCount = Convert.ToInt32(objCount);
                    }
                    return webCount;
                }

            }

        }

        public int GetListCountInSite(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetListCountInSite"))
            {

                int listCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from Lists(nolock) where tp_WebId in (select Id from Webs(nolock) where SiteId=@SiteId)";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        listCount = Convert.ToInt32(objCount);
                    }
                    return listCount;
                }

            }

        }
        #region site level size
        public IAveQueryDataReader AdminReportGetWholeSiteVersionSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWholeSiteVersionSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,doc.ListId,ver.Size from AllDocs(nolock) doc
inner join AllDocVersions(nolock) ver on ver.SiteId=doc.SiteId  and ver.Id=doc.Id and ver.DeleteTransactionId=0x
and doc.IsCurrentVersion=1
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWholeSiteDocSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWholeSiteDocSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,ListId,Size,IsCurrentVersion from AllDocs(nolock) where SiteId=@SiteId
and DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWholeSiteItemSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWholeSiteItemSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,data.tp_ListId as ListId,tp_Size as Size,tp_IsCurrent as IsCurrentVersion from AllDocs(nolock) doc
inner join AllUserData(nolock) data on doc.SiteId=data.tp_SiteId and data.tp_DeleteTransactionId=0x
and (data.tp_IsCurrentVersion=0 or data.tp_IsCurrentVersion=1)  and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWholeSiteRecycleSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWholeSiteRecycleSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,listid,BinId,Size from RecycleBin(nolock) where SiteId=@SiteId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        //NotImplemented
        public IAveQueryDataReader AdminReportGetWholeSiteSOSize(Guid siteId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        //NotImplemented
        public IAveQueryDataReader AdminReportGetOnlySiteRBSSize(Guid siteId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader AdminReportGetOnlySiteRecycleSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetOnlySiteRecycleSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select BinId,ISNULL(Size,0) from RecycleBin(nolock) where SiteId=@SiteId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetOnlySiteItemSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetOnlySiteItemSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select tp_Size,tp_IsCurrent from AllUserData(nolock) where tp_SiteId=@SiteId and tp_DeleteTransactionId=0x ";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetOnlySiteDocSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetOnlySiteDocSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select Size,IsCurrentVersion,UIVersion,Level,ISNULL(DocFlags,0),id from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and size is not null";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetOnlySiteVersionSize(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetOnlySiteVersionSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select ISNULL(ver.Size,0),ver.UIVersion,ver.Level,ver.Id,DocFlags from AllDocVersions(nolock) ver
 where ver.SiteId=@SiteId and ver.DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportSP13GetListInfoUnderSite(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP13GetListInfoUnderSite"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select tp_ID,tp_WebId,tp_RootFolder,tp_BaseType from AllLists(nolock)
where tp_SiteId=@SiteId and tp_DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion

        #region web level size
        //NotImplemented
        public IAveQueryDataReader AdminReportGetWebSOSize(Guid siteId, Guid webId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader AdminReportGetWebRecycleSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebRecycleSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,listid,BinId,Size from RecycleBin(nolock) where SiteId=@SiteId and WebId=@WebId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWebItemSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebRecycleSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,data.tp_ListId as ListId,tp_Size as Size,tp_IsCurrent as IsCurrentVersion from AllDocs(nolock) doc
inner join AllUserData(nolock) data on doc.SiteId=data.tp_SiteId and data.tp_DeleteTransactionId=0x
and (data.tp_IsCurrentVersion=0 or data.tp_IsCurrentVersion=1)  and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1 and doc.WebId=@WebId
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.WebId=@WebId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWebDocSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebDocSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,ListId,Size,IsCurrentVersion from AllDocs(nolock) where SiteId=@SiteId
and DeleteTransactionId=0x and WebId=@WebId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetWebVersioniSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebDocSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,doc.ListId,ver.Id,ISNULL(ver.Size,0)as Size from AllDocs(nolock) doc
inner join AllDocVersions(nolock) ver on ver.SiteId=doc.SiteId  and ver.Id=doc.Id and ver.DeleteTransactionId=0x and doc.WebId=@WebId and doc.IsCurrentVersion=1
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.WebId=@WebId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportSP13GetListInfoUnderWeb(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportSP13GetListInfoUnderWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select tp_ID,tp_RootFolder,tp_BaseType from AllLists(nolock)
where tp_SiteId=@SiteId and tp_WebId=@WebId and tp_DeleteTransactionId=0x";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion

        #region list level size
        //NotImplemented
        public IAveQueryDataReader AdminReportGetListSOSize(Guid siteId, Guid webId, Guid listId, int commandTimeout)
        {
            throw new NotImplementedException();
        }

        public IAveQueryDataReader AdminReportGetListRecycleSize(Guid siteId, Guid webId, Guid listId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetListRecycleSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,listid,BinId,Size from RecycleBin(nolock) where SiteId=@SiteId and WebId=@WebId and ListId=@ListId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetListItemSize(Guid siteId, Guid webId, Guid listId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetListItemSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,data.tp_ListId as ListId,tp_Size as Size,tp_IsCurrent as IsCurrentVersion from AllDocs(nolock) doc
inner join AllUserData(nolock) data on doc.SiteId=data.tp_SiteId and data.tp_DeleteTransactionId=0x
and (data.tp_IsCurrentVersion=0 or data.tp_IsCurrentVersion=1)  and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1 and doc.WebId=@WebId and doc.ListId=@ListId
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.WebId=@WebId and doc.ListId=@ListId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetListDocSize(Guid siteId, Guid webId, Guid listId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetListDocSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select WebId,ListId,Size,IsCurrentVersion from AllDocs(nolock) where SiteId=@SiteId
and DeleteTransactionId=0x and WebId=@WebId and ListId=@ListId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetListVersionSize(Guid siteId, Guid webId, Guid listId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetListVersionSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select doc.WebId,doc.ListId,ver.Id,ISNULL(ver.Size,0) as Size from AllDocs(nolock) doc
inner join AllDocVersions(nolock) ver on ver.SiteId=doc.SiteId  and ver.Id=doc.Id and ver.DeleteTransactionId=0x and doc.WebId=@WebId and doc.ListId=@ListId and doc.IsCurrentVersion=1
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.WebId=@WebId and doc.ListId=@ListId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion

        #endregion

        #region SC/Web Usage Report
        public IAveQueryDataReader AdminReportGetTotalHits(Guid aggregationId, int startDate, int endDate, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTotalHits"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select 
(
select SUM(Frequency) from WAClickAggregationByDate with (NOLOCK)
where AggregationId = @AggregationId AND
IncludeSubSites = 1 )as AllTimeFrequency,
(
select SUM(Frequency) from WAClickAggregationByDate with (NOLOCK)
where AggregationId = @AggregationId AND
IncludeSubSites = 1 AND
DateId BETWEEN @StartDay AND @EndDay
)as LastMonthFrequency";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    cmd.Parameters.Add("@StartDay", startDate);
                    cmd.Parameters.Add("@EndDay", endDate);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetUsageTotalHits(Guid aggregationId, int startDate, int endDate, int timeNow, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetUsageTotalHits"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select 
(
select SUM(Frequency) from WAClickAggregationByDate with (NOLOCK)
where AggregationId = @AggregationId AND
IncludeSubSites = 1 )as AllTimeFrequency,
(
select SUM(Frequency) from WAClickAggregationByDate with (NOLOCK)
where AggregationId = @AggregationId AND
IncludeSubSites = 1 AND
DateId BETWEEN @StartDay AND @EndDay
)as LastMonthFrequency,
(
select top 1 SUM(Frequency) from WAClickAggregationByDate with (NOLOCK)
where AggregationId = @AggregationId AND
IncludeSubSites = 1 AND
DateId <@Now
group by DateId
order by DateId desc
)as LastRecentDay";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    cmd.Parameters.Add("@StartDay", startDate);
                    cmd.Parameters.Add("@EndDay", endDate);
                    cmd.Parameters.Add("@Now", timeNow);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenHitAllTime(Guid aggregationId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenHitAllTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
   top 10 Frequency,
    am.AssetId AS PageId
FROM
(
    SELECT
        ca.ClickedAssetIdHash,
        SUM(ca.Frequency) AS Frequency
    FROM WAClickAggregationByDate AS ca WITH (NOLOCK)
    WHERE
        ca.AggregationId = @AggregationId AND
        ca.IncludeSubSites = 1 
    GROUP BY
        ca.ClickedAssetIdHash
) AS ca
LEFT JOIN WAAssetMetadata AS am WITH (NOLOCK)
    ON am.AssetIdHash = ca.ClickedAssetIdHash
    order by ca.Frequency desc";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenHitLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenHitLastMonth"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
   top 10 Frequency,
    am.AssetId AS PageId
FROM
(
    SELECT
        ca.ClickedAssetIdHash,
        SUM(ca.Frequency) AS Frequency
    FROM WAClickAggregationByDate AS ca WITH (NOLOCK)
    WHERE
        ca.AggregationId = @AggregationId AND
        ca.IncludeSubSites = 1 AND
        ca.DateId BETWEEN @StartDay AND @EndDay
    GROUP BY
        ca.ClickedAssetIdHash
) AS ca
LEFT JOIN WAAssetMetadata AS am WITH (NOLOCK)
    ON am.AssetIdHash = ca.ClickedAssetIdHash
    order by ca.Frequency desc";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    cmd.Parameters.Add("@StartDay", startDate);
                    cmd.Parameters.Add("@EndDay", endDate);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenVisitorAllTime(Guid aggregationId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenVisitorAllTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    top 10 ISNULL(Frequency, 0) AS Frequency,
    UserId AS UserName
FROM (
    SELECT
        UserId, 
        SUM(Frequency) AS Frequency
    FROM WAVisitorAggregationByDate WITH (NOLOCK)
    WHERE
        AggregationId = @AggregationId AND
        IncludeSubSites = 1 
    GROUP BY
        UserId
) AS va
order by Frequency desc";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenVisitorLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenVisitorLastMonth"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    top 10 ISNULL(Frequency, 0) AS Frequency,
    UserId AS UserName
FROM (
    SELECT
        UserId, 
        SUM(Frequency) AS Frequency
    FROM WAVisitorAggregationByDate WITH (NOLOCK)
    WHERE
        AggregationId = @AggregationId AND
        IncludeSubSites = 1 AND
        DateId  BETWEEN @StartDay AND @EndDay
    GROUP BY
        UserId
) AS va
order by Frequency desc";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    cmd.Parameters.Add("@StartDay", startDate);
                    cmd.Parameters.Add("@EndDay", endDate);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenLeastHitAllTime(Guid aggregationId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenLeastHitAllTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
   top 10 Frequency,
    am.AssetId AS PageId
FROM
(
    SELECT
        ca.ClickedAssetIdHash,
        SUM(ca.Frequency) AS Frequency
    FROM WAClickAggregationByDate AS ca WITH (NOLOCK)
    WHERE
        ca.AggregationId = @AggregationId AND
        ca.IncludeSubSites = 1 
    GROUP BY
        ca.ClickedAssetIdHash
) AS ca
LEFT JOIN WAAssetMetadata AS am WITH (NOLOCK)
    ON am.AssetIdHash = ca.ClickedAssetIdHash
    order by ca.Frequency ";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportGetTopTenLeastHitLastMonth(Guid aggregationId, int startDate, int endDate, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetTopTenLeastHitAllTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
   top 10 Frequency,
    am.AssetId AS PageId
FROM
(
    SELECT
        ca.ClickedAssetIdHash,
        SUM(ca.Frequency) AS Frequency
    FROM WAClickAggregationByDate AS ca WITH (NOLOCK)
    WHERE
        ca.AggregationId = @AggregationId AND
        ca.IncludeSubSites = 1 AND
        ca.DateId BETWEEN @StartDay AND @EndDay
    GROUP BY
        ca.ClickedAssetIdHash
) AS ca
LEFT JOIN WAAssetMetadata AS am WITH (NOLOCK)
    ON am.AssetIdHash = ca.ClickedAssetIdHash
    order by ca.Frequency ";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.Add("@AggregationId", aggregationId);
                    cmd.Parameters.Add("@StartDay", startDate);
                    cmd.Parameters.Add("@EndDay", endDate);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        #endregion
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetContentDBSize(int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetContentDBSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"exec sp_spaceused";
                    cmd.CommandTimeout = commandTimeout;
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public string AdminReportGetFarmPersonalSiteLocation(Guid partitionID, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetFarmPersonalSiteLocation"))
            {

                string location = string.Empty;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select Inclusion from PersonalSite with(nolock) where PartitionID=@PartitionID";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@PartitionID", partitionID);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        location = (string)objCount;
                    }
                }
                return location;

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public long AdminReportGetSiteDiskUsed(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetSiteDiskUsed"))
            {

                long diskUsed = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select DiskUsed from Sites(nolock) where Id=@SiteId";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    diskUsed = Convert.ToInt64(cmd.ExecuteScalar());
                }
                return diskUsed;

            }

        }

        #region web general info
        [Obsolete("function invalid.")]
        public IAveQueryDataReader AdminReportGetWebFullControlUsers(string siteId, string webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebFullControlUsers"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select userinfo.tp_id as ID, userInfo.tp_Login as LoginName from (select SiteId, ScopeId from webs with (nolock) where id=@WebId) webs
                        inner join RoleAssignment with (nolock) on webs.ScopeId = RoleAssignment.ScopeId and webs.SiteId=RoleAssignment.SiteId
                        inner join roles with (nolock) on webs.SiteId = roles.SiteId and RoleAssignment.RoleId = roles.RoleID and roles.PermMask = 9223372036854775807
                        inner join userinfo with (nolock) on webs.SiteId = userinfo.tp_SiteId and RoleAssignment.PrincipalId = userinfo.tp_id
                        Union
                        select userinfo.tp_id as ID, userInfo.tp_Login as LoginName from (select SiteId, ScopeId from webs with (nolock) where id=@WebId) webs
                        inner join RoleAssignment with (nolock) on webs.ScopeId = RoleAssignment.ScopeId and webs.SiteId=RoleAssignment.SiteId
                        inner join roles with (nolock) on webs.SiteId = roles.SiteId and RoleAssignment.RoleId = roles.RoleID and roles.PermMask = 9223372036854775807
                        inner join groups with (nolock) on groups.siteId = webs.SiteId and groups.Id = RoleAssignment.PrincipalId
                        inner join groupMembership with (nolock) on webs.SiteId = groupMembership.SiteId and groupMembership.groupId = groups.Id
                        inner join userinfo with (nolock) on webs.SiteId = userinfo.tp_SiteId and groupMembership.MemberId = userinfo.tp_id
                        Union
                        select userinfo.tp_id as ID, userInfo.tp_Login as LoginName from userinfo with (nolock)
                        where userinfo.tp_siteId =@SiteId and userinfo.tp_siteadmin = 1";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public bool AdminReportIsOrphanSite(Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportIsOrphanSite"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select w1.Id from webs w1 with(nolock)
left join webs w2 with(nolock) on w1.ParentWebId=w2.Id
where w1.Id=@WebId and (w1.ParentWebId is null or w2.Id is null)";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    if (cmd.ExecuteScalar() == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public object AdminReportGetWebLastAccessedTime(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebLastAccessedTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select Max(Occurred) from
(
select Occurred from AuditData with(nolock)
inner join AllDocs doc with(nolock) on doc.SiteId=@SiteId and doc.Id=AuditData.ItemId  and doc.WebId=@WebId
union
select Occurred from AuditData with(nolock)
inner join AllLists list with(nolock) on list.tp_SiteId=@SiteId and list.tp_WebId=@WebId and list.tp_ID=AuditData.ItemId
where AuditData.SiteId=@SiteId
) temp";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    object time = cmd.ExecuteScalar();
                    if (time != DBNull.Value)
                    {
                        return time;
                    }
                    else
                    {
                        return null;
                    }
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public string AdminReportGetLastModifier(Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetLastModifier"))
            {

                string lastModifier = string.Empty;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select top 1 ModifiedBy from EventCache(nolock) where WebId=@WebId and ModifiedBy is not NULL order by EventTime desc";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    object modifierValue = cmd.ExecuteScalar();
                    if (modifierValue != DBNull.Value && modifierValue != null)
                    {
                        lastModifier = modifierValue.ToString();
                    }
                }
                return lastModifier;

            }

        }

        #endregion

        #region number info of List and Library
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfDocumentLibraries(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfDocumentLibraries"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count (tp_id) from Lists with (nolock) where tp_SiteId=@SiteId and tp_WebId=@WebId and tp_baseType=1 and tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfLists(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfLists"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count (tp_id) from Lists with (nolock) where tp_SiteId=@SiteId and tp_WebId=@WebId and tp_baseType=0 and tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfLitItems(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfLitItems"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from UserData with(nolock) inner join Lists with (nolock) 
on Lists.tp_SiteId=@SiteId and tp_ListId = Lists.tp_ID and Lists.tp_webId=@WebId and tp_baseType=0 and Lists.tp_Flags & 256 <= 0
where UserData.tp_SiteId=@SiteId and tp_IsCurrent = 1 ";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfDocuments(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfDocuments"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllDocs doc(nolock) 
		inner join AllLists list(nolock) on list.tp_WebId=@WebId and tp_BaseType=1  and list.tp_Flags & 256 <=0
		and doc.ListId=list.tp_ID 
		where SiteId=@SiteId and DeleteTransactionId=0x 
		and WebId=@WebId and type=0 and DoclibRowId is not null and IsCurrentVersion=1";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfAttachments(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfAttachments"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count(Id) from AllDocs with(nolock) where SiteId=@SiteId AND DeleteTransactionId=0x
and DirName like '%/attachments/%' and WebId=@WebId and DoclibRowId is null and ListId is not null";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetDocumentsTotalSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetDocumentsTotalSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select sum(convert(bigint,isnull(size,0))) from AllDocs with (nolock) inner join AllLists with(nolock)
on AllDocs.ListId = AllLists.tp_ID 
where SiteId=@SiteId and SetupPath is null and Size is not null and IsCurrentVersion=1 
and AllLists.tp_SiteId=@SiteId and AllLists.tp_webId=@WebId and tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetBigFile(Guid siteId, Guid webId, long fileSizeBytes, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetBigFile"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count(Id) from AllDocs with (nolock) inner join AllLists with(nolock)
on AllDocs.ListId = AllLists.tp_ID 
where SiteId=@SiteId and SetupPath is null and Size>@size and IsCurrentVersion=1 
and AllLists.tp_SiteId=@SiteId and AllLists.tp_webId=@WebId and tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@size", fileSizeBytes);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetListTotalSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetListTotalSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select sum(size) from (
select sum(convert(bigint,isnull(tp_Size,0))) as size from  AllUserData (nolock)
inner join AllLists(nolock) on AllUserData.tp_listid=alllists.tp_ID
where AllUserData.tp_SiteId=@siteId 
and AllLists.tp_SiteId=@siteId 
and AllLists.tp_WebId=@WebId 
and AllLists.tp_BaseType=0
union
select sum(convert(bigint,isnull(Size,0))) as size from AllDocs with(nolock) 
where SiteId=@siteId 
AND DeleteTransactionId=0x
and DirName like '%/attachments/%' 
and WebId=@WebId  
and DoclibRowId is null and ListId is not null
)ItemWithAttachmentSize";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfListPersonalView(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfListPersonalView"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(AllWebParts.tp_ID) from AllWebParts with (nolock) inner join AllLists(nolock) on AllWebParts.tp_ListId=AllLists.tp_ID
where AllWebParts.tp_SiteId=@SiteId and tp_BaseViewID is null and tp_UserID is not null 
and tp_PageVersion=0 and tp_DisplayName is not null
and AllLists.tp_SiteId=@SiteId and AllLists.tp_WebId=@WebId and AllLists.tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfListPublicView(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfListPublicView"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(AllWebParts.tp_ID) from AllWebParts with (nolock) inner join AllLists(nolock) on AllWebParts.tp_ListId=AllLists.tp_ID
where AllWebParts.tp_SiteId=@SiteId and tp_BaseViewID is null and tp_UserID is null 
and tp_PageVersion=0 and tp_DisplayName is not null
and AllLists.tp_SiteId=@SiteId and AllLists.tp_WebId=@WebId and AllLists.tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfDiscussionBoard(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfDiscussionBoard"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from Lists with (nolock) where tp_SiteId=@SiteId and tp_WebId=@WebId and tp_ServerTemplate = 108";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfDiscussionItem(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfDiscussionItem"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    //attachment被保存在library中
                    cmd.CommandText = @"
select COUNT(1) from AllUserData(nolock) data
inner join lists(nolock)  on lists.tp_siteId=@siteId and
 Lists.tp_ID=data.tp_ListId and Lists.tp_webId=@WebId  and tp_ServerTemplate = 108
where 
data.tp_SiteId=@siteId and data.tp_DeleteTransactionId=0x 
and Lists.tp_webId=@WebId  and tp_ServerTemplate = 108";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetDiscussionBoardTotalSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetDiscussionBoardTotalSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(size) from
(
select SUM(CAST(ISNULL(tp_size,0)as bigint)) as size from UserData with (nolock) inner join Lists with (nolock)
on tp_ListId = Lists.tp_ID 
where UserData.tp_SiteId=@SiteId and Lists.tp_SiteId=@SiteId and Lists.tp_webid=@webid  and tp_ServerTemplate = 108
union all
select SUM(CAST(ISNULL(size,0)as bigint)) as size from Docs with(nolock)inner join Lists with (nolock)
on ListId = Lists.tp_ID 
where Docs.SiteId=@SiteId and Lists.tp_SiteId=@SiteId and Lists.tp_webid=@webid and tp_ServerTemplate = 108
)data ";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader AdminReportSP13GetDiscussionBoardTotalSize(Guid siteId, Guid webId, int commandTimeout)
        {
            return AdminReportGetDiscussionBoardTotalSize(siteId, webId, commandTimeout);
        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfSurvey(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfSurvey"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from Lists with (nolock) where tp_SiteId=@SiteId and tp_webid=@webid and tp_ServerTemplate = 102 and tp_Flags & 256 <= 0";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetNumberOfSurveyResponse(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetNumberOfSurveyResponse"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from UserData with (nolock) inner join Lists with (nolock) 
on tp_ListId = Lists.tp_ID 
where UserData.tp_SiteId=@SiteId and Lists.tp_SiteId=@SiteId and Lists.tp_webid=@webid  and tp_ServerTemplate = 102";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetSurveyTotalSize(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetSurveyTotalSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SUM(tp_size) from UserData with (nolock) inner join Lists with (nolock) on tp_ListId = Lists.tp_ID 
where UserData.tp_SiteId=@SiteId AND Lists.tp_SiteId=@SiteId and Lists.tp_webid=@webid and tp_ServerTemplate = 102";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion

        [QueryReview("2013/04/4", "qwhu")]
        public int AdminReportGetPageNumInWeb(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetPageNumInWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count(id) from Docs with (nolock) where SiteId=@SiteId and WebId=@WebId and IsCurrentVersion=1 and extension='aspx'";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public int AdminReportGetCustomPageNumInWeb(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetCustomPageNumInWeb"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select count(id) from Docs with (nolock) where SiteId=@SiteId and WebId=@WebId and IsCurrentVersion=1 and(DocFlags&64=64) and extension='aspx'";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetWebContentAnalysis(Guid siteId, Guid webId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetWebContentAnalysis"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select ExtensionForFile,COUNT_BIG(ExtensionForFile)as N from AllDocs with(nolock)
where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and ExtensionForFile is not null 
and ExtensionForFile <>'' and IsCurrentVersion=1
group by ExtensionForFile";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@siteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetSCContentAnalysis(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetSCContentAnalysis"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select ExtensionForFile,COUNT_BIG(ExtensionForFile)as N from AllDocs with(nolock)
where SiteId=@SiteId and DeleteTransactionId=0x and ExtensionForFile is not null 
and ExtensionForFile <>'' and IsCurrentVersion=1
group by ExtensionForFile";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetSCLastAccessTime(Guid siteId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetSCLastAccessTime"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select max(Occurred) as Occurred from AuditData with(nolock)  where siteid = @siteid group by siteid";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [QueryReview("2013/04/4", "qwhu")]
        public IAveQueryDataReader AdminReportGetQueryCrawlStatus(string appId, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetQueryCrawlStatus"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT MAX(LogTime) AS MaxLogTime, MIN(LogTime) AS MinLogTime, SUM(NumDocuments) AS NumDocuments
        FROM Search_CrawlDocumentStats(nolock)
        WHERE 
            ApplicationId = @appId AND
            LogTime <= DATEADD(minute, -1, GETUTCDATE()) AND LogTime > DATEADD(minute, -16, GETUTCDATE())";
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@appId", appId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public IAveQueryDataReader AdminReportGetItemSizes(List<string> itemIds, int commandTimeout)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.AdminReportGetItemSizes"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    string commandText = @"with ItemSize(docid,size)as
(
select tp_DocId,SUM(CAST(ISNULL(tp_Size,0) AS BigInt)) from AllUserData (nolock)
where 1=1 {1}
group by tp_DocId
),
DocSize(docid,size)as
(
select Id,SUM(CAST(ISNULL(size,0)+ISNULL(MetaInfoSize,0) AS BigInt)) from AllDocs (nolock)
where 1=1 {0}
group by Id
),
VersionSize(docid,size)as
(
select Id,SUM(CAST(ISNULL(size,0)+ISNULL(MetaInfoSize,0) AS BigInt)) from AllDocVersions (nolock)
where 1=1 {0}
group by Id
)
select DocSize.docid AS Id,CAST(ISNULL(ItemSize.size,0)+ISNULL(VersionSize.size,0)+ISNULL(DocSize.size,0)AS BigInt) AS Size from DocSize
left join VersionSize on DocSize.docid=VersionSize.docid
left join ItemSize on ItemSize.docid=DocSize.docid
";
                    string docIdFilter = GetDocIdFilter(itemIds);
                    string itemIdFilter = GetItemIdFilter(itemIds);
                    cmd.CommandText = String.Format(commandText, docIdFilter, itemIdFilter);
                    cmd.CommandTimeout = commandTimeout;
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Vsersion"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "docid")]
        public IAveQueryDataReader PageTrafficGetItemSizes(List<string> itemIds, int commandTimeout, Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.PageTrafficGetItemSizes"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    string commandText = @"
select Id,CAST(ISNULL(size,0) as bigint) as Size from AllDocs (nolock)
where 1=1 {0} and SiteId = @SiteId and iscurrentversion=1
";
                    string docIdFilter = GetDocIdFilter(itemIds);
                    //string itemIdFilter = GetItemIdFilter(itemIds);
                    cmd.CommandText = String.Format(commandText, docIdFilter);
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        private string GetDocIdFilter(List<string> itemIds)
        {
            StringBuilder itemIdFilter = new StringBuilder("and Id in ('" + itemIds[0] + "' ");
            for (int i = 1; i < itemIds.Count; i++)
            {
                itemIdFilter.Append(",'" + itemIds[i] + "' ");
            }
            itemIdFilter.Append(")");
            return itemIdFilter.ToString();
        }

        private string GetItemIdFilter(List<string> itemIds)
        {
            StringBuilder itemIdFilter = new StringBuilder("and tp_DocId in ('" + itemIds[0] + "' ");
            for (int i = 1; i < itemIds.Count; i++)
            {
                itemIdFilter.Append(",'" + itemIds[i] + "' ");
            }
            itemIdFilter.Append(")");
            return itemIdFilter.ToString();
        }


        #region Search Usage
        [OnlyForSP2010]
        public IAveQueryDataReader GetSearchUsage(bool isGetAllSearchUsage, DateTime beginTime, DateTime endTime, List<string> aggregationIdList)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSearchUsage"))
            {

                StringBuilder aggregationFilter = new StringBuilder();
                aggregationFilter.Append("'" + aggregationIdList[0] + "'");
                for (int i = 1; i < aggregationIdList.Count; i++)
                {
                    aggregationFilter.Append(",'" + aggregationIdList[i] + "'");
                }
                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    string sqlStatement = @"select qta.TotalFrequency,qt.QueryText,case when s.ScopeName ='' or s.ScopeName is null then 'No Scope Name' else s.ScopeName end AS ScopeName ,DateId
    from WAQueryTextAggregationByDate AS qta WITH (NOLOCK)
    LEFT JOIN WAQueryText qt WITH (NOLOCK) on qta.QueryTextHash=qt.QueryTextHash
    LEFT JOIN WAScope AS s WITH (NOLOCK) on qta.ScopeNameHash=s.ScopeNameHash
    where 
        qta.AggregationId in ({0}) and {1}";
                    if (!isGetAllSearchUsage)
                    {
                        command.CommandText = String.Format(sqlStatement, aggregationFilter.ToString(), "DateId between @BeginDayId and @EndDayId");
                        command.Parameters.AddWithValue("@BeginDayId", Convert.ToInt32(AveDateTimeUtility.ConvertToType012(beginTime)));
                        command.Parameters.AddWithValue("@EndDayId", Convert.ToInt32(AveDateTimeUtility.ConvertToType012(endTime)));
                    }
                    else
                    {
                        command.CommandText = String.Format(sqlStatement, aggregationFilter.ToString(), "1=1");
                    }
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }

        public string GetSearchUsageDayColumn(DateTime time)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSearchUsageDayStr"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @" SELECT 'Day'+REPLACE(STR(dbo.fn_GetDaySlot(CONVERT(date, @Time),0),2,0), ' ', '0')+'Hits'";
                    cmd.Parameters.AddWithValue("@Time", time);
                    return cmd.ExecuteScalar() as string;
                }
            }
        }

        public IAveQueryDataReader GetSearchUsageDataByDay(byte[] partitionId, int reportType, DateTime reportDate, string column, int index, int pageSize)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSearchUsageDataByDay"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT TOP {1} ItemUri AS Query, {0} AS Count, ScopeId ,ItemHash 
                     FROM SearchReportsData (nolock) 
                        WHERE PartitionId=@PartitionId 
                        AND ReportType = @ReportType 
                        AND ItemHash > @Index 
                        AND LastProcessingTime >= CONVERT(date, @ReportDate) 
                        AND {0} > 0 ", column, pageSize);
                    cmd.Parameters.AddWithValue("@PartitionId", partitionId);
                    cmd.Parameters.AddWithValue("@ReportType", reportType);
                    cmd.Parameters.AddWithValue("@ReportDate", reportDate);
                    cmd.Parameters.AddWithValue("@Index", index);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }
        #endregion

        #region User Storage
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "rewrite")]
        public IAveQueryDataReader GetUserStorageDocInfo(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageDocInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
select tp_Author,tp_Editor,Size,tp_DocId,type from AllDocs doc(nolock)
inner join AllUserData data (nolock) on data.tp_SiteId=@SiteId and
data.tp_DeleteTransactionId=0x and data.tp_ParentId=@ParentId and doc.Id=data.tp_DocId
and tp_UIVersion=UIVersion
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId
union all
select tp_Author,tp_Editor,ver.Size,tp_DocId,0 from AllUserData data (nolock)
inner join AllDocVersions ver (nolock) on ver.SiteId=@SiteId and ver.Id=data.tp_DocId and ver.UIVersion=data.tp_UIVersion
where data.tp_SiteId=@SiteId and data.tp_DeleteTransactionId=0x and data.tp_IsCurrentVersion=0
and data.tp_ParentId=@ParentId";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public IAveQueryDataReader GetUserStorageAttachment(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageAttachment"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select doc.LeafName,doc.Size,doc.MetaInfo from AllDocs doc (nolock)
inner join AllDocs parent (nolock) on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId = parent.Id
where parent.SiteId=@SiteId and parent.DeleteTransactionId=0x and parent.ParentId=@ParentId";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader GetUserStorageItemInfo(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageItemInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select tp_Size,tp_Author from alldocs doc(nolock)
inner join AllUserData data(nolock) on tp_SiteId=@SiteId and tp_DeleteTransactionId=0x and 
tp_IsCurrentVersion>=0 and tp_ParentId=@ParentId and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId and Type=0";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader GetUserStorageItemInfo(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageItemInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select tp_Size,tp_Author from alldocs doc(nolock)
inner join AllUserData data(nolock) on tp_SiteId=@SiteId and tp_DeleteTransactionId=0x and 
tp_IsCurrentVersion>=0 and tp_ParentId=@ParentId and doc.Id=data.tp_DocId
and doc.IsCurrentVersion=1 
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId and Type=0
and tp_Created between @StartTime and @EndTime";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    SqlParameter startTime = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTime.Value = begin.DateTime.ToString();
                    command.Parameters.Add(startTime);
                    SqlParameter endTime = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTime.Value = end.DateTime.ToString();
                    command.Parameters.Add(endTime);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "rewrite")]
        public IAveQueryDataReader GetUserStorageDocInfoWithTimeScope(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageDocInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
select tp_Author,tp_Editor,Size,tp_DocId,type from AllDocs doc(nolock)
inner join AllUserData data(nolock) on tp_SiteId=@SiteId and
data.tp_DeleteTransactionId=0x and data.tp_ParentId=@ParentId and doc.Id=data.tp_DocId
and tp_UIVersion=UIVersion
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId 
and doc.TimeCreated>@StartTime and doc.TimeCreated<@EndTime
union all
select tp_Author,tp_Editor,version.Size,tp_DocId,0 from AllDocs doc(nolock)
inner join AllDocVersions version (nolock)on version.SiteId=@SiteId and version.DeleteTransactionId=0x and version.Id=doc.Id
inner join AllUserData data (nolock) on data.tp_SiteId=@SiteId and data.tp_DocId=doc.Id and tp_UIVersion=version.UIVersion
where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId=@ParentId 
and doc.TimeCreated between @StartTime and @EndTime and doc.IsCurrentVersion=1";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    SqlParameter startTime = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTime.Value = begin.DateTime.ToString();
                    command.Parameters.Add(startTime);
                    SqlParameter endTime = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTime.Value = end.DateTime.ToString();
                    command.Parameters.Add(endTime);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin", true, "add with nolock")]
        public IAveQueryDataReader GetUserStorageAttachmentWithTimeScope(Guid siteId, Guid parentId, DateTimeOffset begin, DateTimeOffset end)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserStorageAttachment"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select doc.LeafName,doc.Size,doc.MetaInfo from AllDocs doc(nolock)
inner join AllDocs parent (nolock) on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x and doc.ParentId = parent.Id
where parent.SiteId=@SiteId and parent.DeleteTransactionId=0x and parent.ParentId=@ParentId
and doc.TimeCreated between @StartTime and @EndTime";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    SqlParameter startTime = new SqlParameter("@StartTime", SqlDbType.DateTime);
                    startTime.Value = begin.DateTime.ToString();
                    command.Parameters.Add(startTime);
                    SqlParameter endTime = new SqlParameter("@EndTime", SqlDbType.DateTime);
                    endTime.Value = end.DateTime.ToString();
                    command.Parameters.Add(endTime);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }
        #endregion

        #region Site Referrers
        [OnlyForSP2010]
        public IAveQueryDataReader GetSiteReferrerData(bool isSelectedAll, DateTime beginTime, DateTime endTime, string aggregationId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSiteReferrerData"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    string sqlStatement = @"select DateId,Origin,Frequency from WAExternalRefererAggregationByDate(nolock)
where AggregationId=@AggregationId and {0} and IncludeSubSites=1";
                    if (!isSelectedAll)
                    {
                        command.CommandText = String.Format(sqlStatement, "DateId between @BeginDayId and @EndDayId");
                        command.Parameters.AddWithValue("@BeginDayId", Convert.ToInt32(AveDateTimeUtility.ConvertToType012(beginTime)));
                        command.Parameters.AddWithValue("@EndDayId", Convert.ToInt32(AveDateTimeUtility.ConvertToType012(endTime)));
                    }
                    else
                    {
                        command.CommandText = String.Format(sqlStatement, "1=1");
                    }
                    command.Parameters.AddWithValue("@AggregationId", aggregationId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(command));
                }

            }

        }
        #endregion

        #region Best Practice Reports
        [OnlyForSP2010]
        public long BPRSP2010GetUserProfileCount()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetUserProfileCount"))
            {

                long count = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.UserProfile_Full (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        count = Convert.ToInt64(objCount);
                    }
                }
                return count;

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010GetSocialContentCount()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetSocialContentCount"))
            {

                long count = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.SocialTags (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        count += Convert.ToInt64(objCount);
                    }
                }
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.SocialRatings (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        count += Convert.ToInt64(objCount);
                    }
                }
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.SocialComments (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        count += Convert.ToInt64(objCount);
                    }
                }
                return count;

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader GetTermSetByLevelLimit(int level)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetTermSetByLevelLimit"))
            {

                string query = @"SELECT dbo.ECMTermSet.Name as TermSetName, dbo.ECMGroup.Name as GroupName, dbo.ECMTermSetMembership.Path as Path
                    FROM dbo.ECMTermSetMembership,
                    dbo.ECMTermSet, dbo.ECMGroup (nolock)
                    where Path like @path AND
                    dbo.ECMTermSetMembership.TermSetId
                    = dbo.ECMTermSet.Id
                    AND dbo.ECMGroup.Id = dbo.ECMTermSet.GroupId";

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@path", GetPath(level));
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        private string GetPath(int level)
        {
            if (level == 0)
            {
                level++;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < level; i++)
            {
                builder.Append(@"%\");
            }
            builder.Append(@"%");
            return builder.ToString();
        }
        [OnlyForSP2010]
        public long GetTermSetNumber()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetTermSetNumber"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.ECMTermSet (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    return Convert.ToInt64(objCount);
                }

            }

        }
        [OnlyForSP2010]
        public long GetItemNumber()
        {
            long count = default(long);

            count += GetTermSetNumber();
            count += GetTermNumber();

            return count;
        }
        [OnlyForSP2010]
        public long GetTermNumber()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetTermNumber"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from dbo.ECMTerm (nolock)";
                    object objCount = cmd.ExecuteScalar();
                    return Convert.ToInt64(objCount);
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader GetTermSetByTermLimit(long limit)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetTermSetByTermLimit"))
            {

                string query = @"with TermCountById(id, count)
                as(
                 (SELECT ECMTermSet.Id, COUNT(termId)
                FROM ECMTermSetMembership,
                ECMTermSet, ECMGroup (nolock)
                where
                ECMTermSetMembership.TermSetId
                = ECMTermSet.Id
                AND ECMGroup.Id = ECMTermSet.GroupId

                group by ECMTermSet.Id
                )
                )
                select ECMTermSet.Name as TermSetName, ECMGroup.Name as GroupName ,TermCountById.count as TermCount from TermCountById, ECMTermSet, ECMGroup  (nolock)
                where TermCountById.count > @limit
                and TermCountById.id = ECMTermSet.Id
                and ECMGroup.Id = ECMTermSet.GroupId";

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@limit", limit);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010GetItemCount()
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetItemCount"))
            {

                long count = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllDocs(nolock)";
                    object docCount = cmd.ExecuteScalar();
                    if (docCount != DBNull.Value)
                    {
                        count += Convert.ToInt64(docCount);
                    }
                }
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllDocVersions(nolock)";
                    object docVerCount = cmd.ExecuteScalar();
                    if (docVerCount != DBNull.Value)
                    {
                        count += Convert.ToInt64(docVerCount);
                    }
                }

                return count;

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader BPRSP2010GetMajorVersionCount(Guid listId, long maxValue, int isList)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetMajorVersionCount"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    if (isList == 0)
                    {
                        cmd.CommandText = @"with MajorVersionCount (Id,Name,Num)as
(
select doc.Id,doc.LeafName, COUNT(1) as MajorVersionCount from 
AllUserData data(nolock) 
inner join AllDocs doc on doc.Id=data.tp_DocId and doc.Level>0 and doc.IsCurrentVersion=1 
where tp_listid=@ListId and tp_DeleteTransactionId=0x and (tp_UIVersion %512=0)
group by doc.Id,doc.LeafName
)
select * from MajorVersionCount where Num>@MaxValue";
                    }
                    else
                    {
                        cmd.CommandText = @"with MajorVersionCount (Id,Name,Num,ItemId)as
(
select data1.tp_DocId,data1.nvarchar1,COUNT(*),data1.tp_ID 
from AllUserData data1 with(index(AllUserData_PK),nolock)
inner join AllUserData data2 with(index(AllUserData_PK),nolock)
on data1.tp_ListId=data2.tp_ListId and data1.tp_DeleteTransactionId=data1.tp_DeleteTransactionId
and data1.tp_IsCurrentVersion=1 and (data2.tp_IsCurrentVersion=1 or data2.tp_IsCurrentVersion=0) 
and data1.tp_id=data2.tp_id
where data1.tp_ListId=@ListId and data2.tp_ListId=@ListId  and data1.tp_DeleteTransactionId=0x and 
data2.tp_DeleteTransactionId=0x and data1.tp_IsCurrent=1 and (data2.tp_UIVersion %512=0) 
Group by data1.tp_DocId,data1.nvarchar1,data1.tp_ID
)
select * from MajorVersionCount where Num>@MaxValue";
                    }
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.Parameters.AddWithValue("@MaxValue", maxValue);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader BPRSP2010GetGroupCountAUserBelongTo(long maxGroupCount, string siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetGroupCountAUserBelongTo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"with SPUserInfo (MemberId,GroupCount)as
(
select MemberId,COUNT(1) as GroupCount from GroupMembership(nolock) where SiteId=@SiteId group by MemberId
)
select MemberId,tp_Login,tp_Title,GroupCount from SPUserInfo inner join UserInfo on tp_SiteID=@SiteId and 
MemberId=UserInfo.tp_ID and tp_Deleted=0 where GroupCount>@MaxGroupCount";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@MaxGroupCount", maxGroupCount);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010DocumentCountInList(string siteId, string listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010DocumentCountInList"))
            {

                long result = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and IsCurrentVersion=1 and ListId=@ListId and Type=0 and DoclibRowId is not null AND HasStream = 1;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        result = Convert.ToInt64(objCount);
                    }
                }
                return result;

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010GetUserCountInSC(string siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetUserCountInSC"))
            {

                long userCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(tp_ID) from UserInfo (nolock) where tp_SiteID=@SiteId and tp_Deleted=0";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    object userCountObj = cmd.ExecuteScalar();
                    if (userCountObj != DBNull.Value)
                    {
                        userCount = Convert.ToInt64(userCountObj);
                    }
                }
                return userCount;

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010GetGroupCountInSC(string siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetGroupCountInSC"))
            {

                long groupCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(ID) from Groups(nolock) where SiteId=@SiteId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    object groupCountObj = cmd.ExecuteScalar();
                    if (groupCountObj != DBNull.Value)
                    {
                        groupCount = Convert.ToInt64(groupCountObj);
                    }
                }
                return groupCount;

            }

        }
        [OnlyForSP2010]
        public IAveQueryDataReader BPRSP2010GetSecurityScope(string siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetSecurityScope"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select ScopeId,ScopeUrl from perms (nolock) where SiteId=@SiteId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [OnlyForSP2010]
        public long BPRSP2010GetPrincipalCount(string siteId, string scopeId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetPrincipalCount"))
            {

                long principalCount = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(distinct PrincipalId) from dbo.RoleAssignment(nolock) where SiteId=@SiteId and ScopeId=@ScopeId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ScopeId", scopeId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        principalCount = Convert.ToInt64(objCount);
                    }
                }
                return principalCount;

            }

        }
        [OnlyForSP2010]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ScoptCount is the part of method name.  ")]
        public long BPRSP2010SecurityScoptCount(string siteId, string listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010SecurityScoptCount"))
            {

                long result = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(distinct ScopeId) from dbo.AllDocs(nolock) where SiteId=@SiteId and ListId=@ListId;";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        result = Convert.ToInt64(objCount);
                    }
                }
                return result;

            }

        }
        public IAveQueryDataReader BPRSP2010GetAllDocInfoInSC(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetAllDocInfoInSC"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select ListId,HasStream,ScopeId,IsCurrentVersion,Type,DoclibRowId,UIVersion,LeafName,Id from AllDocs(nolock) where SiteId=@SiteId
and DeleteTransactionId=0x and ListId is not null";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetAllUserData(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetAllUserData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp_ListId,tp_DocId,tp_UIVersion,tp_IsCurrent FROM AllUserData(nolock) WHERE tp_SiteId = @SiteId 
                                        AND tp_DeleteTransactionId=0x";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public int BPRSP2010GetSubWebCount(Guid siteId, Guid webId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BPRSP2010GetSubWebCount"))
            {

                int result = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select COUNT(1) from Webs(nolock) where SiteId=@SiteId and ParentWebId=@WebId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    object objCount = cmd.ExecuteScalar();
                    if (objCount != DBNull.Value)
                    {
                        result = Convert.ToInt32(objCount);
                    }
                }
                return result;

            }

        }
        #endregion

        #region RC Common Query

        /// <summary>
        /// return all user info in site collection
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [BothSP10AndSP13]
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader GetSiteUserInfo(Guid siteId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSiteUserInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select tp_ID,tp_DomainGroup,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title from UserInfo(nolock)
where tp_SiteID=@SiteId";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }

        [BothSP10AndSP13]
        public IAveQueryDataReader GetDocumentInfo(Guid siteId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocumentInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"  WITH DocInfo as ( 
                      SELECT AllDocs.Id,[SiteId],[WebId],[ListId],[DirName],[LeafName],[TimeCreated],[tp_Author]
                      FROM [AllDocs] (nolock) 
                      left join AllUserData (nolock)
                      on AllDocs.Id = AllUserData.tp_docId
                      and AllDocs.SiteId = AllUserData.tp_SiteId
                      and AllDocs.DeleteTransactionId = AllUserData.tp_DeleteTransactionId
                      and AllDocs.IsCurrentVersion = AllUserData.tp_IsCurrentVersion
                      and AllDocs.ParentId = AllUserData.tp_ParentId
                      and AllDocs.Level = AllUserData.tp_Level
                      where AllDocs.SiteId = @SiteId
                      and AllDocs.Id = @DocId 
                      and DeleteTransactionId = 0x and IsCurrentVersion = 1 and DoclibRowId is not null)

                      SELECT DocInfo.*, UserInfo.tp_Login, UserInfo.tp_Title from DocInfo with(nolock) 
                      left join UserInfo with(nolock)
                      on DocInfo.tp_Author = UserInfo.tp_ID
                      and DocInfo.SiteId = UserInfo.tp_SiteID
                      and DocInfo.SiteId = @SiteId";
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    command.Parameters.AddWithValue("@DocId", docId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }


        [BothSP10AndSP13]
        public IAveQueryDataReader GetDocumentInfo(Guid siteId, List<Guid> docIds)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocumentInfo"))
            {

                using (SqlCommand command = mQueryWorker.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format(@"  WITH DocInfo as ( 
                      SELECT AllDocs.Id,[SiteId],[WebId],[ListId],[DirName],[LeafName],[TimeCreated],[tp_Author]
                      FROM [AllDocs] (nolock) 
                      left join AllUserData (nolock)
                      on AllDocs.Id = AllUserData.tp_docId
                      and AllDocs.SiteId = AllUserData.tp_SiteId
                      and AllDocs.DeleteTransactionId = AllUserData.tp_DeleteTransactionId
                      and AllDocs.IsCurrentVersion = AllUserData.tp_IsCurrentVersion
                      and AllDocs.ParentId = AllUserData.tp_ParentId
                      and AllDocs.Level = AllUserData.tp_Level
                      where AllDocs.SiteId = @SiteId
                      and AllDocs.Id in {0}
                      and DeleteTransactionId = 0x and IsCurrentVersion = 1 and DoclibRowId is not null)

                      SELECT DocInfo.*, UserInfo.tp_Login, UserInfo.tp_Title from DocInfo with(nolock) 
                      left join UserInfo with(nolock)
                      on DocInfo.tp_Author = UserInfo.tp_ID
                      and DocInfo.SiteId = UserInfo.tp_SiteID
                      and DocInfo.SiteId = @SiteId", GetInClause(docIds));

                    command.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(command.ExecuteReader());
                }

            }

        }

        #endregion

        #region RC Alert
        [NotUsed]
        public IAveQueryDataReader GetAlertItemByAlertId(string alertId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetAlertItemByAlertId"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@AlertId", alertId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"select LeafName as Title from AllDocs(nolock)inner join ImmedSubscriptions(nolock) 
where AllDocs.ID= ImmedSubscriptions.itemdocid and AllDocs.Level>0 and ImmedSubscriptions.Id=@AlertId";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        #endregion

        #region Admin Report Storage Report and Blob
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013DocIdsByParentId(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013DocIdsByParentId"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select doc.Id from AllDocs doc(nolock) where doc.SiteId=@SiteId and doc.DeleteTransactionId=0x
and doc.ParentId=@ParentId and doc.IsCurrentVersion=1 and type=0";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013AttachmentIdsByParentId(Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013AttachmentIdsByParentId"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select doc.Id from AllDocs parent(nolock)
inner join AllDocs doc(nolock) on doc.SiteId=@SiteId and doc.DeleteTransactionId=0x 
and doc.ParentId=parent.Id and doc.type=0 and doc.HasStream=1 
where parent.SiteId= @SiteId and parent.DeleteTransactionId=0x 
and parent.ParentId=@ParentId and parent.type=1;";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013GetInfo(Guid siteId, Guid docId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013GetInfo"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@DocId", docId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
select Size,DocId,Partition,BSN,RbsId from DocStreams stream(nolock)
where stream.SiteId=@SiteId and stream.DocId=@DocId;";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader BlobSP2013GetItemSize(Guid siteId, Guid listId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.BlobSP2013GetItemSize"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListId", listId);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                    select Sum(Cast(ISNULL(tp_Size,0)as bigint)) as Size from AllUserData (nolock)
                    where tp_SiteId=@SiteId and tp_ListId=@ListId and
                    tp_DeleteTransactionId=0x and tp_level=1;";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        #endregion

        [QueryReview("2013/01/17", "Austin Han", true, "Remove tp_RowOrdinal and add tp_IsCurrentVersion to use the cluster index.")]
        public IAveQueryDataReader GetSocialBlogPost(Guid siteId, Guid parentId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSocialBlogPost"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select data.tp_ID,tp_Modified,tp_Created,nvarchar1 as PostTitle,users.tp_Login,users.tp_Title,
datetime1 as PublishedTime,tp_moderationstatus as ApprovedStatus,data.ntext2 as Body from AllUserData (nolock) as data
left join userinfo users(nolock) on users.tp_ID= data.tp_Author and users.tp_SiteID=data.tp_SiteId
WHERE data.tp_SiteId=@SiteId and tp_DeleteTransactionId=0x and tp_ParentId=@ParentId
and (data.tp_IsCurrentVersion=1 OR data.tp_IsCurrentVersion=0)";
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        [QueryReview("2013/01/17", "Austin Han", true, "Remove tp_RowOrdinal and add tp_IsCurrentVersion to use the cluster index.")]
        public IAveQueryDataReader GetSocialBlogCommentsWithPost(Guid siteId, Guid parentId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSocialBlogCommentsWithPost"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"
select data.tp_ID,data.tp_Modified,data.tp_Created,data.nvarchar1 as CommentTitle,data.ntext2 as Body,
users.tp_Login,users.tp_Title, data.int1 as PostId from AllUserData as data
left join userinfo users(nolock) on users.tp_ID= data.tp_Author and users.tp_SiteID=data.tp_SiteId
where data.tp_SiteId=@SiteId and data.tp_DeleteTransactionId=0x and data.tp_ParentId=@ParentId 
and (data.tp_IsCurrentVersion=1 OR data.tp_IsCurrentVersion=0)";
                    cmd.Parameters.AddWithValue("@ParentId", parentId);
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        public Dictionary<string, string> GetLookupItemIdAndDisplayValue(AveLookupFieldInfo fieldInfo)
        {
            Dictionary<string, string> itemIdAndValues = new Dictionary<string, string>();
            string queryCmd = @"SELECT tp_ID, " + fieldInfo.LookupColumnRowNameForQuery + " FROM AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", fieldInfo.LookupList);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(queryCmd))
                {
                    while (dr.Read())
                    {
                        if (dr.IsDBNull(1))
                        {
                            continue;
                        }
                        string itemId = dr[0].ToString();
                        string columnValue = dr[1].ToString();
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            itemIdAndValues[itemId] = columnValue;
                        }
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
            return itemIdAndValues;
        }

        #region SP Usage Data
        [BothSP10AndSP13]
        public IAveQueryDataReader GetUsageData(string viewName, DateTime startTimeToDisplay, DateTime endTimeToDisplay)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUsageData"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select * from " + viewName + " with(nolock) where LogTime >= @LogTimeStart and LogTime <= @LogTimeEnd";
                    cmd.Parameters.AddWithValue("@LogTimeStart", startTimeToDisplay);
                    cmd.Parameters.AddWithValue("@LogTimeEnd", endTimeToDisplay);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        [BothSP10AndSP13]
        public IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetRequestUsageDataWithDefaultColumns"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select FarmId, MachineName, WebApplicationId, SiteId, WebId, LogTime, QueryString, ServerUrl, SiteUrl, WebUrl,
                                    DocumentPath, UserLogin, Title from RequestUsage with(nolock) 
                                    where LogTime >= @LogTimeStart and LogTime <= @LogTimeEnd and WebId <> '00000000-0000-0000-0000-000000000000'";
                    cmd.Parameters.AddWithValue("@LogTimeStart", startTimeToDisplay);
                    cmd.Parameters.AddWithValue("@LogTimeEnd", endTimeToDisplay);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        [BothSP10AndSP13]
        public IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid webId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetRequestUsageDataWithDefaultColumns"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select FarmId, MachineName, WebApplicationId, SiteId, WebId, LogTime, QueryString, ServerUrl, SiteUrl, WebUrl,
                                    DocumentPath, UserLogin, Title from RequestUsage with(nolock) 
                                    where LogTime >= @LogTimeStart and LogTime <= @LogTimeEnd and WebId = @WebId";
                    cmd.Parameters.AddWithValue("@LogTimeStart", startTimeToDisplay);
                    cmd.Parameters.AddWithValue("@LogTimeEnd", endTimeToDisplay);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        [BothSP10AndSP13]
        public IAveQueryDataReader GetRequestUsageDataWithDefaultColumns(DateTime startTimeToDisplay, DateTime endTimeToDisplay, Guid webId, long index, int pageSize)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetRequestUsageDataWithDefaultColumns"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    int partitionId = GetUsageDataPartitionId(startTimeToDisplay);
                    if (startTimeToDisplay.Date == startTimeToDisplay)
                    {
                        startTimeToDisplay = startTimeToDisplay.AddMilliseconds(-3);
                    }
                    string tableName = string.Format("RequestUsage_Partition{0}", partitionId);
                    cmd.CommandText = string.Format(@"SELECT top (@PageSize)
                        FarmId, MachineName, WebApplicationId, SiteId, WebId, LogTime, QueryString, ServerUrl, SiteUrl, WebUrl,
                        DocumentPath, UserLogin, Title
                        from 
                        (SELECT ROW_NUMBER() OVER(ORDER BY LogTime ASC) as RowNumber, 
                        FarmId, MachineName, WebApplicationId, SiteId, WebId, LogTime, QueryString, ServerUrl, SiteUrl, WebUrl,
                        DocumentPath, UserLogin, Title from {0} with(nolock)
						where LogTime > @LogTimeStart and LogTime <= @LogTimeEnd and WebId = @WebId
                        and PartitionId = @PartitionId
						) AS UsageData
                        where UsageData.RowNumber > @PageIndex Order By LogTime ASC", tableName);

                    cmd.Parameters.AddWithValue("@PartitionId", partitionId);
                    cmd.Parameters.AddWithValue("@LogTimeStart", startTimeToDisplay);
                    cmd.Parameters.AddWithValue("@LogTimeEnd", endTimeToDisplay);
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    cmd.Parameters.AddWithValue("@PageIndex", index);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Partition Id: {0}.", partitionId);
                    }
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        private int GetUsageDataPartitionId(DateTime time)
        {
            TimeSpan timeSpan = new TimeSpan(time.Ticks - UsageDataStartTime.Ticks);
            return (timeSpan.Days % 32);
        }

        #endregion

        #region Web Part
        public IAveQueryDataReader GetWebPartsByPage(Guid siteId, Guid pageId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetWebPartsByPage"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp_WebPartIdProperty FROM AllWebParts WITH (INDEX=PageUrlID_FK, FORCESEEK) 
                                    WHERE tp_IsCurrentVersion = 1
                                          and tp_Deleted = 0
                                          and tp_Level = 1
                                          and tp_SiteId = @SiteId
                                          and tp_PageUrlID = @PageId";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@PageId", pageId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }
        #endregion

        #region Storage Metrics
        public long GetSizeFromStorageMetrics(Guid siteId, Guid folderId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSizeFromStorageMetrics"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = "SELECT TotalSize from StorageMetrics (nolock) WHERE SiteId = @SiteId AND DocId = @DocId";
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@DocId", folderId);
                    object size = mQueryWorker.ExecuteScalar(cmd);
                    return Convert.ToInt64(size);
                }

            }

        }

        public long GetSizeFromStorageMetrics(Guid siteId, List<Guid> folderIds)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetSizeFromStorageMetrics"))
            {

                if (folderIds == null || folderIds.Count == 0)
                {
                    return 0;
                }
                long totalSize = 0;
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    List<Guid> tempFolderIds = new List<Guid>();
                    foreach (Guid folderId in folderIds)
                    {

                        tempFolderIds.Add(folderId);
                        if (MaxInClauseLength == tempFolderIds.Count)
                        {
                            totalSize += GetSizeFromStorageMetrics(siteId, tempFolderIds, cmd);
                            tempFolderIds.Clear();
                        }
                    }
                    totalSize += GetSizeFromStorageMetrics(siteId, tempFolderIds, cmd);
                }
                return totalSize;

            }

        }

        private long GetSizeFromStorageMetrics(Guid siteId, List<Guid> folderIds, SqlCommand cmd)
        {
            if (folderIds.Count != 0)
            {
                cmd.CommandText = string.Format("SELECT SUM(TotalSize) from StorageMetrics (nolock) WHERE SiteId = @SiteId AND DocId in {0}", GetInClause(folderIds));
                object size = mQueryWorker.ExecuteScalar(cmd);
                if (DBNull.Value != size)
                {
                    return Convert.ToInt64(size);
                }
            }
            return 0;
        }
        #endregion Storage Metrics


        public IAveQueryDataReader GetItemLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetItemLevelAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = "SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event,  " +
                    " AuditData.ItemId,  AuditData.DocLocation, AllDocs.WebId, AllDocs.ListId, AllDocs.Size, " +
                    " AllDocs.HasStream, AllDocs.DoclibRowId, AllDocs.Type FROM AuditData (nolock) " +
                    "INNER JOIN AllDocs (nolock) on AllDocs.SiteId = AuditData.SiteId and AllDocs.Id = AuditData.ItemId " +
                    "WHERE AuditData.SiteId = @SiteId and AllDocs.SiteId = @SiteId " +
                    "and AuditData.Occurred >= @StartTime and AuditData.Occurred < @EndTime and AllDocs.IsCurrentVersion = 1 " + 
                    " order by AuditData.Occurred";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetItemLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime, List<int> eventTypes, List<int> itemTypes)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetItemLevelAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event,  " +
                    " AuditData.ItemId,  AuditData.DocLocation, AllDocs.WebId, AllDocs.ListId, AllDocs.Size, " +
                    " AllDocs.HasStream, AllDocs.DoclibRowId, AllDocs.Type FROM AuditData (nolock) " +
                    "INNER JOIN AllDocs (nolock) on AllDocs.SiteId = AuditData.SiteId and AllDocs.Id = AuditData.ItemId " +
                    "WHERE AuditData.SiteId = @SiteId and AllDocs.SiteId = @SiteId and AuditData.ItemType in {0} and AuditData.Event in {1} " +
                    "and AuditData.Occurred >= @StartTime and AuditData.Occurred < @EndTime and AllDocs.IsCurrentVersion = 1 " + 
                    " order by AuditData.Occurred",
                    GetInClause(itemTypes), GetInClause(eventTypes));

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetListLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetListLevelAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event,  
                    AuditData.ItemId,  AuditData.DocLocation, AllLists.tp_WebId, AllLists.tp_ID FROM AuditData (nolock) 
                    INNER JOIN AllLists (nolock) on AllLists.tp_SiteId = AuditData.SiteId and AllLists.tp_ID = AuditData.ItemId 
                    WHERE AuditData.SiteId = @SiteId and AllLists.tp_SiteId = @SiteId 
                    and AuditData.Occurred >= @StartTime and AuditData.Occurred < @EndTime order by AuditData.Occurred ";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetWebLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetWebLevelAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {

                    cmd.CommandText = "SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event, " +
                        "AuditData.ItemId,  AuditData.DocLocation, AllWebs.Id FROM AuditData (nolock) " +
                        "INNER JOIN AllWebs (nolock) on AllWebs.SiteId = AuditData.SiteId and AllWebs.Id = AuditData.ItemId " +
                        "WHERE AuditData.SiteId = @SiteId and AllWebs.SiteId = @SiteId " +
                        "and AuditData.Occurred >=  @StartTime and AuditData.Occurred < @EndTime order by AuditData.Occurred ";
                   

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetSiteLevelAuditData(Guid siteId, DateTime startTime, DateTime endTime)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetWebLevelAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = "SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event,  " +
                    " AuditData.ItemId, AuditData.DocLocation FROM AuditData (nolock) " +
                    " WHERE AuditData.SiteId = @SiteId and AuditData.ItemId = @SiteId" +
                    " and AuditData.Occurred >= @StartTime and AuditData.Occurred < @EndTime order by AuditData.Occurred ";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetAuditData(Guid siteId, DateTime startTime, DateTime endTime, List<int> eventTypes, List<int> itemTypes)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetAuditData"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT AuditData.UserId, AuditData.Occurred, AuditData.ItemType, AuditData.Event, " +
                    " AuditData.ItemId, AuditData.DocLocation FROM AuditData (nolock) " +
                    " WHERE AuditData.SiteId = @SiteId and AuditData.ItemType in {0} and AuditData.Event in {1}" +
                    " and AuditData.Occurred >= @StartTime and AuditData.Occurred < @EndTime " +
                    " order by AuditData.Occurred ", GetInClause(itemTypes), GetInClause(eventTypes));

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetItemAddedEvents(Guid siteId, long startEventId, long endEventId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocumentUploads"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = "SELECT EventCache.DocId, " +
                    "EventCache.SiteId,EventCache.WebId,EventCache.ListId,EventCache.ItemFullUrl,EventCache.EventTime,AllUserData.tp_Author " +
                    "FROM EventCache (nolock) INNER JOIN AllUserData (nolock) on EventCache.SiteId = AllUserData.tp_SiteId and EventCache.ItemId = AllUserData.tp_Id " +
                    "and EventCache.ListId = AllUserData.tp_ListId WHERE EventType = 4097 and EventCache.SiteId = @SiteId " +
                    " and AllUserData.tp_SiteId = @SiteId and EventCache.Id > @StartEventId and EventCache.Id <= @EndEventId " +
                    " and AllUserData.tp_IsCurrentVersion = 1";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@StartEventId", startEventId);
                    cmd.Parameters.AddWithValue("@EndEventId", endEventId);

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public long GetNextEventId(DateTime endTime)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DECLARE @return_value int, @ChangeNumber bigint " +
                                  "EXEC	@return_value = proc_GetChangeNumberNext " +
                                  "@ChangeTime = @ChangeTimeParam, " +
                                  "@ChangeNumber = @ChangeNumber OUTPUT " +
                                  "SELECT @ChangeNumber as N'@ChangeNumber'";

                cmd.Parameters.AddWithValue("@ChangeTimeParam", endTime);

                object eventId = cmd.ExecuteScalar();
                if (eventId != null && eventId != DBNull.Value)
                {
                    return Convert.ToInt64(eventId);
                }
            }
            return 0;
        }

        public long GetPreviousEventId(DateTime startTime)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DECLARE @return_value int, @ChangeNumber bigint " +
                                  "EXEC	@return_value = proc_GetChangeNumberPrevious " +
                                  "@ChangeTime = @ChangeTimeParam, " +
                                  "@ChangeNumber = @ChangeNumber OUTPUT " +
                                  "SELECT @ChangeNumber as N'@ChangeNumber'";

                cmd.Parameters.AddWithValue("@ChangeTimeParam", startTime);

                object eventId = cmd.ExecuteScalar();
                if (eventId != null && eventId != DBNull.Value)
                {
                    return Convert.ToInt64(eventId);
                }
            }
            return 0;
        }

        public IAveQueryDataReader GetItemInfos(Guid siteId, List<Guid> itemIds)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetItemInfos"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT AllDocs.Id, AllDocs.Size, AllLists.tp_BaseType, AllDocs.Type FROM AllDocs (nolock) 
                        INNER JOIN AllLists (nolock) on AllLists.tp_ID = AllDocs.ListId WHERE AllDocs.SiteId = @SiteId  
                        and AllLists.tp_SiteId = @SiteId and AllDocs.Id in {0} 
                        and IsCurrentVersion = 1 ", GetInClause(itemIds));

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }

        public IAveQueryDataReader GetCheckedOutFiles(Guid siteId, string libraryUrl)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetCheckedOutFiles"))
            {

                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"SET NOCOUNT ON 
                        DECLARE @UrlLike nvarchar(260) 
                        EXEC proc_EscapeForLike @ListUrl, @UrlLike OUTPUT, 1 
                        SELECT  
                            CheckedOutDoc.DirName, 
                            CheckedOutDoc.LeafName, 
                        	  CheckedOutDoc.Id, 
                            ISNULL(UA.tp_Title,''), 
                            ISNULL(UA.tp_Login,''), 
                            CheckedOutDoc.TimeLastModified 
                        FROM  
                            TVF_Docs_DirNameEqLike_Value(@SiteId, @ListUrl, @UrlLike) AS CheckedOutDoc 
                        OUTER APPLY 
                            TVF_Docs_NoLock_Id_Level(CheckedOutDoc.SiteId, CheckedOutDoc.Id, 2) AS DraftDoc 
                        OUTER APPLY 
                            TVF_AllDocs_NoLock_Id_Level(CheckedOutDoc.SiteId, CheckedOutDoc.Id, 1) AS PublishedDoc 
                        OUTER APPLY 
                            TVF_UserInfo_PK(CheckedOutDoc.SiteId, CheckedOutDoc.CheckoutUserId) AS UA 
                        WHERE 
                            CheckedOutDoc.Level = 255 AND 
                            CheckedOutDoc.DocFlags & 32 <> 0 AND 
                            CheckedOutDoc.Type = 0 AND 
                            DraftDoc.Id IS NULL AND 
                            PublishedDoc.Id IS NULL AND 
                            CheckedOutDoc.DoclibRowId IS NOT NULL AND 
                            CheckedOutDoc.ListId IS NOT NULL 
                        OPTION (FORCE ORDER) ";

                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@ListUrl", libraryUrl);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }

            }

        }
        public IAveQueryDataReader GetUserAndPersonalSite()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetUserAndPersonalSite"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select a.PropertyVal, b.CanonicalMySitePortalUrl, c.NTName, c.PreferredName 
from UserProfileValue a inner join Tenants (nolock) b 
on a.PartitionID = b.PartitionID inner join UserProfile_Full (nolock) c on a.PartitionID = c.PartitionID and a.RecordID = c.RecordID
  where  a.PropertyID = 22 ";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        public IAveQueryDataReader GetMysiteFollowedItems()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetMysiteFollowedItems"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select  a.SiteId, a.Id, a.FullUrl, b.tp_ID, nvarchar10, nvarchar4, nvarchar1 ,nvarchar6, nvarchar2  from AllWebs  (nolock)  a 
  inner join AllLists  (nolock)  b on a.SiteId = b.tp_SiteId
  inner join AllUserData  (nolock)  c on b.tp_ID = c.tp_ListId
  where webtemplate = 21  and b.tp_Title = 'Social' and c.nvarchar4 is not null";

                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        public IAveQueryDataReader GetWebInfos()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetWebInfos"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select SiteId, Id, FullUrl from AllWebs  (nolock) ";
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }

        public IAveQueryDataReader GetDocInfoWithWebId(string webId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.QueryService.GetDocInfoWithWebId"))
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = @"select Id, DirName, LeafName from AllDocs  (nolock)  where WebId = @WebId and Type = 0 and DoclibRowId is not null and IsCurrentVersion = 1";
                    cmd.Parameters.AddWithValue("@WebId", webId);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
                }
            }
        }
    }
}
