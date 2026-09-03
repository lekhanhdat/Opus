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



namespace AvePoint.GCommon.Utility.I18N
{
    #region using directives
    using System;
    using System.Globalization;
    using System.Resources;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
    #endregion

    public class I18NUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 用于缓存资源文件管理器
        /// </summary>
        private static Dictionary<string, AveResourceManager> ResourceMgrCache = new Dictionary<string, AveResourceManager>();

        public static I18NMode I18NMode = I18NMode.Default;

        private static ISystemOptionService systemOptionService;

        /// <summary>
        ///   返回资源文件缓存
        /// </summary>
        protected static AveResourceManager ResourceMgr;

        /// <summary>
        /// 当前语言
        /// </summary>
        public static string CurCulture
        {
            get
            {
                if (!IsUseBrowserCulture)
                {
                    return curCulture;
                }
                string culture = systemOptionService.GetUseBrowserCulture();
                if (string.IsNullOrEmpty(culture))
                {
                    return curCulture;
                }
                return culture;
            }
        }

        public static void LoadResourceManager(ISystemOptionService service, string module, Assembly assembly)
        {
            try
            {
                if (service != null)
                {
                    systemOptionService = service;
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Load Resource Manager Error : {0}", e.Message), e);
            }
        }
        /// <summary>
        /// set resource cache
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="resourceMgr"></param>
        public static void SetResourceCache(string culture, AveResourceManager resourceMgr)
        {
            if (ResourceMgrCache != null && !string.IsNullOrEmpty(culture) && resourceMgr != null)
            {
                ResourceMgrCache[culture] = resourceMgr;
            }
        }
        /// <summary>
        /// get resource
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static AveResourceManager GetResourceByCulture(string culture)
        {
            if (ResourceMgrCache.ContainsKey(culture))
            {
                return ResourceMgrCache[culture];
            }
            return null;
        }

        /// <summary>
        /// 当前语言
        /// </summary>
        public static string curCulture = CultureInfo.CurrentUICulture.ToString();

        /// <summary>
        /// 是否使用浏览器语言
        /// </summary>
        public static bool IsUseBrowserCulture = false;

        /// <summary>
        /// 通过Key(无参数)得到相应国际化文字。此方法会将key中的特殊字符以及参数转成"_".</summary>
        /// <param><c>module</c> 词条所在模块</param>
        /// <param><c>english</c> 英语词条</param>
        /// <returns>以module_english做为key查询value值，如果key在资源文件中不存在，则返回english.</returns>

        public static string BaseGet(string module, string english)
        {
            return GetMessageValue(module, english, CurCulture);
        }

        /// <summary>
        /// 通过Key(有参数)得到相应国际化文字，并强制使用某种特定语言(culture)。如果culture对应的国际化不存在，则返回默认语言(英文)
        /// </summary>
        /// <param name="module">词条所在模块</param>
        /// <param name="english">英语词条</param>
        /// <param name="args">英语词条中的参数</param>
        /// <returns>以module_english做为key查询value值，如果key在资源文件中不存在，则返回english.</returns>
        public static string BaseGet(string module, string english, params object[] args)
        {
            string messageValue = GetMessageValue(module, english, CurCulture);
            string finalKey = string.Format(messageValue, args);
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
            string messageValue = GetMessageValue(module, english, culture);
            string finalKey = string.Format(messageValue, args);
            return finalKey;
        }

        private static string GetMessageValue(string module, string english, string culture)
        {
            string ret = string.Empty;
            string keyFormat = string.Format("{0}_{1}", module, english);
            try
            {
                if (ResourceMgrCache.ContainsKey(module))
                {
                    ret = ResourceMgrCache[module].GetString(keyFormat, culture);
                }
                if (!string.IsNullOrEmpty(culture) && ResourceMgrCache.ContainsKey(culture))
                {
                    ret = ResourceMgrCache[culture].GetString(IsNewLogic(module) ? module : keyFormat, culture);
                }
                if (ResourceMgr != null && string.IsNullOrEmpty(ret))
                {
                    ret = ResourceMgr.GetString(keyFormat, culture);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            return string.IsNullOrEmpty(ret) ? english : ret;
        }
        static bool IsNewLogic(string strSrc)
        {
            bool result = false;
            try
            {
                if (String.IsNullOrEmpty(strSrc))
                {
                    return false;
                }
                int index = strSrc.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1;
                if (index > 0 && index < strSrc.Length)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            return result;
        }
    }
}