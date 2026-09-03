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
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveQueryProvider : IAveQueryProvider
    {
        private IDisposable mQueryProvider;
        private const string mQueryProvider_Type = "Microsoft.SharePoint.Help.SPQueryProvider";

        internal IDisposable QueryProvider
        {
            get
            {
                return mQueryProvider;
            }
        }

        public AveQueryProvider(IDisposable queryProvider)
        {
            mQueryProvider = queryProvider;
        }

        public AveQueryProvider(Uri helpList, uint lcid)
        {
            mQueryProvider = (IDisposable)AveAssemblyUtility.CreateInstance(mQueryProvider_Type, new Type[] { typeof(Uri), typeof(uint) }, new object[] { helpList, lcid });
        }

        #region IDisposable Members

        public void Dispose()
        {
            mQueryProvider.Dispose();
        }

        #endregion

        public DataTable GetHelpCollectionsForAllProducts(string[] queryViewFields, out string[] optOutCollections)
        {
            string[] _optOutCollections = new string[] { };
            DataTable dataTable = AveAssemblyUtility.InvokeMethod(mQueryProvider, "GetHelpCollectionsForAllProducts", new Type[] { queryViewFields.GetType(), _optOutCollections.GetType().MakeByRefType() }, new object[] { queryViewFields, _optOutCollections }) as DataTable;
            optOutCollections = _optOutCollections;
            return dataTable;
        }
    }
}
