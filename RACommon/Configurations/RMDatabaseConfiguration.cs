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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.Common.Configurations
{
    public class RMDatabaseConfiguration : RMBaseConfiguration<RMDatabaseSettingKey>
    {
        public RMDatabaseConfiguration() : base()
        {
        }

        protected override Dictionary<RMDatabaseSettingKey, RMEncryptType> EncryptedItems =>
            RMGlobalConfiguration.EnvSetting.IsDevEnvironment ? null : GetEncryptItems();

        public string ConfigDatabaseInstance { get; private set; }
        public string ConfigDatabaseName { get; private set; }
        public string ConfigDatabaseUserName { get; private set; }
        public string ConfigDatabasePassword { get; private set; }

        private Dictionary<RMDatabaseSettingKey, RMEncryptType> GetEncryptItems()
        {
            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                return null;
            }

            var encryptItems = new Dictionary<RMDatabaseSettingKey, RMEncryptType>()
            {
                { RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL, RMEncryptType.Cipher },
                { RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING, RMEncryptType.Cipher },
            };

            if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                encryptItems.Add(RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING, RMEncryptType.Cipher);
            }
            return encryptItems;
        }

        protected override string GetValueFromConfigFile(RMDatabaseSettingKey key)
        {
            var value = base.GetValueFromConfigFile(key);
            if (key == RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING)
            {
                if (GCommon.Utility.SecurityUtils.ValidateSQLConnectionStringWithBuilder(value, out var sqlCon))
                {
                    //var sqlCon = new SqlConnectionStringBuilder(value);
                    sqlCon.ApplicationName = "Records";
                    sqlCon.ConnectTimeout = 600;
                    ConfigDatabaseInstance = sqlCon.DataSource;
                    ConfigDatabaseName = sqlCon.InitialCatalog;

                    if (!string.IsNullOrEmpty(sqlCon.UserID) && !string.IsNullOrEmpty(sqlCon.Password))
                    {
                        sqlCon.Password = AnalysePwd(sqlCon.Password);
                        ConfigDatabaseUserName = sqlCon.UserID;
                        ConfigDatabasePassword = sqlCon.Password;
                    }
                    value = sqlCon.ToString();
                }
            }
            else if(key == RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL)
            {
                if (GCommon.Utility.SecurityUtils.ValidateSQLConnectionStringWithBuilder(value,out var sqlCon))
                {
                    //var sqlCon = new SqlConnectionStringBuilder(value);
                    sqlCon.ApplicationName = "Records";
                    sqlCon.ConnectTimeout = 600;
                    if (!string.IsNullOrEmpty(sqlCon.Password))
                    {
                        sqlCon.Password = AnalysePwd(sqlCon.Password);
                    }

                    value = sqlCon.ToString();
                }
            }
            else if(key == RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION)
            {
                var accounts = GetSectionValueFromCongfigFile("RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION");
                if (accounts == null || !accounts.Any())
                {
                    value = string.Empty;
                }
                else
                {
                    value = JsonConvert.SerializeObject(accounts.Select(a => a.Value));
                }
            }
            return value;
        }
    }
}

