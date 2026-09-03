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
using SqlKata;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public abstract class BaseArraySearchBuilder : ISearchBuilder, IAdvancedQueryBuilder
    {
        public Query Search(Query query, ExplorerSearchOptionV2 searchOption)
        {
            return Search(query, searchOption, ExplorerSearchColumnOperationLogic.Contains);
        }

        private Query Search(Query query, ExplorerSearchOptionV2 searchOption, ExplorerSearchColumnOperationLogic columnOperationLogic)
        {
            if (!CanSearch(searchOption)) return query;
            return Build(query, columnOperationLogic, searchOption.OperationLogic, searchOption.Key.Trim());
        }

        private Query Build(Query query, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic, string searchKey)
        {
            if (columnOperationLogic == ExplorerSearchColumnOperationLogic.Contains)
            {
                if (searchKey.Contains('*'))
                {
                    //wildcard match
                    return BuildWildcardSearch(query, keyOperationLogic, searchKey);
                }
                else
                {
                    //split key, exact match
                    return BuildContains(query, keyOperationLogic, searchKey);
                }
            }
            else
            {
                return BuildEquals(query, searchKey);
            }
            //return columnOperationLogic == ExplorerSearchColumnOperationLogic.Contains ? BuildContains(query, keyOperationLogic, searchKey) : BuildEquals(query, searchKey);
        }

        private Query BuildContains(Query query,  ExplorerSearchKeyOperationLogic keyOperationLogic, string searchKey)
        {
            var columnName = GetSearchArrayColumnName();
            var splitedKeys = ConvertKey2QueryObject(searchKey, ExplorerSearchColumnOperationLogic.Contains);
            Dictionary<string, List<string>> stringTermsDic = RASharpNLPUtility.AnalyzeStringTerms(splitedKeys.ToArray());

            if (splitedKeys.Count() > 1)
            {
                if (keyOperationLogic == ExplorerSearchKeyOperationLogic.AND)
                {
                    query.Where(a =>
                    {
                        foreach (string subKey in splitedKeys)
                        {
                            a.BuildExists(columnName.FormatColumnName(), $"sub{columnName}", subKey, stringTermsDic);
                        }
                        return a;
                    });             
                }
                else
                {
                    query.WhereExists(q =>
                    {
                        q.FromParent($"sub{columnName}", columnName.FormatColumnName())
                        .WhereArrayContainV2(splitedKeys, string.Empty);
                        return q;
                    });
                }
            }
            else if (splitedKeys.Count() == 1)
            {
                //query.WhereArrayContain(columnName.FormatColumnName(), splitedKeys[0]);
                query.BuildExists(columnName.FormatColumnName(), $"sub{columnName}", splitedKeys[0], stringTermsDic);
            }
            else // use the original search key if no valid keys after spliting
            {
                query.WhereArrayContain(columnName.FormatColumnName(), searchKey);
            }
            return query;
        }

        private Query BuildWildcardSearch(Query query, ExplorerSearchKeyOperationLogic keyOperationLogic, string searchKey)
        {
            var columnName = GetSearchColumnName();
            int starCount = searchKey.Count(a => a == '*');
            if (starCount == 1)
            {
                if(searchKey.IndexOf('*') == 0)
                {
                    //EndsWith
                    query.WhereEnds(columnName.FormatColumnName(), searchKey.TrimStart('*'));
                } 
                else if (searchKey.IndexOf('*') == searchKey.Length - 1)
                {
                    //StartsWith
                    query.WhereStarts(columnName.FormatColumnName(), searchKey.TrimEnd('*'));
                }
                else
                {
                    //Startswith && EndsWith
                    string[] temp = searchKey.Split('*');
                    query.WhereStarts(columnName.FormatColumnName(), temp[0]);
                    query.WhereEnds(columnName.FormatColumnName(), temp[1]);
                }
            } 
            else if(starCount == 2 && searchKey.StartsWith("*") && searchKey.EndsWith("*"))
            {
                //Contains
                query.WhereContains(columnName.FormatColumnName(), searchKey.Trim('*'));
            }
            else
            {
                string formattedKey = RegexUtility.ConvertWildcardPatternToRegex(searchKey);
                //Regex
                query.WhereRegex(columnName.FormatColumnName(), formattedKey);
            }
            return query;
        }
        
        private Query BuildEquals(Query query, string searchKey)
        {
            if (string.IsNullOrEmpty(searchKey)) return query;
            var columnName = GetSearchColumnName().FormatColumnName();
            return query.WhereStringEquals(columnName, searchKey.Trim());
        }

        protected virtual string[] ConvertKey2QueryObject(string key, ExplorerSearchColumnOperationLogic columnOperationLogic)
        {
            return key.SplitSearchKey();
        }
        protected abstract List<string> GetSearchColumnIds();

        protected abstract string GetSearchArrayColumnName();

        protected abstract string GetSearchColumnName();

        private bool CanSearch(ExplorerSearchOptionV2 searchOption)
        {
            return searchOption.IfHasSearchColumns(GetSearchColumnIds());
        }

        #region Advanced search

        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!CanDo(column)) return query;
            var searchOption = Convert2SearchOptionV2(column, objJson, keyOperationLogic);
            return Search(query, searchOption, columnOperationLogic);
        }

        private bool CanDo(ExplorerQueryColumn column)
        {
            return GetSearchColumnIds().Contains(column.Id);
        }

        protected abstract ExplorerSearchOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchKeyOperationLogic keyOperationLogic);
        #endregion
    }
}
