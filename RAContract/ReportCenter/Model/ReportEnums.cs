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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ReportCenter.Model
{
    [Flags]
    [DataContract]
    public enum ActionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Creation = 1,
        [EnumMember]
        Destruction = 2,
    }
    [DataContract]
    public enum DateFrameType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CurrentWeek = 1,
        [EnumMember]
        CurrentMonth = 2,
        [EnumMember]
        Last3Months = 3,
        [EnumMember]
        Last6Months = 4,
        [EnumMember]
        Custom = 5,
    }
    [DataContract]
    public enum TermUsageReportType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Active = 1,
        [EnumMember]
        Retired = 2,
        [EnumMember]
        Orphaned = 3
    }

    public enum CheckStatus
    {
        Unchecked = 0,
        Checked = 1,
        Half = 2
    }
}
