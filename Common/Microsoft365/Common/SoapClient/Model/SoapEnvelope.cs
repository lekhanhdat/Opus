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
    using System.Xml.Serialization;
    using System.Xml.Linq;
    /// <summary>
    /// Represents a SOAP Envelope
    /// </summary>
    [XmlRoot("Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SoapEnvelope
    {
        /// <summary>
        /// The SOAP Envelope Header section
        /// </summary>
        [XmlElement("Header")]
        public SoapEnvelopeHeader Header { get; set; }

        /// <summary>
        /// The SOAP Envelope Body section
        /// </summary>
        [XmlElement("Body")]
        public SoapEnvelopeBody Body { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SoapEnvelope"/>
        /// </summary>
        public SoapEnvelope()
        {
            Header = new SoapEnvelopeHeader();
            Body = new SoapEnvelopeBody();
        }


        /// <summary>
        /// create a new instance
        /// </summary>
        /// <returns></returns>
        public static SoapEnvelope New()
        {
            return new SoapEnvelope();
        }
    }
}
