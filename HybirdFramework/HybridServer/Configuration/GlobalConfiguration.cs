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
using HybridServer.Log;
using HybridServer.Utils;
using Microsoft.Extensions.Configuration;
using System;

namespace HybridServer.Configuration
{
    public static class GlobalConfiguration
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(GlobalConfiguration));
        public static bool IsProduction { get; private set; }
        public static String RedisConn { get; private set; }

        public static void Init(IConfigurationRoot builtConfig, bool isProd)
        {
            try
            {
                IsProduction = isProd;
                var redisConn = builtConfig.GetValue(ConfigKey.RECO_REDIS_CONNECTION_STRING);
                if (IsProduction)
                {
                    try
                    {
                        RedisConn = CipherEncryptionUtil.CipherDecrypt(redisConn);
                        logger.Info($"Redis connection string decrypted.");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Decrypt redis connection string failed, use original redis connection string, error: {ex.Message}");
                        RedisConn = redisConn;
                    }
                }
                else
                {
                    RedisConn = redisConn;
                }
                
            }
            catch (Exception ex)
            {
                logger.Error($"Init Config Error:{ex.ToString()}");
            }
        }

        public static string GetValue(this IConfiguration configuration, ConfigKey key)
        {
            return configuration.GetValue<string>(key.ToString());
        }

        public static T GetValue<T>(this IConfiguration configuration, ConfigKey key)
        {
            return configuration.GetValue<T>(key.ToString());
        }

    }


    public enum  ConfigKey
    {
        KEY_VAULT_URL,
        AVE_LOG_FILE_PATH,
        PUBLIC_IDENTITY_SERVICE_URL,
        PUBLIC_AUDIENCE_URL,
        IDENTITY_SERVICE_URL,
        AUDIENCE_URL,
        SIGNALR_DB_CONNECTION_STRING,
        SIGNALR_CLIENT_TIMEOUT_INTERVAL,
        SIGNALR_HAND_SHAKE_TIMEOUT,
        SIGNALR_KEEP_ALIVE_INTERVAL,
        SIGNALR_MAX_RECEIVE_MESSAGE_SIZE,
        SIGNALR_ENABLE_DETAILED_ERRORS,
        RECO_REDIS_CONNECTION_STRING,
        DEV_MODE,
        ENVIRONMENT_NAME,
    }

}
