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

namespace AvePoint.Media.Storage.Util
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.FS;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    #endregion

    public class Win32API
    {
        [DllImportAttribute("kernel32.dll", EntryPoint = "SetDllDirectoryW")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool SetDllDirectoryW([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpPathName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool FlushFileBuffers(SafeFileHandle hFile);

        public static bool SetDllDirectory(string aditionalDllPath)
        {
            return SetDllDirectoryW(aditionalDllPath);
        }

        public static bool FlushBuffers(SafeFileHandle hFile)
        {
            return FlushFileBuffers(hFile);
        }
    }

    public class StringHelper
    {
        public static string[] SplitDomainAndUsername(string domainWithUsername)
        {
            string username = domainWithUsername;
            string[] result = new string[2];
            if (!string.IsNullOrEmpty(username))
            {
                if (username.Contains("\\"))
                {
                    result[0] = username.Substring(0, username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase));
                    result[1] = username.Substring(username.IndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);
                }
                else if (username.Contains("@"))
                {
                    result[1] = username.Substring(0, username.IndexOf("@", StringComparison.OrdinalIgnoreCase));
                    result[0] = username.Substring(username.IndexOf("@", StringComparison.OrdinalIgnoreCase) + 1);
                }
                else
                {
                    result[0] = ".";
                    result[1] = username;
                }
            }
            return result;
        }
    }

    public class UNCIdentity : IDisposable
    {
        public AveImpersonator Impersonator { get; set; }

        private string location;
        private string username;
        private string password;
        private string domain;

        public UNCIdentity(string location, string domain, string username, string password)
        {
            this.location = location;
            this.domain = domain;
            this.username = username;
            this.password = password;
        }

        private UNCIdentity()
        {
        }

        public UNCIdentity Impersonate()
        {
            //if (sys.SystemLocation.StartsWith(FSSystemConst.UNC_FLAG, StringComparison.OrdinalIgnoreCase))
            UNCIdentity identity = new UNCIdentity();
            if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                if (location.StartsWith("\\", StringComparison.CurrentCultureIgnoreCase))
                {
                    identity.Impersonator = new AveImpersonator(domain, username, password, true);
                }
                else
                {
                    identity.Impersonator = new AveImpersonator(domain, username, password, false);
                }
                identity.Impersonator.Impersonate();
            }
            return identity;
        }

        public void Dispose()
        {
            if (Impersonator != null)
            {
                Impersonator.Dispose();
                Impersonator = null;
            }
        }
    }

    public enum JobType
    {
        SOArchiver,
        SOExtender,
        DPPlatformBackup,
        DPGranularBackup,
        ComplianceEDiscovery,
        DPPlatformBackupForSMSP
    }

    public enum DeviceIndex
    {
        FS = 0,
        FTP = 1,
        TSM = 2,
        EMCCentera = 3,
        Cloud = 4,
        Amazon = 401,
        Rackspace = 402,
        Azure = 403,
        Atmos = 404,
        Att = 405,
        HDS = 406,
        Dropbox = 407,
        S3Compatible = 410,
        DELLDXStorage = 5,
        MirrorFS = 6,
        NetApp = 7,
        LUN = 701,
        CIFS = 702,
        CARINGOStorage = 8,
        Box = 9,
        GoogleDrive = 10,
        SkyDrive = 11,
        IBMStorwizeFamily = 12,
        NFS = 13,
        WMS = 14,
        OpenStack = 501,
        IBMElasticStorage = 502,
        Cleversafe = 601
    }

    public class StorageLogger
    {
        private AveLogger aveLogger;

        public StorageLogger(Type type)
        {
            aveLogger = AveLogger.GetInstance(type);
        }

        public static StorageLogger GetInstance(Type type)
        {
            return new StorageLogger(type);
        }

        #region --public properties--
        public AveLogLevel CurrentLogLevel { get { return aveLogger.CurrentLogLevel; } }
        public bool IsErrorEnabled { get { return aveLogger.IsErrorEnabled; } }
        public bool IsWarnEnabled { get { return aveLogger.IsWarnEnabled; } }
        public bool IsInfoEnabled { get { return aveLogger.IsInfoEnabled; } }
        public bool IsDebugEnabled { get { return aveLogger.IsDebugEnabled; } }
        #endregion

        /// <summary>
        /// 写debug level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Debug(string formatStr, params object[] args)
        {
            aveLogger.Debug(formatStr, args);
        }

        /// <summary>
        /// 写info level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Info(string formatStr, params object[] args)
        {
            aveLogger.Info(formatStr, args);
        }

        /// <summary>
        /// 写warn level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Warn(string formatStr, params object[] args)
        {
            aveLogger.Warn(formatStr, args);
        }

        /// <summary>
        /// 写error level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Error(string formatStr, params object[] args)
        {
            aveLogger.Error(formatStr, args);
        }

        public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMsg)
        {
            try
            {
                aveLogger.Log(eventSource, taskCategory, eventMsg);
            }
            catch (Exception e)
            {
                aveLogger.Warn(e.ToString());
            }
        }
    }

    public class SecretUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SecretUtil));

        public static string DescriptCommunicationPassword(string ePass)
        {
            if (string.IsNullOrEmpty(ePass))
            {
                return ePass;
            }
            try
            {
                byte[] psBinary = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(ePass);//.DecryptInfoToBinary(encryptPassword);
                return AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ConvertBytesToString(psBinary);
            }
            catch (Exception tx)
            {
                logger.Error("Decrypt password failed by using the communication key, " + tx.Message, tx);
                throw;
            }
        }

        public static string DescryptPassword(string encryptPassword)
        {
            if (string.IsNullOrEmpty(encryptPassword))
            {
                return encryptPassword;
            }
            try
            {
                byte[] psBinary = AvePoint.GCommon.Utility.Cryptography.CspCrossPlatformExchangeWrapper.UnWrapKey(encryptPassword);//.DecryptInfoToBinary(encryptPassword);
                return AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ConvertBytesToString(psBinary);
            }
            catch (Exception e)
            {
                logger.Error("Decrypt password failed by using the hardcode key - " + e.Message, e);
                throw;
                //try
                //{
                //    logger.Error("Decrypt password failed by using the hardcode key, try to use communication key, " + e.Message, e);
                //    byte[] psBinary = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.UnWrapKey(encryptPassword);//.DecryptInfoToBinary(encryptPassword);
                //    return AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ConvertBytesToString(psBinary);
                //}
                //catch (Exception tx)
                //{
                //    logger.Error("Decrypt password failed by using the communication key, " + tx.Message, tx);
                //    return encryptPassword;
                //}
            }
        }

        public static string EncryptPassword(string cleartextPassword)
        {
            if (string.IsNullOrEmpty(cleartextPassword))
            {
                return cleartextPassword;
            }
            byte[] psBinary = AvePoint.GCommon.Utility.Cryptography.CryptoUtil.ConvertStringToBytes(cleartextPassword);
            return AvePoint.GCommon.Utility.Cryptography.CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(psBinary);//.EncryptBinaryInfo(psBinary);
            //return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(encryptPs);
        }
    }

    public class UNCObj
    {
        private string domain;
        private string username;
        private string decryptedPassword;
        private string encryptedPassword;
        private string location;

        public string Location
        {
            get { return location; }
            set { this.location = value; }
        }

        public string Domain
        {
            get { return domain; }
        }

        public string Username
        {
            get { return username; }
        }

        public string DecryptedPassword
        {
            get { return decryptedPassword; }
        }

        public string ServerHost
        {
            set;
            get;
        }

        public string ForderName
        {
            set;
            get;
        }

        public static UNCObj ValueOf(XRI uncXRIObj)
        {
            if (string.IsNullOrEmpty(uncXRIObj["name"]) && string.IsNullOrEmpty(uncXRIObj["secret"]))
            {
                foreach (KeyValuePair<string, string> pair in uncXRIObj.Params)
                {
                    Match match = Regex.Match(pair.Key, "system_[0-9]+");
                    if (match.Success == true)
                    {
                        return ValueOf(pair.Value);
                    }
                }
                UNCObj uncObj = new UNCObj();
                string domainWithUserName = uncXRIObj["name"];
                string[] dn = StringHelper.SplitDomainAndUsername(domainWithUserName);
                uncObj.domain = dn[0];
                uncObj.username = dn[1];
                uncObj.decryptedPassword = SecretUtil.DescryptPassword(uncXRIObj["secret"]);
                uncObj.encryptedPassword = uncXRIObj["secret"];
                uncObj.location = uncXRIObj["location"];
                if (!string.IsNullOrEmpty(uncObj.location) && uncObj.location.Contains("\\"))
                {
                    uncObj.ServerHost = GetServerHost(uncObj.location);
                    uncObj.ForderName = GetForderName(uncObj.location);
                }
                return uncObj;
            }
            else
            {
                UNCObj uncObj = new UNCObj();
                string domainWithUserName = uncXRIObj["name"];
                string[] dn = StringHelper.SplitDomainAndUsername(domainWithUserName);
                uncObj.domain = dn[0];
                uncObj.username = dn[1];
                uncObj.decryptedPassword = SecretUtil.DescryptPassword(uncXRIObj["secret"]);
                uncObj.encryptedPassword = uncXRIObj["secret"];
                uncObj.location = uncXRIObj["location"];
                if (!string.IsNullOrEmpty(uncObj.location) && uncObj.location.Contains("\\"))
                {
                    uncObj.ServerHost = GetServerHost(uncObj.location);
                    uncObj.ForderName = GetForderName(uncObj.location);
                }
                return uncObj;
            }
        }

        public static string GetServerHost(String location)
        {
            return location.Substring(2, location.LastIndexOf("\\", StringComparison.CurrentCulture) - 2);
        }

        private static string GetForderName(String location)
        {
            return location.Substring(location.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
        }

        public static UNCObj ValueOf(String uncConnectString)
        {
            XRI uncXRIObj = XRI.ValueOf(uncConnectString);
            return ValueOf(uncXRIObj);
        }
    }

    /// <summary>
    /// 常用静态方法
    /// </summary>
    public class FileUtil
    {
        public static string ReadAllTxt(XLibrary library, string highName, string lowName, Encoding encoding)
        {
            XStream stream = null;
            try
            {
                StorageInfo info = new StorageInfo();
                info.HighName = highName;
                info.LowName = lowName;
                info.BufferSize = 64 * 1024;
                stream = library.OpenStream(info, FileMode.Open);
                //byte[] buffer = new byte[stream.Length];
                //stream.Read(buffer, 0, buffer.Length);
                //String content = encoding.GetString(buffer);
                //return content;
                return new StreamReader(stream, encoding).ReadToEnd();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public static string ReadAllTxt(string connectString, string highName, string lowName, Encoding encoding)
        {
            IXSystem sys = null;
            XStream stream = null;

            try
            {

                sys = XFactory.InstanceSystem(connectString);
                sys.Open();

                StorageInfo info = new StorageInfo();
                info.HighName = highName;
                info.LowName = lowName;
                info.BufferSize = 64 * 1024;
                stream = sys.OpenStream(info, FileMode.Open);
                //byte[] buffer = new byte[stream.Length];
                //stream.Read(buffer, 0, buffer.Length);

                //String content = encoding.GetString(buffer);
                //return content;
                return new StreamReader(stream, encoding).ReadToEnd();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (sys != null)
                {
                    sys.Close();
                }
            }
        }

        public static string ReadAllTxt(string connectString, string highName, string lowName)
        {
            return ReadAllTxt(connectString, highName, lowName, Encoding.UTF8);
        }
        public static string ReadAllTxt(string connectString, StorageInfo info, Encoding encoding)
        {
            IXSystem sys = null;
            XStream stream = null;

            try
            {

                sys = XFactory.InstanceSystem(connectString);
                sys.Open();

                info.BufferSize = 64 * 1024;
                stream = sys.OpenStream(info, FileMode.Open);
                //byte[] buffer = new byte[stream.Length];
                //stream.Read(buffer, 0, buffer.Length);

                //String content = encoding.GetString(buffer);
                //return content;
                return new StreamReader(stream, encoding).ReadToEnd();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (sys != null)
                {
                    sys.Close();
                }
            }
        }
    }

    public struct SHARE_INFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _shi2_netname;

        public string shi2_netname
        {
            get { return _shi2_netname; }
            set { _shi2_netname = value; }
        }
        private uint _shi2_type;

        public uint shi2_type
        {
            get { return _shi2_type; }
            set { _shi2_type = value; }
        }
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _shi2_remark;

        public string shi2_remark
        {
            get { return _shi2_remark; }
            set { _shi2_remark = value; }
        }
        private uint _shi2_permissions;

        public uint shi2_permissions
        {
            get { return _shi2_permissions; }
            set { _shi2_permissions = value; }
        }
        private uint _shi2_max_uses;

        public uint shi2_max_uses
        {
            get { return _shi2_max_uses; }
            set { _shi2_max_uses = value; }
        }
        private uint _shi2_current_uses;

        public uint shi2_current_uses
        {
            get { return _shi2_current_uses; }
            set { _shi2_current_uses = value; }
        }
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _shi2_path;

        public string shi2_path
        {
            get { return _shi2_path; }
            set { _shi2_path = value; }
        }
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _shi2_passwd;

        public string shi2_passwd
        {
            get { return _shi2_passwd; }
            set { _shi2_passwd = value; }
        }
    }

    public class ExecutorContext
    {
        private static StorageLogger logger = StorageLogger.GetInstance(typeof(ExecutorContext));

        public static string BinDirectory
        {
            get
            {
                try
                {
                    string httpRuntimeBin = HttpRuntime.BinDirectory;
                    if (httpRuntimeBin.StartsWith(AppDomain.CurrentDomain.BaseDirectory, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return httpRuntimeBin;
                    }
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("get bin dir failed, we will use " + AppDomain.CurrentDomain.BaseDirectory + ex.Message);
                    string assmblyPath = Assembly.GetExecutingAssembly().Location;
                    string dllName = Assembly.GetExecutingAssembly().ManifestModule.Name;
                    assmblyPath = assmblyPath.TrimEnd(dllName.ToCharArray());
                    return assmblyPath;
                }
            }
        }
    }

    /// <summary>
    /// netapp util class
    /// </summary>
    #region CodeReview

    [AveCodeReview(
    "2012/3/28",
    "dapeng.zhang@avepoint.com",
    "laing.wang@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_2, CodeReviewConstants.CHECK_LIST_ID_CO_6 },
    "ADO-26069",
    true)]
    #endregion
    public class OntapUtil
    {
        private static StorageLogger logger = StorageLogger.GetInstance(typeof(OntapUtil));

        public static List<OntapItemInfo> LoadLuns()
        {
            try
            {
                Assembly ass = XFactory.GetAssembly("netapp_lun_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("LoadLuns");
                object rs = m.Invoke(obj, null);
                return rs as List<OntapItemInfo>;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to load luns please check API is right.", e.Message, e);
            }

            return null;
        }

        public static Dictionary<string, string> SnaplockRetentionTime(SystemProfileDto profile, string name)
        {
            try
            {
                profile.Password = SecretUtil.DescriptCommunicationPassword(profile.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("SnaplockRetentionTime");
                object rs = m.Invoke(obj, new object[] { profile, name });
                return rs as Dictionary<string, string>;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get retention period. Volume:{0}   Error:{1}", name, e.ToString(), e);
            }
            return null;
        }

        public static List<OntapItemInfo> LoadCifsShares(SystemProfileDto profile, string binPath)
        {
            try
            {
                logger.Info("begin LoadCifsShares " + profile.SystemAddress + " SystemAddress : " + profile.SystemAddress);
                profile.Password = SecretUtil.DescriptCommunicationPassword(profile.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("LoadCIFSShares");
                object rs = m.Invoke(obj, new object[] { profile, binPath });
                return rs as List<OntapItemInfo>;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to load cifs shares. Error:{0}", e.Message, e);
            }
            return null;
        }

        public static List<OntapItemInfo> LoadNFSExports(SystemProfileDto profile, string binPath)
        {
            try
            {
                logger.Info("begin LoadNFSExports " + profile.SystemAddress + " SystemAddress : " + profile.SystemAddress);
                profile.Password = SecretUtil.DescriptCommunicationPassword(profile.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("LoadNFSExports");
                object rs = m.Invoke(obj, new object[] { profile, binPath });
                return rs as List<OntapItemInfo>;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to load NFS exports. Error:{0}", e.Message, e);
            }
            return null;
        }

        /// <summary>
        /// UpdateSnapMirror
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="filers"></param>
        /// <returns></returns>
        public static UpdateMirrorResult UpdateSnapMirror(UpdateMirrorVaultParameter parameter)
        {
            var result = UpdateMirrorResult.Unknown;
            if (parameter.System.Type != null)
            {
                if (parameter.System is XLibrary)
                {
                    //var results = (parameter.System as XLibrary).SubSystems.Select(subSystem => InternalUpdateSnapMirror(new UpdateMirrorVaultParameter(subSystem, parameter.Filers, parameter.JobDto, parameter.IsMaintenanceJob))).ToList();
                    List<UpdateMirrorResult> results = new List<UpdateMirrorResult>();
                    List<string> lunsUpdated = new List<string>();
                    foreach (IXSystem subSystem in (parameter.System as XLibrary).SubSystems)
                    {
                        UpdateMirrorVaultParameter newParameter = new UpdateMirrorVaultParameter(subSystem, parameter.Filers, parameter.JobDto, parameter.UpdateMirror, parameter.UpdateVault, parameter.IsMaintenanceJob);
                        if ((subSystem.XriObject.Params.ContainsKey("NetAppType".ToLower(CultureInfo.InvariantCulture)) && subSystem.XriObject.Params["NetAppType".ToLower(CultureInfo.InvariantCulture)].Equals("LUN", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            string mountPoint = InternalGetMountpoint(newParameter);
                            if (!lunsUpdated.Contains(mountPoint.ToLower(CultureInfo.CurrentCulture)))
                            {
                                results.Add(InternalUpdateSnapMirror(newParameter));
                                lunsUpdated.Add(mountPoint.ToLower(CultureInfo.CurrentCulture));
                            }
                        }
                        else
                        {
                            results.Add(InternalUpdateSnapMirror(newParameter));
                        }
                    }

                    if (results.Contains(UpdateMirrorResult.CompleteWithException) || results.Contains(UpdateMirrorResult.Unknown))
                        result = UpdateMirrorResult.CompleteWithException;
                    else if (results.Contains(UpdateMirrorResult.Completed) || results.Contains(UpdateMirrorResult.Skiped))
                        result = results.Contains(UpdateMirrorResult.Failed) ? UpdateMirrorResult.CompleteWithException : UpdateMirrorResult.Completed;
                    else
                        result = UpdateMirrorResult.Failed;
                }
                else
                    result = InternalUpdateSnapMirror(parameter);
            }
            return result;
        }

        private static UpdateMirrorResult InternalUpdateSnapMirror(UpdateMirrorVaultParameter parameter)
        {
            try
            {
                if (parameter.System.Type.Equals("NetAppSystem".ToLower(CultureInfo.InvariantCulture),
                    StringComparison.OrdinalIgnoreCase))
                {
                    var ass = XFactory.GetAssembly("netapp_cifs_vim");
                    var type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var m = type.GetMethod("UpdateSnapMirror");
                    var rs = m.Invoke(obj, new object[] { parameter });
                    return (UpdateMirrorResult)rs;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to UpdateSnapMirror Error:{0}", e.Message, e);
            }
            return UpdateMirrorResult.Failed;
        }

        /// <summary>
        /// UpdateSnapvault
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="filers"></param>
        /// <returns></returns>
        public static UpdateMirrorResult UpdateSnapVault(UpdateMirrorVaultParameter parameter)
        {
            var result = UpdateMirrorResult.Unknown;
            if (parameter.System.Type != null)
            {
                if (parameter.System is XLibrary)
                {
                    //var results = (parameter.System as XLibrary).SubSystems.Select(subSystem => InternalUpdateSnapVault(new UpdateMirrorVaultParameter(subSystem, parameter.Filers, parameter.JobDto, parameter.IsMaintenanceJob))).ToList();

                    List<UpdateMirrorResult> results = new List<UpdateMirrorResult>();
                    List<string> lunsUpdated = new List<string>();
                    foreach (IXSystem subSystem in (parameter.System as XLibrary).SubSystems)
                    {
                        UpdateMirrorVaultParameter newParameter = new UpdateMirrorVaultParameter(subSystem, parameter.Filers, parameter.JobDto, parameter.UpdateMirror, parameter.UpdateVault, parameter.IsMaintenanceJob);
                        if ((subSystem.XriObject.Params.ContainsKey("NetAppType".ToLower(CultureInfo.InvariantCulture)) && subSystem.XriObject.Params["NetAppType".ToLower(CultureInfo.InvariantCulture)].Equals("LUN", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            string mountPoint = InternalGetMountpoint(newParameter);
                            if (!lunsUpdated.Contains(mountPoint.ToLower(CultureInfo.CurrentCulture)))
                            {
                                results.Add(InternalUpdateSnapVault(newParameter));
                                lunsUpdated.Add(mountPoint.ToLower(CultureInfo.CurrentCulture));
                            }
                        }
                        else
                        {
                            results.Add(InternalUpdateSnapVault(newParameter));
                        }
                    }

                    if (results.Contains(UpdateMirrorResult.CompleteWithException) || results.Contains(UpdateMirrorResult.Unknown))
                        result = UpdateMirrorResult.CompleteWithException;
                    else if (results.Contains(UpdateMirrorResult.Completed) || results.Contains(UpdateMirrorResult.Skiped))
                        result = results.Contains(UpdateMirrorResult.Failed) ? UpdateMirrorResult.CompleteWithException : UpdateMirrorResult.Completed;
                    else
                        result = UpdateMirrorResult.Failed;
                }
                else
                    result = InternalUpdateSnapVault(parameter);
            }
            return result;
        }

        private static UpdateMirrorResult InternalUpdateSnapVault(UpdateMirrorVaultParameter parameter)
        {
            try
            {
                if (parameter.System.Type.Equals("NetAppSystem".ToLower(CultureInfo.InvariantCulture),
                    StringComparison.OrdinalIgnoreCase))
                {
                    var ass = XFactory.GetAssembly("netapp_cifs_vim");
                    var type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var m = type.GetMethod("UpdateSnapVault");
                    var rs = m.Invoke(obj, new object[] { parameter });
                    return (UpdateMirrorResult)rs;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to UpdateSnapvault Error:{0} , error message :" + e.Message, e);
            }
            return UpdateMirrorResult.Failed;
        }

        /// <summary>
        /// GetEmsAutoSupportLog
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="emsAutoInfo"></param>
        /// <returns></returns>
        public static string GetEmsAutoSupportLog(SystemProfileDto dto, EmsAutoSupportLogDto emsAutoInfo)
        {
            try
            {
                dto.Password = SecretUtil.DescriptCommunicationPassword(dto.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("GetEmsAutoSupportLog");
                object rs = m.Invoke(obj, new object[] { dto, emsAutoInfo });
                return rs as string;
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get EmsAutoSupportLog Error:{0}", e.Message, e);
            }
            return null;
        }

        /// <summary>
        ///  验证systemProfileDto 是否正确  如果正确则返回systemProfile 的version
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static string ValidateSystemProfile(SystemProfileDto profile, string binPath)
        {
            try
            {
                profile.Password = SecretUtil.DescriptCommunicationPassword(profile.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("ValidateSystemProfile");
                object rs = m.Invoke(obj, new object[] { profile, binPath });
                return (string)rs;
            }
            catch (Exception e)
            {
                logger.Warn("an exception while validateSystemProfile ", e.Message, e);
                return null;
            }
        }

        /// <summary>
        ///  验证systemProfileDto 是否正确  如果正确则返回systemProfile 的version
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static DateTime GetNetAppSystemTime(SystemProfileDto profile)
        {
            try
            {
                profile.Password = SecretUtil.DescriptCommunicationPassword(profile.Password);
                Assembly ass = XFactory.GetAssembly("netapp_cifs_vim");
                Type type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                object obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                MethodInfo m = type.GetMethod("GetNetAppSystemTime");
                object rs = m.Invoke(obj, new object[] { profile });
                return (DateTime)rs;
            }
            catch (Exception e)
            {
                logger.Warn("an exception while GetNetAppSystemTime ", e.Message, e);
                throw;
            }
        }


        /// <summary>
        /// netApp UpdateMirrorResult
        /// </summary>
        public enum UpdateMirrorResult
        {
            Completed = 0,
            CompleteWithException = 1,
            Failed = 2,
            Skiped = 3,
            Unknown = -1
        }

        //TODO
        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetSystemProfileNameFromConnectionString(string connectionString)
        {
            try
            {
                if (XRI.ValueOf(connectionString).Params.ContainsKey("cifs_profile".ToLower(CultureInfo.InvariantCulture)))
                    return XRI.ValueOf(connectionString).Params["cifs_profile".ToLower(CultureInfo.InvariantCulture)];
                else return null;
            }
            catch (Exception t)
            {
                logger.Warn(t.ToString());
                return null;
            }
        }

        public static string GetConnectionParamByKey(string key, string connectionString)
        {
            return XsystemParamUtil.GetParamByKeyFromConnectionString(key, connectionString);
        }

        public class UpdateMirrorVaultParameter
        {
            public IXSystem System { get; set; }
            public List<SystemProfileDto> Filers { get; set; }
            public BaseJobDto JobDto { get; set; }
            public List<String> BackupJobIds { get; set; }
            public bool IsMaintenanceJob { get; set; }
            public bool UpdateMirror { get; set; }
            public bool UpdateVault { get; set; }

            public UpdateMirrorVaultParameter(IXSystem mSystem, List<SystemProfileDto> mFilers, BaseJobDto mJobDto, bool isUpdateMirror, bool isUpdateVault, bool isMaintenance = false)
            {
                this.System = mSystem;
                this.Filers = mFilers;
                this.JobDto = mJobDto;
                this.IsMaintenanceJob = isMaintenance;
                this.UpdateMirror = isUpdateMirror;
                this.UpdateVault = isUpdateVault;
            }

            public UpdateMirrorVaultParameter(IXSystem mSystem, List<SystemProfileDto> mFilers, BaseJobDto mJobDto, List<String> mBackupJobIds)
                : this(mSystem, mFilers, mJobDto, false, false, false)
            {
                this.BackupJobIds = mBackupJobIds;
            }
        }

        /// <summary>
        /// Retention Job中删除Device产生的SnapVault源端快照
        /// </summary>
        /// <param name="parameter"></param>
        public static void DeleteDeviceSnapshotsByJobs(UpdateMirrorVaultParameter parameter)
        {
            try
            {
                if (parameter.System.Type != null)
                {
                    if (parameter.System is XLibrary)
                    {
                        foreach (IXSystem subSystem in (parameter.System as XLibrary).SubSystems)
                        {
                            InternalDeleteDeviceSnapshotsByJobs(new UpdateMirrorVaultParameter(subSystem, parameter.Filers, parameter.JobDto, parameter.BackupJobIds));
                        }
                    }
                    else
                    {
                        InternalDeleteDeviceSnapshotsByJobs(parameter);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to delete snapshots, error: {0}.", e.ToString());
            }
        }

        private static void InternalDeleteDeviceSnapshotsByJobs(UpdateMirrorVaultParameter parameter)
        {
            try
            {
                if (parameter.System.Type.Equals("NetAppSystem".ToLower(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                {
                    var ass = XFactory.GetAssembly("netapp_cifs_vim");
                    var type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var m = type.GetMethod("DeleteDeviceSnapshotsByJobs");
                    m.Invoke(obj, new object[] { parameter });
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to delete snapshots, error: {0}.", e.ToString());
            }
        }

        private static string InternalGetMountpoint(UpdateMirrorVaultParameter parameter)
        {
            try
            {
                if (parameter.System.Type.Equals("NetAppSystem".ToLower(CultureInfo.InvariantCulture),
                    StringComparison.OrdinalIgnoreCase))
                {
                    var ass = XFactory.GetAssembly("netapp_cifs_vim");
                    var type = ass.GetType("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var obj = ass.CreateInstance("AvePoint.Media.Storage.NetApp.NetAppUtil");
                    var m = type.GetMethod("GetMountpoint");
                    var rs = m.Invoke(obj, new object[] { parameter });
                    return (string)rs;
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
            }
            return string.Empty;
        }
    }

    public class XsystemParamUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(XsystemParamUtil));

        public static string GetParamByKeyFromConnectionString(string key, string connectionString)
        {
            try
            {
                if (XRI.ValueOf(connectionString).Params.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)))
                    return XRI.ValueOf(connectionString).Params[key.ToLower(CultureInfo.InvariantCulture)];
                else return null;
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                return null;
            }
        }

        public static string GetParamByKeyFromXRI(string key, XRI xri)
        {
            try
            {
                if (xri.Params.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)))
                    return xri.Params[key.ToLower(CultureInfo.InvariantCulture)];
                else return null;
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                return null;
            }
        }
    }

    /// <summary>
    /// try to acquire zhe designated netshare forder absolute Path,
    /// if an exception occurs in runtime will return zhe relative path.
    /// </summary>
    public class PathUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(PathUtil));

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        public static extern int NetShareGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string netname, int level, ref IntPtr bufptr);

        public static IntPtr ptr = IntPtr.Zero;
        private const string dr = "\\";

        private static string ReName(string path, string serverHost)
        {
            try
            {
                string tempPath = path;
                string tempPath2 = tempPath.Replace(":", @"$");
                tempPath2 = dr + dr + serverHost + dr + tempPath2;
                return tempPath2;
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                throw new ArgumentException();
            }
        }

        private static Boolean IsLocalHostIP(string serverHost)
        {
            try
            {
                System.Net.IPAddress[] addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;
                if (addressList.Length == 0)
                    throw new ArgumentException();
                for (int i = 0; i < addressList.Length; i++)
                {
                    if (serverHost.Equals(addressList[i].ToString(), StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                if (serverHost.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                    return false;
                return true;
            }
            catch (Exception e)
            {
                logger.Warn(e.ToString());
                throw new Exception();
            }
        }

        /// <summary>
        /// example
        /// input   :\\\\10.2.6.33\\shareForder
        /// return  : \\\\10.2.6.33\\C$\\shareForder
        /// Exceptions: retrun  \\\\10.2.6.33\\shareForder
        /// </summary>
        public static string GetNetShareForderRealPath(String connectString)
        {
            Boolean isLocalHost = true;
            try
            {
                UNCObj uncObj = UNCObj.ValueOf(connectString);
                isLocalHost = IsLocalHostIP(uncObj.ServerHost);
                using (AveImpersonator permission = new AveImpersonator(uncObj.Domain, uncObj.Username, uncObj.DecryptedPassword, isLocalHost))
                {
                    permission.Impersonate();
                    NetShareGetInfo(uncObj.ServerHost, uncObj.ForderName, 2, ref ptr);
                    SHARE_INFO shareInfo = (SHARE_INFO)Marshal.PtrToStructure(ptr, typeof(SHARE_INFO));
                    string path = ReName(shareInfo.shi2_path, uncObj.ServerHost);
                    if (path != null)
                        return path;
                    else
                        return connectString;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
                return connectString;
            }
        }

        /// <summary>
        /// instead of Path.combine
        /// </summary>

        public static string CombinePath(string firstPath, string secondPath)
        {
            if (string.IsNullOrEmpty(firstPath))
            {
                return secondPath;
            }
            if (string.IsNullOrEmpty(secondPath) || secondPath.Equals("\\", StringComparison.OrdinalIgnoreCase) || secondPath.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                return firstPath;
            }
            if (secondPath.StartsWith("\\", StringComparison.OrdinalIgnoreCase) || secondPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                secondPath = secondPath.TrimStart(new char[] { '\\', '/' });
            }
            if (secondPath.Contains(":"))
            {
                firstPath = HttpUtility.UrlEncode(firstPath).Replace("+", "%20").Replace("%2f", "/");
                secondPath = HttpUtility.UrlEncode(secondPath).Replace("+", "%20").Replace("%2f", "/");
                return HttpUtility.UrlDecode(Path.Combine(firstPath, secondPath));
            }
            else
            {
                return Path.Combine(firstPath, secondPath);
            }
        }

        public static bool IsSameDrive(IXSystem sourceSystem, IXSystem destSystem)
        {
            var result = SharePathUtil.IsSameDrive(sourceSystem, destSystem);
            return result;
        }
    }

    /// <summary>
    /// 这里定义的storage api的所有常量, 针对具体device的常量可继承相关的类, e.g. FSSystemConst.
    /// </summary>
    public class XConst
    {
        public const string UTF_8 = "UTF-8";
        public const int MILLISEC_PER_SECOND = 1000;
        public const long END_OF_STREAM = 0;
        public const string EQUALS_SIGN = "=";
        public const string SINGLE_QUOTE = "\'";
        public const string DOUBLE_QUOTE = "\"";
        public const string FILE_SEPARATOR = "\\";
        public const string FILE_TEMP_LOCATION = "";
        public static readonly string DOCAVE = "DocAve".ToLower(CultureInfo.InvariantCulture);
        public static readonly string MEDIASTORAGE_PROTOCOL = "DOCAVE-XAM://".ToLower(CultureInfo.InvariantCulture);
    }

    public enum ModifyTimeType
    {
        LastWriteTimeUtcType = 0,

        LastWriteTimeType = 1,

        LastAccessTimeType = 2,

        LastAccessTimeUtcType = 3,

        CreationTimeType = 4,

        CreationTimeUtcType = 5
    }

    public enum FileBlockType
    {
        /// <summary>
        /// 50M one file, 4kb default, configrable, 文件末尾不满4k需要补齐
        /// </summary>
        SingleInstanceLevel_Block = 0,

        /// <summary>
        /// no size limit one file, 1b
        /// </summary>
        SingleInstanceLevel_Block_NoSizeLimit = 1,

        /// <summary>
        /// no size limit one file, no other content
        /// </summary>
        SingleInstanceLevel_File = 2,
    }

    public enum FileStatus
    {
        Unknown,
        Exist,
        NotExist
    }

    public enum DeleteStatus
    {
        Unknown,
        Deleted,
        DeletedWithException
    }

    /// <summary>
    /// 这个enum是为了区分不同功能添加的，某些方法有时候需要根据功能进行一些特殊处理。
    /// </summary>
    public enum ModuleType
    {
        MediaService = 0,
        Connector = 1,
    }

    public class XLibraryConst
    {
        public const string MODE_LAZY_INITSYSTEM = "lazyinit";
        public const string MODE_NOW_INITSYSTEM = "nowinit";
    }

    public class XConvert
    {
        public static StorageInfo FromNames(string highName, string lowName)
        {
            StorageInfo info = new StorageInfo();
            info.HighName = highName;
            info.LowName = lowName;
            return info;
        }

        public static StorageInfo FromNames(string highName, string lowName, string extraStorageInfo)
        {
            StorageInfo info = new StorageInfo();
            info.HighName = highName;
            info.LowName = lowName;
            info.ExtraStorageInfo = extraStorageInfo;
            return info;
        }

        public static StorageInfo FromNames(String highName, String lowerName, Dictionary<String, String> metaInfos)
        {
            return FromNames(highName, lowerName, null, metaInfos);
        }

        public static StorageInfo FromNames(String highName, String lowName, String extraStorageInfo = null, Dictionary<String, String> metaInfos = null)
        {
            var info = new StorageInfo { HighName = highName, LowName = lowName, ExtraStorageInfo = extraStorageInfo };
            if (metaInfos != null)
            {
                foreach (var item in metaInfos)
                {
                    info.MetaInfos[item.Key] = item.Value;
                }
            }
            return info;
        }
    }

    class SHA1Util : IDisposable
    {
        private HashAlgorithm sha1;

        public SHA1Util()
        {
            sha1 = new SHA1CryptoServiceProvider();
        }

        public String GetChecksumStringForBlob(Stream commitStream)
        {
            byte[] hashbytes = sha1.ComputeHash(commitStream);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return Convert(hashbytes);
        }

        public void ChecksumForBlock(byte[] b, int offset, int len)
        {
            sha1.TransformBlock(b, 0, len, b, 0);
        }

        public string ChecksumForBlockFinal(byte[] b, int offset, int len)
        {
            sha1.TransformFinalBlock(b, 0, len);
            return Convert(sha1.Hash);
        }

        private String Convert(byte[] b)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append(b[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            if (sha1 != null)
            {
                sha1.Clear();
            }
        }
    }

    public class XSystemConst
    {
        public static readonly string MODE_LAZY_INITSYSTEM = "lazyInit".ToLower(CultureInfo.InvariantCulture);

        public static readonly string MODE_NOW_INITSYSTEM = "nowInit".ToLower(CultureInfo.InvariantCulture);

        /** Read access is allowed, used in AccessXSet() */
        public const long ACCESS_READ_OK = 0x80000000L;

        /** Write access is allowed, used in AccessXSet() */
        public const long ACCESS_WRITE_APPLICATION_OK = 0x40000000L;

        /** Write access to system fields is allowed, used in AccessXSet() */
        public const long ACCESS_WRITE_SYSTEM_OK = 0x20000000L;

        /** Creating XSet is allowed, not used for AccessXSet(), defined as auth granule */
        public const long ACCESS_CREATE_OK = 0x10000000;

        /** Delete access is allowed, used in AccessXSet() */
        public const long ACCESS_DELETE_OK = 0x08000000L;

        /** Hold/Release operation on an XSet is allowed, used in AccessXSet() */
        public const long ACCESS_HOLD_OK = 0x04000000L;

        /** Event based retention operation on an XSet is OK, used in AccessXSet() */
        public const long ACCESS_RETENTION_EVENT_OK = 0x02000000L;

        /** Submit of a job is allowed, used in AccessXSet() */
        public const long ACCESS_JOB_OK = 0x01000000L;

        /** Commit of a job is allowed, used in AccessXSet() */
        public const long ACCESS_JOB_COMMIT_OK = 0x00800000L;
    }

    public class XStreamConst
    {
        /** Specify an offset seek() from the beginning of the XStream. */
        public const int SEEK_SET = 0;

        /** Specify an offset seek() from the current cursor of the XStream. */
        public const int SEEK_CUR = 1;

        /** Specify an offset seek() from the end of the XStream. */
        public const int SEEK_END = 2;

        /** Open mode read only. Used in openXStream(). */
        public const string MODE_READ_ONLY = "readonly";

        /** Open mode write only, truncate contents. Used in openXStream(). */
        public const string MODE_WRITE_TRUNCATE = "writeonly";

        /** Open mode write only, append to contents. Used in openXStream(). */
        public const string MODE_WRITE_APPEND = "appendonly";

        public const string MODE_EXISTS_DELETE_NEW = "deleteifexistsandnew";

        /** End of File value returned from a read() method when the end of XStream has been reached. */
        public const long EOF = -1;
    }

    public enum XDirectoryMode
    {
        OpenExist,
        CreateNew,
        OpenOrCreate
    }

    public enum XFileMode
    {
        OpenExist,
        CreateNew,
        DeleteAndCreateNew
    }

    public enum XFileAccess
    {
        Read,
        Write
    }

    public enum StorageInterfaceType
    {
        Namespace,
        Object
    }

    /// <summary>
    /// 控制创建分享链接的范围权限，Open表示所有人，Company表示公司用户，Collaborators表示合作者
    /// </summary>
    public enum AcessMode
    {
        Open,
        Company,
        Collaborators
    }

    /// <summary>
    /// 类似Centera有clipid, 用于处理这种情况下的xuid常量
    /// </summary>
    public class XUIDConst
    {
        public const int MAX_LENGTH = 80;

        public const int MIN_LENGTH = 9;
    }

    /// <summary>
    /// Unknow < ConnectedFailed < AuthenticationFailed < Unaccessable < Available < AvailableAndNotFull
    /// </summary>
    public enum XSystemHealth
    {
        Unknown = 0,
        /// <summary>
        /// 由于网络因素链接不上
        /// </summary>
        ConnectedFailed = 1,
        /// <summary>
        /// 认证失败, 比如用户名或密码错误
        /// </summary>
        AuthenticationFailed = 2,
        /// <summary>
        /// 认证通过, 但是因为文件夹不存在等因素导致不可访问
        /// </summary>
        Unaccessable = 3,
        /// <summary>
        /// 可以读, 但是因为磁盘空间等因素不可写
        /// </summary>
        Available = 4,
        /// <summary>
        /// 可读可写
        /// </summary>
        AvailableAndNotFull = 5
    }

    public enum XSystemStatus
    {
        Online,
        Offline
    }

    public enum XSystemUsage
    {
        All,
        Data,
        Index
    }

    /// <summary>
    /// For Extended parameters
    /// </summary>


    public enum XSystemValidateStatus
    {
        UnAvailableDeviceExist,
        Available
    }

    public interface IXUID
    {
        byte[] ToBytes();

        string ToString();

        bool Equals(Object x);

        IXUID Parse(string setName);
    }

    public class MarshalPtrToDSMObjectList : ICustomMarshaler
    {
        private static StorageLogger logger = StorageLogger.GetInstance(typeof(MarshalPtrToDSMObjectList));

        public void CleanUpManagedData(object ManagedObj) { }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            //try
            //{
            //    //Marshal.FreeHGlobal(pNativeData);
            //    //Marshal.FreeBSTR(pNativeData);
            //}
            //catch (Exception t)
            //{
            //    logger.Debug(t.Message, t);
            //    throw t;
            //}
        }

        public int GetNativeDataSize()
        {
            throw new NotSupportedException();
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            return IntPtr.Zero;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return DSMObjectItem.Convert(pNativeData);
        }

        //private static object locker = new object();
        //private static Dictionary<string, ICustomMarshaler> dics = new Dictionary<string, ICustomMarshaler>();

        private static MarshalPtrToDSMObjectList instance = new MarshalPtrToDSMObjectList();

        public static ICustomMarshaler GetInstance(String cookie)
        {
            //lock (locker)
            //{
            //if (!dics.ContainsKey(cookie))
            //{
            //    Type t = Type.GetType(cookie);
            //    ICustomMarshaler m = (ICustomMarshaler)Activator.CreateInstance(t);
            //    dics.Add(cookie, m);
            //}
            //return dics[cookie];
            //}
            //if (instance == null)
            //{
            //    instance = new MarshalPtrToDSMObjectList();
            //}
            return instance;
        }
    }

    public class DSMObjectItem
    {
        private string highName;
        public string HighName { get { return highName; } }
        private string lowName;
        public string LowName { get { return lowName; } }
        private long size;
        public long Size { get { return size; } }

        public static object Convert(IntPtr data)
        {
            try
            {
                if (data == IntPtr.Zero)
                    return null;

                int size = 0;
                for (size = 0; Marshal.ReadByte(data, size) > 0; size++)
                    ;
                if (size > 0)
                {
                    byte[] array = new byte[size];
                    Marshal.Copy(data, array, 0, size);
                    string dataStr = Encoding.UTF8.GetString(array);//.UTF8.GetString(array);
                    string[] dataStrs = dataStr.Split(',');
                    List<DSMObjectItem> objs = new List<DSMObjectItem>();

                    DSMObjectItem item;
                    foreach (string str in dataStrs)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            int index = str.LastIndexOf('\\');
                            if (index >= 0)
                            {
                                item = new DSMObjectItem();
                                item.highName = str.Substring(0, index);
                                string lo = str.Substring(index + 1, str.Length - (index + 1));
                                if (lo.Contains("|"))
                                {
                                    index = lo.LastIndexOf('|');
                                    item.lowName = lo.Substring(0, index);
                                    long fileSize = long.Parse(lo.Substring(index + 1));
                                    item.size = fileSize;
                                }
                                else
                                {
                                    item.lowName = lo;
                                }
                                objs.Add(item);
                            }
                        }
                    }
                    return objs.ToArray();
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
                return null;
            }
        }
    }

    public interface IConvertableUnmanagedData
    {
        object Convert(IntPtr data);
    }

    public class MarshalPtrToUtf8 : ICustomMarshaler
    {
        static MarshalPtrToUtf8 marshaler = new MarshalPtrToUtf8();

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeHGlobal(pNativeData);
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(byte));
        }

        public int GetNativeDataSize(IntPtr ptr)
        {
            int size = 0;
            for (size = 0; Marshal.ReadByte(ptr, size) > 0; size++)
                ;
            return size;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            if (ManagedObj == null)
                return IntPtr.Zero;

            if (ManagedObj.GetType() != typeof(string))
                throw new ArgumentException("CustomMarshal class MarshalPTRToUTF8 only works with System.String variables");

            byte[] array = Encoding.UTF8.GetBytes((string)ManagedObj);
            if (array == null || array.Length <= 0)
            {
                return Marshal.StringToHGlobalAnsi(string.Empty);
            }
            int size = Marshal.SizeOf(array[0]) * array.Length + Marshal.SizeOf(array[0]);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, array.Length);
            Marshal.WriteByte(ptr, size - 1, 0);
            return ptr;
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            if (pNativeData == IntPtr.Zero)
                return null;

            int size = GetNativeDataSize(pNativeData);
            byte[] array = new byte[size];
            Marshal.Copy(pNativeData, array, 0, size);
            return Encoding.UTF8.GetString(array);
        }

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return marshaler;
        }
    }

    public static class XStringHelper
    {

        public static string GetConfigHashForDevice(PhysicalDeviceDto physicalDeviceDto)
        {
            string result = string.Empty;
            IXSystem system = XFactory.InstanceSystem(physicalDeviceDto.BuildXRI());
            system.Open();
            result = system.SystemKey;
            system.Close();
            return result;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Dtrue")]
        /// <summary>
        /// 给XriString在任何情况下都加上readonly = true这一参数。
        /// </summary>
        /// <param name="xriString"></param>
        /// <returns></returns>
        public static string GetReadOnlyConnectionString(string xriString)
        {
            string result = xriString;
            XRI xri = XRI.ValueOf(xriString);
            if (xri.Params.ContainsKey(XRIParameterKeys.ADVANCED_KEY))
            {
                if (Convert.ToBoolean(xri.Params[XRIParameterKeys.ADVANCED_KEY]))
                {
                    string advancedParamValue = xri.Params[XRIParameterKeys.EXTENDED_PARAMETERS];
                    if (!advancedParamValue.Contains(XRIParameterKeys.READONLY))
                    {
                        xri.Params[XRIParameterKeys.EXTENDED_PARAMETERS] = ((advancedParamValue += "\r\nreadonly=true"));
                        result = xri.ToString();
                    }
                }
                else
                {
                    xri.Params[XRIParameterKeys.ADVANCED_KEY] = "true";
                    xri.Params[XRIParameterKeys.EXTENDED_PARAMETERS] = "readonly=true";
                }
                result = xri.ToString();
            }
            else
            {
                result += "&advanced=true&extendedParameters=readonly%3Dtrue".ToLower(CultureInfo.InvariantCulture);
            }
            return result;
        }

        public static string XRIEncode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
        }

        public static string XRIDecode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%").Replace("%5e", "^");
        }

        public static string RemoveFirst(this string str, string patValue)
        {
            if (!str.StartsWith(patValue, StringComparison.CurrentCulture))
            {
                return str;
            }

            int index = str.IndexOf(patValue, StringComparison.CurrentCulture);
            if (index < 0)
            {
                return str;
            }
            return str.Remove(0, patValue.Length);
        }
    }




    /// <summary>
    ///   RelfectSystemParams
    /// </summary>
    public class RelfectSystemParams
    {
        /// <summary>
        /// RelfectProperty
        /// </summary>
        /// <param name="paramDic"></param>
        /// <param name="dic"></param>
        /// <param name="systemType"></param>
        /// <param name="system"></param>
        public static void RelfectProperty(Dictionary<string, string> paramDic, KeyValuePair<string, string> dic, Type systemType, IXSystem system)
        {
            PropertyInfo property = systemType.GetProperty(dic.Key.ToString());
            if (property != null)
            {
                Type propertyType = property.PropertyType;
                if (propertyType == typeof(int))
                {
                    property.SetValue(system, int.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (propertyType == typeof(string))
                {
                    property.SetValue(system, paramDic[dic.Value], BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (propertyType == typeof(bool))
                {
                    property.SetValue(system, bool.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (propertyType == typeof(ulong))
                {
                    property.SetValue(system, ulong.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (propertyType == typeof(double))
                {
                    property.SetValue(system, double.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (propertyType == typeof(long))
                {
                    property.SetValue(system, long.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                else if (property.Equals(typeof(Enum)))
                {
                    property.SetValue(system, int.Parse(paramDic[dic.Value]), BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
                }
                Console.Write(property.GetValue(system, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null));
            }
        }

        /// <summary>
        /// RelfectFields
        /// </summary>
        /// <param name="field"></param>
        /// <param name="system"></param>
        /// <param name="param"></param>
        /// <param name="dic"></param>
        public static void RelfectFields(FieldInfo field, IXSystem system, Dictionary<string, string> param, KeyValuePair<string, string> dic)
        {
            Type fieldType = field.FieldType;
            if (fieldType == typeof(int))
            {
                field.SetValue(system, int.Parse(param[dic.Value]));
            }
            else if (fieldType == typeof(string))
            {
                field.SetValue(system, param[dic.Value]);
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(system, bool.Parse(param[dic.Value]));
            }
            else if (fieldType == typeof(ulong))
            {
                field.SetValue(system, ulong.Parse(param[dic.Value]));
            }
            else if (fieldType == typeof(double))
            {
                field.SetValue(system, double.Parse(param[dic.Value]));
            }
            else if (fieldType == typeof(long))
            {
                field.SetValue(system, long.Parse(param[dic.Value]));
            }
        }

        /// <summary>
        ///  反射动态注入OPEN参数 ，减少OPEN 依赖if 语句判断代码
        /// </summary>
        /// <param name="system">
        ///  当前调用此函数的IXSystem 实例
        /// </param>
        /// <param name="SystemParams">
        /// Directory<string,string>
        /// Key 为当前system 里面需要的并且已经在IXSystem，或者具体实现类例如FSSystem 中定义好的接受connectorString参数的字段。
        /// Value 为key相对应的在connectorString 里面的字段
        /// </param>
        public static void HandlerSystemParams(IXSystem system, Dictionary<string, string> SystemParams)
        {
            Type systemType = system.GetType();
            PropertyInfo pro = systemType.GetProperty("X" + "RI".ToLower(CultureInfo.InvariantCulture) + "Object");
            XRI xri = (XRI)pro.GetValue(system, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
            Dictionary<string, string> paramDic = xri.Params;
            foreach (var dic in SystemParams)
            {
                FieldInfo field = null;
                if (xri.Params.ContainsKey(dic.Value))
                {
                    field = systemType.GetField(dic.Key.ToString(), BindingFlags.Public | BindingFlags.GetProperty |
                        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null)
                    {
                        RelfectProperty(paramDic, dic, systemType, system);
                    }
                    else if (field != null)
                    {
                        RelfectFields(field, system, paramDic, dic);
                    }
                }
            }
        }
    }

    public class StorageTypeUtil
    {
        private static List<string> unkownSizeDeviceList = new List<string>()
        {
            "AmazonSystem",
            "AtmosSystem",
            "AzureSystem",
            "ObjectAtmosSystem",
            "RackspaceSystem",
            "FTPSystem",
            "TSMSystem"
        };

        public static bool CanGetFreeSpace(string type)
        {
            return !unkownSizeDeviceList.Contains(type);
        }
    }

    public class OpenParameter
    {
        public Dictionary<string, string> CustomizedMetaData { get; set; }
        public int MaxRetryCount { get; set; }

        private int retryInterval = 200;

        public virtual int RetryInterval
        {
            get { return retryInterval; }
            set { retryInterval = value; }
        }

        public CustomizedMode CustomizedMetaMode { get; set; }
        public bool NeedRetry { get; set; }
        public string ProxyIp { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        private string modifyTime = string.Empty;
        public string ModifyTime
        {
            get { return modifyTime; }
            set { modifyTime = value; }
        }

        private string physicalId = string.Empty;
        public string PhysicalId
        {
            get { return physicalId; }
            set { physicalId = value; }
        }

        public string PhysicalIdAndMidifyTime
        {
            get
            {
                return physicalId + modifyTime;
            }
        }

        private long secondaryTimeout = 3600;//单位：秒

        public long SecondaryTimeout
        {
            get { return secondaryTimeout; }
            set { secondaryTimeout = value; }
        }

        private bool cacheSecondary = true;

        public bool CacheSecondary
        {
            get { return cacheSecondary; }
            set { cacheSecondary = value; }
        }

        private DateTime beginCacheSecondaryTime;

        public DateTime BeginCacheSecondaryTime
        {
            get
            {
                return this.beginCacheSecondaryTime;
            }
            set
            {
                this.beginCacheSecondaryTime = value;
            }
        }

        bool isSecondaryTimeOut;

        public bool IsSecondaryTimeOut
        {
            get
            {
                if ((SecondaryTimeout * 1000) > (DateTime.UtcNow.Ticks - beginCacheSecondaryTime.Ticks) / 10000)
                {
                    isSecondaryTimeOut = false;
                }
                else
                {
                    isSecondaryTimeOut = true;
                }
                return isSecondaryTimeOut;
            }
        }

        public OpenParameter()
        {
            CustomizedMetaMode = CustomizedMode.SupportAll;
            CustomizedMetaData = new Dictionary<string, string>();
            MaxRetryCount = 6;
        }
    }

    public enum CustomizedMode                //TODO
    {
        Close = -1,
        SupportAll = 0,
        DocAveOnly = 1,
        CustomizedOnly = 2
    }

}