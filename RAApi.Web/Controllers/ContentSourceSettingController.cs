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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Filters;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RMAgentApiPerformanceLogger]
    public class ContentSourceSettingController : RAWebApiBase
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITaxonomyService taxonomyService;
        public ContentSourceSettingController(ITaxonomyService taxonomyService)
        {
            this.taxonomyService = taxonomyService;
        }
        [HttpPost("column/{mailbox}")]
        public ActionResult<RMColumnInfo> GetColumnInfo([FromBody] string mailbox)
        {
            var columnInfo = new RMColumnInfo()
            {
                Id = TermColumnInfo.WellKnowTermColumnId,
                UniqueId = TermColumnInfo.WellKnowTermColumnGuid,
                Name = TermColumnInfo.WellKnowTermColumnName
            };
            try
            {
                Logger.Info("access api get columInfo");
                var term = taxonomyService.GetDefaultTermByMailBox(mailbox);
                if (term != null)
                {
                    columnInfo.DefaultTermId = term.UniqueId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get column info{0}", ex.ToString());
                return BadRequest(new ErrorInfo((int)RestStateCode.GetTermColumnInfo, ex.Message));
            }
            return columnInfo;
        }
    }
}
