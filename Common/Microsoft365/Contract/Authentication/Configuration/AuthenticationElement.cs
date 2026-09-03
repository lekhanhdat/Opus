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
namespace Microsoft365.Authentication.Configuration
{
    public class AuthenticationElement
    {
        public string Domain { get; set; }
        public string Method { get; set; }
        /// <summary>
        /// example: <authenticationElement domain="imaples.online" method="AvePoint.Office365.Api.CertAuthProviderApi, Office365Api" parameters="certBase64=xxx" />
        /// 1.certBase64:authentication base64 certificate content
        /// 2.securityProtocol:TLS requirement for customer's authentication
        /// </summary>
        public AuthenticationParameter Parameters { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain">cusotmer's domain</param>
        /// <param name="method">Authentication framework,</param>
        /// <param name="parameters"></param>
        public AuthenticationElement(string domain, string method, AuthenticationParameter parameters)
        {
            Domain = domain;
            Method = method;
            Parameters = parameters;
        }
        public override string ToString()
        {
            return $"Domain={Domain};Method={Method};Parameters={Parameters}";
        }
    }
    public class AuthenticationParameter
    {
        public string CertBase64 { get; set; }
        //public string Environment { get; set; }
        /// <summary>
        /// not allow null for CertAuthProvider
        /// </summary>
        //public string SecurityProtocol { get; set; }
        public override string ToString()
        {
            //return $"CertBase64={CertBase64?.Length}:{CertBase64?.GetHashCode()};env={Environment};SecurityProtocol={SecurityProtocol}";
            return $"CertBase64={CertBase64?.Length}:{CertBase64?.GetHashCode()};";
        }
    }
}