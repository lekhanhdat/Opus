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
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using AvePoint.GCommon.ComplianceDBWrapper.Common;

namespace AvePoint.GCommon.ComplianceDBWrapper.Utility
{
    public class EDSqlCommand
    {
        private SqlCommand _sqlCommand;

        public EDSqlCommand(SqlCommand sqlCommand)
        {
            this._sqlCommand = sqlCommand;
        }

        public void AddValue(string parameterName, object value)
        {
            this._sqlCommand.Parameters.AddWithValue(parameterName, value.IsNull()?DBNull.Value:value);
        }
        /// <summary>
        /// 为了补充hold Name临时加的方法，忽略大小写，Unicode比较。
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="ignoreCase"></param>
        public  void AddNValue(string parameterName,object value,bool ignoreCase)
        {
            SqlParameter parameter = this._sqlCommand.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
            parameter.SqlDbType=SqlDbType.NVarChar;
            if (ignoreCase)
            {
                parameter.CompareInfo = SqlCompareOptions.IgnoreCase;
            }
        }


        public int ExecuteNonQuery()
        {
            return this._sqlCommand.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return this._sqlCommand.ExecuteScalar();
        }

        public string CommandText 
        {
            set { this._sqlCommand.CommandText = value; }
        }

        public EDSqlDataReader ExecuteReader()
        {
            return new EDSqlDataReader(this._sqlCommand.ExecuteReader());
        }

        public void Dispost()
        {
            this._sqlCommand.Dispose();
        }
    }
}
