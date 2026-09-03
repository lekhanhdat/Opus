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
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model.DataIngestion;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using System.Data.SqlClient;

namespace AvePoint.RA.DB.Dao.DataIngestion.Impl
{
    public class RMDataIngestionMessageDao : BaseDao<RMDataIngestionMessage>,IRMDataIngestionMessageDao
    {
        public async Task AddOrUpdateAsync(RMDataIngestionMessage message)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.DataIngestionMessages.AddOrUpdate(message);
            await context.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(IEnumerable<RMDataIngestionMessage> messages)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.DataIngestionMessages.AddOrUpdate(messages.ToArray());
            await context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var message = await context.DataIngestionMessages.FirstOrDefaultAsync(item => item.Id == id);
            if (message == null) return false;
            context.DataIngestionMessages.Remove(message);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAnalyzedFinishMessageAsync(string uniqueId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var message = await context.DataIngestionMessages.FirstOrDefaultAsync(item => item.UniqueId == uniqueId && item.Status == RMDataIngestionMessageStatus.AnalyzeFinished);
            if (message == null)
            {
                return;
            }
            context.DataIngestionMessages.Remove(message);
            await context.SaveChangesAsync();
        }

        public async Task<RMDataIngestionMessage> TryClaimNextMessageAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var sql = @$"
            ;WITH CTE AS ( 
                SELECT TOP (1) *
                FROM [{context.SchemaName}].[RMDataIngestionMessages]
                WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE Status = @Pending
                ORDER BY CreatedTime
            )
            UPDATE CTE
            SET Status = @Processing
            OUTPUT INSERTED.*";
            var parameters = new[]
            {
                new SqlParameter("@Pending", (int)RMDataIngestionMessageStatus.Pending),
                new SqlParameter("@Processing", (int)RMDataIngestionMessageStatus.Processing)
            };
            return await context.Database.SqlQuery<RMDataIngestionMessage>(sql, parameters).FirstOrDefaultAsync();
        }

        public async Task<int> PrepareNextMessageAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var sql = @$"
            ;WITH CTE AS ( 
                SELECT TOP (1) Id, Status
                FROM [{context.SchemaName}].[RMDataIngestionMessages]
                WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE Status = @Waiting
                ORDER BY CreatedTime
            )
            UPDATE CTE
            SET Status = @Pending";

            var parameters = new[]
            {
                new SqlParameter("@Waiting", (int)RMDataIngestionMessageStatus.Waiting),
                new SqlParameter("@Pending", (int)RMDataIngestionMessageStatus.Pending)
            };
            return await context.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        public async Task<bool> HasExecutableMessagesAsync(RMDataIngestionType ingestionType)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionMessages
                .AsNoTracking()
                .AnyAsync(x => x.Status == RMDataIngestionMessageStatus.Pending && x.IngestionType == ingestionType);
        }

        public async Task<int> GetExecutableMessageCount(RMDataIngestionMessageDto message)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionMessages
                .AsNoTracking()
                .CountAsync(x => x.Status == RMDataIngestionMessageStatus.Pending && x.OperationType == message.OperationType && x.UniqueId == message.UniqueId);
        }

        public async Task<List<String>> GetExecutableMessageAsync(RMDataIngestionType ingestionType)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionMessages
                .AsNoTracking()
                .Where(x => x.Status == RMDataIngestionMessageStatus.Pending && x.IngestionType == ingestionType)
                .Select(x => x.UniqueId)
                .Distinct()
                .ToListAsync();
        }
    }
}
