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



using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;


[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#.cctor()", MessageId = "startfolder")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#BuildValidateXRI()", MessageId = "isvalidate")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#BuildXRI(AvePoint.GCommon.Contract.Storage.Entity.XRIParameter)", MessageId = "isvalidate")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#BuildXRI(AvePoint.GCommon.Contract.Storage.Entity.XRIParameter)", MessageId = "Num")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#GenerateConnectionString(AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType,System.Collections.Generic.Dictionary`2<System.String,System.String>)", MessageId = "ctype")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#GenterateCacheDevice(System.String)", MessageId = "xam")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#GenterateCacheDevice(System.String)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#GenterateFS(System.String,System.String,System.String)", MessageId = "xam")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceDto.#GenterateFS(System.String,System.String,System.String)", MessageId = "fs")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceParameterKeys.#.cctor()", MessageId = "cdn-guid")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceParameterKeys.#.cctor()", MessageId = "cdn")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceParameterKeys.#.cctor()", MessageId = "comm")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceParameterKeys.#.cctor()", MessageId = "ssi")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Contract.Storage.Entity.PhysicalDeviceParameterKeys.#.cctor()", MessageId = "overpath")]
namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDeviceDto
    {
        public PhysicalDeviceDto()
        {
            physicalDeviceSpace = -1;
            spaceType = 0;
            useSpace = -1;
            Extension = new PlysicalDeviceExtension();
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Type { get; set; }   //Storage type
        [DataMember]
        public bool IsSystemStorage { get; set; }

        //[DataMember]
        //public string StorageType { get; set; }

        /// <summary>
        /// GUI用来判断当前的Physical是否本选中，该属性只在页面中使用
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public int DeviceType { get; set; }

        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public long ModifyTime { get; set; }  //Physical device的修改时间。

        [DataMember]
        public int Status { get; set; }  //判断是否是删除的Physical device,以及是否是修改Physical device后新建立的Physical Device

        [DataMember]
        public string AccountProfileId
        {
            get
            {
                if (Extension != null)
                {
                    return Extension.AccountProfile;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (Extension != null)
                {
                    Extension.AccountProfile = value;
                }
            }
        }

        [DataMember]
        public string SystemProfileId
        {
            get
            {
                if (Extension != null)
                {
                    return Extension.SystemProfile;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (Extension != null)
                {
                    Extension.SystemProfile = value;
                }
            }
        }

        [DataMember]
        public PhysicalDeviceUsage Usage { get; set; }

        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// //由于Online和Offline的状态已经取消，因此这个属性从6.1开始已经不在使用
        /// </summary>
        [DataMember]
        public int DeviceMode { get; set; }   //online or offline type.

        [DataMember]
        public PlysicalDeviceExtension Extension { get; set; }

        [DataMember]
        public bool IsEncryptPassword { get; set; }

        [DataMember]
        public SystemProfileDto SystemProfile { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        /// <summary>
        /// 判断Connector创建Physical Device的时候是否要调用Cache的功能
        /// 考虑其他功能也会调用Cache功能，因此当IsNotSentCache=false的时候为发送cache,IsNotSentCache=true的时候为不发送Cache.
        /// </summary>
        [DataMember]
        public bool IsNotSentCache { get; set; }

        /// <summary>
        /// Logical Device前台使用
        /// </summary>
        [DataMember]
        public string MediaId { get; set; }

        /// <summary>
        /// Logical Device前台使用
        /// </summary>
        [DataMember]
        public string NetappType { get; set; }

        [DataMember]
        public List<FarmDto> FarmComboBoxSource { get; set; }

        /// <summary>
        /// Physical Details前台使用
        /// </summary>
        [DataMember]
        public List<string> FarmNames { get; set; }

        /// <summary>
        /// 判断NetApp是否选择所有的Farm
        /// </summary>
        [DataMember]
        public bool IsSelectAllFarm { get; set; }

        [DataMember]
        public List<string> FarmIds { get; set; }

        private int spaceType = 0; //for free space

        [DataMember]
        public string BackupPhysicalDeviceId { get; set; }

        [DataMember]
        public bool IsEnforcementSave { get; set; }

        [DataMember]
        public string LanguageType { get; set; }

        /// <summary>
        /// 该Device所在组，从0开始
        /// </summary>
        [DataMember]
        public int GroupNum { get; set; }

        /// <summary>
        /// 使用优先顺序从0 开始
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Agent Group ID
        /// </summary>
        [DataMember]
        public string AgentGroupId { get; set; }

        /// <summary>
        /// 用来判断device的配置信息是不是相同
        /// </summary>
        [DataMember]
        public string ConfigHashCode { get; set; }

        [DataMember]
        public int SpaceType
        {
            get
            {
                string spaceTypeValue = GetParamValue(PhysicalDeviceParameterKeys.SPACE_TYPE_KEY);
                if (null != spaceTypeValue)
                {
                    return int.Parse(spaceTypeValue);
                }
                return this.spaceType;
            }
            set
            {
                this.spaceType = value;
                AddParameter(PhysicalDeviceParameterKeys.SPACE_TYPE_KEY, value.ToString());
            }
        }

        private long physicalDeviceSpace = -1;  //for free space

        [DataMember]
        public long PhysicalDeviceSpace
        {
            get
            {
                string deviceSpace = GetParamValue(PhysicalDeviceParameterKeys.PHYSICAL_DEVICE_SPACE_KEY);
                if (null != deviceSpace)
                {
                    return long.Parse(deviceSpace);
                }
                return this.physicalDeviceSpace;
            }
            set
            {
                this.physicalDeviceSpace = value;
                AddParameter(PhysicalDeviceParameterKeys.PHYSICAL_DEVICE_SPACE_KEY, value.ToString());
            }
        }

        private float useSpace = -1;//for free space

        [DataMember]
        public float UseSpace
        {
            get
            {
                string space = GetParamValue(PhysicalDeviceParameterKeys.USE_SPACE_KEY);
                if (null != space)
                {
                    return float.Parse(space);
                }
                return this.useSpace;
            }
            set
            {
                this.useSpace = value;
                AddParameter(PhysicalDeviceParameterKeys.USE_SPACE_KEY, value.ToString());
            }
        }

        private string path; // 保存客户的配置信息如用户名密码。

        [DataMember]
        public string Path
        {
            get
            {
                return GetParamValue(PhysicalDeviceParameterKeys.OVERVIEW_PATH_KEY);
            }
            set
            {
                this.path = value;
                this.AddParameter(PhysicalDeviceParameterKeys.OVERVIEW_PATH_KEY, this.path);
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值表示PhysicalDetail返回的Check结果(前台使用)
        /// </summary>
        [DataMember]
        public PhysicalDeviceResult PhysicalDeviceCheckResult { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示PhysicalDetail是否在进行Check(前台使用)
        /// </summary>
        [DataMember]
        public bool IsCheckLoading { get; set; }

        [DataMember]
        public string SnapshotPath { get; set; }

        [DataMember]
        public Boolean NoNeedTransferData { get; set; }
        [DataMember]
        public Boolean FailedBlobBackup { get; set; }

        [DataMember]
        public Dictionary<string, string> ParamList { get; set; }  //for param        

        //<summary>
        //根据各种不同的device类型，增加不同的参数
        //</summary>
        //<param name="key">PhysicalDeviceParameterKeys中的常量</param>
        //<param name="value"></param>
        public void AddParameter(string key, string value)
        {
            if (ParamList == null)
            {
                ParamList = new Dictionary<string, string>();
            }
            if (ParamList.ContainsKey(key))
            {
                ParamList[key] = value;
            }
            else
            {
                ParamList.Add(key, value);
            }
        }

        public string GetParamValue(string key)
        {
            if (ParamList != null)
            {
                foreach (KeyValuePair<string, string> column in ParamList)
                {
                    if (key == column.Key)
                    {
                        return column.Value;
                    }
                }
            }
            return null;
        }

        public void UpdatePassword(List<string> newPassword)
        {
            for (int i = 0; i < newPassword.Count; i++)
            {
                //if (Type == 3)
                //{
                //    ConnectionString = ConnectionString.Replace("&secret=" + XRIUtil.ValueEncode(password[i]), "&secret=" + XRIUtil.ValueEncode(newPassword[i]));
                //    ConnectionString = ConnectionString.Replace("paepsecret=" + XRIUtil.ValueEncode(password[i]), "paepsecret=" + XRIUtil.ValueEncode(newPassword[i]));
                //}
                //else
                //{
                //    ConnectionString = ConnectionString.Replace("secret=" + XRIUtil.ValueEncode(password[i]), "secret=" + XRIUtil.ValueEncode(newPassword[i]));
                //}
                //ConnectionString = ConnectionString.Replace(XRIUtil.ValueEncode(password[i]), XRIUtil.ValueEncode(newPassword[i]));
                ConnectionString = ConnectionString.Replace(password[i], newPassword[i]);

            }
            password = newPassword;
        }

        public void RemovePassword()
        {
            List<string> passes = Password;
            List<string> newPasses = new List<string>();
            for (int i = 0; i < passes.Count; i++)
            {
                newPasses.Add("");
            }
            UpdatePassword(newPasses);
        }


        /// <summary>
        /// 约定 密码key以secret结尾
        /// </summary>
        private const string PASSWORD_PATTERN = "([^&]*)secret=([^&]*)";

        private static Regex r = new Regex(PASSWORD_PATTERN);
        private List<string> password;
        public List<string> Password
        {
            get
            {
                lock (r)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ConnectionString))
                        {
                            Match m = r.Match(ConnectionString);
                            if (m.Success)
                            {
                                password = new List<string>();
                            }
                            while (m.Success)
                            {
                                //password = XRIUtil.ValueDecode(m.Groups[1].Value);
                                password.Add("&" + m.Groups[0].Value);
                                m = m.NextMatch();

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message, e);
                    }
                }
                return password;
            }
        }

        /// <summary>
        /// Location 只是NetShare用来显示path用, 不适应于其他类型Device
        /// </summary>
        private const string LOCATION_PATTERN = "&{0,1}location=([^&]*)&{0,1}";
        private static Regex lr = new Regex(LOCATION_PATTERN);
        private string location;
        public string Location
        {
            get
            {
                lock (lr)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ConnectionString))
                        {
                            Match m = lr.Match(ConnectionString);

                            while (m.Success)
                            {
                                //password = XRIUtil.ValueDecode(m.Groups[1].Value);

                                location = XRIUtil.ValueDecode(m.Groups[1].Value);
                                string forder = StartFolder;
                                if (location != null && location != string.Empty && forder != null && forder != string.Empty)
                                {
                                    if (forder.StartsWith("\\", StringComparison.OrdinalIgnoreCase) || forder.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                                    {
                                        forder = forder.TrimStart(new char[] { '\\', '/' });
                                    }
                                    location = System.IO.Path.Combine(location, forder);
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message, e);
                    }
                }
                return location;
            }
        }

        /// <summary>
        /// Startfolder 只是connector用来显示location + Startfolder用, 不适应于其他模块
        /// </summary>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Startfolder 只是connector用来显示location + Startfolder用, 不适应于其他模块")]
        private const string START_FOLDER_PATTERN = "&{0,1}startfolder=([^&]*)&{0,1}";
        private static Regex sf = new Regex(START_FOLDER_PATTERN);
        private string startFolder;
        public string StartFolder
        {
            get
            {
                lock (sf)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(ConnectionString))
                        {
                            Match m = sf.Match(ConnectionString);

                            while (m.Success)
                            {
                                //password = XRIUtil.ValueDecode(m.Groups[1].Value);
                                startFolder = XRIUtil.ValueDecode(m.Groups[1].Value);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message, e);
                    }
                }
                return startFolder;
            }
        }

        public string BuildXRI()
        {
            return BuildXRI(new XRIParameter());
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "-isvalidate is unmodifiable.")]
        public string BuildXRI(XRIParameter param)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ConnectionString);
            if (!sb.ToString().Contains("&id="))
            {
                sb.Append("&id=");
                sb.Append(Id);
            }
            //add space param
            if (SpaceType == 0 && PhysicalDeviceSpace > 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLowerInvariant());
                    sb.Append(1);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThreshold=".ToLowerInvariant());
                    sb.Append(PhysicalDeviceSpace);
                }
            }
            else if (SpaceType == 1 && UseSpace > 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLowerInvariant());
                    sb.Append(2);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThreshold=".ToLowerInvariant());
                    sb.Append(UseSpace);
                }
            }
            if (!sb.ToString().Contains("&modifyTime=".ToLowerInvariant()))
            {
                sb.Append("&modifyTime=".ToLowerInvariant());
                sb.Append(ModifyTime);
            }
            //add extend param
            if (param.IsCreation && !sb.ToString().Contains("&creation="))
            {
                sb.Append("&creation=true");
            }
            if (param.IsValidate && !sb.ToString().Contains("&isvalidate="))
            {
                sb.Append("&isvalidate=true");

            }
            if (!sb.ToString().Contains("&culture=") && !string.IsNullOrEmpty(param.Culture))
            {
                sb.Append("&culture=");
                sb.Append(param.Culture);
            }
            //add groupNum, order param
            if (!sb.ToString().Contains("&groupNum=".ToLowerInvariant()))
            {
                sb.Append("&groupNum=".ToLowerInvariant());
                sb.Append(GroupNum);
            }
            if (!sb.ToString().Contains("&order="))
            {
                sb.Append("&order=");
                sb.Append(Order);
            }
            return sb.ToString();
        }

        [Obsolete]
        public string BuildXRIWithoutCreationIfNotExists()
        {
            return BuildXRI(false);
        }

        [Obsolete]
        private string BuildXRI(bool creation)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ConnectionString);
            if (!sb.ToString().Contains("&id="))
            {
                sb.Append("&id=");
                sb.Append(Id);
            }
            //add space param
            if (SpaceType == 0 && PhysicalDeviceSpace >= 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLowerInvariant());
                    sb.Append(1);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThreshold=".ToLowerInvariant());
                    sb.Append(PhysicalDeviceSpace);
                }
            }
            else if (SpaceType == 1 && UseSpace >= 0)
            {
                if (!sb.ToString().Contains("&spaceThresholdUnit=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThresholdUnit=".ToLowerInvariant());
                    sb.Append(2);
                }
                if (!sb.ToString().Contains("&spaceThreshold=".ToLowerInvariant()))
                {
                    sb.Append("&spaceThreshold=".ToLowerInvariant());
                    sb.Append(UseSpace);
                }
            }
            if (!sb.ToString().Contains("&modifyTime=".ToLowerInvariant()))
            {
                sb.Append("&modifyTime=".ToLowerInvariant());
                sb.Append(ModifyTime);
            }
            if (!sb.ToString().Contains("&creation="))
            {
                sb.Append("&creation=");
                sb.Append(creation);
            }
            return sb.ToString();
        }

        [Obsolete]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "BuildValidateXRI is obsolete.")]
        public string BuildValidateXRI()
        {
            string xri = BuildXRI(true);
            if (!xri.Contains("&isvalidate="))
            {
                xri += "&isvalidate=true";
            }
            return xri;
        }

        public bool IsSnapLock
        {
            get
            {
                if (ConnectionString.ToLowerInvariant().Contains(PhysicalDeviceParameterKeys.IS_SnapLock_KEY))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsLUN
        {
            get
            {
                if (ConnectionString.ToLowerInvariant().Contains(PhysicalDeviceParameterKeys.IS_LUN_KEY))
                {
                    return true;
                }
                return false;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "-xam is unmodifiable.")]
        public static PhysicalDeviceDto GenterateCacheDevice(string path)
        {
            PhysicalDeviceDto pd = new PhysicalDeviceDto();
            pd.Id = "pd id:" + Guid.NewGuid().ToString();
            pd.Name = "DocAve-Cache-" + new Random().Next(0, 1000);
            pd.Type = 0;
            pd.ConnectionString = "DocAve".ToLowerInvariant() + "-xam://fs_vim?location=" + XRIUtil.ValueEncode(path) + "&" + PARAM_KEY_CACHE + "=true";
            pd.Usage = PhysicalDeviceUsage.All;
            pd.DeviceMode = (int)PhysicalDeviceStatus.Online;
            //TO DO
            pd.SpaceType = 0;
            pd.PhysicalDeviceSpace = 1024;
            return pd;
        }

        /// <summary>
        /// 有些模块需要能够快速生成FS的实例，可以使用这个方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="extendedParameters">Advanced</param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "-xam is unmodifiable.")]
        public static PhysicalDeviceDto GenterateFS(string path, string username, string password, string[] extendedParameters = null)
        {
            PhysicalDeviceDto pd = new PhysicalDeviceDto();
            pd.Id = "pd id:" + Guid.NewGuid().ToString();
            pd.Name = "DocAve-Cache-" + new Random().Next(0, 1000);
            pd.Type = 0;
            pd.ConnectionString = "DocAve".ToLowerInvariant() + "-xam://fs_vim?location=" + XRIUtil.ValueEncode(path) + "&name=" + XRIUtil.ValueEncode(username) + "&secret=" + XRIUtil.ValueEncode(password);
            //拼装Advanced参数到ConnectionString里,  具体参数见 FSFeature.cs
            if (extendedParameters != null && extendedParameters.Length > 0)
            {
                StringBuilder buffer = new StringBuilder();
                bool isFirst = true;
                foreach(string param in extendedParameters)
                {
                    if (!isFirst)
                    {
                        buffer.Append('&');
                    }
                    buffer.Append(param);
                    isFirst = false;
                }
                string temp = "&extendedparameters=" + buffer.ToString().Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
                temp += "&advanced=True";
                pd.ConnectionString += temp;
            }
            pd.Usage = PhysicalDeviceUsage.All;
            pd.DeviceMode = (int)PhysicalDeviceStatus.Online;
            return pd;
        }

        public static PhysicalDeviceDto GenterateSFTP(string host, int port, string rootFolder, string username, string password, string privateKey, string privateKeyPassword)
        {
            PhysicalDeviceDto pd = new PhysicalDeviceDto();
            pd.Id = "pd id:" + Guid.NewGuid().ToString();
            pd.Name = "DocAve-Cache-" + new Random().Next(0, 1000);
            pd.ConnectionString = CreateXri(host, port, rootFolder, username, password, privateKey, privateKeyPassword);
            pd.Usage = PhysicalDeviceUsage.All;
            pd.DeviceMode = (int)PhysicalDeviceStatus.Online;
            return pd;
        }

        public static PhysicalDeviceDto GenterateFTP(string host, int port, string username, string password)
        {
            PhysicalDeviceDto pd = new PhysicalDeviceDto();
            pd.Id = "pd id:" + Guid.NewGuid().ToString();
            pd.Name = "DocAve-Cache-" + new Random().Next(0, 1000);
            pd.Type = 1;
            pd.ConnectionString = string.Format("DocAve".ToLowerInvariant() + "-xam://ftp_vim?host={0}&port={1}&name={2}&secret={3}", XRIUtil.ValueEncode(host), port, XRIUtil.ValueEncode(username), XRIUtil.ValueEncode(password));
            pd.Usage = PhysicalDeviceUsage.All;
            pd.DeviceMode = (int)PhysicalDeviceStatus.Online;
            return pd;
        }

        public static string CreateXri(string host, int port, string rootFolder, string username, string password, string privateKey, string privateKeyPassword)
        {
            var xri = string.Empty;
            xri = string.Format("DocAve".ToLowerInvariant() + "-xam://sftp_vim?host={0}&port={1}&name={2}", XRIUtil.ValueEncode(host), port, XRIUtil.ValueEncode(username));
            if (!String.IsNullOrEmpty(password))
            {
                xri = xri + "&secret=" + XRIUtil.ValueEncode(password);
            }
            if (!String.IsNullOrEmpty(rootFolder))
            {
                xri = xri + "&sftprootfolder=" + XRIUtil.ValueEncode(rootFolder);
            }
            else
            {
                xri = xri + "&sftprootfolder=root";
            }
            if (!String.IsNullOrEmpty(privateKey))
            {
                xri = xri + "&privatekeysecret=" + XRIUtil.ValueEncode(privateKey);
            }
            if (!String.IsNullOrEmpty(privateKeyPassword))
            {
                xri = xri + "&privatekeypasswordsecret=" + XRIUtil.ValueEncode(privateKeyPassword);
            }
            return xri;
        }

        private static string PARAM_KEY_CACHE = "cache";

        public class XRIUtil
        {

            public static string ValueEncode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D").Replace("^", "%5e");
                //return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D");
            }

            public static string ValueDecode(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%").Replace("%5e", "^");
                //return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%");
            }

        }

        //用来直接生成connecting string，目前仅支持Azure,add for SARS
        public static string GenerateConnectionString(StorageDeviceType deviceType, Dictionary<string, string> parameters)
        {
            StringBuilder xriStr = new StringBuilder();
            switch (deviceType)
            {
                case StorageDeviceType.CloudAzure:
                    //docave-xam://azure_vim?accesspoint=http://blob.core.windows.net&cdned=False&ctype=Azure&containername=docave&name=devteststorage&secret=wakneBY0ptJsy9hDnu/Xo9B6Vtguv7hFQXjDNeybS9V3W4iE8uzXIlgAiCmAn4MWIG/saemvJiLmn4qQLWtdvG6GMqSuKFB7lch+6KHFVta6P9NzrqZx6MOHsidypHjaAJe6Y40QeUVxFFbXHuvD8w%3D%3D
                    xriStr.Append(PhysicalDeviceParameterKeys.MEDIASTORAGE_PROTOCOL).Append(PhysicalDeviceParameterKeys.AZURE_VIM).Append("?ctype=Azure");
                    break;
                default:
                    throw new NotImplementedException("Unsupported device type:" + deviceType);
            }
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                xriStr.Append("&").Append(parameter.Key).Append("=").Append(parameter.Value);
            }
            return xriStr.ToString();
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlysicalDeviceExtension
    {
        [DataMember]
        public long UsedSpace { get; set; }

        [DataMember]
        public long TotalSpace { get; set; }

        [DataMember]
        public string AccountProfile { get; set; }

        [DataMember]
        public string SystemProfile { get; set; }
    }

    /// <summary>
    /// 定义Physical Device所支持的参数Key
    /// </summary>
    /// 
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "PhysicalDeviceParameterKeys is unmodifiable as the cause of being referenced.")]
    public class PhysicalDeviceParameterKeys // same as XRIParameterKeys class
    {

        public static readonly string DEVICE_ID_KEY = "id";//用于表示device的Id(physical device id)
        public static readonly string DEVICE_NAME_KEY = "name";// 用于表示device的display name(physical device name)
        public static readonly string DEVICE_STATUS_KEY = "status";//用于表示device是online还是offline
        public static readonly string DEVICE_USAGE_KEY = "usage"; //用于表示device用户，存放all/data/index

        public static readonly string USERNAME_KEY = "name";//用于表示用于各种device用于验证的"用户名"
        public static readonly string PASSWORD_KEY = "secret";//用于表示用于各种device用于验证的"密码"， 这里的值value必须是秘文

        public static readonly string SPACE_THRESHOLD_KEY = "spaceThreshold";//保留空间大小，如果剩余空闲空间小于这个值，那么不能再向这个device写数据，当做磁盘满处理
        public static readonly string SPACE_THRESHOLD_UNIT_KEY = "spaceThresholdUnit";//保留空间单位，可能是MB,也可能是%

        public static readonly string OVERVIEW_PATH_KEY = "overpath";//保存overview时显示的path下的内容

        public static readonly string CACHE_KEY = "cache";//用于表示当前的device system是不是用来做Cache用的.

        public static readonly string MEDIASTORAGE_PROTOCOL = "DOCAVE-XAM://".ToLowerInvariant();//connecting string的固定前缀

        /**************************For Vim Tpye*********************************/
        public const string FS_VIM = "fs_vim";
        public const string FTP_VIM = "ftp_vim";
        public const string CENTERA_VIM = "centera_vim";
        public const string TSM_VIM = "tsm_vim";
        public const string RACKSPACE_VIM = "rackspace_vim";
        public const string ATMOS_VIM = "atmos_vim";
        public const string AT_T_VIM = "at&t_vim";
        public const string AZURE_VIM = "azure_vim";
        public const string AMAZON_VIM = "amazon_vim";
        /**************************For Vim Tpye**********************************/

        /**************************For Cloud*********************************/
        public static readonly string CLOUD_CDN_KEY = "cdn";
        public static readonly string CLOUD_CDN_GUID_KEY = "cdn-guid";
        public static readonly string CLOUD_CUSTOM_DOMAIN_KEY = "custom-domain";
        public static readonly string CLOUD_REGION_KEY = "region";
        public static readonly string CLOUD_TYPE_KEY = "cType";
        public static readonly string CLOUD_OFFLINE_HOST = "cloud_offline_host";
        /**************************For Cloud*********************************/


        /**************************For TSM*********************************/
        public static readonly string DSM_NODE_NAME = "node";
        public static readonly string DSM_NODE_PWD = "secret";
        public static readonly string DSM_MC = "managementClass";
        public static readonly string DSM_COMMMETHOD = "commMethod";
        public static readonly string DSM_PORT = "port";
        /**************************For TSM*********************************/

        /**************************For FS*********************************/
        public static readonly string FS_LOCATION_KEY = "location";
        public static readonly string FS_DEVICE_TYPE_TYPE = "deviceType";
        /**************************For FS*********************************/

        /**************************For FTP*********************************/
        public static readonly string HOST_KEY = "host";
        public static readonly string PORT_KEY = "port";
        /**************************For FTP*********************************/

        /**************************For EMC*********************************/
        public const string EMC_AUTHENTICATION_TYPE_KEY = "authType";

        public const string EMC_ADDRESS_KEY = "address";

        public const string EMC_AUTHENTICATION_NAME_SECRET = "n/sAuth";
        //not key just value
        public const string EMC_AUTHENTICATION_PROFILES_SECRET = "pea";

        public const string EMC_AUTHENTICATION_NAME_KEY = "name";

        public const string EMC_AUTHENTICATION_SECRET_KEY = "secret";

        public const string EMC_AUTHENTICATION_PROFILES_KEY = "profile";

        public const string EMC_AUTHENTICATION_PROFILES_NAME_KEY = "username";

        public const string EMC_AUTHENTICATION_PROFILES_PASSWORD_KEY = "password";
        /**************************For EMC*********************************/

        /**************************For NetApp*********************************/
        public static readonly string IS_LUN_KEY = "NetAppType=LUN".ToLowerInvariant();
        public static readonly string IS_SnapLock_KEY = "SnapLock=true".ToLowerInvariant();
        /**************************For NetApp*********************************/

        /**************************For NetApp ONTAP*********************************/
        public static readonly string STORAGE_SYSTEM_KEY = "storageSystem";
        public static readonly string CONNECTION_TYPE_KEY = "connectionType";
        public static readonly string STORAGE_SYSTEM_USERNAME = "ssiUsername";
        public static readonly string STORAGE_SYSTEM_PASSWORD = "ssiPassword";
        /**************************For NetApp ONTAP*********************************/

        /**************************Space Threshold*********************************/
        public static readonly string SPACE_TYPE_KEY = "spaceType";
        public static readonly string PHYSICAL_DEVICE_SPACE_KEY = "physicalDeviceSpace";
        public static readonly string USE_SPACE_KEY = "useSpace";
        /**************************Space Threshold*********************************/
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalDeviceSpaceThresholdUnit
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        MB,
        [EnumMember]
        PERCENT
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalDeviceStatus
    {
        [EnumMember]
        Online,
        [EnumMember]
        Offline,
        [EnumMember]
        AutomaticOffline,
        [EnumMember]
        Unknown,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalDeviceUsage
    {
        [EnumMember]
        All,
        [EnumMember]
        Data,
        [EnumMember]
        Index
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StorageDeviceType
    {
        [EnumMember]
        NetShare = 0,
        [EnumMember]
        FTP = 1,
        [EnumMember]
        TSM = 2,
        [EnumMember]
        EMCCentera = 3,
        [EnumMember]
        Cloud = 4,
        //Comm Cloud = 4, 4 * 100 + N
        [EnumMember]
        CloudAmazon = 401,
        [EnumMember]
        CloudRackspace = 402,
        [EnumMember]
        CloudAzure = 403,
        [EnumMember]
        CloudAtmos = 404,
        [EnumMember]
        CloudAT_TSynaptic = 405,
        [EnumMember]
        HCP = 406,
        [EnumMember]
        DropBox = 407,
        [EnumMember]
        Egnyte = 409,
        [EnumMember]
        CloudS3Compatible = 410,
        [EnumMember]
        DELL = 5,
        [EnumMember]
        NetApp = 7,
        [EnumMember]
        NetApp_LUN = 701,
        [EnumMember]
        NetApp_CIFS = 702,
        [EnumMember]
        Caringo = 8,
        [EnumMember]
        Box = 9,
        [EnumMember]
        GoogleDrive = 10,
        [EnumMember]
        SkyDrive = 11,
        [EnumMember]
        IBMStorwizeFamily = 12,
        [EnumMember]
        NFS = 13,
        [EnumMember]
        WMS = 14,
        [EnumMember]
        OpenStack = 501,
        [EnumMember]
        IBMElasticStorage = 502,
        [EnumMember]
        Cleversafe = 601
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDeviceLicenseResult
    {
        [DataMember]
        public PhysicalDeviceLicense PDLicenseState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalDeviceLicense : int
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        Docave = 1,
        [EnumMember]
        NetApp = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDeviceResult
    {
        [DataMember]
        public bool IsOnline { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool SpaceComputeable { get; set; }
        [DataMember]
        public long FreeSpace { get; set; }
        [DataMember]
        public long TotalSpace { get; set; }
        [DataMember]
        public ServiceDto Service { get; set; }
        [DataMember]
        public int SystemHealth { get; set; }
        [DataMember]
        public List<string> ErrorMessages { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class XRIParameter
    {
        [DataMember]
        public bool IsCreation { get; set; }

        [DataMember]
        public bool IsValidate { get; set; }

        [DataMember]
        public string Culture { get; set; }

        public XRIParameter()
        {
            IsCreation = true;
        }
    }
}
