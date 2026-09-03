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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Services;
using Google.Apis.Drive.v3.Data;
using Google.Apis.DriveLabels.v2.Data;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Report;
using RAGoogle.Services;
using System.Collections.Concurrent;
using Util;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.GoogleObjDiscover;

public class RMGoogleDiscoverBase
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleDiscoverBase));
    private readonly string anyonePermissionId = "anyoneWithLink";
    private readonly string anyonePermissionName = "Anyone with the link";
    private string? _allLabelsId = null;
    protected ReportCenter reportCenter;

    protected RMAosGoogleAppProfile appInfo;

    protected List<string> Domains;

    protected DataQueue<GoogleItemData> ItemQueue { get; set; }

    protected bool IncludeLabel { get; set; }
    protected bool IncludeVersion { get; set; }

    protected string TeantId { get; set; }

    protected File RootFolder { get; set; }

    protected GoogleDirectoryService directoryService;

    protected ConcurrentDictionary<string, GoogleAppsDriveLabelsV2Label> GoogleLabelMapping { get; set; }

    protected string AllLabelsId
    {
        get
        {
            if (_allLabelsId != null)
            {
                return _allLabelsId;
            }
            return _allLabelsId = GoogleLabelMapping.IsNotNullOrEmpty() ? string.Join(",", GoogleLabelMapping.Values.Select(x => x.Id)) : string.Empty;
        }
    }
    protected Dictionary<string, string> userEmailsCache { get; set; }
    protected DateTime DiscoverStartTime { get; private set; }
    protected DateTime DiscoverEndTime { get; private set; }
    protected ConcurrentBag<string> CacheItemIds { get; set; }
    protected ConcurrentDictionary<string, string> PermissionIdWithUserEmail { get; set; } = [];
    public RMGoogleDiscoverBase(DataQueue<GoogleItemData> itemQueue)
    {
        ItemQueue = itemQueue;
        userEmailsCache = new();
        CacheItemIds = new();
    }

    public void Init(RMAosGoogleAppProfile gProfile)
    {
        appInfo = gProfile;
    }

    public void Init(ReportCenter gReportCenter, RMAosGoogleAppProfile gProfile, bool includeLabel = false, bool includeVersion = false)
    {
        reportCenter = gReportCenter;
        appInfo = gProfile;
        this.IncludeLabel = includeLabel;
        this.IncludeVersion = includeVersion;
        userEmailsCache.Clear();
        InitGoogleLabelNameMappingAsync().Wait();
    }
    public void SetScanTime(DateTime discoverStartTime, DateTime discoverEndTime)
    {
        DiscoverStartTime = discoverStartTime;
        DiscoverEndTime = discoverEndTime;
    }
    protected async Task<string> GetFirstDelegateMemberAsync(List<Permission> members)
    {
        string? internalMember = members?.Find(m => m.Type is "user" && m.Role == "organizer" && !m.EmailAddress.IsExternalUser(Domains))?.EmailAddress;
        if (internalMember.IsNullOrEmpty())
        {
            internalMember = members?.Find(m => m.Type is "user" && !m.EmailAddress.IsExternalUser(Domains))?.EmailAddress ?? "";
            logger.Warn($"Can not find the organizer. Get the first internal user: {internalMember}");
        }
        if (internalMember.IsNullOrEmpty() && members.IsNotNullOrEmpty())
        {
            using (GoogleDirectoryService directoryService = new(appInfo))
            {
                foreach (var group in members.Where(m => m.Type is "group"))
                {
                    string internalGroup = group.EmailAddress;
                    if (internalGroup.IsNotNullOrEmpty())
                    {
                        var member = await directoryService.GetGroupFirstUserAsync(internalGroup, appInfo.TenantId);
                        internalMember = member?.Email;
                        if (internalMember.IsNotNullOrEmpty())
                        {
                            break;
                        }
                    }
                }
            }
        }
        //internlMember.ThrowIfNullOrEmpty("No valid members.");
        return internalMember;
    }
    public async Task<GoogleDriveService> GetDriveService(string driveId)
    {
        GoogleDriveService googleDriveService = null;
        using (GoogleDriveService service = new GoogleDriveService(appInfo))
        using (directoryService = new(appInfo))
        {
            await InitDomainsAsync();
            if (!driveId.IsEmailAddress())
            {
                List<Permission> members = await service.GetPermissionsByIdAsync(driveId, true);
                string firstMemberEmail = await GetFirstDelegateMemberAsync(members);
                googleDriveService = new GoogleDriveService(appInfo, firstMemberEmail);

            }
            else
            {
                googleDriveService = new GoogleDriveService(appInfo, driveId);

            }
        }
        googleDriveService.SetIncludeLabels(AllLabelsId);
        return googleDriveService;
    }

    public async Task<GoogleDriveService> GetDriveService(string selectedDriveId, string ruleDriveId)
    {
        using (GoogleDriveService service = new GoogleDriveService(appInfo))
        using (directoryService = new(appInfo))
        {
            await InitDomainsAsync();
            List<Permission> selectedDriveMembers = await service.GetPermissionsByIdAsync(selectedDriveId, true);
            List<Permission> ruleDriveMembers = await service.GetPermissionsByIdAsync(ruleDriveId, true);
            var commonMembers = selectedDriveMembers
                .Where(selectedDriveMember
                    => ruleDriveMembers.Any(ruleDriveMember => ruleDriveMember.EmailAddress == selectedDriveMember.EmailAddress)).ToList();
            string firstMemberEmail = await GetFirstDelegateMemberAsync(commonMembers);
            return new GoogleDriveService(appInfo, firstMemberEmail);
        }
    }

    private async Task UpdatePermissionToDestinationDrive(Permission selectedDriveMember, List<Permission> ruleDriveMember, string ruleDriveId)
    {
        string firstMemberEmail = await GetFirstDelegateMemberAsync(ruleDriveMember);
        using GoogleDriveService service = new(appInfo, firstMemberEmail);
        await service.CreatePermissionAsync(selectedDriveMember, ruleDriveId, true);
    }

    public async Task DeletePermissionToRuleDrive(IDictionary<string, (bool, string)> permissionDestinationDrives)
    {
        using GoogleDriveService service = new GoogleDriveService(appInfo);
        using (directoryService = new(appInfo)) ;
        await InitDomainsAsync();
        foreach (var permissionDestinationDrive in permissionDestinationDrives)
        {
            if (permissionDestinationDrive.Value.Item1)
            {
                var destDriveId = permissionDestinationDrive.Key;
                List<Permission> destDriveMembers = await service.GetPermissionsByIdAsync(destDriveId, true);
                string firstMemberEmail = await GetFirstDelegateMemberAsync(destDriveMembers);
                using GoogleDriveService destService = new(appInfo, firstMemberEmail);
                await service.DeletePermissionByMemberEmailAsync(permissionDestinationDrive.Value.Item2, destDriveId, true);
            }
        }

    }

    public async Task<(bool, string)> CheckPermissionInDestinationDrive(GoogleDriveTreeNodeDto selectedNode,
        GoogleDriveTreeNodeDto ruleNode)
    {
        using GoogleDriveService service = new GoogleDriveService(appInfo);
        using (directoryService = new(appInfo)) ;
        await InitDomainsAsync();
        List<Permission> ruleDriveMembers = await service.GetPermissionsByIdAsync(ruleNode.ObjectId, true);
        if (selectedNode.Level is NodeLevel.GoogleMyDrive)
        {
            var commonMember = ruleDriveMembers.FirstOrDefault(ruleDriveMember =>
                ruleDriveMember.EmailAddress == selectedNode.FullPath);
            if (commonMember == null)
            {
                Permission user = new()
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = selectedNode.FullPath
                };
                await UpdatePermissionToDestinationDrive(user, ruleDriveMembers, ruleNode.ObjectId);
                return (true, selectedNode.FullPath);
            }
        }
        else
        {
            List<Permission> selectedDriveMembers = await service.GetPermissionsByIdAsync(selectedNode.ObjectId, true);
            var commonMembers = selectedDriveMembers
                .Where(selectedDriveMember
                    => ruleDriveMembers.Any(ruleDriveMember =>
                        ruleDriveMember.EmailAddress == selectedDriveMember.EmailAddress)).ToList();
            string firstMemberEmail = await GetFirstDelegateMemberAsync(commonMembers);
            if (firstMemberEmail.IsNullOrEmpty())
            {
                string memberEmail = await GetFirstDelegateMemberAsync(selectedDriveMembers);
                Permission user = new()
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = memberEmail
                };
                await UpdatePermissionToDestinationDrive(user, ruleDriveMembers, ruleNode.ObjectId);
                return (true, memberEmail);
            }
        }

        return (false, string.Empty);
    }

    public async Task InitDomainsAsync()
    {
        if (Domains.IsNotEmptyCollection())
        {
            return;
        }
        var domains = await directoryService.GetAllDomainsAsync();
        string primaryDomain = domains?.Find(domain => domain.IsPrimary ?? false)?.DomainName ?? appInfo.DomainName;
        Domains = [primaryDomain, .. domains?.Select(domain => domain.DomainName) ?? []];
    }

    protected GoogleItemMetaInfo GenerateMetaInfo(GoogleItemData itemInfo, List<Label> labelInfos)
    {
        var labes = new List<LabelMetaInfo>();
        if (itemInfo.LableIds.IsNotNullOrEmpty())
        {
            foreach (var label in labelInfos)
            {
                if (GoogleLabelMapping.TryGetValue(label.Id, out var labelInfo))
                {
                    labes.Add(new LabelMetaInfo
                    {
                        Id = label.Id,
                        Title = labelInfo.Properties.Title,
                        FieldInfos = GenerateFieldMetaInfo(label.Id, label.Fields, labelInfo.Fields?.ToList()),
                        CreatedTime = DateTime.TryParse(labelInfo.CreateTimeRaw, out var date) ? date.Ticks : 0
                    });
                }
            }
        }
        return itemInfo.ConvertToMetaInfo(labes.OrderByDescending(l => l.CreatedTime).ToList());
    }

    private List<FieldMetaInfo> GenerateFieldMetaInfo(string labelId, IDictionary<string, LabelField> fields, List<GoogleAppsDriveLabelsV2Field> fieldInfos)
    {
        List<FieldMetaInfo> fieldMetaInfos = new();

        if (fieldInfos.IsNotNullOrEmpty())
        {
            foreach (var fieldInfo in fieldInfos)
            {
                FieldMetaInfo field = new()
                {
                    Id = fieldInfo.Id,
                    Title = fieldInfo.Properties.DisplayName,
                };
                GetFieldValue(fields, fieldInfo, field);
                fieldMetaInfos.Add(field);
            }
        }
        return fieldMetaInfos;
    }

    private void GetFieldValue(IDictionary<string, LabelField> fields, GoogleAppsDriveLabelsV2Field fieldInfo, FieldMetaInfo fieldMetaInfo)
    {
        var fieldValue = fields?.FirstOrDefault(f => f.Key == fieldInfo.Id).Value;
        if (fieldValue == null)
        {
            fieldMetaInfo.ValueType =
                fieldInfo.TextOptions != null ? FieldValueType.text :
                fieldInfo.IntegerOptions != null ? FieldValueType.integer :
                fieldInfo.DateOptions != null ? FieldValueType.dateString :
                fieldInfo.SelectionOptions != null ? FieldValueType.selection :
                fieldInfo.UserOptions != null ? FieldValueType.user : default;
            fieldMetaInfo.Values = [string.Empty];
            return;
        }

        if (Enum.TryParse<FieldValueType>(fieldValue.ValueType, out FieldValueType type))
        {
            List<string> values = new();
            switch (type)
            {
                case FieldValueType.text:
                    values.AddRange(fieldValue.Text);
                    break;
                case FieldValueType.integer:
                    values.AddRange(fieldValue.Integer.ConvertAll(x => x.ToString() ?? "0"));
                    break;
                case FieldValueType.dateString:
                    values.AddRange(fieldValue.DateString);
                    break;
                case FieldValueType.selection:
                    values.AddRange(GetSelectionValuesFromFieldInfo(fieldValue.Selection, fieldInfo));
                    break;
                case FieldValueType.user:
                    var userValues = fieldValue.User
                        .SelectMany(u => new[] { u.EmailAddress, u.DisplayName })
                         .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                    values.AddRange(userValues);

                    break;
            }
            fieldMetaInfo.ValueType = type;
            fieldMetaInfo.Values = values;
        }
    }

    public IEnumerable<string> GetSelectionValuesFromFieldInfo(IList<string> values, GoogleAppsDriveLabelsV2Field fieldInfo)
    {
        return values.Select(value => fieldInfo.SelectionOptions.Choices.FirstOrDefault(choice => choice.Id == value)!.Properties.DisplayName);
    }

    internal async Task InitGoogleLabelNameMappingAsync()
    {
        try
        {
            using (GoogleLabelService service = new(appInfo))
            {
                var dics = await service.ListAllLabelsAsync();
                GoogleLabelMapping = new ConcurrentDictionary<string, GoogleAppsDriveLabelsV2Label>(dics, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    protected void GetParentId(File file, GoogleItemData item)
    {
        string parentId = file.Parents.IsNotNullOrEmpty() ? file.Parents[0] : string.Empty;
        if (RootFolder != null && parentId == RootFolder.Id && RootFolder.Parents == null)
        {
            item.ParentId = "root";
        }
        else
        {
            item.ParentId = parentId;
        }
    }

    protected async Task DiscoveryMyDriveFolderAsync(GoogleDriveData gDrive, string folderId, string parentPath, GoogleDriveService service, CancellationToken token, string parentIds)
    {
        logger.Info($"Discovery my drive in folder:{folderId}");
        string? nextToken = null;
        List<File> folders = [];
        do
        {
            (List<File> files, nextToken) = await service.PageMyDriveFilesAsync(folderId, nextToken);
            foreach (File item in files)
            {
                bool isOwnedByMe = item.OwnedByMe ?? false;
                if (!isOwnedByMe && !item.IsFolder())
                {
                    continue;
                }
                if (!DoesItemNeedCollect(item))
                {
                    continue;
                }
                var itemData = item.ConvertToDto(gDrive, parentIds, parentPath, memberEmail: gDrive.DriveName);
                GetParentId(item, itemData);
                if (IncludeLabel)
                {
                    //List<Label> labels = await service.GetLabelsAppliedOnFileAsync(item.Id);
                    var labels = item.LabelInfo?.Labels?.ToList() ?? new();
                    itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                    itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                }

                if (IncludeVersion)
                {
                    var permission = await service.GetPermissionsByIdAsync(itemData.ParentId);
                    itemData.Permissions = permission.Select(x => new Permissions
                    {
                        Id = x.Id,
                        DisplayName = x.Id == anyonePermissionId ? anyonePermissionName : x.DisplayName
                    }).ToList();

                    if (item.IsFileSupportVersion())
                    {
                        itemData.Versions = await service.GetAllFileVersionsAsync(item.Id);
                    }
                }
                if (isOwnedByMe)
                {
                    CacheItemIds.Add(itemData.Id);
                    await ItemQueue.WriteAsync(itemData);
                }
                if (item.IsFolder())
                {
                    folders.Add(item);
                }
            }
        } while (nextToken.IsNotNullOrEmpty());
        foreach (File item in folders)
        {
            await DiscoveryMyDriveFolderAsync(gDrive, item.Id, $"{parentPath}/{item.Name}", service, token, $"{parentIds}/{item.Id}");
        }
    }
    protected async Task DiscoverySharedDriveFolderAsync(GoogleDriveData gDrive, string folderId, string memberEmail, string parentPath, GoogleDriveService service, CancellationToken token, string parentIds)
    {
        logger.Info($"Discovery share drive in folder {folderId}");
        List<File> folders = [];
        string? nextToken = null;
        do
        {
            (List<File> files, nextToken) = await service.PageFilesByDriveIdAsync(gDrive.Id, folderId, nextToken);
            foreach (File item in files)
            {
                if (!DoesItemNeedCollect(item))
                {
                    continue;
                }
                var itemData = item.ConvertToDto(gDrive, parentIds, parentPath, memberEmail: memberEmail).CheckModifiedByEmail(PermissionIdWithUserEmail,item);
                GetParentId(item, itemData);
                if (IncludeLabel)
                {
                    //List<Label> labels = await service.GetLabelsAppliedOnFileAsync(item.Id);
                    var labels = item.LabelInfo?.Labels?.ToList() ?? new();
                    itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                    itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                }
                if (IncludeVersion)
                {
                    var permission = await service.GetPermissionsByIdAsync(itemData.Id);

                    itemData.Permissions = permission.Select(x => new Permissions
                    {
                        Id = x.Id,
                        DisplayName = x.Id == anyonePermissionId ? anyonePermissionName : x.DisplayName,
                    }).ToList();

                    if (item.IsFileSupportVersion())
                    {
                        itemData.Versions = await service.GetAllFileVersionsAsync(item.Id);
                    }
                }
                CacheItemIds.Add(itemData.Id);
                await ItemQueue.WriteAsync(itemData);
                if (item.IsFolder())
                {
                    folders.Add(item);
                }
            }
        } while (nextToken.IsNotNullOrEmpty());
        foreach (File item in folders)
        {
            await DiscoverySharedDriveFolderAsync(gDrive, item.Id, memberEmail, $"{parentPath}/{item.Name}", service, token, $"{parentIds}/{item.Id}");
        }
    }
    #region Archive
    protected async Task DiscoveryMyDriveFilesAsync(GoogleDriveData gDrive, string folderId, string parentPath, GoogleDriveService service, CancellationToken token, string parentIds, QueryType queryType, DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Discovery my drive in folder {folderId}");
        var files = service.PageMyDriveByFolderIdAsync(folderId, queryType);
        await foreach (var fileBatch in files)
        {
            token.ThrowIfCancellationRequested();
            foreach (var item in fileBatch)
            {
                bool isOwnedByMe = item.OwnedByMe ?? false;
                if (!isOwnedByMe && !item.IsFolder())
                {
                    continue;
                }
                if (!DoesItemNeedCollect(item))
                {
                    continue;
                }
                var itemData = item.ConvertToDto(gDrive, parentIds, parentPath, memberEmail: gDrive.DriveName);
                GetParentId(item, itemData);
                if (IncludeLabel)
                {
                    //List<Label> labels = await service.GetLabelsAppliedOnFileAsync(item.Id);
                    var labels = item.LabelInfo?.Labels?.ToList() ?? new();
                    itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                    itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                }
                if (IncludeVersion)
                {
                    var permission = await service.GetPermissionsByIdAsync(itemData.ParentId);
                    itemData.Permissions = permission.Select(x => new Permissions
                    {
                        Id = x.Id,
                        DisplayName = x.Id == anyonePermissionId ? anyonePermissionName : x.DisplayName
                    }).ToList();

                    if (item.IsFileSupportVersion())
                    {
                        itemData.Versions = await service.GetAllFileVersionsAsync(item.Id);
                    }
                }
                if (isOwnedByMe)
                {
                    CacheItemIds.Add(itemData.Id);
                    await itemQueue.WriteAsync(itemData);
                }
            }
        }
    }
    protected async Task DiscoverySharedDriveFilesAsync(GoogleDriveData gDrive, string folderId, string memberEmail, string parentPath, GoogleDriveService service, CancellationToken token, string parentIds, QueryType queryType, DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Discovery share drive in folder {folderId}.");
        string? nextToken = null;
        do
        {
            (List<File> files, nextToken) = await service.PageSharedDriveByFolderAsync(gDrive.Id, folderId, queryType, nextToken);
            foreach (File item in files)
            {
                if (!DoesItemNeedCollect(item))
                {
                    continue;
                }
                var itemData = item.ConvertToDto(gDrive, parentIds, parentPath, memberEmail: memberEmail).CheckModifiedByEmail(PermissionIdWithUserEmail, item);
                GetParentId(item, itemData);
                if (IncludeLabel)
                {
                    //List<Label> labels = await service.GetLabelsAppliedOnFileAsync(item.Id);
                    var labels = item.LabelInfo?.Labels?.ToList() ?? new();
                    itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                    itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                }
                if (IncludeVersion)
                {
                    var permission = await service.GetPermissionsByIdAsync(itemData.Id);

                    itemData.Permissions = permission.Select(x => new Permissions
                    {
                        Id = x.Id,
                        DisplayName = x.Id == anyonePermissionId ? anyonePermissionName : x.DisplayName,
                    }).ToList();

                    if (item.IsFileSupportVersion())
                    {
                        itemData.Versions = await service.GetAllFileVersionsAsync(item.Id);
                    }
                }
                CacheItemIds.Add(itemData.Id);
                await itemQueue.WriteAsync(itemData);
            }
        } while (nextToken.IsNotNullOrEmpty());
    }
    #endregion
    #region private
    protected bool DoesItemNeedCollect(File file)
    {
        return !(file.IsShortcut() || file.IsHomeSite());
    }
    #endregion
}
