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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleFileExtensionDao : IRMDiscoveryGoogleFileExtensionDao
    {
        public async Task AddOrUpdateAsync(string googleOrganizationId, params RMDiscoveryGoogleFileExtension[] fileTypes)
        {
            if (!fileTypes.Any())
            {
                return;
            }
            using var context = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            context.GoogleFileExtensions.AddOrUpdate(fileTypes);
            await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryGoogleFileExtension>> GetAllAsync(string googleOrganizationId)
        {
            using var context = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await context.GoogleFileExtensions.ToListAsync();
        }

        public async Task<List<RMDiscoveryGoogleFileExtension>> GetAsync(string googleOrganizationId, List<int> IDs)
        {
            using var context = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await context.GoogleFileExtensions.Where(e => IDs.Contains(e.Id)).ToListAsync();
        }
    }
}
