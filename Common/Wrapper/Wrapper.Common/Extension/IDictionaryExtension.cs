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

namespace AvePoint.Wrapper.Common
{
    public static class IDictionaryExtension
    {
        public static void AddChildren(this IDictionary<string,object> current,List<IDictionary<string,object>> children)
        {
            current.AddChildren<string, object>(children);
        }

        public static void AddRange(this IDictionary<string, object> current, IDictionary<string, object> props)
        {
            foreach (var kv in props)
            {
                current[kv.Key] = kv.Value;
            }
        }

        public static Dictionary<string, object> ToDictionary(this IDictionary<string, object> current)
        {
            if (current is Dictionary<string, object>)
            {
                return current as Dictionary<string, object>;
            }
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.AddRange(current);
            return dic;
        }

        public static void AddChildren<TKey,TValue>(this IDictionary<string, object> current, List<IDictionary<TKey, TValue>> children)
        {
            current.Add(AveObjectModelConstant.ChildrenProperties, children);
        }

        public static List<IDictionary<string, object>> GetChildren(this IDictionary<string, object> current)
        {
            object result;
            
            if (current.TryGetValue(AveObjectModelConstant.ChildrenProperties, out result))
            {
                //if (result is List<Dictionary<string, object>>)
                //{
                //    var resultNew = new List<IDictionary<string, object>>();
                //    resultNew.AddRange(result as List<Dictionary<string, object>>);
                //    return resultNew;
                //}
                return (List<IDictionary<string, object>>)result;
            }
            return default(List<IDictionary<string, object>>);
        }
    }
}
