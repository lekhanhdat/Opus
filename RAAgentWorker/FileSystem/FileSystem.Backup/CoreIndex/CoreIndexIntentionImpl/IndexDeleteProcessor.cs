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
using AvePoint.Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Core.Index.CoreIndexIntentionImpl
{
    public class IndexDeleteProcessor
    {
        IndexDatabaseHelper dbHelper = new IndexDatabaseHelper();
        public SQLiteConnection dbConnection;
        Guid connectionID;

        String connectionString;
        public void Open(String connectionString)
        {
            this.connectionString = connectionString;
            this.connectionID = Guid.NewGuid();
            {
                this.dbConnection = new SQLiteConnection(connectionString);
                this.dbConnection.Open();
                CreateTable();
            }
        }
        public void Close() 
        {
            //this.dbConnection.Close();
        }
        public void CreateTable()
        {
            SQLiteCommand cmd = new SQLiteCommand(dbConnection);
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS deleteTable (COL_ID nvarchar,COL_TYPE nvarchar,COL_NAME nvarchar,COL_PATHMD5 nvarchar,COL_PARENT_PATH_MD5 nvarchar,COL_ARCHIVE_TIME bigint,COL_CREATE_TIME bigint,COL_STORAGEPOLICYID nvarchar,COL_EXTENSION_7 nvarchar,COL_SITE_PATH nvarchar)";
            cmd.ExecuteNonQuery();
        }
        public void InsertToDeleteDb(List<ArchiverBasicIndex> deleteData)
        {
            string insertText = $"INSERT INTO deleteTable (COL_ID, COL_TYPE,COL_NAME,COL_PATHMD5,COL_PARENT_PATH_MD5,COL_ARCHIVE_TIME,COL_CREATE_TIME,COL_STORAGEPOLICYID,COL_EXTENSION_7,COL_SITE_PATH) VALUES" +
                $" (@Id,@Type,@Name,@PathMD5,@ParentPathMD5,@ArchiveTime,@CreateTime,@StoragePolicyId,@Url,@SitePath)";
            SQLiteCommand cmd = new SQLiteCommand(dbConnection);
            using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand(insertText, dbConnection))
                {
                    command.Parameters.Add(new SQLiteParameter("@Id"));
                    command.Parameters.Add(new SQLiteParameter("@Type"));
                    command.Parameters.Add(new SQLiteParameter("@Name"));
                    command.Parameters.Add(new SQLiteParameter("@PathMD5"));
                    command.Parameters.Add(new SQLiteParameter("@ParentPathMD5"));
                    command.Parameters.Add(new SQLiteParameter("@ArchiveTime"));
                    command.Parameters.Add(new SQLiteParameter("@CreateTime"));
                    command.Parameters.Add(new SQLiteParameter("@StoragePolicyId"));
                    command.Parameters.Add(new SQLiteParameter("@Url"));
                    command.Parameters.Add(new SQLiteParameter("@SitePath"));
                    foreach (var data in deleteData)
                    {
                        command.Parameters["@Id"].Value = data.Id;
                        command.Parameters["@Type"].Value = data.Type;
                        command.Parameters["@Name"].Value = data.Name;
                        command.Parameters["@PathMD5"].Value = data.PathMD5;
                        command.Parameters["@ParentPathMD5"].Value = data.ParentPathMD5;
                        command.Parameters["@ArchiveTime"].Value = data.ArchiveTime;
                        command.Parameters["@CreateTime"].Value = data.CreateTime;
                        command.Parameters["@StoragePolicyId"].Value = data.StoragePolicyId;
                        command.Parameters["@Url"].Value = data.Url;
                        command.Parameters["@SitePath"].Value = data.SitePath;
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
