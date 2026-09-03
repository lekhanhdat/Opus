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
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.MicroKernel.Lifestyle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Reflection;
using Castle.Facilities.Startable;
using Castle.Core;
using Castle.Core.Configuration;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using System.Web.Helpers;
using System.IdentityModel.Claims;
using AvePoint.RA.Api.Web.Common;

namespace AvePoint.RA.Api.Web.Config
{
    public class GlobalConfig
    {

        public static void Init()
        {
            InitCastle();
            InitDefaultData();
        }

        private static void InitDefaultData()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            //init default data in database if necesary
        }


        private static void InitCastle()
        {
            WindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Register(Classes.FromThisAssembly().BasedOn<Microsoft.AspNetCore.Mvc.ControllerBase>().LifestylePerWebRequest());
            //windsorContainer.Register(
            //    Component.For<IWindsorContainer>().Instance(windsorContainer),
            //    Component.For<System.Web.Http.Dependencies.IDependencyResolver>().ImplementedBy<WindsorDependencyResolver>()
            //);

            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile("Config/Castle/ServiceCastle.config"));

            //var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
            //windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
            AppDomain.CurrentDomain.SetData("CoreIOCContainerIdentifier", windsorContainer);

            GlobalConfiguration.Configuration.DependencyResolver = new WindsorDependencyResolver(windsorContainer.Kernel);
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(windsorContainer.Kernel)); 
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new WindsorHttpControllerActivator(windsorContainer));
            PlatformWindsorManager.SetUp(windsorContainer);
           
        }
    }

    public class WindsorDependencyScope : IDependencyScope
    {
        private readonly IKernel container;

        private readonly System.Web.Http.Dependencies.IDependencyResolver resolver;

        private readonly IDisposable scope;

        public WindsorDependencyScope(IKernel container)
        {
            this.container = container;
            this.scope = container.BeginScope();
        }

        public void Dispose()
        {
            this.scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return this.container.HasComponent(serviceType) ? this.container.Resolve(serviceType) : null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.container.ResolveAll(serviceType).Cast<object>();
        }
    }


    internal class WindsorDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly IKernel container;

        public WindsorDependencyResolver(IKernel container)
        {
            this.container = container;
        }

        public IDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(this.container);
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return this.container.HasComponent(serviceType) ? this.container.Resolve(serviceType) : null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.container.ResolveAll(serviceType).Cast<object>();
        }
    }
}