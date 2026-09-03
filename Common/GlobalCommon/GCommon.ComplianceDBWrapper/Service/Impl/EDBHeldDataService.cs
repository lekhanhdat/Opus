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
using AvePoint.GCommon.ComplianceDBWrapper.Common;
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;
using AvePoint.GCommon.Utility;
using System.Collections.Generic;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBHeldDataService : AbstractService
    {
        public EDBHeldDataService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {
        }

        #region - 插入 -

        /// <summary>
        /// 插入Held Data 记录..
        /// </summary>
        /// <param name="data">EDHeldData</param>
        public int Insert(EDHeldData data)
        {

            #region - ExecuteSql - 

            const string executeSql = @"IF NOT EXISTS (SELECT ID FROM CPLED_HeldData WHERE UniqueID = @UniqueID)
                                          INSERT INTO CPLED_HeldData 
	                                        (ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,DisplayURL,FileURL,MetaDataURL,DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType,SubJobID,SiteURL,PathMD5)
                                          VALUES
	                                        (@ID,@Name,@UniqueID,@DataSource,@Size,@CreateBy,@MarkState,@IsCurrent,@Version,@ModifiedTime,@DisplayURL,@FileURL,@MetaDataURL,@DeviceID,@SPGuid,@FarmID,@WebAppID,@SiteID,@WebID,@ListID,@DataType,@SubJobID,@SiteURL,@PathMD5)";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            Guid dataID = Guid.NewGuid();
            cmd.AddValue("@ID", dataID);
            cmd.AddValue("@Name", data.Name);
            cmd.AddValue("@UniqueID",data.UniqueID);
            cmd.AddValue("@DataSource",data.DataSource);
            cmd.AddValue("@Size",data.Size);
            cmd.AddValue("@CreateBy",data.CreateBy);
            cmd.AddValue("@MarkState",MarkState.None);
            cmd.AddValue("@IsCurrent",data.IsCurrent ? 1:0);
            cmd.AddValue("@Version",data.Version);
            cmd.AddValue("@ModifiedTime",data.ModifiedTime);
            cmd.AddValue("@DisplayURL",data.DisplayURL);
            cmd.AddValue("@FileURL", data.FileURL);
            cmd.AddValue("@MetaDataURL", data.MetaDataURL);
            cmd.AddValue("@DeviceID",data.DeviceID);
            cmd.AddValue("@SPGuid",data.SPGuid);
            cmd.AddValue("@FarmID",data.FarmID);
            cmd.AddValue("@WebAppID",data.WebAppID);
            cmd.AddValue("@SiteID",data.SiteID);
            cmd.AddValue("@WebID",data.WebID);
            cmd.AddValue("@ListID",data.ListID);
            cmd.AddValue("@DataType",data.DataType);
            cmd.AddValue("@SubJobID", data.SubJobID);
            cmd.AddValue("@SiteURL",data.SiteURL);
            cmd.AddValue("@PathMD5",data.PathMD5);
            int insertCnt = cmd.ExecuteNonQuery();
            if(insertCnt > 0)
            {
                if(!data.ContentStorageInfo.IsNull())
                {
                    data.ContentStorageInfo.DataID = data.UniqueID;
                    DBWrapper.Use<EDBStorageInfoService>().Insert(data.ContentStorageInfo);
                }
                if(!data.ContentStorageInfo.IsNull())
                {
                    data.MetadataStorageInfo.DataID = data.UniqueID;
                    DBWrapper.Use<EDBStorageInfoService>().Insert(data.MetadataStorageInfo);
                }
            }
            cmd.Dispost();
            return insertCnt;
        }

        #endregion

        #region - 更新纪录 -

        /// <summary>
        /// 根据Held Data记录..
        /// </summary>
        /// <param name="data"> </param>
        public int UpdateByUniqueID(EDHeldData data)
        {
            #region - executeSQL - 

            const string executeSql = @"UPDATE CPLED_HeldData 
                                        SET
	                                        Name = @Name,
	                                        DataSource = @DataSource,
	                                        Size = @Size,
	                                        CreateBy = @CreateBy,
	                                        MarkState = @MarkState,
	                                        IsCurrent = @IsCurrent,
	                                        Version = @Version,
	                                        ModifiedTime = @ModifiedTime,
                                            DisplayURL = @DisplayURL,
                                            FileURL = @FileURL,
                                            MetaDataURL = @MetaDataURL,
                                            DeviceID = @DeviceID,
                                            SPGuid = @SPGuid,
                                            FarmID = @FarmID,
                                            WebAppID = @WebAppID,
                                            SiteID = @SiteID,
                                            WebID = @WebID,
                                            ListID = @ListID,
                                            DataType = @DataType  
                                        WHERE 
	                                        UniqueID = @UniqueID";
            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID", data.UniqueID);
            cmd.AddValue("@Name", data.Name);
            cmd.AddValue("@DataSource", data.DataSource);
            cmd.AddValue("@Size", data.Size);
            cmd.AddValue("@CreateBy",data.CreateBy);
            cmd.AddValue("@MarkState", data.MarkState);
            cmd.AddValue("@IsCurrent", data.IsCurrent?1:0);
            cmd.AddValue("@ModifiedTime", data.ModifiedTime);
            cmd.AddValue("@DisplayURL", data.DisplayURL);
            cmd.AddValue("@FileURL", data.FileURL);
            cmd.AddValue("@MetaDataURL", data.MetaDataURL);
            cmd.AddValue("@DeviceID",data.DeviceID);
            cmd.AddValue("@SPGuid",data.SPGuid);
            cmd.AddValue("@FarmID", data.FarmID);
            cmd.AddValue("@WebAppID", data.WebAppID);
            cmd.AddValue("@SiteID", data.SiteID);
            cmd.AddValue("@WebID", data.WebID);
            cmd.AddValue("@ListID", data.ListID);
            cmd.AddValue("@DataType", data.DataType);
            int cnt = cmd.ExecuteNonQuery();
            if (cnt > 0)
            {
                if (!data.ContentStorageInfo.IsNull())
                {
                    data.ContentStorageInfo.DataID = data.UniqueID;
                    DBWrapper.Use<EDBStorageInfoService>().Insert(data.ContentStorageInfo);
                }
                if (!data.ContentStorageInfo.IsNull())
                {
                    data.MetadataStorageInfo.DataID = data.UniqueID;
                    DBWrapper.Use<EDBStorageInfoService>().Insert(data.MetadataStorageInfo);
                }
            }
            cmd.Dispost();
            return cnt;
        }

        #endregion

        #region - 根据数据的唯一性删除数据 -

        public int DeleteByUniqueID(string uniqueID)
        {
            #region - executeSql - 

            const string executeSql = @"DELETE FROM CPLED_HeldData WHERE UniqueID = @UniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID",uniqueID);
            DBWrapper.Use<EDBStorageInfoService>().Delete(uniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        public int DeleteByUnique(EDHeldData data)
        {
            return DeleteByUniqueID(data.UniqueID);
        }

        #endregion

        #region - 根据UniqueID获得数据 -

        public EDHeldData GetDataByUniqueID(string uniqueID)
        {
            #region - ExecuteSql -

            const string executeSql = @"SELECT 
                                          ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,DisplayURL,FileURL,MetaDataURL,
                                          DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType,SubJobID,SiteURL,PathMD5     
                                      FROM 
                                          CPLED_HeldData 
                                      WHERE 
                                          UniqueID = @UniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@UniqueID", uniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldData data = null;
            while (reader.Read)
            {
                data = new EDHeldData();
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
            }
            cmd.Dispost();
            reader.Close();
            if (data != null)
            {
                EDStorageInfo metadata = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(data.UniqueID, StorageType.Metadata);
                EDStorageInfo content = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(data.UniqueID, StorageType.Content);
                data.MetadataStorageInfo = metadata;
                data.ContentStorageInfo = content;
            }
            return data;
        }

        #endregion

        #region - Mark Methods - 

        public int MarkWebAppSPData(string farmID,Guid webAppID,MarkState markState)
        {
            #region - execute sql - 

            const string executeSql = @"UPDATE CPLED_HeldData SET MarkState = @MarkState WHERE WebAppID = @WebAppID AND FarmID = @FarmID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@MarkState",markState);
            cmd.AddValue("@WebAppID",webAppID);
            cmd.AddValue("@FarmID",farmID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        public int MarkSiteSPData(string farmID,Guid siteID,MarkState markState)
        {

            #region - execute sql -

            const string executeSql = @"UPDATE CPLED_HeldData SET MarkState = @MarkState WHERE SiteID = @SiteID AND FarmID = @FarmID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@MarkState",markState);
            cmd.AddValue("@SiteID",siteID);
            cmd.AddValue("@FarmID",farmID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        public int MarkWebSPData(string farmID,Guid webID,MarkState markState)
        {
            #region - execute sql -

            const string executeSql = @"UPDATE CPLED_HeldData SET MarkState = @MarkState WHERE WebID = @WebID AND FarmID = @FarmID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()){CommandText = executeSql};
            cmd.AddValue("@MarkState",markState);
            cmd.AddValue("@WebID",webID);
            cmd.AddValue("@FarmID",farmID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 根据FarmID，ListID获得List Level的HeldDatas -

        /// <summary>
        /// 对于List删除的情况
        /// 根据ListID获取所有HeldData
        /// </summary>
        /// <param name="farmID"></param>
        /// <param name="listID"></param>
        /// <returns></returns>
        public EDHeldDatas GetHeldDataByListID(string farmID, Guid listID)
        {

            #region - execute sql -

            const string executeSql = @"SELECT 
                                            ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,
                                            DisplayURL,FileURL,MetaDataURL,DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType 
                                          FROM 
                                            CPLED_HeldData 
                                          WHERE 
                                            FarmID = @FarmID 
                                          AND 
                                            ListID = @ListID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@FarmID",farmID);
            cmd.AddValue("@ListID",listID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldDatas datas = new EDHeldDatas();
            while(reader.Read)
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
                datas.Add(data);
            }
            cmd.Dispost();
            reader.Close();
            foreach (var edHeldData in datas)
            {
                edHeldData.MetadataStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Metadata);
                edHeldData.ContentStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Content);
            }
            
            return datas;
        }

        #endregion

        #region - Exists Method -

        #region - 验证HeldData是否存在 -
        /// <summary>
        /// 验证需要检查的HeldData是否存在于Compliance DB中,支持SharePoint与Archive Data,不支持SP Current Data的Last Modify Time检查
        /// </summary>
        /// <param name="edHeldData"></param>
        /// <returns></returns>
        public bool ExistsHeldData(EDHeldData edHeldData)
        {
            #region - executeSql -

            const string executeSql = @"SELECT COUNT(ID) FROM CPLED_HeldData WHERE UniqueID = @UniqueID";

            #endregion
            EDSqlCommand edSqlCommand = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            edSqlCommand.AddValue("@UniqueID", edHeldData.UniqueID);
            int count = (int)edSqlCommand.ExecuteScalar();
            edSqlCommand.Dispost();
            return count > 0;
        }

        #endregion

        #endregion

        #region - 获得与当前版本不同的,SharePoint Data当前版本 -

        public EDHeldData GetNotSameSPCurrentData(string uniqueID, DateTime modifiedTime)
        {

            #region - ExecuteSql -

            const string executeSql = @"SELECT 
                                          ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,DisplayURL,FileURL,MetaDataURL,
                                          DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType   
                                      FROM 
                                          CPLED_HeldData 
                                      WHERE 
                                          UniqueID = @UniqueID AND ModifiedTime != @ModifiedTime AND DataSource = 0 ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@UniqueID", uniqueID);
            cmd.AddValue("@ModifiedTime", modifiedTime);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldData data = null;
            while (reader.Read)
            {
                data = new EDHeldData();
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
            }
            cmd.Dispost();
            reader.Close();
            if (data != null)
            {
                EDStorageInfo metadata = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(data.UniqueID, StorageType.Metadata);
                EDStorageInfo content = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(data.UniqueID, StorageType.Content);
                data.MetadataStorageInfo = metadata;
                data.ContentStorageInfo = content;
            }
            return data;
        }

        public EDHeldData GetNotSameSPCurrentData(EDHeldData edHeldData)
        {
            return GetNotSameSPCurrentData(edHeldData.UniqueID, edHeldData.ModifiedTime);
        }

        #endregion

        #region - 检查是否可以删除 -

        public bool CanDelete(string dataUniqueID)
        {
            #region

            const string executeSql = @"SELECT COUNT(ID) FROM CPLED_HoldRelations WHERE DataUniqueID = @DataUniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@DataUniqueID", dataUniqueID);
            int count = (int)cmd.ExecuteScalar();
            cmd.Dispost();
            return count == 0;
        }

        #endregion

        #region - Get IDs By Scope -

        public List<Guid> GetAllSiteIDs(string farmID, Guid webAppID)
        {
            List<Guid> siteIDs = new List<Guid>();

            #region - execute sql -

            const string executeSql = @"SELECT DISTINCT SiteID FROM CPLED_HeldData WHERE WebAppID = @WebAppID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@WebAppID", webAppID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                siteIDs.Add(reader.GetGuid(0));
            }
            cmd.Dispost();
            reader.Close();
            return siteIDs;
        }

        public List<Guid> GetAllWebIDs(string farmID, Guid siteID)
        {
            List<Guid> webIDs = new List<Guid>();

            #region - execute sql -

            const string executeSql = @"SELECT DISTINCT WebId FROM CPLED_HeldData WHERE SiteID = @SiteID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@SiteID", siteID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                webIDs.Add(reader.GetGuid(0));
            }
            cmd.Dispost();
            reader.Close();
            return webIDs;
        }

        public List<Guid> GetAllListIDs(string farmID, Guid webID)
        {
            List<Guid> listIDs = new List<Guid>();

            #region - execute sql -

            const string executeSql = @"SELECT DISTINCT SiteID FROM CPLED_HeldData WHERE ListID = @ListID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@ListID", webID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                listIDs.Add(reader.GetGuid(0));
            }
            cmd.Dispost();
            reader.Close();
            return listIDs;
        }

        #endregion

        #region - 获得指定版本的SharePoint数据 -

        public EDHeldDatas GetSPData(string farmID, Guid spGuid, bool isCurrent)
        {
            #region - ExecuteSql -

            const string executeSql = @"SELECT 
                                          ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,DisplayURL,FileURL,MetaDataURL,
                                          DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType,SubJobID,SiteURL,PathMD5    
                                        FROM 
                                          CPLED_HeldData 
                                        WHERE 
                                          FarmID = @FarmID 
                                        AND 
                                          SPGuid = @SPGuid 
                                        AND 
                                          DataSource = 0 
                                        AND 
                                          IsCurrent = @IsCurrent ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@FarmID",farmID);
            cmd.AddValue("@SPGuid",spGuid);
            cmd.AddValue("@IsCurrent",isCurrent?1:0);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldDatas edHeldDatas = SetupResults(reader);
            cmd.Dispost();
            reader.Close();
            foreach (var edHeldData in edHeldDatas)
            {
                edHeldData.MetadataStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Metadata);
                edHeldData.ContentStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Content);
            }
            return edHeldDatas;
        }

        public EDHeldDatas GetSPDataWithOutStorageInfo(string farmID, Guid spGuid, bool isCurrent)
        {
            #region - ExecuteSql -

            const string executeSql = @"SELECT 
                                          ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,DisplayURL,FileURL,MetaDataURL,
                                          DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType,SubJobID,SiteURL,PathMD5    
                                        FROM 
                                          CPLED_HeldData 
                                        WHERE 
                                          FarmID = @FarmID 
                                        AND 
                                          SPGuid = @SPGuid 
                                        AND 
                                          DataSource = 0 
                                        AND 
                                          IsCurrent = @IsCurrent ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@SPGuid", spGuid);
            cmd.AddValue("@IsCurrent", isCurrent ? 1 : 0);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDHeldDatas edHeldDatas = SetupResults(reader);
            cmd.Dispost();
            reader.Close();
            return edHeldDatas;
        }

        #endregion

        #region - 根据File的URL或者MetaData URL获取数据UniqueID -

        public string GetUniqueID(string url)
        {
            string uniqueID = string.Empty;

            #region - execute sql -

            string executeSql = @"SELECT UniqueID FROM CPLED_HeldData WHERE FileURL = @FileURL OR MetaDataURL =@MetaDataURL";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddNValue("@FileURL", url,true);
            cmd.AddNValue("@MetaDataURL", url,true);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                uniqueID = reader.GetString(0);
            }
            cmd.Dispost();
            reader.Close();
            return uniqueID;
        }

        public  string GetCurrentVersionUniqueID(string url)
        {
            string uniqueID = string.Empty;
            const string executeSql = "select UniqueID from CPLED_HeldData where SPGuid=(select SPGuid from CPLED_HeldData where FileURL = @FileURL OR MetaDataURL =@MetaDataURL) and IsCurrent=1";
            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddNValue("@FileURL", url, true);
            cmd.AddNValue("@MetaDataURL", url, true);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                uniqueID = reader.GetString(0);
            }
            cmd.Dispost();
            reader.Close();
            return uniqueID;
        }

        #endregion

        #region - Private Method -

        private EDHeldData SetupResult(EDSqlDataReader reader)
        {
            EDHeldDatas datas = SetupResults(reader);
            return datas.Count > 0 ? datas[0] : null;
        }

        public EDHeldDatas SetupResults(EDSqlDataReader reader)
        {
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
            return datas;
        }

        #endregion

        public EDHeldDatas GetHeldDataBySPGuid(string farmID, Guid spGuid)
        {
            #region - execute sql -

            const string executeSql = @"SELECT 
                                            ID,Name,UniqueID,DataSource,Size,CreateBy,MarkState,IsCurrent,Version,ModifiedTime,
                                            DisplayURL,FileURL,MetaDataURL,DeviceID,SPGuid,FarmID,WebAppID,SiteID,WebID,ListID,DataType 
                                          FROM 
                                            CPLED_HeldData 
                                          WHERE 
                                            FarmID = @FarmID 
                                          AND 
                                            SPGuid = @SPGuid ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", farmID);
            cmd.AddValue("@SPGuid", spGuid);
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
                datas.Add(data);
            }
            cmd.Dispost();
            reader.Close();
            foreach (var edHeldData in datas)
            {
                edHeldData.MetadataStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Metadata);
                edHeldData.ContentStorageInfo = DBWrapper.Use<EDBStorageInfoService>().GetStorageInfo(edHeldData.UniqueID, StorageType.Content);
            }

            return datas;
        }

        public int DeleteBySPGuid(string farmId, Guid spGuid)
        {
            #region - executeSql -

            const string executeSql = @"DELETE FROM CPLED_HeldData WHERE SPGuid = @SPGuid ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@SPGuid", spGuid);
            EDHeldDatas heldDatas = GetHeldDataBySPGuid(farmId, spGuid);
            foreach (var heldData in heldDatas)
            {
                DBWrapper.Use<EDBStorageInfoService>().Delete(heldData.UniqueID);
            }
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }
    }
}
