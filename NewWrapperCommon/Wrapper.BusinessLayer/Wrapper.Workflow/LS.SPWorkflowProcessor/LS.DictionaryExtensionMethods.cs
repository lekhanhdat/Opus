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
using AvePoint.Wrapper.Common;

namespace LS.SPWorkflowProcessor
{
    internal static class DictionaryExtensionMethods
    {
        

        internal static void AddEx(this Dictionary<Guid, SPWFAssociationUnit> source, Guid key, SPWFAssociationUnit value)
        {
            if (source != null)
            {
                //ADO-111477：对于UnitsOfRestored，需要控制数据量不超过30，否则可能会出现内存过大的问题。
                //if (source.Count >= 30)
                //    source.Clear();
                source[key] = value;
            }
        }

        internal static void AddEx(this Dictionary<Guid, byte[]> source, Guid key, byte[] value)
        {
            if (source != null)
            {
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
            }
        }

        internal static void AddEx(this Dictionary<Guid, List<SPWFProcessorException>> source, Guid key, List<SPWFProcessorException> value)
        {
            if (source != null)
            {
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
            }
        }

        internal static void AddEx(this Dictionary<Guid, SPWorkflowSubListUnit> source, Guid key, SPWorkflowSubListUnit value)
        {
            if (source != null)
            {
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
            }
        }

        internal static void AddEx(this Dictionary<string, object> source, string key, object value)
        {
            if (source != null)
            {
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
            }
        }

        internal static void AddRange<T>(this Dictionary<string, object> source,IEnumerable<KeyValuePair<string,T>> values)
        {
            foreach (var value in values)
            {
                source.AddEx(value.Key,value.Value);
            }
        }

        internal static void AddEx(this Dictionary<Guid, Guid> source, Guid key, Guid value)
        {
            if (source.ContainsKey(key))
                source[key] = value;
            else
                source.Add(key, value);
        }

        internal static void AddEx(this ThreadSafeDictionary<Guid, Guid> source, Guid key, Guid value)
        {
            if (source.ContainsKey(key))
                source[key] = value;
            else
                source.Add(key, value);
        }

        internal static Guid GetKey(this Dictionary<Guid, Guid> source, int index)
        {

            if (source == null || source.Count == 0)
                return Guid.Empty;

            int i = 0;
            foreach (Guid val in source.Keys)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }

        internal static Guid GetValue(this Dictionary<Guid, Guid> source, int index)
        {
            if (source == null || source.Count == 0)
                return Guid.Empty;
            int i = 0;
            foreach (Guid val in source.Values)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }


        internal static void AddEx(this Dictionary<int, int> source, int key, int value)
        {
            if (source.ContainsKey(key))
                source[key] = value;
            else
                source.Add(key, value);
        }

        internal static int GetKey(this Dictionary<int, int> source, int index)
        {
            if (source == null || source.Count == 0)
                return 0;
            int i = 0;
            foreach (int val in source.Keys)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }

        internal static int GetValue(this Dictionary<int, int> source, int index)
        {
            if (source == null || source.Count == 0)
                return 0;
            int i = 0;
            foreach (int val in source.Values)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }


        internal static void AddEx(this Dictionary<string, string> source, string key, string value)
        {
            if (source.ContainsKey(key))
                source[key] = value;
            else
                source.Add(key, value);
        }

        internal static string GetKey(this Dictionary<string, string> source, int index)
        {
            if (source == null || source.Count == 0)
                return string.Empty;
            int i = 0;
            foreach (string val in source.Keys)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }

        internal static string GetValue(this Dictionary<string, string> source, int index)
        {
            if (source == null || source.Count == 0)
                return string.Empty;
            int i = 0;
            foreach (string val in source.Values)
            {
                if (i == index)
                    return val;
            }
            throw new IndexOutOfRangeException();
        }


        internal static void Dispose(this Dictionary<Guid, SPFieldProcessor> processors)
        {
            foreach (KeyValuePair<Guid, SPFieldProcessor> pair in processors)
                pair.Value.Dispose();
        }
    }
}
