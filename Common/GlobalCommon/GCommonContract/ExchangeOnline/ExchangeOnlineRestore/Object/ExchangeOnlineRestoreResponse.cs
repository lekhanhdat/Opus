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



namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion ==

    /// <summary> 前台很多页面初始化的时候需要请求多项数据，为了减少通讯次数，添加此类来实现一次请求返回多项 </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public  class ExchangeOnlineRestoreResponse
    {
        [DataMember]
        public List<NameAndIdDto> LanguageMappings { get; set; }

        [DataMember]
        public List<NameAndIdDto> UserMappings { get; set; }

        [DataMember]
        public List<NameAndIdDto> DomainMappings { get; set; }

        [DataMember]
        public List<ServiceGroupDto> AgentGroup { get; set; }

        [DataMember]
        public List<ProfileDto> NotificationProfiles { get; set; }

        [DataMember]
        public List<StoragePolicyDto> StoragePolicys { get; set; }

        /// <summary> 标示用户是否配置了Email service setting. </summary>
        [DataMember]
        public bool IsEnableNotificationSetting { get; set; }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum EORequsetType
    {
        [EnumMember]
        Mappings=1,

        [EnumMember]
        AgentGroup=2,

        [EnumMember]
        NotificationSetting = 4,

        [EnumMember]
        NotificationProfile = 8,

        [EnumMember]
        StoragePolicy = 16
    }
}
