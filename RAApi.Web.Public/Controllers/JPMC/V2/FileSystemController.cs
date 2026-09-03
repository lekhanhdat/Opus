using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Public.Common.Requests;
using AvePoint.RA.Api.Web.Public.Common.Response;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("file-system")]
    [MultiGeoValidIPFilter]
    public class FileSystemController : RAWebApiBase
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(FileSystemController));

        private IRetriveDataServices RetriveDataServices => PlatformWindsorManager.GetService<IRetriveDataServices>();
        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpGet("metadata")]
        public async Task<IActionResult> GetFSMetadata([FromQuery] string fullPath)
        {
            try
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    return this.BadRequestApi("Invalid parameters. FullPath is required.");
                }

                var metadata = await RetriveDataServices.GetFSMetadataAsync(new FSMetadataParam
                {
                    FullPath = fullPath
                });

                if (metadata == null)
                {
                    return this.NotFoundApi("FS metadata not found, or the File System Dashboard Data job has not been run yet");
                }

                return this.OkApi(metadata);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get FS metadata", ex);
                return this.InternalServerErrorApi("An error occurred while getting the FS metadata");
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetFSFileCountByCategory(
            [FromQuery] string fullPath,
            [FromQuery] FSMetadataCategory category,
            [FromQuery] string classCode,
            [FromQuery] long startTime,
            [FromQuery] long endTime)
        {
            try
            {
                if (string.IsNullOrEmpty(fullPath) || !Enum.IsDefined(typeof(FSMetadataCategory), category))
                {
                    return this.BadRequestApi("Invalid parameters. FullPath and Category are required.");
                }
                if (category == FSMetadataCategory.ClassCode && string.IsNullOrEmpty(classCode))
                {
                    return this.BadRequestApi("ClassCode is required when Category is ClassCode.");
                }
                if (category != FSMetadataCategory.ClassCode)
                {
                    if (startTime <= 0)
                    {
                        return this.BadRequestApi("StartTime must be greater than 0.");
                    }
                    if (endTime <= 0)
                    {
                        return this.BadRequestApi("EndTime must be greater than 0.");
                    }
                    if (startTime > endTime)
                    {
                        return this.BadRequestApi("Invalid time range. StartTime cannot be later than EndTime.");
                    }
                }

                var data = await RetriveDataServices.GetFSFileCountByCategory(new FSMetadataByCategoryParam
                {
                    FullPath = fullPath,
                    Category = category,
                    ClassCode = classCode,
                    StartTime = startTime,
                    EndTime = endTime
                });

                if (data == null)
                {
                    return this.NotFoundApi("Cannot find data for the specified parameters. Need run file system dashboard first");
                }

                return this.OkApi(data);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get FS file count by category", ex);
                return this.InternalServerErrorApi("An error occurred while getting the FS file count by category");
            }
        }

        [HttpPatch("record-management/disable")]
        public async Task<IActionResult> DisableRecordManagement([FromBody] FileSystemJobNodeRequest param)
        {
            var requestParam = param?.ToContract();
            param = null;
            var nodeParam = requestParam;
            if (nodeParam == null || nodeParam.NodeId == Guid.Empty)
            {
                return this.BadRequestApi("Invalid node Id for disabling record management, the Id must be filled in the params");
            }
            if (string.IsNullOrEmpty(nodeParam.FullPath) || nodeParam.ConnectionGroupId == Guid.Empty || nodeParam.Level == 0)
            {
                return this.BadRequestApi("Invalid parameters for disabling record management, the FullPath, ConnectionGroupId, and Level must be filled in the params");
            }

            var validate = TriggerJobServices.IsNodeEligible(nodeParam);
            if (validate != null)
            {
                return this.FromReturnMessage(validate);
            }

            try
            {
                var fsNode = new RMFSTreeNode
                {
                    Id = nodeParam.NodeId,
                    ConnGroupId = nodeParam.ConnectionGroupId,
                    Level = nodeParam.Level,
                    FullPath = nodeParam.FullPath
                };
                var fsNodeSetting = await TriggerJobServices.BuildTreeNodeAsync(fsNode);
                fsNodeSetting.EnableRecordManagement = (int)RMFSTreeNode.EnableRecordManagementSetting.Disable;
                return this.FromReturnMessage(await FileSystemSettingsService.SaveFSGeneralSetting4JPMC(fsNodeSetting));
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while disabling record management", ex);
                return this.InternalServerErrorApi("An error occurred while disabling record management");
            }
        }

        [HttpPost("dashboard/sync")]
        public async Task<IActionResult> RunFSDashboardJob([FromBody] FileSystemDashboardSyncRequest param)
        {
            var requestParam = param?.ToContract();
            if (requestParam != null && requestParam.Level != (int)NodeLevel.SiteCollection && requestParam.Level != (int)NodeLevel.FSFolder)
            {
                return this.BadRequestApi("Invalid node level.");
            }

            var (isValid, value) = await ValidMultiGeoConnectionDataAsync(requestParam?.PartitionKeyId);
            if (!isValid)
            {
                return this.FromReturnMessage(value);
            }

            return this.FromReturnMessage(await TriggerJobServices.RunFSDashboardJobAsync(requestParam));
        }

        [HttpPost("sync")]
        public async Task<IActionResult> RunDataSyncJob([FromBody] FileSystemJobNodeRequest param)
        {
            var requestParam = param?.ToContract();
            var validationResult = ValidateRunJobNodeParam(requestParam);
            if (validationResult != null)
            {
                return this.FromReturnMessage(validationResult);
            }

            var group = FSConnectionGroupDao.GetGroupById(requestParam.ConnectionGroupId);
            if (group == null)
            {
                return this.BadRequestApi("Invalid connection group ID.");
            }

            return this.FromReturnMessage(await TriggerJobServices.RunDataSyncJobAsync(requestParam));
        }

        [HttpPost("enforce-rule")]
        public async Task<IActionResult> RunDisposalJob([FromBody] FileSystemJobNodeRequest param)
        {
            var requestParam = param?.ToContract();
            var validationResult = ValidateRunJobNodeParam(requestParam);
            if (validationResult != null)
            {
                return this.FromReturnMessage(validationResult);
            }

            var group = FSConnectionGroupDao.GetGroupById(requestParam.ConnectionGroupId);
            if (group == null)
            {
                return this.BadRequestApi("Invalid connection group ID.");
            }

            return this.FromReturnMessage(await TriggerJobServices.RunDisposalJobAsync(requestParam));
        }

        [HttpPost("enforce-rule/pause")]
        public async Task<IActionResult> PauseDisposalProcess([FromBody] DisposalProcessRequest req)
        {
            var requestParam = req?.ToContract();
            var (isValid, message) = await IsValidMultiGeoDataCenterForNodes(requestParam?.NodeIds);
            if (!isValid)
            {
                return this.BadRequestApi(message);
            }

            return this.FromReturnMessage(await RouteMainDCApiActionAsync(requestParam,
                RACommonUtility.MultiGeo.MultiGeoOperationType.MainDCJpmcTriggerJobPauseDisposalProcess,
                RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobPauseDisposalProcess,
                TriggerJobServices.PauseDisposalProcess));
        }

        [HttpPost("enforce-rule/resume")]
        public async Task<IActionResult> ResumeDisposalProcess([FromBody] DisposalProcessRequest req)
        {
            var requestParam = req?.ToContract();
            var (isValid, message) = await IsValidMultiGeoDataCenterForNodes(requestParam?.NodeIds);
            if (!isValid)
            {
                return this.BadRequestApi(message);
            }

            return this.FromReturnMessage(await RouteMainDCApiActionAsync(requestParam,
                RACommonUtility.MultiGeo.MultiGeoOperationType.MainDCJpmcTriggerJobResumeDisposalProcess,
                RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobResumeDisposalProcess,
                TriggerJobServices.ResumeDisposalProcess));
        }

        [HttpPost("enforce-rule/by-class-code")]
        public async Task<IActionResult> RunDisposalByClassCode([FromBody] DisposalByClassCodeRequest param)
        {
            var requestParam = param?.ToContract();
            var validationResult = ValidateRunJobNodeParam(requestParam?.JobNodeParam);
            if (validationResult != null)
            {
                return this.FromReturnMessage(validationResult);
            }
            if (requestParam?.Terms == null || !requestParam.Terms.Any())
            {
                return this.BadRequestApi("Terms are required.");
            }

            var group = FSConnectionGroupDao.GetGroupById(requestParam.JobNodeParam.ConnectionGroupId);
            if (group == null)
            {
                return this.BadRequestApi("Invalid connection group ID.");
            }

            return this.FromReturnMessage(await TriggerJobServices.RunDisposalByClassCodeAsync(requestParam));
        }

        [HttpPost("rcc-report/download")]
        public async Task<IActionResult> DownloadRCCReport([FromBody] RccReportDownloadRequest param)
        {
            var requestParam = param?.ToContract();
            var validateResult = await ValidateRCCParam(requestParam);
            if (validateResult != null)
            {
                return this.FromReturnMessage(validateResult);
            }

            return this.FromReturnMessage(await TriggerJobServices.RunDownloadRCCReportJobAsync(requestParam));
        }

        [HttpPost("apply-class-code")]
        public async Task<IActionResult> RunApplyClassCodeJob([FromBody] ApplyClassCodeRequest param)
        {
            var requestParam = param?.ToContract();
            var validationResult = ValidateApplyClassCodeParam(requestParam);
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var (isPathValidate, dCInternalName) = await FSConnectionGroupDao.CheckPathAndGetDCInternalName(requestParam?.FullPath);
                if (!isPathValidate)
                {
                    return this.BadRequestApi("The path does not exist or incorrect format as a connection fullpath or connection group name.");
                }
                if (string.IsNullOrEmpty(dCInternalName))
                {
                    if (!MultiGeoDataCenterService.IsMainDC())
                    {
                        return this.BadRequestApi("Invalid DataCenter.");
                    }
                }
                else if (!(RMSSOHelper.CurrentDCName?.Equals(dCInternalName) ?? false))
                {
                    return this.BadRequestApi("Invalid DataCenter.");
                }
            }
            if (validationResult != null)
            {
                return this.FromReturnMessage(validationResult);
            }

            return this.FromReturnMessage(await TriggerJobServices.RunApplyClassCodeAsync(requestParam));
        }

        private static RAReturnMessage ValidateApplyClassCodeParam(ApplyClassCodeParam param)
        {
            if (param == null)
            {
                return FailedResult("Invalid parameters.");
            }
            if (string.IsNullOrWhiteSpace(param.ClassCode))
            {
                return FailedResult("ClassCode is required.");
            }
            if (string.IsNullOrWhiteSpace(param.CountryCode))
            {
                return FailedResult("CountryCode is required.");
            }
            if (param.RetentionType <= 0)
            {
                return FailedResult("RetentionType is required.");
            }
            if (string.IsNullOrWhiteSpace(param.FullPath))
            {
                return FailedResult("FullPath is required.");
            }
            if (string.IsNullOrWhiteSpace(param.TermId))
            {
                return FailedResult("TermId is required.");
            }
            return null;
        }

        private static RAReturnMessage ValidateRunJobNodeParam(FSJobNodeParam param)
        {
            if (param == null)
            {
                return FailedResult("Invalid parameters.");
            }
            if (param.NodeId == Guid.Empty)
            {
                return FailedResult("NodeId is required.");
            }
            if (param.ConnectionGroupId == Guid.Empty)
            {
                return FailedResult("ConnectionGroupId is required.");
            }
            if (param.Level <= 0)
            {
                return FailedResult("Level is required.");
            }
            if (string.IsNullOrWhiteSpace(param.FullPath))
            {
                return FailedResult("FullPath is required.");
            }
            return null;
        }

        private async Task<RAReturnMessage> ValidateRCCParam(RCCReportRequestPublic param)
        {
            if (param == null)
            {
                return FailedResult("Invalid parameters.");
            }
            if (param.Nodes == null || !param.Nodes.Any())
            {
                return FailedResult("No nodes provided for RCC report generation.");
            }
            if (param.TimeRange == null)
            {
                return FailedResult("Time range is required for RCC report generation.");
            }
            if (param.TimeRange.PresetType == 0 && param.TimeRange.StartDate >= param.TimeRange.EndDate)
            {
                return FailedResult("Invalid time range for RCC report generation. Start date must be earlier than end date.");
            }
            if (param.ConnectionId == Guid.Empty)
            {
                return FailedResult("Connection ID is required for RCC report generation.");
            }
            if (param.ConnGroupId == Guid.Empty)
            {
                return FailedResult("Connection group ID could not be determined for the provided connection ID.");
            }
            if (param.Level != (int)NodeLevel.FSFolder && param.Level != (int)NodeLevel.FSFile && param.Level != (int)NodeLevel.SiteCollection)
            {
                return FailedResult("Invalid node level for RCC report generation. Only Connection, FSFolder and FSFile levels are supported.");
            }

            var group = FSConnectionGroupDao.GetGroupById(param.ConnGroupId);
            if (group == null)
            {
                return FailedResult("Invalid connection group ID.");
            }
            if (!await ValidMultiGeoTargetDCAsync(param.ConnGroupId, group))
            {
                return FailedResult("Invalid DataCenter.");
            }

            var connection = FSConnectionDao.GetConnectionById(param.ConnectionId);
            if (connection == null)
            {
                return FailedResult("Invalid connection ID.");
            }

            return null;
        }

        private async Task<(bool isValid, string message)> IsValidMultiGeoDataCenterForNodes(List<string> nodeIds)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                if (nodeIds == null || nodeIds.Count < 1)
                {
                    return (false, "No nodeIds provided for ValidMMultiGeoConnectionData validation.");
                }
                var dcInternalNames = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionIdsAsync(nodeIds);
                if (dcInternalNames == null || dcInternalNames.Count < 1)
                {
                    return (false, "No DC internal names found for the provided nodeIds.");
                }
                if (dcInternalNames.Count > 1)
                {
                    return (false, "Does not support processing data that is not located in the current data center.");
                }

                var connectionDCInternalName = dcInternalNames.FirstOrDefault().Key;
                if (string.IsNullOrEmpty(connectionDCInternalName))
                {
                    var isMainDC = MultiGeoDataCenterService.IsMainDC();
                    return (isMainDC, isMainDC ? string.Empty : "Invalid DataCenter.");
                }
                if (!(RMSSOHelper.CurrentDCName?.Equals(connectionDCInternalName) ?? false))
                {
                    return (false, "Invalid DataCenter.");
                }
            }

            return (true, string.Empty);
        }

        private async Task<(bool isValid, RAReturnMessage value)> ValidMultiGeoConnectionDataAsync(string partitionKeyId)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                if (string.IsNullOrEmpty(partitionKeyId) || !Guid.TryParse(partitionKeyId, out var connectionId))
                {
                    return (false, FailedResult("Since the account has Multi-Geo enabled, we need to include the guid partitionKeyId in the request."));
                }
                var fsConnection = FSConnectionDao.GetConnectionById(connectionId);
                if (fsConnection == null)
                {
                    return (false, FailedResult("Cannot find the connection by partitionKeyId, or the connection does not belong to any connection group."));
                }
                var dCInternalName = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
                if (string.IsNullOrEmpty(dCInternalName))
                {
                    if (!MultiGeoDataCenterService.IsMainDC())
                    {
                        return (false, FailedResult("Invalid DataCenter."));
                    }
                }
                else if (!(RMSSOHelper.CurrentDCName?.Equals(dCInternalName) ?? false))
                {
                    return (false, FailedResult("Invalid DataCenter."));
                }
            }

            return (true, null);
        }

        private async Task<bool> ValidMultiGeoTargetDCAsync(Guid fsConnectionGroupId, FSConnectionGroup group = null)
        {
            group ??= FSConnectionGroupDao.GetGroupById(fsConnectionGroupId);
            if (group == null)
            {
                return false;
            }
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return true;
            }
            if (string.IsNullOrEmpty(group.DCInternalName))
            {
                return MultiGeoDataCenterService.IsMainDC();
            }
            return RMSSOHelper.CurrentDCName?.Equals(group.DCInternalName) ?? false;
        }

        private static RAReturnMessage FailedResult(string message)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = message
            };
        }
    }
}

