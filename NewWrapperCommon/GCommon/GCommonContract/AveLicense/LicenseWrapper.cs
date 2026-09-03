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
using System.IO;
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.AveLicense.Detail;

namespace AvePoint.GCommon.Contract.AveLicense
{
    public class LicenseWrapper
    {
        private static LicenseDetail DetailInfo { get; set; }
        private static SettlementBase SettlementBase { get; set; }
        private static object locker = new object();
        private static List<ModuleName> RCFreeModules = new List<ModuleName>() { ModuleName.RC_ActivityHistory2010, ModuleName.RC_Infrastructure2010, ModuleName.RC_RealtimeMonidtoring2010, ModuleName.RC_StorageOptimization2010 };
        private static List<ModuleName> OnlineModules = new List<ModuleName>()
        {
            ModuleName.OF_AdministrationOnline,ModuleName.OF_ConenteManagerOnline,ModuleName.OF_GranularBackupOnline,ModuleName.OF_ReplicatorOnline,ModuleName.OF_GovernanceAutomation,
            ModuleName.OF_DeploymentManagerOnline,ModuleName.OF_ArchiverOnline,ModuleName.OF_AdministrationReportsOnline,ModuleName.OF_ComplianceReportsOnline,
            ModuleName.MG_DocumentumMigrationOnline,ModuleName.MG_eRoomMigrationOnline,ModuleName.MG_FileSystemMigrationOnline,ModuleName.MG_LivelinkMigrationOnline,
            ModuleName.MG_LotusNotesMigrationOnline,ModuleName.MG_PublicFolderMigrationOnline,ModuleName.MG_QuickPlaceMigrationOnline,
            ModuleName.MG_SharePoint2007toRemoteFarm2013,ModuleName.MG_SharePoint2010toRemoteFarm2013,ModuleName.MG_SharePoint2013toRemoteFarm2016,ModuleName.MG_SharePoint2016toRemoteFarm2019,
            ModuleName.OF_ReplicatorSecureGovEditionOnline,
        };
        private static Dictionary<ModuleName, List<ModuleName>> OnlineToLocalDic = new Dictionary<ModuleName, List<ModuleName>>()
        {
            {ModuleName.OF_AdministrationOnline,new List<ModuleName>(){ModuleName.AD_CentralAdmin2010,ModuleName.AD_CentralAdmin2013,ModuleName.AD_CentralAdmin2016,ModuleName.AD_CentralAdmin2019}},
            {ModuleName.OF_ConenteManagerOnline,new List<ModuleName>(){ModuleName.AD_ContentManager2010,ModuleName.AD_ContentManager2013,ModuleName.AD_ContentManager2016,ModuleName.AD_ContentManager2019}},
            {ModuleName.OF_GranularBackupOnline,new List<ModuleName>(){ModuleName.DP_GranularBackup2010,ModuleName.DP_GranularBackup2013,ModuleName.DP_GranularBackup2016,ModuleName.DP_GranularBackup2019}},
            {ModuleName.OF_ReplicatorOnline,new List<ModuleName>(){ModuleName.AD_Replicator2010,ModuleName.AD_Replicator2013,ModuleName.AD_Replicator2016,ModuleName.AD_Replicator2019}},
            {ModuleName.OF_GovernanceAutomation,new List<ModuleName>(){ModuleName.GA_GovernanceAutomation2007,ModuleName.GA_GovernanceAutomation2010,ModuleName.GA_GovernanceAutomation2013,ModuleName.GA_GovernanceAutomation2016,ModuleName.GA_GovernanceAutomation2019}},
            {ModuleName.OF_DeploymentManagerOnline,new List<ModuleName>(){ModuleName.AD_DeploymentManager2010,ModuleName.AD_DeploymentManager2013,ModuleName.AD_DeploymentManager2016,ModuleName.AD_DeploymentManager2019}},
            {ModuleName.OF_ArchiverOnline,new List<ModuleName>(){ModuleName.SO_Archiver2010,ModuleName.SO_Archiver2013,ModuleName.SO_Archiver2016,ModuleName.SO_Archiver2019}},
            {ModuleName.OF_AdministrationReportsOnline,new List<ModuleName>(){ModuleName.RC_Administration2010,ModuleName.RC_Administration2013,ModuleName.RC_Administration2016,ModuleName.RC_Administration2019}},
            {ModuleName.OF_ComplianceReportsOnline,new List<ModuleName>(){ModuleName.RC_ComplianceReports2013,ModuleName.RC_Usage2010,ModuleName.RC_ComplianceReports2016,ModuleName.RC_ComplianceReports2019}},
            {ModuleName.OF_ReplicatorSecureGovEditionOnline,new List<ModuleName>(){ModuleName.AD_ReplicatorSecureGovEdition2010,ModuleName.AD_ReplicatorSecureGovEdition2013, ModuleName.AD_ReplicatorSecureGovEdition2016, ModuleName.AD_ReplicatorSecureGovEdition2019}},
            { ModuleName.MG_DocumentumMigrationOnline,new List<ModuleName>(){ModuleName.MG_DocumentumMigration2010,ModuleName.MG_DocumentumMigration2013,ModuleName.MG_DocumentumMigration2016,ModuleName.MG_DocumentumMigration2019}},
            {ModuleName.MG_eRoomMigrationOnline,new List<ModuleName>(){ModuleName.MG_eRoomMigration2010,ModuleName.MG_eRoomMigration2013,ModuleName.MG_eRoomMigration2016,ModuleName.MG_eRoomMigration2019}},
            {ModuleName.MG_FileSystemMigrationOnline,new List<ModuleName>(){ModuleName.MG_FileSystemMigration2010,ModuleName.MG_FileSystemMigration2013,ModuleName.MG_FileSystemMigration2016,ModuleName.MG_FileSystemMigration2019}},
            {ModuleName.MG_LivelinkMigrationOnline,new List<ModuleName>(){ModuleName.MG_LivelinkMigration2010,ModuleName.MG_LivelinkMigration2013,ModuleName.MG_LivelinkMigration2016,ModuleName.MG_LivelinkMigration2019}},
            {ModuleName.MG_LotusNotesMigrationOnline,new List<ModuleName>(){ModuleName.MG_LotusNotesMigration2010,ModuleName.MG_LotusNotesMigration2013,ModuleName.MG_LotusNotesMigration2016,ModuleName.MG_LotusNotesMigration2019}},
            {ModuleName.MG_PublicFolderMigrationOnline,new List<ModuleName>(){ModuleName.MG_PublicFolderMigration2010,ModuleName.MG_PublicFolderMigration2013,ModuleName.MG_PublicFolderMigration2016,ModuleName.MG_PublicFolderMigration2019}},
            {ModuleName.MG_QuickPlaceMigrationOnline,new List<ModuleName>(){ModuleName.MG_QuickPlaceMigration2010,ModuleName.MG_QuickPlaceMigration2013,ModuleName.MG_QuickPlaceMigration2016,ModuleName.MG_QuickPlaceMigration2019}},
            {ModuleName.MG_SharePoint2007toRemoteFarm2013,new List<ModuleName>(){ModuleName.MG_SharePoint2007to2010,ModuleName.MG_SharePoint2007to2013,ModuleName.MG_SharePoint2007to2016,ModuleName.MG_SharePoint2007to2019}},
            {ModuleName.MG_SharePoint2010toRemoteFarm2013,new List<ModuleName>(){ModuleName.MG_SharePoint2010to2013,ModuleName.MG_SharePoint2010to2016,ModuleName.MG_SharePoint2010to2019}},
            {ModuleName.MG_SharePoint2013toRemoteFarm2016,new List<ModuleName>(){ModuleName.MG_SharePoint2013to2016,ModuleName.MG_SharePoint2013to2019}},
            {ModuleName.MG_SharePoint2016toRemoteFarm2019,new List<ModuleName>(){ModuleName.MG_SharePoint2016to2019}},
        };

        public static void Install(LicenseDetail detail, SettlementBase settlement)
        {
            lock (locker)
            {
                if (settlement == null) throw new ArgumentNullException("License resource provider can not be null.");
                DetailInfo = detail;
                SettlementBase = settlement;
            }
        }

        public static ProductType ProductType
        {
            get
            {
                return DetailInfo.PrimaryInfo.ProductType;
            }
        }

        public static LicenseType LicenseType
        {
            get
            {
                return DetailInfo.PrimaryInfo.LicenseType;
            }
        }

        public static List<string> LicenseIpHosts
        {
            get
            {
                return DetailInfo.PrimaryInfo.HostsAndIPs;
            }
        }

        public static string AccountNumber
        {
            get
            {
                return DetailInfo != null && DetailInfo.PrimaryInfo != null ? DetailInfo.PrimaryInfo.AccountNumber : string.Empty;
            }
        }

        public static bool Support(ModuleName module)
        {
            if (RCFreeModules.Contains(module)) return true;
            if (DetailInfo.Status != LicenseStatus.Valid) return false;
            if (DetailInfo.ModuleDetails.ContainsKey(module) && !IsModuleExpired(module))
            {
                if (OnlineModules.Contains(module))
                {
                    if (module == ModuleName.OF_GranularBackupOnline && DetailInfo.ModuleDetails[module].HasEnforcer)
                    {
                        return true;
                    }
                    else
                    {
                        foreach (var key in OnlineToLocalDic[module])
                        {
                            if (DetailInfo.ModuleDetails.ContainsKey(key) && !IsModuleExpired(key))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Support(string farmId, ModuleName module)
        {
            if (!Support(module)) return false;
            bool support = false;
            /// 对于Migration 和 Office 365，Farm 不受 Register 控制。
            if (DetailInfo.ModuleDetails[module].Container == ModuleContainer.Office365
                || DetailInfo.ModuleDetails[module].Container == ModuleContainer.Migration
                || module == ModuleName.DP_SQLServerDataManager2010
                || module == ModuleName.DP_SQLServerDataManager2013
                || module == ModuleName.DP_SQLServerDataManager2016
                || module == ModuleName.GA_EndUserMigrationService
                || module == ModuleName.DP_VMBackup)
            {
                support = true;
            }
            else
            {
                /// RC 的 Usage；Auditor Reports； Customize 三个功能信息统一
                if ((module == ModuleName.RC_Usage2010
                    || module == ModuleName.RC_AuditorReports2010
                    || module == ModuleName.RC_Customize2010)
                    && DetailInfo.ModuleDetails[ModuleName.RC_Usage2010].RegisteredFarms != null)
                {
                    DetailInfo.ModuleDetails[ModuleName.RC_Usage2010].RegisteredFarms.ForEach(
                        (f) => { if (string.Compare(f.FarmId, farmId, StringComparison.OrdinalIgnoreCase) == 0) support = true; });
                }
                else if (DetailInfo.ModuleDetails[module].RegisteredFarms != null)
                {
                    DetailInfo.ModuleDetails[module].RegisteredFarms.ForEach(
                        (f) => { if (string.Compare(f.FarmId, farmId, StringComparison.OrdinalIgnoreCase) == 0) support = true; });
                }
            }
            return support;
        }

        public static LicenseType GetLicenseType(ModuleName module)
        {
            if (DetailInfo != null && DetailInfo.Status != LicenseStatus.NoLicense && DetailInfo.ModuleDetails.ContainsKey(module))
            {
                return DetailInfo.ModuleDetails[module].LicenseType;
            }
            return LicenseType.Demo;
        }

        /// <summary>
        /// 数据库里不存在的模块，返回值也是false
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static bool IsModuleExpired(ModuleName module)
        {
            if (!DetailInfo.ModuleDetails.ContainsKey(module))
            {
                return false;
            }
            return DetailInfo.ModuleDetails[module].ExpireTime.Ticks - SettlementBase.CachedCurrentTime.Ticks < 0;
        }

        public static bool Contains(ModuleName name)
        {
            return DetailInfo.ModuleDetails.ContainsKey(name);
        }

        public static bool IsMaintenanceExpired()
        {
            //添加detail为空
            if (DetailInfo.Status == LicenseStatus.NoLicense
                && DetailInfo.Maintenance != null
                && DetailInfo.Maintenance.ExpireTime.Ticks - SettlementBase.CachedCurrentTime.Ticks > 0)
            {
                return false;
            }
            return true;
        }

        //public static bool IsEnforcerSupported(ModuleName name)
        //{
        //    if (Support(name))
        //    {
        //        return DetailInfo.ModuleDetails[name].HasEnforcer;
        //    }
        //    return false;
        //}

        //public static bool IsEnforcerSupported(string farmId, ModuleName name)
        //{
        //    if (!IsEnforcerSupported(name)) return false;
        //    bool support = false;
        //    DetailInfo.ModuleDetails[name].RegisteredFarms.ForEach((f) => { if (string.Compare(f.FarmId, farmId, StringComparison.OrdinalIgnoreCase) == 0) support = true; });
        //    return support;
        //}

        public static bool IsSupportExtendedFunction(ModuleName name)
        {
            if (Support(name))
            {
                return DetailInfo.ModuleDetails[name].HasEnforcer;
            }
            return false;
        }

        public static bool IsSupportExtendedFunction(string farmId, ModuleName name)
        {
            if (!IsSupportExtendedFunction(name)) return false;
            bool support = false;
            DetailInfo.ModuleDetails[name].RegisteredFarms.ForEach((f) => { if (string.Compare(f.FarmId, farmId, StringComparison.OrdinalIgnoreCase) == 0) support = true; });
            return support;
        }

        public static List<ModuleName> ModulesWillExpiredIn(TimeSpan span)
        {
            List<ModuleName> list = new List<ModuleName>();
            foreach (var module in DetailInfo.ModuleDetails.Values)
            {
                TimeSpan remainedTime = RemainedAvailableTime(module.Name);
                if (remainedTime.Ticks - span.Ticks < 0)
                {
                    list.Add(module.Name);
                }
            }
            return list;
        }

        public static TimeSpan RemainedAvailableTime(ModuleName module)
        {
            if (!DetailInfo.ModuleDetails.ContainsKey(module)) return new TimeSpan();
            return DetailInfo.ModuleDetails[module].ExpireTime - SettlementBase.CachedCurrentTime;
        }

        public static long LastModifyTime
        {
            get
            {
                return DetailInfo.Status == LicenseStatus.NoLicense ? 0 : DetailInfo.LastModifyTime;
            }
        }

        public static LicenseStatus Status
        {
            get
            {
                return DetailInfo.Status;
            }
        }

        public static bool IsLicenseCompliant
        {
            get
            {
                if (DetailInfo == null || DetailInfo.Status == LicenseStatus.NoLicense) return true;
                foreach (var module in DetailInfo.ModuleDetails.Values)
                {
                    if (!module.IsEnterprised || module.Name == ModuleName.RC_AuditorReports2010 || module.Name == ModuleName.RC_Customize2010)
                    {
                        continue;
                    }
                    if (!IsModuleExpired(module.Name) && !module.IsComplaint)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static bool IsLicenseExist
        {
            get
            {
                return DetailInfo.Status != LicenseStatus.NoLicense;
            }
        }
    }

    public class GUILicenseSettlement : SettlementBase
    {
        private long BaseTime { get; set; }

        private long LocalTimeStamp { get; set; }

        public GUILicenseSettlement(long baseTime)
        {
            BaseTime = baseTime;
            LocalTimeStamp = DateTime.UtcNow.Ticks;
        }

        public override DateTime GetSystemTime()
        {
            return new DateTime(BaseTime + DateTime.UtcNow.Ticks - LocalTimeStamp);
        }
    }

    public class LicenseDenyException : Exception
    {
        /// <summary>
        /// Original english only provided for CLI exception
        /// </summary>
        public string OriginalMessage { get; set; }
        public LicenseDenyException(string message, string english)
            : base(message)
        {
            this.OriginalMessage = english;
        }
    }

    public class LicenseExceptionDto
    {
        public LicenseExceptionDto(ModuleName module, List<string> farmDisplayNames)
        {
            Module = module;
            FarmNames = farmDisplayNames;
        }

        public ModuleName Module { get; private set; }

        public List<string> FarmNames { get; private set; }
    }
}
