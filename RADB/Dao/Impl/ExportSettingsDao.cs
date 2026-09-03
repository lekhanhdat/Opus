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
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using System.Data.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ExportSettingsDao : BaseDao<RMCPExportSetting>, IExportSettingsDao
    {
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private bool HasUpgradeVEOV3 = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.VEOV3) && TenantService.IsNewOpusTenant() && RMKeyValueDao.HasUpgradeVEOV3();

        public bool Delete()
        {
            using var context = GetNewContext();
            var data = context.RMCPExportSetting.FirstOrDefault();
            if (data != null)
            {
                return this.Delete(data);
            }
            else
            {
                return false;
            }
        }

        public bool Delete(int exportType)
        {
            using var context = GetNewContext();
            var data = context.RMCPExportSetting.Where(e => e.ExportSettingType == exportType).ToList();
            if(exportType == (int)ExportSettingType.VEO && HasUpgradeVEOV3)
            {
                data = context.RMCPExportSetting.Where(e => e.ExportSettingType == exportType && e.VEOContent != null && e.VEOHistory != null).ToList();
            }
            if (data.Any())
            {
                return this.BatchDelete(data) > 0;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exportType"></param>
        /// <param name="sourceFlag">默认是SP</param>
        /// <returns></returns>
        public RMCPExportSetting GetExportSetting(int exportType, int sourceFlag = 1)
        {
            using var context = GetNewContext();
            return context.RMCPExportSetting.FirstOrDefault(e => e.ExportSettingType == exportType && e.SourceFlag == sourceFlag);
        }

        public RMCPExportSetting GetExportSetting(bool loadDeactived)
        {
            using var context = GetNewContext();
            if (loadDeactived)
            {
                return context.RMCPExportSetting.FirstOrDefault();
            }
            else
            {
                return context.RMCPExportSetting.FirstOrDefault(e => e.IsActived == true);
            }
        }

        public List<RMCPExportSetting> GetExportSettings()
        {
            using var context = GetNewContext();
            return context.RMCPExportSetting.AsNoTracking().ToList();
        }

        public List<RMCPExportSetting> GetExportSettings(int exportType)
        {
            using var context = GetNewContext();
            return context.RMCPExportSetting.AsNoTracking().Where(e => e.ExportSettingType == exportType).ToList();
        }

        public void SaveOrUpdate(List<RMCPExportSetting> settings)
        {
            using (var ctx = GetNewContext())
            {
                foreach (var item in settings)
                {
                    if (!ctx.RMCPExportSetting.Any(e => e.ExportSettingType == item.ExportSettingType && e.SourceFlag == item.SourceFlag && e.VEOContent == null && e.VEOHistory == null))
                    {
                        ctx.RMCPExportSetting.Add(item);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        var oldData = ctx.RMCPExportSetting
                            .Where(e => e.ExportSettingType == item.ExportSettingType
                            && e.SourceFlag == item.SourceFlag
                            && e.VEOContent == null
                            && e.VEOHistory == null)
                            .FirstOrDefault();
                        ArgumentCheck.NotNull(oldData, nameof(oldData));
                        oldData.ArchiverSetting = item.ArchiverSetting;
                        oldData.ArchiverVEOSetting = item.ArchiverVEOSetting;
                        oldData.FileVEO = item.FileVEO;
                        oldData.RecordVEO = item.RecordVEO;
                        oldData.ManifestVEO = item.ManifestVEO;
                        oldData.IsActived = item.IsActived;
                        oldData.FileName = item.FileName;
                        oldData.ExportSettingType = item.ExportSettingType;
                        oldData.ExportConfig = item.ExportConfig;
                        oldData.SourceFlag = item.SourceFlag;
                        ApplyCurrentValues(ctx, oldData);
                    }
                }
            }
        }

        public void SaveOrUpdateVEOV3(List<RMCPExportSetting> settings)
        {
            using (var ctx = GetNewContext())
            {
                foreach (var item in settings)
                {
                    if (!ctx.RMCPExportSetting.Any(e => e.ExportSettingType == item.ExportSettingType && e.SourceFlag == item.SourceFlag && e.VEOContent != null && e.VEOHistory != null))
                    {
                        ctx.RMCPExportSetting.Add(item);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        var oldData = ctx.RMCPExportSetting
                            .Where(e => e.ExportSettingType == item.ExportSettingType
                            && e.SourceFlag == item.SourceFlag
                            && e.VEOContent != null
                            && e.VEOHistory != null)
                            .FirstOrDefault();
                        ArgumentCheck.NotNull(oldData, nameof(oldData));
                        oldData.VEOContent = item.VEOContent;
                        oldData.VEOHistory = item.VEOHistory;
                        oldData.ArchiverSetting = item.ArchiverSetting;
                        oldData.IsActived = item.IsActived;
                        oldData.FileName = item.FileName;
                        oldData.ExportSettingType = item.ExportSettingType;
                        oldData.ExportConfig = item.ExportConfig;
                        oldData.SourceFlag = item.SourceFlag;
                        ApplyCurrentValues(ctx, oldData);
                    }
                }
            }
        }
    }
}
