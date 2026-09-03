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

namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.Utility;

    using Microsoft.Exchange.WebServices.Data;

    using AvePoint.RA.CommonUtil;

    using System;
    using AvePoint.Common;

    internal class EWSTraceListener : ITraceListener, ISingleton
    {
        private EWSTraceListener()
        {
        }

        private static RALogger logger = RALogger.GetInstance(typeof(EWSTraceListener));

        public void Trace(string traceType, string traceMessage)
        {
            try
            {
                if (traceType.Equals("EwsRequestHttpHeaders") || traceType.Equals("EwsRequestHttpHeadersException"))
                {
                    if (traceMessage.Contains("Authorization") && traceMessage.Contains("</Trace>"))
                    {
                        var headerWithoutToken = traceMessage.Substring(0, traceMessage.LastIndexOf("Authorization"));
                        traceMessage = headerWithoutToken + "</Trace>";
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failure of EWSTrace.error message : {e.Message}");
            }
            logger.Info("EWSTrace: {0}: {1}", traceType, traceMessage);
        }

        public static EWSTraceListener Instance
        {
            get
            {
                return Singleton<EWSTraceListener>.SingletonInstance;
            }
        }
    }

    internal static class EWSTraceUtil
    {
        public static void EnableTraceLog(this ExchangeService self)
        {
            self.TraceEnabled = true;
            self.TraceFlags = TraceFlags.EwsRequestException | TraceFlags.EwsRequestHttpHeadersException | TraceFlags.EwsResponseException | TraceFlags.EwsResponseHttpHeadersException | TraceFlags.EwsTimeZones;
            self.TraceListener = EWSTraceListener.Instance;
        }
    }
}