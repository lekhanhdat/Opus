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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMUniqueIdSettings.AuditHandler
{
    public class RMUniqueIdSettingBeforeAuditHandler : IBeforeAuditHandler
    {
       // private RALogger logger = RALogger.GetInstance(typeof(RMUniqueIdSettingBeforeAuditHandler));

        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.Module = (AuditModule)model;
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            var newSetting = args[0] as UniqueIdSetting;
            if(newSetting != null && newSetting.SourceFlag == Contract.Explorer.SourceFlag.FileSystem)
            {
                info.Action = AuditAction.FSUniqueIDSetting;
                var fsSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
                if (fsSetting != null)
                {
                    if (info.ModifyContent == null)
                    {
                        info.ModifyContent = new List<AuditItem>();

                        AuditItem isActive = new AuditItem();
                        isActive.TargetSetting = "RM_JS_SP_ActiveUniqueId_Acitved";
                        isActive.OldValue = fsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        info.ModifyContent.Add(isActive);

                        AuditItem IdPrefix = new AuditItem();
                        IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                        IdPrefix.OldValue = fsSetting.Prefix;
                        info.ModifyContent.Add(IdPrefix);

                        AuditItem isStore = new AuditItem();
                        isStore.TargetSetting = "RM_JS_FS_UniqueId_Store";
                        isStore.OldValue = fsSetting.OverrideSPPrefix.ToString();
                        info.ModifyContent.Add(isStore);
                    }
                }
                return info;
            }
            if (newSetting != null && newSetting.SourceFlag == Contract.Explorer.SourceFlag.Teams)
            {
                info.Action = AuditAction.TeamsUniqueIDSetting;
                var teamsSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.Teams);
                if (teamsSetting != null)
                {
                    if (info.ModifyContent == null)
                    {
                        info.ModifyContent = new List<AuditItem>();

                        AuditItem isActive = new AuditItem();
                        isActive.TargetSetting = "RM_JS_SP_ActiveUniqueId_Acitved";
                        isActive.OldValue = teamsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        info.ModifyContent.Add(isActive);

                        AuditItem name = new AuditItem();
                        name.TargetSetting = "RM_JS_SP_UniqueIdColumnName";
                        name.OldValue = teamsSetting.Name;
                        info.ModifyContent.Add(name);

                        AuditItem IdPrefix = new AuditItem();
                        IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                        IdPrefix.OldValue = teamsSetting.Prefix;
                        info.ModifyContent.Add(IdPrefix);
                    }
                }
                return info;
            }
            var setting = UniqueIdSettingDao.LoadingUniqueIdSetting();
            if (setting != null)
            {
                if (info.ModifyContent == null)
                {
                    info.ModifyContent = new List<AuditItem>();

                    AuditItem isActive = new AuditItem();
                    isActive.TargetSetting = "RM_JS_SP_ActiveUniqueId_Acitved";
                    isActive.OldValue = setting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                    info.ModifyContent.Add(isActive);

                    AuditItem name = new AuditItem();
                    name.TargetSetting = "RM_JS_SP_UniqueIdColumnName";
                    name.OldValue = setting.Name;
                    info.ModifyContent.Add(name);

                    AuditItem IdPrefix = new AuditItem();
                    IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                    IdPrefix.OldValue = setting.Prefix;
                    info.ModifyContent.Add(IdPrefix);

                }
            }
            return info;
        }
    }
}
