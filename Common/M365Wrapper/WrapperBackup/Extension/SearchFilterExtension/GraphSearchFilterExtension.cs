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
using ExchangeUtility.Graph.SearchFilterExtension;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Text;

namespace ExchangeUtility.Graph
{
    public static class GraphSearchFilterExtension
    {
        public static string ToGraphFilter(this SearchFilter filter, string itemType = "")
        {
            if (filter == null)
                return string.Empty;

            var fb = new StringBuilder();
            var converter = CreateFilterConverter(itemType);
            ConvertToFilter(filter, fb, converter);
            return fb.ToString();
        }

        public static string ToGraphSearch(this SearchFilter filter, string itemType = "")
        {
            if (filter == null)
                return string.Empty;

            var sb = new StringBuilder();
            var converter = CreateSearchConverter(itemType);
            ConvertToSearch(filter, sb, converter);
            return sb.ToString();
        }

        public static (string Filter, string Search) ToGraphQuery(this SearchFilter filter, string itemType = "")
        {
            if (filter == null)
                return (string.Empty, string.Empty);

            var fb = new StringBuilder();
            var sb = new StringBuilder();
            var filterConverter = CreateFilterConverter(itemType);
            var searchConverter = CreateSearchConverter(itemType);

            ConvertToFilter(filter, fb, filterConverter);
            ConvertToSearch(filter, sb, searchConverter);

            return (fb.ToString(), sb.ToString());
        }

        #region Filter conversion

        private static void ConvertToFilter(SearchFilter filter, StringBuilder fb, IFilterConverter converter)
        {
            switch (filter)
            {
                case SearchFilter.IsEqualTo eq when IsExtended(eq.PropertyDefinition):
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)eq.PropertyDefinition, "eq", eq.Value, fb);
                    break;
                case SearchFilter.IsNotEqualTo neq when IsExtended(neq.PropertyDefinition):
                    fb.Append("not (");
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)neq.PropertyDefinition, "eq", neq.Value, fb);
                    fb.Append(')');
                    break;
                case SearchFilter.ContainsSubstring contains when IsExtended(contains.PropertyDefinition):
                    converter.ConvertExtendedContains((ExtendedPropertyDefinition)contains.PropertyDefinition, contains.Value, fb);
                    break;
                case SearchFilter.Exists exists when IsExtended(exists.PropertyDefinition):
                    converter.ConvertExtendedExists((ExtendedPropertyDefinition)exists.PropertyDefinition, fb);
                    break;
                case SearchFilter.IsGreaterThan gt when IsExtended(gt.PropertyDefinition):
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)gt.PropertyDefinition, "gt", gt.Value, fb);
                    break;
                case SearchFilter.IsGreaterThanOrEqualTo gte when IsExtended(gte.PropertyDefinition):
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)gte.PropertyDefinition, "ge", gte.Value, fb);
                    break;
                case SearchFilter.IsLessThan lt when IsExtended(lt.PropertyDefinition):
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)lt.PropertyDefinition, "lt", lt.Value, fb);
                    break;
                case SearchFilter.IsLessThanOrEqualTo lte when IsExtended(lte.PropertyDefinition):
                    converter.ConvertExtendedProp((ExtendedPropertyDefinition)lte.PropertyDefinition, "le", lte.Value, fb);
                    break;
                case SearchFilter.IsEqualTo eq:
                    converter.ConvertEquality(eq.PropertyDefinition, eq.Value, fb);
                    break;
                case SearchFilter.IsNotEqualTo neq:
                    converter.ConvertEqualityNot(neq.PropertyDefinition, neq.Value, fb);
                    break;
                case SearchFilter.IsGreaterThan gt:
                    converter.ConvertComparison(gt.PropertyDefinition, "gt", gt.Value, fb);
                    break;
                case SearchFilter.IsGreaterThanOrEqualTo gte:
                    converter.ConvertComparison(gte.PropertyDefinition, "ge", gte.Value, fb);
                    break;
                case SearchFilter.IsLessThan lt:
                    converter.ConvertComparison(lt.PropertyDefinition, "lt", lt.Value, fb);
                    break;
                case SearchFilter.IsLessThanOrEqualTo lte:
                    converter.ConvertComparison(lte.PropertyDefinition, "le", lte.Value, fb);
                    break;
                case SearchFilter.ContainsSubstring contains:
                    converter.ConvertContains(contains, fb);
                    break;
                case SearchFilter.Exists exists:
                    converter.ConvertExists(exists.PropertyDefinition, fb);
                    break;
                case SearchFilter.Not notFilter:
                    ConvertFilterNot(notFilter, fb, converter);
                    break;
                case SearchFilter.SearchFilterCollection col:
                    ConvertFilterCollection(col, fb, converter);
                    break;
                default:
                    throw new NotSupportedException(
                        $"SearchFilter type {filter.GetType().Name} is not supported for Graph $filter conversion.");
            }
        }

        private static void ConvertFilterNot(SearchFilter.Not notFilter, StringBuilder fb, IFilterConverter converter)
        {
            fb.Append("not (");
            ConvertToFilter(notFilter.SearchFilter, fb, converter);
            fb.Append(')');
        }

        private static void ConvertFilterCollection(SearchFilter.SearchFilterCollection col, StringBuilder fb, IFilterConverter converter)
        {
            if (col.Count == 0) return;

            string logic = col.LogicalOperator == LogicalOperator.And ? " and " : " or ";
            var parts = new StringBuilder();

            for (int i = 0; i < col.Count; i++)
            {
                var subFb = new StringBuilder();
                ConvertToFilter(col[i], subFb, converter);

                if (subFb.Length > 0)
                {
                    if (parts.Length > 0) parts.Append(logic);
                    parts.Append(subFb);
                }
            }

            if (parts.Length > 0)
            {
                fb.Append('(').Append(parts).Append(')');
            }
        }

        #endregion

        #region Search conversion

        private static void ConvertToSearch(SearchFilter filter, StringBuilder sb, ISearchConverter converter)
        {
            switch (filter)
            {
                case SearchFilter.ContainsSubstring contains when !IsExtended(contains.PropertyDefinition):
                    converter.ConvertContains(contains, sb);
                    break;
                case SearchFilter.IsEqualTo eq when !IsExtended(eq.PropertyDefinition):
                    converter.ConvertEquality(eq.PropertyDefinition, eq.Value, sb);
                    break;
                case SearchFilter.Not notFilter:
                    ConvertToSearch(notFilter.SearchFilter, sb, converter);
                    break;
                case SearchFilter.SearchFilterCollection col:
                    ConvertSearchCollection(col, sb, converter);
                    break;
            }
        }

        private static void ConvertSearchCollection(SearchFilter.SearchFilterCollection col, StringBuilder sb, ISearchConverter converter)
        {
            for (int i = 0; i < col.Count; i++)
            {
                ConvertToSearch(col[i], sb, converter);
            }
        }

        #endregion

        #region Factory methods

        private static IFilterConverter CreateFilterConverter(string itemType)
        {
            var baseConverter = new MailboxItemFilterConverter();

            return itemType switch
            {
                ExchangeConstants.ItemType.Message => new MessageFilterConverter(baseConverter),
                _ => baseConverter
            };
        }

        private static ISearchConverter CreateSearchConverter(string itemType)
        {
            var nullConverter = new NullSearchConverter();

            return itemType switch
            {
                ExchangeConstants.ItemType.Message => new MessageSearchConverter(nullConverter),
                _ => nullConverter // Mailbox Item doesn't support $search
            };
        }

        private static bool IsExtended(PropertyDefinitionBase prop) => prop is ExtendedPropertyDefinition;

        #endregion
    }
}