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
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using Storage;
using System.Diagnostics;
using AvePoint.GCommon.Utility.I18N;
using Storage.Util;
using AvePoint.GCommon.Utility;

namespace AvePoint.Media.StorageApi
{



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

        public static UNCObj ValueOf(ConnectionBuilder uncXRIObj)
        {
            if (string.IsNullOrEmpty(uncXRIObj["name"]) && string.IsNullOrEmpty(uncXRIObj["secret"]))
            {
                foreach(KeyValuePair<string, string> pair in uncXRIObj.Params)
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
                uncObj.decryptedPassword = SecretUtil.Decrypt(uncXRIObj["secret"]);
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
                uncObj.decryptedPassword = SecretUtil.Decrypt(uncXRIObj["secret"]);
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
            ConnectionBuilder uncXRIObj = ConnectionBuilder.ValueOf(uncConnectString);
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
                using var ms = new MemoryStream();
                var buffer = new byte[1024];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                String content = encoding.GetString(ms.ToArray());
                return content;
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
                using var ms = new MemoryStream();
                var buffer = new byte[1024];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                String content = encoding.GetString(ms.ToArray());
                return content;
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
                using var ms = new MemoryStream();
                var buffer = new byte[1024];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                String content = encoding.GetString(ms.ToArray());
                return content;
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


    /// <summary>
    /// try to acquire zhe designated netshare forder absolute Path,
    /// if an exception occurs in runtime will return zhe relative path.
    /// </summary>
    public class PathUtil
    {

        public static IntPtr ptr = IntPtr.Zero;

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
                return HttpUtility.UrlDecode(SecurityUtils.SafeCombinePath(firstPath, secondPath));
            }
            else
            {
                return SecurityUtils.SafeCombinePath(firstPath, secondPath);
            }
        }
    }

    /// <summary>
    /// 这里定义的storage api的所有常量, 针对具体device的常量可继承相关的类, e.g. FSSystemConst.
    /// </summary>
    public class XConst
    {
        public const string UTF_8 = "UTF-8";
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
    public enum CustomizedMode                //TODO
    {
        Close = -1,
        SupportAll = 0,
        DocAveOnly = 1,
        CustomizedOnly = 2
    }

    public enum XSystemValidateStatus
    {
        UnAvailableDeviceExist,
        Available
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
            ConnectionBuilder xri = (ConnectionBuilder)pro.GetValue(system, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
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

    public class AdvancedOptionUtil
    {
        public void AssembleAdvancedOption(Dictionary<string, string> param, string extendedParameters)
        {
            string extendedParams = extendedParameters.Replace("%3D", "=");
            Regex regex = new Regex("([^=\r\n]+)=([^\r\n]+)");
            MatchCollection ms = regex.Matches(extendedParams);
            foreach (Match m in ms)
            {
                string key = m.Groups[1].Value.ToLower(CultureInfo.InvariantCulture).Trim();
                string value = m.Groups[2].Value.Trim();
                if (!param.ContainsKey(key))
                {
                    param[key] = value;
                }
            }
        }

        //{[test1,test1],[test2,test2],[test3,tests3]}    \\[([^,]+),([^\\]]+)
        public Dictionary<string, string> ParseCustomizedMetaData(string metaData)
        {
            Dictionary<string, string> customizedMetaDatas = new Dictionary<string, string>();
            Regex regex = new Regex("\\[([^,]+),([^\\]]+)");
            MatchCollection ms = regex.Matches(metaData);
            foreach (Match m in ms)
            {
                string key = m.Groups[1].Value;
                string value = m.Groups[2].Value;
                if (!customizedMetaDatas.ContainsKey(key))
                {
                    customizedMetaDatas[key] = value;
                }
            }
            return customizedMetaDatas;
        }
    }
}
