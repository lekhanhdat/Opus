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
    /// <summary>
    /// This class provide the ability of using the independent IOC container which managed by
    /// Microkernel.
    /// </summary>
    [DebuggerNonUserCode]
    #endregion
  
    public static class MicroKernelServiceLocator
    {
        static readonly IIocContainerAnalyzer iocContainerAnalyzer = IocContainerAnalyzerFactory.GetContainerAnalyzer(AppDomain.CurrentDomain.GetData(MicroKernelConstant.CoreIocContainerIdentifier));

        /// <summary>
        /// Discover a service which managed by microkernel, 
        /// </summary>
        /// <typeparam name="T">the service type in generic style</typeparam>
        /// <param name="serviceId">the service id</param>
        /// <returns>the discover service</returns>
        public static T Discover<T>(String serviceId = default(String))
        {
            return (T)(String.IsNullOrEmpty(serviceId) ? iocContainerAnalyzer.ResolveByType(typeof(T)) : iocContainerAnalyzer.ResolveById(serviceId));
        }

        /// <summary>
        /// release the service which discovered by service locator
        /// </summary>
        /// <param name="service">the service instance</param>
        public static void Destory(Object service)
        {
            iocContainerAnalyzer.Release(service);
        }
    }
}
