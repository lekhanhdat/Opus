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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Transfer.HttpMode.Common
{
    public class HttpModeStream: Stream, IDisposable
    {
        private CycleStream mStream = new CycleStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.CycleStreamSize);
        private bool isNewOne = true;
        private DateTime lastReadTime;
        private DateTime lastWriteTime;

        public HttpModeStream()
        {
            Reset();
        }

        internal void Reset()
        {
            lastReadTime = DateTime.UtcNow;
            lastWriteTime = DateTime.UtcNow;
            WriteTimeout = DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DefaultReconnectTimeout;
            ReadTimeout = DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.DefaultReconnectTimeout;
            mStream.Reset();
            mStream.WriteTimeoutDelegate = UpdateLastWriteTime;
            mStream.ReadTimeoutDelegate = UpdateLastReadTime;
        }

        #region implement stream method

        public override void Write(byte[] buffer, int offset, int count)
        {
            UpdateLastWriteTime();
            mStream.SafeWrite(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            UpdateLastReadTime();
            int readLength = mStream.SafeRead(buffer, offset, count, false);
            return readLength;
        }

        public override long Length
        {
            get { return mStream.Length; }
        }

        /// <summary>
        /// 最大容量
        /// </summary>
        public long Capacity
        {
            get { return mStream.Capacity; }
        }

        public override bool CanRead { get { return true; } }
        //
        // Summary:
        //     When overridden in a derived class, gets a value indicating whether the current
        //     stream supports seeking.
        //
        // Returns:
        //     true if the stream supports seeking; otherwise, false.
        public override bool CanSeek { get { return false; } }


        public override bool CanWrite { get { return false; } }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
            if (!mStream.IsWriteFinish)
            {
                mStream.FinishWrite();
            }
        }

        public bool IsFinishWrite
        {
            get { return mStream.IsWriteFinish; }
        }

        public bool IsStopped
        {
            get { return mStream.IsStopped; }
        }

        public string StopMessage
        {
            get { return mStream.StopMessage; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }


        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int WriteTimeout { get; set; }
        public override int ReadTimeout { get; set; }

        #endregion

        public void Dispose()
        {
            base.Dispose();
            mStream.Dispose();
            mStream = null;
            isNewOne = false;
        }

        public bool ISNewOne
        {
            get { return isNewOne; }
        }

        internal bool IsWriteTimeout()
        {
            return lastWriteTime.AddMinutes(WriteTimeout) < DateTime.UtcNow;
        }


        internal bool IsReadTimeout()
        {
            return lastReadTime.AddMinutes(ReadTimeout) < DateTime.UtcNow;
        }

        public bool IsReadFinish
        { 
            get 
            {
                if (mStream.IsWriteFinish)
                {
                    return mStream.ReadLength == mStream.WriteLength;
                }
                return false;
            }
        }


        internal void UpdateLastWriteTime()
        {
            lastWriteTime = DateTime.UtcNow;
        }

        internal void UpdateLastReadTime()
        {
            lastReadTime = DateTime.UtcNow;
        }

        internal void Stop(string message)
        {
            mStream.Stop(message);
        }
    }
}
