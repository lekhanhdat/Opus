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
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Reflection;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using AvePoint.GCommon;



namespace AvePoint.Common
{
    public class MessageInspectorBehaviorExtensionElement : BehaviorExtensionElement
    {

        public override Type BehaviorType
        {
            get { return typeof(MessageInspectorServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new MessageInspectorServiceBehavior();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageInspectorServiceBehavior : Attribute, IServiceBehavior
    {

        public MessageInspectorServiceBehavior()
        {
        }

        #region IServiceBehavior Members

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
                                          System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (var channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if (channelDispatcher == null)
                {
                    continue;
                }

                foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                {
                    if (string.Compare(endpointDispatcher.ContractName, "IHttpGetHelpPageAndMetadataContract", true) == 0
                        || string.Compare(endpointDispatcher.ContractName, "IMetadataExchange", true) == 0)
                    {
                        continue;
                    }
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CustomizeMessageInspector(endpointDispatcher.ContractName));
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription,
                                         System.ServiceModel.ServiceHostBase serviceHostBase,
                                         System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
                                         System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // Not used in this behavior.
        }

        public void Validate(ServiceDescription serviceDescription,
                             System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            // Not used in this behavior.
        }

        #endregion
    }

    public class CustomizeMessageInspector : IDispatchMessageInspector
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string mContractName;
        
        public CustomizeMessageInspector(string contractName)
        {
            mContractName = contractName;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            request = TraceMessage(request.CreateBufferedCopy(int.MaxValue), true);
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            reply = TraceMessage(reply.CreateBufferedCopy(int.MaxValue), false);
        }

        private Message TraceMessage(MessageBuffer buffer, bool preRealCall)
        {
            Message msg = buffer.CreateMessage();
            if (preRealCall)
            {
                string logMsg = string.Format("Contract Name:{0}\tRequest Time:{1}\tRequest Message:\n{2}", mContractName, DateTime.Now.ToString(), msg.ToString());
                mLog.Info(logMsg);
            }
            else
            {
                string logMsg = string.Format("Contract Name:{0}\tReply Time:{1}\tReply Message:\n{2}", mContractName, DateTime.Now.ToString(), msg.ToString());
                mLog.Info(logMsg);
            }
            return buffer.CreateMessage();
        }
    }

}
