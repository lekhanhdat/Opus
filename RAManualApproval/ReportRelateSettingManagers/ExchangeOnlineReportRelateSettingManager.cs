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
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ReportRelateSettingManagers
{
    public class ExchangeOnlineReportRelateSettingManager : IReportRelateSettingManager
    {
        public SourceFlag Flag => SourceFlag.Exchange;

        private static readonly IEXOSettingDao ExoSettingDao = PlatformWindsorManager.GetService<IEXOSettingDao>();

        private static readonly Dictionary<string, ManualApprovalSettingModel> SettingCache = new Dictionary<string, ManualApprovalSettingModel>();

        public async Task<ManualApprovalSettingModel> GetReportRelateSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo)
        {
            if(SettingCache.TryGetValue(manualApprovalReportInfo.SiteUrl, out var mailboxSettingInfo))
            {
                return mailboxSettingInfo;
            }

            if(ExchangeOnlineDaoMappingManager.TryGetRecordMailBoxId(manualApprovalReportInfo, out var mailboxId))
            {
                var mailboxLocalSettingInfo = ExoSettingDao.Find(item => item.ScopeId == mailboxId && !item.IsRemoved);
                if(mailboxLocalSettingInfo != null)
                {
                    var manualApprovalSettingInfo = AssemblySettingInfo(mailboxLocalSettingInfo);
                    SettingCache[manualApprovalReportInfo.SiteUrl] = manualApprovalSettingInfo;
                    return manualApprovalSettingInfo;
                }
            }

            if(SettingCache.TryGetValue(manualApprovalReportInfo.SiteGroupID.ToString(), out var groupSettingInfo))
            {
                return groupSettingInfo;
            }

            if(ExchangeOnlineDaoMappingManager.TryGetRecordGroupId(manualApprovalReportInfo, out var groupId))
            {
                var groupLocalSettingInfo = ExoSettingDao.Find(item => item.ScopeId == groupId && !item.IsRemoved);
                if(groupLocalSettingInfo != null)
                {
                    var manualApprovalSettingInfo = AssemblySettingInfo(groupLocalSettingInfo);
                    SettingCache[manualApprovalReportInfo.SiteGroupID.ToString()] = manualApprovalSettingInfo;
                    return manualApprovalSettingInfo;
                }
            }

            SettingCache[manualApprovalReportInfo.SiteUrl] = new ManualApprovalSettingModel();
            SettingCache[manualApprovalReportInfo.SiteGroupID.ToString()] = new ManualApprovalSettingModel();

            return new ManualApprovalSettingModel();
        }

        private ManualApprovalSettingModel AssemblySettingInfo(RMExchangeOnlineSetting localSetting)
        {
            var settingInfo = new ManualApprovalSettingModel
            {
                SettingId = localSetting.Id,
                ManualApprovalType = localSetting.ApprovalType,
                IsSendEmialToOwner = localSetting.EMailToRecordOwner,
            };

            if (localSetting.ApprovalType == ApprovalType.ApprovalProcess)
            {
                settingInfo.WorkflowId = localSetting.WorkflowReferenceId;
            }
            else if(localSetting.ApprovalType == ApprovalType.RecordOwners)
            {
                settingInfo.Owners = ExoSettingDao.GetReocrdOwnersBySettingId(localSetting.Id);
            }

            return settingInfo;
        }
    }
}
