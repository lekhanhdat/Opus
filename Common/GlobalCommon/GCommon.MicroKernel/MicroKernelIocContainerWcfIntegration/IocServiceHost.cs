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
    using System.ServiceModel;
    using System.ServiceModel.Description;
    #endregion

    /// <summary>
    /// Provider a communication host which use a Dependency injection to
    /// to get the endpoint dispatcher running time instance provider
    /// <remarks> Generic type here used as a type identifier in internal calls,
    /// if we just pass the container object in the class. we can't get 
    /// the container type if the container object instance is null
    /// </remarks>
    /// </summary>
    /// <typeparam name="TIocContainer">The Ioc Container type</typeparam>
    #region Attribute
    [DebuggerNonUserCode]
    #endregion
    internal class IocServiceHost<TIocContainer> : ServiceHost
    {
        TIocContainer iocContainer;
        Object thumbprint;

        public IocServiceHost(
            TIocContainer container, 
            Type serviceType, 
            Uri[] baseAddresses, 
            Object thumbprint)
            : base(serviceType, baseAddresses)
        {
            this.iocContainer = container;
            this.thumbprint = thumbprint;
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            
            if (this.Description.Behaviors.Find<DependencyInjectionServiceBehavior<TIocContainer>>() == null)
                this.Description.Behaviors.Add(CoreServiceBehaviorBuilder.BuildDependencyInjectionServiceBehavior(this.iocContainer));
            if (this.Description.Behaviors.Find<ServiceCredentials>() == null)
                this.Description.Behaviors.Add(CoreServiceBehaviorBuilder.BuildCredentialsBehavior(this.thumbprint));
            if (this.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
                this.Description.Behaviors.Add(CoreServiceBehaviorBuilder.BuildThrottlingBehavior());
            if (this.Description.Behaviors.Find<ServiceDebugBehavior>() == null)
                this.Description.Behaviors.Add(CoreServiceBehaviorBuilder.BuildDebugBehavior());
            else this.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
        }
    }
}


