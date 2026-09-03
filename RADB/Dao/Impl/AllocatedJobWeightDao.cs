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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class AllocatedJobWeightDao : BaseDao<AllocatedJobWeight>, IAllocatedJobWeightDao
    {
        public void AddOrUpdate(int newAllocatedWeight)
        {
            using var context = GetNewContext();
            var schema = SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName());
            string sql = $@"
                IF EXISTS (SELECT 1 FROM [{schema}].AllocatedJobWeights)
                    UPDATE [{schema}].AllocatedJobWeights SET AllocatedWeight = @p0
                ELSE
                    INSERT INTO [{schema}].AllocatedJobWeights (AllocatedWeight) VALUES (@p0);
                ";

            context.Database.ExecuteSqlCommand(sql, newAllocatedWeight);
        }

        public async Task<int> GetAllocatedJobWeight()
        {
            using var context = GetNewContext();

            var schema = SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName());

            string sql = $@"SELECT TOP 1 AllocatedWeight FROM [{schema}].[AllocatedJobWeights];";

            var result = await context
                .Database
                .SqlQuery<int?>(sql)
                .FirstOrDefaultAsync();
            return result ?? 0;
        }


        public async Task ReleaseJobWeight(int jobWeight)
        {
            using var context = GetNewContext();
            var schema = SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName());
            string sql = $@"
                IF EXISTS (SELECT 1 FROM [{schema}].[AllocatedJobWeights])
                BEGIN
                    UPDATE [{schema}].[AllocatedJobWeights]
                    SET AllocatedWeight = CASE 
                        WHEN AllocatedWeight - @p0 < 0 THEN 0
                        ELSE AllocatedWeight - @p0
                    END;
                END
                ELSE
                BEGIN
                    INSERT INTO [{schema}].[AllocatedJobWeights] (AllocatedWeight) VALUES (0);
                END
                ";
            await context.Database.ExecuteSqlCommandAsync(sql, jobWeight);
        }


    }
}
