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
    using System.Collections.Generic;
    using System.Text;
    using System.DirectoryServices;
    using System.IO;
    using System.Security.AccessControl;
    using Microsoft.Win32;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography;
    using AvePoint.GCommon;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Security.Principal;
    using System.Reflection;
    using System.Configuration;
    using System.Xml;
    using AvePoint.GCommon.Utility;
    using System.ServiceProcess;
    using AvePoint.GCommon.Utility.I18N;
    using System.Diagnostics.CodeAnalysis;

    #endregion

    public class PermissionProvisionManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal const string DocAveGroupName = "DocAve Users";
        internal const string DocAveGroupDescription = "Members in this group are granted the right to use DocAve.";

        public static void EnsureDocAveLocalGroupAndPermissions(bool forcePermission)
        {
            try
            {
                if (!IsLocalGroupExists())
                {
                    EnsureDocAveLocalGroup(DocAveGroupName, DocAveGroupDescription);
                    forcePermission = true;
                }
                if (forcePermission)
                {
                    EnsureDirectoryRightsControl(AveEnv.AgentRootFolder, DocAveGroupName, FileSystemRights.Modify);

                    EnsureRegistryFullControl(@"SOFTWARE\AvePoint\DocAve6", DocAveGroupName);
                    EnsureRegistryFullControl(@"SOFTWARE\Network Appliance\SnapManager for SharePoint 8", DocAveGroupName);
                    EnsureRegistryFullControl(@"SOFTWARE\IBM\SnapManager for SharePoint 8", DocAveGroupName);

                    //EnsureRegistryFullControl(@"SYSTEM\CurrentControlSet\Services\EventLog", DocAveGroupName);
                    EnsureCertificatePrivateKeyFullControl(AveEnv.AgentWcfThumbprint, DocAveGroupName);
                    if (IsLocalGroupExists("WSS_WPG"))
                    {
                        //lazy start run under app pool user, so ,give WSS_WPG permission on certificate
                        EnsureCertificatePrivateKeyFullControl(AveEnv.AgentWcfThumbprint, "WSS_WPG");
                    }
                    EnsureUserRights(DocAveGroupName, "SeBatchLogonRight");
                    string allowLogonLocally = ConfigurationManager.AppSettings["allowLogonLocally"];
                    if (!string.IsNullOrEmpty(allowLogonLocally) && string.Compare(allowLogonLocally, "true", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        EnsureUserRights(DocAveGroupName, "SeInteractiveLogonRight");
                    }

                    //AddHTTPUrlAcl("http://+:14008/", DocAveGroupName);
                    //AddHTTPUrlAcl("http://+:14009/", DocAveGroupName);
                }
            }
            catch (Exception e)
            {
                logger.Warn("ensure local group failed:{0}.", e.ToString());
            }
        }

        /// <summary>
        /// 主要是处理一些安装或者打patch或者修改证书需要操作的内容。
        /// </summary>
        public static void EnsureExtenalActionAfterInstall()
        {
            try
            {
                AvePoint.GCommon.Transfer.Common.DataTransferGlobalConfig.UpdateWcfThumbprint(AveEnv.AgentWcfThumbprint);
                //CommonPackageTransfer.TransferCore.Common.TransferConfiguration.UpdateWcfThumbprint(AveEnv.AgentWcfThumbprint);
                HttpApi.AddHttpsAclUrl(AvePoint.GCommon.Transfer.Common.DataTransferGlobalConfig.DataTransferConfiguration.HttpModePort, DocAveGroupName);
                HttpApi.BindCertificate("0.0.0.0", AvePoint.GCommon.Transfer.Common.DataTransferGlobalConfig.DataTransferConfiguration.HttpModePort, AveEnv.AgentWcfThumbprint, true);
                Invoker.AddTypeSearchAssembly(Assembly.LoadFile(Path.Combine(AveEnv.AgentBinFolder, "CommonPackageTransfer.dll")));
                //Type packageTransfer = Invoker.GetType("CommonPackageTransfer.TransferCore.Common.TransferConfiguration");
                int port = (int)Invoker.GetStaticProperty("CommonPackageTransfer.TransferCore.Common.TransferConfiguration", "Port");
                Invoker.CallStaticMethod("CommonPackageTransfer.TransferCore.Common.TransferConfiguration", "UpdateWcfThumbprint", new object[] { AveEnv.AgentWcfThumbprint });
                HttpApi.AddHttpsAclUrl(port, DocAveGroupName);
                HttpApi.BindCertificate("0.0.0.0", port, AveEnv.AgentWcfThumbprint, true);
            }
            catch (Exception e)
            {
                logger.Warn("ensure external action failed: {0}.", e.ToString());
            }
        }

        /// <summary>
        /// Add HTTP url acl
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userNameOrGroupName"></param>
        public static void AddHTTPUrlAcl(string url, string userNameOrGroupName)
        {
            try
            {
                using (HttpApi httpApi = new HttpApi())
                {
                    var coll = httpApi.QueryHttpNamespaceAcls();

                    AvePoint.GCommon.Security.AccessControl.SecurityDescriptor securityDescriptor = null;
                    AvePoint.GCommon.Security.AccessControl.SecurityIdentity sid = AvePoint.GCommon.Security.AccessControl.SecurityIdentity.SecurityIdentityFromName(userNameOrGroupName);

                    if (coll.TryGetValue(url, out securityDescriptor))
                    {
                        AvePoint.GCommon.Security.AccessControl.AccessControlEntry entry = null;

                        if (securityDescriptor.DACL != null)
                        {
                            logger.Info("The acl detail info of url:{0} is {1}", url, securityDescriptor.DACL.DetailInfo());
                        }
                        logger.Info("The current acl of url:{0} is {1}", url, securityDescriptor);

                        foreach (var item in securityDescriptor.DACL)
                        {
                            if (item.AccountSID.SID.Equals(sid.SID, StringComparison.OrdinalIgnoreCase))
                            {
                                entry = item;
                                break;
                            }
                        }

                        if (entry == null)
                        {
                            entry = new GCommon.Security.AccessControl.AccessControlEntry(sid);
                            entry.AceType = AceType.AccessAllowed;
                            entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);

                            securityDescriptor.DACL.Add(entry);
                            logger.Info("Remove the url:{0} and add it with acl:{1}", url, securityDescriptor);
                            httpApi.RemoveHttpHamespaceAcl(url);
                            httpApi.SetHttpNamespaceAcl(url, securityDescriptor);
                        }
                        else if (!entry.Contains(GCommon.Security.AccessControl.AceRights.GenericExecute))
                        {
                            entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);
                            logger.Info("Remove the url:{0} and add it with acl:{1}", url, securityDescriptor);
                            httpApi.RemoveHttpHamespaceAcl(url);
                            httpApi.SetHttpNamespaceAcl(url, securityDescriptor);
                        }
                    }
                    else
                    {
                        AvePoint.GCommon.Security.AccessControl.AccessControlEntry entry = new GCommon.Security.AccessControl.AccessControlEntry(sid);
                        entry.AceType = AceType.AccessAllowed;
                        entry.Add(GCommon.Security.AccessControl.AceRights.GenericExecute);

                        securityDescriptor = new GCommon.Security.AccessControl.SecurityDescriptor();
                        securityDescriptor.DACL = new GCommon.Security.AccessControl.AccessControlList();
                        securityDescriptor.DACL.Add(entry);

                        httpApi.SetHttpNamespaceAcl(url, securityDescriptor);
                        logger.Info("Add http namespace:{0} with acl:{1}", url, securityDescriptor);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Cannot add urlAcl for:{0} with account:{1}, details:{2}", url, userNameOrGroupName, ex);
            }
        }

        public static void EnsureUserInLocalGroups(String domain, String username)
        {
            var ensureLocalGroups = new List<String> { 
                "IIS_IUSRS",  //IIS7
                "IIS_WPG", //IIS6
                "Performance Monitor Users", 
                "DocAve Users",
                "WSS_WPG", 
                //"WSS_ADMIN_WPG", 
                //If a user is in WSS_ADMIN_WPG group, it will be a farm administrator by default.
                //We remove it from here for future use. 
                "WSS_RESTRICTED_WPG_V4",
                "Backup Operators",};

            ensureLocalGroups.ForEach(group =>
            {
                try
                {
                    if (PermissionProvisionManager.IsLocalGroupExists(group))
                    {
                        PermissionProvisionManager.AddDomainUserToLocalGroup(domain, username, group);
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Warn("Add domain user:{0}\\{1} to local group:{2} failed:{3}.", domain, username, group, ex.ToString());
                }
            });
            //ADO-36342，将DocAve Users组添加到SMSvcHost.exe.config中，防止Windows 2003系统出现host wcf服务权限不足的错误。
            EnsureWCFServiceHostRights(DocAveGroupName, domain, username);
        }

        public static bool IsLocalGroupExists(string groupName = DocAveGroupName)
        {
            String groupPath = String.Format("WinNT://{0}/{1},group", Environment.MachineName, groupName);
            DirectoryEntry theGroup = new DirectoryEntry(groupPath);
            try
            {
                if (theGroup.SchemaClassName != "Group")
                {
                    return false;
                }
            }
            catch (COMException ex)
            {
                logger.Debug("Expected exception while checking group {0}. Exception: {1}", groupName, ex.Message);
                return false;
            }
            return true;
        }

        public static bool IsDomainUserInLocalGroup(string domainName, string userName, string localGroupName)
        {
            bool isSpecialUserInLocalGroup = false;
            try
            {
                WindowsIdentity identity = new WindowsIdentity(string.Format("{0}@{1}", userName, domainName));
                WindowsPrincipal wp = new WindowsPrincipal(identity);
                if (wp.IsInRole(localGroupName))
                {
                    isSpecialUserInLocalGroup = true;
                }
            }
            catch (Exception e)
            {
                //it will cause exception out of domain environment. we will continue check using WMI below
                logger.Debug("An error occurred while checking user/group relationship. {0}", e.ToString());
            }
            if (!isSpecialUserInLocalGroup)
            {
                try
                {
                    isSpecialUserInLocalGroup = IsUserInLocalGroup(localGroupName, userName);
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while checking user/group relationship. {0}", ex.ToString());
                }
            }
            return isSpecialUserInLocalGroup;
        }

        static bool IsUserInLocalGroup(string groupName, string userName)
        {
            using (DirectoryEntry groupEntry = new DirectoryEntry(string.Format("WinNT://./{0},group", groupName)))
            {
                foreach (object member in (IEnumerable)groupEntry.Invoke("Members"))
                {
                    using (DirectoryEntry memberEntry = new DirectoryEntry(member))
                    {
                        if (string.Compare(memberEntry.Name, userName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static void AddDomainUserToLocalGroup(string domain, string userName, string groupName = DocAveGroupName)
        {
            if (IsDomainUserInLocalGroup(domain, userName, groupName))
            {
                logger.Info("domain user:{0}\\{1} already exists in local group:{2}.", domain, userName, groupName);
                return;
            }

            String groupPath = String.Format("WinNT://{0}/{1},group", Environment.MachineName, groupName);
            DirectoryEntry theGroup = new DirectoryEntry(groupPath);
            String userPath = String.Format("WinNT://{0}/{1},user", domain, userName);
            if (string.Compare(domain, ".", StringComparison.OrdinalIgnoreCase) == 0)
            {
                userPath = String.Format("WinNT://{0}/{1},user", Environment.MachineName, userName);
            }
            theGroup.Invoke("Add", new object[] { userPath });
            theGroup.CommitChanges();
            logger.Info("Added domain user:{0}\\{1} to local group:{2}.", domain, userName, groupName);
        }

        static void EnsureDocAveLocalGroup(string groupName, string description)
        {
            var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            try
            {
                //check if group exist, exception will occur if not exist.
                ad.Children.Find(groupName, "group");
            }
            catch (Exception e)
            {
                logger.Debug("can not find group. {0}", e.ToString());
                logger.Info("Creating local group:{0} description: {1}", groupName, description);
                DirectoryEntry newGroup = ad.Children.Add(groupName, "group");
                newGroup.Invoke("Put", new object[] { "Description", description });
                newGroup.CommitChanges();
            }
        }

        static void EnsureDirectoryRightsControl(string directoryName, string accountName, FileSystemRights right)
        {
            try
            {
                logger.Info("Assigning file system right control permission for [{0}] on {1} , rights: {2}", accountName, directoryName, right.ToString());
                DirectorySecurity dSecurity = Directory.GetAccessControl(directoryName);
                dSecurity.AddAccessRule(new FileSystemAccessRule(accountName, right, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(directoryName, dSecurity);
                logger.Info("Assigning file system right control permission for [{0}] on {1} successfully.", accountName, directoryName);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while assigning file system right control permission for [{0}] on {1}. Exception: {2}", accountName, directoryName, ex.ToString());
            }
        }

        static void EnsureRegistryFullControl(string registryPath, string accountName)
        {
            try
            {
                logger.Debug("Assigning registry full control permission for [{0}] on {1}", accountName, registryPath);
                RegistryKey rkey = Registry.LocalMachine.OpenSubKey(registryPath, true);
                if (rkey == null)
                {
                    logger.Debug("Cannot find registry [{0}] ", registryPath);
                    return;
                }
                RegistrySecurity rSecurity = rkey.GetAccessControl(AccessControlSections.All);
                rSecurity.AddAccessRule(new RegistryAccessRule(accountName, RegistryRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                rkey.SetAccessControl(rSecurity);
                rkey.Close();
                logger.Info("Assigning registry full control permission for [{0}] on {1} successfully", accountName, registryPath);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while assigning registry full control permission for [{0}] on {1}. Exception: {2}", accountName, registryPath, ex.ToString());
            }
        }

        static void EnsureCertificatePrivateKeyFullControl(string certThumbprint, string accountName)
        {
            try
            {
                logger.Info("Assigning certificate full control permission for [{0}] on thumbprint {1}", accountName, certThumbprint);
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
                    if (col.Count > 0)
                    {
                        X509Certificate2 cert = col[0];
                        RSACryptoServiceProvider rsa = cert.PrivateKey as RSACryptoServiceProvider;
                        if (rsa != null)
                        {
                            string keyFileLocation = FindKeyFileLocation(rsa.CspKeyContainerInfo.UniqueKeyContainerName);
                            string keyFilePath = Path.Combine(keyFileLocation, rsa.CspKeyContainerInfo.UniqueKeyContainerName);
                            FileSecurity fSecurity = File.GetAccessControl(keyFilePath);
                            fSecurity.AddAccessRule(new FileSystemAccessRule(accountName, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                            File.SetAccessControl(keyFilePath, fSecurity);
                        }
                        else
                        {
                            throw new Exception("The certificate doesn't have private key.");
                        }
                    }
                    else
                    {
                        throw new Exception("cannot find certificate by thumbprint: " + certThumbprint);
                    }
                }
                finally
                {
                    store.Close();
                }
                logger.Info("Assigning certificate full control permission for [{0}] on thumbprint {1} successfully", accountName, certThumbprint);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while assigning certificate full control permission for [{0}] on thumbprint {1}. Exception: {2}", accountName, certThumbprint, ex.ToString());
            }
        }

        static string FindKeyFileLocation(string keyFileName)
        {
            string commonAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string path1 = commonAppDataPath + @"\Microsoft\Crypto\RSA\MachineKeys";
            string[] files = Directory.GetFiles(path1, keyFileName);
            if (files.Length > 0)
            {
                return path1;
            }
            string appDatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path2 = appDatapath + @"\Microsoft\Crypto\RSA\";
            string[] directories = Directory.GetDirectories(path2);
            if (directories.Length > 0)
            {
                foreach (string dir in directories)
                {
                    files = Directory.GetFiles(dir, keyFileName);
                    if (files.Length != 0)
                    {
                        return dir;
                    }
                }
            }
            return "Private key exists but is not accessible";
        }

        static void EnsureUserRights(string accountName, string privilegeName)
        {
            try
            {
                logger.Info("Assigning user rights for [{0}] with {1}", accountName, privilegeName);
                LsaUtility.SetUserRights(accountName, privilegeName);
                logger.Info("Assigning user rights for [{0}] with {1} successfully", accountName, privilegeName);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while assigning user rights for [{0}] with {1}. Exception: {2}", accountName, privilegeName, ex.ToString());
            }
        }

        /// <summary>
        /// 由于Windows 2003系统下不存在IIS_IUSRS这个组，host WCF服务的时候会抛出Access Denied错误，需要在SMSvcHost.exe.config文件中将DocAve Users这个组的sid添加到AllowAccounts
        /// </summary>
        /// <param name="localGroupName">需要添加权限的组名称</param>

        [SuppressMessage("CheckHardCode", "Z100009:CheckString")]
        public static void EnsureWCFServiceHostRights(string localGroupName, string domain, string userName)
        {
            String path = String.Empty;
            try
            {
                //UAV enable了，说明是IIS7已经安装了，不用另外赋权限
                if (OSInformation.UACEnabled)
                {
                    logger.Debug("current operation system is after vista and must have IIS7 installed, do not need to add allow accounts");
                    return;
                }
                //如果user已经是local admin，肯定有host wcf的权限
                else if (IsDomainUserInLocalGroup(domain, userName, "Administrators"))
                {
                    logger.Debug("The domain user {0}\\{1} is in the local group Administrators, must have the permission to host WCF service", domain, userName);
                    return;
                }
                if (Directory.Exists(@"C:\Windows\Microsoft.NET\Framework64"))
                {
                    if (File.Exists(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\SMSvcHost.exe.config"))
                    {
                        path = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\SMSvcHost.exe.config";
                    }
                    else if (File.Exists(@"C:\Windows\Microsoft.NET\Framework64\v3.0\Windows Communication Foundation\SMSvcHost.exe.config"))
                    {
                        path = @"C:\Windows\Microsoft.NET\Framework64\v3.0\Windows Communication Foundation\SMSvcHost.exe.config";
                    }
                }
                else
                {
                    if (File.Exists(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\SMSvcHost.exe.config"))
                    {
                        path = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\SMSvcHost.exe.config";
                    }
                    else if (File.Exists(@"C:\Windows\Microsoft.NET\Framework\v3.0\Windows Communication Foundation\SMSvcHost.exe.config"))
                    {
                        path = @"C:\Windows\Microsoft.NET\Framework\v3.0\Windows Communication Foundation\SMSvcHost.exe.config";
                    }
                }
                if (String.IsNullOrEmpty(path))
                {
                    logger.Error("could not find net framework path above 3.0");
                    return;
                }
                //判断当前agent user的sid是否在配置文件中，如果已经存在，不需要再添加DocAve User这个group的sid
                NTAccount userAccount = new NTAccount(String.Format("{0}\\{1}", domain, userName));
                SecurityIdentifier userIdentifier = (SecurityIdentifier)userAccount.Translate(typeof(SecurityIdentifier));
                String userSid = userIdentifier.ToString();
                if (!String.IsNullOrEmpty(userSid) && IsAccountExist(path, userSid))
                {
                    logger.Debug("The user {0}\\{1} sid has been added in the file {2}, sid is {3}", domain, userSid, path, userSid);
                    return;
                }
                //将DocAve Users这个group的sid添加到配置文件中，获取host wcf service的权限
                NTAccount groupAccount = new NTAccount(localGroupName);
                SecurityIdentifier groupIdentifier = (SecurityIdentifier)groupAccount.Translate(typeof(SecurityIdentifier));
                String groupSid = groupIdentifier.ToString();
                if (!String.IsNullOrEmpty(groupSid))
                {
                    if (IsAccountExist(path, groupSid))
                    {
                        logger.Debug("The group {0} sid has been added in the file {1}, sid is {2}", localGroupName, path, groupSid);
                        return;
                    }
                    else
                    {
                        AddAllowAccounts(path, groupSid);
                    }
                }
                else
                {
                    logger.Debug("Could not get the group {0} sid", localGroupName);
                    return;
                }
                //2003系统只有NetTcpPortSharing这个service，没有depend service或者被其他service depend on，可以直接重启,但是也需要判断
                ServiceController service = new ServiceController("NetTcpPortSharing");
                if (service.DependentServices.Length == 0)
                {
                    logger.Info("start to restart NetTcpPortSharing service");
                    TimeSpan timeout = TimeSpan.FromMinutes(2);
                    try
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        EventIds.Service.StoppedSuccessfullyEventMessage eventMessage =
                            new EventIds.Service.StoppedSuccessfullyEventMessage(ContextValues.Service.ServiceType.NetTcpPortSharingService);
                        logger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_AgentService, eventMessage);
                    }
                    catch (Exception e)
                    {
                        EventIds.Service.StoppedFailedEventMessage eventMessage =
                            new EventIds.Service.StoppedFailedEventMessage(ContextValues.Service.ServiceType.NetTcpPortSharingService, e);
                        logger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_AgentService, eventMessage);
                        throw;
                    }
                    try
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                        EventIds.Service.StartedSuccessfullyEventMessage eventMessage =
                            new EventIds.Service.StartedSuccessfullyEventMessage(ContextValues.Service.ServiceType.NetTcpPortSharingService);
                        logger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_AgentService, eventMessage);
                    }
                    catch (Exception e)
                    {
                        EventIds.Service.StartedFailedEventMessage eventMessage =
                            new EventIds.Service.StartedFailedEventMessage(ContextValues.Service.ServiceType.NetTcpPortSharingService, e);
                        logger.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_AgentService, eventMessage);
                        throw;
                    }
                    logger.Info("restart NetTcpPortSharing service successfully");
                }
                else
                {
                    StringBuilder eventLogString = new StringBuilder();
                    eventLogString.AppendLine(String.Format("Restart service is {0}, display name is {1}", service.ServiceName, service.DisplayName));
                    foreach (ServiceController dependentService in service.DependentServices)
                    {
                        eventLogString.AppendLine(String.Format("Dependent services contain {0}, display name is {1}", dependentService.ServiceName, dependentService.DisplayName));
                    }
                    logger.Warn("We need to restart service and its dependent service manually:{0}", eventLogString.ToString());
                }
            }
            catch (Exception e)
            {
                logger.Warn("Error occurs while Ensure WCF Service Host Rights for Group {0} :{1}, please make sure the group sid is in the allow accounts of the file :{2}", localGroupName, e.ToString(), path);
            }
        }

        /// <summary>
        /// 判断配置文件中是否存在指定的sid
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        private static bool IsAccountExist(string path, string sid)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement activationElement = GetXmlElementChild(doc.DocumentElement, "system.serviceModel.activation");
            if (activationElement == null)
            {
                return false;
            }
            XmlElement tcpElement = GetXmlElementChild(activationElement, "net.tcp");
            if (tcpElement == null)
            {
                return false;
            }
            XmlElement accountElement = GetXmlElementChild(tcpElement, "allowAccounts");
            if (accountElement == null)
            {
                return false;
            }
            foreach (XmlNode node in accountElement.GetElementsByTagName("add"))
            {
                if (((XmlElement)node).HasAttribute("securityIdentifier"))
                {
                    String identifier = node.Attributes["securityIdentifier"].Value;
                    if (!String.IsNullOrEmpty(identifier) && identifier.Equals(sid, StringComparison.OrdinalIgnoreCase))
                    {
                        return true; ;
                    }
                }
                else
                {
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// 在配置文件中添加一个sid，如果已经存在则退出
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sid"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "teredoEnabled is a key")]
        private static void AddAllowAccounts(string path, string sid)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement activationElement = null;
            XmlElement tcpElement = null;
            XmlElement accountElement = null;
            XmlElement addElement = null;
            XmlElement diagnosticsElement = null;
            activationElement = GetXmlElementChild(doc.DocumentElement, "system.serviceModel.activation");
            if (activationElement == null)
            {
                activationElement = doc.CreateElement("system.serviceModel.activation");
                doc.DocumentElement.AppendChild(activationElement);
            }
            tcpElement = GetXmlElementChild(activationElement, "net.tcp");
            diagnosticsElement = GetXmlElementChild(activationElement, "diagnostics");
            if (tcpElement == null)
            {
                tcpElement = doc.CreateElement("net.tcp");
                tcpElement.SetAttribute("listenBacklog", "10");
                tcpElement.SetAttribute("maxPendingConnections", "100");
                tcpElement.SetAttribute("maxPendingAccepts", "2");
                tcpElement.SetAttribute("receiveTimeout", "00:00:10");
                tcpElement.SetAttribute("teredoEnabled", "false");
                activationElement.AppendChild(tcpElement);
            }
            if (diagnosticsElement == null)
            {
                diagnosticsElement = doc.CreateElement("diagnostics");
                diagnosticsElement.SetAttribute("performanceCountersEnabled", "true");
                activationElement.AppendChild(diagnosticsElement);
            }
            accountElement = GetXmlElementChild(tcpElement, "allowAccounts");
            if (accountElement == null)
            {
                accountElement = doc.CreateElement("allowAccounts");
                tcpElement.AppendChild(accountElement);
            }
            foreach (XmlNode node in accountElement.GetElementsByTagName("add"))
            {
                if (((XmlElement)node).HasAttribute("securityIdentifier"))
                {
                    String identifier = node.Attributes["securityIdentifier"].Value;
                    if (!String.IsNullOrEmpty(identifier) && identifier.Equals(sid, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Debug("SMSvcHost.exe.config has contains the sid :{0}", sid);
                        return;
                    }
                }
                else
                {
                    continue;
                }
            }
            if (addElement == null)
            {
                addElement = doc.CreateElement("add");
                addElement.SetAttribute("securityIdentifier", sid);
                accountElement.AppendChild(addElement);
            }
            doc.Save(path);
        }

        private static XmlElement GetXmlElementChild(XmlElement element, string childName)
        {
            XmlElement childElement = null;
            XmlNodeList matchedElementList = element.GetElementsByTagName(childName);
            if (matchedElementList.Count > 0)
            {
                childElement = matchedElementList[0] as XmlElement;
            }
            return childElement;
        }
    }
}
