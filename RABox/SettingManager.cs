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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Text;

namespace RABox
{
    public class SettingManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(SettingManager));
        private readonly IRMBoxSettingsService _boxSettingService;
        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private readonly Dictionary<string, BoxSettingDto> _settingInfoCache;
        private bool _isInitContainerAndConnectionSetting = false;
        private Guid _currentOwnerId = Guid.Empty;

        public SettingManager()
        {
            _boxSettingService = PlatformWindsorManager.GetService<IRMBoxSettingsService>();
            _settingInfoCache = new Dictionary<string, BoxSettingDto>();
        }

        public async Task InitSettingAsync(BoxTreeNode topNode)
        {
            if (!_isInitContainerAndConnectionSetting)
            {
                (var hasSetting, var settingInfo) = await _boxSettingService.TryGetSettingInfoAsync(topNode.ContainerId, topNode.ContainerId);
                if (hasSetting)
                {
                    var settingKey = GenerateSettingKey(topNode, RMNodeLevel.BoxConnectionGroup);
                    AddToSettingInfoCache(settingKey, settingInfo);
                }

                (hasSetting, settingInfo) = await _boxSettingService.TryGetSettingInfoAsync(topNode.ConnectionId, topNode.ContainerId, topNode.ConnectionId);
                if (hasSetting)
                {
                    var settingKey = GenerateSettingKey(topNode, RMNodeLevel.BoxConnection);
                    AddToSettingInfoCache(settingKey, settingInfo);
                }

                _isInitContainerAndConnectionSetting = true;
            }

            GetUserUniqueId(topNode);

            BoxTreeNode currentNode = topNode;

            while (currentNode.Level != RMNodeLevel.BoxConnection)
            {
                if (currentNode.Level == RMNodeLevel.BoxFolder && currentNode.RealId != topNode.RealId &&
                    topNode.Parent.Level == RMNodeLevel.BoxFolder && currentNode.RealId != topNode.Parent.RealId)
                {
                    (var hasSetting, var settingInfo) = await _boxSettingService.TryGetSettingInfoAsync(currentNode.Id, topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                    if (hasSetting)
                    {
                        var settingKey = GenerateSettingKey(currentNode, RMNodeLevel.BoxFolder);
                        AddToSettingInfoCache(settingKey, settingInfo);
                    }
                }

                if (currentNode.Level == RMNodeLevel.BoxUser)
                {
                    (var hasSetting, var settingInfo) = await _boxSettingService.TryGetSettingInfoAsync(currentNode.Id, currentNode.ContainerId, topNode.ConnectionId, currentNode.Id);
                    if (hasSetting)
                    {
                        var settingKey = GenerateSettingKey(currentNode, RMNodeLevel.BoxUser);
                        AddToSettingInfoCache(settingKey, settingInfo);
                    }
                }

                currentNode = currentNode.Parent;
            }
        }

        public async Task<BoxSettingDto> GetSettingInfoAsync(BoxTreeNode topNode, BoxFolderProxy scanFolder)
        {
            using (new PerformanceScope("Box:DataSync:GetSettingInfo", "", true))
            {
                BoxFolderProxy currentFolder = scanFolder;

                while (currentFolder != null && !currentFolder.IsRootFolder)
                {
                    var folderSettingKey = GenerateSettingKey(topNode, RMNodeLevel.BoxFolder, currentFolder);
                    if (_settingInfoCache.TryGetValue(folderSettingKey, out var folderSettingInfo))
                    {
                        return folderSettingInfo;
                    }

                    (var hasSetting, folderSettingInfo) = await _boxSettingService.TryGetSettingInfoAsync(currentFolder.UniqueId.ToString(), topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                    if (hasSetting)
                    {
                        AddToSettingInfoCache(folderSettingKey, folderSettingInfo);
                        return folderSettingInfo;
                    }

                    currentFolder = currentFolder.Parent;
                }

                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).
                    FirstOrDefault(item =>
                    (item.Split('/').Last().StartsWith(SettingPrefixes.FolderNode) && item.Contains(_currentOwnerId.ToString())) ||
                    item.EndsWith(_currentOwnerId.ToString()) ||
                    item.EndsWith(topNode.ConnectionId) ||
                    item.EndsWith(topNode.ContainerId));

                if (settingKey == null)
                {
                    throw new Exception($"The term setting for scanning node: [{topNode.Id}] with node level [{topNode.Level}] not found.");
                }

                return _settingInfoCache[settingKey];
            }
        }

        public bool TryGetScheduleInfo(Record scanRecord, out ScheduleInfo scheduleInfo)
        {
            var profileIds = new List<Guid>(scanRecord.Ancestors);

            profileIds.Reverse();

            profileIds.Add(scanRecord.Id);

            scheduleInfo = _boxSettingService.GetScheduleInfo(profileIds);

            return scheduleInfo != null;
        }

        public async Task<BoxSettingDto> GetSettingInfoAsync(BoxTreeNode topNode, Record scanRecord)
        {
            using (new PerformanceScope("Box:DataSync:GetSettingInfo", "", true))
            {
                var settingScopeIds = new List<Guid> { scanRecord.Id };
                settingScopeIds.AddRange(scanRecord.Ancestors);

                var settingIdsCount = topNode.Parent.Level != RMNodeLevel.BoxFolder ?
                    settingScopeIds.Count - 3 :
                    settingScopeIds.IndexOf(Guid.Parse(topNode.Parent.Id)) + 1;

                var settingFolderIds = settingScopeIds.Take(settingIdsCount);

                foreach (var settingScopeId in settingFolderIds)
                {
                    var folderSettingKey = GenerateSettingKey(topNode, settingScopeIds, settingScopeId);

                    if (_settingInfoCache.TryGetValue(folderSettingKey, out var folderSettingInfo))
                    {
                        return folderSettingInfo;
                    }

                    (var hasSetting, folderSettingInfo) = await _boxSettingService.TryGetSettingInfoAsync(settingScopeId.ToString(), topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                    if (hasSetting)
                    {
                        AddToSettingInfoCache(folderSettingKey, folderSettingInfo);
                        return folderSettingInfo;
                    }

                }

                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).
                    FirstOrDefault(item =>
                    (item.Split('/').Last().StartsWith(SettingPrefixes.FolderNode) && item.Contains(_currentOwnerId.ToString())) ||
                    item.EndsWith(_currentOwnerId.ToString()) ||
                    item.EndsWith(topNode.ConnectionId) ||
                    item.EndsWith(topNode.ContainerId));

                if (settingKey == null)
                {
                    throw new Exception($"The term setting for scanning node: [{topNode.Id}] with node level [{topNode.Level}] not found.");
                }

                return _settingInfoCache[settingKey];
            }
        }

        public async Task ResetSettingInfoAsync(BoxTreeNode topNode, BoxFolderProxy scanFolder)
        {
            try
            {
                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).FirstOrDefault(item => item.EndsWith(scanFolder.UniqueId.ToString()));
                if (!string.IsNullOrEmpty(settingKey))
                {
                    var settingInfo = _settingInfoCache[settingKey];
                    if (settingInfo.ScopeId == scanFolder.UniqueId.ToString())
                    {
                        _logger.Info($"Current box folder node [{scanFolder.Id}] has settings , Reset it.");
                        await _boxSettingService.ResetSyncSettingAsync(scanFolder.UniqueId.ToString(), topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while reset box folder node [{scanFolder.Id}] sync setting info. Error: {e}");
            }
        }

        public async Task ResetSettingInfoAsync(BoxTreeNode topNode, Record scanRecord)
        {
            try
            {
                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).FirstOrDefault(item => item.EndsWith(scanRecord.Id.ToString()));
                if (!string.IsNullOrEmpty(settingKey))
                {
                    var settingInfo = _settingInfoCache[settingKey];
                    if (settingInfo.ScopeId == scanRecord.Id.ToString())
                    {
                        _logger.Info($"Current box folder node [{scanRecord.ExternalId}] has settings , Reset it.");
                        await _boxSettingService.ResetSyncSettingAsync(scanRecord.Id.ToString(), topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while reset box folder node [{scanRecord.Id.ToString()}] sync setting info. Error: {e}");
            }
        }

        public async Task ResetSettingInfoAsync(BoxTreeNode topNode, string jobId)
        {
            try
            {
                var parentJobId = jobId.Split('_')[0];

                if (topNode.StartJobNodeLevel <= RMNodeLevel.BoxConnection)
                {
                    var subJobExtension = topNode.ContainerId + "/" + topNode.ConnectionId;
                    _logger.Info($"Process reset setting for Connection level");
                    await ProcessResetSettingBySubJobAsync(topNode.ConnectionId, topNode.ContainerId, topNode.ConnectionId, parentJobId, subJobExtension);
                }

                if (topNode.StartJobNodeLevel == RMNodeLevel.BoxConnectionGroup)
                {
                    _logger.Info($"Process reset setting for Connection Group level");
                    await ProcessResetSettingBySubJobAsync(topNode.ContainerId, topNode.ContainerId, "", parentJobId, topNode.ContainerId.ToString());
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while reset box node [{topNode.ContainerId}] - [{topNode.ConnectionId}] sync setting info. Error: {e}");
            }
        }

        private async Task ProcessResetSettingBySubJobAsync(string nodeId, string containerId, string connectionId, string parentJobId, string subJobExtension)
        {
            var subJobs = await _subJobDao.FindListAsync(s => s.ParentId == parentJobId && s.String1.Contains(subJobExtension));

            if (!subJobs.Exists(s =>
                s.Status == (int)JobStatus.Failed || s.Status == (int)JobStatus.Wait || s.Status == (int)JobStatus.InProgress))
            {
                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).FirstOrDefault(item => item.EndsWith(nodeId));
                if (!string.IsNullOrEmpty(settingKey))
                {
                    _logger.Info($"Current node [{nodeId}] has settings , Reset it.");
                    await _boxSettingService.ResetSyncSettingAsync(nodeId, containerId, connectionId);
                }
            }
        }

        private string GenerateSettingKey(BoxTreeNode topNode, RMNodeLevel nodeLevel, BoxFolderProxy? folderProxy = null)
        {
            StringBuilder keyBuilder = new StringBuilder();

            keyBuilder.Append(SettingPrefixes.Group).Append(topNode.ContainerId);

            if (!topNode.ConnectionId.Equals(Guid.Empty) && nodeLevel >= RMNodeLevel.BoxConnection)
            {
                keyBuilder.Append('/').Append(SettingPrefixes.Connection).Append(topNode.ConnectionId);
            }

            if (!topNode.Id.Equals(Guid.Empty) && nodeLevel >= RMNodeLevel.BoxUser)
            {
                keyBuilder.Append('/').Append(SettingPrefixes.User).Append(_currentOwnerId);
            }

            if (!topNode.Id.Equals(Guid.Empty) && nodeLevel >= RMNodeLevel.BoxFolder)
            {
                var folderPrefixesInOrder = new StringBuilder();
                BoxTreeNode currentNode = folderProxy == null ? topNode : topNode.Parent.Parent;
                while (currentNode.Level == RMNodeLevel.BoxFolder)
                {
                    folderPrefixesInOrder.Insert(0, '/' + SettingPrefixes.FolderNode + currentNode.Id);
                    currentNode = currentNode.Parent;
                }

                keyBuilder.Append(folderPrefixesInOrder);
            }

            if (folderProxy != null && !folderProxy.IsRootFolder && nodeLevel >= RMNodeLevel.BoxFolder)
            {
                var folderPrefixesInOrder = new StringBuilder();
                BoxFolderProxy currentFolder = folderProxy;
                while (currentFolder != null && !currentFolder.IsRootFolder)
                {
                    folderPrefixesInOrder.Insert(0, '/' + SettingPrefixes.Folder + currentFolder.UniqueId.ToString());
                    currentFolder = currentFolder.Parent;
                }

                keyBuilder.Append(folderPrefixesInOrder);
            }

            return keyBuilder.ToString();
        }

        private string GenerateSettingKey(BoxTreeNode topNode, List<Guid> scopeIds, Guid folderId)
        {
            StringBuilder keyBuilder = new StringBuilder();
            var scopeIdsByOrder = new List<Guid>(scopeIds);
            scopeIdsByOrder.Reverse();

            keyBuilder.Append(SettingPrefixes.Group).Append(scopeIdsByOrder[0])
                .Append('/').Append(SettingPrefixes.Connection).Append(scopeIdsByOrder[1])
                .Append('/').Append(SettingPrefixes.User).Append(scopeIdsByOrder[2]);

            var isNodeFolder = true;
            var topNodeParentId = topNode.Parent.Id.ToString();

            foreach (var item in scopeIdsByOrder.Skip(3))
            {
                var currentId = item.ToString();
                if (isNodeFolder && currentId == topNodeParentId)
                {
                    isNodeFolder = false;
                }

                if (item == folderId)
                {
                    keyBuilder.Append('/').Append(SettingPrefixes.Folder).Append(currentId);
                    break;
                }

                keyBuilder.Append('/').Append(isNodeFolder ? SettingPrefixes.FolderNode : SettingPrefixes.Folder).Append(currentId);
            }

            return keyBuilder.ToString();
        }

        private void AddToSettingInfoCache(string settingKey, BoxSettingDto settingInfo)
        {
            if (!_settingInfoCache.ContainsKey(settingKey))
            {
                _settingInfoCache[settingKey] = settingInfo;
            }
        }

        public async Task ClearUserAndFolderSettings(BoxTreeNode topNode)
        {
            var keysToRemove = _settingInfoCache.Keys
                                                .Where(key => key.Contains(SettingPrefixes.User))
                                                .ToList();

            if (topNode.StartJobNodeLevel <= RMNodeLevel.BoxUser)
            {
                var settingKey = _settingInfoCache.Keys.OrderByDescending(item => item.Count(c => c == '/')).FirstOrDefault(item => item.EndsWith(_currentOwnerId.ToString()));
                if (!string.IsNullOrEmpty(settingKey))
                {
                    _logger.Info($"Current box user node [{_currentOwnerId}] has settings , Reset it.");
                    await _boxSettingService.ResetSyncSettingAsync(_currentOwnerId.ToString(), topNode.ContainerId, topNode.ConnectionId, _currentOwnerId.ToString());
                }
            }

            foreach (var key in keysToRemove)
            {
                _settingInfoCache.Remove(key);
            }
        }

        private void GetUserUniqueId(BoxTreeNode topNode)
        {
            BoxTreeNode currentNode = topNode;

            while (currentNode.Level != RMNodeLevel.BoxConnection)
            {
                if (currentNode.Level == RMNodeLevel.BoxUser)
                {
                    _currentOwnerId = new(currentNode.Id);
                }

                currentNode = currentNode.Parent;
            }
        }

        public (bool,BoxSettingDto?) TryGetSettingInfoByAncestorIds (List<Guid> ancestorIds)
        {
            if (!ancestorIds.Any())
            {
                return (false,null);
            }

            var connGroupId = ancestorIds.Last().ToString();
            var connId = ancestorIds[ancestorIds.Count - 2].ToString();
            var userId = ancestorIds[ancestorIds.Count - 3].ToString();

            if(!_settingInfoCache.TryGetValue(string.Join(",", ancestorIds), out BoxSettingDto? settingCached))
            {
                foreach (var ancestorId in ancestorIds)
                {
                    if(ancestorId.ToString().Equals(connGroupId))
                    {
                        connId = Guid.Empty.ToString();
                        userId = string.Empty;
                    }

                    var (hasSetting, setting) = _boxSettingService.TryGetSettingInfoAsync(ancestorId.ToString(), connGroupId, connId, userId).Result;

                    if (!hasSetting) continue;

                    _settingInfoCache[string.Join(",", ancestorIds)] = setting;

                    settingCached = setting;

                    return (hasSetting, settingCached);
                }

                return (false,null);
            }

            return (true, settingCached);
        }
    }

    static class SettingPrefixes
    {
        public const string Group = "ContainerId:";
        public const string Connection = "ConnectionId:";
        public const string User = "UserId:";
        public const string FolderNode = "FolderNodeId:";
        public const string Folder = "FolderId:";
    }
}
