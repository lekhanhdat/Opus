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
using System;
using System.Collections.Generic;
using System.Text;
using AvePoint.RA.RedisCache.Configurations;
using StackExchange.Redis;

namespace AvePoint.RA.RedisCache
{
    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        private readonly string _name;
        private readonly RedisDBOptions _options;
        private ConnectionMultiplexer _connectionMultiplexer;
        public string DBProviderName => this._name;

        public RedisDatabaseProvider(string name, RedisOptions options)
        {
            _options = options.DBConfig;
            _name = name;
        }
        public IDatabase GetDatabase()
        {
            try
            {
                _connectionMultiplexer = CreateConnectionMultiplexer();
                var database = _connectionMultiplexer.GetDatabase();
                
                return database;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ConnectionMultiplexer CreateConnectionMultiplexer()
        {
            var configurationOptions = BuildConfigurationOptions();
            
            if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected || _connectionMultiplexer.GetDatabase() == null)
            {
                _connectionMultiplexer = RedisConnectionFactory.Connect(configurationOptions.ToString(), _options.IgnoreCertificateValidation, _options.IsDevelopmentEnvironment);
            }
            return _connectionMultiplexer;
        }

        private ConfigurationOptions BuildConfigurationOptions()
        {
            if (!string.IsNullOrWhiteSpace(_options.Connection))
            {
                return ConfigurationOptions.Parse(_options.Connection, true);
            }

            return new ConfigurationOptions
            {
                ConnectTimeout = _options.ConnectionTimeout,
                User = _options.Username,
                Password = _options.Password,
                Ssl = _options.IsSsl,
                SslHost = _options.SslHost,
                AllowAdmin = _options.AllowAdmin,
                AbortOnConnectFail = _options.AbortOnConnectFail,
            };
        }
    }
}
