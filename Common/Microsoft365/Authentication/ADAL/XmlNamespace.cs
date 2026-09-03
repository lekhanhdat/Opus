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
using System.Xml.Linq;

namespace Microsoft365.Authentication.ADAL
{
	internal class XmlNamespace
	{
		public static readonly XNamespace Wsdl = "http://schemas.xmlsoap.org/wsdl/";

		public static readonly XNamespace Wsp = "http://schemas.xmlsoap.org/ws/2004/09/policy";

		public static readonly XNamespace Http = "http://schemas.microsoft.com/ws/06/2004/policy/http";

		public static readonly XNamespace Sp = "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702";

		public static readonly XNamespace Sp2005 = "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";

		public static readonly XNamespace Wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

		public static readonly XNamespace Soap12 = "http://schemas.xmlsoap.org/wsdl/soap12/";

		public static readonly XNamespace Wsa10 = "http://www.w3.org/2005/08/addressing";

		public static readonly XNamespace Trust = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";

		public static readonly XNamespace Trust2005 = "http://schemas.xmlsoap.org/ws/2005/02/trust";

		public static readonly XNamespace Issue = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue";

		public static readonly XNamespace Issue2005 = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";

		public static readonly XNamespace SoapEnvelope = "http://www.w3.org/2003/05/soap-envelope";
	}
}