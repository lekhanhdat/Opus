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


namespace AvePoint.Wrapper.Common
{
    #region using directives
    using AvePoint.Common;
    using AvePoint.GCommon;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    #endregion

    public class WrapperConfigurationForBPOSS
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(WrapperConfigurationForBPOSS));
        public List<string> ListTemplatesInMeetingSite = new List<string> { "Meetings", "Agenda", "MeetingUser", "Decision", "MeetingObjective", "TextBox", "ThingsToBring", "HomePageLibrary" };

        private const string dependenciesFilePath = @"WrapperCommon\AgentCommonOffice365FeatureDependencies.xml";

        public int MaximumThreadsGettingVersions = 20;
        public int MultiListCountNeedSkipSchemal = 1000;
        public long SystemFileBeModifiedTiemSpan = 30 * 60;
        public int HttpWebRequestTimeout { get; set; } //ten minutes
        public int HttpWebRequestReadWriteTimeout { get; set; }  //30 minutes
        public int ClientRequestRetryInterval { get; set; }  //200 ms
        public int HealthScoreWarningValue { get; set; }// default value: 7
        public int HealthScoreSleepTime { get; set; } //default value : 15000 ms
        public int HealthScoreThrottledTimeout { get; set; } //default value: 900000 ms
        public int RetryCount { get; set; } //default value : 60

        public bool EnableHealthScoreMonitor { get; set; }

        public int ClientRequestDuration { get; set; }  //500 millisecond
        public bool UseADFSAuthentication = false;
        public bool UserClaimsAuthentication = false;
        public bool IncludeListView = true;
        public bool SearchPrincipal = false;
        public long UploadLimit = 10 * 1024 * 1024;//10 MB
        public bool KeepModeration = false;
        public bool BackupItemVersionByAPI = true;
        public bool QueryAllPropertiesInDiscver = true;

        public int SleepTime { get; set; }//20 s

        private Dictionary<Guid, List<Guid>> siteFeatureDependencies;

        public Dictionary<Guid, List<Guid>> SiteFeatureDependencies
        {
            get
            {
                if (siteFeatureDependencies == null)
                {
                    siteFeatureDependencies = GetFeatureDependencies("Features/SiteFeatures/SiteFeature");
                }
                return siteFeatureDependencies;
            }
        }

        private Dictionary<Guid, List<Guid>> webFeatureDependencies;

        public Dictionary<Guid, List<Guid>> WebFeatureDependencies
        {
            get
            {
                if (webFeatureDependencies == null)
                {
                    webFeatureDependencies = GetFeatureDependencies("Features/WebFeatures/WebFeature");
                }
                return webFeatureDependencies;
            }
        }

        /// <summary>
        /// true: Include item's version(high performance); false: exclude item's version(low performance)
        /// </summary>
        public bool IncludeVersionForPerformance { get; set; }
        public int ItemBackupMultiThreadCount { get; set; }

        public void Init(XmlElement config, bool IncludeVersions, ref bool changed)
        {
            if (config != null)
            {
                IncludeVersionForPerformance = IncludeVersions;
                XmlNode bpos_s = WrapperConfiguration.EnsureXmlNode(config, "BPOS-S", ref changed);
                HttpWebRequestTimeout = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "HttpWebRequestTimeout", 600000, ref changed);
                HttpWebRequestReadWriteTimeout = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "HttpWebRequestReadWriteTimeout", 1800000, ref changed);
                ClientRequestRetryInterval = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "ClientRequestRetryInterval", 200, ref changed);
                ClientRequestDuration = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "ClientRequestDuration", 500, ref changed);
                UseADFSAuthentication = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "UseADFSAuthentication", false, ref changed);
                UserClaimsAuthentication = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "UseClaimsAuthentication", false, ref changed);
                IncludeListView = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "IncludeListView", true, ref changed);
                SearchPrincipal = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "SearchPrincipal", false, ref changed);
                BackupItemVersionByAPI = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "BackupItemVersionByAPI", true, ref changed);
                UploadLimit = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "UploadLimit", 10, ref changed) * 1024 * 1024;
                SleepTime = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "SleepTime", 20000, ref changed);
                HealthScoreWarningValue = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "HealthScoreWarningValue", 7, ref changed);
                HealthScoreSleepTime = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "HealthScoreSleepTime", 15000, ref changed);
                HealthScoreThrottledTimeout = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "HealthScoreThrottledTimeout", 300000, ref changed);
                RetryCount = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "RetryCount", 60, ref changed);
                EnableHealthScoreMonitor = WrapperConfiguration.GetConfigrationFromNode(bpos_s, "EnableHealthScoreMonitor", true, ref changed);
            }
        }

        /// <summary>
        /// 由于office 365 备份不到feature 的dependencies。从config 文件中读取feature dependencies
        /// 由于配置文件是按照我们定义的，所以不进行容错处理，如果有人为修改错误，直接返回空集合。
        /// </summary>
        /// <param name="scopePath">读取的node 节点path</param>
        /// <param name="cache">存放cache 集合</param>
        private Dictionary<Guid, List<Guid>> GetFeatureDependencies(string scopePath)
        {
            Dictionary<Guid, List<Guid>> cache = new Dictionary<Guid, List<Guid>>(); 
            try
            {
                var path = Path.Combine(AveEnv.AgentDataFolder, dependenciesFilePath);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                var features = xmlDoc.SelectNodes(scopePath);
                foreach (XmlNode featureNode in features)
                {
                    var id = new Guid(featureNode.Attributes["Id"].Value);
                    var dependencies = new List<Guid>();
                    foreach (XmlNode dependcyNode in featureNode.SelectNodes("Dependencies/Feature"))
                    {
                        var dependcyId = new Guid(dependcyNode.Attributes["Id"].Value);
                        dependencies.Add(dependcyId);
                    }
                    cache[id] = dependencies;
                }
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, "An error occurred while getting feature dependencies. Error:{0}", e);
            }
            return cache;
        }

    }
    public partial class WrapperConfiguration
    {
        public class WrapperConfigurationForBPOS
        {
            private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            static WrapperConfigurationForBPOS()
            {
                ListTemplatesInMeetingSite = new List<string> { "Meetings", "Agenda", "MeetingUser", "Decision", "MeetingObjective", "TextBox", "ThingsToBring", "HomePageLibrary" };
                LoadRootFolderUniqueId = false;
                MaximumThreadsGettingVersions = 20;
                MultiListCountNeedSkipSchemal = 1000;
                SystemFileBeModifiedTiemSpan = 30 * 60;

                HttpWebRequestTimeout = 600000; //ten minutes
                HttpCreateWebRequestTimeout = 600000;
                HttpWebRequestReadWriteTimeout = 1800000; //30 minutes
                UseADFSAuthentication = false;
                UploadLimit = 10 * 1024 * 1024;//10 MB
                IncludeListView = true;
                IncludeFormPageWebpart = false;
                DisableInformationRightsManagement = false;
                IncludeVersionForPerformance = true;
                IncludeSystemUpdate = true;
                ItemBackupMultiThreadCount = 0;

                RetryCount = 3 * 2 + 1;
                RetryInterval = 5000;
                RetryMaxTotalSeconds = 30 * 60;
                WebNotThrottledButHighScoreSleepSecondsLimit = 10;
                ForbiddenWebExceptionRetryTimesLimit = 3 * 2 + 1;
                HealthScoreWarningValue = 7;
                HealthScoreSleepTime = 5000 * 3;
                HealthScoreThrottledTimeout = 180000;
                LoginRetryCount = 3;
                LoginRetryInterval = 5000;
                AuthCookieLifttime = 7;
                DetailLog = false;
                IsMultiThreadRestore = true;
                AddWebRetryCount = 6;
                EnsureDigest = true;

                SpecialListTemplateIdsUnderPersonalSite = new List<int> { 113, 116, 121, 123, 124, 175 };

                MonitorResourceUsageException = false;
                EnableMultiLanguage = false;
                MultiLanguageList = new List<string> { "de-DE", "en-US", "fr-FR" };

                VersionCount = -1;
                UpdateSpecificLinks = false;
            }

            public static List<string> ListTemplatesInMeetingSite { get; set; }

            /// <summary>
            /// 默认不load root folder的unique id，因为Replicator有老数据，所以需要保持该逻辑。
            /// 
            /// 对于其他需要unique id的模块，就需要load，外围自己设置该参数。
            /// </summary>
            public static bool LoadRootFolderUniqueId { get; set; }

            public static int MaximumThreadsGettingVersions { get; set; }
            public static int MultiListCountNeedSkipSchemal { get; set; }
            public static long SystemFileBeModifiedTiemSpan { get; set; }
            public static int HttpWebRequestTimeout { get; set; }
            public static int HttpCreateWebRequestTimeout { get; set; }
            public static int HttpWebRequestReadWriteTimeout { get; set; }
            public static bool UseADFSAuthentication { get; set; }
            public static long UploadLimit { get; set; }

            /// <summary>
            /// true: Include item's version(high performance); false: exclude item's version(low performance)
            /// </summary>
            public static bool IncludeListView { get; set; }
            public static bool IncludeFormPageWebpart { get; set; }
            /// <summary>
            /// When create a item with link content type, false will keep source, true will replace with the target reference link
            /// Currently, only replicator could set this property value
            /// </summary>
            public static bool UseTargetReferenceOfLinkContentTypeItem { get; set; }
            public static bool DisableInformationRightsManagement { get; set; }
            public static bool IncludeVersionForPerformance { get; set; }
            /// <summary>
            /// Whether Granular IB includes system update or not
            /// </summary>
            public static bool IncludeSystemUpdate { get; set; }
            public static int ItemBackupMultiThreadCount { get; set; }
            public static int RetryCount { get; set; }
            public static int RetryInterval { get; set; }
            /// <summary>
            /// max retry time span for retry-after
            /// </summary>
            public static int RetryMaxTotalSeconds { get; set; }
            /// <summary>
            ///  WebNotThrottled But HighScore is greater than HealthScoreWarningValue
            /// </summary>
            public static int WebNotThrottledButHighScoreSleepSecondsLimit { get; set; }
            public static int ForbiddenWebExceptionRetryTimesLimit { get; set; }
            public static int HealthScoreWarningValue { get; set; }
            public static int HealthScoreSleepTime { get; set; }
            public static int HealthScoreThrottledTimeout { get; set; }
            public static int LoginRetryCount { get; set; }
            public static int LoginRetryInterval { get; set; }
            public static int AuthCookieLifttime { get; set; }
            public static bool DetailLog { get; set; }
            public static bool IsMultiThreadRestore { get; set; }
            public static int AddWebRetryCount { get; set; }

            public static bool EnsureDigest { get; set; }

            /// <summary>
            /// 需要过滤的template ids
            /// </summary>
            public static List<int> SpecialListTemplateIdsUnderPersonalSite { get; set; }

            /// <summary>
            /// Monitor resource usage exception in the invoke function, only backup could do this.
            /// </summary>
            public static bool MonitorResourceUsageException { get; set; }

            public static bool EnableMultiLanguage { get; set; }
            public static List<string> MultiLanguageList { get; set; }


            public static int VersionCount { get; set; }

            public static bool UpdateSpecificLinks { get; set; }

            public static bool OnlyGetCurrentVersion { get; set; }
            private static bool isIncludeShareLinks;
            public static bool IsIncludeShareLinks
            {
                get
                {
                    return isIncludeShareLinks;
                }
                set
                {
                    mLog.Info($"Set IsIncludeShareLinks to {value}.");
                    isIncludeShareLinks = value;
                }
            }
            public static bool ArchiverRestoreSkipKeepVersionNumber { get; set; }

            public static bool HasArchiverBackupDataWriterException { get; set; }
            public static bool RestoreLookupFieldById { get; set; }
            public static string RecordsBCSColumnInternalName { get; set; }
            public static bool IsEndUserRestore { get; set; }
            public static bool HasItemLevelNode { get; set; }
            public static bool SkipWebPartError { get; set; }
        }
    }
}