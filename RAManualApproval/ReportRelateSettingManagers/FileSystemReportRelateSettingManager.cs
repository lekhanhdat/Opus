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
    public class FileSystemReportRelateSettingManager : IReportRelateSettingManager
    {
        public SourceFlag Flag => SourceFlag.FileSystem;

        private static readonly IFileSystemSettingDao FileSytemSettingDao = PlatformWindsorManager.GetService<IFileSystemSettingDao>();

        private static readonly IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();

        private readonly List<RMFileSystemSetting> Settings;

        private readonly List<FSConnection> Connections;

        public FileSystemReportRelateSettingManager()
        {
            Settings = FileSytemSettingDao.FindAll().OrderByDescending(item => item.FullPath).ToList();
            Connections = FSConnectionDao.FindAll().OrderByDescending(item => item.UNCPath).ToList();
        }

        public async Task<ManualApprovalSettingModel> GetReportRelateSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo)
        {
            var localSetting = Settings.FirstOrDefault(item =>
            manualApprovalReportInfo.Path.StartsWith(item.FullPath.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase));
            if (localSetting == null)
            {
                var connectionInfo = Connections.FirstOrDefault(item =>
                manualApprovalReportInfo.Path.StartsWith(item.UNCPath.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase));

                localSetting = Settings.FirstOrDefault(item => item.ScopeId == connectionInfo.Id);

                localSetting ??= Settings.FirstOrDefault(item => item.ScopeId == connectionInfo.GroupId);
            }

            if (localSetting == null)
            {
                return new ManualApprovalSettingModel();
            }

            var settingInfo = new ManualApprovalSettingModel
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
                settingInfo.Owners = FileSytemSettingDao.GetReocrdOwnersBySettingId(localSetting.Id);
            }

            return settingInfo;
        }
    }
}
