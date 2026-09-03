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
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.RA.CommonUtil;
    using log4net;
    using log4net.Repository.Hierarchy;
    //using AvePoint.GCommon.Utility.Cryptography;
    #endregion

    /// <summary>
    /// 日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    //public class AveLogger : IAveLogger
    //{
    //    static string loggingPrefix;
    //    static string loggingPostfix;
    //    /// <summary>
    //    /// Global级别的，如果Global给disable了，那对象内部的也无效。
    //    /// </summary>
    //    static bool checkSensitiveKeyword = true;
    //    /// <summary>
    //    /// Current对象级别的。
    //    /// </summary>
    //    private bool checkSensitiveKeywordInContext = true;
    //    protected bool log2File = AveLoggerContext.EnableTrace;
    //    IAveLoggerImp loggerImp;
    //    protected static string jobId;

    //    private static readonly object mOverwriteLocker = new object();
    //    private static bool hasUploaded = false;

    //    public static RollingMode RollingStyle
    //    {
    //        get
    //        {
    //            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
    //            Logger rootLogger = hierarchy.Root;
    //            AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

    //            if (appender == null)
    //            {
    //                return default(RollingMode);
    //            }
    //            return appender.RollingStyle;
    //        }
    //        set
    //        {
    //            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
    //            Logger rootLogger = hierarchy.Root;
    //            AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

    //            if (appender == null)
    //            {
    //                return;
    //            }
    //            appender.RollingStyle = value;
    //            appender.ActivateOptions();
    //        }
    //    }

    //    public AveLogger(IAveLoggerImp imp, bool checkSensitiveKeyword)
    //    {
    //        this.loggerImp = imp;
    //        this.checkSensitiveKeywordInContext = checkSensitiveKeyword;
    //    }
    //    /// <summary>
    //    /// for GA+ online, GA+ use windowsazure.diagnotise for log, only use trace log and listener, no need log to file
    //    /// </summary>
    //    /// <param name="type"></param>
    //    /// <param name="onlineMode"></param>
    //    public AveLogger(Type type, string onlineMode)
    //    {
    //        if (!string.IsNullOrEmpty(onlineMode) && onlineMode.Equals("performance", StringComparison.OrdinalIgnoreCase))
    //        {
    //            this.loggerImp = new AveLoggerImp(type, onlineMode);
    //        }
    //        else
    //        {
    //            this.loggerImp = new AveLoggerAzureImpl(type);
    //            this.checkSensitiveKeywordInContext = false;
    //            this.log2File = false;
    //        }
    //    }

    //    public AveLogger(Type type, bool checkSensitiveKeyword)
    //        : this(new AveLoggerImp(type), checkSensitiveKeyword)
    //    {
    //    }

    //    public AveLogger(IAveLoggerImp imp)
    //        : this(imp, true)
    //    {
    //    }

    //    public AveLogger(Type type)
    //        : this(type, true)
    //    {
    //    }

    //    public static string JobId
    //    {
    //        get { return jobId; }
    //    }

    //    /// <summary>
    //    /// 根据Type获取logger实例。
    //    /// <example>
    //    /// AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
    //    /// </example>
    //    /// </summary>
    //    /// <param name="type">Type</param>
    //    /// <returns>logger实例</returns>
    //    public static AveLogger GetInstance(Type type)
    //    {
    //        return GetInstance(type, true);
    //    }

    //    public static AveLogger GetInstance(Type type, bool checkSensitiveKeyword)
    //    {
    //        return new AveLogger(type);
    //    }

    //    public static AveLogger GetInstance(Type type, string onlineMode)
    //    {
    //        return new AveLogger(type, onlineMode);
    //    }

    //    /// <summary>
    //    /// 通过设置Job Id更改Log File Name.
    //    /// </summary>
    //    /// <param name="jobId">Job Id或者你要改的log File Name</param>
    //    public static void SetJobId(string jobId)
    //    {
    //        AveLogger.jobId = jobId;
    //        AveLoggerImp.SetJobId(jobId);
    //    }

    //    public static void SeparateLog(string logFileName)
    //    {
    //        AveLoggerImp.SeparateLog(logFileName);
    //    }

    //    public static void WaitForAllLogsFlush()
    //    {
    //        AveLoggerImp.WaitForAllLogsFlush();
    //    }

    //    /// <summary>
    //    /// 设置Thread的JobId
    //    /// </summary>
    //    /// <param name="jobId"></param>
    //    /// <param name="bSeperateFile">是否按照线程将Log文件分开</param>
    //    public static void SetThreadJobId(string jobId, bool bSeperateFile = true)
    //    {
    //        AveLogger.jobId = jobId;
    //        if (bSeperateFile)
    //        {
    //            AveLoggerImp.SetThreadJobId(jobId);
    //        }
    //        else
    //        {
    //            AveLoggerImp.SetJobId(jobId);
    //        }
    //    }

    //    /// <summary>
    //    /// 设置log的前缀， eg:[agent version=5.6.0.0]	[CIID=20101026114717058#-1#Demo#33]
    //    /// </summary>
    //    /// <param name="logPrefix">log prefix to set</param>
    //    public static void SetCustomizedLogPrefix(string logPrefix)
    //    {
    //        loggingPrefix = logPrefix;
    //    }

    //    /// <summary>
    //    /// 设置log的后缀， eg: 5.6.0.0 实际上一个消息作为前缀还是后缀没有什么特别的，一回事，完全看怎么使用。
    //    /// </summary>
    //    /// <param name="logPostfix">log postfix to set</param>
    //    public static void SetCustomizedLogPostfix(string logPostfix)
    //    {
    //        loggingPostfix = logPostfix;
    //    }

    //    /// <summary>
    //    /// 用来标识是否检查敏感关键字
    //    /// </summary>
    //    public static bool CheckSensitiveKeyword
    //    {
    //        get { return checkSensitiveKeyword; }
    //        set { checkSensitiveKeyword = value; }
    //    }

    //    #region --public properties--
    //    public AveLogLevel CurrentLogLevel { get { return loggerImp.CurrentLogLevel; } }
    //    public bool IsErrorEnabled { get { return loggerImp.IsErrorEnabled; } }
    //    public bool IsWarnEnabled { get { return loggerImp.IsWarnEnabled; } }
    //    public bool IsInfoEnabled { get { return loggerImp.IsInfoEnabled; } }
    //    public bool IsDebugEnabled { get { return loggerImp.IsDebugEnabled; } }

    //    #endregion

    //    #region --public log method--

    //    #region --Debug methods--

    //    /// <summary>
    //    /// 写debug level的日志，要注意formatStr和args的匹配
    //    /// </summary>
    //    /// <param name="formatStr">用来格式化后面参数的字符串</param>
    //    /// <param name="args">可变个数的参数</param>
    //    public void Debug(string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (!IsDebugEnabled) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            WriteEntry(finalMsg, AveLogLevel.DEBUG, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    //        [Obsolete]
    //    //        public void Debug(int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Debug(ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Debug(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }

    //    #endregion

    //    #region --Info methods--

    //    /// <summary>
    //    /// 写info level的日志，要注意formatStr和args的匹配
    //    /// </summary>
    //    /// <param name="formatStr">用来格式化后面参数的字符串</param>
    //    /// <param name="args">可变个数的参数</param>
    //    public void Info(string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (!IsInfoEnabled) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    public void InfoEncryptMessage(string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (!IsInfoEnabled) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            var byteArr = Encoding.UTF8.GetBytes(finalMsg);
    //            finalMsg = Convert.ToBase64String(byteArr);
    //            WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    //        [Obsolete]
    //    //        public void Info(int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Info(ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Info(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Info(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage)
    //    //        {
    //    //
    //    //        }

    //    #endregion

    //    #region --Warn methods--

    //    /// <summary>
    //    /// 写warn level的日志，要注意formatStr和args的匹配
    //    /// </summary>
    //    /// <param name="formatStr">用来格式化后面参数的字符串</param>
    //    /// <param name="args">可变个数的参数</param>
    //    public void Warn(string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (!IsWarnEnabled) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            WriteEntry(finalMsg, AveLogLevel.WARN, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    //        [Obsolete]
    //    //        public void Warn(int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Warn(ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Warn(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }

    //    #endregion

    //    #region --Error methods--

    //    /// <summary>
    //    /// 写error level的日志，要注意formatStr和args的匹配
    //    /// </summary>
    //    /// <param name="formatStr">用来格式化后面参数的字符串</param>
    //    /// <param name="args">可变个数的参数</param>
    //    public void Error(string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (!IsErrorEnabled) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            WriteEntry(finalMsg, AveLogLevel.ERROR, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    //        [Obsolete]
    //    //        public void Error(int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Error(ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, int errorCode, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }

    //    #endregion

    //    #region --Log methods--

    //    public void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args)
    //    {
    //        try
    //        {
    //            if (CurrentLogLevel > aveLogLevel) return;

    //            string finalMsg = GetFinalMessage(formatStr, args);
    //            WriteEntry(finalMsg, aveLogLevel, 0, 0, EventSources.Empty);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    //        [Obsolete]
    //    //        public void Log(AveLogLevel aveLogLevel, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Log(AveLogLevel aveLogLevel, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }
    //    //
    //    //        [Obsolete]
    //    //        public void Log(AveLogLevel aveLogLevel, EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
    //    //        {
    //    //
    //    //        }

    //    //[Obsolete]
    //    //public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage, AveErrorCodeException exception)
    //    //{
    //    //}

    //    public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage)
    //    {
    //        try
    //        {
    //            WriteEntry(eventMessage.EventMessage, eventMessage.LogLevel, eventMessage.EventId, taskCategory, eventSource, eventMessage.EventException);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceWarning(e.ToString());
    //        }
    //    }

    //    #endregion

    //    #endregion

    //    private string GetFinalMessage(string formatStr, params object[] args)
    //    {
    //        string finalMsg = string.Empty;
    //        if (args.Length == 0)
    //        {
    //            finalMsg = formatStr; //兼容原来的 (string msg) 函数
    //        }
    //        else if (args.Length == 1 && formatStr.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) == -1)
    //        {
    //            finalMsg = string.Format("{0}\t{1}", formatStr, args[0]);//兼容原来的 (string msg，Exception e) 函数
    //        }
    //        else
    //        {
    //            finalMsg = string.Format(formatStr, args);//兼容原来的 (string formatStr, params object[] args) 函数
    //        }

    //        if (!string.IsNullOrEmpty(loggingPrefix))
    //        {
    //            finalMsg = loggingPrefix + "    " + finalMsg;
    //        }
    //        if (!string.IsNullOrEmpty(loggingPostfix))
    //        {
    //            finalMsg = finalMsg + "    " + loggingPostfix;
    //        }
    //        if (log2File)//GA+ online will skip this becasue he will call trace log later
    //        {
    //            Trace.WriteLine(finalMsg);
    //        }
    //        return finalMsg;
    //    }

    //    private void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, EventSources eventSource, Exception e = null)
    //    {
    //        string result = string.Empty;

    //        //if (checkSensitiveKeyword && checkSensitiveKeywordInContext && (level == AveLogLevel.ERROR || level == AveLogLevel.WARN))
    //        //{
    //        //    bool containSensitive = AnalyzeMessage(msg, out result);

    //        //    if (containSensitive)
    //        //    {
    //        //        eventId = 0;
    //        //    }
    //        //}
    //        //else
    //        {
    //            result = msg;
    //        }
    //        string realEventSource = EventSourcesUtil.ToEventSourceString(eventSource);
    //        loggerImp.WriteEntry(result, level, eventId, taskCategory, realEventSource, e);
    //    }

    //    #region --check sensitive keyword--
    //    private static string[] tableNames = new string[] { "AllDocs", "AllDocStreams", "AllDocVersions", "AllLinks", "AllLists", "AllUserData", "AllUserDataJunctions" ,//SharePoint DB
    //                                         "AuditData","BuildDependencies","Categories","CollationNames","ComMd","ContentTypes","ContentTypeUsage","EventLog",
    //                                         "Deps","DiskWarningDate","EventBatches","EventCache","EventReceivers","EventSubsMatches","Features","GroupMembership",
    //                                         "Groups","HT_Cache","HT_Settings","Image0x","ImmedSubscriptions","NavNodes","Perms","Personalization","RecycleBin",
    //                                         "RoleAssignment","Roles","SchedSubscriptions","ScheduledWorkItems","SiteQuota","Sites","SiteVersions","TimerLock",
    //                                         "UserInfo","Versions","WebCat","WebMembers","WebPartLists","WebParts","Webs","WelcomeNames","Workflow","WorkflowAssociation",

    //                                         "AntiVirusVendors","Binaries","Classes","CustomTemplates","Databases","Dependencies","EmailEnabledLists","GLOBALS",//SharePoint_Config
    //                                         "InstalledWebPartPackages","LastUpdate","Objects","PendingDistributionLists","SiteCounts","Servers","Services",
    //                                         "SiteMap","TimerLocks","TimerRunningJobs","TimerTargetInstances","Tombstones","VirtualServers","WebPartPackages",

    //                                         "MSSAlertDocHistory","MSSAnchorChangeLog","MSSAnchorPendingChangeLog","MSSAnchorText","MSSAnchorTransactions",//SharedServices_Search_DB
    //                                         "MSSBatchHistory","MSSChangeLogCookies","MSSClickDistanceSeeds","MSSCrawlChangedSourceDocs","MSSCrawlChangedTargetDocs",
    //                                         "MSSCrawlContent","MSSCrawlDeletedErrorList","MSSCrawlDeletedURL","MSSCrawledPropSamples","MSSCrawledPropSamplesCleanup",
    //                                         "MSSCrawlErrorList","MSSCrawlHistory","MSSCrawlHostList","MSSCrawlQueue","MSSCrawlURL","MSSCrawlURLLog","MSSDefinitions",
    //                                         "MSSDocDeleteList","MSSDocProps","MSSDocSDIDS","MSSDuplicateHashes","MSSNextDocID","MSSPropagationPropagationTask",
    //                                         "MSSPropagationSearchServerReady","MSSPropagationSearchServerTable","MSSSecurityDescriptors","MSSSessionDefinitions",
    //                                         "MSSSessionDefinitionsAlt","MSSSessionDocProps","MSSSessionDocPropsAlt","MSSSessionDocSdids","MSSSessionDocSdidsAlt",
    //                                         "MSSSessionDocSignatures","MSSSessionDocSignaturesAlt","MSSSessionDuplicateHashes","MSSSessionDuplicateHashesAlt",
    //                                         "MSSSessionExistingDocs","MSSSessionExistingDocsAlt","MSSTranTempTable0",
    //                                       };

    //    private static string[] SQLKeywords = new string[] { "Cannot insert duplicate key", "System.Data.SqlClient" };

    //    private static bool AnalyzeMessage(string message, out string result)
    //    {
    //        bool containSensitiveKeywords = false;

    //        result = message;

    //        if (!string.IsNullOrEmpty(message))
    //        {
    //            try
    //            {
    //                bool valid = false;
    //                //只检查Agent模块的logger
    //                if (string.IsNullOrEmpty(AveLoggerImp.DefaultConfigurationFile) || AveLoggerImp.DefaultConfigurationFile.EndsWith("AgentLog4net.config", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    valid = true;
    //                }

    //                //包含该[Dump binary]:说明Wrapper里面已经封装了，不需要在check
    //                if (valid && message.IndexOf("[Dump binary]:", StringComparison.OrdinalIgnoreCase) == -1)
    //                {
    //                    foreach (string tableName in tableNames)
    //                    {
    //                        if (message.IndexOf("dbo." + tableName, StringComparison.OrdinalIgnoreCase) >= 0)
    //                        {
    //                            containSensitiveKeywords = true;
    //                            break;
    //                        }
    //                    }

    //                    if (!containSensitiveKeywords)
    //                    {
    //                        foreach (string sqlKeyword in SQLKeywords)
    //                        {
    //                            if (message.IndexOf(sqlKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
    //                            {
    //                                containSensitiveKeywords = true;
    //                                break;
    //                            }
    //                        }
    //                    }

    //                    if (containSensitiveKeywords)
    //                    {
    //                        if (message.Length > 20)
    //                        {
    //                            string header = message.Substring(0, 20).ToLower();
    //                            header = header.Replace("sql", "native");
    //                            header = header.Replace("execute", "Get");
    //                            result = string.Format("{0}.........\r\n{1}", header, WrapperException(message));
    //                        }
    //                        else
    //                        {
    //                            result = string.Format("Native exception:\r\n{0}", WrapperException(message));
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                result += string.Format("\r\n{0}", ex.Message);
    //            }
    //        }

    //        return containSensitiveKeywords;
    //    }

    //    /// <summary>
    //    /// 防止Stack Overflow Exception，因为加密里面也是用AveLogger
    //    /// </summary>
    //    [ThreadStatic]
    //    private static int count;

    //    private static string WrapperException(string exception)
    //    {
    //        string encryptedInfo = string.Empty;

    //        if (exception != null)
    //        {
    //            try
    //            {
    //                count++;
    //                encryptedInfo = exception.ToString();
    //                if (count < 3)
    //                {
    //                    encryptedInfo = string.Format("[Dump binary]:{0}\r\n", InternalCrypto.EncryptMessage(exception.ToString()));
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                encryptedInfo += ex.ToString();
    //            }
    //            finally
    //            {
    //                count--;
    //            }
    //        }

    //        return encryptedInfo;
    //    }

    //    /// <summary>
    //    /// 为了避免很多Link，所以重新整理一个给AveLogger来加密一些信息
    //    /// </summary>
    //    private class InternalCrypto
    //    {
    //        private static byte[] key = { 15, 218, 43, 167, 98, 156, 234, 134 };
    //        private static byte[] iv = { 145, 138, 67, 7, 198, 56, 224, 113 };

    //        public static string EncryptMessage(string message)
    //        {
    //            string result = string.Empty;

    //            if (!string.IsNullOrEmpty(message))
    //            {
    //                try
    //                {
    //                    using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
    //                    {
    //                        using (MemoryStream stream = new MemoryStream())
    //                        {
    //                            using (CryptoStream cryptoStream = new CryptoStream(stream, aesProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write))
    //                            {
    //                                byte[] buffer = Encoding.UTF8.GetBytes(message);
    //                                cryptoStream.Write(buffer, 0, buffer.Length);
    //                                cryptoStream.Close();
    //                                result = Convert.ToBase64String(stream.ToArray());
    //                            }
    //                        }
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
    //                }
    //            }

    //            return result;
    //        }

    //        public static string DecryptMessage(string message)
    //        {
    //            string result = string.Empty;

    //            if (!string.IsNullOrEmpty(message))
    //            {
    //                try
    //                {
    //                    using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
    //                    {
    //                        using (MemoryStream stream = new MemoryStream())
    //                        {
    //                            using (CryptoStream cryptoStream = new CryptoStream(stream, aesProvider.CreateDecryptor(key, iv), CryptoStreamMode.Write))
    //                            {
    //                                byte[] buffer = Convert.FromBase64String(message);//Encoding.UTF8.GetBytes(message);
    //                                cryptoStream.Write(buffer, 0, buffer.Length);
    //                                cryptoStream.Close();
    //                                result = Encoding.UTF8.GetString(stream.ToArray());
    //                            }
    //                        }
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    result = string.Format("[Details]:{0}\r\n[Exception]:{1}", message, ex.ToString());
    //                }
    //            }

    //            return result;
    //        }
    //    }

    //    #endregion


    //}

    public class AveLogger : RALogger, IAveLogger
    {
        AveLogLevel IAveLogger.CurrentLogLevel => AveLogLevel.DEBUG;

        public AveLogger(Type type) : base(type)
        {
        }

        public AveLogger(RA.CommonUtil.IAveLoggerImp imp) : base(imp)
        {
        }

        public AveLogger(RA.CommonUtil.IAveLoggerImp imp, bool checkSensitiveKeyword) : base(imp, checkSensitiveKeyword)
        {
        }

        public AveLogger(Type type, bool checkSensitiveKeyword) : base(type, checkSensitiveKeyword)
        {
        }

        public static new AveLogger GetInstance(Type type)
        {
            return new AveLogger(type);
        }

        public void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args)
        {
            base.Log(ConvertLogLevel(aveLogLevel), formatStr, args);
        }

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

        public void InfoEncryptMessage(string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                string finalMsg = GetFinalMessage(formatStr, args);
                var byteArr = Encoding.UTF8.GetBytes(finalMsg);
                finalMsg = Convert.ToBase64String(byteArr);
                WriteEntry(finalMsg, AveLogLevel.INFO, 0, 0, EventSources.Empty);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        private void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, EventSources eventSource, Exception e = null)
        {
            string result = string.Empty;

            result = msg;
            string realEventSource = EventSourcesUtil.ToEventSourceString(eventSource);

            base.loggerImp.WriteEntry(result, ConvertLogLevel(level), eventId, taskCategory, realEventSource, e);
        }

        private AvePoint.RA.Contract.Services.AveLogLevel ConvertLogLevel(AvePoint.GCommon.AveLogLevel level)
        {
            AvePoint.RA.Contract.Services.AveLogLevel logLevel = RA.Contract.Services.AveLogLevel.INFO;
            switch (level)
            {
                case AveLogLevel.ERROR:
                    logLevel = RA.Contract.Services.AveLogLevel.ERROR;
                    break;
                case AveLogLevel.WARN:
                    logLevel = RA.Contract.Services.AveLogLevel.WARN;
                    break;
                case AveLogLevel.INFO:
                    logLevel = RA.Contract.Services.AveLogLevel.INFO;
                    break;
                case AveLogLevel.DEBUG:
                    logLevel = RA.Contract.Services.AveLogLevel.DEBUG;
                    break;
            }
            return logLevel;
        }

        public static void SetThreadJobId(string jobId)
        { }
    }

    /// <summary>
    /// Logger Common Options
    /// </summary>
    public class AveLoggerContext
    {
        public static bool EnableTrace { get; set; }
        public static bool EnableCacheMode { get; set; }
        public static string LogConfigurationFile { get; set; }

        static AveLoggerContext()
        {
            EnableTrace = ReadFromConfiguration("logEnableTrace", true);
            EnableCacheMode = !ReadFromConfiguration("disableLogCacheMode", false);
            LogConfigurationFile = ReadFromConfiguration("logConfigurationFile", null);
        }

        static bool ReadFromConfiguration(string key, bool defaultValue)
        {
            string keyValue = ConfigurationManager.AppSettings[key];
            bool keyBoolValue = false;
            if (!bool.TryParse(keyValue, out keyBoolValue))
            {
                keyBoolValue = defaultValue;
            }
            return keyBoolValue;
        }

        static string ReadFromConfiguration(string key, string defaultValue)
        {
            string keyValue = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(keyValue))
            {
                return defaultValue;
            }
            return keyValue;
        }
    }
}