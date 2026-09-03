
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
using System;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using Newtonsoft.Json;
using SqlKata;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class DirPathSuggestQueryBuilder : BaseContainsFilterBuilder, IAdvancedQueryBuilder
    {
        protected override string GetFilterColumnName()
        {
            return CosmosConst.C_DirPath;
        }

        protected override bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && !string.IsNullOrEmpty(filterOption.DirPath);
        }

        protected override object GetFilterValue(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.DirPath;
        }

        #region Advanced search
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(objJson);
            return Filter(query, filterOption);
        }

        protected string GetColumnId()
        {
            return QueryCloumnIds.DirPath;
        }

        protected static ExplorerFilterOptionV2 Convert2SearchOptionV2(string objJson)
        {
            return new ExplorerFilterOptionV2
            {
                DirPath = GetRelativePath(JsonConvert.DeserializeObject<string>(objJson))
            };
        }

        public static string GetRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            string result;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                result = uri.AbsolutePath.TrimStart('/').TrimEnd('/');
            }
            else
            {
                result = path.TrimStart('/').TrimEnd('/');
            }

            return string.IsNullOrWhiteSpace(result) ? path : result;
        }
        #endregion
    }
}
