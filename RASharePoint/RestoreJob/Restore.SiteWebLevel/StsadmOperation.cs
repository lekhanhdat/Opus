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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility;

namespace AvePoint.Item.Restore
{
    internal abstract class StsadmOperation
    {
        protected static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected ItemRestoreConfig config;

        public StsadmOperation(ItemRestoreConfig config)
        {
            this.config = config;
        }

        protected void DeleteManageStsadmConfigFile(string stsadmPath, bool needToRestoreStsCfg)
        {
            try
            {
                /* Fortify Issue Type: Path Manipulation 
                * Sink Details:  AvePoint.Item.Restore AveStsadmSite SPRestoreSite 147 
                *                AvePoint.Item.Restore AveStsadmWeb  CreateSite    176
                * Ignore Reason: 路径是预设的，不存在用户恶意输入 
                */
                File.Delete(stsadmPath);
                if (needToRestoreStsCfg)
                {
                    File.Move(stsadmPath + ".original", stsadmPath);
                }
            }
            catch (Exception e)
            {
                log.Warn("Deleted STSADM configuration file failed. Error Message: {0}.", e.Message);
            }
        }

        protected void AddManageStsadmConfigFile(string webconfigPath, string stsadmPath, ref bool needToDelStsCfg, ref bool needToRestoreStsCfg)
        {
            try
            {
                if (File.Exists(stsadmPath))
                {
                    if (File.Exists(stsadmPath + ".original"))
                    {
                        File.Delete(stsadmPath + ".original");
                    }
                    File.Move(stsadmPath, stsadmPath + ".original");
                    needToRestoreStsCfg = true;
                }
                else
                {
                    FileStream tempFileStream = File.Create(stsadmPath);
                    tempFileStream.Close();
                }
                File.Copy(webconfigPath, stsadmPath, true);
                needToDelStsCfg = true;
            }
            catch (Exception e)
            {
                log.Warn("Added STSDAM configuration file failed. Error Message: {0}.", e.Message);
            }
        }

        protected bool RunStsAdmOperation(string args, bool needLog, AveContextKind contextKind)
        {
            string errorMessage = string.Empty;
            return RunStsAdmOperation(args, needLog, out errorMessage, contextKind);
        }

        protected bool RunStsAdmOperation(string args, bool needLog, out string errorMessage, AveContextKind contextKind)
        {
            var startInfo = new ProcessStartInfo();
            Environment.CurrentDirectory = Environment.GetEnvironmentVariable("CommonProgramFiles") + (contextKind == AveContextKind.ServerObjectModel ? "\\Microsoft Shared\\web server extensions\\14\\BIN" : "\\Microsoft Shared\\web server extensions\\12\\BIN");
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "stsadm.exe";
            if (SecurityUtils.ValidateCommandArgs(args))
            {
                startInfo.Arguments = args;
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;

            Process proc = null;
            try
            {
                proc = System.Diagnostics.Process.Start(startInfo);
                proc.WaitForExit();

                errorMessage = string.Empty;

                if (proc.ExitCode != 0)
                {
                    string stsadmError = proc.StandardError.ReadToEnd();
                    //mLog.Error(args + "\n" + stsadmError);
                    if (!String.IsNullOrEmpty(stsadmError))
                    {
                        errorMessage = stsadmError;
                    }
                    if (needLog)
                    {
                        log.Error("STSADM operation is failed. Command:{0}. Error Message: {1}.", args, stsadmError);
                    }
                    return false;
                }
                return true;
            }
            finally
            {
                if (proc != null)
                {
                    proc.Close();
                }
            }
        }

        protected void CreateSite(IAveSiteCollection sites, string url, string ownerEmail, string ownerLogin, ref bool needheadappurl, string destWebAppUrl, string template, AveContextKind contextKind)
        {
            string strError;
            if (sites == null)
            {
                string createSite = null;
                if (!needheadappurl)
                {
                    createSite = "-o createsite -url \"" + url + "\" -owneremail \"" + ownerEmail + "\" -ownerlogin \"" + ownerLogin + "\"";
                    if (!string.IsNullOrEmpty(template))
                    {
                        createSite += " -sitetemplate " + template;
                    }
                }
                else
                {
                    createSite = "-o createsite -url \"" + url + "\" -owneremail \"" + ownerEmail + "\" -ownerlogin \"" + ownerLogin + "\"" + " -hostheaderwebapplicationurl \"" + destWebAppUrl + "\"";
                    if (!string.IsNullOrEmpty(template))
                    {
                        createSite += " -sitetemplate " + template;
                    }
                }
                if (!RunStsAdmOperation(createSite, true, out strError, contextKind))
                {
                    throw new ExecuteStsadmFailedException(createSite, strError);
                }
            }
            else
            {
                //when customer specified a contentDB,the destination site collection is created by API.
                if (needheadappurl)
                {
                    sites.Add(url,
                               null,
                               null,
                               UInt32.Parse("0"),
                               template,
                               ownerLogin,
                               ownerLogin,
                               ownerEmail,
                               null,
                               null,
                               null,
                               true);

                }
                else
                {
                    sites.Add(url,
                               null,
                               null,
                               UInt32.Parse("0"),
                               template,
                               ownerLogin,
                               ownerLogin,
                               ownerEmail);

                }
            }
        }

        protected void RestoreSiteOwner(string siteUrl, string oldOwnerLogin)
        {
            string strError = string.Empty;
            if (!String.IsNullOrEmpty(oldOwnerLogin))
            {
                //使用Stsadm 还原owner在部分环境会使得STSADM.exe进程hang住，API进行同样操作还未发现类似问题
                IAveSite site = null;
                try
                {
                    site = config.ObjectModelFactory.CreateSite(siteUrl);
                    IAveUser newOwner = site.RootWeb.EnsureUser(oldOwnerLogin);
                    site.Owner = newOwner;
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, config.EventCategory, new EventIds.SharePoint.RestoreUserFailedEventMessage(oldOwnerLogin, e));
                    string addUser = "-o siteowner -url " + siteUrl + " -ownerlogin " + config.DestinationInfo.OwerLogin;
                    if (!RunStsAdmOperation(addUser, false, config.ContextKind))
                    {
                        log.Warn("Added site owner failed..");
                    }
                }
                finally
                {
                    if (site != null)
                    {
                        site.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// 对FBA模式的目的端进行特殊操作.
        /// </summary>
        /// <param name="webApp"></param>
        /// <param name="contextKind"></param>
        /// <param name="srcOwnerLogin"></param>
        /// <param name="siteUrl"></param>
        /// <param name="needToDelStsCfg"></param>
        /// <param name="needToRestoreStsCfg"></param>
        /// <returns></returns>
        protected void FBAOperation(IAveWebApplication webApp, AveContextKind contextKind, string srcOwnerLogin, ref string siteUrl, ref bool needToDelStsCfg, ref bool needToRestoreStsCfg)
        {
            if (string.IsNullOrEmpty(srcOwnerLogin))
            {
                //需要对owner为空的case做处理，对于FBA...
            }
            log.Log(AveLogLevel.INFO, string.Format("FBA related operations.Version:{0}", contextKind.ToString()));
            var settings = webApp.IisSettings;
            string stsadmPath = Environment.GetEnvironmentVariable("CommonProgramFiles") + (contextKind == AveContextKind.ServerObjectModel ? "\\Microsoft Shared\\web server extensions\\14\\BIN" : "\\Microsoft Shared\\web server extensions\\12\\BIN");
            foreach (AveUrlZone zone in settings.Keys)
            {
                //if (settings[zone].AuthenticationMode == AuthenticationMode.Forms)
                //{
                string webconfigPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(settings[zone].Path.FullName.TrimEnd('\\'), "Web.config");
                switch (contextKind)
                {
                    case AveContextKind.ServerObjectModel:
                        if (settings[zone].UseFormsClaimsAuthenticationProvider)
                        {
                            AddManageStsadmConfigFile(webconfigPath, stsadmPath, ref needToDelStsCfg, ref needToRestoreStsCfg);
                            //下面的判断是针对FBA类型的目的端被删除后做inplace还原的情况
                            if (string.IsNullOrEmpty(config.DestinationInfo.OwerLogin) || !config.DestinationInfo.OwerLogin.StartsWith("i:0#.f|", StringComparison.OrdinalIgnoreCase))
                            {
                                config.DestinationInfo.OwerLogin = srcOwnerLogin;
                            }
                        }
                        break;
                    case AveContextKind.Server07ObjectModel:
                        AddManageStsadmConfigFile(webconfigPath, stsadmPath, ref needToDelStsCfg, ref needToRestoreStsCfg);
                        //下面的判断是针对FBA类型的目的端被删除后做inplace还原的情况
                        if (string.IsNullOrEmpty(config.DestinationInfo.OwerLogin) || !config.DestinationInfo.OwerLogin.Contains(":"))
                        {
                            config.DestinationInfo.OwerLogin = srcOwnerLogin;
                        }
                        if (!zone.Equals(AveUrlZone.Default))
                        {
                            string defaultWebAppUrl = webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
                            string extendWebAppUrl = webApp.AlternateUrls.GetResponseUrl(zone).Uri.ToString();
                            siteUrl = extendWebAppUrl.TrimEnd('/') + "/" + siteUrl.Remove(0, defaultWebAppUrl.Length);
                        }
                        break;
                }
                //}
            }
        }

    }
}
