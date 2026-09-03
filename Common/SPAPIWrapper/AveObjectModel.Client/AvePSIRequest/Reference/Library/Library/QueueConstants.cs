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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Microsoft.Office.Project.Server.Library
{
	// Token: 0x02000E70 RID: 3696
	public class QueueConstants
	{
		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000F41 RID: 3905 RVA: 0x00058AA4 File Offset: 0x00056CA4
		// (set) Token: 0x06000F42 RID: 3906 RVA: 0x00058AAB File Offset: 0x00056CAB
		internal static bool CurrentlyRunningInQueue { get; set; }

		// Token: 0x06000F43 RID: 3907 RVA: 0x00058AB4 File Offset: 0x00056CB4
		static QueueConstants()
		{
			QueueConstants.InitializeQueueMsgTable();
			QueueConstants.InitializeJobStateMap();
			QueueConstants.InitializeGroupStateMap();
			Array values = Enum.GetValues(typeof(QueueConstants.QueueMsgType));
			QueueConstants.MaxValueQueueMsgType = (QueueConstants.QueueMsgType)values.GetValue(values.Length - 1);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00058BD0 File Offset: 0x00056DD0
		public static IEnumerable<QueueConstants.JobState> InvalidJobStates()
		{
			yield return QueueConstants.JobState.Unknown;
			yield return QueueConstants.JobState.LastState;
			yield break;
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x00058BE6 File Offset: 0x00056DE6
		public static IEnumerable<QueueConstants.JobState> ValidJobStates()
		{
			return Enum.GetValues(typeof(QueueConstants.JobState)).Cast<QueueConstants.JobState>().Except(QueueConstants.InvalidJobStates());
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x00058D18 File Offset: 0x00056F18
		public static IEnumerable<QueueConstants.JobState> CompletedJobStates()
		{
			yield return QueueConstants.JobState.Success;
			yield return QueueConstants.JobState.Failed;
			yield return QueueConstants.JobState.FailedNotBlocking;
			yield return QueueConstants.JobState.Canceled;
			yield break;
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x00058D2E File Offset: 0x00056F2E
		public static IEnumerable<QueueConstants.JobState> PendingJobStates()
		{
			return QueueConstants.ValidJobStates().Except(QueueConstants.CompletedJobStates());
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x00058D40 File Offset: 0x00056F40
		private static void InitializeGroupStateMap()
		{
			QueueConstants.GroupStateMap = new Hashtable();
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Unknown, QueueConstants.JobState.Unknown);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Unlocked, QueueConstants.JobState.ReadyForProcessing);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.LockedForSending, QueueConstants.JobState.SendIncomplete);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.LockedForReceiving, QueueConstants.JobState.Processing);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.CompletedSuccess, QueueConstants.JobState.Success);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.CompletedFailure, QueueConstants.JobState.Failed);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.CompletedFailureNotBlocking, QueueConstants.JobState.FailedNotBlocking);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Skipped, QueueConstants.JobState.ProcessingDeferred);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Blocked, QueueConstants.JobState.CorrelationBlocked);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Canceled, QueueConstants.JobState.Canceled);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.OnHold, QueueConstants.JobState.OnHold);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.Sleeping, QueueConstants.JobState.Sleeping);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.ReadyForLaunch, QueueConstants.JobState.ReadyForLaunch);
			QueueConstants.GroupStateMap.Add(QueueConstants.GroupState.LastState, QueueConstants.JobState.LastState);
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x00058E98 File Offset: 0x00057098
		private static void InitializeJobStateMap()
		{
			QueueConstants.JobStateMap = new Hashtable();
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Unknown, QueueConstants.GroupState.Unknown);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.ReadyForProcessing, QueueConstants.GroupState.Unlocked);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.SendIncomplete, QueueConstants.GroupState.LockedForSending);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Processing, QueueConstants.GroupState.LockedForReceiving);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Success, QueueConstants.GroupState.CompletedSuccess);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Failed, QueueConstants.GroupState.CompletedFailure);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.FailedNotBlocking, QueueConstants.GroupState.CompletedFailureNotBlocking);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.ProcessingDeferred, QueueConstants.GroupState.Skipped);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.CorrelationBlocked, QueueConstants.GroupState.Blocked);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Canceled, QueueConstants.GroupState.Canceled);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.OnHold, QueueConstants.GroupState.OnHold);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.Sleeping, QueueConstants.GroupState.Sleeping);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.ReadyForLaunch, QueueConstants.GroupState.ReadyForLaunch);
			QueueConstants.JobStateMap.Add(QueueConstants.JobState.LastState, QueueConstants.GroupState.LastState);
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x00058FF0 File Offset: 0x000571F0
		private static void InitializeQueueMsgTable()
		{
			QueueConstants.QueueMsgTable = new Hashtable();
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ACProjectSave, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "WinProj Save", "Save Project from WinProj"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AdSyncERP, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "ADSync Synchronization Message", "ADSync Synchronization Message."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AdSyncGroup, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "ADSync Synchronization Message", "ADSync Synchronization Message."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveCategories, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive Categories", "Archive Categories to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveCustomFields, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive Custom Fields", "Archive Custom Fields to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveGlobalProject, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive the global project", "Archive the global project in the versions database"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveResources, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive Resources", "Archive Resources to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveSystemSettings, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive System Settings", "Archive System Settings to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ArchiveViews, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Archive Views", "Archive Views to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.BumpPriority, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Priority Bump", "Priority Bump"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.CBSProjRendezvous, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.ProjectQ, "CBS ProjectQ Rendezvous", "Rendezvous message for CBS to block RDS messages in the project queue."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.CBSRequest, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.ProjectQ, "Cube Build", "Build a Cube"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.CBSTsRendezvous, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.TimesheetQ, "CBS TimesheetQ Rendezvous", "Rendezvous message for CBS to block RDS messages in the timesheet queue."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.CreateProposalProject, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Create Proposal Project - Obsolete", "Creates a Proposal project - Obsolete"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.CreateWssSite, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Create Wss Site", "Creates a WssSite for a project after publish"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.DeleteWssSite, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Delete Wss Site", "Deletes a WssSite for a project"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportEptSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Sync Enterprise Project Type Information", "Sync enterprise project type information in Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.LWPUpgradeToProject, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Upgrade Light Weight Project to a Regular Project - Obsolete", "Upgrades a light weight project to a regular project - Obsolete"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.NotificationMessage, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.TimesheetQ, "Notification", "General Notifications"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectArchive, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Archive", "Archive a Project to the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectArchiveRetentionDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Archive Retention Delete", "Delete a Project from the Versions DB according to the retention policy"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectCheckIn, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "SSP Project Check-In", "Check-in a Project via Server Side Projects"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectCreate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "SSP Project Create", "Create a Project via Server Side Projects"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Delete", "Delete a Project"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectPublish, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Publish", "Publish Project from WinProj, PWA, or SSP"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectPublishSummary, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Summary Publish", "Publish Project Summary"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectRename, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Rename Project", "Renames a project"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectRestore, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Restore", "Restore a Project from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectReversePublish, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Reverse Publish Project - Obsolete", "Copies project data from published DB to working DB - Obsolete"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectUpdate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "SSP Project Update", "Update a Project via Server Side Projects"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectUpdateTeam, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "SSP Update Team", "Create a Project Team via Server Side Projects"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PublishNotifications, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.ProjectQ, "Notifications for Publishing", "Notifications sent when a Project is Published"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportWorkflowProjectDataSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Sync Project workflow information", "Sync project workflow information in Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.QueueCleanup, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Queue Cleanup", "Queue Cleanup"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingAttributeCubeSettingsSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Cube Configuration Settings", "Transfer data to the Reporting DB when Cube Configuration Settings for a custom field are created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingBaseCalendarSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Base Calendar Sync", "Refresh resource capacity data in the Reporting DB when a Base Calendar is created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingCustomFieldMetadataSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Custom Field Sync", "Transfer data to the Reporting DB when a Custom Field is created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingEntityUserViewRefresh, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - User View Refresh", "Refresh the user view for a given entity (project, task, etc) after a Custom Field is created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingFiscalPeriodsSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Fiscal Periods Transfer", "Transfer date to the Reporting DB when fiscal periods are changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingLookupTableSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Lookup Table Sync", "Transfer data to the Reporting DB when a Lookup Table is created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingProjectDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Project Delete", "Transfer data to the Reporting DB when a Project is deleted"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingProjectPublish, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Project Publishing", "Transfer data to the Reporting DB when a Project is published"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingSummaryPublish, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Project Summary Publishing", "Transfer some of the project data to the Reporting DB when it is saved"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingResourcesCapacityRangeSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Resource Capacity Range", "Refresh resource capacity data in the Reporting DB when the Reporting Resource Capacity Time Range is changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingResourceSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Resource Sync", "Transfer data to the Reporting DB when a Resource is created, deleted or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetAdjust, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Adjust", "Transfer data to the Reporting DB when a Timesheet is adjusted"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetClassSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Class Sync", "Transfer data to the Reporting DB when a Timesheet Class is created or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Delete", "Propagate the Timesheet delete operation to the Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetPeriodDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Period Delete", "Propagate the Timesheet Period delete operation to the Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetPeriodSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Period Sync", "Transfer data to the Reporting DB when a Timesheet Period is created or changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetSave, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Save", "Transfer data to the Reporting DB when a Timesheet is saved"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetStatusSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Status Sync", "Transfer data to the Reporting DB when a Timesheet Status is changed"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetProjectAggregation, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Project Aggregation", "Aggregate task level data in the Timesheet Project in Reporting database."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetAssignmentsUpgrade, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumHighPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Assignments Upgrade", "Used to transfer/fix timesheet assignment data during upgrade."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingWSSSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - sync WSS data", "Sync WSS data in Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingWorkflowMetadataSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Sync workflow metadata", "Sync workflow metadata in Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingSolutionCommitedDecisionSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting - Sync Commited Decisions", "Sync commited decisions in Reporting DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ResourcePlanCheckIn, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "ResourcePlan CheckIn", "ResourcePlan CheckIn."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ResourcePlanDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "ResourcePlan Delete", "ResourcePlan Delete."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ResourcePlanPublish, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Resource Plan Publish", "Publishes a resource plan"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ResourcePlanSave, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "ResourcePlan Save", "ResourcePlan Save."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreCategories, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Restore Categories", "Restore Categories from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreCustomFields, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Restore Custom Fields", "Restore Custom Fields from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreGlobalProject, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.TimesheetQ, "Restore the global project", "Restore the global project from the versions database"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreResources, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Restore Resources", "Restore Resources from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreSystemSettings, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Restore System Settings", "Restore System Settings from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RestoreViews, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Restore Views", "Restore Views from the Versions DB"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RulesProcessAll, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Rules - Process All", "Process all rules"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RulesProcessAllAutoForManager, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Rules - AutoProcess All For Manager", "Auto-process all rules for a particular manager"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RulesProcessAllForManager, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Rules - Process All For Manager", "Process all rules for a particular manager"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.RulesProcessSingleForManager, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Rules - Process Single For Manager", "Process a single rule for a manager"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.StatusApproval, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Status Approval", "Reported Actuals have been approved"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer1, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer10, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer2, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer3, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer4, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer5, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer6, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer7, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer8, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.Timer9, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimerMessage, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimerRzNotify, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimerRzProj, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimerRzTS, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "queue internal use", "queue internal use"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Delete", "Delete timesheets"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetMessage, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet", "Timesheet data"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetRecall, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Recall", "Recall a timesheet."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetReview, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Review", "Review a timesheet."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetSubmit, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Submit", "Submit a timesheet."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetUpdate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Update", "Update a timesheet."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetLineApproval, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Timesheet Line Approval", "Approvel a Project Timesheet Line."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingSyncGlobalData, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Synchronize RDB Global Data", "Synchronize global data with reporting databse."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.SynchronizeMembershipForWssSite, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Synchronize membership for a Wss workspace", "Synchronize user membership to a Wss project workspace"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.SynchronizeSingleUserMembershipInWss, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Synchronize single user in Wss", "Synchronize user membership to a Pwa/Wss root site and all the project workspaces"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingRefresh, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Reporting Database Refresh", "Refreshes all or specific areas in the reporting database"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.UpdateScheduledProject, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Changelist Project Update", "Applies new changes in sequence, schedules the project and then saves the scheduled project"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.WorkflowStartWorkflow, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Create", "Starts a new workflow for a new project based on the workflow association"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AnalysisDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Analysis Delete", "Analysis Delete."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AnalysisCreate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Analysis Create", "Analysis Create."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AnalysisUpdate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Analysis Create", "Analysis Update."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PlannerSolutionCreate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Create planner solution", "Creates a new planner solution."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PlannerSolutionDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Delete planner solution", "Deletes a planner solution."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.OptimizerSolutionCreate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Create Optimizer Solution", "Creates an Optimizer Solution"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.OptimizerSolutionDelete, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Delete Optimizer Solution", "Deletes an Optimizer Solution"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PeriodicTasks, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LowPriority, QueueConstants.QueueID.ProjectQ, "PeriodicTasks", "Hook for external periodic tasks"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PDPUpdateProjectImpacts, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Update strategic impact values", "Updates Project Detail Pages project to drivers strategic impact values."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ExchangeSyncTasks, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.LastPriority, QueueConstants.QueueID.ProjectQ, "Exchange Tasks Sync", "Sync's tasks between Project Server and Exchange Server"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.WorkflowCheckinNotify, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Checkin Notification for Workflow", "Notify the workflow that the project has been checked in"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.WorkflowChangeWorkflow, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Workflow Admin", "Changes the running workflow for an existing project"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.WorkflowCommitNotify, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Commit Notification for Workflow", "Notify the workflow that the project has been commited in the optimizer or planner"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ReportingTimesheetAssignmentsRefresh, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumHighPriority, QueueConstants.QueueID.TimesheetQ, "Reporting - Timesheet Assignments Refresh", "Used to transfer/fix timesheet assignment data during Epm refresh."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.AddSingleUserMembershipInWss, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Add single user in Wss", "Adds single user membership to a Pwa/Wss root site and all the project workspaces"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.DeleteSingleUserMembershipInWss, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Delete single user in Wss", "Deletes single user membership to a Pwa/Wss root site and all the project workspaces"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimeSheetUpdateResourceNonWorkingTime, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Update Resource Non Working Time", "Asynchronous update for the non working time of a resource"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.SyncProjectEnterpriseEntities, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Sync Project Enterprise Entities", "Sync the enterprise entities properties that the project used, schedules the project and saves it"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ExchangeCalOofSync, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Exchange Calendar Out Of Office Sync", "Sync user's Exchange Server Calendar Out Of Office time."));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PreparePSPermissionSynchronization, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Prepare ProjectServer Permissions Synchronization", "Prepares synchronization information for synchronization permissions from Project Server to SharePoint"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PSPermissionSynchronizePWASite, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Synchronize ProjectServer Permissions to Project Web App", "Synchronizes global permissions to Project Web App"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PSPermissionSynchronizeProjectSite, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Synchronize ProjectServer Permissions to Project Site", "Synchronizes category permissions to the respective Project Site"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.PreparePSProjectPermissionSynchronization, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Prepare ProjectServer Permissions Synchronization For Projects", "Prepares synchronization information for synchronization projects permissions from Project Server to SharePoint"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ScheduleWebPartSave, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Project Save from Schedule Webpart", "Project Save from Schedule Webpart"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ProjectImportTaskList, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.MediumLowPriority, QueueConstants.QueueID.ProjectQ, "Import a Task List as a Project", "Imports a task list from SharePoint into Project Server"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.TimesheetUpdateSRAForResource, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.TimesheetQ, "Updates Timesheet SRAs", "Updates the SRA for a resource with his/her Timesheet Data"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ActiveMonitorCheck, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Queue Active Monitor Check", "Empty message used to probe the health of the queue system"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ManagedModeTaskSynchronization, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Managed Mode Task Synchronization for Project Publish", "Synchronizes tasks to a SharePoint List"));
			QueueConstants.QueueMsgTable.Add(QueueConstants.QueueMsgType.ResourcePlanMigrate, new QueueConstants.QueueMsgInfo(QueueConstants.QueueMsgPriority.StandardPriority, QueueConstants.QueueID.ProjectQ, "Migrate Resource Plan", "Migrates the resource plan for a project"));
		}

		// Token: 0x040044A1 RID: 17569
		internal const int LockNextGroupSqlCommandTimeout = 90;

		// Token: 0x040044A2 RID: 17570
		public const int MaxConcurrentServerCount = 4;

		// Token: 0x040044A3 RID: 17571
		public static Hashtable QueueMsgTable;

		// Token: 0x040044A4 RID: 17572
		public static Hashtable GroupStateMap;

		// Token: 0x040044A5 RID: 17573
		public static Hashtable JobStateMap;

		// Token: 0x040044A6 RID: 17574
		internal static readonly QueueConstants.QueueMsgType MaxValueQueueMsgType;

		// Token: 0x02000E71 RID: 3697
		public struct QueueMsgInfo
		{
			// Token: 0x06000F4C RID: 3916 RVA: 0x0005A396 File Offset: 0x00058596
			public QueueMsgInfo(QueueConstants.QueueMsgPriority priority, QueueConstants.QueueID queueID, string friendlyName, string description)
			{
				this.MsgPriority = priority;
				this.MsgQueueID = queueID;
				this.MsgFriendlyName = friendlyName;
				this.MsgDescription = description;
			}

			// Token: 0x040044A8 RID: 17576
			public QueueConstants.QueueMsgPriority MsgPriority;

			// Token: 0x040044A9 RID: 17577
			public QueueConstants.QueueID MsgQueueID;

			// Token: 0x040044AA RID: 17578
			public string MsgFriendlyName;

			// Token: 0x040044AB RID: 17579CLS-compliant
			public string MsgDescription;
		}

		// Token: 0x02000E72 RID: 3698
		[ClientCallableType(ServerTypeId = "36D65B01-45D6-4A7D-A9EC-74C9671D2E65")]
		public enum QueueMsgType
		{
			// Token: 0x040044AD RID: 17581
			Unknown,
			// Token: 0x040044AE RID: 17582
			ACProjectSave,
			// Token: 0x040044AF RID: 17583
			AdSyncERP,
			// Token: 0x040044B0 RID: 17584
			AdSyncGroup,
			// Token: 0x040044B1 RID: 17585
			ArchiveCategories,
			// Token: 0x040044B2 RID: 17586
			ArchiveCustomFields,
			// Token: 0x040044B3 RID: 17587
			ArchiveGlobalProject,
			// Token: 0x040044B4 RID: 17588
			ArchiveResources,
			// Token: 0x040044B5 RID: 17589
			[PSObsolete]
			ArchiveSystemSettings,
			// Token: 0x040044B6 RID: 17590
			ArchiveViews,
			// Token: 0x040044B7 RID: 17591
			BumpPriority,
			// Token: 0x040044B8 RID: 17592
			CBSProjRendezvous,
			// Token: 0x040044B9 RID: 17593
			CBSRequest,
			// Token: 0x040044BA RID: 17594
			CBSTsRendezvous,
			// Token: 0x040044BB RID: 17595
			CreateProposalProject,
			// Token: 0x040044BC RID: 17596
			CreateWssSite,
			// Token: 0x040044BD RID: 17597
			DeleteWssSite,
			// Token: 0x040044BE RID: 17598
			LWPUpgradeToProject,
			// Token: 0x040044BF RID: 17599
			NotificationMessage,
			// Token: 0x040044C0 RID: 17600
			ProjectArchive,
			// Token: 0x040044C1 RID: 17601
			ProjectArchiveRetentionDelete,
			// Token: 0x040044C2 RID: 17602
			ProjectCheckIn,
			// Token: 0x040044C3 RID: 17603
			ProjectCreate,
			// Token: 0x040044C4 RID: 17604
			ProjectDelete,
			// Token: 0x040044C5 RID: 17605
			ProjectPublish,
			// Token: 0x040044C6 RID: 17606
			ProjectRename,
			// Token: 0x040044C7 RID: 17607
			ProjectRestore,
			// Token: 0x040044C8 RID: 17608
			[PSObsolete]
			ProjectReversePublish,
			// Token: 0x040044C9 RID: 17609
			ProjectUpdate,
			// Token: 0x040044CA RID: 17610
			ProjectUpdateTeam,
			// Token: 0x040044CB RID: 17611
			PublishNotifications,
			// Token: 0x040044CC RID: 17612
			QueueCleanup,
			// Token: 0x040044CD RID: 17613
			ReportingAttributeCubeSettingsSync,
			// Token: 0x040044CE RID: 17614
			ReportingBaseCalendarSync,
			// Token: 0x040044CF RID: 17615
			ReportingCustomFieldMetadataSync,
			// Token: 0x040044D0 RID: 17616
			ReportingEntityUserViewRefresh,
			// Token: 0x040044D1 RID: 17617
			ReportingFiscalPeriodsSync,
			// Token: 0x040044D2 RID: 17618
			ReportingLookupTableSync,
			// Token: 0x040044D3 RID: 17619
			ReportingProjectDelete,
			// Token: 0x040044D4 RID: 17620
			ReportingProjectPublish,
			// Token: 0x040044D5 RID: 17621
			ReportingResourcesCapacityRangeSync,
			// Token: 0x040044D6 RID: 17622
			ReportingResourceSync,
			// Token: 0x040044D7 RID: 17623
			ReportingTimesheetAdjust,
			// Token: 0x040044D8 RID: 17624
			ReportingTimesheetClassSync,
			// Token: 0x040044D9 RID: 17625
			ReportingTimesheetDelete,
			// Token: 0x040044DA RID: 17626
			ReportingTimesheetPeriodDelete,
			// Token: 0x040044DB RID: 17627
			ReportingTimesheetPeriodSync,
			// Token: 0x040044DC RID: 17628
			ReportingTimesheetSave,
			// Token: 0x040044DD RID: 17629
			ReportingTimesheetStatusSync,
			// Token: 0x040044DE RID: 17630
			ReportingWSSSync,
			// Token: 0x040044DF RID: 17631
			ResourcePlanCheckIn,
			// Token: 0x040044E0 RID: 17632
			ResourcePlanDelete,
			// Token: 0x040044E1 RID: 17633
			ResourcePlanPublish,
			// Token: 0x040044E2 RID: 17634
			ResourcePlanSave,
			// Token: 0x040044E3 RID: 17635
			RestoreCategories,
			// Token: 0x040044E4 RID: 17636
			RestoreCustomFields,
			// Token: 0x040044E5 RID: 17637
			RestoreGlobalProject,
			// Token: 0x040044E6 RID: 17638
			RestoreResources,
			// Token: 0x040044E7 RID: 17639
			[PSObsolete]
			RestoreSystemSettings,
			// Token: 0x040044E8 RID: 17640
			RestoreViews,
			// Token: 0x040044E9 RID: 17641
			RulesProcessAll,
			// Token: 0x040044EA RID: 17642
			RulesProcessAllAutoForManager,
			// Token: 0x040044EB RID: 17643
			RulesProcessAllForManager,
			// Token: 0x040044EC RID: 17644
			RulesProcessSingleForManager,
			// Token: 0x040044ED RID: 17645
			StatusApproval,
			// Token: 0x040044EE RID: 17646
			Timer1,
			// Token: 0x040044EF RID: 17647
			Timer10,
			// Token: 0x040044F0 RID: 17648
			Timer2,
			// Token: 0x040044F1 RID: 17649
			Timer3,
			// Token: 0x040044F2 RID: 17650
			Timer4,
			// Token: 0x040044F3 RID: 17651
			Timer5,
			// Token: 0x040044F4 RID: 17652
			Timer6,
			// Token: 0x040044F5 RID: 17653
			Timer7,
			// Token: 0x040044F6 RID: 17654
			Timer8,
			// Token: 0x040044F7 RID: 17655
			Timer9,
			// Token: 0x040044F8 RID: 17656
			TimerMessage,
			// Token: 0x040044F9 RID: 17657
			TimerRzNotify,
			// Token: 0x040044FA RID: 17658
			TimerRzProj,
			// Token: 0x040044FB RID: 17659
			TimerRzTS,
			// Token: 0x040044FC RID: 17660
			TimesheetMessage,
			// Token: 0x040044FD RID: 17661
			TimesheetDelete,
			// Token: 0x040044FE RID: 17662
			TimesheetRecall,
			// Token: 0x040044FF RID: 17663
			TimesheetReview,
			// Token: 0x04004500 RID: 17664
			TimesheetSubmit,
			// Token: 0x04004501 RID: 17665
			TimesheetUpdate,
			// Token: 0x04004502 RID: 17666
			ReportingSyncGlobalData,
			// Token: 0x04004503 RID: 17667
			SynchronizeMembershipForWssSite,
			// Token: 0x04004504 RID: 17668
			SynchronizeSingleUserMembershipInWss,
			// Token: 0x04004505 RID: 17669
			ReportingRefresh,
			// Token: 0x04004506 RID: 17670
			UpdateScheduledProject,
			// Token: 0x04004507 RID: 17671
			WorkflowStartWorkflow,
			// Token: 0x04004508 RID: 17672
			AnalysisDelete,
			// Token: 0x04004509 RID: 17673
			AnalysisCreate,
			// Token: 0x0400450A RID: 17674
			AnalysisUpdate,
			// Token: 0x0400450B RID: 17675
			PlannerSolutionCreate,
			// Token: 0x0400450C RID: 17676
			PlannerSolutionDelete,
			// Token: 0x0400450D RID: 17677
			OptimizerSolutionCreate,
			// Token: 0x0400450E RID: 17678
			OptimizerSolutionDelete,
			// Token: 0x0400450F RID: 17679
			TimesheetLineApproval,
			// Token: 0x04004510 RID: 17680
			PeriodicTasks,
			// Token: 0x04004511 RID: 17681
			PDPUpdateProjectImpacts,
			// Token: 0x04004512 RID: 17682
			ExchangeSyncTasks,
			// Token: 0x04004513 RID: 17683
			ReportingAttributeCubeDepartmentSync,
			// Token: 0x04004514 RID: 17684
			ReportingTimesheetProjectAggregation,
			// Token: 0x04004515 RID: 17685
			ReportingTimesheetAssignmentsUpgrade,
			// Token: 0x04004516 RID: 17686
			[PSObsolete]
			WorkflowCheckinNotify,
			// Token: 0x04004517 RID: 17687
			WorkflowChangeWorkflow,
			// Token: 0x04004518 RID: 17688
			ProjectPublishSummary,
			// Token: 0x04004519 RID: 17689
			ReportingOlapDatabaseSettingsSync,
			// Token: 0x0400451A RID: 17690
			[PSObsolete]
			ReportingWorkflowMetadataSync,
			// Token: 0x0400451B RID: 17691
			[PSObsolete]
			ReportWorkflowProjectDataSync,
			// Token: 0x0400451C RID: 17692
			ReportEptSync,
			// Token: 0x0400451D RID: 17693
			ReportingSummaryPublish,
			// Token: 0x0400451E RID: 17694
			ReportingSolutionCommitedDecisionSync,
			// Token: 0x0400451F RID: 17695
			WorkflowCommitNotify,
			// Token: 0x04004520 RID: 17696
			ReportingTimesheetAssignmentsRefresh,
			// Token: 0x04004521 RID: 17697
			UpdateProjectSitePath,
			// Token: 0x04004522 RID: 17698
			AddSingleUserMembershipInWss,
			// Token: 0x04004523 RID: 17699
			DeleteSingleUserMembershipInWss,
			// Token: 0x04004524 RID: 17700
			TimeSheetUpdateResourceNonWorkingTime,
			// Token: 0x04004525 RID: 17701
			SyncProjectEnterpriseEntities,
			// Token: 0x04004526 RID: 17702
			LastMessage,
			// Token: 0x04004527 RID: 17703
			ExchangeCalOofSync,
			// Token: 0x04004528 RID: 17704
			PreparePSPermissionSynchronization,
			// Token: 0x04004529 RID: 17705
			PSPermissionSynchronizePWASite,
			// Token: 0x0400452A RID: 17706
			PSPermissionSynchronizeProjectSite,
			// Token: 0x0400452B RID: 17707
			PreparePSProjectPermissionSynchronization,
			// Token: 0x0400452C RID: 17708
			ScheduleWebPartSave,
			// Token: 0x0400452D RID: 17709
			ProjectImportTaskList,
			// Token: 0x0400452E RID: 17710
			TimesheetUpdateSRAForResource,
			// Token: 0x0400452F RID: 17711
			ActiveMonitorCheck,
			// Token: 0x04004530 RID: 17712
			ManagedModeTaskSynchronization,
			// Token: 0x04004531 RID: 17713
			ResourcePlanMigrate
		}

		// Token: 0x02000E73 RID: 3699
		[ClientCallableType(ServerTypeId = "C3034589-BAB9-40CA-AC6D-924CD8D2716B")]
		public enum JobState
		{
			// Token: 0x04004533 RID: 17715
			Unknown,
			// Token: 0x04004534 RID: 17716
			ReadyForProcessing,
			// Token: 0x04004535 RID: 17717
			SendIncomplete,
			// Token: 0x04004536 RID: 17718
			Processing,
			// Token: 0x04004537 RID: 17719
			Success,
			// Token: 0x04004538 RID: 17720
			Failed,
			// Token: 0x04004539 RID: 17721
			FailedNotBlocking,
			// Token: 0x0400453A RID: 17722
			ProcessingDeferred,
			// Token: 0x0400453B RID: 17723
			CorrelationBlocked,
			// Token: 0x0400453C RID: 17724
			Canceled,
			// Token: 0x0400453D RID: 17725
			OnHold,
			// Token: 0x0400453E RID: 17726
			Sleeping,
			// Token: 0x0400453F RID: 17727
			ReadyForLaunch,
			// Token: 0x04004540 RID: 17728
			LastState
		}

		// Token: 0x02000E74 RID: 3700
		public enum GroupState
		{
			// Token: 0x04004542 RID: 17730
			Unknown,
			// Token: 0x04004543 RID: 17731
			Unlocked,
			// Token: 0x04004544 RID: 17732
			LockedForSending,
			// Token: 0x04004545 RID: 17733
			LockedForReceiving,
			// Token: 0x04004546 RID: 17734
			CompletedSuccess,
			// Token: 0x04004547 RID: 17735
			CompletedFailure,
			// Token: 0x04004548 RID: 17736
			CompletedFailureNotBlocking,
			// Token: 0x04004549 RID: 17737
			Skipped,
			// Token: 0x0400454A RID: 17738
			Blocked,
			// Token: 0x0400454B RID: 17739
			Canceled,
			// Token: 0x0400454C RID: 17740
			OnHold,
			// Token: 0x0400454D RID: 17741
			Sleeping,
			// Token: 0x0400454E RID: 17742
			ReadyForLaunch,
			// Token: 0x0400454F RID: 17743
			LastState
		}

		// Token: 0x02000E75 RID: 3701
		public enum RendezvousState
		{
			// Token: 0x04004551 RID: 17745
			Unknown,
			// Token: 0x04004552 RID: 17746
			Initialized,
			// Token: 0x04004553 RID: 17747
			Launched,
			// Token: 0x04004554 RID: 17748
			Docking,
			// Token: 0x04004555 RID: 17749
			Ready,
			// Token: 0x04004556 RID: 17750
			Canceled,
			// Token: 0x04004557 RID: 17751
			Failed,
			// Token: 0x04004558 RID: 17752
			Complete,
			// Token: 0x04004559 RID: 17753
			Purged,
			// Token: 0x0400455A RID: 17754
			LastState
		}

		// Token: 0x02000E76 RID: 3702
		public enum QueueMsgPriority
		{
			// Token: 0x0400455C RID: 17756
			Unknown,
			// Token: 0x0400455D RID: 17757
			RecoverPriority,
			// Token: 0x0400455E RID: 17758
			HighPriority,
			// Token: 0x0400455F RID: 17759
			MediumHighPriority,
			// Token: 0x04004560 RID: 17760
			StandardPriority,
			// Token: 0x04004561 RID: 17761
			MediumLowPriority,
			// Token: 0x04004562 RID: 17762
			LowPriority,
			// Token: 0x04004563 RID: 17763
			LastPriority
		}

		// Token: 0x02000E77 RID: 3703
		public enum CorrelationPriority
		{
			// Token: 0x04004565 RID: 17765
			Unknown,
			// Token: 0x04004566 RID: 17766
			HighPriority,
			// Token: 0x04004567 RID: 17767
			StandardPriority,
			// Token: 0x04004568 RID: 17768
			LowPriority,
			// Token: 0x04004569 RID: 17769
			LastPriority
		}

		// Token: 0x02000E78 RID: 3704
		public enum QueueID
		{
			// Token: 0x0400456B RID: 17771
			UnknownQ,
			// Token: 0x0400456C RID: 17772
			ProjectQ,
			// Token: 0x0400456D RID: 17773
			TimesheetQ,
			// Token: 0x0400456E RID: 17774
			EndQList
		}

		// Token: 0x02000E79 RID: 3705
		public enum StatType
		{
			// Token: 0x04004570 RID: 17776
			None,
			// Token: 0x04004571 RID: 17777
			ProcessingTime = 2,
			// Token: 0x04004572 RID: 17778
			LastStat
		}

		// Token: 0x02000E7A RID: 3706
		public enum BlockPolicy
		{
			// Token: 0x04004574 RID: 17780
			Undefined,
			// Token: 0x04004575 RID: 17781
			Block,
			// Token: 0x04004576 RID: 17782
			DontBlock,
			// Token: 0x04004577 RID: 17783
			LastPolicy
		}

		// Token: 0x02000E7B RID: 3707
		public enum SortColumn
		{
			// Token: 0x04004579 RID: 17785
			Undefined,
			// Token: 0x0400457A RID: 17786
			CorrelationGUID,
			// Token: 0x0400457B RID: 17787
			CorrelationPriority,
			// Token: 0x0400457C RID: 17788
			GroupPriority,
			// Token: 0x0400457D RID: 17789
			JobCompletionState,
			// Token: 0x0400457E RID: 17790
			JobGUID,
			// Token: 0x0400457F RID: 17791
			JobGroupGUID,
			// Token: 0x04004580 RID: 17792
			JobInfoGUID,
			// Token: 0x04004581 RID: 17793
			LastAdminAction,
			// Token: 0x04004582 RID: 17794
			MachineName,
			// Token: 0x04004583 RID: 17795
			MessageType,
			// Token: 0x04004584 RID: 17796
			PercentComplete,
			// Token: 0x04004585 RID: 17797
			QueueCompletedTime,
			// Token: 0x04004586 RID: 17798
			QueueEntryTime,
			// Token: 0x04004587 RID: 17799
			QueueId,
			// Token: 0x04004588 RID: 17800
			QueuePosition,
			// Token: 0x04004589 RID: 17801
			QueueProcessingTime,
			// Token: 0x0400458A RID: 17802
			QueueWakeupTime,
			// Token: 0x0400458B RID: 17803
			ResourceGUID,
			// Token: 0x0400458C RID: 17804
			ServiceName,
			// Token: 0x0400458D RID: 17805
			LastColumn
		}

		// Token: 0x02000E7C RID: 3708
		public enum SortOrder
		{
			// Token: 0x0400458F RID: 17807
			Undefined,
			// Token: 0x04004590 RID: 17808
			Ascending,
			// Token: 0x04004591 RID: 17809
			Descending,
			// Token: 0x04004592 RID: 17810
			LastOrder
		}

		// Token: 0x02000E7D RID: 3709
		public enum AdminAction
		{
			// Token: 0x04004594 RID: 17812
			None,
			// Token: 0x04004595 RID: 17813
			CancelCorrelation,
			// Token: 0x04004596 RID: 17814
			RetryCorrelation,
			// Token: 0x04004597 RID: 17815
			UnblockCorrelation,
			// Token: 0x04004598 RID: 17816
			CancelJob,
			// Token: 0x04004599 RID: 17817
			RetryJob,
			// Token: 0x0400459A RID: 17818
			CanceledByEventHandler,
			// Token: 0x0400459B RID: 17819
			LastAction
		}
	}
}
