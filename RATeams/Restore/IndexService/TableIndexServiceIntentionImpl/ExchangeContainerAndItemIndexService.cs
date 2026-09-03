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

namespace Office365GroupRestore
{
    #region using directives

    using AngleSharp.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;
    using Cloud.Sdk.Data.EDiscovery;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Merged18NResources.MediaServiceExchangeBackUp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;



    #endregion using directives

    public class ExchangeContainerAndItemIndexService
        : ExchangeTableIndexServiceBase
        , IExchangeContainerAndItemIndexService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExchangeContainerAndItemIndexService));
        private static bool? isPlannerTableExsit = null;

        public List<GroupBasicIndex> GetSubContainers(ExchangeIndexInfo parentIndexInfo)
        {
            var indexList = new List<GroupBasicIndex>();
            var result = new List<GroupBasicIndex>();
            var parentPathMd5 = parentIndexInfo.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PARENT_PATH_MD5"] = parentPathMd5;
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@COL_OFFSET"] = parentIndexInfo.OffSet;
            parameters["@COL_LENGTH"] = parentIndexInfo.Length;
            var attachedString = string.Empty;
            if (parentIndexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_CURRENT_JOB_ID", parentIndexInfo.BackupJobId);
                attachedString = " and COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID ";
            }
            var sql = "select MAX(COL_BACKUP_TIME),* from " + IndexConstants.TableNameExchangeContainer
                + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            indexList.AddRange(this.IndexProcessor.ExecuteQuery<GroupContainerIndex>(sql, parameters));

            if (IsPlannerTableExist(IndexConstants.TableNameExchangePlanner))
            {
                var planSql = "select MAX(COL_BACKUP_TIME),* from " + IndexConstants.TableNameExchangePlanner
                    + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                    + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                    + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
                var planIndexList = this.IndexProcessor.ExecuteQuery<PlannerIndex>(planSql, parameters);
                indexList.AddRange(planIndexList);
            }
            if (indexList.Count == 0)
            {
                sql = "select MAX(COL_BACKUP_TIME),* from " + IndexConstants.TableNameExchangeContainer
                    + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                    + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                    + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
                indexList.AddRange(this.IndexProcessor.ExecuteQuery<GroupContainerIndex>(sql, parameters));
            }
            foreach (var tempResult in indexList)
            {
                if (tempResult.BackupType == 0)
                    result.Add(tempResult);
            }
            return result;
        }

        private bool IsPlannerTableExist(string tableName)
        {
            if (isPlannerTableExsit != null)
            {
                return isPlannerTableExsit.GetValueOrDefault();
            }
            isPlannerTableExsit = IsTableExist(tableName);
            var result = isPlannerTableExsit.GetValueOrDefault();
            return result;
        }

        private bool IsTableExist(string tableName)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@TABLENAME"] = tableName.ToLower();
            var sql = "select * from sqlite_master where type = 'table' and name like @TABLENAME";
            var result = this.IndexProcessor.ExecuteQuery(sql, parameters);
            var exist = result?.Rows?.Count > 0;
            return exist;
        }

        public List<string> GetTopicIds(ExchangeIndexInfo parentIndexInfo)
        {
            var sortIds = new HashSet<string>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@MONTH_START_TIME"] = parentIndexInfo.MonthStartTime;
            parameters["@MONTH_END_TIME"] = parentIndexInfo.MonthEndTime;
            var sql = " select COL_SORT_ID from " + IndexConstants.TableNameExchangeItem
                    + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                    + " and COL_BACKUP_TIME <= @END_TIME "
                    + " and COL_EXTENSION_2 >= @MONTH_START_TIME "
                    + " and COL_EXTENSION_2 <= @MONTH_END_TIME "
                    + " order by COL_EXTENSION_2 asc ";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupItemIndex>(sql, parameters);
            indexList.ForEach(i => sortIds.Add(i.SortId));
            return sortIds.ToList();
        }

        public bool IsTopicIdHistoryExist(ExchangeIndexInfo parentIndexInfo)
        {
            var sortIds = new HashSet<string>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@MONTH_START_TIME"] = parentIndexInfo.MonthStartTime;
            parameters["@COL_SORT_ID"] = parentIndexInfo.SortId;
            var sql = " select COUNT(*) from " + IndexConstants.TableNameExchangeItem
                    + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                    + " and COL_SORT_ID = @COL_SORT_ID "
                    + " and COL_BACKUP_TIME <= @END_TIME "
                    + " and COL_EXTENSION_2 < @MONTH_START_TIME ";
            var count = Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, parameters));
            return count > 0;
            //var count = this.IndexProcessor.ExecuteQuery<GroupItemIndex>(sql, parameters);
            //return Convert.ToInt64(count) > 0;
        }

        public List<long> GetItemCreatedTime(ExchangeIndexInfo indexInfo)
        {
            var result = new List<long>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = indexInfo.EndTime;
            var attachedString = string.Empty;
            if (indexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_CURRENT_JOB_ID", indexInfo.BackupJobId);
                attachedString = " and COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID ";
            }
            var sql = "select distinct COL_EXTENSION_2 from " + IndexConstants.TableNameExchangeItem
               + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(indexInfo, parameters)
               + " and COL_BACKUP_TYPE != 2 "
               + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
               + " order by COL_EXTENSION_2 asc ";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupItemIndex>(sql, parameters);
            result = indexList.Select(itemArg => itemArg.CreateTime).ToList();
            return result;
        }

        public List<GroupBasicIndex> LoadConversationItems(ExchangeIndexInfo parentIndexInfo)
        {
            //var result = new List<GroupBasicIndex>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@COL_OFFSET"] = parentIndexInfo.OffSet;
            parameters["@COL_LENGTH"] = parentIndexInfo.Length;
            parameters["@SORT_ID"] = parentIndexInfo.SortId;
            parameters["@MONTH_START_TIME"] = parentIndexInfo.MonthStartTime;
            parameters["@MONTH_END_TIME"] = parentIndexInfo.MonthEndTime;
            var attachedString = string.Empty;
            if (parentIndexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_CURRENT_JOB_ID", parentIndexInfo.BackupJobId);
                attachedString = " and COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID ";
            }
            var sql = "select * from " + IndexConstants.TableNameExchangeItem
                + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                + " and COL_SORT_ID = @SORT_ID "
                + " and COL_EXTENSION_2 >= @MONTH_START_TIME "
                //+ " and COL_EXTENSION_2 <= @MONTH_END_TIME "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 HAVING MAX(COL_BACKUP_TIME) "
                + " order by COL_EXTENSION_2 asc "
                + " Limit @COL_OFFSET, @COL_LENGTH";
            logger.Info("The parameters: COL_PARENT_PATH_MD5:{0}; END_TIME:{1}; COL_OFFSET:{2}; COL_LENGTH:{3}; COL_PARENT_NODE_ID: {4}.", parameters.TryGet("@COL_PARENT_PATH_MD5"), parentIndexInfo.EndTime, parentIndexInfo.OffSet, parentIndexInfo.Length, parameters.TryGet("@PARENT_ID"));
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);

            //var planSql = "select * from " + IndexConstants.TableNameExchangePlanner
            //  + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
            //  + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
            //  + " group by COL_PATH_MD5"
            //  + " Limit @COL_OFFSET, @COL_LENGTH";
            //logger.Info("The parameters: COL_PARENT_PATH_MD5:{0}; END_TIME:{1}; COL_OFFSET:{2}; COL_LENGTH:{3}.", parentPathMd5, parentIndexInfo.EndTime, parentIndexInfo.OffSet, parentIndexInfo.Length);
            //var planIndexList = this.IndexProcessor.ExecuteQuery<PlannerIndex>(planSql, parameters);
            //indexList.AddRange(planIndexList);

            var tempList = indexList.FindAll(index => index.BackupType == 2);
            foreach (var temp in tempList)
            {
                indexList.RemoveAll(index => index.PathMD5 == temp.PathMD5);
            }
            return indexList;
        }

        public List<GroupBasicIndex> GetSubItems(ExchangeIndexInfo parentIndexInfo)
        {
            //var result = new List<GroupBasicIndex>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@COL_OFFSET"] = parentIndexInfo.OffSet;
            parameters["@COL_LENGTH"] = parentIndexInfo.Length;
            parameters["@SORT_ID"] = parentIndexInfo.SortId;
            parameters["@MONTH_START_TIME"] = parentIndexInfo.MonthStartTime;
            parameters["@MONTH_END_TIME"] = parentIndexInfo.MonthEndTime;
            var attachedString = string.Empty;
            if (parentIndexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_CURRENT_JOB_ID", parentIndexInfo.BackupJobId);
                attachedString = " and COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID ";
            }
            var sql = "select * from " + IndexConstants.TableNameExchangeItem
                + " where COL_PARENT_NODE_ID in " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                + " and COL_SORT_ID = @SORT_ID "
                + " and COL_EXTENSION_2 >= @MONTH_START_TIME "
                + " and COL_EXTENSION_2 <= @MONTH_END_TIME "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 HAVING MAX(COL_BACKUP_TIME) "
                + " order by COL_EXTENSION_2 asc "
                + " Limit @COL_OFFSET, @COL_LENGTH";
            logger.Info("The parameters: COL_PARENT_PATH_MD5:{0}; END_TIME:{1}; COL_OFFSET:{2}; COL_LENGTH:{3}; COL_PARENT_NODE_ID:{4}.", parameters.TryGet("@COL_PARENT_PATH_MD5"), parentIndexInfo.EndTime, parentIndexInfo.OffSet, parentIndexInfo.Length, parameters.TryGet("@PARENT_ID"));
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);

            //var planSql = "select * from " + IndexConstants.TableNameExchangePlanner
            //  + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
            //  + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
            //  + " group by COL_PATH_MD5"
            //  + " Limit @COL_OFFSET, @COL_LENGTH";
            //logger.Info("The parameters: COL_PARENT_PATH_MD5:{0}; END_TIME:{1}; COL_OFFSET:{2}; COL_LENGTH:{3}.", parentPathMd5, parentIndexInfo.EndTime, parentIndexInfo.OffSet, parentIndexInfo.Length);
            //var planIndexList = this.IndexProcessor.ExecuteQuery<PlannerIndex>(planSql, parameters);
            //indexList.AddRange(planIndexList);

            var tempList = indexList.FindAll(index => index.BackupType == 2);
            foreach (var temp in tempList)
            {
                indexList.RemoveAll(index => index.PathMD5 == temp.PathMD5);
            }
            return indexList;
        }

        public int GetSubItemsCount(ExchangeIndexInfo parentIndexInfo)
        {
            var parameters = new Dictionary<String, Object>();
            var sql = " select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
                 + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                 + " group by COL_PATH_MD5 ";
            var pathList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        public Int32 GetOneConversationItemsCount(ExchangeIndexInfo parentIndexInfo)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@SORT_ID"] = parentIndexInfo.SortId;
            var sql = " select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
                 + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(parentIndexInfo, parameters)
                 + " and COL_SORT_ID = @SORT_ID "
                 + " group by COL_PATH_MD5 ";
            var pathList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        public void Insert(List<GroupBasicIndex> indexes)
        {
            IndexProcessor.Insert(indexes);
        }

        public GroupBasicIndex GetOneData(bool isContainer, ExchangeIndexInfo indexInfo)
        {
            var result = new GroupBasicIndex();
            var pathMD5 = indexInfo.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PATH_MD5"] = pathMD5;
            parameters["@END_TIME"] = indexInfo.EndTime;
            var attachedString = string.Empty;
            if (indexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_CURRENT_JOB_ID", indexInfo.BackupJobId);
                attachedString = " and COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID ";
            }
            //var sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME"
            //    + attachedString + " group by COL_PATH_MD5 " + " union "
            //    + "select * from " + IndexConstants.TableNameExchangeContainer + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " order by COL_BACKUP_TIME desc";
            var sql = "select * from " + IndexConstants.TableNameExchangeContainer + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " order by COL_BACKUP_TIME desc";
            if (!isContainer)
                sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " group by COL_PATH_MD5 HAVING MAX(COL_BACKUP_TIME)";
            var infoList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            if (infoList == null || infoList.Count == 0)
            {
                infoList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            }
            if (isContainer && IsPlannerTableExist(IndexConstants.TableNameExchangePlanner))
            {
                var planSql = "select * from " + IndexConstants.TableNameExchangePlanner + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " order by COL_BACKUP_TIME desc";
                var planInfoList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(planSql, parameters);
                infoList.AddRange(planInfoList);
            }
            if (infoList != null && infoList.Count > 0)
                result = infoList[0];
            return result;
        }

        public List<String> GetEntireCycleStorageInfos()
        {
            List<String> storageInfos = new List<String>();
            var sql = "select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeItem
                + " union"
                + " select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeContainer;
            var indexes = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, null);
            foreach (GroupBasicIndex index in indexes)
            {
                storageInfos.Add(index.StorageInfo);
            }
            return storageInfos;
        }

        public List<String> GetStorageInfosExceptFullBackup()
        {
            List<String> storageInfos = new List<String>();
            var sql = "select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeItem
                + " where COL_JOBTYPE <> 'EF'"
                + " union"
                + " select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeContainer
                + " where COL_JOBTYPE <> 'EF'";
            var indexes = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, null);
            foreach (GroupBasicIndex index in indexes)
            {
                storageInfos.Add(index.StorageInfo);
            }
            return storageInfos;
        }

        public List<String> GetStorageInfosByJobId(String jobId)
        {
            List<String> storageInfos = new List<String>();
            var sql = "select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeItem
                + " where COL_JOB_ID = @COL_JOB_ID"
                + " union"
                + " select COL_STORAGEINFO from " + IndexConstants.TableNameExchangeContainer
                + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_JOB_ID", jobId);
            var indexes = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            foreach (GroupBasicIndex index in indexes)
            {
                storageInfos.Add(index.StorageInfo);
            }
            return storageInfos;
        }

        public void DeleteItemByJobId(String jobId)
        {
            var sql = "delete from " + IndexConstants.TableNameExchangeItem + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_JOB_ID", jobId);
            this.IndexProcessor.Execute(sql, parameters);
        }

        public GroupBasicIndex GetParentFolder(GroupBasicIndex childIndex)
        {
            var sql = "select * from " + IndexConstants.TableNameExchangeContainer
                    + " where COL_PATH_MD5 = @COL_PATH_MD5 order by COL_BACKUP_TIME desc"; var index = default(GroupBasicIndex);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_PATH_MD5", childIndex.ParentPathMD5);
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }

        public List<GroupBasicIndex> Search(StringBuilder sql, FilterInfo filter, ExchangeBrowseInfo restoreParam)
        {
            var parameters = new Dictionary<String, Object>();
            if (filter.Condition == FilterCondition.Contains)
            {
                if (filter.Criteria.Contains("*") || filter.Criteria.Contains("?"))
                    filter.Criteria = filter.Criteria.Replace("*", "%").Replace("?", "_");
                else
                    filter.Criteria = "%" + filter.Criteria + "%";
            }
            else if (filter.Condition == FilterCondition.Exactly && filter.RuleType == FilterRuleType.Attribute)
                filter.Criteria = "%" + ServiceConstants.Delimiter + filter.Criteria + ServiceConstants.ExtraChar + "%";
            parameters.Add("@TEXT", filter.Criteria);
            if (restoreParam.OnlyOneJob)
            {
                sql.Append(" and COL_JOB_ID = @COL_JOB_ID");
                parameters.Add("@COL_JOB_ID", restoreParam.BackupJobId);
            }
            if (restoreParam.Level == TreeNodeLevel.ExchangeOnlineFolder)
            {
                if (filter.Level == FilterLevel.Attachment ||
                    filter.Level == FilterLevel.Document ||
                    filter.Level == FilterLevel.Item)
                {
                    sql.Append(" and COL_PARENT_PATH_MD5 in (select distinct COL_PATH_MD5 from tb_container_index where (COL_NAME = @RootPath or COL_NAME like @RootPathListPattern))");
                }
                else
                {
                    sql.Append(" and (COL_NAME = @RootPath or COL_NAME like @RootPathListPattern)");
                }
                parameters.Add("@RootPath", restoreParam.Path);
                parameters.Add("@RootPathListPattern", restoreParam.Path + ServiceConstants.Delimiter + "%");
            }
            else if (restoreParam.Level == TreeNodeLevel.ExchangeOnlineMailbox)
            {
                if (filter.Level == FilterLevel.Attachment ||
                    filter.Level == FilterLevel.Document ||
                    filter.Level == FilterLevel.Item)
                {
                    sql.Append(" and COL_PARENT_PATH_MD5 in (select distinct COL_PATH_MD5 from tb_head_index where (COL_NAME like @RootPathSitePattern or COL_NAME like @RootPathListPattern))");
                }
                else
                {
                    sql.Append(" and (COL_NAME like @RootPathSitePattern or COL_NAME like @RootPathListPattern)");
                }
                parameters.Add("@RootPathSitePattern", restoreParam.Path + ServiceConstants.Delimiter + "%");
                parameters.Add("@RootPathListPattern", restoreParam.Path + ServiceConstants.Delimiter + "%");
            }
            sql.Append(" and COL_BACKUP_TIME <= @ENDTIME  group by COL_PATH_MD5 order by COL_NAME desc limit @COUNT");
            parameters.Add("@ENDTIME", restoreParam.EndTime);
            var count = MediaEnvironment.MediaServer.MediaServerMaxSearchCount > 0 ? MediaEnvironment.MediaServer.MediaServerMaxSearchCount : 500;
            parameters.Add("@COUNT", count);
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeContainerAndItemIndexServiceSearchStartExecutingStructuredQueryLanguage, sql.ToString(), CollectionExpand.Expand(parameters));
            return this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql.ToString(), parameters);
        }

        private void UpdateHasAttachColumn()
        {
            try
            {
                logger.Info("Update table [tb_container_index], column [COL_HAS_ATTACH].");
                var sqlUpdateContainerIndex = " update " + IndexConstants.TableNameExchangeContainer
                    + " set COL_HAS_ATTACH = 0 "
                    + " where COL_HAS_ATTACH = '' "
                    + " or COL_HAS_ATTACH is NULL; "
                    + " update " + IndexConstants.TableNameExchangeContainer
                    + " set COL_SEND_DATE = 0 "
                    + " where COL_SEND_DATE = '' "
                    + " or COL_SEND_DATE is NULL";
                var parameters = new Dictionary<String, Object>();
                this.IndexProcessor.Execute(sqlUpdateContainerIndex, parameters);

                logger.Info("Update table [tb_item_index], column [COL_HAS_ATTACH].");
                var sqlUpdateItemIndex = " update " + IndexConstants.TableNameExchangeItem
                   + " set COL_HAS_ATTACH = 0 "
                   + " where COL_HAS_ATTACH = '' "
                   + " or COL_HAS_ATTACH is NULL; "
                   + " update " + IndexConstants.TableNameExchangeItem
                   + " set COL_SEND_DATE = 0 "
                   + " where COL_SEND_DATE = '' "
                   + " or COL_SEND_DATE is NULL";
                this.IndexProcessor.Execute(sqlUpdateItemIndex, parameters);
            }
            catch (Exception ex)
            {
                logger.Warn("Update column [COL_HAS_ATTACH] with exception: {0}", ex.ToString());
            }
        }

        public void ProcessColumnUpgrate()
        {
            logger.Info("Upgrate column [COL_HAS_ATTACH].");
            ProcessColumnUpgrate(ContainerTableName);
            ProcessColumnUpgrate(ItemTableName);
            UpdateHasAttachColumn();
        }

        public void CreateIndex(String columnName)
        {
            try
            {
                logger.Info("Create index on column [{0}].", columnName);
                string idxName = "IDX_" + columnName;

                var sqlCreateIndex = string.Format(@"CREATE INDEX
                        IF NOT EXISTS {0}
                        on tb_item_index({1} asc) ", idxName, columnName);
                var parameters = new Dictionary<String, Object>();
                this.IndexProcessor.Execute(sqlCreateIndex, parameters);
            }
            catch (Exception ex)
            {
                logger.Warn("Create index on column [{0}] with exception: {1}", columnName, ex.ToString());
            }
        }

        public long? GetOldestMessageCreateDate()
        {
            var sql = $"SELECT MIN(COL_SEND_DATE) FROM {IndexConstants.TableNameExchangeItem}";
            var result = this.IndexProcessor.ExecuteScalar(sql, null);
            if (result == null || result == DBNull.Value)
            {
                return null;
            }
            return Convert.ToInt64(result);
        }

        private void ProcessColumnUpgrate(string tableName)
        {
            try
            {
                var parameters = new Dictionary<String, Object>();

                var sqlCreateTempTable = string.Format(" ALTER TABLE {0}_index RENAME TO {0}_temp; ", tableName);
                this.IndexProcessor.Execute(sqlCreateTempTable, parameters);

                var sqlCreateTableScript = tableName.Equals(ContainerTableName) ? CreateContainerTableScript : CreateItemTableScript;
                this.IndexProcessor.Execute(sqlCreateTableScript, parameters);

                var sqlInsertRecord = string.Format(" INSERT INTO {0}_index SELECT * FROM {0}_temp; ", tableName);
                this.IndexProcessor.Execute(sqlInsertRecord, parameters);

                var sqlDropTable = string.Format(" drop table {0}_temp; ", tableName);
                this.IndexProcessor.Execute(sqlDropTable, parameters);
            }
            catch (Exception ex)
            {
                logger.Warn("Upgrate table [{0}_index] column type with exception: {1}", tableName, ex.ToString());
                RollBack(tableName);
            }
        }

        #region CreateTableScript

        private const string ContainerTableName = "tb_container";
        private const string ItemTableName = "tb_item";

        private const string CreateContainerTableScript = @"CREATE TABLE [tb_container_index] (
                                          [COL_ID] CHAR(36) not null ,
                                          [COL_FLAG] BIGINT,
                                          [COL_TYPE] INT,
                                          [COL_PATH] VARCHAR(256),
                                          [COL_NAME] VARCHAR(32672),
                                          [COL_PLAN_ID] VARCHAR(256),
                                          [COL_JOB_ID] VARCHAR(256),
                                          [COL_CYCLE_ID] VARCHAR(256),
                                          [COL_JOB_TYPE] VARCHAR(256),
                                          [COL_PATH_MD5] CHAR(32),
                                          [COL_PARENT_PATH_MD5] CHAR(32),
                                          [COL_BACKUP_TYPE] INT,
                                          [COL_BACKUP_TIME] BIGINT,
                                          [COL_DATA_FILE_OFFSET] BIGINT,
                                          [COL_DATA_FILE_LENGTH] BIGINT,
                                          [COL_DATA_FILE_NUMBER] BIGINT,
                                          [COL_DATA_FILE_PREFIX_NUMBER] BIGINT,
                                          [COL_META_DATA_HEADER_OFFSET] BIGINT,
                                          [COL_CONTENT_OFFSET] BIGINT,
                                          [COL_CONTENT_LENGTH] BIGINT,
                                          [COL_CONTENT_DATA_OFFSET] BIGINT,
                                          [COL_CONTENT_DATA_HEADER_OFFSET] BIGINT,
                                          [COL_CONTENT_DATA_FILE_NUMBER] BIGINT,
                                          [COL_CONTENT_DATA_FILE_PREFIX_NUMBER] BIGINT,
                                          [COL_CONTENT_PAGE_SIZE] BIGINT,
                                          [COL_SEQUENCE] BIGINT,
                                          [COL_PLATFORM_TYPE] INT,
                                          [COL_VERSION] BIGINT,
                                          [COL_ATTRIBUTES] VARCHAR(32762),
                                          [COL_STORAGEINFO] TEXT,
                                          [COL_CRC] BIGINT,
                                          [COL_STORAGE_CRC32] VARCHAR(32672),
                                          [COL_NODE_TYPE] INT,
                                          [COL_EXTENSION_1] INT,
                                          [COL_EXTENSION_2] BIGINT,
                                          [COL_EXTENSION_3] VARCHAR(32),
                                          [COL_EXTENSION_4] TEXT,
                                          [COL_SENDER] TEXT,
                                          [COL_DISPLAY_TO] TEXT,
                                          [COL_SEND_DATE] BIGINT,
                                          [COL_HAS_ATTACH] BOOLEAN,
                                          [COL_CATEGORY] TEXT,
                                          [COL_CURRENT_JOB_ID] VARCHAR(256)
                                );";

        private const string CreateItemTableScript = @"CREATE TABLE [tb_item_index](
                                          [COL_ID] CHAR(36) not null ,
                                          [COL_FLAG] BIGINT,
                                          [COL_TYPE] INT,
                                          [COL_PATH] VARCHAR(256),
                                          [COL_NAME] VARCHAR(32672),
                                          [COL_PLAN_ID] VARCHAR(256),
                                          [COL_JOB_ID] VARCHAR(256),
                                          [COL_CYCLE_ID] VARCHAR(256),
                                          [COL_JOB_TYPE] VARCHAR(256),
                                          [COL_PATH_MD5] CHAR(32),
                                          [COL_PARENT_PATH_MD5] CHAR(32),
                                          [COL_BACKUP_TYPE] INT,
                                          [COL_BACKUP_TIME] BIGINT,
                                          [COL_DATA_FILE_OFFSET] BIGINT,
                                          [COL_DATA_FILE_LENGTH] BIGINT,
                                          [COL_DATA_FILE_NUMBER] BIGINT,
                                          [COL_DATA_FILE_PREFIX_NUMBER] BIGINT,
                                          [COL_META_DATA_HEADER_OFFSET] BIGINT,
                                          [COL_CONTENT_OFFSET] BIGINT,
                                          [COL_CONTENT_LENGTH] BIGINT,
                                          [COL_CONTENT_DATA_OFFSET] BIGINT,
                                          [COL_CONTENT_DATA_HEADER_OFFSET] BIGINT,
                                          [COL_CONTENT_DATA_FILE_NUMBER] BIGINT,
                                          [COL_CONTENT_DATA_FILE_PREFIX_NUMBER] BIGINT,
                                          [COL_CONTENT_PAGE_SIZE] BIGINT,
                                          [COL_SEQUENCE] BIGINT,
                                          [COL_PLATFORM_TYPE] INT,
                                          [COL_VERSION] BIGINT,
                                          [COL_ATTRIBUTES] VARCHAR(32762),
                                          [COL_STORAGEINFO] TEXT,
                                          [COL_CRC] BIGINT,
                                          [COL_STORAGE_CRC32] VARCHAR(32672),
                                          [COL_NODE_TYPE] INT,
                                          [COL_EXTENSION_1] INT,
                                          [COL_EXTENSION_2] BIGINT,
                                          [COL_EXTENSION_3] VARCHAR(32),
                                          [COL_EXTENSION_4] TEXT,
                                          [COL_SENDER] TEXT,
                                          [COL_DISPLAY_TO] TEXT,
                                          [COL_SEND_DATE] BIGINT,
                                          [COL_HAS_ATTACH] BOOLEAN,
                                          [COL_CATEGORY] TEXT,
                                          [COL_CURRENT_JOB_ID] VARCHAR(256)
                                );";

        #endregion

        private void RollBack(string tableName)
        {
            try
            {
                if (CheckTableExist(tableName))
                {
                    var parameters = new Dictionary<String, Object>();
                    var sqlDropTable = string.Format(" drop table {0}_index; ", tableName);
                    this.IndexProcessor.Execute(sqlDropTable, parameters);

                    var sqlRollBack = string.Format(" ALTER TABLE {0}_temp RENAME TO {0}_index; ", tableName);
                    this.IndexProcessor.Execute(sqlRollBack, parameters);
                }
            }
            catch (Exception e)
            {
                logger.Warn("A rollback operation failed with exception: {0}", e.ToString());
            }
        }

        private bool CheckTableExist(string tableName)
        {
            try
            {
                var parameters = new Dictionary<String, Object>();
                var sqlCheckTable = string.Format(" SELECT count(*) FROM sqlite_master WHERE type='table' and name = '{0}_temp'; ", tableName);
                object count = this.IndexProcessor.ExecuteScalar(sqlCheckTable, parameters);

                return Convert.ToInt64(count) == 1;
            }
            catch (Exception ex)
            {
                logger.Warn("Table [{0}_temp] not exist. Reason: {1}", tableName, ex.ToString());
            }
            return false;
        }

        private string GenerateParentNodeIdSelectQuery(ExchangeIndexInfo parentIndexInfo, Dictionary<string, Object> parameters)
        {
            string result = string.Empty;
            if (parentIndexInfo.ParentId != null)
            {
                result = "@PARENT_ID";
                parameters.TryAdd("@PARENT_ID", parentIndexInfo.ParentId);
            }
            else
            {
                result = "(select distinct COL_NODE_ID from tb_container_index where COL_PATH_MD5 = @COL_PARENT_PATH_MD5 order by rowid desc limit 1)";
                parameters.TryAdd("@COL_PARENT_PATH_MD5", parentIndexInfo.Path.ToMD5HashCode());
            }
            return result;
        }

        public Dictionary<string, string> LoadEXONameAndMd5Mapping()
        {
            var parameters = new Dictionary<String, Object>();
            Dictionary<string, string> result = new Dictionary<string, string>();
            var sql = "select COL_PATH_MD5,COL_NAME from " + IndexConstants.TableNameExchangeContainer + " group by COL_PATH_MD5 order by COL_BACKUP_TIME desc";
            var infoList = this.IndexProcessor.ExecuteQuery<ExchangeBasicIndex>(sql, parameters);
            if (infoList != null && infoList.Count > 0)
            {
                foreach (var item in infoList)
                {
                    result.Add(item.PathMD5, item.Name);
                }
            }
            return result;
        }

        public List<ExchangeBasicIndex> GetItemsByParentMd5(string parentMd5)
        {
            var parameters = new Dictionary<String, Object>();
            var sql = "select * from "+ IndexConstants.TableNameExchangeItem + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 group by COL_PATH_MD5 order by COL_BACKUP_TIME desc";
            parameters.Add("@COL_PARENT_PATH_MD5", parentMd5);
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeContainerAndItemIndexServiceSearchStartExecutingStructuredQueryLanguage, sql.ToString(), CollectionExpand.Expand(parameters));
            return this.IndexProcessor.ExecuteQuery<ExchangeBasicIndex>(sql.ToString(), parameters);
        }

        public List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInHeadByParentPathMd5(string parentPath)
        {
            logger.Info(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadItemsStart, parentPath);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            if (parentPath == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadFoldersArgumentNullException);
            }
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 order by rowid asc";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = parentPath;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = DateTime.MaxValue.Ticks;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            stopwatch.Stop();
            logger.Info($"Get Item By ParentPathMd5 cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count}");
            return indexList;
        }

        public ArchiverBasicIndex GetArchiverBasicIndexByPathMd5(string pathMd5)
        {
            logger.Info($"Load Archiver Basic Index by pathMD5 {pathMd5}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            if (pathMd5 == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadFoldersArgumentNullException);
            }
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 order by rowid asc";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMd5;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = DateTime.MaxValue.Ticks;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary).FirstOrDefault() ?? new();
            stopwatch.Stop();
            logger.Info($"Load Archiver Basic Index by pathMD5 cost time:{stopwatch.ElapsedMilliseconds}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetArchiverBasicIndexItemsInBodyByParentPathMd5(string parentPathMd5)
        {
            logger.Info($"Load Item in Body table by parent path Md5 {parentPathMd5}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            if(parentPathMd5 == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadFoldersArgumentNullException);
            }
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
            + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
            + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
            + " group by COL_PATH_MD5 order by rowid asc";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = parentPathMd5;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = DateTime.MaxValue.Ticks;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            stopwatch.Stop();
            logger.Info($"Get Item in Body table By ParentPathMd5 cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count}");
            return indexList;
        }
    }
}