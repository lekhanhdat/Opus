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

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract]
    public class EDSyncFileSettingsDto : EDiscoveryRequest
    {
        /// <summary>
        /// 定义对配置信息.
        /// </summary>
        [DataMember]
        public List<EDSyncFilePlanDto> settings { get; set; }

        /// <summary>
        /// 定义对SyncFileSetting的动作.
        /// </summary>
        [DataMember]
        public EDSyncFileSettingAction Action { get; set; }

        /// <summary>
        /// 单个添加Setting的唯一入口.
        /// </summary>
        /// <param name="settingDto"></param>
        public void Add(EDSyncFilePlanDto settingDto)
        {
            if (this.settings == null)
            {
                this.settings = new List<EDSyncFilePlanDto>();
            }
            this.settings.Add(settingDto);
        }

        /// <summary>
        /// 添加多个Setting的唯一入口.
        /// </summary>
        /// <param name="settings"></param>
        public void AddRange(ref List<EDSyncFilePlanDto> settings)
        {
            this.settings.AddRange(settings);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EDSyncFileSettingAction : uint
    {
        [EnumMember]
        SaveSetting = 0,
        [EnumMember]
        SaveAndRun = 1
    }
}
