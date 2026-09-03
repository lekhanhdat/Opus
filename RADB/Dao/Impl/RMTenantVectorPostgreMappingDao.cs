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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl;

public class RMTenantVectorPostgreMappingDao : IRMTenantVectorPostgreMappingDao
{
    private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMTenantVectorPostgreMappingDao));

    private const string BASE_DATABASE_NAME = "RECO_Vector_PostgreSQL_1";

    private const int MAX_TENANTS_PER_DATABASE = 25;

    /// <summary>
    /// Get or create the database mapping for the tenant's vector data.
    /// Returns the database name that should be used for this tenant.
    /// Every 25 TenantId will be assigned to one DatabaseName.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Database name for the tenant's vector data</returns>
    public string GetOrCreateDatabaseName(string tenantId, bool created = true)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                // Check if mapping already exists
                var existingMapping = context.TenantVectorPostgreMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);

                if (existingMapping != null)
                {
                    _logger.Debug($"Found existing mapping for tenant {tenantId}: {existingMapping.DatabaseName}");
                    return existingMapping.DatabaseName;
                }
                if (created)
                {
                    // Find the database with available space or create a new one
                    var allMappings = context.TenantVectorPostgreMapping.ToList();
                    string databaseName = GetAvailableDatabaseName(allMappings);

                    // Create new mapping
                    var currentTime = DateTime.UtcNow.Ticks;
                    var newMapping = new RMTenantVectorPostgreMapping
                    {
                        TenantId = tenantId,
                        DatabaseName = databaseName,
                        CreatedTime = currentTime,
                        UpdatedTime = currentTime
                    };

                    context.TenantVectorPostgreMapping.Add(newMapping);
                    context.SaveChanges();

                    _logger.Info($"Created new mapping for tenant {tenantId}: {databaseName}");
                    return databaseName;
                }
                else
                {
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error getting or creating database name for tenant {tenantId}: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Get the database mapping for the tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Database mapping or null if not found</returns>
    public RMTenantVectorPostgreMapping GetMappingByTenantId(string tenantId)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                return context.TenantVectorPostgreMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error getting mapping for tenant {tenantId}: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Get all tenants assigned to a specific database.
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <returns>List of tenant mappings</returns>
    public List<RMTenantVectorPostgreMapping> GetMappingsByDatabaseName(string databaseName)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                return context.TenantVectorPostgreMapping
                    .Where(m => m.DatabaseName == databaseName)
                    .ToList();
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error getting mappings for database {databaseName}: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Update the mapping for a tenant.
    /// </summary>
    /// <param name="mapping">Mapping to update</param>
    public void UpdateMapping(RMTenantVectorPostgreMapping mapping)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var existingMapping = context.TenantVectorPostgreMapping
                    .FirstOrDefault(m => m.Id == mapping.Id);

                if (existingMapping != null)
                {
                    existingMapping.DatabaseName = mapping.DatabaseName;
                    existingMapping.UpdatedTime = DateTime.UtcNow.Ticks;
                    
                    context.SaveChanges();
                    _logger.Info($"Updated mapping for tenant {mapping.TenantId}: {mapping.DatabaseName}");
                }
                else
                {
                    _logger.Warn($"Mapping with ID {mapping.Id} not found for update");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error updating mapping for tenant {mapping.TenantId}: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Delete the mapping for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    public void DeleteMapping(string tenantId)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                var mapping = context.TenantVectorPostgreMapping
                    .FirstOrDefault(m => m.TenantId == tenantId);

                if (mapping != null)
                {
                    context.TenantVectorPostgreMapping.Remove(mapping);
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

    /// <summary>
    /// Get all mappings with pagination.
    /// </summary>
    /// <param name="pageIndex">Page index (0-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of mappings</returns>
    public List<RMTenantVectorPostgreMapping> GetMappings(int pageIndex, int pageSize)
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                return context.TenantVectorPostgreMapping
                    .OrderBy(m => m.Id)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error getting mappings with pagination (page: {pageIndex}, size: {pageSize}): {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Get total count of mappings.
    /// </summary>
    /// <returns>Total count</returns>
    public int GetMappingsCount()
    {
        try
        {
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                return context.TenantVectorPostgreMapping.Count();
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Error getting mappings count: {e.Message}", e);
            throw;
        }
    }

    /// <summary>
    /// Find an available database name or create a new one.
    /// Each database can hold up to 25 tenants.
    /// </summary>
    /// <param name="allMappings">All existing mappings</param>
    /// <returns>Available database name</returns>
    private string GetAvailableDatabaseName(List<RMTenantVectorPostgreMapping> allMappings)
    {
        // Group by database name and count tenants
        var databaseUsage = allMappings
            .GroupBy(m => m.DatabaseName)
            .ToDictionary(g => g.Key, g => g.Count());

        // Try to find an existing database with available space
        int dbIndex = 1;
        string candidateName = BASE_DATABASE_NAME;
        
        while (true)
        {
            if (!databaseUsage.ContainsKey(candidateName))
            {
                // This database doesn't exist yet, use it
                return candidateName;
            }
            
            if (databaseUsage[candidateName] < MAX_TENANTS_PER_DATABASE)
            {
                // This database has available space
                return candidateName;
            }
            
            // This database is full, try the next one
            dbIndex++;
            candidateName = $"{BASE_DATABASE_NAME}_{dbIndex}";
        }
    }
}
