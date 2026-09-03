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
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    public class AveSPResource
    {
        private static List<int> lcidList = new List<int>() { 1033, 1041, 1031, 2052 }; ////SAAS-12228 使用resource文件支持多语言 目前支持的语言为英语，日语，德语，汉语

        static AveSPResource()
        {

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Resource file name for English, Japanese, German")]   //添加支持中文,在Wrapper.Resource.ResourceFile中添加Resource文件

        public static List<string> GetStrings(string name)
        {
            List<string> values = new List<string>();
            foreach (int lcid in lcidList)
            {
                values.Add(AveSPResourceFile.ResourceManager.GetString(name, new CultureInfo(lcid)));
            }
            return values;
        }

        public static string GetString(string name, params object[] values)
        {
            return GetString(1033, name, values);
        }

        public static string GetString(int lcid, string name, params object[] values)
        {
            string str = null;
            str = AveSPResourceFile.ResourceManager.GetString(name, new CultureInfo(lcid));
            if (!string.IsNullOrEmpty(str))
            {
                if (values != null && values.Length > 0)
                {
                    str = string.Format(str, values);
                }
            }

            return str;
        }
    }
}
