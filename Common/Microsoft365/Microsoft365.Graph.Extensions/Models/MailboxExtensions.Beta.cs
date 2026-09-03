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
namespace Microsoft365.Graph.Extensions;
using System;
/// <summary>
/// Provides extension methods for mailbox services.
/// </summary>
public static partial class ModelExtensions
{
    public static string? WellKnownFolderName(this GraphBetaModels.MailboxFolder folder)
    {
        return folder.GetFromAdditionalData<string>("wellKnownName");
    }
    public static bool IsDeleted(this GraphBetaModels.MailboxFolder folder)
    {
        return folder.TryGetAdditionalData("@removed", out var _);
    }
    public static string MailboxId(this GraphBetaModels.MailboxFolder folder) => Extension.GetMailboxId(folder);

    public static string? Path(this GraphBetaModels.MailboxFolder folder)
    {
        //Path is the folder name, not the full path.
        return folder.GetPropertyValue(0x66B5, "Path", allowNull: true)?.Replace('\ufffe', '\\');
    }

    public static bool IsRootFolder(this GraphBetaModels.MailboxFolder folder)
    {
        return folder.WellKnownFolderName()?.ToLowerInvariant() switch
        {
            "msgfolderroot" => true,
            "recoverableitemsroot" => true,
            "archivemsgfolderroot" => true,
            "archiverecoverableitemsroot" => true,
            _ => false
        };
    }

    internal static string RawUrl(this GraphBetaModels.MailboxFolder folder)
    {
        return $"{folder.ParentMailboxUrl.EnsureIfNotNullOrEmpty().TrimEnd('/')}/folders/{folder.Id.EnsureIfNotNullOrEmpty()}";
    }
    public static bool IsDeleted(this GraphBetaModels.MailboxItem item)
    {
        return item.TryGetAdditionalData("@removed", out var _);
    }

    public static string? Subject(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0037, "Subject", true, true);
    }

    //Some types do not contain this attribute, such as: "IPM.Contact"
    public static string? SenderName(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0C1A, "SenderName", true, true);
    }

    public static string? SenderEmail(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x5D01, "SenderEmail", true, true);
    }

    public static string? DisplayTo(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0xe04, "DisplayTo", true, true);
    }

    public static string? ModifiedBy(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x3FFA, "ModifiedBy", true);
    }

    public static string? RetentionLabel(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue("String {403fc56b-cd30-47c5-86f8-ede9e35a022b} Name ComplianceTag", "RetentionLabel", true);
    }

    public static string? RetentionLabelId(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x3019, "RetentionLabelId", true, true);
    }

    public static string? DisplayCc(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0E03, "DisplayCc", true, true);
    }

    public static int? ImportanceLevel(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyIntegerValue(0x0017, "ImportanceLevel");
    }

    public static string? ConversationTopic(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0070, "ConversationTopic", true, true);
    }

    public static string? ReceivedRepresentingName(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0044, "ReceivedRepresentingName", true, true);
    }

    public static string? ReceivedRepresentingSmtpAddress(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x5D08, "ReceivedRepresentingSmtpAddress", true, true);
    }

    public static string? SensitivityLevel(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x0036, "SensitivityLevel", true) switch
        {
            "0" => "Normal",
            "1" => "Personal",
            "2" => "Private",
            "3" => "Confidential",
            _ => string.Empty
        };
    }

    public static DateTime? Received(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyDateTimeValue(0x0E06, "Received").GetValueOrDefault();
    }

    /// <summary>
    /// Same as DateTimeSent in EWS
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static DateTime? ClientSubmitTime(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyDateTimeValue(0x0039, "ClientSubmitTime");
    }
    /// <summary>
    /// Same as DateTimeReceived in EWS
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static DateTime? MessageDeliveryTime(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyDateTimeValue(0x0E06, "MessageDeliveryTime");
    }

    public static bool HasAttachments(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyBooleanValue(0x0E1B, "HasAttachments");
    }

    public static bool IsRead(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyBooleanValue(0x0E69, "IsRead");
    }

    public static string? InternetMessageId(this GraphBetaModels.MailboxItem item)
    {
        return item.GetPropertyValue(0x1035, "InternetMessageId", true);
    }

    public static string? RestoreItemId(this GraphBetaModels.MailboxItem item)
    {
        return item?.SingleValueExtendedProperties?.FirstOrDefault(p => p.PropID() == 0xF555)?.Value;
    }

    public static Int32? MsgFlag(this GraphBetaModels.MailboxItem item)
    {
        return item!.GetPropertyIntegerValue(0x0E07, "MsgFlag");
    }

    public static string? ContactEmail1EmailAddress(this GraphBetaModels.MailboxItem item)
    {
        //"String {00062004-0000-0000-c000-000000000046} Id 0x8083"
        return item!.GetPropertyValue(0x8083, "Email1EmailAddress", true);
    }

    #region ReadExtendedProperties
    private static string? GetPropertyValue(this GraphBetaModels.MailboxItem item, int propId, string propertyName, bool allowNull = false, bool isAllowEmpty = false)
    {
        return GetPropertyValue(item?.SingleValueExtendedProperties, propId, propertyName, allowNull, isAllowEmpty);
    }

    private static string? GetPropertyValue(this GraphBetaModels.MailboxItem item, string propId, string propertyName, bool allowNull = false, bool isAllowEmpty = false)
    {
        return GetPropertyValue(item?.SingleValueExtendedProperties, propId, propertyName, allowNull, isAllowEmpty);
    }

    private static string? GetPropertyValue(this GraphBetaModels.MailboxFolder folder, int propId, string propertyName, bool allowNull = false, bool isAllowEmpty = false)
    {
        return GetPropertyValue(folder?.SingleValueExtendedProperties, propId, propertyName, allowNull, isAllowEmpty);
    }

    private static string? GetPropertyValue(IList<GraphBetaModels.SingleValueLegacyExtendedProperty>? properties, int propId, string propertyName, bool allowNull = false, bool isAllowEmpty = false)
    {
        var property = properties?.FirstOrDefault(p => p.PropID() == propId);

        if ((!allowNull) && property == null)
            throw new InvalidOperationException($"{propertyName} property not found.");

        if (property?.Value is null && isAllowEmpty)
        {
            return string.Empty;
        }
        return property?.Value;
    }

    private static string? GetPropertyValue(IList<GraphBetaModels.SingleValueLegacyExtendedProperty>? properties, string propId, string propertyName, bool allowNull = false, bool isAllowEmpty = false)
    {
        var property = properties?.FirstOrDefault(p => p.Id == propId);

        if ((!allowNull) && property == null)
            throw new InvalidOperationException($"{propertyName} property not found.");

        if (property?.Value is null && isAllowEmpty)
        {
            return string.Empty;
        }
        return property?.Value;
    }

    private static DateTime? GetPropertyDateTimeValue(this GraphBetaModels.MailboxItem item, int propId, string propertyName)
    {
        var value = item.GetPropertyValue(propId, propertyName, true);

        return string.IsNullOrEmpty(value) ? default : DateTime.Parse(value);
    }

    private static Int32? GetPropertyIntegerValue(this GraphBetaModels.MailboxItem item, int propId, string propertyName)
    {
        var value = item.GetPropertyValue(propId, propertyName, true);
        return string.IsNullOrEmpty(value) ? default : Int32.Parse(value);
    }

    private static bool GetPropertyBooleanValue(this GraphBetaModels.MailboxItem item, int propId, string propertyName)
    {
        var value = item.GetPropertyValue(propId, propertyName);
        return !string.IsNullOrEmpty(value) && bool.Parse(value);
    }

    #endregion

    private static int? PropID(this GraphBetaModels.SingleValueLegacyExtendedProperty prop)
    {
        var idString = prop.Id?.Split(' ').LastOrDefault();
        if (string.IsNullOrEmpty(idString) || !idString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        return int.Parse(idString[2..], System.Globalization.NumberStyles.HexNumber);
    }

    public static WellKnownFolderName? WellKnownFolderNameEnum(this GraphBetaModels.MailboxFolder folder)
    {
        return folder.WellKnownFolderName()?.ConvertToWellKnownFolderNameEnum();
    }

    public static string ConvertToString(this WellKnownFolderName name)
    {
        return name.ToString().ToLowerInvariant();
    }

    public static WellKnownFolderName? ConvertToWellKnownFolderNameEnum(this string wellKnownFolderName)
    {
        return Enum.TryParse<WellKnownFolderName>(wellKnownFolderName, true, out var result) ? result : null;
    }
}