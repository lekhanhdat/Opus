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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Archiver
{
    public class OneDriveArchiverManualAction : ArchiverManualAction
    {
        protected override SourceFlag ContentSource => SourceFlag.OneDrive;

        private static readonly IOneDriveSettingDao OneDriveSettingDao = PlatformWindsorManager.GetService<IOneDriveSettingDao>();

        private readonly List<RMOneDriveSetting> SettingCache;

        private readonly Dictionary<int, List<AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo>> SettingOwnersCache = new();

        public OneDriveArchiverManualAction()
        {
            var settings = OneDriveSettingDao.FindAll();
            SettingCache = settings.Where(setting => !setting.IsRemoved)
                .OrderByDescending(setting => setting.FullPath).ToList();
        }

        protected override ManualApprovalSettingModel GetSettingInfo(Record record)
        {
            var setting = SettingCache.FirstOrDefault(item =>
                item.SiteGroupId == new Guid(record.ContainerId) &&
                item.SiteId == new Guid(record.AveSiteId) &&
                item.WebId == record.WebId &&
                item.ListId == record.ListId &&
                item.FolderId == record.FolderId &&
                record.ManualFullPath.StartsWith(item.FullPath)
            );

            setting ??= SettingCache.First(item => item.ScopeId == new Guid(record.ContainerId));

            var res = new ManualApprovalSettingModel
            {
                IsSendEmialToOwner = setting.EMailToRecordOwner,
                ManualApprovalType = setting.ApprovalType,
                SettingId = setting.Id,
            };

            if (setting.ApprovalType == ApprovalType.ApprovalProcess)
            {
                res.WorkflowId = setting.WorkflowReferenceId;
            }
            else if (setting.ApprovalType == ApprovalType.RecordOwners)
            {
                if (!SettingOwnersCache.ContainsKey(setting.Id))
                {
                    var owners = OneDriveSettingDao.GetReocrdOwnersBySettingId(setting.Id);
                    SettingOwnersCache[setting.Id] = owners;
                }

                res.Owners = SettingOwnersCache[setting.Id];
            }

            return res;
        }
    }
}
