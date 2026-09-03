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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Data;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    /// <summary>
    /// 处理WebPart,view相关的sql语句
    /// </summary>
    internal partial class AveQueryService
    {
        /// <summary>
        /// 更新WebPart的tp_Level,tp_PageVersion等信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="pageVersion"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        /// <param name="isCurrentVersion"></param>
        /// <param name="uIVersion"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        public void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@Level", oldLevel);
            mQueryWorker.AddParameter("@ID", webPartId);
            mQueryWorker.AddParameter("@IsCurrentVersion", isCurrentVersion);

            string cmdText = @"UPDATE AllWebParts SET ";
            if (newLevel != oldLevel)
            {
                cmdText += "tp_Level=@NewLevel,";
                mQueryWorker.AddParameter("@NewLevel", newLevel);
            }
            if (pageVersion != 0 && pageVersion < uIVersion)
            {
                cmdText += "tp_PageVersion=@PageVersion,";
                mQueryWorker.AddParameter("@PageVersion", pageVersion);
            }
            cmdText += "tp_IsCurrentVersion=@IsCurrentVersion where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0  AND tp_Level=@Level AND tp_ID=@ID";
            mQueryWorker.ExecuteNonQuery(cmdText);

            try
            {
                cmdText = @"SELECT COUNT(tp_Level) FROM WebPartLists WITH(NOLOCK) where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@SourceLevel";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteID", siteId);
                mQueryWorker.AddParameter("@WebPartID", webPartId);
                mQueryWorker.AddParameter("@PageID", fileId);
                mQueryWorker.AddParameter("@CurPageLevel", oldLevel);
                mQueryWorker.AddParameter("@SourceLevel", newLevel);

                if ((int)mQueryWorker.ExecuteScalar(cmdText) == 0)
                {
                    cmdText = @"UPDATE WebPartLists Set tp_Level=@SourceLevel where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@CurPageLevel";
                    mQueryWorker.ExecuteNonQuery(cmdText);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error while update page level in WebPartList. Error: " + ex.ToString());
            }

        }

        /// <summary>
        /// 获取View的LastModifiedTime属性
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="spFile"></param>
        /// <param name="timeLastModified"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateViewLastModifiedTimeByNative(AveBaseItemInfo info, IAveFile spFile, DateTime timeLastModified)
        {
            string cmdStr = @"UPDATE AllDocs SET TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND ParentId=@ParentId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TimeLastModified", timeLastModified);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@ParentId", spFile.ParentFolder.UniqueId);
            mQueryWorker.AddParameter("@ID", spFile.UniqueId);
            mQueryWorker.AddParameter("@UIVersion", spFile.UIVersion);
            mQueryWorker.ExecuteNonQuery(cmdStr, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        /// <summary>
        /// 获取List下所有Views的信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="viewCache"></param>
        /// <param name="listId"></param>
        /// <param name="defaultViewId"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use another method to improve the performance.")]
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid listId, Guid defaultViewId)
        {
            viewCache.Clear();
            string cmdText = @"select tp_ID,tp_DisplayName,tp_Type,tp_PageUrlID,tp_Flags,tp_BaseViewID,tp_UserID 
from AllWebParts WITH(NOLOCK)
where tp_ListId =@listid AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND (tp_Type=1  or tp_Type=0) AND tp_DisplayName<> '' ";

            try
            {
                mQueryWorker.Command.Parameters.Clear();
                mQueryWorker.AddParameter("@listid", listId);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sdr.Read())
                    {
                        if (sdr.IsDBNull(1))
                        {
                            continue;
                        }
                        Guid viewPageId = sdr.GetGuid(3);
                        if (!viewCache.ContainsKey(viewPageId))
                        {
                            viewCache.Add(viewPageId, new List<AveViewInfo>());
                        }
                        List<AveViewInfo> views = viewCache[viewPageId];
                        AveViewInfo viewInfo = new AveViewInfo();
                        viewInfo.Id = sdr.GetGuid(0);
                        viewInfo.Title = sdr.GetString(1);
                        try
                        {
                            if (!sdr.IsDBNull(5))
                            {
                                viewInfo.BaseViewId = sdr.GetByte(5);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetViewsError, ex);
                        }

                        if (!sdr.IsDBNull(6))
                        {
                            viewInfo.UserID = sdr.GetInt32(6);
                        }
                        try
                        {
                            bool isDefaultView = false;
                            if (defaultViewId.Equals(viewInfo.Id))
                            {
                                isDefaultView = true;
                            }
                            viewInfo.IsDefaultView = isDefaultView;
                        }
                        catch (Exception e)
                        {
                            viewInfo.IsDefaultView = false;
                            logger.Warn("An error occurred when getting view:{0}. Reason:{1}.", viewInfo.Title, e);
                        }
                        int i = 0;
                        //CI-17960:在某种情况下tp_flags字段会为空，这个时候就将tp_flags置为0，但是这样还到目的端就变为standard view
                        if (!sdr.IsDBNull(4))
                        {
                            i = Convert.ToInt32(sdr[4]);
                        }
                        uint mFlags = (uint)i;
                        //if ((mFlags & 0x1000) != 0)
                        //{
                        //    if ((mFlags & 0x200000) != 0)
                        //    {
                        //        viewInfo.Scope = (AveViewScope)1;
                        //    }
                        //    else
                        //    {
                        //        viewInfo.Scope = (AveViewScope)2;
                        //    }
                        //}
                        //else if ((mFlags & 0x200000) != 0)
                        //{
                        //    viewInfo.Scope = (AveViewScope)3;
                        //}
                        //else
                        //{
                        //    viewInfo.Scope = (AveViewScope)0;
                        //}

                        //viewInfo.IncludeRootFolder = (mFlags & 0x8000000) != 0;
                        //应用list template mapping时会在目的端显示隐藏的view，所以备份Hidden属性
                        viewInfo.Hidden = (mFlags & 8) != 0;
                        //viewInfo.DefaultViewForContentType = (mFlags & 0x10000000) != 0;
                        //viewInfo.EditorModified = (mFlags & 2) != 0;

                        viewInfo.IsMobileView = (mFlags & 0x800000) != 0;
                        viewInfo.IsDefaultMobileView = (mFlags & 0x1000000) != 0;
                        viewInfo.IsPersonal = (i & 262144) == 262144 ? true : false;
                        viewInfo.ViewType = i;
                        views.Add(viewInfo);
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

        [QueryReview("2012/12/17", "Austin Han")]
        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache, Guid siteId, Guid listId, Guid defaultViewId)
        {
            viewCache.Clear();
            string cmdText = @"select tp_ID,tp_DisplayName,tp_Type,tp_PageUrlID,tp_Flags,tp_BaseViewID,tp_UserID 
from AllWebParts webpart WITH(NOLOCK)
inner join AllDocs  docs WITH(NOLOCK) on docs.SiteId =  webpart.tp_SiteId and docs.Id = webpart.tp_PageUrlID and docs.DoclibRowId is  null
where tp_SiteId=@SiteId AND tp_ListId =@listid AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND (tp_Type=1  or tp_Type=0) AND tp_DisplayName<> '' ";

            try
            {
                mQueryWorker.Command.Parameters.Clear();
                mQueryWorker.AddParameter("@listid", listId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sdr.Read())
                    {
                        if (sdr.IsDBNull(1))
                        {
                            continue;
                        }
                        Guid viewPageId = sdr.GetGuid(3);
                        if (!viewCache.ContainsKey(viewPageId))
                        {
                            viewCache.Add(viewPageId, new List<AveViewInfo>());
                        }
                        List<AveViewInfo> views = viewCache[viewPageId];
                        AveViewInfo viewInfo = new AveViewInfo();
                        viewInfo.Id = sdr.GetGuid(0);
                        viewInfo.Title = sdr.GetString(1);
                        try
                        {
                            if (!sdr.IsDBNull(5))
                            {
                                viewInfo.BaseViewId = sdr.GetByte(5);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetViewsError, ex);
                        }

                        if (!sdr.IsDBNull(6))
                        {
                            viewInfo.UserID = sdr.GetInt32(6);
                        }
                        try
                        {
                            bool isDefaultView = false;
                            if (defaultViewId.Equals(viewInfo.Id))
                            {
                                isDefaultView = true;
                            }
                            viewInfo.IsDefaultView = isDefaultView;
                        }
                        catch (Exception e)
                        {
                            viewInfo.IsDefaultView = false;
                            logger.Warn("An error occurred when getting view:{0}. Reason:{1}.", viewInfo.Title, e);
                        }
                        int i = 0;
                        //CI-17960:在某种情况下tp_flags字段会为空，这个时候就将tp_flags置为0，但是这样还到目的端就变为standard view
                        if (!sdr.IsDBNull(4))
                        {
                            i = Convert.ToInt32(sdr[4]);
                        }
                        uint mFlags = (uint)i;
                        //if ((mFlags & 0x1000) != 0)
                        //{
                        //    if ((mFlags & 0x200000) != 0)
                        //    {
                        //        viewInfo.Scope = (AveViewScope)1;
                        //    }
                        //    else
                        //    {
                        //        viewInfo.Scope = (AveViewScope)2;
                        //    }
                        //}
                        //else if ((mFlags & 0x200000) != 0)
                        //{
                        //    viewInfo.Scope = (AveViewScope)3;
                        //}
                        //else
                        //{
                        //    viewInfo.Scope = (AveViewScope)0;
                        //}

                        //viewInfo.IncludeRootFolder = (mFlags & 0x8000000) != 0;
                        //应用list template mapping时会在目的端显示隐藏的view，所以备份Hidden属性
                        viewInfo.Hidden = (mFlags & 8) != 0;
                        //viewInfo.DefaultViewForContentType = (mFlags & 0x10000000) != 0;
                        //viewInfo.EditorModified = (mFlags & 2) != 0;

                        viewInfo.IsMobileView = (mFlags & 0x800000) != 0;
                        viewInfo.IsDefaultMobileView = (mFlags & 0x1000000) != 0;
                        viewInfo.IsPersonal = (i & 262144) == 262144 ? true : false;
                        viewInfo.ViewType = i;
                        views.Add(viewInfo);
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

            
        /// <summary>
        /// 更新WebPart的View信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="baseViewId"></param>
        /// <param name="view"></param>
        /// <param name="contentTypeId"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        public void UpdateView(Guid webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId, string displayName)
        {

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            bool needUpdateContentType = contentTypeId != null;
            bool needUpdateDisplayName = !string.IsNullOrEmpty(displayName);
            if (needUpdateContentType)
            {
                mQueryWorker.AddParameter("@ContentType", contentTypeId);
            }
            if (needUpdateDisplayName)
            {
                mQueryWorker.AddParameter("@DisplayName", displayName);
            }
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@ID", webPartId);
            string cmdText = string.Empty;
            if (baseViewId >= 0)
            {
                mQueryWorker.AddParameter("@BaseViewID", baseViewId);
                if (view != null)
                {
                    mQueryWorker.AddParameter("@View", view);
                    cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=@BaseViewID,tp_View=@View " +
                        (needUpdateContentType ? ", tp_ContentTypeId=@ContentType" : "") + (needUpdateDisplayName ? ", tp_DisplayName=@DisplayName" : "") + " where tp_SiteId=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1)  AND tp_PageUrlID=@PageID AND tp_ID=@ID";
                }
                else
                {
                    cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=@BaseViewID,tp_View = NULL " +
                        (needUpdateContentType ? ", tp_ContentTypeId=@ContentType " : "") + (needUpdateDisplayName ? ", tp_DisplayName=@DisplayName" : "") + " where tp_SiteId=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_PageUrlID=@PageID AND tp_ID=@ID";
                }
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
            else
            {
                if (view != null)
                {
                    mQueryWorker.AddParameter("@View", view);
                    cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=NULL,tp_View=@View " +
                        (needUpdateContentType ? ", tp_ContentTypeId=@ContentType" : "") + (needUpdateDisplayName ? ", tp_DisplayName=@DisplayName" : "") + " where tp_SiteId=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_PageUrlID=@PageID AND tp_ID=@ID";
                }
                else
                {
                    cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=NULL,tp_View = NULL " +
                        (needUpdateContentType ? ", tp_ContentTypeId=@ContentType " : "") + (needUpdateDisplayName ? ", tp_DisplayName=@DisplayName" : "") + " where tp_SiteId=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_PageUrlID=@PageID AND tp_ID=@ID";
                }
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
        }

        /// <summary>
        /// 更新WebPart的User信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="userId"></param>
        /// <param name="isPersonal"></param>
        [QueryReview("2012/12/18", "Austin Han", true, "Add siteid for WebPartLists table")]
        public void UpdateWebPartUserID(Guid webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@UserID", userId);
            mQueryWorker.AddParameter("@ID", webPartId);
            if (isPersonal)
            {
                string command = "SELECT * FROM Personalization WITH(NOLOCK) WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@UserID";
                if (mQueryWorker.ExecuteScalar(command) != null)
                {
                    command = "DELETE Personalization WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@UserID";
                    mQueryWorker.ExecuteNonQuery(command, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                }

                mQueryWorker.AddParameter("@CurrentUserID", currentUserId);
                command = "UPDATE Personalization SET tp_UserID=@UserID WHERE tp_SiteId=@SiteID AND tp_WebPartID=@ID AND tp_PageUrlId=@PageId AND tp_UserID=@CurrentUserID";

                mQueryWorker.ExecuteNonQuery(command, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
            else
            {
                string command = "UPDATE AllWebParts SET tp_userId=@UserID WHERE tp_siteid=@SiteID AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_PageUrlID=@PageID  AND tp_id=@ID";
                mQueryWorker.ExecuteNonQuery(command, VersionOption.OneItemOrVersion, RowOrdinalOption.None);

                command = "UPDATE WebPartLists SET tp_userId=@UserID WHERE  tp_webpartid=(select top(1) tp_ID from AllWebParts where tp_siteid=@SiteID AND tp_id=@ID)";
                mQueryWorker.ExecuteNonQuery(command, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
        }

        [QueryReview("2012/12/18", "Austin Han", true, "Change tp_ID to tp_WebPartID because there is no tp_ID in Personalization table.")]
        public void UpdatePersonalPropertiesByNative(Guid webPartId, Guid siteId, int currentUserId, byte[] perUserBytes)
        {
            mQueryWorker.ClearParameters();
            string cmdText = @"UPDATE Personalization SET tp_PerUserProperties=@PerUserProperties where tp_SiteId=@SiteId AND tp_WebPartID=@ID AND tp_UserId=@UserId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@PerUserProperties", perUserBytes);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ID", webPartId);
            mQueryWorker.AddParameter("@UserId", currentUserId);

            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        /// <summary>
        /// 获取List下View的Schema信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public string GetListViewSchema(Guid siteId, Guid listId)
        {
            string viewFieldsSchema = null;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                string cmdText = @"select tp_View from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteId and tp_ListId=@ListId and tp_Type=0";
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] bytes = dr["tp_View"] as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            viewFieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                logger.Log(AveLogLevel.ERROR, new AveQueryException(string.Format("Exception Error Code----{0}", queryException.Number), queryException).ToString());
            }
            catch (AveQueryException ex)
            {
                logger.Log(AveLogLevel.ERROR, ex.ToString());
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, new AveQueryException(e.Message, e).ToString());
            }
            return viewFieldsSchema;
        }

        /// <summary>
        /// 获取webpart关联的Personalization表信息
        /// </summary>
        /// <param name="webPartInfo"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void SetWebPartPersonalization(AveWebPartBaseInfo webPartInfo, Guid siteId, Guid itemId)
        {
            string cmdText =
                @"SELECT tp_UserID,tp_PartOrder,tp_ZoneID,tp_IsIncluded,tp_FrameState,tp_PerUserProperties,tp_Cache,tp_Size,tp_Deleted 
                        FROM Personalization WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@WebPartID", webPartInfo.ID);
            webPartInfo.Personalization = AveQueryUtility.GetDBRows<AvePersonalizationInfo>(mQueryWorker, cmdText, "tp_");
        }

        [QueryReview("2012/12/18", "qwhu")]
        [QueryReview("WP-001")]
        public void MoveWebPartProperty(Guid siteId, Guid fileId, Guid fromWebPartId, Guid toWebPartId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@FromID", fromWebPartId);
            string cmdText = @"select tp_AllUsersProperties,tp_PerUserProperties from AllWebParts with(nolock)
                                    where tp_SiteId =@SiteID and tp_PageUrlID=@PageID and tp_ID=@FromID and tp_PageVersion=0 and tp_IsCurrentVersion=1";
            object allUsersProperties = null;
            object perUserProperties = null;
            using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
            {
                if (reader.Read())
                {
                    allUsersProperties = reader[0];
                    perUserProperties = reader[1];
                }
                else
                {
                    return;
                }
            }
            mQueryWorker.AddParameter("@AllUsersProperties", allUsersProperties);
            mQueryWorker.AddParameter("@PerUserProperties", perUserProperties);
            mQueryWorker.AddParameter("@ToID", toWebPartId);

            cmdText = @"delete from AllWebParts where tp_SiteId =@SiteID and tp_PageUrlID=@PageID and tp_ID=@FromID and tp_PageVersion=0 and tp_IsCurrentVersion=1";
            mQueryWorker.ExecuteNonQuery(cmdText);

            cmdText = @"update AllWebParts set tp_AllUsersProperties=@AllUsersProperties, tp_PerUserProperties=@PerUserProperties
                        WHERE tp_SiteId=@SiteID  AND tp_PageUrlID=@PageID AND tp_ID=@ToID";
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public void DeleteAllPersonalWebParts(Guid siteId, Guid docId, int level, List<Guid> viewIds)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", docId);
            mQueryWorker.AddParameter("@Level", level);
            string cmdText = @"select tp_ID from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_Level=@Level AND tp_UserID > 0";
            List<Guid> ids = new List<Guid>();
            using (SqlDataReader sdr = mQueryWorker.ExecuteReader(cmdText))
            {
                while (sdr.Read())
                {
                    Guid id = (Guid)sdr[0];
                    ids.Add(id);
                }
            }
            if (viewIds != null)
            {
                foreach (Guid viewId in viewIds)
                {
                    if (ids.Contains(viewId))
                    {
                        ids.Remove(viewId);
                    }
                }
            }
            if (ids == null || ids.Count == 0)
            {
                return;
            }
            cmdText = "delete from AllWebParts where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_Level=@Level AND tp_UserID > 0 AND tp_ID in ({0})";
            mQueryWorker.ExecuteNonQueryByCount(ids, 100, cmdText);
        }

        /// <summary>
        /// 删除Document上的特定WebPart
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="docId"></param>
        /// <param name="webPartId"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void DeleteWebPartByNative(Guid siteId, Guid docId, Guid webPartId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", docId);
            mQueryWorker.AddParameter("@ID", webPartId);
            string cmdText = @"delete from AllWebParts where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_ID=@ID";
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        /// <summary>
        /// 获取webpart关联的WebPartLists表信息
        /// </summary>
        /// <param name="webPartInfo"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="level"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void SetWebPartLists(AveWebPartBaseInfo webPartInfo, Guid siteId, Guid itemId, byte level)
        {
            mQuerySessionSchema.SetWebPartLists(webPartInfo,siteId,itemId,level);
        }

        /// <summary>
        /// 获取Document下特定Version的所有WebParts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="itemlevel"></param>
        /// <param name="itemIsVersion"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Add SiteId and remove order by to improve the performance")]
        public List<AveWebPartBaseInfo> GetWebParts(Guid siteId, Guid itemId, byte itemlevel, bool itemIsVersion, int version)
        {
            string cmdText =
               @"SELECT wp.tp_ID,wp.tp_ListId,wp.tp_Type,wp.tp_Flags,wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_Version,wp.tp_PartOrder,wp.tp_ZoneID,
                                 wp.tp_IsIncluded,wp.tp_FrameState,wp.tp_View,wp.tp_WebPartTypeId,wp.tp_AllUsersProperties,wp.tp_PerUserProperties,
                                 wp.tp_Cache,wp.tp_UserID,wp.tp_Source,wp.tp_CreationTime,wp.tp_Size,wp.tp_Level,wp.tp_Deleted,wp.tp_HasFGP,
                                 wp.tp_ContentTypeId,wp.tp_PageVersion,wp.tp_SolutionId,wp.tp_IsCurrentVersion,wp.tp_Assembly,wp.tp_Class,wp.tp_WebPartIdProperty
                        FROM AllWebParts wp WITH(NOLOCK)
                        WHERE wp.tp_SiteId=@SiteId AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND wp.tp_PageUrlId=@Id AND wp.tp_Level=@Level AND wp.tp_PageVersion=@PageVersion"; // order by wp.tp_PartOrder ASC
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@Level", itemlevel);
            mQueryWorker.AddParameter("@PageVersion", itemIsVersion ? version : 0);
            List<AveWebPartBaseInfo> data = AveQueryUtility.GetDBRows<AveWebPartBaseInfo>(mQueryWorker, cmdText, "tp_");
            return data;
        }
        
        /// <summary>
        /// change webpart id
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="id"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateWebPartInfo(Guid oldId, Guid siteId, Guid fileId, Guid newId)
        {
            string cmdText = @"UPDATE AllWebParts SET tp_Id=@ID where tp_SiteId=@SiteID AND tp_IsCurrentVersion=1 AND tp_PageUrlID=@PageID AND tp_ID=@WebPartId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@ID", newId);
            mQueryWorker.AddParameter("@WebPartId", oldId);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);

            try
            {
                cmdText = @"SELECT COUNT(tp_WebPartID) FROM WebPartLists WITH(NOLOCK) where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@ID";

                if ((int)mQueryWorker.ExecuteScalar(cmdText) == 0)
                {
                    cmdText = @"UPDATE WebPartLists Set tp_WebPartID=@ID where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID";
                    mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                }
                else
                {
                    cmdText = "DELETE FROM WebPartLists WHERE tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID";
                    mQueryWorker.ExecuteNonQuery(cmdText);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ChangeWebPartIDError, ex);
            }
        }

        public byte[] GetIListWebPartView(Guid siteId, Guid fileId, Guid webPartId)
        {
            string cmdText = @"select top(1) tp_View from AllWebParts where tp_SiteId=@SiteID and tp_PageUrlID=@PageID and tp_ID=@WebPartId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@WebPartId", webPartId);
            return (byte[])mQueryWorker.ExecuteScalar(cmdText);
        }

        public void SetIListWebPartView(Guid siteId, Guid fileId, Guid webPartId, byte[] view)
        {
            string cmdText = @"UPDATE AllWebParts SET tp_View=@View where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_ID=@WebPartId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@View", view);
            mQueryWorker.AddParameter("@WebPartId", webPartId);
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        /// <summary>
        /// 更新WebPart的AllUserProperties和perUserProperties
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="allUsersProperties"></param>
        /// <param name="perUserProperties"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        public void UpdateWebpartPropertiesByNative(Guid webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties)
        {

            mQueryWorker.ClearParameters();
            StringBuilder s = new StringBuilder("UPDATE AllWebParts SET tp_AllUsersProperties=");

            if (allUsersProperties != null)
            {
                s.Append("@AllUsersProperties,");
                mQueryWorker.AddParameter("@AllUsersProperties", allUsersProperties);
            }
            else
            {
                s.Append("NULL,");
            }

            if (perUserProperties != null)
            {
                s.Append("tp_PerUserProperties=@PerUserProperties ");
                mQueryWorker.AddParameter("@PerUserProperties", perUserProperties);
            }
            else
            {
                s.Append("tp_PerUserProperties=NULL ");
            }

            s.Append(@"WHERE tp_SiteId=@SiteID AND (tp_IsCurrentVersion=1 OR tp_IsCurrentVersion=0) AND tp_PageUrlID=@PageID AND tp_ID=@ID");
            mQueryWorker.AddParameter("@SiteID", siteId);
            mQueryWorker.AddParameter("@PageID", fileId);
            mQueryWorker.AddParameter("@ID", webPartId);

            mQueryWorker.ExecuteNonQuery(s.ToString(), VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        /// <summary>
        /// 获取View上的fields
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public string GetViewFields(Guid siteId, Guid listId)
        {
            string viewFieldsSchema = null;

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"select tp_View from AllWebParts WITH(NOLOCK) where tp_SiteId=@SiteId and (tp_IsCurrentVersion = 1 or tp_IsCurrentVersion =0) and tp_ListId=@ListId and tp_Type=0"; //0 means the webpart is in default view
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] bytes = dr["tp_View"] as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            viewFieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
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

            return viewFieldsSchema;
        }

        public void InternalAddWebPart(AveWebPartBaseInfo webPartInfo, Guid siteId, string dirName, string leafName, Guid webPartId)
        {
            using (SqlCommand cmd = GetCommandToCreateWebPart())
            {
                cmd.Parameters["@SiteId"].Value = siteId;
                cmd.Parameters["@DocDirName"].Value = dirName;
                cmd.Parameters["@DocLeafName"].Value = leafName;
                cmd.Parameters["@WebPartId"].Value = webPartId;
                cmd.Parameters["@UpdateQuota"].Value = true;
                if (webPartInfo.ListId != Guid.Empty)
                {
                    cmd.Parameters["@ListId"].Value = webPartInfo.ListId;
                }
                else
                {
                    cmd.Parameters["@ListId"].Value = DBNull.Value; 
                }
                if (webPartInfo.Type > 0)
                {
                    cmd.Parameters["@Type"].Value = webPartInfo.Type;
                }
                else
                {
                    cmd.Parameters["@Type"].Value = DBNull.Value;
                }
                if (webPartInfo.Flags > 0)
                {
                    cmd.Parameters["@Flags"].Value = webPartInfo.Flags;
                }
                else
                {
                    cmd.Parameters["@Flags"].Value = DBNull.Value;
                }
                if (!string.IsNullOrEmpty(webPartInfo.DisplayName))
                {
                    cmd.Parameters["@DisplayName"].Value = webPartInfo.DisplayName;
                }
                else
                {
                    cmd.Parameters["@DisplayName"].Value = DBNull.Value;
                }
                cmd.Parameters["@ContentTypeId"].Value = webPartInfo.ContentTypeId;
                if (webPartInfo.Version > 0)
                {
                    cmd.Parameters["@Version"].Value = webPartInfo.Version;
                }
                else
                {
                    cmd.Parameters["@Version"].Value = DBNull.Value;
                }
                cmd.Parameters["@PartOrder"].Value = webPartInfo.PartOrder;
                cmd.Parameters["@ZoneId"].Value = webPartInfo.ZoneID;
                if (webPartInfo.UserID > 0)
                {
                    cmd.Parameters["@UserId"].Value = webPartInfo.UserID;
                }
                else
                {
                    cmd.Parameters["@UserId"].Value = DBNull.Value;
                }
                cmd.Parameters["@IsIncluded"].Value = webPartInfo.IsIncluded;
                cmd.Parameters["@FrameState"].Value = webPartInfo.FrameState;
                cmd.Parameters["@WebPartTypeId"].Value = webPartInfo.WebPartTypeId;
                if (!string.IsNullOrEmpty(webPartInfo.Assembly))
                {
                    cmd.Parameters["@Assembly"].Value = webPartInfo.Assembly;
                }
                else
                {
                    cmd.Parameters["@Assembly"].Value = DBNull.Value;
                }
                if (!string.IsNullOrEmpty(webPartInfo.Class))
                {
                    cmd.Parameters["@Class"].Value = webPartInfo.Class;
                }
                else
                {
                    cmd.Parameters["@Class"].Value = DBNull.Value;
                }
                if (webPartInfo.SolutionId != Guid.Empty)
                {
                    cmd.Parameters["@SolutionId"].Value = webPartInfo.SolutionId;
                }
                else
                {
                    cmd.Parameters["@SolutionId"].Value = DBNull.Value;
                }
                cmd.Parameters["@SolutionWebId"].Value = DBNull.Value;
                if (webPartInfo.AllUsersProperties != null)
                {
                    cmd.Parameters["@AllUsersProperties"].Value = webPartInfo.AllUsersProperties;
                }
                else
                {
                    cmd.Parameters["@AllUsersProperties"].Value = DBNull.Value;
                }
                if (webPartInfo.PerUserProperties != null)
                {
                    cmd.Parameters["@PerUserProperties"].Value = webPartInfo.PerUserProperties;
                }
                else
                {
                    cmd.Parameters["@PerUserProperties"].Value = DBNull.Value;
                }
                cmd.Parameters["@WebPartIdProperty"].Value = webPartInfo.WebPartIdProperty;
                cmd.Parameters["@Cache"].Value = DBNull.Value;
                if (!string.IsNullOrEmpty(webPartInfo.Source))
                {
                    cmd.Parameters["@Source"].Value = webPartInfo.Source;
                }
                else
                {
                    cmd.Parameters["@Source"].Value = DBNull.Value;
                }
                if (webPartInfo.View != null)
                {
                    cmd.Parameters["@View"].Value = webPartInfo.View;
                }
                else
                {
                    cmd.Parameters["@View"].Value = DBNull.Value;
                }
                cmd.Parameters["@Level"].Value = webPartInfo.Level;
                if (webPartInfo.BaseViewID.HasValue)
                {
                    cmd.Parameters["@BaseViewId"].Value = webPartInfo.BaseViewID.Value;
                }
                else
                {
                    cmd.Parameters["@BaseViewId"].Value = DBNull.Value;
                }
                cmd.Parameters["@bRetainObjectIdentity"].Value = 1;
                cmd.Parameters["@bDeleteUsersOtherWebParts"].Value = 0;

                try
                {
                    cmd.ExecuteNonQuery();
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

        private SqlCommand GetCommandToCreateWebPart()
        {
            SqlCommand command = mQueryWorker.Connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "proc_AddNonListViewFormWebPartForUrl";
            command.Parameters.Add(new SqlParameter("@SiteId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@DocDirName", SqlDbType.NVarChar, 0x100));
            command.Parameters.Add(new SqlParameter("@DocLeafName", SqlDbType.NVarChar, 0x80));
            command.Parameters.Add(new SqlParameter("@WebPartId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@UpdateQuota", SqlDbType.Bit));
            command.Parameters.Add(new SqlParameter("@ListId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@Type", SqlDbType.TinyInt));
            command.Parameters.Add(new SqlParameter("@Flags", SqlDbType.Int));
            command.Parameters.Add(new SqlParameter("@DisplayName", SqlDbType.NVarChar, 0xff));
            command.Parameters.Add(new SqlParameter("@ContentTypeId", SqlDbType.VarBinary, 0xff));
            command.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int));
            command.Parameters.Add(new SqlParameter("@PartOrder", SqlDbType.Int));
            command.Parameters.Add(new SqlParameter("@ZoneId", SqlDbType.NVarChar, 0x40));
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int));
            command.Parameters.Add(new SqlParameter("@IsIncluded", SqlDbType.Bit));
            command.Parameters.Add(new SqlParameter("@FrameState", SqlDbType.TinyInt));
            command.Parameters.Add(new SqlParameter("@WebPartTypeId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@Assembly", SqlDbType.NVarChar, -1));
            command.Parameters.Add(new SqlParameter("@Class", SqlDbType.NVarChar, -1));
            command.Parameters.Add(new SqlParameter("@SolutionId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@SolutionWebId", SqlDbType.UniqueIdentifier));
            command.Parameters.Add(new SqlParameter("@AllUsersProperties", SqlDbType.VarBinary, -1));
            command.Parameters.Add(new SqlParameter("@PerUserProperties", SqlDbType.VarBinary, -1));
            command.Parameters.Add(new SqlParameter("@WebPartIdProperty", SqlDbType.NVarChar, -1));
            command.Parameters.Add(new SqlParameter("@Cache", SqlDbType.VarBinary, -1));
            command.Parameters.Add(new SqlParameter("@Source", SqlDbType.NVarChar, -1));
            command.Parameters.Add(new SqlParameter("@Level", SqlDbType.TinyInt));
            command.Parameters.Add(new SqlParameter("@BaseViewId", SqlDbType.TinyInt));
            command.Parameters.Add(new SqlParameter("@bDeleteUsersOtherWebParts", SqlDbType.Bit));
            command.Parameters.Add(new SqlParameter("@bRetainObjectIdentity", SqlDbType.Bit));
            command.Parameters.Add(new SqlParameter("@View", SqlDbType.VarBinary, -1));
            return command;
        }


    }
}
