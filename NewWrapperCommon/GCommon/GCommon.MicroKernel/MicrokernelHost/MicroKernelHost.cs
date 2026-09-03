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
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIocContainer"></typeparam>
    public sealed class MicroKernelHost<TIocContainer>
    {
        readonly ServiceHostBase hostBase;

        /// <summary>
        /// Occurs when a communication object transitions into the closed state.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when a communication object transitions into the closing state.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs when a communication object transitions into the faulted state.
        /// </summary>
        public event EventHandler Faulted;

        /// <summary>
        /// Occurs when a communication object transitions into the opened state.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        ///  Occurs when a communication object transitions into the opening state.
        /// </summary>
        public event EventHandler Opening;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="baseAddress"></param>
        public MicroKernelHost(TIocContainer container, Uri baseAddress)
            : this(container, new List<Uri> { baseAddress }) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="baseAddress"></param>
        /// <param name="address"></param>
        /// <param name="thumbprint"></param>
        public MicroKernelHost(TIocContainer container, Uri baseAddress, String address, Object thumbprint = null)
            : this(container, typeof(CoreService), new List<Uri> { baseAddress }.ToArray(), new List<String> { address }, thumbprint, true) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="baseAddresses"></param>
        public MicroKernelHost(TIocContainer container, List<Uri> baseAddresses)
            : this(container, typeof(CoreService), baseAddresses.ToArray()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serviceType"></param>
        /// <param name="baseAddresses"></param>
        /// <param name="addresses"></param>
        /// <param name="thumbprint"></param>
        /// <param name="isNoConfigService"></param>
        public MicroKernelHost(
            TIocContainer container,
            Type serviceType,
            Uri[] baseAddresses,
            List<String> addresses = default(List<String>),
            Object thumbprint = default(Object),
            Boolean isNoConfigService = default(Boolean))
        {
            AppDomain.CurrentDomain.SetData(MicroKernelConstant.CoreIocContainerIdentifier, container);
            this.hostBase = new IocServiceHost<TIocContainer>(container, serviceType, baseAddresses, thumbprint);
            if (isNoConfigService && addresses != null)
            {
                addresses.ForEach(address =>
                {
                    var implementedContract = typeof(ICoreService).FullName;
                    if (implementedContract != null)
                        this.hostBase.AddServiceEndpoint(implementedContract,
                                                         CoreBindingBuilder.CustomBinding, address);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Open()
        {
            this.hostBase.Closed += this.Closed;
            this.hostBase.Closing += this.Closing;
            this.hostBase.Faulted += this.Faulted;
            this.hostBase.Opened += this.Opened;
            this.hostBase.Opening += this.Opening;
            this.hostBase.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (this.hostBase.State != CommunicationState.Closed)
                this.hostBase.Abort();
            this.hostBase.Closed -= this.Closed;
            this.hostBase.Closing -= this.Closing;
            this.hostBase.Faulted -= this.Faulted;
            this.hostBase.Opened -= this.Opened;
            this.hostBase.Opening -= this.Opening;
        }
    }
}
