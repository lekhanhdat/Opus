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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMWorkflowStepDao : BaseDao<RMWorkflowStep>, IRMWorkflowStepDao
    {
        private RALogger Logger = RALogger.GetInstance(typeof(RMWorkflowStepDao));
        public void UpdateStep(RMWorkflowStep step)
        {
            using(var context = GetNewContext())
            {
                ApplyCurrentValues(context, step);
            }
        }

        public async Task<IEnumerable<RMWorkflowStep>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.WorkflowStep.AsNoTracking().OrderBy(s => s.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertWorkflowStepTableAsync(IEnumerable<RMWorkflowStep> workflowSteps)
        {
            using var context = GetNewContext();
            string tableName = "RMWorkflowSteps";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                var sqlBuilder = new StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, DefinitionId, Name, DisplayName, ReviewerType, UsedEmailTemplateMode, UsedEmailTemplateId, CustomIntervalSetting) VALUES ");
                int i = 0;
                foreach (var item in workflowSteps)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.DefinitionId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.Name));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.DisplayName));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", (int)item.ReviewerType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", (int)item.UsedEmailTemplateMode));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", item.UsedEmailTemplateId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 7}", (object)item.CustomIntervalSetting ?? DBNull.Value));
                    paramIndex += 8;
                    i++;
                }

                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMWorkflowSteps data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllWorkflowStepAsync()
        {
            return await TruncateAllDataInTableAsync("RMWorkflowSteps");
        }
    }
}
