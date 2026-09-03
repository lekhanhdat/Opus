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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.Server.ControlPanel;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
    using AvePoint.GCommon.MicroKernel.MicroKernelIntentionImpl;
    using AvePoint.Common.SQLServer;
    using Microsoft.Win32;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.AveModuleContract;

    #endregion

    public class AgentRegisterUtil
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void RegisterAgent(string domain, string username, string plainPassword)
        {
            IMAgentService agentControlService = WcfUtility.GetManagerService<IMAgentService>();

            var agentDto = new ServiceDto();
            agentDto.Schema = AveEnv.AgentSchema;
            agentDto.Name = AveEnv.AgentName;
            agentDto.Address = AveEnv.AgentAddress;
            agentDto.Port = AveEnv.AgentPort;
            agentDto.AgentType = AveEnv.AgentType;
            agentDto.Domain = domain;
            agentDto.UserName = username;
            agentDto.Password = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(plainPassword));
            agentDto.Version = AveEnv.AgentVersion;
            agentDto.DisplayVersion = AveEnv.AgentDisplayVersion;
            agentDto.SPVersion = AveEnv.SPVersion;
            agentDto.MossOrWss = AveEnv.MossOrWss;
            agentDto.FarmName = AveEnv.AgentFarmName;
            agentDto.FarmId = AveEnv.AgentFarmId;
            agentDto.ExtraInfo = string.Empty;
            agentDto.EnvironmentInfo = AveEnv.SharePointDisplayVersion;
            agentDto.SharePointTime = DateTime.UtcNow.Ticks;
            agentDto.CpuNumber = Environment.ProcessorCount;
            try
            {
                agentDto.CpuHz = OSInformation.CPUHz;
            }
            catch (Exception e)
            {
                logger.Error("And error occurred while getting CPUHz. Details:{0}", e.ToString());
            }
            agentDto.Passphrase = AveEnv.PassphraseHash;
            agentDto.OEMProductType = (int)AveEnv.AgentProductType;
            agentDto.DotNetVersions = FrameworkVersionDetection.GetAllInstalledDotNetVersions();
            CacheSettingDto cacheSettingDto = new CacheSettingDto();
            cacheSettingDto.SetDiskInfoDto(new DiskInfoDto() { Path = AveEnv.AgentTempFolder });
            agentDto.CacheSetting = cacheSettingDto;

            agentDto.RoleInFarm = FarmRoles.NONE;
            if (AveEnv.SPVersion != 0)
            {
                agentDto.RoleInFarm |= FarmRoles.SPSERVER;
            }
            if (PRAgentRegisterProxy.IsFastInstalled())
            {
                agentDto.RoleInFarm |= FarmRoles.FAST;
            }
            if (PRAgentRegisterProxy.IsSQLInstalled())
            {
                agentDto.RoleInFarm |= FarmRoles.SQL;
            }
            if (PRAgentRegisterProxy.IsNotesInstalled())
            {
                agentDto.RoleInFarm |= FarmRoles.NOTES;
            }
            if (PRAgentRegisterProxy.IsERoomInstalled())
            {
                agentDto.RoleInFarm |= FarmRoles.EROOM;
            }
            if (PRAgentRegisterProxy.IsJSharp2Installed())
            {
                agentDto.RoleInFarm |= FarmRoles.JSHARP;
            }
            if (PRAgentRegisterProxy.IsDocumentumInstalled())
            {
                agentDto.RoleInFarm |= FarmRoles.DFC;
            }
            if (PRAgentRegisterProxy.IsNotesInstalled())
            {
                //if (PRAgentRegisterProxy.IsQuickPlaceInstalled())
                //{
                    agentDto.RoleInFarm |= FarmRoles.QuickPlace;
                //}
            }
            if (AveEnv.SPVersion != 0)
            {
                Tuple<FarmRoles, int> farmInfo = PRAgentRegisterProxy.GetFarmRoles(domain, username, plainPassword);
                if (farmInfo.Item1 != FarmRoles.FAILED)
                {
                    agentDto.RoleInFarm |= farmInfo.Item1;
                }
                else
                {
                    logger.Error("Getting CA or WFE role in farm failed.");
                }
                if (agentDto.FarmServiceDto == null)
                {
                    agentDto.FarmServiceDto = new GCommon.Contract.Server.Common.FarmServiceDto();
                }
                agentDto.FarmServiceDto.FarmServiceCount = farmInfo.Item2;
                agentDto.FarmServiceDto.SPVersion = agentDto.SPVersion;
                logger.Info("SP farm server count:{0}", farmInfo.Item2);
            }

            logger.Info("Agent Information: {0}", agentDto.ToString());
            logger.Info("Agent current runtime version is " + Environment.Version.ToString());
            logger.Info("Agent Installed framework versions as below: ");
            foreach (string v in agentDto.DotNetVersions)
            {
                logger.Info(v);
            }
            logger.Info("SP Account: {0}\\{1}", agentDto.Domain, agentDto.UserName);
            RegisterResult result = agentControlService.Register(agentDto);
            if (result.IsSucceed == false)
            {
                logger.Warn("Register failed. AgentName:{0} AgentAddress:{1} FarmName:{2} FarmId:{3} Reason:{4}", agentDto.Name, agentDto.Address, agentDto.FarmName, agentDto.FarmId, result.FailedReason);
                if (result.FailedReason == FailedReason.FarmInfoIncorrect)
                {
                    foreach (var existingAgentFarmInfo in result.FarmInfos)
                    {
                        logger.Info("Existing AgentName:{0} AgentAddress:{1} FarmName:{2} FarmId:{3}", existingAgentFarmInfo.AgentName, existingAgentFarmInfo.AgentAddress, existingAgentFarmInfo.FarmName, existingAgentFarmInfo.FarmID);
                    }
                    bool farmNameChanged = false;
                    foreach (var existingAgentFarmInfo in result.FarmInfos)
                    {
                        if (string.Compare(agentDto.FarmId, existingAgentFarmInfo.FarmID, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            logger.Info("Found existing FarmName:{0} with FarmId:{1}", existingAgentFarmInfo.FarmName, existingAgentFarmInfo.FarmID);
                            AveEnv.AgentFarmName = existingAgentFarmInfo.FarmName;
                            AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentFarmName = true });
                            farmNameChanged = true;
                            break;
                        }
                    }
                    if (farmNameChanged == false)
                    {
                        logger.Info("FarmName duplicated with different FarmId, Changing it.");
                        string farmName = AveEnv.AgentFarmName;
                        farmName = farmName.Insert(farmName.IndexOf(":", StringComparison.OrdinalIgnoreCase), "_1");
                        logger.Info("Changing FarmName from {0} to {1}", AveEnv.AgentFarmName, farmName);
                        AveEnv.AgentFarmName = farmName;
                        AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentFarmName = true });
                    }
                }
                throw new Exception("Register failed");
            }
            if (!string.IsNullOrEmpty(result.CIID))
            {
                AveEnv.AgentCIID = result.CIID;
                AveEnv.PersistConfiguration(new AveEnv.PersistOptions() { PersistAgentCIID = true });
                logger.SetDeployIdToLogFile(AveEnv.AgentCIID);
            }
            AgentCacheManager.PersistRegisterResult(result.CommunicationEncryptionKey, result.CryptoMode);
            AveStaticEnv.Setup();
        }

        private void AdjustAgentType(ServiceDto agentDto)
        {
            if (agentDto.RoleInFarm != FarmRoles.FAILED && (agentDto.RoleInFarm & FarmRoles.WFE) != FarmRoles.WFE)
            {
                logger.Info("Role in farm does not contain WFE, processing agent type.");
                if (!AveEnv.AgentSkipRemoveAgentType)
                {
                    AveAgentType aveAgentType = new AveAgentType(AveEnv.AgentType);
                    if (aveAgentType.SPAgentTypeList.Contains(PlatformBackup.AGENT_TYPE_PR_CONTROL) || aveAgentType.SPAgentTypeList.Contains(PlatformBackup.AGENT_TYPE_PR_MEMBER))
                    {
                        logger.Info("Remove all SharePoint agent type except PR.");
                        aveAgentType.SPAgentTypeList.Clear();
                        aveAgentType.SPAgentTypeList.Add(PlatformBackup.AGENT_TYPE_PR_CONTROL);
                        aveAgentType.SPAgentTypeList.Add(PlatformBackup.AGENT_TYPE_PR_MEMBER);
                    }
                    else
                    {
                        logger.Info("Remove all SharePoint agent type.");
                        aveAgentType.SPAgentTypeList.Clear();
                    }
                    agentDto.AgentType = aveAgentType.toCombinedAgentTypeString();
                }
                else
                {
                    logger.Info("Skip removing agent type.");
                }
            }
        }

    }

    public class PRAgentRegisterProxy
    {
        private static AveLogger mLog = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly static string WSS_ADMIN_WPG = "WSS_ADMIN_WPG";

        private static bool mIsLocalFarmAdminGroupExists = false;
        static PRAgentRegisterProxy()
        {
            try
            {
                if (PermissionProvisionManager.IsLocalGroupExists(WSS_ADMIN_WPG))
                {
                    mIsLocalFarmAdminGroupExists = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Debug(ex.Message);
            }
        }

        private static bool IsCurrentUserInLocalFarmAdminGroup(string domainName, string userName)
        {
            if (mIsLocalFarmAdminGroupExists)
            {
                if (PermissionProvisionManager.IsDomainUserInLocalGroup(domainName, userName, WSS_ADMIN_WPG))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsFastInstalled()
        {
            bool mIsFast = false;
            try
            {
                string fastPath = string.Empty;
                fastPath = Environment.GetEnvironmentVariable("FASTSEARCH");
                if (string.IsNullOrEmpty(fastPath))
                {
                    RegistryKey rs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\FAST Search Server\Setup");
                    if (rs != null)
                    {
                        fastPath = rs.GetValue("Path").ToString();
                    }
                }
                if (!string.IsNullOrEmpty(fastPath))
                {
                    if (Directory.Exists(fastPath))
                    {
                        mIsFast = true;
                    }
                }
            }
            catch (Exception ex)
            {
                mIsFast = false;
                mLog.Warn("Check the fast search server failed:{0}", ex.Message);
            }
            return mIsFast;
        }

        public static bool IsSQLInstalled()
        {
            SQLServerInstanceCollection col = new SQLServerInstanceCollection();
            col.Initialize();
            if (col.instances.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsAgentServiceStarted(string serviceName)
        {
            try
            {
                System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Warn("Check agent service started exception:{0}", e.ToString());
                //no log need here
                return false;
            }
        }

        public static Tuple<FarmRoles, int> GetFarmRoles(string domainName, string userName, string plainPassword)
        {
            try
            {
                string serviceName = "AgentRoleCheckerService";
                string serviceExe = "SP2007AgentCommonRoleChecker.exe";
                if (AveEnv.IsSharePoint2010)
                {
                    serviceExe = AvePoint.Common.AgentConstants.AgentBinaryName.COMMON_ROLECHECKER_2010;
                }
                else if (AveEnv.IsSharePoint2013OrAbove)
                {
                    serviceExe = AvePoint.Common.AgentConstants.AgentBinaryName.COMMON_ROLECHECKER_2013;
                }

                mLog.Info("SharePoint version is " + AveSPEnv.SPVersion.ToString());

                string processName = serviceExe;
                if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    processName = processName.Substring(0, processName.Length - 4);
                }
                var ps = System.Diagnostics.Process.GetProcessesByName(processName);
                if (ps.Length > 0)
                {
                    foreach (var p in ps) p.Kill();
                }

                string serviceStartedFlagFile = Guid.NewGuid().ToString();
                mLog.Info(string.Format("Start process [{0}] under account [{1}\\{2}]", serviceExe, domainName, userName));

                try
                {
                    StartProcess sp = new StartProcess(domainName, userName, plainPassword, "");
                    sp.Start(Path.Combine(AveEnv.AgentBinFolder, serviceExe), serviceName + " " + serviceStartedFlagFile);
                }
                catch(Exception ex)
                {
                    if (ex is System.ComponentModel.Win32Exception || ex.InnerException is System.ComponentModel.Win32Exception)
                    {
                        var errorCode = ex is System.ComponentModel.Win32Exception ?
                            (ex as System.ComponentModel.Win32Exception).NativeErrorCode :
                            (ex.InnerException as System.ComponentModel.Win32Exception).NativeErrorCode;
                        if (errorCode == 1326)
                        {
                            AgentCredentialManager.ClearAgentCredentialCache();
                        }
                    }
                    throw ex;
                }
                mLog.Info("Checking Agent Role Checker Service's status.");
                DateTime deadline = DateTime.Now.AddMinutes(AveEnv.AgentCheckingRoleInFarmTimeout);
                bool serviceStartedSucceed = true;
                while (true)
                {
                    if (DateTime.Now > deadline)
                    {
                        serviceStartedSucceed = false;
                        mLog.Error("Agent Role Checker Service does not started before deadline.");
                        break;
                    }
                    if (!File.Exists(Path.Combine(AveEnv.AgentTempFolder, serviceStartedFlagFile)))
                    {
                        mLog.Info("Waiting for Agent Role Checker Service Started.");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        mLog.Info("Agent Role Checker Service is started.");
                        File.Delete(Path.Combine(AveEnv.AgentTempFolder, serviceStartedFlagFile));
                        break;
                    }
                }

                if (!serviceStartedSucceed)
                {
                    return new Tuple<FarmRoles, int>(FarmRoles.FAILED, 0);
                }
                else
                {
                    deadline = DateTime.Now.AddMinutes(AveEnv.AgentCheckingRoleInFarmTimeout);
                    while (true)
                    {
                        try
                        {
                            IAPRInstallService prAgentInstallService = CustomizeChannelFactory<IAPRInstallService>.CreateChannel(serviceName, AveEnv.AgentSchema, AveEnv.AgentAddress, AveEnv.AgentPort, "");
                            FarmRoles farmRole = prAgentInstallService.ProcessMessage(domainName + "\\" + userName, IsCurrentUserInLocalFarmAdminGroup(domainName, userName));
                            List<string> spServers = new List<string>();
                            if (AveEnv.IsSharePoint2007)
                            {
                                spServers = prAgentInstallService.GetAllSP2007Servers();
                            }
                            else
                            {
                                spServers = prAgentInstallService.GetAllSPServers();
                            }
                            foreach (string spServer in spServers)
                            {
                                mLog.Info("SPServer: {0}", spServer);
                            }
                            return new Tuple<FarmRoles, int>(farmRole, spServers.Count);
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("getting role in farm exception: {0}", ex.ToString());
                            if (DateTime.Now > deadline)
                            {
                                mLog.Error("getting role in farm failed before deadline.");
                                return new Tuple<FarmRoles, int>(FarmRoles.FAILED, 0);
                            }
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn(ex.ToString());
                return new Tuple<FarmRoles, int>(FarmRoles.FAILED, 0);
            }
        }

        public static bool IsQuickPlaceInstalled()
        {
            try
            {
                RegistryKey hasQuickPlace = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Lotus\QuickPlace");
                if (hasQuickPlace != null)
                {
                    hasQuickPlace.Close();
                    return true;
                }
                else
                {
                    RegistryKey hasQuickPlaceX64 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Lotus\QuickPlace");
                    if (hasQuickPlaceX64 != null)
                    {
                        hasQuickPlaceX64.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking QuickPlace installation. {0}", ex.ToString());
                return false;
            }
        }

        public static bool IsNotesInstalled()
        {
            try
            {
                RegistryKey hasNotes = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Lotus\Notes");
                if (hasNotes != null)
                {
                    hasNotes.Close();
                    return true;
                }
                else
                {
                    RegistryKey hasNotesX64 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Lotus\Notes");
                    if (hasNotesX64 != null)
                    {
                        hasNotesX64.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking Notes installation. {0}", ex.ToString());
                return false;
            }
        }

        public static bool IsERoomInstalled()
        {
            bool eRoomInstalled = false;
            try
            {
                RegistryKey productsReg = Registry.ClassesRoot.OpenSubKey(@"Installer\Products");
                foreach (string subKeyName in productsReg.GetSubKeyNames())
                {
                    RegistryKey subNode = productsReg.OpenSubKey(subKeyName);
                    string productName = (string)subNode.GetValue("ProductName");
                    if (string.Compare(productName, "eRoom Server", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        subNode.Close();
                        eRoomInstalled = true;
                        break;
                    }
                    subNode.Close();
                }
                productsReg.Close();

                if (!eRoomInstalled)
                {
                    RegistryKey uninstallReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                    foreach (string subKeyName in uninstallReg.GetSubKeyNames())
                    {
                        RegistryKey subNode = uninstallReg.OpenSubKey(subKeyName);
                        string productName = (string)subNode.GetValue("DisplayName");
                        if (string.Compare(productName, "eRoom Server", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            subNode.Close();
                            eRoomInstalled = true;
                            break;
                        }
                        subNode.Close();
                    }
                    uninstallReg.Close();
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while checking eRoom installation. {0}", e.ToString());
            }
            return eRoomInstalled;
        }

        public static bool IsJSharp2Installed()
        {
            bool jSharp2Installed = false;
            try
            {
                RegistryKey productKey = Registry.ClassesRoot.OpenSubKey(@"Installer\Products\34053A86A55C7324889C73EEC136DE17");
                if (productKey != null)
                {
                    string productName = Convert.ToString(productKey.GetValue("ProductName"));
                    if (productName.Contains("Microsoft Visual J# 2.0 Redistributable Package"))
                    {
                        jSharp2Installed = true;
                    }
                    productKey.Close();
                }
                else
                {
                    productKey = Registry.ClassesRoot.OpenSubKey(@"Installer\Products\B2D3AAFD7807E46428B337A8322CC972");
                    if (productKey != null)
                    {
                        string productName = Convert.ToString(productKey.GetValue("ProductName"));
                        if (productName.Contains("Microsoft Visual J# 2.0 Redistributable Package - SE"))
                        {
                            jSharp2Installed = true;
                        }
                        productKey.Close();
                    }
                }
                if (!jSharp2Installed)
                {
                    RegistryKey reg = Registry.ClassesRoot.OpenSubKey(@"Installer\Products");
                    foreach (string keyName in reg.GetSubKeyNames())
                    {
                        RegistryKey soft = reg.OpenSubKey(keyName);
                        if (soft.GetValue("ProductName").ToString().Contains("Microsoft Visual J# 2.0 Redistributable Package"))
                        {
                            jSharp2Installed = true;
                        }
                        soft.Close();
                    }
                    reg.Close();
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking J# 2.0  {0}", ex.ToString());
            }
            return jSharp2Installed;
        }

        public static bool IsDocumentumInstalled()
        {
            try
            {
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey uninstall32x = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                RegistryKey uninstall64x = localMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                if (HasDocumentum(uninstall32x) || HasDocumentum(uninstall64x))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while checking EMC Documentum installation. {0}", ex.ToString());
            }
            return false;
        }

        private static bool HasDocumentum(RegistryKey uninstall)
        {
            RegistryKey subKey;
            string productname;
            if (uninstall != null)
            {
                foreach (string subKeyName in uninstall.GetSubKeyNames())
                {
                    subKey = uninstall.OpenSubKey(subKeyName);
                    productname = (string)subKey.GetValue("DisplayName");
                    if (!string.IsNullOrEmpty(productname) && productname.Equals("Documentum DFC Runtime Environment", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
