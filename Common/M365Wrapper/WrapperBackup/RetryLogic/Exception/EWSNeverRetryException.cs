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
namespace ExchangeUtility.Graph
{
    using Microsoft.Exchange.WebServices.Data;

    /// <summary>
    /// Indicate EWS encounter an error which cannot be recovered by wait-retry strategy.
    /// Use this exception to pass through the endless retry barrier.
    /// </summary>
    [System.Serializable]
    public class EWSNeverRetryException : System.Exception
    {
        public ServiceError ErrorCode { get; private set; }

        public EWSNeverRetryException() { }
        public EWSNeverRetryException(string message, ServiceError errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }
        public EWSNeverRetryException(string message, System.Exception inner)
            : base(message, inner)
        {
            var srespEx = inner as ServiceResponseException;
            if (srespEx != null)
            {
                this.ErrorCode = srespEx.ErrorCode;
            }
        }
        protected EWSNeverRetryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}