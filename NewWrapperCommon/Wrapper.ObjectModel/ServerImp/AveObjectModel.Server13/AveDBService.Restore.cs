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
using System.Text;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using AvePoint.Common;
using System.Collections;
using System.IO;


namespace AvePoint.ObjectModel.Server13
{
    internal partial class AveDBQueryService : AveDBServiceBase
    {

        internal int GetTpIdByTpGuid(Guid tp_guid, Guid listId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetTpIdByTpGuid"))
            {
#endif
                int tp_id = 0;
                try
                {
                    SqlConn.ClearParameters();
                    string commadText = @"SELECT max(tp_ID) from AllUserData 
                                        WHERE tp_ListId=@tp_listid 
                                        AND tp_GUID=@tp_guid;";

                    SqlConn.AddParameter("@tp_listid", listId);
                    SqlConn.AddParameter("@tp_guid", tp_guid);

                    tp_id = (int)SqlConn.ExecuteScalar(commadText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTListIte554", tp_guid, listId, e);
                    //mLog.Warn(string.Format("An error occured when getting tp_id by tp_guid:{0}, listid:{1}, error:{2}.", tp_guid, listId, e.ToString()));
                }
                return tp_id;
#if PerformanceLog
            }
#endif
        }

        internal void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.InsertIntoAllUserDatajunction"))
            {
#endif
                string cmdText = @"select tp_SiteId,tp_DeleteTransactionId,tp_IsCurrentVersion,tp_ParentId,
            tp_DocId,tp_CalculatedVersion,tp_Level,tp_UIVersion from AllUserData where tp_ID=@rowId and tp_DocId=@docId and tp_UIVersion=@UIVersion";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@rowId", item.ID);
                SqlConn.AddParameter("@docId", item.UniqueId);
                SqlConn.AddParameter("@UIVersion", version);
                SqlConn.Command.CommandText = cmdText;

                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllUserDataJunctions");
                manager.LoadColumnsInfo(null, SqlConn.Command);
                manager.ResetColumnValue("tp_FieldId", fieldId);
                manager.ResetColumnValue("tp_SourceListId", sourceListId);
                manager.ResetColumnValue("tp_Id", id);
                manager.ResetColumnValue("tp_Ordinal", ordinal);
                manager.MakeInsertCommand(SqlConn.Command);

                if (SqlConn.Command.Parameters.Count > 0)
                {
                    SqlConn.Command.ExecuteNonQuery();
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateColumnByNative"))
            {
#endif
                string cmdText = @"update AllUserData set " + colName + "=@colValue where tp_SiteId=@siteId and tp_DocId=@docId and tp_UIVersion=@UIVersion and tp_RowOrdinal =@rowOrdinal";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@siteId", siteId);
                SqlConn.AddParameter("@colValue", colValue);
                SqlConn.AddParameter("@docId", item.UniqueId);
                SqlConn.AddParameter("@UIVersion", version);
                SqlConn.AddParameter("@rowOrdinal", rowOrdinal);
                SqlConn.Command.CommandText = cmdText;
                SqlConn.Command.ExecuteNonQuery();
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// remove item by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="spSite"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        internal void RemoveListItemInRecycleBin(SPSite spSite, Guid parentId, Guid tp_Guid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.RemoveListItemInRecycleBin"))
            {
#endif
                try
                {
                    SPRecycleBinItemCollection recycleBin = spSite.RecycleBin;

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", spSite.ID);
                    SqlConn.AddParameter("@ParentId", parentId);
                    SqlConn.AddParameter("@TP_GUID", tp_Guid);

                    const string cmdText = @"SELECT Distinct tp_DeleteTransactionId FROM ALLUserData
                                         WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_GUID=@TP_GUID AND tp_DeleteTransactionId<>0x;";

                    using (SqlDataReader sr = SqlConn.ExecuteReader(cmdText))
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
                                //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1714", tp_Guid, e);
                                //mLog.Warn(e, "An error occurred while deleting a item in recycle bin. tp_Guid:{0}", tp_Guid);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1722", tp_Guid, e);
                    //mLog.Warn(e, "An error occurred while deleting items in recycle bin. tp_Guid:{0}", tp_Guid);
                }
#if PerformanceLog
            }
#endif
        }

        internal void RemoveItemInRecycleBin(SPSite spSite, Guid parentId, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.RemoveItemInRecycleBin"))
            {
#endif
                try
                {
                    SPRecycleBinItemCollection recycleBin = spSite.RecycleBin;

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", spSite.ID);
                    SqlConn.AddParameter("@ParentId", parentId);
                    SqlConn.AddParameter("@LeafName", name);

                    const string cmdText = "SELECT Distinct DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId<>0x";

                    using (SqlDataReader sr = SqlConn.ExecuteReader(cmdText))
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
                                //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1667", name, e);
                                //mLog.Warn(e, "An error occurred while deleting a item in recycle bin. Name:{0}", name);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1675", name, e);
                    //mLog.Warn(e, "An error occurred while deleting items in recycle bin. Name:{0}", name);
                }
#if PerformanceLog
            }
#endif
        }

        //修改Alldocs中对应的TimeCreated和TimeLastModified字段。
        internal void UpdateAllDocsPropertyByNative(AveBaseItemInfo info, DateTime timeCreated, DateTime timeLastModified, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateAllDocsPropertyByNative"))
            {
#endif
                try
                {
                    if (timeCreated != DateTime.MinValue && timeLastModified != DateTime.MinValue)
                    {
                        string cmdStr = "UPDATE AllDocs SET TimeCreated=@TimeCreated , TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                        SqlConn.ClearParameters();
                        SqlConn.AddParameter("@TimeCreated", timeCreated);
                        SqlConn.AddParameter("@TimeLastModified", timeLastModified);
                        SqlConn.AddParameter("@SiteId", info.SiteId);
                        SqlConn.AddParameter("@ID", info.GUID);
                        SqlConn.AddParameter("@UIVersion", info.Version);
                        SqlConn.ExecuteNonQuery(cmdStr);
                    }
                    else if (timeCreated != DateTime.MinValue)
                    {
                        string cmdStr = "UPDATE AllDocVersions SET TimeCreated=@TimeCreated WHERE SiteId=@SiteId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                        SqlConn.ClearParameters();
                        SqlConn.AddParameter("@TimeCreated", timeCreated);
                        SqlConn.AddParameter("@SiteId", info.SiteId);
                        SqlConn.AddParameter("@ID", info.GUID);
                        SqlConn.AddParameter("@UIVersion", version);
                        SqlConn.ExecuteNonQuery(cmdStr);
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1051", SPListItem.Url, SPListItem.UniqueId, e);
                    //mLog.Warn(e, "An error occurred while updating an item SpecialProperty. Url:{0}, Id:{1}", SPListItem.Url, SPListItem.UniqueId);
                }
#if PerformanceLog
            }
#endif
        }


        internal bool CreateVersionByNative(AveBaseItemInfo info, int version, RestoringDto restoringDto)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.CreateVersionByNative"))
            {
#endif
                try
                {
                    bool needInsertToAllDocVersions = false;
                    bool needInsertToAllDocs = false;
                    bool needInsertToAllUserData = false;
                    string selectCmdText = null;
                    string updateCmdText = null;
                    string logId = null;
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@Id", info.GUID);
                    SqlConn.AddParameter("@Version", version);
                    SqlConn.AddParameter("@ListId", info.ListId);
                    SqlConn.AddParameter("@RowId", info.RowId);
                    SqlConn.AddParameter("@ParentId", info.ParentId);

                    selectCmdText = "SELECT DeleteTransactionId FROM AllDocVersions WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                    updateCmdText = "UPDATE AllDocVersions Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                    logId = "WP10RTSPItem2009";
                    if (!CheckExistingRecord(selectCmdText, updateCmdText, logId, version, restoringDto.OverWrite, false, ref needInsertToAllDocVersions))
                    {
                        // Conflict version and not overwrite
                        return false;
                    }
                    if (needInsertToAllDocVersions && restoringDto.TargetTable == RestoreTargetTable.AllDocs)
                    {
                        needInsertToAllDocVersions = false;
                    }

                    selectCmdText = "SELECT DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                    updateCmdText = "UPDATE AllDocs Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                    logId = "WP10RTSPItem2030";
                    if (!CheckExistingRecord(selectCmdText, updateCmdText, logId, version, restoringDto.OverWrite, false, ref needInsertToAllDocs))
                    {
                        // Conflict version and not overwrite
                        return false;
                    }
                    if (needInsertToAllDocs && restoringDto.TargetTable == RestoreTargetTable.AllDocVersions)
                    {
                        needInsertToAllDocs = false;
                    }
                    // SharePoint的一个bug: 删除文件后，在AllUserData里面仍然有记录
                    //而且只要AllDocs或者AllDocVersions就已经能判断是否Conflict, not overwrite，所以对于AllUserData，不需判断是否冲突
                    #region
                    selectCmdText = "SELECT tp_DeleteTransactionId FROM AllUserData WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";
                    updateCmdText = "UPDATE AllUserData Set tp_DeleteTransactionId=0x WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";
                    logId = "WP10RTSPItem2031";
                    if (!CheckExistingRecord(selectCmdText, updateCmdText, logId, version, restoringDto.OverWrite, true, ref needInsertToAllUserData))
                    {
                        //return false;
                    }
                    #endregion
                    if (needInsertToAllDocVersions)
                    {
                        InsertIntoAllDocVersions(info, version);
                    }
                    else if (needInsertToAllDocs)
                    {
                        InsertIntoAllDocs(info, version);
                    }
                    if (needInsertToAllUserData)
                    {
                        InsertIntoAllUserData(info, version, needInsertToAllDocs);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.ERROR, "WP10RTSPItem2075", version, e);
                    return false;
                }
#if PerformanceLog
            }
#endif
        }

        private bool CheckExistingRecord(string selectCmdText, string updateCmdText, string logId, int version, bool overWrite, bool isUserData, ref bool needInsert)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.CheckExistingRecord"))
            {
#endif
                using (SqlDataReader sr = SqlConn.ExecuteReader(selectCmdText))
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
                try
                {
                    SqlConn.ExecuteNonQuery(updateCmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, logId, mId, version, e);
                }
                return true;
#if PerformanceLog
            }
#endif
        }

        internal void InsertIntoAllDocVersions(AveBaseItemInfo info, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.InsertIntoAllDocVersions"))
            {
#endif
                string cmdText = @"
SELECT SiteId,Id,UIVersion,TimeCreated,DocFlags,MetaInfoSize,Size,Level,
       DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo,SetupPathVersion
FROM AllDocs
WHERE SiteId=@SiteId AND Id=@Id AND ParentId=@ParentId AND UIVersion=@UIVersion";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@UIVersion", info.Version);
                SqlConn.Command.CommandText = cmdText;

                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllDocVersions");
                manager.LoadColumnsInfo(null, SqlConn.Command);
                manager.ResetColumnValue("UIVersion", version);
                if (version % 512 == 0)
                {
                    manager.ResetColumnValue("Level", (byte)1);
                }
                else
                {
                    manager.ResetColumnValue("Level", (byte)2);
                }
                manager.MakeInsertCommand(SqlConn.Command);

                if (SqlConn.Command.Parameters.Count > 0)
                {
                    SqlConn.Command.ExecuteNonQuery();
                }
#if PerformanceLog
            }
#endif
        }

        internal void InsertIntoAllDocs(AveBaseItemInfo info, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.InsertIntoAllDocs"))
            {
#endif
                string cmdText = @"SELECT [Id] ,[SiteId],[DirName]
      ,[LeafName],[WebId],[ListId] ,[DoclibRowId],[Type] ,[SortBehavior] ,[Size],[ETagVersion]
      ,[EffectiveVersion],[BumpVersion],[UIVersion]
      ,[Dirty],[ListDataDirty],[CacheParseId] ,[DocFlags],[ThicketFlag]
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
FROM AllDocs
WHERE SiteId=@SiteId AND Id=@Id AND ParentId=@ParentId AND UIVersion=@UIVersion";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@UIVersion", info.Version);
                SqlConn.Command.CommandText = cmdText;

                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllDocs");
                manager.LoadColumnsInfo(null, SqlConn.Command);
                List<string> computedColumns = new List<string>();
                computedColumns.Add("ETagVersion");
                computedColumns.Add("EffectiveVersion");
                computedColumns.Add("LTCheckoutUserId");
                computedColumns.Add("UIVersionString");
                computedColumns.Add("HasStream");
                computedColumns.Add("ParentVersionString");
                computedColumns.Add("IsCheckoutToLocal");
                computedColumns.Add("Extension");
                computedColumns.Add("ExtensionForFile");
                manager.AddComputedColumns(computedColumns);
                manager.ResetColumnValue("UIVersion", version);
                manager.ResetColumnValue("Level", 100);
                manager.ResetColumnValue("IsCurrentVersion", false);
                manager.MakeInsertCommand(SqlConn.Command);

                if (SqlConn.Command.Parameters.Count > 0)
                {
                    SqlConn.Command.ExecuteNonQuery();
                }
#if PerformanceLog
            }
#endif
        }

        internal void InsertIntoAllUserData(AveBaseItemInfo info, int version, bool isCurrentVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.InsertIntoAllUserData"))
            {
#endif
                string cmdText = @"select tp_RowOrdinal FROM AllUserData 
WHERE tp_Id=@RowId AND tp_ListId=@ListId AND tp_UIVersion=@UIVersion order by tp_RowOrdinal ASC";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@RowId", info.RowId);
                SqlConn.AddParameter("@ListId", info.ListId);
                SqlConn.AddParameter("@UIVersion", info.Version);
                List<byte> rowOrdinals = new List<byte>();
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
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
FROM AllUserData 
WHERE tp_Id=@RowId AND tp_ListId=@ListId AND tp_UIVersion=@UIVersion AND tp_RowOrdinal=@rowOrdinal";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@RowId", info.RowId);
                    SqlConn.AddParameter("@ListId", info.ListId);
                    SqlConn.AddParameter("@UIVersion", info.Version);
                    SqlConn.AddParameter("@rowOrdinal", row);
                    SqlConn.Command.CommandText = cmdText;

                    AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllUserData");
                    manager.LoadColumnsInfo(null, SqlConn.Command);
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
                    List<string> computedColumns = new List<string>();
                    computedColumns.Add("tp_UIVersionString");
                    manager.AddComputedColumns(computedColumns);
                    manager.MakeInsertCommand(SqlConn.Command);

                    if (SqlConn.Command.Parameters.Count > 0)
                    {
                        SqlConn.Command.ExecuteNonQuery();
                    }
                }
#if PerformanceLog
            }
#endif
        }
        internal void ChangeNextItemId(int toId, Guid listId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeNextItemId"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@ToId", toId);
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
                SqlConn.ExecuteScalar(cmdText);
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// 
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
        /// <param name="sqlConn"></param>
        /// <returns></returns>
        internal int ChangeItemId(
            Guid siteId,
            Guid id,
            Guid rootFolderId,
            int itemType,
            int fromId,
            int toId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeItemId"))
            {
#endif
                if (fromId == toId)
                {
                    return 0;
                }

                //mLog.Log(AveLogLevel.DEBUG, "WP10RTeSPIte1349", fromId, toId);
                //mLog.Debug("Change Item id. FromId:{0}, ToId:{1}", fromId, toId);

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", id);
                SqlConn.AddParameter("@ItemType", itemType);
                SqlConn.AddParameter("@ToId", toId);
                SqlConn.AddParameter("@RootFolderId", rootFolderId);

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
FROM AllDocs
WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x

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
FROM AllUserData 
WHERE tp_SiteId=@SiteId 
AND tp_ListId=@ListId 
AND tp_ID=@ToId 
AND tp_DeleteTransactionId=0x
AND tp_IsCurrentVersion=1
)
BEGIN
  COMMIT TRAN
  SELECT -1
  RETURN
END

SELECT @NextId=NextAvailableId 
FROM AllListsAux WITH(UPDLOCK)
WHERE ListID=@ListId

IF @@ROWCOUNT <> 1
BEGIN
  ROLLBACK TRAN
  SELECT -102
  RETURN
END

IF @ToId>@FromId AND @ToId>=@NextId
BEGIN
  UPDATE AllListsAux SET NextAvailableId=@ToId+1 
  WHERE ListID=@ListId
  
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
FROM AllUserData
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

IF @HasAttachment=1
BEGIN
  DECLARE @AttachmentId uniqueidentifier
  SELECT @AttachmentId=Id FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@RootFolderId AND LeafName='Attachments' AND DeleteTransactionId=0x
  
  IF @@ROWCOUNT <> 0
  BEGIN
    SELECT @AttachmentId=Id FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@AttachmentId AND LeafName=CAST(@FromId AS NVARCHAR(128)) AND DeleteTransactionId=0x
    
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
                    returnCode = (int)SqlConn.ExecuteScalar(cmdText);
                    // returnCode=0, change sucessfully
                    // returnCode<0, change failed.
                    if (returnCode < 0)
                    {
                        //mLog.Log(AveLogLevel.DEBUG, "WP10RTSPItem1520", siteId, id, fromId, toId, itemType, returnCode);
                        //mLog.Debug("Cannot change item id. SiteId:{0}, Id:{1}, FromId:{2}, ToId:{3}, ItemType:{4}, ReturnCode:{5}",
                        //    siteId, id, fromId, toId, itemType, returnCode);
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem1527", siteId, id, fromId, toId, itemType, e);
                    //mLog.Warn(e, "Cannot change item id. SiteId:{0}, Id:{1}, FromId:{2}, ToId:{3}, ItemType:{4}",
                    //        siteId, id, fromId, toId, itemType);
                    returnCode = -1000;
                }
                return returnCode;
#if PerformanceLog
            }
#endif
        }

        //检查list下itemId是否被占用,没被占用返回true。
        internal bool CheckItemIdAvailable(Guid listId, int itemId)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@ListId", listId);
            SqlConn.AddParameter("@ID", itemId);
            SqlConn.Command.CommandText = @"SELECT count(tp_ID) FROM AllUserData WHERE tp_ListId=@ListId AND tp_ID=@ID";
            if ((int)SqlConn.Command.ExecuteScalar() > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void UpdateUserInfoByNative(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@UserListId", listId);
            SqlConn.AddParameter("@LoginName", old.Login);
            SqlConn.AddParameter("@Id", userId);
            SqlConn.AddParameter("@SystemId", old.SystemID);
            SqlConn.AddParameter("@Title", old.Title);

            SqlConn.Command.CommandText = "UPDATE UserInfo SET tp_SystemId=@SystemId,tp_Login=@LoginName,tp_Title=@Title WHERE tp_SiteId=@SiteId AND tp_Id=@Id " +
                                "UPDATE AllUserData SET " + displayField + "=@Title," + nameField + "=@LoginName WHERE tp_ListId=@UserListId AND tp_Id=@Id";

            SqlConn.Command.ExecuteNonQuery();
        }


        internal void UpdateVersionByNative(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, int version)
        {
            try
            {
                if (restoringDto.TargetTable == RestoreTargetTable.AllDocVersions)
                {
                    UpdateAllDocVersions(info, allDocData, version, !info.IsVersion);
                }
                else
                {
                    UpdateAllDocs(info, allDocData, version, info.IsVersion);
                }
                UpdateAllUserData(info, restoringDto, allUserData, version, info.IsVersion);
            }
            catch { }
        }

        internal void UpdateAllDocVersions(AveBaseItemInfo info, Dictionary<string, object> allDocVersions, int version, bool resetValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateAllDocVersions"))
            {
#endif
                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllDocVersions");
                List<string> unUpdateColumns = new List<string>();
                List<string> needUpdateColums = new List<string>();
                needUpdateColums.Add("TimeCreated");
                needUpdateColums.Add("Size");
                needUpdateColums.Add("CheckinComment");
                needUpdateColums.Add("Level");
                needUpdateColums.Add("VirusVendorID");
                needUpdateColums.Add("VirusStatus");
                needUpdateColums.Add("VirusInfo");
                if (resetValue)
                {
                    needUpdateColums.Add("DraftOwnerId");
                }

                Dictionary<string, object> needUpdateDocData = new Dictionary<string, object>();
                foreach (string colum in needUpdateColums)
                {
                    if (allDocVersions.ContainsKey(colum))
                    {
                        needUpdateDocData[colum] = allDocVersions[colum];
                    }
                }
                if (resetValue)
                {
                    if (version % 512 == 0)
                    {
                        needUpdateDocData["Level"] = 1;
                    }
                    else
                    {
                        needUpdateDocData["Level"] = 2;
                    }
                }
                string whereClause = string.Empty;

                if (needUpdateDocData.Count > 0)
                {
                    whereClause = ",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                }
                else
                {
                    whereClause = "DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                }

                manager.MakeUpdateCommand(SqlConn.Command, needUpdateDocData, unUpdateColumns, whereClause);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@Version", version);
                SqlConn.Command.ExecuteNonQuery();
#if PerformanceLog
            }
#endif
        }

        internal void UpdateAllDocs(AveBaseItemInfo info, Dictionary<string, object> allDocsData, int version, bool resetValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateAllDocs"))
            {
#endif
                //只有IsSPInstalled 才能进入到这个Dll中执行;
                //if (!AveEnvironment.IsSPInstalled) { return; }
                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllDocs");
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
                    whereClause = ",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                }
                else
                {
                    whereClause = "DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                }

                manager.MakeUpdateCommand(SqlConn.Command, needUpdateDocData, unUpdateColumns, whereClause);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@Version", version);
                SqlConn.Command.ExecuteNonQuery();
#if PerformanceLog
            }
#endif
        }

        internal void UpdateAllUserData(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allUserData, int version, bool isVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateAllUserData"))
            {
#endif
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
                AveSqlColumnInfoManager manager = new AveSqlColumnInfoManager("AllUserData");
                List<string> unUpdateColumns = new List<string>();
                unUpdateColumns.Add("tp_ID");
                unUpdateColumns.Add("tp_SiteId");
                unUpdateColumns.Add("tp_ListId");
                unUpdateColumns.Add("tp_RowOrdinal");
                unUpdateColumns.Add("tp_Version");
                unUpdateColumns.Add("tp_ItemOrder");
                unUpdateColumns.Add("tp_ContentTypeId");
                unUpdateColumns.Add("tp_UIVersion");
                unUpdateColumns.Add("tp_CalculatedVersion");
                unUpdateColumns.Add("tp_UIVersionString");
                //unUpdateColumns.Add("tp_CheckoutUserId");
                unUpdateColumns.Add("tp_DocId");
                unUpdateColumns.Add("tp_GUID");

                foreach (Dictionary<string, object> userData in rowData.Values)
                {
                    string whereClause = "WHERE tp_Id=@RowId AND tp_ListId=@ListId AND tp_UIVersion=@UIVersion";
                    manager.MakeUpdateCommand(SqlConn.Command, userData, unUpdateColumns, whereClause);
                    SqlConn.AddParameter("@RowId", info.RowId);
                    SqlConn.AddParameter("@ListId", info.ListId);
                    SqlConn.AddParameter("@UIVersion", version);
                    SqlConn.Command.ExecuteNonQuery();
                }
#if PerformanceLog
            }
#endif
        }

        internal void ChangeLevelByNative(AveBaseItemInfo info, SPListItem item, int version, int originaleLevel, int draftOwnerId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeLevelByNative"))
            {
#endif
                try
                {
                    //mLog.Info("change item level, item level:{0},to level:{1}, version:{2}, draftOwnerId:{3}", (int)item.Level, originaleLevel, version, draftOwnerId);
                    string cmdText = @"update AllDocs set Level=@Level,DraftOwnerId=@DraftOwnerId where SiteID=@SiteID and ParentID=@ParentID and Id=@ID and Level=@OldLevel and DeleteTransactionId=0x
                    UPDATE AllUserData SET tp_Level=@Level,tp_DraftOwnerId=@DraftOwnerId WHERE tp_SiteID=@SiteID AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 AND tp_Level=@OldLevel";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@OldLevel", (int)item.Level);
                    SqlConn.AddParameter("@Level", originaleLevel);
                    if ((originaleLevel == 2 || originaleLevel == 255) && draftOwnerId > 0)
                    {
                        SqlConn.AddParameter("@DraftOwnerId", draftOwnerId);
                    }
                    else
                    {
                        SqlConn.AddParameter("@DraftOwnerId", DBNull.Value);
                    }
                    SqlConn.AddParameter("@ID", item.UniqueId);
                    SqlConn.AddParameter("@ListID", item.ParentList.ID);
                    SqlConn.AddParameter("@UIVersion", version);

                    if (SqlConn.ExecuteNonQuery(cmdText) > 0)
                    {
                        info.Level = originaleLevel;
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0758", item.Name, e);
                    //mLog.Warn(e, "An error occur when update File Level.File name:{0}", item.Name);
                }
#if PerformanceLog
            }
#endif
        }
        //TODO:Combine it with the function that with the same name but belong to aveDoc
        internal void ChangeCheckoutUserID(AveBaseItemInfo info, Guid uniqueID, int newUserID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeCheckoutUserID"))
            {
#endif
                //只有IsSPInstalled 才能进入到这个Dll中执行;
                //if (!AveEnvironment.IsSPInstalled)
                //{ return; }
                try
                {
                    string updateAllDocs = "UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND ParentId=@ParentId AND  ID=@ID AND DeleteTransactionId=0x";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", uniqueID);
                    SqlConn.AddParameter("@SiteID", info.SiteId);
                    SqlConn.AddParameter("@UserID", newUserID);
                    SqlConn.AddParameter("@ParentId", info.ParentId);

                    string updateAllUserData = string.Empty;
                    updateAllUserData = "UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID  AND tp_IsCurrentVersion=1 AND tp_DeleteTransactionId=0x";
                    SqlConn.ExecuteNonQuery(updateAllDocs);
                    SqlConn.ExecuteNonQuery(updateAllUserData);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0704", uniqueID, e);
                    //mLog.Warn(e, "An error occur when update AllDocs or AllUserdata.UniqueID:{0}", uniqueID);
                }
#if PerformanceLog
            }
#endif
        }

        internal void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeCheckoutUserID"))
            {
#endif
                try
                {
                    string updateAllDocs = "UPDATE AllDocs SET CheckoutUserId=@UserID,DocFlags = DocFlags|32 WHERE SiteId=@SiteID AND ID=@ID AND DeleteTransactionId=0x";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", uniqueID);
                    SqlConn.AddParameter("@SiteID", siteId);
                    SqlConn.AddParameter("@UserID", newUserID);

                    string updateAllUserData = string.Empty;
                    updateAllUserData = "UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_DocID=@ID  AND tp_IsCurrentVersion=1 AND tp_DeleteTransactionId=0x";

                    SqlConn.ExecuteNonQuery(updateAllDocs);
                    SqlConn.ExecuteNonQuery(updateAllUserData);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10RTSPItem0704", uniqueID, e);
                    //mLog.Warn(e, "An error occur when update AllDocs or AllUserdata.UniqueID:{0}", uniqueID);
                }
#if PerformanceLog
            }
#endif
        }

        // TODO:Add User Mapping
        // TODO:make this an the same function in AveSPItem one function
        internal void ChangeCheckoutUserIDForAllVersion(AveBaseItemInfo info, Guid uniqueID, int newUserID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeCheckoutUserIDForAllVersion"))
            {
#endif
                try
                {
                    string updateAllDocs = "UPDATE AllDocs SET CheckoutUserId=@UserID WHERE ID=@ID AND DeleteTransactionId=0x AND SiteId=@SiteID AND ParentId=@ParentID";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", uniqueID);
                    SqlConn.AddParameter("@SiteID", info.SiteId);
                    SqlConn.AddParameter("@UserID", newUserID);
                    SqlConn.AddParameter("@ParentID", info.ParentId);
                    SqlConn.ExecuteNonQuery(updateAllDocs);

                    string updateAllUserDate = "UPDATE AllUserData  SET tp_CheckoutUserId=@UserID WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID AND tp_DeleteTransactionId=0x";
                    SqlConn.ExecuteNonQuery(updateAllUserDate);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10RTSPDoc00568", uniqueID, e);
                    //mLog.Warn(e, "An error occur when update AllDocs or AllUserdata.UniqueID:{0}", uniqueID);
                }
#if PerformanceLog
            }
#endif
        }

        internal void ChangeModerationStatusByNative(AveBaseItemInfo info, SPFile file, int originalModerationStatus)
        {
            try
            {
                string cmdText = @"UPDATE AllUserData  SET tp_ModerationStatus=@ModerationStatus WHERE  tp_DeleteTransactionId=0x AND tp_ParentId=@ParentId 
                                AND tp_DocId=@ID AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ModerationStatus", originalModerationStatus);
                SqlConn.AddParameter("@ID", file.UniqueId);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@IsCurrentVersion", true);
                SqlConn.AddParameter("@CalculatedVersion", 0);
                SqlConn.AddParameter("@Level", info.Level);

                SqlConn.ExecuteNonQuery(cmdText);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0725", file.Name, e);
            }
        }

        internal void ChangeModerationStatusByNative(AveBaseItemInfo info, SPListItem item, int uiVersion, int originalModerationStatus)
        {
            try
            {
                string cmdText = @"UPDATE AllUserData  SET tp_ModerationStatus=@ModerationStatus WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ModerationStatus", originalModerationStatus);
                SqlConn.AddParameter("@ID", item.UniqueId);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@IsCurrentVersion", true);
                SqlConn.AddParameter("@CalculatedVersion", 0);
                SqlConn.AddParameter("@Level", info.Level);

                SqlConn.ExecuteNonQuery(cmdText);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0725", item.Name, e);
            }
        }
        /// <summary>
        /// keep tp_guid
        /// update tp_guid by SQL after created a new item(listitem, document, folder)
        /// </summary>
        /// <param name="mSqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <param name="tp_Guid"></param>
        internal void ChangeItemTPGuidByNative(AveBaseItemInfo info, Guid siteId, Guid parentId, Guid id, Guid tp_Guid)
        {
            try
            {
                SqlConn.ClearParameters();
                string CommandText = @"Update AllUserData SET tp_GUID=@tp_GUID 
                                       WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocId=@ID AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1
                                      ;";
                SqlConn.AddParameter("@tp_GUID", tp_Guid);
                SqlConn.AddParameter("@ParentId", parentId);
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ID", id);

                SqlConn.ExecuteNonQuery(CommandText);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.ERROR, "WP10RTSPItem1752", id, parentId, e);
                //mLog.Error(string.Format("An error occured when changing tp_guid of a list item {0}, listid:{1}", docLibRowId, listId), e.ToString());
            }
        }

        internal int GetCurrentUIVersion(Guid siteId, Guid id)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetCurrentUIVersion"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@Id", id);
                SqlConn.AddParameter("@SiteId", siteId);
                const string cmdText = "SELECT MAX(UIVersion) FROM AllDocs WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x0";
                return (int)SqlConn.ExecuteScalar(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal DateTime GetLastModifiedByNative(SPList parentList, SPListItem listItem)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetLastModifiedByNative"))
            {
#endif
                DateTime lastModified = DateTime.MinValue;
                try
                {
                    SqlConn.ClearParameters();
                    string cmdText = @"SELECT tp_Modified FROM AllUserData with(nolock)
                                WHERE tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1;";
                    SqlConn.AddParameter("@tp_ListId", parentList.ID);
                    SqlConn.AddParameter("@Id", listItem.ID);
                    lastModified = (DateTime)SqlConn.ExecuteScalar(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem2628", listItem.DisplayName, e);
                    //mLog.Warn("An error occured while getting modified time of item: {0}. Error: {1}.", listItem.DisplayName, e.ToString());
                }
                return lastModified;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// just for doc
        /// </summary>
        /// <param name="parentList"></param>
        /// <param name="parentFolder"></param>
        /// <param name="listItem"></param>
        /// <param name="mSqlConn"></param>
        internal bool MoveDocToConflictFolderByNative(SPList parentList, SPFolder parentFolder, SPListItem listItem, SPFolder conflictFolder, DateTime lastModified, bool isSourceWin)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.MoveDocToConflictFolderByNative"))
            {
#endif
                try
                {
                    string NewName = string.Empty;
                    if (isSourceWin)
                    {
                        NewName = AveSPUtility.GetConflictNewName(listItem.Name, lastModified);
                    }
                    else
                    {
                        NewName = listItem.Name;
                    }

                    SqlConn.ClearParameters();
                    string cmdText = @"UPDATE AllDocs SET DirName=@DirName, LeafName=@LeafName, ParentId=@ParentId
                                WHERE Id=@Id AND DeleteTransactionId=0x;";

                    if (parentFolder.ServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        SqlConn.AddParameter("@DirName", parentFolder.ServerRelativeUrl.Substring(1) + "/" + conflictFolder.Name);
                    }
                    else
                    {
                        SqlConn.AddParameter("@DirName", parentFolder.ServerRelativeUrl + "/" + conflictFolder.Name);
                    }
                    SqlConn.AddParameter("@LeafName", NewName);
                    SqlConn.AddParameter("@ParentId", conflictFolder.UniqueId);
                    SqlConn.AddParameter("@Id", listItem.UniqueId);
                    SqlConn.ExecuteNonQuery(cmdText);

                    SqlConn.ClearParameters();
                    cmdText = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_Guid=@tp_Guid
                        WHERE tp_ListId=@tp_ListId AND tp_ID=@Id AND tp_DeleteTransactionId=0x;";
                    SqlConn.AddParameter("@tp_ParentId", conflictFolder.UniqueId);
                    SqlConn.AddParameter("@tp_Guid", Guid.NewGuid());
                    SqlConn.AddParameter("@tp_ListId", parentList.ID);
                    SqlConn.AddParameter("@Id", listItem.ID);
                    SqlConn.ExecuteNonQuery(cmdText);
                    return true;
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem2676", listItem.Name, e);
                    //mLog.Warn("An error occured while moving the doc [{0}] to the conflict folder. Error: {1}.", listItem.Name, e.ToString());
                    return false;
                }
#if PerformanceLog
            }
#endif
        }

        internal bool MoveListItemToConflictFolderByNative(SPList parentList, SPFolder parentFolder, SPFolder conflictFolder, SPListItem listItem, DateTime lastModified)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.MoveListItemToConflictFolderByNative"))
            {
#endif
                try
                {
                    SPField Titlefiled = null;
                    string TitleColName = string.Empty;
                    string TimeName;
                    TimeName = "(" + lastModified.ToString("MM_dd_yyyy_hh_mm_ss") + ")";
                    try
                    {
                        Titlefiled = listItem.Fields["Title"];
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(Titlefiled.SchemaXml);
                        XmlElement firstNode = (XmlElement)xDoc.FirstChild;
                        if (firstNode.HasAttribute("ColName"))
                        {
                            TitleColName = firstNode.GetAttribute("ColName");
                        }
                    }
                    catch (Exception e)
                    {
                        if (parentList.ParentWeb.Language != 1033)
                        {
                            //not English in destination
                            XmlDocument listFieldsDoc = new XmlDocument();
                            listFieldsDoc.LoadXml(parentList.SchemaXml);
                            foreach (XmlNode fieldNod in listFieldsDoc.GetElementsByTagName("Field"))
                            {
                                XmlElement fieldEle = fieldNod as XmlElement;
                                if (fieldEle.GetAttribute("Name").Equals("Title") && fieldEle.GetAttribute("Name").Equals("Title"))
                                {
                                    if (fieldEle.HasAttribute("ColName"))
                                    {
                                        TitleColName = fieldEle.GetAttribute("ColName");
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    SqlConn.ClearParameters();
                    string cmdText = @"UPDATE AllDocs SET DirName=@DirName, ParentId=@ParentId
                                    WHERE Id=@Id AND DeleteTransactionId=0x;";
                    if (parentFolder.ServerRelativeUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        SqlConn.AddParameter("@DirName", parentFolder.ServerRelativeUrl.Substring(1) + "/" + conflictFolder.Name);
                    }
                    else
                    {
                        SqlConn.AddParameter("@DirName", parentFolder.ServerRelativeUrl + "/" + conflictFolder.Name);
                    }
                    SqlConn.AddParameter("@ParentId", conflictFolder.UniqueId);
                    SqlConn.AddParameter("@Id", listItem.UniqueId);
                    SqlConn.ExecuteNonQuery(cmdText);
                    if (string.IsNullOrEmpty(TitleColName))
                    {
                        cmdText = @"UPDATE AllUserData SET tp_ParentId=@tp_ParentId,tp_GUID=@tp_GUID  WHERE tp_ListId = @tp_ListId AND tp_ID = @tp_ID AND tp_DeleteTransactionId = 0x";
                        SqlConn.AddParameter("@tp_ListId", conflictFolder.ParentListId);
                        SqlConn.AddParameter("@tp_ID", listItem.ID);
                        SqlConn.AddParameter("@tp_GUID", Guid.NewGuid());
                        SqlConn.AddParameter("@tp_ParentId", conflictFolder.UniqueId);
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                    else
                    {

                        cmdText = string.Format(@"UPDATE AllUserData SET tp_ParentId=@tp_ParentId, tp_GUID=@tp_GUID,{0}={0}+'{1}'
                                              WHERE tp_Listid=@ListId And tp_DeleteTransactionId=0x 
                                              And tp_ID=@ID;", TitleColName, TimeName);
                        SqlConn.AddParameter("@tp_ParentId", conflictFolder.UniqueId);
                        SqlConn.AddParameter("@tp_GUID", Guid.NewGuid());
                        SqlConn.AddParameter("@ListId", conflictFolder.ParentListId);
                        SqlConn.AddParameter("@ID", listItem.ID);
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem2724", listItem.Name, e);
                    //mLog.Warn("An error occured while moving the item [{0}] to the conflict folder. Error: {1}.", listItem.Name, e.ToString());
                    return false;
                }
#if PerformanceLog
            }
#endif
        }

        internal int? GetInternalVersion(AveBaseItemInfo info, bool isVersion, Guid id, int UIVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetInternalVersion"))
            {
#endif
                //只有IsSPInstalled 才能进入到这个Dll中执行;
                //if (!AveEnvironment.IsSPInstalled)
                //{
                //    return -1;
                //}
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@Id", id);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@UIVersion", UIVersion);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                string cmdText = string.Empty;
                if (!isVersion)
                {
                    cmdText = "SELECT InternalVersion FROM AllDocs WHERE SiteId=@SiteId AND Id=@Id AND ParentId=@ParentId AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                }
                else
                {
                    cmdText = "SELECT InternalVersion FROM AllDocVersions WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                }

                object result = SqlConn.ExecuteScalar(cmdText);
                if (result != null && result is int)
                {
                    return (int)result;
                }
                else
                {
                    return -1;
                }
#if PerformanceLog
            }
#endif
        }

        internal bool IsCheckOutFile(AveBaseItemInfo info, Guid siteId, Guid parentId, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.IsCheckOutFile"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ParentId", parentId);
                SqlConn.AddParameter("@LeafName", name);

                const string cmdText = "SELECT Id, Level,CheckOutUserId FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND  DeleteTransactionId=0x order by UIVersion ASC";
                bool isCheckOutFile = false;
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte level = (byte)dr["Level"];
                        if (level == 255)
                        {
                            isCheckOutFile = true;
                            info.CheckOutFileUniqueID = (Guid)dr["Id"];
                            info.CheckoutUserId = (int)dr["CheckOutUserId"];
                            break;
                        }
                    }
                }
                return isCheckOutFile;
#if PerformanceLog
            }
#endif
        }

        internal bool IsCheckOutFile(Guid siteId, Guid fileId, ref int checkId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.IsCheckOutFile"))
            {
#endif
                checkId = -1;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ID", fileId);

                const string cmdText = "SELECT Id, Level,CheckOutUserId FROM AllDocs WHERE SiteId=@SiteId AND ID=@ID AND DeleteTransactionId=0x AND Level=255";
                bool isCheckOutFile = false;
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
                    }
                }
                return isCheckOutFile;
#if PerformanceLog
            }
#endif
        }

        internal bool IsCheckOutFile(Guid siteId, string url, ref int checkId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.IsCheckOutFile"))
            {
#endif
                checkId = -1;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                string fileName = url.Substring(url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).Trim('/');
                string dirName = url.Substring(0, url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)).Trim('/');
                SqlConn.AddParameter("@LeafName", fileName);
                SqlConn.AddParameter("@DirName", dirName);

                const string cmdText = "SELECT Id, Level,CheckOutUserId FROM AllDocs WHERE SiteId=@SiteId AND DirName=@DirName AND LeafName=@LeafName AND DeleteTransactionId=0x AND Level=255";
                bool isCheckOutFile = false;
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
                    }
                }
                return isCheckOutFile;
#if PerformanceLog
            }
#endif
        }

        internal bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.IsCheckOutFile"))
            {
#endif
                checkId = -1;
                id = Guid.Empty;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@ID", fileId);

                const string cmdText = "SELECT ID, CheckOutUserId FROM AllDocs WHERE SiteId=@SiteId AND ListId=@ListId AND DoclibRowId=@ID AND DeleteTransactionId=0x AND Level=255";
                bool isCheckOutFile = false;
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        isCheckOutFile = true;
                        checkId = (int)dr["CheckOutUserId"];
                        id = (Guid)dr["ID"];
                    }
                }
                return isCheckOutFile;
#if PerformanceLog
            }
#endif
        }

        //for form library item, to change content.
        internal void ChangeContentByNative(AveBaseItemInfo info, byte[] content)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeContentByNative"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@InternalVersion", info.InternalVersion);
                string cmdText = "UPDATE AllDocStreams SET Content=@Content WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion";
                SqlConn.AddParameter("@Content", content);
                try
                {
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Error, "WP10RTSPItem1306", mName, e);
                    //mLog.Error(e, "An error occurred while empty the content of document '{0}'.", mName);
                }
                cmdText = @" 
                    Declare @Size int
                    Select @Size=DataLength(Content) FROM AllDocStreams WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion
                    UPDATE AllDocs Set Size=@Size WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion
                    ";
                try
                {
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Error, "WP10RTSPItem1319", mName, e);
                    //mLog.Error(e, "An error occurred while updating the content size of document '{0}'.", mName);
                }
                SetRbsIdNull(info);
#if PerformanceLog
            }
#endif
        }
        private void SetRbsIdNull(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.SetRbsIdNull"))
            {
#endif
                if (info.InternalVersion != null && info.InternalVersion.Value != -1)
                {
                    string cmdStr =
                   @"UPDATE    AllDocStreams
            SET              RbsId = NULL
            WHERE     (SiteId = @SiteId) AND (Id = @ID) AND (InternalVersion = @InternalVersion)";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ID", info.GUID);
                    SqlConn.AddParameter("@InternalVersion", info.InternalVersion);
                    SqlConn.ExecuteNonQuery(cmdStr);
                }
#if PerformanceLog
            }
#endif
        }

        internal byte GetLevel(AveBaseItemInfo info, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetLevel"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@UIVersion", info.Version);
                SqlConn.AddParameter("@ParentId", info.ParentId);

                string cmdText = @"SELECT Level FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x
       union SELECT Level FROM AllDocVersions WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x";


                object result = SqlConn.ExecuteScalar(cmdText);
                if (result != null && result is byte)
                {
                    return (byte)result;
                }
                else
                {
                    return 0;
                }
#if PerformanceLog
            }
#endif
        }

        internal int SetInternalVersion(AveBaseItemInfo info, RestoringDto restoringDto, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.SetInternalVersion"))
            {
#endif
                int internalVersion = 0;
                try
                {
                    string cmdText = @"select ISNULL( MAX(InternalVersion),0) from AllDocVersions where SiteId=@SiteId and Id=@ID and UIVersion < @UIVersion";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", info.GUID);
                    SqlConn.AddParameter("@UIVersion", version);
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    int preVersion = (int)SqlConn.ExecuteScalar(cmdText);
                    cmdText = @"select ISNULL( MIN(InternalVersion),0) from AllDocVersions where SiteId=@SiteId and Id=@ID and UIVersion > @UIVersion";
                    int nextVersion = (int)SqlConn.ExecuteScalar(cmdText);
                    if (nextVersion == 0)
                    {
                        cmdText = @"select ISNULL( MIN(InternalVersion),0) from AllDocs where SiteId=@SiteId and Id=@ID and UIVersion > @UIVersion";
                        nextVersion = (int)SqlConn.ExecuteScalar(cmdText);
                    }
                    if (nextVersion == 0)
                    {
                        internalVersion = preVersion + 255;
                    }
                    else
                    {
                        int temp1 = nextVersion - 256;
                        int temp2 = (preVersion + nextVersion) / 2;
                        internalVersion = temp1 > temp2 ? temp1 : temp2;
                    }
                    if (restoringDto.TargetTable == RestoreTargetTable.AllDocVersions)
                    {
                        cmdText = @"update AllDocVersions set InternalVersion=@internalVersion where SiteId=@SiteId and  Id=@ID and UIVersion=@UIVersion and DeleteTransactionId=0x";
                    }
                    else
                    {
                        cmdText = @"update AllDocs set InternalVersion=@internalVersion where SiteId=@SiteId and Id=@ID and UIVersion=@UIVersion and DeleteTransactionId=0x";
                    }
                    SqlConn.AddParameter("@internalVersion", internalVersion);
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch
                {
                    internalVersion = 0;
                }
                return internalVersion;
#if PerformanceLog
            }
#endif
        }

        internal void SetDocFlagAsContent(AveBaseItemInfo info, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.SetDocFlagAsContent"))
            {
#endif
                try
                {
                    string updateFlagCmd = "Update AllDocs Set DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version AND DeleteTransactionId=0x0";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@Id", info.GUID);
                    SqlConn.AddParameter("@Version", version);
                    SqlConn.ExecuteNonQuery(updateFlagCmd);
                }
                catch (Exception e)
                {
                    //mLog.Warn("Reset Arichver Data Flag Exception");
                }
#if PerformanceLog
            }
#endif
        }


        /// <summary>
        /// checkin comment可以通过API改的，但是这个主要是为了改Approve Comment，因为Approve comment和Checkin Comment是同一个。
        /// </summary>
        /// <param name="checkinComment"></param>
        internal void UpdateCheckinCommentByNative(AveBaseItemInfo info, Guid fileGuid, string checkinComment)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateCheckinCommentByNative"))
            {
#endif
                string cmdStr = "UPDATE AllDocs SET CheckinComment=@CheckInComment WHERE SiteId=@SiteId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@CheckInComment", checkinComment);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ID", fileGuid);
                SqlConn.AddParameter("@UIVersion", info.Version);
                SqlConn.ExecuteNonQuery(cmdStr);
#if PerformanceLog
            }
#endif
        }

        internal void ChangeModerationStatusAndDraftOwnerIdByNative(AveBaseItemInfo info, SPFile file, int originalModerationStatus)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeModerationStatusAndDraftOwnerIdByNative"))
            {
#endif
                try
                {
                    string cmdText = @"UPDATE AllUserData  SET tp_ModerationStatus=@ModerationStatus WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ModerationStatus", originalModerationStatus);
                    SqlConn.AddParameter("@ID", file.UniqueId);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@IsCurrentVersion", true);
                    SqlConn.AddParameter("@CalculatedVersion", 0);
                    SqlConn.AddParameter("@Level", info.Level);

                    SqlConn.ExecuteNonQuery(cmdText);

                    cmdText = "UPDATE AllDocs SET DraftOwnerId=null WHERE SiteId=@SiteId AND ParentId=@ParentId AND ID=@ID AND DeleteTransactionId=0x AND Level=@Level";
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0725", file.Name, e);
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdateViewLastModifiedTimeByNative(AveBaseItemInfo info, SPFile spFile, DateTime timeLastModified)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateViewLastModifiedTimeByNative"))
            {
#endif
                string cmdStr = "UPDATE AllDocs SET TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@TimeLastModified", timeLastModified);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ID", spFile.UniqueId);
                SqlConn.AddParameter("@UIVersion", spFile.UIVersion);
                SqlConn.ExecuteNonQuery(cmdStr);

#if PerformanceLog
            }
#endif
        }

        internal void ResetContentByNative(AveSPItemNativeInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ResetContentByNative"))
            {
#endif
                string cmdText = string.Empty;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.ItemId);
                SqlConn.AddParameter("@InternalVersion", info.InternalVersion);

                cmdText = "SELECT COUNT(*) FROM AllDocStreams WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion";
                if ((int)SqlConn.ExecuteScalar(cmdText) > 0)
                {
                    cmdText = "UPDATE AllDocStreams SET Content=0x WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion";
                    try
                    {
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.ERROR, "WP10RTSPItem1212", mName, e);
                        //mLog.Error(e, "An error occurred while empty the content of document '{0}'.", mName);
                    }
                }
                else
                {
                    cmdText = "INSERT INTO AllDocStreams(Id, SiteId, InternalVersion, Content, RbsId) Values(@Id, @SiteId, @InternalVersion, 0x, NULL)";
                    try
                    {
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogLevel.ERROR, "WP10RTSPItem1225", mName, mInternalVersion, e);
                        //mLog.Error(e, "cannot restore version content of file: {0}, internal version: {0} ", mName, mInternalVersion);
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal SqlDataReader GetConflictInfoNormalForListItem(Guid siteId, Guid listId, string nameMapping)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetConflictInfoNormalForListItem"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@LeafName", nameMapping);
                string cmdText = "SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ListId=@ListId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
                return SqlConn.ExecuteReader(cmdText);
#if PerformanceLog
            }
#endif
        }
        internal SqlDataReader GetConflictInfoDeletedForListItem(Guid siteId, Guid listId, string nameMapping)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetConflictInfoDeletedForListItem"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@LeafName", nameMapping);
                string cmdText = "SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ListId=@ListId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
                return SqlConn.ExecuteReader(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal SqlDataReader GetConflictInfoNormal(Guid siteId, Guid parentId, string nameMapping)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@ParentId", parentId);
            SqlConn.AddParameter("@LeafName", nameMapping);
            string cmdText = "SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
            return SqlConn.ExecuteReader(cmdText);
        }
        internal SqlDataReader GetConflictInfoDeleted(Guid siteId, Guid parentId, string nameMapping)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@ParentId", parentId);
            SqlConn.AddParameter("@LeafName", nameMapping);
            string cmdText = "SELECT DeleteTransactionId, DoclibRowId, Level, UIVersion FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName ORDER BY TimeLastModified DESC";
            return SqlConn.ExecuteReader(cmdText);
        }

        internal SqlDataReader GetConflictInfoForReply(Guid siteId, Guid parentId, string messageId, string fieldColumn, bool forDeleted)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetConflictInfoForReply"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@tp_SiteId", siteId);
                SqlConn.AddParameter("@tp_ParentId", parentId);
                SqlConn.AddParameter("@MessageId", messageId);

                StringBuilder cmdTextBuilder = new StringBuilder();
                cmdTextBuilder.Append(@"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData 
                                        WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId and tp_DeleteTransactionId");
                if (forDeleted)
                {
                    cmdTextBuilder.Append("<>");
                }
                else
                {
                    cmdTextBuilder.Append("=");
                }
                cmdTextBuilder.Append("0x and tp_IsCurrentVersion=1 ");
                cmdTextBuilder.Append("and ");
                cmdTextBuilder.Append(fieldColumn);
                cmdTextBuilder.Append("=@MessageId;");
                return SqlConn.ExecuteReader(cmdTextBuilder.ToString());
#if PerformanceLog
            }
#endif
        }

        internal SqlDataReader GetConflictInfoNormal(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@tp_SiteId", siteId);
            SqlConn.AddParameter("@tp_ParentId", parentId);
            SqlConn.AddParameter("@tp_Guid", tp_Guid);
            string cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData 
                                        WHERE tp_SiteId=@tp_SiteId and tp_DeleteTransactionId=0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                        and tp_GUID=@tp_Guid;";
            return SqlConn.ExecuteReader(cmdText);
        }
        internal SqlDataReader GetConflictInfoDeleted(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@tp_SiteId", siteId);
            SqlConn.AddParameter("@tp_ParentId", parentId);
            SqlConn.AddParameter("@tp_Guid", tp_Guid);
            string cmdText = @"SELECT tp_DeleteTransactionId, tp_ID, tp_Level, tp_UIVersion from AllUserData 
                                        WHERE tp_SiteId=@tp_SiteId and tp_DeleteTransactionId<>0 and tp_IsCurrentVersion=1 and tp_ParentId=@tp_ParentId 
                                        and tp_GUID=@tp_Guid;";
            return SqlConn.ExecuteReader(cmdText);
        }


        internal void UpdateFileContentByNative(AveSPItemNativeInfo info, Stream fs)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateFileContentByNative"))
            {
#endif
                int BUFFSIZE = 1 << 20;//1M
                byte[] mBuffer = new byte[BUFFSIZE];
                byte[] updateBuffer;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Id", info.ItemId);
                SqlConn.AddParameter("@InternalVersion", info.InternalVersion);

                string cmdText = "Update AllDocStreams Set Content.write(@tempbuffer,NULL,NULL) where SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion ";
                while (true)
                {
                    int readCount = fs.Read(mBuffer, 0, mBuffer.Length);
                    if (readCount == 0)
                    {
                        break;
                    }
                    if (readCount == mBuffer.Length)
                    {
                        updateBuffer = mBuffer;
                    }
                    else
                    {
                        updateBuffer = new byte[readCount];
                        Array.Copy(mBuffer, 0, updateBuffer, 0, readCount);
                    }
                    SqlConn.AddParameterWithType("@tempbuffer", SqlDbType.VarBinary);
                    SqlConn.AddParameter("@tempbuffer", updateBuffer);
                    int result = SqlConn.ExecuteNonQuery(cmdText);
                }

                cmdText = @"UPDATE AllDocStreams SET RbsId = NULL WHERE (SiteId = @SiteId) AND (Id = @ID) AND (InternalVersion = @InternalVersion)
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (InternalVersion = @InternalVersion);
                            UPDATE AllDocVersions SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (InternalVersion = @InternalVersion)";
                SqlConn.AddParameter("@Size", info.Size);
                SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal void DeleteVersionByNative(AveBaseItemInfo info, SPFile file, int uiVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.DeleteVersionByNative"))
            {
#endif
                try
                {
                    string cmdText = @"delete AllDocStreams where SiteId=@SiteId and Id=@ID and InternalVersion in
(select InternalVersion from AllDocs where SiteId=@SiteId and ParentId=@ParentId and DeleteTransactionId=0x and  Id=@ID and UIVersion=@UIVersion
union
select InternalVersion from AllDocVersions where SiteId=@SiteId and Id=@ID and UIVersion=@UIVersion
)
delete from AllDocs where SiteId=@SiteId and ParentId=@ParentId and Id=@ID and UIVersion=@UIVersion AND DeleteTransactionId=0x
delete from AllDocVersions where SiteId=@SiteId and Id=@ID and UIVersion=@UIVersion
delete from AllUserData where tp_SiteId=@SiteId and tp_ParentId=@ParentId and tp_DocId=@ID and tp_DeleteTransactionId=0x and tp_UIVersion=@UIVersion";

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ID", file.UniqueId);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@UIVersion", uiVersion);
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTSPItem0550", file.Name, e);
                    //mLog.Warn(e, "An error occur when delete file version.File name:{0}", file.Name);
                }
#if PerformanceLog
            }
#endif
        }

        internal DateTime GetVersionModified(Guid siteId, Guid parentId, int rowId, int uiVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetVersionModified"))
            {
#endif
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@RowId", rowId);
                    SqlConn.AddParameter("@ParentId", parentId);
                    SqlConn.AddParameter("@SiteId", siteId);
                    SqlConn.AddParameter("@UIVersion", uiVersion);
                    const string cmdText = @"SELECT tp_Modified FROM AllUserData where tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) 
AND tp_ParentId=@ParentId AND tp_ID=@RowId AND tp_UIVersion=@UIVersion";
                    return (DateTime)SqlConn.ExecuteScalar(cmdText);
                }
                catch
                {
                    return DateTime.MinValue;
                }
#if PerformanceLog
            }
#endif
        }

        internal DateTime GetLastModified(Guid siteId, Guid parentId, int rowId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetLastModified"))
            {
#endif
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@RowId", rowId);
                    SqlConn.AddParameter("@ParentId", parentId);
                    SqlConn.AddParameter("@SiteId", siteId);
                    const string cmdText = "SELECt MAX(tp_Modified) FROM AllUserData where tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_ID=@rowId";
                    return (DateTime)SqlConn.ExecuteScalar(cmdText);
                }
                catch
                {
                    return DateTime.MinValue;
                }
#if PerformanceLog
            }
#endif
        }

        internal void ChangeInstanceIdByNative(AveBaseItemInfo info, SPListItem item, int newInstanceId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.ChangeInstanceIdByNative"))
            {
#endif
                try
                {
                    string cmdText = @"UPDATE AllUserData  SET tp_InstanceId=@InstanceId WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@InstanceId", newInstanceId);
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ID", item.UniqueId);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@IsCurrentVersion", true);
                    SqlConn.AddParameter("@CalculatedVersion", 0);
                    SqlConn.AddParameter("@Level", (int)item.Level);

                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Warn("an error occured while change instanceId. error:{0}", e.ToString());
                }
#if PerformanceLog
            }
#endif
        }


        internal int GetItemEditorByNative(AveBaseItemInfo info, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetItemEditorByNative"))
            {
#endif
                int modified = 0;
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", item.ID);
                    SqlConn.AddParameter("@ListId", item.ParentList.ID);
                    SqlConn.AddParameter("@SiteId", info.SiteId);

                    string cmdText = "SELECT tp_Editor FROM AllUserData WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_ID=@ID AND tp_DeleteTransactionId=0x AND tp_IsCurrent=1";
                    modified = (int)SqlConn.ExecuteScalar(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Warn("An error occured while get item modified.");
                }
                return modified;
#if PerformanceLog
            }
#endif
        }

        internal void SetItemEditorByNative(AveBaseItemInfo info, IAveListItem item, int modified)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.SetItemEditorByNative"))
            {
#endif
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ID", item.ID);
                    SqlConn.AddParameter("@ListId", item.ParentList.ID);
                    SqlConn.AddParameter("@Editor", modified);
                    SqlConn.AddParameter("@SiteId", info.SiteId);

                    string cmdText = "UPDATE AllUserData SET tp_Editor=@Editor WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_ID=@ID AND tp_DeleteTransactionId=0x AND tp_IsCurrent=1";
                    SqlConn.ExecuteNonQuery(cmdText);
                }
                catch (Exception e)
                {
                    //mLog.Warn("An error occured while set item modified.");
                }
#if PerformanceLog
            }
#endif
        }

        internal void RenameAttachment(AveBaseItemInfo info, string oldName, string newName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.RenameAttachment"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@LeafName", oldName);
                SqlConn.AddParameter("@Name", newName);
                string cmdText = "Update AllDocs set LeafName=@Name WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
                SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal Guid GetAttachmentUniqueId(AveBaseItemInfo info, string realName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetAttachmentUniqueId"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@LeafName", realName);

                string cmdText = "SELECT Id FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
                return (Guid)SqlConn.ExecuteScalar(cmdText);
#if PerformanceLog
            }
#endif
        }
        internal int GetAttachmentVersion(AveBaseItemInfo info, string realName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.Import"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@LeafName", realName);

                string cmdText = "SELECT InternalVersion FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
                return (int)SqlConn.ExecuteScalar(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal Guid GetAttachmentsParentID(AveBaseItemInfo info, IAveListItem item)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetAttachmentsParentID"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ParentId", item.ParentList.RootFolder.SubFolders["Attachments"].UniqueId);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@LeafName", item.ID.ToString());
                string cmdText = "SELECT Id FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
                return (Guid)SqlConn.ExecuteScalar(cmdText);
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// change webpart id
        /// </summary>
        /// <param name="webPartId"></param>
        /// <param name="siteId"></param>
        /// <param name="fileId"></param>
        /// <param name="id"></param>
        internal void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, Guid id)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateWebPartInfo"))
            {
#endif
                string cmdText = @"UPDATE AllWebParts SET tp_Id=@ID where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_ID=@WebPartId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteID", siteId);
                SqlConn.AddParameter("@PageID", fileId);
                SqlConn.AddParameter("@ID", id);
                SqlConn.AddParameter("@WebPartId", webPartId);
                SqlConn.ExecuteNonQuery(cmdText);

                try
                {
                    cmdText = @"SELECT COUNT(tp_WebPartID) FROM WebPartLists where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@ID";

                    if ((int)SqlConn.ExecuteScalar(cmdText) == 0)
                    {
                        cmdText = @"UPDATE WebPartLists Set tp_WebPartID=@ID where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID";
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                    else
                    {
                        cmdText = "DELETE FROM WebPartLists WHERE tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID";
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                }
                catch { }
#if PerformanceLog
            }
#endif
        }


        internal void UpdatePropertiesByNative(string webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdatePropertiesByNative"))
            {
#endif
                string idProperty = webPartId;
                if (webPartId != null && webPartId.Length > 36)
                {
                    webPartId = webPartId.Substring(webPartId.Length - 36);
                    webPartId = webPartId.Replace("_", "-");
                }
                else
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTWebPart531", webPartId == null ? "NULL" : webPartId);
                    //mLog.Warn("The webpart id is null or length less than 36. id: {0}", webPartId == null ? "NULL" : webPartId);
                    return;
                }

                SqlConn.ClearParameters();
                StringBuilder s = new StringBuilder("UPDATE AllWebParts SET tp_AllUsersProperties=");

                if (allUsersProperties != null)
                {
                    s.Append("@AllUsersProperties,");
                    SqlConn.AddParameter("@AllUsersProperties", allUsersProperties);
                }
                else
                {
                    s.Append("NULL,");
                }

                if (perUserProperties != null)
                {
                    s.Append("tp_PerUserProperties=@PerUserProperties ");
                    SqlConn.AddParameter("@PerUserProperties", perUserProperties);
                }
                else
                {
                    s.Append("tp_PerUserProperties=NULL ");
                }

                s.Append("WHERE tp_SiteId=@SiteID  AND tp_PageUrlID=@PageID AND (tp_ID=@ID or tp_WebPartIdProperty=@IdProperty)");
                SqlConn.AddParameter("@SiteID", siteId);
                SqlConn.AddParameter("@PageID", fileId);
                SqlConn.AddParameter("@ID", new Guid(webPartId));
                SqlConn.AddParameter("@IdProperty", idProperty);

                SqlConn.ExecuteNonQuery(s.ToString());
#if PerformanceLog
            }
#endif
        }

        internal void UpdateWebPartInfo(Guid webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateWebPartInfo"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteID", siteId);
                SqlConn.AddParameter("@PageID", fileId);
                SqlConn.AddParameter("@Level", oldLevel);
                SqlConn.AddParameter("@ID", webPartId);
                SqlConn.AddParameter("@IsCurrentVersion", isCurrentVersion);

                string cmdText = @"UPDATE AllWebParts SET ";
                if (newLevel != oldLevel)
                {
                    cmdText += "tp_Level=@NewLevel,";
                    SqlConn.AddParameter("@NewLevel", newLevel);
                }
                if (pageVersion != 0 && pageVersion < uIVersion)
                {
                    cmdText += "tp_PageVersion=@PageVersion,";
                    SqlConn.AddParameter("@PageVersion", pageVersion);
                }
                cmdText += "tp_IsCurrentVersion=@IsCurrentVersion where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_PageVersion=0 AND tp_IsCurrentVersion=1 AND tp_Level=@Level AND tp_ID=@ID";
                SqlConn.ExecuteNonQuery(cmdText);

                try
                {
                    cmdText = @"SELECT COUNT(tp_Level) FROM WebPartLists where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@SourceLevel";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteID", siteId);
                    SqlConn.AddParameter("@WebPartID", webPartId);
                    SqlConn.AddParameter("@PageID", fileId);
                    SqlConn.AddParameter("@CurPageLevel", oldLevel);
                    SqlConn.AddParameter("@SourceLevel", newLevel);

                    if ((int)SqlConn.ExecuteScalar(cmdText) == 0)
                    {
                        cmdText = @"UPDATE WebPartLists Set tp_Level=@SourceLevel where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND tp_WebPartID=@WebPartID AND tp_Level=@CurPageLevel";
                        SqlConn.ExecuteNonQuery(cmdText);
                    }
                }
                catch (Exception ex)
                {
                    //mLog.Warn("An error while update page level in WebPartList. Error: " + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdateView(string webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateView"))
            {
#endif
                string idProperty = webPartId;
                if (webPartId != null && webPartId.Length > 36)
                {
                    webPartId = webPartId.Substring(webPartId.Length - 36);
                    webPartId = webPartId.Replace("_", "-");
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@BaseViewID", baseViewId);
                SqlConn.AddParameter("@SiteID", siteId);
                bool needUpdateContentType = contentTypeId != null;
                if (needUpdateContentType)
                {
                    SqlConn.AddParameter("@ContentType", contentTypeId);
                }
                SqlConn.AddParameter("@PageID", fileId);
                SqlConn.AddParameter("@ID", new Guid(webPartId));
                SqlConn.AddParameter("@IdProperty", idProperty);
                string cmdText = string.Empty;
                if (baseViewId >= 0)
                {
                    if (view != null)
                    {
                        SqlConn.AddParameter("@View", view);
                        cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=@BaseViewID,tp_View=@View " +
                            (needUpdateContentType ? ", tp_ContentTypeId=@ContentType" : "") + " where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND (tp_ID=@ID or tp_WebPartIdProperty=@IdProperty)";
                    }
                    else
                    {
                        cmdText = @"UPDATE AllWebParts SET tp_BaseViewID=@BaseViewID,tp_View = NULL " +
                            (needUpdateContentType ? ", tp_ContentTypeId=@ContentType " : "") + " where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND (tp_ID=@ID or tp_WebPartIdProperty=@IdProperty)";
                    }
                    SqlConn.ExecuteNonQuery(cmdText);
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdateUserID(string webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateUserID"))
            {
#endif
                string idProperty = webPartId;
                if (webPartId != null && webPartId.Length > 36)
                {
                    webPartId = webPartId.Substring(webPartId.Length - 36);
                    webPartId = webPartId.Replace("_", "-");
                }
                else
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTWebPart598", webPartId == null ? "NULL" : webPartId);
                    //mLog.Warn("The webpart id is null or length less than 36. id: {0}", webPartId == null ? "NULL" : webPartId);
                    return;
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteID", siteId);
                SqlConn.AddParameter("@PageID", fileId);
                SqlConn.AddParameter("@UserID", userId);
                SqlConn.AddParameter("@WebPartID", webPartId);
                SqlConn.AddParameter("@ID", new Guid(webPartId));
                SqlConn.AddParameter("@IdProperty", idProperty);
                if (isPersonal)
                {
                    string command = "UPDATE Personalization SET tp_UserID=@UserID WHERE tpSiteId=@SiteID AND tp_WebPartID=@WebPartID AND tp_PageUrlId=@PageId AND tp_UserID=@CurrentUserID";
                    SqlConn.AddParameter("@CurrentUserID", currentUserId);
                    SqlConn.ExecuteNonQuery(command);
                }
                else
                {
                    string command = "UPDATE AllWebParts SET tp_userId=@UserID WHERE tp_siteid=@SiteID AND tp_PageUrlID=@PageID  AND (tp_id=@ID or tp_WebPartIdProperty=@IdProperty)";
                    SqlConn.ExecuteNonQuery(command);

                    command = "UPDATE WebPartLists SET tp_userId=@UserID WHERE  tp_webpartid=(select top(1) tp_ID from AllWebParts where tp_siteid=@SiteID AND (tp_id=@ID or tp_WebPartIdProperty=@IdProperty))";
                    SqlConn.ExecuteNonQuery(command);
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdatePersonalPropertiesByNative(string webPartId, Guid siteId, int currentUserId, byte[] perUserBytes)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdatePersonalPropertiesByNative"))
            {
#endif
                SqlConn.ClearParameters();
                if (webPartId != null && webPartId.Length > 36)
                {
                    webPartId = webPartId.Substring(webPartId.Length - 36);
                    webPartId = webPartId.Replace("_", "-");
                }
                else
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTWebPart574", webPartId == null ? "NULL" : webPartId);
                    //mLog.Warn("The webpart id is null or length less than 36. id: {0}", webPartId == null ? "NULL" : webPartId);
                    return;
                }
                string cmdText = @"UPDATE Personalization SET tp_PerUserProperties=@PerUserProperties where tp_SiteId=@SiteId AND tp_ID=@ID AND tp_UserId=@UserId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@PerUserProperties", perUserBytes);
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ID", webPartId);
                SqlConn.AddParameter("@UserId", currentUserId);

                SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }


        internal SqlDataReader GetAlerts(Guid siteId, Guid listId, int itemId, AveSPAlertHostType hostType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetAlerts"))
            {
#endif
                string queryCmd = @"SELECT Id, Properties FROM  ImmedSubscriptions Union SELECT Id,Properties FROM SchedSubscriptions ";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);

                if (listId == Guid.Empty)
                {
                    queryCmd += " WHERE SiteId=@SiteId AND ListId is NULL";
                }
                else
                {
                    queryCmd += " WHERE SiteId=@SiteId AND ListId=@ListId";
                    SqlConn.AddParameter("@ListId", listId);
                }
                switch (hostType)
                {
                    case AveSPAlertHostType.List:
                    case AveSPAlertHostType.Folder:
                        queryCmd += " AND ItemId is NULL AND Deleted=0";
                        break;
                    case AveSPAlertHostType.Doc:
                    case AveSPAlertHostType.Item:
                        SqlConn.AddParameter("@ItemId", itemId);
                        queryCmd += " AND ItemId=@ItemId AND Deleted=0";
                        break;
                    default:
                        break;
                }
                return SqlConn.ExecuteReader(queryCmd);
#if PerformanceLog
            }
#endif
        }
        internal SqlDataReader GetWebAlerts(Guid siteId, Guid webId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebAlerts"))
            {
#endif
                string queryCmd = @"SELECT Id, Properties,ListId  FROM  ImmedSubscriptions  WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0 Union SELECT Id,Properties,ListId  FROM SchedSubscriptions  WHERE SiteId=@SiteId AND WebId=@WebId  AND Deleted=0";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@WebId", siteId);
                return SqlConn.ExecuteReader(queryCmd);
#if PerformanceLog
            }
#endif
        }
        public int GetLookupIdByGUID(Guid lookupListId, Guid GUID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetLookupIdByGUID"))
            {
#endif
                string cmdText = @"SELECT tp_ID from AllUserData WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0  
                                        AND tp_GUID=@GUID AND tp_RowOrdinal=0";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ListId", lookupListId);
                SqlConn.AddParameter("@GUID", GUID);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int tp_ID = dr.GetInt32(0);
                        return tp_ID;
                    }
                }
                return -1;
#if PerformanceLog
            }
#endif
        }

        internal bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFieldInSiteChildren"))
            {
#endif
                try
                {
                    string cmdText = @"SELECT count(contenttypeid) FROM contenttypes 
                                WHERE siteid=@SiteId And Class=0 AND cast(ContentTypeId as uniqueidentifier)=@FieldId 
                                        AND  Scope like @Scope AND DeleteTransactionId=0x";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", siteId);
                    SqlConn.AddParameter("@FieldId", fieldId);
                    SqlConn.AddParameter("@Scope", scope + "/%");
                    if (((int)SqlConn.ExecuteScalar(cmdText)) > 0)
                    {
                        return true;
                    }
                }
                catch { }
                return false;
#if PerformanceLog
            }
#endif
        }

    }
}
