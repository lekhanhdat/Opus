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
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

    public class DefaultCoreMessageHandler : CoreMessageHandler<CoreMessage>
    {
        static List<Type> cachedExpandableType = new List<Type>() { typeof(IList), typeof(IDictionary) };
        static Dictionary<String, Type> cacheTypeDictionary = new Dictionary<String, Type>();
        static Dictionary<String, Assembly> cacheAssemblyDictionary = new Dictionary<String, Assembly>();

        public ICoreServiceLocator ServiceLocator { get; set; }

        public override CoreMessage ProcessMessage(CoreMessage message)
        {
            var invocationContext = message.InvocationContext;
            var invocationInstance = this.ServiceLocator.Discover(invocationContext.TypeKey);
            var argsTypes = this.GetInvocationArgsTypes(invocationContext);
            var args = this.GetInvocationArgs(invocationContext, argsTypes);
            var methodInfo = invocationInstance.GetType().GetMethod(invocationContext.MethodName);
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
                    invocationContext.ReturnValue = SerializerHelper.SerializeToBytesByDataContractSerializer(result);
                    invocationContext.ReturnValueTrueType = result.GetType().AssemblyQualifiedName;
                }
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
                        var itemType = this.GetRedirectItemType(item, invocationContext.RedirectAssemblyName ?? MicroKernelConstant.GCommonContactAssemblyName);
                        result.Add(itemType);
                    });
                }
                else argsTypeList.ForEach(item => result.Add(Type.GetType(item)));
            }
            return result.ToArray();
        }

        Type GetRedirectItemType(String typeName, String assemblyName)
        {
            var result = default(Type);
            if (cacheTypeDictionary.ContainsKey(typeName))
                result = cacheTypeDictionary[typeName];
            else
            {
                var itemRedirectAssembly = GetRedirectAssembly(assemblyName);
                result = itemRedirectAssembly.GetType(name: typeName, throwOnError: true, ignoreCase: true);
                cacheTypeDictionary[typeName] = result;
            }
            return result;
        }

        Assembly GetRedirectAssembly(String assemblyName)
        {
            var result = default(Assembly);
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
            {
                for (int i = 0; i < argsList.Count; i++)
                {
                    var argObj = SerializerHelper.DeserializeFromBase64StringByDataContractSerializer(argsList[i], argsTypeList[i]);
                    result.Add(argObj);
                }
            }
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

        void BuildMicroKernelRuntimeProxyContextEndPointInfo(OperationContext operationContext, MicroKernelRuntime microKernelRuntime)
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

        void BuildMicroKernelRuntimeServerContextEndPointInfo(OperationContext operationContext, MicroKernelRuntime microKernelRuntime)
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
                    if (i != args.Length - 1)
                        argumentsValueResult.AppendFormat("{0}: [{1}],", argsTypes[i].Name, Expand(args[i]));
                    else argumentsValueResult.AppendFormat("{0}: [{1}]", argsTypes[i].Name, Expand(args[i]));
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