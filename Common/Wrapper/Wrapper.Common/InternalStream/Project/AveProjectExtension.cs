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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public static class AveProjectExtension
    {
        public static AveProjectInfo ConvertToProjectInfo(this IAveProject project)
        {
            var info = new AveProjectInfo();

            info.ApprovedEnd = project.ApprovedEnd;
            info.ApprovedStart = project.ApprovedStart;
            info.CalculateActualCosts = project.CalculateActualCosts;
            info.CalculatesActualCosts = project.CalculatesActualCosts;
            if (project.CheckedOutBy != null)
            {
                info.CheckedOutBy = project.CheckedOutBy.LoginName;
                info.CheckedOutById = project.CheckedOutBy.ID;
            }
            info.CheckedOutDate = project.CheckedOutDate;
            info.CheckOutDescription = project.CheckOutDescription;
            info.CheckOutId = project.CheckOutId;
            info.CreatedDate = project.CreatedDate;
            info.CriticalSlackLimit = project.CriticalSlackLimit;
            info.CurrencyCode = project.CurrencyCode;
            info.CurrencyDigits = project.CurrencyDigits;
            info.CurrencySymbol = project.CurrencySymbol;
            info.CurrentDate = project.CurrentDate;
            info.DaysPerMonth = project.DaysPerMonth;
            info.DefaultEffortDriven = project.DefaultEffortDriven;
            info.DefaultEstimatedDuration = project.DefaultEstimatedDuration;
            info.DefaultFinishTime = project.DefaultFinishTime;
            info.DefaultOvertimeRate = project.DefaultOvertimeRate;
            info.DefaultStandardRate = project.DefaultStandardRate;
            info.DefaultStartTime = project.DefaultStartTime;
            info.Description = project.Description;
            info.FieldValues = project.FieldValues;
            info.FinishDate = project.FinishDate;
            info.FiscalYearStartMonth = project.FiscalYearStartMonth;
            info.HasMppPendingImport = project.HasMppPendingImport;
            info.HonorConstraints = project.HonorConstraints;
            info.OriginalId = project.Id;
            info.IsCheckedOut = project.IsCheckedOut;
            info.IsEnterpriseProject = project.IsEnterpriseProject;
            info.LastPublishedDate = project.LastPublishedDate;
            info.LastSavedDate = project.LastSavedDate;
            info.MinutesPerDay = project.MinutesPerDay;
            info.MinutesPerWeek = project.MinutesPerWeek;
            info.MoveActualIfLater = project.MoveActualIfLater;
            info.MoveActualToStatus = project.MoveActualToStatus;
            info.MoveRemainingIfEarlier = project.MoveRemainingIfEarlier;
            info.MoveRemainingToStatus = project.MoveRemainingToStatus;
            info.MultipleCriticalPaths = project.MultipleCriticalPaths;
            info.Name = project.Name;
            info.NewTasksAreManual = project.NewTasksAreManual;
            info.NumberFiscalYearFromStart = project.NumberFiscalYearFromStart;
            if (project.Owner != null)
            {
                info.Owner = project.Owner.LoginName;
                info.OwnerId = project.Owner.ID;
            }
            info.PercentComplete = project.PercentComplete;
            info.ProjectIdentifier = project.ProjectIdentifier;
            info.ProjectSiteUrl = project.ProjectSiteUrl;
            info.ProtectedActualsSynch = project.ProtectedActualsSynch;
            info.ScheduledFromStart = project.ScheduledFromStart;
            info.ShowEstimatedDurations = project.ShowEstimatedDurations;
            info.SplitInProgress = project.SplitInProgress;
            info.SpreadActualCostsToStatus = project.SpreadActualCostsToStatus;
            info.SpreadPercentCompleteToStatus = project.SpreadPercentCompleteToStatus;
            info.StartDate = project.StartDate;
            info.StatusDate = project.StatusDate;
            info.SummaryTaskId = project.SummaryTaskId;
            info.TaskListId = project.TaskListId;
            info.UtilizationDate = project.UtilizationDate;
            info.WeekStartDay = project.WeekStartDay;
            info.WinprojVersion = project.WinprojVersion;
            info.EnterpriseProjectTypeId = project.EnterpriseProjectTypeId;
            return info;
        }

        public static AveProjectTaskInfo CovertToTaskInfo(this IAveProjectTask task)
        {
            var info = new AveProjectTaskInfo();

            info.ActualCostWorkPerformed = task.ActualCostWorkPerformed;
            info.ActualDuration = task.ActualDuration;
            info.ActualDurationTimeSpan = task.ActualDurationTimeSpan;
            info.ActualOvertimeCost = task.ActualOvertimeCost;
            info.ActualOvertimeWork = task.ActualOvertimeWork;
            info.ActualOvertimeWorkTimeSpan = task.ActualOvertimeWorkTimeSpan;
            info.BaselineCost = task.BaselineCost;
            info.BaselineDuration = task.BaselineDuration;
            info.BaselineDurationTimeSpan = task.BaselineDurationTimeSpan;
            info.BaselineFinish = task.BaselineFinish;
            info.BaselineStart = task.BaselineStart;
            info.BaselineWork = task.BaselineWork;
            info.BaselineWorkTimeSpan = task.BaselineWorkTimeSpan;
            info.BudgetCost = task.BudgetCost;
            info.BudgetedCostWorkPerformed = task.BudgetedCostWorkPerformed;
            info.BudgetedCostWorkScheduled = task.BudgetedCostWorkScheduled;
            info.Contact = task.Contact;
            info.CostPerformanceIndex = task.CostPerformanceIndex;
            info.CostVariance = task.CostVariance;
            info.CostVarianceAtCompletion = task.CostVarianceAtCompletion;
            info.CostVariancePercentage = task.CostVariancePercentage;
            info.Created = task.Created;
            info.CurrentCostVariance = task.CurrentCostVariance;
            //public CustomFieldCollection CustomFields
            info.DurationVariance = task.DurationVariance;
            info.DurationVarianceTimeSpan = task.DurationVarianceTimeSpan;
            info.EarliestFinish = task.EarliestFinish;
            info.EarliestStart = task.EarliestStart;
            info.EstimateAtCompletion = task.EstimateAtCompletion;
            info.FinishSlack = task.FinishSlack;
            info.FinishSlackTimeSpan = task.FinishSlackTimeSpan;
            info.FinishVariance = task.FinishVariance;
            info.FinishVarianceTimeSpan = task.FinishVarianceTimeSpan;
            //public FixedCostAccrual FixedCostAccrual;
            info.FreeSlack = task.FreeSlack;
            info.FreeSlackTimeSpan = task.FreeSlackTimeSpan;
            info.Id = task.Id;
            info.IgnoreResourceCalendar = task.IgnoreResourceCalendar;
            info.IsCritical = task.IsCritical;
            info.IsEffortDriven = task.IsEffortDriven;
            info.IsExternalTask = task.IsExternalTask;
            info.IsOverAllocated = task.IsOverAllocated;
            info.IsRecurring = task.IsRecurring;
            info.IsRecurringSummary = task.IsRecurringSummary;
            info.IsRolledUp = task.IsRolledUp;
            info.IsSubProject = task.IsSubProject;
            info.IsSubProjectReadOnly = task.IsSubProjectReadOnly;
            info.IsSubProjectScheduledFromFinish = task.IsSubProjectScheduledFromFinish;
            info.IsSummary = task.IsSummary;
            info.LatestFinish = task.LatestFinish;
            info.LatestStart = task.LatestStart;
            info.LevelingDelay = task.LevelingDelay;
            info.LevelingDelayTimeSpan = task.LevelingDelayTimeSpan;
            info.Modified = task.Modified;
            info.Notes = task.Notes;
            info.OutlinePosition = task.OutlinePosition;
            info.OvertimeCost = task.OvertimeCost;
            info.OvertimeWork = task.OvertimeWork;
            info.OvertimeWorkTimeSpan = task.OvertimeWorkTimeSpan;
            info.PercentWorkComplete = task.PercentWorkComplete;
            info.PreLevelingFinish = task.PreLevelingFinish;
            info.PreLevelingStart = task.PreLevelingStart;
            info.RegularWork = task.RegularWork;
            info.RegularWorkTimeSpan = task.RegularWorkTimeSpan;
            info.RemainingCost = task.RemainingCost;
            info.RemainingOvertimeCost = task.RemainingOvertimeCost;
            info.RemainingOvertimeWork = task.RemainingOvertimeWork;
            info.RemainingOvertimeWorkTimeSpan = task.RemainingOvertimeWorkTimeSpan;
            info.RemainingWork = task.RemainingWork;
            info.RemainingWorkTimeSpan = task.RemainingWorkTimeSpan;
            info.Resume = task.Resume;
            info.ScheduleCostVariance = task.ScheduleCostVariance;
            info.ScheduledDuration = task.ScheduledDuration;
            info.ScheduledDurationTimeSpan = task.ScheduledDurationTimeSpan;
            info.ScheduledFinish = task.ScheduledFinish;
            info.ScheduledStart = task.ScheduledStart;
            info.SchedulePerformanceIndex = task.SchedulePerformanceIndex;
            info.ScheduleVariancePercentage = task.ScheduleVariancePercentage;
            info.StartSlack = task.StartSlack;
            info.StartSlackTimeSpan = task.StartSlackTimeSpan;
            info.StartVariance = task.StartVariance;
            info.StartVarianceTimeSpan = task.StartVarianceTimeSpan;
            info.Stop = task.Stop;
            //public PublishedProject SubProject
            info.ToCompletePerformanceIndex = task.ToCompletePerformanceIndex;
            info.TotalSlack = task.TotalSlack;
            info.TotalSlackTimeSpan = task.TotalSlackTimeSpan;
            info.WorkBreakdownStructure = task.WorkBreakdownStructure;
            info.WorkVariance = task.WorkVariance;
            info.WorkVarianceTimeSpan = task.WorkVarianceTimeSpan;
            
            info.ActualCost = task.ActualCost;
            info.ActualFinish = task.ActualFinish;
            info.ActualStart = task.ActualStart;
            info.ActualWork = task.ActualWork;
            info.ActualWorkTimeSpan = task.ActualWorkTimeSpan;
            //public PublishedAssignmentCollection Assignments
            info.BudgetWork = task.BudgetWork;
            info.BudgetWorkTimeSpan = task.BudgetWorkTimeSpan;
            //public Calendar Calendar
            info.Completion = task.Completion;
            info.ConstraintStartEnd = task.ConstraintStartEnd;
            //public ConstraintType ConstraintType
            info.Cost = task.Cost;
            info.Deadline = task.Deadline;
            info.Duration = task.Duration;
            info.DurationTimeSpan = task.DurationTimeSpan;
            info.FieldValues = task.FieldValues;
            info.Finish = task.Finish;
            info.FinishText = task.FinishText;
            info.FixedCost = task.FixedCost;
            info.IsActive = task.IsActive;
            info.IsLockedByManager = task.IsLockedByManager;
            info.IsManual = task.IsManual;
            info.IsMarked = task.IsMarked;
            info.IsMilestone = task.IsMilestone;
            info.LevelingAdjustsAssignments = task.LevelingAdjustsAssignments;
            info.LevelingCanSplit = task.LevelingCanSplit;
            info.Name = task.Name;
            info.OutlineLevel = task.OutlineLevel;
            info.ParentId = task.ParentId;
            info.PercentComplete = task.PercentComplete;
            info.PercentPhysicalWorkComplete = task.PercentPhysicalWorkComplete;
            //public PublishedTaskLinkCollection Predecessors
            info.Priority = task.Priority;
            info.RemainingDuration = task.RemainingDuration;
            info.RemainingDurationTimeSpan = task.RemainingDurationTimeSpan;
            info.Start = task.Start;
            info.StartText = task.StartText;
            if (task.StatusManager != null)
            {
                info.StatusManager = task.StatusManager.LoginName;
                info.StatusManagerId = task.StatusManager.ID;
            }
            //public PublishedTaskLinkCollection Successors
            //public TaskType TaskType
            info.UsePercentPhysicalWorkComplete = task.UsePercentPhysicalWorkComplete;
            info.Work = task.Work;
            info.WorkTimeSpan = task.WorkTimeSpan;

            return info;
        }

        public static AveProjectCalendarExceptionInfo CovertToCalendarExceptionInfo(this IAveProjectCalendarException calendarException)
        {
            var info = new AveProjectCalendarExceptionInfo();

            info.Finish = calendarException.Finish;
            info.Id = calendarException.Id;
            info.Name = calendarException.Name;
            info.RecurrenceFrequency = calendarException.RecurrenceFrequency;
            info.RecurrenceMonth = calendarException.RecurrenceMonth;
            info.RecurrenceMonthDay = calendarException.RecurrenceMonthDay;
            info.RecurrenceType = calendarException.RecurrenceType;
            info.RecurrenceWeek = calendarException.RecurrenceWeek;
            info.Shift1Finish = calendarException.Shift1Finish;
            info.Shift1Start = calendarException.Shift1Start;
            info.Shift2Finish = calendarException.Shift2Finish;
            info.Shift2Start = calendarException.Shift2Start;
            info.Shift3Finish = calendarException.Shift3Finish;
            info.Shift3Start = calendarException.Shift3Start;
            info.Shift4Finish = calendarException.Shift4Finish;
            info.Shift4Start = calendarException.Shift4Start;
            info.Shift5Finish = calendarException.Shift5Finish;
            info.Shift5Start = calendarException.Shift5Start;
            info.Start = calendarException.Start;

            return info;
        }

        public static AveProjectCalendarInfo ConvertToCalendarInfo(this IAveProjectCalendar calendar)
        {
            var info = new AveProjectCalendarInfo();
            info.BaseCalendarExceptions = new List<AveProjectCalendarExceptionInfo>(calendar.BaseCalendarExceptions.Count);
            foreach (var bce in calendar.BaseCalendarExceptions)
            {
                info.BaseCalendarExceptions.Add(bce.CovertToCalendarExceptionInfo());
            }
            info.Created = calendar.Created;
            info.Id = calendar.Id;
            info.IsStandardCalendar = calendar.IsStandardCalendar;
            info.Modified = calendar.Modified;
            info.Name = calendar.Name;

            return info;
        }

        public static AveProjectLookupTableInfo ConvertToLookupTableInfo(this IAveProjectLookupTable table)
        {
            var info = new AveProjectLookupTableInfo();

            info.AppAlternateId = table.AppAlternateId;
            info.FieldType = (int)table.FieldType;
            info.Entries = new List<AveProjectLookupEntryInfo>(table.Entries.Count);
            foreach (var entry in table.Entries)
            {
                info.Entries.Add(entry.ConvertToLookupEntryInfo());
            }
            info.Id = table.Id;
            info.Masks = new List<AveProjectLookupMaskInfo>();
            foreach (var mask in table.Masks)
            {
                info.Masks.Add(mask.ConvertToLookupMaskInfo());
            }
            info.Name = table.Name;
            info.SortOrder = table.SortOrder;

            return info;
        }

        public static AveProjectLookupEntryInfo ConvertToLookupEntryInfo(this IAveProjectLookupEntry entry)
        {
            var info = new AveProjectLookupEntryInfo();

            info.AppAlternateId = entry.AppAlternateId;
            info.Description = entry.Description;
            info.FullValue = entry.FullValue;
            info.Id = entry.Id;
            info.InternalName = entry.InternalName;
            info.SortIndex = entry.SortIndex;
            info.Value = entry.Value;
            info.HasChildren = entry.HasChildren;
            info.MaskSeparator = entry.MaskSeparator;
            info.ValueTimeSpan = entry.ValueTimeSpan;

            return info;
        }

        public static AveProjectLookupMaskInfo ConvertToLookupMaskInfo(this IAveProjectLookupMask mask)
        {
            var info = new AveProjectLookupMaskInfo();

            info.Length = mask.Length;
            info.MaskType = mask.MaskType;
            info.Separator = mask.Separator;

            return info;
        }

        public static AveProjectCustomFieldInfo ConvertToCustomFieldInfo(this IAveProjectCustomField field)
        {
            var info = new AveProjectCustomFieldInfo();

            info.AppAlternateId = field.AppAlternateId;
            info.Description = field.Description;
            info.EntityType = field.EntityType.ConvertToEntityTypeInfo();
            info.FieldType = field.FieldType;
            info.Formula = field.Formula;
            info.Id = field.Id;
            info.InternalName = field.InternalName;
            info.IsEditableInVisibility = field.IsEditableInVisibility;
            info.IsMultilineText = field.IsMultilineText;
            info.IsRequired = field.IsRequired;
            info.IsWorkflowControlled = field.IsWorkflowControlled;
            info.LookupAllowMultiSelect = field.LookupAllowMultiSelect;
            info.LookupDefaultValue = field.LookupDefaultValue;
            info.LookupEntries = new List<AveProjectLookupEntryInfo>(field.LookupEntries.Count);
            foreach (var entry in field.LookupEntries)
            {
                info.LookupEntries.Add(entry.ConvertToLookupEntryInfo());
            }
            info.LookupTable = field.LookupTable;
            info.Name = field.Name;
            info.RollsDownToAssignments = field.RollsDownToAssignments;

            return info;
        }

        public static AveProjectEntityTypeInfo ConvertToEntityTypeInfo(this IAveProjectEntityType entity)
        {
            var info = new AveProjectEntityTypeInfo();

            info.ID = entity.ID;
            info.Name = entity.Name;

            return info;
        }

        public static AveProjectEnterpriseResourceInfo ConvertToEnterpriseResourceInfo(this IAveProjectEnterpriseResource resource)
        {
            var info = new AveProjectEnterpriseResourceInfo();

            info.Assignments = new List<AveProjectStatusAssignmentInfo>(resource.Assignments.Count);
            foreach (var assignment in resource.Assignments)
            {
                info.Assignments.Add(assignment.ConvertToStatusAssignmentInfo());
            }
            info.BaseCalendar = resource.BaseCalendar.ConvertToCalendarInfo();
            info.CanLevel = resource.CanLevel;
            info.Code = resource.Code;
            info.CostCenter = resource.CostCenter;
            info.Created = resource.Created;
            info.CustomFields = new List<AveProjectCustomFieldInfo>(resource.CustomFields.Count);
            foreach (var field in resource.CustomFields)
            {
                info.CustomFields.Add(field.ConvertToCustomFieldInfo());
            }
            if (resource.DefaultAssignmentOwner != null)
            {
                info.DefaultAssignmentOwner = resource.DefaultAssignmentOwner.LoginName;
                info.DefaultAssignmentOwnerId = resource.DefaultAssignmentOwner.ID;
            }
            info.DefaultBookingType = resource.DefaultBookingType;
            info.Email = resource.Email;
            info.ExternalId = resource.ExternalId;
            info.FieldValues = resource.FieldValues;
            info.Group = resource.Group;
            info.HireDate = resource.HireDate;
            info.Id = resource.Id;
            info.Initials = resource.Initials;
            info.IsActive = resource.IsActive;
            info.IsBudget = resource.IsBudget;
            info.IsCheckedOut = resource.IsCheckedOut;
            info.IsGeneric = resource.IsGeneric;
            info.IsTeam = resource.IsTeam;
            info.MaterialLabel = resource.MaterialLabel;
            info.Modified = resource.Modified;
            info.Name = resource.Name;
            info.Phonetics = resource.Phonetics;
            info.RequiresEngagements = resource.RequiresEngagements;
            info.ResourceCalendarExceptions = new List<AveProjectCalendarExceptionInfo>(resource.ResourceCalendarExceptions.Count);
            foreach (var rce in resource.ResourceCalendarExceptions)
            {
                info.ResourceCalendarExceptions.Add(rce.CovertToCalendarExceptionInfo());
            }
            info.ResourceType = resource.ResourceType;
            info.TerminationDate = resource.TerminationDate;
            if (resource.TimesheetManager != null)
            {
                info.TimesheetManager = resource.TimesheetManager.LoginName;
                info.TimesheetManagerId = resource.TimesheetManager.ID;
            }
            if (resource.User != null)
            {
                info.User = resource.User.LoginName;
                info.UserId = resource.User.ID;
            }

            return info;
        }

        public static AveProjectStatusAssignmentInfo ConvertToStatusAssignmentInfo(this IAveProjectStatusAssignment assignment)
        {
            var info = new AveProjectStatusAssignmentInfo();

            info.ActualFinish = assignment.ActualFinish;
            info.ActualOvertime = assignment.ActualOvertime;
            info.ActualOvertimeTimeSpan = assignment.ActualOvertimeTimeSpan;
            info.ActualStart = assignment.ActualStart;
            info.ActualWork = assignment.ActualWork;
            info.ActualWorkTimeSpan = assignment.ActualWorkTimeSpan;
            info.Comments = assignment.Comments;
            info.CustomFields = new List<AveProjectCustomFieldInfo>(assignment.CustomFields.Count);
            foreach (var field in assignment.CustomFields)
            {
                info.CustomFields.Add(field.ConvertToCustomFieldInfo());
            }
            info.FieldValues = assignment.FieldValues;
            info.Finish = assignment.Finish;
            info.Id = assignment.Id;
            info.IsConfirmed = assignment.IsConfirmed;
            info.Modified = assignment.Modified;
            info.Name = assignment.Name;
            info.Overtime = assignment.Overtime;
            info.OvertimeTimeSpan = assignment.OvertimeTimeSpan;
            info.PercentComplete = assignment.PercentComplete;
            info.RegularWork = assignment.RegularWork;
            info.RegularWorkTimeSpan = assignment.RegularWorkTimeSpan;
            info.RemainingOvertime = assignment.RemainingOvertime;
            info.RemainingOvertimeTimeSpan = assignment.RemainingOvertimeTimeSpan;
            info.RemainingWork = assignment.RemainingWork;
            info.RemainingWorkTimeSpan = assignment.RemainingWorkTimeSpan;
            info.Work = assignment.Work;
            info.WorkTimeSpan = assignment.WorkTimeSpan;

            return info;
        }

        public static AveProjectPhaseInfo ConvertToPhaseInfo(this IAveProjectPhase phase)
        {
            var info = new AveProjectPhaseInfo();

            info.Description = phase.Description;
            info.Id = phase.Id;
            info.Name = phase.Name;

            return info;
        }

        public static AveProjectStageInfo ConvertToStageInfo(this IAveProjectStage stage)
        {
            var info = new AveProjectStageInfo();

            info.Behavior = stage.Behavior;
            info.CheckInRequired = stage.CheckInRequired;
            info.CustomFields = new List<AveProjectStageCustomFieldInfo>(stage.CustomFields.Count);
            foreach (var field in stage.CustomFields)
            {
                info.CustomFields.Add(field.ConvertToStageCustomFieldInfo());
            }
            info.Description = stage.Description;
            info.Id = stage.Id;
            info.Name = stage.Name;
            info.Phase = stage.Phase;
            info.ProjectDetailPages = new List<AveProjectStageDetailPageInfo>(stage.ProjectDetailPages.Count);
            foreach (var page in stage.ProjectDetailPages)
            {
                info.ProjectDetailPages.Add(page.ConvertToStageDetailPageInfo());
            }
            info.SubmitDescription = stage.SubmitDescription;
            info.WorkflowStatusPage = stage.WorkflowStatusPage.ConvertToDetailPageInfo();

            return info;
        }

        public static AveProjectStageCustomFieldInfo ConvertToStageCustomFieldInfo(this IAveProjectStageCustomField field)
        {
            var info = new AveProjectStageCustomFieldInfo();

            info.Id = field.Id;
            info.Name = field.Name;
            info.ReadOnly = field.ReadOnly;
            info.Required = field.Required;

            return info;
        }

        public static AveProjectStageDetailPageInfo ConvertToStageDetailPageInfo(this IAveProjectStageDetailPage page)
        {
            var info = new AveProjectStageDetailPageInfo();

            info.Description = page.Description;
            info.Id = page.Id;
            info.Name = page.Page.Name;
            info.Position = page.Position;
            info.RequiresAttention = page.RequiresAttention;

            return info;
        }

        public static AveProjectDetailPageInfo ConvertToDetailPageInfo(this IAveProjectDetailPage page)
        {
            var info = new AveProjectDetailPageInfo();

            info.Id = page.Id;
            info.Item = page.Item;
            info.Name = page.Name;
            info.PageType = page.PageType;

            return info;
        }

        public static AveProjectEnterpriseProjectTypeInfo ConvertToEPTInfo(this IAveProjectEnterpriseProjectType ept)
        {
            var info = new AveProjectEnterpriseProjectTypeInfo();

            info.Description = ept.Description;
            info.Id = ept.Id;
            info.ImageUrl = ept.ImageUrl;
            info.IsDefault = ept.IsDefault;
            info.IsManaged = ept.IsManaged;
            info.Name = ept.Name;
            info.Order = ept.Order;
            info.PermissionSyncEnable = ept.PermissionSyncEnable;
            info.TaskListSyncEnable = ept.TaskListSyncEnable;
            info.SiteCreationOption = (int)ept.SiteCreationOption;
            info.SiteCreationURL = ept.SiteCreationURL;
            info.ProjectDetailPages = ept.GetDetailPages();//new List<AveProjectDetailPageInfo>(ept.ProjectDetailPages.Count);
            //foreach (var page in ept.ProjectDetailPages)
            //{
            //    info.ProjectDetailPages.Add(page.ConvertToDetailPageInfo());
            //}
            info.ProjectPlanTemplateId = ept.ProjectPlanTemplateId;
            info.WorkflowAssociationId = ept.WorkflowAssociationId;
            info.WorkflowAssociationName = ept.WorkflowAssociationName;
            info.WorkspaceTemplateLCID = ept.WorkspaceTemplateLCID;
            info.WorkspaceTemplateName = ept.WorkspaceTemplateName;

            return info;
        }
    }
}
