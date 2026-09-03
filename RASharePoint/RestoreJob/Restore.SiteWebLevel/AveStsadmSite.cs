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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Item.Common;

namespace AvePoint.Item.Restore
{
    internal class AveStsadmSite : StsadmOperation
    {
        private AveSiteAttributeInfo attributeInfo;
        public bool? IsSiteExist { get; private set; }
        private string url;
        public string Title { get { return GetTitle(config.ObjectModelFactory, url); } private set { } }

        public AveStsadmSite(ItemRestoreConfig config, AveSiteAttributeInfo info)
            : base(config)
        {
            attributeInfo = info;
        }

        public void SPRestoreSite(RestoreContentDto aveSiteDto, string tempFilePath)
        {
            log.Info(@"Looks up a localized string similar to Begin restoring the site... URL: {0}.", aveSiteDto.Name);
            bool needToDelStsCfg = false;
            bool needToRestoreStsCfg = false;
            string webconfigPath = string.Empty;
            url = aveSiteDto.Name;
            IAveSiteCollection sites = null;

            var restoreLock = new AveMutex(url);
            restoreLock.WaitLocked();
            try
            {
                IAveWebApplication webApp = config.ObjectModelFactory.CreateWebApplication(attributeInfo.WebAppUrl);
                sites = TryGetContentDBSites(webApp, config.DestinationInfo.ContentDBId);
                log.Info("Looks up a localized string similar to The DestWebApp is available..");
                bool isOverwrite = config.ContainerConflictResolution == ConflictResolutionType.Replace;
                //Added for doc-24078 to resolve the site bin problem.
                bool destHostheader = false;
                if (AveItemRestoreUtility.IsSiteExisted(config.ObjectModelFactory, url, ref destHostheader))
                {
                    IsSiteExist = true;
                    if (!isOverwrite)
                    {
                        log.Warn(@"Looks up a localized string similar to The destination site {0} has already existed, please select &apos;overwrite&apos; to overwrite it..", url);
                        return;
                    }
                    else
                    {
                        CheckSiteLocked(config.ObjectModelFactory, url);
                        SetSiteProperty(url);
                        log.Info("Looks up a localized string similar to The site already exists, the restore job will start..");
                        webApp.Sites.Restore(url, tempFilePath, isOverwrite, destHostheader);
                        log.Info(@"Looks up a localized string similar to Restored {0} succeefully..", url);

                    }
                }
                else
                {
                    //overwrite = true;
                    IsSiteExist = false;
                    log.Info("Looks up a localized string similar to The site does not exist, the restore job is started..");
                    string strError = string.Empty;
                    string oldOwnerLogin = config.DestinationInfo.OwerLogin;
                    const string ownerEmail = "someone@example.com";
                    bool needheadappurl = false;
                    foreach (AveUrlZone zone in webApp.IisSettings.Keys)
                    {
                        if (!url.StartsWith(webApp.AlternateUrls.GetResponseUrl(zone).Uri.ToString().TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        {
                            needheadappurl = true;
                        }
                        else
                        {
                            needheadappurl = false;
                            break;
                        }
                    }
                    FBAOperation(webApp, config.ContextKind, attributeInfo.OwnerLogin, ref url, ref needToDelStsCfg, ref needToRestoreStsCfg);
                    //DOC-54127----begin
                    AddManagerPath(url, webApp);
                    //DOC-54127 ----end
                    log.Info(@"Looks up a localized string similar to Creating the destination site {0}....", url);
                    CreateSite(sites, url, ownerEmail, config.DestinationInfo.OwerLogin, ref needheadappurl, attributeInfo.WebAppUrl, "STS#1", config.ContextKind);

                    #region restore site collection
                    log.Info(@"Looks up a localized string similar to Restoring the site... Start Time: {0}.", DateTime.Now.ToString());
                    string restoreSite = "-o restore -url \"" + url + "\" " + "-filename \"" + tempFilePath + "\" -overwrite" +
                                            (needheadappurl ? " -hostheaderwebapplicationurl \"" + attributeInfo.WebAppUrl + "\"" : "");
                    if (!RunStsAdmOperation(restoreSite, true, out strError, config.ContextKind))
                    {
                        string errorMessage = strError;
                        string deleteSite = string.Format("-o deletesite -url {0}", aveSiteDto.Name);
                        SetSiteProperty(url);
                        RunStsAdmOperation(deleteSite, false, out strError, config.ContextKind);
                        throw new ExecuteStsadmFailedException(restoreSite, errorMessage);
                    }
                    log.Info(@"Looks up a localized string similar to Restored the site sucessfully. End Time: {0}.", DateTime.Now.ToString());
                    #endregion
                    //D6没有config site administrator功能,在restore site collection的时候会把原端的owner还原回去
                    //因为restore还原后目的端会变成原端的owner，所以需要再重新还owner
                    //Log.Info(RestoreResource.Item_ASAWRSPRestoreSiteOwner);
                    //RestoreSiteOwner(siteUrl, oldOwnerLogin);
                }
                UnLockSite(url);
                log.Info(@"Looks up a localized string similar to Destination Web Application URL: {0}. Site URL: {1}. Temp File: {2}.", attributeInfo.WebAppUrl, url, tempFilePath);
            }
            finally
            {
                if (needToDelStsCfg)
                {
                    string serverPath = config.ContextKind == AveContextKind.ServerObjectModel ? @"\Microsoft Shared\Web Server Extensions\14\BIN\STSADM.EXE.CONFIG" : @"\Microsoft Shared\web server extensions\12\BIN\STSADM.EXE.CONFIG";//Environment.GetEnvironmentVariable("CommonProgramFiles") + (config.ContextKind == AveContextKind.ServerObjectModel ? @"\Microsoft Shared\Web Server Extensions\14\BIN\STSADM.EXE.CONFIG" : @"\Microsoft Shared\web server extensions\12\BIN\STSADM.EXE.CONFIG");
                    string stsadmPath = SecurityUtils.SafeCombinePath(Environment.GetEnvironmentVariable("CommonProgramFiles"), serverPath);
                    DeleteManageStsadmConfigFile(stsadmPath, needToRestoreStsCfg);
                }
                if (restoreLock != null)
                {
                    restoreLock.ReleaseLock();
                }
            }
        }


        private void AddManagerPath(string siteUrl, IAveWebApplication webApp)
        {
            string strError = string.Empty;
            int protocolIndex = siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? "https://".Length : "http://".Length;
            if (siteUrl.LastIndexOf('/') > protocolIndex)
            {
                var tmp = siteUrl.Substring(protocolIndex + 1 + siteUrl.Substring(protocolIndex).IndexOf('/')).Split(new char[] { '/' });
                if (tmp.Length >= 1 && !webApp.Prefixes.Contains(tmp[0]))
                {
                    try
                    {
                        webApp.Prefixes.Add(tmp[0], AvePrefixType.ExplicitInclusion);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, @"Looks up a localized string similar to Add managed path for site collection [{0}] by api failed. Message:{1}.", siteUrl, ex);
                        string addPath = " -o addpath -url \"" + siteUrl + "\" -type explicitinclusion";
                        if (!RunStsAdmOperation(addPath, true, config.ContextKind))
                        {
                            log.Info("Looks up a localized string similar to Added path failed. The path name already exists..");
                        }
                    }
                }
            }

        }

        private void SetSiteProperty(string siteUrl)
        {
            IAveSite site = null;
            try
            {
                log.Info(@"Looks up a localized string similar to Setting the property of {0} for site....", siteUrl);
                site = config.ObjectModelFactory.CreateSite(siteUrl);
                if (site.RootWeb.Properties.ContainsKey("BackedUp"))
                {
                    site.RootWeb.Properties["BackedUp"] = "true";
                }
                else
                {
                    site.RootWeb.Properties.Add("BackedUp", "true");
                }
                site.RootWeb.Properties.Update();
            }
            catch (Exception ex)
            {
                log.Warn(@"Looks up a localized string similar to Setting the property of {0} for site....", ex.ToString());
            }
            finally
            {
                if (site != null)
                {
                    site.Dispose();
                }
            }
        }

        private bool UnLockSite(string tmpSite)
        {
            if (!this.attributeInfo.LockSuccess)
            {
                return false;
            }
            if (log.IsDebugEnabled)
            {
                log.Info(@"Looks up a localized string similar to Unlocking site {0}....", tmpSite);
            }
            IAveSite site = null;
            try
            {
                site = this.config.ObjectModelFactory.CreateSite(tmpSite);
                site.WriteLocked = this.attributeInfo.WriteState;
                site.ReadLocked = this.attributeInfo.ReadState;
                site.Dispose();
                return true;
            }
            catch (Exception e)
            {
                log.Error(@"Looks up a localized string similar to Unlocked operation failed. Error Message: {0}.", e.ToString());
                return false;
            }
            finally
            {
                if (null != site)
                {
                    site.Dispose();
                }
            }
        }

        private IAveSiteCollection TryGetContentDBSites(IAveWebApplication webApp,  Guid contentDBId)
        {
            if (contentDBId != Guid.Empty)
            {
                IAveContentDatabase contentDB = webApp.ContentDatabases[contentDBId];
                if (contentDB != null && contentDB.Exists)
                {
                    return contentDB.Sites;
                }
            }
            return null;
        }

        public static void CheckSiteLocked(AveObjectModelFactory factory, string siteUrl)
        {
            try
            {
                using (IAveSite siteAdmin = factory.CreateSite(siteUrl))
                {
                    if (string.Equals(siteAdmin.Url.TrimEnd('/'), siteUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                    {
                        if (siteAdmin.ReadLocked || siteAdmin.WriteLocked || siteAdmin.ReadOnly)
                        {
                            siteAdmin.RootWeb.AddProperty(Guid.NewGuid().ToString(), 1);
                            siteAdmin.RootWeb.Update();//throw exception if lock.
                        }
                    }
                }
            }
            catch (System.IO.FileNotFoundException e) 
            {
                log.Error($"Check site locked failed, error : {e}");
            }
        }

        public static string GetTitle(AveObjectModelFactory factory, string siteUrl)
        {
            var result = string.Empty;
            try
            {
                using (IAveSite site = factory.CreateSite(siteUrl))
                {
                    result = site.RootWeb.Title;
                }
            }
            catch (System.IO.FileNotFoundException e) 
            { 
                log.Error($"Get site title failed, error : {e}");
            }
            return result;
        }
    }
}
