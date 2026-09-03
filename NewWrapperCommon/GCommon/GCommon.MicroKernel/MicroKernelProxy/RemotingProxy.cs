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
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.ServiceModel;
    using System.Text;
    using System.Linq;

    #endregion using directives

    #region Attribute

    [DebuggerNonUserCode]

    #endregion Attribute

#pragma warning disable 1587
    /// <summary>
    /// This class is the core proxy generate class, to make it more stable, this class is declare
    /// as non debugable, for more details, please contract <see cref="yhzhang@avepoint.com">
    /// </summary>
    /// <typeparam name="TInterface">the proxy base interface type</typeparam>
#pragma warning restore 1587
    internal class RemotingProxy<TInterface> : RealProxy
    {
        // ReSharper disable StaticFieldInGenericType
        static readonly List<Type> cachedExpandableType = new List<Type> { typeof(IList), typeof(IDictionary) };
        // ReSharper restore StaticFieldInGenericType
        // ReSharper disable StaticFieldInGenericType
        static readonly Object syncRoot = new Object();
        // ReSharper restore StaticFieldInGenericType
        readonly EndpointInfo endpoint;
        readonly IMicroKernelTraceSource traceSource = new MicroKernelTraceSource();

        EventHandler<ProxyEventArgs> postProxyInvoke;
        EventHandler<ProxyEventArgs> preProxyInvoke;

        #region Constructor

        public RemotingProxy(EndpointInfo endpoint)
            : base(typeof(TInterface))
        {
            this.endpoint = endpoint;
        }

        #endregion Constructor

        #region Pre and Post PorxyInvoke

        /// <summary>
        /// this kind of style can be thread safely register event
        /// </summary>
        public event EventHandler<ProxyEventArgs> PostProxyInvoke
        {
            add
            {
                lock (syncRoot)
                    this.postProxyInvoke += value;
            }
            remove
            {
                lock (syncRoot)
                {
                    var eventHandler = this.postProxyInvoke;
                    if (eventHandler != null) this.postProxyInvoke -= value;
                }
            }
        }

        /// <summary>
        /// this kind of style can be thread safely register event
        /// </summary>
        public event EventHandler<ProxyEventArgs> PreProxyInvoke
        {
            add
            {
                lock (syncRoot)
                    this.preProxyInvoke += value;
            }
            remove
            {
                lock (syncRoot)
                {
                    var eventHandler = this.preProxyInvoke;
                    if (eventHandler != null) this.preProxyInvoke -= value;
                }
            }
        }

        #endregion Pre and Post PorxyInvoke

        public override IMessage Invoke(IMessage msg)
        {
            var stackTrace = this.GetCurrentInvokeStackTrace(Environment.StackTrace);
            var context = new InvocationContext { Request = (IMethodCallMessage)msg, StackTrace = stackTrace };
            this.PrivateInvoke(this.endpoint, context);
            return context.Reply;
        }

        protected virtual void OnPostProxyInvoke(ProxyEventArgs args)
        {
            var temp = this.postProxyInvoke;
            if (temp != null)
            {
                Array.ForEach(temp.GetInvocationList(), item =>
                {
                    try
                    {
                        item.DynamicInvoke(this, args);
                    }
                    catch (Exception e)
                    {
                        this.traceSource.TraceWarning("The PostProxyInvoke method [{0}] has error occurred, detail [{1}].", item.Method.Name, e.ToString());
                    }
                });
            }
        }

        protected virtual void OnPreProxyInvoke(ProxyEventArgs args)
        {
            var temp = this.preProxyInvoke;
            if (temp != null)
            {
                Array.ForEach(temp.GetInvocationList(), item =>
                {
                    try
                    {
                        item.DynamicInvoke(this, args);
                    }
                    catch (Exception e)
                    {
                        this.traceSource.TraceWarning("The PreProxyInvoke method [{0}] has error occurred, detail [{1}].", item.Method.Name, e.ToString());
                    }
                });
            }
        }

        private ReturnMessage ConvertWcfResultToRealProxyResult(CoreMessage wcfResult, InvocationContext context)
        {
            var invocationContext = wcfResult.InvocationContext;
            var returnValue = default(Object);
            if (invocationContext.ReturnValue != null)
            {
                var targetReturnType = Type.GetType(invocationContext.ReturnValueTrueType) ??
                    (Assembly.GetExecutingAssembly().GetType(invocationContext.ReturnValueTrueTypeWithoutAssemblyName) ??
                    (!String.IsNullOrEmpty(invocationContext.CompatibleReturnValueTrueType) ? Type.GetType(invocationContext.CompatibleReturnValueTrueType) : null) ??
                    ((MethodInfo)context.Request.MethodBase).ReturnType);
                returnValue = SerializerHelper.DeserializeFromBytesByDataContractSerializer(invocationContext.ReturnValue, targetReturnType);
            }
            var result = new ReturnMessage(returnValue, context.Request.Args, context.Request.ArgCount, context.Request.LogicalCallContext, context.Request);
            return result;
        }

        private List<String> GetArgsTypeAssemblyQualifiedNames(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll(parameters, item => item.GetType().AssemblyQualifiedName));
        }

        private List<String> GetArgsTypeName(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll(parameters, item => item.GetType().FullName));
        }

        // ReSharper disable ParameterHidesMember
        private CoreMessage GetCoreServiceInvocationParameter(EndpointInfo endpoint, InvocationContext context)
        // ReSharper restore ParameterHidesMember
        {
            var args = this.GetSerializedArgs(context.Request.Args);
            var argsShortTypeNames = this.GetArgsTypeName(context.Request.Args);
            var argsAssemblyQualifiedNames = this.GetArgsTypeAssemblyQualifiedNames(context.Request.Args);
            var genericParameterShortTypeNames = this.GetGenericParameterTypeNames(context.Request.MethodBase);
            var genericParameterTypeAssemblyQualifiedNames = this.GetGenericParameterTypeAssemblyQualifiedNames(context.Request.MethodBase);
            Debug.Assert(context.Request.MethodBase.DeclaringType != null, "context.Request.MethodBase.DeclaringType != null");
            var result = new CoreMessage
            {
                InvocationContext = new CoreServiceInvocationContext
                {
                    Args = args,
                    ArgsTypeNames = argsAssemblyQualifiedNames,
                    ArgsShortTypeNames = argsShortTypeNames,
                    GenericParameterTypeNames = genericParameterTypeAssemblyQualifiedNames,
                    GenericParameterShortTypeNames = genericParameterShortTypeNames,
                    ArgsCount = context.Request.ArgCount,
                    MethodName = this.GetOperationMethodName(endpoint, context),
                    TypeKey = this.GetTypeKey(endpoint, context),
                    TypeName = context.Request.TypeName,
                    Uri = context.Request.Uri,
                    IsRedirectArgumentType = endpoint.IsRedirectArgumentType,
                    RedirectAssemblyName = endpoint.RedirectAssemblyName,
                    ProxyContext = this.GetProxyContext(context)
                },
                AuthorizationKey = endpoint.AuthorizationKey,
                AllAccountProfilePwdCrc = endpoint.AllAccountProfilePwdCrc
            };
            return result;
        }

        private String GetTypeKey(EndpointInfo endpointInfo, InvocationContext context)
        {
            String result;
            Debug.Assert(context.Request.MethodBase.DeclaringType != null, "context.Request.MethodBase.DeclaringType != null");
            if (String.IsNullOrEmpty(endpointInfo.RemotingTypeKey))
            {
                result = context.Request.MethodBase.DeclaringType.FullName;
                var proxyProtocolAttributes = context.Request.MethodBase.DeclaringType.GetCustomAttributes(true)
                      .Where(attribute => attribute is IProxyProtocolConverter).ToList();
                if (proxyProtocolAttributes.Count > 0)
                {
                    var operationProtocol = proxyProtocolAttributes[0] as IProxyProtocolConverter;
                    Debug.Assert(operationProtocol != null, "operationProtocol != null");
                    result = operationProtocol.ConvertToCurrentType(result);
                }
            }
            else result = endpointInfo.RemotingTypeKey;
            return result;
        }

        private String GetOperationMethodName(EndpointInfo endpointInfo, InvocationContext context)
        {
            var result = context.Request.MethodName;
            if (!endpointInfo.IsUseOldMethod)
            {
                var operationProtocolAttributes = context.Request.MethodBase.GetCustomAttributes(true)
                       .Where(attribute => attribute is IOperationProtocolConverter).ToList();
                if (operationProtocolAttributes.Count > 0)
                {
                    var operationProtocol = operationProtocolAttributes[0] as IOperationProtocolConverter;
                    Debug.Assert(operationProtocol != null, "operationProtocol != null");
                    result = operationProtocol.ConvertToCurrentMethod();
                }
            }

            return result;
        }

        private String GetCurrentInvokeStackTrace(String stackTrace)
        {
            var resultStackTrace = stackTrace;
            const string splitString = "System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData& msgData, Int32 type)";
            var splitStringIndex = stackTrace.IndexOf(splitString, StringComparison.OrdinalIgnoreCase);
            if (splitStringIndex >= 0)
            {
                resultStackTrace = resultStackTrace.Substring(splitStringIndex + splitString.Length);
            }
            return resultStackTrace;
        }

        private List<String> GetGenericParameterTypeAssemblyQualifiedNames(MethodBase methodBase)
        {
            var result = new List<String>();
            if (methodBase.IsGenericMethod)
                Array.ForEach(methodBase.GetGenericArguments(), item => result.Add(item.AssemblyQualifiedName));
            return result;
        }

        private List<String> GetGenericParameterTypeNames(MethodBase methodBase)
        {
            var result = new List<String>();
            if (methodBase.IsGenericMethod)
                Array.ForEach(methodBase.GetGenericArguments(), item => result.Add(item.FullName));
            return result;
        }

        private MicroKernelContext GetProxyContext(InvocationContext context)
        {
            var result = MicroKernelContext.NativeContext;
            result.StackTrace = context.StackTrace;
            result.Extension = MicroKernelRuntimeCache.Extension;
            return result;
        }

        private List<String> GetSerializedArgs(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll(parameters, SerializerHelper.SerializeToBase64StringByDataContractSerializer));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        // ReSharper disable ParameterHidesMember
        private void PrivateInvoke(EndpointInfo endpoint, InvocationContext context)
        // ReSharper restore ParameterHidesMember
        {
            var coreMessage = this.GetCoreServiceInvocationParameter(endpoint, context);
            var coreServiceClientChannel = ClientChannelManager.GetClientChannel(endpoint);
            try
            {
                this.InvokeRemoteMethod(coreServiceClientChannel, context, coreMessage);
            }
            catch (CommunicationException)
            {
                this.RetryInvoke(context, coreMessage);
            }
            catch (System.ObjectDisposedException)
            {
                this.RetryInvoke(context, coreMessage);
            }
            catch (Exception e)
            {
                context.Reply = new ReturnMessage(e, context.Request);
            }
        }

        private void InvokeRemoteMethod(ICoreServiceClientChannel coreServiceClientChannel, InvocationContext context, CoreMessage coreMessage)
        {
            using (new OperationContextScope(coreServiceClientChannel))
            {
                //We use operation context here because only in the new operation context scope the operation context current
                //property is valid
                var args = new ProxyEventArgs { OperationContext = OperationContext.Current, InvocationContext = context };
                try
                {
                    /**
                     * When we invoke the GetInvocationArgsValueString method, we convert the context.Request.MethodSignature object
                     * instance to a Type array, though we can debug the application and find out it's real type is a type array, we
                     * are not sure in some conditions it will be changed, finally we get the description of this property in url:
                     * http://msdn.microsoft.com/en-us/library/system.runtime.remoting.messaging.imethodmessage.methodsignature.aspx
                     * In this page's remarks section, it explains as follows:All the current implementations of IMethodMessage return
                     * an array of Type objects containing the parameter types of the method.
                     *
                     * so we change the type to type array and get the arguments description string
                     */
                    var argsValueString = this.GetInvocationArgsValueString((Type[])context.Request.MethodSignature, context.Request.Args);
                    this.traceSource.TraceInformation(@"Microkernel proxy begin to invoke {0}.{1}({2}){3} on server {4} via {5} protocol, current stack trace:{6}",
                         context.Request.MethodBase.ReflectedType.FullName,
                         context.Request.MethodName,
                         argsValueString,
                         Environment.NewLine,
                         endpoint.HostOrIpAddress,
                         endpoint.Scheme,
                         context.StackTrace);
                    this.OnPreProxyInvoke(args);
                    var wcfResult = coreServiceClientChannel.HandleMessage(coreMessage);
                    if (wcfResult.IsExceptionOccurred)
                    {
                        throw new MicroKernelInternalInvocationException(wcfResult.ExceptionMessage)
                        {
                            ExceptionDetails = wcfResult.ExceptionDetails,
                            ExceptionMessage = wcfResult.ExceptionMessage,
                            ExceptionRawMessage = wcfResult.ExceptionRawMessage
                        };
                    }
                    else
                    {
                        context.Reply = this.ConvertWcfResultToRealProxyResult(wcfResult, context);
                        this.traceSource.TraceInformation(@"Microkernel proxy end to invoke {0}.{1}{2} on server {3} by {4}, return value:{5}.",
                          context.Request.MethodBase.ReflectedType.FullName,
                          context.Request.MethodName,
                          Environment.NewLine,
                          endpoint.HostOrIpAddress,
                          endpoint.Scheme,
                          Expand(context.Reply.ReturnValue));
                    }
                }
                finally { this.OnPostProxyInvoke(args); }
            }
        }
        private void RetryInvoke(InvocationContext context, CoreMessage coreMessage)
        {
            var coreServiceClientChannel = ClientChannelManager.GetClientChannel(endpoint);
            try
            {
                this.InvokeRemoteMethod(coreServiceClientChannel, context, coreMessage);
            }
            catch (Exception e)
            {
                context.Reply = new ReturnMessage(e, context.Request);
            }
        }

        #region This section is copy from another location of microkernel project, need to be restructure in future

        private String Expand(Object obj)
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

        private String ExpandDictionary(IDictionary dictionary)
        {
            var result = new StringBuilder();
            foreach (var item in dictionary.Keys)
            {
                result.AppendFormat("key:[{0}],value:[{1}] {2} ", item, dictionary[item], Environment.NewLine);
            }
            return result.ToString();
        }

        private String ExpandListCollection(IList list)
        {
            var result = new StringBuilder();
            foreach (var item in list)
            {
                result.AppendFormat("item value is:[{0}] {1} ", item, Environment.NewLine);
            }
            return result.ToString();
        }

        //HACK: need to be restructure
        //TODO: need to be restructure
        private String GetInvocationArgsValueString(Type[] argsTypes, Object[] args)
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

        private Boolean IsExpandableType(Type testType)
        {
            return cachedExpandableType.Exists(type => type.IsAssignableFrom(testType));
        }

        #endregion This section is copy from another location of microkernel project, need to be restructure in future
    }
}