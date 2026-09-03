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

namespace AvePoint.GCommon.Network
{
    internal class CircularBuffer
    {
        private int capacity;
        private long size;
        private int tail;
        private byte[] buffer;

        public CircularBuffer(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity must be greater than or equal to zero.", "capacity");

            this.capacity = capacity;
            this.buffer = new byte[capacity];
        }

        public long Size
        {
            get { return size; }
        }

        public int Capacity
        {
            get { return capacity; }
        }

        public void Put(byte[] src, int offset, int count)
        {
            if (tail + count > capacity)
            {
                Array.Copy(src, 0, buffer, tail, capacity - tail);
                Array.Copy(src, offset + (capacity - tail), buffer, 0, count - (capacity - tail));
            }
            else
            {
                Array.Copy(src, offset, buffer, tail, count);
            }
            tail = (tail + count) % capacity;
            size = size + count;
        }

        public byte[] GetLatest(int count)
        {
            if (count > Math.Min(size, capacity)) throw new ArgumentException("size is smaller than count", "count");
            byte[] outBuffer = new byte[count];
            if (tail >= count)
            {
                Array.Copy(buffer, tail - count, outBuffer, 0, count);
            }
            else
            {
                Array.Copy(buffer, capacity - (count - tail), outBuffer, 0, count - tail);
                Array.Copy(buffer, 0, outBuffer, count - tail, tail);
            }
            return outBuffer;
        }
    }
}
