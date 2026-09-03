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
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace ExchangeUtility.Graph.SearchFilterExtension
{
    internal class MailboxItemFilterConverter : IFilterConverter
    {
        public virtual void ConvertComparison(PropertyDefinitionBase prop, string op, object value, StringBuilder builder)
        {
            if (builder == null) return;

            var mapiProp = MapToExtendedProperty(prop);
            if (mapiProp != null)
            {
                string container = GetExtendedPropertyContainer(mapiProp);
                if (mapiProp == OutlookExtendedProperties.PidTagSize)
                {
                    builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and cast(ep/value, Edm.Int32) {op} {Format(value)})");
                }
                else if (mapiProp == OutlookExtendedProperties.PidTagClientSubmitTime ||
                         mapiProp == OutlookExtendedProperties.PidTagLastModifiedTime)
                {
                    builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and cast(ep/value, Edm.DateTimeOffset) {op} {Format(value)})");
                }
                else
                {
                    builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and ep/value {op} {Format(value)})");
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Property '{prop}' is not supported for MailboxItem Graph API filtering.");
            }
        }

        public virtual void ConvertContains(SearchFilter.ContainsSubstring contains, StringBuilder builder)
        {
            if (builder == null) return;

            var prop = contains.PropertyDefinition;
            string escapedValue = EscapeFilterValue(contains.Value);

            var mapiProp = MapToExtendedProperty(prop);
            if (mapiProp != null)
            {
                if (EnsureSendFromToCorrect("contains", mapiProp, escapedValue, out var result))
                {
                    builder.Append(result);
                    return;
                }

                builder.Append(ToContainsString(mapiProp, escapedValue));
            }
            else
            {
                throw new NotSupportedException(
                    $"Property '{prop}' does not support contains filter for MailboxItem.");
            }
        }

        public virtual void ConvertEquality(PropertyDefinitionBase prop, object value, StringBuilder builder)
        {
            if (builder == null) return;

            var mapiProp = MapToExtendedProperty(prop);
            if (mapiProp != null)
            {
                string escapedValue = Format(value);
                string container = GetExtendedPropertyContainer(mapiProp);

                if (mapiProp == OutlookExtendedProperties.PidNameMSIPLabels)
                {
                    var searchValue = $"'_Name={escapedValue.Substring(1, escapedValue.Length - 2)};'";

                    builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and contains(ep/value, {searchValue}))");
                    return;
                }
                else if (mapiProp == OutlookExtendedProperties.PidTagHasAttachments)
                {
                    builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and cast(ep/value, Edm.Boolean) eq {escapedValue})");
                }
                else
                {
                    if (EnsureSendFromToCorrect("eq", mapiProp, escapedValue, out var result))
                    {
                        builder.Append(result);
                        return;
                    }

                    builder.Append(ToEqualsString(mapiProp, escapedValue));
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Property '{prop}' is not supported for MailboxItem Graph API filtering.");
            }
        }

        public virtual void ConvertEqualityNot(PropertyDefinitionBase prop, object value, StringBuilder builder)
        {
            if (builder == null) return;

            builder.Append("not (");
            ConvertEquality(prop, value, builder);
            builder.Append(')');
        }

        public virtual void ConvertExists(PropertyDefinitionBase prop, StringBuilder builder)
        {
            if (builder == null) return;

            var mapiProp = MapToExtendedProperty(prop);
            if (mapiProp != null)
            {
                string container = GetExtendedPropertyContainer(mapiProp);
                builder.Append($"{container}/any(ep: ep/id eq {mapiProp} and ep/value ne null)");
            }
            else
            {
                throw new NotSupportedException(
                    $"Property '{prop}' is not supported for MailboxItem Graph API filtering.");
            }
        }

        public void ConvertExtendedProp(ExtendedPropertyDefinition ext, string op, object value, StringBuilder builder)
        {
            if (builder == null) return;

            string graphId = ext.ToGraphExtendedPropId();
            string container = GetExtendedPropertyContainer(ext);
            builder.Append($"{container}/any(ep: ep/id eq '{graphId}' and ep/value {op} {Format(value)})");
        }

        public void ConvertExtendedContains(ExtendedPropertyDefinition ext, string value, StringBuilder builder)
        {
            if (builder == null) return;

            string graphId = ext.ToGraphExtendedPropId();
            string container = GetExtendedPropertyContainer(ext);
            builder.Append($"{container}/any(ep: ep/id eq '{graphId}' and contains(ep/value, '{EscapeFilterValue(value)}'))");
        }

        public void ConvertExtendedExists(ExtendedPropertyDefinition ext, StringBuilder builder)
        {
            if (builder == null) return;

            string graphId = ext.ToGraphExtendedPropId();
            string container = GetExtendedPropertyContainer(ext);
            builder.Append($"{container}/any(ep: ep/id eq '{graphId}' and ep/value ne null)");
        }

        #region Mapping

        protected IMapiExtendedPropertyDefinition MapToExtendedProperty(PropertyDefinitionBase prop)
        {
            if (prop is not PropertyDefinition p)
                return null;

            if (p == ItemSchema.Subject) return OutlookExtendedProperties.PidTagSubject;
            if (p == ItemSchema.DateTimeReceived) return OutlookExtendedProperties.PidTagMessageDeliveryTime;
            if (p == ItemSchema.DateTimeSent) return OutlookExtendedProperties.PidTagClientSubmitTime;
            if (p == ItemSchema.HasAttachments) return OutlookExtendedProperties.PidTagHasAttachments;
            if (p == ItemSchema.Importance) return OutlookExtendedProperties.PidTagImportance;
            if (p == ItemSchema.DisplayTo) return OutlookExtendedProperties.PidTagDisplayTo;
            if (p == ItemSchema.DisplayCc) return OutlookExtendedProperties.PidTagDisplayCc;
            if (p == EmailMessageSchema.Sender) return OutlookExtendedProperties.PidTagSenderSmtpAddress;
            if (p == EmailMessageSchema.From) return OutlookExtendedProperties.PidTagSenderSmtpAddress;
            if (p == EmailMessageSchema.ReceivedBy) return OutlookExtendedProperties.PidTagReceiverSmtpAddress;
            if (p == EmailMessageSchema.IsRead) return OutlookExtendedProperties.PidTagRead;
            if (p == EmailMessageSchema.InternetMessageId) return OutlookExtendedProperties.PidTagInternetMessageId;
            if (p == EmailMessageSchema.Size) return OutlookExtendedProperties.PidTagSize;
            if (p == ItemSchema.PolicyTag) return OutlookExtendedProperties.PidComplianceTag;
            if (p == ItemSchema.Sensitivity) return OutlookExtendedProperties.PidNameMSIPLabels;
            if (p == ItemSchema.LastModifiedTime) return OutlookExtendedProperties.PidTagLastModifiedTime;

            return null;
        }

        private bool EnsureSendFromToCorrect(string op, IMapiExtendedPropertyDefinition mapiProp, string escapedValue, out string result)
        {
            result = string.Empty;
            if (mapiProp == OutlookExtendedProperties.PidTagSenderSmtpAddress)
            {
                result = $"({ToFilterString(op, OutlookExtendedProperties.PidTagSenderSmtpAddress, escapedValue)} or {ToFilterString(op, OutlookExtendedProperties.PidTagSenderName, escapedValue)})";
                return true;
            }
            else if (mapiProp == OutlookExtendedProperties.PidTagReceiverSmtpAddress)
            {
                result = $"({ToFilterString(op, OutlookExtendedProperties.PidTagReceiverSmtpAddress, escapedValue)} or {ToFilterString(op, OutlookExtendedProperties.PidTagDisplayTo, escapedValue)})";
                return true;
            }
            return false;
        }

        private string ToFilterString(string op, IMapiExtendedPropertyDefinition mapiProp, string escapedValue)
        {
            return op.ToLowerInvariant() switch
            {
                "contains" => ToContainsString(mapiProp, escapedValue),
                "eq" => ToEqualsString(mapiProp, escapedValue),
                _ => throw new NotSupportedException(
                    $"Operator '{op}' is not supported for MailboxItem Graph API filtering.")
            };
        }

        private string ToContainsString(IMapiExtendedPropertyDefinition mapiProp, string escapedValue)
        {
            string container = GetExtendedPropertyContainer(mapiProp);
            return $"{container}/any(ep: ep/id eq {mapiProp} and contains(ep/value, '{escapedValue}'))";
        }

        private string ToEqualsString(IMapiExtendedPropertyDefinition mapiProp, string escapedValue)
        {
            string container = GetExtendedPropertyContainer(mapiProp);
            return $"{container}/any(ep: ep/id eq {mapiProp} and ep/value eq {escapedValue})";
        }

        #endregion

        #region Formatting

        protected string Format(object value)
        {
            return value switch
            {
                null => "null",
                string s => $"'{EscapeFilterValue(s)}'",
                DateTime dt => $"{dt.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}",
                DateTimeOffset dto => $"{dto.UtcDateTime:yyyy-MM-ddTHH:mm:ss.fffZ}",
                bool b => b.ToString().ToLowerInvariant(),
                int i => i.ToString(CultureInfo.InvariantCulture),
                long l => l.ToString(CultureInfo.InvariantCulture),
                double d => d.ToString(CultureInfo.InvariantCulture),
                decimal dec => dec.ToString(CultureInfo.InvariantCulture),
                Importance imp => FormatImportance(imp),
                Sensitivity sens => FormatSensitivity(sens),
                Enum e => $"'{e}'",
                _ => $"'{EscapeFilterValue(value.ToString())}'"
            };
        }

        private static string FormatImportance(Importance importance) => importance switch
        {
            Importance.Low => "low",
            Importance.Normal => "normal",
            Importance.High => "high",
            _ => "normal"
        };

        private static string FormatSensitivity(Sensitivity sensitivity) => sensitivity switch
        {
            Sensitivity.Normal => "0",
            Sensitivity.Personal => "1",
            Sensitivity.Private => "2",
            Sensitivity.Confidential => "3",
            _ => "0"
        };

        protected static string EscapeFilterValue(string value)
            => string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");

        protected static string GetExtendedPropertyContainer(ExtendedPropertyDefinition ext)
            => ext.MapiType.ToString().Contains("Array", StringComparison.Ordinal)
                ? "multiValueExtendedProperties"
                : "singleValueExtendedProperties";

        protected string GetExtendedPropertyContainer(IMapiExtendedPropertyDefinition prop)
            => prop.GetPropType().ToString().Contains("Array", StringComparison.Ordinal)
                ? "multiValueExtendedProperties"
                : "singleValueExtendedProperties";

        #endregion
    }
}