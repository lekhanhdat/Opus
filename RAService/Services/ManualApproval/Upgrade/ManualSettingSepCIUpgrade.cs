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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Upgrade
{
    public class ManualSettingSepCIUpgrade
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualSettingSepCIUpgrade));

        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        public static async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                if(!FunctionSettingDao.TryGet(Contract.FunctionSetting.FunctionSettingType.ManualSetting, out var setting))
                {
                    Logger.Info($"[Manual Setting Sep CI Upgrade Skipped] Current tenant not has manual setting info.");
                    return;
                }

                if(setting.CreatedTime < setting.ModifiedTime)
                {
                    Logger.Info($"[Manual Setting Sep CI Upgrade Skipped] Current tenant already modified manual setting info.");
                    return;
                }

                var settingInfo = JsonConvert.DeserializeObject<ManualApprovalSettings>(setting.SettingInfo);
                settingInfo.EscalationSetting.EscalateSettingType = ManualApprovalEscalateSettingType.NoAction;
                var jsonSettingInfo = JsonConvert.SerializeObject(settingInfo);
                if(!(await FunctionSettingDao.AddOrUpdateSettingInfoAsync(Contract.FunctionSetting.FunctionSettingType.ManualSetting, jsonSettingInfo)))
                {
                    Logger.Error($"[Manual Setting Sep CI Upgrade Error] Current tenant execute modified setting info action failed.");
                    return;
                }

                Logger.Info($"[Manual Setting Sep CI Upgrade Succeed] Current tenant upgrade manual setting info succeed.");
            }
            catch(Exception e)
            {
                Logger.Error($"[Manual Setting Sep CI Upgrade Error] An error occurred while upgarde current tenant manual setting info. Error: {e}");
            }
        }
    }
}
