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
using Microsoft.Extensions.Configuration;

namespace StandaloneTool.Common
{
    public sealed class GlobalConfiguration
    {
        private static readonly Lazy<GlobalConfiguration> _instance = new Lazy<GlobalConfiguration>(() => new GlobalConfiguration());

        private static IConfiguration _configuration;

        private GlobalConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static GlobalConfiguration Instance => _instance.Value;

        public string GetSetting(string key)
        {
            return _configuration[key];
        }

        public T GetSetting<T>(AppSettingKey key)
        {
            var value = _configuration[key.ToString()];
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public T GetSection<T>(string sectionName) where T : new()
        {
            var section = new T();
            section = (T)_configuration.GetSection(sectionName);
            return section;
        }
    }


    public enum AppSettingKey
    {
        MAX_THREAD_COUNT,
        LOG_CONFIG_FILENAME,
        LOG_DEBUG_CONFIG_FILENAME
    }
}
