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
using Cloud.Sdk.Data.AosModern;

using System;
using Util.MSAzure;
using CloudBackupAADEnvironment = AvePoint.GCommon.Contract.CentralAdmin.Object.AADEnvironment;
using CloudBackupAppType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType;

namespace AvePoint.Application.TokenManager.TokenManagement
{
    /// <summary>
    /// TODO:will move to a light common weight project later,refact Application.Office365Api or Create a new one
    /// </summary>
    public static class AuthenticationResourceConvert
    {
        public static CloudBackupAADEnvironment Convert(this AzureEnvironment environment)
        {
            return environment switch
            {
                AzureEnvironment.Worldwide or AzureEnvironment.GCC => CloudBackupAADEnvironment.AzureCloud,
                AzureEnvironment.China => CloudBackupAADEnvironment.AzureChinaCloud,
                AzureEnvironment.Germany => CloudBackupAADEnvironment.AzureGermanyCloud,
                AzureEnvironment.USGovGCCHigh => CloudBackupAADEnvironment.USGovernment,
                AzureEnvironment.USGovDoD => CloudBackupAADEnvironment.USGovernment_DoD,
                _ => CloudBackupAADEnvironment.None,
            };
        }

        public static AzureEnvironment Convert(this CloudBackupAADEnvironment environment)
        {
            return environment switch
            {
                CloudBackupAADEnvironment.AzureChinaCloud => AzureEnvironment.China,
                CloudBackupAADEnvironment.AzureGermanyCloud => AzureEnvironment.Germany,
                CloudBackupAADEnvironment.USGovernment => AzureEnvironment.USGovGCCHigh,
                CloudBackupAADEnvironment.USGovernment_DoD => AzureEnvironment.USGovDoD,
                _ => AzureEnvironment.Worldwide
            };
        }

        public static IdentityProviderType ConvertToCloudSdkTokenAppType(this CloudBackupAppType appType)
        {
            return appType switch
            {
                CloudBackupAppType.Office365 => IdentityProviderType.Office365,
                CloudBackupAppType.SharePoint => IdentityProviderType.SharePoint,
                CloudBackupAppType.Exchange => IdentityProviderType.Exchange,
                CloudBackupAppType.CBForM365 => IdentityProviderType.CBForM365,
                CloudBackupAppType.CBForExchangeApp => IdentityProviderType.CBForExchange,
                CloudBackupAppType.CBForSharePointApp => IdentityProviderType.CBForSharePoint,
                CloudBackupAppType.CustomAzureApp => IdentityProviderType.CustomAzureApp,
                CloudBackupAppType.MicrosoftDelegate => IdentityProviderType.MicrosoftDelegate,
                CloudBackupAppType.YammerApp => IdentityProviderType.Yammer,
                CloudBackupAppType.CustomDelegateApp => IdentityProviderType.CustomDelegateApp,
                CloudBackupAppType.CloudRecords => IdentityProviderType.CloudRecords,
                _ => throw new NotSupportedException(),
            };
        }
    }
}