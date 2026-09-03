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
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.ComplianceDBWrapper.Common;
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBStorageInfoService : AbstractService
    {
        public EDBStorageInfoService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {

        }

        #region - 插入一条StorageInfo记录 -

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public int Insert(EDStorageInfo info)
        {
            #region - execute sql -

            string executeSql = @"INSERT INTO 
                                      CPLED_StorageInfo 
                                  (ID,DataID,DataType,Type,Offset,HightName,LowName,Length,DataVersion,ExtraInfo,ClipID) 
                                  VALUES 
                                  (@ID,@DataID,@DataType,@Type,@Offset,@HightName,@LowName,@Length,@DataVersion,@ExtraInfo,@ClipID) ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@ID",Guid.NewGuid());
            cmd.AddValue("@DataID",info.DataID);
            cmd.AddValue("@DataType",info.DataType);
            cmd.AddValue("@Type",info.StorageType);
            cmd.AddValue("@Offset",info.Offset);
            cmd.AddValue("@HightName",info.HightName);
            cmd.AddValue("@LowName",info.LowName);
            cmd.AddValue("@Length",info.Length);
            cmd.AddValue("@DataVersion",info.DataVersion);
            cmd.AddValue("@ExtraInfo",info.ExtraInfo);
            cmd.AddValue("@ClipID",info.ClipID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 根据DataID,以及StorageType 获得StorageInfo -

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HightName is a protocol.")]
        public EDStorageInfo GetStorageInfo(string dataID,StorageType storageType)
        {
            #region - execute sql -

            string executeSql = @"SELECT 
                                    ID,DataID,DataType,Type,Offset,HightName,LowName,Length,DataVersion,ExtraInfo,ClipID  
                                  FROM 
                                    CPLED_StorageInfo 
                                  WHERE 
                                    DataID = @DataID 
                                  AND 
                                    Type = @StorageType ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@DataID",dataID);
            cmd.AddValue("@StorageType",storageType);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDStorageInfo info = null;
            bool readOne = reader.Read;
            if(readOne)
            {
                info = SetupResult(reader);
            }
            cmd.Dispost();
            reader.Close();
            return info;
        }

        #endregion

        #region - 根据Data ID删除Storage Info -

        public int Delete(string dataID)
        {
            #region - execute sql -

            string executeSql = @"DELETE FROM 
                                    CPLED_StorageInfo 
                                  WHERE 
                                    DataID = @DataID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@DataID",dataID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - Private Method -

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HightName is a protocol.")]
        private EDStorageInfo SetupResult(EDSqlDataReader reader)
        {
            EDStorageInfo storageInfo = new EDStorageInfo();
            storageInfo.ID = reader.GetGuid(0);
            storageInfo.DataID = reader.GetString(1);
            storageInfo.DataType = Enumer.Parse<DataType>(reader.GetInt(2));
            storageInfo.StorageType = Enumer.Parse<StorageType>(reader.GetInt(3));
            storageInfo.Offset = reader.GetLong(4);
            storageInfo.HightName = reader.GetString(5);
            storageInfo.LowName = reader.GetString(6);
            storageInfo.Length = reader.GetLong(7);
            storageInfo.DataVersion = Enumer.Parse<DataVersion>(reader.GetInt(8));
            storageInfo.ExtraInfo = reader.GetString(9);
            storageInfo.ClipID = reader.GetString(10);
            return storageInfo;
        }

        #endregion
    }
}
