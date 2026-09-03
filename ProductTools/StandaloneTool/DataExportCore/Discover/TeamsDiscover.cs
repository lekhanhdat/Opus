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
using AngleSharp.Common;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.EDiscovery;
using DataExportCore.Cache;
using DataExportCore.Discover.Base;
using DataExportCore.Discover.Node;
using DataExportCore.Utils;
using Merged18NResources.MediaServiceExchangeBackUp;

namespace DataExportCore.Discover
{
    public class TeamsDiscover : TeamsDiscoveryBase<TeamsDiscoveryNode>
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(TeamsDiscover));

        private ArchiverSiteMasterIndexContract? _restoreJobInfo;
        private ArchiverSiteMasterIndexContract RestoreJobInfo
        {
            get
            {
                if(_restoreJobInfo == null)
                {
                    _restoreJobInfo = GetCommonSiteMasterIndexByGroupAddress().FirstOrDefault();
                    if (_restoreJobInfo == null) throw new AveException("The Archiver data has already been deleted by the specified Archiver Retention rules.");
                }
                return _restoreJobInfo;
            }
        }

        private const char PathParser = (char)0x12;

        public TeamsDiscover(string groupAddress, IndexDatabaseHelper dbHelper, IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor, string siteUrl)
            : base(groupAddress, dbHelper, IndexProcessor, siteUrl)
        {
        }

        public void Process()
        {
            try
            {
                logger.Info($"Starting the process of discovering Teams {GroupAddress}");

                LoadAllDataEncryptionInfo();

                var teamsContainer = LoadTeamsContainer();
                ExportQueue.Enqueue(teamsContainer);

                Task.Run(() => ProcessContainer(teamsContainer)).Wait();
                ExportQueue.Finish();
                logger.Info("Finished processing the teams export queue.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occur while discover teams {GroupAddress}. Ex: {e}");
            }
        }

        private void ProcessContainer(ContainerDiscoveryNode teamsContainer)
        {
            try
            {
                var subContainers = GetSubContainers(teamsContainer.Index);
                ChannelDiscoveryNode channelDiscoveryNode = null;
                foreach (var container in subContainers)
                {
                    channelDiscoveryNode = new ChannelDiscoveryNode(container);
                    ProcessChannelNode(channelDiscoveryNode);
                }
            }
            catch (Exception e)
            {
                logger.Error($"[{teamsContainer.Level}][{teamsContainer.PathMD5}] An error occurred while processing mail box node: {teamsContainer.Name}. Error: {e}");
            }
        }

        private void ProcessChannelNode(ChannelDiscoveryNode channelDiscoveryNode)
        {
            var parentId = (channelDiscoveryNode.Index as GroupContainerIndex)?.NodeId;
            var itemCount = GetItemsCount(channelDiscoveryNode.Index, parentId);
            if(itemCount > 0)
            {
                Dictionary<string, string> monthTime = GetMonthStartAndEndTime(channelDiscoveryNode.Index, parentId);
                foreach (var timeInfo in monthTime.Values)
                {
                    string[] times = timeInfo.Split('-');
                    List<string>? topicIds = GetTopicIds(channelDiscoveryNode.Index, long.Parse(times[0]), long.Parse(times[1]), parentId);
                    foreach(var topicId in topicIds)
                    {
                        TopicDiscoveryNode topicDiscoveryNode = new TopicDiscoveryNode(new GroupBasicIndex());
                        int offset = 0;
                        int range = 1000;
                        var hasTopicIdHistory = IsTopicIdHistoryExist(channelDiscoveryNode.Index, long.Parse(times[0]), topicId, parentId);
                        if (hasTopicIdHistory)
                        {
                            logger.Info("This topic id: {0} is exist in previous month and skip to handle.", topicId);
                            continue;
                        }
                        var leftCount = GetOneConversationItemsCount(channelDiscoveryNode.Index, topicId, parentId);
                        int length = 0;
                        bool isTopic = true;
                        while (leftCount > 0)
                        {
                            ConversationDiscoverNode conversationDiscoverNode;
                            length = (int)(leftCount > 1000 ? range : leftCount);
                            List<GroupBasicIndex> allItems = LoadConversationItems(offset, length, topicId, long.Parse(times[0]), long.Parse(times[1]), channelDiscoveryNode.Index, parentId);
                            logger.Info("Start to load next {0} items.", length);
                            foreach (GroupBasicIndex index in allItems)
                            {
                                var exchangeId = index.Name.Substring(index.Name.LastIndexOf(PathParser) + 1);
                                if (isTopic) GlobalCache.TopicItemIds.Add(exchangeId);
                                if (!GlobalCache.ItemCreateTimeInfo.ContainsKey(index.Name))
                                    GlobalCache.ItemCreateTimeInfo.Add(index.Name, index.CreateTime);
                                else
                                    logger.Error("Skip the same item to restore, the item name is  {0}.", index.Name);
                                conversationDiscoverNode = new ConversationDiscoverNode(index);
                                conversationDiscoverNode.SitePath = channelDiscoveryNode.Path;
                                topicDiscoveryNode.AddConversation(conversationDiscoverNode);
                            }
                            offset += length;
                            leftCount -= length;
                        }
                        channelDiscoveryNode.AddTopic(topicDiscoveryNode);
                    }
                    ExportQueue.Enqueue(channelDiscoveryNode);
                }
            }
        }

        private List<GroupBasicIndex> LoadConversationItems(int offset, int length, string topicId, long monthStartTime, long monthEndTime, GroupBasicIndex index, string? parentId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            parameters["@COL_OFFSET"] = offset; 
            parameters["@COL_LENGTH"] = length;
            parameters["@SORT_ID"] = topicId;
            parameters["@MONTH_START_TIME"] = monthStartTime;
            parameters["@MONTH_END_TIME"] = monthEndTime;
            var attachedString = string.Empty;
            var sql = "select * from " + IndexConstants.TableNameExchangeItem
                + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index, parentId, parameters)
                + " and COL_SORT_ID = @SORT_ID "
                + " and COL_EXTENSION_2 >= @MONTH_START_TIME "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 HAVING MAX(COL_BACKUP_TIME) "
                + " order by COL_EXTENSION_2 asc "
                + " Limit @COL_OFFSET, @COL_LENGTH";
            logger.Info("The parameters: COL_PARENT_PATH_MD5:{0}; END_TIME:{1}; COL_OFFSET:{2}; COL_LENGTH:{3}; COL_PARENT_NODE_ID: {4}.", parameters.TryGet("@COL_PARENT_PATH_MD5"), RestoreJobInfo.ArchiverTime, offset, length, parameters.TryGet("@PARENT_ID"));
            var indexList = this.IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);

            var tempList = indexList.FindAll(index => index.BackupType == 2);
            foreach (var temp in tempList)
            {
                indexList.RemoveAll(index => index.PathMD5 == temp.PathMD5);
            }
            return indexList;
        }

        private Int32 GetOneConversationItemsCount(GroupBasicIndex index, string topicId, string? parentId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@SORT_ID"] = topicId;
            var sql = " select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
                 + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index, parentId, parameters)
                 + " and COL_SORT_ID = @SORT_ID "
                 + " group by COL_PATH_MD5 ";
            var pathList = this.IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        private bool IsTopicIdHistoryExist(GroupBasicIndex index, long monthStartTime, string topicId, string? parentId)
        {
            var sortIds = new HashSet<string>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            parameters["@MONTH_START_TIME"] = monthStartTime;
            parameters["@COL_SORT_ID"] = topicId;
            var sql = " select COUNT(*) from " + IndexConstants.TableNameExchangeItem
                    + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index, parentId, parameters)
                    + " and COL_SORT_ID = @COL_SORT_ID "
                    + " and COL_BACKUP_TIME <= @END_TIME "
                    + " and COL_EXTENSION_2 < @MONTH_START_TIME ";
            var count = Convert.ToInt64(this.IndexProcessor.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        private List<string>? GetTopicIds(GroupBasicIndex index, long monthStartTime, long monthEndTime, string? parentId)
        {
            var sortIds = new HashSet<string>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            parameters["@MONTH_START_TIME"] = monthStartTime;
            parameters["@MONTH_END_TIME"] = monthEndTime;
            var sql = " select COL_SORT_ID from " + IndexConstants.TableNameExchangeItem
                    + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index, parentId, parameters)
                    + " and COL_BACKUP_TIME <= @END_TIME "
                    + " and COL_EXTENSION_2 >= @MONTH_START_TIME "
                    + " and COL_EXTENSION_2 <= @MONTH_END_TIME "
                    + " order by COL_EXTENSION_2 asc ";
            var indexList = this.IndexProcessor.ExecuteQuery<GroupItemIndex>(sql, parameters);
            indexList.ForEach(i => sortIds.Add(i.SortId));
            return sortIds.ToList();
        }

        private Dictionary<string, string> GetMonthStartAndEndTime(GroupBasicIndex index, string? parentId)
        {
            var monthTime = new Dictionary<string, string>();
            try
            {
                List<long> itemCreatedTimes = GetItemCreatedTime(index, parentId);
                var firstItemTime = new DateTime(itemCreatedTimes[0]);
                var lastItemTime = new DateTime(itemCreatedTimes[itemCreatedTimes.Count - 1]);
                monthTime = GetMonthTime(firstItemTime, lastItemTime);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get channel conversation created time. Reason: {0}. ", ex);
            }
            return monthTime;
        }

        private List<long> GetItemCreatedTime(GroupBasicIndex index, string? parentId)
        {
            var result = new List<long>();
            var parameters = new Dictionary<String, Object>();
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            var attachedString = string.Empty;
            var sql = "select distinct COL_EXTENSION_2 from " + IndexConstants.TableNameExchangeItem
               + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index, parentId, parameters)
               + " and COL_BACKUP_TYPE != 2 "
               + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
               + " order by COL_EXTENSION_2 asc ";
            var indexList = IndexProcessor.ExecuteQuery<GroupItemIndex>(sql, parameters);
            result = indexList.Select(itemArg => itemArg.CreateTime).ToList();
            return result;
        }

        private static Dictionary<string, string> GetMonthTime(DateTime firstItemTime, DateTime lastItemTime)
        {
            var monthTime = new Dictionary<string, string>();
            if (firstItemTime.Year == lastItemTime.Year)
            {
                for (int month = firstItemTime.Month; month <= lastItemTime.Month; month++)
                {
                    DateTime monthStartTime = month != firstItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", firstItemTime.Year, month, 1)) : firstItemTime;
                    DateTime monthEndTime = month != lastItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", firstItemTime.Year, month + 1, 1)) : lastItemTime;
                    monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                }
            }
            else if (firstItemTime.Year < lastItemTime.Year)
            {
                for (int year = firstItemTime.Year; year <= lastItemTime.Year; year++)
                {
                    if (year == firstItemTime.Year)
                    {
                        for (int month = firstItemTime.Month; month <= 12; month++)
                        {
                            DateTime monthStartTime = month != firstItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1)) : firstItemTime;
                            DateTime monthEndTime = month == 12 ? DateTime.Parse(string.Format("{0}/{1}/{2}", year + 1, 1, 1)) : DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1));
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                    else if (year < lastItemTime.Year)
                    {
                        for (int month = 1; month <= 12; month++)
                        {
                            DateTime monthStartTime = DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1));
                            DateTime monthEndTime = month == 12 ? DateTime.Parse(string.Format("{0}/{1}/{2}", year + 1, 1, 1)) : DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1));
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                    else
                    {
                        for (int month = 1; month <= lastItemTime.Month; month++)
                        {
                            DateTime monthStartTime = DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1));
                            DateTime monthEndTime = month != lastItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1)) : lastItemTime;
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                }
            }
            return monthTime;
        }

        private Int64 GetItemsCount(GroupBasicIndex index, string? parentId)
        {
            var parameters = new Dictionary<String, Object>();
            var sql = " select COL_PATH_MD5 from " + IndexConstants.TableNameExchangeItem
                 + " where COL_PARENT_NODE_ID = " + GenerateParentNodeIdSelectQuery(index ,parentId, parameters)
                 + " group by COL_PATH_MD5 ";
            var pathList = IndexProcessor.ExecuteQueryForOneColume<String>(sql, parameters);
            return pathList.Count;
        }

        private string GenerateParentNodeIdSelectQuery(GroupBasicIndex index, string? parentId, Dictionary<string, object> parameters)
        {
            string result = string.Empty;
            if (parentId != null)
            {
                result = "@PARENT_ID";
                parameters.TryAdd("@PARENT_ID", parentId);
            }
            else
            {
                result = "(select distinct COL_NODE_ID from tb_container_index where COL_PATH_MD5 = @COL_PARENT_PATH_MD5 order by rowid desc limit 1)";
                parameters.TryAdd("@COL_PARENT_PATH_MD5", index.Path.ToMD5HashCode());
            }
            return result;
        }

        private List<GroupBasicIndex> GetSubContainers(GroupBasicIndex parentIndex)
        {
            var indexList = new List<GroupBasicIndex>();
            var result = new List<GroupBasicIndex>();
            var parentPathMd5 = parentIndex.Path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PARENT_PATH_MD5"] = parentPathMd5;
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            parameters["@COL_OFFSET"] = 0;
            parameters["@COL_LENGTH"] = Int32.MaxValue - 1;
            var attachedString = string.Empty;
            var sql = "select MAX(COL_BACKUP_TIME),* from " + IndexConstants.TableNameExchangeContainer
                + " where COL_PARENT_PATH_MD5 = @PARENT_PATH_MD5 "
                + " and COL_BACKUP_TIME <= @END_TIME " + attachedString
                + " group by COL_PATH_MD5 order by rowid asc Limit @COL_OFFSET, @COL_LENGTH";
            indexList.AddRange(this.IndexProcessor.ExecuteQuery<GroupContainerIndex>(sql, parameters));

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

        private ContainerDiscoveryNode LoadTeamsContainer()
        {
            var index = LoadOneItem(true, GroupAddress);
            return new ContainerDiscoveryNode(index);
        }

        private GroupBasicIndex LoadOneItem(bool isContainer, string path)
        {
            var result = new GroupBasicIndex();
            var pathMD5 = path.ToMD5HashCode();
            var parameters = new Dictionary<String, Object>();
            parameters["@PATH_MD5"] = pathMD5;
            parameters["@END_TIME"] = RestoreJobInfo.ArchiverTime;
            var attachedString = string.Empty;
            var sql = "select * from " + IndexConstants.TableNameExchangeContainer + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " order by COL_BACKUP_TIME desc";
            if (!isContainer)
                sql = "select * from " + IndexConstants.TableNameExchangeItem + " where COL_PATH_MD5 = @PATH_MD5 and COL_BACKUP_TIME <= @END_TIME" + attachedString + " group by COL_PATH_MD5 HAVING MAX(COL_BACKUP_TIME)";
            var infoList = IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            if (infoList == null || infoList.Count == 0)
            {
                infoList = IndexProcessor.ExecuteQuery<GroupBasicIndex>(sql, parameters);
            }
            if (infoList != null && infoList.Count > 0)
               result = infoList[0];
            return result;
        }
    }
}
