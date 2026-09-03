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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseUnitDto
    {
        [DataMember]
        public LicenseUnitType Type { get; set; }
        [DataMember]
        public long ExpirationTime { get; set; }
        [DataMember]
        public int StorageSize { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseUnitType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Granular = 1 << 0,
        [EnumMember]
        Exchange = 1 << 1,
        [EnumMember]
        ReportCenter = 1 << 2,
        [EnumMember]
        ContentManager = 1 << 3,
        [EnumMember]
        DeploymentManager = 1 << 4,
        [EnumMember]
        Replicator = 1 << 5,
        [EnumMember]
        Administrator = 1 << 6,
        [EnumMember]
        Archiver = 1 << 7,
        [EnumMember]
        IdentityManager = 1 << 8,
        [EnumMember]
        Office365Backup = 1 << 18,
        [EnumMember] // 仅用于判断产品
        Office365Management = 1 << 19,
        [EnumMember] // 仅用于判断产品
        Office365Archiving = 1 << 20,
        [EnumMember]
        DocAveOnline = 1 << 21,
        [EnumMember] // 仅用于判断产品
        Office365Records = 1 << 28
    }

    public class LicenseDtoUtil
    {
        public static LicenseUnitType GetLicenseUnitTypeByName(string name)
        {
            LicenseUnitType type;
            bool isValid = Enum.TryParse(name, out type);
            return isValid ? type : LicenseUnitType.None;
        }
    }
}
