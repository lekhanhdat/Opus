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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using System;
using System.Data.SqlClient;

namespace AvePoint.RA.DB.Explorer
{
    public class ExplorerDBSetting
    {
        public static string DatabaseInstance { get; set; }
        public static string DatabaseName { get; set; }
        public static string DatabaseUsername { get; set; }
        public static string DatabasePassword { get; set; }
        public static bool DatabaseIsIntegrated { get; set; } 
        public static string ConnectionDatabaseString { get; set; }
        public static string Domain { get; set; }
        public static bool DatabaseIsLocalServer { get; set; }
 
       

        public static void CreateConnectionString(string clearTxtPwd)
        {
            SqlConnectionStringBuilder dbBuilder = new SqlConnectionStringBuilder();

            if (!string.IsNullOrEmpty(DatabaseInstance))
            {
                dbBuilder.DataSource = DatabaseInstance;
                DatabaseIsLocalServer = IsLocalInstance(DatabaseInstance);
            }
            else
            {
                throw new Exception("DatabaseInstance is null");
            }

            if (!string.IsNullOrEmpty(DatabaseName))
            {
                dbBuilder.InitialCatalog = DatabaseName;
            }
            else
            {
                throw new Exception("DatabaseName is null");
            }

            dbBuilder.IntegratedSecurity = DatabaseIsIntegrated;
            dbBuilder.MinPoolSize = 200;
            dbBuilder.MaxPoolSize = 3200;
            string domain = null;
            string userId = null;
            if (DatabaseIsIntegrated
                && TryAnalyseDomainAndUser(DatabaseUsername, out domain, out userId))
            {
                Domain = domain;
                DatabaseUsername = userId;
            }
            else
            {
                if (!string.IsNullOrEmpty(DatabaseUsername))
                {
                    dbBuilder.UserID = DatabaseUsername;
                }

                if (!string.IsNullOrEmpty(DatabasePassword))
                {
                    dbBuilder.Password = clearTxtPwd;
                }
            }
            if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                dbBuilder.TrustServerCertificate = true;
            }

            ConnectionDatabaseString = dbBuilder.ConnectionString;
             
        }

        private static bool IsLocalInstance(string instance)
        {
            string[] hostAndInstance = instance.Split('\\', '/');
            string host = hostAndInstance[0];
            if (string.CompareOrdinal(".", host) == 0)
            {
                return true;
            }
            return false;
        }

        public static bool TryAnalyseDomainAndUser(string str, out string domain, out string user)
        {
            domain = string.Empty;
            user = string.Empty;
            int index = str.IndexOf('\\');
            if (index == -1 || index == str.Length) { return false; }
            domain = str.Substring(0, index);
            user = str.Substring(index + 1);
            return true;
        }

    }
}
