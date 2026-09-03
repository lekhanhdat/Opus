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
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao;

public interface IRMTenantVectorPostgreMappingDao
{
    /// <summary>
    /// Get or create the database mapping for the tenant's vector data.
    /// Returns the database name that should be used for this tenant.
    /// Every 25 TenantId will be assigned to one DatabaseName.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Database name for the tenant's vector data</returns>
    string GetOrCreateDatabaseName(string tenantId, bool created = true);

    /// <summary>
    /// Get the database mapping for the tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Database mapping or null if not found</returns>
    RMTenantVectorPostgreMapping GetMappingByTenantId(string tenantId);

    /// <summary>
    /// Get all tenants assigned to a specific database.
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <returns>List of tenant mappings</returns>
    List<RMTenantVectorPostgreMapping> GetMappingsByDatabaseName(string databaseName);

    /// <summary>
    /// Update the mapping for a tenant.
    /// </summary>
    /// <param name="mapping">Mapping to update</param>
    void UpdateMapping(RMTenantVectorPostgreMapping mapping);

    /// <summary>
    /// Delete the mapping for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    void DeleteMapping(string tenantId);

    /// <summary>
    /// Get all mappings with pagination.
    /// </summary>
    /// <param name="pageIndex">Page index (0-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of mappings</returns>
    List<RMTenantVectorPostgreMapping> GetMappings(int pageIndex, int pageSize);

    /// <summary>
    /// Get total count of mappings.
    /// </summary>
    /// <returns>Total count</returns>
    int GetMappingsCount();
}
