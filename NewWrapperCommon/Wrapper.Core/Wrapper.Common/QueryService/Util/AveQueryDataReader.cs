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
using System.Data.SqlClient;

namespace AvePoint.Wrapper.Common
{
    public class AveQueryDataReader : IAveQueryDataReader
    {
        private SqlDataReader mDataReader;

        public AveQueryDataReader(SqlDataReader dataReader)
        {
            mDataReader = dataReader;
        }

        public bool Read()
        {
            try
            {
                return mDataReader.Read();
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public void Dispose()
        {
            if (mDataReader != null)
            {
                try
                {
                    mDataReader.Dispose();
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
                mDataReader = null;
            }
        }

        public void Close()
        {
            try
            {
                mDataReader.Close();
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public bool GetBoolean(int i)
        {
            try
            {
                return mDataReader.GetBoolean(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public byte GetByte(int i)
        {
            try
            {
                return mDataReader.GetByte(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            try
            {
                return mDataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public char GetChar(int i)
        {
            try
            {
                return mDataReader.GetChar(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            try
            {
                return mDataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public string GetDataTypeName(int i)
        {
            try
            {
                return mDataReader.GetDataTypeName(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public DateTime GetDateTime(int i)
        {
            try
            {
                return mDataReader.GetDateTime(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public decimal GetDecimal(int i)
        {
            try
            {
                return mDataReader.GetDecimal(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public double GetDouble(int i)
        {
            try
            {
                return mDataReader.GetDouble(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public Type GetFieldType(int i)
        {
            try
            {
                return mDataReader.GetFieldType(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public float GetFloat(int i)
        {
            try
            {
                return mDataReader.GetFloat(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public Guid GetGuid(int i)
        {
            try
            {
                return mDataReader.GetGuid(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public short GetInt16(int i)
        {
            try
            {
                return mDataReader.GetInt16(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public int GetInt32(int i)
        {
            try
            {
                return mDataReader.GetInt32(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public long GetInt64(int i)
        {
            try
            {
                return mDataReader.GetInt64(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public string GetName(int i)
        {
            try
            {
                return mDataReader.GetName(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public int GetOrdinal(string name)
        {
            try
            {
                return mDataReader.GetOrdinal(name);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public virtual object GetSqlValue(int i)
        {
            try
            {
                return mDataReader.GetSqlValue(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public string GetString(int i)
        {
            try
            {
                return mDataReader.GetString(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public object GetValue(int i)
        {
            try
            {
                return mDataReader.GetValue(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public bool IsDBNull(int i)
        {
            try
            {
                return mDataReader.IsDBNull(i);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public bool NextResult()
        {
            try
            {
                return mDataReader.NextResult();
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public object this[int i]
        {
            get
            {
                try
                {
                    return mDataReader[i];
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }

        }

        public object this[string name]
        {
            get
            {
                try
                {
                    return mDataReader[name];
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public int Depth
        {
            get
            {
                try
                {
                    return mDataReader.Depth;
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public System.Data.DataTable GetSchemaTable()
        {
            try
            {
                return mDataReader.GetSchemaTable();
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public bool IsClosed
        {
            get
            {
                try
                {
                    return mDataReader.IsClosed;
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public int RecordsAffected
        {
            get
            {
                try
                {
                    return mDataReader.RecordsAffected;
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public int FieldCount
        {
            get
            {
                try
                {
                    return mDataReader.FieldCount;
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        public System.Data.IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            try
            {
                return mDataReader.GetValues(values);
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
    }
}
