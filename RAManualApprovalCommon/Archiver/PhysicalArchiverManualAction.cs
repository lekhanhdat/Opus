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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using RAManualApprovalCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace RAManualApprovalCommon.Archiver
{
    public class PhysicalArchiverManualAction: ArchiverManualAction
    {
        protected override SourceFlag ContentSource => SourceFlag.Physical;

        private static readonly IPhysicalRecordSettingDao PhysicalSettingDao = PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();

        private static readonly IRMLocationDao LocationDao = PlatformWindsorManager.GetService<IRMLocationDao>();

        private static readonly Dictionary<Guid, ManualApprovalSettingModel> SettingCache = new Dictionary<Guid, ManualApprovalSettingModel>();

        private static readonly Dictionary<Guid, Guid> TopLocationIdCache = new Dictionary<Guid, Guid>();

        public PhysicalArchiverManualAction(string jobId) : base(jobId, Guid.Empty)
        {
        }

        protected override ManualApprovalSettingModel GetSettingInfo(Record record)
        {
            var topLocationId = GetTopLocationId(record.LocationId);
            if (!SettingCache.TryGetValue(topLocationId, out var settingInfo))
            {
                var localSetting = PhysicalSettingDao.Find(item => item.LocationUniqueId == topLocationId && !item.IsRemoved);
                if (localSetting == null)
                {
                    settingInfo = new ManualApprovalSettingModel();
                }
                else
                {
                    settingInfo = new ManualApprovalSettingModel
                    {
                        SettingId = localSetting.Id,
                        ManualApprovalType = localSetting.ApprovalType,
                        IsSendEmialToOwner = localSetting.EMailToRecordOwner
                    };

                    if (localSetting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
                    {
                        settingInfo.WorkflowId = localSetting.WorkflowReferenceId;
                    }
                    else if (localSetting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
                    {
                        settingInfo.Owners = PhysicalSettingDao.GetReocrdOwnersBySettingId(localSetting.Id);
                    }
                }

                SettingCache[topLocationId] = settingInfo;
            }

            return settingInfo;
        }

        protected override Task ProcessWorkflowOwnerAsync(ManualApprovalRuleModel ruleInfo, Record record)
        {
            var message = $"RM_MA_NoSupport_SiteOwner{I18NEntity.Separator}{"RM_JS_SPS_TabLabel_Physical"}";
            throw new NotImplementedException(message);
        }

        private Guid GetTopLocationId(Guid locationId)
        {
            if (!TopLocationIdCache.TryGetValue(locationId, out var topLocationId))
            {
                var location = LocationDao.GetLocationByUniqueId(locationId);
                var locationIds = location.DirPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (locationIds.Count == 1)
                {
                    topLocationId = locationId;
                }
                else
                {
                    var topLocation = LocationDao.GetLocationById(Convert.ToInt32(locationIds[1]));
                    topLocationId = topLocation.UniqueId;
                }

                TopLocationIdCache[locationId] = topLocationId;
            }
            return topLocationId;
        }
    }
}
