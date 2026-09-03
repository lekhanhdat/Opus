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




namespace AvePoint.Media.Service.DomainModel.Event
{
    #region using directives
    using System;
    using System.Linq;
    using System.Reflection;
    #endregion

    internal abstract class EventIdsPolicyBase<TEventIdsType>
        : IEventIdsPolicy
        where TEventIdsType : IEventIdsType
    {
        Type eventIdsType = typeof(TEventIdsType);
        IEventIdsPolicy nextPolicy;
        public EventIdsPolicyBase(IEventIdsPolicy policy)
        {
            this.nextPolicy = policy;
        }

        public Boolean IsAllowPolicy()
        {
            var result = true;
            var minEventId = this.GetVersion6MinEventId();
            var maxEventId = this.GetVersion6MaxEventId();
            eventIdsType.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public).ForEach(field =>
            {
                if (field.FieldType == typeof(Int32))
                {
                    var fieldValue = Convert.ToInt32(field.GetValue(null));
                    if (fieldValue < minEventId || fieldValue > maxEventId)
                        result = false;
                }
            });
            if (!result && this.nextPolicy != null)
                result = this.nextPolicy.IsAllowPolicy();
            return result;
        }

        protected abstract Int32 GetVersion6MinEventId();
        protected abstract Int32 GetVersion6MaxEventId();

    }

    public static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
        {
            foreach (T item in source)
            {
                func(item);
            }
        }
    }
}