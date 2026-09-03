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
using AvePoint.Api.Web.ApiControllers;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [Route("api/FSMasterIndex/[action]")]
    public class FSMasterIndexController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(FSMasterIndexController));
        private IFSMasterIndexService _FSMasterIndexService;
        private IFSMasterIndexService FSMasterIndexService => PlatformWindsorManager.GetService(ref _FSMasterIndexService);
        private IRMFileSystemRegisterService _FSRegisterService;
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService(ref _FSRegisterService);

        [HttpPost]
        public string InsertIntoFSMasterIndex([FromBody]string fSMasterIndexContractJson)
        {
            try
            {
                FSMasterIndexContract indexDto = SerializerHelper.DeserializeByJsonConvert<FSMasterIndexContract>(fSMasterIndexContractJson);
                return FSMasterIndexService.InsertIntoFSMasterIndex(indexDto);
            }
            catch(Exception e)
            {
                logger.Error($@"Fail insert into fs master index,ex:{e}");
                return null;
            }
        }
        [HttpPost]
        public string GetConnectionMasterWithSubInfosList([FromBody] string connectionId)
        {
            try
            {
                return SerializerHelper.SerializeByJsonSerializer(FSMasterIndexService.GetConnectionMasterWithSubInfosList(connectionId));
            }
            catch (Exception e)
            {
                logger.Error($@"Fail insert into fs master index,ex:{e}");
                return null;
            }
        }
        [HttpPost]
        public string GetConnectionNameById([FromBody] string connectionId)
        {
            try
            {
                return FSRegisterService.GetConnectionNameByIdAsync(new Guid(connectionId));
            }
            catch (Exception e)
            {
                logger.Error($@"Fail GetConnectionNameById index,ex:{e}");
                return null;
            }
        }
        [HttpPost]
        public string GetMasterIndexBySubjobId([FromBody]string subJobId)
        {
            return SerializerHelper.SerializeByJsonSerializer(FSMasterIndexService.GetMasterIndexBySubjobId(subJobId));
        }
        [HttpPost]
        public void DeleteFSMasterIndex([FromBody] string masterIndexInfo)
        {
            FSMasterIndexService.DeleteFSMasterIndex(SerializerHelper.DeserializeByJsonSerializer<FSMasterIndexContract>(masterIndexInfo));
        }
    }
}
