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
using AvePoint.GCommon;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common.Util;
using Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntention;
using Merged18NResources.MediaServiceArchiverBackup;
using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.SqlClient;
using AvePoint.GCommon.Contract.CommonFilter;

namespace Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntentionImpl
{
    public class GDriveArchiverHeadAndBodyIndexService : GDriveArchiverTableIndexServiceBase
        , IGDriveArchiverHeadAndBodyIndexService
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, string> OrderByColMapping = new Dictionary<string, string>
        {
            {"Name", "COL_NAME"},
            {"ArchvieTime", "COL_ARCHIVE_TIME"}
        };
        public void InsertArchiveIndexes(List<GoogleBasicIndex> indexes)
        {
            IndexProcessor.Insert(indexes);
        }
        public String GetItemName(Int64 contentFileNumber, String jobId)
        {
            var itemName = String.Empty;
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_CONTENT_DATA_FILE_NUMBER", contentFileNumber);
            parameters.Add("@COL_JOB_ID", jobId + "%");
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                   + " where COL_CONTENT_DATA_FILE_NUMBER = @COL_CONTENT_DATA_FILE_NUMBER AND COL_JOB_ID LIKE @COL_JOB_ID";
            var indexes = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
            if (indexes.Count > 0)
            {
                itemName = indexes[0].Name;
            }
            return itemName;
        }
        private List<string> GetFoldersMD5(GoogleBasicIndex parent, List<GoogleBasicIndex> indexList)
        {
            List<string> folderMD5 = new List<string>();
            foreach (var child in indexList)
            {
                if (child.Type == 0)
                {
                    continue;
                }
                if (parent.PathMD5 == child.ParentPathMD5)
                {
                    folderMD5.Add(child.PathMD5);
                    var list = GetFoldersMD5(child, indexList);
                    folderMD5.AddRange(list);
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
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            }
            else
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_JOB_ID = @jobId";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectMd5, parameters);
            List<GoogleBasicIndex> webPathMD5 = new List<GoogleBasicIndex>();
            Dictionary<string, List<string>> currentWebFolders = new Dictionary<string, List<string>>();
            foreach (var webPath in indexList)
            {
                //if (webPath.Type == "W")
                {
                    webPathMD5.Add(webPath);
                }
            }
            foreach (var pm5 in webPathMD5)
            {
                List<string> folderMD5 = new List<string>();
                folderMD5 = GetFoldersMD5(pm5, indexList);
                currentWebFolders.Add(pm5.Path, folderMD5);
            }
            return currentWebFolders;
        }

        private Dictionary<string, List<string>> GetDatasFromHeadTableForRemoveStubByJobId(String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = $"%{jobId}%";
            var selectMd5 = string.Empty;
            selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_JOB_ID like @jobId";
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectMd5, parameters);
            List<GoogleBasicIndex> webPathMD5 = new List<GoogleBasicIndex>();
            Dictionary<string, List<string>> currentWebFolders = new Dictionary<string, List<string>>();
            foreach (var webPath in indexList)
            {
                //if (webPath.Type == "W")
                {
                    webPathMD5.Add(webPath);
                }
            }
            foreach (var pm5 in webPathMD5)
            {
                List<string> folderMD5 = GetFoldersMD5(pm5, indexList);
                foreach (string fMD5 in folderMD5)
                {
                    if (currentWebFolders.ContainsKey(pm5.Path))
                    {
                        if (!currentWebFolders[pm5.Path].Contains(fMD5))
                        {
                            currentWebFolders[pm5.Path].Add(fMD5);
                        }
                    }
                    else
                    {
                        currentWebFolders.Add(pm5.Path, new List<string>() { fMD5 });
                    }
                }
            }
            return currentWebFolders;
        }


        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId, ref String stubType, long modifiedTime = 0, bool isFilterSoftDelete = false)
        {
            List<String> documentsType = new List<String>();
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var selectDocumentsType = "select COL_TYPE from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            documentsType = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectDocumentsType, parameters);
            foreach (var type in documentsType)
            {
                if (!type.ToString().Equals("D", StringComparison.CurrentCultureIgnoreCase))
                {
                    return null;
                }
            }
            var selectStubInfo = "select COL_STUBINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_NAME not like '%:%' limit 1";
            var stubTypeXmlString = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectStubInfo, parameters);
            if (stubTypeXmlString.Any() && !stubTypeXmlString.First().IsNullOrEmpty0())
            {
                var doc = new XmlDocument();
                doc.LoadXml(stubTypeXmlString.First());
                var headerExtraAttribute = doc.GetElementsByTagName("StubInfo");
                if (headerExtraAttribute.Count > 0)
                {
                    stubType = headerExtraAttribute[0].Attributes["StubType"].Value;
                }
            }
            string selectDocumentsUrl = string.Empty;
            if (modifiedTime > 0)
            {
                parameters["@dateTime"] = modifiedTime;
                if (isFilterSoftDelete)
                {
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_NAME not like '%:%' and COL_SOFT_DELETE_TIME<@dateTime and COL_SOFT_DELETE_TIME>0 and COL_RETENTION_STATUS = 1";
                }
                else
                {
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_NAME not like '%:%' and COL_MODIFY_TIME<@dateTime";
                }
            }
            else
            {
                selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_NAME not like '%:%'";
            }
            //documentsUrl = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectDocumentsUrl, parameters);
            Dictionary<string, List<string>> matchWebfiles = new Dictionary<string, List<string>>();
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectDocumentsUrl, parameters);
            var folders = GetDatasFromHeadTableForRemoveStub(storagePolicyId, jobId);
            foreach (var tempF in folders)
            {
                List<string> fileUrls = new List<string>();
                matchWebfiles.Add(tempF.Key, new List<string>());
                foreach (var flist in indexList)
                {
                    if (tempF.Value.Contains(flist.ParentPathMD5))
                    {
                        matchWebfiles[tempF.Key].Add(flist.Path);
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

        public Dictionary<string, List<GoogleBasicIndex>> FilterDocumentsByJobId(String jobId, ref String stubType)
        {
            var parameters = new Dictionary<String, Object>();
            var selectStubInfo = "select COL_STUBINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_JOB_ID like @jobId and COL_NAME not like '%:%' limit 1";
            parameters["@jobId"] = $"%{jobId}%";
            var stubTypeXmlString = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectStubInfo, parameters);

            if (stubTypeXmlString == null)
            {
                logger.Error("stub type xml string is null");
                throw new ArgumentNullException(nameof(stubTypeXmlString));
            }

            if (stubTypeXmlString.Any())
            {
                var xmlString = stubTypeXmlString[0];
                if (!string.IsNullOrEmpty(xmlString))
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

            var selectDocumentsUrl = "select [COL_NAME],[COL_EXTENSION_7],[COL_PATH_MD5],[COL_PARENT_PATH_MD5],[COL_ARCHIVE_TIME] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_JOB_ID like @jobId and COL_NAME not like '%:%'";
            Dictionary<string, List<GoogleBasicIndex>> matchWebfiles = new Dictionary<string, List<GoogleBasicIndex>>();
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectDocumentsUrl, parameters);
            var folders = GetDatasFromHeadTableForRemoveStubByJobId(jobId);
            foreach (var tempF in folders)
            {
                matchWebfiles.Add(tempF.Key, new List<GoogleBasicIndex>());
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

        public Dictionary<string, List<string>> FilterDocumentUrlForLifecycle(List<GoogleBasicIndex> item, String jobId, ref String stubType)
        {
            var indexList = new List<GoogleBasicIndex>();
            Dictionary<string, List<string>> matchWebfiles = new Dictionary<string, List<string>>();
          
            foreach (var tm in item)
            {
                if (!tm.Type.Equals(2) && tm.JobId == jobId)
                {
                    return null;
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
                        matchWebfiles[tempF.Key].Add(flist.Path);
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
                selectSiteUrl = "select [COL_EXTENSION_7] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_TYPE = 'E'";
            }
            else
            {
                selectSiteUrl = "select [COL_EXTENSION_7] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_JOB_ID = @jobId and COL_TYPE = 'E'";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectSiteUrl, parameters);
            if (indexList.Any())
            {
                return indexList.First().Path;
            }
            logger.Warn("siteUrl not exsit in TB_HEAD_INDEX");
            try
            {
                logger.Info("Start get all site records in the index db.");
                parameters.Clear();
                selectSiteUrl = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_TYPE = 'E'";
                indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectSiteUrl, parameters);

                foreach (var record in indexList)
                {
                    logger.Info($"Url: {record.Name} COL_STORAGEPOLICYID: {record.StoragePolicyId} COL_JOB_ID: {record.JobId} ");
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
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
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
                deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_SOFT_DELETE_TIME<@dateTime and COL_SOFT_DELETE_TIME>0 and COL_RETENTION_STATUS = 1";
            }
            else
            {
                deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_MODIFY_TIME<@dateTime";
            }
            this.IndexProcessor.Execute(deleteBodyTable, parameters);

            var sql = "SELECT COUNT(*) FROM " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            long exsitFileCount = Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, parameters));
            if (exsitFileCount <= 0)
            {
                var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
                this.IndexProcessor.Execute(deleteHeadTable, parameters);
            }
        }
        public void UpdateAsSoftDelete(String storagePolicyId, String jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            var deleteBodyTable = "update " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " set COL_RETENTION_STATUS = 1 where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
        public void UpdateAsSoftDeleteByTime(String storagePolicyId, String jobId, long dateTime)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            parameters["@dateTime"] = dateTime;
            parameters["@timeNow"] = DateTime.UtcNow.Ticks.ToString();
            var deleteBodyTable = "update " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " set COL_RETENTION_STATUS = 1,COL_SOFT_DELETE_TIME = @timeNow where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_MODIFY_TIME<@dateTime and COL_RETENTION_STATUS = 0";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
        public List<GoogleBasicIndex> GetDeletingDataFromMainIndex(String storagePolicyId, String jobId)
        {
            int offset = 0;
            int indexLimit = 32775;
            int tempResultCount = 0;
            string sql = $"select * from {IndexConstants.TableNameGDriveItem} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId LIMIT @offset, @length";
            List<GoogleBasicIndex> results = new List<GoogleBasicIndex>();
            do
            {
                var parameters = new Dictionary<String, Object>();
                parameters["@storagePolicyId"] = storagePolicyId;
                parameters["@jobId"] = jobId;
                parameters["@offset"] = offset;
                parameters["@length"] = indexLimit;
                var indexes = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
                tempResultCount = indexes.Count;
                offset += tempResultCount;
                results.AddRange(indexes);

            } while (tempResultCount == indexLimit);

            return results;
        }



        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(string storagePolicyId, string jobId, string driveId, long dateTime = 0)
        {
            List<KeyValuePair<string, long>> result = new();
            var containerMaps = GetDatasFromHeadTableForRetentionInfo(storagePolicyId, jobId);
            foreach (var containerMap in containerMaps)
            {
                var containerId = containerMap.Key;
                List<string> allParents = containerMap.Value;
                var driveFileTotalCount = 0L;
                var queryConditionsCount = 100;
                for (int j = 0; j < allParents.Count; j += queryConditionsCount)
                {
                    logger.Info($"Query by parent id, start at {j}");
                    var tempAllParents = allParents.Skip(j).Take(queryConditionsCount).ToList();
                    long fileCountByPage = GetOneDriveFileCountByPage(tempAllParents);
                    driveFileTotalCount += fileCountByPage;
                }
                KeyValuePair<string, long> item = new(containerId, driveFileTotalCount);
                result.Add(item);
            }
            long GetOneDriveFileCountByPage(List<string> parentMD5s)
            {
                var parameters = new Dictionary<String, Object>
                {
                    ["@storagePolicyId"] = storagePolicyId,
                    ["@jobId"] = jobId,
                    ["@driveId"] = driveId
                };
                string getDeleteAllFiles = string.Empty;
                List<SqlParameter> pathMD5Parameters;
                if (dateTime == 0)
                {
                    getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameGDriveItem} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_DRIVE_ID = @driveId" +
    $" and (COL_PARENT_PATH_MD5 in {DatabaseUtility.BuildInClause(parentMD5s, out pathMD5Parameters)})" +
    $" and COL_TYPE in (20,21) and COL_NAME NOT LIKE '%:%'";
                }
                else
                {
                    parameters["@dateTime"] = dateTime;
                    getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameGDriveItem} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_DRIVE_ID = @driveId and COL_MODIFY_TIME<@dateTime" +
    $" and (COL_PARENT_PATH_MD5 in {DatabaseUtility.BuildInClause(parentMD5s, out pathMD5Parameters)})" +
    $" and COL_TYPE in (20,21) and COL_NAME NOT LIKE '%:%'";
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
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId";
            }
            else
            {
                selectMd5 = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " where COL_JOB_ID = @jobId";
            }
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(selectMd5, parameters);
            List<GoogleBasicIndex> pathMD5Conatainers = new();
            Dictionary<string, List<string>> currentContainers = new();
            foreach (var index in indexList)
            {
                if (index.Type is 1 or 2)
                {
                    pathMD5Conatainers.Add(index);
                }
            }
            foreach (var pathMD5Conatainer in pathMD5Conatainers)
            {
                //List<string> folderMD5 = new List<string>();
                List<string> folderMD5 = GetFoldersMD5(pathMD5Conatainer, indexList);
                folderMD5.Insert(0, pathMD5Conatainer.PathMD5);
                currentContainers.Add(pathMD5Conatainer.ItemId, folderMD5);
            }
            return currentContainers;
        }


        public List<String> GetStorageInfosByJobId(String jobId)
        {
            List<String> storageInfos = new List<String>();
            var sql = "select COL_STORAGEINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                + " where COL_JOB_ID = @jobId"
                + " union"
                + " select COL_STORAGEINFO from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer)
                + " where COL_JOB_ID = @jobId";
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@jobId", jobId);
            var indexes = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
            foreach (GoogleBasicIndex index in indexes)
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
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer)
                   + " where COL_JOB_ID = @jobId";
            var indexes = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
            if (indexes.Count > 0)
            {
                dataMode = indexes[0].Flag;
            }
            return dataMode;
        }

        public List<string> GetUniqueRetentions()
        {
            var parameters = new Dictionary<String, Object>();
            string sql = "select distinct COL_RETENTION from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                  + " where COL_RETENTION is not null and COL_RETENTION != ''";
            var itemsList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return itemsList;
        }


        public List<GoogleBasicIndex> GetRetentionData(string retentionId, long orphanTicks)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@COL_RETENTION"] = retentionId;
            parameters["@COL_ARCHIVE_TIME"] = orphanTicks;
            string sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                  + " where COL_RETENTION = @COL_RETENTION and COL_ARCHIVE_TIME < @COL_ARCHIVE_TIME";
            var itemsList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
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
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_PATH_MD5 in (" + sb.ToString() + ") and COL_JOB_ID = @jobId";
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

                var getDeleteAllFiles = $"select count(*) from {IndexConstants.TableNameGDriveItem} where COL_JOB_ID = @jobId and COL_SITE_PATH = @siteURL" +
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
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " where COL_ITEMID in (" + sb.ToString() + ") and COL_JOB_ID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
        public long GetFileCount()
        {
            var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveItem + " WHERE COL_TYPE = 20";
            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, null));
        }

        public long GetFileVersionCount()
        {
            var sql = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveItem + " WHERE COL_TYPE = 21";
            return Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, null));
        }
        public List<GoogleBasicIndex> GetDeletingIndexesByModifiedTime(String storagePolicyId, String jobId, long dateTime, bool filterSoftDeleteDatas)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@storagePolicyId"] = storagePolicyId;
            parameters["@jobId"] = jobId;
            parameters["@dateTime"] = dateTime;
            string sql = string.Empty;
            if (filterSoftDeleteDatas)
            {
                sql = $"SELECT * FROM {IndexConstants.TableNameGDriveItem} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_SOFT_DELETE_TIME<@dateTime and COL_SOFT_DELETE_TIME>0 and COL_RETENTION_STATUS = 1";
            }
            else
            {
                sql = $"SELECT * FROM {IndexConstants.TableNameGDriveItem} where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOB_ID = @jobId and COL_MODIFY_TIME<@dateTime";
            }
            return this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameters);
        }

        public List<GoogleBasicIndex> GetAllGoogleDatasFromItemTableByType(StringBuilder sql, ArchiverRestoreFilter filter, GDriveBrowseInfo restoreParam, ArchiverRestoreOrderBy orderBy)
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
            if (filter.Level == PolicyLevel.GoogleDriveDocument)
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
            parameters.Add("@Url", restoreParam.DriveName);
            parameters.Add("@TEXT", criteria);

            sql.Append(" and COL_ARCHIVE_TIME <= @ENDTIME");
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
            return this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql.ToString(), parameters);
        }

        public GoogleBasicIndex GetOneDataFromHeadOrBodyTable(String path, Int64 endTime)
        {
            GoogleBasicIndex index = null;
            logger.Info($"Begin loading load item from {path}");
            string pathMD5;
            if (path == null)
            {
                throw new ArgumentNullException(MediaServiceArchiverBackupResource.ArchiverHeadAndBodyIndexServiceLoadArgumentNullException);
            }
            else
            {
                pathMD5 = path;//path.ToMD5HashCode();
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
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameterDictionary);
            if (indexList.Count > 0)
            {
                index = indexList[0];
            }
            return index;
        }
        private String GetTableNameByPath(String pathMD5)
        {
            string sql = "select count(COL_ID) from " + IndexConstants.TableNameGDriveItem + " where COL_PATH_MD5= @COL_PATH_MD5";
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = pathMD5;
            long itemCount = (long)IndexProcessor.ExecuteScalar(sql, parameterDictionary);
            return itemCount > 0 ? IndexConstants.TableNameGDriveItem : IndexConstants.TableNameGDriveContainer;
        }

        public List<GoogleBasicIndex> GetDatasFromBodyTable(ArchiverIndexInfo indexInfo)
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
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            stopwatch.Stop();
            logger.Info($"Get datas from body table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}");
            return indexList;
        }
        public List<GoogleBasicIndex> GetVersionsByItemIdFromBodyTable(int topCount, string ItemId, long endTime)
        {
            logger.Info($"get Datas from body index,{ItemId},topCount:{topCount},end time:{endTime}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem)
                + " where COL_ITEMID = @COL_ITEMID "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME "
                + " and COL_VERSION_NUMBER is not null "
                + " group by COL_PATH_MD5 order by COL_MODIFY_TIME DESC limit @LIMITE";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_ITEMID"] = ItemId;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = endTime;
            parameterDictionary["@LIMITE"] = topCount;
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            stopwatch.Stop();
            return indexList;
        }
        private List<GoogleBasicIndex> SortItems(List<GoogleBasicIndex> items)
        {
            items.Sort((x, y) =>
            {
                int result = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                {
                    if (x.ItemMajorVersion < y.ItemMajorVersion)
                        result = -1;
                    else if (x.ItemMajorVersion > y.ItemMajorVersion)
                        result = 1;
                }
                return result;
            });
            return items;
        }
        public List<GoogleBasicIndex> GetDatasFromHeadTable(ArchiverIndexInfo indexInfo)
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
            var sql = "select MAX(COL_ARCHIVE_TIME),* from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer)
                + " where COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME and COL_FLAG % 2 = @COL_FLAG "
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_PARENT_PATH_MD5"] = indexInfo.Path;
            parameterDictionary["@COL_FLAG"] = 0;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = indexInfo.EndTime;
            parameterDictionary["@COL_OFFSET"] = indexInfo.OffSet;
            parameterDictionary["@COL_LENGTH"] = indexInfo.Length;
            var indexList = this.IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameterDictionary);
            stopwatch.Stop();
            logger.Info($"Get datas from head table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath}");
            return indexList;
        }

        public GoogleBasicIndex? GetParentDataFromHeadTable(GoogleBasicIndex childIndex)
        {
            GoogleBasicIndex index = new GoogleBasicIndex();
            Dictionary<string, object> parameterDictionary = new Dictionary<string, object>();
            parameterDictionary["@COL_PATH_MD5"] = childIndex.ParentPathMD5;
            parameterDictionary["@COL_ARCHIVE_TIME"] = childIndex.ArchiveTime;
            String sql = "select * from " + IndexConstants.TableNameGDriveContainer
                + " where COL_PATH_MD5 = @COL_PATH_MD5 "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_TIME "
                + " order by COL_ARCHIVE_TIME desc";
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, parameterDictionary);
            return indexList.FirstOrDefault();
        }

        public List<GoogleBasicIndex> GetAllBodyIndex()
        {
            String sql = "select COL_PATH, COL_TYPE, COL_CONTENT_LENGTH, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_DRIVE_NAME, COL_VERSION_NUMBER from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveItem) + " order by COL_ARCHIVE_TIME desc";
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, null);
            return indexList;
        }

        public List<GoogleBasicIndex> GetAllHeadIndex()
        {
            String sql = "select COL_PATH, COL_TYPE, COL_CONTENT_LENGTH, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_DRIVE_NAME from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameGDriveContainer) + " order by COL_ARCHIVE_TIME desc";
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, null);
            return indexList;
        }

        public List<GoogleBasicIndex> GetAllBodyIndexOnSpecificTimeRange(GDriveBrowseInfo info)
        {
            String sql = @$"
                select COL_PATH, COL_TYPE, COL_CONTENT_LENGTH, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_DRIVE_NAME, COL_VERSION_NUMBER
                from {IndexConstants.TableNameGDriveItem} 
                where COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime} 
                order by COL_ARCHIVE_TIME desc";
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, null);
            return indexList;
        }

        public List<GoogleBasicIndex> GetAllHeadIndexOnSpecificTimeRange(GDriveBrowseInfo info)
        {
            String sql = @$"
                select COL_PATH, COL_TYPE, COL_CONTENT_LENGTH, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_DRIVE_NAME 
                from {IndexConstants.TableNameGDriveContainer}
                where COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime}
                order by COL_ARCHIVE_TIME desc";
            List<GoogleBasicIndex> indexList = IndexProcessor.ExecuteQuery<GoogleBasicIndex>(sql, null);
            return indexList;
        }
    }
}