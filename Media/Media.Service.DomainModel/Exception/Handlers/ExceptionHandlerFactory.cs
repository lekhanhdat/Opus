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



namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    #endregion

    public class ExceptionHandlerFactory
        : IExceptionHandlerFactory
    {
        readonly static Object syncRoot = new Object();
        static Dictionary<Type, IExceptionHandler> cachedExceptionHandler
            = new Dictionary<Type, IExceptionHandler>();

        static ExceptionHandlerFactory()
        {
            var handlers = from type in typeof(IExceptionHandler).Assembly.GetExportedTypes()
                           let typeName = type.FullName
                           where typeof(IExceptionHandler).IsAssignableFrom(type)
                            && type.IsClass && !type.IsAbstract
                           orderby typeName ascending
                           let handler = type
                           select new KeyValuePair<Type, IExceptionHandler>(
                               handler.GetAttribute<ExceptionHandlerAttribute>().ExceptionType,
                               Activator.CreateInstance(handler) as IExceptionHandler);

            cachedExceptionHandler.AddRangeInternal(handlers, true);
        }

        public IExceptionHandler GetHandler(Type exceptionType)
        {
            var result = default(IExceptionHandler);
            lock (syncRoot)
            {
                if (cachedExceptionHandler.ContainsKey(exceptionType))
                    result = cachedExceptionHandler[exceptionType];
                else result = cachedExceptionHandler[typeof(Exception)];
            }
            return result;
        }

        public void ReleaseHandler(Type exceptionType)
        {
            lock (syncRoot)
            {
                if (cachedExceptionHandler.ContainsKey(exceptionType))
                {
                    cachedExceptionHandler.Remove(exceptionType);
                }
            }
        }
    }
}