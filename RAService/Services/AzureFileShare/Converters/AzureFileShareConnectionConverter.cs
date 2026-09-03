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
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.AzureFileShare.Model.Api;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Converters
{
    public class AzureFileShareConnectionConverter
    {

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static RMAesEncryptorWrapper AesEncryptorWrapper => new();

        public static List<AzureFileShareConnectionItem> Convert(List<RMAzureFileShareConnection> connections, GeneralSettingModel gsl)
        {
            if (connections == null)
            {
                return new List<AzureFileShareConnectionItem>();
            }
            return connections.ConvertAll(item => Convert(item, gsl));
        }

        public static AzureFileShareConnectionItem Convert(RMAzureFileShareConnection connection, GeneralSettingModel gsl)
        {
            if (connection == null)
            {
                return null;
            }

            return new AzureFileShareConnectionItem
            {
                Id = connection.Id,
                Name = connection.Name,
                Description = connection.Description,
                AccessEndPoint = connection.AccessEndPoint,
                FileShareName = connection.FileShareName,
                AccountName = connection.AccountName,
                AccountKey = AesEncryptorWrapper.CompatibleDecrypt(connection.AccountKey),
                Created = GeneralSettingService.ConvertTiksToDateTime(gsl, connection.Created, true).SimplifyFormatTime,
                Modified = GeneralSettingService.ConvertTiksToDateTime(gsl, connection.Modified, true).SimplifyFormatTime,
                CreatedBy = connection.CreatedBy,
                ModifiedBy = connection.ModifiedBy,
                ConnectionGroupId = connection.ConnectionGroupId,
                ConnectionGroup = AzureFileShareConnectionGroupConverter.Convert(connection.ConnectionGroup, gsl),
            };
        }

        public static List<RMAzureFileShareConnection> Convert(List<AzureFileShareConnectionItem> connectionItems, GeneralSettingModel gls)
        {
            if (connectionItems == null)
            {
                return new List<RMAzureFileShareConnection>();
            }

            return connectionItems.ConvertAll(item => Convert(item, gls));
        }

        public static RMAzureFileShareConnection Convert(AzureFileShareConnectionItem connectionItem, GeneralSettingModel gls)
        {
            if (connectionItem == null)
            {
                return null;
            }

            return new RMAzureFileShareConnection
            {
                Id = connectionItem.Id,
                Name = connectionItem.Name.Trim(),
                Description = connectionItem.Description.Trim(),
                AccessEndPoint = connectionItem.AccessEndPoint.Trim(),
                FileShareName = connectionItem.FileShareName.Trim(),
                AccountName = connectionItem.AccountName.Trim(),
                AccountKey = AesEncryptorWrapper.Encrypt(connectionItem.AccountKey.Trim()),
                //Created = GeneralSettingService.ConvertDateTimeToUtc(connectionItem.Created, gls).Ticks,
                //Modified = GeneralSettingService.ConvertDateTimeToUtc(connectionItem.Modified, gls).Ticks,
                CreatedBy = connectionItem.CreatedBy,
                ModifiedBy = connectionItem.ModifiedBy,
                ConnectionGroupId = connectionItem.ConnectionGroupId,
                ConnectionGroup = AzureFileShareConnectionGroupConverter.Convert(connectionItem.ConnectionGroup, gls)
            };
        }

        public static AzureFileShareConnectionInfo ConvertToConnectionInfo(AzureFileShareConnectionItem connectionItem)
        {
            return new AzureFileShareConnectionInfo
            {
                ConnectionId = connectionItem.Id,
                AccessEndPoint = connectionItem.AccessEndPoint,
                AccountName = connectionItem.AccountName,
                AccountKey = connectionItem.AccountKey,
                FileShareName = connectionItem.FileShareName,
            };
        }
    }
}
