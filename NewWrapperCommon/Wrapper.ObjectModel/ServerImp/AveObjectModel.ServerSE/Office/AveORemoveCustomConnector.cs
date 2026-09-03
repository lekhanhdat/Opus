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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.Search.Administration;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveORemoveCustomConnector : IAveORemoveCustomConnector,IDisposable
    {
        private object mRemoveCustomConnector;
        private const string mRemoveCustomConnector_Type = "Microsoft.Office.Server.Search.Cmdlet.RemoveCustomConnector";
        private AveOSearchServiceApplication mSearchApp;

        public AveORemoveCustomConnector()
            : this(AveAssemblyUtility.CreateInstance(mRemoveCustomConnector_Type, new Type[] { }, new object[] { }))
        { }

        public AveORemoveCustomConnector(object removeCustomConnector)
        {
            mRemoveCustomConnector = removeCustomConnector;
        }

        #region IAveORemoveCustomConnector Members

        public void DeleteDataObject()
        {
            AveAssemblyUtility.InvokeMethod(mRemoveCustomConnector, "DeleteDataObject", new Type[] { }, new object[] { });
        }

        public string ProtocolName
        {
            get
            {
                return (string)AveAssemblyUtility.GetFieldValue(mRemoveCustomConnector, "m_ProtocolName");
            }
            set
            {
                AveAssemblyUtility.SetFieldValue(mRemoveCustomConnector, "m_ProtocolName", value);
            }
        }

        public IAveOSearchServiceApplication SearchApp
        {
            get
            {
                if (mSearchApp == null)
                {
                    SearchServiceApplication searchApp = (SearchServiceApplication)AveAssemblyUtility.GetFieldValue(mRemoveCustomConnector, "m_SearchApp");
                    if (searchApp != null)
                    {
                        mSearchApp = new AveOSearchServiceApplication(searchApp);
                    }
                }
                return mSearchApp;
            }
            set
            {
                mSearchApp = value as AveOSearchServiceApplication;
                if (mSearchApp != null)
                {
                    AveAssemblyUtility.SetFieldValue(mRemoveCustomConnector, "m_SearchApp", mSearchApp.SearchServiceApplication);
                }
                else
                {
                    AveAssemblyUtility.SetFieldValue(mRemoveCustomConnector, "m_SearchApp", null);
                }
            }
        }

        #endregion

        public void Dispose()
        {
            if (mSearchApp != null)
                mSearchApp.Dispose();
        }
    }
}
