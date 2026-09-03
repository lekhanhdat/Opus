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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Text.RegularExpressions;

namespace RAGoogle.RecordsDisposal.Action.ExportOnly
{
    public class ExportFileDownloadManagement
    {
        private GoogleConfiguration _configuration;
        private GoogleDriveService _driveService;
        private string _defaultDownloadPath;

        private List<string> filePaths = [];

        private IRALogger _logger = RALogger.GetInstance(typeof(ExportFileDownloadManagement));

        private const string GoogleExportCacheFolder = "GoogleExportCache";


        public ExportFileDownloadManagement(GoogleConfiguration configuration, GoogleDriveService driveService)
        {
            _configuration = configuration;
            _defaultDownloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GoogleExportCacheFolder);
            _driveService = driveService;
            Directory.CreateDirectory(_defaultDownloadPath);
        }

        public List<string> GetFilePaths()
        {
            return filePaths;
        }

        public async Task<List<DownloadedFileInfo>> DownloadFileWithVersionsAsync(GoogleItemData item)
        {
            if (item == null)
            {
                _logger.Error("GoogleItemData is null.");
                throw new ArgumentNullException(nameof(item));
            }

            string fileFolder = Path.Combine(_defaultDownloadPath);
            Directory.CreateDirectory(fileFolder);

            var downloadedFiles = new List<DownloadedFileInfo>();
            var orderedRevisions = item.Versions?.OrderByDescending(r => r.ModifiedTimeDateTimeOffset ?? DateTimeOffset.MinValue).ToList();

            try
            {
                if (orderedRevisions == null || !orderedRevisions.Any())
                {
                    return await DownloadCurrentFile(item, fileFolder, downloadedFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while processing file {item.Name}: {ex.Message}");
                throw;
            }

            return await DownloadFileVersions(item, fileFolder, orderedRevisions, downloadedFiles);
        }

        private async Task<List<DownloadedFileInfo>> DownloadFileVersions(GoogleItemData item, string fileFolder, List<Revision> orderedRevisions, List<DownloadedFileInfo> downloadedFiles)
        {
            int versionNumber = orderedRevisions.Count;
            int totalVersionNumber = orderedRevisions.Count;

            foreach (var revision in orderedRevisions)
            {
                var isCurrentVersion = revision == orderedRevisions.FirstOrDefault();
                string fileName = HandleFileNameWithSpecialCharacter(item.Name);

                string fileExtension = GoogleFileExtension.GetFileExtentionFromMimeType(revision.MimeType);

                if (string.IsNullOrEmpty(fileExtension))
                {
                    _logger.Info($"The MIME type {revision.MimeType} is not found in the mimeTypeToExtension.");
                    fileExtension = Path.GetExtension(revision.OriginalFilename);
                }


                string versionLabel = totalVersionNumber == 1 ? "" : $"{versionNumber}.0";
                string formattedFileName = totalVersionNumber == 1 ? $"{Path.GetFileNameWithoutExtension(fileName)}" : $"{Path.GetFileNameWithoutExtension(fileName)}-v{versionLabel}";
                string formattedFileNameForDownload = $"{Guid.NewGuid()}";
                string localFilePath = Path.Combine(fileFolder, formattedFileNameForDownload);

                filePaths.Add(localFilePath);

                _logger.Info($"Downloading {versionLabel} for file {item.Id} with size {revision.Size} to {localFilePath}...");

                try
                {
                    await DownloadRevisionAsync(item, revision.Id, localFilePath, item.Size.GetValueOrDefault());
                    _logger.Info($"Successfully downloaded {versionLabel} for file {item.Id}.");

                    var revisionSize = revision.Size ?? item.Size;

                    AddDownloadedFileInfo(downloadedFiles, item, localFilePath, formattedFileName, versionLabel, revisionSize, fileExtension, isCurrentVersion, revision);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to download {versionLabel} for file {item.Id}. Error: {ex.Message}");
                    throw;
                }

                versionNumber--;
            }

            return downloadedFiles;
        }

        private async Task<List<DownloadedFileInfo>> DownloadCurrentFile(GoogleItemData item, string fileFolder, List<DownloadedFileInfo> downloadedFiles)
        {
            string localFilePath = Path.Combine(fileFolder, item.Name);
            _logger.Info($"Downloading file (no versions) for {item.Id} to {localFilePath}...");

            try
            {
                if (GoogleConstant.GoogleExportMimeType.TryGetValue(item.MimeType, out string mimeType))
                {
                    string fileExtension = GoogleFileExtension.GetFileExtentionFromMimeType(item.MimeType);
                    var formattedFileName = item.Name;
                    var formattedFileNameForDownload = $"{Guid.NewGuid()}";
                    localFilePath = Path.Combine(fileFolder, formattedFileNameForDownload);
                    filePaths.Add(localFilePath);
                    _logger.Info($"Exported Google file {item.Id} with size {item.Size} to {localFilePath}.");
                    item.Size = item.Size ?? 0; //  google app script file size is null, set it to 0
                    if (GoogleConstant.GoogleVideoMimeType.Contains(item.MimeType))
                    {
                        _logger.Info($"Download media name {item.Id} with size {item.Size}. Local temp path: {localFilePath}");

                        await _driveService.DownloadMediaAsync(item.Id, mimeType, localFilePath);

                        _logger.Info("Download media '{0}' successfully. Local temp path: {1}", item.Name, localFilePath);
                    }
                    else if (item.Size < GoogleConstant.DRIVE_FILE_SIZE_10MB) // file < 10MB
                    {
                        await _driveService.ExportFileAsync(item.Id, localFilePath, mimeType);
                    }
                    else
                    {
                        await _driveService.ExportBigFileAsync(item.Id, localFilePath, mimeType);
                    }

                    AddDownloadedFileInfo(downloadedFiles, item, localFilePath, formattedFileName, "", item.Size, fileExtension, true, null);
                    return downloadedFiles;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to download file {item.Id}. Error: {ex.Message}");
                throw;
            }
            return downloadedFiles;
        }

        private async Task DownloadRevisionAsync(GoogleItemData file, string revisionId, string localFilePath, long fileSize)
        {
            try
            {
                await _driveService.DownloadFileVersionToStreamAsync(file, revisionId, localFilePath, fileSize);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error downloading revision {revisionId} of file id: {file.Id}, file name: {file.Name}: {ex.Message}");
                throw;
            }
        }

        private void AddDownloadedFileInfo(List<DownloadedFileInfo> downloadedFiles, GoogleItemData item, string localFilePath, string formattedFileVersionName,
                                           string versionName, long? fileSize, string downloadFileExtension, bool isCurrent , Revision revision)
        {

            var modifiedTime = revision == null ? null : revision.ModifiedTimeDateTimeOffset;
            var OriginalFilename = revision == null ? string.Empty : revision.OriginalFilename;
            var mimeType = revision == null ? item.MimeType : revision.MimeType;
            downloadedFiles.Add(new DownloadedFileInfo
            {
                Id = item.Id,
                FormattedFileVersionName = formattedFileVersionName,
                FileName = HandleFileNameWithSpecialCharacter(item.Name),
                LocalPath = localFilePath,
                VersionName = versionName,
                IsCurrentVersion = isCurrent,
                ModifiedTime = modifiedTime?.UtcDateTime ?? item.ModifiedTime,
                DriveName = item.DriveName,
                ParentId = item.ParentId,
                ParentIds = item.ParentIds,
                FileExtension = item.FileExtension,
                DownloadFileExtension = downloadFileExtension,
                MimeType = mimeType,
                Size = fileSize,
                Labels = item.LableIds ?? new List<string>(),
                Path = item.RelativePath,
                CreatedBy = item.CreatedBy,
                CreatedTime = item.CreatedTime,
                RelativePath = HandleRelativePath(item.RelativePath, item.Name),
                FolderName = string.Empty,
                MemberEmail = item.MemberEmail,
                ModifiedBy = item.ModifiedBy,
                Description = item.Description,
                Permissions = item.Permissions,
                DriveId = item.DriveId,
                GoogleDrivePathUrl = item.Path,
                OriginFileName = OriginalFilename,
                LabelApplyInfos = item.MetaInfo.Labels.Select(x => new DownloadedFileInfo.LabelApplyInfo
                {
                    Id = x.Id,
                    Name = x.Title,
                }).ToList()
            });
        }
        private string HandleFileNameWithSpecialCharacter(string item)
        {
            string fileName = item;
            string invalidChars = @"[\\/:*?""<>|]";
            if (Regex.IsMatch(fileName, invalidChars))
            {
                return Regex.Replace(fileName, invalidChars, "#");
            }

            return fileName;
        }

        private string HandleRelativePath(string relativePath, string fileName)
        {
            int index = relativePath.LastIndexOf(fileName, StringComparison.Ordinal);
            string directory = relativePath.Substring(0, index);
            string file = HandleFileNameWithSpecialCharacter(relativePath.Substring(index));
            return directory + file;
        }

    }
}