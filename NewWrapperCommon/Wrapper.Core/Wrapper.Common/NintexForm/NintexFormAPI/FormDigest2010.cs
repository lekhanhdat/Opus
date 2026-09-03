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
/* FormDigest2010.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Represents the form digest for the Nintex Forms service endpoint on SharePoint 2010.
    /// </summary>
    class FormDigest2010 : FormDigestBase
    {
        /// <summary>
        /// Creates a new instance for the specified URL and credentials.
        /// </summary>
        /// <param name="webUrl">The URL of the SharePoint site.</param>
        /// <param name="credentials">The credentials to use.</param>
        /// <remarks>The class invokes the GetUpdatedFormDigest SOAP method, 
        /// provided by the Sites service from SharePoint 2010, 
        /// to retreive the appropriate form digest value.</remarks>
        public FormDigest2010(string webUrl, ICredentials credentials)
        {
            // The SOAP envelope used to get an updated form digest from the Nintex Forms service endpoint.
            const string body = "<?xml version='1.0' encoding='utf-8'?>"
                + "<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' "
                + "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>"
                + "<soap:Body>"
                + "<GetUpdatedFormDigest xmlns='http://schemas.microsoft.com/sharepoint/soap/' />"
                + "</soap:Body>"
                + "</soap:Envelope>";

            // Validate input.
            if (String.IsNullOrEmpty(webUrl)) return;

            // Create the request URL for the specified site URL.
            var soapRequest = (HttpWebRequest)WebRequest.Create(new Uri(webUrl + "/_vti_bin/sites.asmx"));

            // Configure the request as a SOAP action.
            soapRequest.Headers.Add("SOAPAction", "http://schemas.microsoft.com/sharepoint/soap/GetUpdatedFormDigest");
            soapRequest.UseDefaultCredentials = credentials == null ? true : false;
            soapRequest.PreAuthenticate = true;
            if (credentials != null)
                soapRequest.Credentials = credentials;
            soapRequest.ContentType = "text/xml;charset=\"utf-8\"";
            soapRequest.Accept = "text/xml";
            soapRequest.Method = "POST";

            // Encode and write the SOAP envelope into the request stream. 
            Stream stream = soapRequest.GetRequestStream();
            byte[] byteArra = Encoding.UTF8.GetBytes(body);
            stream.Write(byteArra, 0, byteArra.Length);
            stream.Close();

            // Send the request and parse the value of the form digest from the response.
            using (var restResponse = (HttpWebResponse)soapRequest.GetResponse())
            {
                using (var respStream = restResponse.GetResponseStream())
                {
                    if (respStream == null) return;

                    var doc = XDocument.Parse(new StreamReader(respStream).ReadToEnd());
                    XNamespace ns1 = "http://schemas.microsoft.com/sharepoint/soap/";

                    FormDigestValue = doc.Descendants(ns1 + "GetUpdatedFormDigestResult").First().Value;
                }
            }
        }
    }
}
