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
using System.IdentityModel.Policy;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.Factory
{
    public class WCFChannelFactory<T> : IDisposable
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(WCFChannelFactory<T>), false);

        #region Private Fields
        private WCFInitMethod channelInitMethod = WCFInitMethod.UnSupported;
        private string serviceConfigurationName;
        private string serviceConfigurationAddress;
        private int serviceConfigurationPort;
        private string serviceConfigurationSchema;
        private string jobId;

        private Binding binding;
        private EndpointAddress endpointAddress;
        private ChannelFactory<T> channelFactory;
        private TransferCommunicationSettings communicationSettings;
        #endregion

        internal WCFInitMethod ChannelInitMethod
        {
            get { return channelInitMethod; }
        }
        public string ServiceConfigurationName
        {
            get { return serviceConfigurationName; }
        }
        public string ServiceConfigurationAddress
        {
            get { return serviceConfigurationAddress; }
        }
        public int ServiceConfigurationPort
        {
            get { return serviceConfigurationPort; }
        }
        public string ServiceConfigurationSchema
        {
            get { return serviceConfigurationSchema; }
        }
        public string JobId
        {
            get { return jobId; }
        }
        public Binding Binding
        {
            get { return binding; }
        }
        public EndpointAddress EndpointAddress
        {
            get { return endpointAddress; }
        }
        public ChannelFactory<T> ChannelFactory
        {
            get { return channelFactory; }
            set { channelFactory = value; }
        }
        public TransferCommunicationSettings CommunicationSettings
        {
            get { return communicationSettings; }
        }

        public WCFChannelFactory(string serviceConfigurationName)
        {
            channelInitMethod = WCFInitMethod.ServiceConfigurationName;
            this.serviceConfigurationName = serviceConfigurationName;
        }

        public WCFChannelFactory(Binding binding, EndpointAddress endpointAddr)
        {
            channelInitMethod = WCFInitMethod.BindingAndEndpointAddress;
            this.binding = binding;
            this.endpointAddress = endpointAddr;
        }

        public WCFChannelFactory(string agentAddress, int port, string relatedBaseUri, string serviceName, string jobId)
        {
            Uri uri = UriUtility.CreateUri(DataTransferConfiguration.UriSchema, agentAddress, port, relatedBaseUri, serviceName, jobId);
            this.endpointAddress = new EndpointAddress(uri);
            //NetTcpBinding newBinding = new NetTcpBinding();
            //newBinding.Security.Mode = SecurityMode.None;
            ////********************************************
            ////wdz 临时增加，由于没有配置文件，而默认的值在传输数据的时候小于64k，所以暂时用代码配置，否则传输数据会异常。
            //newBinding.MaxBufferSize = 536870912;
            //newBinding.MaxReceivedMessageSize = 536870912;
            //newBinding.ReaderQuotas.MaxStringContentLength = 536870912;
            //newBinding.ReaderQuotas.MaxArrayLength = 536870912;
            this.binding = DataTransferConfiguration.DefaultDataBinding;//newBinding;
            //********************************************
            this.channelInitMethod = WCFInitMethod.BindingAndEndpointAddress;
        }

        public WCFChannelFactory(TransferCommunicationSettings communicationSettings, string serviceName)
        {
            this.communicationSettings = communicationSettings;
            switch (communicationSettings.Mode)
            {
                case TransferConfigurationLoadMode.Manual:
                    {
                        var uri = UriUtility.CreateUri(communicationSettings.UriSchema, communicationSettings.ServiceAddress,
                            communicationSettings.ServicePort, communicationSettings.RelatedBaseUri, serviceName, communicationSettings.JobId);
                        endpointAddress = new EndpointAddress(uri);
                        if (communicationSettings.EndPointBinding == null)
                        {
                            //var newBinding = new NetTcpBinding();
                            //newBinding.Security.Mode = SecurityMode.None;
                            //newBinding.MaxBufferSize = 536870912;
                            //newBinding.MaxReceivedMessageSize = 536870912;
                            //newBinding.ReaderQuotas.MaxStringContentLength = 536870912;
                            //newBinding.ReaderQuotas.MaxArrayLength = 536870912;
                            this.binding = DataTransferConfiguration.DefaultDataBinding;//newBinding;
                        }
                        else
                        {
                            this.binding = communicationSettings.EndPointBinding;
                        }
                        this.channelInitMethod = WCFInitMethod.BindingAndEndpointAddress;
                    }
                    break;
                case TransferConfigurationLoadMode.Automatic:
                    {
                        this.channelInitMethod = WCFInitMethod.ServiceConfigurationName;
                        this.serviceConfigurationName = communicationSettings.ConfigurationName;
                        this.serviceConfigurationSchema = communicationSettings.UriSchema;
                        this.serviceConfigurationAddress = communicationSettings.ServiceAddress;
                        this.serviceConfigurationPort = communicationSettings.ServicePort;
                        this.jobId = communicationSettings.JobId;
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual void InitChannelFactory()
        {
            PermissiveCertificatePolicy.Enact();
            switch (channelInitMethod)
            {
                case WCFInitMethod.BindingAndEndpointAddress:
                    {
                        channelFactory = new ChannelFactory<T>(binding, endpointAddress);
                    }
                    break;
                case WCFInitMethod.ServiceConfigurationName:
                    {
                        channelFactory = new ChannelFactory<T>(serviceConfigurationName);
                        var uri = UriUtility.CreateUri(channelFactory.Endpoint.Address.Uri.Scheme, serviceConfigurationAddress, serviceConfigurationPort, channelFactory.Endpoint.Address.Uri.AbsolutePath, jobId);
                        channelFactory.Endpoint.Address = new EndpointAddress(uri.ToString());
                    }
                    break;
                default:
                    {
                        throw new Exception("Unsupported Channel init method:" + channelInitMethod.ToString());
                    }
            }

            logger.Info("EndPoint Address:{0}", channelFactory.Endpoint.Address.Uri.ToString());

            DateTransferCustomIdentityVerifer.Plug(channelFactory);
        }

        public virtual T CreateChannel()
        {
            Dispose();
            InitChannelFactory();
            return channelFactory.CreateChannel();
        }

        #region IDisposable Members

        public void Dispose()
        {
            ObjectUtility.CloseChannel(channelFactory);
            channelFactory = null;
        }

        #endregion

        internal enum WCFInitMethod
        {
            UnSupported,
            ServiceConfigurationName,
            BindingAndEndpointAddress,
        }
    }

    public class WCFDuplexChannelFactory<T> : WCFChannelFactory<T>
    {
        private InstanceContext instance;

        public WCFDuplexChannelFactory(InstanceContext instance, string agentAddress, int port, string relatedBaseUri, string serviceName, string jobId)
            : base(agentAddress, port, relatedBaseUri, serviceName, jobId)
        {
            this.instance = instance;
        }

        public WCFDuplexChannelFactory(InstanceContext instance, TransferCommunicationSettings communicationSettings, string serviceName)
            : base(communicationSettings, serviceName)
        {
            this.instance = instance;
        }

        public override void InitChannelFactory()
        {
            PermissiveCertificatePolicy.Enact();
            switch (ChannelInitMethod)
            {
                case WCFInitMethod.BindingAndEndpointAddress:
                    {
                        ChannelFactory = new DuplexChannelFactory<T>(instance, Binding, EndpointAddress);
                    }
                    break;
                case WCFInitMethod.ServiceConfigurationName:
                    {
                        ChannelFactory = new DuplexChannelFactory<T>(instance, ServiceConfigurationName);
                        var uri = UriUtility.CreateUri(ChannelFactory.Endpoint.Address.Uri.Scheme, ServiceConfigurationAddress, ServiceConfigurationPort, ChannelFactory.Endpoint.Address.Uri.AbsolutePath, JobId);
                        ChannelFactory.Endpoint.Address = new EndpointAddress(uri.ToString());
                    }
                    break;
                default:
                    {
                        throw new Exception("Unsupported Channel init method:" + ChannelInitMethod.ToString());
                    }
            }
            logger.Debug("EndPoint Address:{0}", ChannelFactory.Endpoint.Address.Uri.ToString());
            DateTransferCustomIdentityVerifer.Plug(ChannelFactory);
        }
    }

    internal class DateTransferCustomIdentityVerifer : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            return true;
        }
        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            identity = null;
            return true;
        }

        public static void Plug(ChannelFactory cf)
        {
            if (cf.Endpoint.Binding is CustomBinding)
            {
                SslStreamSecurityBindingElement bindingElement = (cf.Endpoint.Binding as CustomBinding).Elements.Find<SslStreamSecurityBindingElement>();
                if (bindingElement != null)
                {
                    bindingElement.IdentityVerifier = new DateTransferCustomIdentityVerifer();
                }
            }
        }
    }
}
