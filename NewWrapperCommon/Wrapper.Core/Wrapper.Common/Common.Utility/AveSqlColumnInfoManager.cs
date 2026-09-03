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
using System.Data;
using System.Data.SqlClient;

namespace AvePoint.Wrapper.Common
{
    public class AveSqlColumnInfoManager
    {
        string mTableName;
        public Dictionary<string, AveSqlColumnInfo> AveSqlColumnInfoList = new Dictionary<string, AveSqlColumnInfo>();
        private bool schemaIsReady = false;

        public AveSqlColumnInfoManager(string tableName)
        {
            mTableName = tableName;
        }

        public void CollectColumnSchemaInfo(SqlCommand cmd)
        {
            if (schemaIsReady)
                return;

            cmd.Parameters.AddWithValue("@TableName", mTableName);
            string text = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@TableName";
            cmd.CommandText = text;
            using (SqlDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    AveSqlColumnInfo columnInfo = new AveSqlColumnInfo();
                    columnInfo.name = sr.GetValue(0).ToString();
                    columnInfo.type = sr.GetValue(1).ToString();
                    if (string.Compare(sr.GetString(2), "YES",StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        columnInfo.isNullAble = true;
                    }
                    AveSqlColumnInfoList.Add(columnInfo.name, columnInfo);
                }
            }

            schemaIsReady = true;
        }

        public void LoadColumnsInfo(Dictionary<string, object> dic, SqlCommand cmd)
        {
            if (!schemaIsReady)
            {
                string commandText = cmd.CommandText;
                CollectColumnSchemaInfo(cmd);
                cmd.CommandText = commandText;
            }

            if (dic != null)
            {
                foreach (string key in AveSqlColumnInfoList.Keys)
                {
                    if (dic.ContainsKey(key))
                    {
                        AveSqlColumnInfoList[key].value = dic[key];
                        AveSqlColumnInfoList[key].valueIsNull = false;
                    }
                }
            }

            using (SqlDataReader sr = cmd.ExecuteReader())
            {
                if (sr.Read())
                {
                    for (int i = 0; i < sr.FieldCount; ++i)
                    {
                        if (!sr.IsDBNull(i))
                        {
                            string column = sr.GetName(i);
                            if (!AveSqlColumnInfoList.ContainsKey(column))
                                continue;
                            AveSqlColumnInfo columnInfo = AveSqlColumnInfoList[column];
                            if (dic != null && dic.Count > 0)
                            {
                                if (!dic.ContainsKey(column))
                                {
                                    columnInfo.value = sr.GetValue(i);
                                    columnInfo.valueIsNull = false;
                                }
                            }
                            else
                            {
                                columnInfo.value = sr.GetValue(i);
                                columnInfo.valueIsNull = false;
                            }
                        }
                    }
                }
            }
        }

        public void AddComputedColumns(List<string> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                AveSqlColumnInfoList[list[i]].isComputedColumn = true;
            }
        }

        public void MakeInsertCommand(SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            StringBuilder columns = new StringBuilder("Insert Into " + mTableName + "(");
            StringBuilder values = new StringBuilder(" Values(");

            foreach (string key in AveSqlColumnInfoList.Keys)
            {
                AveSqlColumnInfo columnInfo = AveSqlColumnInfoList[key];
                if (columnInfo.valueIsNull || columnInfo.isComputedColumn)
                    continue;

                string param = "@" + key;
                cmd.Parameters.AddWithValue(param, columnInfo.value);
                //switch (columnInfo.type.ToLower())
                //{
                //    case "int":
                //        cmd.Parameters.AddWithValue(param, (Int32)columnInfo.value);
                //        break;
                //    case "text":
                //    case "char":
                //    case "nchar":
                //    case "ntext":
                //    case "nvarchar":
                //    case "varchar":
                //        cmd.Parameters.AddWithValue(param, (String)columnInfo.value);
                //        break;
                //    case "bigint":
                //        cmd.Parameters.AddWithValue(param, (Int64)columnInfo.value);
                //        break;
                //    case "binary":
                //    case "image":
                //    case "varbinary":
                //        cmd.Parameters.AddWithValue(param, (Byte[])columnInfo.value);
                //        break;
                //    case "bit":
                //        cmd.Parameters.AddWithValue(param, (Boolean)columnInfo.value);
                //        break;
                //    case "datetime":
                //    case "smalldatetime":
                //    case "timestamp":
                //        cmd.Parameters.AddWithValue(param, (DateTime)columnInfo.value);
                //        break;
                //    case "decimal":
                //    case "money":
                //    case "numeric":
                //    case "smallmoney":
                //        cmd.Parameters.AddWithValue(param, (Decimal)columnInfo.value);
                //        break;
                //    case "float":
                //        cmd.Parameters.AddWithValue(param, (Double)columnInfo.value);
                //        break;
                //    case "real":
                //        cmd.Parameters.AddWithValue(param, (Single)columnInfo.value);
                //        break;
                //    case "smallint":
                //        cmd.Parameters.AddWithValue(param, (Int16)columnInfo.value);
                //        break;
                //    case "tinyint":
                //        cmd.Parameters.AddWithValue(param, (Byte)columnInfo.value);
                //        break;
                //    case "uniqueidentifier":
                //        cmd.Parameters.AddWithValue(param, (Guid)columnInfo.value);
                //        break;
                //    case "Variant":
                //        cmd.Parameters.AddWithValue(param, columnInfo.value);
                //        break;
                //    default:
                //        cmd.Parameters.AddWithValue(param, (String)columnInfo.value);
                //        break;
                //}

                columns.Append(key + ",");
                values.Append("@" + key + ",");
            }

            columns = columns.Remove(columns.Length - 1, 1);
            columns.Append(")");
            values = values.Remove(values.Length - 1, 1);
            values.Append(")");
            columns.Append(values.ToString());
            cmd.CommandText = columns.ToString();
        }

        public void ResetColumnValue(string columnName, object value)
        {
            AveSqlColumnInfo columnInfo = AveSqlColumnInfoList[columnName];
            columnInfo.value = value;
            if (columnInfo.value != null)
            {
                columnInfo.valueIsNull = false;
            }
            else
            {
                columnInfo.valueIsNull = true; 
            }
        }

        //public void LoadColumnsInfo(Dictionary<string, object> dic)
        //{
        //    foreach (string key in AveSqlColumnInfoList.Keys)
        //    {
        //        if (dic.ContainsKey(key))
        //        {
        //            AveSqlColumnInfoList[key].value = dic[key];
        //        }
        //    }
        //}

        public void MakeUpdateCommand(SqlCommand cmd, Dictionary<string,object> dic, List<string> UnUpdateColumnList, string whereClause)
        {
            cmd.Parameters.Clear();
            StringBuilder cmdText = new StringBuilder("Update " + mTableName + " Set ");

            foreach (string key in dic.Keys)
            {
                if (UnUpdateColumnList.Contains(key))
                {
                    continue;
                }
                cmdText.Append(key);
                cmdText.Append("=");
                if (dic[key] == null)
                {
                    cmdText.Append("NULL");
                }
                else
                {
                    string param = "@" + key;
                    cmd.Parameters.AddWithValue(param, dic[key]);
                    cmdText.Append(param);
                }
                cmdText.Append(",");
            }

            cmdText = cmdText.Remove(cmdText.Length - 1, 1);
            cmdText.Append(" ");
            cmdText.Append(whereClause);
            cmd.CommandText = cmdText.ToString();
        }
    }

    public class AveSqlColumnInfo
    {
        public string name;
        public string type;
        public object value;
        public bool valueIsNull = true;
        public bool isComputedColumn;
        public bool isNullAble;
    }
}
