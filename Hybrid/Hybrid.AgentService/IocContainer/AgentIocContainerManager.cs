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



namespace AvePoint.Hybrid.AgentService
{
    #region using directives
    using AvePoint.GCommon.Utility;
    using Castle.Windsor;
    using System;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Hybrid.Utility;
    using AvePoint.Hybrid.Contract;
    using CommonModel.MethodInfo;
    #endregion

    /// <summary>
    /// Main class of agent service to handle the top level service 
    /// </summary>
    public sealed class AgentIocContainerManager : AvePoint.Hybrid.Utility.IIocContainerManager
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        Boolean isDisposed;
        Boolean isContainerLoaded;

        WindsorContainer windsorContainer;
        AvePoint.Hybrid.Utility.IStartable processHostingService;
        AvePoint.Hybrid.Utility.IStartable threadHostingService;
        AvePoint.Hybrid.Utility.IStartable postHostingService;
        AvePoint.Hybrid.Utility.IStartable signalRListenerService;
        AvePoint.Hybrid.Utility.IStartable agentLogCollectorHostingService;

        public void LoadContainer()
        {
            if (!this.isDisposed)
            {
                if (!this.isContainerLoaded)
                {
                    this.windsorContainer = new WindsorContainer();
                    var componentsLoader = new AgentIocComponentManager(this.windsorContainer);
                    componentsLoader.LoadComponents();

                    WindsorManager.SetUp(windsorContainer);

                    this.processHostingService = (AvePoint.Hybrid.Utility.IStartable)WindsorManager.GetService("AvePoint.Hybrid.AgentService.AgentProcessHostingService", typeof(AvePoint.Hybrid.Utility.IStartable));
                    this.processHostingService.Start();

                    this.threadHostingService = (AvePoint.Hybrid.Utility.IStartable)WindsorManager.GetService("AvePoint.Hybrid.AgentService.AgentThreadHostingService", typeof(AvePoint.Hybrid.Utility.IStartable));
                    this.threadHostingService.Start();

                    this.postHostingService = (AvePoint.Hybrid.Utility.IStartable)WindsorManager.GetService("AvePoint.Hybrid.AgentService.AgentPostHostingService", typeof(AvePoint.Hybrid.Utility.IStartable));
                    this.postHostingService.Start();

                    this.signalRListenerService = (AvePoint.Hybrid.Utility.IStartable)WindsorManager.GetService("AvePoint.Hybrid.AgentService.SignalRListenerService", typeof(AvePoint.Hybrid.Utility.IStartable));
                    this.signalRListenerService.Start();

                    this.agentLogCollectorHostingService = (AvePoint.Hybrid.Utility.IStartable)WindsorManager.GetService("AvePoint.Hybrid.AgentService.AgentLogCollectorHostingService", typeof(AvePoint.Hybrid.Utility.IStartable));
                    this.agentLogCollectorHostingService.Start();

                    isContainerLoaded = true;
                }
                else { /*HACK: need to confirm the container's status */ }
            }
            else throw new ObjectDisposedException(typeof(AgentIocContainerManager).FullName);
        }

        public void UnloadContainer()
        {
            if (!this.isDisposed)
            {
                if (this.isContainerLoaded)
                {
                    this.postHostingService.Stop();
                    this.threadHostingService.Stop();
                    this.processHostingService.Stop();
                    this.signalRListenerService.Stop();
                    this.agentLogCollectorHostingService.Stop();
                    this.windsorContainer.Dispose();
                    this.isContainerLoaded = false;
                }
                else { /*HACK: need to confirm the container's status */ }
            }
            else throw new ObjectDisposedException(typeof(AgentIocContainerManager).FullName);
        }

        public void Dispose()
        {
            this.UnloadContainer();
            this.isDisposed = true;
        }
    }
}
