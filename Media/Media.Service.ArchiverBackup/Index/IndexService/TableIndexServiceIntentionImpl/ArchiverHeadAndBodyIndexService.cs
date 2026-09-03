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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives

    using AngleSharp.Common;
    using AngleSharp.Dom;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.Common.Util;
    using Cloud.Sdk.Data.EDiscovery;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Merged18NResources.MediaServiceArchiverBackup;
    using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    #endregion using directives

    public class ArchiverHeadAndBodyIndexService
        : ArchiverTableIndexServiceBase
        , IArchiverHeadAndBodyIndexService
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static int bodyVersionLookupIndexesInitialized;
        Dictionary<string, ArchiveIndexInfo> masterIndexDic = new Dictionary<string, ArchiveIndexInfo>();
        private Dictionary<string, string> OrderByColMapping = new Dictionary<string, string>
        {
            {"Name", "COL_NAME"},
            {"ArchvieTime", "COL_ARCHIVE_TIME"}
        };

        private void EnsureBodyVersionLookupIndexes()
        {
            if (Interlocked.CompareExchange(ref bodyVersionLookupIndexesInitialized, 1, 0) != 0)
            {
                return;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var tableName = SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody);
                var createItemArchiveTimeIndexSql = $"CREATE INDEX IF NOT EXISTS IDX_BODY_ITEMID_ARCHIVE_TIME ON {tableName}(COL_ITEMID, COL_ARCHIVE_TIME);";
                var createDedupeRankIndexSql = $"CREATE INDEX IF NOT EXISTS IDX_BODY_ITEMID_PATH_MODIFY_SEQUENCE ON {tableName}(COL_ITEMID, COL_PATH_MD5, COL_MODIFY_TIME DESC, COL_SEQUENCE DESC);";
                var indexExistsSql = "SELECT 1 FROM sqlite_master WHERE type = 'index' AND name = @INDEX_NAME LIMIT 1";
                var indexExistsParameters = new Dictionary<string, object>();

                indexExistsParameters["@INDEX_NAME"] = "IDX_BODY_ITEMID_ARCHIVE_TIME";
                var itemArchiveTimeIndexExists = this.IndexProcessor.ExecuteScalar(indexExistsSql, indexExistsParameters) != null;

                indexExistsParameters["@INDEX_NAME"] = "IDX_BODY_ITEMID_PATH_MODIFY_SEQUENCE";
                var dedupeRankIndexExists = this.IndexProcessor.ExecuteScalar(indexExistsSql, indexExistsParameters) != null;

                this.IndexProcessor.Execute(createItemArchiveTimeIndexSql, new Dictionary<string, object>());
                this.IndexProcessor.Execute(createDedupeRankIndexSql, new Dictionary<string, object>());

                stopwatch.Stop();
                var createdItemArchiveTimeIndex = !itemArchiveTimeIndexExists;
                var createdDedupeRankIndex = !dedupeRankIndexExists;
                var createdAnyIndex = createdItemArchiveTimeIndex || createdDedupeRankIndex;
                logger.Info($"EnsureBodyVersionLookupIndexes: createdAnyIndex={createdAnyIndex}, createdIndexes=[IDX_BODY_ITEMID_ARCHIVE_TIME:{createdItemArchiveTimeIndex}, IDX_BODY_ITEMID_PATH_MODIFY_SEQUENCE:{createdDedupeRankIndex}], elapsedMs={stopwatch.ElapsedMilliseconds}");
            }
            catch (Exception ex)
            {
                logger.Warn($"EnsureBodyVersionLookupIndexes: failed to ensure indexes, details: {ex}");
            }
        }

        public void InsertArchiveIndexes(List<ArchiverBasicIndex> indexes)
        {
            IndexProcessor.Insert(indexes);
        }
        public Int64 GetDatasCountFromBodyTable(String parentPath, Int64 endTime)
        {
            var parentPathMd5 = parentPath.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PARENT_PATH_MD5", parentPathMd5);
            parameters.Add("@END_TIME", endTime);
            string sql = "select distinct COL_PATH_MD5 from " + IndexConstants.TableNameArchiveBody
                    + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5"
                    + " and COL_ARCHIVE_TIME <= @END_TIME ";
            var itemsList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
#if DEBUG
            //var itemsList1 = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql.Replace("distinct COL_PATH_MD5", "*"), parameters);
#endif
            return itemsList.Count;
        }

        public Int64 GetDatasCountFromBodyTableByFilter(String parentPath, Int64 endTime, StringBuilder sql, BackupDataSearchContract searchContract)
        {
            var filter = searchContract.FilterPolicy;
            var parentPathMd5 = parentPath.ToMD5HashCode();

            sql.Append(" and COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 and COL_ARCHIVE_TIME <= @END_TIME ");
            var parameters = new Dictionary<String, Object>
            {
                { "@COL_PARENT_PATH_MD5", parentPathMd5 },
                { "@END_TIME", endTime }
            };

            var criteria = filter.FilterName;
            if (IsUseFullNameMatch(criteria))
            {
                criteria = criteria.TrimStart('\"').TrimEnd('\"');
            }
            else
            {
                if (filter.FilterName.Contains("*") || filter.FilterName.Contains("?"))
                {
                    criteria = filter.FilterName.Replace("*", "%").Replace("?", "_");
                }
                else
                {
                    criteria = "%" + filter.FilterName + "%";
                }
            }

            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (filter.SkipDocVersion && filter.Level == PolicyLevel.Document)
                {
                    sql.Append($" and (COL_NAME not like\'%:%\')");
                }
                if (!string.IsNullOrEmpty(filter.CreateStartTime) && !string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and (COL_CREATE_TIME > @CREATE_START_TIME and COL_CREATE_TIME < @CREATE_END_TIME)");
                    try
                    {
                        long createStartTime = 0;
                        long createEndTime = 0;
                        DateTime start = new DateTime();
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start) && DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            createStartTime = start.Ticks;
                            createEndTime = end.Ticks;
                        }
                        else
                        {
                            createStartTime = Convert.ToInt64(filter.CreateStartTime);
                            createEndTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_START_TIME", createStartTime);
                        parameters.Add("@CREATE_END_TIME", createEndTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateStartTime or CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME", Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME", Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreatedBy))
                {
                    sql.Append($" and (COL_AUTHOR like @CreatedBy)");
                    parameters.Add("@CreatedBy", "%" + filter.CreatedBy + "%");
                }
                if (!string.IsNullOrEmpty(filter.ModifiedBy))
                {
                    sql.Append($" and (COL_EXTENSION_9 like @ModifiedBy)");
                    parameters.Add("@ModifiedBy", "%" + filter.ModifiedBy + "%");
                }
                if (!string.IsNullOrEmpty(filter.FolderName))
                {
                    string tempFolderName = filter.FolderName.Replace("\\", "/");
                    sql.Append($" and ((SUBSTR(COL_EXTENSION_7,0,LENGTH(COL_EXTENSION_7) - INSTR(REVERSE(COL_EXTENSION_7), '/') + 1)) like @FolderPath)");
                    parameters.Add("@FolderPath", searchContract.SearchNode.SiteUrl + "%" + tempFolderName + "%");
                }
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    logger.Info($"this is endUser restore search,pathMd5 list count is {filter.PathMD5List.Count}");
                    sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.PathMD5List, out var pathMD5Parameters)})");
                    parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
                if (filter.FilterDeleteType == FilterDeletedType.Normal)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 0);
                }
                else if (filter.FilterDeleteType == FilterDeletedType.Soft)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 1);
                }
            }
            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            parameters.Add("@Url", searchContract.SearchNode.SiteUrl);
            parameters.Add("@TEXT", criteria);

            //sql.Append(@$" order by COL_ARCHIVE_TIME DESC,COL_NAME ");

            //if (filter.PageSize >= 0)
            //{
            //    int pageOffset = (filter.PageIndex - 1) * filter.PageSize;
            //    sql.Append(" LIMIT @PageSize OFFSET @PageOffset");
            //    parameters.Add("@PageSize", filter.PageSize);
            //    parameters.Add("@PageOffset", pageOffset);
            //}

            logger.Info($"search sql query is {sql.ToString()}");
            var itemsList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql.ToString(), parameters);
            return itemsList.Count;
        }

        public ArchiverBasicIndex GetParentDataFromHeadTable(ArchiverBasicIndex childIndex)
        {
            ArchiverBasicIndex index = new ArchiverBasicIndex();
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = childIndex.ParentPathMD5;
            parameterDictionary["@COL_ARCHIVE_TIME"] = childIndex.ArchiveTime;
            String sql = "select * from " + IndexConstants.TableNameArchiveHead
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }
        public void UpdateRetentionStatus(String path, Int64 endTime)
        {
            ArchiverBasicIndex index = null;
            logger.Info($"Begin update load item from {path}");
            string pathMD5;
            if (path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadArgumentNullException);
            }
            else
            {
                pathMD5 = path.ToMD5HashCode();
            }
            String tableName = GetTableNameByPath(pathMD5);
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMD5;
            parameterDictionary["@COL_ARCHIVE_TIME"] = endTime;
            parameterDictionary["@COL_FLAG"] = 0;
            String sql = "update " + tableName
                + " set COL_RETENTION_STATUS = 0 "
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " and COL_FLAG % 2 = @COL_FLAG ";
            IndexProcessor.ExecuteQuery(sql, parameterDictionary);
        }

        public void UpdateRetentionStatusByFilter(String path, Int64 endTime, BackupDataSearchContract searchContract)
        {
            logger.Info($"Begin update load items from parent {path}");
            var filter = searchContract.FilterPolicy;
            Dictionary<string, object> parameters = new()
            {
                ["@COL_PARENT_PATH_MD5"] = path,
                ["@COL_ARCHIVE_TIME"] = endTime,
                ["@COL_FLAG"] = 0
            };
            StringBuilder sql = new("update " + IndexConstants.TableNameArchiveBody
                + " set COL_RETENTION_STATUS = 0 "
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_TYPE = 'D' "  // only update retention status for document type
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " and COL_FLAG % 2 = @COL_FLAG ");

            var criteria = filter.FilterName;
            if (IsUseFullNameMatch(criteria))
            {
                criteria = criteria.TrimStart('\"').TrimEnd('\"');
            }
            else
            {
                if (filter.FilterName.Contains("*") || filter.FilterName.Contains("?"))
                {
                    criteria = filter.FilterName.Replace("*", "%").Replace("?", "_");
                }
                else
                {
                    criteria = "%" + filter.FilterName + "%";
                }
            }

            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (filter.SkipDocVersion && filter.Level == PolicyLevel.Document)
                {
                    sql.Append($" and (COL_NAME not like\'%:%\')");
                }
                if (!string.IsNullOrEmpty(filter.CreateStartTime) && !string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and (COL_CREATE_TIME > @CREATE_START_TIME and COL_CREATE_TIME < @CREATE_END_TIME)");
                    try
                    {
                        long createStartTime = 0;
                        long createEndTime = 0;
                        DateTime start = new DateTime();
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start) && DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            createStartTime = start.Ticks;
                            createEndTime = end.Ticks;
                        }
                        else
                        {
                            createStartTime = Convert.ToInt64(filter.CreateStartTime);
                            createEndTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_START_TIME", createStartTime);
                        parameters.Add("@CREATE_END_TIME", createEndTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateStartTime or CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME", Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME", Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreatedBy))
                {
                    sql.Append($" and (COL_AUTHOR like @CreatedBy)");
                    parameters.Add("@CreatedBy", "%" + filter.CreatedBy + "%");
                }
                if (!string.IsNullOrEmpty(filter.ModifiedBy))
                {
                    sql.Append($" and (COL_EXTENSION_9 like @ModifiedBy)");
                    parameters.Add("@ModifiedBy", "%" + filter.ModifiedBy + "%");
                }
                if (!string.IsNullOrEmpty(filter.FolderName))
                {
                    string tempFolderName = filter.FolderName.Replace("\\", "/");
                    sql.Append($" and ((SUBSTR(COL_EXTENSION_7,0,LENGTH(COL_EXTENSION_7) - INSTR(REVERSE(COL_EXTENSION_7), '/') + 1)) like @FolderPath)");
                    parameters.Add("@FolderPath", searchContract.SearchNode.SiteUrl + "%" + tempFolderName + "%");
                }
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    logger.Info($"this is endUser restore search,pathMd5 list count is {filter.PathMD5List.Count}");
                    sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.PathMD5List, out var pathMD5Parameters)})");
                    parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
                if (filter.FilterDeleteType == FilterDeletedType.Normal)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 0);
                }
                else if (filter.FilterDeleteType == FilterDeletedType.Soft)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 1);
                }
            }
            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            parameters.Add("@Url", searchContract.SearchNode.SiteUrl);
            parameters.Add("@TEXT", criteria);

            logger.Info($"update sql is {sql.ToString()}");

            IndexProcessor.Execute(sql.ToString(), parameters);
        }

        public ArchiverBasicIndex GetOneDataFromHeadOrBodyTable(String path, Int64 endTime)
        {
            ArchiverBasicIndex index = null;
            //logger.Info($"Begin loading load item from {path}");
            string pathMD5;
            if (path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadArgumentNullException);
            }
            else
            {
                pathMD5 = path.ToMD5HashCode();
            }
            String tableName = GetTableNameByPath(pathMD5);
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMD5;
            parameterDictionary["@COL_ARCHIVE_TIME"] = endTime;
            parameterDictionary["@COL_FLAG"] = 0;
            String sql = "select * from " + tableName
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " and COL_FLAG % 2 = @COL_FLAG "
                + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }
        public ArchiverBasicIndex GetOneDataFromHeadByPathMd5(String pathMd5, Int64 endTime)
        {
            ArchiverBasicIndex index = null;
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMd5;
            parameterDictionary["@COL_ARCHIVE_TIME"] = endTime;
            parameterDictionary["@COL_FLAG"] = 0;
            String sql = "select * from " + IndexConstants.TableNameArchiveBody
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " and COL_FLAG % 2 = @COL_FLAG "
                + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }
        public ArchiverBasicIndex GetNextBodyIndexBySequence(String jobId, long sequence)
        {
            ArchiverBasicIndex index = new ArchiverBasicIndex();
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_SEQUENCE"] = sequence;
            parameterDictionary["@jobId"] = jobId;
            String bodySql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_SEQUENCE > @COL_SEQUENCE and COL_JOBID = @jobId"
                + " order by COL_SEQUENCE asc limit 1";
            List<ArchiverBasicIndex> bodyIndexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(bodySql, parameterDictionary);
            ArchiverBasicIndex bodyIndex = null;
            long nextBodySequence = -1;
            if (bodyIndexList.Count > 0)
            {
                bodyIndex = bodyIndexList[0];
                nextBodySequence = bodyIndex.Sequence;
            }

            if (nextBodySequence == -1)
            {
                return null;
            }
            else
            {
                return bodyIndex;
            }
        }
        public List<ArchiverBasicIndex> GetDatasFromHeadTable(ArchiverIndexInfo indexInfo)
        {
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            string realPath = indexInfo.Path;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadFoldersArgumentNullException);
            }
            else
            {
                indexInfo.Path = indexInfo.Path.ToMD5HashCode();
            }
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            stopwatch.Stop();
            logger.Info($"Get datas from head table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}, endtime: {indexInfo.EndTime}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetDatasFromHeadTableByFilter(ArchiverIndexInfo indexInfo, StringBuilder sql, BackupDataSearchContract? searchContract)
        {
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            string realPath = indexInfo.Path;
            var criteria = "%%";
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadFoldersArgumentNullException);
            }
            else
            {
                indexInfo.Path = indexInfo.Path.ToMD5HashCode();
            }

            sql.Append(" and COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG ");
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {
                ["@COL_PARENT_PATH_MD5"] = indexInfo.Path,
                ["@COL_FLAG"] = 0,
                ["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime
            };

            if (searchContract != null)
            {
                var filter = searchContract.FilterPolicy;
                criteria = filter.FilterName;
                if (IsUseFullNameMatch(criteria))
                {
                    criteria = criteria.TrimStart('\"').TrimEnd('\"');
                }
                else
                {
                    if (filter.FilterName.Contains("*") || filter.FilterName.Contains("?"))
                    {
                        criteria = filter.FilterName.Replace("*", "%").Replace("?", "_");
                    }
                    else
                    {
                        criteria = "%" + filter.FilterName + "%";
                    }
                }
            }

            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            //parameters.Add("@Url", searchContract.SearchNode.SiteUrl);
            parameters.Add("@TEXT", criteria);

            sql.Append(" group by COL_PATH_MD5 ");
            sql.Append(" order by rowid asc ");

            if (indexInfo.Length < int.MaxValue - 1)
            {
                sql.Append(" LIMIT @COL_LENGTH OFFSET @COL_OFFSET ");
                parameters["@COL_OFFSET"] = indexInfo.OffSet;
                parameters["@COL_LENGTH"] = indexInfo.Length;
            }
            
            logger.Info($"search sql query is {sql.ToString()}");

            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql.ToString(), parameters);
            stopwatch.Stop();
            logger.Info($"Get datas from head table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}, endtime: {indexInfo.EndTime}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetDatasFromBodyTable(ArchiverIndexInfo indexInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            string realPath = indexInfo.Path;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsArgumentNullException);
            }
            else
            {
                indexInfo.Path = indexInfo.Path.ToMD5HashCode();
            }
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            stopwatch.Stop();
            logger.Info($"Get datas from body table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}, endtime: {indexInfo.EndTime}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetCurrentItemsFromBodyTable(ArchiverIndexInfo indexInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            string realPath = indexInfo.Path;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsArgumentNullException);
            }
            else
            {
                indexInfo.Path = indexInfo.Path.ToMD5HashCode();
            }

            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " and (COL_NAME not like '%:%' or COL_TYPE = 'A')"  //Attachment format: ***:***.txt
                + " group by COL_PATH_MD5 Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            stopwatch.Stop();
            logger.Info($"Get current datas from body table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}, endtime: {indexInfo.EndTime}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetDatasFromBodyTableByFilter(ArchiverIndexInfo indexInfo, StringBuilder sql, BackupDataSearchContract searchContract)
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //stopwatch.Start();
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            using var pfmScope = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetDatasFromBodyTableByFilter", $"Get datas under path: {indexInfo.Path}", true);
            string realPath = indexInfo.Path;
            var filter = searchContract.FilterPolicy;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsArgumentNullException);
            }
            else
            {
                indexInfo.Path = indexInfo.Path.ToMD5HashCode();
            }
            sql.Append(" and COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG ");
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {
                ["@COL_PARENT_PATH_MD5"] = indexInfo.Path,
                ["@COL_FLAG"] = 0,
                ["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime
            };
            //parameters["@COL_OFFSET"] = indexInfo.OffSet;
            //parameters["@COL_LENGTH"] = indexInfo.Length;

            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (filter.SkipDocVersion && filter.Level == PolicyLevel.Document)
                {
                    sql.Append($" and (COL_NAME not like\'%:%\')");
                }
                if (!string.IsNullOrWhiteSpace(filter.MainJobId))
                {
                    sql.Append($" and (SUBSTR(COL_JOBID, 1, INSTR(COL_JOBID, '_') - 1) like @MainJobId) ");
                    parameters.Add("@MainJobId", BuildBlurQueryValue(filter.MainJobId));
                }
                if (!string.IsNullOrEmpty(filter.CreateStartTime) && !string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and (COL_CREATE_TIME > @CREATE_START_TIME and COL_CREATE_TIME < @CREATE_END_TIME)");
                    try
                    {
                        long createStartTime = 0;
                        long createEndTime = 0;
                        DateTime start = new DateTime();
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start) && DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            createStartTime = start.Ticks;
                            createEndTime = end.Ticks;
                        }
                        else
                        {
                            createStartTime = Convert.ToInt64(filter.CreateStartTime);
                            createEndTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_START_TIME", createStartTime);
                        parameters.Add("@CREATE_END_TIME", createEndTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateStartTime or CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME", Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME", Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreatedBy))
                {
                    sql.Append($" and (COL_AUTHOR like @CreatedBy)");
                    parameters.Add("@CreatedBy", BuildBlurQueryValue(filter.CreatedBy));
                }
                if (!string.IsNullOrEmpty(filter.ModifiedBy))
                {
                    sql.Append($" and (COL_EXTENSION_9 like @ModifiedBy)");
                    parameters.Add("@ModifiedBy", BuildBlurQueryValue(filter.ModifiedBy));
                }
                if (!string.IsNullOrEmpty(filter.FolderName))
                {
                    string tempFolderName = filter.FolderName.Replace("\\", "/");
                    sql.Append($" and ((SUBSTR(COL_EXTENSION_7,0,LENGTH(COL_EXTENSION_7) - INSTR(REVERSE(COL_EXTENSION_7), '/') + 1)) like @FolderPath)");
                    parameters.Add("@FolderPath", searchContract.SearchNode.SiteUrl + "%" + tempFolderName + "%");
                }
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    logger.Info($"this is endUser restore search,pathMd5 list count is {filter.PathMD5List.Count}");
                    sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.PathMD5List, out var pathMD5Parameters)})");
                    parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
                if (filter.FilterDeleteType == FilterDeletedType.Normal)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 0);
                }
                else if (filter.FilterDeleteType == FilterDeletedType.Soft)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 1);
                }
            }
            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            parameters.Add("@Url", searchContract.SearchNode.SiteUrl);
            parameters.Add("@TEXT", BuildBlurQueryValue(filter.FilterName));

            sql.Append(" group by COL_PATH_MD5");
            if (indexInfo.Length < int.MaxValue - 1)
            {
                sql.Append(" LIMIT @COL_LENGTH OFFSET @COL_OFFSET ");
                parameters["@COL_OFFSET"] = indexInfo.OffSet;
                parameters["@COL_LENGTH"] = indexInfo.Length;
            }

            logger.Info($"search sql query is {sql.ToString()}");

            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql.ToString(), parameters);
            SortItems(indexList);
            //stopwatch.Stop();
            //logger.Info($"Get datas from body table by filter cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}");
            pfmScope.AppendMessage($",query result count is {indexList.Count}, endtime: {indexInfo.EndTime}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetVersionsByItemIdFromBodyTable(int topCount, string ItemId, long endTime, bool isRestoreAllVersions)
        {
            logger.Info($"get Datas from body index,{ItemId},topCount:{topCount},end time:{endTime}");
            using var _ = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetVersionsByItemIdFromBodyTable", $"Get versions of item: {ItemId}", true);
            EnsureBodyVersionLookupIndexes();
            var sql = "with DedupedVersions as ("
                + " select *, ROW_NUMBER() OVER (PARTITION BY COL_PATH_MD5 ORDER BY COL_MODIFY_TIME DESC, COL_SEQUENCE DESC) AS PathRowNum"
                + " from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_ITEMID = @COL_ITEMID "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME "
                + " and COL_NAME like '%:%'"
                + ") select * from DedupedVersions where PathRowNum = 1";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_ITEMID"] = ItemId;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = endTime;
            if (!isRestoreAllVersions && topCount > 0)
            {
                sql += " order by CAST(SUBSTR(COL_NAME, INSTR(COL_NAME, ':') + 1) AS REAL) DESC, COL_MODIFY_TIME DESC, COL_SEQUENCE DESC limit @LIMITE";
                parameterDictionary["@LIMITE"] = topCount;
            }
            else
            {
                sql += " order by CAST(SUBSTR(COL_NAME, INSTR(COL_NAME, ':') + 1) AS REAL) DESC, COL_MODIFY_TIME DESC, COL_SEQUENCE DESC";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);

            //stopwatch.Stop();
            SortItems(indexList);
            return indexList;

        }
        public List<ArchiverBasicIndex> GetDatasFromHeadTable2(ArchiverIndexInfo indexInfo)
        {
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetDatasFromBodyTable2(ArchiverIndexInfo indexInfo)
        {
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, indexInfo.Path);
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            return indexList;
        }
        public List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByType(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
        {
            var parameters = new Dictionary<String, Object>();
            filter.FilterName = filter.FilterName?.Trim() ?? "";
            var colume = string.Empty;
            //else if (filter.Condition == FilterCondition.Exactly && filter.RuleType == FilterRuleType.Attribute)
            //{
            //    criteria = "%" + ServiceConstants.Delimiter + filter.Criteria + ServiceConstants.ExtraChar + "%";
            //}
            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (filter.Level == PolicyLevel.Document)
                {
                    sql.Append($" and (COL_NAME not like\'%:%\')");
                }
                if (!string.IsNullOrEmpty(filter.CreateStartTime))
                {
                    sql.Append($" and COL_CREATE_TIME > @CREATE_START_TIME");
                    try
                    {
                        long startTime = 0;
                        DateTime start = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start))
                        {
                            startTime = start.Ticks;
                        }
                        else
                        {
                            startTime = Convert.ToInt64(filter.CreateStartTime);
                        }
                        parameters.Add("@CREATE_START_TIME", startTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and COL_CREATE_TIME < @CREATE_END_TIME");
                    try
                    {
                        long endTime = 0;
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            endTime = end.Ticks;
                        }
                        else
                        {
                            endTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_END_TIME", endTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateEndTime is illegality ,message:{e.ToString()}");
                    }

                }
                if (!string.IsNullOrEmpty(filter.ArchivedStartTime))
                {
                    sql.Append($" and COL_ARCHIVE_TIME > @ARCHIVE_START_TIME");
                    try
                    {
                        long startTime = 0;
                        DateTime start = new DateTime();
                        if (DateTime.TryParse(filter.ArchivedStartTime, out start))
                        {
                            startTime = start.Ticks;
                        }
                        else
                        {
                            startTime = Convert.ToInt64(filter.ArchivedStartTime);
                        }
                        parameters.Add("@ARCHIVE_START_TIME", startTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ArchiveStartTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ArchivedEndTime))
                {
                    sql.Append($" and COL_ARCHIVE_TIME < @ARCHIVE_END_TIME");
                    try
                    {
                        long endTime = 0;
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.ArchivedEndTime, out end))
                        {
                            endTime = end.Ticks;
                        }
                        else
                        {
                            endTime = Convert.ToInt64(filter.ArchivedEndTime);
                        }
                        parameters.Add("@ARCHIVE_END_TIME", endTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ARCHIVE_END_TIME is illegality ,message:{e.ToString()}");
                    }

                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME",Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME",Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreatedBy))
                {
                    sql.Append($" and (COL_AUTHOR like @CreatedBy)");
                    parameters.Add("@CreatedBy", BuildBlurQueryValue(filter.CreatedBy));
                }
                if (!string.IsNullOrEmpty(filter.ModifiedBy))
                {
                    sql.Append($" and (COL_EXTENSION_9 like @ModifiedBy)");
                    parameters.Add("@ModifiedBy", BuildBlurQueryValue(filter.ModifiedBy));
                }
                if (!string.IsNullOrWhiteSpace(filter.MainJobId))
                {
                    sql.Append($" and (SUBSTR(COL_JOBID, 1, INSTR(COL_JOBID, '_') - 1) like @MainJobId) ");
                    parameters.Add("@MainJobId", BuildBlurQueryValue(filter.MainJobId));
                }
                if (!string.IsNullOrEmpty(filter.FolderName))
                {
                    string tempFolderName=filter.FolderName.Replace("\\","/");
                    sql.Append($" and ((SUBSTR(COL_EXTENSION_7,0,LENGTH(COL_EXTENSION_7) - INSTR(REVERSE(COL_EXTENSION_7), '/') + 1)) like @FolderPath)");
                    parameters.Add("@FolderPath", restoreParam.SiteUrl + "%" + tempFolderName + "%");
                }
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    logger.Info($"this is endUser restore search,pathMd5 list count is {filter.PathMD5List.Count}");
                    sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.PathMD5List, out var pathMD5Parameters)})");
                    parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
                if (filter.FilterDeleteType == FilterDeletedType.Normal)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus",0);
                }
                else if(filter.FilterDeleteType == FilterDeletedType.Soft)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 1);
                }
            }
            if (!string.IsNullOrEmpty(filter.ParentPathMd5))//页面无法赋值，需要用控制台手动赋值
            {
                sql.Append($" and (COL_PARENT_PATH_MD5=@ParentPathMD5)");
                parameters.Add("@ParentPathMD5", filter.ParentPathMd5);
            }
            if (filter.ItemId != null && filter.ItemId.Count > 0)
            {
                var itemIds = filter.ItemId
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();
                if (itemIds.Count > 0)
                {
                    sql.Append($" and (COL_ID in {DatabaseUtility.BuildInClause(itemIds, out var itemIdParameters)})");
                    parameters.AddRangeInternal(itemIdParameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
            }
            if (filter.FullPathMD5List != null && filter.FullPathMD5List.Count > 0)
            {
                logger.Info($"this is rerun failed restore,FullPathList list count is {filter.FullPathMD5List.Count}");
                sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.FullPathMD5List, out var fullPathParameters)})");
                parameters.AddRangeInternal(fullPathParameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
            }
            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            parameters.Add("@Url", restoreParam.SiteUrl);
            parameters.Add("@TEXT", BuildBlurQueryValue(filter.FilterName));
            if (filter.Level != PolicyLevel.Document && filter.Level != PolicyLevel.DocumentVersion && filter.Level != PolicyLevel.Item)
            {
                sql.Append(" and COL_ARCHIVE_TIME <= @ENDTIME group by COL_PATH_MD5 ");// limit @COUNT");
                parameters.Add("@ENDTIME", restoreParam.EndTime);
            }
            if (!string.IsNullOrWhiteSpace(orderBy?.ColName) && OrderByColMapping.ContainsKey(orderBy.ColName))
            {
                sql.Append(@$" Order by {SecurityUtils.SanitizeSQLSchemaName(OrderByColMapping[orderBy.ColName])} {orderBy.Order.ToString()} ");
                orderBy = orderBy.Next;
                while(!string.IsNullOrWhiteSpace(orderBy?.ColName) && OrderByColMapping.ContainsKey(orderBy.ColName))
                {
                    sql.Append($@" ,{SecurityUtils.SanitizeSQLSchemaName(OrderByColMapping[orderBy.ColName])} {orderBy.Order.ToString()} ");
                    orderBy = orderBy.Next;
                }
            }
            else
            {
                sql.Append(@$" order by COL_ARCHIVE_TIME DESC,COL_NAME ");
            }

            if (filter.PageSize>=0)
            {
                int pageOffset = (filter.PageIndex - 1) * filter.PageSize;
                sql.Append(" LIMIT @PageSize OFFSET @PageOffset");
                parameters.Add("@PageSize", filter.PageSize + filter.ExtraQuerySize);
                parameters.Add("@PageOffset", pageOffset);
            }

            //var count = MediaEnvironment.MediaServer.MediaServerMaxSearchCount > 0 ? MediaEnvironment.MediaServer.MediaServerMaxSearchCount : 500;
            //parameters.Add("@COUNT", count);
            logger.Info($"search sql query is {sql.ToString()}");
            return this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql.ToString(), parameters);
        }

        public long GetTotalCountDataFromHeadOrBodyTableByType(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
        {
            var parameters = new Dictionary<String, Object>();
            filter.FilterName = filter.FilterName.Trim();
            BuildWhereConditionBaseOnFilter(sql, filter, restoreParam, parameters);
            logger.Info($"search sql query is {sql.ToString()}");
            return this.IndexProcessor.ExecuteQueryForOneColumeInt64(sql.ToString(), parameters).FirstOrDefault();
        }

        public void BuildWhereConditionBaseOnFilter(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, Dictionary<String, Object> parameters)
        {
            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (filter.Level == PolicyLevel.Document)
                {
                    sql.Append($" and (COL_NAME not like\'%:%\')");
                }
                if (!string.IsNullOrEmpty(filter.CreateStartTime))
                {
                    sql.Append($" and COL_CREATE_TIME > @CREATE_START_TIME");
                    try
                    {
                        long startTime = 0;
                        DateTime start = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start))
                        {
                            startTime = start.Ticks;
                        }
                        else
                        {
                            startTime = Convert.ToInt64(filter.CreateStartTime);
                        }
                        parameters.Add("@CREATE_START_TIME", startTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and COL_CREATE_TIME < @CREATE_END_TIME");
                    try
                    {
                        long endTime = 0;
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            endTime = end.Ticks;
                        }
                        else
                        {
                            endTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_END_TIME", endTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateEndTime is illegality ,message:{e.ToString()}");
                    }

                }
                if (!string.IsNullOrEmpty(filter.ArchivedStartTime))
                {
                    sql.Append($" and COL_ARCHIVE_TIME > @ARCHIVE_START_TIME");
                    try
                    {
                        long startTime = 0;
                        DateTime start = new DateTime();
                        if (DateTime.TryParse(filter.ArchivedStartTime, out start))
                        {
                            startTime = start.Ticks;
                        }
                        else
                        {
                            startTime = Convert.ToInt64(filter.ArchivedStartTime);
                        }
                        parameters.Add("@ARCHIVE_START_TIME", startTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ArchiveStartTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ArchivedEndTime))
                {
                    sql.Append($" and COL_ARCHIVE_TIME < @ARCHIVE_END_TIME");
                    try
                    {
                        long endTime = 0;
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.ArchivedEndTime, out end))
                        {
                            endTime = end.Ticks;
                        }
                        else
                        {
                            endTime = Convert.ToInt64(filter.ArchivedEndTime);
                        }
                        parameters.Add("@ARCHIVE_END_TIME", endTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ARCHIVE_END_TIME is illegality ,message:{e.ToString()}");
                    }

                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME", Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME", Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.CreatedBy))
                {
                    sql.Append($" and (COL_AUTHOR like @CreatedBy)");
                    parameters.Add("@CreatedBy", BuildBlurQueryValue(filter.CreatedBy));
                }
                if (!string.IsNullOrEmpty(filter.ModifiedBy))
                {
                    sql.Append($" and (COL_EXTENSION_9 like @ModifiedBy)");
                    parameters.Add("@ModifiedBy", BuildBlurQueryValue(filter.ModifiedBy));
                }
                if (!string.IsNullOrWhiteSpace(filter.MainJobId))
                {
                    sql.Append($" and (SUBSTR(COL_JOBID, 1, INSTR(COL_JOBID, '_') - 1) like @MainJobId) ");
                    parameters.Add("@MainJobId", BuildBlurQueryValue(filter.MainJobId));
                }
                if (!string.IsNullOrEmpty(filter.FolderName))
                {
                    string tempFolderName = filter.FolderName.Replace("\\", "/");
                    sql.Append($" and ((SUBSTR(COL_EXTENSION_7,0,LENGTH(COL_EXTENSION_7) - INSTR(REVERSE(COL_EXTENSION_7), '/') + 1)) like @FolderPath)");
                    parameters.Add("@FolderPath", restoreParam.SiteUrl + "%" + tempFolderName + "%");
                }
                if (filter.PathMD5List != null && filter.PathMD5List.Count > 0)
                {
                    logger.Info($"this is endUser restore search,pathMd5 list count is {filter.PathMD5List.Count}");
                    sql.Append($" and (COL_PATH_MD5 in {DatabaseUtility.BuildInClause(filter.PathMD5List, out var pathMD5Parameters)})");
                    parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
                if (filter.FilterDeleteType == FilterDeletedType.Normal)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 0);
                }
                else if (filter.FilterDeleteType == FilterDeletedType.Soft)
                {
                    sql.Append($" and (COL_RETENTION_STATUS = @RetentionStatus)");
                    parameters.Add("@RetentionStatus", 1);
                }
            }
            if (!string.IsNullOrEmpty(filter.ParentPathMd5))//页面无法赋值，需要用控制台手动赋值
            {
                sql.Append($" and (COL_PARENT_PATH_MD5=@ParentPathMD5)");
                parameters.Add("@ParentPathMD5", filter.ParentPathMd5);
            }
            if (filter.ItemId != null && filter.ItemId.Count > 0)
            {
                var itemIds = filter.ItemId
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();
                if (itemIds.Count > 0)
                {
                    sql.Append($" and (COL_ID in {DatabaseUtility.BuildInClause(itemIds, out var itemIdParameters)})");
                    parameters.AddRangeInternal(itemIdParameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
                }
            }
            sql.Append($" and (COL_ISSYSTEMFILE=@IsSystermfile)");
            parameters.Add("@IsSystermfile", "False");
            parameters.Add("@Url", restoreParam.SiteUrl);
            parameters.Add("@TEXT", BuildBlurQueryValue(filter.FilterName));
            //if (filter.Level != PolicyLevel.Document && filter.Level != PolicyLevel.DocumentVersion && filter.Level != PolicyLevel.Item)
            //{
            //    sql.Append(" and COL_ARCHIVE_TIME <= @ENDTIME group by COL_PATH_MD5 ");// limit @COUNT");
            //    parameters.Add("@ENDTIME", restoreParam.EndTime);
            //}
        }

        public Dictionary<string, List<ArchiverBasicIndex>> GetVersionsByItemIdsFromBodyTable(int topCount, IEnumerable<string> itemIds, long endTime, bool isRestoreAllVersions)
        {
            var normalizedItemIds = itemIds?
                .Where(itemId => !string.IsNullOrEmpty(itemId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
            if (normalizedItemIds.Count == 0)
            {
                return new Dictionary<string, List<ArchiverBasicIndex>>(StringComparer.OrdinalIgnoreCase);
            }

            logger.Info($"get Datas from body index by batch,count:{normalizedItemIds.Count},topCount:{topCount},end time:{endTime}");
            using var _ = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetVersionsByItemIdsFromBodyTable", $"Get versions of item count: {normalizedItemIds.Count}", true);
            EnsureBodyVersionLookupIndexes();

            List<SqlParameter> itemIdParameters;
            var tableName = SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody);
            var sql = "with FilteredRows as ("
                + "   select COL_ID, COL_ITEMID, COL_PATH_MD5, COL_MODIFY_TIME, COL_SEQUENCE, COL_NAME, COL_TYPE"
                + "   from " + tableName
                + $"   where (COL_ITEMID in {DatabaseUtility.BuildInClause(normalizedItemIds, out itemIdParameters)})"
                + "   and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME "
                + "   and INSTR(COL_NAME, ':') > 0"
                + "), DedupedVersions as ("
                + "   select COL_ID, COL_ITEMID, COL_MODIFY_TIME, COL_SEQUENCE, COL_NAME, COL_TYPE,"
                + "          ROW_NUMBER() OVER (PARTITION BY COL_PATH_MD5 ORDER BY COL_MODIFY_TIME DESC, COL_SEQUENCE DESC) AS PathRowNum"
                + "   from FilteredRows"
                + "), RankedVersions as ("
                + "   select COL_ID, COL_ITEMID, COL_TYPE,"
                + "          ROW_NUMBER() OVER (PARTITION BY COL_ITEMID ORDER BY CAST(SUBSTR(COL_NAME, INSTR(COL_NAME, ':') + 1) AS REAL) DESC, COL_MODIFY_TIME DESC, COL_SEQUENCE DESC) AS ItemRowNum"
                + "   from DedupedVersions where PathRowNum = 1"
                + ") select B.*, R.ItemRowNum"
                + " from RankedVersions R"
                + " join " + tableName + " B on B.COL_ID = R.COL_ID";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = endTime;
            parameterDictionary.AddRangeInternal(itemIdParameters.ToDictionary(p => p.ParameterName, p => p.Value), false);
            if (!isRestoreAllVersions && topCount >= 0)
            {
                sql += " where (R.ItemRowNum <= @COL_TOP_COUNT or R.COL_TYPE in ('I', 'A')) order by R.COL_ITEMID, R.ItemRowNum";
                parameterDictionary["@COL_TOP_COUNT"] = topCount;
            }
            else
            {
                sql += " order by R.COL_ITEMID, R.ItemRowNum";
            }

            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            Dictionary<string, List<ArchiverBasicIndex>> versionLookup = new Dictionary<string, List<ArchiverBasicIndex>>(StringComparer.OrdinalIgnoreCase);
            foreach (var index in indexList)
            {
                if (string.IsNullOrEmpty(index.NodeGuid))
                {
                    continue;
                }

                if (!versionLookup.TryGetValue(index.NodeGuid, out var versions))
                {
                    versions = new List<ArchiverBasicIndex>();
                    versionLookup[index.NodeGuid] = versions;
                }

                versions.Add(index);
            }

            foreach (var versions in versionLookup.Values)
            {
                SortItems(versions);
            }

            return versionLookup;
        }

        private static decimal ParseVersionNumber(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            var lastColonIndex = name.LastIndexOf(':');
            if (lastColonIndex < 0 || lastColonIndex == name.Length - 1)
            {
                return 0;
            }

            return decimal.TryParse(name[(lastColonIndex + 1)..], out var versionNumber)
                ? versionNumber
                : 0;
        }

        private string BuildBlurQueryValue(string criteria)
        {
            if (IsUseFullNameMatch(criteria))
            {
                criteria = criteria.TrimStart('\"').TrimEnd('\"');
            }
            else
            {
                if (criteria.Contains("*") || criteria.Contains("?"))
                {
                    criteria = criteria.Replace("*", "%").Replace("?", "_");
                }
                else
                {
                    criteria = "%" + criteria + "%";
                }
            }
            return criteria;
        }
        private bool IsUseFullNameMatch(string filterNameValue)
        {
            if (!string.IsNullOrEmpty(filterNameValue))
            {
                return filterNameValue.StartsWith('\"') && filterNameValue.EndsWith('\"');
            }
            return false;
        }
        public List<ArchiverBasicIndex> GetAllFSDatasFromHeadOrBodyTableByType(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
        {
            var parameters = new Dictionary<String, Object>();
            filter.FilterName = filter.FilterName.Trim();
            var criteria = filter.FilterName;
            var colume = string.Empty;

            if (filter.FilterName.Contains("*") || filter.FilterName.Contains("?"))
            {
                criteria = filter.FilterName.Replace("*", "%").Replace("?", "_");
            }
            else
            {
                criteria = "%" + filter.FilterName + "%";
            }
            if (filter.Level == PolicyLevel.Document || filter.Level == PolicyLevel.DocumentVersion)
            {
                if (!string.IsNullOrEmpty(filter.CreateStartTime) && !string.IsNullOrEmpty(filter.CreateEndTime))
                {
                    sql.Append($" and (COL_CREATE_TIME > @CREATE_START_TIME and COL_CREATE_TIME < @CREATE_END_TIME)");
                    try
                    {
                        long startTime = 0;
                        long endTime = 0;
                        DateTime start = new DateTime();
                        DateTime end = new DateTime();
                        if (DateTime.TryParse(filter.CreateStartTime, out start) && DateTime.TryParse(filter.CreateEndTime, out end))
                        {
                            startTime = start.Ticks;
                            endTime = end.Ticks;
                        }
                        else
                        {
                            startTime = Convert.ToInt64(filter.CreateStartTime);
                            endTime = Convert.ToInt64(filter.CreateEndTime);
                        }
                        parameters.Add("@CREATE_START_TIME", startTime);
                        parameters.Add("@CREATE_END_TIME", endTime);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"CreateStartTime or CreateEndTime is illegality ,message:{e.ToString()}");
                    }
                }
                if (!string.IsNullOrEmpty(filter.ModifiedStartTime) && !string.IsNullOrEmpty(filter.ModifiedEndTime))
                {
                    sql.Append($" and (COL_MODIFY_TIME > @MODIFY_START_TIME and COL_MODIFY_TIME < @MODIFY_END_TIME)");
                    try
                    {
                        parameters.Add("@MODIFY_START_TIME", Convert.ToDateTime(filter.ModifiedStartTime).Ticks);
                        parameters.Add("@MODIFY_END_TIME", Convert.ToDateTime(filter.ModifiedEndTime).Ticks);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ModifyStartTime or ModifyEndTime is illegality ,message:{e.ToString()}");
                    }
                }
            }
            parameters.Add("@Url", restoreParam.SiteUrl);
            parameters.Add("@TEXT", criteria);


            sql.Append(" and COL_ARCHIVE_TIME <= @ENDTIME group by COL_PATH_MD5 ");// limit @COUNT");
            parameters.Add("@ENDTIME", restoreParam.EndTime);

            if (!string.IsNullOrWhiteSpace(orderBy?.ColName) && OrderByColMapping.ContainsKey(orderBy.ColName))
            {
                sql.Append(@$" Order by {SecurityUtils.SanitizeSQLSchemaName(OrderByColMapping[orderBy.ColName])} {orderBy.Order.ToString()} ");
                orderBy = orderBy.Next;
                while (!string.IsNullOrWhiteSpace(orderBy?.ColName) && OrderByColMapping.ContainsKey(orderBy.ColName))
                {
                    sql.Append($@" ,{SecurityUtils.SanitizeSQLSchemaName(OrderByColMapping[orderBy.ColName])} {orderBy.Order.ToString()} ");
                    orderBy = orderBy.Next;
                }
            }
            else
            {
                sql.Append(@$" order by COL_ARCHIVE_TIME DESC,COL_NAME ");
            }

            if (filter.PageSize >= 0)
            {
                int pageOffset = (filter.PageIndex - 1) * filter.PageSize;
                sql.Append(" LIMIT @PageSize OFFSET @PageOffset");
                parameters.Add("@PageSize", filter.PageSize + filter.ExtraQuerySize);
                parameters.Add("@PageOffset", pageOffset);
            }

            logger.Info($"search sql query is {sql.ToString()}");
            var result = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql.ToString(), parameters);
            foreach (var tempResult in result)
            {
                var masterIndex = GetMasterIndexInternal(tempResult.JobId);
                tempResult.Attributes = masterIndex.UNCPath;
                tempResult.SitePath = masterIndex.ConnectionId;
                tempResult.Url = masterIndex.ConnectionPath + "\\" + tempResult.ExtraInfo + "\\" + tempResult.Name;
            }
            return result;
        }
        private ArchiveIndexInfo GetMasterIndexInternal(string jobId)
        {
            if (masterIndexDic.ContainsKey(jobId))
            {
                return masterIndexDic[jobId];
            }
            else
            {
                var parameters = new Dictionary<String, Object>();
                parameters.Add("@COL_JOB_ID", jobId);
                var sql = "select * from " + IndexConstants.TableNameArchiveIndexInfo + " where COL_JOB_ID = @COL_JOB_ID";
                var indexList = this.IndexProcessor.ExecuteQuery<ArchiveIndexInfo>(sql, parameters);
                if (indexList.Count == 0)
                {
                    logger.Warn($"can not find any master info by job id:{jobId}");
                }
                var masterIndexTemp = indexList.Count == 0?new ArchiveIndexInfo():indexList[0];
                masterIndexDic.Add(jobId, masterIndexTemp);
                return masterIndexTemp;
            }
        }
        public List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByTypeForJob(string sql, ArchiverBrowseInfo restoreParam)
        {
            return this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
        }
        public ArchiverBasicIndex GetAppWeb(ArchiverIndexInfo indexInfo)
        {
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException("An error occurred while loading appweb: parent path is null.");
            }
            ArchiverBasicIndex appWebIndex = null;
            String sql = null;
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_PATH_MD5"] = indexInfo.Path.ToMD5HashCode();
            sql = "select MAX(COL_ARCHIVE_TIME),* from "
                + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_PATH_MD5 = @COL_PATH_MD5"
                + " group by COL_PATH_MD5 order by rowid asc";
            var appList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            var appIndex = appList.Find(app => !app.AppDataName.IsNullOrEmpty());
            if (appIndex != null)
            {
                if (appIndex.Name.Contains("."))
                    parameters.Add("@COL_PARENT_PATH_MD5", appIndex.SitePath.ToMD5HashCode());
                else
                    parameters.Add("@COL_PARENT_PATH_MD5", appIndex.ParentPathMD5);
                sql = "select MAX(COL_ARCHIVE_TIME), * from "
                    + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                    + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 and COL_TYPE = 'P'"
                    + " group by COL_PATH_MD5 order by rowid asc";
                var appWebList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
                if (appWebList != null && appWebList.Count > 0)
                {
                    appWebIndex = appWebList.Find(appWeb => appWeb.Name.Substring(appWeb.Name.LastIndexOf("/") + 1).EqualsIgnoreCase(appIndex.AppDataName));
                }
            }
            return appWebIndex;
        }
        public String GetItemName(Int64 contentFileNumber, String jobId)
        {
            var itemName = String.Empty;
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_CONTENT_DATA_FILE_NUMBER", contentFileNumber);
            parameters.Add("@COL_JOBID", jobId + "%");
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                   + " where COL_CONTENT_DATA_FILE_NUMBER = @COL_CONTENT_DATA_FILE_NUMBER AND COL_JOBID LIKE @COL_JOBID";
            var indexes = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexes.Count > 0)
            {
                itemName = indexes[0].Name;
            }
            return itemName;
        }
        private List<string> GetFoldersMD5(ArchiverBasicIndex parent, List<ArchiverBasicIndex> indexList)
        {
            List<string> folderMD5 = new List<string>();
            foreach (var child in indexList)
            {
                if (child.Type == "W" || child.Type == "E")
                {
                    continue;
                }
                if (parent.PathMD5 == child.ParentPathMD5)
                {
                    folderMD5.Add(child.PathMD5);
                    var list = GetFoldersMD5(child, indexList);
                    folderMD5.AddRange(list);
                }
                else
                {
                    continue;
                }
            }
            return folderMD5;
        }

        private Dictionary<string, List<string>> GetDatasFromHeadTableForRemoveStub(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var selectMd5 = string.Empty;
            if (storagePolicyId != null)
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            }
            else
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_JOBID = @jobId";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectMd5, parameters);
            List<ArchiverBasicIndex> webPathMD5 = new List<ArchiverBasicIndex>();
            Dictionary<string, List<string>> currentWebFolders = new Dictionary<string, List<string>>();
            foreach (var webPath in indexList)
            {
                if (webPath.Type == "W")
                {
                    webPathMD5.Add(webPath);
                }
            }
            foreach (var pm5 in webPathMD5)
            {
                List<string> folderMD5 = new List<string>();
                folderMD5 = GetFoldersMD5(pm5, indexList);
                currentWebFolders.Add(pm5.Url, folderMD5);
            }
            return currentWebFolders;
        }

        private Dictionary<string, List<string>> GetDatasFromHeadTableForRemoveStubByJobId(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = $"%{jobId}%";
            var selectMd5 = string.Empty;
            selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_JOBID like @jobId";
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectMd5, parameters);
            List<ArchiverBasicIndex> webPathMD5 = new List<ArchiverBasicIndex>();
            Dictionary<string, List<string>> currentWebFolders = new Dictionary<string, List<string>>();
            foreach (var webPath in indexList)
            {
                if (webPath.Type == "W")
                {
                    webPathMD5.Add(webPath);
                }
            }
            foreach (var pm5 in webPathMD5)
            {
                List<string> folderMD5 = GetFoldersMD5(pm5, indexList);
                foreach (string fMD5 in folderMD5)
                {
                    if (currentWebFolders.ContainsKey(pm5.Url))
                    {
                        if (!currentWebFolders[pm5.Url].Contains(fMD5))
                        {
                            currentWebFolders[pm5.Url].Add(fMD5);
                        }
                    }
                    else
                    {
                        currentWebFolders.Add(pm5.Url, new List<string>() { fMD5 });
                    }
                }
            }
            return currentWebFolders;
        }


        public Dictionary<string, List<(string, string)>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId, ref String stubType,long modifiedTime = 0, bool isFilterSoftDelete =false)
        {
            List<String> documentsType = new List<String>();
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var selectDocumentsType = "select COL_TYPE from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            documentsType = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectDocumentsType, parameters);
            foreach (var type in documentsType)
            {
                if (!type.ToString().Equals("D", StringComparison.CurrentCultureIgnoreCase))
                {
                    return null;
                }
            }
            var selectStubInfo = "select COL_STUBINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%' limit 1";
            var stubTypeXmlString = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectStubInfo, parameters);
            foreach (string xmlString in stubTypeXmlString)
            {
                if (!xmlString.IsNullOrEmpty())
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xmlString);
                    var headerExtraAttribute = doc.GetElementsByTagName("StubInfo");
                    foreach (XmlNode node in headerExtraAttribute)
                    {
                        stubType = node.Attributes["StubType"].Value;
                        break;
                    }
                }
                break;
            }
            string selectDocumentsUrl = string.Empty;
            if (modifiedTime > 0)
            {
                parameters["@dateTime"] = modifiedTime;
                if (isFilterSoftDelete)
                {
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_ITEMID],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%' and COL_META_TAIL_LENGTH<@dateTime and COL_META_TAIL_LENGTH>0 and COL_RETENTION_STATUS = 1";
                }
                else
                {
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_ITEMID],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%' and COL_MODIFY_TIME<@dateTime";
                }
            }
            else
            {
                selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_ITEMID],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%'";
            }
            //documentsUrl = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectDocumentsUrl, parameters);
            Dictionary<string, List<(string, string)>> matchWebfiles = new Dictionary<string, List<(string, string)>>();
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectDocumentsUrl, parameters);
            var folders = GetDatasFromHeadTableForRemoveStub(storagePolicyId, jobId);
            foreach (var tempF in folders)
            {
                List<string> fileUrls = new List<string>();
                matchWebfiles.Add(tempF.Key, new List<(string, string)>());
                foreach (var flist in indexList)
                {
                    if (tempF.Value.Contains(flist.ParentPathMD5))
                    {
                        matchWebfiles[tempF.Key].Add((flist.Url, flist.NodeGuid));
                    }
                    else
                    {
                        continue;
                    }
                }
                if (matchWebfiles[tempF.Key].Count <= 0)
                {
                    matchWebfiles.Remove(tempF.Key);
                }
            }
            return matchWebfiles;
        }

        public Dictionary<string, List<ArchiverBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType)
        {
            var parameters = new Dictionary<String, Object>();
            var selectStubInfo = "select COL_STUBINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_JOBID like @jobId and COL_NAME not like '%:%' limit 1";
            parameters["@jobId"] = $"%{jobId}%";
            var stubTypeXmlString = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectStubInfo, parameters);

            if (stubTypeXmlString == null)
            {
                logger.Error("stub type xml string is null");
                throw new ArgumentNullException(nameof(stubTypeXmlString));
            }

            if(stubTypeXmlString.Any())
            {
                var xmlString = stubTypeXmlString[0];
                if(!xmlString.IsNullOrEmpty())
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xmlString);
                    var headerExtraAttribute = doc.GetElementsByTagName("StubInfo");
                    if (headerExtraAttribute != null && headerExtraAttribute.Count > 0)
                    {
                        var node = headerExtraAttribute[0];
                        if (node != null)
                        {
                            stubType = node.Attributes["StubType"].Value;
                        }
                    }
                }
            }

            var selectDocumentsUrl = "select [COL_NAME],[COL_EXTENSION_7],[COL_PATH_MD5],[COL_PARENT_PATH_MD5],[COL_ARCHIVE_TIME] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_JOBID like @jobId and COL_NAME not like '%:%'";
            Dictionary<string, List<ArchiverBasicIndex>> matchWebfiles = new Dictionary<string, List<ArchiverBasicIndex>>();
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectDocumentsUrl, parameters);
            var folders = GetDatasFromHeadTableForRemoveStubByJobId(jobId);
            foreach (var tempF in folders)
            {
                matchWebfiles.Add(tempF.Key, new List<ArchiverBasicIndex>());
                foreach (var flist in indexList)
                {
                    if (tempF.Value.Contains(flist.ParentPathMD5))
                    {
                        matchWebfiles[tempF.Key].Add(flist);
                    }
                    else
                    {
                        continue;
                    }
                }
                if (matchWebfiles[tempF.Key].Count <= 0)
                {
                    matchWebfiles.Remove(tempF.Key);
                }
            }
            return matchWebfiles;
        }

        public Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<ArchiverBasicIndex> item, String jobId, ref String stubType)
        {
            var indexList = new List<ArchiverBasicIndex>();
            Dictionary<string, List<string>> matchWebfiles = new Dictionary<string, List<string>>();
            bool hasEnsureStubType = false;
            foreach (var tm in item)
            {
                if (!tm.Type.Equals("D", StringComparison.CurrentCultureIgnoreCase) && tm.JobId == jobId)
                {
                    return null;
                }
                if (!hasEnsureStubType)
                {
                    if (!tm.stubInfo.IsNullOrEmpty())
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(tm.stubInfo);
                        var headerExtraAttribute = doc.GetElementsByTagName("StubInfo");
                        foreach (XmlNode node in headerExtraAttribute)
                        {
                            stubType = node.Attributes["StubType"].Value;
                            break;
                        }
                    }
                    hasEnsureStubType = true;
                }
                if (tm.JobId == jobId && !tm.Name.Contains(":")) //version contain ':'
                {
                    indexList.Add(tm);
                }
            }
            var folders = GetDatasFromHeadTableForRemoveStub(null, jobId);
            foreach (var tempF in folders)
            {
                List<string> fileUrls = new List<string>();
                matchWebfiles.Add(tempF.Key, new List<string>());
                foreach (var flist in indexList)
                {
                    if (tempF.Value.Contains(flist.ParentPathMD5))
                    {
                        matchWebfiles[tempF.Key].Add(flist.Url);
                    }
                    else
                    {
                        continue;
                    }
                }
                if (matchWebfiles[tempF.Key].Count <= 0)
                {
                    matchWebfiles.Remove(tempF.Key);
                }
            }
            return matchWebfiles;
        }
        public string GetSiteUrlFromMainIndex(String storagePolicyId, String jobId)
        {
            string siteUrl = string.Empty;
            var parameters = new Dictionary<String, Object>();
            string selectSiteUrl = string.Empty;
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            if (storagePolicyId != null)
            {
                selectSiteUrl = "select [COL_EXTENSION_7] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_TYPE = 'E'";
            }
            else
            {
                selectSiteUrl = "select [COL_EXTENSION_7] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_JOBID = @jobId and COL_TYPE = 'E'";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectSiteUrl, parameters);
            foreach (var siteCollUrl in indexList)
            {
                return siteCollUrl.Url;
            }
            logger.Warn("siteUrl not exsit in TB_HEAD_INDEX");
            try
            {
                logger.Info("Start get all site records in the index db.");
                parameters.Clear();
                selectSiteUrl = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_TYPE = 'E'";
                indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectSiteUrl, parameters);

                foreach (var record in indexList)
                {
                    logger.Info($"Url: {record.Name} COL_STORAGEPOLICYID: {record.StoragePolicyId} COL_JOBID: {record.JobId} ");
                }
                logger.Info("End get all site records in the index db.");
            }
            catch (Exception ex)
            {
                logger.Warn(ex.ToString());
            }
            return string.Empty;
        }
        public void DeleteDataFromMainIndex(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
            this.IndexProcessor.Execute(deleteHeadTable, parameters);
        }
        public void DeleteDataFromMainIndexByTime(String storagePolicyId, String jobId, long dateTime, bool isFilterSoftDelete)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            parameters["@dateTime"] = dateTime;
            string deleteBodyTable = string.Empty;
            if (isFilterSoftDelete)
            {
                deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_META_TAIL_LENGTH<@dateTime and COL_META_TAIL_LENGTH>0 and COL_RETENTION_STATUS = 1";
            }
            else
            {
                deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_MODIFY_TIME<@dateTime";
            }
            this.IndexProcessor.Execute(deleteBodyTable, parameters);

            var sql = "SELECT COUNT(*) FROM " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            long exsitFileCount = Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, parameters));
            if (exsitFileCount <= 0)
            {
                var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
                this.IndexProcessor.Execute(deleteHeadTable, parameters);
            }
        }
        public void UpdateAsSoftDelete(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var deleteBodyTable = "update " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " set COL_RETENTION_STATUS = 1 where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
        public void UpdateAsSoftDeleteByTime(String storagePolicyId, String jobId, long dateTime)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            parameters["@dateTime"] = dateTime;
            parameters["@timeNow"] = DateTime.UtcNow.Ticks.ToString();
            var deleteBodyTable = "update " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " set COL_RETENTION_STATUS = 1,COL_META_TAIL_LENGTH = @timeNow where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_MODIFY_TIME<@dateTime and COL_RETENTION_STATUS = 0";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
        public List<ArchiverBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId)
        {
            int offset = 0;
            int indexLimit = 32775;
            int tempResultCount = 0;
            string sql = $"select * from {IndexConstants.TableNameArchiveBody} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId LIMIT @offset, @length";
            List<ArchiverBasicIndex> results = new List<ArchiverBasicIndex>();
            do
            {
                var parameters = new Dictionary<String, Object>();
                parameters["@storagePolicyId"] = storagePolicyId;
                parameters["@jobId"] = jobId;
                parameters["@offset"] = offset;
                parameters["@length"] = indexLimit;
                var indexes = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
                tempResultCount = indexes.Count;
                offset += tempResultCount;
                results.AddRange(indexes);

            } while (tempResultCount == indexLimit);

            return results;
        }

        private class TempArchivedFileIndexInfo
        {
            public long RowId { get; set; }
            public string Url { get; set; } = string.Empty;
            public long ContentLength { get; set; }
            public long ContentDataFileNumber { get; set; }
            public string ExtraInfo { get; set; } = string.Empty;
        }
        public List<ArchivedFileIndexInfo> GetArchivedFileIndexes(String storagePolicyId, String jobId)
        {
            long? anchorId = null;
            int indexLimit = 32775;
            int tempResultCount = 0;
            List<ArchivedFileIndexInfo> results = new List<ArchivedFileIndexInfo>();
            do
            {
                string sql =
$@"select RowId, COL_EXTENSION_7 AS Url, COL_EXTENSION_5 AS ContentLength, COL_CONTENT_DATA_FILE_NUMBER AS ContentDataFileNumber, COL_EXTRAINFO AS ExtraInfo
  from {IndexConstants.TableNameArchiveBody} 
  where {(anchorId is null ? string.Empty : "RowId > @anchorId and ")}COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId
  order by RowId asc LIMIT @length";
                var parameters = new Dictionary<String, Object>();
                parameters["@storagePolicyId"] = storagePolicyId;
                parameters["@jobId"] = jobId;
                parameters["@length"] = indexLimit;
                if (anchorId is not null)
                {
                    parameters["@anchorId"] = anchorId;
                }
                var indexes = this.IndexProcessor.ExecuteQueryForAllClass<TempArchivedFileIndexInfo>(sql, parameters);
                tempResultCount = indexes.Count;
                if (tempResultCount > 0)
                {
                    anchorId = indexes[^1].RowId;
                }
                results.AddRange(indexes.Select(i => new ArchivedFileIndexInfo()
                {
                    Url = GetFullPath(i.ExtraInfo, i.Url),
                    ContentDataFileNumber = i.ContentDataFileNumber,
                    ContentLength = i.ContentLength,
                }));

            } while (tempResultCount == indexLimit);

            return results;

            string GetFullPath(string extraInfo, string url)
            {
                var document = new XmlDocument();
                document.LoadXml(extraInfo);
                var apUrlElements = document.GetElementsByTagName("HeaderExtraAttribute");
                if (apUrlElements != null && apUrlElements.Count > 0)
                {
                    var apUrl = apUrlElements[0]?.Attributes["APUrl"]?.Value ?? url;
                    return apUrl.Contains("\\") ? apUrl?.Replace("\\", "/") : apUrl;
                }
                return url;
            }
        }


        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(String storagePolicyId, String jobId, String siteURL, long dateTime = 0)
        {
            List<KeyValuePair<string, long>> result = new();
            var listUrlFolderMap = GetDatasFromHeadTableForRetentionInfo(storagePolicyId, jobId);
            foreach (var listUrlFolder in listUrlFolderMap)
            {
                var listURL = listUrlFolder.Key;
                List<string> allParents = listUrlFolder.Value;
                var libraryFileTotalCount = 0L;
                var queryConditionsCount = 100;
                for (int j = 0; j < allParents.Count; j += queryConditionsCount)
                {
                    logger.Info($"Query by parent id, start at {j}");
                    var tempAllParents = allParents.Skip(j).Take(queryConditionsCount).ToList();
                    long fileCountByPage = GetOneLibraryFileCountByPage(tempAllParents);
                    libraryFileTotalCount += fileCountByPage;
                }
                KeyValuePair<string, long> item = new(listURL, libraryFileTotalCount);
                result.Add(item);
            }
            long GetOneLibraryFileCountByPage(List<string> parentMD5s)
            {
                var parameters = new Dictionary<String, Object>();
                parameters["@storagePolicyId"] = storagePolicyId;
                parameters["@jobId"] = jobId;
                parameters["@siteURL"] = siteURL;
                string getDeleteAllFiles = string.Empty;
                List<SqlParameter> pathMD5Parameters;
                if (dateTime == 0)
                {
                    getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameArchiveBody} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_SITE_PATH = @siteURL" +
    $" and (COL_PARENT_PATH_MD5 in {DatabaseUtility.BuildInClause(parentMD5s, out pathMD5Parameters)})" +
    $" and COL_TYPE = 'D' and COL_NAME NOT LIKE '%:%'";
                }
                else
                {
                    parameters["@dateTime"] = dateTime;
                    getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameArchiveBody} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_SITE_PATH = @siteURL and COL_MODIFY_TIME<@dateTime" +
    $" and (COL_PARENT_PATH_MD5 in {DatabaseUtility.BuildInClause(parentMD5s, out pathMD5Parameters)})" +
    $" and COL_TYPE = 'D' and COL_NAME NOT LIKE '%:%'";
                }
                parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);

                var fileCountResult = this.IndexProcessor.ExecuteScalar(getDeleteAllFiles, parameters);
                _ = long.TryParse(fileCountResult?.ToString(), out long fileCount);
                return fileCount;
            }
            return result;
        }

        private Dictionary<string, List<string>> GetDatasFromHeadTableForRetentionInfo(String? storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var selectMd5 = string.Empty;
            if (storagePolicyId != null)
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            }
            else
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_JOBID = @jobId";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectMd5, parameters);
            List<ArchiverBasicIndex> listPathMD5 = new List<ArchiverBasicIndex>();
            Dictionary<string, List<string>> currentWebFolders = new Dictionary<string, List<string>>();
            foreach (var path in indexList)
            {
                if (path.Type == "L")
                {
                    listPathMD5.Add(path);
                }
            }
            foreach (var pm5 in listPathMD5)
            {
                //List<string> folderMD5 = new List<string>();
                List<string> folderMD5 = GetFoldersMD5(pm5, indexList);
                folderMD5.Insert(0, pm5.PathMD5);
                currentWebFolders.Add(pm5.Url, folderMD5);
            }
            return currentWebFolders;
        }


        public List<String> GetStorageInfosByJobId(String jobId)
        {
            List<String> storageInfos = new List<String>();
            var sql = "select COL_STORAGEINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_JOBID = @jobId"
                + " union"
                + " select COL_STORAGEINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_JOBID = @jobId";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@jobId", jobId);
            var indexes = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            foreach (ArchiverBasicIndex index in indexes)
            {
                storageInfos.Add(index.StorageInfo);
            }
            return storageInfos;
        }

        public Int64 GetJobDataMode(String jobId)
        {
            var dataMode = default(Int64);
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@jobId", jobId);
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                   + " where COL_JOBID = @jobId";
            var indexes = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexes.Count > 0)
            {
                dataMode = indexes[0].Flag;
            }
            return dataMode;
        }

        /// <summary>
        /// Batch retrieves DataMode for multiple job IDs (eliminates N+1 query problem)
        /// </summary>
        /// <param name="jobIds">Collection of job IDs to query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping JobId to DataMode value</returns>
        public async Task<Dictionary<string, Int64>> GetJobDataModesBatchAsync(IEnumerable<string> jobIds, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, Int64>();
            var jobIdList = jobIds?.ToList();
            
            if (jobIdList == null || jobIdList.Count == 0)
            {
                logger.Debug("GetJobDataModesBatchAsync: No job IDs provided");
                return result;
            }

            var stopwatch = Stopwatch.StartNew();
            logger.Debug($"GetJobDataModesBatchAsync: Starting batch query for {jobIdList.Count} job IDs");

            try
            {
                // Escape job IDs for SQL IN clause (protect against SQL injection)
                var escapedJobIds = jobIdList.Select(id => id.Replace("'", "''"));
                var jobIdInClause = string.Join(",", escapedJobIds.Select(id => $"'{id}'"));
                
                string sql = "SELECT COL_JOBID, COL_FLAG FROM " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                       + " WHERE COL_JOBID IN (" + jobIdInClause + ")";

                // Execute query synchronously (no async version available in IndexProcessor)
                var indexes = await Task.Run(() => this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null), cancellationToken);
                
                // Build result dictionary
                foreach (var index in indexes)
                {
                    if (!string.IsNullOrEmpty(index.JobId) && !result.ContainsKey(index.JobId))
                    {
                        result[index.JobId] = index.Flag;
                    }
                }
                
                stopwatch.Stop();
                logger.Info($"GetJobDataModesBatchAsync: Successfully retrieved {result.Count} data modes in {stopwatch.ElapsedMilliseconds}ms");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.Error($"GetJobDataModesBatchAsync: Error retrieving data modes after {stopwatch.ElapsedMilliseconds}ms: {ex}");
                throw;
            }
        }

        public ArchiverBasicIndex GetNextIndexBySequence(String jobId, long sequence)
        {
            ArchiverBasicIndex index = new ArchiverBasicIndex();
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_SEQUENCE"] = sequence;
            parameterDictionary["@jobId"] = jobId;
            String bodySql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_SEQUENCE > @COL_SEQUENCE and COL_JOBID = @jobId"
                + " order by COL_SEQUENCE asc limit 1";
            List<ArchiverBasicIndex> bodyIndexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(bodySql, parameterDictionary);
            ArchiverBasicIndex bodyIndex = null;
            long nextBodySequence = -1;
            if (bodyIndexList.Count > 0)
            {
                bodyIndex = bodyIndexList[0];
                nextBodySequence = bodyIndex.Sequence;
            }

            parameterDictionary["@COL_SEQUENCE"] = sequence;
            String headSql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_SEQUENCE > @COL_SEQUENCE and COL_JOBID = @jobId"
                + " order by COL_SEQUENCE asc limit 1";
            List<ArchiverBasicIndex> headerIndexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(headSql, parameterDictionary);
            ArchiverBasicIndex headIndex = null;
            long nextHeadSequence = -1;
            if (headerIndexList.Count > 0)
            {
                headIndex = headerIndexList[0];
                nextHeadSequence = headIndex.Sequence;
            }

            if (nextHeadSequence > 0 && nextBodySequence - nextHeadSequence > 0)
            {
                return headIndex;
            }
            //nextBodySequence=-1 and nextHeadSequence=-1
            else if (nextBodySequence - nextHeadSequence == 0)
            {
                return null;
            }
            else
            {
                return bodyIndex;
            }
        }

        #region Archiver/Records Lifecycle
        public List<string> GetUniqueRetentions()
        {
            var parameters = new Dictionary<String, Object>();
            string sql = "select distinct COL_RETENTION from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                  + " where COL_RETENTION is not null and COL_RETENTION != ''";
            var itemsList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return itemsList;
        }


        public List<ArchiverBasicIndex> GetRetentionData(string retentionId, long orphanTicks)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_RETENTION"] = retentionId;
            parameters["@COL_ARCHIVE_TIME"] = orphanTicks;
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                  + " where COL_RETENTION = @COL_RETENTION and COL_ARCHIVE_TIME < @COL_ARCHIVE_TIME";
            var itemsList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            return itemsList;
        }

        public void DeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = jobId;
            StringBuilder sb = new StringBuilder();
            bool isfirst = true;
            foreach (var temp in pathMD5)
            {
                if (!isfirst)
                {
                    sb.Append(", ");
                }
                sb.Append("'").Append(temp).Append("'");
                isfirst = false;
            }
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_PATH_MD5 in (" + sb.ToString() + ") and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }

        public List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, String siteURL)
        {
            List<KeyValuePair<string, long>> result = new();
            var listUrlFolderMap = GetDatasFromHeadTableForRetentionInfo(null, jobId);
            foreach (var listUrlFolder in listUrlFolderMap)
            {
                var listURL = listUrlFolder.Key;
                List<string> allParents = listUrlFolder.Value;
                var libraryFileTotalCount = 0L;
                var queryConditionsCount = 100;
                for (int j = 0; j < allParents.Count; j += queryConditionsCount)
                {
                    logger.Info($"Query by parent id, start at {j}");
                    var tempAllParents = allParents.Skip(j).Take(queryConditionsCount).ToList();
                    long fileCountByPage = GetOneLibraryFileCountByPage(tempAllParents);
                    libraryFileTotalCount += fileCountByPage;
                }
                KeyValuePair<string, long> item = new(listURL, libraryFileTotalCount);
                result.Add(item);
            }
            long GetOneLibraryFileCountByPage(List<string> parentMD5s)
            {
                var parameters = new Dictionary<String, Object>();
                parameters["@jobId"] = jobId;
                parameters["@siteURL"] = siteURL;

                StringBuilder sb = new StringBuilder();
                bool isfirst = true;
                foreach (var temp in pathMD5)
                {
                    if (!isfirst)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("'").Append(temp).Append("'");
                    isfirst = false;
                }

                var getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameArchiveBody} where COL_JOBID = @jobId and COL_SITE_PATH = @siteURL" +
                    $" and (COL_PARENT_PATH_MD5 in {DatabaseUtility.BuildInClause(parentMD5s, out var pathMD5Parameters)})" +
                    $" and COL_PATH_MD5 in ({sb} )" +
                    $" and COL_TYPE = 'D' and COL_NAME NOT LIKE '%:%'";

                parameters.AddRangeInternal(pathMD5Parameters.ToDictionary(p => p.ParameterName, p => p.Value), false);

                var fileCountResult = this.IndexProcessor.ExecuteScalar(getDeleteAllFiles, parameters);
                _ = long.TryParse(fileCountResult?.ToString(), out long fileCount);
                return fileCount;
            }
            return result;
        }
        public void DeletedDataFromMainIndexByNodeGuid(string jobId, List<string> nodeGuid)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = jobId;
            StringBuilder sb = new StringBuilder();
            bool isfirst = true;
            foreach (var temp in nodeGuid)
            {
                if (!isfirst)
                {
                    sb.Append(", ");
                }
                sb.Append("'").Append(temp).Append("'");
                isfirst = false;
            }
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_ITEMID in (" + sb.ToString() + ") and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }

        public long GetSubSiteArchiveSize(string subSiteUrl, ArchiverBrowseInfo info)
        {
            logger.Info($"GetSubSiteArchiveSize subSiteUrl: {subSiteUrl}, StartTime: {info?.StartTime}, EndTime: {info?.EndTime}");
            try
            {
                using var pfmScope = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetHeadIndexPage", $"GetSubSiteArchiveSize for subsite:{subSiteUrl}", true);
                String sql = @$"Select Sum(COL_EXTENSION_5) from tb_body_index where COL_EXTENSION_7 like @subSiteUrl ";
                if (info != null)
                {
                    sql += @$" and COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime}";
                }
                var parameters = new Dictionary<String, Object>();
                parameters.Add("@subSiteUrl", $"{subSiteUrl.TrimEnd('/')}/%");

                return IndexProcessor.ExecuteQueryForOneColumeInt64(sql, parameters).FirstOrDefault();
            }
            catch
            {// 如果body表里没有对应记录，sum 函数会返回null 进而异常
                return 0;
            }
            
        }


        public List<ArchiverBasicIndex> GetAllBodyIndex()
        {
            String sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetBodyIndexPage(int pageSize, int pageOffset)
        {
            logger.Info($"GetBodyIndexPage with pageSize: {pageSize}, pageOffSet: {pageOffset}");
            using var pfmScope = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetBodyIndexPage", $"GetBodyIndexPage for pageSize: {pageSize}, pageOffSet: {pageOffset}", true);
            var sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from "
                + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " order by COL_ARCHIVE_TIME desc LIMIT @PageSize OFFSET @PageOffset";
            var parameters = new Dictionary<string, object>
            {
                {"@PageSize", pageSize},
                {"@PageOffset", pageOffset}
            };

            var indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            pfmScope.AppendMessage($",query result count is {indexList.Count}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetAllHeadIndex()
        {
            String sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetHeadIndexPage(int pageSize, int pageOffset)
        {
            logger.Info($"GetHeadIndexPage called with pageSize: {pageSize}, pageOffSet: {pageOffset}");
            using var pfmScope = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetHeadIndexPage", $"GetHeadIndexPage for pageSize: {pageSize}, pageOffSet: {pageOffset}", true);
            var sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from "
                + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " order by COL_ARCHIVE_TIME desc LIMIT @PageSize OFFSET @PageOffset";
            var parameters = new Dictionary<string, object>
            {
                {"@PageSize", pageSize},
                {"@PageOffset", pageOffset}
            };
            var indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            pfmScope.AppendMessage($",query result count is {indexList.Count}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetAllSubSites(ArchiverBrowseInfo info)
        {
            logger.Info($"GetAllSubSites called with StartTime: {info?.StartTime}, EndTime: {info?.EndTime}");
            using var pfmScope = new PerformanceScope("ArchiverHeadAndBodyIndexService:GetHeadIndexPage", $"GetAllSubSites", true);
            String sql = @$"select distinct COL_EXTENSION_7
from tb_head_index 
where COL_TYPE = 'W' 
 ";
            var parameters = new Dictionary<string, object>();
            if (info != null)
            {
                sql += @$" and COL_ARCHIVE_TIME <= @EndTime and COL_ARCHIVE_TIME >= @StartTime";
                parameters.Add("@StartTime", info.StartTime);
                parameters.Add("@EndTime", info.EndTime);
            }
            var indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            pfmScope.AppendMessage($",query result count is {indexList.Count}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetAllBodyIndexOnSpecificTimeRange(ArchiverBrowseInfo info)
        {
            String sql = @$"
                select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH 
                from {IndexConstants.TableNameArchiveBody} 
                where COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime} 
                order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetAllHeadIndexOnSpecificTimeRange(ArchiverBrowseInfo info)
        {
            String sql = @$"
                select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH 
                from {IndexConstants.TableNameArchiveHead}
                where COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime}
                order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }
        #endregion

        #region Full Text Index

        public List<ArchiverBasicIndex> GetNeedFiles(String jobId, String siteUrl, Int32 offset, Int32 length, String isSystemFile)
        {
            var sql = default(String);
            var parameters = new Dictionary<String, Object>();
            var rootSiteName = ".";
            parameters.Add("@COL_JOBID", jobId + "%");
            //parameters.Add("@COL_SITE_URL", siteUrl);
            parameters.Add("@OFFSET", offset);  //本次查询的起始位置
            parameters.Add("@LENGTH", length);  //本次查询的总长度
            parameters.Add("@COL_ISSYSTEMFILE", isSystemFile);
            parameters.Add("@COL_NAME", rootSiteName);

            sql = "select * from  " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
            + " where COL_JOBID LIKE @COL_JOBID and (COL_ISSYSTEMFILE LIKE @COL_ISSYSTEMFILE OR COL_ISSYSTEMFILE IS NULL) and COL_NAME != @COL_NAME "
            + " union  all "
            + " select * from  " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
            + " where COL_JOBID LIKE @COL_JOBID and (COL_ISSYSTEMFILE LIKE @COL_ISSYSTEMFILE OR COL_ISSYSTEMFILE IS NULL) "
            + " order by COL_PARENT_PATH_MD5 "
            + " Limit @OFFSET, @LENGTH";

            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            return indexList;
        }

        public ArchiverBasicIndex GetParentFolder(ArchiverBasicIndex childIndex, String version)
        {
            var sqlBefore5200 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                    + " where COL_PATH_MD5 = @COL_PATH_MD5 and COL_ARCHIVE_TIME >= @START_TIME and COL_ARCHIVE_TIME <= @END_TIME order by COL_ARCHIVE_TIME desc";

            var sqlAfter5200 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                    + " where COL_PATH_MD5 = @COL_PATH_MD5 and COL_ARCHIVE_TIME >= @START_TIME and COL_ARCHIVE_TIME <= @END_TIME order by COL_ARCHIVE_TIME desc";
            var index = default(ArchiverBasicIndex);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_PATH_MD5", childIndex.ParentPathMD5);
            parameters.Add("@START_TIME", -1);
            parameters.Add("@END_TIME", childIndex.ArchiveTime);
            var sql = IsBefore5200Version(version) ? sqlBefore5200 : sqlAfter5200;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }

        public Int64 GetIndexTotalCount(String jobId, String isSystemFile)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_JOBID"] = jobId + "%";
            parameters["@COL_ISSYSTEMFILE"] = isSystemFile;

            var sqlHead = "select count(*) from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
            + " where COL_JOBID LIKE @COL_JOBID and (COL_ISSYSTEMFILE LIKE @COL_ISSYSTEMFILE OR COL_ISSYSTEMFILE IS NULL)";

            var sqlBody = " select count(*) from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
            + " where COL_JOBID LIKE @COL_JOBID and (COL_ISSYSTEMFILE LIKE @COL_ISSYSTEMFILE OR COL_ISSYSTEMFILE IS null)";

            //ignore .(root site)
            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlHead, parameters)) + Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlBody, parameters)) - 1;
        }

        #endregion Full Text Index

        #region End User Archiver

        public ArchiverBasicIndex GetIndex(String pathMd5)
        {
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PATH_MD5", pathMd5);
            var sql = "select * from " + IndexConstants.TableNameArchiveHead + " where COL_PATH_MD5 = @COL_PATH_MD5"
                + " union select * from " + IndexConstants.TableNameArchiveBody + " where COL_PATH_MD5 = @COL_PATH_MD5 order by COL_ARCHIVE_TIME desc";
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexList.Count == 0)
                throw new IndexCanNotFoundException("Current path MD5 is not valid.");
            return indexList[0];
        }

        /// <summary>
        /// 专门用于Full text index mode下export数据获取index的功能，根据具体job进行数据的export
        /// </summary>
        /// <param name="pathMd5"></param>
        /// <param name="subJobId"></param>
        /// <returns></returns>
        public ArchiverBasicIndex GetIndex(String pathMd5, String subJobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PATH_MD5", pathMd5);
            parameters.Add("@COL_JOBID", subJobId);
            var sql = "select * from " + IndexConstants.TableNameArchiveBody + " where COL_PATH_MD5 = @COL_PATH_MD5 and COL_JOBID = @COL_JOBID order by COL_ARCHIVE_TIME desc";
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexList.Count == 0)
                throw new IndexCanNotFoundException("Current path MD5 is not valid.");
            return indexList[0];
        }

        public ArchiverBasicIndex GetParentIndex(String pathMd5)
        {
            var currentIndex = this.GetIndex(pathMd5);
            return this.GetIndex(currentIndex.ParentPathMD5);
        }

        public List<ArchiverBasicIndex> GetChildIndexList(String pathMd5)
        {
            var indexList = new List<ArchiverBasicIndex>();
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PARENT_PATH_MD5", pathMd5);
            var sql = "select max(COL_ARCHIVE_TIME), * from " + IndexConstants.TableNameArchiveHead + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 group by COL_PATH_MD5"
                + " union"
                + " select max(COL_ARCHIVE_TIME), * from " + IndexConstants.TableNameArchiveBody + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 group by COL_PATH_MD5";
            indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            return indexList;
        }

        public Int64 GetChildCount(String pathMd5)
        {
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PARENT_PATH_MD5", pathMd5);
            var sql = "select distinct COL_PATH_MD5 from " + IndexConstants.TableNameArchiveHead + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5"
                + " union"
                + " select distinct COL_PATH_MD5 from " + IndexConstants.TableNameArchiveBody + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5";
            var pathList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        public Boolean CheckSiteCollection(String siteUrl)
        {
            var result = false;
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_NAME", siteUrl);
            var sql = "select * from " + IndexConstants.TableNameArchiveHead + " where COL_NAME = @COL_NAME and COL_TYPE = 'E'";
            var list = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (list.Count > 0)
                result = true;
            return result;
        }

        public Boolean CheckNormalUrl(String url)
        {
            var result = false;
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@URL", url);
            var sql = "select * from " + IndexConstants.TableNameArchiveHead + " where COL_EXTENSION_7 = @URL "
                + "union select * from " + IndexConstants.TableNameArchiveBody + " where COL_EXTENSION_7 = @URL";
            var list = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (list.Count > 0)
                result = true;
            return result;
        }

        public Boolean CheckItemUrl(String url)
        {
            var result = false;
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@URL", "%" + url + "%");
            var sql = "select * from " + IndexConstants.TableNameArchiveBody + " where COL_EXTENSION_7 like @URL";
            var list = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (list.Count > 0)
                result = true;
            return result;
        }

        #endregion End User Archiver

        #region EDiscovery Hold Archiver Data

        public ArchiverBasicIndex GetNeedHoldItemFromHeadTable(String jobId, String name, String pathMD5)
        {
            var paramters = new Dictionary<String, Object>();
            ArchiverBasicIndex result = null;
            paramters.Add("@jobId", jobId);
            paramters.Add("@pathMD5", pathMD5);
            String sql = "select * from " + IndexConstants.TableNameArchiveBody
                + " where COL_JOBID = @jobId and COL_PATH_MD5 = @pathMD5";
            var index = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, paramters);
            if (index != null && index.Count > 0)
            {
                result = index[0];
            }
            return result;
        }

        public List<ArchiverBasicIndex> GetAttachments(String parentPathMD5, String name, String type)
        {
            var paramters = new Dictionary<String, Object>();
            List<ArchiverBasicIndex> result = new List<ArchiverBasicIndex>();
            paramters.Add("@type", type);
            paramters.Add("@name", name + "%");
            paramters.Add("@pathMD5", parentPathMD5);
            String sql = "select * from " + IndexConstants.TableNameArchiveBody
                + " where COL_TYPE = @type and COL_PARENT_PATH_MD5 = @pathMD5 and COL_NAME LIKE @name";
            var index = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, paramters);
            if (index != null && index.Count > 0)
            {
                result = index;
            }
            return result;
        }

        #endregion EDiscovery Hold Archiver Data

        #region Convert Stub
        public List<ArchiverBasicIndex> SearchForConvertStub(string stubTypeStr, int pageSize, int pageOffset)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveBody
                                + " where "
                                + " COL_FLAG % 2 = @COL_FLAG "
                                + " and COL_STUBINFO LIKE @COL_STUBTYPE "
                                + " group by COL_PATH_MD5 order by rowid asc");
            var parameterDictionary = new Dictionary<String, object>
            {
                ["@COL_FLAG"] = 0,
                ["@COL_STUBTYPE"] = $"%StubType%{stubTypeStr}%",
            };

            if (pageSize >= 0)
            {
                sqlBuilder.Append(" LIMIT @PageSize OFFSET @PageOffset");
                parameterDictionary.Add("@PageSize", pageSize);
                parameterDictionary.Add("@PageOffset", pageOffset);
            }
            logger.Info($"search sql query is {sqlBuilder.ToString()}");
            var itemIndexs = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sqlBuilder.ToString(), parameterDictionary);
            return itemIndexs;
        }

        public int SearchCountForConvertStub(string stubTypeStr)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameArchiveBody
                              + " where "
                              + " COL_FLAG % 2 = @COL_FLAG "
                              + " and COL_STUBINFO LIKE @COL_STUBTYPE ");

            var parameterDictionary = new Dictionary<string, object>
            {
                ["@COL_FLAG"] = 0,
                ["@COL_STUBTYPE"] = $"%StubType%{stubTypeStr}%",
            };

            logger.Info($"count sql query is {sqlBuilder.ToString()}");

            try
            {
                var count = IndexProcessor.ExecuteScalar(sqlBuilder.ToString(), parameterDictionary);
                return Convert.ToInt32(count);
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting convert stub count: {ex.Message}");
                return 0;
            }
        }

        public void UpdateStubInfo(String colId, String stubInfo)
        {
            var parameters = new Dictionary<String, Object>
            {
                ["@COL_ID"] = colId,
                ["@STUB_INFO"] = stubInfo
            };
            var updateStubInfosql = "update " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " set COL_STUBINFO = @STUB_INFO where COL_ID = @COL_ID";
            IndexProcessor.Execute(updateStubInfosql, parameters);
        }

        public List<ArchiverBasicIndex> GetAllHeadIndexForConvertStub()
        {
            string headsql = "select MAX(COL_ARCHIVE_TIME),[COL_EXTENSION_7], [COL_PATH_MD5],[COL_PARENT_PATH_MD5], [COL_TYPE] "
                            + "from " + IndexConstants.TableNameArchiveHead + " where "
                            + " COL_FLAG % 2 = 0 "
                            + " group by COL_PATH_MD5 order by rowid asc";
            var headIndexes = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(headsql, new());
            return headIndexes;
        }

        #endregion

        #region Delete Archived SC
        public List<ArchiverBasicIndex> GetDeletingIndexesBySubInfo(string storagePolicyId, string jobId, int pageSize, int pageOffset)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select MAX(COL_ARCHIVE_TIME),* from " + IndexConstants.TableNameArchiveBody
                                + " where "
                                + " COL_JOBID = @COL_JOBID "
                                + " and (COL_TYPE = 'D' or COL_TYPE = 'A') " // query document/doc version/attachment to delete their archived content file 
                                + " and COL_STORAGEPOLICYID = @COL_STORAGEPOLICYID "
                                + " group by COL_PATH_MD5 order by rowid asc");
            var parameterDictionary = new Dictionary<String, object>
            {
                ["@COL_JOBID"] = jobId,
                ["@COL_STORAGEPOLICYID"] = storagePolicyId,
            };

            if (pageSize >= 0)
            {
                sqlBuilder.Append(" LIMIT @PageSize OFFSET @PageOffset");
                parameterDictionary.Add("@PageSize", pageSize);
                parameterDictionary.Add("@PageOffset", pageOffset);
            }
            logger.Info($"search sql query is {sqlBuilder.ToString()}");
            var itemIndexs = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sqlBuilder.ToString(), parameterDictionary);
            return itemIndexs;
        }

        public ArchiverBasicIndex GetContainerItem(string pathMd5)
        {
            var sql = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " WHERE COL_PATH_MD5 = @pathMd5 ORDER BY COL_ID LIMIT 1";
            return IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, new Dictionary<string, object> { { "@pathMd5", pathMd5 } }).First();
        }

        public long GetDeletingIndexesCountForDeleteArchivedSC()
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("select COUNT(DISTINCT COL_PATH_MD5) from " + IndexConstants.TableNameArchiveBody
                              + " where (COL_TYPE = 'D' or COL_TYPE = 'A') " // query document/doc version/attachment to delete their archived content file
                              );
            logger.Info($"count sql query is {sqlBuilder.ToString()}");
            try
            {
                var count = IndexProcessor.ExecuteScalar(sqlBuilder.ToString(), []);
                return Convert.ToInt64(count);
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting convert stub count: {ex.Message}");
                return 0L;
            }
        }

        #endregion

        #region private methods

        private List<ArchiverBasicIndex> SortItems(List<ArchiverBasicIndex> items)
        {
            try
            {
                items.Sort((x, y) =>
                {
                    int result = string.Compare(x.ItemName, y.ItemName, StringComparison.OrdinalIgnoreCase);
                    if (result == 0)
                    {
                        if (x.ItemMajorVersion < y.ItemMajorVersion)
                            result = -1;
                        else if (x.ItemMajorVersion > y.ItemMajorVersion)
                            result = 1;
                        else if (Math.Abs(x.ItemMajorVersion - y.ItemMajorVersion) < 1E-06)
                        {
                            if (x.ItemMinorVersion < y.ItemMinorVersion)
                                result = -1;
                            else if (x.ItemMinorVersion > y.ItemMinorVersion)
                                result = 1;
                            else
                            {
                                if (string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase) > 0)
                                    result = -1;
                                else
                                    result = 0;
                            }
                        }
                    }
                    return result;
                });
            }
            catch (Exception ex)
            {
                //Don't sort if sort failed.
                logger.Info($"ArchiverHeadAndBodyIndexService SortItems.Message:{ex}.");
            }
            return items;
        }

        private String GetTableNameByPath(String pathMD5)
        {
            string sql = "select count(COL_ID) from " + IndexConstants.TableNameArchiveBody + " where COL_PATH_MD5= @COL_PATH_MD5";
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMD5;
            long itemCount = (long)IndexProcessor.ExecuteScalar(sql, parameterDictionary);
            return itemCount > 0 ? IndexConstants.TableNameArchiveBody : IndexConstants.TableNameArchiveHead;
        }

        private Boolean IsBefore5200Version(String version)
        {
            return "5.2".CompareToIngnoreCase(version) >= 0;
        }

        public long GetFileCount()
        {
            var sql= "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, null));
        }

        public long GetFileVersionCount()
        {
            var sql= "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, null));
        }
        //generate the function for getting the List of ArchiverBasicIndex's COL_CONTENT_DATA_FILE_NUMBER and return a List<int>
        //COL_ID,COL_EXTRAINFO,COL_EXTENSION_3,COL_EXTENSION_5,COL_EXTENSION_7,COL_EXTENSION_8,COL_JOBID,COL_CYCLEID,COL_CONTENT_DATA_FILE_NUMBER,COL_MODIFY_TIME,COL_BLOB_INFO,
        public List<ArchiverBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId,long dateTime, bool filterSoftDeleteDatas)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            parameters["@dateTime"] = dateTime;
            string sql = string.Empty;
            if (filterSoftDeleteDatas)
            {
                sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_META_TAIL_LENGTH<@dateTime and COL_META_TAIL_LENGTH>0 and COL_RETENTION_STATUS = 1";
            }
            else
            {
                sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_MODIFY_TIME<@dateTime";
            }
            return this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
        }

        #endregion private methods
    }
}