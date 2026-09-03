

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
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using Newtonsoft.Json;
using SqlKata;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class DirPathListQueryBuilder : BaseArrayFilterBuilder, IAdvancedQueryBuilder
    {
        protected override string GetFilterColumnName()
        {
            return CosmosConst.C_DirPath;
        }

        protected override bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption?.DirPathListItems?.Count > 0;
        }

        protected override object GetFilterValue(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.DirPathListItems;
        }

        public override Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;

            var dirPathColumn = GetFilterColumnName().FormatColumnName();
            var sourceFlagColumn = CosmosConst.C_SourceFlag.FormatColumnName();
            var aveSiteIdColumn = CosmosConst.C_AveSiteId.FormatColumnName();

            return query
                .Where(q => BuildDirPathConditions(q, filterOption, dirPathColumn))
                .Where(q => BuildAveSiteIdConditions(q, filterOption, aveSiteIdColumn))
                .WhereArrayContainV2(new[] { SourceFlag.SharePoint, SourceFlag.OneDrive }, sourceFlagColumn);
        }

        private static Query BuildDirPathConditions(Query query, ExplorerFilterOptionV2 filterOption, string dirPathColumn)
        {
            var items = filterOption.DirPathListItems;
            var first = items[0];

            query.Where(q => q.WhereStarts(dirPathColumn, first.DirPath));
            for (var i = 1; i < items.Count; i++)
            {
                var item = items[i];
                query.OrWhere(q => q.WhereStarts(dirPathColumn, item.DirPath));
            }

            return query;
        }

        private static Query BuildAveSiteIdConditions(Query query, ExplorerFilterOptionV2 filterOption, string aveSiteIdColumn)
        {
            var items = filterOption.DirPathListItems;
            var first = items[0];

            query.Where(q => q.WhereLike(aveSiteIdColumn, first.AveSiteId));
            for (var i = 1; i < items.Count; i++)
            {
               var item = items[i];
                query.OrWhere(q => q.WhereLike(aveSiteIdColumn, item.AveSiteId));
            }

            return query;
        }

        #region Advanced search
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(column, objJson, columnOperationLogic, keyOperationLogic);
            return Filter(query, filterOption);
        }

        protected override string GetColumnId()
        {
            return QueryCloumnIds.DirPath;
        }

        protected override ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            var items = JsonConvert.DeserializeObject<List<DirPathListItem>>(objJson) ?? [];

            return new ExplorerFilterOptionV2
            {
                DirPathListItems = [.. items
                    .Select(item => new DirPathListItem
                    {
                        DirPath = GetRelativePath(item.DirPath),
                        ListId = item.ListId,
                        AveSiteId = item.AveSiteId,
                    })]
            };
        }

        public static string GetRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                return uri.AbsolutePath.TrimEnd('/') + "/";
            }

            return path.TrimEnd('/') + "/";
        }
        #endregion
    }
}
