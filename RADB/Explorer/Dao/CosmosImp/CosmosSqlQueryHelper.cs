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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.MockV2;
using AvePoint.RA.DB.SecurityTrimming;
using Microsoft.Azure.Documents;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class CosmosSqlQueryHelper
    {
        private static string ORDER_BY_LEAFNAME_ASC_CLUASE = " ORDER BY c.leafName ASC";
        public const string SELECT_ALL_CLAUSE_WHERE = "SELECT * FROM Record c WHERE ";
        public const string SELECT_COUNT_CLAUSE_WHERE = "SELECT VALUE COUNT(1) FROM Record c WHERE ";

        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
        //public static SqlQuerySpec BuildSearch(string searchKey, Guid[] exceptIds, int[] nodeTypes, int[] recordStatus, List<SourceFlag> sourceFlags)
        //{
        //    if (string.IsNullOrEmpty(searchKey)) throw new ArgumentNullException("searchKey");
        //    //var nodeTypes = new int[] { (int)NodeLevel.Item , (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
        //    //var recordStatus = new int[] { (int)PhysicalRecordStatus.Open , (int)PhysicalRecordStatus.Closed };

        //    var allExpressionList = new List<string>();
        //    var searchExpressionList = new List<string>();
        //    searchExpressionList.Add("CONTAINS(c.leafName, @nameOrId, true)");
        //    searchExpressionList.Add("CONTAINS(c.recordsId, @nameOrId, true)");
        //    allExpressionList.Add(JoinByOr(searchExpressionList));
        //    allExpressionList.Add("ARRAY_CONTAINS(@nodeTypes, c.nodeType)");
        //    allExpressionList.Add("NOT ARRAY_CONTAINS(@exceptIds, c.id)");
        //    allExpressionList.Add("NOT c.declareAsRecord");
        //    allExpressionList.Add("ARRAY_CONTAINS(@recordStatus, c.recordStatus)");

        //    var sqlParaCollection = new SqlParameterCollection()
        //            {
        //                new SqlParameter("@nameOrId", searchKey),
        //                new SqlParameter("@nodeTypes", nodeTypes),
        //                new SqlParameter("@recordStatus", recordStatus),
        //                new SqlParameter("@exceptIds", exceptIds),
        //            };
        //    CheckSecurity(sourceFlags, allExpressionList, sqlParaCollection);
        //    //var sb = new StringBuilder("SELECT * FROM Record c ")
        //    //                    .Append("WHERE ((CONTAINS(c.leafName, @nameOrId, true) ")
        //    //                    .Append("OR CONTAINS(c.recordsId, @nameOrId, true)) ")
        //    //                    .Append("AND ARRAY_CONTAINS(@nodeTypes, c.nodeType) ")
        //    //                    .Append("AND NOT ARRAY_CONTAINS(@exceptIds, c.id)")
        //    //                    .Append("AND NOT c.declareAsRecord ")
        //    //                    .Append("AND (ARRAY_CONTAINS(@recordStatus, c.recordStatus)) ")
        //    //                    .Append(")")
        //    //                    .Append(" ORDER BY c.collectTime DESC");
        //    var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
        //                        .Append(JoinByAnd(allExpressionList))
        //                        .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);

        //    return new SqlQuerySpec()
        //    {
        //        QueryText = sb.ToString(),
        //        Parameters = sqlParaCollection
        //    };
        //}
        public static SqlQuerySpec BuildSearchForEnduser(string searchKey, Guid[] exceptIds, int[] nodeTypes, int[] recordStatus, List<int> permissionIds)
        {
            //if (string.IsNullOrEmpty(searchKey)) throw new ArgumentNullException("searchKey");
            //var nodeTypes = new int[] { (int)NodeLevel.Item , (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
            //var recordStatus = new int[] { (int)PhysicalRecordStatus.Open , (int)PhysicalRecordStatus.Closed };

            var allExpressionList = new List<string>();
            var searchExpressionList = new List<string>();
            if (!string.IsNullOrEmpty(searchKey))
            {
                searchExpressionList.Add("CONTAINS(c.leafName, @nameOrId, true)");
                searchExpressionList.Add("CONTAINS(c.recordsId, @nameOrId, true)");
            }
            allExpressionList.Add(JoinByOr(searchExpressionList));
            allExpressionList.Add("ARRAY_CONTAINS(@nodeTypes, c.nodeType)");
            allExpressionList.Add("NOT ARRAY_CONTAINS(@exceptIds, c.id)");
            allExpressionList.Add("NOT c.declareAsRecord");
            allExpressionList.Add("ARRAY_CONTAINS(@recordStatus, c.recordStatus)");

            //var sb = new StringBuilder("SELECT * FROM Record c ")
            //                    .Append("WHERE ((CONTAINS(c.leafName, @nameOrId, true) ")
            //                    .Append("OR CONTAINS(c.recordsId, @nameOrId, true)) ")
            //                    .Append("AND ARRAY_CONTAINS(@nodeTypes, c.nodeType) ")
            //                    .Append("AND NOT ARRAY_CONTAINS(@exceptIds, c.id)")
            //                    .Append("AND NOT c.declareAsRecord ")
            //                    .Append("AND (ARRAY_CONTAINS(@recordStatus, c.recordStatus)) ")
            //                    .Append(")")
            //                    .Append(" ORDER BY c.collectTime DESC");

            SqlParameterCollection Parameters;
            if (!string.IsNullOrEmpty(searchKey))
            {
                Parameters = new SqlParameterCollection() 
                    {
                        new SqlParameter("@nameOrId", searchKey),
                        new SqlParameter("@nodeTypes", nodeTypes),
                        new SqlParameter("@recordStatus", recordStatus),
                        new SqlParameter("@exceptIds", exceptIds),
                    };
            }
            else
            {
                Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@nodeTypes", nodeTypes),
                        new SqlParameter("@recordStatus", recordStatus),
                        new SqlParameter("@exceptIds", exceptIds),
                    };
            }
            GeneratePermissionQueryExpression(permissionIds, allExpressionList, Parameters);
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList))
                               .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = Parameters
            };
        }

        private static void BuildSecurityTermCondition(ref List<string> allExpressionList, ref SqlParameterCollection sqlParas, SecurityTermPermissionDto termPermDto)
        {
            if (termPermDto == null)
            {
                return;
            }
            switch (termPermDto.TermPermissionType)
            {
                case TermPermissionMethod.None:
                    //allExpressionList.Add("ARRAY_CONTAINS(@securityTermIds, c.termId, true)");
                    //sqlParas.Add(new SqlParameter("@securityTermIds", new List<Guid>() { Guid.Empty}));
                    allExpressionList.Add("c.termId = @securityTermId");
                    sqlParas.Add(new SqlParameter("@securityTermId", Guid.Empty));
                    break;
                case TermPermissionMethod.All:
                    break;
                case TermPermissionMethod.SpecifyScope:
                    var termIds = termPermDto.TermObjIds;
                    termIds.Add(Guid.Empty);
                    allExpressionList.Add("ARRAY_CONTAINS(@securityTermIds, c.termId, true)");
                    sqlParas.Add(new SqlParameter("@securityTermIds", termIds.ToArray()));
                    break;
                default:
                    break;
            }
        }

        private static void BuildWithoutNodeType(ref List<string> allExpressionList, ref SqlParameterCollection sqlParas)
        { 
            allExpressionList.Add("c.nodeType <> @phyRecordNodeType");
            sqlParas.Add(new SqlParameter("@phyRecordNodeType", RMNodeLevel.PhysicalRecord));
        }

        public static SqlQuerySpec BuildSerch(PhysicalExplorerQueryDto queryDto, SecurityTermPermissionDto termPermDto, bool withoutPhysicalRecord = false)
        {
            if (queryDto == null || string.IsNullOrEmpty(queryDto.NodeId))
            {
                throw new ArgumentNullException("FilterOption.SearchKey");
            }
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection();
            BuildSecurityTermCondition(ref allExpressionList, ref sqlParaCollection, termPermDto);
            if (withoutPhysicalRecord)
            {
                BuildWithoutNodeType(ref allExpressionList, ref sqlParaCollection);
            }
            //if (queryDto != null)
            //{
            //    if (queryDto.NodeId != null)
            //    {
            ////Name && UniqueId Search using contain search filter
            //if (queryDto.FilterOption != null)
            //{
            bool hasFilterCase = false;
            bool hasStatusFilter = false;
            bool orderById = false;
            if (queryDto.FilterOption != null)
            {
                #region Search Key wild match
                if (!string.IsNullOrEmpty(queryDto.FilterOption.SearchKey))
                {
                    hasFilterCase = true;
                    var key = queryDto.FilterOption.SearchKey;
                    if (key.Contains('*'))
                    {
                        //勾了Name 或者全勾了
                        int starCount = key.Count(a => a == '*');
                        //如果选择了Name, 只模糊匹配Name
                        if (starCount == 1)
                        {
                            if (key.StartsWith("*"))
                            {
                                //前边带一个* 
                                string tempSql = "(endswith(c.leafName, @searchName, true) or endswith(c.recordsId, @searchName, true))";
                                allExpressionList.Add(tempSql);
                                sqlParaCollection.Add(new SqlParameter("@searchName", key.TrimStart('*')));
                            }
                            else if (key.EndsWith("*"))
                            {
                                //后边带一个*
                                string tempSql = "(startswith(c.leafName, @searchName, true) or startswith(c.recordsId, @searchName, true))";
                                allExpressionList.Add(tempSql);
                                sqlParaCollection.Add(new SqlParameter("@searchName", key.TrimEnd('*')));
                            }
                            else
                            {
                                //中间带一个*
                                string tempSql = "((startswith(c.leafName, @searchStart, true) and endswith(c.leafName, @searchEnd, true)) or (startswith(c.recordsId, @searchStart, true) and endswith(c.recordsId, @searchEnd, true)))";
                                allExpressionList.Add(tempSql);
                                string[] tempParam = key.Split('*');
                                sqlParaCollection.Add(new SqlParameter("@searchStart", tempParam[0]));
                                sqlParaCollection.Add(new SqlParameter("@searchEnd", tempParam[1]));
                            }
                        }
                        else if (starCount == 2)
                        {
                            if (key.StartsWith("*") && key.EndsWith("*"))
                            {
                                //前后各带一个*
                                string tempSql = "(contains(c.leafName, @searchName, true) or contains(c.recordsId, @searchName, true))";
                                allExpressionList.Add(tempSql);
                                sqlParaCollection.Add(new SqlParameter("@searchName", key.Trim('*')));
                            }
                            else
                            {
                                string tempSql = "RegexMatch(c.leafName, @searchName, 'i')";
                                allExpressionList.Add(tempSql);
                                sqlParaCollection.Add(new SqlParameter("@searchName", RegexUtility.ConvertWildcardPatternToRegex(key)));
                            }
                        }
                        else if (starCount > 2)
                        {
                            string tempSql = "RegexMatch(c.leafName, @searchName, 'i')";
                            allExpressionList.Add(tempSql);
                            sqlParaCollection.Add(new SqlParameter("@searchName", RegexUtility.ConvertWildcardPatternToRegex(key)));
                        }
                    }
                    else
                    {
                        if (IsUniqueIDSearch(queryDto.FilterOption))
                        {
                            allExpressionList.Add("c.recordsId = @searchId");
                            sqlParaCollection.Add(new SqlParameter("@searchId", key));
                            orderById = true;
                        }
                        else if (IsTRIMNumberSearch(queryDto.FilterOption))
                        {
                            allExpressionList.Add("(c.recordsId = @searchId or stringequals(c.leafName, @searchId, true))");
                            sqlParaCollection.Add(new SqlParameter("@searchId", key));
                            //sqlParaCollection.Add(new SqlParameter("@searchName", key.ToLower()));
                        }
                        else
                        {
                            allExpressionList.Add("(stringequals(c.recordsId, @searchId, true) or stringequals(c.leafName, @searchId, true))");
                            sqlParaCollection.Add(new SqlParameter("@searchId", key));
                        }
                    }
                    //    var searchKeyExpressionList = new List<string>();
                    //searchKeyExpressionList.Add("CONTAINS(c.leafName, @searchName, true)");
                    //sqlParaCollection.Add(new SqlParameter("@searchName", key));

                    //searchKeyExpressionList.Add("CONTAINS(c.recordsId, @searchId, true)");
                    //sqlParaCollection.Add(new SqlParameter("@searchId", key));


                    //allExpressionList.Add(JoinByOr(searchKeyExpressionList));
                }

                #endregion
                if (queryDto.FilterOption.Status != (int)RMRecordStatus.None)
                {
                    hasFilterCase = true;
                    // -1 为前后台约定好的值，在传递 -1 的时候，表示搜索所有类型。
                    if (queryDto.FilterOption.Status != (int)RMRecordStatus.All)
                    {
                        hasStatusFilter = true;
                        allExpressionList.Add("c.recordStatus = @filterRecordStatus");
                        sqlParaCollection.Add(new SqlParameter("@filterRecordStatus", queryDto.FilterOption.Status));
                    }
                }

                if (queryDto.FilterOption.NodeType != RMNodeLevel.Undefined)
                {
                    hasFilterCase = true;
                    // -4 为前后台约定好的值，在传递 -4 的时候，表示搜索所有类型。
                    if (queryDto.FilterOption.NodeType != RMNodeLevel.RMSelectAll)
                    {
                        allExpressionList.Add("c.nodeType = @filterRecordNodeType");
                        sqlParaCollection.Add(new SqlParameter("@filterRecordNodeType", queryDto.FilterOption.NodeType));
                    }
                }

                if (queryDto.FilterOption.RecordsOwner != null && queryDto.FilterOption.RecordsOwner.Count > 0)
                {
                    allExpressionList.Add("ARRAY_CONTAINS(@recordOwner, c.recordOwner, true)");
                    sqlParaCollection.Add(new SqlParameter("@recordOwner", queryDto.FilterOption.RecordsOwner.Select(o => $"|{o}|").ToArray()));
                    hasFilterCase = true;
                }

                if (queryDto.FilterOption.CreatedBy != null && queryDto.FilterOption.CreatedBy.Count > 0)
                {
                    allExpressionList.Add("ARRAY_CONTAINS(@createdBy, c.createdBy, true)");
                    sqlParaCollection.Add(new SqlParameter("@createdBy", queryDto.FilterOption.CreatedBy.ToArray()));

                    hasFilterCase = true;
                }

                if (queryDto.FilterOption.ModifiedBy != null && queryDto.FilterOption.ModifiedBy.Count > 0)
                {
                    allExpressionList.Add("ARRAY_CONTAINS(@modifiedBy, c.modifiedBy, true)");
                    sqlParaCollection.Add(new SqlParameter("@modifiedBy", queryDto.FilterOption.ModifiedBy.ToArray()));
                    hasFilterCase = true;
                }

                if (queryDto.FilterOption.TermTreeFilter != null && queryDto.FilterOption.TermTreeFilter != Guid.Empty)
                {
                    if (queryDto.CurrentNodeType != RMNodeLevel.PhysicalFile)
                    {
                        allExpressionList.Add("c.termId = @termId");
                        sqlParaCollection.Add(new SqlParameter("@termId", queryDto.FilterOption.TermTreeFilter));
                    }
                }

            }
            if (!hasStatusFilter)
            {
                var physicalRecordStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Destroyed, (int)RMRecordStatus.Missing };
                allExpressionList.Add("ARRAY_CONTAINS(@physicalRecordStatus, c.recordStatus)");
                sqlParaCollection.Add(new SqlParameter("@physicalRecordStatus", physicalRecordStatus));
            }

            allExpressionList.Add("c.scopeId = @scopeId");
            sqlParaCollection.Add(new SqlParameter("@scopeId", Guid.Empty));
            //Add Filter  Permimssion
            GeneratePermissionQueryExpression(queryDto.PermissionIds, queryDto.HaveCurrentNodePermission, allExpressionList, sqlParaCollection);

            if (!hasFilterCase)
            {
                //hasFilterCase为false只查一层，为true查所有子节点
                var nodeId = new Guid(queryDto.NodeId);
                switch ((int)queryDto.CurrentNodeType)
                {
                    case (int)RMNodeLevel.PhysicalBottomLocation:
                        allExpressionList.Add("ARRAY_CONTAINS(@nodeType, c.nodeType)");
                        sqlParaCollection.Add(new SqlParameter("@nodeType", new List<RMNodeLevel> { RMNodeLevel.PhysicalBox, RMNodeLevel.PhysicalFile }));
                        allExpressionList.Add("c.locationId = @locationId");
                        sqlParaCollection.Add(new SqlParameter("@locationId", nodeId));
                        allExpressionList.Add("c.boxId = @boxId");
                        sqlParaCollection.Add(new SqlParameter("@boxId", Guid.Empty));
                        break;
                    case (int)RMNodeLevel.PhysicalBox:
                        allExpressionList.Add("c.nodeType = @nodeType");
                        sqlParaCollection.Add(new SqlParameter("@nodeType", RMNodeLevel.PhysicalFile));
                        allExpressionList.Add("c.boxId = @boxId");
                        sqlParaCollection.Add(new SqlParameter("@boxId", nodeId));
                        break;
                    case (int)RMNodeLevel.PhysicalFile:
                        allExpressionList.Add("c.nodeType = @nodeType");
                        sqlParaCollection.Add(new SqlParameter("@nodeType", RMNodeLevel.PhysicalRecord));
                        allExpressionList.Add("c.fileId = @fileId");
                        sqlParaCollection.Add(new SqlParameter("@fileId", nodeId));
                        break;
                    case (int)RMNodeLevel.PhysicalRecord:
                    case (int)RMNodeLevel.Undefined:
                    default:
                        break;
                }
            }
            else
            {
                GenerateDeepQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, sqlParaCollection);
            }
            //    else
            //    {
            //        if (queryDto != null)
            //        {
            //            GenerateShallowQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, param);
            //        }
            //    }
            //}
            //else
            //{
            //    if (queryDto != null)
            //    {
            //        GenerateShallowQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, param);
            //    }
            //}
            //    }
            //}

            #region final SqlQuerySpec
            var sb = new StringBuilder(SELECT_ALL_CLAUSE_WHERE)
                                .Append(JoinByAnd(allExpressionList))
                                .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        public static SqlQuerySpec GetRecordsByPermission(List<int> permisionScopeIds, List<Guid> recordsIds)
        {
            var sqlParaCollection = new SqlParameterCollection();
            var allExpressionList = new List<string>();

            GeneratePermissionQueryExpression(permisionScopeIds, allExpressionList, sqlParaCollection);
            List<string> allRecordIds = new List<string>();
            if (recordsIds.Count > 0)
            {
                allRecordIds.Add("ARRAY_CONTAINS(@NodeId, c.nodeId)");
                sqlParaCollection.Add(new SqlParameter("@NodeId", recordsIds));
                allExpressionList.Add(JoinByAnd(allRecordIds));
            }

            #region final SqlQuerySpec
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                                .Append(JoinByAnd(allExpressionList))
                                .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        private static bool IsUniqueIDSearch(PhysicalExplorerFilterOption filterOption)
        {
            bool result = false;
            if (filterOption != null && !string.IsNullOrEmpty(filterOption.SearchKey))
            {
                Regex physicalReg = new Regex("^\\w{1,10}-{1}\\d{2,15}$");  //box-0008 
                result = physicalReg.IsMatch(filterOption.SearchKey);
            }
            return result;
        }

        private static bool IsUniqueIDSearch(ExplorerFilterOption filterOption)
        {
            bool result = false;
            if (filterOption != null && filterOption.SearchOption != null && !string.IsNullOrEmpty(filterOption.SearchOption.Key))
            {
                Regex oldSPUniqueIDReg = new Regex("^\\w{2,12}-{1}\\d{10}$");  //"RECO-0000000008" , old data rec-0000000009
                Regex newDocIDReg = new Regex("^\\w{4,12}-{1}\\d{4,}-{1}\\d+$");  //EDRMS-1291378921-10
                Regex physicalReg = new Regex("^\\w{1,10}-{1}\\d{2,15}$");  //box-0008
                if (!filterOption.SourceFlags.IsNullOrEmpty())
                {
                    if (filterOption.SourceFlags.Contains(SourceFlag.SharePoint) || filterOption.SourceFlags.Contains(SourceFlag.Exchange) || filterOption.SourceFlags.Contains(SourceFlag.FileSystem))
                    {
                        result = oldSPUniqueIDReg.IsMatch(filterOption.SearchOption.Key) || newDocIDReg.IsMatch(filterOption.SearchOption.Key);
                    }
                    if (!result && filterOption.SourceFlags.Contains(SourceFlag.Physical))
                    {
                        result = physicalReg.IsMatch(filterOption.SearchOption.Key);
                    }

                }
                else
                {
                    //Source flags空说明Search来自Electronic Explorer Search
                    result = oldSPUniqueIDReg.IsMatch(filterOption.SearchOption.Key) || newDocIDReg.IsMatch(filterOption.SearchOption.Key);
                }
            }
            return result;
        }
        private static bool IsTRIMNumberSearch(PhysicalExplorerFilterOption filterOption)
        {
            bool result = false;
            if (filterOption != null && !string.IsNullOrEmpty(filterOption.SearchKey))
            {
                //只搜Physical， 不满足Records生成UniqueID的规则， 检查TRIM Import的ID
                string searchKey = filterOption.SearchKey;
                Regex trimReg = new Regex("\\w*-?/?~?[0-9]+");  //Trim的Unique ID非常灵活， 可以是只数字 或者带 - ~ / 连接符，但必须数字结尾
                if (!searchKey.Contains(" ") && trimReg.IsMatch(searchKey))
                {
                    result = true;
                }
            }
            return result;
        }
        private static bool IsTRIMNumberSearch(ExplorerFilterOption filterOption)
        {
            bool result = false;
            if (filterOption != null && filterOption.SearchOption != null && !string.IsNullOrEmpty(filterOption.SearchOption.Key))
            {
                //只搜Physical， 不满足Records生成UniqueID的规则， 检查TRIM Import的ID
                string searchKey = filterOption.SearchOption.Key;
                Regex trimReg = new Regex("\\w*-?/?~?[0-9]+");  //Trim的Unique ID非常灵活， 可以是只数字 或者带 - ~ / 连接符，但必须数字结尾
                if (!searchKey.Contains(" ") && trimReg.IsMatch(searchKey))
                {
                    result = true;
                }
            }
            return result;
        }

        private static string BuildOneStartWildMatchSql(string action, string column, string param, string param1 = null)
        {
            string sql = null;
            if (action == "startswith")
            {
                sql = $"endswith(c.{column}, @{param}, true)";
            }
            else if (action == "endswith")
            {
                sql = $"startswith(c.{column}, @{param}, true)";
            }
            else if (action == "and")
            {
                sql = $"startswith(c.{column}, @{param}, true) and endswith(c.{column}, @{param1}, true)";
            }
            else if (action == "contains")
            {
                sql = $"contains(c.{column}, @{param}, true)";
            }
            return sql;
        }


        /// <summary>
        /// 检查security trimming权限，组装对应的查询条件表达式
        /// </summary>
        /// <param name="sourceFlags"></param>
        /// <param name="allExpressionList"></param>
        /// <param name="sqlParaCollection"></param>
        private static async Task CheckSecurityAsync(List<SourceFlag> sourceFlags, List<string> allExpressionList, SqlParameterCollection sqlParaCollection, bool includeSourceFlagsInQuery = true)
        {
            //if (!RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled()) return;
            //if (sourceFlags.Contains(SourceFlag.SharePoint) || sourceFlags.Contains(SourceFlag.Exchange))
            //{
            var permissionCheckResult = await SecurityTrimmingHelper.CheckAsync(sourceFlags, RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled());
                if (permissionCheckResult.NeedCheck)
                {
                    permissionCheckResult.RemoveSourceFlags(sourceFlags);
                    var containerIds = permissionCheckResult.GetContainerIds();

                    List<string> sourceFlagsExpressionList = new List<string>();
                    sourceFlagsExpressionList.Add("ARRAY_CONTAINS(@containerIds, c.containerId)");
                    sqlParaCollection.Add(new SqlParameter("@containerIds", containerIds.ToArray()));

                    //if there are other source flags
                    var otherSourceFlags = sourceFlags.Except(SourceFlagHelper.GetDefaultContainerIdSource()).ToList();
                    if (otherSourceFlags.Count > 0)
                    {
                        sourceFlagsExpressionList.Add("ARRAY_CONTAINS(@otherSourceFlags, c.sourceFlag)");
                        sqlParaCollection.Add(new SqlParameter("@otherSourceFlags", otherSourceFlags.Select(o => (int)o).ToArray()));
                    }
                    allExpressionList.Add(JoinByOr(sourceFlagsExpressionList));
                }
                else if (includeSourceFlagsInQuery)
            {
                allExpressionList.Add("ARRAY_CONTAINS(@sourceFlags, c.sourceFlag)");
                sqlParaCollection.Add(new SqlParameter("@sourceFlags", sourceFlags.Select(o => (int)o).ToArray()));
            }
            //}
        }


        public static async Task<SqlQuerySpec> BuildSearchAsync(ExplorerQueryDto dto, bool includeSP, bool includeExo, bool includePhy, bool includeFS, SecurityTermPermissionDto termPermDto = null)
        {
            bool isGlobalSearch = includePhy;
            ExplorerFilterOption filterOption = dto.FilterOption;
            if (dto.PermissionIds == null && (filterOption == null))
            {
                throw new ArgumentNullException("SearchOption.Key");
            }

            var sqlParaCollection = new SqlParameterCollection();
            var allExpressionList = new List<string>();

            if (!isGlobalSearch)
            {
                BuildSecurityTermCondition(ref allExpressionList, ref sqlParaCollection, termPermDto);
            }

            #region search condition

            var searchExpressionList = new List<string>();
            SearchType orderBySearchType = SearchType.FileName;
            if (filterOption != null && filterOption.SearchOption != null && !string.IsNullOrEmpty(filterOption.SearchOption.Key)
                && filterOption.SearchOption.Key != "*" && filterOption.SearchOption.Key.Trim('*') != string.Empty)
            {
                string searchValue = filterOption.SearchOption.Key;
                if (searchValue.Contains('*'))
                {
                    bool selectName = (filterOption.SearchOption.SearchType & (int)SearchType.FileName) == (int)SearchType.FileName;
                    string columnName = selectName ? "leafName" : "recordsId";
                    orderBySearchType = selectName ? SearchType.FileName : SearchType.RecordsId;
                    //勾了Name 或者全勾了
                    int starCount = searchValue.Count(a => a == '*');
                    //如果选择了Name, 只模糊匹配Name
                    if (starCount == 1)
                    {
                        if (searchValue.StartsWith("*"))
                        {
                            //前边带一个* 
                            searchExpressionList.Add(BuildOneStartWildMatchSql("startswith", columnName, "searchName"));
                            sqlParaCollection.Add(new SqlParameter("@searchName", searchValue.TrimStart('*')));
                        }
                        else if (searchValue.EndsWith("*"))
                        {
                            //后边带一个*
                            searchExpressionList.Add(BuildOneStartWildMatchSql("endswith", columnName, "searchName"));
                            sqlParaCollection.Add(new SqlParameter("@searchName", searchValue.TrimEnd('*')));
                        }
                        else
                        {
                            //中间带一个*
                            searchExpressionList.Add(BuildOneStartWildMatchSql("and", columnName, "searchStart", "searchEnd"));
                            string[] tempParam = searchValue.Split('*');
                            sqlParaCollection.Add(new SqlParameter("@searchStart", tempParam[0]));
                            sqlParaCollection.Add(new SqlParameter("@searchEnd", tempParam[1]));
                        }
                    }
                    else if (starCount == 2)
                    {
                        if (searchValue.StartsWith("*") && searchValue.EndsWith("*"))
                        {
                            //前后各带一个*
                            searchExpressionList.Add(BuildOneStartWildMatchSql("contains", columnName, "searchName"));
                            sqlParaCollection.Add(new SqlParameter("@searchName", searchValue.Trim('*')));
                        }
                        else
                        {
                            string tempSql = selectName ? $"RegexMatch(c.leafName, @searchName, 'i')" : $"RegexMatch(c.recordsId, @searchName, 'i')";
                            searchExpressionList.Add(tempSql);
                            sqlParaCollection.Add(new SqlParameter("@searchName", RegexUtility.ConvertWildcardPatternToRegex(searchValue)));
                        }
                    }
                    else if (starCount > 2)
                    {
                        string tempSql = selectName ? $"RegexMatch(c.leafName, @searchName, 'i')" : $"RegexMatch(c.recordsId, @searchName, 'i')";
                        searchExpressionList.Add(tempSql);
                        sqlParaCollection.Add(new SqlParameter("@searchName", RegexUtility.ConvertWildcardPatternToRegex(searchValue)));
                    }
                }
                else
                {
                    //不包含 *
                    if ((filterOption.SearchOption.SearchType & (int)SearchType.RecordsId) == (int)SearchType.RecordsId &&
                        (IsUniqueIDSearch(filterOption) || (IsTRIMNumberSearch(filterOption) &&
                    ((filterOption.SourceFlags != null && filterOption.SourceFlags.Count == 1 && filterOption.SourceFlags[0] == SourceFlag.Physical) || filterOption.SourceFlag == SourceFlag.Physical))))
                    {
                        //选中了recordId, 并且符合Id格式
                        string tempSql = "c.recordsId = @searchId";
                        searchExpressionList.Add(tempSql);
                        sqlParaCollection.Add(new SqlParameter("@searchId", searchValue));
                        orderBySearchType = SearchType.RecordsId;
                    }
                    else
                    {
                        string tempSql = null;
                        if (filterOption.SearchOption.SearchType == (int)SearchType.RecordsId)
                        {
                            //tempSql = "lower(c.recordsId) = @searchId";
                            tempSql = "stringequals(c.recordsId, @searchId, true)";
                            orderBySearchType = SearchType.RecordsId;
                        }
                        else if (filterOption.SearchOption.SearchType == (int)SearchType.FileName)
                        {
                            //tempSql = "lower(c.leafName) = @searchId";
                            tempSql = "stringequals(c.leafName, @searchId, true)";
                        }
                        else
                        {
                            tempSql = "(stringequals(c.recordsId, @searchId, true) or stringequals(c.leafName, @searchId, true))";
                        }
                        searchExpressionList.Add(tempSql);
                        sqlParaCollection.Add(new SqlParameter("@searchId", searchValue));
                    }
                }
            }

            if (searchExpressionList.Count > 0)
            {
                allExpressionList.Add(JoinByOr(searchExpressionList));
            }
            #endregion

            #region filters
            #region duedate filter
            ArgumentCheck.NotNull(filterOption, nameof(filterOption));
            if (filterOption.DateInfo != null)
            {
                var timeInfo = filterOption.DateInfo;
                switch (timeInfo.Condition)
                {
                    case DateCondition.Pending:
                        allExpressionList.Add($"c.disposalDueDate = @disposalDueDate");
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDate", DueDateUtil.Pending));
                        break;
                    case DateCondition.NextJob:
                        allExpressionList.Add($"c.disposalDueDate = @disposalDueDate");
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDate", DueDateUtil.NextJob));
                        break;
                    case DateCondition.None:
                        break;
                    case DateCondition.Before:
                        long ticks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        allExpressionList.Add($"c.disposalDueDate > @disposalDueDateFrom");
                        allExpressionList.Add($"c.disposalDueDate <= @disposalDueDateTo");
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDateFrom", DateTime.MinValue.Ticks));
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDateTo", ticks));
                        break;
                    case DateCondition.After:
                        long ticksValue = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        allExpressionList.Add($"c.disposalDueDate > @disposalDueDateFrom");
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDateFrom", ticksValue));
                        break;
                    case DateCondition.FromTo:
                        long startTicks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        long endTicks = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);

                        allExpressionList.Add($"c.disposalDueDate >= @disposalDueDateFrom");
                        allExpressionList.Add($"c.disposalDueDate <= @disposalDueDateTo");
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDateFrom", startTicks));
                        sqlParaCollection.Add(new SqlParameter("@disposalDueDateTo", endTicks));
                        break;
                    default:
                        break;
                }
            }
            #endregion
            if (filterOption.SourceFlags != null)
            {
                await CheckSecurityAsync(filterOption.SourceFlags, allExpressionList, sqlParaCollection);
                includeSP = filterOption.SourceFlags.Contains(SourceFlag.SharePoint);
                includeExo = filterOption.SourceFlags.Contains(SourceFlag.Exchange);
                includePhy = filterOption.SourceFlags.Contains(SourceFlag.Physical);
                includeFS = filterOption.SourceFlags.Contains(SourceFlag.FileSystem);
                if (includePhy && !includeSP && !includeExo && !includeFS)
                {
                    ///只查询Physical数据， 指定scopeId, 不加Source条件
                    searchExpressionList.Add("c.scopeId = @scopeId");
                    sqlParaCollection.Add(new SqlParameter("@scopeId", Guid.Empty));
                }
                else
                {
                    //allExpressionList.Add("ARRAY_CONTAINS(@sourceFlags, c.sourceFlag)");
                    //sqlParaCollection.Add(new SqlParameter("@sourceFlags", filterOption.SourceFlags.Select(o => (int)o).ToArray()));
                    searchExpressionList.Add("c.scopeId != @scopeId");
                    sqlParaCollection.Add(new SqlParameter("@scopeId", Guid.Empty));
                }
            }
            if (filterOption.HoldStatus != null)
            {
                allExpressionList.Add($"c.holdStatus = @holdStatus");
                sqlParaCollection.Add(new SqlParameter("@holdStatus", filterOption.HoldStatus));

            }
            #region terms
            var termExpressionList = new List<string>();
            var nullTermExpressionList = new List<string>();
            if (filterOption.TermIds != null)
            {
                termExpressionList.Add($"ARRAY_CONTAINS(@termIds, c.termId)");
                sqlParaCollection.Add(new SqlParameter("@termIds", filterOption.TermIds.ToArray()));
            }
            if (filterOption.WithOutTerms != null)
            {
                var nullTermNames = new object[] { "", null };
                nullTermExpressionList.Add($"ARRAY_CONTAINS(@termNames, c.termName)");
                sqlParaCollection.Add(new SqlParameter("@termNames", nullTermNames));

                var physicalNodeTypes = new int[] { (int)RMNodeLevel.PhysicalBox, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };
                nullTermExpressionList.Add("NOT ARRAY_CONTAINS(@phyNodeType, c.nodeType)");
                sqlParaCollection.Add(new SqlParameter("@phyNodeType", physicalNodeTypes));
            }

            if (nullTermExpressionList.Count > 0)
            {
                termExpressionList.Add(JoinByAnd(nullTermExpressionList));
            }
            if (termExpressionList.Count > 0)
            {
                allExpressionList.Add(JoinByOr(termExpressionList));
            }
            #endregion
            #region owners
            if (filterOption.Owners != null)
            {
                allExpressionList.Add("ARRAY_CONTAINS(@recordOwner, c.recordOwner, true)");
                sqlParaCollection.Add(new SqlParameter("@recordOwner", filterOption.Owners.Select(o => $"|{o}|").ToArray()));
            }
            #endregion
            #region create by
            if (filterOption.CreatedBy != null && filterOption.CreatedBy.Count > 0)
            {
                var createdByConditions = new List<string>();
                var i = 0;
                foreach (var createdBy in filterOption.CreatedBy)
                {
                    var param = $"@createdBy{i}";
                    createdByConditions.Add($"CONTAINS(c.createdBy, {param}, true)");
                    sqlParaCollection.Add(new SqlParameter($"{param}", createdBy));
                    i++;
                }
                if (createdByConditions.Count > 0)
                {
                    allExpressionList.Add(JoinByOr(createdByConditions));
                }
            }
            #endregion
            #region modified by (only global search)
            if (filterOption.ModifiedBy != null && filterOption.ModifiedBy.Count > 0)
            {
                var modifiedByConditions = new List<string>();
                var i = 0;
                foreach (var modifiedBy in filterOption.ModifiedBy)
                {
                    var param = $"@modifiedBy{i}";
                    modifiedByConditions.Add($"CONTAINS(c.modifiedBy, {param}, true)");
                    sqlParaCollection.Add(new SqlParameter($"{param}", modifiedBy));
                    i++;
                }
                if (modifiedByConditions.Count > 0)
                {
                    allExpressionList.Add(JoinByOr(modifiedByConditions));
                }
            }
            #endregion
            #region modified date(only global search)
            if (filterOption.ModifiedDateInfo != null)
            {
                var timeInfo = filterOption.ModifiedDateInfo;
                switch (timeInfo.Condition)
                {
                    case DateCondition.None:
                        break;
                    case DateCondition.Before:
                        long ticks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        allExpressionList.Add($"c.timeModified > @modifiedDateFrom");
                        allExpressionList.Add($"c.timeModified <= @modifiedDateTo");
                        sqlParaCollection.Add(new SqlParameter("@modifiedDateFrom", DateTime.MinValue.Ticks));
                        sqlParaCollection.Add(new SqlParameter("@modifiedDateTo", ticks));
                        break;
                    case DateCondition.After:
                        long ticksValue = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        allExpressionList.Add($"c.timeModified > @modifiedDateFrom");
                        sqlParaCollection.Add(new SqlParameter("@modifiedDateFrom", ticksValue));
                        break;
                    case DateCondition.FromTo:
                        long startTicks = DateTimeUtil.GetTicks(timeInfo.Value1, timeInfo.TimeZoneId, timeInfo.IsDayLight);
                        long endTicks = DateTimeUtil.GetTicks(timeInfo.Value2, timeInfo.TimeZoneId, timeInfo.IsDayLight);

                        allExpressionList.Add($"c.timeModified >= @modifiedDateFrom");
                        allExpressionList.Add($"c.timeModified <= @modifiedDateTo");
                        sqlParaCollection.Add(new SqlParameter("@modifiedDateFrom", startTicks));
                        sqlParaCollection.Add(new SqlParameter("@modifiedDateTo", endTicks));
                        break;
                    default:
                        break;
                }
            }
            #endregion
            #region rule ids
            if (filterOption.RuleIds != null)
            {
                allExpressionList.Add("ARRAY_CONTAINS(@ruleId, c.ruleId)");
                sqlParaCollection.Add(new SqlParameter("@ruleId", filterOption.RuleIds.ToArray()));
            }
            #endregion
            #region declare record
            if (filterOption.DeclaredRecord != null)
            {
                allExpressionList.Add($"c.declareAsRecord = @declareAsRecord");
                sqlParaCollection.Add(new SqlParameter("@declareAsRecord", filterOption.DeclaredRecord));
            }
            #endregion
            #region file extension or NodeType
            if (filterOption.FileExtensions != null)
            {
                var fileExtensionExpressionList = new List<string>();
                var phyNodeTypes = new List<int>();
                var otherTypes = new List<string>();
                foreach (var item in filterOption.FileExtensions)
                {
                    int.TryParse(item, out int phyType);
                    if (phyType > 0)
                    {
                        phyNodeTypes.Add(phyType);
                    }
                    else
                    {
                        otherTypes.Add(item);
                    }
                }

                if (phyNodeTypes.Count > 0)//query node type
                {
                    fileExtensionExpressionList.Add("ARRAY_CONTAINS(@nodeTypes, c.nodeType)");
                    sqlParaCollection.Add(new SqlParameter("@nodeTypes", phyNodeTypes.ToArray()));
                }

                if (otherTypes.Count > 0)//query file extension
                {
                    fileExtensionExpressionList.Add("ARRAY_CONTAINS(@extensionForFile, c.extensionForFile, true)");
                    sqlParaCollection.Add(new SqlParameter("@extensionForFile", otherTypes.ToArray()));
                }

                if (fileExtensionExpressionList.Count > 0)
                {
                    allExpressionList.Add(JoinByOr(fileExtensionExpressionList));
                }
            }
            #endregion

            if (filterOption.NodeId != null)
            {
                allExpressionList.Add($"c.parentId = @parentId");
                sqlParaCollection.Add(new SqlParameter("@parentId", new Guid(filterOption.NodeId)));
            }
            if (filterOption.SPNodes != null && filterOption.SPNodes.Count != 0)
            {
                var spNodeFilterList = new List<string>();
                var containerIds = filterOption.SPNodes.Where(n => n.Level == (int)NodeLevel.WebApplication).Select(n => n.Id).ToList();
                var siteIds = filterOption.SPNodes.Where(n => n.Level == (int)NodeLevel.SiteCollection).Select(n => n.Id).ToList();
                if (containerIds.Count > 0)
                {
                    spNodeFilterList.Add($"ARRAY_CONTAINS(@containerIds, c.containerId)");
                    sqlParaCollection.Add(new SqlParameter("@containerIds", containerIds.ToArray()));
                }
                if (siteIds.Count > 0)
                {
                    spNodeFilterList.Add($"ARRAY_CONTAINS(@siteIds, c.aveSiteId)");
                    sqlParaCollection.Add(new SqlParameter("@siteIds", siteIds.ToArray()));
                }
                if (spNodeFilterList.Count > 0)
                {
                    allExpressionList.Add(JoinByOr(spNodeFilterList));
                }
            }
            if (filterOption.SourceFlag != SourceFlag.All && filterOption.SourceFlag != SourceFlag.None)
            {
                allExpressionList.Add($"c.sourceFlag = @sourceFlag");
                sqlParaCollection.Add(new SqlParameter("@sourceFlag", filterOption.SourceFlag));
            }
            #endregion

            var electronicStatusAndTypeExpressionList = new List<string>();
            var physicalStatusAndTypeExpressionList = new List<string>();
            var allStatusAndTypeExpressionList = new List<string>();


            var electronicNodeTypeExpressionList = new List<string>();
            if (includeSP)
            {
                electronicNodeTypeExpressionList.Add($"c.nodeType = @nodeTypeSPItem");
                sqlParaCollection.Add(new SqlParameter("@nodeTypeSPItem", (int)NodeLevel.Item));
            }

            if (includeExo)
            {
                electronicNodeTypeExpressionList.Add($"c.nodeType = @nodeTypeEXOItem");
                sqlParaCollection.Add(new SqlParameter("@nodeTypeEXOItem", (int)NodeLevel.ExchangeOnlineItem));
            }

            if (includeFS)
            {
                electronicNodeTypeExpressionList.Add($"c.nodeType = @nodeTypeFSFolder");
                electronicNodeTypeExpressionList.Add($"c.nodeType = @nodeTypeFSFile");
                sqlParaCollection.Add(new SqlParameter("@nodeTypeFSFolder", (int)NodeLevel.FSFolder));
                sqlParaCollection.Add(new SqlParameter("@nodeTypeFSFile", (int)NodeLevel.FSFile));
            }

            if (includeExo || includeSP || includeFS)
            {
                electronicStatusAndTypeExpressionList.Add(JoinByOr(electronicNodeTypeExpressionList));
                electronicStatusAndTypeExpressionList.Add("c.recordStatus = @recordStatus1");
                sqlParaCollection.Add(new SqlParameter("@recordStatus1", (int)RMRecordStatus.Active));
                allStatusAndTypeExpressionList.Add(JoinByAnd(electronicStatusAndTypeExpressionList));
            }

            if (includePhy)
            {
                var physicalStatusExpressionList = new List<string>();
                var physicalRecordStatus = new int[] { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Destroyed, (int)RMRecordStatus.Missing };
                physicalStatusAndTypeExpressionList.Add("ARRAY_CONTAINS(@physicalRecordStatus, c.recordStatus)");
                sqlParaCollection.Add(new SqlParameter("@physicalRecordStatus", physicalRecordStatus));

                var phycicalNodeTypeExpressionList = new List<string>();
                var physicalNodeType = new int[] { (int)RMNodeLevel.PhysicalBox, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };

                physicalStatusAndTypeExpressionList.Add("ARRAY_CONTAINS(@physicalNodeType, c.nodeType)");
                sqlParaCollection.Add(new SqlParameter("@physicalNodeType", physicalNodeType));

                //physicalStatusAndTypeExpressionList.Add($"c.scopeId = @ScopeId");
                //sqlParaCollection.Add(new SqlParameter("@ScopeId", Guid.Empty));


                allStatusAndTypeExpressionList.Add(JoinByAnd(physicalStatusAndTypeExpressionList));

            }

            //需要区分数据源，Phy的查询Type是1，6，2，7
            //SP的查询Type是1,为了防止将SP的Status是2的查询出来，需要将Status和Type结合查询
            allExpressionList.Add(JoinByOr(allStatusAndTypeExpressionList));
            if (includePhy)
            {
                GeneratePermissionQueryExpression(dto.PermissionIds, allExpressionList, sqlParaCollection);
            }
            #region final SqlQuerySpec
            var orderByStatement = "";
            if (dto.IsForGlobalSearchJob)
            {
                orderByStatement = " ORDER BY c.nodeType ASC"; //for gobal search set permission job 
            }
            else 
            {
                string orderby = GetOrderByColumn(dto, isGlobalSearch);
                if(orderby == null)
                {
                    orderByStatement = orderBySearchType == SearchType.RecordsId ? " ORDER BY c.recordsId ASC" : ORDER_BY_LEAFNAME_ASC_CLUASE;
                }
                else
                {
                    orderByStatement = string.Format(" ORDER BY c.{0} {1}", orderby, dto.FilterOption.OrderAsc ? "ASC" : "DESC");
                }
            }
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                                .Append(JoinByAnd(allExpressionList))
                                .Append(orderByStatement);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        /// <summary>
        /// 这里实际是OrderBy Column的白名单
        /// </summary> 
        /// <returns></returns>
        private static string GetOrderByColumn(ExplorerQueryDto queryDto, bool isGlobalSearch)
        {
            if (!isGlobalSearch)
            {
               return null;
            }
            string columnFromGUI = queryDto.FilterOption.OrderColumn;
            switch (columnFromGUI)
            {
                case "leafName": 
                case "recordsId": 
                case "createdBy": 
                case "modifiedBy": 
                case "timeModified":
                    return columnFromGUI;
                default:
                    return null;
            }
        }

        public static SqlQuerySpec BuildSqlForBrowseTree(RMPhysicalExplorerNode node, List<int> permissionIds, bool hasScopePermission, SecurityTermPermissionDto termPermDto)
        {
            var sqlParaCollection = new SqlParameterCollection();
            var allExpressionList = new List<string>();
            var nodeTypeConditions = new List<string>();

            #region Filter ScopeId
            allExpressionList.Add($"c.scopeId = @ScopeId");
            sqlParaCollection.Add(new SqlParameter("@ScopeId", Guid.Empty));
            #endregion

            #region Filter NodeType
            switch (node.NodeType)
            {
                case (int)RMNodeLevel.PhysicalBottomLocation:
                    nodeTypeConditions.Add($"c.nodeType = @BoxNodeType");
                    sqlParaCollection.Add(new SqlParameter("@BoxNodeType", RMNodeLevel.PhysicalBox));
                    if (node.LeafNodeType == 0 || node.LeafNodeType >= (int)RMNodeLevel.PhysicalFile)
                    {
                        nodeTypeConditions.Add($"c.nodeType = @FolderNodeType");
                        sqlParaCollection.Add(new SqlParameter("@FolderNodeType", RMNodeLevel.PhysicalFile));
                    }

                    allExpressionList.Add($"c.boxId = @BoxId");
                    sqlParaCollection.Add(new SqlParameter("@BoxId", Guid.Empty));

                    allExpressionList.Add($"c.locationId = @LocationId");
                    sqlParaCollection.Add(new SqlParameter("@LocationId", new Guid(node.LocationId)));
                    break;
                //case (int)RMNodeLevel.PhysicalCustom:

                case (int)RMNodeLevel.PhysicalBox:
                    nodeTypeConditions.Add($"c.nodeType = @FolderNodeType");
                    sqlParaCollection.Add(new SqlParameter("@FolderNodeType", RMNodeLevel.PhysicalFile));

                    allExpressionList.Add($"c.boxId = @BoxId");
                    sqlParaCollection.Add(new SqlParameter("@BoxId", new Guid(node.Id)));
                    break;
                case (int)RMNodeLevel.PhysicalFile:
                    nodeTypeConditions.Add($"c.nodeType = @RecordNodeType");
                    sqlParaCollection.Add(new SqlParameter("@RecordNodeType", RMNodeLevel.PhysicalRecord));

                    allExpressionList.Add($"c.fileId = @FileId");
                    sqlParaCollection.Add(new SqlParameter("@FileId", new Guid(node.Id)));
                    break;
                default:
                    break;
            }

            var oldNodeTypeConditions = new List<string>();
            if (nodeTypeConditions.Count > 0)
            {
                oldNodeTypeConditions.Add($"NOT IS_DEFINED(c.ancestor_Array)");
                oldNodeTypeConditions.Add(JoinByOr(nodeTypeConditions));
            }

            var newnodeTypeConditions = new List<string>();

            if (oldNodeTypeConditions.Count > 0) newnodeTypeConditions.Add(JoinByAnd(oldNodeTypeConditions));

            //for custom and new format data
            if (node.NodeType == (int)RMNodeLevel.PhysicalBottomLocation && !(node.LeafNodeType == 0 || node.LeafNodeType >= (int)RMNodeLevel.PhysicalFile))
            {
                newnodeTypeConditions.Add($"c.parentId = @RecordParentId and c.nodeType <> @NonPhysicalFile");
                sqlParaCollection.Add(new SqlParameter("@NonPhysicalFile", (int)RMNodeLevel.PhysicalFile)); //filter out folder
            }
            else
            {
                newnodeTypeConditions.Add($"c.parentId = @RecordParentId"); 
            }
            sqlParaCollection.Add(new SqlParameter("@RecordParentId", node.NodeType == (int)RMNodeLevel.PhysicalBottomLocation ? new Guid(node.LocationId): new Guid(node.Id)));
            allExpressionList.Add(JoinByOr(newnodeTypeConditions));

            //if (nodeTypeConditions.Count > 0)
            //{
            //    allExpressionList.Add(JoinByOr(nodeTypeConditions));
            //}
            #endregion

            #region Filter RecordStatus
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            recordStatusConditions.Add($"c.recordStatus <> @MoveStatus");
            sqlParaCollection.Add(new SqlParameter("@MoveStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));
            #endregion

            #region SourceFlag
            allExpressionList.Add($"c.sourceFlag = @SourceFlag");
            sqlParaCollection.Add(new SqlParameter("@SourceFlag", SourceFlag.Physical));
            #endregion

            #region Filter  Permimssion
            GeneratePermissionQueryExpression(permissionIds, hasScopePermission, allExpressionList, sqlParaCollection);
            #endregion

            #region Filter Term Permission
            //remove term permission filter
            //BuildSecurityTermCondition(ref allExpressionList, ref sqlParaCollection, termPermDto);
            #endregion

            #region final SqlQuerySpec
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                                .Append(JoinByAnd(allExpressionList))
                                .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        public static SqlQuerySpec BuildSqlForBrowseTermTree(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission)
        {
            var sqlParaCollection = new SqlParameterCollection();
            var allExpressionList = new List<string>();
            var nodeTypeConditions = new List<string>();

            #region Filter ScopeId
            allExpressionList.Add($"c.scopeId = @ScopeId");
            sqlParaCollection.Add(new SqlParameter("@ScopeId", Guid.Empty));
            #endregion

            #region Filter NodeType
            switch (nodeType)
            {
                case RMNodeType.PhyBox:
                    allExpressionList.Add($"c.nodeType = @BoxNodeType");
                    sqlParaCollection.Add(new SqlParameter("@BoxNodeType", RMNodeLevel.PhysicalBox));
                    break;
                case RMNodeType.PhyFile:
                    allExpressionList.Add($"c.nodeType = @FileNodeType");
                    sqlParaCollection.Add(new SqlParameter("@FileNodeType", RMNodeLevel.PhysicalFile));
                    break;
                default:
                    break;
            }
            #endregion

            #region Filter RecordStatus
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            recordStatusConditions.Add($"c.recordStatus <> @MoveStatus");
            sqlParaCollection.Add(new SqlParameter("@MoveStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));
            #endregion

            #region SourceFlag
            allExpressionList.Add($"c.termId = @TermId");
            sqlParaCollection.Add(new SqlParameter("@TermId", termId));
            #endregion

            #region TermId
            allExpressionList.Add($"c.sourceFlag = @SourceFlag");
            sqlParaCollection.Add(new SqlParameter("@SourceFlag", SourceFlag.Physical));
            #endregion

            #region Filter  Permimssion
            GeneratePermissionQueryExpression(permissionIds, allExpressionList, sqlParaCollection);
            #endregion

            #region final SqlQuerySpec
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                                .Append(JoinByAnd(allExpressionList))
                                .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        public static SqlQuerySpec BuildSqlForBrowseTermTree(RMNodeType nodeType, Guid termId, List<int> permissionIds, bool hasScopePermission, List<Guid> bottomLocationIds)
        {
            var sqlParaCollection = new SqlParameterCollection();
            var allExpressionList = new List<string>();
            var nodeTypeConditions = new List<string>();

            #region Filter ScopeId
            allExpressionList.Add($"c.scopeId = @ScopeId");
            sqlParaCollection.Add(new SqlParameter("@ScopeId", Guid.Empty));
            #endregion

            #region Filter NodeType
            switch (nodeType)
            {
                case RMNodeType.PhyBox:
                    allExpressionList.Add($"c.nodeType = @BoxNodeType");
                    sqlParaCollection.Add(new SqlParameter("@BoxNodeType", RMNodeLevel.PhysicalBox));
                    break;
                case RMNodeType.PhyFile:
                    allExpressionList.Add($"c.nodeType = @FileNodeType");
                    sqlParaCollection.Add(new SqlParameter("@FileNodeType", RMNodeLevel.PhysicalFile));
                    break;
                default:
                    break;
            }
            #endregion

            #region Filter RecordStatus
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            recordStatusConditions.Add($"c.recordStatus <> @MoveStatus");
            sqlParaCollection.Add(new SqlParameter("@MoveStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));
            #endregion

            #region SourceFlag
            allExpressionList.Add($"c.termId = @TermId");
            sqlParaCollection.Add(new SqlParameter("@TermId", termId));
            #endregion

            #region TermId
            allExpressionList.Add($"c.sourceFlag = @SourceFlag");
            sqlParaCollection.Add(new SqlParameter("@SourceFlag", SourceFlag.Physical));
            #endregion

            #region Filter  Permimssion
            GeneratePermissionQueryExpression(permissionIds, allExpressionList, sqlParaCollection);
            #endregion

            #region Bottom LocationIds
            allExpressionList.Add($"ARRAY_CONTAINS(@bottomLocationIds,c.locationId)");
            sqlParaCollection.Add(new SqlParameter("@bottomLocationIds", bottomLocationIds));
            #endregion

            #region final SqlQuerySpec
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                                .Append(JoinByAnd(allExpressionList))
                                .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
            #endregion
        }

        //public static SqlQuerySpec BuildSqlForGetDueRecords(SearchFilterParam param)
        //{
        //    var sqlParaCollection = new SqlParameterCollection();
        //    var allExpressionList = new List<string>();

        //    allExpressionList.Add($"c.ruleId <> @RuleId");
        //    sqlParaCollection.Add(new SqlParameter("@RuleId", Guid.Empty));

        //    if (param.DueDate != default(long))
        //    {
        //        var disposalDueDateConditions = new List<string>();
        //        disposalDueDateConditions.Add($"c.disposalDueDate <= @DisposalDueDate");
        //        sqlParaCollection.Add(new SqlParameter("@DisposalDueDate", param.DueDate));
        //        disposalDueDateConditions.Add($"c.disposalDueDate = @NextJobStatus");
        //        sqlParaCollection.Add(new SqlParameter("@NextJobStatus", -1));
        //        allExpressionList.Add(JoinByOr(disposalDueDateConditions));
        //    }

        //    if (param.DataSource != default(int))
        //    {
        //        allExpressionList.Add($"c.sourceFlag = @SourceFlag");
        //        sqlParaCollection.Add(new SqlParameter("@SourceFlag", param.DataSource));
        //    }

        //    if (!string.IsNullOrWhiteSpace(param.ScopeId))
        //    {
        //        allExpressionList.Add($"c.scopeId = @ScopeId");
        //        sqlParaCollection.Add(new SqlParameter("@ScopeId", param.ScopeId));
        //    }

        //    if (param.SkipHold)
        //    {
        //        allExpressionList.Add($"c.holdStatus = @HoldStatus");
        //        sqlParaCollection.Add(new SqlParameter("@HoldStatus", false));
        //    }

        //    var status = param?.Filter?.RecordStatus;
        //    if (status != null && status.Count > 0)
        //    {
        //        allExpressionList.Add("ARRAY_CONTAINS(@RecordStatus, c.recordStatus)");
        //        sqlParaCollection.Add(new SqlParameter("@RecordStatus", status));
        //    }
        //    else
        //    {
        //        allExpressionList.Add("ARRAY_CONTAINS(@RecordStatus, c.recordStatus)");
        //        sqlParaCollection.Add(new SqlParameter("@RecordStatus", new List<int> { (int)RMRecordStatus.Active }));
        //    }

        //    var nodeTypes = param?.Filter?.NodeTypes;
        //    if (nodeTypes != null && nodeTypes.Count > 0)
        //    {
        //        allExpressionList.Add("ARRAY_CONTAINS(@NodeTypes, c.nodeType)");
        //        sqlParaCollection.Add(new SqlParameter("@NodeTypes", nodeTypes));
        //    }

        //    var searchScope = param?.Filter?.SearchScope;
        //    if (!string.IsNullOrWhiteSpace(searchScope) && param.DataSource != default(int))
        //    {
        //        switch (param.DataSource)
        //        {
        //            case (int)SourceFlag.FileSystem:
        //                allExpressionList.Add("c.dirPath = @dirPath");
        //                sqlParaCollection.Add(new SqlParameter("@dirPath", searchScope));
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    #region final SqlQuerySpec
        //    var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
        //                        .Append(JoinByAnd(allExpressionList))
        //                        .Append(ORDER_BY_LEAFNAME_ASC_CLUASE);
        //    return new SqlQuerySpec()
        //    {
        //        QueryText = sb.ToString(),
        //        Parameters = sqlParaCollection
        //    };
        //    #endregion
        //}

        #region private method

        /// <summary>
        /// 此方法只提供browser 下一层数据的Expression， 对于深层search 不work。
        /// </summary>
        /// <param name="currentNodeLevel">当前节点的NodeLevel是什么，用来指定拼装Express 的级别</param>
        /// <param name="nodeId">当前节点的Id</param>
        /// <param name="allExpressionList">外围实例化一个Expression 集合，用来添加每个级别的特殊条件，最终按照and 关系拼接。PS： 此处可以重构，用另外的方法去维护。</param>
        /// <param name="param"></param>
        //private void GenerateShallowQueryExpression(int currentNodeLevel, Guid nodeId, List<string> allExpressionList, SqlParameterCollection sqlParaCollection)
        //{
        //    var nodeTypeExpressionList = new List<string>();
        //    switch (currentNodeLevel)
        //    {
        //        case (int)RMNodeLevel.PhysicalBottomLocation:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalBox));
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", nodeId));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", Guid.Empty));
        //            break;
        //        case (int)RMNodeLevel.PhysicalBox:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalFile:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalRecord));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "FileId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalRecord:
        //        case (int)RMNodeLevel.Undefined:
        //        default:
        //            break;
        //    }
        //    allExpressionList.Add(nodeTypeExpressionList.Aggregate(Expression.OrElse));
        //}

        /// <summary>
        /// 此方法提供browser 深层数据的Expression
        /// </summary>
        /// <param name="currentNodeLevel">当前节点的NodeLevel是什么，用来指定拼装Express 的级别</param>
        /// <param name="nodeId">当前节点的Id</param>
        /// <param name="allExpressionList">外围实例化一个Expression 集合，用来添加每个级别的特殊条件，最终按照and 关系拼接。PS： 此处可以重构，用另外的方法去维护。</param>
        /// <param name="param"></param>
        private static void GenerateDeepQueryExpression(int currentNodeLevel, Guid nodeId, List<string> allExpressionList, SqlParameterCollection sqlParaCollection)
        {
            switch (currentNodeLevel)
            {
                case (int)RMNodeLevel.PhysicalBottomLocation:
                    allExpressionList.Add("c.locationId = @locationId");
                    sqlParaCollection.Add(new SqlParameter("@locationId", nodeId));
                    break;
                case (int)RMNodeLevel.PhysicalBox:
                    allExpressionList.Add("c.boxId = @boxId");
                    sqlParaCollection.Add(new SqlParameter("@boxId", nodeId));
                    break;
                case (int)RMNodeLevel.PhysicalFile:
                    allExpressionList.Add("c.fileId = @fileId");
                    sqlParaCollection.Add(new SqlParameter("@fileId", nodeId));
                    break;
                case (int)RMNodeLevel.PhysicalRecord:
                case (int)RMNodeLevel.Undefined:
                default:
                    break;
            }
        }
        /// <summary>
        /// For physical search.
        /// </summary>
        /// <param name="permissionIds"></param>
        /// <param name="allExpressionList"></param>
        /// <param name="sqlParaCollection"></param>
        private static void GeneratePermissionQueryExpression(List<int> permissionIds, List<string> allExpressionList, SqlParameterCollection sqlParaCollection)
        {
            #region Filter  Permimssion
            var permissionConditions = new List<string>();
            if (permissionIds != null)
            {

                //user对当前节点有权限
                var allPermissionConditions = new List<string>();
                //for (int i = 0; i < permissionIds.Count; i++)
                //{
                //    var paraName = $"@ScopePermissionId{i}";
                //    allPermissionConditions.Add($"c.scopePermissionId = {paraName}");
                //    sqlParaCollection.Add(new SqlParameter($"{paraName}", permissionIds[i]));
                //}

                if (permissionIds.Count > 0)
                {
                    allPermissionConditions.Add("ARRAY_CONTAINS(@ScopePermissionId, c.scopePermissionId)");
                    sqlParaCollection.Add(new SqlParameter("@ScopePermissionId", permissionIds));
                }

                //需要load没有scopePermissionId属性的老数据
                allPermissionConditions.Add($"NOT IS_DEFINED(c.scopePermissionId)");
                //没有设置全新的数据，默认值是0
                allPermissionConditions.Add($"c.scopePermissionId = 0");
                permissionConditions.Add(JoinByOr(allPermissionConditions));
                allExpressionList.Add(JoinByAnd(permissionConditions));

            }
            #endregion
        }
        public static SqlQuerySpec GenerateContianerIdQueyExpression(Guid scopeId, string containerId)
        {
            var containerConditions = new List<string>();
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection();

            sqlParaCollection.Add(new SqlParameter("@ScopeId", scopeId));
            sqlParaCollection.Add(new SqlParameter("@ContainerId", containerId));
            allExpressionList.Add($"c.scopeId = @ScopeId");

            containerConditions.Add($"NOT IS_DEFINED(c.containerId)");
            containerConditions.Add($"c.containerId != @ContainerId");
            allExpressionList.Add(JoinByOr(containerConditions));

            allExpressionList.Add("NOT IS_NULL(c.dirPath)");

            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList));
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
        }

        public static SqlQuerySpec GenerateQueryByContainerAndNodeTypeExpression(Guid scopeId, string containerId, List<int> nodeTypes, string url)
        {
            var conditions = new List<string>();
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection
            {
                new SqlParameter("@ScopeId", scopeId),
                new SqlParameter("@ContainerId", containerId),
                new SqlParameter("@NodeTypes", nodeTypes.ToArray()),
                new SqlParameter("@url", url),
            };

            allExpressionList.Add($"c.scopeId = @ScopeId");
            allExpressionList.Add($"ARRAY_CONTAINS(@NodeTypes, c.nodeType)");

            conditions.Add($"NOT IS_DEFINED(c.containerId)");
            conditions.Add($"c.containerId = @ContainerId");
            allExpressionList.Add(JoinByOr(conditions));

            allExpressionList.Add("NOT IS_NULL(c.dirPath)");
            allExpressionList.Add($"RIGHT(@url,LENGTH(c.dirPath)) = c.dirPath");
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList))
                               .Append(" order by c.nodeType");
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
        }

        public static SqlQuerySpec GenerateSearchFileSystemBySearchKey(string searchKey)
        {
            var condition = new List<string>();
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection
            {
                new SqlParameter("@searchKey", searchKey),
            };
            condition.Add($"CONTAINS(LOWER(c.leafName), LOWER(@searchKey))");
            allExpressionList.Add(JoinByOr(condition));
            allExpressionList.Add($"c.nodeType = 2100"); 
            allExpressionList.Add($"c.sourceFlag = 2"); // 2 is file system source flag
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList))
                               .Append(" order by c.timeModified desc");
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
        }

        public static SqlQuerySpec GenerateSearchFileSystemBySearchKeyAndConnectionIds(string searchKey, IEnumerable<Guid> connectionIds)
        {
            var condition = new List<string>();
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection
            {
                new SqlParameter("@searchKey", searchKey),
            };
            condition.Add($"CONTAINS(LOWER(c.leafName), LOWER(@searchKey))");
            allExpressionList.Add(JoinByOr(condition));
            allExpressionList.Add($"c.nodeType = 2100");
            allExpressionList.Add($"c.sourceFlag = 2"); // 2 is file system source flag
            allExpressionList.Add($"ARRAY_CONTAINS(@AveSiteId, c.aveSiteId)");
            sqlParaCollection.Add(new SqlParameter("@AveSiteId", connectionIds.ToArray()));
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));
            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList))
                               .Append(" order by c.timeModified desc");
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
        }

        public static SqlQuerySpec GenerateSearchByFullPaths(
            List<string> fullPaths,
            int? nodeType = null,
            int? sourceFlag = null)
            {
            var conditions = new List<string>();
            var sqlParams = new SqlParameterCollection();

            if (fullPaths != null && fullPaths.Any())
            {
                var pathConditions = new List<string>();

                for (int i = 0; i < fullPaths.Count; i++)
                {
                    var path = fullPaths[i];
                    var lastIndex = path.LastIndexOf("\\");

                    if (lastIndex <= 0) continue;

                    var dirPath = path.Substring(0, lastIndex);
                    var leafName = path.Substring(lastIndex + 1);

                    var dirParam = $"@dirPath{i}";
                    var leafParam = $"@leafName{i}";

                    sqlParams.Add(new SqlParameter(dirParam, dirPath));
                    sqlParams.Add(new SqlParameter(leafParam, leafName));

                    pathConditions.Add($"(c.dirPath = {dirParam} AND c.leafName = {leafParam})");
                }

                if (pathConditions.Any())
                {
                    conditions.Add(JoinByOr(pathConditions));
                }
            }

            AddConditionIfHasValue(conditions, sqlParams, "c.nodeType", "@nodeType", nodeType);
            AddConditionIfHasValue(conditions, sqlParams, "c.sourceFlag", "@sourceFlag", sourceFlag);
            conditions.Add($"c.recordStatus <> @DelStatus");
            sqlParams.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            var sb = new StringBuilder("SELECT * FROM Record c");
            if (conditions.Any())
            {
                sb.Append(" WHERE ").Append(JoinByAnd(conditions));
            }
            sb.Append(" ORDER BY c.timeModified DESC");
            return new SqlQuerySpec
            {
                QueryText = sb.ToString(),
                Parameters = sqlParams
            };
        }

        private static void AddConditionIfHasValue(
            List<string> conditions,
            SqlParameterCollection sqlParams,
            string field,
            string paramName,
            int? value)
        {
            if (value.HasValue)
            {
                conditions.Add($"{field} = {paramName}");
                sqlParams.Add(new SqlParameter(paramName, value.Value));
            }
        }


        public static SqlQuerySpec GenerateSearchPhysicalBoxOrFolderBySearchKey(string searchKey, bool isGlobalSearch, bool isSearchFolder, string locationId)
        {
            var condition = new List<string>();
            var allExpressionList = new List<string>();
            var sqlParaCollection = new SqlParameterCollection
            {
                new SqlParameter("@searchKey", searchKey),
            };
            condition.Add($"CONTAINS(LOWER(c.leafName), LOWER(@searchKey))");
            condition.Add($"CONTAINS(LOWER(c.recordsId), LOWER(@searchKey))");
            allExpressionList.Add(JoinByOr(condition));
            allExpressionList.Add($"c.sourceFlag = 4"); // 4 is physical record source flag
            if (!isGlobalSearch && isSearchFolder)
            {
                allExpressionList.Add($"c.nodeType IN (9300, 9400)"); // 9300 is Physical box node type, 9400 is physical file
                allExpressionList.Add("c.locationId = @locationId");
                sqlParaCollection.Add(new SqlParameter("@locationId", locationId));
            }
            else if (isSearchFolder && isGlobalSearch)
            {
                allExpressionList.Add($"c.nodeType IN (9300, 9400)"); 
            }
            else
            {
                allExpressionList.Add($"c.nodeType = 9300");
            }
            var recordStatusConditions = new List<string>();
            recordStatusConditions.Add($"c.recordStatus <> @DelStatus");
            sqlParaCollection.Add(new SqlParameter("@DelStatus", (int)RMRecordStatus.RMDeleted));
            recordStatusConditions.Add($"c.recordStatus <> @MoveStatus");
            sqlParaCollection.Add(new SqlParameter("@MoveStatus", (int)RMRecordStatus.MoveOverwrite));
            allExpressionList.Add(JoinByAnd(recordStatusConditions));


            var sb = new StringBuilder("SELECT * FROM Record c WHERE ")
                               .Append(JoinByAnd(allExpressionList))
                               .Append(" order by c.timeModified desc");
            return new SqlQuerySpec()
            {
                QueryText = sb.ToString(),
                Parameters = sqlParaCollection
            };
        }

        private static void GeneratePermissionQueryExpression(List<int> permissionIds, bool hasCurrentNodePermission, List<string> allExpressionList, SqlParameterCollection sqlParaCollection)
        {
            #region Filter  Permimssion
            var permissionConditions = new List<string>();
            if (permissionIds != null)
            {
                var isQueryAllPermissionData = true;
                if (!hasCurrentNodePermission)
                {
                    //user对当前节点没有权限
                    if (permissionIds.Count == 0)
                    {
                        //子节点没有有权限的数据时，-1代表不返回数据
                        permissionIds.Add(-1);
                        isQueryAllPermissionData = false;
                    }
                    permissionConditions.Add("ARRAY_CONTAINS(@ScopePermissionId, c.scopePermissionId)");
                    sqlParaCollection.Add(new SqlParameter("@ScopePermissionId", permissionIds));
                }
                else
                {
                    //user对当前节点有权限
                    var permissionAndConditions = new List<string>();
                    for (int i = 0; i < permissionIds.Count; i++)
                    {
                        var paraName = $"@ScopePermissionId{i}";
                        permissionAndConditions.Add($"c.scopePermissionId <> {paraName}");
                        sqlParaCollection.Add(new SqlParameter($"{paraName}", permissionIds[i]));
                    }
                    if (permissionAndConditions.Count > 0)
                    {
                        permissionConditions.Add(JoinByAnd(permissionAndConditions));
                    }
                }

                if (permissionConditions.Count > 0)
                {
                    if (isQueryAllPermissionData)
                    {
                        //需要load没有scopePermissionId属性的老数据
                        permissionConditions.Add($"NOT IS_DEFINED(c.scopePermissionId)");
                        //没有设置全新的数据，默认值是0
                        permissionConditions.Add($"c.scopePermissionId = 0");
                    }
                    allExpressionList.Add(JoinByOr(permissionConditions));
                }
            }
            #endregion
        }

        private static string JoinByAnd(IList<string> ls)
        {
            return $"({string.Join(" AND ", ls)})";
        }

        private static string JoinByOr(IList<string> ls)
        {
            return $"({string.Join(" OR ", ls)})";
        }

        #endregion
    }
        /// <summary>
        /// Add for explorer wild search
        /// </summary>
        public class RegexUtility
        {
            /// <summary>
            /// get regular expression string according to the wildcard pattern, support wildcard * and ?, using \ do escape
            /// </summary>
            /// <param name="wildcardPattern">wildcard pattern string</param>
            /// <param name="matchType">string match type</param>
            /// <returns></returns>
            public static string ConvertWildcardPatternToRegex(string wildcardPattern)
            {
                string regexExpression = string.Empty;
                string[] split = wildcardPattern.Split(new string[] { @"\*" }, StringSplitOptions.None);
                for (int i = 0; i < split.Length; i++)
                {
                    if (i == 0)
                    {
                        regexExpression = ConvertWildcardPatternToRegexWithoutEscape(split[i]);
                    }
                    else
                    {
                        regexExpression += @"\*" + ConvertWildcardPatternToRegexWithoutEscape(split[i]);
                    }
                }
                //return regexExpression;
                return String.Format("^{0}$", regexExpression);
            }



            private static string ConvertWildcardPatternToRegexWithoutEscape(string wildcardPattern)
            {
                Regex regex = new Regex("[.$^{\\[(|)*+?\\\\]");
                return regex.Replace(wildcardPattern,
                     delegate (Match m)
                     {
                         switch (m.Value)
                         {
                             //case "?":
                             //    return ".";  //暂时不支持？
                             case "*":
                                 return ".*";
                             default:
                                 return "\\" + m.Value;
                         }
                     });
            }
        }
}
