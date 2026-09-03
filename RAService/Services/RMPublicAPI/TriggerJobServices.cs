using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using AvePoint.RA.Service.Services.Myhub.Actions;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Service.Services.RMReport;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMPublicAPI
{
    public class TriggerJobServices : ITriggerJobServices
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IRMFileSystemBrowserService FSBrowerTreeService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private IRMReportService RMReportService => PlatformWindsorManager.GetService<IRMReportService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService<IRMMyhubServices>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private static ManualApprovalRecordRepository Repository => new ManualApprovalRecordRepository();
        private IExplorerDao ExplorerDao => PlatformWindsorManager.GetService<IExplorerDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private RMMyhubPauseResumeMethod _pauseResumeMethod;
        private RMMyhubPauseResumeMethod pauseResumeMethod => _pauseResumeMethod ??= new RMMyhubPauseResumeMethod();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private const char FSPathSeparator = '\\';
        public async Task<RAReturnMessage> RunDataSyncJobAsync(FSJobNodeParam param)
        {
            try
            {
                logger.Info($"Start to trigger data sync job for nodeId: {param.NodeId}, connectionGroupId: {param.ConnectionGroupId}, level: {param.Level}");
                //Check Is valid param
                var validationResult = IsNodeEligible(param);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var fsNode = new RMFSTreeNode
                {
                    Id = param.NodeId,
                    ConnGroupId = param.ConnectionGroupId,
                    Level = param.Level,
                    FullPath = param.FullPath
                };
                var fsNodeSetting = await BuildTreeNodeAsync(fsNode);
                var validateNodeAvailable = IsNodeAvailable(fsNodeSetting);
                if (validateNodeAvailable != null)
                {
                    return validateNodeAvailable;
                }
                var validateMessage = RMFileSystemSettingsService.CheckNodeInfo(fsNodeSetting);
                if (validateMessage.MessageType == RAMessageType.Successful)
                {
                    return await RMFileSystemSettingsService.RunDataSyncJobAsync(fsNodeSetting, JobRunBy.Control);
                }
                else
                {
                    return validateMessage;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger data sync job for nodeId: {param.NodeId}, connectionGroupId: {param.ConnectionGroupId}, level: {param.Level}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger data sync job"
                };
            }
        }

        public async Task<RAReturnMessage> RunDisposalJobAsync(FSJobNodeParam param)
        {
            try
            {
                logger.Info($"Start to trigger disposal job for nodeId: {param.NodeId}, connectionGroupId: {param.ConnectionGroupId}, level: {param.Level}");
                //Check Is valid param
                var validationResult = IsNodeEligible(param);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var fsNode = new RMFSTreeNode
                {
                    Id = param.NodeId,
                    ConnGroupId = param.ConnectionGroupId,
                    Level = param.Level,
                    FullPath = param.FullPath
                };
                var fsNodeSetting = await BuildTreeNodeAsync(fsNode);
                var validateNodeAvailable = IsNodeAvailable(fsNodeSetting);
                if (validateNodeAvailable != null)
                {
                    return validateNodeAvailable;
                }
                if (fsNodeSetting.DefaultTermId == Guid.Empty || fsNodeSetting.TermSetId == Guid.Empty)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "The selected node has not set ClassCodeSet or ClassCode."
                    };
                }

                var validateMessage = RMFileSystemSettingsService.CheckNodeInfo(fsNodeSetting);
                if (validateMessage.MessageType == RAMessageType.Successful)
                {
                    return await RMFileSystemSettingsService.RunDisposalJobAsync(fsNodeSetting, JobRunBy.Control);
                }
                else
                {
                    return validateMessage;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger disposal job for nodeId: {param.NodeId}, connectionGroupId: {param.ConnectionGroupId}, level: {param.Level}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger disposal job"
                };
            }
        }

        public async Task<RAReturnMessage> RunFSDashboardJobAsync(FileSystemMyhubSelectedNodeDto selectedNode)
        {
            try
            {
                var validationResult = IsNodeEligible(new FSJobNodeParam
                {
                    NodeId = selectedNode.Level == (int)NodeLevel.SiteCollection ? new Guid(selectedNode.PartitionKeyId) : selectedNode.NodeId,
                    FullPath = selectedNode.FullPath,
                    Level = selectedNode.Level,
                    ConnectionGroupId = selectedNode.GroupId
                });
                if (validationResult != null)
                {
                    return validationResult;
                }
                var result = RMMyhubServices.RunFSMyHubDashboardJob(JobRunBy.Control, selectedNode);
                return new RAReturnMessage
                {
                    MessageType = result ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = result ? "FS MyHub dashboard job triggered successfully" : "Failed to trigger FS MyHub dashboard job"
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger FS MyHub dashboard job for selected node: {selectedNode}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger FS MyHub dashboard job"
                };
            }
        }

        public async Task<RAReturnMessage> RunDisposalByClassCodeAsync(FSDisposalClassCodeParam param)
        {
            try
            {
                logger.Info($"Start to trigger disposal by class code job for nodeId: {param.JobNodeParam.NodeId}");

                //Check Is valid param
                var validationResult = IsNodeEligible(param.JobNodeParam);
                if (validationResult != null)
                {
                    return validationResult;
                }

                var request = new FSDisposalByClassCodeRequest
                {
                    ConnectionGroupID = param.JobNodeParam.ConnectionGroupId,
                    NodeId = param.JobNodeParam.NodeId,
                    FullPath = param.JobNodeParam.FullPath,
                    Level = param.JobNodeParam.Level,
                    TermID = param.Terms
                };

                var fsNodeForValidate = new RMFSTreeNode
                {
                    Id = param.JobNodeParam.NodeId,
                    ConnGroupId = param.JobNodeParam.ConnectionGroupId,
                    Level = param.JobNodeParam.Level,
                    FullPath = param.JobNodeParam.FullPath
                };

                var validateResult = await ValidateDisposalByClassCodeRequestAsync(request);
                if (validateResult.MessageType != RAMessageType.Successful)
                {
                    return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = validateResult.ErrorMessage };
                }

                return await RMFileSystemSettingsService.RunDisposalByClassCodeJobAsync(request, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger disposal by class code job for nodeId: {param.JobNodeParam.NodeId}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger disposal by class code job"
                };
            }
        }

        public async Task<RAReturnMessage> RunDownloadRCCReportJobAsync(RCCReportRequestPublic param)
        {
            try
            {
                var rccRequest = new RCCReportRequest
                {
                    Nodes = param.Nodes,
                    ConnGroupId = param.ConnGroupId,
                    ConnectionId = param.ConnectionId,
                    JPMCId = param.JPMCId,
                    Level = param.Level,
                    TimeRange = new RCCReportTimeRange
                    {
                        PresetType = param.TimeRange.PresetType
                    },
                    IsMyHub = param.IsMyHub
                };

                if (param.IsMyHub && param.Nodes != null && param.Nodes.Count > 0)
                {
                    foreach (var node in param.Nodes)
                    {
                        if ((RMMyhubServices.CurrentNodeIsDisableDownloadRCC(param.ConnGroupId.ToString(), node.FullPath)))
                        {
                            return new RAReturnMessage
                            {
                                MessageType = RAMessageType.Failed,
                                ErrorMessage = "Have node is not Allow IO/RO download RCC report, not start job"
                            };
                        }
                    }
                }

                if (param.TimeRange.PresetType == 0)
                {
                    var gls = await GeneralSettingService.GetGeneralSettingAsync();

                    var startDate = GeneralSettingService.ConvertTiksToDateTime(gls, param.TimeRange.StartDate, false).SimplifyFormatTime;
                    var endDate = GeneralSettingService.ConvertTiksToDateTime(gls, param.TimeRange.EndDate, false).SimplifyFormatTime;

                    rccRequest.TimeRange.StartDate = DateTime.Parse(startDate).ToString("yyyy/M/d H:m");
                    rccRequest.TimeRange.EndDate = DateTime.Parse(endDate).ToString("yyyy/M/d H:m");
                }
                return RMFileSystemSettingsService.RunDownloadRCCReportJob(rccRequest, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to trigger download RCC report job", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger Download rcc job"
                };
            }
        }

        public async Task<RAReturnMessage> StopJobsAsync(List<string> ids)
        {
            try
            {
                if (JobMonitorService.StopJobs(ids) > 0)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful,
                        Extension = "Jobs stopped successfully"
                    };
                }
                else
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "No jobs were stopped."
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to stop jobs for jobIds: {string.Join(",", ids)}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to stop jobs"
                };
            }
        }
        private async Task<RAReturnMessage> ValidateDisposalByClassCodeRequestAsync(AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request)
        {
            var result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            if (request == null)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "Request body is null.";
                return result;
            }
            if (request.ConnectionGroupID == Guid.Empty)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "ConnectionGroupID is required.";
                return result;
            }
            if (request.TermID == null || request.TermID.Count == 0)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "At least one TermID is required.";
                return result;
            }

            var nodeSettings = await RMFileSystemSettingsService.LoadFSNodeSettingAsync(new RMFSTreeNode
            {
                Id = request.NodeId,
                ConnGroupId = request.ConnectionGroupID
            });
            if (nodeSettings == null)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node settings could not be found.";
                return result;
            }
            if (nodeSettings.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node is not enabled for record management.";
                return result;
            }
            if (nodeSettings.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.ParentDisable)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node inherits from a parent node that is not enabled for record management.";
                return result;
            }
            if (nodeSettings.DefaultTermId == Guid.Empty || nodeSettings.TermSetId == Guid.Empty)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The selected node has not set ClassCodeSet or ClassCode.";
            }
            return result;
        }
        
        public RAReturnMessage IsNodeEligible(FSJobNodeParam nodeParam)
        {
            var connectionGroup = FSConnectionGroupDao.GetGroupById(nodeParam.ConnectionGroupId);
            if (connectionGroup == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"Invalid connection group id."
                };
            }
            if (nodeParam.Level == (int)NodeLevel.WebApplication)
            {
                connectionGroup = FSConnectionGroupDao.GetGroupById(nodeParam.NodeId);
                if (connectionGroup == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Invalid id or level."
                    };
                }
                if (connectionGroup.Name != nodeParam.FullPath)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Connection group name does not match the provided full path."
                    };
                }
            } 
            else if (nodeParam.Level == (int)NodeLevel.SiteCollection)
            {
                var connection = FSConnectionDao.GetConnectionById(nodeParam.NodeId);
                if (connection == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Invalid id or level."
                    };
                }
                if (connection.UNCPath != nodeParam.FullPath)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Connection path does not match the provided full path."
                    };
                }
            }
            else
            {
                var node = ExplorerDao.GetRecordsByNodeIds(new List<Guid> { nodeParam.NodeId }).FirstOrDefault();
                if (node == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Invalid id or level"
                    };
                }
                if (node.NodeType != nodeParam.Level)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Node level does not match the provided level."
                    };
                }
                var fullPath = Path.Combine(node.DirPath, node.LeafName);
                string Normalize(string path)
                {
                    return path.TrimEnd('\\');
                }

                fullPath = $"{Normalize(node.DirPath)}\\{node.LeafName}";
                logger.Info($"Normalized full path: {fullPath}, Node full path: {nodeParam.FullPath}");
                if (!string.Equals(
                    Normalize(fullPath),
                    Normalize(nodeParam.FullPath)))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Node full path does not match the provided full path."
                    };
                }
               
            }
            return null;
        }

        public async Task<RAReturnMessage> RunApplyClassCodeAsync(ApplyClassCodeParam param)
        {
            try
            {
                logger.Info($"Start to trigger apply class code job for nodeId: {param.FullPath}");
                var termInfo = TermDao.GetRMTermByGuId(new Guid(param.TermId));
                if (termInfo == null || (termInfo != null && termInfo.IsDeprecated))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"Term with id {param.TermId} does not exist or is deprecated."
                    };
                }
                if(termInfo.Name != param.ClassCode)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"The class code {param.ClassCode} does not match the term name associated with the provided term ID."
                    };
                }
                var termSetInfo = TermSetDao.GetRMTermSet(termInfo.TermSetId);
                var group = FSConnectionGroupDao.GetGroupByName(param.FullPath);
                var fsNode = GetFSNode(param.FullPath);
                if (fsNode == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"The specified path {param.FullPath} does not exist as a connection or connection group."
                    };
                }

                var fsNodeSetting = await BuildTreeNodeAsync(fsNode);
                if(fsNodeSetting!= null && fsNodeSetting.TermSetId != termSetInfo.UniqueId)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"TermId not in the apply setting scope."
                    };
                }
                var applyClassSettingDto = new ApplyClassCodeSettingDto
                {
                    ClassCode = param.ClassCode,
                    CountryCode = param.CountryCode,
                    RetentionType = param.RetentionType,
                    StartDate = param.StartDate,
                    ApplyToExistingDoc = param.ApplyToExistingDoc,
                    FSTreeNode = new List<RMFSTreeNode> { fsNodeSetting },
                    TermId = param.TermId,
                };
                var classCodePolicyInfo = new ClassCodePolicyInfo
                {
                    ClassCode = param.ClassCode,
                    CountryCode = param.CountryCode,
                    RetentionScheduleType = (RetentionScheduleType)param.RetentionType,
                    StartDate = new DateTime(param.StartDate),
                    TermUniqueId = param.TermId,
                    FSTreeNode = fsNodeSetting,
                    CurrentNodeId = fsNodeSetting.Id.ToString(),
                    ConnGroupId = fsNodeSetting.ConnGroupId.ToString(),
                    ApplyExistDocument = param.ApplyToExistingDoc,
                    TermSetId = termSetInfo != null ? termSetInfo.UniqueId.ToString() : string.Empty,
                };
                var saveClassCodeInfo = await RMFileSystemSettingsService.SaveClassCodePolicyAsync(classCodePolicyInfo);
                if (saveClassCodeInfo.MessageType != RAMessageType.Successful)
                {
                    return saveClassCodeInfo;
                }
                return await RMFileSystemSettingsService.RunApplyClassCodeJobAsync(applyClassSettingDto, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger apply class code job for nodeId: {param.FullPath}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger apply class code job"
                };
            }
        }

        public async Task<RMFSTreeNode> BuildTreeNodeAsync(RMFSTreeNode sNode)
        {
            try
            {
                logger.Info(
                    "Building tree node. NodeId: {0}, ConnGroupId: {1}, Level: {2}",
                    sNode.Id, sNode.ConnGroupId, sNode.Level);
                BuildParentChain(sNode);
                var resolvedNode = await RMFileSystemSettingsService.LoadFSNodeSettingAsync(sNode);
                if (resolvedNode.Level != (int)NodeLevel.FSFolder)
                {
                    if (resolvedNode.Id != resolvedNode.ConnGroupId)
                    {
                        resolvedNode.Name = FSConnectionDao.GetConnectionById(resolvedNode.Id).Name;
                    }
                    else
                    {
                        resolvedNode.Name = resolvedNode.FullPath;
                    }
                }
                else
                {
                    resolvedNode.Name = ExtractLeafName(resolvedNode.FullPath);
                }

                return resolvedNode;
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"Failed to build tree node. NodeId: {sNode.Id}, ConnGroupId: {sNode.ConnGroupId}, Level: {sNode.Level}",
                    ex);
                throw;
            }
        }

        private void BuildParentChain(RMFSTreeNode sNode)
        {
            var fsRoot = FSBrowerTreeService.LoadFSRoot()[0];

            switch ((NodeLevel)sNode.Level)
            {
                case NodeLevel.WebApplication:
                    AttachParent(sNode, fsRoot);
                    return;

                case NodeLevel.SiteCollection:
                    AttachParent(sNode, CreateGroupNode(sNode.ConnGroupId, fsRoot));
                    return;

                case NodeLevel.FSFolder:
                    AttachFolderAncestors(sNode, CreateGroupNode(sNode.ConnGroupId, fsRoot));
                    return;

                default:
                    logger.Warn(
                        "Unsupported node level for parent chain building. Falling back to group as parent. Level: {0}",
                        sNode.Level);
                    AttachParent(sNode, CreateGroupNode(sNode.ConnGroupId, fsRoot));
                    return;
            }
        }

       
        private void AttachFolderAncestors(RMFSTreeNode folderNode, RMFSTreeNode groupNode)
        {
            var connection = FSConnectionDao.GetParentConnectionInfo(folderNode.FullPath);
            if (connection == null)
            {
                logger.Warn("No parent connection found for folder path: {0}. Attaching folder directly under the group.",folderNode.FullPath);
                AttachParent(folderNode, groupNode);
                return;
            }

            var connectionNode = CreateConnectionNode(connection, groupNode, folderNode.ConnGroupId);

            RMFSTreeNode currentParent = connectionNode;
            string currentPath = connection.UNCPath;

            foreach (var segment in GetIntermediateFolderSegments(connection.UNCPath, folderNode.FullPath))
            {
                currentPath = ConcatPath(currentPath, segment);

                var intermediate = new RMFSTreeNode
                {
                    Id = currentPath.ToLowerInvariant().ToMd5(),
                    Name = segment,
                    Level = (int)NodeLevel.FSFolder,
                    ConnGroupId = folderNode.ConnGroupId,
                    FullPath = currentPath,
                    Parent = currentParent,
                    ParentId = currentParent.Id.ToString()
                };
                currentParent = intermediate;
            }
            AttachParent(folderNode, currentParent);
        }

        private RMFSTreeNode CreateGroupNode(Guid connGroupId, RMFSTreeNode fsRoot)
        {
            var group = FSConnectionGroupDao.GetGroupById(connGroupId);
            if (group == null)
            {
                throw new InvalidOperationException(
                    $"Connection group not found. GroupId: {connGroupId}");
            }

            return new RMFSTreeNode
            {
                Id = group.Id,
                Name = group.Name,
                Level = (int)NodeLevel.WebApplication,
                ConnGroupId = group.Id,
                FullPath = group.Name,
                Parent = fsRoot,
                ParentId = fsRoot.Id.ToString()
            };
        }

        private static RMFSTreeNode CreateConnectionNode(FSConnection connection, RMFSTreeNode groupNode, Guid connGroupId)
        {
            return new RMFSTreeNode
            {
                Id = connection.Id,
                Name = connection.Name,
                Level = (int)NodeLevel.SiteCollection,
                ConnGroupId = connGroupId,
                FullPath = connection.UNCPath,
                Parent = groupNode,
                ParentId = groupNode.Id.ToString()
            };
        }

        private static void AttachParent(RMFSTreeNode child, RMFSTreeNode parent)
        {
            child.Parent = parent;
            child.ParentId = parent.Id.ToString();
        }

        internal static IEnumerable<string> GetIntermediateFolderSegments(string connectionPath, string folderPath)
        {
            if (string.IsNullOrEmpty(connectionPath) ||
                string.IsNullOrEmpty(folderPath) ||
                !folderPath.StartsWith(connectionPath, StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<string>();
            }

            var relative = folderPath.Substring(connectionPath.Length).Trim(FSPathSeparator);
            if (string.IsNullOrEmpty(relative))
            {
                return Array.Empty<string>();
            }

            var segments = relative.Split(FSPathSeparator, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length <= 1
                ? Array.Empty<string>()
                : segments.Take(segments.Length - 1);
        }

        private static string ConcatPath(string parent, string segment) =>
            parent.EndsWith(FSPathSeparator) ? parent + segment : parent + FSPathSeparator + segment;

        private static string ExtractLeafName(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return string.Empty;
            }
            var normalized = fullPath.TrimEnd('\\', '/');
            var index = normalized.LastIndexOfAny(new[] { '\\', '/' });
            return index >= 0
                ? normalized[(index + 1)..]
                : normalized;
        }
        private RMFSTreeNode GetFSNode(string fullPath)
        {
            var group = FSConnectionGroupDao.GetGroupByName(fullPath);
            if (group != null)
            {
                return new RMFSTreeNode
                {
                    Id = group.Id,
                    ConnGroupId = group.Id,
                    Level = (int)NodeLevel.WebApplication,
                    FullPath = group.Name,
                    Name = group.Name
                };
            }

            var connection = FSConnectionDao.GetConnectionByUNCPath(fullPath);
            if (connection != null)
            {
                return new RMFSTreeNode
                {
                    Id = connection.Id,
                    ConnGroupId = connection.GroupId,
                    Level = (int)NodeLevel.SiteCollection,
                    FullPath = connection.UNCPath,
                    Name = connection.Name
                };
            }

            return null;
        }
        public async Task<RAReturnMessage> ApproveAsync(ManualApprovalActionParams param)
        {
            try
            {
                logger.Info($"Start to trigger manual approval action for action: {string.Join(",", param.NeedActionIds.Select(x => x))}");
                param.ManualFromTab = ManualApprovalTab.UnderReview;
                var invalidActionIds = await GetInvalidActionIdsAsync(param);

                if (invalidActionIds.Any())
                {
                    param.NeedActionIds = param.NeedActionIds
                        .Except(invalidActionIds)
                        .ToList();
                }
                if (!param.NeedActionIds.Any())
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"The following action IDs are invalid: {string.Join(", ", invalidActionIds)}"
                    };
                }
                var manualApprovalActionResult = await ManualApprovalService.ApproveAsync(param);
                string invalidMessage = invalidActionIds.Any() ? $" The following action IDs are invalid and were skipped: {string.Join(", ", invalidActionIds)}." : string.Empty;
                if (manualApprovalActionResult != null && manualApprovalActionResult.CompletedStatus == ActionCompletedStatus.Succeed)
                {
                    return new RAReturnMessage
                    {
                        MessageType = invalidActionIds.Any() ? RAMessageType.Exception : RAMessageType.Successful,
                        ErrorMessage = $"{manualApprovalActionResult.Message}{invalidMessage}"
                    };
                }
                if (manualApprovalActionResult != null && manualApprovalActionResult.CompletedStatus == ActionCompletedStatus.HasException)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        ErrorMessage = $"{manualApprovalActionResult.Message}{invalidMessage}"
                    };
                }
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"{manualApprovalActionResult?.Message ?? "Unknown error"}{invalidMessage}"
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger manual approval action for action: {string.Join(",", param.NeedActionIds.Select(x => x))}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger manual approval action"
                };
            }
        }

        public async Task<RAReturnMessage> RejectAsync(ManualApprovalActionParams param)
        {
            try
            {
                logger.Info($"Start to trigger manual approval action for action: {string.Join(",", param.NeedActionIds.Select(x => x))}");

                var maxExtendValidation = await ValidateRejectExtendLimitAsync(param);
                if (maxExtendValidation != null)
                {
                    return maxExtendValidation;
                }
                var invalidActionIds = await GetInvalidActionIdsAsync(param);

                if (invalidActionIds.Any())
                {
                    param.NeedActionIds = param.NeedActionIds
                        .Except(invalidActionIds)
                        .ToList();
                }
                if (!param.NeedActionIds.Any())
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = $"The following action IDs are invalid: {string.Join(", ", invalidActionIds)}"
                    };
                }
                param.ManualFromTab = ManualApprovalTab.UnderReview;
                var manualApprovalActionResult = await ManualApprovalService.RejectAsync(param);
                string invalidMessage = invalidActionIds.Any() ? $" The following action IDs are invalid and were skipped: {string.Join(", ", invalidActionIds)}." : string.Empty;
                if (manualApprovalActionResult != null && manualApprovalActionResult.CompletedStatus == ActionCompletedStatus.Succeed)
                {
                    return new RAReturnMessage
                    {
                        MessageType = invalidActionIds.Any() ? RAMessageType.Exception : RAMessageType.Successful,
                        ErrorMessage = $"{manualApprovalActionResult.Message}{invalidMessage}"
                    };
                }
                if (manualApprovalActionResult != null && manualApprovalActionResult.CompletedStatus == ActionCompletedStatus.HasException)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Exception,
                        ErrorMessage = $"{manualApprovalActionResult.Message}{invalidMessage}"
                    };
                }
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"{manualApprovalActionResult?.Message ?? "Unknown error"}{invalidMessage}"
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger manual approval action for action: {string.Join(",", param.NeedActionIds.Select(x => x))}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to trigger manual approval action"
                };
            }
        }

        private async Task<List<Guid>> GetInvalidActionIdsAsync(ManualApprovalActionParams param)
        {
            var actionIds = param.NeedActionIds.Distinct().ToList();

            var existingItems = await Repository.QueryItemsAsync(x => actionIds.Contains(x.Id));

            var validActionIds = existingItems
                .Where(x => x.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                .Select(x => x.Id)
                .ToHashSet();

            return actionIds
                .Where(id => !validActionIds.Contains(id))
                .ToList();
        }

        private async Task<RAReturnMessage> ValidateRejectExtendLimitAsync(ManualApprovalActionParams param)
        {
            if (param.FromGControl || param.NeedActionIds == null || !param.NeedActionIds.Any())return null;

            var settings = await ManualApprovalService.GetManualApprovalSettingsAsync();
            var maxDisposalExtendCount = settings.DisposalExtentionSetting.MaxDelayTimes;

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
                Filters = new List<ManualApprovalFilterDefinition>
                {
                    new() { FilterOption = ManualApprovalFilterOptions.ItemId, Value = JsonConvert.SerializeObject(param.NeedActionIds) },
                    new() { FilterOption = ManualApprovalFilterOptions.ApprovalStatus, Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Rejected, SOApproveDBStatus.Approved, SOApproveDBStatus.WaitingApprove }) }
                }
            };

            var queryResult = await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition);
            var item = queryResult?.Items?.FirstOrDefault();

            if (string.IsNullOrEmpty(item?.ManualAudit)) return null;

            try
            {
                var jsonToken = JsonConvert.DeserializeObject<JObject>(item.ManualAudit);
                var reviewAudits = jsonToken?.SelectToken("reviewAudits") as JArray;

                if (reviewAudits != null)
                {
                    var rejectCount = reviewAudits.Count(t => string.Equals(t["action"]?.Value<string>(), "Reject", StringComparison.OrdinalIgnoreCase));

                    if (rejectCount >= maxDisposalExtendCount)
                    {
                        logger.Info($"Reject blocked: record {item.Id} has reached or exceeded the maximum disposal extension count [{maxDisposalExtendCount}].");

                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "The object has reached the maximum times disposal can be extended."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error while parsing manualAudit to validate reject extend limit: {ex}");
            }

            return null;
        }
        //public RAReturnMessage RunDeleteInvalidRecordsJob()
        //{
        //    try
        //    {
        //        return ManualApprovalService.RunDeleteInvalidRecordsJob(JobRunBy.Schedule, "RM_TS_RunSchedule");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Failed to Delete Invalid Records", ex);
        //        return new RAReturnMessage
        //        {
        //            MessageType = RAMessageType.Failed,
        //            ErrorMessage = $"An error occurred while Deleting Invalid Records"
        //        };
        //    }
        //}
        public async Task<RAReturnMessage> RunExportRecordsForReviewDataJob()
        {
            try
            {
                var queryDefinition = new ManualApprovalQueryDefinition
                {
                    ManualApprovalTab = ManualApprovalTab.UnderReview,
                    NeedCalculationCount = true,
                    PageSize = 10,
                    Filters = new List<ManualApprovalFilterDefinition>
                    {
                        new ManualApprovalFilterDefinition
                        {
                            FilterOption = ManualApprovalFilterOptions.Source,
                            Value = "[2]" // only export records with source = 2 which means the record is from file system
                        }
                    },
                    Continuation = null
                };
                return await ManualApprovalService.RunExportRecordsForReviewDatasJobAsync(queryDefinition);

            }
            catch (Exception ex)
            {
                logger.Error($"Failed to export pending approve data", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"An error occurred while exporting pending approve data"
                };
            }
        }

        public async Task<RAReturnMessage> ExportHistoryData(ManualApprovalHistoryOption historyOption)
        {
            try
            {
                return ManualApprovalService.RunExportHistoryDatasJob(string.Empty, historyOption);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to export history data", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"An error occurred while exporting history data"
                };
            }
        }

        public async Task<RAReturnMessage> PauseDisposalProcess(PauseOrResumeReq req)
        {
            try
            {
                var checkConnectionsExist = FSConnectionDao.CheckAllConnectionIdsExist(req.NodeIds.Select(x => new Guid(x)).ToList());
                if (!checkConnectionsExist)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "One or more connection IDs do not exist"
                    };
                }
                req.IsPause = 1;
                return await RMMyhubServices.UpdateConnectoinIsPauseAsync(req);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger pause disposal process for parameters: {JsonConvert.SerializeObject(req)}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while pausing the disposal process"
                };
            }

        }

        public async Task<RAReturnMessage> ResumeDisposalProcess(PauseOrResumeReq req)
        {
            try
            {
                var checkConnectionsExist = FSConnectionDao.CheckAllConnectionIdsExist(req.NodeIds.Select(x => new Guid(x)).ToList());
                if (!checkConnectionsExist)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "One or more connection IDs do not exist"
                    };
                }
                req.IsPause = 0;
                return await RMMyhubServices.UpdateConnectoinIsPauseAsync(req);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to trigger resume disposal process for parameters: {JsonConvert.SerializeObject(req)}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An error occurred while resuming the disposal process"
                };
            }

        }

        private RAReturnMessage IsNodeAvailable(RMFSTreeNode node)
        {
            if (node == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"Node does not exist."
                };
            }
            if (node.EnableRecordManagement != (int)RMFSTreeNode.EnableRecordManagementSetting.Enable)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"Node is not enabled for record management."
                };
            }
            if (!node.IsActive)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"Node is not active."
                };
            }
            return null;
        }

    }

}
