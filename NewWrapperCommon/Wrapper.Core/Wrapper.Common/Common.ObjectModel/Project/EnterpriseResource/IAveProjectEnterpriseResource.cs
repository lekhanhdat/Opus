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
    public interface IAveProjectEnterpriseResource
    {
        IAveProjectStatusAssignmentCollection Assignments { get; }
        IAveProjectCalendar BaseCalendar { get; set; }
        bool CanLevel { get; set; }
        string Code { get; set; }
        //public AccrueAt CostAccrual
        string CostCenter { get; set; }
        //public EnterpriseResourceCostRateTableCollection CostRateTables
        DateTime Created { get; }
        IAveProjectCustomFieldCollection CustomFields { get; }
        //user login name
        IAveUser DefaultAssignmentOwner { get; set; }
        AveBookingType DefaultBookingType { get; set; }
        string Email { get; set; }
        //public ResourceEngagementCollection Engagements
        string ExternalId { get; set; }
        Dictionary<string, object> FieldValues { get; }
        string Group { get; set; }
        DateTime HireDate { get; set; }
        Guid Id { get; }
        string Initials { get; set; }
        bool IsActive { get; set; }
        bool IsBudget { get; }
        bool IsCheckedOut { get; }
        bool IsGeneric { get; }
        bool IsTeam { get; }
        string MaterialLabel { get; set; }
        DateTime Modified { get; }
        string Name { get; set; }
        string Phonetics { get; set; }
        bool RequiresEngagements { get; set; }
        IAveProjectCalendarExceptionCollection ResourceCalendarExceptions { get; }
        int ResourceType { get; }
        DateTime TerminationDate { get; set; }
        //user login name
        IAveUser TimesheetManager { get; set; }
        //user login name
        IAveUser User { get; set; }

        void Update();
    }
}
