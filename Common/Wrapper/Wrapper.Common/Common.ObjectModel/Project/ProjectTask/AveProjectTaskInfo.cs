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
using System.Xml.Serialization;

namespace AvePoint.Wrapper.Common
{
    [XmlRoot("Task")]
    public class AveProjectTaskInfo
    {
        #region Task
        public double ActualCostWorkPerformed;
        public string ActualDuration;
        public TimeSpan ActualDurationTimeSpan;
        public double ActualOvertimeCost;
        public string ActualOvertimeWork;
        public TimeSpan ActualOvertimeWorkTimeSpan;
        public double BaselineCost;
        public string BaselineDuration;
        public TimeSpan BaselineDurationTimeSpan;
        public DateTime BaselineFinish;
        public DateTime BaselineStart;
        public string BaselineWork;
        public TimeSpan BaselineWorkTimeSpan;
        public double BudgetCost;
        public double BudgetedCostWorkPerformed;
        public double BudgetedCostWorkScheduled;
        public string Contact;
        public double CostPerformanceIndex;
        public double CostVariance;
        public double CostVarianceAtCompletion;
        public int CostVariancePercentage;
        public DateTime Created;
        public double CurrentCostVariance;
        //public CustomFieldCollection CustomFields
        public string DurationVariance;
        public TimeSpan DurationVarianceTimeSpan;
        public DateTime EarliestFinish;
        public DateTime EarliestStart;
        public double EstimateAtCompletion;
        public string FinishSlack;
        public TimeSpan FinishSlackTimeSpan;
        public string FinishVariance;
        public TimeSpan FinishVarianceTimeSpan;
        //public FixedCostAccrual FixedCostAccrual;
        public string FreeSlack;
        public TimeSpan FreeSlackTimeSpan;
        public Guid Id;
        public bool IgnoreResourceCalendar;
        public bool IsCritical;
        public bool IsEffortDriven;
        public bool IsExternalTask;
        public bool IsOverAllocated;
        public bool IsRecurring;
        public bool IsRecurringSummary;
        public bool IsRolledUp;
        public bool IsSubProject;
        public bool IsSubProjectReadOnly;
        public bool IsSubProjectScheduledFromFinish;
        public bool IsSummary;
        public DateTime LatestFinish;
        public DateTime LatestStart;
        public string LevelingDelay;
        public TimeSpan LevelingDelayTimeSpan;
        public DateTime Modified;
        public string Notes;
        public string OutlinePosition;
        public double OvertimeCost;
        public string OvertimeWork;
        public TimeSpan OvertimeWorkTimeSpan;
        public int PercentWorkComplete;
        public DateTime PreLevelingFinish;
        public DateTime PreLevelingStart;
        public string RegularWork;
        public TimeSpan RegularWorkTimeSpan;
        public double RemainingCost;
        public double RemainingOvertimeCost;
        public string RemainingOvertimeWork;
        public TimeSpan RemainingOvertimeWorkTimeSpan;
        public string RemainingWork;
        public TimeSpan RemainingWorkTimeSpan;
        public DateTime Resume;
        public double ScheduleCostVariance;
        public string ScheduledDuration;
        public TimeSpan ScheduledDurationTimeSpan;
        public DateTime ScheduledFinish;
        public DateTime ScheduledStart;
        public double SchedulePerformanceIndex;
        public int ScheduleVariancePercentage;
        public string StartSlack;
        public TimeSpan StartSlackTimeSpan;
        public string StartVariance;
        public TimeSpan StartVarianceTimeSpan;
        public DateTime Stop;
        //public PublishedProject SubProject
        public double ToCompletePerformanceIndex;
        public string TotalSlack;
        public TimeSpan TotalSlackTimeSpan;
        public string WorkBreakdownStructure;
        public string WorkVariance;
        public TimeSpan WorkVarianceTimeSpan;
        #endregion

        public double ActualCost;
        public DateTime ActualFinish;
        public DateTime ActualStart;
        public string ActualWork;
        public TimeSpan ActualWorkTimeSpan;
        //public PublishedAssignmentCollection Assignments
        public string BudgetWork;
        public TimeSpan BudgetWorkTimeSpan;
        //public Calendar Calendar
        public DateTime Completion;
        public DateTime ConstraintStartEnd;
        //public ConstraintType ConstraintType
        public double Cost;
        public DateTime Deadline;
        public string Duration;
        public TimeSpan DurationTimeSpan;
        public Dictionary<string, object> FieldValues;
        public DateTime Finish;
        public string FinishText;
        public double FixedCost;
        public bool IsActive;
        public bool IsLockedByManager;
        public bool IsManual;
        public bool IsMarked;
        public bool IsMilestone;
        public bool LevelingAdjustsAssignments;
        public bool LevelingCanSplit;
        public string Name;
        public int OutlineLevel;
        public Guid ParentId;
        public int PercentComplete;
        public int PercentPhysicalWorkComplete;
        //public PublishedTaskLinkCollection Predecessors
        public int Priority;
        public string RemainingDuration;
        public TimeSpan RemainingDurationTimeSpan;
        public DateTime Start;
        public string StartText;
        public string StatusManager;
        public int StatusManagerId;
        //public PublishedTaskLinkCollection Successors
        //public TaskType TaskType
        public bool UsePercentPhysicalWorkComplete;
        public string Work;
        public TimeSpan WorkTimeSpan;
    }
}
