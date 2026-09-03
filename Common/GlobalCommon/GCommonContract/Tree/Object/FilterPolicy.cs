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
using System.Text.RegularExpressions;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public interface IFilterPolicy<T>
    {
        bool IsMatch(string standard, T value);
    }

    public class FilterPolicyManager
    {
        private FilterPolicyManager() { }

        public static IFilterPolicy<T> GetPolicy<T>()
        {
            if (typeof(T).Equals(typeof(int)))
            {
                return (IFilterPolicy<T>)IntegerFilterPolicy.SingleInstance;
            }
            if (typeof(T).Equals(typeof(string)))
            {
                return (IFilterPolicy<T>)StringFilterPolicy.SingleInstance;
            }
            if (typeof(T).Equals(typeof(bool)))
            {
                return (IFilterPolicy<T>)BooleanFilterPolicy.SingleInstance;
            }
            throw new NotImplementedException();
        }
    }

    class BooleanFilterPolicy : IFilterPolicy<bool>
    {
        private BooleanFilterPolicy() { }
        private static IFilterPolicy<bool> _instance;
        private static readonly object _lock = new object();
        public static IFilterPolicy<bool> SingleInstance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BooleanFilterPolicy();
                        }
                    }
                }
                return _instance;
            }
        }

        public bool IsMatch(string standard, bool value)
        {
            if (standard.Equals("*"))
            {
                return true;
            }
            bool flag = false;
            if (bool.TryParse(standard, out flag))
            {
                return flag.Equals(value);
            }
            else
            {
                return false;
            }
        }
    }

    class IntegerFilterPolicy : IFilterPolicy<int>
    {
        private IntegerFilterPolicy() { }
        private static IFilterPolicy<int> _instance;
        private static readonly object _lock = new object();
        public static IFilterPolicy<int> SingleInstance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new IntegerFilterPolicy();
                        }
                    }
                }
                return _instance;
            }
        }
        public bool IsMatch(string template, int value)
        {
            if (template.Equals("*"))
            {
                return true;
            }
            int valueParsed = 0;
            if (int.TryParse(template, out valueParsed))
            {
                return valueParsed == value;
            }
            else
            {
                return false;
            }
        }
    }

    class StringFilterPolicy : IFilterPolicy<string>
    {
        private StringFilterPolicy() { }
        private static IFilterPolicy<string> _instance;
        private static readonly object _lock = new object();
        public static IFilterPolicy<string> SingleInstance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new StringFilterPolicy();
                        }
                    }
                }
                return _instance;
            }
        }
        private Dictionary<string, string> _templateDic = new Dictionary<string, string>();

        public bool IsMatch(string template, string value)
        {
            string match = GetWildcardRegexString(template);
            return Regex.IsMatch(value, match);
        }

        private string GetWildcardRegexString(string wildcardStr)
        {
            if (!_templateDic.ContainsKey(wildcardStr))
            {
                lock (_lock)
                {
                    if (!_templateDic.ContainsKey(wildcardStr))
                    {
                        string wildcardRegexStr = "^" + Regex.Escape(wildcardStr).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                        _templateDic[wildcardStr] = wildcardRegexStr;
                    }
                }
            }
            return _templateDic[wildcardStr];
        }
    }
}
