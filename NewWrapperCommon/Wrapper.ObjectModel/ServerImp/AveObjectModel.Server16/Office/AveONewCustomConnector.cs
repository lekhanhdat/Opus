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

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveONewCustomConnector : IAveONewCustomConnector,IDisposable
    {
        private const string mNewCustomConnector_Type = "Microsoft.Office.Server.Search.Cmdlet.NewCustomConnector";
        private object mNewCustomConnector;
        private AveOSearchServiceApplication mSearchApp;

        public AveONewCustomConnector()
            :this(AveAssemblyUtility.CreateInstance(mNewCustomConnector_Type, new Type[] { }, new object[] { }))
        {
            
        }

        public AveONewCustomConnector(object newCustomConnector)
        {
            mNewCustomConnector = newCustomConnector;
        }

        #region IAveONewCustomConnector Members

        public string ModelFilePath
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mNewCustomConnector, "ModelFilePath");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mNewCustomConnector, "ModelFilePath", value);
            }
        }

        public string Name
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mNewCustomConnector, "Name");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mNewCustomConnector, "Name", value);
            }
        }

        public string Protocol
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(mNewCustomConnector, "Protocol");
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mNewCustomConnector, "Protocol", value);
            }
        }

        public IAveOSearchServiceApplication SearchApp
        {
            get
            {
                if (mSearchApp == null)
                {
                    SearchServiceApplication searchApp = (SearchServiceApplication)AveAssemblyUtility.GetFieldValue(mNewCustomConnector, "searchApp");
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
                    AveAssemblyUtility.SetFieldValue(mNewCustomConnector, "searchApp", mSearchApp.SearchServiceApplication);
                }
                else
                {
                    AveAssemblyUtility.SetFieldValue(mNewCustomConnector, "searchApp", null);
                }
            }
        }

        public IAveOCustomConnector CreateDataObject()
        {
            return new AveOCustomConnector((CustomConnector)AveAssemblyUtility.InvokeMethod(mNewCustomConnector, "CreateDataObject", new Type[] { }, new object[] { }));
        }

        #endregion

        public void Dispose()
        {
            if (mSearchApp != null)
                mSearchApp.Dispose();
        }
    }
}
