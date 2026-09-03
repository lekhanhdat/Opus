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
using AvePoint.GCommon;
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.Contract;
using System;
using System.Reflection;

namespace AvePoint.Hybrid.AgentService.ServiceEndpoint
{

    public interface IEPBrowserService
    {
        BrowserResult ListNode(TreeBrowserArgs msg);
        BrowserResult Validate(TreeBrowserArgs msg);
    }


    public class EPBrowserService : IEPBrowserService
    {
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));
        public BrowserResult ListNode(TreeBrowserArgs msg)
        {
            BrowserResult result = new BrowserResult();

            try
            {
                string request = SerializerHelper.SerializeByJsonSerializer(msg);
                string temp = null;
                retryPolicy.ExecuteAction(() => temp = HybridBrowserUtil.Instance.Browse(HybridBrowserType.FileSystem, request));
                if(temp !=null)
                {
                    logger.Info("Message sent to browser process and get response.--" + result.Result);
                    result = SerializerHelper.DeserializeByJsonConvert<BrowserResult>(temp);
                }
                else
                {
                    logger.Info("Message sent to browser process and get response is null .");
                    throw new Exception("Browser fail, no result return.");
                }
                
            }
            catch (Exception e)
            {
                logger.Error("[SERVER] Error: {0}", e.Message);
                result.Result = BrowserResultEnum.Failed;
                result.Message = e.Message;
            }

            return result;
        }

        public BrowserResult Validate(TreeBrowserArgs msg)
        {
            BrowserResult result = new BrowserResult();

            try
            {
                string request = SerializerHelper.SerializeByJsonSerializer(msg);
                retryPolicy.ExecuteAction(() => HybridBrowserUtil.Instance.Browse(HybridBrowserType.FileSystem, request));
                result.Result = BrowserResultEnum.Succeed;
                logger.Info("Message sent to browser process and get response.--" + result.Result);
            }
            catch (Exception e)
            {
                logger.Error("[SERVER] Error: {0}", e.Message);
                result.Result = BrowserResultEnum.Failed;
                result.Message = e.Message;
            }

            return result;
        }

    }
}
