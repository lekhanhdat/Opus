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



namespace AvePoint.GCommon.Contract.CommonFilter
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PolicyCondition
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Exactly = 1,
        [EnumMember]
        StartWith = 2,
        [EnumMember]
        EndWith = 4,
        [EnumMember]
        Contains = 8,
        [EnumMember]
        LessOrEqualThan = 16,
        [EnumMember]
        GreaterOrEqualThan = 32,
        [EnumMember]
        OnlyLastNVersions = 64,
        [EnumMember]
        OnlyLastMajorNVersions = 128,
        [EnumMember]
        OnlyMajorVersions = 256,
        [EnumMember]
        OnlyMionrVersions = 512,
        [EnumMember]
        OnlyApproved = 1024,
        [EnumMember]
        FromTo = 2048,
        [EnumMember]
        Before = 4096,
        [EnumMember]
        After = 8192,
        [EnumMember]
        On = 16384,
        [EnumMember]
        WithIn = 32867,
        [EnumMember]
        OlderThan = 65734,
        [EnumMember]
        LessThan = 65735,
        [EnumMember]
        IsEmpty = 65736,
        [EnumMember]
        ListIn = 65737,
        [EnumMember]
        ExceptLastNVersions = 131468,
        [EnumMember]
        Equals = 262936,
        [EnumMember]
        DoesNotContains = 525872,
        [EnumMember]
        Match = 1051744,
        [EnumMember]
        DoesNotMatch = 2103488,
        [EnumMember]
        IsExactlyNot = 4206976,
        [EnumMember]
        MajorAndMintorVersions = 8413952,
        [EnumMember]
        ExceptLastNMajorVersions = 16777216,     //10 ^ 24
        //[EnumMember]
        //KeepMajorVersions = 33554432,
        //[EnumMember]
        //MinorOfEachMajorVersions = 67108864,
        //[EnumMember]
        //LastMinorOfMajorVersions = 134217728,
        [EnumMember]
        MajorWithoutMinorVersions = 33554432,
        [EnumMember]
        MinorOfEachMajorVersion = 67108864,
        [EnumMember]
        MinorOfTheLatestMajorVersion = 134217728,

        [EnumMember]
        RexMatch = 268435456,

        [EnumMember]
        RexNotMatch = 536870912,

        [EnumMember]
        ComboBoxItemDataTypeIs = 1073741824
    }
}
