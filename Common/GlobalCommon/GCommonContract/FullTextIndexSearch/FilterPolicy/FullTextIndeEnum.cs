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



using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.FullTextIndexSearch.FilterPolicy
{
    /// <summary>
    /// 对应Filter Policy的And Or
    /// </summary>
    [DataContract]
    public enum FullTextIndexAndOr
    {
        [EnumMember]
        None,
        [EnumMember]
        And,
        [EnumMember]
        Or
    }

    /// <summary>
    /// 时间范围
    /// </summary>
    [DataContract]
    public enum FullTextIndexDateTimeRangeType
    {
        [EnumMember]
        Day,
        [EnumMember]
        Week,
        [EnumMember]
        Month,
        [EnumMember]
        Year
    }

    /// <summary>
    /// 文件大小
    /// </summary>
    [DataContract]
    public enum FullTextIndexSizeRangeType
    {
        [EnumMember]
        KB,
        [EnumMember]
        MB,
        [EnumMember]
        GB
    }

    /// <summary>
    /// Level
    /// </summary>
    [DataContract]
    public enum FullTextIndexRuleLevel
    {
        [EnumMember]
        Item,
        [EnumMember]
        ItemVersion,
        [EnumMember]
        Document,
        [EnumMember]
        DocumentVersion,
        [EnumMember]
        Attachment
    }

    /// <summary>
    /// Rule类型
    /// </summary>
    [DataContract]
    public enum FullTextIndexRuleMetaDataType
    {
        [EnumMember]
        Number = 0,
        [EnumMember]
        String = 1,
        [EnumMember]
        DateTime = 2,
        [EnumMember]
        Enum = 3,
        [EnumMember]
        UserDefinedSize = 4,
        [EnumMember]
        UserDefinedNumber = 5,
        [EnumMember]
        UserDefinedString = 6,
        [EnumMember]
        UserDefinedDateTime = 7,
        [EnumMember]
        UserDefinedEnum = 8,
        [EnumMember]
        Version = 9,
        [EnumMember]
        None = 10,
        [EnumMember]
        Size = 11,
        [EnumMember]
        FileFormat = 12,
        [EnumMember]
        ArchiveTime = 13
    }

    /// <summary>
    /// ConditionType
    /// </summary>
    [DataContract]
    public enum FullTextIndexConditionType
    {
        // Number & Size
        [EnumMember]
        LargerThan = 0,
        [EnumMember]
        Equals = 1,
        [EnumMember]
        LessThan = 3,

        // DateTime
        [EnumMember]
        Before = 4,
        [EnumMember]
        After = 5,
        [EnumMember]
        Within = 6,
        [EnumMember]
        On = 7,
        [EnumMember]
        OldThan = 8,

        // String
        [EnumMember]
        Contains = 9,
        [EnumMember]
        DoesNotContain = 10,
        // String
        [EnumMember]
        IsExactly = 11,
        //Enum
        [EnumMember]
        NotEquals = 12,

        //Version
        [EnumMember]
        OnlyLatestVersions = 13,
        [EnumMember]
        OnlyLatestMajorVersions = 14,
        [EnumMember]
        OnlyMajorVersions = 15,
        [EnumMember]
        OnlyApprovedVersions = 16,

        //Will Delete
        [EnumMember]
        IsNotExactly = 18,
        [EnumMember]
        Matches = 19,
        [EnumMember]
        DoesNotMatch = 20,
        [EnumMember]
        None = 17,
		[EnumMember]
        GreaterOrEqualThan = 21,
        [EnumMember]
        LessOrEqualThan = 22
    }

    [DataContract]
    public enum FullTextIndexRuleName
    {
        [EnumMember]
        Name,
        [EnumMember]
        Size,
        [EnumMember]
        ModifiedTime,
        [EnumMember]
        CreatedTime,
        [EnumMember]
        CreatedBy,
        [EnumMember]
        ModifiedBy,
        [EnumMember]
        ContentType,
        [EnumMember]
        ColumnText,
        [EnumMember]
        ColumnNumber,
        [EnumMember]
        ColumnYesOrNo,
        [EnumMember]
        ColumnDataAndTime,
        [EnumMember]
        URL,
        [EnumMember]
        CheckInComment,
        [EnumMember]
        Title,
        [EnumMember]
        FileFormat,
        [EnumMember]
        ArchiveTime
    }
}
