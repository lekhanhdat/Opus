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
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace AvePoint.GCommon.ComplianceDBWrapper.Common
{
    public class EDSqlDataReader
    {
        private SqlDataReader _reader;

        public bool Read
        {
            get
            {
                return this._reader.Read();
            }
        }

        public EDSqlDataReader(SqlDataReader reader)
        {
            this._reader = reader;
        }
        public String GetString(int index)
        {
            SqlString value = this._reader.GetSqlString(index);
            if (value.IsNull)
            {
                return null;
            }
            return value.ToString();
        }

        /// <summary>
        /// 对应数据库类型中的Smallint
        /// </summary>
        /// <param name="index">reader index</param>
        /// <returns>int16</returns>
        public Int16 GetSmallInt(int index)
        {
            return this._reader.GetInt16(index);
        }

        /// <summary>
        /// 对应数据库类型中的tinyint
        /// </summary>
        /// <param name="index">reader index</param>
        /// <returns>byte</returns>
        public byte GetByte(int index)
        {
            return this._reader.GetByte(index);
        }

        public Int32 GetInt(int index)
        {
            return this._reader.GetInt32(index);
        }

        public long GetLong(int index)
        {
            return this._reader.GetInt64(index);
        }

        public Guid GetGuid(int index)
        {
            return this._reader.GetGuid(index);
        }

        public DateTime GetDateTime(int index)
        {
            return this._reader.GetDateTime(index);
        }

        public void Close()
        {
            this._reader.Dispose();
//            this._reader.Close();
            this._reader = null;
        }
    }
}
