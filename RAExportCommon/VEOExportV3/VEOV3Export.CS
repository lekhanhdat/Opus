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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using Storage;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;

namespace RAExportCommon
{
    internal class VEOV3Export : VaultExportBase, IVaultExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType);
        private const string SKIPMESSAGE = "StorageOptimization_VEOOnlyExportDocumentLibrary";
        private string JobTimeStamp = string.Empty;
        internal VEOContentXML? contentVEOXML = null;
        internal VEOHistoryXML? historyVEOXML = null;
        internal VEOSignatureXML? signatureVEOXML = null;
        internal RecordVEOClassV3.VEOContent VEOContentObj;
        internal RecordVEOClassV3.VEOHistory VEOHistoryObj;
        private PolicyLevel _ruleLevel;
        private int totalCount = 0;
        private long totalSize = 0;
        private int cacheVEOFolderCount = 0;
        private XDirectoryInfo currentVEOFolder = new();
        private Dictionary<Guid, int> spoOjbectDepthCache = [];
        private Dictionary<Guid, RecordVEOParametersV3> parentIOCache = [];
        private bool needReGenerateIOs = false;
        private ArchiverSetting ArchiverSetting;
        private ICacheService CacheManager = PlatformWindsorManager.GetService<ICacheService>();
        private string EncryptKey;

        public VEOV3Export(PhysicalDeviceDto deviceDto, string jobId, int ruleLevel, ArchiverSetting archiverSetting, byte[] contentVEO, byte[] historyVEO, string encryptKey)
            : base(deviceDto, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass((PolicyLevel)ruleLevel, archiverSetting, contentVEO, historyVEO, encryptKey);
        }

        private void InitClass(PolicyLevel ruleLevel, ArchiverSetting? archiverSetting, byte[] contentVeo, byte[] historyVeo, string encryptKey)
        {
            _ruleLevel = ruleLevel;
            OpenCache();
            ArchiverSetting = archiverSetting ?? VEOV3CommonMethod.GetExportArchiverSetting(SourceFlag.SharePoint);
            contentVEOXML = InitContentVEOXML(contentVeo);
            historyVEOXML = InitHistoryVEOXML(historyVeo);
            VEOContentObj = RecordVEODataV3.InitVEOContent(contentVEOXML);
            VEOHistoryObj = RecordVEODataV3.InitVEOHistory(historyVEOXML);
            EncryptKey = encryptKey;
        }

        public VEOV3Export(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, int ruleLevel, ArchiverSetting archiverSetting, byte[] contentVEO, byte[] historyVEO, string encryptKey)
            : base(spoDto, user, siteUrl, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass((PolicyLevel)ruleLevel, archiverSetting, contentVEO, historyVEO, encryptKey);
        }

        public VEOV3Export(List<PhysicalDeviceDto> deviceDtos, string jobId, int ruleLevel, ArchiverSetting archiverSetting, byte[] contentVEO, byte[] historyVEO, string encryptKey)
            : base(deviceDtos, jobId, VaultExportFormat.VEO, null, null)
        {
            InitClass((PolicyLevel)ruleLevel, archiverSetting, contentVEO, historyVEO, encryptKey);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportSite(AveSPSite aveSite, VaultExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportSite"))
                {
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Site.", info.FullURL);

                    CreateCacheObject(info.FolderPath);

                    spoOjbectDepthCache.TryAdd(aveSite.SPSite.ID, 1);

                    var reportParams = new RecordVEOParametersV3(
                        VEOV3CommonString.OBJECTTYPE_FILE,
                        "1",
                        aveSite.SPSite.ID.ToString(),
                        aveSite.SPSite.RootWeb.Title,
                        info.FullURL,
                        aveSite.SPSite.RootWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3)
                        ).Init(PolicyLevel.SiteCollection, aveSite: aveSite);

                    new RecordVEODataV3().AddInformationObjectBlock(ref VEOContentObj, reportParams);

                    parentIOCache.TryAdd(aveSite.SPSite.ID, reportParams);
                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export site.It is Export Service Error.", info.FullURL, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export site.", info.FullURL, e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }

            return exportStatus;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportWeb(AveSPWeb aveWeb, VaultExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportWeb"))
                {
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Web.", info.FullURL);

                    CreateCacheObject(info.FolderPath);

                    var level = 1;
                    if (spoOjbectDepthCache.Count > 0)
                    {
                        var tempParentObject = aveWeb.SPWeb.ParentWeb;
                        var isFound = false;
                        while (tempParentObject != null && !tempParentObject.ServerRelativeUrl.Equals(tempParentObject.Site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            if (spoOjbectDepthCache.TryGetValue(tempParentObject.ID, out var depth))
                            {
                                level = depth + 1;
                                isFound = true;
                                break;
                            }
                            tempParentObject = tempParentObject.ParentWeb;
                        }

                        if (!isFound && spoOjbectDepthCache.TryGetValue(aveWeb.ParentSite.SPSite.ID, out var depth1))
                        {
                            level = depth1 + 1;
                        }
                    }

                    spoOjbectDepthCache.TryAdd(aveWeb.SPWeb.ID, level);
                    var reportParams = new RecordVEOParametersV3(
                        VEOV3CommonString.OBJECTTYPE_FILE,
                        level.ToString(),
                        aveWeb.SPWeb.ID.ToString(),
                        aveWeb.SPWeb.Title,
                        info.FullURL,
                        aveWeb.SPWeb.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3)
                        ).Init(PolicyLevel.Site, aveWeb: aveWeb);
                    BuildContainersIOCache(aveWeb.SPWeb.ID, PolicyLevel.Site, reportParams, aveWeb.SPWeb.ParentWebId.ToString(), aveWeb.SPWeb.ParentWeb.IsRootWeb);

                    new RecordVEODataV3().AddInformationObjectBlock(ref VEOContentObj, reportParams);

                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export web.It is Export Service Error.", info.FullURL, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export web.", info.FullURL, e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }

            return exportStatus;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ExportStatus ExportList(AveSPList aveList, VaultExportInfo info)
        {
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("VEOExport_ExportList"))
                {
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export List.", info.FullURL);

                    CreateCacheObject(info.FolderPath);

                    var level = 1;
                    if (spoOjbectDepthCache.Count > 0)
                    {
                        if (aveList.ParentWeb != null && spoOjbectDepthCache.TryGetValue(aveList.ParentWeb.SPWeb.ID, out var depth)
                            || spoOjbectDepthCache.TryGetValue(aveList.ParentSite.SPSite.ID, out depth))
                        {
                            level = depth + 1;
                        }
                    }

                    spoOjbectDepthCache.TryAdd(aveList.SPList.ID, level);

                    var reportParams = new RecordVEOParametersV3(
                        VEOV3CommonString.OBJECTTYPE_FILE,
                        level.ToString(),
                        aveList.SPList.ID.ToString(),
                        aveList.Title,
                        info.FullURL,
                        aveList.SPList.Created.ToString(VEOV3CommonString.FORMAT_DATETIME_V3)
                        ).Init(PolicyLevel.List, aveList: aveList);
                    BuildContainersIOCache(aveList.SPList.ID, PolicyLevel.List, reportParams);

                    new RecordVEODataV3().AddInformationObjectBlock(ref VEOContentObj, reportParams);

                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export list.It is Export Service Error.", info.FullURL, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export list.", info.FullURL, e2.ToString());
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

                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Folder.", info.FullURL);

                    CreateCacheObject(info.FolderPath);

                    var level = 1;
                    if (spoOjbectDepthCache.Count > 0)
                    {
                        var tempParentObject = aveFolder.AveParentFolder;
                        var isFound = false;
                        while (tempParentObject != null && !tempParentObject.SPFolder.ServerRelativeUrl.Equals(tempParentObject.AveList.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            if (spoOjbectDepthCache.TryGetValue(tempParentObject.Id, out var depth))
                            {
                                level = depth + 1;
                                isFound = true;
                                break;
                            }
                            tempParentObject = tempParentObject.AveParentFolder;
                        }

                        if (!isFound && (aveFolder.AveList.SPList != null && spoOjbectDepthCache.TryGetValue(aveFolder.AveList.SPList.ID, out var depth1)
                            || aveFolder.SPFolder.ParentWeb != null && spoOjbectDepthCache.TryGetValue(aveFolder.SPFolder.ParentWeb.ID, out depth1)
                            || spoOjbectDepthCache.TryGetValue(aveFolder.ParentSite.SPSite.ID, out depth1)))
                        {
                            level = depth1 + 1;
                        }
                    }

                    spoOjbectDepthCache.TryAdd(aveFolder.Id, level);

                    string created;
                    if (((DateTime)aveFolder.SPFolder.Item["Created"]).Kind != DateTimeKind.Utc && aveFolder.SPFolder.ParentWeb != null)
                    {
                        created = (aveFolder.SPFolder.ParentWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveFolder.SPFolder.Item["Created"])).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }
                    else
                    {
                        created = ((DateTime)aveFolder.SPFolder.Item["Created"]).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }

                    var reportParams = new RecordVEOParametersV3(
                        VEOV3CommonString.OBJECTTYPE_FILE,
                        level.ToString(),
                        aveFolder.Id.ToString(),
                        aveFolder.SPFolder.Name,
                        info.FullURL,
                        created
                        ).Init(PolicyLevel.Folder, aveFolder: aveFolder);
                    var parentId = aveFolder.SPFolder.ServerRelativeUrl.Equals(aveFolder.AveList.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) 
                        ? aveFolder.AveList.Id : aveFolder.AveParentFolder.Id;
                    BuildContainersIOCache(aveFolder.Id, PolicyLevel.Folder, reportParams, parentId.ToString());
                    new RecordVEODataV3().AddInformationObjectBlock(ref VEOContentObj, reportParams);

                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.It is Export Service Error.", info.FullURL, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export folder.", info.FullURL, e2.ToString());
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
                        return exportStatus;
                    }
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Doc Or DocVersion.", aveDoc.AveSPItem.Id);

                    if (!aveDoc.AveSPItem.SPListItem.File.Exists || aveDoc.AveSPItem.SPListItem.File.UIVersion != aveDoc.AveSPItem.Version)
                    {
                        return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = "StorageOptimization_VEOExportOnlySupportCurrentVersion" };
                    }

                    var cacheFileFullPath = string.Empty;
                    using (var fileContent = aveDoc.AveSPItem.GetContent())
                    {
                        (exportStatus.ExportSize, cacheFileFullPath) = CreateCacheObject(info.FolderPath, info.ContentFilePath, fileContent);
                    }
                    
                    var level = 0;
                    if (_ruleLevel == PolicyLevel.Document)
                    {
                        level = 0;
                    }
                    else if (spoOjbectDepthCache.Count > 0)
                    {
                        level = 1;
                        var isFound = false;
                        var tempParentObject = aveDoc.ParentFolder;
                        while (tempParentObject != null && !tempParentObject.SPFolder.ServerRelativeUrl.Equals(tempParentObject.AveList.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            if (spoOjbectDepthCache.TryGetValue(tempParentObject.Id, out var depth))
                            {
                                level = depth + 1;
                                isFound = true;
                                break;
                            }
                            tempParentObject = tempParentObject.AveParentFolder;
                        }

                        if (!isFound && (aveDoc.AveSPItem.AveSPList.SPList != null && spoOjbectDepthCache.TryGetValue(aveDoc.AveSPItem.AveSPList.SPList.ID, out var depth1)
                            || aveDoc.AveSPWeb != null && spoOjbectDepthCache.TryGetValue(aveDoc.AveSPWeb.SPWeb.ID, out depth1)
                            || spoOjbectDepthCache.TryGetValue(aveDoc.ParentSite.SPSite.ID, out depth1)))
                        {
                            level = depth1 + 1;
                        }
                    }

                    string created;
                    if (((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).Kind != DateTimeKind.Utc)
                    {
                        created = (aveDoc.AveSPWeb.SPWeb.RegionalSettings.TimeZone.LocalTimeToUTC((DateTime)aveDoc.AveSPItem.SPListItem["Created"])).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }
                    else
                    {
                        created = ((DateTime)aveDoc.AveSPItem.SPListItem["Created"]).ToString(VEOV3CommonString.FORMAT_DATETIME_V3);
                    }

                    if (aveDoc.ParentFolder == null)
                    {
                        throw new Exception($"Parent folder not found. Path: {aveDoc.Url}");
                    }

                    var reportParams = new RecordVEOParametersV3(
                        VEOV3CommonString.OBJECTTYPE_RECORD,
                        level.ToString(),
                        aveDoc.AveSPItem.Id.ToString(),
                        aveDoc.AveSPItem.SPListItem.Name,
                        info.FullURL,
                        created,
                        SecurityUtils.SafeCombinePath(info.FolderPath, info.ContentFilePath),
                        SecurityUtils.SafeCombinePath(CacheManager.CacheSystem.SystemLocation, cacheFileFullPath),
                        aveDoc
                        );
                    BuildContainersIOCache(aveDoc.AveSPItem.Id, PolicyLevel.Document, reportParams);

                    new RecordVEODataV3().AddInformationObjectBlock(ref VEOContentObj, reportParams);

                    FinishVEOFolder(false);
                    exportStatus.State = ExportState.Succeed;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.It is Export Service Error.", info.FullURL, e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion.", info.FullURL, e2.ToString());
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
            if (totalSize > 0 && totalCount > 0)
            {
                FinishVEOFolder(true);
            }
            VEOV3CommonMethod.CleanCache(CacheManager.CacheSystem.SystemLocation);
        }

        public List<CsvMetaData> GetCSVMetadata()
        {
            throw new NotImplementedException();
        }

        public void FinishVEOFolder(bool isFinishJob)
        {
            try
            {
                if (!isFinishJob && totalSize < VEOV3CommonMethod.CalculateGBSizeUnit((long)ArchiverSetting.FileSize) && totalCount < ArchiverSetting.FileNumber) return;

                var mVEOContent = ExportVEOContentXmlFile();
                ExportVEOSignatureXmlFile(VEOV3CommonMethod.BuildVEOSignature(mVEOContent), VEOV3CommonString.VEOContentSignature);
                var mVEOHistory = ExportVEOHistoryXmlFile();
                ExportVEOSignatureXmlFile(VEOV3CommonMethod.BuildVEOSignature(mVEOHistory), VEOV3CommonString.VEOHistorySignature);
                var exportDataFileStorageInfo = XConvert.FromNames(currentVEOFolder.HighName, VEOV3CommonString.VEOReadme);
                var destination = SecurityUtils.SafeCombinePath(CacheManager.CacheSystem.SystemLocation, exportDataFileStorageInfo.HighPlusLowName);
                VEOV3CommonMethod.ExportVEOReadmeFile(destination);
                var zipVeoFilePath = SecurityUtils.SafeCombinePath(CacheManager.CacheSystem.SystemLocation, currentVEOFolder.Name + ".zip");
                VEOV3CommonMethod.CreateVEOZipWithPassword(currentVEOFolder.ParentFullName, zipVeoFilePath, EncryptKey);
                var storageInfo = XConvert.FromNames(string.Empty, Path.GetFileName(zipVeoFilePath));

                using (var xStream = CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.Open))
                {
                    ExportInfo exportInfo = new ExportInfo();
                    VaultExportInfo expExportInfo = new VaultExportInfo()
                    {
                        FolderPath = JobId,
                        ContentFilePath = storageInfo.HighPlusLowName
                    };
                    var exportSize = RealVaultExport.ExportContent(exportInfo, expExportInfo, xStream).Size;
                    mLog.Info($"Export {storageInfo.HighPlusLowName} file to blob succeed. Volume: {VaultCover.ConverSizeFormat(exportSize, VaultCover.ConverSizeType.Normal)}");
                }

                if (!isFinishJob)
                {
                    VEOV3CommonMethod.CleanCache(currentVEOFolder.ParentFullName);
                    VEOV3CommonMethod.CleanCache(zipVeoFilePath);
                    OpenTempVeoFolder(Interlocked.Increment(ref cacheVEOFolderCount));
                    if (contentVEOXML != null && historyVEOXML != null)
                    {
                        VEOContentObj = RecordVEODataV3.InitVEOContent(contentVEOXML);
                        VEOHistoryObj = RecordVEODataV3.InitVEOHistory(historyVEOXML);
                    }
                    Interlocked.Exchange(ref totalSize, 0);
                    Interlocked.Exchange(ref totalCount, 0);
                    needReGenerateIOs = true;
                    return;
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while finising the VEO. Ex: " + e);
            }
        }

        #region Private Method

        private void BuildContainersIOCache(Guid id, PolicyLevel level, RecordVEOParametersV3 recordParams, string? parentId = null, bool isParentRootWeb = false)
        {
            if (level == PolicyLevel.Site && isParentRootWeb && recordParams.AveSPWeb != null)
            {
                if (parentIOCache.TryGetValue(recordParams.AveSPWeb.ParentSite.SPSite.ID, out var siteIO))
                {
                    parentIOCache = new() { { recordParams.AveSPWeb.ParentSite.SPSite.ID, siteIO } };
                }
                
            }
            else if (parentIOCache.Values.LastOrDefault(i => i != null && i.Level == level && (parentId == null || i.VId != parentId)) is var spoIO && spoIO != null)
            {
                RemoveAfterKey(new Guid(spoIO.VId));
            }

            if (needReGenerateIOs)
            {
                var VEOData = new RecordVEODataV3();
                var parentIOInOrder = parentIOCache.Values.OrderBy(i => int.Parse(i.VIODepth)).ToList();
                foreach (var containerParams in parentIOInOrder)
                {
                    VEOData.AddInformationObjectBlock(ref VEOContentObj, containerParams);
                }
                needReGenerateIOs = false;
            }

            if (level != PolicyLevel.Document)
            {
                parentIOCache[id] = recordParams;
            }
        }

        private byte[] ExportVEOContentXmlFile()
        {
            byte[] content = [];
            XmlAttributeOverrides aor = new XmlAttributeOverrides();
            XmlAttributes listAttribs = new XmlAttributes();
            listAttribs.XmlElements.Add(new XmlElementAttribute(typeof(RecordVEOClassV3.Record)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            listAttribs.XmlElements.Add(new XmlElementAttribute(typeof(RecordVEOClassV3.Agent)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            listAttribs.XmlElements.Add(new XmlElementAttribute(typeof(RecordVEOClassV3.Relationship)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            listAttribs.XmlElements.Add(new XmlElementAttribute(typeof(RecordVEOClassV3.BaseRDFTemplate)).Do(i => i.Namespace = "http://www.prov.vic.gov.au/ANZS5478"));
            aor.Add(typeof(RecordVEOClassV3.AnzsDescription), "Template", listAttribs);
            aor.Add(typeof(RecordVEOClassV3.RelationshipRelatedEntity), "Template", listAttribs);

            XmlAttributes listAttribs1 = new XmlAttributes();
            listAttribs1.XmlElements.Add(new XmlElementAttribute("Description"));
            aor.Add(typeof(RecordVEOClassV3.RDF), typeof(RecordVEOClassV3.AnzsDescription).Name, listAttribs1);
            aor.Add(typeof(RecordVEOClassV3.RDF), typeof(RecordVEOClassV3.AglsFromVERS2Description).Name, listAttribs1);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/VERS");

            XmlSerializer xs = new XmlSerializer(typeof(RecordVEOClassV3.VEOContent), aor);
            using (Stream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, VEOContentObj, ns);
                memStream.Position = 0;
                using (Stream tempStream = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(memStream);
                        doc.XmlResolver = null;
                        doc.Save(tempStream);
                        tempStream.Position = 0;

                        using (var memoryStream = new MemoryStream())
                        {
                            tempStream.CopyTo(memoryStream);
                            content = memoryStream.ToArray();
                        }

                        tempStream.Position = 0;
                        var contentVEOTempInfo = new StorageInfo { HighName = currentVEOFolder.HighName, LowName = VEOV3CommonString.VEOContent };
                        using (var stream = CacheManager.CacheSystem.OpenStream(contentVEOTempInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(stream);
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while changing content file path." + e.ToString());
                    }
                }
            }

            return content;
        }

        private void ExportVEOSignatureXmlFile(RecordVEOClassV3.SignatureBlock mVEOSign, string fileName)
        {
            XmlSerializerNamespaces ns1 = new XmlSerializerNamespaces();
            ns1.Add("vers", "http://www.prov.vic.gov.au/VERS");

            XmlSerializer xs1 = new XmlSerializer(typeof(RecordVEOClassV3.SignatureBlock));
            using (Stream memStream = new MemoryStream())
            {
                xs1.Serialize(memStream, mVEOSign, ns1);
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

                        var contentVEOTempInfo = new StorageInfo { HighName = currentVEOFolder.HighName, LowName = fileName };
                        using (var stream = CacheManager.CacheSystem.OpenStream(contentVEOTempInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(stream);
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occurred while changing content signature file path." + e.ToString());
                    }
                }
            }
        }

        private byte[] ExportVEOHistoryXmlFile()
        {
            byte[] content = [];
            new RecordVEODataV3().GenerateVEOHistory(ref VEOHistoryObj);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("vers", "http://www.prov.vic.gov.au/VERS");

            XmlSerializer xs = new XmlSerializer(typeof(RecordVEOClassV3.VEOHistory));
            using (Stream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, VEOHistoryObj, ns);
                memStream.Position = 0;
                using (Stream tempStream = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument() { XmlResolver = null };
                    try
                    {
                        doc.Load(memStream);
                        doc.XmlResolver = null;
                        doc.Save(tempStream);
                        tempStream.Position = 0;

                        using (var memoryStream = new MemoryStream())
                        {
                            tempStream.CopyTo(memoryStream);
                            content = memoryStream.ToArray();
                        }

                        tempStream.Position = 0;
                        var contentVEOTempInfo = new StorageInfo { HighName = currentVEOFolder.HighName, LowName = VEOV3CommonString.VEOHistory };
                        using (var stream = CacheManager.CacheSystem.OpenStream(contentVEOTempInfo, FileMode.OpenOrCreate))
                        {
                            tempStream.CopyTo(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"An error occurred while export {VEOV3CommonString.EXOVEOHistory} into cache. Error: {ex.Message}.");

                    }
                }
            }

            return content;
        }

        private void RemoveAfterKey(Guid targetKey)
        {
            bool foundKey = false;
            foreach (var key in parentIOCache.Keys.ToArray())
            {
                if (foundKey || key.Equals(targetKey))
                {
                    parentIOCache.Remove(key);
                    foundKey = true;
                }
            }
        }

        private string GetUserTitle(string name)
        {
            string[] sArray = name.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
            return sArray[1].ToString();
        }

        private static VEOContentXML InitContentVEOXML(byte[] contentVEO)
        {
            VEOContentXML contentVEOXML = null;
            try
            {
                using (MemoryStream recordVEOStream = new MemoryStream(contentVEO))
                {
                    contentVEOXML = (VEOContentXML)new XmlSerializer(typeof(VEOContentXML)).Deserialize(recordVEOStream);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while initing VEOContent XML,Message: {0}.", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_VEOContentExportConfigFileDeserializeException");
            }
            return contentVEOXML;
        }

        private static VEOHistoryXML InitHistoryVEOXML(byte[] historyVEO)
        {
            VEOHistoryXML veoHistory = null;
            try
            {
                using (MemoryStream ms = new MemoryStream(historyVEO))
                {
                    veoHistory = (VEOHistoryXML)new XmlSerializer(typeof(VEOHistoryXML)).Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An Error Occur while Init VEOHistory XML,Message: {0}.", ex.ToString());
                throw new ExportConfigurationFileError("StorageOptimization_VEOHistoryExportConfigFileDeserializeException");
            }
            return veoHistory;
        }

        private (long, string?) CreateCacheObject(string highName, string lowName = "", Stream? fileContent = null)
        {
            long exportSize = 0;
            var fullPath = SecurityUtils.SafeCombinePath(currentVEOFolder.HighName, highName);
            mLog.Info($"debug the path value ,remove later veo container path {currentVEOFolder.HighName} highName {highName} fullpath {fullPath} lowname {lowName}");
            _ = fullPath.Replace("/", @"\");
            _ = lowName.Replace("/", @"\");
            var exportDataFileStorageInfo = XConvert.FromNames(fullPath, lowName);
            if (!string.IsNullOrEmpty(lowName) && fileContent != null)
            {
                mLog.Info($"file path: {exportDataFileStorageInfo.HighPlusLowName}");
                using (var exportDataFileStream = CacheManager.CacheSystem.OpenStream(exportDataFileStorageInfo, FileMode.OpenOrCreate))
                {
                    fileContent.CopyTo(exportDataFileStream);
                    exportSize = exportDataFileStream.Length;
                    exportDataFileStream.Commit();
                }
                Interlocked.Add(ref totalSize, exportSize);
                Interlocked.Increment(ref totalCount);
                return (exportSize, exportDataFileStorageInfo.HighPlusLowName);
            }
            CacheManager.CacheSystem.OpenDirectory(exportDataFileStorageInfo, FileMode.OpenOrCreate);
            return (exportSize,null);
        }

        private void OpenCache()
        {
            var cacheSetting = GenerateCacheSettings();
            CacheManager ??= PlatformWindsorManager.GetService<ICacheService>();
            CacheManager.Open(cacheSetting, false);
            CacheManager.CacheSystem.Open();
            OpenTempVeoFolder(cacheVEOFolderCount);
        }

        private void OpenTempVeoFolder(int subVEOFolderCount)
        {
            var tempRestoreFolder = JobId + "_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "_" + subVEOFolderCount.ToString("D3");
            var restoreTempInfo = new StorageInfo { HighName = SecurityUtils.SafeCombinePath(tempRestoreFolder, tempRestoreFolder + VEOV3CommonString.VEO_V3_FOLDERPOSTFIX), LowName = String.Empty };
            currentVEOFolder = CacheManager.CacheSystem.OpenDirectory(restoreTempInfo, FileMode.OpenOrCreate);
        }

        private CacheSettingDto GenerateCacheSettings()
        {
            string mainJobFolder = JobId.Contains('_') ? JobId.Remove(JobId.IndexOf('_', StringComparison.OrdinalIgnoreCase)) : JobId;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), "ArchiverCache", "VEO", mainJobFolder),
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            return new CacheSettingDto
            {
                Extension = new CacheSettingExtension { Path = new List<PathMap>() { new PathMap() { DiskInfo = disk } } },
                LimitFreeSpace = 1024 * 1024 * 1024,//1 GB
            };
        }

        #endregion
    }
}
