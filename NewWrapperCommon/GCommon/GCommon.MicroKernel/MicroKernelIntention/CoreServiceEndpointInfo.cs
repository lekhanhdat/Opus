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



namespace AvePoint.GCommon.Contract
{
    #region using directives
    using System;

    #endregion

    /// <summary>
    /// Represent the core service endpoint information
    /// </summary>
    public class CoreServiceEndpointInfo
    {
        /// <summary>
        /// This host name or ip address of core service 
        /// </summary>
        public String HostOrIpAddress { get; set; }

        /// <summary>
        /// port of core service
        /// </summary>
        public Int32 Port { get; set; }

        /// <summary>
        /// the uri scheme
        /// </summary>
        public String Scheme { get; set; }

        /// <summary>
        /// This property is to use as a specified IOC container key
        /// </summary>
        public String RemotingTypeKey { get; set; }
        /// <summary>
        /// This property must be set
        /// </summary>
        public String EndpointConfigurationName { get; set; }

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
        /// The authorization key of DocAve communication system
        /// </summary>
        public String AuthorizationKey { get; set; }

        /// <summary>
        /// Identify if redirect the type in another assembly 
        /// </summary>
        public Boolean IsRedirectArgumentType { get; set; }

        /// <summary>
        /// if IsRedirectArgumentType is true, the target assembly name
        /// </summary>
        public String RedirectAssemblyName { get; set; }

        /// <summary>
        /// Give an chance to force use the old method.
        /// </summary>
        public Boolean IsUseOldMethod { get; set; }

        public string AllAccountProfilePwdCrc { get; set; }

        /// <summary>
        /// get the current endpoint information
        /// </summary>
        /// <returns>a string represent current endpoint information</returns>
        public override String ToString()
        {
            return String.Format("{0}://{1}:{2}",Scheme,HostOrIpAddress,Port);
        } 
    }
}
