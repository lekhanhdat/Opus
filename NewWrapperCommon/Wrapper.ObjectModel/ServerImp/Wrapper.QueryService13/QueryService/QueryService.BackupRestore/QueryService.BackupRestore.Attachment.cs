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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        [QueryReview("2012/12/18", "Austin Han")]
        public Dictionary<int, Guid> GetListAttachmentFolderIds(Guid siteId, Guid attachmentRootFolderId)
        {
            try
            {
                var attachmentFolders = new Dictionary<int, Guid>();
                string cmdText = @"select LeafName, Id from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@AttachmentRootFolderId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AttachmentRootFolderId", attachmentRootFolderId);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                {
                    int itemId;
                    while (sr.Read())
                    {
                        string name = sr.GetString(0);
                        if (int.TryParse(name, out itemId))
                        {
                            attachmentFolders.Add(itemId, sr.GetGuid(1));
                        }
                    }
                }
                return attachmentFolders;
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
        /// 重命名Attachment的Name
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Attach-001")]
        public void RenameAttachment(AveBaseItemInfo info, string oldName, string newName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@LeafName", oldName);
            mQueryWorker.AddParameter("@Name", newName);
            string cmdText = @"Update AllDocs set LeafName=@Name WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ";
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllPublishingVersions, RowOrdinalOption.None);
        }

        /// <summary>
        /// 根据Attachment的leafname获取attachment的UniqueId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="realName"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetAttachmentUniqueId(AveBaseItemInfo info, string realName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@LeafName", realName);

            string cmdText = @"SELECT Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName";
            return (Guid)mQueryWorker.ExecuteScalar(cmdText);
        }

        /// <summary>
        /// 获取Attachment的Internal version
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="realName"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We don't need internal version for SP2013.")]
        public int GetAttachmentVersion(AveBaseItemInfo info, string realName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@LeafName", realName);

            string cmdText = @"SELECT InternalVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ";
            return (int)mQueryWorker.ExecuteScalar(cmdText);
        }
        
        [QueryReview("2012/12/18", "Austin Han")]
        public void LoadAttachmentProperties(Guid UniqueId, byte Level, Guid siteId, Guid ParentListID, int ID, Dictionary<string, object> propertyList)
        {
            string queryAllDocs = @"Select TimeCreated, TimeLastModified 
From Alldocs With(noLock) Where SiteId=@SiteId And Id=@Id And Level=@Level ;";
            string queryUserData = @"Select tp_Modified, tp_Created, tp_Author, tp_Editor,{0},{1} 
From AllUserData With(noLock) Where tp_SiteId=@SiteId And tp_ListId=@ListId And tp_DeleteTransactionId=0x And tp_IsCurrentVersion=1 
And tp_ID=@Id And tp_CalculatedVersion=0 And tp_Level=@Level And tp_RowOrdinal=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", UniqueId);
            mQueryWorker.AddParameter("@Level", Level);
            using (var reader = mQueryWorker.ExecuteReader(queryAllDocs))
            {
                if (reader.Read())
                {
                    propertyList["TimeCreated"] = reader.GetDateTime(0);
                    propertyList["TimeModified"] = reader.GetDateTime(1);
                }
            }

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", ParentListID);
            mQueryWorker.AddParameter("@Id", ID);
            mQueryWorker.AddParameter("@Level", Level);
            var queryScript = string.Empty;
            if (propertyList["TP_CreatedByColumn"] == null || string.IsNullOrEmpty(propertyList["TP_CreatedByColumn"].ToString()))
            {
                queryScript = string.Format(queryUserData, "nvarchar1", "nvarchar2");
            }
            else
            {
                queryScript = string.Format(queryUserData, propertyList["TP_ModifiedByColumn"], propertyList["TP_CreatedByColumn"]);
            }

            using (var reader = mQueryWorker.ExecuteReader(queryScript))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        propertyList["TP_TimeModified"] = reader.GetDateTime(0);
                    }
                    if (!reader.IsDBNull(1))
                    {
                        propertyList["TP_TimeCreated"] = reader.GetDateTime(1);
                    }
                    if (!reader.IsDBNull(2))
                    {
                        propertyList["TP_Author"] = reader.GetInt32(2);
                    }
                    if (!reader.IsDBNull(3))
                    {
                        propertyList["TP_Editor"] = reader.GetInt32(3);
                    }
                    if (propertyList["TP_CreatedByColumn"] != null && string.IsNullOrEmpty(propertyList["TP_CreatedByColumn"].ToString()))
                    {
                        if (!reader.IsDBNull(4))
                        {
                            propertyList["TP_ModifiedBy"] = reader.GetString(4);
                        }
                        if (!reader.IsDBNull(5))
                        {
                            propertyList["TP_CreatedBy"] = reader.GetString(5);
                        }
                    }
                }
            }
        }
        
        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Attach-002")]
        public void SaveAttachmentProperties(Guid UniqueId, byte Level, Guid siteId, Guid ParentListID, int ID, Dictionary<string, object> propertyList)
        {
            string updateAllDocs = @"Update Alldocs Set TimeCreated=@TimeCreated, 
TimeLastModified=@TimeLastModified Where SiteId=@SiteId And Id=@Id And Level=@Level ;";
            string updateUserData = @"Update AllUserData Set tp_Modified=@tp_Modified,
tp_Created=@tp_Created, tp_Author=@tp_Author, tp_Editor=@tp_Editor Where tp_SiteId=@SiteId And tp_ListId=@ListId And tp_DeleteTransactionId=0x And tp_IsCurrentVersion=1 
And tp_ID=@Id And tp_CalculatedVersion=0 And tp_Level=@Level And tp_RowOrdinal=0";
            string updateUserDataWithModifiedBy = @"Update AllUserData Set tp_Modified=@tp_Modified,
tp_Created=@tp_Created, tp_Author=@tp_Author, tp_Editor=@tp_Editor, {0}=@ModifiedBy, {1}=@CreatedBy Where tp_SiteId=@SiteId And tp_ListId=@ListId And tp_DeleteTransactionId=0x And tp_IsCurrentVersion=1 
And tp_ID=@Id And tp_CalculatedVersion=0 And tp_Level=@Level And tp_RowOrdinal=0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", UniqueId);
            mQueryWorker.AddParameter("@Level", Level);
            mQueryWorker.AddParameter("@TimeCreated", propertyList["TimeCreated"]);
            mQueryWorker.AddParameter("@TimeLastModified", propertyList["TimeModified"]);
            mQueryWorker.ExecuteNonQuery(updateAllDocs);

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", ParentListID);
            mQueryWorker.AddParameter("@Id", ID);
            mQueryWorker.AddParameter("@Level", Level);
            mQueryWorker.AddParameter("@tp_Modified", propertyList["TP_TimeModified"]);
            mQueryWorker.AddParameter("@tp_Created", propertyList["TP_TimeCreated"]);
            mQueryWorker.AddParameter("@tp_Author", propertyList["TP_Author"]);
            mQueryWorker.AddParameter("@tp_Editor", propertyList["TP_Editor"]);
            if (propertyList["TP_CreatedByColumn"] == null || string.IsNullOrEmpty(propertyList["TP_CreatedByColumn"].ToString()))
            {
                mQueryWorker.ExecuteNonQuery(updateUserData);
            }
            else
            {
                mQueryWorker.AddParameter("@ModifiedBy", propertyList["TP_ModifiedBy"]);
                mQueryWorker.AddParameter("@CreatedBy", propertyList["TP_CreatedBy"]);
                mQueryWorker.ExecuteNonQuery(string.Format(updateUserDataWithModifiedBy, propertyList["TP_ModifiedByColumn"], propertyList["TP_CreatedByColumn"]));
            }

            mQueryWorker.ClearParameters();
        }
        
        /// <summary>
        /// 获取Attachment的Size属性
        /// 有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetAttachmentSize(AveBaseItemInfo info)
        {
            int length = 0;
            try
            {
                string cmdText = @"Select Size From AllDocs With(noLock) Where SiteId =@SiteId
                                      And DeleteTransactionId=0x And Id=@Id And UIVersion=@Version";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Version", info.Version);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        length = dr.GetInt32(0);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Info("Get Attachment Size Error, AttachmentName:{0},Exception:{1}", info.Name, e.ToString());
            }
            return length;
        }

        /// <summary>
        /// 获取AttachmentUrl下的所有Attachment的leafName
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listUrl"></param>
        /// <param name="siteId"></param>
        /// <param name="attachmentUrl"></param>
        /// <param name="attachments"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void SetAttachment(string listUrl, Guid siteId, string attachmentUrl, List<string> attachments)
        {
            try
            {
                StringBuilder cmdText = new StringBuilder();
                cmdText.Append("SELECT LeafName FROM AllDocs WITH(NOLOCK)");
                cmdText.Append("WHERE SiteId=@SiteId AND DirName=@AttachmentUrl AND DeleteTransactionId=0x AND (Level=1 OR Level=2 OR Level=255)");
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AttachmentUrl", attachmentUrl);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                {
                    while (sr.Read())
                    {
                        string attName = sr.GetString(0);
                        attachments.Add(attName);
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
        /// 查询ParentId和LeafName下是否有Attachment
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool IsAttachmentExist(Guid siteId, Guid parentId, string leafName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@LeafName", leafName);

            string cmdText = @"SELECT count(Id) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
            return ((int)mQueryWorker.ExecuteScalar(cmdText) > 0);
        }

        /// <summary>
        /// 获取特定Attachment的Title，TimeCreated等信息
        /// </summary>
        /// <param name="baseItemInfo"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin han")]
        public Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo)
        {
            Dictionary<string, object> dataCache = new Dictionary<string, object>();
            string cmdText =
@"SELECT LeafName as Title, TimeCreated as Created, TimeLastModified as Modified, MetaInfo
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
            if (baseItemInfo.ParentId != Guid.Empty)
            {
                cmdText += " ParentID=@ParentID AND ";
            }
            else
            {
                logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
            }
            cmdText += " Id=@Id AND UIVersion=@Version";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", baseItemInfo.SiteId);
            mQueryWorker.AddParameter("@ParentID", baseItemInfo.ParentId);
            mQueryWorker.AddParameter("@Id", baseItemInfo.GUID);
            mQueryWorker.AddParameter("@Version", baseItemInfo.Version);
            AveQueryUtility.TryGetDBRow(dataCache, mQueryWorker, cmdText);
            return dataCache;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public List<string> GetAttachments(Guid siteId, Guid attachmentFolderId)
        {
            try
            {
                var attachments = new List<string>();
                string cmdText = @"select LeafName from AllDocs(nolock) where SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@AttachmentFolderId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AttachmentFolderId", attachmentFolderId);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                {
                    while (sr.Read())
                    {
                        string attName = sr.GetString(0);
                        attachments.Add(attName);
                    }
                }
                return attachments;
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
    }
}
