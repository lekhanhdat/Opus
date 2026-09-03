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

namespace RAExchangeRetention
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    using AvePoint.Media.Common;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;



    #endregion using directives

    public class ExchangeContainerAndItemIndexService
        : ExchangeTableIndexServiceBase
        , IExchangeContainerAndItemIndexService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExchangeContainerAndItemIndexService));


        public void UpdateAsSoftDelete(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var deleteBodyTable = "update " + IndexConstants.TableNameExchangeItem + " set COL_RETENTION_STATUS = 1 where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }

        public void UpdateAccessTier(int tier, string jobid)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = jobid;
            parameters["@tier"] = tier;
            var updateC = "update " + IndexConstants.TableNameExchangeContainer + " set COL_STORAG_ACCESSTIERTYPE = @tier where COL_JOB_ID = @jobId";
            var updateI = "update " + IndexConstants.TableNameExchangeItem + " set COL_STORAG_ACCESSTIERTYPE = @tier where COL_JOB_ID = @jobId";
            //var updateP = "update " + IndexConstants.TableNameExchangePlanner + " set COL_STORAG_ACCESSTIERTYPE = @tier where COL_JOB_ID = @jobId";

            this.IndexProcessor.Execute(updateC, parameters);
            this.IndexProcessor.Execute(updateI, parameters);
            //this.IndexProcessor.Execute(updateP, parameters);
        }

        public bool IsExistsIndexRelatedToJob(string jobId)
        {
            //var parameters = new Dictionary<String, Object>();
            //parameters["@JobId"] = $"%{jobId}%";
            //var deleteBodyTable = $"SELECT COL_ID FROM {IndexConstants.TableNameExchangeItem} WHERE COL_POOL_GUID LIKE @JobId LIMIT 1;";
            //var result = this.IndexProcessor.ExecuteScalar(deleteBodyTable, parameters);
            //return result != null;
            return false; // Teams & Mailbox not support deduplication
        }

        public List<GroupBasicIndex> GetSubContainers(ExchangeIndexInfo parentIndexInfo)
        {
            var result = new List<GroupBasicIndex>();
            var parentPathMd5 = parentIndexInfo.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PARENT_PATH_MD5"] = parentPathMd5;
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@COL_OFFSET"] = parentIndexInfo.OffSet;
            parameters["@COL_LENGTH"] = parentIndexInfo.Length;
            //parameters["@COL_JOB_ID"] = parentIndexInfo.BackupJobId;
            var attachedString = string.Empty;
            if (parentIndexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_JOB_ID", parentIndexInfo.BackupJobId);
                attachedString = " and COL_JOB_ID = @COL_JOB_ID ";
            }
            var sql = "select MAX(COL_BACKUP_TIME),* from " + IndexConstants.TableNameExchangeContainer
                + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            foreach (var tempResult in indexList)
            {
                if (tempResult.BackupType == 0)
                    result.Add(tempResult);
            }
            return result;
        }

        public List<GroupBasicIndex> GetSubItems(ExchangeIndexInfo parentIndexInfo)
        {
            var result = new List<GroupBasicIndex>();
            var parentPathMd5 = parentIndexInfo.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PARENT_PATH_MD5"] = parentPathMd5;
            parameters["@END_TIME"] = parentIndexInfo.EndTime;
            parameters["@COL_OFFSET"] = parentIndexInfo.OffSet;
            parameters["@COL_LENGTH"] = parentIndexInfo.Length;
            //parameters["@COL_JOB_ID"] = parentIndexInfo.BackupJobId;
            var attachedString = string.Empty;
            if (parentIndexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_JOB_ID", parentIndexInfo.BackupJobId);
                attachedString = " and COL_JOB_ID = @COL_JOB_ID ";
            }
            var sql = "select * from " + IndexConstants.TableNameExchangeItem
                + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " Limit @COL_OFFSET, @COL_LENGTH";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
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
            parameters.Add("@COL_PARENT_PATH_MD5", parentIndexInfo.Path.ToMD5HashCode());
            var sql = " select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5";
            var pathList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        public void Insert(List<GroupBasicIndex> indexes)
        {
            IndexProcessor.Insert(indexes);
        }

        public GroupBasicIndex GetOneData(ExchangeIndexInfo indexInfo)
        {
            var result = new GroupBasicIndex();
            var pathMD5 = indexInfo.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PATH_MD5"] = pathMD5;
            parameters["@END_TIME"] = indexInfo.EndTime;
            //parameters["@COL_JOB_ID"] = indexInfo.BackupJobId;
            var attachedString = string.Empty;
            if (indexInfo.OnlyOneJob)
            {
                parameters.Add("@COL_JOB_ID", indexInfo.BackupJobId);
                attachedString = " and COL_JOB_ID = @COL_JOB_ID ";
            }
            var sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME"
                + attachedString + " union "
                + "select * from " + IndexConstants.TableNameExchangeContainer + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " order by COL_BACKUP_TIME desc";
            var infoList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
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
                    + " where COL_PATH_MD5 = @COL_PATH_MD5 and COL_BACKUP_TIME >= @START_TIME and COL_BACKUP_TIME <= @END_TIME order by COL_BACKUP_TIME desc";
            var index = default(GroupBasicIndex);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_PATH_MD5", childIndex.ParentPathMD5);
            parameters.Add("@START_TIME", -1);
            parameters.Add("@END_TIME", childIndex.BackupTime);
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }

        public void UpdateFormerJobIdToCurrentJobId(String jobId)
        {
            var sqlContainer = "update " + IndexConstants.TableNameExchangeContainer + " set COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID";
            var parametersContainer = new Dictionary<String, Object>();
            parametersContainer.Add("@COL_CURRENT_JOB_ID", jobId);
            var sqlItem = "update " + IndexConstants.TableNameExchangeItem + " set COL_CURRENT_JOB_ID = @COL_CURRENT_JOB_ID";
            var parametersItem = new Dictionary<String, Object>();
            parametersItem.Add("@COL_CURRENT_JOB_ID", jobId);
            this.IndexProcessor.Execute(sqlContainer, parametersContainer);
            this.IndexProcessor.Execute(sqlItem, parametersItem);
        }

        public void DeleteDeleteTypeData()
        {
            DeleteDeleteFolderData();
            DeleteDeleteItemData();
        }

        //public void UpdateDuplicatedData()
        //{
        //    var sqlUpdate = " delete from " + IndexConstants.TableNameExchangeItem
        //        + " where COL_PATH_MD5 in (select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
        //        + " group by COL_PATH_MD5 having count(COL_PATH_MD5) > 1) "
        //        + " and COL_ID not in (select COL_ID from " + IndexConstants.TableNameExchangeItem
        //        + " group by COL_PATH_MD5 having count(COL_PATH_MD5) >1) ";
        //    var parameters = new Dictionary<String, Object>();
        //    this.IndexProcessor.Execute(sqlUpdate, parameters);

        //    sqlUpdate = " delete from " + IndexConstants.TableNameExchangeContainer
        //        + " where COL_PATH_MD5 in (select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeContainer
        //        + " group by COL_PATH_MD5 having count(COL_PATH_MD5) > 1) "
        //        + " and COL_ID not in (select COL_ID from " + IndexConstants.TableNameExchangeContainer
        //        + " group by COL_PATH_MD5 having count(COL_PATH_MD5) >1) ";
        //    this.IndexProcessor.Execute(sqlUpdate, parameters);
        //}

        private void DeleteDeleteFolderData()
        {
            var sqlContainer = " select * from " + IndexConstants.TableNameExchangeContainer
                + " where COL_BACKUP_TYPE = @COL_BACKUP_TYPE ";
            var parametersContainer = new Dictionary<String, Object>();
            parametersContainer.Add("@COL_BACKUP_TYPE", 2);
            var resultList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sqlContainer, parametersContainer);
            foreach (var result in resultList)
            {
                var sqlContainerDelete = " delete from " + IndexConstants.TableNameExchangeContainer
               + " where COL_PATH_MD5 = @COL_PATH_MD5 ";
                var parameters = new Dictionary<String, Object>();
                parameters.Add("@COL_PATH_MD5", result.PathMD5);
                this.IndexProcessor.Execute(sqlContainerDelete, parameters);

                DeleteDeleteFolderData(result.PathMD5);
            }
        }

        public void UpdateHasAttachColumn()
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

        private void DeleteDeleteFolderData(string parentPathMD5)
        {
            var sqlContainer = " select * from " + IndexConstants.TableNameExchangeContainer
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 ";
            var parametersContainer = new Dictionary<String, Object>();
            parametersContainer.Add("@COL_PARENT_PATH_MD5", parentPathMD5);
            var resultList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sqlContainer, parametersContainer);

            var sqlDelete = " delete from " + IndexConstants.TableNameExchangeContainer
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5; "
                + " delete from " + IndexConstants.TableNameExchangeItem
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 ";
            this.IndexProcessor.Execute(sqlDelete, parametersContainer);

            foreach (var result in resultList)
            {
                DeleteDeleteFolderData(result.PathMD5);
            }
        }

        private void DeleteDeleteItemData()
        {
            var sqlDelete = " delete from " + IndexConstants.TableNameExchangeItem
                + " where COL_PATH_MD5 in ("
                + "select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
                + " where COL_BACKUP_TYPE = @COL_BACKUP_TYPE ) ";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_BACKUP_TYPE", 2);
            this.IndexProcessor.Execute(sqlDelete, parameters);
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
            //this.logger.Info(MediaServiceExchangeBackupResource.ExchangeContainerAndItemIndexServiceSearchStartExecutingStructuredQueryLanguage, sql.ToString(), CollectionExpand.Expand(parameters));
            return this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql.ToString(), parameters);
        }

        public Int64 GetRepeatContainerCount(string jobId)
        {
            var parameters = new Dictionary<String, Object>();
            string attachedString = string.Empty;
            if (!string.IsNullOrEmpty(jobId))
            {
                parameters["@COL_JOB_ID"] = jobId;
                attachedString = " and COL_JOB_ID = @COL_JOB_ID ";//POC-12707(COL_CURRENT_JOB_ID改成COL_JOB_ID)
            }
            var sql = @"select count(*) count from " + IndexConstants.TableNameExchangeContainer
                      + " where col_path not like '%%' " + attachedString
                      + " group by col_path_md5, col_job_id having count > 1";

            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, parameters));
        }

        public Int64 GetIndexTotalCount(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_CURRENT_JOB_ID"] = jobId + "%";

            var sqlHead = "select count(*) from " + IndexConstants.TableNameExchangeContainer
            + " where COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID ";

            var sqlBody = " select count(*) from " + IndexConstants.TableNameExchangeItem
            + " where COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID ";

            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlHead, parameters)) + Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlBody, parameters));
        }

        public List<GroupBasicIndex> GetNeedFiles(String jobId, Int32 offset, Int32 length)
        {
            var sql = default(String);
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_CURRENT_JOB_ID", jobId + "%");

            parameters.Add("@OFFSET", offset);  //本次查询的起始位置
            parameters.Add("@LENGTH", length);  //本次查询的总长度

            sql = "select distinct COL_PATH,COL_NAME,COL_TYPE,COL_DATA_FILE_LENGTH from  " + IndexConstants.TableNameExchangeContainer
            + " where COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID "
            + " union  all "
            + " select distinct COL_PATH,COL_NAME,COL_TYPE,COL_DATA_FILE_LENGTH from  " + IndexConstants.TableNameExchangeItem
             + " where COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID "
            + " order by COL_PATH "
            + " Limit @OFFSET, @LENGTH";

            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            return indexList;
        }

        public List<GroupBasicIndex> GetMetaDataIndexs()
        {
            var sql = default(String);
            var parameters = new Dictionary<String, Object>();
            sql = " select distinct COL_PLAN_ID,COL_CYCLE_ID,COL_JOB_ID,COL_DATA_FILE_NUMBER,COL_DATA_FILE_PREFIX_NUMBER,COL_STORAGEINFO from " +
                "( select distinct COL_PLAN_ID,COL_CYCLE_ID,COL_JOB_ID,COL_DATA_FILE_NUMBER,COL_DATA_FILE_PREFIX_NUMBER,COL_STORAGEINFO from " + IndexConstants.TableNameExchangeContainer +
                " union  all " +
                " select distinct COL_PLAN_ID,COL_CYCLE_ID,COL_JOB_ID,COL_DATA_FILE_NUMBER,COL_DATA_FILE_PREFIX_NUMBER,COL_STORAGEINFO from " + IndexConstants.TableNameExchangeItem + ")";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            return indexList;
        }

        public List<GroupBasicIndex> GetContentDataIndexs()
        {
            var sql = default(String);
            var parameters = new Dictionary<String, Object>();
            sql = "select distinct COL_PLAN_ID,COL_CYCLE_ID,COL_JOB_ID,COL_CONTENT_DATA_FILE_NUMBER,COL_CONTENT_DATA_FILE_PREFIX_NUMBER,COL_STORAGEINFO from " + IndexConstants.TableNameExchangeItem;
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            return indexList;
        }

        public void CreateIndexContainerAndItemIndex()
        {
            var createIndexContainerCommand = "CREATE INDEX IF NOT EXISTS IDX_CONTAINER_PATH on " + IndexConstants.TableNameExchangeContainer + "(COL_PATH asc)";
            var parameters = new Dictionary<String, Object>();
            this.IndexProcessor.Execute(createIndexContainerCommand, parameters);
            var createIndexItemCommand = "CREATE INDEX IF NOT EXISTS IDX_ITEM_PATH on " + IndexConstants.TableNameExchangeItem + "(COL_PATH asc)";
            this.IndexProcessor.Execute(createIndexItemCommand, parameters);
        }

        public Int64 GetContainerIndexTotalCount(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_CURRENT_JOB_ID"] = jobId + "%";

            var sqlHead = "select count(*) from " + IndexConstants.TableNameExchangeContainer
            + " where COL_ID in (select COL_ID from " + IndexConstants.TableNameExchangeContainer
            + " group by COL_PATH) "
            + " and COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID ";

            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlHead, parameters));
        }

        public Int64 GetItemIndexTotalCount(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_CURRENT_JOB_ID"] = jobId + "%";

            var sqlBody = " select count(*) from " + IndexConstants.TableNameExchangeItem
            + " where COL_ID in (select COL_ID from " + IndexConstants.TableNameExchangeItem
            + " group by COL_PATH) "
            + " and COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID ";

            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlBody, parameters));
        }

        public Int64 GetItemTotalSize(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_CURRENT_JOB_ID"] = jobId + "%";

            var sqlBody = " select sum(COL_DATA_FILE_LENGTH) from " + IndexConstants.TableNameExchangeItem
                + " where COL_ID in (select COL_ID from " + IndexConstants.TableNameExchangeItem
                + " where COL_CURRENT_JOB_ID LIKE @COL_CURRENT_JOB_ID "
                + " group by COL_PATH) ";

            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sqlBody, parameters));
        }

        public void DeleteContainerAndItemIndexByJobId(String jobId)
        {
            var removeCommand = "delete from " + IndexConstants.TableNameExchangeContainer + " where COL_JOB_ID = @COL_JOB_ID ; "
                + "delete from " + IndexConstants.TableNameExchangeItem + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_JOB_ID", jobId);
            this.IndexProcessor.Execute(removeCommand, parameters);
        }

        public void DeleteContainerAndItemIndexByStorageAndJobId(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            //parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var deleteContainerTable = "delete from " + IndexConstants.TableNameExchangeItem + " where COL_JOB_ID = @jobId";
            var deleteItemTable = "delete from " + IndexConstants.TableNameExchangeContainer + " where COL_JOB_ID = @jobId";
            //var deletePlannerTable = "delete from " + IndexConstants.TableNameExchangeContainer + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            this.IndexProcessor.Execute(deleteContainerTable, parameters);
            this.IndexProcessor.Execute(deleteItemTable, parameters);
            //this.IndexProcessor.Execute(deletePlannerTable, parameters);
        }

        public void ProcessColumnUpgrate()
        {
            logger.Info("Upgrate column [COL_HAS_ATTACH].");
            ProcessColumnUpgrate(ContainerTableName);
            ProcessColumnUpgrate(ItemTableName);
        }

        public int GetContainerCount()
        {
            var sql = "SELECT COUNT(DISTINCT COL_NAME) FROM " + IndexConstants.TableNameExchangeContainer + " WHERE COL_TYPE = 0";
            var parameters = new Dictionary<string, object>();
            return Convert.ToInt32(IndexProcessor.ExecuteScalar(sql, parameters));
        }

        public bool HasContainter(string pathMd5)
        {
            var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameExchangeContainer + " WHERE COL_TYPE = 0 AND COL_PATH_MD5 = @COL_PATH_MD5";
            var parameters = new Dictionary<string, object> { ["@COL_PATH_MD5"] = pathMd5 };
            return Convert.ToInt32(IndexProcessor.ExecuteScalar(sql, parameters)) > 0;
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
    }
}