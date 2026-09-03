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

using AvePoint.GCommon.Transfer.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Transfer.HttpMode
{
    public class AveDataTransferQueue
    {
        public Queue<DataUnit> CacheBuffer = null;
        public int CacheNumber = 0;
        private object QueueLock = new object();

        public AveDataTransferQueue()
            : this(DataTransferGlobalConfig.DataTransferConfiguration.HttpCacheDataNumber)
        {

        }

        public AveDataTransferQueue(int size)
        {
            CacheBuffer = new Queue<DataUnit>(size);
            CacheNumber = size;
        }

        /// <summary>
        /// queue resend data and put to resent queue to resent 
        /// data resent is still left in cache data 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public AveDataTransferQueue QueryReSentData(long number)
        {
            AveDataTransferQueue reSendData = new AveDataTransferQueue(CacheNumber);
            if (CacheBuffer.Count > 0 && CacheBuffer.Peek().SerialNumber > number)
            {
                throw new NoneDataFoundException(string.Format("Can not found DataBlock in reconnection, data number is:{0}, and first data cache in queue is:{1}", number, CacheBuffer.Peek().SerialNumber));
            }
            Queue<DataUnit> tempCacheBuffer = new Queue<DataUnit>();
            while (CacheBuffer.Count > 0)
            {
                DataUnit tempData = CacheBuffer.Dequeue();
                if (tempData.SerialNumber >= number)
                {
                    reSendData.Enque(tempData);
                    tempCacheBuffer.Enqueue(tempData);
                }
            }
            while (tempCacheBuffer.Count > 0)
            {
                CacheBuffer.Enqueue(tempCacheBuffer.Dequeue());
            }
            return reSendData;

        }

        public int Length
        {
            get { return CacheBuffer.Count; }
        }

        /// <summary>
        /// cache data, remove the first cache data if size limit is reached.
        /// </summary>
        /// <param name="data"></param>
        public void Enque(DataUnit data)
        {
            lock (QueueLock)
            {
                if (CacheBuffer.Count >= CacheNumber)
                {
                    CacheBuffer.Dequeue();
                }
                CacheBuffer.Enqueue(data);
            }

        }

        public DataUnit Deque()
        {
            lock (QueueLock)
            {
                return CacheBuffer.Dequeue();
            }
        }

        public DataUnit Peek()
        {
            lock (QueueLock)
            {
                return CacheBuffer.Peek();
            }
        }
    }


    public class DataUnit
    {
        public long SerialNumber;

        public byte[] Buffer;

        public DataUnit(long serialNumber, byte[] buffer, int dataLength)
        {
            SerialNumber = serialNumber;
            Buffer = new byte[dataLength];
            Array.Copy(buffer, Buffer, dataLength);
        }
    }
}
