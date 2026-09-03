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
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMEXOLabelDao : BaseDao<RMEXOLabel>, IRMEXOLabelDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMEXOLabelDao));
        public List<T> GetFilterList<T>(Expression<Func<RMEXOLabel, T>> selectLambda, Expression<Func<RMEXOLabel, bool>> whereLambda)
        {

            if (selectLambda == null)
            {
                return new List<T>();
            }
            using (var context = GetNewContext())
            {
                if (whereLambda != null)
                {
                    return context.RMRetentionLabel.AsQueryable().Where(whereLambda).Select(selectLambda).Distinct().ToList();
                }
                else
                {
                    return context.RMRetentionLabel.AsQueryable().Select(selectLambda).Distinct().ToList();
                }
            }
        }

        public RMEXOLabel GetLabel(int type, int status)
        {
            using (var context = GetNewContext())
            {
                return context.RMRetentionLabel.AsQueryable().Where(r => r.Type == type && r.Status == status).FirstOrDefault();
            }
        }
        public List<RMEXOLabel> GetLabelByStatus(int status)
        {
            using (var context = GetNewContext())
            {
                return context.RMRetentionLabel.AsQueryable().Where(r => r.Status == status).ToList();
            }
        }

        public List<RMEXOLabel> GetLabelByStatusAndType(int status, int type)
        {
            using (var context = GetNewContext())
            {
                return context.RMRetentionLabel.AsQueryable().Where(r => r.Status == status && r.Type == type).ToList();
            }
        }

        public List<RMEXOLabel> GetLabelByType(int type)
        {
            using (var context = GetNewContext())
            {
                return context.RMRetentionLabel.AsQueryable().Where(r => r.Type == type).ToList();
            }
        }

        public async Task<IEnumerable<RMEXOLabel>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMRetentionLabel.AsNoTracking().OrderBy(e => e.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertEXOLabelTableAsync(IEnumerable<RMEXOLabel> exoLabels)
        {
            using var context = GetNewContext();
            string tableName = "RMEXOLabels";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, LabelName, Status, Type, LabelId, recordId, SavedTime) VALUES ");
                int i = 0;
                foreach (var item in exoLabels)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.LabelName));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.Status));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.Type));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", item.LabelId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", item.recordId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", item.SavedTime));
                    paramIndex += 7;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMEXOLabels data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllEXOLabelAsync()
        {
            return await TruncateAllDataInTableAsync("RMEXOLabels");
        }

        public int RemoveOldFaildLabel(int type)
        {
            int rows = 0;
            using (var context = GetNewContext())
            {
                if (context.RMRetentionLabel.AsQueryable().Count(r => r.Type == type && r.Status == (int)RMRetentionLabelStatus.Failed) > 1)
                {
                    string sql1 = "delete from {0}.RMEXOLabels where Status = " + (int)RMRetentionLabelStatus.Failed + " and Type = " + type;
                    rows = context.Database.ExecuteSqlCommand(string.Format(sql1, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)));
                }
            }
            return rows;

        }
      
    }
}
