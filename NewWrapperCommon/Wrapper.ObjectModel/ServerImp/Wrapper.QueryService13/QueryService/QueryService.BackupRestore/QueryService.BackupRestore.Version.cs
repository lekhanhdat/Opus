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
        /// 获取Document特定Version下的信息
        /// 通过API只可以获取version下的部分信息
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="dataCache"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void GetVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetVersionInfo"))
            {

                string cmdText =
                              @"SELECT UIVersion,InternalVersion,TimeCreated,DocFlags,MetaInfoSize,Size,MetaInfo,CheckinComment,
                                 Level,DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo
                        FROM  AllDocVersions WITH(NOLOCK)
                        WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x AND UIVersion=@Version";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
                mQueryWorker.AddParameter("@Id", itemInfo.GUID);
                mQueryWorker.AddParameter("Version", itemInfo.Version);
                AveQueryUtility.TryGetDBRow(dataCache, mQueryWorker, cmdText);

            }

        }

        public void GetListItemVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache)
        {

            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetDocVersions.AllUserData"))
            {

                string cmdText = string.Format(
                            @"SELECT tp_Modified as TimeLastModified,tp_Created as TimeCreated,tp_ParentId as ParentId
                            ,tp_DocId as Id,tp_DeleteTransactionId as DeleteTransactionId,tp_Level as Level,tp_IsCurrentVersion as IsCurrentVersion
                            ,tp_UIVersion as UIVersion,tp_DraftOwnerId as DraftOwnerId,tp_CheckoutUserId as CheckoutUserId
                        FROM  AllUserData WITH(NOLOCK)
                        WHERE tp_SiteId=@SiteId AND tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1)
                        AND tp_ID=@Id AND tp_UIVersion=@Version");
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
                mQueryWorker.AddParameter("@ListId", itemInfo.ListId);
                mQueryWorker.AddParameter("@Id", itemInfo.RowId);
                mQueryWorker.AddParameter("@Version", itemInfo.Version);
                AveQueryUtility.TryGetDBRow(dataCache, mQueryWorker, cmdText);

            }

        }

        /// <summary>
        /// 创建一个Version
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="version"></param>
        /// <param name="restoringDto"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Item-020")]
        public bool CreateVersionByNative(AveBaseItemInfo info, int version, RestoringDto restoringDto)
        {
            try
            {
                bool needInsertToAllDocVersions = false;
                bool needInsertToAllDocs = false;
                bool needInsertToAllUserData = false;
                string selectCmdText = null;
                string updateCmdText = null;
                string logId = null;
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Version", version);
                mQueryWorker.AddParameter("@ListId", info.ListId);
                mQueryWorker.AddParameter("@RowId", info.RowId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);

                selectCmdText = @"SELECT DeleteTransactionId FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                updateCmdText = @"UPDATE AllDocVersions Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
                if (!CheckExistingRecord(selectCmdText, updateCmdText, version, restoringDto.OverWrite, false, ref needInsertToAllDocVersions))
                {
                    // Conflict version and not overwrite
                    return false;
                }
                if (needInsertToAllDocVersions && restoringDto.TargetTable == RestoreTargetTable.AllDocs)
                {
                    needInsertToAllDocVersions = false;
                }

                selectCmdText = @"SELECT DeleteTransactionId FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND (DeleteTransactionId<>0x or DeleteTransactionId = 0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";
                updateCmdText = @"UPDATE AllDocs Set DeleteTransactionId=0x WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND Id=@Id AND UIVersion=@Version";
                if (!CheckExistingRecord(selectCmdText, updateCmdText, version, restoringDto.OverWrite, false, ref needInsertToAllDocs))
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
                selectCmdText = @"SELECT tp_DeleteTransactionId FROM AllUserData  WITH(NOLOCK) WHERE tp_SiteId=@SiteId AND (tp_DeleteTransactionId=0x or tp_DeleteTransactionId<>0x) AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";
                updateCmdText = @"UPDATE AllUserData Set tp_DeleteTransactionId=0x WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId<>0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND tp_ParentId=@ParentId AND tp_DocId=@Id AND tp_UIVersion=@Version";
                if (!CheckExistingRecord(selectCmdText, updateCmdText, version, restoringDto.OverWrite, true, ref needInsertToAllUserData))
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
            catch (SqlException queryException)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while execute CreateVersionByNative. ErrorMessage:{0}.", new AveQueryException(string.Format("Exception Error Code----{0}", queryException.Number), queryException));
                return false;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.ERROR, "Error occur while execute CreateVersionByNative. ErrorMessage:{0}.", e);
                return false;
            }
        }

        /// <summary>
        /// 获取Document的所有UIVersions
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<int> GetDocVersions(AveBaseItemInfo info)
        {
            List<int> versions = new List<int>();
            StringBuilder cmdText;
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.GetDocVersions.AllDocs"))
            {

                cmdText = new StringBuilder(@"Select UIVersion from Alldocs WITH(NOLOCK) where SiteId=@SiteId And DeleteTransactionId=0x And");
                if (info.ParentId != Guid.Empty)
                {
                    cmdText.Append(@" ParentID=@ParentID AND");
                }
                else
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                cmdText.Append(" Id=@Id");
                try
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@SiteId", info.SiteId);
                    mQueryWorker.AddParameter("@ParentID", info.ParentId);
                    mQueryWorker.AddParameter("@Id", info.GUID);
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                    {
                        while (dr.Read())
                        {
                            int version = dr.GetInt32(0);
                            if (!versions.Contains(version))
                            {
                                versions.Add(version);
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
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryService.GetDocVersions.AllDocVersions"))
            {
                cmdText = new StringBuilder(@"Select UIVersion from AllDocVersions WITH(NOLOCK) where SiteId=@SiteId And Id=@Id And DeleteTransactionId=0x");
                try
                {
                    using (SqlDataReader vr = mQueryWorker.ExecuteReader(cmdText.ToString()))
                    {
                        while (vr.Read())
                        {
                            int version = vr.GetInt32(0);
                            if (!versions.Contains(version))
                            {
                                versions.Add(version);
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
            }
            return versions;
        }

        /// <summary>
        /// 更新Document的UIVersion(AllDocVersions/AllDocs，AllUserData)
        /// 有API实现(但更新version有局限性，只能从小到大)
        /// </summary>
        /// <param name="info"></param>
        /// <param name="restoringDto"></param>
        /// <param name="allDocData"></param>
        /// <param name="allUserData"></param>
        /// <param name="version"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        public void UpdateVersionByNative(AveBaseItemInfo info, RestoringDto restoringDto, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, int version)
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
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperQueryServiceResource.UpdataVersionByNativeError, ex);
            }
        }

        [QueryReview("2012/12/17", "Austin Han", true, "Use UNION ALL instead of UNION to improve the performance.")]
        public byte GetLevel(AveBaseItemInfo info, int version)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", info.GUID);
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@UIVersion", version);
            mQueryWorker.AddParameter("@ParentId", info.ParentId);

            string cmdText = @"SELECT Level FROM AllDocs WITH(NOLOCK) WHERE SiteId=@SiteId AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x
               union all SELECT Level FROM AllDocVersions WITH(NOLOCK) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND DeleteTransactionId=0x";


            object result = mQueryWorker.ExecuteScalar(cmdText);
            if (result != null && result is byte)
            {
                return (byte)result;
            }
            else
            {
                return 0;
            }
        }

        [QueryReview("2012/12/11", "Austin Han")]
        [QueryReview("Item-020")]
        private void UpdateAllDocVersions(AveBaseItemInfo info, Dictionary<string, object> allDocVersions, int version, bool resetValue)
        {
            AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllDocVersions");
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
                whereClause = @",DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
            }
            else
            {
                whereClause = @"DocFlags=(DocFlags&(~65536)) WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@Version";
            }
            try
            {
                manager.MakeUpdateCommand(mQueryWorker.Command, needUpdateDocData, unUpdateColumns, whereClause);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
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

        [QueryReview("2012/12/10", "Austin Han")]
        private void InsertIntoAllDocVersions(AveBaseItemInfo info, int version)
        {

            string cmdText = @"
SELECT SiteId,Id,UIVersion,TimeCreated,DocFlags,MetaInfoSize,Size,Level,
       DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo,SetupPathVersion
FROM AllDocs WITH(NOLOCK)
WHERE SiteId=@SiteId AND (DeleteTransactionId=0x or DeleteTransactionId<>0x) AND ParentId=@ParentId AND Id=@Id AND UIVersion=@UIVersion";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", info.SiteId);
            mQueryWorker.AddParameter("@Id", info.GUID);
            mQueryWorker.AddParameter("@ParentId", info.ParentId);
            mQueryWorker.AddParameter("@UIVersion", info.Version);
            mQueryWorker.Command.CommandText = cmdText;

            AveQueryColumnInfoManager manager = new AveQueryColumnInfoManager("AllDocVersions");
            manager.LoadColumnsInfo(null, mQueryWorker.Command);
            manager.ResetColumnValue("UIVersion", version);
            if (version % 512 == 0)
            {
                manager.ResetColumnValue("Level", (byte)1);
            }
            else
            {
                manager.ResetColumnValue("Level", (byte)2);
            }
            manager.MakeInsertCommand(mQueryWorker.Command);

            if (mQueryWorker.Command.Parameters.Count > 0)
            {
                mQueryWorker.Command.ExecuteNonQuery();
            }
        }

    }
}
