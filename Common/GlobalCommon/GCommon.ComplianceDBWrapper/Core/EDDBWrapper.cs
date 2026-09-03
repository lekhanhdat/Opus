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
using AvePoint.GCommon.ComplianceDBWrapper.Service;
using AvePoint.GCommon.ComplianceDBWrapper.Utility;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.ComplianceDBWrapper.Core
{
    //Class Name Creator hyw.
    public class EDDBWrapper : IDisposable
    {
        private static AveLogger Logger = new AveLogger(typeof(EDDBWrapper));

        #region - Params -

        private ConnectionInfo _connectionInfo;

        private SqlConnection _sqlConnection;
        
        #endregion

        #region - 单实例字典 -

        private Dictionary<Type, object> _wrapperServices;

        #endregion

        #region - 私有构造 -

        private EDDBWrapper(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
            _wrapperServices = new Dictionary<Type, object>();
            SqlConnection conn = new SqlConnection(_connectionInfo.GetConnString());
            AveImpersonator impersonator = null;
            if (connectionInfo.SQLConnectionType == SQLConnectionType.WindowsAuthentication)
            {
                impersonator = new AveImpersonator(_connectionInfo.Domain, _connectionInfo.UserID, _connectionInfo.Password,!_connectionInfo.IsLocalMachine, true);
                impersonator.Impersonate();
            }
            conn.Open();
            _sqlConnection = conn;
            if(impersonator != null)
            {
                impersonator.Dispose();
            }
        }

        #endregion

        #region - 初始化Wrapper Factory -

        /// <summary>
        /// 每次初始化Factory,都只是用一个Connection Info.
        /// </summary>
        /// <param name="connectionInfo"></param>
        public static EDDBWrapper Initialization(ConnectionInfo connectionInfo)
        {
            EDDBWrapper factory = new EDDBWrapper(connectionInfo);
            return factory;
        }

        #endregion

        public T Use<T>() where T : AbstractService
        {
            object obj = null;
            if(!_wrapperServices.TryGetValue(typeof(T),out obj))
            {
                var constructorInfo = typeof(T).GetConstructor(new Type[] { typeof(SqlConnection), typeof(EDDBWrapper) });
                obj = (T)constructorInfo.Invoke(new object[] { _sqlConnection, this });
                _wrapperServices.Add(typeof(T), obj);
            }
            return (T)obj;
        }

        #region - Dispose -
        
        public void Dispose()
        {
            if(!_sqlConnection.IsNull())
            {
                _sqlConnection.Dispose();
//                _sqlConnection.Close();
                _sqlConnection = null;
            }
        }

        #endregion
    }
}
