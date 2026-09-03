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
using System.Globalization;
using System.Resources;

namespace AutoInstallationCommon.Utility.I18N
{
    public class I18NUtility
    {
        /// <summary>
        ///     用于缓存资源文件管理器，用于国际化不在当前Assembly进行时，如Gui.Common
        /// </summary>
        protected static Dictionary<string, ResourceManager> ResourceMgrCache;

        /// <summary>
        ///     返回资源文件缓存
        /// </summary>
        protected static ResourceManager ResourceMgr;

        /// <summary>
        ///     当前语言
        /// </summary>
        public static readonly string curCulture = CultureInfo.CurrentUICulture.ToString();

        static I18NUtility()
        {
            if (ResourceMgrCache == null) ResourceMgrCache = new Dictionary<string, ResourceManager>();
        }

        /// <summary>
        ///     通过Key(无参数)得到相应国际化文字。此方法会将key中的特殊字符以及参数转成"_".
        /// </summary>
        /// <param><c>module</c> 词条所在模块</param>
        /// <param><c>english</c> 英语词条</param>
        /// <returns>以module_english做为key查询value值，如果key在资源文件中不存在，则返回english.</returns>
        public static string BaseGet(string module, string english)
        {
            return GetMessageValue(module, english, curCulture);
        }

        /// <summary>
        ///     通过Key(有参数)得到相应国际化文字，并强制使用某种特定语言(culture)。如果culture对应的国际化不存在，则返回默认语言(英文)
        /// </summary>
        /// <param name="module">词条所在模块</param>
        /// <param name="english">英语词条</param>
        /// <param name="args">英语词条中的参数</param>
        /// <returns>以module_english做为key查询value值，如果key在资源文件中不存在，则返回english.</returns>
        public static string BaseGet(string module, string english, params object[] args)
        {
            var messageValue = GetMessageValue(module, english, curCulture);
            var finalKey = string.Format(messageValue, args);
            return finalKey;
        }

        //提供指定culture获取value的方法，该方法可能不会被使用(无参数)
        public static string BaseGetWithCulture(string module, string english, string culture)
        {
            return GetMessageValue(module, english, culture);
        }

        //提供指定culture获取value的方法，该方法可能不会被使用(有参数)
        public static string BaseGetWithCulture(string module, string english, string culture, params object[] args)
        {
            var messageValue = GetMessageValue(module, english, culture);
            var finalKey = string.Format(messageValue, args);
            return finalKey;
        }

        private static string GetMessageValue(string module, string english, string culture)
        {
            /*
             * 当在Xaml中查看视图，及时编译时，防止外部变量影响编译
             * */
            var ret = string.Empty;
            var keyFormat = string.Format("{0}_{1}", module, english);

            try
            {
                if (ResourceMgrCache.ContainsKey(module)) ResourceMgr = ResourceMgrCache[module];
                ret = ResourceMgr.GetString(keyFormat, new CultureInfo(culture));
                if (ret == null) ret = english;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return ret;
        }
    }
}