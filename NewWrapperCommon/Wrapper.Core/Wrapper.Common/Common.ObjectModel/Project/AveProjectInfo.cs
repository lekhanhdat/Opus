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
    public class AveProjectInfo
    {
        #region Project class
        public DateTime ApprovedEnd;
        public DateTime ApprovedStart;
        public bool CalculateActualCosts;
        public bool CalculatesActualCosts;
        public string CheckedOutBy;
        public int CheckedOutById;
        public DateTime CheckedOutDate;
        public string CheckOutDescription;
        public Guid CheckOutId;
        public DateTime CreatedDate;
        public int CriticalSlackLimit;
        //CustomFieldCollection CustomFields
        public DateTime DefaultFinishTime;
        //OvertimeRateFormat DefaultOvertimeRateUnits
        //StandardRateFormat DefaultStandardRateUnits
        public DateTime DefaultStartTime;
        //ProjectEngagementCollection Engagements
        //EnterpriseProjectType EnterpriseProjectType
        public bool HasMppPendingImport;
        public bool HonorConstraints;
        public Guid OriginalId;
        public Guid NewId;
        public bool IsCheckedOut;
        public DateTime LastPublishedDate;
        public DateTime LastSavedDate;
        public bool MoveActualIfLater;
        public bool MoveActualToStatus;
        public bool MoveRemainingIfEarlier;
        public bool MoveRemainingToStatus;
        public bool MultipleCriticalPaths;
        //CommittedDecisionResult OptimizerDecision
        public int PercentComplete;
        //Phase Phase
        //CommittedDecisionResult PlannerDecision
        public string ProjectSiteUrl;
        //ProjectSummaryTask ProjectSummaryTask
        //ProjectType ProjectType
        //QueueJobCollection QueueJobs
        public bool ScheduledFromStart;
        public bool SplitInProgress;
        public bool SpreadActualCostsToStatus;
        public bool SpreadPercentCompleteToStatus;
        //Stage Stage
        public Guid SummaryTaskId;
        public Guid NewSummaryTaskId;
        public Guid TaskListId;
        public string TaskListTitle;

        #endregion

        //PublishedAssignmentCollection Assignments
        //Calendar Calendar
        public string CurrencyCode;
        public int CurrencyDigits;
        //CurrencySymbolPosition CurrencyPosition
        public string CurrencySymbol;
        public DateTime CurrentDate;
        public short DaysPerMonth;
        public bool DefaultEffortDriven;
        public bool DefaultEstimatedDuration;
        //FixedCostAccrual DefaultFixedCostAccrual
        public double DefaultOvertimeRate;
        public double DefaultStandardRate;
        //TaskType DefaultTaskType
        //WorkFormat DefaultWorkFormat
        public string Description;
        public Dictionary<string, object> FieldValues;
        public DateTime FinishDate;
        public short FiscalYearStartMonth;
        //PublishedProject IncludeCustomFields
        public bool IsEnterpriseProject;
        public int MinutesPerDay;
        public int MinutesPerWeek;
        public string Name;
        public bool NewTasksAreManual;
        public bool NumberFiscalYearFromStart;
        //user login name
        public string Owner;
        public int OwnerId;
        public string ProjectIdentifier;
        //PublishedProjectResourceCollection ProjectResources
        public bool ProtectedActualsSynch;
        public bool ShowEstimatedDurations;
        public DateTime StartDate;
        public DateTime StatusDate;
        public IAveProjectTaskCollection Tasks;
        //PublishedTaskLinkCollection TaskLinks
        //TrackingMode TrackingMode
        public DateTime UtilizationDate;
        //ProjectUtilizationType UtilizationType
        public short WeekStartDay;
        public decimal WinprojVersion;

        public Guid EnterpriseProjectTypeId;

        public AveWebCreationInformation ProjectSiteInfo;

        public bool IsNewCreated;
    }

    public class AveProjectBrowserInfo
    {
        public string Name;
        public Guid ID;
        public Guid EnterpriseProjectTypeId;
        public bool IsEnterpriseProject;
        public string Url;
        public bool IsCheckedOut;

    }
}
