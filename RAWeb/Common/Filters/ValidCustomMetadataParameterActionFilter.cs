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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidCustomMetadataParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidCustomMetadataParameterActionFilter));

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMCustomMetadataColumnDao RMCustomMetadataColumnDao => PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();

        private string action;

        public ValidCustomMetadataParameterActionFilter()
        {
        }

        public ValidCustomMetadataParameterActionFilter(string action)
        {
            this.action = action;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (!RMKeyValueDao.TryGetBoolValue("RunDisposalInRecords", out var value))
            {
                logger.Info("Access denied for teams");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
            }

            if (action == "SaveOrUpdateCustomColumns")
            {
                if (actionContext.ActionArguments.Values.FirstOrDefault() is not List<CustomMetadataColumnInfo> customColumns || customColumns.Count == 0)
                {
                    logger.Info("Custom columns are null or empty.");
                    actionContext.Result = new ObjectResult("Invalid custom columns") { StatusCode = (int)HttpStatusCode.BadRequest };
                    return;
                }

                foreach (var column in customColumns)
                {
                    if (string.IsNullOrWhiteSpace(column.ColumnName) || !Enum.IsDefined(typeof(CustomColumnType), column.ColumnType))
                    {
                        actionContext.Result = new ObjectResult("Invalid custom column") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return;
                    }
                }

                var duplicateColumns = customColumns.GroupBy(c => c.ColumnName)
                                                    .Where(g => g.Count() > 1)
                                                    .Select(g => g.Key)
                                                    .ToList();
                if (duplicateColumns.Count != 0)
                {
                    actionContext.Result = new ObjectResult("Duplicate custom column names: " + string.Join(", ", duplicateColumns)) { StatusCode = (int)HttpStatusCode.BadRequest };
                    return;
                }

                var usedColumns = await RMCustomMetadataColumnDao.GetInUsedCustomMetadataColumnsAsync();
                //if the customColumns contains any column that is already in use,and the columnName and column type be changed, return error
                var inUseColumns = customColumns.Where(c => usedColumns.Any(u => u.UniqueId == c.UniqueId && (u.ColumnName != c.ColumnName || u.ColumnType != c.ColumnType))).ToList();
                if (inUseColumns.Count > 0)
                {
                    var errorMessage = "The following custom columns are already in use and cannot be modified: " + string.Join(", ", inUseColumns.Select(c => c.ColumnName));
                    actionContext.Result = new ObjectResult(errorMessage) { StatusCode = (int)HttpStatusCode.BadRequest };
                    return;
                }
            }

            if (action == "SaveOrUpdateCustomMetadatas")
            {
                var customMetadataInfo = actionContext.ActionArguments.Values.FirstOrDefault() as CustomIndexMetadataInfo;
                if (customMetadataInfo.IsEnableCustomIndexMetadata)
                {
                    if(customMetadataInfo == null || customMetadataInfo.CustomIndexMetadataDtos == null)
                    {
                        logger.Info("Custom metadatas are null or empty.");
                        actionContext.Result = new ObjectResult("Invalid custom metadatas") { StatusCode = (int)HttpStatusCode.BadRequest };
                        return;
                    }

                    if (customMetadataInfo.CustomIndexMetadataDtos.Count > 0)
                    {
                        foreach (var metadata in customMetadataInfo.CustomIndexMetadataDtos)
                        {
                            if (string.IsNullOrWhiteSpace(metadata.SourceColumnName) || string.IsNullOrWhiteSpace(metadata.TargetColumnName) || metadata.TargetColumnId == Guid.Empty)
                            {
                                actionContext.Result = new ObjectResult("Invalid custom metadata") { StatusCode = (int)HttpStatusCode.BadRequest };
                                return;
                            }
                            if (!Enum.IsDefined(typeof(CustomColumnType), metadata.ColumnType))
                            {
                                actionContext.Result = new ObjectResult("Invalid custom column type") { StatusCode = (int)HttpStatusCode.BadRequest };
                                return;
                            }
                        }

                        // Check for duplicate source column names, target column names, and target column IDs
                        var duplicateSourceColumns = customMetadataInfo.CustomIndexMetadataDtos.GroupBy(c => c.SourceColumnName)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateSourceColumns.Count > 0)
                        {
                            actionContext.Result = new ObjectResult("Duplicate source column names: " + string.Join(", ", duplicateSourceColumns)) { StatusCode = (int)HttpStatusCode.BadRequest };
                            return;
                        }

                        var duplicateTargetColumns = customMetadataInfo.CustomIndexMetadataDtos.GroupBy(c => c.TargetColumnName)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateTargetColumns.Count > 0)
                        {
                            actionContext.Result = new ObjectResult("Duplicate target column names: " + string.Join(", ", duplicateTargetColumns)) { StatusCode = (int)HttpStatusCode.BadRequest };
                            return;
                        }

                        var duplicateTargetColumnIds = customMetadataInfo.CustomIndexMetadataDtos.GroupBy(c => c.TargetColumnId)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateTargetColumnIds.Count > 0)
                        {
                            actionContext.Result = new ObjectResult("Duplicate target column IDs: " + string.Join(", ", duplicateTargetColumnIds)) { StatusCode = (int)HttpStatusCode.BadRequest };
                            return;
                        }
                    }
                }
            }
        }
    }
}
