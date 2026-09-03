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
using System.Runtime.InteropServices;
using Microsoft.Web.Administration;

namespace AutoInstallationCommon.Utility.Handler
{
    public class IIS7Util : IiiSUtil
    {
        public void DeleteApplicationPool(string appPoolName)
        {
            var iisManager = new ServerManager();

            var isAppPoolEmpty = IsApplicationPoolEmpty(iisManager, appPoolName);

            if (!isAppPoolEmpty)
            {
            }
            else
            {
                var appPool = iisManager.ApplicationPools[appPoolName];
                if (appPool != null)
                {
                    iisManager.ApplicationPools.Remove(appPool);
                    iisManager.CommitChanges();
                }
            }
        }

        public void DeleteWebSite(string webSiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            if (site != null)
            {
                iisManager.Sites.Remove(site);
                iisManager.CommitChanges();
            }
        }

        public void CreateApplicationPool(string appPoolName, string username, string pwd, string framework = "v2.0")
        {
            if (ExistApplicationPool(appPoolName))
                throw new Exception(string.Format("Application pool named {0} already exists.", appPoolName));
            if (string.IsNullOrEmpty(username))
                throw new Exception("Username specified for application pool  is empty.");
            if (string.IsNullOrEmpty(pwd)) throw new Exception("Password specified for application pool  is empty.");
            var iisManager = new ServerManager();

            var appPool = iisManager.ApplicationPools.Add(appPoolName);

            ApplyAppPoolSettings(username, pwd, iisManager, appPool, framework);
        }

        public void CreateWebSite(string webSiteName, string webSiteHostHeader, string appPoolName, string schema,
            int port, string certHashString, string physicalPath, bool isExistingSite, string username, string password,
            bool anonymousAuthentication, bool windowsAuthentication)
        {
            if (ExistApplicationPool(appPoolName) == false)
                throw new Exception(string.Format("Application pool named {0} doesn't exist.", appPoolName));
            if (ExistWebSite(webSiteName) && !isExistingSite)
                throw new Exception(string.Format("Web site named {0} already exists.", webSiteName));
            var iisManager = new ServerManager();
            Site site = null;

            if (string.Compare(schema, "http", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (!isExistingSite)
                {
                    // create new site
                    site = iisManager.Sites.Add(webSiteName, schema, string.Format("*:{0}:", port), physicalPath);
                }
                else
                {
                    site = iisManager.Sites[webSiteName];
                    site.Bindings.Clear();
                    site.Bindings.Add(string.Format("*:{0}:", port), schema);
                    site.Applications["/"].VirtualDirectories["/"].PhysicalPath = physicalPath;
                }
            }
            else if (string.Compare(schema, "https", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var certHash = HashStringToByteArray(certHashString);

                if (!isExistingSite)
                {
                    site = iisManager.Sites.Add(webSiteName, string.Format("*:{0}:{1}", port, webSiteHostHeader),
                        physicalPath, certHash);
                }
                else
                {
                    site = iisManager.Sites[webSiteName];
                    site.Bindings.Clear();
                    site.Bindings.Add(string.Format("*:{0}:", port), certHash, null);
                    site.Applications["/"].VirtualDirectories["/"].PhysicalPath = physicalPath;
                }

                //enable require SSL
                var config = iisManager.GetApplicationHostConfiguration();
                var accessSection = config.GetSection("system.webServer/security/access", webSiteName);
                accessSection["sslFlags"] = @"Ssl";
            }
            else
            {
                throw new Exception("unrecognized schema: " + schema);
            }

            site.ServerAutoStart = true;
            site.Applications["/"].ApplicationPoolName = appPoolName;

            // enable anonymous authentication
            var iisConfig = iisManager.GetApplicationHostConfiguration();
            var anonymousAuthenticationSection =
                iisConfig.GetSection("system.webServer/security/authentication/anonymousAuthentication", webSiteName);
            anonymousAuthenticationSection["enabled"] = anonymousAuthentication;

            // set username to empty, IIS would use App pool user for anonymous authentication 
            anonymousAuthenticationSection["userName"] = "";

            //enable Windows authentication for SSO
            var windowsAuthenticationSection =
                iisConfig.GetSection("system.webServer/security/authentication/windowsAuthentication", webSiteName);
            windowsAuthenticationSection["enabled"] = windowsAuthentication;

            //

            //Microsoft.Web.Administration.ConfigurationSection handlersSection = iisConfig.GetSection("system.webServer/handlers");
            //handlersSection["accessPolicy"] = @"Read, Execute, Script";

            //Microsoft.Web.Administration.ConfigurationElementCollection handlersCollection = handlersSection.GetCollection();

            //Microsoft.Web.Administration.ConfigurationElement addElement = handlersCollection.CreateElement("add");
            //addElement["name"] = @"ADFS";
            //addElement["path"] = @"*.adfs";
            //addElement["verb"] = @"*";
            //addElement["requireAccess"] = @"Execute";
            //handlersCollection.Add(addElement);

            iisManager.CommitChanges();
        }

        public void ChangeCertificate(string webSiteName, string certHashString)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            if (site == null) throw new Exception(string.Format("Can not found web site:{0}.", webSiteName));

            var certHash = HashStringToByteArray(certHashString);
            var bind = site.Bindings[0];
            bind.CertificateHash = certHash;
            iisManager.CommitChanges();
        }

        public void SetFolderAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication,
            string webSiteName, string folderPath)
        {
            var iisManager = new ServerManager();
            var config = iisManager.GetApplicationHostConfiguration();
            var serviceFolderPath = webSiteName.TrimEnd('/');
            var anonymousAuthenticationSection =
                config.GetSection("system.webServer/security/authentication/anonymousAuthentication",
                    serviceFolderPath);
            anonymousAuthenticationSection["enabled"] = anonymousAuthentication;
            var windowsAuthenticationSection =
                config.GetSection("system.webServer/security/authentication/windowsAuthentication", serviceFolderPath);
            windowsAuthenticationSection["enabled"] = windowsAuthentication;

            iisManager.CommitChanges();
        }

        public void StartApplicationPool(string appPoolName)
        {
            var deadLine = DateTime.Now.AddSeconds(15);
            while (true)
                try
                {
                    var iisManager = new ServerManager();
                    var appPool = iisManager.ApplicationPools[appPoolName];
                    if (appPool.State != ObjectState.Starting
                        && appPool.State != ObjectState.Started)
                        appPool.Start();
                    break;
                }
                catch (COMException ex)
                {
                    //The object identifier does not represent a valid object. (Exception from HRESULT: 0x800710D8)
                    if (DateTime.Now > deadLine)
                    {
                        Console.WriteLine("An error occured while starting application pool. exception:{0}", ex);
                        break;
                    }
                }
        }

        public void StartWebSite(string webSiteName)
        {
            var deadLine = DateTime.Now.AddSeconds(15);
            while (true)
                try
                {
                    var iisManager = new ServerManager();
                    var site = iisManager.Sites[webSiteName];
                    if (site.State != ObjectState.Starting
                        && site.State != ObjectState.Started)
                        site.Start();
                    break;
                }
                catch (COMException ex)
                {
                    //The object identifier does not represent a valid object. (Exception from HRESULT: 0x800710D8)
                    if (DateTime.Now > deadLine)
                    {
                        Console.WriteLine("An error occured while starting web site. exception:{0}", ex);
                        break;
                    }
                }
        }

        public void StopApplicationPool(string appPoolName)
        {
            var deadLine = DateTime.Now.AddSeconds(15);
            while (true)
                try
                {
                    var iisManager = new ServerManager();
                    var appPool = iisManager.ApplicationPools[appPoolName];
                    if (appPool.State != ObjectState.Stopping
                        && appPool.State != ObjectState.Stopped)
                        appPool.Stop();
                    break;
                }
                catch (COMException ex)
                {
                    //The object identifier does not represent a valid object. (Exception from HRESULT: 0x800710D8)
                    if (DateTime.Now > deadLine)
                    {
                        Console.WriteLine("An error occured while stopping application pool. exception:{0}", ex);
                        break;
                    }
                }
        }

        public void StopWebSite(string webSiteName)
        {
            var deadLine = DateTime.Now.AddSeconds(15);
            while (true)
                try
                {
                    var iisManager = new ServerManager();
                    var site = iisManager.Sites[webSiteName];
                    if (site.State != ObjectState.Stopping
                        && site.State != ObjectState.Stopped)
                        site.Stop();
                    break;
                }
                catch (COMException ex)
                {
                    //The object identifier does not represent a valid object. (Exception from HRESULT: 0x800710D8)
                    if (DateTime.Now > deadLine)
                    {
                        Console.WriteLine("An error occured while stopping web site. exception:{0}", ex);
                        break;
                    }
                }
        }


        public void SetWebSiteAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication,
            string webSiteName)
        {
            var iisManager = new ServerManager();
            var config = iisManager.GetApplicationHostConfiguration();
            var anonymousAuthenticationSection =
                config.GetSection("system.webServer/security/authentication/anonymousAuthentication", webSiteName);
            anonymousAuthenticationSection["enabled"] = anonymousAuthentication;
            var windowsAuthenticationSection =
                config.GetSection("system.webServer/security/authentication/windowsAuthentication", webSiteName);
            windowsAuthenticationSection["enabled"] = windowsAuthentication;
            iisManager.CommitChanges();
        }

        /// <summary>
        ///     check if application pool has any application
        /// </summary>
        /// <returns></returns>
        private bool IsApplicationPoolEmpty(ServerManager iisManager, string appPoolName)
        {
            foreach (var site in iisManager.Sites)
            foreach (var application in site.Applications)
                if (appPoolName.Equals(application.ApplicationPoolName, StringComparison.OrdinalIgnoreCase))
                    return false;

            return true;
        }

        public bool ExistApplicationPool(string appPoolName)
        {
            var iisManager = new ServerManager();
            var appPool = iisManager.ApplicationPools[appPoolName];
            return appPool != null;
        }

        public bool ExistWebSite(string webSiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            return site != null;
        }

        public void ConfigExistingApplicationPool(string appPoolName, string username, string pwd)
        {
            using (var iisManager = new ServerManager())
            {
                var appPool = iisManager.ApplicationPools[appPoolName];

                if (!string.IsNullOrEmpty(username))
                {
                    appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
                    appPool.ProcessModel.UserName = username;
                    appPool.ProcessModel.Password = pwd;

                    appPool.ProcessModel.LoadUserProfile = true;

                    iisManager.CommitChanges();
                }
            }
        }

        private static void ApplyAppPoolSettings(string username, string pwd, ServerManager iisManager,
            ApplicationPool appPool, string framework)
        {
            appPool.AutoStart = true;
            appPool.ProcessModel.IdleTimeout = TimeSpan.FromMinutes(120);
            appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            appPool.ManagedRuntimeVersion = framework;
            appPool.ProcessModel.IdentityType = ProcessModelIdentityType.SpecificUser;
            appPool.ProcessModel.UserName = username;
            appPool.ProcessModel.Password = pwd;

            // add for SSO
            appPool.ProcessModel.LoadUserProfile = true;

            iisManager.CommitChanges();
        }

        public static byte[] HashStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public void ChangeSitePort(string webSiteName, int port, string certThumbprint, bool isUserDefinedCert)
        {
            using (var iisManager = new ServerManager())
            {
                var site = iisManager.Sites[webSiteName];

                site = iisManager.Sites[webSiteName];
                byte[] certHash = null;

                if (site.Bindings.Count > 0)
                {
                    certHash = site.Bindings[0].CertificateHash;
                    site.Bindings.Clear();
                    site.Bindings.Add(string.Format("*:{0}:", port), certHash, null);

                    iisManager.CommitChanges();
                }
            }
        }

        public bool IsWebSiteStarted(string webSiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            if (site.State == ObjectState.Started) return true;
            return false;
        }

        public bool IsAppPoolStarted(string appPoolName)
        {
            var iisManager = new ServerManager();
            var appPool = iisManager.ApplicationPools[appPoolName];
            if (appPool.State == ObjectState.Started) return true;
            return false;
        }


        public void CreateGAVirtualDirectory(string webSiteName, string virtualDirectory, string appPoolName,
            string physicalPath, string certHashString)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            var certHash = HashStringToByteArray(certHashString);

            site.Applications.Add(virtualDirectory, physicalPath);

            site.Applications[virtualDirectory].ApplicationPoolName = appPoolName;

            iisManager.CommitChanges();
        }

        public string[] GetDocAveWebSitePlusAppPoolName(string physicalPath)
        {
            string[] docaveWebSite = {"", ""};
            var iisManager = new ServerManager();
            foreach (var site in iisManager.Sites)
                try
                {
                    if (string.Equals(site.Applications[0].VirtualDirectories[0].PhysicalPath, physicalPath,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        docaveWebSite[0] = site.Name;
                        docaveWebSite[1] = site.Applications[0].ApplicationPoolName;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to get web site & app pool name. " + e);
                }

            return docaveWebSite;
        }

        #region IISUtil Members

        public void DeleteGAWebSite(string webSiteName)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[webSiteName];
            var app = iisManager.Sites[webSiteName].Applications["/GA"];
            if (site != null)
            {
                site.Applications.Remove(app);
                iisManager.CommitChanges();
            }
        }

        #endregion
    }
}