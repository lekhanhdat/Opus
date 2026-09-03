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

namespace AvePoint.ObjectModel.Server16
{
    class AveQuerySession : IAveQuerySession
    {
        private static string mSqlSession_Type = "Microsoft.SharePoint.Utilities.SqlSession";
        private object mSqlSession;

        public AveQuerySession()
        {
            mSqlSession = AveAssemblyUtility.CreateInstance(mSqlSession_Type);
        }

        public AveQuerySession(object sqlSession)
        {
            mSqlSession = sqlSession;
        }

        internal object SqlSession
        {
            get
            {
                return mSqlSession;
            }
        }

        public SqlDataReader ExecuteReader(SqlCommand command, CommandBehavior behavior)
        {
            return (SqlDataReader)AveAssemblyUtility.InvokeMethod(mSqlSession, "ExecuteReader", new Type[] { command.GetType(), behavior.GetType() }, new object[] { command, behavior });
        }

        public SqlDataReader ExecuteReader(SqlCommand command)
        {
            return (SqlDataReader)AveAssemblyUtility.InvokeMethod(mSqlSession, "ExecuteReader", new Type[] { command.GetType() }, new object[] { command });
        }
    }
}
