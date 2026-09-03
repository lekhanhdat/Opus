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
namespace Microsoft365.Common.SoapClient
{
    using System;
    using System.Xml.Linq;
    /// <summary>
    /// server exception
    /// </summary>
    [Serializable]
    public class SoapFaultException : SoapClientException
    {
        private const string DefaultErrorMessage = "A fault was returned by the server";

        public string Code { get; set; }

        public string String { get; set; }

        public string Actor { get; set; }

        public XElement Detail { get; set; }

        public SoapFaultException() : base(DefaultErrorMessage)
        {

        }

        public SoapFaultException(string message) : base(message)
        {

        }

        public SoapFaultException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public SoapFaultException(Exception innerException) : base(DefaultErrorMessage, innerException)
        {

        }
    }
}
