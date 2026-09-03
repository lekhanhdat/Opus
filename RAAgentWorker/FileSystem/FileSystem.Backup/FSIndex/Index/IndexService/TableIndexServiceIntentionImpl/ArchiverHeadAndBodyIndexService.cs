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

    using AvePoint.GCommon;
    using AvePoint.Media.Service.DomainModel;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.CommonFilter;
    using System.Diagnostics;
    using System.Data.SqlClient;
    using System;
    using AvePoint.GCommon.Utility;
    using System.Linq;
    using AvePoint.RA.Common.Util;
    using AvePoint.RA.Contract.Services;
    using RAFileSystem.FileSystem.FileSystem.Backup;

    #endregion using directives

    public class ArchiverHeadAndBodyIndexService
        : ArchiverTableIndexServiceBase
        , IArchiverHeadAndBodyIndexService
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

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
            logger.Info($"Begin update load item from {path.LogBase64()}");
            string pathMD5;
            if (path == null)
            {
                throw new ArgumentNullException("ArchiverHeadAndBody IndexService LoadArgument NullException");
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
        public ArchiverBasicIndex GetOneDataFromHeadOrBodyTable(String path, Int64 endTime)
        {
            ArchiverBasicIndex index = null;
            logger.Info($"Begin loading load item from {path.LogBase64()}");
            string pathMD5;
            if (path == null)
            {
                throw new ArgumentNullException("ArchiverHeadAndBody IndexService LoadArgument NullException");
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
            logger.Info($"ArchiverHeadAndBody IndexService LoadItems Start{indexInfo.Path.LogBase64()}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            string realPath = indexInfo.Path;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException("ArchiverHeadAndBody IndexService LoadFolders Argument NullException");
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
            logger.Info($"Get datas from head table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath.LogBase64()}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetDatasFromBodyTable(ArchiverIndexInfo indexInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            logger.Info($"ArchiverHeadAndBody IndexService LoadItems Start:{indexInfo.Path.LogBase64()}");
            string realPath = indexInfo.Path;
            if (indexInfo.Path == null)
            {
                throw new ArgumentNullException("ArchiverHeadAndBody IndexService LoadItems Argument NullException");
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
            logger.Info($"Get datas from body table cost time:{stopwatch.ElapsedMilliseconds},query result count is {indexList.Count},path :{realPath.LogBase64()}");
            return indexList;
        }

        public List<ArchiverBasicIndex> GetVersionsByItemIdFromBodyTable(int topCount, string ItemId, long endTime)
        {
            logger.Info($"get Datas from body index,{ItemId},topCount:{topCount},end time:{endTime}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var sql = "select * from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " where COL_ITEMID = @COL_ITEMID "
                + " and COL_ARCHIVE_TIME <= @COL_ARCHIVE_END_TIME "
                + " and COL_NAME like \'%:%\' "
                + " group by COL_PATH_MD5 order by COL_MODIFY_TIME DESC limit @LIMITE";
            Dictionary<String, Object> parameterDictionary = new Dictionary<String, Object>();
            parameterDictionary["@COL_ITEMID"] = ItemId;
            parameterDictionary["@COL_ARCHIVE_END_TIME"] = endTime;
            parameterDictionary["@LIMITE"] = topCount;
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            SortItems(indexList);
            stopwatch.Stop();
            return indexList;
        }
        public List<ArchiverBasicIndex> GetDatasFromHeadTable2(ArchiverIndexInfo indexInfo)
        {
            logger.Info($"ArchiverHeadAndBody IndexService LoadItems Start,indexInfo.Path:{indexInfo.Path.LogBase64()}");
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
            logger.Info($"ArchiverHeadAndBody IndexService LoadItems Start,indexInfo.Path:{indexInfo.Path.LogBase64()}");
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

        public List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByTypeForJob(string sql, ArchiverBrowseInfo restoreParam)
        {
            return this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
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


        public Dictionary<string, List<string>> FilterDocumentUrlFromMainIndex(String storagePolicyId, String jobId, ref String stubType,long modifiedTime = 0, bool isFilterSoftDelete =false)
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
                if (!string.IsNullOrEmpty(xmlString))
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
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%' and COL_META_TAIL_LENGTH<@dateTime and COL_META_TAIL_LENGTH>0 and COL_RETENTION_STATUS = 1";
                }
                else
                {
                    selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%' and COL_MODIFY_TIME<@dateTime";
                }
            }
            else
            {
                selectDocumentsUrl = "select [COL_EXTENSION_7],[COL_PARENT_PATH_MD5] from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId and COL_NAME not like '%:%'";
            }
            //documentsUrl = this.IndexProcessor.ExecuteQueryForOneColume<String>(selectDocumentsUrl, parameters);
            Dictionary<string, List<string>> matchWebfiles = new Dictionary<string, List<string>>();
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(selectDocumentsUrl, parameters);
            var folders = GetDatasFromHeadTableForRemoveStub(storagePolicyId, jobId);
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
                if(!string.IsNullOrEmpty(xmlString))
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
                    if (!string.IsNullOrEmpty(tm.stubInfo))
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
                    logger.Info($"Url: {record.Name.LogBase64()} COL_STORAGEPOLICYID: {record.StoragePolicyId} COL_JOBID: {record.JobId} ");
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
            //var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
            //this.IndexProcessor.Execute(deleteHeadTable, parameters);
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
            //if (exsitFileCount <= 0)
            //{
            //    var deleteHeadTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " where COL_STORAGEPOLICYID = @storagePolicyId and COL_JOBID = @jobId";
            //    this.IndexProcessor.Execute(deleteHeadTable, parameters);
            //}
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


        private Dictionary<string, List<string>> GetDatasFromHeadTableForRetentionInfo(String storagePolicyId, String jobId)
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
            long nextHeadSequence = -1;
            return bodyIndex;
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
            try
            {
                String sql = $@"Select Sum(COL_EXTENSION_5) from tb_body_index where COL_EXTENSION_7 like @subSiteUrl ";
                if (info != null)
                {
                    sql += $@" and COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime}";
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
            var sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from "
                + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveBody)
                + " order by COL_ARCHIVE_TIME desc LIMIT @PageSize OFFSET @PageOffset";
            var parameters = new Dictionary<string, object>
            {
                {"@PageSize", pageSize},
                {"@PageOffset", pageOffset}
            };
            return IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
        }

        public List<ArchiverBasicIndex> GetAllHeadIndex()
        {
            String sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead) + " order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetHeadIndexPage(int pageSize, int pageOffset)
        {
            var sql = "select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH from "
                + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveHead)
                + " order by COL_ARCHIVE_TIME desc LIMIT @PageSize OFFSET @PageOffset";
            var parameters = new Dictionary<string, object>
            {
                {"@PageSize", pageSize},
                {"@PageOffset", pageOffset}
            };
            return IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
        }

        public List<ArchiverBasicIndex> GetAllSubSites(ArchiverBrowseInfo info)
        {
            String sql = $@"select distinct COL_EXTENSION_7
from tb_head_index 
where COL_TYPE = 'W' 
 ";
            if(info != null)
            {
                sql += $@" and COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime}";
            }
            return IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
        }

        public List<ArchiverBasicIndex> GetAllBodyIndexOnSpecificTimeRange(ArchiverBrowseInfo info)
        {
            String sql = $@"
                select COL_EXTRAINFO, COL_EXTENSION_7, COL_TYPE, COL_EXTENSION_5, COL_CREATE_TIME, COL_MODIFY_TIME, COL_ARCHIVE_TIME, COL_SITE_PATH 
                from {IndexConstants.TableNameArchiveBody} 
                where COL_ARCHIVE_TIME <= {info.EndTime} and COL_ARCHIVE_TIME >= {info.StartTime} 
                order by COL_ARCHIVE_TIME desc";
            List<ArchiverBasicIndex> indexList = IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, null);
            return indexList;
        }

        public List<ArchiverBasicIndex> GetAllHeadIndexOnSpecificTimeRange(ArchiverBrowseInfo info)
        {
            String sql = $@"
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
                throw new Exception("Current path MD5 is not valid.");
            return indexList[0];
        }
        public ArchiverBasicIndex GetBodyIndexByMD5(String pathMd5)
        {
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_PATH_MD5", pathMd5);
            var sql = "select * from " + IndexConstants.TableNameArchiveBody + " where COL_PATH_MD5 = @COL_PATH_MD5 order by COL_ARCHIVE_TIME desc";
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameters);
            if (indexList.Count == 0)
                throw new Exception("Current path MD5 is not valid.");
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
                throw new Exception("Current path MD5 is not valid.");
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

        #region private methods

        private List<ArchiverBasicIndex> SortItems(List<ArchiverBasicIndex> items)
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
            return string.Compare(version, "5.2", StringComparison.OrdinalIgnoreCase) >= 0;
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

        public List<ArchiverBasicIndex> GetAllDatasFromHeadOrBodyTableByType(StringBuilder sql, ArchiverRestoreFilter filter, ArchiverBrowseInfo restoreParam)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, long>> GetDeleteDataFromMainIndex(string storagePolicyId, string jobId, string siteURL, long dateTime = 0)
        {
            List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();
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
                KeyValuePair<string, long> item = new KeyValuePair<string, long>(listURL, libraryFileTotalCount);
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

        public List<KeyValuePair<string, long>> GetDeletedDataFromMainIndexByPathMD5(string jobId, List<string> pathMD5, string siteURL)
        {
            throw new NotImplementedException();
        }

        public void InitIndexProcesser(ArchiverIndexService _indexService)
        {
            this.IndexProcessor = _indexService.IndexProcessor;
        }

        #endregion private methods
    }
}