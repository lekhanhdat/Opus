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
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Contract.Storage.Entity;
using System.IO;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.Common;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Wrapper.Backup;
using ADDTAGRESOURCE = Merged18NResources.Archive.ResourceFileForArchiver;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;

namespace RAExportCommon
{
    internal class VEOExport : VaultExportBase, IVaultExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private OrdinalIgnoreCaseStringComparison stringComparison = new OrdinalIgnoreCaseStringComparison();
        private const string SKIPMESSAGE = "StorageOptimization_VEOOnlyExportDocumentLibrary";
        internal GeneratorManifest manifest = null;
        private string JobTimeStamp = string.Empty;
        internal FileVEOXML fileVEOXML = null;
        internal RecordVEOXML recordVEOXML = null;
        internal ManifestVEOXML manifestXML = null;

        //需要支持客户自定义操作，增加三个参数fileVEO，recordVEO，manifestVEO,当客户没有自定义内容时，三个参数的值都为null
        public VEOExport(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV) 
            : base(deviceDto, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(fileVEO, recordVEO, manifestVEO, deviceDto);
        }

        private void InitClass(byte[] fileVeo, byte[] recordVeo, byte[] manifestVeo, PhysicalDeviceDto deviceDto)
        {
            fileVEOXML = InitFileVEOXML(fileVeo);
            recordVEOXML = InitRecordVEOXML(recordVeo);
            manifestXML = InitManifestVEOXML(manifestVeo);
            manifest = new GeneratorManifest(manifestXML, deviceDto);
        }

        public VEOExport(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV) 
            : base(spoDto, user, siteUrl, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(fileVEO, recordVEO, manifestVEO, null);
        }

        public VEOExport(List<PhysicalDeviceDto> deviceDtos, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
            : base(deviceDtos, jobId, format, encryptionKey, encryptionIV)
        {
            fileVEOXML = InitFileVEOXML(fileVEO);
            recordVEOXML = InitRecordVEOXML(recordVEO);
            manifest = new GeneratorManifest(manifestXML);
        }

        public ExportStatus ExportSite(AveSPSite aveSite, VaultExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Succeed };
        }

        public ExportStatus ExportWeb(AveSPWeb aveWeb, VaultExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Succeed };
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportList(AveSPList aveList, VaultExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportList"))
                {
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export List.", aveList.ParentWeb.SPWeb.Url + aveList.Path);
                    JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                    FileVEODate mFileVEODate = new FileVEODate();
                    string crtime = aveList.SPList.Created.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    string createdBy = string.Empty;
                    //office 365 不支持此API，author为null
                    if (aveList.SPList.Author == null)
                    {
                        createdBy = string.Empty;
                    }
                    else
                    {
                        if (aveList.SPList.Author.LoginName.Contains("|"))
                        {
                            int index = aveList.SPList.Author.LoginName.IndexOf('|');
                            createdBy = aveList.SPList.Author.LoginName.Substring(index + 1, aveList.SPList.Author.LoginName.Length - index - 1);
                        }
                        else
                        {
                            createdBy = aveList.SPList.Author.LoginName;
                        }
                    }

                    FileVEOParameters paras = new FileVEOParameters(
                            aveList.Title,
                            aveList.Id.ToString() + "_" + aveList.Title + "_" + JobTimeStamp,
                            "",
                            crtime,
                            null,
                            createdBy,
                            null
                            );
                    FileVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = mFileVEODate.GeneratorVEOData(fileVEOXML, paras, false, null, aveList);

                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
                    ns.Add("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");

                    XmlSerializer xs = new XmlSerializer(typeof(FileVEOClass.VERSEncapsulatedObject));
                    using (Stream memStream = new MemoryStream())
                    {
                        xs.Serialize(memStream, mVERSEncapsulatedObject, ns);
                        memStream.Position = 0;
                        using (Stream tempStream = new MemoryStream())
                        {
                            XmlDocument doc = new XmlDocument();
                            try
                            {
                                doc.Load(memStream);
                                doc.XmlResolver = null;
                                doc.InsertBefore(doc.CreateDocumentType("vers:VERSEncapsulatedObject", null, "vers.dtd", null), doc.DocumentElement);
                                doc.Save(tempStream);
                                tempStream.Seek(0, SeekOrigin.Begin);
                                string name = NameFactory.GetName(info.ContentFilePath);
                                string extensionName = NameFactory.GetExtensionName(info.ContentFilePath);
                                info.ContentFilePath = string.Format("{0}_{1}.{2}", name, JobTimeStamp, extensionName);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occurred while changing content file path." + e.ToString());
                            }
                            manifest.AddItem(doc, info.ContentFilePath, (((long)tempStream.Length) / 1024).ToString());
                            ExportInfo contentInfo = new ExportInfo();
                            exportStatus.ExportSize += RealVaultExport.ExportContent(contentInfo, info, tempStream).Size;
                        }
                    }

                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export list.It is Export Service Error.", aveList.Path, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export list.", aveList.Path, e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }

            return exportStatus;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportFolder(AveSPFolder aveFolder, VaultExportInfo info, bool isRootFolder)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            if (aveFolder == null)
            {
                mLog.Warn(VaultLogFormat.LOG, "The folder instance is null");
                //exportStatus.ErrorMessage = LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException;
                return exportStatus;
            }

            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportFolder"))
                {
                    if (isRootFolder || aveFolder.AveItem == null)
                    {
                        exportStatus.State = ExportState.Skipped;
                        return exportStatus;
                    }
                    //JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                    FileVEODate mFileVEODate = new FileVEODate();

                    string crtime = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    string motime = ((DateTime)aveFolder.SPFolder.Item["Modified"]).ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");

                    string mAuthor = GetUserTitle(aveFolder.SPFolder.Item["Author"].ToString());
                    string mEditor = GetUserTitle(aveFolder.SPFolder.Item["Editor"].ToString());

                    FileVEOParameters paras = new FileVEOParameters(
                            aveFolder.AveItem.Title,
                            aveFolder.AveItem.AveSPList.Id.ToString() + "_" + aveFolder.AveItem.AveSPList.Title + "_" + JobTimeStamp,
                            String.Empty,
                            crtime,
                            motime,
                            mAuthor,
                            mEditor);

                    paras.VParentVEOID =
                        aveFolder.AveList.ServerRelativeUrl == aveFolder.AveParentFolder.ServerRelativeUrl
                        ?
                        aveFolder.AveList.Id.ToString() + "_" + aveFolder.AveList.Title + "_" + JobTimeStamp
                        :
                        aveFolder.AveParentFolder.AveItem.Id.ToString() + "_" + aveFolder.AveParentFolder.AveItem.Title + "_" + JobTimeStamp;


                    FileVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = mFileVEODate.GeneratorVEOData(fileVEOXML, paras, true, aveFolder, null);

                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
                    ns.Add("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");

                    XmlSerializer xs = new XmlSerializer(typeof(FileVEOClass.VERSEncapsulatedObject));
                    using (Stream memStream = new MemoryStream())
                    {
                        xs.Serialize(memStream, mVERSEncapsulatedObject, ns);
                        memStream.Position = 0;
                        using (Stream tempStream = new MemoryStream())
                        {
                            XmlDocument doc = new XmlDocument();
                            try
                            {
                                doc.Load(memStream);
                                doc.XmlResolver = null;
                                doc.InsertBefore(doc.CreateDocumentType("vers:VERSEncapsulatedObject", null, "vers.dtd", null), doc.DocumentElement);
                                doc.Save(tempStream);
                                tempStream.Seek(0, SeekOrigin.Begin);
                                string name = NameFactory.GetName(info.ContentFilePath);
                                string extensionName = NameFactory.GetExtensionName(info.ContentFilePath);
                                info.ContentFilePath = string.Format("{0}_{1}.{2}", name, JobTimeStamp, extensionName);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occurred while changing content file path." + e.ToString());
                            }
                            manifest.AddItem(doc, info.ContentFilePath, (((long)tempStream.Length) / 1024).ToString());
                            ExportInfo contentInfo = new ExportInfo();
                            exportStatus.ExportSize += RealVaultExport.ExportContent(contentInfo, info, tempStream).Size;
                        }
                    }

                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.It is Export Service Error.", FullURL.GetItemFullUrl(aveFolder, false), e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.", FullURL.GetItemFullUrl(aveFolder, false), e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }

            return exportStatus;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportDocOrDocVersion(AveSPDoc aveDoc, VaultExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportDocOrDocVersion"))
                {
                    if (CurrentExportMode == ExportMode.Multile)
                    {
                        RealVaultExport = MultileVaultExport[info.DeviceDtoId];
                    }
                    if (aveDoc == null)
                    {
                        mLog.Warn(VaultLogFormat.LOG, "The doc instance is null");
                        //exportStatus.ErrorMessage = LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException;
                        return exportStatus;
                    }
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Doc Or DocVersion.", aveDoc.AveSPItem.Id);

                    //if (!IsMatchVeoFormat(aveDoc.AveSPItem.SPListItem.Name))
                    //{
                    //    return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = "The file type not a VEO export format, The veo only export Word,Excel,PowerPoint and PDF." };
                    //}

                    //if (!aveDoc.AveSPItem.SPListItem.Versions.GetVersionFromID(aveDoc.AveSPItem.Version).IsCurrentVersion)
                    //{
                    //    return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = "The veo export only support current version." };
                    //}
                    if (!aveDoc.AveSPItem.SPListItem.File.Exists || aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                    {
                        return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = "StorageOptimization_VEOExportOnlySupportCurrentVersion" };
                    }
                    //JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                    RecordVEOData veodata = new RecordVEOData();

                    List<UsageConditionChange> mUsageConditionChanges = new List<UsageConditionChange>();
                    try
                    {
                        string tempValue = string.Empty;
                        foreach (var v in aveDoc.AveSPItem.SPListItem.Versions.OrderBy(i => i.VersionId))
                        {
                            //if (v.VersionId == aveDoc.AveSPItem.Version)
                            //{
                            //    continue;
                            //}
                            if (v.Fields.ContainsField("Disseminated Line Marker"))
                            {
                                //IAveField cField = v.Fields.GetField("Disseminated Line Marker");
                                object fileObject = v["Disseminated Line Marker"];
                                string cFieldValue = string.Empty;
                                if (fileObject == null)
                                {
                                    mLog.Warn("Get [Disseminated Line Marker] field failed,field is null");
                                }
                                else
                                {
                                    cFieldValue = fileObject.ToString();
                                }
                                if (!string.IsNullOrEmpty(cFieldValue))
                                {
                                    cFieldValue = cFieldValue.Split('|')[0];
                                }

                                if (cFieldValue != tempValue)
                                {
                                    try
                                    {
                                        string title = string.Empty;
                                        try
                                        {
                                            string[] sArray = v["Editor"].ToString().Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
                                            title = sArray[1].Trim(',');
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("An error occurred while getting author column.url:{0},version:{1},error is:{2}", v.Url, v.VersionId, e.ToString());
                                        }
                                        UsageConditionChange ucc = new UsageConditionChange(cFieldValue, v.Created.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz"), title);
                                        mUsageConditionChanges.Add(ucc);
                                        tempValue = cFieldValue;
                                    }
                                    catch (Exception e)
                                    {
                                        //log
                                        mLog.Warn("An error occurred while getting UsageConditionChange,url:{0},version:{1},error is:{2}", v.Url, v.VersionId, e.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while getting UsageConditionChange,url is :{0} error is {1}", FullURL.GetItemFullUrl(aveDoc), e.ToString());
                    }

                    RecordVEOParameters paras = new RecordVEOParameters(
                        aveDoc.AveSPItem.Id.ToString(),
                        FullURL.GetItemFullUrl(aveDoc),
                        aveDoc.AveSPItem.SPListItem.Name,
                        aveDoc.AveSPItem.AveSPList.Id.ToString() + "_" + aveDoc.AveSPItem.AveSPList.Title + "_" + JobTimeStamp,
                        aveDoc.AveSPWeb.SPWeb.LanguageCulture.Name,
                        aveDoc.AveSPItem.GetColumnValues(AvePoint.Wrapper.Backup.ColumnsLevel.AllVisiableColumns),
                        aveDoc.AveSPItem.SPListItem.ContentType.Name, recordVEOXML);
                    paras.VLibraryName = aveDoc.AveSPItem.AveSPList.Title;
                    paras.VUsageConditionChanges = mUsageConditionChanges;

                    paras.VParenetFileIdentifier =
                    aveDoc.AveSPItem.AveSPList.ServerRelativeUrl ==
                    aveDoc.ParentFolder.ServerRelativeUrl
                    ?
                    aveDoc.ParentFolder.AveList.Id.ToString() + "_" + aveDoc.ParentFolder.AveList.Title + "_" + JobTimeStamp
                    :
                    aveDoc.ParentFolder.AveItem.Id.ToString() + "_" + aveDoc.ParentFolder.AveItem.Title + "_" + JobTimeStamp;
                    RecordVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = veodata.GeneratorVEOData(recordVEOXML, paras, aveDoc);

                    //VerfiyVEO(mVERSEncapsulatedObject);

                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("vers", "http://www.prov.vic.gov.au/gservice/standard/pros99007.htm");
                    ns.Add("naa", "http://www.naa.gov.au/recordkeeping/control/rkms/contents.html");

                    XmlSerializer xs = new XmlSerializer(typeof(RecordVEOClass.VERSEncapsulatedObject));
                    using (Stream memStream = new MemoryStream())
                    {
                        xs.Serialize(memStream, mVERSEncapsulatedObject, ns);
                        memStream.Position = 0;
                        using (Stream tempStream = new MemoryStream())
                        {
                            XmlDocument doc = new XmlDocument();
                            try
                            {
                                doc.Load(memStream);
                                doc.XmlResolver = null;
                                doc.InsertBefore(doc.CreateDocumentType("vers:VERSEncapsulatedObject", null, "vers.dtd", null), doc.DocumentElement);
                                doc.Save(tempStream);
                                tempStream.Seek(0, SeekOrigin.Begin);
                                string name = NameFactory.GetName(info.ContentFilePath);
                                string extensionName = NameFactory.GetExtensionName(info.ContentFilePath);
                                info.ContentFilePath = string.Format("{0}_{1}.{2}", name, JobTimeStamp, extensionName);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occurred while changing content file path." + e.ToString());
                            }
                            manifest.AddItem(doc, info.ContentFilePath, (((long)tempStream.Length) / 1024).ToString());
                            ExportInfo contentInfo = new ExportInfo();
                            exportStatus.ExportSize += RealVaultExport.ExportContent(contentInfo, info, tempStream).Size;
                        }
                    }
                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.It is Export Service Error.", FullURL.GetItemFullUrl(aveDoc), e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.", FullURL.GetItemFullUrl(aveDoc), e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }

            return exportStatus;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportItemOrItemVersion(AveSPListItem aveListItem, VaultExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = SKIPMESSAGE };
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportAttachment(AveSPAttachment aveAttachment, VaultExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = SKIPMESSAGE };
        }

        public void ExtensionMethod(params object[] parameter)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExtensionMethod"))
            {
                string name = PathValidation.ConverSpecialChar(parameter[0].ToString());
                using (Stream manifestStream = manifest.GenerateManifestStream(JobId))
                {
                    if (manifestStream != null)
                    {
                        mLog.Info("begin export manifest file.");
                        ExportInfo contentInfo = new ExportInfo();
                        VaultExportInfo exportInfo = new VaultExportInfo();
                        exportInfo.FolderPath = JobId;
                        //Change manifest.xml name.
                        exportInfo.ContentFilePath = "manifest.xml";
                        ExportResultInfo result = RealVaultExport.ExportContent(contentInfo, exportInfo, manifestStream);
                    }
                    else
                    {
                        mLog.Info("Because there isn't exported any file,so manifest file not be generate.");
                    }
                }
            }
        }

        #region Private Method
        private string GetUserTitle(string name)
        {
            string[] sArray = name.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
            return sArray[1].ToString();
        }

        private static FileVEOXML InitFileVEOXML(byte[] fileVEO)
        {
            FileVEOXML fileVEOXML = null;
            try
            {
                byte[] fileVEOArray = fileVEO;
                using (MemoryStream fileVEOStream = new MemoryStream(fileVEOArray))
                {
                    fileVEOXML = (FileVEOXML)new XmlSerializer(typeof(FileVEOXML)).Deserialize(fileVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init File VEO XML,Message: {0}.", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_FileVEOExportConfigFileDeserializeException");
            }
            return fileVEOXML;
        }

        private static RecordVEOXML InitRecordVEOXML(byte[] recordVEO)
        {
            RecordVEOXML recordVEOXML = null;
            try
            {
                byte[] recordVEOArray = recordVEO;
                using (MemoryStream recordVEOStream = new MemoryStream(recordVEOArray))
                {
                    recordVEOXML = (RecordVEOXML)new XmlSerializer(typeof(RecordVEOXML)).Deserialize(recordVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init record VEO XML,Message: {0}.", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_RecordVEOExportConfigFileDeserializeException");
            }
            return recordVEOXML;
        }

        private static ManifestVEOXML InitManifestVEOXML(byte[] manifestVEO)
        {
            ManifestVEOXML manifestVEOXML = null;
            try
            {
                byte[] manifestVEOArray = manifestVEO;
                using (MemoryStream manifestVEOStream = new MemoryStream(manifestVEOArray))
                {
                    manifestVEOXML = (ManifestVEOXML)new XmlSerializer(typeof(ManifestVEOXML)).Deserialize(manifestVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init Manifest VEO XML,Message: {0}.", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_ManifestExportConfigFileDeserializeException");
            }
            return manifestVEOXML;
        }

        public List<CsvMetaData> GetCSVMetadata()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
