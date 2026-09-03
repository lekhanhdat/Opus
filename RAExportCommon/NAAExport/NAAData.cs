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
using AvePoint.GCommon;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using LOGRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;

namespace RAExportCommon
{
    public class NAAData
    {
        public NAAData()
        { }

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<NAAMetaInfo> mConfigData = null;

        public static List<NAAMetaInfo> NAAConfigCache
        {
            get
            {
                return mConfigData;
            }
        }

        public static List<NAAMetaInfo> InitConfig(byte[] NAAConfigFile)
        {
            try
            {
                if (mConfigData == null)
                {
                    mConfigData = new List<NAAMetaInfo>();
                    using (MemoryStream configStream = new MemoryStream(NAAConfigFile))
                    {
                        NAAConfig mConfig = (NAAConfig)new XmlSerializer(typeof(NAAConfig)).Deserialize(configStream);
                        mConfigData = mConfig.MetaInfos;
                    }
                }
            }
            catch (Exception ex)
            {
                mConfigData = null;
                mLog.Error("Init NAA Config faild,ERROR:{0}", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_NAAExportConfigFileDeserializeException");
            }
            return mConfigData;
        }
        public static List<NAAMetaInfo> InitConfig()
        {
            try
            {
                if (mConfigData == null)
                {
                    string configLocation = string.Empty; //VaultConfigFileInfo.NAAConfigurationFileFullPath;
                    mConfigData = new List<NAAMetaInfo>();
                    if (!File.Exists(configLocation))
                    {
                        mConfigData = null;
                        mLog.Warn("An error occurred while loading the naa configuration file, file not found.");
                        return mConfigData;
                        //CreateByDefault(configLocation);
                    }
                    using (Stream sm = File.Open(configLocation, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(sm, Encoding.UTF8))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(NAAConfig));
                            object obj = serializer.Deserialize(sr);
                            if (obj != null)
                            {
                                NAAConfig naaConfig = obj as NAAConfig;
                                mConfigData = naaConfig.MetaInfos;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mConfigData = null;
                mLog.Error("Init Config faild,ERROR:{0}", ex.ToString());
            }
            return mConfigData;
        }
        public static void CreateByDefault(string location)
        {

            NAAConfig config = new NAAConfig()
            {
                MetaInfos = new List<NAAMetaInfo>()
                {
                    new NAAMetaInfo() { DisplayName = "Series Number", MappedKey = "ID", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Box Barcode Number", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Container Type", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Barcode No", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Control Symbol", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Alternative Control Symbol", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Title", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "Contents Start Date", MappedKey = "", AdditionalMetadata = false, DateFormat = "dd/MM/yyyy" },
                    new NAAMetaInfo() { DisplayName = "Contents End Date", MappedKey = "", AdditionalMetadata = false, DateFormat = "dd/MM/yyyy" },
                    new NAAMetaInfo() { DisplayName = "Disposal Class", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "File path on transfer carrier media", MappedKey = "", AdditionalMetadata = false },
                    new NAAMetaInfo() { DisplayName = "URL", MappedKey = "Encoded Absolute URL", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Unique Identifier", MappedKey = "Unique Id", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Created", MappedKey = "Created", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Created By", MappedKey = "Created By", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Name", MappedKey = "Name", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Format", MappedKey = "File Type", AdditionalMetadata = true },
                    new NAAMetaInfo() { DisplayName = "Protective Marking Security / Classification", MappedKey = "", AdditionalMetadata = true },
                }
            };
            string xmlConfig = config.ToXmlString();
            using (FileStream fs = File.Create(location))
            {
                byte[] content = Encoding.UTF8.GetBytes(xmlConfig);
                fs.Write(content, 0, content.Length);
            }
        }
    }

    public class NAAColumnContainer
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public const string CSVLINK = "=HYPERLINK(\"{0}\",\"{1}\")";
        private const char SLASH = '/';
        private const string HEADERFORMAT = "Additional Metadata<{0}>";


        private string _revIMColumnName = string.Empty;
        private List<NAAFieldInfo> mFieldInfos = null;
        private List<string> mHeaderInfos = null;
        private string webAppUrl = string.Empty;
        private IAveTaxonomySession taxSession;

        public string RevIMColumnName
        {
            get
            {
                return _revIMColumnName;
            }
            set
            {
                _revIMColumnName = value;
            }
        }

        public NAAColumnContainer(AveSPSite aveSite)
        {
            Init();
            taxSession = aveSite.SPSite.AveSPTaxonomySession;
            webAppUrl = GetWebappUrl(aveSite);
        }

        public void Init()
        {
            try
            {
                if (mFieldInfos == null || mHeaderInfos == null)
                {
                    mFieldInfos = new List<NAAFieldInfo>();
                    mHeaderInfos = new List<string>();
                    if (NAAData.NAAConfigCache == null)
                    {
                        mLog.Error("naa config file init faild.");
                        throw new Exception(LOGRESOURCE.StorageOptimization_SOARSOVaultBefArFailedError);
                    }
                    foreach (var cfgItem in NAAData.NAAConfigCache)
                    {
                        string mappedName = cfgItem.MappedKey;
                        if (!string.IsNullOrEmpty(cfgItem.DisplayName))
                        {
                            string displayName = cfgItem.AdditionalMetadata ? string.Format(HEADERFORMAT, cfgItem.DisplayName) : cfgItem.DisplayName;

                            mFieldInfos.Add(new NAAFieldInfo()
                            {
                                MappedKey = cfgItem.MappedKey.Trim(),
                                DisplayName = displayName,
                                DateFormat = cfgItem.DateFormat,
                                Prefix = cfgItem.Prefix,
                                DefaultValue = cfgItem.DefaultValue,
                            });
                            mHeaderInfos.Add(displayName);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                mLog.Error("error occurred while init naa config,ERROR:{0}", ex.ToString());
                throw;
            }
        }

        public void UpdateColumnInfo(AveSPList aveList)
        {
            IAveFieldCollection fieldCollection = aveList.SPList.Fields;
            foreach (var f in mFieldInfos)
            {
                if (fieldCollection.ContainsField(f.MappedKey))
                {
                    try
                    {
                        f.InternalName = fieldCollection[f.MappedKey].InternalName;
                    }
                    catch (Exception ex)
                    {
                        f.InternalName = string.Empty;
                        mLog.Info("Can not get InternalName in NAA UpdateColumnInfo.Message:{0}.", ex.ToString());
                    }
                    
                }
                else
                {
                    mLog.Warn("naa field not found,MappedSPName:{0},displayName:{1},WebUrl:{2}", f.MappedKey, f.DisplayName, aveList.ParentSite.SPSite.Url);
                }
            }


        }

        public List<MetaDataItemInfo> GetCSVColumnHeadList()
        {
            List<MetaDataItemInfo> heads = new List<MetaDataItemInfo>();
            foreach (string title in mHeaderInfos)
            {
                MetaDataItemInfo info = new MetaDataItemInfo();
                info = new MetaDataItemInfo(title, title, true, title.GetType());
                heads.Add(info);
            }
            return heads;
        }

        public List<MetaDataItemInfo> GetCSVListFromColumnValue(AveSPDoc aveDoc, string disposalClass, string filePath, string exportFileName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("NAAExport_AveSPDocGetCSVListFromColumnValue"))
            {
                var item = aveDoc.AveSPItem;
                Dictionary<string, object> values = item.GetColumnValues();
                AddItemPropertyDiagnoseLog(values);
                List<MetaDataItemInfo> columnValue = new List<MetaDataItemInfo>();
                MetaDataItemInfo info = new MetaDataItemInfo();
                foreach (var fieldInfo in mFieldInfos)
                {
                    string displayName = fieldInfo.DisplayName;
                    string mappedKey = fieldInfo.MappedKey;
                    object value = null;
                    try
                    {
                        if (displayName.Equals("Additional Metadata<Keywords>", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(mappedKey))
                        {
                            #region Keywords
                            string keywordsValue = string.Empty;
                            //site title
                            string siteTtile = item.AveSPList.ParentWeb.SPWeb.Title;
                            //term set value
                            string termSetValue = string.Empty;
                            string bcsTerm = item.SPListItem[RevIMColumnName] == null ? string.Empty : item.SPListItem[RevIMColumnName].ToString();
                            if (bcsTerm.Contains("|"))
                            {
                                string[] tempTerm = bcsTerm.Split('|');
                                termSetValue = tempTerm[0];
                            }
                            //File Name
                            string fileName = string.Empty;
                            fileName = string.IsNullOrEmpty(values["FileLeafRef"].ToString()) ? string.Empty : values["FileLeafRef"].ToString();
                            if (item.SPListItem.File.UIVersion != item.Version)
                            {
                                string fileVersion = item.Version / 512 + "." + item.Version % 512;
                                fileName = fileName + ":" + fileVersion;
                            }
                            //Guid
                            //string fileGuid = string.Empty;
                            //fileGuid = values["GUID"].ToString();
                            keywordsValue = siteTtile + "-" + termSetValue + "-" + fileName;
                            info = new MetaDataItemInfo(displayName, keywordsValue, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                            #endregion
                        }
                        else if (string.IsNullOrEmpty(mappedKey))
                        {
                            string val = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                            info = new MetaDataItemInfo(displayName, val, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                        }
                        bool useFormat = !string.IsNullOrEmpty(fieldInfo.DateFormat);
                        #region get value logic
                        switch (mappedKey)
                        {
                            case "URL":
                                string url = webAppUrl + values["FileRef"].ToString();
                                info = new MetaDataItemInfo(displayName, url, true, typeof(string));
                                //mLog.Info("GetCSVListFromColumnValue URL:{0}, FileRef:{1}, FileName:{2}, FileURL:{3}, FileServerURL:{4}, ListItemRef:{5}.", url, values["FileRef"].ToString(), aveDoc.AveSPItem.SPListItem.File.Name, aveDoc.AveSPItem.SPListItem.File.Url, aveDoc.AveSPItem.SPListItem.File.ServerRelativeUrl, aveDoc.AveSPItem.SPListItem["FileRef"].ToString());
                                columnValue.Add(info);
                                break;
                            case "GUID":
                                info = new MetaDataItemInfo(displayName, values["GUID"].ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "FilePath":
                                string path = string.IsNullOrEmpty(fieldInfo.Prefix) ? filePath : Path.Combine(fieldInfo.Prefix, filePath);
                                info = new MetaDataItemInfo(displayName, path, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "ExportFileName":
                                info = new MetaDataItemInfo(displayName, exportFileName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Checksum":
                                #region Checksum
                                string checkSum = string.Empty;
                                //using (var md5 = MD5.Create())
                                //{
                                //    var hash = md5.ComputeHash(item.SPListItem.File.OpenBinaryStream());
                                //    checkSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                                //}
                                
                                //using (Stream fileStream = item.SPListItem.File.OpenBinaryStream())
                                using (Stream fileStream = aveDoc.AveSPItem.GetContent())
                                {
                                    byte[] bytes = new byte[fileStream.Length];
                                    using (var ms = new MemoryStream())
                                    {
                                        int read;
                                        while ((read = fileStream.Read(bytes, 0, bytes.Length)) > 0)
                                        {
                                            ms.Write(bytes, 0, read);
                                        }
                                        checkSum = HashCodeMd5Helper.HashCodeMD5(ms.ToArray());
                                    }
                                }
                                info = new MetaDataItemInfo(displayName, checkSum, true, typeof(string));
                                columnValue.Add(info);
                                break;
                                #endregion
                            case "Disposal Class":
                            case "Disposal class":
                            case "Disposition Authority":
                                #region disposal class logic.
                                //string disposalClass = string.Empty;
                                //var bcsTerm = item.SPListItem[RevIMColumnName] == null ? string.Empty : item.SPListItem[RevIMColumnName].ToString();
                                //if (bcsTerm.Contains("|"))
                                //{
                                //    string[] tempTerm = bcsTerm.Split('|');
                                //    var termId = tempTerm[1];
                                //    var term = taxSession.GetTerm(Guid.Parse(termId));
                                //    term.CustomProperties.TryGetValue(mappedKey, out disposalClass);
                                //    disposalClass = string.IsNullOrEmpty(disposalClass) ? string.Empty : disposalClass;
                                //    mLog.Debug("Get CSV List From ColumnValue (Disposal Class) termID:{0},revIMColumnName:{1},disposalValue:{2},mappedKey:{3}", termId, RevIMColumnName, disposalClass, mappedKey);
                                //}
                                //string val = string.IsNullOrEmpty(disposalClass) && !string.IsNullOrEmpty(fieldInfo.DefaultValue) ? fieldInfo.DefaultValue : disposalClass;
                                //info = new MetaDataItemInfo(displayName, val, typeof(string));
                                //columnValue.Add(info);
                                info = new MetaDataItemInfo(displayName, disposalClass, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Content Type":
                                var contenttypeId = values["ContentTypeId"].ToString();
                                var contenttypeName = aveDoc.AveSPItem.AveSPList.SPList.ContentTypes.GetById(contenttypeId).Name;
                                info = new MetaDataItemInfo(displayName, contenttypeName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                                string title = string.Empty;
                                if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                                {
                                    title = aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version)["Title"].ToString();
                                }
                                else
                                {
                                    title = string.IsNullOrEmpty(values["Title"].ToString()) ? string.Empty : values["Title"].ToString();
                                }
                                info = new MetaDataItemInfo(displayName, title, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Name":
                                string name = string.Empty;
                                name = string.IsNullOrEmpty(values["FileLeafRef"].ToString()) ? string.Empty : values["FileLeafRef"].ToString();
                                if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                                {
                                    string fileVersion = aveDoc.AveSPItem.Version / 512 + "." + aveDoc.AveSPItem.Version % 512;
                                    name = name + ":" + fileVersion;
                                }
                                info = new MetaDataItemInfo(displayName, name, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "File Type":
                                string format = string.Empty;
                                format = string.IsNullOrEmpty(values["File_x0020_Type"].ToString()) ? string.Empty : values["File_x0020_Type"].ToString();
                                info = new MetaDataItemInfo(displayName, format, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified By":
                                #region modified by
                                string modifiedByName = string.Empty;
                                try
                                {
                                    //aveDoc.AveSPItem.SPListItem["Editor"]获取的UserName格式为9;#mark wang.
                                    //values["Modified_x0020_By"].ToString();获取的UserName格式为 "i:0#.f|membership|xdx@xdx.partner.onmschina.cn" 
                                    if (item.SPListItem != null)
                                    {
                                        string itemUserInfo = string.Empty;
                                        if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                                        {
                                            itemUserInfo = aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version)["Editor"].ToString();
                                        }
                                        else
                                        {
                                            itemUserInfo = aveDoc.AveSPItem.SPListItem["Editor"].ToString();
                                        }
                                        string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                        IAveUser user = item.SPListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                        if (user != null)
                                        {
                                            modifiedByName = user.NoPrefixLoginName;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    modifiedByName = string.Empty;
                                    mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                                }
                                info = new MetaDataItemInfo(displayName, modifiedByName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Modified":
                                #region modified
                                string modifiedTime = string.Empty;
                                DateTime modifiedTimeDate = DateTime.MinValue;
                                //Current Version Kind is UTC and Time is UTC，Other Version Kind is Unspecified but time is UTC.that is all is UTC Time.
                                //if (((DateTime)values["Modified"]).Kind == DateTimeKind.Utc)
                                {
                                    DateTime temp = (DateTime)values["Modified"];
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                    modifiedTimeDate = temp + cstZone.GetUtcOffset(temp);
                                }
                                //else
                                //{
                                //    modifiedTimeDate = ((DateTime)values["Modified"]);
                                //}
                                if (useFormat)
                                {
                                    modifiedTime = modifiedTimeDate.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveDoc.AveSPWeb.SPWeb.RegionalSettings.LocaleId));
                                    modifiedTime = modifiedTimeDate.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + modifiedTimeDate.ToString(culture.DateTimeFormat.ShortTimePattern);
                                }
                                info = new MetaDataItemInfo(displayName, modifiedTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created":
                                #region Created
                                var createdTime = string.Empty;
                                var createdDateTime = DateTime.MinValue;

                                //Current Version Kind is UTC and Time is UTC，Other Version Kind is Unspecified but time is UTC.that is all is UTC Time.
                                //if (((DateTime)values["Created"]).Kind == DateTimeKind.Utc)
                                {
                                    DateTime temp = (DateTime)values["Created"];
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                    createdDateTime = temp + cstZone.GetUtcOffset(temp);
                                }
                                //else
                                //{
                                //    createdDateTime = ((DateTime)values["Created"]);
                                //}
                                if (useFormat)
                                {
                                    createdTime = createdDateTime.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveDoc.AveSPWeb.SPWeb.RegionalSettings.LocaleId));
                                    createdTime = createdDateTime.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + createdDateTime.ToString(culture.DateTimeFormat.ShortTimePattern);
                                }
                                info = new MetaDataItemInfo(displayName, createdTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "File Size":
                            case "FileSize":
                                string fileSize = string.Empty;
                                if (values.ContainsKey("File_x0020_Size"))
                                {
                                    fileSize = ConvertByteToKB(values["File_x0020_Size"].ToString()) + " KB";
                                }
                                else
                                {
                                    fileSize = "0 KB";
                                }
                                info = new MetaDataItemInfo(displayName, fileSize, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created By":
                                #region Created By
                                var createdByName = string.Empty;
                                if (aveDoc.AveSPItem.Author != null)
                                {
                                    int indexUser = aveDoc.AveSPItem.Author.Login.LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (indexUser > 0)
                                    {
                                        createdByName = (aveDoc.AveSPItem.Author.Login).Substring(indexUser + 1);
                                    }
                                    else
                                    {
                                        createdByName = (aveDoc.AveSPItem.Author.Login).ToString();
                                    }
                                }
                                else if (values["Created_x0020_By"] != null)
                                {
                                    int indexUser = values["Created_x0020_By"].ToString().LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                    if (indexUser > 0)
                                    {
                                        createdByName = (values["Created_x0020_By"].ToString()).Substring(indexUser + 1);
                                    }
                                    else
                                    {
                                        createdByName = (values["Created_x0020_By"].ToString()).ToString();
                                    }
                                }
                                info = new MetaDataItemInfo(displayName, createdByName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            default:
                                #region Custom Column 
                                try
                                {
                                    var field = aveDoc.AveSPItem.SPListItem.Fields.GetField(mappedKey);
                                    var internalName = field.InternalName;
                                    //Dictionary<string, object> columns = aveDoc.AveSPItem.GetAllColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllColumns);//aveDoc.AveSPItem.SPListItem.FieldValues;
                                    if (values.ContainsKey(internalName))
                                    {
                                        object tempValue = values[internalName];
                                        #region version reget value
                                        //version reget value by version property.
                                        if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version
                                            && internalName != "ID"
                                            )
                                        {
                                            try
                                            {
                                                tempValue = aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version)[field.InternalName];
                                            }
                                            catch (Exception ex)
                                            {
                                                mLog.Warn("Can't get version field value,field InternalName:{0},file name:{1},version:{2},Message:{3}.", field.InternalName, aveDoc.AveSPItem.SPListItem.File.Name, aveDoc.AveSPItem.Version, ex.ToString());
                                                tempValue = values[internalName];
                                            }
                                        }
                                        #endregion
                                        switch (field.Type)
                                        {
                                            case AveFieldType.DateTime:
                                                if (tempValue != null)
                                                {
                                                    DateTime temp = (DateTime)tempValue;
                                                    if (temp.Kind == DateTimeKind.Utc)
                                                    {
                                                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                                        temp = temp + cstZone.GetUtcOffset(temp);
                                                        if (!useFormat)
                                                        {
                                                            CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveDoc.AveSPWeb.SPWeb.RegionalSettings.LocaleId));
                                                            value = (temp).ToString(culture.DateTimeFormat.ShortDatePattern) + " " + (temp).ToString(culture.DateTimeFormat.ShortTimePattern);
                                                        }
                                                        else
                                                        {
                                                            value = (temp).ToString(fieldInfo.DateFormat);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveDoc.AveSPWeb.SPWeb.RegionalSettings.LocaleId));
                                                        value = temp.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + temp.ToString(culture.DateTimeFormat.ShortTimePattern);
                                                    }
                                                }
                                                else
                                                {
                                                    value = null;
                                                }
                                                break;
                                            case AveFieldType.Invalid:
                                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                                {
                                                    StringBuilder sb = new StringBuilder();
                                                    string[] taxValues = tempValue.ToString().Split(';');
                                                    foreach (var taxValue in taxValues)
                                                    {
                                                        sb.Append(taxValue.Split('|')[0] + ";");
                                                    }
                                                    value = sb.ToString().TrimEnd(';');
                                                }
                                                break;
                                            case AveFieldType.User:
                                            case AveFieldType.Lookup:
                                                if (field.Type == AveFieldType.User)
                                                {
                                                    if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                                                    {
                                                        tempValue = field.GetFieldValueAsText(aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version)[field.InternalName]);
                                                    }
                                                    else
                                                    {
                                                        tempValue = field.GetFieldValueAsText(aveDoc.AveSPItem.SPListItem[field.ID]);
                                                    }
                                                }
                                                else if (field.Type == AveFieldType.Lookup)
                                                {
                                                    if (aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                                                    {
                                                        tempValue = aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version)[field.InternalName];
                                                    }
                                                }
                                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                                {
                                                    StringBuilder sb = new StringBuilder();
                                                    string[] taxValues = tempValue.ToString().Split('#');
                                                    bool needAdd = false;
                                                    foreach (var taxValue in taxValues)
                                                    {
                                                        if (needAdd)
                                                        {
                                                            sb.Append(taxValue);
                                                            needAdd = false;
                                                        }
                                                        else
                                                        {
                                                            needAdd = true;
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(sb.ToString()))
                                                    {
                                                        value = sb.ToString().TrimEnd(';');
                                                    }
                                                    else
                                                    {
                                                        value = tempValue.ToString();
                                                    }
                                                }
                                                break;
                                            case AveFieldType.Boolean:
                                                if (tempValue.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    value = "Yes";
                                                }
                                                else if (tempValue.ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    value = "No";
                                                }
                                                else
                                                {
                                                    value = tempValue.ToString();
                                                }
                                                break;
                                            case AveFieldType.MultiChoice:
                                                if (tempValue != null && tempValue is Array)
                                                {
                                                    try
                                                    {
                                                        string[] stringArray = tempValue as string[];
                                                        StringBuilder stringBuilder = new StringBuilder();
                                                        foreach (var choiceValue in stringArray)
                                                        {
                                                            stringBuilder.Append(choiceValue + ";");
                                                        }
                                                        value = stringBuilder.ToString().TrimEnd(';');
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        mLog.Info("Can not get MultiChoice value,Message:{0}.", ex.ToString());
                                                    }
                                                }
                                                break;
                                            default:
                                                #region special column.
                                                if (internalName.Equals("_IsRecord"))
                                                {
                                                    value = "No";
                                                }
                                                else if (internalName.Equals("RecordsRelated"))
                                                {
                                                    var recordsRelated = string.Empty;
                                                    mLog.Info("Current column is RecordsRelated and get display name in NAA Export.");
                                                    string recordsRelatedValue = tempValue == null ? string.Empty : tempValue.ToString();
                                                    if (!string.IsNullOrEmpty(recordsRelatedValue))
                                                    {
                                                        try
                                                        {
                                                            var sourceUrlValue = recordsRelatedValue;
                                                            XmlDocument xmlDoc = new XmlDocument();
                                                            sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                                                            xmlDoc.LoadXml(sourceUrlValue);
                                                            foreach (XmlNode ele in xmlDoc.GetElementsByTagName("a"))
                                                            {
                                                                recordsRelated += HttpUtility.UrlDecode(ele.InnerText) + ";";
                                                            }
                                                            recordsRelated = recordsRelated.TrimEnd(';');
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            recordsRelated = tempValue.ToString();
                                                            mLog.Info("Can not get RecordsRelated,Message:{0}.", ex.ToString());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        mLog.Info("Current column is RecordsRelated and column value is null in NAA Export.");
                                                    }
                                                    value = recordsRelated;
                                                }
                                                else if (internalName.Equals("ImageSize") && values.ContainsKey("ImageWidth") && values.ContainsKey("ImageHeight"))
                                                {
                                                    value = values["ImageWidth"].ToString() + " x " + values["ImageHeight"].ToString();
                                                }
                                                else if (internalName.Equals("_dlc_DocIdUrl") && values.ContainsKey("_dlc_DocId"))
                                                {
                                                    value = values["_dlc_DocId"].ToString();
                                                }
                                                #endregion
                                                else
                                                {
                                                    value = tempValue.ToString();
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        value = null;
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Info("Can not get column value from SharePoint in item level by item column.Info: {0}.", e.ToString());
                                    value = null;
                                }
                                if (value == null)
                                {
                                    string val1 = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                    info = new MetaDataItemInfo(displayName, val1, true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, value.ToString(), true, value.GetType());
                                }
                                columnValue.Add(info);
                                break;
                                #endregion
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get naa csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                AddMetaDataItemInfoDiagnoseLog(columnValue);
                return columnValue;
            }
        }

        public string ConvertByteToKB(string strByte)
        {
            string kb = "0";
            if (string.IsNullOrEmpty(strByte))
            {
                return kb;
            }
            double i;
            bool b = double.TryParse(strByte, out i);
            if (b)
            {
                double dkb = i / 1024;
                if (dkb > (int)dkb)
                {
                    dkb++;
                }
                int value = (int)dkb;
                kb = value.ToString();
            }
            return kb;
        }


        public List<MetaDataItemInfo> GetCSVListFromColumnValue(AveSPFolder aveFolder, string disposalClass, string filePath)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("NAAExport_AveSPFolderGetCSVListFromColumnValue"))
            {
                var item = aveFolder.AveItem;
                //add this for get folder column value, otherwhise can not get right column value.
                item.UserDataCache = item.GetUserData();
                Dictionary<string, object> values = item.GetColumnValues();
                AddItemPropertyDiagnoseLog(values);
                List<MetaDataItemInfo> columnValue = new List<MetaDataItemInfo>();
                MetaDataItemInfo info = new MetaDataItemInfo();
                foreach (var fieldInfo in mFieldInfos)
                {
                    string displayName = fieldInfo.DisplayName;
                    string mappedKey = fieldInfo.MappedKey;
                    object value = null;
                    try
                    {
                        if (displayName.Equals("Additional Metadata<Keywords>", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(mappedKey))
                        {
                            #region Keywords
                            string keywordsValue = string.Empty;
                            //site title
                            string siteTtile = item.AveSPList.ParentWeb.SPWeb.Title;
                            //term set value
                            string termSetValue = string.Empty;
                            string bcsTerm = item.SPListItem[RevIMColumnName] == null ? string.Empty : item.SPListItem[RevIMColumnName].ToString();
                            if (bcsTerm.Contains("|"))
                            {
                                string[] tempTerm = bcsTerm.Split('|');
                                termSetValue = tempTerm[0];
                            }
                            //File Name
                            string fileName = string.Empty;
                            fileName = string.IsNullOrEmpty(values["FileLeafRef"].ToString()) ? string.Empty : values["FileLeafRef"].ToString();
                            if (item.SPListItem["Version"].ToString() != values["_UIVersionString"].ToString())
                            {
                                fileName = fileName + ":" + values["_UIVersionString"];
                            }
                            //Guid
                            //string fileGuid = string.Empty;
                            //fileGuid = values["GUID"].ToString();
                            keywordsValue = siteTtile + "-" + termSetValue + "-" + fileName;
                            info = new MetaDataItemInfo(displayName, keywordsValue, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                            #endregion
                        }
                        else if(string.IsNullOrEmpty(mappedKey))
                        {
                            string val = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                            info = new MetaDataItemInfo(displayName, val, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                        }
                        bool useFormat = !string.IsNullOrEmpty(fieldInfo.DateFormat);
                        #region get value logic
                        switch (mappedKey)
                        {
                            case "URL":
                                string url = webAppUrl + values["FileRef"].ToString();
                                info = new MetaDataItemInfo(displayName, url, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "GUID":
                                info = new MetaDataItemInfo(displayName, values["GUID"].ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "FilePath":
                                string path = string.IsNullOrEmpty(fieldInfo.Prefix) ? filePath : (fieldInfo.Prefix+"\\"+filePath);
                                info = new MetaDataItemInfo(displayName, path, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "ExportFileName":
                                info = new MetaDataItemInfo(displayName, "Null", true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Checksum":
                                #region Checksum
                                info = new MetaDataItemInfo(displayName, "Null", true, typeof(string));
                                columnValue.Add(info);
                                break;
                                #endregion
                            case "Disposal Class":
                            case "Disposal class":
                            case "Disposition Authority":
                                #region disposal class logic.
                                //string disposalClass = string.Empty;
                                //var bcsTerm = item.SPListItem[RevIMColumnName] == null ? string.Empty : item.SPListItem[RevIMColumnName].ToString();
                                //if (bcsTerm.Contains("|"))
                                //{
                                //    string[] tempTerm = bcsTerm.Split('|');
                                //    var termId = tempTerm[1];
                                //    var term = taxSession.GetTerm(Guid.Parse(termId));
                                //    term.CustomProperties.TryGetValue(mappedKey, out disposalClass);
                                //    disposalClass = string.IsNullOrEmpty(disposalClass) ? string.Empty : disposalClass;
                                //    mLog.Debug("Get CSV List From ColumnValue (Disposal Class) termID:{0},revIMColumnName:{1},disposalValue:{2},mappedKey:{3}", termId, RevIMColumnName, disposalClass, mappedKey);
                                //}
                                //string val = string.IsNullOrEmpty(disposalClass) && !string.IsNullOrEmpty(fieldInfo.DefaultValue) ? fieldInfo.DefaultValue : disposalClass;
                                //info = new MetaDataItemInfo(displayName, val, typeof(string));
                                //columnValue.Add(info);
                                info = new MetaDataItemInfo(displayName, disposalClass, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Content Type":
                                var contenttypeName = item.SPListItem.ContentType.Name;
                                info = new MetaDataItemInfo(displayName, contenttypeName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                                var title = item.SPListItem.Name;
                                info = new MetaDataItemInfo(displayName, title, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified By":
                                #region modified by
                                string modifiedByName = string.Empty;
                                try
                                {
                                    if (item.SPListItem != null)
                                    {
                                        string itemUserInfo = item.SPListItem["Editor"].ToString();
                                        string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                        IAveUser user = item.SPListItem.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
                                        if (user != null)
                                        {
                                            modifiedByName = user.NoPrefixLoginName;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    modifiedByName = string.Empty;
                                    mLog.Warn("Can not get ModifiedBy,Info: {0}.", ex.ToString());
                                }
                                info = new MetaDataItemInfo(displayName, modifiedByName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Modified":
                                #region modified
                                string modifiedTime = string.Empty;
                                DateTime modifiedTimeDate = DateTime.MinValue;
                                if (((DateTime)item.SPListItem["Modified"]).Kind == DateTimeKind.Utc)
                                {
                                    DateTime temp = (DateTime)item.SPListItem["Modified"];
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveFolder.AveList.ParentWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                    modifiedTimeDate = temp + cstZone.GetUtcOffset(temp);
                                }
                                else
                                {
                                    modifiedTimeDate = (DateTime)item.SPListItem["Modified"];
                                }
                                if (useFormat)
                                {
                                    modifiedTime = modifiedTimeDate.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveFolder.SPFolder.ParentWeb.RegionalSettings.LocaleId));
                                    modifiedTime = modifiedTimeDate.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + modifiedTimeDate.ToString(culture.DateTimeFormat.ShortTimePattern);
                                }
                                info = new MetaDataItemInfo(displayName, modifiedTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created":
                                #region Created
                                var createdTime = string.Empty;
                                var createdDateTime = DateTime.MinValue;

                                if (((DateTime)item.SPListItem["Created"]).Kind == DateTimeKind.Utc)
                                {
                                    DateTime temp = (DateTime)item.SPListItem["Created"];
                                    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveFolder.AveList.ParentWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                    createdDateTime = temp + cstZone.GetUtcOffset(temp);
                                }
                                else
                                {
                                    createdDateTime = (DateTime)item.SPListItem["Created"];
                                }
                                if (useFormat)
                                {
                                    createdTime = createdDateTime.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveFolder.SPFolder.ParentWeb.RegionalSettings.LocaleId));
                                    createdTime = createdDateTime.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + createdDateTime.ToString(culture.DateTimeFormat.ShortTimePattern);
                                }
                                info = new MetaDataItemInfo(displayName, createdTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created By":
                                #region Created By
                                var createdByName = string.Empty;
                                int indexUser = (item.Author.Login).LastIndexOf("|", StringComparison.OrdinalIgnoreCase);
                                if (indexUser > 0)
                                {
                                    createdByName = (item.Author.Login).Substring(indexUser + 1);
                                }
                                else
                                {
                                    createdByName = (item.Author.Login).ToString();
                                }
                                info = new MetaDataItemInfo(displayName, createdByName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            default:
                                #region Custom Column 
                                try
                                {
                                    var field = aveFolder.AveItem.SPListItem.Fields.GetField(mappedKey);
                                    var internalName = field.InternalName;
                                    //Dictionary<string, object> columns = aveFolder.AveParentFolder.GetAllColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllColumns);//aveFolder.AveItem.SPListItem.FieldValues;
                                    if (values.ContainsKey(internalName))
                                    {
                                        object tempValue = values[internalName];
                                        switch (field.Type)
                                        {
                                            case AveFieldType.DateTime:
                                                if (tempValue != null)
                                                {
                                                    DateTime temp = (DateTime)tempValue;
                                                    if (temp.Kind == DateTimeKind.Utc)
                                                    {
                                                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(aveFolder.AveList.ParentWeb.SPWeb.RegionalSettings.TimeZone.ID));
                                                        temp = temp + cstZone.GetUtcOffset(temp);
                                                        if (!useFormat)
                                                        {
                                                            CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveFolder.SPFolder.ParentWeb.RegionalSettings.LocaleId));
                                                            value = (temp).ToString(culture.DateTimeFormat.ShortDatePattern) + " " + (temp).ToString(culture.DateTimeFormat.ShortTimePattern);
                                                        }
                                                        else
                                                        {
                                                            value = (temp).ToString(fieldInfo.DateFormat);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        CultureInfo culture = CultureInfo.GetCultureInfo(Convert.ToInt32(aveFolder.SPFolder.ParentWeb.RegionalSettings.LocaleId));
                                                        value = temp.ToString(culture.DateTimeFormat.ShortDatePattern) + " " + temp.ToString(culture.DateTimeFormat.ShortTimePattern);
                                                    }
                                                }
                                                else
                                                {
                                                    value = null;
                                                }
                                                break;
                                            case AveFieldType.Invalid:
                                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                                {
                                                    StringBuilder sb = new StringBuilder();
                                                    string[] taxValues = tempValue.ToString().Split(';');
                                                    foreach (var taxValue in taxValues)
                                                    {
                                                        sb.Append(taxValue.Split('|')[0] + ";");
                                                    }
                                                    value = sb.ToString().TrimEnd(';');
                                                }
                                                break;
                                            case AveFieldType.User:
                                            case AveFieldType.Lookup:
                                                if (field.Type == AveFieldType.User)
                                                {
                                                    tempValue = field.GetFieldValueAsText(item.SPListItem[field.ID]);
                                                }
                                                if (tempValue != null && !string.IsNullOrEmpty(tempValue.ToString()))
                                                {
                                                    StringBuilder sb = new StringBuilder();
                                                    string[] taxValues = tempValue.ToString().Split('#');
                                                    bool needAdd = false;
                                                    foreach (var taxValue in taxValues)
                                                    {
                                                        if (needAdd)
                                                        {
                                                            sb.Append(taxValue);
                                                            needAdd = false;
                                                        }
                                                        else
                                                        {
                                                            needAdd = true;
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(sb.ToString()))
                                                    {
                                                        value = sb.ToString().TrimEnd(';');
                                                    }
                                                    else
                                                    {
                                                        value = tempValue.ToString();
                                                    }
                                                }
                                                break;
                                            case AveFieldType.Boolean:
                                                if (tempValue.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    value = "Yes";
                                                }
                                                else if (tempValue.ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    value = "No";
                                                }
                                                else
                                                {
                                                    value = tempValue.ToString();
                                                }
                                                break;
                                            case AveFieldType.MultiChoice:
                                                if (tempValue != null && tempValue is Array)
                                                {
                                                    try
                                                    {
                                                        string[] stringArray = tempValue as string[];
                                                        StringBuilder stringBuilder = new StringBuilder();
                                                        foreach (var choiceValue in stringArray)
                                                        {
                                                            stringBuilder.Append(choiceValue + ";");
                                                        }
                                                        value = stringBuilder.ToString().TrimEnd(';');
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        mLog.Info("Can not get MultiChoice value,Message:{0}.", ex.ToString());
                                                    }
                                                }
                                                break;
                                            default:
                                                if (internalName.Equals("_dlc_DocIdUrl") && values.ContainsKey("_dlc_DocId"))
                                                {
                                                    value = values["_dlc_DocId"].ToString();
                                                }
                                                else
                                                {
                                                    value = tempValue.ToString();
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        value = null;
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Info("Can not get column value from SharePoint in item level by item column.Info: {0}.", e.ToString());
                                    value = null;
                                }
                                if (value == null)
                                {
                                    string val1 = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                    info = new MetaDataItemInfo(displayName, val1, true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, value.ToString(), true, value.GetType());
                                }
                                columnValue.Add(info);
                                break;
                                #endregion
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get naa csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                AddMetaDataItemInfoDiagnoseLog(columnValue);
                return columnValue;
            }
        }

        public void AddMetaDataItemInfoDiagnoseLog(List<MetaDataItemInfo> columnValue)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (MetaDataItemInfo item in columnValue)
                {
                    sb.Append(" Name:" + item.Name + " ,Value:" + item.Value + ". ");
                }
                mLog.Info("AddMetaDataItemInfoDiagnoseLog:" + sb.ToString());
            }
            catch (Exception ex)
            {
                mLog.Warn("AddMetaDataItemInfoDiagnoseLog:" + ex.ToString());
            }
        }

        public void AddItemPropertyDiagnoseLog(Dictionary<string, object> values)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in values)
                {
                    string tempValue = item.Value == null ? string.Empty : item.Value.ToString();
                    sb.Append(" Name:" + item.Key.ToString() + " ,Value:" + tempValue + ". ");
                }
                mLog.Info("AddItemPropertyDiagnoseLog:" + sb.ToString());
            }
            catch (Exception ex)
            {
                mLog.Warn("AddItemPropertyDiagnoseLog:" + ex.ToString());
            }
        }

        private string GetWebappUrl(AveSPSite aveSite)
        {
            Uri webAppUri = new Uri(aveSite.SPSite.Url);
            //if (aveSite.SPSite.SPMode == AvePoint.Wrapper.Core.Common.WrapperSPMode.O365)
            //{
            string webAppUrl;
            string siteUrl = aveSite.SPSite.Url;
            int lengh = 0;
            if (siteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                lengh = "https://".Length;
            }
            else
            {
                //Server Farm Regist as O365
                lengh = "http://".Length;
            }
            int indexOfSlash = siteUrl.IndexOf("/", lengh, StringComparison.OrdinalIgnoreCase);
            webAppUrl = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppUrl = siteUrl.Substring(0, indexOfSlash);
            }
            webAppUri = new Uri(webAppUrl);
            //}
            //else
            //{
            //    webAppUri = aveSite.SPSite.WebApplication.GetResponseUri(AveUrlZone.Default);
            //}
            return webAppUri.AbsoluteUri.Trim(SLASH);
        }


    }

    internal class NAAFieldInfo
    {
        internal string DisplayName { get; set; }
        internal string MappedKey { get; set; }
        internal string InternalName { get; set; }
        internal string DefaultValue { get; set; }
        internal string Prefix { get; set; }
        internal string Url { get; set; }
        internal string DateFormat { get; set; }
    }
}
