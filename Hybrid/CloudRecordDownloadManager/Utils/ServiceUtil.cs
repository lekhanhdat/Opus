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
using NLog;
using System;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CloudRecordDownloadManager.Utils
{
    public class ServiceUtil
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static bool StopService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if (service.DependentServices.Length == 0)
            {
                TimeSpan timeout = TimeSpan.FromMinutes(3);
                try
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        Log.Info($"start to stop service '{serviceName}'");
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        Log.Info($"service '{serviceName}' stopped");

                        Task.Delay(40000).Wait(); //wait for completly service stopped
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to stop service '{serviceName}', error : {e.ToString()}");
                    return false; ;
                }
            }
            else
            {
                StringBuilder eventLogString = new StringBuilder();
                eventLogString.AppendLine(String.Format("Restart service is {0}, display name is {1}", service.ServiceName, service.DisplayName));
                foreach (ServiceController dependentService in service.DependentServices)
                {
                    eventLogString.AppendLine(String.Format("Dependent services contain {0}, display name is {1}", dependentService.ServiceName, dependentService.DisplayName));
                }
                Log.Warn("We need to restart service and its dependent service manually:{0}", eventLogString.ToString());
                return false;
            }

            return true;
        }

        public static void StartService(string serviceName)
        {
            try
            {
                ServiceController service = new ServiceController(serviceName);
                TimeSpan timeout = TimeSpan.FromMinutes(2);
                Log.Info($"Begin to start service '{serviceName}'");
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Log.Info($"Start service '{serviceName}' successfully");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to start service '{serviceName}', error : {e.ToString()}");
            }
        }
    }
}
