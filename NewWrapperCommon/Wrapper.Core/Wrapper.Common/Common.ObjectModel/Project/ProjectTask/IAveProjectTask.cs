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
 

namespace AvePoint.Wrapper.Common
{
    public interface IAveProjectTask
    {
        #region Task
        double ActualCostWorkPerformed { get; }
        string ActualDuration { get; }
        TimeSpan ActualDurationTimeSpan { get; }
        double ActualOvertimeCost { get; }
        string ActualOvertimeWork { get; }
        TimeSpan ActualOvertimeWorkTimeSpan { get; }
        double BaselineCost { get; }
        string BaselineDuration { get; }
        TimeSpan BaselineDurationTimeSpan { get; }
        DateTime BaselineFinish { get; }
        DateTime BaselineStart { get; }
        string BaselineWork { get; }
        TimeSpan BaselineWorkTimeSpan { get; }
        double BudgetCost { get; }
        double BudgetedCostWorkPerformed { get; }
        double BudgetedCostWorkScheduled { get; }
        string Contact { get; }
        double CostPerformanceIndex { get; }
        double CostVariance { get; }
        double CostVarianceAtCompletion { get; }
        int CostVariancePercentage { get; }
        DateTime Created { get; }
        double CurrentCostVariance { get; }
        //public CustomFieldCollection CustomFields
        string DurationVariance { get; }
        TimeSpan DurationVarianceTimeSpan { get; }
        DateTime EarliestFinish { get; }
        DateTime EarliestStart { get; }
        double EstimateAtCompletion { get; }
        string FinishSlack { get; }
        TimeSpan FinishSlackTimeSpan { get; }
        string FinishVariance { get; }
        TimeSpan FinishVarianceTimeSpan { get; }
        //public FixedCostAccrual FixedCostAccrual;
        string FreeSlack { get; }
        TimeSpan FreeSlackTimeSpan { get; }
        Guid Id { get; }
        bool IgnoreResourceCalendar { get; }
        bool IsCritical { get; }
        bool IsEffortDriven { get; }
        bool IsExternalTask { get; }
        bool IsOverAllocated { get; }
        bool IsRecurring { get; }
        bool IsRecurringSummary { get; }
        bool IsRolledUp { get; }
        bool IsSubProject { get; }
        bool IsSubProjectReadOnly { get; }
        bool IsSubProjectScheduledFromFinish { get; }
        bool IsSummary { get; }
        DateTime LatestFinish { get; }
        DateTime LatestStart { get; }
        string LevelingDelay { get; }
        TimeSpan LevelingDelayTimeSpan { get; }
        DateTime Modified { get; }
        string Notes { get; }
        string OutlinePosition { get; }
        double OvertimeCost { get; }
        string OvertimeWork { get; }
        TimeSpan OvertimeWorkTimeSpan { get; }
        int PercentWorkComplete { get; }
        DateTime PreLevelingFinish { get; }
        DateTime PreLevelingStart { get; }
        string RegularWork { get; }
        TimeSpan RegularWorkTimeSpan { get; }
        double RemainingCost { get; }
        double RemainingOvertimeCost { get; }
        string RemainingOvertimeWork { get; }
        TimeSpan RemainingOvertimeWorkTimeSpan { get; }
        string RemainingWork { get; }
        TimeSpan RemainingWorkTimeSpan { get; }
        DateTime Resume { get; }
        double ScheduleCostVariance { get; }
        string ScheduledDuration { get; }
        TimeSpan ScheduledDurationTimeSpan { get; }
        DateTime ScheduledFinish { get; }
        DateTime ScheduledStart { get; }
        double SchedulePerformanceIndex { get; }
        int ScheduleVariancePercentage { get; }
        string StartSlack { get; }
        TimeSpan StartSlackTimeSpan { get; }
        string StartVariance { get; }
        TimeSpan StartVarianceTimeSpan { get; }
        DateTime Stop { get; }
        //public PublishedProject SubProject
        double ToCompletePerformanceIndex { get; }
        string TotalSlack { get; }
        TimeSpan TotalSlackTimeSpan { get; }
        string WorkBreakdownStructure { get; }
        string WorkVariance { get; }
        TimeSpan WorkVarianceTimeSpan { get; }
        #endregion

        double ActualCost { get; set; }
        DateTime ActualFinish { get; set; }
        DateTime ActualStart { get; set; }
        string ActualWork { get; set; }
        TimeSpan ActualWorkTimeSpan { get; set; }
        //public PublishedAssignmentCollection Assignments
        string BudgetWork { get; set; }
        TimeSpan BudgetWorkTimeSpan { get; set; }
        //public Calendar Calendar
        DateTime Completion { get; set; }
        DateTime ConstraintStartEnd { get; set; }
        //public ConstraintType ConstraintType
        double Cost { get; set; }
        DateTime Deadline { get; set; }
        string Duration { get; set; }
        TimeSpan DurationTimeSpan { get; set; }
        Dictionary<string, object> FieldValues { get; }
        DateTime Finish { get; set; }
        string FinishText { get; set; }
        double FixedCost { get; set; }
        bool IsActive { get; set; }
        bool IsLockedByManager { get; set; }
        bool IsManual { get; set; }
        bool IsMarked { get; set; }
        bool IsMilestone { get; set; }
        bool LevelingAdjustsAssignments { get; set; }
        bool LevelingCanSplit { get; set; }
        string Name { get; set; }
        int OutlineLevel { get; set; }
        Guid ParentId { get; }
        int PercentComplete { get; set; }
        int PercentPhysicalWorkComplete { get; set; }
        //public PublishedTaskLinkCollection Predecessors
        int Priority { get; set; }
        string RemainingDuration { get; set; }
        TimeSpan RemainingDurationTimeSpan { get; set; }
        DateTime Start { get; set; }
        string StartText { get; set; }
        IAveUser StatusManager { get; set; }
        //public PublishedTaskLinkCollection Successors
        //public TaskType TaskType
        bool UsePercentPhysicalWorkComplete { get; set; }
        string Work { get; set; }
        TimeSpan WorkTimeSpan { get; set; }
    }
}
