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

    using System;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using static SP2016SelectQueryString;
    using System.Data.SqlClient;

    internal partial class AveQueryService : IAveCentralAdminQueryService
    {
        #region Central Admin

        #region Central Admin private methods

        internal class AveSpaceInfo
        {
            public double TotalSize => FreeSize + UsedSize;
            public double FreeSize { get; set; }
            public double UsedSize { get; set; }
        }

        /// <summary>
        /// 获取当前DB space的使用情况
        /// </summary>
        /// <returns></returns>
        private AveSpaceInfo GetCurrentDBSpaceUsageInfo()
        {
            var dbSizeInfo = new AveSpaceInfo();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                var getCurrentDBSpaceInfo = GetCurrentDBSpaceInfo_Select_sp_SpaceUsed;
                using (var sr = mQueryWorker.ExecuteReader(getCurrentDBSpaceInfo))
                {
                    while (sr.Read())
                    {
                        var totalMB = Convert.ToDouble(sr.GetString(1).Split(' ')[0], CultureInfo.InvariantCulture);
                        dbSizeInfo.FreeSize = Convert.ToDouble(sr.GetString(2).Split(' ')[0], CultureInfo.InvariantCulture);
                        dbSizeInfo.UsedSize = totalMB - dbSizeInfo.FreeSize;
                        break;
                    }
                }
            });
            return dbSizeInfo;
        }

        /// <summary>
        /// 获取当前DB所在磁盘的剩余空间，如果DB文件在多个磁盘上，返回的是剩余磁盘空间的总和
        /// </summary>
        /// <returns></returns>
        private double GetDiskFreeSizeOfCurrentDB()
        {
            double diskFreesize = 0;
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                var getDBUsedDiskCommand = GetDBUsedDisk_Select_sp_helpfile;
                var logicalDrives = new List<string>();
                using (var sr = mQueryWorker.ExecuteReader(getDBUsedDiskCommand))
                {
                    while (sr.Read())
                    {
                        var logicalDrive = sr.GetString(2).ToLower(CultureInfo.InvariantCulture);
                        logicalDrive = logicalDrive.Substring(0, 1);
                        if (!logicalDrives.Contains(logicalDrive))
                        {
                            logicalDrives.Add(logicalDrive);
                        }
                    }
                }
                var getDiskFreeSpaceCommand = GetDiskFreeSpace_Select_xp_FixedDrives;
                using (var sr = mQueryWorker.ExecuteReader(getDiskFreeSpaceCommand))
                {
                    while (sr.Read())
                    {
                        var logicalDrive = sr.GetString(0).ToLower(CultureInfo.InvariantCulture);
                        var freeSpace = sr.GetInt32(1);
                        if (logicalDrives.Contains(logicalDrive))
                        {
                            diskFreesize += freeSpace;
                        }
                    }
                }
            });
            return diskFreesize;
        }

        /// <summary>
        /// 获取webapp下site id的集合（configDB中的记录）
        /// </summary>
        /// <param name="webAppId"></param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        private List<Guid> GetSiteIdCollectionByWebAppId(Guid webAppId, string cmdText)
        {
            var siteIds = new List<Guid>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@ApplicationId", webAppId);
                using (var reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    siteIds = GetGuidValues(reader, 0);
                }
            });
            return siteIds;
        }

        /// <summary>
        /// 递归调用，可能存在效率问题，需要考虑是否可以使用
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private List<string> GetAllPagesByParentId(Guid siteId, Guid parentId)
        {
            var result = new List<string>();
            var ids = new List<Guid>();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentId);
            using (var reader = mQueryWorker.ExecuteReader(GetSubFolderAndPagesInFolder_Select_AllDocs))
            {
                while (reader.Read())
                {
                    try
                    {
                        var type = Convert.ToInt32(reader[3]);
                        if (type == 1)
                        {
                            ids.Add(reader.GetGuid(0));
                        }
                        else
                        {
                            result.Add($"{reader.GetString(1)}/{reader.GetString(2)}");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Get All Page Error:{0}. Reason:{1}.", e.Message, e);
                    }
                }
            }

            foreach (var subId in ids)
            {
                result.AddRange(GetAllPage(siteId, subId));
            }
            return result;
        }

        private Dictionary<Guid, string> GetAllPageInternal(IDictionary<string,object> parameters,string queryString)
        {
            var result = new Dictionary<Guid, string>();
            ExceptionHandlingScope(() =>
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameters(parameters);
                using (var reader = mQueryWorker.ExecuteReader(queryString))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        //去重
                        if (!result.ContainsKey(id))
                        {
                            result.Add(id, $"{reader.GetString(1)}/{reader.GetString(2)}");
                        }
                    }
                }
            });
            return result;
        }

        #endregion Central Admin private methods

        /// <summary>
        /// 获取数据库所在磁盘的剩余空间和该数据库占用空间和可用空间,无API实现.
        /// </summary>
        /// <param name="usedSize">DB使用空间，单位MB</param>
        /// <param name="freeSize">DB可用空间，单位MB</param>
        /// <param name="diskFreesize">磁盘剩余空间，单位MB</param>
        public void GetDBSize(out double usedSize, out double freeSize, out double diskFreesize)
        {
            diskFreesize = GetDiskFreeSizeOfCurrentDB();
            var dbSpaceInfo = GetCurrentDBSpaceUsageInfo();
            usedSize = dbSpaceInfo.UsedSize;
            freeSize = dbSpaceInfo.FreeSize;
        }

        /// <summary>
        /// 获取SQL 服务器所在机器的HostName,API方式有缺陷，有API实现.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        //todo:wbhu,参数没有任何作用
        public string GetDBServerName()
        {
            var serverName = string.Empty;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetDBServerName_Select_SERVERPROPERTY;
                serverName = mQueryWorker.ExecuteScalar(cmdText) as string;
            });
            return serverName;
        }

        /// <summary>
        /// 根据传入的siteIdFilter 条件查询出对应的site 信息，以IAveQueryDataReader形式返回查询结果
        /// </summary>
        /// <param name="siteIdFilter"></param>
        /// <param name="appUrl"></param>
        /// <param name="appSuffix"></param>
        /// <returns></returns>
        public IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix)
        {
            IAveQueryDataReader result = null;
            var commandText = GetSiteInfoBySiteIdFilterCondition_Select_AllSites_AllWebs(siteIdFilter);
            ExceptionHandlingScope(() =>
            {
                using (var cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue(@"appSuffix", appSuffix);
                    cmd.Parameters.AddWithValue(@"appUrl", appUrl);
                    result = new AveQueryDataReader(cmd.ExecuteReader());
                }
            });
            return result;
        }


        /// <summary>
        /// 根据webapp Id获取一个WebApp下所有SiteCollection的Id拼接的query条件,效率考虑，有API实现.（查询ConfigDB） 
        /// </summary>
        /// <param name="webAppId"></param>
        /// <returns></returns>
        //todo:wbhu,方法名不合理，方法名含义应该是返回一个集合，但是实际上却是拼接出了一个condition，看着像是配合GetOrphanSite使用的，需要再review
        [QueryReview("2012/05/17", "Long Liang")]
        public string GetSiteIds(Guid webAppId)
        {
            var cmdText = GetSiteIdCollectionByWebAppId_Select_SiteMap;
            var siteIdNotInCondition = new StringBuilder();
            var idCollection = GetSiteIdCollectionByWebAppId(webAppId, cmdText);
            if (idCollection != null && idCollection.Count > 0)
            {
                siteIdNotInCondition.Append(" and s.id not in (");
                var isFirst = true;
                idCollection.ForEach(id =>
                {
                    if (!isFirst)
                    {
                        siteIdNotInCondition.Append(",");
                    }
                    else
                    {
                        isFirst = false;
                    }
                    siteIdNotInCondition.Append($"'{id}'");
                });
                siteIdNotInCondition.Append(") ");
            }
            return siteIdNotInCondition.ToString();
        }

        /// <summary>
        /// 获取无权限的Users和Groups,无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <param name="searchUsers"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        [QueryReview("2012/05/17", "Long Liang", true, "should have check the group member permission")]
        public IAveQueryDataReader GetSiteNoPermissionAccounts(Guid siteId, Guid scopeId, List<string> searchUsers)
        {
            IAveQueryDataReader result = null;
            ExceptionHandlingScope(() =>
            {
                var cmdstring = GetSiteNoPermisssionAccountByCondition_Select_UserInfo_RoleAssignment_GroupMembership_Groups(searchUsers);
                using (var command = mQueryWorker.CreateCommand())
                {
                    command.Parameters.AddWithValue("@siteId", siteId);
                    command.Parameters.AddWithValue("@scopeId", scopeId);
                    command.CommandText = cmdstring;
                    result = new AveQueryDataReader(command.ExecuteReader());
                }
            });
            return result;
        }

        /// <summary>
        /// 查询Web下最顶端Navigation关联的Document信息
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        //todo:wbhu,1.方法实现基本上是重写了，需要review和测试 ，2.method name也不合理，实际上是获取welcome page的name
        [QueryReview("2012/05/17", "Long Liang", true, " re-order the index")]
        public string GetDocNameFromDB(Guid siteId, Guid webId)
        {
            var docName = string.Empty;
            ExceptionHandlingScope(() =>
            {
                Guid? docId = null;
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameter("@siteID", siteId);
                mQueryWorker.AddParameter("@webID", webId);
                using (var docIdReader = mQueryWorker.ExecuteReader(GetWelcomePageDocId_Select_NavNodes))
                {
                    if (docIdReader.Read() && !docIdReader.IsDBNull(0))
                    {
                        docId = docIdReader.GetGuid(0);
                    }
                }
                if (docId.HasValue)
                {
                    mQueryWorker.ResetCommand(CommandType.Text);
                    mQueryWorker.AddParameter("@siteID", siteId);
                    mQueryWorker.AddParameter("@ID", docId);
                    using (var docLeafNameReader = mQueryWorker.ExecuteReader(GetLeafNameByDocId_Select_AllDocs))
                    {
                        if (docLeafNameReader.Read() && !docLeafNameReader.IsDBNull(0))
                        {
                            docName = docLeafNameReader.GetString(0);
                        }
                    }
                }
            });
            return docName;
        }



        /// <summary>
        /// 将Orphan Site删除到回收站，无API实现.
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="itemId"></param>
        [QueryReview("2012/05/17", "Long Liang")]
        //todo:wbhu,1.方法名，应该是Recycle Site,而不是Delete 2.参数IAveContentDatabase dataBase 没什么用
        public void RecycleOrphanSiteInDB(IAveContentDatabase dataBase, string itemId)
        {
            var siteId = new Guid(itemId);
            bool siteIsAlreadyInDeletion;
            mQueryWorker.ResetCommand(CommandType.Text);
            mQueryWorker.AddParameter("@SiteId", siteId);
            using (var checkSiteDeletedReader = mQueryWorker.ExecuteReader(GetSiteDeletionIdBySiteId_Select_SiteDeletion))
            {
                siteIsAlreadyInDeletion = checkSiteDeletedReader.Read();
            }
            if (!siteIsAlreadyInDeletion)
            {
                using (var command = mQueryWorker.CreateCommand())
                {
                    command.Parameters.AddWithValue("@SiteId", siteId);
                    using (var trans = mQueryWorker.BeginTransaction())
                    {
                        try
                        {
                            command.Transaction = trans;
                            command.CommandText = SP2016InsertQueryString.RecycleSite_Insert_SiteDeletion;
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();


                            command.CommandText = SP2016UpdateQueryString.UpdateSiteDeletedStatus_Update_AllSites;
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();

                            trans.Commit();
                        }
                        catch (Exception e)
                        {
                            trans.Rollback();
                            logger.Warn("An error occurred while delete orphan site {0},roll back.Error:{1}", siteId, e);
                        }
                    }
                }
            }
        }

        [Obsolete("not used any more, will be removed later, use GetAllPageInWeb instead.")]
        public List<string> GetAllPageOfWeb(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 效率考虑，暂无API实现，可以用于获取当前web下所有page,不包括subsite中page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetAllPageInWeb(Guid siteId, Guid webId, bool isCurrentVersion = true)
        {
            CheckArguments(siteId,nameof(siteId));
            CheckArguments(webId, nameof(webId));

            var parameters = new Dictionary<string, object>
            {
                {"@WebId", webId},
                { "@SiteId", siteId}
            };
            return GetAllPageInternal(parameters, GetAllPagesUnderWeb_Select_AllDocs_WithoutVersion, isCurrentVersion);
        }

        private Dictionary<Guid, string> GetAllPageInternal(IDictionary<string, object> parameters, string queryString, bool isCurrentVersion)
        {
            var result = new Dictionary<Guid, string>();
            try
            {
                mQueryWorker.ResetCommand(CommandType.Text);
                mQueryWorker.AddParameters(parameters);
                using (var reader = mQueryWorker.ExecuteReader(queryString))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        if (!result.ContainsKey(id))
                        {
                            if (isCurrentVersion)
                            {
                                bool pageIsCurrentVersion = reader.GetBoolean(5);
                                if (!pageIsCurrentVersion)
                                {
                                    continue;
                                }
                                else
                                {
                                    string dirName = reader.GetString(1);
                                    string leafName = reader.GetString(2);
                                    string pageUrl = string.Format("{0}/{1}", dirName, leafName);
                                    result.Add(id, pageUrl);
                                }
                            }
                            else// last pulbished version ,maybe is current version
                            {
                                string pageUIVersionString = reader.GetString(4);
                                if (pageUIVersionString.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
                                {
                                    string dirName = reader.GetString(1);
                                    string leafName = reader.GetString(2);
                                    if (!reader.GetBoolean(5))
                                    {
                                        leafName += "?PageVersion=" + reader.GetInt32(3);

                                    }
                                    string pageUrl = string.Format("{0}/{1}", dirName, leafName);
                                    result.Add(id, pageUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                logger.Error("Get All Page Internal Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error("Get All Page Internal Error:{0}. Reason:{1}.", e.Message, e);
                throw new AveQueryException(e.Message, e);
            }
            return result;
        }

        /// <summary>
        /// 递归调用GetAllPagesByParentId，可能存在效率问题
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        //todo:wbhu,如果folder 嵌套的深度和广度比较大的话，效率问题会比较明显
        public List<string> GetAllPage(Guid siteId, Guid parentId)
        {
            var result = new List<string>();
            ExceptionHandlingScope(() =>
            {
                result = GetAllPagesByParentId(siteId, parentId);
            });
            return result;
        }

        /// <summary>
        /// 效率考虑，暂无API实现，可以用于获取folder下的所有page,
        /// Note：如果传入的是webUrl，那么获取的是web以及web的subsite中的所有page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentUrl">example sites/de/subEnglish/Shared Documents,  DirName格式 or API ServerRelativeUrl</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetAllPage(Guid siteId, string parentUrl)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@parentUrl", $"{parentUrl.Trim('/')}%"},
                { "@SiteId", siteId}
            };
            return GetAllPageInternal(parameters, GetAllPagesUnderFolder_Select_AllDocs);
        }

        /// <summary>
        ///获取site中所有webpart
        /// 无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webPartKey"></param>
        /// <param name="webpartNameTemp"></param>
        /// <returns></returns>
        public IAveQueryDataReader WebAddWebPartMessageHandler(Guid siteId, string webPartKey, string webpartNameTemp)
        {
            //todo:wbhu,13中实现就有问题，需要先确认下功能再重写写这个sql，应该是从07 copy过来的
            throw new NotSupportedException("WebAddWebPartMessageHandler is not supported in SP2016 at time moment.");
        }

        /// <summary>
        /// 获取某一scope下的重复文件信息，无API实现
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="webIds"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="fileNamePattern"></param>
        /// <param name="includeFileExtensions"></param>
        /// <param name="searchFile"></param>
        /// <param name="searchAttachment"></param>
        /// <returns></returns>
        public IAveQueryDataReader SearchDuplicateFiles(List<string> siteIds, List<string> webIds, List<string> excludeFileNames, string fileNamePattern, List<string> includeFileExtensions, bool searchFile, bool searchAttachment)
        {
            IAveQueryDataReader reader = null;
            ExceptionHandlingScope(() =>
            {
                var cmdText = GetDuplicateFileQuery_Select_Docs_Lists_Sites(siteIds, webIds, searchFile, searchAttachment, includeFileExtensions, excludeFileNames, fileNamePattern);
                using (var mCommand = mQueryWorker.CreateCommand())
                {
                    mCommand.CommandText = cmdText;
                    reader = new AveQueryDataReader(mCommand.ExecuteReader());
                }
            });
            return reader;
        }

        public void DeleteOrphanSiteInDB(IAveContentDatabase dataBase, string itemId)
        {
            RecycleOrphanSiteInDB(dataBase, itemId);
        }

        #endregion Central Admin
    }
}
