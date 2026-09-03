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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery;
using RADiscovery.Query.Parameter;
using RADiscovery.Query.SourceQuerier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query
{
    public class DiscoveryDataQuerier
    {
        public static Task<List<DiscoveryFileTypeDataInfo>> QueryFileTypes(DiscoveryQueryParameter queryParameter)
        {
            var querier = new DiscoveryFileTypeQuerier(queryParameter);
            return querier.QueryInactiveDataInfo();
        }

        public static Task<List<DiscoverySizeRangeDataInfo>> QuerySizeRanges(DiscoveryQueryParameter queryParameter)
        {
            var querier = new DiscoverySizeRangeQuerier(queryParameter);
            return querier.QueryInactiveDataInfo();
        }

        public static Task<DiscoveryTotalDataInfo> QueryTotalDataInfo(DiscoveryQueryParameter queryParameter)
        {
            var querier = new DiscoveryTotalDataQuerier(queryParameter);
            return querier.QueryInactiveDataInfo();
        }

        public static Task<DiscoveryNodeInfo> QueryInactiveNodeData(DiscoveryQueryParameter queryParameter)
        {
            var querier = new DiscoveryNodeQuerier(queryParameter);
            return querier.QueryInactiveDataInfo();
        }

        public static async Task<List<RMDiscoveryFileType>> GetDiscoveryFileTypes(Guid O365TenantId)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var schemaName = RMDiscoveryDBManager.GetSchemaName(O365TenantId);
            var sql = @$"SELECT [Id],[Name] FROM {schemaName}.[RMFileTypes] WHERE [IsRemoved] = 0";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            var fileTypes = dataCollection.ToList<RMDiscoveryFileType>();
            return fileTypes;
        }
    }
}
