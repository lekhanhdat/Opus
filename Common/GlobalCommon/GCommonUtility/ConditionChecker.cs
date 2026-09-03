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




namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// 用来做各种条件判断的类，比如字符串是否匹配。支持* ? 同时支持转义字符\
    /// </summary>
    public class ConditionChecker
    {
        public static void Test()
        {
        }

        #region  String Checker

        public static bool Match(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            if (ignoreCase)
            {
                value = value.ToLower();
                criteria = criteria.ToLower();
            }
            criteria = RegexUtility.ConvertWildcardPatternToRegex(criteria);
            return CheckStringAccordingRegex(value, criteria);
        }

        public static bool IsExactly(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            if (ignoreCase)
            {
                return string.Compare(value, criteria, StringComparison.OrdinalIgnoreCase) == 0;
            }
            else
            {
                return string.Compare(value, criteria, StringComparison.Ordinal) == 0;
            }
        }

        public static bool Contains(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            if (ignoreCase)
            {
                value = value.ToLower();
                criteria = criteria.ToLower();
            }
            return value.Contains(criteria);
        }

        public static bool StartWith(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            if (ignoreCase)
            {
                return value.StartsWith(criteria, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return value.StartsWith(criteria, StringComparison.Ordinal);
            }
        }

        public static bool EndWith(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            if (ignoreCase)
            {
                return value.EndsWith(criteria, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return value.EndsWith(criteria, StringComparison.Ordinal);
            }
        }

        public static bool IsEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool ListIn(string value, string criteria, bool ignoreCase = true)
        {
            if (null == value) { throw new ArgumentNullException(); }
            if (string.IsNullOrEmpty(criteria)) throw new ArgumentNullException();
            var stringList = criteria.Split(";", StringSplitOptions.RemoveEmptyEntries);
            if (ignoreCase)
            {
                return stringList.Any(i => i.Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return stringList.Any(i => i.Equals(value, StringComparison.Ordinal));
            }
        }

        #endregion

        #region Number Checker
        // Fields 浮点型的误差
        private const double DOUBLEDELTA = 1E-06;

        public static bool LessThan(string value, string criteria)
        {
            ParseLong(value, criteria);
            return long.Parse(value) < long.Parse(criteria);
        }

        public static bool LessThan(int value, int criteria)
        {
            return value < criteria;
        }

        public static bool LessOrEqualThan(string value, string criteria)
        {
            ParseLong(value, criteria);
            return long.Parse(value) <= long.Parse(criteria);
        }

        public static bool LessOrEqualThan(int value, int criteria)
        {
            return value <= criteria;
        }

        public static bool LessOrEqualThan(long value, long criteria)
        {
            return value <= criteria;
        }

        public static bool LessOrEqualThan(double value, double criteria)
        {
            return (value < criteria) || (Equal(value, criteria));
        }

        public static bool BiggerThan(string value, string criteria)
        {
            ParseLong(value, criteria);
            return long.Parse(value) > long.Parse(criteria);
        }

        public static bool BiggerThan(int value, int criteria)
        {
            return value > criteria;
        }

        public static bool BiggerOrEqualThan(string value, string criteria)
        {
            ParseLong(value, criteria);
            return long.Parse(value) >= long.Parse(criteria);
        }

        public static bool BiggerOrEqualThan(int value, int criteria)
        {
            return value >= criteria;
        }

        public static bool BiggerOrEqualThan(long value, long criteria)
        {
            return value >= criteria;
        }

        public static bool BiggerOrEqualThan(double value, double criteria)
        {
            return (value > criteria) || (Equal(value, criteria));
        }

        public static bool Equal(int value, int criteria)
        {
            return value == criteria;
        }

        public static bool Equal(long value, long criteria)
        {
            return value == criteria;
        }

        public static bool Equal(double value, double criteria)
        {
            return Math.Abs(value - criteria) < DOUBLEDELTA;
        }

        public static bool IsZero(double value)
        {
            return (Math.Abs(value) < DOUBLEDELTA);
        }
    #endregion

    #region DateTime Checker

    public static bool Between(DateTime value, DateTime start, DateTime end)
        {
            CheckDataTimeKind(value, start, end);
            return value >= start && value <= end;
        }

        public static bool Before(DateTime value, DateTime criteria)
        {
            CheckDataTimeKind(value, criteria);
            return value < criteria;
        }

        public static bool After(DateTime value, DateTime criteria)
        {
            CheckDataTimeKind(value, criteria);
            return value > criteria;
        }

        public static bool On(DateTime value, DateTime criteria, long precision = 60000)
        {
            CheckDataTimeKind(value, criteria);
            //if (value >= criteria.AddMilliseconds(0 - precision) && value <= criteria.AddMilliseconds(precision))
            //{
            //    return true;
            //}
            if (value >= criteria)
            {
                return value.Subtract(criteria).Days == 0;
            }          
            return false;
        }

        public static bool WithInDays(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value >= DateTime.UtcNow.AddDays(0 - criteria) && value <= DateTime.UtcNow;
        }

        public static bool WithInWeeks(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value >= DateTime.UtcNow.AddDays(0 - criteria * 7) && value <= DateTime.UtcNow;
        }

        public static bool WithInMonths(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value >= DateTime.UtcNow.AddMonths(0 - criteria) && value <= DateTime.UtcNow;
        }

        public static bool WithInYears(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value >= DateTime.UtcNow.AddYears(0 - criteria) && value <= DateTime.UtcNow;
        }

        public static bool OlderThanNow(DateTime value)
        {
            CheckDataTimeKind(value);
            return value < DateTime.UtcNow;
        }

        public static bool OlderThanDays(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria < 0) throw new ArgumentException("criteria must be greater than zero");
            return value < DateTime.UtcNow.AddDays(0 - criteria);
        }

        public static bool OlderThanWeeks(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value < DateTime.UtcNow.AddDays(0 - criteria * 7);
        }

        public static bool OlderThanMonths(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value < DateTime.UtcNow.AddMonths(0 - criteria);
        }

        public static bool OlderThanYears(DateTime value, int criteria)
        {
            CheckDataTimeKind(value);
            if (criteria <= 0) throw new ArgumentException("criteria must be greater than zero");
            return value < DateTime.UtcNow.AddYears(0 - criteria);
        }

        #endregion

        #region util

        private static bool CheckStringAccordingRegex(string value, string criteria)
        {
            Regex regexRule = new Regex(criteria);
            return regexRule.IsMatch(value);
        }

        private static void CheckDataTimeKind(params DateTime[] values)
        {
            foreach (DateTime value in values)
            {
                if (value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("only accept UTC date time kind.");
                }
            }
        }

        private static void ParseLong(params string[] values)
        {
            foreach (string value in values)
            {
                long convertedValue = 0;
                bool result = long.TryParse(value, out convertedValue);
                if (!result)
                {
                    throw new ArgumentException(value + " cannot convert to int.");
                }
            }
        }

        #endregion

        /// <summary>
        /// regular expression utility class
        /// </summary>
        private class RegexUtility
        {
            /// <summary>
            /// get regular expression string according to the wildcard pattern, support wildcard * and ?, using \ do escape
            /// </summary>
            /// <param name="wildcardPattern">wildcard pattern string</param>
            /// <param name="matchType">string match type</param>
            /// <returns></returns>
            public static string ConvertWildcardPatternToRegex(string wildcardPattern)
            {
                string regexExpression = string.Empty;
                string[] split = wildcardPattern.Split(new string[] { @"\*" }, StringSplitOptions.None);
                for (int i = 0; i < split.Length; i++)
                {
                    if (i == 0)
                    {
                        regexExpression = ConvertWildcardPatternToRegexWithEscape(split[i], @"\?");
                    }
                    else
                    {
                        regexExpression += @"\*" + ConvertWildcardPatternToRegexWithEscape(split[i], @"\?");
                    }
                }
                return "^" + regexExpression + "$";
            }

            private static string ConvertWildcardPatternToRegexWithEscape(string wildcardPattern, string escapeValue)
            {
                string[] splitby = wildcardPattern.Split(new string[] { escapeValue }, StringSplitOptions.None);
                string tempRetureValue = string.Empty;
                for (int i = 0; i < splitby.Length; i++)
                {
                    if (i == 0)
                    {
                        tempRetureValue = ConvertWildcardPatternToRegexWithoutEscape(splitby[i]);
                    }
                    else
                    {
                        tempRetureValue += escapeValue;
                        tempRetureValue += ConvertWildcardPatternToRegexWithoutEscape(splitby[i]);
                    }
                }
                return tempRetureValue;
            }

            private static string ConvertWildcardPatternToRegexWithoutEscape(string wildcardPattern)
            {
                Regex regex = new Regex("[.$^{\\[(|)*+?\\\\]");
                return regex.Replace(wildcardPattern,
                     delegate(Match m)
                     {
                         switch (m.Value)
                         {
                             case "?":
                                 return ".";
                             case "*":
                                 return ".*";
                             default:
                                 return "\\" + m.Value;
                         }
                     });
            }
        }
    }
}
