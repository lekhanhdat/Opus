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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Filters.MachineLearning;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.RMMachineLearning
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred: false)]
    [ValidateEnableIntelligentFilter]
    public class RMMLTermApiController : BaseApiController
    {
        #region interface
        private IRMMLTermService RMMLTermService => PlatformWindsorManager.GetService<IRMMLTermService>();
        //private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        #endregion

        [HttpPost]
        public async Task<MLTermResponseResult> AddTerms([FromBody] List<MLTermDto> dtos)
        {
            return await RMMLTermService.AddTerms(dtos);
        }

        [HttpPost]
        public async Task<MLTermResponseResult> DeleteTerms([FromBody] List<Guid> ids)
        {
            return await RMMLTermService.DeleteTerms(ids);
        }

        [HttpPost]
        [ValidateEnablelZeroShotFilter]
        public Task<MLTermResponseResult> UpdateDescription([FromBody] MLTermDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Description) )
            {
                return Task.FromResult(new MLTermResponseResult
                {
                    HasError = true,
                    ErrorMsg = "Description do not allow null"
                });
            }
            return RMMLTermService.UpdateDescription(dto);
        }

        [HttpPost]
        public Task<MLTermResponseResult> SetAutoApply([FromBody] SetAutoApplyParam param)
        {
            return RMMLTermService.SetAutoApplyAsync(param.TermId, param.AutoApply);
        }

        [HttpPost]
        public MLTermResponseResult LoadTerms([FromBody] MLTermQueryParam param)
        {
            return RMMLTermService.LoadTerms(param);
        }

        [HttpPost]
        public MLTermResponseResult LoadUsageTerms([FromBody] UsageTermQueryParam param)
        {
            return RMMLTermService.LoadUsageTerms(param);
        }

        //[HttpPost]
        //public RAReturnMessage StartTrain()
        //{
        //    return RMMLTermService.StartTrainingJob();
        //}

        [HttpPost]
        public ValidateDefaultTermResult ValidateDefaultTerm([FromBody] List<Guid> termIds)
        {
            return RMMLTermService.ValidateDefaultTerm(termIds);
        }

        [HttpPost]
        public Task<string> GetLastUpdatedTime()
        {
            return RMMLTermService.GetLastUpdatedTimeAsync();
        }

        [HttpPost]
        public TrainModeInfo GetTrainModeInfo()
        {
            //TODO Query Mode table
            return null;
        }

        [HttpPost]
        public async Task<RAReturnMessage> CheckPredictionJobRunning([FromBody] int action)
        {
            return await RMMLTermService.CheckPredictionJobRunning(action);
        }

        [HttpGet]
        [ValidateEnableZeroShotPermission]
        public int GetCurrentMode()
        {
            return RMMLTermService.GetCurrentMode();
        }

        [HttpPost]
        [ValidateEnableZeroShotPermission]
        public async Task<RAReturnMessage> SwitchMode([FromBody]int mode)
        {
            return await RMMLTermService.SwitchModeAsync(mode);
        }

        //[HttpPost]
        //public bool SetUpQuickStart()
        //{
        //    return KeyValueService.Save(
        //        new RMNameValueDto
        //        {
        //            Name = KeyNameCollection.MachineLearning,
        //            Value = bool.TrueString,
        //            Type = RMNameValueType.IsSetupQuickStartIntelligent
        //        });
        //}

        //[HttpPost]
        //public bool IsSetUpQuickStart()
        //{
        //    return KeyValueService.Get(KeyNameCollection.MachineLearning, RMNameValueType.IsSetupQuickStartIntelligent) != null;
        //}
        [HttpPost]
        [ValidateEnablelMachineLearningFilter]
        public Task<RAReturnMessage> StartMLJob()
        {
            return RMMLTermService.StartTrainingJobAsync();
        }

#if DEBUG
        [HttpPost]
        public RAReturnMessage StartAnalyseJob()
        {
            return RMMLTermService.StartAnalyseJob();
        }
#endif
    }
}
