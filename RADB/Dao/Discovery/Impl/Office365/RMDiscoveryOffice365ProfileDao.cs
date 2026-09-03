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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365ProfileDao : IRMDiscoveryOffice365ProfileDao
    {
        public async Task InitBuildInDataAsync(Guid o365TenantId)
        {
            var inactiveProfileInfo = new RMDiscoveryOffice365ProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = "RM_FA_Inactive_Default_ProfileName",
                SizeRange = -1,
                SizeRangeQueryMode = RMDiscoverySizeRangeQueryMode.GenerateThanEqual,
                GreaterThanEqualWithoutInDate = -1,
                LessThanEqualWithoutInDate = 999,
                FileExtensionIdsJson = JsonConvert.SerializeObject(new List<int>()),
                RuleIdsJson = JsonConvert.SerializeObject(new List<int>()),
                SortBy = "FileTotalSize",
                CreatedTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                ScanType = RMDiscoveryJobType.Newly,
                PrevScanStatus = RMDiscoveryJobStatus.Finished,
                CurrentScanStatus = RMDiscoveryJobStatus.Waiting,
                ProfileType = RMDiscoveryProfileType.Inactive,
                IsBuildIn = true,
                IsDefault = true,
            };
            var rotProfileInfo = new RMDiscoveryOffice365ProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = "RM_FA_Rot_Default_ProfileName",
                SizeRange = -1,
                SizeRangeQueryMode = RMDiscoverySizeRangeQueryMode.GenerateThanEqual,
                GreaterThanEqualWithoutInDate = -1,
                LessThanEqualWithoutInDate = 999,
                FileExtensionIdsJson = JsonConvert.SerializeObject(new List<int>()),
                RuleIdsJson = JsonConvert.SerializeObject(new List<int>()),
                SortBy = "FileTotalSize",
                CreatedTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                ScanType = RMDiscoveryJobType.Newly,
                PrevScanStatus = RMDiscoveryJobStatus.Finished,
                CurrentScanStatus = RMDiscoveryJobStatus.Waiting,
                ProfileType = RMDiscoveryProfileType.ROT,
                IsBuildIn = true,
                IsDefault = true,
            };

            await AddOrUpdateProfileInfoAsync(o365TenantId, inactiveProfileInfo);
            await AddOrUpdateProfileInfoAsync(o365TenantId, rotProfileInfo);
        }

        public async Task<List<RMDiscoveryOffice365ProfileInfo>> GetProfileInfoesAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);

            return await efContext.Office365ProfileInfoes.ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365ProfileInfo>> GetProfileInfoesAsync(Guid o365TenantId, RMDiscoveryProfileType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);

            return await efContext.Office365ProfileInfoes.Where(item => item.ProfileType == type).OrderBy(item => item.ModifiedTime).ToListAsync();
        }

        public async Task<RMDiscoveryOffice365ProfileInfo> GetProfileInfoByIdAsync(Guid o365TenantId, Guid profileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ProfileInfoes.FirstAsync(item => item.Id == profileId);
        }

        public async Task AddOrUpdateProfileInfoAsync(Guid o365TenantId, RMDiscoveryOffice365ProfileInfo profileInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365ProfileInfoes.AddOrUpdate(profileInfo);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteProfileInfoAsync(Guid o365TenantId, Guid id)
        {
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileInfoes] WHERE Id = @Id";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Id", id));
        }

        public async Task<List<RMDiscoveryProfileFailedInfo>> GetProfileFailedInfoesAsync(Guid o365TenantId, Guid profileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ProfileFailedInfoes.Where(item => item.ProfileId == profileId).ToListAsync();
        }

        public async Task AddOrUpdateProfileFailedInfoesAsync(Guid o365TenantId, params RMDiscoveryProfileFailedInfo[] failedInfoes)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365ProfileFailedInfoes.AddOrUpdate(failedInfoes);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteProfileFailedInfoesAsync(Guid o365TenantId, Guid profileId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMProfileFailedInfoes] WHERE ProfileId = @ProfileId";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ProfileId", profileId));
        }
    }
}
