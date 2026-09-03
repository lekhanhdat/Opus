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
using AvePoint.Wrapper.Backup;
using ADDTAGRESOURCE = Merged18NResources.Archive.ResourceFileForArchiver;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using Microsoft.Exchange.WebServices.Data;
using ExchangeBackupUtility.Graph;

namespace RAExportCommon
{
    internal class EXOVEOExport : EXOExportBase, IEXOExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //private OrdinalIgnoreCaseStringComparison stringComparison = new OrdinalIgnoreCaseStringComparison();

        internal GeneratorManifest manifest = null;
        private string JobTimeStamp = string.Empty;
        internal EXOFileVEOXML fileVEOXML = null;
        internal EXORecordVEOXML recordVEOXML = null;
        internal ManifestVEOXML manifestXML = null;

        //需要支持客户自定义操作，增加三个参数fileVEO，recordVEO，manifestVEO,当客户没有自定义内容时，三个参数的值都为null
        public EXOVEOExport(PhysicalDeviceDto deviceDto, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
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

        public EXOVEOExport(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
            : base(spoDto, user, siteUrl, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(fileVEO, recordVEO, manifestVEO, null);
        }

        public EXOVEOExport(List<PhysicalDeviceDto> deviceDtos, string jobId, VaultExportFormat format, byte[] fileVEO, byte[] recordVEO, byte[] manifestVEO, byte[] encryptionKey, byte[] encryptionIV)
            : base(deviceDtos, jobId, format,encryptionKey, encryptionIV)
        {
            fileVEOXML = InitFileVEOXML(fileVEO);
            recordVEOXML = InitRecordVEOXML(recordVEO);
            manifest = new GeneratorManifest(manifestXML);
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
                        EXOExportInfo exportInfo = new EXOExportInfo();
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

        private static EXOFileVEOXML InitFileVEOXML(byte[] fileVEO)
        {
            EXOFileVEOXML fileVEOXML = null;
            try
            {
                byte[] fileVEOArray = fileVEO;
                using (MemoryStream fileVEOStream = new MemoryStream(fileVEOArray))
                {
                    fileVEOXML = (EXOFileVEOXML)new XmlSerializer(typeof(EXOFileVEOXML)).Deserialize(fileVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init File VEO XML,Message: {0}.", ex.ToString());
                throw new Exception("StorageOptimization_EXOFileVEOExportConfigFileDeserializeException");
            }
            return fileVEOXML;
        }

        private static EXORecordVEOXML InitRecordVEOXML(byte[] recordVEO)
        {
            EXORecordVEOXML recordVEOXML = null;
            try
            {
                byte[] recordVEOArray = recordVEO;
                using (MemoryStream recordVEOStream = new MemoryStream(recordVEOArray))
                {
                    recordVEOXML = (EXORecordVEOXML)new XmlSerializer(typeof(EXORecordVEOXML)).Deserialize(recordVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init record VEO XML,Message: {0}.", ex.ToString());
                throw new Exception("StorageOptimization_EXORecordVEOExportConfigFileDeserializeException");
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
                throw new Exception("StorageOptimization_EXOManifestExportConfigFileDeserializeException");
            }
            return manifestVEOXML;
        }

        public ExportStatus ExportEXOMailBox(Mailbox EXOMailbox, EXOExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Succeed };
        }

        public ExportStatus ExportEXOFolder(Folder EXOFolder, EXOExportInfo info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ExportEXOFolder"))
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                try
                {
                    JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export folder.", EXOFolder.Id.ToString());
                    EXOFileVEOData mFileVEODate = new EXOFileVEOData();
                    string crtime = string.Empty;//.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    string motime = string.Empty;
                    string mAuthor = string.Empty;
                    string mEditor = string.Empty;

                    EXOFileVEOParameters paras = new EXOFileVEOParameters(
                            string.Empty,
                            string.Empty,
                            String.Empty,
                            crtime,
                            motime,
                            mAuthor,
                            mEditor);

                    paras.VParentVEOID = string.Empty;
                    FileVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = mFileVEODate.GeneratorVEOData(fileVEOXML, paras, EXOFolder);

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
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.It is Export Service Error.", EXOFolder.Id.ToString());
                    throw;
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.", EXOFolder.Id.ToString());
                    return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
                }

                return exportStatus;
            }
        }

        public ExportStatus ExportEXOItem(Item EXOItem, EXOExportInfo info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ExportEXOItem"))
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                if (CurrentExportMode == ExportMode.Multile)
                {
                    RealVaultExport = MultileVaultExport[info.DeviceDtoId];
                }
                JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export ExportEXOItem.", EXOItem.Id.ToString());
                EXORecordVEOData veodata = new EXORecordVEOData();
                List<UsageConditionChange> mUsageConditionChanges = new List<UsageConditionChange>();
                try
                {
                    EXORecordVEOParameters paras = new EXORecordVEOParameters(
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        new Dictionary<string, object>()
                        );
                    paras.VLibraryName = string.Empty;
                    paras.VUsageConditionChanges = mUsageConditionChanges;
                    paras.VParenetFileIdentifier = string.Empty;
                    string name = NameFactory.GetName(info.ContentFilePath);
                    string extensionName = NameFactory.GetExtensionName(info.ContentFilePath);
                    info.ContentFilePath = string.Format("{0}_{1}.{2}", name, JobTimeStamp, extensionName);
                    string exportPath = Path.Combine(info.FolderPath, info.ContentFilePath);
                    RecordVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = veodata.GeneratorVEOData(recordVEOXML, paras, EXOItem, info.JobID, exportPath, info.MailFullPath, info.DisposalClassString);
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
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.It is Export Service Error.", EXOItem.Id.ToString(), e1.ToString());
                    throw;
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.", EXOItem.Id.ToString(), e2.ToString());
                    return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
                }
                return exportStatus;
            }
        }

        public ExportStatus ExportEXOItem(IExchangeItem EXOItem, EXOExportInfoV2 info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ExportEXOItem"))
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                if (CurrentExportMode == ExportMode.Multile)
                {
                    RealVaultExport = MultileVaultExport[info.DeviceDtoId];
                }
                JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export ExportEXOItem.", EXOItem.ItemId.ToString());
                EXORecordVEODataV2 veodata = new EXORecordVEODataV2();
                List<UsageConditionChange> mUsageConditionChanges = new List<UsageConditionChange>();
                try
                {
                    EXORecordVEOParameters paras = new EXORecordVEOParameters(
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        new Dictionary<string, object>()
                        );
                    paras.VLibraryName = string.Empty;
                    paras.VUsageConditionChanges = mUsageConditionChanges;
                    paras.VParenetFileIdentifier = string.Empty;
                    string name = NameFactory.GetName(info.ContentFilePath);
                    string extensionName = NameFactory.GetExtensionName(info.ContentFilePath);
                    info.ContentFilePath = string.Format("{0}_{1}.{2}", name, JobTimeStamp, extensionName);
                    string exportPath = Path.Combine(info.FolderPath, info.ContentFilePath);
                    RecordVEOClass.VERSEncapsulatedObject mVERSEncapsulatedObject = veodata.GeneratorVEOData(recordVEOXML, paras, EXOItem, info.JobID, exportPath, info.MailFullPath, info.DisposalClassString);
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
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.It is Export Service Error.", EXOItem.ItemId.ToString(), e1.ToString());
                    throw;
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.", EXOItem.ItemId.ToString(), e2.ToString());
                    return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
                }
                return exportStatus;
            }
        }

        public List<CsvMetaData> GetCSVMetadata()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
