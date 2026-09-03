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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Common;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AvePoint.RA.Service.Services.Archiver.Export
{
    public class VEOCompliantExportService : CompliantExporter
    {
        private IExportSettingService _exportSettingService;
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService(ref _exportSettingService);
        private IExportSettingsDao _exportSettingsDao;
        private IExportSettingsDao ExportSettingsDao => PlatformWindsorManager.GetService(ref _exportSettingsDao);

        private IRMKeyValueDao _keyValueDao;
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService(ref _keyValueDao);
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(VEOCompliantExportService));

        public override List<BaseExportInfo> LoadExportInfos()
        {
            if (!VEOV3CommonMethod.HasUpgradedVEOV3())
            {
                throw new InvalidOperationException("VEO V3 configuration upgrade has not been completed. Cannot load VEO V3 export settings.");
            }

            var exportInfos = new List<BaseExportInfo>();
            var veoSettings = ExportSettingsDao.GetExportSettings((int)ExportSettingType.VEO)
                                               .Where(s => s.VEOContent != null).ToList();

            string sharePointXmlContent;
            string exchangeXmlContent;

            if (veoSettings.Any())
            {
                LoadConfigurationsFromDb(veoSettings, out sharePointXmlContent, out exchangeXmlContent);
            }
            else
            {
                LoadConfigurationsFromTemplate(out sharePointXmlContent, out exchangeXmlContent);
            }

            if (!string.IsNullOrWhiteSpace(sharePointXmlContent))
            {
                exportInfos.Add(CreateExportInfo(sharePointXmlContent, SourceFlag.SharePoint));
            }

            if (!string.IsNullOrWhiteSpace(exchangeXmlContent))
            {
                exportInfos.Add(CreateExportInfo(exchangeXmlContent, SourceFlag.Exchange));
            }

            return exportInfos;
        }

        private void LoadConfigurationsFromDb(List<RMCPExportSetting> veoSettings, out string sharePointXml, out string exchangeXml)
        {
            _logger.Info("Loading user-defined VEO V3 configurations from database.");
            sharePointXml = string.Empty;
            exchangeXml = string.Empty;

            var spSetting = veoSettings.FirstOrDefault(s => s.SourceFlag == (int)SourceFlag.SharePoint);
            if (spSetting?.VEOContent != null)
            {
                sharePointXml = Encoding.UTF8.GetString(spSetting.VEOContent).Trim('\uFEFF');
            }
            else
            {
                _logger.Warn("SharePoint VEO V3 configuration not found in database.");
            }

            var exoSetting = veoSettings.FirstOrDefault(s => s.SourceFlag == (int)SourceFlag.Exchange);
            if (exoSetting?.VEOContent != null)
            {
                exchangeXml = Encoding.UTF8.GetString(exoSetting.VEOContent).Trim('\uFEFF');
            }
            else
            {
                _logger.Warn("Exchange VEO V3 configuration not found in database.");
            }
        }

        private void LoadConfigurationsFromTemplate(out string sharePointXml, out string exchangeXml)
        {
            _logger.Info("User-defined VEO V3 configurations not found, loading from default template.");
            sharePointXml = string.Empty;
            exchangeXml = string.Empty;

            string templateZipPath = ExportSettingService.DownloadTemplateZip(VEOV3CommonString.VEOV3TemplateZipFile);
            if (!File.Exists(templateZipPath))
            {
                _logger.Error($"VEO V3 template zip file not found at path: {templateZipPath}");
                return;
            }

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(templateZipPath))
                {
                    sharePointXml = ReadEntryContent(archive, VEOV3CommonString.VEOContent);
                    exchangeXml = ReadEntryContent(archive, VEOV3CommonString.EXOVEOContent);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to read VEO V3 template zip file.", ex);
            }
        }


        private string ReadEntryContent(ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);
            if (entry == null)
            {
                _logger.Warn($"'{entryName}' not found in the VEO V3 template zip.");
                return string.Empty;
            }

            using (var reader = new StreamReader(entry.Open()))
            {
                return reader.ReadToEnd();
            }
        }

        private BaseExportInfo CreateExportInfo(string xmlContent, SourceFlag sourceFlag)
        {
            try
            {
                var exportInfo = ConvertXmlToBaseExportInfo(xmlContent);
                SetSourceFladAndExportType(exportInfo, sourceFlag);
                return exportInfo;
            }
            catch (Exception ex)
            {
                _logger.Error("Error parsing VEO V3 XML content.", ex);
                //throw;

            }
            return null;

        }

        private void SetSourceFladAndExportType(BaseExportInfo exportInfo, SourceFlag sourceFlag)
        {
            VEOInfo vEOInfo = exportInfo as VEOInfo;
            if (vEOInfo == null)
            {
                return;
            }
            vEOInfo.SourceFlag = sourceFlag;
            vEOInfo.ExportType = Contract.Global.Object.ExportTypeValue.VEO;
            foreach (var child in vEOInfo.ChildVEOInfo ?? new())
            {
                SetSourceFladAndExportType(child, sourceFlag);
            }

            foreach (var child in vEOInfo.ChildTable ?? new())
            {
                SetSourceFladAndExportType(child, sourceFlag);
            }
        }

        public override void SaveAndUploadExportInfos(IEnumerable<BaseExportInfo> exportInfos)
        {
            var workspace = new VEOExportWorkspace();
            try
            {
                if (!VEOV3CommonMethod.HasUpgradedVEOV3())
                {
                    throw new InvalidOperationException("VEO V3 configuration upgrade has not been completed. Cannot load VEO V3 export settings.");
                }

                var existingSettings = ExportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).Where(setting => setting.VEOContent != null && setting.VEOContent.Length > 0).ToList();
                string zipFileName = GetBaseConfiguration(workspace, existingSettings);

                var settingsToUpdate = UpdateConfigFilesAndPrepareEntities(exportInfos, workspace.UnzippedFolderPath, zipFileName);

                PackageAndUpload(workspace, zipFileName);

                _logger.Info("Successfully saved and uploaded VEO V3 export configurations.");
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while saving VEO V3 export configurations.", ex);
                throw;
            }
            finally
            {
                workspace.Cleanup();
            }
        }
        private string GetBaseConfiguration(VEOExportWorkspace workspace, List<RMCPExportSetting> existingSettings)
        {
            string zipFileName;
            if (existingSettings.Any(s => !string.IsNullOrEmpty(s.FileName)))
            {
                _logger.Info("Downloading existing VEO V3 configuration from storage.");
                zipFileName = existingSettings.First(s => !string.IsNullOrEmpty(s.FileName)).FileName;
                using (var stream = ExportSettingService.DownloadConfigureFileToStream(out _))
                {
                    using (var fileStream = new FileStream(workspace.ZipPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
            else
            {
                _logger.Info("Copying VEO V3 configuration from default template.");
                zipFileName = VEOV3CommonString.VEOV3TemplateZipFile;
                string templatePath = ExportSettingService.DownloadTemplateZip(zipFileName);
                File.Copy(templatePath, workspace.ZipPath);
            }

            ZipFile.ExtractToDirectory(workspace.ZipPath, workspace.UnzippedFolderPath);
            return zipFileName;
        }

        private List<RMCPExportSetting> UpdateConfigFilesAndPrepareEntities(IEnumerable<BaseExportInfo> exportInfos, string unzippedFolderPath, string zipFileName)
        {
            var settingsToUpdate = new List<RMCPExportSetting>();

            var spInfo = exportInfos.FirstOrDefault(i => i.SourceFlag == SourceFlag.SharePoint);
            if (spInfo != null)
            {
                string xmlContent = ConvertBaseExportInfoToXml(spInfo);
                string filePath = FindFilePath(unzippedFolderPath, VEOV3CommonString.VEOContent);
                File.WriteAllText(filePath, xmlContent);
                settingsToUpdate.Add(new RMCPExportSetting
                {
                    ExportSettingType = (int)ExportSettingType.VEO,
                    SourceFlag = (int)SourceFlag.SharePoint,
                    VEOContent = Encoding.UTF8.GetBytes(xmlContent),
                    FileName = zipFileName,
                    IsActived = true
                });
            }

            var exoInfo = exportInfos.FirstOrDefault(i => i.SourceFlag == SourceFlag.Exchange);
            if (exoInfo != null)
            {
                string xmlContent = ConvertBaseExportInfoToXml(exoInfo);
                string filePath = FindFilePath(unzippedFolderPath, VEOV3CommonString.EXOVEOContent);
                File.WriteAllText(filePath, xmlContent);
                settingsToUpdate.Add(new RMCPExportSetting
                {
                    ExportSettingType = (int)ExportSettingType.VEO,
                    SourceFlag = (int)SourceFlag.Exchange,
                    VEOContent = Encoding.UTF8.GetBytes(xmlContent),
                    FileName = zipFileName,
                    IsActived = true
                });
            }

            return settingsToUpdate;
        }

        private string FindFilePath(string rootPath, string fileName)
        {
            var files = Directory.GetFiles(rootPath, fileName, SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                throw new FileNotFoundException($"Configuration file '{fileName}' not found within the provided structure.", fileName);
            }
            if (files.Length > 1)
            {
                _logger.Warn($"Multiple instances of configuration file '{fileName}' found. Using the first one: {files[0]}");
            }
            return files[0];
        }

        private void PackageAndUpload(VEOExportWorkspace workspace, string zipFileName)
        {
            if (File.Exists(workspace.ZipPath))
            {
                File.Delete(workspace.ZipPath);
            }
            ZipFile.CreateFromDirectory(workspace.UnzippedFolderPath, workspace.ZipPath);

            using (var stream = new FileStream(workspace.ZipPath, FileMode.Open, FileAccess.Read))
            {
                ExportSettingService.UploadVEOV3Config(zipFileName, stream);
            }
        }

        private class VEOExportWorkspace
        {
            public string ZipPath { get; }
            public string UnzippedFolderPath { get; }

            public VEOExportWorkspace()
            {
                string tempRoot = Path.GetTempPath();
                string guid = Guid.NewGuid().ToString();
                ZipPath = Path.Combine(tempRoot, $"{guid}.zip");
                UnzippedFolderPath = Path.Combine(tempRoot, guid);
                Directory.CreateDirectory(UnzippedFolderPath);
            }

            public void Cleanup()
            {
                if (Directory.Exists(UnzippedFolderPath))
                {
                    Directory.Delete(UnzippedFolderPath, true);
                }
                if (File.Exists(ZipPath))
                {
                    File.Delete(ZipPath);
                }
            }
        }

        protected override string ConvertBaseExportInfoToXml(BaseExportInfo baseExportInfo)
        {
            if (baseExportInfo is null)
            {
                throw new ArgumentNullException(nameof(baseExportInfo));
            }

            if (baseExportInfo is not VEOInfo veoInfo)
            {
                throw new NotSupportedException($"The provided export info type '{baseExportInfo.GetType().FullName}' is not supported.");
            }

            if (string.IsNullOrWhiteSpace(veoInfo.TreeNodeName))
            {
                throw new InvalidOperationException("CurrentNodeName must be populated before serialization.");
            }

            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new Utf8StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                WriteVeoInfo(xmlWriter, veoInfo, true);
            }

            return stringWriter.ToString();
        }

        protected override BaseExportInfo ConvertXmlToBaseExportInfo(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentException("XML content must not be null or whitespace.", nameof(xml));
            }

            var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            if (document.Root is null)
            {
                throw new InvalidOperationException("XML document does not contain a root element.");
            }

            return ReadVeoInfo(document.Root);
        }

        private static void WriteVeoInfo(XmlWriter writer, VEOInfo info, bool isContainer = false)
        {
            writer.WriteStartElement(info.TreeNodeName);

            //if (!isContainer)
            //{
            WriteAttribute(writer, "MetadataName", info.MetadataName);
            WriteAttribute(writer, "DefaultValue", info.DefaultValue);
            WriteAttribute(writer, "ExchangeMetadata", info.ExchangeMetadata);
            WriteAttribute(writer, "SharePointMetadata", info.SharePointMetadata);
            WriteAttribute(writer, "ExchangeMetadataAsSource", info.ExchangeMetadataAsSource);
            WriteAttribute(writer, "SharePointMetadataAsSource", info.SharePointMetadataAsSource);
            //}            

            foreach (var child in info.ChildVEOInfo ?? new())
            {
                if (child is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(child.TreeNodeName))
                {
                    throw new InvalidOperationException("Each child VEO node must define a CurrentNodeName before serialization.");
                }

                WriteVeoInfo(writer, child, true);
            }

            foreach (var child in info.ChildTable ?? new())
            {
                if (child is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(child.TreeNodeName))
                {
                    throw new InvalidOperationException("Each child VEO node must define a CurrentNodeName before serialization.");
                }

                WriteVeoInfo(writer, child);
            }

            writer.WriteEndElement();
        }

        private static void WriteAttribute(XmlWriter writer, string attributeName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteAttributeString(attributeName, value);
            }
        }

        private static void WriteAttribute(XmlWriter writer, string attributeName, bool? value)
        {
            if (value.HasValue)
            {
                writer.WriteAttributeString(attributeName, value.Value ? "true" : "false");
            }
        }

        private static VEOInfo ReadVeoInfo(XElement element)
        {
            var info = new VEOInfo
            {
                TreeNodeName = element.Name.LocalName,
                MetadataName = GetAttributeValue(element, "MetadataName"),
                DefaultValue = GetAttributeValue(element, "DefaultValue"),
                ExchangeMetadata = GetAttributeValue(element, "ExchangeMetadata"),
                SharePointMetadata = GetAttributeValue(element, "SharePointMetadata"),
                ExchangeMetadataAsSource = GetBooleanAttributeValue(element, "ExchangeMetadataAsSource"),
                SharePointMetadataAsSource = GetBooleanAttributeValue(element, "SharePointMetadataAsSource")
            };

            IEnumerable<VEOInfo> infos = element.Elements().Select(ReadVeoInfo);
            info.ChildVEOInfo = infos.Where(info => IsContainerNode(info)).ToList();
            info.ChildTable = infos.Where(info => !IsContainerNode(info)).ToList();
            return info;
        }

        private static bool IsContainerNode(VEOInfo info)
        {
            return (info.DefaultValue is null &&
                   info.ExchangeMetadata is null &&
                   info.SharePointMetadata is null &&
                   info.ExchangeMetadataAsSource is null &&
                   info.SharePointMetadataAsSource is null)
                   || info?.ChildTable?.Any() == true || info?.ChildVEOInfo?.Any() == true;
        }

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            var attribute = element.Attributes()
                .FirstOrDefault(a => string.Equals(a.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));

            return attribute?.Value;
        }

        private static bool? GetBooleanAttributeValue(XElement element, string attributeName)
        {
            var value = GetAttributeValue(element, attributeName);
            if (value is null)
            {
                return null;
            }

            if (bool.TryParse(value, out var parsed))
            {
                return parsed;
            }

            throw new FormatException($"Could not parse attribute '{attributeName}' with value '{value}' as a boolean.");
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
