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
using System.Security.Authentication;
using System.ServiceModel.Channels;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XSslStreamSecurityBindingElement : SslStreamSecurityBindingElement
    {
        private SslProtocols sslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | (SslProtocols)768 | (SslProtocols)3072;

        public SslProtocols XSslProtocols
        {
            get
            {
                return this.sslProtocols;
            }
            set
            {
                this.sslProtocols = value;
                var property = GetType().GetProperty("SslProtocols");
                if (property != null)
                {
                    property.GetSetMethod().Invoke(this, new object[] { this.sslProtocols });
                }
            }
        }

        public XSslStreamSecurityBindingElement() : base()
        {
            if (Enum.IsDefined(typeof(System.Net.SecurityProtocolType), 12288))
            {
                this.sslProtocols |= (SslProtocols)12288;
            }
        }

        internal XSslStreamSecurityBindingElement(SslStreamSecurityBindingElement element) : base(element)
        {
            if(element is XSslStreamSecurityBindingElement)
            {
                this.XSslProtocols = (element as XSslStreamSecurityBindingElement).XSslProtocols;
            }
        }

        protected XSslStreamSecurityBindingElement(XSslStreamSecurityBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.XSslProtocols = elementToBeCloned.XSslProtocols;
        }


        public override BindingElement Clone()
        {
            return new XSslStreamSecurityBindingElement(this);
        }
        public override StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context)
        {
            if (XUtility.UseNetFrameworkDefaultProvider)
            {
                return base.BuildClientStreamUpgradeProvider(context);
            }
            return XSslStreamSecurityUpgradeProvider.CreateClientProvider(this, context);
        }

        public override StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context)
        {
            if (XUtility.UseNetFrameworkDefaultProvider)
            {
                return base.BuildServerStreamUpgradeProvider(context);
            }
            return XSslStreamSecurityUpgradeProvider.CreateServerProvider(this, context);
        }

    }
}
