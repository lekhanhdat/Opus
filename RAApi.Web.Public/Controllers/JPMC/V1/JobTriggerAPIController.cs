using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V1
{

    [Route("api/v1/job-trigger/[action]")]
    [MultiGeoValidIPFilter]
    public class JobTriggerAPIController : RAWebApiBase
    {
        #region Interfaces/DAOs

        private IRMFileSystemSettingsService _RMFileSystemSettingsService;

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService(ref _RMFileSystemSettingsService);
        private static IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();
        private IRMFileSystemBrowserService FSBrowerTreeService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();
        private RALogger logger = RALogger.GetInstance(typeof(JobTriggerAPIController));

        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        #endregion

        #region Controllers

        [HttpPost]
        public async Task<RAReturnMessage> RunDataSyncJobAsync([FromBody] FSJobNodeParam param)
        {
            var validationResult = ValidateRunJobNodeParam(param);
            if (validationResult != null)
            {
                return validationResult;
            }
            var group = FSConnectionGroupDao.GetGroupById(param.ConnectionGroupId);
            if (group == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection group ID."
                };
            }
            return await TriggerJobServices.RunDataSyncJobAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunDisposalJobAsync([FromBody] FSJobNodeParam param)
        {
            var validationResult = ValidateRunJobNodeParam(param);
            if (validationResult != null)
            {
                return validationResult;
            }
            var group = FSConnectionGroupDao.GetGroupById(param.ConnectionGroupId);
            if (group == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection group ID."
                };
            }
            return await TriggerJobServices.RunDisposalJobAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunDisposalByClassCodeAsync([FromBody] FSDisposalClassCodeParam param)
        {
            var validationResult = ValidateRunJobNodeParam(param.JobNodeParam);
            if (validationResult != null)
            {
                return validationResult;
            }
            if (param.Terms == null || !param.Terms.Any())
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Terms are required."
                };
            }

            var group = FSConnectionGroupDao.GetGroupById(param.JobNodeParam.ConnectionGroupId);
            if (group == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection group ID."
                };
            }
            return await TriggerJobServices.RunDisposalByClassCodeAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> DownloadRCCReportAsync([FromBody] RCCReportRequestPublic param)
        {
            var validateResult = await ValidateRCCParam(param);
            if (validateResult != null)
            {
                return validateResult;
            }
            return await TriggerJobServices.RunDownloadRCCReportJobAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> StopJobsAsync([FromBody] List<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "No job IDs provided."
                };
            }
            return await TriggerJobServices.StopJobsAsync(ids);
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
        public async Task<RAReturnMessage> ApproveAsync([FromBody] ManualApprovalActionParams param)
        {
            if (param == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid parameters."
                };
            }
            if (!param.NeedActionIds.Any())
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "No action IDs provided."
                };
            }
            (bool isValid, RAReturnMessage value) = await ValidMMultiGeoConnectionData(param.PartitionKeyId);
            if (!isValid)
            {
                return value;
            }
            return await TriggerJobServices.ApproveAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunExportRecordsForReviewDataJobAsync()
        {
            return await TriggerJobServices.RunExportRecordsForReviewDataJob();
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunExportHistoryDataJobAsync([FromBody] ManualApprovalHistory historyOption)
        {
            try
            {
                // validate if the job can be triggered
                if(historyOption == null)
                {   
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Invalid parameters."
                    };
                }
                if(historyOption.ExportType == (int)TimeRange.Custom)
                {
                    if(historyOption.StartDateTime == 0 || historyOption.EndDateTime == 0)
                    {
                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "Start date and end date are required for custom time range."
                        };
                    }
                    if(historyOption.StartDateTime > historyOption.EndDateTime)
                    {
                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "Start date cannot be later than end date."
                        };
                    }
                }
                return await TriggerJobServices.ExportHistoryData(new ManualApprovalHistoryOption
                {
                    LatestExportType = historyOption.ExportType,
                    CustomDate = new ManualHistoryCustomDataTime
                    {
                        StartDateTime = new DateTime(historyOption.StartDateTime),
                        EndDateTime = new DateTime(historyOption.EndDateTime)
                    },
                    ServiceUrl = string.Empty,
                    Filters = new List<ManualApprovalFilterDefinition>
                    {
                        new ManualApprovalFilterDefinition
                        {
                            FilterOption = ManualApprovalFilterOptions.Source,
                            Value = "[2]" // only export records with source = 2 which means the record is from file system
                        }
                    },
                });
            }
            catch (Exception ex)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = $"An error occurred while running the create and destruction job"
                };
            }
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
        public async Task<RAReturnMessage> RejectAsync([FromBody] ManualApprovalActionParams param)
        {
            if (param == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid parameters."
                };
            }
            if (!param.NeedActionIds.Any())
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "No action IDs provided."
                };
            }
            (bool isValid, RAReturnMessage value) = await ValidMMultiGeoConnectionData(param.PartitionKeyId);
            if (!isValid)
            {
                return value;
            }

            return await TriggerJobServices.RejectAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunApplyClassCodeJobAsync([FromBody] ApplyClassCodeParam param)
        {
            var validationResult = ValidateApplyClassCodeParam(param);
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var (isPathValidate, dCInternalName) = await FSConnectionGroupDao.CheckPathAndGetDCInternalName(param.FullPath);
                if (isPathValidate == false)
                {
                    return ValidationError($"The path does not exist or incorrect format as a connection fullpath or connection group name.");
                }
                if (string.IsNullOrEmpty(dCInternalName))
                {
                    if (!MultiGeoDataCenterService.IsMainDC())
                    {
                        return ValidationError("Invalid DataCenter.");
                    }
                }
                else
                {
                    if (!(RMSSOHelper.CurrentDCName?.Equals(dCInternalName) ?? false))
                    {
                        return ValidationError("Invalid DataCenter.");
                    }
                }
            }
            if (validationResult != null)
            {
                return validationResult;
            }
            return await TriggerJobServices.RunApplyClassCodeAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> RunFSDashboardJobAsync([FromBody] FileSystemMyhubSelectedNodeDto param)
        {
            if(param != null && (param.Level != (int)NodeLevel.SiteCollection && param.Level != (int)NodeLevel.FSFolder))
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid node level."
                };
            }

            (bool isValid, RAReturnMessage value) = await ValidMMultiGeoConnectionData(param.PartitionKeyId);
            if (!isValid)
            {
                return value;
            }

            return await TriggerJobServices.RunFSDashboardJobAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> PauseDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            //var validationResult = ValidateResumeDisposalProcessParams(pauseParameters);
            //if (validationResult != null)
            //{
            //    return validationResult;
            //}
            var (isValid, message) = await IsValidMultiGeoDataCenterForNodes(req.NodeIds);
            if(!isValid)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = message
                };
            }
            return await RouteMainDCApiActionAsync(req, RACommonUtility.MultiGeo.MultiGeoOperationType.MainDCJpmcTriggerJobPauseDisposalProcess,
                RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobPauseDisposalProcess,
                TriggerJobServices.PauseDisposalProcess);
        }

        [HttpPost]
        public async Task<RAReturnMessage> ResumeDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            //var validationResult = ValidateResumeDisposalProcessParams(req.PauseParameters);
            //if (validationResult != null)
            //{
            //    return validationResult;
            //}

            var (isValid, message) = await IsValidMultiGeoDataCenterForNodes(req.NodeIds);
            if(!isValid)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = message
                };
            }
            return await RouteMainDCApiActionAsync(req, RACommonUtility.MultiGeo.MultiGeoOperationType.MainDCJpmcTriggerJobResumeDisposalProcess,
                RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobResumeDisposalProcess,
                TriggerJobServices.ResumeDisposalProcess);
        }

        #endregion

        #region Helpers

        private async Task<(bool isValid, string message)> IsValidMultiGeoDataCenterForNodes(List<string> nodeIds)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                if (nodeIds == null || nodeIds.Count < 1)
                {
                    logger.Warn("No nodeIds provided for ValidMMultiGeoConnectionData validation.");
                    return (false, "No nodeIds provided for ValidMMultiGeoConnectionData validation.");
                }
                var dcInternalNames = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionIdsAsync(nodeIds);

                if (dcInternalNames == null || dcInternalNames.Count < 1)
                {
                    logger.Warn("No DC internal names found for the provided nodeIds.");
                    return (false, "No DC internal names found for the provided nodeIds.");
                }
                if (dcInternalNames.Count > 1)
                {
                    logger.Warn("Multiple DC internal names found for the provided nodeIds.");
                    return (false, "Does not support processing data that is not located in the current data center.");
                }

                var connectionDCInternalName = dcInternalNames.FirstOrDefault().Key;
                if (string.IsNullOrEmpty(connectionDCInternalName))
                {
                    bool isMainDC = MultiGeoDataCenterService.IsMainDC();
                    return (isMainDC, isMainDC ? string.Empty : "Invalid DataCenter.");
                }
                else
                {
                    if (!(RMSSOHelper.CurrentDCName?.Equals(connectionDCInternalName) ?? false))
                    {
                        logger.Warn($"Current DC name {RMSSOHelper.CurrentDCName} does not match the connection's DC internal name {connectionDCInternalName}.");
                        return (false, "Invalid DataCenter.");
                    }
                }
            }
            return (true, string.Empty);
        }

        private async Task<(bool flowControl, RAReturnMessage value)> ValidMMultiGeoConnectionData(string partitionKeyId)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                if (string.IsNullOrEmpty(partitionKeyId) || !Guid.TryParse(partitionKeyId, out var connectionId))
                {
                    return (flowControl: false, value: new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Since the account has Multi-Geo enabled, we need to include the guid partitionKeyId in the request."
                    });
                }
                var fsConnection = FSConnectionDao.GetConnectionById(connectionId);
                if (fsConnection == null)
                {
                    return (flowControl: false, value: new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Cannot find the connection by partitionKeyId, or the connection does not belong to any connection group."
                    });
                }
                var dCInternalName = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
                if (string.IsNullOrEmpty(dCInternalName))
                {
                    if (!MultiGeoDataCenterService.IsMainDC())
                    {
                        return (flowControl: false, value: new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "Invalid DataCenter."
                        });
                    }
                }
                else
                {
                    if (!(RMSSOHelper.CurrentDCName?.Equals(dCInternalName) ?? false))
                    {
                        return (flowControl: false, value: new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "Invalid DataCenter."
                        });
                    }
                }

            }

            return (flowControl: true, value: null);
        }

        private RAReturnMessage ValidateApplyClassCodeParam(ApplyClassCodeParam param)
        {
            if (param == null)
            {
                return ValidationError("Invalid parameters.");
            }
                      
            if (string.IsNullOrWhiteSpace(param.ClassCode))
            {
                return ValidationError("ClassCode is required.");
            }

            if (string.IsNullOrWhiteSpace(param.CountryCode))
            {
                return ValidationError("CountryCode is required.");
            }

            if (param.RetentionType <= 0)
            {
                return ValidationError("RetentionType is required.");
            }

            if (string.IsNullOrWhiteSpace(param.FullPath))
            {
                return ValidationError("FullPath is required.");
            }
            if (string.IsNullOrWhiteSpace(param.TermId))
            {
                return ValidationError("TermId is required.");
            }
            return null;
        }

        private static RAReturnMessage ValidationError(string message)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = message
            };
        }

        private RAReturnMessage ValidateRunJobNodeParam(FSJobNodeParam param)
        {
            if (param == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid parameters."
                };
            }
            if (param.NodeId == Guid.Empty)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "NodeId is required."
                };
            }
            if (param.ConnectionGroupId == Guid.Empty)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "ConnectionGroupId is required."
                };
            }
            if (param.Level <= 0)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Level is required."
                };
            }
            if (string.IsNullOrWhiteSpace(param.FullPath))
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "FullPath is required."
                };
            }
            return null;
        }

        private async Task<RAReturnMessage> ValidateRCCParam(RCCReportRequestPublic param)
        {
            if (param == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid parameters."
                };
            }
            if (param.Nodes == null || !param.Nodes.Any())
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "No nodes provided for RCC report generation."
                };
            }
            if (param.TimeRange == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Time range is required for RCC report generation."
                };
            }
            if (param.TimeRange.PresetType == 0 && (param.TimeRange.StartDate >= param.TimeRange.EndDate))
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid time range for RCC report generation. Start date must be earlier than end date."
                };
            }
            if (param.ConnectionId == Guid.Empty)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Connection ID is required for RCC report generation."
                };
            }
            if (param.ConnGroupId == Guid.Empty)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Connection group ID could not be determined for the provided connection ID."
                };
            }
            if (param.Level != (int)NodeLevel.FSFolder && param.Level != (int)NodeLevel.FSFile && param.Level != (int)NodeLevel.SiteCollection)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid node level for RCC report generation. Only Connection, FSFolder and FSFile levels are supported."
                };
            }
            // check exist
            var group = FSConnectionGroupDao.GetGroupById(param.ConnGroupId);
            if (group == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection group ID."
                };
            }
            if (!(await ValidMultiGeoTargetDCAsync(param.ConnGroupId, group)))
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid DataCenter."
                };
            }
            var connection = FSConnectionDao.GetConnectionById(param.ConnectionId);
            if (connection == null)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Invalid connection ID."
                };
            }
            return null;
        }

        private async Task<bool> ValidMultiGeoTargetDCAsync(Guid fsConnectionGroupId, FSConnectionGroup group = null)
        {
            if (group == null)
            {
                group = FSConnectionGroupDao.GetGroupById(fsConnectionGroupId);
                if (group == null) return false;
            }
            if (!(await MultiGeoSettingService.IsEnableMultiGeoFeature())) return true;
            if (string.IsNullOrEmpty(group.DCInternalName))
            {
                return MultiGeoDataCenterService.IsMainDC();
            }
            return RMSSOHelper.CurrentDCName?.Equals(group.DCInternalName) ?? false;
        }

        #endregion
    }
}
