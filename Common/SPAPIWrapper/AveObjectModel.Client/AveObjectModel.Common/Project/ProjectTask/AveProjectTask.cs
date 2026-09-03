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

using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveProjectTask: AveClientObject, IAveProjectTask
    {
        private IAveRequest mRequest;
        private IAveSite mSite;

        public AveProjectTask(IAveRequest request, IAveSite site, Dictionary<string, object> prop)
        {
            mSite = site;
            this.mRequest = request;
            base.DataCache.AddPropertyies(prop);
        }
        
        public double ActualCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("ActualCost");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualCost", value);
            }
        }

        public double ActualCostWorkPerformed
        {
            get
            {
                return base.DataCache.GetProperty<double>("ActualCostWorkPerformed");
            }
        }

        public string ActualDuration
        {
            get
            {
                return base.DataCache.GetProperty<string>("ActualDuration");
            }
        }

        public TimeSpan ActualDurationTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ActualDurationTimeSpan");
            }
        }

        public DateTime ActualFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ActualFinish");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualFinish", value);
            }
        }

        public double ActualOvertimeCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("ActualOvertimeCost");
            }
        }

        public string ActualOvertimeWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("ActualOvertimeWork");
            }
        }

        public TimeSpan ActualOvertimeWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ActualOvertimeWorkTimeSpan");
            }
        }

        public DateTime ActualStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ActualStart");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualStart", value);
            }
        }

        public string ActualWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("ActualWork");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualWork", value);
            }
        }

        public TimeSpan ActualWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ActualWorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("ActualWorkTimeSpan", value);
            }
        }

        public double BaselineCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("BaselineCost");
            }
        }

        public string BaselineDuration
        {
            get
            {
                return base.DataCache.GetProperty<string>("BaselineDuration");
            }
        }

        public TimeSpan BaselineDurationTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("BaselineDurationTimeSpan");
            }
        }

        public DateTime BaselineFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("BaselineFinish");
            }
        }

        public DateTime BaselineStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("BaselineStart");
            }
        }

        public string BaselineWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("BaselineWork");
            }
        }

        public TimeSpan BaselineWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("BaselineWorkTimeSpan");
            }
        }

        public double BudgetCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("BudgetCost");
            }
        }

        public double BudgetedCostWorkPerformed
        {
            get
            {
                return base.DataCache.GetProperty<double>("BudgetedCostWorkPerformed");
            }
        }

        public double BudgetedCostWorkScheduled
        {
            get
            {
                return base.DataCache.GetProperty<double>("BudgetedCostWorkScheduled");
            }
        }

        public string BudgetWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("BudgetWork");
            }

            set
            {
                base.DataCache.AddChangedProperty("BudgetWork", value);
            }
        }

        public TimeSpan BudgetWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("BudgetWorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("BudgetWorkTimeSpan", value);
            }
        }

        public DateTime Completion
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Completion");
            }

            set
            {
                base.DataCache.AddChangedProperty("Completion", value);
            }
        }

        public DateTime ConstraintStartEnd
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ConstraintStartEnd");
            }

            set
            {
                base.DataCache.AddChangedProperty("ConstraintStartEnd", value);
            }
        }

        public string Contact
        {
            get
            {
                return base.DataCache.GetProperty<string>("Contact");
            }
        }

        public double Cost
        {
            get
            {
                return base.DataCache.GetProperty<double>("Cost");
            }

            set
            {
                base.DataCache.AddChangedProperty("Cost", value);
            }
        }

        public double CostPerformanceIndex
        {
            get
            {
                return base.DataCache.GetProperty<double>("CostPerformanceIndex");
            }
        }

        public double CostVariance
        {
            get
            {
                return base.DataCache.GetProperty<double>("CostVariance");
            }
        }

        public double CostVarianceAtCompletion
        {
            get
            {
                return base.DataCache.GetProperty<double>("CostVarianceAtCompletion");
            }
        }

        public int CostVariancePercentage
        {
            get
            {
                return base.DataCache.GetProperty<int>("CostVariancePercentage");
            }
        }

        public DateTime Created
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Created");
            }
        }

        public double CurrentCostVariance
        {
            get
            {
                return base.DataCache.GetProperty<double>("CurrentCostVariance");
            }
        }

        public DateTime Deadline
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Deadline");
            }

            set
            {
                base.DataCache.AddChangedProperty("Deadline", value);
            }
        }

        public string Duration
        {
            get
            {
                return base.DataCache.GetProperty<string>("Duration");
            }

            set
            {
                base.DataCache.AddChangedProperty("Duration", value);
            }
        }

        public TimeSpan DurationTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("DurationTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("DurationTimeSpan", value);
            }
        }

        public string DurationVariance
        {
            get
            {
                return base.DataCache.GetProperty<string>("DurationVariance");
            }
        }

        public TimeSpan DurationVarianceTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("DurationVarianceTimeSpan");
            }
        }

        public DateTime EarliestFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("EarliestFinish");
            }
        }

        public DateTime EarliestStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("EarliestStart");
            }
        }

        public double EstimateAtCompletion
        {
            get
            {
                return base.DataCache.GetProperty<double>("EstimateAtCompletion");
            }
        }

        public Dictionary<string, object> FieldValues
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");
            }
        }

        public DateTime Finish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Finish");
            }

            set
            {
                base.DataCache.AddChangedProperty("Finish", value);
            }
        }

        public string FinishSlack
        {
            get
            {
                return base.DataCache.GetProperty<string>("FinishSlack");
            }
        }

        public TimeSpan FinishSlackTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("FinishSlackTimeSpan");
            }
        }

        public string FinishText
        {
            get
            {
                return base.DataCache.GetProperty<string>("FinishText");
            }

            set
            {
                base.DataCache.AddChangedProperty("FinishText", value);
            }
        }

        public string FinishVariance
        {
            get
            {
                return base.DataCache.GetProperty<string>("FinishVariance");
            }
        }

        public TimeSpan FinishVarianceTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("FinishVarianceTimeSpan");
            }
        }

        public double FixedCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("FixedCost");
            }

            set
            {
                base.DataCache.AddChangedProperty("FixedCost", value);
            }
        }

        public string FreeSlack
        {
            get
            {
                return base.DataCache.GetProperty<string>("FreeSlack");
            }
        }

        public TimeSpan FreeSlackTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("FreeSlackTimeSpan");
            }
        }

        public Guid Id
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public bool IgnoreResourceCalendar
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IgnoreResourceCalendar");
            }
        }

        public bool IsActive
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsActive");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsActive", value);
            }
        }

        public bool IsCritical
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsCritical");
            }
        }

        public bool IsEffortDriven
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsEffortDriven");
            }
        }

        public bool IsExternalTask
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsExternalTask");
            }
        }

        public bool IsLockedByManager
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsLockedByManager");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsLockedByManager", value);
            }
        }

        public bool IsManual
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsManual");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsManual", value);
            }
        }

        public bool IsMarked
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsMarked");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsMarked", value);
            }
        }

        public bool IsMilestone
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsMilestone");
            }

            set
            {
                base.DataCache.AddChangedProperty("IsMilestone", value);
            }
        }

        public bool IsOverAllocated
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsOverAllocated");
            }
        }

        public bool IsRecurring
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRecurring");
            }
        }

        public bool IsRecurringSummary
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRecurringSummary");
            }
        }

        public bool IsRolledUp
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRolledUp");
            }
        }

        public bool IsSubProject
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSubProject");
            }
        }

        public bool IsSubProjectReadOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSubProjectReadOnly");
            }
        }

        public bool IsSubProjectScheduledFromFinish
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSubProjectScheduledFromFinish");
            }
        }

        public bool IsSummary
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSummary");
            }
        }

        public DateTime LatestFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LatestFinish");
            }
        }

        public DateTime LatestStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LatestStart");
            }
        }

        public bool LevelingAdjustsAssignments
        {
            get
            {
                return base.DataCache.GetProperty<bool>("LevelingAdjustsAssignments");
            }

            set
            {
                base.DataCache.AddChangedProperty("LevelingAdjustsAssignments", value);
            }
        }

        public bool LevelingCanSplit
        {
            get
            {
                return base.DataCache.GetProperty<bool>("LevelingCanSplit");
            }

            set
            {
                base.DataCache.AddChangedProperty("LevelingCanSplit", value);
            }
        }

        public string LevelingDelay
        {
            get
            {
                return base.DataCache.GetProperty<string>("LevelingDelay");
            }
        }

        public TimeSpan LevelingDelayTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("LevelingDelayTimeSpan");
            }
        }

        public DateTime Modified
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Modified");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }

            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public string Notes
        {
            get
            {
                return base.DataCache.GetProperty<string>("Notes");
            }
        }

        public int OutlineLevel
        {
            get
            {
                return base.DataCache.GetProperty<int>("OutlineLevel");
            }

            set
            {
                base.DataCache.AddChangedProperty("OutlineLevel", value);
            }
        }

        public Guid ParentId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ParentId");
            }
        }

        public string OutlinePosition
        {
            get
            {
                return base.DataCache.GetProperty<string>("OutlinePosition");
            }
        }

        public double OvertimeCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("OvertimeCost");
            }
        }

        public string OvertimeWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("OvertimeWork");
            }
        }

        public TimeSpan OvertimeWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("OvertimeWorkTimeSpan");
            }
        }

        public int PercentComplete
        {
            get
            {
                return base.DataCache.GetProperty<int>("PercentComplete");
            }

            set
            {
                base.DataCache.AddChangedProperty("PercentComplete", value);
            }
        }

        public int PercentPhysicalWorkComplete
        {
            get
            {
                return base.DataCache.GetProperty<int>("PercentPhysicalWorkComplete");
            }

            set
            {
                base.DataCache.AddChangedProperty("PercentPhysicalWorkComplete", value);
            }
        }

        public int PercentWorkComplete
        {
            get
            {
                return base.DataCache.GetProperty<int>("PercentWorkComplete");
            }
        }

        public DateTime PreLevelingFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("PreLevelingFinish");
            }
        }

        public DateTime PreLevelingStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("PreLevelingStart");
            }
        }

        public int Priority
        {
            get
            {
                return base.DataCache.GetProperty<int>("Priority");
            }

            set
            {
                base.DataCache.AddChangedProperty("Priority", value);
            }
        }

        public string RegularWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("RegularWork");
            }
        }

        public TimeSpan RegularWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RegularWorkTimeSpan");
            }
        }

        public double RemainingCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("RemainingCost");
            }
        }

        public string RemainingDuration
        {
            get
            {
                return base.DataCache.GetProperty<string>("RemainingDuration");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingDuration", value);
            }
        }

        public TimeSpan RemainingDurationTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RemainingDurationTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("RemainingDurationTimeSpan", value);
            }
        }

        public double RemainingOvertimeCost
        {
            get
            {
                return base.DataCache.GetProperty<double>("RemainingOvertimeCost");
            }
        }

        public string RemainingOvertimeWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("RemainingOvertimeWork");
            }
        }

        public TimeSpan RemainingOvertimeWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RemainingOvertimeWorkTimeSpan");
            }
        }

        public string RemainingWork
        {
            get
            {
                return base.DataCache.GetProperty<string>("RemainingWork");
            }
        }

        public TimeSpan RemainingWorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("RemainingWorkTimeSpan");
            }
        }

        public DateTime Resume
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Resume");
            }
        }

        public double ScheduleCostVariance
        {
            get
            {
                return base.DataCache.GetProperty<double>("ScheduleCostVariance");
            }
        }

        public string ScheduledDuration
        {
            get
            {
                return base.DataCache.GetProperty<string>("ScheduledDuration");
            }
        }

        public TimeSpan ScheduledDurationTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("ScheduledDurationTimeSpan");
            }
        }

        public DateTime ScheduledFinish
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ScheduledFinish");
            }
        }

        public DateTime ScheduledStart
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("ScheduledStart");
            }
        }

        public double SchedulePerformanceIndex
        {
            get
            {
                return base.DataCache.GetProperty<double>("SchedulePerformanceIndex");
            }
        }

        public int ScheduleVariancePercentage
        {
            get
            {
                return base.DataCache.GetProperty<int>("ScheduleVariancePercentage");
            }
        }

        public DateTime Start
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Start");
            }

            set
            {
                base.DataCache.AddChangedProperty("Start", value);
            }
        }

        public string StartSlack
        {
            get
            {
                return base.DataCache.GetProperty<string>("StartSlack");
            }
        }

        public TimeSpan StartSlackTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("StartSlackTimeSpan");
            }
        }

        public string StartText
        {
            get
            {
                return base.DataCache.GetProperty<string>("StartText");
            }

            set
            {
                base.DataCache.AddChangedProperty("StartText", value);
            }
        }

        public string StartVariance
        {
            get
            {
                return base.DataCache.GetProperty<string>("StartVariance");
            }
        }

        public TimeSpan StartVarianceTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("StartVarianceTimeSpan");
            }
        }

        public IAveUser StatusManager
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("StatusManager") && base.DataCache.IsPropertyAvailable("StatusManager" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    int statusManagerId = base.DataCache.GetProperty<int>("StatusManager" + AveObjectModelConstant.ObjectPropertySuffix);
                    IAveUser statusManager = this.mSite.RootWeb.SiteUsers.GetByID(statusManagerId);
                    base.DataCache.AddProperty("StatusManager",statusManager);
                    return statusManager;
                }
                return base.DataCache.GetProperty<IAveUser>("StatusManager");
            }

            set
            {
                base.DataCache.AddChangedProperty("StatusManager", value.ID);
            }
        }

        public DateTime Stop
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Stop");
            }
        }

        public double ToCompletePerformanceIndex
        {
            get
            {
                return base.DataCache.GetProperty<double>("ToCompletePerformanceIndex");
            }
        }

        public string TotalSlack
        {
            get
            {
                return base.DataCache.GetProperty<string>("TotalSlack");
            }
        }

        public TimeSpan TotalSlackTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("TotalSlackTimeSpan");
            }
        }

        public bool UsePercentPhysicalWorkComplete
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UsePercentPhysicalWorkComplete");
            }

            set
            {
                base.DataCache.AddChangedProperty("UsePercentPhysicalWorkComplete", value);
            }
        }

        public string Work
        {
            get
            {
                return base.DataCache.GetProperty<string>("Work");
            }

            set
            {
                base.DataCache.AddChangedProperty("Work", value);
            }
        }

        public string WorkBreakdownStructure
        {
            get
            {
                return base.DataCache.GetProperty<string>("WorkBreakdownStructure");
            }
        }

        public TimeSpan WorkTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("WorkTimeSpan");
            }

            set
            {
                base.DataCache.AddChangedProperty("WorkTimeSpan", value);
            }
        }

        public string WorkVariance
        {
            get
            {
                return base.DataCache.GetProperty<string>("WorkVariance");
            }
        }

        public TimeSpan WorkVarianceTimeSpan
        {
            get
            {
                return base.DataCache.GetProperty<TimeSpan>("WorkVarianceTimeSpan");
            }
        }
    }
}
