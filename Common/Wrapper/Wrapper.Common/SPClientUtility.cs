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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public static class SPClientUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SPClientUtility), false);

        private static XmlNamespaceManager s_odatansmgr;
        private static XmlNamespaceManager s_soapnsmgr;

        private static XmlNamespaceManager SOAPNamespaceManager
        {
            get
            {
                if (s_soapnsmgr == null)
                {
                    XmlNameTable nameTable = new NameTable();
                    XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
                    xmlNamespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    xmlNamespaceManager.AddNamespace("spsoap", "http://schemas.microsoft.com/sharepoint/soap/");
                    s_soapnsmgr = xmlNamespaceManager;
                }
                return s_soapnsmgr;
            }
        }

        internal static XmlNamespaceManager ODataNamespaceManager
        {
            get
            {
                if (s_odatansmgr == null)
                {
                    XmlNameTable nameTable = new NameTable();
                    XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
                    xmlNamespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                    xmlNamespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                    s_odatansmgr = xmlNamespaceManager;
                }
                return s_odatansmgr;
            }
        }
        internal static XmlDocument LoadXml(TextReader reader)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSecureResolver xmlResolver = new XmlSecureResolver(new XmlUrlResolver(), new PermissionSet(PermissionState.None));
            xmlDocument.XmlResolver = xmlResolver;
            XmlReader reader2 = XmlReader.Create(reader);
            xmlDocument.Load(reader2);
            return xmlDocument;
        }

        public static string ExtractSoapError(WebException webEx)
        {
            try
            {
                HttpWebResponse httpWebResponse = webEx.Response as HttpWebResponse;
                if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.InternalServerError && httpWebResponse.ContentType.IndexOf("text/xml", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        XmlDocument xmlDocument = LoadXml(streamReader);
                        XmlNode xmlNode = xmlDocument.SelectSingleNode("soap:Envelope/soap:Body/soap:Fault/detail/spsoap:errorstring", SOAPNamespaceManager);
                        if (xmlNode != null)
                        {
                            return xmlNode.InnerText;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Extract Soap Exception from exception:{0} failed:{1}", webEx, e);
            }
            return null;
        }
    }
}
