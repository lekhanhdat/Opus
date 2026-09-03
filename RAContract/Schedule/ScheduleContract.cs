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
using ProtoBuf.Meta;
using System.Runtime.Serialization;

/// <summary>
/// have reviewed by allen yin
/// </summary>
namespace AvePoint.RA.Contract.Schedule
{
    public enum IntervalType :int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Weekly = 1,
        [EnumMember]
        Daily = 2,
        [EnumMember]
        Hourly = 3,
        [EnumMember]
        Monthly = 4,
    }
    [DataContract]
    public enum ScheduleType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SyncSchedule = 1,
        [EnumMember]
        SharePointSettingSchedule = 2,
        [EnumMember]
        LocationSyncSchedule = 3,
        [EnumMember]
        UpdateRecordLocationSchedule = 4,
        [EnumMember]
        ManualApprovalSchedule = 5,
        [EnumMember]
        DisposalSchedule = 6,
        [EnumMember]
        UniqueIDSettingSchedule = 7,
        [EnumMember]
        CollectionDataSchedule = 8,
        [EnumMember]
        ManualApprovalScheduleTimer = 9,
        [EnumMember]
        ColletionDataSchedule = 10,
        [EnumMember]
        EnforceRetention = 13,
        [EnumMember]
        EXODisposalSchedule = 14,
        [EnumMember]
        SPSyncDataSchedule = 15,
        [EnumMember]
        EXOSyncDataSchedule = 16,
        [EnumMember]
        EXOApplypSchedule = 17,
        [EnumMember]
        EXOEnforceRetention = 18,
        [EnumMember]
        PRDisposalSchedule = 19,
        [EnumMember]
        PRExplorerTimer = 20,
        [EnumMember]
        FSColletionDataSchedule = 21,
        [EnumMember]
        FSDisposalSchedule = 22,
        [EnumMember]
        SyncSecurityContainerSchedule = 23,
        [EnumMember]
        SPOnPremScanNodesSchedule = 24,
        [EnumMember]
        SPOnPremDisposalSchedule = 25,
        [EnumMember]
        SPOnPremApplySettingSchedule = 26,
        [EnumMember]
        SPOnPremDataSyncSchedule = 27,
        [EnumMember]
        SPOnPremUniqueIDSettingSchedule = 28,
        [EnumMember]
        OneDriveSyncDataSchedule = 29,
        [EnumMember]
        OneDriveDisposalSchedule = 30,
        [EnumMember]
        OneDriveEnforceRetention = 31,
        [EnumMember]
        Dashboard = 32,
        [EnumMember]
        ManualApprovalEmailSchedule = 33,
        [EnumMember]
        AzureFileShareDataSyncSchedule = 34,
        [EnumMember]
        ConnectorExplorerTimer = 35,
        [EnumMember]
        Placeholder = 36,//RECO-17842
        [EnumMember]
        SPArchiveJobSchedule = 37,
        [EnumMember]
        OneDriveArchiveJobSchedule = 38,
        [EnumMember]
        ArchiveDataRetentionSchedule = 39,
        [EnumMember]
        MoveDataTierSchedule = 40,
        [EnumMember]
        RebuildStubSchedule = 41,
        [EnumMember]
        ArchiveFullTextIndex = 42,
        [EnumMember]
        RebuildIndexSchedule = 43,
        [EnumMember]
        BoxDisposalSchedule = 44,
        [EnumMember]
        BoxDataSyncSchedule = 45,
		[EnumMember]
        JobNotificationSchedule = 46,
        [EnumMember]
        AdjustSizeSchedule = 47,
        [EnumMember]
        ArchiverDeleteRestoredDataSchedule = 48,
        [EnumMember]
        ApprovalProcessJob = 49,
        [EnumMember]
        ArchiverDedupJobSchedule = 50,
        [EnumMember]
        JobMonitorArchiveSchedule = 51,
        [EnumMember]
        HoldNotificationSchedule = 52,

        //Google
        [EnumMember]
        GoogleDataSyncSchedule = 60,
        [EnumMember]
        GoogleSettingSchedule = 61,
        [EnumMember]
        GoogleDisposalSchedule = 62,     
        [EnumMember]
        GoogleArchiveJobSchedule = 63,


        // Teams
        [EnumMember]
        TeamsArchiveJobSchedule = 70,
        [EnumMember]
        TeamsDisposalSchedule = 71,
        [EnumMember]
        TeamsSyncDataSchedule = 72,
        [EnumMember]
        TeamsSettingSchedule = 73,
        [EnumMember]
        TeamsEnforceRetention = 74,
        [EnumMember]
        TeamsUniqueIDSettingSchedule = 75,

        [EnumMember]
        StubDisposalSchedule = 80,
        [EnumMember]
        APStorageCostEvaluationSchedule = 81,

        //Discovery
        [EnumMember]
        DiscoveryPlanSchedule = 82,
        [EnumMember]
        ContentDueForAction = 83,
        [EnumMember]
        SPOActionAuditReport = 84,
        [EnumMember]
        RestoreReport = 85
        ,
        [EnumMember]
        ArchivedSiteReport = 86

    }
    [DataContract]
    public enum EndType:int
    {
        [EnumMember]
        NoEnd = 0,
        [EnumMember]
        EndByTime = 1,
        [EnumMember]
        EndByOccurrences = 2
    }
}
