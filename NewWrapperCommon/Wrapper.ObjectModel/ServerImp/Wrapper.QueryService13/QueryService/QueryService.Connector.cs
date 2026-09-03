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
using AvePoint.Wrapper.Common;
using System.Data.SqlClient;
using System.Data;
using AvePoint.GCommon;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService : IAveConnectorQueryService
    {
        private delegate void WrapperAction();

        [QueryReview("2012/05/15", "Kexin Guo")]
        private void RunWithWapperedException(WrapperAction action)
        {
            try
            {
                if (action != null)
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                throw new AveQueryException(ex.Message, ex);
            }
        }

        /// <summary>
        /// 保存Connector Media Library的缩略图等信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isVideo"></param>
        [QueryReview("2012/05/03", "Kexin Guo")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-001")]
        public void SaveThumbnail(System.Collections.Hashtable obj, bool isVideo = false)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                foreach (string s in obj.Keys)
                {
                    cmd.Parameters.AddWithValue(s, obj[s]);
                }
                bool snapShotIsNull = obj["@SnapShot"] == null || string.IsNullOrEmpty(obj["@SnapShot"].ToString());

                string commandText = @"Update AlluserData Set {0} Where tp_SiteId = @SiteId And tp_DeleteTransactionId = 0x AND tp_IsCurrentVersion=1 AND tp_ParentId=@ParentId AND tp_DocId=@ItemId AND tp_CalculatedVersion=0 AND tp_Level=@ItemLevel And tp_RowOrdinal=0";
                string text = string.Empty;
                if (isVideo)
                {
                    text = snapShotIsNull ? "nvarchar13=@Player" : "ntext2=@snapShot,nvarchar13=@Player";
                }
                else
                {
                    text = snapShotIsNull ? "nvarchar9=@Resolution,nvarchar13=@Player" : "ntext2=@snapShot,nvarchar9=@Resolution,nvarchar13=@Player";
                }
                cmd.CommandText = string.Format(commandText, text);

                mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataOneRow);
            }
        }

        /// <summary>
        /// 判断一个文件是否为dirty状态
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/05/08", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty for AllDocs")]
        [Obsolete("SP2013 no dirty")]
        [DoNotNeedReview]
        public IAveQueryDataReader IsDirty(AveBaseItemInfo info)
        {
            return null; //No dirty file in SP2013
        }

        /// <summary>
        /// 获取dirty文件的信息
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Kexin Guo", true, "add ParentId for AllDocs")]
        [QueryReview("2012/12/17", "hyyin")]
        public IAveQueryDataReader GetDocInfoForDirty(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                //cmd.Parameters.AddWithValue("@UIVersion", info.Version);

                cmd.CommandText = @"SELECT WebId,ListId,ParentId,UIVersion,Level,InternalVersion,CheckoutUserId,StreamSchema FROM AllDocs with(nolock) WHERE SiteId=@SiteId AND Id=@Id";

                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        /// <summary>
        /// Connector Meta info以及大文件相关的实现
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="buffer"></param>
        /// <param name="isFirstUpdate"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are parameter of the sql statement. ")]
        [QueryReview("2012/05/02", "Kexin Guo")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-002")]
        public void UpdateDBContent(AveBaseItemInfo info, System.IO.Stream fs, bool isRbs)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                using (SqlTransaction transaction = mQueryWorker.BeginTransaction())
                {
                    cmd.Connection = mQueryWorker.Connection;
                    cmd.Transaction = transaction;
                    try
                    {
                        cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                        cmd.Parameters.AddWithValue("@DocId", info.GUID);
                        cmd.Parameters.AddWithValue("@Level", info.Level);
                        cmd.Parameters.AddWithValue("@Partition", 0);
                        cmd.Parameters.AddWithValue("@StreamSchema", 65);
                        if (info.IsCurrentVersion)
                        {
                            cmd.Parameters.AddWithValue("@histVersion", 0);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@histVersion", info.Version);
                        }
                        if (isRbs)
                        {
                            cmd.CommandText = @"
DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND DocsToStreams.Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 1);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 2);
ELSE
BEGIN
    UPDATE DocStreams SET Content = 0x0, [RbsId]=NULL, TYPE=11 WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
	set @row = @@ROWCOUNT
	IF @row=0 RAISERROR ('Cannot find the stream.', 16, 3);
END
IF @histVersion =0
  UPDATE AllDocs SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND Level=@Level
ELSE
  UPDATE AllDocVersions SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@histVersion
";
                            mQueryWorker.ExecuteNonQuery(cmd);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Id", info.GUID);
                            cmd.Parameters.AddWithValue("@InternalVersion", info.InternalVersion);
                            cmd.CommandText = @"
UPDATE DocStreams 
SET Content = 0x0 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";
                            mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                        }

                        byte[] buffer = new byte[1024 * 1024];
                        int lenOnce = 0;
                        bool isFirstUpdate = true;
                        using (fs)
                        {
                            cmd.Parameters.Add("@tempbuffer", System.Data.SqlDbType.Image);
                            while ((lenOnce = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                Array.Resize(ref buffer, lenOnce);
                                cmd.Parameters["@tempbuffer"].Value = buffer;

                                if (isFirstUpdate)
                                {
                                    cmd.CommandText =
        @"
DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 1);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 2) ;
ELSE
BEGIN
    UPDATE DocStreams SET Content.WRITE(@tempbuffer,0,null) WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
	set @row = @@ROWCOUNT
    IF @row=0 RAISERROR ('Cannot find the stream.', 16, 3);
END";
                                    isFirstUpdate = false;
                                }
                                else
                                {
                                    cmd.CommandText =
        @"DECLARE @BSN bigint
DECLARE @row int
SELECT @BSN=BSN FROM DocsToStreams WITH (NOLOCK) WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
set @row = @@ROWCOUNT
IF @row>1 RAISERROR ('More than one stream map.', 16, 4);
IF @row=0 RAISERROR ('Cannot find stream maps.', 16, 5);
ELSE
BEGIN
    UPDATE DocStreams SET Content.WRITE(@tempbuffer,null,null) WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@BSN
	set @row = @@ROWCOUNT
    IF @row=0 RAISERROR ('Cannot find the stream.', 16, 6);
END";
                                }
                                mQueryWorker.ExecuteNonQuery(cmd);
                            }
                        }
                        if (!isFirstUpdate)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 删除Connector的EBS信息
        /// 无api实现
        /// </summary>
        /// <param name="info"></param>
        [QueryReview("2012/05/02", "Kexin Guo")]
        [Obsolete("SP2013 not support EBS")]
        [DoNotNeedReview]
        public void ClearEBsInfo(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@InternalVersion", info.InternalVersion);
                cmd.CommandText = @"
UPDATE DocStreams 
SET Content = 0x0 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";
                mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the parameter of sql  statement. ")]
        /// <summary>
        /// 更新Connector file的文件size
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="size"></param>
        /// <param name="isSP1"></param>
        /// <returns></returns>
        [QueryReview("2012/05/08", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty for AllDocs")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-004")]
        public void UpdateFileSize(AveBaseItemInfo info, int size, bool isSP1, int oldSize)
        {
            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    using (SqlTransaction transaction = mQueryWorker.BeginTransaction())
                    {
                        cmd.Connection = mQueryWorker.Connection;
                        cmd.Transaction = transaction;
                        try
                        {
                            cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                            cmd.Parameters.AddWithValue("@ParentId", info.ParentId);
                            cmd.Parameters.AddWithValue("@DocId", info.GUID);
                            cmd.Parameters.AddWithValue("@Level", info.Level);
                            cmd.Parameters.AddWithValue("@UIVersion", info.Version);
                            cmd.Parameters.AddWithValue("@Size", size);
                            cmd.Parameters.AddWithValue("@Partition", 0);
                            cmd.CommandText =
         @"
--DECLARE @SiteId uniqueidentifier, @ParentId uniqueidentifier, @DocId uniqueidentifier, @Level tinyint, @UIVersion int, @Size int, @Partition tinyint
DECLARE @bsn bigint, @histVersion int, @rowc int

UPDATE AllDocs SET Size=@Size WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND Id=@DocId AND Level=@Level AND UIVersion=@UIVersion
SELECT @rowc=@@ROWCOUNT
IF @rowc>0
	SELECT @histVersion=0
ELSE 
BEGIN
	SELECT @histVersion=@UIVersion
	UPDATE AllDocVersions SET Size=@Size WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@UIVersion
	SELECT @rowc=@@ROWCOUNT
END

IF @rowc>0
BEGIN
    SELECT @bsn=BSN FROM DocsToStreams WHERE SiteId=@SiteId AND DocId=@DocId AND HistVersion=@histVersion AND Level=@Level AND Partition=@Partition
	SELECT @rowc=@@ROWCOUNT
	IF @rowc>1 RAISERROR ('More than one stream map.', 16, 1);
	IF @rowc=0 RAISERROR ('Cannot find stream maps.', 16, 2);
	ELSE
	BEGIN
		UPDATE DocStreams SET Size=@Size WHERE SiteId=@SiteId AND DocId=@DocId AND Partition=@Partition AND BSN=@bsn
		IF @@ROWCOUNT=0 RAISERROR ('Cannot find the stream.', 16, 3);
	END
END
ELSE RAISERROR ('Cannot find the document.', 16, 4);
";
                            mQueryWorker.ExecuteNonQuery(cmd);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                UpdateSiteUsage(size - oldSize, info, isSP1);
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
        /// 更新SiteUsage
        /// 效率考虑，有API实现
        /// 此方法调用存储过程，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="size"></param>
        /// <param name="info"></param>
        /// <param name="isSP1"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "cbDelta is the parameter of sql statement.")]
        [DoNotNeedReview]
        public void UpdateSiteUsage(long size, AveBaseItemInfo info, bool isSP1)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@SiteId", info.SiteId) { Direction = System.Data.ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter("@cbDelta", size) { Direction = System.Data.ParameterDirection.Input });
                cmd.Parameters.Add(new SqlParameter("@fIncrementTimestamp", 1) { Direction = System.Data.ParameterDirection.Input });

                if (isSP1)
                {
                    cmd.Parameters.Add(new SqlParameter("@DocId", info.GUID) { Direction = System.Data.ParameterDirection.Input }); //unique identifier
                }

                cmd.CommandText = "proc_QMChangeSiteDiskUsedAndContentTimestamp";
                mQueryWorker.ExecuteScalar(cmd);
            }
        }

        /// <summary>
        /// 获取DocFlag
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isEffectRecycle"></param>
        /// <returns></returns>

        [QueryReview("2012/05/08", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty and add DeleteTransactionId for AllDocs")]
        [Obsolete("SP2013 not support EBS")]
        [DoNotNeedReview]
        public int GetDocFlag(AveBaseItemInfo info, bool isEffectRecycle)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@Level", info.Level);
                cmd.Parameters.AddWithValue("@UIVersion", info.Version);

                cmd.Parameters.AddWithValue("@ParentId", info.ParentId);
                cmd.Parameters.AddWithValue("@DirName", info.ParentFolderRelativeUrl);
                cmd.Parameters.AddWithValue("@LeafName", info.Name);

                cmd.CommandText = @"SELECT DocFlags 
FROM AllDocs with(nolock) 
WHERE SiteId=@SiteId 
AND Id=@Id 
AND Level=@Level 
AND UIVersion=@UIVersion ";
                //AND DirName=@DirName 
                //AND LeafName=@LeafName";

                if (info.ParentId != Guid.Empty)
                {
                    cmd.CommandText += " AND ParentID=@ParentID ";
                }
                else
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }

                if (!string.IsNullOrEmpty(info.ParentFolderRelativeUrl))
                {
                    cmd.CommandText += @" AND DirName=@DirName ";
                }

                if (!string.IsNullOrEmpty(info.Name))
                {
                    cmd.CommandText += @" AND LeafName=@LeafName ";
                }

                if (!isEffectRecycle)
                {
                    cmd.CommandText += " AND DeleteTransactionId=0x";
                }
                else
                {
                    cmd.CommandText += " AND (DeleteTransactionId=0x or DeleteTransactionId<>0x )";
                }

                object result = mQueryWorker.ExecuteScalar(cmd);
                if (result is int)
                {
                    return (int)result;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取item的internal version
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isEffectRecbin"></param>
        /// <returns></returns>
        [QueryReview("2012/05/08", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty and add DeleteTransactionId for AllDocs")]
        [Obsolete("SP2013 not support internalVersion")]
        [DoNotNeedReview]
        public int GetCurrentItemInternalVersion(AveBaseItemInfo info, bool isEffectRecbin)
        {
            //在13中此方法不需要返回结果
            return -1;
        }

        /// <summary>
        /// 获取Item的部分content，用于判断是否是真实数据
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/05/02", "Kexin Guo")]
        [NotUsed]
        public IAveQueryDataReader GetContentFromDB(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@InternalVersion", info.InternalVersion);

                cmd.CommandText = @"SELECT cast(Content as varbinary(210)) 
FROM DocStreams WITH (INDEX(AllDocStreams_CI),NOLOCK) 
where SiteId=@SiteId 
AND Id=@Id 
AND InternalVersion=@InternalVersion";
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        /// <summary>
        /// 获取Item的部分content及RBSID，用于判断是否是真实数据
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "hyyin")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public IAveQueryDataReader GetContentAndRbsIdFromDB(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@DocId", info.GUID);
                cmd.Parameters.AddWithValue("@UIVersion", info.Version);
                cmd.Parameters.AddWithValue("@Level", info.Level);
                cmd.Parameters.AddWithValue("@Partition", 0);
                cmd.Parameters.AddWithValue("@IsCurrentVersion", info.IsCurrentVersion);
                //--DECLARE @SiteId uniqueidentifier, @DocId uniqueidentifier, @UIVersion int, @Level tinyint, @Partition tinyint, @IsCurrentVersion bit
                //set @SiteId='76b9cb70-de07-4557-b1ee-6051884f39b4'
                //set @DocId='49580d3d-b2b4-4a09-8451-4d4ef57a5adc'
                //set @UIVersion=512
                //set @Level=1
                //set @Partition=0
                //set @IsCurrentVersion=1
                cmd.CommandText = @"
DECLARE @rbsId varbinary(64), @content varbinary(32), @histVersion int, @rowc int

IF @IsCurrentVersion=1
	SET @histVersion=0
ELSE
	SET @histVersion=@UIVersion
BEGIN
    SELECT @rbsId=DS.RbsId, @content=CAST(DS.Content AS varbinary(32)) FROM 
        TVF_DocsToStreams_SiteDocHistVerLvlPart(@SiteId,@DocId,@HistVersion,@Level,@Partition) as DTS
    CROSS APPLY 
        TVF_DocStreams_CI(DTS.SiteId,DTS.DocId,DTS.Partition,DTS.BSN) as DS
	ORDER BY DS.BSN DESC

	SET @rowc=@@ROWCOUNT
END
IF (@rowc>0)
	SELECT @content Content, @rbsId RbsId,@rowc [Rows]
";
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmd));
            }
        }

        /// <summary>
        /// 获取RBSID
        /// 无API实现
        /// </summary>
        /// <param name="rbsId"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Kexin Guo")]
        [QueryReview("2012/12/17", "hyyin")]
        public byte[] GetBlobIdByRbsId(byte[] rbsId)
        {
            byte[] blobId = null;
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@RBSId", rbsId);

                cmd.CommandText = @"SELECT store_blob_id 
FROM [mssqlrbs_resources].[rbs_internal_blobs] WITH (INDEX(rbs_internal_blobs_pk),NOLOCK) 
WHERE 
collection_id =CONVERT(int,substring(@RBSId,9,4)) AND 
blob_number=CONVERT(bigint,SUBSTRING(@RBSId,1,8))";
                object obj = mQueryWorker.ExecuteScalar(cmd);
                if (obj != null)
                {
                    blobId = obj as byte[];
                }
                return blobId;
            }
        }

        /// <summary>
        /// 更新文件 的Owner，load meta info时使用
        /// 无API实现
        /// </summary>
        /// <param name="ownId"></param>
        /// <param name="modifierId"></param>
        /// <param name="info"></param>
        [QueryReview("2012/05/15", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty for AllUserData")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-007")]
        public void UpdateOwnerInfo(int ownId, int modifierId, AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@ParentId", info.ParentId);
                cmd.Parameters.AddWithValue("@Level", info.Level);

                cmd.Parameters.AddWithValue("@tp_Author", ownId);
                cmd.Parameters.AddWithValue("@tp_Editor", modifierId);

                cmd.CommandText = @"
update AllUserData 
set tp_Author=@tp_Author,
tp_Editor=@tp_Editor 
where 
tp_SiteId=@SiteId AND 
tp_DeleteTransactionId=0x AND 
tp_IsCurrentVersion=1 AND 

tp_DocId=@Id AND 
tp_CalculatedVersion=0 AND 
tp_Level=@Level AND 
tp_rowordinal=0";
                if (info.ParentId != Guid.Empty)
                {
                    cmd.CommandText += " AND tp_ParentId=@ParentId ";
                }
                else
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataOneRow);
            }
        }

        /// <summary>
        /// 获取File的CheckOut User Id
        /// API会涉及权限问题，有API实现
        /// </summary>
        /// <param name="siteID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        [Obsolete("Please use GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID) instead")]
        [DoNotNeedReview]
        public int GetCheckOutUserID(Guid siteID, Guid itemID)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", siteID);
                cmd.Parameters.AddWithValue("@Id", itemID);
                cmd.Parameters.AddWithValue("@Level", 255);

                cmd.CommandText = @"SELECT CheckoutUserId 
FROM AllDocs with(nolock) 
WHERE 
SiteId=@SiteId AND 
Id=@Id AND 
Level=@Level AND 
DeleteTransactionId=0x";

                object obj = mQueryWorker.ExecuteScalar(cmd);
                if (obj != null)
                {
                    return (int)obj;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取File的CheckOut User Id
        /// API会涉及权限问题，有API实现
        /// </summary>
        /// <param name="siteID"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        [QueryReview("2012/05/15", "Kexin Guo", true, "Rewrite")]
        [QueryReview("2012/12/17", "hyyin")]
        public int GetCheckOutUserID(Guid siteID, Guid parentID, Guid itemID)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", siteID);
                cmd.Parameters.AddWithValue("@Id", itemID);
                cmd.Parameters.AddWithValue("@Level", 255);
                cmd.Parameters.AddWithValue("@ParentId", parentID);

                cmd.CommandText = @"SELECT CheckoutUserId 
FROM AllDocs with(nolock) 
WHERE 
SiteId=@SiteId AND 
Id=@Id AND 
Level=@Level AND 
DeleteTransactionId=0x";

                if (parentID != Guid.Empty)
                {
                    cmd.CommandText += " AND ParentId=@ParentId ";
                }
                else
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                object obj = mQueryWorker.ExecuteScalar(cmd);
                if (obj != null)
                {
                    return (int)obj;
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断是否为SharePoint 2010 SP1，为了兼容RTM等早期版本的数据库
        /// 无API实现
        /// 此方法查询了view，暂时没有对其进行SQLReview
        /// </summary>
        /// <param name="siteID"></param>
        /// <returns></returns>
        public bool IsSP2010SP1(Guid siteID)
        {
            bool result = false;

            using (SqlCommand cmd = this.mQueryWorker.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WITH(NOLOCK) WHERE TABLE_NAME='AllSites' and COLUMN_NAME='RbsCollectionId'";
                object count = cmd.ExecuteScalar();
                if (count != null && (int)count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 清除RbsId,在D5 RBS stub升级到D6 EBS时使用
        /// </summary>
        /// <param name="siteID"></param>
        /// <returns></returns>
        [QueryReview("2012/05/03", "Kexin Guo")]
        [QueryReview("2012/12/18", "hyyin")]
        public void ClearRbsId(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@InternalVersion", info.InternalVersion);

                cmd.CommandText = @"
UPDATE DocStreams 
SET 
RbsId = NULL 
WHERE 
SiteId = @SiteId AND 
Id = @Id AND 
InternalVersion=@InternalVersion";

                mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            }
        }
        public object[] GetItemsInRecycleBin(AveBaseItemInfo info)
        {
            Dictionary<Guid, AveFileInfo> files = new Dictionary<Guid, AveFileInfo>();
            Dictionary<Guid, AveFolderInfo> folders = new Dictionary<Guid, AveFolderInfo>();
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@ListId", info.ListId);
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.CommandText = @"SELECT RecycleBin.ItemType, RecycleBin.DocId, RecycleBin.DirName, RecycleBin.LeafName, RecycleBin.SiteId, RecycleBin.WebId, AllDocs.Level,  AllDocs.UIVersion, AllDocs.InternalVersion 
                                    FROM RecycleBin(nolock) INNER JOIN AllDocs(nolock) on RecycleBin.DocId = AllDocs.Id  WHERE RecycleBin.SiteId=@SiteId AND RecycleBin.ListId=@ListID AND (RecycleBin.ItemType=1 OR RecycleBin.ItemType=5)";
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmd))
                {
                    while (dr.Read())
                    {
                        int i = 0;
                        byte type = dr.GetByte(i++);
                        Guid itemID = dr.GetGuid(i++);
                        string serverRelativeURL = string.Format("/{0}/{1}", dr.GetString(i++), dr.GetString(i++)).TrimEnd('/');
                        Guid siteId = dr.GetGuid(i++);
                        Guid webId = dr.GetGuid(i++);
                        int level = dr.GetByte(i++);
                        if (type == 5)
                        {
                            if (!folders.ContainsKey(itemID))
                            {
                                AveFolderInfo folderInfo = new AveFolderInfo()
                                {
                                    ServerRelativeUrl = serverRelativeURL,
                                    SiteId = siteId,
                                    WebId = webId,
                                    Level = level
                                };
                                folders.Add(itemID, folderInfo);
                            }
                        }
                        else
                        {
                            int uiVersion = dr.GetInt32(i++);
                            int interanlVersion = dr.GetInt32(i++);
                            if (!files.ContainsKey(itemID))
                            {
                                AveFileInfo fileInfo = new AveFileInfo()
                                {
                                    ServerRelativeUrl = serverRelativeURL,
                                    SiteId = siteId,
                                    WebId = webId,
                                    Version = uiVersion,
                                    InternalVersion = interanlVersion,
                                    Level = level
                                };
                                files.Add(itemID, fileInfo);
                            }
                        }
                    }
                }
            }
            return new object[] { folders, files };
        }

        public Dictionary<Guid, List<AveBaseItemInfo>> GetVersionsInRecycleBin(AveBaseItemInfo info)
        {
            Dictionary<Guid, List<AveBaseItemInfo>> versions = new Dictionary<Guid, List<AveBaseItemInfo>>();
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@ListId", info.ListId);
                cmd.CommandText =
                  @"SELECT Id, UIVersion, InternalVersion, Level, version.Size FROM RecycleBin(nolock)
                    INNER JOIN AllDocVersions version(nolock) ON  version.SiteId=@SiteId AND version.Id=RecycleBin.DocId AND version.UIVersion=RecycleBin.DocVersionId
                    WHERE ListId=@ListId AND ItemType = 2";
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmd))
                {
                    while (dr.Read())
                    {
                        AveBaseItemInfo versionInfo = new AveBaseItemInfo()
                        {
                            GUID = dr.GetGuid(0),
                            Version = dr.GetInt32(1),
                            InternalVersion = dr.GetInt32(2),
                            Level = dr.GetByte(3),
                            Length = dr.GetInt32(4),
                        };

                        List<AveBaseItemInfo> versionInfos;
                        if (!versions.TryGetValue(versionInfo.GUID, out versionInfos))
                        {
                            versionInfos = new List<AveBaseItemInfo>();
                            versions.Add(versionInfo.GUID, versionInfos);
                        }
                        versionInfos.Add(versionInfo);
                    }
                }
            }
            return versions;
        }

        [QueryReview("2012/12/18", "hyyin")]
        public List<AveBaseItemInfo> GetItemVersionsInRecycleBin(AveBaseItemInfo info)
        {
            List<AveBaseItemInfo> versions = new List<AveBaseItemInfo>();
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.CommandText = @"SELECT UIVersion, InternalVersion, Level, Size FROM AllDocVersions(nolock) WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId<>0x";
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmd))
                {
                    while (dr.Read())
                    {
                        AveBaseItemInfo itemInfo = new AveBaseItemInfo()
                        {
                            Version = dr.GetInt32(0),
                            InternalVersion = dr.GetInt32(1),
                            Level = dr.GetByte(2),
                            Length = dr.GetInt32(3)
                        };
                        versions.Add(itemInfo);
                    }
                }
            }
            return versions;
        }

        /// <summary>
        /// Correct the DocFlags if the DocFlags is wrong.
        /// </summary>
        /// <param name="isStub">True if SPFile content is stub.</param>
        /// <returns>True if the DocFlags was wrong, and has been corrected.</returns>
        [QueryReview("Conn-010")]
        public bool CorrectDocFlags(AveBaseItemInfo info, bool isStub)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@ParentId", info.ParentId);
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@Level", info.Level);
                cmd.Parameters.AddWithValue("@UIVersion", info.Version);

                string format =
                    @"UPDATE AllDocs SET {0} WHERE Id=@Id AND Level=@Level AND UIVersion=@UIVersion AND DeleteTransactionId=0x AND {1};
                      UPDATE AllDocVersions SET {0} WHERE SiteId=@SiteId AND Id=@Id AND UIVersion=@UIVersion AND {1};";
                if (isStub)
                {
                    cmd.CommandText = string.Format(format, "DocFlags=DocFlags|65536", "DocFlags&65536<>65536");
                }
                else
                {
                    cmd.CommandText = string.Format(format, "DocFlags=DocFlags&(~65536)", "DocFlags&65536=65536");
                }

                int result = mQueryWorker.ExecuteNonQuery(cmd);
                return (result > 0);
            }
        }
        [DoNotNeedReview("has confirmed with connector")]
        public bool ObjectExists(AveBaseItemInfo info, int objectType)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                string cmdText;
                switch (objectType)
                {
                    case 0: //File or Folder less siteid
                        cmdText = "SELECT COUNT(*) FROM AllDocs(nolock) WHERE SiteID=@SiteId and Id=@Id";
                        cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                        cmd.Parameters.AddWithValue("@Id", info.GUID);
                        break;

                    case 1: //List less siteid
                        cmdText = "SELECT COUNT(*) FROM AllLists(nolock) WHERE tp_SiteId=@SiteID AND tp_WebId=@WebID AND tp_ID=@ListID";
                        cmd.Parameters.AddWithValue("@SiteID", info.SiteId);
                        cmd.Parameters.AddWithValue("@WebID", info.WebId);
                        cmd.Parameters.AddWithValue("@ListID", info.ListId);
                        break;

                    case 3: //Web for SP1
                        cmdText = "SELECT COUNT(*) FROM AllWebs(nolock) WHERE SiteId=@SiteId and Id=@WebId";
                        cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                        cmd.Parameters.AddWithValue("@WebId", info.WebId);
                        break;

                    case 5://Site for SP1
                        cmdText = "SELECT COUNT(*) FROM AllSites(nolock) WHERE Id=@SiteId";
                        cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                        break;

                    default:
                        throw new ArgumentException("Invalid object type: " + objectType);
                }

                cmd.CommandText = cmdText;
                int count = (int)cmd.ExecuteScalar();

                return (count > 0);
            }
        }
        /// <summary>
        /// 获取File或者Folder的Recycle状态
        /// </summary>
        /// <returns>
        /// 0——In SharePoint
        /// 1——In SharePoint Recycle
        /// 2——Permanent Deleted in SharePoint
        /// </returns>
        public int GetItemRecycleStatus(AveBaseItemInfo info)
        {
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@Id", info.GUID);
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.CommandText = "SELECT top 1 DeleteTransactionId FROM AllDocs(nolock) WHERE SiteID = @SiteId AND Id=@Id ORDER BY UIVersion DESC";

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return ((byte[])result).Length > 0 ? 1 : 0;
                }
                return 2;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public void UpdateStreamSchema(AveBaseItemInfo info, byte streamSchema)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                //--返回0表示正常更新StreamSchema，返回1表示删除了DocsToStream和DocStreams表中的RBS记录外层需要删除RBS记录,2表示既没有删除RBS记录，也没有更新StreamSchema

                //declare  @SiteId uniqueidentifier,
                //    @ParentId uniqueidentifier,
                //    @DocId uniqueidentifier,
                //    @Partition tinyint,
                //    @HistVersion int,
                //    @Level tinyint,

                //    @ContentVersion int,           
                //    @NextBSN bigint,               
                //    @StreamIds tvpArrayOfBigInts,
                //    @ChunkSize int,
                //    @Rows int,
                //    @Ret int,

                //    @RbsId varbinary(64),
                //    @BSN int,
                //    @StreamSchema int

                //set @SiteId='854344A7-5257-4355-B86B-D21A59D349FE'
                //set @ParentId='78FE5ECD-0FE7-47D9-8E0B-5DC925687DB8'
                //set @DocId='6F0459CA-8767-4150-AFD9-C64EE693BD7F'
                //set @Partition=0 --CONTENT_PARTITION
                //set @HistVersion=0
                //set @Level=1
                //set @ContentVersion=1
                //set @NextBSN=353
                //set @ChunkSize=0
                //set @StreamSchema=1
                //cmd.CommandText = @"UPDATE AllDocs SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND Level=@Level";
                cmd.CommandText = @"
declare @Partition tinyint,
        @Rows int,
        @HistVersion int,   
        @RbsId varbinary(64),
        @BSN int

set @HistVersion=0
set @Partition=0

SELECT @RbsId= DS.RbsId,@BSN= DS.BSN FROM 
        TVF_DocsToStreams_SiteDocHistVerLvlPart(@SiteId,@DocId,@HistVersion,@Level,@Partition) as DTS
CROSS APPLY 
        TVF_DocStreams_CI(DTS.SiteId,DTS.DocId,DTS.Partition,DTS.BSN) as DS
ORDER BY DS.BSN DESC

SET @Rows=@@ROWCOUNT

IF (@Rows=0)
   SET @Ret=2
ELSE IF(@RbsId IS NOT NULL)
   IF (@Rows=1)
	   BEGIN
		   UPDATE AllDocs  SET StreamSchema=@StreamSchema  WHERE Id=@DocId AND SiteId=@SiteId and Level=@Level and ParentId=@ParentId AND  DeleteTransactionId=0x
		   SET @Ret=0
	   END
   ELSE
	   BEGIN
			BEGIN TRAN
				DELETE 
				  DTS
				FROM 
				  TVF_DocsToStreams_CI(@SiteId,@DocId,@HistVersion,@Level,@Partition,@BSN) as DTS
				DELETE
					DS
				FROM 
					TVF_DocStreams_CI(@SiteId,@DocId,@Partition,@BSN) as DS
			IF @@ERROR <> 0
			   BEGIN
				  ROLLBACK TRAN
				  SET @Ret=2
			   END
			ELSE
			   BEGIN
				  COMMIT TRAN
				  SET @Ret=1
			   END
		END
ELSE
   SET @Ret=2";
                cmd.Parameters.AddWithValue("@StreamSchema", streamSchema);
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@DocId", info.GUID);
                cmd.Parameters.AddWithValue("@Level", info.Level);
                cmd.Parameters.AddWithValue("@ParentId", info.ParentId);
                cmd.Parameters.Add(new SqlParameter("@Ret", SqlDbType.Int) { Direction = ParameterDirection.Output });

                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        public void UpdateVersionStreamSchema(AveBaseItemInfo info, byte streamSchema)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@StreamSchema", streamSchema);
                cmd.Parameters.AddWithValue("@SiteId", info.SiteId);
                cmd.Parameters.AddWithValue("@DocId", info.GUID);
                cmd.Parameters.AddWithValue("@Level", info.Level);
                if (info.IsCurrentVersion)
                {
                    cmd.Parameters.AddWithValue("@histVersion", 0);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@histVersion", info.Version);
                }
                cmd.CommandText = @"
IF @histVersion =0
  UPDATE AllDocs SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND Level=@Level
ELSE
  UPDATE AllDocVersions SET StreamSchema=@StreamSchema WHERE SiteId=@SiteId AND Id=@DocId AND UIVersion=@histVersion
";
                mQueryWorker.ExecuteNonQuery(cmd);
            }
        }

        /// <summary>
        /// 获取RecycleItem的MetaInfo，如果有多条记录，取最高Version
        /// </summary>
        public string GetRecycleItemProperties(AveBaseItemInfo itemInfo)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
            mQueryWorker.AddParameter("@ParentId", itemInfo.ParentId);
            mQueryWorker.AddParameter("@Id", itemInfo.GUID);
            mQueryWorker.AddParameter("@Level", itemInfo.Level);

            StringBuilder cmdString = new StringBuilder();
            cmdString.Append(@"SELECT top 1 MetaInfo FROM AllDocs with(nolock) WHERE SiteId=@SiteId AND Id=@Id AND (DeleteTransactionId >0x OR DeleteTransactionId<0x) ");
            if (itemInfo.ParentId != Guid.Empty)
            {
                cmdString.Append("AND ParentId = @ParentId ");
            }
            if (itemInfo.Level > 0)
            {
                cmdString.Append("AND Level = @Level ");
            }
            cmdString.Append("Order BY UIVersion DESC");

            object result = mQueryWorker.ExecuteScalar(cmdString.ToString());
            if (result == null)
            {
                throw new Exception("Cannot find the meta info of the item in recycle bin.");
            }
            else if (result == DBNull.Value)
            {
                return string.Empty;
            }

            return AveCompressedUtility.GetTCompressedString((byte[])result);
        }

        #region IAveConnectorQueryService Members

        public string GetRecycleFileDiskname(AveBaseItemInfo info, string columnName)
        {
            throw new NotImplementedException();
        }

        public string GetRecycleItemContentTypeId(AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        public bool UpdateRecycleItemProperty(AveBaseItemInfo itemInfo, byte[] metaInfoBytes)
        {
            throw new NotImplementedException();
        }

        #endregion

        public List<Guid> GetConnectorSiteCollectionIDs(string featureId)
        {
            List<Guid> result = new List<Guid>();
            if (string.IsNullOrEmpty(featureId))
            {
                return result;
            }
            using (SqlCommand cmd = mQueryWorker.CreateCommand())
            {
                cmd.Parameters.AddWithValue("@FeatureId", featureId);

                cmd.CommandText = @"SELECT SiteId FROM Features (NOLOCK)  where FeatureId = @FeatureId";

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid siteCollectionId = reader.GetGuid(0);
                        result.Add(siteCollectionId);
                    }
                }
            }
            return result;
        }
    }
}
