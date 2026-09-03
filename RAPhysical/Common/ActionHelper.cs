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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Common
{
    public class ActionHelper
    {
        public static async Task ExecuteAsync<T>(List<Func<T,Task>> actions, T param) where T : class
        {
            if (actions == null || actions.Count == 0) return;
            if (param == null) return;

            foreach (var action in actions)
            {
                await action(param);
            }
        }

        public static async Task ExecuteAsync<T1,T2>(List<Func<T1,T2,Task>> actions, T1 param1, T2 param2) 
            where T1 : class
            where T2 : class
        {
            if (actions == null || actions.Count == 0) return;
            if (param1 == null || param2 == null) return;

            foreach (var action in actions)
            {
                await action(param1, param2);
            }
        }

        //public static void Execute<T>(List<Action<IEnumerable<T>>> actions, IEnumerable<T> items)
        //{
        //    if (actions == null || actions.Count == 0) return;
        //    if (items == null || items.Count() == 0) return;

        //    foreach (var action in actions)
        //    {
        //        action(items);
        //    }
        //}
    }
}
