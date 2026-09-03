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
using System.ServiceModel;
using System.ServiceModel.Description;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Transfer.MQ.Channel;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.MQ.Service;
using AvePoint.GCommon.Transfer.MQ.Interface;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Transfer.MQ;
using System.ServiceModel.Channels;
using System.Configuration;

namespace AvePoint.GCommon.Transfer.Factory
{
    public class WCFServiceHostFactory
    {
        private static readonly AveLogger mLog = new AveLogger(typeof(WCFServiceHostFactory), false);

        private static readonly List<ServiceHost> mServiceHosts = new List<ServiceHost>();

        /// <summary>
        /// 初始化WCF Host, StreamMode use http by default.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="type"></param>
        public static void Init(string address, int port, string relatedBaseUri, string jobId, WCFServiceHostType type)
        {
            Init(address, port, relatedBaseUri, jobId, type, DataTransferGlobalConfig.DataTransferConfiguration.EnableSsl);
        }

        /// <summary>
        /// 初始化WCF Host, StreamMode use http by default.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="type"></param>
        /// <param name="enableSsl"></param>
        public static void Init(string address, int port, string relatedBaseUri, string jobId, WCFServiceHostType type, bool enableSsl)
        {
            mServiceHosts.AddRange(CreateServiceHost(address, port, relatedBaseUri, jobId, type, enableSsl));
        }

        /// <summary>
        /// Create Service Host
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="type"></param>
        /// <param name="enableSsl"></param>
        /// <returns></returns>
        public static List<ServiceHost> CreateServiceHost(string address, int port, string relatedBaseUri, string jobId, 
            WCFServiceHostType type, bool enableSsl)
        {
            var hosts = new List<ServiceHost>();

            if ((type & WCFServiceHostType.MQ) == WCFServiceHostType.MQ)
            {
                hosts.Add(CreateServiceHost(typeof(AveMQWCFService), typeof(IMQWCFService), 
                    DataTransferGlobalConfig.DataTransferConfiguration.MqUriSchema, address, port,
                    relatedBaseUri, WCFServiceHostType.MQ.ToString(), jobId, enableSsl));
                hosts.Add(CreateServiceHost(typeof(AveMQWCFServiceOneWay), typeof(IMQWCFServiceOneWay), 
                    DataTransferGlobalConfig.DataTransferConfiguration.MqUriSchema, address, port, 
                    relatedBaseUri, WCFServiceHostType.MQOneWay.ToString(), jobId, enableSsl));
            }
            if ((type & WCFServiceHostType.FileTransfer) == WCFServiceHostType.FileTransfer)
            {
                hosts.Add(CreateServiceHost(typeof(FileTransferService), typeof(IFileTransferService), 
                    DataTransferGlobalConfig.DataTransferConfiguration.FileTransferServiceUriSchema, 
                    address, port, relatedBaseUri, WCFServiceHostType.FileTransfer.ToString(), jobId, enableSsl));
            }
            if ((type & WCFServiceHostType.DataTransfer) == WCFServiceHostType.DataTransfer)
            {
                hosts.Add(CreateServiceHost(typeof(RelayService), typeof(IRelay), 
                    DataTransferGlobalConfig.DataTransferConfiguration.RelayServiceUriSchema,
                    address, port, relatedBaseUri, WCFServiceHostType.DataTransfer.ToString(), jobId, enableSsl));
            }
            if ((type & WCFServiceHostType.DataTransferStreaming) == WCFServiceHostType.DataTransferStreaming)
            {
                hosts.Add(CreateServiceHost(typeof(StreamModeService), typeof(IStreamRelay), 
                    DataTransferGlobalConfig.DataTransferConfiguration.StreamModeServiceUriSchema, 
                    address, DataTransferGlobalConfig.DataTransferConfiguration.HttpModePort, relatedBaseUri,
                    WCFServiceHostType.DataTransferStreaming.ToString(), jobId, enableSsl));
            }

            return hosts;
        }

        /// <summary>
        /// Start Hosting
        /// </summary>
        public static void StartHosting()
        {
            StartHosting(mServiceHosts);
        }

        /// <summary>
        /// Start Hosting
        /// </summary>
        /// <param name="hosts"></param>
        public static void StartHosting(List<ServiceHost> hosts)
        {
            try
            {
                foreach (var sh in hosts)
                {
                    var addresses = new StringBuilder();
                    foreach (ServiceEndpoint endpoint in sh.Description.Endpoints)
                    {
                        addresses.Append(endpoint.Address.Uri.ToString());
                        addresses.Append("\t");
                        addresses.Append(WCFSharedConfiguration.BindingConfigurationToString(endpoint.Binding));
                        addresses.Append("\t");
                    }
                    try
                    {

                        if (sh.State == CommunicationState.Created)
                        {
                            mLog.Info(string.Format("Begin to host service: {0}  Address: {1}", sh.Description.ServiceType.ToString(), addresses.ToString()));
                            sh.Open();
                            mLog.Info(string.Format("Successfully host service: {0} Address: {1}", sh.Description.ServiceType.ToString(), addresses.ToString()));
                        }
                        else
                        {
                            mLog.Info(string.Format("The host service: {0} Address: {1} is already {2}.", sh.Description.ServiceType.ToString(), addresses.ToString(), sh.State));
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Exception occurs when start hosting {0}. Exception details: {1}", sh.Description.ServiceType.ToString(), ex.ToString(), addresses.ToString());
                        //throw exception with error code, the error information is printted  outside.
                        mLog.Error(errorMsg);
                        //throw;
                    }

                }
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("An error occurred while doing service hosting. Exception: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Stop Hosting
        /// </summary>
        public static void StopHosting()
        {
            StopHosting(mServiceHosts);
        }

        /// <summary>
        /// Stop hosting
        /// </summary>
        public static void StopHosting(List<ServiceHost> hosts)
        {
            foreach (ServiceHost sh in hosts)
            {
                try
                {
                    if (sh.State != CommunicationState.Closed)
                    {
                        mLog.Info(string.Format("Begin to close service: {0}", sh.Description.ServiceType.ToString()));
                        sh.Abort();
                        mLog.Info(string.Format("Successfully close service: {0}", sh.Description.ServiceType.ToString()));
                    }
                    else
                    {
                        mLog.Info(string.Format("Service [{0}] is already in Closed state.", sh.Description.ServiceType.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = string.Format("Exception occurs when stopping hosting {0}. Exception details: {1}", sh.Description.ServiceType.ToString(), ex.ToString());
                    mLog.Error(errorMsg);
                }
            }
        }

        internal static ServiceHost CreateServiceHost(Type serviceType, Type interfaceType, string schema, string address, int port, string relatedBaseUri, string serviceName, string jobId, bool enableSsl)
        {
            var baseAddresses = new List<Uri>();

            baseAddresses.Add(UriUtility.CreateUri(schema, address, port, relatedBaseUri, serviceName, jobId));

            ServiceHost sh = null;

            var times = 3;
            while (true)
            {
                try
                {
                    sh = new WcfConfigurationServiceHost(serviceType, DataTransferGlobalConfig.BuiltInConfiguration, baseAddresses.ToArray());
                    break;
                }
                catch (ConfigurationErrorsException ex)
                {
                    DataTransferGlobalConfig.ReloadConfiguration();
                    times--;

                    DataTransferLogger.Logger(AveLogLevel.DEBUG, "create service host with address:{0} failed:{1}", baseAddresses[0], ex);

                    if(times < 0)
                    {
                        throw;
                    }
                }
            }

            if(!enableSsl)
            {
                sh.Description.Behaviors.Remove(typeof(ServiceCredentials));
                if(sh.Description.Endpoints.Count > 0)
                {
                    foreach(var endpoint in sh.Description.Endpoints)
                    {
                        if (endpoint.Binding is BasicHttpBinding)
                        {
                            ((BasicHttpBinding)endpoint.Binding).Security.Mode = BasicHttpSecurityMode.None;
                        }
                        else if (endpoint.Binding is NetTcpBinding)
                        {
                            ((NetTcpBinding)endpoint.Binding).Security.Mode = SecurityMode.None;
                        }
                        else if(endpoint.Binding is CustomBinding)
                        {
                            var customBinding = (CustomBinding)endpoint.Binding;
                            for(var i= 0; i< customBinding.Elements.Count; i++)
                            {
                                if(customBinding.Elements[i] is SslStreamSecurityBindingElement)
                                {
                                    customBinding.Elements.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return sh;
        }
    }

    /// <summary>
    /// WCF Service Host which can load configuration from speical configuration file
    /// </summary>
    internal class WcfConfigurationServiceHost : ServiceHost
    {
        /// <summary>
        /// configuration
        /// </summary>
        private Configuration configuration = null;

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ServiceHost class with
        /// the instance of the service and its base addresses specified.
        /// </summary>
        /// <param name="singletonInstance">The instance of the hosted service.</param>
        /// <param name="configuration">configuration</param>
        /// <param name="baseAddresses">An System.Array of type System.Uri that contains the base addresses for the
        /// hosted service.</param>
        /// <exception cref="System.ArgumentNullException">singletonInstance is null or configuration file is null</exception>
        public WcfConfigurationServiceHost(object singletonInstance, Configuration configuration, params Uri[] baseAddresses)
            : base()
        {
            VerifyConfiguration(configuration);
            this.InitializeDescription(singletonInstance, new UriSchemeKeyedCollection(baseAddresses));
        }

        /// <summary>
        /// Initializes a new instance of the System.ServiceModel.ServiceHost class with
        /// the type of service and its base addresses specified.
        /// </summary>
        /// <param name="serviceType">The type of hosted service.</param>
        /// <param name="configuration">configuration</param>
        /// <param name="baseAddresses">An System.Array of type System.Uri that contains the base addresses for the
        /// hosted service.</param>
        /// <exception cref="System.ArgumentNullException">serviceType is null.</exception>
        public WcfConfigurationServiceHost(Type serviceType, Configuration configuration, params Uri[] baseAddresses)
            : base()
        {
            VerifyConfiguration(configuration);
            this.InitializeDescription(serviceType, new UriSchemeKeyedCollection(baseAddresses));
        }

        /// <summary>
        /// Verify Configuration file
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException">configurationFile is null</exception>        
        private void VerifyConfiguration(Configuration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            this.configuration = configuration;
        }

        /// <summary>
        /// Loads the service description information from the configuration file and
        /// applies it to the runtime being constructed.
        /// </summary>
        protected override void ApplyConfiguration()
        {
            if (this.Description == null)
            {
                throw new InvalidOperationException("Cannot host service without description.");
            }

            var serviceModel = System.ServiceModel.Configuration.ServiceModelSectionGroup.GetSectionGroup(configuration);
            bool loaded = false;
            foreach (System.ServiceModel.Configuration.ServiceElement service in serviceModel.Services.Services)
            {
                if (service.Name.Equals(this.Description.ConfigurationName, StringComparison.Ordinal))
                {
                    base.LoadConfigurationSection(service);
                    loaded = true;
                    break;
                }
            }
            if (!loaded)
            {
                throw new ArgumentException(string.Format("Cannot find service in configuration file:{0} by name:{1}", configuration.FilePath, this.Description.ConfigurationName));
            }
            this.EnsureAuthorization(this.Description);
            this.EnsureDebug(this.Description);
        }

        /// <summary>
        /// Ensure Authorization
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private ServiceAuthorizationBehavior EnsureAuthorization(ServiceDescription description)
        {
            var serviceAuthorizationBehavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
            if (serviceAuthorizationBehavior == null)
            {
                serviceAuthorizationBehavior = new ServiceAuthorizationBehavior();
                description.Behaviors.Add(serviceAuthorizationBehavior);
            }
            return serviceAuthorizationBehavior;
        }

        /// <summary>
        /// Ensure Debugn Service Behavior
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        private ServiceDebugBehavior EnsureDebug(ServiceDescription description)
        {
            var serviceDebugBehavior = description.Behaviors.Find<ServiceDebugBehavior>();
            if (serviceDebugBehavior == null)
            {
                serviceDebugBehavior = new ServiceDebugBehavior();
                description.Behaviors.Add(serviceDebugBehavior);
            }
            return serviceDebugBehavior;
        }
    }

    /// <summary>
    /// DataTransfer ServiceHost Factory
    /// </summary>
    public class DataTransferServiceHostFactory : System.ServiceModel.Activation.ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new WcfConfigurationServiceHost(serviceType, DataTransferGlobalConfig.BuiltInConfiguration, baseAddresses);
        }
    }

    /// <summary>
    /// 初始化ServiceHost的几个类型
    /// 不能使用int的最高bit位。
    /// </summary>
    [Flags]
    public enum WCFServiceHostType
    {
        /// <summary>
        /// DataTransfer
        /// </summary>
        DataTransfer = 0x01,
        /// <summary>
        /// MQ
        /// </summary>
        MQ = 0x02,
        /// <summary>
        /// MQOneWay
        /// </summary>
        MQOneWay = 0x04,
        /// <summary>
        /// FileTransfer
        /// </summary>
        FileTransfer = 0x08,
        /// <summary>
        /// DataTransfer Streaming
        /// </summary>
        DataTransferStreaming = 0x10,
        /// <summary>
        /// All
        /// </summary>
        ALL = 0x7FFFFFFF,
    }
}
