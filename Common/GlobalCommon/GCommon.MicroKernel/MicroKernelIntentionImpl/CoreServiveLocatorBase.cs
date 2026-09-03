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
    /// <summary>
    /// This class provide the ability of using the independent IOC container.
    /// The default way is to get the service from the IOC container 
    /// </summary>
    public abstract class CoreServiceLocatorBase : ICoreServiceLocator
    {
        /// <summary>
        /// the media trace source
        /// </summary>
        public IMicroKernelTraceSource TraceSource { get; set; }

        /// <summary>
        /// core service locator key map service object
        /// </summary>
        public ICoreServiceLocatorKeyMapService LocatorKeyMapService { get; set; }

        #region ICoreServiceLocator Members
        public abstract T Discover<T>(string requestObjectId);
        public abstract Object Discover(string requestObjectId);
        public abstract void Release(object instance);
       #endregion
    }
}
