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
using AvePoint.RA.Common.RAProcess.Locker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.RAProcess
{
    public class RMProcessMessageQueue
    {
        private const int QUEUE_CAPACITY = 10;

        private readonly string _messageContainerPath;

        private readonly IRMProcessLocker _locker;

        public RMProcessMessageQueue(string messageContainerPath, IRMProcessLocker locker)
        {
            _messageContainerPath = messageContainerPath;
            Directory.CreateDirectory(messageContainerPath);
            _locker = locker;
        }

        public void Enqueue(string message)
        {
            Enqueue(message, CancellationToken.None);
        }

        public void Enqueue(string message, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                Enqueue(message, cts.Token);
            }
        }

        public void Enqueue(string message, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (GetMessageCount(_messageContainerPath) >= QUEUE_CAPACITY)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    _locker.Lock(() =>
                    {
                        var fileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}.message";
                        var filePath = Path.Combine(_messageContainerPath, fileName);
                        File.WriteAllText(filePath, message);
                    });
                    return;
                }
            }
        }

        public string Dequeue()
        {
            return Dequeue(CancellationToken.None);
        }

        public string Dequeue(TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                return Dequeue(cts.Token);
            }
        }

        public string Dequeue(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (GetMessageCount(_messageContainerPath) == 0)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    return _locker.Lock(() =>
                    {
                        var needConsumeFile = Directory.GetFiles(_messageContainerPath, "*.message", SearchOption.TopDirectoryOnly)
                    .OrderBy(file => new FileInfo(file).CreationTimeUtc)
                    .FirstOrDefault();
                        var message = File.ReadAllText(needConsumeFile);
                        File.Delete(needConsumeFile);
                        while (File.Exists(needConsumeFile))
                        {
                            Thread.Sleep(100);
                        }
                        return message;
                    });
                }
            }
        }

        private static int GetMessageCount(string path)
        {
            var files = Directory.GetFiles(path);
            return files.Length;
        }
    }
}
