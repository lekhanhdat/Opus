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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using System.Data;
using Microsoft.Office.Server.Search.Extended.Administration.Common;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOUserContextHelper : AveOAdminOMHelperBase, IAveOUserContextHelper
    {
        private readonly string mUserContextHelper_Type = "Microsoft.Office.Server.Search.Extended.Administration.Facade.UserContextHelper";
        private object mUserContextHelper;
        private DataTable mUserContextListTable;
        private DataTable mUserPropertyTable;

        public AveOUserContextHelper(string siteID)
            : base(siteID)
        {
            mUserContextHelper = AveAssemblyUtility.CreateInstance(mUserContextHelper_Type, new Type[] { typeof(string) }, new object[] { siteID });
        }

        public int TotalRecords
        {
            get
            {
                return (int)AveAssemblyUtility.GetPropertyValue(mUserContextHelper, "TotalRecords");
            }
        }

        public DataTable UserContextListTable
        {
            get
            {
                if (mUserContextListTable == null)
                {
                    object userContextListTable = AveAssemblyUtility.GetPropertyValue(mUserContextHelper, "UserContextListTable");
                    if (userContextListTable != null)
                    {
                        mUserContextListTable = (DataTable)userContextListTable;
                    }
                }
                return mUserContextListTable;
            }
        }

        public DataTable UserPropertyTable
        {
            get
            {
                if (mUserPropertyTable == null)
                {
                    object userPropertyTable = AveAssemblyUtility.GetPropertyValue(mUserContextHelper, "UserPropertyTable");
                    if (userPropertyTable != null)
                    {
                        mUserPropertyTable = (DataTable)userPropertyTable;
                    }
                }
                return mUserPropertyTable;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mUserContextHelper, "UserPropertyTable", value);
                mUserPropertyTable = value;
            }
        }

        public bool DeleteData(string userContextName)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mUserContextHelper, "DeleteData", new Type[] { typeof(string) }, new object[] { userContextName });
        }

        public DataTable GetContextList(string searchValue, int offset, int pageSize, string sortExpression, AveSortDirection sortDirection)
        {
            object retObj = AveAssemblyUtility.InvokeMethod(mUserContextHelper, "GetContextList", new object[] { searchValue, offset, pageSize, sortExpression, (int)sortDirection });
            if (retObj != null)
            {
                return (DataTable)retObj;
            }
            return null;
        }

        public DataTable GetContextNames(string searchValue, int offset, int pageSize, string sortExpression)
        {
            object retObj = AveAssemblyUtility.InvokeMethod(mUserContextHelper, "GetContextNames", new Type[] { typeof(string), typeof(int), typeof(int), typeof(string) }, new object[] { searchValue, offset, pageSize, sortExpression });
            if (retObj != null)
            {
                return (DataTable)retObj;
            }
            return null;
        }

        public Dictionary<string, string> GetUserContext(string ctxName)
        {
            object retObj = AveAssemblyUtility.InvokeMethod(mUserContextHelper, "GetUserContext", new Type[] { typeof(string) }, new object[] { ctxName });
            if (retObj != null)
            {
                return (Dictionary<string, string>)retObj;
            }
            return null;
        }

        public bool SaveUserContext(AveMode mode, Dictionary<string, string> userContextDict)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mUserContextHelper, "SaveUserContext", new Type[] { typeof(Mode), typeof(Dictionary<string, string>) }, new object[] { (Mode)mode, userContextDict });
        }
    }
}
