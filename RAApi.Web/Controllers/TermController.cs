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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common.Utils;
using Cloud.sdk.Data.Records.Classification;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RMAgentApiPerformanceLogger]
    public class TermController : RAWebApiBase
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ITaxonomyService _TaxonomyService;

        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);

        [HttpPost("listnode")]
        [ProducesResponseType(typeof(TermTreeNode), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<TermTreeNode>>> ListNode([FromBody] QueryTermParam queryTermInfo)
        {
            List<TermTreeNode> reulst = new List<TermTreeNode>();
            
            Logger.Info("begin to access api get term tree pager");
            var pager = queryTermInfo.Pager;
            int pIndex = pager.PageIndex = pager.PageIndex == 0 ? pager.PageIndex : pager.PageIndex - 1;

            RMTermType type = (RMTermType)queryTermInfo.Level;

            List<RMTermInfo> terms = await TaxonomyService.GetTaxonomyTreeDataAsync(type, queryTermInfo.TermId.ToString(), pIndex, pager.ItemsPerPage);
            reulst = terms.ConvertAll(t => DataContractConvertUtil.Convert2TermTreeNode(t));
            
            return reulst;
        }

        [HttpPost("listnode/{mailbox}")]
        [ProducesResponseType(typeof(TermTreeNode), (int)HttpStatusCode.OK)]
        public ActionResult<TermTreeNode> GetTermTreeByMailBox([FromBody] string mailbox)
        {
            TermTreeNode termInfo = new();
            
            Logger.Info("access api get term tree by mailBox");
            var treeNode = TaxonomyService.GetTermTreeByMailBox(mailbox);

            termInfo = DataContractConvertUtil.ConvertTermTreeNode(treeNode);
            
            return termInfo;
        }
    }
}
