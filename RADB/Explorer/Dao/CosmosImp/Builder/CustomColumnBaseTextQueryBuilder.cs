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
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharpNLP;
using Newtonsoft.Json;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public abstract class CustomColumnBaseTextQueryBuilder : ICustomColumnSearchBuilder, IAdvancedQueryBuilder
    {
        public Query Search(Query query, ExplorerQueryColumn column, string key, ExplorerSearchKeyOperationLogic operationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            return Search(query, column, key, ExplorerSearchColumnOperationLogic.Contains, operationLogic);
            //if (!CanSearch(column)) return query;

            //if(column.IdsWithDuplicateName != null && column.IdsWithDuplicateName.Count > 1)
            //{
            //    foreach(Guid id in column.IdsWithDuplicateName)
            //    {
            //        var columnName = column.GetCustomColumnName_ValueArray(id);
            //        SearchByKey(query, key, columnName, operationLogic);
            //    }
            //}
            //else
            //{
            //    var columnName = column.GetCustomColumnName_ValueArray();
            //    SearchByKey(query, key, columnName, operationLogic);
            //}


            //return query;

        }

        private Query Search(Query query, ExplorerQueryColumn column, string key, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic operationLogic)
        {
            if (!CanSearch(column)) return query;

            if (column.IdsWithDuplicateName != null && column.IdsWithDuplicateName.Count > 1)
            {
                return query.Where(q =>
                {
                    foreach (Guid id in column.IdsWithDuplicateName)
                    {
                        q.OrWhere(q1 =>
                        {
                            if(columnOperationLogic == ExplorerSearchColumnOperationLogic.Equals)
                            {
                                return BuildEquals(q1, key, column.GetCustomColumnName_Value(id));
                            }
                            else
                            {
                                if (key.Contains('*'))
                                {
                                    return BuildWildcard(q1, key, column.GetCustomColumnName_Value(id), operationLogic); 
                                }
                                return BuildContains(q1, key, column.GetCustomColumnName_ValueArray(id), operationLogic);
                            }
                            //return columnOperationLogic == ExplorerSearchColumnOperationLogic.Equals ? BuildEquals(q1, key, column.GetCustomColumnName_Value(id))
                            //    : BuildContains(q1, key, column.GetCustomColumnName_ValueArray(id), operationLogic);
                        }
                        );
                        
                    }
                    return q;
                });
                
            }
            else
            {
                if (columnOperationLogic == ExplorerSearchColumnOperationLogic.Equals)
                {
                    return BuildEquals(query, key, column.GetCustomColumnName_Value());
                }
                else
                {
                    if (key.Contains('*'))
                    {
                        return BuildWildcard(query, key, column.GetCustomColumnName_Value(), operationLogic);
                    }
                    return BuildContains(query, key, column.GetCustomColumnName_ValueArray(), operationLogic);
                }
                //return columnOperationLogic == ExplorerSearchColumnOperationLogic.Equals ? BuildEquals(query, key, column.GetCustomColumnName_Value())
                //    : BuildContains(query, key, column.GetCustomColumnName_ValueArray(), operationLogic);
            }

        }

        private Query BuildEquals(Query query, string key, string columnName)
        {
            if (string.IsNullOrEmpty(key)) return query;
            return query.WhereStringEquals(columnName, key.Trim());
        }

        private Query BuildContains(Query query, string key, string columnName, ExplorerSearchKeyOperationLogic operationLogic)
        {
            var splitedKeys = ConvertKey2QueryObject(key, ExplorerSearchColumnOperationLogic.Contains);
            Dictionary<string, List<string>> stringTermsDic = RASharpNLPUtility.AnalyzeStringTerms(splitedKeys.ToArray());
            if (splitedKeys.Count() > 1)
            {
                if (operationLogic == ExplorerSearchKeyOperationLogic.AND)
                {
                    query.Where(a => {
                        foreach (string subKey in splitedKeys)
                        {
                            a.BuildExists(columnName, $"subCustomColumnValueArray", subKey, stringTermsDic);
                            //a.Where(b =>
                            //{
                            //    b.WhereArrayContain(columnName, subKey);
                            //    return b;
                            //});
                        }
                        return a;
                    });
                }
                else
                {
                    query.Where(a => a.OrWhereExists(q =>
                    {
                        q.FromParent($"subCustomColumnValueArray", columnName)
                        .WhereArrayContainV2(splitedKeys, string.Empty);
                        return q;
                    }));
                }
            }
            else if (splitedKeys.Count() == 1)
            {
                //query.Where(subqu => subqu.OrWhereArrayContain(columnName, splitedKeys[0]));
                query.Where(subqu =>
                {
                    return subqu.OrWhere(a =>
                    {
                        return a.BuildExists(columnName, $"subCustomColumnValueArray", splitedKeys[0], stringTermsDic);
                    });
                });
            }
            else // use the original search key if no valid keys after spliting
            {
                query.Where(a => a.OrWhereArrayContain(columnName, key));
            }

            return query;
        }

        private Query BuildWildcard(Query query, string searchKey, string columnName, ExplorerSearchKeyOperationLogic operationLogic)
        {
            int starCount = searchKey.Count(a => a == '*');
            if (starCount == 1)
            {
                if (searchKey.IndexOf('*') == 0)
                {
                    //EndsWith
                    query.WhereEnds(columnName, searchKey.TrimStart('*'));
                }
                else if (searchKey.IndexOf('*') == searchKey.Length - 1)
                {
                    //StartsWith
                    query.WhereStarts(columnName, searchKey.TrimEnd('*'));
                }
                else
                {
                    //Startswith && EndsWith
                    string[] temp = searchKey.Split('*');
                    query.WhereStarts(columnName, temp[0]);
                    query.WhereEnds(columnName, temp[1]);
                }
            }
            else if (starCount == 2 && searchKey.StartsWith("*") && searchKey.EndsWith("*"))
            {
                //Contains
                query.WhereContains(columnName, searchKey.Trim('*'));
            }
            else
            {
                //Regex
                string formattedKey = RegexUtility.ConvertWildcardPatternToRegex(searchKey);
                query.WhereRegex(columnName, formattedKey);
            }
            return query;
        }
        /*private Query BuildWildcardArray(Query query, string searchKey, string columnArrayName, ExplorerSearchKeyOperationLogic operationLogic)
        {
            string columnName = $"{columnArrayName}[0]";
            int starCount = searchKey.Count(a => a == '*');
            if (starCount == 1)
            {
                if (searchKey.IndexOf('*') == 0)
                {
                    //EndsWith
                    query.WhereEnds(columnName, searchKey.TrimStart('*'), true);
                }
                else if (searchKey.IndexOf('*') == searchKey.Length - 1)
                {
                    //StartsWith
                    query.WhereStarts(columnName, searchKey.TrimEnd('*'), true);
                }
                else
                {
                    //Startswith && EndsWith
                    string[] temp = searchKey.Split('*');
                    query.WhereStarts(columnName, temp[0], true);
                    query.WhereEnds(columnName, temp[1], true);
                }
            }
            else if (starCount == 2 && searchKey.StartsWith("*") && searchKey.EndsWith("*"))
            {
                //Contains
                query.WhereContains(columnName, searchKey.Trim('*'), true);
            }
            else
            {
                //Regex
                string formattedKey = RegexUtility.ConvertWildcardPatternToRegex(searchKey);
                query.WhereRegex(columnName, formattedKey, true);
            }
            return query;
        }*/
        //protected virtual string[] SplitKey(string key)
        //{
        //    return key.SplitSearchKey();
        //}

        protected virtual string[] ConvertKey2QueryObject(string key, ExplorerSearchColumnOperationLogic columnOperationLogic)
        {
            return columnOperationLogic == ExplorerSearchColumnOperationLogic.Contains ? key.SplitSearchKey() : new string[] { key };
        }


        protected abstract bool CanSearch(ExplorerQueryColumn column);

        #region Advanced search
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!CanSearch(column)) return query;
            return Search(query, column, JsonConvert.DeserializeObject<string>(objJson).ToLower(), columnOperationLogic, keyOperationLogic);
        }
        #endregion
    }
}
