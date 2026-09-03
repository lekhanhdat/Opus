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
//namespace AvePoint.RA.Common.Monitor;

//using AvePoint.RA.CommonUtil;
//using System.Diagnostics.Tracing;

//public class NetworkEventListener : EventListener
//{
//    private static readonly RALogger _logger = RALogger.GetInstance(typeof(NetworkEventListener));

//    protected override void OnEventSourceCreated(EventSource eventSource)
//    {
//        if (eventSource.Name.Contains("Net") ||
//            eventSource.Name.Contains("Http") ||
//            eventSource.Name.Contains("Security") ||
//            eventSource.Name.Contains("Tls") ||
//            eventSource.Name.Contains("Ssl") ||
//            eventSource.Name.Contains("DotNETRuntime"))
//        {
//            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
//        }
//        else if (eventSource.Name.Contains("Azure"))
//        {
//            EnableEvents(eventSource, EventLevel.Informational, EventKeywords.All);
//        }
//    }

//    protected override void OnEventWritten(EventWrittenEventArgs eventData)
//    {
//        string prefix = $"[{eventData.EventSource.Name}] Event: {eventData.EventName}.";

//        if (eventData.Payload != null)
//        {
//            for (int i = 0; i < eventData.Payload.Count; i++)
//            {
//                string payload = eventData.Payload[i]?.ToString() ?? "null";
//                if (payload.Contains("Tls"))
//                {
//                    _logger.Debug($"{prefix} Payload[{i}]: {payload}");
//                }
//            }
//        }
//    }
//}
