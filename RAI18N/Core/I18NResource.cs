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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Text.RegularExpressions;
using AvePoint.RA.I18N.Dto;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.I18N.Core
{

    // ModuleName
    public enum ModuleName
    {
        Home,
        Login,
        ControlPlanel
    }
    
    internal static class I18NResource
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(I18NResource));
        private const string RESOURCE_PREFIX = "RM";
        private static CultureInfo resourceCulture;
        private static ResourceManager resourceManager;
        private static Dictionary<string, I18NMessageDto> I18NMessageDic = new Dictionary<string, I18NMessageDto>();
        static I18NResource()
        {
            // Register Resources
            resourceManager = AvePoint.RA.I18N.Resources.RecordAutomation.ResourceManager;
        }
        
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture == null ? CultureInfo.CurrentUICulture : resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string GetCultureInfoName()
        {
            return Culture.Name;
        }

        internal static string GetString(ModuleName moduleName, string key)
        {
            return GetString(moduleName, key, Culture);
        }

        internal static string GetString(ModuleName moduleName, string key, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }
            if (null == culture)
            {
                culture = Culture;
            }
            string rs = resourceManager.GetString(string.Format("{0}_{1}_{2}", RESOURCE_PREFIX, moduleName.ToString(), key), culture);
            if (string.IsNullOrEmpty(rs))
            {
                return key;
            }
            return rs;
        }

        internal static string GetString(ModuleName moduleName, string key, params object[] args)
        {
            string rs = GetString(moduleName, key);
            if (! string.IsNullOrEmpty(rs) && args.Length > 0)
            {
                rs = string.Format(rs, args);
            }
            return rs;
        }

        internal static string GetString(ModuleName projectName, string key, CultureInfo culture, params object[] args)
        {
            string rs = GetString(projectName, key, culture);
            if (! string.IsNullOrEmpty(rs) && args.Length > 0)
            {
                rs = string.Format(rs, args);
            }
            return rs;
        }
        internal static string GetString(string key)
        {
            //return GetString(key, new CultureInfo("en-US"));
            return GetString(key, Thread.CurrentThread.CurrentUICulture);
        }
        internal static string GetString(string key, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }
            string rs = resourceManager.GetString(key, culture);
            if (string.IsNullOrEmpty(rs))
            {
                return key;
            }
            return rs;
        }
        internal static string GetString(string key, params object[] args)
        {
            string rs = GetString(key);
            if (!string.IsNullOrEmpty(rs) && args.Length > 0)
            {
                rs = string.Format(rs, args);
            }
            return rs;
        }
        internal static void Init()
        {
            var set = resourceManager.GetResourceSet(new CultureInfo(1033), true, true);
            if (set != null)
            {
                var enumer = set.GetEnumerator();
                while (enumer.MoveNext())
                {
                    if (enumer.Key != null && enumer.Value != null)
                    {
                        try
                        {
                            I18NMessageDto messageDto = new I18NMessageDto() { Key = enumer.Key.ToString(), Value = enumer.Value.ToString() };

                            if (!I18NMessageDic.ContainsKey(messageDto.Key))
                            {
                                I18NMessageDic[messageDto.Key] = messageDto;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message, e);
                        }
                    }
                }
            }
        }
    }
}
