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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.SignalRHandler
{
    public class FileSystemJobHandler : EndpointHandlerBase<SFileSystemJobExecute, RecordsJobArgs, FileSystemJobResult>
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public override FileSystemJobResult Process(RecordsJobArgs args)
        {
            FileSystemJobResult result = new FileSystemJobResult();
            try
            {
                var service = (IEPJobService)WindsorManager.GetService("AvePoint.Hybrid.AgentService.ServiceEndpoint.EPJobService", typeof(IEPJobService));
                service.StartJob(args);
                logger.Info("Start job successfully. Jobid: " + args?.JobId);
                result.Result = FileSystemResultEnum.Succeed;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while starting job. JobId: {args?.JobId} Error: {e.ToString()}");
                result.Result = FileSystemResultEnum.Failed;
                result.Message = e.Message;
            }
            return result;
        }
    }
}
