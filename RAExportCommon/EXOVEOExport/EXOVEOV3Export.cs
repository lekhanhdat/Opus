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
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Wrapper.Common;
using Microsoft.Exchange.WebServices.Data;
using Storage;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using static RAExportCommon.RecordVEOClassV3;
using ExchangeBackupUtility.Graph;

namespace RAExportCommon
{
    internal class EXOVEOV3Export : EXOExportBase, IEXOExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _lock = new object();
        private static string JobTimeStamp = string.Empty;
        private EXORecordVEODataV3 EXORecordVEODataV3;
        private NewEXORecordVEODataV3 NewEXORecordVEODataV3;
        internal EXOVEOContentXML EXOVEOContentXML = null;
        internal EXOVEOHistoryXML EXOVEOHistoryXML = null;
        private VEOContent VEOContent;
        private VEOHistory VEOHistory;
        private byte[] VEOContentBytes;
        private byte[] VEOHistoryBytes;
        private ICacheService CacheManager { get; set; }
        private XDirectoryInfo DestinationVEOFolder;
        private StorageInfo MailBoxesFolder;

        //Force export
        private ArchiverSetting ArchiverSetting;
        private long TotalSize;
        private int TotalCount;
        private int SubFolderCount;
        private string EncryptKey;

        public EXOVEOV3Export(PhysicalDeviceDto deviceDto, string jobId, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
            : base(deviceDto, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass(archiverSetting, veoContent, veoHistory, encryptKey);
        }

        private void InitClass(ArchiverSetting archiverSetting, byte[] veoContent, byte[] veoHistory, string encryptKey)
        {
            InitCacheManager();
            ArchiverSetting = archiverSetting ?? VEOV3CommonMethod.GetExportArchiverSetting(SourceFlag.Exchange);
            EXOVEOContentXML = InitEXOVEOContentXML(veoContent);
            EXOVEOHistoryXML = InitEXOVEOHistoryXML(veoHistory);
            EXORecordVEODataV3 = new EXORecordVEODataV3(EXOVEOContentXML);
            NewEXORecordVEODataV3 = new NewEXORecordVEODataV3(EXOVEOContentXML);
            VEOContent = new VEOContent { Version = VEOV3CommonString.VEO_VERSION, HashFunctionAlgorithm = VEOV3CommonString.ALGORITHM_SHA512 };
            EncryptKey = encryptKey;
        }

        public EXOVEOV3Export(SharePointLocationDto spoDto,AveBPOSAccountInfo user, string siteUrl, string jobId, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
            : base(spoDto, user, siteUrl, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass(archiverSetting, veoContent, veoHistory, encryptKey);
        }

        public EXOVEOV3Export(List<PhysicalDeviceDto> deviceDtos, string jobId, byte[] veoContent, byte[] veoHistory, ArchiverSetting archiverSetting, string encryptKey)
            : base(deviceDtos, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass(archiverSetting, veoContent, veoHistory, encryptKey);
        }

        public ExportStatus ExportEXOItem(Item EXOItem, EXOExportInfo info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ExportEXOItem"))
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export ExportEXOItem.", EXOItem.Id.ToString());
                try
                {
                    string msgFilePath = ExchangeUtils.GetEXOItemLocalMSGFilePath(JobId, EXOItem.Id.ToString(), info.service);
                    string exportFileName = string.Format("{0}_{1}.msg", Path.GetFileNameWithoutExtension(msgFilePath), JobTimeStamp);
                    string contentExportPath = Path.Combine(VEOV3CommonString.INBOX, exportFileName);
                    lock (_lock)
                    {
                        EXORecordVEODataV3.BuildVEOContentData(ref VEOContent, EXOItem, JobId, contentExportPath, info.MailFullPath, msgFilePath, info.DisposalClassString);
                        var exportDataFileStorageInfo = XConvert.FromNames(MailBoxesFolder.HighName, exportFileName);
                        using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
                        {
                            ExchangeUtils.GetEXOItemLocalMSGFileStream(msgFilePath).CopyTo(exportDataFileStream);
                            exportDataFileStream.Commit();
                            Interlocked.Increment(ref TotalCount);
                            Interlocked.Add(ref TotalSize, exportDataFileStream.Length);
                        }
                        exportStatus.State = ExportState.Succeed;
                        mLog.Info($"Added msg file into cache, path: {exportDataFileStorageInfo}.");
                        if (TotalSize >= VEOV3CommonMethod.CalculateGBSizeUnit((long)ArchiverSetting.FileSize) || TotalCount == ArchiverSetting.FileNumber)
                        {
                            ForceExportVEOZipFile(info);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export EXOItem.", info.MailFullPath, e);
                    return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e.Message.ToString() };
                }
                return exportStatus;
            }
        }

        public ExportStatus ExportEXOItem(IExchangeItem EXOItem, EXOExportInfoV2 info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ExportEXOItem"))
            {
                ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                JobTimeStamp = DateTime.Now.ToString("MMddyyHHmmssfff");
                mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export ExportEXOItem.", EXOItem.ItemId.ToString());
                try
                {
                    string msgFilePath = ExchangeUtils.GetEXOItemLocalMSGFilePath(JobId, EXOItem).ExecuteAsyncTask();
                    string exportFileName = string.Format("{0}_{1}.msg", Path.GetFileNameWithoutExtension(msgFilePath), JobTimeStamp);
                    string contentExportPath = Path.Combine(VEOV3CommonString.INBOX, exportFileName);
                    lock (_lock)
                    {
                        NewEXORecordVEODataV3.BuildVEOContentData(ref VEOContent, EXOItem, JobId, contentExportPath, info.MailFullPath, msgFilePath, info.DisposalClassString);
                        var exportDataFileStorageInfo = XConvert.FromNames(MailBoxesFolder.HighName, exportFileName);
                        using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
                        {
                            ExchangeUtils.GetEXOItemLocalMSGFileStream(msgFilePath).CopyTo(exportDataFileStream);
                            exportDataFileStream.Commit();
                            Interlocked.Increment(ref TotalCount);
                            Interlocked.Add(ref TotalSize, exportDataFileStream.Length);
                        }
                        exportStatus.State = ExportState.Succeed;
                        mLog.Info($"Added msg file into cache, path: {exportDataFileStorageInfo}.");
                        if (TotalSize >= VEOV3CommonMethod.CalculateGBSizeUnit((long)ArchiverSetting.FileSize) || TotalCount == ArchiverSetting.FileNumber)
                        {
                            ForceExportVEOZipFile(info);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export EXOItem.", info.MailFullPath, e);
                    return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e.Message.ToString() };
                }
                return exportStatus;
            }
        }

        public void ExtensionMethod(params object[] parameter)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXO:ExportVEOV3ZipFile"))
            {
                try
                {
                    if (TotalSize == 0 && TotalCount == 0) return;
                    mLog.Info(VaultLogFormat.LOG, "Start export VEO-V3 zip file to storage.");
                    ExportMandatoryVEOFilesIntoCache();
                    var zipVeoFilePath = Path.Combine(CacheManager.CacheSystem.SystemLocation, DestinationVEOFolder.Name + ".zip");
                    VEOV3CommonMethod.CreateVEOZipWithPassword(DestinationVEOFolder.ParentFullName, zipVeoFilePath, EncryptKey);
                    var storageInfo = XConvert.FromNames(string.Empty, Path.GetFileName(zipVeoFilePath));
                    var xStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.Open);
                    ExportInfo exportInfo = new ExportInfo();
                    EXOExportInfo expExportInfo = new EXOExportInfo()
                    {
                        FolderPath = JobId,
                        ContentFilePath = storageInfo.HighPlusLowName
                    };
                    var exportSize = RealVaultExport.ExportContent(exportInfo, expExportInfo, xStream).Size;
                    mLog.Info($"Export {storageInfo.HighPlusLowName} file to blob succeed. Volume: {VaultCover.ConverSizeFormat(exportSize, VaultCover.ConverSizeType.Normal)}");
                }
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export VEO V3 mandatory files. It is Export Service Error.", DestinationVEOFolder.HighName, e1.ToString());
                    throw;
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export VEO V3 mandatory files.", string.Empty, e2.Message);
                    throw;
                }
                finally
                {
                    var isFinished = bool.Parse(PathValidation.ConverSpecialChar(parameter[0].ToString()));
                    if (isFinished)
                    {
                        VEOV3CommonMethod.CleanCache(CacheManager.CacheSystem.SystemLocation);
                    }
                    else
                    {
                        TotalSize = TotalCount = 0;
                    }
                }
            }
        }

        private static EXOVEOContentXML InitEXOVEOContentXML(byte[] veoContentBytes)
        {
            EXOVEOContentXML veoContent = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(veoContentBytes))
                {
                    veoContent = (EXOVEOContentXML)new XmlSerializer(typeof(EXOVEOContentXML)).Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init File VEO XML,Message: {0}.", ex.ToString());
                throw new Exception("StorageOptimization_EXOVEOContentExportConfigFileDeserializeException");
            }
            return veoContent;
        }

        private static EXOVEOHistoryXML InitEXOVEOHistoryXML(byte[] veoHistoryBytes)
        {
            EXOVEOHistoryXML veoHistory = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(veoHistoryBytes))
                {
                    veoHistory = (EXOVEOHistoryXML)new XmlSerializer(typeof(EXOVEOHistoryXML)).Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init record VEO XML,Message: {0}.", ex.ToString());
                throw new Exception("StorageOptimization_EXOVEOHistoryExportConfigFileDeserializeException");
            }
            return veoHistory;
        }

        private void ForceExportVEOZipFile(EXOExportInfo info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXO:ExportVEOV3ZipFile"))
            {
                try
                {
                    mLog.Info(VaultLogFormat.LOG, "Start force export VEO-V3 zip file to storage.");
                    if (CurrentExportMode == ExportMode.Multile)
                    {
                        RealVaultExport = MultileVaultExport[info.DeviceDtoId];
                    }
                    ExportMandatoryVEOFilesIntoCache();
                    var zipVeoFilePath = Path.Combine(CacheManager.CacheSystem.SystemLocation, DestinationVEOFolder.Name + ".zip");
                    VEOV3CommonMethod.CreateVEOZipWithPassword(DestinationVEOFolder.ParentFullName, zipVeoFilePath, EncryptKey);
                    var storageInfo = XConvert.FromNames(string.Empty, Path.GetFileName(zipVeoFilePath));
                    var xStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.Open);
                    ExportInfo exportInfo = new ExportInfo();
                    EXOExportInfo expExportInfo = new EXOExportInfo()
                    {
                        FolderPath = JobId,
                        ContentFilePath = storageInfo.HighPlusLowName
                    };
                    var exportSize = RealVaultExport.ExportContent(exportInfo, expExportInfo, xStream).Size;
                    mLog.Info($"Force export {storageInfo.HighPlusLowName} file to blob succeed. Volume: {VaultCover.ConverSizeFormat(exportSize, VaultCover.ConverSizeType.Normal)}");
                }
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while force export VEO V3 mandatory files. It is Export Service Error.", DestinationVEOFolder.HighName, e1.ToString());
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while force export VEO V3 mandatory files.", string.Empty, e2.Message);
                }
                finally
                {
                    VEOContent = new VEOContent { Version = VEOV3CommonString.VEO_VERSION, HashFunctionAlgorithm = VEOV3CommonString.ALGORITHM_SHA512 };
                    TotalSize = TotalCount = 0;
                    Interlocked.Increment(ref SubFolderCount);
                    InitDestinationVEOFolder(SubFolderCount);
                    VEOV3CommonMethod.CleanCache(DestinationVEOFolder.ParentFullName);
                    VEOV3CommonMethod.CleanCache(Path.Combine(CacheManager.CacheSystem.SystemLocation, DestinationVEOFolder.Name + ".zip"));
                }
            }
        }

        private void ForceExportVEOZipFile(EXOExportInfoV2 info)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("EXO:ExportVEOV3ZipFile"))
            {
                try
                {
                    mLog.Info(VaultLogFormat.LOG, "Start force export VEO-V3 zip file to storage.");
                    if (CurrentExportMode == ExportMode.Multile)
                    {
                        RealVaultExport = MultileVaultExport[info.DeviceDtoId];
                    }
                    ExportMandatoryVEOFilesIntoCache();
                    var zipVeoFilePath = Path.Combine(CacheManager.CacheSystem.SystemLocation, DestinationVEOFolder.Name + ".zip");
                    VEOV3CommonMethod.CreateVEOZipWithPassword(DestinationVEOFolder.ParentFullName, zipVeoFilePath, EncryptKey);
                    var storageInfo = XConvert.FromNames(string.Empty, Path.GetFileName(zipVeoFilePath));
                    var xStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.Open);
                    ExportInfo exportInfo = new ExportInfo();
                    EXOExportInfo expExportInfo = new EXOExportInfo()
                    {
                        FolderPath = JobId,
                        ContentFilePath = storageInfo.HighPlusLowName
                    };
                    var exportSize = RealVaultExport.ExportContent(exportInfo, expExportInfo, xStream).Size;
                    mLog.Info($"Force export {storageInfo.HighPlusLowName} file to blob succeed. Volume: {VaultCover.ConverSizeFormat(exportSize, VaultCover.ConverSizeType.Normal)}");
                }
                catch (ExportServiceException e1)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while force export VEO V3 mandatory files. It is Export Service Error.", DestinationVEOFolder.HighName, e1.ToString());
                }
                catch (Exception e2)
                {
                    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while force export VEO V3 mandatory files.", string.Empty, e2.Message);
                }
                finally
                {
                    VEOContent = new VEOContent { Version = VEOV3CommonString.VEO_VERSION, HashFunctionAlgorithm = VEOV3CommonString.ALGORITHM_SHA512 };
                    TotalSize = TotalCount = 0;
                    Interlocked.Increment(ref SubFolderCount);
                    InitDestinationVEOFolder(SubFolderCount);
                    VEOV3CommonMethod.CleanCache(DestinationVEOFolder.ParentFullName);
                    VEOV3CommonMethod.CleanCache(Path.Combine(CacheManager.CacheSystem.SystemLocation, DestinationVEOFolder.Name + ".zip"));
                }
            }
        }

        private void ExportMandatoryVEOFilesIntoCache()
        {
            ExportVEOContentXmlFile();
            ExportVEOHistoryXmlFile();
            ExportVEOSignatureFiles();
            var exportDataFileStorageInfo = XConvert.FromNames(DestinationVEOFolder.HighName, VEOV3CommonString.VEOReadme);
            var destination = SecurityUtils.SafeCombinePath(CacheManager.CacheSystem.SystemLocation, exportDataFileStorageInfo.HighPlusLowName);
            VEOV3CommonMethod.ExportVEOReadmeFile(destination);
        }

        private void ExportVEOContentXmlFile()
        {
            XmlAttributeOverrides aor = new XmlAttributeOverrides();
            XmlAttributes templateAttribs = new XmlAttributes();
            templateAttribs.XmlElements.Add(new XmlElementAttribute(typeof(Record)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            templateAttribs.XmlElements.Add(new XmlElementAttribute(typeof(Agent)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            templateAttribs.XmlElements.Add(new XmlElementAttribute(typeof(Relationship)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            templateAttribs.XmlElements.Add(new XmlElementAttribute(typeof(BaseRDFTemplate)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            aor.Add(typeof(AnzsDescription), "Template", templateAttribs);
            aor.Add(typeof(RelationshipRelatedEntity), "Template", templateAttribs);

            XmlAttributes descAttribs = new XmlAttributes();
            descAttribs.XmlElements.Add(new XmlElementAttribute("Description"));
            aor.Add(typeof(RDF), typeof(AnzsDescription).Name, descAttribs);
            aor.Add(typeof(RDF), typeof(AglsFromVERS2Description).Name, descAttribs);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            XmlSerializer xs = new XmlSerializer(typeof(VEOContent), aor);
            ns.Add("vers", "http://www.prov.vic.gov.au/VERS");
            using (Stream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, VEOContent, ns);
                memStream.Position = 0;
                using (Stream tempStream = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument() { XmlResolver = null };
                    try
                    {
                        doc.Load(memStream);
                        doc.Save(tempStream);
                        tempStream.Seek(0, SeekOrigin.Begin);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            tempStream.CopyTo(ms);
                            tempStream.Position = 0;
                            VEOContentBytes = ms.ToArray();
                        }
                        var storageInfo = XConvert.FromNames(DestinationVEOFolder.HighName, VEOV3CommonString.VEOContent);
                        using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(exportDataFileStream);
                            exportDataFileStream.Commit();
                            mLog.Info($"Export {VEOV3CommonString.EXOVEOContent} into cache succeed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"An error occurred while export {VEOV3CommonString.EXOVEOContent} into cache. Error: {ex.Message}.");
                    }
                }
            }
        }

        private void ExportVEOHistoryXmlFile()
        {
            EXORecordVEODataV3 exoRecordVEODataV3 = new EXORecordVEODataV3(EXOVEOHistoryXML);
            VEOHistory = new VEOHistory { Version = VEOV3CommonString.VEO_VERSION };
            exoRecordVEODataV3.BuildVEOHistoryData(ref VEOHistory);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/VERS");

            XmlSerializer xs = new XmlSerializer(typeof(VEOHistory));
            using (Stream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, VEOHistory, ns);
                memStream.Position = 0;
                using (Stream tempStream = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument() { XmlResolver = null };
                    try
                    {
                        doc.Load(memStream);
                        doc.Save(tempStream);
                        tempStream.Seek(0, SeekOrigin.Begin);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            tempStream.CopyTo(ms);
                            tempStream.Position = 0;
                            VEOHistoryBytes = ms.ToArray();
                        }
                        var storageInfo = XConvert.FromNames(DestinationVEOFolder.HighName, VEOV3CommonString.VEOHistory);
                        using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(exportDataFileStream);
                            exportDataFileStream.Commit();
                            mLog.Info($"Export {VEOV3CommonString.EXOVEOHistory} into cache succeed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"An error occurred while export {VEOV3CommonString.EXOVEOHistory} into cache. Error: {ex.Message}.");

                    }
                }
            }
        }

        private void ExportVEOSignatureFiles()
        {
            ProcessExportVEOSignatureFiles(VEOV3CommonMethod.BuildVEOSignature(VEOContentBytes), VEOV3CommonString.VEOContentSignature);
            ProcessExportVEOSignatureFiles(VEOV3CommonMethod.BuildVEOSignature(VEOHistoryBytes), VEOV3CommonString.VEOHistorySignature);
        }

        private void ProcessExportVEOSignatureFiles<T>(T signatureData, string fileName) where T : class
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/VERS");

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (Stream memStream = new MemoryStream())
            {
                serializer.Serialize(memStream, signatureData, ns);
                memStream.Position = 0;
                using (Stream tempStream = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(memStream);
                        doc.XmlResolver = null;
                        doc.Save(tempStream);
                        tempStream.Seek(0, SeekOrigin.Begin);
                        var storageInfo = XConvert.FromNames(DestinationVEOFolder.HighName, fileName);
                        using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(exportDataFileStream);
                            exportDataFileStream.Commit();
                            mLog.Info($"Export {fileName} into cache succeed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"An error occurred while export {fileName} into cache. Error: {ex.Message}.");
                    }
                }
            }
        }

        private void InitCacheManager()
        {
            var cacheSetting = VEOV3CommonMethod.GenerateCacheSettings(JobId);
            CacheManager ??= PlatformWindsorManager.GetService<ICacheService>();
            CacheManager.Open(cacheSetting, false);
            CacheManager.CacheSystem.Open();
            InitDestinationVEOFolder(SubFolderCount);
        }

        private void InitDestinationVEOFolder(int subVEOFolderCount)
        {
            var tempRestoreFolder = JobId + "_" + DateTime.UtcNow.Ticks + "_" + subVEOFolderCount.ToString("D3") + VEOV3CommonString.VEO_V3_FOLDERPOSTFIX;
            var restoreTempInfo = new StorageInfo { HighName = Path.Combine(tempRestoreFolder, tempRestoreFolder), LowName = String.Empty };
            DestinationVEOFolder = CacheManager.CacheSystem.OpenDirectory(restoreTempInfo, FileMode.OpenOrCreate);
            MailBoxesFolder = new StorageInfo { HighName = Path.Combine(DestinationVEOFolder.HighName, VEOV3CommonString.INBOX), LowName = String.Empty };
            CacheManager.CacheSystem.OpenDirectory(MailBoxesFolder, FileMode.OpenOrCreate);
        }

        #region Useless methods

        public ExportStatus ExportEXOMailBox(Mailbox EXOMailbox, EXOExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Succeed };
        }

        public ExportStatus ExportEXOFolder(Folder EXOFolder, EXOExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Succeed };
        }

        List<CsvMetaData> IEXOExport.GetCSVMetadata()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
