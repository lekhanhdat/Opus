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
using System.Configuration;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using AvePoint.GCommon.Transfer.Common;
using System.ServiceModel.Security;
using System.IdentityModel.Policy;
using AvePoint.GCommon.Transfer.MQ;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase", Scope = "type", Target = "AvePoint.GCommon.Transfer.Factory.WCFVirtualChannelFactory`1")]
namespace AvePoint.GCommon.Transfer.Factory
{
    public class WcfChannelFactory<T> : IDisposable
    {
        protected static AveLogger Logger = AveLogger.GetInstance(typeof(WcfChannelFactory<T>), false);

        protected string lastEndPointUri;

        private Type configurationChannelFactoryType;

        #region Private Fields
        protected WCFInitMethod mChannelInitMethod = WCFInitMethod.UnSupported;
        protected string mServiceConfigurationName;
        protected string mServiceConfigurationAddress;
        protected int mServiceConfigurationPort;
        protected string mServiceConfigurationSchema;
        protected string mJobId;

        protected Binding mBinding;
        protected EndpointAddress mEndpointAddress;
        protected ChannelFactory<T> mChannelFactory;
        protected TransferCommunicationSettings mCommunicationSettings;
        protected bool mEnableSsl;
        #endregion

        public WCFInitMethod ChannelInitMethod
        {
            get { return mChannelInitMethod; }
        }
        public string ServiceConfigurationName
        {
            get { return mServiceConfigurationName; }
        }
        public string ServiceConfigurationAddress
        {
            get { return mServiceConfigurationAddress; }
        }
        public int ServiceConfigurationPort
        {
            get { return mServiceConfigurationPort; }
        }
        public string ServiceConfigurationSchema
        {
            get { return mServiceConfigurationSchema; }
        }
        public string JobId
        {
            get { return mJobId; }
        }
        public Binding Binding
        {
            get { return mBinding; }
        }
        public EndpointAddress EndpointAddress
        {
            get { return mEndpointAddress; }
        }
        public ChannelFactory<T> ChannelFactory
        {
            get { return mChannelFactory; }
            set { mChannelFactory = value; }
        }
        public TransferCommunicationSettings CommunicationSettings
        {
            get { return mCommunicationSettings; }
        }

        public WcfChannelFactory(string uriSchema, string agentAddress, int port, string relatedBaseUri, string serviceName, string jobId, string configurationName, bool enableSsl)
        {
            Uri uri = UriUtility.CreateUri(uriSchema, agentAddress, port, relatedBaseUri, serviceName, jobId);
            this.mEndpointAddress = new EndpointAddress(uri);
            this.mServiceConfigurationName = configurationName;
            this.mChannelInitMethod = WCFInitMethod.BindingAndEndpointAddress;
            this.mEnableSsl = enableSsl;
            this.EnsureType();
        }

        public WcfChannelFactory(TransferCommunicationSettings communicationSettings, string serviceName)
        {
            this.mCommunicationSettings = communicationSettings;
            this.mEnableSsl = this.mCommunicationSettings.EnableSsl;
            switch (communicationSettings.Mode)
            {
                case TransferConfigurationLoadMode.Manual:
                    {
                        var uri = UriUtility.CreateUri(communicationSettings.UriSchema, communicationSettings.ServiceAddress,
                            communicationSettings.ServicePort, communicationSettings.RelatedBaseUri, serviceName, communicationSettings.JobId);
                        mEndpointAddress = new EndpointAddress(uri);
                        this.mServiceConfigurationName = communicationSettings.ConfigurationName;
                        this.mChannelInitMethod = WCFInitMethod.BindingAndEndpointAddress;
                    }
                    break;
                case TransferConfigurationLoadMode.Automatic:
                    {
                        this.mChannelInitMethod = WCFInitMethod.ServiceConfigurationName;
                        this.mServiceConfigurationName = communicationSettings.ConfigurationName;
                        this.mServiceConfigurationSchema = communicationSettings.UriSchema;
                        this.mServiceConfigurationAddress = communicationSettings.ServiceAddress;
                        this.mServiceConfigurationPort = communicationSettings.ServicePort;
                        this.mJobId = communicationSettings.JobId;
                    }
                    break;
                default:
                    break;
            }
            this.EnsureType();
        }

        private void EnsureType()
        {
            var genericType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Configuration.ConfigurationChannelFactory`1", false);
            if (genericType != null && genericType.IsGenericType)
            {
                this.configurationChannelFactoryType = genericType.MakeGenericType(typeof(T));
            }
        }

        public virtual void InitChannelFactory()
        {
            PermissiveCertificatePolicy.Enact();
            switch (mChannelInitMethod)
            {
                case WCFInitMethod.BindingAndEndpointAddress:
                    {
                        var times = 3;
                        while (true)
                        {
                            try
                            {
                                if (configurationChannelFactoryType != null)
                                {
                                    mChannelFactory = (ChannelFactory<T>)Activator.CreateInstance(configurationChannelFactoryType, 
                                        new object[] { mServiceConfigurationName, DataTransferGlobalConfig.BuiltInConfiguration, mEndpointAddress });
                                }
                                else
                                { 
                                    mChannelFactory = new WcfConfigurationChannelFactory<T>(mServiceConfigurationName, DataTransferGlobalConfig.BuiltInConfiguration, mEndpointAddress);
                                }
                                break;
                            }
                            //由于反射的Excetion和ConfigurationErrorsException是不一样的Exception，所以先使用最大的Exception来覆盖，
                            //如果以后出现问题，再看看怎么处理这个比较好。
                            catch (Exception ex)
                            {
                                DataTransferGlobalConfig.ReloadConfiguration();
                                times--;

                                DataTransferLogger.Logger(AveLogLevel.DEBUG, "create service host with address:{0} failed:{1}", mEndpointAddress, ex);

                                if (times < 0)
                                {
                                    throw;
                                }
                            }
                        }
                        
                        if (!mEnableSsl)
                        {
                            mChannelFactory.Endpoint.Behaviors.Remove(typeof(ClientCredentials));

                            var currentBinding = mChannelFactory.Endpoint.Binding;
                            if (currentBinding is BasicHttpBinding)
                            {
                                ((BasicHttpBinding)currentBinding).Security.Mode = BasicHttpSecurityMode.None;
                            }
                            else if (currentBinding is NetTcpBinding)
                            {
                                ((NetTcpBinding)currentBinding).Security.Mode = SecurityMode.None;
                            }
                            else if (currentBinding is CustomBinding)
                            {
                                var customBinding = (CustomBinding)currentBinding;
                                for (var i = 0; i < customBinding.Elements.Count; i++)
                                {
                                    if (customBinding.Elements[i] is SslStreamSecurityBindingElement)
                                    {
                                        customBinding.Elements.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case WCFInitMethod.ServiceConfigurationName:
                    {
                        mChannelFactory = new ChannelFactory<T>(mServiceConfigurationName);
                        var uri = UriUtility.CreateUri(mChannelFactory.Endpoint.Address.Uri.Scheme, mServiceConfigurationAddress, mServiceConfigurationPort, mChannelFactory.Endpoint.Address.Uri.AbsolutePath, mJobId);
                        mChannelFactory.Endpoint.Address = new EndpointAddress(uri.ToString());
                    }
                    break;
                default:
                    {
                        throw new Exception("Unsupported Channel init method:" + mChannelInitMethod.ToString());
                    }
            }

            if (string.Compare(lastEndPointUri, mChannelFactory.Endpoint.Address.Uri.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                lastEndPointUri = mChannelFactory.Endpoint.Address.Uri.ToString();
                Logger.Info("EndPoint Address:{0}", mChannelFactory.Endpoint.Address.Uri.ToString());
            }

            DateTransferCustomIdentityVerifer.Plug(mChannelFactory);
        }

        public virtual T CreateChannel()
        {
            Dispose();
            InitChannelFactory();
            return mChannelFactory.CreateChannel();
        }

        #region IDisposable Members

        public void Dispose()
        {
            ObjectUtility.CloseChannel(mChannelFactory);
            mChannelFactory = null;
        }

        #endregion

        public enum WCFInitMethod
        {
            UnSupported,
            ServiceConfigurationName,
            BindingAndEndpointAddress,
        }
    }

    public class WcfDuplexChannelFactory<T> : WcfChannelFactory<T>
    {
        private Type configurationDuplexChannelFactoryType;
        private InstanceContext instance;

        public WcfDuplexChannelFactory(InstanceContext instance, string uriSchema, string agentAddress, int port, string relatedBaseUri, string serviceName, string jobId, string configurationName, bool enableSsl)
            : base(uriSchema, agentAddress, port, relatedBaseUri, serviceName, jobId, configurationName, enableSsl)
        {
            this.instance = instance;
            this.EnsureType();
        }

        public WcfDuplexChannelFactory(InstanceContext instance, TransferCommunicationSettings communicationSettings, string serviceName)
            : base(communicationSettings, serviceName)
        {
            this.instance = instance;
            this.EnsureType();
        }

        private void EnsureType()
        {
            var genericType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Configuration.ConfigurationDuplexChannelFactory`1", false);
            if (genericType != null && genericType.IsGenericType)
            {
                this.configurationDuplexChannelFactoryType = genericType.MakeGenericType(typeof(T));
            }
        }

        public override void InitChannelFactory()
        {
            PermissiveCertificatePolicy.Enact();
            switch (ChannelInitMethod)
            {
                case WCFInitMethod.BindingAndEndpointAddress:
                    {
                        var times = 3;
                        while (true)
                        {
                            try
                            {
                                if (configurationDuplexChannelFactoryType != null)
                                {
                                    ChannelFactory = (ChannelFactory<T>)Activator.CreateInstance(configurationDuplexChannelFactoryType, new object[] { instance, mServiceConfigurationName, mEndpointAddress, DataTransferGlobalConfig.BuiltInConfiguration });
                                }
                                else
                                {
                                    ChannelFactory = new WcfConfigurationDuplexChannelFactory<T>(instance, mServiceConfigurationName, EndpointAddress, DataTransferGlobalConfig.BuiltInConfiguration);
                                }
                                break;
                            }
                            //由于反射的Excetion和ConfigurationErrorsException是不一样的Exception，所以先使用最大的Exception来覆盖，
                            //如果以后出现问题，再看看怎么处理这个比较好。
                            catch (Exception ex)
                            {
                                DataTransferGlobalConfig.ReloadConfiguration();
                                times--;

                                DataTransferLogger.Logger(AveLogLevel.DEBUG, "create service host with address:{0} failed:{1}", EndpointAddress, ex);

                                if (times < 0)
                                {
                                    throw;
                                }
                            }
                        }

                        if (!mEnableSsl)
                        {
                            ChannelFactory.Endpoint.Behaviors.Remove(typeof(ClientCredentials));
                        }
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

            if(string.Compare(lastEndPointUri, ChannelFactory.Endpoint.Address.Uri.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                lastEndPointUri = ChannelFactory.Endpoint.Address.Uri.ToString();
                Logger.Debug("EndPoint Address:{0}", ChannelFactory.Endpoint.Address.Uri.ToString());
            }
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
                var bindingElement = (cf.Endpoint.Binding as CustomBinding).Elements.Find<SslStreamSecurityBindingElement>();
                if (bindingElement != null)
                {
                    bindingElement.IdentityVerifier = new DateTransferCustomIdentityVerifer();
                }
            }
        }
    }

    internal class WcfConfigurationChannelUtility
    {
        internal static void ApplyEndpointFromConfiguration(ServiceEndpoint endpoint, Configuration configuration, string configurationName)
        {
            if (endpoint == null)
            {
                throw new InvalidOperationException("Cannot create channel without endpoint.");
            }

            var serviceModel = System.ServiceModel.Configuration.ServiceModelSectionGroup.GetSectionGroup(configuration);

            ChannelEndpointElement endpointElement = null;
            foreach (System.ServiceModel.Configuration.ChannelEndpointElement item in serviceModel.Client.Endpoints)
            {
                if (item.Name.Equals(configurationName, StringComparison.Ordinal))
                {
                    endpointElement = item;
                    break;
                }
            }
            if (endpointElement == null)
            {
                throw new ArgumentException(string.Format("Cannot find endpoint in configuration file:{0} by name:{1}", configuration.FilePath, configurationName));
            }

            if (endpoint.Binding == null && (!string.IsNullOrEmpty(endpointElement.Binding)))
            {
                var bindingCollection = serviceModel.Bindings[endpointElement.Binding];

                if (bindingCollection == null)
                {
                    throw new ArgumentException(string.Format("Cannot find binding section from configuration file:{0} by name:{1}", configuration.FilePath, endpointElement.Binding));
                }

                Binding binding = null;

                foreach (var item in bindingCollection.ConfiguredBindings)
                {
                    if (item.Name.Equals(endpointElement.BindingConfiguration, StringComparison.Ordinal))
                    {
                        binding = GetBinding(bindingCollection);
                        item.ApplyConfiguration(binding);
                        break;
                    }
                }

                if (binding == null)
                {
                    throw new ArgumentException(string.Format("Cannot find binding from configuration file:{0} by name:{1} with type:{2}", configuration.FilePath, endpointElement.BindingConfiguration, endpointElement.Binding));
                }

                endpoint.Binding = binding;
            }

            if (endpoint.Address == null && endpointElement.Address != null && endpointElement.Address.OriginalString.Length > 0)
            {
                endpoint.Address = new EndpointAddress(endpointElement.Address, LoadIdentity(endpointElement.Identity), endpointElement.Headers.Headers);
            }

            if (serviceModel.CommonBehaviors != null && serviceModel.CommonBehaviors.EndpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(serviceModel.CommonBehaviors.EndpointBehaviors, endpoint.Behaviors, true);
            }

            if (!string.IsNullOrEmpty(endpointElement.BehaviorConfiguration))
            {
                var endpointBehaviors = serviceModel.Behaviors.EndpointBehaviors[endpointElement.BehaviorConfiguration];

                if (endpointBehaviors != null)
                {
                    LoadBehaviors<IEndpointBehavior>(endpointBehaviors, endpoint.Behaviors, false);
                }
            }
        }

        private static void LoadBehaviors<T>(ServiceModelExtensionCollectionElement<BehaviorExtensionElement> behaviorElement, KeyedByTypeCollection<T> behaviors, bool commonBehaviors)
        {
            bool? flag = null;
            var keyedByTypeCollection = new KeyedByTypeCollection<T>();
            for (int i = 0; i < behaviorElement.Count; i++)
            {
                BehaviorExtensionElement behaviorExtensionElement = behaviorElement[i];
                object obj = behaviorExtensionElement.GetType().InvokeMember("CreateBehavior",
                    BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, behaviorExtensionElement, null, System.Globalization.CultureInfo.InvariantCulture);
                if (obj != null)
                {
                    Type type = obj.GetType();
                    if (typeof(T).IsAssignableFrom(type))
                    {
                        if ((!commonBehaviors) || (!ShouldSkipCommonBehavior(type, ref flag)))
                        {
                            keyedByTypeCollection.Add((T)((object)obj));
                            if (behaviors.Contains(type))
                            {
                                behaviors.Remove(type);
                            }
                            behaviors.Add((T)((object)obj));
                        }
                    }
                }
            }
        }

        private static bool ShouldSkipCommonBehavior(Type behaviorType, ref bool? isPT)
        {
            bool result = false;
            if (!isPT.HasValue)
            {
                if (!IsTypeAptca(behaviorType))
                {
                    isPT = new bool?(false);
                    result = isPT.Value;
                }
            }
            else
            {
                if (isPT.Value)
                {
                    result = !IsTypeAptca(behaviorType);
                }
            }
            return result;
        }

        internal static bool IsTypeAptca(Type type)
        {
            Assembly assembly = type.Assembly;
            return IsAssemblyAptca(assembly) || !IsAssemblySigned(assembly);
        }

        private static bool IsAssemblyAptca(Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(AllowPartiallyTrustedCallersAttribute), false).Length > 0;
        }

        private static bool IsAssemblySigned(Assembly assembly)
        {
            byte[] publicKeyToken = assembly.GetName().GetPublicKeyToken();
            return publicKeyToken != null & publicKeyToken.Length > 0;
        }

        private static Binding GetBinding(BindingCollectionElement collectionElement)
        {
            if (collectionElement is CustomBindingCollectionElement)
                return new CustomBinding();
            else if (collectionElement is BasicHttpBindingCollectionElement)
                return new BasicHttpBinding();
            else if (collectionElement is NetMsmqBindingCollectionElement)
                return new NetMsmqBinding();
            else if (collectionElement is NetNamedPipeBindingCollectionElement)
                return new NetNamedPipeBinding();
            else if (collectionElement is NetPeerTcpBindingCollectionElement)
                return new NetPeerTcpBinding();
            else if (collectionElement is NetTcpBindingCollectionElement)
                return new NetTcpBinding();
            else if (collectionElement is WSDualHttpBindingCollectionElement)
                return new WSDualHttpBinding();
            else if (collectionElement is WSHttpBindingCollectionElement)
                return new WSHttpBinding();
            else if (collectionElement is WSFederationHttpBindingCollectionElement)
                return new WSFederationHttpBinding();

            throw new Exception(string.Format("This type:{0} is not supported.", collectionElement.GetType()));
        }

        private static EndpointIdentity LoadIdentity(IdentityElement element)
        {
            EndpointIdentity result = null;
            PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                result = EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            else
            {
                if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
                {
                    result = EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
                }
                else
                {
                    if (properties["dns"].ValueOrigin != PropertyValueOrigin.Default)
                    {
                        result = EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
                    }
                    else
                    {
                        if (properties["rsa"].ValueOrigin != PropertyValueOrigin.Default)
                        {
                            result = EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
                        }
                        else
                        {
                            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
                            {
                                X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
                                x509Certificate2Collection.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
                                if (x509Certificate2Collection.Count == 0)
                                {
                                    throw new InvalidOperationException("Unable to Load Certificate Identity");
                                }
                                X509Certificate2 primaryCertificate = x509Certificate2Collection[0];
                                x509Certificate2Collection.RemoveAt(0);
                                result = EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, x509Certificate2Collection);
                            }
                            else
                            {
                                if (properties["certificateReference"].ValueOrigin != PropertyValueOrigin.Default)
                                {
                                    var x509CertificateStore = new X509Store(element.CertificateReference.StoreName, element.CertificateReference.StoreLocation);
                                    X509Certificate2Collection x509Certificate2Collection2 = null;
                                    try
                                    {
                                        x509CertificateStore.Open(OpenFlags.ReadOnly);
                                        x509Certificate2Collection2 = x509CertificateStore.Certificates.Find(element.CertificateReference.X509FindType, element.CertificateReference.FindValue, false);
                                        if (x509Certificate2Collection2.Count == 0)
                                        {
                                            throw new InvalidOperationException("Unable to Load Certificate Identity");
                                        }
                                        var certificate = new X509Certificate2(x509Certificate2Collection2[0]);
                                        if (element.CertificateReference.IsChainIncluded)
                                        {
                                            var x509Chain = new X509Chain();
                                            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                                            x509Chain.Build(certificate);

                                            certificate = x509Chain.ChainElements[0].Certificate;
                                            var x509Certificate2Collection = new X509Certificate2Collection();
                                            for (int i = 1; i < x509Chain.ChainElements.Count; i++)
                                            {
                                                x509Certificate2Collection.Add(x509Chain.ChainElements[i].Certificate);
                                            }

                                            result = new X509CertificateEndpointIdentity(certificate, x509Certificate2Collection);
                                        }
                                        else
                                        {
                                            result = EndpointIdentity.CreateX509CertificateIdentity(certificate);
                                        }
                                    }
                                    finally
                                    {
                                        ResetAllCertificates(x509Certificate2Collection2);
                                        x509CertificateStore.Close();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static void ResetAllCertificates(X509Certificate2Collection certificates)
        {
            if (certificates != null)
            {
                for (int i = 0; i < certificates.Count; i++)
                {
                    certificates[i].Reset();
                }
            }
        }
    }

    internal class WcfConfigurationChannelFactory<T> : ChannelFactory<T>
    {
        protected string endpointConfigurationName;
        protected readonly Configuration configuration;
        protected EndpointAddress remoteAddress;

        public WcfConfigurationChannelFactory(string endpointConfigurationName, System.Configuration.Configuration configuration, EndpointAddress remoteAddress)
            : base(typeof(T))
        {
            this.endpointConfigurationName = endpointConfigurationName;
            this.configuration = configuration;
            this.remoteAddress = remoteAddress;
            base.InitializeEndpoint(endpointConfigurationName, remoteAddress);
        }

        protected override void ApplyConfiguration(string configurationName)
        {
            WcfConfigurationChannelUtility.ApplyEndpointFromConfiguration(this.Endpoint, configuration, configurationName);
        }
    }

    internal class WcfConfigurationDuplexChannelFactory<T> : DuplexChannelFactory<T>
    {
        private InstanceContext instance;
        protected string endpointConfigurationName;
        protected readonly Configuration configuration;
        protected EndpointAddress remoteAddress;

        public WcfConfigurationDuplexChannelFactory(object callbackObject, string endpointConfigurationName, EndpointAddress remoteAddress, System.Configuration.Configuration configuration)
            : base(callbackObject)
        {
            this.endpointConfigurationName = endpointConfigurationName;
            this.configuration = configuration;
            this.remoteAddress = remoteAddress;
            base.InitializeEndpoint(endpointConfigurationName, remoteAddress);
        }

        protected override void ApplyConfiguration(string configurationName)
        {
            if (configuration != null)
            {
                WcfConfigurationChannelUtility.ApplyEndpointFromConfiguration(this.Endpoint, configuration, configurationName);
            }
        }
    }
}
