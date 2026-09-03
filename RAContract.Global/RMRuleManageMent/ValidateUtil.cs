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
using System.Text.RegularExpressions;

namespace AvePoint.RA.Contract.RMRuleManageMent
{
    public class ValidateUtil
    {
        public static void ValidatePlanName(string s)
        {
            if (s == null || s.Trim().Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                //throw new GeneralException(Messages.Get("miss_plan_name"));
                throw new Exception("");
            }
            if (s.Length > 255)
            {
                //throw new GeneralException(Messages.Get("plan_name_too_long"));
                throw new Exception("");
            }
            List<char> specialChars = new List<char>() { '/', '*', '?', '<', '>', '\"', '|' };
            foreach (char c in s)
            {
                if (specialChars.Contains(c))
                {
                    //throw new GeneralException(Messages.Get("plan_name_contain_special_char"));
                    throw new Exception("");
                }
            }
        }

        public static string ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Email");
            }
            string pattern = @"[\w|\W]+@[\w|\W]+\.[\w|\W]+";
            if (!(!Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase)))
            {
                return email;
            }
            else
            {
                //throw new GeneralException(Messages.Get("email_valid"));
                throw new Exception("");
            }
        }

        public static void ValidatePositiveInt(string s)
        {
            int i = 0;
            if (string.IsNullOrEmpty(s) || (!int.TryParse(s, out i)) || i < 0)
            {
                //throw new GeneralException(Messages.Get("invalid_integer"));
                throw new Exception("");
            }
        }

        /// <summary>
        /// 检查链表中是否有null元素.
        /// </summary>
        /// <typeparam name="T">T 泛型，代表链表元素.</typeparam>
        /// <param name="list">待检查的链表.</param>
        /// <returns>是否有空元素.</returns>
        public static bool IsListHasNullElement<T>(List<T> list) where T : class
        {
            if (list != null)
            {
                foreach (T item in list)
                {
                    if (item == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        public static bool IsListHasNullOrWhiteSpaceElement(List<string> list)
        {
            bool result = false;
            if (list != null && list.Count != 0)
            {
                list.ForEach(item => { if (IsStringNullOrWhiteSpace(item)) result = true; });
            }
            return result;
        }
        public static void ValidateArrayHasNullOrEmptyString(string[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentNullException();
            }
            foreach (string str in array)
            {
                if (string.IsNullOrEmpty(str))
                {
                    //throw new InvalidArgumentException("TODO: this array contains null or empty string.");
                    throw new Exception("");
                }
            }
        }
        /// <summary>
        /// 检查链表是否有重复元素.
        /// </summary>
        /// <param name="list">待检查的链表.</param>
        /// <returns>如果有，返回重复元素，如果没有，返回null.</returns>
        /// <remarks>请在检查完null元素之后，调用此方法进行重复元素检查.</remarks>
        public static string ValidateListHasSameElement(List<string> list)
        {
            if (list != null)
            {
                List<string> tempList = new List<string>();
                foreach (string item in list)
                {
                    if (tempList.Contains(item.ToLower(CultureInfo.InvariantCulture)))
                    {
                        return item;
                    }
                    else
                    {
                        tempList.Add(item.ToLower(CultureInfo.InvariantCulture));
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public static bool IsStringNullOrWhiteSpace(string s)
        {
            if (string.IsNullOrEmpty(s) || string.Empty.Equals(s.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
