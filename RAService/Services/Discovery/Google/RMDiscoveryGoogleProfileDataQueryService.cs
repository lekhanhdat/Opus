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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Inactive;
using AvePoint.RA.Service.Services.Discovery.Google.Query.Profile.Rot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AvePoint.RA.Service.Services.Discovery.Google
{
    public class RMDiscoveryGoogleProfileDataQueryService : IRMDiscoveryGoogleProfileDataQueryService
    {
        private readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleProfileDataQueryService));

        public async Task<Dictionary<string, object>> QueryInactiveAggregateInfo(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveProfileAggregateStatisticDataQuerier(queryParameter);
                return await querier.QueryAsync();
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query google profile aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveProfileNodeDataQuerier(queryParameter);
                var dataInfo = await querier.QueryAsync();

                return dataInfo;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query google profile optimization nodes of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleInactiveProfileNodeTotalAggregateDataQuerier(queryParameter);
                var res = await querier.QueryAsync();

                return res;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query google profile optimization nodes total aggregate info of inactive ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryRotOptimizationNodesAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotProfileNodeDataQuerier(queryParameter);
                var dataInfo = await querier.QueryAsync();

                return dataInfo;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query google profile optimization nodes of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }

        public async Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            try
            {
                var querier = new RMDiscoveryGoogleRotProfileNodeTotalAggregateDataQuerier(queryParameter);
                var res = await querier.QueryAsync();

                return res;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while query google profile optimization nodes total aggregate info of rot ({queryParameter.ToJsonInfo()}). Error: {e}");
                return new();
            }
        }
    }
}
