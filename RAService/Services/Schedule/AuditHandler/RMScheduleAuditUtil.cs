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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Schedule.AuditHandler
{
    public class RMScheduleAuditUtil
    {
        public static ScheduleType[] ContainsSkipRemoveScheduleTypes = new ScheduleType[] { ScheduleType.DisposalSchedule, ScheduleType.EXODisposalSchedule, 
            ScheduleType.PRDisposalSchedule, ScheduleType.OneDriveDisposalSchedule };
        public static bool GetExtensionSkipValue(ScheduleInfo info)
        {
            bool skip = false;
            if (info.JobCategory == ScheduleType.DisposalSchedule)
            {
                skip = JsonConvert.DeserializeObject<RMSPTreeNode>(info.Extentions).SkipRemoveContentAndDestroyAction;
            }
            else if (info.JobCategory == ScheduleType.EXODisposalSchedule)
            {
                skip = JsonConvert.DeserializeObject<RMEXOTreeNode>(info.Extentions).SkipRemoveContentAndDestroyAction;
            }
            else if (info.JobCategory == ScheduleType.PRDisposalSchedule)
            {
                skip = bool.Parse(info.Extentions);
            }
            else if (info.JobCategory == ScheduleType.OneDriveDisposalSchedule)
            {
                skip = JsonConvert.DeserializeObject<RMSPTreeNode>(info.Extentions).SkipRemoveContentAndDestroyAction;
            }
            else if (info.JobCategory == ScheduleType.TeamsDisposalSchedule)
            {
                skip = JsonConvert.DeserializeObject<RMSPTreeNode>(info.Extentions).SkipRemoveContentAndDestroyAction;
            }
            return skip;
        }

        public static bool GetIsUseDecryptValue(ScheduleInfo info)
        {
            bool useDecrypt = false;
            if (info.JobCategory is ScheduleType.DisposalSchedule or ScheduleType.OneDriveDisposalSchedule or ScheduleType.TeamsDisposalSchedule)
            {
                useDecrypt = JsonConvert.DeserializeObject<RMSPTreeNode>(info.Extentions).IsEnableSuperUserDecrypt;
            }
            return useDecrypt;
        }

        public static AuditAction GetDisposalScheduleAction(ScheduleType tempJobCategory)
        {
            var tempAction = AuditAction.Unknown;
            switch (tempJobCategory)
            {
                case ScheduleType.DisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule;
                    break;
                case ScheduleType.EXODisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4EXO;
                    break;
                case ScheduleType.PRDisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4PR;
                    break;
                case ScheduleType.FSDisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4FS;
                    break;
                case ScheduleType.SPOnPremDisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4SPOnPrem;
                    break;
                case ScheduleType.OneDriveDisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4OneDrive;
                    break;
                case ScheduleType.SPArchiveJobSchedule:
                    tempAction = AuditAction.ConfigureArchiverDisposalJobSchedule4SPO;
                    break;
                case ScheduleType.OneDriveArchiveJobSchedule:
                    tempAction = AuditAction.ConfigureArchiverDisposalJobSchedule4OneDrive;
                    break;
                case ScheduleType.BoxDisposalSchedule:
                    tempAction = AuditAction.ConfigureBoxDisposalJobSchedule;
                    break;
                case ScheduleType.ColletionDataSchedule:
                    tempAction = AuditAction.ConfigureCollectionJobSchedule;
                    break;
                case ScheduleType.GoogleDisposalSchedule:
                    tempAction = AuditAction.ConfigureGoogleDisposalJobSchedule;
                    break;
                case ScheduleType.TeamsDisposalSchedule:
                    tempAction = AuditAction.ConfigureDisposalJobSchedule4Teams;
                    break;
                case ScheduleType.TeamsArchiveJobSchedule:
                    tempAction = AuditAction.ConfigureArchiverDisposalJobSchedule4Teams;
                    break;
            }
            return tempAction;
        }
    }
}
