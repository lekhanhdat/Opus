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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    using Contract;
    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

#pragma warning disable 1587
    /// <summary>
    /// The MicroKernel API, this class generate the proxy which will be invoke the microkernel component
    /// </summary>
    /// <example> The following is the usage of the IProxyBuilder Interface and RemotingProxyBuilder class
    /// <code>
    ///     static void TestProxyBulider()
    ///     {
    ///          IProxyBuilder proxyBuilder = new RemotingProxyBuiler();
    ///          var endPoint = new CoreServiceEndpointInfo
    ///          {
    ///               EndpointConfigurationName = "AgentCoreService",
    ///               HostOrIpAddress = "localhost",
    ///               Port = 8080,
    ///               Scheme = net.tcp,
    ///               RemotingTypeKey = null
    ///          }
    ///          var agentService = proxyBuilder.CreateProxy<IAAgentService>(endPoint);
    ///     }
    /// </code>
    /// </example>
#pragma warning restore 1587
#pragma warning disable 1591
    public class RemotingProxyBuilder : IProxyBuilder
#pragma warning restore 1591
    {
        #region IProxyBuilder Members
        /// <summary>
        /// provide a way to do additional work before sending message by MicroKernel
        /// </summary>
        public event EventHandler<ProxyEventArgs> PreProxyInvoke;

        /// <summary>
        /// provide a way to do additional work post sending message by MicroKernel
        /// </summary>
        public event EventHandler<ProxyEventArgs> PostProxyInvoke;

        /*
         * This method is the main method of the class RemotingProxyBuilder, it use the
         * dot net remoting technology to create a transparency proxy
         */

        /// <summary>
        /// Provide the ability of the interface to create a instance to connect to the
        /// remote service and invoke the interface implement.
        /// </summary>
        /// <typeparam name="TInterface">the interface type</typeparam>
        /// <param name="endpoint">the endpoint indicate where the service is</param>
        /// <returns>the interface instance</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public TInterface CreateProxy<TInterface>(CoreServiceEndpointInfo endpoint)
        {
            var endpointInfo = new EndpointInfo(endpoint);
            var remoteProxy = new RemotingProxy<TInterface>(endpointInfo);
            remoteProxy.PreProxyInvoke += this.PreProxyInvoke;
            remoteProxy.PostProxyInvoke += this.PostProxyInvoke;
            return (TInterface)remoteProxy.GetTransparentProxy();
        }

        /// <summary>
        /// Provide the ability of the interface to create a instance to connect to the
        /// remote service and invoke the interface implement.
        /// </summary>
        /// <param name="interfaceType">interface type in type System.Type</param>
        /// <param name="endpoint">the endpoint indicate where the service is</param>
        /// <returns>the interface instance</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Object CreateProxy(Type interfaceType, CoreServiceEndpointInfo endpoint)
        {
            var openGenericMethodCreateProxy = this.GetType().GetMethod("CreateProxy", new[] { typeof(CoreServiceEndpointInfo) });
            var closeGenericMethodCreateProxy = openGenericMethodCreateProxy.MakeGenericMethod(interfaceType);
            return closeGenericMethodCreateProxy.Invoke(this, new Object[] { endpoint });
        }

        /// <summary>
        /// Provide the ability of the interface to create a instance to connect to the
        /// remote service and invoke the interface implement.
        /// </summary>
        /// <param name="interfaceTypeAssemblyQualifiedName">the interface type in System.String</param>
        /// <param name="endpoint">the endpoint indicate where the service is</param>
        /// <returns>the interface instance</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Object CreateProxy(String interfaceTypeAssemblyQualifiedName, CoreServiceEndpointInfo endpoint)
        {
            return this.CreateProxy(Type.GetType(interfaceTypeAssemblyQualifiedName), endpoint);
        }

        #endregion
    }
}