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
using AvePoint.RA.DB.AzureCosmosDB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureCosmosDB.Concurrent
{
    public class RMAzureCosmosDBConcurrentActionBuilder
    {

        private const int DEFAULT_RETRY_TIMES = 3;

        private const int INITIAL_RETRY_DELAY_TIME = 500;

        private const int DEFAULT_MAX_DEGREE_OF_PARALLELISM = 5;

        public int RetryTimes { get; private set; } = DEFAULT_RETRY_TIMES;

        public int InitialRetryDelayTime { get; private set; } = INITIAL_RETRY_DELAY_TIME;

        public int MaxDegreeOfParallelism { get; private set; } = DEFAULT_MAX_DEGREE_OF_PARALLELISM;

        private readonly RMAzureCosmosDBContainer Container;

        private RMAzureCosmosDBConcurrentActionBuilder(RMAzureCosmosDBContainer container) 
        {
            Container = container;
        }

        internal static RMAzureCosmosDBConcurrentActionBuilder CreateBuilder(RMAzureCosmosDBContainer container)
        {
            return new RMAzureCosmosDBConcurrentActionBuilder(container);
        }

        public RMAzureCosmosDBConcurrentActionBuilder WithRetryTimes(int retryTimes)
        {
            RetryTimes = retryTimes;
            return this;
        }

        public RMAzureCosmosDBConcurrentActionBuilder WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            return this;
        }

        public RMAzureCosmosDBConcurrentActionBuilder WithInitialRetryDelayTime(int initialRetryDelayTime)
        {
            InitialRetryDelayTime = initialRetryDelayTime;
            return this;
        }

        public RMAzureCosmosDBImmediatelyConcurrentAction ToImmediately()
        {
            return new RMAzureCosmosDBImmediatelyConcurrentAction(Container, RetryTimes, MaxDegreeOfParallelism, InitialRetryDelayTime);
        }

        public RMAzureCosmosDBDelayConcurrentAction ToDelay()
        {
            return new RMAzureCosmosDBDelayConcurrentAction(Container, RetryTimes, MaxDegreeOfParallelism, InitialRetryDelayTime);
        }
    }
}
