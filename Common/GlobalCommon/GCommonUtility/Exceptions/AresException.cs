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
namespace AvePoint.GCommon.Utility.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This exception is used when error occurred calling web api
    /// </summary>
    [Serializable]
    public class AresException : Exception
    {
        const string defaultErrorMessage = "An unknown error occurred while calling web API.";
        private string errorMessage = string.Empty;
        private string httpErrorCode = string.Empty;

        public string Message = string.Empty;
        public string CorrelationId = string.Empty;
        /// <summary>
        /// Construct method with no parameter
        /// </summary>
        public AresException()
        {
            this.Message = BuildDefaultMessage();
        }
        /// <summary>
        /// Construct method with error message and http error code
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="httpErrorCode"></param>
        public AresException(string message, string httpErrorCode)
            : base(message)
        {
            this.errorMessage = message;
            this.httpErrorCode = httpErrorCode;
            this.Message = BuildMessageForHttp();
        }

        public AresException(string message, string httpErrorCode, string correlationId)
            : base(message)
        {
            this.errorMessage = message;
            this.CorrelationId = correlationId;
            this.httpErrorCode = httpErrorCode;
            this.Message = BuildMessage();
        }

        private string BuildDefaultMessage()
        {
            return defaultErrorMessage;
        }

        private string BuildMessageForHttp()
        {
            if (!string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(httpErrorCode))
            {
                return string.Format("An error occurred while calling web API, http error code: {0}, error message: {1}.", httpErrorCode, errorMessage);
            }
            return defaultErrorMessage;
        }

        private string BuildMessage()
        {
            if (!string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(CorrelationId) && ! string.IsNullOrEmpty(httpErrorCode))
            {
                return string.Format("An error occurred while calling web API, correlation id: {0}, http error code: {1}, error message: {2}.", CorrelationId, httpErrorCode, errorMessage);
            }
            return defaultErrorMessage;
        }

        public override string ToString()
        {
            return string.Format("{0}\r\n{1}.", Message, base.ToString());
        }
    }
}
