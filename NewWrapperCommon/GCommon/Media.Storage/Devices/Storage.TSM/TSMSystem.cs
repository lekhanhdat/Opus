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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Resources.TSMI18N;
    using AvePoint.Media.Storage.Util;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/3/28",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_7 },
    null,
    true)]
    #endregion
    class TSMSystem : AbstractXSystem
    {
        public static Dictionary<String, TSMSystem> mapping = new Dictionary<String, TSMSystem>();
        static Dictionary<String, Int32> ThreadInfo = new Dictionary<String, Int32>();
        public static object countLocker = new object();
        public static Mutex mutex = new Mutex();
        static object checkLocker = new object();
        static object sessionLocker = new object();
        TSMClient client;
        TSMNodeInfo nodeInfo;
        TSMSession systemSession;
        TSMSession checkSession;
        Boolean isValidate;
        List<String> highNameList = new List<String>();
        StorageLogger logger = new StorageLogger(typeof(TSMSystem));
        readonly String selectAll = "\\*";               //在所有查询所有的数据时使用该字符串
        Boolean isSingleSession;
        static readonly Dictionary<String, BrowserLocker> listLockers = new Dictionary<String, BrowserLocker>();

        class BrowserLocker
        {
            public Boolean Listing { get; set; }
        }

        public String MapKey { get; set; }
        /// <summary>
        /// TSM system Constructor. 
        /// </summary>
        /// <param name="xriString">The connecting string.</param>
        /// <param name="parentSystem">The parent system.</param>
        public TSMSystem(String xriString, AbstractXSystem parentSystem)
            : base(xriString, parentSystem)
        {
            this.SupportedFileType = FileBlockType.SingleInstanceLevel_Block;
            this.client = new TSMClient();
            this.SystemHealth = XSystemHealth.Unknown;
            this.Open();
        }

        private static String GetMapKey(String xriString)
        {
            var map = new TSMAddressMap();
            var stringArray = xriString.Split('&');
            foreach (var subString in stringArray)
            {
                if (subString.StartsWith("address", StringComparison.OrdinalIgnoreCase))
                {
                    map.address = subString.Replace("address=", "");
                }
                if (subString.StartsWith("port", StringComparison.OrdinalIgnoreCase))
                {
                    map.port = subString.Replace("port=", "");
                }
                if (subString.StartsWith("node", StringComparison.OrdinalIgnoreCase))
                {
                    map.nodeName = subString.Replace("node=", "");
                }
            }
            return map.ToString();
        }

        public static TSMSystem GetInstance(String xriString, AbstractXSystem parentSystem)
        {
            if (XRI.ValueDecode(xriString).ToLower(CultureInfo.InvariantCulture).Contains(XRIParameterKeys.SINGLESESSIONTRUE) && Thread.CurrentThread.Name != null)
            {
                lock (countLocker)
                {
                    var currentThreadName = Thread.CurrentThread.Name;
                    if (ThreadInfo.ContainsKey(currentThreadName))
                    {
                        ThreadInfo[currentThreadName]++;
                    }
                    else
                    {
                        ThreadInfo.Add(currentThreadName, 1);
                    }
                    var mapKey = GetMapKey(xriString);
                    if (mapping.ContainsKey(mapKey))
                    {
                        if (TSMSystemSafeguard.IsAlive())
                        {
                            TSMSystemSafeguard.RemoveTSMSystem(mapping[mapKey]);
                        }
                        return mapping[mapKey];
                    }
                    else
                    {
                        var system = new TSMSystem(xriString, parentSystem);
                        mapping.Add(mapKey, system);
                        if (TSMSystemSafeguard.IsAlive())
                        {
                            TSMSystemSafeguard.RemoveTSMSystem(system);
                        }
                        return mapping[mapKey];
                    }
                }
            }
            else
            {
                return new TSMSystem(xriString, parentSystem);
            }
        }

        public override Boolean IsSupportAutoCheck
        {
            get
            {
                return false;
            }
        }

        private TSMSession GetSystemSession()
        {
            if (this.systemSession == null && this.nodeInfo != null)
            {
                lock (sessionLocker)
                {
                    if (this.systemSession == null)
                    {
                        this.systemSession = this.client.OpenSession(this.nodeInfo);
                    }
                }
            }
            return this.systemSession;
        }

        /// <summary>
        /// Open a TSM device.
        /// </summary>
        /// <returns>The validate result</returns>
        public override StorageOpenValidResult Open()
        {
            try
            {
                if (this.SystemHealth != XSystemHealth.Unknown)
                {
                    return new StorageOpenValidResult();
                }
                base.Open();
                var parms = this.XriObject.Params;
                this.MapKey = GetMapKey(this.XriObject.ToString());
                this.SystemLocation = "DocAve";
                this.nodeInfo = new TSMNodeInfo();
                this.nodeInfo.TcpServerAddress = this.XriObject[XRIParameterKeys.DSM_SERVER_ADDRESS];
                if (parms.ContainsKey(XRIParameterKeys.FILESPACE_KEY))
                {
                    this.nodeInfo.Filespace = parms[XRIParameterKeys.FILESPACE_KEY];
                }
                else
                {
                    this.nodeInfo.Filespace = "DocAve";
                }
                if (parms.ContainsKey(XRIParameterKeys.LANFREETCPSERVERADDRESS))
                {
                    this.nodeInfo.LanfreeTcpServerAddress = parms[XRIParameterKeys.LANFREETCPSERVERADDRESS];
                }
                if (parms.ContainsKey(XRIParameterKeys.LANFREETCPPORT))
                {
                    this.nodeInfo.Lanfreetcpport = parms[XRIParameterKeys.LANFREETCPPORT];
                }
                if (parms.ContainsKey(XRIParameterKeys.ENABLELANFREE))
                {
                    this.nodeInfo.EnableLanfree = Boolean.Parse(parms[XRIParameterKeys.ENABLELANFREE]);
                }

                if (parms.ContainsKey(XRIParameterKeys.LANFREECOMMENTTHOD))
                {
                    this.nodeInfo.LanfreeCommmethod = parms[XRIParameterKeys.LANFREECOMMENTTHOD];
                }
                if (parms.ContainsKey(XRIParameterKeys.SINGLESESSION_KEY))
                {
                    this.isSingleSession = Boolean.Parse(this.XriObject.Params[XRIParameterKeys.SINGLESESSION_KEY]);
                    this.nodeInfo.IsSingleSession = this.isSingleSession;
                }
                if (parms.ContainsKey(XRIParameterKeys.SYSTEM_ID_KEY))
                {
                    this.nodeInfo.PdID = parms[XRIParameterKeys.SYSTEM_ID_KEY];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_NODE_NAME))
                {
                    this.nodeInfo.Nodename = parms[XRIParameterKeys.DSM_NODE_NAME];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_NODE_PWD))
                {
                    this.nodeInfo.Password = SecretUtil.DescryptPassword(parms[XRIParameterKeys.DSM_NODE_PWD]);
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_COMMMETHOD))
                {
                    this.nodeInfo.CommunicationMethod = parms[XRIParameterKeys.DSM_COMMMETHOD];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_MC))
                {
                    this.nodeInfo.IncludeMC = parms[XRIParameterKeys.DSM_MC];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_PORT))
                {
                    this.nodeInfo.Port = parms[XRIParameterKeys.DSM_PORT];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_ENABLE_NODE_PROXY))
                {
                    this.nodeInfo.EnableNodeProxy = Convert.ToBoolean(parms[XRIParameterKeys.DSM_ENABLE_NODE_PROXY]);
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_Asnodename))
                {
                    this.nodeInfo.Asnodename = parms[XRIParameterKeys.DSM_Asnodename];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_MODIFY_TIME_KEY))
                {
                    this.nodeInfo.ModifyTime = parms[XRIParameterKeys.DSM_MODIFY_TIME_KEY];
                }
                if (parms.ContainsKey(XRIParameterKeys.DSM_VALIDATE_KEY))
                {
                    this.isValidate = Boolean.Parse(parms[XRIParameterKeys.DSM_VALIDATE_KEY]);
                    this.nodeInfo.IsValidate = this.isValidate;
                }

                if (this.nodeInfo.EnableNodeProxy && String.IsNullOrEmpty(this.nodeInfo.Asnodename))
                {
                    throw new ArgumentNullException("Setup client node proxy, but the vaule of the 'Asnodename' parameter is null.");
                }
                var root = default(String);
                if (System.Web.HttpContext.Current == null)
                {
                    root = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    root = AppDomain.CurrentDomain.BaseDirectory + @"bin\";
                }
                root = PathUtil.CombinePath(root, TSMConst.tsmResourceRoot);
                var direvtoryInfo = new DirectoryInfo(root);//why ? 
                root = direvtoryInfo.FullName;
                this.nodeInfo.CommDsmiDir = root + @"\api";
                this.nodeInfo.CommDsmiLogName = "AvePoint-TSM-Debug.log";
                if (!String.IsNullOrEmpty(XFactory.CacheLocation) && this.isValidate)
                {
                    var tempPath = Path.Combine(new DirectoryInfo(XFactory.CacheLocation).FullName, "tsm");
                    this.nodeInfo.CommConfigFileDir = tempPath + @"\opts";
                    this.nodeInfo.CommConfigFile = this.nodeInfo.CommConfigFileDir + @"\dsm.opt";
                    this.nodeInfo.CommDsmiLogDir = tempPath + @"\logs";
                }
                else
                {
                    this.nodeInfo.CommConfigFileDir = root + @"\opts";
                    this.nodeInfo.CommConfigFile = this.nodeInfo.CommConfigFileDir + @"\dsm.opt";
                    this.nodeInfo.CommDsmiLogDir = root + @"\logs";
                }
                if (String.IsNullOrEmpty(this.nodeInfo.PdID))
                {
                    this.isValidate = true;
                }
                if (this.isValidate)
                {
                    this.nodeInfo.ConfigFileDir = this.nodeInfo.CommConfigFileDir + @"\" + Guid.NewGuid().ToString();
                }
                else
                {
                    this.nodeInfo.ConfigFileDir = this.nodeInfo.CommConfigFileDir + @"\" + this.nodeInfo.PdID + this.nodeInfo.ModifyTime;
                }
                this.nodeInfo.ConfigFile = this.nodeInfo.ConfigFileDir + @"\dsm.opt";
                this.nodeInfo.Capacity = 1 * 1024 * 1024 * 1024L;
                this.nodeInfo.Occupancy = 0;
                this.nodeInfo.SizeEstimate = 50 * 1024 * 1024;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                this.Type = "TSMSystem";
                this.IsSimulReadWriteSystem = true;
                this.client.SetUp(nodeInfo);
                logger.Info("open a tsm system, host:{0}, port:{1}, node:{2}, file space:{3}, single session {4}.", this.nodeInfo.TcpServerAddress, this.nodeInfo.Port, this.nodeInfo.Nodename, this.nodeInfo.Filespace, this.isSingleSession);
            }
            catch (Exception ex)
            {
                logger.Error("open tsm system failed:server:{0}, node:{1}, details : {2}.", this.nodeInfo.TcpServerAddress, this.nodeInfo.Nodename, ex);
                this.SystemHealth = XSystemHealth.Unaccessable;
                throw;
            }
            SetSystemDescription();
            return new StorageOpenValidResult();
        }

        /// <summary>
        /// Set description for TSM system.
        /// </summary>
        protected override void SetSystemDescription()
        {
            this.Properties[SystemPropertyKeys.SystemDescriptionKey] = "TSM, Server Address: " + this.nodeInfo.TcpServerAddress + ", Node Name: " + this.nodeInfo.Nodename;
            var keys = new List<String>();
            keys.Add(this.nodeInfo.Nodename);
            keys.Add(this.nodeInfo.Port);
            keys.Add(this.nodeInfo.TcpServerAddress);
            var securityKeys = new List<String>();
            securityKeys.Add(this.nodeInfo.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        /// <summary>
        /// Validate TSM device.
        /// </summary>
        /// <returns>The Validate result</returns>
        public override StorageOpenValidResult Validate()
        {
            CheckState();
            if (this.IsForcePassValidation)
            {
                return base.Validate();
            }
            var result = new StorageOpenValidResult();
            var testSession = default(TSMSession);
            try
            {
                result.IsReadAble = true;
                result.IsWriteAble = true;
                result.IsDeleteAble = true;
                result.TotalSpace = Int64.MaxValue;
                result.TotalFreeSpace = Int64.MaxValue;
                result.TotalUsedSpace = 0;
                this.SystemHealth = XSystemHealth.AvailableAndNotFull;
                result.SystemHealth = XSystemHealth.AvailableAndNotFull;

                if (this.isValidate)
                {
                    testSession = this.client.OpenSession(this.nodeInfo);
                    var createFileResult = this.client.CreateValidateFile(testSession);
                    if (!createFileResult)
                    {
                        result.IsReadAble = false;
                        result.IsWriteAble = false;
                        result.IsDeleteAble = false;
                        result.TotalSpace = 0;
                        result.TotalFreeSpace = 0;
                        result.TotalUsedSpace = 0;
                        this.SystemHealth = XSystemHealth.ConnectedFailed;
                        result.SystemHealth = XSystemHealth.ConnectedFailed;
                        result.Message = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Authentication_failed", AbstractXSystem.Culture);
                    }
                }
            }
            catch (Exception e)
            {
                var errorMessage = String.Empty;
                if (e.Message.Contains("ANS1017E(RC-50) Session rejected: TCP/IP connection failure"))
                {
                    //IP or Port 输入错误
                    errorMessage = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Cannot_connect_to_the_remote_server", AbstractXSystem.Culture);
                }
                if (e.Message.Contains("ANS1025E(RC137) Session rejected: Authentication failure"))
                {
                    //Node 密码输入错误
                    errorMessage = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Authentication_failed", AbstractXSystem.Culture);
                }
                if (e.Message.Contains("ANS1353E(RC53) Session rejected: Unknown or incorrect ID entered"))
                {
                    //Node 名称输入错误
                    errorMessage = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_The_entered_node_name_is_incorrect", AbstractXSystem.Culture);
                }
                if (e.Message.Contains("Can not find users management class"))
                {
                    //Management class 输入错误...Can not find users management class:MGMTName, all defined management class: MGMTName,MGMTName
                    errorMessage = String.Format(TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Cannot_find_the_entered_management_class", AbstractXSystem.Culture), this.nodeInfo.IncludeMC);
                }
                if (errorMessage.Equals(String.Empty))
                {
                    errorMessage = TSMI18N.ResourceManager.GetString("MediaStorage_TSM_Test_failed", AbstractXSystem.Culture);
                }
                var verifyFailedEventMessage = new EventIds.Storage.VerifyFailedEventMessage(this.SystemLocation, ContextValues.Storage.StorageType.TSM, e);
                logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.TSM, verifyFailedEventMessage);
                logger.Error("Validate Physical Device Error : {0}", e);
                this.SystemHealth = XSystemHealth.Unknown;
                result.Message = errorMessage == String.Empty ? e.Message : errorMessage;
                result.SystemHealth = XSystemHealth.ConnectedFailed;
            }
            finally
            {
                try
                {
                    if (testSession != null)
                    {
                        this.client.CloseSession(testSession);
                    }
                    if (this.isValidate)
                    {
                        CleanUpValidateData();
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.Message);
                }
            }
            this.SystemHealth = result.SystemHealth;
            return result;
        }

        /// <summary>
        /// Clean up data for validate.
        /// </summary>
        private void CleanUpValidateData()
        {
            try
            {
                this.client.CleanUpValidateData(this.nodeInfo);
                logger.Debug("Delete temp folder succeed:" + this.nodeInfo.ConfigFileDir);
            }
            catch (Exception ex)
            {
                logger.Warn("Clean up validate data failed:" + ex.Message);
            }
        }

        /// <summary>
        /// Open read or write stream
        /// </summary>
        /// <param name="info">The information of device</param>
        /// <param name="fileMode">Open mode</param>
        /// <returns>The stream</returns>
        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(info);
                var stream = default(TSMStream);
                this.Written = fileMode != FileMode.Open;
                if (this.isSingleSession)
                {
                    stream = new TSMStream(this.client, GetSystemSession(), storageInfo, this.nodeInfo, fileMode, this, mutex);
                }
                else
                {
                    stream = new TSMStream(this.client, GetSystemSession(), storageInfo, this.nodeInfo, fileMode, this, null);
                }
                stream.InitStream();
                return stream;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        private TSMSession GetCheckSession()
        {
            if (this.checkSession == null && this.nodeInfo != null)
            {
                lock (checkLocker)
                {
                    if (this.checkSession == null)
                    {
                        //client.CreateConfigFile(nodeInfo); //TODO
                        this.checkSession = this.client.OpenSession(this.nodeInfo);
                    }
                }
            }
            return this.checkSession;
        }

        /// <summary>
        /// Check that file exists or not
        /// </summary>
        /// <param name="info">Which File  You Want to Cheched</param>
        /// <returns>Exist or not</returns>
        public override Boolean FileExists(StorageInfo info)
        {
            var result = false;
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(info);
                result = this.client.CheckObject(GetCheckSession(), storageInfo.HighName, storageInfo.LowName, DSMObjType.DSM_FILE);
            }
            catch (Exception e)
            {
                logger.Error("Error when check object container name : {0}, object name : {1}. Details : {2}", info.HighName, info.LowName, e);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Check that directory exists or not
        /// </summary>
        /// <param name="info">Which directory  You Want to Cheched</param>
        /// <returns>Exist or not</returns>
        public override Boolean DirectoryExists(StorageInfo info)
        {
            var result = false;
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(info);
                var highName = storageInfo.HighPlusLowName + "*";
                result = this.client.CheckObject(GetCheckSession(), highName, this.selectAll, DSMObjType.DSM_FILE);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Open file
        /// </summary>
        /// <param name="fileInfo">Which file you want to open</param>
        /// <returns>The file infomation</returns>
        public override XFileInfo OpenFile(StorageInfo fileInfo)
        {
            var result = default(XFileInfo);
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(fileInfo);
                var length = this.client.GetLength(GetCheckSession(), storageInfo.HighName, storageInfo.LowName);
                result = new TSMFileInfo(fileInfo.HighName, fileInfo.LowName, length);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            return result;
        }

        /// <summary>
        /// Close all sessions
        /// </summary>
        public override void Close()
        {
            if (this.isSingleSession && !this.isValidate && Thread.CurrentThread.Name != null)
            {
                lock (countLocker)
                {
                    var currentThreadName = Thread.CurrentThread.Name;
                    if (ThreadInfo.ContainsKey(currentThreadName))
                    {
                        ThreadInfo[currentThreadName]--;
                    }
                    var canClose = true;
                    foreach (KeyValuePair<String, Int32> threadinfo in ThreadInfo)
                    {
                        if (threadinfo.Value > 0)
                        {
                            canClose = false;
                            break;
                        }
                    }
                    if (canClose)
                    {
                        logger.Info("close the system and the thread name is {0}", Thread.CurrentThread.Name);
                        if (TSMSystemSafeguard.IsAlive())
                        {
                            TSMSystemSafeguard.AddTSMSystem(this);
                        }
                        else
                        {
                            TSMSystemSafeguard.StartTSMSystemSafeguard();
                            TSMSystemSafeguard.AddTSMSystem(this);
                        }
                    }
                }
            }
            else
            {
                KilledAllSession();
            }
        }

        public void KilledAllSession()
        {
            try
            {
                if (this.systemSession != null)
                {
                    this.client.CloseSession(this.systemSession);
                    this.systemSession = null;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            try
            {
                if (this.checkSession != null)
                {
                    this.client.CloseSession(this.checkSession);
                    this.checkSession = null;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            if (this.highNameList != null && this.highNameList.Count > 0)
            {
                this.highNameList.Clear();
            }
            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }
            logger.Debug("Close tsm sessions");
        }

        /// <summary>
        /// Deleted file
        /// </summary>
        /// <param name="info"></param>
        /// <returns>The result of Deleted</returns>
        public override StorageDeleteResult DeleteFile(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(info);
                result.DeletedFileSize = this.client.DeleteObject(GetCheckSession(), storageInfo.HighName, storageInfo.LowName, DSMObjType.DSM_FILE);
                while (FileExists(info))
                {
                    logger.Debug("File {0} has another version delete it again", info.HighPlusLowName);
                    this.client.DeleteObject(GetCheckSession(), storageInfo.HighName, storageInfo.LowName, DSMObjType.DSM_FILE);
                }
                result.IsDeleted = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            this.Deletion = true;
            return result;
        }

        /// <summary>
        /// Delete All files in this Directory
        /// </summary>
        /// <param name="info">The Storage Infomation</param>
        /// <returns>The deleted result</returns>
        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            var result = new StorageDeleteResult();
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(info);
                logger.Info("Start delete directory {0}", storageInfo.HighPlusLowName);
                //检查以lowName为highName的文件中是否存在数据,存在则删除一条数据
                while (this.client.CheckObject(GetCheckSession(), storageInfo.HighPlusLowName, this.selectAll, DSMObjType.DSM_ANY))
                {
                    var directorySize = this.client.DeleteObject(GetCheckSession(), storageInfo.HighPlusLowName + "*", this.selectAll, DSMObjType.DSM_FILE);
                    logger.Info("Delete directory {0}, the size is {1}", storageInfo.HighPlusLowName, directorySize);
                    result.DeletedFileSize += directorySize;
                }
                //检查是否成功删除文件夹中的所有数据,没有全部删除则list出全部的highname文件夹，然后遍历删除
                int size = this.client.GetObjectNameSizeWithDate(GetCheckSession(), storageInfo.HighPlusLowName + "*", this.selectAll);
                StringBuilder sbNames = new StringBuilder(size);
                var nameList = this.client.GetObjectNames(GetCheckSession(), sbNames, size);
                logger.Info("GetObjectNameSizeWithDate Size = {0}, namelist = {1}", size, nameList);
                foreach (string name in nameList)
                {
                    while (this.client.CheckObject(GetCheckSession(), name, this.selectAll, DSMObjType.DSM_ANY))
                    {
                        logger.Info("Try to delete subfolder {0}", name);
                        var subFolderSize = this.client.DeleteObject(GetCheckSession(), name + "*", this.selectAll, DSMObjType.DSM_FILE);
                        logger.Info("Delete subfolder {0} sucessfully, size is {1}", name, subFolderSize);
                        result.DeletedFileSize += subFolderSize;
                    }
                    if (this.client.CheckObject(GetCheckSession(), name, this.selectAll, DSMObjType.DSM_ANY))
                    {
                        logger.Info("The folder {0} still exist.", name);
                        var lowNameSize = this.client.GetObjectNameSize(this.GetCheckSession(), name, this.selectAll);
                        StringBuilder sbLowNames = new StringBuilder(lowNameSize);
                        var lowNameList = this.client.GetObjectNames(GetCheckSession(), sbLowNames, lowNameSize);
                        logger.Info("There are {0} files under folder {1}", lowNameList.Length, name);
                        foreach (var lowName in lowNameList)
                        {
                            if (string.IsNullOrEmpty(lowName))
                            {
                                continue;
                            }
                            var fileInfo = new StorageInfo(name, lowName);
                            var queryFileInfo = this.OpenFile(fileInfo);
                            var querySize = queryFileInfo == null ? 0 : queryFileInfo.FileSize;
                            var deletFileresult = this.DeleteFile(fileInfo);
                            logger.Info("Try to delete file {0}, the size from QUERY method is {1}, the size from DELETE method is {2}", fileInfo.HighPlusLowName, querySize, deletFileresult.DeletedFileSize);
                            result.DeletedFileSize += deletFileresult.DeletedFileSize;
                        }
                    }
                }
                result.IsDeleted = true;
                logger.Info("The total size is {0}", result.DeletedFileSize);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            this.Deletion = true;
            return result;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            var result = new StorageCopyResult();
            var tempFilePath = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, String.Format("cache\\{0}", Guid.NewGuid().ToString())); //PathUtil.CombinePath(this.SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName));
            try
            {
                var cacheBuffer = new byte[64 * 1024];
                //下载 到本地
                using (var cacheStream = OpenStream(sourceFileInfo, FileMode.Open))
                {
                    var tempFile = new FileInfo(tempFilePath);
                    //目的文件所在路径要是不存在则创建
                    if (!tempFile.Directory.Exists)
                    {
                        tempFile.Directory.Create();
                    }
                    using (var innerStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, cacheBuffer.Length))
                    {
                        var readLen = default(Int32);
                        while ((readLen = cacheStream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                        {
                            innerStream.Write(cacheBuffer, 0, readLen);
                        }
                    }
                }
                //上传到TSM Server
                using (var innerStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, cacheBuffer.Length))
                {
                    using (var uploader = OpenStream(targetFileInfo, FileMode.Create))
                    {
                        var readLen = default(Int32);
                        while ((readLen = innerStream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                        {
                            uploader.Write(cacheBuffer, 0, readLen);
                        }
                        uploader.Commit(true);
                    }
                }
                result.IsCopyed = true;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.IsCopyed = false;
                logger.Error("copy file failed: {0}.", e);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            return result;
        }

        /// <summary>
        /// List All Files In TSM When It Match
        /// </summary>
        /// <param name="dirInfo">Which Files  You Want to List</param>
        /// <returns>The Files</returns>
        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            var result = new List<XFileInfo>();
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(dirInfo);
                result = this.client.ListFile(GetCheckSession(), storageInfo.HighPlusLowName, storageInfo.ListFilter ?? this.selectAll);
            }
            catch (Exception e)
            {
                logger.Error("Error when list container, name : {0}, details {1}.", dirInfo.HighName, e);
                throw;
            }
            return result;
        }

        private static BrowserLocker GetLocker(TSMNodeInfo nodeInfo)
        {
            var key = GetLockerKey(nodeInfo);
            if (!listLockers.ContainsKey(key))
            {
                lock (listLockers)
                {
                    if (!listLockers.ContainsKey(key))
                    {
                        listLockers[key] = new BrowserLocker();
                    }
                }
            }
            return listLockers[key];
        }

        private static String GetLockerKey(TSMNodeInfo nodeInfo)
        {
            return String.Format("{0}-{1}-{2}-{3}", nodeInfo.TcpServerAddress, nodeInfo.Port, nodeInfo.Nodename, nodeInfo.Filespace).Replace(':', '-');
        }

        private static String GetCacheFileFullPath(TSMNodeInfo nodeInfo)
        {
            return Path.Combine(ExecutorContext.BinDirectory, String.Format(@"storage\tsm\opts\{0}.xml", GetLockerKey(nodeInfo)));
        }
        /// <summary>
        /// List All Directories In TSM When It Match
        /// </summary>
        /// <param name="dirInfo">Which Directory  You Want List</param>
        /// <returns>The Directories</returns>
        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            var result = new List<XDirectoryInfo>();
            try
            {
                CheckState();
                var storageInfo = TSMUtil.FormateTsmNode(dirInfo);
                var tree = default(XElement);
                var filepath = GetCacheFileFullPath(this.nodeInfo);
                if (storageInfo.HighPlusLowName.LastIndexOf('\\') <= 0)
                {
                    var locker = GetLocker(this.nodeInfo);
                    if (!locker.Listing)
                    {
                        locker.Listing = true;
                        var thread = new Thread(new ParameterizedThreadStart((Object obj) =>
                        {
                            var system = default(TSMSystem);
                            try
                            {
                                using (system = XFactory.InstanceSystem(obj as string) as TSMSystem)
                                {
                                    var files = system.ListFiles((new StorageInfo() { HighName = "\\*", ListFilter = "\\*a*_0.dat" }));//).ListFile(GetCheckSession(), selectAll, "\\*a*_0.dat");
                                    var root = new XElement("Root");
                                    foreach (var file in files)
                                    {
                                        AddChildren(root, file);
                                    }
                                    lock (TSMSystem.GetLocker(system.nodeInfo))
                                    {
                                        root.Save(TSMSystem.GetCacheFileFullPath(system.nodeInfo));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message, ex);
                            }
                            finally
                            {
                                if (system != null)
                                {
                                    var llr = TSMSystem.GetLocker(system.nodeInfo);
                                    llr.Listing = false;
                                }
                            }
                        }));
                        thread.Start(this.XriString);
                        if (!File.Exists(filepath))
                        {
                            thread.Join();
                        }
                    }
                }
                lock (GetLocker(this.nodeInfo))
                {
                    if (File.Exists(filepath))
                    {
                        tree = XElement.Load(filepath);
                    }
                }
                var elements = ListElement(tree, storageInfo.HighPlusLowName);
                if (elements != null)
                {
                    foreach (var element in elements)
                    {
                        result.Add(new TSMDirectoryInfo(storageInfo.HighPlusLowName.TrimStart('\\').TrimEnd('\\'), element.Attribute("Name").Value.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error when list directory, name : {0}, details {1}.", dirInfo.HighName, e);
                throw;
            }
            return result;
        }

        IEnumerable<XElement> ListElement(XElement root, String fullPath)
        {
            var result = default(IEnumerable<XElement>);
            var names = fullPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach (String name in names)
                {
                    root = root.Elements("Folder").Single(folder => name.Equals(folder.Attribute("Name").Value.ToString(), StringComparison.CurrentCultureIgnoreCase));
                }
                result = root.Elements("Folder");
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            return result;
        }

        private void AddChildren(XElement root, XFileInfo branch)
        {
            var names = branch.HighName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String name in names)
            {
                var nodes = root.Elements("Folder").Where(folder => name.Equals(folder.Attribute("Name").Value.ToString(), StringComparison.CurrentCultureIgnoreCase));
                if (nodes.Count() <= 0)
                {
                    root.Add(new XElement("Folder", new XAttribute("Name", name)));
                }
                root = root.Elements("Folder").Single(n => name.Equals(n.Attribute("Name").Value.ToString(), StringComparison.CurrentCultureIgnoreCase));
            }
        }

        /// <summary>
        /// List all directories and files.
        /// </summary>
        /// <param name="dirInfo">Which you want to list</param>
        /// <returns>The directories and files</returns>
        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            var result = new StorageListResult();
            result.SubDirs = ListDirectories(dirInfo);
            result.Files = ListFiles(dirInfo);
            return result;
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode)
        {
            dirInfo = TSMUtil.FormateTsmNode(dirInfo);
            var dir = new TSMDirectoryInfo(dirInfo.HighName, dirInfo.LowName);
            dir.IsExists = true;
            var exist = this.client.CheckObject(GetSystemSession(), TSMUtil.AddDelimiter(dir.HighName), TSMUtil.AddDelimiter(dir.LowName), DSMObjType.DSM_DIRECTORY);
            if (!exist)
            {
                this.client.CreateDirectory(GetSystemSession(), TSMUtil.AddDelimiter(dir.HighName), TSMUtil.AddDelimiter(dir.LowName));
            }
            return dir;
        }

        public override StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, Boolean isOverWrite)
        {
            throw new NotSupportedException();
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            throw new NotSupportedException();
        }
    }
}
