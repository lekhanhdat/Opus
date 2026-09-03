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
using System.Xml;
using AvePoint.Common;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        /// <summary>
        /// 根据ListId获取List Title
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        [Obsolete("Please use the new method which includes two parameters.")]
        public string GetListTitle(Guid listId)
        {
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"SELECT tp_Title FROM AllLists WITH(NOLOCK) WHERE tp_Id=@ListId";
            return (string)mQueryWorker.ExecuteScalar(cmdText);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public string GetListTitle(Guid siteId, Guid listId)
        {
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"SELECT tp_Title FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_Id=@ListId";
            return (string)mQueryWorker.ExecuteScalar(cmdText);
        }

        /// <summary>
        /// 获取List下的所有Fields
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "Add another method to improve the performance.")]
        [Obsolete("We should use another method which includes the site id in the parameters for SP2013")]
        public string GetFields(Guid webId, Guid listId)
        {
            string fieldsSchema = null;

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"select tp_Fields from AllLists WITH(NOLOCK) where tp_WebId=@WebId and tp_ID=@ListId";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] bytes = dr["tp_Fields"] as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            fieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                        }
                        break;
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
            if (fieldsSchema != null && fieldsSchema.Contains("<"))
            {
                fieldsSchema = fieldsSchema.Substring(fieldsSchema.IndexOf("<", StringComparison.OrdinalIgnoreCase));
            }

            return "<Fields>" + fieldsSchema + "</Fields>";
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public string GetFields(Guid siteId, Guid webId, Guid listId)
        {
            string fieldsSchema = null;

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"select tp_Fields from AllLists WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_WebId=@WebId and tp_ID=@ListId";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] bytes = dr["tp_Fields"] as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            fieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                        }
                        break;
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
            if (fieldsSchema != null && fieldsSchema.Contains("<"))
            {
                fieldsSchema = fieldsSchema.Substring(fieldsSchema.IndexOf("<", StringComparison.OrdinalIgnoreCase));
            }

            return "<Fields>" + fieldsSchema + "</Fields>";
        }
        /// <summary>
        /// 获取ListSetting 信息,API有局限性，只可以获取部分ListSetting信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="listSettingInfo"></param>
        /// <returns></returns>
        public void GetListSettingInfoByNative(Guid siteId, Guid webId, Guid listId, AveListSettingInfo listSettingInfo)
        {

            try
            {
                GetListSettingsByNativeInternal(siteId, webId, listId, listSettingInfo);
                GetListRootFolderSettings(listSettingInfo, siteId);
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

        private void GetListSettingsByNativeInternal(Guid siteId, Guid webId, Guid listId, AveListSettingInfo listSettingInfo)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@SiteId", siteId);
            const string cmdText = @"
        SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
               tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
               tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, 
               tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
               tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
               tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
               tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
               tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID,tp_RootFolder
        FROM AllLists WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId and tp_WebId=@WebId and tp_Id=@ListId";

            AveQueryUtility.GetDBRow(listSettingInfo, mQueryWorker, cmdText, "tp_");
        }

        private void GetListRootFolderSettings(AveListSettingInfo listSettingInfo, Guid siteId)
        {
            var rootFolderId = listSettingInfo.RootFolder.Value;
            listSettingInfo.RootFolderInfo = new AveListRootFolderInfo();

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", rootFolderId);

            const string cmdText = @"SELECT CharSet,TimeCreated,TimeLastModified,MetaInfo,Dirty,DocFlags,WelcomePageUrl 
                    FROM AllDocs WITH (NOLOCK, INDEX=Docs_IdLevelUnique) WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId = 0x";

            AveQueryUtility.GetDBRow(listSettingInfo.RootFolderInfo.Value, mQueryWorker, cmdText);
        }

        /// <summary>
        /// 根据ListTile获取List的Id
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use the other method to improve the performance")]
        [Obsolete("Please use the other method which includes siteId in the parameters.")]
        public Guid GetListId(Guid webId, string listTitle)
        {
            Guid id = Guid.Empty;
            if (String.IsNullOrEmpty(listTitle))
            {
                return id;
            }
            string text = "SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@Title", listTitle);
                object result = mQueryWorker.ExecuteScalar(text);
                if (result != null && result != DBNull.Value)
                {
                    return (Guid)result;
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
            return id;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetListId(Guid siteId, Guid webId, string listTitle)
        {
            Guid id = Guid.Empty;
            if (String.IsNullOrEmpty(listTitle))
            {
                return id;
            }
            string text = "SELECT tp_Id FROM AllLists WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_WebId=@WebId AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@Title", listTitle);
                object result = mQueryWorker.ExecuteScalar(text);
                if (result != null && result != DBNull.Value)
                {
                    return (Guid)result;
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
            return id;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateListModifiedTime(Guid siteId, Guid listId, DateTime lastModified)
        {
            string cmdText = "UPDATE AllListsAux SET Modified=@Modified WHERE SiteId=@SiteId AND ListID=@ListID";
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListID", listId);
            mQueryWorker.AddParameter("@Modified", lastModified);
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        /// <summary>
        /// 获取List下的Fields的SchemaXML
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/02", "Kexin Guo")]
        public string GetFieldsSchemaXML(Guid webId, Guid listId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            string commandText = "Select tp_Fields From AllLists WITH(NOLOCK) Where tp_WebId=@WebId AND tp_Id=@ListId";
            return (string)mQueryWorker.ExecuteScalar(commandText);
        }

        /// <summary>
        /// 判断List是否删除
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han", false, "We should use site id for AllLists table")]
        [Obsolete("Please use another method which contains siteid in the parameters.")]
        public bool IsConflictWithRecycle(string name, Guid webId)
        {
            const string cmdText = @"SELECT tp_WebId from AllLists WITH(NOLOCK)
                                     where tp_WebId = @WebId and tp_Title=@Title and tp_DeleteTransactionId <> 0x";
            bool isConflict = false;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@Title", name);

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.HasRows)
                    {
                        isConflict = true;
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

            return isConflict;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public bool IsConflictWithRecycle(string name, Guid siteId, Guid webId)
        {
            const string cmdText = @"SELECT tp_WebId from AllLists WITH(NOLOCK)
                                     where tp_SiteId=@SiteId and tp_WebId = @WebId and tp_Title=@Title and tp_DeleteTransactionId <> 0x";
            bool isConflict = false;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@Title", name);

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.HasRows)
                    {
                        isConflict = true;
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

            return isConflict;
        }

        /// <summary>
        /// Use Clustered Index: [tp_WebId],[tp_ID] 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="listColumns"></param>
        [QueryReview("2014/03/03", "Cheng Cui")]
        public void UpdateListInfoByNative(Guid siteId, Guid webId, Guid listId, Dictionary<string, object> listColumns)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);

            var stringBuilder = new StringBuilder();
            foreach (var listColumn in listColumns)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append(",");
                }
                stringBuilder.Append(listColumn.Key);
                stringBuilder.Append("=@");
                stringBuilder.Append(listColumn.Key);

                mQueryWorker.AddParameter("@" + listColumn.Key, listColumn.Value);
            }

            string cmdText = string.Format("UPDATE AllLists SET {0} WHERE tp_SiteId=@SiteId AND tp_WebId=@WebId AND tp_ID=@ListId", stringBuilder);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        /// <summary>
        /// 获取List信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han", true, "Add SiteId to improve the performance")]
        public AveListInfo GetListInfo(IAveList list)
        {
            AveListInfo listInfo = new AveListInfo();
            if (list == null)//when {System Folder}, the list is null
            {
                listInfo.Title = AveConstants.SYSTEM_FOLDER;
                return listInfo;
            }
            try
            {
                IAveWeb ParentWeb = list.ParentWeb;

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ListId", list.ID);
                mQueryWorker.AddParameter("@WebId", ParentWeb.ID);
                mQueryWorker.AddParameter("@SiteId", ParentWeb.Site.ID);
                string cmdText = @"SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
                                                   tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
                                                   tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, 
                                                   tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
                                                   tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
                                                   tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
                                                   tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
                                                   tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID
                                            FROM AllLists WITH(NOLOCK)
                                            WHERE tp_SiteId=@SiteId AND tp_WebId=@WebId AND tp_Id=@ListId";

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        ulong flags = (ulong)dr.GetInt64(14);
                        //AllLists.tp_ServerTemplate
                        listInfo.BaseTemplate = dr.GetInt32(7);
                        listInfo.BaseType = dr.GetInt32(5);
                        //listInfo.BaseTemplate = (int)list.BaseTemplate;
                        //AllLists.tp_FeatureId                
                        listInfo.TemplateFeatureId = dr.IsDBNull(6) ? Guid.Empty : dr.GetGuid(6);
                        //listInfo.TemplateFeatureId = list.TemplateFeatureId;
                        //AllLists.tp_Title
                        listInfo.Title = dr.IsDBNull(0) ? string.Empty : dr.GetString(0);
                        //AllLists.tp_Description
                        listInfo.Description = dr.IsDBNull(18) ? string.Empty : dr.GetString(18);
                        //AllLists.tp_ID
                        listInfo.Id = dr.GetGuid(39);
                        string url = list.RootFolder.ServerRelativeUrl.Substring(ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
                        listInfo.Url = ParentWeb.Url.TrimEnd('/') + "/" + url;
                        listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                        if (list.BaseTemplate == AveListTemplateType.ExternalList)
                        {
                            if (list.HasExternalDataSource)
                            {
                                listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(list.DataSource, list.DataSource.GetType(), "ToXml", new object[] { });
                            }
                        }
                        if (ParentWeb.IsRootWeb)
                        {
                            listInfo.RootWebOnly = Ave2010ListFlags.RootWebOnly(flags);
                        }
                        else
                        {
                            listInfo.RootWebOnly = false;
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
            return listInfo;

        }

        public void ReplaceCustomActionId(Guid siteId, Guid webId, string scopeId, Guid oldId, Guid newId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ScopeId", scopeId);
                mQueryWorker.AddParameter("@OldId", oldId);
                mQueryWorker.AddParameter("@NewId", newId);
                var cmdtxt = @"UPDATE CustomActions set Id = @NewId WHERE SiteId=@SiteId AND WebId=@WebId AND ScopeID=@ScopeId AND ID=@OldId";
                mQueryWorker.ExecuteNonQuery(cmdtxt);
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
