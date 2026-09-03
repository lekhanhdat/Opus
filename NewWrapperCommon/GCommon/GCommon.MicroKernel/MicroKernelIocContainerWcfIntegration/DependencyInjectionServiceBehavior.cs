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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class DependencyInjectionServiceBehavior<TIocContainer> : IServiceBehavior
    {
        readonly TIocContainer container;

        public DependencyInjectionServiceBehavior(TIocContainer container)
        {
            this.container = container;
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        { }

        public void AddBindingParameters(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, 
            BindingParameterCollection bindingParameters)
        { }

        /// <summary>
        /// NOTES: You must know the concept of Channel Dispatcher and the endpoint dispatcher,
        /// NOTES: The two concepts are the basic concepts of windows communication foundation 
        /// NOTES: framework, the message dispatcher model use this two concepts as implementations
        /// </summary>
        /// <param name="serviceDescription">the service description object of specific service</param>
        /// <param name="serviceHostBase">the underline communication object</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                {
                    if (endpointDispatcher.ContractName != "IMetadataExchange"
                        && endpointDispatcher.ContractName != "IHttpErrorHandler"
                        && endpointDispatcher.ContractName != "IHttpGetHelpPageAndMetadataContract")
                    {
                        var contractName = endpointDispatcher.ContractName;
                        var serviceEndpointResult = default(ServiceEndpoint);
                        foreach (var serviceEndPointItem in serviceDescription.Endpoints)
                        {
                            if (serviceEndPointItem.Contract.Name == contractName)
                            {
                                serviceEndpointResult = serviceEndPointItem;
                                break;
                            }
                        }
                        if (serviceEndpointResult != null)
                            endpointDispatcher.DispatchRuntime.InstanceProvider 
                                = new IocInstanceProvider<TIocContainer>(this.container, serviceEndpointResult.Contract.ContractType);
                    }
                }
            }
        }
    }
}
