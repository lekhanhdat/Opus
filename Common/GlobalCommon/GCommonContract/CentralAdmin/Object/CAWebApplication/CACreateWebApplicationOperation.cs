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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CACreateWebApplicationOperation : CAOperation
    {
        [DataMember]
        public String Path { get; set; }
        [DataMember]
        public WebApplictionGetOption WebApplictionGetOption { get; set; }
        [DataMember]
        [XmlElement]
        public WebApplictionAddOrExtendOption WebApplictionAddOrExtendOption { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebApplictionGetOption
    {
        [DataMember]
        public Boolean IsCEIP { get; set; }

        [DataMember]
        public Boolean IsClaimsBased { get; set; }

        [DataMember]
        public String SystemDisk { get; set; }

        [DataMember]
        public WebApplicationServiceConnections WebAppServiceConnections { get; set; }

        [DataMember]
        public List<IISWebSite> ExistingWebSites { get; set; }

        [DataMember]
        public List<String> TrustProviderNames { get; set; }

        [DataMember]
        public String DatabaseServerName { get; set; }

        [DataMember]
        public String LoadBalanceUrl { get; set; }

        [DataMember]
        public List<String> ExistApplicationPools { get; set; }

        [DataMember]
        public String SearchService { get; set; }

        [DataMember]
        public String PredefinedSecurityAccount { get; set; }

        [DataMember]
        public List<String> ManagedAccountList { get; set; }

        [DataMember]
        public List<String> Zones { get; set; }

    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebApplicationServiceConnections
    {
        [DataMember]
        [XmlAttribute("isDefault")]
        public Boolean IsDefault { get; set; }
        [DataMember]
        //[XmlArray("Connections")]
        //[XmlArrayItem("Connection")]
        [XmlElement]
        public List<WebApplicationServiceConnection> Connections { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebApplicationServiceConnection
    {
        [DataMember]
        [XmlAttribute("name")]
        public String Name { get; set; }
        [DataMember]
        [XmlAttribute("serviceConnectiontype")]
        public String ServiceConnctionType { get; set; }
        [DataMember]
        [XmlAttribute("checked")]
        public Boolean Checked { get; set; }
        [DataMember]
        [XmlAttribute("defaultChecked")]
        public Boolean DefaultChecked { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebApplictionAddOrExtendOption
    {
        [DataMember]
        [XmlAttribute("isUseClaimsBasedAuthentication")]
        public Boolean IsUseClaimsBasedAuthentication { get; set; }

        [DataMember]
        [XmlElement]
        public ClaimsBasedAuthenticationInfo ClaimsBasedAuthenticationInfo { get; set; }

        [DataMember]
        [XmlElement]
        public WebApplicationServiceConnections WebAppServiceConnections { get; set; }

        [DataMember]
        [XmlElement]
        public IISWebSiteSetting IISWebSite { get; set; }

        [DataMember]
        [XmlElement]
        public SecurityConfiguration SecurityConfiguration { get; set; }

        [DataMember]
        [XmlElement]
        public LoadBalancedSetting LoadBalancedSetting { get; set; }

        [DataMember]
        [XmlElement]
        public ApplicationPoolSetting ApplicationPoolSetting { get; set; }

        [DataMember]
        [XmlAttribute("automaticReStartIIS")]
        public Boolean AutomaticReStartIIS { get; set; }

        [DataMember]
        [XmlElement]
        public DatabaseServerSettings DatabaseServerSettings { get; set; }

        [DataMember]
        [XmlAttribute("failOverServerName")]
        public String FailOverServerName { get; set; }

        [DataMember]
        [XmlElement]
        public SearchServerSettings SearchServerSettings { get; set; }

        [DataMember]
        [XmlAttribute("isCEIPEnabled")]
        public Boolean IsCEIPEnabled { get; set; }

        [DataMember]
        [XmlElement]
        public ScheduleSettings ScheduleSettings { get; set; }

        [DataMember]
        [XmlElement]
        public NotificationSettings NotificationSettings { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ClaimsBasedAuthenticationInfo
    {
        [DataMember]
        [XmlAttribute("isUseAspNetMemberAuthentication")]
        public Boolean IsUseAspNetMemberAuthentication { get; set; }

        [DataMember]
        [XmlAttribute("aspNetMemberProviderName")]
        public String AspNetMemberProviderName { get; set; }

        [DataMember]
        [XmlAttribute("aspNetRoleManagerName")]
        public String AspNetRoleManagerName { get; set; }

        [DataMember]
        [XmlAttribute("signInPageUrlValue")]
        public String  SignInPageUrlValue { get; set; }

        [DataMember]
        [XmlAttribute("isUseTrustedProvider")]
        public Boolean IsUseTrustedProvider { get; set; }

        [DataMember]
        [XmlAttribute("trustedProviderName")]
        public String TrustedProviderName { get; set; }

        [DataMember]
        [XmlAttribute("isUseBasicAuthentication")]
        public Boolean IsUseBasicAuthentication  { get; set; }

        [DataMember]
        [XmlAttribute("isUseNtlmAuthentication")]
        public Boolean IsUseNtlmAuthentication { get; set; }

        [DataMember]
        [XmlAttribute("isUseWindowsAuthentication")]
        public Boolean IsUseWindowsAuthentication { get; set; }

        [DataMember]
        [XmlAttribute("isUseWindowsIntegratedAuthencation")]
        public Boolean IsUseWindowsIntegratedAuthencation { get; set; }
        
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IISWebSite
    {
        [DataMember]
        [XmlAttribute("isUseSSL")]
        public Boolean IsUseSSL { get; set; }
        [DataMember]
        [XmlAttribute("port")]
        public Int32 Port { get; set; }
        [DataMember]
        [XmlAttribute("hostHeader")]
        public String HostHeader { get; set; }
        [DataMember]
        [XmlAttribute("path")]
        public String Path { get; set; }
        [DataMember]
        [XmlAttribute("name")]
        public String Name { get; set; }
        [DataMember]
        [XmlAttribute("description")]
        public String Description { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IISWebSiteSetting : IISWebSite
    {
        [DataMember]
        [XmlAttribute("isUseExistingIISWebSite")]
        public Boolean IsUseExistingIISWebSite { get; set; }
        [DataMember]
        [XmlAttribute("existingIISWebSiteName")]
        public String ExistingIISWebSiteName { get; set; }
        [DataMember]
        [XmlAttribute("newIISWebSiteDescription")]
        public String NewIISWebSiteDescription { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityConfiguration
    {
        [DataMember]
        [XmlAttribute("authenticationProvider")]
        public AuthenticationEnum AuthenticationProvider { get; set; }
        [DataMember]
        [XmlAttribute("isAllowAnonymous")]
        public Boolean IsAllowAnonymous { get; set; }
        [DataMember]
        [XmlAttribute("isUseSSL")]
        public Boolean IsUseSSL { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuthenticationEnum
    {
        [EnumMember]
        Negotiate,
        [EnumMember]
        NTLM,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LoadBalancedSetting
    {
        [DataMember]
        [XmlAttribute("url")]
        public String Url { get; set; }
        [DataMember]
        [XmlAttribute("zone")]
        public String Zone { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ApplicationPoolSetting
    {
        [DataMember]
        [XmlAttribute("isUseExistingApplicaitonPool")]
        public Boolean IsUseExistingApplicaitonPool { get; set; }
        [DataMember]
        [XmlAttribute("existingApplicationPoolName")]
        public String ExistingApplicationPoolName { get; set; }
        [DataMember]
        [XmlAttribute("newApplicationPoolName")]
        public String NewApplicationPoolName { get; set; }
        [DataMember]
        [XmlAttribute("isUsePredefinedApplicationPoolAccount")]
        public Boolean IsUsePredefinedApplicationPoolAccount { get; set; }
        [DataMember]
        [XmlAttribute("predefinedApplicationPoolAccountName")]
        public String PredefinedApplicationPoolAccountName { get; set; }
        [DataMember]
        [XmlAttribute("configurableUsername")]
        public String ConfigurableUsername { get; set; }
        [DataMember]
        [XmlAttribute("configurablePassword")]
        public String ConfigurablePassword { get; set; }
        [DataMember]
        [XmlAttribute("isRegisterNewManagedAccount")]
        public Boolean IsRegisterNewManagedAccount { get; set; }
        [DataMember]
        [XmlElement]
        public ManagedAccountRegisterInfo ManagedAccountRegisterInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManagedAccountRegisterInfo
    {
        [DataMember]
        [XmlAttribute("AccountName")]
        public String AccountName { get; set; }
        [DataMember]
        [XmlAttribute("password")]
        public String Password { get; set; }
        [DataMember]
        [XmlAttribute("isAutoChangePassword")]
        public Boolean IsAutoChangePassword { get; set; }
        [DataMember]
        [XmlAttribute("daysBeforeEnfocedExpiredPolicy")]
        public Int32 DaysBeforeEnfocedExpiredPolicy { get; set; }
        [DataMember]
        [XmlAttribute("isEmailNotificationEnabled")]
        public Boolean IsEmailNotificationEnabled { get; set; }
        [DataMember]
        [XmlAttribute("daysBeforePasswordChangeToNotification")]
        public Int32 DaysBeforePasswordChangeToNotification { get; set; }
        [DataMember]
        [XmlAttribute("passwordChangeScheduleType")]
        public TimeScheduleEnum PasswordChangeScheduleType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeScheduleEnum
    {
        [EnumMember]
        Weekly,
        [EnumMember]
        Monthly,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DatabaseServerSettings
    {
        [DataMember]
        [XmlAttribute("databaseServerName")]
        public String DatabaseServerName { get; set; }
        [DataMember]
        [XmlAttribute("databaseName")]
        public String DatabaseName { get; set; }
        [DataMember]
        [XmlAttribute("isUseWindowsAuthentication")]
        public Boolean IsUseWindowsAuthentication { get; set; }
        [DataMember]
        [XmlAttribute("sqlAuthenticationAccount")]
        public String SqlAuthenticationAccount { get; set; }
        [DataMember]
        [XmlAttribute("sqlAuthenticationPassword")]
        public String SqlAuthenticationPassword { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchServerSettings
    {
        [DataMember]
        [XmlAttribute("searchServerName")]
        public String SearchServerName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleSettings
    {
        [DataMember]
        [XmlAttribute("isRunNow")]
        public Boolean IsRunNow { get; set; }
        [DataMember]
        [XmlAttribute("scheduleTime")]
        public DateTime ScheduleTime { get; set; }
        [DataMember]
        [XmlAttribute("description")]
        public String Description { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationSettings
    {
        [DataMember]
        //[XmlAttribute("notificationUsers")]
        [XmlElement]
        public List<String> NotificationUsers { get; set; }
    }
}
