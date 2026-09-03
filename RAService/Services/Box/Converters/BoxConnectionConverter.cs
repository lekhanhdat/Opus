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
using AvePoint.RA.RACommonUtility.Encryption;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Box.Converters
{
    public static class BoxConnectionConverter
    {
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private static GeneralSettingModel gls => GeneralSettingService.GetGeneralSettingAsync().Result;


        public static List<BoxConnectionItem> Convert(List<RMBoxConnection> connections)
        {
            return connections?.ConvertAll(item =>ConvertToItem(item)) ?? new List<BoxConnectionItem>();
        }

        public static BoxConnectionItem ConvertToItem(this RMBoxConnection connection)
        {
            return connection != null ? new BoxConnectionItem
            {
                Id = connection.Id,
                Name = connection.Name,
                Description = connection.Description,
                AuthenticationType = (BoxAuthenticationType)connection.AuthenticationType,
                EnterpriseId = connection.EnterpriseId,
                ClientId = connection.ClientId,
                ClientSecret = connection.AuthenticationType == (int)BoxAuthenticationType.UserAuth ? AesEncryptorWrapper.Decrypt(connection.ClientSecret) : connection.ClientSecret,
                EmailAddress = connection.EmailAddress,
                JsonFileName = connection.JsonFileName,
                JsonFileContent = connection.AuthenticationType == (int)BoxAuthenticationType.ServerAuth ? AesEncryptorWrapper.Decrypt(connection.JsonFileContent) : connection.JsonFileContent,
                Created = GeneralSettingService.ConvertTiksToDateTime(gls, connection.Created, true).SimplifyFormatTime,
                Modified = GeneralSettingService.ConvertTiksToDateTime(gls, connection.Modified, true).SimplifyFormatTime,
                CreatedBy = connection.CreatedBy,
                ModifiedBy = connection.ModifiedBy,
                ConnectionGroupId = connection.ConnectionGroupId,
                RedirectUrl = connection.RedirectUrl,
            } : null;
        }

        public static RMBoxConnection ConvertToEntity(this BoxConnectionItem connectionItem)
        {
            return connectionItem != null ? new RMBoxConnection
            {
                Id = connectionItem.Id,
                Name = connectionItem.Name,
                Description = connectionItem.Description,
                AuthenticationType = (int)connectionItem.AuthenticationType,
                EnterpriseId = connectionItem.EnterpriseId,
                ClientId = connectionItem.ClientId,
                ClientSecret = connectionItem.AuthenticationType == BoxAuthenticationType.UserAuth && !string.IsNullOrEmpty(connectionItem.ClientSecret) ? AesEncryptorWrapper.Encrypt(connectionItem.ClientSecret) : connectionItem.ClientSecret,
                EmailAddress = connectionItem.EmailAddress,
                JsonFileName = connectionItem.JsonFileName,
                JsonFileContent = connectionItem.AuthenticationType == BoxAuthenticationType.ServerAuth && connectionItem.JsonFileContent != null? AesEncryptorWrapper.Encrypt(connectionItem.JsonFileContent) : connectionItem.JsonFileContent,
                CreatedBy = connectionItem.CreatedBy,
                ModifiedBy = connectionItem.ModifiedBy,
                ConnectionGroupId = connectionItem.ConnectionGroupId,
                RedirectUrl = connectionItem.RedirectUrl,
                AccessToken = connectionItem.AccessToken,
                RefreshToken = connectionItem.RefreshToken,
            } : null;
        }

        public static List<RMBoxConnection> Convert(List<BoxConnectionItem> connectionItems)
        {
            return connectionItems?.ConvertAll(item => ConvertToEntity(item)) ?? new List<RMBoxConnection>();
        }

        public static BoxConnectionViewModel ConvertToViewModel(this BoxConnectionItem connection)
        {
            return connection != null ? new BoxConnectionViewModel
            {
                Id = connection.Id,
                Name = connection.Name,
                Description = connection.Description,
                AuthenticationType = connection.AuthenticationType,
                EnterpriseId = connection.EnterpriseId,
                ClientId = connection.ClientId,
                EmailAddress = connection.EmailAddress,
                JsonFileName = connection.JsonFileName,
                CreatedBy = connection.CreatedBy,
                ModifiedBy = connection.ModifiedBy,
                Created = connection.Created,
                Modified = connection.Modified,
                ConnectionGroupId = connection.ConnectionGroupId,
            } : null;
        }
    }
}