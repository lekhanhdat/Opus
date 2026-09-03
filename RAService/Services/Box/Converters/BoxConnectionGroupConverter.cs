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
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.Box.Converters
{
    public static class BoxConnectionGroupConverter
    {
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static GeneralSettingModel gls => GeneralSettingService.GetGeneralSettingAsync().Result;

        public static BoxConnectionGroupItem ConvertToItem(this RMBoxConnectionGroup connectionGroup)
        {
            return connectionGroup != null ? new BoxConnectionGroupItem
            {
                Id = connectionGroup.Id,
                Name = connectionGroup.Name,
                Description = connectionGroup.Description,
                Created = GeneralSettingService.ConvertTiksToDateTime(gls, connectionGroup.Created, true).SimplifyFormatTime,
                Modified = GeneralSettingService.ConvertTiksToDateTime(gls, connectionGroup.Modified, true).SimplifyFormatTime,
                CreatedBy = connectionGroup.CreatedBy,
                ModifiedBy = connectionGroup.ModifiedBy,
                Connections = BoxConnectionConverter.Convert(connectionGroup.Connections)
            } : null;
        }

        public static RMBoxConnectionGroup ConvertToEntity(this BoxConnectionGroupItem connectionGroupItem)
        {
            return connectionGroupItem != null ? new RMBoxConnectionGroup
            {
                Id = connectionGroupItem.Id,
                Name = connectionGroupItem.Name,
                Description = connectionGroupItem.Description,
                CreatedBy = connectionGroupItem.CreatedBy,
                ModifiedBy = connectionGroupItem.ModifiedBy,
                Connections = BoxConnectionConverter.Convert(connectionGroupItem.Connections)
            } : null;
        }

        public static BoxConnectionGroupViewModel ConvertToViewModel(this BoxConnectionGroupItem connectionGroupItem)
        {
            return connectionGroupItem != null ? new BoxConnectionGroupViewModel
            {
                Id = connectionGroupItem.Id,
                Name = connectionGroupItem.Name,
                Description = connectionGroupItem.Description,
                CreatedBy = connectionGroupItem.CreatedBy,
                ModifiedBy = connectionGroupItem.ModifiedBy,
                Connections = connectionGroupItem.Connections.ConvertAll(item => item.ConvertToViewModel()),
                Created = connectionGroupItem.Created,
                Modified = connectionGroupItem.Modified
            } : null;
        }
    }
}