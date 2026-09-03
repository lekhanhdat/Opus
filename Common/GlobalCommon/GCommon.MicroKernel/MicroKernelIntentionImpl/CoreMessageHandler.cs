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

    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

    public abstract class CoreMessageHandler<TMessage> : ICoreMessageHandler where TMessage : CoreMessage, new()
    {
        public Boolean IsDisposed { get; private set; }
        public IMicroKernelTraceSource TraceSource { get; set; }

        #region ICoreMessageHandler Members

        public abstract TMessage ProcessMessage(TMessage message);

        public CoreMessage HandleMessage(CoreMessage coreMessage)
        {
            var result = default(CoreMessage);
            var processMethod = this.GetType().GetMethod("ProcessMessage");
            var genericArgType = this.GetType().BaseType.GetGenericArguments()[0];
            result = processMethod.Invoke(this, new Object[] { coreMessage }) as CoreMessage;
            return result;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CoreMessageHandler()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    //Dispose managed resource
                }

                //Dispose unmanaged resource
                this.IsDisposed = true;
            }
        }

        #endregion
    }
}