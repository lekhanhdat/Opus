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
using System.Data.SqlClient;
using AvePoint.GCommon.ComplianceDBWrapper.Common;
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBMappingService : AbstractService
    {
        public EDBMappingService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {
            
        }

        #region - 根据DataUniqueID 与 ItemUniqueID 删除Mapping -

        public int DeleteMappingUnique(string dataUniqueID,string itemUniqueID)
        {
            #region - execute sql -

            const string executeSql = @"DELETE FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID AND ItemUniqueID = @ItemUniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@DataUniqueID",dataUniqueID);
            cmd.AddValue("@ItemUniqueID",itemUniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 使用DataUniqueID 与 ItemUnquieID 生成Mapping -

        public int InsertMapping(string dataUniqueID,string itemUniqueID)
        {
            #region - execute sql -

            const string executeSql = @"IF NOT EXISTS (SELECT ID FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID AND ItemUniqueID = @ItemUniqueID) 
                                        INSERT INTO CPLED_HoldRelations 
                                            (ID,DataUniqueID,ItemUniqueID) 
                                        VALUES 
                                            (@ID,@DataUniqueID,@ItemUniqueID) ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@ID",Guid.NewGuid());
            cmd.AddValue("@DataUniqueID",dataUniqueID);
            cmd.AddValue("@ItemUniqueID", itemUniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 根据Data的UniqueID删除Mapping -

        public int DeleteMappingByDataUniqueID(string dataUniqueID)
        {
            #region - execute sql - 

            const string executeSql = @"DELETE FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@DataUniqueID",dataUniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 根据Item的UniqueID删除Mapping -

        public int DeleteMappingByItemUniqueID(string itemUniqueID)
        {
            #region - execute sql - 

            const string executeSql = @"DELETE FROM CPLED_HoldRelations WHERE ItemUniqueID = @ItemUniqueID";

            #endregion

            try
            {
                EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
                cmd.AddValue("@ItemUniqueID", itemUniqueID);
                var count = cmd.ExecuteNonQuery();
                cmd.Dispost();
                return count;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Delete data mapping failed. {0}", ex.ToString()));
            }
        }

        #endregion

        #region - 检查Mapping关系是否已经存在 -

        /// <summary>
        /// 检查是否存在Mapping关系.
        /// </summary>
        /// <param name="dataUniqueID"></param>
        /// <param name="itemUniqueID"></param>
        /// <returns></returns>
        public bool ExistsMapping(string dataUniqueID, string itemUniqueID)
        {
            #region - executeSql -

            const string executeSql = @"SELECT COUNT(ID) FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID AND ItemUniqueID = @ItemUniqueID ";

            #endregion

            EDSqlCommand edSqlCommand = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            edSqlCommand.AddValue("@DataUniqueID", dataUniqueID);
            edSqlCommand.AddValue("@ItemUniqueID", itemUniqueID);
            int count = (int)edSqlCommand.ExecuteScalar();
            edSqlCommand.Dispost();
            return count > 0;

        }

        /// <summary>
        ///根据UniqueID判断Mapping关系
        /// </summary>
        /// <param name="itemUniqueID"></param>
        /// <returns></returns>
        public bool ExistsMapping(string itemUniqueID)
        {
            #region - executeSql-
            const string executeSql = @"SELECT COUNT(ID) FROM CPLED_HoldRelations WHERE ItemUniqueID=@ItemUniqueID";
            #endregion

            EDSqlCommand edlSqlCommand = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            edlSqlCommand.AddValue("@ItemUniqueID", itemUniqueID);
            int count = (int)edlSqlCommand.ExecuteScalar();
            edlSqlCommand.Dispost();
            return count > 0;
        }
        #endregion

        #region - 根据ItemUniqueID,获取其下相关的Data数据 -

        public List<string> GetDataUniqueID(string itemUniqueID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT DataUniqueID FROM CPLED_HoldRelations WHERE ItemUniqueID = @ItemUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@itemUniqueID",itemUniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            List<string> dataUniqueIDs = new List<string>();
            while(reader.Read)
            {
                dataUniqueIDs.Add(reader.GetString(0));                
            }
            cmd.Dispost();
            reader.Close();
            return dataUniqueIDs;
        }

        #endregion

        #region - 根据DataUniqueID,获得旗下相关的Hold Item数据ID -

        public List<string> GetItemIDsByDataUniqueID(string dataUniqueID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT ItemUniqueID FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@DataUniqueID",dataUniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            List<string> itemUniqueIDs = new List<string>();
            while(reader.Read)
            {
                itemUniqueIDs.Add(reader.GetString(0));
            }
            cmd.Dispost();
            reader.Close();
            return itemUniqueIDs;
        }

        #endregion

        public void UpdateMapping(string oldItemUniqueID,string newItemUniqueID)
        {
            #region - execute sql -

            string executeSql = @"Update CPLED_HoldRelations SET ItemUniqueID = @newItemUniqueID WHERE ItemUniqueID = @oldItemUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText =  executeSql};
            cmd.AddValue("@oldItemUniqueID", oldItemUniqueID);
            cmd.AddValue("@newItemUniqueID",newItemUniqueID);
            cmd.ExecuteNonQuery();
            cmd.Dispost();
        }

        public EDHeldDatas GetDataByItemUniqueID(string itemUniqueID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                          D.ID,D.Name,D.UniqueID,D.DataSource,D.Size,D.CreateBy,D.MarkState,D.IsCurrent,D.Version,D.ModifiedTime,D.DisplayURL,D.FileURL,D.MetaDataURL,
                                          D.DeviceID,D.SPGuid,D.FarmID,D.WebAppID,D.SiteID,D.WebID,D.ListID,D.DataType,D.SubJobID,D.SiteURL,D.PathMD5     
                                        FROM 
                                          CPLED_HeldData AS D join CPLED_HoldRelations AS R 
                                        ON 
                                          R.DataUniqueID = D.UniqueID 
                                        WHERE 
                                          R.ItemUniqueID = @ItemUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@itemUniqueID", itemUniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldDatas datas = new EDHeldDatas();
            while (reader.Read)
            {
                EDHeldData data = new EDHeldData();
                data.ID = reader.GetGuid(0);
                data.Name = reader.GetString(1);
                data.UniqueID = reader.GetString(2);
                data.DataSource = Enumer.Parse<DataSource>(reader.GetInt(3));
                data.Size = reader.GetLong(4);
                data.CreateBy = reader.GetString(5);
                data.MarkState = Enumer.Parse<MarkState>(reader.GetInt(6));
                data.IsCurrent = reader.GetInt(7) == 1;
                data.Version = reader.GetString(8);
                data.ModifiedTime = reader.GetDateTime(9);
                data.DisplayURL = reader.GetString(10);
                data.FileURL = reader.GetString(11);
                data.MetaDataURL = reader.GetString(12);
                data.DeviceID = reader.GetString(13);
                data.SPGuid = reader.GetGuid(14);
                data.FarmID = reader.GetString(15);
                data.WebAppID = reader.GetGuid(16);
                data.SiteID = reader.GetGuid(17);
                data.WebID = reader.GetGuid(18);
                data.ListID = reader.GetGuid(19);
                data.DataType = Enumer.Parse<DataType>(reader.GetInt(20));
                data.SubJobID = reader.GetString(21);
                data.SiteURL = reader.GetString(22);
                data.PathMD5 = reader.GetString(23);
                datas.Add(data);
            }
            cmd.Dispost();
            reader.Close();
            return datas;
        }
    }
}
