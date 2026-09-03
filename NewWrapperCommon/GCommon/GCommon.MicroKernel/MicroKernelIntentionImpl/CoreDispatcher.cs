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
    using System.Diagnostics;

    #endregion

    #region Attribute
    /// <summary>
    /// The dispatcher of the message, as a default one
    /// </summary>
    [DebuggerNonUserCode]
    #endregion
    public class CoreDispatcher : ICoreDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public ICoreMessageHandlerFactory CoreMessageHandlerFactory { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IMicroKernelTraceSource TraceSource { get; set; }
        #region ICoreDispatcher Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoreMessage DispatchMessage(CoreMessage message)
        {
            var messageTypeFullName = message.GetType().FullName;
            this.TraceSource.TraceInformation("MicroKernel Core Dispatcher dispatch message type {0}", messageTypeFullName);
            var messageHandler = this.CoreMessageHandlerFactory.CreateMessageHandler(messageTypeFullName);
            return messageHandler.HandleMessage(message);
        }

        #endregion
    }
}
