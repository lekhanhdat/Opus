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
using System.Linq;
using System.Text;
using AvePoint.GCommon.ComplianceDBWrapper.Common;
using AvePoint.GCommon.ComplianceDBWrapper.Core;
using AvePoint.GCommon.ComplianceDBWrapper.Model;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Service.Impl
{
    public class EDBPropertyMappingService : AbstractService
    {
        public EDBPropertyMappingService(SqlConnection conn, EDDBWrapper dbWrapper) : base(conn, dbWrapper)
        {
        }

        #region - 向数据库插入一条记录 - FarmID + SSAID + FieldName + FieldType 为去重标准

        public int Insert(EDPropertyMapping propertyMapping)
        {
            #region - execute sql -

            string executeSql = @"IF NOT EXISTS (SELECT UniqueID FROM CPLED_PropertyMapping WHERE UniqueID = @UniqueID) 
                                  INSERT INTO 
                                       CPLED_PropertyMapping 
                                       (UniqueID,FarmID,SSAID,CurrentCrawlProperty,CurrentManagedProperty,VersionCrawlProperty,VersionManagedProperty,FieldName,FieldType) 
                                  VALUES 
                                       (@UniqueID,@FarmID,@SSAID,@CurrentCrawlProperty,@CurrentManagedProperty,@VersionCrawlProperty,@VersionManagedProperty,@FieldName,@FieldType) ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID",propertyMapping.UniqueID);
            cmd.AddValue("@FarmID",propertyMapping.FarmID);
            cmd.AddValue("@SSAID",propertyMapping.SSAID);
            cmd.AddValue("@CurrentCrawlProperty",propertyMapping.CurrentCrawlProperty);
            cmd.AddValue("@CurrentManagedProperty",propertyMapping.CurrentManagedProperty);
            cmd.AddValue("@VersionCrawlProperty",propertyMapping.VersionCrawlProperty);
            cmd.AddValue("@VersionManagedProperty",propertyMapping.VersionManagedProperty);
            cmd.AddValue("@FieldName",propertyMapping.FieldName);
            cmd.AddValue("@FieldType",propertyMapping.FieldType);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }

        #endregion

        #region - 查询一条Mapping记录是否存 -

        public bool Exists(EDPropertyMapping propertyMapping)
        {

            #region - execute sql -

            string executeSql = @"SELECT COUNT(UniqueID) FROM CPLED_PropertyMapping WHERE UniqueID = @UniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID",propertyMapping.UniqueID);
            int count = (int) cmd.ExecuteScalar();
            cmd.Dispost();
            return count > 0;
        }

        #endregion

        #region 更新Mapping记录

        public int Update(EDPropertyMapping propertyMapping)
        {
            #region - execute sql -

            string executeSql = @"UPDATE 
                                    CPLED_PropertyMapping 
                                  SET 
                                    FarmID = @FarmID,
                                    SSAID = @SSAID,
                                    CurrentCrawlProperty = @CurrentCrawlProperty,
                                    CurrentManagedProperty = @CurrentManagedProperty,
                                    VersionCrawlProperty = @VersionCrawlProperty,
                                    VersionManagedProperty = @VersionManagedProperty,
                                    FieldName = @FieldName,
                                    FieldType = @FieldType 
                                  WHERE 
                                    UniqueID = @UniqueID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) { CommandText = executeSql };
            cmd.AddValue("@FarmID", propertyMapping.FarmID);
            cmd.AddValue("@SSAID", propertyMapping.SSAID);
            cmd.AddValue("@CurrentCrawlProperty", propertyMapping.CurrentCrawlProperty);
            cmd.AddValue("@CurrentManagedProperty", propertyMapping.CurrentManagedProperty);
            cmd.AddValue("@VersionCrawlProperty", propertyMapping.VersionCrawlProperty);
            cmd.AddValue("@VersionManagedProperty", propertyMapping.VersionManagedProperty);
            cmd.AddValue("@FieldName", propertyMapping.FieldName);
            cmd.AddValue("@FieldType", propertyMapping.FieldType);
            cmd.AddValue("@UniqueID", propertyMapping.UniqueID);
            var count = cmd.ExecuteNonQuery();
            cmd.Dispost();
            return count;
        }
        #endregion

        #region - 根据UniqueID获得PropertyMapping记录 -

        public EDPropertyMapping GetPropertyMapping(string uniqueID)
        {
            #region - execute sql -

            string executeSql = @"SELECT 
                                    UniqueID,FarmID,SSAID,CurrentCrawlProperty,CurrentManagedProperty,VersionCrawlProperty,VersionManagedProperty,FieldName,FieldType 
                                  FROM 
                                    CPLED_PropertyMapping 
                                  WHERE 
                                    UniqueID = @UniqueID";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText = executeSql};
            cmd.AddValue("@UniqueID", uniqueID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            EDPropertyMapping propertyMapping = null;
            while(reader.Read)
            {
                propertyMapping = new EDPropertyMapping();
                propertyMapping.UniqueID = reader.GetString(0);
                propertyMapping.FarmID = reader.GetString(1);
                propertyMapping.SSAID = reader.GetGuid(2);
                propertyMapping.CurrentCrawlProperty = reader.GetString(3);
                propertyMapping.CurrentManagedProperty = reader.GetString(4);
                propertyMapping.VersionCrawlProperty = reader.GetString(5);
                propertyMapping.VersionManagedProperty = reader.GetString(6);
                propertyMapping.FieldName = reader.GetString(7);
                propertyMapping.FieldType = reader.GetString(8);
            }
            cmd.Dispost();
            reader.Close();
            return propertyMapping;
        }

        #endregion

        #region - 用FarmID,SSAID 下的所有记录 -

        public EDPropertyMappings GetPropertyMappings(string farmID, Guid ssaID)
        {
            EDPropertyMappings mappings = new EDPropertyMappings();
            
            #region - execute sql - 

            string executeSql = @"SELECT 
                                    UniqueID,FarmID,SSAID,CurrentCrawlProperty,CurrentManagedProperty,
                                    VersionCrawlProperty,VersionManagedProperty,FieldName,FieldType 
                                  FROM 
                                    CPLED_PropertyMapping 
                                  WHERE 
                                    FarmID = @FarmID 
                                  AND 
                                    SSAID = @SSAID ";

            #endregion

            EDSqlCommand cmd = new EDSqlCommand(Conn.CreateCommand()) {CommandText =  executeSql};
            cmd.AddValue("@FarmID",farmID);
            cmd.AddValue("@SSAID",ssaID);
            EDSqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read)
            {
                EDPropertyMapping mapping = new EDPropertyMapping();
                mapping.UniqueID = reader.GetString(0);
                mapping.FarmID = reader.GetString(1);
                mapping.SSAID = reader.GetGuid(2);
                mapping.CurrentCrawlProperty = reader.GetString(3);
                mapping.CurrentManagedProperty = reader.GetString(4);
                mapping.VersionCrawlProperty = reader.GetString(5);
                mapping.VersionManagedProperty = reader.GetString(6);
                mapping.FieldName = reader.GetString(7);
                mapping.FieldType = reader.GetString(8);
                mappings.Add(mapping);
            }
            cmd.Dispost();
            reader.Close();
            return mappings;
        }

        #endregion

    }
}
