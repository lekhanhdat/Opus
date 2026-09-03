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
    using Common;
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using static SP2016UpdateQueryString;
    using static SP2016SelectQueryString;

    internal partial class AveQueryService : IAveConnectorQueryService
    {

        /// <summary>
        /// 保存Connector Media Library的缩略图等信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isVideo"></param>
        [QueryReview("2012/05/03", "Kexin Guo")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-001")]
        public void SaveThumbnail(Hashtable obj, bool isVideo = false)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameters(obj);
                var snapShotIsNull = string.IsNullOrEmpty(obj["@SnapShot"]?.ToString());
                mQueryWorker.Command.CommandText = UpdateThumbnailForConnector_Update_AllUserData(snapShotIsNull, isVideo);
                mQueryWorker.ExecuteNonQuery(mQueryWorker.Command, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataOneRow);
            });
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
            IAveQueryDataReader queryDataReader = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                queryDataReader= new AveQueryDataReader(mQueryWorker.ExecuteReader(GetDocInfoByDocIdForConnector_Select_AllDocs));
            });
            return queryDataReader;
        }

        /// <summary>
        /// Connector Meta info以及大文件相关的实现,无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fs"></param>
        /// <param name="isRbs"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are parameter of the sql statement. ")]
        [QueryReview("2012/05/02", "Kexin Guo")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-002")]
        public void UpdateDBContent(AveBaseItemInfo info, Stream fs, bool isRbs)
        {
            ExceptionHandlingScope(() =>
            {
                using (var cmd = mQueryWorker.CreateCommand())
                {
                    using (var transaction = mQueryWorker.BeginTransaction())
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
                            cmd.Parameters.AddWithValue("@histVersion", info.IsCurrentVersion ? 0 : info.Version);
                            if (isRbs)
                            {
                                cmd.CommandText = UpdateContentInDbForLargeFileV1_Update_DocsToStreams_DocStreams_AllDocs_AllDocVersions;
                                mQueryWorker.ExecuteNonQuery(cmd);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Id", info.GUID);
                                cmd.Parameters.AddWithValue("@InternalVersion", info.InternalVersion);
                                cmd.CommandText = UpdateContentInDbForLargeFileV2_Update_DocStreams;
                                mQueryWorker.ExecuteNonQuery(cmd, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
                            }

                            var buffer = new byte[1024*1024];
                            var isFirstUpdate = true;
                            using (fs)
                            {
                                cmd.Parameters.Add("@tempbuffer", SqlDbType.Image);
                                int lenOnce;
                                while ((lenOnce = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    Array.Resize(ref buffer, lenOnce);
                                    cmd.Parameters["@tempbuffer"].Value = buffer;

                                    if (isFirstUpdate)
                                    {
                                        cmd.CommandText = UpdateContentInDbForLargeFileV3_Update_DocStreams_DocsToStreams;
                                        isFirstUpdate = false;
                                    }
                                    else
                                    {
                                        cmd.CommandText =UpdateContentInDbForLargeFileV4_Update_DocStreams_DocsToStreams;
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
            });
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
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@InternalVersion", info.InternalVersion);
                mQueryWorker.Command.CommandText = ClearEbsInfo_Update_DocStreams;
                mQueryWorker.ExecuteNonQuery(mQueryWorker.Command, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            });
        }

        /// <summary>
        /// 更新Connector file的文件size,无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="size"></param>
        /// <param name="isSP1"></param>
        /// <param name="oldSize"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the parameter of sql  statement. ")]
        [QueryReview("2012/05/08", "Kexin Guo", true, "add Warning when ParentId == Guid.Empty for AllDocs")]
        [QueryReview("2012/12/17", "hyyin")]
        [QueryReview("Conn-004")]
        public void UpdateFileSize(AveBaseItemInfo info, int size, bool isSP1, int oldSize)
        {
            ExceptionHandlingScope(() =>
            {
                using (var cmd = mQueryWorker.CreateCommand())
                {
                    using (var transaction = mQueryWorker.BeginTransaction())
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
                            cmd.CommandText = UpdateFileContentSize_Update_AllDocs_AllDocVersions_DocsToStreams_DocStreams;
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
            });
            UpdateSiteUsage(size - oldSize, info, isSP1);
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
        //todo:wbhu,这个方法外围应该没有,可以考虑从接口去掉,改成private 方法
        public void UpdateSiteUsage(long size, AveBaseItemInfo info, bool isSP1)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.StoredProcedure);
                mQueryWorker.AddParameter("@SiteId", info.SiteId, ParameterDirection.Input);
                mQueryWorker.AddParameter("@cbDelta", size, ParameterDirection.Input);
                mQueryWorker.AddParameter("@fIncrementTimestamp", 1, ParameterDirection.Input);
                //todo:wbhu,isPS1判断是否需要去掉,
                if (isSP1)
                {
                    mQueryWorker.AddParameter("@DocId", info.GUID, ParameterDirection.Input); //unique identifier
                }
                mQueryWorker.ExecuteScalar(UpdateSiteUsage_UPDATE_proc_QMChangeSiteDiskUsedAndContentTimestamp);
            });
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
            var docFlag = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@UIVersion", info.Version);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@DirName", info.ParentFolderRelativeUrl);
                mQueryWorker.AddParameter("@LeafName", info.Name);
                var commandText = GetDocFlagById_Select_AllDocs(info.ParentId, info.ParentFolderRelativeUrl, info.Name, isEffectRecycle);
                if (info.ParentId == Guid.Empty)
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                var result = mQueryWorker.ExecuteScalar(commandText);
                if (result is int)
                {
                    docFlag = (int) result;
                }
            });
            return docFlag;
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
        //todo:wbhu,看看是否可以去掉这个接口
        public IAveQueryDataReader GetContentFromDB(AveBaseItemInfo info)
        {
            IAveQueryDataReader contentReader = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@InternalVersion", info.InternalVersion);
                contentReader = new AveQueryDataReader(mQueryWorker.ExecuteReader(GetParticalContentFromDbForConnector_Select_DocStreams));
            });
            return contentReader;
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
            IAveQueryDataReader dataReader = null;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@DocId", info.GUID);
                mQueryWorker.AddParameter("@UIVersion", info.Version);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@Partition", 0);
                mQueryWorker.AddParameter("@IsCurrentVersion", info.IsCurrentVersion);
                dataReader = new AveQueryDataReader(mQueryWorker.ExecuteReader(GetContentAndRbsIdFromDB_Select_TVF_DocsToStreams_SiteDocHistVerLvlPart_TVF_DocStreams_CI));
            });
            return dataReader;
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
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@RBSId", rbsId);
                var obj = mQueryWorker.ExecuteScalar(GetStoreBlobIdByRBSId_Select_mssqlrbs_resources_rbs_internal_blobs);
                if (obj != null)
                {
                    blobId = obj as byte[];
                }
            });
            return blobId;
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
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@tp_Author", ownId);
                mQueryWorker.AddParameter("@tp_Editor", modifierId);
                if (info.ParentId == Guid.Empty)
                {
                    logger.Warn("info.ParentId equals to Guid.Empty, may be not initialized");
                }
                var commandText = UpdateFileAuthorEditor_Update_AllUserData(info.ParentId);
                mQueryWorker.ExecuteNonQuery(commandText, VersionOption.OneItemOrVersion, RowOrdinalOption.AllUserDataOneRow);

            });
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
            //16不需要检查
            return true;
        }

        /// <summary>
        /// 清除RbsId,在D5 RBS stub升级到D6 EBS时使用
        /// </summary>
        /// <param name="info"></param>
        [QueryReview("2012/05/03", "Kexin Guo")]
        [QueryReview("2012/12/18", "hyyin")]
        public void ClearRbsId(AveBaseItemInfo info)
        {
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@InternalVersion", info.InternalVersion);
                mQueryWorker.ExecuteNonQuery(SetRbsIdToNull_Update_DocStreams, VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            });
        }

        public object[] GetItemsInRecycleBin(AveBaseItemInfo info)
        {
            var files = new Dictionary<Guid, AveFileInfo>();
            var folders = new Dictionary<Guid, AveFolderInfo>();
            var result = new object[] {folders, files};
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@ListId", info.ListId);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                using (var dr = mQueryWorker.ExecuteReader(GetItemsInRecycleBin_Select_RecycleBin_AllDocs))
                {
                    while (dr.Read())
                    {
                        var i = 0;
                        var type = dr.GetByte(i++);
                        var itemId = dr.GetGuid(i++);
                        var serverRelativeUrl = $"/{dr.GetString(i++)}/{dr.GetString(i++)}".TrimEnd('/');
                        var siteId = dr.GetGuid(i++);
                        var webId = dr.GetGuid(i++);
                        var level = dr.GetByte(i++);
                        if (type == 5)
                        {
                            if (!folders.ContainsKey(itemId))
                            {
                                var folderInfo = new AveFolderInfo()
                                {
                                    ServerRelativeUrl = serverRelativeUrl,
                                    SiteId = siteId,
                                    WebId = webId,
                                    Level = level
                                };
                                folders.Add(itemId, folderInfo);
                            }
                        }
                        else
                        {
                            var uiVersion = dr.GetInt32(i++);
                            var interanlVersion = dr.GetInt32(i++);
                            if (!files.ContainsKey(itemId))
                            {
                                var fileInfo = new AveFileInfo()
                                {
                                    ServerRelativeUrl = serverRelativeUrl,
                                    SiteId = siteId,
                                    WebId = webId,
                                    Version = uiVersion,
                                    InternalVersion = interanlVersion,
                                    Level = level
                                };
                                files.Add(itemId, fileInfo);
                            }
                        }
                    }
                }
            });
            return result;
        }

        public Dictionary<Guid, List<AveBaseItemInfo>> GetVersionsInRecycleBin(AveBaseItemInfo info)
        {
            var versions = new Dictionary<Guid, List<AveBaseItemInfo>>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ListId", info.ListId);
                using (var dr = mQueryWorker.ExecuteReader(GetVersionsInRecycleBin_Select_RecycleBin_AllDocVersions))
                {
                    while (dr.Read())
                    {
                        var versionInfo = new AveBaseItemInfo()
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
            });
            return versions;
        }

        [QueryReview("2012/12/18", "hyyin")]
        public List<AveBaseItemInfo> GetItemVersionsInRecycleBin(AveBaseItemInfo info)
        {
            var versions = new List<AveBaseItemInfo>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                using (var dr = mQueryWorker.ExecuteReader(GetItemVersionsById_Select_AllDocVersions))
                {
                    while (dr.Read())
                    {
                        var itemInfo = new AveBaseItemInfo()
                        {
                            Version = dr.GetInt32(0),
                            InternalVersion = dr.GetInt32(1),
                            Level = dr.GetByte(2),
                            Length = dr.GetInt32(3)
                        };
                        versions.Add(itemInfo);
                    }
                }
            });
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
            var result = -1;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@UIVersion", info.Version);
                var command= UpdateFileDocFlag_Update_AllDocs_AllDocVersions(isStub);
                result = mQueryWorker.ExecuteNonQuery(command);
            });
            return (result > 0);
        }

        [DoNotNeedReview("has confirmed with connector")]
        public bool ObjectExists(AveBaseItemInfo info, int objectType)
        {
            var exist = false;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                string cmdText;
                switch (objectType)
                {
                    case 0: //File or Folder less siteid
                        cmdText = GetItemCountById_Select_AllDocs;
                        mQueryWorker.AddParameter("@Id", info.GUID);
                        break;

                    case 1: //List less siteid
                        cmdText = GetListCountById_Select_AllLists;
                        mQueryWorker.AddParameter("@WebID", info.WebId);
                        mQueryWorker.AddParameter("@ListID", info.ListId);
                        break;

                    case 3: //Web for SP1
                        cmdText = GetWebCountById_Select_AllWebs;
                        mQueryWorker.AddParameter("@WebId", info.WebId);
                        break;

                    case 5://Site for SP1
                        cmdText = GetSiteCountById_Select_AllSites;
                        break;

                    default:
                        throw new ArgumentException("Invalid object type: " + objectType);
                }
                var count = (int)mQueryWorker.ExecuteScalar(cmdText);
                exist = count > 0;
            });
            return exist;
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
            var status = 2;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@Id", info.GUID);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                var result = mQueryWorker.ExecuteScalar(GetItemDeleteTransactionIdById_Select_AllDocs);
                if (result != null)
                {
                    status=((byte[])result).Length > 0 ? 1 : 0;
                }
            });
            return status;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public void UpdateStreamSchema(AveBaseItemInfo info, byte streamSchema)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@StreamSchema", streamSchema);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@DocId", info.GUID);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@ParentId", info.ParentId);
                mQueryWorker.AddParameterWithType("@Ret", SqlDbType.Int,ParameterDirection.Output);
                var updateCommand = UpdateStreamSchema_Update_AllDocs_TVF_DocsToStreams_SiteDocHistVerLvlPart_TVF_DocStreams_CI_TVF_DocsToStreams_CI;
                //--返回0表示正常更新StreamSchema
                //返回1表示删除了DocsToStream和DocStreams表中的RBS记录外层需要删除RBS记录
                //2表示既没有删除RBS记录，也没有更新StreamSchema
                mQueryWorker.ExecuteNonQuery(updateCommand);
            });
        }

        public void UpdateVersionStreamSchema(AveBaseItemInfo info, byte streamSchema)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@StreamSchema", streamSchema);
                mQueryWorker.AddParameter("@SiteId", info.SiteId);
                mQueryWorker.AddParameter("@DocId", info.GUID);
                mQueryWorker.AddParameter("@Level", info.Level);
                mQueryWorker.AddParameter("@histVersion", info.IsCurrentVersion ? 0 : info.Version);
                mQueryWorker.ExecuteNonQuery(UpdateStreamSchemaByDocId_Update_AllDocs_AllDocVersions);
            });
        }

        /// <summary>
        /// 获取RecycleItem的MetaInfo，如果有多条记录，取最高Version
        /// </summary>
        public string GetRecycleItemProperties(AveBaseItemInfo itemInfo)
        {
            var properties = string.Empty;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@SiteId", itemInfo.SiteId);
                mQueryWorker.AddParameter("@ParentId", itemInfo.ParentId);
                mQueryWorker.AddParameter("@Id", itemInfo.GUID);
                mQueryWorker.AddParameter("@Level", itemInfo.Level);
                var cmdString = GetRecycleItemProperties_Select_AllDocs(itemInfo.ParentId, itemInfo.Level);
                var result = mQueryWorker.ExecuteScalar(cmdString);
                if (result == null)
                {
                    throw new Exception("Cannot find the meta info of the item in recycle bin.");
                }
                if (result != DBNull.Value)
                {
                    properties = AveCompressedUtility.GetTCompressedString((byte[]) result);
                }
            });
            return properties;
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
            var result = new List<Guid>();
            if (string.IsNullOrEmpty(featureId))
            {
                return result;
            }
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@FeatureId", featureId);
                using (var reader = mQueryWorker.ExecuteReader(GetSiteIdCollectionByFeatureId_Select_Features))
                {
                    while (reader.Read())
                    {
                        var siteCollectionId = reader.GetGuid(0);
                        if (!result.Contains(siteCollectionId))
                        {
                            result.Add(siteCollectionId);
                        }
                    }
                }
            });
            return result;
        }
    }
}
