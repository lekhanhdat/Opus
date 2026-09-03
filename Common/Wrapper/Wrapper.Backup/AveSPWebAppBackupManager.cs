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
using System.Xml;

using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPWebAppPropertyManager
    {
        private AveSPWebApp mAveSPWebApp;

        public AveSPWebAppPropertyManager(AveSPWebApp aveSPWebApp)
        {
            mAveSPWebApp = aveSPWebApp;
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.WebAppProperty.ToString(), GetWebAppProperty());
        }

        public string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.WebAppProperty.ToString(), GetWebAppProperty());
        }

        public AveWebApplicationInfo GetWebAppProperty()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWebAppPropertyManager.GetWebAppProperty"))
            {
                AveWebApplicationInfo webAppInfo = new AveWebApplicationInfo();
                webAppInfo.AlertEnable = mAveSPWebApp.WebApp.AlertsEnabled;
                webAppInfo.AlertsLimited = mAveSPWebApp.WebApp.AlertsLimited;
                webAppInfo.AlertsMaximum = mAveSPWebApp.WebApp.AlertsMaximum;
                webAppInfo.AllowAccessToWebPartCatalog = mAveSPWebApp.WebApp.AllowAccessToWebPartCatalog;
                webAppInfo.AllowPartToPartCommunication = mAveSPWebApp.WebApp.AllowPartToPartCommunication;
                webAppInfo.ChangeLogExpirationEnabled = mAveSPWebApp.WebApp.ChangeLogExpirationEnabled;
                webAppInfo.ChangeLogRetentionPeriod = mAveSPWebApp.WebApp.ChangeLogRetentionPeriod.ToString();
                webAppInfo.DefaultQuotaTemplate = mAveSPWebApp.WebApp.DefaultQuotaTemplate;
                webAppInfo.DefaultTimeZone = mAveSPWebApp.WebApp.DefaultTimeZone;
                webAppInfo.EnableRSSfeeds = mAveSPWebApp.WebApp.SyndicationEnabled;
                webAppInfo.EventHandlersEnabled = mAveSPWebApp.WebApp.EventHandlersEnabled;
                webAppInfo.FormDigestSettingsExpires = mAveSPWebApp.WebApp.FormDigestSettings.Expires;
                webAppInfo.FormDigestSettingsTimeout = mAveSPWebApp.WebApp.FormDigestSettings.Timeout.ToString();
                //webAppInfo.IisSettings = mAveSPWebApp.WebApp.IisSettings;
                webAppInfo.MaximumFileSize = mAveSPWebApp.WebApp.MaximumFileSize;
                webAppInfo.MetaWeblogAuthenticationEnabled = mAveSPWebApp.WebApp.MetaWeblogAuthenticationEnabled;
                webAppInfo.MetaWeblogEnabled = mAveSPWebApp.WebApp.MetaWeblogEnabled;
                webAppInfo.NCodePage = mAveSPWebApp.WebApp.OutboundMailCodePage;
                webAppInfo.OutboundMailReplyToAddress = mAveSPWebApp.WebApp.OutboundMailReplyToAddress;
                webAppInfo.OutboundMailSenderAddress = mAveSPWebApp.WebApp.OutboundMailSenderAddress;
                webAppInfo.PresenceEnabled = mAveSPWebApp.WebApp.PresenceEnabled;
                webAppInfo.RecycleBinCleanupEnabled = mAveSPWebApp.WebApp.RecycleBinCleanupEnabled;
                webAppInfo.RecycleBinRetentionPeriod = mAveSPWebApp.WebApp.RecycleBinRetentionPeriod;
                webAppInfo.RequireContactForSelfServiceSiteCreation = mAveSPWebApp.WebApp.RequireContactForSelfServiceSiteCreation;
                webAppInfo.SecondStageRecycleBinQuota = mAveSPWebApp.WebApp.SecondStageRecycleBinQuota;
                webAppInfo.SecurityValidationIs = mAveSPWebApp.WebApp.FormDigestSettings.Enabled;
                webAppInfo.SelfServiceSiteCreationEnabled = mAveSPWebApp.WebApp.SelfServiceSiteCreationEnabled;
                webAppInfo.SendLoginCredentialsByEmail = mAveSPWebApp.WebApp.SendLoginCredentialsByEmail;
                webAppInfo.RecycleBinEnable = mAveSPWebApp.WebApp.RecycleBinEnabled;
                webAppInfo.MasterPageReferenceEnabled = mAveSPWebApp.WebApp.MasterPageReferenceEnabled;
                webAppInfo.BrowserFileHandling = (int)mAveSPWebApp.WebApp.BrowserFileHandling;
                webAppInfo.BrowserCEIPEnabled = mAveSPWebApp.WebApp.BrowserCEIPEnabled;
                webAppInfo.StrOutboundSMTPServer = mAveSPWebApp.WebApp.OutboundMailServiceInstance == null ? string.Empty : mAveSPWebApp.WebApp.OutboundMailServiceInstance.Server.Address.ToString();
                webAppInfo.AnonymousPolicy = (int)mAveSPWebApp.WebApp.Policies.AnonymousPolicy;

                foreach (KeyValuePair<AveUrlZone, IAveIisSettings> kvp in mAveSPWebApp.WebApp.IisSettings)
                {
                    XmlDocument xDoc = new XmlDocument();
                    AveUrlZone tmpZone = kvp.Key;
                    IAveIisSettings tmpIisSetting = kvp.Value;
                    XmlElement zoneNode = xDoc.CreateElement("Zone");
                    zoneNode.SetAttribute("Name", ((int)tmpZone).ToString());
                    zoneNode.SetAttribute("AllowAnonymous", tmpIisSetting.AllowAnonymous.ToString());
                    zoneNode.SetAttribute("UseBasicAuthentication", tmpIisSetting.UseBasicAuthentication.ToString());
                    zoneNode.SetAttribute("UseWindowsIntegratedAuthentication", tmpIisSetting.UseWindowsIntegratedAuthentication.ToString());
                    zoneNode.SetAttribute("AnonymousPolicy", ((int)mAveSPWebApp.WebApp.ZonePolicies(tmpZone).AnonymousPolicy).ToString());
                    xDoc.AppendChild(zoneNode);
                    webAppInfo.IisSettings.Add(tmpZone.ToString(), xDoc.OuterXml);
                }
                return webAppInfo;
            }
        }
    }

    public class AveSPWebPathInfoManager
    {
        private AveSPWebApp mAveWebApp;

        public AveSPWebPathInfoManager(AveSPWebApp aveWebApp)
        {
            mAveWebApp = aveWebApp;
        }

        private List<AveSPWebAppPathInfo> GetWebAppPathInfoCollection()
        {
            List<AveSPWebAppPathInfo> webAppPathInfoCollection = new List<AveSPWebAppPathInfo>();
            foreach (IAvePrefix prefix in mAveWebApp.WebApp.Prefixes)
            {
                AveSPWebAppPathInfo webAppPathInfo = new AveSPWebAppPathInfo();
                webAppPathInfo.Name = prefix.Name;
                webAppPathInfo.Type = prefix.PrefixType.ToString();
                webAppPathInfoCollection.Add(webAppPathInfo);
            }
            return webAppPathInfoCollection;
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.WebAppPath.ToString(), GetWebAppPathInfoCollection());
        }

        public string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.WebAppPath.ToString(), GetWebAppPathInfoCollection());
        }
    }

    public class AveSPWebPolicyRoleManger
    {
        private AveSPWebApp mAveWebApp;

        public AveSPWebPolicyRoleManger(AveSPWebApp aveSPWebApp)
        {
            mAveWebApp = aveSPWebApp;
        }

        private List<AveSPWebAppPolicyRoleInfo> GetWebAppPolicyRoleInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWebPolicyRoleManger.GetWebAppPolicyRoleInfo"))
            {
                List<AveSPWebAppPolicyRoleInfo> webAppPolicyRoleInfoList = new List<AveSPWebAppPolicyRoleInfo>();
                foreach (IAvePolicyRole policyRole in mAveWebApp.WebApp.PolicyRoles)
                {
                    AveSPWebAppPolicyRoleInfo webAppPolicyRoleInfo = new AveSPWebAppPolicyRoleInfo();
                    webAppPolicyRoleInfo.DenyRightsMask = (ulong)policyRole.DenyRightsMask;
                    webAppPolicyRoleInfo.Description = policyRole.Description;
                    webAppPolicyRoleInfo.GrantRightsMask = (ulong)policyRole.GrantRightsMask;
                    webAppPolicyRoleInfo.ID = policyRole.ID;
                    webAppPolicyRoleInfo.IsSiteAdmin = policyRole.IsSiteAdmin;
                    webAppPolicyRoleInfo.IsSiteAuditor = policyRole.IsSiteAuditor;
                    webAppPolicyRoleInfo.Name = policyRole.Name;
                    webAppPolicyRoleInfo.Type = (int)policyRole.Type;
                    webAppPolicyRoleInfoList.Add(webAppPolicyRoleInfo);
                }
                return webAppPolicyRoleInfoList;
            }
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.WebAppPolicyRole.ToString(), GetWebAppPolicyRoleInfo());
        }

        public string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.WebAppPolicyRole.ToString(), GetWebAppPolicyRoleInfo());
        }
    }

    public class AveSPWebPolicyManager
    {
        private AveSPWebApp mAveSPWebApp;

        public AveSPWebPolicyManager(AveSPWebApp aveWebApp)
        {
            mAveSPWebApp = aveWebApp;
        }

        private List<AveSPWebAppPolicyInfo> GetWebAppPolicyInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWebPolicyManager.GetWebAppPolicyInfo"))
            {
                List<AveSPWebAppPolicyInfo> webAppPolicyInfoList = new List<AveSPWebAppPolicyInfo>();
                foreach (IAvePolicy policy in mAveSPWebApp.WebApp.Policies)
                {
                    AveSPWebAppPolicyInfo policyInfo = new AveSPWebAppPolicyInfo();
                    policyInfo.DisPlayName = policy.DisplayName;
                    policyInfo.IsSystemUser = policy.IsSystemUser;
                    policyInfo.UserName = policy.UserName;
                    foreach (IAvePolicyRole pR in policy.PolicyRoleBindings)
                    {
                        policyInfo.ListRoleBingds.Add(pR.ID);
                    }
                    webAppPolicyInfoList.Add(policyInfo);
                }
                return webAppPolicyInfoList;
            }
        }

        public void Export(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.WebAppPolicy.ToString(), GetWebAppPolicyInfo());
        }

        public string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.WebAppPolicy.ToString(), GetWebAppPolicyInfo());
        }
    }
}