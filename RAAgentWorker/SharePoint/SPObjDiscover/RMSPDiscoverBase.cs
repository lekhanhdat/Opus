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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.SPObjDiscover
{
    public class RMSPDiscoverBase
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AveDiscoverSite DiscoverSite { private set; get; }
        protected SPTreeNodeDto TreeNode { private set; get; }

        //protected JobContext JobContext { private set; get; }
        public int MaxItemsPerThrottledOperation;

        public RMSPDiscoverBase(AveDiscoverSite aveDiscoverSite, SPTreeNodeDto treeNode)
        {
           // WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
            TreeNode = treeNode;
            //JobContext = jobContext;
            DiscoverSite = aveDiscoverSite;
            MaxItemsPerThrottledOperation = GetMaxItemsPerThrottledOperation(DiscoverSite.Site);
        }

        public RMSPDiscoverBase() { }

        public virtual void Init()
        {
        }

        public virtual void RunNow()
        {

        }

        protected bool IsSystemList(AveDiscoverList list)
        {
            bool result = false;
            if (list.Hidden.HasValue && list.Hidden.Value)
            {
                logger.Info("skip hidden list : {0}", string.IsNullOrEmpty(list?.RootFolderUrl.LogBase64()) ? list?.Name.LogBase64() : list?.RootFolderUrl.LogBase64());
                result = true;
            }
            return result;
        }

        protected bool CheckIsDesignList(AveDiscoverList list)
        {
            var listInfo = list.RootFolderUrl.Substring(list.RootFolderUrl.LastIndexOf('/') + 1) + list.ServerTemplate;
            bool isDesignList = false;
            try
            {
                if (SPDicoverCache.Instance.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Check is DesignList error {0}", ex.ToString());
            }
            return isDesignList;
        }

        public int GetMaxItemsPerThrottledOperation(IAveSite aveSite)
        {
            int maxItemsPer = 2000; //5000;  //SPO默认值为5000 并且不能修改， 某些Library 5000分页查询依然会超出Throttle， 限制到2000   from CI
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                    logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");
                    if(maxItemsPer > 2000)
                    {
                        logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                        maxItemsPer = 2000;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }
        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        public string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result.LogBase64()}");
            return result;
        }

        public string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result.LogBase64()}");
            return result;
        }

        public int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            //query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        public int GetLastItemId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        public string GetQueryXml(int startIdx, int endIdx, int rowLimit)
        {
              string  queryXml = $@"
                <View Scope='RecursiveAll'>
                    <Query>
                        <Where>
                            <And>
                                <Gt><FieldRef Name='ID'/><Value Type='Integer'>{startIdx}</Value></Gt>
                                <Leq><FieldRef Name='ID'/><Value Type='Integer'>{endIdx}</Value></Leq>
                            </And>
                        </Where>
                    </Query>
                    <RowLimit>{rowLimit}</RowLimit>
                </View>";
            logger.Info($"ApplyExisting query xml: {queryXml.LogBase64()}");
            return queryXml;
        }

        public AveCamlQuery GetQuery(IAveFolder folder, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            //query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            query.ViewXml = GetQueryXml(startIndex, endIndex, rowLimit);
            return query;
        }

        public AveCamlQuery GetRowIdDiscoverQuery(IAveList list, IAveFolder folder, List<int> rowIds)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                //query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                var group = new QueryGroup();
              
                foreach (var rowId in rowIds)
                {
                    group.Conditions.Add(new QueryCondition(
                             Types.JoinTypes.Or,
                             Types.FieldRefTypes.Name,
                              "ID",
                            Types.FieldTypes.Number,
                            Types.QueryTypes.Eq,
                             rowId.ToString(), false));
                }
                cm.QueryGroup.AddGroup(group);
                //AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                string queryXml = cm.GetFullCAML(false);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                logger.Info($"Process Folder {folder.ServerRelativeUrl.LogBase64()}, row id count: {rowIds.Count}");
                //logger.Info("Query XML:{0}", query.ViewXml);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
            }
            return query;
        }

        public IEnumerable<IAveListItem> GetItemsByRowIds(IAveList list, List<int> rowIds)
        {
            IEnumerable<IAveListItem> items = null;
            using (var performance00 = new AgentPerformanceScope("RMSPDiscoverBase.GetItemsByRowIdTotal", addToStatistics: true))
            {
                //using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    for (int j = 0; j < rowIds.Count; j += 100)
                    {
                        //经测试，每次查询120个rowid时性能较好
                        var tempRowIds = rowIds.Skip(j).Take(100).ToList();
                        AveCamlQuery query = GetRowIdDiscoverQuery(list, list.RootFolder, tempRowIds);
                        using (var performance = new AgentPerformanceScope("RMSPDiscoverBase.GetItemByRowId", addToStatistics: true))
                        {
                            var tempItems = list.GetItemsForRecords(query);
                            if (tempItems != null)
                            {
                                if (items == null)
                                {
                                    items = tempItems;
                                }
                                else
                                {
                                    items = items.Concat(tempItems);
                                }
                            }
                        }
                    }
                }
            }
            return items;
        }

        public AveCamlQuery GetSearchDiscoverQuery(IAveList list, IAveFolder folder, DateTime startTime, DateTime endTime, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                //query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                var group = new QueryGroup();

                group.Conditions.Add(new QueryCondition(
                Types.JoinTypes.And,
                Types.FieldRefTypes.Name,
                SPBuiltInFieldName.ModifiedTime,
                Types.FieldTypes.DateTime,
                Types.QueryTypes.FromTo,
                CreateISO8601DateTimeFromSystemDateTime(startTime),
                CreateISO8601DateTimeFromSystemDateTime(endTime),
                            true));
                cm.QueryGroup.AddGroup(group);
                AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                string queryXml = cm.GetFullCAML(true);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                logger.Info($"Process Folder {folder.ServerRelativeUrl.LogBase64()}, startTime:{startTime}, endTime:{endTime} query xml {queryXml.LogBase64()}");
                logger.Info("Query XML:{0}", query.ViewXml.LogBase64());
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
            }
            return query;
        }
        protected void AddRowLimitQueryCondition(CAMLManager cm, QueryGroup group, int startIndex, int endIndex, int QueryConditionMaxCount)
        {
            //cm.ScopeType = Types.ScopeTypes.Default;
            cm.RowLimit = QueryConditionMaxCount;
            group.Conditions.Add(new QueryCondition(
                              Types.JoinTypes.And,
                              Types.FieldRefTypes.Name,
                               "ID",
                             Types.FieldTypes.Number,
                             Types.QueryTypes.Leq,
                              endIndex.ToString(), false));
            group.Conditions.Add(new QueryCondition(
                                 Types.JoinTypes.And,
                                 Types.FieldRefTypes.Name,
                                 "ID",
                                 Types.FieldTypes.Number,
                                  Types.QueryTypes.Gt,
                                 startIndex.ToString(), false));
        }

        private string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dtValue.Year.ToString("0000"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Month.ToString("00"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Day.ToString("00"));
            stringBuilder.Append("T");
            stringBuilder.Append(dtValue.Hour.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Minute.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Second.ToString("00"));
            stringBuilder.Append("Z");
            return stringBuilder.ToString();
        }

        public int GetListViewThresholdNumber(IAveList list)
        {
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            if (rowLimit > 2000)
            {
                logger.Info("Threshold number is over 2000, limit it to 2000");
                return 2000;
            }
            return rowLimit;
        }
    }
}
