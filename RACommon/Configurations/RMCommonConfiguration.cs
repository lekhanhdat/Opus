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
using AvePoint.RA.Contract.Configurations;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Common.Configurations
{
    public class RMCommonEncryptConfiguration : RMBaseConfiguration<RMCommonSettingKey>
    {
        private Dictionary<RMCommonSettingKey, RMEncryptType> encryptItems = new Dictionary<RMCommonSettingKey, RMEncryptType>()
        {
            { RMCommonSettingKey.RELATED_RECORDS_APP_CLIENT_SECRET, RMEncryptType.Cipher },
            { RMCommonSettingKey.RELATED_RECORDS_APP_CLIENT_SECONDARY_SECRET, RMEncryptType.Cipher },
            { RMCommonSettingKey.RECO_REDIS_CONNECTION_STRING, RMEncryptType.Cipher },
            { RMCommonSettingKey.TELEMETRY_CONNECTION_STRING, RMEncryptType.Cipher },
            { RMCommonSettingKey.SENDGRID_KEY, RMEncryptType.Cipher },
            { RMCommonSettingKey.VERTEX_AI_PRIVATE_KEY, RMEncryptType.Cipher },
            { RMCommonSettingKey.VECTOR_DB_CONNECTION_STRING, RMEncryptType.Cipher },
            { RMCommonSettingKey.AZURE_OPEN_AI_API_KEY, RMEncryptType.Cipher },
        };
        private Dictionary<RMCommonSettingKey, RMEncryptType> encryptItemsInDevMode = new Dictionary<RMCommonSettingKey, RMEncryptType>()
        {
            { RMCommonSettingKey.NOTIFICATION_SETTING, RMEncryptType.Base64 },
        };

        public RMCommonEncryptConfiguration() : base()
        {

        }
        public string GetVertexAIPrivateKey()
        {
            var privateKey = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.VERTEX_AI_PRIVATE_KEY];
            if (!string.IsNullOrEmpty(privateKey) && privateKey.EndsWith("\\n"))
            {
                privateKey = privateKey.Replace("\\n", "\n");
            }
            return privateKey;
        }
        public string GetAzureOpenAIAPIKey()
        {
            var apiKey = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.AZURE_OPEN_AI_API_KEY];
            if (!string.IsNullOrEmpty(apiKey) && apiKey.EndsWith("\\n"))
            {
                apiKey = apiKey.Replace("\\n", "\n");
            }
            return apiKey;
        }
        protected override Dictionary<RMCommonSettingKey, RMEncryptType> EncryptedItems => 
            RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? encryptItemsInDevMode : encryptItems;

    }
}
