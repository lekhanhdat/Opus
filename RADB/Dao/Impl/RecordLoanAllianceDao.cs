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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RecordLoanAllianceDao : BaseDao<RMRecordLoanAlliance>, IRecordLoanAllianceDao
    {
        public bool CreateOrUpdateLoanAlliance(RMRecordLoanAlliance alliance)
        {
            using (var context = GetNewContext())
            {
                RMRecordLoanAlliance existEntity = context.LoanAlliance.FirstOrDefault(d => d.RecordsId == alliance.RecordsId);
                if (existEntity == null)
                {
                    context.LoanAlliance.Add(alliance);
                    return context.SaveChanges() > 0;
                }
                else
                {
                    existEntity.HoldBy = alliance.HoldBy;
                    existEntity.HoldReleaseTime = alliance.HoldReleaseTime;
                    return this.ApplyCurrentValues(context, existEntity);
                }
            }
            return false;

        }

        public void UpdateLoanedBy(Guid id, string holdBy)
        {
            using (var ctx = GetNewContext())
            {
                string sql = "update {0}.RMRecordLoanAlliances set HoldBy = @holdBy where RecordsId = @recordsId";
                SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName);
                int result = ctx.Database.ExecuteSqlCommand(string.Format(sql, ctx.SchemaName), new SqlParameter("holdBy", holdBy), new SqlParameter("recordsId", id));
            }
        }

        public List<RMRecordLoanAlliance> GetPhyRecordAllianceById(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.LoanAlliance.AsQueryable().Where(a => a.RecordsId == id).ToList();
            }
        }
        public List<RMRecordLoanAlliance> GetPhyRecordAllianceByIds(List<Guid> ids)
        {
            if (ids != null && ids.Count != 0)
            {
                using var ctx = GetNewContext();
                return ctx.LoanAlliance.AsQueryable().Where(a => ids.Contains(a.RecordsId)).ToList();
            }
            return [];
        }

        public List<Guid> GetPhyFoldersIdByBoxIds(List<Guid> ids)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.LoanAlliance.AsQueryable().Where(a => ids.Contains(a.ParentId)).Select(l => l.RecordsId).ToList();
            }
        }

        public async Task BatchDeleteRecordAllianceByIdsAsync(List<Guid> ids)
        {
            try
            {
                await this.BatchDeleteAsync(a => ids.Contains(a.RecordsId));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Tuple<string, Guid>> GetAllRecordsIdAndHoldBy()
        {
            using (var ctx = GetNewContext())
            {
                var s = ctx.LoanAlliance.AsQueryable().Select(o => new {o.HoldBy, o.RecordsId}).ToList();
                return s.Select(o => new Tuple<string, Guid>(o.HoldBy, o.RecordsId)).ToList();
            }
        }

        public bool IsRecordsLoan(List<Guid> ids, long ticks)
        {
            using (var ctx = GetNewContext())
            {
                int loanCount = ctx.LoanAlliance.AsQueryable().Count(a => ids.Contains(a.RecordsId));
                return loanCount > 0;
            }
        }

        public List<RMRecordLoanAlliance> GetChildAndParentRecordAllianceByIds(List<Guid> ids)
        {
            if (ids != null && ids.Count != 0)
            {
                using var ctx = GetNewContext();
                return ctx.LoanAlliance.AsQueryable().Where(a => ids.Contains(a.RecordsId) || ids.Contains(a.ParentId)).ToList();
            }
            return [];
        }

        public bool IsRecordsLoan(List<Guid> ids)
        {
            using (var ctx = GetNewContext())
            {
                int loanCount = ctx.LoanAlliance.AsQueryable().Count(a => ids.Contains(a.RecordsId));
                return loanCount > 0;
            }
        }
    }
}
