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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Dashboard
{
    public class DashboardI18ns
    {
        public static  Dictionary<SourceFlag, string> SourceFlagI18ns = new Dictionary<SourceFlag, string>
        {
            { SourceFlag.Exchange, "RM_JS_SPS_TabLabel_EXO" },
            { SourceFlag.OneDrive, "RM_JS_SPS_TabLabel_OneDrive" },
            { SourceFlag.Physical, "RM_JS_SPS_TabLabel_Physical" },
            { SourceFlag.SharePointOnPrem, "RM_JS_SPS_TabLabel_SPLocal" },
            { SourceFlag.FileSystem, "RM_JS_SPS_TabLabel_FS" },
            { SourceFlag.SharePoint, "RM_JS_SPS_TabLabel_SP" },
            { SourceFlag.AzureFileShare, "RM_JS_SPS_TabLabel_AZS" },
            { SourceFlag.Box, "RM_JS_SPS_TabLabel_Box" },
            { SourceFlag.Google, "RM_JS_SPS_TabLabel_GoogleDrive" },
            { SourceFlag.Teams, "RM_JS_SPS_TabLabel_Teams" }
        };

        public static  Dictionary<SOApproveDBStatus, string> ApprovalStatusI18ns = new Dictionary<SOApproveDBStatus, string>
        {
            {SOApproveDBStatus.WaitingApprove, "RM_DSB_WaitingApproval" },
            {SOApproveDBStatus.Approved, "RM_DSB_Approved" },
            {SOApproveDBStatus.Rejected, "RM_DSB_Rejected" },
        };
    }
}
