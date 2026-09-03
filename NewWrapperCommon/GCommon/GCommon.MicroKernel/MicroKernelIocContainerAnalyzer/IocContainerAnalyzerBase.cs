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
    /// <summary>
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    #endregion
  
    public abstract class IocContainerAnalyzerBase : IIocContainerAnalyzer
    {
        readonly Object iocContainer;
        MethodInfo resolveByTypeMethod;
        MethodInfo resolveByIdMethod;
        MethodInfo releaseMethod;

        IocContainerResolveByTypeAnalyzer resolveByTypeAnalyzer;
        IocContainerResolveByIdAnalyzer resolveByIdAnalyzer;
        IocContainerReleaseAnalyzer releaseAnalyzer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        protected IocContainerAnalyzerBase(Object container)
        {
            this.iocContainer = container;
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public IocContainerResolveByIdAnalyzer ResolveById
        {
            get
            {
                if (this.resolveByIdMethod == null && this.resolveByIdAnalyzer == null)
                {
                    this.resolveByIdMethod = this.GetResolveByIdMethod(this.iocContainer.GetType());
                    this.resolveByIdAnalyzer = Delegate.CreateDelegate(typeof(IocContainerResolveByIdAnalyzer), this.iocContainer, this.resolveByIdMethod) as IocContainerResolveByIdAnalyzer;
                }
                return this.resolveByIdAnalyzer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns></returns>
        protected abstract MethodInfo GetResolveByTypeMethod(Type containerType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns></returns>
        protected abstract MethodInfo GetResolveByIdMethod(Type containerType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns></returns>
        protected abstract MethodInfo GetReleaseMethod(Type containerType);
    }
}
