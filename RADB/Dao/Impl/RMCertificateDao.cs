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
    public class RMCertificateDao : BaseDao<RMCertificate>, IRMCertificateDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMCertificateDao));
        public async Task<IEnumerable<RMCertificate>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMCertificate.AsNoTracking().OrderBy(c => c.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> CreateReplicaCertificateAsync(RMCertificate certificate)
        {
            using var context = GetNewContext();
            string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            string sql = $@"
INSERT INTO {schemaName}.RMCertificates
    (Id, Name, Thumbprint, ValidFrom, ValidTo, EncryptedPWD, BinaryContent)
VALUES
    (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";

            return await context.Database.ExecuteSqlCommandAsync(
                sql,
                new System.Data.SqlClient.SqlParameter("@p0", certificate.Id),
                new System.Data.SqlClient.SqlParameter("@p1", certificate.Name),
                new System.Data.SqlClient.SqlParameter("@p2", certificate.Thumbprint),
                new System.Data.SqlClient.SqlParameter("@p3", certificate.ValidFrom),
                new System.Data.SqlClient.SqlParameter("@p4", certificate.ValidTo),
                new System.Data.SqlClient.SqlParameter("@p5", certificate.EncryptedPWD),
                AddBinaryParameter(6, certificate.BinaryContent));
        }

        public async Task<long> MultiGeoInsertCertificateTableAsync(IEnumerable<RMCertificate> certificates)
        {
            using var context = GetNewContext();
            string tableName = "RMCertificates";
            try
            {
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, Name, Thumbprint, ValidFrom, ValidTo, EncryptedPWD, BinaryContent) VALUES ");
                int i = 0;
                foreach (var item in certificates)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", item.Name));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.Thumbprint));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.ValidFrom));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", item.ValidTo));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", item.EncryptedPWD));
                    parameters.Add(AddBinaryParameter(paramIndex + 6, item.BinaryContent));
                    paramIndex += 7;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMCertificates data has error: {ex}");
                return 0;
            }
        }

        private System.Data.SqlClient.SqlParameter AddBinaryParameter(int paramIndex, byte[] item)
        {
            return new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", System.Data.SqlDbType.VarBinary, -1)
            {
                Value = (object)item ?? DBNull.Value
            };
        }

        public async Task<long> MultiGeoDeleteAllCertificateAsync()
        {
            return await TruncateAllDataInTableAsync("RMCertificates");
        }
    }
}
