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

namespace AvePoint.Media.ClassicStorage.Util
{
    /// <summary>
    /// storage sdk api异常的基类.
    /// </summary>
    [Serializable]
    public class XException : ApplicationException
    {
        public XException(string msg) : base(msg) { }

        public XException(string msg, Exception e) : base(msg, e) { }

        public XException(Exception e) : base(e.Message, e) { }
    }

    [Serializable]
    public class PathNotFoundException : XException
    {
        public PathNotFoundException(string msg) : base(msg) { }

        public PathNotFoundException(string msg, Exception t) : base(msg, t) { }
    }

    [Serializable]
    public class PathAlreadyExistsException : XException
    {
        public PathAlreadyExistsException(string msg) : base(msg) { }

        public PathAlreadyExistsException(string msg, Exception t) : base(msg, t) { }
    }

    /// <summary>
    /// 表示Storage SDK system级别的异常, 比如权限问题等.
    /// </summary>

    [Serializable]
    public class XSystemException : XException
    {
        public XSystemException(string msg) : base(msg) { }

        public XSystemException(string msg, Exception e) : base(msg, e) { }
    }

    /// <summary>
    /// 表示Storage SDK IO 异常
    /// </summary>
    [Serializable]
    public class XIOException : XException
    {
        public XIOException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// 表示管理VIM出现的相关异常
    /// </summary>
    [Serializable]
    public class VIMLoadException : XException
    {
        public VIMLoadException(string msg) : base(msg) { }

        public VIMLoadException(string msg, Exception innerE) : base(msg, innerE) { }
    }

    [Serializable]
    public class UnknownException : XException
    {
        public UnknownException(string msg) : base(msg) { }

        public UnknownException(string msg, Exception t) : base(msg, t) { }
    }

    /// <summary>
    /// Storage SDK 不支持操作
    /// </summary>
    [Serializable]
    public class UnsupportedXException : XException
    {
        public UnsupportedXException(string msg) : base(msg) { }
    }

    /// <summary>
    /// XRI相关异常， 比如格式不对等情况
    /// </summary>
    [Serializable]
    public class InvalidXRIException : XException
    {
        public InvalidXRIException(string msg) : base(msg) { }

        public InvalidXRIException(string msg, Exception e) : base(msg, e) { }
    }

    [Serializable]
    public class CatchedToDoMoreExcetion : XException
    {
        public CatchedToDoMoreExcetion(string msg, Exception e) : base(msg, e) { }

        public CatchedToDoMoreExcetion(string msg) : base(msg) { }
    }

    [Serializable]
    public class AuthenticationFailedException : XException
    {
        public AuthenticationFailedException() : base("Authentication Failed.") { }

        public AuthenticationFailedException(string msg) : base(msg) { }

        public AuthenticationFailedException(string msg, Exception e) : base(msg, e) { }

        public AuthenticationFailedException(string msg, string customizedDetail, Exception e)
            : base(msg, e)
        {
            this.customizedDetail = customizedDetail;
        }

        private string customizedDetail;

        public string CustomizedDetail { get { return customizedDetail; } }
    }

    [Serializable]
    public class BucketInOtherRegionException : XException
    {
        public BucketInOtherRegionException(string msg, Exception e) : base(msg, e) { }

        public BucketInOtherRegionException(string msg) : base(msg) { }
    }

    [Serializable]
    public class DeviceNotAvailableException : XException
    {
        public DeviceNotAvailableException(string msg, Exception e) : base(msg, e) { }

        public DeviceNotAvailableException(string msg) : base(msg) { }
    }

    [Serializable]
    public class NotEnoughFreeSpaceException : XException
    {
        public NotEnoughFreeSpaceException(string msg, Exception e) : base(msg, e) { }

        public NotEnoughFreeSpaceException(string msg) : base(msg) { }
    }

    [Serializable]
    public class MethodNotSupportForReadOnlyDeviceException : XException
    {
        public MethodNotSupportForReadOnlyDeviceException(string msg, Exception e) : base(msg, e) { }

        public MethodNotSupportForReadOnlyDeviceException(string msg) : base(msg) { }
    }

    [Serializable]
    public class RetryableException : XException
    {
        public RetryableException(string message)
            : base(message)
        {
        }

        public RetryableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class MoveFailedException : XException
    {
        public MoveFailedException(string message)
            : base(message)
        {
        }

        public MoveFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class DeleteSrcFileFailedException : XException
    {
        public DeleteSrcFileFailedException(string message)
            : base(message)
        {
        }

        public DeleteSrcFileFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class ProhibitDeleteException : XException
    {
        public ProhibitDeleteException(string message)
            : base(message)
        {
        }

        public ProhibitDeleteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class FailoverModeException : XException
    {
        public FailoverModeException(string message)
            : base(message)
        {
        }

        public FailoverModeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class IDNullException : XException
    {
        public IDNullException(string message)
            : base(message)
        {
        }

        public IDNullException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    [Serializable]
    public class DeleteFailedException : XException
    {
        public DeleteFailedException(string message)
            : base(message)
        {
        }

        public DeleteFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}