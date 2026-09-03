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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexParamDto
    {
        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String BackupPlanId { get; set; }

        [DataMember]
        public String BackupJobId { get; set; }

        [DataMember]
        public String BackupCycleId { get; set; }

        /// <summary>
        /// archiver: 从SiteUrls中取得一个SiteUrl.
        /// </summary>
        [DataMember]
        public String SiteUrl { get; set; }

        [DataMember]
        public String WebAppUrl { get; set; }

        /// <summary>
        /// backup and archiver都需要传递哪些备份的site collection
        /// </summary>
        [DataMember]
        public List<String> SiteUrls { get; set; }

        [DataMember]
        public Int32 SPVersion { get; set; }

        /// <summary>
        /// backup: 对应index和data的LogicalDevice
        /// archiver: 对应index的LogicalDevice
        /// </summary>
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        /// <summary>
        /// archiver: data的LogicalDevice
        /// </summary>
        [DataMember]
        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }

        [DataMember]
        public CacheSettingDto CacheLocation { get; set; }

        [DataMember]
        public LogicalDeviceDto SearchDevice { get; set; }

        [DataMember]
        public FullTextIndexJobType JobType { get; set; }

        [DataMember]
        public ProcessType ExecuteType { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        [DataMember]
        public List<ServiceDto> DataMediaInfos { get; set; }

        [DataMember]
        public FullTextIndexSettingDto SettingDto { get; set; }

        [DataMember]
        public FullTextIndexJobPolicy JobPolicy { get; set; }

        public override String ToString()
        {
            return String.Format("FarmName[{0}], JobId[{1}], BackupPlanId[{2}], BackupCycleId[{3}], ProcessType[{4}], JobType[{5}]", FarmName, BackupJobId, BackupPlanId, BackupCycleId, ExecuteType, JobType);
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProcessType
    {
        [EnumMember]
        NEW_PROCESS,
        [EnumMember]
        APP_DOMAIN_PROCESS
    }

}
