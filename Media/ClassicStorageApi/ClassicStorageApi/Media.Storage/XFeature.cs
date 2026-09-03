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




namespace AvePoint.Media.ClassicStorage
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Serialization;
    #endregion

    public interface IXFeature
    {
        StorageType Type { get; set; }

        List<FeatureUnit> Features { get; set; }

    }

    public enum FeatureType
    {
        DocAveGUI = 0,
        ConnectorGUI = 1,
        SingleType = 2,
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class StorageType
    {

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Display { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public int DeviceType { get; set; }

        [DataMember]
        public List<string> Vim { get; set; }


        private List<string> defaultXris = new List<string>();

        [DataMember]
        public List<string> DefaultXris { get { return defaultXris; } set { this.defaultXris = value; } }

        private bool isSupportMovableRetention = true;
        [DataMember]
        public bool IsSupportMovableRetention { get { return isSupportMovableRetention; } set { this.isSupportMovableRetention = value; } }

        [DataMember]
        public bool SoExtenderNotSupported { get; set; }

        [DataMember]
        public bool SoArchiverNotSupported { get; set; }

        private bool isSupportCustomAction = false;
        [DataMember]
        public bool IsSupportCustomAction { get { return isSupportCustomAction; } set { this.isSupportCustomAction = value; } }

        public override string ToString()
        {
            return Display;
        }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class FeatureColor
    {
        [DataMember]
        public int A { get; set; }

        [DataMember]
        public int R { get; set; }

        [DataMember]
        public int G { get; set; }

        [DataMember]
        public int B { get; set; }

        public FeatureColor(int a, int r, int g, int b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public FeatureColor()
        {
        }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class StorageFeature : IXFeature
    {

        #region XFeature Members

        protected static readonly Dictionary<int, IXFeature> instances;
        private List<FeatureUnit> features;
        private StorageType type;
        private bool isNeedSpaceThreshold; // add for storage manager page show space threshold
        private bool isNeedSelectFarm; // add for storage manager page show select farm
        private FeatureColor progressForeground = new FeatureColor(255, 0, 0, 0); // add for storage manager page show device color



        [DataMember]
        public StorageType Type { get { return type; } set { this.type = value; } }

        [DataMember]
        public List<FeatureUnit> Features { get { return features; } set { this.features = value; } }

        [DataMember]
        public bool IsNeedSpaceThreshold { get { return isNeedSpaceThreshold; } set { this.isNeedSpaceThreshold = value; } }

        [DataMember]
        public bool IsNeedSelectFarm { get { return isNeedSelectFarm; } set { this.isNeedSelectFarm = value; } }

        [DataMember]
        public FeatureColor ProgressForeground { get { return progressForeground; } set { this.progressForeground = value; } }

        [DataMember]
        public string Description { get; set; }

        private bool isObjectType;
        [DataMember]
        public bool IsObjectType { get { return isObjectType; } set { this.isObjectType = value; } }

        private List<string> advancedOptions = new List<string>();
        [DataMember]
        public List<string> AdvancedOptions { get { return advancedOptions; } set { this.advancedOptions = value; } }

        public virtual void Init(int type, string cultureInfo = "en")
        {
            CultureInfo culture = new CultureInfo(cultureInfo);
            switch ((FeatureType)type)
            {
                case FeatureType.ConnectorGUI:
                    GenerateConnectorGUIFeatureUnit(culture);
                    break;
                case FeatureType.DocAveGUI:
                    GenerateDocAveGUIFeatureUnit(culture);
                    break;
                case FeatureType.SingleType:
                    GenerateSingleTypeFeatureUnit(culture);
                    break;
                default:
                    GenerateDocAveGUIFeatureUnit(culture);
                    break;
            }
        }
        protected virtual void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {

        }
        protected virtual void GenerateConnectorGUIFeatureUnit(CultureInfo culture)
        {

        }
        protected virtual void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {

        }

        #endregion

        public StorageFeature()
        {
        }

        private List<string> featureXMLs = new List<string>();
        public List<string> FeatureXMLs
        {
            get
            {
                ConvertObj2XML();
                return featureXMLs;
            }
        }

        protected void ConvertObj2XML()
        {
            foreach (StorageFeature sf in featureObjs)
            {
                featureXMLs.Add(FeatureUtility.Serialize(sf));
            }
        }

        private List<StorageFeature> featureObjs = new List<StorageFeature>();
        public List<StorageFeature> FeatureObjs
        {
            get { return featureObjs; }
        }

        protected void Add(StorageFeature storageFeature)
        {
            if (!featureObjs.Contains(storageFeature))
            {
                featureObjs.Add(storageFeature);
            }
        }

        public StorageFeature Instance;

    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class FeatureUnit : IConvertible
    {
        private string keyName;
        private string displayname;
        private string guiType;
        private string valType;
        private string demoVal;
        private string tag;
        private string key;
        private string vim;
        private string canNullOrEmpty = "false";
        private int featureFlag = (int)FeatureUnitFlag.None;
        private string defaultValue;
        private bool canModifi = true;
        private FeatureUnit preFeature;
        private FeatureUnit nextFeature;
        private List<FeatureUnit> childFeatures;

        private List<string> validateRegPats;

        private string value;
        private string visibility;
        private bool hasSparrow;



        [DataMember]
        public bool IsRequiredOption { get; set; }

        [DataMember]
        public string CallBackFeatureTag { get; set; }

        [DataMember]
        public string SpecificEvent { get; set; }

        [DataMember]
        public List<string> ValidateRegPats
        {
            get { return this.validateRegPats; }
            set { this.validateRegPats = value; }
        }

        [DataMember]
        public string DefaultValue
        {
            get { return defaultValue; }
            set { this.defaultValue = value; }
        }

        [DataMember]
        public bool HasSparrow
        {
            get { return hasSparrow; }
            set { this.hasSparrow = value; }
        }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public string KeyName { get { return keyName; } set { this.keyName = value; } }

        [DataMember]
        public string Value { get { return value; } set { this.value = value; } }

        [DataMember]
        public string DisplayName { get { return displayname; } set { this.displayname = value; } }

        [DataMember]
        public string GuiType { get { return this.guiType; } set { this.guiType = value; } }

        [DataMember]
        public string ValType { get { return this.valType; } set { this.valType = value; } }

        [DataMember]
        public string DemoValue { get { return this.demoVal; } set { this.demoVal = value; } }

        [DataMember]
        public FeatureUnit PreFeature { get { return this.preFeature; } set { this.preFeature = value; } }

        [DataMember]
        public FeatureUnit NextFeature { get { return this.nextFeature; } set { this.nextFeature = value; } }

        [DataMember]
        public List<FeatureUnit> ChildFeatures { get { return childFeatures; } set { this.childFeatures = value; } }

        [DataMember]
        public string Visibility { get { return visibility; } set { this.visibility = value; } }

        /// <summary>
        /// 必须项, 用于页面样分组显示
        /// </summary>
        [DataMember]
        public string Tag { get { return tag; } set { this.tag = value; } }

        [DataMember]
        public string Key { get { return key; } set { this.key = value; } }

        [DataMember]
        public string CanNullOrEmpty { get { return canNullOrEmpty; } set { this.canNullOrEmpty = value; } }

        [DataMember]
        public string DataBind { get; set; }

        /// <summary>
        /// Please refer FeatureUnitFlag for more detail.
        /// 0x00: None falg,
        /// 0x01: 标识当前FeatureUnit表示的是路径. (Connector的配置界面需要对Path进行特殊处理)
        /// 0x02: 标识当前FeatureUnit表示的是Cloud的Container. (因为在Manager端配置界面不需要显示Cloud的Container, 而Connector的配置界面需要显示)
        /// </summary>
        [DataMember]
        public int FeatureFlag { get { return featureFlag; } set { this.featureFlag = value; } }


        /// <summary>
        /// 标识当前属性是否能修改,默认是true,表示可以修改
        /// </summary>
        [DataMember]
        public bool CanModifi { get { return canModifi; } set { this.canModifi = value; } }

        /// <summary>
        /// 必须项, 用于页面元素跟Data的绑定纽带
        /// </summary>
        [DataMember]
        public string Vim { get { return vim; } set { this.vim = value; } }



        public List<FeatureUnit> ItemSources()
        {
            List<FeatureUnit> sources = new List<FeatureUnit>();
            if (childFeatures != null && childFeatures.Count > 0)
            {
                foreach (FeatureUnit child in childFeatures)
                {
                    sources.Add(child);
                }
            }
            return sources;
        }

        public FeatureUnit GetByDisplayName(string displayName)
        {
            if (childFeatures != null && childFeatures.Count > 0)
            {
                foreach (FeatureUnit unit in childFeatures)
                {
                    if (unit.displayname.Equals(displayName))
                    {
                        return unit;
                    }
                }
            }
            return null;
        }

        public override string ToString()
        {
            return this.displayname;
        }


        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }

    public class SpecificEvent
    {
        private string name;

        private SpecificEvent(string name)
        {
            this.name = name;
        }

        public static readonly SpecificEvent LoadMedias = new SpecificEvent("LoadMedias");
        public static readonly SpecificEvent LoadSysteProfiles = new SpecificEvent("LoadProfiles");
        public static readonly SpecificEvent SelectedMedia = new SpecificEvent("Selected Media for LUN");
        public static readonly SpecificEvent SelectedProfile = new SpecificEvent("Selected Profile for CIFS Share");
        public static readonly SpecificEvent SelectedLun = new SpecificEvent("Selected Lun");
        public static readonly SpecificEvent SelectedCIFSShare = new SpecificEvent("Selected CIFS Share");
        public static readonly SpecificEvent Default = new SpecificEvent("Default");
        public override string ToString()
        {
            return name.ToString();
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return name.Equals(obj);
        }

    }

    public class FeatureUtility
    {
        private static Dictionary<Type, XmlSerializer> mSerializers = new Dictionary<Type, XmlSerializer>();
        private static Queue<MemoryStream> mStreams = new Queue<MemoryStream>();
        private static XmlSerializerNamespaces NAMESPACE = new XmlSerializerNamespaces();
        private static XmlWriterSettings SETTINGS = new XmlWriterSettings();
        private const int DEFAULT_CACHE_SITE = 10;
        private static bool mIsCache = false;

        static FeatureUtility()
        {
            NAMESPACE.Add("", "");
            SETTINGS.Indent = false;
            SETTINGS.OmitXmlDeclaration = true;
            SETTINGS.Encoding = new System.Text.UTF8Encoding(false);
            SETTINGS.CheckCharacters = false;
        }

        /// <summary>
        /// Set whether use cache mode.
        /// If you set this value to true, then we will cache memory that we use.
        /// We will reuse it when we serialize or deserialize object.
        /// If you serialize or deserialize objects frequently, set this value to true.
        /// Default value is false.
        /// </summary>
        public static bool IsCache
        {
            get { return mIsCache; }
            set { mIsCache = value; }
        }

        public static string Serialize(object obj)
        {
            MemoryStream stream = null;
            try
            {
                XmlSerializer serializer;
                Type type = obj.GetType();
                if (mIsCache)
                {
                    lock (mSerializers)
                    {
                        if (!mSerializers.TryGetValue(type, out serializer))
                        {
                            serializer = new XmlSerializer(type);
                            mSerializers[type] = serializer;
                        }
                    }
                    lock (mStreams)
                    {
                        if (mStreams.Count == 0)
                        {
                            stream = new MemoryStream();
                        }
                        else
                        {
                            stream = mStreams.Dequeue();
                        }
                    }
                }
                else
                {
                    serializer = new XmlSerializer(type);
                    stream = new MemoryStream();
                }
                stream.Position = 0;
                stream.SetLength(0);
                XmlWriter writer = XmlWriter.Create(stream, SETTINGS);
                serializer.Serialize(writer, obj, NAMESPACE);
                return System.Text.Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
            catch (Exception e)
            {
                throw new Exception("Serialize object to xml string error. object:{0}", e);
            }
            finally
            {
                // We do not cache large stream
                if (mIsCache && stream != null && stream.Capacity < 2048 && mStreams.Count < DEFAULT_CACHE_SITE)
                {
                    lock (mStreams)
                    {
                        mStreams.Enqueue(stream);
                    }
                }

            }
        }

        public static T Deserialize<T>(string xml)
        {
            MemoryStream stream = null;
            Type type = typeof(T);
            try
            {
                XmlSerializer serializer;
                if (mIsCache)
                {
                    lock (mSerializers)
                    {
                        if (!mSerializers.TryGetValue(type, out serializer))
                        {
                            serializer = new XmlSerializer(type);
                            mSerializers[type] = serializer;
                        }
                    }
                    lock (mStreams)
                    {
                        if (mStreams.Count == 0)
                        {
                            stream = new MemoryStream();
                        }
                        else
                        {
                            stream = mStreams.Dequeue();
                        }
                    }
                }
                else
                {
                    serializer = new XmlSerializer(type);
                    stream = new MemoryStream();
                }
                byte[] buf = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Position = 0;
                stream.SetLength(0);
                stream.Write(buf, 0, buf.Length);
                stream.Position = 0;
                return (T)serializer.Deserialize(stream);
            }
            catch (Exception e)
            {
                throw new Exception("Deserialize xml string to object error.", e);
            }
            finally
            {
                // We do not cache large stream
                if (mIsCache && stream != null && stream.Capacity < 2048 && mStreams.Count < DEFAULT_CACHE_SITE)
                {
                    lock (mStreams)
                    {
                        mStreams.Enqueue(stream);
                    }
                }
            }
        }
    }

    public enum FeatureUnitFlag
    {
        None = 0,
        /// <summary>
        /// The feature unit is path, (for connector).
        /// </summary>
        Path = 1,
        /// <summary>
        /// The feature unit is cloud container.
        /// </summary>
        CloudContainer = 2
    }

    //public class Type
}
