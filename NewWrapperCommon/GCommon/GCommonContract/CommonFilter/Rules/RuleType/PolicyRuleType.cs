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
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PolicyRuleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ResultLevel = 1,
        [EnumMember]
        Url = 2,
        [EnumMember]
        Title = 4,
        [EnumMember]
        Name = 8,
        [EnumMember]
        Template = 16,
        [EnumMember]
        CreatedBy = 32,
        [EnumMember]
        CreatedTime = 64,
        [EnumMember]
        ModifiedTime = 128,
        [EnumMember]
        Owner = 256,
        [EnumMember]
        Inheritance = 512,
        [EnumMember]
        Permission = 1024,
        [EnumMember]
        Attribute = 2048,
        [EnumMember]
        FullTextIndex = 4096,
        //for auditor
        [EnumMember]
        Country = 8192,

        //Add for CA
        [EnumMember]
        UserAndGroup = 16384,
        [EnumMember]
        ContentType = 32768,
        [EnumMember]
        Versions = 65536,
        [EnumMember]
        Auditing = 131072,
        [EnumMember]
        Versioning = 262144,
        [EnumMember]
        CustomProperty = 524288,
        [EnumMember]
        AnonymousAccess = 1048576,
        [EnumMember]
        LockStatus = 2097152,
        [EnumMember]
        Size = 4194304,

        //Add for PR SQL Server Data Manager
        [EnumMember]
        LastAccessedTime =8388608,
        [EnumMember]
        Columns = 16777216,
        [EnumMember]
        ContentTypes = 33554432,

        //Add for CA item 
        [EnumMember]
        ColumnDateTime = 67108864,
        [EnumMember]
        ColumnBoolean = 134217728,
        [EnumMember]
        ColumnNumber = 268435456,
        [EnumMember]
        ColumnText = 536870912,
        [EnumMember]
        ColumnChoice = 1073741824,

    }
}