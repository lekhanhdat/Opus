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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

    /// <summary>
    /// This class is to manage the WCF Client channel, as a performance way,
    /// We keep an open valid channel to the service. The kept opened channel
    /// is the last endpoint channel.  As a synchronization way, I  make
    /// the GetClientChannel method as synchronization method at first, but at some
    /// scenario it may lead to some problems. So at last, I changed the sync
    /// logic to invoke method. up to now , There is no better way i could find.
    /// </summary>
    internal static class ClientChannelManager
    {
        static IMicroKernelTraceSource traceSource = new MicroKernelTraceSource();
        static Dictionary<EndpointInfo, ClientChannelWrapper> cacheChannels = new Dictionary<EndpointInfo, ClientChannelWrapper>();

        static ClientChannelManager()
        {
            try
            {
                var clientChannelMaintenanceThread = new Thread(CachedChannelMaintenance);
                clientChannelMaintenanceThread.IsBackground = true;
                clientChannelMaintenanceThread.Name = MicroKernelConstant.ClientChannelMaintenanceThreadIdentifier;
                clientChannelMaintenanceThread.Start();
            }
            catch (Exception e)
            {
                traceSource.TraceError("An error occurred while starting cache maintenance thread. {0}", e.ToString());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="targetEndpoint">the endpoint of the caller</param>
        /// <returns>a WCF internal channel of the core service</returns>
        public static ICoreServiceClientChannel GetClientChannel(EndpointInfo targetEndpoint)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return 1 < 2; };

            var resultClientChannel = GetChannelFromCache(targetEndpoint);
            if (resultClientChannel == null)
            {
                resultClientChannel = SafelyCreateChannel(targetEndpoint);
                PutChannelToCache(targetEndpoint, resultClientChannel);
            }
            return resultClientChannel;
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        static ICoreServiceClientChannel GetChannelFromCache(EndpointInfo targetEndpoint)
        {
            var resultClientChannel = default(ICoreServiceClientChannel);
            var cacheEndpoint = default(EndpointInfo);
            foreach (var endpoint in cacheChannels.Keys)
            {
                if (endpoint.Equals(targetEndpoint))
                {
                    cacheEndpoint = endpoint;
                    break;
                }
            }
            if (cacheEndpoint != null)
            {
                var brokenChannels = new List<ICoreServiceClientChannel>();
                foreach (var cacheChannel in cacheChannels[cacheEndpoint].Channels)
                {
                    try
                    {
                        cacheChannel.IsServiceRunning();
                        resultClientChannel = cacheChannel;
                        cacheChannels[cacheEndpoint].LastAccessTime = DateTime.Now;
                        break;
                    }
                    catch (Exception e)
                    {
                        traceSource.TraceError("An error occurred while calling IsServiceRunning. {0}", e.ToString());
                        brokenChannels.Add(cacheChannel);
                    }
                }
                foreach (var brokenChannel in brokenChannels)
                {
                    SafelyReleaseChannel(brokenChannel);
                    cacheChannels[cacheEndpoint].Channels.Remove(brokenChannel);
                }
            }
            return resultClientChannel;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void PutChannelToCache(EndpointInfo targetEndpoint, ICoreServiceClientChannel channel)
        {
            EndpointInfo cacheEndpoint = null;
            foreach (EndpointInfo endpoint in cacheChannels.Keys)
            {
                if (endpoint.Equals(targetEndpoint))
                {
                    cacheEndpoint = endpoint;
                    break;
                }
            }
            if (cacheEndpoint == null)
            {
                cacheChannels.Add(targetEndpoint, new ClientChannelWrapper());
            }
            cacheChannels[targetEndpoint].Channels.Add(channel);
            cacheChannels[targetEndpoint].LastAccessTime = DateTime.Now;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void CheckCachedChannels()
        {
            foreach (var endPoint in cacheChannels.Keys)
            {
                var brokenChannels = new List<ICoreServiceClientChannel>();
                foreach (var channel in cacheChannels[endPoint].Channels)
                {
                    try
                    {
                        channel.IsServiceRunning();
                    }
                    catch (Exception e)
                    {
                        traceSource.TraceError("An error occurred while calling IsServiceRunning. {0}", e.ToString());
                        brokenChannels.Add(channel);
                    }
                }
                foreach (var brokenChannel in brokenChannels)
                {
                    SafelyReleaseChannel(brokenChannel);
                    cacheChannels[endPoint].Channels.Remove(brokenChannel);
                }
            }
            if (cacheChannels.Keys.Count < 50) return;

            var removeEndpoints = new List<EndpointInfo>();
            foreach (var endPoint in cacheChannels.Keys)
            {
                //we should give enough time for operation execution, 2 hours for one operation in case
                if (cacheChannels[endPoint].LastAccessTime < DateTime.Now.AddMinutes(-120))
                {
                    removeEndpoints.Add(endPoint);
                }
            }
            foreach (var removeEndpoint in removeEndpoints)
            {
                foreach (var channel in cacheChannels[removeEndpoint].Channels)
                {
                    SafelyReleaseChannel(channel);
                }
                cacheChannels.Remove(removeEndpoint);
            }
        }

        static void CachedChannelMaintenance()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(5 * 60 * 1000);
                    CheckCachedChannels();
                }
            }
            catch (Exception e)
            {
                traceSource.TraceError("An error occurred in maintenance thread. {0}", e.ToString());
            }
        }

        static ICoreServiceClientChannel SafelyCreateChannel(EndpointInfo endpoint)
        {
            var coreServiceChannelProvider = new CoreChannelProvider();
            var clientChannel = coreServiceChannelProvider.CreateChannel<ICoreServiceClientChannel>(endpoint);
            clientChannel.Open();
            return clientChannel;
        }

        static void SafelyReleaseChannel(ICoreServiceClientChannel cacheChannel)
        {
            try
            {
                cacheChannel.Close();
            }
            catch (Exception e)
            {
                traceSource.TraceError("An error occurred while closing channel. {0}", e.ToString());
                cacheChannel.Abort();
            }
        }
    }
}