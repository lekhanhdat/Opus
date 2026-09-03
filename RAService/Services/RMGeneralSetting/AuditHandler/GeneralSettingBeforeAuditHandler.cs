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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.RMGeneralSetting.AuditHandler
{
    public class GeneralSettingBeforeAuditHandler : IBeforeAuditHandler
    {
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private RALogger logger = RALogger.GetInstance(typeof(GeneralSettingBeforeAuditHandler));
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {
                GeneralSettingModel setting = await GeneralSettingService.GetGeneralSettingAsync();
                Dictionary<AuditItems, string> targetSettings = GeneralSettingService.GetAuditItems(setting);
                if (setting != null)
                {
                    if (info.ModifyContent == null)
                    {
                        info.ModifyContent = new List<AuditItem>();
                    }

                    AuditItem recordsLabelItem = new AuditItem();
                    recordsLabelItem.TargetSetting = "RM_RC_Audit_DeclaredRecordsMigration_RecordsLabel";
                    recordsLabelItem.OldValue = targetSettings[AuditItems.RecordsLabel] ?? string.Empty;
                    info.ModifyContent.Add(recordsLabelItem);

                    AuditItem sessionTimeOutItem = new AuditItem();
                    sessionTimeOutItem.TargetSetting = "RM_Audit_SessionTimeOut";
                    sessionTimeOutItem.OldValue = targetSettings[AuditItems.SessionTimeOut] != null ? targetSettings[AuditItems.SessionTimeOut] : string.Empty;
                    info.ModifyContent.Add(sessionTimeOutItem);

                    AuditItem timeZoneItem = new AuditItem();
                    timeZoneItem.TargetSetting = "RM_Audit_TimeZone";
                    timeZoneItem.OldValue = targetSettings[AuditItems.TimeZone] != null ? targetSettings[AuditItems.TimeZone] : string.Empty;
                    info.ModifyContent.Add(timeZoneItem);

                    AuditItem dataFormatItem = new AuditItem();
                    dataFormatItem.TargetSetting = "RM_Audit_DataFormat";
                    dataFormatItem.OldValue = targetSettings[AuditItems.DataFormat] != null ? targetSettings[AuditItems.DataFormat] : string.Empty;
                    info.ModifyContent.Add(dataFormatItem);

                    AuditItem timeFormatItem = new AuditItem();
                    timeFormatItem.TargetSetting = "RM_Audit_TimeFormat";
                    timeFormatItem.OldValue = targetSettings[AuditItems.TimeFormat] != null ? targetSettings[AuditItems.TimeFormat] : string.Empty;
                    info.ModifyContent.Add(timeFormatItem);

                    var oldSecurityProfileName = string.Empty;
                    var securityPro = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    if (securityPro != null)
                    {
                        oldSecurityProfileName = securityPro.SecurityProfileName;
                    }
                    else
                    {
                        var securityProfile = await GeneralSettingService.SaveUsingSecurityProfileAsync();
                        if (securityProfile != null)
                        {
                            oldSecurityProfileName = securityProfile.SecurityProfiles.Where(a => a.Id == securityProfile.DefaultSecurityProfileId).Select(a => a.Name).FirstOrDefault();
                        }
                    }

                    if (!TenantService.IsNewOpusTenant())
                    {
                        AuditItem securityProfileAudit = new AuditItem();
                        securityProfileAudit.TargetSetting = "RM_Audit_SecurityProfile";
                        securityProfileAudit.OldValue = oldSecurityProfileName;
                        info.ModifyContent.Add(securityProfileAudit);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
            }

            return info;
        }
    }
}
