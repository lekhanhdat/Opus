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
using AvePoint.RA.Common.Audit.Async;
using AvePoint.RA.Common.Audit.JPMC;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Audit.JPMC;
using Castle.Core;
using Castle.MicroKernel.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Common.Audit
{
    public class AuditInterceptorSelector : IModelInterceptorsSelector
    {
        public bool HasInterceptors(ComponentModel model)
        {
            object[] attributes = model.Implementation.GetCustomAttributes(typeof(AuditAttribute), true);
            var asyncAttributes = model.Implementation.GetCustomAttributes(typeof(AsyncAuditAttribute), true);
            return attributes.Length > 0 || asyncAttributes.Any() || HasMethodLevelFSAudit(model.Implementation);
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            var type = model.Implementation;
            var result = new List<InterceptorReference>();

            if (type.GetCustomAttributes(typeof(AuditAttribute), true).Any())
            {
                result.Add(InterceptorReference.ForType<AuditInterceptAdapter>());
            }
            else if (type.GetCustomAttributes(typeof(AsyncAuditAttribute), true).Any())
            {
                result.Add(InterceptorReference.ForType<AuditAsyncInterceptAdapter>());
            }

            if (HasMethodLevelFSAudit(type))
            {
                result.Add(InterceptorReference.ForType<FSAuditInterceptAdapter>());
            }

            return result.ToArray();
        }

        private static bool HasMethodLevelFSAudit(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(m => m.GetCustomAttributes(typeof(FSAuditAttribute), true).Length > 0);
        }
    }
}
