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


namespace Office365GroupRestore
{
    #region
    using System.Collections.Generic;
    using System.Threading;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections.Concurrent;

    #endregion

    public class RestoreDataHandler : RestoreDataHandlerBase
    {
        private ConcurrentQueue<ExchangeDataBlock> queue = new ConcurrentQueue<ExchangeDataBlock>();
        
        private EORestoreType restoreType;

        private long cacheSize = 0;
        
        public override void Add(ExchangeDataBlock dataBlock)
        {
            while (queue.Count >= RestoreConstants.CacheCount && cacheSize > RestoreConstants.CacheSize)
            {
                Process proc = Process.GetCurrentProcess();
                long usedMemory = proc.PrivateMemorySize64;
                logger.Warn("Enqueue wait, current cache size: {0}, current queue count: {1}, used memory: {2}", cacheSize, queue.Count, usedMemory);
                Thread.Sleep(1000);
            }

            queue.Enqueue(dataBlock);
            if (dataBlock.FileTail != null)
            {
                cacheSize += dataBlock.FileTail.FileSize;
            }
        }


        public void ProcessEx(string message)
        {
            var dataBlock = new ExchangeDataBlock() { IsException = true, ExceptionMessage = message };
            Add(dataBlock);
        }
        
    }

}