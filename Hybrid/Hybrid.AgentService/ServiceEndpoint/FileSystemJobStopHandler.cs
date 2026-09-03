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
using AvePoint.Hybrid.AgentService.ServiceEndpoint;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.CommonUtil;
using HybirdProxy.EndpointHandler;
using System;
using System.Reflection;

namespace AvePoint.Hybrid.AgentService.SignalRHandler
{
    public class FileSystemJobStopHandler : EndpointHandlerBase<SRecordsJobStop>
    {
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void Process(SRecordsJobStop param)
        {
            try
            {
                var args = param?.MethodArgs;
                if (args == null || string.IsNullOrWhiteSpace(args.JobId))
                {
                    logger.Warn("Received jobStop with null or empty JobId. Ignoring.");
                    return;
                }

                var service = (IEPJobService)WindsorManager.GetService(
                    "AvePoint.Hybrid.AgentService.ServiceEndpoint.EPJobService",
                    typeof(IEPJobService));

                service.StopJob(args.JobId);
                logger.Info("Stop signal forwarded successfully. JobId: {0}", args.JobId);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while forwarding stop signal. JobId: {0} Error: {1}",
                    param?.MethodArgs?.JobId, e.ToString());
            }
        }
    }
}