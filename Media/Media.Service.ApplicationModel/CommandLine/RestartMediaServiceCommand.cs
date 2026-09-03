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




namespace AvePoint.Media.Service.Command
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.ServiceProcess;
    using AvePoint.GCommon;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaService;
    using Merged18NResources.MediaServiceApplicationModel;
    #endregion

    /// <summary>
    /// Restart Media service, implement the service here because we can use the 
    /// MediaServiceCommand.exe to do additional work
    /// </summary>
    internal class RestartMediaServiceCommand 
        : CommandBase
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        String errorMessage;

        public override string CommandName
        {
            get { return "RestartMediaService"; }
        }

        public override string HelpMessage
        {
            get { return "The syntax of this command is: -RestartMediaService"; }
        }

        public override string SucceedMessage
        {
            get { return "Restart  media service successfully."; }
        }

        public override string ErrorMessage { get { return this.errorMessage; } }

        protected override Boolean ExecuteCommand(List<string> args)
        {
            var result = default(Boolean);
            try
            {
                using (var service = ServiceController.GetServices().FirstOrDefault(item => item.ServiceName.Equals(ServiceConstants.ServiceName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (service != null && service.CanStop)
                    {
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandBeginStopMediaService);
                        service.Stop();
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandWaitForMediaServiceStop);
                        service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandRefreshMediaServiceStopStatus);
                        service.Refresh();
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandBeginStartMediaService);
                        service.Start();
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandWaitForMediaServiceStart);
                        service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandRefreshMediaServiceStartStatus);
                        service.Refresh();
                        this.logger.Info(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandRestartMediaSucceed);
                        result = 1 < 2;
                    }
                }
            }
            catch (Exception e)
            {
                this.errorMessage = string.Format(MediaServiceApplicationModelResource.RestartMediaServiceCommandExecuteCommandRestartMediaError, e);
                this.logger.Error(this.errorMessage);
            }

            return result;
        }

        protected override bool CheckParameters(List<String> args)
        {
            return args.Count == 0;
        }
    }
}
