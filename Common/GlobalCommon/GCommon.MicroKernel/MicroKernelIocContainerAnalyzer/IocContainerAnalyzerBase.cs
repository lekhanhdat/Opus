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
    using System.Reflection;
    #endregion

    #region Attribute
    [DebuggerNonUserCode]
    #endregion
  
    public abstract class IocContainerAnalyzerBase : IIocContainerAnalyzer
    {
        Object iocContainer;
        MethodInfo resolveByTypeMethod;
        MethodInfo resolveByIdMethod;
        MethodInfo releaseMethod;

        IocContainerResolveByTypeAnalyzer resolveByTypeAnalyzer;
        IocContainerResolveByIdAnalyzerEx resolveByIdAnalyzer;
        IocContainerReleaseAnalyzer releaseAnalyzer;

        protected IocContainerAnalyzerBase(Object container)
        {
            this.iocContainer = container;
        }

        public IocContainerResolveByTypeAnalyzer ResolveByType
        {
            get
            {
                if (this.resolveByTypeMethod == null && this.resolveByTypeAnalyzer == null)
                {
                    this.resolveByTypeMethod = this.GetResolveByTypeMethod(this.iocContainer.GetType());
                    this.resolveByTypeAnalyzer = Delegate.CreateDelegate(typeof(IocContainerResolveByTypeAnalyzer), this.iocContainer, this.resolveByTypeMethod) as IocContainerResolveByTypeAnalyzer;
                }
                return this.resolveByTypeAnalyzer;
            }
        }

        public IocContainerResolveByIdAnalyzerEx ResolveById
        {
            get
            {
                if (this.resolveByIdMethod == null && this.resolveByIdAnalyzer == null)
                {
                    this.resolveByIdMethod = this.GetResolveByIdMethod(this.iocContainer.GetType());
                    this.resolveByIdAnalyzer = Delegate.CreateDelegate(typeof(IocContainerResolveByIdAnalyzerEx), this.iocContainer, this.resolveByIdMethod) as IocContainerResolveByIdAnalyzerEx;
                }
                return this.resolveByIdAnalyzer;
            }
        }

        public IocContainerReleaseAnalyzer Release
        {
            get
            {
                if (this.releaseMethod == null && this.releaseAnalyzer == null)
                {
                    this.releaseMethod = this.GetReleaseMethod(this.iocContainer.GetType());
                    this.releaseAnalyzer = Delegate.CreateDelegate(typeof(IocContainerReleaseAnalyzer), this.iocContainer, this.releaseMethod) as IocContainerReleaseAnalyzer;
                }
                return this.releaseAnalyzer;
            }
        }

        protected abstract MethodInfo GetResolveByTypeMethod(Type containerType);
        protected abstract MethodInfo GetResolveByIdMethod(Type containerType);
        protected abstract MethodInfo GetReleaseMethod(Type containerType);
    }
}
