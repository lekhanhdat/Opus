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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.RAProcess
{
    public class RMProcessPool
    {
        private readonly static RALogger logger = RALogger.GetInstance(typeof(RMProcessPool));

        private readonly int _maxCount;

        private readonly string _fileName;

        private readonly string _arguments;

        private readonly List<RMProcess> _processes;

        private readonly CancellationTokenSource _cts;

        public RMProcessPool(int maxCount, string fileName, string arguments)
        {
            _maxCount = maxCount;
            _fileName = fileName;
            _arguments = arguments;
            _processes = new List<RMProcess>();
            _cts = new CancellationTokenSource();
            _ = EnsureStartAsync(_cts.Token);
        }

        public void Start()
        {
            while (_processes.Count < _maxCount)
            {
                var process = new RMProcess(_fileName, _arguments);
                process.Start();
                _processes.Add(process);
            }
        }

        public void Stop()
        {
            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch (Exception ex) {
                logger.Error($"Fail stop RMProcessPool, ex:{ex}");
            }

            foreach (var process in _processes)
            {
                if (process.Exists())
                {
                    process.Close();
                }
            }
        }

        private async Task EnsureStartAsync(CancellationToken token)
        {
            while (true)
            {
                foreach (var process in _processes)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (!process.Exists())
                    {
                        process.Start();
                    }
                }
                await Task.Delay(1000);
            }
        }
    }
}
