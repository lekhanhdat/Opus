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




namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Data.SqlClient;
    using System.Data.Common;
    using System.Reflection;
    using AvePoint.GCommon;
    using System.Data;
    using System.IO;
    using AvePoint.Wrapper.Resource;
    #endregion

    internal class AveConnectionMonitorUtil
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static int GetSPID(AveConnectionControler connectionControler)
        {
            int spid = 0;
            SqlCommand getspid = new SqlCommand("select @@SPID", connectionControler.SqlConnection.Connection);
            spid = Convert.ToInt32(getspid.ExecuteScalar());
            return spid;
        }

        public static string DumpSysProcess(AveConnectionControler connectionControler)
        {
            DbConnectionStringBuilder connBuilder = new DbConnectionStringBuilder();
            connBuilder.ConnectionString = connectionControler.SqlConnection.ConnectionString;
            connBuilder["Initial Catalog"] = "master";
            using (SqlConnection sqlCon = new SqlConnection(connBuilder.ConnectionString))
            {
                StringBuilder dumpInfo = new StringBuilder();
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand("select * from sysprocesses With(nolock) where spid=@spid", sqlCon))
                    {
                        sqlCmd.Parameters.Add("@spid", SqlDbType.Int).Value = connectionControler.Id;
                        sqlCon.Open();
                        using (SqlDataReader dataReader = sqlCmd.ExecuteReader())
                        {
                            DataTable dumpInfoTable = new DataTable();
                            dumpInfoTable.Load(dataReader);
                            dumpInfoTable.WriteXml(new StringWriter(dumpInfo));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSearchInDBError, ex.ToString());
                }
                finally
                {
                    sqlCon.Close();
                }
                return dumpInfo.ToString();
            }
        }

        public static string DumpLocksInfo(AveConnectionControler connectionControler)
        {
            DbConnectionStringBuilder connBuilder = new DbConnectionStringBuilder();
            connBuilder.ConnectionString = connectionControler.SqlConnection.ConnectionString;
            connBuilder["Initial Catalog"] = "master";
            using (SqlConnection sqlCon = new SqlConnection(connBuilder.ConnectionString))
            {
                StringBuilder dumpInfo = new StringBuilder();
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand("select * from syslockinfo With(nolock) where req_spid=@id", sqlCon))
                    {
                        sqlCmd.Parameters.Add("@id", SqlDbType.Int).Value = connectionControler.Id;
                        sqlCon.Open();
                        using (SqlDataReader dataReader = sqlCmd.ExecuteReader())
                        {

                            DataTable dumpInfoTable = new DataTable();
                            dumpInfoTable.Load(dataReader);
                            dumpInfoTable.WriteXml(new StringWriter(dumpInfo));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSearchInDBError, ex.ToString());
                }
                finally
                {
                    sqlCon.Close();
                }
                return dumpInfo.ToString();
            }
        }
    }
}
