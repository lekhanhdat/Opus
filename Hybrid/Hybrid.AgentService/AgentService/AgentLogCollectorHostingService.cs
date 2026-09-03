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
using AvePoint.Hybrid.AgentService.LogCollector;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService
{
    public class AgentLogCollectorHostingService : IStartable
    {
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AgentLogCollectTask logCollectTask;
        public AgentLogCollectorHostingService()
        {
            var checkpointStore = new CollectorCheckPoint(System.IO.Path.Combine(AveEnv.AgentLogFolder, "Checkpoints"));
            var collectors = new List<IAgentLogCollector>
            {
                new AgentServiceLogCollector(checkpointStore),
                new AgentBrowserLogCollector(checkpointStore),
            };
            logCollectTask = new AgentLogCollectTask(collectors);
        }

        public event EventHandler OnStarting;
        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
        public event EventHandler OnStopped;

        public void Start()
        {
            logCollectTask.Start();
            logger.Info("Starting agent log collector hosting service.");
        }

        public void Stop()
        {
            logger.Info("Stopping agent log collector hosting service.");
            this.logCollectTask.Stop();;
        }

    }
}
