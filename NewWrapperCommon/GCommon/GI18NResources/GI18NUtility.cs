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
using System.Globalization;
using System.Resources;

namespace AvePoint.Adonis.Common.I18N
{
    public class I18NUtility
    {
        /// <summary>
        ///   返回资源文件缓存
        /// </summary>
        protected static ResourceManager ResourceMgr = null;

        /// <summary>
        /// 当前语言
        /// </summary>
        private static CultureInfo curCulture = CultureInfo.CurrentCulture;


        public static string BaseGet(string key)
        {
            return GetMessageValue(key, curCulture);
        }


        public static string BaseGet(string key, params object[] args)
        {
            string messageValue = GetMessageValue(key, curCulture);
            string finalKey = string.Format(messageValue, args);
            return finalKey;
        }

        //提供指定culture获取value的方法，该方法可能不会被使用(无参数)
        public static string BaseGetWithCulture(string key, CultureInfo culture)
        {
            return GetMessageValue(key, culture);
        }
        //提供指定culture获取value的方法，该方法可能不会被使用(有参数)
        public static string BaseGetWithCulture(string module, string key, CultureInfo culture, params object[] args)
        {
            string messageValue = GetMessageValue(key, culture);
            string finalKey = string.Format(messageValue, args);
            return finalKey;
        }

        private static string GetMessageValue(string key, CultureInfo culture)
        {

            string ret;
            try
            {
                ret = ResourceMgr.GetString(key, culture);
                if (ret == null)
                {
                    ret = key;
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }

            return ret;
        }
    }
}
