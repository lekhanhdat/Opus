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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMTenantVectorCosmosMappingDao
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMTenantVectorCosmosMappingDao));

        /// <summary>
        /// Get or create the mapping for the tenant's vector container and database.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Tuple of DatabaseName and ContainerName</returns>
        public (string DatabaseName, string ContainerName) GetOrCreateDatabaseAndContainerName(Guid tenantId, bool create = true)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                // Check if mapping already exists
                var existingMapping = context.TenantVectorCosmosMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);

                if (existingMapping != null)
                {
                    return (existingMapping.DatabaseName, existingMapping.ContainerName);
                }
                if (create)
                {
                    // Get all used databases and container counts
                    var allMappings = context.TenantVectorCosmosMapping.ToList();
                    string baseDbName = "RECO_Vector";
                    int dbIndex = 1;
                    string dbName = baseDbName;
                    while (true)
                    {
                        var count = allMappings.Count(m => m.DatabaseName == dbName);
                        if (count < 25)
                            break;
                        dbIndex++;
                        dbName = $"{baseDbName}_{dbIndex}";
                    }
                    var containerName = $"{tenantId}";
                    var newMapping = new RMTenantVectorCosmosMapping
                    {
                        TenantId = tenantId,
                        ContainerName = containerName,
                        DatabaseName = dbName
                    };
                    context.TenantVectorCosmosMapping.Add(newMapping);
                    context.SaveChanges();
                    return (dbName, containerName);
                }
                return (null, null);
                
            }
        }

        /// <summary>
        /// Get the container name for the tenant.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <returns>Container name, or null if not found</returns>
        public string GetContainerName(Guid tenantId)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var mapping = context.TenantVectorCosmosMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);

                return mapping?.ContainerName;
            }
        }

        /// <summary>
        /// Update the mapping's updated time for the tenant.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        public void UpdateMappingTime(Guid tenantId)
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var mapping = context.TenantVectorCosmosMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);

                if (mapping != null)
                {
                    mapping.UpdatedTime = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }

        public void DeleteMapping(string tenantId)
        {
            try
            {
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    var mapping = context.TenantVectorCosmosMapping
                        .FirstOrDefault(m => m.TenantId.ToString().Equals(tenantId));

                    if (mapping != null)
                    {
                        context.TenantVectorCosmosMapping.Remove(mapping);
                        context.SaveChanges();
                        _logger.Info($"Deleted mapping for tenant {tenantId}");
                    }
                    else
                    {
                        _logger.Warn($"No mapping found for tenant {tenantId} to delete");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error deleting mapping for tenant {tenantId}: {e.Message}", e);
                throw;
            }
        }

    }
}
