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
using Microsoft.Exchange.WebServices.Data;
using System.Text;

namespace ExchangeUtility.Graph.SearchFilterExtension
{
    internal class MessageSearchConverter : SearchConverterDecorator
    {
        public MessageSearchConverter(ISearchConverter inner) : base(inner) { }

        public override void ConvertContains(SearchFilter.ContainsSubstring contains, StringBuilder builder)
        {
            if (builder == null) return;

            if (TryMapToKqlProperty(contains.PropertyDefinition, out string kqlProp))
            {
                AppendKqlClause(builder, kqlProp, EscapeKqlValue(contains.Value));
            }
            else
            {
                _inner.ConvertContains(contains, builder);
            }
        }

        public override void ConvertEquality(PropertyDefinitionBase prop, object value, StringBuilder builder)
        {
            if (builder == null || value == null) return;

            if (TryMapToKqlProperty(prop, out string kqlProp))
            {
                AppendKqlClause(builder, kqlProp, EscapeKqlValue(value.ToString()));
            }
            else
            {
                _inner.ConvertEquality(prop, value, builder);
            }
        }

        private static void AppendKqlClause(StringBuilder builder, string property, string value)
        {
            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append($"\"{property}:{value}\"");
        }

        private static bool TryMapToKqlProperty(PropertyDefinitionBase prop, out string kqlProp)
        {
            kqlProp = null;
            if (prop is not PropertyDefinition p) return false;

            // KQL searchable properties for messages
            if (p == ItemSchema.Subject) { kqlProp = "subject"; return true; }
            if (p == ItemSchema.Body) { kqlProp = "body"; return true; }
            if (p == ItemSchema.HasAttachments) { kqlProp = "hasAttachments"; return true; }
            if (p == ItemSchema.Importance) { kqlProp = "importance"; return true; }
            if (p == EmailMessageSchema.From) { kqlProp = "from"; return true; }
            if (p == EmailMessageSchema.Sender) { kqlProp = "from"; return true; }
            if (p == EmailMessageSchema.ToRecipients) { kqlProp = "to"; return true; }
            if (p == EmailMessageSchema.CcRecipients) { kqlProp = "cc"; return true; }
            if (p == EmailMessageSchema.BccRecipients) { kqlProp = "bcc"; return true; }

            return false;
        }

        private static string EscapeKqlValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape double quotes and backslashes for KQL
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}