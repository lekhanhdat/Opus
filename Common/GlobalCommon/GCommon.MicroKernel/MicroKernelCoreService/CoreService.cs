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
    using System.Diagnostics;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false,
        InstanceContextMode = InstanceContextMode.PerCall)]
    #endregion

    /// <summary>
    /// CoreService class is the MicroKernel core class and the main entry of the
    /// the controlservice, mediaservice, agentservice, reportservice,etc
    /// </summary>
    public class CoreService : ICoreService
    {
        /// <summary>
        /// The Dispatcher of the CoreMessage
        /// </summary>
        public ICoreDispatcher Dispatcher { get; set; }

        /// <summary>
        /// The chain of the core service operation Interception
        /// </summary>
        public ICoreServiceOperationInterseption CoreServiceOperationInterseption { get; set; }

        /// <summary>
        /// the trace source of the microkernel
        /// </summary>
        public IMicroKernelTraceSource TraceSource { get; set; }

        #region ICoreService Members

        /// <summary>
        /// Handle a message which invoke the microkernel
        /// </summary>
        /// <param name="message">the coremessage object</param>
        /// <returns>the wrapped result</returns>
        public CoreMessage HandleMessage(CoreMessage message)
        {
            var result = message;
            this.TraceSource.TraceInformation("Begin to invoke CoreService HandleMessage method , type of message:{0}", message.GetType().AssemblyQualifiedName);

            try
            {
                var context = new InterseptionContext { CoreMessage = message, OperationContext = OperationContext.Current };

                if (this.CoreServiceOperationInterseption != null)
                    this.CoreServiceOperationInterseption.PreCoreServiceOperationInvoke(context);

                result = this.Dispatcher.DispatchMessage(message);

                if (this.CoreServiceOperationInterseption != null)
                    this.CoreServiceOperationInterseption.PostCoreServiceOperationInvoke(context);
            }
            catch (Exception exception)
            {
                if (exception is TargetInvocationException)
                {
                    exception = exception.InnerException ?? exception;
                    if (exception is MicroKernelInternalInvocationException)
                        exception = exception.InnerException ?? exception;
                }
                result.IsExceptionOccurred = true;
                result.ExceptionDetails = exception.GetExceptionDetail();
                result.ExceptionMessage = exception.GetExpandedMessage();
                result.ExceptionRawMessage = exception.GetRawMessage();
                this.TraceSource.TraceError("When dispatching microkernel internal call has error occurred, details:{0}", result.ExceptionDetails);
            }

            this.TraceSource.TraceInformation("End to invoke CoreService HandleMessage method, type of message:{0}", message.GetType().AssemblyQualifiedName);

            return result;
        }

        /// <summary>
        /// Check if the core message is running
        /// </summary>
        /// <returns>
        /// If true, the service is running, else the invoke will not go here,  and
        /// receive an WCF exception
        /// </returns>
        public Boolean IsServiceRunning()
        {
            return !String.IsNullOrEmpty("I'm OK");
        }

        #endregion
    }
}