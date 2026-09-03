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
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.OfflineSearch
{
    public class EDSearchResultDBHelper : IDisposable
    {
        private static AveLogger mLog = new AveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SQLiteConnection sqlLiteConnection = null;
        private string dbFile = string.Empty;
        private AveImpersonator _aveImpersonator;

        public EDSearchResultDBHelper(string file)
        {
            dbFile = file;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public void OpenConnection()
        {
            try
            {
                sqlLiteConnection = new SQLiteConnection();
                sqlLiteConnection.ConnectionString = "Data Source=" + dbFile;
                sqlLiteConnection.Open();
            }
            catch (Exception ex)
            {
                mLog.Error("SQLite connection can not open. {0}", ex.ToString());
            }
        }

        public void OpenConnection(string domainName, string userName, string password, bool keepState = false)
        {
            _aveImpersonator = new AveImpersonator(domainName, userName, password, true);
            _aveImpersonator.Impersonate();
            this.OpenConnection();
            if (keepState == false)
            {
                _aveImpersonator.Dispose();
            }
        }

        public void OpenConnection(string domainName, string userName, string password)
        {
            using (var aveImpersonator = new AveImpersonator(domainName, userName, password, true))
            {
                _aveImpersonator.Impersonate();
                this.OpenConnection();
            }
        }

        public void CreateDB()
        {
            try
            {
                using (DbCommand command = sqlLiteConnection.CreateCommand())
                {
                    List<string> columns = new List<string>();
                    columns.Add("ID"); columns.Add("INTEGER PRIMARY KEY AUTOINCREMENT");
                    columns.Add("Title"); columns.Add("VARCHAR");
                    columns.Add("Author"); columns.Add("VARCHAR");
                    columns.Add("Size"); columns.Add("BIGINT");
                    columns.Add("VersionString"); columns.Add("VARCHAR");
                    columns.Add("Location"); columns.Add("VARCHAR");
                    columns.Add("ResultType"); columns.Add("INTEGER");
                    columns.Add("Summary"); columns.Add("VARCHAR");
                    columns.Add("Created"); columns.Add("DATETIME");
                    columns.Add("FarmName"); columns.Add("VARCHAR");
                    columns.Add("SiteURL"); columns.Add("VARCHAR");
                    columns.Add("PathMD5"); columns.Add("VARCHAR");
                    columns.Add("SubJobID"); columns.Add("VARCHAR");
                    StringBuilder sqlString = new StringBuilder();
                    sqlString.Append("CREATE TABLE SearchResultTable (");
                    sqlString.Append(columns[0] + " " + columns[1]);
                    for (int i = 2; i < columns.Count; )
                    {
                        sqlString.Append(", " + columns[i] + " " + columns[i + 1]);
                        i = i + 2;
                    }
                    sqlString.Append(")");
                    command.CommandText = sqlString.ToString();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Create SQLite DB failed. {0}", ex.ToString());
            }
        }

        public List<EDSearchResult> ReadTable()
        {
            if (sqlLiteConnection.State == ConnectionState.Closed)
            {
                throw new Exception("Connection state is closed in function ReadTable");
            }
            List<EDSearchResult> resultList = new List<EDSearchResult>();
            DbDataReader reader = null;
            try
            {
                using (DbCommand cmd = sqlLiteConnection.CreateCommand())
                {
                    cmd.CommandText = "select Title,Author,Size,VersionString,Location,ResultType,Summary,Created,FarmName,SiteURL,PathMD5,SubJobID from SearchResultTable";
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        EDSearchResult searchResult = new EDSearchResult();

                        searchResult.Title = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        searchResult.Author = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        searchResult.Size = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                        searchResult.VersionString = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                        searchResult.Location = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                        searchResult.ResultType = reader.IsDBNull(5) ? (SharePointType)0 : (SharePointType)reader.GetInt32(5);
                        searchResult.Summary = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                        searchResult.Created = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7);
                        searchResult.FarmName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                        searchResult.SiteURL = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                        searchResult.PathMD5 = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                        searchResult.SubJobID = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                        resultList.Add(searchResult);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("Read table failed. {0}", ex.ToString());
            }
            finally
            {
                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Close reader failed. {0}", ex.ToString());
                }
            }
            return resultList;
        }

        private string StringValue(SqlString value)
        {
            if (value.IsNull)
            {
                return null;
            }
            return value.ToString();
        }

        public int InsertTable(string title, string author, long size, string versionString, string Location, int resultType, DateTime modified)
        {
            int result = 0;
            if (sqlLiteConnection.State == ConnectionState.Closed)
            {
                throw new Exception("Connection state is closed in function InsertTable");
            }

            using (SQLiteCommand cmd = sqlLiteConnection.CreateCommand())
            {
                cmd.CommandText = "insert into SearchResultTable(title,author,size,versionString,location,resultType,created) values(@title,@author,@size,@versionString,@location,@resultType,@created)";
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@author", author);
                cmd.Parameters.AddWithValue("@size", size);
                cmd.Parameters.AddWithValue("@versionString", versionString);
                cmd.Parameters.AddWithValue("@location", Location);
                cmd.Parameters.AddWithValue("@resultType", resultType);
                cmd.Parameters.AddWithValue("@created", modified);
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }

        public int InsertTable(EDSearchResult result)
        {
            int count = 0;
            if (sqlLiteConnection.State == ConnectionState.Closed)
            {
                throw new Exception("Connection state is closed in function InsertTable");
            }

            using (SQLiteCommand cmd = sqlLiteConnection.CreateCommand())
            {
                cmd.CommandText =
                                    @"insert into 
                                        SearchResultTable
                                        (title,author,size,versionString,location,resultType,created,FarmName,SiteURL,PathMD5,SubJobID) 
                                    values
                                        (@title,@author,@size,@versionString,@location,@resultType,@created,@FarmName,@SiteURL,@PathMD5,@SubJobID)";
                cmd.Parameters.AddWithValue("@title", result.Title);
                cmd.Parameters.AddWithValue("@author", result.Author);
                cmd.Parameters.AddWithValue("@size", result.Size);
                cmd.Parameters.AddWithValue("@versionString", result.VersionString);
                cmd.Parameters.AddWithValue("@location", result.Location);
                cmd.Parameters.AddWithValue("@resultType", result.ResultType);
                cmd.Parameters.AddWithValue("@summary", result.Summary);
                cmd.Parameters.AddWithValue("@created", result.Created);
                cmd.Parameters.AddWithValue("@FarmName", string.IsNullOrEmpty(result.FarmName) ? (object)DBNull.Value : result.FarmName);
                cmd.Parameters.AddWithValue("@SiteURL", string.IsNullOrEmpty(result.SiteURL) ? (object)DBNull.Value : result.SiteURL);
                cmd.Parameters.AddWithValue("@PathMD5", string.IsNullOrEmpty(result.PathMD5) ? (object)DBNull.Value : result.PathMD5);
                cmd.Parameters.AddWithValue("@SubJobID", string.IsNullOrEmpty(result.SubJobID) ? (object)DBNull.Value : result.SubJobID);
                count = cmd.ExecuteNonQuery();
            }

            return count;
        }

        public void Dispose()
        {
            if (sqlLiteConnection != null)
            {
                sqlLiteConnection.Close();
                sqlLiteConnection.Dispose();
            }
            if (_aveImpersonator != null)
            {
                _aveImpersonator.Dispose();
            }
        }

        public int GetResultCount(string searchKeyword)
        {
            int count = 0;
            DbDataReader reader = null;
            using (SQLiteCommand cmd = this.sqlLiteConnection.CreateCommand())
            {
                StringBuilder countSql = new StringBuilder().Append("SELECT COUNT(ID) FROM SearchResultTable");
                if (!String.IsNullOrEmpty(searchKeyword))
                {
                    countSql.Append(" WHERE ").Append(searchKeyword);
                }
                cmd.CommandText = countSql.ToString();
                try
                {
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    count = reader.GetInt32(0);
                }
                catch (Exception ex)
                {
                    mLog.Error("Execute reader failed. {0}", ex.ToString());
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            return count;
        }

        private string GenerateKeyWordSql(string keyword, string filterStr)
        {
            string result = String.Empty;
            StringBuilder sql = new StringBuilder();
            if (!String.IsNullOrEmpty(keyword))
            {


                sql.Append("(")
                   .Append(String.Format("title like '%{0}%'", keyword))
                   .Append(String.Format(" or versionString like '%{0}%'", keyword))
                   .Append(String.Format(" or location like '%{0}%'", keyword))
                   .Append(String.Format(" or author like '%{0}%'", keyword))
                   .Append(")");


                return sql.ToString();
            }

            if (!String.IsNullOrEmpty(filterStr))
            {
                if (sql.Length != 0)
                {
                    sql.Append(" AND ");
                }


                sql.Append("ResultType IN")
                    .Append(" (")
                    .Append(filterStr)
                    .Append(")");
            }


            if(sql.Length!=0)
            {
                result = " (" + sql.ToString() + ")";
            }

            return result;

        }


        public SearchResultPaging GetResultByPaging(int currentPage, int everyPageCount, string orderStr, string keyword, string filterStr)
        {
            //            orderStr = String.IsNullOrEmpty(orderStr) ? "ID" : orderStr;
            everyPageCount = everyPageCount == 0 ? 15 : everyPageCount;
            SearchResultPaging resultObj = null;
            string searchKeyword = this.GenerateKeyWordSql(keyword, filterStr);
            int count = this.GetResultCount(searchKeyword);
            if (count > 0)
            {
                DbDataReader reader = null;
                using (SQLiteCommand cmd = this.sqlLiteConnection.CreateCommand())
                {
                    resultObj = new SearchResultPaging();
                    resultObj.EveryPageCount = everyPageCount;
                    resultObj.TotalCount = count;
                    resultObj.TotalPage = resultObj.TotalCount / resultObj.EveryPageCount + (resultObj.TotalCount % resultObj.EveryPageCount != 0 ? 1 : 0);
                    currentPage = currentPage == 0 ? 1 : currentPage;
                    resultObj.CurrentPage = currentPage > resultObj.TotalPage ? resultObj.TotalPage : currentPage;
                    resultObj.Results = new List<EDSearchResult>();

                    StringBuilder sql = new StringBuilder();
                    sql.Append(@"SELECT ID,Author,Size,VersionString,Location,Title,ResultType,Summary,Created,FarmName,SiteURL,PathMD5,SubJobID FROM SearchResultTable ")
                         .Append("WHERE ID NOT IN ")
                        .Append("( ")
                        .Append("SELECT ID FROM SearchResultTable ");
                    if (!String.IsNullOrEmpty(searchKeyword))
                    {
                        sql.Append("WHERE ").Append(searchKeyword).Append(" ");
                    }
                    sql.Append("ORDER BY {0} LIMIT @EveryPageCount * (@CurrentPage -1 ) ")
                    .Append(") ");
                    if (!String.IsNullOrEmpty(searchKeyword))
                    {
                        sql.Append("And ").Append(searchKeyword).Append(" ");
                    }
                    sql.Append("ORDER BY {1} LIMIT  @EveryPageCount");

                    cmd.CommandText = String.Format(sql.ToString(), orderStr, orderStr);
                    cmd.Parameters.AddWithValue("@EveryPageCount", resultObj.EveryPageCount);
                    cmd.Parameters.AddWithValue("@CurrentPage", resultObj.CurrentPage);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        EDSearchResult searchResult = new EDSearchResult();
                        searchResult.ID = reader.GetInt32(0);
                        searchResult.Author = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        searchResult.Size = reader.IsDBNull(2) ? 0L : reader.GetInt64(2);
                        searchResult.VersionString = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        searchResult.Location = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        searchResult.Title = reader.IsDBNull(5) ? "" : reader.GetString(5);
                        searchResult.ResultType = reader.IsDBNull(6) ? SharePointType.None : Enumer.Parse<SharePointType>(reader.GetInt32(6));
                        searchResult.Summary = reader.IsDBNull(7) ? "" : reader.GetString(7);
                        searchResult.Created = reader.IsDBNull(8) ? DateTime.UtcNow : reader.GetDateTime(8);
                        searchResult.FarmName = reader.IsDBNull(9) ? "" : reader.GetString(9);
                        searchResult.SiteURL = reader.IsDBNull(10) ? "" : reader.GetString(10);
                        searchResult.PathMD5 = reader.IsDBNull(11) ? "" : reader.GetString(11);
                        searchResult.SubJobID = reader.IsDBNull(12) ? "" : reader.GetString(12);
                        resultObj.Results.Add(searchResult);
                    }

                }
            }
            return resultObj;
        }
    }
}
