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
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.Wrapper.Common
{
    internal class AveQueryColumnInfoManager
    {
        string mTableName;
        public Dictionary<string, AveQueryColumnInfo> AveQueryColumnInfoList = new Dictionary<string, AveQueryColumnInfo>();
        private bool schemaIsReady = false;

        public AveQueryColumnInfoManager(string tableName)
        {
            mTableName = tableName;
        }

        public void CollectColumnSchemaInfo(SqlCommand cmd)
        {
            if (schemaIsReady)
                return;

            cmd.Parameters.AddWithValue("@TableName", mTableName);
            string text = "SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS With(nolock) WHERE TABLE_NAME=@TableName";
            cmd.CommandText = text;
            using (SqlDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    AveQueryColumnInfo columnInfo = new AveQueryColumnInfo();
                    columnInfo.name = sr.GetValue(0).ToString();
                    columnInfo.type = sr.GetValue(1).ToString();
                    if (string.Compare(sr.GetString(2), "YES", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        columnInfo.isNullAble = true;
                    }
                    AveQueryColumnInfoList.Add(columnInfo.name, columnInfo);
                }
            }

            AddComputedColumns(GetComputedColumnList(cmd));

            schemaIsReady = true;
        }

        /// <summary>
        /// To get the computed columns from the system table.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private List<string> GetComputedColumnList(SqlCommand cmd)
        {
            List<string> computedList = new List<string>();

            if (!cmd.Parameters.Contains("@TableName"))
            {
                cmd.Parameters.AddWithValue("@TableName", mTableName);
            }
            string text = "SELECT name FROM sys.computed_columns With(nolock) WHERE object_id = OBJECT_ID(@TableName)";
            cmd.CommandText = text;
            using (SqlDataReader sr = cmd.ExecuteReader())
            {
                while (sr.Read())
                {
                    computedList.Add(sr.GetString(0));
                }
            }

            return computedList;
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
                foreach (string key in AveQueryColumnInfoList.Keys)
                {
                    if (dic.ContainsKey(key))
                    {
                        AveQueryColumnInfoList[key].value = dic[key];
                        AveQueryColumnInfoList[key].valueIsNull = false;
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
                            if (!AveQueryColumnInfoList.ContainsKey(column))
                                continue;
                            AveQueryColumnInfo columnInfo = AveQueryColumnInfoList[column];
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
                AveQueryColumnInfoList[list[i]].isComputedColumn = true;
            }
        }

        public void MakeInsertCommand(SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            StringBuilder columns = new StringBuilder("Insert Into " + mTableName + "(");
            StringBuilder values = new StringBuilder(" Values(");

            foreach (string key in AveQueryColumnInfoList.Keys)
            {
                AveQueryColumnInfo columnInfo = AveQueryColumnInfoList[key];
                if (columnInfo.valueIsNull || columnInfo.isComputedColumn)
                    continue;

                string param = "@" + key;
                cmd.Parameters.AddWithValue(param, columnInfo.value);
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

        public void ResetColumnValues(IDictionary<string,object> columns)
        {
            foreach (var kv in columns)
            {
                ResetColumnValue(kv.Key, kv.Value);
            }
        }
        public void ResetColumnValue(string columnName, object value)
        {
            var columnInfo = TryGetColumnInfo(columnName);
            columnInfo.value = value;
            columnInfo.valueIsNull = (columnInfo.value == null);
        }

        private AveQueryColumnInfo TryGetColumnInfo(string columnName)
        {
            if (AveQueryColumnInfoList.ContainsKey(columnName))
            {
                return AveQueryColumnInfoList[columnName];

            }
            else
            {
                var columnInfo = new AveQueryColumnInfo() { name = columnName };
                AveQueryColumnInfoList.Add(columnName, columnInfo);
                return columnInfo;
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dic"></param>
        /// <param name="unUpdateColumnList"></param>
        /// <param name="whereClause"></param>
        /// <returns>是否组装成一个合法的Update语句</returns>
        public bool MakeUpdateCommand(SqlCommand cmd, Dictionary<string, object> dic, List<string> unUpdateColumnList, string whereClause)
        {
            unUpdateColumnList = unUpdateColumnList ?? new List<string>();
            var hashSet = new HashSet<string>(unUpdateColumnList.Distinct(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
            return MakeUpdateCommand(cmd, dic, hashSet, whereClause);
        }

        public bool MakeUpdateCommand(SqlCommand cmd, Dictionary<string, object> dic, HashSet<string> unUpdateColumnList, string whereClause)
        {
            bool result = false;
            cmd.Parameters.Clear();
            StringBuilder cmdText = new StringBuilder("Update " + mTableName + " Set ");

            foreach (string key in dic.Keys)
            {
                if (unUpdateColumnList != null && unUpdateColumnList.Contains(key))
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
                result = true;
            }

            cmdText = cmdText.Remove(cmdText.Length - 1, 1);
            cmdText.Append(" ");
            cmdText.Append(whereClause);
            cmd.CommandText = cmdText.ToString();
            return result;
        }
    }

    internal class AveQueryColumnInfo
    {
        public string name;
        public string type;
        public object value;
        public bool valueIsNull = true;
        public bool isComputedColumn;
        public bool isNullAble;
    }
}
