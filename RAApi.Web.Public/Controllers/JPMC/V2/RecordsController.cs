using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMPublicAPI.JPMC.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Public.Common.Requests;
using AvePoint.RA.Api.Web.Public.Common.Response;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("records")]
    [MultiGeoValidIPFilter]
    public class RecordsController : RAWebApiBase
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(RecordsController));

        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();
        private IRetriveDataServices RetriveDataServices => PlatformWindsorManager.GetService<IRetriveDataServices>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpPost("batch-approve")]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
        public async Task<IActionResult> Approve([FromBody] ManualApprovalActionRequest param)
        {
            var requestParam = param?.ToContract();
            if (requestParam == null)
            {
                return this.BadRequestApi("Invalid parameters.");
            }
            if (requestParam.NeedActionIds == null || requestParam.NeedActionIds.Count == 0)
            {
                return this.BadRequestApi("No action IDs provided.");
            }

            //var (isValid, value) = await ValidMultiGeoConnectionDataAsync(requestParam.PartitionKeyId);
            //if (!isValid)
            //{
            //    return this.FromReturnMessage(value);
            //}

            return this.FromReturnMessage(await TriggerJobServices.ApproveAsync(requestParam));
        }

        [HttpPost("batch-reject")]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
        public async Task<IActionResult> Reject([FromBody] ManualApprovalActionRequest param)
        {
            var requestParam = param?.ToContract();
            if (requestParam == null)
            {
                return this.BadRequestApi("Invalid parameters.");
            }
            if (requestParam.NeedActionIds == null || requestParam.NeedActionIds.Count == 0)
            {
                return this.BadRequestApi("No action IDs provided.");
            }

            //var (isValid, value) = await ValidMultiGeoConnectionDataAsync(requestParam.PartitionKeyId);
            //if (!isValid)
            //{
            //    return this.FromReturnMessage(value);
            //}

            return this.FromReturnMessage(await TriggerJobServices.RejectAsync(requestParam));
        }

        [HttpGet]
        public async Task<IActionResult> GetRecordItemInformation(
            [FromQuery] Guid connectionId,
            [FromQuery] Guid connectionGroupId,
            [FromQuery] string fullPathConnection,
            [FromQuery] string continuationToken,
            [FromQuery] int pageSize = 10,
            [FromQuery] int level = 2100,
            [FromQuery] bool isDesc = false)
        {
            try
            {
                var queryModel = new RecordItemQueryDefinition
                {
                    ConnectionId = connectionId,
                    ConnectionGroupId = connectionGroupId,
                    FullPathConnection = fullPathConnection,
                    ContinuationToken = continuationToken,
                    PageSize = pageSize,
                    Level = level,
                    IsDesc = isDesc
                };

                var (isValid, errorMessage) = IsValidRecordItemQuery(queryModel);
                if (!isValid)
                {
                    return this.BadRequestApi(errorMessage);
                }

                return this.OkApi(await RetriveDataServices.GetRecordItemInformation(queryModel));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get record item information", ex);
                return this.InternalServerErrorApi("An error occurred while getting the record item information");
            }
        }

        [HttpGet("pending-enforce-rule")]
        public async Task<IActionResult> GetPendingDisposalItem(
            [FromQuery] Guid connectionId,
            [FromQuery] Guid connectionGroupId,
            [FromQuery] string fullPathConnection,
            [FromQuery] string continuationToken,
            [FromQuery] int pageSize = 10,
            [FromQuery] int level = 2100,
            [FromQuery] bool isDesc = false)
        {
            try
            {
                var queryModel = new RecordItemQueryDefinition
                {
                    ConnectionId = connectionId,
                    ConnectionGroupId = connectionGroupId,
                    FullPathConnection = fullPathConnection,
                    ContinuationToken = continuationToken,
                    PageSize = pageSize,
                    Level = level,
                    IsDesc = isDesc
                };

                var (isValid, errorMessage) = IsValidRecordItemQuery(queryModel);
                if (!isValid)
                {
                    return this.BadRequestApi(errorMessage);
                }

                return this.OkApi(await RetriveDataServices.GetPendingDisposalItem(queryModel));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get pending disposal item information", ex);
                return this.InternalServerErrorApi("An error occurred while getting the pending disposal item information");
            }
        }

        [HttpPost("pending-enforce-rule/export")]
        public async Task<IActionResult> ExportPendingDisposal()
        {
            return this.FromReturnMessage(await TriggerJobServices.RunExportRecordsForReviewDataJob());
        }

        [HttpPost("approval-history/export")]
        public async Task<IActionResult> ExportApprovalHistory([FromBody] ApprovalHistoryExportRequest historyOption)
        {
            try
            {
                var requestParam = historyOption?.ToContract();
                if (requestParam == null)
                {
                    return this.BadRequestApi("Invalid parameters.");
                }
                if (requestParam.ExportType == (int)TimeRange.Custom)
                {
                    if (requestParam.StartDateTime == 0 || requestParam.EndDateTime == 0)
                    {
                        return this.BadRequestApi("Start date and end date are required for custom time range.");
                    }
                    if (requestParam.StartDateTime > requestParam.EndDateTime)
                    {
                        return this.BadRequestApi("Start date cannot be later than end date.");
                    }
                }

                return this.FromReturnMessage(await TriggerJobServices.ExportHistoryData(new ManualApprovalHistoryOption
                {
                    LatestExportType = requestParam.ExportType,
                    CustomDate = new ManualHistoryCustomDataTime
                    {
                        StartDateTime = new DateTime(requestParam.StartDateTime),
                        EndDateTime = new DateTime(requestParam.EndDateTime)
                    },
                    ServiceUrl = string.Empty,
                    Filters =
                    [
                        new ManualApprovalFilterDefinition
                        {
                            FilterOption = ManualApprovalFilterOptions.Source,
                            Value = "[2]"
                        }
                    ],
                }));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to export approval history", ex);
                return this.InternalServerErrorApi("An error occurred while running the create and destruction job");
            }
        }

        private (bool IsValid, string ErrorMessage) IsValidRecordItemQuery(RecordItemQueryDefinition queryModel)
        {
            if (queryModel == null)
            {
                return (false, "Query model is required.");
            }
            if (queryModel.Level != 2100 && queryModel.Level != 2200)
            {
                return (false, "Only support for levels 2100 and 2200.");
            }

            var connection = FSConnectionDao.GetConnectionById(queryModel.ConnectionId);
            if (connection == null)
            {
                return (false, "ConnectionId is invalid.");
            }
            if (connection.UNCPath != queryModel.FullPathConnection)
            {
                return (false, "FullPathConnection does not match the connection's UNCPath.");
            }
            if (connection.GroupId != queryModel.ConnectionGroupId)
            {
                return (false, "ConnectionGroupId does not match the connection's GroupId.");
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

