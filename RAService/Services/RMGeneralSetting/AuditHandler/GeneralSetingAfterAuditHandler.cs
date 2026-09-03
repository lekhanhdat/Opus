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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract;

namespace AvePoint.RA.Service.Services.RMGeneralSetting.AuditHandler
{
    public class GeneralSetingAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(GeneralSetingAfterAuditHandler));
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        public IGeneralSettingDao GeneralSettingDao => PlatformWindsorManager.GetService<IGeneralSettingDao>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public  async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                RMCPGeneralSetting  gs = await GeneralSettingDao.GetGeneralSettingByUserAsync(TenantLocalValue.LogonGroupId);
                GeneralSettingModel setting = null;
                if(gs!=null){
                    setting = new GeneralSettingModel()
                    {
                        GeneralSetingId = gs.Id,
                        DataFormatId = gs.DataFormat,
                        TimeFormatId = gs.TimeFormat,
                        SessionTime = gs.SessionTime,
                        TimeZoneId = gs.TimeZone,
                        DayLight = gs.DayLight,
                        SessionTimeUnitId = gs.SessionTimeUnit,
                        isShowDayLight = GeneralSettingConfig.GetTimeZoneInforById(gs.TimeZone).SupportsDaylightSavingTime,
                    };
                }else{
                    setting = GeneralSettingModel.DefaultSetting;
                }

                var recordLabelSetting = await SettingProfileDao.LoadByTypeAsync((int)SettingProfilesType.RecordsLabelSetting);
                if (recordLabelSetting != null)
                {
                    setting.RecordsLabel = recordLabelSetting.Settings;
                }

                Dictionary<AuditItems, string> targetSettings = GeneralSettingService.GetAuditItems(setting);
                bool status = (bool)returnValue;
                RMAuditInfo auditInfo = new RMAuditInfo();
                auditInfo.Object = string.Empty; 
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Status = status ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                {
                    var recordsLabelTarget = "RM_RC_Audit_DeclaredRecordsMigration_RecordsLabel";
                    AuditItem recordsLabel = info.ModifyContent.Where(a => a.TargetSetting.Equals(recordsLabelTarget)).FirstOrDefault();
                    if (recordsLabel != null) { recordsLabel.NewValue = targetSettings[AuditItems.RecordsLabel]; }

                    var sessionTimeOutTarget =  "RM_Audit_SessionTimeOut";
                    AuditItem sessionTimeOut = info.ModifyContent.Where(a => a.TargetSetting.Equals(sessionTimeOutTarget)).FirstOrDefault();
                    if (sessionTimeOut != null) { sessionTimeOut.NewValue = targetSettings[AuditItems.SessionTimeOut]; }

                    var timeZoneTarget = "RM_Audit_TimeZone";
                    AuditItem timeZone = info.ModifyContent.Where(a => a.TargetSetting.Equals(timeZoneTarget)).FirstOrDefault();
                    if (timeZone != null) { timeZone.NewValue = targetSettings[AuditItems.TimeZone]; }

                    var dataFormatTarget = "RM_Audit_DataFormat";
                    AuditItem dataFormat = info.ModifyContent.Where(a => a.TargetSetting.Equals(dataFormatTarget)).FirstOrDefault();
                    if (dataFormat != null) { dataFormat.NewValue = targetSettings[AuditItems.DataFormat]; }

                    var timeFormatTarget = "RM_Audit_TimeFormat";
                    AuditItem timeFormat = info.ModifyContent.Where(a => a.TargetSetting.Equals(timeFormatTarget)).FirstOrDefault();
                    if (timeFormat != null) { timeFormat.NewValue = targetSettings[AuditItems.TimeFormat]; }

                    var newSecurityProfileName = string.Empty;
                    var securityPro = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    if (securityPro != null)
                    {
                        newSecurityProfileName = securityPro.SecurityProfileName;
                    }
                    else
                    {
                        var profile = await GeneralSettingService.SaveUsingSecurityProfileAsync();
                        if (profile != null)
                        {
                            newSecurityProfileName = profile.SecurityProfiles.Where(a => a.Id == profile.DefaultSecurityProfileId).Select(a => a.Name).FirstOrDefault();
                        }
                    }

                    if (!TenantService.IsNewOpusTenant())
                    {
                        var securityProfileTarget = "RM_Audit_SecurityProfile";
                        AuditItem securityProfile = info.ModifyContent.Where(a => a.TargetSetting.Equals(securityProfileTarget)).FirstOrDefault();
                        if (securityProfile != null) { securityProfile.NewValue = newSecurityProfileName; }
                    }
                }
                auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                return auditInfo;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
            }
        }
    }
}
