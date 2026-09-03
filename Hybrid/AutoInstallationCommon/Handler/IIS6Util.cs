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
using System.Collections;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AutoInstallationCommon.Utility.Handler
{
    public class IIS6Util : IiiSUtil
    {
        public static readonly string AspNetIISExeName = "aspnet_regiis.exe";
        public static readonly string MinimizedAspNetVersion = "2.0.50727.0";

        public void ChangeCertificate(string webSiteName, string certHashString)
        {
            throw new NotImplementedException();
        }

        public void DeleteApplicationPool(string appPoolName)
        {
            if (IsApplicationPoolEmpty(appPoolName)) return;
            DirectoryEntry apppools = null;
            try
            {
                apppools = new DirectoryEntry("IIS://Localhost/W3SVC/AppPools");
                var apppool = apppools.Children.Find(appPoolName, "IIsApplicationPool");
                apppools.Children.Remove(apppool);
                apppools.CommitChanges();
            }
            finally
            {
                if (apppools != null) apppools.Close();
            }
        }

        public void DeleteWebSite(string webSiteName)
        {
            DirectoryEntry root = null;
            try
            {
                root = new DirectoryEntry("IIS://localhost/W3SVC");
                DirectoryEntry currentSite = null;
                foreach (DirectoryEntry child in root.Children)
                    if (child.SchemaClassName == "IIsWebServer")
                    {
                        var serverComment = child.Properties["ServerComment"][0].ToString();
                        if (string.Equals(serverComment, webSiteName, StringComparison.OrdinalIgnoreCase))
                        {
                            currentSite = child;
                            break;
                        }
                    }

                root.Children.Remove(currentSite);
                root.CommitChanges();
            }
            finally
            {
                if (root != null) root.Close();
            }
        }

        public void CreateApplicationPool(string appPoolName, string username, string pwd, string framework = "v2.0")
        {
            if (ExistApplicationPool(appPoolName))
                throw new Exception(string.Format("Application pool named {0} already exists.", appPoolName));
            if (string.IsNullOrEmpty(username))
                throw new Exception("Username specified for application pool  is empty.");
            if (string.IsNullOrEmpty(pwd)) throw new Exception("Password specified for application pool  is empty.");
            AssignUserPermission(username);
            var apppools = new DirectoryEntry("IIS://Localhost/W3SVC/AppPools");
            var newpool = apppools.Children.Add(appPoolName, "IIsApplicationPool");
            newpool.InvokeSet("AppPoolIdentityType", 3); //specify a user
            newpool.InvokeSet("WAMUserName", username);
            newpool.InvokeSet("WAMUserPass", pwd);
            newpool.CommitChanges();
            AllowWebServiceExtension();
        }

        public void CreateWebSite(string webSiteName, string webSiteHostHeader, string appPoolName, string schema,
            int port, string certHashString, string physicalPath, bool isExistingSite, string username, string password,
            bool anonymousAuthentication, bool windowsAuthentication)
        {
            if (!ExistApplicationPool(appPoolName))
                throw new Exception(string.Format("Application pool named {0} doesn't exist.", appPoolName));
            if (ExistWebSite(webSiteName) && !isExistingSite)
                throw new Exception(string.Format("Web site named {0} already exists.", webSiteName));

            var w3svc = new DirectoryEntry("IIS://localhost/W3SVC");
            var sites = w3svc.Children;

            DirectoryEntry site = null;
            // Find unused ID value for new web site
            var siteID = 1;
            foreach (DirectoryEntry child in sites)
                if (isExistingSite)
                {
                    if (child.SchemaClassName.Equals("IISWebServer", StringComparison.OrdinalIgnoreCase))
                    {
                        var siteName = child.Properties["ServerComment"][0].ToString();
                        if (siteName.Equals(webSiteName, StringComparison.OrdinalIgnoreCase))
                        {
                            site = child;
                            siteID = int.Parse(child.Name);
                        }
                    }
                }
                else
                {
                    if (child.SchemaClassName.Equals("IISWebServer", StringComparison.OrdinalIgnoreCase))
                    {
                        var serverId = int.Parse(child.Name);
                        if (serverId >= siteID) siteID = serverId + 1;
                    }
                }

            var siteVDir = default(DirectoryEntry);

            // Create web new site
            if (!isExistingSite)
            {
                site = sites.Add(siteID.ToString(), "IIsWebServer");
                site.Invoke("Put", "ServerComment", webSiteName);
                site.CommitChanges();
                siteVDir = site.Children.Add("Root", "IISWebVirtualDir");
                siteVDir.Properties["Path"][0] = physicalPath;
            }
            else
            {
                // use existing web site
                siteVDir = site.Children.Find("Root", "IISWebVirtualDir");
                siteVDir.Properties["Path"].Value = physicalPath;
            }


            // Create root application virtual directory
            siteVDir.Properties["AppFriendlyName"][0] = "Records";
            siteVDir.Properties["AppPoolId"][0] = appPoolName;
            //enable directory permission,script source access default false??
            siteVDir.Properties["AccessRead"][0] = true;
            siteVDir.Properties["AccessWrite"][0] = false;
            siteVDir.Properties["EnableDirBrowsing"][0] = false;
            siteVDir.Properties["DontLog"][0] = false;
            siteVDir.Properties["ContentIndexed"][0] = false;
            siteVDir.Properties["AccessScript"][0] = true;
            siteVDir.Properties["AccessExecute"][0] = false;
            //enable anonymous access & windows integrated access
            if (windowsAuthentication && anonymousAuthentication)
                siteVDir.Properties["AuthFlags"].Value = 5;
            else if (windowsAuthentication)
                siteVDir.Properties["AuthFlags"].Value = 4;
            else if (anonymousAuthentication) siteVDir.Properties["AuthFlags"].Value = 1;
            siteVDir.Properties["AnonymousUserName"][0] = username;
            siteVDir.Properties["AnonymousUserPass"][0] = password;
            if (string.Compare(schema, "https", StringComparison.OrdinalIgnoreCase) == 0)
                siteVDir.Properties["AccessSSLFlags"].Value = 8;
            siteVDir.Invoke("AppCreate", 1);
            siteVDir.CommitChanges();

            //change target framework to V2.0
            siteVDir = site.Children.Find("Root", "IISWebVirtualDir");
            var scriptMapVals = siteVDir.Properties["ScriptMaps"];
            var objScriptMaps = new ArrayList();
            //string frameworkVersion = "2.0.50727";
            var versionRegex = new Regex(@"(?<=\\v)\d{1}\.\d{1}\.\d{1,5}(?=\\)");

            objScriptMaps.AddRange(scriptMapVals);
            objScriptMaps.Add(GetXmlHandlerMappingString());

            siteVDir.Properties["ScriptMaps"].Value = objScriptMaps.ToArray();
            siteVDir.CommitChanges();
            site.CommitChanges();
            UpdateASPNetScriptMappings(siteID);
            RunIISCertDeployVbs("docave.pfx", "a!v@e#p$o%i^n&t", siteID, port);
            ApplyMimeType(siteID);
        }

        public void StartApplicationPool(string appPoolName)
        {
            using (var entry = GetAppPool(appPoolName))
            {
                ///AppPoolState:
                ///1: starting
                ///2: started
                ///3: stopping
                ///4: stopped
                var state = entry.InvokeGet("AppPoolState").ToString();
                if (!state.Equals("1", StringComparison.OrdinalIgnoreCase) &&
                    !state.Equals("2", StringComparison.OrdinalIgnoreCase)) entry.Invoke("Start");
            }
        }

        public void StartWebSite(string webSiteName)
        {
            using (var entry = GetWebSite(webSiteName))
            {
                ///ServerState:
                ///1 (starting)
                ///2 (started)
                ///3 (stopping)
                ///4 (stopped)
                ///5 (pausing)
                ///6 (paused)
                ///7 (continuing)
                var state = entry.InvokeGet("ServerState").ToString();
                if (!state.Equals("1", StringComparison.OrdinalIgnoreCase) &&
                    !state.Equals("2", StringComparison.OrdinalIgnoreCase)) entry.Invoke("Start");
            }
        }

        public void StopApplicationPool(string appPoolName)
        {
            using (var entry = GetAppPool(appPoolName))
            {
                ///AppPoolState:
                ///1: starting
                ///2: started
                ///3: stopping
                ///4: stopped
                var state = entry.InvokeGet("AppPoolState").ToString();
                if (!state.Equals("3", StringComparison.OrdinalIgnoreCase) &&
                    !state.Equals("4", StringComparison.OrdinalIgnoreCase)) entry.Invoke("Stop");
            }
        }

        public void StopWebSite(string webSiteName)
        {
            using (var entry = GetWebSite(webSiteName))
            {
                ///ServerState:
                ///1 (starting)
                ///2 (started)
                ///3 (stopping)
                ///4 (stopped)
                ///5 (pausing)
                ///6 (paused)
                ///7 (continuing)
                var state = entry.InvokeGet("ServerState").ToString();
                if (!state.Equals("3", StringComparison.OrdinalIgnoreCase) &&
                    !state.Equals("4", StringComparison.OrdinalIgnoreCase)) entry.Invoke("Stop");
            }
        }

        public void SetWebSiteAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication,
            string webSiteName)
        {
            //TODO:
        }

        public void SetFolderAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication,
            string webSiteName, string folderPath)
        {
            //TODO:
        }

        private bool IsApplicationPoolEmpty(string appPoolName)
        {
            var w3svc = new DirectoryEntry("IIS://localhost/W3SVC");
            var sites = w3svc.Children;

            foreach (DirectoryEntry child in sites)
                if (child.SchemaClassName.Equals("IISWebServer", StringComparison.OrdinalIgnoreCase))
                    if (appPoolName.Equals(child.Properties["AppPoolId"][0].ToString(),
                        StringComparison.OrdinalIgnoreCase))
                        return true;
            return false;
        }

        public bool ExistApplicationPool(string appPoolName)
        {
            DirectoryEntry apppools = null;
            try
            {
                apppools = new DirectoryEntry("IIS://Localhost/W3SVC/AppPools");
                apppools.Children.Find(appPoolName, "IIsApplicationPool");
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            finally
            {
                if (apppools != null) apppools.Close();
            }
        }

        public bool ExistWebSite(string webSiteName)
        {
            DirectoryEntry root = null;
            try
            {
                root = new DirectoryEntry("IIS://localhost/W3SVC");
                foreach (DirectoryEntry child in root.Children)
                    if (child.SchemaClassName == "IIsWebServer")
                    {
                        var serverComment = child.Properties["ServerComment"][0].ToString();
                        if (string.Equals(serverComment, webSiteName, StringComparison.OrdinalIgnoreCase)) return true;
                    }

                return false;
            }
            finally
            {
                if (root != null) root.Close();
            }
        }

        private void UpdateASPNetScriptMappings(int siteId)
        {
            var sitePath = "W3SVC/" + siteId;

            var exePath = GetAspNetExePath();

            StartProcess(exePath, "-s " + sitePath);
        }

        public void UpdateASPNetScriptMappings(string site)
        {
            var sitePath = site;
            var exePath = GetAspNetExePath();

            StartProcess(exePath, "-s " + sitePath);
        }

        private void RegisterASPNet()
        {
            var exePath = GetAspNetExePath();

            StartProcess(exePath, "-iru");
        }

        private void StartProcess(string fileName, string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
        }

        private string GetXmlHandlerMappingString()
        {
            return ".xml, " + GetAspDllFullPath() + ",5";
        }

        private string GetAspNetExePath()
        {
            return Path.Combine(GetAspNetPath(), AspNetIISExeName);
        }

        public bool IsWebSiteStarted(string webSiteName)
        {
            using (var entry = GetWebSite(webSiteName))
            {
                ///ServerState:
                ///1 (starting)
                ///2 (started)
                ///3 (stopping)
                ///4 (stopped)
                ///5 (pausing)
                ///6 (paused)
                ///7 (continuing)
                var state = entry.InvokeGet("ServerState").ToString();
                if (state.Equals("2", StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            }
        }

        public bool IsAppPoolStarted(string appPoolName)
        {
            using (var entry = GetAppPool(appPoolName))
            {
                ///AppPoolState:
                ///1: starting
                ///2: started
                ///3: stopping
                ///4: stopped
                var state = entry.InvokeGet("AppPoolState").ToString();
                if (state.Equals("2", StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            }
        }

        private void RunIISCertDeployVbs(string pfxPath, string password, int webSiteId, int sslPort)
        {
            //cscript IISCertDeploy.vbs -c docave.pfx -p a!v@e#p$o%i^n&t -i w3svc/1 -port :433: 
            var startInfo = new ProcessStartInfo("cscript");
            startInfo.Arguments =
                string.Format("IISCertDeploy.vbs -c \"{0}\" -p \"{1}\" -i w3svc/{2} -port :{3}: -q OFF", pfxPath,
                    password, webSiteId, sslPort);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;

            var p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            var output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
        }

        private void ApplyMimeType(int siteId)
        {
            var root = new DirectoryEntry("IIS://localhost/W3SVC/" + siteId);
            var mime = root.Properties["MimeMap"];
            var ext1 = ".xap";
            var type1 = "application/x-silverlight-app";
            var ext2 = ".xbap";
            var type2 = "application/x-ms-xbap";
            var ext3 = ".xaml";
            var type3 = "application/xaml+xml";

            //MimeMapClass newMime1 = new MimeMapClass();
            //newMime1.Extension = ext1;
            //newMime1.MimeType = type1;
            //mime.Add(newMime1);
            //MimeMapClass newMime2 = new MimeMapClass();
            //newMime2.Extension = ext2;
            //newMime2.MimeType = type2;
            //mime.Add(newMime2);
            //MimeMapClass newMime3 = new MimeMapClass();
            //newMime3.Extension = ext3;
            //newMime3.MimeType = type3;
            //mime.Add(newMime3);

            root.CommitChanges();
        }

        private void AllowWebServiceExtension()
        {
            var aspPath = GetAspDllFullPath();
            var startInfo = new ProcessStartInfo("cscript");
            startInfo.Arguments = string.Format(Environment.SystemDirectory + "\\iisext.vbs /EnFile " + aspPath);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;

            var p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            var output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
        }

        private string GetAspDllFullPath()
        {
            var subKeyName = "DllFullPath";
            return GetAspNetRegistryValue(subKeyName);
        }

        private string GetAspNetPath()
        {
            var subKeyName = "Path";
            return GetAspNetRegistryValue(subKeyName);
        }

        private static string GetAspNetRegistryValue(string subKeyName)
        {
            using (var aspNetKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ASP.NET"))
            {
                var version = GetAspNetVersion(aspNetKey.GetValue("RootVer").ToString());

                using (var asp = aspNetKey.OpenSubKey(version))
                {
                    if (asp == null)
                        using (var asp20 = aspNetKey.OpenSubKey(MinimizedAspNetVersion))
                        {
                            return asp20.GetValue(subKeyName).ToString();
                        }

                    return asp.GetValue(subKeyName).ToString();
                }
            }
        }

        public static string GetAspNetVersion(string version)
        {
            //compare root version to "2.0.50727.0"
            if (string.Compare(version, MinimizedAspNetVersion, StringComparison.OrdinalIgnoreCase) > 0)
                return version;
            return MinimizedAspNetVersion;
        }

        private DirectoryEntry GetAppPool(string appPoolName)
        {
            using (var appEntry = new DirectoryEntry("IIS://localhost/w3svc/AppPools"))
            {
                return appEntry.Children.Find(appPoolName, "IIsApplicationPool");
            }
        }

        private DirectoryEntry GetWebSite(string webSiteName)
        {
            using (var entry = new DirectoryEntry("IIS://localhost/w3svc"))
            {
                foreach (DirectoryEntry child in entry.Children)
                    if (child.SchemaClassName.Equals("IIsWebServer"))
                    {
                        var value = child.Properties["ServerComment"].Value;
                        if (value != null)
                        {
                            var tmpStr = value.ToString();
                            if (tmpStr.Equals(webSiteName, StringComparison.OrdinalIgnoreCase)) return child;
                        }
                    }
            }

            throw new Exception("Can't find specified iis web site: " + webSiteName);
        }

        private void AssignUserPermission(string username)
        {
            try
            {
                var usernames = username.Split('\\');
                if (usernames[0].Equals(".", StringComparison.OrdinalIgnoreCase))
                    usernames[0] = Environment.MachineName;
                var domain = new DirectoryEntry("WinNT://" + usernames[0]);
                var user = domain.Children.Find(usernames[1], "User");
                var root = new DirectoryEntry("WinNT://" + Environment.MachineName);
                var group = root.Children.Find("IIS_WPG", "group");

                if (!IsUserInGroup(group, user)) //if user not in IIS_WPG
                    group.Invoke("Add", user.Path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //ignore
            }
        }

        private bool IsUserInGroup(DirectoryEntry group, DirectoryEntry user)
        {
            var members = group.Invoke("Members", null);
            foreach (var member in (IEnumerable) members)
            {
                var userInGroup = new DirectoryEntry(member);
                if (userInGroup.Path.Equals(user.Path, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        public void ChangeSitePort(string webSiteName, int port, string certThumbprint, bool isUserDefinedCert)
        {
            var site = GetWebSite(webSiteName);
            var siteId = int.Parse(site.Name);
            RunIISCertDeployVbs("Recordsapp.avepoint.com.pfx", "njGH12#$", siteId, port);
        }
    }
}