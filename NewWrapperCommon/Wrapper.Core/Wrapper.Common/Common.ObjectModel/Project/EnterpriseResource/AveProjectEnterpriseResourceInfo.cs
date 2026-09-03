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
    public class AveProjectEnterpriseResourceInfo
    {
        public List<AveProjectStatusAssignmentInfo> Assignments;
        public AveProjectCalendarInfo BaseCalendar;
        public bool CanLevel;
        public string Code;
        //public AccrueAt CostAccrual
        public string CostCenter;
        //public EnterpriseResourceCostRateTableCollection CostRateTables
        public DateTime Created;
        public List<AveProjectCustomFieldInfo> CustomFields;
        public string DefaultAssignmentOwner;
        public int DefaultAssignmentOwnerId;
        public AveBookingType DefaultBookingType;
        public string Email;
        //public ResourceEngagementCollection Engagements
        public string ExternalId;
        public Dictionary<string, object> FieldValues;
        public string Group;
        public DateTime HireDate;
        public Guid Id;
        public string Initials;
        public bool IsActive;
        public bool IsBudget;
        public bool IsCheckedOut;
        public bool IsGeneric;
        public bool IsTeam;
        public string MaterialLabel;
        public DateTime Modified;
        public string Name;
        public string Phonetics;
        public bool RequiresEngagements;
        public List<AveProjectCalendarExceptionInfo> ResourceCalendarExceptions;
        public int ResourceType;
        public DateTime TerminationDate;
        public string TimesheetManager;
        public int TimesheetManagerId;
        public string User;
        public int UserId;
    }
}
