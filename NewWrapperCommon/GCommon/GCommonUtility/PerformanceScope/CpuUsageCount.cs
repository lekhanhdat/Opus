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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.PerformanceScope
{
    public class CpuUsageCount : IDisposable
    {
        private AveLogger _logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _syncRoot = new object();
        private readonly Timer _timer;
        private readonly ICpuUsageProvider _provider;
        private double _lastSample;

        public CpuUsageCount()
        {
            _provider = CpuUsageProviderFactory.Create();
            _logger.Info("CpuUsageCount created with provider: {0}", _provider.GetType().FullName);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            _provider.SetProcessId(process.Id);
            //_timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }

        public JobMetrics GetMetrics()
        {
            lock (_syncRoot)
            {
                var value = _provider.GetMetrics();
                return value;
            }
        }
        
        public double GetCpuUsage()
        {
            lock (_syncRoot)
            {
                var value = _provider.GetCpuUsage();
                return value;
            }
        }

        public long GetMemoryUsage()
        {
            lock (_syncRoot)
            {
                var value = _provider.GetMemoryUsage();
                return value;
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                _timer?.Dispose();
                _provider.Dispose();
            }
        }

        private void OnTick(object state)
        {
            try
            {
                var value = _provider.GetCpuUsage();
                lock (_syncRoot)
                {
                    _lastSample = value;
                }
            }
            catch
            {
                lock (_syncRoot)
                {
                    _lastSample = 0d;
                }
            }
        }
    }
}
