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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Xml;
using AvePoint.GCommon.Transfer.MQ;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Transfer.Common
{
    class XmlConfiguration
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(XmlConfiguration), false);

        private static object syncObj = new object();
        private static bool isInited = false;
        private static bool isLoading = false;
        private static string configurationFile = string.Empty;

        public static string ConfigurationFile
        {
            get { return configurationFile; }
        }

        internal static void InitiateConfiguration()
        {
            try
            {
                if ((!isInited) && (!isLoading))
                {
                    lock (syncObj)
                    {
                        if ((!isInited) && (!isLoading))
                        {
                            isLoading = true;
                            XmlElement element = null;
                            //<section name="dataTransfer" type="System.Configuration.IgnoreSectionHandler"/>
                            //XmlElement element = ConfigurationManager.GetSection("dataTransfer") as XmlElement;

                            XmlDocument document = new XmlDocument();

                            configurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                            if ((!string.IsNullOrEmpty(configurationFile)) && File.Exists(configurationFile))
                            {
                                document.Load(configurationFile);
                                XmlNode node = document.SelectSingleNode("configuration/dataTransfer");
                                if (node != null)
                                {
                                    element = node as XmlElement;
                                }
                            }

                            if (element == null)
                            {
                                //var assembly = Assembly.GetCallingAssembly();
                                configurationFile = string.Empty;
                                //if (assembly != null)
                                //{
                                //    var location = Assembly.GetCallingAssembly().Location;
                                //    if (!string.IsNullOrEmpty(location))
                                //    {
                                //        configurationFile = Path.Combine(Path.GetDirectoryName(location), "CommonDataTransfer.config");
                                //    }
                                //}

                                if (string.IsNullOrEmpty(configurationFile))
                                {
                                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                    if (!baseDirectory.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
                                    {
                                        baseDirectory = Path.Combine(baseDirectory, "bin");
                                    }
                                    if (Directory.Exists(baseDirectory))
                                    {
                                        configurationFile = Path.Combine(baseDirectory, "CommonDataTransfer.config");
                                    }
                                    else
                                    {
                                        configurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommonDataTransfer.config");
                                    }
                                }

                                if (!File.Exists(configurationFile))
                                {
                                    //document.LoadXml("<configuration><dataTransfer/></configuration>");
                                    document.LoadXml("<configuration><configSections><section name=\"dataTransfer\" type=\"System.Configuration.IgnoreSectionHandler\" /></configSections><dataTransfer/></configuration>");
                                }
                                else
                                {
                                    document.Load(configurationFile);
                                }

                                XmlNode node = document.SelectSingleNode("configuration/dataTransfer");
                                if (node != null)
                                {
                                    element = node as XmlElement;
                                }
                            }

                            logger.Debug("The configuration file is {0}.", configurationFile);
                            bool changed = InitiateDataTransferAndMQ(element);
                            changed |= InitiateSystemServiceModel(document.DocumentElement);

                            if (changed)
                            {
                                if (string.IsNullOrEmpty(configurationFile))
                                {
                                    //TODO
                                }
                                else
                                {
                                    document.Save(configurationFile);
                                }
                            }

                            DataTransferConfiguration.DefaultDataBinding = GetBindingFromConfigurationFile(configurationFile,
                                DataTransferConfiguration.DefaultDataBindingName, DataTransferConfiguration.UriSchema);

                            isInited = true;
                            isLoading = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Initiate the configuration failed:{0}.", ex.ToString());
                isLoading = false;
            }
        }

        private static bool InitiateDataTransferAndMQ(XmlElement xmlElement)
        {
            if (xmlElement == null)
            {
                throw new ArgumentNullException("xmlElement is null, please delete the DataTransfer configuration file next time.");
            }

            bool changed = false;
            XmlElement dataTransferEle = null;
            XmlElement mqEle = null;
            string nodeName = "MQ";

            foreach (XmlNode subNode in xmlElement.ChildNodes)
            {
                if (subNode.Name.Equals("data", StringComparison.OrdinalIgnoreCase))
                {
                    dataTransferEle = subNode as XmlElement;
                }
                else if (subNode.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    mqEle = subNode as XmlElement;
                }
            }

            if (dataTransferEle == null)
            {
                dataTransferEle = xmlElement.OwnerDocument.CreateElement("data");
                xmlElement.AppendChild(dataTransferEle);
                changed = true;
            }
            if (mqEle == null)
            {
                mqEle = xmlElement.OwnerDocument.CreateElement(nodeName.ToLower());
                xmlElement.AppendChild(mqEle);
                changed = true;
            }

            DataTransferConfiguration.Binding = GetAttributeFromXmlElement(xmlElement, "Binding", DataTransferConfiguration.Binding, ref changed);
            DataTransferConfiguration.UriSchema = GetAttributeFromXmlElement(xmlElement, "UriSchema", DataTransferConfiguration.UriSchema, ref changed);
            DataTransferConfiguration.DefaultDataBindingName = GetAttributeFromXmlElement(xmlElement, "DefaultDataBindingName", DataTransferConfiguration.DefaultDataBindingName.ToString(), ref changed);

            DataTransferConfiguration.CycleStreamSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "CycleStreamSize", DataTransferConfiguration.CycleStreamSize.ToString(), ref changed));
            DataTransferConfiguration.DataBlockCompressionMethod = (CompressionMethods)Enum.Parse(typeof(CompressionMethods),
                GetAttributeFromXmlElement(dataTransferEle, "DataBlockCompressionMethod", DataTransferConfiguration.DataBlockCompressionMethod.ToString(), ref changed));
            DataTransferConfiguration.DataBlockEncryptionMethod = (EncryptionAlgorithm)Enum.Parse(typeof(EncryptionAlgorithm),
                GetAttributeFromXmlElement(dataTransferEle, "DataBlockEncryptionMethod", DataTransferConfiguration.DataBlockEncryptionMethod.ToString(), ref changed));
            DataTransferConfiguration.DataBlockProcessorBufferSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DataBlockProcessorBufferSize", DataTransferConfiguration.DataBlockProcessorBufferSize.ToString(), ref changed));
            DataTransferConfiguration.DataBlockProcessorCycleStreamSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DataBlockProcessorCycleStreamSize", DataTransferConfiguration.DataBlockProcessorCycleStreamSize.ToString(), ref changed));
            DataTransferConfiguration.DefaultReconnectTimeout = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "DefaultReconnectTimeout", DataTransferConfiguration.DefaultReconnectTimeout.ToString(), ref changed));
            DataTransferConfiguration.MaxCacheBuffer = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "MaxCacheBuffer", DataTransferConfiguration.MaxCacheBuffer.ToString(), ref changed));
            DataTransferConfiguration.MinReconnectTimeout = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "MinReconnectTimeout", DataTransferConfiguration.MinReconnectTimeout.ToString(), ref changed));
            DataTransferConfiguration.SendBufferSize = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "SendBufferSize", DataTransferConfiguration.SendBufferSize.ToString(), ref changed));
            DataTransferConfiguration.TakeDataBlockTimeOut = int.Parse(GetAttributeFromXmlElement(dataTransferEle, "TakeDataBlockTimeOut", DataTransferConfiguration.TakeDataBlockTimeOut.ToString(), ref changed));
            DataTransferConfiguration.DisablePerformanceLogger = bool.Parse(GetAttributeFromXmlElement(dataTransferEle, "DisablePerformanceLogger", DataTransferConfiguration.DisablePerformanceLogger.ToString(), ref changed));
            DataTransferConfiguration.EnablePerformanceCounter = bool.Parse(GetAttributeFromXmlElement(dataTransferEle, "EnablePerformanceCounter", DataTransferConfiguration.EnablePerformanceCounter.ToString(), ref changed));

            /*
             * Remove unused attribute from 6.0 configuration file
             */
            RemoveAttributeFromXmlElement(dataTransferEle, "UriSchema", ref changed);
            RemoveAttributeFromXmlElement(dataTransferEle, "DefaultDataBindingName", ref changed);

            AveMQConfigure.IsOneWayConnection = bool.Parse(GetAttributeFromXmlElement(mqEle, "IsOneWayConnection", AveMQConfigure.IsOneWayConnection.ToString(), ref changed));
            AveMQConfigure.MaxMessageTimeout = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxMessageTimeout", AveMQConfigure.MaxMessageTimeout.ToString(), ref changed));
            AveMQConfigure.MaxReconnectionTimeOut = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxReconnectionTimeOut", AveMQConfigure.MaxReconnectionTimeOut.ToString(), ref changed));
            AveMQConfigure.MaxRetryTimes = int.Parse(GetAttributeFromXmlElement(mqEle, "MaxRetryTimes", AveMQConfigure.MaxRetryTimes.ToString(), ref changed));
            AveMQConfigure.MQChannelMode = (AveChannelMode)Enum.Parse(typeof(AveChannelMode),
                GetAttributeFromXmlElement(mqEle, "MQChannelMode", AveMQConfigure.MQChannelMode.ToString(), ref changed));
            AveMQConfigure.NoReconnectTimeOut = int.Parse(GetAttributeFromXmlElement(mqEle, "NoReconnectTimeOut", AveMQConfigure.NoReconnectTimeOut.ToString(), ref changed));
            AveMQConfigure.ReconnectionTime = int.Parse(GetAttributeFromXmlElement(mqEle, "ReconnectionTime", AveMQConfigure.ReconnectionTime.ToString(), ref changed));
            AveMQConfigure.TempFolder = GetAttributeFromXmlElement(mqEle, "TempFolder", AveMQConfigure.TempFolder.ToString(), ref changed);
            AveMQConfigure.EnablePerformanceCounter = bool.Parse(GetAttributeFromXmlElement(mqEle, "EnablePerformanceCounter", AveMQConfigure.EnablePerformanceCounter.ToString(), ref changed));

            return changed;
        }

        /// <summary>
        /// upgrade ServiceModel中的节点
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        private static bool InitiateSystemServiceModel(XmlElement xmlElement)
        {
            bool changed = false;

            Tuple<XmlElement, bool> serviceModel = FindAndCreateSubElement(xmlElement, "system.serviceModel");
            changed |= serviceModel.Item2;
            Tuple<XmlElement, bool> bindings = FindAndCreateSubElement(serviceModel.Item1, "bindings");
            changed |= bindings.Item2;
            Tuple<Dictionary<string, XmlElement>, bool> subBinding = FindAndCreateSubElements(bindings.Item1, "basicHttpBinding",
                "netTcpBinding", "wsDualHttpBinding", "customBinding");
            changed |= subBinding.Item2;

            Tuple<XmlElement, bool> basicHttpBinding = EnsureSubBinding(subBinding.Item1["basicHttpBinding"], "DataTransferDefaultDataBinding",
@"
        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
                 maxBufferPoolSize=""524288"" maxBufferSize=""536870912"" transferMode=""Buffered"" maxReceivedMessageSize=""536870912""
                 hostNameComparisonMode=""StrongWildcard"">
          <security mode=""None""></security>
          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
        </binding>");
            changed |= basicHttpBinding.Item2;

            Tuple<XmlElement, bool> netTcpBinding = EnsureSubBinding(subBinding.Item1["netTcpBinding"], "DataTransferDefaultDataBinding",
@"
        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
                 maxBufferSize=""536870912"" maxBufferPoolSize=""524288"" transferMode=""Buffered"" listenBacklog=""10"" maxReceivedMessageSize=""536870912""
                 hostNameComparisonMode=""StrongWildcard"" portSharingEnabled=""True"">
          <security mode=""None""></security>
          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
        </binding>");
            changed |= netTcpBinding.Item2;

            Tuple<XmlElement, bool> wsDualHttpBinding = EnsureSubBinding(subBinding.Item1["wsDualHttpBinding"], "DataTransferDefaultDataBinding",
@"
        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"" 
                 maxBufferPoolSize=""524288"" maxReceivedMessageSize=""536870912"" hostNameComparisonMode=""StrongWildcard"">
          <security mode=""None""></security>
          <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
        </binding>");
            changed |= wsDualHttpBinding.Item2;

            Tuple<XmlElement, bool> customBinding = EnsureSubBinding(subBinding.Item1["customBinding"], "DataTransferDefaultDataBinding",
@"
        <binding name=""DataTransferDefaultDataBinding"" closeTimeout=""01:10:00"" openTimeout=""01:10:00"" receiveTimeout=""01:10:00"" sendTimeout=""01:10:00"">
          <transactionFlow transactionProtocol=""OleTransactions"" />
          <binaryMessageEncoding maxReadPoolSize=""64"" maxWritePoolSize=""16"" maxSessionSize=""2048"">
            <readerQuotas maxDepth=""320"" maxStringContentLength=""2147483647"" maxArrayLength=""536870912"" maxBytesPerRead=""4096"" maxNameTableCharCount=""65536"" />
          </binaryMessageEncoding>
          <!--<sslStreamSecurity requireClientCertificate=""true"" />-->
          <tcpTransport manualAddressing=""false"" maxBufferPoolSize=""524288""
            maxReceivedMessageSize=""536870912"" connectionBufferSize=""102400""
            hostNameComparisonMode=""StrongWildcard"" channelInitializationTimeout=""00:00:05""
            maxBufferSize=""536870912"" maxPendingConnections=""10"" maxOutputDelay=""00:00:00.2000000""
            maxPendingAccepts=""1"" transferMode=""Buffered"" listenBacklog=""10""
            portSharingEnabled=""true"" teredoEnabled=""false"">
            <connectionPoolSettings groupName=""default"" leaseTimeout=""00:05:00""
              idleTimeout=""00:02:00"" maxOutboundConnectionsPerEndpoint=""10"" />
          </tcpTransport>
        </binding>");
            changed |= customBinding.Item2;

            return changed;
        }

        /// <summary>
        /// 查找并且创建
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Tuple<XmlElement, bool> FindAndCreateSubElement(XmlElement parentElement, string name)
        {
            if (parentElement == null)
            {
                throw new ArgumentNullException("parentElement");
            }

            XmlElement element = null;
            bool changed = false;

            foreach (XmlNode subNode in parentElement.ChildNodes)
            {
                if (subNode.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && subNode.NodeType == XmlNodeType.Element)
                {
                    element = (XmlElement)subNode;
                    break;
                }
            }

            if (element == null)
            {
                element = parentElement.OwnerDocument.CreateElement(name);
                parentElement.AppendChild(element);
                changed = true;
            }

            return new Tuple<XmlElement, bool>(element, changed);
        }

        /// <summary>
        /// 批量创建sub element
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        private static Tuple<Dictionary<string, XmlElement>, bool> FindAndCreateSubElements(XmlElement parentElement, params string[] names)
        {
            Dictionary<string, XmlElement> subEles = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
            bool changed = false;

            if (parentElement == null)
            {
                throw new ArgumentNullException("parentElement");
            }
            if (names == null || names.Length == 0)
            {
                throw new ArgumentNullException("names");
            }

            foreach (string name in names)
            {
                subEles[name] = null;
            }

            foreach (XmlNode subNode in parentElement.ChildNodes)
            {
                if (subNode.NodeType == XmlNodeType.Element && subEles.ContainsKey(subNode.Name))
                {
                    subEles[subNode.Name] = (XmlElement)subNode;
                }
            }

            foreach (string name in names)
            {
                if (subEles[name] == null)
                {
                    XmlElement tempNode = parentElement.OwnerDocument.CreateElement(name);
                    parentElement.AppendChild(tempNode);
                    subEles[name] = tempNode;
                    changed = true;
                }
            }

            return new Tuple<Dictionary<string, XmlElement>, bool>(subEles, changed);
        }

        private static Tuple<XmlElement, bool> EnsureSubBinding(XmlElement parentElement, string bindingName, string bindingSchema)
        {
            XmlElement subBinding = null;
            bool changed = false;

            if (parentElement == null)
            {
                throw new ArgumentNullException("parentElement");
            }

            foreach (XmlNode subNode in parentElement.ChildNodes)
            {
                if (subNode.NodeType == XmlNodeType.Element && subNode.Name.Equals("binding", StringComparison.Ordinal))
                {
                    XmlElement subEle = subNode as XmlElement;
                    string subName = subEle.GetAttribute("name");
                    if ((!string.IsNullOrEmpty(subName)) && subName.Equals("DataTransferDefaultDataBinding", StringComparison.Ordinal))
                    {
                        subBinding = subEle;
                        break;
                    }
                }
            }

            if (subBinding == null)
            {
                //subBinding = parentElement.OwnerDocument.CreateElement("binding");
                parentElement.InnerXml += bindingSchema;
                changed = true;
            }

            return new Tuple<XmlElement, bool>(subBinding, changed);
        }

        /// <summary>
        /// 获取默认的Binding
        /// </summary>
        /// <param name="configurationFile"></param>
        /// <param name="bindingName"></param>
        /// <returns></returns>
        private static Binding GetBindingFromConfigurationFile(string configurationFile, string bindingName, string uriSchema)
        {
            Binding binding = null;

            try
            {
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                    new System.Configuration.ExeConfigurationFileMap() { ExeConfigFilename = configurationFile },
                    System.Configuration.ConfigurationUserLevel.None);

                BindingsSection bindingSection = configuration.GetSection("system.serviceModel/bindings") as BindingsSection;

                if (bindingSection != null)
                {
                    binding = ApplyConfiguration(DataTransferConfiguration.Binding, bindingName, bindingSection);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Cannot get the binding from configuration file:{0} with configuration name:{1}, because there is an exception:{2}.", configurationFile, bindingName, ex.ToString());
                binding = GetDefaultBinding(uriSchema);
            }


            return binding;
        }

        internal static Binding ApplyConfiguration(string bindingType, string bindingName, BindingsSection section)
        {
            if (string.IsNullOrEmpty(bindingType))
            {
                throw new ArgumentNullException("bindingName null Exception");
            }

            Binding binding = null;
            switch (bindingType)
            {
                case "netTcpBinding":
                    {
                        binding = new NetTcpBinding();
                        section.NetTcpBinding.Bindings[bindingName].ApplyConfiguration(binding);
                    } break;
                case "basicHttpBinding":
                    {
                        binding = new BasicHttpBinding();
                        section.BasicHttpBinding.Bindings[bindingName].ApplyConfiguration(binding);
                    } break;
                case "wsDualHttpBinding":
                    {
                        binding = new WSDualHttpBinding();
                        section.WSDualHttpBinding.Bindings[bindingName].ApplyConfiguration(binding);
                    } break;
                case "customBinding":
                    {
                        binding = new CustomBinding();
                        section.CustomBinding.Bindings[bindingName].ApplyConfiguration(binding);
                    } break;
                default:
                    {
                        throw new Exception(string.Format("Not Supported. {0}", bindingType));
                    }
            }

            return binding;
        }

        internal static Binding GetDefaultBinding(string uriSchema)
        {
            Binding binding = null;
            if ((!string.IsNullOrEmpty(uriSchema)) && uriSchema.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                BasicHttpBinding basicHttpBind = new BasicHttpBinding();
                basicHttpBind.Security.Mode = BasicHttpSecurityMode.None;
                basicHttpBind.TransferMode = TransferMode.Buffered;
                basicHttpBind.MaxBufferSize = 536870912;
                basicHttpBind.MaxReceivedMessageSize = 536870912;
                basicHttpBind.ReaderQuotas.MaxStringContentLength = 536870912;
                basicHttpBind.ReaderQuotas.MaxArrayLength = 536870912;

                binding = basicHttpBind;
            }
            else// default is NetTcp
            {
                NetTcpBinding tempBinding = new NetTcpBinding();
                tempBinding.Security.Mode = SecurityMode.None;
                tempBinding.PortSharingEnabled = true;
                tempBinding.TransferMode = TransferMode.Buffered;
                tempBinding.MaxBufferSize = 536870912;
                tempBinding.MaxReceivedMessageSize = 536870912;
                tempBinding.ReaderQuotas.MaxStringContentLength = 536870912;
                tempBinding.ReaderQuotas.MaxArrayLength = 536870912;
                binding = tempBinding;
            }

            return binding;
        }

        /// <summary>
        /// Get attribute and auto created the attribute if it does not exist.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="configurationFileChanged"></param>
        /// <returns></returns>
        private static string GetAttributeFromXmlElement(XmlElement element, string name, string defaultValue, ref bool configurationFileChanged)
        {
            string result = defaultValue;

            if (element.HasAttribute(name))
            {
                result = element.GetAttribute(name);
            }
            else
            {
                element.SetAttribute(name, defaultValue);
                configurationFileChanged = true;
            }

            return result;
        }

        /// <summary>
        /// remove special attribute
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="configurationFileChanged"></param>
        /// <returns></returns>
        private static void RemoveAttributeFromXmlElement(XmlElement element, string name, ref bool configurationFileChanged)
        {
            if (element.HasAttribute(name))
            {
                element.RemoveAttribute(name);
                configurationFileChanged = true;
            }
        }
    }
}
