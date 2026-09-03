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

namespace RASalesforce.APIs;

public static class SOQLQueryUtil
{
    private static Regex regExp = new Regex("['@=]+");
    public static (string Where, string Orderby) BuildQuerySOQLClause(DescribeSObjectResult recordType, Query query)
    {
        List<string> whereConditions = [];
        if (query.Filters is not null && query.Filters.Any())
        {
            whereConditions.AddRange(BuildFilterQuery(query.Filters, recordType));
        }
        if (query.SearchText.IsNotNullOrWhiteSpace() && query.SearchByColumns is not null && query.SearchByColumns.Any())
        {
            whereConditions.Add(BuildSearchQuery(query.SearchText!, query.SearchByColumns, recordType));
        }
        return (BuildWhereConditions(whereConditions), query.OrderBy is null ?
            "" : BuildOrderByClause(query.OrderBy, recordType));
    }

    private static string BuildSearchQuery(string searchText, List<string> searchByColumns, DescribeSObjectResult recordType)
    {
        List<string> searchConditions = [];
        searchText = searchText.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        searchByColumns.ForEach(c => 
        {
            var field = recordType.fields.FirstOrDefault(f=>f.name.EqualsIgnoreCase(c));
            if (field is not null) 
            {
                if (searchText.Contains(';'))
                {
                    searchText.Split(';', StringSplitOptions.RemoveEmptyEntries).ForEach(c => 
                    {
                        CheckSQLInjection(c);
                        searchConditions.Add($"( {field.name} LIKE '%{c}%' )");
                    });
                }
                else
                {
                    CheckSQLInjection(c);
                    searchConditions.Add($"( {field.name} LIKE '%{c}%' )");
                }
            }
        });
        return string.Join(" OR ", searchConditions);
    }
    private static IEnumerable<string> BuildFilterQuery(IEnumerable<QueryFilter> filters, DescribeSObjectResult recordType)
    {
        var result = new List<string>();
        foreach (var filter in filters)
        {
            var field = recordType.fields.FirstOrDefault(f => f.name.EqualsIgnoreCase(filter.PropertyName));
            if(field is not null)
            {
                switch (field.type) 
                {
                    case fieldType.datetime:
                        result.Add(GetDateTimeFilterValueEx(field.name, filter));
                        break;
                    case fieldType.@string:
                        result.Add(GetStringFilterValueEx(field.name, filter));
                        break;
                    case fieldType.boolean:
                        result.Add(GetBooleanFilterValueEx(field.name, filter));
                        break;
                    case fieldType.@int:
                        result.Add(GetIntFilterValueEx(field.name, filter));
                        break;
                    default:                        
                        break;
                }
            }
        }
        return result;
    }

    private static string BuildOrderByClause(QueryOrder orderBy, DescribeSObjectResult recordType)
    {
        if (orderBy == null || string.IsNullOrEmpty(orderBy.OrderByKeyword))
        {
            return string.Empty;
        }
        var field = recordType.fields.FirstOrDefault(f => f.name.EqualsIgnoreCase(orderBy.OrderByKeyword));
        if (field != null)
        {
            string order = orderBy.OrderByDesc ? "DESC" : "ASC";
            return $"ORDER BY {field.name} {order}";
        }
        return string.Empty;
    }
    private static string GetDateTimeFilterValueEx(string name, QueryFilter filter)
    {
        if (filter.Value is null || !filter.Value.Any() || filter.Value.Count() > 2 || filter.Value.Count() < 1)
        {
            return string.Empty;
        }
        var datetimeOffsetValues = filter.Value.ConvertAll(s => new DateTimeOffset(new DateTime(Convert.ToInt64(s), DateTimeKind.Utc)));
        if(datetimeOffsetValues.Count() == 1)
        {
            return filter.IsExclude switch
            {
                false => $"{name} >= {datetimeOffsetValues.First().ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz", DateTimeFormatInfo.InvariantInfo)}",
                true => $"{name} <= {datetimeOffsetValues.First().ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz", DateTimeFormatInfo.InvariantInfo)}"
            };
             
        }
        else
        {
            var minTimeValue = datetimeOffsetValues.Min();
            var maxTimeValue = datetimeOffsetValues.Max();
            return $"{name} >= {minTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz", DateTimeFormatInfo.InvariantInfo)} AND {name} <= {maxTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz", DateTimeFormatInfo.InvariantInfo)}";
        }       
    }

    private static string GetIntFilterValueEx(string name, QueryFilter filter)
    {
        if (filter.Value is null || !filter.Value.Any() || filter.Value.Count() > 2 || filter.Value.Count() < 1)
        {
            return string.Empty;
        }
        var values = filter.Value.ConvertAll(s => Convert.ToInt32(s));
        if (values.Count() == 1)
        {
            return $"{name} >= {values.First()}";
        }
        else
        {
            var minValue = values.Min();
            var maxValue = values.Max();
            return $"{name} >= {minValue} AND {name} < {maxValue}";
        }
    }

    private static string GetStringFilterValueEx(string name, QueryFilter filter)
    {
        if (!filter.Value.Any() || filter.Value.Count() != 1)
        {
            return string.Empty;
        }
        var stringValue = filter.Value.First();
        CheckSQLInjection(stringValue);
        return $"{name} = '{stringValue}'";
    }

    private static string GetBooleanFilterValueEx(string name, QueryFilter filter)
    {
        if (!filter.Value.Any() || filter.Value.Count() != 1)
        {
            return string.Empty;
        }
        List<string> booleanCondition = ["1", "true"];
        if (booleanCondition.Contains(filter.Value.First().ToLowerInvariant()))
        {
            return $"{name} = true";
        }
        return $"{name} = false";
    }

    private static string BuildWhereConditions(IEnumerable<string> whereConditions)
    {
        if (whereConditions is not null &&
            whereConditions.Any(a => !string.IsNullOrWhiteSpace(a)))
        {
            whereConditions = whereConditions.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            var andConditionsStr = string.Join(" AND ", whereConditions);
            return $"WHERE {andConditionsStr}";
        }
        return string.Empty;
    }

    private static void CheckSQLInjection(string str)
    {
        if (!string.IsNullOrEmpty(str) && regExp.IsMatch(str))
        {
            throw new ArgumentNullException("SOQL Injection risky");
        }
    }
}
