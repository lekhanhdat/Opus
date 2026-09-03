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

namespace AvePoint.Media.Storage.FTP.Wrapper
{
    #region using directives
    using System;
    #endregion

    /// <summary>
    /// FTP related error
    /// </summary>
    [Serializable]
    public class FtpException : Exception
    {
        /// <summary>
        /// Initializes the exception object
        /// </summary>
		/// <param name="message">The error message</param>
		public FtpException(string message) : base(message) { }
        public FtpException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception triggered on command failures
    /// </summary>
    [Serializable]
    public class FtpCommandException : FtpException
    {
        string _code = null;
        /// <summary>
        /// Gets the completion code associated with the response
        /// </summary>
        public string CompletionCode
        {
            get { return _code; }
            private set { _code = value; }
        }

        /// <summary>
        /// The type of response received from the last command executed
        /// </summary>
        public FtpResponseType ResponseType
        {
            get
            {
                if (_code != null)
                {
                    // we only care about error types, if an exception
                    // is being thrown for a successful response there
                    // is a problem.
                    switch (_code[0])
                    {
                        case '4':
                            return FtpResponseType.TransientNegativeCompletion;
                        case '5':
                            return FtpResponseType.PermanentNegativeCompletion;
                    }
                }

                return FtpResponseType.None;
            }
        }

        /// <summary>
        /// Initalizes a new instance of a FtpResponseException
        /// </summary>
        /// <param name="code">Status code</param>
        /// <param name="message">Associated message</param>
        public FtpCommandException(string code, string message)
            : base(message)
        {
            CompletionCode = code;
        }

        /// <summary>
        /// Initalizes a new instance of a FtpResponseException
        /// </summary>
        /// <param name="reply">The FtpReply to build the exception from</param>
        public FtpCommandException(FtpReply reply)
            : this(reply.Code, reply.ErrorMessage)
        {
        }
    }

    /// <summary>
    /// Exception is thrown when encryption could not be negotiated by the server
    /// </summary>
    [Serializable]
    public class FtpSecurityNotAvailableException : FtpException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FtpSecurityNotAvailableException()
            : base("Security is not available on the server.")
        {
        }

        /// <summary>
        /// Custom error message
        /// </summary>
        /// <param name="message">Error message</param>
        public FtpSecurityNotAvailableException(string message)
            : base(message)
        {
        }
    }
}