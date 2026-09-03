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

using System;
using System.IO;
using System.Threading;
using AvePoint.RA.CommonUtil;
using System.Reflection;
using System.IO.Pipes;
using log4net.Config;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Hybrid.Browser.Util;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.GCommon;

namespace AvePoint.RA.Hybrid.Browser
{
    public class Program
    {
        private static DateTime LastRunTime = DateTime.UtcNow;

        private static readonly TimeSpan ExpireTime = TimeSpan.FromHours(2);

        private static readonly TimeSpan SleepTime = TimeSpan.FromMinutes(1);

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            try
            {
                Logger.Info($"Begin initial hybrid browser process.");
                ServiceInitializationUtil.InitServicePoint();

                CommonConfiguration.InitAppSetting();
                HybridBrowseCommunicator.RegisteBrowseCallBack(() => LastRunTime = DateTime.UtcNow);
                HybridBrowseCommunicator.Init();
                KeepRun();
                Logger.Info("Initial hybrid browser process successful.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occur while initial browser process. Error: {e}");
            }
        }


        private static void KeepRun()
        {
            while(LastRunTime + ExpireTime > DateTime.UtcNow)
            {
                Logger.Info("Keep running....");
                Thread.Sleep(SleepTime);
            }
            HybridBrowseCommunicator.Dispose();
            Logger.Info($"Exit because no request was made within {ExpireTime} minutes.");
        }
    }
}
