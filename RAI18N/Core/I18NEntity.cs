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
using System.Globalization;
using System.Text.RegularExpressions;
using AvePoint.RA.I18N.Core.DaoMigration;

namespace AvePoint.RA.I18N.Core
{
    public static class I18NEntity
    {

        public static readonly string Separator = "|I18NSplit|";
        public static readonly string MultiI18nSeparator = "|MultiI18NSplit|";
        public static string GetString(ModuleName moduleName, string key)
        {
            return I18NResource.GetString(moduleName, key);
        }

        public static string GetString(ModuleName moduleName, string key, CultureInfo culture)
        {
            return I18NResource.GetString(moduleName, key, culture);
        }

        public static string GetString(ModuleName moduleName, string key, params object[] args)
        {
            return I18NResource.GetString(moduleName, key, args);
        }

        public static string GetString(ModuleName moduleName, string key, CultureInfo culture, params object[] args)
        {
            return I18NResource.GetString(moduleName, key, culture, args);
        }
        public static string GetString(string key)
        {
            return I18NResource.GetString(key);
        }
        public static string GetString(string key, CultureInfo culture)
        {
            return I18NResource.GetString(key, culture);
        }

        public static string GetString(string key, params object[] args)
        {
            return I18NResource.GetString(key, args);
        }

        public static bool HasKey(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            return GetMultiStringWithSeparator(key) != key;
        }

        public static string GetMultiStringWithSeparator(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            var strs = str.Split(new string[] { MultiI18nSeparator }, StringSplitOptions.None);
            if (strs.Length == 0)
            {
                return GetStringWithSeparator(str);
            }
            else
            {
                strs = strs.Select(item => GetStringWithSeparator(item)).Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
                return string.Join("; ", strs);
                
            }
        }

        public static string GetStringWithSeparator(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return "";
            }
            var strs = str.Split(new string[] { Separator }, StringSplitOptions.None);
            if (strs.Length == 0)
            {
                return I18NResource.GetString(str);
            }
            else
            {
                var resultStr = I18NResource.GetString(strs[0]);
                for (int i = 1; i < strs.Length; i++)
                {
                    var regex = new System.Text.RegularExpressions.Regex("\\{" + (i - 1) + "\\}");
                    resultStr = regex.Replace(resultStr, I18NResource.GetString(strs[i]));
                }
                return resultStr;
            }
        }

        public static string ReplaceI18NKey(string sourceStr, string startStr, string[] endStrs)
        {
            if (string.IsNullOrEmpty(sourceStr) || string.IsNullOrEmpty(startStr)) return sourceStr;
            try
            {
                for (int i = 0; i < endStrs.Length; i++)
                {

                    var endStr = endStrs[i];
                    var keyMatchRegex = new Regex($"{startStr}.*?{endStr}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matchs = keyMatchRegex.Matches(sourceStr);
                    foreach (Match match in matchs)
                        if (match.Success)
                        {
                            var isValidKey = true;
                            for (int j = i + 1; j < endStrs.Length; j++)
                            {
                                if (isValidKey && match.Value.Contains(endStrs[j]))
                                {
                                    isValidKey = false;
                                }
                            }
                            if (isValidKey)
                            {
                                sourceStr = sourceStr.Replace(match.Value, I18NResource.GetString(Regex.Replace(match.Value, $"{endStr}$", "")) + endStr);
                            }
                        }
                }
            }
            catch
            {
                return I18NEntity.GetString(sourceStr);
            }
            return I18NEntity.GetString(sourceStr);
        }

        public static string GetComment(string key, string defaultValue, params object[] args)
        {
            defaultValue = !string.IsNullOrEmpty(defaultValue) ? defaultValue : key;
            string i18N;
            try
            {
                i18N = DaoMigrationI18NEntity.Execution(key, args);
            }
            catch (Exception e)
            {
                return defaultValue;
            }
            return i18N.Replace("\\\"", "\"");
        }

        public static void Init()
        {
            I18NResource.Init();
        }
    }

}