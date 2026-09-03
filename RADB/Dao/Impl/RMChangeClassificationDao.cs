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
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Core;
using System.Data.Entity;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMChangeClassificationDao : BaseDao<RMChangeClassification>, IRMChangeClassificationDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMChangeClassificationDao));
        public void AddChange(List<Guid> changeIds, int changeType)
        {
            using (var context = GetRMDBContext())
            {
                List<RMChangeClassification> changeObjs = new List<RMChangeClassification>();
                var time = DateTime.UtcNow.Ticks;
                foreach (var id in changeIds)
                {
                    if (!context.ChangeClassifications.Any(t => t.TermId == id && t.ChangeType == changeType))
                    {
                        changeObjs.Add(new RMChangeClassification() { TermId = id, ChangeTime = time, ChangeType = changeType });
                    }
                    else
                    {
                        //update.
                        var entities = context.ChangeClassifications.Where(d => d.TermId == id && d.ChangeType == changeType).ToList();
                        foreach (var entity in entities)
                        {
                            entity.ChangeTime = time;
                        }
                        BatchUpdate(context, entities);
                    }
                }
                if (changeObjs.Count > 0)
                {
                    context.ChangeClassifications.AddRange(changeObjs);
                    context.SaveChanges();
                }
            }
        }

        public List<Guid> GetAllChange(long ticks, int changeType)
        {
            using (var context = GetRMDBContext())
            {
                return context.ChangeClassifications.AsNoTracking().Where(t => t.ChangeType == changeType && t.ChangeTime > ticks).Select(t => t.TermId).ToList();
            }
        }

        public List<RMChangeClassification> GetAllChangedInfo(long ticks, int changeType)
        {
            using var context = GetRMDBContext();
            return context.ChangeClassifications.AsQueryable().Where(t => t.ChangeType == changeType && t.ChangeTime > ticks).ToList();
        }

        public List<Guid> GetAllChangeByType(int changeType)
        {
            using (var context = GetRMDBContext())
            {
                return context.ChangeClassifications.AsNoTracking().Where(t => t.ChangeType == changeType).Select(t => t.TermId).ToList();
            }
        }   

        public Task RemoveChangeAsync(int changeType)
        {
            //using (var context = GetRMDBContext())
            //{
            //    context.ChangeClassifications.Where(t => t.ChangeType == changeType).Delete();
            //}
            return BatchDeleteAsync(t => t.ChangeType == changeType);
        }
        private RMDbContext GetRMDBContext()
        {
            return RMDBContextManager.GetNewDBContext();
        }

        /// <summary>
        ///  Update ChangeClassifications for term and label (key: TermId/LabelId, value: ChangeType)
        /// </summary>
        /// <param name="changes"></param>
        public void AddChangeLabelsAndTerms(Dictionary<Guid, int> changes)
        {
            using (var context = GetRMDBContext())
            {
                List<RMChangeClassification> changeObjs = new List<RMChangeClassification>();
                var time = DateTime.UtcNow.Ticks;
                foreach (var change in changes)
                {
                    if (!context.ChangeClassifications.Any(t => t.TermId == change.Key && t.ChangeType == change.Value))
                    {
                        changeObjs.Add(new RMChangeClassification() { TermId = change.Key, ChangeTime = time, ChangeType = change.Value });
                    }
                    else
                    {
                        //update.
                        var entities = context.ChangeClassifications.Where(d => d.TermId == change.Key && d.ChangeType == change.Value).ToList();
                        foreach (var entity in entities)
                        {
                            entity.ChangeTime = time;
                        }
                        BatchUpdate(context, entities);
                    }
                }
                if (changeObjs.Count > 0)
                {
                    context.ChangeClassifications.AddRange(changeObjs);
                    context.SaveChanges();
                }
            }
        }

        public async Task<IEnumerable<RMChangeClassification>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.ChangeClassifications.AsNoTracking().OrderBy(t => t.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertChangeClassificationTableAsync(IEnumerable<RMChangeClassification> changeClassifications)
        {
            using var context = GetNewContext();
            string tableName = "RMChangeClassifications";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, TermId, ChangeType, ChangeTime, Extension1, Extension2) VALUES ");
                int i = 0;
                foreach (var item in changeClassifications)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.TermId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.ChangeType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.ChangeTime));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", (object)item.Extension1 ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", (object)item.Extension2 ?? DBNull.Value));
                    paramIndex += 6;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMChangeClassifications data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllChangeClassificationAsync()
        {
            return await TruncateAllDataInTableAsync("RMChangeClassifications");
        }
    }
}
