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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidPhysicalBulkUpdateParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidPhysicalBulkUpdateParameterFilter));
        public ValidPhysicalBulkUpdateParameterFilter()
        {

        }

        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj != null)
            {
                ExplorerDao ExplorerDao = new ExplorerDao();
                var dto = parmObj as BuldUpdatePhysicalDto;
                if (dto != null)
                {
                    var records = ExplorerDao.GetRecordByIds(dto.RecordIds);
                    var typeCount = records.Select(r => r.NodeType).Distinct().Count();
                    if (typeCount > 1)
                    {
                        logger.Warn($"Physical object types are not the same.");
                        actionContext.Result = new ObjectResult("Selected Invalid Physical Objects") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }

                    var templateCount = records.Select(r => r.TemplateId).Distinct().Count();
                    if (templateCount > 1)
                    {
                        logger.Warn($"Physical object templates are not the same.");
                        actionContext.Result = new ObjectResult("Selected Invalid Physical Objects") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }

                    foreach (var record in records)
                    {
                        if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                        {
                            logger.Warn($"Can not edit destoryed physical object.");
                            actionContext.Result = new ObjectResult("Selected Invalid Physical Objects") { StatusCode = (int)HttpStatusCode.Forbidden };
                            break;
                        }
                    }

                    if (dto.MetaInfo != null && dto.MetaInfo.Keys != null)
                    {
                        if (DefaultColumnIDs.HideForBulkUpdateIDs.Any(c => dto.MetaInfo.Keys.Contains(c)))
                        {
                            logger.Warn($"Contains can not bulk edit column.");
                            actionContext.Result = new ObjectResult("Edit Column Invalid Physical Objects") { StatusCode = (int)HttpStatusCode.Forbidden };
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}