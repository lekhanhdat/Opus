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
using System.Data;
using System.Data.SqlClient;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    /// <summary>
    /// todo:wbhu,存放一些用DataReader初始化AveObject对象的方法，功能与DiscoverUtility里的部分类似，以后要挪出去的时候可以考虑合并下
    /// </summary>
    internal static class AveDiscoverSqlUtility
    {
        /// <summary>
        /// 根据DataReader初始化AveWebObject对象
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="rootWebUrlLength"></param>
        /// <returns></returns>
        public static AveWebObject GetWebInfoByDataReader(IDataRecord sr, int rootWebUrlLength)
        {
            var fullUrl = sr.GetString(1);
            var name = rootWebUrlLength < 0 ? "." : fullUrl.Substring(rootWebUrlLength).TrimStart('/');
            var web = new AveWebObject
            {
                WebID = sr.GetGuid(0),
                Name = name,
                Title = sr.IsDBNull(2) ? String.Empty : sr.GetString(2),
                FullUrl = fullUrl,
                IsAppWeb = !sr.GetGuid(4).Equals(Guid.Empty),
                AppInstanceId = sr.GetGuid(4),
                DeleteTransactionId= (byte[])sr["DeleteTransactionId"]
            };
            return web;
        }

        /// <summary>
        /// 根据DataReader初始化AveListObject对象
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static AveListObject GetListInfoByDataReader(IDataRecord sr)
        {
            var listObj = new AveListObject();
            InitListInfoByDataReader(sr, listObj);
            var bytes = (byte[])sr[8];
            var fieldsSchema = string.Empty;
            if (bytes != null && bytes.Length > 0)
            {
                fieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
            }
            if (fieldsSchema != null && fieldsSchema.Contains("<"))
            {
                fieldsSchema = fieldsSchema.Substring(fieldsSchema.IndexOf("<", StringComparison.OrdinalIgnoreCase));
            }
            listObj.Fields = "<Fields>" + fieldsSchema + "</Fields>";
            return listObj;
        }

        /// <summary>
        /// 根据DataReader初始化AveListObject对象
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="listObj"></param>
        /// <returns></returns>
        public static void InitListInfoByDataReader(IDataRecord sr, AveListObject listObj)
        {
            var name = sr.GetString(1);
            var rootFolderId = sr.GetGuid(2);
            var nodeType = sr.GetInt32(3);
            var flag = sr.GetInt64(4);
            var rootFolderUrl = sr.GetString(5).Trim('/');
            var serverTemplate = sr.GetInt32(6);
            var deleteTransactionId = (byte[])sr["tp_DeleteTransactionId"];

            listObj.ListId = sr.GetGuid(0);
            listObj.RootFolderId = rootFolderId;
            listObj.Name = name;
            listObj.Title = name;
            listObj.Type = nodeType;
            listObj.RootFolderUrl = rootFolderUrl;
            listObj.Flag = flag;
            listObj.ServerTemplate = serverTemplate;
            listObj.Hidden = (flag & 0x100L) != 0L;
            listObj.DeleteTransactionId = deleteTransactionId;
        }

        /// <summary>
        /// 根据DataReader初始化list的基本属性
        /// </summary>
        /// <param name="listObj"></param>
        /// <param name="reader"></param>
        public static void InitListObjBasicPropertiesByReader(AveListObject listObj, IDataRecord reader)
        {
            listObj.Name = (string) reader["tp_Title"];
            listObj.Title = (string) reader["tp_Title"];
            listObj.RootFolderId = (Guid) reader["tp_RootFolder"];
            listObj.Type = (int) reader["tp_BaseType"];
            listObj.Flag = (long) reader["tp_Flags"];
            listObj.ServerTemplate = (int?) reader["tp_ServerTemplate"];
            listObj.Hidden = ((long) reader["tp_Flags"] & 0x100L) != 0L;
        }

        public static AveSecurityObject GetSecurityObjectInfoByReader(SqlDataReader sr)
        {
            var nativeChangeType = (NativeChangeType) sr.GetValue(0);

            var securityType = DiscoverUtility.GetSecurityObjectType(nativeChangeType);
            var changeType = DiscoverUtility.GetSecurityChangeType(nativeChangeType);
            var changeTime = sr.IsDBNull(5) ? DateTime.MinValue : sr.GetDateTime(5);
            var securityObject = new AveSecurityObject
            {
                ObjectType = securityType,
                ChangeType = changeType,
                PrincipleId = sr.IsDBNull(2) ? -1 : sr.GetInt32(2),
                RoleId = sr.IsDBNull(3) ? -1 : sr.GetInt32(3),
                ScopeId = sr.IsDBNull(4) ? Guid.Empty : sr.GetGuid(4),
                EventTime = changeTime
            };
            return securityObject;
        }
    }
}