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
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleDataQueryDao : IRMDiscoveryGoogleDataQueryDao
    {
        public async Task<List<Dictionary<string, object>>> GetDataDictionaryListAsync(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            return dataCollection.ToDictionary();
        }

        public async Task<List<T>> GetDataListAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            return dataCollection.ToList<T>();
        }

        public async Task<T> GetDataAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            return dataCollection.ToList<T>().FirstOrDefault();
        }

        public async Task<List<RMDiscoveryGoogleAggregateTotalData>> GetAggregateTotalDataListAsync(string googleOrganizationId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleAggregateTotalDataList.ToListAsync();
        }

        public async Task<List<string>> GetSettingInfoAsync(Guid settingId, string _schemaName)
        {
            throw new NotImplementedException();
        }
    }
}
