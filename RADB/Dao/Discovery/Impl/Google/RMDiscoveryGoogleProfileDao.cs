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
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Profile;
using Newtonsoft.Json;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleProfileDao : IRMDiscoveryGoogleProfileDao
    {
        public async Task InitBuildInDataAsync(string googleOrganizationId)
        {
            var inactiveProfileInfo = new RMDiscoveryGoogleProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = "RM_FA_Inactive_Default_ProfileName",
                SizeRange = -1,
                SizeRangeQueryMode = RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual,
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
            var rotProfileInfo = new RMDiscoveryGoogleProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = "RM_FA_Rot_Default_ProfileName",
                SizeRange = -1,
                SizeRangeQueryMode = RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual,
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

            await AddOrUpdateProfileInfoAsync(googleOrganizationId, inactiveProfileInfo);
            await AddOrUpdateProfileInfoAsync(googleOrganizationId, rotProfileInfo);
        }

        public async Task<List<RMDiscoveryGoogleProfileInfo>> GetProfileInfoesAsync(string googleOrganizationId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleProfileInfoes.ToListAsync();
        }

        public async Task<RMDiscoveryGoogleProfileInfo> GetProfileInfoByIdAsync(string googleOrganizationId, Guid profileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleProfileInfoes.FirstAsync(item => item.Id == profileId);
        }

        public async Task<List<RMDiscoveryGoogleProfileInfo>> GetProfileInfoesAsync(string googleOrganizationId, RMDiscoveryProfileType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleProfileInfoes.Where(item => item.ProfileType == type).OrderBy(item => item.ModifiedTime).ToListAsync();
        }

        public async Task AddOrUpdateProfileInfoAsync(string googleOrganizationId, RMDiscoveryGoogleProfileInfo profileInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            efContext.GoogleProfileInfoes.AddOrUpdate(profileInfo);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteProfileInfoAsync(string googleOrganizationId, Guid id)
        {
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileInfoes] WHERE Id = @Id";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Id", id));
        }

        public async Task<List<RMDiscoveryGoogleProfileFailedInfo>> GetProfileFailedInfoesAsync(string googleOrganizationId, Guid profileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleProfileFailedInfoes.Where(item => item.ProfileId == profileId).ToListAsync();
        }

        public async Task AddOrUpdateProfileFailedInfoesAsync(string googleOrganizationId, params RMDiscoveryGoogleProfileFailedInfo[] failedInfoes)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            efContext.GoogleProfileFailedInfoes.AddOrUpdate(failedInfoes);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteProfileFailedInfoesAsync(string googleOrganizationId, Guid profileId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetGoogleSchemaName(googleOrganizationId);
            SecurityUtils.SanitizeSQLSchemaName(schemaName);
            var sql = $"DELETE FROM [{schemaName}].[RMGoogleProfileFailedInfoes] WHERE ProfileId = @ProfileId";
            await context.ExecuteNonQueryAsync(sql, new SqlParameter("@ProfileId", profileId));
        }
    }
}
