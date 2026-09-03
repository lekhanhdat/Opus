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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveWebApplicationInfo
    {
        public bool AlertEnable;
        public bool AlertsLimited;
        public int AlertsMaximum;
        public bool ChangeLogExpirationEnabled;
        public string ChangeLogRetentionPeriod;
        public string DefaultQuotaTemplate;
        public int DefaultTimeZone;
        public bool EventHandlersEnabled;
        public int MaximumFileSize;
        public bool MetaWeblogAuthenticationEnabled;
        public bool MetaWeblogEnabled;
        public string StrOutboundSMTPServer;
        public string OutboundMailReplyToAddress;
        public string OutboundMailSenderAddress;
        public int NCodePage;
        public bool PresenceEnabled;
        public bool RecycleBinCleanupEnabled;
        public int RecycleBinRetentionPeriod;
        public int SecondStageRecycleBinQuota;
        public bool SendLoginCredentialsByEmail;
        public bool FormDigestSettingsExpires;
        public string FormDigestSettingsTimeout;
        public bool SecurityValidationIs;
        public bool EnableRSSfeeds;
        public bool AllowAccessToWebPartCatalog;
        public bool AllowPartToPartCommunication;
        public bool RequireContactForSelfServiceSiteCreation;
        public bool SelfServiceSiteCreationEnabled;
        public bool RecycleBinEnable;
        public bool MasterPageReferenceEnabled;
        public int BrowserFileHandling;
        public bool BrowserCEIPEnabled;
        public int AnonymousPolicy;
        public Dictionary<string, string> IisSettings = new Dictionary<string, string>();
    }

    public class AveSPWebAppPathInfo
    {
        public string Name;
        public string Type;
    }

    public class AveSPWebAppPolicyRoleInfo
    {
        public ulong DenyRightsMask;
        public string Description;
        public ulong GrantRightsMask;
        public Guid ID;
        public bool IsSiteAdmin;
        public bool IsSiteAuditor;
        public string Name;
        public int Type;
    }

    public class AveSPWebAppPolicyInfo
    {
        public string DisPlayName;
        public bool IsSystemUser;
        public string UserName;
        public List<Guid> ListRoleBingds = new List<Guid>();
    }

}
