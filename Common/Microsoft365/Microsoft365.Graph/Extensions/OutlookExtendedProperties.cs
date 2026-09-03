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
public static class OutlookExtendedProperties
{
    #region Public

    #region PropertySet

    /// <summary>
    /// Area: Journal
    /// </summary>
    public static readonly Guid PSETID_Log = new Guid("0006200A-0000-0000-C000-000000000046");

    /// <summary>
    /// Area:Contact
    /// </summary>
    public static readonly Guid PSETID_Address = new Guid("00062004-0000-0000-C000-000000000046");

    #endregion

    #region Item Common
    public static IMapiExtendedPropertyDefinition PidTagSubject => pidTagSubject.Value;
    public static IMapiExtendedPropertyDefinition PidTagInternetMessageId => pidTagInternetMessageId.Value;
    public static IMapiExtendedPropertyDefinition PidTagHasAttachments => pidTagHasAttachments.Value;
    public static IMapiExtendedPropertyDefinition PidTagRead => pidTagRead.Value;
    public static IMapiExtendedPropertyDefinition PidTagSenderName => pidTagSenderName.Value;
    public static IMapiExtendedPropertyDefinition PidTagSenderSmtpAddress => pidTagSenderSmtpAddress.Value;
    public static IMapiExtendedPropertyDefinition PidTagReceiverSmtpAddress => pidTagReceiverSmtpAddress.Value;
    public static IMapiExtendedPropertyDefinition PidTagDisplayTo => pidTagDisplayTo.Value;
    public static IMapiExtendedPropertyDefinition PidTagDisplayCc => pidTagDisplayCc.Value;
    public static IMapiExtendedPropertyDefinition PidTagImportance => pidTagImportance.Value;
    public static IMapiExtendedPropertyDefinition PidDeliveryTime => pidDeliveryTime.Value;
    public static IMapiExtendedPropertyDefinition PidTagModifiedBy => pidTagModifiedBy.Value;
    public static IMapiExtendedPropertyDefinition PidTagSize => pidTagSize.Value;
    public static IMapiExtendedPropertyDefinition PidTagConversationTopic => pidTagConversationTopic.Value;
    public static IMapiExtendedPropertyDefinition PidTagReceivedRepresentingName => pidTagReceivedRepresentingName.Value;
    public static IMapiExtendedPropertyDefinition PidTagReceivedRepresentingSmtpAddress => pidTagReceivedRepresentingSmtpAddress.Value;
    public static IMapiExtendedPropertyDefinition PidTagSensitivity => pidTagSensitivity.Value;
    /// <summary>
    /// DateTimeSent
    /// </summary>
    public static IMapiExtendedPropertyDefinition PidTagClientSubmitTime => pidTagClientSubmitTime.Value;
    /// <summary>
    /// DateTimeReceived
    /// </summary>
    public static IMapiExtendedPropertyDefinition PidTagMessageDeliveryTime => pidTagMessageDeliveryTime.Value;
    public static IMapiExtendedPropertyDefinition PidTagMessageFlags => pidTagMessageFlags.Value;
    // Sensitivity label
    public static IMapiExtendedPropertyDefinition PidNameMSIPLabels => pidNameMSIPLabels.Value;
    public static IMapiExtendedPropertyDefinition PidTagLastModifiedTime => pidTagLastModifiedTime.Value;

    #region Retention Label
    public static IMapiExtendedPropertyDefinition PidComplianceTag => pidComplianceTag.Value;
    public static IMapiExtendedPropertyDefinition PidTagRetentionId => pidTagRetentionId.Value;
    public static IMapiExtendedPropertyDefinition PidTagRetentionPeriod => pidTagRetentionPeriod.Value;
    public static IMapiExtendedPropertyDefinition PidTagRetentionDate => pidTagRetentionDate.Value;
    public static IMapiExtendedPropertyDefinition PidTagRetentionFlags => pidTagRetentionFlags.Value;
    #endregion

    #endregion

    #region Contact
    public static IMapiExtendedPropertyDefinition PidLidEmail1EmailAddress => pidLidEmail1EmailAddress.Value;
    #endregion

    #region Folder

    //PR_FOLDER_PATHNAME
    public static IMapiExtendedPropertyDefinition OtherPidFolderPathName => otherPidFolderPathName.Value;
    #endregion

    #region Custom
    public static IMapiExtendedPropertyDefinition CustomPidRestoreItemId => customRestoreItemId.Value;
    public static IMapiExtendedPropertyDefinition CustomPidItemTermId => customItemTermId.Value;
    #endregion

    #endregion

    #region Private

    #region Item Common
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagSubject = new(() => new TagPropertyDefinition(0x0037, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagInternetMessageId = new(() => new TagPropertyDefinition(0x1035, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagHasAttachments = new(() => new TagPropertyDefinition(0x0E1B, MapiPropertyType.Boolean));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagRead = new(() => new TagPropertyDefinition(0x0E69, MapiPropertyType.Boolean));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagSenderName = new(() => new TagPropertyDefinition(0x0C1A, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagSenderSmtpAddress = new(() => new TagPropertyDefinition(0x5D01, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagReceiverSmtpAddress = new(() => new TagPropertyDefinition(0x5D07, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagDisplayTo = new(() => new TagPropertyDefinition(0x0E04, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagDisplayCc = new(() => new TagPropertyDefinition(0x0E03, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagImportance = new(() => new TagPropertyDefinition(0x0017, MapiPropertyType.Integer));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidDeliveryTime = new(() => new TagPropertyDefinition(0x0E06, MapiPropertyType.SystemTime));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagModifiedBy = new(() => new TagPropertyDefinition(0x3FFA, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagClientSubmitTime = new(() => new TagPropertyDefinition(0x0039, MapiPropertyType.SystemTime));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagMessageDeliveryTime = new(() => new TagPropertyDefinition(0x0E06, MapiPropertyType.SystemTime));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagMessageFlags = new(() => new TagPropertyDefinition(0x0E07, MapiPropertyType.Integer));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidNameMSIPLabels = new(() => new NamedPropertyDefinition("msip_labels", MapiPropertyType.String, new Guid("00020386-0000-0000-C000-000000000046")));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidComplianceTag = new(() => new NamedPropertyDefinition("ComplianceTag", MapiPropertyType.String, new Guid("403FC56B-CD30-47C5-86F8-EDE9E35A022B")));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagRetentionId = new(() => new TagPropertyDefinition(0x3019, MapiPropertyType.Binary));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagRetentionPeriod = new(() => new TagPropertyDefinition(0x301A, MapiPropertyType.Integer));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagRetentionDate = new(() => new TagPropertyDefinition(0x301C, MapiPropertyType.SystemTime));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagRetentionFlags = new(() => new TagPropertyDefinition(0x301D, MapiPropertyType.Integer));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagSize = new(() => new TagPropertyDefinition(0x0E08, MapiPropertyType.Integer));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagLastModifiedTime = new Lazy<IMapiExtendedPropertyDefinition>(() => new TagPropertyDefinition(0x3008, MapiPropertyType.SystemTime));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagConversationTopic = new(() => new TagPropertyDefinition(0x0070, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagReceivedRepresentingName = new(() => new TagPropertyDefinition(0x0044, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagReceivedRepresentingSmtpAddress = new(() => new TagPropertyDefinition(0x5D08, MapiPropertyType.String));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidTagSensitivity = new(() => new TagPropertyDefinition(0x0036, MapiPropertyType.Integer));
    #endregion

    #region Contact
    private static readonly Lazy<IMapiExtendedPropertyDefinition> pidLidEmail1EmailAddress = new(() => new NamedPropertyDefinition(0x8083, MapiPropertyType.String, PSETID_Address));
    #endregion

    #region Folder
    private static readonly Lazy<IMapiExtendedPropertyDefinition> otherPidFolderPathName = new(() => new TagPropertyDefinition(0x66B5, MapiPropertyType.String));
    #endregion

    #region Custom
    private static readonly Lazy<IMapiExtendedPropertyDefinition> customRestoreItemId = new(() => new NamedPropertyDefinition(0xF555, MapiPropertyType.String, PSETID_Log));
    private static readonly Lazy<IMapiExtendedPropertyDefinition> customItemTermId = new(() => new NamedPropertyDefinition(0xF666, MapiPropertyType.String, new Guid("AA44DC13-6491-40C8-8C4C-5FE81370EFF3")));
    #endregion

    #endregion

    /// <summary>
    /// PidTagxxxxx
    /// </summary>
    internal struct TagPropertyDefinition : IMapiExtendedPropertyDefinition
    {
        public readonly UInt16 Id;
        public readonly MapiPropertyType Type;
        private string? shortString;

        public TagPropertyDefinition(UInt16 id, MapiPropertyType type)
        {
            // proptag: 0x0001-0x7fff
            Id = id;
            Type = type;
        }

        public bool Equals(IMapiExtendedPropertyDefinition? other)
        {
            if (other is TagPropertyDefinition otherProp)
            {
                return Id == otherProp.Id && Type == otherProp.Type;
            }
            return false;
        }

        public override string ToString() => $"'{Type} 0x{Id:X}'";
        public string GetShortString() => shortString ??= $"'{(UInt16)Type} {Id}'";

        public MapiPropertyType GetPropType() => Type;
    }

    /// <summary>
    /// PidLidxxxx or PidNamexxxx
    /// </summary>
    internal struct NamedPropertyDefinition : IMapiExtendedPropertyDefinition
    {
        public readonly UInt32 Id;
        public readonly String? Name;
        public readonly MapiPropertyType Type;
        public readonly Guid NamespaceId;

        private string? shortString;
        public readonly bool IsNameIdentifier = false;

        /// <summary>
        /// PidLidxxxx
        /// </summary>
        /// <param name="id">Graph support scope: 0x8000-0xfffe</param>
        public NamedPropertyDefinition(UInt32 id, MapiPropertyType type, Guid namespaceId)
        {
            Id = id;
            Type = type;
            NamespaceId = namespaceId;
        }

        /// <summary>
        /// PidNamexxxx
        /// </summary>
        public NamedPropertyDefinition(String name, MapiPropertyType type, Guid namespaceId)
        {
            Name = name;
            Type = type;
            NamespaceId = namespaceId;
            IsNameIdentifier = true;
        }

        public bool Equals(IMapiExtendedPropertyDefinition? other)
        {
            if (other is NamedPropertyDefinition otherProp)
            {
                if (IsNameIdentifier != otherProp.IsNameIdentifier)
                    return false;
                return (IsNameIdentifier ? Name!.Equals(otherProp.Name, StringComparison.OrdinalIgnoreCase) : Id == otherProp.Id)
                    && Type == otherProp.Type
                    && NamespaceId == otherProp.NamespaceId;
            }
            return false;
        }

        public override string ToString() => IsNameIdentifier
            ? $"'{Type} {NamespaceId.ToString("B")} Name {Name}'"
            : $"'{Type} {NamespaceId.ToString("B")} Id 0x{Id:X}'";
        public string GetShortString() => shortString ??= IsNameIdentifier
            ? $"'{(UInt16)Type} {NamespaceId.ToString("B")} Name {Name}'"
            : $"'{(UInt16)Type} {NamespaceId.ToString("B")} id {Id}'";

        public MapiPropertyType GetPropType() => Type;
    }

}
public interface IMapiExtendedPropertyDefinition : IEquatable<IMapiExtendedPropertyDefinition>
{
    String GetShortString();
    MapiPropertyType GetPropType();
    string ToString();
}

public enum MapiPropertyType : ushort
{
    ApplicationTime = 0,
    ApplicationTimeArray = 1,
    Binary = 2,
    BinaryArray = 3,
    Boolean = 4,
    CLSID = 5,
    CLSIDArray = 6,
    Currency = 7,
    CurrencyArray = 8,
    Double = 9,
    DoubleArray = 10,
    Error = 11,
    Float = 12,
    FloatArray = 13,
    Integer = 14,
    IntegerArray = 15,
    Long = 16,
    LongArray = 17,
    Null = 18,
    Object = 19,
    ObjectArray = 20,
    Short = 21,
    ShortArray = 22,
    SystemTime = 23,
    SystemTimeArray = 24,
    String = 25,
    StringArray = 26,
}
