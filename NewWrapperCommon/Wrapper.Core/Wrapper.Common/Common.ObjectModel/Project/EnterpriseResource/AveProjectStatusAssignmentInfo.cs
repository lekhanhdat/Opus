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
    public class AveProjectStatusAssignmentInfo
    {
        public DateTime ActualFinish;
        public string ActualOvertime;
        public TimeSpan ActualOvertimeTimeSpan;
        public DateTime ActualStart;
        public string ActualWork;
        public TimeSpan ActualWorkTimeSpan;
        //public StatusApprovalType ApprovalStatus
        public string Comments;
        public List<AveProjectCustomFieldInfo> CustomFields;
        public Dictionary<string, object> FieldValues;
        public DateTime Finish;
        //public StatusAssignmentHistoryLineCollection History
        public Guid Id;
        public bool IsConfirmed;
        public DateTime Modified;
        public string Name;
        public string Overtime;
        public TimeSpan OvertimeTimeSpan;
        public short PercentComplete;
        //public PublishedProject Project
        public string RegularWork;
        public TimeSpan RegularWorkTimeSpan;
        public string RemainingOvertime;
        public TimeSpan RemainingOvertimeTimeSpan;
        public string RemainingWork;
        public TimeSpan RemainingWorkTimeSpan;
        //public EnterpriseResource Resource
        public DateTime Start;
        //public StatusTask Task
        public string Work;
        public TimeSpan WorkTimeSpan;
    }
}
