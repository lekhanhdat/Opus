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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.ReportRelateSettingManagers
{
    internal class BoxReportRelateSettingManager : IReportRelateSettingManager
    {
        private static readonly IBoxSettingDao BoxSettingDao = PlatformWindsorManager.GetService<IBoxSettingDao>();

        private static readonly Dictionary<Guid, ManualApprovalSettingModel> ManualApprovalSettingCache = new Dictionary<Guid, ManualApprovalSettingModel>();

        public SourceFlag Flag => SourceFlag.Box;

        public async Task<ManualApprovalSettingModel> GetReportRelateSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo)
        {
            var parentId = manualApprovalReportInfo.ParentID;
            
            if (!ManualApprovalSettingCache.TryGetValue(parentId, out var settingInfo))
            {
                var setting = TryGetSetting(manualApprovalReportInfo);

                if (setting == null)
                {
                    settingInfo = new ManualApprovalSettingModel();
                }
                else
                {
                    settingInfo = new ManualApprovalSettingModel
                    {
                        SettingId = setting.Id,
                        ManualApprovalType = setting.ApprovalType,
                        IsSendEmialToOwner = setting.EMailToRecordOwner
                    };

                    if (setting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
                    {
                        settingInfo.WorkflowId = setting.WorkflowReferenceId;
                    }
                    else if (setting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
                    {
                        settingInfo.Owners = BoxSettingDao.GetRecordOwnersBySettingId(setting.Id);
                    }
                }

                ManualApprovalSettingCache[parentId] = settingInfo;
            }

            return settingInfo;
        }


        private RMBoxSetting TryGetSetting(ManualExportReportInfo manualApprovalReportInfo)
        {
            var ancestorIds = manualApprovalReportInfo.Ancestors;
            RMBoxSetting existSetting = null;
            var existingSettings = BoxSettingDao.FindAll();
            foreach (var ancestorId in ancestorIds)
            {
                existSetting = existingSettings.Find(item =>
                    new Guid(item.ScopeId) == ancestorId);
                if (existSetting != null) break;
            }

            return existSetting;
        }

    }
}
