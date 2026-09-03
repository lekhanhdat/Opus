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
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    public static class XThumbprintUtility
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string OldThumbprint = "ef b6 aa a0 3d 17 26 8b ad 4d e3 d4 e0 9f c0 5e 24 c1 b3 c8";
        public static string CertificateThumbprintFromProduct = string.Empty;

        public static string GetLocalCertificateThumbprint()
        {
            var behaviorsConfig = String.Empty;
            var controlTimerBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"bin\Config\ControlWCFBehaviors.config");
            var controlWebBehaviorsConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\ControlWCFBehaviors.config");

            if (File.Exists(controlWebBehaviorsConfig))
            {
                behaviorsConfig = controlWebBehaviorsConfig;
            }
            else if (File.Exists(controlTimerBehaviorsConfig))
            {
                behaviorsConfig = controlTimerBehaviorsConfig;
            }

            
            if (!string.IsNullOrEmpty(behaviorsConfig) && File.Exists(behaviorsConfig))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(behaviorsConfig);
                XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
                XmlNode serviceCertificateNode = xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
                string wcfThumbprint1 = clientCertificateNode.Attributes["findValue"].Value;
                if (serviceCertificateNode != null)
                {
                    string wcfThumbprint2 = serviceCertificateNode.Attributes["findValue"].Value;
                    if (string.Compare(wcfThumbprint1, wcfThumbprint2, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        logger.Debug("The two thumbprint values in WCF behavior configuration file are different. {0}", behaviorsConfig);
                        //throw new SecurityTokenException("WCF behavior configuration file is invalid.");
                    }
                }
                CertificateThumbprintFromProduct = wcfThumbprint1;
                return wcfThumbprint1;
            }
            //else
            //{
            //    logger.Info("Can not find WCF behavior configuration file {0}", behaviorsConfig);
            //    throw new SecurityTokenException("Can not find WCF behavior configuration file.");
            //}
            return string.Empty;
        }
    }
}