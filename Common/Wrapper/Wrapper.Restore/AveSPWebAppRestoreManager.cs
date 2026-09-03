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
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Xml;
using System.Collections;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Restore
{

    public class AveSPWebAppPropertyManager
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveSPWebApp mAveSPWebApp;
        private AveObjectModelFactory mModelFactory;

        public AveObjectModelFactory ModelFactory
        {
            get { return mModelFactory; }
            set { mModelFactory = value; }
        }

        public AveSPWebAppPropertyManager(AveSPWebApp aveWebApp, AveObjectModelFactory modelFactory)
        {
            mAveSPWebApp = aveWebApp;
            mModelFactory = modelFactory;
        }

        public void RestoreWebAppProperty(AveWebApplicationInfo webAppInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebAppPropertyManager.RestoreWebAppProperty"))
            {
#endif
                try
                {
                    mAveSPWebApp.WebApp.AlertsEnabled = webAppInfo.AlertEnable;
                    mAveSPWebApp.WebApp.AlertsLimited = webAppInfo.AlertsLimited;
                    mAveSPWebApp.WebApp.AlertsMaximum = webAppInfo.AlertsMaximum;
                    mAveSPWebApp.WebApp.AllowAccessToWebPartCatalog = webAppInfo.AllowAccessToWebPartCatalog;
                    mAveSPWebApp.WebApp.AllowPartToPartCommunication = webAppInfo.AllowPartToPartCommunication;
                    mAveSPWebApp.WebApp.ChangeLogExpirationEnabled = webAppInfo.ChangeLogExpirationEnabled;
                    mAveSPWebApp.WebApp.ChangeLogRetentionPeriod = TimeSpan.Parse(webAppInfo.ChangeLogRetentionPeriod);
                    mAveSPWebApp.WebApp.DefaultQuotaTemplate = webAppInfo.DefaultQuotaTemplate;
                    mAveSPWebApp.WebApp.DefaultTimeZone = webAppInfo.DefaultTimeZone;
                    mAveSPWebApp.WebApp.SyndicationEnabled = webAppInfo.EnableRSSfeeds;
                    mAveSPWebApp.WebApp.EventHandlersEnabled = webAppInfo.EventHandlersEnabled;
                    mAveSPWebApp.WebApp.FormDigestSettings.Expires = webAppInfo.FormDigestSettingsExpires;
                    mAveSPWebApp.WebApp.FormDigestSettings.Timeout = TimeSpan.Parse(webAppInfo.FormDigestSettingsTimeout);
                    mAveSPWebApp.WebApp.MaximumFileSize = webAppInfo.MaximumFileSize;
                    mAveSPWebApp.WebApp.MetaWeblogAuthenticationEnabled = webAppInfo.MetaWeblogAuthenticationEnabled;
                    mAveSPWebApp.WebApp.MetaWeblogEnabled = webAppInfo.MetaWeblogEnabled;
                    //mAveSPWebApp.WebApp.OutboundMailCodePage=webAppInfo.NCodePage;
                    // mAveSPWebApp.WebApp.OutboundMailReplyToAddress=webAppInfo.OutboundMailReplyToAddress ;
                    // mAveSPWebApp.WebApp.OutboundMailSenderAddress=webAppInfo.OutboundMailSenderAddress;
                    mAveSPWebApp.WebApp.PresenceEnabled = webAppInfo.PresenceEnabled;
                    mAveSPWebApp.WebApp.RecycleBinCleanupEnabled = webAppInfo.RecycleBinCleanupEnabled;
                    mAveSPWebApp.WebApp.RecycleBinRetentionPeriod = webAppInfo.RecycleBinRetentionPeriod;
                    mAveSPWebApp.WebApp.RequireContactForSelfServiceSiteCreation = webAppInfo.RequireContactForSelfServiceSiteCreation;
                    mAveSPWebApp.WebApp.SecondStageRecycleBinQuota = webAppInfo.SecondStageRecycleBinQuota;
                    mAveSPWebApp.WebApp.FormDigestSettings.Enabled = webAppInfo.SecurityValidationIs;
                    mAveSPWebApp.WebApp.SelfServiceSiteCreationEnabled = webAppInfo.SelfServiceSiteCreationEnabled;
                    mAveSPWebApp.WebApp.SendLoginCredentialsByEmail = webAppInfo.SendLoginCredentialsByEmail;
                    mAveSPWebApp.WebApp.RecycleBinEnabled = webAppInfo.RecycleBinEnable;
                    mAveSPWebApp.WebApp.MasterPageReferenceEnabled = webAppInfo.MasterPageReferenceEnabled;
                    mAveSPWebApp.WebApp.BrowserFileHandling = (AveBrowserFileHandling)webAppInfo.BrowserFileHandling;
                    mAveSPWebApp.WebApp.BrowserCEIPEnabled = webAppInfo.BrowserCEIPEnabled;
                    mAveSPWebApp.WebApp.Policies.AnonymousPolicy = (AveAnonymousPolicy)webAppInfo.AnonymousPolicy;

                    Dictionary<string, string> IisSettings = webAppInfo.IisSettings;
                    XmlDocument xDoc = new XmlDocument();
                    foreach (string urlZone in IisSettings.Keys)
                    {
                        AveUrlZone UrlZone = (AveUrlZone)Enum.Parse(typeof(AveUrlZone), urlZone);
                        string IisSettingDoc = IisSettings[urlZone];

                        xDoc.LoadXml(IisSettingDoc);
                        bool allowAnonymous = Convert.ToBoolean(xDoc.DocumentElement.GetAttribute("AllowAnonymous"));

                        bool useBasicAuthentication = Convert.ToBoolean(xDoc.DocumentElement.GetAttribute("UseBasicAuthentication"));
                        bool useWindowsIntegratedAuthentication = Convert.ToBoolean(xDoc.DocumentElement.GetAttribute("UseWindowsIntegratedAuthentication"));
                        if (mAveSPWebApp.WebApp.IisSettings.ContainsKey(UrlZone))
                        {
                            IAveIisSettings iisSetting = mAveSPWebApp.WebApp.IisSettings[UrlZone];
                            iisSetting.AllowAnonymous = allowAnonymous;

                            iisSetting.UseBasicAuthentication = useBasicAuthentication;
                            iisSetting.UseWindowsIntegratedAuthentication = useWindowsIntegratedAuthentication;
                        }
                        //else //对于URLZone没找到的不应该处理
                        //{
                        //    IAveIisSettings setting = mModelFactory.CreateIisSettings();
                        //    setting.AllowAnonymous = allowAnonymous;
                        //    setting.UseBasicAuthentication = useBasicAuthentication;
                        //    setting.UseWindowsIntegratedAuthentication = useWindowsIntegratedAuthentication;
                        //    mAveSPWebApp.WebApp.IisSettings.Add(UrlZone, setting);
                        //}
                        mAveSPWebApp.WebApp.ZonePolicies(UrlZone).AnonymousPolicy = (AveAnonymousPolicy)Convert.ToInt32(xDoc.DocumentElement.GetAttribute("AnonymousPolicy"));

                        xDoc.RemoveAll();
                    }
                    mAveSPWebApp.WebApp.UpdateMailSettings(webAppInfo.StrOutboundSMTPServer, webAppInfo.OutboundMailSenderAddress, webAppInfo.OutboundMailReplyToAddress, webAppInfo.NCodePage);

                    mAveSPWebApp.WebApp.Update();
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Error, string.Format("An error occurred while restoring WebApp Porperty.\n error message:{0}", e));
                    mLog.Error("An error occurred while restoring WebApp Property.", e);
                }

#if PerformanceLog
            }
#endif
        }
    }

    public class AveSPWebAppPathManager
    {
        private AveSPWebApp mAveSPWebApp;

        public AveSPWebAppPathManager(AveSPWebApp aveSPWebApp)
        {
            mAveSPWebApp = aveSPWebApp;
        }

        public void RestoreWebAppPath(List<AveSPWebAppPathInfo> webAppPathInfoCollection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebAppPathManager.RestoreWebAppPath"))
            {
#endif
                List<string> PrefixName = new List<string>();
                foreach (IAvePrefix prefix in mAveSPWebApp.WebApp.Prefixes)
                {
                    PrefixName.Add(prefix.Name);
                }
                foreach (AveSPWebAppPathInfo webAppPathInfo in webAppPathInfoCollection)
                {
                    if (!PrefixName.Contains(webAppPathInfo.Name))
                    {
                        AvePrefixType prefixType;
                        switch (webAppPathInfo.Type)
                        {
                            case "ExplicitInclusion":
                            case "Explicit":
                                prefixType = AvePrefixType.ExplicitInclusion;
                                break;
                            case "WildcardInclusion":
                            case "Wildcard":
                                prefixType = AvePrefixType.WildcardInclusion;
                                break;
                            case "Exclusion":
                                prefixType = AvePrefixType.Exclusion;
                                break;
                            default:
                                prefixType = AvePrefixType.ExplicitInclusion;
                                break;
                        }
                        mAveSPWebApp.WebApp.Prefixes.Add(webAppPathInfo.Name, prefixType);
                    }

                }
#if PerformanceLog
            }
#endif
        }
    }

    public class AveSPWebAppPolicyRoleManager
    {
        private AveSPWebApp mAveSPWebApp;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPWebAppPolicyRoleManager(AveSPWebApp aveSPWebApp)
        {
            mAveSPWebApp = aveSPWebApp;
        }

        public void RestoreWebAppPolicyRole(List<AveSPWebAppPolicyRoleInfo> webAppPolicyRoleCollection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebAppPolicyRoleManager.RestoreWebAppPolicyRole"))
            {
#endif
                Hashtable dicDstPolicyRole = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (IAvePolicyRole policyRole in mAveSPWebApp.WebApp.PolicyRoles)
                {
                    dicDstPolicyRole.Add(policyRole.Name, policyRole);
                }
                foreach (AveSPWebAppPolicyRoleInfo policyRoleInfo in webAppPolicyRoleCollection)
                {
                    AveBasePermissions DenyRightsMask = (AveBasePermissions)(policyRoleInfo.DenyRightsMask);
                    string Description = policyRoleInfo.Description;
                    AveBasePermissions GrantRightsMask = (AveBasePermissions)(policyRoleInfo.GrantRightsMask);
                    Guid srcId = policyRoleInfo.ID;
                    bool IsSiteAdmin = policyRoleInfo.IsSiteAdmin;
                    bool IsSiteAuditor = policyRoleInfo.IsSiteAuditor;
                    string Name = policyRoleInfo.Name;
                    AvePolicyRoleType Type = (AvePolicyRoleType)(policyRoleInfo.Type);
                    try
                    {
                        IAvePolicyRole tmpRole = null;
                        IAvePolicyRole dst = (IAvePolicyRole)dicDstPolicyRole[Name];
                        if (dst != null)
                        {
                            if (!((ulong)DenyRightsMask == (ulong)dst.DenyRightsMask)
                                && Description.Equals(dst.Description)
                                && ((ulong)GrantRightsMask == (ulong)dst.GrantRightsMask)
                                && IsSiteAdmin.Equals(dst.IsSiteAdmin)
                                && IsSiteAuditor.Equals(dst.IsSiteAuditor)
                                && ((int)Type == (int)dst.Type))
                            {
                                tmpRole = mAveSPWebApp.WebApp.PolicyRoles.Add(Name, Description, GrantRightsMask, DenyRightsMask);
                                mAveSPWebApp.WebApp.Update();
                                mAveSPWebApp.mapPolicyRole.Add(srcId, tmpRole.ID);
                            }
                            else
                            {
                                mAveSPWebApp.mapPolicyRole.Add(srcId, dst.ID);
                                continue;
                            }


                        }
                        else //在目的端没有找到具有"Name"指定的policyRole.直接创建.
                        {
                            tmpRole = mAveSPWebApp.WebApp.PolicyRoles.Add(Name, Description, GrantRightsMask, DenyRightsMask);

                            mAveSPWebApp.mapPolicyRole.Add(srcId, tmpRole.ID);

                            mAveSPWebApp.WebApp.Update();
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogSeverity.Error, string.Format("An error occurred while restore webApp PolicyRole. PolicyRoleName:{0}\n error message:{1}", policyRoleInfo.Name, e));
                        mLog.Error("An error occurred while restore webApp PolicyRole. PolicyRoleName:{0},Exception:{1}", policyRoleInfo.Name, e.ToString());
                    }
#if PerformanceLog
                }
#endif
            }
        }
    }

    public class AveSPWebAppPolicyManager
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPWebApp mAveSPWebApp;

        public AveSPWebAppPolicyManager(AveSPWebApp aveSPWebApp)
        {
            mAveSPWebApp = aveSPWebApp;
        }

        public void RestoreWebAppPolicy(List<AveSPWebAppPolicyInfo> webAppPolicyList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebAppPolicyManager.RestoreWebAppPolicy"))
            {
#endif
                Hashtable dicSourcePolicy = new Hashtable();
                Hashtable dicDestPolicy = new Hashtable();
                foreach (IAvePolicy policy in mAveSPWebApp.WebApp.Policies)
                {
                    dicDestPolicy.Add(policy.UserName, policy);
                }
                foreach (AveSPWebAppPolicyInfo policyInfo in webAppPolicyList)
                {
                    dicSourcePolicy.Add(policyInfo.UserName, policyInfo);
                }
                Hashtable postHT = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (string key in dicSourcePolicy.Keys)
                {
                    #region
                    //获取该key对应的源端policy信息.
                    AveSPWebAppPolicyInfo srcSPPolicy = (AveSPWebAppPolicyInfo)dicSourcePolicy[key];
                    IAvePolicy dstSPPolicy = null;
                    try
                    {
                        string userName = key;
                        //测试目的端是否已经含有userName标识的sppolicy对象.
                        dstSPPolicy = (IAvePolicy)dicDestPolicy[userName];
                        if (dstSPPolicy != null)
                        {
                            //如果已经存在了,那么直接将源端的role绑定到目的端.
                            foreach (Guid id in srcSPPolicy.ListRoleBingds)
                            {
                                try
                                {
                                    dstSPPolicy.PolicyRoleBindings.AddById(mAveSPWebApp.mapPolicyRole[id]);
                                    mAveSPWebApp.WebApp.Update();
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.AddPolicyIdError, e.ToString());
                                    System.Threading.Thread.Sleep(20);
                                    dstSPPolicy.PolicyRoleBindings.AddById(mAveSPWebApp.mapPolicyRole[id]);
                                    mAveSPWebApp.WebApp.Update();
                                }
                            }
                        }
                        else
                        {
                            //目的端无该policy.
                            dstSPPolicy = mAveSPWebApp.WebApp.Policies.Add(srcSPPolicy.UserName, srcSPPolicy.DisPlayName);
                            mAveSPWebApp.WebApp.Update();
                            //绑定Role.
                            foreach (Guid id in srcSPPolicy.ListRoleBingds)
                            {
                                dstSPPolicy.PolicyRoleBindings.AddById(mAveSPWebApp.mapPolicyRole[id]);
                                mAveSPWebApp.WebApp.Update();
                            }
                            //将新加入的policy缓存.
                            dicDestPolicy.Add(dstSPPolicy.UserName, dstSPPolicy);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.AddDicDestPolicyFailed, ex);
                    }
                    #endregion
                }
                //
                foreach (string userName in postHT.Keys)
                {
                    IAvePolicy dstSPPolicy = (IAvePolicy)dicDestPolicy[userName];
                    if (dstSPPolicy == null)
                    {
                        //如果目的端还没有该对象,那好吧.放弃.
                        continue;
                    }
                    else
                    {
                        AveSPWebAppPolicyInfo srcSPPolicy = (AveSPWebAppPolicyInfo)postHT[userName];
                        foreach (Guid id in srcSPPolicy.ListRoleBingds)
                        {
                            dstSPPolicy.PolicyRoleBindings.AddById(mAveSPWebApp.mapPolicyRole[id]);
                        }

                    }
                }
#if PerformanceLog
            }
#endif

        }
    }
}
