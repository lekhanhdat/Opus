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
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Network
{
    [Serializable]
    public class CommonNetworkException : Exception
    {
        public CommonNetworkException() { }

        public CommonNetworkException(string message) : base(message) { }

        public CommonNetworkException(string message, Exception innerException)
            : base(message, innerException) { }

        protected CommonNetworkException(SerializationInfo info, StreamingContext context) { }
    }

    /// <summary>
    /// 在握手过程中发生的，已知和未知的错误都会抛出这个异常
    /// </summary>
    [Serializable]
    public class HandShakeException : CommonNetworkException
    {
        public HandShakeException(string message)
            : base(message)
        {
        }

        public HandShakeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 当网络传输发生异常后，AveNetWorkServer等待AveNetWork重连超时没有收到重连信息，抛出这个异常
    /// </summary>
    [Serializable]
    public class SessionTimeoutException : CommonNetworkException
    {
        private int timeout;
        public int Timeout { get { return this.timeout; } }

        public SessionTimeoutException(string message)
            : base(message)
        {
        }

        public SessionTimeoutException(int timeout, string message)
            : this(message)
        {
            this.timeout = timeout;
        }
    }

    /// <summary>
    /// 当超过预定时间(30min)限制，网络都不能恢复畅通的情况下，抛出这个异常
    /// </summary>
    [Serializable]
    public class NetworkBrokenException : CommonNetworkException
    {
        public NetworkBrokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 在缓冲中，无法找到对方要求重发的数据的时候，抛出这个异常
    /// </summary>
    [Serializable]
    public class CachedBufferOverflowException : CommonNetworkException
    {
        public CachedBufferOverflowException(string message)
            : base(message)
        {
        }
    }
}