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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Aos;
using AvePoint.Wrapper.Common;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Models.GoogleObjectModel;
using RAGoogle.Report;
using RAGoogle.Restore.Common;
using RAGoogle.Restore.Content;
using RAGoogle.Restore.Report;
using System.IO;
using static Google.Apis.Drive.v3.Data.Drive;
using static Google.Apis.Drive.v3.Data.Permission;
using StringExtension = AvePoint.Wrapper.Common.StringExtension;

namespace RAGoogle.Archive.Wrapper
{
    public class AveGDrive : AveGDBase, IAveGDContainer
    {
        private DriveProxy _driveProxy { get; set; }
        private List<DriveMemberProxy> _driveMemberProxies { get; set; }
        //private bool _isNewCreated { get; set; } = false;
        private List<GoogleDriveMember> _driveMembersInfo { get; set; }
        public AveGDrive(RMAosGoogleAppProfile googleAppProfile, GoogleDriveData driveInfo, GoogleActionType action) : base(googleAppProfile, driveInfo, action)
        {
            
        }
        public DriveProxy DriveProxy
        {
            get
            {
                if (_driveProxy == null)
                {
                    throw new ArgumentNullException(nameof(_driveProxy), "Drive proxy is not initialized.");
                }
                return _driveProxy;
            }
        }
        public void ExportBasicInfo(IAveBackupStream output)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDrive.ExportBasicInfo"))
            {
                output.WriteMetadata(AveMetadataType.DriveBasicInfo, GetGoogleDriveBasicInfo());
            }
        }
        public GoogleDriveBasic GetGoogleDriveBasicInfo()
        {
            return new GoogleDriveBasic
            {
                Id = _driveProxy.Id,
                Name = _driveProxy.Name,
                CreatedTime = _driveProxy.CreatedTime,
                ColorRgb = _driveProxy.ColorRgb,
                Hidden = _driveProxy.Hidden,
                Kind = _driveProxy.Kind,
            };
        }
        public void ExportSetting(IAveBackupStream output)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDrive.ExportSetting"))
            {
                output.WriteMetadata(AveMetadataType.DriveSetting, GetGoogleDriveSettingInfo());
            }
        }
        public GoogleDriveSetting GetGoogleDriveSettingInfo()
        {
            return new GoogleDriveSetting
            {
                AdminManagedRestrictions = _driveProxy.Restrictions.AdminManagedRestrictions,
                CopyRequiresWriterPermission = _driveProxy.Restrictions.CopyRequiresWriterPermission ?? false,
                DomainUsersOnly = _driveProxy.Restrictions.DomainUsersOnly ?? false,
                DriveMembersOnly = _driveProxy.Restrictions.DriveMembersOnly ?? false,
                SharingFoldersRequiresOrganizerPermission = _driveProxy.Restrictions.SharingFoldersRequiresOrganizerPermission ?? false
            };

        }

        public void ExportMembers(IAveBackupStream output)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDrive.ExportMembers"))
            {
                output.WriteMetadata(AveMetadataType.DriveMembers, GetDriveMembers());
            }
        }
        public List<GoogleDriveMember> GetDriveMembers()
        {
            return ConvertDriveMemberProxyToGoogleDriveMember(_driveMemberProxies);
        }

        public async Task<(DriveProxy DriveInfo, List<DriveMemberProxy> DrivePermission)> BackupDriveAndDriveMember(GoogleDriveTreeNodeDto selectedNode)
        {
            using (PerformanceScope pc = new PerformanceScope("AveGDrive.BackupDriveAndDriveMember"))
            {
                string driveId = DriveType == DriveType.SharedDrive
                                ? selectedNode.ObjectId
                                : selectedNode.FullPath;

                object drive = DriveType == DriveType.SharedDrive
                    ? await DriveService.GetDriveAsync(driveId)
                    : await DriveService.GetMyDriveAsync(selectedNode.DisplayName);

                var permissions = DriveType == DriveType.SharedDrive
                    ? await DriveService.GetPermissionsByIdAsync(driveId, true)
                    : new();

                _driveProxy = MapDriveProxy(drive);
                _driveMemberProxies = permissions.Select(MapDriveMemberProxy).ToList();

                return (_driveProxy, _driveMemberProxies);
            }
        }
        private DriveProxy MapDriveProxy(object drive)
        {
            var dict = ConvertObjToDictionary(drive);

            if (dict.TryGetValue("Restrictions", out var restrictionsObj) && restrictionsObj is RestrictionsData restrictionsData)
            {
                dict["Restrictions"] = new RestrictionsDataProxy(ConvertObjToDictionary(restrictionsData));
            }

            return new DriveProxy(dict);
        }
        private DriveMemberProxy MapDriveMemberProxy(Permission permission)
        {
            var dict = ConvertObjToDictionary(permission);

            if (dict.TryGetValue("PermissionDetails", out var PermissionDetailsObject) && PermissionDetailsObject is List<PermissionDetailsData> permissionDetails)
            {
                dict["PermissionDetails"] = permissionDetails.Select(p => new PermissionDetailsDataProxy(ConvertObjToDictionary(p))).ToList();
            }
            var test = new DriveMemberProxy(dict);
            return new DriveMemberProxy(dict);
        }


        public List<GoogleDriveMember> ConvertDriveMemberProxyToGoogleDriveMember(List<DriveMemberProxy> permissions)
        {
            return permissions.Select(item => new GoogleDriveMember
            {
                Id = item.Id,
                DisplayName = item.DisplayName,
                Type = item.Type,
                Role = item.Role,
                PhotoLink = item.PhotoLink,
                EmailAddress = item.EmailAddress,
                Domain = item.Domain,
                AllowFileDiscovery = item.AllowFileDiscovery,
                ExpirationTime = item.ExpirationTimeDateTimeOffset == null ? 0 : item.ExpirationTimeDateTimeOffset.Value.Ticks,
                PermissionDetails = item.PermissionDetails?
                    .Select(x => new AvePermissionDetailsData
                    {
                        PermissionRole = x.Role,
                        PermissionType = x.PermissionType,
                        Inherited = x.Inherited,
                        InheritedFrom = x.Inherited == true ? x.InheritedFrom : null
,
                    })
                    .ToList() ?? [],
            }).ToList();
        }
        //private Dictionary<string, object> ToDictionary(object obj)
        //{
        //    if (obj == null) return new();
        //    return obj.GetType()
        //              .GetProperties()
        //              .Select(prop => new { prop.Name, Value = prop.GetValue(obj) })
        //              .Where(x => x.Value != null)
        //              .ToDictionary(
        //                  x => x.Name,
        //                  x => x.Value ?? new()
        //              );
        //}

        #region restore
        public async Task RestoreSelf(RestoreContentDto driveDto)
        {
            _logger.Info($"Restore self,drive name:{driveDto.Name},conflict:{ConflictResolution}");
            _isNewCreated = false;
            if (driveDto.Type == GDriveDataType.SharedDrive)
            {
                var drive = await RootDriveSerivce.GetDriveAsync(driveDto.Id);
                if (drive == null)
                {
                    _logger.Info("not found drive by id.");
                    drive = await RootDriveSerivce.GetDriveByNameAsync(driveDto.Name, driveDto.Id);
                    if(drive == null)
                    {
                        _logger.Info("not found drive by name.");
                        drive = await CreateDriveAsync(driveDto.Name, driveDto.Id);
                        _isNewCreated = true;
                        await Task.Delay(1000 * 60);
                    }
                }
                else
                {
                    AveRestoreReportDto.Status = RestoreStatus.Skipped;
                }
                if(drive != null)
                {
                    _driveNodeInfo.DriveName = drive.Id;
                    _driveNodeInfo.Id = drive.Id;
                    _driveService = null;
                }
                _driveProxy = new DriveProxy(ConvertObjToDictionary(drive));
                
                ObjectIdMappings.TryAdd(driveDto.Id, drive.Id);
                AveRestoreReportDto.SourcePath = drive.Name;
            }
            else
            {
                var myDrive = await DriveService.GetMyDriveAsync(driveDto.Name);
                var dics = new Dictionary<string, object>();
                dics["Id"] = myDrive.Id;
                dics["Name"] = driveDto.DriveName;
                _driveProxy = new DriveProxy(dics);
                ObjectIdMappings.TryAdd(driveDto.Id, _driveProxy.Id);
            }
            ReportCenter.HasCompleteNode = true;
        }
        public async Task HandleRestoreDriveBasic(GoogleDriveBasic driveBasicInfo, RestoreContentDto driveDto)
        {
            _logger.Info($"Restore drive basic info,drive name:{driveDto.Name},conflict:{ConflictResolution}");
            if (_isNewCreated)
            {
                if (driveDto.Type == GDriveDataType.SharedDrive)
                {
                    await RestoreShareDriveBasic(driveBasicInfo);
                }
                else
                {
                    await RestoreMyDriveBasic(driveBasicInfo);
                }
            }
           
            this.AveRestoreReportDto.Status = _isNewCreated ? RestoreStatus.Success : RestoreStatus.Skipped;
            this.AveRestoreReportDto.SourcePath = driveDto.Name;
            this.AveRestoreReportDto.Path = _driveProxy?.Name ?? driveDto.Name;
        }

        private async Task RestoreShareDriveBasic(GoogleDriveBasic driveBasicInfo)
        {
            var drive = await DriveService.GetDriveAsync(_driveProxy.Id);
            if (drive != null)
            {
                var newDrive = new Drive();
                newDrive.Name = driveBasicInfo.Name;
                newDrive.ColorRgb = drive.ColorRgb;
                drive = await DriveService.UpdateDriveAsync(newDrive, _driveProxy.Id);
                _logger.Info($"share drive {driveBasicInfo.Name} exist, allow update, newID {drive.Id}");

                _driveProxy = MapDriveProxy(drive);
            }
        }
        private async Task<Drive> CreateDriveAsync(string driveName, string oldId)
        {
            _logger.Info($"Don't find Share drive id {oldId}, name {driveName}");

            _isNewCreated = true;

            var newDrive = new Drive() { Name = driveName };

            newDrive = await RootDriveSerivce.CreateDrive(newDrive);

            _newDriveId = newDrive.Id;   
            _logger.Info($"share drive {driveName} created successfull, oldID {oldId}, newID {newDrive.Id}");
            return newDrive;
        }
        private async Task RestoreMyDriveBasic(GoogleDriveBasic driveBasicInfo)
        {
            //var drive = await DriveService.GetMyDriveAsync(driveBasicInfo.Name);
            //if (drive != null)
            //{
            //    _driveProxy = MapDriveProxy(drive);
            //    _logger.Info($"my drive {driveBasicInfo.Name} exist, needn't update.");
                
            //}
            //else
            //{
            //    _logger.Info($"my drive {driveBasicInfo.Name} removed in console.");
            //}
        }
        public async Task HandleRestoreDriveMember(List<GoogleDriveMember> driveMemberInfo, RestoreContentDto driveDto)
        {
            if (driveDto.Type == GDriveDataType.MyDrive || !_isNewCreated)
            {
                _logger.Info($"My drive don't backup member, so skip restore, is new created :{_isNewCreated}");
                return;
            }
            _logger.Info($"Update drive member for drive {_driveProxy.Name} with id {_driveProxy.Id}.");
            var permissions = await DriveService.GetPermissionsByIdAsync(_driveProxy.Id);
            var currentMembers = ConvertDriveMemberProxyToGoogleDriveMember(permissions.Select(MapDriveMemberProxy).ToList());
            await MergeSharedDriveMembers(driveMemberInfo, currentMembers, _driveProxy.Id);
        }
        public async Task<bool> MergeSharedDriveMembers(List<GoogleDriveMember> oldMembers, List<GoogleDriveMember> currentMembers, string driveId, FileQuery query = null)
        {
            bool success = true;
            if (oldMembers != null && oldMembers.Count > 0)
            {
                foreach (var oldMember in oldMembers)
                {
                    
                    try
                    {
                        bool needRecreate = false;
                        if (oldMember.Type.EqualIgnoreCase("anyone") || oldMember.Type.EqualIgnoreCase("domain"))
                        {
                            var newPermission = currentMembers.Find(i => i.Type.EqualIgnoreCase(oldMember.Type) && i.Domain.EqualIgnoreCase(oldMember.Domain));
                            if (newPermission != null)
                            {
                                if (Convert.ToBoolean(newPermission.AllowFileDiscovery) != Convert.ToBoolean(oldMember.AllowFileDiscovery) || !newPermission.Role.EqualIgnoreCase(oldMember.Role))
                                {
                                    needRecreate = true;
                                }
                                if (needRecreate)
                                {
                                    await DriveService.DeletePermissionAsync(driveId, newPermission.Id);
                                    await DriveService.CreatePermissionAsync(oldMember, driveId, query);
                                    continue;
                                }
                            }
                            else
                            {
                                await DriveService.CreatePermissionAsync(oldMember, driveId, query);
                            }
                        }
                        else
                        {
                            var newPermission = currentMembers.Find(i => i.EmailAddress.EqualIgnoreCase(oldMember.EmailAddress));
                            if (newPermission != null)
                            {
                                var newPD = newPermission.PermissionDetails.Find(d => d.PermissionRole.EqualIgnoreCase(newPermission.Role));
                                var tempPD = oldMember.PermissionDetails?.Find(d => d.PermissionRole.EqualIgnoreCase(oldMember.Role));
                                if (Convert.ToBoolean(newPD.Inherited) || Convert.ToBoolean(tempPD?.Inherited))
                                {
                                    continue;
                                }
                                await DriveService.UpdatePermissionAsync(oldMember, newPermission, driveId);
                            }
                            else
                            {
                                await DriveService.CreatePermissionAsync(oldMember, driveId);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Warn($"Failed to merge permission {oldMember.Role} for {oldMember.EmailAddress}.Error:{e}");
                        success = false;
                    }
                }
            }

            return success;
        }
        public async Task HandleRestoreDriveSetting(GoogleDriveSetting driveSettingInfo, RestoreContentDto driveDto)
        {
            if (driveDto.Type == GDriveDataType.MyDrive || !_isNewCreated)
            {
                _logger.Info($"My drive don't backup setting, so skip restore drive setting. new created:{_isNewCreated}");
                return;
            }
            _logger.Info($"Update drive setting for drive {_driveProxy.Name} with id {_driveProxy.Id}.");
            _driveProxy.Restrictions = new RestrictionsDataProxy(ConvertObjToDictionary(driveSettingInfo));
            await DriveService.UpdateDriveSettingAsync(_driveProxy);
            return;
        }

        #endregion
    }
}
