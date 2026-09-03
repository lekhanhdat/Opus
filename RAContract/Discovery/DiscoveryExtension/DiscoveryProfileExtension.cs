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
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvePoint.RA.Contract.Discovery.DiscoveryExtension
{
    public static class DiscoveryProfileExtension
    {
        public static JMDiscoveryExportProfileJobDetails GenerateExportProfileActionJobDetail(this RMDiscoveryProfileDataInfo info, JobDetailsStatus status, string comment)
        {
            return new JMDiscoveryExportProfileJobDetails
            {
                ProfileName = info?.Name ?? "Unknown",
                ProfileCriteria = BuildProfileCriteria(info),
                Action = "RM_JS_JM_Action_ExportDiscoveryProfile",
                Status = status,
                FinishTime = DateTime.UtcNow.Ticks.ToString(),
                Comment = comment,
            };
        }

        public static string ToValidName(this string input)
        {
            var validChars = input.Where(c => char.IsLetterOrDigit(c) || c == ' ');
            return new string(validChars.ToArray()).Trim();
        }

        private static string BuildProfileCriteria(RMDiscoveryProfileDataInfo info)
        {
            if (info == null) return string.Empty;
            var criteriaList = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(info.ModifiedTimeRangeLabel))
                criteriaList.Add($"RM_FA_Discovery_ConfigFilter_TimeRange: {info.ModifiedTimeRangeLabel}");

            if (!string.IsNullOrWhiteSpace(info.SizeRangeLabel))
                criteriaList.Add($"RM_DA_Profile_ProfileFileSize: {info.SizeRangeLabel}");

            if (!string.IsNullOrWhiteSpace(info.RuleInfoesLabel))
                criteriaList.Add($"RM_FA_ROTRule_Optimization_ROTrule: {info.RuleInfoesLabel}");

            if (!string.IsNullOrWhiteSpace(info.FileTypeLabel))
                criteriaList.Add($"RM_DA_Profile_ProfileFileType: {info.FileTypeLabel}");
                
            return string.Join("; ", criteriaList);
        }
    }
}
