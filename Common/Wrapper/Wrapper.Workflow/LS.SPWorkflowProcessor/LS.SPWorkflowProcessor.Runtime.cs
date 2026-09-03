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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

using LS.SPWorkflowProcessor.Services;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using AvePoint.Common;
using AvePoint.GCommon.Utility;

namespace LS.SPWorkflowProcessor
{
    public enum TemplateFileConflictRulesEnum
    {
        KeepTarget,
        KeepSource,
    }

    public class SPWorkflowProcessorRuntime
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static string mRootDirectory;
        private static List<RuntimeService> mServices;

        public static string RootDirectory
        {
            get { return mRootDirectory; }
        }

        public static bool ProcessAssociation
        {
            get;
            set;
        }
        public static bool ProcessInstance
        {
            get;
            set;
        }

        private static bool mProcessMarkOnlyWorkflow = true;
        public static bool ProcessMarkOnlyWorkflow
        {
            get { return mProcessMarkOnlyWorkflow; }
            set { mProcessMarkOnlyWorkflow = value; }
        }

        private static bool mUpdateCurrentVersion = false;

        public static bool UpdateCurrentVersion
        {
            get { return mUpdateCurrentVersion; }
            set { mUpdateCurrentVersion = value; }
        }

        public static bool PerformanceMonitorIsOn
        {
            get;
            set;
        }

        private static bool mRestoreHistoryOnly = true;
        public static bool RestoreHistoryOnly
        {
            get { return mRestoreHistoryOnly; }
            set { mRestoreHistoryOnly = value; }
        }

        private static bool mSkipRunningInstance = false;
        public static bool SkipRunningInstance
        {
            get { return mSkipRunningInstance; }
            set { mSkipRunningInstance = value; }
        }

        private static bool mFindTaskItemByOriginalUniqueIdOnly = true;
        public static bool FindTaskItemByOriginalUniqueIdOnly
        {
            get
            {
                return mFindTaskItemByOriginalUniqueIdOnly;
            }
            set
            {
                mFindTaskItemByOriginalUniqueIdOnly = value;
            }
        }

        private static int mPauseTimeAfterCancelWorkflow = 8;
        public static int PauseTimeAfterCancelWorkflow
        {
            get { return mPauseTimeAfterCancelWorkflow; }
            set { mPauseTimeAfterCancelWorkflow = value; }
        }

        private static int mPauseTimeBeforeRestartWorkflow = 1;
        public static int PauseTimeBeforeRestartWorkflow
        {
            get { return mPauseTimeBeforeRestartWorkflow; }
            set { mPauseTimeBeforeRestartWorkflow = value; }
        }

        public static bool Process13ModelWFInstanceByNative
        {
            get;
            set;
        }

        public static bool RestartRunningInstance
        {
            get;
            set;
        }

        public static bool RestoreBuiltinOnly
        {
            get;
            set;
        }

        public static bool RestoreParentAssociationIfNotFound
        {
            get;
            set;
        }

        private static AveObjectModelFactory mFactory = null;

        public static AveObjectModelFactory ObjectModelFactory
        {
            get
            {
                if (mFactory == null)
                {
                    mFactory = WrapperRuntime.CurrentContext.ModelFactory;
                }
                return mFactory;
            }
            set 
            {
                mFactory = value;
            }
        }

        private static int mCachedAssociationCount = 5;
        public static int CachedAssociationCount
        {
            get { return mCachedAssociationCount; }
            set { mCachedAssociationCount = value; }
        }

        private static TemplateFileConflictRulesEnum mTemplateFileConflictRules = TemplateFileConflictRulesEnum.KeepSource;
        public static TemplateFileConflictRulesEnum TemplateFileConflictRules
        {
            get { return mTemplateFileConflictRules; }
            set { mTemplateFileConflictRules = value; }
        }

        public static bool OverwriteNWMessageTemplates
        {
            get;
            set;
        }

        public static bool OverwriteNWContants
        {
            get;
            set;
        }

        public static Dictionary<string, string> AllProcessorParams
        {
            get;
            set;
        }

        public static List<ICustomWorkflowAssociationProc> CustomAssociationProcessors
        { get; set; }

        public static List<ICustomWorkflowInstanceProc> CustomInstanceProcessors
        { get; set; }

        public static Dictionary<Guid, SPWorkflowFileContentCustomProc> CustomTemplateContentProcessors
        { get; set; }


        private static void AddProcessorToCollection(XmlElement processor)
        {

            if (processor.Name.Equals("AssociationProcessor"))
            {
                AddProcessorToCollection(processor, null);
            }
            else if (processor.Name.Equals("InstanceProcessor"))
            {
                AddProcessorToCollection(processor, null);
            }
            else if (processor.Name.Equals("Service"))
            {
                AddProcessorToCollection(processor, null);
            }
            else
            {
                Guid templateId = Guid.Empty;
                if (!processor.Name.Equals("Default"))
                    templateId = new Guid(processor.GetAttribute("TemplateId"));

                SPWorkflowFileContentCustomProc customProc = new SPWorkflowFileContentCustomProc();
                foreach (XmlNode node in processor.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;

                    XmlElement fileProc = (XmlElement)node;
                    AddProcessorToCollection(fileProc, customProc);
                }
                if (customProc.AspxFileProcessor != null || customProc.ConfigFileProcessor != null || customProc.RulesFileProcessor != null || customProc.XomlFileProcessor != null)
                    CustomTemplateContentProcessors.Add(templateId, customProc);
            }
        }

        private static void AddProcessorToCollection(XmlElement processor, SPWorkflowFileContentCustomProc customFileProc)
        {
            string dir = processor.GetAttribute("Assembly");
            string cls = processor.GetAttribute("Class");
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(cls))
                return;

            Assembly predefinedAssem = typeof(SPWFInstanceUnit).Assembly;
            Type procType = predefinedAssem.GetType(cls, false);
            if (procType == null)
            {
                if (dir.StartsWith("[LS]", StringComparison.Ordinal))
                    dir = dir.Substring(4);

                if (dir.EqualIgnoreCase("\\bin\\AgentCommonWrapperWorkflow.dll"))
                {
                    dir = SecurityUtils.SafeCombinePath(mRootDirectory, "bin\\AgentCommonWrapperWorkflow.dll");
                    Assembly assem = Assembly.LoadFrom(dir);
                    procType = assem.GetType(cls, false);
                }
                else if (dir.EqualIgnoreCase("\\bin\\AgentCommonWrapperRestore.dll"))
                {
                    dir = SecurityUtils.SafeCombinePath(mRootDirectory, "bin\\AgentCommonWrapperRestore.dll");
                    Assembly assem = Assembly.LoadFrom(dir);
                    procType = assem.GetType(cls, false);
                }
                else
                {
                    return;
                }
            }
            if (procType == null)
                return;

            if (processor.Name.Equals("AssociationProcessor"))
            {
                //ICustomWorkflowAssociationProc
                ICustomWorkflowAssociationProc instance = (ICustomWorkflowAssociationProc)LSInvoker.CreateNewInstance(procType);
                CustomAssociationProcessors.Add(instance);
            }
            else if (processor.Name.Equals("InstanceProcessor"))
            {
                ICustomWorkflowInstanceProc instance = (ICustomWorkflowInstanceProc)LSInvoker.CreateNewInstance(procType);
                CustomInstanceProcessors.Add(instance);
            }
            else if (processor.Name.Equals("Service"))
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                foreach (XmlNode paramNode in processor.ChildNodes)
                {
                    if (paramNode.Name.Equals("Parameter"))
                    {
                        XmlElement paramElement = (XmlElement)paramNode;
                        string key = paramElement.GetAttribute("Name");
                        string value = paramElement.GetAttribute("Value");
                        if (value.StartsWith("[LS]", StringComparison.Ordinal))
                            value = mRootDirectory + value.Substring(4);
                        if (!param.ContainsKey(key))
                            param.Add(key, value);
                    }
                }
                AddService(procType,param);
                return;
            }
            else
            {
                SPWorkflowFileContentProc instance = (SPWorkflowFileContentProc)LSInvoker.CreateNewInstance(procType);
                if (processor.Name.Equals("ConfigContentProcessor"))
                {
                    customFileProc.ConfigFileProcessor = instance;
                }
                else if (processor.Name.Equals("XOMLContentProcessor"))
                {
                    customFileProc.XomlFileProcessor = instance;
                }
                else if (processor.Name.Equals("RulesContentProcessor"))
                {
                    customFileProc.RulesFileProcessor = instance;
                }
                else if (processor.Name.Equals("FormContentProcessor"))
                {
                    customFileProc.AspxFileProcessor = instance;
                }
                else if (processor.Name.Equals("XAMLContentProcessor"))
                {
                    customFileProc.XamlFileProcessor = instance;
                }
            }

            foreach (XmlNode paramNode in processor.ChildNodes)
            {
                if (paramNode.Name.Equals("Parameter"))
                {
                    XmlElement paramElement = (XmlElement)paramNode;
                    string key = paramElement.GetAttribute("Name");
                    string value = paramElement.GetAttribute("Value");
                    if (!AllProcessorParams.ContainsKey(key))
                        AllProcessorParams.Add(key, value);
                }
            }
        }


        static string mCurrentConfigFile;

        public static void LoadConfiguration(string configFile, string rootDirectory)
        {
            if (string.Equals(configFile, mCurrentConfigFile))
            {
                return;
            }
            if (!File.Exists(configFile))
            {
                return;
            }
            mCurrentConfigFile = configFile;
            mRootDirectory = rootDirectory.TrimEnd('\\');
            AllProcessorParams = new Dictionary<string, string>();
            CustomAssociationProcessors = new List<ICustomWorkflowAssociationProc>();
            CustomInstanceProcessors = new List<ICustomWorkflowInstanceProc>();
            CustomTemplateContentProcessors = new Dictionary<Guid, SPWorkflowFileContentCustomProc>();
            XmlDocument doc = null;
            XmlNode firstNode = null;
            try
            {
                if (!File.Exists(configFile))
                    return;
                doc = new XmlDocument();
                doc.Load(configFile);

                firstNode = (XmlNode)doc.DocumentElement;
                if (firstNode.Name != "LS.Workflow") 
                {
                    return;
                }
                foreach (XmlNode node in firstNode.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;
                    XmlElement xe = (XmlElement)node;
                    if (xe.Name == "Configuration")
                    {
                        #region Self
                        if (xe.GetAttribute("ProcessAssociation").Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessAssociation = true;
                        }
                        if (xe.GetAttribute("ProcessInstance").Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessInstance = true;
                        }
                        if (xe.GetAttribute("ProcessMarkOnlyWorkflow").Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            ProcessMarkOnlyWorkflow = false;
                        }
                        if (xe.GetAttribute("PerformanceMonitor").Equals("on", StringComparison.OrdinalIgnoreCase))
                        {
                            PerformanceMonitorIsOn = true;
                        }
                        if (xe.HasAttribute("CachedAssociationCount"))
                        {
                            CachedAssociationCount = int.Parse(xe.GetAttribute("CachedAssociationCount"));
                        }
                        if (xe.HasAttribute("PauseTimeBeforeRestartWorkflow"))
                        {
                            PauseTimeBeforeRestartWorkflow = int.Parse(xe.GetAttribute("PauseTimeBeforeRestartWorkflow"));
                        }
                        try
                        {
                            if (xe.HasAttribute("TemplateFileConflictRules"))
                            {
                                TemplateFileConflictRules = (TemplateFileConflictRulesEnum)Enum.Parse(typeof(TemplateFileConflictRulesEnum), xe.GetAttribute("TemplateFileConflictRules"), true);
                            }
                        }
                        catch (ArgumentException e) {
                            logger.Error($"error occured when LoadConfiguration,error:{e}");
                        }

                        if (xe.GetAttribute("HistoryOnly").Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            RestoreHistoryOnly = false;
                        }
                        if (xe.GetAttribute("SkipRunningInstance").Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            SkipRunningInstance = true;
                        } 
                        if (xe.GetAttribute("RestartRunningInstance").Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            RestartRunningInstance = true;
                        }

                        //RestoreBuiltinOnly
                        if (xe.GetAttribute("RestoreBuiltinOnly").Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            RestoreBuiltinOnly = false;
                        }

                        //RestoreParentAssociationIfNotFound
                        if (xe.GetAttribute("RestoreParentAssociationIfNotFound").Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            RestoreParentAssociationIfNotFound = true;
                        }

                        //FindTaskItemByOriginalUniqueIdOnly
                        if (xe.GetAttribute("FindTaskItemByOriginalUniqueIdOnly").Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            FindTaskItemByOriginalUniqueIdOnly = false;
                        }
                        #endregion
                    }
                    else if (xe.Name.Equals("CustomAssociationProcessors") || xe.Name.Equals("CustomInstanceProcessors") || xe.Name.Equals("CustomTemplateContentProcessors") || xe.Name.Equals("RuntimeServices"))
                    {
                        foreach (XmlNode node2 in xe.ChildNodes)
                        {
                            if (!(node2 is XmlElement))
                                continue;
                            AddProcessorToCollection((XmlElement)node2);
                        }
                    }
                    else if (xe.Name.Equals("Mappings", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadMapping(xe);
                    }

                }

            }
            catch(Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.ConfigFileLoadError, ex);
            }
            finally
            {
                if (doc != null)
                    doc.RemoveAll();
            }
        }

        private static void LoadMapping(XmlElement xe)
        {
            try
            {
                foreach (XmlNode node in xe.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;
                    XmlElement mappingNode = node as XmlElement;
                    if (mappingNode.Name.Equals("EmailMappings", StringComparison.Ordinal))
                    {
                        foreach (XmlNode emailNode in mappingNode.ChildNodes)
                        {
                            if (!(emailNode is XmlElement))
                                continue;
                            XmlElement email = emailNode as XmlElement;
                            if (email.HasAttribute("sourceEmail") && email.HasAttribute("destinationEmail"))
                            {
                                string src = email.GetAttribute("sourceEmail");
                                string des = email.GetAttribute("destinationEmail");
                                if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(des))
                                {
                                    SPWorkflowCommon.EmailMapping[src.ToLower()] = des;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RuntimeLoadMappingError, e);
            }
        }

        public static void AddService(RuntimeService service, Dictionary<string, string> param)
        {
            if (mServices == null)
                mServices = new List<RuntimeService>();
            mServices.Add(service);

        }

        public static void AddService(Type type,Dictionary<string,string> param)
        {
            if (!type.IsSubclassOf(typeof(RuntimeService)))
                return;
            RuntimeService instance = (RuntimeService)LSInvoker.CreateNewInstance(type,new Type[]{typeof(Dictionary<string,string>)},new object[]{param});
            
            AddService(instance,param);
        }

        public static WebPostponeActionService GetWebPostponeActionService()
        {
            lock (mServices)
            {
                foreach (var service in mServices)
                {
                    if (service is WebPostponeActionService)
                    {
                        return (WebPostponeActionService)service;
                    }
                }
                var param = new Dictionary<string, string>
                {
                    { "RootDirectory", AveEnv.AgentTempFolder }
                };
                var postActionService = new WebPostponeActionService(param);
                mServices.Add(postActionService);
                return postActionService;
            }
            
        }

        public static void Stop()
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
                service.Dispose();
            mServices.Clear();
        }



        public static void Log(string key, params string[] args)
        {
            if (mServices == null)
                return;

            foreach (RuntimeService service in mServices)
            {
                switch (service.RuntimeServiceType)
                {
                    case ServiceType.LoggingService:
                        ((LoggingService)service).WriteLog(key, args);
                        break;
                    default:
                        break;
                }
            }
        }

        public static IAveUser OnUserMapping(string loginName)
        {
            if (mServices == null)
                return null;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.UserMappingService:
                            return ((UserMappingService)service).GetOrCreateUser(loginName);
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.MappingUserError, e);
                }
            }
            return null;
        }

        public static void OnCacheData(string siteId, string webId, string listId, string parentId, int itemId, string index, byte[] data)
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.CacheService:
                            ((CacheService)service).CacheData(siteId, webId, listId, parentId, itemId, index, data);
                            return;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.CacheDataError, e);
                }
            }
        }

        public static void ExecutePostAction(SPWFAssociationProc associationProcessor, SPWFAssociationProc associationProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.PostponeActionService:
                            ((PostponeActionService)service).Execute(associationProcessor, instanceProcessor);
                            continue;
                        case ServiceType.WFDataFilterService:
                            if (instanceProcessor13Model != null && instanceProcessor13Model.ParentItem != null && instanceProcessor13Model.ParentItem.Web != null)
                            {
                                ((WFTaskAndHistoryDataFilter)service).Filter(instanceProcessor13Model.ParentItem.Web, instanceProcessor13Model.TaskListIdAndInstanceMapping, instanceProcessor13Model.HistoryListIdAndInstanceMapping);
                            }
                            continue;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.PostActionError, e);
                }
            }
        }

        public static string OnLanguageMapping(LanguageMappingScopeEnum scope, string originalName)
        {
            if (mServices == null)
                return originalName;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.LanguageMappingService:
                            return ((LanguageMappingService)service).GetMappedName(scope, originalName);
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.LanguageMappingError, e);
                }
            }
            return originalName;
        }

        public static byte[] BackupCustomData(Guid siteId, Guid webId)
        {
            try
            {
                if (NintexWorkflowInstanceProc.IsNintexDllInstalled)
                {
                    Dictionary<string, List<Hashtable>> data = new Dictionary<string, List<Hashtable>>();

                    List<Hashtable> templates = new List<Hashtable>();
                    using (NintexMessageTemplateHelper messageTemplateHelper = new NintexMessageTemplateHelper())
                    {
                        messageTemplateHelper.BackupMessageTemplates(webId, templates);
                    }

                    data.Add("Nintex.MessageTemplates", templates);


                    List<Hashtable> contants = new List<Hashtable>();
                    using (NintexWorkflowConstantHelper contantHelper = new NintexWorkflowConstantHelper())
                    {
                        contantHelper.BackupConstants(webId, contants);
                    }


                    data.Add("Nintex.WorkflowConstants", contants);



                    #region Serialize & Compress Custom Data
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, data);
                    byte[] serializedData = LSUtilityOfBytes.LSStreamToBytes(stream);


                    using (MemoryStream stream2 = new MemoryStream(serializedData.Length))
                    {
                        using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                        {
                            stream3.Write(serializedData, 0, serializedData.Length);
                        }
                        serializedData = stream2.GetBuffer();
                        Array.Resize<byte>(ref serializedData, Convert.ToInt32(stream2.Length));
                    }

                    stream.Dispose();


                    #endregion

                    #region Dispose
                    foreach (KeyValuePair<string, List<Hashtable>> pair in data)
                    {
                        foreach (Hashtable ht in pair.Value)
                        {
                            ht.Clear();
                        }
                        pair.Value.Clear();
                    }
                    data.Clear();
                    #endregion

                    return serializedData;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RestoreCustomDataError, ex);
            }

            return null;
        }

        /*public static void RestoreCustomData(Guid siteId, Guid webId, byte[] serializedData)
        {
            byte[] decompressedData = new byte[0];
            #region Decompress & Deserialized Custom Data
            using (MemoryStream tempStream = new MemoryStream(serializedData))
            {
                tempStream.Position = 0L;
                byte[] temp = new byte[4096];
                using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
                {
                    int readLen;
                    while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                    {
                        LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                    }
                }
                temp = null;
            }


            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(decompressedData);
            Dictionary<string, List<Hashtable>> data = (Dictionary<string, List<Hashtable>>)formatter.Deserialize(stream);
            stream.Dispose();
            decompressedData = null;

            #endregion


            try
            {

                foreach (KeyValuePair<string, List<Hashtable>> pairs in data)
                {
                    if (pairs.Key.StartsWith("nintex", StringComparison.OrdinalIgnoreCase))
                    {
                        if (NintexWorkflowInstanceProc.IsNintexDllInstalled)
                        {
                            string tableName = pairs.Key.Split(new char[] { '.' })[1];
                            if (tableName.Equals("MessageTemplates", StringComparison.OrdinalIgnoreCase))
                            {
                                using (NintexMessageTemplateHelper messageTemplateHelper = new NintexMessageTemplateHelper())
                                {
                                    messageTemplateHelper.RestoreMessageTemplates(siteId, webId, pairs.Value);
                                }
                            }
                            else if (tableName.Equals("WorkflowConstants", StringComparison.OrdinalIgnoreCase))
                            {
                                using (NintexWorkflowConstantHelper contantHelper = new NintexWorkflowConstantHelper())
                                {
                                    contantHelper.RestoreConstants(siteId, webId, pairs.Value);
                                }
                            }
                        }
                    }
                }

            }
            catch(Exception ex) 
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RestoreCustomDataError, ex);
            }
        }*/

        /// <summary>
        /// 缓存当前job中还原过的reusable workflow template id
        /// 内部有并发控制，不需要在调用的地方额外加锁
        /// </summary>
        private static ReusableWorkflowTemplateBaseIdCache restoredWorkflowTemplateIdCache = new ReusableWorkflowTemplateBaseIdCache();
        public static ReusableWorkflowTemplateBaseIdCache RestoredWorkflowTemplateIdCache
        {
            get { return restoredWorkflowTemplateIdCache; }
            set { restoredWorkflowTemplateIdCache = value; }
        }
    }
    public class ReusableWorkflowTemplateBaseIdCache
    {
        private readonly object privateLock = new object();

        private Dictionary<Guid, Dictionary<Guid, List<Guid>>> mRestoredReusableWorkflowTemplateCache;

        public ReusableWorkflowTemplateBaseIdCache()
        {
            mRestoredReusableWorkflowTemplateCache = new Dictionary<Guid, Dictionary<Guid, List<Guid>>> { };
        }

        public void Add(Guid siteId, Guid webId, Guid templateBaseId)
        {
            lock (privateLock)
            {
                if (!mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    mRestoredReusableWorkflowTemplateCache.Add(siteId, new Dictionary<Guid, List<Guid>> { });
                }
                if (!mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId))
                {
                    mRestoredReusableWorkflowTemplateCache[siteId].Add(webId, new List<Guid> { });
                }
                if (!mRestoredReusableWorkflowTemplateCache[siteId][webId].Contains(templateBaseId))
                {
                    mRestoredReusableWorkflowTemplateCache[siteId][webId].Add(templateBaseId);
                }
            }
        }

        public bool Contains(Guid siteId, Guid webId, Guid templateBaseId)
        {
            lock (privateLock)
            {

                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId)
                    && mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId)
                        && mRestoredReusableWorkflowTemplateCache[siteId][webId].Contains(templateBaseId))
                {
                    return true;
                }
                return false;
            }
        }

        public void Remove(Guid siteId, Guid webId)
        {
            lock (privateLock)
            {

                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    if (webId == Guid.Empty)
                    {
                        Remove(siteId);
                    }
                    else if (mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId))
                    {
                        mRestoredReusableWorkflowTemplateCache[siteId][webId].Clear();
                        mRestoredReusableWorkflowTemplateCache[siteId].Remove(webId);
                    }
                }
            }
        }

        public void Remove(Guid siteId)
        {
            lock (privateLock)
            {
                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    foreach (var value in mRestoredReusableWorkflowTemplateCache[siteId].Values)
                    {
                        value.Clear();
                    }
                    mRestoredReusableWorkflowTemplateCache[siteId] = null;
                    mRestoredReusableWorkflowTemplateCache.Remove(siteId);
                }
            }
        }


    }

}
