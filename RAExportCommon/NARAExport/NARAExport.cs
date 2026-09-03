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
using System.Threading.Tasks;
using AvePoint.Wrapper.Backup;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Media.StorageService;
using LOGRESOURCE = Merged18NResources.Export;
using LOGRESOURCEInternationalization = Merged18NResources.ExportForInternationalization;
using System.Reflection;
using AvePoint.Wrapper.Common;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Util.Security;
using System.Text.Json;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using RecordsHotfixMaintenanceService;

namespace RAExportCommon
{
    internal class NARAExport : VaultExportBase, IVaultExport
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool HasInitObjs = false;
        private NARAColumnContainer columnArray = null;
        private AveSPList AveSPList = null;
        private List<string> itemCache = new List<string>();
        public List<CsvMetaData> csvMetadatas = new List<CsvMetaData>();
        private VaultExportInfo medataFileinfo = null;
        private string mDisposalClass = string.Empty;
        private readonly object lockObj = new object();
        private const string SKIPMESSAGE = "StorageOptimization_NARASkipItemAndAttachment";
        private const string CACHEFOLDERNAME = "ExportCache";
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        public string DisposalClass
        {
            get
            {
                return mDisposalClass;
            }
        }
        private void InitObjects(AveSPSite aveSite)
        {
            HasInitObjs = true;
            columnArray = new NARAColumnContainer(aveSite);
        }

        public NARAExport(PhysicalDeviceDto deviceDto, string jobId, string disposalClass, VaultExportFormat format, byte[] NARAConfigFile, byte[] encryptionKey, byte[] encryptionIV)
            : base(deviceDto, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(NARAConfigFile, disposalClass);
        }

        private void InitClass(byte[] naraConfigFile, string disposalClass)
        {
            NARAData.InitConfig(naraConfigFile);
            mDisposalClass = disposalClass;
        }

        public NARAExport(SharePointLocationDto spoDto, AveBPOSAccountInfo user, string siteUrl, string jobId, string disposalClass, VaultExportFormat format, byte[] NARAConfigFile, byte[] encryptionKey, byte[] encryptionIV)
            : base(spoDto,user, siteUrl, jobId, format, encryptionKey, encryptionIV)
        {
            InitClass(NARAConfigFile, disposalClass);
        }

        public ExportStatus ExportSite(AvePoint.Wrapper.Backup.AveSPSite aveSite, VaultExportInfo info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Site(NARA).", aveSite.SPSite.Url.ToString());
            ExportStatus exportStatus = new ExportStatus();
            if (!HasInitObjs)
            {
                InitObjects(aveSite);
            }
            CsvMetaData metaData = new CsvMetaData
            {
                CsvMetadataInfo = columnArray.GetCSVColumnHeadList()
            };
            //exportStatus.ExportSize += RealVaultExport.ExportMetaDataFile(new ExportInfo(), info, metaData).Size;
            this.csvMetadatas.Add(metaData);
            exportStatus.State = ExportState.Succeed;
            return exportStatus;
        }

        public ExportStatus ExportWeb(AvePoint.Wrapper.Backup.AveSPWeb aveWeb, VaultExportInfo info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Web(NARA).", aveWeb.SPWeb.Url.ToString());
            if (!HasInitObjs)
            {
                InitObjects(aveWeb.ParentSite);
            }

            return new ExportStatus();
        }

        public ExportStatus ExportList(AveSPList aveList, VaultExportInfo info)
        {
            mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export List(NARA).", aveList.ParentWeb.SPWeb.Url + aveList.Path);
            if (!HasInitObjs)
            {
                InitObjects(aveList.ParentSite);
            }
            ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };

            if (!(aveList.SPList != null && aveList.SPList.BaseType == AveBaseType.DocumentLibrary))
            {
                this.AveSPList = aveList;
            }
            columnArray.UpdateColumnInfo(aveList);
            itemCache = new List<string>();
            return new ExportStatus() { State = ExportState.Succeed };
        }

        public ExportStatus ExportFolder(AveSPFolder aveFolder, VaultExportInfo info, bool isRootFolder)
        {
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("NARAExport_ExportFolder"))
                {
                    ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                    if (aveFolder == null)
                    {
                        mLog.Warn(VaultLogFormat.LOG, LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException);
                        exportStatus.ErrorMessage = LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException;
                        return exportStatus;
                    }
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export folder(NARA).", FullURL.GetItemFullUrl(aveFolder, false));
                    if (isRootFolder)
                    {
                        return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = "Vault isn't export root folder, So export state is skipped." };
                    }
                    else
                    {
                        lock (lockObj)
                        {
                            if (!HasInitObjs)
                            {
                                InitObjects(aveFolder.ParentSite);
                            }
                            if (this.medataFileinfo == null)
                            {
                                this.medataFileinfo = new VaultExportInfo() { ContentFilePath = info.MetaDataFileName, FolderPath = info.MetaDataFilePath };
                            }
                        }
                        if (!(aveFolder.AveItem.SPListItem == null || aveFolder.AveItem.SPListItem.ContentType == null || aveFolder.AveItem.IsSystemFileOrFolder))
                        {
                            string filePath = info.FolderPath+"\\"+info.ContentFilePath;
                            CsvMetaData metaData = new CsvMetaData
                            {
                                CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(aveFolder, this.DisposalClass, filePath)
                            };
                            //exportStatus.ExportSize += RealVaultExport.ExportMetaDataFile(new ExportInfo(), info, metaData).Size;
                            this.csvMetadatas.Add(metaData);
                            exportStatus.State = ExportState.Succeed;
                        }
                        else
                        {
                            exportStatus.State = ExportState.Skipped;
                            exportStatus.ErrorMessage = LOGRESOURCEInternationalization.Compliance_Vault_SP2013_500bcbd0_6b04_4939_a26d_d74813c4b495;
                        }
                    }
                    return exportStatus;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export ItemOrItemVersion.It is Export Service Error.", FullURL.GetItemFullUrl(aveFolder), e1.ToString());
                throw;
            }
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export ItemOrItemVersion.", FullURL.GetItemFullUrl(aveFolder), e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }
        }

        public ExportStatus ExportItemOrItemVersion(AveSPListItem aveListItem, VaultExportInfo info)
        {

            return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = SKIPMESSAGE };
        }

        public ExportStatus ExportDocOrDocVersion(AveSPDoc aveDoc, VaultExportInfo info)
        {
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("NARAExport_ExportDocOrDocVersion"))
                {
                    ExportStatus exportStatus = new ExportStatus() { State = ExportState.Failed };
                    if (aveDoc == null)
                    {
                        mLog.Warn(VaultLogFormat.LOG, LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException);
                        exportStatus.ErrorMessage = LOGRESOURCE.Vault_SOVTVaultUtilityParameterNullException;
                        return exportStatus;
                    }
                    mLog.Info(VaultLogFormat.LOGWITHPATH, "Start Export Doc Or DocVersion(NARA).", aveDoc.AveSPItem.Id);
                    lock (lockObj)
                    {
                        if (!HasInitObjs)
                        {
                            InitObjects(aveDoc.ParentSite);
                        }
                        if (this.medataFileinfo == null)
                        {
                            this.medataFileinfo = new VaultExportInfo() { ContentFilePath = info.MetaDataFileName, FolderPath = info.MetaDataFilePath };
                        }
                    }
                    string hashString = string.Empty;
                    ExportResultInfo result = null;
                    using (Stream docStream = aveDoc.AveSPItem.GetContent())
                    {
                        string cachePath = string.Empty;
                        try
                        {
                            cachePath = CacheAndGetExportFilePath(info.ContentFilePath, docStream);
                            using (FileStream cacheStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                            {
                                hashString = GetHashStringFromFileContent(cacheStream);
                                ExportInfo contentInfo = new ExportInfo();
                                //keep datetime
                                contentInfo.Created = DateTime.Parse(aveDoc.AveSPItem.SPListItem["Created"].ToString()).ToLocalTime();
                                contentInfo.Modified = DateTime.Parse(aveDoc.AveSPItem.SPListItem["Modified"].ToString()).ToLocalTime();
                                //export content
                                result = RealVaultExport.ExportContent(contentInfo, info, cacheStream);
                                exportStatus.ExportSize += result.Size;
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"An error occurred while export DocOrDocVersion(NARA).error:{e}");
                            throw;
                        }
                        finally
                        {
                            if (File.Exists(cachePath))
                            {
                                File.Delete(cachePath);
                            }
                            else
                            {
                                mLog.Warn($"The cache file is not exist.filename:{cachePath}");
                            }
                        }
                    }
                    string filePath = Path.Combine(info.FolderPath, info.ContentFilePath);
                    columnArray.RevIMColumnName = info.Extension;
                    //export csv medata
                    CsvMetaData metaData = new CsvMetaData
                    {
                        CsvMetadataInfo = columnArray.GetCSVListFromColumnValue(aveDoc, this.DisposalClass, filePath, info.ContentFilePath, hashString)
                    };
                    //RealVaultExport.ExportMetaDataFile(new ExportInfo(), info, metaData);
                    if (result.Size != 0)
                    {
                        this.csvMetadatas.Add(metaData);
                    }
                    else
                    {
                        mLog.Warn("Export Doc Or DocVersion failed(NARA), because the content size is 0.");
                    }
                    exportStatus.State = ExportState.Succeed;
                    return exportStatus;
                }
            }
            catch (ExportServiceException e1)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "Vault_SOVTVaultExportObjectServiceExceportion", FullURL.GetItemFullUrl(aveDoc), e1.ToString());
                throw;
            }
            //catch (AveWrapperCheckoutFileException ex1) RECO-639
            //{
            //    mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion(NAA).It is Export Service Error.", FullURL.GetItemFullUrl(aveDoc), ex1.ToString());
            //    throw;
            //}
            catch (Exception e2)
            {
                mLog.Error(VaultLogFormat.LOGWITHEXCEPTIONPATH, "An error occurred while export DocOrDocVersion(NARA).", FullURL.GetItemFullUrl(aveDoc), e2.ToString());
                return new ExportStatus() { State = ExportState.Failed, ErrorMessage = e2.Message.ToString() };
            }
        }
        private string CacheAndGetExportFilePath(string fileName,Stream exportFileStream)
        {
            string cacheFolder = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, CACHEFOLDERNAME);
            if (!Directory.Exists(cacheFolder))
            {
                mLog.Info("Begin Create export file cache folder for export");
                Directory.CreateDirectory(cacheFolder);
            }
            string exportFilePath = SecurityUtils.SafeCombinePath(cacheFolder, fileName);
            WriteStreamToFile(exportFileStream, exportFilePath);
            return exportFilePath;
        }
        private void WriteStreamToFile(Stream stream, string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
        private string GetHashStringFromFileContent(Stream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            using SHA256 alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(fileStream);
            string hashString = BitConverter.ToString(hash).Replace("-", string.Empty);
            return hashString;
        }

        public ExportStatus ExportAttachment(AveSPAttachment aveAttachment, VaultExportInfo info)
        {
            return new ExportStatus() { State = ExportState.Skipped, ErrorMessage = SKIPMESSAGE };
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public void ExtensionMethod(params object[] parameter)
        {
            //to do rule test..
            //headline
            using (AvePerformanceScope pc = new AvePerformanceScope("NARAExport_ExtensionMethod"))
            {
                List<CsvMetaData> metadataObjs = parameter[0] as List<CsvMetaData>;
                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(new Byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                    bool WriteHead = false;
                    foreach (var metadata in metadataObjs)
                    {
                        if (!WriteHead)
                        {
                            string head = Generate(metadata, true);
                            byte[] headLine = Encoding.UTF8.GetBytes(head);
                            stream.Write(headLine, 0, headLine.Length);
                            WriteHead = true;
                        }
                        string content = Generate(metadata, false);
                        byte[] contentLine = Encoding.UTF8.GetBytes(content);
                        stream.Write(contentLine, 0, contentLine.Length);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    SaveSignature(stream, this.medataFileinfo);
                    RealVaultExport.ExportContent(new ExportInfo(), this.medataFileinfo, stream);
                }
            }
        }

        private void SaveSignature(Stream csvStream, VaultExportInfo medatainfo)
        {
            var setting = SettingProfileService.GetExportSignature();
            if (setting.EnableExportSignature)
            {
                mLog.Info("start signature for csv file");
                VaultExportInfo signatureFile = new VaultExportInfo();
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
                using (System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create())
                {
                    var par = JsonSerializer.Deserialize<RsaParametersSerializable>(rsaParam).ToRSAParameters();
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
                mLog.Info("this export job is no need signature");
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
        private string Generate(MetaData metaData, Boolean isHeaderLine)
        {
            Char Comma = '\u002C';

            /// <summary>
            /// Quote ASCII code is 34
            /// </summary>
            Char Quote = '\u0022';
            var csvMetadataInfo = new StringBuilder();
            var properties = metaData.CsvMetadataInfo;
            for (Int32 i = 0; i < properties.Count; i++)
            {
                var origainalStringValue = isHeaderLine ? properties[i].Name : properties[i].Value;
                origainalStringValue = string.IsNullOrEmpty(origainalStringValue) ? string.Empty : origainalStringValue;
                var stringValue = origainalStringValue.Contains("\"") ? origainalStringValue.Replace("\"", "\"\"") : origainalStringValue;
                if (i < properties.Count - 1)
                    csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", Quote, stringValue, Comma);
                else csvMetadataInfo.AppendFormat("{0}{1}{0}{2}", Quote, stringValue, Environment.NewLine);
            }
            return csvMetadataInfo.ToString();
        }
        public void Dispose()
        {
            try
            {
                this.csvMetadatas.Clear();
            }
            catch (Exception ex)
            {
                mLog.Warn("Dispose metadata error {0}", ex.ToString());
            }
            base.Dispose();
        }

        public List<CsvMetaData> GetCSVMetadata()
        {
            return this.csvMetadatas;
        }
    }
}
