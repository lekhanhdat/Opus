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



namespace AvePoint.GCommon.Network
{
    #region using directives

    using System;

    #endregion

    internal class CircularBuffer
    {
        Int32 tail;
        readonly Byte[] buffer;

        public CircularBuffer(Int32 capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity must be greater than or equal to zero.", "capacity");

            this.Capacity = capacity;
            this.buffer = new byte[capacity];
        }

        public long Size { get; private set; }
        public int Capacity { get; private set; }

        public void Put(byte[] src, int offset, int count)
        {
            if (tail + count > Capacity)
            {
                Array.Copy(src, offset, buffer, tail, Capacity - tail);
                Array.Copy(src, offset + (Capacity - tail), buffer, 0, count - (Capacity - tail));
            }
            else
            {
                Array.Copy(src, offset, buffer, tail, count);
            }
            tail = (tail + count) % Capacity;
            Size = Size + count;
        }

        public byte[] GetLatest(int count)
        {
            if (count > Math.Min(Size, Capacity))
                throw new ArgumentException("size is smaller than count", "count");
            var outBuffer = new byte[count];
            if (count > 0)
            {
                if (tail >= count)
                {
                    Array.Copy(buffer, tail - count, outBuffer, 0, count);
                }
                else
                {
                    Array.Copy(buffer, Capacity - (count - tail), outBuffer, 0, count - tail);
                    Array.Copy(buffer, 0, outBuffer, count - tail, tail);
                }
            }
            return outBuffer;
        }
    }
}
