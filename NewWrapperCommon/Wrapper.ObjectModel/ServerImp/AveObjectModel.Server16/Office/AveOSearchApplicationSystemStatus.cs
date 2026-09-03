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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.WebControls;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOSearchApplicationSystemStatus : AveOSystemStatusBase, IAveOSearchApplicationSystemStatus
    {
        private SearchApplicationSystemStatus mSearchApplicationSystemStatus;

        public AveOSearchApplicationSystemStatus(SearchApplicationSystemStatus searchApplicationSystemStatus)
            : base(searchApplicationSystemStatus)
        {
            mSearchApplicationSystemStatus = searchApplicationSystemStatus;
        }

        public AveOSearchApplicationSystemStatus()
            : this(new SearchApplicationSystemStatus())
        { }

        #region IAveOSearchApplicationSystemStatus members

        public string GetStatusString(IAveOSearchServiceApplication searchApp)
        {
            AveOSearchServiceApplication aveOSearchServiceApplication = searchApp as AveOSearchServiceApplication;
            SearchServiceApplication searchServiceApplication = null;
            if (aveOSearchServiceApplication != null)
            {
                searchServiceApplication = aveOSearchServiceApplication.SearchServiceApplication;
            }
            return AveAssemblyUtility.InvokeMethod(mSearchApplicationSystemStatus, "GetStatusString", new Type[] { typeof(SearchServiceApplication) }, new object[] { searchServiceApplication }).ToString();
        }

        public string GetBackgroundStatusString(IAveOSearchServiceApplication searchApp, IAveOContent content)
        {
            AveOSearchServiceApplication aveOSearchServiceApplication = searchApp as AveOSearchServiceApplication;
            AveOContent aveOContent = content as AveOContent;
            SearchServiceApplication tmpSearchServiceApplication = null;
            Content tmpContent = null;
            if (aveOSearchServiceApplication != null)
            {
                tmpSearchServiceApplication = aveOSearchServiceApplication.SearchServiceApplication;
            }
            if (aveOContent != null)
            {
                tmpContent = aveOContent.Content;
            }
            return AveAssemblyUtility.InvokeMethod(mSearchApplicationSystemStatus, "GetBackgroundStatusString", new Type[] { typeof(SearchServiceApplication), typeof(Content) }, new object[] { tmpSearchServiceApplication, tmpContent }).ToString();
        }

        #endregion
    }
}
