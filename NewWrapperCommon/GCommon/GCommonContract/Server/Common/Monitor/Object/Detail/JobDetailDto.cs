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
using System.Collections.Generic;
using System;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    //[KnownType(typeof(ItemJobDetailDto))]
    //[KnownType(typeof(ReportCenterJobDetailDto))]
    //[KnownType(typeof(ReplicatorJobDetailDto))]
    //[KnownType(typeof(ReplicatorImportPlanJobDetailDto))]
    //[KnownType(typeof(ContentManagerJobDetailDto))]
    //[KnownType(typeof(SOJobDetailDto))]
    //[KnownType(typeof(CentralAdminJobDetailDto))]
    //[KnownType(typeof(PlatformBackupJobDetailDto))]
    //[KnownType(typeof(JobPruningJobDetailDto))]
    //[KnownType(typeof(DeploymentManagerJobDetailDto))]
    //[KnownType(typeof(PlatformMaintenanceJobDetailDto))]
    //[KnownType(typeof(PlatformFarmRebuildJobDetailDto))]
    //[KnownType(typeof(PlatformMigrationJobDetailDto))]
    //[KnownType(typeof(PlatformRestoreJobDetailDto))]
    //[KnownType(typeof(RetentionJobDetailDto))]
    //[KnownType(typeof(MigrationJobDetailDto))]
    //[KnownType(typeof(SPMigrationJobDetailDto))]
    //[KnownType(typeof(BackupDataEIJobDetailDto))]
    //[KnownType(typeof(ArchiverDataEIJobDetailDto))]
    //[KnownType(typeof(EDJobDetailDto))]
    //[KnownType(typeof(EDSyncJobDetailDto))]
    //[KnownType(typeof(EDSearchJobDetailDto))]
    //[KnownType(typeof(ConnectorJobDetailDto))]
    //[KnownType(typeof(ConnectorInventoryJobDetailDto))]
    //[KnownType(typeof(VaultJobDetailDto))]
    //[KnownType(typeof(SOTestRunJobDetailDto))]
    //[KnownType(typeof(SRMAnalyzeSqlBackupJobDetailDto))]
    //[KnownType(typeof(SRMRestoreFromSQLJobDetailDto))]
    //[KnownType(typeof(HASyncJobDetailDto))]
    //[KnownType(typeof(HAFailoverJobDetailDto))]
    //[KnownType(typeof(HAFallbackJobDetailDto))]
    //[KnownType(typeof(CollectReportJobDetailDto))]
    //[KnownType(typeof(HAPreSanJobDetailDto))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
    public class JobDetailDto
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public long Date { get; set; }

        /// <summary>
        /// 前台转换时间所用。
        /// 标明job default Timezone
        /// </summary>
        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// 用来标识URL object name，这样可以方便用户快速定位object
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string SrcURL { get; set; }

        [DataMember]
        public string DestURL { get; set; }

        [DataMember]
        public string SrcAgentHost { get; set; }

        [DataMember]
        public string DestAgentHost { get; set; }

        [DataMember]
        public string MediaHost { get; set; }

        [DataMember]
        public string PhysicalDevice { get; set; }

        [DataMember]
        public string Size { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public string Option { get; set; }
    }
}
