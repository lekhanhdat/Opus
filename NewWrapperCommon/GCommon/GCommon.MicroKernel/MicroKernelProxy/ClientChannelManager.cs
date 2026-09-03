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

    #endregion using directives

    #region Attribute

    /// <summary>
    /// This class is to manage the WCF Client channel, as a performance way,
    /// We keep an open valid channel to the service. The kept opened channel
    /// is the last endpoint channel.  As a synchronization way, I  make
    /// the GetClientChannel method as synchronization method at first, but at some
    /// scenario it may lead to some problems. So at last, I changed the sync
    /// logic to invoke method. up to now , There is no better way i could find.
    /// </summary>
    [DebuggerNonUserCode]

    #endregion Attribute

    internal static class ClientChannelManager
    {
        private static readonly IMicroKernelTraceSource traceSource = new MicroKernelTraceSource();
        private static readonly ConcurrentDic<EndpointInfo, ClientChannelWrapper> cacheChannels = new ConcurrentDic<EndpointInfo, ClientChannelWrapper>();
        private static readonly Object syncRoot = new Object();

        private static readonly TimeSpan CHECKER_EXECUTION_CYCLE;
        private static readonly TimeSpan CHANNEL_EXPIRE_TIME;
        private static readonly int MAX_ENDPOINTS;

        static ClientChannelManager()
        {
            try
            {
                CHECKER_EXECUTION_CYCLE = TimeSpan.FromMinutes(5);
                CHANNEL_EXPIRE_TIME = TimeSpan.FromMinutes(120);
                MAX_ENDPOINTS = 50;

                var clientChannelMaintenanceThread = new Thread(CachedChannelMaintenance) { IsBackground = true, Name = MicroKernelConstant.ClientChannelMaintenanceThreadIdentifier };
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

            var wrapper = GetOrCreateWrapper(targetEndpoint);
            return wrapper.Channel;
        }

        #region wrapper
        private static ClientChannelWrapper GetOrCreateWrapper(EndpointInfo targetEndpoint)
        {
            var cacheEndpoint = GetCachedEndpoint(targetEndpoint);
            if (null == cacheEndpoint)
            {
                cacheEndpoint = CreateNewEndpoint(targetEndpoint);
            }
            ClientChannelWrapper wrapper = null;
            try
            {
                wrapper = cacheChannels[cacheEndpoint];
            }
            catch (KeyNotFoundException)
            {
                cacheEndpoint = CreateNewEndpoint(targetEndpoint);
                wrapper = cacheChannels[cacheEndpoint];
            }
            wrapper.LastAccessTime = DateTime.Now;
            return wrapper;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static EndpointInfo CreateNewEndpoint(EndpointInfo targetEndpoint)
        {
            var cacheEndpoint = GetCachedEndpoint(targetEndpoint);
            if (null == cacheEndpoint)
            {
                var wrapper = new ClientChannelWrapper(targetEndpoint);
                cacheChannels.Add(targetEndpoint, wrapper);
                return targetEndpoint;
            }
            return cacheEndpoint;
        }

        private static EndpointInfo GetCachedEndpoint(EndpointInfo targetEndpoint)
        {
            return cacheChannels.FindKey(endpoint => endpoint.Equals(targetEndpoint));
        }
        #endregion

        #region maintenance
        private static void CachedChannelMaintenance()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(CHECKER_EXECUTION_CYCLE);
                    CheckCachedChannels();
                }
                catch (Exception e)
                {
                    traceSource.TraceError("An error occurred in maintenance thread. {0}", e.ToString());
                }
            }
        }

        private static void CheckCachedChannels()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    ClearInactiveEndpoints();
                    ClearUnavailableChannels();
                }
                finally
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        private static void ClearInactiveEndpoints()
        {
            if (cacheChannels.Count < MAX_ENDPOINTS)
            {
                return;
            }
            var caches = cacheChannels.TakeOut(cache => cache.Value.LastAccessTime.Add(CHANNEL_EXPIRE_TIME) < DateTime.Now);
            foreach (var cache in caches)
            {
                cache.Value.Dispose();
            }
        }

        private static void ClearUnavailableChannels()
        {
            var keys = cacheChannels.KeyList();
            foreach (var key in keys)
            {
                var wrapper = cacheChannels[key];
                wrapper.ClearUnvailableChannel();
            }
        }
        #endregion

    }
}