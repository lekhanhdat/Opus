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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CplDBSettingsResponse : EDiscoveryResponse
    {
        //farm的db配置信息列表，用于GetAllFarmCplDBSettings的返回值
        [DataMember]
        public List<CplDBSettingsDto> CplDBSettingsList { get; set; }

        //IsConfigedDb的返回值，farm是否配置了db
        [DataMember]
        public Dictionary<string, bool> FarmConfigResult { get; set; }

        //返回storage police的列表
        [DataMember]
        public List<StoragePolicyDto> StoragePolicies { get; set; }
        [DataMember]
        public List<LogicalDeviceDto> LogicalDeviceList { get; set; }

        //LoadFarmConfigInfo的返回值，加载单个farm的配置信息
        [DataMember]
        public CplDBSettingsDto CplDBSettings { get; set; }

        //ValidationTest的返回值
        [DataMember]
        public bool ValidationTestResult { get; set; }

        //ConfigDB的返回值，tree表示配置成功，false表示配置失败
//        [DataMember]
//        public bool ConfigDBResult { get; set; }

        [DataMember]
        public ConfigDBResult ConfigDBResult { get; set; }


        //用于判断是否有farm没设置Compliance DB，true表示没有，false表示有
        [DataMember]
        public bool HasNoConfigDBResult { get; set; }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum StateEnum
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            NoFarm = 1
        }

    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfigDBResult
    {
        [EnumMember]
        InputIllegal = 0,
        [EnumMember]
        Failed = 1,

        [EnumMember]
        Success = 2,
        [EnumMember]
        TableExistButValidateFailed = 3,
        [EnumMember]
        TableNotFullyExist = 4,
        [EnumMember]
        TableCreateFailed = 5,
        [EnumMember]
        NotHaveCreatePermission = 6
    }
}
