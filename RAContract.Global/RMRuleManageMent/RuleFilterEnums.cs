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
using AvePoint.RA.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMRuleManageMent
{
    /// <summary>
    /// Determines the filter rule type.
    /// </summary>
    [DataContract]
    public enum ArchiverFilterRuleType
    {
        /// <summary>
        /// Indicates that the rule type of a filter is Name.
        /// </summary>
        [EnumMember]
        Name = 1,
        /// <summary>
        /// Indicates that the rule type of a filter is Document Size.
        /// </summary>
        [EnumMember] 
        DocumentSize = 2,
        /// <summary>
        /// Indicates that the rule type of a filter is Modified Time.
        /// </summary>
        [EnumMember] 
        ModifiedTime = 3,
        /// <summary>
        /// Indicates that the rule type of a filter is Created Time.
        /// </summary>
        [EnumMember] 
        CreatedTime = 4,
        /// <summary>
        /// Indicates that the rule type of a filter is Created By.
        /// </summary>
        [EnumMember] 
        CreatedBy = 5,
        /// <summary>
        /// Indicates that the rule type of a filter is Modified By.
        /// </summary>
        [EnumMember] 
        ModifiedBy = 6,
        /// <summary>
        /// Indicates that the rule type of a filter is Content Type.
        /// </summary>
        [EnumMember] 
        ContentType = 7,
        /// <summary>
        /// Indicates that the rule type of a filter is Column(Text).
        /// </summary>
        [EnumMember] 
        TextColumn = 8,
        /// <summary>
        /// Indicates that the rule type of a filter is Column(Number).
        /// </summary>
        [EnumMember] 
        NumberColumn = 9,
        /// <summary>
        /// Indicates that the rule type of a filter is Column(Boolean).
        /// </summary>
        [EnumMember] 
        BooleanColumn = 10,
        /// <summary>
        /// Indicates that the rule type of a filter is Column(Date and Time).
        /// </summary>
        [EnumMember] 
        DateTimeColumn = 11,
        /// <summary>
        /// Indicates that the rule type of a filter is Parent List Type ID.
        /// </summary>
        [EnumMember] 
        ParentListTypeID = 12,
        /// <summary>
        /// Indicates that the rule type of a filter is Last Accessed Time.
        /// </summary>
        [EnumMember] 
        LastAccessedTime = 13,
        /// <summary>
        /// Indicates that the rule type of a filter is Title.
        /// </summary>
        [EnumMember] 
        Title = 14,
        /// <summary>
        /// Indicates that the rule type of a filter is Size.
        /// </summary>
        [EnumMember] 
        Size = 15,
        /// <summary>
        /// Indicates that the rule type of a filter is Keep the Latest Version.
        /// </summary>
        [EnumMember]
        KeepTheLatestVersion = 16,
        /// <summary>
        /// Indicates that the rule type of a filter is URL.
        /// </summary>
        [EnumMember] 
        URL = 17,
        /// <summary>
        /// Indicates that the rule type of a filter is Custom Property(Text).
        /// </summary>
        [EnumMember] 
        TextCustomProperty = 18,
        /// <summary>
        /// Indicates that the rule type of a filter is Custom Property(Number).
        /// </summary>
        [EnumMember] 
        NumberCustomProperty = 19,
        /// <summary>
        /// Indicates that the rule type of a filter is Custom Property(Boolean).
        /// </summary>
        [EnumMember] 
        BooleanCustomProperty = 20,
        /// <summary>
        /// Indicates that the rule type of a filter is Custom Property(Date and Time).
        /// </summary>
        [EnumMember] 
        DateTimeCustomProperty = 21,
        /// <summary>
        /// Indicates that the rule type of a filter is Primary Administrtor.
        /// </summary>
        [EnumMember] 
        PrimaryAdministrator = 22,
        /// <summary>
        /// Indicates that the rule type of a filter is Site Collection Size Trigger.
        /// </summary>
        [EnumMember]
        SiteCollectionSizeTrigger = 23,
        /// <summary>
        /// Indicates that the rule type of a filter is Conversation Content.
        /// </summary>
        [EnumMember] 
        ConversationContent = 24,
        /// <summary>
        /// Indicates that the rule type of a filter is Participant.
        /// </summary>
        [EnumMember] 
        Participant = 25,
        /// <summary>
        /// Indicates that the rule type of a filter is Posted By.
        /// </summary>
        [EnumMember] 
        PostedBy = 26,
        /// <summary>
        /// Indicates that the rule type of a filter is Replied By.
        /// </summary>
        [EnumMember] 
        RepliedBy = 27,
        /// <summary>
        /// Indicates that the rule type of a filter is Linked By.
        /// </summary>
        [EnumMember] 
        LikedBy = 28,
        /// <summary>
        /// Indicates that the rule type of a filter is Mentioned Name.
        /// </summary>
        [EnumMember] 
        MentionedName = 29,
        /// <summary>
        /// Indicates that the rule type of a filter is Hashtag.
        /// </summary>
        [EnumMember] 
        Hashtag = 30,
        /// <summary>
        ///  Indicates that the rule type of a filter is Term Properties.
        /// </summary>
        [EnumMember] 
        Term = 31,
        [EnumMember] 
        Subject = 40,
        [EnumMember] 
        AttachmentCount = 41,
        [EnumMember] 
        SendDateUTC = 42,
        [EnumMember] 
        SendFrom = 43,
        [EnumMember] 
        SendTo = 44,
        [EnumMember] 
        ParentFolderName = 45,
        [EnumMember] 
        ParentFolderNameHeirarchically = 46,
        [EnumMember] 
        RetentionLabel = 47,
        [EnumMember] 
        LastActiveTime = 48,
        [EnumMember]
        SensitivityLabel = 49,
        // label properties
        [EnumMember]
        TextLabelProperty = 50,
        [EnumMember]
        NumberLabelProperty = 51,
        [EnumMember]
        DateTimeLabelProperty = 52,
        [EnumMember]
        LabelName = 59,
        /// <summary>
        /// File System
        /// </summary>
        [EnumMember] 
        Type = 32,
        /// <summary>
        /// File System
        /// </summary>
        [EnumMember] 
        Owner = 33,
        [EnumMember] 
        FSTerm = 34,
        [EnumMember] 
        FilePath = 35,
        [EnumMember] 
        MetadataTextColumn = 36,
        [EnumMember] 
        MetadataNumberColumn = 37,
        [EnumMember] 
        ParentLibraryName = 38,
        ///<summary>
        /// Teams
        /// </summary>
        [EnumMember]
        Classification = 53,
        [EnumMember]
        DisplayName = 54,
        [EnumMember]
        Member = 55,
        [EnumMember]
        Privacy = 56,
        [EnumMember]
        TeamsStatus = 57,
        [EnumMember]
        TeamType = 58,
        [EnumMember]
        SensitivityLabelFullName = 60,
        [EnumMember]
        DocumentModified = 61,
        [EnumMember]
        ParentLibraryText = 62,
        [EnumMember]
        ParentLibraryNumber = 63,
        [EnumMember]
        ParentLibraryBoolean = 64,
        [EnumMember]
        ParentLibraryDateTime = 65,
        [EnumMember]
        ParentSiteCollectionText = 66,
        [EnumMember]
        ParentSiteCollectionNumber = 67,
        [EnumMember]
        ParentSiteCollectionBoolean = 68,
        [EnumMember]
        ParentSiteCollectionDateTime = 69,
        [EnumMember]
        PropertyBagText = 70,
        [EnumMember]
        PropertyBagNumber = 71,
        [EnumMember]
        PropertyBagBoolean = 72,
        [EnumMember]
        PropertyBagDateTime = 73,
        [EnumMember]
        LastestSubfolderDisposalDate = 74,
        [EnumMember]
        OrphanedFolderRule = 75,
    }
    /// <summary>
    /// Determines the filter condition.
    /// </summary>
    [DataContract]
    public enum ArchiverFilterCondition
    {
        /// <summary>
        /// Indicates that a filter will be used under the Matches condition.
        /// </summary>
        [EnumMember]
        Matches = 1051744,
        /// <summary>
        /// Indicates that a filter will be used under the Does Not Match condition.
        /// </summary>
        [EnumMember] 
        DoesNotMatch = 2103488,
        /// <summary>
        /// Indicates that a filter will be used under the Contains condition.
        /// </summary>
        [EnumMember] 
        Contains = 8,
        /// <summary>
        /// Indicates that a filter will be used under the Does Not Contain condition.
        /// </summary>
        [EnumMember] 
        DoesNotContain = 525872,
        /// <summary>
        /// Indicates that a filter will be used under the Equals condition.
        /// </summary>
        [EnumMember] 
        Equals = 1,
        /// <summary>
        /// Indicates that a filter will be used under the Does Not Equal condition.
        /// </summary>
        [EnumMember] 
        DoesNotEqual = 4206976,
        /// <summary>
        /// Indicates that a filter will be used under the Greater Than or Equal To condition.
        /// </summary>
        [EnumMember] 
        GreaterThanOrEqualTo = 32,
        /// <summary>
        /// Indicates that a filter will be used under the Less Than or Equal To condition.
        /// </summary>
        [EnumMember] 
        LessThanOrEqualTo = 16,
        /// <summary>
        /// Indicates that a filter will be used under the From To condition.
        /// </summary>
        [EnumMember] 
        FromTo = 2048,
        /// <summary>
        /// Indicates that a filter will be used under the Before condition.
        /// </summary>
        [EnumMember] 
        Before = 4096,
        /// <summary>
        /// Indicates that a filter will be used under the Older Than condition.
        /// </summary>
        [EnumMember] 
        OlderThan = 65734,
        /// <summary>
        /// Indicates that a filter will be used under the IsEmpty condition.
        /// </summary>
        [EnumMember] 
        IsEmpty = 65736,
        /// <summary>
        /// Indicates that a filter will be used under the ListIn condition.
        /// </summary>
        [EnumMember] 
        ListIn = 65737,
        /// <summary>
        /// Indicates that a filter will be used under the Major Versions condition.
        /// </summary>
        [EnumMember] 
        MajorVersions = 16777216,
        /// <summary>
        /// Indicates that a filter will be used under the Major and Minor Versions condition.
        /// </summary>
        [EnumMember] 
        MajorAndMinorVersions = 8413952,
        /// <summary>
        /// Indicates that a filter will be used under the Major without Minor Versions condition.
        /// </summary>
        [EnumMember] 
        MajorVersionsNoMinor = 33554432,
        /// <summary>
        /// Indicates that a filter will be used under the Minor of Each Major Versions condition.
        /// </summary>
        [EnumMember] 
        MinorVersionsOfEachMajor = 67108864,
        /// <summary>
        /// Indicates that a filter will be used under the Minor of The Latest Major Versions condition.
        /// </summary>
        [EnumMember] 
        MinorVersionsOfTheLatestMajor = 134217728
    }
    /// <summary>
    /// Determines the logical relationship between filters.
    /// </summary>
    [DataContract]
    public enum ArchiverFilterCombineMode
    {
        /// <summary>
        /// Indicates that filters are combined by And.
        /// </summary>
        [EnumMember]
        And = 0,
        /// <summary>
        /// Indicates that filters are combined by Or.
        /// </summary>
        [EnumMember]
        Or = 1
    }
}
