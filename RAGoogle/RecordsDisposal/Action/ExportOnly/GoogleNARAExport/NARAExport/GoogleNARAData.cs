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
using AvePoint.Wrapper.Common;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using System.Reflection;
using System.Xml.Serialization;

namespace RAGoogle
{
    public class GoogleNARAData
    {
        public GoogleNARAData()
        {
        }
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<GoogleNARAMetaInfo> mConfigData = null;
        public static List<GoogleNARAMetaInfo> NARAConfigCache
        {
            get
            {
                return mConfigData;
            }
        }
        public static List<GoogleNARAMetaInfo> InitConfig(byte[] NARAConfigFile)
        {
            try
            {
                if (mConfigData == null)
                {
                    mConfigData = new List<GoogleNARAMetaInfo>();
                    using (MemoryStream configStream = new MemoryStream(NARAConfigFile))
                    {
                        GoogleNARAConfig mConfig = (GoogleNARAConfig)new XmlSerializer(typeof(GoogleNARAConfig)).Deserialize(configStream);
                        mConfigData = mConfig.MetaInfos;
                    }
                }
            }
            catch (Exception ex)
            {
                mConfigData = null;
                mLog.Error("Init GoogleNARA Config faild,ERROR:{0}", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_GoogleNARAExportConfigFileDeserializeException");
            }
            return mConfigData;
        }
    }
    public class GoogleNARAColumnContainer
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public const string CSVLINK = "=HYPERLINK(\"{0}\",\"{1}\")";


        private const string HEADERFORMAT = "Additional Metadata<{0}>";
        private List<GoogleNARAFieldInfo> mFieldInfos = null;
        private List<string> mHeaderInfos = null;

        public GoogleNARAColumnContainer()
        {
            Init();
        }

        public void Init()
        {
            try
            {
                if (mFieldInfos == null || mHeaderInfos == null)
                {
                    mFieldInfos = new List<GoogleNARAFieldInfo>();
                    mHeaderInfos = new List<string>();
                    if (GoogleNARAData.NARAConfigCache == null)
                    {
                        mLog.Error("GoogleNARA config file init fail.");
                        throw new Exception("An error occurred while exporting Google.");
                    }
                    foreach (var cfgItem in GoogleNARAData.NARAConfigCache)
                    {
                        if (!string.IsNullOrEmpty(cfgItem.DisplayName))
                        {
                            string displayName = cfgItem.AdditionalMetadata ? string.Format(HEADERFORMAT, cfgItem.DisplayName) : cfgItem.DisplayName;

                            mFieldInfos.Add(new GoogleNARAFieldInfo()
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
                mLog.Error("error occurred while init nara config,ERROR:{0}", ex.ToString());
                throw;
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
        public List<MetaDataItemInfo> GetCSVListFromColumnValue(DownloadedFileInfo googleItem, string disposalClass, string filePath, string hashString)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GoogleNARAExport_ItemGetCSVListFromColumnValue"))
            {
                List<MetaDataItemInfo> columnValue = new List<MetaDataItemInfo>();
                MetaDataItemInfo info = new MetaDataItemInfo();
                foreach (var fieldInfo in mFieldInfos)
                {
                    string displayName = fieldInfo.DisplayName;
                    string mappedKey = fieldInfo.MappedKey;
                    try
                    {
                        if (string.IsNullOrEmpty(mappedKey))
                        {
                            string val = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                            info = new MetaDataItemInfo(displayName, val, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                        }
                        switch (mappedKey)
                        {
                            case "ID":
                                info = new MetaDataItemInfo(displayName, googleItem.Id.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Name":
                                var name = googleItem.VersionName == string.Empty ? googleItem.FileName : googleItem.FileName + ":" + googleItem.VersionName;
                                info = new MetaDataItemInfo(displayName, name, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                                info = new MetaDataItemInfo(displayName, googleItem.FolderName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Created":
                                #region Created
                                var createdTime = string.Empty;
                                createdTime = googleItem.CreatedTime.ToString(fieldInfo.DateFormat);//check format
                                info = new MetaDataItemInfo(displayName, createdTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Creator":
                            case "Owners":
                                #region Create By
                                info = new MetaDataItemInfo(displayName, googleItem.CreatedBy, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "ExportFileName":
                                info = new MetaDataItemInfo(displayName, filePath, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Type":
                                info = new MetaDataItemInfo(displayName, googleItem.FileExtension, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Size":
                            case "File size":
                            case "Storage used":
                                var sizeKB = ConvertByteToKB((googleItem.Size.Value).ToString()) + "KB";
                                info = new MetaDataItemInfo(displayName, sizeKB, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Location":
                                //folder parent
                                var dirPath = string.IsNullOrEmpty(googleItem.RelativePath) ? string.Empty : Path.GetDirectoryName(googleItem.RelativePath) ?? string.Empty;
                                info = new MetaDataItemInfo(displayName, dirPath.Replace("\\", "/"), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified":
                            case "Last modified":
                                var modified = string.Empty;
                                modified = googleItem.ModifiedTime.ToString(fieldInfo.DateFormat);
                                info = new MetaDataItemInfo(displayName, modified, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified by":
                                info = new MetaDataItemInfo(displayName, googleItem.ModifiedBy, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Description":
                                info = new MetaDataItemInfo(displayName, googleItem.Description, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Who has access":
                                var permissions = googleItem.Permissions.Select(item => item.DisplayName).ToList();
                                var result = string.Join(", ", permissions);
                                info = new MetaDataItemInfo(displayName, result, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Labels":
                                var labels = googleItem.LabelApplyInfos.Select(item => item.Name).ToList();
                                var labelNames = string.Join(", ", labels);
                                info = new MetaDataItemInfo(displayName, labelNames, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Disposal class":
                            case "Disposition Authority":
                                info = new MetaDataItemInfo(displayName, disposalClass, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "SHA256Hash":
                                info = new MetaDataItemInfo(displayName, hashString, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            default:
                                string value = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                info = new MetaDataItemInfo(displayName, value, true, typeof(string));
                                columnValue.Add(info);
                                break;

                        }
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get Google nara csv column value failed {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                return columnValue;
            }
        }
        public List<MetaDataItemInfo> GetCSVFolderListFromColumnValue(DownloadedFileInfo googleItem, string disposalClass, string exportPath)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GoogleNARAExport_ItemGetCSVListFromColumnValue"))
            {
                List<MetaDataItemInfo> columnValue = new List<MetaDataItemInfo>();
                MetaDataItemInfo info = new MetaDataItemInfo();
                foreach (var fieldInfo in mFieldInfos)
                {
                    string displayName = fieldInfo.DisplayName;
                    string mappedKey = fieldInfo.MappedKey;
                    try
                    {
                        if (string.IsNullOrEmpty(mappedKey))
                        {
                            string val = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                            info = new MetaDataItemInfo(displayName, val, true, typeof(string));
                            columnValue.Add(info);
                            continue;
                        }
                        switch (mappedKey)
                        {
                            case "ID":
                                info = new MetaDataItemInfo(displayName, googleItem.Id.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Name":
                                var name = googleItem.VersionName == string.Empty ? googleItem.FormattedFileVersionName : googleItem.FormattedFileVersionName + ":" + googleItem.VersionName;
                                info = new MetaDataItemInfo(displayName, name, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                                info = new MetaDataItemInfo(displayName, googleItem.FormattedFileVersionName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Created":
                                #region Created
                                var createdTime = string.Empty;
                                createdTime = googleItem.CreatedTime.ToString(fieldInfo.DateFormat);//check format
                                info = new MetaDataItemInfo(displayName, createdTime, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Creator":
                            case "Owners":
                                #region Create By
                                info = new MetaDataItemInfo(displayName, googleItem.CreatedBy, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "ExportFileName":
                                info = new MetaDataItemInfo(displayName, exportPath, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Type":
                                info = new MetaDataItemInfo(displayName, "Google Drive Folder", true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Size":
                            case "File size":
                                info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Storage used":
                                info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Location":
                                info = new MetaDataItemInfo(displayName, googleItem.RelativePath, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified":
                            case "Last modified":
                                var modified = string.Empty;
                                modified = googleItem.ModifiedTime.ToString(fieldInfo.DateFormat);
                                info = new MetaDataItemInfo(displayName, modified, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified by":
                                info = new MetaDataItemInfo(displayName, googleItem.ModifiedBy, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Description":
                                info = new MetaDataItemInfo(displayName, googleItem.Description, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Who has access":
                                var permissions = googleItem.Permissions.Select(item => item.DisplayName).ToList();
                                var result = string.Join(", ", permissions);
                                info = new MetaDataItemInfo(displayName, result, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Labels":
                                var labels = googleItem.LabelApplyInfos.Select(item => item.Name).ToList();
                                var labelNames = string.Join(", ", labels);
                                info = new MetaDataItemInfo(displayName, labelNames, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Disposal class":
                            case "Disposition Authority":
                                info = new MetaDataItemInfo(displayName, disposalClass, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            default:
                                string value = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                info = new MetaDataItemInfo(displayName, value, true, typeof(string));
                                columnValue.Add(info);
                                break;

                        }
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get Google nara csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
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
    }
    internal class GoogleNARAFieldInfo
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
