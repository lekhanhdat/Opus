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




namespace AvePoint.Media.Storage.FTP
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The <c>FtpException</c> class encapsulates an FTP exception.
    /// </summary>
    [Serializable]
    class FtpException : Exception
    {
        private readonly int ftpError;

        public FtpException()
        {
            this.ftpError = 0;            
        }

        public FtpException(string message) : this(-1, message)
        {
        }
        
        public FtpException(int error, string message) : base(message)
        {
            this.ftpError = error;
        }

        public FtpException(string message, Exception innerException) : base(message, innerException)
        {         
        }

        protected FtpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {         
        }

        public int ErrorCode
        {
            get { return this.ftpError; }
        }

        /// <summary>
        /// No specific impelementation is needed of the GetObjectData to serialize this object
        /// because all attributes are redefined.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data. </param>
        /// <param name="context">The destination for this serialization. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}