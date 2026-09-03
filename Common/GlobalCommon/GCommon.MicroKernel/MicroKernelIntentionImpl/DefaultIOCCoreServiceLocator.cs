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
    /// The default 
    /// </summary>
    public class DefaultIOCCoreServiceLocator : CoreServiceLocatorBase
    {
        static IIocContainerAnalyzer containerAnalyzer;
        static DefaultIOCCoreServiceLocator()
        {
            var container = AppDomain.CurrentDomain.GetData(MicroKernelConstant.CoreIocContainerIdentifier);
            if (containerAnalyzer == null)
                containerAnalyzer = IocContainerAnalyzerFactory.GetContainerAnalyzer(container);
        }

        /// <summary>
        /// This method mark as virtual in order to provide way to make more powerful
        /// </summary>
        /// <typeparam name="T">the request type</typeparam>
        /// <param name="requestObjectId">the request id</param>
        /// <returns>the discovered object</returns>
        public override T Discover<T>(String requestObjectId)
        {
            if (this.LocatorKeyMapService != null)
                requestObjectId = this.LocatorKeyMapService.MapKey(requestObjectId);

            this.TraceSource.TraceInformation("After mapping request id in generic discover method, the final request id is {0}", requestObjectId);

            return (T)containerAnalyzer.ResolveById(requestObjectId, typeof(T));
        }

        /// <summary>
        /// This method mark as virtual in order to provide way to make more powerful
        /// </summary>
        /// <param name="requestObjectId">the request id</param>
        /// <returns>the discovered object</returns>
        public override Object Discover(String requestObjectId)
        {
            if (this.LocatorKeyMapService != null)
                requestObjectId = this.LocatorKeyMapService.MapKey(requestObjectId);

            this.TraceSource.TraceInformation("After mapping request id in discover method, the final request id is {0}", requestObjectId);

            return containerAnalyzer.ResolveById(requestObjectId, (Type)null);
        }

        /// <summary>
        /// Provide a ability to release the object instance
        /// </summary>
        /// <param name="instance"></param>
        public override void Release(Object instance)
        {
            containerAnalyzer.Release(instance);
        }
    }
}
