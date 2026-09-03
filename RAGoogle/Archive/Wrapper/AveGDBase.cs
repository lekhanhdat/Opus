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
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Contract.Aos;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Report;
using RAGoogle.Restore.Common;
using RAGoogle.Restore.Report;
using RAGoogle.Services;
using System.Collections.Concurrent;
using RAGoogle.Util;
using System.Reflection;
using AvePoint.Wrapper.Common;
using StringExtension = AvePoint.Wrapper.Common.StringExtension;
using DocumentFormat.OpenXml.Wordprocessing;

namespace RAGoogle.Archive.Wrapper
{
    public class AveGDBase
    {
        public readonly AveLogger _logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public AveGDBase(RMAosGoogleAppProfile googleAppProfile, GoogleDriveData driveInfo, GoogleActionType jobType)
        {
            _appProfile = googleAppProfile;
            _driveNodeInfo = driveInfo;
            _actionType = jobType;
        }
        public AveGDBase(AveGDrive gDrive)
        {
            _appProfile = gDrive.AppProfile;
            _driveNodeInfo = gDrive.DriveNodeInfo;
            _reportCenter = gDrive.ReportCenter;
            _gDrive = gDrive;
            _actionType = gDrive._actionType;
        }
        public AveGDBase(AveGDFolder gFolder)
        {
            _appProfile = gFolder.AppProfile;
            _driveNodeInfo = gFolder.DriveNodeInfo;
            _reportCenter = gFolder.ReportCenter;
            _gDrive =  gFolder.ParentDrive;
            _gFolder = gFolder;
            _actionType =  gFolder.ParentDrive._actionType;
        }
        public AveGDBase()
        {
            // Default constructor for serialization or other purposes
        }
        protected AveGDrive _gDrive { get; set; }
        public AveGDrive ParentDrive
        {
            get
            {
                if (_gDrive == null)
                {
                    throw new ArgumentNullException(nameof(_gDrive), "Parent drive is not initialized.");
                }
                return _gDrive;
            }
        }
        protected AveGDFolder _gFolder { get; set; }
        public AveGDFolder ParentFolder
        {
            get
            {
                if (_gFolder == null)
                {
                    throw new ArgumentNullException(nameof(_gFolder), "Parent folder is not initialized.");
                }
                return _gFolder;
            }
        }
        protected bool _isRootFolder { get; set; } = false;
        protected bool _isNewCreated { get; set; } = false;
        protected virtual string Id { get; set; } = string.Empty;
        protected string _newDriveId { get; set; } = string.Empty;
        protected GoogleDriveData _driveNodeInfo { get; set; }
        public GoogleDriveData DriveNodeInfo { 
            get
            {
                return _driveNodeInfo;
            }
            
        }
        protected RMAosGoogleAppProfile _appProfile { get; set; }
        public RMAosGoogleAppProfile AppProfile
        {
            get
            {
                if (_appProfile == null)
                {
                    throw new ArgumentNullException(nameof(_appProfile), "App profile is not initialized.");
                }
                return _appProfile;
            }
        }
        protected GoogleDriveService _driveService { get; set; }
        protected GoogleLabelService _labelService { get; set; }
        protected GoogleDirectoryService _directoryService { get; set; }
        protected GoogleDriveService _rootDriveService { get; set; }
        public GoogleDriveService RootDriveSerivce
        {
            get
            {
                if(_rootDriveService == null)
                {
                    _rootDriveService = new GoogleDriveService(_appProfile);
                }
                return _rootDriveService;
            }
        }
        protected ReportCenter _reportCenter { get; set; }
        public ReportCenter ReportCenter
        {
            get
            {
                return _reportCenter;
            }
            set
            {
                _reportCenter = value;
            }
        }
        protected AveRestoreReportDto _aveRestoreReportDto { get; set; }
        public AveRestoreReportDto AveRestoreReportDto
        {
            get
            {
                return _aveRestoreReportDto;
            }
            set
            {
                _aveRestoreReportDto = value;
            }
        }
        protected List<string> _domains { get; set; }
        protected GoogleDriveBasic _myDrive;
        public List<string> Domains
        {
            get
            {
                if (_domains.IsNullOrEmpty())
                {
                    _domains = new List<string>();
                }
                return _domains;
            }
        }
        public GoogleDriveService DriveService
        {
            get
            {
                if (_driveService == null)
                {
                    _driveService = InitDriveService(_driveNodeInfo.DriveName).GetAwaiter().GetResult();
                }
                return _driveService;
            }
        }
        protected string _currentFileOwner { get; set; }
        public GoogleLabelService LabelService
        {
            get
            {
                if (_labelService == null)
                {
                    _labelService = new(_appProfile);
                }
                return _labelService;
            }
        }

        public DriveType DriveType
        {
            get
            {
                if (_driveNodeInfo.Type == DriveType.SharedDrive)
                {
                    return DriveType.SharedDrive;
                }
                else if (_driveNodeInfo.Type == DriveType.MyDrive)
                {
                    return DriveType.MyDrive;
                }
                else
                {
                    throw new ArgumentException($"Unknown drive type: {_driveNodeInfo.Type}");
                }
            }
        }
        protected GoogleActionType _actionType { get; set; }
        public ConcurrentDictionary<string, string> ObjectIdMappings
        {
            get
            {
                return GlobalCache.Instance.ObjectIdMappings;
            }
        }
        public ConcurrentDictionary<string, string> ObjectIdAndNameMappings
        {
            get
            {
                return GlobalCache.Instance.ObjectIdAndNameMappings;
            }
        }
        protected ConflictResolutionType _conflictResolutionType = ConflictResolutionType.Skip;
        public ConflictResolutionType ConflictResolution
        {
            get
            {
                return _conflictResolutionType;
            }
            set
            {
                _conflictResolutionType = value;
            }
        }
        protected bool _restoreSharedLinks { get; set; }
        public bool RestoreSharedLinks
        {
            get
            {
                return _restoreSharedLinks;
            }
            set
            {
                _restoreSharedLinks = value;
            }
        }
        public string ServiceAdminUser { get; set; }
        public async Task<GoogleDriveService> InitDriveService(string driveName)
        {
            GoogleDriveService googleDriveService = null;
            using (GoogleDriveService service = new GoogleDriveService(_appProfile))
            using (_directoryService = new(_appProfile))
            {
                await InitDomainsAsync();
                if (_driveNodeInfo.Type == DriveType.SharedDrive)
                {
                    List<Permission> members = await service.GetPermissionsByIdAsync(driveName, true);
                    googleDriveService = await GetServiceWithDelegateMemberAsync(members, _directoryService);
                    ServiceAdminUser = googleDriveService.DelegateUser;
                }
                else
                {
                    ServiceAdminUser = driveName;
                    googleDriveService = new GoogleDriveService(_appProfile, driveName);
                }
            }
            return googleDriveService;
        }
        
        public async Task InitDomainsAsync()
        {
            if (_domains.IsNotEmptyCollection())
            {
                return;
            }
            var domains = await _directoryService.GetAllDomainsAsync();
            string primaryDomain = domains?.Find(domain => domain.IsPrimary ?? false)?.DomainName ?? _appProfile.DomainName;
            _domains = [primaryDomain, .. domains?.Select(domain => domain.DomainName) ?? []];
        }
        
        protected async Task<GoogleDriveService> GetServiceWithDelegateMemberAsync(List<Permission> members, GoogleDirectoryService directoryService)
        {
            GoogleDriveService result = null;
            //get file owner
            if (_actionType == GoogleActionType.Restore)
            {
                if (_currentFileOwner.IsNotNullOrEmpty())
                {
                    _logger.Debug($"Begin to find file owner.");
                    var matchFileOwner = members?.FirstOrDefault(m => m.Type is "user" && m.EmailAddress.EqualIgnoreCase(_currentFileOwner));
                    if (matchFileOwner != null)
                    {
                        result = TryGetSerivce(matchFileOwner.EmailAddress, matchFileOwner.Id);
                    }
                }
                if (result == null && _appProfile != null)
                {
                    _logger.Debug($"Begin to find app service account.");
                    var matchFileOwner = members?.FirstOrDefault(m => m.Type is "user" && m.EmailAddress.EqualIgnoreCase(_appProfile.UserName));

                    if (matchFileOwner != null)
                    {
                        result = TryGetSerivce(matchFileOwner.EmailAddress, matchFileOwner.Id);
                    }
                    else
                    {
                        _logger.Debug($"Not find app service account  in the drive, it will be added.");
                    }
                    if (result == null)
                    {
                        await RootDriveSerivce.CreatePermissionAsync(new Permission()
                        {
                            Type = "user",
                            Role = "writer",
                            EmailAddress = _appProfile.UserName
                        }, _driveNodeInfo.DriveName, true);
                        _logger.Debug($"success to add app service account in the drive");
                        result = TryGetSerivce(_appProfile.UserName);
                    }
                }

            }
            else if (_actionType == GoogleActionType.Backup)
            {
                if (result == null)
                {
                    _logger.Debug($"Begin to find file owner by roles.");
                    var roleTypes = new List<RoleType>() { RoleType.owner, RoleType.organizer, RoleType.fileOrganizer, RoleType.writer, RoleType.commenter, RoleType.reader };
                    foreach (var role in roleTypes)
                    {
                        _logger.Debug($"Query user with role {role}.");
                        var allMatchMembers = members.Where(m => m.Type is "user" && m.Role.EqualIgnoreCase(role.ToString()) && !m.EmailAddress.IsExternalUser(_domains)).ToList();

                        foreach (var member in allMatchMembers)
                        {
                            result = TryGetSerivce(member.EmailAddress, member.Id);
                            if (result != null)
                            {
                                break;
                            }
                        }
                        if (result != null)
                        {
                            break;
                        }
                    }
                }

                if (result == null)
                {
                    _logger.Debug($"not found delegate user with enough permission. will get user from the group");
                    if (members.Any(m => m.Type is "group"))
                    {
                        foreach (var group in members.Where(m => m.Type is "group"))
                        {
                            string internalGroup = group.EmailAddress;
                            if (internalGroup.IsNotNullOrEmpty())
                            {
                                var membersOfGroup = await directoryService.GetGroupAllUsersAsync(internalGroup);
                                foreach (var member in membersOfGroup)
                                {
                                    result = TryGetSerivce(member.Email, member.Id);
                                    if (result != null)
                                    {
                                        break;
                                    }
                                }
                                if (result != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (result != null) return result;
            throw new Exception("not found delegate user.");
        }
        private GoogleDriveService TryGetSerivce(string userEmail, string userId = "")
        {
            try
            {
                _logger.Debug($"Try get drive service with user {userId}.");
                if (userEmail.IsNullOrEmpty())
                {
                    _logger.Warn($"Skip empty email address.");
                    return null;
                }
                var service = new GoogleDriveService(_appProfile, userEmail);
                _logger.Debug($"try to get drive permission.");
                service.GetPermissionsByIdAsync(_driveNodeInfo.Id, true).GetAwaiter().GetResult();
                _logger.Debug($"try to get drive permission sucessful.");
                return service;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to get drive permission with member {userId}, exception: {ex.Message}");
            }
            return null;
        }

        public bool IsNeedCreate(string oldId)
        {
            if (ObjectIdMappings.TryGetValue(oldId, out var value))
            {
                return value != oldId;
            }
            return true;
        }
        protected async Task<File> GetCurrentItem(GDFileBasic dto, bool isAppend)
        {
            FileQuery query2 = new FileQuery()
            {
                QueryString = dto.ParentId == null ? $"'{ParentDrive.DriveProxy.Id}' in parents and name ='{EscapeString(dto.Name)}'" : $"'{dto.ParentId}' in parents and name ='{EscapeString(dto.Name)}'",
                SupportsAllDrives = true,
                IncludeItemsFromAllDrives = true
            };
            query2.QueryString += " and trashed = false";
            var result = await DriveService.ListFileRestoreAsync(query2);
            if (result.Count > 0)
            {
                if (isAppend)
                {
                    return result.Find(f => f.MimeType == dto.MimeType && f.Name.Equals(dto.Name));
                }
                else
                {
                    return result.Find(f => f.Id == dto.DocId);
                }
            }
            return null;
        }
        protected Dictionary<string, object> ConvertObjToDictionary(object obj)
        {
            if (obj == null) return new Dictionary<string, object>();
            return obj.GetType()
                      .GetProperties()
                      .Select(prop => new { prop.Name, Value = prop.GetValue(obj) })
                      .Where(x => x.Value != null)
                      .ToDictionary(
                          x => x.Name,
                          x => x.Value ?? new()
                      );
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
        public virtual async Task HandleRestoreObjectPermission(GDPermissionList permissionInfo)
        {
            if (this is AveGDFolder && !_isNewCreated)
            {
                _logger.Warn($"Skip restoring permissions for folder {Id}. new created:{_isNewCreated}");
                return;
            }
            
            var currentPermissions = await DriveService.GetPermissionsByIdAsync(Id, false);

            foreach (var dto in permissionInfo.Permissions)
            {
                try
                {
                    if (dto.Role == "organizer")
                    {
                        _logger.Warn($"Skip restoring 'organizer' role for non-shared drive: {dto.EmailAddress}");
                        continue;
                    }
                    FileQuery query = null;
                    if (!string.IsNullOrEmpty(dto.ExpirationTimeRaw))
                    {
                        if (DateTimeOffset.TryParse(dto.ExpirationTimeRaw, out var expTime))
                        {
                            if (expTime < DateTimeOffset.Now)
                            {
                                _logger.Warn($"The temporary permission has been expired, skip restoring permission {dto.EmailAddress}. Expiration Time: {dto.ExpirationTimeRaw}");
                                continue;
                            }
                        }
                    }

                    var existing = currentPermissions?.FirstOrDefault(x =>
                        string.Equals(x.Id, dto.Id, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.EmailAddress, dto.EmailAddress, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Type, dto.Type, StringComparison.OrdinalIgnoreCase));

                    if (existing == null)
                    {
                        await CreateItemPermisisonAsync(dto, query);
                    }
                    else if (StringExtension.EqualIgnoreCase(dto.Type, "anyone") || StringExtension.EqualIgnoreCase(dto.Type, "domain"))
                    {
                        if (Convert.ToBoolean(existing.AllowFileDiscovery) != Convert.ToBoolean(dto.AllowFileDiscovery) || !StringExtension.EqualIgnoreCase(existing.Role, dto.Role))
                        {
                            await DriveService.DeletePermissionAsync(Id, existing.Id);
                            await CreateItemPermisisonAsync(dto, query);
                        }
                    }
                    else if (!string.Equals(existing.Role, dto.Role, StringComparison.OrdinalIgnoreCase))
                    {
                        var perm = new Permission
                        {
                            ExpirationTimeDateTimeOffset = new DateTimeOffset(new DateTime(dto.ExpirationTime)),
                            Role = dto.Role
                        };
                        if (StringExtension.EqualIgnoreCase(dto.Role, "owner"))
                        {
                            query = new FileQuery()
                            {
                                TransferOwnership = true
                            };
                        }
                        await DriveService.UpdatePermissionAsync(perm, Id, existing.Id, query);
                        _logger.Info($"Updated permission for {dto.EmailAddress}.");
                    }
                    else
                    {
                        _logger.Info($"Permission for {dto.EmailAddress} already up-to-date.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to handle permission {dto.Role} for {dto.EmailAddress}.Error:{ex}");
                }
                finally
                {

                }
            }
        }
        private async Task CreateItemPermisisonAsync(PermissionInfo dto, FileQuery query)
        {
            var perm = CreatePermissionByDto(dto);
            var result = await DriveService.CreatePermissionAsync(perm, Id, false);
            if (dto.ExpirationTime > 0)
            {
                var body = new Permission()
                {
                    ExpirationTimeDateTimeOffset = new DateTimeOffset(new DateTime(dto.ExpirationTime)),
                    Role = dto.Role
                };

                if (StringExtension.EqualIgnoreCase(dto.Role, "owner"))
                {
                    query = new FileQuery()
                    {
                        TransferOwnership = true
                    };
                }
                var updatePer = await DriveService.UpdatePermissionAsync(body, Id, result.Id, query);
                _logger.Info($"Updated permission for {updatePer.EmailAddress} , type {updatePer.Type} , role {updatePer.Role}");
            }

            _logger.Info($"Created permission for {dto.EmailAddress}.");
        }

        private Permission CreatePermissionByDto(PermissionInfo dto)
        {
            return new Permission
            {
                Type = dto.Type,
                Role = dto.Role,
                EmailAddress = dto.EmailAddress,
                AllowFileDiscovery = dto.Type == "group" || dto.Type == "user"? null : dto.AllowFileDiscovery,
                Domain = dto.Domain
            };
        }

    }
}
