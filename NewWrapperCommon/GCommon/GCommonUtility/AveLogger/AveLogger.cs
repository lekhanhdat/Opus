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



namespace AvePoint.GCommon
{
    #region using directives
    using AvePoint.GCommon.Utility.I18N;
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Xml;
    //using AvePoint.GCommon.Utility.Cryptography;
    #endregion

    /// <summary>
    /// 日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    public class AveLogger : IAveLogger
    {
        static int spVersion = -1;
        static string loggingPrefix;
        static string loggingPostfix;
        private const string MetricLoggerSuffix = ".Metric";
        /// <summary>
        /// Global级别的，如果Global给disable了，那对象内部的也无效。
        /// </summary>
        static bool checkSensitiveKeyword = true;
        /// <summary>
        /// Current对象级别的。
        /// </summary>
        private bool checkSensitiveKeywordInContext = true;
        private readonly Type loggingType;
        private readonly string loggerName;
        IAveLoggerImp loggerImp;

        static object ensureSpVersionLock = new object();

        private static void EnsureSPVersion()
        {
            lock (ensureSpVersionLock)
            {
                if (spVersion == -1)
                {
                    if (Assembly.GetExecutingAssembly().FullName.StartsWith("CommonUtility", StringComparison.OrdinalIgnoreCase))
                    {
                        spVersion = 0;
                    }
                    else
                    {
                        spVersion = SPVersionDetection.GetSPVersion();
                    }
                }
            }
        }

        private static IAveLoggerImp GetDefaultAveLoggerImpInstance()
        {
#if DEBUG
            while (File.Exists("C:\\PauseAveLogger.txt"))
            {
                Thread.Sleep(2000);
            }
#endif
            EnsureSPVersion();
            if (spVersion == 0)
            {
                return new AveLoggerLog4netImp();
            }
            else
            {
                return new AveLoggerULSImp(spVersion);
            }
        }

        private static IAveLoggerImp CreateLoggerImpInstance(Type type, string loggerName)
        {
            IAveLoggerImp logger = GetDefaultAveLoggerImpInstance();
            logger.SetLoggingType(type);
            logger.SetLoggerName(loggerName);
            logger.InitializeInstance();

            return logger;
        }

        private static string GetMetricLoggerName(Type type, string loggerName)
        {
            string loggerIdentity = string.IsNullOrEmpty(loggerName) ? type.FullName : loggerName;
            return string.Concat(loggerIdentity, MetricLoggerSuffix);
        }

        #region --Constructor--

        public AveLogger(Type type)
            : this(type, true)
        {
        }

        public AveLogger(Type type, string loggerName)
            : this(type, loggerName, true)
        {
        }

        public AveLogger(Type type, bool checkSensitiveKeyword)
            : this(type, string.Empty, checkSensitiveKeyword)
        {
        }

        [Obsolete]
        public AveLogger(Type type, bool checkSensitiveKeyword, bool usedForILMerge)
            : this(type, checkSensitiveKeyword)
        {
        }

        public AveLogger(Type type, string loggerName, bool checkSensitiveKeywordInContext)
        {
            //we use this way instead of using implementation constructor, 
            //because it will be easier for passing args to implementation later, it needs change abstract class and some of implementation
            this.loggingType = type;
            this.loggerName = loggerName;
            this.loggerImp = CreateLoggerImpInstance(type, loggerName);

            this.checkSensitiveKeywordInContext = checkSensitiveKeywordInContext;
        }

        #endregion

        #region --GetInstance()--

        /// <summary>
        /// 根据Type获取logger实例。
        /// <example>
        /// AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// </example>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>logger实例</returns>
        public static AveLogger GetInstance(Type type)
        {
            return GetInstance(type, true);
        }

        public static AveLogger GetInstance(Type type, string loggerName)
        {
            return GetInstance(type, loggerName, true);
        }

        public static AveLogger GetInstance(Type type, bool checkSensitiveKeyword)
        {
            return GetInstance(type, string.Empty, checkSensitiveKeyword);
        }

        [Obsolete]
        public static AveLogger GetInstance(Type type, bool checkSensitiveKeyword, bool usedForILMerge)
        {
            return GetInstance(type, string.Empty, checkSensitiveKeyword);
        }

        public static AveLogger GetInstance(Type type, string loggerName, bool checkSensitiveKeyword)
        {
            return new AveLogger(type, loggerName, checkSensitiveKeyword);
        }

        #endregion

        /// <summary>
        /// 通过设置Job Id更改Log File Name.
        /// </summary>
        /// <param name="jobId">Job Id或者你要改的log File Name</param>
        public static void SetJobId(string jobId, bool mergeOldFile = true)
        {
            GetDefaultAveLoggerImpInstance().SetJobId(jobId, mergeOldFile);
        }

        public static void SeparateLogToTenant(string tenantId, string tenantAccount)
        {
            GetDefaultAveLoggerImpInstance().SeparateLogByTenant(tenantId, tenantAccount);
        }

        public static void SeparateLogToTenant(string jobId, string tenantId, string tenantAccount, bool bSeperateFile = false)
        {
            if (bSeperateFile)
            {
                lock (ensureSpVersionLock)
                {
                    GetDefaultAveLoggerImpInstance().SetTreadLogTenantAndJobId(jobId, tenantId, tenantAccount);
                }
            }
            else
            {
                GetDefaultAveLoggerImpInstance().SeparateLogByTenant(jobId, tenantId, tenantAccount);
            }
        }

        public static void SetLogLevel(AveLogLevel logLevel)
        {
            GetDefaultAveLoggerImpInstance().SetLogLevel(logLevel);
        }

        public static void WaitForAllLogsFlush()
        {
            GetDefaultAveLoggerImpInstance().WaitForAllLogsFlush();
        }

        /// <summary>
        /// 设置Thread的JobId
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="bSeperateFile">是否按照线程将Log文件分开</param>
        public static void SetThreadJobId(string jobId, bool bSeperateFile = true)
        {
            if (bSeperateFile)
            {
                lock (ensureSpVersionLock)
                {
                    GetDefaultAveLoggerImpInstance().SetThreadJobId(jobId);
                }
            }
            else
            {
                GetDefaultAveLoggerImpInstance().SetJobId(jobId, true);
            }
        }

        /// <summary>
        /// 设置log的前缀， eg:[agent version=5.6.0.0]	[CIID=20101026114717058#-1#Demo#33]
        /// </summary>
        /// <param name="logPrefix">log prefix to set</param>
        public static void SetCustomizedLogPrefix(string logPrefix)
        {
            loggingPrefix = logPrefix;
        }

        /// <summary>
        /// 设置log的后缀， eg: 5.6.0.0 实际上一个消息作为前缀还是后缀没有什么特别的，一回事，完全看怎么使用。
        /// </summary>
        /// <param name="logPostfix">log postfix to set</param>
        public static void SetCustomizedLogPostfix(string logPostfix)
        {
            loggingPostfix = logPostfix;
        }

        /// <summary>
        /// 用来标识是否检查敏感关键字
        /// </summary>
        public static bool CheckSensitiveKeyword
        {
            get { return checkSensitiveKeyword; }
            set { checkSensitiveKeyword = value; }
        }

        /// <summary>
        /// 把deployId更新到Config文件中的header中，保证更新后再更换的Log文件可以在文件开始处输出deployId
        /// </summary>
        /// <param name="deployId">需要更新的DeployId</param>
        public void SetDeployIdToLogFile(String deployId)
        {
            loggerImp.SetDeployId(deployId);
        }

        #region --public properties--
        public AveLogLevel CurrentLogLevel { get { return loggerImp.CurrentLogLevel; } }
        public bool IsErrorEnabled { get { return CurrentLogLevel == AveLogLevel.ERROR || CurrentLogLevel == AveLogLevel.WARN || CurrentLogLevel == AveLogLevel.INFO || CurrentLogLevel == AveLogLevel.DEBUG; } }
        public bool IsWarnEnabled { get { return CurrentLogLevel == AveLogLevel.WARN || CurrentLogLevel == AveLogLevel.INFO || CurrentLogLevel == AveLogLevel.DEBUG; } }
        public bool IsInfoEnabled { get { return CurrentLogLevel == AveLogLevel.INFO || CurrentLogLevel == AveLogLevel.DEBUG; } }
        public bool IsDebugEnabled { get { return CurrentLogLevel == AveLogLevel.DEBUG; } }

        #endregion

        #region --public log method--

        #region --Debug methods--

        /// <summary>
        /// 写debug level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Debug(string formatStr, params object[] args)
        {
            try
            {
                if (!IsDebugEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.DEBUG, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Debug(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Debug(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Debug(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Info methods--

        /// <summary>
        /// 写info level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Info(string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        public void Metric(string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                string metricLoggerName = GetMetricLoggerName(this.loggingType, this.loggerName);
                IAveLoggerImp metricLogger = CreateLoggerImpInstance(this.loggingType, metricLoggerName);
                metricLogger.WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSourcesUtil.ToEventSourceString(EventSources.Empty), null);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        /// <summary>
        /// 与Metric类似，但不添加log前缀/后缀，也不写入Trace，仅使用formatStr和args组装消息内容。
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void MetricRaw(string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                string finalMsg = FormatMetricMessage(formatStr, args);
                string metricLoggerName = GetMetricLoggerName(this.loggingType, this.loggerName);
                IAveLoggerImp metricLogger = CreateLoggerImpInstance(this.loggingType, metricLoggerName);
                metricLogger.WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSourcesUtil.ToEventSourceString(EventSources.Empty), null);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Info(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage)
        //        {
        //
        //        }

        #endregion

        #region --Warn methods--

        /// <summary>
        /// 写warn level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Warn(string formatStr, params object[] args)
        {
            try
            {
                if (!IsWarnEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.WARN, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Warn(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Warn(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Warn(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Error methods--

        /// <summary>
        /// 写error level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Error(string formatStr, params object[] args)
        {
            try
            {
                if (!IsErrorEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.ERROR, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Error(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, int errorCode, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Log methods--

        public void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args)
        {
            try
            {
                if (CurrentLogLevel > aveLogLevel) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, aveLogLevel, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        //[Obsolete]
        //public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage, AveErrorCodeException exception)
        //{
        //}

        public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage)
        {
            try
            {
                WriteEntry(eventMessage.EventMessage, eventMessage.LogLevel, eventMessage.EventId, taskCategory, eventSource, eventMessage.EventException);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        #endregion

        #endregion

        private static string FormatMetricMessage(string formatStr, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return formatStr;
            }

            if (formatStr.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var builder = new StringBuilder();
                builder.Append(formatStr);

                foreach (var item in args)
                {
                    builder.Append("; ");
                    builder.Append(item);
                }

                return builder.ToString();
            }

            return string.Format(formatStr, args);
        }

        private string GetFinalMessage(string formatStr, params object[] args)
        {
            string finalMsg = formatStr;
            try
            {
                if (args.Length == 0)
                {
                    finalMsg = formatStr; //兼容原来的 (string msg) 函数
                }
                else if (args.Length >= 1 && formatStr.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    //finalMsg = string.Format("{0}\t{1}", formatStr, args[0]);//兼容原来的 (string msg，Exception e) 函数

                    var builder = new StringBuilder();
                    builder.Append(formatStr);

                    foreach (var item in args)
                    {
                        builder.Append("; ");
                        builder.Append(item);
                    }

                    finalMsg = builder.ToString();
                }
                else
                {
                    finalMsg = string.Format(formatStr, args);//兼容原来的 (string formatStr, params object[] args) 函数
                }

                if (!string.IsNullOrEmpty(loggingPrefix))
                {
                    finalMsg = loggingPrefix + "    " + finalMsg;
                }
                if (!string.IsNullOrEmpty(loggingPostfix))
                {
                    finalMsg = finalMsg + "    " + loggingPostfix;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            Trace.WriteLine(finalMsg);
            return finalMsg;
        }

        private void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, EventSources eventSource, Exception e = null)
        {
            string result = string.Empty;

            if (checkSensitiveKeyword && checkSensitiveKeywordInContext && (level == AveLogLevel.ERROR || level == AveLogLevel.WARN))
            {
                bool containSensitive = AnalyzeMessage(msg, out result);

                if (containSensitive)
                {
                    eventId = 0;
                }
            }
            else
            {
                result = msg;
            }
            string realEventSource = EventSourcesUtil.ToEventSourceString(eventSource);
            loggerImp.WriteEntry(result, level, eventId, taskCategory, realEventSource, e);
        }

        #region --check sensitive keyword--
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Check sensitive keyword.")]
        private static string[] tableNames = new string[] { "AllDocs", "AllDocStreams", "AllDocVersions", "AllLinks", "AllLists", "AllUserData", "AllUserDataJunctions" ,//SharePoint DB
                                             "AuditData","BuildDependencies","Categories","CollationNames","ComMd","ContentTypes","ContentTypeUsage","EventLog",
                                             "Deps","DiskWarningDate","EventBatches","EventCache","EventReceivers","EventSubsMatches","Features","GroupMembership",
                                             "Groups","HT_Cache","HT_Settings","Image0x","ImmedSubscriptions","NavNodes","Perms","Personalization","RecycleBin",
                                             "RoleAssignment","Roles","SchedSubscriptions","ScheduledWorkItems","SiteQuota","Sites","SiteVersions","TimerLock",
                                             "UserInfo","Versions","WebCat","WebMembers","WebPartLists","WebParts","Webs","WelcomeNames","Workflow","WorkflowAssociation",

                                             "AntiVirusVendors","Binaries","Classes","CustomTemplates","Databases","Dependencies","EmailEnabledLists","GLOBALS",//SharePoint_Config
                                             "InstalledWebPartPackages","LastUpdate","Objects","PendingDistributionLists","SiteCounts","Servers","Services",
                                             "SiteMap","TimerLocks","TimerRunningJobs","TimerTargetInstances","Tombstones","VirtualServers","WebPartPackages",

                                             "MSSAlertDocHistory","MSSAnchorChangeLog","MSSAnchorPendingChangeLog","MSSAnchorText","MSSAnchorTransactions",//SharedServices_Search_DB
                                             "MSSBatchHistory","MSSChangeLogCookies","MSSClickDistanceSeeds","MSSCrawlChangedSourceDocs","MSSCrawlChangedTargetDocs",
                                             "MSSCrawlContent","MSSCrawlDeletedErrorList","MSSCrawlDeletedURL","MSSCrawledPropSamples","MSSCrawledPropSamplesCleanup",
                                             "MSSCrawlErrorList","MSSCrawlHistory","MSSCrawlHostList","MSSCrawlQueue","MSSCrawlURL","MSSCrawlURLLog","MSSDefinitions",
                                             "MSSDocDeleteList","MSSDocProps","MSSDocSDIDS","MSSDuplicateHashes","MSSNextDocID","MSSPropagationPropagationTask",
                                             "MSSPropagationSearchServerReady","MSSPropagationSearchServerTable","MSSSecurityDescriptors","MSSSessionDefinitions",
                                             "MSSSessionDefinitionsAlt","MSSSessionDocProps","MSSSessionDocPropsAlt","MSSSessionDocSdids","MSSSessionDocSdidsAlt",
                                             "MSSSessionDocSignatures","MSSSessionDocSignaturesAlt","MSSSessionDuplicateHashes","MSSSessionDuplicateHashesAlt",
                                             "MSSSessionExistingDocs","MSSSessionExistingDocsAlt","MSSTranTempTable0",
                                           };

        private static string[] SQLKeywords = new string[] { "Cannot insert duplicate key", "System.Data.SqlClient", "SqlException" };

        private bool AnalyzeMessage(string message, out string result)
        {
            bool containSensitiveKeywords = false;

            result = message;

            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    //包含该[Dump binary]:说明Wrapper里面已经封装了，不需要在check
                    if (loggerImp.IsAgentContext() && message.IndexOf("[Dump binary]:", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        foreach (string tableName in tableNames)
                        {
                            if (message.IndexOf("dbo." + tableName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                containSensitiveKeywords = true;
                                break;
                            }
                        }

                        if (!containSensitiveKeywords)
                        {
                            foreach (string sqlKeyword in SQLKeywords)
                            {
                                if (message.IndexOf(sqlKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    containSensitiveKeywords = true;
                                    break;
                                }
                            }
                        }

                        if (containSensitiveKeywords)
                        {
                            if (message.Length > 20)
                            {
                                string header = message.Substring(0, 20).ToLowerInvariant();
                                header = header.Replace("sql", "native");
                                header = header.Replace("execute", "Get");
                                result = string.Format("{0}.........\r\n{1}", header, WrapperException(message));
                            }
                            else
                            {
                                result = string.Format("Native exception:\r\n{0}", WrapperException(message));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result += string.Format("\r\n{0}", ex.Message);
                }
            }

            return containSensitiveKeywords;
        }

        /// <summary>
        /// 防止Stack Overflow Exception，因为加密里面也是用AveLogger
        /// </summary>
        [ThreadStatic]
        private static int count = 0;

        private static string WrapperException(string exception)
        {
            string encryptedInfo = string.Empty;

            if (exception != null)
            {
                try
                {
                    count++;
                    encryptedInfo = exception.ToString();
                    if (count < 3)
                    {
                        encryptedInfo = string.Format("[Dump binary]:{0}\r\n", InternalCrypto.EncryptMessage(exception.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    encryptedInfo += ex.ToString();
                }
                finally
                {
                    count--;
                }
            }

            return encryptedInfo;
        }

        /// <summary>
        /// 为了避免很多Link，所以重新整理一个给AveLogger来加密一些信息
        /// </summary>
        private class InternalCrypto
        {
            private static byte[] key = { 15, 218, 43, 167, 98, 156, 234, 134 };
            private static byte[] iv = { 145, 138, 67, 7, 198, 56, 224, 113 };

            public static string EncryptMessage(string message)
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(stream, desProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                                    cryptoStream.Write(buffer, 0, buffer.Length);
                                    cryptoStream.Close();
                                    result = Convert.ToBase64String(stream.ToArray());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
                    }
                }

                return result;
            }

            public static string DecryptMessage(string message)
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        using (DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider())
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(stream, desProvider.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                                {
                                    byte[] buffer = Convert.FromBase64String(message);//Encoding.UTF8.GetBytes(message);
                                    cryptoStream.Write(buffer, 0, buffer.Length);
                                    cryptoStream.Close();
                                    result = Encoding.UTF8.GetString(stream.ToArray());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
                    }
                }

                return result;
            }
        }

        #endregion
    }
}
