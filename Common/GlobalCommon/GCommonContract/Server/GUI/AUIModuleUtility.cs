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


using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Server.GUI
{
    public class AUIModuleUtility
    {
        public static Dictionary<GUIModuleType, ModuleInfo> ModuleCollection
        {
            get { return moduleCollection; }
        }

        private static Dictionary<GUIModuleType, ModuleInfo> moduleCollection = new Dictionary<GUIModuleType, ModuleInfo>()
        {
            { GUIModuleType.Vault, new ModuleInfo(){ Module="ControlGUIVault", Feature="AvePoint.Vault.Gui.Xaml.ProjectObj.Vault"}},
            { GUIModuleType.EDiscovery, new ModuleInfo(){ Module="ControlGUIEDiscovery", Feature="AvePoint.Compliance.Gui.eDiscoveryMainPage"}},
            { GUIModuleType.Migration, new ModuleInfo(){ Module="ControlGUIMigration", Feature="AvePoint.Migration.Gui.Xaml.ProjectObj.Migration"}},
            { GUIModuleType.CentralAdmin, new ModuleInfo(){ Module="ControlGUICentralAdmin", Feature="AvePoint.CentralAdmin.Gui.Xaml.CentralAdmin"}},
            { GUIModuleType.ContentManager, new ModuleInfo(){ Module="ControlGUIContentManager", Feature="AvePoint.ContentManager.Gui.Xaml.ContentManager"}},
            { GUIModuleType.Replicator, new ModuleInfo(){ Module="ControlGUIReplicator", Feature="AvePoint.Replicator.Gui.Xaml.ProjectObj.ReplicatorMain"}},
            { GUIModuleType.DeploymentManager, new ModuleInfo(){ Module="ControlGUIDeploymentManager", Feature="AvePoint.DeploymentManager.Gui.Xaml.ProjectObj.DeploymentManager"}},
            { GUIModuleType.Item, new ModuleInfo(){ Module="ControlGUIItem", Feature="AvePoint.Item.Gui.Xaml.GranularBackupAndRestore"}},
            { GUIModuleType.PlatformRecovery, new ModuleInfo(){ Module="ControlGUIPlatformRecovery", Feature="AvePoint.PlatformRecovery.Gui.Xaml.PlatformRecoveryContainer"}},
            { GUIModuleType.ReportCenter, new ModuleInfo(){ Module="ControlGUIReportCenter", Feature="AvePoint.ReportCenter.Gui.Xaml.ReportCenter"}},
            { GUIModuleType.StorageOptimization, new ModuleInfo(){ Module="ControlGUIStorageOptimization", Feature="AvePoint.StorageOptimization.Gui.StorageOptimizationMain"}},
            { GUIModuleType.DBManager, new ModuleInfo(){ Module="ControlGUIDBManager", Feature="AvePoint.DBManager.Gui.Xaml.DBManagerHome"}},
            { GUIModuleType.ControlPanel, new ModuleInfo(){ Module="ControlGUIControlPanel", Feature="AvePoint.ControlPanel.Gui.Xaml.ProjectObj.ControlPanelHome"}},
            {GUIModuleType.ExchangeOnline,new ModuleInfo(){Module="ControlGUIExchangeOnline",Feature="AvePoint.ExchangeOnline.Gui.Xaml.ProjectObj.ExchangeOnlineHome"}},
            { GUIModuleType.SystemSetting, new ModuleInfo(){ Module="ControlGUIControlPanel", Feature="AvePoint.ControlPanel.Gui.Xaml.SystemSetting.SystemSettingRoot"}},
        };
    }

    public enum GUIModuleType
    {
        Vault,
        EDiscovery,
        Migration,
        CentralAdmin,
        ContentManager,
        Replicator,
        DeploymentManager,
        Item,
        PlatformRecovery,
        ReportCenter,
        StorageOptimization,
        DBManager,
        ControlPanel,
        ExchangeOnline,
        SystemSetting
    }

    public class ModuleInfo
    {
        /// <summary>
        /// Dll的名称
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 导航到Dll的主页面
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        /// Dll更改时间
        /// </summary>
        public string LastModifyTime { get; set; }
    }
}
