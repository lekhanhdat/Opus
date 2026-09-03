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
using Castle.MicroKernel;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace AvePoint.RA.Web.Common
{
    public class WindsorHttpControllerActivator : IHttpControllerActivator
    {
        IWindsorContainer kernel;

        public WindsorHttpControllerActivator(IWindsorContainer kernel)
        {
            this.kernel = kernel;
        }
        public System.Web.Http.Controllers.IHttpController Create(System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var controller = (IHttpController)kernel.Resolve(controllerType);
 
            //request.RegisterForDispose(new Release(() => kernel.Release(controller)));
  
            return controller;
        }


    }

    public class Release : IDisposable
    {

        public void Dispose()
        {
             
        }
    }
}