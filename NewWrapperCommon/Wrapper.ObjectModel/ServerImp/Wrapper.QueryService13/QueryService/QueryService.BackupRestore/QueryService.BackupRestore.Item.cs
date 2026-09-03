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
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        [QueryReview("2012/11/21", "Austin Han", true, "Delete Dirty for SP2013")]
        private void InsertIntoAllDocs(AveBaseItemInfo info, int version)
        {
            string cmdText = @"SELECT [Id] ,[SiteId],[DirName]
      ,[LeafName],[WebId],[ListId] ,[DoclibRowId],[Type] ,[SortBehavior] ,[Size],[ETagVersion]
      ,[EffectiveVersion],[BumpVersion],[UIVersion]
      ,[ListDataDirty],[DocFlags],[ThicketFlag]
      ,[CharSet],[ProgId],[TimeCreated],[TimeLastModified] ,[NextToLastTimeModified]
      ,[MetaInfoTimeLastModified] ,[TimeLastWritten],[DeleteTransactionId]
      ,[SetupPathVersion],[SetupPath],[SetupPathUser] ,[CheckoutUserId]
      ,[CheckoutDate],[CheckoutExpires] ,[VersionCreatedSinceSTCheckout],[LTCheckoutUserId]
      ,[VirusVendorID],[VirusStatus],[VirusInfo],[MetaInfo],[MetaInfoSize]
      ,[MetaInfoVersion],[UnVersionedMetaInfo] ,[UnVersionedMetaInfoSize]
      ,[UnVersionedMetaInfoVersion],[WelcomePageUrl] ,[WelcomePageParameters]
      ,[IsCurrentVersion],[Level] ,[AuditFlags] ,[InheritAuditFlags]
      ,[DraftOwnerId],[UIVersionString] ,[ParentId] ,[HasStream]
      ,[ScopeId],[BuildDependencySet] ,[ParentVersion]
      ,[ParentVersionString],[TransformerId],[ParentLeafName]
      ,[IsCheckoutToLocal],[CtoOffset],[Extension] ,[ExtensionForFile] ,[ItemChildCount]
      ,[FolderChildCount],[FileFormatMetaInfo] ,[FileFormatMetaInfoSize]
      ,[FFMConsistent] ,[ContentVersion],[ListSchemaVersion] ,[ClientId]
FROM AllDocs WITH(NOLOCK)
WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@UIVersion", info.Version);
                mQueryWorker.Command.CommandText = cmdText;

                AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllDocs");
                manager.LoadColumnsInfo(null, mQueryWorker.Command);
                //List<string> computedColumns = new List<string>();
                //computedColumns.Add("ETagVersion");
                //computedColumns.Add("EffectiveVersion");
                //computedColumns.Add("LTCheckoutUserId");
                //computedColumns.Add("UIVersionString");
                //computedColumns.Add("HasStream");
                //computedColumns.Add("ParentVersionString");
                //computedColumns.Add("IsCheckoutToLocal");
                //computedColumns.Add("Extension");
                //computedColumns.Add("ExtensionForFile");
                //manager.AddComputedColumns(computedColumns);
                manager.ResetColumnValue("UIVersion", version);
                manager.ResetColumnValue("Level", 100);
                manager.ResetColumnValue("IsCurrentVersion", false);
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
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        [QueryReview("2012/12/11", "Austin Han", true, "add tp_SiteId to improve performance")]
        private void InsertIntoAllUserData(AveBaseItemInfo info, int version, bool isCurrentVersion)
        {
            string cmdText = @"select tp_RowOrdinal FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_Id=@RowId AND tp_UIVersion=@UIVersion order by tp_RowOrdinal ASC";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@RowId", info.RowId);
            mQueryWorker.AddParameter("@ListId", info.ListId);
            mQueryWorker.AddParameter("@UIVersion", info.Version);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            List<byte> rowOrdinals = new List<byte>();
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte rowOrdinal = dr.GetByte(0);
                        if (!rowOrdinals.Contains(rowOrdinal))
                        {
                            rowOrdinals.Add(rowOrdinal);
                        }
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
            foreach (byte row in rowOrdinals)
            {

                cmdText = @"SELECT [tp_ID],[tp_ListId],[tp_SiteId],[tp_RowOrdinal]
      ,[tp_Version],[tp_Author],[tp_Editor] ,[tp_Modified] ,[tp_Created]
      ,[tp_Ordering],[tp_ThreadIndex],[tp_HasAttachment],[tp_ModerationStatus]
      ,[tp_IsCurrent],[tp_ItemOrder],[tp_InstanceID],[tp_GUID] ,[tp_CopySource]
      ,[tp_HasCopyDestinations] ,[tp_AuditFlags],[tp_InheritAuditFlags]
      ,[tp_Size],[tp_WorkflowVersion],[tp_WorkflowInstanceID],[tp_ParentId]
      ,[tp_DocId],[tp_DeleteTransactionId],[tp_ContentTypeId]
      ,[tp_Level] ,[tp_IsCurrentVersion],[tp_UIVersion] ,[tp_CalculatedVersion]
      ,[tp_UIVersionString],[tp_DraftOwnerId] ,[tp_CheckoutUserId]
FROM AllUserData WITH(NOLOCK)
WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_Id=@RowId AND tp_UIVersion=@UIVersion AND tp_RowOrdinal=@rowOrdinal";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@RowId", info.RowId);
                mQueryWorker.AddParameter("@ListId", info.ListId);
                mQueryWorker.AddParameter("@UIVersion", info.Version);
                mQueryWorker.AddParameter("@rowOrdinal", row);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                try
                {
                    mQueryWorker.Command.CommandText = cmdText;

                    AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllUserData");
                    manager.LoadColumnsInfo(null, mQueryWorker.Command);
                    manager.ResetColumnValue("tp_UIVersion", version);
                    manager.ResetColumnValue("tp_IsCurrent", false);
                    manager.ResetColumnValue("tp_Level", 100);
                    manager.ResetColumnValue("tp_CheckoutUserId", null);
                    if (isCurrentVersion)
                    {
                        manager.ResetColumnValue("tp_CalculatedVersion", 0);
                        if (info.IsCheckOut)
                        {
                            // NET-5797 还原checkou user有问题
                            //manager.ResetColumnValue("tp_CheckoutUserId", version);
                        }
                    }
                    else
                    {
                        manager.ResetColumnValue("tp_CalculatedVersion", version);
                        manager.ResetColumnValue("tp_IsCurrentVersion", false);
                    }
                    //List<string> computedColumns = new List<string>();
                    //computedColumns.Add("tp_UIVersionString");
                    //manager.AddComputedColumns(computedColumns);
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
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        [QueryReview("2012/12/11", "Austin Han")]
        [QueryReview("Item-020")]
        private void UpdateAllDocs(AveBaseItemInfo info, Dictionary<string, object> allDocsData, int version, bool resetValue)
        {
            //只有IsSPInstalled 才能进入到这个Dll中执行;
            //if (!AveEnvironment.IsSPInstalled) { return; }
            AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllDocs");
            List<string> unUpdateColumns = new List<string>();
            List<string> needUpdateColums = new List<string>();
            needUpdateColums.Add("LeafName");
            needUpdateColums.Add("Size");
            needUpdateColums.Add("TimeCreated");
            needUpdateColums.Add("TimeLastModified");
            needUpdateColums.Add("NextToLastTimeModified");
            needUpdateColums.Add("MetaInfoTimeLastModified");
            needUpdateColums.Add("TimeLastWritten");
            needUpdateColums.Add("CheckoutDate");
            //if (!resetValue)
            //{
            //    needUpdateColums.Add("IsCurrentVersion");
            //}
            needUpdateColums.Add("Level");
            needUpdateColums.Add("CheckinComment");
            Dictionary<string, object> needUpdateDocData = new Dictionary<string, object>();
            foreach (string colum in needUpdateColums)
            {
                if (allDocsData.ContainsKey(colum))
                {
                    needUpdateDocData[colum] = allDocsData[colum];
                }
            }

            string whereClause = string.Empty;
            if (needUpdateDocData.Count > 0)
            {
                whereClause = ",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";
            }
            else
            {
                whereClause = "DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";
            }
            try
            {
                manager.MakeUpdateCommand(mQueryWorker.Command, needUpdateDocData, unUpdateColumns, whereClause);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Version", version);

                mQueryWorker.Command.ExecuteNonQuery();
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        [QueryReview("2012/04/26", "Sid You")]
        private void SetConflictInfo(RestoringDto restoringDto, SqlDataReader dr)
        {
            if (!dr.IsDBNull(1) && !dr.IsDBNull(2))
            {
                int rowId = dr.GetInt32(1);
                int level = dr.GetByte(2);
                int uiVersion = dr.GetInt32(3);
                if (level == 1)
                {
                    restoringDto.PublishingUIVersion = uiVersion;
                }
                else if (level == 2)
                {
                    restoringDto.DraftUIVersion = uiVersion;
                }
                restoringDto.ConflictRowId = rowId;
            }
            else
            {
                restoringDto.PublishingUIVersion = dr.GetInt32(3);
            }
        }

        [QueryReview("2012/12/11", "Austin Han")]
        [QueryReview("Item-020")]
        private void UpdateAllUserData(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allUserData, int version, bool isVersion)
        {
            if (restoringDto.TargetTable == RestoreTargetTable.AllDocVersions)
            {
                if (allUserData.ContainsKey("tp_Level") && !allUserData["tp_Level"].ToString().Equals("1"))
                {
                    allUserData["tp_Level"] = 2;
                }
                if (allUserData.ContainsKey("tp_IsCurrentVersion"))
                {
                    allUserData["tp_IsCurrentVersion"] = false;
                }
                if (allUserData.ContainsKey("tp_IsCurrent"))
                {
                    allUserData["tp_IsCurrent"] = false;
                }
            }
            Dictionary<string, object> sharedData = new Dictionary<string, object>();
            Dictionary<byte, Dictionary<string, object>> rowData = new Dictionary<byte, Dictionary<string, object>>();
            foreach (string key in allUserData.Keys)
            {
                if (allUserData[key] is KeyValuePair<byte, object>)
                {
                    string colName = key.Substring(0, key.LastIndexOf('#'));
                    KeyValuePair<byte, object> tempValue = (KeyValuePair<byte, object>)allUserData[key];
                    byte row = tempValue.Key;
                    object value = tempValue.Value;
                    if (!rowData.ContainsKey(row))
                    {
                        rowData[row] = new Dictionary<string, object>();
                    }
                    rowData[row].Add(colName, tempValue.Value);
                }
                else
                {
                    sharedData.Add(key, allUserData[key]);
                }
            }
            foreach (string key in sharedData.Keys)
            {
                foreach (Dictionary<string, object> rowValue in rowData.Values)
                {
                    rowValue.Add(key, sharedData[key]);
                }
            }
            AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllUserData");
            List<string> unUpdateColumns = new List<string>();
            unUpdateColumns.Add("tp_ID");
            unUpdateColumns.Add("tp_SiteId");
            unUpdateColumns.Add("tp_ListId");
            unUpdateColumns.Add("tp_RowOrdinal");
            unUpdateColumns.Add("tp_Version");
            //unUpdateColumns.Add("tp_ItemOrder");注释这行代码是因为发现tp_itemOrder会影响ADO-14176中page页上TabwebPart上的顺序，所以还原这个属性。
            unUpdateColumns.Add("tp_ContentTypeId");
            unUpdateColumns.Add("tp_UIVersion");
            unUpdateColumns.Add("tp_CalculatedVersion");
            unUpdateColumns.Add("tp_UIVersionString");
            //unUpdateColumns.Add("tp_CheckoutUserId");
            unUpdateColumns.Add("tp_DocId");
            unUpdateColumns.Add("tp_GUID");

            foreach (Dictionary<string, object> userData in rowData.Values)
            {
                //string whereClause = "WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_Id=@RowId AND tp_UIVersion=@UIVersion";
                //mQueryWorker.AddParameter("@RowId", info.RowId);
                //mQueryWorker.AddParameter("@ListId", info.ListId);
                //mQueryWorker.AddParameter("@UIVersion", version);

                string whereClause = "WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@UIVersion";
                try
                {
                    manager.MakeUpdateCommand(mQueryWorker.Command, userData, unUpdateColumns, whereClause);
                    mQueryWorker.AddParameter("@SiteId", info.SiteId);
                    mQueryWorker.AddParameter("@ParentId", info.ParentId);
                    mQueryWorker.AddParameter("@Id", info.GUID);
                    mQueryWorker.AddParameter("@UIVersion", version);
                    mQueryWorker.Command.ExecuteNonQuery();
                }
                catch (SqlException queryException)
                {
                    throw new AveQueryException(queryException);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        [QueryReview("2012/12/11", "Austin Han")]
        [QueryReview("Item-019")]
        private void UpdateUserDataUIVersion(Guid siteId, Guid parentId, Guid itemId, int oldVersion, int newVersion)
        {
            string cmdText = @"UPDATE AllDocs SET UIVersion = @NewVersion
WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@OldVersion";
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@OldVersion", oldVersion);
            mQueryWorker.AddParameter("@NewVersion", newVersion);

            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
        }

        [QueryReview("2012/12/11", "Austin Han")]
        [QueryReview("Item-019")]
        private void UpdateAllDocsUIVersion(Guid siteId, Guid parentId, Guid itemId, int oldVersion, int newVersion)
        {
            string cmdText = @"UPDATE AllUserData SET tp_UIVersion = @NewVersion
WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@OldVersion";
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            mQueryWorker.AddParameter("@Id", itemId);
            mQueryWorker.AddParameter("@OldVersion", oldVersion);
            mQueryWorker.AddParameter("@NewVersion", newVersion);

            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataAllRows);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info)
        {
            List<Dictionary<string, object>> data = GetUserData(info, string.Empty);
            return data;
        }

        /// <summary>
        /// 根据Info中提供的信息查询数据库AllUserData表中的一条完整记录
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info, string ColNameCollection)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetUserData"))
            {

                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                string cmdText =
              @"SELECT tp_ID,tp_RowOrdinal,tp_Version,tp_Author,tp_Editor,tp_Modified,tp_Created,tp_Ordering,tp_ThreadIndex,
                 tp_HasAttachment,tp_ModerationStatus,tp_IsCurrent,tp_ItemOrder,tp_InstanceID,tp_GUID,tp_CopySource,
                 tp_HasCopyDestinations,tp_AuditFlags,tp_InheritAuditFlags,tp_Size,tp_WorkflowVersion,tp_WorkflowInstanceID,
                 tp_ContentTypeId,tp_Level,tp_IsCurrentVersion,tp_UIVersion,tp_UIVersionString,tp_CalculatedVersion,tp_DraftOwnerId,tp_CheckoutUserId,tp_AppAuthor,tp_AppEditor" + ColNameCollection +
                @" FROM  AllUserData WITH(NOLOCK)
                 WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND
                 tp_ParentId = @ParentId AND tp_DocId = @DocId AND tp_UIVersion = @Version";

                //WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_ID=@RowId 
                //AND (tp_CalculatedVersion = 0 OR tp_CalculatedVersion =@Version) AND (tp_Level = 1 OR tp_Level =2 OR  tp_Level =255 ) AND tp_UIVersion = @Version";
                try
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@Version", info.Version);
                    mQueryWorker.AddParameter("@SiteId", info.SiteId);
                    mQueryWorker.AddParameter("@ParentId", info.ParentId);
                    mQueryWorker.AddParameter("@DocId", info.GUID);
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, object> tempData = new Dictionary<string, object>();
                            AveQueryUtility.GetDBRow(tempData, dr);
                            data.Add(tempData);
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

                return data;

            }

        }

        /// <summary>
        /// 获取Item/Document的Author
        /// user被删除时，无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetFileAuthorIdByNative(Guid siteId, Guid parentId, Guid docId)
        {
            string cmdText = "SELECT tp_Author FROM AllUserData WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_parentid=@ParentId AND tp_DocId=@DocId";
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public List<Dictionary<string, object>> GetVersionAndUserInfo(Guid siteId, Guid parentId, int currentDocLibRowId, int count, string colNameCollection)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetVersionAndUserInfo"))
            {

                var result = new List<Dictionary<string, object>>(count);
                colNameCollection = ProcessColNameCollection(colNameCollection);
                string cmdText = @"SELECT TOP {0}
doc.DoclibRowId AS DOC#DoclibRowId,
docver.UIVersion as VER#UIVersion,docver.InternalVersion as VER#InternalVersion,docver.TimeCreated as VER#TimeCreated,docver.DocFlags as VER#DocFlags,docver.MetaInfoSize as VER#MetaInfoSize,docver.Size as VER#Size,docver.MetaInfo as VER#MetaInfo,docver.CheckinComment as VER#CheckinComment,docver.Level as VER#Level,docver.DraftOwnerId as VER#DraftOwnerId,docver.DeleteTransactionId as VER#DeleteTransactionId,docver.VirusVendorID as VER#VirusVendorID,docver.VirusStatus as VER#VirusStatus,docver.VirusInfo as VER#VirusInfo,
data.tp_ID as UD#tp_ID,data.tp_RowOrdinal as UD#tp_RowOrdinal,data.tp_Version as UD#tp_Version,data.tp_Author as UD#tp_Author,data.tp_Editor as UD#tp_Editor,data.tp_Modified as UD#tp_Modified,data.tp_Created as UD#tp_Created,data.tp_Ordering as UD#tp_Ordering,data.tp_ThreadIndex as UD#tp_ThreadIndex,data.tp_HasAttachment as UD#tp_HasAttachment,data.tp_ModerationStatus as UD#tp_ModerationStatus,data.tp_IsCurrent as UD#tp_IsCurrent,data.tp_ItemOrder as UD#tp_ItemOrder,data.tp_InstanceID as UD#tp_InstanceID,data.tp_GUID as UD#tp_GUID,data.tp_CopySource as UD#tp_CopySource,data.tp_HasCopyDestinations as UD#tp_HasCopyDestinations,data.tp_AuditFlags as UD#tp_AuditFlags,data.tp_InheritAuditFlags as UD#tp_InheritAuditFlags,data.tp_Size as UD#tp_Size,data.tp_WorkflowVersion as UD#tp_WorkflowVersion,data.tp_WorkflowInstanceID as UD#tp_WorkflowInstanceID,data.tp_ContentTypeId as UD#tp_ContentTypeId," + colNameCollection + @"data.tp_Level as UD#tp_Level,data.tp_IsCurrentVersion as UD#tp_IsCurrentVersion,data.tp_UIVersion as UD#tp_UIVersion,data.tp_CalculatedVersion as UD#tp_CalculatedVersion,data.tp_DraftOwnerId as UD#tp_DraftOwnerId,data.tp_CheckoutUserId as UD#tp_CheckoutUserId
FROM AllDocVersions AS docver WITH(NOLOCK) 
INNER JOIN AllDocs AS doc WITH(NOLOCK) ON docver.SiteId=doc.SiteId AND docver.Id=doc.Id AND docver.DeleteTransactionId=0x AND doc.IsCurrentVersion=1 
LEFT JOIN AllUserData AS data WITH(NOLOCK) ON data.tp_SiteId=doc.SiteId AND data.tp_DeleteTransactionId=0x AND data.tp_IsCurrentVersion=0 
AND data.tp_ParentId=doc.ParentId AND data.tp_DocId=docver.Id AND data.tp_CalculatedVersion=docver.UIVersion and data.tp_Level=docver.Level
WHERE doc.SiteId = @SiteId AND doc.ParentId = @ParentId AND doc.DeleteTransactionId = 0x AND
doc.Type = 0 AND doc.DoclibRowId >= @CurrentDoclibRowId
ORDER BY doc.DoclibRowId";

                cmdText = string.Format(cmdText, count);

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ParentID", parentId);
                mQueryWorker.AddParameter("@CurrentDoclibRowId", currentDocLibRowId);
                try
                {
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, object> tempData = new Dictionary<string, object>();
                            AveQueryUtility.GetDBRow(tempData, dr);
                            result.Add(tempData);
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

        /// <summary>
        /// 根据internalVersion和GUID查询Document是否有Stream
        /// 无API实现
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin", true, "Remove internal version for SP2013")]
        public bool GetDocHasStream(AveBaseItemInfo itemInfo, int internalVersion)
        {
            //string cmdText = @"select COUNT(DocId) from DocStreams WITH(NOLOCK) where SiteId=@SiteId and DocId=@Id and InternalVersion=@internalVersion";
            //mQueryWorker.ClearParameters();
            //mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
            //mQueryWorker.AddParameter("@Id", itemInfo.GUID);
            //mQueryWorker.AddParameter("@internalVersion", internalVersion);
            //return ((int)mQueryWorker.ExecuteScalar(cmdText) > 0);
            string cmdText = @"SELECT HasStream FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND DeleteTransactionId=0X AND ParentId=@ParentId AND Id=@Id AND Level=@Level";
            mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
            mQueryWorker.AddParameter("@ParentId", itemInfo.ParentId);
            mQueryWorker.AddParameter("@Id", itemInfo.GUID);
            mQueryWorker.AddParameter("@Level", itemInfo.Level);
            mQueryWorker.AddParameter("@InternalVersion", internalVersion);

            object value = mQueryWorker.ExecuteScalar(cmdText);
            if (value == null)
            {
                cmdText = @"SELECT HasStream FROM AllDocVersions WHERE SiteId=@SiteId AND Id=@Id";
                //mQueryWorker.AddParameter("@UIVersion", itemInfo.);
                value = mQueryWorker.ExecuteScalar(cmdText);
            }
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// 获取Document Current Version下的所有信息.
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="itemId"></param>
        /// <param name="dataCache"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public Dictionary<string, object> GetCurrentVersionDocInfo(Guid siteId, Guid parentId, Guid itemId)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetCurrentVersionDocInfo"))
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
                 ListSchemaVersion,ClientId,InternalVersion,BumpVersion
        FROM AllDocs WITH(NOLOCK)
        WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND";
                if (parentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id AND IsCurrentVersion=1 AND (Level=1 OR Level=2 OR Level=255)";

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                if (parentId != Guid.Empty)
                {
                    mQueryWorker.AddParameter("@ParentID", parentId);
                }
                mQueryWorker.AddParameter("@Id", itemId);
                Dictionary<string, object> dataCache = new Dictionary<string, object>();
                AveQueryUtility.TryGetDBRow(dataCache, mQueryWorker, cmdText);
                return dataCache;

            }

        }

        /// <summary>
        /// 读取Internal Version下的Document的内容
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="Id"></param>
        /// <param name="internalVersion"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han", true, "We still need to adjust this method for SP2013")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public IAveQueryDataReader ExportContentByNative(AveBaseItemInfo info, int internalVersion)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.ExportContentByNative"))
            {
                string cmdText = @"SELECT Content FROM DocsToStreams dts, DocStreams ds WITH(NOLOCK) WHERE dts.SiteId=ds.SiteId AND dts.DocId=ds.DocId AND dts.Level=@Level AND
 dts.DocId=@Id AND dts.SiteId=@SiteId AND dts.BSN=ds.BSN AND dts.Partition = ds.Partition ";
                try
                {
                    mQueryWorker.ClearParameters();
                    if (info.IsVersion)
                    {
                        cmdText += " AND dts.HistVersion = @UIVersion";
                        mQueryWorker.AddParameter("@UIVersion", info.Version);
                    }
                    else
                    {
                        cmdText += " AND dts.HistVersion = 0";
                    }
                    mQueryWorker.AddParameter("@SiteId", info.SiteId);
                    mQueryWorker.AddParameter("@Id", info.GUID);
                    mQueryWorker.AddParameter("@Level", info.Level);
                    return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText, CommandBehavior.SequentialAccess));
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

        /// <summary>
        /// 用来统一使用Native方法更改AllDocs
        /// </summary>
        /// <param name="info">SiteId,ParentId,Level,</param>
        /// <param name="uniqueId"></param>
        /// <param name="docdataObjects"></param>
        [QueryReview("2014/02/26", "Cheng Cui", true, "Use Clusterd Index [tp_SiteId],[tp_DeleteTransactionId],[tp_IsCurrentVersion],[tp_ParentId],[tp_DocId],[tp_CalculatedVersion],[tp_Level],[tp_RowOrdinal]")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "userdata")]
        public void ChangeUserdataByNative(AveBaseItemInfo info, Guid uniqueId, Dictionary<string, object> userdata)
        {
            try
            {
                if (userdata.Count == 0)
                {
                    return;
                }

                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ID", uniqueId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Level", info.Level);

                var stringBuilder = new StringBuilder();
                foreach (var listColumn in userdata)
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

                string cmdText = string.Format(@"UPDATE AllUserData  SET {0} WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocId=@ID AND tp_CalculatedVersion=0 AND tp_Level=@Level", stringBuilder);
                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred when change userdata. file:{0}, Reason:{1}.", info.Name, e);
            }
        }
        
        /// <summary>
        /// 用来统一使用Native方法更改AllDocs
        /// </summary>
        /// <param name="info">SiteId,ParentId,Level,UnVersionedMetaInfo,Name需要初始化</param>
        /// <param name="uniqueId"></param>
        /// <param name="docdataObjects"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docData")]
        public void ChangeDocdataByNative(AveBaseItemInfo info, Guid uniqueId, Dictionary<string, object> docdataObjects)
        {
            try
            {
                if (docdataObjects.Count == 0)
                {
                    return;
                }
                mQueryWorker.ClearParameters();
                var stringBuilder = new StringBuilder();
                foreach (var listColumn in docdataObjects)
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(",");
                    }
                    stringBuilder.Append(listColumn.Key);
                    stringBuilder.Append("=@");
                    stringBuilder.Append(listColumn.Key);

                    if (string.Equals(listColumn.Key, "UnVersionedMetaInfo", StringComparison.OrdinalIgnoreCase) && info.UnVersionedMetaInfo == null)
                    {
                        mQueryWorker.Command.Parameters.Add("@UnVersionedMetaInfo", SqlDbType.VarBinary, -1);
                        mQueryWorker.Command.Parameters["@UnVersionedMetaInfo"].Value = DBNull.Value;
                        continue;
                    }
                    mQueryWorker.AddParameter("@" + listColumn.Key, listColumn.Value);
                }

                string cmdText = string.Format(@"UPDATE AllDocs SET {0} WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@Id AND Level=@Level", stringBuilder);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Id", uniqueId);
                mQueryWorker.AddParameter("@Level", info.Level);

                mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred when change docdata. file:{0}, Reason:{1}.", info.Name, e);
            }
        }
    }
}
