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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SPProvisioningContainerDao : BaseDao<SPProvisioningContainer>, ISPProvisioningContainerDao
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(SPProvisioningContainerDao));

        public async Task<bool> CreateIfNotExistsAsync(string tenantId, string webUrl, string listId)
        {
            tenantId = tenantId?.Trim() ?? throw new ArgumentNullException(nameof(tenantId));
            webUrl = webUrl?.Trim()?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(webUrl));
            listId = listId?.Trim();
            var id = HashCodeHelper.ToHashCode($"{webUrl}|{listId}", "MD5");
            using var context = GetNewContext();
            string sql = $"SELECT 1 FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.SPProvisioningContainer WHERE ID=@ID; ";

            var value = await context.Database.SqlQuery<int>(sql, new SqlParameter("@ID", id)).FirstOrDefaultAsync();
            if (value > 0)
            {
                logger.Info($"The container already exists: {webUrl}|{listId}");
                return true;
            }
            else
            {
                logger.Info($"Start adding the container: {webUrl}|{listId}");
                context.SPProvisioningContainers.Add(new SPProvisioningContainer
                {
                    Id = id,
                    TenantId = tenantId,
                    WebUrl = webUrl,
                    ListId = listId,
                    Created = DateTime.UtcNow.Ticks
                });

                return (await context.SaveChangesAsync()) > 0;
            }
        }
    }
}
