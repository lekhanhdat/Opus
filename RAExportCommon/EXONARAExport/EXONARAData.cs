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
using AvePoint.Media.Core.IO.Input;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using LOGRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;

namespace RAExportCommon
{
    public class EXONARAData
    {
        public EXONARAData()
        { }

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<EXONARAMetaInfo> mConfigData = null;

        public static List<EXONARAMetaInfo> NARAConfigCache
        {
            get
            {
                return mConfigData;
            }
        }

        public static List<EXONARAMetaInfo> InitConfig(byte[] NARAConfigFile)
        {
            try
            {
                if (mConfigData == null)
                {
                    mConfigData = new List<EXONARAMetaInfo>();
                    using (MemoryStream configStream = new MemoryStream(NARAConfigFile))
                    {
                        EXONARAConfig mConfig = (EXONARAConfig)new XmlSerializer(typeof(EXONARAConfig)).Deserialize(configStream);
                        mConfigData = mConfig.MetaInfos;
                    }
                }
            }
            catch (Exception ex)
            {
                mConfigData = null;
                mLog.Error("Init EXONARA Config faild,ERROR:{0}", ex.ToString());
                throw new Exception("StorageOptimization_EXONARAExportConfigFileDeserializeException");
            }
            return mConfigData;
        }
        public static List<EXONARAMetaInfo> InitConfig()
        {
            try
            {
                if (mConfigData == null)
                {
                    string configLocation = string.Empty; //VaultConfigFileInfo.NARAConfigurationFileFullPath;
                    mConfigData = new List<EXONARAMetaInfo>();
                    if (!File.Exists(configLocation))
                    {
                        mConfigData = null;
                        mLog.Warn("An error occurred while loading the EXONARA configuration file, file not found.");
                        return mConfigData;
                        //CreateByDefault(configLocation);
                    }
                    using (Stream sm = File.Open(configLocation, FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader sr = new StreamReader(sm, Encoding.UTF8))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(EXONARAConfig));
                            object obj = serializer.Deserialize(sr);
                            if (obj != null)
                            {
                                EXONARAConfig naraConfig = obj as EXONARAConfig;
                                mConfigData = naraConfig.MetaInfos;
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

            EXONARAConfig config = new EXONARAConfig()
            {
                MetaInfos = new List<EXONARAMetaInfo>()
                {
                    new EXONARAMetaInfo() { DisplayName = "Identifier:FileName", MappedKey = "Name", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Identifier:RecordID", MappedKey = "GUID", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Title", MappedKey = "Title", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Description", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Creator", MappedKey = "Created By", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Date:CreationDate", MappedKey = "Created", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Rights:SecurityClassification", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Rights:PreviousSecurityClassification", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Rights:AccessRights", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Rights:UsageRights", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Rights:RightsHolder", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Coverage:SpatialCoverage", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Coverage:TemporalCoverage", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Relation:HasPart", MappedKey = "", AdditionalMetadata = false },
                    new EXONARAMetaInfo() { DisplayName = "Relation:IsPartOf", MappedKey = "", AdditionalMetadata = false },
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

    public class EXONARAColumnContainer
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public const string CSVLINK = "=HYPERLINK(\"{0}\",\"{1}\")";


        private const string HEADERFORMAT = "Additional Metadata<{0}>";
        private string _revIMColumnName = string.Empty;
        private List<EXONARAFieldInfo> mFieldInfos = null;
        private List<string> mHeaderInfos = null;

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

        public EXONARAColumnContainer()
        {
            Init();
        }

        public void Init()
        {
            try
            {
                if (mFieldInfos == null || mHeaderInfos == null)
                {
                    mFieldInfos = new List<EXONARAFieldInfo>();
                    mHeaderInfos = new List<string>();
                    if (EXONARAData.NARAConfigCache == null)
                    {
                        mLog.Error("EXONARA config file init faild.");
                        throw new Exception(LOGRESOURCE.StorageOptimization_SOARSOVaultBefArFailedError4EXO);
                    }
                    foreach (var cfgItem in EXONARAData.NARAConfigCache)
                    {
                        string mappedName = cfgItem.MappedKey;
                        if (!string.IsNullOrEmpty(cfgItem.DisplayName))
                        {
                            string displayName = cfgItem.AdditionalMetadata ? string.Format(HEADERFORMAT, cfgItem.DisplayName) : cfgItem.DisplayName;

                            mFieldInfos.Add(new EXONARAFieldInfo()
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

        public List<MetaDataItemInfo> GetCSVListFromColumnValue(Item EXOItem, string disposalClass, string exportPath, string filePath, string tempFilePath, string jobId, ExchangeService service,string hashString)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXONAAExport_ItemGetCSVListFromColumnValue"))
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
                        bool useFormat = !string.IsNullOrEmpty(fieldInfo.DateFormat);
                        switch (mappedKey)
                        {
                            case "GUID":
                            case "ID":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.Id.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "ExportPath":
                                string path = string.IsNullOrEmpty(fieldInfo.Prefix) ? exportPath : Path.Combine(fieldInfo.Prefix, exportPath);
                                info = new MetaDataItemInfo(displayName, path, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "FilePath":
                                string fPath = string.IsNullOrEmpty(fieldInfo.Prefix) ? filePath : Path.Combine(fieldInfo.Prefix, filePath);
                                info = new MetaDataItemInfo(displayName, fPath, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Checksum":
                                string checkSum = string.Empty;
                                using (var md5 = MD5.Create())
                                {
                                    using (Stream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                                    {
                                        var hash = md5.ComputeHash(docStream);
                                        checkSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                                    }
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(checkSum, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Disposal Class":
                            case "Disposal class":
                            case "Disposition Authority":
                                #region disposal class logic.
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(disposalClass, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "LastModifiedName":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.LastModifiedName, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                            case "Name":
                            case "Subject":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.Subject ?? string.Empty, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsNew":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsNew.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsUnmodified":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsUnmodified.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsDraft":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsDraft.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "DisplayCc":
                            case "SendCc":
                                var messageSendCc = EXOItem as EmailMessage;
                                if (messageSendCc != null && messageSendCc.CcRecipients != null && messageSendCc.CcRecipients.Count > 0)
                                {
                                    info = new MetaDataItemInfo(displayName, ProcessAdditionalField(string.Join("; ", messageSendCc.CcRecipients.Select(address => ExchangeUtils.EmailAddressToFormatString(address))), fieldInfo), true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.DisplayCc, fieldInfo), true, typeof(string));
                                }
                                columnValue.Add(info);
                                break;
                            case "DisplayTo":
                            case "SendTo":
                                var messageSendTo = EXOItem as EmailMessage;
                                if (messageSendTo != null && messageSendTo.ToRecipients != null && messageSendTo.ToRecipients.Count > 0)
                                {
                                    info = new MetaDataItemInfo(displayName, ProcessAdditionalField(string.Join("; ", messageSendTo.ToRecipients.Select(address => ExchangeUtils.EmailAddressToFormatString(address))), fieldInfo), true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.DisplayTo, fieldInfo), true, typeof(string));
                                }
                                columnValue.Add(info);
                                break;
                            case "Importance":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.Importance.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "AttachmentsCount":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.Attachments.Count.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "TimeNow":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(DateTime.UtcNow.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "File Type":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(ExchangeUtils.ConvertMailTypeToDisplayType(EXOItem.ItemClass), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "SHA256Hash":
                                info = new MetaDataItemInfo(displayName, hashString, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Size":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(ExchangeUtils.ConvertByteToKB(EXOItem.Size.ToString()) + " KB", fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified":
                                #region modified
                                string modifiedTime = string.Empty;
                                if (useFormat)
                                {
                                    modifiedTime = EXOItem.LastModifiedTime.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    modifiedTime = EXOItem.LastModifiedTime.ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(modifiedTime, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created":
                                #region Created
                                var createdTime = string.Empty;
                                if (useFormat)
                                {
                                    createdTime = EXOItem.DateTimeCreated.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    createdTime = EXOItem.DateTimeCreated.ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(createdTime, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "DateTimeSent":
                            case "Send Time":
                                #region DateTimeSent
                                var dateTimeSend = string.Empty;
                                if (useFormat)
                                {
                                    dateTimeSend = EXOItem.DateTimeSent.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    dateTimeSend = EXOItem.DateTimeSent.ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(dateTimeSend, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "DateTimeReceived":
                            case "Received Time":
                                #region DateTimeReceived
                                var dateTimeReceived = string.Empty;
                                if (useFormat)
                                {
                                    dateTimeReceived = EXOItem.DateTimeReceived.ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    dateTimeReceived = EXOItem.DateTimeReceived.ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(dateTimeReceived, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Sender":
                            case "Created By":
                                #region Sender
                                var sender = string.Empty;
                                var message = EXOItem as EmailMessage;
                                if (message != null && message.Sender != null)
                                {
                                    //sender = message.Sender.Name;
                                    //sender = message.Sender.Address;
                                    sender = ExchangeUtils.EmailAddressToFormatString(message.Sender);
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(sender, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            default:
                                #region EXOItem ExtendedProperties 
                                ExtendedProperty extendedProperties = EXOItem.ExtendedProperties.Where(x => x.PropertyDefinition.Name == mappedKey).FirstOrDefault();
                                if (extendedProperties == null)
                                {
                                    string val1 = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                    info = new MetaDataItemInfo(displayName, val1, true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, extendedProperties.Value.ToString(), true, typeof(string));
                                }
                                columnValue.Add(info);
                                break;
                                #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get EXO nara csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                return columnValue;
            }
        }

        public List<MetaDataItemInfo> GetCSVListFromColumnValue(IExchangeItem EXOItem, string disposalClass, string exportPath, string filePath, string tempFilePath, string jobId, ExchangeService service, string hashString)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXONAAExport_ItemGetCSVListFromColumnValue"))
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
                        bool useFormat = !string.IsNullOrEmpty(fieldInfo.DateFormat);
                        switch (mappedKey)
                        {
                            case "GUID":
                            case "ID":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.ItemId.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "ExportPath":
                                string path = string.IsNullOrEmpty(fieldInfo.Prefix) ? exportPath : Path.Combine(fieldInfo.Prefix, exportPath);
                                info = new MetaDataItemInfo(displayName, path, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "FilePath":
                                string fPath = string.IsNullOrEmpty(fieldInfo.Prefix) ? filePath : Path.Combine(fieldInfo.Prefix, filePath);
                                info = new MetaDataItemInfo(displayName, fPath, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Checksum":
                                string checkSum = string.Empty;
                                using (var md5 = MD5.Create())
                                {
                                    using (Stream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                                    {
                                        var hash = md5.ComputeHash(docStream);
                                        checkSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                                    }
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(checkSum, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Disposal Class":
                            case "Disposal class":
                            case "Disposition Authority":
                            #region disposal class logic.
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(disposalClass, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "LastModifiedName":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.ModifiedBy, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Title":
                            case "Name":
                            case "Subject":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.ItemName ?? string.Empty, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsNew":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsNew.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsUnmodified":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsUnmodified.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "IsDraft":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.IsDraft.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "DisplayCc":
                            case "SendCc":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.DisplayCc, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "DisplayTo":
                            case "SendTo":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.DisplayTo, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Importance":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.Importance.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "AttachmentsCount":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(EXOItem.AttachmentCount.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "TimeNow":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(DateTime.UtcNow.ToString(), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "File Type":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(ExchangeUtils.ConvertMailTypeToDisplayType(EXOItem.ItemType), fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "SHA256Hash":
                                info = new MetaDataItemInfo(displayName, hashString, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Size":
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(ExchangeUtils.ConvertByteToKB(EXOItem.ItemSize.ToString()) + " KB", fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Modified":
                            #region modified
                                string modifiedTime = string.Empty;
                                if (useFormat)
                                {
                                    modifiedTime = EXOItem.Modified.ToLocalTime().ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    modifiedTime = EXOItem.Modified.ToLocalTime().ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(modifiedTime, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Created":
                            #region Created
                                var createdTime = string.Empty;
                                if (useFormat)
                                {
                                    createdTime = EXOItem.Created.ToLocalTime().ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    createdTime = EXOItem.Created.ToLocalTime().ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(createdTime, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "DateTimeSent":
                            case "Send Time":
                            #region DateTimeSent
                                var dateTimeSend = string.Empty;
                                if (useFormat)
                                {
                                    dateTimeSend = EXOItem.SendDateUTC.ToLocalTime().ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    dateTimeSend = EXOItem.SendDateUTC.ToLocalTime().ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(dateTimeSend, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "DateTimeReceived":
                            case "Received Time":
                            #region DateTimeReceived
                                var dateTimeReceived = string.Empty;
                                if (useFormat)
                                {
                                    dateTimeReceived = EXOItem.Received.ToLocalTime().ToString(fieldInfo.DateFormat);
                                }
                                else
                                {
                                    dateTimeReceived = EXOItem.Received.ToLocalTime().ToString();
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(dateTimeReceived, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Sender":
                            case "Created By":
                            #region Sender
                                var sender = string.Empty;
                                if (!string.IsNullOrEmpty(EXOItem.Sender))
                                {
                                    //sender = message.Sender.Name;
                                    //sender = message.Sender.Address;
                                    sender = ExchangeUtils.EmailAddressToFormatString(EXOItem.Sender);
                                }
                                info = new MetaDataItemInfo(displayName, ProcessAdditionalField(sender, fieldInfo), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            default:
                            #region EXOItem ExtendedProperties
                                var extendedProperty = EXOItem.GetProperties()[mappedKey];
                                if (string.IsNullOrEmpty(extendedProperty))
                                {
                                    string val1 = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                    info = new MetaDataItemInfo(displayName, val1, true, typeof(string));
                                }
                                else
                                {
                                    info = new MetaDataItemInfo(displayName, extendedProperty.ToString(), true, typeof(string));
                                }
                                columnValue.Add(info);
                                break;
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get EXO nara csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                return columnValue;
            }
        }

        public List<MetaDataItemInfo> GetCSVListFromColumnValue(Folder EXOFolder, string disposalClass, string filePath)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXONARAExport_FolderGetCSVListFromColumnValue"))
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
                        bool useFormat = !string.IsNullOrEmpty(fieldInfo.DateFormat);
                        switch (mappedKey)
                        {
                            case "GUID":
                            case "ID":
                                info = new MetaDataItemInfo(displayName, EXOFolder.Id.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "FilePath":
                                string path = string.IsNullOrEmpty(fieldInfo.Prefix) ? filePath : Path.Combine(fieldInfo.Prefix, filePath);
                                info = new MetaDataItemInfo(displayName, path, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Disposal Class":
                            case "Disposal class":
                            case "Disposition Authority":
                                #region disposal class logic.
                                info = new MetaDataItemInfo(displayName, disposalClass, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            #endregion
                            case "Name":
                                info = new MetaDataItemInfo(displayName, EXOFolder.DisplayName, true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "UnreadCount":
                                info = new MetaDataItemInfo(displayName, EXOFolder.UnreadCount.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "TotalCount":
                                info = new MetaDataItemInfo(displayName, EXOFolder.TotalCount.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "ChildFolderCount":
                                info = new MetaDataItemInfo(displayName, EXOFolder.ChildFolderCount.ToString(), true, typeof(string));
                                columnValue.Add(info);
                                break;
                            case "Size":
                                info = new MetaDataItemInfo(displayName, "0 KB", true, typeof(string));
                                columnValue.Add(info);
                                break;
                            default:
                                string val1 = string.IsNullOrEmpty(fieldInfo.DefaultValue) ? string.Empty : fieldInfo.DefaultValue;
                                info = new MetaDataItemInfo(displayName, val1, true, typeof(string));
                                columnValue.Add(info);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        info = new MetaDataItemInfo(displayName, string.Empty, true, typeof(string));
                        mLog.Warn("get EXO nara csv column value faild {0},ERROR:{1}", displayName, ex.ToString());
                        columnValue.Add(info);
                    }
                }
                return columnValue;
            }
        }

        private string ProcessAdditionalField(string value, EXONARAFieldInfo EXONARAFieldInfo)
        {
            string additionalInfo = value;
            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(EXONARAFieldInfo.DefaultValue))
            {
                additionalInfo = EXONARAFieldInfo.DefaultValue;
            }
            if (!string.IsNullOrEmpty(EXONARAFieldInfo.Prefix))
            {
                additionalInfo = Path.Combine(EXONARAFieldInfo.Prefix, additionalInfo);
            }
            return additionalInfo;
        }
    }

    internal class EXONARAFieldInfo
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
