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
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        /// <summary>
        /// 获取List下，特定Field是否是Lookup field
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public bool GetFieldCollectionRelationship(string siteId, string listId, string fieldId)
        {
            string text = @"SELECT * FROM AllLookupRelationships WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND FieldId = @FieldId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@FieldId", fieldId);

                using (SqlDataReader reader = mQueryWorker.ExecuteReader(text))
                {
                    return reader.HasRows;
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
        /// 根据item的GUID查询item的DocLibRowId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="lookupListId"></param>
        /// <param name="GUID"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han", false, "We should use site id for AllUserData table")]
        [Obsolete("Please use another method which contains site id in the parameters.")]
        public int GetLookupIdByGUID(Guid lookupListId, Guid GUID)
        {
            string cmdText = @"SELECT tp_ID from AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0  
                                                AND tp_GUID=@GUID AND (tp_Level=1 OR tp_Level=2 OR tp_Level=255) AND tp_RowOrdinal=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", lookupListId);
            mQueryWorker.AddParameter("@GUID", GUID);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int tp_ID = dr.GetInt32(0);
                        return tp_ID;
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
            return -1;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public int GetLookupIdByGUID(Guid siteId, Guid lookupListId, Guid GUID)
        {
            string cmdText = @"SELECT tp_ID from AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0  
                                                AND tp_GUID=@GUID AND (tp_Level=1 OR tp_Level=2 OR tp_Level=255) AND tp_RowOrdinal=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", lookupListId);
            mQueryWorker.AddParameter("@GUID", GUID);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int tp_ID = dr.GetInt32(0);
                        return tp_ID;
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
            return -1;
        }

        /// <summary>
        /// 获取Folder下Document的Content和Url
        /// 效率考虑，有API实现
        /// 当前ContentType已改为API方式备份，此方法不会再被调用，若此方法以后会调用，则需要考虑用ParentId 来代替DirName 进行查询
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="folderUrl"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<AveContentTypeFileInfo> GetContentTypeCollectionResources(Guid siteId, string folderUrl)
        {
            List<AveContentTypeFileInfo> ResourceFolderFiles = new List<AveContentTypeFileInfo>();
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@DirName", folderUrl.TrimStart('/'));
                string cmdText = @"select Content, LeafName from AllDocs WITH(NOLOCK)
inner join DocStreams WITH(NOLOCK) on AllDocs.SiteId = DocStreams.SiteId AND AllDocs.Id = DocStreams.DocId 
where AllDocs.SiteId=@SiteId AND DeleteTransactionId=0x AND DirName=@DirName";
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] content = dr["Content"] as byte[];
                        string url = folderUrl + "/" + dr["LeafName"] as string;
                        ResourceFolderFiles.Add(new AveContentTypeFileInfo(url, content));
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

            return ResourceFolderFiles;
        }

        /// <summary>
        /// 获取ContentType的真实Name
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            return mQuerySessionSchema.GetContentTypeName(siteId, contentTypeId);
        }

        [QueryReview("2012/12/18", "Austin Han", false, "We should use site id for AllLists tables.")]
        public List<string> GetAllListContentTypes(Guid siteId,Guid webId)
        {
            var contentTypeInfos = new List<string>();
            try
            {
                string cmdText = @"SELECT tp_ContentTypes FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId";
                mQueryWorker.AddParameter("@WebId", webId);
                if (siteId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    cmdText = String.Format("{0} AND tp_SiteId=@siteId",cmdText);
                }
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            string cts = AveCompressedUtility.GetTCompressedString(reader.GetValue(0) as byte[]);
                            if (!contentTypeInfos.Contains(cts))
                            {
                                contentTypeInfos.Add(cts);
                            }
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
            return contentTypeInfos;
        }
        [Obsolete("Please use another method which contains site id in the parameters.")]
        public List<string>GetAllListContentTypes(Guid webId)
        {
            return GetAllListContentTypes(Guid.Empty, webId);
        }
        /// <summary>
        /// 获取Contenttype InfoTree
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="contentTypeInfo"></param>
        /// <param name="siteId"></param>
        /// <param name="parentIdList"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList)
        {
            mQuerySessionSchema.GetParentContentTypeInfoTree(contentTypeInfo, siteId, parentIdList);
        }

        /// <summary>
        /// 删除Datajunction表中特定的某条记录
        /// 无API实现
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldId"></param>
        /// <param name="sourceListId"></param>
        /// <param name="version"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Field-001")]
        public void RemoveDataJunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version)
        {
            string cmdText = "Delete From AllUserDataJunctions Where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_UIVersion=@UIVersion And tp_FieldId=@FieldId And tp_DocId=@DocId And tp_SourceListId=@ListId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@UIVersion", version);
                mQueryWorker.AddParameter("@SiteId", item.ParentList.ParentWeb.Site.ID);
                mQueryWorker.AddParameter("@FieldId", fieldId);
                mQueryWorker.AddParameter("@DocId", item.UniqueId);
                mQueryWorker.AddParameter("@ListId", sourceListId);
                mQueryWorker.Command.CommandText = cmdText;
                mQueryWorker.Command.ExecuteNonQuery();
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
        /// 获取List下ContentTypes,此重载不建议13、16使用。
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use another method to improve the performance.")]
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        public string GetContentTypeContent(Guid listId, Guid webId)
        {
            mQueryWorker.ClearParameters();
            try
            {
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@WebId", webId);
                string cmdText = @"select tp_ContentTypes from AllLists WITH(NOLOCK) where tp_WebId=@WebId and tp_ID=@ListId";
                string contentTypesContent = null;
                byte[] content = mQueryWorker.ExecuteScalar(cmdText) as byte[];
                if (content != null)
                {
                    contentTypesContent = AveCompressedUtility.GetTCompressedString(content);
                }
                return contentTypesContent;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeContentError, ex);
                return null;
            }

        }
        
        [QueryReview("2012/12/17", "Austin Han")]
        public string GetContentTypeSchema(Guid siteId, Guid listId, Guid webId)
        {
            mQueryWorker.ClearParameters();
            try
            {
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@WebId", webId);
                string cmdText = @"select tp_ContentTypes from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_WebId=@WebId and tp_ID=@ListId";
                string contentTypesContent = null;
                byte[] content = mQueryWorker.ExecuteScalar(cmdText) as byte[];
                if (content != null)
                {
                    contentTypesContent = AveCompressedUtility.GetTCompressedString(content);
                }
                return contentTypesContent;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeContentError, ex);
                return null;
            }

        }

        /// <summary>
        /// 查询site下某个Field是否存在
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="siteId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId)
        {
            return mQuerySessionSchema.GetFieldInSiteChildren(scope, siteId, fieldId);
        }

        /// <summary>
        /// 获取Site中Scope下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope)
        {
            return mQuerySessionSchema.GetContentTypeInfos(siteId, scope);
        }

        /// <summary>
        /// 查询Field的Definition
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<string> GetFields(Guid siteId, string scope)
        {
            return mQuerySessionSchema.GetFields(siteId, scope);
        }

        /// <summary>
        /// 查询ContentType在site下是否存在
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="ctId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool CheckContentTypeExist(Guid siteId, byte[] ctId)
        {
            return mQuerySessionSchema.CheckContentTypeExist(siteId, ctId);
        }

        /// <summary>
        /// 判断特定ContentType在Site下是否存在
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scope"></param>
        /// <param name="ctId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, byte[] ctId)
        {
            try
            {
                string cmdTxt = @"WITH CT
                                                    AS
                                                    (SELECT * FROM 
                                                    TVF_ContentTypes_SiteClassCTId(
                                                    @SiteId, 1, @ContentTypeId))
                                                    SELECT COUNT(ContentTypeId) FROM CT WITH(NOLOCK) WHERE SCOPE LIKE @Scope";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@Scope", scope.TrimStart('/') + "/%");
                mQueryWorker.AddParameter("@ContentTypeId", ctId);

                if (((int)mQueryWorker.ExecuteScalar(cmdTxt)) > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.CheckIfContentTypeExistInChildrenError, ex);
            }
            return false;
        }

        /// <summary>
        /// 根据ContenTypeId查询contentType的Name
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Use parameter instead of string.")]
        public string GetWebCTNameById(Guid siteId, string contentTypeId)
        {
            return mQuerySessionSchema.GetWebCTNameById(siteId, contentTypeId);
        }

        /// <summary>
        /// 获取Item存储在AllUserDatajunction中的信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="infoItem"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo infoItem)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetUserDataJunction"))
            {

                if (infoItem.RowId <= 0)
                {
                    return null;
                }
                string cmdText = @"SELECT tp_FieldId,tp_Id,tp_UIVersion,tp_Ordinal,tp_SourceListId
                                       FROM AllUserDataJunctions WITH(NOLOCK)
                                       WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@DocId AND tp_UIVersion=@Version
                                       ORDER BY tp_FieldId,tp_Ordinal";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", infoItem.SiteId);
                mQueryWorker.AddParameter("@ParentId", infoItem.ParentId);
                mQueryWorker.AddParameter("@DocId", infoItem.GUID);
                mQueryWorker.AddParameter("@Version", infoItem.Version);
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                try
                {
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, object> dataCache = new Dictionary<string, object>();
                            AveQueryUtility.GetDBRow(dataCache, dr);
                            data.Add(dataCache);
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
                if (data != null && data.Count > 0)
                {
                    return data;
                }

                return null;

            }

        }

        /// <summary>
        /// 向AllUserDataJunction表中中插入一条记录
        /// 无API实现
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fieldId"></param>
        /// <param name="sourceListId"></param>
        /// <param name="id"></param>
        /// <param name="ordinal"></param>
        /// <param name="version"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "This is an insert command.")]
        public void InsertIntoAllUserDataJunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
            string cmdText = @"select tp_SiteId,tp_DeleteTransactionId,tp_IsCurrentVersion,tp_ParentId,
                    tp_DocId,tp_CalculatedVersion,tp_Level,tp_UIVersion from AllUserData WITH(NOLOCK) where
                    tp_ID=@rowId and tp_DocId=@docId and tp_UIVersion=@UIVersion";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@rowId", item.ID);
                mQueryWorker.AddParameter("@docId", item.UniqueId);
                mQueryWorker.AddParameter("@UIVersion", version);
                mQueryWorker.Command.CommandText = cmdText;

                AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllUserDataJunctions");
                manager.LoadColumnsInfo(null, mQueryWorker.Command);
                manager.ResetColumnValue("tp_FieldId", fieldId);
                manager.ResetColumnValue("tp_SourceListId", sourceListId);
                manager.ResetColumnValue("tp_Id", id);
                manager.ResetColumnValue("tp_Ordinal", ordinal);
                manager.MakeInsertCommand(mQueryWorker.Command);

                if (mQueryWorker.Command.Parameters.Count > 0)
                {
                    mQueryWorker.Command.ExecuteNonQuery();
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
        /// 获取Item存储在AllUserDatajunction中的信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="infoItem"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> GetFolderItemsUserDataJunctions(Guid siteId, Guid parentId, int maxRow)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AveQueryService.GetFolderItemsUserDataJunctions"))
            {
                string cmdText = string.Format(@"SELECT TOP({0}) tp_FieldId,tp_Id,tp_UIVersion,tp_Ordinal,tp_SourceListId,tp_DocId
                                       FROM AllUserDataJunctions WITH(NOLOCK)
                                       WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion>=0 AND tp_ParentId=@ParentId
                                       ORDER BY tp_DocId,tp_UIVersion,tp_FieldId,tp_Ordinal", maxRow);
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                var data = new Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>>();
                try
                {
                    Guid currentItemId = Guid.Empty;
                    int currentUIVersion = -1;
                    Dictionary<int, List<Dictionary<string, object>>> currentItemData = null;
                    List<Dictionary<string, object>> currentVersionData = null;
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, object> dataCache = new Dictionary<string, object>();
                            AveQueryUtility.GetDBRow(dataCache, dr);
                            var itemId = (Guid)dataCache["tp_DocId"];
                            var uiVersion = (int)dataCache["tp_UIVersion"];
                            dataCache.Remove("tp_DocId");
                            if (currentItemId != itemId)
                            {
                                currentItemId = itemId;
                                currentUIVersion = -1;
                                currentItemData = new Dictionary<int, List<Dictionary<string, object>>>();
                                data.Add(currentItemId, currentItemData);
                            }
                            if (currentUIVersion != uiVersion)
                            {
                                currentUIVersion = uiVersion;
                                currentVersionData = new List<Dictionary<string, object>>();
                                currentItemData.Add(currentUIVersion, currentVersionData);
                            }
                            currentVersionData.Add(dataCache);
                        }
                    }
                    return data;
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

        public Dictionary<Guid, Dictionary<int, List<Dictionary<string, object>>>> GetFolderItemsUserDataJunctions(Guid siteId, Guid parentId)
        {
            return GetFolderItemsUserDataJunctions(siteId, parentId, 1000);
        }


    }
}
