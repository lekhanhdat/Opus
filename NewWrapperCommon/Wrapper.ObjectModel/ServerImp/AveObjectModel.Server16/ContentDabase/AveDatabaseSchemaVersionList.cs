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
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;

namespace AvePoint.ObjectModel.Server16
{
    [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
    class AveDatabaseSchemaVersionList : IAveDatabaseSchemaVersionList,IDisposable
    {
        private const string mDatabaseSchemaVersionList_Type = "Microsoft.SharePoint.WebControls";
        private SPDatabaseSchemaVersionList mDatabaseSchemaVersionList;

        public AveDatabaseSchemaVersionList(SPDatabaseSchemaVersionList databaseSchemaVersionList)
        {
            mDatabaseSchemaVersionList = databaseSchemaVersionList;
        }

        public AveDatabaseSchemaVersionList()
        {
            mDatabaseSchemaVersionList = new SPDatabaseSchemaVersionList();
        }

        public string Status(IAveDatabase database, bool bNeedsUpgrade, bool bChildrenNeedsUpgrade, AveTriState tIsBackwardsCompatible)
        {
            Type[] types = new Type[] { typeof(SPDatabase), typeof(bool), typeof(bool), typeof(TriState) };
            object[] objs = new object[] { (database as AveDatabase).Database, bNeedsUpgrade, bChildrenNeedsUpgrade, (TriState)tIsBackwardsCompatible };
            return (string)AveAssemblyUtility.InvokeStaticMethod(mDatabaseSchemaVersionList_Type, "Status", types, objs);
        }

        public void Dispose()
        {
            if (mDatabaseSchemaVersionList != null)
            {
                mDatabaseSchemaVersionList.Dispose();
                mDatabaseSchemaVersionList = null;
            }
        }
    }
}
