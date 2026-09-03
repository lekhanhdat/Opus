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
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.Hybrid.Browser.Contract;
using HybirdProxy.EndpointHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.SignalRHandler
{
    public class SharePointOnPremQuererHandler : EndpointHandlerBase<SharePointOnPremQuererExecute, SharePointOnPremQuererArgs, SharePointOnPremQuererResult>
    {
        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremQuererHandler));

        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));

        public override SharePointOnPremQuererResult Process(SharePointOnPremQuererArgs param)
        {
            Logger.Info($"Receive message....");
            try
            {
                var request = SerializerHelper.SerializeByJsonSerializer(param);
                var response = RetryPolicy.ExecuteAction(() => HybridBrowserUtil.Instance.Browse(HybridBrowserType.SharePointOnPremQuerer, request));
                var result = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremQuererResult>(response);
                Logger.Info($"Message send to querier process and get response.");
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"[SERVER] Error: {e}");
                return new SharePointOnPremQuererResult();
            }
        }
    }
}
