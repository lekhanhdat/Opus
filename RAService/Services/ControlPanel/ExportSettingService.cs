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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Tenant;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;
using SkiaSharp;
using System.Xml;
using Microsoft.Azure.Cosmos.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using ICSharpCode.SharpZipLib.GZip;
using Aspose.Pdf.Forms;
using System.Security.Cryptography;
using AvePoint.RA.Contract.Archiver;
using System.Text.Json;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Common;
using RAExportCommon;

namespace AvePoint.RA.Service.ControlPanel
{
    [Audit]
    public class ExportSettingService : RMServiceBase, IExportSettingService
    {
        const string ControlArchiverSettings = "ArchiverSettings.config";
        const string ControlArchiverVEOSettings = "ArchiverVEOSettings.config";
        const string FileVEO = "FileVEO.xml";
        const string RecordVEO = "RecordVEO.xml";
        const string ManifestVEO = "ManifestVEO.xml";
        const string NAAFile = "NAA Configuration File.xml";
        const string EXONAAFile = "EXO NAA Configuration File.xml";
        const string NARAFile = "NARA Configuration File.xml";
        const string EXONARAFile = "EXO NARA Configuration File.xml";
        const string GoogleNARAFile = "Google NARA Configuration File.xml";
        const string EXOFileVEO = "EXOFileVEO.xml";
        const string EXORecordVEO = "EXORecordVEO.xml";
        const string EXOManifestVEO = "EXOManifestVEO.xml";

        private RALogger logger = RALogger.GetInstance(typeof(ExportSettingService));
        public IExportSettingsDao exportSettingsDao => PlatformWindsorManager.GetService<IExportSettingsDao>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => PlatformWindsorManager.GetService<IExportDataEncryptionSettingService>();
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.ConfigureExportSetting, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> UploadCoinfigAsync(string voeFilename, Stream veoInput, bool veoIsNoChangeDirectSave, string naaFileName, Stream naaInput, bool naaIsNoChangeDirectSave, string naraFileName, Stream naraInput, bool naraIsNoChangeDirectSave, bool enableExportEncryption, bool enabledDatasum, bool needToUpgradeVEOV3)
        {
            if (!veoIsNoChangeDirectSave)
            {
                if (string.IsNullOrEmpty(voeFilename))
                {
                    DeleteVeoConfig(voeFilename);
                }
                else
                {
                    if (needToUpgradeVEOV3 || VEOV3CommonMethod.HasUpgradedVEOV3())
                    {
                        UploadVEOV3Config(voeFilename, veoInput);
                    }
                    else
                    {
                        UploadVeoConfig(voeFilename, veoInput);
                    }
                }
            }

            if (!naaIsNoChangeDirectSave)
            {
                if (string.IsNullOrEmpty(naaFileName))
                {
                    DeleteNaaConfig(naaFileName);
                }
                else
                {
                    UploadNaaConfig(naaFileName, naaInput);
                }
            }

            if (!naraIsNoChangeDirectSave)
            {
                if (string.IsNullOrEmpty(naraFileName))
                {
                    DeleteNaraConfig(naraFileName);
                }
                else
                {
                    UploadNaraConfig(naraFileName, naraInput);
                }
            }

            if (enableExportEncryption)
            {
                await ExportDataEncryptionSettingService.EnableExportDataEncryptionAsync();
            }
            else
            {
                await ExportDataEncryptionSettingService.DisableExportDataEncryptionAsync();
            }
            return true;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.ConfigureExportSetting, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> UploadConfigAsyncForGoogleOne(string naraFileName, byte[] naraInput, bool naraIsNoChangeDirectSave, bool enabledDatasum)
        {
            if (!naraIsNoChangeDirectSave)
            {
                if (string.IsNullOrEmpty(naraFileName))
                {
                    DeleteNaraConfig(naraFileName);
                }
                else
                {
                    UploadNaraConfigForGoogleOne(naraFileName, naraInput);
                }
            }

            return true;
        }
        public async System.Threading.Tasks.Task MigrateVEOTemplateAsync(byte[] zipConfigContent, string fileName)
        {
            var veoSettings = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO);
            if(veoSettings != null && veoSettings.Count > 0)
            {
                logger.Info($"Opus already set VEO template.");
                return;
            }

            if(!Path.GetExtension(fileName).Equals("zip", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".zip";
            }

            var opusVEOTemplatePath = DownloadTemplateZip("VEO Configuration Files.zip");

            RMCPExportSetting setting = new RMCPExportSetting();
            setting.IsActived = true;
            setting.FileName = fileName;
            setting.ExportSettingType = (int)ExportSettingType.VEO;
            setting.SourceFlag = (int)SourceFlag.SharePoint;
            setting.DAOMigrated = true;

            RMCPExportSetting settingEXO = new RMCPExportSetting();
            settingEXO.IsActived = true;
            settingEXO.FileName = fileName;
            settingEXO.ExportSettingType = (int)ExportSettingType.VEO;
            settingEXO.SourceFlag = (int)SourceFlag.Exchange;
            settingEXO.DAOMigrated = true;

            HashSet<string> checkingFiles = new HashSet<string>();
            using (var stream = new FileStream(opusVEOTemplatePath, FileMode.Open, FileAccess.Read))
            using (ZipArchive zip = new ZipArchive(stream))
            {
                foreach (var entry in zip.Entries)
                {
                    checkingFiles.Add(entry.Name);
                    switch (entry.Name)
                    {
                        case "FileVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.FileVEO = ms.ToArray();
                            }
                            break;
                        case "RecordVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.RecordVEO = ms.ToArray();
                            }
                            break;
                        case "ManifestVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.ManifestVEO = ms.ToArray();
                            }
                            break;
                        case "EXOFileVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                settingEXO.FileVEO = ms.ToArray();
                            }
                            break;
                        case "EXORecordVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                settingEXO.RecordVEO = ms.ToArray();
                            }
                            break;
                        case "EXOManifestVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                settingEXO.ManifestVEO = ms.ToArray();
                            }
                            break;
                        case "ArchiverSettings.config":
                            var archiverReader = new StreamReader(entry.Open());
                            setting.ArchiverSetting = archiverReader.ReadToEnd();
                            settingEXO.ArchiverSetting = setting.ArchiverSetting;
                            break;
                        case "ArchiverVEOSettings.config":
                            var veoReader = new StreamReader(entry.Open());
                            setting.ArchiverVEOSetting = veoReader.ReadToEnd();
                            settingEXO.ArchiverVEOSetting = setting.ArchiverVEOSetting;
                            break;
                        default:
                            break;
                    }
                }
            }

            using (var stream = new MemoryStream(zipConfigContent))
            using (ZipArchive zip = new ZipArchive(stream))
            {
                // DAO 的config zip中有的 config file， 就用 DAO的
                foreach (var entry in zip.Entries)
                {
                    checkingFiles.Remove(entry.Name);
                    switch (entry.Name)
                    {
                        case "FileVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.FileVEO = ms.ToArray();
                            }
                            break;
                        case "RecordVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.RecordVEO = ms.ToArray();
                            }
                            break;
                        case "ManifestVEO.xml":
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                setting.ManifestVEO = ms.ToArray();
                            }
                            break;
                        case "ArchiverSettings.config":
                            var archiverReader = new StreamReader(entry.Open());
                            setting.ArchiverSetting = archiverReader.ReadToEnd();
                            settingEXO.ArchiverSetting = setting.ArchiverSetting;
                            break;
                        case "ArchiverVEOSettings.config":
                            var veoReader = new StreamReader(entry.Open());
                            setting.ArchiverVEOSetting = veoReader.ReadToEnd();
                            settingEXO.ArchiverVEOSetting = setting.ArchiverVEOSetting;
                            break;
                        default:
                            break;
                    }
                }

            }

            var tempZipConfigFilePath = Path.Combine(WebUtil.GetInstallPath(), "Temp", Guid.NewGuid().ToString() + ".zip");
            Directory.CreateDirectory(Path.GetDirectoryName(tempZipConfigFilePath));
            using (var fs = File.Create(tempZipConfigFilePath))
            {
                await fs.WriteAsync(zipConfigContent, 0, zipConfigContent.Length);
            }

            using (var stream = new FileStream(tempZipConfigFilePath, FileMode.Open))
            using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Update))
            {
                // DAO 的config zip里没有的config file，用Opus default template里的config file来补齐
                foreach (var checkFile in checkingFiles)
                {
                    var newEntry = zip.CreateEntry(checkFile);
                    using var entryStream = newEntry.Open();
                    switch (checkFile)
                    {
                        case "FileVEO.xml":
                            using (MemoryStream ms = new MemoryStream(setting.FileVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "RecordVEO.xml":

                            using (MemoryStream ms = new MemoryStream(setting.RecordVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "ManifestVEO.xml":
                            using (MemoryStream ms = new MemoryStream(setting.ManifestVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "EXOFileVEO.xml":
                            using (MemoryStream ms = new MemoryStream(settingEXO.FileVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "EXORecordVEO.xml":
                            using (MemoryStream ms = new MemoryStream(settingEXO.RecordVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "EXOManifestVEO.xml":
                            using (MemoryStream ms = new MemoryStream(settingEXO.ManifestVEO))
                            {
                                ms.WriteTo(entryStream);
                            }
                            break;
                        case "ArchiverSettings.config":
                            var archiverSettingContent = Encoding.UTF8.GetBytes(setting.ArchiverSetting);
                            await entryStream.WriteAsync(archiverSettingContent, 0, archiverSettingContent.Length);
                            break;
                        case "ArchiverVEOSettings.config":
                            var archiverVEOSettingContent = Encoding.UTF8.GetBytes(setting.ArchiverVEOSetting);
                            await entryStream.WriteAsync(archiverVEOSettingContent, 0, archiverVEOSettingContent.Length);
                            break;
                        default:
                            break;
                    }
                }
            }

            var tenantFolderName = JobReportUtility.GetTenantIdentity();
            var blobName = $"{tenantFolderName}{Path.DirectorySeparatorChar}{JobReportUtility.ExportVEOConfig}{Path.DirectorySeparatorChar}{fileName}";

            // delete if exists
            RAStorageUtil.DeleteReportBlob(blobName);
            RAStorageUtil.UploadReportBlob(blobName, tempZipConfigFilePath);
            exportSettingsDao.SaveOrUpdate(new List<RMCPExportSetting>() { setting, settingEXO });

            File.Delete(tempZipConfigFilePath);
        }

        public async Task<ExportSettingEx> GetSavedFileInfosAsync()
        {
            ExportSettingEx exportSettingEx = new ExportSettingEx();
            List<ExportSetting> result = new List<ExportSetting>();
            List<RMCPExportSetting> exportSettings = exportSettingsDao.GetExportSettings();
            #region VEO-V3
            exportSettingEx.HasVEOV3Permission = VEOV3CommonMethod.HasVEOV3Permission();
            exportSettingEx.HasUpgradeVEOV3 = VEOV3CommonMethod.HasUpgradedVEOV3();
            exportSettings = exportSettings
                .Where(s => s.ExportSettingType != (int)ExportSettingType.VEO
                    || (exportSettingEx.HasUpgradeVEOV3 && exportSettingEx.HasVEOV3Permission
                        ? s.VEOHistory != null && s.VEOContent != null
                        : s.VEOHistory == null && s.VEOContent == null))
                .ToList();
            #endregion
            var isNewOpusTenant = TenantService.IsNewOpusTenant();
            foreach (var setting in exportSettings)
            {   
                ExportSetting es = new ExportSetting();
                var configZip = GetConfigureFileBlobName(setting.FileName, (ExportSettingType)setting.ExportSettingType);

                if (!RAStorageUtil.TryGetReportBlobLength(configZip, out var contentSize))
                {
                    es.FileSize = "0";
                    es.FileName = string.Empty;
                }
                else
                {
                    es.FileSize = (contentSize / 1024.0).ToString("f2");
                    es.FileName = setting.FileName;
                    es.ExportSettingType = (ExportSettingType)setting.ExportSettingType;
                }

                result.Add(es);
            }
            exportSettingEx.Settings = result;
            exportSettingEx.EncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
            exportSettingEx.EncryptionKey = ExportDataEncryptionSettingService.GetCurrentAesKey().Extension;
            SettingProfileDto mDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.ExportLocationDevice,
                Name = "UsingExportLocationDevice"
            };
            if (isNewOpusTenant)
            {
                var dto = SettingProfileDao.Load(mDto);
                if (dto != null)
                {
                    var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(dto);
                    exportSettingEx.CurrentExportLocationId = tempDto.Settings;
                }
                else
                {
                    exportSettingEx.CurrentExportLocationId = null;
                }
            }
            else
            {
                var exportLocationId = GlobalSettingService.GetCurrentExportLocationId();
                logger.Info($"Selected location id: {exportLocationId}");
                if (!string.IsNullOrEmpty(exportLocationId)) 
                {
                    exportSettingEx.CurrentExportLocationId = exportLocationId;
                }
                else
                {
                    exportSettingEx.CurrentExportLocationId = null;
                }
            }

            exportSettingEx.StorageInfo = new List<StorageIdAndName>();
            if (isNewOpusTenant)
            {
                var storageFilterResult = await StorageDeviceDao.GetStoragesDeviceByFilterAsync(true);        
                foreach (var storage in storageFilterResult)
                {
                    exportSettingEx.StorageInfo.Add(new StorageIdAndName() { Id = storage.Id.ToString(), Name = storage.Name });
                }
            }
            else
            {
                var allExportLocations = await GlobalSettingService.GetAllExportLocationAsync();
                foreach (var exportLocation in allExportLocations)
                {
                    exportSettingEx.StorageInfo.Add(new StorageIdAndName() { Id = exportLocation.Id, Name = exportLocation.Name });
                }
            }
            var exportSignatureSetting = SettingProfileService.GetExportSignature();
            exportSettingEx.ExportNARADataChecksumEnabled = exportSignatureSetting.EnableExportSignature;
            exportSettingEx.ExportNARAPublicKey = AesEncryptorWrapper.Decrypt(exportSignatureSetting.PublicKey);

            var exportSignatureForVeoSetting = SettingProfileService.GetExportSignatureForVEO();
            exportSettingEx.ExportVEOPublicKey = AesEncryptorWrapper.Decrypt(exportSignatureForVeoSetting.PublicKey);

            return exportSettingEx;
        }

        public async Task<ExportSettingEx> GetSavedFileInfosAsyncForGoogleOne()
        {
            ExportSettingEx exportSettingEx = new ExportSettingEx();
            List<ExportSetting> result = new List<ExportSetting>();
            List<RMCPExportSetting> exportSettings = exportSettingsDao.GetExportSettings();
            var isNewOpusTenant = TenantService.IsNewOpusTenant();
            foreach (var setting in exportSettings)
            {
                ExportSetting es = new ExportSetting();
                var configZip = GetConfigureFileBlobName(setting.FileName, (ExportSettingType)setting.ExportSettingType);

                if (!RAStorageUtil.TryGetReportBlobLength(configZip, out var contentSize))
                {
                    es.FileSize = "0";
                    es.FileName = string.Empty;
                }
                else
                {
                    es.FileSize = (contentSize / 1024.0).ToString("f2");
                    es.FileName = setting.FileName;
                    es.ExportSettingType = (ExportSettingType)setting.ExportSettingType;
                }

                result.Add(es);
            }
            exportSettingEx.Settings = result;
          
            var exportSignatureSetting = SettingProfileService.GetExportSignature();
            exportSettingEx.ExportNARADataChecksumEnabled = exportSignatureSetting.EnableExportSignature;
            exportSettingEx.ExportNARAPublicKey = AesEncryptorWrapper.Decrypt(exportSignatureSetting.PublicKey);

            return exportSettingEx;
        }
        public string GetSavedFileName(out double size, out bool isActive)
        {
            var exportSettings = exportSettingsDao.GetExportSetting(true);
            if (exportSettings == null)
            {
                size = 0;
                isActive = false;
                return string.Empty;
            }
            var veoConfigZip = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.VEO);
            if (!RAStorageUtil.TryGetReportBlobLength(veoConfigZip, out var contentSize))
            {
                size = 0;
                isActive = false;
                return string.Empty;
            }
            size = (contentSize / 1024.0);
            isActive = exportSettings.IsActived;
            return Path.GetFileName(exportSettings.FileName);
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.DownloadTemplate, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public string DownloadTemplateZip(string fileName)
        {
            try
            {
                var isNewOpusTenant = TenantService.IsNewOpusTenant();
                string filepath = string.Empty;
                if (isNewOpusTenant)
                {
                    filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", fileName);
                }
                else
                {
                    filepath = Path.Combine(WebUtil.GetInstallPath(), "Config","DAOExportTemplate", fileName);
                }
                return filepath;
            }
            catch
            {
                return String.Empty;
            }
        }
        //该方法前台没有调用
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.DeleteExportSetting, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public string DeleteConfigureFileName()
        {
            var filename = string.Empty;
            var exportSettings = exportSettingsDao.GetExportSetting(true);
            if (exportSettings != null)
            {
                var veoconfig = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.VEO);
                RAStorageUtil.DeleteReportBlob(veoconfig);
                filename = exportSettings.FileName;
            }
            exportSettingsDao.Delete();
            try
            {
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                RAStorageUtil.DeleteReportBlobs($"{tenantFolderName}/{JobReportUtility.ExportVEOConfig}");
            }
            catch (Exception e)
            {
                logger.Warn("Delete VEO configure error:{0}", e.ToString());
            }
            return filename;
        }

        //该方法前台没有调用
        //public bool ExportSettignsOnlyChangeActived(bool isActived)
        //{
            //RMCPExportSetting active = exportSettingsDao.GetExportSetting(true);
            //if (active != null)
            //{
            //    active.IsActived = isActived;
            //    exportSettingsDao.SaveOrUpdate(active);
            //}
            //return true;
        //}

        public string GetConfigureFileName(ExportSettingType type)
        {
            var exportSettings = exportSettingsDao.GetExportSetting((int)type);
            if (exportSettings != null)
            {
                return exportSettings.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public Stream DownloadConfigureFileToStream(out string filename)
        {
            var condition = VEOV3CommonMethod.HasUpgradedVEOV3() ? (Func<RMCPExportSetting, bool>)(s => s.VEOContent != null && s.VEOHistory != null) : (s => s.VEOContent == null && s.VEOHistory == null);
            RMCPExportSetting exportSettings = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition);
            var veoblob = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.VEO);
            if (SecurityUtils.IsValidFileName(exportSettings.FileName))
            {
                filename = exportSettings.FileName;
            }
            else
            {
                throw new Exception("Invalid file name");
            }
            return RAStorageUtil.DownloadReportBlobToStream(veoblob);
        }

        public Stream DownloadNAAConfigureFileToStream(out string filename)
        {
            var exportSettings = exportSettingsDao.GetExportSetting((int)ExportSettingType.NAA);
            var nnablob = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.NAA);
            filename = exportSettings.FileName;
            return RAStorageUtil.DownloadReportBlobToStream(nnablob);
        }

        public Stream DownloadNARAConfigureFileToStream(out string filename)
        {
            var exportSettings = exportSettingsDao.GetExportSetting((int)ExportSettingType.NARA);
            var nnablob = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.NARA);
            filename = exportSettings.FileName;
            return RAStorageUtil.DownloadReportBlobToStream(nnablob);
        }

        private bool UploadVeoConfig(string filename, Stream inputStream)
        {
            byte[] cacheFile = StreamToBytes(inputStream);
            try
            {
                //assemble data
                var validationFile = BytesToStream(cacheFile);
                RMCPExportSetting setting = new RMCPExportSetting();
                RMCPExportSetting settingEXO = new RMCPExportSetting();
                setting.IsActived = true;
                setting.FileName = filename;
                setting.ExportSettingType = (int)ExportSettingType.VEO;
                setting.SourceFlag = (int)SourceFlag.SharePoint;
                settingEXO.IsActived = true;
                settingEXO.FileName = filename;
                settingEXO.ExportSettingType = (int)ExportSettingType.VEO;
                settingEXO.SourceFlag = (int)SourceFlag.Exchange;
                string errorFile = string.Empty;
                try
                {
                    using (ZipArchive zip = new ZipArchive(validationFile))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            switch (entry.Name)
                            {
                                case "FileVEO.xml":
                                    errorFile = FileVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.FileVEO = ms.ToArray();
                                    }
                                    break;
                                case "RecordVEO.xml":
                                    errorFile = RecordVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.RecordVEO = ms.ToArray();
                                    }
                                    break;
                                case "ManifestVEO.xml":
                                    errorFile = ManifestVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.ManifestVEO = ms.ToArray();
                                    }
                                    break;
                                case "EXOFileVEO.xml":
                                    errorFile = EXOFileVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.FileVEO = ms.ToArray();
                                    }
                                    break;
                                case "EXORecordVEO.xml":
                                    errorFile = EXORecordVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.RecordVEO = ms.ToArray();
                                    }
                                    break;
                                case "EXOManifestVEO.xml":
                                    errorFile = EXOManifestVEO;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.ManifestVEO = ms.ToArray();
                                    }
                                    break;
                                case "ArchiverSettings.config":
                                    errorFile = ControlArchiverSettings;
                                    var archiverReader = new StreamReader(entry.Open());
                                    setting.ArchiverSetting = archiverReader.ReadToEnd();
                                    settingEXO.ArchiverSetting = setting.ArchiverSetting;
                                    break;
                                case "ArchiverVEOSettings.config":
                                    errorFile = ControlArchiverVEOSettings;
                                    var veoReader = new StreamReader(entry.Open());
                                    setting.ArchiverVEOSetting = veoReader.ReadToEnd();
                                    settingEXO.ArchiverVEOSetting = setting.ArchiverVEOSetting;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Error("read file error:{0}", e.ToString());
                    throw new ExportConfigZipIllegalException(string.Format(I18NEntity.GetString("RM_ES_ReadFileError"), errorFile));
                }

                //validation files
                var errorFiles = new List<string>();
                if (setting.FileVEO == null)
                {
                    errorFiles.Add(FileVEO);
                }

                if (settingEXO.FileVEO == null)
                {
                    errorFiles.Add(EXOFileVEO);
                }

                if (setting.RecordVEO == null)
                {
                    errorFiles.Add(RecordVEO);
                }

                if (settingEXO.RecordVEO == null)
                {
                    errorFiles.Add(EXORecordVEO);
                }

                if (setting.ManifestVEO == null)
                {
                    errorFiles.Add(ManifestVEO);
                }

                if (settingEXO.ManifestVEO == null)
                {
                    errorFiles.Add(EXOManifestVEO);
                }

                if (string.IsNullOrEmpty(setting.ArchiverSetting))
                {
                    errorFiles.Add(setting.ArchiverSetting);
                }

                if (string.IsNullOrEmpty(setting.ArchiverVEOSetting))
                {
                    errorFiles.Add(ControlArchiverVEOSettings);
                }

                if (errorFiles.Count != 0)
                {
                    logger.Error("{0} can`t found or are error in veo zip file", string.Join(",", errorFiles));
                    throw new ExportConfigZipIllegalException(I18NEntity.GetString("RM_ES_NAA_ConfigFileIncomplete"));
                }

                //upload file to storage
                var uploadFile = BytesToStream(cacheFile);
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                StringBuilder folderName = null;
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    folderName = new StringBuilder(Path.Combine(tenantFolderName, JobReportUtility.ExportVEOConfig));
                }

                var oldsetting = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(s => s.VEOContent == null && s.VEOHistory == null);
                if (oldsetting != null)
                {
                    var oldBlobName = folderName.ToString() + Path.DirectorySeparatorChar + oldsetting.FileName;
                    RAStorageUtil.DeleteReportBlob(oldBlobName);
                    //var tenantFolder = container.GetDirectoryReference(tenantFolderName);
                    //var veoFolder = tenantFolder.GetDirectoryReference(JobReportUtility.ExportVEOConfig);
                    //foreach (var item in veoFolder.ListBlobs(true).OfType<CloudBlockBlob>())
                    //{
                    //    item.DeleteIfExists();
                    //}
                }

                var blobName = new StringBuilder(folderName.ToString());
                blobName.Append(Path.DirectorySeparatorChar).Append(filename);

                //upload browser to blob
                RAStorageUtil.UploadReportBlob(blobName.ToString(), uploadFile);
                List<RMCPExportSetting> result = new List<RMCPExportSetting>();
                result.Add(setting);
                result.Add(settingEXO);
                exportSettingsDao.SaveOrUpdate(result);
            }
            catch (Exception e)
            {
                logger.Error("VEO settings save to DB error. error details: {0}", e.ToString());
                throw;
            }
            finally
            {
                cacheFile = null;
            }
            return true;
        }

        public bool UploadVEOV3Config(string filename, Stream inputStream)
        {
            byte[] cacheFile = StreamToBytes(inputStream);
            try
            {
                //assemble data
                var validationFile = BytesToStream(cacheFile);
                RMCPExportSetting setting = new RMCPExportSetting();
                RMCPExportSetting settingEXO = new RMCPExportSetting();
                setting.IsActived = true;
                setting.FileName = filename;
                setting.ExportSettingType = (int)ExportSettingType.VEO;
                setting.SourceFlag = (int)SourceFlag.SharePoint;
                settingEXO.IsActived = true;
                settingEXO.FileName = filename;
                settingEXO.ExportSettingType = (int)ExportSettingType.VEO;
                settingEXO.SourceFlag = (int)SourceFlag.Exchange;
                string errorFile = string.Empty;
                try
                {
                    using (ZipArchive zip = new ZipArchive(validationFile))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            switch (entry.Name)
                            {
                                case VEOV3CommonString.VEOContent:
                                    errorFile = VEOV3CommonString.VEOContent;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.VEOContent = ms.ToArray();
                                    }
                                    break;
                                case VEOV3CommonString.VEOHistory:
                                    errorFile = VEOV3CommonString.VEOHistory;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.VEOHistory = ms.ToArray();
                                    }
                                    break;
                                case VEOV3CommonString.EXOVEOContent:
                                    errorFile = VEOV3CommonString.EXOVEOContent;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.VEOContent = ms.ToArray();
                                    }
                                    break;
                                case VEOV3CommonString.EXOVEOHistory:
                                    errorFile = VEOV3CommonString.EXOVEOHistory;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.VEOHistory = ms.ToArray();
                                    }
                                    break;
                                case ControlArchiverSettings:
                                    errorFile = ControlArchiverSettings;
                                    var archiverReader = new StreamReader(entry.Open());
                                    setting.ArchiverSetting = archiverReader.ReadToEnd();
                                    settingEXO.ArchiverSetting = setting.ArchiverSetting;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("read file error:{0}", e.ToString());
                    throw new ExportConfigZipIllegalException(string.Format(I18NEntity.GetString("RM_ES_ReadFileError"), errorFile));
                }

                //validation files
                var errorFiles = new List<string>();

                if (setting.VEOContent == null)
                {
                    errorFiles.Add(VEOV3CommonString.VEOContent);
                }

                if (setting.VEOHistory == null)
                {
                    errorFiles.Add(VEOV3CommonString.VEOHistory);
                }

                if (settingEXO.VEOContent == null)
                {
                    errorFiles.Add(VEOV3CommonString.EXOVEOContent);
                }
                
                if (settingEXO.VEOHistory == null)
                {
                    errorFiles.Add(VEOV3CommonString.EXOVEOHistory);
                }

                if (string.IsNullOrEmpty(setting.ArchiverSetting))
                {
                    errorFiles.Add(setting.ArchiverSetting);
                }

                if (errorFiles.Count != 0)
                {
                    logger.Error("{0} can't found or are error in veo zip file", string.Join(",", errorFiles));
                    throw new ExportConfigZipIllegalException(I18NEntity.GetString("RM_ES_NAA_ConfigFileIncomplete"));
                }

                //upload file to storage
                var uploadFile = BytesToStream(cacheFile);
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                StringBuilder folderName = null;
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    folderName = new StringBuilder(Path.Combine(tenantFolderName, JobReportUtility.ExportVEOConfig));
                }

                var oldsetting = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(s => s.VEOContent != null && s.VEOHistory != null);
                if (oldsetting != null)
                {
                    var oldBlobName = folderName.ToString() + Path.DirectorySeparatorChar + oldsetting.FileName;
                    RAStorageUtil.DeleteReportBlob(oldBlobName);
                }

                var blobName = new StringBuilder(folderName.ToString());
                blobName.Append(Path.DirectorySeparatorChar).Append(filename);

                //upload browser to blob
                RAStorageUtil.UploadReportBlob(blobName.ToString(), uploadFile);
                List<RMCPExportSetting> result = new List<RMCPExportSetting>();
                result.Add(setting);
                result.Add(settingEXO);
                exportSettingsDao.SaveOrUpdateVEOV3(result);
                if (!VEOV3CommonMethod.HasUpgradedVEOV3())
                {
                    var keyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeVEOV3, Value = "True" };
                    RMKeyValueDao.SaveOrUpdateAsync(keyValueEntity);
                }
            }
            catch (Exception e)
            {
                logger.Error("VEO settings save to DB error. error details: {0}", e.ToString());
                throw;
            }
            finally
            {
                cacheFile = null;
            }
            return true;
        }

        public bool UploadNaaConfig(string filename, Stream inputStream)
        {
            byte[] cacheFile = StreamToBytes(inputStream);
            try
            {
                //assemble data
                var validationFile = BytesToStream(cacheFile);
                RMCPExportSetting setting = new RMCPExportSetting();
                RMCPExportSetting settingEXO = new RMCPExportSetting();
                setting.IsActived = true;
                setting.FileName = filename;
                setting.ExportSettingType = (int)ExportSettingType.NAA;
                setting.SourceFlag = (int)SourceFlag.SharePoint;
                settingEXO.IsActived = true;
                settingEXO.FileName = filename;
                settingEXO.ExportSettingType = (int)ExportSettingType.NAA;
                settingEXO.SourceFlag = (int)SourceFlag.Exchange;
                string errorFile = string.Empty;
                try
                {
                    using (ZipArchive zip = new ZipArchive(validationFile))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            switch (entry.Name)
                            {
                                case "NAA Configuration File.xml":
                                    errorFile = NAAFile;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.ExportConfig = ms.ToArray();
                                    }
                                    break;
                                case "EXO NAA Configuration File.xml":
                                    errorFile = EXONAAFile;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.ExportConfig = ms.ToArray();
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("read file error:{0}", e.ToString());
                    throw new ExportConfigZipIllegalException(string.Format(I18NEntity.GetString("RM_ES_ReadFileError"), errorFile));
                }

                //validation
                var errorFiles = new List<string>();
                if (setting.ExportConfig == null)
                {
                    errorFiles.Add(NAAFile);
                }
                if(settingEXO.ExportConfig == null)
                {
                    errorFiles.Add(EXONAAFile);
                }
                if(errorFiles.Count()>0)
                {
                    logger.Error("{0} can`t found in naa zip file", string.Join(",", errorFiles));
                    throw new ExportConfigZipIllegalException(I18NEntity.GetString("RM_ES_NAA_ConfigFileIncomplete"));
                }
                //upload file to storage
                var uploadFile = BytesToStream(cacheFile);
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                StringBuilder folderName = null;
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    folderName = new StringBuilder(Path.Combine(tenantFolderName, JobReportUtility.ExportNAAConfig));
                }

                var oldsetting = exportSettingsDao.GetExportSetting((int)ExportSettingType.NAA);
                ArgumentCheck.NotNull(folderName, nameof(folderName));
                if (oldsetting != null)
                {
                    var oldBlobName = folderName.ToString() + Path.DirectorySeparatorChar + oldsetting.FileName;
                    RAStorageUtil.DeleteReportBlob(oldBlobName);
                }

                var blobName = new StringBuilder(folderName.ToString());
                blobName.Append(Path.DirectorySeparatorChar).Append(filename);
                //upload browser to blob
                RAStorageUtil.UploadReportBlob(blobName.ToString(), uploadFile);
                List<RMCPExportSetting> result = new List<RMCPExportSetting>();
                result.Add(setting);
                result.Add(settingEXO);
                exportSettingsDao.SaveOrUpdate(result);
            }
            catch (Exception e)
            {
                logger.Error("NAA settings save to DB error. error details: {0}", e.ToString());
                throw;
            }
            finally
            {
                cacheFile = null;
            }
            return true;
        }
        public bool UploadNaraConfig(string filename, Stream inputStream)
        {
            byte[] cacheFile = StreamToBytes(inputStream);
            try
            {
                //assemble data
                var validationFile = BytesToStream(cacheFile);
                RMCPExportSetting setting = new RMCPExportSetting();
                RMCPExportSetting settingEXO = new RMCPExportSetting();
                setting.IsActived = true;
                setting.FileName = filename;
                setting.ExportSettingType = (int)ExportSettingType.NARA;
                setting.SourceFlag = (int)SourceFlag.SharePoint;
                settingEXO.IsActived = true;
                settingEXO.FileName = filename;
                settingEXO.ExportSettingType = (int)ExportSettingType.NARA;
                settingEXO.SourceFlag = (int)SourceFlag.Exchange;
                //Google
                RMCPExportSetting settingGoogle = new RMCPExportSetting();
                settingGoogle.IsActived = true;
                settingGoogle.FileName = filename;
                settingGoogle.ExportSettingType = (int)ExportSettingType.NARA;
                settingGoogle.SourceFlag = (int)SourceFlag.Google;
                string errorFile = string.Empty;
                try
                {
                    using (ZipArchive zip = new ZipArchive(validationFile))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            switch (entry.Name)
                            {
                                case "NARA Configuration File.xml":
                                    errorFile = NARAFile;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        setting.ExportConfig = ms.ToArray();
                                    }
                                    break;
                                case "EXO NARA Configuration File.xml":
                                    errorFile = EXONARAFile;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingEXO.ExportConfig = ms.ToArray();
                                    }
                                    break;
                                case "Google NARA Configuration File.xml":
                                    errorFile = GoogleNARAFile;
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        entry.Open().CopyTo(ms);
                                        settingGoogle.ExportConfig = ms.ToArray();
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("read file error:{0}", e.ToString());
                    throw new ExportConfigZipIllegalException(string.Format(I18NEntity.GetString("RM_ES_ReadFileError"), errorFile));
                }

                //validation
                var errorFiles = new List<string>();
                if (setting.ExportConfig == null)
                {
                    errorFiles.Add(NARAFile);
                }
                if (settingEXO.ExportConfig == null)
                {
                    errorFiles.Add(EXONARAFile);
                }
                if (settingGoogle.ExportConfig == null)
                {
                    errorFiles.Add(GoogleNARAFile);
                }
                if (errorFiles.Count() > 0)
                {
                    logger.Error("{0} can`t found in naa zip file", string.Join(",", errorFiles));
                    throw new ExportConfigZipIllegalException(I18NEntity.GetString("RM_ES_NAA_ConfigFileIncomplete"));
                }

                //upload file to storage
                var uploadFile = BytesToStream(cacheFile);
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                StringBuilder folderName = null;
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    folderName = new StringBuilder(Path.Combine(tenantFolderName, JobReportUtility.ExportNARAConfig));
                }

                var oldsetting = exportSettingsDao.GetExportSetting((int)ExportSettingType.NARA);
                ArgumentCheck.NotNull(folderName,nameof(folderName));
                if (oldsetting != null)
                {
                    var oldBlobName = folderName.ToString() + Path.DirectorySeparatorChar + oldsetting.FileName;
                    RAStorageUtil.DeleteReportBlob(oldBlobName);
                }

                var blobName = new StringBuilder(folderName.ToString());
                blobName.Append(Path.DirectorySeparatorChar).Append(filename);
                //upload browser to blob
                RAStorageUtil.UploadReportBlob(blobName.ToString(), uploadFile);
                List<RMCPExportSetting> result = new List<RMCPExportSetting>();
                result.Add(setting);
                result.Add(settingEXO);
                result.Add(settingGoogle);
                exportSettingsDao.SaveOrUpdate(result);
            }
            catch (Exception e)
            {
                logger.Error("NARA settings save to DB error. error details: {0}", e.ToString());
                throw;
            }
            finally
            {
                cacheFile = null;
            }
            return true;
        }

        private bool UploadNaraConfigForGoogleOne(string filename, byte[] cacheFile)
        {
            try
            {
                var validationFile = BytesToStream(cacheFile);             
                //Google
                RMCPExportSetting settingGoogle = new RMCPExportSetting();
                settingGoogle.IsActived = true;
                settingGoogle.FileName = filename;
                settingGoogle.ExportSettingType = (int)ExportSettingType.NARA;
                settingGoogle.SourceFlag = (int)SourceFlag.Google;
                string errorFile = string.Empty;
                try
                {
                    using (ZipArchive zip = new ZipArchive(validationFile))
                    {
                        var entry = zip.GetEntry("Google NARA Configuration File.xml");
                        if (entry != null)
                        {
                            errorFile = GoogleNARAFile;
                            using var ms = new MemoryStream();
                            entry.Open().CopyTo(ms);
                            settingGoogle.ExportConfig = ms.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("read file error:{0}", e.ToString());
                    throw new ExportConfigZipIllegalException(string.Format(I18NEntity.GetString("RM_ES_ReadFileError"), errorFile));
                }

                //validation
                var errorFiles = new List<string>();
                if (settingGoogle.ExportConfig == null)
                {
                    errorFiles.Add(GoogleNARAFile);
                }
                if (errorFiles.Count() > 0)
                {
                    logger.Error("{0} can`t found in naa zip file", string.Join(",", errorFiles));
                    throw new ExportConfigZipIllegalException(I18NEntity.GetString("RM_ES_NAA_ConfigFileIncomplete"));
                }

                //upload file to storage
                var uploadFile = BytesToStream(cacheFile);
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                StringBuilder folderName = null;
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    folderName = new StringBuilder(Path.Combine(tenantFolderName, JobReportUtility.ExportNARAConfig));
                }

                var oldsetting = exportSettingsDao.GetExportSetting((int)ExportSettingType.NARA);
                ArgumentCheck.NotNull(folderName, nameof(folderName));
                if (oldsetting != null)
                {
                    var oldBlobName = folderName.ToString() + Path.DirectorySeparatorChar + oldsetting.FileName;
                    RAStorageUtil.DeleteReportBlob(oldBlobName);
                }

                var blobName = new StringBuilder(folderName.ToString());
                blobName.Append(Path.DirectorySeparatorChar).Append(filename);
                //upload browser to blob
                RAStorageUtil.UploadReportBlob(blobName.ToString(), uploadFile);
                List<RMCPExportSetting> result = new List<RMCPExportSetting>();
                result.Add(settingGoogle);
                exportSettingsDao.SaveOrUpdate(result);
            }
            catch (Exception e)
            {
                logger.Error("NARA settings save to DB error. error details: {0}", e.ToString());
                throw;
            }
            finally
            {
                cacheFile = null;
            }
            return true;
        }
        public void DeleteMigratedVeoConfig()
        {
            var condition = VEOV3CommonMethod.HasUpgradedVEOV3() ? (Func<RMCPExportSetting, bool>)(s => s.VEOContent != null && s.VEOHistory != null) : (s => s.VEOContent == null && s.VEOHistory == null);
            RMCPExportSetting exportSettings = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition);
            if (exportSettings != null && exportSettings.DAOMigrated == true)
            {
                var veoConfig = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.VEO);
                RAStorageUtil.DeleteReportBlob(veoConfig);

                exportSettingsDao.Delete((int)ExportSettingType.VEO);

                try
                {
                    var tenantFolderName = JobReportUtility.GetTenantIdentity();
                    RAStorageUtil.DeleteReportBlobs($"{tenantFolderName}/{JobReportUtility.ExportVEOConfig}");
                }
                catch (Exception e)
                {
                    logger.Warn("Delete VEO configure error:{0}", e.ToString());
                }
            }
        }
        
        
        public async Task<StorageInfoExportSetting> GetStorageInfoInExportSettingsAsync()
        {
            StorageInfoExportSetting storageInfoExportSetting = new();
            SettingProfileDto mDto = new()
            {
                Type = (int)SettingProfilesType.ExportLocationDevice,
                Name = "UsingExportLocationDevice"
            };
            var isNewOpusTenant = TenantService.IsNewOpusTenant();
            if (isNewOpusTenant)
            {
                var dto = SettingProfileDao.Load(mDto);
                if (dto != null)
                {
                    var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(dto);
                    storageInfoExportSetting.CurrentExportLocationId = tempDto.Settings;
                }
                else
                {
                    storageInfoExportSetting.CurrentExportLocationId = null;
                }
            }
            else
            {
                var exportLocationId = GlobalSettingService.GetCurrentExportLocationId();
                logger.Info($"Selected location id: {exportLocationId}");
                if (!string.IsNullOrEmpty(exportLocationId)) 
                {
                    storageInfoExportSetting.CurrentExportLocationId = exportLocationId;
                }
                else
                {
                    storageInfoExportSetting.CurrentExportLocationId = null;
                }
            }
            
            if (isNewOpusTenant)
            {
                var storageFilterResult = await StorageDeviceDao.GetStoragesDeviceByFilterAsync(true);        
                foreach (var storage in storageFilterResult)
                {
                    storageInfoExportSetting.StorageInfo.Add(new StorageIdAndName { Id = storage.Id.ToString(), Name = storage.Name });
                }
            }
            else
            {
                var allExportLocations = await GlobalSettingService.GetAllExportLocationAsync();
                foreach (var exportLocation in allExportLocations)
                {
                    storageInfoExportSetting.StorageInfo.Add(new StorageIdAndName { Id = exportLocation.Id, Name = exportLocation.Name });
                }
            }

            return storageInfoExportSetting;
        }


        private bool DeleteVeoConfig(string filename)
        {
            RMCPExportSetting exportSettings = new RMCPExportSetting();
            bool hasUpgradedVEOV3 = VEOV3CommonMethod.HasUpgradedVEOV3();
            var condition = hasUpgradedVEOV3 ? (Func<RMCPExportSetting, bool>)(s => s.VEOContent != null && s.VEOHistory != null) : (s => s.VEOContent == null && s.VEOHistory == null);
            exportSettings = exportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition);
            if (exportSettings != null)
            {
                var veoconfig = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.VEO);
                filename = exportSettings.FileName;
                RAStorageUtil.DeleteReportBlob(veoconfig);
            }
                exportSettingsDao.Delete((int)ExportSettingType.VEO);
            try
            {
                if (!hasUpgradedVEOV3)
                {
                    var tenantFolderName = JobReportUtility.GetTenantIdentity();
                    RAStorageUtil.DeleteReportBlobs($"{tenantFolderName}/{JobReportUtility.ExportVEOConfig}");
                }
            }
            catch (Exception e)
            {
                logger.Warn("Delete VEO configure error:{0}", e.ToString());
            }
            return !string.IsNullOrEmpty(filename);
        }

        private bool DeleteNaaConfig(string filename)
        {
            var exportSettings = exportSettingsDao.GetExportSetting((int)ExportSettingType.NAA);
            if (exportSettings != null)
            {
                var nnaconfig = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.NAA);
                filename = exportSettings.FileName;
                RAStorageUtil.DeleteReportBlob(nnaconfig);
            }
            exportSettingsDao.Delete((int)ExportSettingType.NAA);
            try
            {
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                RAStorageUtil.DeleteReportBlobs($"{tenantFolderName}/{JobReportUtility.ExportNAAConfig}");
            }
            catch (Exception e)
            {
                logger.Warn("Delete NAA configure error:{0}", e.ToString());
            }
            return !string.IsNullOrEmpty(filename);
        }
        private bool DeleteNaraConfig(string filename)
        {
            var exportSettings = exportSettingsDao.GetExportSetting((int)ExportSettingType.NARA);
            if (exportSettings != null)
            {
                var nnaconfig = GetConfigureFileBlobName(exportSettings.FileName, ExportSettingType.NARA);
                filename = exportSettings.FileName;
                RAStorageUtil.DeleteReportBlob(nnaconfig);
            }
            exportSettingsDao.Delete((int)ExportSettingType.NARA);
            try
            {
                var tenantFolderName = JobReportUtility.GetTenantIdentity();
                RAStorageUtil.DeleteReportBlobs($"{tenantFolderName}/{JobReportUtility.ExportNARAConfig}");
            }
            catch (Exception e)
            {
                logger.Warn("Delete NARA configure error:{0}", e.ToString());
            }
            return !string.IsNullOrEmpty(filename);
        }
        private byte[] StreamToBytes(Stream stream)
        {
            //byte[] bytes = new byte[stream.Length];
            //stream.Read(bytes, 0, bytes.Length);
            //stream.Seek(0, SeekOrigin.Begin);

            //Quality Issue
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        private string GetConfigureFileBlobName(string fileName, ExportSettingType exportSettingType)
        {
            var folderName = "";
            switch (exportSettingType)
            {
                case ExportSettingType.VEO:
                    folderName = JobReportUtility.ExportVEOConfig;
                    break;
                case ExportSettingType.NAA:
                    folderName = JobReportUtility.ExportNAAConfig;
                    break;
                case ExportSettingType.NARA:
                    folderName = JobReportUtility.ExportNARAConfig;
                    break;
                default:
                    break;
            }
            return $"{JobReportUtility.GetTenantIdentity()}/{folderName}/{fileName}";
        }

        public async Task<StorageInfoExportSetting> GetGoogleStorageInfoInExportSettingsAsync()
        {
            StorageInfoExportSetting storageInfoExportSetting = new();
            SettingProfileDto mDto = new()
            {
                Type = (int)SettingProfilesType.ExportLocationDevice,
                Name = "UsingExportLocationDevice"
            };
            var isNewOpusTenant = TenantService.IsNewOpusTenant();
            if (isNewOpusTenant)
            {
                var dto = SettingProfileDao.Load(mDto);
                if (dto != null)
                {
                    var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(dto);
                    storageInfoExportSetting.CurrentExportLocationId = tempDto.Settings;
                }
                else
                {
                    storageInfoExportSetting.CurrentExportLocationId = null;
                }
            }
            else
            {
                var exportLocationId = GlobalSettingService.GetCurrentExportLocationId();
                logger.Info($"Selected location id: {exportLocationId}");
                if (!string.IsNullOrEmpty(exportLocationId))
                {
                    storageInfoExportSetting.CurrentExportLocationId = exportLocationId;
                }
                else
                {
                    storageInfoExportSetting.CurrentExportLocationId = null;
                }
            }

            if (isNewOpusTenant)
            {
                var storageFilterResult = await StorageDeviceDao.GetGoogleStoragesDeviceAsync();
                foreach (var storage in storageFilterResult)
                {
                    storageInfoExportSetting.StorageInfo.Add(new StorageIdAndName { Id = storage.Id.ToString(), Name = storage.Name });
                }
            }
            else
            {
                var allExportLocations = await GlobalSettingService.GetAllExportLocationAsync();
                allExportLocations = allExportLocations.Where(x => x.Type == (int)StorageDeviceType.SFTP || x.Type == (int)StorageDeviceType.CloudAzure || x.Type == (int)StorageDeviceType.Google).ToList();
                foreach (var exportLocation in allExportLocations)
                {
                    storageInfoExportSetting.StorageInfo.Add(new StorageIdAndName { Id = exportLocation.Id, Name = exportLocation.Name });
                }
            }

            return storageInfoExportSetting;
        }
    }
}
