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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.RMWeb.Setting;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CsvMetaData = AvePoint.GCommon.Media.StorageService.CsvMetaData;

namespace RAGoogle
{
    internal class GoogleNARAExport : GoogleExportBase, IGoogleExport
    {
        private static AveLogger _logger = AveLogger.GetInstance(typeof(GoogleNARAExport));
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        private GoogleNARAColumnContainer columnArray;

        private const string EXPORTMETADATAFORMAT = "{0}_NARA.CSV";

        //id, metadata 
        private ConcurrentDictionary<string, CsvMetaData> csvParentedMetadataWithPath = new();
        //parent id, metadata
        private ConcurrentDictionary<Tuple<string, string>, ConcurrentBag<CsvMetaData>> csvChildMetadataWithPath = new();
        private GoogleExportInfo medataFileinfo;
        private readonly object lockObj = new object();
        private ConcurrentDictionary<string, string> allFolderIdAndNames = new();
        private string _disposalClass = string.Empty;
        private ConcurrentBag<GoogleItemData> _folderExportCache = new();

        public GoogleNARAExport(PhysicalDeviceDto deviceDto, string driveName, string jobId, string disposalClass, byte[] NARAConfigFile)
            : base(deviceDto, jobId)
        {
            var pathDrive = SecurityUtils.SafeCombinePath(jobId, driveName);
            InitClass(NARAConfigFile, disposalClass, pathDrive, driveName);
        }

        private void InitClass(byte[] NARAConfigFile, string disposalClass, string pathDrive, string driveName)
        {
            GoogleNARAData.InitConfig(NARAConfigFile);
            _disposalClass = disposalClass;
            medataFileinfo = new GoogleExportInfo
            {
                ContentFilePath = string.Format(EXPORTMETADATAFORMAT, driveName),
                FolderPath = pathDrive
            };
            columnArray = new();
        }
        public void ExportGoogleFolder(GoogleItemData folder)
        {
            _logger.Info($"Start Export GoogleFolder(NARA) {folder.Id}.");
            try
            {
                _folderExportCache.Add(folder);
                //columnArray = new GoogleNARAColumnContainer();
                //DownloadedFileInfo downloadedFile = folder.ToDownloadedFileInfo();
                //CsvMetaData metaData = new CsvMetaData
                //{
                //    CsvMetadataInfo = columnArray.GetCSVFolderListFromColumnValue(downloadedFile, _disposalClass, allFolderIdAndNames[folder.Id])
                //};
                //csvParentedMetadataWithPath.TryAdd(folder.Id, metaData);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while exporting GoogleFolder(NARA). Item id: {folder.Id} and Error: {ex}");
            }
        }
        public ExportStatus ExportGoogleItem(GoogleExportInfo info)
        {
            _logger.Info($"Start Export GoogleItem(NARA) {info.GoogleItem.Id}.");
            ExportStatus exportStatus = new ExportStatus
            {
                State = ExportState.Failed
            };
            try
            {

                string tempFilePath = info.GoogleItem.LocalPath;
                using Stream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                var hashString = GetHashStringFromFileContent(docStream);
                ExportInfo contentInfo = new ExportInfo
                {
                    //keep datetime
                    Created = info.GoogleItem.CreatedTime.ToLocalTime(),
                    Modified = DateTime.Parse(info.GoogleItem.ModifiedTime.ToString()),
                };
                //export content
                docStream.Position = 0;
                ExportResultInfo result = RealVaultExport.ExportContent(contentInfo, info, docStream);

                exportStatus.ExportSize += result.Size;
                exportStatus.State = ExportState.Succeed;
                string exportPath = info.ContentFilePath;
                //export csv medata
                CsvMetaData metaData = new CsvMetaData
                {
                    CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(info.GoogleItem, _disposalClass, exportPath, hashString),
                };
                if (!allFolderIdAndNames.ContainsKey(info.GoogleItem.ParentId))
                {
                    allFolderIdAndNames.TryAdd(info.GoogleItem.ParentId, info.FolderName);
                }
                var parentId = info.GoogleItem.ParentId;
                var parentPath = info.GoogleItem.RelativePath.IndexOf('/') > 0 ? info.GoogleItem.RelativePath.Substring(0, info.GoogleItem.RelativePath.LastIndexOf('/')) : info.GoogleItem.RelativePath;
                var tuple = new Tuple<string, string>(parentId, parentPath);
                if (!csvChildMetadataWithPath.ContainsKey(tuple))
                {
                    csvChildMetadataWithPath.TryAdd(tuple, new ConcurrentBag<CsvMetaData>());
                }
                csvChildMetadataWithPath[tuple].Add(metaData);
                return exportStatus;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while export ExportGoogleItem(NARA).Item id: {info.GoogleItem.Id} and Error: {ex}");
                exportStatus.ErrorMessage = ex.Message;
                return exportStatus;
            }
        }

        private string GetHashStringFromFileContent(Stream fileStream)
        {
            byte[] fileContentByte;
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            using SHA256 alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(fileStream);
            string hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
            return hashString;
        }
        public void ExtensionMethod(params object[] parameter)
        {
            using (PerformanceScope pc = new("NARAExport_ExtensionMethod"))
            {
                List<CsvMetaData> metadataObjs = parameter[0] as List<CsvMetaData>;
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write([0xEF, 0xBB, 0xBF], 0, 3);
                    bool writeHead = false;
                    foreach (var metadata in metadataObjs)
                    {
                        if (!writeHead)
                        {
                            string head = Generate(metadata, true);
                            byte[] headLine = Encoding.UTF8.GetBytes(head);
                            stream.Write(headLine, 0, headLine.Length);
                            writeHead = true;
                        }
                        string content = Generate(metadata, false);
                        byte[] contentLine = Encoding.UTF8.GetBytes(content);
                        stream.Write(contentLine, 0, contentLine.Length);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    SaveSignature(stream, medataFileinfo);
                    var result = RealVaultExport.ExportContent(new ExportInfo(), medataFileinfo, stream);
                    _logger.Info(
                        $"Successfully exported NARAExport csv {medataFileinfo.ContentFilePath}, stream length: {result.Size}");
                }
            }
        }
        private Stream ConvertStringToStream(string input)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
            {
                writer.Write(input);
                writer.Flush();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
        private string Generate(MetaData metaData, bool isHeaderLine)
        {
            char comma = '\u002C';

            char quote = '\u0022';
            var csvMetadataInfo = new StringBuilder();
            var properties = metaData.CsvMetadataInfo;
            for (int i = 0; i < properties.Count; i++)
            {
                var origainalStringValue = isHeaderLine ? properties[i].Name : properties[i].Value;
                origainalStringValue = string.IsNullOrEmpty(origainalStringValue) ? string.Empty : origainalStringValue;
                var stringValue = origainalStringValue.Contains("\"") ? origainalStringValue.Replace("\"", "\"\"") : origainalStringValue;
                if (i < properties.Count - 1)
                    csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", quote, stringValue, comma);
                else csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", quote, stringValue, Environment.NewLine);
            }
            return csvMetadataInfo.ToString();
        }
        private void SaveSignature(Stream csvStream, GoogleExportInfo medatainfo)
        {
            var setting = SettingProfileService.GetExportSignature();
            if (setting.EnableExportSignature)
            {
                _logger.Info("start signature for csv file");
                GoogleExportInfo signatureFile = new GoogleExportInfo();
                signatureFile.ContentFilePath = medatainfo.ContentFilePath.Substring(0, medatainfo.ContentFilePath.LastIndexOf(".")) + "_Signature.txt";
                signatureFile.FolderPath = medatainfo.FolderPath;
                byte[] data;
                byte[] signedHash;
                string rsaParam = setting.SharedParametersJson;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    csvStream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
                using SHA256 alg = SHA256.Create();
                byte[] hash = alg.ComputeHash(data);
                using (RSA rsa = RSA.Create())
                {
                    var par = JsonSerializer.Deserialize<RsaParametersSerializable>(rsaParam)!.ToRSAParameters();
                    rsa.ImportParameters(par);
                    RSAPKCS1SignatureFormatter rsaFormatter = new(rsa);
                    rsaFormatter.SetHashAlgorithm(nameof(SHA256));

                    signedHash = rsaFormatter.CreateSignature(hash);
                }
                string base64Signature = Convert.ToBase64String(signedHash);
                RealVaultExport.ExportContent(new ExportInfo(), signatureFile, ConvertStringToStream(base64Signature));
            }
            else
            {
                _logger.Info("this export job is no need signature");
            }
        }
        public void HandleCSVMetadataFolder()
        {
            if (allFolderIdAndNames.Count == 0)
            {
                return;
            }
            var folders = _folderExportCache.Where(x => allFolderIdAndNames.Keys.Any(y => y == x.Id)).ToList();
            columnArray = new GoogleNARAColumnContainer();
            foreach (var folder in folders)
            {
                DownloadedFileInfo downloadedFile = folder.ToDownloadedFileInfo();
                CsvMetaData metaData = new CsvMetaData
                {
                    CsvMetadataInfo = columnArray.GetCSVFolderListFromColumnValue(downloadedFile, _disposalClass, allFolderIdAndNames[folder.Id])
                };
                csvParentedMetadataWithPath.TryAdd(folder.Id, metaData);
            }
        }

        public List<CsvMetaData> SortCSVMetadata()
        {
            var csvMetaData = new List<CsvMetaData>();
            var csvWithStructure = csvChildMetadataWithPath.OrderBy(x => x.Key.Item2).ToList();

            foreach (var item in csvWithStructure)
            {
                if (csvParentedMetadataWithPath.TryGetValue(item.Key.Item1, out var csvParentFolder))
                {//parent is folder , add it first
                    csvMetaData.Add(csvParentFolder);
                }
                csvMetaData.AddRange(item.Value);
            }
            return csvMetaData;
        }
    }
}
