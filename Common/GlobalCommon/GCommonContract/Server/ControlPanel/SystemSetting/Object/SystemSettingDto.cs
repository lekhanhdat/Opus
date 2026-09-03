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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object
{
    [KnownType(typeof(SystemSettingContent))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SystemSettingDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Extension { set; get; }

        [DataMember]
        public SystemSettingType Type { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public byte[] BinaryData { get; set; }

        [DataMember]
        public ISystemSettingContent Content { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SystemSettingType : int
    {
        [EnumMember]
        MOM = 0,

        //[EnumMember]
        //SCOM = 1,

        //[EnumMember]
        //JobReportLocation = 2,

        [EnumMember]
        NotificationSetting = 3,

        [EnumMember]
        SecuritySetting = 4,

        [EnumMember]
        PassphraseInfo = 5,

        //[EnumMember]
        //JobPruning = 6,

        [EnumMember]
        SystemSetting = 7,

        [EnumMember]
        LogManager = 8,

        //[EnumMember]
        //SingleSignOn = 9,

        //[EnumMember]
        //AuthenticationManager = 10,

        //[EnumMember]
        //WindowsAuthentication = 11,

        //[EnumMember]
        //UpdateManager = 12,

        //[EnumMember]
        //SystemLicense = 13,

        [EnumMember]
        CommunicationEncryptionKey = 14,

        //[EnumMember]
        //DataEncryptionProfile = 15,

        //[EnumMember]
        //LanguageTranslation = 16,

        //[EnumMember]
        //RunOncePatch = 17,

        //[EnumMember]
        //ASUPSetting = 18,

        //[EnumMember]
        //RunOnceStamp = 19,

        //[EnumMember]
        //PatchInstallerMark = 20,

        [EnumMember]
        FipsAlgorithmPolicy = 21,

        [EnumMember]
        InitDefaultDataMark = 22,

        //[EnumMember]
        //CEIPSetting = 25,

        //[EnumMember]
        //RecycleDBSetting = 26,

        [EnumMember]
        JobParallelSetting = 27,

        /// <summary>
        /// 注册Tenant时，为Tenant分配的Tenant DB的Default Size Quota
        /// </summary>
        [EnumMember]
        TenantDBDefaultQuota = 28,

        /// <summary>
        /// 当前使用的用于承载Tenant DB的Server Instance
        /// </summary>
        [EnumMember]
        CurrentTenantDBInstanse = 29,

        [EnumMember]
        AzureStorageCredential = 30,

        [EnumMember]
        CAPEAccountFilterSetting = 31,

        [EnumMember]
        MasterKey = 32,

        [EnumMember]
        ConfigSetting = 33,

        [EnumMember]
        StorageTableConfigurationSetting = 34,

        [EnumMember]
        ArchiverDatabase = 35,

        [EnumMember]
        PlanCalculateTimestamp = 36,

        [EnumMember]
        GlobalDefaultDevice = 37,

        [EnumMember]
        ReplicatorRealTimeTableConfigSetting = 38,

        [EnumMember]
        SystemSecret = 39,

        [EnumMember]
        SPOItemOrFileVerionLimitSetting = 40,
    }
}
