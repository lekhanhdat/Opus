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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CentralDatabaseDto : IProfileContent
    {
        [DataMember]
        public string ProfileId { get; set; }//存储在Peofile表的ID

        [DataMember]
        public ConBean conBean { get; set; }

        [DataMember]
        public ConBean oldConBean { get; set; }

        [DataMember]
        public List<string> oldTables { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string JobDetails { get; set; }

        [DataMember]
        public string JobNotification { get; set; }

        [DataMember]
        public string JobStatitics { get; set; }

        [DataMember]
        public string Description { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConBean
    {
        [DataMember]
        public string Catalog { get; set; } //database name

        [DataMember]
        public int Timeout { get; set; } //Connection time= 10

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string OldPassword { get; set; }//加密时使用

        [DataMember]
        public string Security { get; set; } //SSPI Windows 用户使用

        [DataMember]
        public string Host { get; set; } //server name

        [DataMember]
        public int Authentication { get; set; }//Windows or SQL

        [DataMember]
        public int Port { get; set; }//端口号，暂时没用

        [DataMember]
        public bool SpacifyWindowsAccount { get; set; }//Windows 特定用户

        [DataMember]
        public string Failover { get; set; }//Failover server name
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DataBaseResult
    {
        [DataMember]
        public ValidateDataBaseResult resultType { get; set; }//操作成功失败

        [DataMember]
        public string Message { get; set; } //返回附加信息，备用

        [DataMember]
        public List<CentralDatabaseBeUsedDto> CentralDatabaseBeUsedDtos { get; set; }

        [DataMember]
        public List<string> tables {get;set;}

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ValidateDataBaseResult
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Success = 1,

        [EnumMember]
        ConnectionError = 2,

        [EnumMember]
        SaveError = 4,

        [EnumMember]
        Exception = 8,

        [EnumMember]
        DatabaseExist = 10,

        [EnumMember]
        NameExit = 12,

        [EnumMember]
        TableisExist = 13,
    }
}
