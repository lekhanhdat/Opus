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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CplDBSettingsDto : ProfileDto
    {
        [DataMember]
        public string DBServer { get; set; }
        [DataMember]
        public string DBName { get; set; }
        [DataMember]
        public AuthenticationType AuthenticationType { get; set; }
        [DataMember]
        public string Account { get; set; }
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// DB设置的类型,是正常的还是Connection String类型的
        /// </summary>
        [DataMember]
        public DBSettingType DBSettingType { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public bool IsAvailable { get; set; }

        [DataMember]
        public String ConnectionString { get; set; }

        [DataMember]
        public string FailoverPartner { get; set; }

        [DataMember]
        public bool IsSameMachine { get; set; }

        public const string ID_PREFIX = "CPLDB_";

        public static string GenerateId(string farmId)
        {
            if(String.IsNullOrEmpty(farmId))
            {
                return null;
            }
            return ID_PREFIX + farmId;
        }

    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuthenticationType
    {
        [EnumMember]
        Windows = 0,
        [EnumMember]
        SQL = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DBSettingType
    {
        [EnumMember]
        Normal,
        [EnumMember]
        ConnectionString
    }


}
