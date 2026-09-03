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
using Microsoft.Office.Server.Search.Administration.TopologyExport;
using AvePoint.Wrapper.Common;
using System.Reflection;
using Microsoft.Office.Server.Search.Administration;
namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOTopologySettings : IAveOTopologySettings
    {
        private TopologySettings mTopologySettings;

        public AveOTopologySettings(TopologySettings topologySettings)
        {
            mTopologySettings = topologySettings;
        }
        public AveOTopologySettings(IAveOSearchServiceApplication searchApplication)
        {
            AveOSearchServiceApplication mSearchApplication = searchApplication as AveOSearchServiceApplication;
            var obj= AveAssemblyUtility.CreateInstance(typeof(TopologySettings), new Type[] { typeof(SearchServiceApplication) }, new object[] { mSearchApplication.SearchServiceApplication }) as TopologySettings;
            mTopologySettings = obj;
        }
        public AveOTopologySettings()
        {
            mTopologySettings = new TopologySettings();
        }

        #region IAveOTopologySettings members

        public IAveOAdminTopologySettings AdminTopology
        {
            get
            {
                if (mTopologySettings.AdminTopology != null)
                {
                    return new AveOAdminTopologySettings(mTopologySettings.AdminTopology);
                }
                return null;
            }
        }

        public IAveOCrawlTopologySettings CrawlTopology
        {
            get
            {
                if (mTopologySettings.AdminTopology != null)
                {
                    return new AveOCrawlTopologySettings(mTopologySettings.CrawlTopology);
                }
                return null;
            }
        }

        public IAveOQueryTopologySettings QueryTopology
        {
            get
            {
                if (mTopologySettings.AdminTopology != null)
                {
                    return new AveOQueryTopologySettings(mTopologySettings.QueryTopology);
                }
                return null;
            }
        }

        #endregion

        #region add for SP2013
        public IAveOAnalyticsReportingStoreCollectionSettings AnalyticsReportingStoreCollection
        {
            get
            {
                if (mTopologySettings.AnalyticsReportingStoreCollection != null)
                {
                    return new AveOAnalyticsReportingStoreCollectionSettings(mTopologySettings.AnalyticsReportingStoreCollection);
                }
                return null;
            }
        }
        #endregion
    }
}
