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
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBHoldItemService : AbstractService
    {

        public EDBHoldItemService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {
        }

        #region - 添加Hold Item -

        public int InsertHoldItem(EDHoldItem item)
        {
            #region - executeSql -

            const string executeSql = @"INSERT INTO 
                                        CPLED_HoldItem 
                                      (ID,Name,Description,ModifiedTime,ManagedBy,FullPath,UniqueID,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid) 
                                        VALUES 
                                      (@ID,@Name,@Description,@ModifiedTime,@ManagedBy,@FullPath,@UniqueID,@Type,@ParentID,@FarmID,@WebAppID,@SiteID,@WebID,@ListID,@SPGuid) ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@ID",Guid.NewGuid());
            cmd.AddValue("@Name",item.Name);
            cmd.AddValue("@Description",item.Description);
            cmd.AddValue("@ModifiedTime",item.ModifiedTime.IsNullTime()?DateTime.UtcNow:item.ModifiedTime);
            cmd.AddValue("@ManagedBy",item.ManagedBy);
            cmd.AddValue("@FullPath",item.FullPath);
            cmd.AddValue("@UniqueID",item.UniqueID);
            cmd.AddValue("@Type",item.ItemType);
            cmd.AddValue("@ParentID",item.ParentID);
            cmd.AddValue("@FarmID",item.FarmID);
            cmd.AddValue("@WebAppID",item.WebAppID);
            cmd.AddValue("@SiteID",item.SiteID);
            cmd.AddValue("@WebID",item.WebID);
            cmd.AddValue("@ListID",item.ListID);
            cmd.AddValue("@SPGuid",item.SPGuid);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 编辑Hold Item -

        public int EditHoldItem(EDHoldItem item)
        {
            #region - execute sql -

            const string executeSql = @"UPDATE 
                                        CPLED_HoldItem 
                                      SET 
                                        Name = @Name,
                                        Description = @Description,
                                        ModifiedTime = @ModifiedTime,
                                        ManagedBy = @ManagedBy,
                                        FullPath = @FullPath,
                                        Type = @Type,
                                        ParentID = @ParentID,
                                        FarmID = @FarmID,
                                        WebAppID = @WebAppID,
                                        SiteID = @SiteID,
                                        WebID = @WebID,
                                        ListID = @ListID,
                                        SPGuid = @SPGuid 
                                      WHERE 
                                        UniqueID = @UniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@Name",item.Name);
            cmd.AddValue("@Description",item.Description);
            cmd.AddValue("@ModifiedTime", item.ModifiedTime.IsNullTime() ? DateTime.UtcNow : item.ModifiedTime);
            cmd.AddValue("@ManagedBy",item.ManagedBy);
            cmd.AddValue("@FullPath",item.FullPath);
            cmd.AddValue("@Type",item.ItemType);
            cmd.AddValue("@ParentID",item.ParentID);
            cmd.AddValue("@FarmID",item.FarmID);
            cmd.AddValue("@WebAppID",item.WebAppID);
            cmd.AddValue("@SiteID",item.SiteID);
            cmd.AddValue("@WebID",item.WebID);
            cmd.AddValue("@ListID",item.ListID);
            cmd.AddValue("@UniqueID",item.UniqueID);
            cmd.AddValue("@SPGuid",item.SPGuid);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 删除Hold Item - 

        /// <summary>
        /// 根据UniqueID删除一个SharePoint HoldItem.
        /// Review 2012/05/10
        /// </summary>
        /// <param name="edHoldItem">SharePoint HoldItem.</param>
        /// <returns></returns>
        public int DeleteHoldItem(EDHoldItem edHoldItem)
        {
            #region - executeSql -

            //查询该Item,是否有对应关系.如果不存在关联则进行删除.
            const string executeSql =
                                        @"DELETE FROM 
	                                        CPLED_HoldItem 
                                          WHERE 
	                                        UniqueID = @UniqueID";

            #endregion

            var edSqlCommand = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            edSqlCommand.AddValue("@UniqueID", edHoldItem.UniqueID);
            try
            {
                var count = edSqlCommand.ExecuteNonQuery();
                edSqlCommand.Dispost();
                return count;
            }
            catch (Exception ex)
            {
                throw new Exception("DeleteSPHoldItemOnly Failed. /r/n Message: " + ex.Message + " /r/n " + edHoldItem,ex);
            }
        }

        #endregion

        #region - 获得Hold Item -

        public EDHoldItem GetHoldItem(string unqiueID)
        {
            #region - execute sql - 

            string executeSql = @"SELECT 
                                    ID,Name,Description,ModifiedTime,ManagedBy,FullPath,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                  FROM 
                                    CPLED_HoldItem 
                                  WHERE                           UniqueID = @UniqueID ";

            #endregion

            EDHoldItem item = null;
            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID",unqiueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read)
            {
                item = new EDHoldItem();
                item.ID = reader.GetGuid(0);
                item.Name = reader.GetString(1);
                item.Description = reader.GetString(2);
                item.ModifiedTime = reader.GetDateTime(3);
                item.ManagedBy = reader.GetString(4);
                item.FullPath = reader.GetString(5);
                item.ItemType = Enumer.Parse<ItemType>(reader.GetInt(6));
                item.ParentID = reader.GetString(7);
                item.FarmID = reader.GetString(8);
                item.WebAppID = reader.GetGuid(9);
                item.SiteID = reader.GetGuid(10);
                item.WebID = reader.GetGuid(11);
                item.ListID = reader.GetGuid(12);
                item.SPGuid = reader.GetGuid(13);
            }
            cmd.Dispost();
            reader.Close();
            return item;
        }

        #endregion

        #region - 根据WebID获得所有Item - 

        /// <summary>
        /// 根据WebID，获取Web下所有HoldItem，同步用
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webID"></param>
        /// <returns></returns>
        public EDHoldItems GetHoldItems(string farmID, Guid webID)
        {
            #region - execute sql - 

            const string executeSql = @"SELECT 
                                          ID,Name,Description,ModifiedTime,ManagedBy,FullPath,UniqueID,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                      FROM 
                                          CPLED_HoldItem 
                                      WHERE 
                                          FarmID = @FarmID 
                                      AND 
                                          WebID = @WebID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@FarmID",farmID);
            cmd.AddValue("@WebID",webID);
            EDSqlDataReader reader = cmd.ExecuteReader();

            EDHoldItems items = new EDHoldItems();

            while (reader.Read)
            {
                EDHoldItem item = new EDHoldItem();
                item.ID = reader.GetGuid(0);
                item.Name = reader.GetString(1);
                item.Description = reader.GetString(2);
                item.ModifiedTime = reader.GetDateTime(3);
                item.ManagedBy = reader.GetString(4);
                item.FullPath = reader.GetString(5);
                item.UniqueID = reader.GetString(6);
                item.ItemType = Enumer.Parse<ItemType>(reader.GetInt(7));
                item.ParentID = reader.GetString(8);
                item.FarmID = reader.GetString(9);
                item.WebAppID = reader.GetGuid(10);
                item.SiteID = reader.GetGuid(11);
                item.WebID = reader.GetGuid(12);
                item.ListID = reader.GetGuid(13);
                item.SPGuid = reader.GetGuid(14);
                items.Add(item);
            }
            cmd.Dispost();
            reader.Close();
            return items;


        }

        #endregion

        #region-根据WebAppID获取HoldItem-
        /// <summary>
        /// 根据WebAppId，获取WebAplication下的所有HoldItem，同步用
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="webAppID"></param>
        /// <returns></returns>
        public EDHoldItems GetHoldItemsByWebAppID(string farmID, Guid webAppID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                          ID,Name,Description,ModifiedTime,ManagedBy,FullPath,UniqueID,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                      FROM 
                                          CPLED_HoldItem 
                                      WHERE 
                                          FarmID = @FarmID 
                                      AND 
                                          WebAppID = @WebAppID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@WebAppID", webAppID);
            EDSqlDataReader reader = cmd.ExecuteReader();

            EDHoldItems items = new EDHoldItems();

            while (reader.Read)
            {
                EDHoldItem item = new EDHoldItem();
                item.ID = reader.GetGuid(0);
                item.Name = reader.GetString(1);
                item.Description = reader.GetString(2);
                item.ModifiedTime = reader.GetDateTime(3);
                item.ManagedBy = reader.GetString(4);
                item.FullPath = reader.GetString(5);
                item.UniqueID = reader.GetString(6);
                item.ItemType = Enumer.Parse<ItemType>(reader.GetInt(7));
                item.ParentID = reader.GetString(8);
                item.FarmID = reader.GetString(9);
                item.WebAppID = reader.GetGuid(10);
                item.SiteID = reader.GetGuid(11);
                item.WebID = reader.GetGuid(12);
                item.ListID = reader.GetGuid(13);
                item.SPGuid = reader.GetGuid(14);
                items.Add(item);
            }
            cmd.Dispost();
            reader.Close();
            return items;
        }
        #endregion

        #region -根据SiteID获取HoldItems-
        /// <summary>
        /// 根据SiteID，获取site下的所有HoldItem，同步用
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="siteID"></param>
        /// <returns></returns>
        public EDHoldItems GetHoldItemsBySiteID(string farmID, Guid siteID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                          ID,Name,Description,ModifiedTime,ManagedBy,FullPath,UniqueID,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                      FROM 
                                          CPLED_HoldItem 
                                      WHERE 
                                          FarmID = @FarmID 
                                      AND 
                                          SiteID = @SiteID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@SiteID", siteID);
            EDSqlDataReader reader = cmd.ExecuteReader();

            EDHoldItems items = new EDHoldItems();

            while (reader.Read)
            {
                EDHoldItem item = new EDHoldItem();
                item.ID = reader.GetGuid(0);
                item.Name = reader.GetString(1);
                item.Description = reader.GetString(2);
                item.ModifiedTime = reader.GetDateTime(3);
                item.ManagedBy = reader.GetString(4);
                item.FullPath = reader.GetString(5);
                item.UniqueID = reader.GetString(6);
                item.ItemType = Enumer.Parse<ItemType>(reader.GetInt(7));
                item.ParentID = reader.GetString(8);
                item.FarmID = reader.GetString(9);
                item.WebAppID = reader.GetGuid(10);
                item.SiteID = reader.GetGuid(11);
                item.WebID = reader.GetGuid(12);
                item.ListID = reader.GetGuid(13);
                item.SPGuid = reader.GetGuid(14);
                items.Add(item);
            }
            cmd.Dispost();
            reader.Close();
            return items;
        }
        #endregion

        #region - Exists Methods -

        #region - 根据ItemUniqueID,查询此Item是否存在 -

        public bool ExistsHoldItem(string uniqueID)
        {
            #region - executeSql -

            const string executeSql = @"SELECT COUNT(ID) FROM CPLED_HoldItem WHERE UniqueID = @UniqueID";

            #endregion

            EDSqlCommand edSqlCommand = new EDSqlCommand(this.Conn.CreateCommand()) { CommandText = executeSql };
            edSqlCommand.AddValue("@UniqueID", uniqueID);
            int cnt = (int)edSqlCommand.ExecuteScalar();
            edSqlCommand.Dispost();
            return cnt > 0;
        }

        public bool ExistsHoldItem(EDHoldItem edHoldItem)
        {
            return ExistsHoldItem(edHoldItem.UniqueID);
        }

        #endregion

        #endregion

        #region - 用DocAve Item UniqueID获得子child -

        public EDHoldItems GetChild(string uniqueID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                        ID,Name,Description,ModifiedTime,ManagedBy,FullPath,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid 
                                      FROM 
                                        CPLED_HoldItem 
                                      WHERE 
                                        ParentID = @UniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID",uniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHoldItems items =SetupResults(reader);
            cmd.Dispost();
            reader.Close();
            return items;
        }

        #endregion

        #region - 使用FarmID 获得所有的DcoAve Hold Item -

        public EDHoldItems GetDocAveItems(string farmID)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                        ID,Name,Description,ModifiedTime,ManagedBy,FullPath,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                      FROM 
                                        CPLED_HoldItem 
                                      WHERE 
                                        FarmID = @FarmID 
                                      AND 
                                        Type = @Type";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@FarmID",farmID);
            cmd.AddValue("@Type",ItemType.DocAveItem);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHoldItems items = SetupResults(reader);
            reader.Close();
            return items;
        }

        #endregion

        public int UpdateIitemByID (EDHoldItem item)
        {
            #region - execute sql -

            const string executeSql = @"UPDATE 
                                        CPLED_HoldItem 
                                      SET 
                                        Name = @Name,
                                        Description = @Description,
                                        ModifiedTime = @ModifiedTime,
                                        ManagedBy = @ManagedBy,
                                        FullPath = @FullPath,
                                        Type = @Type,
                                        ParentID = @ParentID,
                                        FarmID = @FarmID,
                                        WebAppID = @WebAppID,
                                        SiteID = @SiteID,
                                        WebID = @WebID,
                                        ListID = @ListID,
                                        SPGuid = @SPGuid,
                                        UniqueID = @UniqueID  
                                      WHERE 
                                        ID = @ID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@Name", item.Name);
            cmd.AddValue("@Description", item.Description);
            cmd.AddValue("@ModifiedTime", item.ModifiedTime.IsNullTime() ? DateTime.UtcNow : item.ModifiedTime);
            cmd.AddValue("@ManagedBy", item.ManagedBy);
            cmd.AddValue("@FullPath", item.FullPath);
            cmd.AddValue("@Type", item.ItemType);
            cmd.AddValue("@ParentID", item.ParentID);
            cmd.AddValue("@FarmID", item.FarmID);
            cmd.AddValue("@WebAppID", item.WebAppID);
            cmd.AddValue("@SiteID", item.SiteID);
            cmd.AddValue("@WebID", item.WebID);
            cmd.AddValue("@ListID", item.ListID);
            cmd.AddValue("@UniqueID", item.UniqueID);
            cmd.AddValue("@SPGuid", item.SPGuid);
            cmd.AddValue("@ID",item.ID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        public EDHoldItems GetAllHoldItems(string farmId)
        {
            #region - execute sql -

            const string executeSql =
                @"SELECT 
                                          ID,Name,Description,ModifiedTime,ManagedBy,FullPath,Type,ParentID,FarmID,WebAppID,SiteID,WebID,ListID,SPGuid  
                                      FROM 
                                          CPLED_HoldItem 
                                      WHERE 
                                          FarmID = @FarmID
                                      AND 
                                          Type != 0";

            #endregion
            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmId);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHoldItems items = SetupResults(reader);
            cmd.Dispost();
            reader.Close();
            return items;
        }

        #region - Private Method -

        private EDHoldItems SetupResults(EDSqlDataReader reader)
        {
            EDHoldItems items = new EDHoldItems();
            while (reader.Read)
            {
                EDHoldItem item = new EDHoldItem();
                item.ID = reader.GetGuid(0);
                item.Name = reader.GetString(1);
                item.Description = reader.GetString(2);
                item.ModifiedTime = reader.GetDateTime(3);
                item.ManagedBy = reader.GetString(4);
                item.FullPath = reader.GetString(5);
                item.ItemType = Enumer.Parse<ItemType>(reader.GetInt(6));
                item.ParentID = reader.GetString(7);
                item.FarmID = reader.GetString(8);
                item.WebAppID = reader.GetGuid(9);
                item.SiteID = reader.GetGuid(10);
                item.WebID = reader.GetGuid(11);
                item.ListID = reader.GetGuid(12);
                item.SPGuid = reader.GetGuid(13);
                items.Add(item);
            }
            return items;
        }

        #endregion
    }
}