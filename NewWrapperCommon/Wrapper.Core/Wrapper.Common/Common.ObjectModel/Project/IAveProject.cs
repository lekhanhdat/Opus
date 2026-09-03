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
    public interface IAveProject
    {
        #region Project class
        DateTime ApprovedEnd { get; }
        DateTime ApprovedStart { get; }
        bool CalculateActualCosts { get; }
        bool CalculatesActualCosts { get; }
        IAveUser CheckedOutBy { get; set; }
        DateTime CheckedOutDate { get; }
        string CheckOutDescription { get; }
        Guid CheckOutId { get; }
        DateTime CreatedDate { get; }
        int CriticalSlackLimit { get; }
        //CustomFieldCollection CustomFields
        DateTime DefaultFinishTime { get; }
        //OvertimeRateFormat DefaultOvertimeRateUnits
        //StandardRateFormat DefaultStandardRateUnits
        DateTime DefaultStartTime { get; }
        //ProjectEngagementCollection Engagements
        IAveProjectEnterpriseProjectType EnterpriseProjectType { get; }
        bool HasMppPendingImport { get; }
        bool HonorConstraints { get; }
        Guid Id { get; }
        bool IsCheckedOut { get; }
        DateTime LastPublishedDate { get; }
        DateTime LastSavedDate { get; }
        bool MoveActualIfLater { get; }
        bool MoveActualToStatus { get; }
        bool MoveRemainingIfEarlier { get; }
        bool MoveRemainingToStatus { get; }
        bool MultipleCriticalPaths { get; }
        //CommittedDecisionResult OptimizerDecision
        int PercentComplete { get; }
        //Phase Phase
        //CommittedDecisionResult PlannerDecision
        string ProjectSiteUrl { get; }
        //ProjectSummaryTask ProjectSummaryTask
        //ProjectType ProjectType
        //QueueJobCollection QueueJobs
        bool ScheduledFromStart { get; }
        bool SplitInProgress { get; }
        bool SpreadActualCostsToStatus { get; }
        bool SpreadPercentCompleteToStatus { get; }
        //Stage Stage
        Guid SummaryTaskId { get; }
        Guid TaskListId { get; }

        #endregion

        //PublishedAssignmentCollection Assignments
        //Calendar Calendar
        string CurrencyCode { get; }
        int CurrencyDigits { get; }
        //CurrencySymbolPosition CurrencyPosition
        string CurrencySymbol { get; }
        DateTime CurrentDate { get; }
        short DaysPerMonth { get; }
        bool DefaultEffortDriven { get; }
        bool DefaultEstimatedDuration { get; }
        //FixedCostAccrual DefaultFixedCostAccrual
        double DefaultOvertimeRate { get; }
        double DefaultStandardRate { get; }
        //TaskType DefaultTaskType
        //WorkFormat DefaultWorkFormat
        string Description { get; }
        Dictionary<string, object> FieldValues { get; }
        DateTime FinishDate { get; }
        short FiscalYearStartMonth { get; }
        //PublishedProject IncludeCustomFields
        bool IsEnterpriseProject { get; }
        int MinutesPerDay { get; }
        int MinutesPerWeek { get; }
        string Name { get; }
        bool NewTasksAreManual { get; }
        bool NumberFiscalYearFromStart { get; }
        IAveUser Owner { get; set; }
        string ProjectIdentifier { get; }
        //PublishedProjectResourceCollection ProjectResources
        bool ProtectedActualsSynch { get; }
        bool ShowEstimatedDurations { get; }
        DateTime StartDate { get; }
        DateTime StatusDate { get; }
        IAveProjectTaskCollection Tasks { get; }
        //PublishedTaskLinkCollection TaskLinks
        //TrackingMode TrackingMode
        DateTime UtilizationDate { get; }
        //ProjectUtilizationType UtilizationType
        short WeekStartDay { get; }
        decimal WinprojVersion { get; }

        Guid EnterpriseProjectTypeId { get; }

        IAveProject Draft { get; }

        void Delete();
    }
}
