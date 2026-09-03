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
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Converters
{
    public class AzureFileShareConnectionGroupConverter
    {
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        public static List<AzureFileShareConnectionGroupItem> Convert(List<RMAzureFileShareConnectionGroup> connectionGroups, GeneralSettingModel gls)
        {
            if(connectionGroups == null)
            {
                return new List<AzureFileShareConnectionGroupItem>();
            }

            return connectionGroups.ConvertAll(item => Convert(item, gls));
        }

        public static AzureFileShareConnectionGroupItem Convert(RMAzureFileShareConnectionGroup connectionGroup, GeneralSettingModel gls)
        {
            if(connectionGroup == null)
            {
                return null;
            }

            return new AzureFileShareConnectionGroupItem
            {
                Id = connectionGroup.Id,
                Name = connectionGroup.Name,
                Description = connectionGroup.Description,
                Created = GeneralSettingService.ConvertTiksToDateTime(gls, connectionGroup.Created, true).SimplifyFormatTime,
                Modified = GeneralSettingService.ConvertTiksToDateTime(gls, connectionGroup.Modified, true).SimplifyFormatTime,
                CreatedBy = connectionGroup.CreatedBy,
                ModifiedBy = connectionGroup.ModifiedBy,
                Connections = AzureFileShareConnectionConverter.Convert(connectionGroup.Connections, gls)
            };
        }

        public static RMAzureFileShareConnectionGroup Convert(AzureFileShareConnectionGroupItem connectionGroupItem, GeneralSettingModel gls)
        {
            if(connectionGroupItem == null)
            {
                return null;
            }

            return new RMAzureFileShareConnectionGroup
            {
                Id = connectionGroupItem.Id,
                Name = connectionGroupItem.Name,
                Description = connectionGroupItem.Description,
                //Created = GeneralSettingService.ConvertDateTimeToUtc(connectionGroupItem.Created, gls).Ticks,
                //Modified = GeneralSettingService.ConvertDateTimeToUtc(connectionGroupItem.Modified, gls).Ticks,
                CreatedBy = connectionGroupItem.CreatedBy,
                ModifiedBy = connectionGroupItem.ModifiedBy,
                Connections = AzureFileShareConnectionConverter.Convert(connectionGroupItem.Connections, gls)
            };
        }
    }
}
