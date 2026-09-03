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
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;
using AvePoint.GCommon.ComplianceDBWrapper.Common;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBAttachmentService : AbstractService
    {
        public EDBAttachmentService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {
        }

        #region - 插入一条附件记录 -

        public int Insert(EDAttachment attachment)
        {
            #region - execute sql -

            string executeSql = @"INSERT INTO 
                                     CPLED_Attachment 
                                     (ID,ItemUniqueID,DeviceID,Name) 
                                  VALUES
                                     (@ID,@ItemUniqueID,@DeviceID,@Name)";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            Guid attachmentID = Guid.NewGuid();
            cmd.AddValue("@ID", attachmentID);
            cmd.AddValue("@ItemUniqueID", attachment.ItemUniqueID);
            cmd.AddValue("@DeviceID", attachment.DeviceID);
            cmd.AddValue("@Name",attachment.Name);
            int cnt = cmd.ExecuteNonQuery();
            if(cnt > 0)
            {
                if(attachment.MetadataStorageInfo != null)
                {
                    attachment.MetadataStorageInfo.DataID = attachmentID.ToString();
                    DBWrapper.Use<EDBStorageInfoService>().Insert(attachment.MetadataStorageInfo);
                }
                if(attachment.ContentStorageInfo != null)
                {
                    attachment.ContentStorageInfo.DataID = attachmentID.ToString();
                    DBWrapper.Use<EDBStorageInfoService>().Insert(attachment.ContentStorageInfo);
                }
                
            }
            cmd.Dispost();
            return cnt;
        }

        #endregion

        #region - 根据ItemUniqueID删除附件信息 -

        public int Delete(string itemUniqueID)
        {

            EDAttachments attachments = GetAttachments(itemUniqueID);
            foreach (var edAttachment in attachments)
            {
                DBWrapper.Use<EDBStorageInfoService>().Delete(edAttachment.ID.ToString());
            }

            #region  - execute sql -

            string executeSql = "DELETE FROM CPLED_Attachment WHERE ItemUniqueID = @ItemUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@ItemUniqueID", itemUniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 根据ItemUniqueID获得相关的所有附件信息 -

        public EDAttachments GetAttachments(string itemUniqueID)
        {

            #region - execute sql -

            const string executeSql = @"SELECT 
                                      ID,ItemUniqueID,DeviceID,Name  
                                  FROM 
                                      CPLED_Attachment 
                                  WHERE 
                                      ItemUniqueID = @ItemUniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@ItemUniqueID", itemUniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDAttachments attachments = new EDAttachments();
            while (reader.Read)
            {
                EDAttachment attachment = new EDAttachment();
                attachment.ID = reader.GetGuid(0);
                attachment.ItemUniqueID = reader.GetString(1);
                attachment.DeviceID = reader.GetString(2);
                attachment.Name = reader.GetString(3);
                attachments.Add(attachment);
            }
            cmd.Dispost();
            reader.Close();
            foreach (var edAttachment in attachments)
            {
                edAttachment.ContentStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edAttachment.ID.ToString(), StorageType.Content);
                edAttachment.MetadataStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edAttachment.ID.ToString(), StorageType.Metadata);
            }
            return attachments;
        }

        #endregion
    }
}
