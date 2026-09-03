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
using System.Web.Script.Serialization;
using AvePoint.GCommon;
using System.Collections;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class JsonConvertor
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(JsonConvertor));

        //convert from json

        public static object GetValuesFromJson(string jsonString, params string[] keys)
        {
            try
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                Dictionary<string, object> jsonData = (Dictionary<string, object>)js.DeserializeObject(jsonString);
                if (keys == null || keys.Length == 0)
                {
                    return jsonData;
                }
                else
                {
                    return GetValuesFromJson(jsonData, keys);
                }
            }
            catch (Exception)
            {
                logger.Error("json string is " + jsonString);
                throw;
            }
        }
        public static object GetValuesFromJson(Dictionary<string, object> jsonData, params string[] keys)
        {
            List<string> resultList = new List<string>();
            if (keys == null || keys.Length == 0)
            {
                return jsonData;
            }
            Dictionary<string, object> currentDictionary = jsonData;
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                object tempObject = currentDictionary[key];

                if (tempObject is Dictionary<string, object>)
                {
                    currentDictionary = tempObject as Dictionary<string, object>;
                }
                else if (tempObject is Object[])
                {
                    if (i == keys.Length - 1)
                    {
                        return tempObject;
                    }
                    else
                    {
                        Object[] arrayObject = tempObject as Object[];
                        string[] subKeys = new string[keys.Length - 1 - i];
                        Array.Copy(keys, i + 1, subKeys, 0, keys.Length - 1 - i);
                        string[] subResult = GetJsonStringValuesArray(arrayObject, subKeys);
                        AddNotNullVaulesToList(resultList, subResult);
                        return resultList.ToArray();
                    }

                }
                else if (tempObject is string)
                {
                    resultList.Add(tempObject as string);
                    return resultList.ToArray();
                }
                else
                {
                    logger.Error("invalid call " + key + " " + tempObject.GetType());
                    return null;
                }
            }

            return null;

        }

        public static string[] GetJsonStringValuesArray(Object[] arrayObject, params string[] keys)
        {
            List<string> resultList = new List<string>();
            for (int j = 0; j < arrayObject.Length; j++)
            {
                Object tempObject = arrayObject[j];
                if (tempObject is Dictionary<string, object>)
                {
                    Dictionary<string, object> subDictionary = arrayObject[j] as Dictionary<string, object>;
                    AddNotNullVaulesToList(resultList, GetValuesFromJson(subDictionary, keys) as string[]);
                }
                else
                {
                    Object[] subArrayObject = tempObject as Object[];
                    AddNotNullVaulesToList(resultList, GetJsonStringValuesArray(subArrayObject, keys));
                }

            }
            return resultList.ToArray();
        }

        private static void AddNotNullVaulesToList(List<string> list, string[] strs)
        {
            foreach (string str in strs)
            {
                if (str != null && str.Length > 0)
                {
                    list.Add(str);
                }
            }
        }



        //convert to json
        public static string GenJsonString(List<string> strList)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();
            foreach (string str in strList)
            {
                string[] strs = str.Split(new string[] { "=" }, StringSplitOptions.None);
                string keyStr = strs[0];
                string value = strs[1];
                string[] keys = keyStr.Split(new string[] { "." }, StringSplitOptions.None);

                Dictionary<string, object> currentJsonData = jsonData;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i == keys.Length - 1)
                    {
                        currentJsonData.Add(keys[i], value);
                    }
                    else
                    {
                        if (currentJsonData.Keys.Contains(keys[i]))
                        {
                            currentJsonData = (Dictionary<string, object>)currentJsonData[keys[i]];
                        }
                        else
                        {
                            Dictionary<string, object> tempJsonData = new Dictionary<string, object>();
                            currentJsonData.Add(keys[i], tempJsonData);
                            currentJsonData = tempJsonData;
                        }
                    }

                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            string result = js.Serialize(jsonData);
            return result;
        }

        public static string GenJsonString(Hashtable hashtable)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();
            foreach (string keyStr in hashtable.Keys)
            {
                string value = hashtable[keyStr] as string;
                string[] keys = keyStr.Split(new string[] { "." }, StringSplitOptions.None);

                Dictionary<string, object> currentJsonData = jsonData;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i == keys.Length - 1)
                    {
                        currentJsonData.Add(keys[i], value);
                    }
                    else
                    {
                        if (currentJsonData.Keys.Contains(keys[i]))
                        {
                            currentJsonData = (Dictionary<string, object>)currentJsonData[keys[i]];
                        }
                        else
                        {
                            Dictionary<string, object> tempJsonData = new Dictionary<string, object>();
                            currentJsonData.Add(keys[i], tempJsonData);
                            currentJsonData = tempJsonData;
                        }
                    }

                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            string result = js.Serialize(jsonData);
            return result;
        }
    }
}
