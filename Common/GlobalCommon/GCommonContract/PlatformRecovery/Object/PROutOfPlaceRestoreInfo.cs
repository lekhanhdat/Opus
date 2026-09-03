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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    /// <summary>
    /// PR Out-Of-Place还原在界面上配置的信息，统一由这个类负责记录，并存放在PRTreeNodeDto中
    /// </summary>
    [KnownType(typeof(PRWebAppOOPInfo))]
    [KnownType(typeof(PRIisOOPInfo))]
    [KnownType(typeof(PRDBOOPInfo))]
    [KnownType(typeof(PRRawDBInfo))]
    [KnownType(typeof(PRSSAOOPInfo))]
    [KnownType(typeof(PRServiceAppProxyGroupOOPInfo))]
    [KnownType(typeof(PRServiceAppProxyOOPInfo))]
    [KnownType(typeof(PRServiceAppOOPInfo))]
    [KnownType(typeof(PRSearchComponentOOPInfo))]
    [KnownType(typeof(PRSspOOPInfo))]
    [KnownType(typeof(PRSsoOOPInfo))]
    [KnownType(typeof(PRSearchIndexOOPInfo))]

    #region Service Application OutofPlace Contract
    [KnownType(typeof(PRServiceApplicationOOPInfo))]
    [KnownType(typeof(PRServiceApplicationDBOOPInfo))]
    [KnownType(typeof(PRServiceApplicationProxyOOPInfo))]
    [KnownType(typeof(PRBDCServiceApplicationOOPInfo))]
    [KnownType(typeof(PRExcelServiceApplicationOOPInfo))]
    [KnownType(typeof(PRWebAnalyticsApplicationOOPInfo))]
    [KnownType(typeof(PRAccessServiceApplicationOOPInfo))]
    [KnownType(typeof(PRSecureStoreServiceApplicationOOPInfo))]
    [KnownType(typeof(PRPerformanceServiceApplicationOOPInfo))]
    [KnownType(typeof(PRUserProfileServiceApplicationOOPInfo))]
    [KnownType(typeof(PRVisioGraphicsServiceApplicationOOPInfo))]
    [KnownType(typeof(PRPowerPointWebServiceApplicationOOPInfo))]
    [KnownType(typeof(PRConversionServiceApplicationOOPInfo))]
    [KnownType(typeof(PRWordAutomationServiceApplicationOOPInfo))]
    [KnownType(typeof(PRManagedMetadataServiceApplicationOOPInfo))]
    [KnownType(typeof(PRSubscriptionSettingsServiceApplicationOOPInfo))]
    #endregion



    [DataContract]
    public class PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string FullPath { get; set; }
    }

    [DataContract]
    public class PRWebAppOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string AppName { get; set; }
        [DataMember]
        public string AppUrl { get; set; }
        [DataMember]
        public string AppPoolName { get; set; }
        [DataMember]
        public string AppPoolUserName { get; set; }
        [DataMember]
        public string AppPoolPassword { get; set; }
        [DataMember]
        public List<PRIisOOPInfo> IisOOPInfoList { get; set; }
        [DataMember]
        public PRServiceAppProxyGroupOOPInfo AppProxyGroupInfo { get; set; }
    }

    [DataContract]
    public class PRIisOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Port { get; set; }
        [DataMember]
        public string HostHeader { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string AppUrl { get; set; }
        [DataMember]
        public string Zone { get; set; }
        [DataMember]
        public string ParentAppUrl { get; set; }
    }

    [DataContract]
    public class PRServiceApplicationOOPInfo : PROutOfPlaceRestoreInfo
    {
        private bool mDefaultProxyGroup = false;
        [DataMember]
        public string ServiceAppName { get; set; }
        [DataMember]
        public int ServiceAppPoolType { get; set; } // 0--exist, 1--create
        [DataMember]
        public string ServiceAppPoolName { get; set; }
        [DataMember]
        public string SecurityAccount { get; set; }
        [DataMember]
        public string SecurityAccountPassword { get; set; }
        [DataMember]
        public bool DefaultProxyGroup
        {
            get { return mDefaultProxyGroup; }
            set { mDefaultProxyGroup = value; }
        }
    }

    [DataContract]
    public class PRServiceApplicationDBOOPInfo : PRDBOOPInfo
    {
        [DataMember]
        public string ParentServiceAppName { get; set; }
    }

    [DataContract]
    public class PRServiceApplicationProxyOOPInfo : PROutOfPlaceRestoreInfo
    {
        private bool mDefaultProxyGroup = false;
        [DataMember]
        public string ServiceAppProxyName { get; set; }
        [DataMember]
        public int ConnectType { get; set; } // 0--name, 1--address
        [DataMember]
        public string ConnectAppName { get; set; }
        [DataMember]
        public string ConnectAppAddress { get; set; }
        [DataMember]
        public bool DefaultProxyGroup
        {
            get { return mDefaultProxyGroup; }
            set { mDefaultProxyGroup = value; }
        }
        //SecureStoreServiceApplication
        [DataMember]
        public string RefreshKeyPassphrase { get; set; }
    }

    [DataContract]
    public class PRExcelServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        //
    }

    [DataContract]
    public class PRAccessServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        //
    }

    [DataContract]
    public class PRVisioGraphicsServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        //
    }

    [DataContract]
    public class PRPowerPointWebServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        //
    }


    [DataContract]
    public class PRConversionServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        //
    }

    [DataContract]
    public class PRBDCServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {

    }

    [DataContract]
    public class PRPerformanceServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {

    }

    [DataContract]
    public class PRSecureStoreServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        private bool mAuditLogEnabled = false;
        [DataMember]
        public bool AuditLogEnabled
        {
            get { return mAuditLogEnabled; }
            set { mAuditLogEnabled = value; }
        }
        [DataMember]
        public int DaysUntilPurge { get; set; }
    }

    [DataContract]
    public class PRUserProfileServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        [DataMember]
        public string ProfileSync { get; set; }
        [DataMember]
        public string MySiteHostUrl { get; set; }
        [DataMember]
        public string MySiteManagedPath { get; set; }
        [DataMember]
        public int SiteNamingFormat { get; set; }
    }

    [DataContract]
    public class PRWebAnalyticsApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        [DataMember]
        public string DataRetention { get; set; }
    }

    [DataContract]
    public class PRWordAutomationServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        private bool mPartitionedMode = false;
        [DataMember]
        public bool PartitionedMode
        {
            get { return mPartitionedMode; }
            set { mPartitionedMode = value; }
        }
    }

    [DataContract]
    public class PRManagedMetadataServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
        private bool mReportSync = false;
        [DataMember]
        public string ContentTypeHub { get; set; }
        [DataMember]
        public bool ReportSync
        {
            get { return mReportSync; }
            set { mReportSync = value; }
        }
    }

    [DataContract]
    public class PRSubscriptionSettingsServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {
 
    }

    [DataContract]
    public class PRProjectServiceApplicationOOPInfo : PRServiceApplicationOOPInfo
    {

    }

    [DataContract]
    public class PRProjectSiteOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string WebAppUrl { get; set; }        
        [DataMember]        
        public string AdministratorAccount { get; set; }
    }

    [DataContract]
    public class PRRawDBInfo : PRDBOOPInfo
    {
    }

    [DataContract]
    public class PRDBOOPInfo : PROutOfPlaceRestoreInfo
    {

        [DataMember]
        public string DbName { get; set; }
        [DataMember]
        public string DbServer { get; set; }
        [DataMember]
        public string DbLocation { get; set; }
        [DataMember]
        public string DbLogLocation { get; set; }
        [DataMember]
        public bool WindowsAuthentication { get; set; }
        [DataMember]
        public string DbUserName { get; set; }
        [DataMember]
        public string DbPassword { get; set; }
        [DataMember]
        public string ParentAppUrl { get; set; }
        [DataMember]
        public string FailoverDBServer { get; set; }
        [DataMember]
        public List<string> ParentWebAppUrlList { get; set; }
        [DataMember]
        public List<PRDBFileInfo> DBFileList { get; set; }
    }

    [DataContract]
    public class PRSSAOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string SSAName { get; set; }
        [DataMember]
        public string ServiceAccountName { get; set; }
        [DataMember]
        public string AdminAppPool { get; set; }
        [DataMember]
        public string AdminAppPoolUserName { get; set; }
        [DataMember]
        public string QueryAppPool { get; set; }
        [DataMember]
        public string QueryAppPoolUserName { get; set; }
    }

    [DataContract]
    public class PRServiceAppProxyGroupOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string WebAppName { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
        [DataMember]
        public List<PRServiceAppProxyOOPInfo> ProxyOutOfPlaceInfos { get; set; }
    }

    [DataContract]
    public class PRServiceAppProxyOOPInfo : PROutOfPlaceRestoreInfo
    {
        private bool mIsDefault = false;

        [DataMember]
        public string TypeFullName { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsDefault
        {
            get
            {
                return mIsDefault;
            }
            set
            {
                mIsDefault = value;
            }
        }
    }

    [DataContract]
    public class PRSearchServerOOPInfo
    {
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string DefaultIndexLocation { get; set; }
    }

    [DataContract]
    public class PRServiceAppOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public PRNodeTypeId TypeId { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class PRSearchComponentOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string IndexLocation { get; set; }
        [DataMember]
        public string FailoverOnly { get; set; }
    }

    [DataContract]
    public class PRSspOOPInfo : PROutOfPlaceRestoreInfo
    {

        [DataMember]
        public string SspName { get; set; }
        [DataMember]
        public string SspUserName { get; set; }
        [DataMember]
        public string SspPassword { get; set; }
        [DataMember]
        public string AdminAppUrl { get; set; }
        [DataMember]
        public string MySiteAppUrl { get; set; }
        [DataMember]
        public string MySiteWebPath { get; set; }
    }

    [DataContract]
    public class PRSsoOOPInfo : PROutOfPlaceRestoreInfo
    {
        private uint mTicketTimeoutMin = 2;
        private uint mPurgeAuditDays = 10;

        [DataMember]
        public string SsoAdminID { get; set; }
        [DataMember]
        public string SsoAppDefAdminId { get; set; }
        [DataMember]
        public uint TicketTimeoutMin
        {
            get
            {
                return mTicketTimeoutMin;
            }
            set
            {
                mTicketTimeoutMin = value;
            }
        }
        [DataMember]
        public uint PurgeAuditDays
        {
            get
            {
                return mPurgeAuditDays;
            }
            set
            {
                mPurgeAuditDays = value;
            }
        }
    }

    [DataContract]
    public class PRSearchIndexOOPInfo : PROutOfPlaceRestoreInfo
    {
        [DataMember]
        public string SspName { get; set; }
        [DataMember]
        public string IndexServer { get; set; }
        [DataMember]
        public string IndexLocation { get; set; }
    }
}
