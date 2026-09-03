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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMGoogleLabelInfoDao : BaseDao<RMGoogleLabelInfo>, IRMGoogleLabelInfoDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMGoogleLabelInfoDao));
        public List<RMGoogleLabelInfo> GetGoogleTermsInforByTenantIdAndTermUniqueIds(string tenantId, List<Guid> ids)
        {
            using var context = GetNewContext();
            return context.RMGoogleLabelInfo.Where(x => x.TenantId == tenantId && ids.Contains(x.TermUniqueId)).ToList();
        }

        public async Task<(RMGoogleLabelInfo,string)> GetGoogleTermsByLabelId(string labelId)
        {
            using var context = GetNewContext();
            var query = await context.RMGoogleLabelInfo.Where(googleLabel => googleLabel.LabelId == labelId).Join(context.Terms.Where(term => !term.IsRemoved), googleLabel => googleLabel.TermUniqueId, term => term.UniqueId, (googleLabel, term) => new
            {
                TermName = term.Name,
                GoogleLabelInfo = googleLabel
            }).ToListAsync();
            return query.Count == 0 ? (null, string.Empty) : (query[0].GoogleLabelInfo, query[0].TermName);
        }

        public async Task<IEnumerable<RMGoogleLabelInfo>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMGoogleLabelInfo.AsNoTracking().OrderBy(x => x.UniqueId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertGoogleLabelInfoTableAsync(IEnumerable<RMGoogleLabelInfo> googleLabelInfos)
        {
            using var context = GetNewContext();
            string tableName = "RMGoogleLabelInfoes";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (UniqueId, LabelId, TermId, TermUniqueId, LabelName, LabelType, TenantId, State, Extension) VALUES ");
                int i = 0;
                foreach (var item in googleLabelInfos)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.UniqueId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", (object)item.LabelId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.TermId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.TermUniqueId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", (object)item.LabelName ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", (int)item.LabelType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", (object)item.TenantId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 7}", item.State));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 8}", (object)item.Extension ?? DBNull.Value));
                    paramIndex += 9;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMGoogleLabelInfoes data has error: {ex}");
                return 0;
            }
        }
        public async Task<long> MultiGeoDeleteAllGoogleLabelInfoAsync()
        {
            return await TruncateAllDataInTableAsync("RMGoogleLabelInfoes");
        }
    }
}
