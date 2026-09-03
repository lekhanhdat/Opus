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




using AvePoint.Common;
using AvePoint.GCommon;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public enum AveLanguageMappingType
    {
        ListMapping,
        FieldMapping,
        PermissionMapping,
        ContentTypeMapping,
        NavigationMapping,
        ViewMapping
    }

    public class AveLanguageProcesser : IDisposable
    {
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static AveLanguageProcesser mLanguageProcessor;
        private readonly static object mLock = new object();
        private uint mSrcId;
        private uint mDesId;
        private AutoResetEvent[] autoEvents;
        private readonly object lockObj = new object();
        private Hashtable tempSrcResX = new Hashtable();
        private Hashtable tempDesResX = new Hashtable();
        private string mRootDir;
        private string mJobDir;

        public AveVolatileCache<string, string> ListMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ViewMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ListMappingFromRes = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> FieldMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> PermissionMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> ContentTypeMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<string, string> NavigationMapping = new AveVolatileCache<string, string>(null, StringComparer.OrdinalIgnoreCase);
        public AveVolatileCache<uint, string> ResourceFileMapping = new AveVolatileCache<uint, string>();
        public string ResXRootPath = string.Empty;
        public static Dictionary<uint, string> CultureIdNameMapping = new Dictionary<uint, string>();
        private bool mustOnlyKeyAndValue = false;
        public static AveObjectModelFactory Factory { get; set; }
        /// <summary>
        /// offline excel 功能需要缓存已经找到name，否则效率太慢了
        /// </summary>
        private Dictionary<string, string> mExcelMultiLanguageCache = new Dictionary<string, string>();

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

        public static void ResetLanguageInstance()
        {
            if (mLanguageProcessor != null)
            {
                lock (mLock)
                {
                    if (mLanguageProcessor != null)
                    {
                        mLanguageProcessor = null;
                    }
                }
            }
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

            //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SPTimerV4");

            //if (key != null)
            //{
            //    ResXRootPath = key.GetValue("ImagePath").ToString();
            //    ResXRootPath = ResXRootPath.Substring(1, ResXRootPath.IndexOf("BIN\\OWSTIMER.EXE", StringComparison.OrdinalIgnoreCase) - 1) + "Resources\\";
            //}

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

            //RegistryKey key = Factory.CreateAveRegister().OpenKeyForLanguageProcesser();

            //if (key != null)
            //{
            //    ResXRootPath = key.GetValue("ImagePath").ToString();
            //    ResXRootPath = ResXRootPath.Substring(1, ResXRootPath.IndexOf("BIN\\OWSTIMER.EXE", StringComparison.OrdinalIgnoreCase) - 1) + "Resources\\";
            //}

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
            mLog.Info("[SAAS-30604]Begin to load language mapping.CurrentExistMapping:[{0}->{1}],NewMappingForCurrentWeb:[{2}->{3}]", mSrcId,mDesId,srcId, desId);
            if (mSrcId == 0 && mDesId == 0 && srcId == desId)
            {
                return;
            }
            if (mSrcId == srcId && mDesId == desId)
            {
                return;
            }
            this.mustOnlyKeyAndValue = mustOnlyKeyAndValue;
            
            mSrcId = srcId;
            mDesId = desId;

            try
            {
                Clear();
                LoadListMappingFromResX();
                LoadConfigXml(mRootDir + AgentConstants.AgentConfigurationFileName.AgentCommonLanguageMappingFile, srcId, desId, string.Empty);
                LoadXmlMapping(path, srcId, desId, xml);
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
            XmlNode srcNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']");
            XmlNode desNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']");
            if (srcNode == null || desNode == null)
            {
                return;
            }

            XmlNode srcListNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/Lists");
            XmlNode desListNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Lists");
            if (srcListNode != null && desListNode != null)
            {
                foreach (XmlNode node in srcListNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    //XmlNode temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Lists/Node[@key='" + key + "']");
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Lists/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Lists/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        ListMapping[srcValue] = desValue;
                    }
                }
            }

            XmlNode srcViewNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/Views");
            XmlNode desViewNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Views");
            if (srcViewNode != null && desViewNode != null)
            {
                foreach (XmlNode node in srcViewNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    //XmlNode temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Lists/Node[@key='" + key + "']");
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Views/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Views/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        ViewMapping[srcValue] = desValue;
                    }
                }
            }

            XmlNode srcPermissionNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/Permissions");
            XmlNode desPermissionNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Permissions");
            if (srcPermissionNode != null && desPermissionNode != null)
            {
                foreach (XmlNode node in srcPermissionNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    //XmlNode temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Permissions/Node[@key='" + key + "']");
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Permissions/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Permissions/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        PermissionMapping[srcValue] = desValue;
                    }

                }
            }

            XmlNode srcContentTypeNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/ContentType");
            XmlNode desContentTypeNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/ContentType");
            if (srcContentTypeNode != null && desContentTypeNode != null)
            {
                foreach (XmlNode node in srcContentTypeNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    //XmlNode temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/ContentType/Node[@key='" + key + "']");
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/ContentType/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/ContentType/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        ContentTypeMapping[srcValue] = desValue;
                    }

                }
            }

            XmlNode srcFieldNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/Columns");
            XmlNode desFieldNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Columns");
            if (srcFieldNode != null && desFieldNode != null)
            {
                foreach (XmlNode node in srcFieldNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Columns/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Columns/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        FieldMapping[srcValue] = desValue;
                    }
                }
            }
            XmlNode srcNavigationNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + srcId.ToString() + "']/Navigations");
            XmlNode desNavigationNode = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Navigations");
            if (srcNavigationNode != null && desNavigationNode != null)
            {
                foreach (XmlNode node in srcNavigationNode.ChildNodes)
                {
                    string key = node.Attributes["key"].Value;
                    string srcValue = node.Attributes["value"].Value;
                    //XmlNode temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Navigations/Node[@key='" + key + "']");
                    XmlNode temp = null;
                    if (key.Contains("'"))
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Navigations/Node[@key=\"" + key + "\"]");
                    }
                    else
                    {
                        temp = xDoc.SelectSingleNode("/LanguageMapping/Language[@id='" + desId.ToString() + "']/Navigations/Node[@key='" + key + "']");
                    }
                    if (temp != null)
                    {
                        string desValue = temp.Attributes["value"].Value;
                        NavigationMapping[srcValue] = desValue;
                    }
                }
            }
            if (mustOnlyKeyAndValue)
            {
                //change两次是为了让最终的listmapping的key和value值都是唯一的
                ExchangeKeyAndValue();
                ExchangeKeyAndValue();
            }
        }

        private void Clear()
        {
            mExcelMultiLanguageCache.Clear();
            ListMapping.Clear();
            NavigationMapping.Clear();
            ViewMapping.Clear();
            ListMappingFromRes.Clear();
            FieldMapping.Clear();
            PermissionMapping.Clear();
            ContentTypeMapping.Clear();
            NavigationMapping.Clear();
        }

        public void LoadSrcResX(object stateInfo)
        {
            throw new NotImplementedException("LoadSrcResX");
            //try
            //{
            //    ResXResourceReader srcReader = null;
            //    if (File.Exists(JobDir + "\\" + mSrcId + "src.resx"))
            //    {
            //        srcReader = new ResXResourceReader(JobDir + "\\" + mSrcId + "src.resx");
            //    }
            //    else if (ResourceFileMapping.ContainsKey(mSrcId) && File.Exists(ResXRootPath + ResourceFileMapping[mSrcId].ToString()))
            //    {
            //        srcReader = new ResXResourceReader(ResXRootPath + ResourceFileMapping[mSrcId].ToString());
            //    }
            //    else
            //    {
            //        mLog.Warn("No resource file \n" + mSrcId + "\\src.resx");
            //    }
            //    if (srcReader != null)
            //    {
            //        try
            //        {
            //            foreach (DictionaryEntry de in srcReader)
            //            {
            //                //Links_folder--->Links 
            //                if (de.Key.ToString().ToLower().EndsWith("_folder", StringComparison.OrdinalIgnoreCase))
            //                    continue;
            //                tempSrcResX[de.Key.ToString()] = de.Value;
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            mLog.Info("Failed to load source resource file, msg : {0}", e.Message);
            //        }
            //    }
            //    autoEvents[0].Set();
            //}
            //catch (Exception ex)
            //{
            //    mLog.Info("Failed to load source resource file, message : {0}", ex.Message);
            //}
        }

        public void LoadDesResX(object stateInfo)
        {
            throw new NotImplementedException("LoadDesResX");
            //try
            //{
            //    ResXResourceReader desReader = null;
            //    if (File.Exists(JobDir + "\\" + mDesId + "src.resx"))
            //    {
            //        desReader = new ResXResourceReader(JobDir + "\\" + mDesId + "src.resx");
            //    }
            //    else if (ResourceFileMapping.ContainsKey(mDesId) && File.Exists(ResXRootPath + ResourceFileMapping[mDesId].ToString()))
            //    {
            //        desReader = new ResXResourceReader(ResXRootPath + ResourceFileMapping[mDesId].ToString());
            //    }
            //    else
            //    {
            //        mLog.Warn("No resource file \n" + mDesId + "\\src.resx");
            //    }
            //    if (desReader != null)
            //    {
            //        try
            //        {
            //            foreach (DictionaryEntry de in desReader)
            //            {
            //                //Links_folder--->Links 
            //                if (de.Key.ToString().ToLower().EndsWith("_folder", StringComparison.OrdinalIgnoreCase))
            //                    continue;
            //                tempDesResX[de.Key.ToString()] = de.Value;
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            mLog.Info("Failed to load dest resource file, msg : {0}", e.Message);
            //        }
            //    }
            //    autoEvents[1].Set();
            //}
            //catch (Exception ex)
            //{
            //    mLog.Info("Failed to load dest resource file, message : {0}", ex.Message);
            //}
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
            DistinctMapping(ContentTypeMapping);
            DistinctMapping(ViewMapping);
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
        private AveVolatileCache<string, string> CopyMapping(AveVolatileCache<string, string> tempMapping)
        {
            AveVolatileCache<string, string> mapping = new AveVolatileCache<string, string>();
            tempMapping.Keys.ToList().ForEach(key => mapping[key] = tempMapping[key]);
            return mapping;
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
            throw new NotImplementedException("GetTitleWithRealName");
            //try
            //{
            //    string tempResourceFile;
            //    if (!ResourceFileMapping.TryGetValue(id, out tempResourceFile))
            //    {
            //        tempResourceFile = string.Empty;
            //    }
            //    //zj offline excel效率问题，foreach时间太久，所以缓存起来
            //    if (mExcelMultiLanguageCache.ContainsKey(ResXRootPath + tempResourceFile + name))
            //    {
            //        return mExcelMultiLanguageCache[ResXRootPath + tempResourceFile + name];
            //    }
            //    ResXResourceReader desReader = new ResXResourceReader(ResXRootPath + tempResourceFile.ToString());
            //    foreach (DictionaryEntry de in desReader)
            //    {
            //        if (de.Key.ToString() == name)
            //        {
            //            mExcelMultiLanguageCache[ResXRootPath + tempResourceFile.ToString() + name] = de.Value.ToString();
            //            return de.Value.ToString();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    mLog.Warn("GetTitleWithRealName Error: " + e.Message + e.StackTrace);
            //}
            return string.Empty;
        }

        public AveLanguageProcesser Clone()
        {
            AveLanguageProcesser dest = new AveLanguageProcesser();
            AveObjectCopy.CopyObject(dest, mLanguageProcessor);
            dest.ListMapping = CopyMapping(mLanguageProcessor.ListMapping);
            dest.ViewMapping = CopyMapping(mLanguageProcessor.ViewMapping);
            dest.FieldMapping = CopyMapping(mLanguageProcessor.FieldMapping);
            dest.PermissionMapping = CopyMapping(mLanguageProcessor.PermissionMapping);
            dest.ListMappingFromRes = CopyMapping(mLanguageProcessor.ListMappingFromRes);
            dest.ContentTypeMapping = CopyMapping(mLanguageProcessor.ContentTypeMapping);
            return dest;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (ListMapping != null)
            {
                ListMapping.Dispose();
                ListMapping = null;
            }
            if (ViewMapping != null)
            {
                ViewMapping.Dispose();
                ViewMapping = null;
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
