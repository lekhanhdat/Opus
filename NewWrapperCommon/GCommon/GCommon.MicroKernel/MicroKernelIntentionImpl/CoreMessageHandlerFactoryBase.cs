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
    using System.Collections.Generic;
    using System.Diagnostics;
    #endregion

    #region Attribute
    ///<Summary>
    /// Represent a ICoreMessageHandlerFactory, Which provide a basic function of the
    /// factory.
    ///</Summary>
    [DebuggerNonUserCode]
    #endregion

    public abstract class CoreMessageHandlerFactoryBase : ICoreMessageHandlerFactory
    {
        static readonly Object syncRoot = new Object();
        readonly Dictionary<String, ICoreMessageHandler> cachedMessageHandler = new Dictionary<String, ICoreMessageHandler>();

        /// <summary>
        /// 
        /// </summary>
        public IMicroKernelTraceSource TraceSource { get; set; }

        #region ICoreMessageHandlerFactory Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHandlerKey"></param>
        /// <returns></returns>
        public virtual ICoreMessageHandler CreateMessageHandler(String messageHandlerKey)
        {
            ICoreMessageHandler handler;
            if (!this.cachedMessageHandler.TryGetValue(messageHandlerKey, out handler))
            {
                lock (syncRoot)
                {
                    if (!this.cachedMessageHandler.TryGetValue(messageHandlerKey, out handler))
                    {
                        this.TraceSource.TraceInformation("Begin to initialize a message handler with message key {0}", messageHandlerKey);
                        handler = this.GetMessageHandler(messageHandlerKey);
                        this.cachedMessageHandler[messageHandlerKey] = handler;
                    }
                }
            }
            else
            {
                if (handler == null || handler.IsDisposed)
                {
                    lock (syncRoot)
                    {
                        if (handler == null || handler.IsDisposed)
                        {
                            this.TraceSource.TraceInformation("Begin to reinitialize a message handler with message key {0}", messageHandlerKey);
                            this.cachedMessageHandler.Remove(messageHandlerKey);
                            handler = this.GetMessageHandler(messageHandlerKey);
                            this.cachedMessageHandler[messageHandlerKey] = handler;
                        }
                    }
                }
            }

            return handler;
        }

        /// <summary>
        /// In order to implements the factory, you should override this method in 
        /// your own class.
        /// </summary>
        /// <param name="messageHandlerKey">The message handler key</param>
        /// <returns>a message handler</returns>
        public abstract ICoreMessageHandler GetMessageHandler(String messageHandlerKey);

        #endregion
    }
}
