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
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class CoreChannelProvider : ICoreChannelPovider
    {
        /// <summary>
        /// using endpoint information to create a wcf channel.
        /// </summary>
        /// <typeparam name="TChannel">the channel type</typeparam>
        /// <param name="endpoint">the channel connection information</param>
        /// <returns>the channel object</returns>
        public TChannel CreateChannel<TChannel>(EndpointInfo endpoint)
        {
            var channel = endpoint.Scheme.StartsWith("https", StringComparison.OrdinalIgnoreCase) 
                ? this.GetHttpsChannel<TChannel>(endpoint) : this.GetNetTcpChannel<TChannel>(endpoint);
            return channel;
        }

        /// <summary>
        /// Provide the entry of getting a net tcp channel to special address 
        /// </summary>
        /// <typeparam name="TChannel">the channel object type</typeparam>
        /// <param name="endpoint">the endpoint information</param>
        /// <returns>the created channel</returns>
        TChannel GetNetTcpChannel<TChannel>(EndpointInfo endpoint)
        {
            return this.GetChannel<TChannel>(endpoint, CoreBindingBuilder.CustomBinding, MicroKernelConstant.NetTcpDefaultEndpointAddress);
        }

        /// <summary>
        /// Provide the entry of getting a https channel to special address 
        /// </summary>
        /// <typeparam name="TChannel">the channel object type</typeparam>
        /// <param name="endpoint">the endpoint information</param>
        /// <returns>the created channel</returns>
        TChannel GetHttpsChannel<TChannel>(EndpointInfo endpoint)
        {
            return this.GetChannel<TChannel>(endpoint, CoreBindingBuilder.BasicHttpBinding, MicroKernelConstant.HttpsDefaultEndpointAddress);
        }

        /// <summary>
        /// Provide the entry of getting a channel to special address 
        /// </summary>
        /// <typeparam name="TChannel">the channel object type</typeparam>
        /// <param name="endpoint">the endpoint information</param>
        /// <param name="binding">binding of the endpoint</param>
        /// <param name="defaultEndpointAddress">the default endpoint address</param>
        /// <returns>the created channel</returns>
        TChannel GetChannel<TChannel>(EndpointInfo endpoint, Binding binding, String defaultEndpointAddress)
        {
            ChannelFactory<TChannel> channelFactory;
            if (!String.IsNullOrEmpty(endpoint.EndpointConfigurationName))
            {
                channelFactory = new ChannelFactory<TChannel>(endpoint.EndpointConfigurationName);
                var customBinding = channelFactory.Endpoint.Binding as CustomBinding;
                if (customBinding != null)
                    customBinding.Elements.Find<SslStreamSecurityBindingElement>().IdentityVerifier = new CoreChannelIdentityVerifier();
            }
            else
            {
                channelFactory = new ChannelFactory<TChannel>(binding);

                channelFactory.Endpoint.Address = String.IsNullOrEmpty(endpoint.EndpointAddress) ? new EndpointAddress(defaultEndpointAddress) : new EndpointAddress(endpoint.EndpointAddress);
                if (channelFactory.Endpoint.Binding is CustomBinding)
                {
                    if (channelFactory.Credentials != null)
                    {
                        channelFactory.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, endpoint.X509CertificateValidationThumbprintFindValue ?? MicroKernelConstant.DefaultThumbprint);
                        channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
                        channelFactory.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new CoreChannelX509CertificateValidator(endpoint.X509CertificateValidationThumbprintFindValue);
                    }
                }
            }
            var uriBuilder = new UriBuilder(
                endpoint.Scheme,
                endpoint.HostOrIpAddress,
                endpoint.Port,
                channelFactory.Endpoint.Address.Uri.AbsolutePath);
            channelFactory.Endpoint.Address = new EndpointAddress(uriBuilder.Uri.ToString());
            return channelFactory.CreateChannel();
        }
    }
}
