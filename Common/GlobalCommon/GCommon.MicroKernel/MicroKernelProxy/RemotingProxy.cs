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

    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

    /// <summary>
    /// This class is the core proxy generate class, to make it more stable, this class is declare
    /// as non debugable, for more details, please contract <see cref="yhzhang@avepoint.com">
    /// </summary>
    /// <typeparam name="TInterface">the proxy base interface type</typeparam>
    internal class RemotingProxy<TInterface> : RealProxy
    {
        static Object syncRoot = new Object();
        static List<Type> cachedExpandableType = new List<Type>() { typeof(IList), typeof(IDictionary) };
        IMicroKernelTraceSource traceSource = new MicroKernelTraceSource();
        EndpointInfo endpoint;

        EventHandler<ProxyEventArgs> preProxyInvoke;
        EventHandler<ProxyEventArgs> postProxyInvoke;

        #region Constructor

        public RemotingProxy(EndpointInfo endpoint)
            : base(typeof(TInterface))
        {
            this.endpoint = endpoint;
        }

        #endregion

        #region Pre and Post PorxyInvoke

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
                    this.preProxyInvoke -= value;
            }
        }

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
                    this.postProxyInvoke -= value;
            }
        }

        #endregion

        public override IMessage Invoke(IMessage msg)
        {
            var statckTrace = this.GetCurrentInvokeStactTrace(Environment.StackTrace);
            var context = new InvocationContext { Request = (IMethodCallMessage)msg, StackTrace = statckTrace };
            this.PrivateInvoke(this.endpoint, context);
            return context.Reply;
        }

        protected virtual void OnPreProxyInvoke(ProxyEventArgs args)
        {
            var temp = this.preProxyInvoke;
            if (temp != null)
            {
                Array.ForEach<Delegate>(temp.GetInvocationList(), item =>
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

        protected virtual void OnPostProxyInvoke(ProxyEventArgs args)
        {
            var temp = this.postProxyInvoke;
            if (temp != null)
            {
                Array.ForEach<Delegate>(temp.GetInvocationList(), item =>
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        void PrivateInvoke(EndpointInfo endpoint, InvocationContext context)
        {
            var coreMessage = this.GetCoreServiceInvocationParameter(endpoint, context);
            var coreServiceClientChannel = ClientChannelManager.GetClientChannel(endpoint);
            try
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
            catch (Exception e)
            {
                context.Reply = new ReturnMessage(e, context.Request);
            }
        }

        CoreMessage GetCoreServiceInvocationParameter(EndpointInfo endpoint, InvocationContext context)
        {
            var args = this.GetSerializedArgs(context.Request.Args);
            var argsShortTypeNames = this.GetArgsTypeName(context.Request.Args);
            var argsAssemblyQualifiedNames = this.GetArgsTypeAssemblyQualifiedNames(context.Request.Args);
            var genericParameterShortTypeNames = this.GetGenericParameterTypeNames(context.Request.MethodBase);
            var genericParameterTypeAssemblyQualifiedNames = this.GetGenericParameterTypeAssemblyQualifiedNames(context.Request.MethodBase);
            var result = new CoreMessage
            {
                InvocationContext = new CoreServiceInvocationContext
                {
                    Args = args,
                    ArgsTypeNames = argsAssemblyQualifiedNames,
                    ArgsShortTypeNames = argsShortTypeNames,
                    GenericParameterTypeNames = genericParameterTypeAssemblyQualifiedNames,
                    GenericParameterShortTypeNames = genericParameterShortTypeNames,
                    IdentityType = IdentityManager.IdentityType,
                    IdentityContent = IdentityManager.IdentityContent,
                    ArgsCount = context.Request.ArgCount,
                    MethodName = context.Request.MethodName,
                    TypeKey = endpoint.RemotingTypeKey ?? context.Request.MethodBase.DeclaringType.FullName,
                    TypeName = context.Request.TypeName,
                    Uri = context.Request.Uri,
                    IsRedirectArgumentType = endpoint.IsRedirectArgumentType,
                    RedirectAssemblyName = endpoint.RedirectAssemblyName,
                    ProxyContext = this.GetProxyContext(context)
                },
                AuthorizationKey = endpoint.AuthorizationKey
            };
            return result;
        }

        MicroKernelContext GetProxyContext(InvocationContext context)
        {
            var result = MicroKernelContext.NativeContext;
            result.StackTrace = context.StackTrace;
            result.Extension = MicroKernelRuntimeCache.Extension;
            return result;
        }

        List<String> GetGenericParameterTypeNames(MethodBase methodBase)
        {
            var result = new List<String>();
            if (methodBase.IsGenericMethod)
                Array.ForEach<Type>(methodBase.GetGenericArguments(), item => result.Add(item.FullName));
            return result;
        }

        List<String> GetGenericParameterTypeAssemblyQualifiedNames(MethodBase methodBase)
        {
            var result = new List<String>();
            if (methodBase.IsGenericMethod)
                Array.ForEach<Type>(methodBase.GetGenericArguments(), item => result.Add(item.AssemblyQualifiedName));
            return result;
        }

        List<String> GetArgsTypeAssemblyQualifiedNames(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll<Object, String>(parameters, item => item.GetType().AssemblyQualifiedName));
        }

        List<String> GetArgsTypeName(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll<Object, String>(parameters, item => item.GetType().FullName));
        }

        List<String> GetSerializedArgs(Object[] parameters)
        {
            return new List<String>(Array.ConvertAll<Object, String>(parameters, item => SerializerHelper.SerializeToBase64StringByDataContractSerializer(item)));
        }

        ReturnMessage ConvertWcfResultToRealProxyResult(CoreMessage wcfResult, InvocationContext context)
        {
            var invocationContext = wcfResult.InvocationContext;
            var returnValue = default(Object);
            if (invocationContext.ReturnValue != null)
                returnValue = SerializerHelper.DeserializeFromBytesByDataContractSerializer(invocationContext.ReturnValue,
                     Type.GetType(invocationContext.ReturnValueTrueType) ?? ((MethodInfo)context.Request.MethodBase).ReturnType);
            var result = new ReturnMessage(returnValue, context.Request.Args, context.Request.ArgCount, context.Request.LogicalCallContext, context.Request);
            return result;
        }

        #region This section is copy from another location of microkernel project, need to be restructure in future

        //HACK: need to be restructure
        //TODO: need to be restructure
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

        String GetCurrentInvokeStactTrace(String stackTrace)
        {
            var resultStackTrace = stackTrace;
            var splitString = "System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData& msgData, Int32 type)";
            var splitStringIndex = stackTrace.IndexOf(splitString);
            if (splitStringIndex >= 0)
            {
                resultStackTrace = resultStackTrace.Substring(splitStringIndex + splitString.Length);
            }
            return resultStackTrace;
        }
    }
}