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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public static class ExplorerQueryOptionV3Extension
    {
        /// <summary>
        /// 如果最后一个search column的ExplorerSearchKeyOperationLogic为OR，那么修改为AND
        /// </summary>
        /// <param name="searchOption"></param>
        public static void ChangeLastOperationLogic(this ExplorerQueryOptionV3 searchOption)
        {
            if (searchOption == null || searchOption.Values.Count == 0) return;
            var last = searchOption.Values.ElementAt(searchOption.Values.Count - 1);
            if (last.ColumnsLogic == ExplorerSearchKeyOperationLogic.OR)
            {
                last.ColumnsLogic = ExplorerSearchKeyOperationLogic.AND;
            }
        }
        /// <summary>
        /// 以Or为分隔符，把相关的查询参数分组， 比如A and B or C and D or E， 会分为3组， A and B， C and D， E，
        ///最终的实现逻辑是(A and B) or(C and D) or(E)的形式
        /// </summary>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<ExplorerQueryOptionV3> Split(this ExplorerQueryOptionV3 searchOption)
        {
            var result = new List<ExplorerQueryOptionV3>();
            ExplorerQueryOptionV3 group = null;
            bool needNewGroup = true;
            foreach (var value in searchOption.Values)
            {
                if (needNewGroup)
                {
                    group = new ExplorerQueryOptionV3() { Values = new List<ExplorerSearchOptionV3>() };
                    result.Add(group);
                    needNewGroup = false;
                }

                group.Values.Add(value);

                if (value.ColumnsLogic == ExplorerSearchKeyOperationLogic.OR)
                {
                    needNewGroup = true;
                }
                
            }
            return result;

        }

        /// <summary>
        /// 检查是否能够转化为basic search， 要查询的columns只有符合下面条件才可以转换：
        ///1）有且只能有Name和Unique ID的查询; 
        ///2）Logic只能是Contains; 
        ///3）Column之间只能是OR的关系; 
        ///4）Search的key要相同; 
        /// </summary>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static bool CanConvertBasicSearchCriteria(this ExplorerQueryOptionV3 searchOption)
        {
            try
            {
                if (searchOption.Values == null || searchOption.Values.Count == 0) return true;

                var columnIds = new List<string> { DefaultColumnIDs.UniqueId, DefaultColumnIDs.NameOrTitle};
                var allColumnIds = searchOption.Values.Select(o => o.Column.Id.ToLower()).Distinct();
                if (!(allColumnIds.Contains(DefaultColumnIDs.UniqueId) && allColumnIds.Contains(DefaultColumnIDs.NameOrTitle))) return false;   //should only have both UniqueId and Name

                var firstKeyValue = searchOption.Values.First().Value.Trim();
                var count = searchOption.Values.Count;
                for (var i = 0; i < count; i++)
                {
                    var v = searchOption.Values[i];
                    if (v.ColumnOperationLogic == ExplorerSearchColumnOperationLogic.Equals
                        //|| !columnIds.Contains(v.Column.Id) //non Name or UniqueId columns
                        //|| v.Column.Type.HasValue        //custom columns
                        || (v.ColumnsLogic == ExplorerSearchKeyOperationLogic.AND && i != (count -1))  //no need to check logic for the last item
                        || !string.Equals(v.Value.Trim(), firstKeyValue, StringComparison.OrdinalIgnoreCase))  //different keys)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断GUI在执行Advanced search后的data，能否做相应的Action,规则如下：
        ///1) Column之间只能是AND关系, 并且只能包含一种SourceFlag
        ///2) 或者如果条件中有OR，那么按照OR分组为多个group, 对于每个Group，都需要包含同一个SourceFlag
        /// </summary>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static bool CanDoGlobalAction(this ExplorerQueryOptionV3 searchOption)
        {
            try
            {
                if (searchOption.Values == null || searchOption.Values.Count == 0) return false;
                searchOption.ChangeLastOperationLogic();
                var searchOptionGroups = searchOption.Split();
                IEnumerable<SourceFlag> sourceFlags = new List<SourceFlag>();
                foreach(var searchOptionGroup in searchOptionGroups)
                {
                    var tmpSourceFlags = GetDataSources(searchOptionGroup.Values);
                    if (tmpSourceFlags.Count != 1) return false;
                    sourceFlags = sourceFlags.Union(tmpSourceFlags);
                }

                return sourceFlags.Count() == 1;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanDoPhysicalBulkUpdate(this ExplorerQueryOptionV3 searchOption)
        {
            try
            {
                if (searchOption.Values == null || searchOption.Values.Count == 0) return false;
                searchOption.ChangeLastOperationLogic();
                var searchOptionGroups = searchOption.Split();
                IEnumerable<string> templates = new List<string>();
                foreach (var searchOptionGroup in searchOptionGroups)
                {
                    var tempTemplates = GetTemplates(searchOptionGroup.Values);
                    if (tempTemplates.Count != 1) return false;
                    templates = templates.Union(tempTemplates);
                }

                return templates.Count() == 1;
            }
            catch
            {
                return false;
            }
        }

        public static bool HasDelayedLoan(this ExplorerQueryOptionV3 queryOption)
        {
            return queryOption.Values.Any(o => QueryCloumnIds.Loan == o.Column.Id);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchOption"></param>
        /// <param name="recordIds"></param>
        public static void AssembleRecordsId(this ExplorerQueryOptionV3 searchOption, List<Guid> recordIds)
        {
            searchOption.Values.Add(new ExplorerSearchOptionV3
            {
                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.LoanDate },
                Value = JsonConvert.SerializeObject(recordIds)
            });

        }

        private static List<SourceFlag> GetDataSources(List<ExplorerSearchOptionV3> group)
        {
            IEnumerable<SourceFlag> result = new List<SourceFlag> ();
            var temps = group.Where(v => string.Equals(v.Column.Id, QueryCloumnIds.SourceFlag, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach(var temp in temps)
            {
                var o = JsonConvert.DeserializeObject<List<SourceFlag>>(temp.Value);
                result = result.Union(o);
            }

            return result.ToList();
        }

        private static List<string> GetTemplates(List<ExplorerSearchOptionV3> group)
        {
            IEnumerable<string> result = new List<string>();
            var temps = group.Where(v => string.Equals(v.Column.Id, QueryCloumnIds.TemplateId, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var temp in temps)
            {
                var o = JsonConvert.DeserializeObject<List<string>>(temp.Value);
                result = result.Union(o);
            }

            return result.ToList();
        }
    }
}
