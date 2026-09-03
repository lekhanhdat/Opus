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
using AvePoint.RA.Contract.Aos;
using RAGoogle.Models;
using File = Google.Apis.Drive.v3.Data.File;
using RAGoogle.Report;
using RAGoogle.Restore.Report;
using AvePoint.Wrapper.Common;
using RAGoogle.Extension;
using RAGoogle.Models.GoogleObjectModel;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.RA.Common.Global.Utils;
using Newtonsoft.Json;
using AngleSharp.Io;
using AvePoint.GCommon.GraphAPI;
using Microsoft.Azure.Amqp.Framing;
using AvePoint.GCommon.Contract.Media.Object;
using Google.Apis.Drive.v3.Data;
using AvePoint.RA.Common;

namespace RAGoogle.Archive.Wrapper
{
    public class AveGDFolder : AveGDFile
    {
        private readonly string anyonePermissionId = "anyoneWithLink";
        private readonly string anyonePermissionName = "Anyone with the link";
        private FileProxy _folderProxy { get; set; }
        public FileProxy FolderProxy
        {
            get => _folderProxy;
            set => _folderProxy = value;
        }
        protected override string Id
        {
            get
            {
                return _folderProxy.Id ?? base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        private GDPermissionList _archivePermissionInfo { get; set; }

        public AveGDFolder(RMAosGoogleAppProfile googleAppProfile, GoogleDriveData driveInfo, ReportCenter reportCenter, GoogleActionType action) : base(googleAppProfile, driveInfo, action)
        {
            _reportCenter = reportCenter;
        }
        //root folder
        public AveGDFolder(AveGDrive gDrive) : base(gDrive)
        {
            _gDrive = gDrive;
            _isRootFolder = true;
        }
        //sub folder
        public AveGDFolder(AveGDFolder gFolder) : base(gFolder)
        {
            _gDrive = gFolder.ParentDrive;
        }
       
        public async Task HandleRestoreGoogleFolderBasicInfo(GDFileBasic srcBasicInfo)
        {
            using var _ = new PerformanceScope("AveGDFolder.HandleRestoreGoogleFolderBasicInfo");
            try
            {
                _isNewCreated = false;
                _currentFileOwner = srcBasicInfo.ModifiedBy;
                ObjectIdMappings.TryGetValue(srcBasicInfo.ParentId, out var realParentId);
                srcBasicInfo.ParentId = realParentId ?? srcBasicInfo.ParentId;
                //find folder
                var currentFolder  = await DriveService.PageFoldersByIdAsync(srcBasicInfo.ParentId, srcBasicInfo.DocId);
                if (currentFolder == null)
                {
                    currentFolder = await DriveService.PageFoldersByNameAsync(realParentId, srcBasicInfo.Name);
                }

                //restore folder
                if (currentFolder == null)
                {
                    currentFolder = await DriveService.CreateNewFolderAsync(srcBasicInfo);
                    ObjectIdMappings.TryAdd(srcBasicInfo.DocId, currentFolder.Id);
                    _isNewCreated = true;
                    _logger.Info($"Create folder successfully !");
                }
                else
                {
                    ObjectIdMappings[srcBasicInfo.DocId]= currentFolder.Id;
                    _logger.Info($"Get exist folder successfully.");
                }
                Id = currentFolder.Id;
                _folderProxy = new FileProxy(ConvertObjToDictionary(currentFolder));
                //update metadata;
                if (_isNewCreated)
                {
                    var newFolder = new File
                    {
                        Name = srcBasicInfo.Name,
                        Description = srcBasicInfo.Description,
                        FolderColorRgb = srcBasicInfo.ColorRgb,
                        Starred = srcBasicInfo.Starred,
                        ModifiedTimeDateTimeOffset = srcBasicInfo.ModifiedTime == 0 ? null : new DateTimeOffset(new DateTime(srcBasicInfo.ModifiedTime)),
                        Properties = srcBasicInfo.Properties.IsNullOrEmpty() ? new Dictionary<string, string>() : SerializerHelper.DeserializeByJsonConvert<IDictionary<string, string>>(srcBasicInfo.Properties),
                        AppProperties = srcBasicInfo.AppProperties.IsNullOrEmpty() ? new Dictionary<string, string>() : SerializerHelper.DeserializeByJsonConvert<IDictionary<string, string>>(srcBasicInfo.AppProperties),
                    };
                    await DriveService.UpdateFolderAsync(newFolder, currentFolder.Id);
                    _logger.Info($"Restore folder metadata successfully !");
                }
                else
                {
                    _logger.Warn($"Restore folder metadata skipped");
                }
                this.AveRestoreReportDto.Path = srcBasicInfo.Path;
                this.AveRestoreReportDto.Size = 0;
                this.AveRestoreReportDto.Status = _isNewCreated ? RestoreStatus.Success : RestoreStatus.Skipped;
                _reportCenter.HasCompleteNode = true;
            }
            catch (Exception ex)
            {
                _reportCenter.HasErrorNode = true;
                this.AveRestoreReportDto.Status = RestoreStatus.Failed;
                this.AveRestoreReportDto.ErrorMessage = ex.Message;
                _logger.Warn($"Failed to handle folder basic info {srcBasicInfo.Name} Drive {srcBasicInfo.DriveName}.Error:{ex}");
            }
        }

        

        public static bool NeedUpdateFileItemGoogleDrive(File currentFolder, GDFileBasic srcBasicInfo)
        {
            if (currentFolder.Name != srcBasicInfo.Name)
                return true;

            if ((currentFolder.Description ?? string.Empty) != (srcBasicInfo.Description ?? string.Empty))
                return true;

            if (currentFolder.Starred != srcBasicInfo.Starred)
                return true;

            if ((currentFolder.FolderColorRgb ?? string.Empty) != (srcBasicInfo.ColorRgb ?? string.Empty))
                return true;

            return false;
        }

        public override async Task<FileProxy> BackupSelf(GoogleItemData item)
        {
            using var _ = new PerformanceScope("AveGDFolder.BackupSelf");
            var folder = await DriveService.GetFileByIdAsync(item.Id);
            var permissions = await DriveService.GetPermissionsByIdAsync(item.Id);
            _folderProxy = await MapFileProxy(folder, permissions, null);
            return _folderProxy;
        }

        public void ExportFolderBasicInfo(IAveBackupStream output, GoogleItemData item)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFolder.ExportFolderBasicInfo"))
            {
                output.WriteMetadata(AveMetadataType.DriveFolderMetadata, GetGoogleFolderBasicInfo(item));
            }
        }

        public void ExportFolderPermissionsInfo(IAveBackupStream output)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDFolder.ExportFolderPermissionsInfo"))
            {
                var permissionList = new GDPermissionList
                {
                    Permissions = GetGDFilePermission(_folderProxy)
                };
                output.WriteMetadata(AveMetadataType.DriveFolderPermission, permissionList);
            }
        }

        public GDFileBasic GetGoogleFolderBasicInfo(GoogleItemData item)
        {
            var archiveFolderBasicInfo = new GDFileBasic()
            {
                DocId = _folderProxy.Id,
                ParentId = _folderProxy.Parents.IsNotNullOrEmpty() ? _folderProxy.Parents[0] : item.ParentId,
                ParentIds = item.ParentIds,
                DriveName = item.DriveName,
                Name = _folderProxy.Name,
                MimeType = _folderProxy.MimeType,
                Path = item.RelativePath,
                CreatedBy = item.CreatedBy,
                ModifiedBy = _folderProxy.LastModifyingUser?.EmailAddress,
                ModifiedById = _folderProxy.LastModifyingUser?.PermissionId,
                Level = item.Level.ToString(),
                Type = (int)GDriveDataType.Folder,
                //ModifierName = item.ModifierName,
                ModifiedTime = _folderProxy.ModifiedTimeDateTimeOffset == null ? 0 : _folderProxy.ModifiedTimeDateTimeOffset.Value.Ticks,
                CreatedTime = _folderProxy.CreatedTimeDateTimeOffset == null ? 0 : _folderProxy.CreatedTimeDateTimeOffset.Value.Ticks,
                DriveId = _folderProxy.DriveId,
                Description = _folderProxy.Description,
                ColorRgb = _folderProxy.FolderColorRgb,
                Starred = _folderProxy.Starred,
                Properties = JsonConvert.SerializeObject(_folderProxy.Properties),
                AppProperties = JsonConvert.SerializeObject(_folderProxy.AppProperties),
            };
            return archiveFolderBasicInfo;
        }

        private async Task<GDPermissionList> GetItemPermisisonsObject(string docId)
        {
            var permissions = await DriveService.GetPermissionsByIdAsync(docId, false);

            var folderPermissions = permissions.Select(per => new PermissionInfo
            {
                Id = per.Id,
                DisplayName = per.Id == anyonePermissionId ? anyonePermissionName : per.DisplayName,
                AllowFileDiscovery = per.AllowFileDiscovery,
                Type = per.Type,
                Deleted = per.Deleted,
                EmailAddress = per.EmailAddress,
                ExpirationTimeRaw = per.ExpirationTimeRaw,
                ExpirationTime = per.ExpirationTimeDateTimeOffset == null ? 0 : per.ExpirationTimeDateTimeOffset.Value.Ticks,
                PhotoLink = per.PhotoLink,
                Role = per.Role,
                Domain = per.Domain,
                PermissionDetails = per.PermissionDetails?.Select(detail => new AvePermissionDetailsData
                {
                    PermissionRole = detail.Role,
                    PermissionType = detail.PermissionType,
                    Inherited = detail.Inherited,
                    InheritedFrom = detail.InheritedFrom,
                }).ToList() ?? [],
            }).ToList();

            return new GDPermissionList
            {
                Permissions = folderPermissions
            };
        }
    }
}
