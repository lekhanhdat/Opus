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

namespace AvePoint.Wrapper.Common
{
    public class PartitionedList<T> : List<T>
    {
        private int _index = 0;
        private int _blockLength;

        public int BlockLength
        {
            get
            {
                return _blockLength;
            }
            set
            {
                _blockLength = value;
            }
        }

        public PartitionedList(int blockLength)
        {
            _blockLength = blockLength;
        }

        public PartitionedList(IEnumerable<T> collection, int blockLength)
            : base(collection)
        {
            _blockLength = blockLength;
        }

        public PartitionedList(int capacity, int blockLength)
            : base(capacity)
        {
            _blockLength = blockLength;
        }

        public List<T> GetNextPartition()
        {
            List<T> partition = GetCertainPartition(_index);
            Interlocked.Add(ref _index, _blockLength);
            return partition;
        }

        public List<T> GetCertainPartition(int startPosition)
        {
            if (startPosition > base.Count - 1)
            {
                return null;
            }

            int reserveLength = base.Count - startPosition;
            int length = Math.Min(reserveLength, _blockLength);
            T[] array = new T[length];
            try
            {
                Array.Copy(base.ToArray(), startPosition, array, 0, length);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
            return new List<T>(array);
        }

        public List<T> GetCertainPartition(int startPosition, int totalLength)
        {
            if (startPosition > base.Count - 1)
            {
                return null;
            }

            List<T> partitionBlock = new List<T>();

            #region Get whole size block
            int times;
            if ((times = totalLength / _blockLength) > 0)
            {
                partitionBlock.AddRange(GetCertainMultiPartition(startPosition, times));
            }
            #endregion

            #region Get partitial size block
            int newStartPosition = startPosition + times * _blockLength;
            int length;
            if ((length = totalLength % _blockLength) > 0)
            {
                partitionBlock.AddRange(GetCertainPartialBlock(newStartPosition, length));
            }
            #endregion

            return partitionBlock;
        }

        private List<T> GetCertainMultiPartition(int startPosition, int times)
        {
            if (times <= 0)
            {
                return null;
            }
            List<T> partitionBlock = new List<T>();
            for (int i = 0; i < times; i++)
            {
                partitionBlock.AddRange(GetCertainPartition(startPosition));
                startPosition += _blockLength;
            }
            return partitionBlock;
        }

        private List<T> GetCertainPartialBlock(int startPosition, int length)
        {
            if (length >= _blockLength)
            {
                return null;
            }
            List<T> partitionBlock = new List<T>();

            T[] array = new T[length];
            try
            {
                Array.Copy(base.ToArray(), startPosition, array, 0, length);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
            partitionBlock.AddRange(array);
            return partitionBlock;
        }
    }
}
