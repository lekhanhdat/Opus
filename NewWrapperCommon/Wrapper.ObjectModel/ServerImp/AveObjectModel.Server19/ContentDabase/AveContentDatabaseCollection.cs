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
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveContentDatabaseCollection : AveAbstractCommonCollection<IAveContentDatabase>, IAveContentDatabaseCollection
    {
        private SPContentDatabaseCollection mContentDBColl;

        public AveContentDatabaseCollection(SPContentDatabaseCollection dbColl)
            : base(dbColl)
        {
            mContentDBColl = dbColl;
        }

        public AveContentDatabaseCollection(IAveVirtualServer virtualServer)
            : this(new SPContentDatabaseCollection((virtualServer as AveVirtualServer).VirtualServer))
        { }

        #region IAveContentDatabaseCollection Members

        public IAveContentDatabase this[Guid id]
        {
            get
            {
                SPContentDatabase contentDatabase = mContentDBColl[id];
                if (contentDatabase == null)
                {
                    return null;
                }
                return new AveContentDatabase(contentDatabase);
            }
        }

        public IAveContentDatabase Add(string strDatabaseServer, string strDatabaseName, string strDatabaseUsername, string strDatabasePassword, int warningSiteCount, int maximumSiteCount, int status, bool provision, Guid lockId, int addFlags)
        {
            SPContentDatabase db = (SPContentDatabase)AveAssemblyUtility.InvokeMethod(mContentDBColl, "Add",
                    new Type[] { typeof(String), typeof(String), typeof(String), typeof(String), typeof(Int32), typeof(Int32), typeof(Int32), typeof(Boolean), typeof(Guid), typeof(Int32) },
                    new object[] { 
                     strDatabaseServer,
                     strDatabaseName,
                     strDatabaseUsername, 
                     strDatabasePassword, 
                     warningSiteCount,
                     maximumSiteCount,
                     status, 
                     provision, 
                     lockId, 
                     addFlags });
            return new AveContentDatabase(db);
        }

        #endregion

        public override IAveContentDatabase this[int index]
        {
            get
            {
                SPContentDatabase contentDatabase = mContentDBColl[index];
                if (contentDatabase == null)
                {
                    return null;
                }
                return new AveContentDatabase(contentDatabase);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveContentDatabase(t as SPContentDatabase);
        }

        public override int Count
        {
            get { return mContentDBColl.Count; }
        }

        public void Delete(Guid gDatabaseId)
        {
            mContentDBColl.Delete(gDatabaseId);
        }
    }
}
