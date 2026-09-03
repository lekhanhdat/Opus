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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Xml;
    #endregion
    public enum ModuleUserAgent
    {
        #region DAO
        Backup,
        Restore,
        CA,
        PE,
        DM,
        CM,
        CMHSM,
        RP,
        Archive,
        RC,
        Browser
        #endregion DAO
    }
    public enum ProductUserAgent
    {
        AOS=0,
        DAO=1,
        CB=2,
        GAO=3
    }
    public class Office365UserAgentGenerator
    {
        private static string mSharePointRequestUserAgent { get; set; }
        private static string mDefaultVersion { get; set; }

        static Office365UserAgentGenerator()
        {
            var defaultVersion = FileVersionInfo.GetVersionInfo(typeof(Office365UserAgentGenerator).Assembly.Location);
            mDefaultVersion = $"{defaultVersion.ProductMajorPart}.{defaultVersion.ProductMinorPart}";
            mSharePointRequestUserAgent = $"ISV|AvePoint|DAO/{mDefaultVersion}";
        }

        public static string Default { get { return mSharePointRequestUserAgent; } }

        public static string Create(ModuleUserAgent module, bool interactive = false, string version=null, string companyName="AvePoint", ProductUserAgent productName = ProductUserAgent.DAO)
        {
            string moduleName = module == ModuleUserAgent.Browser ? "" : module.ToString();
            return Create(companyName, productName.ToString(), moduleName, version, interactive);
        }
        public static string Create(string module, bool interactive = false, string version = null, string companyName = "AvePoint", ProductUserAgent productName = ProductUserAgent.DAO)
        {
            return Create(companyName, productName.ToString(), module, version, interactive);
        }
        public static string Create(string companyName, string productName, string module, string version, bool interactive)
        {
            return interactive ? $"ISV|{companyName}|{productName}{module}/{version ?? mDefaultVersion}|Interactive"
                 : $"ISV|{companyName}|{productName}{module}/{version ?? mDefaultVersion}";
        }

    }
    public partial class WrapperConfiguration {
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
                UniqueFieldsResolution = new UniqueFieldResolution()
                {
                    ConflictOption = UniqueFieldConflictOption.Field,
                    RestorationOption = UniqueFieldRestorationOption.Skip,
                };//need configurated in GUI, default value: skip unique value with Item level.

                MonitorResourceUsageException = false;
                EnableMultiLanguage = false;
                MultiLanguageList = new List<string> { "de-DE", "en-US", "fr-FR" };

                VersionCount = -1;
                UpdateSpecificLinks = false;
            }

            public static Guid O365TenantId { get; set; }

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
            public static bool LogToken { get; set; }
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


            public static UniqueFieldResolution UniqueFieldsResolution { get; set; }

            /// <summary>
            /// Monitor resource usage exception in the invoke function, only backup could do this.
            /// </summary>
            public static bool MonitorResourceUsageException { get; set; }

            public static bool EnableMultiLanguage { get; set; }
            public static List<string> MultiLanguageList { get; set; }

            public static void SetUserAgent(string userAgent)
            {
                DefaultUserAgent = userAgent;
                mLog.Info($"Set User Agent Tag for Office365 Request:{userAgent}");
            }

            public static string DefaultUserAgent { get; private set; } = Office365UserAgentGenerator.Default;

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
            public static List<ArchiverRestoreVersionMapping> ArchiverRestoreVersionMapping { get; set; } = new List<ArchiverRestoreVersionMapping>();

            public static bool HasArchiverBackupDataWriterException { get; set; }
            public static bool RestoreLookupFieldById { get; set; }
            public static string RecordsBCSColumnInternalName { get; set; }
            public static bool IsEndUserRestore { get; set; }
            public static bool SkipReplaceFoler { get; set; }

            public static bool IsRestoreToSPOLibOrFolder { get; set; }

            public static RestoreObjectLevel RestoreObjectLevel { get; set; }
            public static RestoreScope RestoreScope { get; set; }

            public static bool OverWriteReplaceFoler { get; set; }
            public static bool OverWriteApp { get; set; }
            public static bool HasItemLevelNode { get; set; }
            public static bool SkipWebPartError { get; set; }
            public static bool SkipCacheLookColumn { get; set; }
            public static bool HasLATRule { get; set; }
            public static bool SiteMgtApiEnable { get; set; } = true;
            public static bool IsSearchAllRestore { get; set; }
            public static DateTime LATMgtApiEnableTime { get; set; } = DateTime.MinValue;
            public static int QUERY_VALUES_LIMITE_FILE { get; set; } = 60;
            public static List<string> SkipRoleName { get; set; } = new List<string>();
            public static List<int> SkipRoleId { get; set; } = new List<int>();
        }
    }
    public class ArchiverRestoreVersionMapping
    {
        public int DoclibRowId { get; set; }
        public int PreviousRestoreFileBackupVersion { get; set; }
        public int PreviousRestoreFileMappingVersion { get; set; }
    }
}