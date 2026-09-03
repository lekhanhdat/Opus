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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;

namespace LS.SPWorkflowProcessor
{

    public static class ExtensionMethods
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal static List<Hashtable> Clone(this List<Hashtable> list)
        {
            return new List<Hashtable>(list);
        }

        internal static void CloneInto(this List<Hashtable> srcList, ref List<Hashtable> dstList)
        {
            if (dstList != null)
            {
                dstList.Clear();
                dstList = null;
            }
            dstList = srcList.Clone();

        }

        internal static string GetExtension(this IAveFile spFile)
        {
            int index = spFile.Name.LastIndexOf('.');
            if (index >= 0)
                return spFile.Name.Substring(index + 1);
            else
                return string.Empty;
        }

        internal static string ToUpperEx(this string source, int startIndex, int len)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(source.Substring(0,startIndex));
            if ((len + startIndex) > source.Length)
                len = source.Length - startIndex;
            builder.Append(source.Substring(startIndex, len).ToUpper());
            if((len+startIndex)<source.Length)
                builder.Append(source.Substring(startIndex + len));
            return builder.ToString();
        }

        internal static void AddEx(this Hashtable source, object key, object value)
        {
            if (source != null)
            {
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
            }
        }

        public static object GetEx(this Hashtable source, object key)
        {
            if (source == null || source.Count == 0 || !source.ContainsKey(key))
                return null;
            return source[key];
        }

        internal static IAveListItem GetItemByOriginalUniqueId(this IAveList list, Guid origUniqueId)
        {
            try
            {
                IAveQuery query = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateQuery();
                query.QueryString = "<Where><Eq><FieldRef Name=\"" + SPWorkflowCommon.OriginalUniqueIdFieldName + "\"></FieldRef><Value Type=\"Text\">" + origUniqueId.ToString("B") + "</Value></Eq></Where>";

                IAveListItemCollection items = list.GetItems(query);
                if (items.Count != 0)
                {
                    return items[0];
                }
                else
                {
                    if (!SPWorkflowProcessorRuntime.FindTaskItemByOriginalUniqueIdOnly)
                    {
                        IAveQuery query2 = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateQuery();
                        query2.QueryString = "<Where><Eq><FieldRef Name=\"GUID\"></FieldRef><Value Type=\"Guid\">" + origUniqueId.ToString("B") + "</Value></Eq></Where>";
                        IAveListItemCollection items2 = list.GetItems(query2);
                        if (items2.Count != 0)
                        {
                            return items2[0];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else 
                    {
                        return null;
                    }
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetItemByUniqueIdError, e.ToString());
                return null;
            }

        }

        internal static IAveListItemCollection GetItemBySpecifiedField(this IAveList list, string fieldName, Guid fieldValue)
        {
            try
            {
                IAveQuery query = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateQuery();
                query.QueryString = "<Where><Eq><FieldRef Name=\"" + fieldName + "\"></FieldRef><Value Type=\"Text\">" + fieldValue.ToString("B").Trim(new char[] { '{', '}' }) + "</Value></Eq></Where>";

                IAveListItemCollection items = list.GetItems(query);
                return items;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetItemByUniqueIdError, e.ToString());
                return null;
            }

        }

        internal static int SetBinaryBitValue(this int source, int position, bool value)
        {
            if (value)
                return source | position;
            else
            {
                int k = 0;
                while ((position & 1) == 0)
                {
                    position = position >> 1;
                    k++;
                }

                return source & (~(1 << k));
            }
        }
    }
}
