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

namespace AvePoint.GCommon.MicroKernel.DuckTyping
{
    #region using directives
    using System;
    using System.Runtime.Remoting;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    public class DuckingType : IDuckTyping
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="duck"></param>
        /// <returns></returns>
        public T Cast<T>(Object duck)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="duck"></param>
        /// <returns></returns>
        public object Cast(Type toType, Object duck)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="staticType"></param>
        /// <returns></returns>
        public T StaticCast<T>(Type staticType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="staticType"></param>
        /// <returns></returns>
        public object StaticCast(Type toType, Type staticType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duck"></param>
        /// <returns></returns>
        public object Uncast(object duck)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="duck"></param>
        /// <returns></returns>
        public bool CanCast<T>(object duck)
        {
            return !RemotingServices.IsTransparentProxy(duck);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="duck"></param>
        /// <returns></returns>
        public bool CanCast(Type toType, object duck)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TTo"></typeparam>
        /// <typeparam name="TFrom"></typeparam>
        /// <returns></returns>
        public bool CanCast<TTo, TFrom>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="duckType"></param>
        /// <returns></returns>
        public bool CanCast<T>(Type duckType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="fromType"></param>
        /// <returns></returns>
        public bool CanCast(Type toType, Type fromType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="staticType"></param>
        /// <returns></returns>
        public bool CanStaticCast<T>(Type staticType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="staticType"></param>
        /// <returns></returns>
        public bool CanStaticCast(Type toType, Type staticType)
        {
            throw new NotImplementedException();
        }
    }
}