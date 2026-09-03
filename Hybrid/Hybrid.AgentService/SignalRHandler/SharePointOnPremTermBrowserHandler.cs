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
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.Contract;
using HybirdProxy.EndpointHandler;
using System;

namespace AvePoint.Hybrid.AgentService.SignalRHandler
{
    public class SharePointOnPremTermBrowserHandler : EndpointHandlerBase<SharePointOnPremTermBrowserExecute, SharePointOnPremTermBrowserArgs, SharePointOnPremTermBrowserResult>
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremTermBrowserHandler));

        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));

        public override SharePointOnPremTermBrowserResult Process(SharePointOnPremTermBrowserArgs param)
        {
            Logger.Info($"Receive message....");
            try
            {
                var request = SerializerHelper.SerializeByJsonSerializer(param);
                var response = RetryPolicy.ExecuteAction(() => HybridBrowserUtil.Instance.Browse(HybridBrowserType.SharePointOnPremTerm, request));
                var result = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremTermBrowserResult>(response);
                Logger.Info($"Message send to browser term process and get response. --[{result.Result}]");
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"[SERVER] Error: {e}");
                return new SharePointOnPremTermBrowserResult
                {
                    Result = SharePointOnPremTermBrowserResultEnum.Failed,
                    Message = e.Message
                };
            }
        }
    }
}
