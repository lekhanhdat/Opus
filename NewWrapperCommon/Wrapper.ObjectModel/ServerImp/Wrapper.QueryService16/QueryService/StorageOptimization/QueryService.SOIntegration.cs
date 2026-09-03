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
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using AvePoint.Wrapper.Resource.QueryService;
    using static SP2016SelectQueryString;
    using static SP2016UpdateQueryString;

    internal partial class AveQueryService : IAveSOIntegrationQueryService
    {
        private enum SOBlobProviderType : byte
        {//参与了位运算, 请不要修改枚举对应的int值.
            Unknown = 0,
            EBS = 1,
            RBS = 2
        }

        private enum DataType
        {
            None,
            Content,
            Stub,
        }

        #region private Methods

        /// <summary>
        /// 获取一个item的stub类型的attachments.该方法通过Docflags值或RbsId 来判断，不能通过API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemRowId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/16", "Kexin Guo", true, "change the order of the query conditions")]
        private List<string> GetStubAttachmentsByDocLibRowId(Guid siteId, Guid webId, Guid listId, int itemRowId)
        {
            var stubAttachmentList = new List<string>();
            var attachmentDir = GetAttachmentsFolderUrlByListId(siteId, webId, listId);
            if (attachmentDir == null)
            {
                return stubAttachmentList;
            }
            var attachmentParentId = GetItemAttachmentParentFolderDocId(siteId, itemRowId, attachmentDir);
            if (!attachmentParentId.HasValue)
            {
                return stubAttachmentList;
            }
            stubAttachmentList = GetStubItemUrlByParentId(siteId, attachmentParentId.Value);
            return stubAttachmentList;
        }

        private List<string> GetStubItemUrlByParentId(Guid siteId, Guid attachmentParentId)
        {

            var stubAttachmentUrlList = new List<string>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", attachmentParentId);
                var stubAttachmentRelativeUrl = GetStubItemUrlByParentId_Select_AllDocs_DocStreams;
                using (var sr = mQueryWorker.ExecuteReader(stubAttachmentRelativeUrl))
                {
                    while (sr.Read())
                    {
                        stubAttachmentUrlList.Add(sr.GetString(0).Trim('/'));
                    }
                }
            });
            return stubAttachmentUrlList;
        }

        private string GetAttachmentsFolderUrlByListId(Guid siteId, Guid webId, Guid listId)
        {
            object attachmentDirObj = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                var getAttachmentFolderCommandText = GetAttachmentsFolderUrlByListId_Select_AllDocs_AllLists;
                attachmentDirObj = mQueryWorker.ExecuteScalar(getAttachmentFolderCommandText);
            });
            return (string) attachmentDirObj;
        }

        private Guid? GetItemAttachmentParentFolderDocId(Guid siteId, int itemRowId, string attachmentsFolderUrl)
        {
            object attachmentParentId = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocLibRowId", itemRowId.ToString());
                mQueryWorker.AddParameter("@AttachmentDir", attachmentsFolderUrl.Trim('/'));
                var getDocIdCommandText = GetItemAttachmentFolderUrlByDirNameLeafName_Select_AllDocs;
                attachmentParentId = mQueryWorker.ExecuteScalar(getDocIdCommandText);
            });
            return (Guid?) attachmentParentId;
        }


        private List<StubDocumentInfo> GetStubAttachmentInfoByParentId(int startNum, int endNum, Guid siteId, Guid? attachmentParentId, string itemTitle)
        {
            List<StubDocumentInfo> result = new List<StubDocumentInfo>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", attachmentParentId);
                mQueryWorker.AddParameter("@StartNum", startNum);
                mQueryWorker.AddParameter("@endNum", endNum);
                var stubAttachmentRelativeUrl = GetStubAttachmentRelativeUrl_Select_AllDocs_DocStreams;
                using (var sr = mQueryWorker.ExecuteReader(stubAttachmentRelativeUrl))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var stubAttachment = new StubDocumentInfo
                            {
                                IsAttachment = true,
                                DocId = sr.GetGuid(0),
                                DirName = sr.GetString(1),
                                LeafName = sr.GetString(2),
                                Size = sr.IsDBNull(3) ? 0 : sr.GetInt32(3),
                                UIVersion = sr.GetInt32(4),
                                RbsId = sr.IsDBNull(5) ? null : sr.GetValue(5) as byte[],
                                Content = sr.IsDBNull(6) ? null : sr.GetValue(6),
                                ItemLeafName = itemTitle
                            };
                            result.Add(stubAttachment);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetColumnValueError, ex);
                        }
                    }
                }
            });
            return result;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private int GetStubFileCount(Guid siteId, Guid parentId, bool includeVersion)
        {
            var totalNum = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                var totalCountCommandText = includeVersion
                    ? SelectStubItemAndVersionCount_Select_AllDocs_DocsToStreams_DocStreams_AllDocVersions 
                    : GetStubDocumentCountByParentId_Select_AllDocs_DocStreams;
                totalNum = (int) mQueryWorker.ExecuteScalar(totalCountCommandText);
            });
            return totalNum;
        }

        private List<int> GetItemRowIdsByParentId(Guid siteId, Guid parentId)
        {
            var docLibRowIdList = new List<int>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);

                string docLibRowIdStr = GetItemRowIdsByParentId_Select_AllDocs;

                using (SqlDataReader sr = mQueryWorker.ExecuteReader(docLibRowIdStr))
                {
                    while (sr.Read())
                    {
                        docLibRowIdList.Add(sr.GetInt32(0));
                    }
                }
            });
            return docLibRowIdList;
        }

        private List<StubDocumentInfo> GetStubAttachmentsInFolderByDB(int startNum, int endNum, Guid siteId, Guid webId, Guid listId, Guid parentId, string attachmentDir)
        {
            var result = new List<StubDocumentInfo>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@AttachmentDir", attachmentDir.Trim('/') + "/");
                mQueryWorker.AddParameter("@StartNum", startNum);
                mQueryWorker.AddParameter("@endNum", endNum);

                var itemStubAttachmentsInFolder = GetStubAttachmentsInFolder_Select_AllDocs_DocsToStreams_DocStreams;
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
            });
            return result;
        }

        private int GetStubAttachmentsTotalNumberInFolderByDB(Guid siteId, Guid webId, Guid listId, Guid parentId, string attachmentDir)
        {
            var totalNum = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@AttachmentDir", attachmentDir.Trim('/') + "/");

                var totalCount = GetStubAttachmentsTotalCount_Select_AllDocs_DocsToStreams_DocStreams;
                totalNum = (int)mQueryWorker.ExecuteScalar(totalCount);
            });
            return totalNum;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dts short for DocsToStreams table, ds short for DocStreams.")]
        private List<StubDocumentInfo> GetStubFilesInfo(int startNum, int endNum, Guid siteId, Guid parentId)
        {
            var result = new List<StubDocumentInfo>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@StartNum", startNum);
                mQueryWorker.AddParameter("@endNum", endNum);
                var stubFilesAndVersions = GetStubFileAndVersions_Select_AllDocs_DocsToStreams_DocStreams_AllDocVersions;
                using (var sr = mQueryWorker.ExecuteReader(stubFilesAndVersions))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var stubFile = new StubDocumentInfo
                            {
                                IsAttachment = false,
                                DocId = sr.GetGuid(0),
                                DirName = sr.GetString(1),
                                ItemLeafName = sr.GetString(2),
                                LeafName = sr.GetString(2),
                                UIVersion = sr.GetInt32(3),
                                IsCurrentVersion = sr.GetInt32(4) == 1,
                                Size = sr.IsDBNull(5) ? 0 : sr.GetInt32(5)
                            };
                            result.Add(stubFile);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetColumnValueError, ex);
                        }
                    }
                }
            });
            return result;
        }


        #endregion

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
            const int count = 100*1024;
            var temp = new byte[count];
            long mPosition = 0;
            ExceptionHandlingScope(() =>
            {
                var commandGetContent = GetContentByDocId_Select_DocStreams;
                using (var cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = commandGetContent;
                    cmd.Parameters.AddWithValue("@SiteId", siteId);
                    cmd.Parameters.AddWithValue("@Id", itemId);
                    cmd.Parameters.AddWithValue("@InternalVersion", internalVersion);
                    using (var dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            while (mPosition < size)
                            {
                                var rs = (int) dr.GetBytes(0, mPosition, temp, 0, count);
                                mPosition += rs;

                                dataStream.Write(temp, 0, rs);
                            }
                        }
                    }
                }
            });
        }

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
            var fileUrls = new List<string>();
            try
            {
                fileUrls = GetStubAttachmentsByDocLibRowId(siteId, webId, listId, itemId);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetStubAttachmentsError, e);
            }
            return fileUrls;
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
            var siteId = listItem.Web.Site.ID;
            var webId = listItem.Web.ID;
            var listId = listItem.ParentList.ID;
            var itemTitle = listItem.Title;
            string attachmentsFolderUrl = GetAttachmentsFolderUrlByListId(siteId, webId, listId);
            if (attachmentsFolderUrl == null)
            {
                return new List<StubDocumentInfo>();
            }
            var attachmentParentId = GetItemAttachmentParentFolderDocId(siteId, listItem.ID, attachmentsFolderUrl);
            if (!attachmentParentId.HasValue)
            {
                return new List<StubDocumentInfo>();
            }
            totalNum = GetStubFileCount(siteId, attachmentParentId.Value,false);
            return GetStubAttachmentInfoByParentId(startNum, endNum,siteId, attachmentParentId, itemTitle);
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
            var docLibRowIdList = GetItemRowIdsByParentId(siteId, parentId);
            foreach (var docLibRowId in docLibRowIdList)
            {
                stubAttachemntList = GetStubAttachmentsByDocLibRowId(siteId, webId, listId, docLibRowId);
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
        public List<StubDocumentInfo> GetItemStubAttachmentsInFolderByDB(Guid siteId,Guid webId,Guid listId,Guid parentId, int startNum, int endNum, ref int totalNum)
        {
            string attachmentDir = GetAttachmentsFolderUrlByListId(siteId, webId, listId);
            if (attachmentDir == null)
            {
                return new List<StubDocumentInfo>();
            }
            totalNum = GetStubAttachmentsTotalNumberInFolderByDB(siteId, webId, listId, parentId, attachmentDir);
            return GetStubAttachmentsInFolderByDB(startNum, endNum, siteId, webId, listId, parentId, attachmentDir);
        }

        public long GetMaxRbs(Guid siteId, Guid docId)
        {
            long maxRbs = -1;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DocId", docId);
                var text =GetMaxRbsBsnByDocId_Select_DocsToStreams;
                using (var reader = mQueryWorker.ExecuteReader(text))
                {
                    if (reader.Read())
                    {
                        maxRbs = (long) reader[0];
                    }
                }
            });
            return maxRbs;
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
            return GetStubItemUrlByParentId(siteId, parentId);
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
            var siteId = folder.ParentWeb.Site.ID;
            var parentId = folder.UniqueId;
            totalNum=GetStubFileCount(siteId, parentId,true);
            var result = GetStubFilesInfo(startNum, endNum, siteId, parentId);
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
            if (null == mQueryWorker)
            {
                return Guid.Empty;
            }
            var stubId = Guid.Empty;
            var d6Stub = false;
            ExceptionHandlingScope(() =>
            {
                var getStubIdByRbsId = GetStubBlobIdByRBSId_Select_rbs_internal_blobs;
                mQueryWorker.AddParameter("@RBSId", rbsId);
                d6Stub = false;
                using (var sr = mQueryWorker.ExecuteReader(getStubIdByRbsId))
                {
                    if (sr.Read())
                    {
                        var blobId = new byte[20];
                        var len = (int) sr.GetBytes(0, 0, blobId, 0, 20);

                        if (blobId[0] == 'D' && blobId[1] == 'O' && blobId[2] == 'C')
                        {
                            d6Stub = true;
                            var result = new byte[16];
                            Array.Copy(blobId, 4, result, 0, 16);
                            stubId = new Guid(result);
                        }
                        //else for non-D6 stub, return Guid.Empty
                    }
                }
            });
            isD6Stub = d6Stub;
            return stubId;
        }



        public void UpdateContentNative13(List<AveShredStubInfo> shredInfoList, Guid siteId, Guid DocId, Stream stream)
        {
            ExceptionHandlingScope(() =>
            {
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

                        mQueryWorker.ExecuteNonQuery(SetContentNullNative_Update_DocStreams);

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

                            mQueryWorker.ExecuteNonQuery(UpdateContentNative_Update_DocStreams);

                            sizeToRead -= hasRead;
                            if (sizeToRead == 0) { break; }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 更新file的数据库中的Size字段
        /// </summary>
        /// <param name="docInfo"></param>
        [QueryReview("2012/05/16", "Kexin Guo", true, "add DeleteTransactionId and ParentId for AllDocs")]
        public void UpdateDocumentSize(AveSPItemNativeInfo docInfo)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", docInfo.SiteId);
                mQueryWorker.AddParameter("@ParentId", docInfo.Folder.UniqueId);
                mQueryWorker.AddParameter("@Id", docInfo.ItemId);
                mQueryWorker.AddParameter("@Size", docInfo.Size);
                mQueryWorker.AddParameter("@InternalVersion", docInfo.InternalVersion);
                var updateText = UpdateDocumentSize_Update_AllDocs;
                mQueryWorker.ExecuteNonQuery(updateText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            });
        }

        public void UpdateEBSStubByNative(Guid siteId, Guid parentId, Guid docId, int uiVersion, AveStorageInfo storageInfo, byte[] content)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 更新file的数据库中的Content，Size，RbsId等字段，对于Rbs的stub数据，需要将Content设置为null并更新RbsId，无法通过API来实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="uiVersion"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="storageInfo"></param>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        public void UpdateRbsID(Guid siteId, Guid parentId, Guid uniqueId, int uiVersion, byte[] data, int type, AveStorageInfo storageInfo)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                string cmdText;
                mQueryWorker.AddParameter("@Id", uniqueId);
                mQueryWorker.AddParameter("@UIVersion", uiVersion);
                if ((DataType) type == DataType.Stub)
                {
                    mQueryWorker.AddParameter("@RbsId", data);
                    mQueryWorker.AddParameter("@Size", storageInfo.Size);
                    cmdText = UpdateContentFiletoStub_Update_DocStreams;
                }
                else
                {
                    mQueryWorker.AddParameter("@Content", data);
                    mQueryWorker.AddParameter("@Size", data.Length);
                    cmdText = UpdateStubFileToContent_Update_DocStreams;
                }
                mQueryWorker.ExecuteNonQuery(cmdText);
            });
        }

        public void UpdateStubDocumentSize(int level, Guid parentId, Guid docId, Guid siteId, int size, long nextBSN)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@Id", docId);
                mQueryWorker.AddParameter("@Size", size);
                mQueryWorker.AddParameter("@Level", level);
                mQueryWorker.AddParameter("@NextBSN", nextBSN);
                var updateText = UpdateStubDocumentSize_Update_AllDocs;
                mQueryWorker.ExecuteNonQuery(updateText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            });
        }

        /// <summary>
        /// 更新stub类型的file数据库中Content，Size等字段
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <param name="version"></param>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        public void UpdateStubFileStream(Guid siteId, Guid parentId, Guid uniqueId, byte[] bytes, long length, int version)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@Id", uniqueId);
                mQueryWorker.AddParameter("@UIVersion", version);
                mQueryWorker.AddParameter("@Content", bytes);
                mQueryWorker.AddParameter("@Size", length);
                var cmdText = UpdateStubDocumentContent_Update_DocStreams;
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                cmdText = UpdateStubDocumentAndVersionSize_Update_AllDocs_AllDocVersions;
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            });
        }
    }
}
