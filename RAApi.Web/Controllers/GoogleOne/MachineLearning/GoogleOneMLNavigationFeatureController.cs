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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.MachineLearning
{
    [Route("api/googleone/machinelearning/navigation")]
    public class GoogleOneMLNavigationFeatureController : GoogleOneApiBaseController
    {
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        
        private readonly IRMMLTermService _mlClassificationService = PlatformWindsorManager.GetService<IRMMLTermService>();

        [HttpGet("enabled")]
        public Task<bool> CheckEnabledZeroShotAI()
        {
            return Task.Run(()=> _keyValueDao.EnableZeroShotFeature());
        }
        
        [HttpGet("currentmode")]
        public Task<int> GetCurrentMode()
        {
            return Task.Run(()=>_mlClassificationService.GetCurrentMode());
        }

        [HttpPost("switchmode/{mode:int}")]
        public async Task<bool> SwitchMode(int mode)
        {
            var returnMessage = await _mlClassificationService.SwitchModeAsync(mode);
            return returnMessage.MessageType == RAMessageType.Successful;
        }
    }
}
