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
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;

namespace AvePoint.Item.Restore
{
    internal class AveStsadmWeb : StsadmOperation
    {
        private string siteUrl;
        private string ownerLogin;
        public bool? IsWebExist { get; private set; }

        public AveStsadmWeb(ItemRestoreConfig config, string siteUrl, string ownerLogin)
            : base(config)
        {
            this.siteUrl = siteUrl;
            this.ownerLogin = ownerLogin;
        }

        public void SPRestoreWeb(string destWebName, string filePath, bool noFileCompression, ref bool isSiteFailed)
        {
            if (IsWebExisted(this.siteUrl, destWebName))
            {
                IsWebExist = true;
                if (config.ContainerConflictResolution == ConflictResolutionType.Replace)
                {
                    log.Info("Deleting the existed destination site or web....");
                    DeleteWeb(destWebName);
                    WebToSite(destWebName, filePath, noFileCompression, ref isSiteFailed);
                }
            }
            else
            {
                IsWebExist = false;
                WebToSite(destWebName, filePath, noFileCompression, ref isSiteFailed);
            }
        }

        private void ImportObject(string destWebName, string tempFilePath, bool noFileCompression)
        {
            string webUrl = this.siteUrl + "/" + destWebName;
            string importString = "-o import -url \"" + webUrl + "\" -filename \"" + tempFilePath + "\" -includeusersecurity -nologfile -quiet";
            if (noFileCompression)
            {
                importString = importString + " -nofilecompression";
            }

            log.Info("Importing the web by STSADM... Start Time: {0}.", DateTime.Now.ToString());

            string strError;

            if (!RunStsAdmOperation(importString, true, out strError, config.ContextKind))
            {
                if (!string.IsNullOrEmpty(strError))
                {
                    throw new AveException(strError);
                }
            }
            log.Info("Imported the web sucessfully. End Time: {0} Temp File: {1}.", DateTime.Now.ToString(), tempFilePath);
        }

        private void WebToSite(string webName, string fileName, bool noFileCompression, ref bool isSiteFailed)
        {
            CreateSiteOrWeb(this.siteUrl, webName, ref isSiteFailed);
            try
            {
                ImportObject(webName, fileName, noFileCompression);
            }
            finally
            {
                DeleteWebExcludeSelf(string.IsNullOrEmpty(webName) ? AveConstants.ROOT_WEB : webName);
            }
        }

        private void CreateSite(string siteUrl)
        {
            bool needToDelStsCfg = false;
            bool needToRestoreStsCfg = false;
            string stsadmPath = string.Empty;
            string webconfigPath = string.Empty;
            bool needheadappurl = false;
            IAveSiteCollection sites = null;
            try
            {
                IAveWebApplication webApp = config.ObjectModelFactory.CreateWebApplication(AveItemRestoreUtility.GetWebAppUrl(config.ObjectModelFactory, siteUrl));
                stsadmPath = Environment.GetEnvironmentVariable("CommonProgramFiles") + (config.ContextKind == AveContextKind.ServerObjectModel ? @"\Microsoft Shared\Web Server Extensions\14\BIN\STSADM.EXE.CONFIG" : @"\Microsoft Shared\web server extensions\12\BIN\STSADM.EXE.CONFIG");
                FBAOperation(webApp, config.ContextKind, this.ownerLogin, ref siteUrl, ref needToDelStsCfg, ref needToRestoreStsCfg);
                //this.mDBInfo.GetDBStatus(webApp);
                if (config.DestinationInfo.ContentDBId != Guid.Empty)
                {
                    IAveContentDatabase contentDB = webApp.ContentDatabases[config.DestinationInfo.ContentDBId];
                    if (contentDB != null && contentDB.Exists)
                    {
                        if (contentDB.Status == AveObjectStatus.Offline || contentDB.Status == AveObjectStatus.Disabled)
                        {
                            throw new Exception(string.Format("Cannot create the specified site collection. The content database {0} of web application {1} is offline.", contentDB.Name, webApp.Name));
                        }
                        else
                        {
                            sites = contentDB.Sites;
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot get content database by id " + config.DestinationInfo.ContentDBId);
                    }

                    //this.mDBInfo.OffLineDB(webApp, Config.DestinationInfo.ContentDBId);
                }
                //DOC-54127----begin
                log.Info("Adding manager path....");
                if (!(siteUrl.LastIndexOf('/') > 6 && siteUrl.Substring(8 + siteUrl.Substring(7).IndexOf('/')).Split(new[] { '/' }).Length > 1 && webApp.Prefixes.Contains(siteUrl.Substring(8 + siteUrl.Substring(7).IndexOf('/')).Split(new[] { '/' })[0])))
                {
                    string strError;
                    string addPath = " -o addpath -url \"" + siteUrl + "\" -type explicitinclusion";
                    if (!RunStsAdmOperation(addPath, true, out strError, config.ContextKind))
                    {
                        log.Info("Added manager path failed..");
                    }
                }
                //DOC-54127----end
                const string ownerEmail = "someone@example.com";
                foreach (AveUrlZone zone in webApp.IisSettings.Keys)
                {
                    if (!siteUrl.StartsWith(webApp.AlternateUrls.GetResponseUrl(zone).Uri.ToString().TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        needheadappurl = true;
                    }
                    else
                    {
                        needheadappurl = false;
                        break;
                    }
                }
                log.Info("Creating the site {0}....", siteUrl);
                string destWebAppUrl = AveItemRestoreUtility.GetWebAppUrl(config.ObjectModelFactory, siteUrl);
                CreateSite(sites, siteUrl, ownerEmail, config.DestinationInfo.OwerLogin, ref needheadappurl, destWebAppUrl, null, config.ContextKind);
            }
            finally
            {
                if (needToDelStsCfg)
                {
                    DeleteManageStsadmConfigFile(stsadmPath, needToRestoreStsCfg);
                }
            }
        }

        private void CreateSiteOrWeb(string siteUrl, string webName, ref bool isSiteFailed)
        {
            if (webName.Equals(AveConstants.ROOT_WEB) || string.IsNullOrEmpty(webName))
            {
                try
                {
                    CreateSite(siteUrl);
                }
                catch
                {
                    isSiteFailed = true;
                    throw;
                }
                RestoreSiteOwner(siteUrl, this.ownerLogin);
            }
            else
            {
                log.Info("Creating the web {0}....", siteUrl + "/" + webName);
                #region site level retore job hang sometimes if we use stsadm createweb,use api instead
                using (IAveSite site = config.ObjectModelFactory.CreateSite(siteUrl))
                using (IAveWeb web = site.AllWebs.Add(site.ServerRelativeUrl.TrimEnd('/') + "/" + webName, null, null, 0, "", false, false))
                {
                }
                #endregion
            }
        }

        private void DeleteSite(string siteUrl)
        {
            IAveSite site = null;
            try
            {
                site = config.ObjectModelFactory.CreateSite(siteUrl);
                IAveWeb web = site.RootWeb;
                if (web.Properties.ContainsKey("BackedUp"))
                {
                    web.Properties["BackedUp"] = "true";
                }
                else
                {
                    web.Properties.Add("BackedUp", "true");
                }
                web.Properties.Update();
                site.Delete();
            }
            finally
            {
                if (site != null)
                {
                    site.Dispose();
                }
            }
        }

        private void DeleteWebExcludeSelf(string webName)
        {
            using (IAveSite site = config.ObjectModelFactory.CreateSite(this.siteUrl))
            using (IAveWeb web = webName == AveConstants.ROOT_WEB ? site.RootWeb : site.OpenWeb(webName))
            {
                foreach (IAveWeb subWeb in web.Webs)
                {
                    log.Info("Deleting needless web....");
                    DeleteWeb(subWeb.ServerRelativeUrl);
                    subWeb.Dispose();
                }
            }
        }

        private void DeleteWeb(string nameOrUrl)
        {
            if (string.IsNullOrEmpty(nameOrUrl)
                || nameOrUrl.Equals(AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase))
            {
                DeleteSite(this.siteUrl);
                return;
            }
            using (var site = config.ObjectModelFactory.CreateSite(this.siteUrl))
            {
                using (var web = site.OpenWeb(nameOrUrl))
                {
                    DeleteWeb(web);
                }
            }
        }

        private void DeleteWeb(IAveWeb web)
        {
            try
            {
                foreach (IAveWeb subWeb in web.Webs)
                {
                    DeleteWeb(subWeb);
                    subWeb.Dispose();
                }
                if (web.Properties.ContainsKey("BackedUp"))
                {
                    web.Properties["BackedUp"] = "true";
                }
                else
                {
                    web.Properties.Add("BackedUp", "true");
                }
                web.Properties.Update();
                web.Delete();
            }
            catch (Exception e)
            {
                log.Warn("Deleted the web failed.", e.Message, e.StackTrace);
            }
        }

        private bool IsWebExisted(string siteUrl, string webName)
        {
            try
            {
                using (var tempSite = config.ObjectModelFactory.CreateSite(siteUrl))
                {
                    if (webName.Equals(AveConstants.ROOT_WEB) || string.IsNullOrEmpty(webName))
                    {
                        return string.Equals(tempSite.Url.TrimEnd('/'), siteUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
                    }
                    bool result = false; //if the web is already existed then result = true
                    string webUrl = siteUrl + "/" + webName;
                    foreach (var w in tempSite.AllWebs)
                    {
                        if (w.Url == webUrl)
                        {
                            result = true;
                        }
                        w.Dispose();
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while checking if the destination site collection that has been existed.SiteUrl:{0}. Error Message: {1}.", siteUrl, e.ToString());
                return false;
            }
        }

        public string GetTitle(string siteUrl, string webName)
        {
            var result = string.Empty;
            try
            {
                using (var tempSite = config.ObjectModelFactory.CreateSite(siteUrl))
                {
                    if (webName.Equals(AveConstants.ROOT_WEB) || string.IsNullOrEmpty(webName))
                    {
                        result = tempSite.RootWeb.Title;
                    }
                    else
                    {
                        using (var web = tempSite.OpenWeb(webName))
                        {
                            result = web.Title;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while getting web title.SiteUrl:{0}, web nam:{1}. Error Message: {2}.", siteUrl, webName, e.ToString());
            }
            return result;
        }


    }
}
