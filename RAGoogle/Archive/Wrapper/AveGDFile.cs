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
using Aspose.Pdf;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.RA.Contract.Aos;
using AvePoint.Wrapper.Common;
using Google;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using Microsoft.Office.Project.Server.Schema;
using RAGoogle.Models;
using RAGoogle.Models.GoogleObjectModel;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using RAGoogle.Restore.Common;
using RAGoogle.Restore.Report;
using RAGoogle.Restore.Content;
using RAGoogle.Services;
using RAGoogle.Util;
using static Google.Apis.Drive.v3.Data.Permission;
using StringExtension = AvePoint.Wrapper.Common.StringExtension;
using System.IO;
using System.Reflection.Metadata;
using System.Linq;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common;
using RAGoogle.Extension;

namespace RAGoogle.Archive.Wrapper
{
    public class AveGDFile : AveGDBase
    {
        private FileProxy _fileProxy { get; set; }
        private GoogleItemData _itemData { get; set; }
        public GoogleItemData ItemData
        {
            get { return _itemData; }
            set
            {
                _itemData = value;
            }
        }
        private bool _isCurrentVersion { get; set; }
        private bool _isVersion { get; set; }
        public bool IsCurrentVersion
        {
            get { return _isCurrentVersion; }
            set { _isCurrentVersion = value; }
        }
        protected override string Id
        {
            get
            {
                return _fileProxy?.Id ?? base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        public AveGDFile(RMAosGoogleAppProfile googleAppProfile, GoogleDriveData driveInfo, GoogleActionType action) : base(googleAppProfile, driveInfo, action)
        {
        }
        public AveGDFile(AveGDrive gDrive) : base(gDrive)
        {
        }

        public AveGDFile(AveGDFolder parentFolder) : base(parentFolder)
        {
        }
        #region Get data from api
        public virtual async Task<FileProxy> BackupSelf(GoogleItemData item)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFile.BackupSelf"))
            {
                _itemData = item;
                var file = await DriveService.GetFileByIdAsync(item.Id);
                var labels = await DriveService.GetLabelsAppliedOnFileAsync(item.Id);
                var permissions = await DriveService.GetPermissionsByIdAsync(item.Id);

                _fileProxy = await MapFileProxy(file, permissions, labels);
                return _fileProxy;
            }
        }
        public virtual async Task<FileProxy> MapFileProxy(Google.Apis.Drive.v3.Data.File file, List<Permission> permissions, List<Label> labels)
        {
            var dict = ConvertObjToDictionary(file);
            if (dict.TryGetValue("Owners", out var ownersObject) && ownersObject is List<User> owners)
            {
                dict["Owners"] = owners.Select(o => new UserProxy(ConvertObjToDictionary(o))).ToList();
            }
            if (dict.TryGetValue("LastModifyingUser", out var lastModifiedBy) && lastModifiedBy is User lastModifiedByUser)
            {
                dict["LastModifyingUser"] = lastModifiedByUser != null ? new UserProxy(ConvertObjToDictionary(lastModifiedByUser)) : null;
            }
            if (!dict.ContainsKey("Permissions"))
            {
                dict["Permissions"] = permissions.Select(MapPermisisonProxy).ToList();
            }
            else
            {
                if (dict.TryGetValue("Permissions", out var permissionobject) && permissionobject is List<Permission> permissionDatas)
                {
                    dict["Permissions"] = permissionDatas.Select(MapPermisisonProxy).ToList();
                }
            }
            if (!dict.ContainsKey("MimeType"))
            {
                dict["MimeType"] = _itemData.MimeType ?? file.MimeType ?? throw new Exception("Not found file mime type.");
            }

            if (dict.TryGetValue("ContentRestrictions", out var contentRestrictionObjects))
            {
                if (contentRestrictionObjects is List<ContentRestriction> contentRestrictions)
                {
                    dict["ContentRestrictions"] = contentRestrictions.Select(o => new ContentRestrictionProxy(ConvertObjToDictionary(o))).ToList();
                }
            }
            if(labels.IsNotNullOrEmpty())
            {
                dict["Labels"] = labels.Select(MapLabelProxy).ToList();
            }
            return new FileProxy(dict);
        }
        public LabelProxy MapLabelProxy(Label label)
        {
            var dict = ConvertObjToDictionary(label);
            if (dict.TryGetValue("Fields", out var FieldObjects) && FieldObjects is Dictionary<string, LabelField> fields)
            {
                var fieldsDic = new Dictionary<string, LabelFieldProxy>();
                foreach(var field in fields)
                {
                    fieldsDic[field.Key] = MapLabelFieldProxy(field.Value);
                }
            }
            return new LabelProxy(dict);
        }
        public LabelFieldProxy MapLabelFieldProxy(LabelField labelField)
        {
            var dict = ConvertObjToDictionary(labelField);
            if (dict.TryGetValue("User", out var usersObject) && usersObject is List<User> users)
            {
                dict["User"] = users.Select(o => new UserProxy(ConvertObjToDictionary(o))).ToList();
            }
            return new LabelFieldProxy(dict);
        }
        public PermissionProxy MapPermisisonProxy(Permission permission)
        {
            var dict = ConvertObjToDictionary(permission);
            if (dict.TryGetValue("PermissionDetails", out var PermissionDetailsObject) && PermissionDetailsObject is List<PermissionDetailsData> permissionDetails)
            {
                dict["PermissionDetails"] = permissionDetails.Select(p => new PermissionDetailsDataProxy(ConvertObjToDictionary(p))).ToList();
            }
            return new PermissionProxy(dict);
        }
        public async Task<(List<DownloadedFileInfo>, List<string>)> FileVersionsDownloadedAsync()
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFile.FileVersionsDownloadedAsync"))
            {
                var downloadManagement = new ExportFileDownloadManagement(null, DriveService);
                var allFilePaths = downloadManagement.GetFilePaths();
                var allDownLoadFiles = await downloadManagement.DownloadFileWithVersionsAsync(_itemData);
                return (allDownLoadFiles, allFilePaths);
            }
        }
        #endregion
        #region Export
        public void ExportFileMetaData(IAveBackupStream output, DownloadedFileInfo versionItem)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFile.ExportFileMetaData"))
            {
                output.WriteMetadata(AveMetadataType.DriveFileMetadata, GetGDFileBasic(versionItem));
            }
        }
        private GDFileBasic GetGDFileBasic(DownloadedFileInfo versionItem)
        {
            var filebasic = new GDFileBasic
            {
                DocId = _fileProxy.Id,
                Name = _fileProxy.Name,
                MimeType = versionItem.IsCurrentVersion ? _fileProxy.MimeType : versionItem.MimeType,
                Size = versionItem.IsCurrentVersion ? _fileProxy.Size : versionItem.Size,
                ParentId = _fileProxy.Parents.IsNotNullOrEmpty() ? _fileProxy.Parents[0] : string.Empty,
                ParentIds = _itemData.ParentIds,
                DriveName = _itemData.DriveName,
                Level = _itemData.Level.ToString(),
                Description = _fileProxy.Description,
                Starred = _fileProxy.Starred,
                Type = (int)GDriveDataType.File,
                Path = _itemData.RelativePath,
                Labels = ConvertFileLabelDto(_fileProxy.Labels),
                IsCurrentVersion = versionItem.IsCurrentVersion,
                OriginalFilename = versionItem.OriginFileName,
                ModifiedTime = versionItem.IsCurrentVersion ? (_fileProxy.ModifiedTimeDateTimeOffset == null ? 0 : _fileProxy.ModifiedTimeDateTimeOffset.Value.Ticks) :  (versionItem.ModifiedTime.Ticks),
                CreatedTime = _fileProxy.CreatedTimeDateTimeOffset == null ? 0 : _fileProxy.CreatedTimeDateTimeOffset.Value.Ticks,
                ContentRestrictions = _fileProxy.ContentRestrictions?.Select(c => new ContentRestrictionData
                {
                    ReadOnly = c.ReadOnly__,
                    Reason = c.Reason
                }).ToList(),
                ModifiedById = _fileProxy.LastModifyingUser?.PermissionId,
                ModifiedBy = _fileProxy.LastModifyingUser?.EmailAddress,
            };
            if (_fileProxy.Owners != null && _fileProxy.Owners.Count > 0)
            {
                filebasic.Owners = new UserData
                {
                    OwnerEmail = _fileProxy.Owners[0].EmailAddress,
                    OwnerDisplayName = _fileProxy.Owners[0].DisplayName
                };
            }
            return filebasic;
        }
        private List<LabelData> ConvertFileLabelDto(List<LabelProxy> labels)
        {
            List<LabelData> result = new List<LabelData>();
            if (labels != null && labels.Any())
            {
                labels.ForEach((item) =>
                {
                    var dto = new LabelData()
                    {
                        Id = item.Id,
                        Kind = item.Kind,
                        RevisionId = item.RevisionId,
                    };
                    if (item.Fields != null && item.Fields.Any())
                    {
                        dto.Fields = new Dictionary<string, LabelFieldData>();
                        foreach (var field in item.Fields)
                        {
                            if (!dto.Fields.ContainsKey(field.Key))
                            {
                                dto.Fields.Add(field.Key, ConvertFileLabelFieldDto(field.Value));
                            }
                        }
                    }
                    result.Add(dto);
                });
            }
            return result;
        }
        private LabelFieldData ConvertFileLabelFieldDto(LabelFieldProxy field)
        {
            LabelFieldData result = null;
            if (field != null)
            {
                result = new LabelFieldData()
                {
                    Id = field.Id,
                    Kind = field.Kind,
                    ValueType = field.ValueType,

                };
                if (field.DateString != null)
                {
                    result.DateString = field.DateString.ToList();
                }
                else if (field.Integer != null)
                {
                    result.Integer = field.Integer.ToList();
                }
                else if (field.Selection != null)
                {
                    result.Selection = field.Selection.ToList();
                }
                else if (field.Text != null)
                {
                    result.Text = field.Text.ToList();
                }
                else if (field.User != null)
                {
                    result.User = field.User.Select(e => new UserData
                    {
                        OwnerDisplayName = e.DisplayName,
                        OwnerEmail = e.EmailAddress,
                    }).ToList();
                }
            }
            return result;
        }

        public void ExportFilePermission(IAveBackupStream output)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFile.ExportFilePermission"))
            {
                output.WriteMetadata(AveMetadataType.DriveFilePermission, GetGDFilePermission(_fileProxy));
            }
        }
        public List<PermissionInfo> GetGDFilePermission(FileProxy file)
        {
            var filePermission = file.Permissions.Select(x => new PermissionInfo
            {
                Id = x.Id,
                Type = x.Type,
                Role = x.Role,
                EmailAddress = x.EmailAddress,
                AllowFileDiscovery = x.AllowFileDiscovery,
                Domain = x.Domain,
                ExpirationTime = x.ExpirationTimeDateTimeOffset == null ? 0 : x.ExpirationTimeDateTimeOffset.Value.Ticks,
                PermissionDetails = x.PermissionDetails?
                    .Select(x => new AvePermissionDetailsData
                    {
                        PermissionRole = x.Role,
                        PermissionType = x.PermissionType,
                        Inherited = x.Inherited,
                        InheritedFrom = x.InheritedFrom,
                    })
                    .ToList() ?? [],
            }).ToList();

            return filePermission;
        }
        public void ExportContent(IAveBackupStream output, string localPath)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFile.ExportContent"))
            using (Stream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            {
                if (stream != null)
                {
                    try
                    {
                        //to do: set size
                        byte[] buffer = output.DataBuffer;
                        int length;
                        output.FlushMetadata(stream.Length);
                        long readSize = 0;
                        while (readSize < stream.Length)
                        {
                            length = stream.Read(buffer, 0, buffer.Length);
                            if (length == 0)
                            {
                                break;
                            }
                            readSize += length;
                            output.WriteContent(buffer, 0, length);
                        }
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                }
                else
                {
                    output.FlushMetadata(0);
                }
            }

        }
        #endregion
        #region Restore
        public async Task<bool> HandleRestoreFileMetaData(RestoreContentDto aveItemDto, GDFileBasic srcBasicInfo, IAveRestoreStream RestoreStream)
        {
            using var _ = new PerformanceScope("AveGDFile.HandleRestoreFileMetaData");
            var fileOldId = srcBasicInfo.DocId;
            ObjectIdMappings.TryGetValue(srcBasicInfo.ParentId, out var realParentId);
            ObjectIdMappings.TryGetValue(fileOldId, out var existFileId);
            
            srcBasicInfo.DocId = existFileId ?? srcBasicInfo.DocId;
            _isVersion = aveItemDto.Version.IsNotNullOrEmpty();//srcBasicInfo.Name.Contains(":");
            _isCurrentVersion = !_isVersion;
            _currentFileOwner = srcBasicInfo.CreatedBy ?? srcBasicInfo.ModifiedBy;
            //srcBasicInfo.Name = srcBasicInfo.Name;// GetOriginalFileName(srcBasicInfo.Name);
            srcBasicInfo.ParentId = realParentId ?? srcBasicInfo.ParentId;
            var hasRestoredPreviouVersion = existFileId != null; // false: first file, true:file version
            var contentPath = WriteStreamToFile(new AveSPFileStream(RestoreStream));
            var currentFile = await GetCurrentItem(srcBasicInfo, ConflictResolution == ConflictResolutionType.AppendItemOrDocumentByReNamed);
            _logger.Info($"File status: id: {fileOldId}, version:{aveItemDto.Version}, is version:{_isVersion}, is current version:{_isCurrentVersion}, has exist file:{hasRestoredPreviouVersion}");
            if (currentFile == null) // not get file, should create it directly
            {
                _isNewCreated = true;
                _logger.Info($"File is not found");
            }
            else // only need to handle conflict resolution for first version
            {
                _logger.Info($"File is restored currentId {currentFile.Id}, backup: {fileOldId}");

                switch (ConflictResolution)
                {
                    case ConflictResolutionType.Skip:
                        if (!hasRestoredPreviouVersion)
                        {
                            _logger.Info($"File {fileOldId} exists in Drive, skip restore.");
                            this.AveRestoreReportDto.Status = RestoreStatus.Skipped;
                            _fileProxy = await MapFileProxy(currentFile, new(), new());
                            ObjectIdMappings.TryAdd(fileOldId, currentFile.Id);
                            _isNewCreated = false;
                            return true;
                        } break;

                    case ConflictResolutionType.Overwrite:
                        if (!hasRestoredPreviouVersion)
                        {
                            _logger.Info($"File {fileOldId} exists, delete old file before restoring.");
                            await DriveService.TryDeleteItemById(currentFile.Id);
                            _logger.Info($"File {fileOldId} deleted successfully.");
                            _isNewCreated = true;
                        }
                        break;

                    case ConflictResolutionType.AppendItemOrDocumentByReNamed:
                        
                        if (!hasRestoredPreviouVersion)
                        {
                            await GenerateFileName(srcBasicInfo);
                            _logger.Info($"File {fileOldId} exists, appending new renamed :{srcBasicInfo.Name}.");
                            _isNewCreated = true;
                        }
                        else
                        {
                            if(ObjectIdAndNameMappings.TryGetValue(srcBasicInfo.DocId, out var fileName))
                            {
                                srcBasicInfo.Name = fileName;
                            }
                        }
                        break;
                }
            }
            currentFile = await CreateFileOrVersion(srcBasicInfo, hasRestoredPreviouVersion, contentPath);
            this.AveRestoreReportDto.Size = srcBasicInfo.Size ?? 0;
            _fileProxy = await MapFileProxy(currentFile, new(), new());

            ObjectIdMappings.TryAdd(fileOldId, currentFile.Id);
            srcBasicInfo.DocId = currentFile.Id;
            if (ConflictResolution == ConflictResolutionType.AppendItemOrDocumentByReNamed)
            {
                ObjectIdAndNameMappings.TryAdd(currentFile.Id, currentFile.Name);
            }
            if(_isCurrentVersion)
            {//last version update all properties
                await UpdateVersionDownload(currentFile.Id);
                //restore metadata
                await UpdateFileAsync(srcBasicInfo);
                //restore labels
                await RestoreFileLabels(srcBasicInfo);
                _logger.Info($"File {fileOldId} restore label finish.");
            }
            return false;
        }
        private async Task UpdateVersionDownload(string fileId)
        {
            try
            {
                var versions = await DriveService.GetAllFileVersionsAsync(fileId);
                if (versions != null)
                {
                    foreach (var ver in versions)
                    {
                        if (ver.KeepForever == false)
                        {
                            ver.KeepForever = true;
                            await DriveService.UpdateFileVersion(fileId, ver.Id, ver);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
               _logger.Error($"Update version download failed.Error:{ex}");
            }
        }
        private async Task<bool> UpdateFileAsync(GDFileBasic basicInfo)
        {
            using var _ = new PerformanceScope("AveGDFile.UpdateFileAsync");
            var file = new Google.Apis.Drive.v3.Data.File()
            {
                ModifiedTimeDateTimeOffset = basicInfo.ModifiedTime == 0 ? null : new DateTimeOffset(new DateTime(basicInfo.ModifiedTime)),
                Properties = basicInfo.Properties.IsNullOrEmpty() ? new Dictionary<string, string>() : SerializerHelper.DeserializeByJsonConvert<IDictionary<string, string>>(basicInfo.Properties),
                AppProperties = basicInfo.AppProperties.IsNullOrEmpty() ? new Dictionary<string, string>() : SerializerHelper.DeserializeByJsonConvert<IDictionary<string, string>>(basicInfo.AppProperties),

                ContentRestrictions = new List<ContentRestriction>()
                {
                    new ContentRestriction
                    {
                        ReadOnly__ = basicInfo.ContentRestrictions?.FirstOrDefault()?.ReadOnly ?? false,
                        Reason = basicInfo.ContentRestrictions?.FirstOrDefault()?.Reason
                    }
                }
            };
            if (GoogleConstant.GoogleGASMimeType.Any(s => s == basicInfo.MimeType))
            { 
                file.ContentRestrictions = null; // google doc not support content restriction
            }
            await DriveService.UpdateFolderAsync(file,basicInfo.DocId);
            _logger.Info($"Update file lock status finish.");
            return true;
        }
        private async Task<Google.Apis.Drive.v3.Data.File> CreateFileOrVersion(GDFileBasic file,  bool isVersion, string filePath)
        {
            using var _ = new PerformanceScope("AveGDFile.CreateFileOrVersion");
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return await UploadFileAsync(file, stream, isVersion);
            }
        }
        private string WriteStreamToFile(Stream stream)
        {
            var path = Path.Combine(WrapperConfiguration.TempDirectory, Guid.NewGuid().ToString());
            using (FileStream localStream = new FileStream(path, FileMode.Create))
            {
                Int32 readLen = 0;
                stream.Position = 0;
                var cacheBuffer = new Byte[1024 * 64];
                while ((readLen = stream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                {
                    localStream.Write(cacheBuffer, 0, readLen);
                }
            }
            return path;
        }

        public string GetOriginalFileName(string versionName)
        {
            int index = versionName.LastIndexOf(':');
            if (index == -1) return versionName;

            return versionName.Substring(0, index);
        }
        private async Task<bool> RestoreFileLabels(GDFileBasic dto, FileQuery query = null)
        {
            using var _ = new PerformanceScope("AveGDFile.RestoreFileLabels");
            if (dto.Labels != null && dto.Labels.Any())
            {
                var labels = await LabelService.ListLabelsPublishedAsync();
                return await DriveService.RestoreFileLabels(dto, labels, query);
            }
            else
            {
                return true;
            }
        }
        
        public async Task<Google.Apis.Drive.v3.Data.File> UploadFileAsync(GDFileBasic dto, Stream stream, bool restoreRevision)
        {
            try
            {
                var newFile = CreateFileByDto(dto);
                Google.Apis.Drive.v3.Data.File file;
                if (!restoreRevision)
                {
                    newFile.Parents = new List<string>() { dto.ParentId };
                }
                if (GoogleConstant.GoogleVideoMimeType.Contains(newFile.MimeType))
                {
                    newFile.MimeType = GoogleConstant.GoogleMP4; // force to mp4 for video
                }
                //var oldId = dto.DocId;
                if (stream == null && !restoreRevision)
                {
                    file = await DriveService.CreateFileAsync(newFile);
                    //dto.DocId = file.Id;
                    //_fileProxy = MapFileProxy(file, new List<Permission>(), new List<Label>());
                }
                else
                {
                    stream.Position = 0;
                    var mimeType = GoogleConstant.GoogleExportMimeType.TryGetValue(dto.MimeType, out var exportMimeType) ? exportMimeType : dto.MimeType;
                    file = await DriveService.UploadFileAsync(newFile, dto.DocId, mimeType, restoreRevision, stream);
                    if (GoogleConstant.GoogleGASMimeType.Any(s=>s == dto.MimeType) && !dto.ParentId.EqualIgnoreCase(file.Parents[0]))
                    {
                        await DriveService.MoveToNewFolder(file.Id, file.Parents[0], dto.ParentId);
                    }
                    //dto.DocId = file.Id;
                    //_fileProxy = MapFileProxy(file, new List<Permission>(), new List<Label>());
                }
                _logger.Info($"Create file successfully.New Id:{dto.DocId}");
                //ObjectIdMappings.TryAdd(oldId, dto.DocId);
                return file;
            }
            catch (GoogleApiException gex)
            {
                _logger.Error($"An error occurred when uploading file {dto.DocId} failed.Error:{gex}");
                stream?.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when uploading file {dto.DocId} failed.Error:{e}");
                stream?.Dispose();
                if (e.Message.Contains("File ResponseBody is null"))
                {
                    throw;
                }
            }

            throw new CommonException($"File {dto.DocId} upload failed.");
        }
        protected async Task GenerateFileName(GDFileBasic dto)
        {
            string fileName = dto.Name;

            File tempFile = null;
            FileQuery query = new FileQuery()
            {
                SupportsAllDrives = true,
                IncludeItemsFromAllDrives = true,
            };

            int i = 1;
            string appendFileName;
            do
            {
                _logger.Info($"GetFileNameWithSuffix for {dto.DocId}, mimeType: {dto.MimeType}");
                appendFileName = GoogleFileExtension.GetFileNameWithSuffix(dto.Name, $"_{i++}");
                if (appendFileName.Contains('*'))
                {
                    query.QueryString = dto.ParentId == null ? $"'{ParentDrive.DriveProxy.Id}' in parents and fullText contains '{EscapeString(appendFileName)}'" : $"'{dto.ParentId}' in parents and fullText contains '{EscapeString(appendFileName)}'";
                }
                else
                {
                    query.QueryString = dto.ParentId == null ? $"'{ParentDrive.DriveProxy.Id}' in parents and name contains '{EscapeString(appendFileName)}'" : $"'{dto.ParentId}' in parents and name contains '{EscapeString(appendFileName)}'";
                }
                query.QueryString += " and trashed = false";
                var appendFiles = await DriveService.ListFileRestoreAsync(query);
                tempFile = appendFiles?.Find(f => f.Name.Equals(appendFileName));
                
            }
            while (tempFile != null);
            dto.Name = appendFileName;
        }
        private File CreateFileByDto(GDFileBasic dto)
        {
            return new File()
            {
                Name = dto.Name,
                MimeType = dto.MimeType,
                Description = dto.Description,
                Starred = dto.Starred,
                OriginalFilename = dto.OriginalFilename,
                ModifiedTimeDateTimeOffset = dto.ModifiedTime == 0 ? null : new DateTimeOffset(new DateTime(dto.ModifiedTime)),
            };
        }
        #endregion
    }
}
