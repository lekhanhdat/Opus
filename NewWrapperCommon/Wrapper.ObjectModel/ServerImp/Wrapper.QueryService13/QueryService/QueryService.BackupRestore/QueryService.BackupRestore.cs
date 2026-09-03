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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.QueryService;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService : IAveBackupRestoreQueryService13
    {
        #region Private Methods

        [QueryReview("2012/05/09", "Fengfu Zhang")]
        private string GetFileFilter(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(filter);
                foreach (XmlNode node in doc.GetElementsByTagName("property"))
                {
                    string value = node.Attributes["value"].Value;
                    string name = node.Attributes["name"].Value;
                    if (name.Equals("filterPath", StringComparison.OrdinalIgnoreCase))
                    {
                        return value.Trim('/');
                    }
                }
            }
            return "";
        }

        [QueryReview("2012/05/09", "Fengfu Zhang")]
        private bool CheckExistingRecord(string selectCmdText, string updateCmdText, int version, bool overWrite, bool isUserData, ref bool needInsert)
        {
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(selectCmdText))
                {
                    if (!sr.HasRows)
                    {
                        needInsert = true;
                        return true;
                    }
                    // Here we should check whether table is ALLUserData, if yes, we have to update record(tp_DeletedTractionId!=0x) for deleted items in destination.
                    if (!isUserData)
                    {
                        if (!overWrite)
                        {
                            return false;
                        }
                    }
                    sr.Read();
                    byte[] transactionId = sr.GetSqlBinary(0).Value;
                    if (transactionId.Length == 0)  //  DeleteTransactionId = 0x
                    {
                        return true;
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                mQueryWorker.ExecuteNonQuery(updateCmdText, VersionOption.OneItemOrVersion, isUserData ? RowOrdinalOption.AllUserDataAllRows : RowOrdinalOption.None);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.UpdateDeleteDocError, ex);
            }
            return true;
        }

        #endregion

        public void SetIsolationLevel(IsolationLevel level)
        {
            mQueryWorker.SetIsolateLevel(level);
        }

        #region Backup

        /// <summary>
        /// 获取SiteSetting信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>

        [QueryReview("2012/12/13", "Austin Han")]
        public AveSiteSettingInfo GetSiteSettingFromSites(IAveSite site)
        {
            return mQuerySessionSchema.GetSiteSettingFromSites(site);
        }

        /// <summary>
        /// 查询SiteCollection的大小
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public long GetSiteSizeFromSites(IAveSite site)
        {
            return mQuerySessionSchema.GetSiteSizeFromSites(site);
        }

        /// <summary>
        /// 获取SiteSetting的详细信息(包括SolutionIdCollection，MetaInfo等)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han", true, "Added SiteId to improve the performance ")]
        public AveSiteSettingInfo GetSiteSettingAndMetaInfo(IAveSite site)
        {
            return mQuerySessionSchema.GetFullSiteSetting(site);
        }

        public string GetPageUrlById(Guid siteId, Guid pageId)
        {
            return mQuerySessionSchema.GetPageUrlById(siteId, pageId);
        }

        public string GetWebFullUrlById(Guid siteId, Guid webId)
        {
            return mQuerySessionSchema.GetWebFullUrlById(siteId, webId);
        }

        public void GetSubWebsUrl(Guid siteId, Guid parentWebId, Dictionary<string, Dictionary<Guid, string>> infos)
        {
            mQuerySessionSchema.GetSubWebsUrl(siteId, parentWebId, infos);
        }
        public void GetListPagesUrl(Guid siteId, Guid listId, Dictionary<string, Dictionary<Guid, string>> infos)
        {
            mQuerySessionSchema.GetListPagesUrl(siteId, listId, infos);
        }

        /// <summary>
        /// 获取Web Setting 信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public AveWebSettingInfo GetWebSettingFromWebs(IAveWeb web)
        {
            return mQuerySessionSchema.GetWebSettingFromWebs(web);
        }

        /// <summary>
        /// 获取Web的Size
        /// 无API实现
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han", false, "This function won't work in SP2013 because there is no ComMd table in SP2013")]
        public Dictionary<Guid, long> GetAllWebSize(IAveSite site)
        {
            return mQuerySessionSchema.GetAllWebSize(site);
        }



        /// <summary>
        /// look up for DoclibRowId in discussion board list by ThreadIndex
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="threadIndex"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex)
        {
            int parentId = 0;
            string cmdText = @"select tp_ID from AllUserData WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1)  AND tp_ListId=@ListId AND tp_ThreadIndex =@ThreadIndex";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ThreadIndex", threadIndex);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        parentId = dr.GetInt32(0);
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
            return parentId;
        }

        /// <summary>
        /// 获取Item/Document的ModifiedBy
        /// user被删除时，无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetFileModifiedByIdByNative(Guid siteId, Guid parentId, Guid docId)
        {
            string cmdText = "SELECT tp_Editor FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_parentid=@ParentId AND tp_DocId=@DocId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@DocId", docId);
                return (int)mQueryWorker.ExecuteScalar(cmdText);
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



        public List<Dictionary<string, object>> GetVersionAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count)
        {
            return GetVersionAndUserInfo(siteId, parentId, currentDocLibRowId, count, string.Empty);
        }

        /// <summary>
        /// 查询Document的一条完整记录
        /// API只能获取部分信息
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="dataCache"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void GetDocInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetDocInfo"))
            {

                string cmdText =
    @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
                 DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
                 NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
                 SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
                 LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
                 UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
                 WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
                 DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
                 ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
                 ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
                 ListSchemaVersion,ClientId,InternalVersion,BumpVersion,StreamSchema
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
                if (itemInfo.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                else
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                cmdText += " Id=@Id AND UIVersion=@Version";

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
                mQueryWorker.AddParameter("@ParentID", itemInfo.ParentId);
                mQueryWorker.AddParameter("@Id", itemInfo.GUID);
                mQueryWorker.AddParameter("@Version", itemInfo.Version);
                AveQueryUtility.TryGetDBRow(dataCache, mQueryWorker, cmdText);

            }

        }

        public List<Dictionary<string, object>> GetDocAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count)
        {
            return GetDocAndUserInfo(siteId, parentId, currentDocLibRowId, count, string.Empty);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("BulkQuery-001")]
        public List<Dictionary<string, object>> GetDocAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count, string colNameCollection)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetDocAndUserInfo"))
            {

                var result = new List<Dictionary<string, object>>(count);
                colNameCollection = ProcessColNameCollection(colNameCollection);
                #region Assembly Command Text
                string cmdText = @"SELECT top {0}
doc.Id as DOC#Id,doc.DirName as DOC#DirName,doc.LeafName as DOC#LeafName,doc.DoclibRowId as DOC#DoclibRowId,doc.Type as DOC#Type,doc.SortBehavior as DOC#SortBehavior,doc.Size as DOC#Size,doc.UIVersion as DOC#UIVersion,doc.Dirty as DOC#Dirty,doc.ListDataDirty as DOC#ListDataDirty,doc.DocFlags as DOC#DocFlags,doc.ThicketFlag as DOC#ThicketFlag,doc.CharSet as DOC#CharSet,doc.ProgId as DOC#ProgId,doc.TimeCreated as DOC#TimeCreated,doc.TimeLastModified as DOC#TimeLastModified,doc.NextToLastTimeModified as DOC#NextToLastTimeModified,doc.MetaInfoTimeLastModified as DOC#MetaInfoTimeLastModified,doc.TimeLastWritten as DOC#TimeLastWritten,doc.SetupPathVersion as DOC#SetupPathVersion,doc.SetupPath as DOC#SetupPath,doc.SetupPathUser as DOC#SetupPathUser,doc.CheckoutUserId as DOC#CheckoutUserId,doc.CheckoutDate as DOC#CheckoutDate,doc.CheckoutExpires as DOC#CheckoutExpires,doc.VersionCreatedSinceSTCheckout as DOC#VersionCreatedSinceSTCheckout,doc.LTCheckoutUserId as DOC#LTCheckoutUserId,doc.VirusVendorID as DOC#VirusVendorID,doc.VirusStatus as DOC#VirusStatus,doc.VirusInfo as DOC#VirusInfo,doc.MetaInfo as DOC#MetaInfo,doc.MetaInfoSize as DOC#MetaInfoSize,doc.MetaInfoVersion as DOC#MetaInfoVersion,doc.UnVersionedMetaInfo as DOC#UnVersionedMetaInfo,doc.UnVersionedMetaInfoSize as DOC#UnVersionedMetaInfoSize,doc.UnVersionedMetaInfoVersion as DOC#UnVersionedMetaInfoVersion,doc.WelcomePageUrl as DOC#WelcomePageUrl,doc.WelcomePageParameters as DOC#WelcomePageParameters,doc.IsCurrentVersion as DOC#IsCurrentVersion,doc.Level as DOC#Level,doc.CheckinComment as DOC#CheckinComment,doc.AuditFlags as DOC#AuditFlags,doc.InheritAuditFlags as DOC#InheritAuditFlags,doc.DraftOwnerId as DOC#DraftOwnerId,doc.UIVersionString as DOC#UIVersionString,doc.ParentId as DOC#ParentId,doc.HasStream as DOC#HasStream,doc.ScopeId as DOC#ScopeId,doc.BuildDependencySet as DOC#BuildDependencySet,doc.ParentVersion as DOC#ParentVersion,doc.ParentVersionString as DOC#ParentVersionString,doc.TransformerId as DOC#TransformerId,doc.ParentLeafName as DOC#ParentLeafName,doc.IsCheckoutToLocal as DOC#IsCheckoutToLocal,doc.CtoOffset as DOC#CtoOffset,doc.Extension as DOC#Extension,doc.ExtensionForFile as DOC#ExtensionForFile,doc.ItemChildCount as DOC#ItemChildCount,doc.FolderChildCount as DOC#FolderChildCount,doc.FileFormatMetaInfo as DOC#FileFormatMetaInfo,doc.FileFormatMetaInfoSize as DOC#FileFormatMetaInfoSize,doc.ListSchemaVersion as DOC#ListSchemaVersion,doc.ClientId as DOC#ClientId,doc.InternalVersion as DOC#InternalVersion,doc.BumpVersion as DOC#BumpVersion,
data.tp_ID as UD#tp_ID,data.tp_RowOrdinal as UD#tp_RowOrdinal,data.tp_Version as UD#tp_Version,data.tp_UIVersionString as UD#tp_UIVersionString,data.tp_Author as UD#tp_Author,data.tp_Editor as UD#tp_Editor,data.tp_Modified as UD#tp_Modified,data.tp_Created as UD#tp_Created,data.tp_Ordering as UD#tp_Ordering,data.tp_ThreadIndex as UD#tp_ThreadIndex,data.tp_HasAttachment as UD#tp_HasAttachment,data.tp_ModerationStatus as UD#tp_ModerationStatus,data.tp_IsCurrent as UD#tp_IsCurrent,data.tp_ItemOrder as UD#tp_ItemOrder,data.tp_InstanceID as UD#tp_InstanceID,data.tp_GUID as UD#tp_GUID,data.tp_CopySource as UD#tp_CopySource,data.tp_HasCopyDestinations as UD#tp_HasCopyDestinations,data.tp_AuditFlags as UD#tp_AuditFlags,data.tp_InheritAuditFlags as UD#tp_InheritAuditFlags,data.tp_Size as UD#tp_Size,data.tp_WorkflowVersion as UD#tp_WorkflowVersion,data.tp_WorkflowInstanceID as UD#tp_WorkflowInstanceID," + colNameCollection + @"data.tp_ContentTypeId as UD#tp_ContentTypeId
FROM AllDocs as doc WITH(NOLOCK) LEFT JOIN AllUserData as data WITH(NOLOCK) 
ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND (data.tp_IsCurrentVersion=0 OR data.tp_IsCurrentVersion=1) AND data.tp_ParentId=doc.ParentId AND data.tp_DocId = doc.Id AND data.tp_UIVersion = doc.UIVersion
WHERE doc.SiteId = @SiteId AND doc.ParentId = @ParentId AND doc.DeleteTransactionId = 0x AND
doc.Type = 0 AND doc.DoclibRowId >= @CurrentDoclibRowId
ORDER BY doc.DoclibRowId";

                cmdText = string.Format(cmdText, count);
                #endregion

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentID", parentId);
                mQueryWorker.AddParameter("@CurrentDoclibRowId", currentDocLibRowId);
                try
                {
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {

                        using (AvePerformanceScope scope1 = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetDocAndUserInfo.ReadData"))
                        {

                            while (dr.Read())
                            {
                                Dictionary<string, object> tempData = new Dictionary<string, object>();
                                AveQueryUtility.GetDBRow(tempData, dr);
                                result.Add(tempData);
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
                return result;

            }

        }

        private string ProcessColNameCollection(string ColNameCollection)
        {
            StringBuilder result = new StringBuilder();
            string[] items = ColNameCollection.Split(',');
            foreach (string item in items)
            {
                if (!item.Equals("", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Append("data." + item + " as UD#" + item + ",");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取document的InternalVersion
        /// 无API实现
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <returns></returns>
        [QueryReview("2012/05/09", "Fengfu Zhang", true, "add Warning when ParentId == Guid.Empty for AllDocs")]
        [Obsolete("There is no internal version in SharePoint 2013")]
        public int GetInternalVersion(AveBaseItemInfo itemInfo)
        {
            string cmdText = @"SELECT InternalVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
            if (itemInfo.ParentId != Guid.Empty)
            {
                cmdText += " ParentID=@ParentID AND ";
            }
            else
            {
                if (itemInfo.ItemType != AveItemType.Attachement)
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
            }
            cmdText += " Id=@Id AND UIVersion=@UIVersion ";

            cmdText += @" UNION SELECT InternalVersion FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND ID=@ID AND UIVersion=@UIVersion";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", itemInfo.GUID);
                mQueryWorker.AddParameter("@UIVersion", itemInfo.Version);
                mQueryWorker.AddParameter("@ParentID", itemInfo.ParentId);
                mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
                object result = mQueryWorker.ExecuteScalar(cmdText);
                if (result is int)
                {
                    return (int)result;
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }


        /// <summary>
        /// look up for DoclibRowId in discussion board list by ThreadIndex
        /// 无API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="threadIndex"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use SiteId for SP2013")]
        [Obsolete("We should use the other method which includes siteId in the parameters to improve the performance.")]
        public int GetThreadIndexParentId(Guid listId, byte[] threadIndex)
        {
            string cmdText = @"select tp_ID from AllUserData WITH(NOLOCK) where tp_ListId=@ListId and (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) and (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) and tp_ThreadIndex =@ThreadIndex ";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ThreadIndex", threadIndex);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        return dr.GetInt32(0);
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

        [QueryReview("2012/12/17", "Austin Han")]
        public int GetThreadIndexParentId(Guid siteId, Guid listId, byte[] threadIndex)
        {
            string cmdText = @"select tp_ID from AllUserData WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_ListId=@ListId and (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) and (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) and tp_ThreadIndex =@ThreadIndex ";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ThreadIndex", threadIndex);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        return dr.GetInt32(0);
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
        /// 获取Site中lcid下的所有webTemplates
        /// </summary>
        /// <param name="site"></param>
        /// <param name="lcid"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public Dictionary<Guid, string> GetALLWebTemplates(IAveSite site, uint lcid)
        {
            return mQuerySessionSchema.GetALLWebTemplates(site, lcid);
        }

        /// <summary>
        /// 获取Document的checkoutUserId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetCheckOutUserId(AveBaseItemInfo info)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", info.GUID);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@Version", info.Version);
            mQueryWorker.AddParameter("@ParentID", info.ParentId);

            string cmdText = string.Empty;
            cmdText = @"SELECT CheckoutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
            if (info.ParentId != Guid.Empty)
            {
                cmdText += @" ParentID=@ParentID AND ";
            }
            else
            {
                logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
            }
            cmdText += @" Id=@Id AND UIVersion=@Version";

            object result = mQueryWorker.ExecuteScalar(cmdText);
            if (result != null && result is int)
            {
                return (int)result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 更新Item的信息（editor，Author，Created等)
        /// 无API实现
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="author"></param>
        /// <param name="modified"></param>
        /// <param name="created"></param>
        /// <param name="info"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-001")]
        public void UpdateSpecialPropertyByNative(string editor, string author, DateTime modified, DateTime created, AveBaseItemInfo info)
        {
            try
            {
                string cmdStr = string.Empty;
                mQueryWorker.ClearParameters();
                if (info.UserData != null && info.UserData.ContainsKey("#tp_AppEditor") && info.UserData.ContainsKey("#tp_AppAuthor"))
                {
                    cmdStr = @"UPDATE AllUserData  SET tp_AppEditor=@AppEditor, tp_AppAuthor=@APPAuthor,tp_Editor=@Editor, tp_Author=@Author,tp_Created=@Created,
                                   tp_Modified=@Modified WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                            AND tp_Level=@Level AND tp_UIVersion=@Version";

                    mQueryWorker.AddParameter("@AppEditor", info.UserData["#tp_AppEditor"].ToString());
                    mQueryWorker.AddParameter("@AppAuthor", info.UserData["#tp_AppEditor"].ToString());
                }
                else
                {
                    cmdStr = @"UPDATE AllUserData SET tp_Editor=@Editor, tp_Author=@Author,tp_Created=@Created,
                                       tp_Modified=@Modified WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                                AND tp_Level=@Level AND tp_UIVersion=@Version";

                }

                mQueryWorker.AddParameter("@Editor", editor);
                mQueryWorker.AddParameter("@Author", author);
                mQueryWorker.AddParameter("@Created", created);
                mQueryWorker.AddParameter("@Modified", modified);

                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ID", info.GUID);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@Version", info.OriginalVersion);

                mQueryWorker.ExecuteNonQuery(cmdStr, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while updating an item SpecialProperty. Url:{0}, Id:{1}, Reason:{2}.", info.ServerRelativeUrl, info.GUID, e);
            }
        }

        /// <summary>
        /// 更新Item的Modified by和Created By字段
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="modifiedBy"></param>
        /// <param name="createdBy"></param>
        /// <param name="colNameModified"></param>
        /// <param name="colNameCreated"></param>
        /// <param name="info"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-002")]
        //        public void UpdateModifiedBy(string modifiedBy, string createdBy, string colNameModified, string colNameCreated, AveBaseItemInfo info)
        //        {
        //            try
        //            {
        //                string cmdStr = string.Empty;
        //                cmdStr = @"UPDATE AllUserData SET " + colNameModified + @" = @ModifiedBy," + colNameCreated + @" =@CreatedBy
        //                                         WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_ParentId=@ParentId AND tp_DocID=@ID 
        //                                 AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level AND tp_RowOrdinal=0";

        //                mQueryWorker.ClearParameters();

        //                mQueryWorker.AddParameter("@SiteId", info.SiteId);
        //                mQueryWorker.AddParameter("@ID", info.GUID);
        //                mQueryWorker.AddParameter("@ParentId", info.ParentId);
        //                mQueryWorker.AddParameter("@IsCurrentVersion", true);
        //                mQueryWorker.AddParameter("@CalculatedVersion", 0);
        //                mQueryWorker.AddParameter("@Level", info.Level);
        //                mQueryWorker.AddParameter("@ModifiedBy", modifiedBy);
        //                mQueryWorker.AddParameter("@CreatedBy", createdBy);

        //                mQueryWorker.ExecuteNonQuery(cmdStr, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataOneRow);
        //            }
        //            catch (SqlException queryException)
        //            {
        //                throw new AveQueryException(queryException);
        //            }
        //            catch (AveQueryException)
        //            {
        //                throw;
        //            }
        //            catch (Exception e)
        //            {
        //                throw new AveQueryException(e.Message, e);
        //            }
        //        }

        /// <summary>
        /// 根据info信息(leafName，parentId，siteid）获取Folder的unique Id
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetFolderIdByName(AveBaseItemInfo info)
        {
            Guid id = Guid.Empty;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@LeafName", info.Name);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            string cmdText = @"SELECT ID FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x AND type=1";

            object idObject = mQueryWorker.ExecuteScalar(cmdText);
            if (idObject != null && idObject != DBNull.Value)
            {
                id = (Guid)idObject;
            }
            return id;

        }

        /// <summary>
        /// 获取List Folder下不可见的文件
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<AveHiddenFileInfo> GetHiddenFiles(Guid siteId, Guid webId, Guid listId, Guid folderId)
        {
            List<AveHiddenFileInfo> hiddenFiles = new List<AveHiddenFileInfo>();
            StringBuilder commandText = new StringBuilder("SELECT Id, LeafName, UIVersion, DocFlags, Level , TimeLastModified ,HasStream , Size  FROM AllDocs WITH(NOLOCK) WHERE SiteId = @SiteId AND DeleteTransactionId=0x AND ParentId=@FolderId AND WebId = @WebId ");
            if (listId != Guid.Empty)
            {
                commandText.Append("AND ListId = @ListId ");
            }
            commandText.Append("AND Type = 0 AND DocLibRowId IS NULL AND IsCurrentVersion = 1 ORDER BY LeafName, UIVersion");
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            if (listId != Guid.Empty)
            {
                mQueryWorker.AddParameter("@ListId", listId);
            }
            mQueryWorker.AddParameter("@FolderId", folderId);
            try
            {
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(commandText.ToString()))
                {
                    while (reader.Read())
                    {

                        AveHiddenFileInfo fileInfo = new AveHiddenFileInfo();
                        fileInfo.ID = reader[0].ToString();
                        fileInfo.Name = reader[1].ToString();
                        fileInfo.Version = reader.GetInt32(2);
                        fileInfo.DocFlags = reader.GetInt32(3);
                        fileInfo.Level = reader.GetByte(4);
                        fileInfo.TimeLastModified = reader.GetDateTime(5);
                        fileInfo.HasStream = reader.GetInt32(6) == 1 ? true : false;
                        fileInfo.Size = reader.GetInt32(7);
                        hiddenFiles.Add(fileInfo);
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

            return hiddenFiles;
        }

        /// <summary>
        /// 根据Item的DocLibRowId获取Item的Guid
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We shoudl use another method to improve the performance.")]
        [Obsolete("Please use another method which includes site id in the parameters.")]
        public Guid GetListItemGuid(Guid listId, int rowId)
        {
            Guid tpGUid = Guid.Empty;
            string cmdText = @"SELECT tp_GUID from AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 
                                                AND tp_ID=@RowId AND tp_RowOrdinal=0";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@RowId", rowId);
                object result = mQueryWorker.ExecuteScalar(cmdText);
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
            return tpGUid;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetListItemGuid(Guid siteId, Guid listId, int rowId)
        {
            Guid tpGUid = Guid.Empty;
            string cmdText = @"SELECT tp_GUID from AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 
                                                AND tp_ID=@RowId AND tp_RowOrdinal=0";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@RowId", rowId);
                object result = mQueryWorker.ExecuteScalar(cmdText);
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
            return tpGUid;
        }        

        /// <summary>
        /// 根据Item的DoclibRowId获取Item的tp_Guid
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="lookupListId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use siteId to improve the performance.")]
        [Obsolete("Please use another method which includes siteId in the parameters.")]
        public Guid GetLookupGUIDById(Guid lookupListId, int rowId)
        {
            string cmdText = @"SELECT tp_GUID from AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ID=@RowId AND tp_CalculatedVersion=0 
                                        AND (tp_Level=1 OR tp_Level=2 OR tp_Level=255) AND tp_RowOrdinal=0";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ListId", lookupListId);
                mQueryWorker.AddParameter("@RowId", rowId);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        Guid tp_GUID = dr.GetGuid(0);
                        return tp_GUID;
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
            return Guid.Empty;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetLookupGUIDById(Guid siteId, Guid lookupListId, int rowId)
        {
            string cmdText = @"SELECT tp_GUID from AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ID=@RowId AND tp_CalculatedVersion=0 
                                        AND (tp_Level=1 OR tp_Level=2 OR tp_Level=255) AND tp_RowOrdinal=0";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ListId", lookupListId);
                mQueryWorker.AddParameter("@RowId", rowId);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        Guid tp_GUID = dr.GetGuid(0);
                        return tp_GUID;
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
            return Guid.Empty;
        }

        /// <summary>
        /// 判断是否是Ave的Stub
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="Id"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We don't support EBS for now. So this is not useful method.")]
        public bool CheckContentIfAveStub(Guid siteId, Guid Id, int internalVersion)
        {
            bool isAveStub = false;
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.CheckContentIfAveStub"))
            {
                string cmdText = @"SELECT Content FROM DocStreams WITH(NOLOCK) WHERE SiteId=@SiteId AND DocId=@Id AND InternalVersion=@InternalVersion";
                try
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@Id", Id);
                    mQueryWorker.AddParameter("@InternalVersion", internalVersion);
                    object result = mQueryWorker.ExecuteScalar(cmdText);
                    if (result != null && result != DBNull.Value)
                    {
                        byte[] stub = (byte[])result;
                        if ((stub[0] == 'D') && (stub[1] == 'O') && (stub[2] == 'C'))
                        {
                            isAveStub = true;
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
            return isAveStub;
        }

        /// <summary>
        /// 获取Web的所有子Web的数量
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="serverRelativeUrl"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetSubWebCounts(Guid siteId, string serverRelativeUrl)
        {
            return mQuerySessionSchema.GetSubWebCounts(siteId, serverRelativeUrl);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public DateTime GetLastAccessedDayOfWeb(Guid siteId, Guid webId)
        {
            DateTime lastAccessedDay = DateTime.MinValue;
            string cmdText = @"
            SELECT TimeCreated, FullUrl, LastAccess = case 
                WHEN DayLastAccessed=0 then TimeCreated
                    else DATEADD(d, DayLastAccessed + 65536, '01/01/1899')
                end
            FROM Webs WITH(NOLOCK)
            WHERE Id=@WebId and SiteId=@SiteId";
            try
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetLastAccessedDayOfWeb"))
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@WebId", webId);
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText, CommandBehavior.SequentialAccess))
                    {
                        if (dr.Read())
                        {
                            lastAccessedDay = dr.GetDateTime(2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting last accessed day of web: {0}. siteId: {1}. Error{2}.", webId, siteId, ex.ToString());
            }
            return lastAccessedDay;
        }

        /// <summary>
        /// 获取Navigation节点的MetaInfo,效率考虑，有API实现
        /// </summary>
        /// <param name="web"></param>
        /// <param name="Eid"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the parameter of sql statement. ")]
        [QueryReview("2012/04/26", "Qianwen Hu")]
        public string GetNavigationNodeMetainfo(IAveWeb web, int Eid)
        {
            string metainfo = string.Empty;

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", web.Site.ID);
            mQueryWorker.AddParameter("@WebId", web.ID);
            mQueryWorker.AddParameter("@Eid", Eid);
            string cmdText = @"SELECT NodeMetainfo FROM NavNodes WITH(NOLOCK) WHERE SiteId=@SiteId AND WebId=@WebId AND Eid=@Eid";

            try
            {
                object nodeMetaInfo = mQueryWorker.ExecuteScalar(cmdText);
                if (nodeMetaInfo != null && nodeMetaInfo != DBNull.Value)
                {
                    metainfo = Encoding.UTF8.GetString((byte[])nodeMetaInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetNavigationNodeMetainfoError, ex);
            }

            return metainfo;
        }

        [QueryReview("2012/05/17", "Long Liang")]
        /// <summary>
        /// 获取Site/Web下的Features信息,效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public AveFeatureInfoBox GetFeatures(Guid siteId, Guid webId, AveFeatureScope scope)
        {
            AveFeatureInfoBox featureBox = new AveFeatureInfoBox();

            string cmdText = @"SELECT FeatureId FROM Features WITH(NOLOCK) WHERE SiteId=@SiteId and WebId=@WebId ORDER BY TimeActivated";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sdr.Read())
                    {
                        AveFeatureInfo info = new AveFeatureInfo();
                        info.Id = sdr.GetGuid(0);
                        info.Scope = scope;
                        featureBox.FeatureList.Add(info);
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
            return featureBox;
        }

        #region RBS Utility

        /// <summary>
        /// 备份RBS的Stub信息
        /// 无API实现
        /// </summary>
        /// <param name="rbs_id"></param>
        /// <param name="blobStoreId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/21", "Guoqin Sun")]
        public AveRBSStubInfo BackupRBSStub(byte[] rbs_id, short blobStoreId, int collectionId)
        {
            long blob_num = GenerateBlobNumber(rbs_id);
            if (0 == blob_num)
                throw new Exception("Get blob number error, check the sqlConnection of RBSSharedOpsBackup Object is available");
            AveRBSStubInfo stubInfo = null;
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

                    stubInfo = new AveRBSStubInfo(tem_blobId, tem_poolId, AveRBSCommon.RBS_PROVIDER_NAME_SP2013, dataLen);
                    stubInfo.RBSId = rbs_id;
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
            return stubInfo;
        }

        #endregion

        #region Workflow


        /// <summary>
        /// 备份Task下的Item
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupTasks(Guid siteId, Guid webId, Guid listId, Guid workflowInstanceId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", listId);
                mQueryWorker.AddParameter("@WorkflowInstanceId", workflowInstanceId);
                mQueryWorker.AddParameter("@nvarchar6", workflowInstanceId.ToString().Trim(new char[] { '{', '}' }));
                string commandText = @"DECLARE @parentId uniqueidentifier
                       SELECT @parentId=tp_RootFolder FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId AND tp_ID=@ListId
                       SELECT * FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) 
                               AND tp_ParentId=@parentId AND (tp_WorkflowInstanceId=@WorkflowInstanceId or nvarchar6=@nvarchar6) ORDER BY tp_Id,tp_Version";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
                //using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                //{
                //    while (sdr.Read())
                //    {
                //        //SPWorkflowSubItemUnit taskUnit = new SPWorkflowSubItemUnit(WorkflowSubItemType.Task, parentUnit);
                //        //Dictionary<string, string> fieldDic = mWebLevelFieldProcessor.GetDBFieldToSPFieldDic(mParentItem.Web.Lists[(Guid)parentUnit.Properties[CustomFieldProfix + "TaskListId"]]);
                //        Hashtable property = new Hashtable(StringComparer.OrdinalIgnoreCase);
                //        SetPropsFromDataReader(sdr, 0, fieldDic, 0, property);
                //        propertyList.Add(property);
                //    }
                //}
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
        /// 备份HistoryList下的Item
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="historyListId"></param>
        /// <param name="workflowInstanceId"></param>
        /// <param name="instanceIdColName"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Qianwen Hu", false, "未使用索引，可能存在效率问题，暂无改进方法，可考虑使用其他方式实现")]
        public IAveQueryDataReader BackupHistory(Guid siteId, Guid webId, Guid historyListId, Guid workflowInstanceId, string instanceIdColName)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@ListId", historyListId);
                mQueryWorker.AddParameter("@WorkflowInstanceId", workflowInstanceId);
                string commandText = @"DECLARE @parentId uniqueidentifier
                       SELECT @parentId=tp_RootFolder FROM AllLists WITH(NOLOCK) WHERE tp_WebId=@WebId AND tp_ID=@ListId
                       SELECT * FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) 
                               AND tp_ParentId=@parentId AND " + instanceIdColName + "=@WorkflowInstanceId ORDER BY tp_Id,tp_Version";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(commandText));
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

        #endregion

        #region Restore

        /// <summary>
        /// 当文件的Version很多的时候，使用该方法还原Version以提高效率[ADO-91211]
        /// </summary>
        /// <param name="orignalVersion"></param>
        /// <param name="siteId"></param>
        /// <param name="uniqueId"></param>
        /// <param name="version"></param>
        /// <param name="rowId"></param>
        /// <param name="parentFolderId"></param>
        [QueryReview("Item-003")]
        public void IncreaseVersionByNative(int originalVersion, Guid siteId, Guid uniqueId, int version, int rowId, Guid parentFolderId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@originalVersion", originalVersion);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@Id", uniqueId);
                mQueryWorker.AddParameter("@Version", version);
                mQueryWorker.AddParameter("@RowId", rowId);
                string commandText = string.Empty;
                if (parentFolderId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@ParentId", parentFolderId);

                    commandText = @"            
            Begin Tran
               Update AllDocs Set UIVersion=@originalVersion where SiteId=@SiteId And DeleteTransactionId=0x And Id=@Id And UIVersion=@Version And ParentId=@ParentId
               Update AllUserData Set tp_UIVersion=@originalVersion where tp_SiteId=@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion = 1 or tp_IsCurrentVersion = 0) And tp_Id=@RowId And tp_UIVersion=@Version And tp_ParentId=@ParentId And tp_DocId=@Id
            IF @@ERROR != 0
              Begin
                Rollback Tran
              End
            Else
              Begin
                Commit Tran
              End";
                }
                else
                {
                    commandText = @"            
            Begin Tran
               Update AllDocs Set UIVersion=@originalVersion where SiteId=@SiteId And DeleteTransactionId=0x And Id=@Id And UIVersion=@Version
               Update AllUserData Set tp_UIVersion=@originalVersion where tp_SiteId=@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion = 1 or tp_IsCurrentVersion = 0) And tp_Id=@RowId And tp_UIVersion=@Version
            IF @@ERROR != 0
              Begin
                Rollback Tran
              End
            Else
              Begin
                Commit Tran
              End";
                }
                mQueryWorker.ExecuteNonQuery(commandText);
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString(), "An error occurred while increasing version for large version.\noriginalVersion:{0}, SiteId:{1},Id:{2},Version:{3},RowId:{4},ParentId{5}",
                    originalVersion, siteId, uniqueId, version, rowId, parentFolderId);
            }
        }

        /// <summary>
        /// 根据Item的tp_GUID获取Item的DocLibRowId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="tp_guid"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use site id to improve performance in SP2013")]
        [Obsolete("Please use the other method which includes siteId in parameters.")]
        public int GetTpIdByTpGuid(Guid tp_guid, Guid listId)
        {
            int tp_id = 0;
            try
            {
                mQueryWorker.ClearParameters();
                string commadText = @"SELECT max(tp_ID) from AllUserData WITH(NOLOCK) 
                                                WHERE tp_ListId=@tp_listid 
                                                AND tp_GUID=@tp_guid;";

                mQueryWorker.AddParameter("@tp_listid", listId);
                mQueryWorker.AddParameter("@tp_guid", tp_guid);

                tp_id = (int)mQueryWorker.ExecuteScalar(commadText);
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("An error occurred when getting tp_id by tp_guid:{0}, listid:{1}, error:{2}.", tp_guid, listId, e.ToString()));
            }
            return tp_id;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public int GetTpIdByTpGuid(Guid siteId, Guid tp_guid, Guid listId)
        {
            int tp_id = 0;
            try
            {
                mQueryWorker.ClearParameters();
                string commadText = @"SELECT max(tp_ID) from AllUserData WITH(NOLOCK) 
                                                WHERE tp_SiteId=@SiteId AND tp_ListId=@Listid 
                                                AND tp_GUID=@Guid;";
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@Listid", listId);
                mQueryWorker.AddParameter("@Guid", tp_guid);

                tp_id = (int)mQueryWorker.ExecuteScalar(commadText);
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("An error occurred when getting tp_id by tp_guid:{0}, listid:{1}, error:{2}.", tp_guid, listId, e.ToString()));
            }
            return tp_id;
        }

        /// <summary>
        /// 更新Item下某个field值
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <param name="rowOrdinal"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ColValue is the parameter of the sql statement.")]
        [QueryReview("Item-004")]
        public void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {

            string cmdText = string.Empty;
            if (rowOrdinal >= 0)
                cmdText = @"update AllUserData set " + colName + "=@colValue where tp_SiteId=@siteId and tp_DeleteTransactionId=0x and tp_DocId=@docId and tp_UIVersion=@UIVersion and tp_RowOrdinal =@rowOrdinal";
            else
                cmdText = @"update AllUserData set " + colName + "=@colValue where tp_SiteId=@siteId and tp_DeleteTransactionId=0x and tp_DocId=@docId and tp_UIVersion=@UIVersion";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@siteId", siteId);
            mQueryWorker.AddParameter("@colValue", colValue);
            mQueryWorker.AddParameter("@docId", item.UniqueId);
            mQueryWorker.AddParameter("@UIVersion", version);
            if (rowOrdinal >= 0)
            {
                mQueryWorker.AddParameter("@rowOrdinal", rowOrdinal);
            }
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, rowOrdinal >= 0 ? RowOrdinalOption.AllUserDataOneRow : RowOrdinalOption.AllUserDataAllRows);
        }

        /// <summary>
        /// remove item by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="mQueryWorker"></param>
        /// <param name="spSite"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void RemoveListItemInRecycleBin(IAveSite site, Guid parentId, Guid tp_Guid)
        {
            try
            {
                IAveRecycleBinItemCollection recycleBin = site.RecycleBin;

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", site.ID);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@TP_GUID", tp_Guid);

                const string cmdText = @"SELECT Distinct tp_DeleteTransactionId FROM ALLUserData WITH(NOLOCK) 
                                                 WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId<>0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_GUID=@TP_GUID;";

                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            Guid itemid = new Guid((byte[])sr.GetValue(0));
                            Guid[] tempid = new Guid[1];
                            tempid[0] = itemid;
                            recycleBin.Delete(tempid);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while deleting a item in recycle bin. tp_Guid:{0}, Reason:{1}.", tp_Guid, e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting items in recycle bin. tp_Guid:{0}, Reason:{1}.", tp_Guid, e);
            }
        }

        /// <summary>
        /// 根据parentId和name删除RecycleBin中特定Item
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="site"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        [QueryReview("2012/12/17", "Austin Han", true, "Move the delete logic out of the SQL Reader.")]
        public void RemoveItemInRecycleBin(IAveSite site, Guid parentId, string name)
        {
            try
            {
                IAveRecycleBinItemCollection recycleBin = site.RecycleBin;

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", site.ID);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@LeafName", name);

                const string cmdText = @"SELECT Distinct DeleteTransactionId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName";

                List<Guid[]> ids = new List<Guid[]>();
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            Guid itemid = new Guid((byte[])sr.GetValue(0));
                            Guid[] tempid = new Guid[1];
                            tempid[0] = itemid;
                            ids.Add(tempid);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while deleting a item in recycle bin. Name:{0}, Reason:{1}.", name, e);
                        }
                    }
                }
                foreach (var tmpId in ids)
                {
                    recycleBin.Delete(tmpId);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting items in recycle bin. Name:{0}, Reason:{1}.", name, e);
            }
        }

        /// <summary>
        /// 修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="version"></param>
        [QueryReview("2012/12/17", "Austin Han", true, "Add siteId to use IdLevelUnique index.")]
        [QueryReview("Item-001")]
        public void UpdateAllDocsPropertyByNative(AveBaseItemInfo info, DateTime timeCreated, DateTime timeLastModified, int version)
        {
            try
            {
                if (timeCreated != DateTime.MinValue && timeLastModified != DateTime.MinValue)
                {
                    mQueryWorker.ClearParameters();
                    StringBuilder sCmd = new StringBuilder();
                    sCmd.Append(@"UPDATE AllDocs SET TimeCreated=@TimeCreated,TimeLastModified=@TimeLastModified,UnVersionedMetaInfo=@UnVersionedMetaInfo,UnVersionedMetaInfoVersion=@UnVersionedMetaInfoVersion,UnVersionedMetaInfoSize=@UnVersionedMetaInfoSize 
WHERE SiteId=@SiteId AND Id=@ID AND DeleteTransactionId=0x");
                    if (info.Level > 0)
                    {
                        sCmd.Append(" AND Level=@Level");
                        mQueryWorker.AddParameter("@Level", info.Level);
                    }
                    else
                    {
                        if (info.ParentId != Guid.Empty)
                        {
                            sCmd.Append(" AND ParentId=@ParentId");
                            mQueryWorker.AddParameter("@ParentId", info.ParentId);
                        }
                        sCmd.Append(" AND UIVersion=@UIVersion");
                        mQueryWorker.AddParameter("@UIVersion", info.Version);
                    }
                    mQueryWorker.AddParameter("@TimeCreated", timeCreated);
                    mQueryWorker.AddParameter("@TimeLastModified", timeLastModified);
                    mQueryWorker.AddParameter("@SiteId", info.SiteId);
                    mQueryWorker.AddParameter("@ID", info.GUID);

                    if (info.UnVersionedMetaInfo == null)
                    {
                        mQueryWorker.Command.Parameters.Add("@UnVersionedMetaInfo", SqlDbType.VarBinary, -1);
                        mQueryWorker.Command.Parameters["@UnVersionedMetaInfo"].Value = DBNull.Value;
                        //mQueryWorker.AddParameter("@UnVersionedMetaInfo", DBNull.Value);
                        mQueryWorker.AddParameter("@UnVersionedMetaInfoSize", DBNull.Value);
                        mQueryWorker.AddParameter("@UnVersionedMetaInfoVersion", DBNull.Value);
                    }
                    else
                    {
                        mQueryWorker.AddParameter("@UnVersionedMetaInfo", info.UnVersionedMetaInfo);
                        mQueryWorker.AddParameter("@UnVersionedMetaInfoSize", info.UnVersionedMetaInfo.LongLength);
                        mQueryWorker.AddParameter("@UnVersionedMetaInfoVersion", info.UnVersionedMetaInfoVersion);
                    }

                    mQueryWorker.ExecuteNonQuery(sCmd.ToString(), VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                }

            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while updating an item SpecialProperty. Url:{0}, Id:{1}, Reason:{2}.", info.ServerRelativeUrl, info.GUID, e);
            }
        }

        /// <summary>
        /// 修改webs中author字段。
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="version"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use siteId for AllWebs table")]
        [Obsolete("Please use another method which contains siteId in the parameters.")]
        public void UpdateWebsAuthorByNative(int userId, Guid webId)
        {
            try
            {
                this.mQuerySessionSchema.UpdateWebsAuthorByNative(userId, Guid.Empty, webId);
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update webs.Web Id:{0}, Reason:{1}.", webId, e);
            }
        }
        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateWebsAuthorByNative(int userId, Guid siteId, Guid webId)
        {
            try
            {
                this.mQuerySessionSchema.UpdateWebsAuthorByNative(userId, siteId, webId);
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update webs.Web Id:{0}, Reason:{1}.", webId, e);
            }
        }



        /// <summary>
        /// 修改List的NextAvailableId
        /// 无API实现
        /// </summary>
        /// <param name="toId"></param>
        /// <param name="listId"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use siteid for AllListsAux table")]
        [Obsolete("Please use another method which contains siteId in the parameters.")]
        [QueryReview("Item-006")]
        public void ChangeNextItemId(int toId, Guid listId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ToId", toId);
            string cmdText = @"
        DECLARE @NextId INT
        SELECT @NextId=NextAvailableId 
        FROM AllListsAux WITH(UPDLOCK)
        WHERE ListID=@ListId
        IF @ToId>=@NextId
        BEGIN
          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
          WHERE ListID=@ListId
        END";
            mQueryWorker.ExecuteNonQuery(cmdText);
        }

        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-006")]
        public void ChangeNextItemId(int toId, Guid siteId, Guid listId)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.Connection.CreateCommand())
                {
                    cmd.CommandText = "[dbo].[proc_SetNextId]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SiteId", siteId) { Direction = System.Data.ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter("@WebId", Guid.Empty) { Direction = System.Data.ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter("@ListId", listId) { Direction = System.Data.ParameterDirection.Input });
                    cmd.Parameters.Add(new SqlParameter("@NextAvailableId", toId + 1) { Direction = System.Data.ParameterDirection.Input });
                    cmd.ExecuteNonQuery();
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

            #region use SQL
            //            mQueryWorker.ClearParameters();
            //            mQueryWorker.AddParameter("@SiteId", siteId);
            //            mQueryWorker.AddParameter("@ListId", listId);
            //            mQueryWorker.AddParameter("@ToId", toId);
            //            string cmdText = @"
            //        DECLARE @NextId INT
            //        SELECT @NextId=NextAvailableId 
            //        FROM AllListsAux WITH(UPDLOCK)
            //        WHERE ListID=@ListId AND SiteId=@SiteId
            //        IF @ToId>=@NextId
            //        BEGIN
            //          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
            //          WHERE ListID=@ListId AND SiteId=@SiteId
            //        END";
            //            mQueryWorker.ExecuteNonQuery(cmdText);
            #endregion
        }

        /// <summary>
        /// keep 源端的Item Id
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Add SiteId to for AllUserData and AllListsAux table")]
        [QueryReview("Item-007")]
        public int ChangeItemId(Guid siteId, Guid listId, Guid parentId, Guid id, int fromId, int toId)
        {
            if (fromId == toId)
            {
                return 0;
            }

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);//用于查询UserData确定ToId是否可用。
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@FromId", fromId);
            mQueryWorker.AddParameter("@ToId", toId);

            string cmdText = @"
        SET NOCOUNT ON
        DECLARE @NextId INT
        BEGIN TRAN
        
        
        IF EXISTS(SELECT TOP 1 0 
        FROM AllUserData 
        WHERE tp_ListId=@ListId 
        AND tp_IsCurrentVersion=1 
        AND tp_ID=@ToId
        AND tp_CalculatedVersion=0
        AND tp_SiteId=@SiteId
        )
        BEGIN
          COMMIT TRAN
          SELECT -1
          RETURN
        END
        
        SELECT @NextId=NextAvailableId 
        FROM AllListsAux WITH(UPDLOCK)
        WHERE ListID=@ListId AND SiteId=@SiteId
        
        IF @@ROWCOUNT <> 1
        BEGIN
          ROLLBACK TRAN
          SELECT -102
          RETURN
        END
        
        IF @ToId>@FromId AND @ToId>=@NextId
        BEGIN
          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
          WHERE ListID=@ListId AND SiteId=@SiteId
          
          IF @@ROWCOUNT <> 1
          BEGIN
            ROLLBACK TRAN
            SELECT -103
            RETURN
          END
        END
        
        -- Do we need to handle version?
        
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId,tp_ItemOrder=(@ToId*100) 
        WHERE tp_SiteId=@SiteId 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        AND tp_ParentId=@ParentId 
        AND tp_DocId=@Id 
        END
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -105
          RETURN
        END
        
        UPDATE AllDocs SET DoclibRowId=@ToId WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id 
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END

        UPDATE NameValuePair SET ItemId=@ToId WHERE SiteId=@SiteId ListId=@ListId AND ItemId=@FromId            

        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END
                
        COMMIT TRAN
        SELECT 0
        RETURN";

            int returnCode = 0;
            try
            {
                returnCode = (int)mQueryWorker.ExecuteScalar(cmdText);
                // returnCode=0, change sucessfully
                // returnCode<0, change failed.
                if (returnCode < 0)
                {
                    logger.Debug("Cannot change item id. SiteId:{0}, Id:{1}, FromId:{2}, ToId:{3}, ReturnCode:{4}",
                        siteId, id, fromId, toId, returnCode);
                }
            }
            catch (Exception e)
            {
                logger.Warn(WrapperQueryServiceResource.KeepItemIDError, e);
                returnCode = -1000;
            }
            return returnCode;
        }

        /// <summary>
        /// keep 源端的Item Id
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="rootFolderId"></param>
        /// <param name="itemType">
        /// itemType=1, list item
        /// itemType=2, document
        /// itemType=3, folder
        /// </param>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="mQueryWorker"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Add SiteId for AllListsAux table")]
        public int ChangeItemId(
            Guid siteId,
            Guid id,
            Guid rootFolderId,
            int itemType,
            int fromId,
            int toId)
        {
            if (fromId == toId)
            {
                return 0;
            }


            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@ItemType", itemType);
            mQueryWorker.AddParameter("@ToId", toId);
            mQueryWorker.AddParameter("@RootFolderId", rootFolderId);

            string cmdText = @"
        SET NOCOUNT ON
        DECLARE @FromId INT
        DECLARE @NextId INT
        DECLARE @WebId uniqueidentifier
        DECLARE @ListId uniqueidentifier
        DECLARE @FromDirName NVARCHAR(256)
        DECLARE @FromLeafName NVARCHAR(128)
        DECLARE @FromParentId uniqueidentifier
        DECLARE @HasAttachment BIT
        DECLARE @FromItemOrder INT
        BEGIN TRAN
        
        SELECT @WebId=WebId,@ListId=ListId,@FromId=DoclibRowId,@FromDirName=DirName,@FromLeafName=LeafName,@FromParentId=ParentId
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND Id=@Id
        
        IF @@ROWCOUNT=0
        BEGIN
          COMMIT TRAN
          SELECT -100
          RETURN
        END
        
        IF @FromId IS NULL
        BEGIN
          COMMIT TRAN
          SELECT -101
          RETURN
        END
        
        IF @FromId=@ToId
        BEGIN
          COMMIT TRAN
          SELECT 0
          RETURN
        END
        
        IF EXISTS(SELECT TOP 1 tp_ID 
        FROM AllUserData WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId 
        AND tp_ListId=@ListId 
        AND tp_ID=@ToId 
        AND tp_IsCurrentVersion=1
        )
        BEGIN
          COMMIT TRAN
          SELECT -1
          RETURN
        END
        
        SELECT @NextId=NextAvailableId 
        FROM AllListsAux WITH(UPDLOCK)
        WHERE ListID=@ListId AND SiteId=@SiteId
        
        IF @@ROWCOUNT <> 1
        BEGIN
          ROLLBACK TRAN
          SELECT -102
          RETURN
        END
        
        IF @ToId>@FromId AND @ToId>=@NextId
        BEGIN
          UPDATE AllListsAux SET NextAvailableId=@ToId+1 
          WHERE ListID=@ListId AND SiteId=@SiteId
          
          IF @@ROWCOUNT <> 1
          BEGIN
            ROLLBACK TRAN
            SELECT -103
            RETURN
          END
        END
        
        -- Do we need to handle version?
        
        SET @HasAttachment = 0
        
        SELECT @HasAttachment=tp_HasAttachment,@FromItemOrder=tp_ItemOrder
        FROM AllUserData WITH(NOLOCK)
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -104
          RETURN
        END
        
        DECLARE @ToLeafName NVARCHAR(128)
        IF @ItemType <> 1
        BEGIN
          SET @ToLeafName=@FromLeafName
        END
        ELSE
        BEGIN
          SET @ToLeafName=CAST(@ToId AS NVARCHAR(128))+N'_.000'
        END
        
        IF @FromItemOrder IS NULL
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId 
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        END
        ELSE
        BEGIN
          UPDATE AllUserData 
        SET tp_Id=@ToId,tp_ItemOrder=(@ToId*100) 
        WHERE tp_SiteId=@SiteId 
        AND tp_ParentId=@FromParentId 
        AND tp_DocId=@Id 
        AND tp_DeleteTransactionId=0x
        AND tp_IsCurrentVersion=1
        END
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -105
          RETURN
        END
        
        UPDATE AllDocs SET LeafName=@ToLeafName,DoclibRowId=@ToId WHERE SiteId=@SiteId AND ParentId=@FromParentId AND Id=@Id AND DeleteTransactionId=0x
        
        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END

        UPDATE NameValuePair SET ItemId=@ToId WHERE SiteId=@SiteId ListId=@ListId AND ItemId=@FromId            

        IF @@ERROR <> 0
        BEGIN
          ROLLBACK TRAN
          SELECT -106
          RETURN
        END
        
        IF @HasAttachment=1
        BEGIN
          DECLARE @AttachmentId uniqueidentifier
          SELECT @AttachmentId=Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@RootFolderId AND LeafName='Attachments' AND DeleteTransactionId=0x
          
          IF @@ROWCOUNT <> 0
          BEGIN
            SELECT @AttachmentId=Id FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@AttachmentId AND LeafName=CAST(@FromId AS NVARCHAR(128)) AND DeleteTransactionId=0x
            
            IF @@ROWCOUNT <> 0
            BEGIN
              DECLARE @ToIdString NVARCHAR(128)
              DECLARE @ToAttachmentDirName NVARCHAR(256)
              SET @ToIdString=CAST(@ToId AS NVARCHAR(128))
              SET @ToAttachmentDirName=@FromDirName+'/Attachments/'+@ToIdString
              
              UPDATE AllDocs SET LeafName=@ToIdString WHERE SiteId=@SiteId AND Id=@AttachmentId AND DeleteTransactionId=0x
              UPDATE AllDocs SET DirName=@ToAttachmentDirName WHERE SiteId=@SiteId AND ParentId=@AttachmentId AND DeleteTransactionId=0x
              
              IF @@ERROR <> 0
              BEGIN
                ROLLBACK TRAN
                SELECT -107
                RETURN
              END
            END
          END
        END
        
        COMMIT TRAN
        SELECT 0
        RETURN";

            int returnCode = 0;
            try
            {
                returnCode = (int)mQueryWorker.ExecuteScalar(cmdText);
                // returnCode=0, change sucessfully
                // returnCode<0, change failed.
                if (returnCode < 0)
                {
                    logger.Debug("Cannot change item id. SiteId:{0}, Id:{1}, FromId:{2}, ToId:{3}, ItemType:{4}, ReturnCode:{5}",
                        siteId, id, fromId, toId, itemType, returnCode);
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.KeepItemIDError, e);
                //mLog.Warn("Cannot change item id. SiteId:{0}, Id:{1}, FromId:{2}, ToId:{3}, ItemType:{4}",
                //siteId, id, fromId, toId, itemType, e);
                returnCode = -1000;
            }
            return returnCode;
        }

        /// <summary>
        /// 检查list下itemId是否被占用,没被占用返回true。
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool CheckItemIdAvailable(Guid siteId, Guid listId, int itemId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ID", itemId);
            string commandText = @"SELECT TOP 1 0 FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId=0x OR tp_DeleteTransactionId!=0x) AND tp_IsCurrentVersion=1 AND tp_ID=@ID";
            return mQueryWorker.ExecuteScalar(commandText) == null;
        }



        /// <summary>
        /// 更新Item的Level信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <param name="version"></param>
        /// <param name="originaleLevel"></param>
        /// <param name="draftOwnerId"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-008")]
        public void ChangeLevelByNative(AveBaseItemInfo info, IAveListItem item, int version, int originaleLevel, int draftOwnerId)
        {
            try
            {
                //mLog.Info("change item level, item level:{0},to level:{1}, version:{2}, draftOwnerId:{3}", (int)item.Level, originaleLevel, version, draftOwnerId);
                string allDocsCmdText = @"update AllDocs set Level=@Level,DraftOwnerId=@DraftOwnerId where SiteID=@SiteID and DeleteTransactionId=0x and ParentID=@ParentID and Id=@ID and Level=@OldLevel";
                string allUserdataCmdText = @"UPDATE AllUserData SET tp_Level=@Level,tp_DraftOwnerId=@DraftOwnerId WHERE tp_SiteID=@SiteID AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocID=@ID AND tp_CalculatedVersion=0 AND tp_Level=@OldLevel";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@OldLevel", (int)item.Level);
                mQueryWorker.AddParameter("@Level", originaleLevel);
                if ((originaleLevel == 2 || originaleLevel == 255) && draftOwnerId > 0)
                {
                    mQueryWorker.AddParameter("@DraftOwnerId", draftOwnerId);
                }
                else
                {
                    mQueryWorker.AddParameter("@DraftOwnerId", DBNull.Value);
                }
                mQueryWorker.AddParameter("@ID", item.UniqueId);
                mQueryWorker.AddParameter("@ListID", item.ParentList.ID);
                mQueryWorker.AddParameter("@UIVersion", version);

                if (mQueryWorker.ExecuteNonQuery(allDocsCmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None) + mQueryWorker.ExecuteNonQuery(allUserdataCmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows) > 0)
                {
                    info.Level = originaleLevel;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update File Level.File name:{0}. Reason:{1}.", item.Name, e);
            }
        }
        //TODO:Combine it with the function that with the same name but belong to aveDoc
        /// <summary>
        /// 更新Document/Item的CheckOutUserID
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="uniqueID"></param>
        /// <param name="newUserID"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-009")]
        public void ChangeCheckoutUserID(AveBaseItemInfo info, Guid uniqueID, int newUserID)
        {
            //只有IsSPInstalled 才能进入到这个Dll中执行;
            //if (!AveEnvironment.IsSPInstalled)
            //{ return; }
            try
            {
                string updateAllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND DeleteTransactionId=0x AND ParentId=@ParentId AND  ID=@ID";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", uniqueID);
                mQueryWorker.AddParameter("@SiteID", info.SiteId);
                mQueryWorker.AddParameter("@UserID", newUserID);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);

                string updateAllUserData = string.Empty;
                updateAllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocID=@ID";
                mQueryWorker.ExecuteNonQuery(updateAllDocs, VersionOption.AllPublishingVersions, RowOrdinalOption.None);
                mQueryWorker.ExecuteNonQuery(updateAllUserData, VersionOption.AllPublishingVersions, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update AllDocs or AllUserData.UniqueID:{0}, Reason:{1}.", uniqueID, e);
            }
        }

        public void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, Guid parentId, int newUserID)
        {
            try
            {
                string updateAllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND DeleteTransactionId=0x AND ParentId=@ParentId AND  ID=@ID";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", uniqueID);
                mQueryWorker.AddParameter("@SiteID", siteId);
                mQueryWorker.AddParameter("@UserID", newUserID);
                mQueryWorker.AddParameter("@ParentId", parentId);

                string updateAllUserData = string.Empty;
                updateAllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocID=@ID";
                mQueryWorker.ExecuteNonQuery(updateAllDocs, VersionOption.AllPublishingVersions, RowOrdinalOption.None);
                mQueryWorker.ExecuteNonQuery(updateAllUserData, VersionOption.AllPublishingVersions, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update AllDocs or AllUserData.UniqueID:{0}, Reason:{1}.", uniqueID, e);
            }
        }

        /// <summary>
        /// 更新Document/Item的CheckOutUserID
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="uniqueID"></param>
        /// <param name="newUserID"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-009")]
        public void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID)
        {
            try
            {
                string updateAllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND ID=@ID AND DeleteTransactionId=0x";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", uniqueID);
                mQueryWorker.AddParameter("@SiteID", siteId);
                mQueryWorker.AddParameter("@UserID", newUserID);

                string updateAllUserData = string.Empty;
                updateAllUserData = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x  AND tp_IsCurrentVersion=1 AND tp_DocID=@ID";

                mQueryWorker.ExecuteNonQuery(updateAllDocs, VersionOption.AllPublishingVersions, RowOrdinalOption.None);
                mQueryWorker.ExecuteNonQuery(updateAllUserData, VersionOption.AllPublishingVersions, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occur when update AllDocs or AllUserData.UniqueID:{0}. Reason:{1}.", uniqueID, e);
            }
        }

        // TODO:Add User Mapping
        // TODO:make this an the same function in AveSPItem one function
        /// <summary>
        /// 更新Item下所有Version的CheckoutUserId信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="uniqueID"></param>
        /// <param name="newUserID"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-009")]
        public void ChangeCheckoutUserIDForAllVersion(Guid siteId, Guid parentId, Guid fileId, int newUserID)
        {
            try
            {
                string updateAllDocs = @"UPDATE AllDocs SET CheckoutUserId=@UserID WHERE SiteId=@SiteID AND DeleteTransactionId=0x AND ParentId=@ParentID AND ID=@ID";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", fileId);
                mQueryWorker.AddParameter("@SiteID", siteId);
                mQueryWorker.AddParameter("@UserID", newUserID);
                mQueryWorker.AddParameter("@ParentID", parentId);
                mQueryWorker.ExecuteNonQuery(updateAllDocs, VersionOption.AllPublishingVersions, RowOrdinalOption.None);

                string updateAllUserDate = @"UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocID=@ID";
                mQueryWorker.ExecuteNonQuery(updateAllUserDate, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred when change checkout user. UniqueID:{0}, Reason:{1}.", fileId, e);
            }
        }

        /// <summary>
        /// 获取Document的当前UIVersion
        /// 有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("Please use GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id) for proformance")]
        public int GetCurrentUIVersion(Guid siteId, Guid id)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@SiteId", siteId);
            const string cmdText = @"SELECT UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x0 and IsCurrentVersion=1";
            return (int)mQueryWorker.ExecuteScalar(cmdText);
        }

        /// <summary>
        /// 获取Document的当前UIVersion
        /// 有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid id)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetUserDataJunction"))
            {

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@Id", id);
                mQueryWorker.AddParameter("@SiteId", siteId);
                string cmdText = @"SELECT UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x0 AND Id=@Id and IsCurrentVersion=1";
                if (parentId != Guid.Empty)
                {
                    cmdText = @"SELECT UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x0 AND ParentId=@ParentId AND Id=@Id and IsCurrentVersion=1";
                    mQueryWorker.AddParameter("@ParentId", parentId);
                }
                return (int)mQueryWorker.ExecuteScalar(cmdText);

            }

        }

        /// <summary>
        /// 获取Item的LastModifiedTime
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="docLibRowId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use siteId for AllUserData table.")]
        [Obsolete("Please use another method which contains siteid in the parameters.")]
        public DateTime GetLastModifiedByNative(Guid listId, int docLibRowId)
        {
            DateTime lastModified = DateTime.MinValue;
            try
            {
                mQueryWorker.ClearParameters();
                string cmdText = @"SELECT tp_Modified FROM AllUserData with(nolock)
                                        WHERE tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND (tp_Level=1 or tp_Level=2 or tp_Level=255)";
                mQueryWorker.AddParameter("@tp_ListId", listId);
                mQueryWorker.AddParameter("@Id", docLibRowId);
                lastModified = (DateTime)mQueryWorker.ExecuteScalar(cmdText);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting modified time of item: {0}. Error: {1}.", docLibRowId, e.ToString());
            }
            return lastModified;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public DateTime GetLastModifiedByNative(Guid siteId, Guid listId, int docLibRowId, bool onlyPublishVersion)
        {
            DateTime lastModified = DateTime.MinValue;
            try
            {
                mQueryWorker.ClearParameters();
                string cmdText = onlyPublishVersion ?
                    @"SELECT tp_Modified FROM AllUserData with(nolock)
                                        WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND (tp_Level=1 or tp_Level=2 or tp_Level=255)" :
                    @"SELECT tp_Modified FROM AllUserData with(nolock)
                                        WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND (tp_Level=1 or tp_Level=2 or tp_Level=255) AND tp_IsCurrent=1";
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@tp_ListId", listId);
                mQueryWorker.AddParameter("@Id", docLibRowId);
                lastModified = (DateTime)mQueryWorker.ExecuteScalar(cmdText);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting modified time of item: {0}. Error: {1}.", docLibRowId, e.ToString());
            }
            return lastModified;
        }

        /// <summary>
        /// just for doc
        /// 把冲突的item移到冲突文件夹下，并且修改其name和tp_guid
        /// 无API实现
        /// </summary>
        /// <param name="parentList"></param>
        /// <param name="parentFolder"></param>
        /// <param name="listItem"></param>
        /// <param name="mmQueryWorker"></param>
        [QueryReview("2012/12/17", "Austin Han", true, "Use site id for AllDocs and AllUserData table")]
        [QueryReview("Item-012")]
        public bool MoveDocToConflictFolderByNative(Guid listId, string parentFolderServerRelativeUrl, string listItemName, int docLibRowId, Guid listItemUniqueId, Guid conflictFolderUniqueId, string conflictFolderName, DateTime lastModified, bool isSourceWin, Guid siteId)
        {
            try
            {
                string NewName = string.Empty;
                if (isSourceWin)
                {
                    NewName = AveSPUtility.GetConflictNewName(listItemName, lastModified);
                }
                else
                {
                    NewName = listItemName;
                }

                mQueryWorker.ClearParameters();
                string cmdText = @"UPDATE AllDocs SET DirName=@DirName, LeafName=@LeafName, ParentId=@ParentId
                                        WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x;";

                if (parentFolderServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    mQueryWorker.AddParameter("@DirName", parentFolderServerRelativeUrl.Substring(1) + "/" + conflictFolderName);
                }
                else
                {
                    mQueryWorker.AddParameter("@DirName", parentFolderServerRelativeUrl + "/" + conflictFolderName);
                }
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@LeafName", NewName);
                mQueryWorker.AddParameter("@ParentId", conflictFolderUniqueId);
                mQueryWorker.AddParameter("@Id", listItemUniqueId);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllPublishingVersions, RowOrdinalOption.None);

                mQueryWorker.ClearParameters();
                cmdText = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_Guid=@tp_Guid
                                WHERE tp_SiteId=@SiteId AND tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x;";
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@tp_ParentId", conflictFolderUniqueId);
                mQueryWorker.AddParameter("@tp_Guid", Guid.NewGuid());
                mQueryWorker.AddParameter("@tp_ListId", listId);
                mQueryWorker.AddParameter("@Id", docLibRowId);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);

                mQueryWorker.ClearParameters();
                cmdText = @"UPDATE AllUserDataJunctions SET tp_ParentId=@tp_ParentId
                        WHERE tp_SiteId=@tp_SiteId AND tp_DeleteTransactionId=0x AND tp_DocId=@tp_DocId";
                mQueryWorker.AddParameter("@tp_ParentId", conflictFolderUniqueId);
                mQueryWorker.AddParameter("@tp_SiteId", siteId);
                mQueryWorker.AddParameter("@tp_DocId", listItemUniqueId);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);
                return true;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while moving the doc [{0}] to the conflict folder. Error: {1}.", listItemName, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 把冲突的item移到冲突文件夹下，并且修改其name和tp_guid
        /// 无API实现
        /// </summary>
        /// <param name="titleColName"></param>
        /// <param name="parentFolderServerRelativeUrl"></param>
        /// <param name="conflictFolderName"></param>
        /// <param name="conflictFolderlistId"></param>
        /// <param name="conflictFolderUniqueId"></param>
        /// <param name="docLibRowId"></param>
        /// <param name="listItemUniqueId"></param>
        /// <param name="lastModified"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Add SiteId for AllDocs and AllUserData tables")]
        public bool MoveListItemToConflictFolderByNative(string titleColName, string parentFolderServerRelativeUrl, string conflictFolderName, Guid conflictFolderlistId, Guid conflictFolderUniqueId, int docLibRowId, Guid listItemUniqueId, DateTime lastModified, Guid siteId)
        {
            try
            {
                string TimeName;
                TimeName = "(" + AveDateTimeUtility.ConvertToType008(lastModified) + ")";
                mQueryWorker.ClearParameters();
                string cmdText = @"UPDATE AllDocs SET DirName=@DirName, ParentId=@ParentId
                                            WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x;";
                if (parentFolderServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    mQueryWorker.AddParameter("@DirName", parentFolderServerRelativeUrl.Substring(1) + "/" + conflictFolderName);
                }
                else
                {
                    mQueryWorker.AddParameter("@DirName", parentFolderServerRelativeUrl + "/" + conflictFolderName);
                }
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentId", conflictFolderUniqueId);
                mQueryWorker.AddParameter("@Id", listItemUniqueId);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllPublishingVersions, RowOrdinalOption.None);
                if (string.IsNullOrEmpty(titleColName))
                {
                    cmdText = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId,tp_GUID=@tp_GUID  WHERE tp_SiteId=@tp_SiteId AND tp_ListId = @tp_ListId AND tp_ID = @tp_ID AND tp_DeleteTransactionId = 0x";
                    mQueryWorker.AddParameter("@tp_SiteId", siteId);
                    mQueryWorker.AddParameter("@tp_ListId", conflictFolderlistId);
                    mQueryWorker.AddParameter("@tp_ID", docLibRowId);
                    mQueryWorker.AddParameter("@tp_GUID", Guid.NewGuid());
                    mQueryWorker.AddParameter("@tp_ParentId", conflictFolderUniqueId);
                    mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);
                }
                else
                {

                    cmdText = string.Format(@"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_GUID=@tp_GUID,{0}={0}+'{1}'
                                                      WHERE tp_Listid=@ListId And tp_DeleteTransactionId=0x 
                                                      And tp_ID=@ID;", titleColName, TimeName);
                    mQueryWorker.AddParameter("@tp_ParentId", conflictFolderUniqueId);
                    mQueryWorker.AddParameter("@tp_GUID", Guid.NewGuid());
                    mQueryWorker.AddParameter("@ListId", conflictFolderlistId);
                    mQueryWorker.AddParameter("@ID", docLibRowId);
                    mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);
                }

                mQueryWorker.ClearParameters();
                cmdText = @"UPDATE AllUserDataJunctions SET tp_ParentId=@tp_ParentId 
                        WHERE tp_SiteId=@tp_SiteId AND tp_DeleteTransactionId=0x AND tp_DocId=@tp_DocId";
                mQueryWorker.AddParameter("@tp_ParentId", conflictFolderUniqueId);
                mQueryWorker.AddParameter("@tp_SiteId", siteId);
                mQueryWorker.AddParameter("@tp_DocId", listItemUniqueId);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataAllRows);
                return true;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while moving the item [{0}] to the conflict folder. Error: {1}.", docLibRowId, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 查询ThumbNail的author/tp_create/create，editor/tp_Modify/modify
        /// 有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        /// <param name="uiversion"></param>
        /// <param name="level"></param>
        /// <param name="author"></param>
        /// <param name="editor"></param>
        /// <param name="tp_create"></param>
        /// <param name="tp_modify"></param>
        /// <param name="create"></param>
        /// <param name="modify"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-012")]
        public bool QueryBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, AveBasicItemInfo basicItemInfo)
        {
            bool existRecord = false;
            mQueryWorker.ClearParameters();
            string cmdText = @"Select TimeCreated,TimeLastModified From AllDocs With(noLock) Where SiteId =@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And Level=@Level And UIVersion=@UIVersion";
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UIVersion", uiversion);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Level", level);
            using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
            {
                if (dr.Read())
                {
                    basicItemInfo.Create = dr.GetDateTime(0);
                    basicItemInfo.Modify = dr.GetDateTime(1);
                }
                else
                {
                    return existRecord;
                }
            }

            cmdText = @"Select tp_Modified,tp_Created,tp_Author,tp_Editor From AllUserData With(noLock) Where tp_SiteId =@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) And tp_ParentId=@ParentId And tp_DocId=@Id And tp_UIVersion=@UIVersion And tp_Level=@Level";
            using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
            {
                if (dr.Read())
                {
                    basicItemInfo.Tp_modify = dr.GetDateTime(0);
                    basicItemInfo.Tp_create = dr.GetDateTime(1);
                    basicItemInfo.Author = dr.GetInt32(2);
                    basicItemInfo.Editor = dr.GetInt32(3);
                }
                else
                {
                    return existRecord;
                }
            }
            return true;
        }

        /// <summary>
        /// 更新ThumbNail的BasicInfo(author/tp_create/create，editor/tp_Modify/modify)
        /// 有API实现，但有局限性(比如一个item已经是approve状态  这个时候就不能用api更新)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        /// <param name="uiversion"></param>
        /// <param name="level"></param>
        /// <param name="author"></param>
        /// <param name="editor"></param>
        /// <param name="tp_create"></param>
        /// <param name="tp_modify"></param>
        /// <param name="create"></param>
        /// <param name="modify"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-012")]
        public void UpdateBasicInfoForThumbNail(Guid siteId, Guid parentId, Guid itemId, int uiversion, int level, AveBasicItemInfo basicItemInfo)
        {
            mQueryWorker.ClearParameters();
            string cmdText = @"Update AllDocs Set TimeCreated=@TimeCreated, TimeLastModified = @TimeLastModified Where SiteId =@SiteId And DeleteTransactionId=0x And ParentId=@ParentId And Id=@Id And UIVersion=@UIVersion And Level=@Level";
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UIVersion", uiversion);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Level", level);
            mQueryWorker.AddParameter("@TimeCreated", basicItemInfo.Create);
            mQueryWorker.AddParameter("@TimeLastModified", basicItemInfo.Modify);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);

            cmdText = @"Update AllUserData Set tp_Modified=@tp_Modified,tp_Created=@tp_Created,tp_Author=@tp_Author,tp_Editor=@tp_Editor Where tp_SiteId =@SiteId And tp_DeleteTransactionId=0x And (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1)  And tp_ParentId=@ParentId And tp_DocId=@Id And tp_UIVersion=@UIVersion And tp_Level=@Level";
            mQueryWorker.AddParameter("@tp_Modified", basicItemInfo.Tp_modify);
            mQueryWorker.AddParameter("@tp_Created", basicItemInfo.Tp_create);
            mQueryWorker.AddParameter("@tp_Author", basicItemInfo.Author);
            mQueryWorker.AddParameter("@tp_Editor", basicItemInfo.Editor);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows);
        }

        /// <summary>
        /// 获取Item的Internal Version
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isVersion"></param>
        /// <param name="id"></param>
        /// <param name="UIVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Qianwen Hu")]
        [Obsolete("There is no internal version in SharePoint 2013")]
        public int? GetInternalVersion(AveBaseItemInfo info, bool isVersion, Guid id, int UIVersion)
        {
            //只有IsSPInstalled 才能进入到这个Dll中执行;
            //if (!AveEnvironment.IsSPInstalled)
            //{
            //    return -1;
            //}
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", id);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@UIVersion", UIVersion);
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            string cmdText = string.Empty;
            if (!isVersion)
            {
                cmdText = @"SELECT InternalVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND ParentId=@ParentId AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
            }
            else
            {
                cmdText = @"SELECT InternalVersion FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
            }

            object result = mQueryWorker.ExecuteScalar(cmdText);
            if (result != null && result is int)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 根据parentId和Name去获取File的checkout状态
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool IsCheckOutFile(AveBaseItemInfo info, Guid siteId, Guid parentId, string name)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@LeafName", name);

            const string cmdText = @"SELECT Id, Level,CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND  DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName order by UIVersion ASC";
            bool isCheckOutFile = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte level = (byte)dr["Level"];
                        if (level == 255)
                        {
                            isCheckOutFile = true;
                            if (info != null)
                            {
                                info.CheckOutFileUniqueID = (Guid)dr["Id"];
                                info.CheckoutUserId = (int)dr["CheckOutUserId"];
                            }
                            break;
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
            return isCheckOutFile;
        }

        /// <summary>
        /// 根据File的UniqueId去获取File的checkout状态
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="checkId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "Add siteId for AllDocs table")]
        public bool IsCheckOutFile(Guid siteId, Guid fileId, ref int checkId)
        {
            checkId = -1;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ID", fileId);

            const string cmdText = @"SELECT Id, Level,CheckOutUserId FROM AllDocs With(NoLock) WHERE SiteId=@SiteId AND ID=@ID AND Level=255";
            bool isCheckOutFile = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
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
            return isCheckOutFile;
        }

        /// <summary>
        /// 判断某个Version是否为Checkout Version
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="checkId"></param>
        /// <returns></returns>
        public bool IsCheckOutVersion(Guid siteId, Guid fileId, int uiVersion, ref int checkId)
        {
            checkId = -1;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ID", fileId);
            mQueryWorker.AddParameter("@UIVersion", uiVersion);

            const string cmdText = @"SELECT Id, Level, CheckOutUserId FROM AllDocs With(NoLock) WHERE SiteId=@SiteId AND ID=@ID AND Level=255 AND UIVersion=@UIVersion";
            bool isCheckOutFile = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
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
            return isCheckOutFile;
        }

        /// <summary>
        /// 根据File的Url去获取File的checkout状态
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <param name="checkId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool IsCheckOutFile(Guid siteId, string url, ref int checkId)
        {
            checkId = -1;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            string fileName = url.Substring(url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).Trim('/');
            string dirName = url.Substring(0, url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).Trim('/');
            mQueryWorker.AddParameter("@LeafName", fileName);
            mQueryWorker.AddParameter("@DirName", dirName);

            const string cmdText = @"SELECT Id, Level,CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DirName=@DirName AND LeafName=@LeafName AND DeleteTransactionId=0x AND Level=255";
            bool isCheckOutFile = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
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
            return isCheckOutFile;
        }

        /// <summary>
        /// 根据File的DocLibRowId去获取File的checkout状态
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="fileId"></param>
        /// <param name="checkId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id)
        {
            checkId = -1;
            id = Guid.Empty;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ID", fileId);

            const string cmdText = @"SELECT ID, CheckOutUserId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ListId=@ListId AND DoclibRowId=@ID AND DeleteTransactionId=0x AND Level=255";
            bool isCheckOutFile = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
                        id = (Guid)dr["ID"];
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
            return isCheckOutFile;
        }

        /// <summary>
        /// for form library item, to change content.
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="content"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We don't support to update content by native method.")]
        public void ChangeContentByNative(AveBaseItemInfo info, byte[] content)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 更新Item的Internal Version
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="restoringDto"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [Obsolete("We don't need internal version for SP2013.")]
        [QueryReview("Item-014")]
        public int SetInternalVersion(AveBaseItemInfo info, RestoringDto restoringDto, int version)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 修改Document的CheckIn comment
        /// checkin comment可以通过API改的，但是这个主要是为了改Approve Comment，因为Approve comment和Checkin Comment是同一个。
        /// </summary>
        /// <param name="checkinComment"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateCheckinCommentByNative(AveBaseItemInfo info, Guid fileGuid, string checkinComment)
        {
            string cmdStr = @"UPDATE AllDocs SET CheckinComment=@CheckInComment WHERE SiteId=@SiteId and DeleteTransactionId=0x and ParentId=@ParentId and Id=@ID and UIVersion=@UIVersion";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@CheckInComment", checkinComment);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            mQueryWorker.AddParameter("@ID", fileGuid);
            mQueryWorker.AddParameter("@UIVersion", info.Version);
            mQueryWorker.ExecuteNonQuery(cmdStr, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

       
        /// <summary>
        /// 将Document的Content清空.
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void ResetContentByNative(AveSPItemNativeInfo info)
        {
            throw new NotSupportedException("Failed to restore only the historical versions. To restore historical versions in SharePoint 2013, select the desired versions along with the current version, and then perform the restore job.");//已经国际化        
        }

        /// <summary>
        /// 获取List下最大的RowId
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetMaxListItemRowId(Guid siteId, Guid listId)
        {
            int maxRowId = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            string cmdText = @"SELECT MAX(DoclibRowId) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId and ListId=@ListId AND (DoclibRowId IS NOT NULL)";
            string result = mQueryWorker.ExecuteScalar(cmdText).ToString();
            if (!string.IsNullOrEmpty(result))
            {
                int.TryParse(result, out maxRowId);
            }
            return maxRowId;
        }

        /// <summary>
        /// 获取ParentId下最大的LeafName
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public string GetMaxSubLeafName(Guid siteId, Guid parentId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            string cmdText = @"SELECT MAX(LeafName) FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND (DeleteTransactionId=0x OR DeleteTransactionId<>0x) AND ParentId=@ParentId";
            return mQueryWorker.ExecuteScalar(cmdText) as string;
        }
        
        /// <summary>
        /// ListItem的冲突判断
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="restoringDto"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        public void CheckConflictInfoForListItem(Guid siteId, Guid listId, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@TP_ID", restoringDto.NameMapping.Substring(0, restoringDto.NameMapping.IndexOf("_", StringComparison.OrdinalIgnoreCase)));
            string cmdText = @"select tp_DeleteTransactionId,tp_ID,tp_Level ,tp_UIVersion FROM AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId =@ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion =1 and tp_ID=@TP_ID and tp_CalculatedVersion=0 and tp_Level>0 and tp_RowOrdinal=0";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
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

            cmdText = "select tp_ID FROM AllUserData With(nolock) where tp_SiteId=@SiteId AND tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and tp_ID=@TP_ID and tp_CalculatedVersion=0 and tp_Level>0 and tp_RowOrdinal=0";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1; //conflict with RecycleBin
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }

        /// <summary>
        /// Document的冲突判断
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="restoringDto"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        public void CheckConflictInfo(Guid siteId, Guid parentId, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@LeafName", restoringDto.NameMapping);
            string cmdText = @"SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
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
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@LeafName", restoringDto.NameMapping);
            cmdText = @"SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1; //conflict with RecycleBin
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }

        /// <summary>
        /// Document的冲突判断
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <param name="restoringDto"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We should use tp_ListId for AllUserData table.")]
        [Obsolete("Please use another method which contains listid in the parameters.")]
        public void CheckConflictInfo(Guid siteId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@tp_SiteId", siteId);
            mQueryWorker.AddParameter("@tp_ParentId", parentId);
            mQueryWorker.AddParameter("@tp_Guid", tp_Guid);
            string cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData With(nolock)
                                                WHERE tp_SiteId=@tp_SiteId and tp_DeleteTransactionId=0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid;";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
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
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@tp_SiteId", siteId);
            mQueryWorker.AddParameter("@tp_ParentId", parentId);
            mQueryWorker.AddParameter("@tp_Guid", tp_Guid);
            cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData  WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_DeleteTransactionId<>0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid;";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1; //conflict with RecycleBin
                        //SetConflictInfo(ref mConflictRecycleId, dr);
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public void CheckConflictInfo(Guid siteId, Guid listId, Guid parentId, Guid tp_Guid, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@tp_SiteId", siteId);
            mQueryWorker.AddParameter("@tp_ListId", listId);
            mQueryWorker.AddParameter("@tp_ParentId", parentId);
            mQueryWorker.AddParameter("@tp_Guid", tp_Guid);
            string cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData With(nolock)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ListId=@tp_ListId and tp_DeleteTransactionId=0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid;";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
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
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@tp_SiteId", siteId);
            mQueryWorker.AddParameter("@tp_ListId", listId);
            mQueryWorker.AddParameter("@tp_ParentId", parentId);
            mQueryWorker.AddParameter("@tp_Guid", tp_Guid);
            cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData  WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_RowOrdinal=0 and tp_ListId=@tp_ListId and tp_DeleteTransactionId<>0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                                and tp_GUID=@tp_Guid;";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1; //conflict with RecycleBin
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }

        /// <summary>
        /// ListItem的冲突判断
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="restoringDto"></param>
        [QueryReview("2012/12/17", "Austin Han", true, "Add siteid for AllUserData table.")]
        public void CheckConflictInfoForListItem(Guid siteId, Guid listId, string title, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@title", title);

            // Will handle current look as normal ones, so delete "and nvarchar1!='Current'"
            string cmdText = @"select tp_DeleteTransactionId,tp_ID,tp_Level ,tp_UIVersion FROM AllUserData With(nolock) where tp_SiteId=@SiteId and tp_ListId =@ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion =1 and nvarchar1=@title and tp_CalculatedVersion=0 and tp_RowOrdinal=0";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;  //conflict with current document
                        SetConflictInfo(restoringDto, dr);
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

            // Will handle current look as normal ones, so delete "and nvarchar1!='Current'"
            cmdText = @"select tp_ID FROM AllUserData With(nolock) where tp_SiteId=@SiteId and tp_ListId =@ListId and tp_DeleteTransactionId<>0x and tp_IsCurrentVersion =1 and nvarchar1=@title and tp_CalculatedVersion=0 and tp_RowOrdinal=0";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1; //conflict with RecycleBin
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }


        [QueryReview("2012/12/17", "Austin Han")]
        public void CheckConflictInfoBySpecialColumn(Guid siteId, Guid parentId, object columnValue, string fieldColumn, RestoringDto restoringDto)
        {
            int conflictType = 0;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@tp_SiteId", siteId);
            mQueryWorker.AddParameter("@tp_ParentId", parentId);
            mQueryWorker.AddParameter("@ColumnValue", columnValue);

            StringBuilder cmdTextBuilderForDeleted = new StringBuilder();
            cmdTextBuilderForDeleted.Append(@"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId and tp_DeleteTransactionId");
            cmdTextBuilderForDeleted.Append("<>");
            cmdTextBuilderForDeleted.Append("0x and tp_IsCurrentVersion=1 ");
            cmdTextBuilderForDeleted.Append("and ");
            cmdTextBuilderForDeleted.Append(fieldColumn);
            cmdTextBuilderForDeleted.Append("=@ColumnValue;");
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdTextBuilderForDeleted.ToString()))
                {
                    while (dr.Read())
                    {
                        conflictType |= 1;
                        SetConflictInfo(restoringDto, dr);
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

            StringBuilder cmdTextBuilder = new StringBuilder();
            cmdTextBuilder.Append(@"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData WITH(NOLOCK)
                                                WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId and tp_DeleteTransactionId");
            cmdTextBuilder.Append("=");
            cmdTextBuilder.Append("0x and tp_IsCurrentVersion=1 ");
            cmdTextBuilder.Append("and ");
            cmdTextBuilder.Append(fieldColumn);
            cmdTextBuilder.Append("=@ColumnValue;");
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdTextBuilder.ToString()))
                {
                    while (dr.Read())
                    {
                        conflictType |= 2;
                        SetConflictInfo(restoringDto, dr);
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
            restoringDto.ConflictType = (ConflictType)conflictType;
        }

        /// <summary>
        /// 更新document的Content
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fs"></param>
        [QueryReview("2012/12/17", "Austin Han", false, "We should find another method to update content by Native.")]
        public void UpdateFileContentByNative(AveSPItemNativeInfo info, Stream fs)
        {
            throw new NotSupportedException("Failed to restore only the historical versions. To restore historical versions in SharePoint 2013, select the desired versions along with the current version, and then perform the restore job.");//已经国际化      
        }



        /// <summary>
        /// 获取特定Version的Item的Modified属性
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="rowId"></param>
        /// <param name="uiVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public DateTime GetVersionModified(Guid siteId, Guid parentId, int rowId, int uiVersion)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@RowId", rowId);
                mQueryWorker.AddParameter("@ParentId", parentId);
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@UIVersion", uiVersion);
                const string cmdText = @"SELECT tp_Modified FROM AllUserData WITH(NOLOCK) where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) 
AND tp_ParentId=@ParentId AND tp_ID=@RowId AND tp_UIVersion=@UIVersion";
                var obj = mQueryWorker.ExecuteScalar(cmdText);
                if (obj != null && obj != DBNull.Value)
                {
                    return (DateTime)obj;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetVersionModifiedError, ex);
            }
            return DateTime.MinValue;
        }


        /// <summary>
        /// 获取Item的Editor属性
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin")]
        public int GetItemEditorByNative(AveBaseItemInfo info, IAveListItem item)
        {
            int modified = 0;
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", item.ID);
                mQueryWorker.AddParameter("@ListId", item.ParentList.ID);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);

                string cmdText = @"SELECT tp_Editor FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ListId=@ListId AND tp_ID=@ID";
                modified = (int)mQueryWorker.ExecuteScalar(cmdText);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while get item modified. Reason:{0}.", e);
            }
            return modified;
        }

        /// <summary>
        /// 更新Item的Modified属性.
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="item"></param>
        /// <param name="modified"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-018")]
        public void SetItemEditorByNative(AveBaseItemInfo info, IAveListItem item, int modified)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ID", item.ID);
                mQueryWorker.AddParameter("@ListId", item.ParentList.ID);
                mQueryWorker.AddParameter("@Editor", modified);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);

                string cmdText = @"UPDATE AllUserData SET tp_Editor=@Editor WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ListId=@ListId AND tp_ID=@ID  ";
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while set item modified. Reason:{0}.", e);
            }
        }

        /// <summary>
        /// 更新Item的tp_GUID属性
        /// 有API实现
        /// </summary>
        /// <param name="tpGuid"></param>
        /// <param name="itemUniqueId"></param>
        /// <param name="parentUniqueId"></param>
        /// <param name="siteId"></param>
        /// <param name="isCurrentVersion"></param>
        /// <param name="level"></param>
        /// <param name="calculatedVersion"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Item-011")]
        public void UpdateItemGuid(Guid tpGuid, Guid itemUniqueId, Guid parentUniqueId, Guid siteId, bool isCurrentVersion, byte level, int calculatedVersion)
        {
            mQueryWorker.ClearParameters();
            string command = @"Update AllUserData SET tp_GUID=@tp_GUID 
                                WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_ParentId=@ParentId AND tp_DocId=@ID  
                                AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level ;";
            int curentVersion = 0;
            if (isCurrentVersion)
            {
                curentVersion = 1;
            }
            mQueryWorker.AddParameter("tp_GUID", tpGuid);
            mQueryWorker.AddParameter("ParentId", parentUniqueId);
            mQueryWorker.AddParameter("ID", itemUniqueId);
            mQueryWorker.AddParameter("SiteId", siteId);
            mQueryWorker.AddParameter("IsCurrentVersion", curentVersion);
            mQueryWorker.AddParameter("Level", level);
            mQueryWorker.AddParameter("CalculatedVersion", calculatedVersion);

            mQueryWorker.ExecuteNonQuery(command, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows);
        }

        /// <summary>
        /// 获取Site下所有Web的Id信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public List<Guid> GetAllWebsGuidByNative(Guid siteId)
        {
            return mQuerySessionSchema.GetAllWebsGuidByNative(siteId);
        }

        /// <summary>
        /// 根据parentId和folder的name获取folder的UniqueId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>        
        [QueryReview("2012/12/18", "Austin Han")]
        public Guid GetFolderIdByName(string name, Guid siteId, Guid parentId)
        {
            Guid id = Guid.Empty;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@LeafName", name);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            string cmdText = @"SELECT ID FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x AND type=1";

            object idObject = mQueryWorker.ExecuteScalar(cmdText);
            if (idObject != null && idObject != DBNull.Value)
            {
                id = (Guid)idObject;
            }

            return id;
        }

        public Guid GetItemIdByName(Guid siteId, Guid webId, string leafName, string dirName)
        {
            var newId = Guid.Empty;
            try
            {

                string cmdText = string.Format(@"Select Id from AllDocs With(noLock)where SiteId=@SiteId and WebId=@WebId and DeleteTransactionId=0x and LeafName=@LeafName{0}",
                  !string.IsNullOrEmpty(dirName) ? " AND DirName=@DirName" : string.Empty);
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@LeafName", leafName);
                mQueryWorker.AddParameter("@DirName", !string.IsNullOrEmpty(dirName) ? dirName : string.Empty);
                using (var dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        newId = (Guid)dr[0];
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
            return newId;
        }

        /// <summary>
        /// 为目的端还原hidden web做准备
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webSettingInfo"></param>
        /// <param name="siteManagedMappings"></param>
        /// <param name="sourceSiteInfo"></param>
        /// <param name="destSiteUrl"></param>
        /// <param name="webIdMapping"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping)
        {
            return mQuerySessionSchema.ReloadHiddenWebProperty(siteId, webSettingInfo, siteManagedMappings, sourceSiteInfo, destSiteUrl, webIdMapping);
        }

        /// <summary>
        ///判断Web是否删除
        ///效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webUrl"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public bool IsConflictWithRecycle(Guid siteId, string webUrl)
        {
            return mQuerySessionSchema.IsConflictWithRecycle(siteId, webUrl);
        }

        /// <summary>
        /// 将Document的属性(TimeCreated，TimeLastModified,ParentId等)更新到数据库
        /// 无API实现
        /// </summary>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="parentId"></param>
        /// <param name="siteId"></param>
        /// <param name="leafName"></param>
        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Item-001")]
        public void UpdateAllDocsPropertyByNative(DateTime timeCreated, DateTime timeLastModified, Guid parentId, Guid siteId, string leafName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@TimeCreated", timeCreated);
            mQueryWorker.AddParameter("@TimeLastModified", timeLastModified);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@LeafName", leafName);
            string cmdText = @"Update AllDocs set TimeCreated=@TimeCreated, TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ";
            mQueryWorker.ExecuteNonQuery(cmdText);

        }

        /// <summary>
        /// 查询List下下一个可用的DoclibRowId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han", false, "We should use site id for AllListsAux table.")]
        [Obsolete("Please use another method which contains site id in the parameters.")]
        public int GetNextAvailableId(Guid listId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", listId);
            return (int)mQueryWorker.ExecuteScalar("SELECT NextAvailableId FROM AllListsAux WITH(NOLOCK) WHERE ListID=@ListId");
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public int GetNextAvailableId(Guid siteId, Guid listId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            return (int)mQueryWorker.ExecuteScalar(@"SELECT NextAvailableId FROM AllListsAux WITH(NOLOCK) WHERE SiteId=@SiteId AND ListID=@ListId");
        }

        /// <summary>
        /// 根据url查询Web对应的Id
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public Guid GetWebId(Guid siteId, string url)
        {
            return mQuerySessionSchema.GetWebId(siteId, url);
        }

        /// <summary>
        /// 查询特定Item是否删除
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="id"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han")]
        public bool IsItemExist(Guid listId, int id, Guid siteId)
        {
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@tp_ListId", listId);
                mQueryWorker.AddParameter("@tp_ID", id);
                mQueryWorker.AddParameter("@SiteId", siteId);
                string cmdText = @"SELECT count(*) from AllUserData WITH(NOLOCK)
                                        WHERE tp_SiteId=@SiteId and tp_ListId=@tp_ListId and tp_DeleteTransactionId=0x and tp_IsCurrentVersion=1 and tp_ID=@tp_ID;";
                return (int)mQueryWorker.ExecuteScalar(cmdText) == 0;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.CheckSpcifiedItemDeleteError, ex);
                return false;
            }
        }



        [QueryReview("2012/12/18", "Austin Han", true, "Add site id for AllUserData table")]
        public List<int> GetItemsByColumnValue(Guid siteId, Guid listId, string ColName, string ColValue)
        {
            string cmdText = @"select tp_Id FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND " + ColName + " =@Value order by tp_RowOrdinal ASC";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@Value", ColValue);
            List<int> itemIds = new List<int>();
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);
                        if (!itemIds.Contains(id))
                        {
                            itemIds.Add(id);
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception exception)
            {
                throw new AveQueryException(exception.Message, exception);
            }
            return itemIds;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public List<int> GetItemsByColumnValue(Guid siteId, Guid listId, Guid parentId, string colName, string colValue)
        {
            string cmdText = @"select tp_Id FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 
AND tp_ParentId=@parentId And " + colName + " =@Value collate Chinese_PRC_CS_AS_WS order by tp_ID ASC";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@parentId", parentId);
            mQueryWorker.AddParameter("@Value", colValue);
            List<int> itemIds = new List<int>();
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);
                        if (!itemIds.Contains(id))
                        {
                            itemIds.Add(id);
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception exception)
            {
                throw new AveQueryException(exception.Message, exception);
            }
            return itemIds;
        }


        #endregion

        #region IAveBackupRestoreQueryService Members

        #endregion

        /// <summary>
        /// 判断item是否存在，包括回收站，目前用于append
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/18", "Austin Han", false, "We should use site id for AllUserData table.")]
        [Obsolete("Please use another method which contains site id in the parameters.")]
        public DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid listId, int itemId)
        {
            DateTime dt = DateTime.MinValue;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ID", itemId);
            string commandText = @"SELECT tp_Modified FROM AllUserData WITH(NOLOCK) WHERE tp_ListId=@ListId AND (tp_DeleteTransactionId = 0x Or tp_DeleteTransactionId <> 0x) AND tp_IsCurrentVersion=1 AND tp_ID=@ID and tp_IsCurrent=1";
            object o = mQueryWorker.ExecuteScalar(commandText);
            if (o != null)
            {
                dt = (DateTime)o;
            }
            return dt;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public DateTime CheckItemIdAvailableAndGetModifiedTimeForAppend(Guid siteId, Guid listId, int itemId)
        {
            DateTime dt = DateTime.MinValue;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@ID", itemId);
            string commandText = @"SELECT tp_Modified FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId = 0x Or tp_DeleteTransactionId <> 0x) AND tp_IsCurrentVersion=1 AND tp_ID=@ID and tp_IsCurrent=1";
            object o = mQueryWorker.ExecuteScalar(commandText);
            if (o != null)
            {
                dt = (DateTime)o;
            }
            return dt;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        [QueryReview("Item-004")]
        public void UpdateWorkflowStatusFieldValue(Guid siteId, Guid listId, Guid tpGuid, int tpId, byte[] StatusFieldValue, short rowOrdinal, string statusField)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ListId", listId);
            mQueryWorker.AddParameter("@tpGuid", tpGuid);
            mQueryWorker.AddParameter("@tpId", tpId);
            mQueryWorker.AddParameter("@rowOrdinal", rowOrdinal);
            mQueryWorker.AddParameter("@StatusFieldValue", StatusFieldValue);
            string commandText = @"UPDATE AllUserData Set " + statusField + "=@StatusFieldValue WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_ListId=@ListId AND tp_GUID=@tpGuid AND tp_ID=@tpId AND tp_RowOrdinal=@rowOrdinal";
            mQueryWorker.ExecuteNonQuery(commandText);
        }

        public string GetItemContentTypeId(Guid siteId, Guid parentId, Guid docId, int itemVersion)
        {
            string contentTypeId = string.Empty;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@DocId", docId);
            mQueryWorker.AddParameter("@Version", itemVersion);
            string cmdText = @"
            SELECT tp_contenttypeId FROM AllUserData WITH (NOLOCK)
            WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=1 or tp_IsCurrentVersion=0) AND tp_ParentId=@ParentId AND tp_DocId=@DocId AND tp_UIVersion=@Version";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(0))
                        {
                            contentTypeId = AveConvert.ConvertByteToContentTypeId((byte[])dr.GetValue(0));
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
            return contentTypeId;
        }

        public long GetNativeContentSize(AveBaseItemInfo mBaseItemInfo)
        {
            try
            {
                string cmdText = @"select sum(size) from DocStreams WITH(NOLOCK) where SiteId=@SiteId and DocId = @DocId and RbsId is null";
                mQueryWorker.ClearParameters();

                mQueryWorker.AddParameter("@SiteId", mBaseItemInfo.SiteId);
                mQueryWorker.AddParameter("@DocId", mBaseItemInfo.GUID);

                return (int)mQueryWorker.ExecuteScalar(cmdText);
            }
            //暂时这么处理一下
            catch (InvalidCastException)
            {
                return 0;
            }
        }

        public string GetDocIdUrl(string docDirName, string docLeafName, Guid siteId)
        {
            throw new NotImplementedException();
        }


        public bool DoesUserHasEnoughPermission()
        {
            mQueryWorker.ClearParameters();
            try
            {
                const string dboQueryCmd = @"select IS_ROLEMEMBER('db_owner')";

                var result = (int)mQueryWorker.ExecuteScalar(dboQueryCmd);

                return result == 1;
            }
            catch (SqlException queryException)
            {
                logger.Warn("cannot get permission result:{0}", new AveQueryException(queryException));
            }
            catch (Exception e)
            {
                logger.Warn("cannot get permission result:{0}", e);
            }
            return false;
        }

        public Dictionary<string, Dictionary<string, Dictionary<int, string>>> GetContentTypeResource(Guid siteId, Guid webId, Guid listId)
        {
            var result = new Dictionary<string, Dictionary<string, Dictionary<int, string>>>();
            string queryCmd = @"Select ResourceName, LCID, NvarcharVal, NtextVal From Resources WITH(NOLOCK)
                                     Where SiteId = @SiteId And WebId = @WebId And ListId = @ListId And(ResourceName Like '_CTDesc%' Or ResourceName Like '_CTName%')";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            mQueryWorker.AddParameter("@ListId", listId);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(queryCmd))
                {
                    while (dr.Read())
                    {
                        string resourceName = dr.GetString(0);
                        string realResourceName;
                        string contentTypeId;
                        int lcid = dr.GetInt32(1);
                        string value;
                        RetrieveUserResourceInfo(resourceName, out realResourceName, out contentTypeId);
                        if (!dr.IsDBNull(2))
                        {
                            value = dr.GetString(2);
                        }
                        else
                        {
                            value = dr.GetString(3);
                        }
                        if (!result.ContainsKey(contentTypeId))
                        {
                            result[contentTypeId] = new Dictionary<string, Dictionary<int, string>>();
                        }
                        if (!result[contentTypeId].ContainsKey(realResourceName))
                        {
                            result[contentTypeId][realResourceName] = new Dictionary<int, string>();
                        }
                        result[contentTypeId][realResourceName][lcid] = value;
                    }
                }
            }
            catch (SqlException queryException)
            {
                logger.Warn("cannot get contenttype resources. Error:{0}", new AveQueryException(queryException));
            }
            catch (Exception e)
            {
                logger.Warn("cannot get contenttype resources. Error:{0}", e);
            }
            return result;
        }
        private void RetrieveUserResourceInfo(string resourceName, out string realResourceName, out string contentTypeId)
        {
            realResourceName = string.Empty;
            contentTypeId = string.Empty;
            if (resourceName.StartsWith("_CTName", StringComparison.OrdinalIgnoreCase))
            {
                realResourceName = AveUserResourceConstants.TITLE_RESOUCE;
                contentTypeId = resourceName.Substring(7);
            }
            else if (resourceName.StartsWith("_CTDesc", StringComparison.OrdinalIgnoreCase))
            {
                realResourceName = AveUserResourceConstants.DESCRIPTION_RESOUCE;
                contentTypeId = resourceName.Substring(7);
            }
            else
            {
                logger.Warn("Unknow resource name. Name: {0}", resourceName);
            }
        }
    }
}
