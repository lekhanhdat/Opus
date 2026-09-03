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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AvePoint.RA.Web.Controllers.Discovery.PlanProfile
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryPlanProfileApiController : BaseApiController
    {
        private readonly IRMDiscoveryPlanProfileService _planProfileService = PlatformWindsorManager.GetService<IRMDiscoveryPlanProfileService>();

        [HttpPost]
        public async Task<RMDiscoveryPlanProfileInfo> GetPlanProfileById([FromBody] int id)
        {
            try
            {
                return await _planProfileService.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting Plan Profile with id {id}. Error: {ex}");
                throw;
            }
        }

        [HttpPost]
        public async Task<RMDiscoveryPlanProfilePageInfo> GetPlanProfilesPaged([FromBody] RMDiscoveryPlanProfilePageRequest request)
        {
            try
            {
                if (request == null || request.PageIndex < 1 || request.PageSize < 1)
                {
                    throw new ArgumentException("Invalid pagination parameters.");
                }

                return await _planProfileService.GetPagedAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting paged Plan Profiles. Error: {ex}");
                throw;
            }
        }

        [HttpPost]
        public async Task<List<string>> GetAllSelectedSiteByProfileId([FromBody] int profileId)
        {
            try
            {
                if (profileId <= 0)
                {
                    return new List<string>();
                }

                return await _planProfileService.GetAllSelectedSiteByProfileIdAsync(profileId);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting all selected sites for PlanProfileId {profileId}. Error: {ex}");

                return new List<string>();
            }
        }

        [HttpPost]
        [ValidDiscoveryPlanProfileParameterActionFilter("SaveOrUpdatePlanProfile")]
        public async Task<RAReturnMessage> SaveOrUpdatePlanProfile([FromBody] RMDiscoveryPlanProfileInfo profileInfo)
        {
            try
            {
                if (profileInfo.Id == 0)
                {
                    int newId = await _planProfileService.CreateAsync(profileInfo);
                    return new RAReturnMessage { MessageType = RAMessageType.Successful, Extsion1 = newId };
                }

                bool updated = await _planProfileService.UpdateAsync(profileInfo);
                return new RAReturnMessage
                {
                    MessageType = updated ? RAMessageType.Successful : RAMessageType.Failed,
                    Extsion1 = profileInfo.Id
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving Plan Profile. Error: {ex}");
                return new RAReturnMessage { MessageType = RAMessageType.Failed };
            }
        }

        [HttpPost]
        public async Task<RAReturnMessage> DeletePlanProfiles([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "No Plan Profiles selected for deletion." };
            }

            try
            {
                bool deleted = await _planProfileService.DeleteAsync(ids);
                return new RAReturnMessage
                {
                    MessageType = deleted ? RAMessageType.Successful : RAMessageType.Failed
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error deleting Plan Profiles with ids {string.Join(",", ids)}. Error: {ex}");
                return new RAReturnMessage { MessageType = RAMessageType.Failed };
            }
        }

        [HttpPost]
        public async Task<RAReturnMessage> TriggerDalJobAsync([FromBody] RMDiscoveryTriggerDalJob triggerDalJob)
        {
            return await _planProfileService.TriggerDalJob(triggerDalJob, JobRunBy.Control);
        }

        [HttpPost]
        public async Task<RMRemoteSiteCollectionPageInfo> GetSiteCollectionsInfo([FromBody] RMRemoteSiteCollectionPageRequest request)
        {
            try
            {
                if (request == null || request.PageIndex < 1 || request.PageSize < 1)
                {
                    throw new ArgumentException("Invalid pagination parameters.");
                }

                return await _planProfileService.GetAllSiteCollectionNodesAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting paged site collections. Error: {ex}");
                throw;
            }
        }

        [HttpPost]
        public async Task<RMRemoteSiteCollectionPageInfo> GetMappedSitesPaged([FromBody] RMRemoteSiteCollectionPageRequest request)
        {
            try
            {
                if (request == null || request.PageIndex < 1 || request.PageSize < 1)
                {
                    throw new ArgumentException("Invalid pagination parameters.");
                }

                return await _planProfileService.GetMappedSitesPagedAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting mapped sites for PlanProfileId {request?.PlanProfileId}. Error: {ex}");
                throw;
            }
        }


        [HttpGet]
        public async Task<bool> GetPlanChatDisplayConfiguration()
        {
            return await _planProfileService.GetPlanChatDisplayConfiguration();
        }

        [HttpPost]
        public async Task<bool> EnableAIMessageAsync()
        {
            return await _planProfileService.EnableAIMessageAsync();
        }

        [HttpGet]
        public async Task<RMDiscoveryTriggerDalJob> GetConfigurationInfoAsync()
        {
            return await _planProfileService.GetConfigurationInfoAsync();
        }
    }
}
