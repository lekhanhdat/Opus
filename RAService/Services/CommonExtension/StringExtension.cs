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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CommonExtension
{
    public static class StringExtension
    {
        public static string ConvertI18NTimeRangeType(this TimeRangeType rangeType)
        {
            return rangeType switch
            {
                TimeRangeType.CurrentWeek => I18NEntity.GetString("RM_RC_Audit_Range_5D"),
                TimeRangeType.CurrentMonth => I18NEntity.GetString("RM_RC_Audit_Range_1M"),
                TimeRangeType.Last3Month => I18NEntity.GetString("RM_RC_Audit_Range_3M"),
                TimeRangeType.Last6Month => I18NEntity.GetString("RM_RC_Audit_Range_6M"),
                _ => rangeType.ToString()
            };
        }

        public static string MapI18NKeyPhysicalOptionsJSON(this string value)
        {
            return value switch
            {
                "Open" => I18NEntity.GetString("RM_Template_Column_Value_Status_Open"),
                "Destroyed" => I18NEntity.GetString("RM_Template_Column_Value_Status_Destroyed"),
                "Closed" => I18NEntity.GetString("RM_Template_Column_Value_Status_Closed"),
                "Missing" => I18NEntity.GetString("RM_Template_Column_Value_Status_Missing"),
                "Document" => I18NEntity.GetString("RM_Template_Column_Value_Format_Document"),
                "Cassette" => I18NEntity.GetString("RM_Template_Column_Value_Format_Cassette"),
                "Map" => I18NEntity.GetString("RM_Template_Column_Value_Format_Map"),
                "Plan" => I18NEntity.GetString("RM_Template_Column_Value_Format_Play"),
                "DVD" => I18NEntity.GetString("RM_Template_Column_Value_Format_DVD"),
                "Internal use only" => I18NEntity.GetString("RM_Template_Column_Value_ProtectiveMarking_InternalUsedOnly"),
                "Public" => I18NEntity.GetString("RM_Template_Column_Value_ProtectiveMarking_Public"),
                "Confidential" => I18NEntity.GetString("RM_Template_Column_Value_ProtectiveMarking_Confidential"),
                "Highly confidential" => I18NEntity.GetString("RM_Template_Column_Value_ProtectiveMarking_HighlyConfidential"),
                _ => I18NEntity.GetString(value)
            };
        }
    }
}
