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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class GeneralSettingDao : BaseDao<RMCPGeneralSetting>,IGeneralSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(GeneralSettingDao));
        public async Task<RMCPGeneralSetting> GetGeneralSettingByUserAsync(string tenantId)
        {
            RMCPGeneralSetting data = null;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                data = await ctx.RMCPGeneralSetting.Where<RMCPGeneralSetting>(RMCPGeneralSetting => RMCPGeneralSetting.TenantId.Equals(tenantId)).FirstOrDefaultAsync();

            }

            return data;
        }

        public GeneralSettingModel GetCurrentGeneralSetting()
        {
            RMCPGeneralSetting model = null;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                model = ctx.RMCPGeneralSetting.AsNoTracking().Where<RMCPGeneralSetting>(RMCPGeneralSetting => RMCPGeneralSetting.TenantId.Equals(TenantLocalValue.LogonGroupId)).FirstOrDefault();
            }
            if (model != null)
            {
                var res = new GeneralSettingModel()
                {
                    GeneralSetingId = model.Id,
                    DataFormatId = model.DataFormat,
                    TimeFormatId = model.TimeFormat,
                    SessionTime = model.SessionTime,
                    TimeZoneId = model.TimeZone,
                    DayLight = model.DayLight,
                    SessionTimeUnitId = model.SessionTimeUnit,
                    isShowDayLight = GeneralSettingConfig.GetTimeZoneInforById(model.TimeZone).SupportsDaylightSavingTime,
                    EmailSenderDefinition = new EmailSenderDefinition 
                    { 
                        EmailSenderType = EmailSenderType.Default,
                        AppProfileId = string.Empty,
                        EmailSender = null
                    }
                };

                if(!string.IsNullOrWhiteSpace(model.EmailSenderDefinition))
                {
                    res.EmailSenderDefinition = JsonConvert.DeserializeObject<EmailSenderDefinition>(model.EmailSenderDefinition);
                }

                return res;
            }

            return GeneralSettingModel.DefaultSetting;
        }


        public async Task<bool> UpdateOrSaveGeneralSettingByUserAsync(RMCPGeneralSetting model, string tenantId)
        {
            bool result = true;
            try
            {
                RMCPGeneralSetting oldData = null;

                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    oldData = await ctx.RMCPGeneralSetting.Where<RMCPGeneralSetting>(RMCPGeneralSetting => RMCPGeneralSetting.TenantId.Equals(tenantId)).FirstOrDefaultAsync();
                    if (oldData == null)
                    {
                        ctx.RMCPGeneralSetting.Add(model);
                        await ctx.SaveChangesAsync();
                        await RMCacheManager.GeneralSetingAdded();
                    }
                    else
                    {
                        oldData.DataFormat = model.DataFormat;
                        oldData.DayLight = model.DayLight;
                        oldData.RegistedEmail = model.RegistedEmail;
                        oldData.SessionTime = model.SessionTime;
                        oldData.SessionTimeUnit = model.SessionTimeUnit;
                        oldData.TimeFormat = model.TimeFormat;
                        oldData.TimeZone = model.TimeZone;
                        oldData.EmailSenderDefinition = model.EmailSenderDefinition;
                        await ctx.SaveChangesAsync();
                        await RMCacheManager.GeneralSettingUpdated();
                        //this.Update(oldData);
                    }
                }

            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return result;
        }

        public void UpdateOrSaveGeneralSettingById(RMCPGeneralSetting model, string tenantId)
        {
            RMCPGeneralSetting oldData = null;
            using (var context = RMDBContextManager.GetSystemDBContext())
            {
                oldData = context.RMCPGeneralSetting.Where<RMCPGeneralSetting>(RMCPGeneralSetting => RMCPGeneralSetting.TenantId.Equals(tenantId)).FirstOrDefault();
                if (oldData == null)
                {
                    context.RMCPGeneralSetting.Add(model);
                    context.SaveChanges();
                }
                else
                {
                    this.Update(model);
                }
            }

        }

        public bool DeleteGeneralSettingByUser(string tenantId)
        {
            bool result = true;
            try
            {
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    RMCPGeneralSetting data = context.RMCPGeneralSetting.Where<RMCPGeneralSetting>(RMCPGeneralSetting => RMCPGeneralSetting.TenantId.Equals(tenantId)).FirstOrDefault();
                    if (data != null)
                    {
                        context.RMCPGeneralSetting.Remove(data);
                        context.SaveChanges();
                        logger.Info("success to delete general setting:{0}", tenantId);
                    }
                }
            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return result;
        }
        private bool Update(RMCPGeneralSetting entity)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var entry = ctx.Entry(entity);
                if (entry.State == EntityState.Modified)
                {
                    return ctx.SaveChanges() > 0;
                }
                else if (entry.State == EntityState.Detached)
                {
                    ctx.DetachLocalObject<RMCPGeneralSetting>(entity);
                    ctx.Set<RMCPGeneralSetting>().Attach(entity);
                    entry.State = EntityState.Modified;
                    return ctx.SaveChanges() > 0;
                }
                return false;
            }

        }

    }
}
