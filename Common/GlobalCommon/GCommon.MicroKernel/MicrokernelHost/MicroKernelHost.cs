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
    using System.ServiceModel;
    #endregion

    public sealed class MicroKernelHost<TIocContainer>
    {
        ServiceHostBase hostBase;

        // Summary:
        //     Occurs when a communication object transitions into the closed state.
        public event EventHandler Closed;
        //
        // Summary:
        //     Occurs when a communication object transitions into the closing state.
        public event EventHandler Closing;
        //
        // Summary:
        //     Occurs when a communication object transitions into the faulted state.
        public event EventHandler Faulted;
        //
        // Summary:
        //     Occurs when a communication object transitions into the opened state.
        public event EventHandler Opened;
        //
        // Summary:
        //     Occurs when a communication object transitions into the opening state.
        public event EventHandler Opening;

        public MicroKernelHost(TIocContainer container, Uri baseAddress)
            : this(container, new List<Uri> { baseAddress }) { }

        public MicroKernelHost(TIocContainer container, Uri baseAddress, String address, Object thumbprint = null)
            : this(container, typeof(CoreService), new List<Uri> { baseAddress }.ToArray(), new List<String> { address }, thumbprint, true) { }

        public MicroKernelHost(TIocContainer container, List<Uri> baseAddresses)
            : this(container, typeof(CoreService), baseAddresses.ToArray()) { }

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
                addresses.ForEach(address => this.hostBase.AddServiceEndpoint(typeof(ICoreService).FullName, CoreBindingBuilder.CustomBinding, address));
        }

        public void Open()
        {
            this.hostBase.Closed += this.Closed;
            this.hostBase.Closing += this.Closing;
            this.hostBase.Faulted += this.Faulted;
            this.hostBase.Opened += this.Opened;
            this.hostBase.Opening += this.Opening;
            this.hostBase.Open();
        }

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
