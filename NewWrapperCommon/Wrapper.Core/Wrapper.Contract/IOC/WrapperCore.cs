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
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Core.Internal;
using System.Reflection;
using AvePoint.Wrapper.Core.SPAPI;

namespace AvePoint.Wrapper.Core.IOC
{
    /// <summary>
    /// Plugin Manager
    /// </summary>
    public class WrapperCore
    {
        private const string O365AuthenticationsNodeName = "o365Authentications";
        private const string AuthenticationNodeName = "authentication";
        private const string DeploymentsNodeName = "deployments";
        private const string DeploymentNodeName = "deployment";
        private const string SPAPIsNodeName = "spAPIs";
        private const string SPAPINodeName = "spAPI";
        private const string InstancesNodeName = "instances";
        private const string InstanceNodeName = "instance";

        private Dictionary<string, O365AuthenticationNode> o365AuthenticationNodes = new Dictionary<string, O365AuthenticationNode>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DeploymentNode> deploymentNodes = new Dictionary<string, DeploymentNode>(StringComparer.OrdinalIgnoreCase);
        private List<IWrapperDeploymentAPI> deploymentAPI = new List<IWrapperDeploymentAPI>();
        private Dictionary<string, SPAPINode> spAPINodes = new Dictionary<string, SPAPINode>(StringComparer.OrdinalIgnoreCase);
        private List<ISPAPIUtility> spAPIs = new List<ISPAPIUtility>();
        private Dictionary<string, object> instances = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        static WrapperCore()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            Manager = new WrapperCore(Path.Combine(WrapperEnv.RootFolder, Constants.WrapperCoreConfigurationFile));
        }

        static System.Reflection.Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }

        public static WrapperCore Manager { get; set; }

        /// <summary>
        /// Load information from app.config
        /// </summary>
        public WrapperCore() : this(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)
        {

        }

        /// <summary>
        /// load information from special file
        /// </summary>
        /// <param name="fileName"></param>
        public WrapperCore(string fileName)
        {
            var document = new XmlDocument();

            document.Load(fileName);

            var wrapperNode = document.SelectSingleNode("configuration/wrapper");

            DeserializeWrapperNode(wrapperNode);
        }

        private void DeserializeWrapperNode(XmlNode wrapperNode)
        {
            if (wrapperNode != null)
            {
                foreach (XmlNode node in wrapperNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        switch (node.Name)
                        {
                            case O365AuthenticationsNodeName:
                                DeserializeAuthentications(node.ChildNodes);
                                break;
                            case DeploymentsNodeName:
                                DeserializeDeployments(node.ChildNodes);
                                break;
                            case SPAPIsNodeName:
                                DeserializeSPAPIs(node.ChildNodes);
                                break;
                            case InstancesNodeName:
                                DeserializeInstances(node.ChildNodes);
                                break;
                        }
                    }
                }
            }
        }

        private void DeserializeInstances(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals(InstanceNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var instance = new InstanceNode(node as XmlElement);

                        var instanceObject = Activator.CreateInstance(instance.Type);

                        lock (instances)
                        {
                            instances[instance.Id] = instanceObject;
                        }
                    }
                    catch (BadImageFormatException ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex);
                    }
                }
            }
        }

        private void DeserializeSPAPIs(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals(SPAPINodeName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var spAPINode = new SPAPINode(node as XmlElement);

                        lock (spAPINodes)
                        {
                            spAPINodes[spAPINode.Id] = spAPINode;
                        }
                    }
                    catch (BadImageFormatException ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex);
                    }
                }
            }
        }

        private void DeserializeDeployments(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals(DeploymentNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var deploymentNode = new DeploymentNode(node as XmlElement);

                        lock (deploymentNodes)
                        {
                            deploymentNodes[deploymentNode.Id] = deploymentNode;
                        }
                    }
                    catch (BadImageFormatException ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex);
                    }
                }
            }
        }

        private void DeserializeAuthentications(XmlNodeList nodes)
        {
            foreach(XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name.Equals(AuthenticationNodeName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var authenticationNode = new O365AuthenticationNode(node as XmlElement);

                        lock (o365AuthenticationNodes)
                        {
                            o365AuthenticationNodes[authenticationNode.Id] = authenticationNode;
                        }
                    }
                    catch (BadImageFormatException ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex.Message);
                    }
                    catch(Exception ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_LoadNodeFailed, node.OuterXml, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Get O365Authentications
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public IO365Authentication[] ResolveO365Authentications(string url)
        {
            var authentications = new List<IO365Authentication>();

            var nodes = o365AuthenticationNodes.Values.ToList();

            foreach(var node in nodes)
            {
                if(string.IsNullOrEmpty(node.Scope) || (node.Scope.Length == 1 && node.Scope[0]=='*') || Regex.IsMatch(url, node.Scope))
                {
                    try
                    {
                        if (node.Type == null)
                        {
                            if (node.ReflectionOnlyType != null)
                            {
                                node.Type = Type.GetType(node.ReflectionOnlyType.AssemblyQualifiedName, true, false);
                            }
                            else
                            {
                                node.Type = Type.GetType(node.TypeAsString, true, false);
                            }
                        }

                        authentications.Add((IO365Authentication)Activator.CreateInstance(node.Type));
                    }
                    catch(Exception ex)
                    {
                        WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_CreateInstanceFailed, node.ReflectionOnlyType.FullName, ex);
                    }
                }
            }

            if(authentications.Count == 0)
            {
                throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_ResolveO365AuthenticationsFailed, url), WrapperErrorCode.ResolveO365AuthenticationsFailed);
            }

            return authentications.ToArray();
        }

        internal IWrapperDeploymentAPI ResolveWrapperDeploymentAPI(WrapperSPMode spMode, Version version)
        {
            lock(deploymentAPI)
            {
                foreach(var item in deploymentAPI)
                {
                    if(item.Support(spMode, version))
                    {
                        return item;
                    }
                }

                lock(deploymentNodes)
                {
                    foreach(var item in deploymentNodes)
                    {
                        if (item.Value.Scope == spMode && version != null && item.Value.Version.Major == version.Major)
                        {
                            if(item.Value.Type == null)
                            {
                                item.Value.Type = Type.GetType(item.Value.ReflectionOnlyType.AssemblyQualifiedName, true, false);
                            }

                            var api = (IWrapperDeploymentAPI)Activator.CreateInstance(item.Value.Type);

                            api.Initialize();

                            deploymentAPI.Add(api);

                            return api;
                        }
                    }
                }
            }

            throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_DeploymentAPIIsNotAvailable, spMode, version), WrapperErrorCode.DeploymentAPINotAvailable);

            //return deployments.ToArray();
        }

        internal ISPAPIUtility ResolveSPAPI(WrapperSPMode spMode, Version version)
        {
            lock (spAPIs)
            {
                foreach (var item in spAPIs)
                {
                    if (item.Support(spMode, version))
                    {
                        return item;
                    }
                }

                lock (spAPINodes)
                {
                    foreach (var item in spAPINodes)
                    {
                        if (item.Value.Scope == spMode && version != null && item.Value.Version.Major == version.Major)
                        {
                            if (item.Value.Type == null)
                            {
                                item.Value.Type = Type.GetType(item.Value.ReflectionOnlyType.AssemblyQualifiedName, true, false);
                            }

                            var api = (ISPAPIUtility)Activator.CreateInstance(item.Value.Type);

                            api.Initialize();

                            spAPIs.Add(api);

                            return api;
                        }
                    }
                }
            }

            throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_SPAPIIsNotAvailable, spMode, version), WrapperErrorCode.DeploymentAPINotAvailable);

        }

        /// <summary>
        /// Resolve Instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T[] ResolveAll<T>()
        {
            var validInstances = new List<T>();

            lock(instances)
            {
                validInstances.AddRange(from item in instances where item.Value is T select (T) item.Value);
            }

            if (validInstances.Count == 0)
            {
                throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_InstanceNotAvailable, typeof(T).FullName), WrapperErrorCode.InstanceNotAvailable);
            }

            return validInstances.ToArray();
        }

        /// <summary>
        /// Resolve Instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T Resolve<T>()
        {
            lock (instances)
            {
                foreach(var item in instances)
                {
                    if(item.Value is T)
                    {
                        return (T)item.Value;
                    }
                }
            }

            throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_InstanceNotAvailable, typeof(T).FullName), WrapperErrorCode.InstanceNotAvailable);
        }
    }
}
