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



namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Globalization;

    #endregion

    /// <summary>
    /// This class is used a customized wcf config file
    /// </summary>
    public class WCFSharedConfiguration
    {
        static Object syncRoot = new Object();
        static Boolean loaded;

        static String configurationFile = String.Empty;
        static String globalDefaultBasicHttpBindingName = "globalDefaultBasicHttpBinding";
        static String globalDefaultNetTcpBindingName = "globalDefaultNetTcpBinding";
        static String globalDefaultServiceBehaviorName = "globalDefaultServiceBehavior";
        static String globalDefaultEndPointBehaviorName = "globalDefaultEndPointBehavior";

        static Configuration configuration;
        static BasicHttpBinding defaultBasicHttpBinding;
        static NetTcpBinding defaultNetTcpBinding;
        static List<IServiceBehavior> defaultServiceBehaviors;
        static List<IEndpointBehavior> defaultEndPointBehaviors;

        /// <summary>
        /// Certificate Thumb print
        /// </summary>
        public static String CertificateThumbprint { get; set; }

        /// <summary>
        /// Set config file path
        /// </summary>
        /// <param name="configFile"></param>
        public static void SetConfigurationFile(String configFile)
        {
            configurationFile = configFile;
        }

        public static void LoadSharedConfigureations()
        {
            lock (syncRoot)
            {
                if (loaded == false)
                {
                    Initialize();
                    loaded = true;
                }
            }
        }

        /// <summary>
        /// reload config file
        /// </summary>
        public static void ReloadSharedConfigureations()
        {
            lock (syncRoot)
            {
                configuration = null;
                defaultBasicHttpBinding = null;
                defaultNetTcpBinding = null;
                defaultServiceBehaviors = null;
                Initialize();
                loaded = true;
            }
        }

        /// <summary>
        /// apply service host behavior config
        /// </summary>
        /// <param name="serviceHost">service host object</param>
        public static void ApplyServiceHostBehaviorConfigurations(ServiceHost serviceHost)
        {
            if (defaultServiceBehaviors != null)
            {
                foreach (IServiceBehavior behavior in defaultServiceBehaviors)
                {
                    if (serviceHost.Description.Behaviors.Contains(behavior.GetType()))
                    {
                        serviceHost.Description.Behaviors.Remove(behavior.GetType());
                    }
                    serviceHost.Description.Behaviors.Add(behavior);
                }
            }
        }

        public static void ApplyEndpointBindingConfigurations(ServiceEndpoint endPoint)
        {
            if (IsDefaultBindingConfiguration(endPoint.Binding))
            {
                //apply global service endpoint binding configuration
                if (endPoint.Binding.GetType() == typeof(BasicHttpBinding)
                    && defaultBasicHttpBinding != null)
                {
                    endPoint.Binding = defaultBasicHttpBinding;
                }
                if (endPoint.Binding.GetType() == typeof(NetTcpBinding)
                    && defaultNetTcpBinding != null)
                {
                    endPoint.Binding = defaultNetTcpBinding;
                }
            }
        }

        public static void ApplyEndpointBindingBehaviorConfigurations(ServiceEndpoint endPoint)
        {
            if (defaultEndPointBehaviors != null)
            {
                foreach (IEndpointBehavior behavior in defaultEndPointBehaviors)
                {
                    if (endPoint.Behaviors.Contains(behavior.GetType()))
                    {
                        endPoint.Behaviors.Remove(behavior.GetType());
                    }
                    endPoint.Behaviors.Add(behavior);
                }
            }
        }

        public static void Save()
        {
            if (configuration != null)
            {
                configuration.Save(ConfigurationSaveMode.Modified);
            }
        }

        public static string BindingConfigurationToString(Binding binding)
        {
            if (binding is NetTcpBinding)
            {
                NetTcpBinding netTcpBinding = binding as NetTcpBinding;
                return string.Format("NetTcpBinding Configuration: OpenTimeout={0} CloseTimeout={1} SendTimeout={2} ReceiveTimeout={3} PortSharingEnabled={4}"
                    , binding.OpenTimeout.ToString()
                    , binding.CloseTimeout.ToString()
                    , binding.SendTimeout.ToString()
                    , binding.ReceiveTimeout.ToString()
                    , netTcpBinding.PortSharingEnabled.ToString()
                    );
            }
            if (binding is BasicHttpBinding)
            {
                return string.Format("BasicHttpBinding Configuration: OpenTimeout={0} CloseTimeout={1} SendTimeout={2} ReceiveTimeout={3}"
                    , binding.OpenTimeout.ToString()
                    , binding.CloseTimeout.ToString()
                    , binding.SendTimeout.ToString()
                    , binding.ReceiveTimeout.ToString()
                    );
            }
            return string.Empty;
        }

        private WCFSharedConfiguration() { }

        private static void Initialize()
        {
            configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = configurationFile }, ConfigurationUserLevel.None);
            var basicHttpBinding = ResolveBinding(globalDefaultBasicHttpBindingName);
            if (basicHttpBinding != null)
                defaultBasicHttpBinding = basicHttpBinding as BasicHttpBinding;
            var netTcpBinding = ResolveBinding(globalDefaultNetTcpBindingName);
            if (netTcpBinding != null)
                defaultNetTcpBinding = netTcpBinding as NetTcpBinding;
            var defaultServiceBehaviorsList = ResolveServiceBehavior(globalDefaultServiceBehaviorName);
            if (defaultServiceBehaviorsList != null)
                defaultServiceBehaviors = defaultServiceBehaviorsList;
            var defaultEndPointBehaviorsList = ResolveEndPointBehavior(globalDefaultEndPointBehaviorName);
            if (defaultEndPointBehaviorsList != null)
                defaultEndPointBehaviors = defaultEndPointBehaviorsList;
        }

        private static Binding ResolveBinding(string name)
        {
            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(configuration);
            var section = serviceModel.Bindings;
            foreach (var bindingCollection in section.BindingCollections)
            {
                foreach (var bindingElement in bindingCollection.ConfiguredBindings)
                {
                    if (string.Compare(bindingElement.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var binding = (Binding)Activator.CreateInstance(bindingCollection.BindingType);
                        binding.Name = bindingElement.Name;
                        bindingElement.ApplyConfiguration(binding);
                        return binding;
                    }
                }
            }
            return null;
        }

        private static bool IsDefaultBindingConfiguration(Binding binding)
        {
            if (binding is BasicHttpBinding)
            {
                BasicHttpBinding inputBinding = binding as BasicHttpBinding;
                BasicHttpBinding defaultBinding = new BasicHttpBinding();
                if (inputBinding.OpenTimeout == defaultBinding.OpenTimeout
                    && inputBinding.CloseTimeout == defaultBinding.CloseTimeout
                    && inputBinding.SendTimeout == defaultBinding.SendTimeout
                    && inputBinding.ReceiveTimeout == defaultBinding.ReceiveTimeout
                    && inputBinding.MaxBufferPoolSize == defaultBinding.MaxBufferPoolSize
                    && inputBinding.MaxBufferSize == defaultBinding.MaxBufferSize
                    && inputBinding.MaxReceivedMessageSize == defaultBinding.MaxReceivedMessageSize
                    )
                {
                    return true;
                }
            }
            if (binding is NetTcpBinding)
            {
                NetTcpBinding inputBinding = binding as NetTcpBinding;
                NetTcpBinding defaultBinding = new NetTcpBinding();
                if (inputBinding.OpenTimeout == defaultBinding.OpenTimeout
                    && inputBinding.CloseTimeout == defaultBinding.CloseTimeout
                    && inputBinding.SendTimeout == defaultBinding.SendTimeout
                    && inputBinding.ReceiveTimeout == defaultBinding.ReceiveTimeout
                    && inputBinding.MaxBufferPoolSize == defaultBinding.MaxBufferPoolSize
                    && inputBinding.MaxBufferSize == defaultBinding.MaxBufferSize
                    && inputBinding.MaxReceivedMessageSize == defaultBinding.MaxReceivedMessageSize
                    )
                {
                    return true;
                }
            }
            return false;
        }

        private static List<IServiceBehavior> ResolveServiceBehavior(String name)
        {
            ServiceModelSectionGroup serviceModel = ServiceModelSectionGroup.GetSectionGroup(configuration);
            BehaviorsSection section = serviceModel.Behaviors;
            foreach (ServiceBehaviorElement serviceBehaviorElement in section.ServiceBehaviors)
            {
                if (String.Compare(serviceBehaviorElement.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    List<IServiceBehavior> serviceBehaviors = new List<IServiceBehavior>();
                    foreach (BehaviorExtensionElement extensionElement in serviceBehaviorElement)
                    {
                        if (extensionElement is ServiceCredentialsElement)
                        {
                            ServiceCredentialsElement sce = extensionElement as ServiceCredentialsElement;
                            var certThumbprint = sce.ServiceCertificate.FindValue;
                            if (!String.IsNullOrEmpty(CertificateThumbprint)
                                && String.Compare(certThumbprint, CertificateThumbprint, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                sce.ServiceCertificate.FindValue = CertificateThumbprint;
                            }
                        }
                        var extension = extensionElement.GetType().InvokeMember("CreateBehavior",
                              BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null, extensionElement, null, CultureInfo.InvariantCulture);
                        serviceBehaviors.Add((IServiceBehavior)extension);
                    }
                    return serviceBehaviors;
                }
            }
            return null;
        }

        private static List<IEndpointBehavior> ResolveEndPointBehavior(string name)
        {
            ServiceModelSectionGroup serviceModel = ServiceModelSectionGroup.GetSectionGroup(configuration);
            BehaviorsSection section = serviceModel.Behaviors;
            foreach (EndpointBehaviorElement endPointBehaviorElement in section.EndpointBehaviors)
            {
                if (String.Compare(endPointBehaviorElement.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    List<IEndpointBehavior> serviceBehaviors = new List<IEndpointBehavior>();
                    foreach (BehaviorExtensionElement extensionElement in endPointBehaviorElement)
                    {
                        if (extensionElement is ClientCredentialsElement)
                        {
                            ClientCredentialsElement cce = extensionElement as ClientCredentialsElement;
                            string certThumbprint = cce.ClientCertificate.FindValue;
                            if (!string.IsNullOrEmpty(CertificateThumbprint)
                                && string.Compare(certThumbprint, CertificateThumbprint, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                cce.ClientCertificate.FindValue = CertificateThumbprint;
                            }
                        }
                        var extension = extensionElement.GetType().InvokeMember("CreateBehavior",
                              BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                              null, extensionElement, null, CultureInfo.InvariantCulture);
                        serviceBehaviors.Add((IEndpointBehavior)extension);
                    }
                    return serviceBehaviors;
                }
            }
            return null;
        }
    }
}
