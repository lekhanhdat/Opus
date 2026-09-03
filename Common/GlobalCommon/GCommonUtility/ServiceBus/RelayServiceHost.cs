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
namespace AvePoint.GCommon.Utility.ServiceBus
{
    using AvePoint.GCommon.Contract.AvePointService;
    using AvePoint.GCommon.Utility.AvePointService;
    using Microsoft.ServiceBus;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading.Tasks;

    internal class RelayServiceHost : IServiceBusHost, IAresRemoteEventReceiver
    {
        private Func<AresRemoteEventProperty, AresRemoteEventResultDto> syncFunc = null;
        private Action<AresRemoteEventProperty> asyncFunc = null;
        private SbConnectionDto connectionDto = null;

        public RelayServiceHost(Action<AresRemoteEventProperty> asyncFunc, Func<AresRemoteEventProperty, AresRemoteEventResultDto> syncFunc)
        {
            this.syncFunc += syncFunc;
            this.asyncFunc += asyncFunc;
        }

        public void StartHost()
        {
            //ServiceHost serviceHost = new ServiceHost(typeof(RelayServiceHost));
            //ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(IAresRemoteEventReceiver), new NetTcpRelayBinding(),
            //        ServiceBusEnvironment.CreateServiceUri(connectionDto.Schema, connectionDto.Namespace, connectionDto.SbPath));
            //endpoint.EndpointBehaviors.Add(
            //        new TransportClientEndpointBehavior()
            //        {
            //            TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionDto.SAKeyName, connectionDto.SAKey)
            //        });
            //serviceHost.Open();
        }

        public AresRemoteEventResultDto ProcessEvent(AresRemoteEventProperty properties)
        {
            return syncFunc(properties);
        }

        public void ProcessOneWayEvent(AresRemoteEventProperty properties)
        {
            asyncFunc(properties);
        }

        public void SafeStopHost()
        {

        }
    }

    internal interface IAresRemoteEventReceiver
    {
        [OperationContract]
        AresRemoteEventResultDto ProcessEvent(AresRemoteEventProperty properties);

        [OperationContract]
        void ProcessOneWayEvent(AresRemoteEventProperty properties);
    }
}
