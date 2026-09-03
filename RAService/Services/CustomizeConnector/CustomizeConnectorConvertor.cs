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
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector
{
    public class CustomizeConnectorConvertor
    {
        public static CustomizeConnectorInfo Convert(RMCustomizeConnectorContentSource contentSourceInfo)
        {
            if(contentSourceInfo == null)
            {
                return null;
            }
            return new CustomizeConnectorInfo
            {
                Id = contentSourceInfo.Id,
                Name = contentSourceInfo.Name,
                Flag = contentSourceInfo.Flag,
                Description = contentSourceInfo.Description,
                ColumnInfoes = contentSourceInfo.Templates.FirstOrDefault()?.Columns
                .OrderBy(item => item.Order)
                .ToList()
                .ConvertAll(item =>
                {
                    return new CustomizeConnectorColumnInfo
                    {
                        Id = item.Id,
                        Name = item.Origin == CustomizeConnectorOrigin.BuildIn ? I18NEntity.GetString(item.Name) : item.Name,
                        Type = item.Type,
                        Order = item.Order,
                        Extention = item.Extention,
                        InternalName = item.InternalName,
                        Origin = item.Origin,
                        Scope = item.Scope,
                        IsRequired = item.IsRequired,
                        IsHidden = item.IsHidden,
                    };
                }),
            };
        }

        public static RMCustomizeConnectorContentSource Convert(CustomizeConnectorInfo connectorInfo)
        {
            if (connectorInfo == null)
            {
                return null;
            }
            return new RMCustomizeConnectorContentSource
            {
                Id = connectorInfo.Id,
                Name = connectorInfo.Name,
                Description = connectorInfo.Description,
                Templates = new List<RMCustomizeConnectorTemplate>
                {
                    new RMCustomizeConnectorTemplate
                    {
                        Name = connectorInfo.Name,
                        Columns = connectorInfo.ColumnInfoes.ConvertAll(item =>
                        {
                            return new RMCustomizeConnectorColumn
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Type = item.Type,
                                Order = item.Order,
                                Extention = item.Extention,
                                InternalName = item.InternalName,
                                Origin = item.Origin,
                                Scope = item.Scope,
                                IsRequired = item.IsRequired,
                                IsHidden = item.IsHidden
                            };
                        })
                    }
                }
            };
        }
    }
}
