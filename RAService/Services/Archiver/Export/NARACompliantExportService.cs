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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.ControlPanel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Graph.Models;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml.Linq;

namespace AvePoint.RA.Service.Services.Archiver.Export
{
    public class NARACompliantExportService : CompliantExporter
    {
        private IExportSettingService exportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        private IExportSettingsDao exportSettingsDao => PlatformWindsorManager.GetService<IExportSettingsDao>();
        private ILicenseHelperService licenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private RALogger logger = RALogger.GetInstance(typeof(NARACompliantExportService));
        const string NARAFile = "NARA Configuration File.xml";
        const string EXONARAFile = "EXO NARA Configuration File.xml";
        const string GoogleNARAFile = "Google NARA Configuration File.xml";
        const string TemplateFileName = "NARA Configuration File.zip";

        public override List<BaseExportInfo> LoadExportInfos()
        {
            try
            {
                var nARAExportSettings = exportSettingsDao.GetExportSettings((int)ExportSettingType.NARA).Where(_ => _.ExportConfig != null).ToList();
                if (!nARAExportSettings.Any())
                {
                    return LoadExportInfoFromTemplate();
                }
                return LoadExportInfoFromDB(nARAExportSettings);
            }
            catch (Exception ex)
            {
                logger.Error("Load export NARA infos have errors: {0}", ex);
                return new List<BaseExportInfo>();
            }
        }

        private List<BaseExportInfo> LoadExportInfoFromTemplate(bool isNeedCheckLicense = true)
        {
            var defaultNaraTemplatePath = exportSettingService.DownloadTemplateZip(TemplateFileName);
            if (!File.Exists(defaultNaraTemplatePath))
            {
                logger.Error($"NARA template zip file not found at path: {defaultNaraTemplatePath}");
                return new List<BaseExportInfo>();
            }

            using var defaultNaraTemplateStream = new FileStream(defaultNaraTemplatePath, FileMode.Open, FileAccess.Read);
            return LoadExportInfoFromZipStream(defaultNaraTemplateStream, isNeedCheckLicense);
        }

        private List<BaseExportInfo> LoadExportInfoFromDB(List<RMCPExportSetting> nARAExportSettings)
        {
            var result = new List<BaseExportInfo>();
            var sharePointSetting = nARAExportSettings.Where(_ => _.SourceFlag == (int)SourceFlag.SharePoint).FirstOrDefault();
            var hasGoogleLicense = licenseHelperService.HasOpusGoogleLicense;
            var hasILLicense = licenseHelperService.HasOpusILLicense;
            var hasSOLicense = licenseHelperService.HasOpusSOLicense;
            if(hasILLicense || hasSOLicense)
            {
                if (sharePointSetting != null)
                {
                    var sharePointXML = Encoding.UTF8.GetString(sharePointSetting.ExportConfig).Trim('\uFEFF');
                    var exportInfo = ConvertXmlToBaseExportInfo(sharePointXML);
                    exportInfo.SourceFlag = SourceFlag.SharePoint;
                    exportInfo.ExportType = ExportTypeValue.NARA;
                    result.Add(exportInfo);
                }
            }
            if (hasILLicense)
            {
                var exoSetting = nARAExportSettings.Where(_ => _.SourceFlag == (int)SourceFlag.Exchange).FirstOrDefault();
                if (exoSetting != null)
                {
                    var exoXML = Encoding.UTF8.GetString(exoSetting.ExportConfig).Trim('\uFEFF');
                    var exportInfo = ConvertXmlToBaseExportInfo(exoXML);
                    exportInfo.SourceFlag = SourceFlag.Exchange;
                    exportInfo.ExportType = ExportTypeValue.NARA;
                    result.Add(exportInfo);
                }
            }
            if (hasGoogleLicense)
            {
                var googleSetting = nARAExportSettings.Where(_ => _.SourceFlag == (int)SourceFlag.Google).FirstOrDefault();
                if (googleSetting != null)
                {
                    var googleXML = Encoding.UTF8.GetString(googleSetting.ExportConfig).Trim('\uFEFF');
                    var exportInfo = ConvertXmlToBaseExportInfo(googleXML);
                    exportInfo.SourceFlag = SourceFlag.Google;
                    exportInfo.ExportType = ExportTypeValue.NARA;
                    result.Add(exportInfo);
                }
            }
            return result;
        }

        private List<BaseExportInfo> LoadExportInfoFromZipStream(Stream zipExportStreamFile, bool isNeedCheckLicense)
        {
            var hasGoogleLicense = licenseHelperService.HasOpusGoogleLicense;
            var hasILLicense = licenseHelperService.HasOpusILLicense;
            var hasSOLicense = licenseHelperService.HasOpusSOLicense;
            var result = new List<BaseExportInfo>();
            using (ZipArchive zip = new ZipArchive(zipExportStreamFile))
            {
                foreach (var entry in zip.Entries)
                {
                    BaseExportInfo exportInfo = null;
                    if(isNeedCheckLicense && (!hasSOLicense && !hasILLicense))
                    {
                        if (entry.Name.Equals(NARAFile)) continue;
                    }
                    if(isNeedCheckLicense && !hasILLicense)
                    {
                        if (entry.Name.Equals(EXONARAFile)) continue;
                    }
                    if (isNeedCheckLicense && !hasGoogleLicense)
                    {
                        if (entry.Name.Equals(GoogleNARAFile)) continue;
                    }
                    exportInfo = ConvertXmlToBaseExportInfo(ExportStreamHelper.GetStringFromStream(entry.Open()));
                    switch (entry.Name)
                    {
                        case EXONARAFile:
                            exportInfo.SourceFlag = SourceFlag.Exchange;
                            break;
                        case GoogleNARAFile:
                            exportInfo.SourceFlag = SourceFlag.Google;
                            break;
                        case NARAFile:
                            exportInfo.SourceFlag = SourceFlag.SharePoint;
                            break;
                    }
                    exportInfo.ExportType = ExportTypeValue.NARA;
                    result.Add(exportInfo);
                }
            }
            return result;
        }

        public override void SaveAndUploadExportInfos(IEnumerable<BaseExportInfo> exportInfos)
        {
            var nARAInfos = exportInfos?.Where(_ => _.ExportType == ExportTypeValue.NARA) ?? [];
            string fileName = string.Empty;
            var tempFolderPath = Path.GetTempPath();
            var guid = Guid.NewGuid().ToString();
            var unZipFolderPath = Path.Combine(tempFolderPath, guid);
            var zipFolderPath = Path.Combine(tempFolderPath, $"{guid}.zip");
            bool hasEXOData = false, hasGoogleData = false, hasSPOData = false;
            (string exportFolderPath, string zipFileName) = FindExportInfoFileInfo(unZipFolderPath);
            if (!Directory.Exists(exportFolderPath))
                Directory.CreateDirectory(exportFolderPath);
            foreach (var exportInfo in exportInfos)
            {
                var xmlInfo = ConvertBaseExportInfoToXml(exportInfo);
                switch (exportInfo.SourceFlag)
                {
                    case SourceFlag.Exchange:
                        hasEXOData = true;
                        fileName = EXONARAFile;
                        break;
                    case SourceFlag.Google:
                        hasGoogleData = true;
                        fileName = GoogleNARAFile;
                        break;
                    case SourceFlag.OneDrive:
                    case SourceFlag.SharePoint:
                        hasSPOData = true;
                        fileName = NARAFile;
                        break;
                }
                File.WriteAllText(Path.Combine(exportFolderPath, fileName), xmlInfo);
            }
            List<BaseExportInfo> exportInfoTemplate = null;
            if (!hasEXOData)
            {
                WriteExportInfoData(SourceFlag.Exchange, EXONARAFile);
            }
            if (!hasSPOData)
            {
                WriteExportInfoData(SourceFlag.SharePoint, NARAFile);
            }
            if (!hasGoogleData)
            {
                WriteExportInfoData(SourceFlag.Google, GoogleNARAFile);
            }
            void WriteExportInfoData(SourceFlag sourceFlag, string fileName) {
                var setting = exportSettingsDao.GetExportSettings((int)ExportSettingType.NARA).Where(_ => _.SourceFlag == (int)sourceFlag).FirstOrDefault();
                if (setting != null)
                {
                    var xml = Encoding.UTF8.GetString(setting.ExportConfig).Trim('\uFEFF');
                    File.WriteAllText(Path.Combine(exportFolderPath, fileName), xml);
                }
                else
                {
                    if (exportInfoTemplate == null)
                    {
                        exportInfoTemplate = LoadExportInfoFromTemplate(false);
                    }
                    var templateSetting = exportInfoTemplate.Where(_ => _.SourceFlag == sourceFlag).FirstOrDefault();
                    if (templateSetting != null)
                    {
                        var xml = ConvertBaseExportInfoToXml(templateSetting);
                        File.WriteAllText(Path.Combine(exportFolderPath, fileName), xml);
                    }
                }
            }
            ExportStreamHelper.UploadFileToStorage(unZipFolderPath, zipFolderPath, zipFileName, exportSettingService.UploadNaraConfig);
        }

        private (string fullPath, string fileName) FindExportInfoFileInfo(string unZipFolderPath)
        {
            var setting = exportSettingsDao.GetExportSettings((int)ExportSettingType.NARA).FirstOrDefault();
            if (setting != null)
            {
                using (var zipStream = exportSettingService.DownloadNARAConfigureFileToStream(out _))
                {
                    using(var zip = new ZipArchive(zipStream))
                    {
                        var exportInfoEntry = zip.Entries.Where(_ => _.Name.Equals(NARAFile)
                            || _.Name.Equals(EXONARAFile) || _.Name.Equals(GoogleNARAFile)).FirstOrDefault();
                        if (exportInfoEntry != null)
                        {
                            var fullPath = Path.Combine(unZipFolderPath, exportInfoEntry.FullName);
                            var directoryPath = Path.GetDirectoryName(fullPath);
                            return (directoryPath, setting?.FileName ?? TemplateFileName);
                        }
                    }
                }
            }
            return (unZipFolderPath ,setting?.FileName ?? TemplateFileName);
        }

        protected override string ConvertBaseExportInfoToXml(BaseExportInfo baseExportInfo)
        {
            var nARAInfo = baseExportInfo as NARAInfo;
            var rootName = string.Empty;
            switch (nARAInfo.SourceFlag)
            {
                case SourceFlag.Exchange:
                    rootName = "EXONARAConfig";
                    break;
                case SourceFlag.Google:
                    rootName = "GoogleNARAConfig";
                    break;
                case SourceFlag.SharePoint:
                case SourceFlag.OneDrive:
                default:
                    rootName = "NARAConfig";
                    break;
            }
            var rootElement = new XElement(rootName);
            var columns = nARAInfo.ExportColumnInfoes?.OrderBy(_ => _.Order).ToList() ?? [];
            foreach (var info in columns)
            {
                var subElement = new XElement("ColumnMapping",
                    new XAttribute("DisplayName", info.DisplayName ?? ""),
                    new XAttribute("MappedKey", info.MappedKey ?? ""),
                    new XAttribute("AdditionalMetadata", info.Additional ? "true" : "false"));
                if (!string.IsNullOrEmpty(info.Prefix)) subElement.SetAttributeValue("Prefix", info.Prefix);
                if (info.Format != null) subElement.SetAttributeValue("DateFormat", info.Format);
                if (!string.IsNullOrEmpty(info.DefaultValue)) subElement.SetAttributeValue("DefaultValue", info.DefaultValue);
                rootElement.Add(subElement);
            }
            var result = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                rootElement
            );
            return result.ToString();
        }

        protected override BaseExportInfo ConvertXmlToBaseExportInfo(string xml)
        {
            var rootElement = XDocument.Parse(xml);
            var config = new NARAInfo();
            if (config.ExportColumnInfoes == null)
            {
                config.ExportColumnInfoes = new List<ExportColumnInfo>();
            }
            int order = 0;
            foreach (var el in rootElement.Descendants("ColumnMapping"))
            {
                var mapping = new ExportColumnInfo
                {
                    Order = order++,
                    DisplayName = (string)el.Attribute("DisplayName"),
                    MappedKey = (string)el.Attribute("MappedKey"),
                    Additional = bool.TryParse((string)el.Attribute("AdditionalMetadata"), out var parseResult) && parseResult,
                    DefaultValue = (string)el.Attribute("DefaultValue"),
                    Prefix = (string)el.Attribute("Prefix"),
                    Format = (string)el.Attribute("DateFormat"),
                };
                config.ExportColumnInfoes.Add(mapping);
            }
            return config;
        }
    }
}
