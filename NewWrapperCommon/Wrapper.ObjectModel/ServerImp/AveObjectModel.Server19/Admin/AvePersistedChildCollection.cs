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



using System;
using AvePoint.Wrapper.Common;
using System.Collections;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server19
{
    abstract class AvePersistedChildCollection<T> : AvePersistedObjectCollection<T>, IAvePersistedChildCollection<T> where T : IAvePersistedObject
    {
        public AvePersistedChildCollection(IEnumerable persistedChildCollection)
            : base(persistedChildCollection)
        { }

        public AvePersistedChildCollection()
        { }

        public virtual void Remove(Guid id)
        {
            T local = base[id];
            if (local != null)
            {
                local.Delete();
            }
        }

        public virtual T Ensure(T newObj)
        {
            Type genericType = GetGenericType(typeof(T));
            return (T)CreateElementInstance(typeof(T), AveAssemblyUtility.InvokeMethod(mPersistedObjectCollection, "Ensure", new Type[] { genericType }, new object[] { (newObj as AvePersistedObject).PersistedObject }));
        }
    }
}
