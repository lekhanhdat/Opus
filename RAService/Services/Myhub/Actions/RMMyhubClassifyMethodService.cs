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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Myhub;
using AvePoint.RA.Service.Services.Myhub.Views;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.TermManagement;
using Google.Api.Gax.ResourceNames;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using PnP.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AvePoint.RA.Service.Services.MyHub.Actions.RMMyhubRunActionMethod;

namespace AvePoint.RA.Service.Services.MyHub.Actions
{
    public class RMMyhubClassifyMethodService
    {
        RALogger logger = RALogger.GetInstance(typeof(RMMyhubServices));

        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IRMFileSystemBrowserService FileSystemBrowserService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private IRMMyhubFileClassifyMethodService FileClassifyMethodService => PlatformWindsorManager.GetService<IRMMyhubFileClassifyMethodService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public bool IsFolderTarget(RMMyhubClassifyItem target)
        {
            return target != null && target.NodeType == (int)NodeLevel.FSFolder;
        }
        //由于是借用API调用方法，先build出所有需要的类型格式，待后期逐渐删减
        #region job logic
        public async Task<List<MyhubClassifyReturnMessage>> RunFolderClassifyJobAsync(List<RMMyhubClassifyItem> folders, RMMyhubClassifyQueryInfo queryInfo)
        {
            try
            {
                var msgList = new List<MyhubClassifyReturnMessage>();
                var folderTreeNodeList = new List<RMFSTreeNode>();
                var folderNodes = BuildRelativeNodes(folders);
                var groupNode = new RMFSTreeNode();
                foreach (var folderNode in folderNodes)
                {
                    var folderTreeNode = await FileSystemSettingsService.LoadFSNodeSettingAsync(folderNode);
                    groupNode = await FileSystemSettingsService.LoadFSNodeSettingAsync(BuildGroupNode(folderTreeNode.ConnectionId));
                    var nodeType = (NodeLevel)folderTreeNode.Level;
                    if (!await FileSystemSettingsService.LoadFSNodeEnableRecordManagement(Guid.Parse(folderTreeNode.ConnectionId)) || !await FileSystemSettingsService.LoadFSNodeEnableRecordManagement(groupNode.Id))
                    {
                        msgList.Add(new MyhubClassifyReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            Type = nodeType,
                            Message = I18NEntity.GetString("RM_FS_Myhub_Classify_RecordManagementDisabled"),
                            Name = folderTreeNode.Name
                        });
                        return msgList;
                    }
                    else if (folderTreeNode.TermSetId == Guid.Empty)
                    {

                        if (groupNode.TermSetId == Guid.Empty)
                        {
                            msgList.Add(new MyhubClassifyReturnMessage
                            {
                                MessageType = RAMessageType.Failed,
                                Type = nodeType,
                                Message = I18NEntity.GetString("RM_FS_Myhub_Classify_NoClassCodeScope"),
                                Name = folderTreeNode.Name
                            });
                            return msgList;
                        }
                        else
                        {
                            folderTreeNodeList.Add(groupNode);
                        }
                    }
                    else
                    {
                        folderTreeNodeList.Add(folderTreeNode);
                    }
                }
                if (folderNodes.Count == 0)
                {
                    logger.Warn("No valid folder node found for running classify job.");
                    msgList.Add(new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = NodeLevel.Undefined,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_NoValidFolderNode")
                    });
                    return msgList;
                }
                var classCodePolicyInfos = await BuildClassCodePolicyInfo(folderNodes, queryInfo);
                if (classCodePolicyInfos.Count == 0)
                {
                    logger.Warn("Failed to build class code policy info for folder classification.");
                    msgList.Add(new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = (NodeLevel)folderNodes.FirstOrDefault().Level,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_FailedBuildClassCodePolicy")
                    });
                    return msgList;
                }
                foreach (var info in classCodePolicyInfos)
                {
                    var policyResult = await FileSystemSettingsService.MyhubSaveClassCodePolicyAsync(info, queryInfo);
                    if (policyResult.MessageType == RAMessageType.Failed)
                    {
                        msgList.Add(new MyhubClassifyReturnMessage
                        {
                            MessageType = policyResult.MessageType,
                            Type = (NodeLevel)info.FSTreeNode.Level,
                            Message = policyResult.ErrorMessage
                        });
                        return msgList;
                    }
                }
                logger.Info($"Class code policy info saved successfully for folder classification, starting to run classify job for {folderNodes.Count} folders.");
                var settingDto = await BuildApplyClassCodeSetting(folderNodes, queryInfo);
                var msg = await FileSystemSettingsService.RunApplyClassCodeJobAsync(settingDto, JobRunBy.Control);
                msgList.Add(new MyhubClassifyReturnMessage
                {
                    MessageType = msg.MessageType,
                    Type = (NodeLevel)settingDto.FSTreeNode.FirstOrDefault().Level,
                    Message = msg.MessageType == RAMessageType.Successful ? null : msg.ErrorMessage,
                });
                return msgList;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while running classify job for folders in RunFolderClassifyJobAsync,{ex}");

                return new List<MyhubClassifyReturnMessage>
                {
                    new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFolder")
                    }
                };
            }

        }
        private async Task<List<ClassCodePolicyInfo>> BuildClassCodePolicyInfo(List<RMFSTreeNode> folderNodes, RMMyhubClassifyQueryInfo queryInfo)
        {
            var termUniqueId = queryInfo.TermUniqueId;
            if (termUniqueId == Guid.Empty)
            {
                logger.Warn("Cannot resolve term id for the selected class code because TermUniqueId is empty.");
            }
            var result = new List<ClassCodePolicyInfo>();
            foreach (var folderNode in folderNodes)
            {
                var retentionScheduleType = ResolveRetentionScheduleType(queryInfo.RetentionType);
                var startDate = retentionScheduleType == RetentionScheduleType.Event ? await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(queryInfo.StartDate), queryInfo.TimeZoneId) : DateTime.MinValue;
                var info = new ClassCodePolicyInfo
                {
                    TermUniqueId = termUniqueId == Guid.Empty ? string.Empty : termUniqueId.ToString(),
                    ConnGroupId = folderNode.ConnGroupId.ToString(),
                    CurrentNodeId = folderNode.Id.ToString(),
                    ClassCode = queryInfo.ClassCode,
                    CountryCode = queryInfo.CountryCode,
                    RetentionScheduleType = retentionScheduleType,
                    StartDate = startDate,
                    ApplyExistDocument = queryInfo.IsApplySubItem,
                    FSTreeNode = folderNode,
                    TermSetId = folderNode.TermSetId.ToString(),
                    IsMyhubClassify = true
                };
                result.Add(info);
            }
            return result;
        }
        private RetentionScheduleType ResolveRetentionScheduleType(string retentionType)
        {
            var result = retentionType switch
            {
                "Event" => RetentionScheduleType.Event,
                _ => RetentionScheduleType.Flat
            };
            return result;
        }
        private async Task<ApplyClassCodeSettingDto> BuildApplyClassCodeSetting(List<RMFSTreeNode> folderNodes, RMMyhubClassifyQueryInfo queryInfo)
        {
            var termUniqueId = queryInfo.TermUniqueId;
            if (termUniqueId == Guid.Empty)
            {
                logger.Warn("Cannot resolve term id for the selected class code because TermUniqueId is empty.");
            }
            var retentionType = ConvertRetentionTypeToInt(queryInfo.RetentionType);
            var startDate = retentionType == (int)RetentionScheduleType.Event ? (await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(queryInfo.StartDate), queryInfo.TimeZoneId)).Ticks : 0;
            var settingDto = new ApplyClassCodeSettingDto
            {
                ClassCode = queryInfo.ClassCode,
                CountryCode = queryInfo.CountryCode,
                RetentionType = retentionType,
                StartDate = startDate,
                ApplyToExistingDoc = queryInfo.IsApplySubItem,
                FSTreeNode = folderNodes,
                TermId = termUniqueId.ToString(),
                IsMyhubClassify = true
            };
            return settingDto;
        }


        private List<RMFSTreeNode> BuildRelativeNodes(IEnumerable<RMMyhubClassifyItem> folders)
        {
            var result = new List<RMFSTreeNode>();

            foreach (var folder in folders ?? Enumerable.Empty<RMMyhubClassifyItem>())
            {
                if (!IsFolderTarget(folder))
                {
                    continue;
                }

                var node = BuildRelativeNode(folder);
                if (node != null)
                {
                    result.Add(node);
                }
            }

            return result
                .GroupBy(node => node.Id)
                .Select(group => group.First())
                .ToList();
        }
        // 1. 提取公共的构建基础节点的逻辑
        private (RMFSTreeNode rootNode, RMFSTreeNode groupNode, RMFSTreeNode connectionNode, FSConnection connection, FSConnectionGroup group)?
            BuildBaseNodes(string partitionKeyId)
        {
            if (!Guid.TryParse(partitionKeyId, out var connectionId))
            {
                logger.Error($"Invalid connection id: {partitionKeyId}");
                return null;
            }

            var connection = FSConnectionDao.GetConnectionById(connectionId);
            if (connection == null)
            {
                logger.Error($"Connection not found: {connectionId}");
                return null;
            }

            var group = FSConnectionGroupDao.GetGroupById(connection.GroupId);
            if (group == null)
            {
                logger.Error($"Connection group not found: {connection.GroupId}");
                return null;
            }

            var rootNode = FileSystemBrowserService.LoadFSRoot()?.FirstOrDefault()
                ?? new RMFSTreeNode
                {
                    Id = RecordsConstants.FS_ROOT_GUID,
                    Name = "File System",
                    Level = (int)NodeLevel.Farm
                };

            var groupNode = new RMFSTreeNode
            {
                Id = group.Id,
                Name = group.Name,
                Level = (int)NodeLevel.WebApplication,
                ConnGroupId = group.Id,
                FullPath = group.Name,
                Parent = rootNode,
                ParentId = rootNode.Id.ToString()
            };

            var connectionNode = new RMFSTreeNode
            {
                Id = connection.Id,
                ConnectionId = connection.Id.ToString(),
                Name = connection.Name,
                Level = (int)NodeLevel.SiteCollection,
                AgentId = connection.AgentId,
                ConnGroupId = group.Id,
                FullPath = connection.UNCPath,
                PathType = connection.PathType,
                Parent = groupNode,
                ParentId = groupNode.Id.ToString()
            };

            return (rootNode, groupNode, connectionNode, connection, group);
        }
        private RMFSTreeNode BuildGroupNode(string partitionKeyId)
        {

            if (string.IsNullOrWhiteSpace(partitionKeyId))
            {
                logger.Error($"L2PartitionKey cannot be null or empty.");
                return null;
            }
            var result = BuildBaseNodes(partitionKeyId);
            if (result == null)
            {
                logger.Error($"Failed to BuildGroupNode for connection: {partitionKeyId}");
                return null;
            }
            var (rootNode, groupNode, connectionNode, connection, group) = result.Value;

            return groupNode;
        }
        private RMFSTreeNode BuildFileNode(RMMyhubActionTarget target)
        {
            var result = BuildBaseNodes(target.L2PartitionKey);
            if (result == null)
            {
                logger.Error($"Failed to BuildFileNode for connection: {target.L2PartitionKey}");
                return null;
            }
            var (rootNode, groupNode, connectionNode, connection, group) = result.Value;

            var filePath = AppendDirectorySeparator(target.DirPath) + target.Name;

            return new RMFSTreeNode
            {
                Id = target.SelectId,
                ConnectionId = connection.Id.ToString(),
                Name = target.Name,
                Level = (int)NodeLevel.FSFile,
                AgentId = connection.AgentId,
                ConnGroupId = group.Id,
                FullPath = filePath,
                Parent = connectionNode,
                ParentId = connectionNode.Id.ToString()
            };
        }

        private RMFSTreeNode BuildRelativeNode(RMMyhubClassifyItem folder)
        {
            if (folder == null
                || string.IsNullOrWhiteSpace(folder.PartitionKeyId)
                || string.IsNullOrWhiteSpace(folder.FullPath))
            {
                logger.Warn($"Invalid folder data: folder is null or missing PartitionKeyId/FullPath");
                return null;
            }

            var result = BuildBaseNodes(folder.PartitionKeyId);
            if (result == null)
            {
                logger.Error($"Failed to build base nodes for folder: {folder.Id}, PartitionKeyId: {folder.PartitionKeyId}");
                return null;
            }

            var (rootNode, groupNode, connectionNode, connection, group) = result.Value;

            // 如果文件夹路径就是连接根路径，返回连接节点
            if (folder.FullPath == connection.UNCPath)
            {
                return connectionNode;
            }

            return new RMFSTreeNode
            {
                Id = folder.Id,
                ConnectionId = connection.Id.ToString(),
                Name = folder.LeafName,
                Level = (int)NodeLevel.FSFolder,
                AgentId = connection.AgentId,
                ConnGroupId = group.Id,
                FullPath = folder.FullPath,
                Parent = connectionNode,
                ParentId = connectionNode.Id.ToString(),
            };
        }

        private static int ConvertRetentionTypeToInt(string retentionType)
        {
            return retentionType switch
            {
                "Event" => (int)RetentionScheduleType.Event,
                "Flat" => (int)RetentionScheduleType.Flat,
                _ => 0
            };
        }

        #endregion


        //更新record字段数据，返回值为已修改的的记录数
        public async Task<List<MyhubClassifyReturnMessage>> UpdateClassifyAsync(List<RMMyhubActionTarget> queryResults, RMMyhubClassifyQueryInfo queryInfo)
        {
            try
            {
                var msgList = new List<MyhubClassifyReturnMessage>();
                if (queryResults == null || queryResults.Count == 0)
                {
                    logger.Warn("No target files found for updating classify job.");
                    msgList.Add(new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = NodeLevel.FSFile,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_NoTargetFiles")
                    });
                    return msgList;

                }

                var timerDto = await ResolveRetentionTimer(queryInfo);
                if (timerDto == null)
                {
                    logger.Warn(I18NEntity.GetString("RM_FS_ClassCode_NotMatchAnyRule"));
                    msgList.Add(new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = NodeLevel.FSFile,
                        Message = I18NEntity.GetString("RM_FS_ClassCode_NotMatchAnyRule")
                    });
                    return msgList;
                }

                var targets = queryResults.Where(item => item != null).ToList();
                if (targets.Count == 0)
                {
                    logger.Warn("No valid targets found after deserialization.");
                    msgList.Add(new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = NodeLevel.FSFile,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_NoTargetFiles")
                    });
                    return msgList;
                }
                foreach (var target in targets)
                {
                    try
                    {
                        var result = await ProcessSingleTargetAsync(target, queryInfo, timerDto);
                        msgList.Add(result);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error processing target {target?.SelectId}: {ex.Message}");
                        msgList.Add(new MyhubClassifyReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            Type = NodeLevel.FSFile,
                            Message = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFile"),
                            Name = target?.SelectId.ToString()
                        });
                    }
                }
                return msgList;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while updating classify for records in UpdateClassifyAsync,{ex}");
                return new List<MyhubClassifyReturnMessage>
                {
                    new MyhubClassifyReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        Type = NodeLevel.FSFile,
                        Message = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFile")
                    }
                };
            }
        }
        private async Task<MyhubClassifyReturnMessage> ProcessSingleTargetAsync(RMMyhubActionTarget target, RMMyhubClassifyQueryInfo queryInfo, OlderThanTimeDto timerDto)
        {

            var classCodePolicyInfo = await BuildFileClassCodePolicyInfo(target, queryInfo);

            var auditResult = await FileClassifyMethodService.PatchClassifyAsync(classCodePolicyInfo, queryInfo, target, timerDto);

            return new MyhubClassifyReturnMessage
            {
                MessageType = auditResult.MessageType,
                Type = NodeLevel.FSFile,
                Message = auditResult.MessageType == RAMessageType.Successful? null: auditResult.ErrorMessage,
                Name = target.Name
            };
        }
        private async Task<ClassCodePolicyInfo> BuildFileClassCodePolicyInfo(RMMyhubActionTarget target, RMMyhubClassifyQueryInfo queryInfo)
        {
            var fileNode = BuildFileNode(target);
            var retentionType = ResolveRetentionScheduleType(queryInfo.RetentionType);
            var startDate = retentionType == RetentionScheduleType.Event ? (await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(queryInfo.StartDate), queryInfo.TimeZoneId)) : DateTime.MinValue;
            return new ClassCodePolicyInfo
            {
                TermUniqueId = queryInfo.TermUniqueId == Guid.Empty ? string.Empty : queryInfo.TermUniqueId.ToString(),
                ConnGroupId = fileNode.ConnGroupId.ToString(),
                CurrentNodeId = fileNode.Id.ToString(),
                ClassCode = queryInfo.ClassCode,
                CountryCode = queryInfo.CountryCode,
                RetentionScheduleType = retentionType,
                StartDate = startDate,
                ApplyExistDocument = false,
                FSTreeNode = fileNode,
                TermSetId = string.Empty,
                IsMyhubClassify = true
            };
        }
        private async Task<OlderThanTimeDto> ResolveRetentionTimer(RMMyhubClassifyQueryInfo queryInfo)
        {
            var termUniqueId = queryInfo.TermUniqueId;
            if (termUniqueId == Guid.Empty)
            {
                return null;
            }
            var retentionType = ConvertRetentionTypeToInt(queryInfo.RetentionType);
            var startDate = retentionType == (int)RetentionScheduleType.Event ? (await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(queryInfo.StartDate), queryInfo.TimeZoneId)).Ticks : 0;
            return TaxonomyService.GetTheRetentionUnitByClassCode(new ApplyClassCodeSettingDto
            {
                ClassCode = queryInfo.ClassCode,
                CountryCode = queryInfo.CountryCode,
                RetentionType = retentionType,
                StartDate = startDate,
                TermId = termUniqueId.ToString()
            });
        }
        public (string sql, List<SqlParameter> parameter) BuildQueryForReturnValue(RMMyhubClassifyReturnInfo queryInfo)
        {
            if (queryInfo.Id.Length != 0)
                return (BaseSqlForReturnValue(), BaseSqlParametersForReturnValue(queryInfo.Id.FirstOrDefault()));
            else
                return (BaseSqlForDriveReturnValue(), BaseSqlParametersForReturnValue(Guid.Parse(queryInfo.PartitionKeyId)));
        }
        private string BaseSqlForReturnValue()
        {
            return @"SELECT VALUE {
    ""Id"": c.nodeId,
    ""ClassCode"":c.classCode,
    ""CountryCode"":c.countryCode,
    ""RetentionType"":c.retentionType,
    ""StartDate"":c.startDate,
    ""TermUniqueId"":c.termId
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.nodeId = @Id
AND c.recordStatus = @statuses
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        private string BaseSqlForDriveReturnValue()
        {
            return @"SELECT VALUE {
    ""Id"": c.nodeId,
    ""ClassCode"":c.classCode,
    ""CountryCode"":c.countryCode,
    ""RetentionType"":c.retentionType,
    ""StartDate"":c.startDate,
    ""TermUniqueId"":c.termId
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey = @Id
AND c.id=c.scopeId
AND c.recordStatus = @statuses
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        private List<SqlParameter> BaseSqlParametersForReturnValue(Guid id)
        {
            return
            [
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@Id", id),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active)
            ];
        }


        public (string sql, List<SqlParameter> parameter) BuildFolderTargetQuery(Guid id)
        {
            return
            (
                @"SELECT TOP 1 VALUE {
    ""Id"": c.nodeId,
    ""NodeType"": c.nodeType,
    ""PartitionKeyId"": c.l2PartitionKey,
    ""DirPath"": c.dirPath,
    ""LeafName"": c.leafName
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.nodeId = @Id
AND c.recordStatus = @statuses
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)",
                [
                    new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                    new SqlParameter("@Id", id),
                    new SqlParameter("@statuses", (int)RMRecordStatus.Active)
                ]
            );
        }

        public (string sql, List<SqlParameter> parameter) BuildSubItemClassifyQuery(RMMyhubClassifyItem folder)
        {
            return
            (
                @"SELECT VALUE {
    ""SelectId"": c.nodeId,
    ""L1PartitionKey"": c.l1PartitionKey,
    ""L2PartitionKey"": c.l2PartitionKey,
    ""L3PartitionKey"": c.l3PartitionKey
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.recordStatus = @statuses
AND c.l2PartitionKey = @l2PartitionKey
AND (c.nodeType = @folderNodeType OR c.nodeType = @fileNodeType)
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)
AND (c.dirPath = @fullPath OR STARTSWITH(c.dirPath, @descendantPath))",
                [
                    new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                    new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                    new SqlParameter("@l2PartitionKey", folder.PartitionKeyId),
                    new SqlParameter("@folderNodeType", (int)NodeLevel.FSFolder),
                    new SqlParameter("@fileNodeType", (int)NodeLevel.FSFile),
                    new SqlParameter("@fullPath", folder.FullPath),
                    new SqlParameter("@descendantPath", AppendDirectorySeparator(folder.FullPath))
                ]
            );
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if (path.EndsWith("\\", StringComparison.Ordinal) || path.EndsWith("/", StringComparison.Ordinal))
            {
                return path;
            }

            var separator = path.Contains('\\') ? '\\' : '/';
            return path + separator;
        }
    }
}
