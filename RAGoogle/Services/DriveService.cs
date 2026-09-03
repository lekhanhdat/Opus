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

using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Services;
using Google;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Archive.Wrapper;
using Google.Apis.DriveLabels.v2.Data;
using RAGoogle.GoogleObjDiscover.Services;
using RAGoogle.Models;
using RAGoogle.Models.GoogleObjectModel;
using RAGoogle.Util;
using static Google.Apis.Drive.v3.Data.Drive;
using File = Google.Apis.Drive.v3.Data.File;
using StringExtension = AvePoint.Wrapper.Common.StringExtension;
using RAGoogle.Extension;

namespace RAGoogle.Services;

public class GoogleDriveService : BaseService, IDisposable
{
    private static IRALogger logger = RALogger.GetInstance(typeof(GoogleDriveService));
    private DriveApi _driveApi;
    private string _labelIds { get; set; }
    public GoogleDriveService(RMAosGoogleAppProfile app, string impersonateUser = "") : base(app, impersonateUser, GoogleScopeType.Drive)
    {
        _driveApi = new(initializer);
    }
    public string DelegateUser => this.impersonateUser;
    
    #region drive
    public async Task<Drive> GetDriveAsync(string driveId)
    {
        try
        {
            FileQuery query = new()
            {
                SharedDriveId = driveId,
            };
            return await _driveApi.GetDriveAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get drive by id {driveId} failed, Message: {ex}");
            //throw;
        }
        return null;
    }
    public async Task<Drive> GetDriveByNameAsync(string driveName, string driveId)
    {
        FileQuery query = new()
        {
            QueryString = $"name = '{driveName}'",
            UseDomainAdminAccess = true,
            SupportsAllDrives = true,
            IncludeItemsFromAllDrives = true,
        };
        try
        {
            var drives = await _driveApi.ListSharedDrivesAsync(query);
            return drives.FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.Error($"Get drive by name {driveName} failed, Message: {ex}");
            throw;
        }
    }
    public async Task<File> GetMyDriveAsync(string userEmail)
    {
        try
        {
            FileQuery query = new FileQuery
            {
                QuotaUser = userEmail
            };
            return await _driveApi.GetRootFolderAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get root folder my drive failed, Message: {ex}");
            throw;
        }
    }
    public async Task<Drive> CreateDrive(Drive drive)
    {
        try
        {
            return await _driveApi.CreateDrivesync(drive);
        }
        catch (Exception ex)
        {
            logger.Error($"Create drive failed, Message: {ex}");
            throw;
        }
    }
    public async Task<Drive> UpdateDriveAsync(Drive drive, string driveId)
    {
        try
        {
            return await _driveApi.UpdateDriveAsync(drive, driveId);
        }
        catch (Exception ex)
        {
            logger.Error($"Update drive failed, Message: {ex}");
            throw;
        }
    }
    public async Task<Drive> UpdateDriveSettingAsync(DriveProxy driveProxy)
    {
        try
        {
            var drive = new Drive(); //await this.GetDriveAsync(driveProxy.Id);
            drive.Restrictions = new RestrictionsData
            {
                AdminManagedRestrictions = driveProxy.Restrictions.AdminManagedRestrictions,
                CopyRequiresWriterPermission = driveProxy.Restrictions.CopyRequiresWriterPermission,
                DomainUsersOnly = driveProxy.Restrictions.DomainUsersOnly,
                DriveMembersOnly = driveProxy.Restrictions.DriveMembersOnly,
                SharingFoldersRequiresOrganizerPermission = driveProxy.Restrictions.SharingFoldersRequiresOrganizerPermission,
            };
            return await _driveApi.UpdateDriveAsync(drive, driveProxy.Id);
        }
        catch (Exception ex)
        {
            logger.Error($"Update drive failed, Message: {ex}");
            throw;
        }
    }
    public async Task<File> GetRootFolderMyDriveAsync()
    {
        try
        {
            return await _driveApi.GetRootFolderAsync();
        }
        catch (Exception ex)
        {
            logger.Error($"Get root folder my drive failed, Message: {ex}");
            throw;
        }
    }
    #endregion

    #region permission
    public async Task<List<Permission>> GetPermissionsByIdAsync(string itemId, bool isSharedDrive = false)
    {
        try
        {
            FileQuery query = new()
            {
                UseDomainAdminAccess = isSharedDrive
            };
            return await _driveApi.ListPermissionAsync(itemId, query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get file's permissions failed, Message: {ex}");
            throw;
        }
    }
    public async Task<string> DeletePermissionAsync(string fileId, string permissionId, FileQuery query = null)
    {
        try
        {
            return await _driveApi.DeletePermissionAsync(fileId, permissionId);
        }
        catch (Exception ex)
        {
            logger.Error($"Delete permissions failed, Message: {ex}");
            throw;
        }
    }
    public async Task UpdatePermissionAsync(GoogleDriveMember oldPermission, GoogleDriveMember newPermission, string fileId, FileQuery query = null)
    {
        if (oldPermission.ExpirationTime > 0 && oldPermission.ExpirationTime < DateTime.Now.Ticks)
        {
            logger.Warn($"The temporary permission has been expired,skip to restore the permission {oldPermission.EmailAddress}.The Expiration Time is {oldPermission.ExpirationTime}");
            return;
        }
        bool needUpdate = false;
        var body = new Permission();
        if (!StringExtension.EqualIgnoreCase(newPermission.Role, oldPermission.Role))
        {
            body.Role = oldPermission.Role;
            if (StringExtension.EqualIgnoreCase(oldPermission.Role, "owner"))
            {
                if (query == null)
                {
                    query = new FileQuery()
                    {
                        TransferOwnership = true
                    };
                }
                else
                {
                    query.TransferOwnership = true;
                }
            }
            needUpdate = true;
        }
        if (newPermission.ExpirationTime != oldPermission.ExpirationTime && oldPermission.ExpirationTime > DateTime.Now.Ticks)
        {
            body.ExpirationTimeDateTimeOffset = new DateTimeOffset(new DateTime(oldPermission.ExpirationTime));
            body.Role = oldPermission.Role;
            needUpdate = true;
        }

        if (needUpdate)
        {
            if (newPermission.ExpirationTime > 0 && (oldPermission.ExpirationTime == 0 || (oldPermission.ExpirationTime > 0 && oldPermission.ExpirationTime > DateTime.Now.Ticks)))
            {
                logger.Info($"The current permission has an expiration date and needs to be deleted. Then create a new one. {fileId}.");
                await DeletePermissionAsync(fileId, newPermission.Id);
                await CreatePermissionAsync(oldPermission, fileId, query);
            }
            else
            {
                await _driveApi.UpdatePermissionAsync(body, fileId, newPermission.Id, query);
                logger.Info($"Update permission successfully for file {fileId}.");
            }
        }
    }
    public async Task<Permission> CreatePermissionAsync(GoogleDriveMember dto, string fileId, FileQuery query = null)
    {
        if (dto.ExpirationTime != 0 && dto.ExpirationTime < DateTimeOffset.Now.Ticks)
        {
            logger.Warn($"The temporary permission has been expired,skip to restore the permission {dto.EmailAddress}.The Expiration Time is {dto.ExpirationTime}");
            return null;
        }

        var permission = CreatePermissionByDto(dto);
        var result = await _driveApi.CreatePermissionAsync(permission, fileId, query);
        if (dto.ExpirationTime > 0)
        {
            var body = new Permission()
            {
                ExpirationTimeDateTimeOffset = new DateTimeOffset(new DateTime(dto.ExpirationTime)),
                Role = dto.Role
            };
            await _driveApi.UpdatePermissionAsync(body, fileId, result.Id, query);
        }
        logger.Info($"Create permissions successfully for file {fileId}.");
        return result;
    }
    private Permission CreatePermissionByDto(GoogleDriveMember dto)
    {
        return new Permission()
        {
            Type = dto.Type,
            Role = dto.Role,
            EmailAddress = dto.EmailAddress,
            AllowFileDiscovery = dto.AllowFileDiscovery,
            Domain = dto.Domain
        };
    }

    public async Task<Permission> CreatePermissionAsync(Permission permission, string itemId, bool isSharedDrive = false)
    {
        try
        {
            FileQuery query = new()
            {
                UseDomainAdminAccess = isSharedDrive
            };
            return await _driveApi.CreatePermissionAsync(permission, itemId, query);
        }
        catch (Exception ex)
        {
            logger.Error($"Create permission failed, Message: {ex}");
            throw;
        }
    }

    public async System.Threading.Tasks.Task DeletePermissionByMemberEmailAsync(string memberEmail, string itemId, bool isSharedDrive = false)
    {
        try
        {
            FileQuery query = new()
            {
                UseDomainAdminAccess = isSharedDrive
            };
            await _driveApi.DeletePermissionByMemberEmailAsync(memberEmail, itemId, query);
        }
        catch (Exception ex)
        {
            logger.Error($"Create permission failed, Message: {ex}");
            throw;
        }
    }
    #endregion

    #region item 
    public async Task<File> MoveToNewFolder(string fileId, string oldFolderId, string newFolderId)
    {
        try
        {
            return await _driveApi.MoveToNewFolderAsync(fileId, oldFolderId, newFolderId);
        }
        catch (GoogleApiException gex)
        {
            if (gex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                logger.Error($"User does not have sufficient permission to move item by id: {fileId}");
                throw new Exception(I18NResource.InvalidUserPermission);
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Move to new folder failed, Message: {ex}");
            throw;
        }
    }

    public async Task<(List<File> files, string? pageToken)> PageMyDriveFilesAsync(string folderId, string? pageToken = null)
    {
        List<File> files = [];
        try
        {
            FileQuery query = new()
            {
                QueryString = $"trashed = false and '{folderId}' in parents",
                SearchInDrive = true,
                SupportsAllDrives = false,
                IncludeItemsFromAllDrives = false,
                PageSize = 100,
                PageToken = pageToken,
                IncludeLabels = _labelIds,
            };
            (files, pageToken) = await _driveApi.ListFilesAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get files failed, Message: {ex}");
            throw;
        }
        return (files, pageToken);
    }
    public IAsyncEnumerable<List<File>> PageMyDriveByFolderIdAsync(string folderId, QueryType queryType)
    {
        var queryString = queryType switch
        {
            QueryType.All => $"trashed = false and '{folderId}' in parents",
            QueryType.File => $"trashed = false and '{folderId}' in parents and mimeType != 'application/vnd.google-apps.folder'",
            QueryType.Folder => $"trashed = false and '{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
            _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null)
        };
        FileQuery query = new()
        {
            QueryString = queryString,
            SearchInDrive = true,
            SupportsAllDrives = false,
            IncludeItemsFromAllDrives = false,
            PageSize = 100,
            PageToken = null,
            IncludeLabels = _labelIds,
        };
        return _driveApi.ListAllFilesAsync(query);
    }
    public IAsyncEnumerable<List<File>> PageSharedDriveByFolderIdAsync(string driveId, string folderId, QueryType queryType)
    {
        var queryString = queryType switch
        {
            QueryType.All => $"trashed = false and '{folderId}' in parents",
            QueryType.File => $"trashed = false and '{folderId}' in parents and mimeType != 'application/vnd.google-apps.folder'",
            QueryType.Folder => $"trashed = false and '{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
            _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null)
        };
        FileQuery query = new()
        {
            QueryString = queryString,
            IncludeItemsFromAllDrives = true,
            SupportsAllDrives = true,
            SearchInDrive = true,
            SharedDriveId = driveId,
            PageSize = 100,
            PageToken = null,
            IncludeLabels = _labelIds,
        };
        return _driveApi.ListAllFilesAsync(query);
    }
    public async Task<(List<File> files, string? pageToken)> PageSharedDriveByFolderAsync(string driveId, string folderId, QueryType queryType, string? pageToken = null)
    {
        List<File> files = [];
        try
        {
            var queryString = queryType switch
            {
                QueryType.All => $"trashed = false and '{folderId}' in parents",
                QueryType.File => $"trashed = false and '{folderId}' in parents and mimeType != 'application/vnd.google-apps.folder'",
                QueryType.Folder => $"trashed = false and '{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
                _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null)
            };
            FileQuery query = new()
            {
                QueryString = queryString,
                IncludeItemsFromAllDrives = true,
                SupportsAllDrives = true,
                SearchInDrive = true,
                SharedDriveId = driveId,
                PageSize = 100,
                PageToken = pageToken,
                IncludeLabels = _labelIds,
            };
            (files, pageToken) = await _driveApi.ListFilesAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get files by drive id {driveId} failed, Message: {ex}");
            throw;
        }
        return (files, pageToken);
    }
    public async Task<List<File>> PageMyDriveFoldersAsync(string folderId, string? pageToken = null)
    {
        List<File> files = [];
        try
        {
            do
            {
                FileQuery query = new()
                {
                    QueryString = $"trashed = false and '{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
                    SearchInDrive = true,
                    SupportsAllDrives = false,
                    IncludeItemsFromAllDrives = false,
                    PageSize = 100,
                    OrderBy = "name_natural",
                    PageToken = pageToken,
                };
                (var resultFiles, pageToken) = await _driveApi.ListFilesAsync(query);
                files.AddRange(resultFiles);
            } while (pageToken.IsNotNullOrEmpty());

        }
        catch (Exception ex)
        {
            logger.Error($"Get files failed, Message: {ex}");
            throw;
        }
        return (files);
    }

    public async Task<(List<File> files, string? pageToken)> PageFilesByDriveIdAsync(string driveId, string folderId, string? pageToken = null)
    {
        List<File> files = [];
        try
        {
            FileQuery query = new()
            {
                QueryString = $"trashed = false and '{folderId}' in parents",
                IncludeItemsFromAllDrives = true,
                SupportsAllDrives = true,
                SearchInDrive = true,
                SharedDriveId = driveId,
                PageSize = 100,
                PageToken = pageToken,
                IncludeLabels = _labelIds,
            };
            (files, pageToken) = await _driveApi.ListFilesAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get files by drive id {driveId} failed, Message: {ex}");
            throw;
        }
        return (files, pageToken);
    }

    public async Task<List<File>> PageFoldersByDriveIdAsync(string driveId, string folderId, string? pageToken = null)
    {
        List<File> files = [];
        try
        {
            do
            {
                FileQuery query = new()
                {
                    QueryString =
                        $"trashed = false and '{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
                    IncludeItemsFromAllDrives = true,
                    SupportsAllDrives = true,
                    SearchInDrive = true,
                    SharedDriveId = driveId,
                    PageSize = 100,
                    OrderBy = "name_natural",
                    PageToken = pageToken,
                };
                (var resultFiles, pageToken) = await _driveApi.ListFilesAsync(query);
                files.AddRange(resultFiles);
            } while (pageToken.IsNotNullOrEmpty());

        }
        catch (Exception ex)
        {
            logger.Error($"Get files by drive id {driveId} failed, Message: {ex}");
            throw;
        }
        return (files);
    }
    public async Task<File> PageFoldersByNameAsync(string parentId, string name, string? pageToken = null)
    {
        try
        {
            do
            {
                FileQuery query = new()
                {
                    QueryString = $"trashed = false and '{parentId}' in parents and name ='{EscapeString(name)}' and mimeType = 'application/vnd.google-apps.folder'",
                    IncludeItemsFromAllDrives = true,
                    SupportsAllDrives = true,
                    //SearchInDrive = true,
                    PageSize = 100,
                    OrderBy = "name_natural",
                    PageToken = pageToken,
                };
                (var resultFolders, pageToken) = await _driveApi.ListFilesAsync(query);

                if(resultFolders.IsNotNullOrEmpty()) return resultFolders.First();

            } while (pageToken.IsNotNullOrEmpty());

        }
        catch (Exception ex)
        {
            logger.Error($"Get files by parent id {parentId} failed,name:{name} Message: {ex}");
        }
        return null;
    }
    public async Task<File> PageFoldersByIdAsync(string parentId, string id, string? pageToken = null)
    {
        try
        {
            do
            {
                FileQuery query = new()
                {
                    QueryString = $"trashed = false and '{parentId}' in parents and mimeType = 'application/vnd.google-apps.folder'",
                    IncludeItemsFromAllDrives = true,
                    SupportsAllDrives = true,
                    PageSize = 100,
                    OrderBy = "name_natural",
                    PageToken = pageToken,
                };
                (var folders, pageToken) = await _driveApi.ListFilesAsync(query);

                var folder = folders?.FirstOrDefault(f => f.Id == id);
                if (folder != null)
                {
                    return folder;
                }

            } while (pageToken.IsNotNullOrEmpty());

        }
        catch (Exception ex)
        {
            logger.Error($"Not found folder by parent id {parentId} failed,id:{id} Message: {ex}");
        }
        return null;
    }
    protected string EscapeString(string name)
    {
        string result = name;
        if (name.Contains('\\'))
        {
            result = result.Replace("\\", "\\\\");
        }
        if (name.Contains('\''))
        {
            result = result.Replace("\'", "\\'");
        }

        return result;
    }
    public async Task<(List<File> files, string? pageToken)> PageFilesByFolderIdAsync(string folderId, string? pageToken)
    {
        List<File> files = [];
        try
        {
            FileQuery query = new()
            {
                QueryString = $"trashed = false and '{folderId}' in parents",
                IncludeItemsFromAllDrives = true,
                SupportsAllDrives = true,
                PageToken = pageToken,
                IncludeLabels = _labelIds,
            };
            (files, pageToken) = await _driveApi.ListFilesAsync(query);
        }
        catch (Exception ex)
        {
            logger.Error($"Get files by folder id {folderId} failed, Message: {ex}");
            throw;
        }
        return (files, pageToken);
    }

    public async Task<File> GetFileByIdAsync(string id, string labelId = "")
    {
        try
        {
            var file = await _driveApi.GetFileAsync(id, labelId);
            return file;
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Warn($"Can not found item. Item can be deleted. Item id: {id}");
                return null;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Get files by id {id} failed, Message: {ex}");
            throw;
        }
    }

    //public async Task<File> GetFileBasicInfoByIdAsync(string id)
    //{
    //    try
    //    {
    //        var file = await _driveApi.GetFileBasicInfoAsync(id);
    //        return file;
    //    }
    //    catch (GoogleApiException ex)
    //    {
    //        if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
    //        {
    //            logger.Warn($"Can not found item. Item id: {id}");
    //            return null;
    //        }
    //        throw;
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.Error($"Get files by id {id} failed, Message: {ex}");
    //        throw;
    //    }
    //}

    public async Task DownloadFileAsync(string id, string path)
    {
        try
        {
            await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.DownloadFileAsync(id, path));
            logger.Info("Download file by id '{0}' to '{1}' successfully.", id, path);
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Warn($"Can not found item. Item can be deleted. Item id: {id}");
                return;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Download file by id {id} failed, Message: {ex}");
            throw;
        }
    }

    public async Task DownloadMediaAsync(string id, string mimeType, string path)
    {
        try
        {
            await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.DownloadMediaAsync(id, mimeType, path));
            logger.Info("Download file by id '{0}' to '{1}' successfully.", id, path);
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Warn($"Can not found item. Item can be deleted. Item id: {id}");
                return;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Download media by id {id} failed, Message: {ex}");
            throw;
        }
    }

    public async Task ExportFileAsync(string id, string path, string mimeType)
    {
        try
        {
            await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.ExportFileAsync(id, path, mimeType));
            logger.Info("Export file by id '{0}' to '{1}' successfully.", id, path);
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Warn($"Can not found item. Item can be deleted. Item id: {id}");
                return;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Export file by id {id} failed, Message: {ex}");
            throw;
        }
    }

    public async Task ExportBigFileAsync(string id, string path, string mimeType)
    {
        try
        {
            await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.ExportBigFileAsync(id, path, mimeType));
            logger.Info("Export file by id '{0}' to '{1}' successfully.", id, path);
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Warn($"Can not found item. Item can be deleted. Item id: {id}");
                return;
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Export file by id {id} failed, Message: {ex}");
            throw;
        }
    }

    public async Task<File> CreateNewFolderAsync(GDFileBasic basicInfo)
    {
        try
        {
            var newFolder = new File
            {
                Name = basicInfo.Name,
                MimeType = basicInfo.MimeType,
                FolderColorRgb = basicInfo.ColorRgb,
                Starred = basicInfo.Starred,
            };

            newFolder.Parents = [basicInfo.ParentId];
            

            if (!string.IsNullOrEmpty(basicInfo.CreatedBy))
            {
                newFolder.Owners = [new User() { DisplayName = basicInfo.CreatedBy, }];
            }
            var file = await _driveApi.CreateFolderAsync(newFolder);
            logger.Info($"Created restore folder {basicInfo.DocId} successfully.");
            return file;
        }
        catch (Exception ex)
        {
            logger.Error($"Created restore folder {basicInfo.DocId} failed, Message: {ex}");
            throw;
        }
    }

    public async Task UpdateFolderAsync(File folderInfo, string docId)
    {
        try
        {
            await _driveApi.UpdateFolderAsync(folderInfo, docId);
            logger.Info($"Updated restore folder {folderInfo.Name} successfully.");
        }
        catch (Exception ex)
        {
            logger.Error($"Created restore folder {folderInfo.Name} failed, Message: {ex}");
            throw;
        }
    }

    public async Task<File?> UploadFileAsync(GoogleItemData item, string parentId, string path)
    {
        try
        {
            File uploadFile = new()
            {
                Name = item.Name,
                // MimeType = item.MimeType,
                Parents = [parentId],
            };
            uploadFile = await _driveApi.CreateFileAsync(uploadFile, path);
            return uploadFile;
        }
        catch (Exception ex)
        {
            logger.Error($"Upload file {item.Name} failed, Message: {ex}");
            throw;
        }
    }

    public async Task<bool> TryDeleteItemById(string id)
    {
        try
        {
            return await _driveApi.DeleteFileAsync(id);
        }
        catch (GoogleApiException gex)
        {
            if (gex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                logger.Error($"User does not have sufficient permission to delete item by id: {id}");
                throw new Exception(I18NResource.InvalidUserPermission);
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Delete files by id {id} failed, Message: {ex}");
            throw;
        }
    }

    public async Task<List<Label>> GetLabelsAppliedOnFileAsync(string id)
    {
        try
        {
            var labels = await _driveApi.ListFileLabelsAsync(id);
            return labels;
        }
        catch (Exception ex)
        {
            logger.Error($"Get label applied on file by file id failed, Message: {ex}");
            throw;
        }
    }
    public async Task<List<string>> GetLabelsIdOnFileAsync(string id)
    {
        try
        {
            var labels = await GetLabelsAppliedOnFileAsync(id);
            if (labels.IsNotNullOrEmpty())
            {
                return labels.Select(x => x.Id).ToList();
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Get label applied on file by file id failed, Message: {ex}");
            throw;
        }
        return null;
    }
    public async Task<bool> AppliedLabelOnFileAsync(string labelId, string fileId)
    {
        try
        {
            var labels = await _driveApi.AppliedLabelOnFileAsync(labelId, fileId);
            if (labels.Count > 0)
            {
                return true;
            }
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception(I18NResource.LabelNoPermission);
            }
            else if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (ex.Message.Contains("The Label cannot be applied because the Label limit on File"))
                {
                    throw new Exception("AppliedLabelReachoutLimit");
                }
                throw new Exception(I18NResource.LabelNoPermission);
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Applied label {labelId} on file {fileId} failed, Message: {ex}");
            throw;
        }
        return false;
    }

    public async Task<(bool IsTrashed, bool IsLocked)> GetFileStatusAsync(string fileId)
    {
        try
        {
            var file = await _driveApi.GetFileAsync(fileId);
            bool isTrashed = file?.Trashed ?? false;
            bool isLocked = !file?.Capabilities.CanRename ?? false;
            return (isTrashed, isLocked);
        }
        catch (GoogleApiException ex)
        {
            logger.Error($"Failed to get file status for {fileId}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> BatchRemoveLabelsOnFileAsync(List<string> labelIds, string fileId)
    {
        try
        {
            var labels = await _driveApi.BatchRemoveLabelOnFileAsync(labelIds, fileId);
            if (labels.Count > 0)
            {
                return true;
            }
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception(I18NResource.LabelNoPermission);
            }
            else if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception(I18NResource.LabelInvalidOverwritePermissionException);
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Batch remove labels on file {fileId} failed, Message: {ex}");
            throw;
        }
        return false;
    }

    public async Task<List<Revision>> GetAllFileVersionsAsync(string docId)
    {
        try
        {
            var versions = await _driveApi.GetAllFileVersionsAsync(docId);
            return versions;
        }
        catch (Exception ex)
        {
            logger.Error($"Get version list on file by file id {docId} failed, Message: {ex}");
            return [];
        }
    }
    public async Task<bool> UpdateFileVersion(string docId, string revisionId, Revision body)
    {
        try
        {
            var newRivision = new Revision
            {
                KeepForever = body.KeepForever,
            };
            var versions = await _driveApi.UpdateFileVersion(docId, revisionId, newRivision);
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"Get version list on file by file id {docId} failed, Message: {ex}");
        }
        return false;
    }
    public async Task<Permission> UpdatePermissionAsync(Permission body, string fileId, string permissionId, FileQuery query = null)
    {
        try
        {
            var permission = await _driveApi.UpdatePermissionAsync(body, fileId, permissionId, query);
            return permission;
        }
        catch (Exception ex)
        {
            logger.Error($"Updated permissions {fileId} failed, Message: {ex}");
            return null;
        }
    }

    public async Task ExportFileToStreamAsync(string fileId, Stream outputStream, string mimeType)
    {
        try
        {
            if (string.IsNullOrEmpty(fileId) || outputStream == null || string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentException("Invalid arguments provided.");
            }

            await _driveApi.ExportFileToStreamAsync(fileId, outputStream, mimeType);

            logger.Info($"Successfully exported file {fileId} as {mimeType}.");
        }
        catch (GoogleApiException ex)
        {
            logger.Error($"Failed to export file {fileId}. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error in ExportFileToStreamAsync: {ex.Message}");
            throw;
        }
    }

    public async Task DownloadFileToStreamAsync(string fileId, Stream outputStream)
    {
        try
        {
            if (string.IsNullOrEmpty(fileId) || outputStream == null)
            {
                throw new ArgumentException("Invalid arguments provided.");
            }

            await _driveApi.DownloadFileToStreamAsync(fileId, outputStream);

            logger.Info($"Successfully downloaded file {fileId} to stream.");
        }
        catch (GoogleApiException ex)
        {
            logger.Error($"Failed to download file {fileId}. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error in DownloadFileToStreamAsync: {ex.Message}");
            throw;
        }
    }

    public async Task DownloadFileVersionToStreamAsync(GoogleItemData file, string revisionId, string localPath, long fileSize)
    {
        try
        {
            if (string.IsNullOrEmpty(file.Id) || string.IsNullOrEmpty(revisionId) || localPath == null)
            {
                throw new ArgumentException("Invalid arguments provided.");
            }
            if (GoogleConstant.GoogleExportMimeType.TryGetValue(file.MimeType, out var exportMimeType) || fileSize < GoogleConstant.DRIVE_FILE_SIZE_100MB)
            {
                await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.DownloadFileVersionToStreamAsync(file, revisionId, localPath));
            }
            else
            {
                await GoogleRequestExtension.ExecuteWithRetryAsync(() => _driveApi.DownloadBigFileVersionToStreamAsync(file.Id, revisionId, localPath, fileSize));
            }

            logger.Info($"Successfully downloaded version of file with {file.Id} to stream.");
        }
        catch (GoogleApiException ex)
        {
            logger.Error($"Failed to download version {revisionId}. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error in DownloadVersionToStreamAsync: {ex.Message}");
            throw;
        }
    }
    public async Task<File> CreateFileAsync(File file)
    {
        try
        {
            return await _driveApi.CreateFileAsync(file);
        }
        catch (Exception ex)
        {
            logger.Error($"Create file failed, Message: {ex}");
            throw;
        }
    }
    public async Task<File> UploadFileAsync(File drive, string existFileId, string mineType, bool retoreVersion, Stream stream)
    {
        //int retryTimes = 0;
        //do
        //{
        //    try
        //    {
                return await _driveApi.UploadFileAsync(drive, existFileId, mineType, retoreVersion, stream);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Upload File failed, retry times:{retryTimes}, Message: {ex}");
        //        if (retryTimes >= 2)
        //        {
        //            logger.Error($"Upload File failed after {retryTimes + 1} attempts, giving up.");
        //            throw;
        //        }
        //    }
        //    finally
        //    {
        //        retryTimes++;
        //    }
        //}
        //while (retryTimes < 3);
        //throw new Exception("Upload file failed.");
    } 
    public void SetIncludeLabels(string labelIds)
    {
        _labelIds = labelIds;
    }
    #endregion
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _driveApi?.Dispose();
        _driveApi = null;
    }

    public async Task<string> DownloadGoogleFileToMemoryStreamAsync(string fileId, Stream outputStream, string? mimeType = null)
    {
        string exportFileExtension;
        try
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                // If mimeType is not provided, get the file metadata to determine the mimeType
                var file = await _driveApi.GetFileAsync(fileId);
                mimeType = file.MimeType;
            }
            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentException("The mimeType cannot be determined for the file.");
            }

            string exportMimeType;
            // Google Docs to PDF
            if (mimeType == "application/vnd.google-apps.document")
            {
                exportMimeType = "application/pdf";
                exportFileExtension = "pdf";
            }
            // Google Sheets (Excel)
            else if (mimeType == "application/vnd.google-apps.spreadsheet")
            {
                // Export as Excel file
                exportMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                exportFileExtension = "xlsx";
            }
            // Google Slides to PPTX
            else if (mimeType == "application/vnd.google-apps.presentation")
            {
                exportMimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                exportFileExtension = "pptx";
            }
            else
            {
                throw new InvalidOperationException("The file is not a supported Google Doc type.");
            }
            await _driveApi.ExportFileToStreamAsync(fileId, outputStream, exportMimeType);
        }
        catch (GoogleApiException ex)
        {
            logger.Error($"Failed to download Google Doc to stream. Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error in DownloadGoogleDocToStreamAsync: {ex.Message}");
            throw;
        }
        return exportFileExtension;
    }
    public async Task<List<File>> ListFileRestoreAsync(FileQuery query, string? pageToken = null)
    {
        var result = new List<File>();
        try
        {
            do
            {
                (var files, pageToken) = await _driveApi.ListFilesAsync(query);
                foreach (var file in files)
                {
                    if (GoogleConstant.UnsupportedRestoreMimeType.Contains(file.MimeType) || file.MimeType.Contains("application/vnd.google-apps.drive-sdk"))
                    {
                        continue;
                    }
                    result.Add(file);
                }
            } while (pageToken.IsNotNullOrEmpty());

        }
        catch (Exception ex)
        {
            logger.Error($"Get files to restore fail, Message: {ex}");
            throw;
        }
        return result;
    }
    public async Task<bool> RestoreFileLabels(Archive.Wrapper.GDFileBasic file, List<GoogleAppsDriveLabelsV2Label> currentLabels, FileQuery query = null)
    {
        bool isSuccess = true;
        try
        {
            logger.Info($"Start to restore labels for file {file.DocId}.");
            if (file.Labels != null && file.Labels.Any())
            {
                logger.Info($"Label count: {file.Labels.Count}");
                if (currentLabels == null || currentLabels.Count == 0)
                {
                    logger.Info("No label was obtained.");
                    isSuccess = false;
                    return isSuccess;
                }
                ModifyLabelsRequest modifyLabelsRequest = new ModifyLabelsRequest();
                modifyLabelsRequest.LabelModifications = new List<LabelModification>();
                foreach (var oldLabel in file.Labels)
                {
                    var currentLabel = currentLabels.Find(i => i.Id.Equals(oldLabel.Id));
                    if (currentLabel == null)
                    {
                        logger.Info($"The label does not exist. Skip to restore. Label id: {oldLabel}");
                        isSuccess = false;
                        continue;
                    }
                    if (!(currentLabel.Lifecycle.State.Equals("PUBLISHED")))
                    {
                        isSuccess = false;
                        continue;
                    }
                    var labelModification = CreateLabelModificationByDto(oldLabel, currentLabel);
                    modifyLabelsRequest.LabelModifications.Add(labelModification);
                }
                try
                {
                    await _driveApi.RestoreFileLabelAsync(modifyLabelsRequest, file.DocId);
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to restore file. Try restoring each label individually. Error: {e}");
                    isSuccess = false;
                    foreach (var tempLabelModification in modifyLabelsRequest.LabelModifications)
                    {
                        await RestoreFileLabelAsync(file.DocId, tempLabelModification);
                    }
                }
            }
            logger.Info($"Modify labels successfully for file {file.DocId}.");
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to modify labels. Error: {ex}");
            isSuccess = false;
        }
        return isSuccess;
    }
    public async Task RestoreFileLabelAsync(string fileId, LabelModification labelModification)
    {
        try
        {
            ModifyLabelsRequest modifyLabelsRequest = new ModifyLabelsRequest();
            modifyLabelsRequest.LabelModifications = new List<LabelModification>();
            modifyLabelsRequest.LabelModifications.Add(labelModification);
            await _driveApi.RestoreFileLabelAsync(modifyLabelsRequest, fileId);
        }
        catch (Exception e)
        {
            logger.Error($"Failed to associate label. The current tag is deleted or unavailable. LabelId:{labelModification.LabelId} Error: {e}");
        }
    }
    private LabelModification CreateLabelModificationByDto(LabelData oldLabel, GoogleAppsDriveLabelsV2Label currentLabel)
    {
        LabelModification labelModification = new LabelModification()
        {
            Kind = oldLabel.Kind,
            LabelId = oldLabel.Id,
        };
        if (currentLabel.Fields == null)
        {
            logger.Info($"The label does not get a field. label id: {oldLabel.Id}");
        }
        else
        {
            if (oldLabel.Fields != null)
            {
                labelModification.FieldModifications = new List<LabelFieldModification>();
                foreach (var oldField in oldLabel.Fields)
                {
                    var currentField = currentLabel.Fields.ToList().Find(i => i.Id.Equals(oldField.Key));
                    if (currentField == null)
                    {
                        continue;
                    }
                    if (!(currentField.Lifecycle.State.Equals("PUBLISHED")))
                    {
                        continue;
                    }
                    LabelFieldModification field = new LabelFieldModification()
                    {
                        FieldId = oldField.Key,
                        Kind = oldField.Value.Kind,
                    };
                    if (oldField.Value.DateString != null && currentField.DateOptions != null && oldField.Value.ValueType.Equals("dateString"))
                    {
                        field.SetDateValues = oldField.Value.DateString;
                    }
                    else if (oldField.Value.Text != null && currentField.TextOptions != null && oldField.Value.ValueType.Equals("text"))
                    {
                        field.SetTextValues = oldField.Value.Text;
                    }
                    else if (oldField.Value.Integer != null && currentField.IntegerOptions != null && oldField.Value.ValueType.Equals("integer"))
                    {
                        field.SetIntegerValues = oldField.Value.Integer;
                    }
                    else if (oldField.Value.User != null && currentField.UserOptions != null && oldField.Value.ValueType.Equals("user"))
                    {
                        field.SetUserValues = oldField.Value.User.Select(u => u.OwnerEmail).ToList();
                    }
                    else if (oldField.Value.Selection != null && currentField.SelectionOptions != null && oldField.Value.ValueType.Equals("selection"))
                    {
                        if (currentField.SelectionOptions.Choices == null)
                        {
                            //The options list does not get a choice
                        }
                        else
                        {
                            var currentChoices = currentField.SelectionOptions.Choices.ToList();
                            if (currentChoices != null && currentChoices.Any())
                            {
                                List<string> choices = new List<string>();
                                if (oldField.Value.Selection != null && oldField.Value.Selection.Any())
                                {
                                    foreach (var choiceId in oldField.Value.Selection)
                                    {
                                        var choice = currentChoices.Find(i => i.Id.Equals(choiceId));
                                        if (choice == null)
                                        {
                                            continue;
                                        }
                                        if (!(choice.Lifecycle.State.Equals("PUBLISHED")))
                                        {
                                            continue;
                                        }
                                        choices.Add(choiceId);
                                    }
                                }
                                field.SetSelectionValues = choices;
                            }
                        }
                    }
                    else
                    {
                        field.UnsetValues = true;
                    }
                    labelModification.FieldModifications.Add(field);
                }
            }
        }
        return labelModification;
    }
    private Archive.Wrapper.GDFileBasic ConvertFile2FileBasic(File file)
    {
        var dto = new Archive.Wrapper.GDFileBasic()
        {
            DocId = file.Id,
            Name = file.Name,
            MimeType = file.MimeType,
            ModifiedTime = file.ModifiedTimeDateTimeOffset?.Ticks ?? 0,
            CreatedTime = file.CreatedTimeDateTimeOffset?.Ticks ?? 0,
            ParentId = file.Parents.IsNotNullOrEmpty<string>() ? file.Parents[0] : string.Empty,
            Description = file.Description,
            Starred = file.Starred,
            Size = Convert.ToInt64(file.Size),
        };
        if (file.Owners != null && file.Owners.Count > 0)
        {
            dto.Owners = new UserData
            {
                OwnerDisplayName = file.Owners[0].DisplayName,
                OwnerEmail = file.Owners[0].EmailAddress
            };
        }
        if (file.MimeType.Equals(GoogleConstant.GoogleFolder, StringComparison.InvariantCultureIgnoreCase))
        {
            dto.Type = (int)GDriveDataType.Folder;
        }
        else
        {
            dto.Type = (int)GDriveDataType.File;
        }
        return dto;
    }

    ~GoogleDriveService()
    {
        Dispose(false);
    }
}
