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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    #endregion
   
    public static class CollectionExpand
    {
        static List<Type> cachedType = new List<Type>() { typeof(IList), typeof(IDictionary) };

        public static Boolean IsExpandableType(Type testType)
        {
            return cachedType.Exists(type => type.IsAssignableFrom(testType));
        }

        public static String Expand(Object obj)
        {
            var result = default(String);
            if (obj != null)
            {
                if (IsExpandableType(obj.GetType()))
                {
                    if (obj is IList)
                    { result = ExpandListCollection(obj as IList); }
                    else if (obj is IDictionary)
                    { result = ExpandDictionary(obj as IDictionary); }
                }
                else result = obj.ToString();
            }

            return result;
        }

        static String ExpandDictionary(IDictionary dictionary)
        {
            var result = new StringBuilder();
            foreach (var item in dictionary.Keys)
            {
                result.AppendFormat("key:[{0}],value:[{1}] {2} ", item, dictionary[item], Environment.NewLine);
            }
            return result.ToString();
        }

        static String ExpandListCollection(IList list)
        {
            var result = new StringBuilder();
            foreach (var item in list)
            {
                result.AppendFormat("item value is:[{0}] {1} ", item, Environment.NewLine);
            }
            return result.ToString();
        }
    }
}
