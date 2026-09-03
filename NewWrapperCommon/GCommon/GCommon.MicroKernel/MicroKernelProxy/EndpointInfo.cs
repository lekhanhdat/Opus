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

namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Diagnostics;
    using Contract;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class EndpointInfo
    {
        /// <summary>
        /// This property must be set
        /// </summary>
        public String HostOrIpAddress { get; set; }

        /// <summary>
        /// This property must be set
        /// </summary>
        public Int32 Port { get; set; }

        public String Scheme { get; set; }

        /// <summary>
        /// This property is must be set
        /// </summary>
        public String EndpointConfigurationName { get; set; }

        /// <summary>
        /// This property is to identify the remoting class of the IOC container
        /// </summary>
        public String RemotingTypeKey { get; set; }

        /// <summary>
        /// This value is must be set when not use the configuration file
        /// </summary>
        public String EndpointAddress { get; set; }

        /// <summary>
        /// This value is must be set when not use the configuration file,
        /// if not set in the situation, the docave pfx file thrumbprint will
        /// be used as default
        /// </summary>
        public Object X509CertificateValidationThumbprintFindValue { get; set; }

        /// <summary>
        /// The authorization key of docave communication system
        /// </summary>
        public String AuthorizationKey { get; set; }

        public Boolean IsRedirectArgumentType { get; set; }

        public String RedirectAssemblyName { get; set; }

        public Boolean IsUseOldMethod { get; set; }

        public string AllAccountProfilePwdCrc { get; set; }

        public EndpointInfo()
        { }

        public EndpointInfo(CoreServiceEndpointInfo endpoint)
        {
            this.EndpointConfigurationName = endpoint.EndpointConfigurationName;
            this.HostOrIpAddress = endpoint.HostOrIpAddress;
            this.Port = endpoint.Port;
            this.Scheme = endpoint.Scheme;
            this.RemotingTypeKey = endpoint.RemotingTypeKey;
            this.EndpointAddress = endpoint.EndpointAddress;
            this.X509CertificateValidationThumbprintFindValue = endpoint.X509CertificateValidationThumbprintFindValue;
            this.AuthorizationKey = endpoint.AuthorizationKey;
            this.IsRedirectArgumentType = endpoint.IsRedirectArgumentType;
            this.RedirectAssemblyName = endpoint.RedirectAssemblyName;
            this.IsUseOldMethod = endpoint.IsUseOldMethod;
            this.AllAccountProfilePwdCrc = endpoint.AllAccountProfilePwdCrc;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.ToString().Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        // override object.GetHashCode
        public override Int32 GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override String ToString()
        {
            var prefixAddress = new UriBuilder(this.Scheme, this.HostOrIpAddress, this.Port).Uri.ToString();
            if (String.IsNullOrEmpty(this.EndpointConfigurationName))
                return prefixAddress + this.EndpointAddress;
            return prefixAddress + this.EndpointConfigurationName;
        }
    }
}
