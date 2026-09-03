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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ControlPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMUniqueIdSettings.AuditHandler
{
    public class RMUniqueIdSettingAfterAuditHandler : IAfterAuditHandler
    {
        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();

        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();

        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                info.Module = (AuditModule)model;
                info.Action = (AuditAction)action;
                info.Category = (AuditCategory)category;

                var newSetting = args[0] as UniqueIdSetting;
                if (newSetting != null && newSetting.SourceFlag == Contract.Explorer.SourceFlag.FileSystem)
                {
                    info.Action = AuditAction.FSUniqueIDSetting;
                    var enableUniqueIdsetting = await AgentMgmtService.CheckIfEnableFSUniqueIdSetting();
                    if (!enableUniqueIdsetting)
                    {
                        info.Status = 1;
                        return info;
                    }
                    var fsSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
                    if (fsSetting != null)
                    {
                        if (info.ModifyContent == null)
                        {
                            info.ModifyContent = new List<AuditItem>();

                            AuditItem isActive = new AuditItem();
                            isActive.TargetSetting = "RM_JS_SP_ActiveUniqueId_Acitved";
                            isActive.NewValue = fsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(isActive);

                            AuditItem IdPrefix = new AuditItem();
                            IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                            IdPrefix.NewValue = fsSetting.Prefix;
                            info.ModifyContent.Add(IdPrefix);

                            AuditItem isStore = new AuditItem();
                            IdPrefix.TargetSetting = "RM_JS_FS_UniqueId_Store";
                            IdPrefix.NewValue = fsSetting.OverrideSPPrefix.ToString();
                            info.ModifyContent.Add(isStore);
                        }
                        else
                        {
                            AuditItem isActive = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_ActiveUniqueId_Acitved")).FirstOrDefault();
                            isActive.NewValue = fsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";

                            AuditItem IdPrefix = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_IdFomate_Prefix")).FirstOrDefault();
                            IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                            IdPrefix.NewValue = fsSetting.Prefix;

                            AuditItem isStore = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_FS_UniqueId_Store")).FirstOrDefault();
                            isStore.TargetSetting = "RM_JS_FS_UniqueId_Store";
                            isStore.NewValue = fsSetting.OverrideSPPrefix.ToString();
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
                            isActive.NewValue = teamsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                            info.ModifyContent.Add(isActive);


                            AuditItem name = new AuditItem();
                            name.TargetSetting = "RM_JS_SP_UniqueIdColumnName";
                            name.NewValue = teamsSetting.Name;
                            info.ModifyContent.Add(name);


                            AuditItem IdPrefix = new AuditItem();
                            IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                            IdPrefix.NewValue = teamsSetting.Prefix;
                            info.ModifyContent.Add(IdPrefix);
                        }
                        else
                        {
                            AuditItem isActive = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_ActiveUniqueId_Acitved")).FirstOrDefault();
                            isActive.NewValue = teamsSetting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";

                            AuditItem name = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_UniqueIdColumnName")).FirstOrDefault();
                            name.NewValue = teamsSetting.Name;

                            AuditItem IdPrefix = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_IdFomate_Prefix")).FirstOrDefault();
                            IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                            IdPrefix.NewValue = teamsSetting.Prefix;
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
                        isActive.NewValue = setting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                        info.ModifyContent.Add(isActive);


                        AuditItem name = new AuditItem();
                        name.TargetSetting = "RM_JS_SP_UniqueIdColumnName";
                        name.NewValue = setting.Name;
                        info.ModifyContent.Add(name);


                        AuditItem IdPrefix = new AuditItem();
                        IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                        IdPrefix.NewValue = setting.Prefix;
                        info.ModifyContent.Add(IdPrefix);
                    }
                    else
                    {
                        AuditItem isActive = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_ActiveUniqueId_Acitved")).FirstOrDefault();
                        isActive.NewValue = setting.IsActived ? "RM_JS_Common_Yes" : "RM_JS_Common_No";

                        AuditItem name = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_UniqueIdColumnName")).FirstOrDefault();
                        name.NewValue = setting.Name;

                        AuditItem IdPrefix = info.ModifyContent.Where(a => a.TargetSetting.Equals("RM_JS_SP_IdFomate_Prefix")).FirstOrDefault();
                        IdPrefix.TargetSetting = "RM_JS_SP_IdFomate_Prefix";
                        IdPrefix.NewValue = setting.Prefix;
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return info;
        }
    }
}
