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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry
{
    [DataContract]
    public enum TelemetryEventType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        HomepageLoaded = 1,
        [EnumMember]
        ContentPageLoaded = 2,
        [EnumMember]
        ApplySettings = 3,
        [EnumMember]
        RunEnforceRuleActions = 4,
        [EnumMember]
        Search = 5,
        [EnumMember]
        Filter = 6,
        [EnumMember]
        DashboardLoaded = 7,
        [EnumMember]
        CreateContentDueProfile = 8,
        [EnumMember]
        CreateCreationAndDestructionProfile = 9,
        [EnumMember]
        ViewAuditReport = 10,
        [EnumMember]
        RunJob = 11,
        [EnumMember]
        TermSynchronise = 12,
        [EnumMember]
        LoanRequest = 13,
        [EnumMember]
        RecordCreationRequest = 14,
        [EnumMember]
        BoxCreationRequest = 15,
        [EnumMember]
        FolderCreationRequest = 16,
        [EnumMember]
        RuleAdded = 17,
        [EnumMember]
        RuleModified = 18,
        [EnumMember]
        RuleDeleted = 19,
        [EnumMember]
        ActionAuditProfile = 20,
        [EnumMember]
        MonitorFailedJob = 21,
        [EnumMember]
        MonitorLongRunningJob = 22,
        [EnumMember]
        MonitorSpecificExceptionJob = 23,
        [EnumMember]
        MonitorAgentStatus = 24,
        [EnumMember]
        SOCustomer = 25,
        [EnumMember]
        MigrationJob = 26,
        [EnumMember]
        LabelSyncToGoogle = 27,
        [EnumMember]
        LabelSyncFromGoogle = 28,
        [EnumMember]
        DBUpgradeInfo = 29,
        [EnumMember]
        PermissionSyncFailedInfo = 30,
        [EnumMember]
        DeleteRestoredAvePointBlockData = 31,
        [EnumMember]
        ExportCsvFile = 31,
        [EnumMember]
        DiscoveryAndAnalysisEachJobInfo = 32,
        [EnumMember]
        DiscoveryAndAnalysisEachProfileJobInfo = 33
    }
}
