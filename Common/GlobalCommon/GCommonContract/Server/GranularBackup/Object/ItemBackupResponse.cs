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




namespace AvePoint.GCommon.Contract.Server.GranularBackup.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ItemBackupResponse
    {
        [DataMember]
        public List<StoragePolicyDto> StoragePolicys { get; set; }

        [DataMember]
        public List<NameAndIdDto> FilterPolicys { set; get; }

        [DataMember]
        public GranularBackupPlanDto QuickBackupDefaultSettings { get; set; }

        [DataMember]
        public Dictionary<string, List<ScheduleDto>> ScheduleSchemes { get; set; }

        [DataMember]
        public List<ServiceGroupDto> AgentGroups { get; set; }

        [DataMember]
        public List<ProfileDto> NotificationProfiles { get; set; }

        [DataMember]
        public List<NameAndIdDto> SecurityProfiles { get; set; }

        [DataMember]
        public List<NameAndIdDto> PlanGroups { get; set; }

        /// <summary> 标示用户是否配置了Email service setting. </summary>
        [DataMember]
        public bool IsEnableNotificationSetting { get; set; }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ItemBackupRequsetType
    {
        [EnumMember]
        StoragePolicy = 1,

        [EnumMember]
        FilterPolicy = 2,

        [EnumMember]
        QuickBackupDefaultSettings = 4,

        [EnumMember]
        ScheduleScheme = 8,

        [EnumMember]
        AgentGroup = 16,

        [EnumMember]
        NotificationSetting = 32,

        [EnumMember]
        SecurityProfile = 64,

        [EnumMember]
        PlanGroup = 128,

        [EnumMember]
        NotificationProfile = 256,
    }
}
