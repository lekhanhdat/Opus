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
using System.Net;
using System.Text.RegularExpressions;

namespace AvePoint.Common.AUI.Controls
{
    /// <summary>
    /// 包含一些<see cref="AUIValidationBase"/>需要用到的工具
    /// </summary>
    public class AveInputValidationHelper
    {
        ///// <summary>
        ///// 判断当前的PlanName是否合法
        ///// </summary>
        ///// <param name="planName">要判断的计划名</param>
        ///// <param name="validation">出现错误时需要添加错误的<see cref="AUIValidationBase"/></param>
        ///// <returns>true 代表计划名合法</returns>
        //public static bool ValidatePlanName(string planName, AUIValidationBase validation)
        //{
        //    if (planName == null || planName.Equals(string.Empty))
        //    {
        //        if(null != validation)
        //        {
        //            validation.AddError("PlanNameValidator", "Plan name can not be empty!");
        //        }
        //        return false;
        //    }
        //    else if (null != validation)
        //    {
        //        validation.RemoveError("PlanNameValidator", "Plan name can not be empty!");
        //    }
        //    if (planName.Length < 5)
        //    {
        //        if(null != validation)
        //        {
        //            validation.AddError("PlanNameValidator", "Plan name can not be less than 5 characters!");
        //        }
        //        return false;
        //    }
        //    else if (null != validation)
        //    {
        //        validation.RemoveError("PlanNameValidator", "Plan name can not be less than 5 characters!");
        //    }
        //    return true;
        //}

        public static bool IsNullOrEmptyString(string s)
        {
            return !(s != null && !s.Equals(string.Empty));
        }

        public static bool IsInt(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            int i;
            bool b = int.TryParse(s, out i);
            return b;
        }

        public static bool IsDouble(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            double d;
            bool b = double.TryParse(s, out d);
            return b;
        }

        public static bool IsDateTime(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            DateTime dt;
            bool b = DateTime.TryParse(s, out dt);
            return b;
        }

        public static int? TryGetIntValue(string s)
        {
            return IsInt(s) ? int.Parse(s) as int? : null;
        }

        public static double? TryGetDoubleValue(string s)
        {
            return IsDouble(s) ? double.Parse(s) as double? : null;
        }

        public static DateTime? TryGetDateTimeValue(string s)
        {
            return IsDateTime(s) ? DateTime.Parse(s) as DateTime? : null;
        }

        public static bool HasWhiteSpace(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            return s.Contains(" ");
        }

        public static bool HasOnlyDigits(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            string pattern = @"[^\p{N}]";
            return !Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsEmail(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            return !(!Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase));
        }

        public static bool IsUrl(string s)
        {
            if (IsNullOrEmptyString(s))
            {
                return false;
            }
            return Regex.IsMatch(s, @"[a-zA-z]+://[^\s]*");
        }

        public static bool IsURLName(string s)
        {
            List<char> specialChars = new List<char>() { '\"', '#', '%', '&', '*', ':', '<', '>', '?', '\\', '/', '{', '}', '~', '|' };
            foreach (char c in s)
            {
                if (specialChars.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断字符串是否为合法的PlanName
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsPlanName(string s)
        {
            if (s == null || s.Length == 0)
            {
                return false;
            }
            List<char> specialChars = new List<char>() { '/', '*', '?', '<', '>', '\"', '|' };
            foreach (char c in s)
            {
                if (specialChars.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 验证字符串是否是IPV4地址
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIPV4(string s)
        {
            return IsIPAddress(s) && s.Split('.').Length == 4;
        }

        /// <summary>
        /// 判断字符串是否是IPV6地址
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIPV6(string s)
        {
            return IsIPAddress(s) && (!IsIPV4(s));
        }

        /// <summary>
        /// 是否IPAddress类型（IPv4 的情况下使用以点分隔的四部分表示法格式表示，IPv6 的情况下使用冒号与十六进制格式表示）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string s)
        {
            IPAddress addr;
            return System.Net.IPAddress.TryParse(s, out addr);

        }
    }
}
