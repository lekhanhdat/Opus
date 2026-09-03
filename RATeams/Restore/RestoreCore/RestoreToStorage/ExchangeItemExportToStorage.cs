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
using Aspose.Email;
using Aspose.Email.Calendar;
using Aspose.Email.Calendar.Recurrences;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Archiver;
using DocumentFormat.OpenXml.Bibliography;
using ExchangeCommonWrapper;
using Google.Api.Gax.ResourceNames;
using Job.ModernManagement.Report;
using Office365GroupBackup;
using Office365GroupRestore;
using RAArchiverCommon;
using Storage;
using System;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MailAddress = Aspose.Email.MailAddress;
using MailAddressCollection = Aspose.Email.MailAddressCollection;
using MailMessage = Aspose.Email.MailMessage;

namespace RATeams.Restore.RestoreCore.RestoreToStorage
{
    public class ExchangeItemExportToStorage
    {
        public String TempRestoreFolder => folderIndex == 1 ? tempRestoreFolder : tempRestoreFolder + "(" + folderIndex + ")";
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExchangeItemExportToStorage));

        private IKeyValueService keyValueService => PlatformWindsorManager.GetService<IKeyValueService>();

        int folderIndex = 1;
        int zipIndex = 1;
        long zipSizeLimit = 20l * 1024 * 1024 * 1024;
        long currentFolderSize = 0;
        string tempRestoreFolder;

        public long CurrentFolderSize => currentFolderSize;
        public ICacheService CacheManager { get; set; }
        private CacheSettingDto cacheSettingDto;

        RestoreConfig _config;
        private IXSystem _destinationPhysicalDevice;
        private IReportCenter _report;
        private string conversationFolderPath;
        private string calendarFolderPath;
        private string siteCollectionFolderPath;
        private string conversationChannelFolderPath;
        public bool exportHasError = false;
        private int currentConversationChildCount = 1;
        ReportDto currentReport;
        private int FileNameLenth = 199;
        private string _groupMailBoxAddress;
        BaseRestoreHelperBatch RestoreHelperBatch;

        private const int MAX_FILE_NAME_LENGTH = 240;

        private bool IsRestoreTeamsOutPlace;

        public void SetRestoreHelperBatch(BaseRestoreHelperBatch baseRestoreHelperBatch)
        {
            this.RestoreHelperBatch = baseRestoreHelperBatch;
        }

        public ExchangeItemExportToStorage(RestoreConfig config,IReportCenter report)
        {
            _config = config;
            _groupMailBoxAddress = GetGroupMailBoxAddress(config);
            _destinationPhysicalDevice = config.DestinationPhysicalDevice;
            _report = report;
            this.tempRestoreFolder =config.RestoreJobId.Substring(0, config.RestoreJobId.IndexOf("_"));
            CacheManager = new CacheService();
            cacheSettingDto = config.exchangeRestoreJob.CacheSetting;
            IsRestoreTeamsOutPlace = _config.JobType == (int)JobType.TeamsOutPlaceRestore;
            InitSetting();
        }

        private string GetGroupMailBoxAddress(RestoreConfig config)
        {
            try
            {
                return config.exchangeRestoreJob.ExchangeTreeRoot.EmailAddress;
            }
            catch(Exception e)
            {
                logger.Error($"Get Group mail box have error {e}");
                return string.Empty;
            }
        }

        private void InitSetting()
        {
            try
            {
                this.CacheManager.Open(cacheSettingDto, false);
                InitFolderLevel();
                SetZipSizeLimit();
            }
            catch (Exception e)
            {
                logger.Error($"something error when init restore setting,error:{e}");
            }
        }

        private void InitFolderLevel()
        {
            logger.Info($"Current mail box is {_groupMailBoxAddress}");
            var restoreTempInfo = new StorageInfo { HighName = this.TempRestoreFolder, LowName = String.Empty };
            conversationFolderPath = IsRestoreTeamsOutPlace ? SecurityUtils.SafeCombinePath(this.TempRestoreFolder, _groupMailBoxAddress, I18NEntity.GetString("RM_Archiver_JobDetailGroupMailboxLevel") , I18NEntity.GetString("RM_Archiver_JobDetailConversationLevel")) : SecurityUtils.SafeCombinePath(this.TempRestoreFolder, I18NEntity.GetString("RM_Archiver_JobDetailConversationLevel"));
            calendarFolderPath = IsRestoreTeamsOutPlace ? SecurityUtils.SafeCombinePath(this.TempRestoreFolder, _groupMailBoxAddress, I18NEntity.GetString("RM_Archiver_JobDetailGroupMailboxLevel"), I18NEntity.GetString("RM_Archiver_JobDetailCalendarLevel")) : SecurityUtils.SafeCombinePath(this.TempRestoreFolder, I18NEntity.GetString("RM_Archiver_JobDetailCalendarLevel"));
            var restoreCalendarInfo = new StorageInfo { HighName = calendarFolderPath, LowName = String.Empty };
            var restoreConversationInfo = new StorageInfo { HighName = conversationFolderPath, LowName = String.Empty };

            if (this.CacheManager.CacheSystem.DirectoryExists(restoreTempInfo))
            {
                this.CacheManager.CacheSystem.DeleteDirectory(restoreTempInfo);
            }

            this.CacheManager.CacheSystem.OpenDirectory(restoreTempInfo, FileMode.OpenOrCreate);
            this.CacheManager.CacheSystem.OpenDirectory(restoreConversationInfo, FileMode.OpenOrCreate);
            this.CacheManager.CacheSystem.OpenDirectory(restoreCalendarInfo, FileMode.OpenOrCreate);
        }

        private void SetZipSizeLimit()
        {
            long size = keyValueService.GetOOPRestoreJobZipSizeLimit();
            if (size > 0)
            {
                zipSizeLimit = size;
            }
        }

        public void RestoreToStorage(ExchangeDataBlockType type, IEnumerable<ExchangeDataBlockForBatch> exchangeDataBlockForBatch)
        {
            ReportDto report = new ReportDto();
            try
            {
                bool needAddReport = true;
                foreach (var exchangeDataBlock in exchangeDataBlockForBatch)
                {
                    report = new ReportDto
                    {
                        Status = ReportStatus.Success,
                        Option = RestoreOption.NewCreated.GetEnumDescription(),
                        EntityType = JobReportDetailEntityType.Objects,
                        Path ="",
                        SourcePath = "",
                    };
                    if (type == ExchangeDataBlockType.Item) continue;
                    if (type == ExchangeDataBlockType.Post)
                    {
                        string path = string.Empty;
                        try
                        {
                            needAddReport = false;
                            var internalConversationFolder = SecurityUtils.SafeCombinePath(conversationFolderPath, exchangeDataBlock.FileHeader.ParentName);
                            string parentName = RemoveNotAllowedChars(exchangeDataBlock.FileHeader.ParentName);
                            string folderName = SecurityUtils.SafeCombinePath(conversationFolderPath, parentName);
                            path = internalConversationFolder;
                            var con = new StorageInfo { HighName = folderName, LowName = String.Empty };
                            this.CacheManager.CacheSystem.OpenDirectory(con, FileMode.OpenOrCreate);
                            var metaList = exchangeDataBlock.RestoreData.MetadataLists;
                            long size = 0;
                            foreach (var meta in metaList)
                            {
                                if (meta.MetadataType == AvePoint.Metadata.AveMetadataType.ExchangeItem)
                                {
                                    string? metaJson = meta.GetMetadataObject().ToString();
                                    RMGraphMailReciver from = null;
                                    Dictionary<string, string> metaDataList = new Dictionary<string, string>();
                                    if (!string.IsNullOrEmpty(metaJson))
                                    {
                                        metaDataList = SerializerHelper.DeserializeByJsonConvert<Dictionary<string, string>>(metaJson);
                                        from = SerializerHelper.DeserializeByJsonConvert<RMGraphMailReciver>(metaDataList["From"]);
                                    }
                                    string bodyContent = ReadBodyContent(exchangeDataBlock);
                                    size = bodyContent.Length;
                                    ExportPostToEml(exchangeDataBlock.FileHeader.ParentName, from, bodyContent, this.CacheManager.CacheSystem.SystemLocation, folderName, metaDataList.ContainsKey("Importance")? metaDataList["Importance"]:string.Empty);
                                }
                            }
                            if (currentReport == null)
                            {
                                currentReport = new ReportDto
                                {
                                    Status = ReportStatus.Success,
                                    Option = RestoreOption.NewCreated.GetEnumDescription(),
                                    EntityType = JobReportDetailEntityType.Objects,
                                    Path = "",
                                    SourcePath = IsRestoreTeamsOutPlace && !string.IsNullOrEmpty(_groupMailBoxAddress) ? Path.Combine(_groupMailBoxAddress, exchangeDataBlock.FileHeader.ParentName) : internalConversationFolder,
                                    Type = ReportNodeHeader.Email,
                                    Name = exchangeDataBlock.FileHeader.ParentName,
                                    Size = size
                                };
                            }
                            if (currentConversationChildCount == exchangeDataBlock.FileHeader.ChildCount)
                            {
                                currentConversationChildCount = 1;
                                _report.AddRestoreReport(currentReport);
                                currentFolderSize += currentReport.Size;
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(currentReport.Size, currentReport.SourcePath);
                                currentReport = null;
                            }
                            else
                            {
                                currentReport.Size += size;
                                currentConversationChildCount++;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"restore failed when restore post:{path},error:{e}");
                            exportHasError = true;
                            _report.AddRestoreReport(new ReportDto
                            {
                                Status = ReportStatus.Failed,
                                Option = RestoreOption.NewCreated.GetEnumDescription(),
                                EntityType = JobReportDetailEntityType.Objects,
                                Path = IsRestoreTeamsOutPlace && !string.IsNullOrEmpty(_groupMailBoxAddress) ? Path.Combine(_groupMailBoxAddress, exchangeDataBlock.FileHeader.ParentName) : "",
                                SourcePath = path,
                                Type = ReportNodeHeader.Email,
                                Name = exchangeDataBlock.FileHeader.ParentName,
                                ErrorMessage= e.Message,

                            });
                        }

                    }
                    if (type == ExchangeDataBlockType.Event)
                    {
                        try
                        {
                            report.Type = ReportNodeHeader.Event;
                            var internalCalendarFolder = SecurityUtils.SafeCombinePath(calendarFolderPath, exchangeDataBlock.FileHeader.ParentName);
                            string parentName = RemoveNotAllowedChars(exchangeDataBlock.FileHeader.ParentName);
                            string folderName = SecurityUtils.SafeCombinePath(calendarFolderPath, parentName);
                            var con = new StorageInfo { HighName = folderName, LowName = String.Empty };
                            this.CacheManager.CacheSystem.OpenDirectory(con, FileMode.OpenOrCreate);
                            var metaList = exchangeDataBlock.RestoreData.MetadataLists;
                            GroupCalendarEvent calendarEvent = null;
                            foreach (var meta in metaList)
                            {
                                if (meta.MetadataType == AvePoint.Metadata.AveMetadataType.ExchangeCalendarEvent)
                                {
                                    string? metaJson = meta.GetMetadataObject().ToString();
     
                                    if (!string.IsNullOrEmpty(metaJson))
                                    {
                                        calendarEvent = SerializerHelper.DeserializeByJsonConvert<GroupCalendarEvent>(metaJson);
                                    }
                                    ExportEventToIcs(calendarEvent,this.CacheManager.CacheSystem.SystemLocation, folderName);
                                }
                            }
                            report.Name = calendarEvent?.Subject;
                            report.Title = calendarEvent?.Subject;
                            report.Size = calendarEvent.Body.Content.Length;
                            currentFolderSize += report.Size;
                            report.SourcePath = IsRestoreTeamsOutPlace && !string.IsNullOrEmpty(_groupMailBoxAddress) ? SecurityUtils.SafeCombinePath(_groupMailBoxAddress, exchangeDataBlock.FileHeader.ParentName, calendarEvent?.Subject) : SecurityUtils.SafeCombinePath(internalCalendarFolder, calendarEvent?.Subject);
                            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(report.Size, report.SourcePath);
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }
                    if (type == ExchangeDataBlockType.Attachment)
                    {
                        report.Type = ReportNodeHeader.Attachment;
                        var internalConversationFolder = SecurityUtils.SafeCombinePath(conversationFolderPath, exchangeDataBlock.FileHeader.ParentName);
                        var tempFile = new StorageInfo { HighName = internalConversationFolder, LowName = exchangeDataBlock.FileHeader.Name + "temp" };
                        var attachment = new StorageInfo { HighName = internalConversationFolder, LowName = exchangeDataBlock.FileHeader.Name };
                        try
                        {
                            byte[] buffer = new byte[64 * 1024];
                            using (XStream stream = this.CacheManager.CacheSystem.OpenStream(tempFile, FileMode.OpenOrCreate))
                            {
                                while (true)
                                {
                                    int len = exchangeDataBlock.RestoreData.ContentStream.Read(buffer, 0, buffer.Length);
                                    if (len <= 0) break;
                                    stream.Write(buffer, 0, len);
                                }
                                stream.Flush();
                            }
                            long size = 0;
                            using (var base64Stream = File.OpenRead(SecurityUtils.SafeCombinePath(this.CacheManager.CacheSystem.SystemLocation, internalConversationFolder, exchangeDataBlock.FileHeader.Name + "temp")))
                            {
                                using (var fileStream = this.CacheManager.CacheSystem.OpenStream(attachment, FileMode.OpenOrCreate))
                                {
                                    var transform = new FromBase64Transform();
                                    byte[] tempBuffer = new byte[4096];
                                    int bytesRead;

                                    while ((bytesRead = base64Stream.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
                                    {
                                        byte[] decoded = transform.TransformFinalBlock(tempBuffer, 0, bytesRead);
                                        fileStream.Write(decoded, 0, decoded.Length);
                                    }
                                    size = fileStream.Length;
                                    fileStream.Flush();
                                }
                            }
                            report.Name = exchangeDataBlock.FileHeader.Name;
                            report.Title = exchangeDataBlock.FileHeader.Name;
                            report.Size = size;
                            currentFolderSize += report.Size;
                            report.SourcePath = IsRestoreTeamsOutPlace && !string.IsNullOrEmpty(_groupMailBoxAddress) ? SecurityUtils.SafeCombinePath(_groupMailBoxAddress, exchangeDataBlock.FileHeader.ParentName, exchangeDataBlock.FileHeader.Name) : SecurityUtils.SafeCombinePath(internalConversationFolder, exchangeDataBlock.FileHeader.Name);
                            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(report.Size, report.SourcePath);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"something error when restore attach to storage,error:{e}");
                            throw;
                        }
                        finally
                        {
                            try
                            {
                                this.CacheManager.CacheSystem.DeleteFile(tempFile);
                            }
                            catch (Exception e)
                            {
                                logger.Error($"something error when delete temp file,error:{e}");
                            }
                        }

                    }
                    if(type == ExchangeDataBlockType.SiteCollection || type == ExchangeDataBlockType.Web || type == ExchangeDataBlockType.SiteList || type == ExchangeDataBlockType.SiteFolder)
                    {
                        report.Type = type switch
                        {
                            ExchangeDataBlockType.SiteCollection => ReportNodeHeader.SiteCollection,
                            ExchangeDataBlockType.Web => ReportNodeHeader.Web,
                            ExchangeDataBlockType.SiteList => ReportNodeHeader.List,
                            ExchangeDataBlockType.SiteFolder => ReportNodeHeader.SiteFolder,
                            _ => ' '
                        };
                        if (string.IsNullOrEmpty(siteCollectionFolderPath))
                        {
                            siteCollectionFolderPath = SecurityUtils.SafeCombinePath(this.TempRestoreFolder, _groupMailBoxAddress, I18NEntity.GetString("RM_Archiver_JobDetailSiteCollectionLevel"));
                            var restoreSiteCollectionInfo = new StorageInfo { HighName = siteCollectionFolderPath, LowName = String.Empty };
                            if (!this.CacheManager.CacheSystem.DirectoryExists(restoreSiteCollectionInfo))
                                this.CacheManager.CacheSystem.OpenDirectory(restoreSiteCollectionInfo, FileMode.OpenOrCreate);
                        }
                        report.SourcePath = exchangeDataBlock.FileHeader.Path;
                        report.Name = exchangeDataBlock.FileHeader.Name;
                        report.Title = exchangeDataBlock.FileHeader.Name;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(0, report.SourcePath);
                    }

                    if (type == ExchangeDataBlockType.SiteAttachmentItem || type == ExchangeDataBlockType.SiteDocumentItem || type == ExchangeDataBlockType.SiteVersionItem)
                    {
                        report.Type = type switch
                        {
                            ExchangeDataBlockType.SiteDocumentItem => ReportNodeHeader.Document,
                            ExchangeDataBlockType.SiteVersionItem => ReportNodeHeader.DocumentVersion,
                            ExchangeDataBlockType.SiteAttachmentItem => ReportNodeHeader.SiteAttachment,
                            _ => ' '
                        };
                        if (string.IsNullOrEmpty(siteCollectionFolderPath))
                        {
                            siteCollectionFolderPath = SecurityUtils.SafeCombinePath(this.TempRestoreFolder, _groupMailBoxAddress, I18NEntity.GetString("RM_Archiver_JobDetailSiteCollectionLevel"));
                            var restoreSiteCollectionInfo = new StorageInfo { HighName = siteCollectionFolderPath, LowName = String.Empty };
                            if(!this.CacheManager.CacheSystem.DirectoryExists(restoreSiteCollectionInfo))
                                this.CacheManager.CacheSystem.OpenDirectory(restoreSiteCollectionInfo, FileMode.OpenOrCreate);
                        }
                        string parentFolder = SecurityUtils.SafeCombinePath(siteCollectionFolderPath, exchangeDataBlock.FileHeader.ParentFullPath);
                        var storageInfo = GetItemStorageInfo(parentFolder, exchangeDataBlock.FileHeader.Name, type, exchangeDataBlock.FileHeader.ItemName);
                        long size = 0;
                        byte[] buffer = new byte[64 * 1024];
                        using (XStream stream = this.CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.OpenOrCreate))
                        {
                            while (true)
                            {
                                int len = exchangeDataBlock.RestoreData.ContentStream.Read(buffer, 0, buffer.Length);
                                if (len <= 0) break;
                                stream.Write(buffer, 0, len);
                                size += len;
                            }
                            stream.Flush();
                        }

                        report.Name = exchangeDataBlock.FileHeader.Name;
                        report.Title = exchangeDataBlock.FileHeader.Name;
                        report.Size = size;
                        currentFolderSize += report.Size;
                        report.SourcePath = exchangeDataBlock.FileHeader.Path;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(report.Size, report.SourcePath);
                    }

                    if (needAddReport)
                    {
                        _report.AddRestoreReport(report);
                    }
                }

                if (type == ExchangeDataBlockType.Item)
                {
                    report = new ReportDto
                    {
                        Status = ReportStatus.Success,
                        Option = RestoreOption.NewCreated.GetEnumDescription(),
                        EntityType = JobReportDetailEntityType.Objects,
                        Path = "",
                        SourcePath = "",
                        Type = ReportNodeHeader.Conversation
                    };

                    if (string.IsNullOrEmpty(conversationChannelFolderPath))
                    {
                        conversationChannelFolderPath = SecurityUtils.SafeCombinePath(this.TempRestoreFolder, _groupMailBoxAddress, I18NEntity.GetString("RM_Archiver_JobDetailConversationChannelLevel"));
                        var restoreSiteCollectionInfo = new StorageInfo { HighName = conversationChannelFolderPath, LowName = String.Empty };
                        if (!this.CacheManager.CacheSystem.DirectoryExists(restoreSiteCollectionInfo))
                            this.CacheManager.CacheSystem.OpenDirectory(restoreSiteCollectionInfo, FileMode.OpenOrCreate);
                    }

                    long size = 0;
                    var data = exchangeDataBlockForBatch.First().RestoreData;
                    var entity = data.Metadata;
                    var dataSource = entity.Type == "IPM.SkypeTeams.Message" ? DataSource.EWS : DataSource.Graph;
                    RestoreConversationAsHtml restoreConversation = dataSource == DataSource.Graph ? new RestoreConversationFromGraphAsHtml(RestoreHelperBatch) : new RestoreConversationFromEwsAsHtml(RestoreHelperBatch);
                    var (channelName, folderName, fileName) = restoreConversation.GenerateFileInfo(entity);
                    if (string.IsNullOrEmpty(channelName))
                    {
                        var splitPath = entity.DisplayPath.Trim('/').Trim('\\').Split('\\');
                        channelName = splitPath[^2];
                        fileName = channelName + fileName;
                    }
                    report.SourcePath = entity.DisplayPath;
                    //var storageInfo = new StorageInfo { HighName = Path.Combine(conversationChannelFolderPath, channelName), LowName = fileName };
                    var storageInfo = GetItemStorageInfo(SecurityUtils.SafeCombinePath(conversationChannelFolderPath, channelName), fileName, type, fileName);
                    using (var content = restoreConversation.GenerateConversationHtml(exchangeDataBlockForBatch))
                    {
                        using (XStream stream = this.CacheManager.CacheSystem.OpenStream(storageInfo, FileMode.OpenOrCreate))
                        {
                            byte[] buffer = new byte[64 * 1024];
                            while (true)
                            {
                                int len = content.Read(buffer, 0, buffer.Length);
                                if (len <= 0) break;
                                stream.Write(buffer, 0, len);
                                size += len;
                            }
                            stream.Flush();
                        }
                    }
                    report.Size = size;
                    currentFolderSize += size;
                    if (needAddReport)
                    {
                        _report.AddRestoreReport(report);
                    }

                }

                if (currentFolderSize > zipSizeLimit)
                {
                    UploadZipFileToStorage();
                }
            }
            catch (Exception e)
            {
                exportHasError = true;
                report.Status = ReportStatus.Failed;
                report.ErrorMessage = e.Message;
                _report.AddRestoreReport(report);
                logger.Error($"something error when restore to storage,error:{e}");
            }
        }

        private StorageInfo GetItemStorageInfo(string parentFolder, string name, ExchangeDataBlockType type, string itemName)
        {
            int colonPosition = name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (type == ExchangeDataBlockType.SiteAttachmentItem)
            {
                string folderName = $"{name.Substring(0, colonPosition)}Attachment\\";
                parentFolder = Path.Combine(parentFolder, folderName);
                name = name.Substring(colonPosition + 1);
            }
            else
            {
                string fileName = itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) >= 0 ? itemName.Remove(itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)) : itemName;
                string extension = itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) >= 0 ? itemName.Substring(itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)) : string.Empty;
                string newFileName = colonPosition > 0 ? fileName + '_' + name.Substring(colonPosition + 1) + extension : name;
                name = newFileName;
            }
            logger.Info($"Item path: {parentFolder}, name {name} and type: {type}");
            parentFolder = parentFolder.Replace("\\", "/");
            var info = XConvert.FromNames(ReplaceInvalidChar(parentFolder, false), ReplaceInvalidChar(TruncateFileName(name, colonPosition > 0), true));
            return info;
        }

        String ReplaceInvalidChar(String srcStr, bool isFile)
        {
            Char[] invalidCS = Path.GetInvalidFileNameChars();
            foreach (char c in invalidCS)
            {
                srcStr = !(!isFile && (c == Path.DirectorySeparatorChar || c == '/')) ? srcStr.Replace(c, '_') : srcStr;
            }
            return srcStr;
        }

        private string TruncateFileName(string fileName, bool hasFileVersion = false)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            if (fileName.Length <= MAX_FILE_NAME_LENGTH) return fileName;
            string extension = Path.GetExtension(fileName);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            int allowed = MAX_FILE_NAME_LENGTH - (extension?.Length ?? 0);
            string version = string.Empty;
            if (hasFileVersion)
            {
                version = nameWithoutExt.Substring(nameWithoutExt.LastIndexOf('_'));
                allowed -= version.Length;
            }
            if (allowed <= 0)
            {
                return fileName.Substring(0, MAX_FILE_NAME_LENGTH);
            }
            if (nameWithoutExt.Length > allowed)
            {
                nameWithoutExt = nameWithoutExt.Substring(0, allowed);
            }
            return nameWithoutExt + "..." + version + extension;
        }

        public void UploadZipFileToStorage()
        {
            string zipFilePath = string.Empty;
            try
            {
                string zipFileName = zipIndex == 1 ? this.TempRestoreFolder + ".zip" : this.TempRestoreFolder + $"({zipIndex}).zip";
                StorageInfo tempStorageInfo = new StorageInfo { HighName = string.Empty, LowName = zipFileName };
                zipFilePath = SecurityUtils.SafeCombinePath(this.CacheManager.CacheSystem.SystemLocation, zipFileName);
                if (IsRestoreTeamsOutPlace)
                {
                    try
                    {
                        DeepPathZipService deepPathZipService = new DeepPathZipService();
                        deepPathZipService.Zip(SecurityUtils.SafeCombinePath(this.CacheManager.CacheSystem.SystemLocation, this.TempRestoreFolder), zipFilePath, this._config.ZipFilePassword, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"zip the directory {zipFilePath} failed. {e.ToString()}");
                        ZipFolder(zipFilePath);
                    }
                }
                else
                {
                    ZipFolder(zipFilePath);
                }
                var lenth = this.CacheManager.CacheSystem.OpenFile(tempStorageInfo).FileSize;
                logger.Info("Restore job summary total size is:{0}", lenth);
                tempStorageInfo.Length = lenth;
                logger.Info("The restore zip file size is:{0}", lenth);
                using (XStream cacheStream = this.CacheManager.CacheSystem.OpenStream(tempStorageInfo, FileMode.Open))
                {
                    _config.DestinationPhysicalDevice.CommitStream(cacheStream, tempStorageInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to upload zip file. Reason: {0}", ex.ToString());
                throw;
            }
            finally
            {
                InitFolderLevel();
                FileUtility.ForceDelete(zipFilePath);
                currentFolderSize = 0;
                zipIndex++;
            }
        }

        private void ZipFolder(string zipFilePath)
        {
            try
            {
                ZipUtil.ZipFolder(SecurityUtils.SafeCombinePath(this.CacheManager.CacheSystem.SystemLocation, this.TempRestoreFolder), zipFilePath, this._config.ZipFilePassword, Encoding.UTF8);
                //ZipUtil.ZipFolder(restoreFolderPath, zipFilePath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                logger.Warn($"zip the directory {zipFilePath} failed, maybe the path is too long, try to zip with alphaFS. {e.ToString()}");
                ZipUtil.ZipFolderForLongPath(SecurityUtils.SafeCombinePath(this.CacheManager.CacheSystem.SystemLocation, this.TempRestoreFolder), zipFilePath, this._config.ZipFilePassword, Encoding.UTF8);
            }
        }

        public void ExportEventToIcs(GroupCalendarEvent graphEventData, string icsFilePath,string folder)
        {
            // 1. 解析 Graph API 返回的事件数据
            string subject = graphEventData.Subject ?? "未命名事件";
            string location = graphEventData.Location?.DisplayName ?? string.Empty;
            string description = StripHtmlTags(graphEventData.Body?.Content ?? string.Empty);

            // 2. 处理时间（需考虑时区）
            DateTime startTime = DateTime.Parse(graphEventData.Start.DateTime);
            DateTime endTime = DateTime.Parse(graphEventData.End.DateTime);
            string timeZone = graphEventData.Start.TimeZone ?? "UTC";

            // 3. 创建日历事件
            var appointment = new Appointment(location, subject, description, startTime, endTime, new Aspose.Email.MailAddress(graphEventData.Organizer.MailAddress.Address, graphEventData.Organizer.MailAddress.Name), new MailAddressCollection())
            {
                StartTimeZone = timeZone,
                EndTimeZone = timeZone,
            };
            // appointment.IsDescriptionHtml = true;
            // 4. 添加参与者
            if (graphEventData.Attendees != null)
            {
                foreach (var attendee in graphEventData.Attendees)
                {
                    appointment.Attendees.Add(new MailAddress(
                        attendee.MailAddress.Address,
                        attendee.MailAddress.Name
                    ));
                }
            }
            try
            {
                appointment.MicrosoftBusyStatus = (MSBusyStatus)graphEventData.LegacyFreeBusyStatus;
                if (graphEventData.Recurrence.Pattern.Type == "weekly")
                {
                    WeeklyRecurrencePattern weeklyRecurrencePattern = new WeeklyRecurrencePattern(GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone), graphEventData.Recurrence.Pattern.Interval);
                    weeklyRecurrencePattern.StartDays = new CalendarDay[graphEventData.Recurrence.Pattern.DaysOfWeek.Length];
                    int i = 0;
                    foreach (var day in graphEventData.Recurrence.Pattern.DaysOfWeek)
                    {
                        weeklyRecurrencePattern.StartDays[i] = ((CalendarDay)Enum.Parse(typeof(CalendarDay), day.First().ToString().ToUpper() + string.Join("", day.Skip(1)), true));
                        i++;
                    }
                    appointment.Recurrence = weeklyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type.Contains("Yearly"))
                {
                    YearlyRecurrencePattern yearlyRecurrencePattern = new YearlyRecurrencePattern();
                    yearlyRecurrencePattern.EndDate = GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone);
                    yearlyRecurrencePattern.Interval = graphEventData.Recurrence.Pattern.Interval;
                    yearlyRecurrencePattern.StartPosition = (DayPosition)Enum.Parse(typeof(DayPosition), graphEventData.Recurrence.Pattern.Index.First().ToString().ToUpper() + string.Join("", graphEventData.Recurrence.Pattern.Index.Skip(1)));
                    yearlyRecurrencePattern.StartDay = (CalendarDay)Enum.Parse(typeof(DayPosition), GetDateTimeByString(graphEventData.Recurrence.Range.StartDate, graphEventData.Recurrence.Range.RecurrenceTimeZone).DayOfWeek.ToString());
                    yearlyRecurrencePattern.StartOffset = graphEventData.Recurrence.Pattern.DayOfMonth;
                    yearlyRecurrencePattern.StartMonth = (CalendarMonth)graphEventData.Recurrence.Pattern.Month;
                    appointment.Recurrence = yearlyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type.Contains("Monthly"))
                {
                    MonthlyRecurrencePattern monthlyRecurrencePattern = new MonthlyRecurrencePattern();
                    monthlyRecurrencePattern.EndDate = GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone);
                    monthlyRecurrencePattern.Interval = graphEventData.Recurrence.Pattern.Interval;
                    monthlyRecurrencePattern.StartPosition = (DayPosition)Enum.Parse(typeof(DayPosition), graphEventData.Recurrence.Pattern.Index.First().ToString().ToUpper() + string.Join("", graphEventData.Recurrence.Pattern.Index.Skip(1)));
                    monthlyRecurrencePattern.StartDay = (CalendarDay)Enum.Parse(typeof(DayPosition), GetDateTimeByString(graphEventData.Recurrence.Range.StartDate, graphEventData.Recurrence.Range.RecurrenceTimeZone).DayOfWeek.ToString());
                    monthlyRecurrencePattern.StartOffset = graphEventData.Recurrence.Pattern.DayOfMonth;
                    appointment.Recurrence = monthlyRecurrencePattern;
                }
                if (graphEventData.Recurrence.Pattern.Type == "daily")
                {
                    DailyRecurrencePattern dailyRecurrencePattern = new DailyRecurrencePattern(GetDateTimeByString(graphEventData.Recurrence.Range.EndDate, graphEventData.Recurrence.Range.RecurrenceTimeZone), graphEventData.Recurrence.Pattern.Interval);
                    appointment.Recurrence = dailyRecurrencePattern;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to set MicrosoftBusyStatus for event {subject}. Error: {e.Message}");
            }
            //// 5. 处理提醒（如果存在）
            //if (graphEventData.ReminderMinutesBeforeStart != null)
            //{
            //    appointment= graphEventData.ReminderMinutesBeforeStart;
            //}
            string fileName = subject;
            if (fileName.Length > FileNameLenth)
            {
                fileName = fileName.Substring(0, FileNameLenth);
            }
            fileName = RemoveNotAllowedChars(fileName);
            icsFilePath = SecurityUtils.SafeCombinePath(icsFilePath, folder, fileName);
            // 6. 保存为 ICS 文件
            appointment.Save(icsFilePath + "_" + DateTime.UtcNow.Ticks + ".ics", AppointmentSaveFormat.Ics);
        }
        private DateTime GetDateTimeByString(string endDateStr,string timeZoneStr)
        {
            DateTime endDateUtc = DateTime.ParseExact(endDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneStr);

            DateTime endDateInTimeZone = DateTime.SpecifyKind(endDateUtc, DateTimeKind.Unspecified);
            DateTime endDateWithTimeZone = TimeZoneInfo.ConvertTimeToUtc(endDateInTimeZone, timeZone);
            return endDateWithTimeZone;
        }
        private string RemoveNotAllowedChars(string input)
        {
            return Regex.Replace(input, @"[\\/:*?""<>|]", "");
        }
        private string StripHtmlTags(string html)
        {
            string noTagNewlines = Regex.Replace(html, @"<[^>]*[\r\n\t]*[^>]*>", " ");
            noTagNewlines = Regex.Replace(noTagNewlines, @"[ ]+", " ");
            return noTagNewlines.Trim();
        }
        public void ExportPostToEml(string topic, RMGraphMailReciver from, string body, string outputPath,string folder,string priority)
        {
            // 创建 MailMessage 对象
            var message = new MailMessage
            {
                From = new MailAddress(from?.MailAddress.Address, from?.MailAddress.Name),
                Subject = topic,
                Body = body,
                HtmlBody = body,
            };
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            // 添加收件人
            string mailName = RestoreConfig.CurrentMailboxAddress.Split('@')[0];
            message.To.Add(new MailAddress(RestoreConfig.CurrentMailboxAddress, mailName));
            if (priority == Aspose.Email.MailPriority.High.ToString())
            {
                message.Priority = Aspose.Email.MailPriority.High;
            }
            else if (priority == Aspose.Email.MailPriority.Low.ToString())
            {
                message.Priority = Aspose.Email.MailPriority.Low;
            }
            else
            {
                message.Priority = Aspose.Email.MailPriority.Normal;
            }
            //// 添加抄送（如果有）
            //if (graphPostData.ccRecipients != null)
            //{
            //    foreach (var ccRecipient in graphPostData.ccRecipients)
            //    {
            //        message.CC.Add(new MailAddress(ccRecipient.emailAddress.address, ccRecipient.emailAddress.name));
            //    }
            //}
            string fileName = topic;
            if (fileName.Length> FileNameLenth)
            {
                fileName = fileName.Substring(0, FileNameLenth);
            }
            fileName = RemoveNotAllowedChars(fileName);
            outputPath = SecurityUtils.SafeCombinePath(outputPath, folder, fileName);
            var options = new EmlSaveOptions(MailMessageSaveType.EmlFormat);
            message.IsBodyHtml = true;
            // 保存为 EML 文件
            message.Save(outputPath + "_" + DateTime.UtcNow.Ticks + ".eml", options);
        }
        private string ReadBodyContent(ExchangeDataBlockForBatch data)
        {
            using (var reader = new StreamReader(data.RestoreData.ContentStream))
            {
                string line;
                string result = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    result += line;
                }
                RMGraphMailBody tempResult = SerializerHelper.DeserializeByJsonConvert<RMGraphMailBody>(result);
                return tempResult.Content;
            }
        }
        //private void CreateFolderOrFile(ArchiverBasicIndex index, String dirPath)
        //{
        //    string fileName = index.Name;
        //    int colonPosition = fileName.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
        //    AveSharePointType aveType = (AveSharePointType)index.Type[0];
        //    if (aveType == AveSharePointType.TYPE_FOLDER || aveType == AveSharePointType.TYPE_LIST ||
        //        aveType == AveSharePointType.TYPE_SITE || aveType == AveSharePointType.TYPE_WEB)
        //    {
        //        logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileOpen, dirPath);
        //        StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), string.Empty);
        //        if (!this.CacheManager.CacheSystem.DirectoryExists(info))
        //        {
        //            this.CacheManager.CacheSystem.OpenDirectory(info, FileMode.OpenOrCreate);
        //            this.itemDetailMessage.Status = 0;
        //        }
        //        else
        //        {
        //            this.itemDetailMessage.Status = 10;//if status is 10,not report
        //        }
        //    }
        //    else if (aveType == AveSharePointType.TYPE_ATTACHMENTS)
        //    {
        //        this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileOpen, dirPath);
        //        // create new folder for Attachment
        //        StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), string.Empty);
        //        this.CacheManager.CacheSystem.OpenDirectory(info, FileMode.OpenOrCreate);
        //        fileName = fileName.Substring(colonPosition + 1);
        //        info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), ReplaceInvalidChar(fileName, true));
        //        WriteDataToFile(info, index);
        //    }
        //    else if (aveType == AveSharePointType.TYPE_DOCUMENT || aveType == AveSharePointType.TYPE_VERSION)
        //    {
        //        //documment类型含version的处理:(举例说明a.txt:1.0 ---> a_1.0.txt)
        //        var name = index.ItemName.LastIndexOfIgnoreCase(".") >= 0 ? index.ItemName.Remove(index.ItemName.LastIndexOfIgnoreCase(".")) : index.ItemName;
        //        var extension = index.ItemName.LastIndexOfIgnoreCase(".") >= 0 ? index.ItemName.Substring(index.ItemName.LastIndexOfIgnoreCase(".")) : null;
        //        var newFileName = colonPosition > 0 ? name + '_' + index.Name.Substring(colonPosition + 1) + extension : index.Name;
        //        StorageInfo info = XConvert.FromNames(ReplaceInvalidChar(dirPath, false), ReplaceInvalidChar(newFileName, true));
        //        WriteDataToFile(info, index);
        //    }
        //    else
        //    {
        //        logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceCreateFolderOrFileNotWrite, fileName, aveType);
        //        this.itemDetailMessage.Status = 2;
        //    }
        //}
    }
}
