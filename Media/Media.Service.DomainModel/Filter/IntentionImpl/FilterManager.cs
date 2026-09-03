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


namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
    #endregion

    public class FilterManager
        : IFilterManager
    {
        const string SQL_FORMAT_LIKE = " {0} like @TEXT escape '/'";
        const string SQL_FORMAT_NOT_LIKE = " {0} not like @TEXT escape '/'";
        const string SQL_FORMAT_EQUAL = " {0} = @TEXT ";
        const string SQL_FORMAT_NOT_EQUAL = " {0} != @TEXT";

        public string GenerateFilterQuery(FilterInfo filterInfo)
        {
            StringBuilder queryBuilder = new StringBuilder();
            var columnName = string.Empty;
            switch (filterInfo.RuleType)
            {
                case FilterRuleType.Title:
                    columnName = "COL_NAME";
                    queryBuilder.Append(GenerateQueryForString(columnName, filterInfo));
                    break;
                case FilterRuleType.Attachment:
                    columnName = "COL_HAS_ATTACH";
                    queryBuilder.Append(GenerateQueryForString(columnName, filterInfo));
                    break;
                case FilterRuleType.SendDate:
                    columnName = "COL_SEND_DATE";
                    queryBuilder.Append(GenerateQueryForDateTime(columnName, filterInfo));
                    break;
                case FilterRuleType.Category:
                    columnName = "COL_CATEGORY";
                    queryBuilder.Append(GenerateQueryForString(columnName, filterInfo));
                    break;
                case FilterRuleType.SendFrom:
                    columnName = "COL_SENDER";
                    queryBuilder.Append(GenerateQueryForString(columnName, filterInfo));
                    break;
                case FilterRuleType.SendTo:
                    columnName = "COL_DISPLAY_TO";
                    queryBuilder.Append(GenerateQueryForString(columnName, filterInfo));
                    break;
            }
            return queryBuilder.ToString();
        }

        private string GenerateQueryForDateTime(string columnName, FilterInfo filterInfo)
        {
            StringBuilder queryBuilder = new StringBuilder();
            switch (filterInfo.Condition)
            {
                case FilterCondition.Before:
                    queryBuilder.AppendFormat(" {0} < '{1}'", columnName, filterInfo.Criteria);
                    break;
                case FilterCondition.After:
                    queryBuilder.AppendFormat(" {0} > '{1}'", columnName, filterInfo.Criteria);
                    break;
                case FilterCondition.FromTo:
                    queryBuilder.AppendFormat(" ({0} >= '{1}' AND {0} <= '{2}')", columnName, filterInfo.Criteria1, filterInfo.Criteria2);
                    break;
            }

            return queryBuilder.ToString();
        }

        private string GenerateQueryForString(string columnName, FilterInfo filterInfo)
        {
            if (!IsExactlyMatch(filterInfo))
            {
                filterInfo.Criteria = SQLiteUtil.EscapeString(filterInfo.Criteria);
            }
            string queryFormat = GenerateQueryStringFormat(filterInfo);
            return string.Format(queryFormat, columnName);
            //return string.Format(queryFormat, columnName, criteria);
        }

        private bool IsExactlyMatch(FilterInfo filterInfo)
        {
            switch (filterInfo.Condition)
            {
                case FilterCondition.Exactly:
                case FilterCondition.DoesNotEquals:
                    return !IsExchangeItemTitleRule(filterInfo);
                case FilterCondition.Contains:
                case FilterCondition.DoesNotContains:
                case FilterCondition.Match:
                    return false;
                default:
                    return true;
            }
        }
        private static string GenerateQueryStringFormat(FilterInfo filterInfo)
        {
            const char PathParser = (char)0x12;
            switch (filterInfo.Condition)
            {
                case FilterCondition.Exactly:
                    if (IsExchangeItemTitleRule(filterInfo))
                    {
                        filterInfo.Criteria = string.Format("{0}%", filterInfo.Criteria + PathParser);
                        return SQL_FORMAT_LIKE;
                    }
                    return SQL_FORMAT_EQUAL;
                case FilterCondition.DoesNotEquals:
                    if (IsExchangeItemTitleRule(filterInfo))
                    {
                        filterInfo.Criteria = string.Format("{0}%", filterInfo.Criteria + PathParser);
                        return SQL_FORMAT_NOT_LIKE;
                    }
                    return SQL_FORMAT_NOT_EQUAL;
                case FilterCondition.Contains:
                    filterInfo.Criteria = string.Format("%{0}%", filterInfo.Criteria);
                    return SQL_FORMAT_LIKE;
                case FilterCondition.DoesNotContains:
                    filterInfo.Criteria = string.Format("%{0}%", filterInfo.Criteria);
                    return SQL_FORMAT_NOT_LIKE;
                case FilterCondition.Match:
                    filterInfo.Criteria = ReplaceWildcard(filterInfo.Criteria);
                    if (IsExchangeItemTitleRule(filterInfo))
                    {
                        filterInfo.Criteria = string.Format("{0}%", filterInfo.Criteria + PathParser);
                    }
                    return SQL_FORMAT_LIKE;
                default:
                    throw new ArgumentException(string.Format("Filter condition is not support, condition: {0}", filterInfo.Condition));
            }
        }
        private static string ReplaceWildcard(string input)
        {
            if (input.Contains("*") || input.Contains("?"))
            {
                return input.Replace('*', '%').Replace('?', '_');
            }
            return input;
        }
        private static bool IsExchangeItemTitleRule(FilterInfo filterInfo)
        {
            return filterInfo.Level == FilterLevel.ExchangeOnlineItem && filterInfo.RuleType == FilterRuleType.Title;
        }




        class SQLiteUtil
        {
            static Dictionary<char, string> map = new Dictionary<char, string>()
            {
                {'/', @"//"},  
                {'\'', @"/'"},  
                {'[', @"/["},  
                {']', @"/]"},  
                {'%', @"/%"},  
                {'&',@"/&"},  
                {'_', @"/_"},  
                {'(', @"/("},  
                {')', @"/)"},  
            };
            public static string EscapeString(string value)
            {
                var builder = new StringBuilder(value.Length);
                foreach (var c in value)
                {
                    string encodeString;
                    if (map.TryGetValue(c, out encodeString))
                    {
                        builder.Append(encodeString);
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
                return builder.ToString();
            }
        }
    }
}
