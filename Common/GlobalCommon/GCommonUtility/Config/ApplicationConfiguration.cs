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
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.Config
{
    public class ApplicationConfiguration
    {
        public string AppCertFile { get; set; }
        public string AppCertSecret { get; set; }
        public string AzureRegion { get; set; }
        public string ConfigDatabaseConnection { get; set; }
        public string DBManagerUsername { get; set; }
        public string DBManagerPassword { get; set; }
        public string ExchangeClientId { get; set; }
        public string IMCacheDBName { get; set; }        
        public string IsProductEnvironment { get; set; }
        public string IsStaging { get; set; }
        public StorageInfo JobContextStorageXri { get; set; }
        public string JobQueueConnectionString { get; set; }        
        public string JobQueueNameMapping { get; set; }
        public StorageInfo JobReportStorageXri { get; set; }
        public LogStorageXri LogStorageXri { get; set; }
        public string Office365ClientId { get; set; }
        public string AOS_API_URL { get; set; }
        public string AOS_MODERN_API_URL { get; set; }
        public string PortalURL { get; set; }
        public string InternalPortalApiUrl { get; set; }
        public string PortalSubscriptionName { get; set; }
        public string PortalTopicName { get; set; }
        public string PortalTopicConnectionString { get; set; }
        public string RedisCacheSettings { get; set; }
        public string SharePointClientId { get; set; }
        public string StartupConfig { get; set; }
        public string SimpleLogin { get; set; }
        public string InsiderEnvironment { get; set; }

        public string AosTokenApiURL { get; set; }
        #region unknown but maybe agent
        public string ElasticPoolName { get; set; }
        public StorageInfo AgentStorageXri { get; set; }
        public string WcfAgentHost { get; set; }
        #endregion

        #region special attributes for manager role configration
        public string SyncConsumeTaskCount { get; set; }
        public string DownLoadSubInfoFromStorageMaxTaskCount { get; set; }
        public string AuditCacheCleanDays { get; set; }
        public string AuditRetiveSubJobMaxSiteCollection { get; set; }
        public string JobReportFileMaxSize { get; set; }
        public string SMTPServerSizeLimitation { get; set; }
        #endregion        

        #region for web api
        public string TelemetryStorageSAS { get; set; }
        #endregion
        public string RealTimeConnectionString { get; set; }

        #region 
        public string ControlServiceAddress { get; set; }
        public StorageInfo HotfixStorageXri { get; set; }
        public string PageViewStorageConnString { get; set; }
        #endregion
    }

    public class LogStorageXri
    {
        public StorageInfo API_LogStorageXri { get; set; }
        public StorageInfo Web_Timer_TimerTask_LogStorageXri { get; set; }
    }

    public class StorageInfo
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}
