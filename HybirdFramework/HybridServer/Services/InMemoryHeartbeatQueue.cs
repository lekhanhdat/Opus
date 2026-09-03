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
using HybridServer.EF.Entity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HybridServer.Services
{

    public interface IInMemoryHeartbeatQueue
    {
        ValueTask EnqueueAsync(Agent heartbeat);
        IAsyncEnumerable<Agent> ReadAllAsync(CancellationToken ct);
    }

    public class InMemoryHeartbeatQueue : IInMemoryHeartbeatQueue
    {
        private readonly Channel<Agent> _channel;
        public ChannelReader<Agent> Reader => _channel.Reader;
        public InMemoryHeartbeatQueue()
        {
            // The maximum number of items the bounded channel may store
            var options = new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest 
            };
            _channel = Channel.CreateBounded<Agent>(options);
        }

        public ValueTask EnqueueAsync(Agent heartbeat)
        {
            return _channel.Writer.WriteAsync(heartbeat);
        }

        public IAsyncEnumerable<Agent> ReadAllAsync(CancellationToken ct)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }

    }
}
