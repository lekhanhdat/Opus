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
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class ClientChannelWrapper
    {
        private static readonly IMicroKernelTraceSource traceSource = new MicroKernelTraceSource();
        private EndpointInfo endpoint;
        private ICoreServiceClientChannel channel = null;

        private object syncObject = new object();
        private static readonly TimeSpan CHANNEL_CREATION_TIMEOUT = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan CHANNEL_CLEAR_TIMEOUT = TimeSpan.FromSeconds(10);

        internal ClientChannelWrapper(EndpointInfo endpoint)
        {
            this.endpoint = endpoint;
        }

        public DateTime LastAccessTime { get; set; }

        public EndpointInfo Endpoint
        {
            get { return this.endpoint; }
        }

        public ICoreServiceClientChannel Channel
        {
            get
            {
                var channel = this.channel;
                this.LastAccessTime = DateTime.Now;
                if (!IsAvailableChannel(channel))
                {
                    channel = this.CreateNewChannel();
                }
                return channel;
            }
        }

        private ICoreServiceClientChannel CreateNewChannel()
        {
            if (Monitor.TryEnter(syncObject, CHANNEL_CREATION_TIMEOUT))
            {
                try
                {
                    if (null != this.channel)
                    {
                        if (!IsAvailableChannel(this.channel))
                        {
                            this.SafelyReleaseChannel();
                        }
                    }
                    if (null == this.channel)
                    {
                        this.channel = SafelyCreateChannel();
                    }
                    return this.channel;
                }
                finally
                {
                    Monitor.Exit(syncObject);
                }
            }
            else
            {
                throw new MicroKernelChannelCreationException(this.endpoint.ToString());
            }
        }

        private bool IsAvailableChannel(ICoreServiceClientChannel channel)
        {
            if (null != channel && channel.State < CommunicationState.Closing)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            this.SafelyReleaseChannel();
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void ClearUnvailableChannel()
        {
            if (Monitor.TryEnter(syncObject, CHANNEL_CLEAR_TIMEOUT))
            {
                try
                {
                    if (null != this.channel && !IsAvailableChannel(this.channel))
                    {
                        this.SafelyReleaseChannel();
                    }
                }
                finally
                {
                    Monitor.Exit(syncObject);
                }
            }
        }

        private ICoreServiceClientChannel SafelyCreateChannel()
        {
            var coreServiceChannelProvider = new CoreChannelProvider();
            var clientChannel = coreServiceChannelProvider.CreateChannel<ICoreServiceClientChannel>(this.endpoint);
            clientChannel.Open();
            return clientChannel;
        }

        private void SafelyReleaseChannel()
        {
            if (null != this.channel)
            {
                try
                {
                    this.channel.Close();
                }
                catch (Exception e)
                {
                    traceSource.TraceError("An error occurred while closing channel. {0}", e.ToString());
                    if (null != this.channel)
                    {
                        this.channel.Abort();
                    }
                }
                finally
                {
                    this.channel = null;
                }
            }
        }

    }
    
}
