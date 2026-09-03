using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMPublicAPI.JPMC.Model;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V1
{
    [Route("api/v1/retrieve-data/[action]")]
    [MultiGeoValidIPFilter]
    public class RetriveDataApiController : RAWebApiBase
    {
        private IRetriveDataServices RetriveDataServices => PlatformWindsorManager.GetService<IRetriveDataServices>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private RALogger logger = RALogger.GetInstance(typeof(RetriveDataApiController));
        [HttpPost]
        public async Task<IActionResult> GetJobReport([FromBody] JobReportParam param)
        {
            try
            {
                // Validate parameters
                if (param == null)
                {
                    return BadRequest("Parameters are required.");
                }
                if (!param.StartTime.HasValue)
                {
                    return BadRequest("StartTime is required.");
                }
                var report = await RetriveDataServices.GetJobReportAsync(param);
                if (report == null)
                {
                    return NotFound("Job report not found.");
                }
                return Ok(report);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get job report", ex);
                return StatusCode(500, "An error occurred while getting the job report");
            }
        }

        // [HttpPost]
        // public async Task<IActionResult> GetJobDetail([FromBody] Guid jobId)
        // {
        //     try
        //     {
        //         if (jobId == Guid.Empty)
        //         {
        //             return BadRequest("JobId is required.");
        //         }
        //         //var report = await RetriveDataServices.GetJobReportByCategoryAsync(param);
        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         logger.Error("Failed to get job report by category", ex);
        //         return StatusCode(500, "An error occurred while getting the job report by category");
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> GetFSMetadata([FromBody] FSMetadataParam param)
        {
            try
            {
                if (param == null || string.IsNullOrEmpty(param.FullPath))
                {
                    return BadRequest("Invalid parameters. FullPath is required.");
                }
                var metadata = await RetriveDataServices.GetFSMetadataAsync(param);

                if (metadata == null)
                {
                    return NotFound("FS metadata not found, or the File System Dashboard Data job has not been run yet");
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get FS metadata", ex);
                return StatusCode(500, "An error occurred while getting the FS metadata");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetFSFileCountByCategory([FromBody] FSMetadataByCategoryParam param)
        {
            try
            {
                if (param == null || string.IsNullOrEmpty(param.FullPath) || !Enum.IsDefined(typeof(FSMetadataCategory), param.Category))
                {
                    return BadRequest("Invalid parameters. FullPath and Category are required.");
                }
                if (param.Category == FSMetadataCategory.ClassCode && string.IsNullOrEmpty(param.ClassCode))
                {
                    return BadRequest("ClassCode is required when Category is ClassCode.");
                }
                if (param.Category != FSMetadataCategory.ClassCode)
                {
                    if (param.StartTime <= 0)
                    {
                        return BadRequest("StartTime must be greater than 0.");
                    }
                    if (param.EndTime <= 0)
                    {
                        return BadRequest("EndTime must be greater than 0.");
                    }
                    if (param.StartTime > param.EndTime)
                    {
                        return BadRequest("Invalid time range. StartTime cannot be later than EndTime.");
                    }
                }
                var data = await RetriveDataServices.GetFSFileCountByCategory(param);
                if (data == null)
                {
                    return NotFound("Cannot find data for the specified parameters. Need run file system dashboard first");
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get FS file count by category", ex);
                return StatusCode(500, "An error occurred while getting the FS file count by category");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetJobDetails([FromBody] JMDetailsQuery queryModel)
        {
            try
            {
                var (isValid, errorMessage) = IsValidJobDetailsQuery(queryModel);
                if (!isValid)
                    return BadRequest(errorMessage);
                var details = await RetriveDataServices.GetJobDetails(queryModel);
                if (details == null)
                {
                    return NotFound("Job details not found.");
                }
                return Ok(details);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get job details", ex);
                return StatusCode(500, "An error occurred while getting the job details");
            }
        }

        [HttpPost]
        public async Task <IActionResult> GetRecordItemInformation([FromBody] RecordItemQueryDefinition queryModel)
        {
            try
            {
                var (isValid, errorMessage) = IsValidRecordItemQuery(queryModel);
                if (!isValid)
                {
                    return BadRequest(errorMessage);
                }
                return Ok(await RetriveDataServices.GetRecordItemInformation(queryModel));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get record item information", ex);
                return StatusCode(500, "An error occurred while getting the record item information");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPendingDisposalItem([FromBody] RecordItemQueryDefinition queryModel)
        {
            try             
            {
                var (isValid, errorMessage) = IsValidRecordItemQuery(queryModel);
                if (!isValid)
                {
                    return BadRequest(errorMessage);
                }
                return Ok(await RetriveDataServices.GetPendingDisposalItem(queryModel));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get pending disposal item information", ex);
                return StatusCode(500, "An error occurred while getting the pending disposal item information");
            }
        }

        private (bool IsValid, string ErrorMessage) IsValidRecordItemQuery(RecordItemQueryDefinition queryModel)
        {
            if (queryModel == null)
            {
                return (false, "Query model is required.");
            }
            if ((queryModel.Level != 2100 && queryModel.Level != 2200))
            {
                return (false, "Only support for levels 2100 and 2200.");
            }
            var connection = FSConnectionDao.GetConnectionById(queryModel.ConnectionId);
            if (connection == null)
            {
                return (false, "ConnectionId is invalid.");
            }
            if (connection != null && connection.UNCPath != queryModel.FullPathConnection)
            {
                return (false, "FullPathConnection does not match the connection's UNCPath.");
            }
            if(connection != null && connection.GroupId != queryModel.ConnectionGroupId)
            {
                return (false, "ConnectionGroupId does not match the connection's GroupId.");
            }
            return (true, string.Empty);
        }
        private (bool IsValid, string ErrorMessage) IsValidJobDetailsQuery(JMDetailsQuery queryModel)
        {
            if (queryModel == null)
            {
                return (false, "Query model is required.");
            }
            if (queryModel.PageSize <= 0 || queryModel.CurrentPage <= 0)
            {
                return (false, "PageSize and CurrentPage must be greater than 0.");
            }
            if (string.IsNullOrEmpty(queryModel.JobID))
            {
                return (false, "JobID is required.");
            }
            if (queryModel.JobType <= 0)
            {
                return (false, "JobType must be greater than 0.");
            }
            return (true, string.Empty);
        }
    }
}