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

using AvePoint.GCommon;
using System;
using System.IO;

namespace AvePoint.Wrapper.Common
{
    public class AveSPDataStreamReader : Stream, IDisposable
    {
        public const long USE_SPDATA_STREAM_READER_LIMIT = 1024 * 1024 * 1024;    // 1 GB

        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSPDataStreamReader));

        private long contentLength;
        private Stream innerStream;
        private IDisposable[] disposeObjs;

        public AveSPDataStreamReader(Stream innerStream, long contentLength, params IDisposable[] disposeObjs)
        {
            logger.Info($"Input content length is: {contentLength}");
            this.contentLength = this.TryGetInnerStreamLength(out var length) ? length : contentLength;
            this.innerStream = innerStream;
            this.disposeObjs = disposeObjs;
        }

        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                return this.contentLength;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set
            {
                innerStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            try
            {
                innerStream.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error($"Dispose stream failed. {ex}");
            }

            if(disposeObjs != null)
            {
                foreach (var disposeObj in disposeObjs)
                {
                    try
                    {
                        disposeObj.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Dispose related object failed. {ex}");
                    }
                }
            }
        }

        private bool TryGetInnerStreamLength(out long length)
        {
            length = -1;
            try
            {
                length = innerStream.Length;
                return true;
            }
            catch (Exception ex)
            {
                logger.Info($"Get content length from inner stream failed. {ex}");
            }
            return false;
        }
    }
}
