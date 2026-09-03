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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    #endregion

    #region Attribute

    /// <summary>
    /// The default Message handler implemented by MicroKernel itself
    /// </summary>
    [DebuggerNonUserCode]
    #endregion

    public class DefaultCoreMessageHandler
        : CoreMessageHandler<CoreMessage>
    {
        static readonly List<Type> cachedExpandableType = new List<Type> { typeof(IList), typeof(IDictionary) };
        static readonly Dictionary<String, Type> cacheTypeDictionary = new Dictionary<String, Type>();
        static readonly Dictionary<String, Assembly> cacheAssemblyDictionary = new Dictionary<String, Assembly>();


        public Dictionary<String, String> CompatibleTypeMappingDictionary { get; set; }

        /// <summary>
        /// The service locator that will be used to get the microkernel external server
        /// </summary>
        public ICoreServiceLocator ServiceLocator { get; set; }

        /// <summary>
        /// Process the microkernel message by the default handler
        /// </summary>
        /// <param name="message">the core message</param>
        /// <returns>the result core message</returns>
        public override CoreMessage ProcessMessage(CoreMessage message)
        {
            var invocationContext = message.InvocationContext;
            var invocationInstance = this.ServiceLocator.Discover(invocationContext.TypeKey);
            var argsTypes = this.GetInvocationArgsTypes(invocationContext);
            var args = this.GetInvocationArgs(invocationContext, argsTypes);
            var methodInfo = invocationInstance.GetType().GetMethod(invocationContext.MethodName, argsTypes);
            if (methodInfo.IsGenericMethodDefinition)
            {
                var genericParameterTypes = this.GetInvocationGenericParameterArgs(invocationContext);
                methodInfo = methodInfo.MakeGenericMethod(genericParameterTypes);
            }
            try
            {
                var argsValueString = this.GetInvocationArgsValueString(argsTypes, args);
                var runtime = this.BuildMicroKernelRuntime(invocationContext);
                this.TraceSource.TraceInformation(
                    "MicroKernel finally to dispatch the invoke to method {0}.{1}({2}) with runtime information:{3}",
                    invocationInstance.GetType().FullName,
                    invocationContext.MethodName,
                    argsValueString,
                    runtime);
                var result = methodInfo.Invoke(invocationInstance, args);
                this.TraceSource.TraceInformation(
                    "MicroKernel finally to get the return of the method {0}.{1}({2}) with return value:{3}",
                    invocationInstance.GetType().FullName,
                    invocationContext.MethodName,
                    argsValueString,
                    Expand(result));
                if (result != null)
                {
                    var compatibleType = result.GetType()
                        .GetCustomAttributes(typeof(CompatibleTypeConventionAttribute), false);
                    if (compatibleType.Length > 0)
                    {
                        invocationContext.BuildResult(
                            SerializerHelper.SerializeToBytesByDataContractSerializer(result),
                            result.GetType().AssemblyQualifiedName,
                            result.GetType().FullName,
                            (compatibleType[0] as CompatibleTypeConventionAttribute).CompatibleType);
                    }
                    else
                    {
                        invocationContext.BuildResult(
                            SerializerHelper.SerializeToBytesByDataContractSerializer(result),
                            result.GetType().AssemblyQualifiedName,
                            result.GetType().FullName);
                    }
                }
                invocationContext.ClearRequestMessage();
            }
            catch (Exception e)
            {
                //Please do not modify the following line
                var exception = e.InnerException ?? e;
                this.TraceSource.TraceError(
                    "Method {0} invoke at  at type {1} has error occurred, detail:{2}",
                    invocationContext.MethodName,
                    invocationInstance.GetType().FullName, exception);
                throw new MicroKernelInternalInvocationException(exception.ToString(), exception);
            }
            finally
            {
                this.DestroyMicroKernelRuntime();
                this.ServiceLocator.Release(invocationInstance);
            }
            return message;
        }

        #region Suppose this three method never throw exception

        /// <summary>
        /// In some conditions, the generic parameter type may lead some issue, currently
        /// we hope we did not have this invoke of method
        /// </summary>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        Type[] GetInvocationGenericParameterArgs(CoreServiceInvocationContext invocationContext)
        {
            var result = new List<Type>();
            var genericParameterTypeNames = invocationContext.GenericParameterTypeNames;
            if (genericParameterTypeNames != null && genericParameterTypeNames.Count > 0)
                genericParameterTypeNames.ForEach(item => result.Add(Type.GetType(item)));
            return result.ToArray();
        }

        Type[] GetInvocationArgsTypes(CoreServiceInvocationContext invocationContext)
        {
            var result = new List<Type>();
            var argsTypeList = invocationContext.IsRedirectArgumentType
                ? invocationContext.ArgsShortTypeNames : invocationContext.ArgsTypeNames;
            if (argsTypeList != null && argsTypeList.Count > 0)
            {
                if (invocationContext.IsRedirectArgumentType)
                {
                    argsTypeList.ForEach(item =>
                    {
                        var itemType = this.GetRedirectItemType(
                            item,
                            invocationContext.RedirectAssemblyName ?? MicroKernelConstant.GCommonContactAssemblyName);
                        result.Add(itemType);
                    });
                }
                else argsTypeList.ForEach(item => result.Add(this.GetCompatibleType(item)));
            }
            return result.ToArray();
        }

        Type GetCompatibleType(String typeName)
        {
            var result = Type.GetType(typeName);
            if (result==null)
            {
                if (this.CompatibleTypeMappingDictionary!=null&&
                    this.CompatibleTypeMappingDictionary.ContainsKey(typeName))
                {
                    result = Type.GetType(this.CompatibleTypeMappingDictionary[typeName]);
                }
            }
            return result;
        }

        Type GetRedirectItemType(String typeName, String assemblyName)
        {
            Type result;
            if (cacheTypeDictionary.ContainsKey(typeName))
                result = cacheTypeDictionary[typeName];
            else
            {
                var itemRedirectAssembly = GetRedirectAssembly(assemblyName);
                result = itemRedirectAssembly.GetType(typeName, throwOnError: true, ignoreCase: true);
                cacheTypeDictionary[typeName] = result;
            }
            return result;
        }

        Assembly GetRedirectAssembly(String assemblyName)
        {
            Assembly result;
            if (cacheAssemblyDictionary.ContainsKey(assemblyName))
            {
                result = cacheAssemblyDictionary[assemblyName];
            }
            else
            {
                result = Assembly.Load(assemblyName);
                cacheAssemblyDictionary[assemblyName] = result;
            }
            return result;
        }

        Object[] GetInvocationArgs(CoreServiceInvocationContext invocationContext, Type[] argsTypeList)
        {
            var result = new List<Object>();
            var argsList = invocationContext.Args;
            if (argsList != null && argsList.Count > 0)
                result.AddRange(argsList.Select((t, i) => SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(t, argsTypeList[i])));
            return result.ToArray();
        }

        /// <summary>
        /// Build the microkernel runtime of invocation, as you may know, the runtime object
        /// can not deliver to another thread, be careful
        /// </summary>
        /// <param name="invocationContext">the microkernel invocation context</param>
        /// <returns>the runtime behalf the invocation</returns>
        MicroKernelRuntime BuildMicroKernelRuntime(CoreServiceInvocationContext invocationContext)
        {
            MicroKernelRuntime.Current = new MicroKernelRuntime
            {
                OperationContext = OperationContext.Current,
                ProxyContext = invocationContext.ProxyContext,
                ServerContext = MicroKernelContext.NativeContext
            };
            this.BuildMicroKernelRuntimeServerContextEndPointInfo(OperationContext.Current, MicroKernelRuntime.Current);

            if (MicroKernelRuntime.Current.ProxyContext == null)
            {
                MicroKernelRuntime.Current.ProxyContext = new MicroKernelContext();
            }
            this.BuildMicroKernelRuntimeProxyContextEndPointInfo(OperationContext.Current, MicroKernelRuntime.Current);

            return MicroKernelRuntime.Current;
        }

        // ReSharper disable UnusedParameter.Local
        void BuildMicroKernelRuntimeProxyContextEndPointInfo(OperationContext operationContext, MicroKernelRuntime microKernelRuntime)
        // ReSharper restore UnusedParameter.Local
        {
            var messageProperties = OperationContext.Current.IncomingMessageProperties;
            if (messageProperties.Keys.Contains(RemoteEndpointMessageProperty.Name))
            {
                var endpointRemoteEndpointMessageProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (endpointRemoteEndpointMessageProperty != null)
                {
                    microKernelRuntime.ProxyContext.IPAddress = endpointRemoteEndpointMessageProperty.Address;
                    microKernelRuntime.ProxyContext.Port = endpointRemoteEndpointMessageProperty.Port;
                }
            }
        }

        // ReSharper disable UnusedParameter.Local
        void BuildMicroKernelRuntimeServerContextEndPointInfo(OperationContext operationContext, MicroKernelRuntime microKernelRuntime)
        // ReSharper restore UnusedParameter.Local
        {
            var contextChannel = OperationContext.Current.Channel;
            if (contextChannel != null)
            {
                microKernelRuntime.ServerContext.IPAddress = OperationContext.Current.Channel.LocalAddress.Uri.DnsSafeHost;
                microKernelRuntime.ServerContext.Port = OperationContext.Current.Channel.LocalAddress.Uri.Port;
            }
        }

        /// <summary>
        /// after the invocation , destroy the runtime object
        /// </summary>
        void DestroyMicroKernelRuntime()
        {
            MicroKernelRuntime.Current = null;
        }

        String GetInvocationArgsValueString(Type[] argsTypes, Object[] args)
        {
            var argumentsValueResult = new StringBuilder(String.Empty);
            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    argumentsValueResult.AppendFormat(i != args.Length - 1 ? "{0}: [{1}]," : "{0}: [{1}]",
                                                      argsTypes[i].Name, Expand(args[i]));
                }
            }
            return argumentsValueResult.ToString();
        }

        Boolean IsExpandableType(Type testType)
        {
            return cachedExpandableType.Exists(type => type.IsAssignableFrom(testType));
        }

        String Expand(Object obj)
        {
            var result = default(String);
            if (obj != null)
            {
                if (IsExpandableType(obj.GetType()))
                {
                    if (obj is IList)
                    { result = ExpandListCollection(obj as IList); }
                    else if (obj is IDictionary)
                    { result = ExpandDictionary(obj as IDictionary); }
                }
                else result = obj.ToString();
            }

            return result;
        }

        String ExpandDictionary(IDictionary dictionary)
        {
            var result = new StringBuilder();
            foreach (var item in dictionary.Keys)
            {
                result.AppendFormat("key:[{0}],value:[{1}] {2} ", item, dictionary[item], Environment.NewLine);
            }
            return result.ToString();
        }

        String ExpandListCollection(IList list)
        {
            var result = new StringBuilder();
            foreach (var item in list)
            {
                result.AppendFormat("item value is:[{0}] {1} ", item, Environment.NewLine);
            }
            return result.ToString();
        }
        #endregion
    }
}