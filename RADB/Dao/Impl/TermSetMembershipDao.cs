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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermSetMembershipDao : BaseDao<RMTermSetMembership>, ITermSetMembershipDao
    {
        private RALogger Logger = RALogger.GetInstance(typeof(TermSetMembershipDao));
        public void AddTermSetMemberShip()
        {

        }

        public bool IsTermUsed(int termId)
        {
            return this.Exist(t => t.IsSource == true && t.TermId == termId);
        }

        public List<RMTermSetMembership> GetRMTermSetMemberships(int[] termIds,bool isWithRemoved = false)
        {
            using var context = GetNewContext();
            if (isWithRemoved)
            {
                return context.TermSetMemberships.AsQueryable().Where(tm => Enumerable.Contains(termIds, tm.TermId)).ToList();
            }
            return context.TermSetMemberships.AsQueryable().Where(tm => Enumerable.Contains(termIds, tm.TermId) && tm.IsRemoved == isWithRemoved).ToList();
        }
        public int GetSubTermCountByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Where(tm => tm.TermSetId == termSetId && tm.IsRemoved == false && tm.ParentTermId == 0).Count();
        }
        public List<RMTermSetMembership> GetSubTermMembershipsByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSetId && a.ParentTermId == 0 && a.IsRemoved == false).ToList();
        }
        public List<RMTermSetMembership> GetSubTermMembershipByTermId(int termId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == termId && a.IsRemoved == false).ToList();
        }

        public RMTermSetMembership GetMembershipByTermId(int termId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Where(a => a.TermId == termId && a.IsRemoved == false).FirstOrDefault();
        }

        public RMTermSetMembership GetByTermNameAndParentId(int parentId, string termName, bool isRootTerm =  false)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (isRootTerm)
                {
                    return context.TermSetMemberships.FirstOrDefault(a => a.TermSetId == parentId && a.ParentTermId == 0 && a.TermName.Equals(termName, System.StringComparison.OrdinalIgnoreCase) && !a.IsRemoved);
                }
                else
                {
                    return context.TermSetMemberships.FirstOrDefault(a => a.ParentTermId == parentId && a.TermName.Equals(termName, System.StringComparison.OrdinalIgnoreCase) && !a.IsRemoved);
                }
            }
        }
        public void DeleteAllMemberShips()
        {
            using var context = GetNewContext();
            var ships = context.TermSetMemberships.ToList();
            if (ships.Count > 0)
            {
                context.TermSetMemberships.RemoveRange(ships);
                context.SaveChanges();
            }
        }

        public string GetMaxDeepTermPath()
        {
            using(var context = RMDBContextManager.GetNewDBContext())
            {
                var sql = $"Select Top 1 Path From {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMTermSetMemberships Where IsRemoved = 0 Order By DATALENGTH(Path) DESC";
                return context.Database.SqlQuery<string>(sql).FirstOrDefault();
            }
        }

        public async Task<IEnumerable<RMTermSetMembership>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.TermSetMemberships.AsNoTracking().OrderBy(a => a.TermId).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermSetMembershipTableAsync(IEnumerable<RMTermSetMembership> termSetMemberships)
        {
            using var context = GetNewContext();
            string tableName = "RMTermSetMemberships";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName}([TermId],[TermSetId],[ParentTermId],[TermName],[Path],[IsSource],[IsRemoved]) VALUES");
                int i = 0;
                foreach (var item in termSetMemberships)
                { 
                    if(i>0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.TermId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.TermSetId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", item.ParentTermId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.TermName));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.Path));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.IsSource));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.IsRemoved));
                    paramIndex += 7;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMTermSetMemberships data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllTermSetMembershipAsync()
        {
            return await TruncateAllDataInTableAsync("RMTermSetMemberships");
        }
    }
}
