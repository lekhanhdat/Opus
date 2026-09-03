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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.Hybrid.Utility;
    using AvePoint.RA.CommonUtil;
    using SD = System.Diagnostics;
    #endregion

    public class AgentPreHostingService : IStartable
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        List<String> processesNeedStopWhenRestarted = new List<String>
        { 
            //add process name here you want to be stopped when restarting server.

        };

        public event EventHandler OnStarting;
        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
        public event EventHandler OnStopped;

        public void Start()
        {
            this.StopProcesses();
            this.ValidateCertificate();
        }

        public void Stop()
        {
            this.StopProcesses();
        }

        void StopProcesses()
        {
            var stoppingServiceFile = Path.Combine(AveEnv.AgentDataFolder, "StoppingService.AVE");
            if (File.Exists(stoppingServiceFile))
            { File.Delete(stoppingServiceFile); }

            var isProcessRunning = this.processesNeedStopWhenRestarted.Exists(processName => SD.Process.GetProcessesByName(processName).Length > 0);
            if (isProcessRunning)
            {
                File.Create(stoppingServiceFile).Close();
                Thread.Sleep(15000);
                File.Delete(stoppingServiceFile);
                this.processesNeedStopWhenRestarted.ForEach(processName =>
                {
                    try
                    {
                        Array.ForEach<SD.Process>(SD.Process.GetProcessesByName(processName), process => process.Kill());
                    }
                    catch (Exception ex)
                    {
                        //this.logger.Error("An error occurred while killing process: {0} Exception: {1}", pName, ex.ToString());
                        //this.logger.Error(AgentCommonResources.PreHostingStopProcessesErrorOccurredWhileKillingProcess, processName, ex.ToString());
                    }
                });
            }
        }

        void ValidateCertificate()
        {
            try
            {
                logger.Info("Validate certificate.");
                //CertificateManagementUtil.CertificateCanDoKeyExchange(AveEnv.AgentWcfThumbprint);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while validating certificate. {0}", e.ToString());
            }
        }
    }
}
