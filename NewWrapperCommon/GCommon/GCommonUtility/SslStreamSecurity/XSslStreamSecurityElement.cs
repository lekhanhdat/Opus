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
using System.Configuration;
using System.Reflection;
using System.Security.Authentication;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    public sealed class XSslStreamSecurityElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;
        private static readonly AveLogger Logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection
                    {
                        new ConfigurationProperty("requireClientCertificate", typeof(bool), false, null, null, ConfigurationPropertyOptions.None),
                        new ConfigurationProperty("sslProtocols", typeof(XSslProtocols), XSslProtocols.Ssl3 | XSslProtocols.Tls | XSslProtocols.Tls11 | XSslProtocols.Tls12, null, null, ConfigurationPropertyOptions.None)
                    };
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("requireClientCertificate", DefaultValue = false)]
        public bool RequireClientCertificate
        {
            get
            {
                return (bool)base["requireClientCertificate"];
            }
            set
            {
                base["requireClientCertificate"] = value;
            }
        }

        [ConfigurationProperty("sslProtocols", DefaultValue = (XSslProtocols.Ssl3 | XSslProtocols.Tls | XSslProtocols.Tls11 | XSslProtocols.Tls12))]
        public XSslProtocols SslProtocols
        {
            get
            {
                return (XSslProtocols)base["sslProtocols"];
            }
            private set
            {
                base["sslProtocols"] = value;
            }
        }

        public override Type BindingElementType
        {
            get
            {
                return typeof(XSslStreamSecurityBindingElement);
            }
        }

        public XSslStreamSecurityElement()
        {

        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            XSslStreamSecurityBindingElement sslStreamSecurityBindingElement = (XSslStreamSecurityBindingElement)bindingElement;
            sslStreamSecurityBindingElement.RequireClientCertificate = this.RequireClientCertificate;
            //sslStreamSecurityBindingElement.XSslProtocols = (SslProtocols)(int)this.SslProtocols;
            if (UseTls1())
            {
                Logger.Debug("Use Tls1");
                //this.SslProtocols = XSslProtocols.Tls;
                sslStreamSecurityBindingElement.XSslProtocols = (SslProtocols)(int)XSslProtocols.Tls;
            }
            else
            {
                sslStreamSecurityBindingElement.XSslProtocols = (SslProtocols)(int)this.SslProtocols;
            }
        }
        private bool UseTls1()
        {
            try
            {
                if (this.SslProtocols == (XSslProtocols.Ssl3 | XSslProtocols.Tls | XSslProtocols.Tls11 | XSslProtocols.Tls12))
                {
                   
                    if (XUtility.UseNetFrameworkDefaultProvider)
                    {
                        
                        string thumbprintValue = XThumbprintUtility.CertificateThumbprintFromProduct;

                        if (string.IsNullOrEmpty(thumbprintValue))
                        {
                            thumbprintValue = XThumbprintUtility.GetLocalCertificateThumbprint();
                        }

                        if (!string.IsNullOrEmpty(thumbprintValue) && string.Equals(thumbprintValue, XThumbprintUtility.OldThumbprint, StringComparison.OrdinalIgnoreCase))
                        {
                            //Logger.Debug("Use default attribute value");
                            Logger.Debug("local thumbprint value is " + thumbprintValue);
                            //Logger.Debug("Certificate use tls");
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Error("Check Certificate Thumbprint error.");
                Logger.Error(e.ToString());
                return false;
            }
            
        }
        protected override BindingElement CreateBindingElement()
        {
            XSslStreamSecurityBindingElement sslStreamSecurityBindingElement = new XSslStreamSecurityBindingElement();
            this.ApplyConfiguration(sslStreamSecurityBindingElement);
            return sslStreamSecurityBindingElement;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            XSslStreamSecurityElement sslStreamSecurityElement = (XSslStreamSecurityElement)from;
            this.RequireClientCertificate = sslStreamSecurityElement.RequireClientCertificate;
            this.SslProtocols = sslStreamSecurityElement.SslProtocols;
        }

        protected override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            XSslStreamSecurityBindingElement sslStreamSecurityBindingElement = (XSslStreamSecurityBindingElement)bindingElement;
            this.RequireClientCertificate = sslStreamSecurityBindingElement.RequireClientCertificate;
            this.SslProtocols = (XSslProtocols)(int)sslStreamSecurityBindingElement.XSslProtocols;
        }
    }
}
