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
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Threading;
using AvePoint.GCommon.Contract.AgentService.Object;
using AvePoint.GCommon.Utility;
using System.Configuration;

namespace AvePoint.GCommon//CommonUtility20
{
    /// <summary>
    /// use WMI and other way to get load balance information
    /// </summary>
    public static class AveLoadBalanceInfoUtility
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AveLoadBalanceInfoUtility));

        static bool canInitLoadBalance = true;
        static AveLoadBalanceInfo loadBalanceInfo = new AveLoadBalanceInfo();

        /// <summary>
        /// Get load balance information according the IPAddress
        /// </summary>
        /// <param name="hostOrIPAddress">agent address or media address</param>
        /// <returns></returns>
        public static AveLoadBalanceInfo GetLoadBalanceInfo(string hostOrIPAddress)
        {
            return GetLoadBalanceInfoUsingAnotherThread(hostOrIPAddress);
        }

        static AveLoadBalanceInfo GetLoadBalanceInfoUsingAnotherThread(string hostOrIPAddress)
        {
            string skipLoadBalance = ConfigurationManager.AppSettings["SkipLoadBalance"];
            if (string.IsNullOrEmpty(skipLoadBalance))
            {
                try
                {
                    Thread getLoadBalanceInfoThread = new Thread(new ParameterizedThreadStart(GetLoadBalanceInfoThread));
                    getLoadBalanceInfoThread.Name = "Get LoadBalance Info Thread";
                    getLoadBalanceInfoThread.IsBackground = true;
                    getLoadBalanceInfoThread.Start(hostOrIPAddress);

                    if (!getLoadBalanceInfoThread.Join(60000))
                    {
                        canInitLoadBalance = false;
                        try
                        {
                            getLoadBalanceInfoThread.Suspend();// output the stack trace and kill this thread because it hangs in WMI.
                            StackTrace stackTrace = new StackTrace(getLoadBalanceInfoThread, false);
                            logger.Warn("The thread of GetLoadBalanceInformation is still running at:" + stackTrace.ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Suspend the GetLoadBalanceInformation failed:" + ex.ToString());
                        }
                        finally
                        {
                            try
                            {
                                getLoadBalanceInfoThread.Resume();
                            }
                            catch (Exception e)
                            {
                                logger.Warn("Resume exception:{0}", e.ToString());
                            }
                            try
                            {
                                getLoadBalanceInfoThread.Abort();// need to abort because the thread of GetLoadBalanceInformation maybe hang...
                            }
                            catch (Exception e)
                            {
                                logger.Warn("Abort exception:{0}", e.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    canInitLoadBalance = false;
                    logger.Warn("GetLoadBalanceInformationByThread failed:" + ex.ToString());
                }
            }
            else
            {
                GetDefaultLoadBalanceInfo();
            }
            return loadBalanceInfo;
        }

        /// <summary>
        /// get default loadbalance information for specail environment case, and using configuration file.
        /// </summary>
        static void GetDefaultLoadBalanceInfo()
        {
            loadBalanceInfo.WindowsCPUHz = 2000;
            loadBalanceInfo.CPUUsage = 10;
            loadBalanceInfo.NetWorkInterfaceAdapterCaption = null;
            loadBalanceInfo.NetworkBandWidth = 0;
            loadBalanceInfo.NetworkSentSpeed = 0;
            loadBalanceInfo.NetworkReceivedSpeed = 120;

            loadBalanceInfo.TotalVisibleMemorySize = 2137350144;
            loadBalanceInfo.FreePhysicalMemory = 1923615130;
            loadBalanceInfo.MemoryUsage = 10;
            TimeSpan lTime = (TimeSpan)(System.DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0));
            loadBalanceInfo.CurrentTime = (long)lTime.TotalMilliseconds - 28800000;
        }

        static void GetLoadBalanceInfoThread(object hostOrIPAddress)
        {
            try
            {
                loadBalanceInfo.WindowsCPUHz = OSInformation.CPUHz;
                loadBalanceInfo.CPUUsage = OSInformation.CPUUsage;

                AveNetworkInterfaceInformation networkInterfaceInfo = AveNetworkingUtil.GetNetworkInterfaceInformation(hostOrIPAddress.ToString());
                loadBalanceInfo.NetWorkInterfaceAdapterCaption = networkInterfaceInfo.NetWorkInterfaceAdapterCaption;
                loadBalanceInfo.NetworkBandWidth = networkInterfaceInfo.NetworkBandWidth;
                loadBalanceInfo.NetworkSentSpeed = networkInterfaceInfo.NetworkSentSpeed;
                loadBalanceInfo.NetworkReceivedSpeed = networkInterfaceInfo.NetworkReceivedSpeed;

                loadBalanceInfo.TotalVisibleMemorySize = OSInformation.TotalVisibleMemorySize;
                loadBalanceInfo.FreePhysicalMemory = OSInformation.FreePhysicalMemory;
                loadBalanceInfo.MemoryUsage = (int)(((loadBalanceInfo.TotalVisibleMemorySize - loadBalanceInfo.FreePhysicalMemory) * 100) / loadBalanceInfo.TotalVisibleMemorySize);

                TimeSpan lTime = (TimeSpan)(System.DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0));
                loadBalanceInfo.CurrentTime = (long)lTime.TotalMilliseconds - 28800000;
            }
            catch (Exception ex)
            {
                logger.Warn("Get LoadBalance information failed:" + ex.ToString());
            }
        }

    }
}
