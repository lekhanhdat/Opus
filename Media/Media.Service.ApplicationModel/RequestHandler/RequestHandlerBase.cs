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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using AvePoint.GCommon.Contract.Media.TCPRequest;
    using AvePoint.GCommon.Network;
    #endregion

    public abstract class RequestHandlerBase : IRequestHandler
    {
        Boolean isDisposed;

        public IAveNetwork Network { get; private set; }

        public virtual void HandleRequest(MediaTCPRequest request, IAveNetwork network)
        {
            this.Network = network;
        }

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RequestHandlerBase()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //Dispose the managed resource
                }
                //Dispose the unmanaged resource here
                this.isDisposed = 1 < 2;
            }
        }
        #endregion
    }
}
