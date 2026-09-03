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
using System.Reflection;
using System.IO;
using System.Threading;
using System.Xml;
using System.Resources;
using System.Windows.Forms;
using System.Collections;
using Microsoft.Win32;
using AvePoint.GCommon;
using AvePoint.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public enum AveLanguageMappingType
    {
        ListMapping,
        FieldMapping,
        PermissionMapping,
        ContentTypeMapping,
        NavigationMapping,
        ViewTitleMapping,
    }

    public enum AveSourceLanguagePlatForm
    {
        Undefined,
        Sharepoint07,
        Sharepoint10,
        Sharepoint13,
        Sharepoint16,
        SharepointOnline,
    }

    public class AveLanguageProcesser : IDisposable
    {
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static AveLanguageProcesser mLanguageProcessor;
        private static object mLock = new object();
        private uint mSrcId;
        private uint mDesId;
        private AutoResetEvent[] autoEvents;
        private object lockObj = new object();
        private Hashtable tempSrcResX = new Hashtable();
        private Hashtable tempDesResX = new Hashtable();
        private string mRootDir;
        private string mJobDir;
        static readonly List<uint> languagesInConfigXml = new List<uint> { 1033, 1041, 1031, 1036 };

        public AveVolatileCache<string, string> ListMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ViewTitleMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ListMappingFromRes = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> FieldMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> PermissionMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ContentTypeMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> NavigationMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<uint, string> ResourceFileMapping = new AveVolatileCache<uint, string>();
        public string ResXRootPath = string.Empty;
        public static readonly Dictionary<uint, string> CultureIdNameMapping = new Dictionary<uint, string>();
        private bool mustOnlyKeyAndValue = false;
        private List<string> mLanguageFilePath = null;
        public static AveObjectModelFactory Factory { get; set; }
        /// <summary>
        /// offline excel 功能需要缓存已经找到name，否则效率太慢了
        /// </summary>
        private Dictionary<string, string> mExcelMultiLanguageCache = new Dictionary<string, string>();
        private AveContextKind mContextKind;
        public AveContextKind ContextKind
        {
            get
            {
                return mContextKind;
            }
            set
            {
                mContextKind = value;
            }
        }

        private AveSourceLanguagePlatForm sourcePlatForm;
        public AveSourceLanguagePlatForm SourcePlatForm
        {
            get
            {
                return sourcePlatForm;
            }
            set
            {
                sourcePlatForm = value;
            }
        }

        private bool mIsMigration;
        public bool IsMigration
        {
            get
            {
                return mIsMigration;
            }
            set
            {
                mIsMigration = value;
            }
        }

        public void AddLanguageFilePath(string languageFilePath)
        {
            if (mLanguageFilePath == null)
            {
                mLanguageFilePath = new List<string>();
            }
            if (!mLanguageFilePath.Contains(languageFilePath))
            {
                mLanguageFilePath.Add(languageFilePath);
            }
        }

        public string JobDir
        {
            get { return mJobDir; }
        }

        public uint SrcId
        {
            get { return mSrcId; }
            set { mSrcId = value; }
        }

        public uint DesId
        {
            get { return mDesId; }
            set { mDesId = value; }
        }

        private AveLanguageProcesser()
        { }

        public static AveLanguageProcesser GetLanguageInstance(string rootDir, string jobDir, AveObjectModelFactory factory)
        {
            if (mLanguageProcessor == null)
            {
                lock (mLock)
                {
                    if (mLanguageProcessor == null)
                    {
                        Factory = factory;
                        mLanguageProcessor = new AveLanguageProcesser(rootDir, jobDir, factory);
                    }
                }
            }
            return mLanguageProcessor;
        }

        /// <summary>
        /// only used for new code, don't use this function in your code.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="jobDir"></param>
        /// <returns></returns>
        [Obsolete("This method will be deprecated and removed later. key--001")]
        internal static AveLanguageProcesser CreateInstance(string rootDir, string jobDir)
        {
            return new AveLanguageProcesser(rootDir, jobDir);
        }
        public static AveLanguageProcesser CreateTempLanguageProcesser()
        {
            var languageProcesser = new AveLanguageProcesser(AveEnv.AgentRootFolder, string.Empty);
            if (mLanguageProcessor != null)
            {
                languageProcesser.ContextKind = mLanguageProcessor.ContextKind;
                languageProcesser.IsMigration = mLanguageProcessor.IsMigration;
                languageProcesser.SourcePlatForm = mLanguageProcessor.SourcePlatForm;
            }
            return languageProcesser;
        }

        public static AveLanguageProcesser GetLanguageInstance(string rootDir, string jobDir)
        {
            if (mLanguageProcessor == null)
            {
                lock (mLock)
                {
                    if (mLanguageProcessor == null)
                    {
                        mLanguageProcessor = new AveLanguageProcesser(rootDir, jobDir);
                    }
                }
            }
            return mLanguageProcessor;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Language resource information")]
        static AveLanguageProcesser()
        {
            CultureIdNameMapping.Add(1031, "de-de");
            CultureIdNameMapping.Add(1033, "en-us");
            CultureIdNameMapping.Add(1035, "fi-fi");//
            CultureIdNameMapping.Add(1053, "sv-se");//
            CultureIdNameMapping.Add(1041, "ja-jp");
            CultureIdNameMapping.Add(1043, "nl-nl");//Holand
            CultureIdNameMapping.Add(1061, "et-ee");//
            CultureIdNameMapping.Add(1062, "lt-lt");//
            CultureIdNameMapping.Add(1063, "lv-lv");//
            CultureIdNameMapping.Add(2052, "zh-cn");
            CultureIdNameMapping.Add(1029, "cs-cz");//Czech
            CultureIdNameMapping.Add(1036, "fr-fr");//Franch
            CultureIdNameMapping.Add(1040, "it-it");//Italian
            CultureIdNameMapping.Add(1046, "pt-br");//portuguese-brazilian
            CultureIdNameMapping.Add(3082, "es-es");//Spanish
            CultureIdNameMapping.Add(1045, "pl-pl");//Polish
            CultureIdNameMapping.Add(1025, "ar-sa");//Saudi Arabia
            CultureIdNameMapping.Add(1037, "he-Il");//Hebrew
            CultureIdNameMapping.Add(1042, "ko-kr");//Korea
            CultureIdNameMapping.Add(3084, "fr-ca");
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Language resource information")]
        private AveLanguageProcesser(string rootDir, string jobDir)
        {
            this.mRootDir = rootDir;
            this.mJobDir = jobDir;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SPTimerV4");

            if (key != null)
            {
                ResXRootPath = key.GetValue("ImagePath").ToString();
                ResXRootPath = ResXRootPath.Substring(1, ResXRootPath.IndexOf("BIN\\OWSTIMER.EXE", StringComparison.OrdinalIgnoreCase) - 1) + "Resources\\";
            }

            //Add language ID mapped resource file
            ResourceFileMapping[(uint)1031] = "core.de-DE.resx";
            ResourceFileMapping[(uint)1033] = "core.en-US.resx";
            ResourceFileMapping[(uint)1035] = "core.fi-fi.resx";//
            ResourceFileMapping[(uint)1053] = "core.sv-se.resx";//
            ResourceFileMapping[(uint)1041] = "core.ja-JP.resx";
            ResourceFileMapping[(uint)1043] = "core.nl-nl.resx";//Holand
            ResourceFileMapping[(uint)1061] = "core.et-ee.resx";//
            ResourceFileMapping[(uint)1062] = "core.lt-lt.resx";//
            ResourceFileMapping[(uint)1063] = "core.lv-lv.resx";//
            ResourceFileMapping[(uint)2052] = "core.zh-CN.resx";
            ResourceFileMapping[(uint)1029] = "core.cs-cz.resx";//Czech
            ResourceFileMapping[(uint)1036] = "core.fr-fr.resx";//Franch
            ResourceFileMapping[(uint)1040] = "core.it-it.resx";//Italian
            ResourceFileMapping[(uint)1046] = "core.pt-br.resx";//portuguese-brazilian
            ResourceFileMapping[(uint)3082] = "core.es-es.resx";//Spanish
            ResourceFileMapping[(uint)1045] = "core.pl-pl.resx";//Polish
            ResourceFileMapping[(uint)1025] = "core.ar-SA.resx";//Saudi Arabia
            ResourceFileMapping[(uint)1037] = "core.he-Il.resx";//Hebrew
            ResourceFileMapping[(uint)1042] = "core.ko-KR.resx";//Korea
            ResourceFileMapping[(uint)3084] = "core.fr-CA.resx";
            autoEvents = new AutoResetEvent[]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Language resource information")]
        private AveLanguageProcesser(string rootDir, string jobDir, AveObjectModelFactory factory)
        {
            this.mRootDir = rootDir;
            this.mJobDir = jobDir;

            RegistryKey key = Factory.CreateAveRegister().OpenKeyForLanguageProcesser();

            if (key != null)
            {
                ResXRootPath = key.GetValue("ImagePath").ToString();
                ResXRootPath = ResXRootPath.Substring(1, ResXRootPath.IndexOf("BIN\\OWSTIMER.EXE", StringComparison.OrdinalIgnoreCase) - 1) + "Resources\\";
            }

            //Add language ID mapped resource file
            ResourceFileMapping[(uint)1031] = "core.de-DE.resx";
            ResourceFileMapping[(uint)1033] = "core.en-US.resx";
            ResourceFileMapping[(uint)1035] = "core.fi-fi.resx";//
            ResourceFileMapping[(uint)1053] = "core.sv-se.resx";//
            ResourceFileMapping[(uint)1041] = "core.ja-JP.resx";
            ResourceFileMapping[(uint)1043] = "core.nl-nl.resx";//Holand
            ResourceFileMapping[(uint)1061] = "core.et-ee.resx";//
            ResourceFileMapping[(uint)1062] = "core.lt-lt.resx";//
            ResourceFileMapping[(uint)1063] = "core.lv-lv.resx";//
            ResourceFileMapping[(uint)2052] = "core.zh-CN.resx";
            ResourceFileMapping[(uint)1029] = "core.cs-cz.resx";//Czech
            ResourceFileMapping[(uint)1036] = "core.fr-fr.resx";//Franch
            ResourceFileMapping[(uint)1040] = "core.it-it.resx";//Italian
            ResourceFileMapping[(uint)1046] = "core.pt-br.resx";//portuguese-brazilian
            ResourceFileMapping[(uint)3082] = "core.es-es.resx";//Spanish
            ResourceFileMapping[(uint)1045] = "core.pl-pl.resx";//Polish
            ResourceFileMapping[(uint)1025] = "core.ar-SA.resx";//Saudi Arabia
            ResourceFileMapping[(uint)1037] = "core.he-Il.resx";//Hebrew
            ResourceFileMapping[(uint)1042] = "core.ko-KR.resx";//Korea
            ResourceFileMapping[(uint)3084] = "core.fr-CA.resx";
            autoEvents = new AutoResetEvent[]
            {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };
        }
        public void LoadMapping(string path, uint srcId, uint desId, string xml, bool mustOnlyKeyAndValue = false)
        {
            if (mSrcId == srcId && mDesId == desId)
            {
                return;
            }
            this.mustOnlyKeyAndValue = mustOnlyKeyAndValue;
            try
            {
                Clear();
                
                if (!languagesInConfigXml.Contains(srcId) || !languagesInConfigXml.Contains(desId))
                {
                    LoadListMappingFromResX();
                }
                switch (mContextKind)
                {
                    //todo:qlluo: language mapping
                    case AveContextKind.Server13ObjectModel:
                        LoadConfigXml(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommon2013LanguageMappingFile, srcId, desId, string.Empty);
                        break;
                    case AveContextKind.Server16ObjectModel:
                    case AveContextKind.Server19ObjectModel:
                    case AveContextKind.ClientObjectModel:
                        LoadConfigXml(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommonOffice365LanguageMappingFile, srcId, desId, string.Empty);
                        break;
                    default://10 also use default language mapping file
                        LoadConfigXml(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommonLanguageMappingFile, srcId, desId, string.Empty);
                        break;
                }
                LoadXmlMapping(path, srcId, desId, xml);
                mSrcId = srcId;
                mDesId = desId;
            }
            catch (Exception e)
            {
                mLog.Warn("LoadMapping: " + e.Message + e.StackTrace);
            }
        }

        public bool LanguageRexSame()
        {
            return (mSrcId == mDesId);
        }

        //为replicator添加，因为replicator在发送资源文件前会判断目的端是否存在，如果存在就需要目的端自身将资源文件load到job文件夹下。
        public bool LoadDestFile(uint destId)
        {
            string FilePath = ResXRootPath + ResourceFileMapping[destId].ToString();
            if (File.Exists(FilePath))
            {
                string jobFile = JobDir + "\\" + destId + "src.resx";
                if (!File.Exists(jobFile))
                {
                    File.Copy(FilePath, jobFile);
                }
                return true;
            }
            return false;
        }

        public bool isSreFileExist(uint destId)
        {
            return File.Exists(JobDir + "\\" + destId + "src.resx");
        }

        private void LoadConfigXml(string path, uint srcId, uint desId, string xml)
        {
            LoadXmlMapping(path, srcId, desId, xml);
        }

        public void LoadXmlMapping(string path, uint srcId, uint desId, string xml)
        {
            const string languageStrFormat = "/LanguageMapping/Language[@id='{0}']";
            const string languageListsStrFormat = "/LanguageMapping/Language[@id='{0}']/Lists";
            const string languagePermissionsStrFormat = "/LanguageMapping/Language[@id='{0}']/Permissions";
            const string languageContentTypeStrFormat = "/LanguageMapping/Language[@id='{0}']/ContentType";
            const string languageColumnsStrFormat = "/LanguageMapping/Language[@id='{0}']/Columns";
            const string languageNavigationStrFormat = "/LanguageMapping/Language[@id='{0}']/Navigation";
            const string languageViewsStrFormat = "/LanguageMapping/Language[@id='{0}']/Views";
            XmlDocument xDoc = new XmlDocument();
            if (!string.IsNullOrEmpty(xml))
            {
                xDoc.LoadXml(xml);
            }
            else if (File.Exists(path))
            {
                xDoc.Load(path);
            }
            else
            {
                return;
            }

            var xSouceDoc = GetSourceLangeDoc(xml, xDoc);

            XmlNode srcNode = xSouceDoc.SelectSingleNode(string.Format(languageStrFormat,srcId));
            XmlNode desNode = xDoc.SelectSingleNode(string.Format(languageStrFormat, desId));
            if (srcNode == null || desNode == null)
            {
                return;
            }

            //load list title mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languageListsStrFormat, srcId, desId, ListMapping);
            //load permission level mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languagePermissionsStrFormat, srcId, desId, PermissionMapping);
            //load contentType mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languageContentTypeStrFormat, srcId, desId, ContentTypeMapping);
            //load column name mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languageColumnsStrFormat, srcId, desId, FieldMapping);
            //load navigation mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languageNavigationStrFormat, srcId, desId, NavigationMapping);
            //load view title mapping
            GenerateLanguageMappingbyType(xSouceDoc, xDoc, languageViewsStrFormat, srcId, desId, ViewTitleMapping);

            if (mustOnlyKeyAndValue)
            {
                //change两次是为了让最终的listmapping的key和value值都是唯一的
                ExchangeKeyAndValue();
                ExchangeKeyAndValue();
            }
        }

        private XmlDocument GetSourceLangeDoc(string xml, XmlDocument xDoc)
        {
            XmlDocument xSouceDoc = new XmlDocument();
            //Migration 且没有GUI上language mapping时使用
            if (mIsMigration && string.IsNullOrEmpty(xml))
            {
                switch (sourcePlatForm)
                {
                    case AveSourceLanguagePlatForm.Sharepoint10:
                        xSouceDoc.Load(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommonLanguageMappingFile);
                        break;
                    case AveSourceLanguagePlatForm.Sharepoint13:
                        xSouceDoc.Load(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommon2013LanguageMappingFile);
                        break;
                    default:
                        xSouceDoc.Load(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommonLanguageMappingFile);
                        break;
                }
            }
            else
            {
                xSouceDoc = xDoc;
            }
            return xSouceDoc;
        }

        /// <summary>
        /// convert mapping information in xml nodes to dictionary
        /// </summary>
        /// <param name="xSouceDoc">source mapping XmlDocument</param>
        /// <param name="xDoc">dest mapping XmlDocument</param>
        /// <param name="typeStrFormat">format string for element search. Example: "/LanguageMapping/Language[@id='{0}']/Columns"</param>
        /// <param name="srcId"> source language id</param>
        /// <param name="desId">destination language id</param>
        /// <param name="mapping">mapping dictionary that stores the mapping result</param>
        private void GenerateLanguageMappingbyType(XmlDocument xSouceDoc, XmlDocument xDoc, string typeStrFormat, uint srcId, uint desId, AveVolatileCache<string, string> mapping)
        {
            XmlNode srcNode = xSouceDoc.SelectSingleNode(string.Format(typeStrFormat, srcId));
            XmlNode desNode = xDoc.SelectSingleNode(string.Format(typeStrFormat, desId));
            if (srcNode != null && desNode != null)
            {
                foreach (XmlNode node in srcNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    XmlNode temp = desNode.SelectSingleNode(".//Node[@key='" + key + "']");
                    if (temp != null)
                    {
                        mapping[srcValue] = temp.Attributes["value"].Value;
                    }
                }
            } 
        }

        public void Clear()
        {
            mExcelMultiLanguageCache.Clear();
            ListMapping.Clear();
            ViewTitleMapping.Clear();
            FieldMapping.Clear();
            PermissionMapping.Clear();
            ContentTypeMapping.Clear();
            NavigationMapping.Clear();
        }

        public void LoadSrcResX(object stateInfo)
        {
            string file = null;
            try
            {
                ResXResourceReader srcReader = null;
                if (File.Exists(JobDir + "\\" + mSrcId + "src.resx"))
                {
                    file = JobDir + "\\" + mSrcId + "src.resx";
                    srcReader = new ResXResourceReader(file);
                }
                else if (File.Exists(ResXRootPath + ResourceFileMapping[mSrcId].ToString()))
                {
                    file = ResXRootPath + ResourceFileMapping[mSrcId].ToString();
                    srcReader = new ResXResourceReader(ResXRootPath + ResourceFileMapping[mSrcId].ToString());
                }
                else
                {
                    mLog.Warn("No resource file \n" + mSrcId + "\\src.resx");
                }
                if (srcReader != null)
                {
                    foreach (DictionaryEntry de in srcReader)
                    {
                        //Links_folder--->Links 
                        if (de.Key.ToString().ToLower(CultureInfo.InvariantCulture).EndsWith("_folder", StringComparison.OrdinalIgnoreCase))
                            continue;
                        tempSrcResX[de.Key.ToString()] = de.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Load resource file {0} failed:{1}", file, ex);
            }
            finally
            {
                autoEvents[0].Set();
            }
        }

        public void LoadDesResX(object stateInfo)
        {
            string file = null;
            try
            {
                ResXResourceReader desReader = null;
                if (File.Exists(JobDir + "\\" + mDesId + "src.resx"))
                {
                    file = JobDir + "\\" + mDesId + "src.resx";
                    desReader = new ResXResourceReader(file);
                }
                else if (File.Exists(ResXRootPath + ResourceFileMapping[mDesId].ToString()))
                {
                    file = ResXRootPath + ResourceFileMapping[mDesId].ToString();
                    desReader = new ResXResourceReader(file);
                }
                else
                {
                    mLog.Warn("No resource file \n" + mDesId + "\\src.resx");
                }
                if (desReader != null)
                {
                    foreach (DictionaryEntry de in desReader)
                    {
                        //Links_folder--->Links 
                        if (de.Key.ToString().ToLower(CultureInfo.CurrentCulture).EndsWith("_folder", StringComparison.OrdinalIgnoreCase))
                            continue;
                        tempDesResX[de.Key.ToString()] = de.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Load resource file {0} failed:{1}", file, ex);
            }
            finally
            {
                autoEvents[1].Set();
            }
        }

        private void LoadListMappingFromResX()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadSrcResX));
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDesResX));

            WaitHandle.WaitAll(autoEvents);

            foreach (string key in tempSrcResX.Keys)
            {
                if (tempDesResX.Contains(key))
                {
                    ListMapping[tempSrcResX[key].ToString()] = tempDesResX[key].ToString();
                    ListMappingFromRes[tempSrcResX[key].ToString()] = tempDesResX[key].ToString();
                }
            }
            if (mustOnlyKeyAndValue)
            {
                //change两次是为了让最终的listmapping的key和value值都是唯一的
                ExchangeKeyAndValue();
                ExchangeKeyAndValue();
            }
            tempSrcResX.Clear();
            tempDesResX.Clear();

            autoEvents[0].Reset();
            autoEvents[1].Reset();
        }

        /// <summary>
        /// 将key和value值对换
        /// </summary>
        /// <param name="volatileCache"></param>
        /// <returns></returns>
        public void ExchangeKeyAndValue()
        {
            DistinctMapping(ListMapping);
            DistinctMapping(FieldMapping);
            DistinctMapping(PermissionMapping);
            DistinctMapping(ListMappingFromRes);
        }

        /// <summary>
        /// 对当前list key value对换；
        /// </summary>
        /// <param name="mapping"></param>
        private void DistinctMapping(AveVolatileCache<string, string> mapping)
        {
            using (AveVolatileCache<string, string> tmp = new AveVolatileCache<string, string>())
            {
                mapping.Keys.ToList().ForEach(key => tmp[mapping[key]] = key);
                mapping.Clear();
                tmp.Keys.ToList().ForEach(key => mapping[key] = tmp[key]);
            }
        }

        /// <summary>
        /// 主要是为了给copyObject方法处理完之后仍然引用之前对象的属性重新赋值，避免以后的修改对之前的对象造成影响；
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="tempMapping"></param>
        private void CopyMapping(ref AveVolatileCache<string, string> mapping, AveVolatileCache<string, string> tempMapping)
        {
            mapping = new AveVolatileCache<string, string>(null,StringComparer.OrdinalIgnoreCase);
            //tempMapping.Keys.ToList().ForEach(key => mapping[key] = tempMapping[key]);
            foreach (string key in tempMapping.Keys)
            {
                mapping[key] = tempMapping[key];
            }
        }

        public void CopeResXTo(string path, uint id)
        {
            lock (lockObj)
            {
                try
                {
                    string resXPath = ResXRootPath + ResourceFileMapping[id].ToString();
                    File.Copy(resXPath, path, true);
                }
                catch (Exception e)
                {
                    mLog.Warn(string.Format("Copy resource file error : \nLcid: {0}\npath:{1}\n{2}", id, path, e.ToString()));
                }
            }
        }

        public string GetTitleWithRealName(string name, uint id)
        {
            try
            {
                //zj offline excel效率问题，foreach时间太久，所以缓存起来
                if (mExcelMultiLanguageCache.ContainsKey(ResXRootPath + ResourceFileMapping[id].ToString() + name))
                {
                    return mExcelMultiLanguageCache[ResXRootPath + ResourceFileMapping[id].ToString() + name];
                }
                ResXResourceReader desReader = new ResXResourceReader(ResXRootPath + ResourceFileMapping[id].ToString());
                foreach (DictionaryEntry de in desReader)
                {
                    if (de.Key.ToString() == name)
                    {
                        mExcelMultiLanguageCache[ResXRootPath + ResourceFileMapping[id].ToString() + name] = de.Value.ToString();
                        return de.Value.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("GetTitleWithRealName Error: " + e.Message + e.StackTrace);
            }
            return string.Empty;
        }

        public AveLanguageProcesser Clone()
        {
            AveLanguageProcesser dest = new AveLanguageProcesser();
            AveObjectCopy.CopyObject(dest, mLanguageProcessor);
            CopyMapping(ref dest.ListMapping, mLanguageProcessor.ListMapping);
            CopyMapping(ref dest.FieldMapping, mLanguageProcessor.FieldMapping);
            CopyMapping(ref dest.PermissionMapping, mLanguageProcessor.PermissionMapping);
            CopyMapping(ref dest.ListMappingFromRes, mLanguageProcessor.ListMappingFromRes);
            return dest;
        }

        public void DeleteLanguageFile()
        {
            try
            {
                if (mLanguageFilePath != null)
                {
                    foreach (string filePath in mLanguageFilePath)
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while delete Language File.{0}", e.ToString());
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (ListMapping != null)
            {
                ListMapping.Dispose();
                ListMapping = null;
            }
            if (FieldMapping != null)
            {
                FieldMapping.Dispose();
                FieldMapping = null;
            }
            if (PermissionMapping != null)
            {
                PermissionMapping.Dispose();
                PermissionMapping = null;
            }
            if (ContentTypeMapping != null)
            {
                ContentTypeMapping.Dispose();
                ContentTypeMapping = null;
            }
            if (ListMappingFromRes != null)
            {
                ListMappingFromRes.Dispose();
                ListMappingFromRes = null;
            }
            if (ResourceFileMapping != null)
            {
                ResourceFileMapping.Dispose();
                ResourceFileMapping = null;
            }
            if (mLanguageFilePath != null)
            {
                mLanguageFilePath.Clear();
                mLanguageFilePath = null;
            }
            if (autoEvents != null)
            {
                foreach (var e in autoEvents)
                {
                    e.Close();
                }
                autoEvents = null;
            }
        }
        #endregion
    }
}
