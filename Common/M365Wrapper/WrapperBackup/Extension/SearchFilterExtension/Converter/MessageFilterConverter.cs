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
using Microsoft365.Graph.Extensions;
using System;
using System.Text;

namespace ExchangeUtility.Graph.SearchFilterExtension
{
    internal class MessageFilterConverter : FilterConverterDecorator
    {
        public MessageFilterConverter(IFilterConverter inner) : base(inner) { }

        public override void ConvertComparison(PropertyDefinitionBase prop, string op, object value, StringBuilder builder)
        {
            if (builder == null) return;

            if (TryMapToGraphField(prop, out string field))
            {
                AppendComparisonFilter(field, op, value, builder);
            }
            else
            {
                _inner.ConvertComparison(prop, op, value, builder);
            }
        }

        public override void ConvertContains(SearchFilter.ContainsSubstring contains, StringBuilder builder)
        {
            if (builder == null) return;

            if (!TryMapToGraphField(contains.PropertyDefinition, out string field))
            {
                _inner.ConvertContains(contains, builder);
                return;
            }

            string escapedValue = EscapeFilterValue(contains.Value);
            string function = contains.ContainmentMode == ContainmentMode.Prefixed ? "startsWith" : "contains";
            AppendStringFunction(function, field, escapedValue, builder);
        }

        public override void ConvertEquality(PropertyDefinitionBase prop, object value, StringBuilder builder)
        {
            if (builder == null) return;

            if (TryMapToGraphField(prop, out string field))
            {
                AppendComparisonFilter(field, "eq", value, builder);
            }
            else
            {
                _inner.ConvertEquality(prop, value, builder);
            }
        }

        public override void ConvertEqualityNot(PropertyDefinitionBase prop, object value, StringBuilder builder)
        {
            if (builder == null) return;

            builder.Append("not (");
            ConvertEquality(prop, value, builder);
            builder.Append(')');
        }

        public override void ConvertExists(PropertyDefinitionBase prop, StringBuilder builder)
        {
            if (builder == null) return;

            if (TryMapToGraphField(prop, out string field))
            {
                builder.Append($"{field} ne null");
            }
            else
            {
                _inner.ConvertExists(prop, builder);
            }
        }

        #region Private helpers

        private void AppendComparisonFilter(string field, string op, object value, StringBuilder builder)
        {
            if (IsCollectionField(field))
            {
                string basePath = GetCollectionBasePath(field);
                string itemPath = GetCollectionItemPath(field);
                builder.Append($"{basePath}/any(r: r/{itemPath} {op} {Format(value)})");
            }
            else
            {
                builder.Append($"{field} {op} {Format(value)}");
            }
        }

        private void AppendStringFunction(string function, string field, string escapedValue, StringBuilder builder)
        {
            if (IsCollectionField(field))
            {
                string basePath = GetCollectionBasePath(field);
                string itemPath = GetCollectionItemPath(field);
                builder.Append($"{basePath}/any(r: {function}(r/{itemPath}, '{escapedValue}'))");
            }
            else
            {
                builder.Append($"{function}({field}, '{escapedValue}')");
            }
        }

        private static bool TryMapToGraphField(PropertyDefinitionBase prop, out string field)
        {
            field = null;
            if (prop is not PropertyDefinition p) return false;

            field = MapPropertyToGraphField(p);
            return field != null;
        }

        private static string MapPropertyToGraphField(PropertyDefinition p)
        {
            if (p == ItemSchema.Id) return "id";
            if (p == ItemSchema.Subject) return "subject";
            if (p == ItemSchema.Body) return "body/content";
            if (p == ItemSchema.DateTimeReceived) return "receivedDateTime";
            if (p == ItemSchema.DateTimeSent) return "sentDateTime";
            if (p == ItemSchema.DateTimeCreated) return "createdDateTime";
            if (p == ItemSchema.LastModifiedTime) return "lastModifiedDateTime";
            if (p == ItemSchema.HasAttachments) return "hasAttachments";
            if (p == ItemSchema.Importance) return "importance";
            if (p == ItemSchema.Size) return "size";
            if (p == ItemSchema.Categories) return "categories";
            if (p == ItemSchema.ItemClass) return "itemClass";
            if (p == ItemSchema.ConversationId) return "conversationId";
            if (p == ItemSchema.ParentFolderId) return "parentFolderId";
            if (p == EmailMessageSchema.From) return "from/emailAddress/address";
            if (p == EmailMessageSchema.Sender) return "sender/emailAddress/address";
            if (p == EmailMessageSchema.IsRead) return "isRead";
            if (p == EmailMessageSchema.IsDraft) return "isDraft";
            if (p == EmailMessageSchema.ToRecipients) return "toRecipients/emailAddress/address";
            if (p == EmailMessageSchema.CcRecipients) return "ccRecipients/emailAddress/address";
            if (p == EmailMessageSchema.BccRecipients) return "bccRecipients/emailAddress/address";
            if (p == EmailMessageSchema.ReplyTo) return "replyTo/emailAddress/address";
            if (p == EmailMessageSchema.InternetMessageId) return "internetMessageId";
            if (p == FolderSchema.DisplayName) return "displayName";
            if (p == FolderSchema.ParentFolderId) return "parentFolderId";
            if (p == FolderSchema.TotalCount) return "totalItemCount";
            if (p == FolderSchema.ChildFolderCount) return "childFolderCount";
            if (p == FolderSchema.UnreadCount) return "unreadItemCount";
            if (p == ItemSchema.PolicyTag) return OutlookExtendedProperties.PidComplianceTag.ToString();
            if (p == ItemSchema.Sensitivity) return OutlookExtendedProperties.PidNameMSIPLabels.ToString();

            return null;
        }

        private static bool IsCollectionField(string field)
            => field.StartsWith("toRecipients/", StringComparison.Ordinal) ||
               field.StartsWith("ccRecipients/", StringComparison.Ordinal) ||
               field.StartsWith("bccRecipients/", StringComparison.Ordinal) ||
               field.StartsWith("replyTo/", StringComparison.Ordinal);

        private static string GetCollectionBasePath(string field)
        {
            int idx = field.IndexOf('/');
            return idx > 0 ? field[..idx] : field;
        }

        private static string GetCollectionItemPath(string field)
        {
            int idx = field.IndexOf('/');
            return idx > 0 ? field[(idx + 1)..] : string.Empty;
        }

        private static string Format(object value) => value switch
        {
            null => "null",
            string s => $"'{EscapeFilterValue(s)}'",
            DateTime dt => $"'{dt.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}'",
            DateTimeOffset dto => $"'{dto.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}'",
            bool b => b.ToString().ToLowerInvariant(),
            int i => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
            long l => l.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => $"'{EscapeFilterValue(value?.ToString())}'"
        };

        private static string EscapeFilterValue(string value)
            => string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");

        #endregion
    }
}