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
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common
{
    public class PlatformWindsorManager
    {
        private static WindsorContainer windsorContainer;

        public static void SetUp(WindsorContainer container)
        {
            windsorContainer = container;
        }

        public static WindsorContainer Container => windsorContainer;

        public static T GetService<T>()
        {
            return windsorContainer.Kernel.HasComponent(typeof(T)) ? (T)windsorContainer.Kernel.Resolve(typeof(T)) : default(T);
        }
        public static T GetService<T>(ref T service)
        {
            if(EqualityComparer<T>.Default.Equals(service, default))
            {
                service = GetService<T>();
            }
            return service;
        }
        public static object GetService(Type serviceType)
        {
            return windsorContainer.Kernel.HasComponent(serviceType) ? windsorContainer.Kernel.Resolve(serviceType) : null;
        }
        public static object GetService(string serviceKey, Type serviceType)
        {
            return windsorContainer.Kernel.HasComponent(serviceKey) ? windsorContainer.Kernel.Resolve(serviceKey,serviceType) : null;
        }

        public static T GetService<T>(string serviceKey)
        {
            return windsorContainer.Kernel.HasComponent(serviceKey) ? (T)windsorContainer.Kernel.Resolve(serviceKey, typeof(T)) : default(T);
        }
        public static IEnumerable<object> GetServices(Type serviceType)
        {
            return windsorContainer.Kernel.ResolveAll(serviceType).Cast<object>();
        }
    }
}
